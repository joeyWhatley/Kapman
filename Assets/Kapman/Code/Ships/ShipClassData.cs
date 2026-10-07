using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Kapman.Game;

namespace Kapman.Ships
{
    /// <summary>
    /// ONE KIND OF SHIP, AS A BODY - the numbers the hull, the scales and the oars are built from (ship physics 2.1): her
    /// size, her shape (block coefficient and section shape), what she weighs and where it sits, how she swings, how the
    /// water holds her back and what her oars give. One row of Data/Ships/ships.json. Lengths in metres, masses in
    /// kilograms, heights above the keel; from/to are years on the water with 0 meaning "before the game" or "no end"
    /// (the first build's ship_classes_793.json convention).
    /// </summary>
    public sealed class ShipClass
    {
        public string id = "", name = "";
        /// <summary>"in_use" (a ship the game sails now) or "later" (numbers kept for a hull that is not in the game yet).</summary>
        public string status = "in_use";
        public int from, to;

        // size and shape
        public float length, beam, lwl, depth, draught, cb, sectionQ;

        // weights (kg) and their heights above the keel (m)
        public float hullMassKg, hullCgM, crew, manKg, crewCgM, gearKg, gearCgM, cargoKg, cargoCgM;

        // how she swings: radii of gyration (m) and the added-mass coefficients on each axis (ship physics 2.2)
        public float kRoll, kPitch, kYaw;
        public float addSurge, addSway, addHeave, addRoll, addPitch, addYaw;

        // the water's hold on her
        public float heaveDampingRatio, heaveDragCd, lateralArea, lateralCd;
        public float[] crFn = new float[0], cr = new float[0];

        // the oars and the steering oar
        public int oars;
        public float oarPowerW, oarEfficiency, oarPullMaxN, oarTurnTorqueNm, rudderArea, rudderArm;
        /// <summary>The sail: its area (m2; 0 = no sail), its centre of effort over the waterline (m), the closest she sails to
        /// the true wind (degrees).</summary>
        public float sailArea, sailCe, closestToWind;

        // checks and limits
        public float gm, hsLimit;

        /// <summary>The open hull (ship physics 2.8): how much the gunwale rises at the ends (m), her windage side-on (m2),
        /// the laps' leak when sound (m3/h), the contact speed that cracks a plank (m/s), and how far past the
        /// downflooding angle she still comes back (degrees).</summary>
        public float sheer, windage, leakM3h, plankLimit, vanishingBeyondFlood;
        /// <summary>The ground tackle (ship physics 2.9): the anchor's weight (kg) and kind, the rope's length (m) and
        /// what it bears new (N).</summary>
        public float anchorKg, rodeM, rodeStrengthN;
        public bool anchorIsStone;

        /// <summary>Everything aboard at her laden draught: hull, crew, gear and cargo (kg).</summary>
        public float LadenMass => hullMassKg + crew * manKg + gearKg + cargoKg;

        /// <summary>Her freeboard amidships at the laden draught (m).</summary>
        public float Freeboard => depth - draught;

        /// <summary>The heel at which the gunwale amidships meets still water, as a box (degrees): atan(2 F / B)
        /// (ship physics 1.6).</summary>
        public float DownfloodingDeg => (float)(Math.Atan2(2.0 * Freeboard, beam) * 180.0 / Math.PI);

        /// <summary>The heel past which she does not come back (degrees): the downflooding angle and the class's margin.</summary>
        public float VanishingDeg => DownfloodingDeg + vanishingBeyondFlood;

        /// <summary>Whether the class is on the water in a year (from/to, 0 = open).</summary>
        public bool OnWaterIn(int year) => (from == 0 || year >= from) && (to == 0 || year <= to);

        /// <summary>The residuary resistance coefficient at a Froude number, straight-line between the table's points
        /// and held at its ends (ship physics 2.4).</summary>
        public float Cr(float fn)
        {
            int n = crFn.Length;
            if (fn <= crFn[0])
                return cr[0] * Math.Max(0f, fn) / crFn[0];               // down to nothing at rest
            for (int i = 1; i < n; i++)
                if (fn <= crFn[i])
                {
                    float t = (fn - crFn[i - 1]) / (crFn[i] - crFn[i - 1]);
                    return cr[i - 1] + (cr[i] - cr[i - 1]) * t;
                }
            return cr[n - 1];
        }

        public override string ToString() => $"{name} ({id})";
    }

    /// <summary>
    /// THE SHIPS AS BODIES - reads Data/Ships/ships.json into <see cref="ShipClass"/> rows. Every number the body needs
    /// must be there: a missing or nonsensical one is an error naming the ship and the field, never a quiet default.
    /// </summary>
    public sealed class ShipClassData
    {
        /// <summary>The file in the project (relative to the project folder, as the EditMode tests run).</summary>
        public const string ProjectPath = "Assets/Kapman/Data/Ships/ships.json";

        public readonly List<ShipClass> classes = new List<ShipClass>();

        public static ShipClassData Load(string path) => Parse(File.ReadAllText(path));

        public static ShipClassData Parse(string json)
        {
            var root = Json.Parse(json);
            var rows = Json.Arr(root, "classes");
            if (rows == null || rows.Count == 0)
                throw new FormatException("ships.json: no \"classes\"");
            var data = new ShipClassData();
            foreach (var row in rows)
                data.classes.Add(Row(row));
            var ids = new HashSet<string>();
            foreach (var c in data.classes)
                if (!ids.Add(c.id))
                    throw new FormatException($"ships.json: the id '{c.id}' is used twice");
            return data;
        }

        /// <summary>The class with this id; an error naming the ids there are when it is not in the file.</summary>
        public ShipClass Get(string id)
        {
            foreach (var c in classes)
                if (c.id == id)
                    return c;
            throw new KeyNotFoundException($"ships.json has no ship '{id}' (it has: {string.Join(", ", classes.ConvertAll(c => c.id))})");
        }

        static ShipClass Row(object o)
        {
            string id = Json.Str(o, "id");
            if (string.IsNullOrEmpty(id))
                throw new FormatException("ships.json: a class without an id");
            var c = new ShipClass
            {
                id = id,
                name = Json.Str(o, "name", id),
                status = Json.Str(o, "status", "in_use"),
                from = (int)Need(o, id, "from", 0f),
                to = (int)Need(o, id, "to", 0f),
                length = Need(o, id, "length_m"),
                beam = Need(o, id, "beam_m"),
                lwl = Need(o, id, "lwl_m"),
                depth = Need(o, id, "depth_m"),
                draught = Need(o, id, "draught_m"),
                cb = Need(o, id, "cb"),
                sectionQ = Need(o, id, "section_q"),
                hullMassKg = Need(o, id, "hull_mass_kg"),
                hullCgM = Need(o, id, "hull_cg_m"),
                crew = Need(o, id, "crew", 0f),
                manKg = Need(o, id, "man_kg"),
                crewCgM = Need(o, id, "crew_cg_m"),
                gearKg = Need(o, id, "gear_kg", 0f),
                gearCgM = Need(o, id, "gear_cg_m"),
                cargoKg = Need(o, id, "cargo_kg", 0f),
                cargoCgM = Need(o, id, "cargo_cg_m"),
                kRoll = Need(o, id, "k_roll_m"),
                kPitch = Need(o, id, "k_pitch_m"),
                kYaw = Need(o, id, "k_yaw_m"),
                heaveDampingRatio = Need(o, id, "heave_damping_ratio"),
                heaveDragCd = Need(o, id, "heave_drag_cd", 0f),
                lateralArea = Need(o, id, "lateral_area_m2"),
                lateralCd = Need(o, id, "lateral_cd"),
                oars = (int)Need(o, id, "oars", 0f),
                oarPowerW = Need(o, id, "oar_power_w", 0f),
                oarEfficiency = Need(o, id, "oar_efficiency", 0f),
                oarPullMaxN = Need(o, id, "oar_pull_max_n", 0f),
                oarTurnTorqueNm = Need(o, id, "oar_turn_torque_nm", 0f),
                rudderArea = Need(o, id, "steering_oar_area_m2", 0f),
                rudderArm = Need(o, id, "steering_oar_arm_m", 0f),
                sailArea = Optional(o, id, "sail_area_m2"),
                sailCe = Optional(o, id, "sail_ce_m"),
                closestToWind = Optional(o, id, "closest_to_wind_deg"),
                gm = Need(o, id, "gm_m"),
                hsLimit = Need(o, id, "hs_limit_m"),
                sheer = Need(o, id, "sheer_m", 0f),
                windage = Need(o, id, "windage_m2", 0f),
                leakM3h = Need(o, id, "leak_m3_per_h", 0f),
                plankLimit = Need(o, id, "plank_limit_ms"),
                vanishingBeyondFlood = Need(o, id, "vanishing_beyond_flood_deg", 0f),
                anchorKg = Need(o, id, "anchor_kg", 0f),
                rodeM = Need(o, id, "rode_m", 0f),
                rodeStrengthN = Need(o, id, "rode_strength_n", 0f),
            };
            string kind = Json.Str(o, "anchor_kind", "");
            if (kind != "iron" && kind != "stone")
                throw new FormatException($"ships.json: '{id}' \"anchor_kind\" must be iron or stone, not '{kind}'");
            c.anchorIsStone = kind == "stone";
            var added = Json.Obj(o, "added_mass");
            if (added == null)
                throw new FormatException($"ships.json: '{id}' has no \"added_mass\"");
            c.addSurge = Need(added, id, "surge", 0f);
            c.addSway = Need(added, id, "sway", 0f);
            c.addHeave = Need(added, id, "heave", 0f);
            c.addRoll = Need(added, id, "roll", 0f);
            c.addPitch = Need(added, id, "pitch", 0f);
            c.addYaw = Need(added, id, "yaw", 0f);
            c.crFn = Floats(o, id, "cr_fn");
            c.cr = Floats(o, id, "cr");
            if (c.crFn.Length != c.cr.Length || c.crFn.Length < 2)
                throw new FormatException($"ships.json: '{id}' needs cr_fn and cr of the same length (2 or more)");
            for (int i = 1; i < c.crFn.Length; i++)
                if (c.crFn[i] <= c.crFn[i - 1])
                    throw new FormatException($"ships.json: '{id}' cr_fn must rise");
            if (c.draught >= c.depth)
                throw new FormatException($"ships.json: '{id}' draws {c.draught} m in a hull {c.depth} m deep");
            if (c.cb * c.sectionQ > 1.0001f)
                throw new FormatException($"ships.json: '{id}' cb {c.cb} is too full for section_q {c.sectionQ} (cb x q must not pass 1)");
            return c;
        }

        /// <summary>A number the row may leave out (0 then), never below 0: the sail of a ship that has none.</summary>
        static float Optional(object o, string id, string key)
        {
            double v = Json.Num(o, key, 0.0);
            if (double.IsNaN(v) || v < 0.0)
                throw new FormatException($"ships.json: '{id}' has \"{key}\" {v}, below 0");
            return (float)v;
        }

        /// <summary>A number the row must have: positive unless a lowest value is given.</summary>
        static float Need(object o, string id, string key, float least = float.Epsilon)
        {
            double v = Json.Num(o, key, double.NaN);
            if (double.IsNaN(v))
                throw new FormatException($"ships.json: '{id}' has no number \"{key}\"");
            if (v < least)
                throw new FormatException(string.Format(CultureInfo.InvariantCulture, "ships.json: '{0}' \"{1}\" is {2}, below {3}", id, key, v, least));
            return (float)v;
        }

        static float[] Floats(object o, string id, string key)
        {
            var a = Json.Arr(o, key);
            if (a == null)
                throw new FormatException($"ships.json: '{id}' has no list \"{key}\"");
            var f = new float[a.Count];
            for (int i = 0; i < a.Count; i++)
                f[i] = a[i] is double d ? (float)d : throw new FormatException($"ships.json: '{id}' \"{key}\" holds a non-number");
            return f;
        }
    }
}
