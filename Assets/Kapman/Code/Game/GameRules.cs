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

        // ------------------------------------------------------------------ wind and tide (9 Oct 2026)
        // The numbers that decide how much the sea moves her are graphs. What stays in code: the tide's stream itself
        // (an astronomical model), the adding up of the pushes as directions, and keeping her off the land.

        static double Share(double value, double from, double upTo) => Math.Max(0.0, Math.Min(1.0, (value - from) / Math.Max(1e-6, upTo - from)));

        /// <summary>Her leeway under sail, as the code reckons it.</summary>
        public static double LeewayByCode(double off, double wind, double beating, double reaching, double running, double atWind, double most)
        {
            double angle = off < 60.0 ? beating : off < 120.0 ? reaching : running;
            return Math.Min(most, angle * Math.Max(0.4, Math.Min(2.2, wind / Math.Max(0.1, atWind))));
        }

        /// <summary>HER LEEWAY UNDER SAIL (the graph "Rule - her leeway under sail"), degrees off her heading: the table's
        /// angle for the wind forward of 60 degrees off her bow, on the beam (to 120), or abaft that; more in more
        /// wind and less in less (between four tenths and 2.2 times, the angles being those at `atWind` m/s); never
        /// more than `most`. sea_push.json holds the numbers.</summary>
        public static double Leeway(double off, double wind, double beating, double reaching, double running, double atWind, double most) =>
            Ask("leeway", LeewayByCode(off, wind, beating, reaching, running, atWind, most), off, wind, beating, reaching, running, atWind, most);

        /// <summary>How fast she is blown downwind with no sail drawing, as the code reckons it.</summary>
        public static double BlownByCode(double wind, bool lyingTo, double bare, double lyingToShare) => wind * (lyingTo ? lyingToShare : bare);

        /// <summary>BLOWN DOWNWIND WITH NO SAIL (the graph "Rule - blown downwind with no sail"), m/s: a share of the
        /// wind's speed, one under oars or with the sail down, another lying to.</summary>
        public static double Blown(double wind, bool lyingTo, double bare, double lyingToShare) =>
            Ask("blown", BlownByCode(wind, lyingTo, bare, lyingToShare), wind, lyingTo ? 1.0 : 0.0, bare, lyingToShare);

        /// <summary>How fast the top of the sea runs, as the code reckons it.</summary>
        public static double SeaRunsByCode(double wind, double share) => wind * share;

        /// <summary>THE TOP OF THE SEA RUNS (the graph "Rule - the top of the sea runs"), m/s: a share of the wind's speed.</summary>
        public static double SeaRuns(double wind, double share) => Ask("sea_runs", SeaRunsByCode(wind, share), wind, share);

        /// <summary>How far the steersman makes out a landfall, as the code reckons it.</summary>
        public static double LandfallByCode(double far, bool night, double nightShare, bool thick, double air)
        {
            double m = far * (night ? nightShare : 1.0);
            if (thick)
                m = Math.Min(m, air);
            return Math.Max(300.0, m);
        }

        /// <summary>HOW FAR HE MAKES OUT A LANDFALL (the graph "Rule - how far he makes out a landfall"), real metres:
        /// `far` by day in clear air; a share of that by night; in thick weather no further than the `air` lets him
        /// see; never under 300 m.</summary>
        public static double Landfall(double far, bool night, double nightShare, bool thick, double air) =>
            Ask("landfall", LandfallByCode(far, night, nightShare, thick, air), far, night ? 1.0 : 0.0, nightShare, thick ? 1.0 : 0.0, air);

        /// <summary>What he keeps of his error along a coast, as the code reckons it.</summary>
        public static double ErrorKeptByCode(double hours, bool night, bool thick, double eyeDay, double eyeNight)
        {
            double eye = thick ? 0.0 : night ? eyeNight : eyeDay;
            return Math.Exp(-Math.Max(0.0, eye) * hours);
        }

        /// <summary>WHAT HE KEEPS OF HIS ERROR ALONG A COAST (the graph "Rule - what he keeps of his error along a
        /// coast"): the share of it he still has after so many hours, the land telling him `eyeDay` of it an hour by
        /// day, `eyeNight` by night, and nothing in thick fog.</summary>
        public static double ErrorKept(double hours, bool night, bool thick, double eyeDay, double eyeNight) =>
            Ask("error_kept", ErrorKeptByCode(hours, night, thick, eyeDay, eyeNight), hours, night ? 1.0 : 0.0, thick ? 1.0 : 0.0, eyeDay, eyeNight);

        /// <summary>Can she lie to her anchor, as the code reckons it.</summary>
        public static bool CanAnchorByCode(double wind, double gale, double depth, double anchorDepth) => wind <= gale && depth <= anchorDepth;

        /// <summary>CAN SHE LIE TO HER ANCHOR (the graph "Rule - can she lie to her anchor"): it does not blow a gale, and
        /// the water is no deeper than she can anchor in.</summary>
        public static bool CanAnchor(double wind, double gale, double depth, double anchorDepth) =>
            Ask("anchor", CanAnchorByCode(wind, gale, depth, anchorDepth) ? 1.0 : 0.0, wind, gale, depth, anchorDepth) >= 0.5;

        // ------------------------------------------------------------------ fewer prompts (the user's rulings of 10 Oct 2026)

        /// <summary>Does the steersman lay her course again himself, as the code reckons it.</summary>
        /// <summary>DOES A PRESS ON THE CHART TAKE HER LINE (13 Oct 2026: the course pulled with the mouse)? As the code
        /// has it: when the pointer is within `holdPx` pixels of the line.</summary>
        public static bool PressTakesLineByCode(double distancePx, double holdPx) => distancePx <= holdPx;

        /// <summary>The same, by the graph "Rule - does the press take the line" where it is there.</summary>
        public static bool PressTakesLine(double distancePx, double holdPx) =>
            Ask("press_takes_line", PressTakesLineByCode(distancePx, holdPx) ? 1.0 : 0.0, distancePx, holdPx) >= 0.5;

        /// <summary>HOW FAST SHE ANSWERS THE HELM under your hand (13 Oct 2026): degrees a second (real seconds at speed 1).
        /// Given her class's rate of turn, her way as a share of her full speed (0..1: with little way on her the rudder
        /// bites less) and the helm over (-1..1). As the code has it: the share kept between 0 and 1, times 0.7, plus
        /// 0.3, times the rate, times the helm.</summary>
        public static double HelmAnswerByCode(double turnRate, double wayShare, double rudder) =>
            ((Math.Max(0.0, Math.Min(1.0, wayShare)) * 0.7) + 0.3) * turnRate * rudder;

        /// <summary>The same, by the graph "Rule - how fast she answers the helm" where it is there.</summary>
        public static double HelmAnswer(double turnRate, double wayShare, double rudder) =>
            Ask("helm_answer", HelmAnswerByCode(turnRate, wayShare, rudder), turnRate, wayShare, rudder);

        /// <summary>THE HELM GIVEN BACK (13 Oct 2026): does the steersman lay her course again from where she is? As the
        /// code has it: when a course is set and the air is not thick.</summary>
        public static bool LaysAgainOnGiveBackByCode(bool courseSet, bool thick) => courseSet && !thick;

        /// <summary>The same, by the graph "Rule - does the steersman lay her course again when the helm is given back".</summary>
        public static bool LaysAgainOnGiveBack(bool courseSet, bool thick) =>
            Ask("gives_back_laid", LaysAgainOnGiveBackByCode(courseSet, thick) ? 1.0 : 0.0, courseSet ? 1.0 : 0.0, thick ? 1.0 : 0.0) >= 0.5;

        public static bool LaysAgainHimselfByCode(bool landInSight, bool thick) => landInSight && !thick;

        /// <summary>DOES THE STEERSMAN LAY HER COURSE AGAIN HIMSELF (the graph "Rule - does the steersman lay her course
        /// again himself"; the user by the picker, 10 Oct 2026: "He corrects along a coast ... Out of sight of land it
        /// stays as now"). Set well off her line: with the land in sight, and air he can see it through, he lays her
        /// course again from where she is and the clock runs on. Out of sight of land, or in thick air, he tells her
        /// and it is hers to lay.</summary>
        public static bool LaysAgainHimself(bool landInSight, bool thick) =>
            Ask("lays_again", LaysAgainHimselfByCode(landInSight, thick) ? 1.0 : 0.0, landInSight ? 1.0 : 0.0, thick ? 1.0 : 0.0) >= 0.5;

        /// <summary>Does she sail on when the air clears, as the code reckons it.</summary>
        public static bool SailsOnWhenClearByCode(bool lostInThickAir, double sees, double clearFrom) => lostInThickAir && sees >= clearFrom;

        /// <summary>DOES SHE SAIL ON WHEN THE AIR CLEARS (the graph "Rule - does she sail on when the air clears"; the
        /// user by the picker, 10 Oct 2026: "Sail on by herself"). Lying to with a reckoning that was lost in thick air:
        /// when he makes out a landfall at `clearFrom` metres or more he lays her course from where she is. A reckoning
        /// lost in clear air waits for her.</summary>
        public static bool SailsOnWhenClear(bool lostInThickAir, double sees, double clearFrom) =>
            Ask("sails_on", SailsOnWhenClearByCode(lostInThickAir, sees, clearFrom) ? 1.0 : 0.0, lostInThickAir ? 1.0 : 0.0, sees, clearFrom) >= 0.5;

        /// <summary>Is a card answered as she answered it the last time, as the code reckons it.</summary>
        public static bool AsBeforeByCode(bool stands, bool better, bool meets) => stands && !better && !meets;

        /// <summary>IS A CARD ANSWERED AS BEFORE (the graph "Rule - is a card answered as before"; the user by the
        /// picker, 10 Oct 2026, of the king's toll: "As you answered last time"). `stands`: its people remember an
        /// answer of hers that may stand again (the card's rule names those), and it can be given today. `better`: an
        /// answer the rule ranks above that one is open to her today (a charter she did not hold then). `meets`: a
        /// choice is open that brings them to her in person (something to hide).</summary>
        public static bool AsBefore(bool stands, bool better, bool meets) =>
            Ask("as_before", AsBeforeByCode(stands, better, meets) ? 1.0 : 0.0, stands ? 1.0 : 0.0, better ? 1.0 : 0.0, meets ? 1.0 : 0.0) >= 0.5;

        /// <summary>What a lee shore costs her hull in a step, as the code reckons it.</summary>
        public static double LeeShoreByCode(bool lyingTo, double speed, double push, double clawOff, bool gale, double strainHour, double strainGale, double hours) =>
            lyingTo || speed < push * clawOff ? (gale ? strainGale : strainHour) * hours : 0.0;

        /// <summary>WHAT A LEE SHORE COSTS HER HULL (the graph "Rule - what a lee shore costs her hull"): the share of
        /// the hull lost in this step of so many hours, when she lies to or has not `clawOff` times the push in her
        /// own way; more in a gale; nothing when she keeps clear.</summary>
        public static double LeeShore(bool lyingTo, double speed, double push, double clawOff, bool gale, double strainHour, double strainGale, double hours) =>
            Ask("lee_shore", LeeShoreByCode(lyingTo, speed, push, clawOff, gale, strainHour, strainGale, hours),
                lyingTo ? 1.0 : 0.0, speed, push, clawOff, gale ? 1.0 : 0.0, strainHour, strainGale, hours);

        /// <summary>How rough a tide race is now, as the code reckons it.</summary>
        public static double RoughRaceByCode(double rough, double run, double wind, double against)
        {
            double f = 0.35 + 0.65 * against * Share(wind, 2.0, 10.0) + 0.2 * Share(wind, 8.0, 16.0);
            return rough * run * Math.Min(1.2, f);
        }

        /// <summary>HOW ROUGH A TIDE RACE IS NOW (the graph "Rule - how rough a tide race is"): its own roughness, by how
        /// hard the stream runs (nothing at slack water), by a third of itself at the least and more with the wind
        /// against the stream (from 2 m/s, all of it at 10) and in a hard wind (from 8 to 16), a fifth over at the most.</summary>
        public static double RoughRace(double rough, double run, double wind, double against) =>
            Ask("rough_race", RoughRaceByCode(rough, run, wind, against), rough, run, wind, against);

        /// <summary>How rough a river's bar is now, as the code reckons it.</summary>
        public static double RoughBarByCode(double rough, double run, double wind, double onshore) =>
            rough * Share(wind, 3.0, 12.0) * (0.25 + 0.75 * onshore) * (0.6 + 0.4 * run);

        /// <summary>HOW ROUGH A RIVER'S BAR IS NOW (the graph "Rule - how rough a river's bar is"): its own roughness, by
        /// the wind (from 3 m/s, all of it at 12), by how squarely the wind blows on to it, and worse while the tide runs.</summary>
        public static double RoughBar(double rough, double run, double wind, double onshore) =>
            Ask("rough_bar", RoughBarByCode(rough, run, wind, onshore), rough, run, wind, onshore);

        /// <summary>How rough a shoal is now, as the code reckons it.</summary>
        public static double RoughShoalByCode(double rough, double run, double wind) => rough * Share(wind, 4.0, 13.0) * (0.7 + 0.3 * run);

        /// <summary>HOW ROUGH A SHOAL IS NOW (the graph "Rule - how rough a shoal is"): its own roughness, by the wind (from
        /// 4 m/s, all of it at 13), a little worse while the tide runs.</summary>
        public static double RoughShoal(double rough, double run, double wind) => Ask("rough_shoal", RoughShoalByCode(rough, run, wind), rough, run, wind);

        /// <summary>How rough an open cape is now, as the code reckons it.</summary>
        public static double RoughCapeByCode(double rough, double run, double wind) => rough * Share(wind, 5.0, 14.0) * (0.6 + 0.4 * run);

        /// <summary>HOW ROUGH AN OPEN CAPE IS NOW (the graph "Rule - how rough an open cape is"): its own roughness, by the
        /// wind (from 5 m/s, all of it at 14), worse while the tide runs.</summary>
        public static double RoughCape(double rough, double run, double wind) => Ask("rough_cape", RoughCapeByCode(rough, run, wind), rough, run, wind);

        /// <summary>The share of her speed she keeps in rough water, as the code reckons it.</summary>
        public static double RoughSlowByCode(double rough, double kept) => 1.0 - (1.0 - kept) * Math.Min(1.0, rough);

        /// <summary>HER SPEED IN ROUGH WATER (the graph "Rule - her speed in rough water"): the share of it she keeps, all
        /// of it in quiet water and `kept` of it where the roughness is 1 or more.</summary>
        public static double RoughSlow(double rough, double kept) => Ask("rough_slow", RoughSlowByCode(rough, kept), rough, kept);

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            ByGraph = 0;
            ByCode = 0;
        }
    }
}
