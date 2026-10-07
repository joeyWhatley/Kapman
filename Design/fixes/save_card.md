# Save fix: the open card, and the course's kinds (7 Oct 2026)

Read, not run. Nothing under Assets was changed. First draft; the card part follows below as it is found.

## B. Course.Kinds rebuilt on load  (Assets/Kapman/Code/Game/Voyage/Course.cs)

Fault: Capture (line 201-212) writes dest, next, km, points, water. It does not write Kinds. Restore (214-240)
clears Kinds at line 219 and never fills it, so LegIsRiver (177) is false for every leg until SetTo runs again.

Fix: write the kinds; read them back; for an older save without them, plan the same course again and take its kinds
if the line is the same length.

### B1. Capture: replace lines 208-211

```csharp
            var water = new List<object>();
            foreach (var d in Depths)
                water.Add(float.IsInfinity(d) ? 9999.0 : (double)d);        // (JSON has no infinity: "plenty")
            var kinds = new List<object>();
            foreach (var k in Kinds)
                kinds.Add((double)k);
            return new Dictionary<string, object>
            {
                ["dest"] = Destination, ["next"] = Next, ["km"] = Km, ["points"] = pts, ["water"] = water, ["kinds"] = kinds,
            };
```

### B2. Restore: after line 231 (the Depths count check), before "if (Points.Count >= 2)" at 232, add

```csharp
                foreach (var x in Json.Arr(d, "kinds") ?? new List<object>())
                    if (x is double k)
                        Kinds.Add((byte)k);
                if (Kinds.Count != Points.Count)
                    Kinds.Clear();                                       // (an older save: rebuilt below)
```

and inside "if (Points.Count >= 2) { ... }" after Km is set (line 236), add

```csharp
                    if (Kinds.Count == 0)
                        RebuildKinds();
```

### B3. New method, after Restore (before Clear at line 242)

```csharp
        /// <summary>An older save has no kinds: the same course is planned again from its first point, and its kinds
        /// (and its water, if that was missing too) are taken when the line comes out with the same number of points.
        /// If it does not (the lanes were baked again since), the kinds stay empty, as before: she sails every leg.</summary>
        static void RebuildKinds()
        {
            if (Destination == null || Points.Count < 2)
                return;
            try
            {
                var line = new List<WorldXZ>();
                var water = new List<float>();
                var kinds = new List<byte>();
                if (!Plan(Destination, Points[0], null, line, water, kinds, out _) || kinds.Count != Points.Count)
                    return;
                Kinds.AddRange(kinds);
                if (Depths.Count == 0)
                    Depths.AddRange(water);
            }
            catch (Exception)
            {
                Kinds.Clear();                                           // (no game data loaded: leave it as it was)
            }
        }
```

### B4. Test (EditMode, NUnit)

```csharp
        [Test]
        public void Course_KindsComeBackOnLoad()
        {
            var saved = new Dictionary<string, object>
            {
                ["dest"] = "dorestad", ["next"] = 2.0, ["km"] = 3.0,
                ["points"] = new List<object>
                {
                    new List<object> { 0.0, 0.0 }, new List<object> { 1000.0, 0.0 },
                    new List<object> { 2000.0, 0.0 }, new List<object> { 3000.0, 0.0 },
                },
                ["water"] = new List<object> { 9999.0, 9999.0, 4.0, 9999.0 },
                ["kinds"] = new List<object>
                {
                    (double)LaneGraph.Sea, (double)LaneGraph.River, (double)LaneGraph.Sea, (double)LaneGraph.Sea,
                },
            };
            try
            {
                Course.Restore(saved);
                Assert.AreEqual(4, Course.Kinds.Count);
                Assert.IsTrue(Course.LegIsRiver(1));
                Assert.IsFalse(Course.LegIsRiver(2));
                // round trip: what Capture writes, Restore reads to the same
                var again = Course.Capture();
                Course.Restore(null);
                Assert.AreEqual(0, Course.Kinds.Count);
                Course.Restore(again);
                Assert.IsTrue(Course.LegIsRiver(1));
                Assert.AreEqual(2, Course.Next);
                Assert.AreEqual("dorestad", Course.Destination);
            }
            finally
            {
                Course.Restore(null);
            }
        }
```

Note: a real save goes through Json.Write then Json.Read; the test should also pass the dictionary through them
if Json.Read gives numbers as double (the code above assumes it, as the "water" lines at 227-229 do).

## A. The open card comes back on its page, with its cast

Files: Assets/Kapman/Code/Core/Encounters/EncounterSession.cs, Assets/Kapman/Code/Game/Encounters/EncounterDirector.cs.

Fault (as read):
- EncounterDirector.Capture (516-525) only puts the open card's id first in the queue (519-520).
- Restore (528-535) sets Current = null (530). OpenNext (333-350) then opens it new.
- EncounterSession.Open (137-142) draws the cast again: a fixedRole actor through npcs.Holder (the same holder, so
  the same person), every other actor through npcs.Forge (142): a NEW person each time. So the face changes, and
  one more forged person is left in the registry at every load.
- The dice: Dice(card) (582-586) is seeded by the Hour at the time of opening. A card stops the clock, so the hour
  is usually the same; the fix keeps the hour the card first opened at, to be sure.
- The page: EncounterSession has NO page field (its public fields are card, world, actors, lines, options,
  returning, result). A grep for "page" over Game/UI/Frame and Game/Encounters found it only in FrameRoot.Self.cs.
  So where the frame keeps "which page of the card" was NOT FOUND in the time. The fix gives the session a plain
  `page` number that is saved and restored; the card panel must write it when it turns a page and read it on Opened.

The old line stays (the id still goes first in the queue), so a save read by older code still reopens the card.

### A1. EncounterSession.cs: a page, and a cast handed in

After line 97 (`public ChoiceResult result;`) add

```csharp
        /// <summary>The page of the card the reader is on (from 0). The panel sets it as it turns pages; it is saved
        /// with an open card and set again when the card comes back after a load.</summary>
        public int page;
```

Replace lines 137-142 (the head of Open and the cast loop)

```csharp
        public static EncounterSession Open(EncounterDef card, EncounterWorld world) => Open(card, world, null);

        /// <summary>As Open, with a cast already drawn (a saved game's open card): actor key -> the person's id. An
        /// actor whose person is no longer known is drawn as usual.</summary>
        public static EncounterSession Open(EncounterDef card, EncounterWorld world, Dictionary<string, string> cast)
        {
            var s = new EncounterSession(card, world);
            foreach (var spec in card.actors)
            {
                Npc kept = cast != null && cast.TryGetValue(spec.key, out var id) ? world.npcs.Get(id) : null;
                s.actors[spec.key] = kept ?? (spec.fixedRole ? world.npcs.Holder(spec, world.port, world.year, world.day)
                                                             : world.npcs.Forge(spec, world.port, world.year));
            }
```

(the rest of Open, from `var vars = world.vars;` at 143, is unchanged.)

### A2. EncounterDirector.cs

After line 199 (`public EncounterSession Current ...`) add

```csharp
        /// <summary>A saved game's open card, waiting to come back as it was: its id, page, cast and the hour it opened.</summary>
        string keptCard;
        int keptPage;
        long keptHour;
        Dictionary<string, string> keptCast;
        /// <summary>The game hour the card on screen opened at (its dice are seeded by it).</summary>
        long openedHour;
```

Replace OpenNext's body from line 340 to 349 (after the `if (card == null) return null;`)

```csharp
            string port = state.callPort ?? "";
            Prepare(port, Day, port.Length > 0);
            bool back = keptCard != null && keptCard == card.id;
            openedHour = back ? keptHour : Hour;
            var world = new EncounterWorld
            {
                book = book, state = state, npcs = npcs, game = game, vars = vars, merchant = merchant,
                port = Facts(port), year = Year, day = Day, roll = roll ?? Dice(card.id, openedHour),
            };
            Current = EncounterSession.Open(card, world, back ? keptCast : null);
            if (back)
                Current.page = keptPage;
            keptCard = null;
            keptCast = null;
            Opened?.Invoke(Current);
            return Current;
```

Replace Capture (516-525)

```csharp
        public Dictionary<string, object> Capture()
        {
            var st = state.Capture();
            var d = new Dictionary<string, object>
            {
                ["version"] = 1, ["merchant"] = merchant, ["armed"] = watch.Armed, ["state"] = st, ["npcs"] = npcs.Capture(),
            };
            // the card on screen, unanswered; or the one a load brought back that has not opened again yet
            string id = Current != null ? (Current.Chosen ? null : Current.card.id) : keptCard;
            if (id == null)
                return d;
            if (Current != null && st.TryGetValue("queue", out var q) && q is List<object> line)
                line.Insert(0, id);                                  // (a kept card is still first in the saved line)
            var cast = new Dictionary<string, object>();
            if (Current != null)
            {
                foreach (var a in Current.actors)
                    if (a.Value != null)
                        cast[a.Key] = a.Value.id;
            }
            else if (keptCast != null)
                foreach (var a in keptCast)
                    cast[a.Key] = a.Value;
            d["open"] = new Dictionary<string, object>
            {
                ["card"] = id, ["page"] = (double)(Current != null ? Current.page : keptPage),
                ["hour"] = (double)(Current != null ? openedHour : keptHour), ["cast"] = cast,
            };
            return d;
        }
```

Replace Restore (528-535)

```csharp
        public void Restore(Dictionary<string, object> d)
        {
            Current = null;
            state.Restore(Json.Obj(d, "state"));
            npcs.Restore(Json.Obj(d, "npcs"));
            watch.Armed = Json.Bool(d, "armed");
            merchant = Json.Str(d, "merchant", merchant);
            keptCard = null;
            keptCast = null;
            keptPage = 0;
            var open = Json.Obj(d, "open");
            string id = open != null ? Json.Str(open, "card", null) : null;
            // only if it is still the first in line (it always is in a save this code wrote)
            if (!string.IsNullOrEmpty(id) && state.queue.Count > 0 && state.queue[0] == id)
            {
                keptCard = id;
                keptPage = Math.Max(0, (int)Json.Num(open, "page", 0));
                keptHour = (long)Json.Num(open, "hour", Hour);
                keptCast = new Dictionary<string, string>();
                var cast = Json.Obj(open, "cast");
                if (cast != null)
                    foreach (var a in cast)
                        if (a.Value is string who)
                            keptCast[a.Key] = who;
            }
        }
```

Replace Dice (582-586)

```csharp
        Func<double> Dice(string card) => Dice(card, Hour);

        Func<double> Dice(string card, long hour)
        {
            var rnd = Sim.SimRandom.For(seed, "choice/" + card + "/" + hour);
            return () => rnd.Next();
        }
```

Also: EndCall (295-299) removes harbour cards from the queue. If the kept card is removed there before it reopens,
OpenNext takes another card, `back` is false and the kept fields are dropped: right, nothing more to do.

### A3. The panel (not written: the file was not found in the time)

Wherever the frame's card panel turns a page: `director.Current.page = n;`. Where it handles Opened:
start from `session.page` and not from 0. Until that is done the cast comes back but the page does not.

### A4. Test (Assets/Kapman/Tests/EditMode/EncounterRunnerTests.cs; it builds a director at line 30)

`NewDirector()` and `"card_with_stranger"` stand for that file's own helper and a test card that has one actor
that is NOT fixedRole (a forged stranger) and at least one option. Use the names that file has. Both directors
must share nothing (each its own NpcRegistry), or the test proves nothing.

```csharp
        [Test]
        public void OpenCard_ComesBackOnItsPage_WithItsCast()
        {
            var d = NewDirector();
            d.SetTime(10.0, 793, 6, 240);
            d.state.Enqueue("card_with_stranger", true);
            var s = d.OpenNext();
            Assert.NotNull(s);
            s.page = 2;
            var faces = s.actors.ToDictionary(a => a.Key, a => a.Value.id);
            var said = s.lines.Select(l => l.text).ToList();

            var saved = d.Capture();        // better: through Json.Write then Json.Read, the file's own form

            var e = NewDirector();
            e.SetTime(10.5, 793, 6, 252);                                // a later hour: the dice must not move
            e.Restore(saved);
            Assert.IsNull(e.Current);
            var t = e.OpenNext();
            Assert.NotNull(t);
            Assert.AreEqual("card_with_stranger", t.card.id);
            Assert.AreEqual(2, t.page);
            foreach (var f in faces)
                Assert.AreEqual(f.Value, t.actors[f.Key].id, "the same person as " + f.Key);
            CollectionAssert.AreEqual(said, t.lines.Select(l => l.text).ToList());
            Assert.AreEqual(0, e.state.queue.Count(id => id == "card_with_stranger"), "not in line twice");

            // saved again before it reopens: still kept, still first in line
            var g = NewDirector();
            g.Restore(saved);
            var again = g.Capture();
            Assert.IsTrue(again.ContainsKey("open"));
            var st = (Dictionary<string, object>)again["state"];
            Assert.AreEqual("card_with_stranger", ((List<object>)st["queue"])[0]);
        }

        [Test]
        public void OldSaveWithoutOpen_ReopensAsBefore()
        {
            var d = NewDirector();
            d.state.Enqueue("card_with_stranger", true);
            d.OpenNext();
            var saved = d.Capture();
            saved.Remove("open");
            var e = NewDirector();
            e.Restore(saved);
            Assert.NotNull(e.OpenNext());
        }
```

## Not checked (out of time)

- That a forged (not fixedRole) person is in NpcRegistry.Capture "npcs", so npcs.Get(id) finds them after a load.
  If forged people are not kept, A1 falls back to a new draw: then the cast map must hold the whole person, not the id.
- That Npc has a public `id` (RunEffects line 508 uses `who.id`, so likely) and state.queue is a List of string.
- Json.Read / Json.Write names, and that numbers read back as double (Course.Restore lines 226-229 assume so).
- The frame's card panel and its page state (A3).
- EncounterSession.Open marks the card shown again on reopening (cool-down restarts): harmless, same as today.
- Whether the test files need `using System.Linq;` and `using Kapman.World;` (LaneGraph).
- The audit's other gap (a card answered, outcome page up) is left as it is: its effects are already applied.
- Nothing here was compiled or run.
