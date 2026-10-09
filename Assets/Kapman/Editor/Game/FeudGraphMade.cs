using System.Collections.Generic;
using Kapman.Game;
using Kapman.Graphs;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kapman.EditorTools
{
    /// <summary>
    /// THE GRAPH OF WHAT SHE DOES ABOUT RAVN'S DEATH, as first made, and its Script Machine on the '--- GAME ---' object
    /// of the Game scene. As with the game's other graphs: once made it is the user's, to open and change; this only
    /// makes it if it is missing.
    /// Menu: Kapman > Make physical > 3e. The graph of the feud.   Batch: -executeMethod Kapman.EditorTools.FeudGraphMade.Make
    /// </summary>
    public static class FeudGraphMade
    {
        public const string Path = GameGraphs.Folder + "/Feud - what she does about Ravn's death.asset";

        /// <summary>The answers, in the order of the graph's rows.</summary>
        public static readonly string[] Answers =
        {
            "swear", "law", "quiet", "bury_priest", "pyre", "bury_water", "bury_aboard", "blood", "blood_lost", "lender", "lender_lost", "price", "outlaw_paid", "outlaw_seized", "let_lie",
        };

        public static ScriptGraphAsset Feud()
        {
            var g = new GraphBuilder("Feud - what she does about Ravn's death",
                "The lender's men have killed Ravn, and it lies on her until it is answered (Code/Game/Voyage/Feud.cs; Data/feud_rules_793.json). " +
                "She answers on the feud's seven cards (Data/encounters/wokerer_793.json), from the Errands window, or by saying nothing until the days run out: what each answer does, a row to an answer. " +
                "His scene is the graph \"Scene - Ravn is killed\"; when each card is put before her is the code's.");
            const float y0 = 0f, row = 150f, x0 = 800f, dx = 440f;
            float Y(string answer) => y0 + System.Array.IndexOf(Answers, answer) * row;
            float tall = row * Answers.Length;
            g.Group(-40, -90, x0 + dx * 4 + 380, tall + 90, "She answers",
                "Over him she says what is to be done (swear his blood, take it to the count, or say nothing); then where he lies; then blood or silver, or the lender himself; and an outlaw's dealings with the count.",
                GraphBuilder.Change);
            var on = g.Add(new OnRavnsDeathIsAnswered(), 0, y0);
            var which = g.Add(new SwitchOnString { options = new List<string>(Answers) }, 420, y0);
            g.Flow(on.trigger, which.enter);
            g.Data(on.answer, which.selector);
            ControlOutput Of(string answer) => which.branches[System.Array.IndexOf(Answers, answer)].Value;

            // what she says over him
            {
                var swears = g.Add(new FeudSheSwearsHisBlood(), x0, Y("swear"));
                g.Flow(Of("swear"), swears.enter);
                var law = g.Add(new FeudSheTakesItToTheCount(), x0, Y("law"));
                g.Flow(Of("law"), law.enter);
                var quiet = g.Add(new FeudSheSaysNothing(), x0, Y("quiet"));
                g.Flow(Of("quiet"), quiet.enter);
            }
            // where he lies
            {
                var priest = g.Add(new FeudHeIsLaidToRest(), x0, Y("bury_priest"));
                g.Flow(Of("bury_priest"), priest.enter);
                g.Set(priest.place, 1);
                var fire = g.Add(new FeudHeIsLaidToRest(), x0, Y("pyre"));
                g.Flow(Of("pyre"), fire.enter);
                g.Set(fire.place, 2);
                var water = g.Add(new FeudHeIsLaidToRest(), x0, Y("bury_water"));
                g.Flow(Of("bury_water"), water.enter);
                g.Set(water.place, 3);
                var aboard = g.Add(new FeudHeIsCarriedAboard(), x0, Y("bury_aboard"));
                g.Flow(Of("bury_aboard"), aboard.enter);
            }
            // blood: she has killed the one who did it
            {
                float y = Y("blood");
                var dead = g.Add(new FeudHisKillerIsDead(), x0, y);
                g.Flow(Of("blood"), dead.enter);
                var fine = g.Add(new FeudThePortsLordFinesTheBlood(), x0 + dx, y);
                g.Flow(dead.exit, fine.enter);
                var word = g.Add(new FeudTheWordGoesRound(), x0 + dx * 2, y);
                g.Flow(fine.exit, word.enter);
                g.Set(word.deed, "took_blood_for_her_man");
                var over = g.Add(new FeudItIsOver(), x0 + dx * 3, y);
                g.Flow(word.exit, over.enter);
                g.Set(over.how, 1);
                var lost = g.Add(new FeudTheFightIsLost(), x0, Y("blood_lost"));
                g.Flow(Of("blood_lost"), lost.enter);
            }
            // the lender himself
            {
                float y = Y("lender");
                var dead = g.Add(new FeudTheLenderIsDead(), x0, y);
                g.Flow(Of("lender"), dead.enter);
                var word = g.Add(new FeudTheWordGoesRound(), x0 + dx, y);
                g.Flow(dead.exit, word.enter);
                g.Set(word.deed, "killed_the_lender");
                var over = g.Add(new FeudItIsOver(), x0 + dx * 2, y);
                g.Flow(word.exit, over.enter);
                g.Set(over.how, 3);
                float yLost = Y("lender_lost");
                var thrown = g.Add(new FeudSheIsThrownOut(), x0, yLost);
                g.Flow(Of("lender_lost"), thrown.enter);
                var rage = g.Add(new LenderIsPastPatience(), x0 + dx, yLost);
                g.Flow(thrown.exit, rage.enter);
            }
            // the count's court: his price
            {
                float y = Y("price");
                var price = g.Add(new FeudHisPriceComesOffTheStick(), x0, y);
                g.Flow(Of("price"), price.enter);
                var over = g.Add(new FeudItIsOver(), x0 + dx, y);
                g.Flow(price.exit, over.enter);
                g.Set(over.how, 2);
            }
            // an outlaw, and the count
            {
                var paid = g.Add(new FeudShePaysTheLendersPrice(), x0, Y("outlaw_paid"));
                g.Flow(Of("outlaw_paid"), paid.enter);
                var seized = g.Add(new FeudTheCountsMenTakeTheCargo(), x0, Y("outlaw_seized"));
                g.Flow(Of("outlaw_seized"), seized.enter);
            }
            // she said nothing, and the days of that have run out
            {
                var over = g.Add(new FeudItIsOver(), x0, Y("let_lie"));
                g.Flow(Of("let_lie"), over.enter);
                g.Set(over.how, 4);
            }
            g.Note(0, 420, 380, 420, "The answers",
                "swear: she swears his blood.\nlaw: she takes it to the count.\nquiet: she says nothing.\nbury_priest, pyre, bury_water: where he is laid.\nbury_aboard: carried aboard, for a fire on a shore.\n" +
                "blood: she has killed the one who did it.\nblood_lost: that fight went against her.\nlender: she has killed the lender.\nlender_lost: she was thrown out of his house.\n" +
                "price: the count sets his price on the lender.\noutlaw_paid, outlaw_seized: an outlaw before the count's men.\nlet_lie: she said nothing, and the days ran out.");
            g.Note(0, 880, 380, 300, "The numbers, and what is said",
                "How much the crew's heart rises, the fines and the prices are the table's (Game/Data/feud_rules_793.json). How a fight goes is the card's own chance, carried by \"Rule - how a fight stands\". " +
                "What a part has to say of what happened is put into the card's end, in the order the parts run. PLACEHOLDER WORDS, in Feud.cs: the user's to write.");
            g.Note(0, 1220, 380, 260, "What is not here",
                "WHEN each card is put before her (over him; the one who did it, when the rope men next come; where he lies; a shore for his fire) is the code's, by the table's numbers (Feud.Step). " +
                "The punishment that falls at his death (a man fewer, the crew's heart, the mark on her) falls before his scene, in the code (Feud.RavnKilled).");
            return g.Save(Path);
        }

        /// <summary>Makes the graph again OVER the one that is there (into the same asset, so the machine that runs it keeps
        /// it). Only for a graph nobody has changed by hand: what was changed in the graph window is lost.
        /// Batch: -executeMethod Kapman.EditorTools.FeudGraphMade.Remake</summary>
        public static void Remake()
        {
            GraphBuilder.ReplaceExisting = true;
            try
            {
                Make();
            }
            finally
            {
                GraphBuilder.ReplaceExisting = false;
            }
        }

        [MenuItem("Kapman/Make physical/3e. The graph of the feud")]
        public static void Make()
        {
            // (the scene first: opening a scene lets go of assets nothing holds, and the graph just made would be one)
            var scene = EditorSceneManager.OpenScene(KapmanPhysical.GameScene, OpenSceneMode.Single);
            var graph = Feud();
            if (graph == null)
                graph = AssetDatabase.LoadAssetAtPath<ScriptGraphAsset>(Path);
            if (graph == null)
                throw new System.InvalidOperationException(Path + " could not be made or read.");
            var game = GameObject.Find(GoonsGraphMade.GameObjectName);
            if (game == null)
                throw new System.InvalidOperationException(KapmanPhysical.GameScene + " has no object named " + GoonsGraphMade.GameObjectName + " (the one that runs the game's graphs).");
            bool has = false;
            foreach (var m in game.GetComponents<ScriptMachine>())
                has |= m.nest.macro == graph;
            if (!has)
            {
                var machine = game.AddComponent<ScriptMachine>();
                machine.nest.source = GraphSource.Macro;
                machine.nest.macro = graph;
                EditorUtility.SetDirty(machine);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Kapman, made physical, step 3e: the object " + GoonsGraphMade.GameObjectName + " runs " + Path + ".");
        }
    }
}
