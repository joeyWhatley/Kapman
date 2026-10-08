using Kapman.Sailing;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// THE CHART AS THE GAME (the user, 4 Oct 2026: "build the game via the 2D map aspect"; picker: "the chart is the
    /// game") - the scene with no 3D level in it: the whole world in the world frame, the merchant's ship a mark that
    /// sails her charted course on the game's clock (ChartBody), every trading place of the tables a port she can lie at
    /// (made here at the start, each with its berth where the tables put it), and the map open all the time (MapView's
    /// chart mode). Everything else is the game as it stands: the ports' graphs moor her and open the port's menu, the
    /// market docks beside the map, the world's ships sail and meet her, the chronicle comes on its days.
    ///
    /// The world's scale here is the clock's rate (60): see ChartBody. Unity's origin is the world frame's (56 N 5 E); the
    /// sailing world lies within 30 game kilometres of it, so nothing has to shift.
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class ChartWorld : MonoBehaviour
    {
        [Tooltip("Where she lies at the start (a port's id).")]
        public string homePort = "dorestad";
        [Tooltip("Game metres round a berth within which a ship has arrived (each port's reach).")]
        public float arrivalRadius = 30f;

        public static ChartWorld Current { get; private set; }

        void Awake()
        {
            Current = this;
        }

        void OnDestroy()
        {
            if (Current == this)
                Current = null;
        }

        void Start()
        {
            // the trading places: one Port each, where the tables put it (KapmanGame's Awake has read them)
            var root = new GameObject("--- PORTS (the chart's) ---").transform;
            int made = 0;
            foreach (var def in GameData.Ports)
            {
                if (def == null || string.IsNullOrEmpty(def.id))
                    continue;
                var (x, z) = WorldFrame.ToWorld(def.lat, def.lon);
                var go = new GameObject("Port " + def.id);
                go.transform.SetParent(root, false);
                go.transform.position = FloatingOrigin.ToUnity(new WorldPos(x, 0.0, z));
                var berth = new GameObject("Berth").transform;
                berth.SetParent(go.transform, false);
                var port = go.AddComponent<Port>();
                port.id = def.id;
                port.berth = berth;
                port.arrivalRadius = arrivalRadius;
                port.autoPlace = false;
                made++;
            }
            // the ship: lying at home
            var game = KapmanGame.Current;
            var home = GameData.Port(homePort);
            if (!SaveLoaded)
                Landings.Clear();                                         // (a new game: where the last one lay is forgotten)
            if (game != null && game.ship != null && home != null && !SaveLoaded)
            {
                var (x, z) = WorldFrame.ToWorld(home.lat, home.lon);
                game.ship.Teleport(FloatingOrigin.ToUnity(new WorldPos(x, 0.0, z)), 270f);
            }
            // a new game starts with something aboard: bread and ale for the four men, and a first cargo to carry
            // (she used to start empty with no food at all). A saved game keeps what it had.
            // The four casks came with the boat (ruling 14 of 7 Oct 2026: marked), put aboard by the man who lent the
            // silver for her. They are the merchant's to sell and are owed for with the boat ("They are on the stick
            // too"), so what they fetch is never told as gain (8 Oct 2026: nor any more as "his stake").
            if (!SaveLoaded && Trade.Hold != null && Voyage.FoodDays < 0.5)
                // (no quernstones since 7 Oct 2026: with them she lay 864 of 900 kg full and could not take one cask more)
                foreach (var (id, n) in new[] { ("rhenish_wine", 4), ("ships_bread", 1), ("ale", 1) })
                {
                    var g = GameData.Good(id);
                    if (g == null)
                        continue;
                    int take = System.Math.Min(n, Trade.Hold.RoomFor(g));
                    if (take > 0)
                        Trade.Hold.Add(g, take, 0.0, game != null ? game.Day : 0.0, id == CountsGood ? CountsOwn : "");
                }
            Debug.Log($"Kapman chart: {made} ports in the world frame at 1:{FloatingOrigin.HorizontalScale:0}; she lies at {(home != null ? home.name : homePort)}.");
        }

        /// <summary>The good the creditor puts aboard at the start, and the mark its lots carry in the hold (Lot.whose).</summary>
        public const string CountsGood = "rhenish_wine", CountsOwn = "count";

        /// <summary>A saved game put her somewhere already (the save's loader sets this before Start).</summary>
        public static bool SaveLoaded;

        /// <summary>
        /// A new game's people start at their own port (the user, 7 Oct 2026: "depending on your culture you start
        /// somewhere else"). She is laid at that port's berth as she was laid at the first one: made fast, no course, the
        /// call there in progress and quiet (nobody comes down to a boat that has not arrived), the port's menu open.
        /// Nothing else of the start is moved yet: the boat, the cargo and the debt are the same at every home.
        /// False when the game has no such port; true when she lies there (already, or now).
        /// </summary>
        public bool StartAt(string portId)
        {
            var home = GameData.Port(portId);
            var game = KapmanGame.Current;
            if (home == null || game == null || game.ship == null)
                return false;
            if (Port.LyingId() == portId)
            {
                homePort = portId;
                return true;
            }
            homePort = portId;
            Course.Clear();
            var (x, z) = WorldFrame.ToWorld(home.lat, home.lon);
            game.ship.Teleport(FloatingOrigin.ToUnity(new WorldPos(x, 0.0, z)), 270f);
            game.ship.anchored = true;
            riverX = double.NaN;
            // (her own home is what she knows, and the port nearest it: not Dorestat's, which a Frisian's game would
            // have lifted the fog round before the people were chosen)
            KapmanGame.KnowStart(home);
            Known.HearOf(home);
            var cards = EncounterRunner.Director;
            if (cards != null)
            {
                cards.EndCall();
                cards.BeginCallQuietly(portId);
            }
            // (the menu of the port she lay at is given up, not only hidden: its flow went on waiting behind the new
            // port's menu, and every choice there was then taken twice)
            TextPanel.AbandonWaits();
            SaveGame.LaidAt(portId);                                      // (the ports' graph: made fast here, its menu open)
            Debug.Log($"Kapman chart: a new game's people start at {home.name}: she lies there.");
            return true;
        }

        // ------------------------------------------------------------------ where she is, for the cards and the frame

        /// <summary>Where the merchant's ship is, real metres in the world frame.</summary>
        public static Kapman.Game.Sim.WorldXZ ShipWorld()
        {
            var game = KapmanGame.Current;
            var w = FloatingOrigin.ToWorld(game != null ? game.ShipPosition : Vector3.zero);
            return new Kapman.Game.Sim.WorldXZ(w.x, w.z);
        }

        /// <summary>The weather where she is now, from the model the world's ships sail by (WorldSim.WeatherAt: the chart
        /// has no level, so no LevelWeather). False before the world is made.</summary>
        public static bool TryWeather(out Kapman.Weather.WeatherSample sample)
        {
            sample = default;
            var game = KapmanGame.Current;
            if (game == null || game.World == null || game.ship == null)
                return false;
            sample = game.World.WeatherAt(ShipWorld());
            return true;
        }

        static double riverX = double.NaN, riverZ;
        static bool riverNow;

        /// <summary>
        /// Is she on a river (else the sea)? Under way on a charted course it is the leg she is sailing; lying still it is
        /// the water round her: the nearest point of the lanes and what runs from it within two legs (a river if a river
        /// lane does and no sea, coast or sound lane). Looked up again only when she has moved a kilometre. No lanes: the sea.
        /// </summary>
        public static bool OnRiver()
        {
            if (Course.IsSet && Course.Points.Count > 1)
                return Course.LegIsRiver(Mathf.Min(Course.Next, Course.Points.Count - 1));
            var at = ShipWorld();
            if (!double.IsNaN(riverX) && Kapman.Game.Sim.WorldXZ.Distance(at, new Kapman.Game.Sim.WorldXZ(riverX, riverZ)) < 1000.0)
                return riverNow;
            riverX = at.x;
            riverZ = at.z;
            riverNow = false;
            var g = Kapman.Game.Sim.SimRoutes.Graph(KapmanGame.Current != null ? KapmanGame.Current.lanesFile : Kapman.Game.Sim.SimRoutes.DefaultLanes);
            if (g == null)
                return false;
            int node = g.Nearest(at.x, at.z);
            if (node < 0)
                return false;
            int river = 0, open = 0;
            for (int k = g.adjStart[node]; k < g.adjStart[node + 1]; k++)
            {
                int e = g.adjEdge[k];
                Count(g.edgeKind[e], ref river, ref open);
                int other = g.Other(e, node);
                for (int j = g.adjStart[other]; j < g.adjStart[other + 1]; j++)
                    Count(g.edgeKind[g.adjEdge[j]], ref river, ref open);
            }
            riverNow = river > 0 && open == 0;
            return riverNow;
        }

        static void Count(byte kind, ref int river, ref int open)
        {
            if (kind == LaneGraph.River || kind == LaneGraph.Pending)
                river++;
            else if (kind == LaneGraph.Sea || kind == LaneGraph.Coastal || kind == LaneGraph.Sound)
                open++;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            Current = null;
            SaveLoaded = false;
            riverX = double.NaN;
        }
    }
}
