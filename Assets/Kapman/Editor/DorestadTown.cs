using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kapman.Levels;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kapman.EditorTools
{
    /// <summary>
    /// DORESTAD'S EMPORIUM AS THE EXCAVATIONS SHOW IT (Van Es and Verwers, Hoogstraat 1967-77; Excavations at Dorestad
    /// 1-4; the 2026 research notes in Research/dorestad_c800_notes.md), laid along the west bank of the Kromme Rijn
    /// from the fork with the Lek:
    ///   - the harbour: a comb of earth-and-wood DAMS, not pile jetties, on an 18 m module. Each parcel carries a pair of
    ///     dams 6-7 m wide side by side with a 4 m muddy ditch to the next pair; wattle sides on oak and ash posts, wicker
    ///     mats, planks or bare mud on top, built in short compartments (caissons) closed by transverse post rows, the
    ///     first 30 m by the bank older and heavier, the head tapering into the shoal with an end-revetment of thick
    ///     posts. They run from the old bank out over the silted bed, ~200 m at the apex of the bend, 80-100 m to the
    ///     north, ~40 m at the ends, in a lobed line: some project 20-30 m beyond their neighbours, some stop short
    ///     while the parcel's fences run on, a few are 30 m period-1 stubs. Each storehouse dam begins with a small
    ///     building at the root and then carries pile-founded storehouses chained like Bryggen over half to two
    ///     thirds of its length, with large warehouses in the outer part near the apex (the 'new district' of c. 800);
    ///     the other dam of the pair is left open for work, tents, barrels and hauled-up boats.
    ///   - the bank: a wattle revetment along the old bank, a 2 m plank-and-wicker road behind it (planked on the damp
    ///     stretches), pits behind the revetment, fenced front yards.
    ///   - the vicus, 150-200 m deep and densely built: 9 m half-plots at right angles to the river in sections of 27
    ///     and 54 m, each section a front yard, a narrow house and a back yard with its barrel well and pits; a lane
    ///     after every second or third house; small 4-6 post granaries; smithing in the south half; infield plots,
    ///     pens and hedge lines in the strip toward the farms.
    ///   - the rear strip: big farms, one per ~27-40 m, straight or slightly bowed houses 8-11 m by 20-30 m with the
    ///     byre end, wattle fences round each yard, a heavy granary, a kraal with its trough and the cattle way to the
    ///     basin pasture, a family grave plot on some, single apple trees in the yard; then the fields: irregular
    ///     blocks near the farms, long strips only on the eng.
    ///   - cemeteries on the landward edge every ~300 m, fenced, rows of E-W graves in groups; a small wooden church
    ///     without a tower at one of them (Frankenhof).
    ///   - the Roman fort (castrum, the Upkirika) across the Lek to the south, 1-2 ha, square, bank and ditch, tumbled
    ///     tuff along the bank; the De Geer farm cluster on its levee 500 m north-west. No church, wall or rampart in
    ///     the town itself.
    /// Plot breadths within the module, the setbacks, which parcels are twinned or empty, and the farms' yard shapes
    /// are drawn at random (seeded) within the excavated ranges: those are guesses, the module and the sizes are not.
    /// Every guess beyond the notes is marked in a comment where it is made. Everything is a prefab instance to move,
    /// delete or swap for the user's own models.
    /// </summary>
    public static class DorestadTown
    {
        // Orchard stays in the enum for the painter; nothing writes it since the notes put orchards after the Middle
        // Ages (apple was a collected/garden fruit c. 800: single trees in yards). Hedge is a planting band for the
        // vegetation (osier along the plot lines; hedge pits 15-30 cm are attested, the species is a guess).
        public enum Use : byte { Natural = 0, Yard = 1, Lane = 2, Field = 3, Farmyard = 4, Foreshore = 5, Orchard = 6, Track = 7, Shoal = 8, Cemetery = 9, Market = 10, Hedge = 11 }

        /// <summary>One 18 m parcel of the harbour: where its dams root on the old bank, and how far they run out.</summary>
        public struct Parcel
        {
            public int index;
            public float s;              // along the bank from the fork (m)
            public Vector2 bank;         // the dam root, on the old bank line
            public Vector2 outward;      // toward the water
            public Vector2 along;        // along the bank (downstream)
            public float width;          // the parcel's breadth (18 m, some wider)
            public float damLength;      // root to the water's edge: the parcel's wet bed (the river's shoal follows it)
            public float built;          // how far the dams themselves run (shorter where they stop short or are stubs, longer for a lobe)
            public bool twin;            // a pair of dams
            public bool storehouse;      // chained storehouses on the first dam
            public bool empty;           // no dams: a landing beach
            public bool lobe;            // projects beyond its neighbours: a ship lies alongside the head
            public bool fenceOn;         // the dam stops short but the parcel's fences run on over the bed
            public bool stub;            // a 30 m period-1 stub only
        }

        /// <summary>A tree the town asks the vegetation to plant: kind as DorestadVegetation counts them (0 ash, 1 oak, 2 willow, 3 osier).</summary>
        public struct PlannedTree
        {
            public Vector2 p;
            public int kind;
            public float scale;      // against the kind's base scale
            public int tint;         // 0-3
        }

        public class Plan
        {
            public byte[] use;
            public int res;
            public float texel;
            public Transform player;
            public Vector3 waterfrontView, waterfrontFocus, forkFocus, downRiverView, downRiverFocus;
            public Vector3 townCentre, houseFocus, houseInland, exitDownstream, entryFromSea;
            public float entryYaw;
            public List<Light> hearths = new List<Light>(), lanterns = new List<Light>();
            public List<ParticleSystem> smokes = new List<ParticleSystem>();
            public List<GameObject> bonfires = new List<GameObject>();
            public List<AudioSource> townSounds = new List<AudioSource>();
            /// <summary>The flame meshes of the fire baskets and fires: for TownLife to show only after dusk and to flicker.</summary>
            public List<Renderer> flames = new List<Renderer>();
            /// <summary>Single trees the town wants planted (apple trees in the farmyards, the oaks on the fort's bank).</summary>
            public List<PlannedTree> trees = new List<PlannedTree>();
            /// <summary>Where the fort was put across the Lek (for the bake's meta.json 'fort' key, so its cleared zone follows).</summary>
            public Vector3 fortCentre;
            public List<Parcel> parcels;
            /// <summary>The strip-field blocks: centre, the strips' direction, across, half sizes (the painter aligns its strips to them).</summary>
            public List<(Vector2 c, Vector2 u, Vector2 v, float a, float b)> fields = new List<(Vector2, Vector2, Vector2, float, float)>();

            public void FillRect(Vector2 c, Vector2 u, Vector2 v, float a, float b, byte code)
            {
                float r = Mathf.Sqrt(a * a + b * b);
                int x0 = Mathf.Max(0, (int)((c.x - r) / texel)), x1 = Mathf.Min(res - 1, (int)((c.x + r) / texel));
                int y0 = Mathf.Max(0, (int)((c.y - r) / texel)), y1 = Mathf.Min(res - 1, (int)((c.y + r) / texel));
                for (int y = y0; y <= y1; y++)
                    for (int x = x0; x <= x1; x++)
                    {
                        Vector2 p = new Vector2((x + 0.5f) * texel, (y + 0.5f) * texel) - c;
                        if (Mathf.Abs(Vector2.Dot(p, u)) <= a && Mathf.Abs(Vector2.Dot(p, v)) <= b)
                            use[y * res + x] = code;
                    }
            }

            /// <summary>Marks a line's band with a land use (a road, a lane, a track).</summary>
            public void FillLine(Vector2 a, Vector2 b, float halfWidth, byte code)
            {
                float len = Vector2.Distance(a, b);
                if (len < 0.5f)
                    return;
                Vector2 u = (b - a) / len, v = new Vector2(-u.y, u.x);
                FillRect((a + b) / 2f, u, v, len / 2f, halfWidth, code);
            }

            public byte Get(Vector2 p)
            {
                int x = (int)(p.x / texel), y = (int)(p.y / texel);
                if (x < 0 || y < 0 || x >= res || y >= res)
                    return 255;
                return use[y * res + x];
            }
        }

        const string Dir = DorestadBuilder.LevelDir;
        const float Apex = 850f;             // along the bank from the fork: the bend's apex, the harbour's heart (Hoogstraat I)
        const float HarbourStart = 110f, HarbourEnd = 1700f;
        const float Module = 18f;

        // ------------------------------------------------------------------ the bank and the harbour's parcels

        class Bank
        {
            public List<Vector2> line;
            public float[] s;
            public float side;      // +1: the town is on the left of the downstream direction

            public Vector2 At(float d, out Vector2 tangent)
            {
                int i = 1;
                while (i < s.Length - 1 && s[i] < d)
                    i++;
                float t = Mathf.InverseLerp(s[i - 1], s[i], d);
                int a = Mathf.Max(0, i - 3), b = Mathf.Min(line.Count - 1, i + 2);
                tangent = (line[b] - line[a]).normalized;
                return Vector2.Lerp(line[i - 1], line[i], t);
            }

            public float Length => s[s.Length - 1];
        }

        static Bank MakeBank(DorestadBuilder.Grid g)
        {
            var line = new List<Vector2>(g.channel);
            if (line.Count < 4)
                throw new System.Exception("Kapman Dorestad: no Kromme Rijn line in the level data.");
            if (DistToLek(g, line[line.Count - 1]) < DistToLek(g, line[0]))
                line.Reverse();
            var bank = new Bank { line = line, s = new float[line.Count] };
            for (int i = 1; i < line.Count; i++)
                bank.s[i] = bank.s[i - 1] + Vector2.Distance(line[i - 1], line[i]);
            int near = 0;
            for (int i = 1; i < line.Count; i++)
                if (Vector2.Distance(line[i], g.port) < Vector2.Distance(line[near], g.port))
                    near = i;
            bank.At(bank.s[near], out var tn);
            Vector2 leftN = new Vector2(-tn.y, tn.x);
            bank.side = Vector2.Dot(g.port - line[near], leftN) >= 0 ? 1f : -1f;
            return bank;
        }

        /// <summary>The water's edge on the town's side at a point along the channel, and the inland direction.</summary>
        static Vector2 WaterEdge(DorestadBuilder.Grid g, Bank bank, float s, out Vector2 inland, out Vector2 along)
        {
            Vector2 c = bank.At(s, out var t);
            inland = new Vector2(-t.y, t.x) * bank.side;
            along = t;
            for (float k = 0f; k < 200f; k += 0.5f)
                if (g.SD(c + inland * k) < 0f)
                    return c + inland * k;
            return c + inland * 30f;
        }

        /// <summary>How far the dams run out at a point along the bank: longest at the apex, short at the ends.</summary>
        static float DamLength(float s, float jitter)
        {
            float bell = Mathf.Exp(-((s - Apex) / 430f) * ((s - Apex) / 430f));
            return (40f + 125f * bell) * (0.85f + 0.3f * jitter);
        }

        /// <summary>
        /// Lays out the harbour's parcels, before the river is shaped (the shoal is lowered under them). The waterfront
        /// is lobed as the excavators describe it (ED3 ch. VII): runs of 3-6 parcels share a length, some dams project
        /// 20-30 m beyond their neighbours, some stop short while the parcel's fences run on, a few are 30 m stubs.
        /// </summary>
        public static List<Parcel> Harbour(DorestadBuilder.Grid g, int seed)
        {
            var rnd = new System.Random(seed);
            var bank = MakeBank(g);
            var parcels = new List<Parcel>();
            float end = Mathf.Min(HarbourEnd, bank.Length - 200f);
            int index = 0;
            float s = HarbourStart;
            int runLeft = 0;
            float runFactor = 1f;
            while (s < end)
            {
                if (runLeft <= 0)
                {
                    runLeft = rnd.Next(3, 7);
                    runFactor = 0.85f + (float)rnd.NextDouble() * 0.3f;
                }
                runLeft--;
                float width = rnd.NextDouble() < 0.12 ? Module * 1.5f : Module;
                Vector2 edge = WaterEdge(g, bank, s + width / 2f, out var inland, out var along);
                float len = DamLength(s + width / 2f, (float)rnd.NextDouble()) * runFactor;
                var pc = new Parcel
                {
                    index = index++,
                    s = s + width / 2f,
                    outward = -inland,
                    along = along,
                    width = width,
                    damLength = len,
                    built = len,
                    twin = rnd.NextDouble() < 0.72,
                    storehouse = rnd.NextDouble() < 0.65,
                    empty = rnd.NextDouble() < 0.07,
                };
                double shape = rnd.NextDouble();
                if (shape < 0.12)
                {
                    pc.lobe = true;                        // projects past the shoal's end into the water: a ship lies alongside
                    pc.built = len * 1.25f;
                }
                else if (shape < 0.30)
                {
                    pc.fenceOn = true;                     // HS II: dams to 100 m, the parcel staked to 180 m
                    pc.built = len * 0.6f;
                }
                else if (shape < 0.40)
                {
                    pc.stub = true;                        // a period-1 stub beside full dams
                    pc.built = Mathf.Min(len, 30f);
                    pc.twin = false;
                    pc.storehouse = rnd.NextDouble() < 0.4;
                }
                pc.bank = edge + inland * pc.damLength;
                // the landing beach: three empty parcels together just north of the apex
                if (Mathf.Abs(pc.s - (Apex + 120f)) < Module * 1.6f)
                {
                    pc.empty = true;
                    pc.twin = false;
                    pc.stub = false;
                    pc.fenceOn = false;
                    pc.lobe = false;
                    pc.built = pc.damLength;
                }
                parcels.Add(pc);
                s += width;
            }
            return parcels;
        }

        // ------------------------------------------------------------------ the build

        static Material Mat(string source, string slug, string name, float smooth = 0.08f, Color? tint = null) =>
            AssetLibrary.Lit(source, slug, $"{Dir}/Materials/{name}.mat", smooth, tint);

        class Kit
        {
            public Material[] walls, thatch;
            public Material wattle, wattleFine, plankWall, planks, timber, darkTimber, piles, staves, hoop, stone, linen, net, fish, logs, mat, damPlanks, damMud, mudMat, wetMud, ember, flame, inside, smoke, smokeDark;
            public GameObject[] narrow, longHouse, hall, storehouse, warehouse, church, tents, wells;
            public GameObject granarySmall, granaryRick, granaryHeavy, smithy, shed, hurdle, postRow, gate, woodpile, haystack, midden, rack, querns, barrels, barrel, crates, bales;
            public GameObject lantern, bonfire, revetment, roadPlank, gangway, gangplank, bollard, mooring, timberStack, trough, trestle, strake, graveStone, tuff, wallStub;
        }

        public static Plan Build(DorestadBuilder.Grid g, Transform parent, int seed, List<Parcel> parcels)
        {
            var rnd = new System.Random(seed + 1);
            float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);
            bool Chance(double p) => rnd.NextDouble() < p;
            float Yaw(Vector2 dir) => Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
            var plan = new Plan { res = DorestadBuilder.AlphaRes, texel = g.size / DorestadBuilder.AlphaRes, parcels = parcels };
            plan.use = new byte[plan.res * plan.res];
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), Dir + "/Meshes"));
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), Dir + "/Prefabs"));
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), Dir + "/Materials"));
            var kit = MakeKit(rnd);
            var bank = MakeBank(g);
            float wl = g.wl;

            var town = new GameObject("Dorestad: the emporium (from the excavations; blockouts to replace)").transform;
            town.SetParent(parent, false);
            Transform Group(string name)
            {
                var t = new GameObject(name).transform;
                t.SetParent(town, false);
                return t;
            }
            var gDams = Group("The harbour: dams on the old riverbed");
            var gStores = Group("Storehouses on the dams");
            var gBank = Group("The bank: revetment, road, front yards");
            var gVicus = Group("The vicus: houses, wells, granaries, workshops");
            var gYards = Group("Yards: fences, woodpiles, racks, middens");
            var gFarms = Group("The farms behind the street");
            var gFields = Group("Fields and tracks");
            var gBurials = Group("Cemeteries and the church");
            var gFort = Group("The fort across the Lek, and De Geer");
            var gFire = Group("Fires");
            var gBoats = Group("Moored boats");
            var ship = AssetLibrary.Gislinge("Assets/Kapman/Prefabs/Ships/Ship_Gislinge.prefab");

            // a thing set down at an explicit height (on a dam's top), with its own tint; its flames and lights kept
            GameObject At(GameObject prefab, Transform group, Vector2 p, float y, float yaw, float pitch = 0f, float roll = 0f)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, group);
                go.transform.SetPositionAndRotation(new Vector3(p.x, y, p.y), Quaternion.Euler(pitch, yaw, roll));
                Jitter(go, rnd);
                return go;
            }
            void Fire(GameObject go)
            {
                plan.lanterns.AddRange(go.GetComponentsInChildren<Light>());
                foreach (var r in go.GetComponentsInChildren<MeshRenderer>())
                    if (r.name.StartsWith("Flame"))
                        plan.flames.Add(r);
            }

            // the town's heart for the camera: the apex of the bend, where the parcels were densest (HS I). No market
            // square: the 'permanent annual fair' was the river-bed district itself (ED4 ch. VIII 2.2)
            Parcel apexParcel = parcels.OrderBy(p => Mathf.Abs(p.s - Apex)).First();
            Vector2 apexRoot = apexParcel.bank, apexIn = -apexParcel.outward, apexAlong = apexParcel.along;
            Vector2 centreC = apexRoot + apexIn * 20f;
            plan.townCentre = new Vector3(centreC.x, g.H(centreC), centreC.y);

            // ---- the harbour: the shoal, the revetment and the road, the dams, the storehouses
            Vector2 roadPrev = Vector2.zero;
            bool roadStarted = false;
            int dams = 0, stores = 0, baskets = 0, boatsInDitches = 0;
            Vector2 playerSpot = Vector2.zero;
            float playerYaw = 0f;
            bool playerSet = false, hullDone = false, beachFire = false;
            var lanePoints = new List<Vector2>();
            foreach (var pc in parcels)
            {
                Vector2 root = pc.bank, outw = pc.outward, along = pc.along, inland = -outw;
                float yawOut = Yaw(outw);
                float busy = Mathf.Exp(-((pc.s - Apex) / 400f) * ((pc.s - Apex) / 400f));
                bool nearApex = Mathf.Abs(pc.s - Apex) < 250f;
                // the shoal under and between the dams, out to the water and the beach beyond (the parcel's full bed)
                plan.FillRect(root + outw * (pc.damLength / 2f + 4f), outw, along, pc.damLength / 2f + 6f, pc.width / 2f + 0.1f, (byte)(pc.empty ? Use.Foreshore : Use.Shoal));
                // the road along the roots, 6 m inland of them; planked where the ground is damp (a guess at where:
                // the notes say wicker/planks on posts over the wet stretches); the revetment at the roots with its pits
                Vector2 roadC = root + inland * 6f;
                if (roadStarted)
                {
                    plan.FillLine(roadPrev, roadC, 1.2f, (byte)Use.Lane);
                    if (g.H(roadC) < wl + 0.9f)
                        PlankRoad(g, kit, gBank, roadPrev, roadC, rnd);
                }
                roadPrev = roadC;
                roadStarted = true;
                Revetment(g, kit, gBank, root - along * (pc.width / 2f), root + along * (pc.width / 2f), rnd, pc.empty ? 0.5f : 0.06f);
                for (float k = -pc.width / 2f + R(1f, 3f); k < pc.width / 2f - 1f; k += R(4f, 6f))
                    Mound(g, root + inland * 2f + along * k, 0.6f, -0.35f);        // the row of pits on the old bank
                if (pc.empty)
                {
                    // the landing beach: timber stacked for the dams, cargo, tents of the traders who camped here, boats
                    // of the Utrecht kind drawn up, a hull being broken (ship nails and a dismantled hull in HS II)
                    for (int k = rnd.Next(2, 5); k > 0; k--)
                    {
                        Vector2 bp = root + outw * R(3f, Mathf.Max(6f, pc.damLength * 0.5f)) + along * R(-pc.width / 2f + 2f, pc.width / 2f - 2f);
                        var pick = rnd.NextDouble();
                        var prefab = pick < 0.3 ? kit.woodpile : pick < 0.5 ? kit.barrel : pick < 0.65 ? kit.barrels : pick < 0.85 ? kit.crates : kit.querns;
                        var pr = Put(g, prefab, gBank, bp, R(0f, 360f), rnd, -0.06f);
                        if (prefab == kit.woodpile)
                            pr.transform.localScale = new Vector3(R(1.5f, 2.5f), R(1.2f, 2f), R(1.5f, 2.5f));
                    }
                    for (int k = rnd.Next(1, 3); k > 0; k--)
                    {
                        Vector2 tp = root + outw * R(8f, Mathf.Max(12f, pc.damLength * 0.45f)) + along * R(-pc.width / 2f + 3f, pc.width / 2f - 3f);
                        Put(g, kit.tents[rnd.Next(kit.tents.Length)], gBank, tp, R(0f, 360f), rnd, -0.04f);
                        if (Chance(0.5))
                            Put(g, kit.barrel, gBank, tp + along * R(-4f, 4f) + outw * R(3f, 5f), yawOut + R(-30f, 30f), rnd, -0.06f);
                    }
                    Put(g, kit.timberStack, gBank, root + outw * R(4f, 8f) + along * R(-4f, 4f), yawOut + R(-20f, 20f), rnd, -0.05f);
                    if (Chance(0.6))
                    {
                        Vector2 bp = root + outw * R(pc.damLength * 0.55f, pc.damLength * 0.85f) + along * R(-4f, 4f);
                        var b = DorestadRiver.UtrechtBoat(kit.timber, kit.linen, gBoats, $"Utrecht boat drawn up {pc.index}");
                        b.transform.SetPositionAndRotation(new Vector3(bp.x, g.H(bp) + 0.1f, bp.y), Quaternion.Euler(R(-3f, 3f), yawOut + 180f + R(-15f, 15f), R(4f, 10f)));
                    }
                    if (!hullDone)
                    {
                        hullDone = true;
                        Vector2 hp = root + outw * (pc.damLength * 0.4f) + along * 2f;
                        BrokenHull(g, kit, gBank, hp, yawOut + R(-20f, 20f), rnd);
                    }
                    if (!beachFire)
                    {
                        // the shipwrights' and traders' fire on the beach (ship repair and breaking are attested in HS II;
                        // a fire here is a guess): TownLife lights it on the feast eves until it is wired to any evening
                        beachFire = true;
                        Vector2 fp = root + outw * R(10f, 16f) + along * R(-5f, 5f);
                        var bf = (GameObject)PrefabUtility.InstantiatePrefab(kit.bonfire, gFire);
                        bf.transform.position = new Vector3(fp.x, g.H(fp), fp.y);
                        Fire(bf);
                        bf.SetActive(false);
                        plan.bonfires.Add(bf);
                    }
                    continue;
                }

                // the dams: a pair side by side (6.5 m each, a 1 m seam) or a single one, the ditch to the next pair
                int count = pc.twin ? 2 : 1;
                float damW = 6.5f;
                float offset0 = pc.twin ? -(damW / 2f + 0.5f) : 0f;
                Vector2 headOfFirst = Vector2.zero;
                float topOfFirst = wl + 1f;
                for (int d = 0; d < count; d++)
                {
                    float off = offset0 + d * (damW + 1f) + R(-0.4f, 0.4f);
                    float len = pc.built * (d == 0 ? 1f : R(0.9f, 1.0f));
                    if (!pc.stub && Chance(0.04))
                        len *= R(0.45f, 0.75f);        // a dam still being built, or fallen in
                    len = Mathf.Max(12f, len);
                    Vector2 c = root + outw * (len / 2f) + along * off;
                    float top = wl + 1.0f + R(-0.1f, 0.12f);
                    float baseY = wl - 0.7f;
                    var dam = Dam(kit, len, damW, top - baseY, pc.index * 2 + d);
                    var go = new GameObject($"Dam {pc.index}{(count > 1 ? (d == 0 ? "a" : "b") : "")}");
                    go.transform.SetParent(gDams, false);
                    go.transform.SetPositionAndRotation(new Vector3(c.x, baseY, c.y), Quaternion.Euler(0f, yawOut, 0f));
                    var mf = go.AddComponent<MeshFilter>();
                    mf.sharedMesh = dam;
                    var mr = go.AddComponent<MeshRenderer>();
                    mr.sharedMaterials = new[] { kit.wattleFine, kit.mat, kit.piles, kit.damPlanks, kit.damMud, kit.wetMud };
                    go.AddComponent<MeshCollider>().sharedMesh = dam;
                    go.isStatic = true;
                    dams++;
                    Vector2 head = root + outw * len + along * off;
                    if (d == 0)
                    {
                        headOfFirst = head;
                        topOfFirst = top;
                    }
                    // what stands on it: chained storehouses on the first dam of a storehouse parcel, and on the second
                    // dam too near the apex (HS I period 2: 'a complete new district with large warehouses')
                    bool chained = (d == 0 && pc.storehouse) || (d == 1 && Mathf.Abs(pc.s - Apex) < 150f && Chance(0.6));
                    if (chained)
                    {
                        // each dam began against the bank with a small building (ED3 ch. VIII 2.4: 4 x 6 m)
                        var shed = At(kit.shed, gStores, root + outw * 3.6f + along * off, top + 0.02f, yawOut + 180f + R(-3f, 3f));
                        shed.name = $"Shed at the root of dam {pc.index}";
                        float at = 7.2f + R(0.8f, 2.5f);
                        int chain = Mathf.Max(1, Mathf.FloorToInt(len * R(0.45f, 0.7f) / 11f));
                        float boatGapAt = len * R(0.5f, 0.8f);
                        bool gapDone = false;
                        for (int k = 0; k < chain; k++)
                        {
                            bool outer = at > len * 0.5f;
                            GameObject sp;
                            float sl;
                            if (k == 0 && busy > 0.6f && Chance(0.4))
                            {
                                sp = kit.storehouse[2];                       // the tower-like short store by the bank
                                sl = 6f;
                            }
                            else if (nearApex && outer && Chance(0.6))
                            {
                                sp = kit.warehouse[rnd.Next(kit.warehouse.Length)];
                                sl = 10f;
                            }
                            else
                            {
                                sp = kit.storehouse[rnd.Next(2)];
                                sl = sp.name.Contains("x9") ? 9f : 8f;
                            }
                            if (!gapDone && at > boatGapAt)
                            {
                                // one gap in the row for a boat hauled up
                                gapDone = true;
                                if (Chance(0.6))
                                    Logboat(kit, gBoats, root + outw * (at + 5f) + along * (off + R(-1.2f, 1.2f)), top + 0.02f, yawOut + 180f + R(-8f, 8f), R(6f, 14f), rnd, $"Logboat hauled up on dam {pc.index}");
                                at += 12f;
                            }
                            if (at + sl > len - 3f)
                                break;
                            Vector2 sc = root + outw * (at + sl / 2f) + along * off;
                            var s = At(sp, gStores, sc, top + 0.3f, yawOut + 180f + R(-2f, 2f));   // on its piles
                            s.name = $"Storehouse {stores} on dam {pc.index}";
                            plan.hearths.AddRange(s.GetComponentsInChildren<Light>());
                            at += sl + R(0.8f, 2.5f);
                            stores++;
                        }
                        // bollards at the head's corners
                        At(kit.bollard, gDams, head - outw * 0.9f + along * (damW / 2f - 0.5f), top - 0.1f, R(0f, 360f), R(-3f, 3f), R(-3f, 3f));
                        At(kit.bollard, gDams, head - outw * 0.9f - along * (damW / 2f - 0.5f), top - 0.1f, R(0f, 360f), R(-3f, 3f), R(-3f, 3f));
                    }
                    else
                    {
                        // the open dam: work in its outer half, cargo, barrels, a rack; near the apex the traders' tents
                        // with a barrel row and a fire basket, and a boat hauled up on its side
                        int things = rnd.Next(2, 4 + (int)(busy * 3));
                        for (int k = 0; k < things && len > 20f; k++)
                        {
                            float at = R(len * 0.5f, len - 5f);
                            Vector2 pcq = root + outw * at + along * (off + R(-1.6f, 1.6f));
                            var pick = rnd.NextDouble();
                            var prefab = pick < 0.2 ? kit.barrels : pick < 0.4 ? kit.barrel : pick < 0.55 ? kit.querns : pick < 0.7 ? kit.crates : pick < 0.85 ? kit.bales : kit.rack;
                            At(prefab, gStores, pcq, top - 0.03f, R(0f, 360f));
                        }
                        if (Mathf.Abs(pc.s - Apex) < 300f && len > 30f)
                        {
                            int tents = rnd.Next(1, 3);
                            for (int k = 0; k < tents; k++)
                            {
                                float at = R(len * 0.55f, len - 8f);
                                Vector2 tp = root + outw * at + along * (off + R(-0.6f, 0.6f));
                                At(kit.tents[rnd.Next(kit.tents.Length)], gStores, tp, top - 0.02f, yawOut + (Chance(0.5) ? 0f : 180f) + R(-12f, 12f));
                                if (k == 0)
                                {
                                    var fb = At(kit.lantern, gStores, tp + outw * R(4.5f, 6f) + along * (off + R(-1.5f, 1.5f)), top - 0.02f, R(0f, 360f));
                                    Fire(fb);
                                    baskets++;
                                }
                            }
                            if (Chance(0.5))
                            {
                                // barrels end-on in a row, ready to roll up the dam
                                float at = R(len * 0.3f, len * 0.5f);
                                int n = rnd.Next(4, 9);
                                for (int k = 0; k < n; k++)
                                    At(kit.barrel, gStores, root + outw * (at + R(-0.3f, 0.3f)) + along * (off - (n - 1) * 0.39f + k * 0.78f), top - 0.03f, yawOut + 90f + R(-4f, 4f));
                            }
                            if (Chance(0.35))
                            {
                                var ub = DorestadRiver.UtrechtBoat(kit.timber, kit.linen, gBoats, $"Utrecht boat on its side, dam {pc.index}");
                                Vector2 bp = root + outw * (len * R(0.3f, 0.6f)) + along * off;
                                ub.transform.SetPositionAndRotation(new Vector3(bp.x, top + 0.5f, bp.y), Quaternion.Euler(0f, yawOut + R(-10f, 10f), 70f));
                            }
                        }
                    }
                    // a gangplank across the seam between the twin dams every 18-25 m
                    if (d == 1)
                        for (float gz = R(8f, 20f); gz < len - 4f; gz += R(18f, 25f))
                            At(kit.gangplank, gDams, root + outw * gz + along * (offset0 + damW / 2f + 0.5f), top + 0.02f, yawOut + 90f + R(-6f, 6f));
                    // a fire basket at the head where it's busy (no street lanterns on a Carolingian harbour: open fire
                    // baskets and hearths are what the period had; how many is a guess)
                    if (Chance(0.05 + 0.12 * busy))
                    {
                        var fb = At(kit.lantern, gDams, head - outw * 1.5f + along * (off + 1.6f), top - 0.02f, R(0f, 360f));
                        Fire(fb);
                        baskets++;
                    }
                    // a plank gangway from one head in three down to the water
                    if (Chance(0.33))
                        At(kit.gangway, gDams, head + outw * 0.3f + along * (off + R(-1.5f, 1.5f)), top - 0.05f, yawOut, 12f);
                    // boats at the heads: river traders of the Utrecht kind (expanded logboats, the Vleuten 1 of 734), and
                    // now and then a clinker ship like the Dorestat 1 (the Gislinge stands in for it); the lobes' heads
                    // more often, with a row of mooring posts off them
                    if (pc.lobe && d == 0)
                        for (int k = rnd.Next(4, 7); k > 0; k--)
                        {
                            Vector2 mp = head + outw * R(2f, 6f) + along * (off + R(-3.5f, 3.5f));
                            var m = At(kit.mooring, gDams, mp, Mathf.Min(g.H(mp), wl) - 0.3f, R(0f, 360f), R(-6f, 6f), R(-6f, 6f));
                            m.transform.localScale = new Vector3(1f, (wl + R(0.8f, 1.3f) - (Mathf.Min(g.H(mp), wl) - 0.3f)) / 2f, 1f);
                        }
                    if (Chance(pc.lobe ? 0.6 : 0.3))
                    {
                        Vector2 b = head + outw * 6f + along * (off + R(-2f, 2f));
                        float yaw = Yaw(along) + R(-15f, 15f);
                        float y = Mathf.Max(wl, g.H(b) + 0.05f);
                        if (Chance(0.7))
                        {
                            var ub = DorestadRiver.UtrechtBoat(kit.timber, kit.linen, gBoats, $"Utrecht boat at dam {pc.index}");
                            ub.transform.SetPositionAndRotation(new Vector3(b.x, y + 0.05f, b.y), Quaternion.Euler(0f, yaw, R(-2f, 2f)));
                            ub.AddComponent<ShipMotion>().heading = yaw;
                            var m = ub.GetComponent<ShipMotion>();
                            m.length = 15f;
                            m.beam = 1.6f;
                            m.draught = 0.4f;
                        }
                        else
                        {
                            var boat = (GameObject)PrefabUtility.InstantiatePrefab(ship, gBoats);
                            boat.transform.SetPositionAndRotation(new Vector3(b.x, y, b.y), Quaternion.Euler(0f, yaw, 0f));
                            boat.AddComponent<ShipMotion>().heading = yaw;
                        }
                    }
                    // a short dam: the parcel's fences run on over the bed to the neighbours' length (ED3 ch. VII)
                    if (pc.fenceOn && pc.damLength * 0.8f > len + 6f)
                    {
                        Vector2 a = head + along * (damW / 2f + 0.3f), b = root + outw * (pc.damLength * 0.8f) + along * (off + damW / 2f + 0.3f);
                        FenceLine(g, kit.hurdle, gDams, a, b, rnd, 0.3f, 0.35f);
                        FenceLine(g, kit.hurdle, gDams, a - along * (damW + 0.6f), b - along * (damW + 0.6f), rnd, 0.3f, 0.35f);
                    }
                }
                // a logboat in the dry ditch between this pair and the next (ED3 ch. V 2.3: boats of 2-3 m beam lay fully
                // between two dams); the shoal is dry in the sailing season, so it sits on the mud
                if (pc.twin && Chance(0.4))
                {
                    Vector2 lp = root + outw * (pc.built * R(0.55f, 0.85f)) + along * (pc.width / 2f);
                    Logboat(kit, gBoats, lp, Mathf.Max(wl, g.H(lp) + 0.05f), yawOut + 180f + R(-6f, 6f), 0f, rnd, $"Logboat in the ditch at parcel {pc.index}");
                    boatsInDitches++;
                }
                // the player's ship: moored at a storehouse dam's head just south of the apex
                if (!playerSet && pc.s > Apex - 60f && pc.storehouse && !pc.stub)
                {
                    Vector2 b = headOfFirst + outw * 5.5f + along * (pc.twin ? 3.5f : 0f);
                    playerSpot = b;
                    playerYaw = Yaw(along);
                    playerSet = true;
                    plan.waterfrontFocus = new Vector3(headOfFirst.x, wl + 2f, headOfFirst.y);
                    // the view: 7 m beside the ship, 3.5 m up, 30 degrees off the dam line so a hull and a post sit in front
                    Vector2 side = outw * Mathf.Cos(30f * Mathf.Deg2Rad) - along * Mathf.Sin(30f * Mathf.Deg2Rad);
                    Vector2 view = b + side * 7f;
                    plan.waterfrontView = new Vector3(view.x, wl + 3.5f, view.y);
                }
            }
            if (!playerSet)
            {
                var pc = parcels[parcels.Count / 2];
                playerSpot = pc.bank + pc.outward * (pc.built + 6f);
                playerYaw = Yaw(pc.along);
                plan.waterfrontFocus = new Vector3(playerSpot.x, wl + 2f, playerSpot.y);
                plan.waterfrontView = new Vector3(playerSpot.x, wl + 3.5f, playerSpot.y) + new Vector3(pc.outward.x, 0f, pc.outward.y) * 7f;
            }

            // ---- the vicus: half-plots of 9 m behind the road, 150-200 m deep, densely built (ED4 ch. V): each strip
            // divided lengthwise into sections of 54 or 27 m, each section a front yard, a house and a back yard with
            // its well and pits; the last 30 m a fenced infield
            int houses = 0, wells = 0, granaries = 0, smithies = 0;
            bool houseFocusSet = false;
            float laneCountdown = 2f;
            var farmRoadPts = new List<Vector2>();
            foreach (var pc in parcels)
            {
                Vector2 root = pc.bank, inland = -pc.outward, along = pc.along;
                Vector2 roadC = root + inland * 6f;
                float depth = R(150f, 200f);
                float yawIn = Yaw(inland);
                // the plot's yard, from the road back, and the farm road's point behind it
                plan.FillRect(roadC + inland * (depth / 2f + 2f), inland, along, depth / 2f, pc.width / 2f, (byte)Use.Yard);
                farmRoadPts.Add(roadC + inland * (depth + 8f));
                // a lane along the plot's edge after every second or third house
                laneCountdown -= pc.width / 9f;
                if (laneCountdown <= 0f)
                {
                    Vector2 a = roadC + along * (pc.width / 2f), b = a + inland * (depth + 10f);
                    plan.FillLine(a, b, 1.1f, (byte)Use.Lane);
                    lanePoints.Add(b);
                    laneCountdown = Chance(0.5) ? 2f : 3f;
                }
                // front yards between the road and the roots: fenced, with a rack or cargo
                if (!pc.empty && Chance(0.7))
                {
                    Vector2 fy = root + inland * 3f;
                    FenceLine(g, kit.hurdle, gBank, fy - along * (pc.width / 2f - 0.5f) + inland * 2.5f, fy - along * (pc.width / 2f - 0.5f) - inland * 2.5f, rnd, 0.1f, 0.15f);
                    if (Chance(0.5))
                        Put(g, Chance(0.5) ? kit.rack : kit.barrel, gBank, fy + along * R(-4f, 4f), R(0f, 360f), rnd, -0.05f);
                }
                int halves = Mathf.RoundToInt(pc.width / 9f);
                for (int hp = 0; hp < halves; hp++)
                {
                    float off = (hp - (halves - 1) / 2f) * 9f;
                    if (Chance(0.12))
                        continue;                          // an empty half-plot
                    float sec = 0f;
                    int sections = 0;
                    while (sec < depth - 30f)
                    {
                        float secLen = Chance(0.4) ? 27f : 54f;
                        float pHouse = 0.8f - 0.25f * (sec / depth);
                        if (!Chance(pHouse))
                        {
                            // a section without a house: a pair of small granaries, or nothing
                            if (Chance(0.3))
                            {
                                Vector2 gp = roadC + inland * (sec + R(6f, secLen - 6f)) + along * (off + R(-2f, 2f));
                                Put(g, kit.granarySmall, gVicus, gp, yawIn + R(-20f, 20f), rnd);
                                Put(g, Chance(0.4) ? kit.granaryRick : kit.granarySmall, gVicus, gp + along * 3f + inland * R(-1f, 1f), yawIn + 90f + R(-20f, 20f), rnd);
                                granaries += 2;
                            }
                            sec += secLen;
                            continue;
                        }
                        bool longOne = sections == 0 && secLen >= 54f && Chance(0.12);
                        bool sideOn = Chance(0.2);
                        bool smith = sections > 0 && pc.s > Apex && Chance(0.14);
                        var prefab = smith ? kit.smithy : longOne ? kit.longHouse[rnd.Next(kit.longHouse.Length)] : kit.narrow[rnd.Next(kit.narrow.Length)];
                        float hl = smith ? 10f : longOne ? 24f : 12f + 2f * (prefab.name.Contains("x14") ? 1f : prefab.name.Contains("x16") ? 2f : 0f);
                        float hw = smith ? 6f : prefab.name.Contains("5.5x") ? 5.5f : prefab.name.Contains("6x") ? 6f : 5f;
                        float halfAlong = sideOn ? hw / 2f : hl / 2f;        // the house's reach along the strip
                        float at = sec + (sections == 0 ? R(4f, 8f) : R(2f, 6f)) + halfAlong;
                        if (at + halfAlong > sec + secLen - 2f)
                            at = sec + secLen - 2f - halfAlong;
                        Vector2 hc = roadC + inland * at + along * (off + R(-0.8f, 0.8f));
                        float yaw = yawIn + R(-8f, 8f) + (sideOn ? 90f : 0f);
                        var house = Place(g, prefab, gVicus, hc, yaw, hl / 2f, hw / 2f, rnd, plan, smith ? $"Smithy {smithies}" : $"House {houses}");
                        if (smith)
                        {
                            smithies++;
                            plan.townSounds.Add(AddSound(gVicus, "Smithy", "smithy_254370", new Vector3(hc.x, g.H(hc) + 1.5f, hc.y), 5f, 70f, 0.3f));
                        }
                        else
                            houses++;
                        if (!houseFocusSet && pc.s > Apex + 40f && sections == 0)
                        {
                            plan.houseFocus = new Vector3(hc.x, g.H(hc), hc.y);
                            plan.houseInland = new Vector3(inland.x, 0f, inland.y);
                            houseFocusSet = true;
                        }
                        // the back yard: the well behind the gable (1.4 per house, as ~1,000 were in use), the woodpile
                        // beside the door, a midden, a small granary, pits at some gable ends
                        float back = at + halfAlong;
                        Vector2 behind = roadC + inland * (back + R(3f, 8f)) + along * (off + R(-3f, 3f));
                        PutWell(g, kit, gYards, behind, rnd, plan);
                        wells++;
                        if (Chance(0.4))
                        {
                            PutWell(g, kit, gYards, behind + along * R(-4f, 4f) + inland * R(6f, 10f), rnd, plan);
                            wells++;
                        }
                        if (Chance(0.7))
                            Put(g, kit.woodpile, gYards, roadC + inland * (at - halfAlong + R(2f, 6f)) + along * (off + (Chance(0.5) ? 3.6f : -3.6f)), yaw + R(-20f, 20f), rnd, -0.04f);
                        if (Chance(0.5))
                            Put(g, kit.midden, gYards, behind + along * R(-4f, 4f) + inland * R(2f, 5f), R(0f, 360f), rnd, -0.08f);
                        if (Chance(0.4))
                        {
                            Put(g, Chance(0.3) ? kit.granaryRick : kit.granarySmall, gVicus, roadC + inland * (back + R(8f, 14f)) + along * (off + R(-3f, 3f)), yaw + R(-30f, 30f), rnd);
                            granaries++;
                        }
                        if (Chance(0.1))
                        {
                            // a row of shallow pits off the gable end, with their spoil beside them
                            Vector2 pa = roadC + inland * (back + 1.5f) + along * (off - 3f);
                            for (int k = rnd.Next(3, 7); k > 0; k--)
                            {
                                Ditch(g, pa, pa + along * 1.5f, 0.4f, 1.2f);
                                pa += along * R(1.8f, 2.6f);
                            }
                            Put(g, kit.midden, gYards, pa + inland * 2f, R(0f, 360f), rnd, -0.08f);
                        }
                        sec += secLen;
                        sections++;
                    }
                    // the last 30 m: a fenced infield on the old yard soil
                    Vector2 fa = roadC + inland * (depth - 30f) + along * (off - 4.3f), fb = fa + along * 8.6f;
                    FenceLine(g, kit.hurdle, gYards, fa, fb, rnd, 0.1f, 0.2f, Chance(0.5) ? 2.4f : 0f);
                    // fences down the plot's sides, with gaps and gates, hurdle and post row mixed; a pen behind some houses
                    if (Chance(0.35))
                        FenceLine(g, kit.hurdle, gYards, roadC + along * (off + 4.5f) + inland * 4f, roadC + along * (off + 4.5f) + inland * R(40f, 90f), rnd, 0.1f, 0.3f, 0f, 1f, kit.postRow);
                    if (Chance(0.18))
                    {
                        Vector2 pen = roadC + inland * R(40f, depth - 40f) + along * (off + R(-2f, 2f));
                        Pen(g, kit, gYards, pen, inland, along, R(3f, 4.5f), R(2.5f, 3.5f), rnd);
                        plan.FillRect(pen, inland, along, 4f, 3f, (byte)Use.Lane);
                    }
                }
                // the strip between the vicus and the farms (ED4 ch. III: infield plots on the old yard soil, hedge
                // pits, kraals, drainage trenches, farms pushing in); 30% of parcels left open so it still thins out
                if (Chance(0.3))
                    continue;
                float sd0 = depth + 10f;
                Vector2 su = inland, sv = along;
                if (Chance(0.45))
                {
                    Vector2 ic = roadC + inland * (sd0 + 12f) + along * R(-pc.width / 4f, pc.width / 4f);
                    plan.FillRect(ic, su, sv, 12f, 8f, (byte)Use.Field);
                    plan.fields.Add((ic, su, sv, 12f, 8f));
                    Ditch(g, ic - su * 12f + sv * 9.5f, ic + su * 12f + sv * 9.5f, 0.4f, 1.6f);
                    Ditch(g, ic - su * 12f - sv * 9.5f, ic + su * 12f - sv * 9.5f, 0.4f, 1.6f);
                }
                if (Chance(0.15))
                {
                    Vector2 pen = roadC + inland * (sd0 + R(30f, 45f)) + along * R(-5f, 5f);
                    Pen(g, kit, gYards, pen, inland, along, R(4f, 6f), R(3f, 4.5f), rnd);
                    plan.FillRect(pen, inland, along, 5f, 4f, (byte)Use.Lane);
                }
                if (Chance(0.25))
                {
                    Vector2 gp = roadC + inland * (sd0 + R(20f, 40f)) + along * R(-6f, 6f);
                    Put(g, kit.granarySmall, gYards, gp, R(0f, 360f), rnd);
                    PutStack(g, kit, gYards, gp + along * R(-6f, 6f) + inland * R(5f, 9f), rnd);
                }
                if (Chance(0.3))
                    plan.FillLine(roadC + inland * (sd0 + 2f) + along * (pc.width / 2f - 0.5f), roadC + inland * (sd0 + R(30f, 50f)) + along * (pc.width / 2f - 0.5f), 0.75f, (byte)Use.Hedge);
            }

            // ---- the farm road behind the vicus, and the farms
            for (int i = 1; i < farmRoadPts.Count; i++)
                if (Vector2.Distance(farmRoadPts[i - 1], farmRoadPts[i]) < 60f)
                    plan.FillLine(farmRoadPts[i - 1], farmRoadPts[i], 1.6f, (byte)Use.Track);
            int farms = 0, cemeteries = 0, graves = 0;
            float nextFarm = HarbourStart + 20f, nextCemetery = HarbourStart + 150f;
            var fieldBlocks = new List<(Vector2 c, Vector2 u, Vector2 v, float a, float b, bool block)>();
            for (float s = HarbourStart; s < HarbourEnd && s < bank.Length - 250f; s += 10f)
            {
                WaterEdge(g, bank, s, out var inland, out var along);
                var pc = parcels.OrderBy(p => Mathf.Abs(p.s - s)).First();
                Vector2 roadC = pc.bank + (-pc.outward) * 6f + along * (s - pc.s);
                if (s >= nextFarm)
                {
                    nextFarm = s + R(27f, 42f);
                    float depth = R(205f, 245f);
                    Vector2 fc = roadC + inland * depth;
                    if (g.H(fc) < wl + 0.6f || g.SD(fc) > -60f)
                        continue;
                    bool parallel = Chance(0.25);
                    float fyaw = Yaw(inland) + R(-15f, 15f) + (parallel ? 90f : 0f);
                    Vector2 fu = new Vector2(Mathf.Sin(fyaw * Mathf.Deg2Rad), Mathf.Cos(fyaw * Mathf.Deg2Rad)), fv = new Vector2(fu.y, -fu.x);
                    float ya = R(22f, 30f), yb = R(16f, 24f);
                    plan.FillRect(fc, fu, fv, ya, yb, (byte)Use.Farmyard);
                    var hall = kit.hall[rnd.Next(kit.hall.Length)];
                    float hallLen = hall.name.Contains("x22") ? 22f : hall.name.Contains("x25") ? 25f : hall.name.Contains("x27") ? 27f : 30f;
                    Vector2 hc = fc + fv * R(-3f, 3f);
                    Place(g, hall, gFarms, hc, fyaw, hallLen / 2f, 5.5f, rnd, plan, $"Farm {farms}");
                    // the yard's irregular fence, with a gate toward the road
                    Vector2[] corners =
                    {
                        fc + fu * (ya + R(-3f, 3f)) + fv * (yb + R(-3f, 3f)), fc - fu * (ya + R(-3f, 3f)) + fv * (yb + R(-3f, 3f)),
                        fc - fu * (ya + R(-3f, 3f)) - fv * (yb + R(-3f, 3f)), fc + fu * (ya + R(-3f, 3f)) - fv * (yb + R(-3f, 3f)),
                    };
                    for (int k = 0; k < 4; k++)
                    {
                        FenceLine(g, Chance(0.7) ? kit.hurdle : kit.postRow, gFarms, corners[k], corners[(k + 1) % 4], rnd, 0.08f, 0.25f, k == 3 ? 5f : 0f);
                        // a ditch outside the fence on three sides
                        if (k != 3)
                        {
                            Vector2 mid = (corners[k] + corners[(k + 1) % 4]) / 2f;
                            Vector2 outward = (mid - fc).normalized;
                            Ditch(g, corners[k] + outward * 2.5f, corners[(k + 1) % 4] + outward * 2.5f, 0.5f, 2f);
                        }
                    }
                    if (Chance(0.6))
                        Put(g, kit.granaryHeavy, gFarms, fc + fv * (yb - 7f) * (Chance(0.5) ? 1f : -1f) + fu * R(-8f, 8f), fyaw + R(-10f, 10f), rnd);
                    for (int k = rnd.Next(1, 3); k > 0; k--)
                        Put(g, Chance(0.4) ? kit.granaryRick : kit.granarySmall, gFarms, fc - fu * R(4f, ya - 4f) + fv * R(-yb + 3f, yb - 3f), R(0f, 360f), rnd);
                    for (int k = rnd.Next(1, 4); k > 0; k--)
                        PutStack(g, kit, gFarms, fc - fu * R(3f, ya - 3f) + fv * R(-yb + 3f, yb - 3f), rnd);
                    Put(g, kit.woodpile, gFarms, hc + fv * R(6f, 8f) - fu * R(2f, 8f), fyaw + R(-30f, 30f), rnd, -0.04f);
                    PutWell(g, kit, gFarms, hc - fv * R(7f, 10f) - fu * R(-6f, 6f), rnd, plan);
                    wells++;
                    // the byre end (the hall's +z, the longer part): the dung heap 3-5 m off its gable, the kraal next to
                    // it with a trough at its gate, and the cattle way out to the basin pasture (cattle were the main
                    // animal, Prummel; hoof-prints in the Lek silt at the levee's edge)
                    Vector2 byre = hc + fu * (hallLen / 2f);
                    Put(g, kit.midden, gFarms, byre + fu * R(3f, 5f) + fv * R(-2f, 2f), R(0f, 360f), rnd, -0.08f);
                    Vector2 kc = byre + fu * R(9f, 12f) + fv * R(-4f, 4f);
                    float ka = R(5f, 8f), kb = R(4f, 6f);
                    Pen(g, kit, gFarms, kc, fu, fv, ka, kb, rnd);
                    plan.FillRect(kc, fu, fv, ka, kb, (byte)Use.Lane);
                    Put(g, kit.trough, gFarms, kc + fu * (ka + 1.2f) + fv * 1.5f, fyaw + 90f + R(-10f, 10f), rnd, -0.04f);
                    CattleWay(g, plan, kc + fu * (ka + 1f), fu, wl);
                    // a family grave plot on the yard for some (ED4 ch. VI: small private grave groups on house yards)
                    if (Chance(0.3))
                    {
                        Vector2 gc = fc - fu * (ya - 5f) + fv * (yb - 5f) * (Chance(0.5) ? 1f : -1f);
                        Vector2[] gcorners = { gc + fu * 3f + fv * 2f, gc - fu * 3f + fv * 2f, gc - fu * 3f - fv * 2f, gc + fu * 3f - fv * 2f };
                        for (int k = 0; k < 4; k++)
                            FenceLine(g, kit.postRow, gFarms, gcorners[k], gcorners[(k + 1) % 4], rnd, 0.05f, 0.15f, k == 0 ? 1.2f : 0f);
                        graves += Graves(g, kit, gFarms, gc, 2.2f, 1.4f, rnd.Next(4, 9), rnd, plan);
                    }
                    // apple trees in the yard: the notes allow apple only as a collected/garden fruit c. 800, so single
                    // trees, no orchard blocks (the orchards round Wijk are post-medieval)
                    for (int k = rnd.Next(1, 4); k > 0; k--)
                        plan.trees.Add(new PlannedTree { p = fc + fu * R(-ya + 4f, ya - 4f) + fv * R(-yb + 3f, yb - 3f), kind = 2, scale = R(0.55f, 0.7f), tint = 1 });
                    // the track out to the fields, and the field blocks beyond: irregular blocks near the farms (the
                    // notes' blokverkaveling; long strips belong only to the eng)
                    Vector2 trackEnd = fc + inland * R(120f, 260f) + along * R(-30f, 30f);
                    plan.FillLine(roadC + inland * (depth - ya - 2f), fc + inland * (ya + 2f), 1.3f, (byte)Use.Track);
                    plan.FillLine(fc + inland * (ya + 2f), trackEnd, 1.1f, (byte)Use.Track);
                    int blocks = rnd.Next(2, 5);
                    for (int k = 0; k < blocks; k++)
                    {
                        Vector2 bc = trackEnd + inland * R(-40f, 90f) + along * R(-110f, 110f);
                        float bang = fyaw + R(-25f, 25f);
                        Vector2 bu = new Vector2(Mathf.Sin(bang * Mathf.Deg2Rad), Mathf.Cos(bang * Mathf.Deg2Rad)), bv = new Vector2(bu.y, -bu.x);
                        if (plan.Get(bc) == 0 && g.H(bc) > wl + 0.55f && g.SD(bc) < -50f)
                            fieldBlocks.Add((bc, bu, bv, R(35f, 70f), R(25f, 50f), true));
                    }
                    farms++;
                }
                if (s >= nextCemetery)
                {
                    nextCemetery = s + R(280f, 340f);
                    Vector2 cc = roadC + inland * R(280f, 320f);
                    if (g.H(cc) > wl + 0.6f && plan.Get(cc) == 0)
                    {
                        Vector2 cu = inland, cv = along;
                        float ca = R(22f, 30f), cb = R(22f, 32f);
                        plan.FillRect(cc, cu, cv, ca, cb, (byte)Use.Cemetery);
                        Vector2[] corners = { cc + cu * ca + cv * cb, cc - cu * ca + cv * cb, cc - cu * ca - cv * cb, cc + cu * ca - cv * cb };
                        for (int k = 0; k < 4; k++)
                            FenceLine(g, kit.postRow, gBurials, corners[k], corners[(k + 1) % 4], rnd, 0.05f, 0.15f, k == 0 ? 3f : 0f);
                        // the graves: Christian E-W inhumations in rows, in groups (De Heul: 9 grave groups), a few in
                        // limestone sarcophagi
                        int groups = rnd.Next(3, 7), total = rnd.Next(60, 151);
                        for (int gi = 0; gi < groups; gi++)
                        {
                            Vector2 gc = cc + cu * R(-ca + 8f, ca - 8f) + cv * R(-cb + 8f, cb - 8f);
                            if (cemeteries == 1 && Vector2.Dot(gc - cc, cu) > ca - 20f)
                                gc -= cu * 12f;                    // not under the church
                            graves += Graves(g, kit, gBurials, gc, R(5f, 9f), R(4f, 7f), total / groups, rnd, plan);
                        }
                        if (cemeteries == 1)
                        {
                            // Frankenhof: the small wooden church c. 800, in its fenced yard, the graves E-W round it
                            Place(g, kit.church[0], gBurials, cc + cu * (ca - 10f), 90f + R(-4f, 4f), 7f, 4f, rnd, plan, "Church (Frankenhof)");
                            // the bell: off unless the user turns it on (no bell is attested; G4 the 'bell-cage' is read as a granary)
                            plan.townSounds.Add(AddSound(gBurials, "Church bell", "bell_one_toll_554655", new Vector3(cc.x, g.H(cc) + 6f, cc.y), 30f, 600f, 0.0f));
                        }
                        cemeteries++;
                    }
                }
            }
            foreach (var fb in fieldBlocks)
            {
                plan.FillRect(fb.c, fb.u, fb.v, fb.a, fb.b, (byte)Use.Field);
                plan.fields.Add((fb.c, fb.u, fb.v, fb.a, fb.b));
                Furrows(g, fb.c, fb.u, fb.v, fb.a, fb.b);
                if (fb.block)
                {
                    // a second rectangle turned and shifted, so the union is a block shape, not a stamp
                    float ang = Yaw(fb.u) + R(10f, 35f) * (Chance(0.5) ? 1f : -1f);
                    Vector2 bu = new Vector2(Mathf.Sin(ang * Mathf.Deg2Rad), Mathf.Cos(ang * Mathf.Deg2Rad)), bv = new Vector2(bu.y, -bu.x);
                    Vector2 bc = fb.c + fb.u * R(10f, 25f) * (Chance(0.5) ? 1f : -1f) + fb.v * R(-8f, 8f);
                    float ba = fb.a * R(0.6f, 0.9f), bb = fb.b * R(0.6f, 0.9f);
                    plan.FillRect(bc, bu, bv, ba, bb, (byte)Use.Field);
                    plan.fields.Add((bc, bu, bv, ba, bb));
                    Furrows(g, bc, bu, bv, ba, bb);
                }
                Ditch(g, fb.c + fb.v * (fb.b + 1.5f) - fb.u * fb.a, fb.c + fb.v * (fb.b + 1.5f) + fb.u * fb.a, 0.45f, 2.2f);
                Ditch(g, fb.c - fb.v * (fb.b + 1.5f) - fb.u * fb.a, fb.c - fb.v * (fb.b + 1.5f) + fb.u * fb.a, 0.45f, 2.2f);
            }
            // ditches along both sides of the farm road, which drained the vicus
            for (int i = 1; i < farmRoadPts.Count; i++)
            {
                Vector2 a = farmRoadPts[i - 1], b = farmRoadPts[i];
                if (Vector2.Distance(a, b) > 60f)
                    continue;
                Vector2 dir = (b - a).normalized, side = new Vector2(-dir.y, dir.x);
                Ditch(g, a + side * 3.2f, b + side * 3.2f, 0.5f, 1.8f);
                Ditch(g, a - side * 3.2f, b - side * 3.2f, 0.5f, 1.8f);
            }
            // the "eng": the open arable on the levee crest beyond the farms, in long strips (the only strip fields)
            {
                var pcm = apexParcel;
                Vector2 inland = -pcm.outward, along = pcm.along;
                for (int k = 0; k < 14; k++)
                {
                    Vector2 bc = apexRoot + inland * R(480f, 760f) + along * R(-700f, 700f);
                    if (plan.Get(bc) != 0 || g.H(bc) < wl + 0.6f || g.SD(bc) < -2000f)
                        continue;
                    float bang = Yaw(inland) + R(-30f, 30f);
                    Vector2 bu = new Vector2(Mathf.Sin(bang * Mathf.Deg2Rad), Mathf.Cos(bang * Mathf.Deg2Rad)), bv = new Vector2(bu.y, -bu.x);
                    plan.FillRect(bc, bu, bv, R(60f, 110f), R(40f, 80f), (byte)Use.Field);
                }
            }
            // lanes' ends joined to the farm road
            foreach (var lp in lanePoints)
                plan.FillLine(lp, lp + (-apexParcel.outward) * 12f, 1.1f, (byte)Use.Track);

            // ---- the fort across the Lek (castrum Dorestat, the Upkirika), and De Geer
            {
                // the fort's exact site is lost (engulfed by the river, ED4 ch. VIII 1.1): it is sought explicitly on
                // the far side of the Lek from the fork, perpendicular to the Lek's line, on the first dry ground past
                // the water, so a ship on the Lek sees its mound over the bank. A guess, as the notes say.
                Vector2 fork = bank.line[0];
                Vector2 t0 = (bank.line[Mathf.Min(3, bank.line.Count - 1)] - bank.line[0]).normalized;
                var lek = LekLine(g);
                Vector2 lekT = Vector2.right;
                if (lek.Count > 3)
                {
                    int near = 0;
                    for (int i = 1; i < lek.Count; i++)
                        if (Vector2.Distance(lek[i], fork) < Vector2.Distance(lek[near], fork))
                            near = i;
                    lekT = (lek[Mathf.Min(near + 1, lek.Count - 1)] - lek[Mathf.Max(near - 1, 0)]).normalized;
                }
                Vector2 nrm = new Vector2(-lekT.y, lekT.x);
                Vector2 south = Vector2.Dot(nrm, t0) > 0f ? -nrm : nrm;       // away from the Kromme Rijn's first reach
                bool crossedWater = false, found = false;
                Vector2 fc = fork;
                for (float k = 20f; k < 1400f; k += 10f)
                {
                    Vector2 p = fork + south * k;
                    if (p.x < 150f || p.y < 150f || p.x > g.size - 150f || p.y > g.size - 150f)
                        break;
                    bool water = g.SD(p) > 0f || g.Class(p) == 10;
                    if (water)
                        crossedWater = true;
                    else if (crossedWater && g.SD(p) < -30f && g.H(p) > wl + 0.5f)
                    {
                        fc = p + south * 90f;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    // no far bank found: fall back to dry ground south of the fork along the Kromme Rijn's line
                    fc = fork - t0 * 280f;
                    for (float k = 0f; k < 400f && (g.SD(fc) > -45f || g.H(fc) < wl + 0.5f); k += 15f)
                        fc = fork - t0 * (280f + k);
                    found = g.SD(fc) < -45f;
                }
                if (found)
                {
                    plan.fortCentre = new Vector3(fc.x, g.H(fc), fc.y);
                    Debug.Log($"Kapman Dorestad: the fort (castrum Dorestat) put at ({fc.x:0}, {fc.y:0}) m across the Lek; pass it to the bake's meta.json as 'fort'.");
                    Vector2 fu = south, fv = new Vector2(-south.y, south.x);
                    float half = 65f;
                    Earthwork(g, fc, fu, fv, half, half, 1.8f, 1.2f);
                    plan.FillRect(fc, fu, fv, half - 8f, half - 8f, (byte)Use.Yard);
                    float fyaw = Yaw(fu);
                    // a wattle palisade on the bank (a guess: no palisade is attested, the Roman walls were being robbed)
                    Vector2[] bankCorners = { fc + fu * half + fv * half, fc - fu * half + fv * half, fc - fu * half - fv * half, fc + fu * half - fv * half };
                    for (int k = 0; k < 4; k++)
                        FenceLine(g, kit.hurdle, gFort, bankCorners[k], bankCorners[(k + 1) % 4], rnd, 0.05f, 0.1f, k == 2 ? 4f : 0f, 1.6f);
                    // tumbled tuff blocks along the bank line and a stub of robbed wall: Levefanum's stone went to
                    // Dorestad's hearths (ED4 ch. VII 3.4)
                    for (int k = rnd.Next(20, 31); k > 0; k--)
                    {
                        int side = rnd.Next(4);
                        Vector2 a = bankCorners[side], b = bankCorners[(side + 1) % 4];
                        Vector2 tp = Vector2.Lerp(a, b, (float)rnd.NextDouble()) + (fc - Vector2.Lerp(a, b, 0.5f)).normalized * R(-3f, 6f);
                        Put(g, kit.tuff, gFort, tp, R(0f, 360f), rnd, -0.12f, 0.7f, 1.3f);
                    }
                    Place(g, kit.wallStub, gFort, fc - fu * 30f - fv * 20f, fyaw + R(-20f, 20f), 3f, 1.5f, rnd, null, "Robbed wall stub");
                    // the Upkirika (the fort's church, a small wooden one here: no tower is attested c. 800), two houses,
                    // a granary
                    Place(g, kit.church[1], gFort, fc + fu * 8f, 90f + R(-5f, 5f), 8f, 5f, rnd, plan, "Upkirika, in the fort");
                    Place(g, kit.narrow[0], gFort, fc - fu * 22f + fv * 14f, fyaw + 90f, 7f, 3f, rnd, plan, "House in the fort");
                    Place(g, kit.narrow[2], gFort, fc - fu * 20f - fv * 18f, fyaw, 8f, 3f, rnd, plan, "House in the fort");
                    Put(g, kit.granaryHeavy, gFort, fc + fv * 26f, fyaw, rnd);
                    // six oaks along its river side, so a dark crest stands over the Lek from the ship
                    for (int k = 0; k < 6; k++)
                        plan.trees.Add(new PlannedTree { p = fc - fu * (half + R(4f, 10f)) + fv * R(-half + 6f, half - 6f), kind = 1, scale = R(1.2f, 1.4f), tint = rnd.Next(3) });
                }
                else
                    Debug.LogWarning("Kapman Dorestad: no dry ground found across the Lek for the fort; none built.");

                // De Geer c. 800: a cluster of farmsteads oriented N-S on the older levee 500 m north-west, with barrel
                // and tree-trunk wells (JALC 2010). The ditched 90 x 380 m enclosure with its boat-shaped hall is the
                // (late) 9th century's: for a later phase, not built here.
                Vector2 geer = apexRoot + (-apexParcel.outward) * 420f + apexParcel.along * 380f;
                for (float k = 0f; k < 300f && (plan.Get(geer) != 0 || g.H(geer) < wl + 0.7f); k += 20f)
                    geer += apexParcel.along * 20f;
                if (plan.Get(geer) == 0 && g.H(geer) > wl + 0.6f)
                {
                    Vector2 gu = Vector2.up, gv = Vector2.right;        // world +z: the levee's N-S line, as the notes read it
                    int steads = rnd.Next(5, 8);
                    Vector2 c = geer - gu * (steads - 1) * 25f;
                    Vector2 first = c;
                    for (int k = 0; k < steads; k++)
                    {
                        if (plan.Get(c) == 0 && g.H(c) > wl + 0.5f)
                        {
                            float yaw = R(-10f, 10f);
                            plan.FillRect(c, gu, gv, 15f, 12f, (byte)Use.Farmyard);
                            Vector2[] yc = { c + gu * 15f + gv * 12f, c - gu * 15f + gv * 12f, c - gu * 15f - gv * 12f, c + gu * 15f - gv * 12f };
                            for (int e = 0; e < 4; e++)
                                FenceLine(g, kit.hurdle, gFort, yc[e], yc[(e + 1) % 4], rnd, 0.08f, 0.2f, e == 1 ? 3f : 0f);
                            bool big = Chance(0.5);
                            var p = big ? kit.hall[rnd.Next(kit.hall.Length)] : kit.longHouse[rnd.Next(kit.longHouse.Length)];
                            Place(g, p, gFort, c + gv * R(-2f, 2f), yaw, big ? 12f : 12f, big ? 5.5f : 3f, rnd, plan, $"De Geer farmstead {k}");
                            PutWell(g, kit, gFort, c + gv * R(6f, 9f) + gu * R(-8f, 8f), rnd, plan);
                            Put(g, kit.granarySmall, gFort, c - gv * R(6f, 9f) + gu * R(-10f, 10f), yaw + R(-30f, 30f), rnd);
                            if (Chance(0.5))
                                PutStack(g, kit, gFort, c - gv * R(4f, 8f) + gu * R(-12f, 12f), rnd);
                            wells++;
                        }
                        c += gu * R(40f, 60f);
                    }
                    // a track from the cluster back to the farm road
                    Vector2 nearest = farmRoadPts.Count > 0 ? farmRoadPts.OrderBy(q => Vector2.Distance(q, first)).First() : first;
                    if (Vector2.Distance(nearest, first) < 900f)
                        plan.FillLine(first - gu * 14f, nearest, 1.3f, (byte)Use.Track);
                }
            }

            // ---- the views, the ways out, the player's ship
            {
                plan.forkFocus = new Vector3(bank.line[0].x, wl, bank.line[0].y);
                Vector2 c = bank.At(Mathf.Min(HarbourEnd + 150f, bank.Length - 40f), out _);
                plan.downRiverView = new Vector3(c.x, wl + 4f, c.y);
                Vector2 f = bank.At(900f, out _);
                plan.downRiverFocus = new Vector3(f.x, wl + 2f, f.y);
                var line = bank.line;
                Vector2 e = line[Mathf.Max(0, line.Count - 4)];
                plan.exitDownstream = new Vector3(e.x, wl, e.y);
                Vector2 a = line[Mathf.Max(0, line.Count - 14)], a2 = line[Mathf.Max(0, line.Count - 20)];
                plan.entryFromSea = new Vector3(a.x, wl, a.y);
                plan.entryYaw = Mathf.Atan2(a2.x - a.x, a2.y - a.y) * Mathf.Rad2Deg;
                if (!houseFocusSet)
                {
                    plan.houseFocus = plan.townCentre;
                    plan.houseInland = new Vector3(apexIn.x, 0f, apexIn.y);
                }
            }
            {
                var p = (GameObject)PrefabUtility.InstantiatePrefab(ship, parent);
                p.name = "Your ship: the Gislinge";
                p.transform.SetPositionAndRotation(new Vector3(playerSpot.x, wl, playerSpot.y), Quaternion.Euler(0f, playerYaw, 0f));
                var sm = p.AddComponent<ShipMotion>();
                sm.player = true;
                sm.heading = playerYaw;
                var l = new GameObject("Stern lantern", typeof(Light));
                l.transform.SetParent(p.transform, false);
                l.transform.localPosition = new Vector3(0f, 1.6f, -3.2f);
                var light = l.GetComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.58f, 0.28f);
                light.range = 9f;
                light.intensity = 0f;
                light.shadows = LightShadows.None;
                plan.lanterns.Add(light);
                plan.player = p.transform;
            }
            // hearth sounds here and there
            int hk = 0;
            foreach (var h in plan.hearths)
                if (hk++ % 11 == 0)
                    plan.townSounds.Add(AddSound(h.transform, "Hearth", "fire_454745", h.transform.position, 1.5f, 14f, 0.25f));

            Debug.Log($"Kapman Dorestad: {parcels.Count} parcels, {dams} dams, {stores} storehouses, {baskets} fire baskets, {boatsInDitches} boats in the ditches, {houses} houses, {wells} wells, {granaries} granaries, {smithies} smithies, {farms} farms, {cemeteries} cemeteries, {graves} graves, {fieldBlocks.Count} field blocks, {plan.trees.Count} planned trees, {plan.flames.Count} flames.");
            return plan;
        }

        // ------------------------------------------------------------------ placing

        /// <summary>Places a building on ground flattened under it, with a little tint and lean of its own.</summary>
        static GameObject Place(DorestadBuilder.Grid g, GameObject prefab, Transform parent, Vector2 c, float yaw, float halfLong, float halfWide, System.Random rnd, Plan plan, string name)
        {
            Vector2 u = new Vector2(Mathf.Sin(yaw * Mathf.Deg2Rad), Mathf.Cos(yaw * Mathf.Deg2Rad)), v = new Vector2(u.y, -u.x);
            float y = Flatten(g, c, u, v, halfLong + 1f, halfWide + 1f, 2.5f);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.name = name;
            go.transform.SetPositionAndRotation(new Vector3(c.x, y, c.y), Quaternion.Euler(0f, yaw, 0f));
            Jitter(go, rnd);
            if (plan != null)
            {
                plan.hearths.AddRange(go.GetComponentsInChildren<Light>());
                // every hearth smokes (TownLife thins them between the cooking peaks)
                plan.smokes.AddRange(go.GetComponentsInChildren<ParticleSystem>());
                // the worn ground at the door and round the house
                plan.FillRect(c - u * (halfLong + 2f), u, v, 2.5f, 2f, (byte)Use.Lane);
            }
            return go;
        }

        static GameObject Put(DorestadBuilder.Grid g, GameObject prefab, Transform parent, Vector2 p, float yaw, System.Random rnd, float sink = 0f, float scaleLo = 0.9f, float scaleHi = 1.1f)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.transform.SetPositionAndRotation(new Vector3(p.x, g.H(p) + sink, p.y), Quaternion.Euler((float)(rnd.NextDouble() - 0.5) * 4f, yaw, (float)(rnd.NextDouble() - 0.5) * 4f));
            float s = scaleLo + (float)rnd.NextDouble() * (scaleHi - scaleLo);
            go.transform.localScale = new Vector3(s, s * (0.95f + (float)rnd.NextDouble() * 0.1f), s);
            Jitter(go, rnd);
            return go;
        }

        /// <summary>
        /// A well: a wine barrel sunk so its rim and 40 cm of staves show (the excavated ones were flush with the ground),
        /// a hollowed trunk for some, a bucket frame over some, trodden mud round it.
        /// </summary>
        static GameObject PutWell(DorestadBuilder.Grid g, Kit kit, Transform parent, Vector2 p, System.Random rnd, Plan plan)
        {
            double pick = rnd.NextDouble();
            var prefab = pick < 0.15 ? kit.wells[2] : pick < 0.45 ? kit.wells[1] : kit.wells[0];
            var go = Put(g, prefab, parent, p, (float)rnd.NextDouble() * 360f, rnd, 0f, 0.95f, 1.05f);
            go.transform.rotation = Quaternion.Euler(0f, go.transform.eulerAngles.y, 0f);     // upright: it is sunk, not set down
            if (plan != null)
                plan.FillRect(p, Vector2.up, Vector2.right, 0.8f, 0.8f, (byte)Use.Lane);
            return go;
        }

        /// <summary>A haystack with a height of its own (1.8-3.2 m) and the props leaning on it.</summary>
        static GameObject PutStack(DorestadBuilder.Grid g, Kit kit, Transform parent, Vector2 p, System.Random rnd)
        {
            var go = Put(g, kit.haystack, parent, p, (float)rnd.NextDouble() * 360f, rnd, -0.05f, 0.75f, 1.25f);
            float h = 0.52f + (float)rnd.NextDouble() * 0.4f;      // 1.8-3.2 m stacks
            go.transform.localScale = new Vector3(go.transform.localScale.x, h, go.transform.localScale.z);
            return go;
        }

        /// <summary>
        /// A little colour of its own on every instance: no two walls or roofs quite the same, the thatch most of all
        /// (new thatch is gold for a season, then greys).
        /// </summary>
        static void Jitter(GameObject go, System.Random rnd)
        {
            float hue = 0.94f + (float)rnd.NextDouble() * 0.12f, val = 0.9f + (float)rnd.NextDouble() * 0.2f;
            float thatchVal = 0.75f + (float)rnd.NextDouble() * 0.4f;
            foreach (var r in go.GetComponentsInChildren<MeshRenderer>())
            {
                var ms = r.sharedMaterials;
                for (int i = 0; i < ms.Length; i++)
                {
                    if (ms[i] == null || !ms[i].HasProperty("_BaseColor") || ms[i].shader == null || ms[i].shader.name.Contains("Unlit"))
                        continue;
                    bool thatch = ms[i].name.StartsWith("Thatch");
                    float v = thatch ? thatchVal : val;
                    var mpb = new MaterialPropertyBlock();
                    r.GetPropertyBlock(mpb, i);
                    Color c = ms[i].GetColor("_BaseColor");
                    mpb.SetColor("_BaseColor", new Color(c.r * hue * v, c.g * v, c.b * (2f - hue) * v, c.a));
                    r.SetPropertyBlock(mpb, i);
                }
            }
        }

        /// <summary>Flattens the ground under a footprint to its mean, feathered out over `feather` metres. Returns the floor's height.</summary>
        public static float Flatten(DorestadBuilder.Grid g, Vector2 c, Vector2 u, Vector2 v, float a, float b, float feather)
        {
            float sum = 0f;
            int count = 0;
            for (float x = -a; x <= a; x += g.cell)
                for (float y = -b; y <= b; y += g.cell)
                {
                    sum += g.H(c + u * x + v * y);
                    count++;
                }
            float floor = sum / Mathf.Max(1, count) + 0.08f;
            float r = Mathf.Sqrt(a * a + b * b) + feather;
            int x0 = Mathf.Max(0, (int)((c.x - r) / g.cell)), x1 = Mathf.Min(g.n - 1, (int)((c.x + r) / g.cell) + 1);
            int z0 = Mathf.Max(0, (int)((c.y - r) / g.cell)), z1 = Mathf.Min(g.n - 1, (int)((c.y + r) / g.cell) + 1);
            for (int z = z0; z <= z1; z++)
                for (int x = x0; x <= x1; x++)
                {
                    Vector2 p = new Vector2(x * g.cell, z * g.cell) - c;
                    float du = Mathf.Abs(Vector2.Dot(p, u)) - a, dv = Mathf.Abs(Vector2.Dot(p, v)) - b;
                    float d = Mathf.Max(du, dv);
                    if (d > feather)
                        continue;
                    int i = z * g.n + x;
                    if (g.sd[i] > -1f)
                        continue;
                    float w = d <= 0f ? 1f : 1f - d / feather;
                    g.h[i] = Mathf.Lerp(g.h[i], floor, w);
                }
            return floor;
        }

        /// <summary>A drainage ditch along a line: a shallow V, `width` across and `depth` deep, with a little spoil beside it.</summary>
        public static void Ditch(DorestadBuilder.Grid g, Vector2 a, Vector2 b, float depth, float width)
        {
            float len = Vector2.Distance(a, b);
            if (len < 1f)
                return;
            Vector2 u = (b - a) / len, v = new Vector2(-u.y, u.x);
            float r = width + 1.5f;
            float minX = Mathf.Min(a.x, b.x) - r, maxX = Mathf.Max(a.x, b.x) + r, minZ = Mathf.Min(a.y, b.y) - r, maxZ = Mathf.Max(a.y, b.y) + r;
            int x0 = Mathf.Max(0, (int)(minX / g.cell)), x1 = Mathf.Min(g.n - 1, (int)(maxX / g.cell) + 1);
            int z0 = Mathf.Max(0, (int)(minZ / g.cell)), z1 = Mathf.Min(g.n - 1, (int)(maxZ / g.cell) + 1);
            for (int z = z0; z <= z1; z++)
                for (int x = x0; x <= x1; x++)
                {
                    Vector2 p = new Vector2(x * g.cell, z * g.cell) - a;
                    float along = Vector2.Dot(p, u);
                    if (along < 0f || along > len)
                        continue;
                    float across = Mathf.Abs(Vector2.Dot(p, v));
                    int i = z * g.n + x;
                    if (g.sd[i] > -2f)
                        continue;
                    if (across < width / 2f)
                        g.h[i] -= depth * (1f - across / (width / 2f));
                    else if (across < width / 2f + 1.2f)
                        g.h[i] += depth * 0.15f * (1f - (across - width / 2f) / 1.2f);
                }
        }

        /// <summary>
        /// The ard's scratch across a field block: faint lines along the strips' direction. The 8th-9th century ard
        /// cross-ploughed flat fields; ridge-and-furrow needs the mouldboard plough and is a later image, so the
        /// amplitude is 1.5 cm, nothing seen from the air.
        /// </summary>
        static void Furrows(DorestadBuilder.Grid g, Vector2 c, Vector2 u, Vector2 v, float a, float b)
        {
            const float period = 1.6f, amplitude = 0.015f;
            float r = Mathf.Sqrt(a * a + b * b);
            int x0 = Mathf.Max(0, (int)((c.x - r) / g.cell)), x1 = Mathf.Min(g.n - 1, (int)((c.x + r) / g.cell) + 1);
            int z0 = Mathf.Max(0, (int)((c.y - r) / g.cell)), z1 = Mathf.Min(g.n - 1, (int)((c.y + r) / g.cell) + 1);
            for (int z = z0; z <= z1; z++)
                for (int x = x0; x <= x1; x++)
                {
                    Vector2 p = new Vector2(x * g.cell, z * g.cell) - c;
                    float du = Vector2.Dot(p, u), dv = Vector2.Dot(p, v);
                    if (Mathf.Abs(du) > a - 1f || Mathf.Abs(dv) > b - 1f)
                        continue;
                    g.h[z * g.n + x] += amplitude * Mathf.Sin(dv / period * Mathf.PI * 2f);
                }
        }

        /// <summary>A round hump (or, with a negative height, a pit).</summary>
        static void Mound(DorestadBuilder.Grid g, Vector2 c, float radius, float height)
        {
            int x0 = Mathf.Max(0, (int)((c.x - radius) / g.cell)), x1 = Mathf.Min(g.n - 1, (int)((c.x + radius) / g.cell) + 1);
            int z0 = Mathf.Max(0, (int)((c.y - radius) / g.cell)), z1 = Mathf.Min(g.n - 1, (int)((c.y + radius) / g.cell) + 1);
            for (int z = z0; z <= z1; z++)
                for (int x = x0; x <= x1; x++)
                {
                    float d = Vector2.Distance(new Vector2(x * g.cell, z * g.cell), c) / radius;
                    if (d < 1f && g.sd[z * g.n + x] < -1f)
                        g.h[z * g.n + x] += height * (1f - d * d);
                }
        }

        /// <summary>An oblong grave mound: `along` by `across` metres, `height` high, its long axis along u.</summary>
        static void Oblong(DorestadBuilder.Grid g, Vector2 c, Vector2 u, float along, float across, float height)
        {
            Vector2 v = new Vector2(-u.y, u.x);
            float r = Mathf.Max(along, across) / 2f + g.cell;
            int x0 = Mathf.Max(0, (int)((c.x - r) / g.cell)), x1 = Mathf.Min(g.n - 1, (int)((c.x + r) / g.cell) + 1);
            int z0 = Mathf.Max(0, (int)((c.y - r) / g.cell)), z1 = Mathf.Min(g.n - 1, (int)((c.y + r) / g.cell) + 1);
            for (int z = z0; z <= z1; z++)
                for (int x = x0; x <= x1; x++)
                {
                    Vector2 p = new Vector2(x * g.cell, z * g.cell) - c;
                    float du = Vector2.Dot(p, u) / (along / 2f), dv = Vector2.Dot(p, v) / (across / 2f);
                    float d = du * du + dv * dv;
                    if (d < 1f && g.sd[z * g.n + x] < -1f)
                        g.h[z * g.n + x] += height * (1f - d);
                }
        }

        /// <summary>
        /// Graves in rows: Christian E-W inhumations (the long axis world +x), pitch 1.2 m across and 2.6 m along with
        /// a little jitter, oblong mounds 2 x 0.8 m and 0.2 m high, 3-5% in a stone box half sunk. Returns how many.
        /// </summary>
        static int Graves(DorestadBuilder.Grid g, Kit kit, Transform parent, Vector2 c, float halfX, float halfZ, int count, System.Random rnd, Plan plan)
        {
            float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);
            int cols = Mathf.Max(1, Mathf.FloorToInt(halfX * 2f / 2.6f)), rows = Mathf.Max(1, Mathf.FloorToInt(halfZ * 2f / 1.2f));
            int made = 0;
            for (int rI = 0; rI < rows && made < count; rI++)
                for (int cI = 0; cI < cols && made < count; cI++)
                {
                    if (rnd.NextDouble() < 0.15)
                        continue;
                    Vector2 p = c + new Vector2(-halfX + 1.3f + cI * 2.6f + R(-0.3f, 0.3f), -halfZ + 0.6f + rI * 1.2f + R(-0.3f, 0.3f));
                    if (plan != null && plan.Get(p) == 255)
                        continue;
                    Oblong(g, p, Vector2.right, 2.0f, 0.8f, 0.2f);
                    if (rnd.NextDouble() < 0.04)
                        Put(g, kit.graveStone, parent, p, R(-4f, 4f), rnd, -0.3f, 0.95f, 1.05f);
                    made++;
                }
            return made;
        }

        /// <summary>A bank round a rectangle with a ditch outside it (the fort).</summary>
        static void Earthwork(DorestadBuilder.Grid g, Vector2 c, Vector2 u, Vector2 v, float a, float b, float bankHeight, float ditchDepth)
        {
            float r = Mathf.Sqrt(a * a + b * b) + 14f;
            int x0 = Mathf.Max(0, (int)((c.x - r) / g.cell)), x1 = Mathf.Min(g.n - 1, (int)((c.x + r) / g.cell) + 1);
            int z0 = Mathf.Max(0, (int)((c.y - r) / g.cell)), z1 = Mathf.Min(g.n - 1, (int)((c.y + r) / g.cell) + 1);
            for (int z = z0; z <= z1; z++)
                for (int x = x0; x <= x1; x++)
                {
                    Vector2 p = new Vector2(x * g.cell, z * g.cell) - c;
                    float d = Mathf.Max(Mathf.Abs(Vector2.Dot(p, u)) - a, Mathf.Abs(Vector2.Dot(p, v)) - b);   // 0 at the edge
                    int i = z * g.n + x;
                    if (g.sd[i] > -1f)
                        continue;
                    if (d > -4f && d <= 1.5f)
                        g.h[i] += bankHeight * (1f - Mathf.Abs((d + 1.25f) / 2.75f));
                    else if (d > 1.5f && d < 8f)
                        g.h[i] -= ditchDepth * (1f - Mathf.Abs((d - 4.75f) / 3.25f));
                }
        }

        /// <summary>The cattle way from a kraal out to the nearest low pasture (ground under wl + 0.5 m) within 300 m.</summary>
        static void CattleWay(DorestadBuilder.Grid g, Plan plan, Vector2 from, Vector2 preferred, float wl)
        {
            Vector2 best = Vector2.zero;
            float bestD = 1e9f;
            for (float ang = 0f; ang < 360f; ang += 20f)
            {
                Vector2 d = new Vector2(Mathf.Cos(ang * Mathf.Deg2Rad), Mathf.Sin(ang * Mathf.Deg2Rad));
                for (float r = 20f; r < 300f; r += 10f)
                {
                    Vector2 p = from + d * r;
                    if (plan.Get(p) == 255)
                        break;
                    if (g.SD(p) > -8f)
                        break;                               // not into the river
                    if (g.H(p) < wl + 0.5f && plan.Get(p) == 0)
                    {
                        float score = r - 40f * Vector2.Dot(d, preferred);
                        if (score < bestD)
                        {
                            bestD = score;
                            best = p;
                        }
                        break;
                    }
                }
            }
            if (bestD < 1e8f)
            {
                // two legs, so it bends round the yard
                Vector2 mid = Vector2.Lerp(from, best, 0.5f) + new Vector2(-(best - from).y, (best - from).x).normalized * 6f;
                plan.FillLine(from, mid, 0.75f, (byte)Use.Track);
                plan.FillLine(mid, best, 0.75f, (byte)Use.Track);
            }
        }

        static void Pen(DorestadBuilder.Grid g, Kit kit, Transform parent, Vector2 c, Vector2 u, Vector2 v, float a, float b, System.Random rnd)
        {
            Vector2[] corners = { c + u * a + v * b, c - u * a + v * b, c - u * a - v * b, c + u * a - v * b };
            for (int k = 0; k < 4; k++)
                FenceLine(g, kit.hurdle, parent, corners[k], corners[(k + 1) % 4], rnd, 0.05f, 0.1f, k == 0 ? 1.6f : 0f);
        }

        /// <summary>
        /// A fence from a to b, as the excavated ones were: 'often repaired and shifted'. The run is split into stretches
        /// of 3-6 sections, each with its own sideways shift and a few degrees of yaw; at a stretch's end now and then a
        /// gap with a gate (two taller posts); 8% of sections fallen; with `alt` the stretches alternate between the two
        /// fence kinds (hurdle and post row mixed). `gap` leaves an opening at the start; `scaleY` makes a palisade.
        /// </summary>
        static void FenceLine(DorestadBuilder.Grid g, GameObject fence, Transform parent, Vector2 a, Vector2 b, System.Random rnd, float missing, float lean, float gap = 0f, float scaleY = 1f, GameObject alt = null)
        {
            float len = Vector2.Distance(a, b);
            if (len < 1f)
                return;
            Vector2 dir = (b - a) / len, side = new Vector2(-dir.y, dir.x);
            float yaw = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
            float section = fence.name.StartsWith("Rail") ? 3f : 2.4f;
            float start = gap > 0 ? gap : 0f;
            int stretchLeft = 0;
            float shift = 0f, turn = 0f;
            GameObject kind = fence;
            bool gateNext = false;
            for (float k = start + section / 2f; k < len - 0.3f; k += section)
            {
                if (stretchLeft <= 0)
                {
                    stretchLeft = rnd.Next(3, 7);
                    shift = (float)(rnd.NextDouble() - 0.5);
                    turn = (float)(rnd.NextDouble() - 0.5) * 6f;
                    if (alt != null)
                        kind = rnd.NextDouble() < 0.5 ? fence : alt;
                    if (gateNext && GatePrefab != null)
                    {
                        // a gate: two taller posts and a missing section
                        gateNext = false;
                        Vector2 gp = a + dir * k + side * shift;
                        var gate = (GameObject)PrefabUtility.InstantiatePrefab(GatePrefab, parent);
                        gate.transform.SetPositionAndRotation(new Vector3(gp.x, g.H(gp) - 0.05f, gp.y), Quaternion.Euler(0f, yaw + 90f + turn, 0f));
                        stretchLeft--;
                        continue;
                    }
                    gateNext = rnd.NextDouble() < 0.5;
                }
                stretchLeft--;
                if (rnd.NextDouble() < missing)
                    continue;
                Vector2 p = a + dir * k + side * shift;
                var f = (GameObject)PrefabUtility.InstantiatePrefab(kind, parent);
                bool fallen = rnd.NextDouble() < 0.08;
                f.transform.SetPositionAndRotation(new Vector3(p.x, g.H(p) - (fallen ? 0.3f : 0.05f), p.y),
                    Quaternion.Euler(fallen ? 70f * (rnd.NextDouble() < 0.5 ? 1f : -1f) : (float)(rnd.NextDouble() - 0.5) * lean * 20f,
                                     yaw + 90f + turn + (float)(rnd.NextDouble() - 0.5) * 6f,
                                     (float)(rnd.NextDouble() - 0.5) * lean * 30f));
                f.transform.localScale = new Vector3(1f, (0.85f + (float)rnd.NextDouble() * 0.3f) * scaleY, 1f);
            }
        }

        static GameObject GatePrefab;

        /// <summary>
        /// The wattle revetment along the top of the old bank (ED3 ch. IV 2.3: renewed several times): panels with their
        /// base 0.6 m below the levee's edge, posts every 0.6 m, so the bank drops to the shoal behind them.
        /// </summary>
        static void Revetment(DorestadBuilder.Grid g, Kit kit, Transform parent, Vector2 a, Vector2 b, System.Random rnd, float missing)
        {
            float len = Vector2.Distance(a, b);
            if (len < 1f)
                return;
            Vector2 dir = (b - a) / len;
            float yaw = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
            for (float k = 1.2f; k < len - 0.3f; k += 2.4f)
            {
                if (rnd.NextDouble() < missing)
                    continue;
                Vector2 p = a + dir * k;
                var f = (GameObject)PrefabUtility.InstantiatePrefab(kit.revetment, parent);
                f.transform.SetPositionAndRotation(new Vector3(p.x, g.H(p) - 0.6f, p.y), Quaternion.Euler((float)(rnd.NextDouble() - 0.5) * 4f, yaw + 90f + (float)(rnd.NextDouble() - 0.5) * 5f, 0f));
                f.transform.localScale = new Vector3(1f, 0.9f + (float)rnd.NextDouble() * 0.25f, 1f);
            }
        }

        /// <summary>The road laid as planks on posts over a damp stretch: 2.4 m segments at 10 cm over the ground.</summary>
        static void PlankRoad(DorestadBuilder.Grid g, Kit kit, Transform parent, Vector2 a, Vector2 b, System.Random rnd)
        {
            float len = Vector2.Distance(a, b);
            if (len < 1f || len > 80f)
                return;
            Vector2 dir = (b - a) / len;
            float yaw = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
            for (float k = 1.2f; k < len; k += 2.4f)
            {
                Vector2 p = a + dir * k;
                var f = (GameObject)PrefabUtility.InstantiatePrefab(kit.roadPlank, parent);
                f.transform.SetPositionAndRotation(new Vector3(p.x, g.H(p) + 0.1f, p.y), Quaternion.Euler((float)(rnd.NextDouble() - 0.5) * 3f, yaw + (float)(rnd.NextDouble() - 0.5) * 4f, (float)(rnd.NextDouble() - 0.5) * 3f));
                Jitter(f, rnd);
            }
        }

        static Mesh logboatMesh;

        /// <summary>A logboat set down: on the mud in a ditch, hauled up on a dam, with its own tint.</summary>
        static GameObject Logboat(Kit kit, Transform parent, Vector2 p, float y, float yaw, float roll, System.Random rnd, string name)
        {
            if (logboatMesh == null)
                logboatMesh = SaveMesh(MeshKit.Logboat("Logboat_5m_town", 5.2f, 0.75f));
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Part(go.transform, "Hull", logboatMesh, kit.timber, kit.timber);
            go.transform.SetPositionAndRotation(new Vector3(p.x, y, p.y), Quaternion.Euler((float)(rnd.NextDouble() - 0.5) * 4f, yaw, roll));
            Jitter(go, rnd);
            return go;
        }

        /// <summary>
        /// The hull being broken on the landing beach (ship nails and a dismantled clinker hull in HS II): a big logboat
        /// keel-up, loose strakes fanned beside it, a trestle, a woodpile.
        /// </summary>
        static void BrokenHull(DorestadBuilder.Grid g, Kit kit, Transform parent, Vector2 p, float yaw, System.Random rnd)
        {
            float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);
            var hull = Logboat(kit, parent, p, g.H(p) + 0.9f, yaw, 180f, rnd, "Hull being broken up");
            hull.transform.rotation = Quaternion.Euler(6f, yaw, 180f);
            hull.transform.localScale = Vector3.one * 2.6f;
            Vector2 u = new Vector2(Mathf.Sin(yaw * Mathf.Deg2Rad), Mathf.Cos(yaw * Mathf.Deg2Rad)), v = new Vector2(u.y, -u.x);
            for (int k = 0; k < 6; k++)
            {
                Vector2 sp = p + v * (3f + k * 0.7f) + u * R(-2f, 2f);
                var s = Put(g, kit.strake, parent, sp, yaw + R(-25f, 25f), rnd, 0.02f, 0.95f, 1.05f);
                s.transform.rotation = Quaternion.Euler(R(-3f, 3f), yaw + k * 9f - 20f, R(-6f, 6f));
            }
            Put(g, kit.trestle, parent, p - v * 5f + u * R(-2f, 2f), yaw + R(-20f, 20f), rnd, -0.03f);
            Put(g, kit.woodpile, parent, p - v * 8f + u * R(-3f, 3f), yaw + R(-30f, 30f), rnd, -0.04f);
        }

        static float DistToLek(DorestadBuilder.Grid g, Vector2 p)
        {
            float best = 1e9f;
            for (float a = 0; a < 360; a += 30)
                for (float r = 10; r < 600; r += 20)
                {
                    Vector2 q = p + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad)) * r;
                    if (g.Class(q) == 10 && r < best)
                        best = r;
                }
            return best;
        }

        /// <summary>A centre line for the Lek through the level: the middle of the class-10 water, west to east.</summary>
        static List<Vector2> LekLine(DorestadBuilder.Grid g)
        {
            var line = new List<Vector2>();
            for (float x = 10f; x < g.size; x += 40f)
            {
                float sum = 0f, count = 0f;
                for (float z = 0f; z < g.size; z += g.cell * 2f)
                    if (g.Class(new Vector2(x, z)) == 10)
                    {
                        sum += z;
                        count++;
                    }
                if (count > 3f)
                    line.Add(new Vector2(x, sum / count));
            }
            return line;
        }

        static AudioSource AddSound(Transform parent, string name, string clip, Vector3 at, float min, float max, float volume)
        {
            var go = new GameObject("Sound: " + name, typeof(AudioSource));
            go.transform.SetParent(parent, true);
            go.transform.position = at;
            var a = go.GetComponent<AudioSource>();
            a.clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/Kapman/Audio/Sounds/{clip}.mp3");
            a.loop = true;
            a.playOnAwake = volume > 0f;
            a.spatialBlend = 1f;
            a.rolloffMode = AudioRolloffMode.Linear;
            a.minDistance = min;
            a.maxDistance = max;
            a.volume = volume;
            a.dopplerLevel = 0f;
            return a;
        }

        // ------------------------------------------------------------------ the dams

        static readonly Dictionary<string, Mesh> DamMeshes = new Dictionary<string, Mesh>();

        /// <summary>
        /// A dam as one mesh, built as the excavations describe it (ED3 ch. III 2): short compartments (caissons) of
        /// 8-14 m, each with its own top height, width and surface (wicker mat, planks or bare mud), closed by
        /// transverse rows of 10-15 sturdy posts; side posts at 0.6-1.6 m with the first 30 m from the bank heavier and
        /// closer (the period-1 dam); the head tapering 0.4 m into the shoal with a 12-post end-revetment and a wattle
        /// end panel; a dark wet band at the waterline. Submeshes: 0 wattle sides, 1 wicker mat tops, 2 posts, 3 plank
        /// tops, 4 mud tops (and the head's slope), 5 the waterline band. Cached by length, width, height and a
        /// variant, so dams of one length still differ. Local z runs root (-) to head (+); y = 0 is the dam's foot.
        /// </summary>
        static Mesh Dam(Kit kit, float length, float width, float height, int seed)
        {
            float len = Mathf.Round(length / 2f) * 2f;
            int variant = seed % 5;
            string key = $"{len:0}_{Mathf.RoundToInt(width * 10f)}_{Mathf.RoundToInt(height * 10f)}_{variant}";
            if (DamMeshes.TryGetValue(key, out var cached) && cached != null)
                return cached;
            var rnd = new System.Random(variant * 7919 + (int)len * 13 + 5);
            float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);
            var parts = new List<CombineInstance>[6];
            for (int i = 0; i < 6; i++)
                parts[i] = new List<CombineInstance>();
            var temp = new List<Mesh>();
            Mesh Keep(Mesh m)
            {
                temp.Add(m);
                return m;
            }
            void Add(int sub, Mesh m, int subOf, Matrix4x4 tr) => parts[sub].Add(new CombineInstance { mesh = m, subMeshIndex = subOf, transform = tr });
            // the post meshes: a metre tall, scaled to each post's height
            float[] radii = { 0.08f, 0.11f, 0.14f, 0.16f };
            var postMeshes = new Mesh[radii.Length];
            for (int i = 0; i < radii.Length; i++)
                postMeshes[i] = Keep(MeshKit.Cylinder("dam_post", radii[i], 1f, 6, 1f));
            void Post(float x, float y0, float y1, float z, float r, float leanX, float leanZ)
            {
                int pi = 0;
                for (int i = 1; i < radii.Length; i++)
                    if (Mathf.Abs(radii[i] - r) < Mathf.Abs(radii[pi] - r))
                        pi = i;
                Add(2, postMeshes[pi], 0, Matrix4x4.TRS(new Vector3(x, y0, z), Quaternion.Euler(leanX, R(0f, 360f), leanZ), new Vector3(1f, Mathf.Max(0.3f, y1 - y0), 1f)));
            }

            // the caissons
            var caissons = new List<(float z0, float z1, float top, float w)>();
            float z = -len / 2f, stepH = 0f;
            int ci = 0;
            while (z < len / 2f - 0.5f)
            {
                float cl = Mathf.Min(R(8f, 14f), len / 2f - z);
                if (len / 2f - (z + cl) < 5f)
                    cl = len / 2f - z;                     // no stub caisson at the head
                float z1 = z + cl;
                if (ci % 3 == 0)
                    stepH = R(-0.1f, 0.1f);                // steps of a decimetre between runs of compartments
                float hi = Mathf.Max(0.6f, height + stepH + R(-0.08f, 0.08f));
                float wi = width + R(-0.3f, 0.3f);
                bool head = z1 >= len / 2f - 0.01f;
                float topLen = cl, topZc = (z + z1) / 2f;
                if (head && cl > 10f)
                {
                    // the last 8 m taper 0.4 m down into the shoal, mud on top
                    float body = cl - 8f;
                    Add(0, Keep(MeshKit.Box("dam_body", new Vector3(wi, hi - 0.1f, body), 0.45f)), 0, Matrix4x4.Translate(new Vector3(0f, 0f, z + body / 2f)));
                    var wedge = Keep(MeshKit.Wedge("dam_head", new Vector3(wi, hi - 0.02f, 8f), 0.4f, 0.45f));
                    Add(0, wedge, 0, Matrix4x4.Translate(new Vector3(0f, 0f, z1 - 4f)));
                    Add(4, wedge, 1, Matrix4x4.Translate(new Vector3(0f, 0f, z1 - 4f)));
                    topLen = body;
                    topZc = z + body / 2f;
                }
                else
                    Add(0, Keep(MeshKit.Box("dam_body", new Vector3(wi, hi - 0.1f, cl), 0.45f)), 0, Matrix4x4.Translate(new Vector3(0f, 0f, topZc)));
                // the top: a wicker mat, planks or bare mud, each caisson its own
                double pick = rnd.NextDouble();
                int sub = pick < 0.5 ? 1 : pick < 0.75 ? 3 : 4;
                float tile = sub == 1 ? 0.8f : sub == 3 ? 1.6f : 2.2f;
                Add(sub, Keep(MeshKit.Box("dam_top", new Vector3(wi + 0.25f, 0.12f, topLen + 0.1f), tile)), 0, Matrix4x4.Translate(new Vector3(0f, hi - 0.1f, topZc)));
                caissons.Add((z, z1, hi, wi));
                z = z1;
                ci++;
            }
            (float top, float w) AtZ(float zz)
            {
                foreach (var c in caissons)
                    if (zz >= c.z0 - 0.01f && zz <= c.z1 + 0.01f)
                        return (c.top, c.w);
                return (height, width);
            }

            // the transverse rows: at the root (the end-revetment near the bank, 1-1.5 m high), at every joint, and at
            // the head: 10-15 round posts 15-30 cm thick with 0-40 cm gaps, standing proud of the top
            void Row(float zz, float proud, float rMin, float rMax)
            {
                var (top, w) = AtZ(Mathf.Clamp(zz, -len / 2f + 0.1f, len / 2f - 0.1f));
                float x = -w / 2f + R(0.05f, 0.25f);
                while (x < w / 2f - 0.05f)
                {
                    float r = R(rMin, rMax);
                    Post(x, -0.4f, top + proud + R(-0.06f, 0.06f), zz + R(-0.04f, 0.04f), r, R(-2f, 2f), R(-2f, 2f));
                    x += 2f * r + R(0f, 0.4f);
                }
            }
            Row(-len / 2f + 0.3f, 0.6f, 0.12f, 0.16f);
            for (int i = 1; i < caissons.Count; i++)
                Row(caissons[i].z0, 0.3f, 0.12f, 0.16f);
            Row(len / 2f - 0.35f, 0.5f, 0.12f, 0.16f);
            // the wattle end panel 0.2 m back from the head's posts, a low parapet over the mat
            {
                var (top, w) = AtZ(len / 2f - 0.6f);
                Add(0, Keep(MeshKit.Box("dam_end_panel", new Vector3(w, 1.2f, 0.1f), 0.45f)), 0, Matrix4x4.Translate(new Vector3(0f, top - 1.05f, len / 2f - 0.6f)));
            }
            // the side posts: the first 30 m from the bank (period 1) heavier and closer with the tops 15 cm higher,
            // the rest 0.9-1.6 m apart, leaning a few degrees
            for (int side = -1; side <= 1; side += 2)
            {
                float zz = -len / 2f + 0.5f;
                while (zz < len / 2f - 0.5f)
                {
                    bool old = zz + len / 2f < 30f;
                    float r = old ? 0.15f : R(0.07f, 0.14f);
                    var (top, w) = AtZ(zz);
                    Post(side * (w / 2f + r + 0.02f), -0.4f, top + (old ? 0.15f : 0f) + R(0.3f, 0.6f), zz, r, R(-4f, 4f), R(-4f, 4f) * side);
                    zz += old ? R(0.6f, 0.8f) : R(0.9f, 1.6f);
                }
            }
            // the waterline: a dark wet band
            Add(5, Keep(MeshKit.Box("dam_waterline", new Vector3(width + 0.35f, 0.35f, len - 0.2f), 1f)), 0, Matrix4x4.Translate(new Vector3(0f, 0.475f, 0f)));

            // put it together: one submesh per material
            var subMeshes = new Mesh[6];
            for (int i = 0; i < 6; i++)
            {
                if (parts[i].Count == 0)
                    Add(i, Keep(MeshKit.Box("dam_none", new Vector3(0.02f, 0.02f, 0.02f), 1f)), 0, Matrix4x4.Translate(new Vector3(0f, -6f, 0f)));
                subMeshes[i] = new Mesh { indexFormat = IndexFormat.UInt32 };
                subMeshes[i].CombineMeshes(parts[i].ToArray(), true, true);
            }
            var m = new Mesh { name = $"Dam_{len:0}m", indexFormat = IndexFormat.UInt32 };
            var all = new CombineInstance[6];
            for (int i = 0; i < 6; i++)
                all[i] = new CombineInstance { mesh = subMeshes[i], subMeshIndex = 0, transform = Matrix4x4.identity };
            m.CombineMeshes(all, false, false);
            m.RecalculateBounds();
            foreach (var t in temp)
                Object.DestroyImmediate(t);
            foreach (var t in subMeshes)
                Object.DestroyImmediate(t);
            string path = $"{Dir}/Meshes/{m.name}_{key}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null)
                AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(m, path);
            DamMeshes[key] = m;
            return m;
        }

        // ------------------------------------------------------------------ the kit of prefabs

        /// <summary>The relief of a normal map, by whichever name the pipeline's shader gives it.</summary>
        static Material Bump(Material m, float scale)
        {
            if (m == null)
                return m;
            if (m.HasProperty("_NormalScale"))
                m.SetFloat("_NormalScale", scale);
            if (m.HasProperty("_BumpScale"))
                m.SetFloat("_BumpScale", scale);
            EditorUtility.SetDirty(m);
            return m;
        }

        static Kit MakeKit(System.Random rnd)
        {
            var k = new Kit();
            // the walls: wattle screens between paired posts, daubed (ED4 ch. IV 1.1); four tints, no planks on a dwelling
            k.walls = new[]
            {
                Bump(Mat("polyhaven", "clay_plaster", "Daub_pale", 0.08f, new Color(1.0f, 0.97f, 0.9f)), 1.5f),
                Bump(Mat("polyhaven", "clay_plaster", "Daub_ochre", 0.08f, new Color(0.86f, 0.74f, 0.55f)), 1.5f),
                Bump(Mat("ambientcg", "Wicker002", "Wattle_wall", 0.1f, new Color(0.9f, 0.85f, 0.72f)), 1.2f),
                Bump(Mat("polyhaven", "clay_plaster", "Daub_grey", 0.08f, new Color(0.7f, 0.64f, 0.52f)), 1.5f),
            };
            k.plankWall = Mat("polyhaven", "raw_plank_wall", "Plank_wall", 0.08f, new Color(0.8f, 0.76f, 0.68f));   // only in the river bed (ED3 allows planks there)
            k.thatch = new[]
            {
                Mat("polyhaven", "reed_roof_03", "Thatch_gold", 0.05f, new Color(1.0f, 0.92f, 0.7f)),
                Mat("polyhaven", "thatch_roof_angled", "Thatch_grey", 0.05f, new Color(0.72f, 0.72f, 0.66f)),
                Mat("polyhaven", "riet_01", "Thatch_dark", 0.05f, new Color(0.62f, 0.56f, 0.46f)),
            };
            k.wattle = Mat("ambientcg", "Wicker002", "Wattle", 0.1f);
            k.wattleFine = Mat("ambientcg", "Wicker002", "Wattle_fine", 0.1f, new Color(0.55f, 0.48f, 0.36f));
            k.mat = Mat("ambientcg", "Wicker003", "Wicker_mat", 0.05f, new Color(0.5f, 0.45f, 0.36f));
            k.damPlanks = Mat("polyhaven", "old_planks_02", "Dam_planks", 0.08f);
            k.damMud = Mat("polyhaven", "muddy_tracks", "Dam_mud", 0.12f);
            k.planks = Mat("polyhaven", "weathered_planks", "Planks");
            k.timber = Mat("polyhaven", "rough_wood", "Timber");
            k.darkTimber = Mat("polyhaven", "rough_wood", "Dark_timber", 0.1f, new Color(0.14f, 0.11f, 0.08f));
            k.piles = Mat("polyhaven", "bark_willow", "Piles");
            k.staves = Mat("polyhaven", "wooden_barrel", "Barrel", 0.1f, new Color(1.15f, 1.05f, 0.9f));
            k.hoop = Mat("ambientcg", "Wicker002", "Hoop", 0.1f, new Color(0.35f, 0.3f, 0.22f));
            k.stone = Mat("polyhaven", "dark_rock", "Basalt");
            k.linen = Mat("polyhaven", "rough_linen", "Linen", 0.1f, new Color(0.60f, 0.55f, 0.45f));    // unbleached
            k.net = Mat("ambientcg", "Wicker002", "Net", 0.1f, new Color(0.4f, 0.36f, 0.3f));
            k.fish = Mat("polyhaven", "rough_linen", "Fish", 0.3f, new Color(0.55f, 0.58f, 0.55f));
            k.logs = Mat("polyhaven", "wood_trunk_wall", "Logs");
            k.mudMat = Mat("polyhaven", "brown_mud_02", "Mud_heap");
            k.wetMud = Mat("polyhaven", "brown_mud_02", "Wet_mud", 0.4f, new Color(0.45f, 0.4f, 0.33f));
            k.ember = AssetLibrary.Glow($"{Dir}/Materials/Ember.mat", new Color(1f, 0.42f, 0.12f), 1.3f);
            k.flame = AssetLibrary.Glow($"{Dir}/Materials/Flame.mat", new Color(1f, 0.55f, 0.2f), 2.5f);
            k.inside = AssetLibrary.Glow($"{Dir}/Materials/Inside.mat", new Color(0.02f, 0.015f, 0.01f), 1f);
            k.smoke = HdrpMaterials.Particle($"{Dir}/Materials/Smoke.mat", "Smoke", new Color(0.42f, 0.42f, 0.46f, 0.6f));
            k.smokeDark = HdrpMaterials.Particle($"{Dir}/Materials/Smoke_dark.mat", "Smoke_dark", new Color(0.3f, 0.3f, 0.32f, 0.7f));
            Material Thatch()
            {
                // new thatch is gold for a season, then greys: 20% gold, 50% grey, 30% dark
                double p = rnd.NextDouble();
                return p < 0.2 ? k.thatch[0] : p < 0.7 ? k.thatch[1] : k.thatch[2];
            }

            // the narrow houses of the vicus: three sizes, each in three dresses, one in three with a half-hip
            var narrow = new List<GameObject>();
            float[][] sizes = { new[] { 5f, 12f }, new[] { 5.5f, 14f }, new[] { 6f, 16f } };
            for (int s = 0; s < sizes.Length; s++)
                for (int v = 0; v < 3; v++)
                    narrow.Add(HousePrefab(k, $"House_{sizes[s][0]}x{sizes[s][1]}_v{v}", sizes[s][0], sizes[s][1], 1.7f + 0.3f * v, 48f + 4f * v,
                                           k.walls[(s + v) % k.walls.Length], Thatch(), true, rnd, false, v == 2, false));
            k.narrow = narrow.ToArray();
            k.longHouse = new[]
            {
                HousePrefab(k, "House_6x24_v0", 6f, 24f, 2.1f, 50f, k.walls[0], Thatch(), true, rnd),
                HousePrefab(k, "House_6x26_v1", 6f, 26f, 2.0f, 47f, k.walls[2], Thatch(), true, rnd),
            };
            // the farmhouses: c. 800 the long walls were straight or only slightly curved (ED4 ch. IV 1.1.2); the fine
            // boat shapes are the Danish period's, from c. 850
            k.hall = new[]
            {
                FarmPrefab(k, "Farmhouse_9x22", 8.2f, 9f, 22f, 2.0f, 7.4f, k.walls[1], Thatch()),
                FarmPrefab(k, "Farmhouse_10x27", 9.2f, 10f, 27f, 2.0f, 8.5f, k.walls[0], Thatch()),
                FarmPrefab(k, "Farmhouse_11x30", 10.2f, 11f, 30f, 2.2f, 9.2f, k.walls[2], Thatch()),
                FarmPrefab(k, "Farmhouse_9.5x25_straight", 9.5f, 9.5f, 25f, 2.1f, 8f, k.walls[3], Thatch()),
            };
            // the storehouses on the dams: pile-founded, 2 m wattle walls, 4 m ridge; the tower-like short one by the
            // bank; and the warehouses of the new district, one of them two-storeyed with a loading door
            k.storehouse = new[]
            {
                HousePrefab(k, "Storehouse_4.6x9_v0", 4.6f, 9f, 2.0f, 42f, k.walls[2], k.thatch[1], false, rnd, true),
                HousePrefab(k, "Storehouse_4.6x8_v1", 4.6f, 8f, 2.0f, 45f, k.walls[2], k.thatch[0], false, rnd, true),
                HousePrefab(k, "Storehouse_4.6x6_tower", 4.6f, 6f, 3.4f, 50f, k.plankWall, k.thatch[2], false, rnd, true),
            };
            k.warehouse = new[]
            {
                HousePrefab(k, "Warehouse_4.6x10", 4.6f, 10f, 2.6f, 45f, k.walls[2], Thatch(), false, rnd, true),
                HousePrefab(k, "Warehouse_4.6x10_loft", 4.6f, 10f, 3.8f, 45f, k.walls[3], Thatch(), false, rnd, true, false, true),
            };
            k.granarySmall = GranaryPrefab(k, "Granary_small", 3.2f, 3.2f, 1.3f, false);
            k.granaryRick = GranaryPrefab(k, "Granary_small_rick", 3.2f, 3.2f, 1.3f, true);
            k.granaryHeavy = GranaryPrefab(k, "Granary_heavy_6x10", 6f, 10f, 0.9f, false);
            k.smithy = SmithyPrefab(k);
            k.shed = HousePrefab(k, "Shed_4x6", 4f, 6f, 1.6f, 40f, k.plankWall, k.thatch[1], false, rnd);
            k.church = new[] { ChurchPrefab(k, "Church_wooden_6x12", 6f, 12f), ChurchPrefab(k, "Church_Upkirika_7x15", 7f, 15f) };
            k.tents = new[] { TentPrefab(k, "Tent", false), TentPrefab(k, "Tent_open", true) };
            k.wells = new[] { WellPrefab(k, "Well_barrel", 0), WellPrefab(k, "Well_frame", 1), WellPrefab(k, "Well_trunk", 2) };
            k.hurdle = HurdlePrefab(k);
            k.postRow = PostRowPrefab(k);
            k.gate = GatePrefabMake(k);
            GatePrefab = k.gate;
            k.woodpile = SimplePrefab("Woodpile", MeshKit.Box("Woodpile", new Vector3(1.8f, 0.9f, 1.1f), 1.1f), k.logs);
            k.haystack = HaystackPrefab(k);
            k.midden = SimplePrefab("Midden", MeshKit.Cylinder("Midden", 1.6f, 0.6f, 10, 1.5f, true), k.mudMat, k.mudMat);
            k.rack = RackPrefab(k);
            k.querns = QuernPrefab(k);
            k.barrels = BarrelStackPrefab(k);
            k.barrel = SingleBarrelPrefab(k);
            k.crates = CratesPrefab(k);
            k.bales = SimplePrefab("Bales of cloth", MeshKit.Box("Bale", new Vector3(1.4f, 0.9f, 0.8f), 0.9f), k.linen);
            k.lantern = FireBasketPrefab(k);
            k.bonfire = BonfirePrefab(k);
            k.revetment = RevetmentPrefab(k);
            k.roadPlank = RoadPlankPrefab(k);
            k.gangway = GangwayPrefab(k);
            k.gangplank = SimplePrefab("Gangplank", MeshKit.Box("Gangplank", new Vector3(0.4f, 0.05f, 3.2f), 1.5f), k.planks);
            k.bollard = SimplePrefab("Bollard", MeshKit.Cylinder("Bollard", 0.16f, 1.4f, 8, 1f), k.piles, k.piles);
            k.mooring = SimplePrefab("Mooring post", MeshKit.Cylinder("Mooring_post_town", 0.12f, 2f, 7, 1f), k.piles, k.piles);
            k.timberStack = TimberStackPrefab(k);
            k.trough = TroughPrefab(k);
            k.trestle = TrestlePrefab(k);
            k.strake = SimplePrefab("Strake", MeshKit.Box("Strake", new Vector3(0.25f, 0.03f, 4.2f), 1.5f), k.planks);
            k.graveStone = SimplePrefab("Sarcophagus", MeshKit.Box("Sarcophagus", new Vector3(2.1f, 0.5f, 0.7f), 1f), Mat("polyhaven", "stone_01", "Limestone", 0.1f));
            var tuff = Mat("polyhaven", "stone_01", "Tuff", 0.1f, new Color(0.8f, 0.78f, 0.72f));
            k.tuff = SimplePrefab("Tuff block", MeshKit.Box("Tuff_block", new Vector3(0.6f, 0.4f, 0.4f), 0.6f), tuff);
            k.wallStub = SimplePrefab("Robbed wall stub", MeshKit.Box("Wall_stub", new Vector3(6f, 1.2f, 3f), 1f), tuff);
            return k;
        }

        static Mesh SaveMesh(Mesh m)
        {
            string path = $"{Dir}/Meshes/{m.name}.asset";
            var old = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (old != null)
                AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(m, path);
            return m;
        }

        static GameObject SavePrefab(GameObject go, string name)
        {
            var p = PrefabUtility.SaveAsPrefabAsset(go, $"{Dir}/Prefabs/{name}.prefab");
            Object.DestroyImmediate(go);
            return p;
        }

        static GameObject Part(Transform parent, string name, Mesh mesh, params Material[] mats)
        {
            var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false);
            go.GetComponent<MeshFilter>().sharedMesh = mesh;
            var use = new Material[mesh.subMeshCount];
            for (int i = 0; i < use.Length; i++)
                use[i] = mats[Mathf.Min(i, mats.Length - 1)];
            go.GetComponent<MeshRenderer>().sharedMaterials = use;
            return go;
        }

        static GameObject PartAt(Transform parent, string name, Mesh mesh, Vector3 at, Vector3 euler, params Material[] mats)
        {
            var go = Part(parent, name, mesh, mats);
            go.transform.SetLocalPositionAndRotation(at, Quaternion.Euler(euler));
            return go;
        }

        static GameObject SimplePrefab(string name, Mesh mesh, params Material[] mats)
        {
            var root = new GameObject(name);
            Part(root.transform, name, SaveMesh(mesh), mats);
            return SavePrefab(root, name);
        }

        static Light PointLight(Transform parent, string name, Vector3 at, Color colour, float range)
        {
            var go = new GameObject(name, typeof(Light));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = at;
            var l = go.GetComponent<Light>();
            l.type = LightType.Point;
            l.color = colour;
            l.range = range;
            l.intensity = 1f;
            l.shadows = LightShadows.None;
            return l;
        }

        /// <summary>
        /// A house of the excavated kind: paired wall posts proud of the daubed wattle, a wall plate and sill beams, a
        /// thatched roof at a pitch with thickness at the eaves, eaves boards, a ridge roll, a splash plinth of trodden
        /// mud, a real door opening with a timber frame in the gable facing the road (-z) or the long side; lived in,
        /// it has a low hearth glow seen through the door, a hearth light and smoke leaving by a gable top (no
        /// chimneys). `raised` puts it on piles with a plank floor (the dams' storehouses); `hip` gives a half-hip;
        /// `loft` a loading door up in the gable (the two-storey warehouse).
        /// </summary>
        static GameObject HousePrefab(Kit k, string name, float width, float length, float wallHeight, float pitchDeg, Material walls, Material thatch, bool lived, System.Random rnd,
                                      bool raised = false, bool hip = false, bool loft = false)
        {
            var root = new GameObject(name);
            float ridge = wallHeight + width / 2f * Mathf.Tan(pitchDeg * Mathf.Deg2Rad);
            float pitch = 1.4f + (float)rnd.NextDouble() * 0.6f, overhang = 0.8f, hipDepth = hip ? 1.5f : 0f;
            var mesh = SaveMesh(MeshKit.House(name + "_mesh", width, length, wallHeight, ridge, overhang, 1.1f, 2.5f, pitch, hipDepth, raised ? 0f : 0.8f, 0.22f, true));
            var body = Part(root.transform, "Walls and thatch", mesh, walls, thatch, k.timber);
            body.AddComponent<MeshCollider>().sharedMesh = mesh;
            // the ridge roll, the eaves boards, the plinth or the piles
            float rollLen = length + overhang * 1.2f - 2f * hipDepth + 0.1f;
            PartAt(root.transform, "Ridge roll", SaveMesh(MeshKit.Cylinder(name + "_ridge", 0.16f, rollLen, 8, 1f)), new Vector3(0f, ridge - 0.1f, -rollLen / 2f), new Vector3(90f, 0f, 0f), k.thatch[2], k.thatch[2]);
            var board = SaveMesh(MeshKit.Box(name + "_eave", new Vector3(0.08f, 0.25f, length + overhang * 1.2f), 1f));
            PartAt(root.transform, "Eaves board", board, new Vector3(width / 2f + overhang - 0.1f, wallHeight - overhang * 0.5f - 0.3f, 0f), Vector3.zero, k.timber);
            PartAt(root.transform, "Eaves board", board, new Vector3(-width / 2f - overhang + 0.1f, wallHeight - overhang * 0.5f - 0.3f, 0f), Vector3.zero, k.timber);
            if (!raised)
                PartAt(root.transform, "Splash plinth", SaveMesh(MeshKit.Box(name + "_plinth", new Vector3(width + 0.16f, 0.35f, length + 0.16f), 1.5f)), new Vector3(0f, -0.22f, 0f), Vector3.zero, k.mudMat);
            else
            {
                PartAt(root.transform, "Plank floor", SaveMesh(MeshKit.Box(name + "_floor", new Vector3(width + 0.1f, 0.12f, length + 0.1f), 1.5f)), new Vector3(0f, -0.12f, 0f), Vector3.zero, k.planks);
                var pile = SaveMesh(MeshKit.Cylinder(name + "_pile", 0.12f, 0.5f, 7, 1f));
                int rows = length > 8.5f ? 3 : 2, cols = 3;
                for (int i = 0; i < cols; i++)
                    for (int j = 0; j < rows; j++)
                        PartAt(root.transform, "Pile", pile, new Vector3((i - 1f) * (width / 2f - 0.3f), -0.6f, (j / (float)(rows - 1) - 0.5f) * (length - 1.2f)), Vector3.zero, k.piles, k.piles);
            }
            // the door: a dark opening with a timber frame, between the wall posts
            bool sideDoor = rnd.NextDouble() < 0.3 && lived;
            float dx = MeshKit.DoorSlot(width, pitch, rnd), dz = MeshKit.DoorSlot(length, pitch, rnd);
            Vector3 doorPos = sideDoor ? new Vector3(width / 2f, 0f, dz) : new Vector3(dx, 0f, -length / 2f);
            Quaternion doorRot = sideDoor ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.identity;
            var door = new GameObject("Door").transform;
            door.SetParent(root.transform, false);
            door.SetLocalPositionAndRotation(doorPos, doorRot);
            var opening = Part(door, "Opening", SaveMesh(MeshKit.Box(name + "_opening", new Vector3(0.95f, 1.75f, 0.5f), 1f)), k.inside);
            opening.transform.localPosition = new Vector3(0f, 0.02f, 0.2f);
            var frameMesh = SaveMesh(MeshKit.Box(name + "_jamb", new Vector3(0.14f, 1.85f, 0.14f), 1f));
            Part(door, "Jamb", frameMesh, k.timber).transform.localPosition = new Vector3(-0.55f, 0f, -0.08f);
            Part(door, "Jamb", frameMesh, k.timber).transform.localPosition = new Vector3(0.55f, 0f, -0.08f);
            var lintel = Part(door, "Lintel", SaveMesh(MeshKit.Box(name + "_lintel", new Vector3(1.3f, 0.14f, 0.16f), 1f)), k.timber);
            lintel.transform.localPosition = new Vector3(0f, 1.85f, -0.08f);
            if (loft)
            {
                // the loading door up in the river gable (+z, toward the water on the dams), a beam over it for the tackle
                float lx = MeshKit.DoorSlot(width, pitch, rnd);
                PartAt(root.transform, "Loading door", SaveMesh(MeshKit.Box(name + "_loft_door", new Vector3(1.0f, 1.4f, 0.3f), 1f)), new Vector3(lx, 2.2f, length / 2f - 0.2f), Vector3.zero, k.inside);
                PartAt(root.transform, "Hoist beam", SaveMesh(MeshKit.Box(name + "_hoist", new Vector3(0.14f, 0.14f, 1.2f), 1f)), new Vector3(lx, 3.7f, length / 2f + 0.3f), Vector3.zero, k.timber);
            }
            if (lived)
            {
                // the hearth seen low through the open door (houses had no windows), its light, and the smoke leaving
                // through one gable top
                var glow = Part(door, "Hearth glow", SaveMesh(MeshKit.Box(name + "_glow", new Vector3(0.5f, 0.4f, 0.04f), 1f)), k.ember);
                glow.transform.localPosition = new Vector3(0f, 0.3f, 0.42f);
                PointLight(door, "Hearth light", new Vector3(0f, 1.1f, -0.9f), new Color(1f, 0.5f, 0.2f), 7f).intensity = 0.6f;
                float gz = rnd.NextDouble() < 0.5 ? length / 2f - 0.6f : -length / 2f + 0.6f;
                Smoke(root.transform, new Vector3(0f, ridge - 0.35f, gz), k.smoke, 10f);
            }
            return SavePrefab(root, name);
        }

        /// <summary>
        /// A farmhouse: straight or slightly bowed long walls with their posts, a hipped thatch roof, a splash plinth,
        /// two opposed doors in the long walls a third of the way along (the dwelling end), the byre's wide door in
        /// the far gable; a hearth at one door, smoke at both ends of the ridge.
        /// </summary>
        static GameObject FarmPrefab(Kit k, string name, float endWidth, float midWidth, float length, float wallHeight, float ridge, Material walls, Material thatch)
        {
            var root = new GameObject(name);
            var mesh = SaveMesh(MeshKit.BowedHall(name + "_mesh", endWidth, midWidth, length, wallHeight, ridge, 0.9f, 1.1f, 3f, 0.3f, 1.8f));
            var body = Part(root.transform, "Walls and thatch", mesh, walls, thatch, k.mudMat, k.timber);
            body.AddComponent<MeshCollider>().sharedMesh = mesh;
            float dz = -length / 6f;
            float wAt = Mathf.Lerp(endWidth, midWidth, Mathf.Sin((dz / length + 0.5f) * Mathf.PI)) / 2f;
            for (int side = -1; side <= 1; side += 2)
            {
                var door = new GameObject(side > 0 ? "Door (east)" : "Door (west)").transform;
                door.SetParent(root.transform, false);
                door.SetLocalPositionAndRotation(new Vector3(side * (wAt - 0.3f), 0f, dz), Quaternion.Euler(0f, side * 90f, 0f));
                Part(door, "Opening", SaveMesh(MeshKit.Box(name + "_opening", new Vector3(1.0f, 1.8f, 0.6f), 1f)), k.inside).transform.localPosition = new Vector3(0f, 0.02f, 0.25f);
                var jamb = SaveMesh(MeshKit.Box(name + "_jamb", new Vector3(0.14f, 1.9f, 0.14f), 1f));
                Part(door, "Jamb", jamb, k.timber).transform.localPosition = new Vector3(-0.58f, 0f, -0.08f);
                Part(door, "Jamb", jamb, k.timber).transform.localPosition = new Vector3(0.58f, 0f, -0.08f);
                if (side > 0)
                {
                    var glow = Part(door, "Hearth glow", SaveMesh(MeshKit.Box(name + "_glow", new Vector3(0.5f, 0.4f, 0.04f), 1f)), k.ember);
                    glow.transform.localPosition = new Vector3(0f, 0.3f, 0.5f);
                    PointLight(door, "Hearth light", new Vector3(0f, 1.2f, -1.2f), new Color(1f, 0.5f, 0.2f), 9f).intensity = 0.6f;
                }
            }
            var byre = Part(root.transform, "Byre door", SaveMesh(MeshKit.Box(name + "_byre", new Vector3(2.2f, 2.0f, 0.5f), 1f)), k.inside);
            byre.transform.localPosition = new Vector3(0f, 0.02f, length / 2f - 0.1f);
            Smoke(root.transform, new Vector3(0f, ridge - 0.35f, -length * 0.3f + 0.5f), k.smoke, 10f);
            Smoke(root.transform, new Vector3(0f, ridge - 0.35f, length * 0.3f - 0.5f), k.smoke, 6f);
            return SavePrefab(root, name);
        }

        /// <summary>A granary on posts, wattle-walled; the small ones read as rickstands, so some carry a stack under the floor.</summary>
        static GameObject GranaryPrefab(Kit k, string name, float w, float l, float legs, bool rick)
        {
            var root = new GameObject(name);
            var postMesh = SaveMesh(MeshKit.Box(name + "_post", new Vector3(0.25f, legs + 0.3f, 0.25f), 1f));
            int nx = w > 4f ? 3 : 2, nz = l > 6f ? 3 : 2;
            for (int i = 0; i < nx; i++)
                for (int j = 0; j < nz; j++)
                    Part(root.transform, "Post", postMesh, k.timber).transform.localPosition = new Vector3((i / (float)(nx - 1) - 0.5f) * (w - 0.5f), -0.3f, (j / (float)(nz - 1) - 0.5f) * (l - 0.5f));
            var store = Part(root.transform, "Store", SaveMesh(MeshKit.House(name + "_store", w, l, 1.6f, 1.6f + w * 0.45f, 0.4f, 1.1f, 2f, 0f, 0f, 0.1f, 0.15f)), k.wattle, k.thatch[1]);
            store.transform.localPosition = new Vector3(0f, legs, 0f);
            if (rick)
                PartAt(root.transform, "Rick", SaveMesh(MeshKit.Cylinder(name + "_rick", Mathf.Min(w, l) / 2f - 0.3f, legs - 0.15f, 10, 1.5f, false, 0.5f)), new Vector3(0f, 0f, 0f), Vector3.zero, k.thatch[1], k.thatch[1]);
            return SavePrefab(root, name);
        }

        static GameObject SmithyPrefab(Kit k)
        {
            var root = new GameObject("Smithy");
            var mesh = SaveMesh(MeshKit.House("Smithy_mesh", 6f, 10f, 2.2f, 4.4f, 0.8f, 1.1f, 2.5f, 1.6f, 0f, 0.8f, 0.22f));
            Part(root.transform, "Walls and thatch", mesh, k.walls[2], k.thatch[2], k.timber).AddComponent<MeshCollider>().sharedMesh = mesh;
            PartAt(root.transform, "Splash plinth", SaveMesh(MeshKit.Box("Smithy_plinth", new Vector3(6.16f, 0.35f, 10.16f), 1.5f)), new Vector3(0f, -0.22f, 0f), Vector3.zero, k.mudMat);
            // the open front with the forge glowing inside
            var opening = Part(root.transform, "Open front", SaveMesh(MeshKit.Box("Smithy_front", new Vector3(3.2f, 2.0f, 0.5f), 1f)), k.inside);
            opening.transform.localPosition = new Vector3(0f, 0.02f, -4.85f);
            var forge = Part(root.transform, "Forge", SaveMesh(MeshKit.Box("Smithy_forge", new Vector3(1.2f, 0.9f, 0.6f), 1f)), k.ember);
            forge.transform.localPosition = new Vector3(0.8f, 0.1f, -3.4f);
            PointLight(root.transform, "Forge light", new Vector3(0.8f, 1.2f, -3.2f), new Color(1f, 0.45f, 0.15f), 10f).intensity = 2.5f;
            var anvil = Part(root.transform, "Anvil", SaveMesh(MeshKit.Box("Smithy_anvil", new Vector3(0.7f, 0.7f, 0.3f), 1f)), k.stone);
            anvil.transform.localPosition = new Vector3(-1f, 0f, -3.8f);
            Smoke(root.transform, new Vector3(0f, 4.05f, -4.4f), k.smokeDark, 16f);
            return SavePrefab(root, "Smithy");
        }

        /// <summary>
        /// A small wooden church c. 800: nave and narrower chancel, wattle-and-daub walls with their posts, thatch at
        /// the houses' pitch, one timber cross on the west gable. No tower: none is attested (the 'bell-cage' G4 is now
        /// read as a granary).
        /// </summary>
        static GameObject ChurchPrefab(Kit k, string name, float w, float l)
        {
            var root = new GameObject(name);
            float ridge = 2.6f + w / 2f * Mathf.Tan(50f * Mathf.Deg2Rad);
            var mesh = SaveMesh(MeshKit.House(name + "_mesh", w, l, 2.6f, ridge, 0.7f, 1.1f, 2.5f, 1.6f, 0f, 0.8f, 0.22f));
            Part(root.transform, "Nave", mesh, k.walls[0], k.thatch[1], k.timber).AddComponent<MeshCollider>().sharedMesh = mesh;
            float cw = w * 0.65f, cridge = 2.2f + cw / 2f * Mathf.Tan(50f * Mathf.Deg2Rad);
            var chancel = Part(root.transform, "Chancel", SaveMesh(MeshKit.House(name + "_chancel", cw, 4f, 2.2f, cridge, 0.5f, 1.1f, 2.5f, 1.6f, 0f, 0.8f, 0.22f)), k.walls[0], k.thatch[1], k.timber);
            chancel.transform.localPosition = new Vector3(0f, 0f, l / 2f + 1.9f);
            PartAt(root.transform, "Splash plinth", SaveMesh(MeshKit.Box(name + "_plinth", new Vector3(w + 0.16f, 0.35f, l + 0.16f), 1.5f)), new Vector3(0f, -0.22f, 0f), Vector3.zero, k.mudMat);
            PartAt(root.transform, "Ridge roll", SaveMesh(MeshKit.Cylinder(name + "_ridge", 0.16f, l + 0.9f, 8, 1f)), new Vector3(0f, ridge - 0.1f, -(l + 0.9f) / 2f), new Vector3(90f, 0f, 0f), k.thatch[2], k.thatch[2]);
            PartAt(root.transform, "Cross", SaveMesh(MeshKit.Box(name + "_cross", new Vector3(0.1f, 1.2f, 0.1f), 1f)), new Vector3(0f, ridge - 0.25f, -l / 2f - 0.35f), Vector3.zero, k.timber);
            PartAt(root.transform, "Cross arm", SaveMesh(MeshKit.Box(name + "_crossarm", new Vector3(0.7f, 0.1f, 0.1f), 1f)), new Vector3(0f, ridge + 0.5f, -l / 2f - 0.35f), Vector3.zero, k.timber);
            var door = Part(root.transform, "Door", SaveMesh(MeshKit.Box(name + "_door", new Vector3(1.2f, 2.2f, 0.5f), 1f)), k.inside);
            door.transform.localPosition = new Vector3(0f, 0.02f, -l / 2f + 0.15f);
            return SavePrefab(root, name);
        }

        /// <summary>
        /// A trader's ridge tent of unbleached linen, 4 x 5.5 m, open at the ends, on two crossed pole pairs and a ridge
        /// pole, with six guy ropes to pegs; `open` rolls one side up and shows the dark inside. The period's tents are
        /// ridge tents on crossed poles (a guess from the finds elsewhere; none is excavated at Dorestad).
        /// </summary>
        static GameObject TentPrefab(Kit k, string name, bool open)
        {
            var root = new GameObject(name);
            float wall = open ? 0.9f : 0.1f;
            var mesh = SaveMesh(MeshKit.House(name + "_mesh", 4f, 5.5f, wall, 2.3f, 0.15f, 1.5f, 1.5f, 0f, 0f, 0f, 0f, false));
            Part(root.transform, "Cloth", mesh, k.linen, k.linen);
            if (open)
                PartAt(root.transform, "Inside", SaveMesh(MeshKit.Box(name + "_inside", new Vector3(3.6f, 0.85f, 0.3f), 1f)), new Vector3(0f, 0.02f, -2.6f), Vector3.zero, k.inside);
            var pole = SaveMesh(MeshKit.Cylinder(name + "_pole", 0.05f, 2.6f, 6, 1f));
            for (int e = -1; e <= 1; e += 2)
            {
                PartAt(root.transform, "Pole", pole, new Vector3(0.45f, 0f, e * 2.7f), new Vector3(0f, 0f, 20f), k.timber, k.timber);
                PartAt(root.transform, "Pole", pole, new Vector3(-0.45f, 0f, e * 2.7f), new Vector3(0f, 0f, -20f), k.timber, k.timber);
            }
            PartAt(root.transform, "Ridge pole", SaveMesh(MeshKit.Cylinder(name + "_ridge", 0.05f, 6f, 6, 1f)), new Vector3(0f, 2.34f, -3f), new Vector3(90f, 0f, 0f), k.timber, k.timber);
            var rope = SaveMesh(MeshKit.Box(name + "_rope", new Vector3(0.015f, 0.015f, 2.05f), 1f));
            var peg = SaveMesh(MeshKit.Box(name + "_peg", new Vector3(0.05f, 0.35f, 0.05f), 1f));
            for (int side = -1; side <= 1; side += 2)
                for (int i = -1; i <= 1; i++)
                {
                    float z = i * 2.2f;
                    PartAt(root.transform, "Guy rope", rope, new Vector3(side * 2.4f, 0.47f, z), new Vector3(27.8f, side * 90f, 0f), k.darkTimber);
                    PartAt(root.transform, "Peg", peg, new Vector3(side * 3.3f, -0.1f, z), new Vector3(0f, 0f, -side * 20f), k.timber);
                }
            return SavePrefab(root, name);
        }

        /// <summary>
        /// A well as the excavations found them: a 3 m wine barrel (0.66 m across, willow hoops) sunk upright so its
        /// rim and 40 cm of staves show, a plank frame round the top; kind 1 with a bucket frame over it; kind 2 a
        /// hollowed tree trunk (18+ of those). Origin at the ground.
        /// </summary>
        static GameObject WellPrefab(Kit k, string name, int kind)
        {
            var root = new GameObject(name);
            if (kind == 2)
            {
                PartAt(root.transform, "Trunk", SaveMesh(MeshKit.Cylinder(name + "_trunk", 0.35f, 2.5f, 12, 1.2f)), new Vector3(0f, -2.1f, 0f), Vector3.zero, k.piles, k.darkTimber);
                PartAt(root.transform, "Rim", SaveMesh(MeshKit.Cylinder(name + "_rim", 0.4f, 0.1f, 12, 1f)), new Vector3(0f, 0.35f, 0f), Vector3.zero, k.timber, k.darkTimber);
            }
            else
            {
                PartAt(root.transform, "Barrel", SaveMesh(MeshKit.Cylinder(name + "_barrel", 0.33f, 3f, 14, 1.2f, true)), new Vector3(0f, -2.6f, 0f), Vector3.zero, k.staves, k.darkTimber);
                PartAt(root.transform, "Hoop", SaveMesh(MeshKit.Cylinder(name + "_hoop", 0.36f, 0.06f, 14, 1f)), new Vector3(0f, 0.25f, 0f), Vector3.zero, k.hoop, k.hoop);
                var plank = SaveMesh(MeshKit.Box(name + "_plank", new Vector3(0.12f, 0.12f, 0.9f), 1f));
                PartAt(root.transform, "Frame", plank, new Vector3(0.45f, 0.4f, 0f), Vector3.zero, k.planks);
                PartAt(root.transform, "Frame", plank, new Vector3(-0.45f, 0.4f, 0f), Vector3.zero, k.planks);
                PartAt(root.transform, "Frame", plank, new Vector3(0f, 0.4f, 0.45f), new Vector3(0f, 90f, 0f), k.planks);
                PartAt(root.transform, "Frame", plank, new Vector3(0f, 0.4f, -0.45f), new Vector3(0f, 90f, 0f), k.planks);
            }
            if (kind == 1)
            {
                var post = SaveMesh(MeshKit.Box(name + "_post", new Vector3(0.12f, 1.8f, 0.12f), 1f));
                PartAt(root.transform, "Post", post, new Vector3(0.7f, 0f, 0f), Vector3.zero, k.timber);
                PartAt(root.transform, "Post", post, new Vector3(-0.7f, 0f, 0f), Vector3.zero, k.timber);
                PartAt(root.transform, "Crossbar", SaveMesh(MeshKit.Box(name + "_bar", new Vector3(1.6f, 0.1f, 0.1f), 1f)), new Vector3(0f, 1.75f, 0f), Vector3.zero, k.timber);
                PartAt(root.transform, "Rope", SaveMesh(MeshKit.Box(name + "_rope", new Vector3(0.02f, 0.6f, 0.02f), 1f)), new Vector3(0f, 1.15f, 0f), Vector3.zero, k.darkTimber);
                PartAt(root.transform, "Bucket", SaveMesh(MeshKit.Box(name + "_bucket", new Vector3(0.3f, 0.3f, 0.3f), 1f)), new Vector3(0f, 0.85f, 0f), Vector3.zero, k.staves);
            }
            return SavePrefab(root, name);
        }

        /// <summary>A wattle hurdle: seven stakes with the weave between them at a 45 cm tile, the stakes 20 cm above the panel.</summary>
        static GameObject HurdlePrefab(Kit k)
        {
            var root = new GameObject("Hurdle");
            PartAt(root.transform, "Wattle", SaveMesh(MeshKit.Box("Hurdle_wattle", new Vector3(2.4f, 0.9f, 0.06f), 0.45f)), new Vector3(0f, 0.05f, 0f), Vector3.zero, k.wattleFine);
            var stake = SaveMesh(MeshKit.Cylinder("Hurdle_stake", 0.03f, 1.15f, 5, 1f));
            for (int i = 0; i < 7; i++)
                PartAt(root.transform, "Stake", stake, new Vector3(-1.2f + 0.4f * i, 0f, 0f), Vector3.zero, k.timber, k.darkTimber);
            PartAt(root.transform, "Post", SaveMesh(MeshKit.Box("Hurdle_post", new Vector3(0.12f, 1.2f, 0.12f), 1f)), new Vector3(-1.2f, -0.1f, 0f), Vector3.zero, k.timber);
            return SavePrefab(root, "Hurdle");
        }

        /// <summary>A row of posts, the other excavated boundary (cemeteries, some yards): 1.1 m posts every 0.5 m, no rails.</summary>
        static GameObject PostRowPrefab(Kit k)
        {
            var root = new GameObject("Postrow");
            var post = SaveMesh(MeshKit.Cylinder("Postrow_post", 0.07f, 1.1f, 6, 1f));
            for (int i = 0; i < 5; i++)
                PartAt(root.transform, "Post", post, new Vector3(-1.2f + 0.5f * i, -0.05f, 0f), new Vector3(0f, i * 61f, 0f), k.timber, k.darkTimber);
            return SavePrefab(root, "Postrow");
        }

        static GameObject GatePrefabMake(Kit k)
        {
            var root = new GameObject("Gate");
            var post = SaveMesh(MeshKit.Box("Gate_post", new Vector3(0.14f, 1.6f, 0.14f), 1f));
            PartAt(root.transform, "Gatepost", post, new Vector3(-1.1f, -0.1f, 0f), Vector3.zero, k.timber);
            PartAt(root.transform, "Gatepost", post, new Vector3(1.1f, -0.1f, 0f), Vector3.zero, k.timber);
            return SavePrefab(root, "Gate");
        }

        /// <summary>A haystack: a tapering stack and a cone of grey thatch on a pole, with props leaning on it; PutStack gives it a height of its own.</summary>
        static GameObject HaystackPrefab(Kit k)
        {
            var root = new GameObject("Haystack");
            Part(root.transform, "Stack", SaveMesh(MeshKit.Cylinder("Haystack_body", 1.5f, 2.2f, 12, 1.5f, false, 1.0f)), k.thatch[1], k.thatch[1]);
            PartAt(root.transform, "Cone", SaveMesh(MeshKit.Cylinder("Haystack_cone", 1.1f, 1.3f, 12, 1.5f, false, 0.15f)), new Vector3(0f, 2.2f, 0f), Vector3.zero, k.thatch[1], k.thatch[1]);
            Part(root.transform, "Pole", SaveMesh(MeshKit.Box("Haystack_pole", new Vector3(0.12f, 4.0f, 0.12f), 1f)), k.timber);
            var prop = SaveMesh(MeshKit.Cylinder("Haystack_prop", 0.04f, 2.6f, 5, 1f));
            for (int i = 0; i < 4; i++)
            {
                float a = i * 97f;
                Vector3 at = new Vector3(Mathf.Sin(a * Mathf.Deg2Rad) * 2.2f, 0f, Mathf.Cos(a * Mathf.Deg2Rad) * 2.2f);
                PartAt(root.transform, "Prop", prop, at, new Vector3(25f, a + 180f, 0f), k.timber, k.darkTimber);
            }
            return SavePrefab(root, "Haystack");
        }

        /// <summary>A drying rack: a net of fine wicker weave hung from the bar, five fish on it.</summary>
        static GameObject RackPrefab(Kit k)
        {
            var root = new GameObject("Drying rack");
            var post = SaveMesh(MeshKit.Box("Rack_post", new Vector3(0.1f, 2.0f, 0.1f), 1f));
            Part(root.transform, "Post", post, k.timber).transform.localPosition = new Vector3(-1.5f, 0f, 0f);
            Part(root.transform, "Post", post, k.timber).transform.localPosition = new Vector3(1.5f, 0f, 0f);
            PartAt(root.transform, "Bar", SaveMesh(MeshKit.Box("Rack_bar", new Vector3(3.2f, 0.08f, 0.08f), 1f)), new Vector3(0f, 1.9f, 0f), Vector3.zero, k.timber);
            PartAt(root.transform, "Net", SaveMesh(MeshKit.Box("Rack_net", new Vector3(2.8f, 1.1f, 0.03f), 0.3f)), new Vector3(0f, 0.8f, 0f), Vector3.zero, k.net);
            var fish = SaveMesh(MeshKit.Box("Rack_fish", new Vector3(0.08f, 0.3f, 0.03f), 1f));
            for (int i = 0; i < 5; i++)
                PartAt(root.transform, "Fish", fish, new Vector3(-1.1f + i * 0.55f, 1.55f, 0.04f), new Vector3(0f, 0f, 180f + (i % 2 == 0 ? 6f : -6f)), k.fish);
            return SavePrefab(root, "Rack");
        }

        /// <summary>Mayen quern roughouts, 45-50 cm across, in stacks of 8-12.</summary>
        static GameObject QuernPrefab(Kit k)
        {
            var root = new GameObject("Mayen quernstones (stacked)");
            var q = SaveMesh(MeshKit.Cylinder("Quernstone", 0.24f, 0.08f, 16, 0.6f));
            var rnd = new System.Random(3);
            for (int stack = 0; stack < 3; stack++)
                for (int i = 0, n = 8 + rnd.Next(5); i < n; i++)
                {
                    var p = Part(root.transform, "Quern", q, k.stone, k.stone);
                    float s = 0.92f + (float)rnd.NextDouble() * 0.12f;
                    p.transform.localPosition = new Vector3(stack * 0.58f - 0.58f, i * 0.082f, (float)rnd.NextDouble() * 0.04f);
                    p.transform.localRotation = Quaternion.Euler(0f, (float)rnd.NextDouble() * 360f, 0f);
                    p.transform.localScale = new Vector3(s, 1f, s);
                }
            return SavePrefab(root, "Querns");
        }

        static Mesh barrelMesh, hoopMesh, chockMesh;

        /// <summary>A Rhineland wine barrel (3 m by 0.66 m, willow hoops) lying along x with its bottom at y = 0, under a parent.</summary>
        static void BarrelPart(Kit k, Transform parent, Vector3 at, float yawDeg)
        {
            if (barrelMesh == null)
                barrelMesh = SaveMesh(MeshKit.Cylinder("Wine_barrel", 0.33f, 3f, 14, 1.2f, true, -1f, true));
            if (hoopMesh == null)
                hoopMesh = SaveMesh(MeshKit.Cylinder("Barrel_hoop", 0.36f, 0.06f, 14, 1f));
            var b = new GameObject("Barrel").transform;
            b.SetParent(parent, false);
            b.SetLocalPositionAndRotation(at + new Vector3(0f, 0.37f, 0f), Quaternion.Euler(0f, yawDeg, 0f));
            var body = new GameObject("Staves").transform;
            body.SetParent(b, false);
            body.SetLocalPositionAndRotation(new Vector3(1.5f, 0f, 0f), Quaternion.Euler(0f, 0f, 90f));
            Part(body, "Staves", barrelMesh, k.staves, k.darkTimber);
            foreach (float h in new[] { 0.35f, 1.05f, 1.95f, 2.65f })
                PartAt(body, "Hoop", hoopMesh, new Vector3(0f, h, 0f), Vector3.zero, k.hoop, k.hoop);
        }

        static GameObject BarrelStackPrefab(Kit k)
        {
            var root = new GameObject("Wine barrels from the Rhineland");
            if (chockMesh == null)
                chockMesh = SaveMesh(MeshKit.Box("Barrel_chock", new Vector3(0.3f, 0.15f, 0.15f), 1f));
            foreach (float z in new[] { -0.78f, 0f, 0.78f })
                BarrelPart(k, root.transform, new Vector3(0f, 0f, z), 0f);
            foreach (float z in new[] { -0.39f, 0.39f })
                BarrelPart(k, root.transform, new Vector3(0f, 0.64f, z), 0f);
            foreach (float x in new[] { -1.1f, 1.1f })
                foreach (float z in new[] { -1.1f, 1.1f })
                    PartAt(root.transform, "Chock", chockMesh, new Vector3(x, 0f, z), Vector3.zero, k.timber);
            return SavePrefab(root, "Barrels");
        }

        static GameObject SingleBarrelPrefab(Kit k)
        {
            var root = new GameObject("Wine barrel on its side");
            if (chockMesh == null)
                chockMesh = SaveMesh(MeshKit.Box("Barrel_chock", new Vector3(0.3f, 0.15f, 0.15f), 1f));
            BarrelPart(k, root.transform, Vector3.zero, 0f);
            PartAt(root.transform, "Chock", chockMesh, new Vector3(1.1f, 0f, 0.3f), Vector3.zero, k.timber);
            return SavePrefab(root, "Barrel");
        }

        static GameObject CratesPrefab(Kit k)
        {
            var root = new GameObject("Pottery in straw crates");
            var c = SaveMesh(MeshKit.Box("Crate", new Vector3(0.9f, 0.6f, 0.7f), 0.9f));
            Part(root.transform, "Crate", c, k.planks).transform.localPosition = new Vector3(-0.5f, 0f, 0f);
            Part(root.transform, "Crate", c, k.planks).transform.localPosition = new Vector3(0.5f, 0f, 0.1f);
            Part(root.transform, "Crate", c, k.planks).transform.localPosition = new Vector3(0f, 0.6f, 0.05f);
            return SavePrefab(root, "Crates");
        }

        static Mesh flameSmall, flameBig;

        /// <summary>Three crossed tongues of flame, named Flame so TownLife can hide them by day and flicker them.</summary>
        static void Flames(Kit k, Transform parent, Vector3 at, bool big)
        {
            if (flameSmall == null)
                flameSmall = SaveMesh(MeshKit.Flame("Flame_small", 0.22f, 0.25f, 1f));
            if (flameBig == null)
                flameBig = SaveMesh(MeshKit.Flame("Flame_big", 0.9f, 2.2f, 1f));
            for (int i = 0; i < 3; i++)
                PartAt(parent, "Flame", big ? flameBig : flameSmall, at, new Vector3(0f, i * 60f, 0f), k.flame);
        }

        /// <summary>
        /// A fire basket on a post: eight dark bars round an ember block with the flames above, a light. Open fire
        /// baskets and hearths are what lit a Carolingian harbour; a basket on a post is a guess at its form.
        /// </summary>
        static GameObject FireBasketPrefab(Kit k)
        {
            var root = new GameObject("Fire basket");
            Part(root.transform, "Post", SaveMesh(MeshKit.Cylinder("Basket_post", 0.07f, 1.2f, 7, 1f)), k.timber, k.darkTimber);
            var bar = SaveMesh(MeshKit.Box("Basket_bar", new Vector3(0.03f, 0.35f, 0.03f), 1f));
            for (int i = 0; i < 8; i++)
            {
                float a = i * 45f * Mathf.Deg2Rad;
                PartAt(root.transform, "Bar", bar, new Vector3(Mathf.Sin(a) * 0.18f, 1.2f, Mathf.Cos(a) * 0.18f), new Vector3(0f, i * 45f, 0f), k.darkTimber);
            }
            PartAt(root.transform, "Embers", SaveMesh(MeshKit.Box("Basket_embers", new Vector3(0.26f, 0.14f, 0.26f), 1f)), new Vector3(0f, 1.22f, 0f), Vector3.zero, k.ember);
            Flames(k, root.transform, new Vector3(0f, 1.32f, 0f), false);
            PointLight(root.transform, "Basket light", new Vector3(0f, 1.6f, 0f), new Color(1f, 0.58f, 0.28f), 8f);
            return SavePrefab(root, "Fire basket");
        }

        /// <summary>A big open fire: logs leaning together, embers, tall flames, its light and smoke.</summary>
        static GameObject BonfirePrefab(Kit k)
        {
            var root = new GameObject("Fire on the beach");
            var log = SaveMesh(MeshKit.Cylinder("Fire_log", 0.12f, 2.6f, 6, 1f));
            for (int i = 0; i < 10; i++)
            {
                var p = Part(root.transform, "Log", log, k.timber, k.darkTimber);
                p.transform.localRotation = Quaternion.Euler(28f, i * 36f, 0f);
                p.transform.localPosition = Quaternion.Euler(0f, i * 36f, 0f) * new Vector3(0f, 0f, -0.9f);
            }
            PartAt(root.transform, "Embers", SaveMesh(MeshKit.Box("Fire_embers", new Vector3(0.8f, 0.3f, 0.8f), 1f)), new Vector3(0f, 0.1f, 0f), Vector3.zero, k.ember);
            Flames(k, root.transform, new Vector3(0f, 0.3f, 0f), true);
            PointLight(root.transform, "Firelight", new Vector3(0f, 2f, 0f), new Color(1f, 0.55f, 0.25f), 30f).intensity = 6f;
            var ps = Smoke(root.transform, new Vector3(0f, 2.5f, 0f), k.smokeDark, 14f);
            var main = ps.main;
            main.startSize = new ParticleSystem.MinMaxCurve(1.6f, 2.6f);
            return SavePrefab(root, "Fire on the beach");
        }

        /// <summary>A 2.4 m panel of the bank revetment: fine wattle with four posts; its base is set 0.6 m below the levee edge.</summary>
        static GameObject RevetmentPrefab(Kit k)
        {
            var root = new GameObject("Revetment");
            PartAt(root.transform, "Wattle", SaveMesh(MeshKit.Box("Revetment_wattle", new Vector3(2.4f, 0.9f, 0.1f), 0.45f)), Vector3.zero, Vector3.zero, k.wattleFine);
            var post = SaveMesh(MeshKit.Cylinder("Revetment_post", 0.06f, 1.3f, 6, 1f));
            for (int i = 0; i < 4; i++)
                PartAt(root.transform, "Post", post, new Vector3(-0.9f + 0.6f * i, -0.2f, 0.1f), new Vector3(0f, i * 83f, 0f), k.piles, k.darkTimber);
            return SavePrefab(root, "Revetment");
        }

        /// <summary>A 2.4 m length of the plank road on two posts (2 m wide, as the excavated bank road).</summary>
        static GameObject RoadPlankPrefab(Kit k)
        {
            var root = new GameObject("Plank road");
            Part(root.transform, "Planks", SaveMesh(MeshKit.Box("Road_planks", new Vector3(2f, 0.08f, 2.4f), 1.5f)), k.planks);
            var post = SaveMesh(MeshKit.Cylinder("Road_post", 0.07f, 0.6f, 6, 1f));
            PartAt(root.transform, "Post", post, new Vector3(0.8f, -0.5f, 0f), Vector3.zero, k.piles, k.darkTimber);
            PartAt(root.transform, "Post", post, new Vector3(-0.8f, -0.5f, 0f), Vector3.zero, k.piles, k.darkTimber);
            return SavePrefab(root, "Plank road");
        }

        /// <summary>A plank gangway from a dam head down to the water: 4.5 m with three cross battens, its origin at the head's end.</summary>
        static GameObject GangwayPrefab(Kit k)
        {
            var root = new GameObject("Gangway");
            PartAt(root.transform, "Planks", SaveMesh(MeshKit.Box("Gangway_planks", new Vector3(0.7f, 0.08f, 4.5f), 1.5f)), new Vector3(0f, 0f, 2.25f), Vector3.zero, k.planks);
            var batten = SaveMesh(MeshKit.Box("Gangway_batten", new Vector3(0.7f, 0.03f, 0.08f), 1f));
            for (int i = 0; i < 3; i++)
                PartAt(root.transform, "Batten", batten, new Vector3(0f, 0.08f, 0.9f + i * 1.4f), Vector3.zero, k.timber);
            return SavePrefab(root, "Gangway");
        }

        /// <summary>Twenty oak posts stacked for the dams (Hoogstraat I alone used 10,000 posts per 100 m of bank).</summary>
        static GameObject TimberStackPrefab(Kit k)
        {
            var root = new GameObject("Timber for the dams");
            var post = SaveMesh(MeshKit.Cylinder("Dam_timber", 0.12f, 4f, 7, 1f, false, -1f, true));
            int[] rows = { 6, 5, 4, 3, 2 };
            var rnd = new System.Random(11);
            for (int r = 0; r < rows.Length; r++)
                for (int i = 0; i < rows[r]; i++)
                {
                    float z = (i - (rows[r] - 1) / 2f) * 0.25f;
                    PartAt(root.transform, "Post", post, new Vector3(-2f + (float)rnd.NextDouble() * 0.3f, 0.12f + r * 0.21f, z), new Vector3(0f, 0f, -90f), k.piles, k.darkTimber);
                }
            return SavePrefab(root, "Timber stack");
        }

        /// <summary>A hollowed-trunk water trough at the kraal's gate.</summary>
        static GameObject TroughPrefab(Kit k)
        {
            var root = new GameObject("Trough");
            Part(root.transform, "Trunk", SaveMesh(MeshKit.Box("Trough_body", new Vector3(2f, 0.5f, 0.5f), 1f)), k.timber);
            PartAt(root.transform, "Water", SaveMesh(MeshKit.Box("Trough_inset", new Vector3(1.8f, 0.1f, 0.3f), 1f)), new Vector3(0f, 0.42f, 0f), Vector3.zero, k.inside);
            return SavePrefab(root, "Trough");
        }

        static GameObject TrestlePrefab(Kit k)
        {
            var root = new GameObject("Trestle");
            var post = SaveMesh(MeshKit.Box("Trestle_post", new Vector3(0.12f, 0.9f, 0.12f), 1f));
            PartAt(root.transform, "Post", post, new Vector3(-1f, 0f, 0f), Vector3.zero, k.timber);
            PartAt(root.transform, "Post", post, new Vector3(1f, 0f, 0f), Vector3.zero, k.timber);
            PartAt(root.transform, "Bar", SaveMesh(MeshKit.Box("Trestle_bar", new Vector3(2.4f, 0.1f, 0.1f), 1f)), new Vector3(0f, 0.85f, 0f), Vector3.zero, k.timber);
            return SavePrefab(root, "Trestle");
        }

        /// <summary>
        /// Smoke leaving a roof: slow, long-lived, growing to seven times its size so the plume leans and breaks in the
        /// wind (TownLife sets the rate by the hour and the wind's push). HDRP's unlit particles take no colour over
        /// their life, so the fade is in the material's alpha and the size.
        /// </summary>
        static ParticleSystem Smoke(Transform parent, Vector3 at, Material material, float rate)
        {
            var go = new GameObject("Smoke through the thatch", typeof(ParticleSystem));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = at;
            go.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            var ps = go.GetComponent<ParticleSystem>();
            var main = ps.main;
            // (4 Oct: a thin haze through the thatch, not billowing columns; TownLife sets the rate by the hour)
            main.startLifetime = 12f;
            main.startSpeed = 0.45f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.6f, 1.0f);
            main.startColor = new Color(0.42f, 0.42f, 0.46f, 0.28f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 24;
            var em = ps.emission;
            em.rateOverTime = Mathf.Min(rate, 1.2f);
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 10f;
            shape.radius = 0.35f;
            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 3.5f));
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                         new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.1f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;
            var limit = ps.limitVelocityOverLifetime;
            limit.enabled = true;
            limit.dampen = 0.1f;
            limit.limit = 1.5f;
            var noise = ps.noise;
            noise.enabled = true;
            noise.strength = 0.4f;
            noise.frequency = 0.15f;
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = material;
            r.shadowCastingMode = ShadowCastingMode.Off;
            return ps;
        }
    }
}
