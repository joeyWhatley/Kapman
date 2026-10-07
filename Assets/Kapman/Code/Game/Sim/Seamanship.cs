using System;
using System.Collections.Generic;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game.Sim
{
    /// <summary>The seamanship table (Data/seamanship_793.json). Game metres across are 32 real metres.</summary>
    public sealed class SeamanshipRules
    {
        /// <summary>Game metres to starboard of the lane's line, and the least room when passing, by the kind of water
        /// (LaneGraph.Sea, Coastal, Sound, River, Pending, Flats, Portage, Approach).</summary>
        public readonly float[] keepSide = { 20f, 12f, 8f, 5f, 5f, 8f, 0f, 3f };
        public readonly float[] pass = { 24f, 14f, 9f, 6f, 6f, 9f, 2f, 3f };
        /// <summary>Real km at each end of a passage over which she comes in from her side to the berth.</summary>
        public double taperKm = 2.0;
        public string[] giveWayOrder = { "rowing_gives_way_to_sail", "overtaking_keeps_clear", "smaller_gives_way", "other_on_starboard_gives_way" };
        public float smallerBy = 3f;
        public double[] slowSteps = { 0.75, 0.5, 0.25, 0.0 };
        public float standOutGame = 40f, standOutFade = 0.5f;
        public int passes = 3;
        public double seaCrossingKm = 40.0, foulMarginDeg = 20.0, checkEveryH = 6.0, maxWaitDays = 4.0;
        public double berthShare = 1.0, holdBackKm = 1.5, spacingLengths = 2.0, moorSide = 1.4;
        /// <summary>Days at anchor off a full landing before she goes in anyway and moors alongside another (the
        /// crowded harbours of the time moored ships two and three abreast); days a departure waits for another ship
        /// near her berth to get clear before she picks her way out past her.</summary>
        public double maxAnchorDays = 4.0, maxStartWaitDays = 1.0;
        public int berthMin = 2;
        public double hashKm = 20.0;

        static readonly string[] Kinds = { "sea", "coastal", "sound", "river", "pending", "flats", "portage", "approach" };

        public static SeamanshipRules Load(TextAsset json)
        {
            var r = new SeamanshipRules();
            if (json == null)
                return r;
            var o = Json.Parse(json.text);
            var side = Json.Obj(o, "keep_side_game_m");
            var room = Json.Obj(o, "pass_game_m");
            for (int k = 0; k < Kinds.Length; k++)
            {
                r.keepSide[k] = (float)Json.Num(side, Kinds[k], r.keepSide[k]);
                r.pass[k] = (float)Json.Num(room, Kinds[k], r.pass[k]);
            }
            r.taperKm = Json.Num(o, "taper_real_km", r.taperKm);
            var g = Json.Obj(o, "give_way");
            var order = Json.Arr(g, "order");
            if (order != null && order.Count > 0)
                r.giveWayOrder = order.ConvertAll(x => x as string ?? "").ToArray();
            r.smallerBy = (float)Json.Num(g, "smaller_by_length_m", r.smallerBy);
            var steps = Json.Arr(g, "slow_steps");
            if (steps != null && steps.Count > 0)
                r.slowSteps = steps.ConvertAll(x => x is double v ? v : 0.0).ToArray();
            r.standOutGame = (float)Json.Num(g, "stand_out_game_m", r.standOutGame);
            r.standOutFade = (float)Json.Num(g, "stand_out_fades_per_hour", r.standOutFade);
            r.passes = (int)Json.Num(g, "passes", r.passes);
            var f = Json.Obj(o, "fair_wind");
            r.seaCrossingKm = Json.Num(f, "sea_crossing_real_km", r.seaCrossingKm);
            r.foulMarginDeg = Json.Num(f, "foul_margin_deg", r.foulMarginDeg);
            r.checkEveryH = Json.Num(f, "check_every_game_h", r.checkEveryH);
            r.maxWaitDays = Json.Num(f, "max_wait_days", r.maxWaitDays);
            var b = Json.Obj(o, "berths");
            r.berthShare = Json.Num(b, "share_of_peak", r.berthShare);
            r.berthMin = (int)Json.Num(b, "min", r.berthMin);
            r.holdBackKm = Json.Num(b, "hold_back_real_km", r.holdBackKm);
            r.spacingLengths = Json.Num(b, "spacing_lengths", r.spacingLengths);
            r.moorSide = Json.Num(b, "moor_side_share", r.moorSide);
            r.maxAnchorDays = Json.Num(b, "max_anchor_days", r.maxAnchorDays);
            r.maxStartWaitDays = Json.Num(b, "max_start_wait_days", r.maxStartWaitDays);
            r.hashKm = Json.Num(o, "hash_cell_real_km", r.hashKm);
            return r;
        }

        /// <summary>Real metres to starboard she keeps on water of this kind.</summary>
        public double SideM(byte kind) => keepSide[Math.Min(kind, (byte)(keepSide.Length - 1))] * SimUnits.Across;

        /// <summary>Real metres between two ships' middles below which they're too close, on these waters.</summary>
        public double PassM(byte kindA, byte kindB, ShipClassDef a, ShipClassDef b)
        {
            float room = Math.Min(pass[Math.Min(kindA, (byte)(pass.Length - 1))], pass[Math.Min(kindB, (byte)(pass.Length - 1))]);
            return (room + 0.5 * (a.beam + b.beam)) * SimUnits.Across;
        }
    }

    /// <summary>
    /// SEAMANSHIP - the world's ships keep clear of each other, as real vessels do (the user: "Make sure the boats react to
    /// each other and plan and don't collide and act like real vessels"; Plans/ai_fleets.md, "Added: seamanship").
    ///
    /// Every hour, after each ship has worked out how far she'd go (WorldSim), each pair that could meet within the hour
    /// (a spatial hash) is checked for its closest point of approach along the straight lines the 3D and the map draw
    /// between hours. Two ships that would come closer than the passing room for those waters: the give-way ship (by the
    /// table's rules, first that applies) slows - three-quarters, half, a quarter, stop - until they clear; if slowing
    /// can't do it, or she's overtaking, she stands out to one side, away from the other; and last the stand-on ship
    /// slows too. Ships meeting head-on pass port to port because each keeps to her own side of the lane. Ships anchored
    /// off are kept clear of as well. A few passes settle the knock-on effects. Deterministic: pairs in index order.
    /// </summary>
    public static class Seamanship
    {
        static readonly List<SimShip> moving = new List<SimShip>();
        static readonly Dictionary<long, List<SimShip>> cells = new Dictionary<long, List<SimShip>>();

        /// <summary>Settles this hour's moves of every sailing ship (her `go` and `standOut`). Returns the pairs left
        /// closer than their passing room (0 when all is well).</summary>
        public static int Resolve(WorldSim w)
        {
            var rules = w.sea;
            moving.Clear();
            foreach (var s in w.ships)
                if ((s.state == ShipState.Sailing || s.state == ShipState.Anchored) && !s.chasing)
                    moving.Add(s);
            if (w.Player != null)
                moving.Add(w.Player);                                      // (the merchant's ship: everyone keeps clear of her)
            if (moving.Count < 2)
                return 0;
            double cell = rules.hashKm * 1000.0;
            foreach (var list in cells.Values)
                list.Clear();
            foreach (var s in moving)
            {
                long key = Key(s.pos, cell);
                if (!cells.TryGetValue(key, out var list))
                    cells[key] = list = new List<SimShip>();
                list.Add(s);
            }
            int left = 0;
            for (int pass = 0; pass < Math.Max(1, rules.passes); pass++)
            {
                bool changed = false;
                left = 0;
                foreach (var a in moving)
                {
                    int cx = (int)Math.Floor(a.pos.x / cell), cz = (int)Math.Floor(a.pos.z / cell);
                    for (int dx = -1; dx <= 1; dx++)
                        for (int dz = -1; dz <= 1; dz++)
                        {
                            if (!cells.TryGetValue(Pack(cx + dx, cz + dz), out var list))
                                continue;
                            foreach (var b in list)
                            {
                                if (b.index <= a.index || (a.state == ShipState.Anchored && b.state == ShipState.Anchored))
                                    continue;
                                int r = Settle(w, a, b);
                                if (r > 0)
                                    changed = true;
                                if (r < 0)
                                    left++;
                            }
                        }
                }
                if (!changed)
                    break;
            }
            return left;
        }

        /// <summary>0: clear as they are; 1: one of them gave way and now they clear; -1: still too close.</summary>
        static int Settle(WorldSim w, SimShip a, SimShip b)
        {
            if ((a.isPlayer && b.heaveTo) || (b.isPlayer && a.heaveTo))
                return 0;                                                  // (hove to for his hail: he's meant to come alongside)
            double room = w.sea.PassM(KindOf(a), KindOf(b), a.cls, b.cls);
            // two already closer than that (they met at a landing, or one lay up for the night in the other's way) only
            // mustn't close further: holding them to the full room would stop them for good
            double now = WorldXZ.Distance(a.pos, b.pos);
            if (now < room)
                room = Math.Max(0.0, now - 1.0);
            if (room <= 0.0 || Closest(w, a, b) >= room)
                return 0;
            var (give, stand, overtaking) = WhoGivesWay(w, a, b);
            w.stats.giveWays++;
            double go0 = give.go, out0 = give.standOut;
            if (!overtaking)
                foreach (double f in w.sea.slowSteps)
                {
                    give.go = go0 * f;
                    if (Closest(w, a, b) >= room)
                        return 1;
                }
            // stand out to the side away from the other (overtaking: pass her; or slowing alone couldn't clear), as far
            // as gives the passing room abeam, never more than the table's most
            give.go = overtaking ? go0 : give.go;
            double lateral = Lateral(give, stand);
            double away = Away(w, give, stand, lateral);
            double need = Math.Max(0.0, room * 1.15 - Math.Abs(lateral));
            give.standOut = out0 + away * Math.Min(need, w.sea.standOutGame * SimUnits.Across);
            give.holdOut = 2;
            w.stats.standOuts++;
            if (Closest(w, a, b) >= room)
                return 1;
            if (overtaking)
                foreach (double f in w.sea.slowSteps)                       // can't get by: drop back behind her
                {
                    give.go = go0 * f;
                    if (Closest(w, a, b) >= room)
                        return 1;
                }
            if (stand.state == ShipState.Sailing && !stand.isPlayer)       // last: the stand-on ship slows too (never the merchant's)
            {
                double so = stand.go;
                foreach (double f in w.sea.slowSteps)
                {
                    stand.go = so * f;
                    if (Closest(w, a, b) >= room)
                        return 1;
                }
            }
            return -1;
        }

        /// <summary>The give-way and stand-on ships, and whether the give-way one is overtaking. A ship at anchor always
        /// stands on.</summary>
        static (SimShip give, SimShip stand, bool overtaking) WhoGivesWay(WorldSim w, SimShip a, SimShip b)
        {
            if (a.isPlayer)                                                   // the merchant's ship follows her line: she stands on
                return (b, a, false);
            if (b.isPlayer)
                return (a, b, false);
            if (a.state == ShipState.Anchored)
                return (b, a, false);
            if (b.state == ShipState.Anchored)
                return (a, b, false);
            double ha = Heading(a), hb = Heading(b);
            bool sameWay = Math.Abs(Delta(ha, hb)) < 67.5;
            foreach (string rule in w.sea.giveWayOrder)
                switch (rule)
                {
                    case "rowing_gives_way_to_sail":
                        bool ra = Rowing(w, a), rb = Rowing(w, b);
                        if (ra != rb)
                            return ra ? (a, b, false) : (b, a, false);
                        break;
                    case "overtaking_keeps_clear":
                        if (sameWay)
                        {
                            // the one behind (along their common way) and coming up faster
                            double ux = Math.Sin(ha * Math.PI / 180.0), uz = Math.Cos(ha * Math.PI / 180.0);
                            double ahead = (b.pos.x - a.pos.x) * ux + (b.pos.z - a.pos.z) * uz;
                            if (ahead > 0.0 && a.go > b.go)
                                return (a, b, true);
                            if (ahead < 0.0 && b.go > a.go)
                                return (b, a, true);
                        }
                        break;
                    case "smaller_gives_way":
                        if (Math.Abs(a.cls.length - b.cls.length) >= w.sea.smallerBy)
                            return a.cls.length < b.cls.length ? (a, b, false) : (b, a, false);
                        break;
                    case "other_on_starboard_gives_way":
                        if (!sameWay)
                        {
                            // the ship that has the other on her own starboard (right) side gives way
                            double bearAB = Travel.Bearing(a.pos, b.pos);
                            double rel = Delta(ha, bearAB);
                            return rel > 0.0 && rel < 180.0 ? (a, b, false) : (b, a, false);
                        }
                        break;
                }
            return a.index > b.index ? (a, b, false) : (b, a, false);   // (the later one, if nothing else decides)
        }

        static bool Rowing(WorldSim w, SimShip s) => !s.cls.hasSail;

        /// <summary>Real metres the other lies to her starboard (+) or port (-) of her way.</summary>
        static double Lateral(SimShip me, SimShip other)
        {
            double h = Heading(me) * Math.PI / 180.0;
            double rx = Math.Cos(h), rz = -Math.Sin(h);                    // her starboard
            return (other.pos.x - me.pos.x) * rx + (other.pos.z - me.pos.z) * rz;
        }

        /// <summary>+1 to stand out to starboard, -1 to port: away from where the other is (overtaking or dead ahead: to
        /// port; meeting dead ahead: to starboard).</summary>
        static double Away(WorldSim w, SimShip me, SimShip other, double lateral)
        {
            if (Math.Abs(lateral) < 0.25 * w.sea.SideM(KindOf(me)) + 1.0)
                return other.state == ShipState.Sailing && Math.Abs(Delta(Heading(me), Heading(other))) < 67.5 ? -1.0 : 1.0;
            return lateral > 0.0 ? -1.0 : 1.0;
        }

        /// <summary>Hours ahead a ship looks when the merchant's ship is the other: he can't alter, so she must see him
        /// coming and hold back in time.</summary>
        public const double PlayerHorizonHours = 2.5;

        /// <summary>The least distance between two ships over this hour (or, with the merchant's ship, the next few,
        /// each going on as she goes this hour), each in a straight line from where she is.</summary>
        public static double Closest(WorldSim w, SimShip a, SimShip b)
        {
            var a1 = a.state == ShipState.Sailing ? w.Where(a, a.go, a.standOut) : a.pos;
            var b1 = b.state == ShipState.Sailing ? w.Where(b, b.go, b.standOut) : b.pos;
            return Closest(a.pos, a1, b.pos, b1, a.isPlayer || b.isPlayer ? PlayerHorizonHours : 1.0);
        }

        /// <summary>Closest approach of two points moving in straight lines from a0 to a1 and b0 to b1 over one step,
        /// looking `steps` steps ahead (going on as they go).</summary>
        public static double Closest(WorldXZ a0, WorldXZ a1, WorldXZ b0, WorldXZ b1, double steps = 1.0)
        {
            double px = a0.x - b0.x, pz = a0.z - b0.z;
            double vx = (a1.x - a0.x) - (b1.x - b0.x), vz = (a1.z - a0.z) - (b1.z - b0.z);
            double vv = vx * vx + vz * vz;
            double t = vv > 1e-9 ? Math.Max(0.0, Math.Min(steps, -(px * vx + pz * vz) / vv)) : 0.0;
            double x = px + vx * t, z = pz + vz * t;
            return Math.Sqrt(x * x + z * z);
        }

        /// <summary>The kind of water she's on (the merchant's ship counts as open sea, so the other's water sets the room).</summary>
        public static byte KindOf(SimShip s) =>
            s.isPlayer || s.kinds.Count == 0 ? LaneGraph.Sea : s.kinds[Math.Max(0, Math.Min(s.leg, s.kinds.Count - 1))];

        static double Heading(SimShip s)
        {
            if (s.path.Count < 2)
                return s.heading;
            int leg = Math.Max(0, Math.Min(s.leg, s.path.Count - 2));
            return Travel.Bearing(s.path[leg], s.path[leg + 1]);
        }

        static double Delta(double from, double to) => ((to - from) % 360.0 + 540.0) % 360.0 - 180.0;

        static long Key(WorldXZ p, double cell) => Pack((int)Math.Floor(p.x / cell), (int)Math.Floor(p.z / cell));
        static long Pack(int x, int z) => ((long)x << 32) ^ (uint)z;
    }
}
