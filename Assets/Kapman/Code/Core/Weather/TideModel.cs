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
