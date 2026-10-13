using System.Collections;
using System.Collections.Generic;
using Kapman.Levels;
using Kapman.Levels.Water;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// PORT - one trading place in the world: its blockout (simple shapes for now, yours to replace) and the berth a
    /// ship lies to. The ports' logic (arriving, the market, leaving) is in the graphs (Graphs/Ports); this only knows
    /// where things are.
    ///   - With <see cref="autoPlace"/> on it finds its own shore at the start: it turns its seaward side to the
    ///     water, moves its berth out to water deep enough for a ship, and sets its buildings on the ground and its
    ///     jetties on the water. Turn it off once you've placed a port by hand.
    ///   - Its children: "Berth" (where a ship lies to), and the blockout parts. Parts named "Jetty..." sit on the
    ///     water; everything else on the ground.
    ///
    /// ported 4 Oct: the 3D side of this is the only part of the ports' code that touched the first build's world
    /// (LandStreamer for the ground, Tide for the sea's level, CompassMath for a heading). In the rebuild it reads the
    /// ground from the scene's terrains (Kapman.Levels.Water.TerrainHeights) and the water's level from the level
    /// (LevelInfo.WaterLevel, which already holds the tide), both behind the two small helpers below. Everything else
    /// (the berth, the arrival radius, Nearest) is as it was.
    /// </summary>
    public class Port : MonoBehaviour
    {
        [Tooltip("The port's id in Data/ports_793.json.")]
        public string id;
        [Tooltip("Where a ship lies to (a child named Berth).")]
        public Transform berth;
        [Tooltip("Game metres from the berth within which a ship has arrived.")]
        public float arrivalRadius = 30f;
        // ported 4 Oct: off by default in the rebuild. The first build's ports were blockouts dropped on streamed land, so
        // each found its own shore; the rebuild's harbours are built by hand (Dorestad by the editor tools), and
        // auto-placing one would move the hand-set town and its parts. Turn it on for a quick blockout on a new level.
        [Tooltip("Find the shore, face the water and settle the parts at the start (off once placed by hand).")]
        public bool autoPlace = false;
        [Tooltip("Real metres of water a ship needs at the berth.")]
        public float berthDepth = 1.2f;

        /// <summary>Every port in the world now.</summary>
        public static readonly List<Port> All = new List<Port>();

        /// <summary>True when the port stood in the scene before play (Kapman > Make physical > 4) and was taken as saved.</summary>
        [System.NonSerialized] public bool FromScene;

        public PortDef Def => GameData.Port(id);
        public string Title => Def != null ? Def.Title : id;
        public bool Placed { get; private set; }

        void OnEnable() => All.Add(this);

        void OnDisable() => All.Remove(this);

        IEnumerator Start()
        {
            if (!autoPlace)
            {
                Placed = true;
                yield break;
            }
            // ported 4 Oct: the first build waited for the land streamer's tiles; the rebuild's terrains are in the scene,
            // so one frame is enough for them to be active. No terrain at all: nothing to find the shore on, so say so.
            yield return null;
            var land = TerrainHeights.FromScene();
            if (land.Terrains == 0)
            {
                Debug.LogWarning($"Port {id}: autoPlace is on but the scene has no terrain to find the shore on; left where it is.", this);
                Placed = true;
                yield break;
            }
            PlaceOnShore(land);
            Placed = true;
        }

        /// <summary>Game metres from the berth to a Unity position (across the ground; height ignored).</summary>
        public float DistanceTo(Vector3 position)
        {
            var b = berth != null ? berth.position : transform.position;
            b.y = position.y = 0f;
            return Vector3.Distance(b, position);
        }

        /// <summary>The port nearest a Unity position (null if there are none).</summary>
        public static Port Nearest(Vector3 position)
        {
            Port best = null;
            float bestD = float.MaxValue;
            foreach (var p in All)
            {
                float d = p.DistanceTo(position);
                if (d < bestD)
                {
                    bestD = d;
                    best = p;
                }
            }
            return best;
        }

        /// <summary>The id of the port the merchant's ship lies at (anchored, inside its reach), or null at sea.</summary>
        public static string LyingId()
        {
            var game = KapmanGame.Current;
            if (game == null || game.ship == null || !game.ship.anchored)
                return null;
            var p = Nearest(game.ShipPosition);
            return p != null && p.DistanceTo(game.ShipPosition) < p.arrivalRadius ? p.id : null;
        }

        // ------------------------------------------------------------------------------------------ finding the shore

        // ported 4 Oct: was LandStreamer.GroundHeight(at); the terrains' height at a point, in Unity metres
        float Ground(ILandHeights land, Vector3 at) => land.HeightAt(at.x, at.z);

        // ported 4 Oct: was Tide.Level / FloatingOrigin.VerticalScale; the level's water height (Unity metres) already
        // holds the tide, the season and any flood
        float Sea => LevelInfo.Current != null ? LevelInfo.Current.WaterLevel : 0f;

        // ported 4 Oct: was Kapman.Sailing.CompassMath.FromVector (a compass heading in degrees, 0 north, 90 east)
        static float HeadingOf(Vector3 v) => Mathf.Repeat(Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg, 360f);

        void PlaceOnShore(ILandHeights land)
        {
            // which way is the water: of 32 directions, the one with the most water within a few hundred game metres
            Vector3 centre = transform.position;
            Vector3 seaward = Vector3.forward;
            float most = -1f;
            for (int i = 0; i < 32; i++)
            {
                float a = i * Mathf.PI * 2f / 32f;
                var dir = new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a));
                float wet = 0f;
                for (int k = 1; k <= 12; k++)
                    if (Ground(land, centre + dir * (k * 12f)) < Sea)
                        wet += 1f / k;                                   // (nearer water counts more)
                if (wet > most)
                {
                    most = wet;
                    seaward = dir;
                }
            }
            // the shore: walk seaward from the port till the ground is under the sea (or back, if we started in it)
            Vector3 shore = centre;
            bool startWet = Ground(land, centre) < Sea;
            for (int k = 0; k < 400; k++)
            {
                var p = centre + seaward * (startWet ? -k : k) * 0.5f;
                bool wet = Ground(land, p) < Sea;
                if (wet != startWet)
                {
                    shore = p;
                    break;
                }
            }
            transform.SetPositionAndRotation(new Vector3(shore.x, 0f, shore.z), Quaternion.LookRotation(seaward, Vector3.up));
            var anchor = GetComponent<WorldAnchor>();
            if (anchor != null)
                anchor.position = FloatingOrigin.ToWorld(transform.position);

            // the berth: out from the shore to water deep enough for a ship (within reach of the jetties' ends)
            if (berth != null)
            {
                float need = -berthDepth / (float)FloatingOrigin.VerticalScale + Sea;
                var at = shore + seaward * 8f;
                for (int k = 0; k < 200 && Ground(land, at) > need; k++)
                    at += seaward * 1f;
                berth.position = new Vector3(at.x, Sea, at.z);
                // the player's ship lying here at the start (the scene's origin is the town's spot, on the mud): on the
                // berth, lying along the shore (the user, 2026-10-03: "the ship is stuck at the port in dorestad")
                var game = KapmanGame.Current;
                if (game != null && game.ship != null && game.ship.anchored && DistanceTo(game.ship.transform.position) < arrivalRadius * 2f)
                {
                    var along = Quaternion.Euler(0f, 90f, 0f) * seaward;
                    game.ship.Teleport(new Vector3(berth.position.x, game.ship.transform.position.y, berth.position.z), HeadingOf(along));
                }
            }

            // the parts: on the ground, jetties on the water
            foreach (Transform part in transform)
            {
                if (part == berth)
                    continue;
                var p = part.position;
                float half = part.lossyScale.y * 0.5f;
                bool onWater = part.name.StartsWith("Jetty");
                float ground = Ground(land, p);
                p.y = onWater ? Mathf.Max(ground, Sea) + 0.25f : Mathf.Max(ground, Sea) + half - 0.05f;
                part.position = p;
            }
        }

        void OnDrawGizmosSelected()
        {
            if (berth == null)
                return;
            Gizmos.color = new Color(0.9f, 0.7f, 0.3f, 0.8f);
            Gizmos.DrawWireSphere(berth.position, arrivalRadius);
        }

        /// <summary>In the Scene view every port shows as a small mark with its id, selected or not (14 Oct 2026: the
        /// ports stand in the scene, to be seen and moved by hand).</summary>
        void OnDrawGizmos()
        {
            var at = berth != null ? berth.position : transform.position;
            Gizmos.color = new Color(0.9f, 0.7f, 0.3f, 0.9f);
            Gizmos.DrawSphere(at, 8f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(at + Vector3.up * 12f, id);
#endif
        }
    }
}
