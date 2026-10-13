# KAPMAN: the project built the usual way

The user, 7 Oct 2026: "prepare to turn the entire project into a new project called all caps KAPMAN, and make it like
people usually make it, not just all in script, build it physically and everywhere you can make it visual scripting."

This is the plan for that, to be read and ruled on before anything is moved. Nothing is built yet.

## Where the project stands today (counted 7 Oct 2026)

- About 72,000 lines of C# in 295 files. Most of the game is made by code as it starts.
- The screen is built wholly in code: 236 places make a window, a label or a button; there is not one UI file that can
  be opened and moved in the editor.
- 53 places make a game object in code as the game runs, and 9 parts start themselves when a scene loads (the opening,
  the music, the notes, the port's picture, the ship's 3D view and others): none of them is in a scene to be seen.
- The scenes are built by editor tools from data, not laid out by hand.
- Visual scripting is already in use: 12 graphs run the ports, the market, the meetings at sea, events and the world's
  step, with 12 files of nodes of our own.
- 203 data files (ports, goods, cards, people, texts), 113 prefabs, 421 edit-mode tests and 25 play tests.

## What "the usual way" means here

1. **A new project, KAPMAN**, in its own folder (G:\KAPMAN: see Ruled, 4), Unity 6000.6.3f1 with HDRP as now, holding
   only what the game uses: no demo scenes of bought packs, no old levels, no dead code. Kapman2 stays as it is until
   KAPMAN passes the same tests.
2. **Scenes you can open and see.** Three: Boot (the loader), Title (the sea and the name), Game (the chart, the
   frame, the ship). Everything the game has stands in the Hierarchy as an object with its settings in the Inspector:
   cameras, lights, the sea, the ship, the screen's documents, the sound sources, and one Systems object for the parts
   that today start themselves unseen.
3. **A screen you can open and move.** Every window, pop-up, the menu, the opening's balloon and the frame itself
   become UI files (UXML, edited in Unity's UI Builder) with the stylesheets we have. Code only fills in the words and
   numbers; it no longer builds the screen.
4. **Prefabs for what repeats**: the ship, the jetty, a crewman, a port's mark, a pop-up window, a row of a list, the
   exclamation mark, the rain.
5. **Settings as assets**: what one tunes by hand (the clock's speeds, the music's cues, the title's weathers, the
   opening's lines, the first jobs' costs) becomes assets with fields in the Inspector. The big tables stay data files
   (they are made and checked by scripts, and the tests read them).
6. **Visual scripting for everything that is a flow**, and Timeline for what is timed:
   the slow start, the title, the opening's talk, the showroom, the first morning, the first jobs, coming in and
   leaving a port, meetings, events, the music's cues, the cuts through black. Each is a graph one can open and read
   from left to right. The node library grows to serve them.
7. **Code stays where it is sums and rules**: the world's traders, prices, the weather, sailing, the calendar, saving,
   reading data, the talking text. Each offers nodes to the graphs. (Rules written as graphs would run slower, could
   not be tested as they are today, and would be far harder to keep right: this is where most projects stop too.)
8. **Animation the usual way** where it serves: an Animator and clips for the rowers and for the screen's own motions
   (the box coming up, the letters going under), in place of code that moves them.

## The order of work

Each step ends with a game that runs, its tests passing, and a commit.

| Step | What | Seen by the user at its end |
|---|---|---|
| 0 | The new project: settings, packages, folders; only the assets the game uses copied in; the code moved as it is | KAPMAN opens and plays as Kapman2 does today |
| 1 | Scenes made physical: Boot, Title, Game laid out with real objects; the nine self-starting parts become components on scene objects | Open a scene and everything is in the Hierarchy |
| 2 | The screen as UI files, in the Windows XP look in Byzantium (the frame all round, thinner sides) | Open any window in UI Builder and move it |
| 3 | Flows as graphs and timelines: the start, the opening, the first jobs; then ports, meetings, events (which are graphs already) tidied onto the same pattern | Open a graph and read the opening left to right |
| 4 | Prefabs and animation: ship, crew, jetty, rain, marks | Drag a prefab into a scene and it works |
| 5 | Settings as assets | Change the title's weather or the music's cues in the Inspector |
| 6 | Clearing out: dead code, a page that says where everything is | A project a newcomer could find their way in |

## What it costs, and the risks

- It is a large piece of work: steps 1 to 3 are most of it. Step 2 alone replaces 236 code-built pieces of screen.
- Until step 0 is done and its tests pass, nothing of Kapman2 is thrown away.
- The play tests click real buttons by their words; they carry over if the windows keep their names. They are the
  guard that nothing was lost in the move.
- Graphs are made by me as files, as the twelve we have were. I cannot look at them as you can in the editor: after
  each one I shall ask you to open it once and say whether it reads well.
- What you have asked for tonight and not yet seen built (the frame in the XP look) would be done once, in step 2,
  not twice.

## Ruled (the user, 7 Oct 2026, night, by the picker)

"Build what you can, start on the new project, move over to everything visual scripting etc., built like an actual
Unity project. I'm happy with the vision we've made so let's move forward."

1. **The XP frame** was built in Kapman2 that evening; its style sheets carry over.
2. **Visual scripting goes as far as the rules themselves** ("Rules as graphs too"): not only the flows (the start,
   the opening, a port visit, the alehouse, meetings, events, the music's cues, the first jobs) but the rules too,
   wherever a graph can hold them: a deal's sums, the toll, hiring and pay, bread and heart, standing, what a card's
   choice does. What stays code, and shows in the graphs as nodes: the world's own clockwork that runs thousands of
   times a step (the other ships and their trade, the routes over the lanes, the weather, the tides), the reading of
   data files, saving, and drawing. Each rule moved gets a test that runs the graph and checks its sums against the
   code it replaces, before the code is taken out.
3. **The screens are UI Builder files** (UXML, with the style sheets as they are): every window, pop-up, the menu,
   the opening's balloon and the frame itself can be opened in UI Builder and moved. Code fills in words and
   numbers and no longer builds the screen.
4. **It lives at G:\KAPMAN** (G:\kapman_unity\KAPMAN is the first build's folder, Kapman, under another case: Windows
   takes them for one). It starts from Kapman2 as it stands when the fixes of 7 Oct are committed. Kapman2 is then
   the archive: nothing more is built there. The history starts fresh; Kapman2's stays where it is.

## What changes in the order of work

Step 3 grows: after the flows, the rules. In this order, each with its test against the code it replaces:
the toll and a deal's sums; hand money and pay; bread, health and heart; standing at a port; what a card's choice
does (the effects are half way there: they are a small language already); the debt and its day; the first jobs.

## Where it stands

| Step | State | Notes |
|---|---|---|
| 0 | DONE, 8 Oct 2026 (c732d16) | G:\KAPMAN made by G:\kapman_unity\compile_check\kapman_uses.py and kapman_make.py from Kapman2 at f45f934: 818 asset files (150 MB) of 4,515 (15.7 GB). |
| 1a | DONE (79dfb43) | The five parts that made themselves when the scene loaded (Showtime, Game music, Play notes, Port view, Ship view) are objects of the Game scene under "--- SYSTEMS ---" (Kapman > Make physical > 1). |
| 1b | HALF DONE (14 Oct 2026, DECISIONS 141) | The scene is Scenes/Kapman.unity (was L_Chart). The ship stage stands in it under SYSTEMS (Kapman > Make physical > 3): the boat's model as a prefab instance, Sun, Moon, the sky and air's volume with its profile asset, the camera and rain, HDRP's sea; ShipStage takes them up at play and makes only her fit-out, the jetty, the wake and the lines. The 50 ports are objects "Port id" under "--- PORTS (the chart's) ---" (Kapman > Make physical > 4, DECISIONS 142), taken as saved at play. STILL TO DO: Boot, Title and Game as three scenes. |
| 2a | DONE (40f6411) | Game/UI/Frame.uxml: the whole frame, the Source Asset of the Frame's UI Document. FrameRoot finds its parts by name. Buttons of the rack can be moved, taken out or added in UI Builder. |
| 2b | DONE (95c038a) | Game/UI/Parts: Window, Float (what is said, pop-ups), Option, Line, LineButton, LogLine. Named in Resources/FrameParts. |
| 2c | DONE (51e907b) | Game/UI/MainMenu.uxml (the menu) and Showtime.uxml (the opening's layers), with Parts/MenuOption, ShowChoice, ShowFace, ShowName. |
| 2d | BEGUN (14 Oct 2026, DECISIONS 143 to 146) | DONE: the chart (Game/UI/MapView.uxml: its layers, the card, the foot, the chronicle, the ship's and port's cards; Kapman > Make physical > 2d); the market (Game/UI/MarketPanel.uxml and its row Parts/MarketRow.uxml; 2e); the old text panel and the notices (TextPanel.uxml + Parts/TextOption.uxml, Notices.uxml + Parts/Notice.uxml; 2f); the port's picture (parts of Frame.uxml); the chest (in MapView.uxml). TO DO: the notes of F2, the places, the ship view's parts into Frame.uxml. Then each side window's own layout (22 panels: today each fills a body with lines; a file per window would let its sections be moved). |
| 3a, 3b | DONE (bb0acf5, 09a0514) | Rules held by graphs: Game/Graphs/Rules (the toll on a sale; the crew's pay, health and heart), named in Resources/GameRules, asked through GameRules, run by Kapman.Graphs.RuleGraphs where the asset lies. Each keeps its sum in code for the world's other ships and as the fallback; RuleGraphTests holds graph and code together. To add a rule: a start node (RuleNodes.cs), a graph (Editor/Game/RuleGraphsMade.cs), the asking (GameRules.cs), a test. |
| 3c | DONE (96b3e8e) | The opening is a flow in a graph on the Showtime object ("Opening - the lender, the boat, the first morning"; nodes in OpeningNodes.cs). |
| 3d | to do | More rules (hand money and the sail's cost; standing at a port; what a card's choice does; the debt and its day), and more flows (the slow start and the title; coming in and leaving are graphs already; the alehouse and the other places; the music's cues). |
| 4 to 6 | to do | Prefabs and animation; settings as assets; clearing out (the old level builders in Assets/Kapman/Editor, the tests of the old 3D levels). |

The rule for the screen: a whole screen is the Source Asset of its UI Document; a part the code takes copies of is a
file of Game/UI/Parts, named in Resources/FrameParts. A part is found by its NAME: it may be moved and restyled freely
in UI Builder, but a renamed or deleted part is reported in the Console and replaced by a plain stand-in.
Beware: UI Builder drops the comments of a style sheet when it saves one. Moving a part in the canvas writes to the
UXML, which is safe; changing a selector of Frame.uss in UI Builder would strip the notes in that file.

Kapman2 (G:\kapman_unity\Kapman2) is the archive from f45f934 on: nothing more is built there.
