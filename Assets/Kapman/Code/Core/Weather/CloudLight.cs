using System;

namespace Kapman.Weather
{
    /// <summary>The kinds of cloud the weather engine names (weather document 1.1: low, middle, high and through the depths).</summary>
    public enum CloudType : byte
    {
        /// <summary>No cloud worth the name.</summary>
        Clear,
        /// <summary>High (8-10 km): mares' tails, thin, 12-24 hours ahead of a warm front.</summary>
        Cirrus,
        /// <summary>High: a milky veil, the ring round the Sun; a few hundred km ahead of a warm front.</summary>
        Cirrostratus,
        /// <summary>Middle (2-7 km): patches and rolls, a mackerel sky.</summary>
        Altocumulus,
        /// <summary>Middle: a grey sheet, the Sun a pale disc through it; thickening ahead of a warm front, light rain.</summary>
        Altostratus,
        /// <summary>Low: lumpy grey rolls with gaps; the grey of a settled winter high.</summary>
        Stratocumulus,
        /// <summary>Low: heaped fair-weather cloud, and the showers of cold air.</summary>
        Cumulus,
        /// <summary>Low: a flat grey sheet at 150-400 m; drizzle, the warm sector, lifted fog.</summary>
        Stratus,
        /// <summary>Through the depths: the thick dark deck of steady rain at a warm front, its base 300-800 m.</summary>
        Nimbostratus,
        /// <summary>Through the depths: the thunder cloud, downpours, hail and squalls; a cold front's line, a summer storm.</summary>
        Cumulonimbus,
    }

    /// <summary>Which front of a low a place stands nearest.</summary>
    public enum FrontKind : byte
    {
        None,
        /// <summary>The warm front: cloud thickening and lowering ahead of it, steady rain at it, the warm sector behind.</summary>
        Warm,
        /// <summary>The cold front: a line of thunder cloud and downpours, the wind veering, clearer colder air behind.</summary>
        Cold,
    }

    /// <summary>
    /// HOW MUCH LIGHT A CLOUD LETS THROUGH (weather document 2.5). The Sun's lux on the ground is scaled by the cloud
    /// column's transmittance, from a table by kind of cloud that reproduces the measured light (Wikipedia, Daylight:
    /// 100,000 lux clear at noon, 1,000-2,000 under an overcast, under 200 under a very dark storm deck):
    ///
    ///   kind                              optical depth (for the drawing)   transmittance where it covers
    ///   clear                             0                                 1
    ///   cirrus, cirrostratus              0.1-2                             0.8-0.9
    ///   altostratus                       5-20                              0.2-0.5
    ///   altocumulus                       3-10 (guess)                      0.35-0.6 (guess: between the veil and the sheet)
    ///   stratocumulus, fair cumulus       10-30                             0.1-0.2
    ///   stratus                           15-40                             0.03-0.08
    ///   nimbostratus                      50-200                            0.01-0.025
    ///   cumulonimbus (under the core)     100-500                           0.002-0.005
    ///
    /// A layer passes cover x T + (1 - cover) of what reaches it, and the layers multiply (a thin veil above a deck dims
    /// it further). No value ever exceeds the two-stream estimate of a deck's diffuse transmittance, 1 / (1 + 0.75 tau
    /// (1 - g)) with g 0.85, which is the physical ceiling; the table sits under it everywhere. Pure arithmetic.
    /// </summary>
    public static class CloudLight
    {
        /// <summary>The asymmetry factor of cloud droplets' scattering (forward), for the two-stream ceiling.</summary>
        public const double Asymmetry = 0.85;

        /// <summary>The table's transmittance where the kind covers the sky: from its thinnest (hi) to its thickest (lo).</summary>
        public static void TransmittanceRange(CloudType type, out double thick, out double thin)
        {
            switch (type)
            {
                case CloudType.Cirrus: thick = 0.85; thin = 0.9; return;
                case CloudType.Cirrostratus: thick = 0.8; thin = 0.88; return;
                case CloudType.Altocumulus: thick = 0.35; thin = 0.6; return;
                case CloudType.Altostratus: thick = 0.2; thin = 0.5; return;
                case CloudType.Stratocumulus: thick = 0.1; thin = 0.2; return;
                case CloudType.Cumulus: thick = 0.1; thin = 0.2; return;
                case CloudType.Stratus: thick = 0.03; thin = 0.08; return;
                case CloudType.Nimbostratus: thick = 0.01; thin = 0.025; return;
                case CloudType.Cumulonimbus: thick = 0.002; thin = 0.005; return;
                default: thick = 1.0; thin = 1.0; return;
            }
        }

        /// <summary>The kind's optical depth range (for the cloud drawing only: the light comes from the table).</summary>
        public static void OpticalDepthRange(CloudType type, out double thin, out double thick)
        {
            switch (type)
            {
                case CloudType.Cirrus: thin = 0.1; thick = 1.0; return;
                case CloudType.Cirrostratus: thin = 0.5; thick = 2.0; return;
                case CloudType.Altocumulus: thin = 3.0; thick = 10.0; return;
                case CloudType.Altostratus: thin = 5.0; thick = 20.0; return;
                case CloudType.Stratocumulus: thin = 10.0; thick = 30.0; return;
                case CloudType.Cumulus: thin = 10.0; thick = 30.0; return;
                case CloudType.Stratus: thin = 15.0; thick = 40.0; return;
                case CloudType.Nimbostratus: thin = 50.0; thick = 200.0; return;
                case CloudType.Cumulonimbus: thin = 100.0; thick = 500.0; return;
                default: thin = 0.0; thick = 0.0; return;
            }
        }

        /// <summary>The optical depth of a kind at a thickness (0 its thinnest .. 1 its thickest).</summary>
        public static double OpticalDepth(CloudType type, double thickness)
        {
            OpticalDepthRange(type, out double thin, out double thick);
            return thin + (thick - thin) * Clamp01(thickness);
        }

        /// <summary>The two-stream ceiling on a deck's diffuse transmittance: 1 / (1 + 0.75 tau (1 - g)).</summary>
        public static double TwoStream(double opticalDepth) => 1.0 / (1.0 + 0.75 * Math.Max(0.0, opticalDepth) * (1.0 - Asymmetry));

        /// <summary>The transmittance where a kind covers the sky, at a thickness (0 its thinnest .. 1 its thickest), never
        /// over the two-stream ceiling.</summary>
        public static double Transmittance(CloudType type, double thickness)
        {
            if (type == CloudType.Clear)
                return 1.0;
            TransmittanceRange(type, out double thick, out double thin);
            double t = Clamp01(thickness);
            double table = thin + (thick - thin) * t;
            return Math.Min(table, TwoStream(OpticalDepth(type, t)));
        }

        /// <summary>What one layer passes of the light reaching it: cover x T + (1 - cover).</summary>
        public static double Layer(CloudType type, double cover, double thickness)
        {
            double c = Clamp01(cover);
            return c * Transmittance(type, thickness) + (1.0 - c);
        }

        /// <summary>The whole column: the low, middle and high layers' shares multiplied (0..1).</summary>
        public static double Column(CloudType lowType, double low, double lowThickness,
                                    CloudType midType, double mid, double midThickness,
                                    CloudType highType, double high, double highThickness) =>
            Layer(lowType, low, lowThickness) * Layer(midType, mid, midThickness) * Layer(highType, high, highThickness);

        /// <summary>The kind's name in plain words.</summary>
        public static string Name(CloudType type)
        {
            switch (type)
            {
                case CloudType.Cirrus: return "cirrus";
                case CloudType.Cirrostratus: return "cirrostratus";
                case CloudType.Altocumulus: return "altocumulus";
                case CloudType.Altostratus: return "altostratus";
                case CloudType.Stratocumulus: return "stratocumulus";
                case CloudType.Cumulus: return "cumulus";
                case CloudType.Stratus: return "stratus";
                case CloudType.Nimbostratus: return "nimbostratus";
                case CloudType.Cumulonimbus: return "cumulonimbus";
                default: return "clear";
            }
        }

        static double Clamp01(double x) => x < 0.0 ? 0.0 : x > 1.0 ? 1.0 : x;
    }
}
