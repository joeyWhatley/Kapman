using Kapman.Game;
using Kapman.Graphs;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kapman.EditorTools
{
    /// <summary>
    /// THE GRAPH OF YOUR HAND ON THE HELM, as first made, and its Script Machine on the '--- GAME ---' object of the
    /// Game scene (13 Oct 2026). As with the game's other graphs: once made it is the user's, to open and change; this
    /// only makes it if it is missing.
    /// Menu: Kapman > Make physical > 3h. The graph of the helm.   Batch: -executeMethod Kapman.EditorTools.HelmGraphMade.Make
    /// </summary>
    public static class HelmGraphMade
    {
        public const string Path = GameGraphs.Folder + "/Voyage - the helm is yours.asset";

        public static ScriptGraphAsset Helm()
        {
            var g = new GraphBuilder("Voyage - the helm is yours",
                "In the 3D view your hand takes the helm (A and D, or the mouse held to a side), she answers it, and her charted course waits (Code/Game/Voyage/HandHelm.cs; the turning is ChartBody.Move's, the keys ShipStageHook's). " +
                "When the helm is given back (H, or the swap to the chart) the steersman lays her course again from where she is, or she lies to; the log is told. " +
                "How fast she answers, and whether he lays her course again, are the rule graphs' (Game/Graphs/Rules).");
            g.Group(-40, -90, 2300, 420, "The helm is yours",
                "Your ruling of 13 October 2026: steer her yourself in the 3D view. (It overturns the rulings of 26 September and 10 October that her bow is not turned by hand.)",
                GraphBuilder.Change);
            var on = g.Add(new OnTheHelm(), 0, 0);
            var back = g.Add(new If(), 460, 0);
            g.Flow(on.trigger, back.enter);
            g.Data(on.givenBack, back.condition);
            var lay = g.Add(new HelmLayAgain(), 860, -40);
            g.Flow(back.ifTrue, lay.enter);
            var sayLaid = g.Add(new HelmSay(), 1360, -40);
            g.Flow(lay.exit, sayLaid.enter);
            g.Data(lay.words, sayLaid.words);
            var say = g.Add(new HelmSay(), 1360, 100);
            g.Flow(back.ifFalse, say.enter);
            g.Data(on.words, say.words);
            g.Note(-20, 200, 560, 260, "The moments",
                "taken: your hand is on the helm; her charted course waits.\n" +
                "let_go: the helm is amidships again; she holds her heading (nothing is said).\n" +
                "given_back: the helm is the steersman's again (H, or the swap back to the chart): he lays her course again from where she is, or she lies to.\n" +
                "aground: your hand has run her aground; the helm is let go, and the getting off is as it always was (the Aground graph).");
            g.Note(580, 200, 560, 260, "The words",
                "The words come with the event, or from the laying (\"The helm is the steersman's again: her course is laid again from where she is, 98 km, about 1.2 days.\"): PLACEHOLDER WORDS, in HandHelm.cs, the user's to write. " +
                "The clock does not drop back for the helm: it is the player's own hand.");
            g.Note(1180, 200, 560, 260, "What is not here",
                "The keys and the mouse (ShipStageHook), how she turns and makes way under your hand (ChartBody.Move), and the laying by the lanes (Adrift.LayAgain, Course.SetTo) are the code's. " +
                "How fast she answers the helm, and whether the steersman lays her course again when it is given back, are the rule graphs.");
            return g.Save(Path);
        }

        /// <summary>The graph made again OVER the one that is there, into the same asset. Only while nobody has changed
        /// it by hand. Batch: -executeMethod Kapman.EditorTools.HelmGraphMade.Remake</summary>
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

        [MenuItem("Kapman/Make physical/3h. The graph of the helm")]
        public static void Make()
        {
            var scene = EditorSceneManager.OpenScene(KapmanPhysical.GameScene, OpenSceneMode.Single);
            var graph = Helm();
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
            Debug.Log("Kapman, made physical, step 3h: the object " + GoonsGraphMade.GameObjectName + " runs " + Path + ".");
        }

        /// <summary>Everything of the helm in one headless run: the frame's parts (the helm's mark), the rules' graphs
        /// (how she answers, the course laid again), and this graph on the scene.
        /// Batch: -executeMethod Kapman.EditorTools.HelmGraphMade.MakeAll</summary>
        public static void MakeAll()
        {
            KapmanPhysical.Step2Parts();
            RuleGraphsMade.Make();
            Make();
        }
    }
}
