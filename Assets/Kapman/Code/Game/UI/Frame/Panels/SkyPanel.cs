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
            return FrameUi.Mix(h, VoyageTide.Known ? (VoyageTide.Rising ? 1 : 2) : 0);
        }

        /// <summary>How a wind lies to a course: "fair, from astern", "on the beam", "foul, on the nose" (the angle is
        /// between where she heads and where the wind blows from, 0 to 180).</summary>
        public static string Lies(float offBow)
        {
            float a = Mathf.Abs(Mathf.DeltaAngle(0f, offBow));
            return a < 50f ? "foul, on the nose" : a < 80f ? "close, forward of the beam" : a < 120f ? "on the beam" : a < 155f ? "fair, on the quarter" : "fair, from astern";
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
        }

        static string Cap(string s) => string.IsNullOrEmpty(s) ? "" : char.ToUpperInvariant(s[0]) + s.Substring(1);
    }
}
