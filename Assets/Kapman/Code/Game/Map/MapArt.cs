using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// THE CHART'S PICTURE (the user, 4 Oct 2026: "a placeholder 2D map ... something that can be replaced with my own hand
    /// drawn one") - plain PNG tiles on disk that the chart shows under its marks, in place of the tiles it draws itself
    /// (MapRenderer). Bake/world/make_map_art.py writes the placeholder (pixel-flat, the Byzantium palette, the land of
    /// c. 800); redraw any tile at the same size and the chart shows yours.
    ///   map_art.json      the frame: west and north (real metres, the world frame), metres a pixel, the tile's pixels,
    ///                     how many tiles across and down, the file names
    ///   t_{row}_{col}.png one tile; row 0 the northernmost, col 0 the westernmost
    ///   overview.png      the whole window small, shown from far out and under tiles still loading
    /// THE SAME PICTURE AT FINER SIZES (the user, the same night: "Why's the map so small I want you to make it absolutely
    /// collosal full sized full detail full resolution"): if pyramid.json lies beside map_art.json
    /// (Bake/world/make_map_pyramid.py), the chart has more levels to zoom through, each a folder of tiles of the same
    /// 4096 pixels over the same window from the same north-west corner:
    ///   level 0  120 m a pixel   t_{row}_{col}.png        6 x 6 tiles
    ///   level 1   60 m           L1/t_{row}_{col}.png    12 x 12
    ///   level 2   30 m           L2/t_{row}_{col}.png    24 x 24   (98,304 pixels a side)
    ///   level 3   10 m           L3/t_{row}_{col}.png    72 x 72, but only round the ports
    /// A finer level lists the tiles it has; where it has none (open sea, or no port near) the chart shows the next
    /// coarser one. Without pyramid.json there is level 0 alone, as before.
    /// Looked for in StreamingAssets/MapArt/world (a built game), then in the project's MapArt/world beside Assets (the
    /// editor: nothing to import, drop the files in). Tiles are read when the view wants them and the least lately used
    /// are let go (a 4096-pixel tile is 64 MB on the card).
    /// THE READING IS DONE OFF THE MAIN THREAD (7 Oct 2026, night: the user, "the map not updating on the fly fast
    /// enough"): a tile was read, unpacked and recoloured by the engine's own calls in one frame, the best part of a
    /// second with the game stood still, a tile a frame. Now two threads of their own do that (MapPng) for the tiles
    /// the view asked for last, the newest asking first; the main thread only puts one finished tile a frame on the
    /// card. What the view no longer asks for is dropped from the waiting list each frame.
    /// </summary>
    public sealed class MapArt
    {
        /// <summary>One size of pixel of the picture.</summary>
        public sealed class Level
        {
            public int index;
            public double metresPerPixel;
            public int cols, rows;
            public string pattern;
            /// <summary>The tiles that have a file (row * cols + col); null when every tile may have one (level 0).</summary>
            public HashSet<int> have;

            public bool Has(int row, int col) =>
                row >= 0 && col >= 0 && row < rows && col < cols && (have == null || have.Contains(row * cols + col));
        }

        public double West { get; private set; }
        public double North { get; private set; }
        public double MetresPerPixel { get; private set; }
        public int TilePixels { get; private set; }
        public int Cols { get; private set; }
        public int Rows { get; private set; }
        public double TileMetres => TilePixels * MetresPerPixel;
        public double East => West + Cols * TileMetres;
        public double South => North - Rows * TileMetres;
        public Texture2D Overview { get; private set; }
        public string Folder { get; private set; }

        /// <summary>How many tiles are kept at once.</summary>
        public int capacity = 10;

        readonly List<Level> levels = new List<Level>();
        readonly Dictionary<int, (Texture2D tex, int used)> tiles = new Dictionary<int, (Texture2D, int)>();
        readonly HashSet<int> missing = new HashSet<int>();
        readonly List<int> wanted = new List<int>();
        int frame;

        // the reading, off the main thread
        sealed class Done
        {
            public int key, generation, width, height;
            public Color32[] pixels;                                  // null: not read (no file, or a kind of PNG MapPng declines)
            public bool declined;                                     // there is a file, and it must be read the old way
            public string path;
        }

        /// <summary>How many tiles are read at once (each holds 64 MB of pixels while it is read).</summary>
        public const int Readers = 2;
        readonly HashSet<int> reading = new HashSet<int>();
        readonly ConcurrentQueue<Done> done = new ConcurrentQueue<Done>();
        readonly ConcurrentBag<Color32[]> spare = new ConcurrentBag<Color32[]>();   // (the big arrays are used again, not thrown to the collector)
        int generation;
        /// <summary>Tiles being read now (for a test, a readout).</summary>
        public int Reading => reading.Count;
        /// <summary>A test may ask for the old way: every tile read whole on the main thread when asked for.</summary>
        public static bool ReadOnMainThread;
        string overviewFile = "overview.png";

        static MapArt shared;
        static bool looked;

        /// <summary>How many sizes of pixel the picture has (1 without a pyramid).</summary>
        public int LevelCount => levels.Count;

        /// <summary>A level, 0 the coarsest.</summary>
        public Level LevelInfo(int level) => levels[Mathf.Clamp(level, 0, levels.Count - 1)];

        /// <summary>The width of one tile of a level (real metres).</summary>
        public double TileMetresAt(int level) => TilePixels * LevelInfo(level).metresPerPixel;

        /// <summary>The level to show at a zoom (real metres a screen pixel): the coarsest whose own pixel is no bigger
        /// than a screen pixel, and the finest there is when the view is closer than that.</summary>
        public int LevelFor(double metresPerScreenPixel)
        {
            for (int i = 0; i < levels.Count; i++)
                if (levels[i].metresPerPixel <= metresPerScreenPixel * 1.0001)
                    return i;
            return levels.Count - 1;
        }

        /// <summary>The chart's picture, if there is one (looked for once).</summary>
        public static MapArt Shared()
        {
            if (looked)
                return shared;
            looked = true;
            foreach (var folder in new[]
                     {
                         Path.Combine(Application.streamingAssetsPath, "MapArt", "world"),
                         Path.Combine(Application.dataPath, "..", "MapArt", "world"),
                     })
            {
                shared = Open(folder);
                if (shared != null)
                    break;
            }
            return shared;
        }

        /// <summary>Opens a folder of map art (null if it has no map_art.json).</summary>
        public static MapArt Open(string folder)
        {
            string index = Path.Combine(folder, "map_art.json");
            if (!File.Exists(index))
                return null;
            var art = new MapArt { Folder = folder };
            string pattern = "t_{row}_{col}.png";
            try
            {
                var o = Json.Parse(File.ReadAllText(index));
                art.West = Json.Num(o, "west");
                art.North = Json.Num(o, "north");
                art.MetresPerPixel = Json.Num(o, "metresPerPixel", 120.0);
                art.TilePixels = (int)Json.Num(o, "tilePixels", 4096.0);
                art.Cols = (int)Json.Num(o, "cols", 1.0);
                art.Rows = (int)Json.Num(o, "rows", 1.0);
                pattern = Json.Str(o, "tiles", pattern);
                art.overviewFile = Json.Str(o, "overview", "overview.png");
                string over = Path.Combine(folder, art.overviewFile);
                if (File.Exists(over))
                    art.Overview = Read(over);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Kapman map art: " + index + " could not be read (" + e.Message + "); the chart draws its own tiles.");
                return null;
            }
            if (art.Cols < 1 || art.Rows < 1 || art.TilePixels < 1 || art.MetresPerPixel <= 0.0)
                return null;
            art.levels.Add(new Level { index = 0, metresPerPixel = art.MetresPerPixel, cols = art.Cols, rows = art.Rows, pattern = pattern });
            art.ReadPyramid(Path.Combine(folder, "pyramid.json"));
            return art;
        }

        /// <summary>The finer levels, if make_map_pyramid.py has baked them: each must sit on the same corner with the same
        /// tile size and be finer than the last. A fault here only costs the finer levels.</summary>
        void ReadPyramid(string path)
        {
            if (!File.Exists(path))
                return;
            try
            {
                var o = Json.Parse(File.ReadAllText(path));
                if (Math.Abs(Json.Num(o, "west", double.NaN) - West) > 1.0 || Math.Abs(Json.Num(o, "north", double.NaN) - North) > 1.0 ||
                    (int)Json.Num(o, "tilePixels") != TilePixels)
                {
                    Debug.LogWarning("Kapman map art: " + path + " is of another window than map_art.json; the finer levels are left out.");
                    return;
                }
                var list = Json.Arr(o, "levels");
                if (list == null)
                    return;
                foreach (var item in list)
                {
                    int n = (int)Json.Num(item, "level", -1.0);
                    if (n != levels.Count)                                  // (level 0 is map_art.json's; the rest in order)
                        continue;
                    var lv = new Level
                    {
                        index = n,
                        metresPerPixel = Json.Num(item, "metresPerPixel"),
                        cols = (int)Json.Num(item, "cols"),
                        rows = (int)Json.Num(item, "rows"),
                        pattern = Json.Str(item, "tiles", "L" + n + "/t_{row}_{col}.png"),
                        have = new HashSet<int>(),
                    };
                    if (lv.metresPerPixel <= 0.0 || lv.metresPerPixel >= levels[levels.Count - 1].metresPerPixel || lv.cols < 1 || lv.rows < 1)
                        break;
                    var have = Json.Arr(item, "have");
                    if (have != null)
                        foreach (var rc in have)
                            if (rc is List<object> pair && pair.Count == 2 && pair[0] is double r && pair[1] is double c)
                                lv.have.Add((int)r * lv.cols + (int)c);
                    if (lv.have.Count == 0)
                        break;
                    levels.Add(lv);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Kapman map art: " + path + " could not be read (" + e.Message + "); the chart shows the 120 m picture alone.");
            }
        }

        static Texture2D Read(string path)
        {
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = Path.GetFileName(path) };
            // (kept readable only while a colour scheme has to be laid on it: MapPalette.Apply lets it go)
            if (!tex.LoadImage(File.ReadAllBytes(path), MapPalette.Scheme == 0))
            {
                UnityEngine.Object.Destroy(tex);
                return null;
            }
            MapPalette.Apply(tex, MapPalette.Scheme);
            return tex;
        }

        /// <summary>The tile a world point lies in (false outside the picture).</summary>
        public bool TileAt(double x, double z, out int row, out int col) => TileAt(0, x, z, out row, out col);

        /// <summary>The tile of a level a world point lies in (false outside the picture).</summary>
        public bool TileAt(int level, double x, double z, out int row, out int col)
        {
            var lv = LevelInfo(level);
            double size = TilePixels * lv.metresPerPixel;
            col = (int)Math.Floor((x - West) / size);
            row = (int)Math.Floor((North - z) / size);
            return col >= 0 && row >= 0 && col < lv.cols && row < lv.rows;
        }

        /// <summary>A tile's west and north edges (real metres).</summary>
        public (double west, double north) Corner(int row, int col) => Corner(0, row, col);

        /// <summary>The west and north edges of a tile of a level (real metres).</summary>
        public (double west, double north) Corner(int level, int row, int col)
        {
            double size = TileMetresAt(level);
            return (West + col * size, North - row * size);
        }

        /// <summary>One number for a tile of a level (levels have at most 72 x 72 tiles).</summary>
        public static int Key(int level, int row, int col) => (level << 20) | (row << 10) | col;

        /// <summary>True when a level has a file for this tile (a finer level has none over open sea, or far from a port).</summary>
        public bool Has(int level, int row, int col) => level >= 0 && level < levels.Count && levels[level].Has(row, col);

        /// <summary>A tile's picture, or null while it isn't read yet (it's asked for: Pump reads it) or has no file.</summary>
        public Texture2D Tile(int row, int col) => Tile(0, row, col);

        /// <summary>The picture of a tile of a level, or null while it isn't read yet (it's asked for: Pump reads it) or
        /// has no file.</summary>
        public Texture2D Tile(int level, int row, int col)
        {
            if (!Has(level, row, col))
                return null;
            int key = Key(level, row, col);
            if (tiles.TryGetValue(key, out var t))
            {
                tiles[key] = (t.tex, frame);
                return t.tex;
            }
            if (!missing.Contains(key) && !reading.Contains(key) && !wanted.Contains(key))
                wanted.Add(key);
            return null;
        }

        /// <summary>Each frame the chart is drawn: a tile that has been read is put on the card (one a frame), the tiles
        /// asked for last are given to the readers, and the least lately used are let go.</summary>
        public void Pump()
        {
            frame++;
            if (ReadOnMainThread)
            {
                if (wanted.Count > 0)
                {
                    int key = wanted[wanted.Count - 1];
                    wanted.RemoveAt(wanted.Count - 1);
                    Whole(key, PathOf(key));
                }
            }
            else
            {
                // one read tile on to the card (the one part that must be done here)
                while (done.TryDequeue(out var d))
                {
                    if (d.generation != generation)
                    {
                        Spare(d.pixels);
                        continue;                                         // (read in colours since changed: the new reading has the key now)
                    }
                    reading.Remove(d.key);
                    if (d.pixels == null)
                    {
                        if (d.declined)
                            Whole(d.key, d.path);                         // (a PNG of a kind MapPng does not read: the old way)
                        else
                            missing.Add(d.key);
                        break;
                    }
                    var tex = new Texture2D(d.width, d.height, TextureFormat.RGBA32, false)
                        { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = Path.GetFileName(d.path) };
                    tex.SetPixelData(d.pixels, 0);
                    tex.Apply(false, true);
                    Spare(d.pixels);
                    if (tiles.TryGetValue(d.key, out var had) && had.tex != null)
                        UnityEngine.Object.Destroy(had.tex);              // (never two pictures of one tile)
                    tiles[d.key] = (tex, frame);
                    break;
                }
                // the newest askings to the readers; what is left was asked for by a view since moved on, and is dropped
                // (the view asks again each frame for what it still shows)
                while (wanted.Count > 0 && reading.Count < Readers)
                {
                    int key = wanted[wanted.Count - 1];
                    wanted.RemoveAt(wanted.Count - 1);
                    if (reading.Contains(key) || tiles.ContainsKey(key))
                        continue;
                    reading.Add(key);
                    string path = PathOf(key);
                    int which = MapPalette.Scheme, gen = generation;
                    Task.Run(() => ReadOff(key, path, which, gen));
                }
                wanted.Clear();
            }
            while (tiles.Count > Math.Max(2, capacity))
            {
                int oldest = -1, age = int.MaxValue;
                foreach (var kv in tiles)
                    if (kv.Value.used < age)
                    {
                        age = kv.Value.used;
                        oldest = kv.Key;
                    }
                if (oldest < 0 || age >= frame - 1)
                    break;                                                 // (everything kept is on screen)
                UnityEngine.Object.Destroy(tiles[oldest].tex);
                tiles.Remove(oldest);
            }
        }

        /// <summary>A big array back for the next reading: two are kept, more are left to the collector.</summary>
        void Spare(Color32[] pixels)
        {
            if (pixels != null && spare.Count < 2)
                spare.Add(pixels);
        }

        string PathOf(int key)
        {
            int level = key >> 20, row = (key >> 10) & 1023, col = key & 1023;
            return Path.Combine(Folder, LevelInfo(level).pattern.Replace("{row}", row.ToString()).Replace("{col}", col.ToString()));
        }

        /// <summary>A tile read whole on the main thread, the old way (a PNG of a kind MapPng declines; a test).</summary>
        void Whole(int key, string path)
        {
            Texture2D tex = null;
            if (File.Exists(path))
            {
                try { tex = Read(path); }
                catch (Exception e) { Debug.LogWarning("Kapman map art: " + path + ": " + e.Message); }
            }
            if (tex == null)
                missing.Add(key);
            else
            {
                if (tiles.TryGetValue(key, out var had) && had.tex != null)
                    UnityEngine.Object.Destroy(had.tex);
                tiles[key] = (tex, frame);
            }
        }

        /// <summary>On a thread of its own: the file read, unpacked and recoloured (MapPng), and handed back.</summary>
        void ReadOff(int key, string path, int which, int gen)
        {
            var d = new Done { key = key, generation = gen, path = path };
            try
            {
                if (File.Exists(path))
                {
                    spare.TryTake(out var buffer);
                    d.pixels = MapPng.Read(File.ReadAllBytes(path), which, buffer, out d.width, out d.height);
                    if (d.pixels == null)
                    {
                        d.declined = true;
                        if (buffer != null)
                            spare.Add(buffer);
                    }
                }
            }
            catch (Exception e)
            {
                d.pixels = null;
                d.declined = true;
                Debug.LogWarning("Kapman map art: " + path + ": " + e.Message + " (read the old way)");
            }
            done.Enqueue(d);
        }

        /// <summary>True while a tile's picture is held (once let go, its Image must drop it).</summary>
        public bool Holds(int row, int col) => Holds(0, row, col);

        /// <summary>True while the picture of a tile of a level is held.</summary>
        public bool Holds(int level, int row, int col) => tiles.ContainsKey(Key(level, row, col));

        /// <summary>The colours changed (MapPalette.Scheme): everything read is let go and read again in the new ones.
        /// Whoever shows the pictures must drop them first.</summary>
        public void Recolour()
        {
            foreach (var t in tiles.Values)
                if (t.tex != null)
                    UnityEngine.Object.Destroy(t.tex);
            tiles.Clear();
            wanted.Clear();
            missing.Clear();
            generation++;                                                 // (what the readers have in hand is in the old colours: dropped when it comes)
            reading.Clear();
            if (Overview != null)
                UnityEngine.Object.Destroy(Overview);
            string over = Path.Combine(Folder, overviewFile);
            Overview = File.Exists(over) ? Read(over) : null;
        }

        // play mode starts without a domain reload: look again (the user may have redrawn a tile)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            if (shared != null)
            {
                foreach (var t in shared.tiles.Values)
                    if (t.tex != null)
                        Drop(t.tex);
                if (shared.Overview != null)
                    Drop(shared.Overview);
            }
            shared = null;
            looked = false;
        }

        /// <summary>A picture given up, in play or out of it (9 Oct 2026: a test that looked at the chart outside play left
        /// its overview to be destroyed by the next new game, which outside play is an error).</summary>
        static void Drop(Texture2D tex)
        {
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(tex);
            else
                UnityEngine.Object.DestroyImmediate(tex);
        }
    }
}
