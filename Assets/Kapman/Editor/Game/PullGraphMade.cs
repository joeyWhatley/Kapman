using Kapman.Game;
using Kapman.Graphs;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kapman.EditorTools
{
    /// <summary>
    /// THE GRAPH OF THE COURSE PULLED ON THE CHART, as first made, and its Script Machine on the '--- GAME ---' object of
    /// the Game scene (13 Oct 2026). As with the game's other graphs: once made it is the user's, to open and change;
    /// this only makes it if it is missing.
    /// Menu: Kapman > Make physical > 3g. The graph of the course pulled.   Batch: -executeMethod Kapman.EditorTools.PullGraphMade.Make
    /// </summary>
    public static class PullGraphMade
    {
        public const string Path = GameGraphs.Folder + "/Chart - the course is pulled.asset";

        public static ScriptGraphAsset Pull()
        {
            var g = new GraphBuilder("Chart - the course is pulled",
                "A knot of her course has been let go on the chart, pulled again, or taken off (Code/Game/Voyage/CoursePulls.cs; the chart's pulling is in Map/MapView.cs). " +
                "Here the knot is taken to the water where it lay on land, her course is laid again through her knots by the lanes (Course.SetThrough), and the log is told. " +
                "Whether a press takes the line at all is the rule graph's (Game/Graphs/Rules: does the press take the line).");
            g.Group(-40, -90, 2300, 420, "The course is pulled",
                "Your ruling of 13 October 2026: pull the line anywhere; by the lanes through your point; a point let go on land goes to the nearest water; a point may be let go in the dark.",
                GraphBuilder.Change);
            var on = g.Add(new OnTheCourseIsPulled(), 0, 0);
            var land = g.Add(new If(), 460, 0);
            g.Flow(on.trigger, land.enter);
            g.Data(on.onLand, land.condition);
            var water = g.Add(new KnotToWater(), 860, -40);
            g.Flow(land.ifTrue, water.enter);
            var lay = g.Add(new LayThroughKnots(), 1300, 0);
            g.Flow(water.exit, lay.enter);
            g.Flow(land.ifFalse, lay.enter);
            var say = g.Add(new CourseSay(), 1800, 0);
            g.Flow(lay.exit, say.enter);
            g.Data(lay.words, say.words);
            g.Note(-20, 200, 560, 260, "The moments",
                "pulled: a new knot was let go on the line. It goes among her knots by the point of the line it was pulled from.\n" +
                "moved: a knot she already had was pulled again.\n" +
                "off: a knot was taken off (a right click on it).\n" +
                "On land: the point let go lies on dry land or the sands, by the world bake.");
            g.Note(580, 200, 560, 260, "The words",
                "The words come from the laying (\"Her course is laid again through your knot: 212 km, about 2.5 days.\"): PLACEHOLDER WORDS, in CoursePulls.cs, the user's to write. " +
                "Where no water leads through the point, her course stands as it was and the log says so. The clock does not drop back for a pull: it is the player's own hand.");
            g.Note(1180, 200, 560, 260, "What is not here",
                "The pointer on the chart (which press takes the line, the knot following the hand, the right click) and the laying itself by the lanes are the code's (MapView.cs, Course.cs). " +
                "How near the pointer must be to the line is the rule graph \"Rule - does the press take the line\".");
            return g.Save(Path);
        }

        /// <summary>The graph made again OVER the one that is there, into the same asset. Only while nobody has changed
        /// it by hand: what was changed in the graph window is lost.
        /// Batch: -executeMethod Kapman.EditorTools.PullGraphMade.Remake</summary>
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

        [MenuItem("Kapman/Make physical/3g. The graph of the course pulled")]
        public static void Make()
        {
            // (the scene first: opening a scene lets go of assets nothing holds, and the graph just made would be one)
            var scene = EditorSceneManager.OpenScene(KapmanPhysical.GameScene, OpenSceneMode.Single);
            var graph = Pull();
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
            Debug.Log("Kapman, made physical, step 3g: the object " + GoonsGraphMade.GameObjectName + " runs " + Path + ".");
        }

        /// <summary>Everything of 13 Oct 2026 in one headless run: the frame's parts (the knot's file), the rules' graphs
        /// (the press that takes the line), and this graph on the scene.
        /// Batch: -executeMethod Kapman.EditorTools.PullGraphMade.MakeAll</summary>
        public static void MakeAll()
        {
            KapmanPhysical.Step2Parts();
            RuleGraphsMade.Make();
            Make();
        }
    }
}
