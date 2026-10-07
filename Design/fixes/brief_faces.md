# Brief faces (ruling 9, 7 Oct): every panel a BRIEF face and a FULL face

Read for this: FramePanel.cs (whole), FrameWindows.cs (whole), FrameRoot.cs lines 70-114, 318-330, 428-548,
FrameRoot.Floats.cs lines 12-58, 108-150 and 200-340 (part), and the Fill/Stamp of every panel (outline by grep; Crew,
Hold, Voyage, Sky, House, Ship read line by line). Nothing under Assets was changed. Nothing was compiled or run.

Marks used below: [read] = I read the member in the panel's own code; [grep] = I saw the line in a grep outline only,
check the exact member name in the file at the line given before pasting.

## 1. What is there today

- `FramePanel` (Assets/Kapman/Code/Game/UI/Frame/FramePanel.cs, class at line 28): ONE body per panel
  (`Body`, `Attach(body)`, `Detach()`, `Tick()`, `Redraw()`, `Fill(body)`, `Stamp()`, `Live()`, `OnShown()`, `OnHidden()`).
  One object of each panel lives for the whole play (`FramePanels`, same file), so one panel cannot today be in the left
  column and in a float at once.
- Tiling state: `FrameWindows` / `FrameWorkspace` (FrameWindows.cs). A workspace keeps `name`, `layout`, `slots` (panel
  ids), `focus`. No size. Kept between runs by `Capture()` / `Restore()` as `current;name|layout|focus|a,b,c;...`
  (PlayerPrefs, FrameRoot.cs lines 79 and 112).
- Tiling view: `FrameRoot.DrawWindows(key)` (FrameRoot.cs line 433) and `FrameRoot.Window(w, index, last)` (line 493).
  `Window` builds the head (before / title-picker / next / close), a ScrollView, a `frame-body` box, then
  `panel.Attach(body); shown.Add(panel);` (lines 537-541). `shown[i].Tick()` runs every 0.25 s (line 327).
  The key that makes it draw again is built at lines 428-430.
- Floating windows: `FrameRoot.OpenFloat(FloatSpec spec)` (FrameRoot.Floats.cs line 118), `CloseFloat(string id)`
  (line 137), `FloatOpen(id)` (line 113). `FloatSpec` (line 20) has `id, title, who, speaker, speakerLine, face, lines,
  small, result, options, mustAnswer, holdsClock, shut, content, look`. `content` is "its own content in place of the
  lines"; `look` is a stylesheet class for that window alone. `Shut(f)` (line 304) calls `spec.shut` if set, ELSE
  `CloseFloat(id)`: so a `shut` action must close the float itself. Opening the same id again rebuilds the window in
  place (lines 122-131). `FloatsBuild()` clears every float when the frame is built again.

## 2. The panels (19) and their brief lines

Order = `Order` in the class. Line 1 is the one that matters now (it is the only line at size Small). Up to four lines.

### voyage - VoyagePanel.cs (Order 10, class line 16) [read]
1. Course set: `"Bound for " + GameData.Port(Course.Destination).name + ", " + FramePassage.DaysWords(Course.KmLeft(ChartWorld.ShipWorld()))`
   Not set, in port (`FrameGame.PortHere != null`): `"At " + FrameGame.PortHere.name + ". No course set."`
   Not set, at sea: `"No course set."`   No ship (`KapmanGame.Current?.ship == null`): `"No ship yet."`
2. `$"{left:0} km to go by water"` - `Course.KmLeft(ChartWorld.ShipWorld())` (Fill line ~158).
3. Ports known: `KnownPorts().Count` (the panel's own method, used in its Stamp line ~135): `"You know of N other ports."`
4. In port with a course: `"Cast off when you will."`
Stamp: the panel's own Stamp already covers all of it (Course.Destination, KmLeft/5, PortHere).

### field - FieldPanel.cs (Order 15, class line 12) [grep]
1. `ahead == 0 ? "None of them is ahead of you." : ...` - the closing line the Fill builds at line 124 (the count `ahead`).
   No course: `"No course set: no field."` (the condition of Fill line 105).
2. `$"You have {mine:0} km to run."` (Fill line 110).
3. How many are bound there: the count of rows `r` (Fill lines 114-122); none: `"No other ship is bound there now."`
4. The nearest rival: first row `r.name + ", " + r.stands` (line 122).
Note: `mine`, `ahead` and the rows are locals of Fill; move the reckoning into one private method both faces call.

### sky - SkyPanel.cs (Order 17, class line 12) [read]
1. `Cap(FrameGame.WindWords(air.windSpeed, air.windFrom))` from `ChartWorld.TryWeather(out var air)` (Fill line ~51).
   No weather: `"There is no weather to read yet."`
2. `"For your course: " + Lies(Mathf.DeltaAngle(bearing, air.windFrom)) + "."` (Fill lines ~53-59; only when `Course.IsSet`).
3. Sky word: `air.storm > 0.5f ? "a storm" : air.fog > 0.6f ? "fog" : ...` (line 64), with `FrameUi.PartOfDay(LevelClock.Current.HourOfDay)` (line 66).
4. `VoyageTide.Rising ? "Tide making" : "Tide ebbing"` (line 68); or `ChartBody.Current.Knots`, "She makes x knots" (line ~62).
When `air.storm > 0.5f` line 1 becomes `"A storm. " + the wind words` (the thing that matters now).

### trade - TradePanel.cs (Order 20, class line 15) [grep]
1. In a market: `$"Silver {FrameGame.Silver:0} d. Room {Trade.Hold.Free:0} kg."` (Fill line 121).
   Shut or ruined: `market.Ruined ? "Nobody trades here any more." : "The market is shut."` (line 99).
   No market: `"She lies at no market."` (line 67).
2. `$"Prices in {market.zone.unit}."` (line 93).
3. Best sale aboard here: needs the rows the Fill builds at lines 100-119 (not read): name the dearest good in the hold that this market buys.
4. `"Nothing is bought or sold here now."` when it is so (line 120).

### port - PortPanel.cs (Order 25, class line 12) [grep]
1. `FrameGame.PortHere.name`, then `Reputation.Line(here)` (Fill line 38). No port: `"She lies at no port."` (line 34).
2. `text.IntentionText` (line 47) when a talk is up; `"Finish what is in front of you first."` when that is so (line 41).
3. How many things can be done: the count of the menu's options the Fill lists (lines 48-58, not read); none: `"Nothing more to be done here but trade."`

### people - PeoplePanel.cs (Order 27, class line 11) [grep]
1. `"N people to talk to at " + port.name` - the list the Fill walks (lines 43-70, not read). No port: `"She lies at no port."` (line 33).
2. and 3. The first two names of that list with what they are.
4. `"Nobody here is written of for this year."` when so (line 50).

### hold - HoldPanel.cs (Order 30, class line 9) [read]
1. `$"{hold.Used:0} of {hold.capacity:0} kg of room taken"` - `Trade.Hold` (Fill line ~33). Not started: `"There is no hold to look into yet."` Empty: `$"She is empty. Room {hold.Free:0} kg."`
2. and 3. The two heaviest lines of `hold.Contents`: `FrameGame.Units(good, count)` + `$"{count * good.kg:0} kg"`.
4. `$"Food {FrameUi.Days(Voyage.FoodDays)}, drink {FrameUi.Days(Voyage.DrinkDays)}."` (when `Voyage.Started`; both are in the Stamp).
When food or drink is under 2 days, line 4 becomes line 1.

### crew - CrewPanel.cs (Order 35, class line 9) [read]
1. `(Voyage.Men == 1 ? "One man" : Voyage.Men + " men") + ", " + FrameGame.HealthWord(Voyage.Health) + ", " + FrameGame.HeartWord(Voyage.Morale)`.
   Not started: `"There is no crew to speak of yet."`
2. `$"Pay owed {Voyage.PayOwed:0} d."`
3. `"Food " + FrameUi.Days(Voyage.FoodDays) + ", ale and water " + FrameUi.Days(Voyage.DrinkDays)` (`< 0.25` reads "none").
4. `"At sea " + Voyage.DaysAtSea + " days"`.
When `Voyage.FoodDays < 0.25 || Voyage.DrinkDays < 0.25` line 3 goes first.

### tasks - TasksPanel.cs (Order 40, class line 10) [grep]
1. The first thing taken on (Fill lines 40-65, not read), else `"Nothing taken on yet."` (line 39).
2. Debt: `"You owe nothing."` (line 68) or the debt line above it (`Trade.Debt`, as HousePanel reads it).
3. Orders: count, or `"None."` (line 74).
4. Posted here: count, or `"None posted here now."` (line 97).

### word - WordPanel.cs (Order 50, class line 12) [grep]
1. `$"The freshest word is {...}."` - local `freshest` (Fill line 44). None: `"No word of any market yet."` (line 33).
2. and 3. The first two `notes`: `port.name` + `Age(age)` (line 61).
4. `$"And {notes.Count - shown} more, older."` (line 68).

### chronicle - ChroniclePanel.cs (Order 60, class line 15) [grep]
1. The newest thing heard (first row of the list the Fill builds, lines 36-79, not read). None: `"No word of anything has reached you yet."` (line 80).
2. and 3. The two before it.
4. `"News is as old as the ship that brought it."` (line 81).

### event - EventPanel.cs (Order 70, class line 10) [grep]
1. A card up: `spec.who` or the card's title (Fill line 50; `spec` is the `FloatSpec` from `FrameRoot.CardSpec`, Floats line 474).
   None: `$"She lies at {port.name}."` (the line at 42-44); `"Something waits its turn."` (line 38).
2. First written line of `spec.lines` (line 52).
3. `"N choices."` - `spec.options.Count` (lines 58-62).
4. `"It waits for your answer. The clock is held."` (line 66).
PRESS: this panel has no full face of its own: its full face IS the card float. Override `OpenFull` to bring the card
float to the front and to do nothing when no card is up.

### self - SelfPanel.cs (Order 80, class line 9) [grep]
1. Who you are: people, faith, past (the heading the Fill builds at lines 24-36, not read). New game: `"A new game asks who you are."` (line 23).
2. and 3. The two highest stats: name + `level` (line 44, `Self.Stat(id)`).
4. `was.gift` (line 54).

### bonds - BondsPanel.cs (Order 82, class line 13) [grep]
1. `"You know N people."` - the count of rows at line 52; none: `"You know nobody yet."` (line 76).
2. and 3. The two last spoken to: `Cap(name)` + `last` (line 52).

### faith - FaithPanel.cs (Order 84, class line 12) [grep]
1. The next day of your faith: `name + ": " + when + "."` (line 57); none: `"No day of your faith falls in the next two months."` (line 55).
2. Troth: `troth.Level(Self.Stat("troth"))` (line 33).
3. `"You swear: " + faith.swears` (line 36).

### tongues - TonguesPanel.cs (Order 86, class line 11) [grep]
1. `"You speak N tongues."` - rows at line 42 (`Self.TongueName(id)`); none: `"You have no tongue written down yet."` (line 44).
2. Your best tongue and `How(level, held)` (line 42).
3. `heard + " more heard, not yet learned."` (line 60).

### ship - ShipPanel.cs (Order 90, class line 10) [read]
1. `"The " + ship.shipClass.shipName` (`KapmanGame.Current.ship.shipClass`). No ship: `"There is no ship to look over yet."`
2. `$"Draws {c.draught:0.##} m. Carries {Trade.Hold.capacity:0} kg."`
3. `$"{cls.sailKn:0.#} knots under sail"` / `$"{cls.oarsKn:0.#} under oars"` (`game.World.ClassOf(ChartBody.Current.classId)`).
4. `$"Hands she wants: {cls.crew:0}"`.

### house - HousePanel.cs (Order 92, class line 10) [read]
1. `Trade.Debt > 0.01 ? $"Owed {Trade.Debt:0} d. In hand {FrameGame.Silver:0} d." : $"Nothing owed. In hand {FrameGame.Silver:0} d."` (the Fill's own heading). Not started: `"There is no house to reckon yet."`
2. `$"Due {FrameGame.DateOfDay(Trade.DebtDue)}, {days:0} days from now."` with `days = Trade.DebtDue - KapmanGame.Current.Day` (past due: "fell due ... ago", and then THIS is line 1).
3. `short_ > 0.5 ? $"Short by {short_:0} d today." : "The silver in hand would cover it today."` (`short_ = Trade.Debt - FrameGame.Silver`).
4. Name: `name.Level(Self.Stat("name"))` (line 51).

### chest - ChestFramePanel.cs (Order 94, class line 10) [grep]
1. `Chest.Items.Count` things: `"N things in the chest."`; none: `"The chest is empty."` (line 42).
2. and 3. The first two: `title` + `tag` (line 33); `past.chestThing` (line 24).
PRESS: the full face is the chest itself (today "press I", line 43). Override `OpenFull` to open the chest the way the
I key does (find that call where the frame reads its keys; not read) in place of a float.

## 3. The change to the base class (FramePanel.cs, lines 28-96)

The full face stays `Fill` (ruling 9: "what is built so far is the full face"), so NO panel has to change to keep
working. A panel gets a brief face by overriding one method that returns words; the base draws them. Until a panel
overrides it, its brief face says "Press to open."

Replace the whole `FramePanel` class with (the enum goes above it, in the same namespace):

```csharp
    /// <summary>How much of a panel's brief face the left column shows: one line, three lines, or all (four at most).</summary>
    public enum FrameSize { Small = 1, Medium = 3, Large = 4 }

    public abstract class FramePanel
    {
        public abstract string Id { get; }
        public abstract string Title { get; }
        public virtual int Order => 100;

        /// <summary>The body of the FULL face (in its floating window); null while it is not open.</summary>
        public VisualElement Body { get; private set; }
        /// <summary>The body of the BRIEF face (in the left column); null while no window shows it.</summary>
        public VisualElement BriefBody { get; private set; }
        public bool Shown => Body != null;
        public bool BriefShown => BriefBody != null;
        public FrameRoot Frame { get; internal set; }

        /// <summary>The id of the floating window that holds the full face.</summary>
        public string FloatId => "panel:" + Id;

        int stamp, briefStamp;
        bool filled, briefFilled;
        FrameSize size = FrameSize.Medium;
        readonly List<string> briefLines = new List<string>();

        // ---- the brief face (the left column)

        internal void AttachBrief(VisualElement body, FrameSize size)
        {
            BriefBody = body;
            this.size = size;
            briefFilled = false;
            RedrawBrief();
        }

        internal void DetachBrief() => BriefBody = null;

        void RedrawBrief()
        {
            if (BriefBody == null)
                return;
            briefFilled = true;
            BriefBody.Clear();
            briefLines.Clear();
            try
            {
                Brief(briefLines);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                briefLines.Clear();
                briefLines.Add("This could not be read: " + e.Message);
            }
            if (briefLines.Count == 0)
                briefLines.Add("Press to open.");
            int n = Mathf.Min(briefLines.Count, Mathf.Min(4, (int)size));
            for (int i = 0; i < n; i++)
            {
                var l = new Label(briefLines[i] ?? "") { pickingMode = PickingMode.Ignore };
                l.AddToClassList(i == 0 ? "frame-brief-first" : "frame-brief-line");
                BriefBody.Add(l);
            }
            briefStamp = BriefStamp();
        }

        /// <summary>The brief face: one to four lines, the FIRST the one that matters now. Plain words, no controls.</summary>
        protected virtual void Brief(List<string> lines) { }
        /// <summary>Changes when the brief lines have changed. As it stands: the full face's stamp.</summary>
        protected virtual int BriefStamp() => Stamp();
        /// <summary>What a press on the brief face does. As it stands: the full face in a floating window.</summary>
        public virtual void OpenFull() => Frame?.OpenPanel(this);

        // ---- the full face (a floating window)

        internal void Attach(VisualElement body)
        {
            if (Body != null && Body != body)
                OnHidden();
            Body = body;
            filled = false;
            OnShown();
            Redraw();
        }

        internal void Detach()
        {
            if (Body == null)
                return;
            OnHidden();
            Body = null;
        }

        internal void Tick()
        {
            // (a float shut by something else, or the frame built again: the body has left the screen)
            if (Body != null && Body.panel == null)
                Detach();
            if (BriefBody != null && (!briefFilled || BriefStamp() != briefStamp))
                RedrawBrief();
            if (Body == null)
                return;
            if (!filled || Stamp() != stamp)
                Redraw();
            else
                Live();
        }

        /// <summary>Builds both faces again now (after a button of the panel changed something).</summary>
        public void Redraw()
        {
            RedrawBrief();
            if (Body == null)
                return;
            filled = true;
            Body.Clear();
            try
            {
                Fill(Body);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Body.Add(FrameUi.P("This window could not be drawn: " + e.Message));
            }
            stamp = Stamp();
        }

        protected abstract void Fill(VisualElement body);
        protected virtual int Stamp() => 0;
        protected virtual void Live() { }
        protected virtual void OnShown() { }
        protected virtual void OnHidden() { }
    }
```

Things to watch:
- `Stamp()` of VoyagePanel calls `Fresh()` and `Reckon()` (lines 132-133). It now runs for the brief face while the
  float is shut, and for every panel in the column: the same work as today for a shown panel, no more.
- `Body.panel == null`: `OpenPanel` (section 5e) attaches AFTER `OpenFloat` has put the content in the hierarchy, so it
  is not null by the next Tick. If `Build(f)` (Floats line 167) does not add `spec.content` to the window (I did not
  read lines 167-200), this check would shut the full face at once: read those lines first.
- A panel that keeps a pick (VoyagePanel `picked`, the Tasks part/all buttons) keeps it: still one object per panel.
- `Live()` runs for the full face only. A brief line with a running figure must be in `BriefStamp`.

Example override, CrewPanel (members as read in its Fill, lines 26-40):

```csharp
        protected override void Brief(List<string> lines)
        {
            if (!Voyage.Started)
            {
                lines.Add("There is no crew to speak of yet.");
                return;
            }
            string men = (Voyage.Men == 1 ? "One man" : Voyage.Men + " men") + ", " + FrameGame.HealthWord(Voyage.Health) + ", " + FrameGame.HeartWord(Voyage.Morale);
            string food = "Food " + (Voyage.FoodDays < 0.25 ? "none" : FrameUi.Days(Voyage.FoodDays)) + ", ale and water " + (Voyage.DrinkDays < 0.25 ? "none" : FrameUi.Days(Voyage.DrinkDays)) + ".";
            bool hungry = Voyage.FoodDays < 0.25 || Voyage.DrinkDays < 0.25;
            if (hungry)
                lines.Add(food);
            lines.Add(men);
            lines.Add($"Pay owed {Voyage.PayOwed:0} d.");
            if (!hungry)
                lines.Add(food);
            lines.Add(Voyage.DaysAtSea <= 0 ? "Not yet at sea." : Voyage.DaysAtSea == 1 ? "At sea 1 day." : "At sea " + Voyage.DaysAtSea + " days.");
        }

        // (the full face's stamp does not hold the food and the drink)
        protected override int BriefStamp() => FrameUi.Mix(FrameUi.Mix(Stamp(), (int)(Voyage.FoodDays * 4.0)), (int)(Voyage.DrinkDays * 4.0));
```
(CrewPanel.cs needs `using System.Collections.Generic;` for `List<string>`. The same for every panel that overrides Brief.)

## 4. Sizes kept with the layout (FrameWindows.cs)

The size belongs to a panel in a workspace (so stepping, swapping and closing cannot put sizes out of step with the
slots). The kept line stays the old one while every size is Medium: a size is written only when it is not Medium.

`FrameWorkspace` (line 11), add:

```csharp
        /// <summary>The size of a panel's brief face here (a panel not named: Medium).</summary>
        public readonly Dictionary<string, FrameSize> sizes = new Dictionary<string, FrameSize>();
        public FrameSize SizeOf(string id) => id != null && sizes.TryGetValue(id, out var s) ? s : FrameSize.Medium;
```

`FrameWindows`, after `Set` (ends line ~211), add:

```csharp
        /// <summary>The brief face of window `index` one size on: one line, three lines, all, and round.</summary>
        public bool Resize(int index)
        {
            var w = Space;
            if (index < 0 || index >= w.slots.Count)
                return false;
            var now = w.SizeOf(w.slots[index]);
            SetSize(index, now == FrameSize.Small ? FrameSize.Medium : now == FrameSize.Medium ? FrameSize.Large : FrameSize.Small);
            return true;
        }

        public void SetSize(int index, FrameSize size)
        {
            var w = Space;
            if (index < 0 || index >= w.slots.Count || w.SizeOf(w.slots[index]) == size)
                return;
            w.sizes[w.slots[index]] = size;
            Version++;
        }
```

`Capture()`: replace its `foreach (var w in spaces)` line with

```csharp
            foreach (var w in spaces)
            {
                b.Append(';').Append(w.name).Append('|').Append((int)w.layout).Append('|').Append(w.focus).Append('|');
                for (int i = 0; i < w.slots.Count; i++)
                {
                    if (i > 0)
                        b.Append(',');
                    b.Append(w.slots[i]);
                    var size = w.SizeOf(w.slots[i]);
                    if (size != FrameSize.Medium)
                        b.Append(':').Append((int)size);
                }
            }
```

`Restore()`: replace the inner `foreach (string id in f[3].Split(','))` loop with

```csharp
                foreach (string part in f[3].Split(','))
                {
                    if (part.Length == 0)
                        continue;
                    int colon = part.IndexOf(':');
                    string id = colon < 0 ? part : part.Substring(0, colon);
                    if (id.Length == 0)
                        continue;
                    w.slots.Add(id);
                    if (colon >= 0 && int.TryParse(part.Substring(colon + 1), out int n) && (n == 1 || n == 3 || n == 4))
                        w.sizes[id] = (FrameSize)n;
                }
```

- A layout kept before this change (no `:n`) restores as it was, every panel Medium: nothing the player arranged is lost.
- A panel id must never hold `:` `,` `|` or `;` (none of the 19 does).
- `Reset()` makes new workspaces, so sizes go back to Medium with it: right.
- Optional: a fourth field in `Defaults` (line 47) for starting sizes (Sea: voyage Large, hold Medium).

## 5. The tiling view (FrameRoot.cs) and the float (FrameRoot.Floats.cs)

a) The key (lines 428-430): add the sizes, so a resize draws the windows again. Insert before `"/" + FramePanels.Ids.Count`:

```csharp
                   "/" + string.Join(",", w.slots.ConvertAll(id => ((int)w.SizeOf(id)).ToString())) +
```

b) `OnDisable` (lines 108-109) and `DrawWindows` (lines 436-437): `p.Detach()` becomes `p.DetachBrief()`.
   In `OnDisable` also shut the full faces: `foreach (var p in FramePanels.All) p.Detach();`
   `DrawWindows` must NOT detach full faces: a change of workspace must leave an open floating panel up.

c) The tick (lines 326-328): a panel with only its full face open must tick too. Replace the loop with

```csharp
                foreach (var p in FramePanels.All)
                    if (p.BriefShown || p.Shown)
                        p.Tick();
```
   (`shown` is then used only for the detach in DrawWindows; keep it.)

d) `Window(...)` (line 493): the head gets a size button, and the body is the brief face inside one button.
   After the "next panel" head button (line 513) add:

```csharp
            var size = w.SizeOf(id);
            head.Add(HeadButton(FrameGlyph.Kind.Down, size == FrameSize.Small ? "One line. Press for three." : size == FrameSize.Medium ? "Three lines. Press for all." : "All lines. Press for one.",
                                () => { pick = -1; Windows.Resize(index); }));
```
   (a glyph of its own would be better than Down, which the title already uses; `FrameGlyph.Kind` is in FrameDrawn.cs, not read.)
   Replace lines 535-542 (`else if (panel != null) { ... }`) with:

```csharp
            else if (panel != null)
            {
                var press = new Button(() => panel.OpenFull()) { tooltip = "Open " + panel.Title };
                press.AddToClassList("frame-brief");
                var body = Box("frame-body");
                body.pickingMode = PickingMode.Ignore;
                press.Add(body);
                scroll.Add(press);
                panel.Frame = this;
                panel.AttachBrief(body, w.SizeOf(id));
                shown.Add(panel);
            }
            win.AddToClassList(size == FrameSize.Small ? "small" : size == FrameSize.Medium ? "medium" : "large");
```
   The window's own `PointerDownEvent` (line 501, trickle down) still gives it the focus first; the button still gets the click.
   In Rows the windows must now take the height of their lines, not share the column equally (the stylesheet, f).

e) Opening the full face. New methods in FrameRoot.Floats.cs, after `CloseFloat` (ends line ~147):

```csharp
        /// <summary>A panel's FULL face as a floating window over the chart (a press on its brief face). Pressed again
        /// while it is up, it comes to the front.</summary>
        public void OpenPanel(FramePanel panel)
        {
            if (panel == null || floatLayer == null)
                return;
            var f = floats.Find(x => x.spec.id == panel.FloatId);
            if (f != null)
            {
                ToFront(f);
                return;
            }
            var body = new VisualElement();
            body.AddToClassList("frame-body");
            var spec = new FloatSpec { id = panel.FloatId, title = panel.Title, content = body, look = "frame-float-panel" };
            spec.shut = () => ClosePanel(panel);
            OpenFloat(spec);
            panel.Frame = this;
            panel.Attach(body);
        }

        public void ClosePanel(FramePanel panel)
        {
            if (panel == null)
                return;
            panel.Detach();
            CloseFloat(panel.FloatId);
        }
```
   Check before pasting (the part of Floats I did not read, lines 150-200):
   1. `ToFront(Float f)` (line 150) only brings it to the front.
   2. `Build(f)` (line 167) puts `spec.content` into the scroll view in place of the lines, and draws no empty face place when `spec.face == null`.
   3. With no options, `mustAnswer` false and `holdsClock` false the float does not hold the clock (`StandingHint`, line 275, gives "": right).
   4. The number keys and Enter on the front float do nothing when `options.Count == 0` (`Pick`, line 291, guards the index: right).
   5. Several floats step down and right by `slot` (five places): a panel float takes a place like any other.
   Keys: Esc shuts the front float (there already). Enter on the focused left window should call
   `FramePanels.Get(Windows.Focused)?.OpenFull()` where the frame reads its keys (not read: where N, X, R, C, T are handled in FrameRoot.cs), but only while no float with options is in front, since Enter takes that float's marked choice.
   `FrameWindows.Show(id)` (line ~216: "a button anywhere can say show the Voyage window"): its callers now bring up
   only the brief face. Grep `Windows.Show(` and decide for each whether it should also call `OpenPanel`.

f) The stylesheet (Assets/Kapman/Game/UI/Frame.uss, not read; there is also Design/fixes/Frame.proposed.uss, not read). Draft only:

```css
.frame-brief { background-color: rgba(0,0,0,0); border-width: 0; margin: 0; padding: 0; align-items: stretch; flex-direction: column; }
.frame-brief:hover { background-color: rgba(0,0,0,0.06); }
.frame-brief-first { white-space: normal; -unity-text-align: upper-left; }
.frame-brief-line { white-space: normal; -unity-text-align: upper-left; opacity: 0.75; }
.frame-win.small, .frame-win.medium, .frame-win.large { flex-grow: 0; flex-shrink: 0; }
.frame-float-panel { width: 520px; max-height: 80%; }
```

## 6. What changes for the player, so nothing is lost by surprise

- Every control of every panel (buy and sell, pick a port, pay a debt, give up the course) moves out of the left column
  into the float. Nothing is removed: `Fill` is untouched. But until a brief face is pressed no button of a panel is on
  screen, and the first use is one click more than today.
- The picker in a window (the title button, `pick == index`, lines 521-534) is untouched.
- Tests: any EditMode test that calls `panel.Attach(...)`, reads `Capture()` strings or counts `shown` needs a look
  (Capture strings are unchanged while all sizes are Medium). The tests were not read.
- Columns widens the left side (`wide`, lines 444-446): with brief faces it may no longer be wanted. Left as is.
- Tabs shows one brief face at a time. Left as is; with brief faces Rows is the natural layout for all four workspaces,
  and the Self and House defaults (FrameWindows.cs lines 52-53) could go from Tabs to Rows. That is a ruling to ask for.

## 7. Not done

- Thirteen of the nineteen panels are marked [grep]: their brief lines name the Fill line to copy from, not a checked
  member. Only CrewPanel has a drafted `Brief` override.
- FrameRoot.Floats.cs lines 150-200 (ToFront, the start of Build) and the frame's key handling were not read.
- Frame.uss was not read; the style rules are a draft.
- Nothing was compiled.

## 8. Order of work

1. FramePanel.cs (section 3) + FrameWindows.cs (section 4): compiles alone; every brief face says "Press to open."
2. FrameRoot.cs + Floats (section 5) + the stylesheet. Smoke test: press, shut with Esc, change workspace with a float
   up, resize, quit and come back (sizes kept).
3. `Brief` overrides, panel by panel: the six [read] first (voyage, sky, hold, crew, ship, house).
4. EventPanel and ChestFramePanel `OpenFull` overrides.
