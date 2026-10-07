using System.Collections.Generic;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game.Sim
{
    /// <summary>
    /// HARBOUR LIFE - the port you're at is busy as it was: its traders and local boats moored along the jetties, sails
    /// taken in, rafted two abreast where it's crowded, and boats drawn up on the beach where there's only a landing.
    /// How many: the traffic table's boats afloat and drawn up at that port in this month (Data/traffic_793.json:
    /// harbours; Dorestad in summer holds ~75 afloat), as many as the jetties hold at true size. The hulls are the
    /// player's boat, scaled: sea-going traders large, local boats small, the smallest without a mast. They're read from
    /// the port's own blockout ("Jetty..." parts), so your own jetties get boats too. Only near the port; rebuilt when you
    /// come to another or the month turns.
    /// Ported 4 Oct: the land and the water under a spot come from the stage (SimStage: the level's terrains and water),
    /// not from LandStreamer and WaterSource.
    /// </summary>
    public class HarbourLife : MonoBehaviour
    {
        public GameObject prefab;
        [Tooltip("Game metres from a port's berth within which its harbour is shown.")]
        public float showWithin = 500f;
        [Tooltip("The most boats shown at one port.")]
        public int maxBoats = 28;
        [Tooltip("Metres of the Gislinge model's length (its scale 1).")]
        public float modelLength = 7.7f;

        readonly List<GameObject> pool = new List<GameObject>();
        readonly List<(Vector3 at, Vector3 along, float length, float beam)> taken = new List<(Vector3, Vector3, float, float)>();
        int used;
        Port shown;
        int shownMonth;
        Dictionary<string, (float[] present, float[] afloat, float sea, float local)> harbours;

        // they're placed once in Unity positions: when the floating origin moves the world, they move with it
        void OnEnable() => FloatingOrigin.Shifted += MoveWithTheWorld;
        void OnDisable() => FloatingOrigin.Shifted -= MoveWithTheWorld;

        void MoveWithTheWorld()
        {
            for (int i = 0; i < used; i++)
                pool[i].transform.position += FloatingOrigin.LastShift;
        }

        void LateUpdate()
        {
            var game = KapmanGame.Current;
            if (game == null || game.ship == null || prefab == null)
                return;
            var port = Port.Nearest(game.ShipPosition);
            bool near = port != null && port.Placed && port.DistanceTo(game.ShipPosition) < showWithin;
            if (!near)
            {
                if (shown != null)
                    Clear();
                return;
            }
            if (port != shown || game.Month != shownMonth)
                Build(port, game.Month);
        }

        void Clear()
        {
            foreach (var b in pool)
                b.SetActive(false);
            used = 0;
            shown = null;
        }

        void Build(Port port, int month)
        {
            Clear();
            shown = port;
            shownMonth = month;
            taken.Clear();
            if (port.berth != null)                                   // the player's berth stays clear
                taken.Add((port.berth.position, port.berth.forward, 9f, 4f));
            Load();
            var def = port.Def;
            float afloat = 6f, drawnUp = 3f, seaShare = 0.4f;
            if (def != null && harbours.TryGetValue(def.id, out var h))
            {
                afloat = h.afloat[month - 1];
                drawnUp = Mathf.Max(0f, h.present[month - 1] - afloat);
                seaShare = Mathf.Clamp01(h.sea / Mathf.Max(1f, h.sea + h.local));
            }
            var rng = SimRandom.For(793, "harbour." + (def != null ? def.id : port.name) + "." + month);
            foreach (var jetty in port.GetComponentsInChildren<Transform>())
            {
                if (!jetty.name.StartsWith("Jetty") || !jetty.TryGetComponent<Renderer>(out var r))
                    continue;
                if (jetty.name.Contains("beach"))
                    DrawUp(jetty, r, Mathf.Min(drawnUp, maxBoats - used), ref rng);
                else
                    Moor(jetty, r, Mathf.Min(afloat, maxBoats - used), seaShare, ref rng);
            }
        }

        /// <summary>Boats along both sides of a jetty, bow out, two abreast where there are many.</summary>
        void Moor(Transform jetty, Renderer r, float want, float seaShare, ref SimRandom rng)
        {
            Vector3 along = jetty.forward, side = jetty.right;
            float length = jetty.lossyScale.z, half = jetty.lossyScale.x * 0.5f;
            Vector3 root = jetty.position - along * (length * 0.5f);
            int placed = 0;
            for (int row = 0; row < 2 && placed < want; row++)
                for (int s = -1; s <= 1 && placed < want; s += 2)
                {
                    float d = length * 0.25f;
                    while (d < length * 1.15f && placed < want && used < maxBoats)
                    {
                        bool sea = rng.Chance(seaShare);
                        float boatLength = sea ? 14f : rng.Range(5, 9);
                        float scale = boatLength / modelLength, beam = 1.5f * scale;
                        Vector3 at = root + along * (d + boatLength * 0.5f) + side * s * (half + beam * 0.5f + 0.25f + row * (beam + 0.2f));
                        d += boatLength + 0.6f;
                        if (Overlaps(at, along, boatLength, beam) || !Water(at, out float y))
                            continue;
                        taken.Add((at, along, boatLength, beam));
                        at.y = y;
                        var b = Next();
                        b.transform.SetPositionAndRotation(at, Quaternion.LookRotation(along, Vector3.up));
                        b.transform.localScale = Vector3.one * scale;
                        ShipBodies.SetRig(b, sea || boatLength > 7f ? ShipBodies.Rig.Furled : ShipBodies.Rig.NoMast);
                        placed++;
                    }
                }
        }

        /// <summary>Boats drawn up on the beach beside a landing, bows up the shore.</summary>
        void DrawUp(Transform landing, Renderer r, float want, ref SimRandom rng)
        {
            Vector3 up = landing.forward, side = landing.right;
            for (int i = 0; i < Mathf.CeilToInt(want) && used < maxBoats; i++)
            {
                float scale = rng.Range(5, 10) / modelLength;
                int k = (i / 2 + 1) * (i % 2 == 0 ? 1 : -1);             // either side of the landing, outward
                Vector3 at = landing.position + side * (k * (1.5f * scale + 0.6f) + Mathf.Sign(k) * landing.lossyScale.x * 0.5f) - up * 1.5f;
                // ported 4 Oct: on the ground there (the stage's land above the water, plus the water's level)
                float above = SimStage.LandAboveWater(SimStage.ToWorld(at));
                if (!float.IsNaN(above))
                    at.y = SimStage.WaterHeight(at) + above + 0.05f;
                var b = Next();
                b.transform.SetPositionAndRotation(at, Quaternion.LookRotation(-up, Vector3.up) * Quaternion.Euler(-4f, 0f, (float)rng.Range(-6.0, 6.0)));
                b.transform.localScale = Vector3.one * scale;
                ShipBodies.SetRig(b, ShipBodies.Rig.NoMast);
            }
        }

        /// <summary>Whether a boat here would touch one already moored (or the player's berth): their footprints, as
        /// boxes along their headings, measured in the first one's frame (with a little room between).</summary>
        bool Overlaps(Vector3 at, Vector3 along, float length, float beam)
        {
            foreach (var t in taken)
            {
                Vector3 d = at - t.at;
                d.y = 0f;
                Vector3 fwd = t.along;
                fwd.y = 0f;
                fwd.Normalize();
                Vector3 right = Vector3.Cross(Vector3.up, fwd);
                float alongGap = Mathf.Abs(Vector3.Dot(d, fwd)), sideGap = Mathf.Abs(Vector3.Dot(d, right));
                if (alongGap < (length + t.length) * 0.5f + 0.4f && sideGap < (beam + t.beam) * 0.5f + 0.3f)
                    return true;
            }
            return false;
        }

        /// <summary>Whether a spot is water deep enough for a boat (and the water's height there).</summary>
        static bool Water(Vector3 at, out float y)
        {
            float above = SimStage.LandAboveWater(SimStage.ToWorld(at));
            if (!float.IsNaN(above) && above > -0.3f)
            {
                y = 0f;
                return false;
            }
            y = SimStage.WaterHeight(at);
            return true;
        }

        GameObject Next()
        {
            if (used >= pool.Count)
                pool.Add(ShipBodies.Make(prefab, transform, "Harbour boat"));
            var b = pool[used++];
            b.SetActive(true);
            return b;
        }

        void Load()
        {
            if (harbours != null)
                return;
            harbours = new Dictionary<string, (float[], float[], float, float)>();
            var game = KapmanGame.Current;
            if (game == null || game.traffic == null)
                return;
            var root = Json.Parse(game.traffic.text);
            foreach (var kv in Json.Obj(Json.Obj(root, "harbours"), "ports") ?? new Dictionary<string, object>())
            {
                var present = Json.Arr(kv.Value, "present");
                var afloat = Json.Arr(kv.Value, "afloat");
                if (present == null || afloat == null || present.Count != 12 || afloat.Count != 12)
                    continue;
                harbours[kv.Key] = (present.ConvertAll(x => x is double v ? (float)v : 0f).ToArray(),
                                    afloat.ConvertAll(x => x is double v ? (float)v : 0f).ToArray(),
                                    (float)Json.Num(kv.Value, "sea"), (float)Json.Num(kv.Value, "local"));
            }
        }
    }
}
