using System;
using System.Collections.Generic;
using System.Globalization;

namespace Kapman.Game
{
    /// <summary>
    /// THE FIRST JOBS (the user, 7 Oct 2026: Ravn explains "the first objective to gather up crew mates to set sail for
    /// the first voyage, maybe then discover that the sail is in poor condition and needs to be mended before they can
    /// set sail"; asked whether these are real rules or only told: "Real, simple"). A game begun through the opening
    /// starts with one man aboard, Ravn, and a worn sail. Before she can cast off:
    ///   - HANDS: men are hired on the quay of the home port until she has her crew (hand money down, a few pennies a
    ///     man). The hands on offer are the three of the crew the cards know: Aldger, Brunger and Beornwine.
    ///   - THE SAIL: mended at the port for a little silver and some hours of the clock.
    /// Cast off is refused, in words, until both are done. Then the jobs are over and nothing here is asked again.
    /// The men hired are a count, as the crew is everywhere in the game today: their names are said and not kept.
    /// THE LIST (the critic's play of 8 Oct 2026, B1, taken up by the user: "after Ravn stops, nothing on screen holds
    /// what he said"): from the first morning until she casts off the first time the jobs stand on the screen as a
    /// list that ticks off: the sail, the crew, a cargo (Ravn's word on what to buy and where to take it), a course,
    /// cast off. <see cref="Listed"/> is that time; the frame draws the list under the buttons (FrameRoot.Jobs.cs).
    /// Pure rules with the game's purse and crew: the frame shows them and asks.
    /// </summary>
    public static class FirstJobs
    {
        /// <summary>Hand money for a man, the cost of mending the sail (deniers), and the hours it takes.</summary>
        public const double HandMoney = 4.0, MendCost = 12.0, MendHours = 6.0;
        /// <summary>The men she must have aboard before she puts out the first time.</summary>
        public const int CrewWanted = 4;

        public sealed class Hand
        {
            public string name = "", line = "";
            public bool hired;
        }

        /// <summary>
        /// THE MEN ON THE QUAY ARE THE CREW OF THE CARDS (8 Oct 2026, night). Until then five names were made up for
        /// the quay, three of them were hired, and from the first card on the crew were Aldger, Brunger and
        /// Beornwine, whom nobody had hired. Now those three are the men who look for a boat on the first morning:
        /// who each is, in a line (from the sheets of the story cast). Placeholders for the words of the writer.
        /// </summary>
        static readonly (string name, string line)[] Crew =
        {
            ("Aldger", "A steersman out of Doccinga. Says the worst first, and then what can be done about it."),
            ("Brunger", "A Saxon off the Weser, heavy in the shoulder. Few words, and those slow."),
            ("Beornwine", "An English boy of eighteen with ink on his fingers. Sharp eyes. Talks when he is afraid."),
        };

        /// <summary>The jobs are set and not yet all done.</summary>
        public static bool Active { get; private set; }
        /// <summary>The port they are done at (her home).</summary>
        public static string Port { get; private set; } = "";
        public static bool SailWorn { get; private set; }
        /// <summary>The game day the mending is done at; below 0 when nobody is at it.</summary>
        public static double MendDoneAt { get; private set; } = -1.0;
        public static bool Mending => MendDoneAt >= 0.0;
        public static IReadOnlyList<Hand> Hands => hands;
        /// <summary>Counts up with every change (a panel's stamp).</summary>
        public static int Version { get; private set; }

        static readonly List<Hand> hands = new List<Hand>();

        public static int HandsShort => Active ? Math.Max(0, CrewWanted - Voyage.Men) : 0;

        /// <summary>The list "Before she sails" stands on the screen: from the first morning until she first casts off.</summary>
        public static bool Listed { get; private set; }
        /// <summary>Ravn's word on the first cargo: the good to buy at home and the port that pays for it ("" none).</summary>
        public static string TipGood { get; private set; } = "";
        public static string TipPort { get; private set; } = "";
        /// <summary>Something has been bought for the hold (what came with the boat is not bought).</summary>
        public static bool Bought
        {
            get
            {
                if (!Trade.Started)
                    return false;
                foreach (var lot in Trade.Hold.Lots)
                    if (lot.whose.Length == 0 && lot.cost > 0.0)
                        return true;
                return false;
            }
        }

        /// <summary>Ravn has said what to buy and where to take it: the list, the market and the Voyage window show it.</summary>
        public static void Tip(string good, string port)
        {
            TipGood = good ?? "";
            TipPort = port ?? "";
            Version++;
        }

        /// <summary>The first order has been offered (once a game).</summary>
        public static bool OrderOffered { get; private set; }

        /// <summary>
        /// A STAKE WITH A DAY INSIDE THE FIRST HOUR (the alpha's row 17; 8 Oct 2026). On the first morning somebody down
        /// the river wants some of what came with the boat by a near day (trade_rules.json "start.first_order"): an
        /// order offered by name, which the game asks about when Ravn has had his say. True, with its parts, while
        /// there is such an order still to offer: the jobs stand, she lies somewhere else than where it is wanted,
        /// that port is there this year, and she has that many aboard.
        /// </summary>
        public static bool FirstOrder(out PortDef port, out GoodDef good, out int units, out double days, out double premium)
        {
            var r = TradeRules.Current;
            port = null;
            good = null;
            units = 0;
            days = premium = 0.0;
            if (!Active || OrderOffered || r == null || r.firstOrderUnits <= 0 || !Trade.Started)
                return false;
            port = GameData.Port(r.firstOrderPort);
            good = GameData.Good(r.firstOrderGood);
            units = r.firstOrderUnits;
            days = r.firstOrderDays;
            premium = r.firstOrderPremium;
            int year = KapmanGame.Current != null ? KapmanGame.Current.Year : 793;
            return port != null && good != null && days > 0.0 && port.id != Port && port.Exists(year) && Trade.Hold.Count(good.id) >= units;
        }

        /// <summary>Offers the first order (Ravn has done speaking): the game then asks, take it or refuse it. The order,
        /// or null where there was none to offer.</summary>
        public static Commission OfferFirstOrder()
        {
            if (!FirstOrder(out var port, out var good, out int units, out double days, out double premium))
                return null;
            var game = KapmanGame.Current;
            OrderOffered = true;
            Version++;
            return Commissions.Offer(port, good, units, days, premium, game != null ? game.Year : 793, game != null ? game.Month : 5, game != null ? game.Day : 0.0);
        }

        /// <summary>She has cast off: the list is done with (Landings sees it).</summary>
        public static void Sailed()
        {
            if (!Listed)
                return;
            Listed = false;
            Version++;
        }

        /// <summary>What she still wants before she can put out, as the parts of one sentence: "the sail", "three more
        /// men", and "a course" where none is set. Empty: she is ready.</summary>
        public static List<string> Wanting(bool courseSet)
        {
            var wants = new List<string>();
            if (Active && SailWorn)
                wants.Add(Mending ? "the sail back from the mending" : "the sail mended");
            int n = HandsShort;
            if (n > 0)
                wants.Add((n == 1 ? "one" : n == 2 ? "two" : n == 3 ? "three" : n.ToString()) + (n == 1 ? " more man" : " more men"));
            if (!courseSet)
                wants.Add("a course");
            return wants;
        }

        // ------------------------------------------------------------------ setting them

        /// <summary>The jobs are set: one man aboard, the sail worn, and the three of the crew waiting on the home quay.
        /// (`people` and `seed` chose made-up names until 8 Oct 2026; the men are the same in every game now.)</summary>
        public static void Begin(string port, string people, int seed)
        {
            Active = true;
            Listed = true;
            OrderOffered = false;
            TipGood = TipPort = "";
            Port = port ?? "";
            SailWorn = true;
            MendDoneAt = -1.0;
            hands.Clear();
            foreach (var (name, line) in Crew)
                hands.Add(new Hand { name = name, line = line });
            Voyage.SetMen(1);
            Landings.Clear();                                             // (a new game: no landing is kept from the one before)
            Version++;
        }

        // ------------------------------------------------------------------ doing them

        static ZoneDef Zone
        {
            get
            {
                var port = GameData.Port(Kapman.Game.Port.LyingId());
                return port != null ? TradeRules.Current.ZoneOf(port.region) : null;
            }
        }

        static string Show(double deniers)
        {
            var z = Zone;
            return z != null ? z.Show(deniers) : deniers.ToString("0.#", CultureInfo.InvariantCulture) + " d";
        }

        /// <summary>She lies at the port the jobs are done at.</summary>
        public static bool Here => Active && Kapman.Game.Port.LyingId() == Port;

        /// <summary>A hand is hired: his hand money is paid and he comes aboard. False, with why, when it cannot be.</summary>
        public static bool Hire(int index, out string fact)
        {
            fact = "";
            if (!Active || index < 0 || index >= hands.Count || hands[index].hired)
                return false;
            if (!Here)
            {
                fact = "The men are on the quay at home.";
                return false;
            }
            if (HandsShort <= 0)
            {
                fact = "She has her crew.";
                return false;
            }
            if (!Trade.Started || !Trade.Purse.Pay(HandMoney, Zone, TradeRules.Current, out _))
            {
                fact = $"You have not the {Show(HandMoney)} of hiring silver.";
                return false;
            }
            hands[index].hired = true;
            Voyage.Hire(1);
            fact = $"{hands[index].name} takes {Show(HandMoney)} of hiring silver and comes aboard.";
            Version++;
            Check();
            return true;
        }

        /// <summary>The sail is given to be mended: paid for now, done after some hours of the clock.</summary>
        public static bool Mend(double today, out string fact)
        {
            fact = "";
            if (!Active || !SailWorn || Mending)
                return false;
            if (!Here)
            {
                fact = "The sail is mended in port, at home.";
                return false;
            }
            if (!Trade.Started || !Trade.Purse.Pay(MendCost, Zone, TradeRules.Current, out _))
            {
                fact = $"You have not the {Show(MendCost)} for thread, cloth and the sailmaker's day.";
                return false;
            }
            MendDoneAt = today + MendHours / 24.0;
            fact = $"You pay {Show(MendCost)} and the sail goes ashore to be mended. It will be six hours.";
            Version++;
            return true;
        }

        /// <summary>The clock has moved: the mending is done when its hours are up. True when it was done just now.</summary>
        public static bool Tick(double today)
        {
            if (!Active || !Mending || today < MendDoneAt)
                return false;
            MendDoneAt = -1.0;
            SailWorn = false;
            Version++;
            Check();
            return true;
        }

        static void Check()
        {
            if (Active && !SailWorn && HandsShort <= 0)
                Active = false;                                           // (both done: nothing here is asked again)
        }

        /// <summary>She may not cast off yet, and why, as it is said aboard.</summary>
        public static bool Blocks(out string why)
        {
            why = "";
            if (!Active)
                return false;
            int n = HandsShort;
            if (n > 0 && SailWorn)
                why = $"Not yet. She wants {Count(n)} more to row her, and the sail must be mended first." + (Mending ? " It is being mended now." : "");
            else if (n > 0)
                why = $"Not yet. She wants {Count(n)} more to row her. They sit in the alehouse.";
            else if (Mending)
                why = "Not yet. The sail is ashore, being mended.";
            else if (SailWorn)
                why = "Not yet. The sail is rotten along the foot and must be mended first.";
            else
                return false;
            return true;
        }

        static string Count(int n) => n == 1 ? "one shipman" : n == 2 ? "two shipmen" : n == 3 ? "three shipmen" : n + " shipmen";

        // ------------------------------------------------------------------ the saved game

        static Dictionary<string, object> Capture()
        {
            var list = new List<object>();
            foreach (var h in hands)
                list.Add(new Dictionary<string, object> { ["name"] = h.name, ["line"] = h.line, ["hired"] = h.hired });
            return new Dictionary<string, object>
            {
                ["active"] = Active, ["port"] = Port, ["sail_worn"] = SailWorn, ["mend_done_at"] = MendDoneAt, ["hands"] = list,
                ["listed"] = Listed, ["tip_good"] = TipGood, ["tip_port"] = TipPort, ["order"] = OrderOffered,
            };
        }

        static void Restore(object saved)
        {
            Clear();
            if (!(saved is Dictionary<string, object> d))
                return;
            Active = Json.Bool(d, "active", false);
            Port = Json.Str(d, "port");
            SailWorn = Json.Bool(d, "sail_worn", false);
            MendDoneAt = Json.Num(d, "mend_done_at", -1.0);
            // (a save from before the list: it stands while the jobs do)
            Listed = Json.Bool(d, "listed", Active);
            TipGood = Json.Str(d, "tip_good");
            TipPort = Json.Str(d, "tip_port");
            // (a save from before the first order: not offered again in a game already under way)
            OrderOffered = Json.Bool(d, "order", true);
            foreach (var o in Json.Arr(d, "hands") ?? new List<object>())
                hands.Add(new Hand { name = Json.Str(o, "name"), line = Json.Str(o, "line"), hired = Json.Bool(o, "hired", false) });
            Version++;
        }

        static void Clear()
        {
            Active = false;
            Listed = false;
            OrderOffered = false;
            TipGood = TipPort = "";
            Port = "";
            SailWorn = false;
            MendDoneAt = -1.0;
            hands.Clear();
        }

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void OnPlay()
        {
            Clear();
            Version = 0;
            ContentSave.Register("first_jobs", Capture, Restore);
        }
    }
}
