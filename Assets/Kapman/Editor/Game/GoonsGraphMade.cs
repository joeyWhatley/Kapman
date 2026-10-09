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
    /// THE GRAPH OF THE LENDER'S MEN, as first made, and its Script Machine on the '--- GAME ---' object of the Game
    /// scene (where the game's other graphs are). As with those: once made it is the user's, to open and change; this
    /// only makes it if it is missing.
    /// Menu: Kapman > Make physical > 3d. The graph of the lender's men.   Batch: -executeMethod Kapman.EditorTools.GoonsGraphMade.Make
    /// </summary>
    public static class GoonsGraphMade
    {
        public const string Path = GameGraphs.Folder + "/Encounter - the lender's men.asset";
        public const string GameObjectName = "--- GAME ---";

        /// <summary>The answers she can give them, in the order of the graph's rows.</summary>
        public static readonly string[] Answers = { "paid", "paid_well", "done", "cargo", "astray", "drunk", "poured", "shaken", "routed", "fooled", "nothing" };

        public static ScriptGraphAsset LendersMen()
        {
            var g = new GraphBuilder("Encounter - the lender's men",
                "The lender sends his men to collect, a handful of times in the life of the debt (Code/Game/Voyage/Collectors.cs; Data/collectors_793.json). " +
                "Four moments of theirs, each an event here, and what happens at each: his day to send comes; they catch her; she answers them on their card; they give up. " +
                "Their boat itself (sailing the lanes, asking round a quay, seeing her, rowing for her) is in the code. Their words are in the cards (Data/encounters/wokerer_793.json).");

            // ---- 1. his day to send
            g.Group(-40, -90, 1500, 400, "1. The lender's day to send comes",
                "Four times in the life of the debt. If she has paid a pound or more on the stick since the last time, he has had something of his own and sends nobody; else a boat of his men sets out.",
                GraphBuilder.Watch);
            var due = g.Add(new OnTheLendersDayToSend(), 0, 0);
            var paidLately = g.Add(new If(), 420, 0);
            g.Flow(due.trigger, paidLately.enter);
            g.Data(due.paidLately, paidLately.condition);
            var coolsNoSend = g.Add(new LenderCools(), 700, -20);
            g.Flow(paidLately.ifTrue, coolsNoSend.enter);
            g.Set(coolsNoSend.by, 1.0);
            var setOut = g.Add(new LendersMenSetOut(), 700, 130);
            g.Flow(paidLately.ifFalse, setOut.enter);
            g.Note(1000, -40, 420, 260, "The days",
                "The days they set out, how many men, and how many days they stay out are the table's (collectors_793.json, \"sendings\"). " +
                "\"Lately\" is a pound on the stick (paidLatelyD) since the last sending or the last time she paid them.");

            // ---- 2. they catch her
            float y2 = 420f;
            g.Group(-40, y2 - 90, 1500, 300, "2. They catch her",
                "Made fast at a quay they have come to, or alongside at sea after a chase. Their card comes: the quay's (harbour.goons.quay) or the sea's (sea.goons.alongside).", GraphBuilder.Talk);
            var caught = g.Add(new OnTheLendersMenCatchHer(), 0, y2);
            var card = g.Add(new LendersMenTheirCardComes(), 420, y2);
            g.Flow(caught.trigger, card.enter);
            g.Note(760, y2 - 40, 660, 180, "Their card",
                "What they say and what she can answer is the card's (Game/Data/encounters/wokerer_793.json; the words in Game/Data/writing/wokerer.json). " +
                "Each answer ends in one of the answers of section 3. Take this part out and they stand by her and say nothing.");

            // ---- 3. she answers them
            float y3 = 800f, row = 150f;
            float x0 = 800f, dx = 440f;
            g.Group(-40, y3 - 90, x0 + dx * 8 + 420, row * Answers.Length + 520, "3. She answers them",
                "What each answer does, a row to an answer. The lender cools when she pays and is the angrier when they come home with nothing or fooled; a way round them is counted, and is harder the next time.",
                GraphBuilder.Change);
            var answered = g.Add(new OnTheLendersMenAreAnswered(), 0, y3);
            var which = g.Add(new SwitchOnString { options = new List<string>(Answers) }, 420, y3);
            g.Flow(answered.trigger, which.enter);
            g.Data(answered.answer, which.selector);
            ControlOutput Of(string answer) => which.branches[System.Array.IndexOf(Answers, answer)].Value;
            float Y(string answer) => y3 + System.Array.IndexOf(Answers, answer) * row;

            // paid: a pound on the stick
            {
                float y = Y("paid");
                var cools = g.Add(new LenderCools(), x0, y);
                g.Flow(Of("paid"), cools.enter);
                g.Set(cools.by, 1.0);
                var paid = g.Add(new LendersMenSheHasPaid(), x0 + dx, y);
                g.Flow(cools.exit, paid.enter);
                g.Set(paid.visits, 1);
                var home = g.Add(new LendersMenGoHome(), x0 + dx * 2, y);
                g.Flow(paid.exit, home.enter);
            }
            // paid_well: two pounds or more
            {
                float y = Y("paid_well");
                var cools = g.Add(new LenderCools(), x0, y);
                g.Flow(Of("paid_well"), cools.enter);
                g.Set(cools.by, 2.0);
                var paid = g.Add(new LendersMenSheHasPaid(), x0 + dx, y);
                g.Flow(cools.exit, paid.enter);
                g.Set(paid.visits, 2);
                var home = g.Add(new LendersMenGoHome(), x0 + dx * 2, y);
                g.Flow(paid.exit, home.enter);
            }
            // done: all of it
            {
                float y = Y("done");
                var all = g.Add(new LenderHasAllOfIt(), x0, y);
                g.Flow(Of("done"), all.enter);
                var off = g.Add(new LendersMenCalledOff(), x0 + dx, y);
                g.Flow(all.exit, off.enter);
            }
            // cargo for an instalment
            {
                float y = Y("cargo");
                var cargo = g.Add(new LendersMenCargoForAnInstalment(), x0, y);
                g.Flow(Of("cargo"), cargo.enter);
                var home = g.Add(new LendersMenGoHome(), x0 + dx, y);
                g.Flow(cargo.exit, home.enter);
            }
            // astray: the lie worked
            {
                float y = Y("astray");
                var worked = g.Add(new LendersMenAWayRoundThemWorked(), x0, y);
                g.Flow(Of("astray"), worked.enter);
                g.Set(worked.trick, "lie");
                var away = g.Add(new LendersMenSentTheWrongWay(), x0 + dx, y);
                g.Flow(worked.exit, away.enter);
                var angrier = g.Add(new LenderIsTheAngrier(), x0 + dx * 2, y + 30);
                g.Flow(away.nowhere, angrier.enter);
                g.Set(angrier.by, 1.0);
                var home = g.Add(new LendersMenGoHome(), x0 + dx * 3, y + 30);
                g.Flow(angrier.exit, home.enter);
            }
            // drunk: the drink worked
            {
                float y = Y("drunk");
                var cask = g.Add(new LendersMenACaskIsGone(), x0, y);
                g.Flow(Of("drunk"), cask.enter);
                var worked = g.Add(new LendersMenAWayRoundThemWorked(), x0 + dx, y);
                g.Flow(cask.exit, worked.enter);
                g.Set(worked.trick, "drink");
                var sleep = g.Add(new LendersMenSleep(), x0 + dx * 2, y);
                g.Flow(worked.exit, sleep.enter);
            }
            // poured: the drink was wasted (what they do then is the card's next answer: nothing, after a way failed)
            {
                float y = Y("poured");
                var cask = g.Add(new LendersMenACaskIsGone(), x0, y);
                g.Flow(Of("poured"), cask.enter);
            }
            // shaken: she ran from them at sea
            {
                float y = Y("shaken");
                var worked = g.Add(new LendersMenAWayRoundThemWorked(), x0, y);
                g.Flow(Of("shaken"), worked.enter);
                g.Set(worked.trick, "run");
                var lose = g.Add(new LendersMenLoseHer(), x0 + dx, y);
                g.Flow(worked.exit, lose.enter);
            }
            // routed: she has killed the one who killed Ravn
            {
                float y = Y("routed");
                var angrier = g.Add(new LenderIsTheAngrier(), x0, y);
                g.Flow(Of("routed"), angrier.enter);
                g.Set(angrier.by, 1.0);
                var home = g.Add(new LendersMenGoHome(), x0 + dx, y);
                g.Flow(angrier.exit, home.enter);
            }
            // fooled: another way round them worked (hide the silver, plead)
            {
                float y = Y("fooled");
                var worked = g.Add(new LendersMenAWayRoundThemWorked(), x0, y);
                g.Flow(Of("fooled"), worked.enter);
                g.Data(answered.trick, worked.trick);
                var angrier = g.Add(new LenderIsTheAngrier(), x0 + dx, y);
                g.Flow(worked.exit, angrier.enter);
                g.Set(angrier.by, 1.0);
                var home = g.Add(new LendersMenGoHome(), x0 + dx * 2, y);
                g.Flow(angrier.exit, home.enter);
            }
            // nothing: what they do depends
            {
                float y = Y("nothing");
                g.Group(x0 - 40, y - 70, dx * 8 + 400, 560, "4. Given nothing, what they do depends",
                    "On the lender's temper, on what there is aboard to take, on whether she has just made fools of them, and on how often she has paid nothing (the two rule graphs). " +
                    "They only talk; or take; or take and beat a man; or take and kill one; and the third time she pays nothing, it is Ravn.", GraphBuilder.Change);
                var given = g.Add(new LendersMenGivenNothing(), x0, y);
                g.Flow(Of("nothing"), given.enter);
                g.Data(answered.angry, given.angry);
                var does = g.Add(new SwitchOnInteger { options = new List<int> { 0, 1, 2, 3, 4 } }, x0 + dx, y);
                g.Flow(given.exit, does.enter);
                g.Data(given.does, does.selector);
                // 0: they only talk
                var talk = g.Add(new LendersMenOnlyTalk(), x0 + dx * 2, y - 20);
                g.Flow(does.branches[0].Value, talk.enter);
                var talkAngrier = g.Add(new LenderIsTheAngrier(), x0 + dx * 3, y - 20);
                g.Flow(talk.exit, talkAngrier.enter);
                g.Set(talkAngrier.by, 1.0);
                var talkHome = g.Add(new LendersMenGoHome(), x0 + dx * 4, y - 20);
                g.Flow(talkAngrier.exit, talkHome.enter);
                // 1 to 4: they take what they came for
                var take = g.Add(new LendersMenTake(), x0 + dx * 2, y + 190);
                for (int i = 1; i <= 4; i++)
                    g.Flow(does.branches[i].Value, take.enter);
                var man = g.Add(new SwitchOnInteger { options = new List<int> { 2, 3, 4 } }, x0 + dx * 3 + 60, y + 190);
                g.Flow(take.exit, man.enter);
                g.Data(given.does, man.selector);
                var beat = g.Add(new LendersMenBeatAMan(), x0 + dx * 4, y + 110);
                g.Flow(man.branches[0].Value, beat.enter);
                var kill = g.Add(new LendersMenKillAMan(), x0 + dx * 4, y + 230);
                g.Flow(man.branches[1].Value, kill.enter);
                var ravn = g.Add(new LendersMenKillRavn(), x0 + dx * 4, y + 350);
                g.Flow(man.branches[2].Value, ravn.enter);
                // then: home, and the lender the angrier if they got nothing
                var gotNothing = g.Add(new If(), x0 + dx * 5, y + 190);
                g.Flow(man.@default, gotNothing.enter);
                g.Flow(beat.exit, gotNothing.enter);
                g.Flow(kill.exit, gotNothing.enter);
                g.Flow(ravn.exit, gotNothing.enter);
                g.Data(take.gotNothing, gotNothing.condition);
                var emptyAngrier = g.Add(new LenderIsTheAngrier(), x0 + dx * 6 - 120, y + 110);
                g.Flow(gotNothing.ifTrue, emptyAngrier.enter);
                g.Set(emptyAngrier.by, 1.0);
                var home = g.Add(new LendersMenGoHome(), x0 + dx * 7 - 140, y + 190);
                g.Flow(emptyAngrier.exit, home.enter);
                g.Flow(gotNothing.ifFalse, home.enter);
                g.Note(x0, y + 270, 760, 190, "The two rules",
                    "What they come for is \"Rule - what the lender's men come for\" (an instalment, more as his temper climbs). " +
                    "What they do is \"Rule - what the lender's men do\": 0 they only talk, 1 they take, 2 and beat a man, 3 and kill a man, 4 and kill Ravn. " +
                    "Both are in Game/Graphs/Rules. Ravn's death then has a scene of its own: the graph \"Scene - Ravn is killed\".");
            }
            g.Note(0, y3 + 160, 380, 330, "The answers",
                "paid: a pound on the stick.\npaid_well: two pounds or more.\ndone: all of it.\ncargo: cargo for an instalment.\nastray: the lie worked.\n" +
                "drunk: the drink worked.\npoured: the drink was wasted.\nshaken: she ran, at sea.\nrouted: she killed the one who killed Ravn.\n" +
                "fooled: another way round them worked.\nnothing: she gives them nothing.");
            g.Note(0, y3 + 520, 380, 250, "What is said",
                "What a part has to say of what happened (\"They carry off two casks of wine ...\") is put into the card's end, in the order the parts run. " +
                "PLACEHOLDER WORDS, in Collectors.cs: the user's to write.");

            // ---- 5. they give up
            float y5 = y3 + row * Answers.Length + 560f;
            g.Group(-40, y5 - 90, 1500, 300, "5. They give up",
                "Their days are out, or no quay knows where she is, or they were sent the wrong way and have found it out. They go home with nothing (that is theirs), and the lender is the angrier.",
                GraphBuilder.Watch);
            var gaveUp = g.Add(new OnTheLendersMenGiveUp(), 0, y5);
            var upAngrier = g.Add(new LenderIsTheAngrier(), 420, y5);
            g.Flow(gaveUp.trigger, upAngrier.enter);
            g.Set(upAngrier.by, 1.0);
            return g.Save(Path);
        }

        /// <summary>Makes the graph again OVER the one that is there (into the same asset, so the machine that runs it keeps
        /// it). Only for a graph nobody has changed by hand: what was changed in the graph window is lost.
        /// Batch: -executeMethod Kapman.EditorTools.GoonsGraphMade.Remake</summary>
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

        [MenuItem("Kapman/Make physical/3d. The graph of the lender's men")]
        public static void Make()
        {
            // (the scene first: opening a scene lets go of assets nothing holds, and the graph just made would be one)
            var scene = EditorSceneManager.OpenScene(KapmanPhysical.GameScene, OpenSceneMode.Single);
            var graph = LendersMen();
            if (graph == null)
                graph = AssetDatabase.LoadAssetAtPath<ScriptGraphAsset>(Path);
            if (graph == null)
                throw new System.InvalidOperationException(Path + " could not be made or read.");
            var game = GameObject.Find(GameObjectName);
            if (game == null)
                throw new System.InvalidOperationException(KapmanPhysical.GameScene + " has no object named " + GameObjectName + " (the one that runs the game's graphs).");
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
            Debug.Log("Kapman, made physical, step 3d: the object " + GameObjectName + " runs " + Path + ".");
        }
    }
}
