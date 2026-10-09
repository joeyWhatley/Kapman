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
    /// THE GRAPH OF RAVN'S SCENE, as first made (from the table's staging: feud_rules_793.json, "scene"), and its Script
    /// Machine on the Showtime object of the Game scene. As with the game's other graphs: once made it is the user's,
    /// to open and change; this only makes it if it is missing.
    /// Menu: Kapman > Make physical > 3c. The graph of Ravn's scene.   Batch: -executeMethod Kapman.EditorTools.SceneGraphMade.Make
    /// </summary>
    public static class SceneGraphMade
    {
        public const string Path = GameGraphs.Folder + "/Scene - Ravn is killed.asset";

        public static ScriptGraphAsset RavnIsKilled()
        {
            var beats = Feud.Rules.scene;
            var g = new GraphBuilder("Scene - Ravn is killed",
                "The lender's men have killed Ravn and their card is shut: his scene, part by part and line by line, left to right. " +
                "Each node is a part of the show (Code/Game/UI/Showtime.cs); the words of each line are in the writing, by the line's key.");
            const float step = 340f, rowHeight = 420f;
            // (two rows: up to the hit and its own line; then what follows)
            int turn = beats.FindIndex(b => b.hit);
            turn = turn >= 0 ? turn + 1 : (beats.Count + 1) / 2;
            float x = 0f, y = 0f;
            ControlOutput from;
            var on = g.Add(new OnRavnIsKilled { coroutine = true }, x, y);
            from = on.trigger;
            x += 300f;
            var up = g.Add(new SceneThePurpleComesUp(), x, y);
            g.Flow(from, up.enter);
            from = up.exit;
            x += step;
            float widest = x;
            for (int i = 0; i < beats.Count; i++)
            {
                if (i == turn)
                {
                    widest = Mathf.Max(widest, x);
                    x = 300f;
                    y += rowHeight;
                }
                var beat = beats[i];
                if (beat.hit)
                {
                    var struck = g.Add(new SceneTheScreenIsStruck(), x, y);
                    g.Flow(from, struck.enter);
                    from = struck.exit;
                    x += 280f;
                }
                var line = g.Add(new SceneSayALine(), x, y);
                g.Flow(from, line.enter);
                g.Set(line.who, beat.who ?? "");
                g.Set(line.key, beat.key ?? "");
                g.Set(line.breaksOff, beat.cut);
                from = line.exit;
                x += step;
            }
            var card = g.Add(new SceneTheNamesCard(), x, y);
            g.Flow(from, card.enter);
            g.Set(card.big, "Ravn");
            g.Data(on.plate, card.small);
            x += step;
            var lifts = g.Add(new SceneThePurpleLifts(), x, y);
            g.Flow(card.exit, lifts.enter);
            x += step;
            var after = g.Add(new RavnWhatFollowsHisDeath(), x, y);
            g.Flow(lifts.exit, after.enter);
            x += step;
            widest = Mathf.Max(widest, x);
            g.Group(-40, -90, widest + 60, rowHeight - 60, "1. They come the third time",
                "The purple comes up; the lender's men say what they have come for; Ravn begins a riddle and is cut off; the screen is struck, and the teller says what was done.", GraphBuilder.Talk);
            g.Group(-40, rowHeight - 90, widest + 60, rowHeight - 60, "2. After the knife",
                "Those who are left speak; his name and where he died; the purple lifts on the game; and what follows: she stands over him.", GraphBuilder.Talk);
            float notes = rowHeight * 2f - 110f;
            g.Note(-20, notes, 620, 210, "How it hangs together",
                "'The purple comes up' takes the game in hand: the clock is held and the keys are the show's. 'The purple lifts' gives it back. Leave those two in " +
                "(a scene left standing with nothing running is lifted by the show itself after a while, and the Console says so). " +
                "'What follows his death' asks her what is to be done: left out, it follows by itself a moment later.");
            g.Note(640, notes, 620, 210, "The lines",
                "Each 'say a line' has WHO speaks (one of the scene's cast: brute, man, ravn, aldger; nothing for the teller, who has no face) and the KEY of its words. " +
                "The words are in Game/Data/writing/wokerer.json (feud.scene.1 ...): placeholders for the user's own. " +
                "At sea the words of the key with .sea after it stand in, where they are written. 'Breaks off': said to its last letter, and nobody is waited for.");
            g.Note(1300, notes, 620, 210, "The hit",
                "'The screen is struck' is the red filter and the shaken picture, in hard steps: two flashes and no more (three or more in a second is what harms some eyes). " +
                "The line after it comes whole at once, under the red. Move it before another line, or take it out.");
            g.Note(1960, notes, 620, 210, "Put things between; and without this graph",
                "A wait (Time > Wait For Seconds), a sound, a line of your own: join it between two parts. " +
                "Where no graph listens for his death (a batch run, a scene without this machine) the same parts are run from code, staged by the table " +
                "(Game/Data/feud_rules_793.json, \"scene\"): keep the two alike.");
            return g.Save(Path);
        }

        [MenuItem("Kapman/Make physical/3c. The graph of Ravn's scene")]
        public static void Make()
        {
            // (the scene first: opening a scene lets go of assets nothing holds, and the graph just made would be one)
            var scene = EditorSceneManager.OpenScene(KapmanPhysical.GameScene, OpenSceneMode.Single);
            var graph = RavnIsKilled();
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
            Debug.Log("Kapman, made physical, step 3c: the Showtime object runs " + Path + ".");
        }

        /// <summary>The scene as a graph stages it: its lines in the order the flow comes to them (who, the words' key,
        /// whether it breaks off, whether the screen is struck as it comes), read from the graph by walking the flow
        /// from "On Ravn is killed". For a test, and for whoever wants the table brought in line with the graph.</summary>
        public static List<SceneBeat> Staged(ScriptGraphAsset asset, out List<string> parts)
        {
            var beats = new List<SceneBeat>();
            parts = new List<string>();
            if (asset == null || asset.graph == null)
                return beats;
            IUnit at = null;
            foreach (var unit in asset.graph.units)
                if (unit is OnRavnIsKilled)
                    at = unit;
            bool struck = false;
            var seen = new HashSet<IUnit>();
            while (at != null && seen.Add(at))
            {
                parts.Add(at.GetType().Name);
                if (at is SceneTheScreenIsStruck)
                    struck = true;
                else if (at is SceneSayALine line)
                {
                    beats.Add(new SceneBeat { who = Default(line, "who") as string ?? "", key = Default(line, "key") as string ?? "", cut = Default(line, "breaksOff") is bool b && b, hit = struck });
                    struck = false;
                }
                IUnit next = null;
                foreach (var output in at.controlOutputs)
                    if (output.hasValidConnection)
                    {
                        next = output.connection.destination.unit;
                        break;
                    }
                at = next;
            }
            return beats;
        }

        static object Default(IUnit unit, string key) => unit.defaultValues.TryGetValue(key, out var value) ? value : null;
    }
}
