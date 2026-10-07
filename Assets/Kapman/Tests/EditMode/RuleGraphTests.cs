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
