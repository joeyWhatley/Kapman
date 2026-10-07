# UI research: why an interface looks machine-made, and the cure

6 October 2026. Written after the user said of the first mockup: "its ok, just pretty big and bulk and not very
intuitive, reads very much like made by ai".

Three parts: what the tells are and why they happen (1 and 2), what five hand-made games do instead (3), and a
checklist with both mockups marked against it (4 to 6). Section 7 says what a third pass should do.

## How to read this

- **(?)** marks what I could not check tonight. Web search was used up for this session before I began, so I could
  only fetch pages I could name. Nine pages answered (see Sources). The Game UI Database refused (403), and I could
  not reach a Persona 5 UI essay, anything on the Hades interface, Lucas Pope on the Obra Dinn book, or the Pentiment
  GDC talk. What I say of those is from knowing the games, and is marked.
- The numbers about the mockups are counted from their own files (`ui_mock/template.html`, `template2.html`), not
  judged by eye. Screen shares are worked out from the CSS sizes at 16:9; where a height depends on the text inside,
  the share is an estimate and says "about".
- I made both mockups being marked here. The marking is as hard as I can make it for that reason.

---

## 1. The short answer

An interface looks machine-made when **every part of it was decided by the same rule**. One border for everything.
One label style for everything. One kind of box, repeated. Each thing the game knows gets a tab, each tab gets the
same number of sub-tabs, and the whole is then explained in a list of principles underneath.

A person does not work like that. A person has **one idea** ("it is a logbook", "it is a hologram on his back", "it
is a ransom note") and bends everything to it, and then spends care unevenly: a great deal on the three things the
player touches every minute, almost none on the rest. The result is lopsided, and the lopsidedness is what reads as
a hand.

So the cure is not a nicer border. It is: pick the one object this interface is, throw out what does not belong on
that object, and make the few things left differ from each other the way real things do.

---

## 2. Why the machine look happens

**It is the average.** Anthropic's own note on this, written about models making web pages, says a model "tends to
converge toward generic, 'on distribution' outputs", and names the defaults: "Overused font families (Inter, Roboto,
Arial, system fonts)", "Clichéd color schemes (particularly purple gradients on white backgrounds)", "Predictable
layouts and component patterns", "Cookie-cutter design that lacks context-specific character". Its cures are to
decide typography, colour, motion and backgrounds one by one, to name a real-world source to draw from, and to forbid
the defaults by name. (Anthropic cookbook, "Prompting for frontend aesthetics".)

Note what that list contains: **purple**. Both Kapman mockups are purple and gold on near-black. The purple came
from a real decision (the Byzantium theme, the palette in the design docs), so it is not there by default. But a
player does not know that. A dark screen with purple edges and gold figures is one of the commonest looks of a
generated interface, and the first mockup wears it over a layout that is also the commonest one. Section 7 comes
back to this.

**It is fair to everything.** A machine laying out a game gives each system the same weight, because nothing told it
which matter. Ship, Crew, Ledger, Chest, Chronicle: five tabs, each with two to four sub-tabs. That is an inventory
of the design document, not a screen. A designer who has played the game knows the player looks at silver and the
wind a hundred times for each look at the Chronicle, and builds accordingly.

**It is a web page.** The patterns nearest to hand are those of web applications: a top bar, a left rail of icons, a
side drawer, cards, chips, tags, segmented buttons, a close cross in the corner. None of them is a game's. Games
that feel made (below) have almost none of them.

**It explains itself.** Under the first mockup is a list headed "The rules that make it easy", eight of them, each
with a bold title. An interface that is easy does not need the list. The list is the tell: it is the designer's
reasoning left on the page, where a person would have put it in a notebook.

---

## 3. What the made ones do

Each of these is one idea carried all the way through. That is the whole lesson; the rest is detail.

### Persona 5 (Atlus, 2016): the idea is a pose

Checked: the art director was Masayoshi Suto, "whose most notable work on earlier games included the user interface
(UI) displays". Red "was chosen to evoke a harsh feeling". The director Hashino "wanted to demonstrate how a shift
in perspective could alter a dull life into an exciting one". (Wikipedia, Persona 5.)

From knowing the game (?): three colours only, red, black and white. Nothing is level: every panel and line of type
is tilted a few degrees. Letters are cut out like a ransom note, each a different size. Each menu is a different
drawing of the hero in a different pose, and the menu's lines hang off his body. Moving between menus is a hard cut
with a shape wiping across.

**What it teaches.** It is as loud and as large as an interface gets, and nobody calls it bulky. Size is not the
fault. Sameness is. Persona 5 has no two screens laid out alike. And it obeys a limit harder than any grid: three
colours, no exceptions.

### Dead Space (Visceral, 2008): the idea is that the man can see it too

Checked: health and energy are shown on the back of the suit, the ammunition count appears when a weapon is raised,
and maps, inventory and calls are projected in the world. "Immersion was a core element of the game"; the team found
"the setting was better realised and conveyed as a side effect of this approach". (Wikipedia, Dead Space.) Dino
Ignacio's GDC 2013 talk, "Crafting Destruction: The Evolution of the Dead Space User Interface", covers the "early
attempts and concepts" behind it (GDC Vault; I read the description, not the talk).

Marcus Andrews of DICE gave the words for this in 2010: **diegetic** ("included in the game world, i.e., it can be
seen and heard by the game characters"), non-diegetic, spatial, and meta. His warning matters as much as the words:
"The rule of functionality preservation and translation must be met." Far Cry 2 put its map and watch in the hero's
hands and still needed a plain HUD beside them, and the two fought. (Game Developer, "Game UI Discoveries: What
Players Want". The four terms come from Fagerholt and Lorentzon's 2009 thesis "Beyond the HUD" (?).)

**What it teaches.** Put each fact on the thing it is a fact about. Health on the body. Shots on the gun. And do it
only where nothing is lost by it.

### Return of the Obra Dinn (Lucas Pope, 2018): the idea is one book

Checked: the look is "'1-bit' monochromatic", "inspired by games on early Macintosh computers". The player "is given
a logbook that includes a drawing of all the crew members, the crew roster, and a plan of the ship". Fates are
"validated only in sets of three". (Wikipedia, Return of the Obra Dinn.)

From knowing the game (?): there is no HUD at all while walking the ship. Every menu in the game is a page of that
one book: the map is a page, the crew list is a page, the answer form is a sentence on a page with blanks to fill.
A face you cannot yet name is blurred in the drawing and clears when you have seen enough. Pope has written that the
book took far longer than he planned (?).

**What it teaches.** One object can hold everything, if you are willing to make everything fit its pages. And two
colours are enough. The limit is the style.

### Pentiment (Obsidian, 2022): the idea is that someone is writing this

Checked: the art "is a mix of late medieval manuscripts, early print, and woodcuts"; the team "looked at the
Nuremberg Chronicle, as well as illuminated medieval manuscripts in the Getty Museum and Huntington Library".
(Wikipedia, Pentiment.) Lettermatic drew five type families for it: a scribe's hand, a humanist hand, a monastic
blackletter, a printer's type and a peasant's hand, so that the writing is "reflective of their level of education,
and their standing". Each has several drawings of each letter, because "Humans effectively cannot write the same
letter twice". The aim was "the appearance of gently bleeding ink", and the designer wrote of "Freeing myself from a
goal of perfection". (Lettermatic, Pentiment case study.)

From knowing the game (?): speech is written out stroke by stroke as you watch. The ink goes down wet and dries
paler. The writer now and then makes a slip, scratches it and writes it again. A printer's words are stamped down a
block at a time. When the hero learns that a man is better schooled than he thought, the man's hand changes on the
page. Hard words are underlined in red and the view pulls back to show the note in the margin of the book.

**What it teaches.** The hand is in the *making* of the mark, not in a texture laid behind it. Parchment-coloured
rectangles are not a manuscript. Uneven letters, written in time, are.

### Hades (Supergiant, 2020): the idea is one artist's line

Checked: only that the art is "primarily by in-house artist Jen Zee". (Wikipedia, Hades.) I found nothing tonight on
its interface.

From knowing the game (?): the frames, the icons and the portraits are drawn in the same heavy black line as the
world, so the interface looks cut from the same sheet. Each god has a colour and a sign, and a gift from that god
carries both, so you know whose it is before you read a word. The health bar and the coin count are small and sit
in corners. The panel for choosing a gift is large, because that is the choice that matters. Weight follows
importance.

**What it teaches.** Big where the decision is, tiny everywhere else. And icons drawn by the hand that drew the
world, not taken from a set.

### The two libraries

**interfaceingame.com** (checked): about 250 games and over 20,000 screens, sorted by game, by genre and theme
(Medieval is one), and by the kind of screen (menu, inventory, dialogue, HUD). It has interviews with working UI
people, among them David Sheldon-Hicks of Territory Studio and several from Ubisoft. **Game UI Database**
(gameuidatabase.com) would not answer tonight; from knowing it (?), it is larger and sorts screens by game, by
screen type, and by material and style, so one can pull every paper, wood, diegetic or pixel interface side by
side. It is the right place to spend ten minutes before the third pass: filter by Medieval and by map screen, and
look at what the hand-made ones leave out.

---

## 4. The checklist: sixteen tells and their cures

| # | The tell | Why it reads as a machine | The cure |
|---|---|---|---|
| 1 | **Every box is the same box.** One border, one fill, one shadow on every panel. | Nothing was looked at twice. | Each thing has the edge of what it is: a stick is a stick, a board is cut. Or have no boxes: type on the map. |
| 2 | **Tabs, and tabs inside tabs.** | It is the design document's table of contents. | Click the thing itself. One layer deep. What will not fit is cut or moved to winter. |
| 3 | **Even counts.** Five tabs, five layers, three sub-tabs each, three choices each. | Real needs do not come in fives. | Let one thing have seven lines and the next have one. |
| 4 | **Small upper-case labels with wide spacing** over every number ("SILVER", "OWED", "STORES"). | The dashboard habit. | A drawn sign, or the unit, or nothing: "86 d" beside a coin needs no word. |
| 5 | **Coloured pills and tags** for state (green good, gold fair, red bad). | Web application status badges. | A word in the sentence, or a change in the drawing itself (a paler ink, a broken line). |
| 6 | **A frame on all four sides**: bar on top, bar below, rail left, panel right. | The world becomes a window in a program. | At most one edge is owned. The rest of the interface sits on the map as loose things. |
| 7 | **Everything always on.** Six figures in the bottom bar whether they matter or not. | No one decided what matters now. | Show what changed or is running short. The rest on asking. |
| 8 | **Glows, veils, gradients, double borders, see-through dark panels.** | Depth by effect instead of by drawing. | Flat. One hard line. If there is shade, it is drawn. |
| 9 | **Line icons from one set**, the same stroke, in a 24-unit square. | The same icons are in every web app. | Draw them, in pixels, by the hand that draws the map. Until then, a word is better than a stock icon. |
| 10 | **Text that explains the interface.** Tooltips that say what a click does; a line that says what the chart now shows; a list of rules underneath. | The reasoning was left on the page. | Delete it. If it is unclear without the words, the layout is wrong. |
| 11 | **Modern controls showing through**: a cross to close, pause and play triangles, a letter in a square for a face, chips, segmented buttons. | Each one says "program". | Close by clicking away. Time is a thing in the world you take hold of. A face is drawn or absent. |
| 12 | **Modern measures**: m/s, kg, "410 / 660", a calendar date in figures. | No one in 793 said it, and BRIEF rule 2 says so. | "A fresh wind from the west." "Nearly laden." "Midsummer eve." Figures only where a merchant counted: silver, lots, days. |
| 13 | **Perfect alignment and even spacing everywhere.** | A hand drifts; a grid does not. | Set loose things a little off square. Vary the gaps. Not randomly: the way things lie when put down. |
| 14 | **The safe palette**: dark ground, purple edge, gold figures. | A very common generated look (section 2). | Fewer colours used harder, or a ground that is a material. See section 7. |
| 15 | **No one idea.** Asked "what object is this?", the answer is "a HUD". | That is the root of all the others. | Name the object in three words and cut what does not belong on it. |
| 16 | **Care spread evenly.** The Chronicle tab is as finished as the purse. | A person runs out of time and spends it where it counts. | Three things made well. The rest plain, small, or not there. |

Marks used below: **yes** = the mockup has the tell; **part**; **no** = clear of it.

---

## 5. The first mockup, marked (`template.html`)

What it is: a top bar (date, day stick, speed), a bottom bar (six figures), a left rail of five icon tabs opening a
drawer with sub-tabs, a right panel for the place with five sub-tabs, a row of five map layers with chips and a
legend, and a card bottom centre. Below the screen, a table of "five depths" and eight rules.

**Counted from the file:**

- Doors to information: 5 rail tabs holding 15 sub-tabs, 5 place sub-tabs, 5 layers holding 15 chips. **Forty-five
  things to click that are not a port.**
- Screen covered: the two bars take 11.5% of the height between them. With a port open (the state it starts in)
  about **38%** of the screen is interface. With a drawer open as well, about **61%**, and the chart is a strip 44%
  of the width.
- `text-transform: uppercase` appears 8 times and `letter-spacing` 14 times. Every label on the screen is small
  capitals in the pixel face.
- 16 named colours. One panel style, used for six different things.

| # | Tell | Mark | What is there |
|---|---|---|---|
| 1 | Same box | **yes** | `.panel` is the day bar, the tally, the rail, the drawer, the place and the card. Even port names sit in little bordered boxes. |
| 2 | Tabs in tabs | **yes** | Two full sets, left and right, plus layers with chips: three tab systems on one screen. |
| 3 | Even counts | **yes** | Five tabs, five layers, five place tabs, five "depths", three talks, three goods. |
| 4 | Upper-case labels | **yes** | All of them. "SILVER 86 d", "OWED 480 d", "STORES", "HOLD", "CREW", "WIND". |
| 5 | Pills and tags | **yes** | "MICHAELMAS" in a gold tag, "FAIR FOR THE RIVER" in a green one, price tags on ports in three colours. |
| 6 | Framed on four sides | **yes** | Top, bottom, left always; right as soon as anything is clicked. |
| 7 | Always on | **yes** | Six figures in the tally, four speed buttons, five tabs, five layers, at all times. |
| 8 | Effects | **yes** | A double border made of two shadows, see-through panels, a radial dark veil over the chart. |
| 9 | Stock line icons | **yes** | Five outline icons, stroke 1.6 in a 24 square: the exact form of the common web sets. |
| 10 | Explains itself | **yes** | The lede, the depth labels, the five-row table, the eight rules, a legend line for every layer. |
| 11 | Modern controls | **yes** | "x" to close, "II 1x 2x 4x", chips, a letter in a square for each man, a "1-5 Esc" key hint in the rail. |
| 12 | Modern measures | **yes** | "W 5 m/s", "410 / 660 kg", "21 June 793". (It does also say "Midsummer eve", which is right.) |
| 13 | Perfect alignment | **yes** | Everything is on the grid and flush to an edge. |
| 14 | Safe palette | **yes** | Night, purple border, gold numbers, at full strength. |
| 15 | No one idea | **yes** | It calls itself "the table" but nothing on it is a thing on a table. It is a strategy-game HUD. |
| 16 | Even care | **yes** | The Chronicle's three sub-tabs are as built as the market. |

**Sixteen of sixteen.** The user's three complaints map straight onto it: "big and bulk" is tells 6 and 7 (38% to
61% of the screen), "not very intuitive" is tells 2 and 3 (forty-five doors, and you must learn which of three tab
systems holds a thing), "made by ai" is the rest together.

**What was right in it and should be kept:** the day stick as an idea (dates that matter, cut on a stick); "left is
yours, right is here" as a rule of thumb; the word on the item with who said it and when; the clock held while you
read; her mark never covered. These are decisions about the game, not about the look, and they survive.

---

## 6. The second mockup, marked (`template2.html`)

What it is: the chart is the whole screen. On it: a status slip top left (date, a season bar, what is next, four
figures), a wind sign top right, and three slips that open when called: your boat (a drawn hull with the cargo and
the men in it), the place you clicked (joined to it by a line), and what is said. A large winter council panel once
a year. A switch between a paper look and a dark ink look.

**Counted from the file:** no tabs. No upper-case rule and no letter-spacing rule at all. Standing interface: the
status slip (19% of the width, about a tenth of the height) and the wind sign, **about 3% of the screen**. With boat,
place and talk all open, roughly a quarter. The winter council covers 70% by 85% of the screen, about 60%, by
design.

| # | Tell | Mark | What is there |
|---|---|---|---|
| 1 | Same box | **yes** | Better in idea, the same in fact. `.slip` is one rectangle with one border and one hard drop shadow, used for the date, the boat, the place, the talk and the council. Five identical slips is still one box five times. The "paper" is a flat fill, `#eadfc4`; nothing about it is paper. |
| 2 | Tabs in tabs | **no** | Gone. You click the boat, the place, the cask. The market is one step inside a port, with a way back. |
| 3 | Even counts | **part** | The page still says "five things on the chart", twice. The contents inside are uneven, which is right. |
| 4 | Upper-case labels | **no** | Gone. Figures stand beside small drawn signs (a coin, a loaf, a hull). |
| 5 | Pills and tags | **part** | No tags. But prices on the chart are still green, gold or red figures, and the left edge of each "word" is colour-coded seen or heard. A code the player must learn. |
| 6 | Framed on four sides | **no** | No frame at all. |
| 7 | Always on | **part** | Much less. But the status slip still always shows date, hour, three clock buttons, a season bar, a next-date line and four figures: ten things in a slip that could be three. |
| 8 | Effects | **part** | The veil is gone. Left over: a season bar that is a four-colour gradient (a progress bar by another name), a hard drop shadow on every slip. The dark edge round the chart lettering is honest work: it keeps the names readable. |
| 9 | Stock line icons | **part** | The coin, loaf, hull, stick and chest are small filled drawings made for this, not a set. Right direction. They are smooth vector shapes on a pixel map, though: a third hand. The wind sign is a circle with a stock arrow. |
| 10 | Explains itself | **yes** | Still. The lede ("Smaller, and no tabs. You click the thing itself"), the line under the screen, tooltips ("The clock waits while you look", "What you have. Click for your boat"), and in the boat slip: "The chart now shows where it pays, as far as you know." |
| 11 | Modern controls | **yes** | A cross on every slip. Pause and play triangles in the date slip. A letter in a coloured square for the speaker's face, a letter in a circle for each man. Hearts for the crew: a video-game sign, not a thing from 793. Numbered keys on replies. |
| 12 | Modern measures | **yes** | "W 5 m/s", "SW 7 m/s", "410 / 660". Beside them "fair for the river" and "foul for home", which are exactly right and make the figures unnecessary. |
| 13 | Perfect alignment | **yes** | Every slip is square to the screen and pinned at the same small margins. Things put down on a table do not lie like that. |
| 14 | Safe palette | **part** | The paper look gets away from it: dark ink on cream is not the generated look. The ink look is the first mockup's palette again. The accent on both is still the same purple. |
| 15 | No one idea | **part** | There is half an idea: slips laid on a chart. It is not carried through. A slip does not have a close button, a clock control, or a drop shadow at a fixed offset. And the best thing on the screen, the hull with the cargo in it, is an object sitting inside a slip, when it could be the object. |
| 16 | Even care | **part** | Better: the boat is plainly where the care went. The winter council is a two-column form with headings, the least made thing here. |

**Three clear, eight in part, five still there.** It answers "big and bulk" (3% standing against 38%) and most of
"not very intuitive" (no tabs; click the thing). It does not yet answer "made by ai". It is the same designer with
fewer boxes. What is left is tells 1, 10, 11, 12 and 13: the slips are all alike, the screen still talks about
itself, program controls show through, the units are modern, and everything is squared up.

**What is right in it and should be kept:** the hull drawing as the hold; names lettered on the chart with no boxes;
the line from a slip to the place it speaks of; "fair for the river" and "foul for home"; small ports in italic,
great ones upright; another ship shown solid when seen, hollow when only heard of, dashed where she is guessed to
be now. That last one is the game's own idea (knowledge has an age) drawn as a mark, with no legend. It is the most
hand-made thing in either mockup. More like that.

---

## 7. What a third pass should do

In order of what it buys.

1. **Name the object and obey it.** The honest candidates, from what the game is: (a) *the chart on a board, with
   the skipper's own things lying on it*: a notched stick, a purse, a wax tablet; or (b) *one wax tablet or
   tally-board* that everything is written on. Merchants of the time kept count on sticks and wax; paper had not
   reached northern Europe and parchment was costly and mostly the church's (?: check against the BRIEF's sources
   before it is drawn). This matters: "paper slips" is itself a period slip, and the mockup's switch is named Paper.
   Whichever is chosen, the test for every element is "would this be lying on that board?" A close button would
   not.
2. **Make the three things that are touched every minute, and make them different from each other.** The day stick
   (long, thin, notched, wood). The purse (small, round, a number). The boat (the hull drawing, standing free, no
   slip around it). Three shapes, three materials, none a rectangle with a border. Everything else is lettering on
   the chart or one plain written panel.
3. **Take the words off.** No tooltips that describe clicking, no line saying what the chart now shows, no count of
   how many things are on screen. If the mockup needs a note for the user, it goes in this folder, not on the page.
4. **Say it as 793 says it.** Wind as a word and a direction drawn on the chart beside her. The hold as the drawing,
   full or not, with no kilograms. The day as a feast. Figures for silver, lots and days only.
5. **Kill the program controls.** Close by clicking the chart. Time: take hold of the stick, or one key; no
   triangles. Men: a drawn head or just a name, never an initial in a shape, never hearts.
6. **Break the square.** Loose things lie a degree or two off true and at uneven distances from the edge. One of
   them may overlap the edge of another. Do this by hand, per thing, not with a random number.
7. **The palette, for the user to decide.** The Byzantium purple is a chosen theme and I am not proposing to drop
   it. But as the colour of *every edge and every accent* on a dark ground it reads as a default. Two ways to keep
   it and lose the look: use it for **one thing only** (what is yours: your mark, your house's flag, your ships) and
   let the interface be wood, wax and ink colours; or keep the dark glass decided earlier for the one written panel
   and give the three objects in point 2 their own materials. The sister document (`map_first.md`, section 14, "The
   hand") reaches the same fork from the other side.
8. **Uneven on purpose.** A port with two things to do shows two lines. The winter council, which comes once a year
   and is the big decision, may be large and may be the most drawn screen in the game (Hades: big where the choice
   is). The Chronicle may be a single line of the latest news until someone asks.

**A test to run on any future mockup, in one minute:** cover the map with your hand. If what is left could be the
interface of a different game, it is not finished.

---

## Sources

Fetched and read tonight:

- Anthropic cookbook, "Prompting for frontend aesthetics":
  https://github.com/anthropics/claude-cookbooks/blob/main/coding/prompting_for_frontend_aesthetics.ipynb
- Marcus Andrews (EA DICE), "Game UI Discoveries: What Players Want", Game Developer, 23 February 2010:
  https://www.gamedeveloper.com/design/game-ui-discoveries-what-players-want
- Dino Ignacio, "Crafting Destruction: The Evolution of the Dead Space User Interface", GDC 2013 (description only):
  https://www.gdcvault.com/play/1017723/Crafting-Destruction-The-Evolution-of
- Lettermatic, Pentiment case study: https://lettermatic.com/custom/pentiment
- Interface In Game: https://interfaceingame.com/
- Wikipedia: Persona 5 https://en.wikipedia.org/wiki/Persona_5 ; Pentiment
  https://en.wikipedia.org/wiki/Pentiment_(video_game) ; Dead Space
  https://en.wikipedia.org/wiki/Dead_Space_(2008_video_game) ; Return of the Obra Dinn
  https://en.wikipedia.org/wiki/Return_of_the_Obra_Dinn ; Hades https://en.wikipedia.org/wiki/Hades_(video_game)

Tried and not reached (so not used; what the text says of them is marked (?)):

- Game UI Database, https://www.gameuidatabase.com/ (403).
- Ridwan Khan, "The UI and UX of Persona 5" (no answer).
- Lucas Pope's Obra Dinn devlog on the book (the page I fetched did not cover it).
- Josh Sawyer and Hannah Kennedy's Pentiment talks at GDC 2023 (the address I guessed was another talk).
- Fagerholt and Lorentzon, "Beyond the HUD", Chalmers, 2009 (named from memory, not fetched).
- Web search itself: the session's allowance was already spent.

Kapman's own files read: `Design/ui_mock/template.html`, `Design/ui_mock/template2.html`, `Design/chart_game.md`,
`Design/research/ui/map_first.md` (section 14).
