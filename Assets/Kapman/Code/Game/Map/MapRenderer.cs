using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>A drawn map tile: one tile of the baked land at a level, as a picture (256 x 256).</summary>
    public sealed class MapTile
    {
        public int level, col, row;
        public Texture2D texture;
        public bool ready;
        public int lastUsed;
        internal bool queued;
    }

    /// <summary>
    /// THE MAP'S DRAUGHTSMAN - draws the 2D map straight from the baked land (the first build's world bake, MapLand; the 3D
    /// levels of the rebuild are cut from the same bake, so the map is a true trace of them), tile by tile, on worker threads:
    ///   - the sea pale silver, a little darker where it's deep, with fine lines following the coast out to sea
    ///     (at 4, 15 and 40 m deep), as the old charts drew them
    ///   - the land a light silver-parchment, softly shaded by its relief (lit from the north-west) and a touch
    ///     darker on high ground, with faint contours
    ///   - the coast, the rivers' banks and the lakes' shores in dark ink
    /// The rivers cut for the ship (Bake/rivers) show as water with inked banks, as on the sea.
    /// Each tile is drawn once and kept (the least used go first); drawing is off the main thread, and finished
    /// tiles are handed to the GPU a few per frame.
    /// </summary>
    public sealed class MapRenderer
    {
        public const int Size = 256;

        readonly MapLand land;
        readonly Dictionary<long, MapTile> tiles = new Dictionary<long, MapTile>();
        readonly ConcurrentQueue<(MapTile tile, Color32[] pixels)> done = new ConcurrentQueue<(MapTile, Color32[])>();
        readonly int capacity;
        int working, frame;

        /// <summary>Tiles being drawn at once (worker threads).</summary>
        public int maxWorking = 6;

        // the palette (silver and ink), with the 800 AD ground's kinds as faint tints
        static readonly Color SeaShallow = C(209, 217, 226), SeaDeep = C(184, 196, 210), Flats = C(220, 222, 220);
        static readonly Color LandTone = C(238, 235, 227), Highland = C(211, 206, 196);
        static readonly Color Marsh = C(224, 230, 220), Peat = C(222, 214, 202), Sand = C(245, 239, 222);
        static readonly Color Ink = C(48, 54, 62);

        static Color C(int r, int g, int b) => new Color(r / 255f, g / 255f, b / 255f, 1f);

        public MapRenderer(MapLand land, int capacity = 600)
        {
            this.land = land;
            this.capacity = capacity;
        }

        public MapLand Land => land;

        /// <summary>Real metres across one cell of a level.</summary>
        public double Cell(int level) => land.Levels[Mathf.Clamp(level, 0, land.LevelCount - 1)].cell;

        /// <summary>The world rectangle (real metres) a tile covers: west, north, and its size.</summary>
        public (double west, double north, double size) Bounds(int level, int col, int row)
        {
            var lv = land.Levels[level];
            double size = Size * lv.cell;
            return (lv.west + col * size, lv.north - row * size, size);
        }

        static long Key(int level, int col, int row) => ((long)level << 48) | ((long)(uint)row << 24) | (uint)col;

        /// <summary>A tile, drawn or on its way (asks for it to be drawn if it isn't).</summary>
        public MapTile Get(int level, int col, int row)
        {
            long key = Key(level, col, row);
            if (!tiles.TryGetValue(key, out var t))
            {
                t = new MapTile { level = level, col = col, row = row };
                tiles[key] = t;
            }
            t.lastUsed = frame;
            if (!t.ready && !t.queued && working < maxWorking)
            {
                t.queued = true;
                working++;
                Task.Run(() =>
                {
                    Color32[] px = null;
                    try { px = Draw(level, col, row); }
                    catch (Exception e) { Debug.LogWarning("Kapman map: tile " + level + "/" + col + "/" + row + ": " + e.Message); }
                    done.Enqueue((t, px));
                });
            }
            return t;
        }

        /// <summary>Hands finished tiles to the GPU (a few a frame) and lets go of the least used. Call each frame.</summary>
        public void Pump(int perFrame = 6)
        {
            frame++;
            for (int i = 0; i < perFrame && done.TryDequeue(out var d); i++)
            {
                working--;
                d.tile.queued = false;
                if (d.pixels == null)
                    continue;
                if (d.tile.texture == null)
                    d.tile.texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
                        { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, name = $"Map {d.tile.level}/{d.tile.col}/{d.tile.row}" };
                d.tile.texture.SetPixelData(d.pixels, 0);
                d.tile.texture.Apply(false);
                d.tile.ready = true;
            }
            if (tiles.Count > capacity)
            {
                var old = new List<KeyValuePair<long, MapTile>>(tiles);
                old.Sort((a, b) => a.Value.lastUsed.CompareTo(b.Value.lastUsed));
                for (int i = 0; i < old.Count - capacity; i++)
                {
                    var t = old[i].Value;
                    if (t.queued || t.lastUsed >= frame - 2)
                        continue;
                    if (t.texture != null)
                        UnityEngine.Object.Destroy(t.texture);
                    tiles.Remove(old[i].Key);
                }
            }
        }

        public void Clear()
        {
            foreach (var t in tiles.Values)
                if (t.texture != null)
                    UnityEngine.Object.Destroy(t.texture);
            tiles.Clear();
        }

        // ------------------------------------------------------------------------------------------ drawing

        /// <summary>One tile's picture (rows top = north, as Texture2D wants bottom-up: flipped on writing).
        /// Lines (the coast, the depth lines, the contours) are drawn by their distance from each pixel (the height
        /// over its slope), so they're smooth and even, not stepped cell by cell.</summary>
        Color32[] Draw(int level, int col, int row)
        {
            int n = Size, w = n + 2;
            var h = new float[w * w];
            var s = new byte[n * n];
            land.ReadHeights(level, col * n - 1, row * n - 1, w, w, h);
            land.ReadSurfaces(level, col * n, row * n, n, n, s);
            double cell = Cell(level);
            // relief: slopes exaggerated more at coarse levels, so hills read at every zoom
            float ex = 1.5f * Mathf.Sqrt((float)(cell / 30.0));
            float contour = cell <= 60 ? 100f : cell <= 240 ? 250f : cell <= 960 ? 500f : 1000f;
            var lx = new Vector3(-0.5f, 0.5f, 0.7f).normalized;             // light from the north-west, high
            var px = new Color32[n * n];
            for (int i = 0; i < n; i++)
            {
                int ri = (i + 1) * w, gy = row * n + i;
                for (int j = 0; j < n; j++)
                {
                    int c = ri + j + 1, gx = col * n + j;
                    float hc = h[c], hl = h[c - 1], hr = h[c + 1], hu = h[c - w], hd = h[c + w];
                    byte kind = s[i * n + j];
                    // the height's slope (metres a pixel), and the distance in pixels to the coast (+ on land)
                    float sx = (hr - hl) * 0.5f, sz = (hd - hu) * 0.5f;
                    float g = Mathf.Sqrt(sx * sx + sz * sz) + 1e-3f;
                    float toCoast = hc / g;
                    bool lake = (kind == 13 || kind == 15) && hc >= 0f;      // inland water of 800 AD above the sea
                    float wet = lake ? 1f : Mathf.Clamp01(0.5f - toCoast);    // how much of the pixel is water

                    var colour = default(Color);
                    if (wet > 0f)
                    {
                        float depth = Mathf.Sqrt(Mathf.Clamp01(-hc / 300f));
                        var water = Color.Lerp(SeaShallow, SeaDeep, depth);
                        if (kind == 2)                                        // tidal flats: paler, dotted
                            water = Dot(Flats, gx, gy, 3, 1, 0.16f);
                        // the depth lines out from the coast (4, 15 and 40 m)
                        float a = Line(hc, -4f, g) * 0.32f + Line(hc, -15f, g) * 0.2f + Line(hc, -40f, g) * 0.12f;
                        colour = Color.Lerp(water, Ink, a);
                    }
                    if (wet < 1f)
                    {
                        float dx = (hr - hl) / (float)(2 * cell) * ex, dz = (hd - hu) / (float)(2 * cell) * ex;
                        var nrm = new Vector3(-dx, dz, 1f).normalized;            // (east, north, up): rows run south
                        float shade = Mathf.Clamp(Vector3.Dot(nrm, lx), 0f, 1f);
                        var ground = Ground(kind, Mathf.Clamp01(hc / 1800f), gx, gy);
                        float k = 0.82f + 0.3f * shade;
                        ground = new Color(Mathf.Min(1f, ground.r * k), Mathf.Min(1f, ground.g * k), Mathf.Min(1f, ground.b * k), 1f);
                        // faint contours
                        float v = Mathf.Round(hc / contour) * contour;
                        if (v > 0f)
                            ground = Color.Lerp(ground, Ink, Line(hc, v, g) * 0.1f);
                        colour = wet > 0f ? Color.Lerp(ground, colour, wet) : ground;
                    }
                    // the coast, and the shores of the lakes
                    float ink = lake ? (Shore(s, n, i, j) ? 0.55f : 0f) : Mathf.Clamp01(1.1f - Mathf.Abs(toCoast)) * 0.8f;
                    if (ink > 0f)
                        colour = Color.Lerp(colour, Ink, ink);
                    px[(n - 1 - i) * n + j] = colour;                       // Texture2D rows run bottom-up
                }
            }
            return px;
        }

        /// <summary>The ground's colour: by height, tinted by its 800 AD kind (Bake: the Vos map's types), the wet
        /// kinds with the old charts' marks (marsh in short strokes, peat in dots, sand finely dotted).</summary>
        static Color Ground(byte kind, float high, int gx, int gy)
        {
            var c = Color.Lerp(LandTone, Highland, high);
            switch (kind)
            {
                case 3: case 4: case 16: return Dash(Marsh, gx, gy);        // salt marsh, its ridges, the rivers' reeds
                case 17: return Dot(Sand, gx, gy, 4, 2, 0.1f);              // the rivers' sand and mud bars
                case 9: return Color.Lerp(c, Marsh, 0.6f);                  // river plains and brook valleys
                case 5: return Dot(Peat, gx, gy, 5, 2, 0.16f);              // peat bog
                case 2: case 6: case 7: case 12: return Dot(Sand, gx, gy, 4, 2, 0.1f);   // flats, dunes, beach plains, river dunes
                case 8: return Color.Lerp(c, Sand, 0.45f);                  // sandy uplands
                default: return c;
            }
        }

        static Color Dot(Color c, int gx, int gy, int every, int shift, float a) =>
            gy % every == 0 && (gx + (gy / every) * shift) % every == 0 ? Color.Lerp(c, Ink, a) : c;

        static Color Dash(Color c, int gx, int gy) =>
            gy % 7 == 0 && (gx + (gy / 7) * 4) % 9 < 3 ? Color.Lerp(c, Ink, 0.2f) : c;

        /// <summary>How much of a line at height v (a pixel wide) covers this pixel: by the distance to it, the
        /// height's difference over its slope.</summary>
        static float Line(float hc, float v, float slope) => Mathf.Clamp01(1f - Mathf.Abs(hc - v) / slope);

        /// <summary>Whether a lake's pixel borders dry land (within the tile).</summary>
        static bool Shore(byte[] s, int n, int i, int j)
        {
            bool Dry(int a, int b) => a >= 0 && b >= 0 && a < n && b < n && s[a * n + b] != 13 && s[a * n + b] != 15 && s[a * n + b] != 1;
            return Dry(i - 1, j) || Dry(i + 1, j) || Dry(i, j - 1) || Dry(i, j + 1);
        }
    }
}
