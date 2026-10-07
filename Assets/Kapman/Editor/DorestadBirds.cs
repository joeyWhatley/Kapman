using System.Collections.Generic;
using System.Linq;
using Kapman.Levels;
using UnityEngine;

namespace Kapman.EditorTools
{
    /// <summary>
    /// Flocks for a river place: gulls over the harbour (one flock follows the player's boat) and low over the dam
    /// heads, a row of them perched on the heads, geese on the basins, crows on the fields and rooks over the market,
    /// swallows at the harbour, herons standing on the shoal's outer edge and the fork's bar, ducks on the slack water
    /// of the inner bends. Birds are the only animals (the user's rule); herons and ducks are the living scale a sailor
    /// sees first on a lowland river (19 fish species at Dorestad, bream kept in pools: the wetland is attested).
    /// Each flock names its species group in Assets/Kapman/Data/Wildlife/species.json (wild geese, gulls, crows, swallow,
    /// heron, mallard): the level's Wildlife sets how much of it is there from that calendar.
    /// </summary>
    public static class DorestadBirds
    {
        static readonly Color GullWhite = new Color(0.96f, 0.96f, 0.96f);
        static readonly Color HeronGrey = new Color(0.6f, 0.6f, 0.63f);
        static readonly Color DuckDark = new Color(0.12f, 0.10f, 0.08f);
        static readonly Color CrowBlack = new Color(0.06f, 0.06f, 0.07f);

        /// <summary>cover: the land cover (DorestadLandCover.LoadCover()); without it the herons and ducks go by the water's edge alone.</summary>
        public static void Make(DorestadBuilder.Grid g, DorestadTown.Plan plan, Transform parent, System.Random rnd, byte[] cover = null)
        {
            var root = new GameObject("Birds (flocks on the clock)").transform;
            root.SetParent(parent, false);
            float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);
            Vector3 town = plan.townCentre;
            Vector2 townC = new Vector2(town.x, town.z);
            float wl = g.wl;
            byte Cover(Vector2 p) => cover != null ? DorestadLandCover.Sample(cover, g, p) : (byte)255;
            bool MudOrBar(Vector2 p)
            {
                byte c = Cover(p);
                return c == 255 || c == DorestadLandCover.Mud || c == DorestadLandCover.Bar;
            }
            float Slope(Vector2 p)
            {
                const float d = 4f;
                float dx = g.H(p + new Vector2(d, 0f)) - g.H(p - new Vector2(d, 0f));
                float dz = g.H(p + new Vector2(0f, d)) - g.H(p - new Vector2(0f, d));
                return Mathf.Sqrt(dx * dx + dz * dz) / (2f * d);
            }
            Flock Add(Flock.Kind kind, Vector3 at, int count, float radius, float height, float speed, float span, Color colour,
                      bool follow = false, Vector3[] perches = null, float startle = 45f)
            {
                var go = new GameObject($"{kind} ({count})");
                go.transform.SetParent(root, false);
                go.transform.position = at;
                var f = go.AddComponent<Flock>();
                f.kind = kind;
                f.speciesId = Flock.DefaultSpecies(kind);    // the species group in species.json whose calendar the flock keeps
                f.count = count;
                f.radius = radius;
                f.height = height;
                f.speed = speed;
                f.wingspan = span;
                f.colour = colour;
                f.groundCentre = at;
                f.follow = follow;
                f.perches = perches;
                f.startle = startle;
                return f;
            }

            // the harbour's heart: the apex parcel and its dam head
            Vector3 apexHead = plan.waterfrontFocus;
            Vector2 apexOut = Vector2.zero;
            bool hasParcels = plan.parcels != null && plan.parcels.Count > 0;
            if (hasParcels)
            {
                var apex = plan.parcels.OrderBy(p => Mathf.Abs(p.s - 850f)).First();
                Vector2 head = apex.bank + apex.outward * apex.damLength;
                apexHead = new Vector3(head.x, wl, head.y);
                apexOut = apex.outward;
            }

            // gulls: over the harbour (they follow a moving boat), low over the dam heads, and over the fork
            Add(Flock.Kind.Gulls, new Vector3(plan.waterfrontFocus.x, wl, plan.waterfrontFocus.z), 14, 90f, R(8f, 18f), 10f, 1.5f, GullWhite, true);
            Add(Flock.Kind.Gulls, apexHead, 24, 60f, R(6f, 12f), 9f, 1.5f, GullWhite);
            Add(Flock.Kind.Gulls, new Vector3(plan.forkFocus.x, wl, plan.forkFocus.z), 10, 140f, R(12f, 20f), 10f, 1.5f, GullWhite);
            // gulls perched along the dam heads nearest the apex (one each): the dam tops stand about a metre over
            // the water (DorestadTown: wl + 1.0), the head's posts a little higher
            int perched = 0;
            if (hasParcels)
            {
                var perches = new List<Vector3>();
                foreach (var pc in plan.parcels.Where(p => !p.empty && Mathf.Abs(p.s - 850f) < 170f).OrderBy(p => Mathf.Abs(p.s - 850f)).Take(10))
                {
                    Vector2 head = pc.bank + pc.outward * (pc.damLength - 0.8f) + pc.along * R(-2.5f, 2.5f);
                    perches.Add(new Vector3(head.x, wl + 1.15f, head.y));
                }
                if (perches.Count >= 6)
                {
                    Add(Flock.Kind.Gulls, apexHead, perches.Count, 40f, 10f, 9f, 1.5f, GullWhite, false, perches.ToArray(), 22f);
                    perched = perches.Count;
                }
            }

            // geese on the wet basins: low open land away from the town
            int geese = 0;
            for (int tries = 0; tries < 400 && geese < 5; tries++)
            {
                var p = new Vector2(R(200f, g.size - 200f), R(200f, g.size - 200f));
                if (g.SD(p) > -60f || g.H(p) - wl > 0.7f || plan.Get(p) != 0 || Vector2.Distance(p, townC) < 700f)
                    continue;
                Add(Flock.Kind.Geese, new Vector3(p.x, g.H(p), p.y), 9 + rnd.Next(10), R(200f, 400f), R(40f, 80f), 13f, 1.6f, new Color(0.2f, 0.18f, 0.16f));
                geese++;
            }
            // crows on the fields and the farms
            int crows = 0;
            for (int tries = 0; tries < 400 && crows < 4; tries++)
            {
                var p = new Vector2(R(100f, g.size - 100f), R(100f, g.size - 100f));
                byte use = plan.Get(p);
                if (use != (byte)DorestadTown.Use.Field && use != (byte)DorestadTown.Use.Farmyard)
                    continue;
                Add(Flock.Kind.Crows, new Vector3(p.x, g.H(p), p.y), 6 + rnd.Next(10), R(80f, 160f), R(15f, 35f), 8f, 0.95f, CrowBlack);
                crows++;
            }
            // rooks low over the market and the nearest field block
            Add(Flock.Kind.Crows, town, 16, 70f, R(4f, 8f), 8f, 0.95f, CrowBlack);
            if (plan.fields != null && plan.fields.Count > 0)
            {
                var f = plan.fields.OrderBy(x => Vector2.Distance(x.c, townC)).First();
                Add(Flock.Kind.Crows, new Vector3(f.c.x, g.H(f.c), f.c.y), 16, 80f, R(4f, 8f), 8f, 0.95f, CrowBlack);
            }
            // swallows over the harbour's water on summer evenings
            Add(Flock.Kind.Swallows, plan.waterfrontFocus, 18, 90f, 6f, 12f, 0.35f, new Color(0.1f, 0.1f, 0.14f));

            // herons standing in the shallows: four at the shoal's outer edge beyond the apex dams, four on the fork's bar
            int herons = 0;
            bool Stand(Vector2 p, float sdLo, float sdHi)
            {
                float sd = g.SD(p);
                return sd > sdLo && sd < sdHi && plan.Get(p) == 0 && MudOrBar(p) && Slope(p) < 0.1f;
            }
            Vector3 OnGround(Vector2 p) => new Vector3(p.x, Mathf.Max(g.H(p), wl - 0.25f), p.y);
            void Herons(Vector2 around, float search)
            {
                for (int tries = 0; tries < 800; tries++)
                {
                    var p = around + new Vector2(R(-search, search), R(-search, search));
                    if (p.x < 50f || p.y < 50f || p.x > g.size - 50f || p.y > g.size - 50f || !Stand(p, -4f, 1f))
                        continue;
                    var spots = new List<Vector3> { OnGround(p) };
                    for (int k = 0; k < 60 && spots.Count < 4; k++)
                    {
                        var q = p + new Vector2(R(-12f, 12f), R(-12f, 12f));
                        if (Stand(q, -5f, 1.2f) && spots.All(s => Vector2.Distance(new Vector2(s.x, s.z), q) > 2.5f))
                            spots.Add(OnGround(q));
                    }
                    if (spots.Count < 3)
                        continue;
                    Add(Flock.Kind.Herons, OnGround(p), spots.Count, 25f, 12f, 7f, 1.75f, HeronGrey, false, spots.ToArray(), 45f);
                    herons++;
                    return;
                }
            }
            Vector2 apexHead2 = new Vector2(apexHead.x, apexHead.z);
            Herons(apexHead2 + apexOut * 35f, 120f);
            Herons(new Vector2(plan.forkFocus.x, plan.forkFocus.z), 260f);

            // ducks on the slack water of the inner bends: in the water, a few metres off a flat mud or sand shore,
            // away from the harbour's traffic
            int ducks = 0;
            var duckSpots = new List<Vector2>();
            for (int tries = 0; tries < 3000 && ducks < 3; tries++)
            {
                var p = new Vector2(R(150f, g.size - 150f), R(150f, g.size - 150f));
                float sd = g.SD(p);
                if (sd < 3f || sd > 14f || Vector2.Distance(p, townC) < 300f || duckSpots.Any(d => Vector2.Distance(d, p) < 150f))
                    continue;
                bool slack = false;
                for (int k = 0; k < 8 && !slack; k++)
                {
                    float a = k * Mathf.PI / 4f;
                    var q = p + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 14f;
                    slack = g.SD(q) < 0f && MudOrBar(q) && Slope(q) < 0.08f;
                }
                if (!slack)
                    continue;
                Add(Flock.Kind.Ducks, new Vector3(p.x, wl, p.y), 8, 14f, 0f, 0f, 0.55f, DuckDark);
                duckSpots.Add(p);
                ducks++;
            }
            Debug.Log($"Kapman birds: 3 gull flocks and {perched} gulls perched on the dam heads, {geese} geese skeins, {crows} crow flocks, 2 rook flocks, " +
                      $"1 of swallows, {herons} heron stands, {ducks} duck groups.");
        }
    }
}
