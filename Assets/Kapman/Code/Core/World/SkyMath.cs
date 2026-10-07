using System;

namespace Kapman.World
{
    /// <summary>The naked-eye planets (their row in the JPL elements; 2 is the Earth-Moon barycentre).</summary>
    public enum Planet { Mercury = 0, Venus = 1, Mars = 3, Jupiter = 4, Saturn = 5 }

    /// <summary>
    /// SKY MATHS - where the Sun, the Moon, the planets and the stars stood, for any moment of 793-1066 seen from
    /// anywhere on the map (Plans/environment.md, step A; the research in Plans/environment_research/sky_maths.md).
    /// Following Meeus, Astronomical Algorithms (2nd ed.): time and Delta T (Espenak-Meeus polynomial, 500-1600 AD),
    /// sidereal time (12.4), the Sun (ch. 25), the Moon (ch. 47, all terms; phase and brightness ch. 48), the planets
    /// from JPL's "Approximate positions of the planets" (Standish; tables 2a/2b, valid 3000 BC - 3000 AD) precessed
    /// to the date (21.5-21.7), IAU 2006 precession for the stars, refraction (Saemundsson) and airmass (Kasten-Young).
    /// Checked against JPL Horizons (DE441) at 793, 800, 950 and 1066 (Tests/EditMode/SkyTests.cs): the Sun within
    /// 0.01 degree, the Moon 0.01, the planets 0.2; the eclipses of 793-815 on their hours. Degrees and days
    /// throughout; Julian Days on the Julian calendar (as the game's clock); UT unless named "jde" (terrestrial time).
    /// Directions on the sky come out as (east, up, north), the game's x, y, z.
    /// </summary>
    public static class SkyMath
    {
        public const double D2R = Math.PI / 180.0;
        public const double AuKm = 149597870.7;
        const double EarthRadiusKm = 6378.14, MoonRadiusKm = 1737.4, SunRadiusKm = 696000.0;

        static double N360(double a) { a %= 360.0; return a < 0 ? a + 360.0 : a; }
        static double Wrap180(double a) { a = N360(a); return a > 180.0 ? a - 360.0 : a; }
        static double S(double deg) => Math.Sin(deg * D2R);
        static double C(double deg) => Math.Cos(deg * D2R);

        // ------------------------------------------------------------------------------------------------ time

        /// <summary>The Julian Day of a Julian-calendar date and UT hour (Meeus 7.1, B = 0).</summary>
        public static double JulianDay(int year, int month, int day, double hour = 0.0)
        {
            if (month <= 2)
            {
                year -= 1;
                month += 12;
            }
            return Math.Floor(365.25 * (year + 4716)) + Math.Floor(30.6001 * (month + 1)) + day - 1524.5 + hour / 24.0;
        }

        /// <summary>The year (with its fraction) of a Julian Day.</summary>
        public static double YearOf(double jd) => 2000.0 + (jd - 2451545.0) / 365.25;

        /// <summary>Seconds by which the Earth's turning had fallen behind steady time (Espenak & Meeus 2006,
        /// 500-1600 AD: about 50 minutes in 793, 21 in 1066).</summary>
        public static double DeltaT(double year)
        {
            double u = (Math.Max(500.0, Math.Min(1600.0, year)) - 1000.0) / 100.0;
            return 1574.2 - 556.01 * u + 71.23472 * u * u + 0.319781 * u * u * u - 0.8503463 * u * u * u * u
                   - 0.005050998 * Math.Pow(u, 5) + 0.0083572073 * Math.Pow(u, 6);
        }

        /// <summary>Terrestrial time (the planets' clock) for a UT Julian Day.</summary>
        public static double Jde(double jdUt) => jdUt + DeltaT(YearOf(jdUt)) / 86400.0;

        static double Centuries(double jd) => (jd - 2451545.0) / 36525.0;

        /// <summary>Greenwich mean sidereal time, degrees (Meeus 12.4).</summary>
        public static double Gmst(double jdUt)
        {
            double T = Centuries(jdUt);
            return N360(280.46061837 + 360.98564736629 * (jdUt - 2451545.0) + 0.000387933 * T * T - T * T * T / 38710000.0);
        }

        /// <summary>The mean obliquity of the ecliptic, degrees (Meeus 22.2; 23.595 in 800).</summary>
        public static double Obliquity(double T) =>
            23.0 + 26.0 / 60.0 + 21.448 / 3600.0 - (46.8150 * T + 0.00059 * T * T - 0.001813 * T * T * T) / 3600.0;

        /// <summary>Nutation in longitude, degrees (Meeus 22, to 0.5").</summary>
        public static double NutationLongitude(double T)
        {
            double om = 125.04452 - 1934.136261 * T, L = 280.4665 + 36000.7698 * T, Lp = 218.3165 + 481267.8813 * T;
            return (-17.20 * S(om) - 1.32 * S(2 * L) - 0.23 * S(2 * Lp) + 0.21 * S(2 * om)) / 3600.0;
        }

        // ------------------------------------------------------------------------------------------------ the Sun

        /// <summary>The Sun's apparent ecliptic longitude of the date (degrees) and its distance (AU) (Meeus ch. 25).</summary>
        public static void Sun(double jde, out double lambda, out double rAu)
        {
            double T = Centuries(jde);
            double L0 = 280.46646 + 36000.76983 * T + 0.0003032 * T * T;
            double M = 357.52911 + 35999.05029 * T - 0.0001537 * T * T;
            double e = 0.016708634 - 0.000042037 * T - 0.0000001267 * T * T;
            double c = (1.914602 - 0.004817 * T - 0.000014 * T * T) * S(M) + (0.019993 - 0.000101 * T) * S(2 * M) + 0.000289 * S(3 * M);
            rAu = 1.000001018 * (1 - e * e) / (1 + e * C(M + c));
            double om = 125.04 - 1934.136 * T;
            lambda = N360(L0 + c - 0.00569 - 0.00478 * S(om));
        }

        // ------------------------------------------------------------------------------------------------ the Moon

        static readonly int[,] MoonLR =
        {
            {0,0,1,0,6288774,-20905355},{2,0,-1,0,1274027,-3699111},{2,0,0,0,658314,-2955968},{0,0,2,0,213618,-569925},
            {0,1,0,0,-185116,48888},{0,0,0,2,-114332,-3149},{2,0,-2,0,58793,246158},{2,-1,-1,0,57066,-152138},
            {2,0,1,0,53322,-170733},{2,-1,0,0,45758,-204586},{0,1,-1,0,-40923,-129620},{1,0,0,0,-34720,108743},
            {0,1,1,0,-30383,104755},{2,0,0,-2,15327,10321},{0,0,1,2,-12528,0},{0,0,1,-2,10980,79661},
            {4,0,-1,0,10675,-34782},{0,0,3,0,10034,-23210},{4,0,-2,0,8548,-21636},{2,1,-1,0,-7888,24208},
            {2,1,0,0,-6766,30824},{1,0,-1,0,-5163,-8379},{1,1,0,0,4987,-16675},{2,-1,1,0,4036,-12831},
            {2,0,2,0,3994,-10445},{4,0,0,0,3861,-11650},{2,0,-3,0,3665,14403},{0,1,-2,0,-2689,-7003},
            {2,0,-1,2,-2602,0},{2,-1,-2,0,2390,10056},{1,0,1,0,-2348,6322},{2,-2,0,0,2236,-9884},
            {0,1,2,0,-2120,5751},{0,2,0,0,-2069,0},{2,-2,-1,0,2048,-4950},{2,0,1,-2,-1773,4130},
            {2,0,0,2,-1595,0},{4,-1,-1,0,1215,-3958},{0,0,2,2,-1110,0},{3,0,-1,0,-892,3258},
            {2,1,1,0,-810,2616},{4,-1,-2,0,759,-1897},{0,2,-1,0,-713,-2117},{2,2,-1,0,-700,2354},
            {2,1,-2,0,691,0},{2,-1,0,-2,596,0},{4,0,1,0,549,-1423},{0,0,4,0,537,-1117},
            {4,-1,0,0,520,-1571},{1,0,-2,0,-487,-1739},{2,1,0,-2,-399,0},{0,0,2,-2,-381,-4421},
            {1,1,1,0,351,0},{3,0,-2,0,-340,0},{4,0,-3,0,330,0},{2,-1,2,0,327,0},
            {0,2,1,0,-323,1165},{1,1,-1,0,299,0},{2,0,3,0,294,0},{2,0,-1,-2,0,8752},
        };

        static readonly int[,] MoonB =
        {
            {0,0,0,1,5128122},{0,0,1,1,280602},{0,0,1,-1,277693},{2,0,0,-1,173237},{2,0,-1,1,55413},{2,0,-1,-1,46271},
            {2,0,0,1,32573},{0,0,2,1,17198},{2,0,1,-1,9266},{0,0,2,-1,8822},{2,-1,0,-1,8216},{2,0,-2,-1,4324},
            {2,0,1,1,4200},{2,1,0,-1,-3359},{2,-1,-1,1,2463},{2,-1,0,1,2211},{2,-1,-1,-1,2065},{0,1,-1,-1,-1870},
            {4,0,-1,-1,1828},{0,1,0,1,-1794},{0,0,0,3,-1749},{0,1,-1,1,-1565},{1,0,0,1,-1491},{0,1,1,1,-1475},
            {0,1,1,-1,-1410},{0,1,0,-1,-1344},{1,0,0,-1,-1335},{0,0,3,1,1107},{4,0,0,-1,1021},{4,0,-1,1,833},
            {0,0,1,-3,777},{4,0,-2,1,671},{2,0,0,-3,607},{2,0,2,-1,596},{2,-1,1,-1,491},{2,0,-2,1,-451},
            {0,0,3,-1,439},{2,0,2,1,422},{2,0,-3,-1,421},{2,1,-1,1,-366},{2,1,0,1,-351},{4,0,0,1,331},
            {2,-1,1,1,315},{2,-2,0,-1,302},{0,0,1,3,-283},{2,1,1,-1,-229},{1,1,0,-1,223},{1,1,0,1,223},
            {0,1,-2,-1,-220},{2,1,-1,-1,-220},{1,0,1,1,-185},{2,-1,-2,-1,181},{0,1,2,1,-177},{4,0,-2,-1,176},
            {4,-1,-1,-1,166},{1,0,1,-1,-164},{4,0,1,-1,132},{1,0,-1,-1,-119},{4,-1,0,-1,115},{2,-2,0,1,107},
        };

        /// <summary>The Moon's apparent geocentric ecliptic longitude and latitude of the date (degrees) and its distance
        /// (km) (Meeus ch. 47, every term; plus nutation in longitude).</summary>
        public static void Moon(double jde, out double lambda, out double beta, out double distKm)
        {
            double T = Centuries(jde), T2 = T * T, T3 = T2 * T, T4 = T3 * T;
            double Lp = 218.3164477 + 481267.88123421 * T - 0.0015786 * T2 + T3 / 538841.0 - T4 / 65194000.0;
            double D = 297.8501921 + 445267.1114034 * T - 0.0018819 * T2 + T3 / 545868.0 - T4 / 113065000.0;
            double M = 357.5291092 + 35999.0502909 * T - 0.0001536 * T2 + T3 / 24490000.0;
            double Mp = 134.9633964 + 477198.8675055 * T + 0.0087414 * T2 + T3 / 69699.0 - T4 / 14712000.0;
            double F = 93.2720950 + 483202.0175233 * T - 0.0036539 * T2 - T3 / 3526000.0 + T4 / 863310000.0;
            double A1 = 119.75 + 131.849 * T, A2 = 53.09 + 479264.290 * T, A3 = 313.45 + 481266.484 * T;
            double E = 1 - 0.002516 * T - 0.0000074 * T2;
            double sl = 0, sr = 0, sb = 0;
            for (int i = 0; i < 60; i++)
            {
                double arg = MoonLR[i, 0] * D + MoonLR[i, 1] * M + MoonLR[i, 2] * Mp + MoonLR[i, 3] * F;
                double ef = Math.Abs(MoonLR[i, 1]) == 1 ? E : Math.Abs(MoonLR[i, 1]) == 2 ? E * E : 1.0;
                sl += MoonLR[i, 4] * ef * S(arg);
                sr += MoonLR[i, 5] * ef * C(arg);
                double argb = MoonB[i, 0] * D + MoonB[i, 1] * M + MoonB[i, 2] * Mp + MoonB[i, 3] * F;
                double efb = Math.Abs(MoonB[i, 1]) == 1 ? E : Math.Abs(MoonB[i, 1]) == 2 ? E * E : 1.0;
                sb += MoonB[i, 4] * efb * S(argb);
            }
            sl += 3958 * S(A1) + 1962 * S(Lp - F) + 318 * S(A2);
            sb += -2235 * S(Lp) + 382 * S(A3) + 175 * S(A1 - F) + 175 * S(A1 + F) + 127 * S(Lp - Mp) - 115 * S(Lp + Mp);
            lambda = N360(Lp + sl / 1e6 + NutationLongitude(T));
            beta = sb / 1e6;
            distKm = 385000.56 + sr / 1000.0;
        }

        /// <summary>
        /// The Moon's phase seen from the Earth: the share of its disc lit (0 new .. 1 full), its phase angle (degrees,
        /// 0 full .. 180 new), whether it's waxing, its magnitude, and the light it gives on the ground (lux, above the
        /// air) (Meeus ch. 48; the magnitude as Allen / Horizons: -12.7 at full).
        /// </summary>
        public static void MoonPhase(double sunLambda, double sunRAu, double moonLambda, double moonBeta, double moonKm,
                                     out double lit, out double phaseAngle, out bool waxing, out double magnitude, out double lux)
        {
            double cpsi = C(moonBeta) * C(moonLambda - sunLambda);
            double psi = Math.Acos(Math.Max(-1.0, Math.Min(1.0, cpsi)));
            double R = sunRAu * AuKm;
            double i = Math.Atan2(R * Math.Sin(psi), moonKm - R * Math.Cos(psi));
            phaseAngle = i / D2R;
            lit = (1.0 + Math.Cos(i)) * 0.5;
            waxing = Wrap180(moonLambda - sunLambda) > 0.0;
            magnitude = -12.717 + 1.49 * Math.Abs(i) + 0.0431 * Math.Pow(i, 4) + 5.0 * Math.Log10(sunRAu * moonKm / 385000.6);
            lux = Lux(magnitude);
        }

        /// <summary>Lux on the ground from a body of this magnitude (above the air).</summary>
        public static double Lux(double magnitude) => 2.54e-6 * Math.Pow(10.0, -0.4 * magnitude);

        // ------------------------------------------------------------------------------------------------ the planets

        // JPL "Approximate positions of the planets" (Standish), table 2a (3000 BC - 3000 AD): a, a', e, e', I, I', L, L',
        // perihelion, perihelion', node, node' (AU, degrees, per century). Row 2 is the Earth-Moon barycentre.
        static readonly double[,] Elements =
        {
            {0.38709843,0.0,0.20563661,0.00002123,7.00559432,-0.00590158,252.25166724,149472.67486623,77.45771895,0.15940013,48.33961819,-0.12214182},
            {0.72332102,-0.00000026,0.00676399,-0.00005107,3.39777545,0.00043494,181.97970850,58517.81560260,131.76755713,0.05679648,76.67261496,-0.27274174},
            {1.00000018,-0.00000003,0.01673163,-0.00003661,-0.00054346,-0.01337178,100.46691572,35999.37306329,102.93005885,0.31795260,-5.11260389,-0.24123856},
            {1.52371243,0.00000097,0.09336511,0.00009149,1.85181869,-0.00724757,-4.56813164,19140.29934243,-23.91744784,0.45223625,49.71320984,-0.26852431},
            {5.20248019,-0.00002864,0.04853590,0.00018026,1.29861416,-0.00322699,34.33479152,3034.90371757,14.27495244,0.18199196,100.29282654,0.13024619},
            {9.54149883,-0.00003065,0.05550825,-0.00032044,2.49424102,0.00451969,50.07571329,1222.11494724,92.86136063,0.54179478,113.63998702,-0.25015002},
        };

        // table 2b: b, c, s, f for Jupiter and Saturn (the others none)
        static readonly double[,] Extra =
        {
            {0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},
            {-0.00012452,0.06064060,-0.35635438,38.35125000},{0.00025899,-0.13434469,0.87320147,38.35125000},
        };

        static void Heliocentric(int p, double T, out double x, out double y, out double z)
        {
            double a = Elements[p, 0] + Elements[p, 1] * T, e = Elements[p, 2] + Elements[p, 3] * T, I = Elements[p, 4] + Elements[p, 5] * T;
            double L = Elements[p, 6] + Elements[p, 7] * T, w = Elements[p, 8] + Elements[p, 9] * T, Om = Elements[p, 10] + Elements[p, 11] * T;
            double om = w - Om;
            double M = L - w + Extra[p, 0] * T * T + Extra[p, 1] * C(Extra[p, 3] * T) + Extra[p, 2] * S(Extra[p, 3] * T);
            M = Wrap180(M) * D2R;
            double E = M + e * Math.Sin(M);
            for (int i = 0; i < 20; i++)
                E -= (E - e * Math.Sin(E) - M) / (1 - e * Math.Cos(E));
            double xp = a * (Math.Cos(E) - e), yp = a * Math.Sqrt(1 - e * e) * Math.Sin(E);
            double co = C(om), so = S(om), cO = C(Om), sO = S(Om), cI = C(I), sI = S(I);
            x = (co * cO - so * sO * cI) * xp + (-so * cO - co * sO * cI) * yp;
            y = (co * sO + so * cO * cI) * xp + (-so * sO + co * cO * cI) * yp;
            z = (so * sI) * xp + (co * sI) * yp;
        }

        /// <summary>Ecliptic J2000 to the ecliptic of the date (Meeus 21.5-21.7), degrees.</summary>
        static void PrecessEcliptic(double T, double l0, double b0, out double l, out double b)
        {
            double eta = (47.0029 * T - 0.03302 * T * T + 0.000060 * T * T * T) / 3600.0;
            double Pi = 174.876384 + (-869.8089 * T + 0.03536 * T * T) / 3600.0;
            double p = (5029.0966 * T + 1.11113 * T * T - 0.000006 * T * T * T) / 3600.0;
            double A = C(eta) * C(b0) * S(Pi - l0) - S(eta) * S(b0);
            double B = C(b0) * C(Pi - l0);
            double Cc = C(eta) * S(b0) + S(eta) * C(b0) * S(Pi - l0);
            l = N360(p + Pi - Math.Atan2(A, B) / D2R);
            b = Math.Asin(Math.Max(-1.0, Math.Min(1.0, Cc))) / D2R;
        }

        /// <summary>A planet's apparent geocentric ecliptic longitude and latitude of the date (degrees), its distances
        /// from the Earth and the Sun (AU), and its magnitude.</summary>
        public static void PlanetPosition(Planet planet, double jde, out double lambda, out double beta, out double deltaAu, out double rAu, out double magnitude)
        {
            int p = (int)planet;
            double T = Centuries(jde);
            Heliocentric(2, T, out double ex, out double ey, out double ez);
            Heliocentric(p, T, out double px, out double py, out double pz);
            double d = Math.Sqrt((px - ex) * (px - ex) + (py - ey) * (py - ey) + (pz - ez) * (pz - ez));
            Heliocentric(p, T - 0.0057755183 * d / 36525.0, out px, out py, out pz);    // (light time)
            double dx = px - ex, dy = py - ey, dz = pz - ez;
            deltaAu = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            rAu = Math.Sqrt(px * px + py * py + pz * pz);
            double l0 = N360(Math.Atan2(dy, dx) / D2R), b0 = Math.Asin(dz / deltaAu) / D2R;
            PrecessEcliptic(T, l0, b0, out lambda, out beta);
            lambda = N360(lambda + NutationLongitude(T));
            double R = Math.Sqrt(ex * ex + ey * ey + ez * ez);
            double cosi = (rAu * rAu + deltaAu * deltaAu - R * R) / (2.0 * rAu * deltaAu);
            double i = Math.Acos(Math.Max(-1.0, Math.Min(1.0, cosi))) / D2R, five = 5.0 * Math.Log10(rAu * deltaAu);
            switch (planet)                                                         // (Meeus 41, the older formulae)
            {
                case Planet.Mercury: magnitude = -0.42 + five + 0.0380 * i - 0.000273 * i * i + 0.000002 * i * i * i; break;
                case Planet.Venus: magnitude = -4.40 + five + 0.0009 * i + 0.000239 * i * i - 0.00000065 * i * i * i; break;
                case Planet.Mars: magnitude = -1.52 + five + 0.016 * i; break;
                case Planet.Jupiter: magnitude = -9.40 + five + 0.005 * i; break;
                default: magnitude = -8.88 + five - 0.3; break;                   // (Saturn: the rings, on average)
            }
        }

        // ------------------------------------------------------------------------------------------------ to the sky

        /// <summary>Ecliptic (of the date) to equatorial (of the date), degrees.</summary>
        public static void EclipticToEquatorial(double lambda, double beta, double eps, out double ra, out double dec)
        {
            ra = N360(Math.Atan2(S(lambda) * C(eps) - Math.Tan(beta * D2R) * S(eps), C(lambda)) / D2R);
            dec = Math.Asin(Math.Max(-1.0, Math.Min(1.0, S(beta) * C(eps) + C(beta) * S(eps) * S(lambda)))) / D2R;
        }

        /// <summary>Equatorial (of the date) to the observer's sky: (east, up, north), a unit vector; LST and latitude in
        /// degrees.</summary>
        public static void ToHorizon(double ra, double dec, double lst, double lat, out double east, out double up, out double north)
        {
            double H = lst - ra;
            double x = C(dec) * C(H), y = -C(dec) * S(H), z = S(dec);
            east = y;
            up = S(lat) * z + C(lat) * x;
            north = C(lat) * z - S(lat) * x;
        }

        /// <summary>Degrees a body is lifted by the air at a true altitude (Saemundsson, 10 C, 1010 hPa).</summary>
        public static double Refraction(double altitude)
        {
            if (altitude < -2.0)
                return 0.0;
            double h = Math.Max(-1.9, altitude);
            return 1.02 / Math.Tan((h + 10.3 / (h + 5.11)) * D2R) / 60.0;
        }

        /// <summary>How much air a body's light comes through at an altitude, in zenith airmasses (Kasten-Young).</summary>
        public static double Airmass(double altitude)
        {
            double z = 90.0 - Math.Max(-1.0, altitude);
            return 1.0 / (C(z) + 0.50572 * Math.Pow(96.07995 - z, -1.6364));
        }

        /// <summary>The Moon seen from a place on the Earth (its parallax, up to a degree): topocentric RA and Dec.</summary>
        public static void Topocentric(double ra, double dec, double distKm, double lst, double lat, out double raTopo, out double decTopo, out double distTopo)
        {
            double mx = distKm * C(dec) * C(ra), my = distKm * C(dec) * S(ra), mz = distKm * S(dec);
            double ox = EarthRadiusKm * C(lat) * C(lst), oy = EarthRadiusKm * C(lat) * S(lst), oz = EarthRadiusKm * 0.99664719 * S(lat);
            double x = mx - ox, y = my - oy, z = mz - oz;
            distTopo = Math.Sqrt(x * x + y * y + z * z);
            raTopo = N360(Math.Atan2(y, x) / D2R);
            decTopo = Math.Asin(z / distTopo) / D2R;
        }

        // ------------------------------------------------------------------------------------------------ the stars

        /// <summary>
        /// IAU 2006 precession, J2000 equator to the equator of the date: v_date = P v_J2000, row-major 3x3. Equatorial
        /// unit vectors are (cos dec cos ra, cos dec sin ra, sin dec).
        /// </summary>
        public static double[] Precession(double jde)
        {
            double t = Centuries(jde), t2 = t * t, t3 = t2 * t, t4 = t3 * t, t5 = t4 * t;
            double zeta = (2.650545 + 2306.083227 * t + 0.2988499 * t2 + 0.01801828 * t3 - 0.000005971 * t4 - 0.0000003173 * t5) / 3600.0;
            double z = (-2.650545 + 2306.077181 * t + 1.0927348 * t2 + 0.01826837 * t3 - 0.000028596 * t4 - 0.0000002904 * t5) / 3600.0;
            double th = (2004.191903 * t - 0.4294934 * t2 - 0.04182264 * t3 - 0.000007089 * t4 - 0.0000001274 * t5) / 3600.0;
            // P = R3(-z) R2(theta) R3(-zeta)
            double cz = C(zeta), sz = S(zeta), cZ = C(z), sZ = S(z), ct = C(th), st = S(th);
            return new[]
            {
                cZ * ct * cz - sZ * sz, -cZ * ct * sz - sZ * cz, -cZ * st,
                sZ * ct * cz + cZ * sz, -sZ * ct * sz + cZ * cz, -sZ * st,
                st * cz, -st * sz, ct,
            };
        }

        /// <summary>A star's J2000 place moved on by its proper motion for so many years (pmRa = mu_alpha cos dec and
        /// pmDec in arcseconds a year), as an equatorial unit vector.</summary>
        public static void ProperMotion(double ra, double dec, double pmRa, double pmDec, double years, out double x, out double y, out double z)
        {
            double ka = pmRa / 3600.0 * D2R * years, kd = pmDec / 3600.0 * D2R * years;
            x = C(dec) * C(ra) - S(ra) * ka - S(dec) * C(ra) * kd;
            y = C(dec) * S(ra) + C(ra) * ka - S(dec) * S(ra) * kd;
            z = S(dec) + C(dec) * kd;
            double len = Math.Sqrt(x * x + y * y + z * z);
            x /= len;
            y /= len;
            z /= len;
        }

        // ------------------------------------------------------------------------------------------------ eclipses

        /// <summary>The share of the Sun's disc the Moon hides, seen from a place (0..1), from their topocentric places
        /// and distances.</summary>
        public static double SunCovered(double sunRa, double sunDec, double sunKm, double moonRa, double moonDec, double moonKm)
        {
            double cosSep = S(sunDec) * S(moonDec) + C(sunDec) * C(moonDec) * C(sunRa - moonRa);
            double d = Math.Acos(Math.Max(-1.0, Math.Min(1.0, cosSep)));
            double rs = Math.Asin(SunRadiusKm / sunKm), rm = Math.Asin(MoonRadiusKm / moonKm);
            if (d >= rs + rm)
                return 0.0;
            if (d <= Math.Abs(rm - rs))
                return rm >= rs ? 1.0 : rm * rm / (rs * rs);
            // the area two overlapping circles share, over the Sun's
            double a = Math.Acos((d * d + rs * rs - rm * rm) / (2 * d * rs)), b = Math.Acos((d * d + rm * rm - rs * rs) / (2 * d * rm));
            double area = rs * rs * (a - Math.Sin(2 * a) * 0.5) + rm * rm * (b - Math.Sin(2 * b) * 0.5);
            return area / (Math.PI * rs * rs);
        }
    }
}
