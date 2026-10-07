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
            var made = new List<ScriptGraphAsset> { Toll(), Pay(), Health(), Heart() };
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
