// RUNS THE CREW'S DRAFT CODE ON THE PACK'S REAL SKELETON, outside Unity: reads a character prefab (text YAML) into the
// stand-in transforms of UnityShim.cs, seats him on a bench of the pack's ship with an oar on the rail, steps the stroke
// and (1) prints how well the hands, feet and blade do what they should, (2) draws the bones as lines, from the side,
// from astern and from above, into PNG sheets. Run:  dotnet run --project Design/ship_view/check  [out folder]
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using Kapman.Game.ShipView;
using UnityEngine;

static class RowCheck
{
    const string Pack = "G:/kapman_unity/Kapman2/Assets/Stylized_Medieval_Vikings_Pack/";

    static float F(string s) => float.Parse(s, CultureInfo.InvariantCulture);

    /// <summary>A prefab's transforms, with their names and local places, as the YAML has them.</summary>
    static Transform LoadPrefab(string path)
    {
        string txt = File.ReadAllText(path);
        var names = new Dictionary<string, string>();
        var made = new Dictionary<string, Transform>();
        var father = new Dictionary<string, string>();
        var order = new Dictionary<string, List<string>>();
        foreach (string doc in Regex.Split(txt, @"^--- ", RegexOptions.Multiline))
        {
            var head = Regex.Match(doc, @"^!u!(\d+) &(-?\d+)");
            if (!head.Success)
                continue;
            string cls = head.Groups[1].Value, id = head.Groups[2].Value;
            if (cls == "1")
                names[id] = Regex.Match(doc, @"m_Name: (.*)").Groups[1].Value.Trim();
            else if (cls == "4")
            {
                var t = new GameObject("(stripped)").transform;
                var go = Regex.Match(doc, @"m_GameObject: \{fileID: (-?\d+)\}");
                t.name = go.Success ? "#" + go.Groups[1].Value : "(stripped)";
                var r = Regex.Match(doc, @"m_LocalRotation: \{x: ([-\d.e]+), y: ([-\d.e]+), z: ([-\d.e]+), w: ([-\d.e]+)\}");
                var p = Regex.Match(doc, @"m_LocalPosition: \{x: ([-\d.e]+), y: ([-\d.e]+), z: ([-\d.e]+)\}");
                var s = Regex.Match(doc, @"m_LocalScale: \{x: ([-\d.e]+), y: ([-\d.e]+), z: ([-\d.e]+)\}");
                if (r.Success)
                    t.localRotation = new Quaternion(F(r.Groups[1].Value), F(r.Groups[2].Value), F(r.Groups[3].Value), F(r.Groups[4].Value));
                if (p.Success)
                    t.localPosition = new Vector3(F(p.Groups[1].Value), F(p.Groups[2].Value), F(p.Groups[3].Value));
                if (s.Success)
                    t.localScale = new Vector3(F(s.Groups[1].Value), F(s.Groups[2].Value), F(s.Groups[3].Value));
                var f = Regex.Match(doc, @"m_Father: \{fileID: (-?\d+)\}");
                father[id] = f.Success ? f.Groups[1].Value : "0";
                var kids = new List<string>();
                var block = Regex.Match(doc, @"m_Children:(.*?)m_Father", RegexOptions.Singleline);
                if (block.Success)
                    foreach (Match k in Regex.Matches(block.Groups[1].Value, @"fileID: (-?\d+)"))
                        kids.Add(k.Groups[1].Value);
                order[id] = kids;
                made[id] = t;
            }
        }
        Transform root = null;
        foreach (var kv in made)
        {
            var t = kv.Value;
            if (t.name.StartsWith("#") && names.TryGetValue(t.name.Substring(1), out string n))
                t.name = n;
        }
        // children in the prefab's own order
        foreach (var kv in order)
            foreach (string kid in kv.Value)
                if (made.TryGetValue(kid, out var c))
                    c.SetParent(made[kv.Key], false);
        foreach (var kv in made)
            if (kv.Value.parent == null && kv.Value.name.StartsWith("SMV_"))
                root = kv.Value;
        return root;
    }

    static Transform Child(Transform parent, string name, Vector3 at, Quaternion turn)
    {
        var t = new GameObject(name).transform;
        t.SetParent(parent, false);
        t.localPosition = at;
        t.localRotation = turn;
        return t;
    }

    // ------------------------------------------------------------------ a small picture

    sealed class Sheet
    {
        public readonly int w, h;
        readonly byte[] px;
        public Sheet(int w, int h) { this.w = w; this.h = h; px = new byte[w * h * 3]; for (int i = 0; i < px.Length; i++) px[i] = 244; }
        public void Dot(int x, int y, int rgb, int r = 0)
        {
            for (int dy = -r; dy <= r; dy++)
                for (int dx = -r; dx <= r; dx++)
                {
                    int X = x + dx, Y = y + dy;
                    if (X < 0 || Y < 0 || X >= w || Y >= h)
                        continue;
                    int i = (Y * w + X) * 3;
                    px[i] = (byte)(rgb >> 16); px[i + 1] = (byte)(rgb >> 8); px[i + 2] = (byte)rgb;
                }
        }
        public void Line(float x0, float y0, float x1, float y1, int rgb, int r = 0)
        {
            int n = (int)Math.Max(Math.Abs(x1 - x0), Math.Abs(y1 - y0)) + 1;
            for (int i = 0; i <= n; i++)
                Dot((int)MathF.Round(x0 + (x1 - x0) * i / n), (int)MathF.Round(y0 + (y1 - y0) * i / n), rgb, r);
        }
        public void Save(string path)
        {
            using var raw = new MemoryStream();
            for (int y = 0; y < h; y++) { raw.WriteByte(0); raw.Write(px, y * w * 3, w * 3); }
            using var zipped = new MemoryStream();
            using (var z = new ZLibStream(zipped, CompressionLevel.Optimal, true))
                z.Write(raw.GetBuffer(), 0, (int)raw.Length);
            using var f = File.Create(path);
            f.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
            var ihdr = new byte[13];
            Be(ihdr, 0, w); Be(ihdr, 4, h); ihdr[8] = 8; ihdr[9] = 2;
            Chunk(f, "IHDR", ihdr, 13);
            Chunk(f, "IDAT", zipped.GetBuffer(), (int)zipped.Length);
            Chunk(f, "IEND", new byte[0], 0);
        }
        static void Be(byte[] b, int at, int v) { b[at] = (byte)(v >> 24); b[at + 1] = (byte)(v >> 16); b[at + 2] = (byte)(v >> 8); b[at + 3] = (byte)v; }
        static readonly uint[] Table = MakeTable();
        static uint[] MakeTable()
        {
            var t = new uint[256];
            for (uint n = 0; n < 256; n++) { uint c = n; for (int k = 0; k < 8; k++) c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1; t[n] = c; }
            return t;
        }
        static void Chunk(Stream f, string type, byte[] data, int len)
        {
            var l = new byte[4]; Be(l, 0, len); f.Write(l);
            var ty = System.Text.Encoding.ASCII.GetBytes(type); f.Write(ty);
            f.Write(data, 0, len);
            uint c = 0xFFFFFFFFu;
            foreach (byte b in ty) c = Table[(c ^ b) & 0xFF] ^ (c >> 8);
            for (int i = 0; i < len; i++) c = Table[(c ^ data[i]) & 0xFF] ^ (c >> 8);
            var cb = new byte[4]; Be(cb, 0, (int)(c ^ 0xFFFFFFFFu)); f.Write(cb);
        }
    }

    enum View { Side, Astern, Above }

    // a world point to a cell of the sheet: 100 px a metre
    static (float, float) Map(Vector3 p, View v, int cx, int cy, Vector3 centre)
    {
        const float S = 100f;
        Vector3 d = p - centre;
        switch (v)
        {
            case View.Side: return (cx + d.z * S, cy - d.y * S);        // from starboard: the bow to the right
            case View.Astern: return (cx + d.x * S, cy - d.y * S);      // from astern: starboard to the right
            default: return (cx + d.x * S, cy - d.z * S);               // from above: the bow up, starboard to the right
        }
    }

    static int Colour(string name)
    {
        if (name.Contains("arm.L") || name.Contains("hand.L") || name == "shoulder.L") return 0xD03030;
        if (name.Contains("arm.R") || name.Contains("hand.R") || name == "shoulder.R") return 0x2050D0;
        if (name.EndsWith(".L")) return 0x209040;
        if (name.EndsWith(".R")) return 0x207080;
        return 0x202020;
    }

    static void DrawBones(Sheet s, Transform t, View v, int cx, int cy, Vector3 centre)
    {
        for (int i = 0; i < t.childCount; i++)
        {
            var c = t.GetChild(i);
            if (c.name == "(stripped)" || c.name.Contains("forearm.L.0") || c.name.Contains("forearm.R.0"))
                continue;
            var (x0, y0) = Map(t.position, v, cx, cy, centre);
            var (x1, y1) = Map(c.position, v, cx, cy, centre);
            s.Line(x0, y0, x1, y1, Colour(c.name), 1);
            DrawBones(s, c, v, cx, cy, centre);
        }
    }

    static void DrawScene(Sheet s, View v, int cx, int cy, Vector3 centre, RowingRig rig, float water, int only = -1)
    {
        int index = -1;
        // the water, the bench top, the rail
        var (wx0, wy) = Map(centre + new Vector3(-9, 0, -9), v, cx, cy, centre);
        if (v != View.Above)
        {
            var (_, yw) = Map(new Vector3(centre.x, water, centre.z), v, cx, cy, centre);
            s.Line(cx - 175, yw, cx + 175, yw, 0x70A0D0);
        }
        foreach (var o in rig.oars)
        {
            index++;
            if (!o.pivot.gameObject.activeSelf || (only >= 0 && index != only))
                continue;
            Vector3 tip = o.pivot.TransformPoint(new Vector3(o.bladeOut + 0.7f, 0, 0)), grip = o.pivot.TransformPoint(new Vector3(-o.gripInner - 0.08f, 0, 0));
            var (a0, b0) = Map(tip, v, cx, cy, centre);
            var (a1, b1) = Map(grip, v, cx, cy, centre);
            s.Line(a0, b0, a1, b1, 0x8A5A20, 1);
            var (bx, by) = Map(o.pivot.TransformPoint(new Vector3(o.bladeOut, 0, 0)), v, cx, cy, centre);
            s.Dot((int)bx, (int)by, 0xB07020, 3);
            var (tx, ty) = Map(o.thole.position, v, cx, cy, centre);
            s.Dot((int)tx, (int)ty, 0x000000, 2);
        }
        index = -1;
        foreach (var r in rig.rowers)
        {
            index++;
            if (only >= 0 && index != only)
                continue;
            var (sx, sy) = Map(r.seat.position, v, cx, cy, centre);
            s.Line(sx - 22, sy, sx + 22, sy, 0x907050, 1);
            DrawBones(s, r.body.hips, v, cx, cy, centre);
            var (hx, hy) = Map(r.body.head.position + r.body.head.up * 0.09f, v, cx, cy, centre);
            s.Dot((int)hx, (int)hy, 0x202020, 4);
        }
    }

    static int Main(string[] args)
    {
        string outDir = args.Length > 0 ? args[0] : ".";
        Directory.CreateDirectory(outDir);
        int bad = 0;

        // ---------------------------------------------------------------- the ship, a bench, a thole, an oar, a man
        var shipGo = new GameObject("Ship");
        var ship = shipGo.transform;
        var rig = shipGo.AddComponent<RowingRig>();
        var oars = new List<RowingRig.Oar>();
        var rowers = new List<RowingRig.Rower>();
        // the pack's starboard bench "Ship_Bench (3)": its plank is centred at (1.702, 1.709, -0.408), top 0.036 above;
        // and the port bench "Ship_Bench (3)" at (-1.711, 1.709, -0.028). The rail's top amidships is at y 2.10.
        // (each: the plank's middle x, its top y, its z; then the rail's top x and y at the thole's station, from the tool's table)
        float[][] benches = { new[] { 1.702f, 1.745f, -0.408f, 2.430f, 2.104f }, new[] { -1.711f, 1.745f, -0.028f, 2.457f, 2.097f } };
        string[] men = { "SMV_Viking_Settler_1", "SMV_Viking_Karl_1" };
        for (int i = 0; i < benches.Length; i++)
        {
            float side = benches[i][0] > 0 ? 1f : -1f;
            // the seating tool's rule (draft/Editor/ShipViewSeating.cs): the thole on the rail's top 0.30 m nearer the
            // stern than the bench's middle, a hand's width inside the rail; the seat's middle 0.56 m inboard of it
            float tholeZ = benches[i][2] - 0.30f;
            float railX = benches[i][3], railY = benches[i][4];
            var seat = Child(ship, "Seat " + i, new Vector3(side * (railX - 0.03f - 0.56f), benches[i][1], benches[i][2]), Quaternion.AngleAxis(180f, Vector3.up));
            var thole = Child(ship, "Thole " + i, new Vector3(side * (railX - 0.03f), railY + 0.02f, tholeZ),
                              side > 0 ? Quaternion.identity : Quaternion.AngleAxis(180f, Vector3.up));
            var pivot = Child(thole, "Pivot", Vector3.zero, Quaternion.identity);
            var shaft = Child(pivot, "Oar_2", Vector3.zero, Quaternion.identity);
            oars.Add(new RowingRig.Oar { thole = thole, pivot = pivot, shaft = shaft, side = side, bladeOut = 3.12f, gripOuter = 0.40f, gripInner = 0.68f, shaftShift = 0.346f });
            var man = LoadPrefab(Pack + "Prefabs - Characters/" + men[i] + ".prefab");
            if (man == null) { Console.WriteLine("could not read " + men[i]); return 2; }
            man.SetParent(ship, false);
            rowers.Add(new RowingRig.Rower { man = man, seat = seat, oar = i, seed = 1.3f + i * 2.1f });
        }
        rig.oars = oars.ToArray();
        rig.rowers = rowers.ToArray();
        rig.Build();
        var b0 = rig.rowers[0].body;
        Console.WriteLine($"skeleton: valid {b0.Valid}, hips {b0.hipHeight:0.000} m up, arm reach {b0.armReach:0.000} m, " +
                          $"thigh {(b0.shinL.position - b0.thighL.position).magnitude:0.000}, shin {(b0.footL.position - b0.shinL.position).magnitude:0.000}, " +
                          $"shoulders {(b0.upperL.position - b0.upperR.position).magnitude:0.000} apart, faces +z: toe z {FindDeep(b0.root, "toe.L_end").position.z - b0.root.position.z:0.00}, left at x {b0.upperL.position.x - b0.root.position.x:0.00}");
        if (!b0.Valid) return 2;

        // ---------------------------------------------------------------- the stroke, level water, upright ship
        float water = ship.TransformPoint(new Vector3(0, rig.waterline, 0)).y;
        Console.WriteLine("\nphase  beat      sweep  dip  lean | blade over water | inner wrist to grip, outer | elbows below shoulders L R | ankles off L R | head up, fwd");
        float worstGrip = 0f, worstAnkle = 0f;
        for (int k = 0; k < 20; k++)
        {
            float p = k / 20f;
            rig.Phase = p; rig.effort = 1f; rig.oarsOut = 1f;
            rig.Pose(1f);
            var st = Stroke.At(p);
            string beat = p < Stroke.Drive ? "drive" : p < Stroke.Drive + Stroke.Finish ? "finish" : p < 1f - Stroke.Catch ? "recovery" : "catch";
            for (int i = 0; i < 1; i++)
            {
                var o = rig.oars[i]; var r = rig.rowers[i]; var b = r.body;
                Vector3 blade = o.pivot.TransformPoint(new Vector3(o.bladeOut, 0, 0));
                Vector3 gi = o.pivot.TransformPoint(new Vector3(-o.gripInner, 0, 0)), go = o.pivot.TransformPoint(new Vector3(-o.gripOuter, 0, 0));
                bool leftInner = (b.Shoulder(true) - o.thole.position).sqrMagnitude > (b.Shoulder(false) - o.thole.position).sqrMagnitude;
                float dIn = ((leftInner ? b.handL : b.handR).position - gi).magnitude, dOut = ((leftInner ? b.handR : b.handL).position - go).magnitude;
                float eL = b.Shoulder(true).y - b.foreL.position.y, eR = b.Shoulder(false).y - b.foreR.position.y;
                Vector3 aL = r.seat.TransformPoint(new Vector3(-rig.footApart, -rig.footDown, rig.footAhead)), aR = r.seat.TransformPoint(new Vector3(rig.footApart, -rig.footDown, rig.footAhead));
                float fL = (b.footL.position - aL).magnitude, fR = (b.footR.position - aR).magnitude;
                Vector3 hd = r.seat.InverseTransformPoint(b.head.position);
                Console.WriteLine($"{p:0.00}   {beat,-8}  {st.sweep:0.00}  {st.dip:0.00} {st.lean:0.00} | {blade.y - water,6:0.00} m         | {dIn:0.000}  {dOut:0.000}            | {eL,5:0.00} {eR,5:0.00}             | {fL:0.000} {fR:0.000}   | {hd.y:0.00} {hd.z:0.00}");
                worstGrip = Mathf.Max(worstGrip, Mathf.Max(Mathf.Abs(dIn - rig.palm), Mathf.Abs(dOut - rig.palm)));
                worstAnkle = Mathf.Max(worstAnkle, Mathf.Max(fL, fR));
                bool inWater = blade.y < water, wantIn = st.dip > 0.9f, wantOut = st.dip < 0.1f;
                if ((wantIn && !inWater) || (wantOut && inWater)) { Console.WriteLine("   ! the blade is on the wrong side of the water"); bad++; }
                if (float.IsNaN(b.handL.position.x) || float.IsNaN(b.head.position.x)) { Console.WriteLine("   ! not a number"); bad++; }
            }
        }
        Console.WriteLine($"\nworst miss of a hand from its grip {worstGrip * 100f:0.0} cm (over 3 cm: the oar is out of his reach there); worst ankle miss {worstAnkle * 100f:0.0} cm");
        if (worstGrip > 0.03f) bad++;
        if (worstAnkle > 0.03f) bad++;

        // the pictures: eight moments of the stroke for the starboard man, from the side, from astern, from above
        foreach (View v in new[] { View.Side, View.Astern, View.Above })
        {
            var sheet = new Sheet(4 * 400, 2 * 330);
            for (int k = 0; k < 8; k++)
            {
                rig.Phase = k / 8f;
                rig.Pose(1f);
                Vector3 centre = rig.rowers[0].seat.position + new Vector3(v == View.Side ? 0f : 0.9f, v == View.Above ? 0f : 0.25f, -0.25f);
                int cx = (k % 4) * 400 + 200, cy = (k / 4) * 330 + 165;
                DrawScene(sheet, v, cx, cy, centre, rig, water, 0);
                for (int y = 0; y < 660; y++) sheet.Dot((k % 4) * 400, y, 0xC0C0C0);
                for (int x = 0; x < 1600; x++) sheet.Dot(x, 330, 0xC0C0C0);
                for (int n = 0; n <= k; n++) sheet.Dot(cx - 190 + n * 8, cy - 155, 0x000000, 2);     // (the moment's number, as dots)
            }
            sheet.Save(Path.Combine(outDir, "stroke_" + v.ToString().ToLowerInvariant() + ".png"));
        }
        // the port man, to see that he mirrors: four moments from astern and from above
        var port = new Sheet(4 * 400, 2 * 330);
        for (int k = 0; k < 4; k++)
        {
            rig.Phase = k / 4f;
            rig.Pose(1f);
            Vector3 centre = rig.rowers[1].seat.position + new Vector3(-0.9f, 0.25f, -0.25f);
            DrawScene(port, View.Astern, k * 400 + 200, 165, centre, rig, water, 1);
            DrawScene(port, View.Above, k * 400 + 200, 495, centre - new Vector3(0, 0.25f, 0), rig, water, 1);
        }
        port.Save(Path.Combine(outDir, "port.png"));

        // ---------------------------------------------------------------- resting on the oars, and the oars away
        var rest = new Sheet(4 * 360, 2 * 300);
        float[][] states = { new[] { 0f, 1f }, new[] { 0f, 0.6f }, new[] { 0f, 0f }, new[] { 0.5f, 1f } };
        for (int k = 0; k < states.Length; k++)
        {
            rig.effort = states[k][0]; rig.oarsOut = states[k][1]; rig.Phase = 0.2f;
            rig.Pose(2f);
            Vector3 centre = ship.TransformPoint(new Vector3(0f, 2.0f, -0.4f));
            DrawScene(rest, View.Side, k * 360 + 180, 150, rig.rowers[0].seat.position + new Vector3(0, 0.25f, -0.25f), rig, water, 0);
            DrawScene(rest, View.Astern, k * 360 + 180, 450, rig.rowers[0].seat.position + new Vector3(0.9f, 0.25f, -0.25f), rig, water, 0);
            var b = rig.rowers[0].body;
            Console.WriteLine($"effort {states[k][0]:0.0} oars out {states[k][1]:0.0}: hands at {rig.rowers[0].seat.InverseTransformPoint(b.handL.position)} and {rig.rowers[0].seat.InverseTransformPoint(b.handR.position)} from the seat; oar shown {rig.oars[0].pivot.gameObject.activeSelf}");
        }
        rest.Save(Path.Combine(outDir, "rest.png"));

        // ---------------------------------------------------------------- the ship heeled and pitched: nothing may change in her own frame
        rig.effort = 1f; rig.oarsOut = 1f; rig.Phase = 0.25f;
        rig.waterHeight = null;
        rig.Pose(1f);
        Vector3 handBefore = ship.InverseTransformPoint(rig.rowers[1].body.handL.position), headBefore = ship.InverseTransformPoint(rig.rowers[1].body.head.position);
        ship.rotation = Quaternion.Euler(0f, 35f, 0f);
        ship.position = new Vector3(120f, 0.4f, -60f);
        rig.Pose(1f);
        Vector3 handAfter = ship.InverseTransformPoint(rig.rowers[1].body.handL.position), headAfter = ship.InverseTransformPoint(rig.rowers[1].body.head.position);
        float moved = Mathf.Max((handAfter - handBefore).magnitude, (headAfter - headBefore).magnitude);
        Console.WriteLine($"\nship turned 35 degrees and moved 134 m: the port man's hand and head move {moved * 100f:0.00} cm in her frame (must be 0)");
        if (moved > 0.002f) bad++;
        ship.rotation = Quaternion.Euler(4f, 35f, 9f);
        rig.Pose(1f);
        handAfter = ship.InverseTransformPoint(rig.rowers[1].body.handL.position);
        Console.WriteLine($"ship heeled 9 and pitched 4 degrees as well: his hand moves {(handAfter - handBefore).magnitude * 100f:0.0} cm in her frame (the oar dips to the level water, and his hands with it)");
        ship.rotation = Quaternion.identity;
        ship.position = Vector3.zero;

        // ---------------------------------------------------------------- the rule: when she rows and when she sails
        Console.WriteLine("\nthe rule:");
        (string, CrewInputs, CrewMode)[] cases =
        {
            ("at a berth", new CrewInputs { moored = true, wind = 6, sailKnots = 4, oarKnots = 2.5f, closestToWind = 58 }, CrewMode.Moored),
            ("leaving the berth, a fair wind", new CrewInputs { nearPort = true, wind = 6, windOffBow = 150, sailKnots = 4, oarKnots = 2.5f, closestToWind = 58 }, CrewMode.Rowing),
            ("at sea, a fair wind", new CrewInputs { wind = 6, windOffBow = 120, sailKnots = 4.2f, oarKnots = 2.5f, closestToWind = 58 }, CrewMode.Sailing),
            ("at sea, a calm", new CrewInputs { wind = 1.0f, windOffBow = 90, sailKnots = 0, oarKnots = 2.5f, closestToWind = 58 }, CrewMode.Rowing),
            ("at sea, beating, light air", new CrewInputs { wind = 2.5f, windOffBow = 20, sailKnots = 1.1f, oarKnots = 2.5f, closestToWind = 58 }, CrewMode.Rowing),
            ("at sea, beating, a breeze", new CrewInputs { wind = 9f, windOffBow = 20, sailKnots = 2.9f, oarKnots = 2.5f, closestToWind = 58 }, CrewMode.Sailing),
            ("on a river, wind ahead", new CrewInputs { onRiver = true, wind = 9f, windOffBow = -30, sailKnots = 2.9f, oarKnots = 2.5f, closestToWind = 58 }, CrewMode.Rowing),
            ("on a river, wind astern", new CrewInputs { onRiver = true, wind = 6f, windOffBow = 170, sailKnots = 4f, oarKnots = 2.5f, closestToWind = 58 }, CrewMode.Sailing),
            ("a gale", new CrewInputs { lyingTo = true, wind = 19f, windOffBow = 40, sailKnots = 0, oarKnots = 2.5f, closestToWind = 58 }, CrewMode.LyingTo),
        };
        foreach (var (what, input, want) in cases)
        {
            var got = ShipCrew.Decide(input);
            Console.WriteLine($"  {what,-34} -> {got}{(got == want ? "" : "   ! expected " + want)}");
            if (got != want) bad++;
        }

        // ---------------------------------------------------------------- the crew over a minute: moored, away under oars, the sail set
        var crew = shipGo.AddComponent<ShipCrew>();
        crew.rig = rig;
        crew.sailSet = new GameObject("Ship_Sail_Open");
        crew.sailFurled = new GameObject("Ship_Sail_Close");
        rig.effort = 0f; rig.oarsOut = 0f;
        Console.WriteLine("\na minute of it (every 4 s):  t  mode  oars out  effort  strokes/min  sail set");
        float tt = 0f;
        for (int step = 0; step < 60 * 30; step++)
        {
            tt += 1f / 30f;
            crew.inputs = tt < 6f ? new CrewInputs { moored = true, wind = 6, sailKnots = 4, oarKnots = 2.5f, closestToWind = 58 }
                        : tt < 30f ? new CrewInputs { nearPort = true, knots = 2, wind = 6, windOffBow = 140, sailKnots = 4, oarKnots = 2.5f, closestToWind = 58 }
                        : new CrewInputs { knots = 4, wind = 6, windOffBow = 140, sailKnots = 4, oarKnots = 2.5f, closestToWind = 58 };
            crew.Step(1f / 30f, tt);
            rig.Step(1f / 30f, tt);
            if (step % 120 == 119)
                Console.WriteLine($"  {tt,5:0}  {crew.Mode,-8} {rig.oarsOut:0.00}  {rig.effort:0.00}  {rig.strokesPerMinute:0.0}  {crew.sailSet.activeSelf}");
        }
        // ---------------------------------------------------------------- the helmsman at the steering oar
        // the pack's ShipSteerOar_1 hangs at (1.19, 3.42, -6.35) in the ship, turned (0.041, 0.819, 0.175, 0.545); the
        // after deck is at y 1.87. The seating tool's rule: he stands at (0.78, 1.87, -6.75) facing the bow, and holds
        // the shaft 0.32 m below the oar's own origin, on the side nearest him.
        var steer = Child(ship, "ShipSteerOar_1", new Vector3(1.19f, 3.42f, -6.35f), new Quaternion(0.041f, 0.819f, 0.175f, 0.545f));
        var stand = Child(ship, "Helm stand", new Vector3(0.78f, 1.87f, -6.75f), Quaternion.identity);
        Vector3 onShaft = steer.TransformPoint(new Vector3(0f, -0.32f, 0f));
        Vector3 toHim = stand.position + new Vector3(0f, 1.3f, 0f) - onShaft;
        var grip = Child(steer, "Helm grip", steer.InverseTransformPoint(onShaft + toHim.normalized * 0.11f), Quaternion.identity);
        var helmMan = LoadPrefab(Pack + "Prefabs - Characters/SMV_Viking_Settler_2.prefab");
        helmMan.SetParent(ship, false);
        crew.helmsman = helmMan; crew.helmStand = stand; crew.helmGrip = grip; crew.steerOar = steer;
        crew.Build();
        crew.inputs = new CrewInputs { knots = 4, wind = 6, windOffBow = 140, sailKnots = 4, oarKnots = 2.5f, closestToWind = 58, turning = 10f };
        for (int n = 0; n < 60; n++) { tt += 1f / 30f; crew.Step(1f / 30f, tt); }
        var hb = new CrewBody(helmMan);
        float helmMiss = (hb.handR.position - grip.position).magnitude;
        Console.WriteLine($"\nthe helmsman: the steering oar's shaft runs {steer.up} in the ship, its grip is at {grip.position}; his right wrist is {helmMiss * 100f:0.0} cm from it; his head is at {hb.head.position}");
        if (helmMiss > 0.03f) { Console.WriteLine("   ! the steering oar is out of his reach"); bad++; }
        var hs = new Sheet(2 * 400, 330);
        Vector3 hc = stand.position + new Vector3(0.1f, 1.0f, 0f);
        DrawBones(hs, hb.hips, View.Side, 200, 165, hc);
        DrawBones(hs, hb.hips, View.Astern, 600, 165, hc);
        foreach (var (v, cx) in new[] { (View.Side, 200), (View.Astern, 600) })
        {
            var (x0, y0) = Map(steer.TransformPoint(new Vector3(0, 0.98f, 0)), v, cx, 165, hc);
            var (x1, y1) = Map(steer.TransformPoint(new Vector3(0, -2.2f, 0)), v, cx, 165, hc);
            hs.Line(x0, y0, x1, y1, 0x8A5A20, 2);
            var (dx, dy) = Map(stand.position, v, cx, 165, hc);
            hs.Line(dx - 60, dy, dx + 60, dy, 0x907050, 1);
            var (gx, gy) = Map(grip.position, v, cx, 165, hc);
            hs.Dot((int)gx, (int)gy, 0xD000D0, 3);
        }
        hs.Save(Path.Combine(outDir, "helm.png"));

        // ---------------------------------------------------------------- the small boat: the stage's fit-out (StageShip.Fit) for the 7.7 m boat
        // The hull at 0.431, her boards 0.108 m over the water, a thwart's top (the seat) at 0.208, the gunwale at 0.384,
        // its half-breadth 1.0 m, the tholes 0.3 m aft of the thwart and 0.03 over the gunwale. One man in the middle of
        // the thwart pulls a pair, a hand to each (draft/ShipViewJoin.cs: PackCrewRig seats him so); the helmsman sits.
        {
            var boatGo = new GameObject("Boat");
            var boat = boatGo.transform;
            var brig = boatGo.AddComponent<RowingRig>();
            float deck = 0.108f, seatY = 0.208f, gunwale = 0.384f, halfBeam = 1.0f, z = 1.309f;
            var seat = Child(boat, "Seat", new Vector3(0f, seatY, z), Quaternion.AngleAxis(180f, Vector3.up));
            var pair = new List<RowingRig.Oar>();
            foreach (float side in new[] { -1f, 1f })
            {
                var thole = Child(boat, "Thole", new Vector3(side * halfBeam, gunwale + 0.03f, z - 0.3f), side > 0 ? Quaternion.identity : Quaternion.AngleAxis(180f, Vector3.up));
                var pivot = Child(thole, "Pivot", Vector3.zero, Quaternion.identity);
                var shaft = Child(pivot, "Oar_1", Vector3.zero, Quaternion.identity);
                // the pack's Oar_1 (0.47 m inboard, the blade's middle 1.70 m out) stretched by half and slid inboard
                float reachIn = halfBeam - 0.22f, shift = 0.47f * 1.5f - (reachIn + 0.08f);
                pair.Add(new RowingRig.Oar { thole = thole, pivot = pivot, shaft = shaft, side = side, bladeOut = 1.70f * 1.5f + shift, shaftShift = shift, gripInner = reachIn, gripOuter = reachIn - 0.28f });
            }
            var sculler = LoadPrefab(Pack + "Prefabs - Characters/SMV_Viking_Settler_3.prefab");
            sculler.SetParent(boat, false);
            brig.oars = pair.ToArray();
            brig.rowers = new[] { new RowingRig.Rower { man = sculler, seat = seat, oar = 0, oar2 = 1, seed = 4.4f } };
            brig.waterline = 0f;
            brig.footDown = (seatY - deck) - 0.126f;
            brig.footAhead = 0.62f;
            brig.bladeDepth = 0.10f;
            brig.bladeClear = 0.22f;
            brig.catchAngle = 20f;                                           // (a pair: the handles are longer inboard, so less sweep)
            brig.finishAngle = 22f;
            brig.reach = 24f;
            brig.Build();
            var sb = brig.rowers[0].body;
            float worst = 0f, worstFoot = 0f;
            bool bladeBad = false;
            Console.WriteLine("\nthe small boat, one man pulling a pair:  phase | blades over the water port, starboard | wrists to grips left, right | hands over the seat | knees over the seat");
            for (int k = 0; k < 10; k++)
            {
                brig.Phase = k / 10f; brig.effort = 1f; brig.oarsOut = 1f;
                brig.Pose(1f);
                var st = Stroke.At(brig.Phase);
                float o0 = brig.oars[0].pivot.TransformPoint(new Vector3(brig.oars[0].bladeOut, 0, 0)).y, o1 = brig.oars[1].pivot.TransformPoint(new Vector3(brig.oars[1].bladeOut, 0, 0)).y;
                // he faces the stern, so the oar on his left is the starboard one
                Vector3 gl = brig.oars[1].pivot.TransformPoint(new Vector3(-brig.oars[1].gripInner, 0, 0)), gr = brig.oars[0].pivot.TransformPoint(new Vector3(-brig.oars[0].gripInner, 0, 0));
                float dl = (sb.handL.position - gl).magnitude, dr = (sb.handR.position - gr).magnitude;
                worst = Mathf.Max(worst, Mathf.Max(Mathf.Abs(dl - brig.palm), Mathf.Abs(dr - brig.palm)));
                Vector3 ankle = seat.TransformPoint(new Vector3(-brig.footApart, -brig.footDown, brig.footAhead));
                worstFoot = Mathf.Max(worstFoot, (sb.footL.position - ankle).magnitude);
                if ((st.dip > 0.9f && (o0 > 0 || o1 > 0)) || (st.dip < 0.1f && (o0 < 0 || o1 < 0))) bladeBad = true;
                Console.WriteLine($"  {brig.Phase:0.0} | {o0,6:0.00} {o1,6:0.00} | {dl:0.000} {dr:0.000} | {seat.InverseTransformPoint(sb.handL.position).y:0.00} | {seat.InverseTransformPoint(sb.Knee(true)).y:0.00}");
            }
            Console.WriteLine($"  worst miss of a hand {worst * 100f:0.0} cm, of an ankle {worstFoot * 100f:0.0} cm{(bladeBad ? "; ! a blade on the wrong side of the water" : "")}");
            if (worst > 0.03f || worstFoot > 0.03f || bladeBad) bad++;

            var small = new Sheet(5 * 360, 2 * 300);
            for (int k = 0; k < 4; k++)
            {
                brig.Phase = k / 4f;
                brig.Pose(1f);
                DrawScene(small, View.Side, k * 360 + 180, 150, seat.position + new Vector3(0, 0.45f, -0.3f), brig, 0f, 0);
                DrawScene(small, View.Astern, k * 360 + 180, 450, seat.position + new Vector3(0, 0.45f, -0.3f), brig, 0f, -1);
            }

            // the helmsman seated aft, his hand on the steering oar (scaled with the hull)
            var steer2 = Child(boat, "ShipSteerOar_1", new Vector3(0.513f, 0.957f, -2.737f), new Quaternion(0.041f, 0.819f, 0.175f, 0.545f));
            steer2.localScale = new Vector3(0.431f, 0.431f, 0.431f);
            var helmSeat = Child(boat, "Helm seat", new Vector3(0.25f, seatY, -2.618f), Quaternion.identity);
            Vector3 axis = steer2.up, at = steer2.position;
            Vector3 onShaft2 = at + axis * ((helmSeat.position.y + 0.4f - at.y) / axis.y);
            Vector3 toHim2 = helmSeat.position + Vector3.up * 0.5f - onShaft2;
            var grip2 = Child(boat, "Helm grip", onShaft2 + toHim2.normalized * (0.11f * 0.431f), Quaternion.identity);
            var crew2 = boatGo.AddComponent<ShipCrew>();
            crew2.rig = brig;
            crew2.helmsman = LoadPrefab(Pack + "Prefabs - Characters/SMV_Viking_Settler_4.prefab");
            crew2.helmsman.SetParent(boat, false);
            crew2.helmStand = helmSeat;
            crew2.helmSits = true;
            crew2.helmGrip = grip2;
            crew2.Build();
            crew2.inputs = new CrewInputs { knots = 4, wind = 6, windOffBow = 140, sailKnots = 4, oarKnots = 2.5f, closestToWind = 58, turning = 5f };
            float t2 = 100f;
            for (int n = 0; n < 400; n++) { t2 += 1f / 30f; crew2.Step(1f / 30f, t2); brig.Step(1f / 30f, t2); }
            var hb2 = new CrewBody(crew2.helmsman);
            float miss2 = (hb2.handR.position - grip2.position).magnitude;
            Console.WriteLine($"  the helmsman sits at {helmSeat.position}, his hips at {hb2.hips.position}; the grip on the steering oar is at {grip2.position}; his right wrist is {miss2 * 100f:0.0} cm from it; mode {crew2.Mode}; the sculler's hands are back on his thighs: {seat.InverseTransformPoint(sb.handL.position)}");
            if (miss2 > 0.03f) { Console.WriteLine("   ! the steering oar is out of his reach"); bad++; }
            Vector3 hc2 = helmSeat.position + new Vector3(0.1f, 0.45f, 0f);
            foreach (var (v, cx) in new[] { (View.Side, 4 * 360 + 180), (View.Astern, 4 * 360 + 180) })
            {
                int cy = v == View.Side ? 150 : 450;
                DrawBones(small, hb2.hips, v, cx, cy, hc2);
                var (x0, y0) = Map(steer2.TransformPoint(new Vector3(0, 0.98f, 0)), v, cx, cy, hc2);
                var (x1, y1) = Map(steer2.TransformPoint(new Vector3(0, -3.2f, 0)), v, cx, cy, hc2);
                small.Line(x0, y0, x1, y1, 0x8A5A20, 1);
                var (sx, sy) = Map(helmSeat.position, v, cx, cy, hc2);
                small.Line(sx - 22, sy, sx + 22, sy, 0x907050, 1);
                var (wx, wy) = Map(new Vector3(hc2.x, 0f, hc2.z), v, cx, cy, hc2);
                small.Line(cx - 170, wy, cx + 170, wy, 0x70A0D0);
                var (gx, gy) = Map(grip2.position, v, cx, cy, hc2);
                small.Dot((int)gx, (int)gy, 0xD000D0, 3);
            }
            small.Save(Path.Combine(outDir, "small_boat.png"));
        }

        Console.WriteLine(bad == 0 ? "\nALL CHECKS PASSED" : $"\n{bad} CHECK(S) FAILED");
        return bad == 0 ? 0 : 1;
    }

    static Transform FindDeep(Transform t, string name)
    {
        if (t.name == name) return t;
        for (int i = 0; i < t.childCount; i++) { var f = FindDeep(t.GetChild(i), name); if (f != null) return f; }
        return null;
    }
}
