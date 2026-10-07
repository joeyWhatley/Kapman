using UnityEngine;

namespace Kapman.Ships
{
    /// <summary>How she ends, if she does (ship physics 2.8): afloat; swamped (full to the gunwale, awash on her own wood);
    /// capsized (over past her vanishing angle). The body only says so: the game decides what it means (the user's
    /// rule: lost at sea, a salvage in a river or a port).</summary>
    public enum ShipFate { Afloat, Swamped, Capsized }

    /// <summary>The standing bail order (ship physics 3): the crew bail by themselves as the water rises; all hands takes
    /// every man off the oars and the sheets to bail; or nobody bails.</summary>
    public enum BailOrder { AsNeeded, AllHands, Stop }

    /// <summary>
    /// THE WATER IN HER (ship physics 2.8). It comes in by four roads, in m3 a second:
    ///  1. over the gunwale: wherever the water outside stands above the gunwale (amidships her depth, rising by the sheer
    ///     towards the ends, the heel and the waves in it), a weir C_w x length x excess^1.5 x sqrt(2 g) with C_w = 0.1 (the
    ///     design's guess: a quarter of a sharp-crested weir's), on each side cell's length; a sea over the bow, the lee
    ///     gunwale dipping and a following sea over the stern are all this;
    ///  2. spray: in a sea of Hs above 0.3 of her freeboard, 0.005 Hs (1 - cos(angle to the sea)) m3 a minute for each metre
    ///     of beam (none running before it, most head to it);
    ///  3. the laps: the class's leak for a sound hull, ten times more when she works hard in a sea (her pitch and roll
    ///     rates), plus whatever planks are cracked or stove in;
    ///  4. rain: the weather's rain (0-1, 1 = 10 mm an hour) on her open hull.
    /// The crew bail (the standing order): nobody under 5 cm over the keel, one man from 5 cm until it is under 3, two from
    /// 15 cm until it is under 10; all hands puts every man to it at the burst rate. A man lifts 0.6 litres a scoop every
    /// 1.5 s, about 25 l a minute held, 40 in a burst, slower when he has to reach for it under 5 cm; four men hold about
    /// 100 l a minute, so a sea that pours in 28 l a second cannot be bailed, only stopped.
    /// The water lies in her as a pool from the keel up (her own shape, 0.9 of the outside for the planking and frames): its
    /// weight sits low on the centreline (the body takes it into her mass), and its free surface runs to the low side as
    /// she heels, a heeling moment of rho g i sin(heel) (i the pool's surface second moment about her centreline), which is
    /// the design's GM loss of i / V; it follows the heel half a second late, so a flooded boat lurches. She takes no more
    /// than floats her awash (with her own wood and trapped air a swamped boat lies with her gunwale at the water rather
    /// than sinking), so the scene keeps her for the salvage or the end.
    /// Her fate: capsized when her heel stays past her vanishing angle (the downflooding angle and the class's margin) for
    /// a second, or passes 90 degrees; swamped when the gunwale has been under for 3 s with the water above 60 percent of
    /// what she can hold.
    /// </summary>
    public sealed class Bilge
    {
        public const float WeirCoefficient = 0.1f;
        public const float ManLitresPerMinute = 25f, BurstLitresPerMinute = 40f;
        public const float OneManFrom = 0.05f, OneManUntil = 0.03f, TwoMenFrom = 0.15f, TwoMenUntil = 0.10f;
        /// <summary>The share of her outside volume that is room inside (planks and frames take the rest) (guess).</summary>
        public const float InteriorShare = 0.9f;
        public const float SwampShare = 0.6f, SwampSeconds = 3f, CapsizeSeconds = 1f;
        /// <summary>How late the pool follows her heel (s) (guess).</summary>
        public const float PoolLag = 0.5f;
        /// <summary>She floats awash with her gunwale this share of the way under.</summary>
        public const float AwashShare = 0.97f;

        readonly ShipClass cls;
        readonly HullProbes hull;
        readonly float gunwaleVolume, openArea;
        int bailers;
        float poolSin, underFor, overFor, lastWater = -1f;

        /// <summary>The standing order.</summary>
        public BailOrder order = BailOrder.AsNeeded;
        /// <summary>The leak from damage (m3/h): cracked and stove-in planks.</summary>
        public float damageLeak;

        /// <summary>The water aboard (m3), its depth over the keel upright (m), the centre of its weight above the keel (m),
        /// and the room in her to the gunwale (m3).</summary>
        public float Water { get; private set; }
        public float Depth { get; private set; }
        public float PoolHeight { get; private set; }
        public float Capacity { get; private set; }
        /// <summary>The most she can take with the mass she has (awash, or full) (m3).</summary>
        public float Full { get; private set; }
        /// <summary>After each step, m3 a second: all that came in, and by road; what the crew bailed out; the men bailing.</summary>
        public float Inflow { get; private set; }
        public float OverGunwale { get; private set; }
        public float Spray { get; private set; }
        public float Leaks { get; private set; }
        public float RainIn { get; private set; }
        public float Bailing { get; private set; }
        public int Bailers => bailers;
        /// <summary>The pool's free surface: its second moment about her centreline (m4) and the metacentric height it takes
        /// from her (m).</summary>
        public float SurfaceInertia { get; private set; }
        public float GMLoss { get; private set; }
        /// <summary>The least height of her gunwale above the water outside (m; below 0 the gunwale is under).</summary>
        public float LowestFreeboard { get; private set; } = float.PositiveInfinity;
        public bool GunwaleUnder => LowestFreeboard < 0f;
        public ShipFate Fate { get; private set; }
        public bool Lost => Fate != ShipFate.Afloat;

        public Bilge(ShipClass cls, HullProbes hull)
        {
            this.cls = cls;
            this.hull = hull;
            gunwaleVolume = Hydrostatics.Volume(hull, hull.depth);
            Capacity = InteriorShare * gunwaleVolume;
            Full = Capacity;
            foreach (var c in hull.cells)
                openArea += c.area;
        }

        /// <summary>The weir: m3 a second over a length of gunwale (m) with the water standing this far above it (m).</summary>
        public static float Overflow(float length, float excess) =>
            excess > 0f ? WeirCoefficient * length * excess * Mathf.Sqrt(excess) * Mathf.Sqrt(2f * Hydrostatics.G) : 0f;

        /// <summary>What men bailing lift out (m3 a second) with the water this deep over the keel; burst: all hands.</summary>
        public static float BailRate(int men, float depth, bool burst)
        {
            if (men <= 0)
                return 0f;
            float reach = depth < OneManFrom ? Mathf.Max(0.3f, depth / OneManFrom) : 1f;
            return men * (burst ? BurstLitresPerMinute : ManLitresPerMinute) / 60000f * reach;
        }

        /// <summary>Puts water in her (a sea breaking aboard; the tests).</summary>
        public void Add(float m3) => Water = Mathf.Clamp(Water + m3, 0f, Full);

        /// <summary>Pumped dry, her fate cleared, the damage kept (a salvage, or she is taken up somewhere new).</summary>
        public void Empty()
        {
            Water = 0f;
            Fate = ShipFate.Afloat;
            underFor = overFor = poolSin = 0f;
            bailers = 0;
            lastWater = -1f;
        }

        /// <summary>
        /// One step: the water in and out, the free surface's moment added to her torque, her fate. Rain 0-1; the sea's
        /// significant height (m) and the way its waves run (degrees from north); her mass without the water (kg).
        /// </summary>
        public void Step(ShipSim sim, float rain, float seaHs, float seaTowardsDeg, float dryMass, float dt, ref Vector3 torque)
        {
            var s = sim.State;
            var m = sim.Mass;
            Quaternion r = s.rotation;
            Full = Mathf.Clamp((AwashShare * gunwaleVolume * Hydrostatics.Rho - dryMass) / Hydrostatics.Rho, 0f, Capacity);

            // 1. over the gunwale, cell by side cell
            float over = 0f, lowest = float.PositiveInfinity;
            var cells = hull.cells;
            for (int i = 0; i < cells.Length; i++)
            {
                var c = cells[i];
                if (c.centre)
                    continue;
                float breadth = Mathf.Abs(c.keel.x) / HullProbes.SideOut;
                float u = 2f * c.keel.z / hull.lwl;
                Vector3 g = new Vector3(Mathf.Sign(c.keel.x) * 0.5f * breadth, hull.keelY + hull.depth + cls.sheer * u * u, c.keel.z);
                float gy = (s.position + r * (g - m.cg)).y;
                float excess = sim.Forces.HeightAt(i) - gy;
                lowest = Mathf.Min(lowest, -excess);
                over += Overflow(hull.stationLength, excess);
            }
            LowestFreeboard = lowest;
            OverGunwale = over;

            // 2. spray
            Spray = 0f;
            if (seaHs > 0.3f * cls.Freeboard)
            {
                float a = Mathf.DeltaAngle(Rot.Heading(r), seaTowardsDeg) * Mathf.Deg2Rad;
                Spray = 0.005f * seaHs * (1f - Mathf.Cos(a)) * cls.beam / 60f;
            }

            // 3. the laps, working in a sea; 4. rain
            Vector3 w = s.angularVelocity;
            float working = Mathf.Clamp01((Mathf.Abs(w.x) + Mathf.Abs(w.z)) / 0.5f);
            Leaks = (cls.leakM3h * (1f + 10f * working) + damageLeak) / 3600f;
            RainIn = Mathf.Max(0f, rain) * openArea * 0.01f / 3600f;
            Inflow = over + Spray + Leaks + RainIn;

            // the crew bail
            int crew = Mathf.Max(0, Mathf.RoundToInt(cls.crew));
            if (order == BailOrder.Stop)
                bailers = 0;
            else if (order == BailOrder.AllHands)
                bailers = crew;
            else
            {
                if (Depth > TwoMenFrom)
                    bailers = 2;
                else if (bailers >= 2 && Depth < TwoMenUntil)
                    bailers = 1;
                if (bailers == 0 && Depth > OneManFrom)
                    bailers = 1;
                else if (bailers == 1 && Depth < OneManUntil)
                    bailers = 0;
                bailers = Mathf.Min(bailers, crew);
            }
            Bailing = Mathf.Min(BailRate(bailers, Depth, order == BailOrder.AllHands), Water / dt + Inflow);
            Water = Mathf.Clamp(Water + (Inflow - Bailing) * dt, 0f, Full);

            // the pool: its depth, the centre of its weight, its free surface
            if (Mathf.Abs(Water - lastWater) > 1e-6f)
            {
                lastWater = Water;
                Pool(Water);
            }
            GMLoss = SurfaceInertia / Mathf.Max(m.mass / Hydrostatics.Rho, 1e-3f);
            float heel = Rot.Heel(r);
            poolSin += (Mathf.Sin(heel * Mathf.Deg2Rad) - poolSin) * Mathf.Min(1f, dt / PoolLag);
            if (SurfaceInertia > 0f)
                torque += r * new Vector3(0f, 0f, Hydrostatics.Rho * Hydrostatics.G * SurfaceInertia * poolSin);

            // her fate
            underFor = GunwaleUnder ? underFor + dt : 0f;
            overFor = Mathf.Abs(heel) > cls.VanishingDeg ? overFor + dt : 0f;
            if (Fate == ShipFate.Afloat)
            {
                if (overFor > CapsizeSeconds || Mathf.Abs(heel) > 90f || (r * Vector3.up).y < 0f)
                    Fate = ShipFate.Capsized;
                else if (underFor > SwampSeconds && Water > SwampShare * Full)
                    Fate = ShipFate.Swamped;
            }
        }

        /// <summary>The pool's depth over the keel for a volume (upright), the height of its centre, and its free surface's
        /// second moment about her centreline: station by station, (its breadth at that depth)^3 x length / 12.</summary>
        void Pool(float volume)
        {
            if (volume <= 1e-6f)
            {
                Depth = PoolHeight = SurfaceInertia = 0f;
                return;
            }
            float lo = 0f, hi = hull.depth;
            for (int it = 0; it < 24 && hi - lo > 1e-4f; it++)
            {
                float mid = 0.5f * (lo + hi);
                if (InteriorShare * Hydrostatics.Volume(hull, mid) < volume)
                    lo = mid;
                else
                    hi = mid;
            }
            float d = 0.5f * (lo + hi);
            Depth = d;
            float v = 0f, mom = 0f, inertia = 0f;
            var cells = hull.cells;
            int stations = hull.stationZ.Length;
            for (int st = 0; st < stations; st++)
            {
                float area = 0f;
                for (int k = 0; k < 3; k++)
                {
                    var c = cells[st * 3 + k];
                    float cv = c.Volume(d);
                    v += cv;
                    mom += cv * c.Centroid(d);
                    area += c.Area(d);
                }
                float breadth = area / hull.stationLength;
                inertia += hull.stationLength * breadth * breadth * breadth / 12f;
            }
            PoolHeight = v > 0f ? mom / v : 0f;
            SurfaceInertia = inertia;
        }
    }
}
