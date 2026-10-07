using System.Collections.Generic;

namespace Kapman.Game.Sim
{
    /// <summary>InPort: at a landing, dealing. Waiting: loaded and ready, waiting in port for a fair wind. Sailing: on her
    /// passage. Anchored: arrived, but the landing is full - anchored off (or moored along the bank) for her turn.</summary>
    public enum ShipState { InPort, Sailing, LaidUp, Lost, Waiting, Anchored }

    /// <summary>A point of the world, in real metres (the bakes' and the map's frame).</summary>
    public struct WorldXZ
    {
        public double x, z;
        public WorldXZ(double x, double z) { this.x = x; this.z = z; }
        public static double Distance(WorldXZ a, WorldXZ b) => System.Math.Sqrt((a.x - b.x) * (a.x - b.x) + (a.z - b.z) * (a.z - b.z));
    }

    /// <summary>
    /// A SHIP OF THE WORLD, simulated: her class, home and crew; her purse and hold (the same kinds as the player's, so she
    /// deals with markets the same way); what she knows of prices; where she is and where she's bound. Plain data, so a
    /// run can be saved and replayed from its seed.
    /// </summary>
    public sealed class SimShip
    {
        public int index;
        /// <summary>"dorestad.3": her home port and her number there (also the seed of her dice).</summary>
        public string id;
        public ShipClassDef cls;
        public int home;
        public string homeRegion;

        public ShipState state = ShipState.InPort;
        /// <summary>The port she's at (in port or laid up), else the one she left.</summary>
        public int at;
        /// <summary>The port she's bound for (-1: none chosen yet).</summary>
        public int dest = -1;
        /// <summary>The game day she next acts in port (dealing, then sailing).</summary>
        public double readyDay;

        public Purse purse;
        public Hold hold;
        public readonly Knowledge knows = new Knowledge();
        public SimRandom rng;

        // the passage
        public readonly List<WorldXZ> path = new List<WorldXZ>();
        /// <summary>The kind of water of each leg (LaneGraph.Sea ... Approach): how far to her side she keeps.</summary>
        public readonly List<byte> kinds = new List<byte>();
        public int leg;                                            // she's between path[leg] and path[leg + 1]
        public double along;                                       // real metres along the leg
        public double done, total;                                 // real metres along the whole passage, and its length
        public WorldXZ pos;                                        // where she is: the lane's line, plus her side of it
        public WorldXZ prevPos;                                    // where she was an hour ago (the map moves her smoothly between)
        public double heading;                                     // degrees, 0 = north, 90 = east
        public double passageKm, departedDay;

        // seamanship (Seamanship.cs)
        /// <summary>Real metres she goes this hour (planned, then cut by giving way).</summary>
        public double go;
        /// <summary>Real metres she stands out from her side of the lane to keep clear (+ starboard, - port); held while
        /// she's passing (holdOut hours), then fades.</summary>
        public double standOut;
        public int holdOut;
        /// <summary>Lying up for the night (a coasting craft), or hove to: sail down.</summary>
        public bool lyingUp;
        /// <summary>Hove to for the merchant's hail (Encounter): she waits where she is.</summary>
        public bool heaveTo;

        // raiders (RaiderBrain)
        /// <summary>A band of raiders (longships), not a trader.</summary>
        public bool raider;
        /// <summary>Raiders: their people (raiders_793.json origins), how many ships and men, the season they're out for,
        /// how many places struck this season.</summary>
        public string origin;
        public int bandShips = 1, men, seasonYear, raidsThisSeason;
        /// <summary>Raiders chasing the merchant (RaiderEncounter moves them; the sim leaves them be).</summary>
        public bool chasing;
        /// <summary>The day she was last taken by raiders (they leave her be for a while after).</summary>
        public double lastTakenDay = double.MinValue;

        /// <summary>The merchant's own ship, as the world's ships see her (WorldSim.player): she stands on, they keep clear
        /// (she follows her charted line; the user's rule: no steering).</summary>
        public bool isPlayer;
        /// <summary>When she began waiting for a fair wind (game day), or anchored off for a berth (game hours), and her
        /// place in the anchorage.</summary>
        public double waitSince, anchoredSince;
        /// <summary>The day she lay up for want of silver to pay her crew (-1: she isn't broke).</summary>
        public double brokeSince = -1.0;
        /// <summary>Waiting for a fair wind (not only for the way out to clear).</summary>
        public bool windBound;
        public int anchorSlot = -1;

        // her year, for the statistics
        public int voyages;
        public bool voyageOpen;                                    // sailed and not yet sold out at the other end
        public double startWorth, voyageCost, voyageRevenue, profit, wages, owedWages;
        public string cargoNote = "";

        public bool Out => state == ShipState.Sailing;
    }
}
