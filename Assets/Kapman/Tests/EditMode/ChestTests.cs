using System.Collections.Generic;
using Kapman.Game;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>The chest (Plans/map_play.md): a riddle is answered by a thing; a wrong one costs a try; out of tries it
    /// rests until something new is learned that bears on it; a person's reward waits at their port; Latin needs learning
    /// or a reader; the chest and notes save and load.</summary>
    public class ChestTests
    {
        const string Texts = @"{ ""texts"": [
            { ""id"": ""t_norse"", ""title"": ""A Norse riddle"", ""form"": ""rune_stick"", ""language"": ""Old Norse"", ""script"": ""latin"",
              ""original"": ""Hvat er þat"", ""translation"": ""What is that"", ""translator"": ""X"", ""credit"": ""test"" },
            { ""id"": ""t_latin"", ""title"": ""A Latin riddle"", ""form"": ""parchment"", ""language"": ""Latin"", ""script"": ""latin"",
              ""original"": ""Quid est"", ""translation"": ""What is it"", ""translator"": ""Y"", ""credit"": ""test"" } ] }";

        const string Riddles = @"{ ""start"": ""r_easy"",
            ""palette"": [
              { ""id"": ""anchor"", ""kind"": ""thing"", ""name"": ""an anchor"", ""known"": ""start"" },
              { ""id"": ""swan"", ""kind"": ""animal"", ""name"": ""a swan"", ""known"": ""start"" },
              { ""id"": ""ox"", ""kind"": ""animal"", ""name"": ""an ox"", ""known"": ""start"" },
              { ""id"": ""wheat"", ""kind"": ""good"", ""name"": ""wheat"", ""gameId"": ""wheat"", ""known"": ""market"" } ],
            ""riddles"": [
              { ""id"": ""r_easy"", ""text"": ""t_norse"", ""answers"": [""anchor""], ""difficulty"": 1, ""tries"": 2,
                ""reward"": ""w_silver"", ""wakeOn"": [ { ""kind"": ""market"", ""id"": ""wheat"" } ] },
              { ""id"": ""r_latin"", ""text"": ""t_latin"", ""answers"": [""swan""], ""difficulty"": 3, ""reward"": ""w_tip"" } ],
            ""rewards"": [
              { ""id"": ""w_silver"", ""kind"": ""silver"", ""what"": ""a gift of silver"", ""where"": ""dorestad"", ""silver"": 30 },
              { ""id"": ""w_tip"", ""kind"": ""text"", ""what"": ""another text"", ""text"": ""t_norse"" } ] }";

        [SetUp]
        public void Load()
        {
            GameData.LoadGoods(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/goods_793.json"));
            GameData.LoadPorts(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/ports_793.json"));
            TradeRules.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/trade_rules.json"));
            ChestData.Load(new TextAsset(Texts), new TextAsset(Riddles), null);
            Chest.Clear();
            Known.Clear();
            Markets.Clear();
            Trade.Begin(TradeRules.Current, 900f, 0.0);
        }

        [TearDown]
        public void Done()
        {
            Chest.Clear();
            Known.Clear();
            Markets.Clear();
            Trade.Begin(TradeRules.Current, 900f, 0.0);
        }

        [Test]
        public void TheRightThingAnswersItAndItsRewardWaitsAtTheGiversPort()
        {
            Chest.Begin();
            var item = Chest.Items[0];
            Assert.AreEqual("r_easy", item.def, "the start riddle");
            double silver = Trade.Purse.Silver(TradeRules.Current);
            Chest.Answer(item, "anchor");
            Assert.IsTrue(item.solved);
            Assert.IsTrue(item.rewardDue, "a person's reward: bring the answer back");
            Assert.AreEqual(silver, Trade.Purse.Silver(TradeRules.Current), 1e-6, "not paid yet");
            Chest.AtPort(GameData.Port("dorestad"));
            Assert.IsFalse(item.rewardDue);
            Assert.Greater(Trade.Purse.Silver(TradeRules.Current), silver + 1.0, "paid at the giver's port");
        }

        [Test]
        public void OutOfTriesItRestsTillSomethingNewBearsOnIt()
        {
            Chest.Begin();
            var item = Chest.Items[0];
            Chest.Answer(item, "swan");
            Assert.AreEqual(1, item.tries);
            Chest.Answer(item, "swan");
            Assert.AreEqual(1, item.tries, "the same wrong answer twice costs nothing");
            Chest.Answer(item, "ox");
            Assert.IsTrue(item.resting, "out of tries: it rests");
            Chest.Answer(item, "anchor");
            Assert.IsFalse(item.solved, "resting: no answer counts");
            Chest.CheckWakes();
            Assert.IsTrue(item.resting, "nothing new learned yet");
            // wheat seen in a market: the riddle's wake
            Trade.Ledger.Note(Markets.Of(GameData.Port("dorestad"), 793, 0.0), 1.0, 6);
            Chest.CheckWakes();
            Assert.IsFalse(item.resting, "woken");
            Assert.AreEqual(2, item.tries, "its tries back");
            Chest.Answer(item, "anchor");
            Assert.IsTrue(item.solved);
        }

        [Test]
        public void LatinNeedsLearningOrAReader()
        {
            var latin = Chest.Give("riddle", "r_latin", "a monk", "dorestad", quiet: true);
            var norse = Chest.Give("text", "t_norse", "a trader", null, quiet: true);
            Assert.IsTrue(Chest.CanRead(norse), "their own tongue's neighbour: at once");
            Assert.IsFalse(Chest.CanRead(latin), "Latin: not yet");
            Assert.IsNotNull(Chest.WhyNotRead(latin));
            Chest.ReadToYou(latin);
            Assert.IsTrue(Chest.CanRead(latin), "read out by a monk");
            var another = ChestData.Text("t_latin");
            Assert.IsTrue(another.NeedsLearning);
            for (int i = 0; i < Chest.LessonsForLatin; i++)
                Chest.Lesson();
            Assert.IsTrue(Chest.KnowsLatin, "learned");
        }

        [Test]
        public void AKeyRewardComesAtOnce()
        {
            var item = Chest.Give("riddle", "r_latin", "a monk", "dorestad", quiet: true);
            Chest.Answer(item, "swan");
            Assert.IsTrue(item.solved);
            Assert.IsFalse(item.rewardDue);
            Assert.IsTrue(Chest.Has("t_norse"), "the text it won, at once");
        }

        [Test]
        public void TheChestAndNotesSaveAndLoad()
        {
            Chest.Begin();
            var item = Chest.Items[0];
            Chest.Answer(item, "swan");
            Chest.ToggleMark(item, 2);
            var note = Chest.AddNote("ships at anchor?", "riddle:r_easy");
            Chest.Pin(note, 123.0, -45.0);
            for (int i = 0; i < 3; i++)
                Chest.Lesson();
            string saved = Json.Write(Chest.Capture());
            Chest.Clear();
            Chest.Restore(Json.Parse(saved) as Dictionary<string, object>);
            var back = Chest.Items[0];
            Assert.AreEqual(1, back.tries);
            Assert.Contains("swan", back.tried);
            Assert.IsTrue(back.marks.Contains(2));
            Assert.AreEqual(3, Chest.LatinLessons);
            var n = Chest.Notes[0];
            Assert.AreEqual("ships at anchor?", n.text);
            Assert.AreEqual(123.0, n.x, 1e-9);
            Assert.Contains("riddle:r_easy", n.links);
            Assert.AreEqual(1, new List<PlayerNote>(Chest.NotesFor("riddle:r_easy")).Count);
        }

        [Test]
        public void SearchFindsNotesAndReadableTexts()
        {
            Chest.Give("text", "t_norse", "a trader", null, quiet: true);
            Chest.AddNote("the anchor holds the ship");
            var (notes, items) = Chest.Search("anchor");
            Assert.AreEqual(1, notes.Count);
            var (_, found) = Chest.Search("What is that");
            Assert.AreEqual(1, found.Count, "found in the English, which can be read");
        }
    }
}
