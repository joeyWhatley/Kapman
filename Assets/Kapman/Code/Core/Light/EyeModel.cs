using System;

namespace Kapman.World
{
    /// <summary>
    /// THE EYE - how far the player's eye has adapted to the dark (Design/lighting_and_atmosphere.md 2.2; the user's
    /// decision 12 of 4 October: night as a dark-adapted eye sees it, stars and outlines and no colour, a lantern
    /// resetting it). One number, the adaptation: 0 the day's eye (the cones), 1 fully dark-adapted (the rods). It heads
    /// for the target the scene sets, slowly into the dark and fast back into the light:
    ///   target      0 at a scene EV100 of 2 and above, 1 at -8 and below, straight between;
    ///   into dark   an exponential with a time constant of 20 game minutes (full dark adaptation takes 20-30 minutes,
    ///               https://en.wikipedia.org/wiki/Adaptation_(eye)); one game minute is one real second, so the real
    ///               figure is usable as it stands: 63% after 20 game minutes, 95% after an hour;
    ///   into light  an exponential with a time constant of 8 game seconds (light adaptation takes seconds): a lamp lit
    ///               on deck takes a dark-adapted eye under a fifth within 15 game seconds, a quarter of a real second.
    /// Stepped in game minutes (paused, it holds; at 4x it adapts four times as fast in real time, as the clock runs);
    /// a time jump is stepped exactly (an hour forward is an hour of adapting), a step back in time settles it.
    /// What it gives: the exposure's floor for HDRP (LimitMin), the night fill, the colour drained from the shadows and
    /// midtones, the grain, the Moon's perceived tint and the lookout's night sight. The camera's three night cheats
    /// (the Moon's boost, the night fill, the stars at x5) stand in for the rods' sensitivity: they are documented in the
    /// design (2.2) and are not to be "fixed".
    /// </summary>
    public sealed class EyeModel
    {
        /// <summary>The scene EV100 at and above which the eye is the day's (no adaptation).</summary>
        public const double DayEv = 2.0;
        /// <summary>The scene EV100 at and below which the eye heads for full dark adaptation.</summary>
        public const double NightEv = -8.0;

        /// <summary>Time constant of dark adaptation, game minutes (the rods: 20-30 minutes).</summary>
        public double darkMinutes = 20.0;
        /// <summary>Time constant of light adaptation, game minutes (8 game seconds).</summary>
        public double lightMinutes = 8.0 / 60.0;
        /// <summary>The exposure's floor (EV) for the day's eye and for the fully dark-adapted one: the floor follows the
        /// eye, so the camera can open up only as far as the eye has adapted.</summary>
        public double dayFloor = 2.0, nightFloor = -6.0;
        /// <summary>The night fill's light at full adaptation, lux (20x the real starlight: the rods' cheat).</summary>
        public double fillLux = 0.04;
        /// <summary>The colour drained from the shadows and midtones at full adaptation (a saturation offset, -100..0;
        /// capped at -50 so a fire in frame, which still drives the cones, keeps its orange).</summary>
        public double fullDrain = -50.0;
        /// <summary>The film grain by day and at full adaptation (the noise of low light; it keeps the night from banding).</summary>
        public double dayGrain = 0.28, nightGrain = 0.45;
        /// <summary>The lookout's night sight with no adaptation, as a share of his full night range (Sight.RangeKm).</summary>
        public double unadaptedSight = 0.2;
        /// <summary>The Moon's light as the dark-adapted eye sees it (the Purkinje shift: blue-silver), linear RGB.</summary>
        public double moonBlueR = 0.78, moonBlueG = 0.85, moonBlueB = 1.0;

        /// <summary>0 the day's eye .. 1 fully dark-adapted.</summary>
        public double Adaptation { get; private set; }

        /// <summary>The adaptation a scene of an EV100 asks for: 0 at EV 2 and above, 1 at EV -8 and below.</summary>
        public static double Target(double sceneEv100)
        {
            if (double.IsNaN(sceneEv100))
                return 0.0;
            double t = (sceneEv100 - DayEv) / (NightEv - DayEv);
            return t < 0.0 ? 0.0 : t > 1.0 ? 1.0 : t;
        }

        /// <summary>
        /// Moves the eye on by so many game minutes in a scene of an EV100 (the light it is looking at: the sky and the
        /// ground, plus any lamp or flash in sight). Into the dark slowly, into the light fast, both exactly for any step.
        /// No time passing (paused) holds it; time going backwards settles it on the scene.
        /// </summary>
        public void Step(double dtGameMinutes, double sceneEv100)
        {
            double target = Target(sceneEv100);
            if (double.IsNaN(dtGameMinutes) || dtGameMinutes < 0.0)
            {
                Adaptation = target;
                return;
            }
            if (dtGameMinutes == 0.0)
                return;
            double tau = target > Adaptation ? darkMinutes : lightMinutes;
            if (!(tau > 0.0))
            {
                Adaptation = target;
                return;
            }
            Adaptation = target + (Adaptation - target) * Math.Exp(-dtGameMinutes / tau);
        }

        /// <summary>Puts the eye straight at what a scene asks for, as if it had been there a long while (a loaded game,
        /// a level opened in the editor, a shot, the clock set back).</summary>
        public void Settle(double sceneEv100) => Adaptation = Target(sceneEv100);

        /// <summary>Sets the adaptation outright (a saved game's eye), clamped to 0..1.</summary>
        public void SetAdaptation(double adaptation) =>
            Adaptation = double.IsNaN(adaptation) ? 0.0 : adaptation < 0.0 ? 0.0 : adaptation > 1.0 ? 1.0 : adaptation;

        static double Lerp(double a, double b, double t) => a + (b - a) * t;

        static double SmoothStep(double from, double to, double x)
        {
            double t = (x - from) / (to - from);
            t = t < 0.0 ? 0.0 : t > 1.0 ? 1.0 : t;
            return t * t * (3.0 - 2.0 * t);
        }

        /// <summary>The floor of HDRP's automatic exposure (EV): the day's floor coming down to the night's as the eye adapts.</summary>
        public double LimitMin => Lerp(dayFloor, nightFloor, Adaptation);

        /// <summary>The night fill's light, lux: it comes in only as the rods adapt.</summary>
        public double FillLux => fillLux * Adaptation;

        /// <summary>The saturation offset on the shadows and midtones (0 .. -50): rod vision is grey, and the colour goes
        /// only in the last stretch of adapting (from 0.6 to 1).</summary>
        public double ColourDrain => Lerp(0.0, fullDrain, SmoothStep(0.6, 1.0, Adaptation));

        /// <summary>The film grain's intensity.</summary>
        public double Grain => Lerp(dayGrain, nightGrain, Adaptation);

        /// <summary>The lookout's night term in Sight.RangeKm, scaled: 0.2 with no adaptation, 1 at full (a lit pot drops
        /// him at once; 20 game minutes of dark bring most of it back).</summary>
        public double NightSightScale => Lerp(unadaptedSight, 1.0, Adaptation);

        /// <summary>The Moon's light as the eye sees it, linear RGB: neutral to the day's eye, blue-silver to the
        /// dark-adapted one (the Purkinje shift is the eye's, not the Moon's).</summary>
        public void MoonTint(out double r, out double g, out double b)
        {
            double a = Adaptation;
            r = Lerp(1.0, moonBlueR, a);
            g = Lerp(1.0, moonBlueG, a);
            b = Lerp(1.0, moonBlueB, a);
        }
    }
}
