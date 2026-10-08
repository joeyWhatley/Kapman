// 7 Oct 2026: THE SELF AS THE WRITERS' DATA. Reads Game/Data/story/self_793.json (peoples, faiths, pasts, the five
// stats) through ContentFiles. If the file is missing or does not parse, a small table of its own stands in, so the
// creation window always has something true to offer, and FromFile says which it was.
using System;
using System.Collections.Generic;

namespace Kapman.Game
{
    /// <summary>A people the player may be: its name, where its merchants start, its first tongue and the tongues it
    /// half knows, the faiths usual among it, and what is hard about the start.</summary>
    public sealed class PeopleDef
    {
        public string id = "", name = "", home = "", firstTongue = "", hard = "", debtTo = "", debtDue = "";
        public bool firstRelease;
        public double debt;
        public readonly Dictionary<string, double> halfKnown = new Dictionary<string, double>(StringComparer.Ordinal);
        public string[] usualFaiths = Array.Empty<string>();
    }

    /// <summary>A faith: its name, the family it counts as, what it asks of a merchant and what it opens (the first of
    /// each list in the data, as one line each), and how it swears.</summary>
    public sealed class FaithDef
    {
        public string id = "", name = "", countsAs = "", asks = "", opens = "", swears = "";
        public bool firstRelease;
    }

    /// <summary>A past: its name, the stats it tilts (a stat set to a value), its gift in words (and the flag that
    /// carries it, if any), the thing in the chest, and the person who knows you.</summary>
    public sealed class PastDef
    {
        public string id = "", name = "", gift = "", giftFlag = "", chestThing = "", chestWhat = "", knows = "", knowsWhy = "";
        public readonly Dictionary<string, int> tilt = new Dictionary<string, int>(StringComparer.Ordinal);
    }

    /// <summary>The mark a known deed puts on a stat (story/self_793.json "deed_marks"): fought raiders off, Nerve up
    /// by one for thirty days. `faith`: only for a player of that faith ("christian" covers its kinds); "": anybody.</summary>
    public sealed class DeedMark
    {
        public string deed = "", stat = "", id = "", why = "", untilFlag = "", untilSaid = "", faith = "";
        public int size;
        public double days;
    }

    /// <summary>One of the five stats: its name, what it is, and the word for each of its five levels.</summary>
    public sealed class StatDef
    {
        public string id = "", name = "", what = "";
        public string[] levels = Array.Empty<string>();

        /// <summary>The word for so many pips ("ready" for three of Tongue), or "".</summary>
        public string Level(int pips) => pips >= 1 && pips <= levels.Length ? levels[pips - 1] : "";
    }

    /// <summary>
    /// THE SELF'S TABLES (Design/fate_faith_and_self.md sections 1, 2 and 6a): the peoples, faiths, pasts and stats a
    /// player is made of, from story/self_793.json, or from the small table here when the file cannot be read.
    /// </summary>
    public sealed class SelfBook
    {
        public const string File = "story/self_793.json";

        public readonly List<PeopleDef> peoples = new List<PeopleDef>();
        public readonly List<FaithDef> faiths = new List<FaithDef>();
        public readonly List<PastDef> pasts = new List<PastDef>();
        public readonly List<StatDef> stats = new List<StatDef>();
        public readonly List<DeedMark> deedMarks = new List<DeedMark>();

        /// <summary>The marks a deed of this kind puts on the player.</summary>
        public IEnumerable<DeedMark> DeedMarks(string deed)
        {
            foreach (var d in deedMarks)
                if (d.deed == deed)
                    yield return d;
        }
        /// <summary>What every stat starts at, and Troth (the data's "start").</summary>
        public int startStat = 1, startTroth = 3;
        /// <summary>The tables came from the writers' file (false: the small table in the code).</summary>
        public bool FromFile { get; private set; }

        static readonly ContentCache<SelfBook> cache = new ContentCache<SelfBook>("the self", Load);
        public static SelfBook Current => cache.Value;

        public SelfBook()
        {
            Fallback();
        }

        public PeopleDef People(string id) => peoples.Find(p => p.id == id);
        public FaithDef Faith(string id) => faiths.Find(f => f.id == id);
        public PastDef Past(string id) => pasts.Find(p => p.id == id);
        public StatDef Stat(string id) => stats.Find(s => s.id == id);

        /// <summary>The peoples a new game offers: those of the first release whose home port the game has (the test:
        /// "is this port in the tables"; null offers every first-release people).</summary>
        public List<PeopleDef> Offered(Func<string, bool> portInGame)
        {
            var list = new List<PeopleDef>();
            foreach (var p in peoples)
                if (p.firstRelease && (portInGame == null || portInGame(p.home)))
                    list.Add(p);
            if (list.Count == 0 && peoples.Count > 0)
                list.Add(peoples[0]);
            return list;
        }

        /// <summary>The faiths a people may begin in: its usual ones that the first release has; if none, the first
        /// release's faiths; never empty while the book has a faith.</summary>
        public List<FaithDef> FaithsFor(PeopleDef people)
        {
            var list = new List<FaithDef>();
            if (people != null)
                foreach (string id in people.usualFaiths)
                {
                    var f = Faith(id);
                    if (f != null && f.firstRelease)
                        list.Add(f);
                }
            if (list.Count == 0)
                foreach (var f in faiths)
                    if (f.firstRelease)
                        list.Add(f);
            if (list.Count == 0 && faiths.Count > 0)
                list.Add(faiths[0]);
            return list;
        }

        static SelfBook Load()
        {
            var book = new SelfBook();
            var root = ContentFiles.ParseObject(File);
            if (root == null)
                return book;                                              // (the small table; ContentFiles has said why)
            var read = new SelfBook();
            read.peoples.Clear();
            read.faiths.Clear();
            read.pasts.Clear();
            read.stats.Clear();
            var start = Json.Obj(root, "start");
            read.startStat = Clamp(ContentJson.Int(start, "every_stat", 1));
            read.startTroth = Clamp(ContentJson.Int(start, "troth", 3));
            foreach (var x in ContentJson.Rows(root, "peoples"))
            {
                var p = new PeopleDef
                {
                    id = ContentJson.Text(x, "id"), name = ContentJson.Text(x, "name"), home = ContentJson.Text(x, "home"),
                    firstTongue = ContentJson.Text(x, "first_tongue"), hard = ContentJson.Text(x, "hard"),
                    firstRelease = Json.Bool(x, "in_first_release"), usualFaiths = ContentJson.Strings(x, "usual_faiths"),
                };
                foreach (var kv in ContentJson.Pairs(x, "half_known"))
                    if (kv.Value is double level)
                        p.halfKnown[kv.Key] = level;
                var debt = Json.Obj(x, "debt");
                p.debt = ContentJson.Num(debt, "sum_d");
                p.debtTo = ContentJson.Text(debt, "to_office");
                p.debtDue = ContentJson.Text(debt, "due");
                if (p.id.Length > 0)
                    read.peoples.Add(p);
            }
            foreach (var x in ContentJson.Rows(root, "faiths"))
            {
                var f = new FaithDef
                {
                    id = ContentJson.Text(x, "id"), name = ContentJson.Text(x, "name"), countsAs = ContentJson.Text(x, "counts_as"),
                    swears = ContentJson.Text(x, "swears"), firstRelease = Json.Bool(x, "in_first_release"),
                };
                foreach (var a in ContentJson.Rows(x, "by_calendar"))
                {
                    f.asks = ContentJson.Text(a, "asks");
                    break;
                }
                foreach (var o in ContentJson.Rows(x, "opens"))
                {
                    f.opens = ContentJson.Text(o, "what");
                    break;
                }
                if (f.id.Length > 0)
                    read.faiths.Add(f);
            }
            foreach (var x in ContentJson.Rows(root, "pasts"))
            {
                var p = new PastDef { id = ContentJson.Text(x, "id"), name = ContentJson.Text(x, "name") };
                foreach (var kv in ContentJson.Pairs(x, "tilt"))
                    if (kv.Value is double v)
                        p.tilt[kv.Key] = Clamp((int)Math.Round(v));
                var gift = Json.Obj(x, "gift");
                p.gift = ContentJson.Text(gift, "what");
                p.giftFlag = ContentJson.Text(gift, "flag");
                var chest = Json.Obj(x, "chest");
                p.chestThing = ContentJson.Text(chest, "thing");
                p.chestWhat = ContentJson.Text(chest, "what");
                var knows = Json.Obj(x, "knows_you");
                p.knows = ContentJson.Text(knows, "place_id");
                p.knowsWhy = ContentJson.Text(knows, "why");
                if (p.id.Length > 0)
                    read.pasts.Add(p);
            }
            foreach (var x in ContentJson.Rows(root, "stats"))
            {
                var s = new StatDef
                {
                    id = ContentJson.Text(x, "id"), name = ContentJson.Text(x, "name"), what = ContentJson.Text(x, "what"),
                    levels = ContentJson.Strings(x, "levels"),
                };
                if (s.id.Length > 0)
                    read.stats.Add(s);
            }
            foreach (var x in ContentJson.Rows(root, "deed_marks"))
            {
                var d = new DeedMark
                {
                    deed = ContentJson.Text(x, "deed"), stat = ContentJson.Text(x, "stat"), id = ContentJson.Text(x, "id"), why = ContentJson.Text(x, "why"),
                    untilFlag = ContentJson.Text(x, "until_flag"), untilSaid = ContentJson.Text(x, "until"), faith = ContentJson.Text(x, "faith"),
                    size = ContentJson.Int(x, "size"), days = Json.Num(x, "days", 0.0),
                };
                if (d.deed.Length > 0 && d.id.Length > 0 && Self.IsStat(d.stat) && d.size != 0 && (d.days > 0.0 || d.untilFlag.Length > 0))
                    read.deedMarks.Add(d);
                else
                    ContentFiles.Warn("deed_mark:" + d.id, $"{File}: the deed mark '{d.id}' is not whole (a deed, an id, one of the five stats, a size, and days or until_flag): left out");
            }
            // a file that lacks a whole family is a half-written one: the small table stands in, and it is said
            bool whole = read.peoples.Count > 0 && read.faiths.Count > 0 && read.pasts.Count > 0;
            foreach (string id in Self.StatIds)
                whole &= read.Stat(id) != null;
            if (!whole)
            {
                ContentFiles.Warn("shape:" + File, $"{File} lacks peoples, faiths, pasts or one of the five stats: the game's own small table is used");
                return book;
            }
            read.FromFile = true;
            return read;
        }

        static int Clamp(int v) => Math.Max(1, Math.Min(5, v));

        /// <summary>The small table of the code: what the creation window offers when the writers' file cannot be read.</summary>
        void Fallback()
        {
            peoples.Add(new PeopleDef
            {
                id = "frisian", name = "Frisian", home = "dorestad", firstTongue = "old_frisian", firstRelease = true, debt = 1920,
                hard = "The plain start.", usualFaiths = new[] { "christian", "old_gods" },
            });
            faiths.Add(new FaithDef
            {
                id = "christian", name = "Christian, of Rome", countsAs = "christian", firstRelease = true,
                asks = "No loading, selling or carting on a Sunday.", opens = "Abbeys as hosts and as keepers of silver.",
            });
            faiths.Add(new FaithDef
            {
                id = "old_gods", name = "The old gods", countsAs = "old_gods", firstRelease = true,
                asks = "Offer at the turns of the year.", opens = "Halls and markets in the north.",
            });
            var steer = new PastDef { id = "steersmans_child", name = "The steersman's child", gift = "You read water.", chestThing = "Your father's lead and line" };
            steer.tilt["nerve"] = 2;
            steer.tilt["lore"] = 2;
            pasts.Add(steer);
            var clerk = new PastDef { id = "toll_clerk", name = "The toll-keeper's clerk", gift = "You know the tolls by heart.", chestThing = "A false weight you kept" };
            clerk.tilt["tongue"] = 3;
            pasts.Add(clerk);
            stats.Add(new StatDef { id = "tongue", name = "Tongue", what = "Talking a thing through.", levels = new[] { "halting", "plain", "ready", "persuasive", "silver" } });
            stats.Add(new StatDef { id = "name", name = "Name", what = "How you look and who stands behind you.", levels = new[] { "unknown", "known on the quay", "vouched for", "of good name", "named in halls" } });
            stats.Add(new StatDef { id = "nerve", name = "Nerve", what = "Holding firm.", levels = new[] { "easily turned", "steady enough", "firm", "hard to move", "unbending" } });
            stats.Add(new StatDef { id = "lore", name = "Lore", what = "What you have read, heard and can quote.", levels = new[] { "unlettered", "has heard things", "well taught", "learned", "deep" } });
            stats.Add(new StatDef { id = "troth", name = "Troth", what = "How well you keep the way of your own faith.", levels = new[] { "lapsed", "slack", "observant", "faithful", "devout" } });
        }
    }
}
