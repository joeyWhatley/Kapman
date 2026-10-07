# The first ten minutes of a new game (read from code, not run)

Written 7 Oct 2026 from reading, not playing: `Design/wiring_plan.md` section 8, `Assets/Kapman/Code/Game/UI/Frame/*.cs`
(FrameRoot.Self.cs, FrameRoot.Rack.cs, FrameWindows.cs, the top-bar part of FrameRoot.cs, the empty-state lines of
`Panels/*.cs`) and `Assets/Kapman/Code/Game/Chart/ChartWorld.cs`. Every quoted line is a string in the code.
What was NOT read (so the minutes below are thinner there): the bodies of the panels (only their empty-state lines),
FrameRoot.Floats.cs, the title menu, the port's own menu (TextPanel), and the pictures in `Captures/chart`. The
timings are an estimate of a reader's pace, not measured.

## What a new game is, as the code has it

- She lies at the home port (`ChartWorld.homePort = "dorestad"`), heading 270, moored, no course.
- Aboard at the start (ChartWorld.Start, only if there is under half a day of food): 4 rhenish_wine, 2 quernstones,
  1 ships_bread, 1 ale, each as far as the hold has room. Nothing tells the player this was given or why.
- The frame: top bar (date, part of day, silver as "N d", debt or "nothing owed", "bread for ..."), tiled windows,
  the portrait with "Crew of N: health, heart", the ship window (a drawn stand-in) with the wind and "At <port>",
  a log, eight function buttons.
- Four workspaces (FrameWindows.Defaults): 1 Sea (voyage, hold), 2 Port (trade, port, people), 3 Self (self, bonds,
  faith, tongues), 4 House (house, tasks, word, chronicle). In port the frame goes to Port, at sea to Sea
  (FrameRoot.cs line 375).
- The eight buttons: Cast off, Wait, Measure, Look out, Sound, Hail, Rest, Offer.

## Minute by minute

**0:00 to 1:30. "Who are you?"** After the title's New game a floating window opens that cannot be shut
(mustAnswer, holds the clock): "Who are you?" / "Three choices before the first morning. They hold for the whole
life." Three groups of chips: Your people, Your faith, Your past, each with one line under it (people: "You start at
<port>, and the world has the colours of the <name> palette. Your tongue is ...", plus the first sentence of what is
hard; faith: "It asks: ... It opens: ..."; past: stat names with bare numbers, the gift, "You keep: ..."). One
button: "Begin: <people>; <faith>; <past>". Picking a people changes the chart's colours at once and resets the faith.
- Works out alone: that the three lines are all the guidance there is; what a stat number such as "<Stat> 2" is out of.
- Lost: the past's tilt is a number with no scale and no word on what the stat does. "They hold for the whole life"
  with no way to read more before choosing (the Wiki panel is not built, 8.8).
- Not as the brief says: the start is the people's home port (`world.StartAt(people.home)`), so only a people whose
  home is dorestad begins at Dorestat. The boat, cargo and debt are the same at every home (ChartWorld comment).

**1:30 to 2:30. The first look.** The window shuts. The log's first line: "You begin. <People>, of <Port>. <Faith>.
<Past>." The call at home is quiet (BeginCallQuietly): nobody comes down, no card. The Port workspace is up: Trade,
Port, People. The top bar shows silver in "d" and a debt.
- Works out alone: that "d" is pence; why there is a debt already and to whom; that the clock is running in port
  ("In port the clock runs", 8.5) and bread is being eaten while they read.
- Lost: NO HINT WHAT TO DO FIRST. The log holds one line of who you are and nothing of what to do. There is wine and
  quernstones in the hold and no word of where they are wanted.

**2:30 to 4:30. Poking at the port.** Trade lists the market; People lists who can be spoken to (a talk writes "You
speak with <name> at <place>."); Port lists what can be done. The waterfront talk may log "You hear the talk of the
waterfront: N things new to you. The Chronicle has them." (Chronicle is on workspace 4, not on screen.)
- Works out alone: buy low here, sell elsewhere, with no price elsewhere known: Word says "You have no word of any
  market's prices yet. Look at a market, or talk to people in port."
- Lost: where a place has no people written, People says "Nothing is written of the people of this place yet."
  Where a port has nothing but its market, Port says "Nothing more to be done here but trade."

**4:30 to 5:30. Trying the buttons.** Cast off is greyed; pressed, the log says "Cast off: no course is set. Pick a
port in the Voyage window and set its course first." This is the best hint in the game, and it is only got by
pressing a greyed button (or hovering it). Sound: "Sound: the lead is read along a charted course, and none is set."
Hail: "Hail: not yet. Hailing a sail or the shore has no rule in the game yet." Rest: "Rest: not yet. A watch below
has no rule in the game yet, and no hours are skipped." Offer: "Offer: not yet. Offerings and vows come with the
faiths." Wait opens "For what? The clock runs while you wait, and bread is eaten." with, by day and with no course,
both real choices greyed ("It is light already." / "No course is set: fair for where?") and only "Never mind".
- Lost: three of eight buttons say "not yet" (not greyed: Look leaves can = true for them, so they look live). Wait
  in port by day is a window with nothing to pick.

**5:30 to 6:30. Finding the Voyage window.** It is on workspace 1 (Sea), and in port the frame shows workspace 2. The
player has to press [1] or the "1 Sea" button, or click a port's mark on the chart (a click picks that port in the
Voyage window, 8.6). If nothing else is known: "You know of no other port yet."
- Works out alone: that workspaces are numbered buttons; that the chart's marks can be clicked.
- Lost: the hint names "the Voyage window", which is not on screen, and does not say where it is.

**6:30 to 7:30. Setting a course.** Log: "Course set for <Port>: N km by water, about <days>." Cast off now says on
hover "Cast off for <Port>." Measure [D]: "The measure is in your hand: click two places on the chart."
- Works out alone: whether the bread aboard (1 ships_bread, 1 ale, shown as "bread for ...") lasts the days named;
  nothing sets the two figures side by side.
- Lost: Cast off can still be refused with "Finish what is in front of you first." when the port's menu is not the
  open thing; it does not say what is in front of you.

**7:30 to 10:00. Under way.** "You cast off from <Port> for <Port>." The frame goes to Sea: Voyage and Hold. The mark
sails the charted course on the game's clock. Cast off has become "Lie to". Look out gives the wind, the sky, the
sails in sight ("No sail in sight." or "A sail to the <airt>, N km off: too far to make out.") and the nearest known
port. Sound gives fathoms on a sea leg, or "The lead: the river's own channel, deep enough for her." A card may come
(none at sea within four game hours of the last, 8.7); it stops the clock, and the bar says "waits for your answer".
- Works out alone: the speed buttons; that Space lets the clock run with a card still open.
- Lost: meetings at sea, raiders, the king's guard boats and the events of events_793.json do not happen on the
  chart (8.8), so sails are seen and nothing comes of them, and Hail says "not yet". A card with an unwritten key
  shows "[write: key]" (4,426 such keys, 8.8). A card's speaker has an empty shape for a face. `ice` reads 0.
  On a river leg the first ten minutes may end with nothing having happened but the mark moving.

## Where the player is lost, gathered

1. No first step is given anywhere; the only pointer is behind a greyed button.
2. The window that hint names (Voyage) is on another workspace than the one shown in port.
3. Hail, Rest and Offer look live and answer "not yet".
4. Wait, by day in port, offers nothing.
5. The starting cargo, the debt and "d" are unexplained.
6. Stat numbers at creation have no scale.
7. Panels that say only what is missing: Bonds ("You know nobody yet..."; "How the people of the towns feel about you
   is not kept yet"), Crew ("The crew are not yet people with names..."), Faith ("Keeping these days is not counted
   yet..."), House ("Plots, stores, other boats and an heir are not in the game yet."), Ship ("Her wear, her fittings
   and a yard to mend her at are not in the game yet."), Sky ("No tide is reckoned on the chart yet."), Tasks
   ("Nothing taken on yet."; passengers: none carried), Tongues ("No words of other tongues met yet."), Chronicle
   ("Nothing is known yet."), Event ("Nothing is asked of you now").
8. The Chronicle is pointed at by the log and sits on workspace 4.
9. Sails are sighted and cannot be met.
10. "[write: key]" on cards.

## The ten cheapest mends, one line each

1. After "You begin.", add two log lines: what is aboard, and what to do (pick a port in Voyage [1], set a course, Cast off).
2. Make the Cast off hint say where: "...in the Voyage window (workspace 1 Sea, or click a port on the chart)...".
3. Put `voyage` in the Port workspace's defaults (FrameWindows.Defaults), so the course can be set where she lies.
4. Set `can = false` for Hail, Rest and Offer in `FrameRoot.Look`, so they are greyed like any button that cannot be used.
5. In WaitFloat, when both choices are off, say so in one line ("Nothing to wait for now") instead of two greyed rows.
6. Give the top bar's silver, debt and bread boxes a tooltip each (what "d" is, who is owed and by when, how many men eat).
7. In PastLine, write the tilt with a sign and a scale, or add the stat's own one-line meaning from SelfBook.
8. In the "waterfront talk" log line, name the workspace: "The Chronicle (4 House) has them."
9. Say in "Finish what is in front of you first." what it is (the open window's title).
10. Mark "(later)" in the panel list the panels whose body is mostly a "not yet" line (Crew, Ship, House), and hide "[write: key]" lines on cards.

## Two things for the lead, not mends

- Section 8.2 of wiring_plan.md still calls the third workspace News (The word, Chronicle, Event); the code has Self
  and House, and 18 panel files where 8.1 lists eight. The plan is behind the code.
- The brief says a new game begins at Dorestat; the code begins it at the chosen people's home port.
