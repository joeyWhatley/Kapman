using Kapman.Game;
using Kapman.Graphs;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kapman.EditorTools
{
    /// <summary>
    /// THE OPENING'S GRAPH, as first made, and its Script Machine on the Showtime object of the Game scene. As with the
    /// game's other graphs: once made it is the user's, to open and change; this only makes it if it is missing.
    /// Menu: Kapman > Make physical > 3b. The opening's graph.   Batch: -executeMethod Kapman.EditorTools.OpeningGraphMade.Make
    /// </summary>
    public static class OpeningGraphMade
    {
        public const string Path = GameGraphs.Folder + "/Opening - the lender, the boat, the first morning.asset";

        public static ScriptGraphAsset Opening()
        {
            var g = new GraphBuilder("Opening - the lender, the boat, the first morning",
                "What follows the title when a new game begins, part by part, left to right. Each node is a part of the show (Code/Game/UI/Showtime.cs).");
            g.Group(-40, -90, 3080, 330, "A new game begins",
                "The box comes up; who are you; the game begins as answered; the lender speaks; the showroom; the first morning; the first click; the marks; Ravn.", GraphBuilder.Talk);
            var on = g.Add(new OnOpening { coroutine = true }, 0, 0);
            float x = 320f;
            ControlOutput from = on.trigger;
            T Next<T>(T node) where T : OpeningPart
            {
                g.Add(node, x, 0);
                g.Flow(from, node.enter);
                from = node.exit;
                x += 300f;
                return node;
            }
            Next(new OpeningBoxComesUp());
            Next(new OpeningWhoAreYou());
            Next(new OpeningGameBegins());
            Next(new OpeningLenderSpeaks());
            Next(new OpeningTheShowroom());
            Next(new OpeningTheFirstMorning());
            Next(new OpeningWaitForTheFirstClick());
            Next(new OpeningTheMarks());
            Next(new OpeningRavnSpeaks());
            g.Note(-20, 270, 620, 190, "How it hangs together",
                "'The box comes up' takes the game in hand: the clock is held and the keys are the show's. 'The first morning' gives it back and arms the next click. " +
                "'Wait for the first click' and 'The marks' go together. Leave those in; any other part can be left out, or others put between them.");
            g.Note(640, 270, 620, 190, "Put things between",
                "A wait (Time > Wait For Seconds), a sound, a line of your own (Kapman > Text), a check on what was answered: join it between two parts. " +
                "The words themselves are in Showtime.cs for now (AskFirst, LenderLines, PlateLines, Ravn's lines): placeholders for the user's own.");
            g.Note(1300, 270, 620, 190, "What comes before",
                "The slow start and the title (the loader, the sea, the name, the menu) are still in Showtime.cs (SlowStart, TitleNow, LeaveTitle). " +
                "This graph starts when a new game is chosen there, or from the menu in a game.");
            return g.Save(Path);
        }

        [MenuItem("Kapman/Make physical/3b. The opening's graph")]
        public static void Make()
        {
            // (the scene first: opening a scene lets go of assets nothing holds, and the graph just made would be one)
            var scene = EditorSceneManager.OpenScene(KapmanPhysical.GameScene, OpenSceneMode.Single);
            var graph = Opening();
            if (graph == null)
                graph = AssetDatabase.LoadAssetAtPath<ScriptGraphAsset>(Path);
            if (graph == null)
                throw new System.InvalidOperationException(Path + " could not be made or read.");
            var show = Object.FindAnyObjectByType<Showtime>(FindObjectsInactive.Include);
            if (show == null)
                throw new System.InvalidOperationException(KapmanPhysical.GameScene + " has no Showtime (Kapman > Make physical > 1).");
            bool has = false;
            foreach (var m in show.GetComponents<ScriptMachine>())
            {
                if (m.nest.macro == null)
                    Object.DestroyImmediate(m);                           // (a machine with no graph: left by a run that lost it)
                else
                    has |= m.nest.macro == graph;
            }
            if (!has)
            {
                var machine = show.gameObject.AddComponent<ScriptMachine>();
                machine.nest.source = GraphSource.Macro;
                machine.nest.macro = graph;
                EditorUtility.SetDirty(machine);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Kapman, made physical, step 3b: the Showtime object runs " + Path + ".");
        }
    }
}
