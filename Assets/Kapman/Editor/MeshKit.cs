using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kapman.EditorTools
{
    /// <summary>
    /// SIMPLE SHAPES FOR THE BLOCKOUT - boxes, gable roofs with the timber that holds them up, the bow-sided halls of
    /// the delta's farms, round barrels and posts, a hollowed trunk for a boat, a flame, with UVs in metres (divided by
    /// the texture's tile size) so a texture keeps its scale whatever the size. These are placeholders the user can
    /// replace with their own models: one prefab per kind.
    /// </summary>
    public static class MeshKit
    {
        class Builder
        {
            public readonly List<Vector3> v = new List<Vector3>();
            public readonly List<Vector3> n = new List<Vector3>();
            public readonly List<Vector2> uv = new List<Vector2>();
            public readonly List<List<int>> subs = new List<List<int>>();

            public List<int> Sub(int i)
            {
                while (subs.Count <= i)
                    subs.Add(new List<int>());
                return subs[i];
            }

            public void Quad(int sub, Vector3 a, Vector3 b, Vector3 c, Vector3 d, float tile)
            {
                // a-b-c-d counter-clockwise seen from the front; UVs along a->b and a->d in metres
                Vector3 nrm = Vector3.Cross(d - a, b - a).normalized;
                Vector3 ux = (b - a).normalized, uy = (d - a).normalized;
                int i0 = v.Count;
                foreach (var p in new[] { a, b, c, d })
                {
                    v.Add(p);
                    n.Add(nrm);
                    uv.Add(new Vector2(Vector3.Dot(p - a, ux), Vector3.Dot(p - a, uy)) / tile);
                }
                var t = Sub(sub);
                t.AddRange(new[] { i0, i0 + 2, i0 + 1, i0, i0 + 3, i0 + 2 });
            }

            public void Tri(int sub, Vector3 a, Vector3 b, Vector3 c, float tile)
            {
                // a-b-c counter-clockwise seen from the front
                Vector3 nrm = Vector3.Cross(c - a, b - a).normalized;
                Vector3 ux = (b - a).normalized, uy = Vector3.Cross(nrm, ux);
                int i0 = v.Count;
                foreach (var p in new[] { a, b, c })
                {
                    v.Add(p);
                    n.Add(nrm);
                    uv.Add(new Vector2(Vector3.Dot(p - a, ux), Vector3.Dot(p - a, uy)) / tile);
                }
                Sub(sub).AddRange(new[] { i0, i0 + 2, i0 + 1 });
            }

            public Mesh Build(string name)
            {
                var m = new Mesh { name = name };
                if (v.Count > 65000)
                    m.indexFormat = IndexFormat.UInt32;
                m.SetVertices(v);
                m.SetNormals(n);
                m.SetUVs(0, uv);
                m.subMeshCount = subs.Count;
                for (int i = 0; i < subs.Count; i++)
                    m.SetTriangles(subs[i], i);
                m.RecalculateBounds();
                m.RecalculateTangents();
                return m;
            }
        }

        /// <summary>A box standing on y = 0, centred on x and z.</summary>
        public static Mesh Box(string name, Vector3 size, float tile)
        {
            var b = new Builder();
            AddBox(b, 0, Vector3.zero, size, tile);
            return b.Build(name);
        }

        static void AddBox(Builder b, int sub, Vector3 centreBottom, Vector3 s, float tile)
        {
            float x = s.x / 2, z = s.z / 2, y0 = centreBottom.y, y1 = centreBottom.y + s.y;
            Vector3 c = new Vector3(centreBottom.x, 0, centreBottom.z);
            Vector3 P(float px, float py, float pz) => c + new Vector3(px, py, pz);
            b.Quad(sub, P(-x, y0, -z), P(x, y0, -z), P(x, y1, -z), P(-x, y1, -z), tile);   // south
            b.Quad(sub, P(x, y0, z), P(-x, y0, z), P(-x, y1, z), P(x, y1, z), tile);       // north
            b.Quad(sub, P(x, y0, -z), P(x, y0, z), P(x, y1, z), P(x, y1, -z), tile);       // east
            b.Quad(sub, P(-x, y0, z), P(-x, y0, -z), P(-x, y1, -z), P(-x, y1, z), tile);   // west
            b.Quad(sub, P(-x, y1, -z), P(x, y1, -z), P(x, y1, z), P(-x, y1, z), tile);     // top
            b.Quad(sub, P(-x, y0, z), P(x, y0, z), P(x, y0, -z), P(-x, y0, -z), tile);     // bottom
        }

        /// <summary>
        /// A box on y = 0 whose top slopes down toward +z by `endDrop`: a dam's head tapering into the shoal.
        /// Submesh 0 the sides and ends, 1 the sloping top.
        /// </summary>
        public static Mesh Wedge(string name, Vector3 size, float endDrop, float tile)
        {
            var b = new Builder();
            float x = size.x / 2, z = size.z / 2, y0 = 0f, y1 = size.y, y2 = Mathf.Max(0.05f, size.y - endDrop);
            Vector3 P(float px, float py, float pz) => new Vector3(px, py, pz);
            b.Quad(0, P(-x, y0, -z), P(x, y0, -z), P(x, y1, -z), P(-x, y1, -z), tile);   // the root end, full height
            b.Quad(0, P(x, y0, z), P(-x, y0, z), P(-x, y2, z), P(x, y2, z), tile);       // the head end, lower
            b.Quad(0, P(x, y0, -z), P(x, y0, z), P(x, y2, z), P(x, y1, -z), tile);       // east
            b.Quad(0, P(-x, y0, z), P(-x, y0, -z), P(-x, y1, -z), P(-x, y2, z), tile);   // west
            b.Quad(1, P(-x, y1, -z), P(x, y1, -z), P(x, y2, z), P(-x, y2, z), tile);     // the top, sloping down to the head
            b.Quad(0, P(-x, y0, z), P(x, y0, z), P(x, y0, -z), P(-x, y0, -z), tile);     // bottom
            return b.Build(name);
        }

        /// <summary>A flat triangle standing on y = 0 in the x-y plane, seen from both sides: a tongue of flame.</summary>
        public static Mesh Flame(string name, float width, float height, float tile)
        {
            var b = new Builder();
            Vector3 l = new Vector3(-width / 2, 0, 0), r = new Vector3(width / 2, 0, 0), top = new Vector3(width * 0.12f, height, 0);
            b.Tri(0, l, r, top, tile);
            b.Tri(0, r, l, top, tile);
            return b.Build(name);
        }

        /// <summary>
        /// Where a door can sit in a gable without a wall post standing in it: the middle of one of the gaps between
        /// the gable's posts (the same spacing House() uses), chosen at random.
        /// </summary>
        public static float DoorSlot(float width, float postPitch, System.Random rnd)
        {
            if (postPitch <= 0.1f)
                return (float)(rnd.NextDouble() - 0.5) * (width - 2.4f);
            int nx = Mathf.Max(1, Mathf.RoundToInt((width - 0.4f) / postPitch));
            int j = rnd.Next(nx);
            return -width / 2 + 0.2f + (width - 0.4f) * (j + 0.5f) / nx;
        }

        /// <summary>
        /// A house: walls (submesh 0) from `below` under the ground to the eaves, and a gable roof (submesh 1, thatch)
        /// with gable ends (submesh 0). Long along z. Origin at the ground in the middle. Options, all off by default:
        ///   postPitch: the paired wall posts of the excavated houses, standing 9 cm proud, with a wall plate at the eaves
        ///     and a sill beam at the ground (submesh 2, timber);
        ///   hipDepth: a half-hip, the ridge's ends pulled in by this much with a small thatch triangle at each end;
        ///   eaveThickness: the thatch's cut edge at the eaves, so the roof has thickness seen from below;
        ///   gables false: open ends (a tent).
        /// </summary>
        public static Mesh House(string name, float width, float length, float wallHeight, float ridgeHeight, float overhang,
                                 float wallTile, float roofTile, float postPitch = 0f, float hipDepth = 0f, float below = 0.8f,
                                 float eaveThickness = 0f, bool gables = true)
        {
            var b = new Builder();
            AddBox(b, 0, new Vector3(0, -below, 0), new Vector3(width, wallHeight + below, length), wallTile);
            float x = width / 2 + overhang, z = length / 2 + overhang * 0.6f, ye = wallHeight - overhang * 0.5f, yr = ridgeHeight;
            bool hip = hipDepth > 0.01f;
            float zr = hip ? Mathf.Max(0.5f, z - hipDepth) : z;      // the ridge's ends
            float yh = hip ? ye + (yr - ye) * 0.5f : ye, xh = hip ? x * 0.5f : x;   // where the half-hip cuts the slopes
            if (!hip)
            {
                // the two slopes
                b.Quad(1, new Vector3(x, ye, -z), new Vector3(x, ye, z), new Vector3(0, yr, z), new Vector3(0, yr, -z), roofTile);
                b.Quad(1, new Vector3(-x, ye, z), new Vector3(-x, ye, -z), new Vector3(0, yr, -z), new Vector3(0, yr, z), roofTile);
                // their undersides
                b.Quad(1, new Vector3(x, ye, z), new Vector3(x, ye, -z), new Vector3(0, yr, -z), new Vector3(0, yr, z), roofTile);
                b.Quad(1, new Vector3(-x, ye, -z), new Vector3(-x, ye, z), new Vector3(0, yr, z), new Vector3(0, yr, -z), roofTile);
            }
            else
            {
                // each slope: the lower band the full length, the upper part between the ridge's ends, a hip at each end
                b.Quad(1, new Vector3(x, ye, -z), new Vector3(x, ye, z), new Vector3(xh, yh, z), new Vector3(xh, yh, -z), roofTile);
                b.Quad(1, new Vector3(xh, yh, -z), new Vector3(xh, yh, z), new Vector3(0, yr, zr), new Vector3(0, yr, -zr), roofTile);
                b.Quad(1, new Vector3(-x, ye, z), new Vector3(-x, ye, -z), new Vector3(-xh, yh, -z), new Vector3(-xh, yh, z), roofTile);
                b.Quad(1, new Vector3(-xh, yh, z), new Vector3(-xh, yh, -z), new Vector3(0, yr, -zr), new Vector3(0, yr, zr), roofTile);
                b.Tri(1, new Vector3(xh, yh, z), new Vector3(-xh, yh, z), new Vector3(0, yr, zr), roofTile);
                b.Tri(1, new Vector3(-xh, yh, -z), new Vector3(xh, yh, -z), new Vector3(0, yr, -zr), roofTile);
                // the undersides
                b.Quad(1, new Vector3(x, ye, z), new Vector3(x, ye, -z), new Vector3(xh, yh, -z), new Vector3(xh, yh, z), roofTile);
                b.Quad(1, new Vector3(xh, yh, z), new Vector3(xh, yh, -z), new Vector3(0, yr, -zr), new Vector3(0, yr, zr), roofTile);
                b.Quad(1, new Vector3(-x, ye, -z), new Vector3(-x, ye, z), new Vector3(-xh, yh, z), new Vector3(-xh, yh, -z), roofTile);
                b.Quad(1, new Vector3(-xh, yh, -z), new Vector3(-xh, yh, z), new Vector3(0, yr, zr), new Vector3(0, yr, -zr), roofTile);
                b.Tri(1, new Vector3(-xh, yh, z), new Vector3(xh, yh, z), new Vector3(0, yr, zr), roofTile);
                b.Tri(1, new Vector3(xh, yh, -z), new Vector3(-xh, yh, -z), new Vector3(0, yr, -zr), roofTile);
            }
            // the thatch's thickness at the eaves: its cut edge, and a strip of underside below the lower 0.6 m
            if (eaveThickness > 0.001f)
            {
                float run = Mathf.Min(0.6f, Mathf.Sqrt(x * x + (yr - ye) * (yr - ye)) * 0.5f);
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 d = new Vector3(-s * x, yr - ye, 0).normalized;      // up the slope
                    Vector3 n = new Vector3(s * (yr - ye), x, 0).normalized;     // out of the slope
                    Vector3 P = new Vector3(s * x, ye, s * z), Q = new Vector3(s * x, ye, -s * z);
                    Vector3 off = n * eaveThickness;
                    b.Quad(1, P, Q, Q - off, P - off, roofTile);
                    b.Quad(1, Q + d * run - off, P + d * run - off, P - off, Q - off, roofTile);
                }
            }
            // the gables (wall material), front and back
            float zw = length / 2;
            if (gables && !hip)
            {
                b.Tri(0, new Vector3(-width / 2, wallHeight, -zw), new Vector3(width / 2, wallHeight, -zw), new Vector3(0, yr - 0.2f, -zw), wallTile);
                b.Tri(0, new Vector3(width / 2, wallHeight, zw), new Vector3(-width / 2, wallHeight, zw), new Vector3(0, yr - 0.2f, zw), wallTile);
            }
            else if (gables)
            {
                float yt = yh + (yr - yh) * 0.3f;
                float xg = width / 2 * Mathf.Clamp01((yr - 0.2f - yt) / Mathf.Max(0.01f, yr - 0.2f - wallHeight));
                b.Quad(0, new Vector3(-width / 2, wallHeight, -zw), new Vector3(width / 2, wallHeight, -zw), new Vector3(xg, yt, -zw), new Vector3(-xg, yt, -zw), wallTile);
                b.Quad(0, new Vector3(width / 2, wallHeight, zw), new Vector3(-width / 2, wallHeight, zw), new Vector3(-xg, yt, zw), new Vector3(xg, yt, zw), wallTile);
            }
            // the timber: paired wall posts proud of the wall, a wall plate at the eaves, sill beams at the ground
            if (postPitch > 0.1f)
            {
                float ph = wallHeight + 0.3f;
                int nz = Mathf.Max(1, Mathf.RoundToInt((length - 0.4f) / postPitch));
                for (int i = 0; i <= nz; i++)
                {
                    float pz = -length / 2 + 0.2f + (length - 0.4f) * i / nz;
                    AddBox(b, 2, new Vector3(width / 2 + 0.01f, -0.2f, pz), new Vector3(0.16f, ph, 0.16f), 1f);
                    AddBox(b, 2, new Vector3(-width / 2 - 0.01f, -0.2f, pz), new Vector3(0.16f, ph, 0.16f), 1f);
                }
                int nx = Mathf.Max(1, Mathf.RoundToInt((width - 0.4f) / postPitch));
                for (int i = 1; i < nx; i++)
                {
                    float px = -width / 2 + 0.2f + (width - 0.4f) * i / nx;
                    AddBox(b, 2, new Vector3(px, -0.2f, length / 2 + 0.01f), new Vector3(0.16f, ph, 0.16f), 1f);
                    AddBox(b, 2, new Vector3(px, -0.2f, -length / 2 - 0.01f), new Vector3(0.16f, ph, 0.16f), 1f);
                }
                AddBox(b, 2, new Vector3(width / 2 + 0.02f, wallHeight - 0.2f, 0), new Vector3(0.2f, 0.2f, length + 0.2f), 1f);
                AddBox(b, 2, new Vector3(-width / 2 - 0.02f, wallHeight - 0.2f, 0), new Vector3(0.2f, 0.2f, length + 0.2f), 1f);
                AddBox(b, 2, new Vector3(width / 2 + 0.02f, -0.02f, 0), new Vector3(0.24f, 0.28f, length + 0.24f), 1f);
                AddBox(b, 2, new Vector3(-width / 2 - 0.02f, -0.02f, 0), new Vector3(0.24f, 0.28f, length + 0.24f), 1f);
                AddBox(b, 2, new Vector3(0, -0.02f, length / 2 + 0.02f), new Vector3(width + 0.24f, 0.28f, 0.24f), 1f);
                AddBox(b, 2, new Vector3(0, -0.02f, -length / 2 - 0.02f), new Vector3(width + 0.24f, 0.28f, 0.24f), 1f);
            }
            return b.Build(name);
        }

        /// <summary>
        /// A bow-sided hall, the delta's farmhouse: long walls bowed out (widest in the middle; straight when endWidth
        /// equals midWidth), a hipped thatch roof following them down low. Submesh 0 walls, 1 thatch; with `plinth` a
        /// band of trodden mud at the foot (submesh 2) and with `postPitch` the wall posts (submesh 3). Long along z.
        /// </summary>
        public static Mesh BowedHall(string name, float endWidth, float midWidth, float length, float wallHeight, float ridgeHeight,
                                     float overhang, float wallTile, float roofTile, float plinth = 0f, float postPitch = 0f)
        {
            var b = new Builder();
            const int N = 12;
            var east = new Vector3[N + 1];
            var west = new Vector3[N + 1];
            for (int i = 0; i <= N; i++)
            {
                float t = i / (float)N;
                float z = (t - 0.5f) * length;
                float w = Mathf.Lerp(endWidth, midWidth, Mathf.Sin(t * Mathf.PI)) / 2;
                east[i] = new Vector3(w, 0, z);
                west[i] = new Vector3(-w, 0, z);
            }
            Vector3 up = Vector3.up * wallHeight, down = Vector3.down * 0.8f;
            for (int i = 0; i < N; i++)
            {
                b.Quad(0, east[i] + down, east[i + 1] + down, east[i + 1] + up, east[i] + up, wallTile);
                b.Quad(0, west[i + 1] + down, west[i] + down, west[i] + up, west[i + 1] + up, wallTile);
            }
            b.Quad(0, west[0] + down, east[0] + down, east[0] + up, west[0] + up, wallTile);
            b.Quad(0, east[N] + down, west[N] + down, west[N] + up, east[N] + up, wallTile);
            // the splash plinth: the same outline 8 cm out, from under the ground to `plinth`
            if (plinth > 0.01f)
            {
                Vector3 pd = Vector3.down * 0.35f, pu = Vector3.up * plinth;
                Vector3 Out(Vector3 p, float dz) => new Vector3(p.x + Mathf.Sign(p.x) * 0.08f, 0, p.z + dz);
                for (int i = 0; i < N; i++)
                {
                    b.Quad(2, Out(east[i], 0) + pd, Out(east[i + 1], 0) + pd, Out(east[i + 1], 0) + pu, Out(east[i], 0) + pu, wallTile);
                    b.Quad(2, Out(west[i + 1], 0) + pd, Out(west[i], 0) + pd, Out(west[i], 0) + pu, Out(west[i + 1], 0) + pu, wallTile);
                }
                b.Quad(2, Out(west[0], -0.08f) + pd, Out(east[0], -0.08f) + pd, Out(east[0], -0.08f) + pu, Out(west[0], -0.08f) + pu, wallTile);
                b.Quad(2, Out(east[N], 0.08f) + pd, Out(west[N], 0.08f) + pd, Out(west[N], 0.08f) + pu, Out(east[N], 0.08f) + pu, wallTile);
                // the tops of the band
                for (int i = 0; i < N; i++)
                {
                    b.Quad(2, east[i] + pu, east[i + 1] + pu, Out(east[i + 1], 0) + pu, Out(east[i], 0) + pu, wallTile);
                    b.Quad(2, Out(west[i], 0) + pu, Out(west[i + 1], 0) + pu, west[i + 1] + pu, west[i] + pu, wallTile);
                }
            }
            // the wall posts, following the bow
            if (postPitch > 0.1f)
            {
                int count = Mathf.Max(2, Mathf.RoundToInt(length / postPitch));
                for (int i = 0; i <= count; i++)
                {
                    float t = i / (float)count;
                    float z = (t - 0.5f) * (length - 0.3f);
                    float w = Mathf.Lerp(endWidth, midWidth, Mathf.Sin(t * Mathf.PI)) / 2;
                    AddBox(b, 3, new Vector3(w + 0.01f, -0.2f, z), new Vector3(0.18f, wallHeight + 0.3f, 0.18f), 1f);
                    AddBox(b, 3, new Vector3(-w - 0.01f, -0.2f, z), new Vector3(0.18f, wallHeight + 0.3f, 0.18f), 1f);
                }
            }
            // the hipped roof: the eaves (out and down from the wall tops) up to a ridge along the middle
            float ye = wallHeight - overhang * 0.6f, r0 = -length / 2 + length * 0.2f, r1 = length / 2 - length * 0.2f;
            Vector3 OutE(Vector3 p) => new Vector3(p.x + Mathf.Sign(p.x) * overhang, ye, p.z);
            Vector3 Ridge(float z) => new Vector3(0, ridgeHeight, Mathf.Clamp(z, r0, r1));
            for (int i = 0; i < N; i++)
            {
                Vector3 e0 = OutE(east[i]), e1 = OutE(east[i + 1]), w0 = OutE(west[i]), w1 = OutE(west[i + 1]);
                Vector3 re0 = Ridge(east[i].z), re1 = Ridge(east[i + 1].z);
                AddSlope(b, e0, e1, re1, re0, roofTile);
                AddSlope(b, w1, w0, re0, re1, roofTile);
            }
            // the hips at the ends
            Vector3 ends0e = OutE(east[0]) + new Vector3(0, 0, -overhang), ends0w = OutE(west[0]) + new Vector3(0, 0, -overhang);
            Vector3 endsNe = OutE(east[N]) + new Vector3(0, 0, overhang), endsNw = OutE(west[N]) + new Vector3(0, 0, overhang);
            AddTri2(b, ends0w, ends0e, Ridge(r0), roofTile);
            AddTri2(b, endsNe, endsNw, Ridge(r1), roofTile);
            return b.Build(name);
        }

        static void AddSlope(Builder b, Vector3 a, Vector3 c, Vector3 d, Vector3 e, float tile)
        {
            b.Quad(1, a, c, d, e, tile);
            b.Quad(1, c, a, e, d, tile);
        }

        static void AddTri2(Builder b, Vector3 a, Vector3 c, Vector3 d, float tile)
        {
            b.Tri(1, a, c, d, tile);
            b.Tri(1, c, a, d, tile);
        }

        /// <summary>
        /// A barrel, a post or a stack: an upright cylinder on y = 0 (submesh 0 sides, 1 the caps). `bulge` swells the
        /// middle like a cask; `topRadius` (0 or more) tapers it toward the top (a haystack, a cone); `bothCaps` closes
        /// the bottom too, for one that will lie on its side.
        /// </summary>
        public static Mesh Cylinder(string name, float radius, float height, int sides, float tile, bool bulge = false, float topRadius = -1f, bool bothCaps = false)
        {
            var b = new Builder();
            const int Rings = 4;
            float RingR(int r)
            {
                float t = r / (float)Rings;
                float rr = topRadius >= 0f ? Mathf.Lerp(radius, topRadius, t) : radius;
                return rr * (bulge ? 1f + 0.12f * Mathf.Sin(Mathf.PI * t) : 1f);
            }
            for (int s = 0; s < sides; s++)
            {
                float a0 = s * Mathf.PI * 2 / sides, a1 = (s + 1) * Mathf.PI * 2 / sides;
                for (int r = 0; r < Rings; r++)
                {
                    float h0 = height * r / Rings, h1 = height * (r + 1) / Rings;
                    float k0 = RingR(r), k1 = RingR(r + 1);
                    Vector3 p00 = new Vector3(Mathf.Cos(a0) * k0, h0, Mathf.Sin(a0) * k0);
                    Vector3 p10 = new Vector3(Mathf.Cos(a1) * k0, h0, Mathf.Sin(a1) * k0);
                    Vector3 p11 = new Vector3(Mathf.Cos(a1) * k1, h1, Mathf.Sin(a1) * k1);
                    Vector3 p01 = new Vector3(Mathf.Cos(a0) * k1, h1, Mathf.Sin(a0) * k1);
                    b.Quad(0, p00, p10, p11, p01, tile);
                }
                float rt = RingR(Rings), rb = RingR(0);
                if (rt > 0.005f)
                {
                    Vector3 c = new Vector3(0, height, 0);
                    b.Tri(1, c, new Vector3(Mathf.Cos(a0) * rt, height, Mathf.Sin(a0) * rt), new Vector3(Mathf.Cos(a1) * rt, height, Mathf.Sin(a1) * rt), tile);
                }
                if (bothCaps && rb > 0.005f)
                    b.Tri(1, Vector3.zero, new Vector3(Mathf.Cos(a1) * rb, 0, Mathf.Sin(a1) * rb), new Vector3(Mathf.Cos(a0) * rb, 0, Mathf.Sin(a0) * rb), tile);
            }
            return b.Build(name);
        }

        /// <summary>
        /// A logboat: a hollowed trunk as an open half-shell, pointed at the bow (+z), seen from both sides so the hollow
        /// shows. Submeshes 0 and 1 are the same shell (two materials map onto it). Origin at the keel's bottom.
        /// </summary>
        public static Mesh Logboat(string name, float length, float beam)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            const int rings = 10, sides = 8;
            for (int r = 0; r <= rings; r++)
            {
                float t = r / (float)rings;
                float z = (t - 0.5f) * length;
                float taper = 1f - Mathf.Pow(Mathf.Abs(t - 0.45f) / 0.55f, 2.2f);
                float half = beam * 0.5f * Mathf.Max(0.12f, taper);
                for (int s = 0; s <= sides; s++)
                {
                    float a = Mathf.PI + Mathf.PI * s / sides;      // the lower half of a circle, gunwale to gunwale
                    verts.Add(new Vector3(Mathf.Cos(a) * half, Mathf.Sin(a) * half * 0.6f + half * 0.6f, z));
                }
            }
            for (int r = 0; r < rings; r++)
                for (int s = 0; s < sides; s++)
                {
                    int a = r * (sides + 1) + s, b = a + 1, c = a + sides + 1, d = c + 1;
                    tris.AddRange(new[] { a, b, c, b, d, c, a, c, b, b, c, d });   // both sides
                }
            var m = new Mesh { name = name };
            m.SetVertices(verts);
            m.subMeshCount = 2;
            m.SetTriangles(tris, 0);
            m.SetTriangles(tris, 1);
            m.RecalculateNormals();
            m.RecalculateBounds();
            var uv = new Vector2[verts.Count];
            for (int i = 0; i < uv.Length; i++)
                uv[i] = new Vector2(verts[i].z / 2f, (verts[i].x + verts[i].y) / 1.2f);
            m.uv = uv;
            m.RecalculateTangents();
            return m;
        }
    }
}
