using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kapman.Game;
using Kapman.Game.Frame;
using Kapman.Game.Sim;
using Kapman.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Kapman.Tests
{
    /// <summary>
    /// A LOOK, NOT A TEST OF THE SUITE: a day and a half on the open sea as the sea is, with the wind and the tide of
    /// the day (no sea of a test's own making). She is put on the open water of the way from Dorestat to Lundenwic and
    /// sails; whenever the log says she is set off her line, or the reckoning is lost, her course is laid again from
    /// where she is, as a player would. What it writes (Captures/sea/sea_day.txt) is for tuning the numbers of
    /// Game/Data/sea_push.json: each hour her place, how the steersman knows it, how far she is set and off her line,
    /// the tide's stream, the wind's drift, her speed; and how often she had to be set right.
    /// </summary>
    public class SeaLookTests
    {
        [UnityTest, Timeout(1500000)]
        public IEnumerator ADayAndAHalfOnTheOpenSeaAsTheSeaIs()
        {
            ChartPlay play = null;
            yield return SeaMovesHerTests.NewGame("kapman_sea_look_saves", p => play = p);
            bool quietWas = ClockDrops.Quiet;
            ClockDrops.Quiet = false;
            var dropped = new List<ClockReason>();
            System.Action<ClockReason> onDrop = why => dropped.Add(why);
            ClockDrops.Dropped += onDrop;
            var dry = new List<string>();
            System.Action<long> onStep = minute =>
            {
                var w = ChartWorld.ShipWorld();
                if (Adrift.Helm != Helm.Fast && Adrift.Helm != Helm.River && Adrift.Helm != Helm.ByEye && !Course.IsWater(w.x, w.z))
                    dry.Add($"minute {minute}: {w.x:0} {w.z:0} ({Adrift.Helm})");
            };
            KapmanGame.Stepped += onStep;
            var told = new StringBuilder();
            try
            {
                var game = KapmanGame.Current;
                var map = MapView.Current;
                var body = ChartBody.Current;
                var line = new List<WorldXZ>();
                var kinds = new List<byte>();
                Assert.IsTrue(Course.Plan("lundenwic", ChartWorld.ShipWorld(), Port.LyingId(), line, null, kinds, out double km), "no way from Dorestat to Lundenwic");
                var (runFrom, runTo) = SeaMovesHerTests.LongestOpenRun(line, kinds);
                Assert.Greater(runFrom, 0);
                int startAt = runFrom;
                double runM = 0.0;
                for (int i = runFrom + 1; i <= runTo && startAt == runFrom; i++)
                {
                    runM += WorldXZ.Distance(line[i - 1], line[i]);
                    if (runM > 15000.0)
                        startAt = i;
                }
                var start = line[startAt];
                Assert.IsTrue(Course.SetTo("traiectum", ChartWorld.ShipWorld(), Port.LyingId()));
                yield return play.Click(SeaMovesHerTests.Verb(play, "Cast off"), "Cast off");
                yield return ChartPlay.Frames(10);
                yield return SeaMovesHerTests.AnswerAll(play);
                var u = FloatingOrigin.ToUnity(new WorldPos(start.x, 0.0, start.z));
                game.ship.Teleport(new Vector3(u.x, 0f, u.z), 270f);
                game.ship.anchored = false;
                for (int i = startAt; i <= runTo; i++)
                    Exploration.See(line[i].x, line[i].z, 45000.0);
                Exploration.Apply();
                Assert.IsTrue(Course.SetTo("lundenwic", ChartWorld.ShipWorld(), null));
                Adrift.Forget();
                told.AppendLine($"put at {start.x:0} {start.z:0}, bound for Lundenwic, {Course.Km:0} km; the numbers: warn at {SeaPush.Rules.offLineWarnM / 1000.0:0} km, landfall {SeaPush.Rules.landfallM / 1000.0:0} km");

                int logFrom = FrameLog.Lines.Count, laid = 0, lost = 0;
                double hour0 = game.Hours, lastHour = -1.0, mostOff = 0.0, mostSet = 0.0;
                float wall = Time.realtimeSinceStartup;
                while (game.Hours - hour0 < 36.0 && Time.realtimeSinceStartup - wall < 600f && Port.LyingId() == null)
                {
                    if (play.FrontFloat() != null)
                        yield return SeaMovesHerTests.AnswerAll(play);
                    mostOff = System.Math.Max(mostOff, Adrift.OffLine);
                    mostSet = System.Math.Max(mostSet, Adrift.SetM);
                    if (game.Hours - hour0 >= lastHour + 1.0)
                    {
                        lastHour = System.Math.Floor(game.Hours - hour0);
                        var w = ChartWorld.ShipWorld();
                        var (te, tn) = SeaPush.Stream(w, SimStage.JulianDay);
                        told.AppendLine($"hour {lastHour,2:0}: helm {Adrift.Helm,-9}; set {Adrift.SetM / 1000.0,4:0.0} km {SeaPush.Sets(Adrift.Set.x, Adrift.Set.z),-10}; off line {Adrift.OffLine / 1000.0,4:0.0} km; " +
                                         $"tide {System.Math.Sqrt(te * te + tn * tn):0.00} m/s {SeaPush.Sets(te, tn),-10}; wind drift {System.Math.Sqrt(Adrift.Blow.east * Adrift.Blow.east + Adrift.Blow.north * Adrift.Blow.north):0.00} m/s {SeaPush.Sets(Adrift.Blow.east, Adrift.Blow.north),-10}; " +
                                         $"{body.Knots:0.0} kn; wind {body.Wind:0} m/s from {body.WindFrom:000}; leg {Course.Next}/{Course.Points.Count - 1}; {Course.KmLeft(w):0} km to go; rough {RoughWaters.RoughNow:0.0}; hull {Boat.Hull:0.000}");
                    }
                    for (int i = logFrom; i < FrameLog.Lines.Count; i++)
                    {
                        string text = FrameLog.Lines[i].Whole;
                        bool off = text.StartsWith("Wind and tide have set her"), gone = text.Contains("he cannot make it out");
                        if (off || gone || text.Contains("shore") || text.Contains("lies to") || text.Contains("too deep") || text.Contains(": the "))
                            told.AppendLine($"   {game.Hours - hour0:0.0} h: {text}");
                        if (off || gone)
                        {
                            // as a player would: her course laid again from where she is
                            if (gone)
                                lost++;
                            if (Course.SetTo("lundenwic", ChartWorld.ShipWorld(), null))
                                laid++;
                            game.ship.anchored = false;
                        }
                    }
                    logFrom = FrameLog.Lines.Count;
                    if (play.FrontFloat() == null && (GameSpeed.Level < 4 || GameSpeed.Paused))
                        yield return play.Click(SeaMovesHerTests.Small(play, "5"), "speed 5");
                    yield return null;
                }
                told.AppendLine($"after {game.Hours - hour0:0.0} h: her course was laid again {laid} times ({lost} of them after the reckoning was lost); the most she was set {mostSet / 1000.0:0.0} km, the most off her line {mostOff / 1000.0:0.0} km; " +
                                $"lying at '{Port.LyingId()}'; drops {string.Join(" ", dropped.GroupBy(d => d).Select(g => g.Key + "x" + g.Count()))}; on the land {dry.Count}");
                File.WriteAllText(Path.Combine(play.dir, "sea_day.txt"), told.ToString());
                yield return play.Click(SeaMovesHerTests.Small(play, "Pause"), "Pause");
                map.Follow();
                yield return ChartPlay.Settle(0.5f);
                yield return play.Shot("sea_day_at_its_end");
                Assert.IsEmpty(dry, "she was carried on to the land: " + string.Join("; ", dry.Take(5)));
            }
            finally
            {
                KapmanGame.Stepped -= onStep;
                ClockDrops.Dropped -= onDrop;
                ClockDrops.Quiet = quietWas;
                ClockSettings.AsNew = false;
                SaveGame.FolderOverride = null;
                if (told.Length > 0 && play != null)
                    File.WriteAllText(Path.Combine(play.dir, "sea_day.txt"), told.ToString());
                play?.End();
                FrameRoot.FreshLayout = false;
                GameSpeed.SetLevel(0);
            }
        }
    }
}
