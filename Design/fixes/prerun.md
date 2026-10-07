# The pre-run: the world starts on 1 April

Source: `Design/research/next/money.md`, section 11 item 1 and the tuned set, row 1.
Written from reading the code only. Nothing was compiled or run. Points I did not check are marked **(not checked)**.

## What is wrong now

`Assets/Kapman/Code/Game/KapmanGame.cs`, `MakeWorld()`:

- line 294: `Markets.OpenAll(Year, Day);` opens every market at its usual stock on the player's first morning.
- lines 296 to 299: `new WorldSim(..., clock.year, clock.startDay, clock.GameHours, ...)` starts the 56 traders in the same hour.

So every price gap is open on day 0. Money.md puts that at about 520 d a season for the sensible player.
A cask of Rhenish wine asks 44 d at Dorestat on 1 May. With the traders at work since 1 April it asks about 60 d.

## The change in one line

Make the markets and the world `N` days before the clock's start, step the world `N x 24` hours with no player and
no drawing, and only then hand it to the game. `N` is the days from `world_from` (1 April) to the clock's start.

| Clock's start | Day of year | N days | World hours stepped |
|---|---|---|---|
| 1 May 793 | 121 | 30 | 720 |
| 21 June 793 (the clock's default, `startDay` 172) | 172 | 81 | 1,944 |

## Which way: negative days, not a moved day 0

Money.md allows either. I would let the world's hours and the markets' days run negative, and leave day 0 on the
clock's start. Reasons, from the code:

- `Calendar.At` (`Code/Core/World/Calendar.cs:40`) takes negative hours: it has `while (day < 0) day += DaysIn(--year);`.
- `Seasons.DateOf` (`Code/Game/Trade/Seasons.cs:73`) takes a negative day: `startAbsolute + (long)Math.Floor(day)`.
- `Market.CatchUp` (`Code/Game/Trade/Market.cs:155`) is `while (day < today)`, so a market made at day -30 steps up to 0.
- `WorldSim.Hour()` (`Code/Game/Sim/WorldSim.cs:244`) uses `Math.Floor(Day)` and `lastDay = int.MinValue`, so the
  daily work fires on negative days too.
- Nothing else moves: `Seasons.SetStart` (`KapmanGame.cs:169`), `Trade.Begin(..., Day)` (line 204), the debt's day,
  the tick ladder, the saved games and `MapView.cs:1159` (`game.Hours - (world.Hours - 1.0)`) all still read day 0
  as the clock's start. At the end of the pre-run `world.Hours` equals `clock.GameHours`, as now.

**(not checked)** Whether anything inside the sim uses the day as an array index or takes `%` of it
(`TraderBrain`, `RaiderBrain.Daily`, `Fairs.Daily`, `SimStats.Daily`, `Shocks`). A negative `%` in C# is negative.
Grep `Code/Game/Sim` and `Code/Game/Trade` for `% ` and `(int)` next to `Day` before trusting the first run.

## The edits

### 1. `Assets/Kapman/Game/Data/trade_rules.json`, the "start" block (lines 57 to 62)

Line 61 `"due_days": 365` becomes:

```json
  "due_days": 365,
  "world_from": "04-01", "_world_from": "month-day the world's traders and markets start; stepped to the clock's start before the first frame. Empty = the player's first morning"
```

### 2. `Assets/Kapman/Code/Game/Trade/TradeRules.cs`

After line 66 (`public int dueDays = 365;`):

```csharp
        /// <summary>The month and day the world's traders and markets start ("04-01"; 0 = the player's first morning).</summary>
        public int worldFromMonth = 4, worldFromDay = 1;

        /// <summary>Whole days from world_from back from the clock's start (0 if the start is on or before it).</summary>
        public int PreRunDays(int year, int startDay)
        {
            if (worldFromMonth < 1 || worldFromMonth > 12)
                return 0;
            int doy = worldFromDay;
            for (int m = 1; m < worldFromMonth; m++)
                doy += Kapman.World.Calendar.DaysIn(year, m);
            return System.Math.Max(0, startDay - doy);
        }
```

After line 160 (`r.dueDays = (int)Json.Num(st, "due_days", r.dueDays);`):

```csharp
                    string from = Json.Str(st, "world_from", "04-01");
                    var md = (from ?? "").Split('-');
                    if (md.Length == 2 && int.TryParse(md[0], out int fm) && int.TryParse(md[1], out int fd))
                    {
                        r.worldFromMonth = fm;
                        r.worldFromDay = fd;
                    }
                    else
                        r.worldFromMonth = 0;                                    // empty or unreadable: no pre-run
```

**(not checked)** That `Kapman.World.Calendar` is the namespace of `Code/Core/World/Calendar.cs` (KapmanGame.cs:207
writes `Kapman.World.Calendar.At`, so it should be) and that `Json.Str` has a three-argument form (line 159 uses one).
In 793 (not a leap year) 1 April is day 91: 121 - 91 = 30, 172 - 91 = 81. These match money.md.

### 3. `Assets/Kapman/Code/Game/Sim/WorldSim.cs`: one new method, after `Hour()` (after line 278)

```csharp
        /// <summary>
        /// Steps the world to `toHours` with nobody watching (the pre-run at a new game: the traders have been at work
        /// since world_from). No player, no keeping clear of her; the same hours as play, so one seed gives one world.
        /// </summary>
        public int RunTo(double toHours)
        {
            var player = playerNow;
            playerNow = null;
            int n = 0;
            while (Hours + 1.0 <= toHours + 1e-9)
            {
                Hour();
                n++;
            }
            playerNow = player;
            return n;
        }
```

**(not checked)** `UpdatePlayer()` with `playerNow == null`. `MakeWorld` sets `playerNow` before `Populate()` today,
and the tests build a `WorldSim` without it (`Tests/EditMode/PortLifeTests.cs:48`), so null should be safe.

### 4. `Assets/Kapman/Code/Game/KapmanGame.cs`, `MakeWorld()`

Line 294 to 299, now:

```csharp
            Markets.OpenAll(Year, Day);
            Shocks.Clear();
            world = new Kapman.Game.Sim.WorldSim(Kapman.Game.Sim.SimData.Load(shipClasses, traffic), new Kapman.Game.Sim.SimRules(),
                                                 Kapman.Game.Sim.SimRoutes.Load(lanesFile), weather, worldSeed, clock.year, clock.startDay, clock.GameHours,
```

becomes:

```csharp
            // the world has been at work since world_from (1 April): markets and ships are made as of that day
            int back = TradeRules.Current.PreRunDays(clock.year, clock.startDay);
            double from = clock.GameHours - back * 24.0;                 // negative: day 0 stays the clock's start
            Markets.Clear();
            Markets.OpenAll(Year, from / 24.0);
            Shocks.Clear();
            world = new Kapman.Game.Sim.WorldSim(Kapman.Game.Sim.SimData.Load(shipClasses, traffic), new Kapman.Game.Sim.SimRules(),
                                                 Kapman.Game.Sim.SimRoutes.Load(lanesFile), weather, worldSeed, clock.year, clock.startDay, from,
```

`Markets.Clear()` is new here: without it a market already opened at day 0 by something earlier in the frame
(`MarketLine()` at line 338 calls `Markets.Of`, though it returns first while `World == null`) would stay cold.
**(not checked)** That nothing has put the player's own goods into a market before `MakeWorld` runs. It runs in the
first `Update`, so it should be clean at a new game.

Then, after `world.Populate();` (line 315) and before `Listen();`:

```csharp
            // the held states that began before world_from (the famine of 793, from January) are felt through the pre-run
            var fromDate = Kapman.World.Calendar.At(clock.year, clock.startDay, from);
            if (back > 0)
            {
                Listen();
                foreach (var e in Chronicle.Ongoing(fromDate, 60))
                    Chronicle.Replay(e);
                var watch = System.Diagnostics.Stopwatch.StartNew();
                int stepped = world.RunTo(clock.GameHours);
                Debug.Log($"Kapman: the world pre-run, {back} days ({stepped} hours) in {watch.ElapsedMilliseconds} ms; markets {Markets.Hash():X16}");
            }
```

The existing replays at lines 319 to 322 stay as they are (they bring the news and the shocks of the last 60 days
to the first morning). Two things to settle when this is built, both **(not checked)**:

- `ChronicleCame` (line 390 to 404) applies a shock with `Day`, which is the clock's day (0), not the world's
  (-30). For the replay before the pre-run it should use `world.Day`. Read lines 380 to 410 and pass the day in.
- Replaying `Ongoing` twice (before the pre-run and at line 319) must not double the famine. If `Shocks.Apply`
  adds, drop the famine from the second loop when `back > 0`, or make the second loop `Recent` only.
- In play `Shocks.Daily(Current.Day)` runs each game day (line 143) and keeps held famines dear. The pre-run does
  not raise the tick ladder, so add `Shocks.Daily(Day);` inside `WorldSim.Hour()`'s daily block (line 250), or
  call it from `RunTo` once a day. Without it the famine eases off during the 30 days.

### 5. A loaded game

`SaveGame.Load()` (KapmanGame.cs near line 484) restores the markets and the world over whatever `MakeWorld` made.
The pre-run is then wasted time, not wrong. If the order allows, skip it when a load is pending: `back = 0`.
**(not checked)** where the load sits relative to the first `MakeWorld`.

## What it costs at the start

Not measured (the rules of this task forbid running Unity). What can be said from the code:

- The work is 720 calls of `WorldSim.Hour()` from 1 May, 1,944 from 21 June, in one frame, before anything draws.
- One `Hour()` is: a weather snapshot, `CountVisitors`, `Step` for each ship (56 traders and the raiders),
  `Seamanship.Resolve` over the ships under way, `Advance` for each, `RaiderBrain.Hunt`; once a day
  `Markets.CatchUpAll` over every port and the day's pay.
- Money.md's own tables come from 200 seasons of 151 days each per row, about 725,000 world hours a row, run many
  times over. So an hour of the world is cheap. My guess is well under a second for 720 hours and one to two
  seconds for 1,944. That is a guess. The `Debug.Log` in edit 4 prints the real figure on the first run.
- If it is over about half a second, the pre-run should sit behind the loading card, not in the first `Update`.
  `keepClear` can be switched off for the pre-run (`world.keepClear = false` before `RunTo`, back after) to save
  the pair checks, but then the pre-run is not the same world as money.md measured. Decide by the number.
- `world.stats.csvEvery = world.stats.sampleEvery = 0` (line 302) is set before the pre-run already. Keep it so.

## The test

New file `Assets/Kapman/Tests/EditMode/PreRunTests.cs`, on the pattern of `PortLifeTests.cs` (lines 17 to 49).
It builds the world the way the game does, so it is the test for edits 2 and 3. The ask on the first morning is
the mean of several seeds, as in money.md's table (mean of 24 seeds: 60 d against 44 d cold).

```csharp
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Kapman.Game;
using Kapman.Game.Sim;

namespace Kapman.Tests
{
    public class PreRunTests
    {
        const string Data = "Assets/Kapman/Game/Data/";
        const int Year = 793, FirstOfMay = 121;
        TradeRules rules;

        [SetUp]
        public void Load()
        {
            GameData.LoadGoods(AssetDatabase.LoadAssetAtPath<TextAsset>(Data + "goods_793.json"));
            GameData.LoadPorts(AssetDatabase.LoadAssetAtPath<TextAsset>(Data + "ports_793.json"));
            rules = TradeRules.Load(AssetDatabase.LoadAssetAtPath<TextAsset>(Data + "trade_rules.json"));
            Seasons.SetStart(Year, FirstOfMay);
            Markets.Clear();
        }

        [TearDown]
        public void Done()
        {
            Markets.Clear();
            Seasons.SetStart(793, 172);
        }

        WorldSim World(int seed, int back)
        {
            var data = SimData.Load(AssetDatabase.LoadAssetAtPath<TextAsset>(Data + "ship_classes_793.json"),
                                    AssetDatabase.LoadAssetAtPath<TextAsset>(Data + "traffic_793.json"));
            Markets.Clear();
            Markets.OpenAll(Year, -back);
            var w = new WorldSim(data, new SimRules(), new StraightRoutes(), new Kapman.Weather.WeatherModel(793), seed,
                                 Year, FirstOfMay, -back * 24.0);
            w.stats.csvEvery = w.stats.sampleEvery = 0;
            w.Populate();
            w.RunTo(0.0);
            return w;
        }

        float WineAsked()
        {
            var m = Markets.Of(GameData.Port("dorestad"), Year, 0.0);
            return m.Asks(m.Get("rhenish_wine"), 5);
        }

        [Test]
        public void WorldFromIsTheFirstOfApril()
        {
            Assert.AreEqual(30, rules.PreRunDays(793, 121), "1 May");
            Assert.AreEqual(81, rules.PreRunDays(793, 172), "21 June");
            Assert.AreEqual(0, rules.PreRunDays(793, 60), "a start before 1 April: no pre-run");
        }

        [Test]
        public void TheColdTableAsksFortyFour()
        {
            World(1, 0);
            Assert.AreEqual(44f, WineAsked(), 3f, "an untouched Dorestat on 1 May");
        }

        [Test]
        public void TheFirstMorningAsksAboutSixtyForRhenishWine()
        {
            int back = rules.PreRunDays(Year, FirstOfMay);
            float mean = Enumerable.Range(1, 8).Select(seed => { World(seed, back); return WineAsked(); }).Average();
            Assert.AreEqual(60f, mean, 8f, "Dorestat, 1 May, traders at work since 1 April (money.md: 60 d, not 44)");
            Assert.Greater(mean, 50f, "not the cold table");
        }

        [Test]
        public void TheWorldEndsOnTheClocksStart()
        {
            var w = World(1, 30);
            Assert.AreEqual(0.0, w.Hours, 1e-6);
            Assert.AreEqual(5, w.Date.month);
            Assert.AreEqual(1, w.Date.day);
        }

        [Test]
        public void OneSeedGivesOneWorld()
        {
            World(7, 30);
            ulong a = Markets.Hash();
            World(7, 30);
            Assert.AreEqual(a, Markets.Hash(), "two pre-runs of one seed");
        }

        [Test]
        public void ThePreRunIsQuick()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            World(1, 81);
            Debug.Log($"pre-run, 81 days: {watch.ElapsedMilliseconds} ms");
            Assert.Less(watch.ElapsedMilliseconds, 3000, "the longest pre-run (a start on 21 June)");
        }
    }
}
```

Cautions on the test, all **(not checked)**:

- The namespaces (`Kapman.Game` for `Markets`, `GameData`, `TradeRules`, `Seasons`; `Kapman.Game.Sim` for the
  sim) and the test assembly's name: copy the `using` lines and the namespace from `PortLifeTests.cs` lines 1 to 16.
- `GameDate`'s field for the day of the month: I wrote `.day`; `Calendar.At` builds it as `new GameDate(year, doy, m, d, hour)`.
- The good's id `rhenish_wine` is the one `KapmanGame.HudGoods` uses (line 328), and the port's id is `dorestad`
  (name "Dorestat", `ports_793.json` line 4).
- Money.md's 60 d was measured with the game's routes (`SimRoutes.Load(lanesFile)`), the seamanship and raider
  tables and the fairs. The test uses `StraightRoutes` as the other tests do, so its mean may sit a few d off.
  That is why the band is 8 d. If it lands outside, load the lanes, seamanship, raiders and fairs as `MakeWorld`
  does (lines 296 to 301) before widening the band. The 44 d cold figure in money.md is also a 24-seed mean on
  1 May; a cold table has no seed in it, so 44 should be near exact.
- The famine is not in this test (no chronicle replay), and money.md's 60 d row is before the famine change, with
  the famine on all Food. Wine is not Food, so it should not matter for this good.

## Order of work

1. Edit 1 and 2, run `WorldFromIsTheFirstOfApril`.
2. Edit 3, run the rest of `PreRunTests`. Read the printed time.
3. Edit 4, then the three open points on the chronicle and `Shocks.Daily`.
4. Play: the H panel's market line (`MarketLine()`, KapmanGame.cs:331) shows Dorestat's wine asks/pays on the
   first morning. It should read about 60, not 44, from a 1 May start.
