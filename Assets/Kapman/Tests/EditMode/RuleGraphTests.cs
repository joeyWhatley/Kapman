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
