using System;

namespace Kapman.Chain
{
    /// <summary>A ship's speeds through the water (m/s): rowing in a river place, sailing on an ocean level, and cruising
    /// on a cut-out reach (which sets her hours across it).</summary>
    public sealed class ChainShip
    {
        public string id = "";
        public double riverMps, seaMps, reachMps;

        public ChainShip(string id, double riverMps, double seaMps, double reachMps)
        {
            this.id = id;
            this.riverMps = riverMps;
            this.seaMps = seaMps;
            this.reachMps = reachMps;
        }

        /// <summary>The player's ship, at the chain's pace.</summary>
        public static ChainShip Player(ChainData data) =>
            new ChainShip("player", data.pace.riverMps, data.pace.seaMps, data.pace.reachMps);

        /// <summary>The same ship faster or slower by a factor (a class at 3.5 knots against the player's 5: 0.7).</summary>
        public ChainShip Scaled(string newId, double factor) =>
            new ChainShip(newId, riverMps * factor, seaMps * factor, reachMps * factor);
    }

    /// <summary>Where a ship is on a route: the leg, and how far into it (metres sailed in a level's leg, game hours
    /// spent in a join's reach).</summary>
    public struct ChainProgress
    {
        public int leg;
        public double done;
        public bool arrived;
    }

    /// <summary>
    /// THE WORLD'S SHIPS ON THE CHAIN - the fleet keeps the player's pace (decision 3): a ship's progress along a route is
    /// counted in chain minutes, the same arithmetic as the player's. Inside a level she sails its metres at her class
    /// speed as if every real second were a game minute (the clock's 1 s = 1 game minute), so she passes the player at a
    /// believable rate; across a join's cut-out reach she spends the hours the player's date jumps by there (decision 9:
    /// the reach's real kilometres at her cruising speed), plus the deck beat's few seconds. A trader and the player who
    /// cast off together therefore make port together. Her real kilometres (the chart, the markets, news, "a ship was seen
    /// off the Nore") are derived from that progress by the route's map, never the other way round.
    /// </summary>
    public static class ChainSim
    {
        /// <summary>
        /// The game hours a join's cut-out reach adds (decision 9): its real length at a ship's speed through the water.
        /// The player's clock jumps by this at the cut (the tick ladder raises Jumped and replays every tick between); a sim
        /// ship spends it inside the reach. A join with no reach (a strait baked into both seas) adds nothing.
        /// </summary>
        public static double JumpHours(ChainJoin join, double speedMps)
        {
            if (join == null)
                throw new ArgumentNullException(nameof(join));
            if (speedMps <= 0.0)
                throw new ArgumentOutOfRangeException(nameof(speedMps), "a ship crossing a reach needs a speed above 0");
            return join.realKm * 1000.0 / speedMps / 3600.0;
        }

        /// <summary>A ship's speed over the level's metres on a sail leg (m/s): rowing in a river place, with the stream
        /// added going down and taken off going up where the level's water pushes ships; sailing on the sea.</summary>
        public static double SpeedIn(RouteLeg leg, ChainShip ship)
        {
            if (leg.level.IsOcean)
                return ship.seaMps;
            double v = ship.riverMps;
            var w = leg.level.water;
            if (w.pushesShips && w.currentMps > 0.0)
                v = leg.downstream ? v + w.currentMps : Math.Max(v * 0.25, v - w.currentMps);
            return v;
        }

        /// <summary>The game hours a leg takes a ship: a sail leg's metres at her speed (a real second at 1x is a game
        /// minute); a join's deck beat (seconds of play, so game minutes) and its reach.</summary>
        public static double LegHours(RouteLeg leg, ChainShip ship)
        {
            if (leg.kind == RouteLegKind.Sail)
                return leg.metres / SpeedIn(leg, ship) / 60.0;
            return leg.join.BeatSeconds / 60.0 + JumpHours(leg.join, ship.reachMps);
        }

        /// <summary>The game hours of a whole route.</summary>
        public static double RouteHours(RouteMap map, ChainShip ship)
        {
            double h = 0.0;
            foreach (var leg in map.legs)
                h += LegHours(leg, ship);
            return h;
        }

        /// <summary>The minutes of play at 1x a route takes (the route budget's measure): the legs sailed and the deck
        /// beats; the date jumps are not played.</summary>
        public static double PlayMinutes(RouteMap map, ChainShip ship)
        {
            double m = 0.0;
            foreach (var leg in map.legs)
                m += leg.kind == RouteLegKind.Sail ? leg.metres / SpeedIn(leg, ship) / 60.0 : leg.join.BeatSeconds / 60.0;
            return m;
        }

        /// <summary>The game hours the date jumps over a whole route (the reaches, at the ship's cruising speed).</summary>
        public static double RouteJumpHours(RouteMap map, ChainShip ship)
        {
            double h = 0.0;
            foreach (var leg in map.legs)
                if (leg.kind == RouteLegKind.Join)
                    h += JumpHours(leg.join, ship.reachMps);
            return h;
        }

        /// <summary>Moves a ship on along her route by some game hours (one hour from the world's sim, or a jump's many).</summary>
        public static void Advance(RouteMap map, ChainShip ship, ref ChainProgress p, double gameHours)
        {
            if (gameHours < 0.0)
                throw new ArgumentOutOfRangeException(nameof(gameHours), "the chain only runs forward (a reversed clock is not replayed)");
            double left = gameHours;
            while (!p.arrived && left > 0.0)
            {
                var leg = map.legs[p.leg];
                double legHours = LegHours(leg, ship);
                double doneHours = leg.kind == RouteLegKind.Sail ? p.done / SpeedIn(leg, ship) / 60.0 : p.done;
                double need = legHours - doneHours;
                if (left < need)
                {
                    double hours = doneHours + left;
                    p.done = leg.kind == RouteLegKind.Sail ? hours * 60.0 * SpeedIn(leg, ship) : hours;
                    return;
                }
                left -= need;
                NextLeg(map, ref p);
            }
        }

        static void NextLeg(RouteMap map, ref ChainProgress p)
        {
            p.done = 0.0;
            if (p.leg + 1 >= map.legs.Count)
            {
                p.arrived = true;
                p.done = map.legs[p.leg].kind == RouteLegKind.Sail ? map.legs[p.leg].metres : 0.0;
                return;
            }
            p.leg++;
        }

        /// <summary>The share (0-1) of the current leg behind her.</summary>
        public static double Fraction(RouteMap map, ChainShip ship, ChainProgress p)
        {
            if (p.arrived)
                return 1.0;
            var leg = map.legs[p.leg];
            if (leg.kind == RouteLegKind.Sail)
                return leg.metres > 0.0 ? Math.Min(1.0, p.done / leg.metres) : 1.0;
            double hours = LegHours(leg, ship);
            return hours > 0.0 ? Math.Min(1.0, p.done / hours) : 1.0;
        }

        /// <summary>Her real kilometres from the route's start (derived from her chain progress).</summary>
        public static double RealKm(RouteMap map, ChainShip ship, ChainProgress p) =>
            map.RealKm(p.leg, Fraction(map, ship, p));

        /// <summary>Where she is on the real map.</summary>
        public static (double lat, double lon) ChartPoint(RouteMap map, ChainShip ship, ChainProgress p) =>
            map.ChartPoint(p.leg, Fraction(map, ship, p));

        /// <summary>Her place in a built level's metres, when she is sailing one (for a body in a resident level); false in a
        /// reach or a planned level, where she has no body.</summary>
        public static bool LocalPoint(RouteMap map, ChainShip ship, ChainProgress p, out ChainLevel level, out double x, out double z)
        {
            var leg = map.legs[p.leg];
            level = leg.level;
            x = z = 0.0;
            if (leg.kind != RouteLegKind.Sail || !leg.level.Built)
                return false;
            (x, z) = map.LegPoint(p.leg, Fraction(map, ship, p));
            return true;
        }
    }
}
