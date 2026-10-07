# Pop-ups and tiled windows a player can live with

7 October 2026. For the Command frame (layout G of `Design/ui_mock/template3.html`) as the user ruled it on
7 October: a settings bar, eight function buttons, tiled windows under them "like i3 or sway", and "lots of popups"
floating over the chart. Nothing here was run in Unity.

**What I read.** `GAME_MAKER.md`, `maker/hands.md` and `heart.md`, `fate_faith_and_self.md`, both critiques,
`DECISIONS.md`, `wiring_plan.md` (step 1, and the 7 October ruling at its foot), the mockup's code,
`research/ui/unity_ui.md`, `research/depth/old_school_huds.md` and `onboarding_flow.md`, and the code under
`Assets/Kapman/Code/Game/UI` and `Encounters`. **There is no `UI/Frame` folder yet.** Nothing of the frame is built,
so section 5 is a plan and not a review.

**The marks.** *Checked* means I read it in this run, on the page named in Sources or in the XML reference of the
editor on this machine (6000.6.3f1). **(?)** means from memory and not checked: this session's web searches were
already spent, so I could only fetch pages I could name, and several gave nothing. **(g)** is a first guess for the
soak to tune. Words in quotes from a web page are as the fetch gave them and may differ from the page by a word.
Sizes are in points: one point is one pixel at 1920 x 1080 (section 4.7).

## 0. The short answer

1. **One decision at a time.** One card is open, never two. It holds the clock but not your hands: every window can
   still be read and the chart still moved. Other cards wait behind a lamp on the clock.
2. **At sea only a card, a talk and a button's own window hold the clock. The tiled windows never do.** With no
   skipping of time a crossing is 18 to 96 real minutes. The player must be able to read the chest while she sails.
   This changes the rule of 4 October (section 7, question 1).
3. **Everything that happens is a log line first.** A pop-up is only a louder way to tell it. Missing one loses
   nothing, and most things never pop up at all: they are a count on a tab.
4. **A pop-up never covers what it is about, and says where to look**: a line to the place on the chart, a tag to
   the window, and a ghost number in that window while you hover a choice.
5. **The setting is on the pop-up.** Each has a small bell: "things like this: ask, warn, tell, count, log only".
   Thirteen classes, not two hundred message types.
6. **The tiled side is i3's tree made small**: four workspaces, at most four windows in each, each window a stack
   of tabs. Every key has a mouse way with a handle you can see. One undo. Nothing can be lost.
7. **The frame teaches itself by growing.** Minute 0 has one window and no bar. Tabs, a split and a second
   workspace each arrive once, when their thing exists, with a gold flash. Nothing in the first hour needs a drag.
8. **Four width classes** (narrow, standard, wide, double), each with its own saved layout. Large text and a small
   screen are the same case.
9. **Build**: a layout tree in plain C# (tested without Unity), a tile builder, one float layer with four child
   layers, one key router. Own splitter and own tab strip: Unity's `TwoPaneSplitView` and `TabView` do not fit.

## 1. Findings

### 1.1 Games with many pop-ups and many windows

| Game | What it does | What players say | Kapman takes | Kapman leaves |
|---|---|---|---|---|
| **Crusader Kings II** | Each message type has one of five priorities: a pop-up like an event; a high feed; a low feed; the log only; not shown. Set for each category and each source (checked) | The wiki's own warning: "too many and game will become a headache" (checked) | The five steps as one ladder | A setting for every type and source: a table nobody opens before hour fifty |
| **Crusader Kings III** | An event is a window with a face, a text and two to four choices; small things are toasts that fade and a feed; standing "issues" sit at the top (?) | The 1.5 patch notes show the complaint and the cure: "Players will no longer be spammed with letter events when someone wants to invite their children to a playdate" (checked) | The event window is the card, as the mock already draws it | Small asks dressed as cards; the same letter three times running |
| **Europa Universalis IV** | About 80 kinds of alert as banners in three colours, red the most urgent, then yellow, then green; a click opens the screen it is about (checked). A setting for each message type (pop-up, log, pause), reached by a right click on the message (?) | Old hands turn most pop-ups to the log; new players drown (?) | A place for things that are still true; a click goes to the matter; the setting reached from the message | Eighty kinds |
| **Stellaris** | The outliner at the right edge: four tabs, a gear to choose its lists and their order, a double click centres on an item. Alerts: "Square alerts remain until the circumstance changes or the player right clicks them to dismiss"; round ones go by themselves; red, orange, green by urgency (checked) | | Two kinds of notice: one that passes, one that stands. A right click sends one away | The outliner (the book cut it) |
| **Dwarf Fortress** | About 200 announcement types, each with flags: `BOX` "causes a box to appear in the middle of the screen, requiring a click on the Okay button to close it"; `P` pause; `R` recentre; `D_D` the list; `ALERT` under the alert button. The Steam version sets them in Settings (checked). In practice three tiers: small; great (pause and recentre); game-changing (those and a box) | Stopping the pause and the recentre on small things is among the first changes players make (?) | The route is flags in data, a row for each kind of event. Three tiers | Recentring by itself: Kapman's chart never moves on its own |
| **RimWorld** | Letters: envelopes at the right; "blue envelopes are good events, grey is neutral, yellow is bad, and red denotes direct threats". "Other events will pause the game and pop up a window where the player must make a choice." A click on the envelope "offers the option to jump" to the place (checked, for one event). A right click sends a letter away; alerts are a second list of things still true and cannot be sent away; the game can pause itself on a threat (?) | Held up as the readable one (?) | Letters (once) apart from alerts (still true): the same split as Stellaris. A click jumps to the matter. Colour and shape together | A tall pile of unread envelopes: Kapman's go to the log by themselves |
| **EVE Online** | Every screen is a free window. Windows stack into tabs when one is dropped on another, snap to each other and to the edges, pin, lock, shrink (?) | Strong, and the usual first-hour word for it is window soup; old hands pass saved layouts round (?) | Snap; tabs by a drop; a lock | Free placing of everything; a start that needs arranging |
| **Football Manager** | The inbox is the hub. One Continue button, which will not go on while an item must be answered (?). FM26 rebuilt the interface: "confusing and overwhelming" (Dexerto); "mostly negative user reviews on Steam upon release, with players criticising the new interface" (checked) | | What must be answered blocks the clock, and the blocked control says what blocks it. A learned layout is never re-shuffled by the game | News that looks like a must-answer |
| **Uplink** | A pretend desktop: small tool windows you place while a trace runs and the clock does not wait (?) | | A frame of instruments is a pleasure in itself | Windows opening on top of each other at one spot |
| **Factorio** | One main window at a time; another opening shuts the first; one key shuts whatever is open (?) | | One closing key and one order of closing | |
| **Dear ImGui, docking** | A window is docked by dragging its title bar or its tab; holding Shift stops it docking; a preview with five targets: centre, left, right, up, down; saved to an `.ini`; `DockBuilder` sets the start by code (checked) | | Exactly this for the mouse: five drop targets and a preview before release | Nesting with no limit; a window torn loose by accident |

### 1.2 Tiling window managers

What i3, sway and dwm get right (checked: the i3 user guide, sway(5), the dwm tutorial):

- **A tree.** i3: containers hold windows or containers, and each split has a direction. Nothing overlaps, nothing is
  lost, no space is wasted.
- **Tabs are only a layout.** Four layouts of a container: split across, split down, stacking, tabbed.
- **Workspaces by number**, each with its own tree. dwm has nine tags.
- **One focus.** Keys go to it. It moves by direction.
- **Mark, do not steal.** sway's `focus_on_window_activation` is `urgent` by default: a window that wants you is
  marked on its workspace button, and you go when you choose.
- **The mouse came late, and it works.** i3 4.21 added `tiling_drag`: drag a title bar to move a tiled window.
  sway has it on by default, with 9 pixels of movement before a drag begins. Borders drag to resize.
- **dwm's two ideas**: one master window with a stack of small ones beside it; and monocle, one window whole.

What has to change for a player who has never seen one:

| i3 or sway | Why it fails a first-time player | Kapman |
|---|---|---|
| All behind a modifier key and a config file | Nothing to see, so nothing to find | Every act has a handle, a cursor and a line on the hint line. Keys are the second way |
| The pointer moves the focus (i3's default) | In Kapman the window in focus sets the chart's lens (the hand's rule 3: the panel asks, the chart answers): the chart would flicker as the pointer crossed the column | Click to focus |
| A new window splits wherever the focus is | The layout changes under you | A new panel arrives in its saved home. "+" opens a picker in place |
| Nesting to any depth | Nobody can see the tree; a drop makes a sliver | Two levels, four windows, smallest sizes, snaps |
| Layout keys act at once | One stray key and "my windows are gone" | Window keys are behind Alt; every change has an undo and says what it did |
| Closing a window kills the program | Fear | Closing hides a view. The panel is always in the picker |
| Workspaces made on demand | Lost among empty ones | Four named ones, which arrive with the game |
| No sense of what the game is doing | | The column follows the ship: Port at the quay, Sea under way |
| Any window may float | A panel over the chart hides the key to the game | Panels stay tiled. Only cards, talks, notices and a button's window float |
| Moving and resizing by keys or a free drag | A pad has neither | Snap steps: move by drop target, resize by twelfths |

### 1.3 Seven lessons

1. Keep "it happened" apart from "it is still true". (RimWorld's letters and alerts; Stellaris's round and square.)
2. The log takes everything. A pop-up is a loudness, not a place. (CK2's fourth priority; Dwarf Fortress's list.)
3. Holding the clock is a flag used rarely, and the player can always see what holds it. (Football Manager's
   Continue.)
4. Put the setting on the message.
5. A click on a message goes to its matter. The view never goes there by itself.
6. A drag has a threshold, a preview and a way out. (sway's 9 pixels; ImGui's five targets.)
7. Mark, do not steal. A thing that wants you wears a count. Only a card may take the focus, and it waits half a
   second before it takes a key.

## 2. The load, counted for this game

The clock is the reason this matters. One game minute is one real second at 1x, and nothing is skipped.

| | 1x | 2x | 4x |
|---|---|---|---|
| A watch of four hours | 4 real minutes | 2 | 1 |
| A game day | 24 real minutes | 12 | 6 |
| Dorestat to Lundenwic, 3 to 4 days | 72 to 96 real minutes | 36 to 48 | 18 to 24 |

The book's limits: four signs a game day, never the same one twice in six hours; two cards a leg, none on a leg
under half a day; in port one card a call and a chain step. From `onboarding_flow.md` P6: two warnings and six
counted things a game day.

So at sea the clock is held at most about five times a game day: once in five real minutes at 1x, once in 72
seconds at 4x. A whole crossing has at most 14 signs and 2 cards; six to nine holds is likely (g). That is the most a
player will bear while reading in a window. Four rules keep it there:

- **R1. The gap.** After a hold ends, no new sign or card is drawn for one game hour (60 seconds at 1x, 15 at 4x)
  (g). Danger ignores the gap: aground, raiders, the king's men. So does a chain step that follows from your answer.
- **R2. Notices are counted in real seconds, not game time.** At most two in any 20 real seconds, three on screen.
  The rest go to the log and the counts only. So 4x is not four times louder.
- **R3.** The same thing twice in a game hour is one line with a count (`onboarding_flow.md` P6).
- **R4.** Nothing that only tells may hold the clock.

One more, for the things you ask for yourself: **the answer to your own press is never counted away.** Sound, Look
out and Measure always show their answer at once.

## 3. The floating windows

### 3.1 Three laws

- **One decision at a time.** One card open. While it is unanswered, every press that changes the game is grey,
  with one reason on the hint line: "First answer: the lookout." Reading is never blocked: windows, tabs,
  workspaces, the chart, Measure, Look out, the chest.
- **You can always see what holds the clock.** The clock in the settings bar turns gold and reads "Held: the
  lookout waits." Press it and the card comes to the front. When the hold ends the clock goes back to the speed it
  had (1x, 2x or 4x).
- **The log remembers.** Every float writes its line when it opens, and a card writes its outcome when answered.
  Closing or missing a float loses nothing.

### 3.2 The four kinds

| | **Card** | **Talk** | **Notice** | **A button's window** |
|---|---|---|---|---|
| What it is | A choice you must make. A *sign* is a small card from one of your four men | A person speaking, and your replies. You opened it: People here, or Hail | A fact, told once. A *warning* is a notice that stays while it is true | The choices of a function button: Wait, Look out, Hail, Offer |
| Clock | Held | Held (at sea; in port it is held anyway) | Runs. A warning drops it to 1x once (`GameSpeed.Slow` exists) | Held while open |
| Takes the focus | Yes, at once; it takes no key and no click for its first 0.5 real seconds | Yes: you opened it | Never | Yes |
| Size | 520 wide, up to 560 high. A sign: 380 wide | 520 wide, up to 480 high | 320 wide, one or two lines of 62 letters | 360 wide |
| Looks | Gold title band with the hold sign (two bars). 4-point border, a hard shadow of 6. The chart, and only the chart, dims by a quarter. A face of 64, the speaker's name, the text, numbered choices at least 40 high, each with its cost line; a risk word only where there is risk. No close button: a low bar, "set aside" | Lilac title band with a speech sign. 3-point border. A face of 64, the name, how they feel toward you as a word, the turning line, numbered replies with the stat tag in lilac. A close button. When nothing is left to say the band greys: "said" | No title band, a 2-point border, no shadow, the sign of its class at the left. A warning has a flag sign and a gold left edge | A plain band with the button's own sign, and a tail to the button that opened it |
| Where it opens | A berth beside the middle of the chart, on the side away from what it is about (3.3) | The berth the card does not use; the next ones step down and right | A strip at the top right of the chart, under the day rule; newest on top, warnings above all | Beside the button rack, level with the pressed button |
| Most at once | 1 open, 3 waiting behind the lamp | 3 in port (the three pips), 1 at sea | 3 passing and 2 warnings | 1 |
| Sent away by | Answering. Esc sets it aside, still holding | Close, Esc, or "Take your leave", always the last reply | Time: 6 real seconds, and the pointer over it stops the count. A right click. A click goes to what it is about. A warning goes when its matter ends | A choice, Esc, a click outside it, or its button again |
| Keys | 1 to 9. Enter: the marked choice. Space on a sign: "as ordered". Then Enter or Space: "Go on" | 1 to 9, Esc | None | 1 to 9, Esc |
| After | The outcome is written in the same window, with "Go on". It closes, and a waiting card opens 0.4 seconds later | Stays until closed. Its gist is already in the log and in The word | Fades in 0.6 seconds | Closes |
| In this game | The steward and the stick; "What tidings?" on arrival; the wind has backed; a sail closing; the two men on the drying bank; the count's card at Michaelmas | The keeper, a skipper just in, the benches; a Dane with a verse in his own tongue; a hailed fishing boat | "A word: wine pays well at Lundenwic." "Tongue has grown." Warning: "Bread for three days." | "Wait: till first light, till the flood, till the wind is fair" |

Each kind has its own sign in the corner as well as its own colour, so the kinds can be told apart by shape. A float
opens in 0.15 seconds, with no bounce. All float timing is in real seconds, because the time scale is nought while
a card holds. A sound for each kind is one public-domain recording or none.

The Esc key closes in one order: a drag in hand; a button's window; the talk in front; then it sets the card aside.
With a card set aside, Esc brings it back. Only with nothing open does Esc open the menu.

### 3.3 Where they open: berths, steps, and never on the matter

- The chart's free rectangle is what the frame's pieces leave: about 864 x 800 in the standard class. Step 1 of
  `wiring_plan.md` already names it `ChartSafeRect`.
- There are two **berths**: left (16 in from the tile column) and right (16 in from the portrait's column), each
  centred in height. A card takes the berth farther from what it is about. With nothing to point at it takes the
  left berth, beside the windows you read.
- If it would still cover its matter it slides up or down. If it cannot clear (the narrow class), it stays, and its
  tag says where in words: "off Texel, a day north-east".
- The first talk takes the berth the card does not use. Each next talk opens 32 right and 32 down: a title's height
  and a little, so every title stays in reach. After the third it starts again.
- Where you drag a kind, the next of that kind opens. The step from its berth is saved for each kind.
- A float's title can never leave the screen. After a change of screen size all are pulled back in.
- Nothing is placed by the game over the notice strip (the top 96 of the free rectangle), over another float's
  title, or over the day rule, the log, the portrait or the ship window. You may drag one there.
- From back to front: the chart; rings and the tether; the frame; talks, the last one clicked in front; the card; a
  button's window; notices; a window being dragged. A talk can never cover a card.
- A click anywhere on a float brings it to the front and gives it the focus, as the mock does on mouse down.

The mock opens every float 42% of the screen wide at the centre. The free chart is 45% wide, so one float hides
nearly all of it. That is why the card is 520 wide and sits to one side.

### 3.4 The queue and the lamp

- A card holds the clock, so the world cannot make a second one while it is open. Only cards born in the same
  moment wait, or one you bring on yourself: mooring with a chain step due.
- The order: danger; signs; chain steps; port and story cards. Within one class, the oldest first.
- At most three wait. A fourth of the lowest class becomes a log line and takes its own safe choice, which its data
  must name. A card with no safe choice is never dropped.
- A waiting sign whose matter has ended is dropped with a line: "The wind went back west before you answered."
- The lamp is the clock's own place in the settings bar: "Held: the lookout waits. 2 more." This is the book's Event
  lamp, moved from the rack: the rack is now eight verbs.
- **Set aside.** The card shrinks to its title on the notice strip so you can see the chart. The clock stays held.
  The lamp, its title, or any grey press brings it back.
- **The half second.** For 0.5 real seconds after a card opens, its choices fade in and take no key and no click.
  This is for the "2" you were pressing for workspace 2.
- No card opens while you are dragging anything. It waits until you let go.
- A save is refused while a card is open or waiting, with the reason on the hint line. `SaveGame.Busy` already
  refuses during a meeting. A save closes talks and a button's window.

### 3.5 What never pops up

Every event goes through one router, set in data, on CK2's ladder: **ask** (a card), **warn**, **tell** (a
notice), **count**, **log**.

| What happens | Route | Where it shows |
|---|---|---|
| A price or a word heard; the gist of a talk | count | The word; ink on the chart; the log |
| The wind veers and the order still holds | log | Wind and tide; the ship window shows it |
| The wind turns so the order cannot be kept | ask (a sign) | The weather man's head lights |
| A sail seen far off | count | The field; a grey sail on the chart |
| A sail closing on you | warn, then ask in hailing reach | A ring on the sail |
| Dawn, noon, dusk, the turn of the tide | log | |
| Dusk with no landing in reach | ask (a sign) | |
| Bread eaten, wages paid | log | Hold |
| Bread for three days or less; bread shorter than the way | warn | Hold; the bread line on the day rule |
| A man low in heart | count; warn under a third | Crew; his head dims |
| News off a ship | count | Chronicle |
| One of the hours of history (`fate_faith_and_self.md` 4) | ask | |
| A feast eve, a fast day | count | Faith and days; a flag on the day rule |
| A debt or a promise: seven days off, two days off, the day | flag, warn, ask | Tasks; the day rule |
| A stat rises; a bond deepens; a word of a tongue learned | tell | Yourself, Bonds, Tongues |
| A new window arrives | tell | The window flashes gold twice |
| Arrival: "What tidings?" | ask | |
| The reveal on arrival | In the arrival card's outcome | The log |
| The reckoning after the sale | tell, and it stays until clicked | Trade; the log |
| Aground, raiders, the king's men | ask (danger) | |
| Saved; a setting changed; an undo | The hint line | |
| What a control does and costs | The hint line | Never a box that follows the pointer |
| A tip for a stalled player | log, as a line from one of the men (`onboarding_flow.md` P7) | |
| Another house's ship that you cannot see | nothing | You learn it later, as news |

**Counts.** A count is a gold dot with a number, to 9 and then "9+". It sits on the tab. If that tab's workspace is
not showing, the sum sits on the workspace's button. It clears when the panel has been in view for one second, not
on a click.

### 3.6 How a pop-up points at what it is about

1. **To the chart: the tether.** A 2-point gold line runs from the float's nearest edge to a ring of 20 round the
   matter: a port, a sail, a bar, her own mark, or a flag on the day rule. Only the float in focus draws its line,
   so there is never more than one across the chart. The others keep a faint ring. The ring beats twice when the
   float opens.
2. **Off the edge: an arrow.** It sits at the chart's edge with the name and the way in days: "Lundenwic, 3 days
   west". A click moves the chart there, and F brings it back to her. The chart never moves by itself.
3. **To a window: the tag.** A float names its window at its foot: "Hold", "Tasks". Hover the tag and that window's
   title glows gold; if it is on another workspace, that workspace's button glows. A click goes there. If no window
   shows that panel, it opens as a tab on the largest stack of its home workspace.
4. **The ghost number.** Hover a choice, and each cost shows in the window where that number lives, before you
   press. The bread row in Hold reads "7 days, then 4" in gold. The silver in the settings bar reads "74 d, then
   54". The day rule shows a pale stretch for the hours lost. This is the hand's rule 5 ("the cost is on the control
   before the press") carried into the windows. It is also the answer to the critics' "cards have no teeth": you see
   the bite before you choose.
5. **Back the other way.** A ring on the chart, a count on a tab and a line in the log can all be clicked, and each
   opens what it stands for.
6. **The portrait** is the face of the float in focus. With none in focus it is you. When a man speaks a sign, his
   head lights among the four.

### 3.7 The player's own settings

**On the pop-up.** Every float has a small bell in its title. It opens one line: "Things like this: Ask me, Warn
me, Tell me, Count it, Log only." A step that cannot be chosen is grey, with the reason.

| Class | Starts as | May be set |
|---|---|---|
| Danger: aground, raiders, the king's men | ask | ask only |
| Signs: the order cannot be kept | ask | for each of the five signs: ask, or "as ordered, and tell me" |
| People and story cards | ask | ask only: they are decisions |
| Debts, promises and their days | flag, warn, ask (3.5) | the two early steps from warn to count; the day itself is ask |
| Other sails | count | warn to log |
| Wind, tide and sky | log | tell to log |
| Trade and the word | count | tell to log |
| Bread, wages, stores | log; warn at three days | the three-day warning never below count |
| Crew | count | tell to log |
| Faith and feasts | count | tell to log |
| News and the chronicle | count | tell to log |
| Yourself, bonds, tongues | tell | tell to log |
| The frame: a new window, a save | tell, the hint line | tell to log |

"As ordered, and tell me" on a sign is a rule of play and not only a switch. The book says: "An order that covers
the case is obeyed without a word." The player is writing standing orders. The man still speaks, in the log.

**In Options, "Windows and messages":**

- Notices stay 4, 6 or 10 seconds, or until clicked. Starts at 6.
- Slow to 1x on a warning: on.
- Dim the chart behind a card: on.
- Tethers: on.
- The turning line in a talk: slow, steady, quick, at once.
- Windows follow the ship: on (4.2).
- Moving windows: drag the name; only with Alt held; locked. Starts as drag the name.
- Size: small, normal, large, very large (0.9, 1, 1.15, 1.3).
- The frame's width on a very wide screen: the whole screen, 21:9, 16:9.
- **Quiet watch.** A bell in the settings bar. While it is lit, until the next port, every "tell" is a "count".
  Cards and warnings still come. It is for reading the chest while she sails.
- Three presets: Everything told; Steady (the start); Old hand (cards and warnings only).

Three things cannot be turned off: the cards of the decision classes, the hold lamp, the log.

## 4. The tiled side

### 4.1 The model

- A workspace has one tree. A node is a **split** (rows or columns, two or three children, each with a share) or a
  **stack** (one or more panels as tabs, one showing).
- Two levels of split at most. Four stacks a workspace in the standard class; 2 narrow, 6 wide, 8 double.
- **Chrome only when there is more than one.** A stack with one panel has no tab strip: its title is its tab. The
  workspace bar shows only when there are two workspaces. A grip shows only between two windows. (sway does this
  with borders: `smart_borders`.)
- A panel may be in several workspaces, once in each, as in the mock. There are seventeen panels. The mock's
  eighteenth, Event, is now the card.
- The smallest stack is 280 x 132: a title of 28 and four rows of 26. A grip stops there.
- The column has a width for each workspace: from 420 to half the screen, and never so wide that the chart has less
  than 640. It starts at 557 (the mock's 29%), and at 883 for a workspace in columns (the mock's "wide").
- **Fold.** Press the lit workspace button and the column folds to its bar; the chart runs to the rim (book, law 2).
  Any workspace button unfolds it.
- The mock's Rows, Columns and Tabs buttons stay. They are three ready arrangements of the same tree, for players
  who never drag.
- A window in focus has a 3-point gold outline. Its title: the two arrows, the name with a small arrow for the
  picker, the count, the close button.

### 4.2 The default workspaces, at the start and as the game grows

The frame "as it came" has a home for all seventeen panels. A panel that has not arrived is hidden and its share
goes to its neighbours. So a panel arriving is only un-hidden, in a home you can already have moved. The game never
re-shuffles what the player arranged.

| Workspace | As it came (standard and wide) | Narrow | Double |
|---|---|---|---|
| **1 Port** | Rows: [People here, Tasks, Crew] 45; [Trade, Hold, The word, Ship] 55 | The same two stacks | The two stacks side by side, 883 wide |
| **2 Sea** | Rows: [Voyage] 40; [The field, Crew] 30; [Wind and tide, Hold] 30 | [Voyage] 55; [The field, Wind and tide, Crew, Hold] 45 | Voyage at the left; the two small stacks at the right |
| **3 Self** | Rows: [Yourself, Bonds, Tongues] 50; [Faith and days, Chest, Chronicle] 50 | The same | Side by side |
| **4 House** | Columns, 883 wide: [House, Ship]; [Tasks, Chronicle] | Rows, 520 wide | The same as standard |

Square brackets are one stack; the first name is the tab that shows. Numbers are shares in hundredths.

Each panel has one **first home**: the first workspace above that lists it. A workspace arrives with the first
panel whose first home it is. So Tasks, Chronicle and Ship have a place long before 4 House exists, and 4 House
arrives only with House.

| When | What arrives | Where |
|---|---|---|
| Minute 0 | People here | 1 Port, alone, the whole column. No bar, no tabs |
| The steward's card is answered | Tasks | A tab behind People here, with a count of 1 |
| The first word of a price | Trade, and The word behind it | A second window under People here |
| The first lot bought | Hold | A tab with Trade |
| The first port picked on the chart, or the first Measure | Voyage, and with it **2 Sea** and the workspace bar | |
| The keeper first names who sailed | The field | Under Voyage |
| The first cast off | Wind and tide | Under The field |
| The first sign spoken by a man | Crew | A tab with The field, and with People here |
| The first feast eve or the first Offer | Faith and days, and with it **3 Self** | St John's eve is day 3, so it falls on the first crossing |
| The first reckoning, or the first press on your own portrait | Yourself | 3 Self. The reckoning names the stat that rose |
| The first talk in a tongue not your own | Tongues | 3 Self |
| The first person who becomes "known" | Bonds | 3 Self |
| The first text or dated news | Chest, Chronicle | 3 Self |
| She is first hurt, or first lies at a yard | Ship | 1 Port, a tab with Trade |
| The first Martinmas | House, and with it **4 House** | Tasks, Chronicle and Ship show there too |

**Windows follow the ship.** On cast off the column turns to 2 Sea. On mooring it turns to 1 Port. It turns only if
you are on the other of those two. If you are reading in 3 Self it leaves you there and the button glows.

After the first winter the bar's menu can add two workspaces of your own, 5 and 6, which you name.

The 7 October ruling leans on the chest to make the leg worth its minutes. The book puts the Chest in stage 2. If
the leg is to have reading in it, the first evening must give a first text (section 7, question 6).

### 4.3 The first ten minutes, with nothing to read

Six principles. Start with one window. Each new piece arrives once, with its thing. Every handle shows itself under
the pointer: a cursor, a gold edge, a line on the hint line. Nothing in the first hour needs a drag. Any change can
be undone. No teaching pop-ups: the hint line and the men do it.

| About | The frame does | The player does | It teaches |
|---|---|---|---|
| 0:00 | One window, People here, fills the column. No bar, no tabs. Buttons that cannot be used are grey. A card: the steward, his face in the portrait, a gold line from the card to the flag "St John: 40 d" on the day rule. The clock is gold: "Held: the steward waits" | Clicks a choice, or presses 2 | A card comes beside the middle, must be answered, holds the clock and points at what it is about. Numbers answer it |
| 0:30 | The outcome is written in the card, with "Go on". Then a notice: "A new window: Tasks." Tasks slides in as a tab behind People here with a count of 1, and beats gold twice | Clicks the tab, or does not | Tabs. A count means something new is here |
| 1:00 | People here shows three rows, each with Talk | Clicks Talk on the keeper. A talk opens with a line to Dorestat's mark. Clicks a skipper: a second talk opens a step down and right | Talks are windows. Several can be open. The pointer becomes a hand on a title: it can be dragged |
| 2:00 | The skipper's word. Lundenwic inks in, with a ring. A notice fades. Trade arrives as a second window under People here, in 0.3 seconds | Reads it | Windows tile. A new one takes its share and nothing overlaps |
| 2:30 | A grip shows between the two windows under the pointer. The hint line: "Drag to give one window more room. Double click: equal shares." | Drags it, or not | Resizing. Not needed |
| 3:00 | Buys wine. Hold arrives as a tab with Trade | | Tabs again |
| 3:30 | Clicks Lundenwic on the chart. The bar appears at the foot of the column: "1 Port", "2 Sea". "2 Sea" glows with a count. Cast off is grey: "Set a course first: Sea, Voyage." | Clicks "2 Sea" | The column has pages. A glowing button has something new |
| 4:00 | Voyage, alone. He picks a preset; the order reads back as one sentence; Cast off lights | Presses Cast off | The buttons do. The windows show and set |
| 4:10 | She is away. The field and Wind and tide arrive under Voyage | | |
| 5:00 to 8:00 | The first sign, which the first leg brings within two game hours: the weather man, a small card, Space for "as ordered". Then a notice: "A sail ahead", with a ring on the chart | Presses Space, or chooses | A sign is a small card. A notice fades and needs nothing. The log keeps both |
| About 8:00 | If he has gone between the two workspaces three times in a minute, the hint line says: "Drag a window's name onto a page's button to keep it there." | Maybe | Moving a window |
| 9:00 | A card with a cost in bread. Hovering the choice turns the bread row in Hold gold: "7 days, then 4". If Hold is on the other page, "1 Port" glows | Hovers, chooses | A pop-up points into the windows |
| 10:00 | He has answered two or three cards, held two talks, seen four notices, used two pages and three windows and a tab. He has dragged nothing unless he wished | | |

**How to know it works.** Three people who have never seen a tiling manager. By minute 10 each has switched
workspace alone and none has asked where a window went. Note: seconds to the first switch; how many pressed undo;
whether anyone locked the windows.

### 4.4 Every key has a mouse way, and a pad way

| To | Mouse | Keys | Pad |
|---|---|---|---|
| Focus a window | Click in it | Alt+arrows by direction; Tab and Shift+Tab for next and last | Left trigger held, then the D-pad |
| Change workspace | Its button | 1 to 6 when no float is in focus; Alt+1 to 6 always | The two bumpers |
| Fold the column | The lit workspace button | The key under Esc | Click the left stick |
| Change the panel a window shows | Click its name: the picker. The wheel over the title, or the two arrows: next and last | Alt+P; Alt+comma, Alt+stop | Y |
| A new window | "+" on the bar. It splits the window in focus along its longer side and opens the picker in the new half | Alt+N | Left trigger and Y |
| Close a window | Its close button; a middle click on its title | Alt+X | Left trigger and X |
| Move a window | Drag its name. Five targets show on every other window: the middle, and the four sides | Alt+Shift+arrows | Left trigger and A lifts it; the D-pad picks the target; A drops; B cancels |
| Make it a tab of another | Drop on the middle | The same | The same |
| Send it to another workspace | Drop on that workspace's button. Hold it there 0.6 seconds to look first | Alt+Shift+1 to 6 | Lift it, a bumper, drop |
| Resize | Drag the grip between two windows. Drag the column's right edge | Alt+Ctrl+arrows, a twelfth a press | Left trigger and the right stick, a twelfth a push |
| Equal shares | Double click a grip | Alt+= | |
| One window whole (dwm's monocle) | Double click its title; again to go back | Alt+Enter | Left trigger, click the left stick |
| Arrange all: rows, columns, tabs | The three buttons on the bar | Alt+R, Alt+C, Alt+T | The bar is reached by the D-pad |
| Undo the last change to the windows | The arrow on the bar, lit for 30 seconds after a change | Ctrl+Z | Left trigger and B |
| The frame as it came | A right click on the bar: "This workspace as it came", "All as they came" | | The same menu |
| Focus the next float | Click it | Tab goes through the floats first | View |
| Move a float | Drag its title | | Left trigger and the right stick: six places |
| Answer | Click the choice | 1 to 9, Enter, Space | The D-pad and A |
| Set a card aside; bring it back | Its low bar; the lamp | Esc; Esc | B; B |
| Go to what it is about | The tag, the ring, the edge arrow | G | X |
| The eight verbs | Click | Section 7, question 5 | Right trigger held: a wheel of eight |

**Why Alt.** The mock's bare R, C, T, N and X act at once. One stray key re-lays the windows, which is the worst
first hour a tiling manager gives. Bare letters are also wanted for the verbs. And one modifier for every window act
is i3's own way. While Alt is held, every title shows its keys, so the frame labels itself. Workspaces are not on
the function keys: Alt+F4 shuts a Windows program. Arrows alone walk the rows of the window in focus.

A pad never needs a free drag. A window moves by drop target, a float sits in one of six places, a share changes by
twelfths.

### 4.5 Safety

- **Undo.** Ten steps of changes to the windows, for this sitting. Each change says what it did on the hint line:
  "Trade moved under Voyage. Ctrl+Z puts it back."
- A drag begins after 8 points of movement. Esc or the right button drops it. A drop on nothing changes nothing.
- **A preview before release.** The half, or the whole, that the window would take is drawn in gold. A drop that
  would break a limit is not offered: a fifth stack, a third level, a window under its smallest size.
- **Lock.** "Moving windows: locked" hides the grips and makes the titles plain.
- **Nothing can be lost.** Closing a view leaves the panel in the picker. A count for a panel shown nowhere sits on
  its home workspace's button, and a click opens it there as a tab.
- The last window of a workspace cannot be closed, as in the mock.
- The picker shows only panels that have arrived. One already on this workspace is dim, and choosing it swaps the
  two, as the mock does.

### 4.6 What is saved

| Kept with | What | Why |
|---|---|---|
| The player: one `frame.json` beside the saves, not in them | For each width class: each workspace's tree, shares, width, folded or not, and the one last shown. For each kind of float: its step from its berth. All of 3.7. Size. Keys | A new game, an heir and an old save all open in your frame |
| The save game | Which panels have arrived (the `hud_` flags through `ContentFlags`, `wiring_plan.md` step 1). Counts not yet seen. The log. "As ordered" on each of the five signs: they are orders and belong to the voyage | They are facts of this life |
| Nothing | Open talks, a button's window, notices, the undo list | A save and a load close them |
| Refused | A save while a card is open or waiting | 3.4 |

A save with fewer panels arrived than the frame file holds: the tree is shown with those hidden, and is not
changed. A frame file from another version: unknown panels are skipped, missing ones go to their home "as it came".
A broken file: the frame as it came, one line on the hint line, and the bad file kept as `frame.bad.json`.

### 4.7 From 1280 x 720 to a very wide screen

The scale is s = the smaller of width / 1920 and height / 1080, never under 0.75, times the Size setting. Points are
pixels / s. If the kit becomes pixel sprites, s becomes a whole-number step as in `research/ui/unity_ui.md`
section 2; the classes below are in points and hold either way.

| Screen | s | Points | Class | Reading text (18) in pixels |
|---|---|---|---|---|
| 1280 x 720 | 0.75 | 1707 x 960 | narrow | 13.5 |
| 1280 x 800 hand-held, Size large | 0.86 | 1484 x 928 | narrow | 15.5 |
| 1366 x 768 | 0.75 | 1821 x 1024 | narrow | 13.5 |
| 1600 x 900 | 0.83 | 1920 x 1080 | standard | 15 |
| 1920 x 1080 | 1 | 1920 x 1080 | standard | 18 |
| 1920 x 1200 | 1 | 1920 x 1200 | standard | 18 |
| 2560 x 1440 | 1.33 | 1920 x 1080 | standard | 24 |
| 3840 x 2160 | 2 | 1920 x 1080 | standard | 36 |
| 2560 x 1080 | 1 | 2560 x 1080 | wide | 18 |
| 3440 x 1440 | 1.33 | 2580 x 1080 | wide | 24 |
| 5120 x 1440 | 1.33 | 3840 x 1080 | double | 24 |
| 1920 x 1080, Size very large | 1.3 | 1477 x 831 | narrow | 23.4 |

Narrow is under 1850 points wide; standard 1850 to 2399; wide 2400 to 3299; double 3300 and over.

| | Narrow | Standard | Wide | Double |
|---|---|---|---|---|
| Tile column at the start | 520 | 557 | 557 | 883 |
| Portrait and ship column | 400 | 461 | 461 | 461 |
| Free chart width at 16:9 or its own shape | 755 | 864 | 1504 | 1138 inside a 21:9 frame, and more beyond it |
| Stacks a workspace | 2 | 4 | 6 | 8 |
| Card width | 460 | 520 | 520 | 520 |
| Talks open in port | 2 | 3 | 3 | 3 |
| Notices on screen | 2 and 1 warning | 3 and 2 | 3 and 2 | 3 and 2 |
| Berths | Left; the centre if it must | Left and right | Left and right | Inside the frame's width |

- **The chart keeps 640.** If the chart would have less, the ship window folds to its caption and the portrait to
  96, so the right column is 300; then the tile column gives, down to 420. Very large text at 1080p: 1477 - 436 -
  316 = 725.
- **A layout for each class.** The first time in a class with no layout, one is made from the nearest. Going down,
  the smallest stack becomes tabs of its neighbour until the count fits. Going up, it is copied.
- **On a double-wide screen** the frame's pieces keep to the middle 21:9 at the start, and the chart runs on beyond
  them at both sides. The far edges of such a screen are out of easy sight.
- **On a wide screen** the extra goes to the chart at the start. The player may widen the column to half the screen
  and use six stacks.
- **Every panel must work in any stack from 280 x 132 up.** That is a rule for whoever writes a panel: one column of
  rows, a scrolling body, no fixed widths. `MarketPanel` today is a box 1260 pixels wide (`unity_ui.md` section 1).
  It must be re-laid as rows before it can be a tile.

## 5. Building it in Unity 6 UI Toolkit

### 5.1 What is there today

- `Assets/Kapman/Code/Game/UI/Frame` does not exist.
- UI Toolkit: `MapTable` (the top bar and the outliner), `MarketPanel`, `TextPanel`, `ChestPanel`, `Notices`.
- **The card is IMGUI.** `EncounterPanel` draws in `OnGUI` and reads the number keys itself.
- `Notices` fades three labels at the top in seven seconds, in unscaled time. `Alerts` holds or slows the clock.
  `PanelClock.PanelOpen` holds it while four named panels are open.
- At least ten files read `Keyboard.current` for themselves (`EncounterPanel`, `MarketPanel`, `TextPanel`,
  `MapTable`, `MapView`, `GameSpeed`, `MainMenu`, `KapmanGame` and others). With digits for both choices and
  workspaces, two of them will answer the same press.
- Input System 1.20.0 is the only input handler. `Assets/InputSystem_Actions.inputactions` is the project-wide
  asset and has a UI map with Navigate, Submit and Cancel. So a pad's moves reach UI Toolkit as navigation events
  (checked: the manifest, the project settings, the asset, and the manual's "Runtime event system").
- The project's own `Kapman.Game.Json` (`Core/Data/Json.cs`) reads and writes dictionaries and lists. `SaveGame`
  uses it. The frame file should too.

Checked in the 6000.6.3f1 XML reference: `PointerManipulator`; `PointerCaptureHelper.CapturePointer`,
`ReleasePointer`, `HasPointerCapture`; `PointerCaptureOutEvent`; `VisualElement.BringToFront` ("Brings this element
to the end of its parent children list"), `PlaceInFront`, `PlaceBehind`; `UsageHints.DynamicTransform`;
`IStyle.translate`, `IStyle.cursor`; `IPanel.Pick`; `RuntimePanelUtils.ScreenToPanel`; `Painter2D`,
`generateVisualContent`, `MarkDirtyRepaint`; `NavigationMoveEvent` with `direction` (`Next` and `Previous` among
them), `NavigationSubmitEvent`, `NavigationCancelEvent`; `TrickleDown.TrickleDown`; `VisualElement.schedule`,
`SetEnabled`; `TwoPaneSplitView`; `TabView.reorderable`, `Tab.closeable`.

### 5.2 Why not the ready-made controls

- **`TwoPaneSplitView`**: "Requires exactly two child elements to operate", and "One pane is fixed-size while the
  other pane has flex-grow style set to 1" (checked). Three rows would need nesting, and a pane fixed in points
  does not keep its share when the screen changes. An own grip is about 60 lines.
- **`TabView`**: tabs can be re-ordered and closed (checked), but a tab cannot be dragged out to another stack or
  to a split target (?). Here the title row is the tab strip. An own strip is about 80 lines.
- **`viewDataKey`** keeps scroll and such in the editor. At run time, save by hand (?).

### 5.3 The parts

```
UI/Frame/
  FrameRoot.cs       the document's tree: layers, places, ChartSafeRect, the scale and the width class
  FrameLayout.cs     PLAIN C#: Workspace, Node, every change as a method, undo, limits, the filter, Json
  FramePanels.cs     the list of panels: id, title, home, the flag that says it has arrived, how to make it
  TileArea.cs        builds the column from the tree and puts each panel's kept element in its stack
  TitleBar.cs        the name and its picker, the arrows, the tabs, the close button
  TitleDrag.cs       a PointerManipulator: threshold, ghost, drop targets, cancel
  Grip.cs            a PointerManipulator: shares, smallest sizes, snaps
  WorkspaceBar.cs    the workspace buttons and their counts, Rows, Columns, Tabs, "+", undo, the menu
  FrameFocus.cs      the one focus, a stack or a float; the chart's lens reads it
  FrameKeys.cs       the one place where keys are read and sent on
  Floats/FloatLayer.cs    Open, Close, Front; berths and steps; Holding
  Floats/CardQueue.cs     PLAIN C#: the order, the gap, dropping, the safe choice
  Floats/CardWindow.cs    the card in UI Toolkit, over the director that EncounterPanel reads today
  Floats/TalkWindow.cs, ButtonWindow.cs, NoticeStrip.cs
  Floats/Tether.cs        one element over the chart: the rings, the line, the edge arrow
  Tell.cs            PLAIN C#: the router (ask, warn, tell, count, log), the counts in seconds, the classes
```

The layers are children of the root in this order, the later in front: `chart-hit`; `overlay` (the tether, with
`pickingMode` set to Ignore); `frame`; `talks`; `card`; `button-window`; `notices`; `drag`. The order between kinds
is the order of these parents. `BringToFront()` only orders talks among talks.

### 5.4 The tree

```csharp
public sealed class Node
{
    public bool split;                 // false: a stack
    public bool rows;                  // a split: children one above the other
    public List<Node> kids;            // a split: two or three
    public List<float> share;          // a split: adds to 1
    public List<string> panels;        // a stack: panel ids, as tabs
    public int showing;                // a stack: which tab shows
}

public sealed class Workspace { public string name; public float width; public bool folded; public Node root; }

public enum Drop { Tab, Left, Right, Above, Below }

public sealed class FrameLayout
{
    public int version, current;
    public List<Workspace> spaces;

    public bool CanDrop(int space, Node onto, Drop where, int mostStacks, Vector2 size);
    public bool Move(string panel, int space, Node onto, Drop where);
    public bool SendTo(string panel, int fromSpace, int toSpace);
    public bool Close(string panel, int space);
    public bool Open(string panel, int space, Node into);
    public void SetShare(Node split, int grip, float moved);       // keeps the smallest sizes, snaps
    public void Arrange(int space, int how);                       // rows, columns, tabs
    public bool Undo();
    public Node Shown(int space, System.Func<string, bool> arrived);   // what TileArea draws
    public Dictionary<string, object> ToJson();
    public static FrameLayout FromJson(object read, FrameLayout asItCame);
}
```

After every change the tree is tidied, as i3 tidies its own: a split with one child becomes that child; an empty
stack goes; shares are made to add to 1. `FrameLayout` knows nothing of `VisualElement`, so all of it is tested in
EditMode with no scene.

### 5.5 Drawing the tiles

- A split is a `VisualElement` with `flexDirection` Row or Column. Each child has `flexGrow` = its share x 1000,
  `flexBasis` 0, `minWidth` 280 and `minHeight` 132. Layout does the rest. No positions are set by hand.
- Between two children sits a `Grip`, 10 points, with its mark in the middle and a resize `cursor` from USS.
- A stack is a title row of 28 and a body. The body holds the panel's own root element. A panel is made once and
  kept in `FramePanels`. A rebuild moves it to its new parent, so its scroll and its state live on.
- After each change the column's elements are rebuilt from `Shown(...)`. Four stacks: there is nothing to measure.
- A panel arriving: `Tell` raises the notice and the stack wears the USS class `arrived` for 1.2 seconds (two gold
  beats, by a USS transition).

### 5.6 Dragging a title

```csharp
sealed class TitleDrag : PointerManipulator
{
    const float Threshold = 8f;
    Vector2 down; bool pressed, dragging;

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<PointerDownEvent>(e => { if (e.button == 0 && !Locked) { pressed = true; down = e.position; } });
        target.RegisterCallback<PointerMoveEvent>(OnMove);
        target.RegisterCallback<PointerUpEvent>(OnUp);
        target.RegisterCallback<PointerCaptureOutEvent>(_ => Cancel());
    }

    void OnMove(PointerMoveEvent e)
    {
        if (!pressed) return;
        if (!dragging)
        {
            if (((Vector2)e.position - down).magnitude < Threshold) return;
            dragging = true;
            target.CapturePointer(e.pointerId);         // only now: a plain click on the name still opens the picker
            Ghost.Show(title);                          // in the drag layer; usageHints = DynamicTransform
        }
        Ghost.MoveTo(e.position);                       // style.translate, no layout
        var stack = TileArea.StackAt(e.position);       // each stack's worldBound.Contains, as ChestPanel's chips do
        Preview.Show(stack, DropFor(stack, e.position));// one gold element; nothing if CanDrop says no
    }
}
```

- The target is found from where the pointer is in the stack's rectangle: the middle 40% each way is Tab; outside
  it, the nearest side.
- A workspace button is a target too. `schedule.Execute(...).StartingIn(600)` turns to that workspace while the
  window is held over it.
- `PointerCaptureOutEvent` must cancel. `unity_ui.md` notes that the chest's drag lacks it and can leave its ghost
  on screen.
- Esc: `FrameKeys` asks `TitleDrag` first.

### 5.7 The grip

The pointer's movement in points, over the split's length, is the share moved from one neighbour to the other. It
stops where either would go under its smallest size. It snaps within 12 points of a quarter, a third, a half, two
thirds and three quarters. A double click gives equal shares. The column's right edge is the same manipulator on the
workspace's `width`.

### 5.8 The floats

- A float is a `VisualElement` with `position: absolute`. `left` and `top` are set when it opens and when a drag
  ends. During a drag it moves by `style.translate` with `usageHints = UsageHints.DynamicTransform`, so no layout
  runs each frame (`unity_ui.md`, point 4 of its short answer).
- **Front and focus.** `RegisterCallback<PointerDownEvent>(..., TrickleDown.TrickleDown)` on the float's root:
  `BringToFront(); FrameFocus.Set(this);`. Trickle down, so a press on any child fronts the window first.
- **The hold.** `FloatLayer.Holding` is true with a card open or waiting, a talk open, or a button's window open.
  `PanelClock.PanelOpen` becomes `FloatLayer.Holding`: the tiles are not in it. Each holds under its own name
  (`GameSpeed.Hold("card")`, `"talk"`, `"button"`) so the lamp can say which.
- **`CardWindow` replaces the IMGUI card.** It makes the calls `EncounterPanel` makes now: `Choose(i)`,
  `ChooseDefault()`, `Continue()`, `Dismiss()`, and it reads `Chosen` and `DefaultIndex`. `EncounterPanel.Covers`
  and the taking back of the ship's click go: a float takes its own pointer events.
- **The half second.** `SetEnabled(false)` on the choices, then
  `schedule.Execute(() => choices.SetEnabled(true)).StartingIn(500)`, with the fade as a USS transition. Check
  once that the scheduler and USS transitions run while `Time.timeScale` is 0 (?): the card holds the clock, and it
  must still fade in.
- **The tether.** One element over the chart with `generateVisualContent` and `Painter2D`: the rings, one line, one
  arrow. `MarkDirtyRepaint()` only when the float in focus moves, the chart moves or the matter moves. The matter's
  point comes from the chart's own sum in `MapView`.
- **The ghost number.** A choice already carries its effects in the cards' verbs (`silver`, `goods`, and the
  missing `hours` and `days`). On `PointerEnterEvent` of a choice: `Preview.Show(effects)`. A panel that shows one
  of those numbers listens and draws "then N" in gold. On leaving: `Preview.Clear()`.
- **Notices.** `Notices` becomes `NoticeStrip`: add the matter, the click, the right click, the pointer stopping
  the count, the limit of two in 20 seconds, and a line to the log for every one. The router is the `NoticeBus`
  that `onboarding_flow.md` P6 and step 4 of `wiring_plan.md` already name; here it is `Tell`, reading one data
  file of routes, a row for each kind of event.

### 5.9 Keys and the pad

`FrameKeys.Tick()` runs once a frame, in this order: a drag in hand (Esc drops it); typing (`UiTyping.Active`:
nothing else is read); Alt held (the window keys); the float in focus (1 to 9, Enter, Space, Esc); the bare game
keys (Space to hold, Enter to cast off, D, F, 1 to 6 for workspaces). The polls in the other files move here one by
one.

Tab is UI Toolkit's own key for the next focusable thing. Catch `NavigationMoveEvent` at the root when its
`direction` is `Next` or `Previous`, and turn it into the next or the last window.

For a pad, make every row and button `focusable`, give `:focus` the gold edge in USS, and when a window takes the
focus call `Focus()` on its first row. The left trigger (window keys), the right trigger (the wheel of eight verbs)
and the two bumpers are four actions to add to the asset's UI map.

### 5.10 Tests

- `FrameLayout`: every change keeps the limits; shares add to 1; a move and an undo give the same Json; a fifth
  stack is refused; the filter hides what has not arrived; narrow is made from standard; an unknown panel in the
  file is skipped; a broken file gives the frame as it came.
- `CardQueue`: danger before a sign before a story card; the gap of an hour; a fourth card drops the lowest with
  its safe choice and a log line; a sign whose matter has ended is dropped.
- `Tell`: seven "tell" events in 20 seconds show two and log seven; the same line twice in an hour is one with a
  count; a class set to "log only" raises nothing; a class that is "ask only" cannot be lowered.
- Berths: in the standard and wide classes a card never covers its matter, wherever in the free chart that is.
- In `ChartGameTests`: with a card open the clock is held and a Buy is refused; at sea with only tiles open the
  clock runs.

### 5.11 The order of work

| Night | Work | Needed for the playable hour |
|---|---|---|
| 1 | `FrameLayout` and its tests; "as it came" for the four classes | Yes |
| 2 | `FrameRoot`'s layers, `TileArea`, the title with its picker and arrows, the bar: all the mock does, by clicks | Yes |
| 3 | `FloatLayer`, `CardWindow` off IMGUI, `CardQueue`, the lamp, `PanelClock` changed | Yes |
| 4 | `Tell`, the log, counts, `NoticeStrip` | Yes |
| 5 | `TalkWindow`, `ButtonWindow`, berths and steps | Yes |
| 6 | `FrameKeys` taking the polls; the `Grip` | The keys, yes. The grip can wait |
| 7 | `TitleDrag`, the drop targets, undo, the lock | No |
| 8 | The tether, the tags, the ghost number | The ghost number, yes: it is the teeth you can see |
| 9 | The width classes, the frame file, Size | Narrow before any hand-held. Else no |
| 10 | The pad | No |

## 6. Where this parts from the pages before it

- **The clock.** The hand's rule 6 and the book's law 5 say the clock runs only with Voyage in focus, and the
  4 October decision holds it while a panel is open. With windows always open and no Sail on, that would stop the
  clock for good. Here the tiles never hold it.
- **The rack.** The book's rack of seven numbered panel buttons with an Event lamp is gone by the 7 October ruling.
  The lamp moves to the clock. Event is not a panel.
- **Rank 3.** `onboarding_flow.md` P6 puts "a slip beside her mark". It is kept as the warning. It sits on the
  notice strip with a ring on its matter first, and can be pinned beside the matter later (`unity_ui.md` 6.6).
- **What may hold.** `old_school_huds.md` P4 lets only a card stop the clock. Here a talk and a button's window do
  too. Both are opened by the player and both close on Esc.
- **The mock.** Floats 42% wide at the centre become 520 in a berth. Bare R, C, T, N and X go behind Alt. Arrows
  alone walk rows, so window focus by arrows is Alt+arrows.

## 7. Yours to decide

1. **Do the tiled windows hold the clock at sea?** I say no. Only a card, a talk and a button's window hold it. In
   port the clock is held until Wait or Cast off, as the book says, so the market and the talks cost nothing there.
2. **Does a talk hold the clock at sea?** I say yes: a hailed boat does not sail off in the middle of a sentence.
3. **May a panel float over the chart?** I say not in the first release. It hides the chart and is one more thing to
   learn. If you want it later: drag a title onto the chart, two at most.
4. **Windows follow the ship**: on at the start? I say yes, with the switch in Options.
5. **Keys for the eight verbs.** The rack is four by two. Q W E R over A S D F would put each button under the
   finger in the same place. That moves Measure off D and Follow her off F. Or keep Enter and D and leave six
   verbs to the mouse.
6. **The Chest in the first evening?** The ruling asks for reading on the leg. Then the first evening must give one
   text, and the Chest arrives in stage 1.
7. **A very wide screen**: the extra to the chart, as I have it, or a second column of windows at the right?

## What I could not check

Marked (?) above: how EU4's message settings are reached and what its steps are; CK3's toasts, feed and issues, and
whether it has message settings of its own (not in the notes of patches 1.5, 1.7 or 1.9); RimWorld's right click, its alerts and its pause on a threat; EVE's window
rules and what players say of them; Football Manager's Continue rule; Uplink's windows; Factorio's one-window rule;
what players say of Dwarf Fortress's pauses; whether a `TabView` tab can be dragged to another view; whether
`viewDataKey` keeps anything at run time; whether the scheduler and USS transitions run at time scale 0. The seven
holds a crossing, the gap of an hour, the 0.5 seconds, the 8 points and the sizes are my first guesses (g) for
play to tune. No part of this was run.

## Sources

Fetched 7 October 2026:

- CK2 wiki, Message settings: https://ck2.paradoxwikis.com/Message_settings
- CK3 wiki, Patch 1.5: https://ck3.paradoxwikis.com/Patch_1.5
- EU4 wiki, Alerts: https://eu4.paradoxwikis.com/Alerts
- Stellaris wiki, Main interface: https://stellaris.paradoxwikis.com/Main_interface
- Dwarf Fortress wiki, Announcement and Announcements.txt: https://dwarffortresswiki.org/index.php/Announcement ,
  https://dwarffortresswiki.org/index.php/Announcements.txt
- RimWorld wiki, Events: https://rimworldwiki.com/wiki/Events
- Wikipedia, Football Manager 26: https://en.wikipedia.org/wiki/Football_Manager_26
- i3 user's guide: https://i3wm.org/docs/userguide.html
- sway(5): https://man.archlinux.org/man/sway.5.en
- dwm tutorial: https://dwm.suckless.org/tutorial/
- Dear ImGui wiki, Docking: https://github.com/ocornut/imgui/wiki/Docking
- Unity 6 manual, TwoPaneSplitView; Runtime event system; FAQ for events and input:
  https://docs.unity3d.com/6000.0/Documentation/Manual/UIE-uxml-element-TwoPaneSplitView.html ,
  https://docs.unity3d.com/6000.0/Documentation/Manual/UIE-Runtime-Event-System.html ,
  https://docs.unity3d.com/6000.0/Documentation/Manual/UIE-faq-event-and-input-system.html
- On this machine: `C:/Program Files/Unity/Hub/Editor/6000.6.3f1/Editor/Data/Managed/UnityEngine/UnityEngine.UIElementsModule.xml`;
  `Packages/manifest.json`; `Assets/InputSystem_Actions.inputactions`; the code named in 5.1.

Asked for and not found (404, or the page had nothing on it): the EU4 and CK3 wiki pages on message settings and
the interface; the RimWorld wiki's Letters, Alerts and Options; the EVE University pages on windows; Factorio's
Friday Facts 277 (it is about the process, not the rules); Wikipedia on Uplink's interface.
