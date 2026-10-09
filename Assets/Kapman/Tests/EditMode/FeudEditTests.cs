using System.Collections.Generic;
using System.Linq;
using Kapman.Encounters;
using Kapman.Game;
using Kapman.Game.Sim;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>
    /// RAVN'S DEATH AND THE FEUD (the user, 9 Oct 2026: "Ravn needs an impactful death scene as genuine punishment for not
    /// repaying the debt but opens up a bloodfued opportunity"; by the picker: the feud is "You against the lender", it
    /// ends in "Blood or silver", and "All of it, live"). Held here, without the screen:
    ///   - the table is whole and holds only names the code reads;
    ///   - a fight is carried by her nerve, her men against theirs, and weapons aboard;
    ///   - the scene is spoken (the table stages it, the writing words it, one line breaks off) and the seven cards are
    ///     written, and order nothing the code does not know;
    ///   - his death is punished: a man fewer, the crew's heart struck and held under a ceiling, a mark on her, until it
    ///     is answered; she may say nothing, and that runs out; she may swear late;
    ///   - blood: the one who did it killed, and it is answered; at law: his price comes off the stick; the lender
    ///     killed: nothing is owed, his cards come no more, and she is an outlaw until his price is paid;
    ///   - with the rule off he dies in a sentence, as he did;
    ///   - the cards: over him the oath and then where he lies; the fire; the one who did it, a card of his own before
    ///     the rope men's; the lender's door, the count's court, the count's men.
    /// The scene itself on the screen, and all of it as a player has it, is held by FeudTests (play).
    /// </summary>
    public class FeudEditTests
    {
        const string Data = "Assets/Kapman/Game/Data/";
        const string Ravn = "cast.crew.ravn";
        const string Quay = "harbour.goons.quay", Sea = "sea.goons.alongside";
        static readonly string[] Cards = { Feud.OathCard, Feud.BurialCard, Feud.PyreCard, Feud.KillerCard, Feud.DoorCard, Feud.CourtCard, Feud.OutlawCard };
        static readonly string[] Laws =
        {
            "feud_swear", "feud_law", "feud_quiet", "feud_bury_priest", "feud_bury_aboard", "feud_bury_water", "feud_pyre", "feud_blood", "feud_blood_lost",
            "feud_lender", "feud_lender_lost", "feud_price", "feud_outlaw_paid", "feud_outlaw_seized",
        };

        static TextAsset File(string name) => AssetDatabase.LoadAssetAtPath<TextAsset>(Data + name);

        EncounterState flags;

        [SetUp]
        public void Load()
        {
            GameData.LoadGoods(File("goods_793.json"));
            GameData.LoadPorts(File("ports_793.json"));
            TradeRules.Load(File("trade_rules.json"));
            Wording.Load(File("wording_793.json"));
            Reputation.Load(File("reputation_793.json"));
            Markets.Clear();
            Collectors.Reset();
            Feud.Reset();
            Self.Reset();
            Self.SetStat("nerve", 3.0);
            Self.SetStat("name", 3.0);
            flags = new EncounterState();
            ContentFlags.Use(flags, () => 0.0);
        }

        [TearDown]
        public void Done()
        {
            Collectors.Reset();
            Feud.Reset();
            Self.Reset();
            ContentFlags.Use(null);
            Markets.Clear();
        }

        static WorldXZ Home()
        {
            var home = GameData.Port(Collectors.Rules.home);
            var (x, z) = Kapman.World.WorldFrame.ToWorld(home.lat, home.lon);
            return new WorldXZ(x, z);
        }

        /// <summary>A game with the lender in it, four men aboard, and the rope men at her berth for the third time she
        /// gives them nothing.</summary>
        void TheThirdTime()
        {
            Trade.Begin(TradeRules.Current, 900f, 0.0);
            Voyage.SetMen(4);
            Voyage.Rest(0.0, 1.0);
            flags.SetFlag(Collectors.OpeningFlag, 1.0, 0.0, 0.0);
            Collectors.Refused(2);
            Collectors.PutOut(3, Home(), 2.0, Collectors.Rules.home);
        }

        [Test]
        public void TheTableIsWholeAndHoldsOnlyNamesTheCodeReads()
        {
            var text = File(Feud.File);
            Assert.IsNotNull(text, Feud.File + " is not in Game/Data");
            var root = Json.Parse(text.text) as Dictionary<string, object>;
            Assert.IsNotNull(root);
            var fields = typeof(FeudRules).GetFields().Where(f => !f.IsStatic).Select(f => f.Name).ToList();
            foreach (string key in root.Keys)
                if (key != "about")
                    CollectionAssert.Contains(fields, key, Feud.File + " names a number the code does not read: " + key);
            foreach (string field in fields)
                Assert.IsTrue(root.ContainsKey(field), Feud.File + " leaves out " + field);
            var r = Feud.Rules;
            Assert.IsTrue(r.on);
            Assert.Less(r.deathHeart, 0.0);
            // (the punishment is that they are never "in good heart" while he lies unanswered, and can still be "steady")
            Assert.Less(r.heartCeiling, 0.75);
            Assert.GreaterOrEqual(r.heartCeiling, 0.5);
            Assert.IsTrue(Self.IsStat(r.markStat), "the mark lies on no stat of hers: " + r.markStat);
            Assert.Less(r.markSize, 0.0);
            Assert.Greater(r.priceD, 0.0);
            Assert.Greater(r.outlawFineD, r.priceD, "to kill the lender is cheaper than his price for Ravn");
            Assert.Greater(r.quietDays, 30.0);
            Assert.Greater(r.shoreKm, 0.5);
        }

        [Test]
        public void HisFireNeedsAShore()
        {
            double km = Feud.Rules.shoreKm;
            if (!Course.KnowsWater)
            {
                // (no world to ask: there is always a shore, and the rule is held on the chart by FeudTests)
                Assert.IsTrue(Feud.ShoreNear(default, km));
                return;
            }
            // sixty km out in the open sea there is none
            var (sx, sz) = Kapman.World.WorldFrame.ToWorld(52.5, 3.0);
            Assert.IsTrue(Course.IsWater(sx, sz), "52.5 N 3 E is land");
            Assert.IsFalse(Feud.ShoreNear(new WorldXZ(sx, sz), km), "sixty km out there is a shore within " + km + " km");
            // the first land due east on that latitude: two km off it there is one, and well outside the reach there is not
            double x = sx;
            while (Course.IsWater(x, sz) && x < sx + 200000.0)
                x += 250.0;
            Assert.IsFalse(Course.IsWater(x, sz), "no land due east of the open sea at 52.5 N");
            Assert.IsTrue(Feud.ShoreNear(new WorldXZ(x - 2000.0, sz), km), "two km off the land there is no shore within " + km + " km");
            Assert.IsFalse(Feud.ShoreNear(new WorldXZ(x - km * 1000.0 - 6000.0, sz), km), "six km beyond its reach the shore is still near");
        }

        [Test]
        public void AFightIsCarriedByNerveMenAndWeapons()
        {
            var r = new FeudRules();
            Assert.AreEqual(3.0, Feud.Fight(r, 3.0, 2, 2, false, 0.0), 1e-9, "man for man, it is her nerve");
            Assert.AreEqual(5.0, Feud.Fight(r, 3.0, 4, 2, false, 0.0), 1e-9, "two men more are two points");
            Assert.AreEqual(2.0, Feud.Fight(r, 3.0, 2, 3, false, 0.0), 1e-9, "a man fewer is a point lost");
            Assert.AreEqual(6.0, Feud.Fight(r, 3.0, 12, 2, false, 0.0), 1e-9, "more than three men over count as three");
            Assert.AreEqual(0.0, Feud.Fight(r, 3.0, 1, 9, false, 0.0), 1e-9, "more than three men under count as three");
            Assert.AreEqual(4.0, Feud.Fight(r, 3.0, 2, 2, true, 0.0), 1e-9, "weapons aboard are a point");
            Assert.AreEqual(2.0, Feud.Fight(r, 3.0, 2, 2, false, r.fightDoor), 1e-9, "their own yard is against her");
        }

        [Test]
        public void TheSceneAndTheCardsAreWrittenAParagraphAtATime()
        {
            var book = EncounterData.Book();
            // THE SCENE IS SPOKEN, and staged in the table: who says each line, and the one line that breaks off
            var staged = Feud.Rules.scene;
            Assert.GreaterOrEqual(staged.Count, 6, "the scene of his death is a line or two");
            Assert.LessOrEqual(staged.Count, 16, "the scene of his death is a wall of text");
            var spoken = new List<string>();
            foreach (var beat in staged)
            {
                string words = book.Written(beat.key);
                Assert.IsNotNull(words, "a line of the scene has no words: " + beat.key);
                spoken.Add(words);
                if (beat.who.Length > 0)
                    Assert.IsTrue(Feud.Rules.sceneCast.ContainsKey(beat.who), "a line of the scene is said by nobody the table knows: " + beat.who);
                // (Pokémon pacing: a box holds a sentence or two, and no line of the scene is more than three boxes)
                Assert.LessOrEqual(Kapman.Game.Frame.TextReveal.Pages(words, Kapman.Game.Frame.TextReveal.PageChars).Count, 3, "a line of the scene is a speech: " + words);
            }
            Assert.GreaterOrEqual(staged.Count(x => x.who.Length > 0), staged.Count / 2, "the scene is told, not spoken");
            foreach (string who in new[] { "ravn", "aldger", "brute", "man" })
            {
                Assert.IsTrue(staged.Any(x => x.who == who), who + " says nothing in the scene");
                var person = Feud.Rules.sceneCast[who];
                Assert.IsNotEmpty(person.name);
                Assert.IsTrue(person.cast.Length > 0 && person.role.Length > 0, who + " is nobody of the cast");
            }
            // the last thing Ravn says breaks off, in the middle of a word
            var cuts = staged.Where(x => x.cut).ToList();
            Assert.AreEqual(1, cuts.Count, "one line of the scene breaks off, and only one");
            Assert.AreEqual("ravn", cuts[0].who, "it is Ravn who is cut off");
            Assert.AreEqual(cuts[0], staged.Last(x => x.who == "ravn"), "Ravn speaks again after he is cut off");
            string broken = book.Written(cuts[0].key);
            Assert.IsTrue(char.IsLetter(broken[broken.Length - 1]), "the line that breaks off ends tidily: " + broken);
            // what cuts him off is the one hit of the scene: red, and the screen shaken, as the next line comes up
            var hits = staged.Where(x => x.hit).ToList();
            Assert.AreEqual(1, hits.Count, "the scene has one hit, and only one");
            Assert.AreEqual(staged.IndexOf(cuts[0]) + 1, staged.IndexOf(hits[0]), "the hit does not follow the line it cuts off");
            StringAssert.Contains("knife", book.Written(hits[0].key), "the hit is not the line that says what was done to him");
            // (the two halves of what his sheet says he feared: he cannot speak, and the cross is made over him)
            StringAssert.Contains("Ravn", string.Join(" ", spoken));
            StringAssert.Contains("cross", string.Join(" ", spoken));
            Assert.IsNotNull(book.Written("mark." + Feud.Mark), "the mark that lies on her has no words");
            Assert.IsNotNull(book.Written("mark." + Feud.Mark + ".until"));

            foreach (string id in Cards.Concat(new[] { Quay, Sea }))
            {
                var c = book.Card(id);
                Assert.IsNotNull(c, "the card " + id + " is not in the book");
                Assert.IsNotNull(book.Written(c.titleKey), id + " has no title written");
                Assert.IsNotNull(book.Written(c.intentionKey), id + " has no scene written");
                CollectionAssert.DoesNotContain(c.leans, Ravn, id + " leans on Ravn, who is dead when it comes");
                foreach (var l in c.lines)
                {
                    string said = book.Written(l.key);
                    Assert.IsNotNull(said, id + " has a line with no words: " + l.key);
                    Assert.LessOrEqual(said.Length, 240, id + ": a line is more than a paragraph: " + said);
                }
                foreach (var o in c.options)
                {
                    Assert.IsNotNull(book.Written(o.labelKey), id + " " + o.key + " has no label written");
                    if (o.Chance == null)
                        Assert.IsNotEmpty(Outcomes.Text(book, o, ""), id + " " + o.key + " has no written end");
                    else
                    {
                        Assert.IsNotEmpty(Outcomes.Text(book, o, "success"), id + " " + o.key + " has no end for the chance won");
                        Assert.IsNotEmpty(Outcomes.Text(book, o, "failure"), id + " " + o.key + " has no end for the chance lost");
                    }
                    foreach (var v in o.effects.SelectMany(e => e.Verbs()))
                    {
                        if (v.verb == "law" && v.a.StartsWith("feud_"))
                            CollectionAssert.Contains(Laws, v.a, id + " " + o.key + " orders what the feud does not know");
                        if (v.verb == "mark")
                            Assert.IsNotNull(book.Written("mark." + v.b), id + " " + o.key + " puts a mark with no words: " + v.b);
                    }
                }
            }
            // nobody new is named: the one who speaks over him is the spine's steersman, Ravn's friend
            foreach (string id in new[] { Feud.OathCard, Feud.BurialCard, Feud.PyreCard })
                Assert.AreEqual("cast.crew.aldger", book.Card(id).actors.Single().cast, id);
            Assert.AreEqual(book.Card(Quay).actors.First(x => x.key == "brute").cast, book.Card(Feud.KillerCard).actors.Single().cast, "the one who did it is another man than the rope man");
            // every order the feud knows does something or nothing, and never throws, with no game running
            foreach (string law in Laws)
                Assert.DoesNotThrow(() => Feud.Law(law), law);
        }

        [Test]
        public void HisDeathIsPunishedAndLiesOnHerUntilItIsAnswered()
        {
            TheThirdTime();
            var r = Feud.Rules;
            Assert.AreEqual(1.0, Feud.HeartCeiling, 1e-9, "their heart has a ceiling while he lives");
            Assert.Greater(Voyage.Morale, 0.9);
            Assert.AreEqual(FeudStage.None, Feud.Stage);
            Assert.AreEqual(3, Self.StatNow(r.markStat));

            string said = Collectors.Law("goons_teeth");
            // the card's end only turns to him: the scene tells it
            StringAssert.Contains("looks at Ravn", said);
            StringAssert.DoesNotContain("knife", said, "the card tells his death, and the scene tells it again");
            Assert.IsTrue(Collectors.RavnGone);
            Assert.AreEqual(3, Voyage.Men);
            Assert.AreEqual(FeudStage.Scene, Feud.Stage);
            Assert.IsTrue(Feud.Unanswered);
            Assert.IsTrue(Feud.KillerAlive);
            Assert.IsTrue(Feud.AtSea, "with no game running she lies at no port: he died at sea");
            // the punishment: their heart struck down and held down, and a mark on her
            Assert.AreEqual(r.heartCeiling, Feud.HeartCeiling, 1e-9);
            Assert.LessOrEqual(Voyage.Morale, r.heartCeiling + 1e-9, "their heart is over its ceiling");
            Voyage.Rest(0.0, 1.0);
            Voyage.Affect(0.0, 1.0);
            Assert.LessOrEqual(Voyage.Morale, r.heartCeiling + 1e-9, "a rest ashore lifts their heart over its ceiling");
            Assert.IsTrue(Self.HasMark(Feud.Mark), "no mark lies on her");
            Assert.AreEqual(2, Self.StatNow(r.markStat), "the mark takes nothing from her " + r.markStat);
            StringAssert.Contains("killed him for your debt", Feud.Words());
            // (with no game running there is no writing: the scene falls back to its last line)
            CollectionAssert.AreEqual(new[] { "Ravn is dead." }, Feud.SceneLines().Select(l => l.text).ToArray());
            StringAssert.StartsWith("Killed at sea", Feud.Plate());

            // over him: nothing can be done about it until the scene is over
            Assert.AreEqual(0.0, Feud.Var("feud.oath"));
            Assert.AreEqual(0.0, Feud.Var("feud.open"));
            Assert.AreEqual(0.0, Feud.Var("feud.said"));
            Feud.PastScene();
            Assert.AreEqual(FeudStage.Oath, Feud.Stage);
            Assert.AreEqual(1.0, Feud.Var("feud.oath"));
            Assert.AreEqual(1.0, Feud.Var("feud.killer"));

            // she says nothing: it is open, it lies on the boat, and it runs out with its days
            Feud.Law("feud_quiet");
            Assert.AreEqual(FeudStage.Open, Feud.Stage);
            Assert.AreEqual(1.0, Feud.Var("feud.open"));
            Assert.AreEqual(1.0, Feud.Var("feud.said"));
            Assert.AreNotEqual(0.0, flags.Flag(Feud.Quiet, r.quietDays - 1.0));
            Assert.AreEqual(0.0, flags.Flag(Feud.Quiet, r.quietDays + 1.0), "saying nothing lies on the boat for ever");
            Assert.IsFalse(Feud.IsSworn);
            Assert.IsTrue(Feud.CanSwear, "she said nothing over him, and may never swear");
            StringAssert.Contains("said nothing", Feud.Words());

            // she swears it late (the Errands window)
            StringAssert.Contains("swear", Feud.Swear());
            Assert.IsTrue(Feud.IsSworn);
            Assert.IsFalse(Feud.CanSwear);
            Assert.AreEqual(0.0, flags.Flag(Feud.Quiet, 1.0));
            Assert.LessOrEqual(Voyage.Morale, r.heartCeiling + 1e-9);

            // where he lies: carried aboard, then his fire
            Assert.AreEqual(0.0, Feud.Var("feud.buried"));
            Feud.Law("feud_bury_aboard");
            Assert.IsTrue(Feud.LiesAboard);
            Assert.AreEqual(0.0, Feud.Var("feud.buried"));
            StringAssert.Contains("aboard", Feud.Words());
            Feud.Law("feud_pyre");
            Assert.IsFalse(Feud.LiesAboard);
            Assert.AreEqual(2.0, Feud.Var("feud.buried"));

            // blood: the rope men come again, and she kills the one who did it
            Collectors.PutOut(4, Home(), 3.0, Collectors.Rules.home);
            double silver = Trade.Purse.Silver(TradeRules.Current);
            said = Feud.Law("feud_blood");
            StringAssert.Contains("answered", said);
            Assert.AreEqual(silver, Trade.Purse.Silver(TradeRules.Current), 0.001, "with no port under her there is no lord to fine it");
            Assert.AreEqual(FeudStage.Settled, Feud.Stage);
            Assert.AreEqual(1, Feud.Ended);
            Assert.IsFalse(Feud.KillerAlive);
            Assert.IsFalse(Feud.Unanswered);
            Assert.AreEqual(0.0, Feud.Var("feud.open"));
            Assert.AreEqual(1.0, Feud.Var("feud.said"), "it is over, and where he lies can no longer be asked");
            Assert.AreEqual(0.0, Feud.Var("feud.killer"));
            Assert.AreEqual(1.0, Feud.HeartCeiling, 1e-9, "it is answered, and their heart is held down still");
            Assert.IsFalse(Self.HasMark(Feud.Mark), "it is answered, and the mark lies on her still");
            Assert.AreEqual(3, Self.StatNow(r.markStat));
            Voyage.Rest(0.0, 1.0);
            Assert.Greater(Voyage.Morale, 0.9);
            // and the rest of them go home to say so
            Collectors.Law("goons_routed");
            Assert.AreEqual(GoonState.Home, Collectors.State);
            Assert.AreEqual(4.0, Collectors.Temper, 1e-9, "he has lost a man and is no angrier");
            // it cannot be sworn again, or said nothing over, once it is over
            Feud.Law("feud_swear");
            Feud.Law("feud_quiet");
            Assert.AreEqual(FeudStage.Settled, Feud.Stage);
            Assert.AreEqual(0.0, flags.Flag(Feud.Sworn, 0.0));
        }

        [Test]
        public void AtLawHisPriceComesOffTheStick()
        {
            TheThirdTime();
            var r = Feud.Rules;
            Collectors.Law("goons_teeth");
            Feud.PastScene();
            Feud.Law("feud_law");
            Assert.IsTrue(Feud.IsAtLaw);
            Assert.IsTrue(Feud.CanSwear, "she took it to the count, and may not turn to blood");
            // a saved game keeps where the matter stands
            var saved = ContentSave.Capture();
            Assert.IsTrue(ContentSave.Registered("feud"), "the feud is in no saved game");
            Feud.Forget();
            Assert.AreEqual(FeudStage.None, Feud.Stage);
            ContentSave.Restore(saved);
            Assert.AreEqual(FeudStage.Open, Feud.Stage, "a loaded game has forgotten that Ravn is dead");
            Assert.IsTrue(Feud.IsAtLaw);
            Assert.IsTrue(Feud.AtSea);
            Assert.AreEqual(r.heartCeiling, Feud.HeartCeiling, 1e-9);
            double owed = Trade.Debt;
            Assert.Greater(owed, r.priceD);
            string said = Feud.Law("feud_price");
            Assert.AreEqual(owed - r.priceD, Trade.Debt, 0.5, said);
            StringAssert.Contains("off your stick", said);
            Assert.AreEqual(2, Feud.Ended);
            Assert.AreEqual(FeudStage.Settled, Feud.Stage);
            Assert.AreEqual(1.0, Feud.HeartCeiling, 1e-9);
            Assert.IsFalse(Self.HasMark(Feud.Mark));
            Assert.IsTrue(Feud.KillerAlive, "silver was taken for him, and the one who did it is dead of it");
            Assert.IsFalse(Feud.CanSwear, "the price was taken, and she may still swear his blood");
        }

        [Test]
        public void TheLenderKilledNothingIsOwedAndSheIsAnOutlaw()
        {
            TheThirdTime();
            var r = Feud.Rules;
            var rules = TradeRules.Current;
            Collectors.Law("goons_teeth");
            Feud.PastScene();
            Feud.Law("feud_swear");
            var book = EncounterData.Book();
            string[] his = { "harbour.wokerers_day.home", "harbour.wokerers_stick.in_time", "harbour.wokerers_stick.late" };
            foreach (string id in his)
                Assert.IsTrue(flags.Ready(book.Card(id), 0.0), id + " cannot come while the lender lives");

            // she goes in to him and is thrown out: he is past patience
            Feud.Law("feud_lender_lost");
            Assert.AreEqual(Collectors.Rules.temperMost, Collectors.Temper, 1e-9);
            Assert.IsFalse(Feud.LenderDead);
            Assert.IsTrue(Feud.IsSworn);

            // she goes in to him, and he is dead
            flags.SetFlag(Trade.WorkFlag, 1.0, 0.0, 0.0);
            Assert.Greater(Trade.Debt, 100.0);
            string said = Feud.Law("feud_lender");
            StringAssert.Contains("nothing is owed", said);
            StringAssert.Contains("outlaw", said);
            Assert.AreEqual(0.0, Trade.Debt, 1e-9);
            Assert.IsTrue(Feud.LenderDead);
            Assert.IsTrue(Feud.IsOutlaw);
            Assert.AreEqual(3, Feud.Ended);
            Assert.AreEqual(1.0, Feud.HeartCeiling, 1e-9);
            Assert.IsFalse(Self.HasMark(Feud.Mark));
            Assert.AreEqual(0.0, flags.Flag(Trade.WorkFlag, 0.0), "he is dead, and she works for him still");
            StringAssert.Contains("outlaw", Feud.Words());
            // no card of his can come again: a dead man does not hand her the stick
            foreach (string id in his)
                Assert.IsFalse(flags.Ready(book.Card(id), 0.0), id + " can still come, and the lender is dead");

            // an outlaw in the Franks' ports: the hold is theirs, until his price is paid to the count
            var wine = GameData.Good("rhenish_wine");
            Trade.Hold.Add(wine, 4, 0.0, 0.0);
            said = Feud.Law("feud_outlaw_seized");
            Assert.AreEqual(0, Trade.Hold.Count(wine.id), said);
            StringAssert.Contains("take the cargo", said);
            StringAssert.Contains("find nothing", Feud.Law("feud_outlaw_seized"));
            double silver = Trade.Purse.Silver(rules);
            Assert.Less(silver, r.outlawFineD);
            StringAssert.Contains("outlaw still", Feud.Law("feud_outlaw_paid"));
            Assert.AreEqual(silver, Trade.Purse.Silver(rules), 0.001, "short of his price, and silver was taken all the same");
            Assert.IsTrue(Feud.IsOutlaw);
            Trade.Purse.Add("denier", r.outlawFineD);
            silver = Trade.Purse.Silver(rules);
            said = Feud.Law("feud_outlaw_paid");
            Assert.IsFalse(Feud.IsOutlaw, said);
            Assert.AreEqual(silver - r.outlawFineD, Trade.Purse.Silver(rules), 6.0, said);
        }

        [Test]
        public void WithTheRuleOffHeDiesInASentence()
        {
            Feud.Use(new FeudRules { on = false });
            TheThirdTime();
            string said = Collectors.Law("goons_teeth");
            StringAssert.Contains("puts his knife in Ravn", said);
            Assert.IsTrue(Collectors.RavnGone);
            Assert.AreEqual(3, Voyage.Men);
            Assert.AreEqual(FeudStage.None, Feud.Stage);
            Assert.AreEqual(1.0, Feud.HeartCeiling, 1e-9);
            Assert.IsFalse(Self.HasMark(Feud.Mark));
            Assert.AreEqual("", Feud.Words());
        }

        // ------------------------------------------------------------------ the cards

        /// <summary>A director over the feud's cards and the rope men's, with the fake game and the numbers set by hand.</summary>
        static (EncounterDirector d, FakeGame game) Director()
        {
            var game = new FakeGame();
            var d = new EncounterDirector(EncounterData.BookOf(Cards.Concat(new[] { Quay, Sea }).ToArray()), 793, game);
            d.portInfo = port => port == "dorestad" || port == "traiectum" ? ("franks", "frisian") : ("danes", "danish");
            d.roll = () => 0.0;
            d.vars.nums["silver"] = 300.0;
            d.vars.nums["debt"] = 1920.0;
            d.vars.nums["debt_short"] = 1620.0;
            d.state.SetFlag(Trade.LenderFlag, 1.0, 0.0, 0.0);
            d.SetTime(150.0, 793, 9, 150 * 24);
            return (d, game);
        }

        static List<string> Call(EncounterDirector d, string port) => d.BeginCall(port).Select(c => c.id).ToList();

        static void Leave(EncounterDirector d)
        {
            for (int i = 0; i < 12; i++)
            {
                var s = d.OpenNext();
                if (s == null)
                    break;
                var r = d.ChooseDefault();
                if (r == null || !r.ok)
                    d.Choose(s.options.FindIndex(o => o.enabled));
                d.Continue();
            }
            d.EndCall();
        }

        static bool Can(EncounterSession s, string key) => s.options[s.card.Option(key).index].enabled;
        static List<string> LawsOf(FakeGame game) => game.laws.Select(l => l.kind).ToList();

        [Test]
        public void OverHimSheSaysWhatIsToBeDoneAndThenWhereHeLies()
        {
            var (d, game) = Director();
            // while he lives, and before his scene is over, nothing of it can come
            foreach (string id in Cards)
                Assert.IsFalse(d.CanOffer(id), id + " can come while Ravn lives");
            d.state.SetFlag(EncounterState.GoneFlag(Ravn), 1.0, 0.0, 0.0);
            Assert.IsFalse(d.CanOffer(Feud.OathCard), "the oath's card can come before his scene is over");
            d.vars.nums["feud.oath"] = 1.0;
            Assert.IsTrue(d.Offer(Feud.OathCard), "his scene is over, and she cannot stand over him");
            var s = d.OpenNext();
            Assert.AreEqual(Feud.OathCard, s.card.id);
            StringAssert.Contains("Ten years", string.Join(" ", s.lines.Select(l => l.text)), "his friend of ten years says nothing over him");
            Assert.IsTrue(Can(s, "swear") && Can(s, "law") && Can(s, "quiet"));
            Assert.IsTrue(s.card.Option("quiet").isDefault, "left alone, she swears or goes to law");
            var r = d.Choose(s.card.Option("swear").index);
            Assert.IsTrue(r.ok, r.Text);
            CollectionAssert.Contains(LawsOf(game), "feud_swear");
            Assert.IsNotEmpty(r.written);
            Assert.AreEqual(Feud.BurialCard, r.then, "where he lies does not follow the oath");
            d.Continue();

            // where he lies, at sea: no priest; his friend names the fire and the sea
            d.vars.nums["feud.oath"] = 0.0;
            d.vars.nums["feud.open"] = 1.0;
            d.vars.nums["feud.said"] = 1.0;
            s = d.OpenNext();
            Assert.IsNotNull(s, "where he lies did not follow");
            Assert.AreEqual(Feud.BurialCard, s.card.id);
            string said = string.Join(" ", s.lines.Select(l => l.text));
            StringAssert.Contains("a shore with wood", said);
            StringAssert.DoesNotContain("priest", said, "there is a priest at sea");
            Assert.IsFalse(Can(s, "priest"), "the priest's ground can be had at sea");
            Assert.IsTrue(Can(s, "aboard") && Can(s, "water"));
            game.laws.Clear();
            r = d.Choose(s.card.Option("aboard").index);
            CollectionAssert.Contains(LawsOf(game), "feud_bury_aboard");
            d.Continue();
            // carried aboard, the question is not asked again; and the fire can come, at sea
            d.state.SetFlag(Feud.Aboard, 1.0, 0.0, 0.0);
            Assert.IsFalse(d.CanOffer(Feud.BurialCard), "he lies aboard, and where he lies is asked again");
            d.vars.nums["feud.aboard_days"] = 1.0;
            Assert.IsTrue(d.Offer(Feud.PyreCard), "he lies aboard at sea, and no shore can be looked for");
            s = d.OpenNext();
            Assert.AreEqual(Feud.PyreCard, s.card.id);
            Assert.IsTrue(Can(s, "burn") && Can(s, "water") && Can(s, "on"));
            game.laws.Clear();
            r = d.Choose(s.card.Option("on").index);
            CollectionAssert.IsEmpty(LawsOf(game), "to sail on with him orders something");
            d.Continue();
            // four days aboard, and he can be carried no further
            d.SetTime(155.0, 793, 9, 155 * 24);
            d.vars.nums["feud.aboard_days"] = 5.0;
            Assert.IsTrue(d.Offer(Feud.PyreCard));
            s = d.OpenNext();
            Assert.IsFalse(Can(s, "on"), "he can lie aboard for ever");
            game.laws.Clear();
            r = d.Choose(s.card.Option("burn").index);
            CollectionAssert.Contains(LawsOf(game), "feud_pyre");
            Assert.IsNotEmpty(r.written);
            d.Continue();

            // where he lies, at a quay: the priest would take him, for a penny and a candle
            d.state.SetFlag(Feud.Aboard, 0.0, 0.0, 0.0);
            Call(d, "dorestad");
            Assert.IsTrue(d.Offer(Feud.BurialCard));
            s = d.OpenNext();
            Assert.AreEqual(Feud.BurialCard, s.card.id);
            StringAssert.Contains("priest", string.Join(" ", s.lines.Select(l => l.text)));
            Assert.IsTrue(Can(s, "priest"));
            game.laws.Clear();
            game.calls.Clear();
            r = d.Choose(s.card.Option("priest").index);
            CollectionAssert.Contains(LawsOf(game), "feud_bury_priest");
            Assert.IsTrue(game.calls.Any(c => c.StartsWith("silver -4")), "the priest buries him for nothing: " + string.Join("; ", game.calls));
            d.Continue();
            // buried, it is not asked again
            d.vars.nums["feud.buried"] = 1.0;
            Assert.IsFalse(d.CanOffer(Feud.BurialCard));
        }

        [Test]
        public void TheOneWhoDidItHasACardOfHisOwnBeforeTheirs()
        {
            var book = EncounterData.Book();
            // a card holds nine choices at most, and a choice that cannot be taken is still shown: so nothing of Ravn
            // stands on the rope men's own cards while he lives
            foreach (string id in new[] { Quay, Sea })
            {
                var theirs = book.Card(id);
                Assert.LessOrEqual(theirs.options.Count, 9);
                foreach (var o in theirs.options)
                    StringAssert.DoesNotContain("Ravn", book.Written(o.labelKey), id + " shows a choice about Ravn's death while he lives");
            }
            var (d, game) = Director();
            d.vars.nums["goons.caught"] = 1.0;
            Call(d, "dorestad");
            // he lives: the card of the one who did it cannot come; theirs says nothing of him
            Assert.IsFalse(d.CanOffer(Feud.KillerCard), "the one who did it can be killed for Ravn while Ravn lives");
            Assert.IsTrue(d.Offer(Quay));
            var s = d.OpenNext();
            StringAssert.DoesNotContain("sorry about the Dane", string.Join(" ", s.lines.Select(l => l.text)));
            d.Choose(s.card.Option("pound").index);
            d.Continue();
            Leave(d);

            // he is dead, it is open, and the one who did it stands on her quay: his card goes before theirs
            d.state.SetFlag(EncounterState.GoneFlag(Ravn), 1.0, 0.0, 0.0);
            d.vars.nums["feud.open"] = 1.0;
            d.vars.nums["feud.killer"] = 1.0;
            d.vars.nums["feud.fight"] = 3.0;
            d.SetTime(160.0, 793, 9, 160 * 24);
            d.roll = () => 0.0;
            Call(d, "dorestad");
            Assert.IsTrue(d.Offer(Quay));
            Assert.IsTrue(d.Offer(Feud.KillerCard), "he is dead, the one who did it is here, and she cannot go for him");
            s = d.OpenNext();
            Assert.AreEqual(Feud.KillerCard, s.card.id, "their own card came before the one who did it");
            StringAssert.Contains("I remember", string.Join(" ", s.lines.Select(l => l.text)));
            Assert.IsTrue(Can(s, "kill") && Can(s, "wait"));
            Assert.IsTrue(s.card.Option("wait").isDefault, "left alone, a man is killed");
            game.laws.Clear();
            var r = d.Choose(s.card.Option("kill").index);
            Assert.AreEqual("success", r.branch);
            CollectionAssert.AreEqual(new[] { "feud_blood", "goons_routed" }, LawsOf(game));
            Assert.IsNotEmpty(r.written);
            d.Continue();
            Leave(d);
            // the fight lost: a man of hers, and then they do what they do when they are angry
            d.SetTime(161.0, 793, 9, 161 * 24);
            d.roll = () => 0.99;
            Call(d, "dorestad");
            Assert.IsTrue(d.Offer(Feud.KillerCard));
            s = d.OpenNext();
            game.laws.Clear();
            r = d.Choose(s.card.Option("kill").index);
            Assert.AreEqual("failure", r.branch);
            CollectionAssert.AreEqual(new[] { "feud_blood_lost", "goons_teeth_angry" }, LawsOf(game));
            d.Continue();
            Leave(d);
            // not now: nothing is ordered, and their own card follows and speaks of him
            d.SetTime(162.0, 793, 9, 162 * 24);
            Call(d, "dorestad");
            Assert.IsTrue(d.Offer(Quay));
            Assert.IsTrue(d.Offer(Feud.KillerCard));
            s = d.OpenNext();
            game.laws.Clear();
            r = d.Choose(s.card.Option("wait").index);
            CollectionAssert.IsEmpty(LawsOf(game), "to let him stand orders something");
            d.Continue();
            s = d.OpenNext();
            Assert.AreEqual(Quay, s.card.id, "their own card did not follow");
            StringAssert.Contains("sorry about the Dane", string.Join(" ", s.lines.Select(l => l.text)), "the rope man says nothing of Ravn when he has killed him");
            d.Choose(s.card.Option("nothing").index);
            d.Continue();
            Leave(d);
            // at sea the same card can come
            d.vars.nums["goons.sea"] = 1.0;
            d.SetTime(163.0, 793, 9, 163 * 24);
            Assert.IsTrue(d.CanOffer(Feud.KillerCard), "alongside at sea, the one who did it cannot be gone for");
            // the one who did it dead, or the matter closed: it cannot come again, and theirs is silent on him
            d.vars.nums["feud.killer"] = 0.0;
            Assert.IsFalse(d.CanOffer(Feud.KillerCard), "he can be killed twice");
            d.vars.nums["feud.killer"] = 1.0;
            d.vars.nums["feud.open"] = 0.0;
            Assert.IsFalse(d.CanOffer(Feud.KillerCard), "his price was taken, and he can still be killed for it");
            d.vars.nums["feud.killer"] = 0.0;
            Assert.IsTrue(d.Offer(Sea));
            s = d.OpenNext();
            StringAssert.DoesNotContain("sorry about the Dane", string.Join(" ", s.lines.Select(l => l.text)));
        }

        [Test]
        public void AtDorestatTheDoorAndTheCourtAndInTheFranksPortsTheCountsMen()
        {
            var (d, game) = Director();
            d.state.SetFlag(EncounterState.GoneFlag(Ravn), 1.0, 0.0, 0.0);
            d.vars.nums["feud.open"] = 1.0;
            d.vars.nums["feud.killer"] = 1.0;
            // nothing sworn, nothing at law, no outlaw: none of the three comes
            var came = Call(d, "dorestad");
            CollectionAssert.DoesNotContain(came, Feud.DoorCard);
            CollectionAssert.DoesNotContain(came, Feud.CourtCard);
            CollectionAssert.DoesNotContain(came, Feud.OutlawCard);
            Leave(d);

            // sworn: the lender's door, at Dorestat and nowhere else
            d.state.SetFlag(Feud.Sworn, 1.0, 0.0, 0.0);
            d.SetTime(151.0, 793, 9, 151 * 24);
            CollectionAssert.DoesNotContain(Call(d, "traiectum"), Feud.DoorCard, "the lender's door stands at Traiectum");
            Leave(d);
            came = Call(d, "dorestad");
            CollectionAssert.Contains(came, Feud.DoorCard, "she has sworn it and made fast at Dorestat, and his door is not there");
            CollectionAssert.DoesNotContain(came, Feud.CourtCard);
            var s = d.OpenNext();
            Assert.AreEqual(Feud.DoorCard, s.card.id);
            StringAssert.Contains("God keep you", string.Join(" ", s.lines.Select(l => l.text)), "Bubo is not at the door");
            Assert.IsTrue(Can(s, "killer") && Can(s, "lender") && Can(s, "leave"));
            Assert.IsTrue(s.card.Option("leave").isDefault, "left alone, somebody is killed");
            game.laws.Clear();
            var r = d.Choose(s.card.Option("lender").index);
            Assert.AreEqual("success", r.branch);
            CollectionAssert.AreEqual(new[] { "feud_lender" }, LawsOf(game));
            Assert.IsNotEmpty(r.written);
            d.Continue();
            Leave(d);
            // it does not stand there again the same week
            d.SetTime(153.0, 793, 9, 153 * 24);
            CollectionAssert.DoesNotContain(Call(d, "dorestad"), Feud.DoorCard, "his door is asked at every call");
            Leave(d);
            // a boat of them is out: the one who did it is not in the yard; and the fight at his table lost
            d.SetTime(160.0, 793, 9, 160 * 24);
            d.vars.nums["goons.out"] = 1.0;
            d.roll = () => 0.99;
            CollectionAssert.Contains(Call(d, "dorestad"), Feud.DoorCard);
            s = d.OpenNext();
            Assert.IsFalse(Can(s, "killer"), "he is out in their boat, and in the yard as well");
            game.laws.Clear();
            r = d.Choose(s.card.Option("lender").index);
            Assert.AreEqual("failure", r.branch);
            CollectionAssert.Contains(LawsOf(game), "feud_lender_lost");
            d.Continue();
            Leave(d);
            // the one who did it called out
            d.SetTime(170.0, 793, 9, 170 * 24);
            d.vars.nums["goons.out"] = 0.0;
            d.roll = () => 0.0;
            CollectionAssert.Contains(Call(d, "dorestad"), Feud.DoorCard);
            s = d.OpenNext();
            game.laws.Clear();
            r = d.Choose(s.card.Option("killer").index);
            CollectionAssert.AreEqual(new[] { "feud_blood" }, LawsOf(game));
            d.Continue();
            Leave(d);

            // at law, not sworn: the count's court
            d.state.SetFlag(Feud.Sworn, 0.0, 0.0, 0.0);
            d.state.SetFlag(Feud.AtLaw, 1.0, 0.0, 0.0);
            d.SetTime(180.0, 793, 10, 180 * 24);
            came = Call(d, "dorestad");
            CollectionAssert.Contains(came, Feud.CourtCard, "she said the count should have it, and the count does not hear her");
            CollectionAssert.DoesNotContain(came, Feud.DoorCard);
            s = d.OpenNext();
            Assert.AreEqual(Feud.CourtCard, s.card.id);
            Assert.IsTrue(Can(s, "price") && Can(s, "blood") && Can(s, "wait"));
            game.laws.Clear();
            r = d.Choose(s.card.Option("price").index);
            Assert.AreEqual("success", r.branch);
            CollectionAssert.AreEqual(new[] { "feud_price" }, LawsOf(game));
            d.Continue();
            Leave(d);
            d.SetTime(195.0, 793, 10, 195 * 24);
            Call(d, "dorestad");
            s = d.OpenNext();
            game.laws.Clear();
            r = d.Choose(s.card.Option("blood").index);
            CollectionAssert.AreEqual(new[] { "feud_swear" }, LawsOf(game), "she leaves the hall for blood, and nothing is sworn");
            d.Continue();
            Leave(d);

            // an outlaw: the count's men at every port of the Franks, and at none of the Danes
            d.state.SetFlag(Feud.AtLaw, 0.0, 0.0, 0.0);
            d.vars.nums["feud.open"] = 0.0;
            d.state.SetFlag(Feud.Outlaw, 1.0, 0.0, 0.0);
            d.SetTime(210.0, 793, 11, 210 * 24);
            CollectionAssert.DoesNotContain(Call(d, "ripa"), Feud.OutlawCard, "the Franks' count has men at Ripa");
            Leave(d);
            CollectionAssert.Contains(Call(d, "traiectum"), Feud.OutlawCard, "she is an outlaw, and nobody at Traiectum knows it");
            s = d.OpenNext();
            Assert.AreEqual(Feud.OutlawCard, s.card.id);
            Assert.IsFalse(Can(s, "pay"), "four pounds can be paid out of 300 d");
            Assert.IsFalse(Can(s, "hold"), "an empty hold can be given up");
            Assert.IsTrue(Can(s, "talk"));
            d.Choose(s.card.Option("talk").index);
            d.Continue();
            Leave(d);
            d.vars.nums["silver"] = 1000.0;
            d.vars.nums["hold.value"] = 400.0;
            CollectionAssert.Contains(Call(d, "dorestad"), Feud.OutlawCard, "the count's men come once and never again");
            s = d.OpenNext();
            Assert.IsTrue(Can(s, "pay") && Can(s, "hold"));
            game.laws.Clear();
            d.Choose(s.card.Option("pay").index);
            CollectionAssert.AreEqual(new[] { "feud_outlaw_paid" }, LawsOf(game));
            d.Continue();
            Leave(d);
        }
    }
}
