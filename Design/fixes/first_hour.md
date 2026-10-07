# The first hour: a stake with a day, six river legs, one screen

7 October 2026, written between about 12:15 and 13:10. For the builders. It answers the newcomer's first, second and
fifth changes (`Design/critique/review_newcomer.md`, section 6: plus 6, plus 5 and plus 3 each taken alone; by his
own reckoning his five overlap and come to about fourteen together) and his cons 1, 2, 5, 11 and 17.

Nothing in the project was changed but this file. Unity and git were not run. Paths are under `Assets/Kapman/`
unless they start with `Design` or `Bake`.

**What was checked, and how.**

- Every code edit below was applied to a copy of the project's source by a script that refuses an edit whose old
  lines are not found exactly once (one line stands twice, and the edit says so): 54 edits in 22
  files, all found, against the files as they stood at 13:04 today. Six builders were changing the code meanwhile, so **search for the words, not the line**: a line
  number here is of that minute.
- With the edits and the four new files in, `Kapman.Core`, `Kapman.Game`, `Kapman.Tests.EditMode` and
  `Kapman.Tests.PlayMode` were compiled outside Unity with Unity's own compiler (Roslyn of 6000.6.3f1) and the
  project's own reference lists (`Library/Bee/artifacts/1900b0aE.dag/*.rsp`): no errors, and no warning in a new or
  changed file (two old warnings in Game, twelve old ones in PlayMode).
- The plain parts of the new test file were **run** outside Unity by a small runner on the compiled assemblies: the
  two new verbs' grammar, the first payment's sums, the three fittings' speeds, the fixed screen. 64 checks, 64 pass.
- The data drafts (five cards, the tally's new lines, 46 writing keys, the goods table's block) were checked by a
  script: they parse, every effect has a verb and a shape the changed engine takes, every good, port, role and card
  named exists, every key a card names has a draft. **They were not passed through the game's own loader.**
- **Not run:** no test in Unity, no frame of the game, no soak of the economy. Section 8 lists what that leaves open.

**Written elsewhere today, and not repeated here** (section 6 says where each touches this): the clock itself
(`fixes/clock_five_speeds.md`); the strings "(to come)", "not yet", numbers in brackets, rules in quote marks, the
Rest and Offer buttons (`fixes/page_and_strings.md`, which leaves this file "the first hour's fixed screen, the half
row cut by a window's edge and the three names for a penny"); the whole debt's own day (`fixes/debt_day.md`); orders
(`fixes/orders.md`); gales, the hull and shortened sail (`fixes/risk_is_real.md`); one cast (`fixes/one_cast.md`).

## 0. In short

1. **The tally opens the game.** A card may be marked `"opening": true`. When the player has said who they are, and
   she lies at home, such a card goes in line and nothing else does. The quiet home call stays quiet (its test
   stands as it is); the opening is a second, later step, because the quiet call is made in the first frame, behind
   the "Who are you?" window and at Dorestat for every people. One line of data makes the tally that card.
2. **The steward cuts a first payment at minute nought: 120 d (ten dozen of the forty), due on the tenth day**,
   11 May 793 from a start on 1 May. Two new verbs, `due 120 in 10` and `debt -120`. The top bar reads
   `First 120 d by 11 May: 10 days. Owed 480 d, due in 151 days` (the second half as `debt_day.md` leaves it).
   The purse at the start is 86 d, so it cannot be paid from the purse, and it is about half of what the four
   casks fetch at Lundenwic. It is paid at home, to the steward, on his card: in silver, or with four casks at
   thirty. Nothing is added for lateness (the king forbids it, as the card says already). A miss shuts the grace
   road at Michaelmas and cools the steward.
3. **Under the five-speed clock the first hour is played at speed 2 on the river and speed 3 on the open sea**, with
   the clock at 1 in port and held on a card, as ruling 26 has it. It then holds four game days of river and the
   crossing: six river legs, six calls, the first payment settled at home at about minute 45, and the Strand at
   Lundenwic between minute 50 and 60. Section 1 has it minute by minute.
4. **Each of the four river ports wants one thing another of the four sells.** A port may now make and want what
   its region does not: nine figures in a new block of `goods_793.json`, read by four lines of code. On a table
   nobody has traded on yet the five carries clear 2 to 10 d a unit. All are small: a cake, a dozen, a crate, a
   sack, a wheel. The start cargo loses its quernstones so that 78 kg of room stand free beside the wine.
5. **Three things to buy at 12, 15 and 18 d, one at each port below home, each of which changes a leg**: a tow line
   at Traiectum (on a river she is never slower than three knots), a lead at Witla (she does not creep over the
   flats), reef bands at Walichrum (in a hard wind she goes on where she would lie to). They are three of the forty
   in `story/ship_upgrades_793.json`, at that file's own prices. Three cards and one small class.
6. **One fixed screen until the first payment's card is answered** (or the fourteenth day): in port Trade (tall),
   Voyage, People here; at sea Voyage, The field, Wind and tide. No bar of workspaces, no layouts, no sizes, no
   window to close or pick. No row is drawn cut through by a window's edge. The top bar's debt can be clicked: at
   home it takes her to the steward.
7. **The tests.** `EncounterChartTests.ANewGamesCallAtHomeBringsNobody` is not changed. A new file of ten tests;
   the play test answers the tally after every "Begin" at Dorestat (a helper is drafted) and its two "a card opened
   at home" lines change their meaning, not their place (section 5).

Seven choices in this file are mine and the user may overrule them: section 7.

## 1. The clock, and what the hour holds

### 1.1 What was written for the slow clock

- **The reviewer's first con and most of his 68** rest on a game day of six real minutes at best (the old 4x, which
  is speed 2 now). Michaelmas was fifteen hours of running clock away. Under ruling 26 it is two and a half hours at
  speed 3 and 38 minutes at speed 4 (`clock_five_speeds.md` 1.2). That is still not inside a first hour played at
  the speeds a first hour can be followed at, so the stake with a near day is still wanted. His sum "ten game days,
  which is one hour at 4x" holds at speed 2 with the clock never stopping. With the stops it does not, and section
  1.3 counts them.
- **"Two to fifteen minutes apart at 4x"** for the river ports is speed 2 now, and stands.
- **`Design/research/next/crossing.md`** fills a crossing of 13 to 30 minutes with two or three small offers every
  four game hours. At speed 3 the crossing is 2 to 5 and a half minutes. As the hour's last act it needs two or
  three cards, not thirty lamps.
- **`Design/research/next/money.md`**: "at 4x ten days are one real hour: 8 d in the middle hour". At speed 3 ten
  days are ten minutes, so the thin middle of the game is thin for minutes, not hours. The first hour still needs
  something silver can become, because it is played slower than that.
- **The book's pass marks** (`GAME_MAKER.md`, stage 1): "cast off before minute 5" is tight now that the tally
  stands in front (it is read from 1:30 to 3:30); say minute 7. "A sale by minute 20" is met by minute 10.
- **`Design/checks/goods_first_hour.md`** found two paying trades among the first five ports because Dorestat,
  Traiectum and Witla share one region. That does not rest on the clock, and section 3 mends it.

### 1.2 The speeds

From `clock_five_speeds.md` 1.1: speed 1 is a game hour a real minute; speed 2 is four game hours a minute; speed 3
is a game day a minute. That write-up's rules, which this hour leans on: a card holds the clock; making fast drops
it to 1; nearing port drops it to 2; **nothing raises it by itself**; `+` goes back to the speed she had.

**So a new player who never touches the clock sails at speed 1**, and at speed 1 Dorestat to Traiectum is 3 to 9
real minutes and Witla 11 to 34. The hour below needs two things of the clock's builder, both small:
- a new game's "speed she had" is 2, so that after the first Cast off the bar reads `[+] back to 2`;
- the first time a course with a sea leg is set, the log says once: "The open sea is long. The clock has a speed 3."

Her own speeds, from the compiled code (the Gislinge boat's class; a straight sum, no bends, no slowing at marks):

| | knots | 30 km (Traiectum) | 55 km (Witla to Walichrum) | 111 km (Witla) |
|---|---|---|---|---|
| A fair wind on the quarter, 8 m/s | 5.38 | 3.0 h | 5.5 h | 11.1 h |
| Beating into 8 m/s | 1.83 | 8.8 h | 16.2 h | 32.7 h |
| A calm, at the oars | 1.75 | 9.3 h | 17.0 h | 34.2 h |
| At the tow line (section 3.4) | 3.00 | 5.4 h | 9.9 h (not a river: no tow) | 20.0 h |

`crossing.md` reckoned the same legs along the real bends, fair to foul at 8 m/s: Traiectum 3.5 to 7.5 game hours,
Witla 15 to 25, Walichrum from Dorestat 19 to 43, Lundenwic by the Lek 50 to 111 (a calm: 134). The table below
uses those. **Witla to Walichrum is not in any study**: 55 km is the difference of the two ways from Dorestat, and
6 to 14 game hours is my sum from the Traiectum leg's speeds.

### 1.3 The hour, minute by minute

Real minutes on the left. The game begins at about six in the morning of Wednesday 1 May 793 (the bar says "early
morning"; the hour itself was not read). Gains are on a table nobody has traded on yet (section 3.3).

| Real minutes | Clock | Game time | What happens |
|---|---|---|---|
| 0:00 to 1:30 | held | day 0, morning | "Who are you?": people, faith, past. |
| 1:30 to 3:30 | held | the same | **The tally.** The steward: 480 pennies by Michaelmas; ten dozen of them in ten days; the wine in the boat to be sold or carried, as you please. The bar: `86 d · bread for 7 days · First 120 d by 11 May: 10 days. Owed 480 d, due in 151 days`. |
| 3:30 to 6:30 | 1 | 06:00 to 09:00 | Dorestat, the fixed port screen. The hold has 78 kg of room beside the four casks. Wax is 35 d a cake and the word under it says Traiectum pays well for it. Two cakes. Voyage: Traiectum. Cast off. |
| 6:30 to 8:00 | 2 | 3.5 to 7.5 h | **Leg 1, to Traiectum, 30 km.** The sea screen: Voyage, The field, Wind and tide. One river card may come. |
| 8:00 to 12:00 | 1, held on cards | 3 h | Traiectum. The toll man. The wax sold to the prior's want: about 17 d clear on two cakes. **The first sale, by minute 10.** Within the game hour the ferryman's card: a tow line, 15 d. |
| 12:00 to 13:30 | 2 | 3.5 to 7.5 h | **Leg 2, home, 30 km.** With the line aboard and a foul wind the ship's line reads "On the river, bound for Dorestat: towed from the bank", and the leg is 5 hours, not 9. |
| 13:30 to 16:30 | 1 | 3 h | Dorestat again. Combs for Witla (9 d a dozen), jugs for Walichrum (21 d a crate): the purse will not buy all the room holds. A sack of bread (3 d): the bar reads "bread for 6 days" and the day is in 9. |
| 16:30 to 22:00 | 2 | 15 to 25 h | **Leg 3, down the Lek to Witla, 111 km.** The first night under way ("Night falls."), a crew card, sails in The field. Day 1. |
| 22:00 to 25:30 | 1 | 3 h | Witla. The toll man. Combs: 5 d a dozen clear. The pilot's card: a lead, 18 d (the steersman's child has the father's, and Gerlef says so). |
| 25:30 to 28:30 | 2 | 6 to 14 h (my sum) | **Leg 4, to Walichrum, 55 km**, the first salt water. Day 2. |
| 28:30 to 32:00 | 1 | 3 h | Walichrum. The jugs: 6 d a crate clear. Madder (9 d a sack) for Dorestat's dyers, cheese (3 d a wheel) for Witla. The sail-sewer's card: reef bands, 12 d. |
| 32:00 to 35:00 | 2 | 6 to 14 h | **Leg 5, back to Witla.** |
| 35:00 to 37:30 | 1 | 2.5 h | Witla. The cheese: 2 d a wheel clear. Day 2 or 3. |
| 37:30 to 43:00 | 2 | 15 to 25 h | **Leg 6, up the Lek, home, 111 km.** The tow line's long leg: 20 hours in a foul wind, not 33. |
| 43:00 to 48:00 | 1, held on the card | 3 h | **Dorestat, day 3 or 4** (Saturday 4 or Sunday 5 May). The madder: 5 d a sack clear. The purse: about 70 to 150 d by what was bought. **The first payment, six or seven days before its day** (a click on the bar's debt): pay 120 d and the casks are free cargo; or hand the four casks back and sail with what the purse buys; or sell one or two casks here at about 32 d and pay; or keep all four and run for Lundenwic, 2 to 5 and a half days each way by the measure, and be back by Saturday or not. |
| 48:00 to 54:00 | 3, held on cards | 2 to 4.6 days | **The crossing, 415 km.** Two or three cards. If it blows over 17 m/s and the reef bands are on her, she goes on. Over the Thames flats she creeps if there is no lead. |
| 54:00 to 60:00 | 1 | 3 to 6 h | **Lundenwic, day 5 to 9.** The wine: about 59 d a cask after the toll with the world's traders at work, 72 d on a cold table (`money.md`, section 3). If the payment was not made: "Tomorrow is the steward's day" comes in the log on Friday 10 May wherever she is, and the clock drops. |

What the sums come to: sailing 12 to 23 minutes at speed 2 for the six river legs and 2 to 5 for the crossing at
speed 3; six calls of about three minutes at speed 1; about ten minutes of cards with the clock held; a minute and
a half of creation. 45 to 60 minutes to the Strand. **The tenth day itself comes at about minute 55 to 65** for a
player who keeps moving and has not paid; a player who takes the river road and pays on the way out has settled it
at about minute 45. At speed 1 throughout the tenth day is four real hours away: that is why 1.2 asks the clock's
builder for its two lines.

## 2. A stake that falls due inside the first hour (the first change)

### 2.1 How a new game opens today

- `Code/Game/Chart/ChartWorld.cs`, `Start` (line 39): makes a port for every row of the ports table, lays the ship
  at `homePort` ("dorestad"), and, for a new game, puts aboard 4 `rhenish_wine`, 2 `quernstones`, 1 `ships_bread`,
  1 `ale`, each as far as the hold has room (line 72). The hold has 660 kg of room (`money.md`): the wine takes 552,
  one pair of querns 72, the bread 30. The ale never fits. 6 kg are left.
- `Code/Game/KapmanGame.cs` line 207: `Trade.Begin` gives the purse (85.5 d) and the debt, 480 d due in 365 days.
- `Code/Game/Encounters/EncounterRunner.cs`, `HomeCall` (line 189): in the first frames, when she lies at a port and
  no call is in progress, `d.BeginCallQuietly(at)`.
- `Code/Game/Encounters/EncounterDirector.cs`, `BeginCallQuietly` (line 277): the call is in progress (the cards
  read the port and "moored"), but no arrival set and no visit draw are made. Nobody comes down.
- `Code/Game/UI/Frame/FrameRoot.Self.cs`, `BeginSelf` (line 186): on "Begin" the self is made and
  `world.StartAt(people.home)` lays her at her people's port. For a Frisian she lies there already and nothing
  moves. For the others the Dorestat call is ended (which drops any harbour card waiting) and a quiet call is begun
  at the new port.
- `Game/Data/encounters/chain_house_debt_793.json`, the first card, `harbour.chain_house_debt.s01_tally`: weight
  40, once, `when` "moored and port.id = dorestad and year < 796 and not flag.ch_house_debt_1". Its own note calls
  it "the game's opening". As the code stands it can come only by the visit draw of a later arrival at Dorestat.
  Each of its three choices sets `ch_house_debt_1`, posts a line of news, and schedules the rival's card in 20 days
  and the count's day "in 100" (which from 1 May is 9 August, not Michaelmas: the reviewer's con 9).
- The engine has no verb that changes the debt (that file's own "about" says so), so no card can lower the sum on
  the bar. `Code/Game/UI/Frame/Panels/TasksPanel.cs` pays it by two buttons, "Pay 20 d" and "Pay it all", in any
  port with a market (the reviewer's con 17).
- Two tests hold the quiet call: `Tests/EditMode/EncounterChartTests.cs`, `ANewGamesCallAtHomeBringsNobody` (line
  87), and `Tests/PlayMode/ChartGameTests.cs`, the two lines "a card opened at home on a new game: nobody comes down
  to a boat that has not arrived" (lines 433 and 860 at 13:04).

### 2.2 The rule

**The opening.** A harbour card may carry `"opening": true`. `EncounterDirector.Opening()` puts in line the opening
cards whose `when` holds at the port of the call, in the book's order, two at most, and nothing else. The runner
calls it once, when `Self.Chosen` is true, she lies still and the home call is in progress; never for a loaded
game, never for a boat that has already sailed.

Why it is not inside the quiet call, as the reviewer put it ("a mark that the quiet call lets through"): the quiet
call is made before the creation window is answered. A card put in line there would open over "Who are you?" (the
frame's `EncounterRunner.Blocked` waits only for the title, the pause menu, the chest and a happening), and it would
be judged before `me.people` and `me.past` are known. So the quiet call stays as it is, and its edit-mode test with
it. What changes is that a new game is no longer cardless at home: the play test's two lines now say "no card but
the opening" (section 5).

Today only Dorestat has an opening card. A Dane at Ripa gets none, and still has a debt to a Frankish count on his
bar (the reviewer's con 8). The content pass on starts by people writes one for each home: mark it `"opening":
true` and cut its first payment with `due`. Where it is paid, and on which two cards, are three constants in the
runner today (`EncounterRunner.FirstPaymentPort`, `FirstPaymentCard`, `FirstPaymentLateCard`: Dorestat's); a start
with another creditor needs them by home, which is a table of one row a people and is not drafted here.

A new game sets the self back to "not chosen" before the scene starts again (`MainMenu` runs every reset method
of the game, `Self`'s among them; read at `Code/Game/UI/MainMenu.cs` line 216), so `Self.Chosen` is false until
"Begin" in a second game too. The new `Fittings.Reset` is one of those methods.

**The first payment.**

| | | Why |
|---|---|---|
| The sum | 120 d | "Ten dozen of the forty": the steward counts in dozens already. The purse is 86 d (81 d in the money of Dorestat, where foreign coin loses a tenth), so it is 34 d short at minute nought. Four casks fetch 235 to 290 d at Lundenwic (59 to 72 d each after the toll): it is "about half of what the first four casks fetch", as the reviewer asked. |
| The day | the tenth day from the card: 11 May 793, a Saturday | No Sunday, no feast (`checks/calendar_first_season.md`: the Rogation days begin on Monday 13 May). The river road brings her home on day 3 or 4, so it is not time that is short on that road but silver. Lundenwic and back before the day is possible in a fair wind and not in a foul one. |
| What is needed | 120 d, and about 13 d of pay and bread in ten days (`money.md`: 1 d a day, and a second sack) | So about 47 d must come from trade, or from the casks. |
| Where the 47 d can come from | the river: up to about 70 d on a cold table if every leg is worked and nothing is bought (3.3), a third to a half of that once the world's traders have worked the same wants; the three fittings cost 45 d together; a cask sold at home clears about 32 d | The pinch is meant: the first payment costs about two casks at Lundenwic prices, or the fittings, or a week of good legs. |
| The way out that is always there | four casks back to the steward at thirty | 120 d struck off, the hold empty. About the home price, well under Lundenwic's. Any four casks of Rhenish wine will do. |
| Where it is paid | at Dorestat, to the steward, on his card | By a click on the bar's debt, or the Tasks window, on any day up to the day; on the day he comes down to the boat himself. "Pay 20 d" and "Pay it all" go. |
| If the day goes by | nothing is added (his own line: "He asks back what he lent and not a penny over it") | The bar reads `First 120 d was due 11 May: 2 days late` in the warning colour. He comes down once a day while she lies at Dorestat. Each time he is put off he thinks worse of her (3 points), the flag `ch_house_debt_notch_missed` is set, and the "grace" choice on the count's day needs it not to be. |

What the count does on Michaelmas itself is still not ruled (`debt_day.md` section 1 says the same). This file does
not rule it.

### 2.3 The code

**The opening, and the steward's day.** The loader takes the new field; the director puts the opening in line; the
runner asks for it once, and from the first payment's day on has the steward come down once a day.

**`Assets/Kapman/Code/Core/Encounters/EncounterDef.cs`**

After:

```csharp
        public bool arrival;
```

add:

```csharp
        /// <summary>Opens a new game (Design/fixes/first_hour.md 2): put in line once, when the player has said who they
        /// are and she lies at home with the call there in progress, quietly. Not of the arrival set.</summary>
        public bool opening;
```

**`Assets/Kapman/Code/Core/Encounters/EncounterLoader.cs`**

In place of:

```csharp
            "id", "stage", "arrival", "when", "weight",
```

put:

```csharp
            "id", "stage", "arrival", "opening", "when", "weight",
```

After:

```csharp
            c.arrival = Json.Bool(o, "arrival");
```

add:

```csharp
            c.opening = Json.Bool(o, "opening");
```

After:

```csharp
                errors.Add($"{where}: only a harbour card comes on arrival.");
```

add:

```csharp
            if (c.opening && (c.arrival || c.stage != Stage.Harbour))
                errors.Add($"{where}: an opening card is a harbour card, and not of the arrival set.");
```

**`Assets/Kapman/Code/Game/Encounters/EncounterDirector.cs`**

Before:

```csharp
        /// <summary>The ship has cast off: the harbour cards still waiting go (the toll man does not follow her out), and the
```

add:

```csharp
        /// <summary>A new game has begun (the player has said who they are, and she lies at home with the call there in
        /// progress, quietly): the cards marked "opening" that hold here go in line, in the book's order, at most
        /// Scheduler.ArrivalMax of them. Nothing else comes: no arrival set, no visit draw. Returns the cards put in
        /// line (none when no call is in progress).</summary>
        public List<EncounterDef> Opening()
        {
            var queued = new List<EncounterDef>();
            if (!InCall)
                return queued;
            Prepare(state.callPort, Day, true);
            foreach (var c in book.cards)
            {
                if (queued.Count >= Scheduler.ArrivalMax)
                    break;
                if (!c.opening || c.stage != Stage.Harbour || !c.InMonth(Month) || !state.Ready(c, Day) || !c.when.Eval(vars))
                    continue;
                state.Enqueue(c.id);
                queued.Add(c);
            }
            return queued;
        }
```

**`Assets/Kapman/Code/Game/Encounters/EncounterRunner.cs`**

After:

```csharp
                HomeCall(d, lying);
```

add:

```csharp
                Opening(d, lying);
                FirstPaymentDay(d, lying);
```

Before:

```csharp
        /// <summary>The chart without the frame: a card waits while the title or the pause menu is up, or the market, the
```

add:

```csharp
        bool openingDone;                                                 // the chart: a new game's opening card is in line

        /// <summary>The chart, a new game: once the player has said who they are (Self.Chosen) and she lies at home with
        /// the call there in progress, the card that opens the game goes in line (Design/fixes/first_hour.md 2). Once.
        /// A saved game brings its own cards; a boat that has already sailed has no opening.</summary>
        void Opening(EncounterDirector d, bool lying)
        {
            if (openingDone || !homeLooked)
                return;
            if (ChartWorld.SaveLoaded || d.watch.Armed)
            {
                openingDone = true;
                return;
            }
            if (!Self.Chosen || !lying || !d.InCall)
                return;
            openingDone = true;
            d.Opening();
        }

        /// <summary>Where the first payment is made, the steward's card for it up to and on its day, and his card once
        /// the day has gone by (Design/fixes/first_hour.md 2.2).</summary>
        public const string FirstPaymentPort = "dorestad", FirstPaymentCard = "harbour.chain_house_debt.s01b_notch",
                            FirstPaymentLateCard = "harbour.chain_house_debt.s01c_late";

        double dunnedDay = -1.0, toldEve = -1.0, toldDay = -1.0;          // the chart: the game days he came down, and the days it was said

        /// <summary>The chart: a first payment stands. Its eve and its morning are said once each, wherever she is (when
        /// the five-speed clock is in, each also drops it to speed 1: ruling 26). From its day on, while she lies at
        /// the steward's port, he comes down for it once a day: on the day with the card of the day, after it with
        /// the card of the late day. Before the day the player goes to him (the Tasks window).</summary>
        void FirstPaymentDay(EncounterDirector d, bool lying)
        {
            var game = KapmanGame.Current;
            if (game == null || !Trade.Started || Trade.FirstDue <= 0.01 || !Self.Chosen)
                return;
            double left = Trade.FirstDaysLeft(game.Day), today = Math.Floor(game.Day);
            string at = GameData.Port(FirstPaymentPort)?.name ?? FirstPaymentPort;
            if (left == 1.0 && toldEve != today)
            {
                toldEve = today;
                Voyage.Say($"Tomorrow is the steward's day: {Trade.FirstDue:0} d, at {at}.");
            }
            if (left == 0.0 && toldDay != today)
            {
                toldDay = today;
                Voyage.Say($"Today is the steward's day: {Trade.FirstDue:0} d, at {at}.");
            }
            if (left > 0.0 || dunnedDay == today || !lying || !d.InCall || d.Current != null || d.Waiting || d.state.callPort != FirstPaymentPort)
                return;
            if (d.Offer(left == 0.0 ? FirstPaymentCard : FirstPaymentLateCard))
                dunnedDay = today;
        }
```

**The two verbs.** `debt ±n` and `due <deniers> in <days>`, through the same seam to the game as `silver`. Two
condition names for the cards: `debt_first` (what still stands of the first payment) and `debt_first_days` (whole
days to its day).

**`Assets/Kapman/Code/Core/Encounters/IGameEffects.cs`**

Before:

```csharp
        /// <summary>Units of a good put in (+) or taken out of (-) the hold.</summary>
```

add:

```csharp
        /// <summary>The debt changed by so many deniers (below 0: struck off the tally; the purse is not touched: the
        /// option's cost took the silver or the goods). A first payment that stands is paid down first.</summary>
        string Debt(double deniers);
        /// <summary>A part of the debt falls due in so many days (the first notch on the tally). 0 deniers: none stands.</summary>
        string Due(double deniers, double days);
```

**`Assets/Kapman/Code/Core/Encounters/Effects.cs`**

After:

```csharp
                case "silver": ctx.Say(g?.Silver(n)); break;
```

add:

```csharp
                case "debt": ctx.Say(g?.Debt(n)); break;
                case "due": ctx.Say(g?.Due(n, days)); break;
```

In place of:

```csharp
            "tongue",
        };
```

put:

```csharp
            "tongue", "debt", "due",
        };
```

In place of:

```csharp
                case "silver":
                case "health":
                case "morale":
                    if (argc == 1 && Number(t[1], out e.n))
```

put:

```csharp
                case "due":
                    // due <deniers> in <days>
                    if (argc == 3 && t[2] == "in" && Number(t[1], out e.n) && e.n >= 0 && Number(t[3], out e.days) && e.days >= 0)
                        return e;
                    why = "'due' is 'due <deniers> in <days>'";
                    return null;
                case "silver":
                case "debt":
                case "health":
                case "morale":
                    if (argc == 1 && Number(t[1], out e.n))
```

After:

```csharp
    /// silver ±n | goods &lt;good&gt; ±n | health ±f | morale ±f | men ±n
```

add:

```csharp
    /// debt ±n | due &lt;deniers&gt; in &lt;days&gt;            (the tally: struck off or added; a first payment cut on it)
```

**`Assets/Kapman/Code/Core/Encounters/Cond.cs`**

After:

```csharp
["debt_due_days"] = (VarType.Number, null),
```

add:

```csharp

            ["debt_first"] = (VarType.Number, null), ["debt_first_days"] = (VarType.Number, null),
```

**`Assets/Kapman/Code/Game/Encounters/EncounterGameVars.cs`**

After:

```csharp
                case "debt_due_days":
                    return Trade.Started ? Trade.DebtDue - Today : 0.0;
```

add:

```csharp
                case "debt_first":                                       // what is still to pay of the first payment (0: none stands)
                    return Trade.Started ? Trade.FirstDue : 0.0;
                case "debt_first_days":                                  // whole days to its day (0 on the day, below 0 after)
                    return Trade.Started ? Trade.FirstDaysLeft(Today) : 0.0;
```

**`Assets/Kapman/Code/Game/Encounters/GameEffects.cs`**

Before:

```csharp
        public string Goods(string good, int count)
        {
            var g = GameData.Good(good);
```

add:

```csharp
        public string Debt(double deniers)
        {
            if (!Trade.Started || deniers == 0.0)
                return null;
            if (deniers > 0.0)
            {
                Trade.Debt += deniers;
                return $"{Show(deniers)} more is cut on the tally. Owed: {Show(Trade.Debt)}.";
            }
            double off = Trade.PayDown(-deniers);
            if (off <= 0.0)
                return null;
            return Trade.Debt > 0.01 ? $"{Show(off)} is struck off the tally. Still owed: {Show(Trade.Debt)}."
                                     : $"{Show(off)} is struck off the tally. Nothing is owed.";
        }

        public string Due(double deniers, double days)
        {
            if (!Trade.Started)
                return null;
            Trade.CutFirst(deniers, Today + days);
            if (Trade.FirstDue <= 0.0)
                return null;
            double left = Trade.FirstDaysLeft(Today);
            return $"A first payment is cut on the tally: {Show(Trade.FirstDue)}, due in {left:0} {(left == 1.0 ? "day" : "days")}.";
        }
```

**The first payment in the trade's own state**, saved with the rest. `PayDown` is also what the old `PayDebt` now
calls, so a payment made any other way pays the first payment down first.

**`Assets/Kapman/Code/Game/Trade/Trade.cs`**

In place of:

```csharp
        public static double DebtDue { get; private set; }
```

put:

```csharp
        public static double DebtDue { get; private set; }
        /// <summary>A part of the debt that falls due before the rest (the first notch on the tally): the deniers of it
        /// still to pay, and the game day (whole) it is due. 0: none stands.</summary>
        public static double FirstDue { get; private set; }
        public static double FirstDueDay { get; private set; }

        /// <summary>The steward cuts a first payment on the tally: so many deniers of the debt, due on a game day (its
        /// hours are dropped: the whole day counts). 0 deniers: none stands any more. Never more than is owed.</summary>
        public static void CutFirst(double deniers, double day)
        {
            FirstDue = Math.Max(0.0, Math.Min(deniers, Debt));
            FirstDueDay = FirstDue > 0.0 ? Math.Floor(day) : 0.0;
        }

        /// <summary>So many deniers struck off the debt without touching the purse (a card's cost took the silver, or
        /// the goods). A first payment that stands is paid down first, and is gone when nothing of it is left. Returns
        /// what came off (never more than was owed).</summary>
        public static double PayDown(double deniers)
        {
            double off = Math.Max(0.0, Math.Min(deniers, Debt));
            Debt -= off;
            FirstDue = Math.Max(0.0, Math.Min(FirstDue - off, Debt));
            if (FirstDue < 0.01)
                CutFirst(0.0, 0.0);
            return off;
        }

        /// <summary>Whole days left to the first payment's day (0 on the day itself, below 0 after it); 0 with none standing.</summary>
        public static double FirstDaysLeft(double today) => FirstDue > 0.0 ? FirstDueDay - Math.Floor(today) : 0.0;
```

In place of:

```csharp
            DebtDue = today + rules.dueDays;
        }
```

put:

```csharp
            DebtDue = today + rules.dueDays;
            CutFirst(0.0, 0.0);                                           // (the steward cuts it, on the tally card)
        }
```

In place of:

```csharp
            ["debt"] = Debt, ["creditor"] = Creditor, ["due"] = DebtDue,
```

put:

```csharp
            ["debt"] = Debt, ["creditor"] = Creditor, ["due"] = DebtDue, ["first"] = FirstDue, ["first_day"] = FirstDueDay,
```

After:

```csharp
            DebtDue = Json.Num(d, "due");
```

add:

```csharp
            FirstDue = Json.Num(d, "first");                              // (a save from before the first payment: none stands)
            FirstDueDay = Json.Num(d, "first_day");
```

In place of:

```csharp
                return Fail(r, how);
            Debt -= deniers;
```

put:

```csharp
                return Fail(r, how);
            PayDown(deniers);
```

**The top bar.** The first payment is said before the whole. `debt_day.md` section 2e rewrites the second half of
`DebtLine` (the whole debt by its day): keep its text for `whole` and keep these lines round it.

**`Assets/Kapman/Code/Game/UI/Frame/FrameGame.cs`**

In place of:

```csharp
        /// <summary>"Owed 480 d, due in 100 days", "Owed 480 d, 3 days overdue", or "" with no debt.</summary>
        public static string DebtLine()
        {
            if (!Trade.Started || Trade.Debt <= 0.01)
                return "";
            var game = KapmanGame.Current;
            double due = game != null ? Trade.DebtDue - game.Day : 0.0;
            return $"Owed {Trade.Debt:0} d, " + (due >= 0.0 ? $"due in {due:0} days" : $"{-due:0} days overdue");
        }
```

put:

```csharp
        /// <summary>"Owed 480 d, due in 100 days", "Owed 480 d, 3 days overdue", or "" with no debt. While a first
        /// payment stands it comes first: "First 120 d by 11 May: 10 days. Owed 480 d, due in 151 days".</summary>
        public static string DebtLine()
        {
            if (!Trade.Started || Trade.Debt <= 0.01)
                return "";
            var game = KapmanGame.Current;
            double due = game != null ? Trade.DebtDue - game.Day : 0.0;
            string whole = $"Owed {Trade.Debt:0} d, " + (due >= 0.0 ? $"due in {due:0} days" : $"{-due:0} days overdue");
            string first = FirstLine();
            return first.Length > 0 ? first + ". " + whole : whole;
        }

        /// <summary>The first payment as the top bar says it: "First 120 d by 11 May: 10 days", "...: tomorrow",
        /// "...: today", "First 120 d was due 11 May: 2 days late"; "" when none stands.</summary>
        public static string FirstLine()
        {
            if (!Trade.Started || Trade.FirstDue <= 0.01)
                return "";
            var game = KapmanGame.Current;
            string day = ShortDate(Trade.FirstDueDay);
            double left = game != null ? Trade.FirstDaysLeft(game.Day) : 0.0;
            if (left > 1.0)
                return $"First {Trade.FirstDue:0} d by {day}: {left:0} days";
            if (left == 1.0)
                return $"First {Trade.FirstDue:0} d by {day}: tomorrow";
            if (left == 0.0)
                return $"First {Trade.FirstDue:0} d by {day}: today";
            return $"First {Trade.FirstDue:0} d was due {day}: {-left:0} {(left == -1.0 ? "day" : "days")} late";
        }

        /// <summary>The date of a game day without its year, "11 May".</summary>
        public static string ShortDate(double day)
        {
            var clock = LevelClock.Current;
            if (clock == null)
                return $"day {day:0}";
            var d = Calendar.At(clock.year, clock.startDay, day * 24.0);
            return $"{d.day} {d.MonthName}";
        }
```

**`Assets/Kapman/Code/Game/UI/Frame/FrameRoot.cs`**

In place of:

```csharp
                debt.EnableInClassList("warn", game != null && Trade.Debt > 0.01 && Trade.DebtDue - game.Day < 14.0);
```

put:

```csharp
                debt.EnableInClassList("warn", game != null && Trade.Debt > 0.01 &&
                                               (Trade.DebtDue - game.Day < 14.0 || (Trade.FirstDue > 0.01 && Trade.FirstDaysLeft(game.Day) < 3.0)));
```

**Paid at home, to the steward.** In the Tasks window the two buttons go and one row takes their place. On the top
bar the debt can be clicked. `fixes/orders.md` 4.4 replaces the whole of `TasksPanel.cs`; its draft keeps the block
below unchanged (its lines 722 to 729), so this edit goes into it as it stands. After it the private method `Pay` in
that file has no caller: take it out.

**`Assets/Kapman/Code/Game/UI/Frame/Panels/TasksPanel.cs`**

In place of:

```csharp
                if (here != null)
                {
                    var market = Markets.Of(here, game.Year, game.Day);
                    var part = FrameUi.Btn("Pay 20 d", () => Pay(market, 20.0));
                    var all = FrameUi.Btn("Pay it all", () => Pay(market, Trade.Debt));
                    part.SetEnabled(silver >= System.Math.Min(20.0, Trade.Debt));
                    all.SetEnabled(silver >= Trade.Debt);
                    body.Add(FrameUi.Row(part, all));
                }
                else
                    body.Add(FrameUi.P("It is paid at a market, in port."));
```

put:

```csharp
                if (Trade.FirstDue > 0.01)
                    body.Add(FrameUi.Ln("The first payment", FrameGame.FirstLine() + ". " +
                                        (silver >= Trade.FirstDue ? "You have the silver for it." : $"You are short of it by {Trade.FirstDue - silver:0} d."),
                                        FrameUi.Num($"{Trade.FirstDue:0} d")));
                // it is paid to the count's steward, at home, on his card: never from a button in any port
                var cards = EncounterRunner.Director;
                var home = GameData.Port(EncounterRunner.FirstPaymentPort);
                string card = cards == null || cards.Current != null ? null
                            : cards.CanOffer(EncounterRunner.FirstPaymentCard) ? EncounterRunner.FirstPaymentCard
                            : cards.CanOffer(EncounterRunner.FirstPaymentLateCard) ? EncounterRunner.FirstPaymentLateCard : null;
                if (here != null && here.id == EncounterRunner.FirstPaymentPort && card != null)
                    body.Add(FrameUi.Go("Take it to the count's steward", () =>
                    {
                        if (cards.Offer(card))
                            Say("You go up to the dam head, to the steward.");
                    }));
                else
                    body.Add(FrameUi.P("It is paid to the count's steward at " + (home != null ? home.name : "home") + ", and nowhere else."));
```

**`Assets/Kapman/Code/Game/UI/Frame/FrameRoot.cs`**

In place of:

```csharp
            topMid.Add(Figure(null, debt, "What you owe, and when it falls due."));
```

put:

```csharp
            topMid.Add(Figure(null, debt, "What you owe, and when it falls due. At home, click it to go to the count's steward."));
            debt.RegisterCallback<ClickEvent>(_ => ToTheSteward());
```

Before:

```csharp
        /// <summary>She moors: the Port workspace comes up. She casts off: the Sea workspace. Only at the change, so the
```

add:

```csharp
        /// <summary>The top bar's debt, clicked: at the steward's port, while a first payment stands, his card opens (the
        /// one way to pay it); anywhere else the log says where it is paid.</summary>
        void ToTheSteward()
        {
            if (!Trade.Started || Trade.Debt <= 0.01)
                return;
            var cards = EncounterRunner.Director;
            var here = FrameGame.PortHere;
            string home = GameData.Port(EncounterRunner.FirstPaymentPort)?.name ?? "home";
            if (Trade.FirstDue <= 0.01)
            {
                FrameLog.Add($"The debt is paid to the count at {home}, on his day.");
                return;
            }
            if (cards == null || here == null || here.id != EncounterRunner.FirstPaymentPort)
            {
                FrameLog.Add($"The first payment is made to the count's steward at {home}, and nowhere else.");
                return;
            }
            if (cards.Current != null)
                return;
            if (cards.Offer(EncounterRunner.FirstPaymentCard) || cards.Offer(EncounterRunner.FirstPaymentLateCard))
                FrameLog.Add("You go up to the dam head, to the steward.");
        }
```

### 2.4 The data

**`Game/Data/encounters/chain_house_debt_793.json`, the tally card.** After `"once": true,` add the line
`"opening": true,`. Its `lines` and `options` become the block below: one new line (`s01.first`), and in each of
the three choices `due 120 in 10`, the three fittings sent for at their ports (3.4), and the count's day in 151
days, not 100 (1 May to Michaelmas; if the content pass on dates has mended that line already, keep its number).

```json
   "lines": [
    { "who": "steward", "key": "chain_house_debt.s01.cut" },
    { "who": "steward", "key": "chain_house_debt.s01.day", "when": "year < 794 and month <= 9" },
    { "who": "steward", "key": "chain_house_debt.s01.day_late", "when": "year > 793 or month > 9" },
    { "who": "steward", "key": "chain_house_debt.s01.first" },
    { "who": "steward", "key": "chain_house_debt.s01.law" },
    { "who": "steward", "key": "chain_house_debt.s01.wants", "when": "standing >= known" }
   ],
   "options": [
    { "key": "take", "label_key": "chain_house_debt.s01.take", "label": "Take your half and say you will be there", "check": "true", "default": true,
      "effects": [ "flag ch_house_debt_1", "flag ch_house_debt_owed = 480", "npc steward feeling +2 took",
                   "due 120 in 10",
                   "news chain_house_debt.news_corn at dorestad",
                   "schedule harbour.first.tow_line.traiectum in 0 at traiectum",
                   "schedule harbour.first.lead.witla in 0 at witla",
                   "schedule harbour.first.reef_bands.walichrum in 0 at walichrum",
                   "schedule harbour.chain_house_debt.s02_rival in 20 at dorestad",
                   "schedule harbour.chain_house_debt.s03_michaelmas in 151 at dorestad" ] },
    { "key": "gift", "label_key": "chain_house_debt.s01.gift", "label": "Leave twelve pennies for the count's table, as the custom is", "check": "silver >= 12",
      "cost": "silver 12",
      "effects": [ "flag ch_house_debt_1", "flag ch_house_debt_owed = 480", "flag ch_house_debt_gift", "npc steward feeling +6 gift",
                   "due 120 in 10",
                   "news chain_house_debt.news_corn at dorestad",
                   "schedule harbour.first.tow_line.traiectum in 0 at traiectum",
                   "schedule harbour.first.lead.witla in 0 at witla",
                   "schedule harbour.first.reef_bands.walichrum in 0 at walichrum",
                   "schedule harbour.chain_house_debt.s02_rival in 20 at dorestad",
                   "schedule harbour.chain_house_debt.s03_michaelmas in 151 at dorestad" ] },
    { "key": "argue", "label_key": "chain_house_debt.s01.argue", "label": "Say the boat was never worth two pounds", "check": "true",
      "effects": [ "flag ch_house_debt_1", "flag ch_house_debt_owed = 480", "npc steward feeling -4 argued",
                   "due 120 in 10",
                   "news chain_house_debt.news_corn at dorestad",
                   "schedule harbour.first.tow_line.traiectum in 0 at traiectum",
                   "schedule harbour.first.lead.witla in 0 at witla",
                   "schedule harbour.first.reef_bands.walichrum in 0 at walichrum",
                   "schedule harbour.chain_house_debt.s02_rival in 20 at dorestad",
                   "schedule harbour.chain_house_debt.s03_michaelmas in 151 at dorestad" ] }
   ],
```

In the same file, the count's day, the choice `grace`: its `check` becomes
`"year < 794 and standing >= known and not flag.ch_house_debt_notch_missed"`.

**Two new cards, in the same file, straight after the tally.** Two and not one, because a choice's effects cannot
ask what day it is: before the day "Not today" costs nothing, after it "You have neither" is a miss.

```json
  {
   "id": "harbour.chain_house_debt.s01b_notch",
   "note": "Step 1b (Design/fixes/first_hour.md 2): the first payment the steward cut on the tally card, 120 d (ten dozen of the forty), due on the tenth day (11 May 793 from a start on 1 May). Never drawn and never scheduled: the player goes to him at home on any day up to the day (the Tasks window: EncounterDirector.Offer), and on the day itself he comes down to a boat lying at Dorestad (EncounterRunner.FirstPaymentDay). Two ways to settle: silver, or four casks of Rhenish wine at thirty a cask (about the home price in May: 39 d less the spread and the toll is 31.6 d). Putting it off before the day costs nothing. Steward: cast.hartwin.",
   "stage": "harbour",
   "when": "moored and port.id = dorestad and flag.ch_house_debt_1 and debt_first > 0 and debt_first_days >= 0",
   "weight": 0,
   "cooldown_days": 0,
   "actors": {
    "steward": { "role": "steward", "people": "frankish", "faith": "port", "tongue": "port", "age": [50, 60], "fixed": true, "note": "cast.hartwin" }
   },
   "title_key": "chain_house_debt.s01b.title",
   "title": "The first notch",
   "intention_key": "chain_house_debt.s01b.intention",
   "intention": "The steward's board at the dam head: a knife, the two halves of the hazel stick, and a bowl for silver.",
   "lines": [
    { "who": "steward", "key": "chain_house_debt.s01b.early", "when": "debt_first_days > 0" },
    { "who": "steward", "key": "chain_house_debt.s01b.day", "when": "debt_first_days = 0" }
   ],
   "options": [
    { "key": "pay", "label_key": "chain_house_debt.s01b.pay", "label": "Count out ten dozen pennies (120 d)", "check": "silver >= 120",
      "cost": "silver 120",
      "effects": [ "debt -120", "flag ch_house_debt_notch", "flag fh_done", "npc steward feeling +4 paid_first" ] },
    { "key": "wine", "label_key": "chain_house_debt.s01b.wine", "label": "Have four casks of the wine carried up to him (120 d off the stick)", "check": "hold.rhenish_wine >= 4",
      "cost": "goods rhenish_wine 4",
      "effects": [ "debt -120", "flag ch_house_debt_notch", "flag fh_done" ] },
    { "key": "later", "label_key": "chain_house_debt.s01b.later", "label": "Not today", "check": "true", "default": true,
      "effects": [ ] }
   ],
   "memory": {
    "pay": "paid the first notch in silver",
    "wine": "gave back the count's wine for the first notch",
    "later": "came to the board and paid nothing"
   }
  },
  {
   "id": "harbour.chain_house_debt.s01c_late",
   "note": "Step 1c (Design/fixes/first_hour.md 2): the first payment after its day has gone by. He comes down to a boat lying at Dorestad once a day while it stands (EncounterRunner.FirstPaymentDay), and the player may go to him (the Tasks window). Nothing is added to the sum for lateness (the ban on usury: s01.law). What a miss costs: the steward cools each time he is put off, and the grace road of step 3 shuts (flag ch_house_debt_notch_missed, read by the check of s03 'grace'). Steward: cast.hartwin.",
   "stage": "harbour",
   "when": "moored and port.id = dorestad and flag.ch_house_debt_1 and debt_first > 0 and debt_first_days < 0",
   "weight": 0,
   "cooldown_days": 0,
   "actors": {
    "steward": { "role": "steward", "people": "frankish", "faith": "port", "tongue": "port", "age": [50, 60], "fixed": true, "note": "cast.hartwin" }
   },
   "title_key": "chain_house_debt.s01c.title",
   "title": "The first notch, late",
   "intention_key": "chain_house_debt.s01c.intention",
   "intention": "The steward's board at the dam head. Your half of the stick lies on it with no notch cut.",
   "lines": [
    { "who": "steward", "key": "chain_house_debt.s01c.late" },
    { "who": "steward", "key": "chain_house_debt.s01c.again", "when": "flag.ch_house_debt_notch_missed" }
   ],
   "options": [
    { "key": "pay", "label_key": "chain_house_debt.s01c.pay", "label": "Count out ten dozen pennies (120 d)", "check": "silver >= 120",
      "cost": "silver 120",
      "effects": [ "debt -120", "flag ch_house_debt_notch", "flag ch_house_debt_notch_late", "flag fh_done", "npc steward feeling +1 paid_first_late" ] },
    { "key": "wine", "label_key": "chain_house_debt.s01c.wine", "label": "Have four casks of the wine carried up to him (120 d off the stick)", "check": "hold.rhenish_wine >= 4",
      "cost": "goods rhenish_wine 4",
      "effects": [ "debt -120", "flag ch_house_debt_notch", "flag ch_house_debt_notch_late", "flag fh_done" ] },
    { "key": "neither", "label_key": "chain_house_debt.s01c.neither", "label": "You have neither", "check": "true", "default": true,
      "effects": [ "flag ch_house_debt_notch_missed", "flag fh_done", "npc steward feeling -3 missed_first" ] }
   ],
   "memory": {
    "pay": "paid the first notch late",
    "wine": "gave back the count's wine, late",
    "neither": "came past the day with neither silver nor wine"
   }
  },
```

**The words**, for `Game/Data/writing/chain_house_debt.json` (the steward's) and `Game/Data/writing/rhine_frisia.json`
(the three fittings of 3.4), inside `"entries"`. Then run `Bake/content/merge_writing.py`, which is what puts them
in `writing.json`. They are drafts in the house's plain manner, for the user to overwrite. The `out_` keys are the
written end of each choice, named as `chain_pilots_secret` names its own (`out_read`, `out_tell`); no code shows
written ends yet (the reviewer's fourth change), so they wait for that builder.

```json
  { "key": "chain_house_debt.s01.first", "text": "Ten dozen of the forty come first, ten days from this morning. Sell the wine or carry it, that is your trade. On the tenth day I want the pennies on this board, or the four casks back at thirty apiece." },

  { "key": "chain_house_debt.s01b.title", "text": "The first notch" },
  { "key": "chain_house_debt.s01b.intention", "text": "The steward's board at the dam head: a knife, the two halves of the hazel stick, and a bowl for silver." },
  { "key": "chain_house_debt.s01b.early", "text": "You are before your day. I have nothing against that. Ten dozen pennies, or the four casks at thirty." },
  { "key": "chain_house_debt.s01b.day", "text": "This is the day. Ten dozen pennies, or the four casks at thirty." },
  { "key": "chain_house_debt.s01b.pay", "text": "Count out ten dozen pennies (120 d)" },
  { "key": "chain_house_debt.s01b.wine", "text": "Have four casks of the wine carried up to him (120 d off the stick)" },
  { "key": "chain_house_debt.s01b.later", "text": "Not today" },
  { "key": "chain_house_debt.s01b.out_pay", "text": "He counts them twice into the bowl, lays the two halves together and cuts one notch across both. Thirty dozen still stand on the stick." },
  { "key": "chain_house_debt.s01b.out_wine", "text": "Two of his men roll the casks up the dam. He cuts the notch across both halves and gives you back yours. Your hold is empty and thirty dozen still stand on the stick." },
  { "key": "chain_house_debt.s01b.out_later", "text": "He nods and puts the knife down." },

  { "key": "chain_house_debt.s01c.title", "text": "The first notch, late" },
  { "key": "chain_house_debt.s01c.intention", "text": "The steward's board at the dam head. Your half of the stick lies on it with no notch cut." },
  { "key": "chain_house_debt.s01c.late", "text": "The day has gone by. I told the count's table when it would be. Ten dozen pennies, or the four casks at thirty." },
  { "key": "chain_house_debt.s01c.again", "text": "You came to this board once already with nothing in your hands." },
  { "key": "chain_house_debt.s01c.pay", "text": "Count out ten dozen pennies (120 d)" },
  { "key": "chain_house_debt.s01c.wine", "text": "Have four casks of the wine carried up to him (120 d off the stick)" },
  { "key": "chain_house_debt.s01c.neither", "text": "You have neither" },
  { "key": "chain_house_debt.s01c.out_pay", "text": "He counts them twice and cuts the notch. He does not say it is late. Thirty dozen still stand on the stick." },
  { "key": "chain_house_debt.s01c.out_wine", "text": "His men roll the casks up the dam. He cuts the notch and gives you back your half without a word." },
  { "key": "chain_house_debt.s01c.out_neither", "text": "He adds nothing to the stick: the king forbids it. He turns it over once and puts it back on its peg. At Michaelmas he will remember who kept a day and who did not." },

  { "key": "rhine_frisia.traiectum.tow_line.title", "text": "A line on the rail" },
  { "key": "rhine_frisia.traiectum.tow_line.intention", "text": "On the strand under the wall a long bast line is drying over a rail. The ferryman is working an eye into its end." },
  { "key": "rhine_frisia.traiectum.tow_line.home", "text": "You came down with the stream and the wind behind you. Going home you will have neither, and four men at the oars go about as fast as a duck." },
  { "key": "rhine_frisia.traiectum.tow_line.price", "text": "A short mast forward and this line. My sons walk the bank with it and you steer. Fifteen pennies." },
  { "key": "rhine_frisia.traiectum.tow_line.buy", "text": "Buy the tow mast and the line (15 d)" },
  { "key": "rhine_frisia.traiectum.tow_line.no", "text": "Not this time" },
  { "key": "rhine_frisia.traiectum.tow_line.out_buy", "text": "The mast is stepped before the hour is out. On a river, when the wind is foul or gone, she will be towed at a walking pace." },
  { "key": "rhine_frisia.traiectum.tow_line.out_no", "text": "He goes on with the eye. The line will be here." },

  { "key": "rhine_frisia.witla.lead.title", "text": "A lead for the mouth" },
  { "key": "rhine_frisia.witla.lead.intention", "text": "The pilot's shed at the Maas mouth. Leads hang from a beam by their lines, each with a pinch of tallow in its foot." },
  { "key": "rhine_frisia.witla.lead.bottom", "text": "Below here the bottom moves every winter. Without a lead you cross the flats a pole's length at a time, and you sit a tide on them all the same." },
  { "key": "rhine_frisia.witla.lead.yours", "text": "That one at your belt I know by its knots. Keep it. You need none of mine." },
  { "key": "rhine_frisia.witla.lead.price", "text": "Eighteen pennies. The line is knotted at the fathom and the knots are true." },
  { "key": "rhine_frisia.witla.lead.buy", "text": "Buy a lead and line (18 d)" },
  { "key": "rhine_frisia.witla.lead.no", "text": "Not this time" },
  { "key": "rhine_frisia.witla.lead.out_buy", "text": "He arms it with tallow for you. Over flats and coming in to a strange strand she need not creep now: the lead tells the bottom." },
  { "key": "rhine_frisia.witla.lead.out_no", "text": "He hangs it back on the beam." },

  { "key": "rhine_frisia.walichrum.reef_bands.title", "text": "Three rows of cords" },
  { "key": "rhine_frisia.walichrum.reef_bands.intention", "text": "On the creek strand a sail lies spread on the shingle, and someone with a sail needle is sewing short cords across it in rows." },
  { "key": "rhine_frisia.walichrum.reef_bands.third", "text": "Outside the islands it comes on to blow, and you have all of your sail or none of it. With these you have half." },
  { "key": "rhine_frisia.walichrum.reef_bands.price", "text": "Three rows, twelve pennies. Done before the tide." },
  { "key": "rhine_frisia.walichrum.reef_bands.buy", "text": "Have the cords sewn on her sail (12 d)" },
  { "key": "rhine_frisia.walichrum.reef_bands.no", "text": "Not this time" },
  { "key": "rhine_frisia.walichrum.reef_bands.out_buy", "text": "The cords are on by the turn of the tide. In a hard wind she can go on under a small sail where she would have lain to." },
  { "key": "rhine_frisia.walichrum.reef_bands.out_no", "text": "The needle goes on. There are other sails on the strand." },
```

### 2.5 With the five-speed clock

- `clock_five_speeds.md` 4.3 has two drops for the debt: a week before its day (to speed 2) and the day before (to
  speed 1), both reckoned from `Trade.DebtDue`. **Reckon them from the nearer of the two days.** Where its
  `ClockDrops` works out the days left to the debt, use
  `Trade.FirstDue > 0.01 ? Math.Min(Trade.DebtDue, Trade.FirstDueDay) - day : Trade.DebtDue - day`. One line; not
  compiled together with that draft.
- The runner's two log lines ("Tomorrow is the steward's day", "Today is the steward's day") are said on the same
  game day at every speed, because they compare whole game days.
- The steward comes down once a game day while she lies at Dorestat. At speed 1 in port (the clock drops there by
  itself) that is once in 24 real minutes.
- An ordinary card waits a week and then takes its default (ruling 12). The card of the day's default is "Not
  today", which costs nothing; the late card's is "You have neither", which is the miss. So a player who lets the
  clock run over the steward for a week has missed, and the log says so (`fixes/card_clock.md`).

## 3. The river hour (the second change)

### 3.1 Why nothing pays on the river today

`Code/Game/Trade/Market.cs` line 99 gives a market its makes and wants by its port's **region**. Dorestat, Traiectum
and Witla are all `rhine_delta`: the same goods at the same usual price, less the spread and the toll. Walichrum
(`zeeland`) sells salt, cheese and madder, and none of the other three wants any of them. That is the finding of
`checks/goods_first_hour.md`: one paying run, wine to Lundenwic, and timber back.

### 3.2 A port's own makes and wants

**`Game/Data/goods_793.json`**: a new top-level block, beside `"regions"` and `"sources"` (the key order does not
matter). The loader skips any value that is not an object and any key under a port that is not `makes` or `wants`,
so the notes can stay.

```json
 "port_trade": {
  "_about": "A port's own making and wanting, where it differs from its region's (Design/fixes/first_hour.md 3): port id, then makes and wants, each a good id and a strength as in made and wanted (0: this port does not, though its region does). Read by GameData.LoadGoods into GoodDef.madeAt and wantedAt. The four river ports of the first hour each want one thing another of the four sells. Strengths are starting guesses, to tune by the soak.",
  "dorestad":  { "makes": { "wax": 1, "combs": 2, "tating_ware": 1 }, "wants": { "madder": 2 },
                 "_why": "wax: the wax seller of harbour.black_haired_merchant.dorestad. combs: the comb makers of the dig at Wijk bij Duurstede (from memory (?)). tating_ware: the tin-foiled jugs come down the Rhine and are found here (from memory (?)). madder: for the dyers of Frisian cloth (a guess (?))." },
  "traiectum": { "wants": { "wax": 2 },
                 "_why": "the bishop's churches and the school: the prior's store has cakes of wax stacked like loaves (places/traiectum.json, traiectum.cellar)." },
  "witla":     { "wants": { "combs": 2, "cheese": 2 },
                 "_why": "a beach market at the river's mouth where crews wait for a pilot and a wind: small wares and food (a guess (?))." },
  "walichrum": { "makes": { "madder": 2 }, "wants": { "tating_ware": 2 },
                 "_why": "madder: Zeeland's red, already made 1 in its region (well known later; for 793 a guess (?)). tating_ware: a rich beach market (a guess (?))." }
 },
```

**The code.** `GoodDef` gains the two tables and two lookups; the four places that ask a region now ask the port.
The world's traders and the word on a price read the same figures (`Market.UsualPrice`, `Lore.UsualPrice`), so a
boatman's "Traiectum pays well for wax" comes of itself.

**`Assets/Kapman/Code/Game/Data/GameData.cs`**

After:

```csharp
        public float WantedIn(string region) => region != null && wanted.TryGetValue(region, out var s) ? s : 0f;
```

add:

```csharp

        /// <summary>A port's own making and wanting of this good, where it differs from its region's (the goods table's
        /// "port_trade" block): port id to strength. A 0 there: this port does not, though its region does.</summary>
        public readonly Dictionary<string, float> madeAt = new Dictionary<string, float>(), wantedAt = new Dictionary<string, float>();
        /// <summary>How strongly a port makes it: its own figure if it has one, else its region's.</summary>
        public float MadeAt(PortDef p) => p == null ? 0f : p.id != null && madeAt.TryGetValue(p.id, out var s) ? s : MadeIn(p.region);
        /// <summary>How keenly a port wants it: its own figure if it has one, else its region's.</summary>
        public float WantedAt(PortDef p) => p == null ? 0f : p.id != null && wantedAt.TryGetValue(p.id, out var s) ? s : WantedIn(p.region);
```

In place of:

```csharp
                g.index = goods.Count;
                goods.Add(g);
                goodById[g.id] = g;
            }
        }
```

put:

```csharp
                g.index = goods.Count;
                goods.Add(g);
                goodById[g.id] = g;
            }
            // a port's own making and wanting ("port_trade": port id -> "makes" / "wants", each good id -> strength)
            foreach (var kv in Json.Obj(root, "port_trade") ?? new Dictionary<string, object>())
            {
                var own = kv.Value as Dictionary<string, object>;
                if (own == null)
                    continue;
                foreach (var m in Json.Obj(own, "makes") ?? new Dictionary<string, object>())
                    if (m.Value is double made && goodById.TryGetValue(m.Key, out var good))
                        good.madeAt[kv.Key] = (float)made;
                foreach (var w in Json.Obj(own, "wants") ?? new Dictionary<string, object>())
                    if (w.Value is double wanted && goodById.TryGetValue(w.Key, out var good))
                        good.wantedAt[kv.Key] = (float)wanted;
            }
        }
```

**`Assets/Kapman/Code/Game/Trade/Market.cs`**

In place of:

```csharp
                    good = g, made = g.MadeIn(port.region), wanted = g.WantedIn(port.region),
```

put:

```csharp
                    good = g, made = g.MadeAt(port), wanted = g.WantedAt(port),
```

**`Assets/Kapman/Code/Game/Known/Lore.cs`**

In place of:

```csharp
            float made = g.MadeIn(port.region), wanted = g.WantedIn(port.region);
```

put:

```csharp
            float made = g.MadeAt(port), wanted = g.WantedAt(port);
```

In place of:

```csharp
            if (rules == null || port == null || g == null || g.MadeIn(port.region) <= 0f)
```

put:

```csharp
            if (rules == null || port == null || g == null || g.MadeAt(port) <= 0f)
```

**`Assets/Kapman/Code/Game/Map/MapView.cs`**

In place of:

```csharp
                    float made = good.MadeIn(def.region), wanted = good.WantedIn(def.region);
```

put:

```csharp
                    float made = good.MadeAt(def), wanted = good.WantedAt(def);
```

**The start cargo.** The quernstones had no buyer nearer than Lundenwic and filled the room the river needs, and
the ale never fitted. Four casks and a sack of bread leave 78 kg.

**`Assets/Kapman/Code/Game/Chart/ChartWorld.cs`**

In place of:

```csharp
                foreach (var (id, n) in new[] { ("rhenish_wine", 4), ("quernstones", 2), ("ships_bread", 1), ("ale", 1) })
```

put:

```csharp
                foreach (var (id, n) in new[] { ("rhenish_wine", 4), ("ships_bread", 1) })   // (78 kg of room left for the river's small parcels)
```

### 3.3 The four wants, and the six legs

Usual prices in May 793 on a table nobody has traded on yet, by `Market.cs`'s own rule (the market asks a tenth
over its price and pays a tenth under; the Frankish toll takes a tenth of a sale). "Room" is hold space, kg times
bulk; 78 are free.

| Port | Wants of its own | Sold at | A unit, room | Asked there | Paid here, after the toll | Clear a unit |
|---|---|---|---|---|---|---|
| Traiectum | Beeswax (`wax`, 2) | Dorestat (makes 1) | cake, 10 | 35.2 d | 45.4 d | **10.2 d** |
| Witla | Antler combs (`combs`, 2) | Dorestat (makes 2) | dozen, 2 | 8.6 d | 13.6 d | **5.0 d** |
| Walichrum | Tating-ware jugs (`tating_ware`, 2) | Dorestat (makes 1) | crate of 6, 20 | 21.1 d | 27.2 d | **6.1 d** |
| Witla | Cheese (`cheese`, 2) | Walichrum (its region makes 2) | wheel, 12 | 3.4 d | 5.4 d | **2.0 d** |
| Dorestat | Madder (`madder`, 2) | Walichrum (makes 2; its region 1) | sack, 30 | 8.6 d | 13.6 d | **5.0 d** |

The wax agrees with a card that is already written: the wax seller at Dorestat asks thirty "and I am robbing
myself. The church pays forty and complains" (`harbour.black_haired_merchant.dorestad`), and warns of the prior at
Traiectum.

| Leg | Km | Game hours | Carries | Clear, cold table | Teaches |
|---|---|---|---|---|---|
| 1. Dorestat to Traiectum | 30 | 3.5 to 7.5 | 2 cakes of wax (70 d laid out) | about 17 d | a want; the toll; the second unit pays less than the first |
| 2. Traiectum to Dorestat | 30 | 3.5 to 7.5 | the tow line, if bought | nothing; in a foul wind 3 to 4 hours saved | a thing bought changes a leg |
| 3. Dorestat to Witla | 111 | 15 to 25 | combs (and the jugs for leg 4) | 5 d a dozen: about 20 to 25 d on five | the purse, not the hold, is the limit; a night under way |
| 4. Witla to Walichrum | 55 (my sum) | 6 to 14 (my sum) | 2 or 3 crates of jugs | about 12 to 17 d | salt water; a beach market |
| 5. Walichrum to Witla | 55 | 6 to 14 | cheese | about 6 to 12 d on three to six wheels | small things in the room that is left |
| 6. Witla to Dorestat | 111 | 15 to 25 | 2 sacks of madder | about 10 d | the way home against the wind; the steward |

About 65 to 80 d in all on a cold table, laid out from a purse of 86 d that also buys bread and, if she chooses,
45 d of fittings. **These are my sums on usual prices, not a run.** Once the world's traders work the markets from
1 April (ruling 14, `fixes/prerun.md`) they will find the same wants, and what is left to her will be nearer a
third to a half of that: the scale `money.md` calls right (a laden leg 13 d in the mean). That is why the first
payment bites.

**What the soak must show before the numbers are trusted** (`Bake/economy/soak.py` does not know `port_trade`: give
its market the same two lookups first). With the traders at work since 1 April, from 1 May: (a) on each of legs 1,
3, 4, 5 and 6 a carry that clears 4 d or more on what the purse can lay out, in 8 seeded starts of 10; (b) the
sensible player who works the river and keeps at least two casks for Lundenwic has 120 d in silver at Dorestat by
day 10 in 50 to 70 starts of 100. Over 85: make the first payment 144 d (twelve dozen). Under 35: make it 96 d
(eight dozen), or make Traiectum want wax 3. The dial is each want's strength, 1 to 3.

### 3.4 Three things to buy, each of which changes a leg

They are in `Game/Data/story/ship_upgrades_793.json` already, at these prices. Fitted is the cards' flag
`unlock_upgrade_<id>`, as that file says. Nothing reads those flags today.

| Thing | Where, who | Price | What it does on the chart | The leg it changes |
|---|---|---|---|---|
| A tow mast and line (`tow_gear`) | Traiectum: the ferryman (`traiectum.aldlef`), whose line "drying over a rail" is in the place file | 15 d | On a river leg she is never slower than 3 knots. Beating she makes 1.83 and at the oars 1.75 | legs 2 and 6, and every river leg after, when the wind is foul or gone: 30 km in 5.4 h, not 8.8; 111 km in 20 h, not 33 |
| A lead and line (`sounding_line`) | Witla: Gerlef the pilot (`witla.gerlef`) | 18 d | Without it she makes 2 knots at most over the flats (lanes of kind `Flats`); with it her own speed | the crossing's 15 km of Thames flats (`crossing.md`): about 4 hours without, under 2 with. Which other legs of the twelve ports cross flats was not read from the lanes |
| Reef bands (`reef_bands`) | Walichrum: a sail-sewer on the creek strand (invented) | 12 d | Without them she lies to above 17 m/s (`Travel.Speed`). With them she lies to only above 21, and between the two makes half of what she made at 17 on that course | the crossing, on a day of hard wind: she goes on at about 2 to 3 knots where she would have stood still |

The steersman's child has the father's lead from the start (`story/self_793.json`): `Fittings.Has` counts it, and
Gerlef says "Keep it. You need none of mine." The data's own numbers that this leaves out, on purpose, for the
first release: the day at a yard that the tow gear and the reef bands take, the haulers' hire, and the tow's "14 km
a day" (that is against a stream; the chart reckons no stream, and at 14 km a day the tow would be slower than her
own oars). The three numbers in `Fittings` are mine, to tune.

**`Code/Game/Voyage/Fittings.cs`** (new):

```csharp
using System;
using Kapman.Game.Sim;
using Kapman.World;

namespace Kapman.Game
{
    /// <summary>
    /// THE BOAT'S SMALL FITTINGS on the chart (Design/fixes/first_hour.md 3.4): three things of
    /// Data/story/ship_upgrades_793.json that a river port sells for 12 to 18 d, each of which changes a leg.
    /// Fitted is the cards' flag unlock_upgrade_&lt;id&gt; (a card's choice sets it; it is saved with the cards' flags).
    ///   tow_gear       on a river she is never slower than the towing pace: men on the bank with the line
    ///   reef_bands     in a hard wind she goes on under a small sail where she would lie to
    ///   sounding_line  over the flats she does not creep: the lead tells the bottom
    /// The numbers are starting guesses, to tune by play.
    /// </summary>
    public static class Fittings
    {
        public const string TowGear = "tow_gear", ReefBands = "reef_bands", SoundingLine = "sounding_line";

        /// <summary>Knots made good at the tow line (a brisk walk on a towpath; the chart reckons no stream).</summary>
        public const double TowKnots = 3.0;
        /// <summary>Without reef bands she lies to above 17 m/s (Travel.Speed). With them she lies to only above this
        /// wind, and between the two makes this share of what she made at 17 m/s on the same course.</summary>
        public const double GaleWind = 17.0, ReefedLieTo = 21.0, ReefedShare = 0.5;
        /// <summary>Knots she makes at most over the flats (the lanes of kind Flats) with no lead aboard: a man in the
        /// bow with a pole.</summary>
        public const double CreepKnots = 2.0;

        /// <summary>Tests: what is fitted, in place of the flags (null: the flags).</summary>
        public static Func<string, bool> Override;

        /// <summary>Is it fitted? (The steersman's child has the father's lead and line from the start.)</summary>
        public static bool Has(string id)
        {
            if (Override != null)
                return Override(id);
            if (id == SoundingLine && Self.Past == "steersmans_child")
                return true;
            var cards = EncounterRunner.Director;
            var game = KapmanGame.Current;
            return cards != null && cards.state.Flag("unlock_upgrade_" + id, game != null ? game.Day : 0.0) > 0.0;
        }

        /// <summary>
        /// Her speed made good (real m/s) with her fittings. `made` and `lying` are what Travel.Speed gave for this course
        /// and wind; `water` is the kind of the leg she sails (LaneGraph.Sea ... Approach; 255: no charted leg).
        /// </summary>
        public static double Made(ShipClassDef c, double made, ref bool lying, double courseDeg, double windFrom, double windSpeed, byte water)
        {
            Towing = Reefed = Creeping = false;
            if (lying && c != null && windSpeed <= ReefedLieTo && Has(ReefBands))
            {
                made = Travel.Speed(c, courseDeg, windFrom, GaleWind, out _) * ReefedShare;
                lying = false;
                Reefed = true;
            }
            if (lying)
                return made;
            if (water == LaneGraph.River && made < TowKnots * SimUnits.KnotMs && Has(TowGear))
            {
                made = TowKnots * SimUnits.KnotMs;
                Towing = true;
            }
            if (water == LaneGraph.Flats && made > CreepKnots * SimUnits.KnotMs && !Has(SoundingLine))
            {
                made = CreepKnots * SimUnits.KnotMs;
                Creeping = true;
            }
            return made;
        }

        /// <summary>What her fittings, or the want of one, did at the last look (the player's ship only).</summary>
        public static bool Towing { get; private set; }
        public static bool Reefed { get; private set; }
        public static bool Creeping { get; private set; }

        /// <summary>The same in words, for the line under the ship ("" when nothing of it is so).</summary>
        public static string Words => Towing ? "towed from the bank" : Reefed ? "under a reefed sail" : Creeping ? "feeling her way, no lead aboard" : "";

        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            Override = null;
            Towing = Reefed = Creeping = false;
        }
    }
}
```

**The hooks.** Her speed on the chart goes through `Fittings.Made`; the line under the ship says what it did.

**`Assets/Kapman/Code/Game/Chart/ChartBody.cs`**

Before:

```csharp
                    Sheltering = lying;
```

add:

```csharp
                    // her own small fittings (Design/fixes/first_hour.md 3.4): the tow line on a river, reef bands in a hard wind
                    made = Fittings.Made(cls, made, ref lying, Mathf.Repeat(want, 360f), WindFrom, Wind, Course.IsSet ? Course.LegKind(Mathf.Min(Course.Next, Course.Points.Count - 1)) : (byte)255);
```

**`Assets/Kapman/Code/Game/Voyage/Course.cs`**

After:

```csharp
        public static bool LegIsRiver(int point) => point >= 0 && point < Kinds.Count && Kinds[point] == LaneGraph.River;
```

add:

```csharp

        /// <summary>The kind of water of the leg that ends at point `point` of the course (LaneGraph.Sea ... Approach);
        /// 255 where there is none.</summary>
        public static byte LegKind(int point) => point >= 0 && point < Kinds.Count ? Kinds[point] : (byte)255;
```

**`Assets/Kapman/Code/Game/UI/Frame/FrameRoot.cs`**

In place of:

```csharp
                           : (river ? "On the river" : "At sea") + (bound != null ? ", bound for " + bound : "");
```

put:

```csharp
                           : (river ? "On the river" : "At sea") + (bound != null ? ", bound for " + bound : "") +
                             (Fittings.Words.Length > 0 ? ": " + Fittings.Words : "");
```

`fixes/risk_is_real.md` replaces the same call in `ChartBody.cs` with the boat's own state (`Boat`: her sail set
full, shortened or down, and an order for a gale). When both are in: let `Fittings.Made` take the speed that draft
arrives at; drop the reef branch from `Fittings.Made`; and where that draft offers a shortened sail and "run under
shortened sail", ask `Fittings.Has(Fittings.ReefBands)` first. That is the book's own rule ("Reefed: seven tenths
of her speed; needs reef bands, 12 d", `GAME_MAKER.md` line 204). Not compiled together.

The measure and the Voyage window still give their two figures for her class alone (`FramePassage.Days`): they do
not know the fittings. To show the tow line there, the foul figure of a river way would take
`Math.Max(foul, Fittings.TowKnots * SimUnits.KnotMs)` when it is fitted; `FramePassage.Days(km)` does not know
today whether a way is a river, so that is not drafted.

**The three cards**, for `Game/Data/encounters/rhine_frisia_793.json` (a file the game already lists; a new file
would have to be added to the runner's list of card files in the scene). The tally sends for each at its port
(2.4), so each comes at the first call there, within a game hour of making fast, and "Not this time" sends for it
again in three days.

```json
  {
   "id": "harbour.first.tow_line.traiectum",
   "note": "The first hour's three small things to buy (Design/fixes/first_hour.md 3.4), one at each river port below home. Traiectum: the tow mast and line, tow_gear of story/ship_upgrades_793.json (15 d; had at traiectum). The ferryman is traiectum.aldlef, as in harbour.vecht_crossing.traiectum; the line drying over a rail is in places/traiectum.json (traiectum.strand). Never drawn: the tally card schedules it for the first call here, and 'Not this time' sends for it again in three days. What it does: Kapman.Game.Fittings (on a river she is never slower than the towing pace). The upgrade's day at a yard and its haulers' hire are not asked in the first release.",
   "stage": "harbour",
   "when": "moored and port.id = traiectum and not flag.unlock_upgrade_tow_gear",
   "weight": 0,
   "cooldown_days": 0,
   "actors": {
    "ferryman": { "role": "ferryman", "people": "frisian", "faith": "christian", "tongue": "port", "age": [38, 67], "fixed": true, "sex": "m", "note": "traiectum.aldlef" }
   },
   "title_key": "rhine_frisia.traiectum.tow_line.title",
   "title": "A line on the rail",
   "intention_key": "rhine_frisia.traiectum.tow_line.intention",
   "intention": "On the strand under the wall a long bast line is drying over a rail. The ferryman is working an eye into its end.",
   "lines": [
    { "who": "ferryman", "key": "rhine_frisia.traiectum.tow_line.home" },
    { "who": "ferryman", "key": "rhine_frisia.traiectum.tow_line.price" }
   ],
   "options": [
    { "key": "buy", "label_key": "rhine_frisia.traiectum.tow_line.buy", "label": "Buy the tow mast and the line (15 d)", "check": "silver >= 15", "cost": "silver 15",
      "effects": [ "flag unlock_upgrade_tow_gear", "npc ferryman feeling +2 bought_line" ] },
    { "key": "no", "label_key": "rhine_frisia.traiectum.tow_line.no", "label": "Not this time", "check": "true", "default": true,
      "effects": [ "schedule harbour.first.tow_line.traiectum in 3 at traiectum" ] }
   ],
   "memory": {
    "buy": "bought a tow line of him",
    "no": "looked at the tow line and left it"
   }
  },
  {
   "id": "harbour.first.lead.witla",
   "note": "The first hour's three small things to buy (Design/fixes/first_hour.md 3.4). Witla: a sounding lead and line, sounding_line of story/ship_upgrades_793.json (18 d; its other price, two pigs of lead and 6 d, is left for later: lead is sold at Lundenwic). The pilot is witla.gerlef (cast.gerlef), as in harbour.pilots_shed.witla. The steersman's child has the father's lead already (story/self_793.json, the past's chest thing): the pilot says so, and Kapman.Game.Fittings counts it. Never drawn: scheduled by the tally card. What it does: Fittings (over flats and in an approach she creeps without one).",
   "stage": "harbour",
   "when": "moored and port.id = witla and not flag.unlock_upgrade_sounding_line",
   "weight": 0,
   "cooldown_days": 0,
   "actors": {
    "pilot": { "role": "pilot", "people": "frisian", "faith": "christian", "tongue": "port", "age": [48, 67], "fixed": true, "sex": "m", "note": "witla.gerlef (cast.gerlef)" }
   },
   "title_key": "rhine_frisia.witla.lead.title",
   "title": "A lead for the mouth",
   "intention_key": "rhine_frisia.witla.lead.intention",
   "intention": "The pilot's shed at the Maas mouth. Leads hang from a beam by their lines, each with a pinch of tallow in its foot.",
   "lines": [
    { "who": "pilot", "key": "rhine_frisia.witla.lead.bottom" },
    { "who": "pilot", "key": "rhine_frisia.witla.lead.yours", "when": "me.past = steersmans_child" },
    { "who": "pilot", "key": "rhine_frisia.witla.lead.price", "when": "me.past != steersmans_child" }
   ],
   "options": [
    { "key": "buy", "label_key": "rhine_frisia.witla.lead.buy", "label": "Buy a lead and line (18 d)", "check": "silver >= 18", "cost": "silver 18",
      "effects": [ "flag unlock_upgrade_sounding_line", "npc pilot feeling +2 bought_lead" ] },
    { "key": "no", "label_key": "rhine_frisia.witla.lead.no", "label": "Not this time", "check": "true", "default": true,
      "effects": [ "schedule harbour.first.lead.witla in 3 at witla" ] }
   ],
   "memory": {
    "buy": "bought a lead and line of him",
    "no": "looked at the leads and left them"
   }
  },
  {
   "id": "harbour.first.reef_bands.walichrum",
   "note": "The first hour's three small things to buy (Design/fixes/first_hour.md 3.4). Walichrum: reef bands, reef_bands of story/ship_upgrades_793.json (12 d; reef points on a sail of these years are reconstructed, not found: that file says so). Sewn on the creek strand (places/walichrum.json, walichrum.strand), by an invented sail-sewer. Never drawn: scheduled by the tally card. What it does: Fittings (in a hard wind she goes on under a small sail where she would lie to). The upgrade's day at a yard is not asked in the first release.",
   "stage": "harbour",
   "when": "moored and port.id = walichrum and not flag.unlock_upgrade_reef_bands",
   "weight": 0,
   "cooldown_days": 0,
   "actors": {
    "sewer": { "role": "craftsman", "people": "frisian", "faith": "port", "tongue": "port", "age": [30, 60], "fixed": true }
   },
   "title_key": "rhine_frisia.walichrum.reef_bands.title",
   "title": "Three rows of cords",
   "intention_key": "rhine_frisia.walichrum.reef_bands.intention",
   "intention": "On the creek strand a sail lies spread on the shingle, and someone with a sail needle is sewing short cords across it in rows.",
   "lines": [
    { "who": "sewer", "key": "rhine_frisia.walichrum.reef_bands.third" },
    { "who": "sewer", "key": "rhine_frisia.walichrum.reef_bands.price" }
   ],
   "options": [
    { "key": "buy", "label_key": "rhine_frisia.walichrum.reef_bands.buy", "label": "Have the cords sewn on her sail (12 d)", "check": "silver >= 12", "cost": "silver 12",
      "effects": [ "flag unlock_upgrade_reef_bands", "npc sewer feeling +2 bought_bands" ] },
    { "key": "no", "label_key": "rhine_frisia.walichrum.reef_bands.no", "label": "Not this time", "check": "true", "default": true,
      "effects": [ "schedule harbour.first.reef_bands.walichrum in 3 at walichrum" ] }
   ],
   "memory": {
    "buy": "had reef bands sewn on the sail",
    "no": "looked at the reef bands and left them"
   }
  },
```

Their words are in 2.4.

### 3.5 Lundenwic, the hour's last act

Nothing new is built for it. What this file changes about it: the wine is hers to sell only if the first payment is
made, or the day is risked; the reef bands and the lead are bought for this leg and nothing before it needs them;
and the hour ends on the first big sale, with the bar reading `Owed 360 d` or a day one or two off.

## 4. One screen for the first hour (the fifth change)

### 4.1 The rule

Until the first payment's card is answered (the cards' flag `fh_done`: paid, wine handed back, or missed), or the
fourteenth game day, whichever is first, the left side is one screen in port and one at sea:

| | Top | Middle | Bottom |
|---|---|---|---|
| In port | Trade, large | Voyage, small | People here, medium |
| At sea | Voyage, large | The field, medium | Wind and tide, small |

No bar of workspaces and layouts under the windows. No arrows, no picker and no cross on a window's head: a window
has its name. Keys 1 to 4, R, C, T, N and X do nothing. The frame itself still changes from the one screen to the
other when she moors and casts off. Voyage is on both, so "pick a port in the Voyage window" is always in sight
(`checks/first_ten_minutes.md`, mends 2 and 3). The Port window is not on it: its one working row, "Talk to people",
is what People here is for. The Hold is not on it: Trade shows what is aboard. Tasks is not on it: the bar's debt
is clicked instead (2.3).

When it ends the player's own arrangement comes back if one is kept, else the four workspaces as today, and the log
says once: "The windows are yours to arrange from here: the bar under them, and the keys 1 to 4." A setting
(`kapman.frame.free` = 1 in PlayerPrefs; there is no settings page yet, as `clock_five_speeds.md` 4.3 also found)
gives the windows from the first minute in every game.

### 4.2 The code

`FrameWindows` holds the state, as it does for everything else about the windows, so the tests can drive it without
a scene. `FrameRoot` decides when, hides the bar and the heads' buttons, and keeps the fixed screen out of the
player's kept arrangement.

**`Assets/Kapman/Code/Game/UI/Frame/FrameWindows.cs`**

After:

```csharp
        /// <summary>The index of the workspaces a new player starts with: at sea, and in port.</summary>
        public const int SeaSpace = 0, PortSpace = 1;
```

add:

```csharp

        /// <summary>THE FIRST HOUR'S SCREEN (Design/fixes/first_hour.md 4): while true there is one screen in port and one
        /// at sea, and nothing to arrange: no workspace, layout, size, new window, close, step or pick changes anything.</summary>
        public bool Fixed { get; private set; }

        /// <summary>The fixed screen: at sea, and in port (the panels, top to bottom, and the size of each).</summary>
        public static readonly (string[] panels, int[] sizes)[] FixedSpaces =
        {
            (new[] { "voyage", "field", "sky" }, new[] { Large, Medium, Small }),
            (new[] { "trade", "voyage", "people" }, new[] { Large, Small, Medium }),
        };

        /// <summary>Puts the fixed screen up (true), or gives back the workspaces a new player starts with (false: the
        /// caller then restores the player's own arrangement, if one is kept).</summary>
        public void SetFixed(bool on)
        {
            if (Fixed == on)
                return;
            int was = Current;
            Fixed = false;
            Reset();
            if (on)
            {
                var known = panels();
                for (int i = 0; i < FixedSpaces.Length && i < spaces.Count; i++)
                {
                    var w = spaces[i];
                    w.layout = FrameLayout.Rows;
                    w.slots.Clear();
                    w.sizes.Clear();
                    w.focus = 0;
                    for (int k = 0; k < FixedSpaces[i].panels.Length; k++)
                    {
                        string id = FixedSpaces[i].panels[k];
                        if (!Has(known, id) || w.slots.Contains(id))
                            continue;
                        w.slots.Add(id);
                        if (FixedSpaces[i].sizes[k] != Medium)
                            w.sizes[id] = FixedSpaces[i].sizes[k];
                    }
                    Mend(w, known);
                }
            }
            Current = on ? Math.Min(Math.Max(was, 0), FixedSpaces.Length - 1) : 0;
            Fixed = on;
            Version++;
        }
```

In place of:

```csharp
            if (index < 0 || index >= spaces.Count || index == Current)
                return false;
```

put:

```csharp
            if (index < 0 || index >= spaces.Count || index == Current || (Fixed && index >= FixedSpaces.Length))
                return false;
```

In place of:

```csharp
            if (Space.layout == layout)
                return;
```

put:

```csharp
            if (Fixed || Space.layout == layout)
                return;
```

In place of:

```csharp
            var w = Space;
            if (index < 0 || index >= w.slots.Count)
                return;
            size = Math.Max(Small, Math.Min(Large, size));
```

put:

```csharp
            var w = Space;
            if (Fixed || index < 0 || index >= w.slots.Count)
                return;
            size = Math.Max(Small, Math.Min(Large, size));
```

In place of:

```csharp
            var w = Space;
            if (w.slots.Count >= MaxWindows)
                return false;
```

put:

```csharp
            var w = Space;
            if (Fixed || w.slots.Count >= MaxWindows)
                return false;
```

In place of:

```csharp
            if (w.slots.Count <= 1 || index < 0 || index >= w.slots.Count)
                return false;
```

put:

```csharp
            if (Fixed || w.slots.Count <= 1 || index < 0 || index >= w.slots.Count)
                return false;
```

In place of:

```csharp
            if (index < 0 || index >= w.slots.Count || all.Count == 0)
                return false;
```

put:

```csharp
            if (Fixed || index < 0 || index >= w.slots.Count || all.Count == 0)
                return false;
```

In place of:

```csharp
            if (index < 0 || index >= w.slots.Count || !Has(panels(), id) || w.slots[index] == id)
                return false;
```

put:

```csharp
            if (Fixed || index < 0 || index >= w.slots.Count || !Has(panels(), id) || w.slots[index] == id)
                return false;
```

**`Assets/Kapman/Code/Game/UI/Frame/FrameRoot.cs`**

After:

```csharp
        /// <summary>Tests and tools: start from the workspaces a new player has, and keep nothing.</summary>
        public static bool FreshLayout;
```

add:

```csharp
        /// <summary>The first hour's one fixed screen (Design/fixes/first_hour.md 4). Tests of the workspaces set it false.</summary>
        public static bool FixedFirstHour = true;
        /// <summary>The setting that gives the windows to the player from the first minute (1), in every game.</summary>
        public const string FreePref = "kapman.frame.free";
        /// <summary>The cards' flag that ends the fixed screen (the first payment's card sets it), and the game day by
        /// which it ends whatever was done.</summary>
        public const string FirstHourDoneFlag = "fh_done";
        public const double FirstHourLastDay = 14.0;
```

Before:

```csharp
            Workspace();
            KeepLayout();
```

add:

```csharp
            FirstHourScreen();
```

Before:

```csharp
        /// <summary>She moors: the Port workspace comes up. She casts off: the Sea workspace. Only at the change, so the
```

add:

```csharp
        /// <summary>Is the first hour's fixed screen up now: a new player's game, until the first payment's card is
        /// answered (the cards' flag), or the fourteenth day, or the player's own setting.</summary>
        public static bool FirstHourNow()
        {
            if (!FixedFirstHour || PlayerPrefs.GetInt(FreePref, 0) != 0)
                return false;
            var game = KapmanGame.Current;
            if (game == null)
                return true;
            if (game.Day >= FirstHourLastDay)
                return false;
            var cards = EncounterRunner.Director;
            return cards == null || cards.state.Flag(FirstHourDoneFlag, game.Day) <= 0.0;
        }

        /// <summary>Puts the fixed screen up, or takes it down and gives the player the windows (their own arrangement,
        /// where one is kept), and says so once in the log.</summary>
        void FirstHourScreen()
        {
            bool want = FirstHourNow();
            if (want == Windows.Fixed)
                return;
            Windows.SetFixed(want);
            pick = -1;
            lyingKnown = false;                                           // (the screen for where she lies comes up again)
            if (!want)
            {
                if (!(FrameSave.Layout.Length > 0 && Windows.Restore(FrameSave.Layout)) && keepLayout && !FreshLayout)
                    Windows.Restore(PlayerPrefs.GetString(Pref, ""));
                FrameLog.Add("The windows are yours to arrange from here: the bar under them, and the keys 1 to 4.");
            }
            keptVersion = Windows.Version;                                // (neither change is the player's own: not kept)
        }
```

In place of:

```csharp
            for (int i = 0; i < 4; i++)
                if (kb[Key.Digit1 + i].wasPressedThisFrame || kb[Key.Numpad1 + i].wasPressedThisFrame)
```

put:

```csharp
            for (int i = 0; i < 4 && !Windows.Fixed; i++)                 // (the fixed screen: no workspaces to go to)
                if (kb[Key.Digit1 + i].wasPressedThisFrame || kb[Key.Numpad1 + i].wasPressedThisFrame)
```

In place of:

```csharp
            // the bar: the workspaces, then the layouts and a new window
            wmbar.Clear();
```

put:

```csharp
            // the bar: the workspaces, then the layouts and a new window (none of it on the first hour's fixed screen)
            wmbar.Clear();
            wmbar.style.display = Windows.Fixed ? DisplayStyle.None : DisplayStyle.Flex;
            root.EnableInClassList("fixed", Windows.Fixed);
            if (Windows.Fixed)
                return;
```

In place of:

```csharp
            var head = Box("frame-win-head");
            head.Add(HeadButton(FrameGlyph.Kind.Left, "The panel before", () => { pick = -1; Windows.Step(index, -1); }));
            var title = new Button(() => pick = pick == index ? -1 : index) { tooltip = "Choose a panel for this window" };
            title.AddToClassList("frame-win-title");
            var name = new Label(panel != null ? panel.Title : id) { pickingMode = PickingMode.Ignore };
            name.AddToClassList("frame-win-name");
            title.Add(name);
            title.Add(new FrameGlyph(FrameGlyph.Kind.Down));
            head.Add(title);
            head.Add(HeadButton(FrameGlyph.Kind.Right, "The next panel", () => { pick = -1; Windows.Step(index, 1); }));
            if (w.slots.Count > 1)
                head.Add(HeadButton(FrameGlyph.Kind.Cross, "Close this window  [X]", () => { pick = -1; Windows.Close(index); }));
            win.Add(head);
```

put:

```csharp
            var head = Box("frame-win-head");
            var name = new Label(panel != null ? panel.Title : id) { pickingMode = PickingMode.Ignore };
            name.AddToClassList("frame-win-name");
            if (Windows.Fixed)
            {
                // the first hour's fixed screen: a window has its name and nothing to press
                var plain = Box("frame-win-title");
                plain.Add(name);
                head.Add(plain);
            }
            else
            {
                head.Add(HeadButton(FrameGlyph.Kind.Left, "The panel before", () => { pick = -1; Windows.Step(index, -1); }));
                var title = new Button(() => pick = pick == index ? -1 : index) { tooltip = "Choose a panel for this window" };
                title.AddToClassList("frame-win-title");
                title.Add(name);
                title.Add(new FrameGlyph(FrameGlyph.Kind.Down));
                head.Add(title);
                head.Add(HeadButton(FrameGlyph.Kind.Right, "The next panel", () => { pick = -1; Windows.Step(index, 1); }));
                if (w.slots.Count > 1)
                    head.Add(HeadButton(FrameGlyph.Kind.Cross, "Close this window  [X]", () => { pick = -1; Windows.Close(index); }));
            }
            win.Add(head);
```

In place of:

```csharp
            if (!keepLayout || FreshLayout || Windows.Version == keptVersion)
                return;
```

put:

```csharp
            if (!keepLayout || FreshLayout || Windows.Fixed || Windows.Version == keptVersion)
                return;
```

In place of (it stands 2 times in the file: each one):

```csharp
            if (keepLayout && !FreshLayout && Windows != null)
```

put:

```csharp
            if (keepLayout && !FreshLayout && Windows != null && !Windows.Fixed)
```

In place of:

```csharp
            else if (kb.nKey.wasPressedThisFrame) { pick = -1; Windows.Add(); }
            else if (kb.xKey.wasPressedThisFrame) { pick = -1; Windows.Close(Windows.Space.focus); }
```

put:

```csharp
            else if (kb.nKey.wasPressedThisFrame && !Windows.Fixed) { pick = -1; Windows.Add(); }
            else if (kb.xKey.wasPressedThisFrame && !Windows.Fixed) { pick = -1; Windows.Close(Windows.Space.focus); }
```

**`Game/UI/Frame.uss`** (not compile-checked: a stylesheet). After the rule `.frame-root .frame-win` (line 244):

```css
/* the first hour's fixed screen: the large window has three shares of the height, the medium two, the small one */
.frame-root.fixed .frame-win.size-2 { flex-grow: 3; }
.frame-root.fixed .frame-win.size-1 { flex-grow: 2; }
.frame-root.fixed .frame-win.size-0 { flex-grow: 1; }
/* a list that goes on below the window's edge says so with a line of the frame's gold, not with half a row */
.frame-root .frame-win-scroll.more-below { border-bottom-width: 2px; border-bottom-color: rgb(184, 150, 70); }
```

### 4.3 With brief faces (ruling 9)

`fixes/brief_faces.md` turns each window of the left column into one to four lines and opens the full panel as a
floating window over the chart. The fixed screen is one layer down from that (which panels, in which order, at
which size) and stands either way. When brief faces are in: its sizes become that draft's `FrameSize` (Small 1 line,
Medium 3, Large 4), so `FixedSpaces` reads Large, Small, Medium as now; and **on making fast during the first hour
the Trade panel's full window is opened by itself**, from under the top bar to the log, and shut on casting off.
That is how "the market given its height" is kept once the column holds brief lines. One call where that draft
opens a full window; not drafted here, because that code is not in the project yet.

### 4.4 No half row at a window's edge

The reviewer's con 11: "Ship's bread is cut through its second line". A window's list now shows whole rows only: a
row an edge would cut is not drawn, and keeps its place, so nothing jumps while scrolling. Compiled, never seen
running: UI Toolkit's layout events are the part to watch.

**`Code/Game/UI/Frame/FrameWholeRows.cs`** (new):

```csharp
using UnityEngine;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// NO HALF ROW AT A WINDOW'S EDGE (Design/fixes/first_hour.md 4.4; the newcomer's con 11: "Ship's bread is cut
    /// through its second line"). A window's list shows whole rows only: a row the window's lower or upper edge would
    /// cut through is not drawn (it keeps its place, so nothing jumps), and the window is marked "more-below" so the
    /// stylesheet can show that the list goes on. The rows are the children of the window's body (FrameUi.Ln, P, H).
    /// </summary>
    public static class FrameWholeRows
    {
        /// <summary>Holds a window's list to whole rows from now on: call once, when its ScrollView is made.</summary>
        public static void Hold(ScrollView scroll)
        {
            if (scroll == null)
                return;
            scroll.contentViewport.RegisterCallback<GeometryChangedEvent>(_ => Trim(scroll));
            scroll.contentContainer.RegisterCallback<GeometryChangedEvent>(_ => Trim(scroll));
            scroll.verticalScroller.valueChanged += _ => scroll.schedule.Execute(() => Trim(scroll));
        }

        /// <summary>Looks at every row once: whole and in sight, or out of sight, it is drawn; cut by an edge, it is not.</summary>
        public static void Trim(ScrollView scroll)
        {
            var view = scroll.contentViewport.worldBound;
            if (float.IsNaN(view.height) || view.height <= 0f)
                return;
            bool more = false;
            foreach (var body in scroll.contentContainer.Children())
                foreach (var row in body.Children())
                {
                    var r = row.worldBound;
                    if (float.IsNaN(r.height))
                        continue;
                    bool outside = r.yMax <= view.yMin || r.yMin >= view.yMax;
                    bool whole = r.yMin >= view.yMin - 1f && r.yMax <= view.yMax + 1f;
                    bool tooTall = r.height > view.height;               // (a row taller than the window is shown as far as it goes)
                    row.style.visibility = outside || whole || tooTall ? Visibility.Visible : Visibility.Hidden;
                    more |= r.yMax > view.yMax + 1f;
                }
            scroll.EnableInClassList("more-below", more);
        }
    }
}
```

**`Assets/Kapman/Code/Game/UI/Frame/FrameRoot.cs`**

After:

```csharp
            scroll.AddToClassList("frame-win-scroll");
```

add:

```csharp
            FrameWholeRows.Hold(scroll);                                  // (no row is shown cut through by the window's edge)
```

### 4.5 One word for a penny

The reviewer's con 14 counts three names in ten minutes: "34 d", "got 33 deniers", "3.4 g silver". Where each comes
from, as far as it was read:

- "d" is the frame's own (`FrameGame`: the top bar, the panels) and `ZoneDef.Show` for the Frankish zone
  (`Code/Game/Trade/TradeRules.cs` line 27).
- "g silver" is `ZoneDef.Show` for the zone of weighed silver (Ripa): the market's rows are in the place's
  reckoning while the purse on the bar is in deniers.
- "deniers" is in the sale's sentence: `Trade.Sell` builds "Sold {goods} for {price}...: took {coins}" with
  `Purse.LastCoins` (`Code/Game/Trade/Trade.cs`, about line 133). `Purse.LastCoins` was not read.

The rule to hold: on the frame every sum is said in "d", as the bar says it. A place's own reckoning goes in the
row's tooltip ("3.4 g of silver by the scales of Ripa"), and the coins that changed hands are named only there. Two
edits: `Purse.LastCoins` gives its figure as "33 d" where the frame asks, and the frame's market rows show
deniers with the zone's figure as the tip. Not drafted: neither place was read closely enough to give lines.

## 5. The tests

### 5.1 Edit mode

- **`Tests/EditMode/EncounterChartTests.cs`, `ANewGamesCallAtHomeBringsNobody`: no change.** The quiet call still
  brings nobody. The opening is asked for by a second call, and the new file tests that.
- **`Tests/EditMode/EffectsTests.cs`**, the tests' stand-in for the game, gains the two methods of the seam:

**`Assets/Kapman/Tests/EditMode/EffectsTests.cs`**

Before:

```csharp
        public string Goods(string good, int count) => Log(Inv($"goods {good} {count:+0;-0}"));
```

add:

```csharp
        public double debt = 480.0;
        public string Debt(double deniers)
        {
            debt += deniers;
            return Log(Inv($"debt {deniers:+0.##;-0.##}"));
        }
        public string Due(double deniers, double days) => Log(Inv($"due {deniers:0.##} in {days:0.##}"));
```

- **`Tests/EditMode/FirstHourTests.cs`** (new; ten tests). It must assert:
  1. an opening card comes when `Opening()` is called in a quiet call, and neither an arrival card nor a drawn one
     comes with it; with no call in progress none comes; it opens a game once;
  2. the loader refuses an opening card at sea, and one that is also of the arrival set;
  3. DATA: the tally is the opening card; each of its three choices cuts `due 120 in 10` and sends for the three
     fittings; "pay" and "wine" on both of the steward's cards strike 120 d off and set `fh_done`, and each has a
     cost; "Not today" is the card of the day's default and has no effect; "You have neither" is the late card's
     default, sets the missed flag, and adds nothing to the sum;
  4. the two verbs parse, and four wrong shapes are refused;
  5. the first payment is part of the debt with a day of its own: whole days, paid down first, kept in a save,
     never more than is owed;
  6. the top bar says the first payment before the whole, and only the whole once it is paid;
  7. a port may make and want what its region does not; its neighbours do not; a 0 of its own overrides its region;
  8. DATA: each of the five river carries of 3.3 clears between 1 d and 12 d a unit on usual prices in May;
  9. each fitting changes a leg, and only its own kind of leg (the figures of 3.4);
  10. the first hour's screen is fixed, nothing on it can be arranged, and the four workspaces come back after it.

  The two marked DATA pass only once the data of 2.4, 3.2 and 3.4 is in. Tests 4, 5, 9 and 10 are the ones the
  runner ran outside Unity.

```csharp
using System.Collections.Generic;
using System.Linq;
using Kapman.Encounters;
using Kapman.Game;
using Kapman.Game.Frame;
using Kapman.Game.Sim;
using Kapman.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>
    /// The first hour (Design/fixes/first_hour.md), without the scene: the card that opens a new game, the first payment
    /// the steward cuts on the tally (the verbs, the trade's own state, the words of the top bar), a port's own makes and
    /// wants, the three small fittings, and the one fixed screen.
    /// The two tests marked DATA pass only once the data of that write-up is in (the cards, the goods table's block).
    /// </summary>
    public class FirstHourTests
    {
        const string Toll = "harbour.tollman.frankish", Tally = "harbour.chain_house_debt.s01_tally", Notch = "harbour.chain_house_debt.s01b_notch",
                     Late = "harbour.chain_house_debt.s01c_late";

        static string Card(string id, string stage, string how) =>
            "{\"id\":\"" + id + "\",\"stage\":\"" + stage + "\"," + how + ",\"when\":\"true\",\"cooldown_days\":0," +
            "\"title_key\":\"" + id + ".title\",\"title\":\"A test\"," +
            "\"options\":[{\"key\":\"ok\",\"label_key\":\"" + id + ".ok\",\"label\":\"Go on\",\"check\":\"true\"}]}";

        static EncounterBook BookOf(out List<string> errors, params string[] cards)
        {
            var src = EncounterData.Sources();
            src.cards.Clear();
            src.cards.Add(("encounters/test_cards.json", "{\"cards\":[" + string.Join(",", cards) + "]}"));
            var book = EncounterLoader.Load(src);
            errors = book.errors;
            return book;
        }

        static EncounterDirector Director(EncounterBook book, FakeGame game = null)
        {
            var d = new EncounterDirector(book, 793, game ?? new FakeGame());
            d.portInfo = port => ("franks", "frisian");
            d.roll = () => 0.0;
            d.SetTime(0.25, 793, 5, 6);                                   // 1 May 793, early morning
            return d;
        }

        // ------------------------------------------------------------------ 1. the opening

        [Test]
        public void ANewGameOpensOnItsOpeningCardAndNothingElse()
        {
            var book = BookOf(out var errors,
                Card("test.arrival", "harbour", "\"arrival\":true"),
                Card("test.visit", "harbour", "\"weight\":50"),
                Card("test.opening", "harbour", "\"opening\":true,\"once\":true"));
            Assert.IsTrue(book.Ok, string.Join("\n", errors));
            Assert.IsTrue(book.Card("test.opening").opening);
            var d = Director(book);
            Assert.AreEqual(0, d.Opening().Count, "no call in progress: no opening");
            d.BeginCallQuietly("dorestad");
            Assert.IsFalse(d.Waiting, "the quiet call itself still brings nobody");
            var queued = d.Opening();
            Assert.AreEqual(1, queued.Count, "the opening card, and neither the arrival card nor a drawn one");
            Assert.AreEqual("test.opening", queued[0].id);
            var s = d.OpenNext();
            Assert.IsNotNull(s);
            Assert.AreEqual("test.opening", s.card.id);
            Assert.IsTrue(d.Choose(0).ok);
            Assert.IsTrue(d.Continue());
            Assert.IsFalse(d.Waiting, "nothing follows it");
            Assert.AreEqual(0, d.Opening().Count, "it opens a game once");
        }

        [Test]
        public void AnOpeningCardIsAHarbourCardOffTheArrivalSet()
        {
            Assert.IsFalse(BookOf(out var e1, Card("test.bad1", "sea", "\"opening\":true,\"rate\":1")).Ok);
            StringAssert.Contains("opening", string.Join("\n", e1));
            Assert.IsFalse(BookOf(out var e2, Card("test.bad2", "harbour", "\"opening\":true,\"arrival\":true")).Ok);
            StringAssert.Contains("opening", string.Join("\n", e2));
        }

        /// <summary>DATA: the tally is the opening card; each of its choices cuts 120 d due in 10 days and sends for the
        /// three fittings where they are sold; and the first payment's two cards settle it as the write-up says.</summary>
        [Test]
        public void TheTallyOpensTheGameAndCutsTheFirstPayment()
        {
            var game = new FakeGame();
            var d = Director(EncounterData.BookOf(Tally, Toll, Notch, Late,
                "harbour.chain_house_debt.s02_rival", "harbour.chain_house_debt.s03_michaelmas",
                "harbour.first.tow_line.traiectum", "harbour.first.lead.witla", "harbour.first.reef_bands.walichrum"), game);
            d.BeginCallQuietly("dorestad");
            var queued = d.Opening();
            Assert.AreEqual(1, queued.Count, "the tally, and not the toll man");
            Assert.AreEqual(Tally, queued[0].id);
            var s = d.OpenNext();
            Assert.AreEqual(Tally, s.card.id);
            foreach (var o in s.card.options)
            {
                var sources = o.effects.Select(e => e.source).ToList();
                CollectionAssert.Contains(sources, "due 120 in 10", o.key + " cuts the first payment");
                CollectionAssert.Contains(sources, "schedule harbour.first.tow_line.traiectum in 0 at traiectum", o.key);
                CollectionAssert.Contains(sources, "schedule harbour.first.lead.witla in 0 at witla", o.key);
                CollectionAssert.Contains(sources, "schedule harbour.first.reef_bands.walichrum in 0 at walichrum", o.key);
            }
            Assert.IsTrue(d.Choose(s.card.Option("take").index).ok);
            CollectionAssert.Contains(game.calls, "due 120 in 10");
            Assert.IsTrue(d.state.scheduled.Exists(x => x.card == "harbour.first.tow_line.traiectum" && x.port == "traiectum"));
            // the first payment's two cards (up to its day, and after it): either way of settling strikes 120 d off the
            // tally and ends the fixed screen; putting it off before the day does nothing; after the day it is a miss
            foreach (string id in new[] { Notch, Late })
                foreach (string key in new[] { "pay", "wine" })
                {
                    var option = d.book.Card(id).Option(key);
                    Assert.IsNotNull(option, id + " " + key);
                    var sources = option.effects.Select(e => e.source).ToList();
                    CollectionAssert.Contains(sources, "debt -120", id + " " + key);
                    CollectionAssert.Contains(sources, "flag fh_done", id + " " + key);
                    Assert.AreEqual(1, option.cost.Count, id + " " + key + " is paid for");
                }
            var later = d.book.Card(Notch).Option("later");
            Assert.IsTrue(later.isDefault, "left for its week, the card of the day pays nothing");
            Assert.AreEqual(0, later.effects.Count, "and before the day that costs nothing");
            var miss = d.book.Card(Late).Option("neither");
            Assert.IsTrue(miss.isDefault);
            var missed = miss.effects.Select(e => e.source).ToList();
            CollectionAssert.Contains(missed, "flag ch_house_debt_notch_missed");
            CollectionAssert.Contains(missed, "flag fh_done");
            Assert.IsFalse(missed.Exists(x => x.StartsWith("debt ") || x.StartsWith("silver ")), "nothing is added to the sum: the ban on usury stands");
        }

        // ------------------------------------------------------------------ 2. the verbs

        [Test]
        public void TheTallysVerbsParse()
        {
            string why = null;
            var debt = Effects.ParseVerb("debt -120", ref why);
            Assert.IsNotNull(debt, why);
            Assert.AreEqual(-120.0, debt.n, 1e-9);
            var due = Effects.ParseVerb("due 120 in 10", ref why);
            Assert.IsNotNull(due, why);
            Assert.AreEqual(120.0, due.n, 1e-9);
            Assert.AreEqual(10.0, due.days, 1e-9);
            Assert.IsNotNull(Effects.ParseVerb("due 0 in 0", ref why), "none stands any more");
            Assert.IsNull(Effects.ParseVerb("due 120", ref why));
            Assert.IsNull(Effects.ParseVerb("due -5 in 3", ref why));
            Assert.IsNull(Effects.ParseVerb("due 120 by 10", ref why));
            Assert.IsNull(Effects.ParseVerb("debt", ref why));
        }

        // ------------------------------------------------------------------ 3. the first payment in the trade's state

        [Test]
        public void TheFirstPaymentIsPartOfTheDebtWithADayOfItsOwn()
        {
            GameData.LoadGoods(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/goods_793.json"));
            var rules = TradeRules.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/trade_rules.json"));
            Trade.Begin(rules, 900f, 0.0);
            Assert.AreEqual(0.0, Trade.FirstDue, "none stands until the steward cuts it");
            Assert.AreEqual(0.0, Trade.FirstDaysLeft(3.0));
            Trade.CutFirst(120.0, 10.26);                                 // cut early on the first morning, for ten days on
            Assert.AreEqual(120.0, Trade.FirstDue, 1e-9);
            Assert.AreEqual(10.0, Trade.FirstDueDay, 1e-9, "the whole day counts");
            Assert.AreEqual(10.0, Trade.FirstDaysLeft(0.26), 1e-9);
            Assert.AreEqual(0.0, Trade.FirstDaysLeft(10.9), 1e-9, "all of the day itself is in time");
            Assert.AreEqual(-2.0, Trade.FirstDaysLeft(12.1), 1e-9);
            // paid in part: the first payment is paid down first
            Assert.AreEqual(50.0, Trade.PayDown(50.0), 1e-9);
            Assert.AreEqual(rules.startDebt - 50.0, Trade.Debt, 1e-9);
            Assert.AreEqual(70.0, Trade.FirstDue, 1e-9);
            // a saved game keeps it
            var saved = Trade.Capture();
            Trade.Begin(rules, 900f, 0.0);
            Trade.Restore(saved);
            Assert.AreEqual(70.0, Trade.FirstDue, 1e-9);
            Assert.AreEqual(10.0, Trade.FirstDueDay, 1e-9);
            // paid: none stands, and the rest of the payment comes off the whole
            Assert.AreEqual(100.0, Trade.PayDown(100.0), 1e-9);
            Assert.AreEqual(rules.startDebt - 150.0, Trade.Debt, 1e-9);
            Assert.AreEqual(0.0, Trade.FirstDue, 1e-9);
            Assert.AreEqual(0.0, Trade.FirstDueDay, 1e-9);
            // never more than is owed
            Trade.CutFirst(100000.0, 3.0);
            Assert.AreEqual(Trade.Debt, Trade.FirstDue, 1e-9);
            Assert.AreEqual(Trade.FirstDue, Trade.PayDown(100000.0), 1e-9);
            Assert.AreEqual(0.0, Trade.Debt, 1e-9);
            Assert.AreEqual(0.0, Trade.FirstDue, 1e-9);
        }

        [Test]
        public void TheTopBarSaysTheFirstPaymentBeforeTheWhole()
        {
            var rules = TradeRules.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/trade_rules.json"));
            Trade.Begin(rules, 900f, 0.0);
            Assert.AreEqual("", FrameGame.FirstLine());
            StringAssert.StartsWith("Owed 480 d", FrameGame.DebtLine());
            Trade.CutFirst(120.0, 10.0);
            // (no game and no clock here: the day is said as a number)
            StringAssert.StartsWith("First 120 d ", FrameGame.FirstLine());
            StringAssert.StartsWith("First 120 d ", FrameGame.DebtLine());
            StringAssert.Contains(". Owed 480 d", FrameGame.DebtLine());
            Trade.PayDown(120.0);
            StringAssert.StartsWith("Owed 360 d", FrameGame.DebtLine());
        }

        // ------------------------------------------------------------------ 4. a port's own makes and wants

        [Test]
        public void APortMayMakeAndWantWhatItsRegionDoesNot()
        {
            const string json = "{\"goods\":[{\"id\":\"wax\",\"name\":\"Beeswax\",\"value\":40,\"kg\":10,\"made\":{\"saxony\":2},\"wanted\":{\"rhineland\":3}}]," +
                                "\"port_trade\":{\"dorestad\":{\"makes\":{\"wax\":1}},\"traiectum\":{\"wants\":{\"wax\":2}},\"colonia\":{\"wants\":{\"wax\":0}},\"nowhere\":{\"wants\":{\"no_such_good\":2}}}}";
            try
            {
                GameData.LoadGoods(new TextAsset(json));
                var wax = GameData.Good("wax");
                var dorestad = new PortDef { id = "dorestad", region = "rhine_delta" };
                var traiectum = new PortDef { id = "traiectum", region = "rhine_delta" };
                var witla = new PortDef { id = "witla", region = "rhine_delta" };
                var colonia = new PortDef { id = "colonia", region = "rhineland" };
                var mogontia = new PortDef { id = "mogontia", region = "rhineland" };
                Assert.AreEqual(1f, wax.MadeAt(dorestad));
                Assert.AreEqual(0f, wax.WantedAt(dorestad));
                Assert.AreEqual(2f, wax.WantedAt(traiectum));
                Assert.AreEqual(0f, wax.MadeAt(traiectum));
                Assert.AreEqual(0f, wax.MadeAt(witla), "its neighbours in the region are as the region is");
                Assert.AreEqual(0f, wax.WantedAt(witla));
                Assert.AreEqual(3f, wax.WantedAt(mogontia), "a port with no figure of its own has its region's");
                Assert.AreEqual(0f, wax.WantedAt(colonia), "a 0 of its own: this port does not, though its region does");
                Assert.AreEqual(0f, wax.MadeAt(null));
            }
            finally
            {
                GameData.LoadGoods(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/goods_793.json"));
            }
        }

        /// <summary>DATA: each of the four river ports wants one thing of its own that another of the four sells, and on a
        /// table nobody has traded on yet each of those carries clears between 1 d and 12 d a unit after the spread and
        /// the toll (May 793).</summary>
        [Test]
        public void EachRiverPortHasAWantOfItsOwnThatARiverPortServes()
        {
            GameData.LoadGoods(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/goods_793.json"));
            GameData.LoadPorts(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/ports_793.json"));
            var rules = TradeRules.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/trade_rules.json"));
            foreach (var (good, from, to) in new[]
                     {
                         ("wax", "dorestad", "traiectum"), ("combs", "dorestad", "witla"), ("tating_ware", "dorestad", "walichrum"),
                         ("cheese", "walichrum", "witla"), ("madder", "walichrum", "dorestad"),
                     })
            {
                var g = GameData.Good(good);
                PortDef a = GameData.Port(from), b = GameData.Port(to);
                Assert.IsNotNull(g, good);
                Assert.Greater(g.MadeAt(a), 0f, $"{from} does not sell {good}");
                Assert.Greater(g.WantedAt(b), 0f, $"{to} does not want {good}");
                var buy = new Market(a, rules, 793, 0.0);
                var sell = new Market(b, rules, 793, 0.0);
                float asked = buy.UsualPrice(buy.Get(good), 5) * (1f + rules.spread * 0.5f);
                float paid = sell.UsualPrice(sell.Get(good), 5) * (1f - rules.spread * 0.5f) * (1f - sell.zone.toll);
                Assert.Greater(paid - asked, 1f, $"{good} from {from} to {to} does not pay: asked {asked:0.0}, paid {paid:0.0} after the toll");
                Assert.Less(paid - asked, 12f, $"{good} from {from} to {to} pays too well for a river hop: asked {asked:0.0}, paid {paid:0.0}");
            }
        }

        // ------------------------------------------------------------------ 5. the fittings

        [Test]
        public void EachFittingChangesALeg()
        {
            var boat = new ShipClassDef { id = "gislinge", hasSail = true, sailKn = 4.8f, maxKn = 6.5f, oarsKn = 3.5f, rowingHours = 6f, closestToWind = 58f, polar = 1f };
            var fitted = new HashSet<string>();
            Fittings.Override = fitted.Contains;
            try
            {
                // a light wind dead ahead: she rows, at 1.75 knots
                bool lying;
                double rowed = Travel.Speed(boat, 0.0, 0.0, 4.0, out lying);
                Assert.AreEqual(1.75 * SimUnits.KnotMs, rowed, 0.01);
                Assert.AreEqual(rowed, Fittings.Made(boat, rowed, ref lying, 0.0, 0.0, 4.0, LaneGraph.River), 1e-9, "no tow line: the oars");
                fitted.Add(Fittings.TowGear);
                Assert.AreEqual(Fittings.TowKnots * SimUnits.KnotMs, Fittings.Made(boat, rowed, ref lying, 0.0, 0.0, 4.0, LaneGraph.River), 1e-9, "towed on a river");
                Assert.AreEqual(rowed, Fittings.Made(boat, rowed, ref lying, 0.0, 0.0, 4.0, LaneGraph.Sea), 1e-9, "nobody tows her at sea");
                double fair = Travel.Speed(boat, 0.0, 225.0, 8.0, out lying);
                Assert.AreEqual(fair, Fittings.Made(boat, fair, ref lying, 0.0, 225.0, 8.0, LaneGraph.River), 1e-9, "a fair wind beats the tow");

                // a hard wind: without reef bands she lies to; with them she goes on, slowly, up to 21 m/s
                double gale = Travel.Speed(boat, 0.0, 225.0, 19.0, out lying);
                Assert.IsTrue(lying);
                Assert.AreEqual(0.0, Fittings.Made(boat, gale, ref lying, 0.0, 225.0, 19.0, LaneGraph.Sea), 1e-9);
                Assert.IsTrue(lying);
                fitted.Add(Fittings.ReefBands);
                double reefed = Fittings.Made(boat, gale, ref lying, 0.0, 225.0, 19.0, LaneGraph.Sea);
                Assert.IsFalse(lying, "reef bands: she goes on");
                Assert.Greater(reefed, 1.0 * SimUnits.KnotMs);
                Assert.Less(reefed, fair);
                Travel.Speed(boat, 0.0, 225.0, 22.0, out lying);
                Assert.AreEqual(0.0, Fittings.Made(boat, 0.0, ref lying, 0.0, 225.0, 22.0, LaneGraph.Sea), 1e-9);
                Assert.IsTrue(lying, "over 21 m/s nothing helps");

                // the flats: with no lead she creeps
                lying = false;
                Assert.AreEqual(Fittings.CreepKnots * SimUnits.KnotMs, Fittings.Made(boat, fair, ref lying, 0.0, 225.0, 8.0, LaneGraph.Flats), 1e-9);
                Assert.AreEqual(fair, Fittings.Made(boat, fair, ref lying, 0.0, 225.0, 8.0, LaneGraph.Approach), 1e-9, "only the flats: not the way in to a berth");
                fitted.Add(Fittings.SoundingLine);
                Assert.AreEqual(fair, Fittings.Made(boat, fair, ref lying, 0.0, 225.0, 8.0, LaneGraph.Flats), 1e-9, "the lead tells the bottom");
            }
            finally
            {
                Fittings.Override = null;
            }
        }

        // ------------------------------------------------------------------ 6. the one screen

        static readonly string[] Panels =
        {
            "voyage", "field", "sky", "trade", "port", "people", "hold", "crew", "tasks", "word", "chronicle", "event",
            "self", "bonds", "faith", "tongues", "ship", "house", "chest",
        };

        [Test]
        public void TheFirstHoursScreenIsFixed()
        {
            var w = new FrameWindows(() => Panels);
            Assert.IsFalse(w.Fixed);
            Assert.AreEqual(4, w.spaces.Count);
            w.SetFixed(true);
            Assert.IsTrue(w.Fixed);
            CollectionAssert.AreEqual(new[] { "voyage", "field", "sky" }, w.spaces[FrameWindows.SeaSpace].slots);
            CollectionAssert.AreEqual(new[] { "trade", "voyage", "people" }, w.spaces[FrameWindows.PortSpace].slots);
            Assert.AreEqual(FrameLayout.Rows, w.spaces[FrameWindows.PortSpace].layout);
            Assert.IsTrue(w.Go(FrameWindows.PortSpace), "the frame itself still goes from the sea's screen to the port's");
            Assert.AreEqual(FrameWindows.Large, w.SizeOf(0), "the market has its height");
            Assert.AreEqual(FrameWindows.Small, w.SizeOf(1));
            // nothing to arrange
            int version = w.Version;
            Assert.IsFalse(w.Go(2));
            Assert.IsFalse(w.Go(3));
            w.SetLayout(FrameLayout.Columns);
            w.SetLayout(FrameLayout.Tabs);
            w.SetSize(0, FrameWindows.Small);
            Assert.IsFalse(w.Add());
            Assert.IsFalse(w.Close(0));
            Assert.IsFalse(w.Step(0, 1));
            Assert.IsFalse(w.Set(0, "hold"));
            Assert.AreEqual(version, w.Version, "nothing moved");
            CollectionAssert.AreEqual(new[] { "trade", "voyage", "people" }, w.Space.slots);
            // hour two: the workspaces a new player had before
            w.SetFixed(false);
            Assert.IsFalse(w.Fixed);
            Assert.AreEqual(FrameWindows.Defaults.Length, w.spaces.Count);
            CollectionAssert.AreEqual(FrameWindows.Defaults[FrameWindows.PortSpace].panels, w.spaces[FrameWindows.PortSpace].slots);
            Assert.IsTrue(w.Go(2));
            Assert.IsTrue(w.Go(FrameWindows.SeaSpace));
            Assert.IsTrue(w.Add());
        }
    }
}
```

- Two tests that may notice the start cargo or the river's new carries, not read: anything in
  `Tests/EditMode/EconomyTests.cs` or `EconomyNightTwoTests.cs` that asserts nothing pays between two ports of the
  Rhine delta. None was found by a search for "quernstones" or "ale" in the play test.

### 5.2 Play mode

**`Tests/PlayMode/FirstHourPlay.cs`** (new; compiled with the play tests): the tally answered through the frame,
as a player would.

```csharp
using System;
using System.Collections;
using System.Linq;
using Kapman.Game;
using Kapman.Game.Frame;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// Steps of the first hour for the play tests (Design/fixes/first_hour.md 5): the card that opens a new game is
    /// answered through the frame, as a player would, before the test goes on to what it is about.
    /// </summary>
    public static class FirstHourPlay
    {
        public const string Tally = "harbour.chain_house_debt.s01_tally";

        /// <summary>
        /// Straight after "Begin" by a merchant whose home is Dorestat: the tally opens the game, holds the clock and
        /// has cut nothing yet. Takes the choice named (take, gift or argue), goes on, and checks that 120 d now stand
        /// on the top bar, due on the tenth day.
        /// </summary>
        public static IEnumerator TheTally(ChartPlay play, string option = "take", string shot = null)
        {
            yield return ChartPlay.Until(() => EncounterRunner.Director != null && EncounterRunner.Director.Current != null, 10f);
            var cards = EncounterRunner.Director;
            Assert.IsNotNull(cards != null ? cards.Current : null, "no card opened the new game: the tally should");
            Assert.AreEqual(Tally, cards.Current.card.id, "the new game did not open on the tally");
            Assert.IsTrue(PanelClock.Holding, "the tally holds the clock");
            Assert.AreEqual(0.0, Trade.FirstDue, 1e-9, "the first payment is cut by the choice, not before it");
            if (shot != null)
                yield return play.Shot(shot);
            string words = cards.Current.options.First(o => o.def.key == option).label;
            var choice = play.Choices(play.FrontFloat()).FirstOrDefault(c => c.words.EndsWith(words, StringComparison.Ordinal));
            Assert.IsNotNull(choice.button, "the tally has no choice '" + words + "'");
            yield return play.Click(choice.button, "the tally's choice '" + words + "'");
            yield return ChartPlay.Frames(20);
            Assert.IsTrue(cards.Current != null && cards.Current.Chosen, "the tally's choice was not taken");
            yield return play.ClickButton("Go on", play.FrontFloat());
            yield return ChartPlay.Frames(10);
            Assert.IsNull(cards.Current, "the tally did not shut");
            Assert.AreEqual(120.0, Trade.FirstDue, 1e-9, "the steward cut no first payment");
            Assert.AreEqual(10.0, Trade.FirstDueDay, 1e-9, "the first payment is not due on the tenth day");
            StringAssert.StartsWith("First 120 d by 11 May: 10 days. Owed 480 d", FrameGame.DebtLine(), "the top bar does not say the first payment's day");
            Assert.IsTrue(FrameRoot.Current.Windows.Fixed == FrameRoot.FixedFirstHour, "the first hour's screen is not as the test asked for it");
        }
    }
}
```

**`Tests/PlayMode/ChartGameTests.cs`** (line numbers of 13:04; it was being changed all morning):

1. After every "Begin" by a merchant whose home is Dorestat, add `yield return FirstHourPlay.TheTally(play);`.
   Those are line 237 (`yield return play.Click(begin[0].button, "Begin");`, in `SheSailsTheChart`: give it the
   shot name `"03b_the_tally"`), line 949 (`ThenNothingHoldsHerAtSea`, "the first of each: a Frisian of Dorestat"),
   line 1322 (`ThenTheGameSavesItselfAndComesBack`) and line 912 (the helper `AnswerAllIfBegun`, which begins with
   the creation window as it stands: a Frisian of Dorestat). Line 838 is the Dane, and gets none.
2. In `SheSailsTheChart`, the lines straight after "Begin" that say the clock runs in port
   (`Assert.IsFalse(PanelClock.Holding, "windows must not hold the clock: only a card does");` and the three lines
   after it) must come after the tally is answered: while it is up, the clock is held.
3. Line 433, `Assert.IsNull(cards.Current, "a card opened at home on a new game: nobody comes down to a boat that
   has not arrived");` keeps its place and its check; its words become "a card is up at home after the tally was
   answered: nothing but the opening comes to a boat that has not arrived". Add after it:
   `Assert.IsTrue(cards.state.WasShown(FirstHourPlay.Tally), "the tally did not open the game");`.
4. Line 860, the same line for the Dane at Ripa: no change to the check. No opening card holds at Ripa until the
   starts by people are written. Add `Assert.AreEqual(0.0, Trade.FirstDue, 1e-9, "a first payment was cut with no
   steward to cut it");`.
5. The four tests use the workspaces (the button "1 Sea" at line 480, the key 1 at lines 290, 377 and 1343, a
   window's size at line 1441, and `CollectionAssert.AreEqual(new[] { "trade", "port", "people" }, ...)` at lines
   267 and 384).
   Wherever a test sets `FrameRoot.FreshLayout = true;` (lines 178, 792, 813, 931, 938, 1297, 1306, 1478) add
   `FrameRoot.FixedFirstHour = false;` beside it, and `FrameRoot.FixedFirstHour = true;` beside each
   `FrameRoot.FreshLayout = false;` (lines 779, 901, 1238, 1533). Those tests then go on testing the free windows.
6. A new play test, `TheFirstHourIsOneScreen`, with `FixedFirstHour` left true. Outline, not written:
   new game; a Frisian; `FirstHourPlay.TheTally`; assert `frame.Windows.Fixed`, the slots "trade", "voyage",
   "people", that no button "1 Sea" and no button with the tip "Close this window  [X]" is to be found under
   `frame.Root`, and that pressing 1 and C changes nothing; a shot; click the bar's debt and assert the steward's
   card is up; "Not today"; set a course for Traiectum from the Voyage window on that same screen; cast off; assert
   the slots "voyage", "field", "sky"; a shot; at Traiectum wait for `harbour.first.tow_line.traiectum`, buy, assert
   `Fittings.Has(Fittings.TowGear)` and that the silver fell by 15 d; home; go to the steward with 120 d in the
   purse (give the purse the difference with the test's own hand); "Count out ten dozen pennies"; assert
   `Trade.Debt` is 360, `Trade.FirstDue` is 0, the bar's debt starts "Owed 360 d", `frame.Windows.Fixed` is false
   two frames on, the four workspaces are back, and the log's last line begins "The windows are yours".

## 6. The order of work, and where this meets the other write-ups

1. **The engine and the trade state** (2.3: the opening, the verbs, the first payment, the test stand-in). Then the
   data of 2.4 and `merge_writing.py`. Run the edit-mode tests: `FirstHourTests` 1 to 6 pass, and no other test
   should move.
2. **The top bar, the Tasks row, the runner's day** (2.3, the last two groups). Then the play test's edits 1 to 4.
   Play it: a new Frisian game must open on the tally, and the bar must read the first payment.
3. **The river** (3.2: the goods block, the four lines, the start cargo). `FirstHourTests` 7 and 8. Run the soak
   with the block before trusting 3.3.
4. **The fittings** (3.4). `FirstHourTests` 9.
5. **The screen** (4.2, the stylesheet, 4.4). The play test's edits 5 and 6. Look at a screenshot of the port
   screen and of the sea screen at 1920 by 1080 before going on.

Where it meets the others:

| Write-up | Meets this at | What to do |
|---|---|---|
| `fixes/debt_day.md` | `Trade.Begin`, `Trade.Capture` and `Restore`, `FrameGame.DebtLine`, `EncounterGameVars` `debt_due_days` | Both edit the same lines. Take its `Begin` with the date, and keep `CutFirst(0.0, 0.0);` as the last line of the first `Begin`; add `["first"]` and `["first_day"]` beside its `["due_name"]`; its `DebtLine` text becomes `whole` here. Its `DebtDaysLeft` and this file's `FirstDaysLeft` are the same sum for the two days. |
| `fixes/clock_five_speeds.md` | the debt's two drops; a new game's speed | 2.5 and 1.2. |
| `fixes/orders.md` | `TasksPanel.cs` (whole file replaced), `GameEffects.cs` (two edits, elsewhere in the file) | 2.3: the block is unchanged in its draft. |
| `fixes/page_and_strings.md` | the Rest and Offer buttons, "(to come)", "Night falls. [Z]", the Port window's rows | Not touched here. Its 2.7 takes out the sentences that name a window "in the first hour's fixed screen": this file is that screen. Add the new strings of this file to its rule's test: none has a number in brackets but the choices' "(120 d)", "(15 d)", which are costs, and the book's law 4 asks for them. |
| `fixes/risk_is_real.md` | `ChartBody.cs`, the call to `Travel.Speed`; reefing | 3.4. |
| `fixes/brief_faces.md` | `FrameWindows` sizes, `FrameRoot.Window` | 4.3. Its `Window` is rewritten: the head's buttons must still be left out while `Windows.Fixed`. |
| `fixes/one_cast.md` | the steward as a person of Dorestat's file | If the steward is one of the people in People here, his Talk can open the first payment's card too (`EncounterDirector.Offer`). Not read for whether he is. |
| `fixes/card_clock.md` | the default of the two new cards | 2.5. |
| The content pass on the cards | the tally's words and dates; starts by people; written ends | 2.2 and 2.4. |

## 7. Choices made here that the user may overrule

1. **120 d on the tenth day.** The reviewer said "about ten game days out" and "about half of what the first four
   casks fetch". 96 d or 144 d, or the eighth or twelfth day, are one number each in the tally's three choices.
2. **A miss adds nothing to the sum.** It costs the steward's regard and the grace road. A harder miss (he takes
   what wine is aboard; the whole falls due at Lammas) was weighed and left: it wants a ruling on what the count
   does at all, which nobody has given.
3. **The wine may be sold before the first payment is made.** The steward says so. The other reading (the casks
   are the count's until then and may not be sold) needs a mark on a lot in the hold, which `debt_day.md` section 3
   also stopped short of.
4. **The quernstones leave the start cargo.**
5. **The five river carries and their strengths**, and three guesses about what was made and wanted where (marked
   (?) in the block).
6. **The three fittings' numbers**: 3 knots at the tow, 21 m/s and half speed under reef bands, 2 knots over the
   flats with no lead. And that a boat with no lead creeps: it makes every boat that has not bought one slower on
   15 km of the Thames.
7. **The fixed screen ends at the first payment's card, or the fourteenth day**, and shows Voyage in port in place
   of the Port window.

## 8. What I could not check

- Nothing was run in Unity. The opening was never seen to open, the bar never seen to read the first payment, the
  fixed screen never seen. The compile and the 64 checks say the code is whole and the sums are right. They do not
  say it plays.
- The cards were not passed through `EncounterLoader`. In particular an empty `"effects": [ ]` (the choice "Not
  today") and the two `debt_first` names inside a `when` were checked only against the grammar as read.
- **The river's gains are sums on usual prices**, not a run of the economy: section 3.3 says what the soak must
  show. How fast the world's traders close the five new wants is the largest unknown in this file.
- Witla to Walichrum: 55 km and 6 to 14 game hours are my sums. Whether a way between them crosses lanes of kind
  `Flats` (so that the lead matters before the Thames) was not read from `lanes.bin`.
- Whether `Course.Kinds` has an entry for the point `Course.Next` on every leg: `ChartBody` asks it the way
  `ChartWorld.OnRiver` does (line 158), and `Course.LegKind` answers 255 out of range, which changes nothing.
- A scheduled card is put in line without its `when` being asked again (`Code/Core/Encounters/Scheduler.cs`, `Tick`,
  line 177). The three fitting cards are scheduled once and each sends for itself again only from "Not this
  time", so none can come once it is bought. The steward's cards are never scheduled for that reason.
- The steward's day is not kept in a save (`dunnedDay` in the runner): after a load on the same game day he may
  come down a second time.
- The card's check `silver >= 120` reads the purse in the money of the port (81 d at Dorestat at the start, where
  foreign coin loses a tenth), and the bar reads it in deniers' worth (86 d). The Tasks row's "You are short of it
  by N d" uses the bar's figure. At the margin the two differ by up to a tenth of the foreign coin held.
- `FrameWholeRows` hides a row by `visibility`, which keeps its place. Whether UI Toolkit sends the layout events
  it listens for when a panel fills its body again without changing size was not seen.
- The top bar's debt is a `Label` inside a box that has the tooltip. That the click reaches the label, and that the
  frame's top bar is not set to ignore the pointer, was not seen.
- Which edit-mode economy tests assert the old emptiness of the river, if any.
- The Dane, the Frank and the others still begin with a count's debt and no steward: that is the starts by
  people, and not this file.
- Which hour of the day a new game begins at (the bar says "early morning"): the table of 1.3 assumes about six.
