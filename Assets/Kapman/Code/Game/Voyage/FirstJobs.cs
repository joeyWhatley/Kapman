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
    ///     man). The hands on offer are a short list of names of her people, the same each time for the same game.
    ///   - THE SAIL: mended at the port for a little silver and some hours of the clock.
    /// Cast off is refused, in words, until both are done. Then the jobs are over and nothing here is asked again.
    /// The men hired are a count, as the crew is everywhere in the game today: their names are said and not kept.
    /// Pure rules with the game's purse and crew: the frame shows them (People here, Port, Tasks) and asks.
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

        /// <summary>What a hand on the quay is, in a line. Placeholders for the user's own words.</summary>
        static readonly string[] Lines =
        {
            "A fisher's son. A strong back, and no sea under him yet.",
            "An old river man. Knows every bank between here and the sea.",
            "Down from the uplands. Eats for two and rows for two.",
            "Rowed for a bigger boat and was put ashore. Does not say why.",
            "Young, quick, and sick in any swell. Says it passes.",
            "Has a knife, a cloak and no other trade.",
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

        // ------------------------------------------------------------------ setting them

        /// <summary>The jobs are set: one man aboard, the sail worn, and hands of her people waiting on the home quay.
        /// `seed` makes the same hands for the same game.</summary>
        public static void Begin(string port, string people, int seed)
        {
            Active = true;
            Port = port ?? "";
            SailWorn = true;
            MendDoneAt = -1.0;
            hands.Clear();
            var rng = new Random(seed * 7919 + 13);
            var used = new HashSet<string>(StringComparer.Ordinal) { "Ravn", "Hrafn", Self.Called };
            int line = rng.Next(Lines.Length);
            for (int i = 0; i < CrewWanted + 1; i++)
            {
                string name = "";
                for (int tries = 0; tries < 12 && (name.Length == 0 || used.Contains(name)); tries++)
                    name = PeriodNames.Pick(people, false, rng.NextDouble());
                if (name.Length == 0)
                    name = "A shipman";
                used.Add(name);
                hands.Add(new Hand { name = name, line = Lines[(line + i) % Lines.Length] });
            }
            Voyage.SetMen(1);
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
                why = $"Not yet. She wants {Count(n)} more to row her. Look under People here.";
            else if (Mending)
                why = "Not yet. The sail is ashore, being mended.";
            else if (SailWorn)
                why = "Not yet. The sail is rotten along the foot and must be mended first. Look under Port.";
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
            foreach (var o in Json.Arr(d, "hands") ?? new List<object>())
                hands.Add(new Hand { name = Json.Str(o, "name"), line = Json.Str(o, "line"), hired = Json.Bool(o, "hired", false) });
            Version++;
        }

        static void Clear()
        {
            Active = false;
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
