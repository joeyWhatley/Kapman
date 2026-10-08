using System;
using System.Collections.Generic;
using Kapman.Game.Sim;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>How the merchant came to know something.</summary>
    public enum How
    {
        /// <summary>Seen at first hand.</summary>
        Seen,
        /// <summary>Told by someone who saw it.</summary>
        Told,
        /// <summary>Talk: second-hand or worse (the hops say how far).</summary>
        Talk,
        /// <summary>Written word: a letter, a monastery's news.</summary>
        Written,
        /// <summary>A rumour: who knows where it came from.</summary>
        Rumour,
        /// <summary>What every merchant knows (the goods table: cheap where made, dear where wanted).</summary>
        Lore,
        /// <summary>The merchant's own doing.</summary>
        Own,
    }

    /// <summary>One thing the merchant knows: what, where, when it was true, when and how it was learned, from whom.</summary>
    public sealed class Fact
    {
        public int id;
        /// <summary>raid, taken, a chronicle kind (war, death, famine...), fair, ship_seen, raiders_seen.</summary>
        public string kind;
        /// <summary>The news item's id (news), or the ship's id (a sighting).</summary>
        public string refId;
        /// <summary>The chronicle entry it tells of, if any.</summary>
        public string chronicleId;
        public string text, from;
        public int port = -1, learnedAt = -1;
        /// <summary>Where (real metres), if not a port; NaN if unknown.</summary>
        public double x = double.NaN, z = double.NaN;
        public double trueDay, learnedDay;
        public How how;
        public int hops;
        /// <summary>Small numbers that go with it: a sighting's stage (1 sighted, 2 made out) and the men seen.</summary>
        public double a, b;
    }

    /// <summary>What the merchant believes a market pays and asks for a good, how sure, and from what.</summary>
    public struct KnownPrice
    {
        /// <summary>Believed price (deniers): paid to a seller before the toll, and asked of a buyer (NaN: not expected on offer).</summary>
        public float pays, asks;
        public bool onOffer;
        /// <summary>False: nothing noted there; it's only what every merchant knows.</summary>
        public bool noted;
        /// <summary>When the note it rests on was true (game day), how old, how it was learned and from whom.</summary>
        public double day, age;
        public How how;
        public int hops;
        public string from;
        /// <summary>How sure (0-1), and the range the true price likely lies in.</summary>
        public float sure, lo, hi;
        /// <summary>The note's own price and the usual price it fades toward, for the explanations.</summary>
        public float notedPays, usualPays, weight;
    }

    /// <summary>The rules of knowing (Data/knowing_793.json): how sure each way of learning is, how fast each kind of
    /// knowledge goes stale, how far told prices can be off.</summary>
    public sealed class KnowingRules
    {
        public float sureSeen = 1f, sureTold = 0.85f, sureTalk = 0.7f, surePerHop = 0.1f, sureWritten = 0.8f, sureRumour = 0.45f, sureLore = 0.3f;
        public double priceHalfLife = 30.0, newsHalfLife = 90.0, raidersSeenHalfLife = 4.0, shipSeenHalfLife = 3.0;
        public float errorTalk = 0.03f, errorPerHop = 0.02f, errorWritten = 0.01f, errorRumour = 0.12f;
        public double keepAllDays = 365.0, seasonAfterYears = 10.0;
        public float sureWord = 0.8f, likelyWord = 0.55f, doubtfulWord = 0.3f;
        /// <summary>The word on the item (Known.Word): hard news counts while younger than this many days; a price seen,
        /// a price told, anything noted at all; and how many days back a ship that sailed ahead still counts.</summary>
        public double wordNewsDays = 60.0, wordSeenDays = 7.0, wordToldDays = 14.0, wordKnownDays = 90.0, wordAheadDays = 3.0;
        /// <summary>A told price is said as a word: "well" at this many times the good's table value or more, "poorly" at
        /// this many times or less, else "the usual".</summary>
        public float paysWell = 1.15f, paysPoorly = 0.85f;
        /// <summary>The word is said against the price where she lies: a price within this share of it is "much the
        /// same". (Where "far more" begins is not a number here: it is worked out from the market's gap and the toll,
        /// Known.FarOver.)</summary>
        public float sameWithin = 0.05f;
        /// <summary>A price told by one who saw it himself is said as a rough figure ("about 36 d"). False: as a word,
        /// like all talk (the rule of 4 October: a number only for what she saw herself).</summary>
        public bool toldFigure = true;

        public static KnowingRules Load(TextAsset json)
        {
            var r = new KnowingRules();
            if (json == null)
                return r;
            var root = Json.Parse(json.text);
            var sure = Json.Obj(root, "sure");
            r.sureSeen = (float)Json.Num(sure, "seen", r.sureSeen);
            r.sureTold = (float)Json.Num(sure, "told", r.sureTold);
            r.sureTalk = (float)Json.Num(sure, "talk", r.sureTalk);
            r.surePerHop = (float)Json.Num(sure, "per_hop", r.surePerHop);
            r.sureWritten = (float)Json.Num(sure, "written", r.sureWritten);
            r.sureRumour = (float)Json.Num(sure, "rumour", r.sureRumour);
            r.sureLore = (float)Json.Num(sure, "lore", r.sureLore);
            var half = Json.Obj(root, "half_life_days");
            r.priceHalfLife = Json.Num(half, "price", r.priceHalfLife);
            r.newsHalfLife = Json.Num(half, "news", r.newsHalfLife);
            r.raidersSeenHalfLife = Json.Num(half, "raiders_seen", r.raidersSeenHalfLife);
            r.shipSeenHalfLife = Json.Num(half, "ship_seen", r.shipSeenHalfLife);
            var err = Json.Obj(root, "error");
            r.errorTalk = (float)Json.Num(err, "talk", r.errorTalk);
            r.errorPerHop = (float)Json.Num(err, "per_hop", r.errorPerHop);
            r.errorWritten = (float)Json.Num(err, "written", r.errorWritten);
            r.errorRumour = (float)Json.Num(err, "rumour", r.errorRumour);
            var keep = Json.Obj(root, "keep");
            r.keepAllDays = Json.Num(keep, "all_days", r.keepAllDays);
            r.seasonAfterYears = Json.Num(keep, "season_after_years", r.seasonAfterYears);
            var words = Json.Obj(root, "words");
            r.sureWord = (float)Json.Num(words, "sure", r.sureWord);
            r.likelyWord = (float)Json.Num(words, "likely", r.likelyWord);
            r.doubtfulWord = (float)Json.Num(words, "doubtful", r.doubtfulWord);
            var word = Json.Obj(root, "word");
            r.wordNewsDays = Json.Num(word, "news_days", r.wordNewsDays);
            r.wordSeenDays = Json.Num(word, "seen_days", r.wordSeenDays);
            r.wordToldDays = Json.Num(word, "told_days", r.wordToldDays);
            r.wordKnownDays = Json.Num(word, "known_days", r.wordKnownDays);
            r.wordAheadDays = Json.Num(word, "ahead_days", r.wordAheadDays);
            r.paysWell = (float)Json.Num(word, "pays_well", r.paysWell);
            r.paysPoorly = (float)Json.Num(word, "pays_poorly", r.paysPoorly);
            r.sameWithin = (float)Json.Num(word, "same_within", r.sameWithin);
            r.toldFigure = Json.Bool(word, "told_figure", r.toldFigure);
            return r;
        }
    }

    /// <summary>
    /// WHAT THE MERCHANT KNOWS (Plans/map_play.md: "only what you know") - the one place every map mode, chart, planner and
    /// tooltip reads from, so nothing on the map side shows the hidden truth. Prices are the price notes in the chest
    /// (<see cref="Trade.Ledger"/>); this holds the rest: the news heard (its words kept, as told: with when and from whom),
    /// ships sighted and spoken to, the places heard of. Each fact is surest when learned and fades as it ages, by how it
    /// came (seen, told, talk, rumour) and what it is (a price goes stale in a month, a sighting in days).
    /// It only watches: nothing here feeds the world's simulation, and nothing here asks a market (which would bring it
    /// up to date and change what the world's dice decide). The word on an item (the one line under a good) is in Word.cs.
    /// </summary>
    public static partial class Known
    {
        public static KnowingRules Rules = new KnowingRules();

        static readonly List<Fact> facts = new List<Fact>();
        static readonly Dictionary<string, Fact> news = new Dictionary<string, Fact>();          // by news id
        static readonly Dictionary<string, Fact> sightings = new Dictionary<string, Fact>();     // the latest of each ship
        static readonly Dictionary<string, double> spoken = new Dictionary<string, double>();   // ship id -> day spoken to
        static readonly HashSet<string> heardOf = new HashSet<string>();                        // port ids
        static readonly Dictionary<string, (double here, double region, double day)> standing = new Dictionary<string, (double, double, double)>();
        static int nextId;

        /// <summary>Goes up whenever something new is learned (views redraw when it changes).</summary>
        public static int Version { get; private set; }

        /// <summary>Something new is known.</summary>
        public static event Action<Fact> Learned;

        public static void Load(TextAsset json) => Rules = KnowingRules.Load(json);

        public static IReadOnlyList<Fact> Facts => facts;

        // ------------------------------------------------------------------------------------------ learning

        /// <summary>A piece of news heard (once: hearing it again changes nothing). `from`: who told it; `at`: the port
        /// it was heard at (-1 at sea).</summary>
        public static Fact HearNews(NewsItem item, double today, string from, How how = How.Talk, int hops = 1, int at = -1)
        {
            if (item == null || string.IsNullOrEmpty(item.id) || news.ContainsKey(item.id))
                return null;
            var f = new Fact
            {
                id = nextId++, kind = item.kind, refId = item.id, chronicleId = item.refId, text = item.text, from = from,
                port = item.port, learnedAt = at, trueDay = item.day, learnedDay = today, how = how, hops = hops,
            };
            if (item.port < 0 && !double.IsNaN(item.x))
            {
                f.x = item.x;
                f.z = item.z;
            }
            facts.Add(f);
            news[item.id] = f;
            if (item.port >= 0 && item.port < GameData.Ports.Count)
                heardOf.Add(GameData.Ports[item.port].id);
            Version++;
            Learned?.Invoke(f);
            return f;
        }

        /// <summary>The news heard of a chronicle entry (the first word of it), or null.</summary>
        public static Fact EntryFact(string chronicleId)
        {
            Fact first = null;
            if (chronicleId != null)
                foreach (var f in news.Values)
                    if (f.chronicleId == chronicleId && (first == null || f.learnedDay < first.learnedDay))
                        first = f;
            return first;
        }

        /// <summary>The merchant's standing at a port, as found there on a visit (and the region's), with the day.</summary>
        public static void NoteStanding(PortDef port, double here, double region, double day)
        {
            if (port != null)
                standing[port.id] = (here, region, day);
        }

        /// <summary>The standing last found at a port, if ever there.</summary>
        public static bool StandingAt(PortDef port, out double here, out double region, out double day)
        {
            here = region = day = 0.0;
            if (port == null || !standing.TryGetValue(port.id, out var s))
                return false;
            (here, region, day) = s;
            return true;
        }

        public static bool HeardNews(string newsId) => newsId != null && news.ContainsKey(newsId);

        /// <summary>Has word of this chronicle entry reached the merchant (or was it before the game began)?</summary>
        public static bool HeardOfEntry(string chronicleId)
        {
            foreach (var f in news.Values)
                if (f.chronicleId == chronicleId)
                    return true;
            return false;
        }

        /// <summary>The lookout's sighting of a ship (kept: the latest of each, and every band of raiders).</summary>
        public static Fact Sighted(SimShip s, bool madeOut, double x, double z, double today)
        {
            if (s == null)
                return null;
            string kind = s.raider && madeOut ? "raiders_seen" : "ship_seen";
            sightings.TryGetValue(s.id, out var f);
            bool fresh = f == null || f.kind != kind || today - f.trueDay > 1.0;
            if (fresh)
            {
                f = new Fact { id = nextId++, kind = kind, refId = s.id, how = How.Seen };
                if (kind == "raiders_seen" || !sightings.ContainsKey(s.id))
                    facts.Add(f);
                else
                    facts[facts.IndexOf(sightings[s.id])] = f;               // (an ordinary ship: only her latest is kept)
                sightings[s.id] = f;
            }
            f.x = x;
            f.z = z;
            f.trueDay = f.learnedDay = today;
            f.a = madeOut ? 2 : 1;
            if (s.raider && madeOut)
            {
                // the men are counted by eye: off by up to a fifth, the same count each time for the same band
                float off = 1f + 0.2f * (2f * Hash01(s.id, Math.Floor(today), 0, 7) - 1f);
                f.b = Math.Max(1, Math.Round(s.men * off / 5.0) * 5.0);
                f.text = $"You made out raiders, {s.bandShips} {(s.bandShips == 1 ? "ship" : "ships")} and about {f.b:0} men.";
            }
            else
                f.text = madeOut ? $"You made out {SeaWatch.Label(s)}." : "You sighted a sail.";
            Version++;
            if (fresh)
                Learned?.Invoke(f);
            return f;
        }

        /// <summary>The latest sighting of a ship, or null.</summary>
        public static Fact SightingOf(string shipId) => shipId != null && sightings.TryGetValue(shipId, out var f) ? f : null;

        /// <summary>The merchant has spoken to a ship (hailed her, or been hailed): where she's bound and what she carries are
        /// known from then.</summary>
        public static void Spoke(SimShip s, double today)
        {
            if (s == null)
                return;
            spoken[s.id] = today;
            Version++;
            if (s.home >= 0 && s.home < GameData.Ports.Count)
                heardOf.Add(GameData.Ports[s.home].id);
            if (s.dest >= 0 && s.dest < GameData.Ports.Count)
                heardOf.Add(GameData.Ports[s.dest].id);
        }

        public static bool HasSpoken(string shipId) => shipId != null && spoken.ContainsKey(shipId);

        /// <summary>A place has been heard of (named in talk, by a ship, in news).</summary>
        public static void HearOf(PortDef port)
        {
            if (port != null && heardOf.Add(port.id))
                Version++;
        }

        public static bool IsHeardOf(PortDef port) => port != null && heardOf.Contains(port.id);

        // ------------------------------------------------------------------------------------------ reading

        /// <summary>How sure something learned this way is when it's learned (0-1).</summary>
        public static float SureWhenLearned(How how, int hops)
        {
            var r = Rules;
            switch (how)
            {
                case How.Seen: case How.Own: return r.sureSeen;
                case How.Told: return r.sureTold;
                case How.Talk: return Mathf.Max(0.05f, r.sureTalk - r.surePerHop * Mathf.Max(0, hops - 1));
                case How.Written: return r.sureWritten;
                case How.Rumour: return r.sureRumour;
                default: return r.sureLore;
            }
        }

        /// <summary>The share a told price may be off by (0: exact), by how it came.</summary>
        public static float ErrorShare(How how, int hops)
        {
            var r = Rules;
            switch (how)
            {
                case How.Talk: return r.errorTalk + r.errorPerHop * Mathf.Max(0, hops - 1);
                case How.Written: return r.errorWritten;
                case How.Rumour: return r.errorRumour;
                default: return 0f;
            }
        }

        /// <summary>How much an age (days) leaves of what was known, for a half-life (days): 1 new, 0.5 at the half-life.</summary>
        public static float Weight(double ageDays, double halfLife) =>
            halfLife <= 0.0 ? 1f : (float)Math.Pow(0.5, Math.Max(0.0, ageDays) / halfLife);

        static double HalfLife(string kind)
        {
            switch (kind)
            {
                case "price": return Rules.priceHalfLife;
                case "raiders_seen": return Rules.raidersSeenHalfLife;
                case "ship_seen": return Rules.shipSeenHalfLife;
                default: return Rules.newsHalfLife;
            }
        }

        /// <summary>How sure a fact is today (0-1).</summary>
        public static float Sure(Fact f, double today) =>
            f == null ? 0f : SureWhenLearned(f.how, f.hops) * Weight(today - f.trueDay, HalfLife(f.kind));

        /// <summary>Plain words for how sure: "sure", "likely", "doubtful", "stale".</summary>
        public static string Words(float sure) =>
            sure >= Rules.sureWord ? "sure" : sure >= Rules.likelyWord ? "likely" : sure >= Rules.doubtfulWord ? "doubtful" : "stale";

        /// <summary>
        /// What the merchant believes a market pays and asks for a good today. From the surest note there (the newest,
        /// unless an older one seen at first hand is surer), fading toward what every merchant knows as it ages (the same
        /// way the AI traders' belief fades: half-way in a month). With no note, it's that common knowledge alone.
        /// </summary>
        public static KnownPrice Price(PortDef port, GoodDef g, int month, double today)
        {
            var k = new KnownPrice { asks = float.NaN, day = double.NaN, age = double.PositiveInfinity, how = How.Lore };
            if (port == null || g == null)
                return k;
            k.usualPays = Lore.UsualPays(port, g, month);
            float usualAsks = Lore.UsualAsks(port, g, month);
            PriceNote best = null;
            float bestSure = 0f;
            var notes = Trade.Started ? Trade.Ledger.History(port.id) : null;
            if (notes != null)
                for (int i = notes.Count - 1, looked = 0; i >= 0 && looked < 8; i--)
                {
                    var n = notes[i];
                    if (!n.prices.ContainsKey(g.id))
                        continue;
                    looked++;
                    float s = SureWhenLearned(n.how, n.hops) * Weight(today - n.day, Rules.priceHalfLife);
                    if (s > bestSure + 1e-4f)
                    {
                        best = n;
                        bestSure = s;
                    }
                }
            if (best == null)
            {
                k.pays = k.notedPays = k.usualPays;
                k.asks = usualAsks;
                k.onOffer = !float.IsNaN(usualAsks);
                k.sure = Rules.sureLore;
                k.lo = k.pays * 0.6f;
                k.hi = k.pays * 1.6f;
                return k;
            }
            var seen = best.prices[g.id];
            float w = Weight(today - best.day, Rules.priceHalfLife);
            k.noted = true;
            k.day = best.day;
            k.age = Math.Max(0.0, today - best.day);
            k.how = best.how;
            k.hops = best.hops;
            k.from = best.source;
            k.weight = w;
            k.notedPays = seen.pays;
            k.pays = w * seen.pays + (1f - w) * k.usualPays;
            if (seen.onOffer)
                k.asks = float.IsNaN(usualAsks) ? (w >= 0.5f ? seen.asks : float.NaN) : w * seen.asks + (1f - w) * usualAsks;
            else
                k.asks = w >= 0.5f ? float.NaN : usualAsks;
            k.onOffer = !float.IsNaN(k.asks);
            k.sure = bestSure;
            float band = 0.05f + 0.25f * (1f - w) + ErrorShare(best.how, best.hops);
            k.lo = k.pays * (1f - band);
            k.hi = k.pays * (1f + band);
            return k;
        }

        /// <summary>How fresh what's known of a port's market is: the age (days) of the newest note there (infinite: none).</summary>
        public static double PricesAge(PortDef port, double today)
        {
            var n = port != null && Trade.Started ? Trade.Ledger.Latest(port.id) : null;
            return n == null ? double.PositiveInfinity : Math.Max(0.0, today - n.day);
        }

        /// <summary>The news heard, the latest first (by when it happened).</summary>
        public static List<Fact> News()
        {
            var list = new List<Fact>(news.Values);
            list.Sort((p, q) => q.trueDay.CompareTo(p.trueDay));
            return list;
        }

        /// <summary>The raiders sighted, the latest first.</summary>
        public static List<Fact> RaidersSeen()
        {
            var list = facts.FindAll(f => f.kind == "raiders_seen");
            list.Sort((p, q) => q.trueDay.CompareTo(p.trueDay));
            return list;
        }

        /// <summary>A number between 0 and 1 fixed by its inputs (a told price's error, a count by eye): the same every time
        /// for the same thing, so nothing is drawn again after a load.</summary>
        public static float Hash01(string what, double day, int i, int j)
        {
            unchecked
            {
                ulong h = 14695981039346656037UL;
                foreach (char c in what ?? "")
                    h = (h ^ c) * 1099511628211UL;
                long d = (long)Math.Round(day * 1440.0);
                for (int b = 0; b < 8; b++)
                    h = (h ^ (byte)(d >> (8 * b))) * 1099511628211UL;
                h = (h ^ (uint)i) * 1099511628211UL;
                h = (h ^ (uint)j) * 1099511628211UL;
                h ^= h >> 33;
                h *= 0xff51afd7ed558ccdUL;
                h ^= h >> 33;
                return (h >> 40) / (float)(1UL << 24);
            }
        }

        // ------------------------------------------------------------------------------------------ saving

        /// <summary>What's known, as plain data (a saved game).</summary>
        public static Dictionary<string, object> Capture()
        {
            var list = new List<object>();
            foreach (var f in facts)
                list.Add(new Dictionary<string, object>
                {
                    ["id"] = f.id, ["kind"] = f.kind, ["ref"] = f.refId, ["chronicle"] = f.chronicleId, ["text"] = f.text, ["from"] = f.from,
                    ["port"] = f.port, ["at"] = f.learnedAt, ["x"] = double.IsNaN(f.x) ? null : (object)f.x, ["z"] = double.IsNaN(f.z) ? null : (object)f.z,
                    ["day"] = f.trueDay, ["learned"] = f.learnedDay, ["how"] = (int)f.how, ["hops"] = f.hops, ["a"] = f.a, ["b"] = f.b,
                });
            var spokenList = new List<object>();
            foreach (var kv in spoken)
                spokenList.Add(new List<object> { kv.Key, kv.Value });
            var heard = new List<object>();
            foreach (var p in heardOf)
                heard.Add(p);
            var stand = new Dictionary<string, object>();
            foreach (var kv in standing)
                stand[kv.Key] = new List<object> { kv.Value.here, kv.Value.region, kv.Value.day };
            return new Dictionary<string, object> { ["next"] = nextId, ["facts"] = list, ["spoken"] = spokenList, ["heard_of"] = heard, ["standing"] = stand };
        }

        public static void Restore(Dictionary<string, object> d)
        {
            Clear();
            if (d == null)
                return;
            nextId = (int)Json.Num(d, "next");
            foreach (var x in Json.Arr(d, "facts") ?? new List<object>())
            {
                var f = new Fact
                {
                    id = (int)Json.Num(x, "id"), kind = Json.Str(x, "kind"), refId = Json.Str(x, "ref", null), chronicleId = Json.Str(x, "chronicle", null),
                    text = Json.Str(x, "text"), from = Json.Str(x, "from", null), port = (int)Json.Num(x, "port", -1), learnedAt = (int)Json.Num(x, "at", -1),
                    trueDay = Json.Num(x, "day"), learnedDay = Json.Num(x, "learned"), how = (How)(int)Json.Num(x, "how"), hops = (int)Json.Num(x, "hops"),
                    a = Json.Num(x, "a"), b = Json.Num(x, "b"),
                };
                var od = x as Dictionary<string, object>;
                f.x = od != null && od.TryGetValue("x", out var vx) && vx is double dx ? dx : double.NaN;
                f.z = od != null && od.TryGetValue("z", out var vz) && vz is double dz ? dz : double.NaN;
                facts.Add(f);
                if (f.kind == "ship_seen" || f.kind == "raiders_seen")
                {
                    if (f.refId != null && (!sightings.TryGetValue(f.refId, out var was) || was.trueDay <= f.trueDay))
                        sightings[f.refId] = f;
                }
                else if (f.refId != null)
                    news[f.refId] = f;
                nextId = Math.Max(nextId, f.id + 1);
            }
            foreach (var x in Json.Arr(d, "spoken") ?? new List<object>())
                if (x is List<object> l && l.Count >= 2 && l[0] is string id && l[1] is double day)
                    spoken[id] = day;
            foreach (var x in Json.Arr(d, "heard_of") ?? new List<object>())
                if (x is string p)
                    heardOf.Add(p);
            foreach (var kv in Json.Obj(d, "standing") ?? new Dictionary<string, object>())
                if (kv.Value is List<object> l && l.Count >= 3 && l[0] is double h && l[1] is double r && l[2] is double day)
                    standing[kv.Key] = (h, r, day);
            Version++;
        }

        /// <summary>
        /// An older save (version 1) kept only the ids of the news heard: whichever of them the waterfronts and ships
        /// still hold come back with their words (as heard on the day saved); the rest are gone.
        /// </summary>
        public static void FromOldSave(List<object> heardIds, WorldSim world, double today)
        {
            if (heardIds == null || world == null)
                return;
            var want = new HashSet<string>();
            foreach (var x in heardIds)
                if (x is string s)
                    want.Add(s);
            for (int i = 0; i < GameData.Ports.Count && want.Count > 0; i++)
                foreach (var (item, learned, from) in world.Board(i).News)
                    if (want.Remove(item.id))
                        HearNews(item, Math.Min(today, learned), from, How.Talk, 1, i);
        }

        /// <summary>Forgets everything (a new game; entering Play).</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Clear()
        {
            facts.Clear();
            news.Clear();
            sightings.Clear();
            spoken.Clear();
            heardOf.Clear();
            standing.Clear();
            nextId = 0;
            Version++;
        }
    }
}
