using System;
using System.Collections.Generic;
using Kapman.Game.Sim;
using Kapman.World;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// HOW LONG A PASSAGE TAKES, as a merchant reckons it: a low and a high figure in days ("a measuring thing for the
    /// map and it's in days", the user, 7 Oct 2026). The way is the course planner's (the sea lanes, as a boat like hers
    /// can go: Course.Plan, and <see cref="Between"/> for two places that are not ports); the speeds are her own class's
    /// from Travel.Speed, the same sum the chart's ship sails by: the LOW figure with a fair wind (a fresh breeze on the
    /// quarter, her best point), the HIGH figure with a light wind dead ahead (beating, or the oars where they beat
    /// tacking). She sails night and day, so a day is twenty-four hours of it.
    /// </summary>
    public static class FramePassage
    {
        /// <summary>A fresh breeze, and a light one (m/s): the winds the two figures are reckoned for.</summary>
        public const double FairWind = 8.0, LightWind = 4.0;

        /// <summary>Her class (the chart's ship's), or null before the world is made.</summary>
        public static ShipClassDef HerClass
        {
            get
            {
                var game = KapmanGame.Current;
                var body = ChartBody.Current;
                return game != null && game.World != null ? game.World.ClassOf(body != null ? body.classId : "gislinge") : null;
            }
        }

        /// <summary>Her speed made good with a fair wind, and with a foul one (m/s). Knots of the course's own plan if her
        /// class is not known yet.</summary>
        public static (double fair, double foul) Speeds(ShipClassDef cls)
        {
            if (cls == null)
                return (Course.PlanKnots * SimUnits.KnotMs * 1.4, Course.PlanKnots * SimUnits.KnotMs * 0.5);
            double fair = Travel.Speed(cls, 0.0, 225.0, FairWind, out _);    // the wind on her quarter
            double foul = Travel.Speed(cls, 0.0, 0.0, LightWind, out _);     // dead ahead, and light
            return (Math.Max(0.1, fair), Math.Max(0.1, Math.Min(fair, foul)));
        }

        /// <summary>The days a way of so many real km takes her: with a fair wind, and with a foul one.</summary>
        public static (double low, double high) Days(double km)
        {
            var (fair, foul) = Speeds(HerClass);
            double m = km * 1000.0;
            return (m / (SimUnits.RealMetresPerGameHour(fair) * 24.0), m / (SimUnits.RealMetresPerGameHour(foul) * 24.0));
        }

        /// <summary>The same as plain words: "1 to 2 days".</summary>
        public static string DaysWords(double km)
        {
            var (low, high) = Days(km);
            return FrameUi.Days(low, high);
        }

        public enum Wind { Unknown, Fair, Working, Foul, Gale }

        /// <summary>The wind where she is now, for a way that begins on this line: fair (she makes most of her best),
        /// foul (she beats or rows), in between, or a gale she would lie to in.</summary>
        public static Wind WindNow(IList<WorldXZ> line)
        {
            var cls = HerClass;
            if (cls == null || line == null || line.Count < 2 || !ChartWorld.TryWeather(out var air))
                return Wind.Unknown;
            // the way she would head: toward the line's point some 8 km on (a river's first bends are not the passage)
            var from = line[0];
            var to = line[line.Count - 1];
            double gone = 0.0;
            for (int i = 1; i < line.Count; i++)
            {
                gone += WorldXZ.Distance(line[i - 1], line[i]);
                if (gone >= 8000.0)
                {
                    to = line[i];
                    break;
                }
            }
            double now = Travel.Speed(cls, Travel.Bearing(from, to), air.windFrom, air.windSpeed, out bool lying);
            if (lying)
                return Wind.Gale;
            var (fair, foul) = Speeds(cls);
            return now >= fair * 0.7 ? Wind.Fair : now <= foul * 1.2 ? Wind.Foul : Wind.Working;
        }

        public static string WindWords(Wind w)
        {
            switch (w)
            {
                case Wind.Fair: return "fair wind today";
                case Wind.Working: return "a working wind today";
                case Wind.Foul: return "foul wind today";
                case Wind.Gale: return "a gale: she would lie to";
                default: return "";
            }
        }

        /// <summary>The real km of a line.</summary>
        public static double Km(IList<WorldXZ> line)
        {
            double m = 0.0;
            for (int i = 0; i + 1 < line.Count; i++)
                m += WorldXZ.Distance(line[i], line[i + 1]);
            return m / 1000.0;
        }

        /// <summary>
        /// The way by water between two places that need not be ports (the measure): from the lanes' point nearest the
        /// first to the one nearest the second, as her course would go (off the flats where there is a way), with the
        /// two places themselves at its ends. With no lanes baked it is the straight line. False when the water does not
        /// join them.
        /// </summary>
        public static bool Between(WorldXZ a, WorldXZ b, List<WorldXZ> line, out double km)
        {
            line.Clear();
            km = 0.0;
            var game = KapmanGame.Current;
            var g = SimRoutes.Graph(game != null ? game.lanesFile : SimRoutes.DefaultLanes);
            line.Add(a);
            if (g != null)
            {
                var router = new LaneRouter(g);
                var edges = new List<int>();
                int na = -1;
                bool found = false;
                foreach (bool offFlats in new[] { true, false })
                {
                    var profile = Course.Profile(offFlats);
                    na = g.Nearest(a.x, a.z, profile);
                    int nb = g.Nearest(b.x, b.z, profile);
                    edges.Clear();
                    if (na >= 0 && nb >= 0 && (na == nb || router.Route(na, nb, profile, edges, out _)))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    return false;
                var pts = new List<(double x, double z)>();
                int n = na;
                foreach (int e in edges)
                {
                    g.AppendCourse(e, n, pts);
                    n = g.Other(e, n);
                }
                if (pts.Count == 0)
                    pts.Add((g.nodeX[na], g.nodeZ[na]));
                foreach (var (x, z) in pts)
                    line.Add(new WorldXZ(x, z));
            }
            line.Add(b);
            km = Km(line);
            return true;
        }
    }
}
