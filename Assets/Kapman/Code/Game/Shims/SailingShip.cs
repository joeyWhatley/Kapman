// ported 4 Oct: a SHIM for the first build's Kapman.Sailing.SailingShip (Code/Sailing/SailingShip.cs, 533 lines of sailing
// physics on a 1:32 sea). The rebuild's ship is Kapman.Levels.ShipMotion (she rides HDRP's water and takes one order: a
// point to make for). The game layer (Aground, TideWait, VoyagePlanner, SaveGame, the map, the ports) still names
// SailingShip and reads these members, so this wraps a ShipMotion on the same GameObject with the old names. The members
// are exactly those the game layer calls; the rest of the old ship (sail trim, heel, beaching) is not here.
using Kapman.Levels;
using Kapman.Levels.Water;
using UnityEngine;

namespace Kapman.Land
{
    /// <summary>What the ground is where a keel meets it (the first build's Kapman.Land.Ground; Aground names it in its words).</summary>
    public enum Ground
    {
        Water,
        Sand,
        Mud,
        Shingle,
        Earth,
        Rock,
        Snow,
    }
}

namespace Kapman.Sailing
{
    /// <summary>
    /// SHIP CLASS (shim) - the facts of one kind of ship the game layer reads: her name, her size, her hold. The first
    /// build's class also held her sailing polar (speedByWindAngle and the rest); the rebuild's ShipMotion sails by the
    /// level's shipSpeed, so only the facts are kept. One is made at runtime for the player's ship if none is assigned.
    /// </summary>
    [CreateAssetMenu(menuName = "Kapman/Ship Class", fileName = "ShipClass")]
    public class ShipClass : ScriptableObject
    {
        [Header("The ship")]
        public string shipName = "Gislinge boat";
        [TextArea] public string about = "A 7.7 m working boat for fishing and carrying goods.";
        [Tooltip("Metres, stem to stern.")] public float length = 7.7f;
        [Tooltip("Metres, widest.")] public float beam = 1.5f;
        [Tooltip("Metres of water she draws, loaded.")] public float draught = 0.45f;
        [Tooltip("Hold space: kilograms of dense cargo she carries (light, bulky goods fill her first: each good's bulk).")]
        public float cargoKg = 900f;
        [Tooltip("Knots under sail in a fair wind, and at the oars.")]
        public float maxKnots = 6.5f, rowingKnots = 3.5f;
        [Tooltip("Degrees off the wind she can sail, at best.")]
        public float closestToWind = 58f;
    }

    /// <summary>
    /// THE PLAYER'S SHIP (shim) - the first build's SailingShip API over the rebuild's ShipMotion. Put it on the ship's
    /// GameObject (KapmanGame adds it to ShipMotion.Player itself if the scene has none). Positions are the level's Unity
    /// metres; the world frame comes from FloatingOrigin (LevelFrame sets it for the level).
    /// Differences from the first build, on purpose:
    ///   - she never beaches (ShipMotion stops short of shoals): Beached and Beaching are always false;
    ///   - Aground is true only when the water where she lies falls below her draught (a falling tide, a flood going down);
    ///   - "anchored" lies her still (her order is cancelled); a click order that takes her away clears it.
    /// </summary>
    [DefaultExecutionOrder(5)]
    public class SailingShip : MonoBehaviour
    {
        [Tooltip("What she is (empty: made at runtime from the ShipMotion's length and draught).")]
        public ShipClass shipClass;
        [Tooltip("Water she wants under her keel beyond her draught (m).")]
        public float keelClearance = 0.15f;

        ShipMotion motion;
        bool anchoredNow;

        /// <summary>The rebuild's ship on this GameObject (added if missing; the first one made is the player's).</summary>
        public ShipMotion Motion
        {
            get
            {
                if (motion == null)
                {
                    motion = GetComponent<ShipMotion>();
                    if (motion == null)
                    {
                        motion = gameObject.AddComponent<ShipMotion>();
                        motion.player = ShipMotion.Player == null;
                    }
                }
                return motion;
            }
        }

        /// <summary>Metres of water she draws (the ShipMotion's).</summary>
        public float draught
        {
            get => Motion.draught;
            set => Motion.draught = value;
        }

        /// <summary>Lying still (at a berth, at anchor, while dealing): her order is cancelled. A new order away from here clears it.</summary>
        public bool anchored
        {
            get => anchoredNow;
            set
            {
                if (value && !anchoredNow)
                    Motion.MakeFor(transform.position);                       // (no way on her: the goal is where she is)
                anchoredNow = value;
            }
        }

        /// <summary>Compass heading, degrees (0 north, 90 east).</summary>
        public float Heading => Mathf.Repeat(Motion.heading, 360f);
        /// <summary>Speed through the water, m/s.</summary>
        public float Speed => Motion.Speed;
        /// <summary>Velocity over the ground (the river's stream included), m/s.</summary>
        public Vector3 GroundVelocity => Motion.Velocity;
        /// <summary>The wind's strength here, m/s (the level's weather).</summary>
        public float TrueWind => LevelWeather.Current != null ? LevelWeather.Current.Wind : 0f;
        /// <summary>Degrees off the bow the true wind comes from; + = from starboard.</summary>
        public float TrueWindAngle
        {
            get
            {
                var w = LevelWeather.Current;
                if (w == null)
                    return 0f;
                var v = w.WindVector;                                        // the way it blows to (east, north)
                float from = Mathf.Atan2(v.x, v.y) * Mathf.Rad2Deg + 180f;
                return Mathf.DeltaAngle(Heading, from);
            }
        }
        public bool Rowing => Motion.Speed > 0.05f;
        public bool InIrons => false;
        public bool Wearing => false;
        /// <summary>The water where she lies has fallen below her draught: she's fast on the bottom.</summary>
        public bool Aground { get; private set; }
        public bool Beaching => false;
        public bool Beached => false;
        public float KeelOnGround => Aground ? 1f : 0f;
        public Vector3 KeelContact => transform.position;
        /// <summary>What her keel is on: the river's or the sea's bottom (mud) when aground, else water.</summary>
        public Land.Ground GroundUnderKeel => Aground ? Land.Ground.Mud : Land.Ground.Water;
        /// <summary>Metres of water she needs: her draught and the clearance under her keel.</summary>
        public float NeedsDepth => draught + keelClearance;
        /// <summary>Her sail's fill, 0..1: under way she's taken as under sail or oars at full.</summary>
        public float SailFill => Rowing ? 1f : 0f;
        public string PointOfSail => Rowing ? "under way" : "lying still";
        public string Tack => TrueWindAngle >= 0f ? "wind on the starboard side" : "wind on the port side";

        /// <summary>
        /// Metres of water at a Unity position: the water's height there (HDRP's surface, tide and flood in it) less the
        /// ground's (the terrain, or whatever a ray from above hits: a jetty). Infinite where there is no ground at all.
        /// </summary>
        public float Depth(Vector3 at)
        {
            float ground = GroundAt(at, out bool found);
            if (!found)
                return float.PositiveInfinity;
            return WaterSampler.Height(at) - ground;
        }

        /// <summary>The ground's height at a point: the terrain under it, else the first collider a ray from 500 m up hits.</summary>
        public static float GroundAt(Vector3 at, out bool found)
        {
            found = true;
            foreach (var t in Terrain.activeTerrains)
            {
                var td = t.terrainData;
                if (td == null)
                    continue;
                var p = t.transform.position;
                if (at.x < p.x || at.z < p.z || at.x > p.x + td.size.x || at.z > p.z + td.size.z)
                    continue;
                return t.SampleHeight(at) + p.y;
            }
            if (Physics.Raycast(new Vector3(at.x, 500f, at.z), Vector3.down, out var hit, 1000f, ~0, QueryTriggerInteraction.Ignore))
                return hit.point.y;
            found = false;
            return 0f;
        }

        /// <summary>Puts her somewhere at once (a saved game loaded, pushed off the ground): there, heading so, no way on.</summary>
        public void Teleport(Vector3 position, float heading)
        {
            heading = Mathf.Repeat(heading, 360f);
            transform.SetPositionAndRotation(position, Quaternion.Euler(0f, heading, 0f));
            Motion.heading = heading;
            Motion.MakeFor(position);                                         // (the goal is where she is: she stops)
        }

        void OnEnable()
        {
            var m = Motion;
            if (shipClass == null)
            {
                shipClass = ScriptableObject.CreateInstance<ShipClass>();
                shipClass.name = "Gislinge boat";
                shipClass.length = m.length;
                shipClass.beam = m.beam;
                shipClass.draught = m.draught;
            }
        }

        void LateUpdate()
        {
            float d = Depth(transform.position);
            Aground = !float.IsPositiveInfinity(d) && d < draught - 0.02f;
            // ordered away from where she lay: she's not anchored any more
            if (anchoredNow && Motion.Goal.HasValue)
            {
                var g = Motion.Goal.Value;
                g.y = transform.position.y;
                if ((g - transform.position).sqrMagnitude > 36f)
                    anchoredNow = false;
            }
        }
    }
}
