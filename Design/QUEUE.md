# The queue

The user, 7 Oct 2026, about 11:15: "You do what you think needs to be done."

This is what I think needs to be done, in order, once Unity is free (the second builder of the frame and the reviewer
are using it; one Unity runs at a time). The aim is the one the book sets (Design/GAME_MAKER.md): **one good hour**,
Dorestat to Lundenwic, that a stranger can play. Everything below serves that or stands in its way.

Rules for every step: compile, run the tests, look at a screenshot, commit; never push; one Unity at a time; plain
words; nothing under Assets changed by two hands at once. The rulings are Design/DECISIONS.md, 7 October, items 1 to 16.

## 1. Mend what is broken (small, and each one unblocks play)

| What | Why | Where it is written |
|---|---|---|
| Four ports cannot be reached; the Maas mouth sends her the long way | the player's planner shuts every "pending" lane (Course.cs line 64): re-kind 12 edges, do not open them all | Design/research/next/routes_fix.md |
| Nothing happens at sea | raiders, guard boats, dated events and trader meetings wait for the map to close, and on the chart it never does; a guard boat can hold her for ever from 800 | Design/fix_sea_meetings.md |
| The debt is no stake | famine on grain only, English toll a tenth (ready: Bake/economy/apply_money_fix.py --apply); the world's traders at work from 1 April (code); the debt due on a day | Design/research/next/money.md |
| Colours some players cannot tell apart | the seventeen moves (ready: Bake/world/apply_palette_mend.py --apply) | Design/research/next/art.md B3 |
| Text too small | four sizes (18, 22, 26, 30 at 1080p) and a floor of 18 px | Design/text_size_audit.md |
| A crash loses the game | autosave at each port call and on quit; the frame's log and layout into the save; an open card comes back as it was | Design/save_audit.md |
| The chart is silent | the soundscape that the 3D levels have, on the chart, by where she is | Design/sound_audit.md |

## 2. Bring in what the helpers made (it lies outside Assets, checked but not yet in the game)

| What | From |
|---|---|
| Cards that wait a week, pressing cards that hold her, the option each takes by itself, a face for each speaker (rulings 5, 10, 12) | Bake/content/out/card_rules_793.json, Bake/content/card_rules.py |
| The user's sprites shown in the game, with stand-ins (ruling 13) | Design/art_loader/ |
| The wiki: its data, its window, linked words, the box under the cursor (rulings 3, 6) | Bake/content/out/wiki_793.json, Bake/content/wiki_rules.json, Design/wiki.md |
| Anything the art check added | Bake/art, Design/art_guide.md |

## 3. The frame as ruled today (rulings 7 to 9)

- Everything about the chart on the right; the measure there; Oars among the eight buttons.
- Every panel with two faces: brief in the left column (three sizes), full in a floating window.
- The button on the ship view that changes its place with the chart.

## 4. The 3D view of the ship (ruling 11)

From the two plans and their drafts in Design/ship_view/: the stage (sea, sky, weather, wind from the game's own
code), the jetty built from the pack's planks and logs when she is moored, the crew at the oars, four or five PSX
looks to flip between, and the view at full size when it changes places with the chart.

## 5. Play it, and tell the user the truth

A play-through from a new game to Lundenwic by the frame alone, with a screenshot at each step, every one looked at;
what works, what does not, what to do next. Then the user plays.

## Left for the user, or for later

- **To read:** the sixty first-hour cards (Design/content/first_hour_cards.md when the helper has written it); the pay
  cap proposal (Bake/economy/out/pay_cap_proposed.json: 766 amounts, 72 marked for a human eye); the wording faults
  (Design/content/wording_fixes_ports.md and _sea.md: 145 lines); the nine trap chains (Design/content/trap_chains.md:
  my choice is to leave all nine out of the first release, as the card study planned).
- **To draw:** DRAW FIRST.html in the Desktop folder.
- **To start whenever they wish:** the Steam paperwork (Design/steam_release.md).
- **Later:** the licences written down properly (Design/credits_audit.md); sounds still to find for the ship view;
  the other three card rewrites; the later books.

## How it is run

One builder at a time, each with a short brief and this file, because one Unity runs at a time and a fresh agent
reads less than this long session does. Each step ends in a commit. I look at every screenshot before the next starts.
