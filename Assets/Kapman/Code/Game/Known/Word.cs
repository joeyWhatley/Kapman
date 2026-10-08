using System;
using System.Collections.Generic;
using System.Globalization;
using Kapman.Game.Sim;

namespace Kapman.Game
{
    /// <summary>How the word on an item came to the merchant: the small mark beside the line.</summary>
    public enum WordMark
    {
        /// <summary>Nothing is known.</summary>
        None,
        /// <summary>Seen with the merchant's own eyes.</summary>
        Seen,
        /// <summary>Told, talked of, or a rumour.</summary>
        Heard,
        /// <summary>What the time of year means (the trading year every merchant carries in his head).</summary>
        Season,
        /// <summary>The law on the good there.</summary>
        Law,
    }

    /// <summary>
    /// THE WORD ON THE ITEM (Design/research_trade_seasons_and_hints.md 6): the one line of what the merchant knows of a
    /// good at a port, ready to be said: a wording key (wording_793.json "word"), the words that fill it, a mark for how
    /// it came, and a tilt (the way the price there leans: +1 worth carrying there, -1 not, 0 neither).
    /// </summary>
    public struct GoodWord
    {
        /// <summary>Which of the eight rules fired: 1 hard news, 2 a price seen, 3 a price told, 4 talk and rumour, 5 the
        /// season, 6 who sailed ahead, 7 the law, 8 nothing known.</summary>
        public int rule;
        /// <summary>The wording key: "seen", "told", "talk.at", "rumour", "news.shut", "season.comes", "ahead",
        /// "law.no_export", "none" ...</summary>
        public string key;
        /// <summary>The words that fill the line's {slots}: good, port, month, and by rule price, pays, person, place,
        /// age, news, ships, realm, law, line.</summary>
        public (string name, string value)[] slots;
        public WordMark mark;
        /// <summary>The merchant's gut from the rule that fired: +1 the price there leans up (worth carrying there), -1 it
        /// leans down (cheap there, or nobody buying), 0 neither.</summary>
        public int tilt;
        /// <summary>The good and the port the line is about (ids; the port is "" when it names none).</summary>
        public string good, port;
        /// <summary>How old what it rests on is, in days (NaN: it has no age), and the age as a wording key
        /// ("age.last_week"; "" when it has none). The line says the age in words, never in days.</summary>
        public double age;
        public string ageKey;

        /// <summary>The value of a slot ("" when the line has none of that name).</summary>
        public string Slot(string name)
        {
            if (slots != null)
                foreach (var (n, v) in slots)
                    if (n == name)
                        return v ?? "";
            return "";
        }

        /// <summary>The line in the words of wording_793.json (the port's own, its people's, the table's).</summary>
        public string Text => Wording.Word(this);

        public override string ToString() => Text;
    }

    /// <summary>What the word on an item is asked against: the day, where she lies, and the ships of the world.</summary>
    public struct WordAsk
    {
        /// <summary>Game days since the start (the hours as a fraction).</summary>
        public double today;
        /// <summary>The month (1-12) and the year; 0: worked out from the day by the economy's calendar (Seasons).</summary>
        public int month, year;
        /// <summary>The port the merchant lies at (null at sea): ships that left it lately sailed ahead of her.</summary>
        public PortDef here;
        /// <summary>The world's ships (null: none are looked at).</summary>
        public IReadOnlyList<SimShip> ships;
        // the price the word on one good is said against, worked out once for a line or for a row of the ledger
        internal WordHere against;
        internal string againstGood;
    }

    /// <summary>How exactly a price is known, from least to most: not at all; as a word against where she lies (talk, a
    /// rumour, word grown old); as a rough figure ("about 36 d": told by one who saw it himself, or her own sight of it
    /// more than a week old); to the penny (seen with her own eyes within the week).</summary>
    public enum PriceGrade { None, Word, About, Exact }

    /// <summary>The price the word is said against: what she saw where she lies now, or (at sea, or where she has not
    /// looked at the market) the last price of the good she saw with her own eyes.</summary>
    public struct WordHere
    {
        /// <summary>The port it was seen at (null: she has seen no price of the good in the days a note counts).</summary>
        public PortDef port;
        /// <summary>She lies there now ("than here"); false: it is where she last saw one ("than at Dorestat").</summary>
        public bool lying;
        /// <summary>What they paid and asked there for one unit as she saw them (deniers), and whether any was on offer.</summary>
        public float pays, asks;
        public bool onOffer;
        /// <summary>The game day she saw them.</summary>
        public double day;
        public bool Known => port != null;
    }

    /// <summary>One cell of the ledger: what the merchant knows of a good's price at a port, how exactly, how old, and
    /// how it stands against where she lies.</summary>
    public struct PriceCell
    {
        public PriceGrade grade;
        /// <summary>The rule of the word it comes by: 2 a price seen, 3 a price told, 4 talk, a rumour, or grown old (0: nothing).</summary>
        public int rule;
        /// <summary>What they pay and ask there for one unit, as noted (deniers; asks is NaN where none was on offer).</summary>
        public float pays, asks;
        public bool onOffer;
        /// <summary>The game day it was true, and its age as a wording key ("age.last_week").</summary>
        public double day;
        public string ageKey;
        /// <summary>True when there is a price to set it against, and this port is not that price's own.</summary>
        public bool compared;
        /// <summary>What they pay there against what they pay here, and what they ask against what they ask here: +2 far
        /// more, +1 more, 0 much the same, -1 less, -2 far less (0 too when nothing is compared, or none is on offer).</summary>
        public int paysLean, asksLean;
        /// <summary>The note it rests on (who told it: Known.Person(note)); null when nothing is known.</summary>
        public PriceNote note;
        /// <summary>The good it is about (its id).</summary>
        public string goodId;
    }

    public static partial class Known
    {
        // the first words of goods' names that keep their capital in the middle of a line (a place or a people)
        static readonly HashSet<string> ProperFirst = new HashSet<string>
        {
            "Rhenish", "Frankish", "Frisian", "Mayen", "Tating-ware", "Pingsdorf", "Ipswich", "Ulfberht", "Droitwich",
        };

        // the age words from the youngest to the oldest (the note on an item prefers the youngest)
        static readonly string[] AgeOrder =
        {
            "age.this_morning", "age.yesterday", "age.two_days", "age.few_days", "age.last_week", "age.fortnight", "age.last_month",
        };

        // the news that can make a line, the newest first: worked out again when something is learned or the day asked changes
        static readonly List<Fact> recentNews = new List<Fact>();
        static int recentVersion = -1;
        static double recentToday = double.NaN, recentDays = double.NaN;

        // ------------------------------------------------------------------------------------------ asking

        /// <summary>The live game's day, date, the port she lies at and the world's ships.</summary>
        public static WordAsk AskNow()
        {
            var game = KapmanGame.Current;
            if (game == null)
                return new WordAsk();
            var world = game.World;
            return new WordAsk { today = game.Day, month = game.Month, year = game.Year, here = GameData.Port(Port.LyingId()), ships = world?.ships };
        }

        /// <summary>The one line of what the merchant knows of a good at a port, now (the live game).</summary>
        public static GoodWord Word(string good, string port) => Word(GameData.Good(good), GameData.Port(port), AskNow());

        /// <summary>
        /// The one line of what the merchant knows of a good at a port. Eight rules in order, and the first that fires
        /// wins: (1) hard news of the place that touches the good's market, younger than 60 days; (2) a price seen there
        /// with the merchant's own eyes, younger than 7 days; (3) a price told by someone at first or second hand, younger
        /// than 14 days; (4) talk and rumour, and whatever else is noted and not yet 90 days old; (5) the season: what
        /// comes to market and what is wanted there this month, and the good's own cheap and dear months; (6) ships that
        /// left for there ahead of her with the good aboard; (7) the law on the good there; (8) nothing known. The days
        /// are in knowing_793.json ("word"). A price is said AGAINST THE PRICE WHERE SHE LIES (<see cref="Here"/>): a
        /// price she saw within the week with its figure and the difference ("2 d more than here"); one told by a man
        /// who saw it, or her own older sight, as a rough figure ("about 36 d"); talk and rumour as a word ("more than
        /// here"). Only with nothing to set it against is it "well", "poorly" or "the usual" by the good's table value.
        /// The same every time for the same knowledge and day: nothing is drawn, and no market is asked.
        /// </summary>
        public static GoodWord Word(GoodDef good, PortDef port, WordAsk ask)
        {
            Now(ref ask);
            ask = AskFor(good, ask);
            if (good == null || port == null)
                return Make(8, "none.anywhere", WordMark.None, 0, good, port, ask, double.NaN);
            if (TryKnown(good, port, ask, out var w))
                return w;
            if (SeasonWord(good, port, ask, out w))
                return w;
            if (AheadWord(good, port, ask, out w))
                return w;
            if (LawWord(good, port, ask, out w))
                return w;
            return NothingWord(good, port, ask);
        }

        /// <summary>
        /// The note on the item itself, for the market's rows and the hold: the freshest thing known of the good anywhere
        /// but here (rules 1 to 4 over every port: the earliest rule first, then the youngest word, and one that leans
        /// before one that does not); with nothing such, the season here, the ships that left here with it, the law here.
        /// </summary>
        public static GoodWord WordOn(GoodDef good, WordAsk ask)
        {
            Now(ref ask);
            ask = AskFor(good, ask);
            if (good == null)
                return Make(8, "none.anywhere", WordMark.None, 0, null, null, ask, double.NaN);
            var best = default(GoodWord);
            bool have = false;
            var ports = GameData.Ports;
            for (int i = 0; i < ports.Count; i++)
            {
                if (ports[i] == ask.here || !TryKnown(good, ports[i], ask, out var w))
                    continue;
                if (!have || Better(w, best))
                {
                    best = w;
                    have = true;
                }
            }
            if (have)
                return best;
            if (ask.here != null && SeasonWord(good, ask.here, ask, out var s))
                return s;
            if (AheadAnywhere(good, ask, out var a))
                return a;
            if (ask.here != null && LawWord(good, ask.here, ask, out var l))
                return l;
            return Make(8, "none.anywhere", WordMark.None, 0, good, null, ask, double.NaN);
        }

        /// <summary>Is a the better note for an item than b: the earlier rule, then the younger age word, then one that
        /// leans, then the younger by the day.</summary>
        static bool Better(GoodWord a, GoodWord b)
        {
            if (a.rule != b.rule)
                return a.rule < b.rule;
            int ra = AgeRank(a.ageKey), rb = AgeRank(b.ageKey);
            if (ra != rb)
                return ra < rb;
            if ((a.tilt == 0) != (b.tilt == 0))
                return a.tilt != 0;
            return !double.IsNaN(a.age) && (double.IsNaN(b.age) || a.age < b.age - 1e-9);
        }

        static int AgeRank(string key)
        {
            int i = Array.IndexOf(AgeOrder, key ?? "");
            return i >= 0 ? i : AgeOrder.Length;
        }

        /// <summary>The month and year of the day asked, by the economy's calendar, where the asker gave none.</summary>
        static void Now(ref WordAsk ask)
        {
            if (ask.month >= 1 && ask.month <= 12 && ask.year > 0)
                return;
            Seasons.DateOf(ask.today, out int year, out int doy);
            if (ask.year <= 0)
                ask.year = year;
            if (ask.month < 1 || ask.month > 12)
                ask.month = Seasons.MonthOfIndex(Seasons.Index(year, doy));
        }

        // ------------------------------------------------------------------------------------------ the words

        /// <summary>
        /// An age in words, never in days, as a wording key: "age.this_morning" (the same day), "age.yesterday",
        /// "age.two_days", "age.few_days" (three to six days), "age.last_week" (7-10), "age.fortnight" (11-20),
        /// "age.last_month" (21-59), "age.since_spring" (and summer, autumn, winter: 60 days or more, in an earlier
        /// season than now), "age.old_talk" (most of a year or more). `day`: the game day it was true.
        /// </summary>
        public static string AgeKey(double day, double today)
        {
            if (double.IsNaN(day) || double.IsInfinity(day))
                return "age.old_talk";
            double days = Math.Max(0.0, Math.Floor(today) - Math.Floor(day));
            if (days < 1.0) return "age.this_morning";
            if (days < 2.0) return "age.yesterday";
            if (days < 3.0) return "age.two_days";
            if (days < 7.0) return "age.few_days";
            if (days < 11.0) return "age.last_week";
            if (days < 21.0) return "age.fortnight";
            if (days < 60.0) return "age.last_month";
            if (days >= 270.0)
                return "age.old_talk";
            string then = SeasonOf(day), now = SeasonOf(today);
            if (then != now)
                return "age.since_" + then;
            return days < 90.0 ? "age.last_month" : "age.old_talk";
        }

        /// <summary>The season a game day falls in: winter, spring, summer or autumn (as the game's clock names them).</summary>
        static string SeasonOf(double day)
        {
            Seasons.DateOf(day, out _, out int doy);
            return KapmanGame.SeasonOf(doy);
        }

        /// <summary>How a market pays for a good, as a wording key and a tilt: "pays.well" (+1) at the rules' share of the
        /// good's table value or more, "pays.poorly" (-1) at the lower share or less, else "pays.fair" (0).</summary>
        public static string PaysKey(float pays, GoodDef good, out int tilt)
        {
            float ratio = good != null && good.value > 0f ? pays / good.value : 1f;
            tilt = ratio >= Rules.paysWell ? 1 : ratio <= Rules.paysPoorly ? -1 : 0;
            return tilt > 0 ? "pays.well" : tilt < 0 ? "pays.poorly" : "pays.fair";
        }

        // ------------------------------------------------------------------------------------------ against where she lies

        /// <summary>The ask with the price it is said against worked out for this good: once for a line, or once for a
        /// row of the ledger (ask every cell of the row with what this returns).</summary>
        public static WordAsk AskFor(GoodDef good, WordAsk ask)
        {
            if (good != null && ask.againstGood != good.id)
            {
                ask.against = Here(good, ask);
                ask.againstGood = good.id;
            }
            return ask;
        }

        /// <summary>
        /// The price the word on a good is said against (the strategy critic, 7 October: "say it against where she
        /// lies"): what she saw at the port she lies at, once she has seen its market (the frame notes it when she
        /// moors: FrameDeal.See); else the last price of the good she saw with her own eyes anywhere, while it is young
        /// enough to count (knowing_793.json "word": known_days). From the price notes alone: no market is asked.
        /// </summary>
        public static WordHere Here(GoodDef good, WordAsk ask)
        {
            var here = default(WordHere);
            if (good == null || !Trade.Started)
                return here;
            if (ask.here != null && OwnSight(ask.here.id, good.id, ask.today, out double at, out var seen))
                return new WordHere { port = ask.here, lying = true, pays = seen.pays, asks = seen.asks, onOffer = seen.onOffer, day = at };
            var ports = GameData.Ports;
            for (int i = 0; i < ports.Count; i++)
            {
                if (!OwnSight(ports[i].id, good.id, ask.today, out at, out seen) || (here.port != null && at <= here.day))
                    continue;
                here = new WordHere { port = ports[i], lying = false, pays = seen.pays, asks = seen.asks, onOffer = seen.onOffer, day = at };
            }
            return here;
        }

        /// <summary>The newest price of a good the merchant saw with her own eyes at a port, while it still counts.</summary>
        static bool OwnSight(string port, string good, double today, out double day, out SeenPrice price)
        {
            day = double.NaN;
            price = default;
            var notes = Trade.Ledger.History(port);
            for (int i = notes.Count - 1; i >= 0; i--)
            {
                var n = notes[i];
                double age = today - n.day;
                if (age < 0.0)
                    continue;
                if (age >= Rules.wordKnownDays)
                    return false;                                        // (the notes before it are older still)
                if ((n.how == How.Seen || n.how == How.Own) && n.prices.TryGetValue(good, out price))
                {
                    day = n.day;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// The share by which the price at the place of sale must stand over the price at the place of buying before
        /// carrying the good from the one to the other pays: the market's own gap between what it asks and what it pays
        /// (trade_rules.json "spread"), and the toll where it is sold. With a spread of 0.2 that is 0.36 into a port that
        /// takes a tenth and 0.22 into one that takes no toll. This is where "far more than here" begins, so the word
        /// means: buying it here to sell it there would pay, if the word still holds.
        /// </summary>
        public static float FarOver(PortDef sellAt)
        {
            var rules = TradeRules.Current;
            if (rules == null)
                return 0.25f;
            float half = Math.Max(0f, Math.Min(0.9f, rules.spread * 0.5f));
            float toll = sellAt != null ? rules.ZoneOf(sellAt.region)?.toll ?? 0f : 0f;
            return (1f + half) / (1f - half) / Math.Max(0.5f, 1f - toll) - 1f;
        }

        /// <summary>How a price there stands against a price here: +2 far more (by `farUp` or more: carrying it there
        /// pays), +1 more, 0 much the same (within knowing_793.json "word" same_within), -1 less, -2 far less (so much
        /// less that carrying it from there to here pays: `farDown`).</summary>
        public static int Lean(float there, float here, float farUp, float farDown)
        {
            if (float.IsNaN(there) || float.IsNaN(here) || here <= 0f || there <= 0f)
                return 0;
            float r = there / here, same = Math.Max(0f, Rules.sameWithin);
            if (r >= 1f + Math.Max(farUp, same)) return 2;
            if (r >= 1f + same) return 1;
            if (r <= 1f / (1f + Math.Max(farDown, same))) return -2;
            if (r <= 1f / (1f + same)) return -1;
            return 0;
        }

        static string LeanKey(int lean) => lean >= 2 ? "far_more" : lean == 1 ? "more" : lean == 0 ? "same" : lean == -1 ? "less" : "far_less";

        /// <summary>The reckoning a price is said in: that of the port the word is said against (so two prices can be set
        /// side by side), and with nothing to set it against the port's own.</summary>
        public static ZoneDef Reckoning(PortDef port, WordAsk ask) => TradeRules.Current?.ZoneOf((ask.against.port ?? port)?.region);

        static string Shown(double deniers, ZoneDef zone) =>
            zone != null ? zone.Show(deniers) : deniers.ToString("0", CultureInfo.InvariantCulture) + " d";

        /// <summary>A price as a rough figure ("36 d", said as "about 36 d"): to the whole unit under twenty, to the
        /// nearest two under fifty, to the nearest five under two hundred, then to the nearest ten, so it cannot be taken
        /// for a price seen.</summary>
        public static string Rough(double deniers, ZoneDef zone)
        {
            double v = deniers / (zone != null ? Math.Max(0.0001, zone.unitDeniers) : 1.0);
            double step = v < 20.0 ? 1.0 : v < 50.0 ? 2.0 : v < 200.0 ? 5.0 : 10.0;
            double r = Math.Max(step, Math.Round(v / step, MidpointRounding.AwayFromZero) * step);
            return r.ToString("0", CultureInfo.InvariantCulture) + " " + (zone != null ? zone.unit : "d");
        }

        /// <summary>"here", or "at Dorestat" where the word is said against the last price she saw and not where she lies.</summary>
        static string HereWords(PortDef port, WordAsk ask) =>
            ask.against.lying ? Wording.WordText(port, "here.lying")
                              : Wording.Fill(Wording.WordText(port, "here.last"), ("place", ask.against.port != null ? ask.against.port.name : ""));

        /// <summary>A price as words against where she lies: "more than here", "far less than at Dorestat".</summary>
        static string AgainstWords(PortDef port, WordAsk ask, int lean) =>
            Wording.Fill(Wording.WordText(port, "against." + LeanKey(lean)), ("here", HereWords(port, ask)));

        /// <summary>The sentence that sets a figure against where she lies ("That is 2 d more than here."): the difference
        /// as a figure when both prices were seen (`exact`), else as a word.</summary>
        static string AgainstLine(PortDef port, WordAsk ask, PriceCell cell, bool exact)
        {
            string words = AgainstWords(port, ask, cell.paysLean);
            if (exact)
            {
                var zone = Reckoning(port, ask);
                double unit = zone != null ? Math.Max(0.0001, zone.unitDeniers) : 1.0;
                double by = cell.pays - ask.against.pays;
                if (Math.Abs(by) / unit >= 0.05)
                    words = Wording.Fill(Wording.WordText(port, by > 0.0 ? "against.by.more" : "against.by.less"),
                                         ("diff", Shown(Math.Abs(by), zone)), ("here", HereWords(port, ask)));
                else
                    words = AgainstWords(port, ask, 0);
            }
            return Wording.Fill(Wording.WordText(port, "against.line"), ("against", words));
        }

        /// <summary>
        /// ONE CELL OF THE LEDGER: what the merchant knows of a good's price at a port, by the same notes and the same
        /// order as the word on the item (rules 2 to 4), so the cell and the line never disagree. Ask a whole row with
        /// one <see cref="AskFor"/>.
        /// </summary>
        public static PriceCell Cell(GoodDef good, PortDef port, WordAsk ask)
        {
            Now(ref ask);
            if (good == null || port == null)
                return new PriceCell { ageKey = "", pays = float.NaN, asks = float.NaN, day = double.NaN };
            return CellOf(good, port, AskFor(good, ask));
        }

        static PriceCell CellOf(GoodDef good, PortDef port, WordAsk ask)
        {
            var cell = new PriceCell { ageKey = "", pays = float.NaN, asks = float.NaN, day = double.NaN, goodId = good.id };
            if (!Trade.Started)
                return cell;
            var notes = Trade.Ledger.History(port.id);
            PriceNote seen = null, told = null, other = null;
            for (int i = notes.Count - 1, looked = 0; i >= 0 && looked < 12; i--)
            {
                var n = notes[i];
                if (!n.prices.ContainsKey(good.id))
                    continue;
                looked++;
                double age = ask.today - n.day;
                if (age < 0.0)
                    continue;
                if (age >= Rules.wordKnownDays)
                    break;                                               // (the notes before it are older still)
                bool mine = n.how == How.Seen || n.how == How.Own;
                if (mine && age < Rules.wordSeenDays)
                    seen = seen ?? n;
                else if (!mine && n.how != How.Rumour && n.how != How.Lore && n.hops <= 1 && age < Rules.wordToldDays)
                    told = told ?? n;
                else
                    other = other ?? n;
            }
            var note = seen ?? told ?? other;
            if (note == null)
                return cell;
            var price = note.prices[good.id];
            bool own = note.how == How.Seen || note.how == How.Own;
            bool firstHand = note == told && note.hops == 0 && (note.how == How.Told || note.how == How.Written);
            cell.note = note;
            cell.rule = note == seen ? 2 : note == told ? 3 : 4;
            cell.grade = note == seen ? PriceGrade.Exact : own || (firstHand && Rules.toldFigure) ? PriceGrade.About : PriceGrade.Word;
            cell.pays = price.pays;
            cell.onOffer = price.onOffer;
            cell.asks = price.onOffer ? price.asks : float.NaN;
            cell.day = note.day;
            cell.ageKey = AgeKey(note.day, ask.today);
            if (cell.ageKey.StartsWith("age.since_", StringComparison.Ordinal))
                cell.ageKey = "age.old_talk";
            var here = ask.against;
            cell.compared = here.Known && here.port != port;
            if (cell.compared)
            {
                float up = FarOver(port), down = FarOver(here.port);
                cell.paysLean = Lean(price.pays, here.pays, up, down);
                cell.asksLean = price.onOffer && here.onOffer ? Lean(price.asks, here.asks, up, down) : 0;
            }
            return cell;
        }

        /// <summary>
        /// A cell as it is written in the ledger: the figure she saw ("36 d"), a rough figure ("about 36 d"), a word
        /// against where she lies ("far more", "cheaper"), "none" where none was on offer (`asks`), "" where nothing is
        /// known. In the reckoning of the port the row is said against. `ask` is the row's (<see cref="AskFor"/>).
        /// </summary>
        public static string CellText(PriceCell cell, PortDef port, WordAsk ask, bool asks)
        {
            if (cell.grade == PriceGrade.None)
                return "";
            if (asks && !cell.onOffer)
                return Wording.WordText(port, "cell.none");
            var zone = Reckoning(port, ask);
            float v = asks ? cell.asks : cell.pays;
            if (cell.grade == PriceGrade.Exact)
                return Shown(v, zone);
            if (cell.grade == PriceGrade.About)
                return Wording.Fill(Wording.WordText(port, "cell.about"), ("price", Rough(v, zone)));
            if (!cell.compared)
                return asks ? Wording.WordText(port, "cell.offer") : Wording.WordText(port, PaysKey(cell.pays, GameData.Good(cell.goodId), out _));
            if (asks && !ask.against.onOffer)
                return Wording.WordText(port, "cell.offer");
            return Wording.WordText(port, (asks ? "cell.asks." : "cell.") + LeanKey(asks ? cell.asksLean : cell.paysLean));
        }

        /// <summary>A good's name as it reads in the middle of a line: without what is in brackets, and with a small first
        /// letter unless its first word is a place's or a people's ("Rhenish wine", "salt herring").</summary>
        public static string GoodName(GoodDef g)
        {
            if (g == null || string.IsNullOrEmpty(g.name))
                return "";
            string n = g.name;
            int cut = n.IndexOf(" (", StringComparison.Ordinal);
            if (cut > 0)
                n = n.Substring(0, cut);
            int space = n.IndexOf(' ');
            string first = space > 0 ? n.Substring(0, space) : n;
            return ProperFirst.Contains(first) ? n : char.ToLowerInvariant(n[0]) + n.Substring(1);
        }

        /// <summary>
        /// Who told a price, as a person and never a label: a skipper of his people ("a Frisian skipper"), someone on the
        /// waterfront it was heard at ("the toll man at Dorestat": one of the table's, the same one every time for the
        /// same note), or, where the note does not say, a man who saw it, a letter, a man on the quay.
        /// </summary>
        public static string Person(PriceNote n)
        {
            if (n == null)
                return Wording.WordText(null, "person.someone");
            var of = GameData.Port(n.tellerOf);
            string teller = string.IsNullOrEmpty(n.teller) ? (n.how == How.Written ? "letter" : n.how == How.Told ? "witness" : "someone") : n.teller;
            if (teller == "skipper")
                return of == null ? Wording.WordText(null, "person.skipper.plain")
                                  : Wording.Fill(Wording.WordText(of, "person.skipper"), ("people", Wording.WordText(of, "people." + Wording.CultureOf(of))), ("place", of.name));
            if (teller == "quay")
            {
                if (of == null)
                    return Wording.WordText(null, "person.someone");
                // (8 Oct 2026, the alpha's row 5: "a price told rightly and by whom". One of that port's own people, by
                // name, where its page has somebody whose trade hears what markets pay; else one of the table's four)
                var named = QuayTeller(n);
                if (named != null)
                    return Wording.Fill(Wording.WordText(of, "person.named"), ("name", PlaceBook.Titled(named)), ("place", of.name));
                return Wording.Fill(Wording.WordText(of, "person.quay", Hash01(n.port + "/" + n.tellerOf, n.day, 0, 17)), ("place", of.name));
            }
            string who = Wording.WordText(of, "person." + teller);
            return who.Length > 0 ? who : Wording.WordText(of, "person.someone");
        }

        /// <summary>Who on that waterfront told a price: one of the port's own people whose trade hears what the markets
        /// pay (PlaceBook.Tellers), the same one every time for the same note; null where its page has none, or the
        /// note is not talk of a waterfront.</summary>
        public static PersonDef QuayTeller(PriceNote n)
        {
            if (n == null || n.teller != "quay" || string.IsNullOrEmpty(n.tellerOf))
                return null;
            var at = PlaceBook.Current.TellersAt(n.tellerOf, KapmanGame.Current != null ? KapmanGame.Current.Year : 793);
            if (at.Count == 0)
                return null;
            return at[Math.Min(at.Count - 1, (int)(Hash01(n.port + "/" + n.tellerOf, n.day, 0, 17) * at.Count))];
        }

        static string Sentence(string text)
        {
            string t = (text ?? "").Trim();
            if (t.Length == 0)
                return "";
            char end = t[t.Length - 1];
            return end == '.' || end == '!' || end == '?' ? t : t + ".";
        }

        /// <summary>A word with the slots every line has (good, port, month) and its own, and its age from the day it
        /// rests on (NaN: it has none).</summary>
        static GoodWord Make(int rule, string key, WordMark mark, int tilt, GoodDef good, PortDef port, WordAsk ask, double day,
                             params (string name, string value)[] own)
        {
            var slots = new (string name, string value)[own.Length + 4];
            slots[0] = ("good", GoodName(good));
            slots[1] = ("port", port != null ? port.name : "");
            slots[2] = ("month", ask.month >= 1 && ask.month <= 12 ? Kapman.World.Calendar.MonthNames[ask.month - 1] : "");
            string ageKey = double.IsNaN(day) ? "" : AgeKey(day, ask.today);
            // talk two or three months old is old talk ("since spring" is for the line that says nothing has been heard)
            if (rule != 8 && ageKey.StartsWith("age.since_", StringComparison.Ordinal))
                ageKey = "age.old_talk";
            slots[3] = ("age", ageKey.Length > 0 ? Wording.WordText(port, ageKey) : "");
            Array.Copy(own, 0, slots, 4, own.Length);
            return new GoodWord
            {
                rule = rule, key = key, mark = mark, tilt = tilt, good = good != null ? good.id : "", port = port != null ? port.id : "",
                slots = slots, age = double.IsNaN(day) ? double.NaN : Math.Max(0.0, ask.today - day), ageKey = ageKey,
            };
        }

        // ------------------------------------------------------------------------------------------ rules 1 to 4: what is known

        /// <summary>Rules 1 to 4 for a good at a port: hard news, a price seen, a price told, talk and rumour.</summary>
        static bool TryKnown(GoodDef good, PortDef port, WordAsk ask, out GoodWord w) =>
            NewsWord(good, port, ask, out w) || LedgerWord(good, port, ask, out w) || FairWord(good, port, ask, out w);

        /// <summary>The news heard that can make a line (a raid, an entry of the chronicle that shocks markets, a fair),
        /// not yet too old, the newest first.</summary>
        static List<Fact> RecentNews(double today)
        {
            double days = Rules.wordNewsDays;
            if (recentVersion == Version && recentToday == today && recentDays == days)
                return recentNews;
            recentVersion = Version;
            recentToday = today;
            recentDays = days;
            recentNews.Clear();
            foreach (var f in news.Values)
            {
                double age = today - f.trueDay;
                if (age < 0.0 || age >= days)
                    continue;
                if (f.kind == "raid" || f.kind == "fair" || Shocks.For(Chronicle.Entry(f.chronicleId)) != null)
                    recentNews.Add(f);
            }
            recentNews.Sort((a, b) => a.trueDay != b.trueDay ? b.trueDay.CompareTo(a.trueDay) : a.id.CompareTo(b.id));
            return recentNews;
        }

        /// <summary>Did the news happen at this port: the port it names, or for an entry of the chronicle the ports it
        /// names (or every port of its regions).</summary>
        static bool NewsAt(Fact f, PortDef port, int year)
        {
            var e = Chronicle.Entry(f.chronicleId);
            return e != null ? Shocks.PortsOf(e, year).Contains(port) : f.port == port.index;
        }

        /// <summary>Rule 1: hard news of the place that touches the good's market ("Walcheren was burned in June. Nobody
        /// there is buying."): the market shut (while the shock shuts it), or the good dear or cheap there since.</summary>
        static bool NewsWord(GoodDef good, PortDef port, WordAsk ask, out GoodWord w)
        {
            w = default;
            foreach (var f in RecentNews(ask.today))
            {
                if (f.kind == "fair" || !NewsAt(f, port, ask.year))
                    continue;
                var e = Chronicle.Entry(f.chronicleId);
                string key;
                int tilt;
                if (e == null)
                {
                    // the world's own raiders: the stock carried off, and what is left dear
                    key = "news.dear";
                    tilt = 1;
                }
                else
                {
                    var s = Shocks.For(e);
                    if (s == null || !Shocks.Touches(s.goods, good))
                        continue;
                    if (ask.today - f.trueDay < s.disruptDays)
                    {
                        key = "news.shut";
                        tilt = -1;
                    }
                    else
                    {
                        key = s.dearer > 0f ? "news.dear" : s.dearer < 0f ? "news.cheap" : "news";
                        tilt = s.dearer > 0f ? 1 : s.dearer < 0f ? -1 : 0;
                    }
                }
                w = Make(1, key, f.how == How.Seen || f.how == How.Own ? WordMark.Seen : WordMark.Heard, tilt, good, port, ask, f.trueDay,
                         ("news", Sentence(f.text)));
                return true;
            }
            return false;
        }

        /// <summary>Rules 2, 3 and 4 from the price notes, each said against where she lies (<see cref="Here"/>): a price
        /// seen lately (its figure, and the difference as a figure); a price told lately by someone at first or second
        /// hand (a man who saw it himself: "about" and a rough figure; one mouth further: a word); and whatever else is
        /// noted and not yet too old: talk through more mouths or grown old ("they say"), a rumour, the merchant's own
        /// older sight of it (a rough figure: she did see it). The tilt is the comparison's: up when they pay more there
        /// than here. Only with no price to set it against at all is it the good's table value that speaks.</summary>
        static bool LedgerWord(GoodDef good, PortDef port, WordAsk ask, out GoodWord w)
        {
            w = default;
            var cell = CellOf(good, port, ask);
            var note = cell.note;
            if (note == null)
                return false;
            var zone = Reckoning(port, ask);
            int tilt;
            string pays, against = "";
            if (cell.compared)
            {
                tilt = Math.Sign(cell.paysLean);
                pays = AgainstWords(port, ask, cell.paysLean);
                against = AgainstLine(port, ask, cell, cell.grade == PriceGrade.Exact);
            }
            else
            {
                // nothing to set it against: the word every merchant would use of the good (and no lean at all where the
                // port asked about is itself the one place she has seen it)
                pays = Wording.WordText(port, PaysKey(cell.pays, good, out tilt));
                if (ask.against.Known)
                    tilt = 0;
            }
            if (cell.rule == 2)
                w = Make(2, "seen", WordMark.Seen, tilt, good, port, ask, note.day, ("price", Shown(cell.pays, zone)), ("pays", pays), ("against", against));
            else if (cell.rule == 3 && cell.grade == PriceGrade.About)
                w = Make(3, "told.about", WordMark.Heard, tilt, good, port, ask, note.day,
                         ("person", Person(note)), ("price", Rough(cell.pays, zone)), ("pays", pays), ("against", against));
            else if (cell.rule == 3)
                w = Make(3, "told", WordMark.Heard, tilt, good, port, ask, note.day, ("person", Person(note)), ("pays", pays));
            else if (note.how == How.Seen || note.how == How.Own)
                w = Make(4, "saw", WordMark.Seen, tilt, good, port, ask, note.day, ("price", Rough(cell.pays, zone)), ("pays", pays), ("against", against));
            else if (note.how == How.Rumour)
                w = Make(4, "rumour", WordMark.Heard, tilt, good, port, ask, note.day, ("pays", pays));
            else
            {
                // talk has a place where it was heard on a waterfront ("they say at Quentovic"); a skipper's port is his home
                var at = note.teller == "quay" ? GameData.Port(note.tellerOf) : null;
                w = Make(4, at != null ? "talk.at" : "talk", WordMark.Heard, tilt, good, port, ask, note.day,
                         ("place", at != null ? at.name : ""), ("pays", pays));
            }
            return true;
        }

        /// <summary>Rule 4, the news side: word that a fair is on there (its market full, and a little cheaper).</summary>
        static bool FairWord(GoodDef good, PortDef port, WordAsk ask, out GoodWord w)
        {
            w = default;
            foreach (var f in RecentNews(ask.today))
            {
                if (f.kind != "fair" || f.port != port.index || ask.today - f.trueDay >= 30.0)
                    continue;
                w = Make(4, "talk.fair", WordMark.Heard, -1, good, port, ask, f.trueDay, ("news", Sentence(f.text)));
                return true;
            }
            return false;
        }

        // ------------------------------------------------------------------------------------------ rules 5 to 8

        /// <summary>Rule 5, the season: what the port's region wants and what comes to its markets this month
        /// (trade_year_793.json), then the good's own dear and cheap months (the goods table).</summary>
        static bool SeasonWord(GoodDef good, PortDef port, WordAsk ask, out GoodWord w)
        {
            w = default;
            var month = TradeYear.Of(port.region, ask.month);
            string line = month != null ? month.line : "";
            if (month != null && month.Wants(good.id))
                w = Make(5, "season.wanted", WordMark.Season, 1, good, port, ask, double.NaN, ("line", line));
            else if (month != null && month.Comes(good.id))
                w = Make(5, "season.comes", WordMark.Season, -1, good, port, ask, double.NaN, ("line", line));
            else if (Array.IndexOf(good.dear, ask.month) >= 0)
                w = Make(5, "season.dear", WordMark.Season, 1, good, port, ask, double.NaN, ("line", line));
            else if (Array.IndexOf(good.cheap, ask.month) >= 0)
                w = Make(5, "season.cheap", WordMark.Season, -1, good, port, ask, double.NaN, ("line", line));
            else
                return false;
            return true;
        }

        /// <summary>Is this one of the world's ships that left the port she lies at lately, with the good aboard, and is
        /// still on her way?</summary>
        static bool SailedAhead(SimShip s, GoodDef good, WordAsk ask)
        {
            if (s == null || s.raider || s.isPlayer || s.dest < 0 || s.at != ask.here.index || s.dest == ask.here.index)
                return false;
            if (s.state != ShipState.Sailing && s.state != ShipState.Anchored)
                return false;
            double ago = ask.today - s.departedDay;
            return ago >= 0.0 && ago <= Rules.wordAheadDays && s.hold != null && s.hold.Count(good.id) > 0;
        }

        static GoodWord Ahead(GoodDef good, PortDef port, WordAsk ask, int ships, double latest) =>
            Make(6, "ahead", WordMark.Seen, -1, good, port, ask, latest, ("ships", Wording.WordText(port, ships >= 5 ? "ships.many" : "ships." + ships)));

        /// <summary>Rule 6, who sailed ahead: ships that left the port she lies at for that port in the last days, with
        /// the good aboard ("Three ships left for Lundenwic before you with wine aboard.").</summary>
        static bool AheadWord(GoodDef good, PortDef port, WordAsk ask, out GoodWord w)
        {
            w = default;
            if (ask.ships == null || ask.here == null || ask.here == port)
                return false;
            int count = 0;
            double latest = double.MinValue;
            for (int i = 0; i < ask.ships.Count; i++)
            {
                var s = ask.ships[i];
                if (s == null || s.dest != port.index || !SailedAhead(s, good, ask))
                    continue;
                count++;
                latest = Math.Max(latest, s.departedDay);
            }
            if (count == 0)
                return false;
            w = Ahead(good, port, ask, count, latest);
            return true;
        }

        /// <summary>Rule 6 for the note on an item: the port most of the ships that left here with the good are bound for
        /// (the first in the ports table among equals).</summary>
        static bool AheadAnywhere(GoodDef good, WordAsk ask, out GoodWord w)
        {
            w = default;
            if (ask.ships == null || ask.here == null)
                return false;
            var counts = new Dictionary<int, (int ships, double latest)>();
            for (int i = 0; i < ask.ships.Count; i++)
            {
                var s = ask.ships[i];
                if (s == null || !SailedAhead(s, good, ask) || s.dest >= GameData.Ports.Count)
                    continue;
                counts.TryGetValue(s.dest, out var c);
                counts[s.dest] = (c.ships + 1, c.ships == 0 ? s.departedDay : Math.Max(c.latest, s.departedDay));
            }
            int best = -1;
            foreach (var kv in counts)
                if (best < 0 || kv.Value.ships > counts[best].ships || (kv.Value.ships == counts[best].ships && kv.Key < best))
                    best = kv.Key;
            if (best < 0)
                return false;
            w = Ahead(good, GameData.Ports[best], ask, counts[best].ships, counts[best].latest);
            return true;
        }

        /// <summary>Rule 7, the law on the good there this year (the latest of them: death, from 864, over the older
        /// ban): "law.no_export", "law.death", "law.no_sale".</summary>
        static bool LawWord(GoodDef good, PortDef port, WordAsk ask, out GoodWord w)
        {
            w = default;
            var rules = TradeRules.Current;
            var zone = rules?.ZoneOf(port.region);
            if (zone == null || string.IsNullOrEmpty(good.law))
                return false;
            var laws = rules.LawsOn(good, zone, ask.year);
            if (laws.Count == 0)
                return false;
            var law = laws[laws.Count - 1];
            w = Make(7, "law." + (law.rule ?? "").Trim().Replace(' ', '_'), WordMark.Law, 0, good, port, ask, double.NaN,
                     ("realm", zone.name ?? ""), ("law", law.note ?? ""));
            return true;
        }

        /// <summary>Rule 8, nothing known: never a word of it there ("none"), or none since a season gone by
        /// ("none.since": "No word on wine at Lundenwic since spring."), or only old talk ("none.old").</summary>
        static GoodWord NothingWord(GoodDef good, PortDef port, WordAsk ask)
        {
            double day = double.NaN;
            if (Trade.Started)
            {
                var notes = Trade.Ledger.History(port.id);
                for (int i = notes.Count - 1; i >= 0; i--)
                    if (notes[i].day <= ask.today && notes[i].prices.ContainsKey(good.id))
                    {
                        day = notes[i].day;
                        break;
                    }
            }
            if (double.IsNaN(day))
                return Make(8, "none", WordMark.None, 0, good, port, ask, double.NaN);
            bool since = AgeKey(day, ask.today).StartsWith("age.since_", StringComparison.Ordinal);
            return Make(8, since ? "none.since" : "none.old", WordMark.None, 0, good, port, ask, day);
        }
    }
}
