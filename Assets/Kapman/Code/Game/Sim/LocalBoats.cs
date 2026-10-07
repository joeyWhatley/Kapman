using System;
using System.Collections.Generic;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game.Sim
{
    /// <summary>
    /// LOCAL BOATS - the small craft of the shore round the player: fishing, fowling, carrying, working the weirs and
    /// traps in the creeks and along the shore (Data/traffic_793.json: fishing - boats out per 10 km of shore by region and
    /// month on a fair day; in 793 fishing is inshore and estuarine, no sea-going fleets). Out from first light, home by
    /// dusk; half as many when the wind is up, none in a gale. Each patch of near-shore water (500 m) gets its boats from
    /// its own dice for the day, so they're the same each time you pass. Logboats, punts and small clinker boats: the
    /// Gislinge model scaled small, mostly without a mast. Only near the player (a small boat drops below the horizon at
    /// ~8 km).
    /// Ported 4 Oct: the land, the water, the wind and the clock come from the stage (SimStage: the level's terrains,
    /// water, weather and clock), not from LandStreamer, WaterSource, LocalWeather and GameClock. The first build's
    /// ShipBody (she rode the sea and left a wake) is gone with its water system: the boats sit on the water's height
    /// and swing, without pitch, roll or wake.
    /// </summary>
    public class LocalBoats : MonoBehaviour
    {
        [Tooltip("For now, no local boats at all (the user, 2026-10-03: \"remove all other ships in the rivers and sea for now\").")]
        public bool off = true;
        public GameObject prefab;
        [Tooltip("The most local boats shown (the nearest).")]
        public int maxBoats = 20;
        [Tooltip("Real km round the player within which local boats are shown.")]
        public float reachKm = 7f;
        [Tooltip("Real metres of a patch of water (each patch has its own boats for the day).")]
        public float patch = 500f;
        [Tooltip("Metres of the Gislinge model's length (its scale 1).")]
        public float modelLength = 7.7f;

        sealed class Boat
        {
            public GameObject go;
            public double x, z;
            public float heading, length, phase;
        }

        readonly List<Boat> boats = new List<Boat>();
        readonly List<(double x, double z, float heading, float length, float phase, double d2)> wanted = new List<(double, double, float, float, float, double)>();
        Dictionary<string, (float[] months, string classes)> fishing;
        Dictionary<string, Dictionary<string, float>> classSets;
        float nextRefresh;
        WorldXZ lastWhere;
        int lastDay = -1;

        void LateUpdate()
        {
            var game = KapmanGame.Current;
            if (off)
            {
                foreach (var b in boats)
                    if (b.go != null && b.go.activeSelf)
                        b.go.SetActive(false);
                return;
            }
            if (game == null || game.ship == null || prefab == null)
                return;
            var me = SimStage.ToWorld(game.ShipPosition);
            int day = (int)Math.Floor(game.Day);
            bool moved = (me.x - lastWhere.x) * (me.x - lastWhere.x) + (me.z - lastWhere.z) * (me.z - lastWhere.z) > patch * patch;
            if (Time.unscaledTime >= nextRefresh || moved || day != lastDay)
            {
                nextRefresh = Time.unscaledTime + 3f;
                lastWhere = me;
                lastDay = day;
                Refresh(game, me, day);
            }
            // place them (the scene's origin may have moved), riding the water, swinging a little at their work
            float t = Time.time;
            foreach (var b in boats)
            {
                if (!b.go.activeSelf)
                    continue;
                var at = SimStage.ToScene(new WorldXZ(b.x, b.z));       // (on the water: ToScene gives the water's height)
                b.go.transform.SetPositionAndRotation(at, Quaternion.Euler(0f, b.heading + 6f * Mathf.Sin(t * 0.13f + b.phase), 0f));
            }
        }

        void Refresh(KapmanGame game, WorldXZ me, int day)
        {
            Load(game);
            wanted.Clear();
            if (float.IsNaN(SimStage.LandAboveWater(me)))                // (no terrain in this scene: no shore to work)
            {
                Show();
                return;
            }
            // daylight and weather
            double sun = SimStage.SunElevationAt(me);
            SimStage.Wind(out _, out float wind);
            float out_ = sun < 2.0 ? 0f : wind > 17f ? 0f : wind > 10f ? 0.5f : 1f;
            if (out_ <= 0f)
            {
                Show();
                return;
            }
            int month = game.Month;
            double reach = reachKm * 1000.0;
            int n = (int)Math.Ceiling(reach / patch);
            long cx0 = (long)Math.Floor(me.x / patch), cz0 = (long)Math.Floor(me.z / patch);
            for (long i = -n; i <= n; i++)
                for (long k = -n; k <= n; k++)
                {
                    double x = (cx0 + i + 0.5) * patch, z = (cz0 + k + 0.5) * patch;
                    double d2 = (x - me.x) * (x - me.x) + (z - me.z) * (z - me.z);
                    if (d2 > reach * reach)
                        continue;
                    var here = new WorldXZ(x, z);
                    if (!SimStage.OnStage(here))
                        continue;                                    // (not in this level's scene)
                    float h = SimStage.LandAboveWater(here);
                    if (h > -0.3f || h < -20f)
                        continue;                                    // land, or deep open water
                    if (!NearLand(x, z, 1500.0))
                        continue;                                    // inshore only
                    var (region, perTen) = Fishing(x, z, month);
                    // boats out per 10 km of shore: a patch of near-shore water has about half a kilometre of shore
                    double expected = perTen / 10.0 * (patch / 1000.0) * out_;
                    var rng = SimRandom.For(day, "local." + (cx0 + i) + "." + (cz0 + k));
                    int count = (int)Math.Floor(expected + rng.Next());
                    for (int b = 0; b < count; b++)
                    {
                        double bx = x + (rng.Next() - 0.5) * patch * 0.8, bz = z + (rng.Next() - 0.5) * patch * 0.8;
                        if (SimStage.LandAboveWater(new WorldXZ(bx, bz)) > -0.3f)
                            continue;
                        float length = Length(region, ref rng);
                        wanted.Add((bx, bz, (float)(rng.Next() * 360.0), length, (float)(rng.Next() * 6.28), (bx - me.x) * (bx - me.x) + (bz - me.z) * (bz - me.z)));
                    }
                }
            wanted.Sort((a, b) => a.d2.CompareTo(b.d2));
            Show();
        }

        void Show()
        {
            int used = 0;
            foreach (var w in wanted)
            {
                if (used >= maxBoats)
                    break;
                if (used >= boats.Count)
                    boats.Add(new Boat { go = ShipBodies.Make(prefab, transform, "Local boat") });
                var b = boats[used++];
                b.x = w.x;
                b.z = w.z;
                b.heading = w.heading;
                b.phase = w.phase;
                if (Math.Abs(b.length - w.length) > 0.01f)
                {
                    b.length = w.length;
                    b.go.transform.localScale = Vector3.one * (w.length / modelLength);
                    ShipBodies.SetRig(b.go, w.length > 7.5f ? ShipBodies.Rig.Furled : ShipBodies.Rig.NoMast);
                }
                b.go.SetActive(true);
            }
            for (int i = used; i < boats.Count; i++)
                boats[i].go.SetActive(false);
        }

        static bool NearLand(double x, double z, double within)
        {
            for (int a = 0; a < 8; a++)
            {
                double ang = a * Math.PI / 4.0;
                if (SimStage.LandAboveWater(new WorldXZ(x + Math.Cos(ang) * within, z + Math.Sin(ang) * within)) > 0f)
                    return true;
            }
            return false;
        }

        /// <summary>The fishing region (the nearest port's) and its boats out per 10 km of shore this month.</summary>
        (string region, float perTen) Fishing(double x, double z, int month)
        {
            string region = null;
            double best = double.MaxValue;
            foreach (var p in GameData.Ports)
            {
                var (px, pz) = WorldFrame.ToWorld(p.lat, p.lon);
                double d = (px - x) * (px - x) + (pz - z) * (pz - z);
                if (d < best)
                {
                    best = d;
                    region = p.region;
                }
            }
            if (region != null && fishing.TryGetValue(region, out var f))
                return (region, f.months[Mathf.Clamp(month, 1, 12) - 1]);
            return (region, 3f);
        }

        float Length(string region, ref SimRandom rng)
        {
            var game = KapmanGame.Current;
            var classes = region != null && fishing.TryGetValue(region, out var f) && classSets.TryGetValue(f.classes, out var set) ? set : null;
            var data = game.World != null ? game.World.data : null;
            if (classes != null && data != null)
            {
                double r = rng.Next(), sum = 0.0;
                foreach (var kv in classes)
                    sum += kv.Value;
                r *= sum;
                foreach (var kv in classes)
                {
                    r -= kv.Value;
                    if (r <= 0.0 && data.classes.TryGetValue(kv.Key, out var c))
                        return c.length;
                }
            }
            return (float)rng.Range(5.0, 8.0);
        }

        void Load(KapmanGame game)
        {
            if (fishing != null)
                return;
            fishing = new Dictionary<string, (float[], string)>();
            classSets = new Dictionary<string, Dictionary<string, float>>();
            if (game.traffic == null)
                return;
            var f = Json.Obj(Json.Parse(game.traffic.text), "fishing");
            foreach (var kv in Json.Obj(f, "regions") ?? new Dictionary<string, object>())
            {
                var m = Json.Arr(kv.Value, "out_fair_day");
                if (m != null && m.Count == 12)
                    fishing[kv.Key] = (m.ConvertAll(v => v is double d ? (float)d : 0f).ToArray(), Json.Str(kv.Value, "classes"));
            }
            foreach (var kv in Json.Obj(f, "class_sets") ?? new Dictionary<string, object>())
            {
                var set = new Dictionary<string, float>();
                foreach (var c in (kv.Value as Dictionary<string, object>) ?? new Dictionary<string, object>())
                    if (c.Value is double share)
                        set[c.Key] = (float)share;
                classSets[kv.Key] = set;
            }
        }
    }
}
