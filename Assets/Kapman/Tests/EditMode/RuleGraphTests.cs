using System;
using Kapman.Game;
using Kapman.Graphs;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// THE RULES THE GRAPHS HOLD (KAPMAN, step 3: "Rules as graphs too"). Each rule of the merchant's that is a graph
    /// (Assets/Kapman/Game/Graphs/Rules, named in Resources/GameRules) is run here, as an asset and with no scene,
    /// against the sum in code that every other ship goes by, over thousands of cases. They must say the same to the
    /// hundredth: if a graph is changed on purpose, its case here is changed with it.
    /// </summary>
    public class RuleGraphTests
    {
        [SetUp]
        public void Graphs() => RuleGraphs.Install();

        [TearDown]
        public void Code() => GameRules.Graph = null;                 // (the other tests of this run go by the code, as before)

        [Test]
        public void TheBookOfRulesHoldsTheToll()
        {
            Assert.IsNotNull(RuleGraphs.Book, "no Resources/GameRules (Kapman > Make physical > 3. The rules' graphs)");
            Assert.IsTrue(RuleGraphs.Has("toll"), "no graph of the book starts with \"Rule: the toll on a sale\"");
            Assert.IsFalse(RuleGraphs.Has("no such rule"));
        }

        [Test]
        public void TheGraphOfTheTollSaysWhatTheCodeSays()
        {
            var dice = new Random(793);
            double[] shares = { 0.0, 0.04, 0.1, 0.1, 0.1, 0.125, 0.25 };
            double[] factors = { 1.0, 1.0, 1.0, 0.5, 1.5, 0.0, 2.0 };
            for (int i = 0; i < 5000; i++)
            {
                double soldFor = Math.Round(dice.NextDouble() * (i % 7 == 0 ? 20000.0 : 600.0), 2);
                double share = shares[dice.Next(shares.Length)], factor = factors[dice.Next(factors.Length)];
                bool free = dice.Next(9) == 0;
                int asked = GameRules.ByGraph;
                double byGraph = GameRules.Toll(soldFor, share, factor, free);
                Assert.AreEqual(asked + 1, GameRules.ByGraph, "the toll was not reckoned by its graph (the code's sum stood in)");
                Assert.AreEqual(GameRules.TollByCode(soldFor, share, factor, free), byGraph, 0.0,
                    $"sold for {soldFor}, share {share}, times {factor}, free {free}: the graph and the code do not agree");
            }
            // and the plain case, by hand: a tenth of 46.90 is 4.69
            Assert.AreEqual(4.69, GameRules.Toll(46.9, 0.1, 1.0, false), 1e-9);
            Assert.AreEqual(0.0, GameRules.Toll(46.9, 0.1, 1.0, true), 0.0);
        }

        [Test]
        public void TheGraphsOfAFightAndOfTheLendersMenSayWhatTheCodeSays()
        {
            // (9 Oct 2026: the rules of that day's work were C# sums, and the user's ruling is that rules are graphs)
            foreach (string rule in new[] { "fight", "trick", "instalment", "goons" })
                Assert.IsTrue(RuleGraphs.Has(rule), "no graph of the book holds the rule \"" + rule + "\" (Kapman > Make physical > 3. The rules' graphs)");
            var dice = new Random(794);
            bool Coin(int oneIn) => dice.Next(oneIn) == 0;
            var seen = new int[5];
            for (int i = 0; i < 6000; i++)
            {
                int asked = GameRules.ByGraph;
                // how a fight stands
                double nerve = dice.Next(1, 6), place = Coin(2) ? 0.0 : -dice.Next(0, 4), perMan = Coin(4) ? dice.NextDouble() * 2.0 : 1.0, forWeapons = Coin(4) ? dice.NextDouble() * 2.0 : 1.0;
                int herMen = dice.Next(1, 14), theirMen = dice.Next(1, 8);
                bool weapons = Coin(3);
                Assert.AreEqual(GameRules.FightByCode(nerve, herMen, theirMen, weapons, place, perMan, forWeapons), GameRules.Fight(nerve, herMen, theirMen, weapons, place, perMan, forWeapons), 0.0,
                    $"a fight: nerve {nerve}, {herMen} men against {theirMen}, weapons {weapons}, place {place}");
                // a way round them
                double stat = dice.Next(1, 6), toldPenalty = Coin(4) ? dice.NextDouble() * 8.0 : 5.0, laterPenalty = Coin(4) ? dice.NextDouble() * 3.0 : 1.0;
                int usedBefore = dice.Next(0, 4), visit = dice.Next(0, 6);
                Assert.AreEqual(GameRules.TrickEaseByCode(stat, usedBefore, visit, toldPenalty, laterPenalty), GameRules.TrickEase(stat, usedBefore, visit, toldPenalty, laterPenalty), 0.0,
                    $"a way round them: stat {stat}, used {usedBefore} times, sending {visit}");
                // what they come for
                double temper = Coin(3) ? dice.Next(0, 8) : Math.Round(dice.NextDouble() * 7.0, 2), instalment = Coin(3) ? 240.0 : Math.Round(dice.NextDouble() * 600.0), moreByTemper = Coin(3) ? 0.5 : dice.NextDouble();
                double want = GameRules.Instalment(temper, instalment, moreByTemper);
                Assert.AreEqual(GameRules.InstalmentByCode(temper, instalment, moreByTemper), want, 0.0, $"what they come for: temper {temper}, an instalment of {instalment}, {moreByTemper} more a step");
                // what they do
                bool fooled = Coin(3), ravnAlive = !Coin(4);
                double aboard = Coin(3) ? 0.0 : Math.Round(dice.NextDouble() * 900.0), throwOf = dice.NextDouble();
                int men = dice.Next(1, 7), unpaid = dice.Next(0, 5);
                double warnUpTo = Coin(5) ? dice.Next(0, 3) : 1.0, beatFrom = Coin(5) ? dice.Next(1, 6) : 3.0, killFrom = Coin(5) ? dice.Next(2, 7) : 4.0, killEarly = Coin(5) ? dice.NextDouble() : 0.3, ravnAt = Coin(6) ? 0.0 : 3.0;
                double byCode = GameRules.GoonsDoByCode(temper, fooled, aboard, men, throwOf, unpaid, ravnAlive, warnUpTo, beatFrom, killFrom, killEarly, ravnAt, want);
                Assert.AreEqual(byCode, GameRules.GoonsDo(temper, fooled, aboard, men, throwOf, unpaid, ravnAlive, warnUpTo, beatFrom, killFrom, killEarly, ravnAt, want), 0.0,
                    $"what they do: temper {temper}, fooled {fooled}, {aboard} aboard, {men} men, throw {throwOf}, unpaid {unpaid}, Ravn alive {ravnAlive}, " +
                    $"talk up to {warnUpTo}, beat from {beatFrom}, kill from {killFrom}, early {killEarly}, Ravn at {ravnAt}, want {want}");
                seen[(int)byCode]++;
                Assert.AreEqual(asked + 4, GameRules.ByGraph, "one of the four was not reckoned by its graph (the code's sum stood in)");
            }
            // (every rung of the ladder was climbed in those cases: they talk, take, beat, kill, and kill Ravn)
            for (int does = 0; does < seen.Length; does++)
                Assert.Greater(seen[does], 20, "the cases never came to the answer " + does);
            // and the plain cases, by hand
            Assert.AreEqual(5.0, GameRules.Fight(3.0, 4, 2, false, 0.0, 1.0, 1.0), 1e-9, "nerve 3 and two men more is 5");
            Assert.AreEqual(-2.0, GameRules.TrickEase(3.0, 1, 1, 5.0, 1.0), 1e-9, "a way that fooled them once before is five harder");
            Assert.AreEqual(360.0, GameRules.Instalment(3.0, 240.0, 0.5), 1e-9, "at temper 3 they come for a pound and a half");
            Assert.AreEqual(4.0, GameRules.GoonsDo(1.0, false, 500.0, 4, 0.9, 3, true, 1.0, 3.0, 4.0, 0.3, 3.0, 240.0), 0.0, "the third time, it is Ravn");
            Assert.AreEqual(0.0, GameRules.GoonsDo(1.0, false, 500.0, 4, 0.9, 1, true, 1.0, 3.0, 4.0, 0.3, 3.0, 240.0), 0.0, "the first time, they only talk");
        }

        [Test]
        public void TheGraphsOfWindAndTideSayWhatTheCodeSays()
        {
            // (9 Oct 2026: the sea's rules were C# sums, and the user's ruling is that rules are graphs)
            string[] rules = { "leeway", "blown", "sea_runs", "landfall", "error_kept", "anchor", "lee_shore", "rough_race", "rough_bar", "rough_shoal", "rough_cape", "rough_slow" };
            foreach (string rule in rules)
                Assert.IsTrue(RuleGraphs.Has(rule), "no graph of the book holds the rule \"" + rule + "\" (Kapman > Make physical > 3. The rules' graphs)");
            var dice = new Random(795);
            bool Coin(int oneIn) => dice.Next(oneIn) == 0;
            double Up(double most) => dice.NextDouble() * most;
            int canAnchor = 0, strained = 0;
            for (int i = 0; i < 5000; i++)
            {
                int asked = GameRules.ByGraph;
                // her leeway: the table's own angles most of the time, any at all the rest
                double off = Coin(8) ? new[] { 0.0, 59.999, 60.0, 119.999, 120.0, 180.0 }[dice.Next(6)] : Up(180.0), wind = Coin(10) ? 0.0 : Up(30.0);
                double beating = Coin(3) ? Up(25.0) : 10.0, reaching = Coin(3) ? Up(15.0) : 6.0, running = Coin(3) ? Up(8.0) : 2.0, atWind = Coin(6) ? Up(0.2) : Coin(2) ? 8.0 : Up(20.0), most = Coin(3) ? Up(40.0) : 25.0;
                Assert.AreEqual(GameRules.LeewayByCode(off, wind, beating, reaching, running, atWind, most), GameRules.Leeway(off, wind, beating, reaching, running, atWind, most), 0.0,
                    $"leeway: the wind {off} off her bow at {wind} m/s; {beating}, {reaching}, {running} at {atWind}, {most} at most");
                // blown downwind, and the top of the sea
                bool lyingTo = Coin(2);
                double bare = Coin(3) ? Up(0.1) : 0.02, lying = Coin(3) ? Up(0.1) : 0.035, seaShare = Coin(3) ? Up(0.05) : 0.015;
                Assert.AreEqual(GameRules.BlownByCode(wind, lyingTo, bare, lying), GameRules.Blown(wind, lyingTo, bare, lying), 0.0, $"blown: {wind} m/s, lying to {lyingTo}");
                Assert.AreEqual(GameRules.SeaRunsByCode(wind, seaShare), GameRules.SeaRuns(wind, seaShare), 0.0, $"the sea runs: {wind} m/s at {seaShare}");
                // his landfall
                bool night = Coin(2), thick = Coin(3);
                double far = Coin(4) ? Up(60000.0) : 30000.0, nightShare = Coin(3) ? Up(1.0) : 0.4, air = Coin(6) ? Up(250.0) : Up(40000.0);
                Assert.AreEqual(GameRules.LandfallByCode(far, night, nightShare, thick, air), GameRules.Landfall(far, night, nightShare, thick, air), 0.0,
                    $"landfall: {far} m, night {night} ({nightShare}), thick {thick}, the air {air} m");
                // what he keeps of his error
                double hours = Coin(8) ? 0.0 : Up(3.0), eyeDay = Coin(4) ? Up(2.0) - 0.2 : 0.5, eyeNight = Coin(4) ? Up(1.0) - 0.1 : 0.1;
                Assert.AreEqual(GameRules.ErrorKeptByCode(hours, night, thick, eyeDay, eyeNight), GameRules.ErrorKept(hours, night, thick, eyeDay, eyeNight), 0.0,
                    $"error kept: {hours} h, night {night}, thick {thick}, {eyeDay} by day, {eyeNight} by night");
                // her anchor
                double gale = Coin(5) ? Up(30.0) : 17.2, depth = Coin(6) ? 12.0 : Up(40.0), anchorDepth = Coin(4) ? Up(30.0) : 12.0;
                bool can = GameRules.CanAnchor(wind, gale, depth, anchorDepth);
                Assert.AreEqual(GameRules.CanAnchorByCode(wind, gale, depth, anchorDepth), can, $"anchor: {wind} m/s (a gale from {gale}), {depth} m deep (to {anchorDepth})");
                canAnchor += can ? 1 : 0;
                // a lee shore
                double speed = Coin(5) ? 0.0 : Up(5.0), push = Up(2.5), clawOff = Coin(4) ? Up(3.0) : 1.2, strainHour = Coin(4) ? Up(0.05) : 0.01, strainGale = Coin(4) ? Up(0.1) : 0.03;
                bool blowsGale = Coin(3);
                double costs = GameRules.LeeShore(lyingTo, speed, push, clawOff, blowsGale, strainHour, strainGale, hours);
                Assert.AreEqual(GameRules.LeeShoreByCode(lyingTo, speed, push, clawOff, blowsGale, strainHour, strainGale, hours), costs, 0.0,
                    $"a lee shore: lying to {lyingTo}, her way {speed}, the push {push} (x{clawOff}), a gale {blowsGale}, {hours} h");
                strained += costs > 0.0 ? 1 : 0;
                // the rough waters
                double rough = Coin(4) ? Up(2.0) : 1.0, run = Coin(6) ? 0.0 : Coin(6) ? 1.0 : Up(1.0), against = Coin(4) ? 0.0 : Up(1.0), onshore = Coin(4) ? 0.6 : Up(1.0);
                Assert.AreEqual(GameRules.RoughRaceByCode(rough, run, wind, against), GameRules.RoughRace(rough, run, wind, against), 0.0, $"a race: {rough}, the stream {run}, {wind} m/s, against {against}");
                Assert.AreEqual(GameRules.RoughBarByCode(rough, run, wind, onshore), GameRules.RoughBar(rough, run, wind, onshore), 0.0, $"a bar: {rough}, the stream {run}, {wind} m/s, on to it {onshore}");
                Assert.AreEqual(GameRules.RoughShoalByCode(rough, run, wind), GameRules.RoughShoal(rough, run, wind), 0.0, $"a shoal: {rough}, the stream {run}, {wind} m/s");
                Assert.AreEqual(GameRules.RoughCapeByCode(rough, run, wind), GameRules.RoughCape(rough, run, wind), 0.0, $"a cape: {rough}, the stream {run}, {wind} m/s");
                double now = Coin(5) ? 0.0 : Up(1.6), kept = Coin(4) ? Up(1.0) : 0.6;
                Assert.AreEqual(GameRules.RoughSlowByCode(now, kept), GameRules.RoughSlow(now, kept), 0.0, $"her speed in rough water: {now} rough, {kept} kept");
                Assert.AreEqual(asked + rules.Length, GameRules.ByGraph, "one of the twelve was not reckoned by its graph (the code's sum stood in)");
            }
            Assert.Greater(canAnchor, 200, "the cases hardly ever let her anchor");
            Assert.Greater(5000 - canAnchor, 200, "the cases always let her anchor");
            Assert.Greater(strained, 200, "the cases hardly ever strain her on a lee shore");
            Assert.Greater(5000 - strained, 200, "the cases always strain her on a lee shore");
            // and the plain cases, by hand
            Assert.AreEqual(10.0, GameRules.Leeway(45.0, 8.0, 10.0, 6.0, 2.0, 8.0, 25.0), 1e-9, "beating in the table's own wind she makes the table's own leeway");
            Assert.AreEqual(3.0, GameRules.Leeway(170.0, 12.0, 10.0, 6.0, 2.0, 8.0, 25.0), 1e-9, "running in half as much wind again she makes half as much leeway again");
            Assert.AreEqual(25.0, GameRules.Leeway(10.0, 40.0, 20.0, 6.0, 2.0, 8.0, 25.0), 1e-9, "never more than the most");
            Assert.AreEqual(0.35, GameRules.Blown(10.0, true, 0.02, 0.035), 1e-9);
            Assert.AreEqual(12000.0, GameRules.Landfall(30000.0, true, 0.4, false, 0.0), 1e-6, "by night he sees four tenths as far");
            Assert.AreEqual(2000.0, GameRules.Landfall(30000.0, false, 0.4, true, 2000.0), 1e-6, "in thick weather no further than the air lets him");
            Assert.AreEqual(300.0, GameRules.Landfall(30000.0, false, 0.4, true, 50.0), 1e-6, "never under 300 m");
            Assert.AreEqual(1.0, GameRules.ErrorKept(1.0, false, true, 0.5, 0.1), 1e-12, "in thick fog the land tells him nothing");
            Assert.AreEqual(Math.Exp(-0.5), GameRules.ErrorKept(1.0, false, false, 0.5, 0.1), 1e-12);
            Assert.IsTrue(GameRules.CanAnchor(8.0, 17.2, 10.0, 12.0));
            Assert.IsFalse(GameRules.CanAnchor(8.0, 17.2, 20.0, 12.0), "she anchors in water too deep for it");
            Assert.IsFalse(GameRules.CanAnchor(20.0, 17.2, 10.0, 12.0), "she lies to her anchor in a gale");
            Assert.AreEqual(0.0, GameRules.LeeShore(false, 3.0, 1.0, 1.2, false, 0.01, 0.03, 1.0), 0.0, "with the way on to keep clear, a shore costs her nothing");
            Assert.AreEqual(0.03, GameRules.LeeShore(true, 3.0, 1.0, 1.2, true, 0.01, 0.03, 1.0), 1e-12, "lying to on a lee shore in a gale");
            Assert.AreEqual(0.0, GameRules.RoughRace(1.0, 0.0, 12.0, 1.0), 0.0, "a race is quiet at slack water");
            Assert.AreEqual(1.0, GameRules.RoughSlow(0.0, 0.6), 0.0);
            Assert.AreEqual(0.6, GameRules.RoughSlow(1.5, 0.6), 1e-12, "where it is very rough she keeps the table's share and no less");
        }

        [Test]
        public void TheGraphsOfTheCrewsDaySayWhatTheCodeSays()
        {
            foreach (string rule in new[] { "pay", "health", "heart" })
                Assert.IsTrue(RuleGraphs.Has(rule), "no graph of the book holds the rule \"" + rule + "\"");
            var dice = new Random(1066);
            bool Coin(int oneIn) => dice.Next(oneIn) == 0;
            for (int i = 0; i < 4000; i++)
            {
                int asked = GameRules.ByGraph;
                int men = dice.Next(1, 40);
                double pay = Math.Round(dice.NextDouble() * 2.0, 3);
                Assert.AreEqual(GameRules.DayPayByCode(men, pay), GameRules.DayPay(men, pay), 0.0, $"pay: {men} men at {pay}");

                double health = dice.NextDouble() * 1.2 - 0.1, hungerLoss = dice.NextDouble() * 0.2, thirstLoss = dice.NextDouble() * 0.3, gain = dice.NextDouble() * 0.1;
                bool fed = !Coin(3), watered = !Coin(4);
                Assert.AreEqual(GameRules.HealthByCode(health, fed, watered, hungerLoss, thirstLoss, gain), GameRules.Health(health, fed, watered, hungerLoss, thirstLoss, gain), 0.0,
                    $"health {health}, fed {fed}, watered {watered}");

                double heart = dice.NextDouble() * 1.2 - 0.1, sea = Coin(2) ? 0.0 : dice.NextDouble() * 0.2;
                bool unpaid = Coin(3), inPort = Coin(2);
                double a = dice.NextDouble() * 0.1, b = dice.NextDouble() * 0.1, c = dice.NextDouble() * 0.1, d = dice.NextDouble() * 0.1;
                Assert.AreEqual(GameRules.HeartByCode(heart, sea, fed, watered, unpaid, inPort, a, b, c, d), GameRules.Heart(heart, sea, fed, watered, unpaid, inPort, a, b, c, d), 0.0,
                    $"heart {heart}, sea {sea}, fed {fed}, watered {watered}, unpaid {unpaid}, in port {inPort}");
                Assert.AreEqual(asked + 3, GameRules.ByGraph, "a rule of the crew's day was not reckoned by its graph");
            }
        }

        [Test]
        public void WithNoGraphTheCodeStands()
        {
            GameRules.Graph = null;
            int byCode = GameRules.ByCode;
            Assert.AreEqual(4.69, GameRules.Toll(46.9, 0.1, 1.0, false), 1e-9);
            Assert.AreEqual(byCode + 1, GameRules.ByCode);
        }
    }
}
