using Kapman.Game;
using Kapman.Graphs;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kapman.EditorTools
{
    /// <summary>
    /// THE GRAPH OF WHAT THE SEA TELLS HER, as first made, and its Script Machine on the '--- GAME ---' object of the
    /// Game scene. As with the game's other graphs: once made it is the user's, to open and change; this only makes it
    /// if it is missing.
    /// Menu: Kapman > Make physical > 3f. The graph of the sea's moments.   Batch: -executeMethod Kapman.EditorTools.SeaGraphMade.Make
    /// </summary>
    public static class SeaGraphMade
    {
        public const string Path = GameGraphs.Folder + "/Voyage - the sea moves her.asset";

        public static ScriptGraphAsset Sea()
        {
            var g = new GraphBuilder("Voyage - the sea moves her",
                "Wind and tide carry her besides her own way (Code/Game/Voyage/Adrift.cs, SeaPush.cs, RoughWaters.cs; Data/sea_push.json, rough_waters_793.json). " +
                "When that does something she should know of, it comes here: the log says it, and the clock drops back where the moment calls for it. " +
                "How much the sea moves her is the rule graphs' (Game/Graphs/Rules: her leeway, blown downwind, a lee shore, the rough waters ...).");
            g.Group(-40, -90, 2100, 420, "The sea moves her",
                "Five moments: she is set well off her line; she is held off a lee shore; lying to in deep water, she is carried; the steersman has lost his reckoning; she comes into rough water.",
                GraphBuilder.Talk);
            var on = g.Add(new OnTheSeaMovesHer(), 0, 0);
            var say = g.Add(new LogSay(), 460, 0);
            g.Flow(on.trigger, say.enter);
            g.Data(on.words, say.words);
            var drops = g.Add(new If(), 860, 0);
            g.Flow(say.exit, drops.enter);
            g.Data(on.drops, drops.condition);
            var drop = g.Add(new ClockDropBack(), 1200, 0);
            g.Flow(drops.ifTrue, drop.enter);
            g.Data(on.why, drop.why);
            g.Note(-20, 200, 560, 260, "The moments",
                "off_line: wind and tide have set her well off her line (the table's offLineWarnM).\nlee_shore: she is set down on a shore she has not the way on to keep clear of.\n" +
                "adrift: lying to in water too deep to anchor in, she goes where wind and tide carry her.\nlost: the steersman has run his distance and cannot make out his landfall; she lies to.\n" +
                "rough: she comes into rough water (a race, a bar, a shoal, an open cape).");
            g.Note(580, 200, 560, 260, "The words, and the clock",
                "The words come with the event (\"Wind and tide have set her 9 km to the north-east of her line.\"): PLACEHOLDER WORDS, in Adrift.cs and RoughWaters.cs, the user's to write. " +
                "The clock drops back for all but 'adrift'; each reason has its own switch in the settings. To treat one moment otherwise, put a Switch on 'What' after the event.");
            g.Note(1180, 200, 560, 260, "What is not here",
                "What moves her and how much: the tide's stream (a model of the real tides), her leeway, the wind's drift, the steersman's reckoning, keeping her off the land. " +
                "Their numbers are rule graphs (Game/Graphs/Rules) and the table (Game/Data/sea_push.json); the rest is the code's (Adrift.Carry, SeaPush).");
            return g.Save(Path);
        }

        [MenuItem("Kapman/Make physical/3f. The graph of the sea's moments")]
        public static void Make()
        {
            // (the scene first: opening a scene lets go of assets nothing holds, and the graph just made would be one)
            var scene = EditorSceneManager.OpenScene(KapmanPhysical.GameScene, OpenSceneMode.Single);
            var graph = Sea();
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
            Debug.Log("Kapman, made physical, step 3f: the object " + GoonsGraphMade.GameObjectName + " runs " + Path + ".");
        }
    }
}
