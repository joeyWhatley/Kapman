using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// THE CHART'S LAND (ported 4 Oct) - the whole world's baked land, read straight from the first build's world bake
    /// (G:/kapman_unity/data/baked/world, written by Bake/kapman_bake/tiles.py):
    ///   world.json                   the index: the grid, the levels, the surface types
    ///   L{level}/{row}/{col}.kt      one tile: 256 x 256 cells, float16 heights and uint8 surfaces (zlib)
    /// The 2D chart (MapRenderer) is drawn from this, as it was in the first build. The 3D side no longer streams this
    /// bake (the rebuild's levels are Unity terrains), so the chart keeps its own reader: the first build's TileStoreLand
    /// with only what the map needs. Tiles are read on demand and kept in a cache of recently used ones; safe from
    /// worker threads. The fine levels (30 and 60 m) only run along the water: where one has no tile, reads blend the
    /// next coarser level instead. Beyond the map, reads give deep sea.
    /// </summary>
    public sealed class MapLand : IDisposable
    {
        public const int Cells = 256;
        const float FillHeight = -3000f;
        const byte FillSurface = 1;

        /// <summary>One level of the bake: its cell size (real metres) and its grid, from the shared north-west corner.</summary>
        public sealed class Level
        {
            public int level;
            public double cell, west, north;
            public int width, height;
        }

        /// <summary>The world rectangle the bake covers (real metres).</summary>
        [Serializable]
        public class BoundsInfo
        {
            public double west, south, east, north;
        }

        [Serializable] class LevelJson { public int level; public double cell; public bool complete; }
        [Serializable] class SurfaceJson { public int id; public string name; public string color; }
        [Serializable]
        class IndexJson
        {
            public string name, about;
            public int tile_cells;
            public BoundsInfo bounds;
            public LevelJson[] levels;
            public SurfaceJson[] surfaces;
        }

        sealed class Tile
        {
            public float[] heights;
            public byte[] surfaces;
            public LinkedListNode<long> node;
        }

        /// <summary>The default place of the world bake (the first build's; see Bake/world/README.md).</summary>
        public const string DefaultFolder = "G:/kapman_unity/data/baked/world";

        public string Name { get; }
        public string About { get; }
        public BoundsInfo Bounds { get; }
        public Level[] Levels { get; }
        public int LevelCount => Levels.Length;
        /// <summary>The finest level that covers everywhere (finer levels only run along the water).</summary>
        public int FinestLevel { get; }
        public string[] SurfaceNames { get; } = new string[256];

        readonly string root;
        readonly int capacity;
        readonly Dictionary<long, Tile> cache = new Dictionary<long, Tile>();
        readonly HashSet<long> missing = new HashSet<long>();
        readonly LinkedList<long> recent = new LinkedList<long>();
        readonly object gate = new object();

        static MapLand shared;
        static string sharedFolder;
        static bool warned;

        MapLand(string folder, IndexJson index, int cacheTiles)
        {
            root = folder;
            capacity = cacheTiles;
            Name = index.name;
            About = index.about;
            Bounds = index.bounds;
            Levels = new Level[index.levels.Length];
            int finest = index.levels.Length - 1;
            foreach (var lv in index.levels)
            {
                Levels[lv.level] = new Level
                {
                    level = lv.level, cell = lv.cell, west = Bounds.west, north = Bounds.north,
                    width = (int)Math.Round((Bounds.east - Bounds.west) / lv.cell),
                    height = (int)Math.Round((Bounds.north - Bounds.south) / lv.cell),
                };
                if (lv.complete)
                    finest = Math.Min(finest, lv.level);
            }
            FinestLevel = finest;
            foreach (var s in index.surfaces ?? new SurfaceJson[0])
                if (s.id >= 0 && s.id < 256)
                    SurfaceNames[s.id] = s.name;
        }

        /// <summary>Opens the world bake in a folder (it holds world.json). Throws if it isn't there.</summary>
        public static MapLand Open(string folder, int cacheTiles = 1500)
        {
            string path = Path.Combine(folder, "world.json");
            if (!File.Exists(path))
                throw new FileNotFoundException("No world bake here - run the world bake (Bake/world/README.md)", path);
            return new MapLand(folder, JsonUtility.FromJson<IndexJson>(File.ReadAllText(path)), cacheTiles);
        }

        /// <summary>The one world bake the game shares (the map, the fog of war's bounds): opened on first use, or null
        /// (with a warning, once) when the bake isn't on this machine.</summary>
        public static MapLand Shared(string folder = null)
        {
            folder = string.IsNullOrEmpty(folder) ? DefaultFolder : folder;
            if (shared != null && sharedFolder == folder)
                return shared;
            try
            {
                shared = Open(folder);
                sharedFolder = folder;
                warned = false;
            }
            catch (Exception e)
            {
                if (!warned)
                    Debug.LogWarning("Kapman map: the chart can't be drawn, there is no world bake at " + folder + " (" + e.Message + ")");
                warned = true;
                shared = null;
            }
            return shared;
        }

        static long Key(int level, int col, int row) => ((long)level << 48) | ((long)(uint)row << 24) | (uint)col;

        /// <summary>A tile (cached), or null if the bake has none there.</summary>
        Tile Get(int level, int col, int row)
        {
            long key = Key(level, col, row);
            lock (gate)
            {
                if (cache.TryGetValue(key, out var hit))
                {
                    recent.Remove(hit.node);
                    recent.AddFirst(hit.node);
                    return hit;
                }
                if (missing.Contains(key))
                    return null;
            }
            var tile = Load(level, col, row);
            lock (gate)
            {
                if (tile == null)
                {
                    missing.Add(key);
                    return null;
                }
                if (cache.TryGetValue(key, out var other))              // another thread got there first
                    return other;
                tile.node = recent.AddFirst(key);
                cache[key] = tile;
                while (cache.Count > capacity)
                {
                    long old = recent.Last.Value;
                    recent.RemoveLast();
                    cache.Remove(old);
                }
                return tile;
            }
        }

        Tile Load(int level, int col, int row)
        {
            if (col < 0 || row < 0)
                return null;
            string path = Path.Combine(root, "L" + level, row.ToString(), col + ".kt");
            if (!File.Exists(path))
                return null;
            using var f = new BinaryReader(File.OpenRead(path));
            var magic = f.ReadBytes(4);
            if (magic[0] != 'K' || magic[1] != 'P' || magic[2] != 'T' || magic[3] != '1')
                throw new InvalidDataException("Not a Kapman tile: " + path);
            f.ReadInt32(); f.ReadInt32(); f.ReadInt32();                   // level, col, row
            int cells = f.ReadInt32();
            f.ReadSingle(); f.ReadSingle();                                // min, max
            var half = Inflate(f.ReadBytes(f.ReadInt32()), cells * cells * 2);
            var surf = Inflate(f.ReadBytes(f.ReadInt32()), cells * cells);
            var heights = new float[cells * cells];
            for (int i = 0; i < heights.Length; i++)
                heights[i] = HalfToFloat((ushort)(half[i * 2] | (half[i * 2 + 1] << 8)));
            return new Tile { heights = heights, surfaces = surf };
        }

        /// <summary>A float16 to a float (the first build used Unity.Mathematics for this; the game assembly doesn't reference it).</summary>
        static float HalfToFloat(ushort h)
        {
            int sign = (h >> 15) & 1, exp = (h >> 10) & 0x1F, mant = h & 0x3FF;
            float v;
            if (exp == 0)
                v = mant * (1f / 16777216f);                               // subnormal: mant * 2^-24
            else if (exp == 31)
                v = mant == 0 ? float.PositiveInfinity : float.NaN;
            else
                v = (1f + mant / 1024f) * Pow2(exp - 15);
            return sign == 1 ? -v : v;
        }

        static float Pow2(int e) => (float)Math.Pow(2.0, e);

        /// <summary>zlib data (a 2-byte header, deflate, a checksum) to bytes.</summary>
        static byte[] Inflate(byte[] zlib, int size)
        {
            var result = new byte[size];
            using var input = new MemoryStream(zlib, 2, zlib.Length - 2);
            using var deflate = new DeflateStream(input, CompressionMode.Decompress);
            int read = 0;
            while (read < size)
            {
                int n = deflate.Read(result, read, size - read);
                if (n == 0)
                    break;
                read += n;
            }
            return result;
        }

        /// <summary>Heights (metres) of a window of cells at a level, rows north to south.</summary>
        public void ReadHeights(int level, int col0, int row0, int cols, int rows, float[] dest)
        {
            for (int r = 0; r < rows; r++)
            {
                int row = row0 + r, tr = FloorDiv(row, Cells), ir = row - tr * Cells;
                int c = 0;
                while (c < cols)
                {
                    int col = col0 + c, tc = FloorDiv(col, Cells), ic = col - tc * Cells;
                    int run = Math.Min(cols - c, Cells - ic);
                    var tile = Get(level, tc, tr);
                    int d = r * cols + c;
                    if (tile != null)
                        Array.Copy(tile.heights, ir * Cells + ic, dest, d, run);
                    else if (level < FinestLevel)
                        for (int k = 0; k < run; k++) dest[d + k] = CoarserHeight(level, col + k, row);
                    else
                        for (int k = 0; k < run; k++) dest[d + k] = FillHeight;
                    c += run;
                }
            }
        }

        /// <summary>One cell's height at a level: from its tile or, where a fine level has none, from the coarser levels.</summary>
        float CellHeight(int level, int col, int row)
        {
            int tc = FloorDiv(col, Cells), tr = FloorDiv(row, Cells);
            var tile = Get(level, tc, tr);
            if (tile != null)
                return tile.heights[(row - tr * Cells) * Cells + (col - tc * Cells)];
            return level < FinestLevel ? CoarserHeight(level, col, row) : FillHeight;
        }

        /// <summary>A cell of a fine level where it has no tile: blended (bilinear) from the next coarser level.</summary>
        float CoarserHeight(int level, int col, int row)
        {
            double fc = col * 0.5 - 0.25, fr = row * 0.5 - 0.25;             // the cell's centre, in coarser cells
            int c = (int)Math.Floor(fc), r = (int)Math.Floor(fr);
            float tx = (float)(fc - c), ty = (float)(fr - r);
            float a = CellHeight(level + 1, c, r), b = CellHeight(level + 1, c + 1, r);
            float d = CellHeight(level + 1, c, r + 1), e = CellHeight(level + 1, c + 1, r + 1);
            float north = a + (b - a) * tx, south = d + (e - d) * tx;
            return north + (south - north) * ty;
        }

        /// <summary>One cell's surface at a level: from its tile or, where a fine level has none, the coarser cell over it.</summary>
        byte CellSurface(int level, int col, int row)
        {
            int tc = FloorDiv(col, Cells), tr = FloorDiv(row, Cells);
            var tile = Get(level, tc, tr);
            if (tile != null)
                return tile.surfaces[(row - tr * Cells) * Cells + (col - tc * Cells)];
            return level < FinestLevel ? CellSurface(level + 1, FloorDiv(col, 2), FloorDiv(row, 2)) : FillSurface;
        }

        /// <summary>Surface types of a window of cells at a level, rows north to south.</summary>
        public void ReadSurfaces(int level, int col0, int row0, int cols, int rows, byte[] dest)
        {
            for (int r = 0; r < rows; r++)
            {
                int row = row0 + r, tr = FloorDiv(row, Cells), ir = row - tr * Cells;
                int c = 0;
                while (c < cols)
                {
                    int col = col0 + c, tc = FloorDiv(col, Cells), ic = col - tc * Cells;
                    int run = Math.Min(cols - c, Cells - ic);
                    var tile = Get(level, tc, tr);
                    int d = r * cols + c;
                    if (tile != null)
                        Array.Copy(tile.surfaces, ir * Cells + ic, dest, d, run);
                    else if (level < FinestLevel)
                        for (int k = 0; k < run; k++) dest[d + k] = CellSurface(level + 1, FloorDiv(col + k, 2), FloorDiv(row, 2));
                    else
                        for (int k = 0; k < run; k++) dest[d + k] = FillSurface;
                    c += run;
                }
            }
        }

        /// <summary>The height at a place (real metres), from the finest level there is there (30 m along the water), bilinear.</summary>
        public float HeightAt(double x, double z)
        {
            var lv = Levels[0];
            double fc = (x - lv.west) / lv.cell - 0.5, fr = (lv.north - z) / lv.cell - 0.5;
            int c = (int)Math.Floor(fc), r = (int)Math.Floor(fr);
            float tx = (float)(fc - c), ty = (float)(fr - r);
            float a = CellHeight(0, c, r), b = CellHeight(0, c + 1, r);
            float d = CellHeight(0, c, r + 1), e = CellHeight(0, c + 1, r + 1);
            float north = a + (b - a) * tx, south = d + (e - d) * tx;
            return north + (south - north) * ty;
        }

        /// <summary>The surface type at a place (real metres).</summary>
        public byte SurfaceAt(double x, double z)
        {
            var lv = Levels[0];
            return CellSurface(0, (int)Math.Floor((x - lv.west) / lv.cell), (int)Math.Floor((lv.north - z) / lv.cell));
        }

        static int FloorDiv(int a, int b) => a >= 0 ? a / b : -((-a + b - 1) / b);

        public void Dispose()
        {
            lock (gate)
            {
                cache.Clear();
                recent.Clear();
                missing.Clear();
            }
        }

        // Enter Play Mode runs without a domain reload: the shared bake is opened afresh each play
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            shared?.Dispose();
            shared = null;
            sharedFolder = null;
            warned = false;
        }
    }
}
