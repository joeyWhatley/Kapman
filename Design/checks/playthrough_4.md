# The fourth play-through: the whole first life, with the chart's fog

10 October 2026, small hours. Asked for by the picker: "Ready it for your play".

## What was done

The same test as before (`Assets/Kapman/Tests/PlayMode/PlayThroughTests.cs`), brought up to date with the rulings
of 9 October. It plays a new game by the real buttons, from the title to the reckoning at Michaelmas 794.

- **It finds ports as a player does.** A port she has only heard of is looked for by its line under "By hearsay"
  in the Voyage window, a click on the water in the middle of its ring, "Set this course", Cast off. When its
  harbour comes in sight it is charted by its name. It looks where cargo aboard was bought for such a port
  (Ravn's bar iron for Medemelacha), and otherwise for one new port every fourth call, of the first release's
  twelve.
- **It answers the sea.** When the game says wind and tide have set her off her line, it stops the clock and
  presses "Lay the course again". When the steersman has lost his reckoning in thick air it lies to until he can
  see, then lays the course again.
- **It stops when something is wrong.** Under way with no way made for a day and a night, or her course taken
  from her six times running, is a fault that ends the walk with everything that could hold her written down.
- **Its trader keeps silver to trade with.** It pays the whole debt only when 200 d would be left.
- It takes 45 minutes on the wall for the 516 days. Run it as its own process, or the tool's time runs out:
  `Start-Process Unity.exe -ArgumentList '-batchmode','-projectPath','G:\KAPMAN','-runTests','-testPlatform','PlayMode','-testFilter','PlayThroughTests',...`.
  `KAPMAN_PT_DAYS=24` in the environment gives a walk of 24 days (5 minutes).

The journal and the faults of this walk are beside this file (`playthrough_4_journal.txt`,
`playthrough_4_faults.txt`). Its 705 pictures are in `Captures/playthrough` until the next walk writes over them.
The journals of the two walks that went wrong before it are in `G:\kapman_unity\walk_runs` (run1, run2).

## How it went

1 May 793 to 30 September 794: 516 days in 44 minutes on the wall. 200 calls at 13 ports, 206 passages, 377 cards
answered, 6,275 clicks and 5,441 keys. She was never lost. The page's rule read all 705 pictures and found no
fault: nothing written for the makers, no text out of its box, no writing missing.

- **All twelve ports of the first release were found.** Eight by pointing at their rings: Medemelacha (2 May),
  Daventre (7 May), Doccinga (12 May), Walichrum (20 May), Serræ (1 June), Quentovic (19 June), Lundenwic
  (24 June), Ripa (2 July). Witla and Fordwic came in sight by the way.
- **The money.** The purse began at 86 d and stood highest at 1,604 d. The first order was handed over at
  Traiectum; Ravn's iron sold at Medemelacha at a gain.
- **The lender's men met her three times, at sea.** On 3 October 793 she had 17 d and told them so: they turned
  out the purse and carried off ten crates of jugs, counted as 112 d on the stick. On 4 April and 31 July 794 she
  counted out a pound.
- **The debt was paid on 24 September 794 at Medemelacha, five days before the day.** At home on the 25th the
  card "A clean stick" came, with Ravn's line. On Michaelmas the bar says "nothing owed".
- **The sea.** Her course was laid again 483 times: 477 when the game said she was 8 km or more off her line,
  6 when the steersman had lost his reckoning. All six were in thick air (he saw 300 m to a kilometre), and she
  lay to for it between one hour and eight. Her seams were done at a yard 21 times and her sail three times.
- **She sailed through the winter** (31 calls from December to February): the game has no winter yet.

I looked at 16 pictures of the four walks myself, seven of them of this walk's 705 (her course laid again, the rope
men alongside, the silver for the debt, the clean stick, the reckoning, the crew's window, a standstill). Not at
every one, as I did on 9 October: the page's rule read them all.

## What the walks found in the game, mended at once

Three walks were made before this one went through. What they showed, and DECISIONS has each in full:

1. **Cast off, she could stand still for ever** (DECISIONS 135). The point she steered for, 300 m ahead along her
   course, was her own place whenever she lay 150 m short of the berth's own point. She stood at Doccinga from
   13 May 793 to a December gale, with nothing said; the saved game of that mooring stood still again when loaded.
   It could happen at any port. The point she steers for is now never her own place.
2. **A port behind tidal flats could not be found by pointing** (134). Pointed at Lundenwic's ring she lay to in
   the Thames mouth, 50 km short. A course to a place never tried the flats. It does now, where the deep water
   would leave her far short.
3. **The debt's window and its button disagreed** (134). At Serræ, with 1,477 d and 1,440 d owed, Errands said
   "You have the silver for it" and "Pay it all" answered "not enough silver": her Frankish coin is worth less
   at an English market. Both go by what the purse buys where she lies now, and say so.
4. **A reckoning lost in a fog said nothing of the fog** (133). The steersman lost it four times in four hours at
   one spot. The log and the Voyage window now say that the air is thick and that he would wait for it to clear.
5. **Twelve names of the calendar carried brackets** in the Faith and days window (134).

## Seen and not mended: yours to rule

1. **She is set 8 km off her line two or three times a day at sea**, and each time the clock drops back and the
   course wants laying again: 477 times in this walk. It is the ruling of 9 October ("you correct it by pointing
   again"), and it may be too often.
2. **The king's toll came 117 times**: the same card at every call at a Frankish port.
3. **The crew's heart was "close to leaving" at 135 of the 200 calls** and nobody left: the crew's cards are held.
4. **She stood nearly still for six hours or more 19 times**, in narrow water with the tide against her, or lying
   to in a gale. Nothing on the screen says the tide holds her.
5. **Whether she should take up her course again by herself when a fog clears.** Today the player lays it again.
6. **Nothing happens in winter, and nothing after the debt is paid**: the game goes on as before.
7. **A tall card covers the left end of the chart's row of buttons** (Measure and half of Land) while it is up.
