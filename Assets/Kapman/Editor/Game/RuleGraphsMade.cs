using System.Collections.Generic;
using Kapman.Graphs;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Kapman.EditorTools
{
    /// <summary>
    /// THE GRAPHS THAT HOLD RULES, as first made (Assets/Kapman/Game/Graphs/Rules), and the book that names them
    /// (Resources/GameRules). As with the game's other graphs: after they are made they are the user's, to open and
    /// change in the graph window; this only makes the ones that are missing.
    /// Menu: Kapman > Make physical > 3. The rules' graphs.   Batch: -executeMethod Kapman.EditorTools.RuleGraphsMade.Make
    /// </summary>
    public static class RuleGraphsMade
    {
        public const string Folder = "Assets/Kapman/Game/Graphs/Rules";
        public const string BookPath = "Assets/Kapman/Game/Resources/GameRules.asset";

        [MenuItem("Kapman/Make physical/3. The rules' graphs")]
        public static void Make()
        {
            var made = new List<ScriptGraphAsset> { Toll(), Pay(), Health(), Heart(), Fight(), Trick(), Instalment(), GoonsDo() };
            var book = AssetDatabase.LoadAssetAtPath<RuleBook>(BookPath);
            if (book == null)
            {
                book = ScriptableObject.CreateInstance<RuleBook>();
                AssetDatabase.CreateAsset(book, BookPath);
            }
            foreach (var g in made)
                if (g != null && !book.graphs.Contains(g))
                    book.graphs.Add(g);
            book.graphs.RemoveAll(g => g == null);
            EditorUtility.SetDirty(book);
            AssetDatabase.SaveAssets();
            Debug.Log("Kapman, made physical, step 3: " + BookPath + " names " + book.graphs.Count + " graph(s) of rules in " + Folder + ".");
        }

        // ------------------------------------------------------------------ a fight, and the lender's men (9 Oct 2026)

        /// <summary>How a fight stands: her nerve, a point for each man she has more than they (three either way at the
        /// most), a point for weapons aboard, and what the place costs her.</summary>
        public static ScriptGraphAsset Fight()
        {
            var g = new GraphBuilder("Rule - how a fight stands",
                "How a fight stands for her, on her nerve's own scale. The game asks this graph whenever a card's chance is a fight (GameRules.Fight, from Feud.Var: feud.fight, feud.door).");
            g.Group(-40, -80, 2300, 520, "How a fight stands",
                "Her nerve; then so much for each man she has more than they, or fewer (three either way at the most); then so much for weapons aboard; then what the place costs her.", GraphBuilder.Change);
            var start = g.Add(new FightRule(), 0, 0);
            var more = g.Add(new TakeOffIf(), 440, 180);
            g.Data(start.herMen, more.from);
            g.Data(start.theirMen, more.amount);
            var three = g.Add(new KeepBetween(), 720, 180);
            g.Data(more.left, three.number);
            g.Set(three.least, -3.0);
            g.Set(three.most, 3.0);
            var worth = g.Add(new GenericMultiply(), 980, 220);
            g.Data(start.perMan, worth.a);
            g.Data(three.kept, worth.b);
            var men = g.Add(new AddIf(), 1220, 120);
            g.Data(start.nerve, men.to);
            g.Data(worth.product, men.amount);
            var armed = g.Add(new AddIf(), 1500, 160);
            g.Data(men.sum, armed.to);
            g.Data(start.forWeapons, armed.amount);
            g.Data(start.weapons, armed.when);
            var place = g.Add(new AddIf(), 1780, 200);
            g.Data(armed.sum, place.to);
            g.Data(start.place, place.amount);
            var answer = g.Add(new RuleAnswer(), 2040, 0);
            g.Flow(start.trigger, answer.enter);
            g.Data(place.sum, answer.number);
            g.Note(-20, 330, 620, 110, "Where the numbers come from",
                "fightPerMan, fightWeapons, fightDoor and lenderHard in Game/Data/feud_rules_793.json. Her nerve is the stat as it stands, marks and all. Their men: the boat that is out, or the men in the lender's yard.");
            g.Note(640, 330, 620, 110, "What the answer does",
                "A card's chance takes it as its \"by\": each point over 3 adds a twentieth to the card's own odds, each point under 3 takes one off. The odds themselves are in the cards (encounters/wokerer_793.json).");
            return g.Save($"{Folder}/Rule - how a fight stands.asset");
        }

        /// <summary>How easy a way round the lender's men is: her stat, less for each time that way has fooled them
        /// before, less for each sending after the first.</summary>
        public static ScriptGraphAsset Trick()
        {
            var g = new GraphBuilder("Rule - a way round the lender's men",
                "How easy a way round the lender's men is, on her stat's own scale. The game asks this graph for each way on their card (GameRules.TrickEase, from Collectors.Var: goons.ease_hide and the rest).");
            g.Group(-40, -80, 2040, 560, "A way round the lender's men",
                "The stat that carries the trick; less for each time that way has fooled them before; less for each sending after the first.", GraphBuilder.Change);
            var start = g.Add(new TrickRule(), 0, 0);
            var used = g.Add(new KeepBetween(), 420, 160);
            g.Data(start.usedBefore, used.number);
            g.Set(used.least, 0.0);
            g.Set(used.most, 999.0);
            var told = g.Add(new GenericMultiply(), 680, 200);
            g.Data(start.toldPenalty, told.a);
            g.Data(used.kept, told.b);
            var afterTold = g.Add(new TakeOffIf(), 920, 100);
            g.Data(start.stat, afterTold.from);
            g.Data(told.product, afterTold.amount);
            var sendings = g.Add(new TakeOffIf(), 420, 340);
            g.Data(start.visit, sendings.from);
            g.Set(sendings.amount, 1.0);
            var later = g.Add(new KeepBetween(), 680, 340);
            g.Data(sendings.left, later.number);
            g.Set(later.least, 0.0);
            g.Set(later.most, 999.0);
            var laterCost = g.Add(new GenericMultiply(), 940, 340);
            g.Data(start.laterPenalty, laterCost.a);
            g.Data(later.kept, laterCost.b);
            var ease = g.Add(new TakeOffIf(), 1220, 160);
            g.Data(afterTold.left, ease.from);
            g.Data(laterCost.product, ease.amount);
            var answer = g.Add(new RuleAnswer(), 1500, 0);
            g.Flow(start.trigger, answer.enter);
            g.Data(ease.left, answer.number);
            g.Note(-20, 480, 620, 110, "Where the numbers come from",
                "toldPenalty and laterPenalty in Game/Data/collectors_793.json; which stat carries which way is in the same file (tricks). \"He said look under the stones this time.\"");
            return g.Save($"{Folder}/Rule - a way round the lender's men.asset");
        }

        /// <summary>What the lender's men come for: an instalment, and more of it for each step of his temper over 2.</summary>
        public static ScriptGraphAsset Instalment()
        {
            var g = new GraphBuilder("Rule - what the lender's men come for",
                "What the lender's men come for when they are given nothing, in deniers. The game asks this graph when their card is answered so (GameRules.Instalment, from Collectors.Bite).");
            g.Group(-40, -80, 2100, 440, "What the lender's men come for",
                "An instalment; and so much more of it again for each step of the lender's temper over 2 (a way round them that has just failed counts as a step). To the whole penny.", GraphBuilder.Change);
            var start = g.Add(new InstalmentRule(), 0, 0);
            var over = g.Add(new TakeOffIf(), 420, 160);
            g.Data(start.temper, over.from);
            g.Set(over.amount, 2.0);
            var steps = g.Add(new KeepBetween(), 680, 160);
            g.Data(over.left, steps.number);
            g.Set(steps.least, 0.0);
            g.Set(steps.most, 999.0);
            var more = g.Add(new GenericMultiply(), 940, 200);
            g.Data(start.moreByTemper, more.a);
            g.Data(steps.kept, more.b);
            var share = g.Add(new AddIf(), 1180, 200);
            g.Set(share.to, 1.0);
            g.Data(more.product, share.amount);
            var sum = g.Add(new GenericMultiply(), 1440, 120);
            g.Data(start.instalment, sum.a);
            g.Data(share.sum, sum.b);
            var whole = g.Add(new ToTheWhole(), 1680, 120);
            g.Data(sum.product, whole.sum);
            var answer = g.Add(new RuleAnswer(), 1880, 0);
            g.Flow(start.trigger, answer.enter);
            g.Data(whole.rounded, answer.number);
            g.Note(-20, 300, 620, 110, "Where the numbers come from",
                "instalmentD and moreByTemper in Game/Data/collectors_793.json. The lender's temper starts at 1, falls when she pays and rises when his men come home with nothing or fooled.");
            return g.Save($"{Folder}/Rule - what the lender's men come for.asset");
        }

        /// <summary>What the lender's men do when they are given nothing: they talk, take, beat a man, kill a man, or
        /// kill Ravn.</summary>
        public static ScriptGraphAsset GoonsDo()
        {
            var g = new GraphBuilder("Rule - what the lender's men do",
                "What the lender's men do when they are given nothing: 0 they only talk, 1 they take what they came for, 2 they take and beat a man, 3 they take and kill a man, 4 they take and kill Ravn. " +
                "The game asks this graph when their card is answered so (GameRules.GoonsDo, from Collectors.Bite).");
            g.Group(-40, -100, 3900, 1260, "What the lender's men do when they are given nothing",
                "It depends (the user, 9 Oct 2026: \"Dependant\", \"It'll be dynamic you know\"): on the lender's temper, on what there is aboard to take, on whether she has just made fools of them, and on how often she has paid nothing.",
                GraphBuilder.Change);
            var start = g.Add(new GoonsDoRule(), 0, 0);
            // his temper, a step more when a way round them has just failed
            var temper = g.Add(new AddIf(), 520, 0);
            g.Data(start.temper, temper.to);
            g.Set(temper.amount, 1.0);
            g.Data(start.fooled, temper.when);
            // IS IT RAVN: the rule is on, she has paid nothing that often, he lives, and he is not her last man
            var ruleOn = g.Add(new IsIt { how = IsIt.How.AtLeast }, 520, 200);
            g.Data(start.ravnAt, ruleOn.a);
            g.Set(ruleOn.b, 1.0);
            var counted = g.Add(new IsIt { how = IsIt.How.AtLeast }, 520, 340);
            g.Data(start.unpaid, counted.a);
            g.Data(start.ravnAt, counted.b);
            var many = g.Add(new IsIt { how = IsIt.How.Over }, 520, 480);
            g.Data(start.men, many.a);
            g.Set(many.b, 1.0);
            var due = g.Add(new BothOrEither { how = BothOrEither.How.Both }, 820, 260);
            g.Data(ruleOn.so, due.one);
            g.Data(counted.so, due.other);
            var alive = g.Add(new BothOrEither { how = BothOrEither.How.Both }, 1100, 300);
            g.Data(due.so, alive.one);
            g.Data(start.ravnAlive, alive.other);
            var ravn = g.Add(new BothOrEither { how = BothOrEither.How.Both }, 1380, 340);
            g.Data(alive.so, ravn.one);
            g.Data(many.so, ravn.other);
            // DO THEY ONLY TALK: his temper is no higher than that, and it is not Ravn
            var calm = g.Add(new IsIt { how = IsIt.How.AtMost }, 820, 20);
            g.Data(temper.sum, calm.a);
            g.Data(start.warnUpTo, calm.b);
            var talk = g.Add(new BothOrEither { how = BothOrEither.How.FirstAndNotSecond }, 1660, 60);
            g.Data(calm.so, talk.one);
            g.Data(ravn.so, talk.other);
            // IS THERE LITTLE TO TAKE: under half of what they came for; and is it hard on her (little, or fools made of them)
            var half = g.Add(new GenericMultiply(), 520, 660);
            g.Data(start.want, half.a);
            g.Value(half.b, 0.5, 340, 720);
            var little = g.Add(new IsIt { how = IsIt.How.Under }, 820, 620);
            g.Data(start.aboard, little.a);
            g.Data(half.product, little.b);
            var hard = g.Add(new BothOrEither { how = BothOrEither.How.Either }, 1100, 640);
            g.Data(little.so, hard.one);
            g.Data(start.fooled, hard.other);
            // IS A MAN KILLED: his temper has reached that; or a step short of it, with little aboard, a lie just told, and the throw against her
            var killNow = g.Add(new IsIt { how = IsIt.How.AtLeast }, 820, 820);
            g.Data(temper.sum, killNow.a);
            g.Data(start.killFrom, killNow.b);
            var stepShort = g.Add(new TakeOffIf(), 520, 960);
            g.Data(start.killFrom, stepShort.from);
            g.Set(stepShort.amount, 1.0);
            var near = g.Add(new IsIt { how = IsIt.How.AtLeast }, 820, 960);
            g.Data(temper.sum, near.a);
            g.Data(stepShort.left, near.b);
            var unlucky = g.Add(new IsIt { how = IsIt.How.Under }, 820, 1100);
            g.Data(start.throwOf, unlucky.a);
            g.Data(start.killEarly, unlucky.b);
            var nearAndLittle = g.Add(new BothOrEither { how = BothOrEither.How.Both }, 1100, 940);
            g.Data(near.so, nearAndLittle.one);
            g.Data(little.so, nearAndLittle.other);
            var andFooled = g.Add(new BothOrEither { how = BothOrEither.How.Both }, 1380, 980);
            g.Data(nearAndLittle.so, andFooled.one);
            g.Data(start.fooled, andFooled.other);
            var early = g.Add(new BothOrEither { how = BothOrEither.How.Both }, 1660, 1020);
            g.Data(andFooled.so, early.one);
            g.Data(unlucky.so, early.other);
            var kill = g.Add(new BothOrEither { how = BothOrEither.How.Either }, 1940, 860);
            g.Data(killNow.so, kill.one);
            g.Data(early.so, kill.other);
            // IS A MAN BEATEN: his temper has reached that
            var beat = g.Add(new IsIt { how = IsIt.How.AtLeast }, 1380, 500);
            g.Data(temper.sum, beat.a);
            g.Data(start.beatFrom, beat.b);
            // with more men than one aboard, and hard on her: killed, or beaten, or only taken from
            var beatenOrTaken = g.Add(new ThisOrThat(), 2220, 520);
            g.Data(beat.so, beatenOrTaken.when);
            g.Set(beatenOrTaken.yes, 2.0);
            g.Set(beatenOrTaken.no, 1.0);
            var withMen = g.Add(new ThisOrThat(), 2500, 700);
            g.Data(kill.so, withMen.when);
            g.Set(withMen.yes, 3.0);
            g.Data(beatenOrTaken.picked, withMen.no);
            // else (her last man, or nothing hard about it): beaten only if it is hard on her and his temper has reached it
            var beatHard = g.Add(new BothOrEither { how = BothOrEither.How.Both }, 2220, 340);
            g.Data(beat.so, beatHard.one);
            g.Data(hard.so, beatHard.other);
            var alone = g.Add(new ThisOrThat(), 2500, 360);
            g.Data(beatHard.so, alone.when);
            g.Set(alone.yes, 2.0);
            g.Set(alone.no, 1.0);
            var crowd = g.Add(new BothOrEither { how = BothOrEither.How.Both }, 2500, 540);
            g.Data(many.so, crowd.one);
            g.Data(hard.so, crowd.other);
            var taken = g.Add(new ThisOrThat(), 2800, 520);
            g.Data(crowd.so, taken.when);
            g.Data(withMen.picked, taken.yes);
            g.Data(alone.picked, taken.no);
            // Ravn before any of that; and talk before everything
            var orRavn = g.Add(new ThisOrThat(), 3080, 360);
            g.Data(ravn.so, orRavn.when);
            g.Set(orRavn.yes, 4.0);
            g.Data(taken.picked, orRavn.no);
            var done = g.Add(new ThisOrThat(), 3360, 120);
            g.Data(talk.so, done.when);
            g.Set(done.yes, 0.0);
            g.Data(orRavn.picked, done.no);
            var answer = g.Add(new RuleAnswer(), 3640, 0);
            g.Flow(start.trigger, answer.enter);
            g.Data(done.picked, answer.number);
            g.Note(2900, 760, 880, 260, "The answer",
                "0  they only talk, and go home.\n1  they take what they came for: silver first, then cargo for the rest.\n2  they take, and beat a man.\n3  they take, and kill a man.\n4  they take, and kill Ravn.\n" +
                "What they take is another rule (Rule - what the lender's men come for).");
            g.Note(-20, 1000, 460, 130, "Where the numbers come from",
                "warnUpTo, beatFrom, killFrom, killEarly and ravnAtRefusal in Game/Data/collectors_793.json. The throw is the game's own, one for each sending.");
            return g.Save($"{Folder}/Rule - what the lender's men do.asset");
        }

        /// <summary>What the crew is owed for a day: so much a man.</summary>
        public static ScriptGraphAsset Pay()
        {
            var g = new GraphBuilder("Rule - the crew's pay for a day",
                "What is added each day to what the crew is owed. The game asks this graph once a game day (GameRules.DayPay, from Voyage.Day).");
            g.Group(-40, -80, 1100, 420, "The crew's pay for a day", "The men aboard, times a man's pay for a day.", GraphBuilder.Change);
            var start = g.Add(new PayRule(), 0, 0);
            var times = g.Add(new GenericMultiply(), 380, 120);
            g.Data(start.men, times.a);
            g.Data(start.payAMan, times.b);
            var answer = g.Add(new RuleAnswer(), 700, 0);
            g.Flow(start.trigger, answer.enter);
            g.Data(times.product, answer.number);
            g.Note(-20, 220, 520, 130, "Where the numbers come from",
                "A man's pay a day: 'pay' in Game/Data/voyage_rules.json. They are paid what is owed when she next lies in a port; a man owed too long goes ashore for good (Voyage.InPort).");
            g.Note(540, 220, 480, 130, "Change it here",
                "A steersman's share, more for a long voyage, less for a boy: put it between the two nodes. The test RuleGraphTests holds this graph to the plain sum until it is changed on purpose.");
            return g.Save($"{Folder}/Rule - the crew's pay for a day.asset");
        }

        /// <summary>The crew's health after a day.</summary>
        public static ScriptGraphAsset Health()
        {
            var g = new GraphBuilder("Rule - the crew's health after a day",
                "How a day leaves the crew's health (0 to 1). The game asks this graph once a game day (GameRules.Health, from Voyage.Day).");
            g.Group(-40, -80, 1760, 520, "The crew's health after a day",
                "A day without food takes from it, a day without drink takes from it, a day with both gives some back; never under 0 nor over 1.", GraphBuilder.Change);
            var start = g.Add(new HealthRule(), 0, 0);
            var hunger = g.Add(new TakeOffIf(), 420, 140);
            g.Data(start.health, hunger.from);
            g.Data(start.hungerLoss, hunger.amount);
            g.Data(start.hungry, hunger.when);
            var thirst = g.Add(new TakeOffIf(), 700, 180);
            g.Data(hunger.left, thirst.from);
            g.Data(start.thirstLoss, thirst.amount);
            g.Data(start.thirsty, thirst.when);
            var good = g.Add(new AddIf(), 980, 220);
            g.Data(thirst.left, good.to);
            g.Data(start.gain, good.amount);
            g.Data(start.well, good.when);
            var kept = g.Add(new Between01(), 1240, 220);
            g.Data(good.sum, kept.number);
            var answer = g.Add(new RuleAnswer(), 1480, 0);
            g.Flow(start.trigger, answer.enter);
            g.Data(kept.kept, answer.number);
            g.Note(-20, 330, 560, 100, "Where the numbers come from",
                "hungerPerDay, thirstPerDay and recoverPerDay in Game/Data/voyage_rules.json. Food and drink are what is in the hold and the casks (Voyage.Day).");
            return g.Save($"{Folder}/Rule - the crew's health after a day.asset");
        }

        /// <summary>The crew's heart after a day.</summary>
        public static ScriptGraphAsset Heart()
        {
            var g = new GraphBuilder("Rule - the crew's heart after a day",
                "How a day leaves the crew's heart (0 to 1). The game asks this graph once a game day (GameRules.Heart, from Voyage.Day).");
            g.Group(-40, -80, 2320, 620, "The crew's heart after a day",
                "A rough sea takes from it, and hunger, thirst and pay owed for more than a week; a day in port gives some back; never under 0 nor over 1.", GraphBuilder.Change);
            var start = g.Add(new HeartRule(), 0, 0);
            var sea = g.Add(new TakeOffIf(), 440, 180);
            g.Data(start.heart, sea.from);
            g.Data(start.lostToTheSea, sea.amount);
            var hungry = g.Add(new TakeOffIf(), 720, 220);
            g.Data(sea.left, hungry.from);
            g.Data(start.hungryLoss, hungry.amount);
            g.Data(start.hungry, hungry.when);
            var thirsty = g.Add(new TakeOffIf(), 1000, 260);
            g.Data(hungry.left, thirsty.from);
            g.Data(start.thirstyLoss, thirsty.amount);
            g.Data(start.thirsty, thirsty.when);
            var unpaid = g.Add(new TakeOffIf(), 1280, 300);
            g.Data(thirsty.left, unpaid.from);
            g.Data(start.unpaidLoss, unpaid.amount);
            g.Data(start.unpaid, unpaid.when);
            var port = g.Add(new AddIf(), 1560, 340);
            g.Data(unpaid.left, port.to);
            g.Data(start.portGain, port.amount);
            g.Data(start.inPort, port.when);
            var kept = g.Add(new Between01(), 1820, 340);
            g.Data(port.sum, kept.number);
            var answer = g.Add(new RuleAnswer(), 2060, 0);
            g.Flow(start.trigger, answer.enter);
            g.Data(kept.kept, answer.number);
            g.Note(-20, 420, 620, 110, "Where the numbers come from",
                "seasickPerDay, hungryPerDay, thirstyPerDay, unpaidPerDay and recoverInPort in Game/Data/voyage_rules.json. " +
                "Lost to the sea today: a rough sea, less each day as the men get their sea legs (Voyage.Day).");
            return g.Save($"{Folder}/Rule - the crew's heart after a day.asset");
        }

        /// <summary>The toll on a sale of the merchant's: a share of what the market paid, times what a toll man here has
        /// made of it, to the hundredth of a denier; nothing where a charter frees her.</summary>
        public static ScriptGraphAsset Toll()
        {
            var g = new GraphBuilder("Rule - the toll on a sale",
                "What the toll takes when the merchant sells. The game asks this graph each time (GameRules.Toll); the world's other ships go by the same sum in code.");
            g.Group(-40, -80, 1640, 560, "The toll on what the merchant sells",
                "Sold for, times the toll's share, times what it is multiplied by here and now, to the hundredth of a denier. Free of toll here: nothing.", GraphBuilder.Change);
            var start = g.Add(new TollRule(), 0, 0);
            var share = g.Add(new GenericMultiply(), 380, 150);
            g.Data(start.soldFor, share.a);
            g.Data(start.share, share.b);
            var here = g.Add(new GenericMultiply(), 620, 200);
            g.Data(share.product, here.a);
            g.Data(start.factor, here.b);
            var round = g.Add(new RoundToHundredth(), 860, 200);
            g.Data(here.product, round.sum);
            var pick = g.Add(new SelectUnit(), 1100, 120);
            g.Data(start.free, pick.condition);
            g.Value(pick.ifTrue, 0.0, 900, 90);
            g.Data(round.rounded, pick.ifFalse);
            var answer = g.Add(new RuleAnswer(), 1360, 0);
            g.Flow(start.trigger, answer.enter);
            g.Data(pick.selection, answer.number);
            g.Note(-20, 300, 520, 170, "Where the numbers come from",
                "The toll's share: each zone's 'toll' in Game/Data/trade_rules.json (0.1 is a tenth).\n" +
                "Times, here and now: 1 as a rule; a toll man bought off or crossed changes it for a while (the cards).\n" +
                "Free of toll here: a charter won in the chest (Chest.TollFreeAt).");
            g.Note(540, 300, 560, 170, "Change it here",
                "This graph is the merchant's rule. Put a node between 'To the hundredth' and the answer and his toll changes " +
                "(a flat fee added, a ceiling, a kinder toll for a known man...). The world's other ships keep to the plain sum in code " +
                "(GameRules.TollByCode), and a test holds this graph to that sum until it is changed on purpose (RuleGraphTests).");
            return g.Save($"{Folder}/Rule - the toll on a sale.asset");
        }
    }
}
