# Card clock (rulings 5, 10, 12): where the waiting time is kept, counted, saved, shown

Read 7 Oct from the code as it stands. Paths are under `Assets/Kapman/Code/` unless said. Nothing was changed.

## 0. What is already there (do not rebuild)

| What | Where |
|---|---|
| The card on screen | `EncounterDirector.Current`, Game/Encounters/EncounterDirector.cs:199; opened `OpenNext()` :333, answered `Choose(int)` :354, closed `Continue()` :397, Esc `Dismiss()` :409 |
| A card stops the clock | `FloatSpec.holdsClock = !s.Chosen` in `FrameRoot.CardSpec`, Game/UI/Frame/FrameRoot.Floats.cs:476; `FloatHoldsClock` :87; `PanelClock.Holds` set at :334 |
| The player sets it running again at once | `PanelClock.GoOn()` Game/UI/PanelClock.cs:57, called by `GameSpeed.SetLevel` (GameSpeed.cs:57) and `TogglePause` (:76); `PanelClock.LetGo` at Floats.cs:335 sets `Float.released = true` |
| The words on the card | `StandingHint(Float)` Floats.cs:275-281; the clock strip FrameRoot.cs:601-602 |
| The log | `CardChose` Floats.cs:529 -> `FrameLog.Add(title + ": " + r.Text)` |
| The save | `EncounterDirector.Capture()` :516 (an open unanswered card goes back first in line), `Restore` :528; SaveGame.cs:110 and :185 |

So ruling 5 (stops the clock, may be overridden at once) is DONE. Missing: the waiting time, the option taken by itself, the held ship, the line on the card, the save of the time, the log line. `card_rules_793.json` is read by no code yet (grep `card_rules` in Assets/Kapman/Code: no hit).

## 1. The rules file

`Bake/content/out/card_rules_793.json`: an "about" text, then one row a card. A row (line 8 on):

```json
{ "id": "...", "open": true, "urgency": "ordinary", "holds_ship": false, "patience_hours": 168,
  "default": { "index": 3, "key": "unload", "label": "Lie where he points and unload" },
  "fallbacks": [2, 0, 1], ... speaker, portrait, picture ... }
```

Counted in the file: 1392 rows; urgency pressing 43; holds_ship true 35 (so 8 pressing cards do NOT hold the ship: read both fields, never infer one from the other); patience_hours 168 x1349, and 1, 2, 3, 6, 12, 24, 48, 72 for the rest.

VERIFIED: the rows are under the top-level key `"cards"` (line 4), and the file also has `"ordinary_hours": 168` (line 3): read that for the ordinary wait instead of the constant if wanted.
NOT VERIFIED (I ran out of time): the exact names of the `Json` helpers for a number and for parsing a text. The draft below uses `Json.Num` and `Json.Parse`; a grep for them in files named Json*.cs found nothing, so look the real names up in the Core Json class before pasting. (`Json.Obj`, `Json.Arr`, `Json.Str`, `Json.Bool` are real: EncounterDirector.cs:531-534, EncounterState.cs:277-281.)

Shipping it: copy to `Assets/Kapman/Game/Data/card_rules_793.json` (beside reputation_793.json, read by `Table("card_rules_793")` in EncounterRunner.cs LoadBook). Do NOT put it in `Data/encounters/`: LoadBook reads every .json there as a card file except two names (EncounterRunner.cs, the line `if (name == "npc_roles_793.json" || name == "names_placeholder.json")`). The content step owns that copy; this note does not make it.

## 2. Where the time is KEPT: the director (pure, testable)

New file `Game/Encounters/CardRules.cs`:

```csharp
using System;
using System.Collections.Generic;
using Kapman.Encounters;

namespace Kapman.Game
{
    /// <summary>One card's rule of 7 Oct (card_rules_793.json): how long it waits and what it takes by itself.</summary>
    public sealed class CardRule
    {
        public string id = "";
        public bool pressing, holdsShip;
        public double patienceHours = CardRules.OrdinaryHours;
        public int defaultIndex = -1;
        public string defaultKey = "";
        public readonly List<int> fallbacks = new List<int>();
    }

    public sealed class CardRules
    {
        /// <summary>An ordinary card waits seven game days of running clock.</summary>
        public const double OrdinaryHours = 168.0;
        readonly Dictionary<string, CardRule> byId = new Dictionary<string, CardRule>();
        public int Count => byId.Count;

        /// <summary>A card with no row is ordinary: 168 hours, the card's own default, the ship not held.</summary>
        public CardRule For(string id) => id != null && byId.TryGetValue(id, out var r) ? r : new CardRule { id = id ?? "" };

        public static CardRules Read(string json)
        {
            var rules = new CardRules();
            if (string.IsNullOrEmpty(json))
                return rules;
            var d = Json.Parse(json) as Dictionary<string, object>;        // CHECK the helper's name
            foreach (var x in Json.Arr(d, "cards") ?? new List<object>())  // CHECK the key's name
            {
                if (!(x is Dictionary<string, object> row))
                    continue;
                var r = new CardRule
                {
                    id = Json.Str(row, "id"),
                    pressing = Json.Str(row, "urgency") == "pressing",
                    holdsShip = Json.Bool(row, "holds_ship"),
                    patienceHours = Math.Max(1.0, Json.Num(row, "patience_hours", OrdinaryHours)),
                };
                var def = Json.Obj(row, "default");
                if (def != null)
                {
                    r.defaultIndex = (int)Json.Num(def, "index", -1);
                    r.defaultKey = Json.Str(def, "key");
                }
                foreach (var f in Json.Arr(row, "fallbacks") ?? new List<object>())
                    r.fallbacks.Add(Convert.ToInt32(f));
                if (r.id.Length > 0)
                    rules.byId[r.id] = r;
            }
            return rules;
        }
    }
}
```

In `EncounterDirector.cs`, after `Current` (line 199) add:

```csharp
        /// <summary>The rules of 7 Oct (null: every card ordinary).</summary>
        public CardRules rules;
        /// <summary>Running game hours the card on screen has waited unanswered (0 when none is open).</summary>
        public double WaitedHours { get; private set; }
        /// <summary>A waiting time carried over a save: the card it belongs to, and its hours.</summary>
        string savedWaitCard = "";
        double savedWaitHours;
        double nextTry;                                                   // (when none can be taken: try again an hour on)
        /// <summary>The card took an option by itself (for the log).</summary>
        public event Action<EncounterSession, ChoiceResult> TookItself;
        /// <summary>True while the director itself is choosing (Chose fires too: the log must not write it twice).</summary>
        public bool TakingItself { get; private set; }

        public CardRule RuleNow => Current != null ? (rules != null ? rules.For(Current.card.id) : new CardRule { id = Current.card.id }) : null;
        /// <summary>Hours it will still wait (0 when none is open or it is answered).</summary>
        public double HoursLeft => Current == null || Current.Chosen ? 0.0 : Math.Max(0.0, RuleNow.patienceHours - WaitedHours);
        /// <summary>A card that holds the ship is open and unanswered.</summary>
        public bool HoldsShip => Current != null && !Current.Chosen && RuleNow.holdsShip;
```

In `OpenNext()` just before `Opened?.Invoke(Current);` (line 348):

```csharp
            WaitedHours = card.id == savedWaitCard ? savedWaitHours : 0.0;
            savedWaitCard = "";
            savedWaitHours = 0.0;
            nextTry = 0.0;
```

In `Continue()` and in the closing branch of `Dismiss()` (after `Current = null;`, lines 402 and 418): `WaitedHours = 0.0;`

## 3. Where it is COUNTED: only while the clock runs

New method in the director (after `ChooseDefault`, line 394):

```csharp
        /// <summary>
        /// Running game hours have passed with the card open (the runner, each frame: 0 while the clock is held, so the
        /// time counts only while it runs). When the patience is spent the card takes its default by itself: the rule's
        /// key, its index, then its fallbacks in order, the first the card offers today. If none can be taken it goes on
        /// waiting and tries again each hour. Returns the result when it took one, else null.
        /// </summary>
        public ChoiceResult Wait(double hours)
        {
            var s = Current;
            if (s == null || s.Chosen || !(hours > 0.0))
                return null;
            WaitedHours += hours;
            var rule = RuleNow;
            if (WaitedHours < rule.patienceHours || WaitedHours < nextTry)
                return null;
            var order = new List<int>();
            if (rule.defaultKey.Length > 0)                               // (by key first: an index goes stale when a card is edited)
                order.Add(s.options.FindIndex(o => o.def.key == rule.defaultKey));
            order.Add(rule.defaultIndex);
            order.AddRange(rule.fallbacks);
            order.Add(s.DefaultIndex);                                    // (a card with no row: its own usual answer)
            TakingItself = true;
            try
            {
                foreach (int i in order)
                {
                    if (i < 0 || i >= s.options.Count || !s.options[i].enabled)
                        continue;
                    var r = Choose(i);
                    if (r == null || !r.ok)
                        continue;
                    TookItself?.Invoke(s, r);
                    Continue();                                           // it closes; the log says what was taken
                    return r;
                }
            }
            finally { TakingItself = false; }
            nextTry = WaitedHours + 1.0;
            return null;
        }
```

Called from `EncounterRunner.Update()` in place of the `if (game != null) d.SetTime(...)` pair. Add a field `double lastHours = double.NaN;` beside `lastLook`:

```csharp
            if (game != null)
            {
                d.SetTime(game.Day, game.Year, game.Month, (long)Math.Floor(game.Hours));
                // rulings 10 and 12: the card waits in running game hours. game.Hours stands still while the clock is
                // held (time scale 0), so nothing counts then; a load or a new game moves it backwards or far: not counted.
                double h = game.Hours;
                double dh = double.IsNaN(lastHours) ? 0.0 : h - lastHours;
                lastHours = h;
                if (dh > 0.0 && dh < 24.0 * 30.0 && !GameSpeed.Paused)
                    d.Wait(dh);
            }
```

In the static `Restore(...)` add `if (Instance != null) Instance.lastHours = double.NaN;` so the jump of a load is not counted. In `EnsureDirector()` after `d.standingAt = ...`: `d.rules = CardRules.Read(Table("card_rules_793"));` and a `Debug.Log` of `d.rules.Count` (0 means the file is not in Data yet: every card is then ordinary, nothing breaks).

A rest or a tide wait jumps many hours in a frame: that is running clock, it counts, and the card may take its option during the rest. That is the ruling as written.

## 4. Where it is SAVED

`EncounterDirector.Capture()` (line 521) adds two keys to the returned dictionary; `Restore` (line 534) reads them. The card itself already goes back first in line (lines 519-520).

```csharp
            // Capture: in the returned dictionary
            ["wait_card"] = Current != null && !Current.Chosen ? Current.card.id : savedWaitCard,
            ["wait_hours"] = Current != null && !Current.Chosen ? WaitedHours : savedWaitHours,

            // Restore: after merchant = ...
            WaitedHours = 0.0;
            nextTry = 0.0;
            savedWaitCard = Json.Str(d, "wait_card", "");
            savedWaitHours = Json.Num(d, "wait_hours", 0.0);
```

"version" stays 1: an older save has neither key and the card starts its wait again (nothing is lost, nothing breaks). The frame's `Float.released` (the player let the clock run) is NOT saved: after a load the card holds the clock again until Space. That is the safe side.

## 5. How the SHIP IS HELD (holds_ship)

One static on the runner: `public static bool ShipHeld => Director != null && Director.HoldsShip;`

On the chart the ship's step is one line, Game/Chart/ChartBody.cs:74. Change it to:

```csharp
            float step = LevelClock.Paused || EncounterRunner.ShipHeld ? 0f : Time.deltaTime * rate * LevelClock.Speed / Mathf.Max(0.001f, scale);
```

She keeps her course and her order (nothing is cleared, `anchored` is not set, so no port call begins and no cast-off fires): she makes no way while the card is unanswered, and goes on by herself when it is answered or takes its option. The clock, the weather and the other ships run.

NOT VERIFIED: that ChartBody is on the player's ship only (if the other ships use it too, test `motion == ShipMotion.Player` first). In a 3D level the same test would go at Runtime/Levels/ShipMotion.cs:146 (`if (Driven && !LevelClock.Paused)`), but Runtime may not see Game: add `public static bool ShipHeld;` to LevelClock and let the runner set it each frame. Not needed for the chart release.

Game/Voyage/VoyagePlanner.cs:143 counts the hours under way by the same clock: leave it (the days pass, which is the cost of not answering).

## 6. How it is SHOWN: a line on the card

`FloatSpec` (Floats.cs:20-45) gets one field; `CardSpec` (:474) sets it; `StandingHint` (:275) says it; `FloatsUpdate` (:364) keeps it fresh (CardSpec is built only when the card changes, :462, so the hours must be read live).

```csharp
        // FloatSpec
        /// <summary>How long it will wait and what it then takes ("" none): read each frame.</summary>
        public Func<string> waits;

        // CardSpec, inside if (!s.Chosen), after the options loop
                spec.waits = () => WaitLine(d);

        /// <summary>"It will wait 6 days and 4 hours more, then: Lie where he points and unload."</summary>
        public static string WaitLine(EncounterDirector d)
        {
            var s = d.Current;
            if (s == null || s.Chosen)
                return "";
            var rule = d.RuleNow;
            int i = rule.defaultKey.Length > 0 ? s.options.FindIndex(o => o.def.key == rule.defaultKey) : -1;
            if (i < 0)
                i = rule.defaultIndex >= 0 && rule.defaultIndex < s.options.Count ? rule.defaultIndex : s.DefaultIndex;
            string then = i >= 0 ? ", then: " + s.options[i].label : "";
            return (rule.holdsShip ? "The ship is held. " : "") + "It will wait " + Span(d.HoursLeft) + " more" + then + ".";
        }

        public static string Span(double hours)
        {
            int h = (int)Math.Ceiling(Math.Max(0.0, hours));
            int days = h / 24, rest = h % 24;
            string a = days > 0 ? days + (days == 1 ? " day" : " days") : "";
            string b = rest > 0 || days == 0 ? rest + (rest == 1 ? " hour" : " hours") : "";
            return a.Length > 0 && b.Length > 0 ? a + " and " + b : a + b;
        }

        // StandingHint, replacing the holdsClock branch (lines 278-279)
            if (spec.holdsClock)
            {
                string w = spec.waits != null ? spec.waits() : "";
                return (f.released ? "The clock runs. " : "The clock is held: Space lets it run. ") +
                       (w.Length > 0 ? w : "It waits for your answer.");
            }

        // FloatsUpdate, after AlertFloats(); (the hours change while the clock runs)
            foreach (var f in floats)
                if (f.spec.waits != null && f.hint != null && f.hintUntil <= 0f)
                {
                    string t = StandingHint(f);
                    if (f.hint.text != t)
                        f.hint.text = t;
                }
```

The option named in the line is the first choice; if it is greyed on the day a fallback is taken instead, and the log says which. The clock strip (FrameRoot.cs:601-602) stays as it is. The wording is a placeholder for the user to rewrite.

## 7. The LOG says what was taken

In `CardFloat` (Floats.cs:444-450), beside `d.Chose += CardChose` add `d.TookItself += CardTookItself` (and the matching `-=` there and in `FloatsDisable`, :355-356). Then:

```csharp
        void CardChose(EncounterSession s, ChoiceResult r)
        {
            var d = EncounterRunner.Director;
            if (d != null && d.TakingItself)
                return;                                                   // (CardTookItself writes it)
            if (s != null && r != null && r.ok)
                FrameLog.Add(s.Title + ": " + r.Text);
        }

        void CardTookItself(EncounterSession s, ChoiceResult r)
        {
            string label = "";
            foreach (var o in s.options)
                if (o.def.key == r.option)
                    label = o.label;
            FrameLog.Add(s.Title + ": left unanswered, so it went this way: " + label + ". " + r.Text);
        }
```

The old IMGUI EncounterPanel is switched off under the frame (Floats.cs:369-373), so nothing else needs the event.

## 8. Tests (EditMode: Assets/Kapman/Tests/EditMode/CardClockTests.cs)

Build the director the way EncounterTests.cs / EncounterRunnerTests.cs / EncounterData.cs do (I did not read their fixture helper: reuse it, called `MakeDirector(...)` below, with a card `t.card` of options a, b, c where b is the card's default and c needs silver the fake game lacks).

```csharp
        [Test] public void Rules_read_a_row()
        {
            var r = CardRules.Read("{\"cards\":[{\"id\":\"x\",\"urgency\":\"pressing\",\"holds_ship\":true,\"patience_hours\":2," +
                                   "\"default\":{\"index\":1,\"key\":\"b\"},\"fallbacks\":[0,2]}]}").For("x");
            Assert.IsTrue(r.pressing); Assert.IsTrue(r.holdsShip); Assert.AreEqual(2.0, r.patienceHours);
            Assert.AreEqual("b", r.defaultKey); CollectionAssert.AreEqual(new[] { 0, 2 }, r.fallbacks);
        }
        [Test] public void A_card_with_no_row_is_ordinary()   // 168 h, not held, the card's own default
        [Test] public void Ordinary_waits_seven_days_then_takes_its_default()
        {
            var d = MakeDirector(); d.state.Enqueue("t.card"); d.OpenNext();
            Assert.IsNull(d.Wait(167.9)); Assert.IsNotNull(d.Current);
            var r = d.Wait(0.2);
            Assert.AreEqual("b", r.option); Assert.IsNull(d.Current); Assert.AreEqual(0.0, d.WaitedHours);
        }
        [Test] public void No_time_counts_while_the_clock_is_held()   // Wait(0) a thousand times: WaitedHours 0, card open
        [Test] public void Pressing_takes_after_its_own_time_and_holds_the_ship()
            // rules: pressing, holds_ship, 2 h: HoldsShip true on open, false after Wait(2.0) took the option
        [Test] public void Pressing_that_does_not_hold_the_ship()     // urgency pressing, holds_ship false: HoldsShip false
        [Test] public void Default_that_cannot_be_taken_falls_back_in_order()   // default c (no silver), fallbacks [0]: takes a
        [Test] public void None_can_be_taken_goes_on_waiting()        // all greyed: Wait null, Current stays, tries again an hour on
        [Test] public void Answering_before_the_time_takes_nothing_by_itself()  // Choose(0); Wait(500) == null
        [Test] public void The_wait_is_saved_and_comes_back()
        {
            var d = MakeDirector(); d.state.Enqueue("t.card"); d.OpenNext(); d.Wait(100.0);
            var saved = d.Capture();
            var e = MakeDirector(); e.Restore(saved); e.OpenNext();
            Assert.AreEqual("t.card", e.Current.card.id); Assert.AreEqual(100.0, e.WaitedHours, 1e-9);
            Assert.AreEqual(68.0, e.HoursLeft, 1e-9);
        }
        [Test] public void An_old_save_without_the_keys_starts_the_wait_again()
        [Test] public void TookItself_fires_once_and_TakingItself_is_true_inside_Chose()
        [Test] public void Span_words()   // 168 -> "7 days", 148.2 -> "6 days and 5 hours", 0.4 -> "1 hour", 24 -> "1 day"
        [Test] public void WaitLine_names_the_option_and_the_held_ship()   // FrameFloatTests.cs style, on CardSpec(d, s).waits()
        [Test] public void Every_row_of_the_real_file_points_at_an_option_of_its_card()
            // reads Bake/content/out/card_rules_793.json and the book: default.index < options, key matches; open cards only
```

## 9. Order of work (each compiles and tests alone)

1. CardRules.cs and its two tests.
2. Director: fields, OpenNext, Wait, Continue/Dismiss, Capture/Restore, and their tests.
3. Runner: load the rules, count the hours, ShipHeld; ChartBody.cs:74.
4. Frame: waits, StandingHint, FloatsUpdate, the log.
5. Copy the rules file into Data (the content step) and the whole-file test.

## 10. Open points for the user (not decided here)

- A card NOT yet opened (waiting in line behind another, or kept back by the title or the chest): its time does not count in this draft; only the card on screen waits.
- Esc on a card that is not an arrival card still closes it unanswered with no option taken (`Dismiss`, :409-420). Under ruling 10 that is a second way out: keep it, or make Esc only show the hint.
- After a load the card holds the clock again though the player had let it run.
- Resting through the night counts as running time: an ordinary card can be answered for you while you rest.
- Harbour cards still waiting go when she casts off (`EndCall`, :295-299): that is the queue, not the open card. An open harbour card stays open at sea today; unchanged here.
