using System;
using System.Collections.Generic;
using System.Linq;
using Kapman.Game.Sim;
using UnityEngine;

namespace Kapman.Game
{
    // ported 4 Oct: unchanged from the first build. Everything here is in the world frame (WorldFrame.ToWorld of lat/lon,
    // Kapman.World in the rebuild's Core) or in the game's tables; CheckVoyages(x, z) takes world metres from its caller
    // (KapmanGame each game hour), which is where the ship's scene position is turned into the world: the 3D hand-off.

    /// <summary>Something written in the chest: a text, or a riddle (whose text it carries), with where it came from and
    /// how the merchant is getting on with it.</summary>
    public sealed class ChestItem
    {
        public int id;
        /// <summary>"text" or "riddle".</summary>
        public string kind;
        /// <summary>The text's or riddle's id in the tables.</summary>
        public string def;
        public double got;
        /// <summary>Who gave or sold it ("a monk of Lindisfarena"), and the port it was had at (an id; null at sea).</summary>
        public string from, at;
        /// <summary>A riddle: tries left; answered; resting (out of tries until something new is learned).</summary>
        public int tries;
        public bool solved, resting;
        /// <summary>A Latin text read out by a monk: its English is open from then on.</summary>
        public bool readToYou;
        /// <summary>Answered right, its reward waits at the giver's port.</summary>
        public bool rewardDue;
        /// <summary>The answers tried (thing ids: struck through), and the words marked as clues (their places in the text).</summary>
        public readonly List<string> tried = new List<string>();
        public readonly HashSet<int> marks = new HashSet<int>();
        /// <summary>What was already known when it came to rest (so only something new wakes it).</summary>
        public readonly HashSet<string> restKnown = new HashSet<string>();
        public int restPorts, restTexts;
        /// <summary>Runes: the sound chosen under each rune (its place in the text -> its place in the rune's sounds), and
        /// whether the whole has been read.</summary>
        public readonly Dictionary<int, int> runeChoice = new Dictionary<int, int>();
        public bool runesRead;

        public TextDef Text => kind == "text" ? ChestData.Text(def) : kind == "verse" ? ChestData.Text(ChestData.Verse(def)?.textId) : ChestData.Text(ChestData.Riddle(def)?.textId);
        public RiddleDef Riddle => kind == "riddle" ? ChestData.Riddle(def) : null;
        public VerseDef Verse => kind == "verse" ? ChestData.Verse(def) : null;
        /// <summary>A verse: the kennings already given their meaning (their places in the verse).</summary>
        public readonly HashSet<int> slotsDone = new HashSet<int>();
        /// <summary>A riddle of two or three answers: the things put in each place so far (judged together).</summary>
        public string[] filled;
    }

    /// <summary>A note of the merchant's own: words, joined to riddles, texts, places and things ("riddle:ID", "text:ID",
    /// "port:ID", "thing:ID"), and maybe pinned to a place on the map.</summary>
    public sealed class PlayerNote
    {
        public int id;
        public string text = "";
        public readonly List<string> links = new List<string>();
        public double day;
        /// <summary>Pinned to the map here (real metres), or NaN.</summary>
        public double x = double.NaN, z = double.NaN;
    }

    /// <summary>
    /// THE CHEST (Plans/map_play.md) - everything written the merchant keeps aboard, and what they make of it:
    ///   - texts and riddles, in their own tongue; the switch to English is the merchant's reading: at once for their own
    ///     tongue and runes; Latin (and Irish) only once learned (lessons at monasteries) or read out by a monk;
    ///   - riddles answered by dragging a thing the merchant knows onto them. A few tries each; out of tries, a riddle rests
    ///     till something new is learned that bears on it (a good seen, a port visited, a text read, news heard); then the
    ///     tries come back. Hard ones pay big: a reward given at the giver's port when the answer is brought back, or at
    ///     once where the answer is itself the key (a price tip, another text);
    ///   - the merchant's own notes: words joined to riddles, texts, places and things, pinned to the map, searched.
    /// The words round the texts (who gives a riddle, what's said) are the user's to write.
    /// </summary>
    public static class Chest
    {
        /// <summary>Lessons at monasteries before Latin can be read.</summary>
        public const int LessonsForLatin = 6;

        static readonly List<ChestItem> items = new List<ChestItem>();
        static readonly List<PlayerNote> notes = new List<PlayerNote>();
        static readonly Dictionary<string, int> tollFree = new Dictionary<string, int>();        // port id -> the last year it holds
        static readonly List<(int a, int b)> threads = new List<(int, int)>();                  // strings between pinned notes
        static readonly HashSet<string> foundLegs = new HashSet<string>();                     // old texts' places found by sailing there
        static int nextId, nextNote;
        static double lastVerseDay = double.MinValue;
        static bool beenAtSea;                                                                  // (things seen at sea are known after)
        static readonly HashSet<string> promised = new HashSet<string>();                     // big rewards won, for their own steps

        public static bool KnowsLatin { get; private set; }
        public static int LatinLessons { get; private set; }
        /// <summary>Goes up whenever the chest changes (views redraw).</summary>
        public static int Version { get; private set; }
        public static event Action Changed;

        public static IReadOnlyList<ChestItem> Items => items;
        public static IReadOnlyList<PlayerNote> Notes => notes;

        static double Today => KapmanGame.Current != null ? KapmanGame.Current.Day : 0.0;
        static int Year => KapmanGame.Current != null ? KapmanGame.Current.Year : 793;

        /// <summary>Something outside the chest changed what it shows (a dig): the views redraw.</summary>
        public static void Touched() => Touch();

        static void Touch()
        {
            Version++;
            Changed?.Invoke();
        }

        // ------------------------------------------------------------------------------------------ having things

        public static bool Has(string def) => def != null && items.Exists(i => i.def == def);
        public static ChestItem Get(int id) => items.Find(i => i.id == id);

        /// <summary>Puts a text or a riddle in the chest (once), from someone, at a port (or at sea).</summary>
        public static ChestItem Give(string kind, string def, string from, string at, bool quiet = false)
        {
            if (Has(def) || (kind == "riddle" ? ChestData.Riddle(def) == null : kind == "verse" ? ChestData.Verse(def) == null : ChestData.Text(def) == null))
                return null;
            var r = kind == "riddle" ? ChestData.Riddle(def) : null;
            var item = new ChestItem { id = nextId++, kind = kind, def = def, got = Today, from = from, at = at, tries = r != null ? r.tries : 0 };
            items.Add(item);
            var t = item.Text;
            if (!quiet)
                Voyage.Say($"You have {(kind == "riddle" ? "a riddle" : kind == "verse" ? "a verse" : "a text")} on a {t?.FormName ?? "leaf"}{(string.IsNullOrEmpty(from) ? "" : " from " + from)} in the chest now.");
            CheckWakes();
            Touch();
            return item;
        }

        // ------------------------------------------------------------------------------------------ reading

        /// <summary>Tongues the merchant must learn first (or have read out): Latin and Irish, Greek, Arabic. Their own
        /// tongue and its neighbours (Old Frisian, Old English, Old Saxon, Old Norse) and runes are read at once.</summary>
        public static bool NeedsLearning(string language)
        {
            string l = (language ?? "").ToLowerInvariant();
            return l.Contains("latin") || l.Contains("irish") || l.Contains("greek") || l.Contains("arabic");
        }

        /// <summary>A reader aboard (a monk passenger): to come with passengers.</summary>
        public static bool ReaderAboard => false;

        /// <summary>Does the switch turn this item's text into English now?</summary>
        public static bool CanRead(ChestItem item)
        {
            var t = item?.Text;
            if (t == null)
                return false;
            if (string.IsNullOrEmpty(t.translation))
                return false;
            if (!t.NeedsLearning)
                return true;
            bool latin = (t.language ?? "").ToLowerInvariant().Contains("latin");
            return item.readToYou || ReaderAboard || (latin && KnowsLatin);
        }

        /// <summary>Why the switch won't turn it into English (null: it will).</summary>
        public static string WhyNotRead(ChestItem item)
        {
            var t = item?.Text;
            if (t == null)
                return "Nothing to read.";
            if (string.IsNullOrEmpty(t.translation))
                return "There's no free English of this text: only its own words.";
            if (CanRead(item))
                return null;
            return $"You can't read {t.language} yet. A monk could read it to you at a monastery, or you could learn it there " +
                   $"({LatinLessons} of {LessonsForLatin} lessons).";
        }

        /// <summary>At a monastery: a monk reads a text out (its English is open from then on).</summary>
        public static void ReadToYou(ChestItem item)
        {
            if (item == null || item.readToYou)
                return;
            item.readToYou = true;
            Touch();
        }

        /// <summary>A lesson in letters at a monastery; after enough, Latin is read.</summary>
        public static string Lesson()
        {
            if (KnowsLatin)
                return "You read Latin already.";
            LatinLessons = Math.Min(LessonsForLatin, LatinLessons + 1);
            if (LatinLessons >= LessonsForLatin)
                KnowsLatin = true;
            Touch();
            return KnowsLatin ? "You can read Latin now." : $"A lesson in letters: {LatinLessons} of {LessonsForLatin}.";
        }

        // ------------------------------------------------------------------------------------------ things known

        /// <summary>Does the merchant know this thing (to drag it onto a riddle)? Common things from the start; goods once
        /// seen in a market (or carried); places once seen or heard of; things of the texts once read.</summary>
        public static bool Knows(ThingDef th)
        {
            if (th == null)
                return false;
            switch (th.known)
            {
                case "market":
                {
                    if (th.gameId == null)
                        return true;
                    if (Trade.Started && (Trade.Hold.Count(th.gameId) > 0 || Trade.Ledger.Seen(th.gameId).Any()))
                        return true;
                    return false;
                }
                case "place":
                case "seen_in_port":
                case "talk":
                {
                    var p = GameData.Port(th.gameId);
                    if (p != null)
                    {
                        var (x, z) = Kapman.World.WorldFrame.ToWorld(p.lat, p.lon);
                        return Known.IsHeardOf(p) || (Exploration.Ready && Exploration.Seen(x, z) > 0.35f);
                    }
                    if (th.at.Length == 0)
                        return th.known != "place";
                    // seen or heard of at one of its ports: been there (seen), or heard of it there (talk)
                    foreach (var id in th.at)
                    {
                        var at = GameData.Port(id);
                        if (at == null)
                            continue;
                        if (Known.StandingAt(at, out _, out _, out _) || (th.known == "talk" && Known.IsHeardOf(at)))
                            return true;
                    }
                    return false;
                }
                case "text":
                    // taught by a text read: one it names, or any in the chest that teaches it
                    foreach (var i in items)
                        if ((Array.IndexOf(th.at, i.def) >= 0 || (i.Text != null && Array.IndexOf(i.Text.teaches, th.id) >= 0))
                            && (i.kind != "text" || CanRead(i) || !i.Text.NeedsLearning))
                            return true;
                    return false;
                case "seen_at_sea":
                    return beenAtSea || th.at.Length == 0 && Trade.Started && Voyage.DaysAtSea > 0.0;
                default:
                    return true;                                          // start: what anyone knows
            }
        }

        /// <summary>Every thing the merchant knows, by kind, then name.</summary>
        public static List<ThingDef> KnownThings() =>
            ChestData.Things.Values.Where(Knows).OrderBy(t => t.kind).ThenBy(t => t.name).ToList();

        // ------------------------------------------------------------------------------------------ answering

        /// <summary>A thing dragged onto a riddle: right, it's answered (and its reward comes); wrong, a try is gone; out of
        /// tries, it rests. Returns the fact.</summary>
        public static string Answer(ChestItem item, string thingId)
        {
            var r = item?.Riddle;
            var thing = ChestData.Thing(thingId);
            if (r == null || thing == null)
                return "";
            if (item.solved)
                return "Answered already.";
            if (item.resting)
                return "It rests: learn something new that bears on it, and try again.";
            if (item.tried.Contains(thingId))
                return $"You've tried {thing.name} already.";
            if (Array.IndexOf(r.answers, thingId) >= 0)
            {
                item.solved = true;
                string fact = $"{Cap(thing.name)}: that's it.";
                fact += " " + Win(item, r);
                fact += SetDone(r);
                Touch();
                Voyage.Say(fact);
                return fact;
            }
            item.tried.Add(thingId);
            item.tries = Math.Max(0, item.tries - 1);
            string not = $"Not {thing.name}.";
            if (item.tries == 0)
            {
                Rest(item);
                not += " Out of tries: it rests until you learn something new that bears on it.";
            }
            else
                not += $" {item.tries} {(item.tries == 1 ? "try" : "tries")} left.";
            Touch();
            return not;
        }

        /// <summary>A riddle of two or three answers: puts a thing in one of its places (not judged yet).</summary>
        public static void Fill(ChestItem item, int slot, string thingId)
        {
            var r = item?.Riddle;
            if (r == null || item.solved || item.resting || slot < 0 || slot >= r.SlotCount)
                return;
            if (item.filled == null || item.filled.Length != r.SlotCount)
                item.filled = new string[r.SlotCount];
            item.filled[slot] = thingId;
            Touch();
        }

        /// <summary>Judges a riddle of two or three answers with every place filled: all right, it's answered; else a try is
        /// gone (which part was wrong isn't told), and the places are cleared. Returns the fact.</summary>
        public static string Judge(ChestItem item)
        {
            var r = item?.Riddle;
            if (r == null || item.solved || item.resting || item.filled == null || item.filled.Any(string.IsNullOrEmpty))
                return "Fill every place first.";
            string combo = string.Join("+", item.filled);
            if (item.tried.Contains(combo))
                return "You've tried that already.";
            bool right = true;
            for (int s = 0; s < r.SlotCount; s++)
                if (Array.IndexOf(r.slots[s], item.filled[s]) < 0)
                    right = false;
            string names = string.Join(" and ", item.filled.Select(t => ChestData.Thing(t)?.name ?? t));
            if (right)
            {
                item.solved = true;
                string fact = $"{Cap(names)}: that's it. " + Win(item, r) + SetDone(r);
                Touch();
                return fact;
            }
            item.tried.Add(combo);
            item.tries = Math.Max(0, item.tries - 1);
            item.filled = new string[r.SlotCount];
            string not = $"Not {names}.";
            if (item.tries == 0)
            {
                Rest(item);
                not += " Out of tries: it rests until you learn something new that bears on it.";
            }
            else
                not += $" {item.tries} {(item.tries == 1 ? "try" : "tries")} left.";
            Touch();
            return not;
        }

        /// <summary>Each game hour away from port: things seen at sea are known from then.</summary>
        public static void AtSea()
        {
            if (!beenAtSea)
            {
                beenAtSea = true;
                Touch();
            }
        }

        static void Rest(ChestItem item)
        {
            item.resting = true;
            item.restKnown.Clear();
            foreach (var w in item.Riddle.wakeOn)
                if (Satisfied(w))
                    item.restKnown.Add(w.kind + ":" + w.id);
            item.restPorts = PortsVisited();
            item.restTexts = items.Count(i => i.kind == "text");
        }

        /// <summary>Wakes resting riddles that something newly learned bears on (their tries come back).</summary>
        public static void CheckWakes()
        {
            foreach (var item in items)
            {
                if (!item.resting || item.solved || item.Riddle == null)
                    continue;
                bool wake = false;
                var r = item.Riddle;
                if (r.wakeOn.Count > 0)
                {
                    foreach (var w in r.wakeOn)
                        if (!item.restKnown.Contains(w.kind + ":" + w.id) && Satisfied(w))
                            wake = true;
                }
                else
                    wake = PortsVisited() > item.restPorts || items.Count(i => i.kind == "text") > item.restTexts;
                if (!wake)
                    continue;
                item.resting = false;
                item.tries = r.tries;
                Voyage.Say("Something you've learned bears on a riddle in your chest: it can be tried again.");
                Touch();
            }
        }

        static bool Satisfied((string kind, string id) w)
        {
            switch (w.kind)
            {
                case "market": return Trade.Started && (Trade.Ledger.Seen(w.id).Any() || Trade.Hold.Count(w.id) > 0);
                case "port": return Known.StandingAt(GameData.Port(w.id), out _, out _, out _);
                case "text": return Has(w.id);
                case "thing": return Knows(ChestData.Thing(w.id));
                case "news": return Known.News().Exists(f => f.chronicleId == w.id || f.kind == w.id || f.refId == w.id);
                default: return false;
            }
        }

        static int PortsVisited() => GameData.Ports.Count(p => Known.StandingAt(p, out _, out _, out _));

        // ------------------------------------------------------------------------------------------ verses: kennings decoded

        /// <summary>A thing dragged onto a kenning of a verse: right, the kenning is read (all read: the verse is). No tries:
        /// a verse is decoded at leisure. Returns the fact.</summary>
        public static string AnswerKenning(ChestItem item, int slot, string thingId)
        {
            var v = item?.Verse;
            var thing = ChestData.Thing(thingId);
            if (v == null || thing == null || slot < 0 || slot >= v.slots.Count || item.solved)
                return "";
            if (item.slotsDone.Contains(slot))
                return "That one's read already.";
            if (v.slots[slot].thing != thingId)
                return $"Not {thing.name}.";
            item.slotsDone.Add(slot);
            string fact = $"\"{v.slots[slot].original}\": {thing.name}.";
            if (item.slotsDone.Count >= v.slots.Count)
            {
                item.solved = true;
                fact += " The whole verse is read." + (string.IsNullOrEmpty(v.answer) ? "" : " " + v.answer);
                Deeds.Record("won_riddle", "read a skald's verse", Deeds.NearestPlace());
            }
            Touch();
            return fact;
        }

        // ------------------------------------------------------------------------------------------ the old ways: found by sailing

        public static bool Found(LegDef leg) => leg != null && foundLegs.Contains(leg.Key);

        /// <summary>A big reward won (a hoard's place, a ship, a cove, a right, a board), waiting for its own step.</summary>
        public static bool Promised(string reward) => reward != null && promised.Contains(reward);
        public static IEnumerable<string> PromisedRewards => promised;

        /// <summary>Each game hour: the ship near a place an old text in the chest sails to finds it - the fog lifts round it,
        /// it's pinned on the map, and how near your own mark was is told.</summary>
        public static void CheckVoyages(double x, double z)
        {
            foreach (var item in items)
            {
                var t = item.Text;
                if (item.kind != "text" || t == null || !t.voyage)
                    continue;
                foreach (var leg in ChestData.LegsOf(t.id))
                {
                    if (double.IsNaN(leg.toLat) || foundLegs.Contains(leg.Key))
                        continue;
                    var (lx, lz) = Kapman.World.WorldFrame.ToWorld(leg.toLat, leg.toLon);
                    double km = Math.Sqrt((lx - x) * (lx - x) + (lz - z) * (lz - z)) / 1000.0;
                    if (km > 30.0)
                        continue;
                    foundLegs.Add(leg.Key);
                    if (Exploration.Ready)
                        Exploration.Reveal(lx, lz, Exploration.Sight * 2.0);
                    var port = GameData.Port(leg.toPort);
                    if (port != null)
                        Known.HearOf(port);
                    // how near the merchant's own mark for this leg was
                    string mark = "";
                    var pin = notes.Find(n => n.links.Contains("leg:" + leg.Key) && !double.IsNaN(n.x));
                    if (pin != null)
                        mark = $" Your mark was {Math.Sqrt((pin.x - lx) * (pin.x - lx) + (pin.z - lz) * (pin.z - lz)) / 1000.0:0} km from it.";
                    var found = AddNote("Found: " + leg.toName, "text:" + t.id, "leg:" + leg.Key);
                    Pin(found, lx, lz);
                    Voyage.Say($"As {t.title.Split('(')[0].Trim()} tells: {leg.toName}.{mark}");
                    Deeds.Record("won_riddle", "found the way an old text tells", Deeds.NearestPlace());
                }
            }
        }

        // ------------------------------------------------------------------------------------------ rewards

        /// <summary>An answer found: rewards that are a person's to give wait at their port (bring the answer back); a key
        /// (a price tip, another text) comes at once.</summary>
        static string Win(ChestItem item, RiddleDef r)
        {
            var w = ChestData.Reward(r.reward);
            if (w == null)
                return "";
            bool fromPerson = w.kind == "silver" || w.kind == "standing" || w.kind == "toll" || w.kind == "lodging";
            string port = w.where ?? item.at;
            if (fromPerson && port != null)
            {
                item.rewardDue = true;
                return $"Take the answer to {GameData.Port(port)?.name ?? port}: {w.what}.";
            }
            return Apply(w, null);
        }

        /// <summary>Lying at a port (each game hour): answers brought back are rewarded; riddles to be found here are found.</summary>
        public static void AtPort(PortDef port)
        {
            if (port == null)
                return;
            foreach (var item in items)
            {
                if (!item.rewardDue || item.Riddle == null)
                    continue;
                var w = ChestData.Reward(item.Riddle.reward);
                if (w == null)
                    continue;
                bool here = (w.where ?? item.at) == port.id && (w.to == 0 || Year <= w.to);
                bool later = w.laterWhere == port.id && Year >= w.laterFrom;
                if (!here && !later)
                    continue;
                item.rewardDue = false;
                Voyage.Say("You bring back the answer. " + Apply(w, port));
                Touch();
            }
            // a verse given at a hall here (one a day): a poet's, carved down for you
            foreach (var v in ChestData.Verses.Values)
            {
                var a = v.acquire;
                if (a == null || a.how != "given" || Has(v.id) || !a.OpenIn(Year) || Array.IndexOf(a.where, port.id) < 0 || Math.Floor(Today) <= lastVerseDay)
                    continue;
                lastVerseDay = Math.Floor(Today);
                Give("verse", v.id, string.IsNullOrEmpty(a.who) ? "a poet at " + port.name : a.who, port.id);
                break;
            }
            // the rune stones here: copied into the chest, to read yourself
            foreach (var ins in ChestData.Inscriptions)
                if (ins.port == port.id && !Has("stone:" + ins.id))
                    Give("text", "stone:" + ins.id, "the stone itself", port.id);
            foreach (var r in ChestData.Riddles.Values)
            {
                var a = r.acquire;
                if (a == null || (a.how != "found" && a.how != "given") || Has(r.id) || !a.OpenAt(port.id, Year))
                    continue;
                if (!PreviousSolved(r))
                    continue;
                Give("riddle", r.id, string.IsNullOrEmpty(a.who) ? (a.how == "found" ? "found at " + port.name : "given at " + port.name) : a.who, port.id);
            }
            foreach (var t in ChestData.Texts.Values)
            {
                var a = t.acquire;
                if (a != null && (a.how == "found" || a.how == "given") && !Has(t.id) && a.OpenAt(port.id, Year))
                    Give("text", t.id, string.IsNullOrEmpty(a.who) ? "given at " + port.name : a.who, port.id);
            }
        }

        /// <summary>In a chain, the riddle before must be answered first.</summary>
        static bool PreviousSolved(RiddleDef r)
        {
            if (r.set == null || !ChestData.Sets.TryGetValue(r.set, out var s) || s.kind != "chain")
                return true;
            int i = Array.IndexOf(s.riddles, r.id);
            if (i <= 0)
                return true;
            var prev = items.Find(x => x.def == s.riddles[i - 1]);
            return prev != null && prev.solved;
        }

        static string SetDone(RiddleDef r)
        {
            if (r.set == null || !ChestData.Sets.TryGetValue(r.set, out var s))
                return "";
            foreach (var id in s.riddles)
                if (!items.Exists(i => i.def == id && i.solved))
                    return "";
            var w = ChestData.Reward(s.reward);
            return w == null ? " The whole set is answered." : " The whole set is answered: " + Apply(w, null);
        }

        /// <summary>Gives a reward; returns the fact.</summary>
        static string Apply(RewardDef w, PortDef at)
        {
            var rules = TradeRules.Current;
            var zone = at != null && rules != null ? rules.ZoneOf(at.region) : null;
            switch (w.kind)
            {
                case "silver":
                    if (Trade.Started && w.silver > 0)
                        Trade.Purse.Receive(w.silver, zone, rules, out _);
                    return $"{w.what} ({w.silver:0} deniers' worth).";
                case "standing":
                    Deeds.Record(w.size == "small" ? "won_riddle" : "won_great_riddle", "answered a riddle", at ?? GameData.Port(w.where));
                    return w.what;
                case "toll":
                {
                    string port = w.where;
                    if (port != null)
                        tollFree[port] = Year + Math.Max(1, w.years);
                    return $"{w.what} (no toll on your sales at {GameData.Port(port)?.name ?? port} until {Year + Math.Max(1, w.years)}).";
                }
                case "lodging":
                    Voyage.Rest(0.15, 0.15);
                    return w.what;
                case "tip":
                {
                    var p = GameData.Port(w.where);
                    var world = KapmanGame.Current?.World;
                    var snap = p != null && world != null ? Freshest(world, p.index) : null;
                    if (snap != null && Trade.Started)
                        Trade.Ledger.Hear(snap, "told as the riddle's reward", How.Told, 0, Today);
                    return $"{w.what}{(snap != null ? " The prices at " + p.name + " go into your notes." : "")}";
                }
                case "text":
                    if (w.text != null)
                        Give(ChestData.Riddle(w.text) != null ? "riddle" : "text", w.text, "as a reward", at?.id, quiet: true);
                    return w.what;
                // "hoard" rewards are only promised for now: the treasure-map hunt (Hoards) is off until it is rebuilt on
                // real practice (fetching an owner's silver for a share; Plans/map_play.md, "Fetching buried silver").
                default:
                    promised.Add(w.id);                                   // a ship, a cove, a right, a board: their own steps
                    return w.what + ".";
            }
        }

        /// <summary>The freshest prices of a port that any waterfront knows (a reward's tip).</summary>
        static PriceSnapshot Freshest(WorldSim world, int port)
        {
            PriceSnapshot best = null;
            for (int i = 0; i < GameData.Ports.Count; i++)
            {
                var s = world.Board(i).Of(port);
                if (s != null && (best == null || s.day > best.day))
                    best = s;
            }
            return best;
        }

        /// <summary>No toll on the merchant's sales here this year (a riddle's reward)?</summary>
        public static bool TollFreeAt(PortDef p, int year) => p != null && tollFree.TryGetValue(p.id, out int until) && year <= until;

        // ------------------------------------------------------------------------------------------ getting riddles

        /// <summary>A writing to be had at a port: a riddle, a text or a verse, its price and who has it.</summary>
        public struct Offer
        {
            public string kind, def, title;
            public Acquire acquire;
        }

        /// <summary>Everything to be bought at a port this year that isn't in the chest yet: riddles, texts (the old ways), verses.</summary>
        public static List<Offer> OffersAt(PortDef p, int year)
        {
            var list = new List<Offer>();
            if (p == null)
                return list;
            bool Here(Acquire a) => a != null && a.how == "bought" && a.OpenAt(p.id, year);
            foreach (var r in ForSaleAt(p, year))
                list.Add(new Offer { kind = "riddle", def = r.id, title = ChestData.Text(r.textId)?.title ?? r.id, acquire = r.acquire });
            foreach (var t in ChestData.Texts.Values)
                if (Here(t.acquire) && !Has(t.id))
                    list.Add(new Offer { kind = "text", def = t.id, title = t.title, acquire = t.acquire });
            foreach (var v in ChestData.Verses.Values)
                if (Here(v.acquire) && !Has(v.id))
                    list.Add(new Offer { kind = "verse", def = v.id, title = ChestData.Text(v.textId)?.title ?? v.id, acquire = v.acquire });
            return list;
        }

        /// <summary>Buys an offer; returns the fact.</summary>
        public static string Buy(Offer o, PortDef at)
        {
            if (o.acquire == null || at == null || !Trade.Started)
                return "";
            var rules = TradeRules.Current;
            var zone = rules.ZoneOf(at.region);
            if (!Trade.Purse.Pay(o.acquire.price, zone, rules, out string how))
                return $"You haven't the {zone.Show(o.acquire.price)} it costs.";
            Give(o.kind, o.def, string.IsNullOrEmpty(o.acquire.who) ? "bought at " + at.name : o.acquire.who, at.id);
            return $"Bought for {zone.Show(o.acquire.price)}: {how}.";
        }

        /// <summary>The writings to be had at a port this year (bought): riddles and texts not yet in the chest.</summary>
        public static List<RiddleDef> ForSaleAt(PortDef p, int year) =>
            p == null ? new List<RiddleDef>() :
            ChestData.Riddles.Values.Where(r => r.acquire != null && r.acquire.how == "bought" && r.acquire.OpenAt(p.id, year)
                                                && !Has(r.id) && PreviousSolved(r)).ToList();

        /// <summary>Buys a writing; returns the fact.</summary>
        public static string Buy(RiddleDef r, PortDef at)
        {
            if (r == null || at == null || !Trade.Started)
                return "";
            var rules = TradeRules.Current;
            var zone = rules.ZoneOf(at.region);
            if (!Trade.Purse.Pay(r.acquire.price, zone, rules, out string how))
                return $"You haven't the {zone.Show(r.acquire.price)} it costs.";
            Give("riddle", r.id, string.IsNullOrEmpty(r.acquire.who) ? "bought at " + at.name : r.acquire.who, at.id);
            return $"Bought for {zone.Show(r.acquire.price)}: {how}.";
        }

        /// <summary>A ship hailed may give a riddle (now and then: the same for the same ship and day). The fact, or "".</summary>
        public static string GivenAtSea(SimShip s, double day)
        {
            if (s == null)
                return "";
            if (Known.Hash01(s.id, Math.Floor(day), 0, 41) > 0.3f)
                return "";
            var r = ChestData.Riddles.Values.FirstOrDefault(x => x.acquire != null && x.acquire.how == "given" && x.acquire.OpenAt("sea", Year)
                && !Has(x.id) && PreviousSolved(x));
            if (r == null)
                return "";
            Give("riddle", r.id, "her master", null, quiet: true);
            return "Her master gives you a riddle to work on: it's in your chest.";
        }

        /// <summary>The start: the riddle the merchant begins with (the table's "start").</summary>
        public static void Begin()
        {
            if (items.Count == 0 && ChestData.StartRiddle != null)
                Give("riddle", ChestData.StartRiddle, "Ravn", "dorestad", quiet: true);
        }

        // ------------------------------------------------------------------------------------------ notes

        public static PlayerNote AddNote(string text, params string[] links)
        {
            var n = new PlayerNote { id = nextNote++, text = text ?? "", day = Today };
            foreach (var l in links)
                if (!string.IsNullOrEmpty(l) && !n.links.Contains(l))
                    n.links.Add(l);
            notes.Add(n);
            Touch();
            return n;
        }

        public static void EditNote(PlayerNote n, string text)
        {
            if (n == null || n.text == text)
                return;
            n.text = text ?? "";
            Touch();
        }

        public static void DeleteNote(PlayerNote n)
        {
            if (n != null && notes.Remove(n))
            {
                threads.RemoveAll(t => t.a == n.id || t.b == n.id);
                Touch();
            }
        }

        public static void Link(PlayerNote n, string link)
        {
            if (n != null && !string.IsNullOrEmpty(link) && !n.links.Contains(link))
            {
                n.links.Add(link);
                Touch();
            }
        }

        public static void Pin(PlayerNote n, double x, double z)
        {
            if (n == null)
                return;
            n.x = x;
            n.z = z;
            Touch();
        }

        public static IEnumerable<PlayerNote> NotesFor(string link) => notes.Where(n => n.links.Contains(link));

        public static PlayerNote Note(int id) => notes.Find(n => n.id == id);

        /// <summary>The strings between pinned notes (the map's string and pins: a fleet's track, a line of reasoning).</summary>
        public static IReadOnlyList<(int a, int b)> Threads => threads;

        /// <summary>Ties a string between two notes (again: unties it).</summary>
        public static void Thread(int a, int b)
        {
            if (a == b)
                return;
            int i = threads.FindIndex(t => (t.a == a && t.b == b) || (t.a == b && t.b == a));
            if (i >= 0)
                threads.RemoveAt(i);
            else
                threads.Add((a, b));
            Touch();
        }

        /// <summary>Turns the sound under a rune on to the next it can stand for; when every word reads, the whole is read.
        /// Returns the fact when it's all read, else "".</summary>
        public static string TurnRune(ChestItem item, int at, int soundCount)
        {
            if (item == null || item.runesRead || soundCount <= 0)
                return "";
            item.runeChoice[at] = item.runeChoice.TryGetValue(at, out int i) ? (i + 1) % (soundCount + 1) : 0;   // (the last step: blank)
            if (item.runeChoice[at] == soundCount)
                item.runeChoice.Remove(at);
            string fact = "";
            if (AllRunesRead(item))
            {
                item.runesRead = true;
                fact = $"You've read the runes of {item.Text?.title} yourself.";
                CheckWakes();
            }
            Touch();
            return fact;
        }

        /// <summary>Are all the words of a rune text read (or given, where their spelling can't be matched)?</summary>
        public static bool AllRunesRead(ChestItem item)
        {
            var t = item?.Text;
            if (t == null || t.normalized == null)
                return false;
            var words = RuneReading.Words(t.original);
            var targets = RuneReading.Targets(t.normalized);
            if (words.Count != targets.Count)
                return false;
            int at = 0;
            for (int w = 0; w < words.Count; w++)
            {
                var chosen = new List<string>();
                foreach (var rune in words[w])
                {
                    var sounds = RuneReading.SoundsOf(rune);
                    chosen.Add(item.runeChoice.TryGetValue(at, out int s) && s < sounds.Length ? sounds[s] : null);
                    at++;
                }
                if (!RuneReading.Readable(words[w], targets[w]))
                    continue;                                              // (given)
                if (!RuneReading.IsRead(chosen, targets[w]))
                    return false;
            }
            return true;
        }

        /// <summary>Marks or unmarks a word of a riddle's text as a clue.</summary>
        public static void ToggleMark(ChestItem item, int word)
        {
            if (item == null)
                return;
            if (!item.marks.Add(word))
                item.marks.Remove(word);
            Touch();
        }

        /// <summary>Notes and texts in the chest with these words in them.</summary>
        public static (List<PlayerNote> notes, List<ChestItem> items) Search(string words)
        {
            string q = (words ?? "").Trim();
            if (q.Length == 0)
                return (notes.ToList(), items.ToList());
            bool In(string s) => s != null && s.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;
            return (notes.Where(n => In(n.text)).ToList(),
                    items.Where(i => In(i.Text?.original) || In(i.Text?.title) || (CanRead(i) && In(i.Text?.translation))).ToList());
        }

        // ------------------------------------------------------------------------------------------ saving

        public static Dictionary<string, object> Capture()
        {
            var list = new List<object>();
            foreach (var i in items)
                list.Add(new Dictionary<string, object>
                {
                    ["id"] = i.id, ["kind"] = i.kind, ["def"] = i.def, ["got"] = i.got, ["from"] = i.from, ["at"] = i.at, ["tries"] = i.tries,
                    ["solved"] = i.solved, ["resting"] = i.resting, ["read"] = i.readToYou, ["due"] = i.rewardDue,
                    ["tried"] = i.tried.ConvertAll(t => (object)t), ["marks"] = i.marks.Select(m => (object)m).ToList(),
                    ["rest_known"] = i.restKnown.Select(k => (object)k).ToList(), ["rest_ports"] = i.restPorts, ["rest_texts"] = i.restTexts,
                    ["runes"] = i.runeChoice.Select(kv => (object)new List<object> { kv.Key, kv.Value }).ToList(), ["runes_read"] = i.runesRead,
                    ["slots"] = i.slotsDone.Select(s => (object)s).ToList(),
                    ["filled"] = i.filled == null ? null : i.filled.Select(f => (object)f).ToList(),
                });
            var noteList = new List<object>();
            foreach (var n in notes)
                noteList.Add(new Dictionary<string, object>
                {
                    ["id"] = n.id, ["text"] = n.text, ["links"] = n.links.ConvertAll(l => (object)l), ["day"] = n.day,
                    ["x"] = double.IsNaN(n.x) ? null : (object)n.x, ["z"] = double.IsNaN(n.z) ? null : (object)n.z,
                });
            var toll = new Dictionary<string, object>();
            foreach (var kv in tollFree)
                toll[kv.Key] = kv.Value;
            return new Dictionary<string, object>
            {
                ["next"] = nextId, ["next_note"] = nextNote, ["latin"] = KnowsLatin, ["lessons"] = LatinLessons,
                ["items"] = list, ["notes"] = noteList, ["toll_free"] = toll,
                ["threads"] = threads.Select(t => (object)new List<object> { t.a, t.b }).ToList(),
                ["found_legs"] = foundLegs.Select(l => (object)l).ToList(),
                ["at_sea"] = beenAtSea, ["promised"] = promised.Select(p => (object)p).ToList(),
            };
        }

        public static void Restore(Dictionary<string, object> d)
        {
            Clear();
            if (d == null)
                return;
            nextId = (int)Json.Num(d, "next");
            nextNote = (int)Json.Num(d, "next_note");
            KnowsLatin = Json.Bool(d, "latin");
            LatinLessons = (int)Json.Num(d, "lessons");
            foreach (var x in Json.Arr(d, "items") ?? new List<object>())
            {
                var i = new ChestItem
                {
                    id = (int)Json.Num(x, "id"), kind = Json.Str(x, "kind"), def = Json.Str(x, "def"), got = Json.Num(x, "got"),
                    from = Json.Str(x, "from", null), at = Json.Str(x, "at", null), tries = (int)Json.Num(x, "tries"), solved = Json.Bool(x, "solved"),
                    resting = Json.Bool(x, "resting"), readToYou = Json.Bool(x, "read"), rewardDue = Json.Bool(x, "due"),
                    restPorts = (int)Json.Num(x, "rest_ports"), restTexts = (int)Json.Num(x, "rest_texts"),
                };
                foreach (var t in Json.Arr(x, "tried") ?? new List<object>())
                    if (t is string s) i.tried.Add(s);
                foreach (var m in Json.Arr(x, "marks") ?? new List<object>())
                    if (m is double v) i.marks.Add((int)v);
                foreach (var k in Json.Arr(x, "rest_known") ?? new List<object>())
                    if (k is string s) i.restKnown.Add(s);
                foreach (var rc in Json.Arr(x, "runes") ?? new List<object>())
                    if (rc is List<object> l && l.Count >= 2 && l[0] is double a && l[1] is double b) i.runeChoice[(int)a] = (int)b;
                i.runesRead = Json.Bool(x, "runes_read");
                foreach (var sl in Json.Arr(x, "slots") ?? new List<object>())
                    if (sl is double sv) i.slotsDone.Add((int)sv);
                var fl = Json.Arr(x, "filled");
                if (fl != null)
                    i.filled = fl.Select(f => f as string).ToArray();
                items.Add(i);
                nextId = Math.Max(nextId, i.id + 1);
            }
            foreach (var x in Json.Arr(d, "notes") ?? new List<object>())
            {
                var n = new PlayerNote { id = (int)Json.Num(x, "id"), text = Json.Str(x, "text"), day = Json.Num(x, "day") };
                foreach (var l in Json.Arr(x, "links") ?? new List<object>())
                    if (l is string s) n.links.Add(s);
                var o = x as Dictionary<string, object>;
                n.x = o != null && o.TryGetValue("x", out var vx) && vx is double dx ? dx : double.NaN;
                n.z = o != null && o.TryGetValue("z", out var vz) && vz is double dz ? dz : double.NaN;
                notes.Add(n);
                nextNote = Math.Max(nextNote, n.id + 1);
            }
            foreach (var kv in Json.Obj(d, "toll_free") ?? new Dictionary<string, object>())
                if (kv.Value is double y)
                    tollFree[kv.Key] = (int)y;
            foreach (var x in Json.Arr(d, "found_legs") ?? new List<object>())
                if (x is string s) foundLegs.Add(s);
            beenAtSea = Json.Bool(d, "at_sea");
            foreach (var x in Json.Arr(d, "promised") ?? new List<object>())
                if (x is string s) promised.Add(s);
            foreach (var x in Json.Arr(d, "threads") ?? new List<object>())
                if (x is List<object> l && l.Count >= 2 && l[0] is double a && l[1] is double b)
                    threads.Add(((int)a, (int)b));
            Touch();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Clear()
        {
            items.Clear();
            notes.Clear();
            tollFree.Clear();
            threads.Clear();
            foundLegs.Clear();
            lastVerseDay = double.MinValue;
            beenAtSea = false;
            promised.Clear();
            nextId = nextNote = 0;
            KnowsLatin = false;
            LatinLessons = 0;
            Version++;
        }

        static string Cap(string t) => string.IsNullOrEmpty(t) ? t : char.ToUpperInvariant(t[0]) + t.Substring(1);
    }
}
