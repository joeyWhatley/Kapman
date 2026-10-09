using System.Collections.Generic;
using Kapman.Game.Sim;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// THE FIELD - the other ships bound for the port she is bound for, from the world's ships: who is ahead of her on
    /// the way and who is behind, by how far each still has to run; and those still loading for it where they lie.
    /// With no course set there is no field: the panel says so.
    /// </summary>
    public sealed class FieldPanel : FramePanel
    {
        public override string Id => "field";
        public override string Title => "The field";
        public override int Order => 15;

        /// <summary>One ship of the field: what she is, what she is doing, and how she stands to the merchant.</summary>
        public struct Rival
        {
            public string name, doing, stands;
            public double kmToRun;
            public bool underWay;
        }

        protected override int Stamp()
        {
            var game = KapmanGame.Current;
            int h = FrameUi.Mix(17, Course.Destination ?? "");
            // (the ships move all the time: the field is looked at again each game hour)
            return FrameUi.Mix(h, game != null ? (int)(game.Day * 24.0) : 0);
        }

        /// <summary>How far the merchant still has to run on her charted course, km.</summary>
        public static double ToRunKm()
        {
            if (!Course.IsSet || Course.Points.Count == 0)
                return 0.0;
            var at = ChartWorld.ShipWorld();
            int next = System.Math.Max(0, System.Math.Min(Course.Next, Course.Points.Count - 1));
            double m = WorldXZ.Distance(at, Course.Points[next]);
            for (int i = next; i + 1 < Course.Points.Count; i++)
                m += WorldXZ.Distance(Course.Points[i], Course.Points[i + 1]);
            return m / 1000.0;
        }

        /// <summary>The field for a destination: the world's traders bound there, those under way nearest the port first,
        /// then those still in port. `mine` is how far the merchant has to run (km).</summary>
        public static List<Rival> Field(IEnumerable<SimShip> ships, int dest, double mine, System.Func<int, string> portName)
        {
            var list = new List<Rival>();
            if (ships == null || dest < 0)
                return list;
            foreach (var s in ships)
            {
                if (s == null || s.isPlayer || s.raider || s.dest != dest || s.state == ShipState.Lost || s.state == ShipState.LaidUp)
                    continue;
                string home = portName != null ? portName(s.home) : "";
                string what = (s.cls != null && !string.IsNullOrEmpty(s.cls.name) ? Article(s.cls.name) : "A ship") + (string.IsNullOrEmpty(home) ? "" : " of " + home);
                var r = new Rival { name = what };
                if (s.state == ShipState.Sailing)
                {
                    r.underWay = true;
                    r.kmToRun = System.Math.Max(0.0, s.total - s.done) / 1000.0;
                    r.doing = $"Under way, {r.kmToRun:0} km to run.";
                    r.stands = r.kmToRun < mine ? "ahead" : "behind";
                }
                else if (s.state == ShipState.Anchored)
                {
                    r.underWay = true;
                    r.kmToRun = 0.0;
                    r.doing = "Come in, waiting off the landing for a berth.";
                    r.stands = "ahead";
                }
                else
                {
                    string at = portName != null ? portName(s.at) : "";
                    r.kmToRun = double.MaxValue;
                    r.doing = (s.state == ShipState.Waiting ? "Loaded, waiting for a wind" : "Still in port") + (string.IsNullOrEmpty(at) ? "." : " at " + at + ".");
                    r.stands = "not sailed";
                }
                list.Add(r);
            }
            list.Sort((a, b) => a.kmToRun.CompareTo(b.kmToRun));
            return list;
        }

        static string Article(string name)
        {
            string n = name.Trim();
            if (n.Length == 0)
                return "A ship";
            if (n.StartsWith("the ", System.StringComparison.OrdinalIgnoreCase) || n.StartsWith("a ", System.StringComparison.OrdinalIgnoreCase))
                return char.ToUpperInvariant(n[0]) + n.Substring(1);
            return ("aeiouAEIOU".IndexOf(n[0]) >= 0 ? "An " : "A ") + char.ToLowerInvariant(n[0]) + n.Substring(1);
        }

        protected override void Fill(VisualElement body)
        {
            var game = KapmanGame.Current;
            var to = Course.IsSet ? GameData.Port(Course.Destination) : null;
            if (to == null || game == null || game.World == null)
            {
                // (a place pointed at on the chart is nobody's port: no other ship is bound for it)
                body.Add(FrameUi.H(Course.ToPlace ? "Bound for " + Course.PlaceWords : "No course set"));
                body.Add(FrameUi.P(Course.ToPlace ? "No other ship is bound there: it is a place, not a port." : "No course is set, so nobody is ahead of you or behind."));
                return;
            }
            double mine = ToRunKm();
            body.Add(FrameUi.H("Bound for " + to.name));
            body.Add(FrameUi.P($"You have {mine:0} km to run."));
            var field = Field(game.World.ships, to.index, mine, i => i >= 0 && i < GameData.Ports.Count ? GameData.Ports[i].name : "");
            if (field.Count == 0)
            {
                body.Add(FrameUi.P("No other ship is bound there now."));
                return;
            }
            int ahead = 0;
            foreach (var r in field)
            {
                if (r.stands == "ahead")
                    ahead++;
                body.Add(FrameUi.Ln(r.name, r.doing, FrameUi.Tag(r.stands)));
            }
            body.Add(FrameUi.P(ahead == 0 ? "None of them is ahead of you." : ahead == 1 ? "One of them is ahead of you: the market will have met her cargo first."
                                                                                          : ahead + " of them are ahead of you: the market will have met their cargoes first."));
        }
    }
}
