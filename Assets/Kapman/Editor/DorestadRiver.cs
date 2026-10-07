using System.Collections.Generic;
using System.Linq;
using Kapman.Levels;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kapman.EditorTools
{
    /// <summary>
    /// THE RIVERS AND THEIR SHORES - what a 800 AD Lek and Kromme Rijn had and the lidar's smoothed bake lost: point
    /// bars on the insides of the bends (sand, just over the water), cut banks on the outsides (steep), shelving mud
    /// shores, pools in the lowest basin hollows, the old channel scars as wet gullies, the harbour's shoal (the broad
    /// silted bed the dams were built out over as the channel moved east: DRY in the sailing season but for its outer
    /// end, the ditches between the dam pairs holding puddles, a shelving beach beyond the heads, from the excavations),
    /// a sand tongue at the fork, the Lek's sand islands and the Kromme Rijn's ford, cattle terracettes on the grazed
    /// margins, and 20-40 cm of micro-relief so the ground isn't a sheet. Shape() edits the heights before the town and
    /// the terrain and leaves a bend-side map for the painter; Dress() adds the props after: fish weirs in the basin
    /// gullies, boats drawn up in the dry ditches, a hull being broken up on the beach, mooring posts.
    /// </summary>
    public static class DorestadRiver
    {
        public const string Dir = DorestadBuilder.LevelDir;

        /// <summary>The harbour's shoal: on the town's side of the Kromme Rijn, from the bank out to the dams' heads.</summary>
        public struct Shoal
        {
            public Vector2 bankStart, bankEnd;    // where the shoal begins and ends along the bank
        }

        // Which side of a bend each cell near a river lies on, written by Shape() for the painter (DorestadLandCover
        // reads it through SideAt: a cut bank gets a metre of mud, a point bar's shore twenty, a straight reach a few).
        public const byte SideNone = 0, SideInner = 1, SideOuter = 2, SideStraight = 3;
        public static byte[] BendSide;          // SideNone / SideInner / SideOuter / SideStraight per cell
        public static byte[] BendStrength;      // |curvature| 0..255 per cell (0 on a straight reach)
        static int bendN;

        public static byte SideAt(DorestadBuilder.Grid g, Vector2 p)
        {
            if (BendSide == null || bendN != g.n)
                return SideNone;
            int x = Mathf.Clamp(Mathf.RoundToInt(p.x / g.cell), 0, g.n - 1), z = Mathf.Clamp(Mathf.RoundToInt(p.y / g.cell), 0, g.n - 1);
            return BendSide[z * g.n + x];
        }

        public static float StrengthAt(DorestadBuilder.Grid g, Vector2 p)
        {
            if (BendStrength == null || bendN != g.n)
                return 0f;
            int x = Mathf.Clamp(Mathf.RoundToInt(p.x / g.cell), 0, g.n - 1), z = Mathf.Clamp(Mathf.RoundToInt(p.y / g.cell), 0, g.n - 1);
            return BendStrength[z * g.n + x] / 255f;
        }

        // the ford on the Kromme Rijn below the harbour, set by Shape() for the town to lay its tracks to: the point
        // on the channel's centre line, the direction across the river toward the town's bank, and whether one was made
        public static Vector2 Ford, FordToTown;
        public static bool HasFord;

        // the dams' breadth (DorestadTown.Build: damW), so the ditches between the pairs are known here too
        const float DamWidth = 6.5f;
        // Hoogstraat II, where the ship nails and the dismantled hull were found (ED4 ch. VII 3.9): about 150 m
        // downstream of the bend's apex, which DorestadTown puts at 850 m from the fork
        const float HoogstraatTwo = 1000f;

        public static void Shape(DorestadBuilder.Grid g, byte[] cover, List<DorestadTown.Parcel> parcels)
        {
            int n = g.n;
            float wl = g.wl, cell = g.cell;
            var h = g.h;
            var sd = g.sd;
            int changed = 0;
            var rnd = new System.Random(7930);
            float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);
            BendSide = new byte[n * n];
            BendStrength = new byte[n * n];
            bendN = n;
            HasFord = false;

            // 1. micro-relief: two scales of gentle noise on every bit of land, more in the basins (hummocks, old
            //    ridge-and-furrow) and none on the water; and cattle terracettes on the grazed margins within 300 m of
            //    the water (the hoof-printed Lek silt at the levee's edge, Van der Tuuk ch. 2): 3-5 cm steps that repeat
            //    every 4 m across the slope, i.e. run along the contour
            for (int z = 0; z < n; z++)
                for (int x = 0; x < n; x++)
                {
                    int i = z * n + x;
                    if (sd[i] > -3f)
                        continue;
                    float px = x * cell, pz = z * cell;
                    float r = (Mathf.PerlinNoise(px / 23f + 50f, pz / 23f + 50f) - 0.5f) * 0.28f
                            + (Mathf.PerlinNoise(px / 7f + 80f, pz / 7f + 80f) - 0.5f) * 0.12f;
                    float shore = Mathf.Clamp01(-sd[i] / 12f);          // fades out at the water's edge
                    byte cv = cover != null ? cover[i] : DorestadLandCover.Crest;
                    float amount = cv == DorestadLandCover.Basin || cv == DorestadLandCover.Reed || cv == DorestadLandCover.Carr ? 1.3f : 0.8f;
                    h[i] += r * shore * amount;
                    if (sd[i] > -300f && (cv == DorestadLandCover.Flank || cv == DorestadLandCover.Basin)
                        && x >= 2 && z >= 2 && x < n - 2 && z < n - 2
                        && Mathf.PerlinNoise(px / 35f + 61f, pz / 35f + 29f) > 0.62f)
                    {
                        float gx = h[i + 2] - h[i - 2], gz = h[i + 2 * n] - h[i - 2 * n];
                        float gl = Mathf.Sqrt(gx * gx + gz * gz);
                        if (gl > 1e-4f)
                        {
                            float across = (px * gx + pz * gz) / gl;                 // distance up the slope
                            float saw = Mathf.Repeat(across / 4f, 1f);              // a step every 4 m
                            h[i] += (saw - 0.5f) * (0.03f + 0.02f * Mathf.PerlinNoise(px / 50f, pz / 50f));
                        }
                    }
                }

            // 2. the bends: the river's curvature along its channel lines decides bars and cut banks, and every cell
            //    of the shore is told which side of a bend it lies on (for the painter)
            var lekLine = LekLine(g, out var lekWidths);
            var lines = new List<List<Vector2>> { g.channel, lekLine };
            foreach (var line in lines)
            {
                if (line.Count < 6)
                    continue;
                var s = new float[line.Count];
                for (int i = 1; i < line.Count; i++)
                    s[i] = s[i - 1] + Vector2.Distance(line[i - 1], line[i]);
                for (int i = 2; i < line.Count - 2; i++)
                {
                    Vector2 t = (line[i + 2] - line[i - 2]).normalized;
                    Vector2 nrm = new Vector2(-t.y, t.x);
                    // curvature: which way the line turns here (positive: turns left)
                    Vector2 t0 = (line[i] - line[i - 2]).normalized, t1 = (line[i + 2] - line[i]).normalized;
                    float turn = t0.x * t1.y - t0.y * t1.x;
                    float k = Mathf.Clamp(turn / Mathf.Max(1f, s[i + 2] - s[i - 2]) * 400f, -1f, 1f);
                    bool straight = Mathf.Abs(k) < 0.08f;
                    Vector2 c = line[i];

                    // the bend-side map: a band across the river and 45 m up both shores, half-way to the neighbours
                    // along the line so the bands join (the inside of a left turn is on the left)
                    byte sideLeft = straight ? SideStraight : (k > 0 ? SideInner : SideOuter);
                    byte sideRight = straight ? SideStraight : (k > 0 ? SideOuter : SideInner);
                    byte strength = (byte)Mathf.RoundToInt(Mathf.Clamp01(Mathf.Abs(k)) * 255f);
                    float halfStep = (s[Mathf.Min(i + 1, s.Length - 1)] - s[Mathf.Max(i - 1, 0)]) / 4f + cell;
                    for (float u = 0f; u <= 200f; u += cell)
                        for (float v = -halfStep; v <= halfStep; v += cell)
                            for (int sign = -1; sign <= 1; sign += 2)
                            {
                                Vector2 p = c + nrm * (sign * u) + t * v;
                                int xi = Mathf.RoundToInt(p.x / cell), zi = Mathf.RoundToInt(p.y / cell);
                                if (xi < 0 || zi < 0 || xi >= n || zi >= n)
                                    continue;
                                int j = zi * n + xi;
                                if (sd[j] > 8f || sd[j] < -45f)
                                    continue;
                                BendSide[j] = sign > 0 ? sideLeft : sideRight;
                                BendStrength[j] = strength;
                            }
                    if (straight)
                        continue;
                    float inner = k > 0 ? 1f : -1f;                   // the inside of the bend is on the left when turning left
                    // the bar: inside of the bend, from the water's edge out across the shallows and up onto the shore
                    float reach = 10f + 60f * Mathf.Abs(k);
                    for (float u = -reach * 0.8f; u <= reach; u += cell)
                        for (float v = -6f; v <= 6f; v += cell)
                        {
                            Vector2 p = c + nrm * (inner * u) + t * v;
                            int xi = Mathf.RoundToInt(p.x / cell), zi = Mathf.RoundToInt(p.y / cell);
                            if (xi < 0 || zi < 0 || xi >= n || zi >= n)
                                continue;
                            int j = zi * n + xi;
                            float d = sd[j];
                            if (d > 0f && d < 25f * Mathf.Abs(k) + 6f)
                            {
                                // the shallows on the inside: the bed rises toward a bar just over the water
                                float rise = Mathf.Lerp(wl + 0.15f, wl - 0.6f, Mathf.Clamp01(d / (25f * Mathf.Abs(k) + 6f)));
                                float noise = Mathf.PerlinNoise(p.x / 18f, p.y / 18f) * 0.25f;
                                if (h[j] < rise + noise)
                                {
                                    h[j] = rise + noise;
                                    if (h[j] > wl + 0.02f)
                                    {
                                        sd[j] = -1f;   // it stands out of the water: a bar
                                        if (cover != null) cover[j] = DorestadLandCover.Bar;
                                    }
                                    changed++;
                                }
                            }
                            else if (d <= 0f && d > -14f)
                            {
                                // the shore inside the bend shelves gently
                                float target = wl + 0.05f + (-d) * 0.045f;
                                if (h[j] > target)
                                {
                                    h[j] = Mathf.Lerp(h[j], target, 0.7f);
                                    if (cover != null && cover[j] != DorestadLandCover.Bar) cover[j] = DorestadLandCover.Mud;
                                    changed++;
                                }
                            }
                        }
                    // the cut bank: outside of the bend, steep and a little higher
                    for (float u = 0f; u <= 10f; u += cell)
                        for (float v = -6f; v <= 6f; v += cell)
                        {
                            Vector2 p = c - nrm * (inner * u) + t * v;
                            int xi = Mathf.RoundToInt(p.x / cell), zi = Mathf.RoundToInt(p.y / cell);
                            if (xi < 0 || zi < 0 || xi >= n || zi >= n)
                                continue;
                            int j = zi * n + xi;
                            float d = sd[j];
                            if (d <= 0f && d > -8f)
                            {
                                float lip = wl + 0.6f + 0.25f * Mathf.Abs(k) + (-d) * 0.08f;
                                if (h[j] < lip)
                                {
                                    h[j] = lip;
                                    changed++;
                                }
                            }
                            else if (d > 0f && d < 6f && h[j] > wl - 1.6f)
                                h[j] = Mathf.Lerp(h[j], wl - 1.6f, 0.6f);       // deep right under the cut bank
                        }
                }
            }

            // 3. the harbour's shoal: the silted bed the dams run out over. The dams were built once the shoal had
            //    silted to c. 4 m NAP while sailing-season water stood at or under 3 m (ED3 ch. V 2.3; ED4 ch. VIII
            //    2.2), so on a June day most of the comb stands DRY on mud: the shoal falls from the old bank's foot
            //    to just over the water at four fifths of the dams, goes under over the last fifth (boats of 2-3 m
            //    beam lie a dozen metres into the 4 m spaces between the pairs, ships moor at the heads), and beyond
            //    the heads a shelving beach runs down into the channel over 30 m. The spaces between the pairs lie a
            //    little lower (the dams' soil was dug from them) and hold summer puddles. The dams' wattle sides then
            //    stand 0.5-1 m over the mud, as excavated. The old bank itself eases down to the shoal over 8 m.
            if (parcels != null)
                foreach (var pc in parcels)
                {
                    float L = pc.damLength;
                    const float Beach = 30f;
                    float halfW = pc.width * 0.5f + 2f;
                    float pairHalf = pc.twin ? DamWidth + 0.5f : DamWidth * 0.5f + 0.5f;   // the dams' footprint; the ditch lies beyond it
                    for (float u = -8f; u <= L + Beach; u += cell * 0.8f)
                        for (float v = -halfW; v <= halfW; v += cell * 0.8f)
                        {
                            Vector2 p = pc.bank + pc.outward * u + pc.along * v;
                            int xi = Mathf.RoundToInt(p.x / cell), zi = Mathf.RoundToInt(p.y / cell);
                            if (xi < 0 || zi < 0 || xi >= n || zi >= n)
                                continue;
                            int j = zi * n + xi;
                            if (u < 0f)
                            {
                                // the old bank's edge, from the levee down to the shoal: rounded at the top, steep at the foot
                                float tt = (u + 8f) / 8f;
                                float edge = Mathf.Lerp(h[j], wl + 0.45f, tt * tt);
                                if (edge < h[j] && sd[j] <= 0f)
                                {
                                    h[j] = edge;
                                    changed++;
                                }
                                continue;
                            }
                            float frac = u / L;
                            float target;
                            if (frac < 0.8f)
                                target = Mathf.Lerp(wl + 0.45f, wl + 0.12f, frac / 0.8f);
                            else if (frac < 1f)
                                target = Mathf.Lerp(wl + 0.12f, wl - 0.2f, (frac - 0.8f) / 0.2f);
                            else
                                target = Mathf.Lerp(wl - 0.2f, wl - 1.3f, Mathf.Sqrt(Mathf.Clamp01((u - L) / Beach)));
                            target += (Mathf.PerlinNoise(p.x / 9f, p.y / 9f) - 0.5f) * 0.16f;
                            bool ditch = !pc.empty && Mathf.Abs(v) > pairHalf && u <= L;
                            if (ditch)
                            {
                                // the spaces lie lower toward the heads (silted up at the root), so the water runs a
                                // dozen or two metres into them: 36 m at the apex's 200 m dams, 7 m at the ends' 40 m
                                target -= 0.12f * frac;
                                // summer puddles in the spaces between the pairs
                                if (frac > 0.3f && frac < 0.85f && Mathf.PerlinNoise(p.x / 9f + 7f, p.y / 9f + 3f) > 0.72f)
                                    target = Mathf.Min(target, wl - 0.12f);
                            }
                            if (u <= L)
                            {
                                // the shoal: the levee's edge cut down to it (the channel had been here before it moved
                                // east), the old bed raised to it (it silted)
                                h[j] = target;
                                bool dry = h[j] > wl;
                                sd[j] = dry ? -1f : 1f;
                                if (cover != null) cover[j] = dry ? (frac < 0.55f ? DorestadLandCover.Mud : DorestadLandCover.Bar) : DorestadLandCover.Water;
                                changed++;
                            }
                            else if (h[j] < target)
                            {
                                // the beach beyond the heads: the bed raised to the shelving profile, never lowered
                                h[j] = target;
                                if (sd[j] <= 0f)
                                {
                                    sd[j] = 1f;
                                    if (cover != null) cover[j] = DorestadLandCover.Water;
                                }
                                changed++;
                            }
                        }
                }

            // 4. pools and the old gullies: the lowest basin hollows hold water all year
            if (cover != null)
                for (int z = 1; z < n - 1; z++)
                    for (int x = 1; x < n - 1; x++)
                    {
                        int i = z * n + x;
                        if (cover[i] != DorestadLandCover.Pool)
                            continue;
                        h[i] = Mathf.Min(h[i], wl - 0.45f);
                        sd[i] = 1.5f;
                        changed++;
                        // ease the rim
                        for (int dz = -2; dz <= 2; dz++)
                            for (int dx = -2; dx <= 2; dx++)
                            {
                                int j = (z + dz) * n + x + dx;
                                if (cover[j] != DorestadLandCover.Pool)
                                {
                                    float rim = wl + 0.08f * (Mathf.Abs(dx) + Mathf.Abs(dz));
                                    if (h[j] > rim)
                                        h[j] = rim;
                                }
                            }
                    }

            // 5. the fork: a sand tongue where the Kromme Rijn leaves the Lek, on the town's side of its mouth (the
            //    wedge between the two rivers grew as the fork kept shifting, ED4 ch. VIII 1.1), trailing downstream
            //    into the Kromme Rijn: out of the water for the first 40 m, its tip under water, tapering away by
            //    90 m; reeds on its back half. It never touches the cut-bank cells, so there is no ring round it.
            float townSide = TownSide(g);
            if (g.channel.Count > 3)
            {
                Vector2 f = g.channel[0];
                Vector2 t = (g.channel[2] - g.channel[0]).normalized;
                Vector2 nrm = new Vector2(-t.y, t.x) * townSide;
                for (float u = -25f; u <= 90f; u += cell)
                    for (float vv = 0f; vv <= 42f; vv += cell)
                    {
                        Vector2 p = f + t * u + nrm * vv;
                        int xi = Mathf.RoundToInt(p.x / cell), zi = Mathf.RoundToInt(p.y / cell);
                        if (xi < 0 || zi < 0 || xi >= n || zi >= n)
                            continue;
                        int j = zi * n + xi;
                        if (sd[j] <= 0f || BendSide[j] == SideOuter)
                            continue;
                        float along = u < 0f ? -u / 25f : u / 90f;
                        float across = (vv - 16f) / 14f;
                        float r = Mathf.Sqrt(along * along + across * across);
                        if (r > 1.2f)
                            continue;
                        float crest = u <= 40f ? wl + 0.3f : Mathf.Lerp(wl + 0.3f, wl - 1.2f, (u - 40f) / 50f);
                        float bar = Mathf.Lerp(crest, wl - 1.4f, Mathf.Pow(Mathf.Clamp01(r), 2f)) + (Mathf.PerlinNoise(p.x / 14f, p.y / 14f) - 0.5f) * 0.3f;
                        if (h[j] < bar)
                        {
                            h[j] = bar;
                            if (h[j] > wl + 0.02f)
                            {
                                sd[j] = -1f;
                                if (cover != null) cover[j] = u < 25f ? DorestadLandCover.Reed : DorestadLandCover.Bar;
                            }
                            changed++;
                        }
                    }
            }

            // 6. the Lek's sand islands (middelwaarden: the pre-regulation Nederrijn had at least ten, Tastbare Tijd;
            //    for 800 a 100-200 m channel with bars and islands is the notes' guess) every 450-650 m where the
            //    channel is wider than 170 m, and the ford on the Kromme Rijn 500 m below the harbour: a low sandy
            //    island with the channel split round it, knee-deep on the far side, still deep enough for a ship on the
            //    town's side ('several not very wide channels round a low sandy island', NO18 ch. VIII 2). Where each
            //    island lies along the river and how big it is are guesses within the notes' sizes.
            int islands = 0;
            if (lekLine.Count > 6)
            {
                float since = 250f, next = R(450f, 650f);
                for (int i = 2; i < lekLine.Count - 2; i++)
                {
                    since += Vector2.Distance(lekLine[i - 1], lekLine[i]);
                    if (since < next || lekWidths[i] < 170f || Vector2.Distance(lekLine[i], g.channel.Count > 0 ? g.channel[0] : Vector2.one * -1e5f) < 300f)
                        continue;
                    Vector2 t = (lekLine[i + 2] - lekLine[i - 2]).normalized;
                    Vector2 nrm = new Vector2(-t.y, t.x);
                    Vector2 c = lekLine[i] + nrm * R(-25f, 25f);
                    changed += Island(g, cover, c, t, R(30f, 60f), R(12f, 20f), wl + 0.3f, wl - 1.0f);
                    islands++;
                    since = 0f;
                    next = R(450f, 650f);
                }
            }
            if (g.channel.Count > 10)
            {
                float harbourEnd = parcels != null && parcels.Count > 0 ? parcels.Max(p => p.s) : 1700f;
                float fordAt = harbourEnd + R(400f, 600f);
                float along = 0f;
                for (int i = 1; i < g.channel.Count - 3; i++)
                {
                    along += Vector2.Distance(g.channel[i - 1], g.channel[i]);
                    if (along < fordAt)
                        continue;
                    Vector2 t = (g.channel[i + 2] - g.channel[i - 1]).normalized;
                    Vector2 nrm = new Vector2(-t.y, t.x) * townSide;
                    Vector2 c = g.channel[i] - nrm * 20f;                     // off the centre line, on the far side
                    changed += Island(g, cover, c, t, 35f, 10f, wl + 0.25f, wl - 0.9f);
                    Ford = g.channel[i];
                    FordToTown = nrm;
                    HasFord = true;
                    // the ford's shallows: the far channel wading depth, the town's channel still a metre deep
                    for (float u = -45f; u <= 45f; u += cell)
                        for (float v = -50f; v <= 50f; v += cell)
                        {
                            Vector2 p = g.channel[i] + t * u + nrm * v;
                            int xi = Mathf.RoundToInt(p.x / cell), zi = Mathf.RoundToInt(p.y / cell);
                            if (xi < 0 || zi < 0 || xi >= n || zi >= n)
                                continue;
                            int j = zi * n + xi;
                            if (sd[j] <= 0f)
                                continue;
                            float bed = v < -8f ? wl - 0.45f : wl - 1.0f;
                            bed += (Mathf.PerlinNoise(p.x / 11f + 3f, p.y / 11f + 5f) - 0.5f) * 0.2f;
                            float fade = Mathf.Clamp01((45f - Mathf.Abs(u)) / 15f);
                            float target = Mathf.Lerp(h[j], bed, fade);
                            if (h[j] < target)
                            {
                                h[j] = target;
                                changed++;
                            }
                        }
                    islands++;
                    break;
                }
            }
            Debug.Log($"Kapman river: {changed:n0} cells shaped (bars, cut banks, terracettes, the harbour's dry shoal and beach, pools, the fork's tongue, {islands} islands and the ford).");
        }

        /// <summary>A low sand island in a channel: an ellipse a by b along t, `top` in the middle falling to `rim` at its edge and on down into the bed.</summary>
        static int Island(DorestadBuilder.Grid g, byte[] cover, Vector2 c, Vector2 t, float a, float b, float top, float rim)
        {
            int n = g.n, changed = 0;
            float cell = g.cell, wl = g.wl;
            Vector2 nrm = new Vector2(-t.y, t.x);
            for (float u = -a * 1.4f; u <= a * 1.4f; u += cell)
                for (float v = -b * 1.4f; v <= b * 1.4f; v += cell)
                {
                    Vector2 p = c + t * u + nrm * v;
                    int xi = Mathf.RoundToInt(p.x / cell), zi = Mathf.RoundToInt(p.y / cell);
                    if (xi < 0 || zi < 0 || xi >= n || zi >= n)
                        continue;
                    int j = zi * n + xi;
                    if (g.sd[j] <= 0f)
                        continue;
                    float r = Mathf.Sqrt((u / a) * (u / a) + (v / b) * (v / b));
                    if (r > 1.4f)
                        continue;
                    float target = r <= 1f ? Mathf.Lerp(top, rim, r * r) : Mathf.Lerp(rim, rim - 1.2f, (r - 1f) / 0.4f);
                    target += (Mathf.PerlinNoise(p.x / 12f + 9f, p.y / 12f + 1f) - 0.5f) * 0.2f;
                    if (g.h[j] >= target)
                        continue;
                    g.h[j] = target;
                    if (g.h[j] > wl + 0.02f)
                    {
                        g.sd[j] = -1f;
                        if (cover != null) cover[j] = DorestadLandCover.Bar;
                    }
                    changed++;
                }
            return changed;
        }

        /// <summary>+1 when the town (the port) lies on the left of the Kromme Rijn's downstream direction, else -1 (as DorestadTown decides it).</summary>
        static float TownSide(DorestadBuilder.Grid g)
        {
            var line = g.channel;
            if (line.Count < 4)
                return 1f;
            int near = 0;
            for (int i = 1; i < line.Count; i++)
                if (Vector2.Distance(line[i], g.port) < Vector2.Distance(line[near], g.port))
                    near = i;
            int a = Mathf.Max(0, near - 3), b = Mathf.Min(line.Count - 1, near + 2);
            Vector2 t = (line[b] - line[a]).normalized;
            Vector2 left = new Vector2(-t.y, t.x);
            return Vector2.Dot(g.port - line[near], left) >= 0f ? 1f : -1f;
        }

        /// <summary>A centre line for the Lek through the level: the middle of the class-10 water, west to east, with the channel's width at each point.</summary>
        static List<Vector2> LekLine(DorestadBuilder.Grid g, out List<float> widths)
        {
            var line = new List<Vector2>();
            widths = new List<float>();
            for (float x = 10f; x < g.size; x += 40f)
            {
                float sum = 0f, count = 0f, half = 0f;
                for (float z = 0f; z < g.size; z += g.cell * 2f)
                {
                    var p = new Vector2(x, z);
                    if (g.Class(p) == 10)
                    {
                        sum += z;
                        count++;
                        half = Mathf.Max(half, g.SD(p));
                    }
                }
                if (count > 3f)
                {
                    line.Add(new Vector2(x, sum / count));
                    widths.Add(2f * half);
                }
            }
            return line;
        }

        // ------------------------------------------------------------------ props

        public static void Dress(DorestadBuilder.Grid g, DorestadTown.Plan plan, byte[] cover, Transform parent, System.Random rnd)
        {
            float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);
            var root = new GameObject("River life: weirs, boats drawn up, posts").transform;
            root.SetParent(parent, false);
            var timber = AssetLibrary.Lit("polyhaven", "rough_wood", $"{Dir}/Materials/Timber.mat", 0.08f);
            var wattle = AssetLibrary.Lit("ambientcg", "Wicker002", $"{Dir}/Materials/Wattle.mat", 0.1f);
            var piles = AssetLibrary.Lit("polyhaven", "bark_willow", $"{Dir}/Materials/Piles.mat", 0.08f);
            var linen = AssetLibrary.Lit("polyhaven", "rough_linen", $"{Dir}/Materials/Linen.mat", 0.1f);
            var postMesh = Save(MeshKit.Cylinder("Weir_post", 0.07f, 2.2f, 6, 1f), "Weir_post");
            var panelMesh = Save(MeshKit.Box("Weir_panel", new Vector3(1.4f, 1.1f, 0.06f), 1.2f), "Weir_panel");
            var trapMesh = Save(MeshKit.Cylinder("Weir_trap", 0.28f, 1.4f, 8, 1f), "Weir_trap");
            var logboat = Save(Logboat("Logboat_5m", 5.2f, 0.75f), "Logboat_5m");
            var halfHull = Save(Logboat("Hull_half_7m", 7f, 2.2f), "Hull_half_7m");
            var plankMesh = Save(MeshKit.Box("Hull_plank", new Vector3(0.26f, 0.05f, 3.4f), 1f), "Hull_plank");
            var mooring = Save(MeshKit.Cylinder("Mooring_post", 0.12f, 1.6f, 7, 1f), "Mooring_post");
            Vector2 townC = new Vector2(plan.townCentre.x, plan.townCentre.z);

            // fish weirs: V-shaped fences of posts and wattle with a wicker trap at the point, in the basin gullies
            // where a pool meets the reed (the notes put weirs in side channels and basin gullies, not in the main
            // channel), the arms opening toward the river the gully drains to
            int weirs = 0;
            if (cover != null)
            {
                var spots = WeirSpots(g, cover, 150f);
                if (spots.Count < 4)
                    spots = WeirSpots(g, cover, 400f);
                int want = Mathf.Min(spots.Count, 4 + rnd.Next(5));
                for (int w = 0; w < want; w++)
                {
                    Vector2 c = spots[w];
                    Vector2 toRiver = Vector2.zero;
                    float best = float.MaxValue;
                    foreach (var q in g.channel)
                    {
                        float d = Vector2.Distance(q, c);
                        if (d < best) { best = d; toRiver = q - c; }
                    }
                    if (best == float.MaxValue)
                        break;
                    toRiver.Normalize();
                    Vector2 perp = new Vector2(-toRiver.y, toRiver.x);
                    var weir = new GameObject($"Fish weir {weirs + 1}").transform;
                    weir.SetParent(root, false);
                    weir.position = new Vector3(c.x, g.wl, c.y);
                    float len = R(6f, 12f);
                    for (int arm = -1; arm <= 1; arm += 2)
                    {
                        Vector2 dir = (toRiver + perp * (arm * 0.6f)).normalized;
                        for (float u = 1f; u < len; u += 1.4f)
                        {
                            Vector2 p = c + dir * u;
                            float bed = Mathf.Min(g.H(p), g.wl - 0.1f);
                            var post = Part(weir, "Post", postMesh, piles, piles);
                            post.transform.SetPositionAndRotation(new Vector3(p.x, bed - 0.3f, p.y), Quaternion.Euler(R(-4f, 4f), R(0, 360), R(-4f, 4f)));
                            post.transform.localScale = new Vector3(1f, (g.wl + 0.6f - bed + R(0f, 0.3f)) / 2.2f, 1f);
                            var panel = Part(weir, "Wattle", panelMesh, wattle);
                            Vector2 mid = p + dir * 0.7f;
                            panel.transform.SetPositionAndRotation(new Vector3(mid.x, g.wl - 0.45f, mid.y), Quaternion.Euler(0f, Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg + 90f, 0f));
                        }
                    }
                    // the trap at the point, a wicker basket lying half under the water
                    var trap = Part(weir, "Wicker trap", trapMesh, wattle);
                    Vector2 tp = c - toRiver * 0.9f;
                    trap.transform.SetPositionAndRotation(new Vector3(tp.x, g.wl - 0.15f, tp.y), Quaternion.Euler(90f + R(-8f, 8f), Mathf.Atan2(toRiver.x, toRiver.y) * Mathf.Rad2Deg, 0f));
                    weirs++;
                }
            }

            // the harbour's beach: logboats drawn up in the dry spaces between the dam pairs (stern to the water), two
            // or three river boats of the Utrecht kind hauled up the same way, and near Hoogstraat II a clinker hull
            // being broken up on the landing beach (ship nails and a dismantled hull were found there)
            int ditchBoats = 0, hauled = 0;
            if (plan.parcels != null && plan.parcels.Count > 1)
            {
                var pcs = plan.parcels;
                int wantHauled = 2 + rnd.Next(2);
                for (int i = 0; i < pcs.Count - 1; i++)
                {
                    var a = pcs[i];
                    var b = pcs[i + 1];
                    if (a.empty || b.empty)
                        continue;                                  // the landing beach has its own boats
                    Vector2 edge = a.bank + a.along * (a.width * 0.5f);
                    float L = Mathf.Min(a.damLength, b.damLength);
                    float sternYaw = Mathf.Atan2(-a.outward.x, -a.outward.y) * Mathf.Rad2Deg;   // the bow inland
                    double pick = rnd.NextDouble();
                    if (pick < 0.36)
                    {
                        float u = R(0.3f, 0.74f) * L;
                        Vector2 p = edge + a.outward * u + a.along * R(-0.7f, 0.7f);
                        var lb = Part(root, "Logboat in the space between the dams", logboat, timber, timber);
                        lb.transform.SetPositionAndRotation(new Vector3(p.x, g.H(p) + 0.05f, p.y), Quaternion.Euler(R(-2f, 2f), sternYaw + R(-7f, 7f), R(-10f, 10f)));
                        ditchBoats++;
                    }
                    else if (pick < 0.62 && hauled < wantHauled && L > 70f)
                    {
                        float u = R(0.5f, 0.72f) * L;
                        Vector2 p = edge + a.outward * u;
                        var ub = UtrechtBoat(timber, linen, root, $"Utrecht boat hauled up between the dams {i}");
                        ub.transform.SetPositionAndRotation(new Vector3(p.x, g.H(p) + 0.1f, p.y), Quaternion.Euler(R(-4f, 2f), sternYaw + R(-5f, 5f), R(4f, 9f)));
                        hauled++;
                    }
                }
                // the dismantled hull: on the landing beach nearest Hoogstraat II, or in the space nearest it
                var beach = pcs.Where(p => p.empty).OrderBy(p => Mathf.Abs(p.s - HoogstraatTwo)).ToList();
                DorestadTown.Parcel site = beach.Count > 0 ? beach[0] : pcs.OrderBy(p => Mathf.Abs(p.s - HoogstraatTwo)).First();
                Vector2 hc = site.empty
                    ? site.bank + site.outward * (site.damLength * 0.55f) + site.along * R(-site.width * 0.5f + 4f, site.width * 0.5f - 4f)
                    : site.bank + site.outward * (site.damLength * 0.5f) + site.along * (site.width * 0.5f);
                var wreck = new GameObject("A hull being broken up on the beach (Hoogstraat II)").transform;
                wreck.SetParent(root, false);
                float wyaw = Mathf.Atan2(site.outward.x, site.outward.y) * Mathf.Rad2Deg + R(-25f, 25f);
                var keelHalf = Part(wreck, "Hull half, keel down", halfHull, timber, timber);
                keelHalf.transform.SetPositionAndRotation(new Vector3(hc.x, g.H(hc) + 0.05f, hc.y), Quaternion.Euler(R(-3f, 3f), wyaw, R(8f, 18f)));
                Vector2 hc2 = hc + site.along * R(3.5f, 5f) + site.outward * R(-2f, 2f);
                var overHalf = Part(wreck, "Hull half, turned over", halfHull, timber, timber);
                overHalf.transform.SetPositionAndRotation(new Vector3(hc2.x, g.H(hc2) + 0.7f, hc2.y), Quaternion.Euler(R(-3f, 3f), wyaw + R(-30f, 30f), 180f + R(-10f, 10f)));
                for (int k = rnd.Next(7, 11); k > 0; k--)
                {
                    Vector2 pp = hc + site.along * R(-6f, 8f) + site.outward * R(-5f, 5f);
                    var plank = Part(wreck, "Loose strake", plankMesh, timber);
                    plank.transform.SetPositionAndRotation(new Vector3(pp.x, g.H(pp) + 0.03f, pp.y), Quaternion.Euler(R(-4f, 4f), wyaw + R(-40f, 40f), R(-6f, 6f)));
                }
                for (int k = 3; k > 0; k--)
                {
                    Vector2 pp = hc + site.along * R(-5f, 7f) + site.outward * R(-4f, 4f);
                    var post = Part(wreck, "Post", mooring, piles, piles);
                    post.transform.SetPositionAndRotation(new Vector3(pp.x, g.H(pp) - 0.2f, pp.y), Quaternion.Euler(R(-8f, 8f), R(0, 360), R(-8f, 8f)));
                }
            }

            // boats drawn up on the mud and sand of the shores near the town, and mooring posts
            int boats = 0, posts = 0;
            for (float z = 0f; z < g.size; z += 9f)
                for (float x = 0f; x < g.size; x += 9f)
                {
                    var p = new Vector2(x + R(-3f, 3f), z + R(-3f, 3f));
                    byte cv = cover != null ? DorestadLandCover.Sample(cover, g, p) : (byte)0;
                    byte use = plan.Get(p);
                    bool shore = (cv == DorestadLandCover.Mud || cv == DorestadLandCover.Bar || use == (byte)DorestadTown.Use.Foreshore) && g.SD(p) < -2f && g.SD(p) > -16f;
                    if (!shore)
                        continue;
                    float dist = Vector2.Distance(p, townC);
                    float chance = dist < 500f ? 0.05f : dist < 1500f ? 0.012f : 0.003f;
                    if (rnd.NextDouble() < chance && boats < 24)
                    {
                        // the bow up the shore: find which way the water lies
                        Vector2 toWater = Vector2.zero;
                        for (float a = 0f; a < 360f; a += 45f)
                        {
                            Vector2 d = new Vector2(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad));
                            if (g.SD(p + d * 8f) > g.SD(p)) toWater += d;
                        }
                        toWater.Normalize();
                        var b = Part(root, "Logboat drawn up", logboat, timber, timber);
                        float yaw = Mathf.Atan2(toWater.x, toWater.y) * Mathf.Rad2Deg + 180f + R(-20f, 20f);
                        b.transform.SetPositionAndRotation(new Vector3(p.x, g.H(p) + 0.05f, p.y), Quaternion.Euler(R(-3f, 3f), yaw, R(-12f, 12f)));
                        boats++;
                    }
                    else if (rnd.NextDouble() < chance * 0.6f && posts < 40)
                    {
                        var m = Part(root, "Mooring post", mooring, piles, piles);
                        m.transform.SetPositionAndRotation(new Vector3(p.x, g.H(p) - 0.2f, p.y), Quaternion.Euler(R(-6f, 6f), R(0, 360), R(-6f, 6f)));
                        posts++;
                    }
                }
            Debug.Log($"Kapman river: {weirs} fish weirs in the gullies, {ditchBoats} logboats and {hauled} Utrecht boats in the harbour's dry spaces, a hull broken up on the beach, {boats} boats drawn up on the shores, {posts} mooring posts.");
        }

        /// <summary>Where a pool meets the reed within `maxDist` of the water: spots at least 60 m apart, nearest the river first.</summary>
        static List<Vector2> WeirSpots(DorestadBuilder.Grid g, byte[] cover, float maxDist)
        {
            var spots = new List<(Vector2 p, float d)>();
            int n = g.n;
            for (int z = 2; z < n - 2; z += 2)
                for (int x = 2; x < n - 2; x += 2)
                {
                    int i = z * n + x;
                    if (cover[i] != DorestadLandCover.Pool || g.sd[i] < -maxDist)
                        continue;
                    bool reed = cover[i + 1] == DorestadLandCover.Reed || cover[i - 1] == DorestadLandCover.Reed
                             || cover[i + n] == DorestadLandCover.Reed || cover[i - n] == DorestadLandCover.Reed;
                    if (!reed)
                        continue;
                    var p = new Vector2(x * g.cell, z * g.cell);
                    bool near = false;
                    foreach (var s in spots)
                        if (Vector2.Distance(s.p, p) < 60f) { near = true; break; }
                    if (!near)
                        spots.Add((p, -g.sd[i]));
                }
            return spots.OrderBy(s => s.d).Select(s => s.p).ToList();
        }

        static Mesh utrechtHull;

        /// <summary>
        /// A river trader of the Utrecht kind, as the Vleuten 1 (734 AD) and the Utrecht ship (c. 1000): an expanded
        /// logboat, the bottom hollowed from one oak, the sides raised with strakes on treenails, no iron; about 15 m by
        /// 1.5 m, a mast and a furled sail. A blockout from the finds' sizes; the sheer and the ends are guesses.
        /// </summary>
        public static GameObject UtrechtBoat(Material hull, Material sail, Transform parent, string name)
        {
            if (utrechtHull == null)
                utrechtHull = Save(Logboat("Utrecht_boat_hull", 15f, 1.8f), "Utrecht_boat_hull");
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var body = Part(go.transform, "Hull", utrechtHull, hull, hull);
            body.isStatic = false;
            body.transform.localPosition = new Vector3(0f, -0.25f, 0f);
            var mast = Part(go.transform, "Mast", Save(MeshKit.Cylinder("Utrecht_mast", 0.09f, 7f, 7, 1f), "Utrecht_mast"), hull, hull);
            mast.isStatic = false;
            mast.transform.localPosition = new Vector3(0f, 0.2f, -1f);
            var yard = Part(go.transform, "Furled sail", Save(MeshKit.Cylinder("Utrecht_sail_furled", 0.16f, 5.6f, 7, 1f), "Utrecht_sail_furled"), sail, sail);
            yard.isStatic = false;
            yard.transform.localPosition = new Vector3(-2.8f, 3.6f, -1f);
            yard.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
            go.isStatic = false;
            return go;
        }

        static Mesh Logboat(string name, float length, float beam)
        {
            // a hollowed trunk: an open half-shell, pointed at the bow
            var verts = new List<Vector3>();
            var tris = new List<int>();
            const int rings = 10, sides = 8;
            for (int r = 0; r <= rings; r++)
            {
                float t = r / (float)rings;
                float z = (t - 0.5f) * length;
                float taper = 1f - Mathf.Pow(Mathf.Abs(t - 0.45f) / 0.55f, 2.2f);
                float half = beam * 0.5f * Mathf.Max(0.12f, taper);
                for (int s = 0; s <= sides; s++)
                {
                    float a = Mathf.PI + Mathf.PI * s / sides;      // the lower half of a circle, gunwale to gunwale
                    verts.Add(new Vector3(Mathf.Cos(a) * half, Mathf.Sin(a) * half * 0.6f + half * 0.6f, z));
                }
            }
            for (int r = 0; r < rings; r++)
                for (int s = 0; s < sides; s++)
                {
                    int a = r * (sides + 1) + s, b = a + 1, c = a + sides + 1, d = c + 1;
                    tris.AddRange(new[] { a, b, c, b, d, c, a, c, b, b, c, d });   // both sides, so the hollow shows
                }
            var m = new Mesh { name = name };
            m.SetVertices(verts);
            m.SetTriangles(tris, 0);
            m.subMeshCount = 2;
            m.SetTriangles(tris, 1);
            m.RecalculateNormals();
            m.RecalculateBounds();
            var uv = new Vector2[verts.Count];
            for (int i = 0; i < uv.Length; i++)
                uv[i] = new Vector2(verts[i].z / 2f, (verts[i].x + verts[i].y) / 1.2f);
            m.uv = uv;
            return m;
        }

        static Mesh Save(Mesh m, string name)
        {
            string path = $"{Dir}/Meshes/{name}.asset";
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), $"{Dir}/Meshes"));
            var old = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (old != null)
                AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(m, path);
            return m;
        }

        static GameObject Part(Transform parent, string name, Mesh mesh, params Material[] mats)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.GetComponent<MeshRenderer>();
            var use = new Material[mesh.subMeshCount];
            for (int i = 0; i < use.Length; i++)
                use[i] = mats[Mathf.Min(i, mats.Length - 1)];
            r.sharedMaterials = use;
            go.isStatic = true;
            return go;
        }
    }
}
