using System;
using System.Collections.Generic;
using System.Linq;
using Kapman.Game;

namespace Kapman.Encounters
{
    /// <summary>How the toll is taken on the merchant's sales at a port: in full, not at all this call (a charter), or
    /// halved for some days (a gift to the toll man).</summary>
    public enum TollMode { Sales, None, Eased }

    /// <summary>
    /// WHAT THE ENCOUNTER LAYER KEEPS (saved with the game): flags with their expiry day, the toll's ease or waiver at each
    /// port, the port calls (which port, how many times before), when each card was last shown and which "once" cards are
    /// done, the follow-up cards waiting for their day and port, the cards waiting to open (the arrival set, a drawn card)
    /// and the last game hour drawn. Days are game days since the start (as KapmanGame.Day).
    /// </summary>
    public sealed class EncounterState
    {
        public const double Forever = -1.0;

        readonly Dictionary<string, (double value, double until)> flags = new Dictionary<string, (double, double)>(StringComparer.Ordinal);
        readonly Dictionary<string, bool> waived = new Dictionary<string, bool>(StringComparer.Ordinal);
        readonly Dictionary<string, double> easedUntil = new Dictionary<string, double>(StringComparer.Ordinal);
        readonly Dictionary<string, int> visits = new Dictionary<string, int>(StringComparer.Ordinal);
        readonly Dictionary<string, double> lastShown = new Dictionary<string, double>(StringComparer.Ordinal);
        readonly HashSet<string> done = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>A follow-up card: due on a day, at a port (empty: wherever the merchant is).</summary>
        public sealed class Scheduled
        {
            public string card = "", port = "";
            public double day;
        }

        public readonly List<Scheduled> scheduled = new List<Scheduled>();

        /// <summary>Cards waiting to open, first first (the arrival set, then the visit's card; an hourly draw only when
        /// nothing waits).</summary>
        public readonly List<string> queue = new List<string>();

        /// <summary>The last game hour the hourly draw ran for (nothing is drawn again for an hour at or before it).</summary>
        public long lastHour = -1;

        /// <summary>The port of the call in progress ("" at sea), the calls there before this one, and when it began.</summary>
        public string callPort = "";
        public int callVisitsBefore;
        public double callStartDay;

        // ------------------------------------------------------------------ flags

        /// <summary>Sets a flag to a value; with days > 0 it lapses that many days from today (set on day 1 for 30: gone on day 31).</summary>
        public void SetFlag(string name, double value, double day, double days)
        {
            if (string.IsNullOrEmpty(name))
                return;
            flags[name] = (value, days > 0.0 ? day + days : Forever);
            FlagVersion++;
        }

        /// <summary>Adds to a flag's value ("flag &lt;name&gt; +n": a count the world keeps, such as st_dorestad_helped). A flag
        /// that lapses keeps its day; one never set, or already lapsed, starts from 0 and holds for good.</summary>
        public void AddFlag(string name, double by, double day)
        {
            if (string.IsNullOrEmpty(name))
                return;
            if (flags.TryGetValue(name, out var f) && (f.until == Forever || day < f.until))
                flags[name] = (f.value + by, f.until);
            else
                flags[name] = (by, Forever);
            FlagVersion++;
        }

        /// <summary>A flag's value today (0 if never set or lapsed).</summary>
        public double Flag(string name, double day)
        {
            if (name == null || !flags.TryGetValue(name, out var f))
                return 0.0;
            return f.until == Forever || day < f.until ? f.value : 0.0;
        }

        /// <summary>Drops the flags that have lapsed by today.</summary>
        public void ExpireFlags(double day)
        {
            foreach (var name in flags.Where(kv => kv.Value.until != Forever && day >= kv.Value.until).Select(kv => kv.Key).ToList())
            {
                flags.Remove(name);
                FlagVersion++;
            }
        }

        public int FlagCount => flags.Count;

        /// <summary>Counts up whenever a flag is set, a lapsed flag is dropped or the state is restored from a save: what a
        /// reader that keeps a list made from the flags (the unlocks, the chain journal) compares to know its list is old.
        /// A flag that lapses by its day changes nothing here until the hourly draw drops it (ExpireFlags).</summary>
        public int FlagVersion { get; private set; }

        /// <summary>Every flag that holds today (set, not lapsed, not 0) with its value, in the names' order: for the
        /// readers that ask by family (every unlock_ flag, every ch_ flag of a chain).</summary>
        public List<KeyValuePair<string, double>> Flags(double day)
        {
            var list = new List<KeyValuePair<string, double>>();
            foreach (var kv in flags)
                if (kv.Value.value != 0.0 && (kv.Value.until == Forever || day < kv.Value.until))
                    list.Add(new KeyValuePair<string, double>(kv.Key, kv.Value.value));
            list.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
            return list;
        }

        // ------------------------------------------------------------------ the toll

        /// <summary>
        /// The toll man's word: "sales" (the toll in full this call: a waiver ends), "none" (no toll on sales this call: it
        /// ends when the ship casts off) or "eased" (half the toll for days from today).
        /// </summary>
        public void SetToll(string port, TollMode mode, double day, double days)
        {
            if (string.IsNullOrEmpty(port))
                return;
            switch (mode)
            {
                case TollMode.None:
                    waived[port] = true;
                    break;
                case TollMode.Eased:
                    easedUntil[port] = Math.Max(easedUntil.TryGetValue(port, out double u) ? u : 0.0, day + Math.Max(0.0, days));
                    break;
                default:
                    waived.Remove(port);
                    break;
            }
        }

        /// <summary>The share of the zone's toll taken on the merchant's sales at a port today: 1, 0.5 (eased) or 0 (waived).</summary>
        public double TollFactor(string port, double day)
        {
            if (string.IsNullOrEmpty(port))
                return 1.0;
            if (waived.TryGetValue(port, out bool w) && w)
                return 0.0;
            return easedUntil.TryGetValue(port, out double until) && day < until ? 0.5 : 1.0;
        }

        // ------------------------------------------------------------------ port calls

        /// <summary>A call begins (the ship moored): the calls before it are counted, and this one added.</summary>
        public void BeginCall(string port, double day)
        {
            callPort = port ?? "";
            callVisitsBefore = Visits(callPort);
            visits[callPort] = callVisitsBefore + 1;
            callStartDay = day;
        }

        /// <summary>The call ends (cast off): a waived toll ends with it.</summary>
        public void EndCall()
        {
            if (!string.IsNullOrEmpty(callPort))
                waived.Remove(callPort);
            callPort = "";
            callVisitsBefore = 0;
        }

        /// <summary>Calls made at a port, the one in progress included.</summary>
        public int Visits(string port) => port != null && visits.TryGetValue(port, out int n) ? n : 0;

        /// <summary>The calls at a port before the one in progress (what a card's "port.visits" reads).</summary>
        public int VisitsBefore(string port) => port == callPort ? callVisitsBefore : Visits(port);

        // ------------------------------------------------------------------ cards

        // ------------------------------------------------------------------ the dead

        /// <summary>
        /// ONE OF THE STANDING CAST IS DEAD, OR GONE FOR GOOD (the user, 9 Oct 2026: "Maybe it you don't pay up for the
        /// 3rd time they kill Ravn?"). A flag says so: "gone_cast_crew_ravn" for "cast.crew.ravn". It is a flag so that
        /// it is saved with the deck, and a card's own conditions can ask it. What follows from it, for every card:
        /// he does not speak (EncounterSession.Open leaves his lines out); a card that leans on him does not come
        /// (<see cref="Ready"/>: EncounterDef.leans); and a text may be written for when he is gone, under its own key
        /// with ".without_ravn" after it (EncounterSession.Without).
        /// </summary>
        public static string GoneFlag(string castId)
        {
            var name = new System.Text.StringBuilder("gone_");
            foreach (char c in castId ?? "")
                name.Append(char.IsLetterOrDigit(c) ? c : '_');
            return name.ToString();
        }

        /// <summary>The last part of a cast id ("ravn" of "cast.crew.ravn"): what a text written for when he is gone is
        /// named by.</summary>
        public static string ShortName(string castId)
        {
            if (string.IsNullOrEmpty(castId))
                return "";
            int dot = castId.LastIndexOf('.');
            return dot >= 0 ? castId.Substring(dot + 1) : castId;
        }

        /// <summary>Is that one of the standing cast dead, or gone for good?</summary>
        public bool Gone(string castId, double day) => !string.IsNullOrEmpty(castId) && Flag(GoneFlag(castId), day) != 0.0;

        /// <summary>Can this card come today: not a "once" card already shown, past its cool-down, and nobody it leans
        /// on is dead.</summary>
        public bool Ready(EncounterDef card, double day)
        {
            if (card == null || (card.once && done.Contains(card.id)))
                return false;
            foreach (string cast in card.leans)
                if (Gone(cast, day))
                    return false;
            return card.cooldownDays <= 0.0 || !lastShown.TryGetValue(card.id, out double last) || day - last >= card.cooldownDays;
        }

        /// <summary>A card was shown today.</summary>
        public void Shown(EncounterDef card, double day)
        {
            lastShown[card.id] = day;
            if (card.once)
                done.Add(card.id);
        }

        public bool WasShown(string card) => card != null && lastShown.ContainsKey(card);

        /// <summary>A follow-up card on a day, at a port (null or "": wherever the merchant is then).</summary>
        public void Schedule(string card, double day, string port)
        {
            if (!string.IsNullOrEmpty(card))
                scheduled.Add(new Scheduled { card = card, day = day, port = port ?? "" });
        }

        /// <summary>Puts a card in line to open (not twice).</summary>
        public void Enqueue(string card, bool first = false)
        {
            if (string.IsNullOrEmpty(card) || queue.Contains(card))
                return;
            if (first)
                queue.Insert(0, card);
            else
                queue.Add(card);
        }

        // ------------------------------------------------------------------ the save

        public Dictionary<string, object> Capture()
        {
            var f = new Dictionary<string, object>();
            foreach (var kv in flags.OrderBy(kv => kv.Key, StringComparer.Ordinal))
                f[kv.Key] = new List<object> { kv.Value.value, kv.Value.until };
            var w = new List<object>();
            foreach (var kv in waived.Where(kv => kv.Value).OrderBy(kv => kv.Key, StringComparer.Ordinal))
                w.Add(kv.Key);
            var eased = new Dictionary<string, object>();
            foreach (var kv in easedUntil.OrderBy(kv => kv.Key, StringComparer.Ordinal))
                eased[kv.Key] = kv.Value;
            var v = new Dictionary<string, object>();
            foreach (var kv in visits.OrderBy(kv => kv.Key, StringComparer.Ordinal))
                v[kv.Key] = kv.Value;
            var shown = new Dictionary<string, object>();
            foreach (var kv in lastShown.OrderBy(kv => kv.Key, StringComparer.Ordinal))
                shown[kv.Key] = kv.Value;
            var s = new List<object>();
            foreach (var x in scheduled)
                s.Add(new Dictionary<string, object> { ["card"] = x.card, ["day"] = x.day, ["port"] = x.port });
            return new Dictionary<string, object>
            {
                ["flags"] = f, ["waived"] = w, ["eased"] = eased, ["visits"] = v, ["shown"] = shown,
                ["done"] = done.OrderBy(x => x, StringComparer.Ordinal).Cast<object>().ToList(), ["scheduled"] = s,
                ["queue"] = queue.Cast<object>().ToList(), ["last_hour"] = (double)lastHour,
                ["call_port"] = callPort, ["call_before"] = callVisitsBefore, ["call_start"] = callStartDay,
            };
        }

        public void Restore(Dictionary<string, object> d)
        {
            flags.Clear();
            FlagVersion++;
            waived.Clear();
            easedUntil.Clear();
            visits.Clear();
            lastShown.Clear();
            done.Clear();
            scheduled.Clear();
            queue.Clear();
            lastHour = -1;
            callPort = "";
            callVisitsBefore = 0;
            callStartDay = 0.0;
            if (d == null)
                return;
            foreach (var kv in Json.Obj(d, "flags") ?? new Dictionary<string, object>())
                if (kv.Value is List<object> l && l.Count >= 2 && l[0] is double value && l[1] is double until)
                    flags[kv.Key] = (value, until);
            foreach (var x in Json.Arr(d, "waived") ?? new List<object>())
                if (x is string port)
                    waived[port] = true;
            foreach (var kv in Json.Obj(d, "eased") ?? new Dictionary<string, object>())
                if (kv.Value is double u)
                    easedUntil[kv.Key] = u;
            foreach (var kv in Json.Obj(d, "visits") ?? new Dictionary<string, object>())
                if (kv.Value is double n)
                    visits[kv.Key] = (int)n;
            foreach (var kv in Json.Obj(d, "shown") ?? new Dictionary<string, object>())
                if (kv.Value is double day)
                    lastShown[kv.Key] = day;
            foreach (var x in Json.Arr(d, "done") ?? new List<object>())
                if (x is string id)
                    done.Add(id);
            foreach (var x in Json.Arr(d, "scheduled") ?? new List<object>())
                scheduled.Add(new Scheduled { card = Json.Str(x, "card"), day = Json.Num(x, "day"), port = Json.Str(x, "port") });
            foreach (var x in Json.Arr(d, "queue") ?? new List<object>())
                if (x is string id)
                    queue.Add(id);
            lastHour = (long)Json.Num(d, "last_hour", -1.0);
            callPort = Json.Str(d, "call_port");
            callVisitsBefore = (int)Json.Num(d, "call_before");
            callStartDay = Json.Num(d, "call_start");
        }
    }
}
