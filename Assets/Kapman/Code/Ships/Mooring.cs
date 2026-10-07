using UnityEngine;

namespace Kapman.Ships
{
    /// <summary>
    /// THE ANCHOR (ship physics 2.9): a rope from her stem head to a point on the bottom. The rope is a spring that only
    /// pulls (a wet hemp rope stretches about 15 percent at its breaking load, a guess, so a short rope is stiff and a long
    /// one gives) with some damping. The bottom holds the anchor's weight times the ground's factor (Grounds.Holding:
    /// a 15 kg iron anchor holds 1,470 N in sand), less at a short scope (rope out over depth): all of it from 3:1, falling
    /// to 0.15 of it at 1:1. Let go, the anchor drops under her bow and the crew pay the rope out as she drops back, at
    /// 0.5 m/s, checking it round the post at no more than 0.3 of what the anchor holds so it sets without dragging. When
    /// the rope's pull along the bottom passes the hold, the anchor drags towards her, holding only 0.7 of it while it
    /// drags, until the pull has stayed under half the hold for 5 s and it bites again (guesses): dragging is the normal
    /// failure, and the crew call it. Riding to a sea, the bow lifting on each crest snatches the rope far above the
    /// wind's steady pull: that is what drags her. The rope parts when the pull passes its strength times its integrity
    /// (1 new, less with age and chafe): a new rope only parts in a storm on a bad bottom, an old one in a gale; then the
    /// anchor is lost (Restore gives a new one). Weighing: the crew haul the rope in at 0.3 m/s (no harder than 300 N a
    /// man) and the anchor breaks out when the rope is up and down.
    /// </summary>
    public sealed class Anchor
    {
        public const float RopeStretch = 0.15f, RopeDamping = 0.3f, DragShare = 0.7f;
        public const float HaulSpeed = 0.3f, PayOutSpeed = 0.5f, CheckShare = 0.3f;
        public const float BiteAgainBelow = 0.5f, BiteSeconds = 5f, DefaultScope = 3f;

        readonly ShipClass cls;
        float stiffness, calmFor, wanted;

        /// <summary>The rope's state: 1 new, less with age and chafe.</summary>
        public float integrity = 1f;

        /// <summary>On the bottom with the rope out; the crew hauling it in; dragging; the rope parted (the anchor lost).</summary>
        public bool Down { get; private set; }
        public bool Weighing { get; private set; }
        public bool Dragging { get; private set; }
        public bool Parted { get; private set; }
        /// <summary>Where the anchor lies (world), the rope out (m), the rope the crew mean to have out (m), the water over the
        /// anchor (m), the bottom there.</summary>
        public Vector3 Point { get; private set; }
        public float Rode { get; private set; }
        public float RodeWanted => wanted;
        public float Depth { get; private set; }
        public GroundKind Bottom { get; private set; }
        /// <summary>After each step: the rope's pull (N), what the bottom holds now (N), how far it has dragged since it was let
        /// go (m).</summary>
        public float Tension { get; private set; }
        public float Holding { get; private set; }
        public float Dragged { get; private set; }
        /// <summary>The pull against the hold (the crew's "the anchor's working" above 0.8).</summary>
        public float Strain => Holding > 0f ? Tension / Holding : 0f;
        public float Scope => Depth > 0.05f ? Rode / Depth : 0f;

        public Anchor(ShipClass cls)
        {
            this.cls = cls;
        }

        /// <summary>Her stem head, where the rope runs over the bow, in the model's frame.</summary>
        public static Vector3 BowLocal(ShipSim sim) =>
            new Vector3(0f, sim.Hull.keelY + sim.Hull.depth + sim.Class.sheer, sim.Mass.cg.z + 0.5f * sim.Hull.lwl);

        /// <summary>
        /// Lets go under her bow: the crew pay out the scope ordered (3:1 by default, and a metre over) or what rope there
        /// is. False when there is no bottom with the rope she has, or none under her at all.
        /// </summary>
        public bool LetGo(ShipSim sim, IWaterField water, IBedField bed, float scope = DefaultScope)
        {
            if (cls.anchorKg <= 0f || cls.rodeM <= 0f || bed == null || Parted)
                return false;
            Vector3 bow = sim.State.position + sim.State.rotation * (BowLocal(sim) - sim.Mass.cg);
            if (!bed.Bed(bow, out float bottom, out _, out GroundKind kind))
                return false;
            water.Sample(bow, out float level, out _);
            float depth = level - bottom;
            if (depth <= 0.05f || depth + 0.5f > cls.rodeM)
                return false;
            Point = new Vector3(bow.x, bottom, bow.z);
            Depth = depth;
            Bottom = kind;
            wanted = Mathf.Clamp(scope * depth + 1f, depth + 1f, cls.rodeM);
            Rode = Mathf.Min(wanted, Mathf.Max(depth + 0.5f, bow.y - bottom));
            Down = true;
            Weighing = Dragging = false;
            Dragged = 0f;
            Tension = 0f;
            calmFor = 0f;
            Stiffen();
            return true;
        }

        /// <summary>More rope (m), as far as there is: the crew pay it out.</summary>
        public void PayOut(float metres)
        {
            if (Down)
                wanted = Mathf.Clamp(wanted + metres, 0.5f, cls.rodeM);
        }

        /// <summary>The crew start hauling it in.</summary>
        public void Weigh()
        {
            if (Down)
                Weighing = true;
        }

        /// <summary>Aboard at once (she has been moved by something else: a level entry, a load, a salvage).</summary>
        public void Stow()
        {
            Down = Weighing = Dragging = false;
            Tension = 0f;
        }

        /// <summary>A new rope and anchor (the shipwright, a chandler).</summary>
        public void Restore()
        {
            Parted = false;
            integrity = 1f;
        }

        void Stiffen() => stiffness = cls.rodeStrengthN / (RopeStretch * Mathf.Max(Rode, 1f));

        /// <summary>The rope's pull on her bow (added to her force and torque), the anchor's dragging and the rope's parting.</summary>
        public void Compute(ShipSim sim, IWaterField water, IBedField bed, float dt, ref Vector3 force, ref Vector3 torque)
        {
            Tension = 0f;
            if (!Down)
                return;
            var s = sim.State;
            Vector3 bow = s.position + s.rotation * (BowLocal(sim) - sim.Mass.cg);
            water.Sample(Point, out float level, out _);
            Depth = Mathf.Max(0.05f, level - Point.y);
            float fullHold = Grounds.Holding(Bottom, cls.anchorIsStone, cls.anchorKg);
            float check = CheckShare * fullHold;

            Vector3 to = Point - bow;
            float dist = to.magnitude;
            bool payingOut = !Weighing && Rode < wanted;
            if (Weighing)
            {
                // the men haul while they can: no harder than 300 N a man
                float haulMost = Mathf.Max(1f, cls.crew) * Grounding.ManPush;
                if (stiffness * Mathf.Max(0f, dist - Rode) < haulMost)
                    Rode = Mathf.Max(0f, Rode - HaulSpeed * dt);
                Stiffen();
                float across = new Vector2(to.x, to.z).magnitude;
                if (Rode <= dist + 0.3f && across < 0.5f + 0.3f * Depth)
                {
                    Down = Weighing = Dragging = false;                         // up and down: it breaks out and comes aboard
                    return;
                }
            }
            else if (payingOut)
            {
                // steadily, and the rope surges round the post whenever she pulls harder than the men check it
                Rode = Mathf.Min(wanted, Mathf.Max(Rode + PayOutSpeed * dt, dist - check / stiffness));
                Stiffen();
            }
            if (dist < 1e-3f)
                return;
            Vector3 dir = to / dist;
            float scopeShare = Mathf.Clamp01(0.15f + 0.85f * (Scope - 1f) / 2f);
            Holding = fullHold * scopeShare * (Dragging ? DragShare : 1f);
            float stretch = dist - Rode;
            if (stretch <= 0f)
            {
                calmFor += dt;
                if (calmFor > BiteSeconds)
                    Dragging = false;
                return;                                                        // slack
            }
            float damping = 2f * RopeDamping * Mathf.Sqrt(stiffness * sim.Mass.mass);
            float lengthening = -Vector3.Dot(s.PointVelocity(bow), dir);
            float t = Mathf.Max(0f, stiffness * stretch + damping * lengthening);
            if (payingOut)
                t = Mathf.Min(t, check);

            if (t > integrity * cls.rodeStrengthN)
            {
                Tension = t;
                Parted = true;
                Down = Weighing = Dragging = false;
                return;
            }

            // the pull along the bottom against the hold
            Vector3 flat = new Vector3(dir.x, 0f, dir.z);
            float flatShare = flat.magnitude;
            float along = t * flatShare;
            if (along > Holding && flatShare > 0.05f)
            {
                if (!Dragging)
                {
                    Dragging = true;
                    Holding *= DragShare;
                }
                calmFor = 0f;
                // the anchor comes home towards her until the rope's stretch pulls no more than the bottom holds
                float most = Holding / flatShare;
                float slide = Mathf.Max(0f, stretch - most / stiffness);
                Vector3 p = Point - flat / flatShare * slide;
                if (bed != null && bed.Bed(p, out float bottom, out _, out GroundKind kind))
                {
                    p.y = bottom;
                    Bottom = kind;
                }
                Point = p;
                Dragged += slide;
                t = most;
            }
            else if (Dragging)
            {
                calmFor = along < BiteAgainBelow * Holding ? calmFor + dt : 0f;
                if (calmFor > BiteSeconds)
                    Dragging = false;
            }
            Tension = t;
            Vector3 f = dir * t;
            force += f;
            torque += Vector3.Cross(bow - s.position, f);
        }
    }

    /// <summary>
    /// LYING AT A BERTH (ship physics 2.9): a bow line and a stern line to posts (Dorestad's piles, a quay's bollards). The
    /// lines' eyes ride up and down the posts, so they act across the water only and a rising or falling tide never hangs
    /// her on them; each pulls only when taut (the anchor rope's spring, the class's rope strength) and they can part.
    /// </summary>
    public sealed class Berth
    {
        readonly ShipClass cls;
        Vector3 bowPost, sternPost;
        float bowLine, sternLine;

        public bool Fast { get; private set; }
        public bool Parted { get; private set; }
        /// <summary>After each step: the harder of the two lines' pulls (N).</summary>
        public float Tension { get; private set; }

        public Berth(ShipClass cls)
        {
            this.cls = cls;
        }

        public static Vector3 SternLocal(ShipSim sim) =>
            new Vector3(0f, sim.Hull.keelY + sim.Hull.depth + sim.Class.sheer, sim.Mass.cg.z - 0.5f * sim.Hull.lwl);

        /// <summary>Makes her fast to two posts with this much slack in each line (m).</summary>
        public void MakeFast(ShipSim sim, Vector3 bowPostAt, Vector3 sternPostAt, float slack = 0.5f)
        {
            var s = sim.State;
            Vector3 bow = s.position + s.rotation * (Anchor.BowLocal(sim) - sim.Mass.cg);
            Vector3 stern = s.position + s.rotation * (SternLocal(sim) - sim.Mass.cg);
            bowPost = bowPostAt;
            sternPost = sternPostAt;
            bowLine = Flat(bow - bowPost).magnitude + slack;
            sternLine = Flat(stern - sternPost).magnitude + slack;
            Fast = true;
            Parted = false;
        }

        /// <summary>Makes her fast where she lies, to posts at her stem and stern (the piles alongside).</summary>
        public void MakeFastHere(ShipSim sim, float slack = 0.5f)
        {
            var s = sim.State;
            MakeFast(sim, s.position + s.rotation * (Anchor.BowLocal(sim) - sim.Mass.cg),
                     s.position + s.rotation * (SternLocal(sim) - sim.Mass.cg), slack);
        }

        public void CastOff()
        {
            Fast = false;
            Tension = 0f;
        }

        static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

        public void Compute(ShipSim sim, float dt, ref Vector3 force, ref Vector3 torque)
        {
            Tension = 0f;
            if (!Fast)
                return;
            var s = sim.State;
            Vector3 bow = s.position + s.rotation * (Anchor.BowLocal(sim) - sim.Mass.cg);
            Vector3 stern = s.position + s.rotation * (SternLocal(sim) - sim.Mass.cg);
            float a = Line(sim, bow, bowPost, bowLine, ref force, ref torque);
            float b = Line(sim, stern, sternPost, sternLine, ref force, ref torque);
            Tension = Mathf.Max(a, b);
            if (Tension > cls.rodeStrengthN)
            {
                Parted = true;
                Fast = false;
            }
        }

        static float Line(ShipSim sim, Vector3 at, Vector3 post, float length, ref Vector3 force, ref Vector3 torque)
        {
            var s = sim.State;
            Vector3 to = Flat(post - at);
            float dist = to.magnitude;
            float stretch = dist - length;
            if (stretch <= 0f || dist < 1e-3f)
                return 0f;
            Vector3 dir = to / dist;
            float k = sim.Class.rodeStrengthN / (Anchor.RopeStretch * Mathf.Max(length, 1f));
            float c = 2f * Anchor.RopeDamping * Mathf.Sqrt(k * sim.Mass.mass);
            float t = Mathf.Max(0f, k * stretch - c * Vector3.Dot(Flat(s.PointVelocity(at)), dir));
            Vector3 f = dir * t;
            force += f;
            torque += Vector3.Cross(at - s.position, f);
            return t;
        }
    }
}
