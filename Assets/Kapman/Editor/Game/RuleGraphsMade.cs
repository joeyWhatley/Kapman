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
            var made = new List<ScriptGraphAsset>
            {
                Toll(), Pay(), Health(), Heart(), Fight(), Trick(), Instalment(), GoonsDo(),
                Leeway(), Blown(), SeaRuns(), Landfall(), ErrorKept(), Anchor(), LeeShore(), RoughRace(), RoughBar(), RoughShoal(), RoughCape(), RoughSlow(),
                LaysAgain(), SailsOn(), AsBefore(),
            };
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

        /// <summary>The sixteen rule graphs of 9 Oct 2026 (a fight, the lender's men, wind and tide) made again OVER the
        /// ones that are there, into the same assets. Only while nobody has changed them by hand: what was changed in
        /// the graph window is lost. The four of 8 Oct (the toll, and the crew's day) are not touched.
        /// Batch: -executeMethod Kapman.EditorTools.RuleGraphsMade.RemakeOfNinthOctober</summary>
        public static void RemakeOfNinthOctober()
        {
            GraphBuilder.ReplaceExisting = true;
            try
            {
                Fight();
                Trick();
                Instalment();
                GoonsDo();
                Leeway();
                Blown();
                SeaRuns();
                Landfall();
                ErrorKept();
                Anchor();
                LeeShore();
                RoughRace();
                RoughBar();
                RoughShoal();
                RoughCape();
                RoughSlow();
            }
            finally
            {
                GraphBuilder.ReplaceExisting = false;
            }
            Make();
        }

        /// <summary>The three rule graphs of 10 Oct 2026 (fewer prompts) made again OVER the ones that are there, into the
        /// same assets. Only while nobody has changed them by hand.
        /// Batch: -executeMethod Kapman.EditorTools.RuleGraphsMade.RemakeOfTenthOctober</summary>
        public static void RemakeOfTenthOctober()
        {
            GraphBuilder.ReplaceExisting = true;
            try
            {
                LaysAgain();
                SailsOn();
                AsBefore();
            }
            finally
            {
                GraphBuilder.ReplaceExisting = false;
            }
            Make();
        }

        // ------------------------------------------------------------------ fewer prompts (10 Oct 2026)

        /// <summary>Is a number of the game a yes (1) or a no (0)? The yes-or-no the graph's nodes go on with.</summary>
        static ValueOutput Yes(GraphBuilder g, ValueOutput number, float x, float y)
        {
            var yes = g.Add(new IsIt { how = IsIt.How.AtLeast }, x, y);
            g.Data(number, yes.a);
            g.Set(yes.b, 0.5);
            return yes.so;
        }

        /// <summary>Set well off her line: does the steersman lay her course again himself?</summary>
        public static ScriptGraphAsset LaysAgain()
        {
            var g = new GraphBuilder("Rule - does the steersman lay her course again himself",
                "Wind and tide have set her well off her line. Does the steersman lay her course again himself, with the clock running on, or does he tell her and leave it to her? " +
                "The game asks this graph the moment she is set that far off (GameRules.LaysAgainHimself, from Adrift.Carry).");
            g.Group(-40, -80, 2000, 520, "Does he lay her course again himself",
                "Your ruling of 10 October 2026: with the land in sight he does. Out of sight of land it is yours to lay. In thick air he cannot see the land, so there too he tells you.", GraphBuilder.Change);
            var start = g.Add(new LaysAgainRule(), 0, 0);
            var coast = Yes(g, start.land, 520, 20);
            var thick = Yes(g, start.thick, 520, 200);
            var seen = g.Add(new BothOrEither { how = BothOrEither.How.FirstAndNotSecond }, 840, 100);
            g.Data(coast, seen.one);
            g.Data(thick, seen.other);
            var does = g.Add(new ThisOrThat(), 1280, 100);
            g.Data(seen.so, does.when);
            g.Set(does.yes, 1.0);
            g.Set(does.no, 0.0);
            var answer = g.Add(new RuleAnswer(), 1600, 0);
            g.Flow(start.trigger, answer.enter);
            g.Data(does.picked, answer.number);
            g.Note(-20, 320, 620, 130, "What the game gives",
                "Land is in sight: she is on a coast's own lane, or dry land lies within as far as he makes out a landfall now (30 km by day, 12 km by night). " +
                "The air is thick: he makes out a landfall at under a kilometre (fog, heavy rain).");
            return g.Save($"{Folder}/Rule - does the steersman lay her course again himself.asset");
        }

        /// <summary>Lying to with a reckoning lost: does she sail on by herself now?</summary>
        public static ScriptGraphAsset SailsOn()
        {
            var g = new GraphBuilder("Rule - does she sail on when the air clears",
                "She lies to because the steersman lost his reckoning. Does he lay her course again by himself now, so that she sails on? " +
                "The game asks this graph each minute she lies so (GameRules.SailsOnWhenClear, from Adrift.SailsOnNowItIsClear).");
            g.Group(-40, -80, 2000, 520, "Does she sail on when the air clears",
                "Your ruling of 10 October 2026: when a fog clears she sails on by herself. A reckoning lost in clear air is not a matter of waiting: that one stays yours to lay.", GraphBuilder.Change);
            var start = g.Add(new SailsOnRule(), 0, 0);
            var lost = Yes(g, start.lost, 520, 20);
            var clear = g.Add(new IsIt { how = IsIt.How.AtLeast }, 520, 200);
            g.Data(start.sees, clear.a);
            g.Data(start.clearFrom, clear.b);
            var both = g.Add(new BothOrEither { how = BothOrEither.How.Both }, 840, 100);
            g.Data(lost, both.one);
            g.Data(clear.so, both.other);
            var does = g.Add(new ThisOrThat(), 1280, 100);
            g.Data(both.so, does.when);
            g.Set(does.yes, 1.0);
            g.Set(does.no, 0.0);
            var answer = g.Add(new RuleAnswer(), 1600, 0);
            g.Flow(start.trigger, answer.enter);
            g.Data(does.picked, answer.number);
            g.Note(-20, 320, 560, 110, "Where the numbers come from",
                "How far he sees is the graph \"Rule - how far he makes out a landfall\". The air counts as clear from a kilometre (Adrift.BlindM).");
            return g.Save($"{Folder}/Rule - does she sail on when the air clears.asset");
        }

        /// <summary>A card whose people remember her last answer: is it answered the same way, with nothing asked?</summary>
        public static ScriptGraphAsset AsBefore()
        {
            var g = new GraphBuilder("Rule - is a card answered as before",
                "A card comes whose people remember how she answered it the last time. Is it answered the same way by itself, with a line in the log, or does the card come up for her? " +
                "The game asks this graph as the card is about to open (GameRules.AsBefore, from EncounterDirector.OpenNext). Which cards, and which of their answers may stand, is in Game/Data/card_rules_793.json.");
            g.Group(-40, -80, 2440, 760, "Is a card answered as before",
                "Your ruling of 10 October 2026, of the king's toll: \"As you answered last time\". It is, when her last answer may stand; unless a better answer is open to her today, or something brings them to her in person.",
                GraphBuilder.Change);
            var start = g.Add(new AsBeforeRule(), 0, 0);
            var stands = Yes(g, start.stands, 520, 0);
            var better = Yes(g, start.better, 520, 180);
            var meets = Yes(g, start.meets, 520, 360);
            var first = g.Add(new BothOrEither { how = BothOrEither.How.FirstAndNotSecond }, 840, 80);
            g.Data(stands, first.one);
            g.Data(better, first.other);
            var second = g.Add(new BothOrEither { how = BothOrEither.How.FirstAndNotSecond }, 1280, 180);
            g.Data(first.so, second.one);
            g.Data(meets, second.other);
            var does = g.Add(new ThisOrThat(), 1720, 180);
            g.Data(second.so, does.when);
            g.Set(does.yes, 1.0);
            g.Set(does.no, 0.0);
            var answer = g.Add(new RuleAnswer(), 2040, 0);
            g.Flow(start.trigger, answer.enter);
            g.Data(does.picked, answer.number);
            g.Note(-20, 500, 620, 150, "What the game gives, for the king's toll",
                "Her last answer stands: the toll man remembers that she declared her cargo, or showed a charter, and she can do the same today. " +
                "A better answer is open: she declared the last time and holds a charter now. Something brings them: there are swords or captives aboard, which she might hide.");
            return g.Save($"{Folder}/Rule - is a card answered as before.asset");
        }

        // ------------------------------------------------------------------ wind and tide (9 Oct 2026)

        const string SeaTable = "Game/Data/sea_push.json";

        /// <summary>Her leeway under sail: the table's angle by where the wind is, more in more wind, never over the most.</summary>
        public static ScriptGraphAsset Leeway()
        {
            var g = new GraphBuilder("Rule - her leeway under sail",
                "How many degrees off her heading she is set under sail, to the side the wind blows to. The game asks this graph each minute she sails (GameRules.Leeway, from SeaPush.Wind).");
            g.Group(-40, -80, 2500, 640, "Her leeway under sail",
                "The table's angle for where the wind is: forward of 60 degrees off her bow she beats, to 120 she reaches, abaft that she runs. More in more wind and less in less, and never over the most.",
                GraphBuilder.Change);
            var start = g.Add(new LeewayRule(), 0, 0);
            var ahead = g.Add(new IsIt { how = IsIt.How.Under }, 520, 0);
            g.Data(start.off, ahead.a);
            g.Set(ahead.b, 60.0);
            var beam = g.Add(new IsIt { how = IsIt.How.Under }, 520, 150);
            g.Data(start.off, beam.a);
            g.Set(beam.b, 120.0);
            var reachOrRun = g.Add(new ThisOrThat(), 820, 200);
            g.Data(beam.so, reachOrRun.when);
            g.Data(start.reaching, reachOrRun.yes);
            g.Data(start.running, reachOrRun.no);
            var angle = g.Add(new ThisOrThat(), 1120, 60);
            g.Data(ahead.so, angle.when);
            g.Data(start.beating, angle.yes);
            g.Data(reachOrRun.picked, angle.no);
            var atLeast = g.Add(new NoLessThan(), 520, 380);
            g.Data(start.atWind, atLeast.number);
            g.Set(atLeast.least, 0.1);
            var ratio = g.Add(new DividedBy(), 820, 380);
            g.Data(start.wind, ratio.a);
            g.Data(atLeast.kept, ratio.b);
            var byWind = g.Add(new KeepBetween(), 1120, 380);
            g.Data(ratio.quotient, byWind.number);
            g.Set(byWind.least, 0.4);
            g.Set(byWind.most, 2.2);
            var times = g.Add(new Times(), 1420, 160);
            g.Data(angle.picked, times.a);
            g.Data(byWind.kept, times.b);
            var capped = g.Add(new NoMoreThan(), 1700, 160);
            g.Data(times.product, capped.number);
            g.Data(start.most, capped.most);
            var answer = g.Add(new RuleAnswer(), 2000, 0);
            g.Flow(start.trigger, answer.enter);
            g.Data(capped.kept, answer.number);
            g.Note(-20, 420, 480, 120, "Where the numbers come from",
                "leewayBeating, leewayReaching, leewayRunning, leewayWind and leewayMost in " + SeaTable + ". Which side she is set to, and how far that carries her at her speed, is the code's (SeaPush.Wind).");
            return g.Save($"{Folder}/Rule - her leeway under sail.asset");
        }

        /// <summary>Blown downwind with no sail drawing: a share of the wind's speed.</summary>
        public static ScriptGraphAsset Blown()
        {
            var g = new GraphBuilder("Rule - blown downwind with no sail",
                "How fast she is blown bodily downwind with no sail drawing, in m/s. The game asks this graph each minute she is under oars, has the sail down, or lies to (GameRules.Blown, from SeaPush.Wind).");
            g.Group(-40, -80, 1500, 420, "Blown downwind with no sail", "A share of the wind's speed: one under oars or with the sail down, another lying to.", GraphBuilder.Change);
            var start = g.Add(new BlownRule(), 0, 0);
            var share = g.Add(new ThisOrThat(), 480, 120);
            g.Data(start.lyingTo, share.when);
            g.Data(start.lyingToShare, share.yes);
            g.Data(start.bare, share.no);
            var times = g.Add(new Times(), 780, 60);
            g.Data(start.wind, times.a);
            g.Data(share.picked, times.b);
            var answer = g.Add(new RuleAnswer(), 1060, 0);
            g.Flow(start.trigger, answer.enter);
            g.Data(times.product, answer.number);
            g.Note(-20, 240, 480, 90, "Where the numbers come from", "windageBare and windageLyingTo in " + SeaTable + ".");
            return g.Save($"{Folder}/Rule - blown downwind with no sail.asset");
        }

        /// <summary>The top of the sea itself runs at a share of the wind's speed.</summary>
        public static ScriptGraphAsset SeaRuns()
        {
            var g = new GraphBuilder("Rule - the top of the sea runs",
                "How fast the top of the sea itself runs before a steady wind, in m/s. The game asks this graph each minute she is at sea (GameRules.SeaRuns, from SeaPush.Wind).");
            g.Group(-40, -80, 1200, 380, "The top of the sea runs", "A share of the wind's speed. It runs a little to the right of where the wind blows (the table's seaTurn, in degrees: the code's).", GraphBuilder.Change);
            var start = g.Add(new SeaRunsRule(), 0, 0);
            var times = g.Add(new Times(), 420, 60);
            g.Data(start.wind, times.a);
            g.Data(start.share, times.b);
            var answer = g.Add(new RuleAnswer(), 720, 0);
            g.Flow(start.trigger, answer.enter);
            g.Data(times.product, answer.number);
            g.Note(-20, 200, 480, 90, "Where the numbers come from", "seaShare (and seaTurn) in " + SeaTable + ".");
            return g.Save($"{Folder}/Rule - the top of the sea runs.asset");
        }

        /// <summary>How far the steersman makes out a landfall: less by night, no further than thick air lets him see.</summary>
        public static ScriptGraphAsset Landfall()
        {
            var g = new GraphBuilder("Rule - how far he makes out a landfall",
                "How far the steersman makes out his landfall, in real metres. The game asks this graph each minute she runs for one over open water (GameRules.Landfall, from Adrift.Sight).");
            g.Group(-40, -80, 2200, 520, "How far he makes out a landfall",
                "So far by day in clear air; a share of that by night; in thick weather no further than the air lets him see; never under 300 m.", GraphBuilder.Change);
            var start = g.Add(new LandfallRule(), 0, 0);
            var byNight = g.Add(new ThisOrThat(), 480, 120);
            g.Data(start.night, byNight.when);
            g.Data(start.nightShare, byNight.yes);
            g.Set(byNight.no, 1.0);
            var far = g.Add(new Times(), 780, 60);
            g.Data(start.far, far.a);
            g.Data(byNight.picked, far.b);
            var inAir = g.Add(new NoMoreThan(), 1060, 220);
            g.Data(far.product, inAir.number);
            g.Data(start.air, inAir.most);
            var seen = g.Add(new ThisOrThat(), 1340, 120);
            g.Data(start.thick, seen.when);
            g.Data(inAir.kept, seen.yes);
            g.Data(far.product, seen.no);
            var least = g.Add(new NoLessThan(), 1620, 120);
            g.Data(seen.picked, least.number);
            g.Set(least.least, 300.0);
            var answer = g.Add(new RuleAnswer(), 1900, 0);
            g.Flow(start.trigger, answer.enter);
            g.Data(least.kept, answer.number);
            g.Note(-20, 320, 480, 110, "Where the numbers come from",
                "landfallM and landfallNight in " + SeaTable + ". Thick weather is fog or hard rain where she is; how far the air lets him see is the weather's own.");
            return g.Save($"{Folder}/Rule - how far he makes out a landfall.asset");
        }

        /// <summary>What the steersman keeps of his error along a coast: the land tells him a share of it an hour.</summary>
        public static ScriptGraphAsset ErrorKept()
        {
            var g = new GraphBuilder("Rule - what he keeps of his error along a coast",
                "Along a coast the land tells the steersman a share of his error each hour. The answer is the share he still has after the step (1: all of it). " +
                "The game asks this graph each minute she coasts by reckoning (GameRules.ErrorKept, from Adrift.SteersBy).");
            g.Group(-40, -80, 1800, 500, "What he keeps of his error along a coast",
                "By day the land tells him so much of it an hour, by night less, in thick fog nothing. On the open sea he is told nothing at all (this is not asked there).", GraphBuilder.Change);
            var start = g.Add(new ErrorKeptRule(), 0, 0);
            var byNight = g.Add(new ThisOrThat(), 480, 160);
            g.Data(start.night, byNight.when);
            g.Data(start.eyeNight, byNight.yes);
            g.Data(start.eyeDay, byNight.no);
            var inFog = g.Add(new ThisOrThat(), 780, 120);
            g.Data(start.thick, inFog.when);
            g.Set(inFog.yes, 0.0);
            g.Data(byNight.picked, inFog.no);
            var left = g.Add(new WhatIsLeftAfter(), 1080, 80);
            g.Data(inFog.picked, left.rate);
            g.Data(start.hours, left.hours);
            var answer = g.Add(new RuleAnswer(), 1400, 0);
            g.Flow(start.trigger, answer.enter);
            g.Data(left.left, answer.number);
            g.Note(-20, 320, 480, 90, "Where the numbers come from", "coastEye and coastEyeNight in " + SeaTable + ".");
            return g.Save($"{Folder}/Rule - what he keeps of his error along a coast.asset");
        }

        /// <summary>Can she lie to her anchor: no gale, and water shallow enough.</summary>
        public static ScriptGraphAsset Anchor()
        {
            var g = new GraphBuilder("Rule - can she lie to her anchor",
                "Lying to with no course, can she lie to her anchor and not be carried? 1 she can, 0 she cannot. The game asks this graph each minute she lies to at sea (GameRules.CanAnchor, from Adrift.Carry).");
            g.Group(-40, -80, 1700, 460, "Can she lie to her anchor", "It does not blow a gale, and the water is no deeper than she can anchor in.", GraphBuilder.Change);
            var start = g.Add(new AnchorRule(), 0, 0);
            var calm = g.Add(new IsIt { how = IsIt.How.AtMost }, 480, 20);
            g.Data(start.wind, calm.a);
            g.Data(start.gale, calm.b);
            var shallow = g.Add(new IsIt { how = IsIt.How.AtMost }, 480, 180);
            g.Data(start.depth, shallow.a);
            g.Data(start.anchorDepth, shallow.b);
            var both = g.Add(new BothOrEither { how = BothOrEither.How.Both }, 780, 100);
            g.Data(calm.so, both.one);
            g.Data(shallow.so, both.other);
            var can = g.Add(new ThisOrThat(), 1060, 100);
            g.Data(both.so, can.when);
            g.Set(can.yes, 1.0);
            g.Set(can.no, 0.0);
            var answer = g.Add(new RuleAnswer(), 1340, 0);
            g.Flow(start.trigger, answer.enter);
            g.Data(can.picked, answer.number);
            g.Note(-20, 300, 480, 90, "Where the numbers come from", "anchorDepthM in " + SeaTable + "; the wind of a gale is the weather's own (GaleDice.GaleWind).");
            return g.Save($"{Folder}/Rule - can she lie to her anchor.asset");
        }

        /// <summary>What a lee shore costs her hull in a step, or nothing when she keeps clear of it.</summary>
        public static ScriptGraphAsset LeeShore()
        {
            var g = new GraphBuilder("Rule - what a lee shore costs her hull",
                "Held off a shore that wind and tide would set her on: the share of her hull it costs in this step, or nothing when she has the way on to keep clear. " +
                "The game asks this graph each minute she is held off a shore (GameRules.LeeShore, from Adrift.Carry).");
            g.Group(-40, -80, 2000, 560, "What a lee shore costs her hull",
                "It works her when she lies to, or when her own way is not so many times the push onto the shore. Then it costs so much of the hull an hour, and more in a gale.", GraphBuilder.Change);
            var start = g.Add(new LeeShoreRule(), 0, 0);
            var need = g.Add(new Times(), 480, 100);
            g.Data(start.push, need.a);
            g.Data(start.clawOff, need.b);
            var slow = g.Add(new IsIt { how = IsIt.How.Under }, 760, 60);
            g.Data(start.speed, slow.a);
            g.Data(need.product, slow.b);
            var helpless = g.Add(new BothOrEither { how = BothOrEither.How.Either }, 1040, 20);
            g.Data(start.lyingTo, helpless.one);
            g.Data(slow.so, helpless.other);
            var rate = g.Add(new ThisOrThat(), 480, 300);
            g.Data(start.gale, rate.when);
            g.Data(start.strainGale, rate.yes);
            g.Data(start.strainHour, rate.no);
            var cost = g.Add(new Times(), 780, 320);
            g.Data(rate.picked, cost.a);
            g.Data(start.hours, cost.b);
            var costs = g.Add(new ThisOrThat(), 1320, 160);
            g.Data(helpless.so, costs.when);
            g.Data(cost.product, costs.yes);
            g.Set(costs.no, 0.0);
            var answer = g.Add(new RuleAnswer(), 1620, 0);
            g.Flow(start.trigger, answer.enter);
            g.Data(costs.picked, answer.number);
            g.Note(-20, 380, 440, 90, "Where the numbers come from", "clawOff, shoreStrainHour and shoreStrainGale in " + SeaTable + ".");
            return g.Save($"{Folder}/Rule - what a lee shore costs her hull.asset");
        }

        /// <summary>How rough a tide race is now: by how hard the stream runs, worst with the wind against it.</summary>
        public static ScriptGraphAsset RoughRace()
        {
            var g = new GraphBuilder("Rule - how rough a tide race is",
                "How rough a tide race is now, from 0 (quiet) to its own roughness and a fifth over. The game asks this graph each minute she is in one (GameRules.RoughRace, from RoughWaters.Roughness).");
            g.Group(-40, -80, 2500, 620, "How rough a tide race is",
                "Its own roughness, by how hard the stream runs (nothing at slack water). A third of that at the least; more with the wind against the stream (from 2 m/s, all of it at 10); " +
                "more again in a hard wind (from 8 to 16); a fifth over at the most.", GraphBuilder.Change);
            var start = g.Add(new RoughRaceRule(), 0, 0);
            var fresh = g.Add(new ShareBetween(), 520, 140);
            g.Data(start.wind, fresh.number);
            g.Set(fresh.from, 2.0);
            g.Set(fresh.upTo, 10.0);
            var hard = g.Add(new ShareBetween(), 520, 400);
            g.Data(start.wind, hard.number);
            g.Set(hard.from, 8.0);
            g.Set(hard.upTo, 16.0);
            var against = g.Add(new Times(), 520, 280);
            g.Set(against.a, 0.65);
            g.Data(start.against, against.b);
            var againstWind = g.Add(new Times(), 820, 200);
            g.Data(against.product, againstWind.a);
            g.Data(fresh.share, againstWind.b);
            var atLeast = g.Add(new AddIf(), 1100, 160);
            g.Set(atLeast.to, 0.35);
            g.Data(againstWind.product, atLeast.amount);
            var hardWind = g.Add(new Times(), 820, 400);
            g.Set(hardWind.a, 0.2);
            g.Data(hard.share, hardWind.b);
            var sum = g.Add(new AddIf(), 1380, 220);
            g.Data(atLeast.sum, sum.to);
            g.Data(hardWind.product, sum.amount);
            var capped = g.Add(new NoMoreThan(), 1660, 220);
            g.Data(sum.sum, capped.number);
            g.Set(capped.most, 1.2);
            var running = g.Add(new Times(), 1380, 40);
            g.Data(start.rough, running.a);
            g.Data(start.run, running.b);
            var rough = g.Add(new Times(), 1940, 100);
            g.Data(running.product, rough.a);
            g.Data(capped.kept, rough.b);
            var answer = g.Add(new RuleAnswer(), 2200, 0);
            g.Flow(start.trigger, answer.enter);
            g.Data(rough.product, answer.number);
            g.Note(-20, 300, 480, 150, "Where the numbers come from",
                "The water's own roughness is in Game/Data/rough_waters_793.json. How hard the stream runs is the tide model's, against its own hardest there in a fortnight. " +
                "The shares on the nodes (0.35, 0.65, 0.2; 2 to 10, 8 to 16; 1.2) are this rule's own: change them here.");
            return g.Save($"{Folder}/Rule - how rough a tide race is.asset");
        }

        /// <summary>How rough a river's bar is now: by the wind, by how squarely it blows on to the bar, worse while the tide runs.</summary>
        public static ScriptGraphAsset RoughBar()
        {
            var g = new GraphBuilder("Rule - how rough a river's bar is",
                "How rough the bar of a river mouth is now, from 0 (quiet) to its own roughness. The game asks this graph each minute she is on one (GameRules.RoughBar, from RoughWaters.Roughness).");
            g.Group(-40, -80, 2300, 560, "How rough a river's bar is",
                "Its own roughness, by the wind (from 3 m/s, all of it at 12), by how squarely the wind blows on to it (a quarter with none of it, all with it square on), and worse while the tide runs.",
                GraphBuilder.Change);
            var start = g.Add(new RoughBarRule(), 0, 0);
            var fresh = g.Add(new ShareBetween(), 520, 100);
            g.Data(start.wind, fresh.number);
            g.Set(fresh.from, 3.0);
            g.Set(fresh.upTo, 12.0);
            var byWind = g.Add(new Times(), 820, 40);
            g.Data(start.rough, byWind.a);
            g.Data(fresh.share, byWind.b);
            var on = g.Add(new Times(), 520, 260);
            g.Set(on.a, 0.75);
            g.Data(start.onshore, on.b);
            var onTo = g.Add(new AddIf(), 820, 240);
            g.Set(onTo.to, 0.25);
            g.Data(on.product, onTo.amount);
            var byShore = g.Add(new Times(), 1120, 120);
            g.Data(byWind.product, byShore.a);
            g.Data(onTo.sum, byShore.b);
            var tide = g.Add(new Times(), 820, 400);
            g.Set(tide.a, 0.4);
            g.Data(start.run, tide.b);
            var byTide = g.Add(new AddIf(), 1120, 360);
            g.Set(byTide.to, 0.6);
            g.Data(tide.product, byTide.amount);
            var rough = g.Add(new Times(), 1420, 200);
            g.Data(byShore.product, rough.a);
            g.Data(byTide.sum, rough.b);
            var answer = g.Add(new RuleAnswer(), 1720, 0);
            g.Flow(start.trigger, answer.enter);
            g.Data(rough.product, answer.number);
            g.Note(-20, 300, 460, 130, "Where the numbers come from",
                "The water's own roughness, and the bearing its bar lies open to, are in Game/Data/rough_waters_793.json. The shares on the nodes are this rule's own: change them here.");
            return g.Save($"{Folder}/Rule - how rough a river's bar is.asset");
        }

        /// <summary>How rough a shoal (or an open cape) is now: by the wind, a little worse while the tide runs.</summary>
        static ScriptGraphAsset RoughInWind<T>(string title, string name, T start, System.Func<T, (ValueOutput rough, ValueOutput run, ValueOutput wind)> given,
                                               double from, double upTo, double atSlack, double byTide, string about, string asked) where T : RuleUnit
        {
            var g = new GraphBuilder(title, about + " The game asks this graph each minute she is in one (" + asked + ", from RoughWaters.Roughness).");
            g.Group(-40, -80, 1900, 460, name, "Its own roughness, by the wind (from " + from + " m/s, all of it at " + upTo + "), and by the tide: " + atSlack + " of it at slack water, all of it with the stream at its hardest.",
                GraphBuilder.Change);
            g.Add(start, 0, 0);
            // (a node's ports are made when it is put into a graph: they are asked for after that)
            var (roughOf, runOf, windOf) = given(start);
            var fresh = g.Add(new ShareBetween(), 520, 100);
            g.Data(windOf, fresh.number);
            g.Set(fresh.from, from);
            g.Set(fresh.upTo, upTo);
            var byWind = g.Add(new Times(), 820, 40);
            g.Data(roughOf, byWind.a);
            g.Data(fresh.share, byWind.b);
            var tide = g.Add(new Times(), 520, 260);
            g.Set(tide.a, byTide);
            g.Data(runOf, tide.b);
            var withTide = g.Add(new AddIf(), 820, 240);
            g.Set(withTide.to, atSlack);
            g.Data(tide.product, withTide.amount);
            var rough = g.Add(new Times(), 1120, 120);
            g.Data(byWind.product, rough.a);
            g.Data(withTide.sum, rough.b);
            var answer = g.Add(new RuleAnswer(), 1420, 0);
            g.Flow(start.trigger, answer.enter);
            g.Data(rough.product, answer.number);
            g.Note(-20, 260, 460, 110, "Where the numbers come from",
                "The water's own roughness is in Game/Data/rough_waters_793.json. The shares on the nodes are this rule's own: change them here.");
            return g.Save($"{Folder}/{title}.asset");
        }

        public static ScriptGraphAsset RoughShoal()
        {
            return RoughInWind("Rule - how rough a shoal is", "How rough a shoal is", new RoughShoalRule(), s => (s.rough, s.run, s.wind), 4.0, 13.0, 0.7, 0.3,
                               "How rough shoal water is now, from 0 (quiet) to its own roughness: the sea breaks on it in any fresh wind.", "GameRules.RoughShoal");
        }

        public static ScriptGraphAsset RoughCape()
        {
            return RoughInWind("Rule - how rough an open cape is", "How rough an open cape is", new RoughCapeRule(), s => (s.rough, s.run, s.wind), 5.0, 14.0, 0.6, 0.4,
                               "How rough the water off a cape with no shelter is now, from 0 (quiet) to its own roughness: a steep confused sea in a fresh wind.", "GameRules.RoughCape");
        }

        /// <summary>The share of her speed she keeps in rough water.</summary>
        public static ScriptGraphAsset RoughSlow()
        {
            var g = new GraphBuilder("Rule - her speed in rough water",
                "The share of her speed she keeps in rough water (1: all of it). The game asks this graph each step she makes way in one (GameRules.RoughSlow, from RoughWaters.Slow).");
            g.Group(-40, -80, 1900, 440, "Her speed in rough water", "All of it in quiet water; where the roughness is 1 or more, the table's share; between, between.", GraphBuilder.Change);
            var start = g.Add(new RoughSlowRule(), 0, 0);
            var lost = g.Add(new TakeOffIf(), 480, 180);
            g.Set(lost.from, 1.0);
            g.Data(start.kept, lost.amount);
            var most = g.Add(new NoMoreThan(), 480, 40);
            g.Data(start.rough, most.number);
            g.Set(most.most, 1.0);
            var off = g.Add(new Times(), 780, 120);
            g.Data(lost.left, off.a);
            g.Data(most.kept, off.b);
            var keeps = g.Add(new TakeOffIf(), 1060, 120);
            g.Set(keeps.from, 1.0);
            g.Data(off.product, keeps.amount);
            var answer = g.Add(new RuleAnswer(), 1340, 0);
            g.Flow(start.trigger, answer.enter);
            g.Data(keeps.left, answer.number);
            g.Note(-20, 260, 440, 90, "Where the numbers come from", "roughSlow in " + SeaTable + "; how rough it is now is the four rules of the rough waters.");
            return g.Save($"{Folder}/Rule - her speed in rough water.asset");
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
            var worth = g.Add(new Times(), 980, 220);
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
            var used = g.Add(new NoLessThan(), 420, 160);
            g.Data(start.usedBefore, used.number);
            g.Set(used.least, 0.0);
            var told = g.Add(new Times(), 680, 200);
            g.Data(start.toldPenalty, told.a);
            g.Data(used.kept, told.b);
            var afterTold = g.Add(new TakeOffIf(), 920, 100);
            g.Data(start.stat, afterTold.from);
            g.Data(told.product, afterTold.amount);
            var sendings = g.Add(new TakeOffIf(), 420, 340);
            g.Data(start.visit, sendings.from);
            g.Set(sendings.amount, 1.0);
            var later = g.Add(new NoLessThan(), 680, 340);
            g.Data(sendings.left, later.number);
            g.Set(later.least, 0.0);
            var laterCost = g.Add(new Times(), 940, 340);
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
            var steps = g.Add(new NoLessThan(), 680, 160);
            g.Data(over.left, steps.number);
            g.Set(steps.least, 0.0);
            var more = g.Add(new Times(), 940, 200);
            g.Data(start.moreByTemper, more.a);
            g.Data(steps.kept, more.b);
            var share = g.Add(new AddIf(), 1180, 200);
            g.Set(share.to, 1.0);
            g.Data(more.product, share.amount);
            var sum = g.Add(new Times(), 1440, 120);
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
            var half = g.Add(new Times(), 520, 660);
            g.Data(start.want, half.a);
            g.Set(half.b, 0.5);
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
