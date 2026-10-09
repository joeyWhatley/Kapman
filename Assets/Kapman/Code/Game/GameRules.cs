using System;

namespace Kapman.Game
{
    /// <summary>
    /// THE RULES THE GRAPHS HOLD (KAPMAN, step 3; the user, 7 Oct 2026, by the picker: "Rules as graphs too"). A rule of
    /// the merchant's own dealings (the toll on what he sells, what a hand is paid...) is a visual scripting graph in
    /// Assets/Kapman/Game/Graphs/Rules: open it, read it left to right, change it. The game asks for the rule here by
    /// its name; the graphs' side (Kapman.Graphs.RuleGraphs) runs the graph and gives its answer back.
    ///
    /// Each rule keeps its sum in code too, beside the asking: it is what the WORLD'S OTHER SHIPS go by (they deal
    /// thousands of times a step: a graph is too slow for that), it is what the game falls back on where no graph
    /// holds the rule (a test without the graphs, a graph taken away), and a test runs the graph against it over
    /// thousands of cases, so the two say the same until the graph is changed on purpose. Change a rule's graph and
    /// the merchant's dealings change; the world's ships keep to the code's.
    /// </summary>
    public static class GameRules
    {
        /// <summary>Set by the graphs' side: runs the graph of the rule `name` with what is given and returns its answer;
        /// `ok` false where no graph holds the rule.</summary>
        public static Func<string, double[], (bool ok, double answer)> Graph;

        /// <summary>How many times a graph gave the answer, and how many times the code's stood (for a test, a readout).</summary>
        public static int ByGraph { get; private set; }
        public static int ByCode { get; private set; }

        static double Ask(string rule, double byCode, params double[] given)
        {
            var graph = Graph;
            if (graph != null)
            {
                try
                {
                    var (ok, answer) = graph(rule, given);
                    if (ok && !double.IsNaN(answer) && !double.IsInfinity(answer))
                    {
                        ByGraph++;
                        return answer;
                    }
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError("Kapman rules: the graph of the rule \"" + rule + "\" failed (" + e.Message + "): the code's sum is used.");
                }
            }
            ByCode++;
            return byCode;
        }

        // ------------------------------------------------------------------ the rules

        /// <summary>The toll as the code reckons it (every ship's): the share of what a sale fetched, to the hundredth
        /// of a denier; none where she is free of toll.</summary>
        public static double TollByCode(double soldFor, double share, double factor, bool free) =>
            free ? 0.0 : Math.Round(soldFor * share * factor, 2);

        /// <summary>THE TOLL ON A SALE OF THE MERCHANT'S (the graph "Rule - the toll on a sale"): `soldFor` deniers the
        /// market paid, `share` the zone's toll (0.1 is a tenth), `factor` what the toll is multiplied by here and now
        /// (a toll man bought off or angered: EncounterRunner.TollFactor), `free` where a charter frees her.</summary>
        public static double Toll(double soldFor, double share, double factor, bool free) =>
            Ask("toll", TollByCode(soldFor, share, factor, free), soldFor, share, factor, free ? 1.0 : 0.0);

        static double Between01(double x) => Math.Max(0.0, Math.Min(1.0, x));

        /// <summary>What the crew is owed for a day, as the code reckons it: so much a man.</summary>
        public static double DayPayByCode(int men, double payAMan) => men * payAMan;

        /// <summary>THE CREW'S PAY FOR A DAY (the graph "Rule - the crew's pay for a day"): deniers added to what is owed
        /// them, for `men` aboard at `payAMan` a day (voyage_rules.json: pay).</summary>
        public static double DayPay(int men, double payAMan) => Ask("pay", DayPayByCode(men, payAMan), men, payAMan);

        /// <summary>The crew's health after a day, as the code reckons it.</summary>
        public static double HealthByCode(double health, bool fed, bool watered, double hungerLoss, double thirstLoss, double gain)
        {
            if (!fed)
                health -= hungerLoss;
            if (!watered)
                health -= thirstLoss;
            if (fed && watered)
                health += gain;
            return Between01(health);
        }

        /// <summary>THE CREW'S HEALTH AFTER A DAY (the graph "Rule - the crew's health after a day"): 0 to 1. A day
        /// without food or without drink takes from it; a day with both gives some back (voyage_rules.json).</summary>
        public static double Health(double health, bool fed, bool watered, double hungerLoss, double thirstLoss, double gain) =>
            Ask("health", HealthByCode(health, fed, watered, hungerLoss, thirstLoss, gain),
                health, fed ? 0.0 : 1.0, watered ? 0.0 : 1.0, fed && watered ? 1.0 : 0.0, hungerLoss, thirstLoss, gain);

        /// <summary>The crew's heart after a day, as the code reckons it.</summary>
        public static double HeartByCode(double heart, double lostToTheSea, bool fed, bool watered, bool unpaid, bool inPort,
                                         double hungryLoss, double thirstyLoss, double unpaidLoss, double portGain)
        {
            heart -= lostToTheSea;
            if (!fed)
                heart -= hungryLoss;
            if (!watered)
                heart -= thirstyLoss;
            if (unpaid)
                heart -= unpaidLoss;
            if (inPort)
                heart += portGain;
            return Between01(heart);
        }

        /// <summary>THE CREW'S HEART AFTER A DAY (the graph "Rule - the crew's heart after a day"): 0 to 1. A rough sea
        /// takes from it (`lostToTheSea`, less as they get their sea legs), and hunger, thirst and pay owed for more than
        /// a week; a day in port gives some back (voyage_rules.json).</summary>
        public static double Heart(double heart, double lostToTheSea, bool fed, bool watered, bool unpaid, bool inPort,
                                   double hungryLoss, double thirstyLoss, double unpaidLoss, double portGain) =>
            Ask("heart", HeartByCode(heart, lostToTheSea, fed, watered, unpaid, inPort, hungryLoss, thirstyLoss, unpaidLoss, portGain),
                heart, lostToTheSea, fed ? 0.0 : 1.0, watered ? 0.0 : 1.0, unpaid ? 1.0 : 0.0, inPort ? 1.0 : 0.0, hungryLoss, thirstyLoss, unpaidLoss, portGain);

        // ------------------------------------------------------------------ a fight, and the lender's men (9 Oct 2026)

        /// <summary>How a fight stands, as the code reckons it.</summary>
        public static double FightByCode(double nerve, int herMen, int theirMen, bool weapons, double place, double perMan, double forWeapons) =>
            nerve + perMan * Math.Max(-3, Math.Min(3, herMen - theirMen)) + (weapons ? forWeapons : 0.0) + place;

        /// <summary>HOW A FIGHT STANDS (the graph "Rule - how a fight stands"), on her nerve's own scale: a card's chance
        /// adds a twentieth to its odds for each point of this over 3. Her `nerve`; `perMan` for each man she has
        /// more than they, or fewer (three either way at the most); `forWeapons` where there are weapons aboard; and
        /// what the `place` costs her (their own yard). feud_rules_793.json holds the numbers.</summary>
        public static double Fight(double nerve, int herMen, int theirMen, bool weapons, double place, double perMan, double forWeapons) =>
            Ask("fight", FightByCode(nerve, herMen, theirMen, weapons, place, perMan, forWeapons),
                nerve, herMen, theirMen, weapons ? 1.0 : 0.0, place, perMan, forWeapons);

        /// <summary>How easy a way round the lender's men is, as the code reckons it.</summary>
        public static double TrickEaseByCode(double stat, int usedBefore, int visit, double toldPenalty, double laterPenalty) =>
            stat - toldPenalty * Math.Max(0, usedBefore) - laterPenalty * Math.Max(0, visit - 1);

        /// <summary>HOW EASY A WAY ROUND THE LENDER'S MEN IS (the graph "Rule - a way round the lender's men"), on her
        /// stat's own scale: the `stat` that carries the trick, less `toldPenalty` for each time that way has fooled
        /// them before, less `laterPenalty` for each sending after the first. collectors_793.json holds the numbers.</summary>
        public static double TrickEase(double stat, int usedBefore, int visit, double toldPenalty, double laterPenalty) =>
            Ask("trick", TrickEaseByCode(stat, usedBefore, visit, toldPenalty, laterPenalty), stat, usedBefore, visit, toldPenalty, laterPenalty);

        /// <summary>What the lender's men come for, as the code reckons it.</summary>
        public static double InstalmentByCode(double temper, double instalment, double moreByTemper) =>
            Math.Round(instalment * (1.0 + moreByTemper * Math.Max(0.0, temper - 2.0)));

        /// <summary>WHAT THE LENDER'S MEN COME FOR (the graph "Rule - what the lender's men come for"), in deniers: an
        /// `instalment`, and `moreByTemper` of it again for each step of the lender's `temper` over 2 (a way round them
        /// that has just failed counts as a step). collectors_793.json holds the numbers.</summary>
        public static double Instalment(double temper, double instalment, double moreByTemper) =>
            Ask("instalment", InstalmentByCode(temper, instalment, moreByTemper), temper, instalment, moreByTemper);

        /// <summary>What the lender's men do when they are given nothing, as the code reckons it: 0 they only talk, 1
        /// they take what they came for, 2 they take and beat a man, 3 they take and kill a man, 4 they take and
        /// kill Ravn.</summary>
        public static double GoonsDoByCode(double temper, bool fooled, double aboard, int men, double throwOf, int unpaid, bool ravnAlive,
                                           double warnUpTo, double beatFrom, double killFrom, double killEarly, double ravnAt, double want)
        {
            double t = temper + (fooled ? 1.0 : 0.0);
            bool ravn = ravnAt >= 1.0 && unpaid >= ravnAt && ravnAlive && men > 1;
            if (t <= warnUpTo && !ravn)
                return 0.0;
            if (ravn)
                return 4.0;
            bool little = aboard < want * 0.5;
            if (men > 1 && (little || fooled))
                return t >= killFrom || (t >= killFrom - 1.0 && little && fooled && throwOf < killEarly) ? 3.0 : t >= beatFrom ? 2.0 : 1.0;
            return t >= beatFrom && (little || fooled) ? 2.0 : 1.0;
        }

        /// <summary>WHAT THE LENDER'S MEN DO WHEN THEY ARE GIVEN NOTHING (the graph "Rule - what the lender's men do"; the
        /// user, 9 Oct 2026: "Dependant", "It'll be dynamic you know"). The answer is one of five: 0 they only talk,
        /// 1 they take what they came for, 2 they take and beat a man, 3 they take and kill a man, 4 they take and
        /// kill Ravn. Given: the lender's `temper`; whether she has just tried a way round them and it `fooled`
        /// nobody; what there is `aboard` to take (silver and cargo, in deniers); her `men`; a throw of 0 to 1; how
        /// many visits have ended with nothing paid of her own will (`unpaid`); whether Ravn lives; and the ladder's
        /// own numbers from collectors_793.json (up to which they only talk, from which a man is beaten, from which
        /// one is killed, the chance of a killing a step early, at which count it is Ravn) with what they came for.</summary>
        public static double GoonsDo(double temper, bool fooled, double aboard, int men, double throwOf, int unpaid, bool ravnAlive,
                                     double warnUpTo, double beatFrom, double killFrom, double killEarly, double ravnAt, double want) =>
            Ask("goons", GoonsDoByCode(temper, fooled, aboard, men, throwOf, unpaid, ravnAlive, warnUpTo, beatFrom, killFrom, killEarly, ravnAt, want),
                temper, fooled ? 1.0 : 0.0, aboard, men, throwOf, unpaid, ravnAlive ? 1.0 : 0.0, warnUpTo, beatFrom, killFrom, killEarly, ravnAt, want);

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            ByGraph = 0;
            ByCode = 0;
        }
    }
}
