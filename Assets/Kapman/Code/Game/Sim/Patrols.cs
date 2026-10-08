using System;
using System.Collections.Generic;
using System.Linq;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game.Sim
{
    /// <summary>A watch station of the king's men: a port and the waters round it (Data/patrols_793.json).</summary>
    public sealed class PatrolStation
    {
        public string id, port, realm;
        public double radiusKm, strictness;
    }

    /// <summary>The law's table (Data/patrols_793.json): who searches where, how hard, and what the answers cost.</summary>
    public sealed class PatrolRules
    {
        public readonly Dictionary<string, double> portSearch = new Dictionary<string, double>();
        public readonly Dictionary<string, double> zoneDefault = new Dictionary<string, double>();
        public readonly List<PatrolStation> stations = new List<PatrolStation>();
        public int guardBoatsFrom = 800;
        public bool testGuardBoatsNow;
        public double coverMax = 0.6, fineShare = 0.25, giftShare = 0.1, giftMinD = 12.0, giftTaken = 0.55, runEscape = 0.45, dumpMinKm = 2.0;
        public bool aiSearched = true;

        public static PatrolRules Load(TextAsset json)
        {
            var r = new PatrolRules();
            if (json == null)
                return r;
            var o = Json.Parse(json.text);
            foreach (var kv in Json.Obj(o, "port_searches") ?? new Dictionary<string, object>())
                if (kv.Value is double d)
                    r.portSearch[kv.Key] = d;
            foreach (var kv in Json.Obj(o, "zone_default") ?? new Dictionary<string, object>())
                if (kv.Value is double d)
                    r.zoneDefault[kv.Key] = d;
            foreach (var x in Json.Arr(o, "stations") ?? new List<object>())
                r.stations.Add(new PatrolStation
                {
                    id = Json.Str(x, "id"), port = Json.Str(x, "port"), realm = Json.Str(x, "realm"),
                    radiusKm = Json.Num(x, "radius_km", 25), strictness = Json.Num(x, "strictness", 0.3),
                });
            r.guardBoatsFrom = (int)Json.Num(o, "guard_boats_from", r.guardBoatsFrom);
            r.testGuardBoatsNow = o is Dictionary<string, object> root && root.TryGetValue("test_guard_boats_now", out var flag) && flag is bool on && on;
            var s = Json.Obj(o, "search");
            r.coverMax = Json.Num(s, "cover_max", r.coverMax);
            r.fineShare = Json.Num(s, "fine_share", r.fineShare);
            r.giftShare = Json.Num(s, "gift_share", r.giftShare);
            r.giftMinD = Json.Num(s, "gift_min_d", r.giftMinD);
            r.giftTaken = Json.Num(s, "gift_taken", r.giftTaken);
            r.runEscape = Json.Num(s, "run_escape", r.runEscape);
            r.dumpMinKm = Json.Num(s, "dump_min_km", r.dumpMinKm);
            return r;
        }

        /// <summary>The chance a ship leaving this port is searched.</summary>
        public double Strictness(PortDef p)
        {
            if (p == null)
                return 0.0;
            if (portSearch.TryGetValue(p.id, out double s))
                return s;
            var zone = TradeRules.Current?.ZoneOf(p.region);
            return zone != null && zoneDefault.TryGetValue(zone.id, out double z) ? z : 0.0;
        }
    }

    /// <summary>
    /// DEEDS - what the merchant has done that people will remember (fled the king's men, bribed an officer, paid
    /// raiders, fought): facts with where and when, for reputation to come (Plans/ai_fleets.md, phase 6).
    /// </summary>
    public static class Deeds
    {
        public static readonly List<(string when, string deed, string where)> Log = new List<(string, string, string)>();

        public static void Record(string deed, string where)
        {
            var game = KapmanGame.Current;
            Log.Add((game != null ? game.Date : "", deed, where ?? ""));
        }

        /// <summary>A deed of a kind (reputation_793.json 'deeds') done at a place: kept, and his standing there, in its
        /// region and (as the news of it travels) among its people elsewhere changes by it.</summary>
        public static void Record(string kind, string deed, PortDef at)
        {
            Record(deed, at?.name);
            Reputation.Deed(kind, at, deed);
            Self.MarkFromDeed(kind);                                     // (a deed that is known lies on a stat for a while)
        }

        /// <summary>The place nearest the merchant's ship (where a deed at sea is known first).</summary>
        public static PortDef NearestPlace()
        {
            var game = KapmanGame.Current;
            return game != null ? Port.Nearest(game.ShipPosition)?.Def : null;
        }

        public static List<object> Capture() => Log.ConvertAll(d => (object)new List<object> { d.when, d.deed, d.where });

        public static void Restore(List<object> data)
        {
            Log.Clear();
            foreach (var x in data ?? new List<object>())
                if (x is List<object> l && l.Count >= 3)
                    Log.Add((l[0] as string, l[1] as string, l[2] as string));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Clear() => Log.Clear();
    }

    public enum PatrolStage { None, Search, Signal }

    /// <summary>
    /// THE KING'S MEN (Plans/ai_fleets.md, phase 5) - the law kept at ports and, from 800, at sea. Casting off from a port
    /// with officials, bound out of its realm, a ship may be searched (the port's strictness); what's found of goods a law
    /// bites on there and then (trade_rules.json 'laws': 'no export' only when bound out of the zone) depends on how much
    /// the hold hides. The merchant's answers: let them search, declare what he carries (seized, no fine), or offer the
    /// officer a gift; at sea, to a guard boat's signal: heave to, dump the goods overboard, or run. Every outcome is
    /// facts; deeds are kept for reputation. The world's traders are searched by the same chances.
    /// </summary>
    public static class Patrols
    {
        public static PatrolRules Rules;
        public static PatrolStage Stage { get; private set; }
        /// <summary>The port (or station's port) whose officers these are, and how strict.</summary>
        public static PortDef At { get; private set; }
        public static double Strictness { get; private set; }
        /// <summary>The graphs' "On the king's men": a search is due ("port") or a guard boat signals ("sea").</summary>
        public static event Action<string> Due;
        static System.Random dice = new System.Random(800);
        static readonly Dictionary<string, double> stationSeen = new Dictionary<string, double>();

        /// <summary>The goods aboard a law bites on, leaving `zone` in `year` (bound out of it or not).</summary>
        public static List<(GoodDef good, int count, LawDef law)> UnderLaw(Hold hold, ZoneDef zone, int year, bool boundOut)
        {
            var found = new List<(GoodDef, int, LawDef)>();
            if (hold == null || zone == null || TradeRules.Current == null)
                return found;
            foreach (var (good, count) in hold.Contents)
            {
                if (string.IsNullOrEmpty(good.law))
                    continue;
                LawDef worst = null;                                       // (the severest law that bites: death over no export)
                foreach (var law in TradeRules.Current.LawsOn(good, zone, year))
                    if ((law.rule == "no export" || law.rule == "death") && boundOut && (worst == null || law.rule == "death"))
                        worst = law;
                if (worst != null)
                    found.Add((good, count, worst));
            }
            return found;
        }

        /// <summary>True if a passage from a port to another leaves the first's money zone (the realm).</summary>
        public static bool BoundOut(PortDef from, PortDef to)
        {
            if (from == null || to == null || TradeRules.Current == null)
                return false;
            return TradeRules.Current.ZoneOf(from.region)?.id != TradeRules.Current.ZoneOf(to.region)?.id;
        }

        /// <summary>The share of a search a hold hides: cover_max times how full she is.</summary>
        public static double Cover(Hold hold) => hold == null || hold.capacity <= 0f ? 0.0 : Rules.coverMax * Math.Min(1.0, hold.Used / hold.capacity);

        // ------------------------------------------------------------------------------------------ the merchant

        /// <summary>He's cast off from a port: the officers may search (a panel if there's anything to find; a notice if
        /// not).</summary>
        public static void CastOff(PortDef port)
        {
            var game = KapmanGame.Current;
            if (Rules == null || port == null || Stage != PatrolStage.None || game == null || !Trade.Started)
                return;
            double strict = Math.Min(1.0, Rules.Strictness(port) * Reputation.SearchFactor(port));   // (stricter with the ill-regarded)
            if (strict <= 0.0 || dice.NextDouble() >= strict)
                return;
            var dest = GameData.Port(Course.Destination);
            var zone = TradeRules.Current.ZoneOf(port.region);
            var law = UnderLaw(Trade.Hold, zone, game.Year, BoundOut(port, dest));
            if (law.Count == 0)
            {
                Voyage.Say($"The officers at {port.name} searched the ship and found nothing against the law.");
                return;
            }
            At = port;
            Strictness = strict;
            Stage = PatrolStage.Search;
            Due?.Invoke("port");
        }

        /// <summary>At sea (from 800, or with the test flag): a guard boat near a watch station signals a ship bound out
        /// of the realm with goods under its law. Once a day per station.</summary>
        public static void LookAtSea()
        {
            var game = KapmanGame.Current;
            // (7 Oct 2026: "at the map table", not "the map is open". On the chart the map is always open: the signal
            // waited for it to shut, the stage stood at Signal for good, and the planner held her there for ever)
            bool mapOpen = MapView.AwayFromSea;
            if (signalWaiting && Stage == PatrolStage.Signal && !mapOpen)
            {
                signalWaiting = false;                                       // (the alert answered: back at sea, they signal)
                Due?.Invoke("sea");
                return;
            }
            if (Rules == null || Stage != PatrolStage.None || game == null || game.ship == null || !Trade.Started
                || (game.Year < Rules.guardBoatsFrom && !Rules.testGuardBoatsNow) || game.ship.anchored)
                return;
            if (Encounter.Stage != EncounterStage.None || RaiderEncounter.Stage != RaidStage.None || (TextPanel.Current != null && TextPanel.Current.Open))
                return;
            // (not inside a port's reach: held there she would be taken for arrived, and the port's own menu would come
            // up over the officers' talk. As a trader's meeting and the raiders keep clear of the ports too)
            var near = Port.Nearest(game.ShipPosition);
            if (near != null && near.DistanceTo(game.ShipPosition) < near.arrivalRadius * 2f)
                return;
            var me = SimStage.ToWorld(game.ShipPosition);               // ported 4 Oct: was FloatingOrigin.ToWorld (the stage maps the scene to the world)
            foreach (var st in Rules.stations)
            {
                var p = GameData.Port(st.port);
                if (p == null || (stationSeen.TryGetValue(st.id, out double last) && game.Day - last < 1.0))
                    continue;
                var (x, z) = WorldFrame.ToWorld(p.lat, p.lon);
                double km = Math.Sqrt((x - me.x) * (x - me.x) + (z - me.z) * (z - me.z)) / 1000.0;
                if (km > st.radiusKm)
                    continue;
                stationSeen[st.id] = game.Day;
                var zone = TradeRules.Current.ZoneOf(p.region);
                var law = UnderLaw(Trade.Hold, zone, game.Year, BoundOut(p, GameData.Port(Course.Destination)));
                if (law.Count == 0 || dice.NextDouble() >= st.strictness)
                    continue;
                At = p;
                Strictness = st.strictness;
                Stage = PatrolStage.Signal;
                if (mapOpen)
                {
                    signalWaiting = true;
                    Alerts.GuardBoat();
                    return;
                }
                Due?.Invoke("sea");
                return;
            }
        }

        /// <summary>A guard boat off a port signals her now (a tool, a test: what LookAtSea does when its dice fall).
        /// False if something else is on.</summary>
        public static bool Signal(PortDef at, double strictness = 1.0)
        {
            if (at == null || Stage != PatrolStage.None || Encounter.Stage != EncounterStage.None || RaiderEncounter.Stage != RaidStage.None)
                return false;
            At = at;
            Strictness = strictness;
            Stage = PatrolStage.Signal;
            signalWaiting = false;
            Due?.Invoke("sea");
            return true;
        }

        /// <summary>THE KING'S MEN DO NOT WAIT FOR EVER (7 Oct 2026, ruling 10: a pressing card holds the ship and takes
        /// one of its options by itself when its time is up). Game hours of running clock a signal or a search waits
        /// for an answer; then she heaves to (at sea) or lets them search (in port): the answer that asks nothing of
        /// her but the search itself. SeaHolds counts the hours.</summary>
        public static double PressHours = 2.0;

        static bool signalWaiting;

        /// <summary>The facts of it: whose men, and what they're after.</summary>
        public static string Facts()
        {
            if (At == null)
                return "";
            var game = KapmanGame.Current;
            var law = UnderLaw(Trade.Hold, TradeRules.Current.ZoneOf(At.region), game != null ? game.Year : 793, true);
            string what = law.Count == 0 ? "" : " Aboard, under the law here: " + string.Join(", ", law.Select(l => Trade.UnitsNamed(l.good, l.count))) + ".";
            return Stage == PatrolStage.Signal
                ? $"A boat of the king's men off {At.name} signals you to heave to.{what}"
                : $"The officers at {At.name} come aboard to search the ship before she leaves.{what}";
        }

        /// <summary>"search", "declare", "gift", "heave_to", "dump", "run": the outcome, as facts.</summary>
        public static string Answer(string what)
        {
            var game = KapmanGame.Current;
            if (Stage == PatrolStage.None || At == null || game == null)
                return End("");
            var zone = TradeRules.Current.ZoneOf(At.region);
            var law = UnderLaw(Trade.Hold, zone, game.Year, true);
            switch (what)
            {
                case "declare":
                    return End(law.Count == 0 ? "You declared your cargo; nothing in it is against the law." :
                                                "You declared it. " + Seize(law, zone, 0.0) + " No fine.");
                case "gift":
                {
                    double worth = Worth(law, zone);
                    double gift = Math.Max(Rules.giftMinD, worth * Rules.giftShare);
                    double silver = Trade.Purse.Silver(TradeRules.Current);
                    if (silver < gift)
                        return Search(law, zone, 1.0, Rules.fineShare, $"You hadn't {gift:0} deniers to give. ");
                    if (dice.NextDouble() < Rules.giftTaken * (1.0 - Strictness * 0.5))
                    {
                        Trade.Purse.PayUpTo(gift, TradeRules.Current);
                        Deeds.Record("bribed_officer", "bribed the king's officer", At);
                        return End($"The officer took your gift of {gift:0} deniers' worth and looked the other way.");
                    }
                    return Search(law, zone, 1.0, Rules.fineShare * 2.0, "He refused your gift and searched hard. ");
                }
                case "dump":
                {
                    if (law.Count == 0)
                        return End("There was nothing to throw over.");
                    foreach (var (good, count, _) in law)
                        Trade.Hold.Remove(good, count);
                    Deeds.Record("threw goods overboard before the king's men", At.name);
                    return End("You threw " + string.Join(", ", law.Select(l => Trade.UnitsNamed(l.good, l.count))) +
                               " overboard before they came alongside. They searched and found nothing.");
                }
                case "run":
                    if (dice.NextDouble() < Rules.runEscape)
                    {
                        Deeds.Record("fled_kings_men", "fled the king's men", At);
                        return End("You ran, and they couldn't catch you. They will remember the ship.");
                    }
                    return Search(law, zone, 1.0, Rules.fineShare * 2.0, "You ran, and they caught you. ");
                default:                                                    // search, heave to
                    return Search(law, zone, 1.0 - Cover(Trade.Hold), Rules.fineShare, "");
            }
        }

        static string Search(List<(GoodDef good, int count, LawDef law)> law, ZoneDef zone, double findChance, double fine, string before)
        {
            if (law.Count == 0)
                return End(before + "They searched and found nothing against the law.");
            if (dice.NextDouble() >= findChance)
                return End(before + "They searched, but didn't find what you carry.");
            bool judgement = law.Any(l => l.law.rule == "death");
            string seized = Seize(law, zone, fine);
            if (judgement)
            {
                Deeds.Record("taken_for_judgement", "taken for judgement", At);
                return End(before + seized + " You are taken before the count for judgement.");
            }
            Deeds.Record("goods_seized", "goods seized by the king's men", At);
            return End(before + seized);
        }

        static string Seize(List<(GoodDef good, int count, LawDef law)> law, ZoneDef zone, double fineShare)
        {
            double worth = Worth(law, zone);
            foreach (var (good, count, _) in law)
                Trade.Hold.Remove(good, count);
            string what = "They seized " + string.Join(", ", law.Select(l => Trade.UnitsNamed(l.good, l.count))) + ".";
            if (fineShare <= 0.0)
                return what;
            double silver = Trade.Purse.Silver(TradeRules.Current);
            double fine = Math.Min(silver, worth * fineShare);
            if (fine > 0.0)
                fine = Trade.Purse.PayUpTo(fine, TradeRules.Current);
            return what + $" A fine of {fine:0} deniers' worth.";
        }

        /// <summary>Deniers the goods would fetch here (the port's market pays).</summary>
        static double Worth(List<(GoodDef good, int count, LawDef law)> law, ZoneDef zone)
        {
            var game = KapmanGame.Current;
            var m = game != null && At != null ? Markets.Of(At, game.Year, game.Day) : null;
            double sum = 0.0;
            foreach (var (good, count, _) in law)
            {
                var g = m?.Get(good.id);
                sum += count * (g != null ? m.Pays(g, game.Month) : good.value);
            }
            return sum;
        }

        static string End(string fact)
        {
            Stage = PatrolStage.None;
            At = null;
            if (!string.IsNullOrEmpty(fact))
                Voyage.Say(fact);
            return fact;
        }

        // ------------------------------------------------------------------------------------------ the world's traders

        /// <summary>A trader of the world leaving a port, bound out of its realm: searched by the same chances; what's
        /// found under the law is seized. Returns the units seized.</summary>
        public static int SearchTrader(WorldSim w, SimShip s, PortDef from, PortDef to)
        {
            if (Rules == null || !Rules.aiSearched || s.raider || from == null)
                return 0;
            double strict = Rules.Strictness(from);
            if (strict <= 0.0 || !s.rng.Chance(strict))
                return 0;
            var law = UnderLaw(s.hold, TradeRules.Current.ZoneOf(from.region), w.Date.year, BoundOut(from, to));
            w.stats.searches++;
            if (law.Count == 0 || !s.rng.Chance(1.0 - Rules.coverMax * Math.Min(1.0, s.hold.Used / Math.Max(1f, s.hold.capacity))))
                return 0;
            int units = 0;
            foreach (var (good, count, _) in law)
            {
                s.hold.Remove(good, count);
                units += count;
            }
            w.stats.seizures++;
            return units;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearState()
        {
            Stage = PatrolStage.None;
            At = null;
            signalWaiting = false;
            stationSeen.Clear();
        }
    }
}
