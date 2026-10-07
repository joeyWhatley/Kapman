using System;

namespace Kapman.World
{
    /// <summary>
    /// THE LIGHT ON THE GROUND - how many lux reach a flat surface at 52 N from the Sun, the sky, the twilight, the Moon
    /// and the stars, for a Sun altitude, a cloud deck, a storm and a fog (Design/lighting_and_atmosphere.md 2.1), and
    /// the exposure value a camera or an eye would meet there (2.2). Pure arithmetic, no scene: the sky dome, the eye and
    /// the tests all ask it the same questions. Altitudes in degrees, true (before the air's lift).
    ///
    /// The anchors, from Wikipedia's Daylight and Lux pages and HDRP's Physical Light Units table (read 4 October 2026):
    /// 100,000 lux at a clear June noon; 1,000-2,000 under a full overcast at noon; under 200 under storm clouds; 400 at a
    /// clear sunset; 40 at an overcast one; 3.4 at the end of civil twilight (the Sun 6 degrees under); 0.002 from the
    /// stars and the airglow on a clear moonless night, 0.0001 under an overcast one. The shape between anchors is the
    /// design document's simple model (extraterrestrial 133 klux, extinction 0.25 per Kasten-Young air mass, the blue
    /// sky as 10% of the extraterrestrial times the square root of the Sun's sine), good to about 20%.
    /// </summary>
    public static class LightingModel
    {
        /// <summary>The Sun's light outside the air, lux, as this model's anchor (it returns 100,000 at the June noon).</summary>
        public const double SunOutsideAirLux = 133000.0;
        /// <summary>Optical depth of the clear air per air mass (the air and a little haze).</summary>
        public const double Extinction = 0.25;
        /// <summary>The clear blue sky's light on the flat with the Sun at the zenith, lux (12,500 at the June noon).</summary>
        public const double ClearSkyLux = 13300.0;
        /// <summary>A full overcast's light at the June noon, lux (the middle of the cited 1,000-2,000).</summary>
        public const double OvercastNoonLux = 1500.0;
        /// <summary>The Sun's noon altitude on 21 June at 52 N, degrees: the overcast is anchored there.</summary>
        public const double JuneNoonAltitude = 61.4;
        /// <summary>The light at sunset, clear, lux (the Daylight page).</summary>
        public const double SunsetLux = 400.0;
        /// <summary>The light at the end of civil twilight (the Sun 6 degrees under), lux.</summary>
        public const double CivilDuskLux = 3.4;
        /// <summary>The stars and the airglow on a clear moonless night, and under an overcast one, lux.</summary>
        public const double StarsClearLux = 0.002, StarsOvercastLux = 0.0001;
        /// <summary>A grey card's share of the light it gets (18%), the reflectance exposure is calibrated against.</summary>
        public const double GreyCard = 0.18;
        /// <summary>The reflected-light meter's calibration constant (ISO 2720; HDRP's ConvertEV100ToExposure uses it).</summary>
        public const double MeterK = 12.5;
        /// <summary>The faintest light this model counts, lux: darker than any night the game has (an overcast moonless
        /// one is 0.0001), so the exposure of a black scene comes out as a number instead of minus infinity.</summary>
        public const double FloorLux = 1e-6;

        const double D2R = Math.PI / 180.0;

        static double Clamp01(double v) => v > 0.0 ? (v < 1.0 ? v : 1.0) : 0.0;      // (not a number counts as 0)
        static double SinUp(double altitude) => altitude > 0.0 ? Math.Sin(Math.Min(altitude, 90.0) * D2R) : 0.0;

        // ------------------------------------------------------------------------------------------------ the Sun

        /// <summary>The clear-sky Sun's light on a surface square to it, lux: what a sail or a hull side facing the low
        /// Sun gets (thousands of lux just after sunrise, while the flat ground gets a few hundred). 0 once the Sun's
        /// centre is under the horizon.</summary>
        public static double SunNormalLux(double altitude)
        {
            if (!(altitude > 0.0))
                return 0.0;
            return SunOutsideAirLux * Math.Exp(-Extinction * SkyMath.Airmass(Math.Min(altitude, 90.0)));
        }

        /// <summary>The clear-sky Sun's light on the flat ground, lux (88,000 at the June noon, 0 at sunset).</summary>
        public static double DirectLux(double altitude) => SunNormalLux(altitude) * SinUp(altitude);

        /// <summary>
        /// The stand-in for the cloud deck's transmittance until the weather engine gives its own
        /// (WeatherSample.cloudTransmittance, weather 2.5; GAME_PLAN 2.4 rule 2): (1 - overcast)^2 (1 - storm), so a full
        /// deck lets no direct Sun through at all. Overcast and storm 0..1.
        /// </summary>
        public static double CloudTransmittance(double overcast, double storm)
        {
            double o = 1.0 - Clamp01(overcast);
            return o * o * (1.0 - Clamp01(storm));
        }

        /// <summary>
        /// The one cloud factor on the Sun's direct light: the deck's transmittance (0 a full deck .. 1 a clear sky),
        /// clamped. Not a number counts as a clear sky, so a weather that has not sampled yet never blacks the Sun out.
        /// </summary>
        public static double CloudDim(double transmittance) => double.IsNaN(transmittance) ? 1.0 : Clamp01(transmittance);

        /// <summary>
        /// The factor the clock dims HDRP's Sun by when HDRP's own volumetric clouds also shadow it (lighting 4.2): the
        /// model's cloud factor divided by what HDRP's clouds leave (1 - their shadow opacity times the cover; the share
        /// they take, 0..1, is the argument), so the ground gets the model's lux once, not the model's times HDRP's.
        /// Never above 1: where HDRP's clouds already take more than the model asks, the Sun is left at full strength.
        /// </summary>
        public static double CloudDim(double transmittance, double hdrpCloudShadow)
        {
            double left = 1.0 - Clamp01(hdrpCloudShadow);
            double dim = CloudDim(transmittance);
            if (left <= 1e-6)
                return dim > 0.0 ? 1.0 : 0.0;
            return Math.Min(1.0, dim / left);
        }

        /// <summary>How much of the Sun gets through a fog layer of a depth (m) whose mean free path is fogMfp (m), along
        /// the slanted path through the layer (so a low Sun loses more). An infinite or non-positive free path, or a
        /// layer of no depth, is no fog.</summary>
        public static double FogTransmittance(double altitude, double fogMfp, double fogHeight)
        {
            if (!(fogMfp > 0.0) || double.IsInfinity(fogMfp) || !(fogHeight > 0.0))
                return 1.0;
            double a = altitude > 0.0 ? Math.Min(altitude, 90.0) : 0.0;
            return Math.Exp(-fogHeight * SkyMath.Airmass(a) / fogMfp);
        }

        /// <summary>The Sun's direct light on the flat ground, lux, through a cloud deck of a transmittance (0..1; the
        /// weather engine's sample once it lands) and a fog.</summary>
        public static double SunLuxThrough(double altitude, double cloudTransmittance, double fogMfp = double.PositiveInfinity, double fogHeight = 0.0) =>
            DirectLux(altitude) * CloudDim(cloudTransmittance) * FogTransmittance(altitude, fogMfp, fogHeight);

        /// <summary>The Sun's direct light on the flat ground, lux, under an overcast and a storm (0..1) and in a fog,
        /// through the stand-in transmittance (0 under a full deck).</summary>
        public static double SunLux(double altitude, double overcast, double storm, double fogMfp = double.PositiveInfinity, double fogHeight = 0.0) =>
            SunLuxThrough(altitude, CloudTransmittance(overcast, storm), fogMfp, fogHeight);

        // ------------------------------------------------------------------------------------------------ the sky

        /// <summary>
        /// The twilight's light, lux: a two-piece log fit through the three cited anchors, 400 at the horizon, 3.4 at -6
        /// degrees (civil dusk) and 2e-4 at -18 (0.03 at -12 falls out of the second piece; no page gives it), nothing
        /// below -18. Above the horizon it hands over to the blue sky within the first 2 degrees, so the sum stays smooth
        /// through sunrise.
        /// </summary>
        public static double TwilightLux(double altitude)
        {
            if (double.IsNaN(altitude) || altitude < -18.0)
                return 0.0;
            if (altitude <= -6.0)
                return CivilDuskLux * Math.Pow(10.0, (altitude + 6.0) / 2.84);
            if (altitude <= 0.0)
                return SunsetLux * Math.Pow(10.0, altitude / 2.90);
            return altitude < 2.0 ? SunsetLux * (1.0 - altitude / 2.0) : 0.0;
        }

        /// <summary>The clear blue sky's light on the flat, lux (12,500 at the June noon; 0 at the horizon, where the
        /// twilight term carries the 400).</summary>
        public static double ClearSkyLuxAt(double altitude) => ClearSkyLux * Math.Sqrt(SinUp(altitude));

        /// <summary>A full overcast's light on the flat, lux: 1,500 at the June noon, about 430 at a December one.</summary>
        public static double OvercastSkyLuxAt(double altitude) => OvercastNoonLux * SinUp(altitude) / Math.Sin(JuneNoonAltitude * D2R);

        /// <summary>
        /// The sky's light on the flat ground, lux: the clear sky and the overcast mixed by the overcast, a storm deck
        /// taking nine tenths of it (150 lux at a June noon), plus the twilight, which an overcast and a storm each cut to
        /// a tenth (40 lux at an overcast sunset, as cited; 4 under a storm, where the cited figure is under 1: right to
        /// the order).
        /// </summary>
        public static double SkyLux(double altitude, double overcast, double storm)
        {
            double o = Clamp01(overcast), s = Clamp01(storm);
            double clear = ClearSkyLuxAt(altitude);
            double day = clear + (OvercastSkyLuxAt(altitude) - clear) * o;
            return day * (1.0 - 0.9 * s) + TwilightLux(altitude) * (1.0 - 0.9 * o) * (1.0 - 0.9 * s);
        }

        // ------------------------------------------------------------------------------------------------ the night

        /// <summary>
        /// The Moon's light on the flat ground, lux: the project's one Moon (Sky.MoonLux, from SkyMath's magnitude by
        /// phase and distance: 0.31 full, 0.028 at a quarter) times the sine of its altitude, a full overcast leaving 30%.
        /// The camera's boost (the clock's moonBoost) is not in it: this is the real light.
        /// </summary>
        public static double MoonLux(double skyMoonLux, double moonAltitude, double overcast) =>
            (skyMoonLux > 0.0 ? skyMoonLux : 0.0) * SinUp(moonAltitude) * (1.0 - 0.7 * Clamp01(overcast));

        /// <summary>The stars and the airglow, lux: 0.002 clear, 0.0001 under a full overcast.</summary>
        public static double StarLux(double overcast) => StarsClearLux + (StarsOvercastLux - StarsClearLux) * Clamp01(overcast);

        /// <summary>
        /// Everything on the flat ground, lux: the Sun, the sky with its twilight, the Moon and the stars. The Sun and
        /// the sky are both sunlight, so an eclipse (the share of the Sun's disc covered, 0..1) takes from both.
        /// </summary>
        public static double TotalLux(double sunAltitude, double overcast, double storm, double skyMoonLux, double moonAltitude, double sunCovered = 0.0) =>
            (SunLux(sunAltitude, overcast, storm) + SkyLux(sunAltitude, overcast, storm)) * (1.0 - Clamp01(sunCovered))
            + MoonLux(skyMoonLux, moonAltitude, overcast) + StarLux(overcast);

        // ------------------------------------------------------------------------------------------------ exposure

        /// <summary>The luminance (cd/m2) of a matte surface of an albedo under an illuminance (lux): E a / pi, the
        /// illuminance floored at <see cref="FloorLux"/>.</summary>
        public static double Luminance(double lux, double albedo = GreyCard) =>
            (lux > FloorLux ? lux : FloorLux) * albedo / Math.PI;

        /// <summary>The exposure value at ISO 100 that renders a luminance (cd/m2) as middle grey: log2(L 100 / 12.5),
        /// the luminance floored at a grey card's under <see cref="FloorLux"/>.</summary>
        public static double EV100FromLuminance(double luminance)
        {
            double floor = FloorLux * GreyCard / Math.PI;
            return Math.Log((luminance > floor ? luminance : floor) * 100.0 / MeterK, 2.0);
        }

        /// <summary>The exposure value of a scene lit by so many lux on an 18% grey ground (lighting 2.2): 15.5 at a June
        /// noon, 9.4 under its overcast, -2.8 under a true full Moon, -10.1 by starlight.</summary>
        public static double EV100FromLux(double lux, double albedo = GreyCard) => EV100FromLuminance(Luminance(lux, albedo));

        /// <summary>HDRP's exposure multiplier for an EV100: 1 / (1.2 2^EV) (Common.hlsl ConvertEV100ToExposure).</summary>
        public static double ExposureFromEV100(double ev100) => 1.0 / (1.2 * Math.Pow(2.0, ev100));
    }
}
