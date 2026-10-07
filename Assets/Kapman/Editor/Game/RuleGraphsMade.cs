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
            var made = new List<ScriptGraphAsset> { Toll() };
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
