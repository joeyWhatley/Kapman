# The first hour and the flow of play

6 October 2026. For the thick-frame HUD of `ui_mock/template3.html` (layout E, "map shows through"): top bar, buttons
and panels on the left, portrait top right, ship window bottom right, the log along the foot. **(?)** marks what I
could not check: the web search budget was spent, so some games are from memory. Every number is a first guess. All
spoken lines are placeholders for you to write.

One clash: `ui_direction.md` section 4 bans a message log, a quest list and buttons. The frame you asked for brings
them back. This page follows the frame, and keeps that page's rule that a hover says what a thing is, never what a
click does.

## 1. Findings

| Game | What it does | The rule to take |
|---|---|---|
| Plants vs. Zombies (George Fan, GDC 2012) | Blend teaching into play; doing over reading; "get the player to do it once"; "maximum of eight words on the screen"; tips only for those who struggle; no noise. [1] | One new thing at a time, done once by hand, in eight words. Help only on a stall. |
| Dredge | "Go Out > Get Fish > Sell > Upgrade > Go Out > Get Caught At Night". Time is the limit, not fuel. [2] You start owing for the boat; the first jobs come from named townsfolk (?). | The debt is the tutorial. To-do lines are people asking. |
| Sunless Sea | Teaching "chancy journeys" instead of "sticking to home and grinding is already difficult"; the early game "stayed rawer and grindier than it should have been". [3] | Push the player out of the home river by a set day. Test on new players only. |
| Slay the Spire | Enemy intent shown as icons; testers found "having the numbers available was more engaging". [4] | Show the cost before the choice. |
| Into the Breach | Every enemy move is drawn before it happens; telegraphing "helped to hasten the pace". [5] | A danger is on the chart before it is on a card. |
| Hades | Dialogue moves on with each run, "making each attempt ... meaningful"; a list of small prophecies pays for trying each thing once (?). [6] | A setback gets a new line from a person. The list rewards trying. |
| Stardew Valley | The journal opens with "Introductions", then "Getting Started" (one parsnip, 100 g), then "To The Beach" by letter on day 2. [7] | Three open lines. Each teaches one verb and pays a little. |
| Crusader Kings III | A term inside a tooltip can be hovered; the tooltip locks after a moment so the pointer can enter it (?). | Words explain words, on demand. |
| Civilization | The advisor marks one choice in a list and can be switched off (?). | Advice comes from a named person, on a choice already on screen. |
| NN/g | "Designs that go beyond 2 disclosure levels typically have low usability." [8] | Two levels deep, never three. |

**What they share.** The first goal is a debt or a promise set by a person. The screen starts nearly empty and grows
with use. Costs and dangers are shown before the choice. Words are short, optional and nested. Help waits for a stall.

## 2. Proposals

### P1. The frame grows: buttons are earned by use

**Rule.** A button exists only after the thing it opens has happened once. You asked for buttons "dependant on how
many needed": in the first hour that count grows from 2 to 6.

| Button | Appears when | Usual minute |
|---|---|---|
| Hold, Tasks | at the start | 0 |
| Ledger | the first word on a price is heard | 5 |
| Crew | a man first speaks on a card | 11 |
| Chronicle | the first news is told | 18 |
| Chest | the first text is given (the evening fitt) | 25 |
| World | the first war or shut port touches a course | 45 |
| House | the first winter, or a second boat | later |

**Numbers.** One new button at a time, at least 90 seconds apart. It slides in under the last one with one log line,
and carries a small count until opened. Never a greyed button. Where no button or panel is, the chart shows.

**Build.** `HudButtons`: a list in data (`id`, `when` in `Cond.cs` words, `order`). It sets the flag `hud.<id>`,
saved with the house, so an heir keeps the full frame. An option, "Show the whole frame", sets all eight.
Test: a fresh game has two buttons; each condition adds exactly one.

### P2. Three promises: the Tasks panel is the tutorial and the story

**Rule.** Tasks holds promises to named people, each with a day. It never holds an instruction. Three open at most in
the first hour (later seven). The first three teach the five beats.

| Task | Given by | Teaches | Pays |
|---|---|---|---|
| The priest's wine to Traiectum: 4 casks | a priest's man at the dam heads | the hold, a course, cast off, arriving | 12 d |
| The count's earnest: 40 d by St John's day | the count's reeve | the debt, a marked day, coming home | the debt falls to 440 d |
| A word for Folkmar's sister at Traiectum | a man of your crew | talks, the crew's wants | his mood, and one true price word |

**Numbers.** A line is at most eight words and a day. A done line turns pale, stays one game day, then goes to the
log. A missed task is not deleted: it becomes a line on what the person now thinks.

**The player sees.** One line at minute 2, three by minute 5. Pointing at a line rings its port on the chart and its
day in the top bar.

**Build.** `Tasks` reads commissions and chain flags, which exist. Add a `task` field (the line's key) to a commission
and to a chain step. The first three are plain cards in `encounters/first_days_793.json`.

### P3. A first debt day inside the hour

**Rule.** Decision 10 makes the debt one sum on one day. At one second a minute, Michaelmas is about 40 hours of
sailing away, so a new player never meets the debt on the first evening. Add an **earnest**: 40 d to the reeve by St
John's day, 24 June, three days after the start. It can be paid early. This changes decision 10 and is yours to decide.

**Numbers.** Start: 86 d, 9 days of bread, 12 empty lots. The first run pays 12 d carriage and 10 to 40 d on your own
goods. Paid: the debt reads 440 d. Missed: no game over; the debt becomes 528 d (480 and a tenth) and the reeve likes
you less. A third road: ask for a week, at 10 d.

**The player sees.** A marked day in the top bar from minute 3. A log line two days before, a slip the evening
before, a card on the day with the reeve in the portrait.

**Build.** One scheduled card, two flags (`earnest_paid`, `earnest_missed`), an optional first mark on `Debt`.
Test: paying sets 440; sailing past the day sets 528 and never ends the game.

### P4. The first hour, minute by minute

Real minutes. The clock is held in every panel.

| Minute | Game time | What happens | New on screen |
|---|---|---|---|
| 0:00 | 21 June, third hour | New game. One board: who you are, what you owe. "Begin". | |
| to 0:02 | held | She lies at the dam heads. The town slip shows three sites only: the dam heads, the reeve's hall, the ale-house. | Top bar: settings, date, 86 d, 9 days of bread. Hold, Tasks. Your portrait. Ship window: the boat at the dam. Log: three lines |
| to 0:04 | held | The reeve. His face takes the portrait. The debt and the earnest are said. | A marked day. Task 2 |
| to 0:05 | held | The priest's man. Four casks drop into the hold. | Task 1. Hold shows 4 |
| to 0:08 | held | **First trade.** The market. Each good carries its word. You buy with your own silver. | Ledger. Three talk pips |
| to 0:09 | held | **First course.** Click Traiectum: the line is drawn, "half a day". Cast off. | The clock's 1x, 2x, 4x: absent until the clock could run |
| to 0:15 | to the ninth hour | **First voyage**: 30 km down the river, 6.5 minutes at 1x. The ship window shows the banks. | After 60 s at 1x, the helmsman's line on speed |
| 0:11 | | **First card**, 40% into the leg: a ferry rope across the stream. Three choices, none ruinous, odds "sure, sure, likely". Folkmar speaks. | Crew |
| to 0:18 | held | **First arrival.** The reveal line. The wine is handed over: 12 d. Your own goods are sold. | Log: "The run made 19 d after bread." |
| to 0:22 | held | Three talks. The priest tells of Lindisfarne, two weeks old. Folkmar's sister. | Chronicle. Task 3 done |
| 0:25 | evening | The first free choice: lodge, or sail by night (the log warns first). A crewman gives the first lines of a poem. | Chest. Night in the ship window |
| to 0:40 | 22 June | Home against the stream, about 9 hours. One or two cards; the first is weather, seen coming on the chart. | Bread at 8 days |
| to 0:50 | 23 June, held | Dorestad with all its sites. Haggle once. A skipper from Lundenwic: wine pays well there, but Mercian ports are shut to Franks. | World. Log: St John's is tomorrow. A slip at dusk |
| to 0:55 | 24 June | **First debt day.** The reeve's card: pay 40 d, ask a week, or let it pass. | The debt reads 440 d, or the harder sum |
| to 1:00 | held | The sea is offered: the Lundenwic word is in the Ledger and a new task leads west. | Six buttons. A sea course shows "2 to 4 days" |

**The player sees.** About a fifth of the screen under the frame at minute 0, a third at minute 60 with one panel
open (?). The chart everywhere else.

**Build.** No tutorial mode. It is ordinary play plus: the three starting cards, the earnest, the button conditions,
a list of three "first call" sites for Dorestad, one fixed first river card, and start stores of 9 days (the chart
game now starts with none).

### P5. Tooltips: two deep, and the word has a source

**Rule.** (1) The hover line says what the thing is. (2) Hold still, or press Alt, and it locks; underlined words in
it can be pointed at. (3) The note that opens from such a word is the last. A note on a period word ends with its
source.

**Numbers.** Shows after 0.35 s. Locks after 1.0 s still, or at once on Alt. Hover line: two lines, about 62 letters.
Note: eight lines. A period word is underlined the first three times it is seen. Costs always read in one order:
silver, hours or days, odds.

**The player sees.** Over "86 d": "Silver. 86 pennies' weight." Locked: "You owe the count 480 d by **Michaelmas**."
Over Michaelmas: "29 September. The count's day." Over a price: the word on it, who said it and when.

**Build.** `TooltipLayer` in the top panel: a stack of at most two, the lock timer, an edge flip. `glossary_793.json`
(`term`, `line`, `note`, `source`), marked in text by a tag at write time. A seen-count per term in the save.
Test: a third level cannot open.

### P6. The log and the four ranks of notice

**Rule.** Everything that happens is one log line. Its rank, set in data, decides how loudly it is told.

| Rank | What | Shown as | Clock | Limit |
|---|---|---|---|---|
| 1 | a fact: the wind veered, a sail seen, a price heard | a log line | runs | none |
| 2 | a thing to weigh: a task due in two days, stores low, a man uneasy | a log line, a count on its button, a ring on the chart | runs | 6 a game day |
| 3 | a thing coming: a gale building, the debt eve, a strange sail closing | a slip beside her mark | drops to 1x | 2 a game day outside danger |
| 4 | a choice | a card | held | 1 to 3 a leg |

**Numbers.** Three lines show in the frame; a click opens twelve; the wheel reads back. The log keeps the year, then
feeds the winter saga. A line starts with the hour by name (Prime, Terce, Now) and is at most 62 letters. Ink as in
`ui_direction.md`: warm white seen, pale stone heard, stone stale. Two of the same line within an hour become one
with a count.

**The player sees.** "Terce. A skipper in from Lundenwic says wine pays well there." Clicking it opens the Ledger at
wine and rings Lundenwic. No line asks to be dismissed.

**Build.** `NoticeBus.Say(key, rank, subject)`; `LogBook` as a `ListView` over a saved list; each line holds its
`subject` (a port, good, man or ship) for the click. Reuse the cards' speed drop for rank 3. Test: seven rank 2
notices in one day show six and fold one.

### P7. The men speak up only when you stall

**Rule.** No advisor, no pop-up tip. A hint is a line from one of your four men, in the log and for a moment in the
portrait, and only after a stall.

**Numbers.** Triggers: 45 s in port with nothing clicked; a course that outruns the bread; 60 s at 1x on a first leg;
a full hold clicked three times; the debt eve with too little silver. One hint in three minutes at most. Each hint
three times in a life at most, and never once the thing has been done. One switch: "The men keep their counsel."

**The player sees.** "Aldgrim: We have bread for nine days, skipper. That is twelve."

**Build.** `Hints`: a table (`id`, `when`, `quiet_after_flag`, `line`, `speaker`), an idle timer, counts in the save.

### P8. Shown before it is chosen

**Rule.** No choice in the first hour costs something the screen did not show first.

**Numbers.** A course shows its days as a range. An act shows silver and hours. A card shows the odds as a word. A
hazard stands on the chart at least two game hours before its card. The first card of a new kind is never the worst.

**Build.** A seen-flag per card family; the pacer picks the mildest card of an unseen family. Test: across 200 seeded
first hours, nobody loses the boat or a man before St John's day.

## 3. How to know it works

Sit a new player down, say nothing, and watch. Pass marks: cast off by minute 10, the first sale by minute 20, every
button opened once by minute 60, the earnest paid or knowingly refused. Write the minute of each `hud.<id>` flag to a
file so the table in P4 can be corrected from real runs.

## 4. Yours to decide

1. **The earnest** (P3): a first 40 d on St John's day, or one debt day only, with the toll as the first lesson.
2. **Buttons that appear** (P1), or all eight from the start.
3. **Hints from the crew** (P7): on by default, or off.
4. **The log's hours**: by name (Prime, Terce), or plain ("morning").

## Sources

1. George Fan, GDC 2012: https://www.gamedeveloper.com/design/gdc-2012-10-tutorial-tips-from-i-plants-vs-zombies-i-creator-george-fan
2. Dredge: https://www.gamedeveloper.com/design/trawling-in-the-deep-how-black-salt-games-made-spooky-fishing-rpg-i-dredge-i-
3. Sunless Sea postmortem: https://www.gamedeveloper.com/audio/postmortem-failbetter-games-i-sunless-sea-i-
4. Slay the Spire: https://en.wikipedia.org/wiki/Slay_the_Spire
5. Into the Breach: https://en.wikipedia.org/wiki/Into_the_Breach
6. Hades: https://en.wikipedia.org/wiki/Hades_(video_game)
7. Stardew Valley quests: https://stardewvalleywiki.com/Quests
8. NN/g, progressive disclosure: https://www.nngroup.com/articles/progressive-disclosure/

From memory, not reached (?): Crusader Kings III's tooltips, Civilization's advisor, Dredge's pursuits list.
