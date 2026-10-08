# The first play-through, a picture at each step

9 October 2026, small hours. The road's step 9, the alpha's row 18.

## What was done

The game was played by the test's hands through the real buttons, from the title to the Michaelmas reckoning:
the loader, the title, the lender's questions, the showroom, Ravn, the first order, the list "Before she sails"
(the sail by its row's button, the three men in the alehouse), and then a whole season of trade.

- The test is `Assets/Kapman/Tests/PlayMode/PlayThroughTests.cs`. It is long and is not part of the suites. Run it
  by its name: `-runTests -testPlatform PlayMode -testFilter PlayThroughTests` (with graphics). `KAPMAN_PT_DAYS=6`
  in the environment gives a short walk of six days.
- The pictures are in `Captures/playthrough`, with `journal.txt` (every step in words) and `faults.txt` (what
  the page's rule found on each picture) beside them. Each run writes them anew. The first walk's journal and
  faults are kept beside this file (`playthrough_1_journal.txt`, `playthrough_1_faults.txt`); its 549 pictures
  are on sheets of four in `G:\kapman_unity\pt_run1`. The picture numbers below are the first walk's.
- The season is played by a plain trader. It sells what it brought, buys what the next port pays most for,
  keeps bread aboard, rests the men in port, has a yard mend her, answers every card, and is home for the day.
  It reads the markets as they are, which a player cannot: it is there to carry the hands through every window,
  not to play well.

## How it went

151 days in 14 minutes on the wall. 62 calls at 16 ports, 60 passages, 166 cards answered, 64 ships sighted,
4 gales met lying to, the sail split twice and her seams done twice at a yard. She was never lost. 1,495 clicks,
1,048 keys.

The purse began at 86 d. The debt of 1920 d was paid on 13 August at Quentovic, 47 days before the day. The purse
stood highest at 1479 d (15 July) and ended at 421 d. On 24 September she was home; the card "A clean stick"
came with the lender's line and Ravn's, and on Michaelmas the bar says "nothing owed".

I looked at every picture (138 sheets of four). What I found is below, in three lists.

**The walk was then made again with the mends in.** It ended otherwise: 55 calls, 171 cards, raiders alongside on
18 June (talked to, then paid), and on Michaelmas the purse held 784 d against 1920 d owed. That walk found one
more fault, the worst of all (list 1, point 1). Its journal and faults are `playthrough_2_journal.txt` and
`playthrough_2_faults.txt`. The page's rule found 31 faults on the first walk's pictures and 6 on this one's.

**And a third time, with that mended.** 43 calls, 165 cards, the sail split off Quentovic on 16 September and
was sewn at sea by its own card. On Michaelmas at first light the lender's card came to her at home, "Two men
and a rope", with 1580 d in the purse against 1920 d: "Tell him you have not got it", and she works for him.
Journal and faults: `playthrough_3_journal.txt`, `playthrough_3_faults.txt` (5 faults of the page: three are the
texts' own brackets, two were the debt's words in the bar, mended after).

## 1. Mended at once (they would have spoiled your own play)

1. **The lender never came for a boat that lay at home on the day.** His card meets her as she moors. The second
   walk lay at Dorestat from 25 September with 784 d against 1920 d owed: Michaelmas passed, three more days
   passed, nothing was said, and she did not work for him. The day now comes to her where she lies: made fast
   with the day come and the boat still owed for, his card is asked for from first light.
2. **Bound for one port she was made fast at another.** Up the Wantsum for Fordwic she was moored at Sarre seven
   times in eight, with "Bound for Fordwic: 9 km to go" in the Voyage window. Any port whose reach her course
   crossed took her. Now a port beside her way is passed; to call there, chart the course to it.
3. **A split sail could not be mended at sea.** Her sail split on 1 August. Its card was left to the deck's
   chance and had not come by the 13th: she made 78 km a day on half a sail for twelve days. The card of the
   split sail and the card of the gaining leak are now offered when it happens, as the gale's are.
4. **The first sign of a gale came "within the hour".** The weather was read twelve hours ahead where she lay,
   and she sails into weather. It is read along her course now, where she will be each hour.
5. **Two systems told the same thing.** The old happenings "She's taking water" ("a strake has sprung", with the
   Ship window saying the hull was sound), "A gale" and "The water turns" came beside the boat and the cards of
   the same things. The three are out of the happenings' table.
6. **A ship sighting opened on top of a card.** "Hold your course / Close and hail her" came up over what came of
   a card, before it was read. No sail is offered while a card is on the screen.
7. **The debt's words were cut off in the top bar** on days whose date is long, and in port. They are said whole
   where there is room, and without the day's name where there is not (the tip and Errands say the day).
8. **Errands kept the words of an old deed at its head.** On 13 August it still began with the hand-over of 2 May.
   What a button there said is dropped at another port or on another day.
9. **A yard's work was offered on a sound hull** from the third day on ("Her seams: 11 p"). It is offered when
   she is no longer sound.
10. **A tight boat was "wet underfoot" and lost room in the hold** (660 kg down to 560 and back, every two days).
   A sound hull's weep is scooped out as it comes: she is dry.
11. **Rune stones of the 900s were given in 793** at Ripa: Gorm's stone, Harald's stone, Tove's stone. A stone
    is copied into the chest only from the year it stands. Their names lose the scholars' numbers ("Gorm's stone").
12. **"Not now." with no reason.** A choice shut for want of silver said "Not now. You need 48 d of silver."; one
    shut by two things said "Not now. Not now." Only the part that shuts a choice is said now. New reasons:
    "Only with the sail shortened.", "Only if you carry 4 bolts of Frisian cloth."
13. **"The crew have lost heart." for a hundredth part** (after shortening sail). Small changes have smaller
    words now: "The crew grumble.", "The crew have lost some heart." (placeholders, yours to write).
14. **The Book had none of 502 people's pages open** after a season. A person of a place met on a card is met.
15. **"Bubo, the factor of Reric"**: the lender's man was named for the port where his card first opened. A
    person of a place is of that place.
16. **The ship view's two lines ran together** ("At Dorestatnear gale, west"), and it said "Lying to in the gale"
    in a near gale. Two lines now; "in the gale" only in one.
17. **What came of a card let the clock run.** The clock ran again from the choice, and at the fastest clock a
    day passes in the four seconds a card's end takes to read: an evening song opened at 22:00 was done at 01:45.
    A card holds the clock until it is gone on from now; you may still set the clock running under it. THIS IS
    MY READING of your ruling ("it stops the clock but you can override it"): yours to overrule.
18. Small words: "Hire Aldger (4 d)" is "Hire Aldger, 4 d"; a mark says ", for 35 days"; "for 1 men" is "one
    man"; "Talk: talk on the waterfront" is "Talk on the waterfront"; "You broke his word" is "your word"; "No
    tide is reckoned on the chart yet." is gone; Tongues' rule is said in plain words; House says "your standing
    there" away from home; Crew says "still in port" in port; the tag "Ravn's word" no longer pushes the Sell
    button out of its line or squeezes a port's days; the Ship window's small print is shorter.

Tests: `PlayThroughMendsTests` (5) hold what a test can hold without the running game.

## 2. Seen, and not mended yet (mine)

1. **A fifth and a sixth man on a boat of four.** Fourteen crew cards cast "the hand" as a stranger ("Uffo, the
   sailor", "Folcger, the sailor"): Blows, The nicked penny, Counting in his tongue, the cards of a man who is
   sick, goes home or must go. Who of the four may fall sick or leave is a ruling (Design/fixes/one_cast.md,
   parts 4 and 6).
2. **The small ship view is black or white in some pictures** at the fastest clock (pt_086, pt_088, pt_349).
3. **To check: what is learned in play.** Journey has the same 4 pieces of news and 35 markets at the end as on
   the first morning, and Tongues says "No words of other tongues met yet" after a season among five tongues.
   The trader never pressed "Talk to people" in a port, where news is heard, so part of this is its own doing.
   The next walk should talk on every quay and then be read again.
4. Calendar texts carry brackets ("Saint John the Baptist (midsummer)", "eat meat (unless a feast falls on
   it)"); so do three fitts of Beowulf ("Banquet (continued)").
5. Hold: a catch of fish is told as "Your own from the start."
6. House: the Name line's pips run out of the window's right edge.
7. A place's card says "A low house with a guarded door and no window on the street side": the page's rule
   takes "window" for a workshop word. The rule is too wide there.
8. Raiders alongside: after a parley they will not hear, the same four choices come back with "Parley" still
   among them, for ever (the third walk talked to two shiploads of Northmen for six minutes).

## 3. For you to rule

1. **Three walks, three ends.** The same plain trader, with the markets open to it, paid the whole debt on
   13 August in the first walk (one sale of glass vessels and Tating ware at Lindisfarena took the purse from
   547 d to 1458 d), stood 1136 d short on the day in the second, and 340 d short in the third. The debt can
   be paid and can be missed. Whether that is the hardness you want is yours to say.
2. **The whole debt was paid by a button in Errands at Quentovic**, far from the lender. Should it be paid only
   at Dorestat, or to his man?
3. **The men are paid twice.** Their wage is paid by itself in port ("The men are paid 1 d."), and the cards "The
   reckoning" and "The promise" ask 30 d and 20 d "their share" besides.
4. **After one hard passage the men's heart stayed "low" for five weeks**, paid, fed and rested.
5. **The men are "weary" after four days at sea and "spent" after seven** (they row half as well, and the sea
   finds her twice as soon). A night lying to rests them. The numbers are first guesses.
6. **The king's toll card came at 25 of 62 calls**: every Frankish port, every time.
7. **64 ship sightings**, each asking "Hold your course / Close and hail her".
8. **The old happenings still come beside the cards** (A man asks passage, Fish, A dispute at the market,
   Becalmed, Fog, A man falls sick, grumbling, letters, theft): no speaker, their own plain words. "A man asks
   passage" pays 8 d at once and nobody is carried. Which stay?
9. **On the first morning she knows 35 markets' prices and 135 of 140 wares** from the waterfront's talk.
10. **1 May is Philip and James**, a feast, "not to be done: work", on the day the game says to mend the sail.
11. **A name given at random can be "Bubo"**, the stand-in name of the lender's man.
12. **The Port window's title is "Dorestat (Wijk bij Duurstede)"**: the modern name, in brackets.
13. **What four pressing cards take when nobody answers** is the safe thing since the boat can be lost
    (DECISIONS 114). The walk took those answers each time and lost heart each time ("lie to": less heart).
