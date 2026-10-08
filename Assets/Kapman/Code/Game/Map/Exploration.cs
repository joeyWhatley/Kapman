using System;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// WHAT THE MERCHANT HAS SEEN - the map's fog of war. The world is a grid of CELL-metre squares, each from 0
    /// (never seen) to 1 (seen); the fog over the map is the opposite, and it hides what it covers (9 Oct 2026). It
    /// lifts round the ship as she sails (out to about SIGHT real metres: the coast and hills she'd see from the
    /// water), and the merchant knows his home waters from the start. Saved with the game (SaveGame).
    ///
    /// WHAT IS IN SIGHT NOW (the user, 7 Oct 2026, night: "nor is the fog of war interactive, it's just static": in his
    /// home waters everything was known from the start, so nothing ever lifted). Three states now, as in any game of
    /// the kind: never seen (the fog); known, but out of sight now (a thin veil: what is remembered); and in sight now,
    /// a clear round about the ship that goes with her (<see cref="See"/>, each game minute). What is in sight becomes
    /// known, as before. The sight of the moment is not saved: it is where she is.
    /// </summary>
    public static class Exploration
    {
        public const double CellSize = 8000.0;                    // real metres per fog square
        public const double Sight = 30000.0;                      // real metres seen round the ship
        public const double Home = 160000.0;                      // real metres known round home at the start
        /// <summary>
        /// THE UNKNOWN IS HIDDEN (the user, 9 Oct 2026: "Why are we doing such a subtle fog? Shouldn't it be you can't see
        /// anything you haven't been?"). The fog over what she has never seen is whole: nothing of the chart shows
        /// through it. (It was 0.94 in the first build and was thinned to 0.55 on 3 October, when the user, zoomed all the
        /// way out, saw "a thick film ... I can't see anything except a tiny little circle". The map then zoomed out to
        /// 13,000 km across, where the known waters are a dot; it stops at the painted chart now.)
        /// </summary>
        public const float FogAlpha = 1f;
        /// <summary>The fog's own colour: the frame's dark purple, not black.</summary>
        public static readonly Color32 Dark = new Color32(30, 16, 38, 255);

        static float[] seen;
        static int cols, rows;
        static double west, north;
        static Texture2D texture;
        static RectInt dirty;
        static bool anyDirty;
        /// <summary>The veil over what is known but not in sight now (0: none; the fog over the unknown is FogAlpha).</summary>
        public const float RememberedAlpha = 0.24f;
        static double sightX, sightZ, sightR;
        static bool sightOn;

        public static bool Ready => seen != null;

        /// <summary>The fog as a picture over the world (white, with its alpha the fog): for the map.</summary>
        public static Texture2D Texture => texture;
        public static (double west, double north, double east, double south) Extent =>
            (west, north, west + cols * CellSize, north - rows * CellSize);

        /// <summary>A fresh world, all in fog, over the land's bounds (real metres).</summary>
        public static void Begin(double w, double n, double e, double s)
        {
            west = w;
            north = n;
            cols = Math.Max(1, (int)Math.Ceiling((e - w) / CellSize));
            rows = Math.Max(1, (int)Math.Ceiling((n - s) / CellSize));
            seen = new float[cols * rows];
            if (texture != null)
            {
                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(texture);
                else
                    UnityEngine.Object.DestroyImmediate(texture);
            }
            texture = new Texture2D(cols, rows, TextureFormat.RGBA32, false)
                { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, name = "Map fog" };
            var px = new Color32[cols * rows];
            for (int i = 0; i < px.Length; i++)
                px[i] = new Color32(Dark.r, Dark.g, Dark.b, (byte)(255 * FogAlpha));
            texture.SetPixels32(px);
            texture.Apply(false);
            anyDirty = false;
            sightOn = false;
        }

        /// <summary>She is here and sees this far (real metres): it becomes known, and it is what is in sight now (the
        /// clear round that goes with her; the rest of what is known lies under its thin veil).</summary>
        public static void See(double x, double z, double radius)
        {
            if (seen == null)
                return;
            Reveal(x, z, radius);
            if (sightOn && radius == sightR && Math.Abs(x - sightX) < 25.0 && Math.Abs(z - sightZ) < 25.0)
                return;
            if (sightOn)
                Touch(sightX, sightZ, sightR);
            sightX = x;
            sightZ = z;
            sightR = radius;
            sightOn = true;
            Touch(x, z, radius);
        }

        /// <summary>How far a place is in sight now, 0-1 (0 with no sight set).</summary>
        public static float InSight(double x, double z)
        {
            if (!sightOn)
                return 0f;
            double d = Math.Sqrt((x - sightX) * (x - sightX) + (z - sightZ) * (z - sightZ));
            return Mathf.Clamp01((float)((sightR + CellSize - d) / (2.0 * CellSize)));
        }

        /// <summary>The squares round a place are to be drawn again.</summary>
        static void Touch(double x, double z, double radius)
        {
            int c0 = Math.Max(0, (int)Math.Floor((x - radius - CellSize - west) / CellSize));
            int c1 = Math.Min(cols - 1, (int)Math.Floor((x + radius + CellSize - west) / CellSize));
            int r0 = Math.Max(0, (int)Math.Floor((north - z - radius - CellSize) / CellSize));
            int r1 = Math.Min(rows - 1, (int)Math.Floor((north - z + radius + CellSize) / CellSize));
            if (c1 < c0 || r1 < r0)
                return;
            Mark(new RectInt(c0, r0, c1 - c0 + 1, r1 - r0 + 1));
        }

        static void Mark(RectInt box)
        {
            if (!anyDirty)
                dirty = box;
            else
            {
                int xMin = Math.Min(dirty.xMin, box.xMin), yMin = Math.Min(dirty.yMin, box.yMin);
                int xMax = Math.Max(dirty.xMax, box.xMax), yMax = Math.Max(dirty.yMax, box.yMax);
                dirty = new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
            }
            anyDirty = true;
        }

        /// <summary>How well a place is known, 0-1.</summary>
        public static float Seen(double x, double z)
        {
            if (seen == null)
                return 1f;
            int c = (int)Math.Floor((x - west) / CellSize), r = (int)Math.Floor((north - z) / CellSize);
            return c < 0 || r < 0 || c >= cols || r >= rows ? 0f : seen[r * cols + c];
        }

        /// <summary>Lifts the fog round a place (real metres), fading out over the last two squares.</summary>
        public static void Reveal(double x, double z, double radius)
        {
            if (seen == null)
                return;
            int c0 = Math.Max(0, (int)Math.Floor((x - radius - CellSize - west) / CellSize));
            int c1 = Math.Min(cols - 1, (int)Math.Floor((x + radius + CellSize - west) / CellSize));
            int r0 = Math.Max(0, (int)Math.Floor((north - z - radius - CellSize) / CellSize));
            int r1 = Math.Min(rows - 1, (int)Math.Floor((north - z + radius + CellSize) / CellSize));
            bool changed = false;
            for (int r = r0; r <= r1; r++)
                for (int c = c0; c <= c1; c++)
                {
                    double cx = west + (c + 0.5) * CellSize, cz = north - (r + 0.5) * CellSize;
                    double d = Math.Sqrt((cx - x) * (cx - x) + (cz - z) * (cz - z));
                    float v = Mathf.Clamp01((float)((radius + CellSize - d) / (2.0 * CellSize)));
                    int i = r * cols + c;
                    if (v > seen[i] + 0.01f)
                    {
                        seen[i] = v;
                        changed = true;
                    }
                }
            if (!changed)
                return;
            Mark(new RectInt(c0, r0, c1 - c0 + 1, r1 - r0 + 1));
        }

        /// <summary>Puts the fog's changes into its picture (call once a frame or so).</summary>
        public static void Apply()
        {
            if (!anyDirty || texture == null)
                return;
            var px = new Color32[dirty.width * dirty.height];
            for (int r = 0; r < dirty.height; r++)
                for (int c = 0; c < dirty.width; c++)
                {
                    float known = seen[(dirty.yMin + r) * cols + dirty.xMin + c];
                    // the fog over the unknown; over the known a thin veil, lifted where she sees now
                    float veil = FogAlpha * (1f - known);
                    if (sightOn && known > 0f)
                        veil += RememberedAlpha * known * (1f - InSight(west + (dirty.xMin + c + 0.5) * CellSize, north - (dirty.yMin + r + 0.5) * CellSize));
                    // the texture's rows run bottom-up (south first)
                    px[(dirty.height - 1 - r) * dirty.width + c] = new Color32(Dark.r, Dark.g, Dark.b, (byte)(255 * Mathf.Clamp01(veil)));
                }
            texture.SetPixels32(dirty.xMin, rows - dirty.yMin - dirty.height, dirty.width, dirty.height, px);
            texture.Apply(false);
            anyDirty = false;
        }

        /// <summary>What's been seen, as plain data (a saved game): the grid and its cells (base64 floats).</summary>
        public static System.Collections.Generic.Dictionary<string, object> Capture() => seen == null ? null :
            new System.Collections.Generic.Dictionary<string, object>
            {
                ["cols"] = cols, ["rows"] = rows, ["west"] = west, ["north"] = north, ["seen"] = Json.Floats(seen),
            };

        /// <summary>What was seen, as saved (once the fog has begun over the same land).</summary>
        public static void Restore(System.Collections.Generic.Dictionary<string, object> d)
        {
            if (d == null || seen == null)
                return;
            var cells = Json.Floats(Json.Str(d, "seen"));
            if ((int)Json.Num(d, "cols") != cols || (int)Json.Num(d, "rows") != rows || cells.Length != seen.Length)
                return;                                              // (another map: keep what's here)
            Array.Copy(cells, seen, seen.Length);
            dirty = new RectInt(0, 0, cols, rows);
            anyDirty = true;
            Apply();
        }

        /// <summary>No fog at all: everything counts as known (before a game has a chart; a test that made a fog of its own).</summary>
        public static void Clear()
        {
            seen = null;
            texture = null;
            anyDirty = false;
            sightOn = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() => Clear();
    }
}
