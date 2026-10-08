# The alpha

The user, 7 Oct 2026: "I think we will get it to an alpha version with the basics all done then add more content."

So the order is: the basics, all of them, working and joined, in one build that can be played from a new game to the
first reckoning; and only then more to meet. This page says what "the basics" are, where each stands, and what waits.

## What the alpha is

One life begun on 1 May 793 at the home port of the people chosen, played on the twelve ports with the cards that
exist, as far as the first reckoning of the debt at Michaelmas and on into the years after, with nothing in it that
says "not yet".

## The basics, and where each stands

| # | Basic | Today | Where it is being done |
|---|---|---|---|
| 1 | A new game: people, faith, past; home port by people | built | - |
| 2 | The clock: pause and five speeds, dropping back by itself | ruled (item 26), not built | queue part 2; Design/fixes/clock_five_speeds.md |
| 3 | Sailing: a course, cast off, sail or oars, weather that matters, all twelve ports reachable | built but for oars; the four ports mended today | queue parts 1 and 5 |
| 4 | The boat can be hurt and lost like any other | not built | Design/fixes/risk_is_real.md, second queue |
| 5 | Trading: buy and sell by the lot, a price told rightly and by whom, the toll | built in part; advice wrong, one unit at a time | Design/fixes/word_and_lots.md, one_cast.md, second queue |
| 6 | Money that pays, and a debt with a day and an end (paid, or what follows if not) | the debt is no stake; no day | queue part 2; Design/fixes/money_that_pays.md, debt_day.md |
| 7 | Cards in port and at sea: they wait a week, pressing ones hold her, each choice has a written end, the speaker has a name and a face | cards open; the rest not built | queue part 3; the content pass |
| 8 | Meetings at sea: other traders, a guard boat, a raider | being mended now | queue parts 1 and 3 |
| 9 | Orders that can be taken, carried in parts and paid fairly | broken three ways | Design/fixes/orders.md, second queue |
| 10 | People: talk, the old tongue turning into English, five stats that open choices and that events move | built but for events moving stats | queue part 4 |
| 11 | The frame as ruled: brief panels that open full windows, chart tools on the right, text large enough | part built | queue parts 2 and 5 |
| 12 | The wiki: linked words, a box under the cursor, pages that fill as you learn | data baked, not in the game | queue part 4 |
| 13 | The 3D view of the ship: sea, weather, the jetty, the crew at the oars, the PSX looks, changing places with the chart | first pictures today, in a copy | the ship view build, then my merge |
| 14 | Saving: autosave, nothing lost on a load | being done now | queue part 1 |
| 15 | Sound on the chart | not built | queue part 2 |
| 16 | The user's sprites show in the game, with stand-ins | drafted | queue part 3 |
| 17 | The first hour: a stake due inside it, clear what to do, a clean screen | not built | Design/fixes/first_hour.md, page_and_strings.md, second queue |
| 18 | It holds together: tests pass; a play-through from a new game to the reckoning with a picture at each step | per part | queue part 6, and again after the second queue |

The alpha is reached when every row reads "built" and the play-through of row 18 has been looked at by the user.

## What waits until after the alpha (more to meet, not basics)

- More cards, chains, ports and people than exist today; the other 38 ports; the later years' cards.
- Heirs and later lives; the fellowship, partners and many ships (Design/fellowship.md, scale_to_fleets_plan.md).
- The hours of history, tempers and pillars, bonds as a system, wars, settlements (Design/fate_faith_and_self.md,
  living_world.md).
- Reading and riddles at sea (Design/fixes/reading_at_sea.md), more peoples and faiths (Design/peoples_and_faiths.md).
- Final art (the user draws; DRAW FIRST.html), more sound, Steam (Design/steam_release.md).
- The user's own hand through the words.

## The order of work

1. The queue running now (Design/QUEUE.md, six parts).
2. A second queue, from the write-ups in Design/fixes/ that answer rows 4, 5, 6, 9 and 17, and whatever the first
   queue left.
3. The ship view merged in.
4. One play-through of the whole, and the same three critics again, as the alpha's review.
5. Then content.

## Where it stood on 7 October at 16:30 (the queue was stopped here, at the user's word, to re-evaluate)

Built and committed on main since this page was written (EditMode 417 tests, 415 pass, 0 fail, 2 skipped; the chart
play-through 6 of 6, 111 pictures in Captures/chart): row 2 the five-speed clock, with proof that the same seed ends
the same slowly and at speed 5; row 3 all twelve ports reachable; row 6 the debt of 1,920 d due at Michaelmas, the
traders at work since 1 April, the count's casks his own; row 8 meetings at sea (traders hailed, a guard boat that
cannot hold her for ever, raiders); row 11 in part (text in four sizes, nothing under 18 px); row 14 autosave and a
load that brings everything back; row 15 sound on the chart. Also the palette mend.
Built on its own branch, not merged: row 13 the 3D view of the ship (branch shipview in G:/kapman_unity/Kapman2_ship:
Design/ship_view/BRING_HOME.md). The cards fitted to the rules, on branch content in G:/kapman_unity/Kapman2_content.
Not started: the queue's parts 3 to 6 (the art loader and the card clock; the wiki and stats; the frame as ruled; the
play-through) and everything in Design/fixes that answers rows 4, 5, 9 and 17.
Known faults to carry forward: the debt chain's cards still say 480 d under a bar that says 1,920; paying through the
cards does not lower the debt shown; she starts with a hold too full to buy a cask; the ten-year run of the world
leaves three traders stuck at the new margin (a skipped test); the log does not yet read as sentences (ruling 39).

## 7 October, evening: three of the known faults mended, and notes while playing

- The log reads as sentences (ruling 39). A card answered is told as what you did, from the card's own line of what
  the people remember ("You sat low and listened."), then what came of it in sentences, with the feelings of everybody
  in one sentence and no inner word in brackets. The lookout's lines, word that comes, and a button's refusal likewise.
  Code/Core/Encounters/LogProse.cs; tests LogProseTests.
- The debt cards and the bar are one debt. The chain house_debt asks for what is still owed (Trade.Debt, 1,920 d at the
  start), by tale, by weight in the new penny (367 in 480 of it) or a quarter down on a surety, and paying on a card
  takes it off the bar. New card effects "law debt_paid", "law debt_weight", "law debt_quarter", "law debt_cleared";
  new checks debt_short, debt_weight_short, debt_quarter_short. The rewards inside the chain (120 d, 240 d, 466 d)
  were written against the old 480 d and are NOT rescaled: to be looked at with the economy.
- The start cargo has lost its quernstones (Design/fixes/first_hour.md). STILL OPEN: her hold is 660 kg and the four
  casks of the count take 552, so 78 kg stand free: sacks can be bought on the first morning, a cask cannot. A cask
  needs either three casks of the count in place of four (ruling 14 says four) or a bigger hold. The user decides.
- Notes while playing: F2 holds the clock and picks a thing on the screen; a slip takes what should change and, for
  words, the words it should say instead. Kept in Notes/play_notes.md, Notes/play_notes.jsonl and Notes/shots.
  Code/Game/UI/Frame/PlayNotes.cs; test PlayNotesTests.
- Tests after this: EditMode 421 total, 419 passed, 0 failed, 2 skipped; PlayMode ChartGameTests 6 of 6 and
  PlayNotesTests 1 of 1.

## 7 October, late evening: the beginning of the game, talking text, music for now

Built from the user's own script of the same evening (DECISIONS items 47 to 51; pictures Captures/chart/open_*.png):
- Row 17 (the first hour) has its start: a fake loader, the title, the lender's five questions in a box that comes up
  from the bottom (people, name, faith, past), who he is and what is owed, the boat alone turning on a pink void, the
  first morning with the place's card, the marks on the first click, and Ravn, who says what must be done and what to
  buy. Showtime.cs, TextReveal.cs, PeriodNames.cs + Resources/names_793.json, ShipStage.Showroom, test OpeningTests.
- The first jobs are rules: one man aboard at the start, hands hired on the quay, the sail mended at the port, no
  casting off until both are done. FirstJobs.cs.
- Music for now, from a Music folder beside the game that is in no commit and no build: GameMusic.cs.
- All of it is off in a batch run, so the other play tests still use the old creation window.
Still to do here: the art slots for the lender's face, Ravn's face, the name as a drawn sign and the mark; the count's
chain of cards (house_debt) sleeps in a game begun through the opening and wants rewriting to the lender or dropping;
the crew as people (Ravn's own poor stats) waits for the crew system; the hold's 78 kg (see above) is still open.

## 8 October, night: three rows moved (G:\KAPMAN; DECISIONS 101 to 105)

- Row 6 (a debt with a day and an end): BUILT for a game begun through the opening. The lender's man who keeps
  turning up and takes any sum on the stick; the day itself; paid, the stick is given up; unpaid, she works for the
  lender (half of what she sells goes to the stick) until it is clean. What follows a missed day is my choice and the
  user's to overrule (DECISIONS 104).
- Row 7 (cards): "each choice has a written end" is BUILT as far as the script goes: the 1,990 ends and 233 pairs of
  chance ends that were written and never shown are shown. 2,466 choices have none written yet. "The speaker has a
  name and a face" is BUILT: 1,569 of the cards' 2,328 people are named people, the same on every card, and the
  holders of offices are the ports' own. Still not built in this row: cards that wait a week, pressing ones that
  hold her.
- Row 10 (people): the men hired on the first morning are the crew the cards know.

## 8 October, late night: the road's steps 1, 7 and 5 (DECISIONS 107 to 109)

- Row 7 (cards): BUILT. Every card of the first release has its words in the game (the five regions' 5,259 texts
  were merged). A card left unanswered with the clock running waits a week and then takes one of its own options;
  a pressing card waits hours and holds her where she is; the card says so under itself and the log says what it
  took. Still open in this row: 778 choices of the release have no written end.
- Row 10 (people): BUILT. Events move the stats: a stat is a base and the marks that lie on it, each with a cause
  and an end, two up or down at most. Cards put them on, known deeds by a table, hunger and sickness by themselves.
  Yourself shows them.
- Row 3 (sailing): the oars are BUILT (DECISIONS 110). The order Out oars stands beside her course in the Voyage
  window: the hard pull, for the hours that are in the men, at the cost of bread and strength.
- Row 9 (orders): BUILT (DECISIONS 111). Offered and taken or refused, brought in parts, paid by the unit like a
  sale, warned of and closed on their day.

