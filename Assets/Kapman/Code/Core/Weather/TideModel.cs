using System;
using System.IO;
using UnityEngine;

namespace Kapman.Weather
{
    /// <summary>
    /// TIDE MODEL - predicts the ocean tide at any place on the map and any moment: EOT20's ten main constituents
    /// (Bake/tides writes them to tides.bin) summed with the Moon's and the Sun's positions for the date. Those come from
    /// the classic mean longitudes and Schureman's nodal factors, good over centuries, so the spring tides of 793 fall
    /// on the days they did.
    /// Heights are real metres about mean sea level. Read-only once loaded, so worker threads may share it.
    /// </summary>
    public sealed class TideModel
    {
        readonly string[] names;
        readonly float[][] re, im;                                          // per constituent, rows south to north
        readonly double west, south, step;
        readonly int nx, ny;

        public int Constituents => names.Length;

        TideModel(string[] names, float[][] re, float[][] im, double west, double south, double step, int nx, int ny)
        {
            this.names = names;
            this.re = re;
            this.im = im;
            this.west = west;
            this.south = south;
            this.step = step;
            this.nx = nx;
            this.ny = ny;
        }

        /// <summary>Reads tides.bin ("KTD1": constituents, grid, names, then per constituent float16 re and im grids).</summary>
        public static TideModel Load(string path)
        {
            using var reader = new BinaryReader(File.OpenRead(path));
            if (new string(reader.ReadChars(4)) != "KTD1")
                throw new InvalidDataException("not a Kapman tides file: " + path);
            int n = reader.ReadInt32();
            double west = reader.ReadDouble(), south = reader.ReadDouble(), step = reader.ReadDouble();
            int nx = reader.ReadInt32(), ny = reader.ReadInt32();
            var names = new string[n];
            for (int c = 0; c < n; c++)
                names[c] = new string(reader.ReadChars(8)).TrimEnd('\0');
            var re = new float[n][];
            var im = new float[n][];
            for (int c = 0; c < n; c++)
            {
                re[c] = Halves(reader, nx * ny);
                im[c] = Halves(reader, nx * ny);
            }
            return new TideModel(names, re, im, west, south, step, nx, ny);
        }

        static float[] Halves(BinaryReader reader, int count)
        {
            var bytes = reader.ReadBytes(count * 2);
            var values = new float[count];
            for (int i = 0; i < count; i++)
                values[i] = Mathf.HalfToFloat((ushort)(bytes[2 * i] | (bytes[2 * i + 1] << 8)));
            return values;
        }

        /// <summary>The tide (m about mean sea level) at a place, at a moment given as a Julian day.</summary>
        public float HeightAt(double lat, double lon, double julianDay)
        {
            var moment = At(julianDay);
            return HeightAt(lat, lon, moment);
        }

        /// <summary>The Moon and Sun for a moment, worked out once for any number of places.</summary>
        public Moment At(double julianDay)
        {
            var m = new Moment { cos = new double[names.Length], sin = new double[names.Length] };
            Astronomy(julianDay, out double T, out double s, out double h, out double p, out double N);
            for (int c = 0; c < names.Length; c++)
            {
                Argument(names[c], T, s, h, p, N, out double v, out double f);
                m.cos[c] = f * Math.Cos(v * Math.PI / 180.0);
                m.sin[c] = f * Math.Sin(v * Math.PI / 180.0);
            }
            return m;
        }

        public float HeightAt(double lat, double lon, Moment m)
        {
            double fx = (lon - west) / step, fy = (lat - south) / step;
            if (fx < 0 || fy < 0 || fx > nx - 1 || fy > ny - 1)
                return 0f;
            int x0 = Math.Min(nx - 2, (int)fx), y0 = Math.Min(ny - 2, (int)fy);
            double tx = fx - x0, ty = fy - y0;
            int i00 = y0 * nx + x0, i10 = i00 + 1, i01 = i00 + nx, i11 = i01 + 1;
            double sum = 0.0;
            for (int c = 0; c < names.Length; c++)
            {
                var r = re[c];
                var q = im[c];
                double a = (r[i00] * (1 - tx) + r[i10] * tx) * (1 - ty) + (r[i01] * (1 - tx) + r[i11] * tx) * ty;
                double b = (q[i00] * (1 - tx) + q[i10] * tx) * (1 - ty) + (q[i01] * (1 - tx) + q[i11] * tx) * ty;
                sum += a * m.cos[c] + b * m.sin[c];                          // f A cos(V + u - G)
            }
            return (float)sum;
        }

        /// <summary>
        /// THE STREAM OF THE TIDE (9 Oct 2026; the user: "an intelligent realistic current and weather system that can blow
        /// and push the ship off course"). The model holds the height of the tide, not its stream. The stream is
        /// reckoned from it as for a wave that travels: the crest of each constituent moves the way its high water
        /// comes later (the gradient of its phase, taken with no unwrapping from the two grids), and the water under
        /// it runs that way at the height times the square root of g over the depth, in step with the height: with
        /// the wave while the water stands above its mean, against it below. Where the tide stands and does not
        /// travel (the phase hardly changes from place to place: the head of a bay) the stream is let fall to nothing
        /// and not guessed at. So it runs hardest where the water is shallow and the range large, turns when the
        /// real tide turns, and is no survey: a narrows may run harder than this says.
        /// East and north, real metres a second. `depth`: real metres of water (kept between 4 and 400).
        /// </summary>
        public void StreamAt(double lat, double lon, Moment m, double depth, out double east, out double north)
        {
            east = north = 0.0;
            double fx = (lon - west) / step, fy = (lat - south) / step;
            if (fx < 0 || fy < 0 || fx > nx - 1 || fy > ny - 1 || m.cos == null)
                return;
            int x0 = Math.Min(nx - 2, (int)fx), y0 = Math.Min(ny - 2, (int)fy);
            double tx = fx - x0, ty = fy - y0;
            int i00 = y0 * nx + x0, i10 = i00 + 1, i01 = i00 + nx, i11 = i01 + 1;
            double h = Math.Max(4.0, Math.Min(400.0, depth));
            double celerity = Math.Sqrt(9.81 * h), perHeight = Math.Sqrt(9.81 / h);
            // real metres a cell, east and north
            double mx = step * 111320.0 * Math.Max(0.05, Math.Cos(lat * Math.PI / 180.0)), my = step * 110574.0;
            for (int c = 0; c < names.Length; c++)
            {
                double w = Speed(names[c]);
                if (w <= 0.0)
                    continue;
                var r = re[c];
                var q = im[c];
                double a00 = r[i00], a10 = r[i10], a01 = r[i01], a11 = r[i11];
                double b00 = q[i00], b10 = q[i10], b01 = q[i01], b11 = q[i11];
                // (a corner on the land has no tide: across the shore there is no gradient to take)
                if (Dry(a00, b00) || Dry(a10, b10) || Dry(a01, b01) || Dry(a11, b11))
                    continue;
                double a = (a00 * (1 - tx) + a10 * tx) * (1 - ty) + (a01 * (1 - tx) + a11 * tx) * ty;
                double b = (b00 * (1 - tx) + b10 * tx) * (1 - ty) + (b01 * (1 - tx) + b11 * tx) * ty;
                double amp2 = a * a + b * b;
                if (amp2 < 1e-6)
                    continue;
                double ax = ((a10 - a00) * (1 - ty) + (a11 - a01) * ty) / mx, ay = ((a01 - a00) * (1 - tx) + (a11 - a10) * tx) / my;
                double bx = ((b10 - b00) * (1 - ty) + (b11 - b01) * ty) / mx, by = ((b01 - b00) * (1 - tx) + (b11 - b10) * tx) / my;
                // the gradient of the phase, radians a metre: (a grad b - b grad a) / (a2 + b2)
                double gx = (a * bx - b * ax) / amp2, gy = (a * by - b * ay) / amp2;
                double g = Math.Sqrt(gx * gx + gy * gy);
                if (g < 1e-12)
                    continue;
                double travelling = Math.Min(1.0, g * celerity / w);         // (1: a wave that travels at the speed its depth gives)
                double height = a * m.cos[c] + b * m.sin[c];
                double run = height * perHeight * travelling;
                east += run * gx / g;
                north += run * gy / g;
            }
        }

        static bool Dry(double a, double b) => double.IsNaN(a) || double.IsNaN(b) || (a == 0.0 && b == 0.0);

        /// <summary>The speed of a constituent, radians a second (0: not one we know).</summary>
        static double Speed(string name)
        {
            switch (name)
            {
                case "M2": return 1.405189e-4;
                case "S2": return 1.454441e-4;
                case "N2": return 1.378797e-4;
                case "K2": return 1.458423e-4;
                case "2N2": return 1.352405e-4;
                case "K1": return 7.292117e-5;
                case "O1": return 6.759774e-5;
                case "P1": return 7.252295e-5;
                case "Q1": return 6.495854e-5;
                case "M4": return 2.810378e-4;
                default: return 0.0;
            }
        }

        /// <summary>A moment's astronomical arguments, per constituent: f cos(V + u) and f sin(V + u).</summary>
        public struct Moment
        {
            public double[] cos, sin;
        }

        /// <summary>
        /// The mean longitudes, degrees: T the hour angle of the mean Sun at Greenwich, s the Moon, h the Sun, p the
        /// Moon's perigee, N its ascending node.
        /// </summary>
        static void Astronomy(double jd, out double T, out double s, out double h, out double p, out double N)
        {
            double c = (jd - 2451545.0) / 36525.0;                           // Julian centuries from J2000
            s = 218.3164477 + 481267.88123421 * c;
            h = 280.46646 + 36000.76983 * c;
            p = 83.3532465 + 4069.0137287 * c;
            N = 125.04452 - 1934.136261 * c;
            double hours = ((jd + 0.5) - Math.Floor(jd + 0.5)) * 24.0;       // since midnight, Greenwich
            T = 15.0 * hours + 180.0;
        }

        /// <summary>A constituent's argument V + u (degrees) and nodal factor f, by Doodson's numbers and Schureman's formulae.</summary>
        static void Argument(string name, double T, double s, double h, double p, double N, out double v, out double f)
        {
            double n = N * Math.PI / 180.0;
            double fM2 = 1.0004 - 0.0373 * Math.Cos(n) + 0.0002 * Math.Cos(2 * n), uM2 = -2.14 * Math.Sin(n);
            double fK1 = 1.0060 + 0.1150 * Math.Cos(n) - 0.0088 * Math.Cos(2 * n) + 0.0006 * Math.Cos(3 * n);
            double uK1 = -8.86 * Math.Sin(n) + 0.68 * Math.Sin(2 * n) - 0.07 * Math.Sin(3 * n);
            double fO1 = 1.0089 + 0.1871 * Math.Cos(n) - 0.0147 * Math.Cos(2 * n) + 0.0014 * Math.Cos(3 * n);
            double uO1 = 10.80 * Math.Sin(n) - 1.34 * Math.Sin(2 * n) + 0.19 * Math.Sin(3 * n);
            double fK2 = 1.0241 + 0.2863 * Math.Cos(n) + 0.0083 * Math.Cos(2 * n) - 0.0015 * Math.Cos(3 * n);
            double uK2 = -17.74 * Math.Sin(n) + 0.68 * Math.Sin(2 * n) - 0.04 * Math.Sin(3 * n);
            switch (name)
            {
                case "M2": v = 2 * T + 2 * h - 2 * s + uM2; f = fM2; break;
                case "S2": v = 2 * T; f = 1.0; break;
                case "N2": v = 2 * T + 2 * h - 3 * s + p + uM2; f = fM2; break;
                case "K2": v = 2 * T + 2 * h + uK2; f = fK2; break;
                case "2N2": v = 2 * T + 2 * h - 4 * s + 2 * p + uM2; f = fM2; break;
                case "K1": v = T + h + 90.0 + uK1; f = fK1; break;
                case "O1": v = T + h - 2 * s - 90.0 + uO1; f = fO1; break;
                case "P1": v = T - h - 90.0; f = 1.0; break;
                case "Q1": v = T + h - 3 * s + p - 90.0 + uO1; f = fO1; break;
                case "M4": v = 4 * T + 4 * h - 4 * s + 2 * uM2; f = fM2 * fM2; break;
                default: v = 0.0; f = 0.0; break;                           // not one we know: left out
            }
        }
    }
}
