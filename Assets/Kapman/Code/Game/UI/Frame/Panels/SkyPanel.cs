using Kapman.Levels;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// WIND AND TIDE - the air where she is, from the model the world's ships sail by: the wind as a sailor says it and
    /// how it lies to her course, the fog, the rain, a storm; the part of the day; and the tide where the game reckons
    /// one. On the chart no tide is reckoned yet: the panel says so in a line.
    /// </summary>
    public sealed class SkyPanel : FramePanel
    {
        public override string Id => "sky";
        public override string Title => "Wind and tide";
        public override int Order => 17;

        protected override int Stamp()
        {
            int h = 17;
            if (ChartWorld.TryWeather(out var air))
            {
                h = FrameUi.Mix(h, (int)air.windSpeed);
                h = FrameUi.Mix(h, (int)(air.windFrom / 22.5f));
                h = FrameUi.Mix(h, (int)(air.fog * 4f));
                h = FrameUi.Mix(h, (int)(air.storm * 4f));
                h = FrameUi.Mix(h, (int)air.rain);
            }
            var clock = LevelClock.Current;
            h = FrameUi.Mix(h, clock != null ? FrameUi.PartOfDay(clock.HourOfDay) : "");
            h = FrameUi.Mix(h, Course.IsSet ? Course.Next : -1);
            // (the stream and the drift to the quarter knot and the airt; her set to the half km; how he knows her place)
            var (se, sn) = Stream();
            h = FrameUi.Mix(h, SeaPush.Runs(se, sn) + SeaPush.Sets(se, sn));
            h = FrameUi.Mix(h, SeaPush.Runs(Adrift.Blow.east, Adrift.Blow.north) + SeaPush.Sets(Adrift.Blow.east, Adrift.Blow.north));
            h = FrameUi.Mix(h, (int)Adrift.Helm);
            h = FrameUi.Mix(h, (int)(Adrift.OffLine / 500.0));
            h = FrameUi.Mix(h, Adrift.Strained);
            h = FrameUi.Mix(h, RoughWaters.Version);
            return FrameUi.Mix(h, VoyageTide.Known ? (VoyageTide.Rising ? 1 : 2) : 0);
        }

        /// <summary>How a wind lies to a course: "fair, from astern", "on the beam", "foul, on the nose" (the angle is
        /// between where she heads and where the wind blows from, 0 to 180).</summary>
        public static string Lies(float offBow)
        {
            float a = Mathf.Abs(Mathf.DeltaAngle(0f, offBow));
            return a < 50f ? "foul, on the nose" : a < 80f ? "close, forward of the beam" : a < 120f ? "on the beam" : a < 155f ? "fair, on the quarter" : "fair, from astern";
        }

        /// <summary>The tide's stream where she is now, east and north in m/s (nothing on a river, nor made fast).</summary>
        static (double east, double north) Stream()
        {
            var game = KapmanGame.Current;
            if (ChartBody.Current == null || game == null || game.ship == null || !SeaPush.Rules.on || Port.LyingId() != null || ChartWorld.OnRiver())
                return (0.0, 0.0);
            return SeaPush.Stream(ChartWorld.ShipWorld(), Kapman.Game.Sim.SimStage.JulianDay);
        }

        /// <summary>How the steersman knows her place, in a line (null: nothing to say). PLACEHOLDER WORDS, the user's.</summary>
        public static string HelmWords()
        {
            switch (Adrift.Helm)
            {
                case Helm.Reckoning: return "By reckoning alone: no land in sight to steer by.";
                case Helm.Coasting: return "By reckoning, with an eye on the land.";
                case Helm.ByEye: return "By eye: he steers for what he sees.";
                case Helm.Drifting: return "Lying to, and carried by wind and tide.";
                case Helm.Anchored: return "Lying to her anchor.";
                default: return null;
            }
        }

        protected override void Fill(VisualElement body)
        {
            var clock = LevelClock.Current;
            if (!ChartWorld.TryWeather(out var air))
            {
                body.Add(FrameUi.P("There is no weather to read yet."));
                return;
            }
            body.Add(FrameUi.H(Cap(FrameGame.WindWords(air.windSpeed, air.windFrom))));
            string lies = null;
            if (Course.IsSet && Course.Points.Count > 0)
            {
                var at = ChartWorld.ShipWorld();
                var to = Course.Points[Mathf.Clamp(Course.Next, 0, Course.Points.Count - 1)];
                float bearing = Mathf.Atan2((float)(to.x - at.x), (float)(to.z - at.z)) * Mathf.Rad2Deg;
                lies = "For your course: " + Lies(Mathf.DeltaAngle(bearing, air.windFrom)) + ".";
            }
            body.Add(FrameUi.Ln("Wind", lies ?? "No course is set to judge it by.", FrameUi.Num($"{air.windSpeed:0} m/s")));
            var body2 = ChartBody.Current;
            if (body2 != null && KapmanGame.Current != null && KapmanGame.Current.ship != null && !KapmanGame.Current.ship.anchored)
                body.Add(FrameUi.Ln("She makes", body2.Sheltering ? (body2.Wind > Kapman.Game.Sim.GaleDice.GaleWind ? "Lying to in the gale." : "Lying to.") : null,
                                    FrameUi.Num($"{body2.Knots:0.0} knots")));
            body.Add(FrameUi.Ln("Sky", null, FrameUi.Tag(air.storm > 0.5f ? "a storm" : air.fog > 0.6f ? "fog" : air.fog > 0.3f ? "mist" : air.rain > 2f ? "rain" : air.rain > 0.3f ? "showers" : "clear enough")));
            if (clock != null)
                body.Add(FrameUi.Ln("The day", LevelClock.Night > 0.5f ? "Dark: a coasting craft lies up." : null, FrameUi.Tag(FrameUi.PartOfDay(clock.HourOfDay))));
            // (where no tide is known nothing is said of it: the page does not say what the game lacks)
            if (VoyageTide.Known)
                body.Add(FrameUi.Ln("Tide", null, FrameUi.Tag(VoyageTide.Rising ? "making" : "ebbing")));
            // THE SEA MOVES HER (9 Oct 2026): the stream where she is, what the wind does to her, how the steersman
            // knows her place, and how far she is off her line. Nothing of it on a river or made fast.
            if (ChartBody.Current != null && KapmanGame.Current != null && KapmanGame.Current.ship != null && Port.LyingId() == null && !ChartWorld.OnRiver())
            {
                var (se, sn) = Stream();
                string runs = SeaPush.Runs(se, sn);
                // (one run of words each, no figure at the right: the window is narrow)
                body.Add(FrameUi.Ln("The tide's stream", runs.Length > 0 ? "Setting to the " + SeaPush.Sets(se, sn) + ", " + runs + "." : "Slack, or none to speak of here."));
                var blow = Adrift.Blow;
                string drift = SeaPush.Runs(blow.east, blow.north);
                if (drift.Length > 0)
                    body.Add(FrameUi.Ln("The wind's drift", "To the " + SeaPush.Sets(blow.east, blow.north) + ", " + drift + "."));
                string helm = HelmWords();
                if (helm != null)
                    body.Add(FrameUi.Ln("The steersman", helm));
                if (Adrift.OffLine > VoyagePanel.OffLineShownM && (Adrift.Helm == Helm.Reckoning || Adrift.Helm == Helm.Coasting))
                {
                    string airt = SeaPush.Sets(Adrift.Set.x, Adrift.Set.z);
                    body.Add(FrameUi.Ln("Off her line", airt.Length > 0 ? "To the " + airt + " of it." : null, FrameUi.Num($"{Adrift.OffLine / 1000.0:0} km")));
                }
                if (Adrift.Strained)
                    body.Add(FrameUi.P("She is held off a shore, and it works her hull."));
                // rough water: a race, a bar, a shoal, an open head
                if (RoughWaters.Now != null && RoughWaters.RoughNow >= RoughWaters.OverUnder)
                    body.Add(FrameUi.Ln("Rough water", RoughWaters.NameOf(RoughWaters.Now) + ".",
                                        FrameUi.Tag(RoughWaters.RoughNow < 0.5 ? "lumpy" : RoughWaters.RoughNow < 1.0 ? "rough" : "very rough")));
            }
        }

        static string Cap(string s) => string.IsNullOrEmpty(s) ? "" : char.ToUpperInvariant(s[0]) + s.Substring(1);
    }
}
