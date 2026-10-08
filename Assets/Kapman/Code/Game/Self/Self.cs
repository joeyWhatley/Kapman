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
        /// "" for none chosen: a face of the same pool stands (FrameArt.SelfUnchosen), not the old drawn portrait (the
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
            return new Dictionary<string, object>
            {
                ["people"] = People, ["faith"] = Faith, ["past"] = Past, ["chosen"] = Chosen, ["called"] = Called, ["face"] = Face,
                ["stats"] = s, ["tongues"] = t, ["seen"] = w, ["met"] = m,
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
