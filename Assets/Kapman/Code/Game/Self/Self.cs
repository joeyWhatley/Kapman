// 7 Oct 2026: THE SELF (the user: "in character customisation we will add culture, religion and personal background,
// there will be a simple stat system for specific dialogue options"). One record for the life being played: people,
// faith, past, the five stats, the tongues, the words of other tongues seen, and the people of the places met. It is
// kept in the saved game as a part of the content (ContentSave, key "self").
using System;
using System.Collections.Generic;

namespace Kapman.Game
{
    /// <summary>
    /// THE SELF: who the player is. Three picks made once, before the first morning (people, faith, past), and five
    /// stats of one to five pips: Tongue, Name, Nerve, Lore, Troth. A stat rises by use and never by spending points:
    /// each win of a check made by it adds a share of a pip, a larger share against someone who resisted, a smaller one
    /// the higher the stat already stands. Until a game has chosen, the self is the plain start: a Frisian of Dorestat,
    /// a Christian, with no past picked, so every other scene and every test reads something sane.
    /// The cards read it as me.tongue, me.name, me.nerve, me.lore, me.troth (numbers) and me.people, me.faith, me.past
    /// (text), and tongue.&lt;name&gt; for how well a tongue is known (0 to 1).
    /// </summary>
    public static class Self
    {
        public static readonly string[] StatIds = { "tongue", "name", "nerve", "lore", "troth" };
        public const int MinStat = 1, MaxStat = 5;
        /// <summary>A check counts as won against someone who resisted when its written chance is this or less.</summary>
        public const double ResistedChance = 0.55;
        /// <summary>The share of a pip a win adds at one pip: against one who resisted, and against one who did not.</summary>
        public const double HardGain = 1.0 / 3.0, EasyGain = 1.0 / 10.0;
        /// <summary>A piece of another tongue seen this often stays English from the start.</summary>
        public const int StaysAfter = 10;

        public sealed class MetRecord
        {
            public int times;
            public double first, last;
        }

        public static string People { get; private set; } = "frisian";
        public static string Faith { get; private set; } = "christian";
        public static string Past { get; private set; } = "";
        /// <summary>The name she or he goes by (asked at the start: "And your name?"); "" until it is given. Not the
        /// stat called Name, which is how far one's name carries.</summary>
        public static string Called { get; private set; } = "";
        /// <summary>The three picks have been made (a new game asks; a loaded game brings them).</summary>
        public static bool Chosen { get; private set; }
        /// <summary>Counts up with every change (a panel's stamp).</summary>
        public static int Version { get; private set; }

        static readonly Dictionary<string, double> stats = new Dictionary<string, double>(StringComparer.Ordinal);
        static readonly Dictionary<string, double> tongues = new Dictionary<string, double>(StringComparer.Ordinal);
        static readonly Dictionary<string, int> seen = new Dictionary<string, int>(StringComparer.Ordinal);
        static readonly Dictionary<string, MetRecord> met = new Dictionary<string, MetRecord>(StringComparer.Ordinal);

        static Self()
        {
            Plain();
        }

        // ------------------------------------------------------------------ the picks

        /// <summary>The picks are made: the stats start as the book has them, the past tilts them, the people's tongues
        /// are known, and the world takes the people's colours. An id the book does not have falls back to its first.</summary>
        public static void Begin(string people, string faith, string past)
        {
            var book = SelfBook.Current;
            var p = book.People(people) ?? (book.peoples.Count > 0 ? book.peoples[0] : null);
            var f = book.Faith(faith) ?? (book.FaithsFor(p).Count > 0 ? book.FaithsFor(p)[0] : null);
            var was = book.Past(past);
            People = p != null ? p.id : "frisian";
            Faith = f != null ? f.id : "christian";
            Past = was != null ? was.id : "";
            Start(book, p, was);
            Chosen = true;
            MapPalette.SetPeople(People);
            Version++;
        }

        /// <summary>The name she or he goes by is given (or changed).</summary>
        public static void Call(string name)
        {
            Called = (name ?? "").Trim();
            Version++;
        }

        /// <summary>The player's own face: the art slot of the picture chosen at the very start ("portrait_pool/p04.png");
        /// "" for none chosen: the first face of the same pool stands (FrameArt.SelfUnchosen), not the old drawn portrait (the
        /// user, 7 Oct 2026, late: "replace the AI image of the player and make it a choice at the very start out of all
        /// the choices"; 8 Oct, seeing the drawn one in a game that had skipped the choice: "Why can I see the old ai
        /// generated portrait").</summary>
        public static string Face { get; private set; } = "";

        public static void SetFace(string slot)
        {
            Face = slot ?? "";
            Version++;
        }

        static void Start(SelfBook book, PeopleDef p, PastDef was)
        {
            stats.Clear();
            tongues.Clear();
            foreach (string id in StatIds)
                stats[id] = id == "troth" ? book.startTroth : book.startStat;
            if (was != null)
                foreach (var kv in was.tilt)
                    if (stats.ContainsKey(kv.Key))
                        stats[kv.Key] = Math.Max(stats[kv.Key], Math.Min(MaxStat, kv.Value));
            if (p != null)
            {
                if (p.firstTongue.Length > 0)
                    tongues[p.firstTongue] = 1.0;
                foreach (var kv in p.halfKnown)
                    tongues[kv.Key] = Math.Max(0.0, Math.Min(1.0, kv.Value));
            }
            // (the minster's schooling is Latin letters: the data's gift for that past)
            if (was != null && was.id == "minster_schooled")
                tongues["latin"] = 1.0;
        }

        /// <summary>The plain start, not chosen: a Frisian of Dorestat, a Christian, no past.</summary>
        static void Plain()
        {
            People = "frisian";
            Faith = "christian";
            Past = "";
            Called = "";
            Face = "";
            Chosen = false;
            stats.Clear();
            tongues.Clear();
            seen.Clear();
            met.Clear();
            marks.Clear();
            foreach (string id in StatIds)
                stats[id] = id == "troth" ? 3.0 : 1.0;
            tongues["old_frisian"] = 1.0;
            tongues["old_english"] = 0.5;
            tongues["old_frankish"] = 0.5;
            tongues["old_saxon"] = 0.5;
        }

        // ------------------------------------------------------------------ the stats

        public static bool IsStat(string id) => id != null && Array.IndexOf(StatIds, id) >= 0;

        /// <summary>A stat with the part of a pip it has gathered (1.0 to 5.0).</summary>
        public static double StatRaw(string id) => id != null && stats.TryGetValue(id, out double v) ? v : MinStat;

        /// <summary>A stat in pips, 1 to 5.</summary>
        public static int Stat(string id) => Math.Max(MinStat, Math.Min(MaxStat, (int)Math.Floor(StatRaw(id) + 1e-9)));

        /// <summary>Sets a stat (a test, a tool, a rule that takes a pip away).</summary>
        public static void SetStat(string id, double value)
        {
            if (!IsStat(id))
                return;
            stats[id] = Math.Max(MinStat, Math.Min(MaxStat, value));
            Version++;
        }

        /// <summary>The share of a pip a win adds to a stat that stands at `pips`, on a check of this written chance.</summary>
        public static double Gain(int pips, double chance) =>
            (chance <= ResistedChance + 1e-9 ? HardGain : EasyGain) / Math.Max(1, pips);

        /// <summary>
        /// A check made by a stat was won: it rises a little (by use, never by points). Troth is not raised here: it
        /// follows how the faith's days and duties are kept. True when the win filled a pip.
        /// </summary>
        public static bool Used(string stat, double chance)
        {
            if (!IsStat(stat) || stat == "troth")
                return false;
            int before = Stat(stat);
            if (before >= MaxStat)
                return false;
            stats[stat] = Math.Min(MaxStat, StatRaw(stat) + Gain(before, chance));
            Version++;
            return Stat(stat) > before;
        }

        // ------------------------------------------------------------------ the tongues

        /// <summary>The older names some cards use for a tongue.</summary>
        public static string TongueId(string name)
        {
            switch (name)
            {
                case "norse": return "old_norse";
                case "english": return "old_english";
                case "frisian": return "old_frisian";
                case "frankish": return "old_frankish";
                case "saxon": return "old_saxon";
                default: return name ?? "";
            }
        }

        /// <summary>How well a tongue is held, 0 to 1, without what follows from its kin.</summary>
        public static double TongueHeld(string tongue)
        {
            tongue = TongueId(tongue);
            double v = tongues.TryGetValue(tongue, out double held) ? held : 0.0;
            if (tongue == "latin" && Chest.KnowsLatin)
                v = 1.0;
            return v;
        }

        /// <summary>How well a tongue is followed, 0 to 1: held outright, or through a tongue near to it (a Frisian
        /// follows a good deal of English).</summary>
        public static double TongueLevel(string tongue)
        {
            tongue = TongueId(tongue);
            double held = TongueHeld(tongue);
            if (held >= 1.0)
                return held;
            // (the tongues' book knows which tongue is near which)
            return Math.Max(held, TongueBook.Current.Follow(tongue, TongueHeld));
        }

        public static void SetTongue(string tongue, double level)
        {
            tongue = TongueId(tongue);
            if (tongue.Length == 0)
                return;
            tongues[tongue] = Math.Max(0.0, Math.Min(1.0, level));
            Version++;
        }

        /// <summary>A tongue's plain name: the tongues' book's, or its id made plain ("old_norse" is "Norse").</summary>
        public static string TongueName(string id)
        {
            id = TongueId(id);
            if (id.Length == 0)
                return "";
            string named = TongueBook.Current.Name(id);
            if (!string.IsNullOrEmpty(named))
                return named;
            string s = (id.StartsWith("old_", StringComparison.Ordinal) ? id.Substring(4) : id).Replace('_', ' ');
            return char.ToUpperInvariant(s[0]) + s.Substring(1);
        }

        /// <summary>The tongues held at all, by id.</summary>
        public static IEnumerable<KeyValuePair<string, double>> Tongues => tongues;

        // ------------------------------------------------------------------ words of other tongues seen

        /// <summary>A piece of speech as the record keeps it: lower case, without the marks at its ends.</summary>
        public static string Piece(string words) => (words ?? "").Trim().Trim('.', ',', ';', ':', '!', '?', '"', '\'').ToLowerInvariant();

        /// <summary>How often a piece of another tongue has been seen to turn.</summary>
        public static int Seen(string words) => seen.TryGetValue(Piece(words), out int n) ? n : 0;

        /// <summary>A piece of another tongue was seen (a line turned, a sprinkled word read).</summary>
        public static void See(string words)
        {
            string key = Piece(words);
            if (key.Length == 0)
                return;
            seen[key] = Seen(key) + 1;
            Version++;
        }

        public static int SeenCount => seen.Count;

        /// <summary>Every piece seen, with how often.</summary>
        public static IEnumerable<KeyValuePair<string, int>> SeenAll => seen;

        // ------------------------------------------------------------------ the people of the places met

        /// <summary>What is kept of a person of the places files (null: never spoken to).</summary>
        public static MetRecord Met(string personId) => personId != null && met.TryGetValue(personId, out var m) ? m : null;

        public static IEnumerable<KeyValuePair<string, MetRecord>> MetAll => met;

        /// <summary>The player spoke with a person of the places files on this game day.</summary>
        public static void MarkMet(string personId, double day)
        {
            if (string.IsNullOrEmpty(personId))
                return;
            if (!met.TryGetValue(personId, out var m))
                met[personId] = m = new MetRecord { first = day };
            m.times++;
            m.last = day;
            Version++;
        }

        // ------------------------------------------------------------------ marks: what events put on a stat

        /// <summary>
        /// A MARK ON A STAT (the user, 7 Oct 2026: "Stats can be effected by in game events with caps and dynamic
        /// decisions and what not like all the other games that have implemented such mechanics"; the rule is
        /// Design/fixes/stats_events.md). A named thing an event puts on a stat: its size (one, rarely two, up or
        /// down), a cause the player can read, and an end: a day, or a flag set somewhere ("until the stick is
        /// clean"). Never "for good": that is a change of the base. The same mark put on twice starts its time again.
        /// </summary>
        public sealed class StatMark
        {
            public string id = "", stat = "", why = "", untilFlag = "", untilSaid = "";
            public int size;
            /// <summary>The game day it ends on; below 0: it has no day (it ends by its flag).</summary>
            public double until = -1.0;

            /// <summary>Whole days it still has from `today`; -1 for a mark that ends by its flag.</summary>
            public int DaysLeft(double today) => until < 0.0 ? -1 : Math.Max(0, (int)Math.Ceiling(until - today));
        }

        /// <summary>All the marks on a stat together move it by no more than this, either way: the third good thing does
        /// nothing until one ends.</summary>
        public const int MarkCap = 2;

        static readonly List<StatMark> marks = new List<StatMark>();

        /// <summary>The game day, as the marks count it (0 where no game runs).</summary>
        public static double Now => KapmanGame.Current != null ? KapmanGame.Current.Day : 0.0;

        /// <summary>Puts a mark on a stat. `days` above 0: it ends then; else it ends when `untilFlag` is set; a mark
        /// given neither has thirty days, for a mark must end. Null if the stat or the size is none.</summary>
        public static StatMark Mark(string stat, int size, string id, double days, string untilFlag, string why, double today, string untilSaid = "")
        {
            if (!IsStat(stat) || size == 0 || string.IsNullOrEmpty(id))
                return null;
            marks.RemoveAll(m => m.id == id);
            var mark = new StatMark
            {
                id = id, stat = stat, size = Math.Max(-MarkCap, Math.Min(MarkCap, size)), why = why ?? "",
                until = days > 0.0 ? today + days : -1.0, untilFlag = untilFlag ?? "", untilSaid = untilSaid ?? "",
            };
            if (mark.until < 0.0 && mark.untilFlag.Length == 0)
                mark.until = today + 30.0;
            marks.Add(mark);
            Version++;
            return mark;
        }

        /// <summary>Takes a mark off. False if it was not on.</summary>
        public static bool Unmark(string id)
        {
            if (marks.RemoveAll(m => m.id == id) == 0)
                return false;
            Version++;
            return true;
        }

        static bool Over(StatMark m, double today) =>
            (m.until >= 0.0 && today >= m.until) || (m.untilFlag.Length > 0 && ContentFlags.Ready && ContentFlags.Has(m.untilFlag));

        /// <summary>The marks that lie on a stat now (null: on any), the ended ones dropped.</summary>
        public static List<StatMark> MarksOn(string stat, double today)
        {
            if (marks.RemoveAll(m => Over(m, today)) > 0)
                Version++;
            return marks.FindAll(m => stat == null || m.stat == stat);
        }

        public static List<StatMark> MarksOn(string stat) => MarksOn(stat, Now);

        public static bool HasMark(string id) => MarksOn(null, Now).Exists(m => m.id == id);

        /// <summary>What the marks on a stat come to, within the cap.</summary>
        public static int MarkSum(string stat, double today)
        {
            int sum = 0;
            foreach (var m in MarksOn(stat, today))
                sum += m.size;
            return Math.Max(-MarkCap, Math.Min(MarkCap, sum));
        }

        /// <summary>The marks on a stat would move it further than the cap lets them.</summary>
        public static bool Capped(string stat, double today)
        {
            int sum = 0;
            foreach (var m in MarksOn(stat, today))
                sum += m.size;
            return Math.Abs(sum) > MarkCap;
        }

        /// <summary>A stat as it is now: the base and what lies on it, never under 1 nor over 5. It is what a choice
        /// asks for and what a chance leans on; only growth by use moves the base.</summary>
        public static int StatNow(string id, double today) => Math.Max(MinStat, Math.Min(MaxStat, Stat(id) + MarkSum(id, today)));

        public static int StatNow(string id) => StatNow(id, Now);

        /// <summary>A stat's name as the book has it ("Nerve").</summary>
        public static string StatName(string id)
        {
            string name = SelfBook.Current.Stat(id)?.name;
            return !string.IsNullOrEmpty(name) ? name : string.IsNullOrEmpty(id) ? "" : char.ToUpperInvariant(id[0]) + id.Substring(1);
        }

        /// <summary>How long a mark has yet, in words: "for 12 days", "until the stick is clean", "for a time".</summary>
        public static string Lasts(StatMark m, double today)
        {
            if (m == null)
                return "";
            if (m.untilSaid.Length > 0)
                return m.untilSaid;
            int left = m.DaysLeft(today);
            return left < 0 ? "for a time" : left == 1 ? "for a day" : $"for {left} days";
        }

        /// <summary>A mark as a sentence for the log and the card: "Your Nerve is up by one for 30 days: you fought
        /// raiders off."</summary>
        public static string Said(StatMark m, double today)
        {
            if (m == null)
                return null;
            string by = Math.Abs(m.size) == 1 ? "one" : "two";
            string said = $"Your {StatName(m.stat)} is {(m.size > 0 ? "up" : "down")} by {by} {Lasts(m, today)}";
            return m.why.Length > 0 ? said + ": " + m.why + "." : said + ".";
        }

        /// <summary>A mark's name made plain where nobody has written its cause ("hid_under_the_sail" is "hid under the
        /// sail").</summary>
        public static string PlainCause(string id)
        {
            string s = id ?? "";
            int dot = s.LastIndexOf('.');
            if (dot >= 0)
                s = s.Substring(dot + 1);
            return s.Replace('_', ' ');
        }

        /// <summary>A deed that is known (reputation_793.json "deeds") puts the marks the self's book gives it
        /// (story/self_793.json "deed_marks"): fought raiders off, Nerve up by one for a month.</summary>
        public static void MarkFromDeed(string kind)
        {
            double today = Now;
            foreach (var d in SelfBook.Current.DeedMarks(kind))
                if (d.faith.Length == 0 || Faith.StartsWith(d.faith, StringComparison.Ordinal))
                    Mark(d.stat, d.size, d.id, d.days, d.untilFlag, d.why, today, d.untilSaid);
        }

        /// <summary>The body's own marks, looked at each day of the voyage: no bread aboard, the sickness aboard. Each
        /// lies on a stat while it is so and is gone the day it is mended.</summary>
        public static void Body(bool hungry, bool sick)
        {
            double today = Now;
            if (hungry)
                Mark("nerve", -1, "body.hungry", 2.0, "", "there is no bread aboard", today, "until the men are fed");
            else
                Unmark("body.hungry");
            if (sick)
                Mark("tongue", -1, "body.sick", 2.0, "", "there is sickness aboard", today, "until the crew is well");
            else
                Unmark("body.sick");
        }

        // ------------------------------------------------------------------ the saved game

        public static Dictionary<string, object> Capture()
        {
            var s = new Dictionary<string, object>();
            foreach (var kv in stats)
                s[kv.Key] = kv.Value;
            var t = new Dictionary<string, object>();
            foreach (var kv in tongues)
                t[kv.Key] = kv.Value;
            var w = new Dictionary<string, object>();
            foreach (var kv in seen)
                w[kv.Key] = (double)kv.Value;
            var m = new Dictionary<string, object>();
            foreach (var kv in met)
                m[kv.Key] = new Dictionary<string, object> { ["times"] = (double)kv.Value.times, ["first"] = kv.Value.first, ["last"] = kv.Value.last };
            var on = new List<object>();
            foreach (var k in marks)
                on.Add(new Dictionary<string, object>
                {
                    ["id"] = k.id, ["stat"] = k.stat, ["size"] = (double)k.size, ["until"] = k.until, ["flag"] = k.untilFlag, ["why"] = k.why, ["said"] = k.untilSaid,
                });
            return new Dictionary<string, object>
            {
                ["people"] = People, ["faith"] = Faith, ["past"] = Past, ["chosen"] = Chosen, ["called"] = Called, ["face"] = Face,
                ["stats"] = s, ["tongues"] = t, ["seen"] = w, ["met"] = m, ["marks"] = on,
            };
        }

        /// <summary>A loaded game's self. A save from before the self was kept (null) is the plain start, taken as chosen:
        /// a game already under way is not asked who it is.</summary>
        public static void Restore(object saved)
        {
            Plain();
            Chosen = true;
            if (saved is Dictionary<string, object> d)
            {
                People = Json.Str(d, "people", People);
                Faith = Json.Str(d, "faith", Faith);
                Past = Json.Str(d, "past", Past);
                Called = Json.Str(d, "called", "");
                Face = Json.Str(d, "face", "");
                Chosen = Json.Bool(d, "chosen", true);
                if (Json.Obj(d, "stats") is Dictionary<string, object> s)
                    foreach (string id in StatIds)
                        stats[id] = Math.Max(MinStat, Math.Min(MaxStat, Json.Num(s, id, stats[id])));
                if (Json.Obj(d, "tongues") is Dictionary<string, object> t)
                {
                    tongues.Clear();
                    foreach (var kv in t)
                        if (kv.Value is double level)
                            tongues[kv.Key] = Math.Max(0.0, Math.Min(1.0, level));
                }
                if (Json.Obj(d, "seen") is Dictionary<string, object> w)
                    foreach (var kv in w)
                        if (kv.Value is double n)
                            seen[kv.Key] = (int)n;
                if (Json.Obj(d, "met") is Dictionary<string, object> m)
                    foreach (var kv in m)
                        if (kv.Value is Dictionary<string, object> r)
                            met[kv.Key] = new MetRecord { times = (int)Json.Num(r, "times"), first = Json.Num(r, "first"), last = Json.Num(r, "last") };
                foreach (var x in Json.Arr(d, "marks") ?? new List<object>())
                    if (x is Dictionary<string, object> k && IsStat(Json.Str(k, "stat")) && Json.Str(k, "id").Length > 0)
                        marks.Add(new StatMark
                        {
                            id = Json.Str(k, "id"), stat = Json.Str(k, "stat"), size = (int)Json.Num(k, "size"), until = Json.Num(k, "until", -1.0),
                            untilFlag = Json.Str(k, "flag", ""), why = Json.Str(k, "why", ""), untilSaid = Json.Str(k, "said", ""),
                        });
            }
            MapPalette.SetPeople(People);
            Version++;
        }

        /// <summary>Back to the plain start, not chosen (a new game; a test).</summary>
        public static void Reset()
        {
            Plain();
            Version++;
        }

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void OnPlay()
        {
            Plain();
            Version = 0;
            ContentSave.Register("self", Capture, Restore);
        }
    }
}
