# The debt falls due on a day (Michaelmas), and the count's casks are not gain

Ruling: DECISIONS.md, 7 October, item 10 ("the debt is a fixed sum due on a day"; easy: 480 d by Michaelmas).
Nothing under Assets was changed; this is the change to make. Paths are under `Assets/Kapman/`.

## 1. How it is held now

| What | Where | Now |
|---|---|---|
| The sum | `Game/Data/trade_rules.json` line 59, `"debt": 480` | read at `Code/Game/Trade/TradeRules.cs:158` into `startDebt` (line 64) |
| The term | `trade_rules.json` line 61, `"due_days": 365` | read at `TradeRules.cs:160` into `dueDays` (line 66) |
| The due day | `Code/Game/Trade/Trade.cs:36`, `public static double DebtDue` | a game-day number (days since the start), set once at `Trade.cs:53`: `DebtDue = today + rules.dueDays;` |
| Saved | `Trade.cs:60` (`["due"] = DebtDue`) and `Trade.cs:73` | the day number, so saves need no change |
| Begun from | `Code/Game/KapmanGame.cs:204` | `Trade.Begin(TradeRules.Current, cargo..., Day);` |

So a game begun 1 May 793 is due 365 days on, 30 April 794, not 29 September 793.

### Where it is printed

| Place | Line | Text now |
|---|---|---|
| Top bar | `Code/Game/UI/Frame/FrameGame.cs:75-82` (`DebtLine`), shown at `FrameRoot.cs:586-588` | "Owed 480 d, due in 365 days" / "Owed 480 d, 3 days overdue" |
| Top bar warning | `FrameRoot.cs:588` | class `warn` when fewer than 14 days are left |
| House panel | `Frame/Panels/HousePanel.cs:39-40` | already says the date: "The debt falls due on {date}, N days from now." |
| Tasks panel | `Frame/Panels/TasksPanel.cs:51` | already says the date: "Due on {date}: N days from now." |
| Old market panel | `Code/Game/UI/MarketPanel.cs:446-447` | "..., due in N days" |
| Port talk | `Code/Game/Ports/PortLife.cs:120-121` | "You owe ... , due in N days." |
| Old map table | `Code/Game/Map/MapTable.cs:189-190` | "Owed: 480 d, due in N days" |
| Encounter variable | `Code/Game/Encounters/EncounterGameVars.cs:55-56`, `debt_due_days` | used by `Game/Data/encounters/crew_793.json:129` (`debt_due_days < 40`) |

House and Tasks need no change once `DebtDue` is the right day: they print it through `FrameGame.DateOfDay` (`FrameGame.cs:50-54`).

### What happens on the day if it is not paid, as the code has it now

Nothing. No code acts on `DebtDue`. A search of `Code/Game` finds it only in the printing lines above, the save, and the `debt_due_days` variable. After the day the bar turns to "N days overdue" and stays in the `warn` colour, and the one crew card in `crew_793.json:129` can still come up (it asks for fewer than 40 days left, which stays true when the number goes below zero). No seizure, no interest, no end of the game. This fix does not add one: what the count does on Michaelmas is still to be ruled and written.

## 2. The change: the due day as a date

### 2a. `Game/Data/trade_rules.json`, lines 57-62

```json
 "start": {
  "coins": {"denier": 40, "penny": 25, "dirham": 6, "hacksilver": 30},
  "debt": 480, "_debt": "deniers owed (two pounds of silver) for the ship, to the count",
  "creditor": "creditor.count", "_creditor": "the key for the creditor's name in writing.json (yours to write)",
  "due_month": 9, "due_day": 29, "due_name": "Michaelmas", "_due": "the day the debt falls due: the next one after the start (29 September 793 for a game begun 1 May 793)",
  "due_days": 365, "_due_days": "only used when no clock is known (tests, a level with no clock)"
 }
```

### 2b. `Code/Game/Trade/TradeRules.cs`

After line 66 (`public int dueDays = 365;`):

```csharp
        /// <summary>The day of the year the debt falls due (29 September, Michaelmas); month 0: after dueDays instead.</summary>
        public int dueMonth = 9, dueDay = 29;
        public string dueName = "Michaelmas";
```

After line 160 (`r.dueDays = ...`):

```csharp
                    r.dueMonth = (int)Json.Num(st, "due_month", r.dueMonth);
                    r.dueDay = (int)Json.Num(st, "due_day", r.dueDay);
                    r.dueName = Json.Str(st, "due_name", r.dueName);
```

### 2c. `Code/Game/Trade/Trade.cs`

Replace `Begin` (lines 44-54) with the two below. The old signature stays, so every other caller and test still compiles.

```csharp
        /// <summary>What the due day is called ("Michaelmas"), or "" when it is only a count of days.</summary>
        public static string DebtDueName { get; private set; } = "";

        /// <summary>A new merchant: the starting purse and debt from the rules, an empty hold of the ship's size.
        /// The debt falls due after the rules' count of days (no calendar known).</summary>
        public static void Begin(TradeRules rules, float cargoKg, double today)
        {
            Purse = new Purse();
            foreach (var kv in rules.startCoins)
                Purse.Add(kv.Key, kv.Value);
            Hold = new Hold { capacity = cargoKg };
            Ledger = new Ledger();
            Debt = rules.startDebt;
            Creditor = rules.creditor;
            DebtDue = today + rules.dueDays;
            DebtDueName = "";
        }

        /// <summary>The same, with the calendar known: the debt falls due on the rules' day of the year (Michaelmas),
        /// the next one after today. `year`, `startDay`: the year and day of the year that game day 0 is.</summary>
        public static void Begin(TradeRules rules, float cargoKg, double today, int year, int startDay)
        {
            Begin(rules, cargoKg, today);
            double due = DueDayOf(year, startDay, today, rules.dueMonth, rules.dueDay);
            if (due >= 0.0)
            {
                DebtDue = due;
                DebtDueName = rules.dueName ?? "";
            }
        }

        /// <summary>The game day (whole) of the next `day` of `month` after `today`; -1 if there is none within two
        /// years (a month of 0, or a day that month never has). Begun on the day itself: a year on.</summary>
        public static double DueDayOf(int year, int startDay, double today, int month, int day)
        {
            if (month < 1 || month > 12 || day < 1)
                return -1.0;
            int from = (int)Math.Floor(today) + 1;
            for (int i = from; i < from + 732; i++)
            {
                var d = Calendar.At(year, startDay, i * 24.0);
                if (d.month == month && d.day == day)
                    return i;
            }
            return -1.0;
        }

        /// <summary>Whole days left to the due day (0 on the day itself, below 0 after it).</summary>
        public static double DebtDaysLeft(double today) => Math.Floor(DebtDue) - Math.Floor(today);
```

`Calendar` is `Code/Core/World/Calendar.cs:29` (`At(startYear, startDay, hours)`, line 40); add its `using` to Trade.cs if it is not there already (I did not read the top of the file).

Save and load: add the name beside the day so a loaded game still says "Michaelmas".

```csharp
            // Trade.cs:60, in Capture
            ["debt"] = Debt, ["creditor"] = Creditor, ["due"] = DebtDue, ["due_name"] = DebtDueName,
            // Trade.cs:73, in Restore, after DebtDue
            DebtDueName = Json.Str(d, "due_name", "");
```

A save made before this keeps its old day (start + 365) and no name. That is deliberate: nothing in a saved game is rewritten.

### 2d. `Code/Game/KapmanGame.cs:204`

```csharp
            float room = Mathf.Max(100f, cargo - (float)(voyage.WaterCapacityL * voyage.kgPerLitre));
            if (clock != null)
                Trade.Begin(TradeRules.Current, room, Day, clock.year, clock.startDay);
            else
                Trade.Begin(TradeRules.Current, room, Day);
```

CHECK BEFORE COMMITTING (not verified here): this passes `clock.year` and `clock.startDay` because `FrameGame.DateOfDay` (`FrameGame.cs:53`) turns a game day into a date with the same two numbers. Lines 196-197 just above keep `firstYear` and `firstStartDay` for a game carried over a level change. If `Day` is counted from those and not from the clock's own start, pass `firstYear, firstStartDay` here, and `DateOfDay` has the same fault.

Arithmetic: 1 May 793 is day 121 of the year, 29 September is day 272, so `DebtDue` = 151. `LevelClock.startDay` defaults to 172 (21 June, `LevelClock.cs:31`), which gives 100.

### 2e. The top bar: `Code/Game/UI/Frame/FrameGame.cs:74-82`

```csharp
        /// <summary>"Owed 480 d by Michaelmas, 29 September 793: 151 days left", "... : due today",
        /// "Owed 480 d since Michaelmas, 29 September 793: 3 days overdue", or "" with no debt.</summary>
        public static string DebtLine()
        {
            if (!Trade.Started || Trade.Debt <= 0.01)
                return "";
            var game = KapmanGame.Current;
            if (game == null)
                return $"Owed {Trade.Debt:0} d";
            double left = Trade.DebtDaysLeft(game.Day);
            string day = (Trade.DebtDueName.Length > 0 ? Trade.DebtDueName + ", " : "") + DateOfDay(Trade.DebtDue);
            if (left > 0.0)
                return $"Owed {Trade.Debt:0} d by {day}: {left:0} {(left == 1.0 ? "day" : "days")} left";
            if (left == 0.0)
                return $"Owed {Trade.Debt:0} d by {day}: due today";
            return $"Owed {Trade.Debt:0} d since {day}: {-left:0} {(left == -1.0 ? "day" : "days")} overdue";
        }
```

If that is too long for the bar, the short form is `Owed 480 d, Michaelmas (151 days)`. The date's wording comes from `DateWords`, which I did not read; if it prints the year and the bar is tight, drop the year there.

`FrameRoot.cs:588`, use the same whole-day count:

```csharp
                debt.EnableInClassList("warn", game != null && Trade.Debt > 0.01 && Trade.DebtDaysLeft(game.Day) < 14.0);
```

### 2f. The other printers (same wording, optional but they will otherwise disagree with the bar)

- `HousePanel.cs:38` and `TasksPanel.cs:50`: `double days = Trade.DebtDaysLeft(game.Day);` and put `Trade.DebtDueName` before the date when it is not empty. The stamps at `HousePanel.cs:22` and `TasksPanel.cs:26` can stay.
- `MarketPanel.cs:446-447`, `PortLife.cs:121`, `MapTable.cs:189-190`: replace `Trade.DebtDue - day` with `Trade.DebtDaysLeft(day)`.
- `EncounterGameVars.cs:56`: `return Trade.Started ? Trade.DebtDaysLeft(Today) : 0.0;`. The card at `crew_793.json:129` (`debt_due_days < 40`) now comes up from 20 August and not from late March; no data change needed.

## 3. The four casks are the count's own goods

Where they come aboard: `Code/Game/Chart/ChartWorld.cs:69-80`. A new game puts 4 `rhenish_wine`, 2 `quernstones`, 1 `ships_bread`, 1 `ale` in the hold with `Trade.Hold.Add(g, take, 0.0, day)`: cost 0.

How a sale prints gain: `Trade.cs:133-138`. `gain = d.deniers - d.toll - d.cost`, and the sentence "{gain} more than they cost." is added only when `d.cost > 0`.

So selling the four casks alone already prints no gain. The fault is in two cases:

1. `Hold.Add` (`Code/Game/Trade/Hold.cs:65-77`) merges anything of the same good put aboard within a day into one lot. Buy one cask of wine at Dorestad on the first day and it joins the count's four: five casks at the cost of one. Every sale from that lot then prints nearly the whole price as "more than they cost".
2. A sale that takes some given and some bought casks (`Hold.Remove`, lines 80-98, oldest first) has `d.cost > 0`, so the whole price of the given casks is printed as gain.

(`Ledger` is made at `Trade.cs:50`; I did not find or read the class, so whether it adds the sale up as profit somewhere is NOT checked. Look there too.)

### 3a. `Code/Game/Trade/Hold.cs`

Line 68-69, do not merge goods that cost nothing with goods that were paid for:

```csharp
            var last = lots.LastOrDefault(l => l.good == g);
            if (last != null && Math.Abs(last.day - day) < 1.0 && (last.cost > 0.0) == (cost > 0.0))
```

`Remove` (lines 80-98), count the units that had cost nothing:

```csharp
        /// <summary>Of the units last taken out by Remove, how many had cost nothing (given, not bought).</summary>
        public int LastFree { get; private set; }

        /// <summary>Takes units out, the oldest first; returns what they cost (deniers).</summary>
        public double Remove(GoodDef g, int n)
        {
            double cost = 0.0;
            LastFree = 0;
            foreach (var l in lots.Where(l => l.good == g).ToList())
            {
                if (n <= 0)
                    break;
                int take = Math.Min(n, l.count);
                double part = l.cost * take / l.count;
                if (l.cost <= 0.0)
                    LastFree += take;
                l.count -= take;
                l.cost -= part;
                cost += part;
                n -= take;
                if (l.count <= 0)
                    lots.Remove(l);
            }
            return cost;
        }
```

### 3b. `Code/Game/Trade/Trade.cs:133-138`, in `Sell`

Gain is reckoned only on the units that were bought; the given ones are named for what they are.

```csharp
            int free = Math.Min(Hold.LastFree, d.count);                 // the count's own (or found): nothing paid for them
            double paidShare = d.count > 0 ? (d.count - free) / (double)d.count : 0.0;
            double gain = (d.deniers - d.toll) * paidShare - d.cost;
            string after = "";
            if (d.cost > 0)
                after = gain >= 0 ? $"{m.zone.Show(gain)} more than they cost." : $"{m.zone.Show(-gain)} less than they cost.";
            if (free > 0 && d.cost > 0)
                after = $"Of the {d.count - free} you bought: " + after;
            r.text = Wording.Outcome(m.port, d.toll > 0 ? "sold_toll" : "sold",
                                     d.toll > 0 ? "Sold {goods} for {price}, less the toll of {toll_paid}: took {coins}." : "Sold {goods} for {price}: took {coins}.",
                                     ("goods", Units(good, d.count)), ("good", good.name.ToLowerInvariant()), ("price", m.zone.Show(d.deniers)),
                                     ("coins", Purse.LastCoins), ("toll_paid", m.zone.Show(d.toll))) + " " + after;
```

`Hold.LastFree` is read straight after `Deal.Sell` (`Deal.cs:116` calls `hold.Remove`), with nothing between that removes from the hold. The sentence wording is placeholder: the user writes the words.

This treats every cost-0 lot alike (the count's casks, a find at `GameEffects.cs:81`, `GameEvents.cs:202`), which is right: none of them was bought. If the count's goods must be told apart from a find (for a line such as "the count's wine"), that needs a flag on `Lot` and in its save, a bigger change not drafted here.

## 4. Tests: add to `Assets/Kapman/Tests/EditMode/TradeTests.cs`

```csharp
        [Test]
        public void TheDebtFallsDueAtMichaelmas()
        {
            // a game begun 1 May 793 (day 121 of the year): due 29 September 793, 151 days on
            Trade.Begin(rules, 900f, 0.0, 793, 121);
            Assert.AreEqual(480.0, Trade.Debt, 0.001);
            Assert.AreEqual(151.0, Trade.DebtDue, 0.001);
            Assert.AreEqual("Michaelmas", Trade.DebtDueName);
            var due = Calendar.At(793, 121, Trade.DebtDue * 24.0);
            Assert.AreEqual(793, due.year);
            Assert.AreEqual(9, due.month);
            Assert.AreEqual(29, due.day);
            // whole days, whatever the hour
            Assert.AreEqual(151.0, Trade.DebtDaysLeft(0.9), 0.001);
            Assert.AreEqual(0.0, Trade.DebtDaysLeft(151.5), 0.001);
            Assert.AreEqual(-3.0, Trade.DebtDaysLeft(154.2), 0.001);

            // begun on Michaelmas itself (day 272): the next one, a year on (793 is no leap year, 794 neither)
            Trade.Begin(rules, 900f, 0.0, 793, 272);
            Assert.AreEqual(365.0, Trade.DebtDue, 0.001);
            // begun the day before: tomorrow
            Trade.Begin(rules, 900f, 0.0, 793, 271);
            Assert.AreEqual(1.0, Trade.DebtDue, 0.001);

            // no calendar: the count of days, and no name
            Trade.Begin(rules, 900f, 10.0);
            Assert.AreEqual(10.0 + rules.dueDays, Trade.DebtDue, 0.001);
            Assert.AreEqual("", Trade.DebtDueName);

            // the day survives a save
            Trade.Begin(rules, 900f, 0.0, 793, 121);
            var saved = Trade.Capture();
            Trade.Begin(rules, 900f, 0.0);
            Trade.Restore(saved);
            Assert.AreEqual(151.0, Trade.DebtDue, 0.001);
            Assert.AreEqual("Michaelmas", Trade.DebtDueName);
        }

        [Test]
        public void TheCountsCasksAreNotGain()
        {
            var wine = GameData.Good("rhenish_wine");
            Assert.NotNull(wine);
            var hold = new Hold { capacity = 5000f };
            hold.Add(wine, 4, 0.0, 0.0);          // the count's own: nothing paid
            hold.Add(wine, 1, 30.0, 0.0);         // bought the same day: its own lot
            Assert.AreEqual(2, hold.Lots.Count);
            Assert.AreEqual(0.0, hold.Remove(wine, 4), 0.001);   // the oldest first: the count's four
            Assert.AreEqual(4, hold.LastFree);
            Assert.AreEqual(30.0, hold.Remove(wine, 1), 0.001);
            Assert.AreEqual(0, hold.LastFree);

            // two bought the same day still make one lot
            hold.Add(wine, 1, 30.0, 3.0);
            hold.Add(wine, 2, 64.0, 3.2);
            Assert.AreEqual(1, hold.Lots.Count);
            Assert.AreEqual(94.0, hold.Lots[0].cost, 0.001);
        }
```

`Calendar` may need a `using` in the test file (its namespace was not read). The Sell wording is not tested here because the end-to-end sale needs a `Market`; copy the set-up of the existing buy-and-sell test in the same file and assert that a sale of the four given casks has a `text` with no "more than they cost".

## 5. Not done, not checked

- Nothing was compiled or run (the rules of this task: no Unity).
- What the count does on the day when unpaid: no code exists; still to be ruled.
- `KapmanGame.cs:196-197` `firstYear`/`firstStartDay` against `clock.year`/`clock.startDay` (see 2d).
- The `Ledger` class: not found in `Code/Game/Trade/Ledger.cs`, not read.
- `FrameGame.DateWords` and the bar's width: not read.
- Hard terms (20 pounds) from ruling 10: the sum is `start.debt`; no difficulty switch drafted here.
