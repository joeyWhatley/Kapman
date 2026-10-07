using UnityEngine;

namespace Kapman.Ships
{
    /// <summary>What the bottom is (ship physics 2.9): how it grips a keel and how it holds an anchor.</summary>
    public enum GroundKind { Mud, Sand, Gravel, Weed, Rock }

    /// <summary>
    /// THE GROUND'S NUMBERS (ship physics 2.9; all guesses there): the keel's friction on it (mud 0.3, sand 0.5, gravel
    /// 0.7, rock 0.9 of the load; weed 0.4, a guess between mud and sand), and what it holds of an anchor's weight: a
    /// fluked iron anchor digs in (sand 10 times its weight, mud 8, gravel 6, weed 2, rock 1); a stone only lies there
    /// (sand 3, mud 2, anything else 1). Gravel and rock crack planks; mud, sand and weed do not.
    /// </summary>
    public static class Grounds
    {
        public static float Friction(GroundKind k)
        {
            switch (k)
            {
                case GroundKind.Mud: return 0.3f;
                case GroundKind.Sand: return 0.5f;
                case GroundKind.Gravel: return 0.7f;
                case GroundKind.Weed: return 0.4f;
                default: return 0.9f;
            }
        }

        /// <summary>What the bottom holds, as a multiple of the anchor's weight.</summary>
        public static float HoldingFactor(GroundKind k, bool stone)
        {
            if (stone)
                return k == GroundKind.Sand ? 3f : k == GroundKind.Mud ? 2f : 1f;
            switch (k)
            {
                case GroundKind.Sand: return 10f;
                case GroundKind.Mud: return 8f;
                case GroundKind.Gravel: return 6f;
                case GroundKind.Weed: return 2f;
                default: return 1f;
            }
        }

        /// <summary>The pull (N) an anchor of this weight and kind holds on this bottom at a good scope (3:1 or more).</summary>
        public static float Holding(GroundKind k, bool stone, float anchorKg) => HoldingFactor(k, stone) * anchorKg * Hydrostatics.G;

        /// <summary>Hard ground: a hull that meets it fast enough cracks a plank.</summary>
        public static bool Hard(GroundKind k) => k == GroundKind.Gravel || k == GroundKind.Rock;
    }

    /// <summary>
    /// THE BOTTOM UNDER THE WATER (ship physics 2.9): its height under a point, its upward normal and what it is. In the
    /// scene it is a ray from above that skips her own hull (SceneBed); in the tests a flat or a sloping bed.
    /// </summary>
    public interface IBedField
    {
        /// <summary>The bottom under a point: false where there is none within reach (nothing under the point).</summary>
        bool Bed(Vector3 p, out float height, out Vector3 normal, out GroundKind kind);
    }

    /// <summary>A level bottom at one height (the tests' harbour floor).</summary>
    public sealed class FlatBed : IBedField
    {
        public float height;
        public GroundKind kind;

        public FlatBed(float height, GroundKind kind = GroundKind.Sand)
        {
            this.height = height;
            this.kind = kind;
        }

        public bool Bed(Vector3 p, out float h, out Vector3 normal, out GroundKind k)
        {
            h = height;
            normal = Vector3.up;
            k = kind;
            return true;
        }
    }

    /// <summary>A plane bottom (the tests' shelving beach): its height at the world's origin, rising this much per metre
    /// going towards a bearing (degrees from north): 0.02 is a 1:50 slope.</summary>
    public sealed class SlopingBed : IBedField
    {
        public float height, rise, towards;
        public GroundKind kind;

        public SlopingBed(float height, float rise, float towards, GroundKind kind = GroundKind.Sand)
        {
            this.height = height;
            this.rise = rise;
            this.towards = towards;
            this.kind = kind;
        }

        public bool Bed(Vector3 p, out float h, out Vector3 normal, out GroundKind k)
        {
            float r = towards * Mathf.Deg2Rad;
            float dx = Mathf.Sin(r), dz = Mathf.Cos(r);
            h = height + rise * (dx * p.x + dz * p.z);
            normal = new Vector3(-rise * dx, 1f, -rise * dz).normalized;
            k = kind;
            return true;
        }
    }

    /// <summary>
    /// HER KEEL ON THE BOTTOM (ship physics 2.9). She touches at contact points of her own: the keel under each centre
    /// cell, and the turn of the bilge under each side cell (where the round section meets the side cell's line, half a
    /// draught up for a round bilge, on the keel line for a flat-bottomed hull), so a keeled boat that dries out on her
    /// keel falls over onto a bilge and lies there heeled, as boats do at low water. Each point pressed into the bottom
    /// feels a stiff spring and damper along the bottom's normal (she settles about 3 cm into it carrying her whole
    /// weight on three points) and friction across it by the ground (Grounds.Friction), smoothed below 0.1 m/s so she
    /// creeps rather than chatters, and capped so one step can never throw her backwards. She stops on soft ground; on
    /// gravel or rock, when she meets the bottom faster than the class's plank limit (the hardest blow of that moment,
    /// once a touch) a plank cracks (a leak of 0.1 m3/h; at twice the limit a plank is stove in, 1 m3/h: the design's
    /// 0.05-0.2 and 1 m3/h). A bottom more than 0.35 m
    /// above a point is a step (a quay's edge, a jetty, a wall), not a slope: it stops her going on (a stiff drag on that
    /// point's way across it) without lifting her onto it; the colliders' own contacts are the later, proper answer.
    /// She comes off when the water rises under her (the load falls, so the friction does), when the crew row or sail
    /// her off with more than the friction, or when they go over the side and push (PushOff: the men's weight out of her
    /// and 300 N a man towards deeper water, a guess). The bottom is looked at again every 5 steps or 0.2 m (every 25
    /// steps when it is more than 2 m below her keel): it does not move, she does. While she has less than two draughts
    /// of water under her keel the shallow-water factor rises on her resistance (design 2.4: 1 + 0.5 (2T/h - 1)^2).
    /// </summary>
    public sealed class Grounding
    {
        /// <summary>How far she settles into the bottom with her whole weight on three points (m): the contacts' stiffness.</summary>
        public const float SettleDepth = 0.03f;
        /// <summary>A bottom this far above a contact point is a step, not a slope (m).</summary>
        public const float StepHeight = 0.35f;
        /// <summary>Below this speed across the bottom (m/s) the friction acts as a stiff drag.</summary>
        public const float SlipSpeed = 0.1f;
        /// <summary>A man over the side pushes this hard (N) (guess).</summary>
        public const float ManPush = 300f;
        /// <summary>The leak a cracked plank and a stove-in plank let in (m3/h).</summary>
        public const float CrackLeak = 0.1f, StoveLeak = 1f;

        const float ResampleMove = 0.2f, Far = 2f;
        /// <summary>A point against a step is held by a drag of this many times her mass a second: she stops there in a
        /// step or two (and creeps a few millimetres a second if the oars keep pulling).</summary>
        const float WallGrip = 25f;
        /// <summary>She must have been clear of the bottom this long (s) for a touch to be a new one (a bump as she runs up a
        /// bank is the same touch).</summary>
        const float ClearSeconds = 2f;
        const int ResampleSteps = 5, FarResampleSteps = 25;

        readonly ShipClass cls;
        readonly Vector3[] points;
        readonly float[] bedY, fn, cv;
        readonly Vector3[] bedN, vt;
        readonly GroundKind[] bedKind;
        readonly bool[] found, touching, wall;
        readonly float stiffness, damping, weight;
        Vector3 sampledAt;
        int sinceSample = int.MaxValue / 2;
        bool far;
        float pushLeft, clearFor = float.PositiveInfinity, fastFor, freeFor;

        /// <summary>After each Compute: a point of her is on the bottom; she is fast on it (touching, carrying a share of her
        /// weight and all but stopped); the bottom's push on her (N).</summary>
        public bool Touching { get; private set; }
        public bool Aground { get; private set; }
        public float Load { get; private set; }
        /// <summary>The water between her lowest contact point and the bottom (m; below 0 pressed into it; +infinity where no
        /// bottom was found), and the same under her keel amidships and at her stern.</summary>
        public float LeastClearance { get; private set; } = float.PositiveInfinity;
        public float CentreClearance { get; private set; } = float.PositiveInfinity;
        public float SternClearance { get; private set; } = float.PositiveInfinity;
        /// <summary>What she is on (or the bottom under her middle when she is afloat).</summary>
        public GroundKind Kind { get; private set; }
        /// <summary>A point of her is against a step (a quay, a jetty, a wall).</summary>
        public bool AgainstStep { get; private set; }
        /// <summary>Counts since she was made: times she has touched, planks cracked and stove in, and the leak they let in
        /// (m3/h).</summary>
        public int Touches { get; private set; }
        public int PlanksCracked { get; private set; }
        public int PlanksStove { get; private set; }
        public float LeakAdded { get; private set; }
        /// <summary>The crew are over the side pushing her off.</summary>
        public bool PushingOff => pushLeft > 0f;
        /// <summary>The shallow-water factor on her residuary resistance (1 in deep water).</summary>
        public float Shallow { get; private set; } = 1f;

        public Grounding(ShipClass cls, HullProbes hull, float mass)
        {
            this.cls = cls;
            int n = hull.cells.Length;
            points = new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                var c = hull.cells[i];
                float y = hull.keelY;
                if (!c.centre && c.q > 1.0001f)
                    y += c.draught * Mathf.Pow(2f * HullProbes.SideOut, 1f / (c.q - 1f));   // where the round section reaches the cell's line
                points[i] = new Vector3(c.keel.x, y, c.keel.z);
            }
            bedY = new float[n];
            fn = new float[n];
            cv = new float[n];
            bedN = new Vector3[n];
            vt = new Vector3[n];
            bedKind = new GroundKind[n];
            found = new bool[n];
            touching = new bool[n];
            wall = new bool[n];
            weight = mass * Hydrostatics.G;
            stiffness = weight / (3f * SettleDepth);
            damping = 2f * 0.5f * Mathf.Sqrt(stiffness * mass / 3f);
        }

        /// <summary>Her contact points in the model's frame (for the gizmos).</summary>
        public Vector3 Point(int i) => points[i];
        public int Points => points.Length;
        public bool PointTouching(int i) => touching[i];

        /// <summary>Forgets the bottom she was on (after she is put somewhere new).</summary>
        public void Forget()
        {
            sinceSample = int.MaxValue / 2;
            for (int i = 0; i < touching.Length; i++)
            {
                touching[i] = false;
                found[i] = false;
            }
            pushLeft = fastFor = freeFor = 0f;
            clearFor = float.PositiveInfinity;
            Touching = Aground = AgainstStep = false;
            Load = 0f;
            Shallow = 1f;
            LeastClearance = CentreClearance = SternClearance = float.PositiveInfinity;
        }

        /// <summary>The crew go over the side and push her towards deeper water for up to this long (game seconds), or until
        /// she floats.</summary>
        public void PushOff(float seconds = 60f) => pushLeft = Mathf.Max(pushLeft, seconds);
        public void StopPushing() => pushLeft = 0f;

        /// <summary>The bottom's force on her (world, through her centre of gravity) and its torque, added in; and the push
        /// of the crew over the side when they are pushing.</summary>
        public void Compute(in ShipState s, in MassProps m, Vector3 massDiagonal, IBedField bed, float dt, ref Vector3 force, ref Vector3 torque)
        {
            Touching = AgainstStep = false;
            Load = 0f;
            if (bed == null)
            {
                Aground = false;
                Shallow = 1f;
                LeastClearance = CentreClearance = SternClearance = float.PositiveInfinity;
                return;
            }
            sinceSample++;
            bool look = sinceSample >= (far ? FarResampleSteps : ResampleSteps) || (s.position - sampledAt).sqrMagnitude > ResampleMove * ResampleMove;
            if (look)
            {
                sinceSample = 0;
                sampledAt = s.position;
            }

            float least = float.PositiveInfinity, centre = float.PositiveInfinity, stern = float.PositiveInfinity;
            float centreZ = float.PositiveInfinity, sternZ = float.PositiveInfinity;
            float cvSum = 0f, hardest = 0f;
            bool wasTouching = false;
            for (int i = 0; i < points.Length; i++)
                wasTouching |= touching[i];
            Vector3 downhill = Vector3.zero;
            int n = points.Length;
            for (int i = 0; i < n; i++)
            {
                fn[i] = cv[i] = 0f;
                wall[i] = false;
                Vector3 wp = s.position + s.rotation * (points[i] - m.cg);
                if (look)
                    found[i] = bed.Bed(wp, out bedY[i], out bedN[i], out bedKind[i]);
                if (!found[i])
                {
                    touching[i] = false;
                    continue;
                }
                float clear = wp.y - bedY[i];
                least = Mathf.Min(least, clear);
                float az = Mathf.Abs(points[i].z - m.cg.z);
                if (points[i].x == 0f && az < centreZ)
                {
                    centreZ = az;
                    centre = clear;
                    Kind = bedKind[i];
                }
                if (points[i].x == 0f && points[i].z < sternZ)
                {
                    sternZ = points[i].z;
                    stern = clear;
                }
                if (clear >= 0f)
                {
                    touching[i] = false;
                    continue;
                }
                float pen = -clear;
                Vector3 v = s.PointVelocity(wp);
                if (pen > StepHeight)
                {
                    // a step: stop her way across it, do not lift her onto it
                    wall[i] = true;
                    vt[i] = new Vector3(v.x, 0f, v.z);
                    cv[i] = WallGrip * m.mass;                                 // stops that point at once (the cap below keeps it stable)
                    cvSum += cv[i];
                    AgainstStep = true;
                    continue;
                }
                Vector3 nrm = bedN[i];
                float vn = Vector3.Dot(v, nrm);
                float f = stiffness * pen - damping * vn;
                if (f < 0f)
                    f = 0f;
                fn[i] = f;
                Load += f;
                if (!touching[i])
                {
                    touching[i] = true;
                    Kind = bedKind[i];
                    if (Grounds.Hard(bedKind[i]))
                        hardest = Mathf.Max(hardest, v.magnitude);
                }
                Vector3 across = v - nrm * vn;
                vt[i] = across;
                cv[i] = Grounds.Friction(bedKind[i]) * f / Mathf.Max(across.magnitude, SlipSpeed);
                cvSum += cv[i];
                downhill += new Vector3(nrm.x, 0f, nrm.z) * f;
                Touching = true;
            }
            bool fresh = Touching && !wasTouching && clearFor > ClearSeconds;
            clearFor = Touching ? 0f : clearFor + dt;
            if (fresh)
            {
                // she meets the bottom after being clear of it: one plank judged by the hardest blow of that moment
                Touches++;
                if (hardest > 2f * cls.plankLimit)
                {
                    PlanksStove++;
                    LeakAdded += StoveLeak;
                }
                else if (hardest > cls.plankLimit)
                {
                    PlanksCracked++;
                    LeakAdded += CrackLeak;
                }
            }

            // no step may take more than 0.8 of her way across the bottom (the explicit step stays stable)
            float lightest = Mathf.Min(massDiagonal.x, massDiagonal.z);
            float scale = cvSum > 0f ? Mathf.Min(1f, 0.8f * lightest / dt / cvSum) : 1f;
            for (int i = 0; i < n; i++)
            {
                if (fn[i] <= 0f && !wall[i])
                    continue;
                Vector3 wp = s.position + s.rotation * (points[i] - m.cg);
                Vector3 f = -vt[i] * (cv[i] * scale);
                if (!wall[i])
                    f += bedN[i] * fn[i];
                force += f;
                torque += Vector3.Cross(wp - s.position, f);
            }

            // the crew over the side: their weight out of her and their shove towards deeper water
            if (pushLeft > 0f)
            {
                pushLeft -= dt;
                if (!Touching)
                    pushLeft = 0f;
                else
                {
                    Vector3 way = downhill.sqrMagnitude > 1e-6f * Load * Load ? downhill.normalized : -(s.rotation * Vector3.forward);
                    way.y = 0f;
                    way.Normalize();
                    force += way * (cls.crew * ManPush) + Vector3.up * (cls.crew * cls.manKg * Hydrostatics.G);
                }
            }

            LeastClearance = least;
            CentreClearance = centre;
            SternClearance = stern;
            far = least > Far;
            Vector3 flat = new Vector3(s.velocity.x, 0f, s.velocity.z);
            // fast on the bottom once she has lain on it, all but stopped, for a second; off it once clear for a second
            bool fast = Touching && Load > 0.01f * weight && flat.magnitude < 0.15f;
            fastFor = fast ? fastFor + dt : 0f;
            freeFor = Touching ? 0f : freeFor + dt;
            if (fastFor > 1f)
                Aground = true;
            else if (freeFor > 1f)
                Aground = false;
            // the shallow-water factor from the water under her keel amidships (h = clearance + her draught)
            float t = cls.draught;
            float h = float.IsPositiveInfinity(centre) ? float.PositiveInfinity : Mathf.Max(centre, 0f) + t;
            if (h < 2f * t)
            {
                float a = 2f * t / Mathf.Max(h, 0.5f * t) - 1f;
                Shallow = 1f + 0.5f * a * a;
            }
            else
                Shallow = 1f;
        }
    }
}
