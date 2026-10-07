# Feel and feedback: how the chart comes alive

6 October 2026. Nothing here was run in Unity. **(?)** marks what is from memory and not checked (the search budget
was used up; nine pages were fetched). Sound: CC0 or public-domain recordings only, cut and layered, never made from
nothing. No music.

## 1. Findings

| Game | What it does | What Kapman takes |
|---|---|---|
| Theory (Swink) | Feel is control "with interactions emphasised by polish". Weak or quiet sound makes objects feel weak; good sound is "non-repetitive". [1] | Every act gets a picture answer and a sound answer. Every sound is a pool, never one clip. |
| Dredge | Time advances only "whenever the player performs actions such as sailing or fishing". At night "the music gives way to static-like noise". [2] | The sound bed itself is the alarm. Danger is the bed thinning out. |
| Obra Dinn | Correct fates are confirmed "only in sets of three". [3] A death is heard before it is seen (?). | Hold praise back and give it in one lump: the reckoning after a leg, not a chime per sale. |
| Papers, Please | The "clunky" interface was "a deliberate design choice". [4] The stamp has travel, a thump and leaves ink (?). | An act with weight takes two moves and leaves a mark you can see later. |
| Mini Metro | A system makes "sounds based on events in the game"; the harmony changes with "the size and shape" of the network. [5] | No free-standing jingles. Each sound is caused by a thing on the chart. |
| FTL | Calm and battle versions of each track "play at the same time", the game "cross-fading between the versions". [6] | The same with recorded beds: fair and foul run together and the weather sets the mix. |
| Dorfromantik | Tracks "that would not be boring or distract". Biomes "shift into each other". [7] | Seasons slide, they do not switch. |
| Balatro | "Lo-fi music and irreverent sound effects". [8] The score counts up, faster and higher (?). | The purse counts up coin by coin. A big sum takes longer and sounds heavier. |
| Loop Hero | Tiles laid side by side change each other; an "80's aesthetic". [9] | Feel in pixel art is two-frame swaps and whole-pixel moves, not engine tweens. |

Three lessons run through all nine.

1. **Answer within a tenth of a second.** The result may come later; the click is answered at once.
2. **Size the answer to the act.** Games that feel cheap celebrate everything the same.
3. **Leave a mark.** A cut notch, an empty seat, a burnt port. Marks are the cheapest depth there is.

## 2. The house rules of feel

**Rule A. Three sizes, with a budget.**

| Size | For | Picture | Sound | How often |
|---|---|---|---|---|
| Tick | Every click | 1 pixel, 2 frames | One short clip, -18 dB | Always |
| Mark | An act done (a sale, a toll, casting off) | 3 to 6 frames, a number moves | One or a few clips, -10 dB | Each act |
| Bell | A death, home, winter, a best-ever run | The bed changes, the clock is held, a mark stays on the stick | The mix itself changes for 3 to 6 s | At most once in ten real minutes |

**Rule B. Pixel motion.** Things move by whole pixels at 10 frames a second. Nothing is scaled, turned, blurred or
faded by the engine (`ui_direction.md`, section 4). A fade is a dither in 3 steps.

**Rule C. Sound pools.** At least 4 cuts a pool. Never the same cut twice running. Level jitter of 1.5 dB. No pitch
shifting (the audio plan already bans it for water). A larger event picks a larger recording.

**Rule D. Quiet is a sound.** A warning takes sound away before it adds any. Ducking takes 300 ms down, 1.5 s back.

**Rule E. Loss is slower than gain.** Silver coming in is quick and bright. Silver going out is counted slowly.

**Build, once, for all of section 3:** `FeelBus` in Core (a static event: name, size, place on the chart, amount).
`FeelPlayer` at runtime reads `feel.json` (event, clip pool, level, duck, frames) and plays through the existing
`SoundVoices`. Game code only says `FeelBus.Fire("sale", lots, silver)`. One night. EditMode test: every event name
used in code has a row, and every clip in a row is on disk with a credit line.

## 3. The acts

Clock: 1 real second is 1 game minute at 1x.

| Act | Rule and numbers | What the player sees | What to build |
|---|---|---|---|
| **Casting off** | Mark. Two moves: "Cast off", then the line is let go. She stays 400 ms, then moves. Speed is held at 1x for 3 s. The bed crossfades from port to water over 2.5 s. | The mooring line drops (2 frames). Her mark turns to the course. A wake of 2x2 dots, 3 long, each lasting 2 s. | Event on the unmoor step. Clips: a `ropes_straining` cut, `footsteps_on_planks`, then `oars_paddles_calm` on a river or a `sail_flapping` cut at sea. Wake dots in `MapView`. |
| **A sale** | Mark. One coin clip a lot, 90 ms apart, at most 12 heard. The purse counts up over 0.4 s (under 12 d) to 1.2 s (over 240 d). No colour for profit. A sale at twice the cost adds one heavy "purse set down" clip and a nod from the buyer. | The lot leaves its hold cell (2 frames), a penny hops to the purse, the number rolls. | `PurseCounter`. Coin pool: **a gap** (section 7). Until found: cuts of `wooden_gate_bolts` as a wooden knock. |
| **A toll** | Mark, by rule E. Coins go out 150 ms apart, at most 6. The purse counts down over 1.5 s. Then one bolt clip: the way is open. | The toll-keeper in the portrait place. Pennies leave one by one. The bar at that port lifts (2 frames). | Hook the pay step of the toll encounter. `wooden_gate_bolts_741621` is imported. |
| **A card at sea** | Mark. Speed falls to 1x for 1 s, then the clock is held. The bed ducks 6 dB. The card arrives with the one sound of its fact (a gull, a creak). After a choice, 600 ms of nothing, then the result. | The card steps in 4 px over 3 frames beside her mark. The portrait looks toward it. | A `sound` field on each card (a pool name). The wait in the card runner. |
| **Storm warning** | A ladder, by rule D. **6 h out:** gulls stop, far thunder, a darker band on the windward edge of the chart. **3 h out:** the wind arrow stands by her, the rigging rises 6 dB, one man says one line. **1 h out:** speed to 1x and the card. Each rung rings the nearest lee. | The sea steps one shade darker each rung. The portrait looks to windward, then pulls his hood up. | `StormWatch`: reads what the weather model holds ahead on her course, fires three events. Needs a "nearest shelter" query on the lanes. |
| **A death** | Bell. The clock is held. All but water ducks to -20 dB for 4 s. No sting. At the next port with a church, one bell. | His head leaves his seat. **The seat stays empty** until a man is hired. A notch in blood is cut on the stick and never fades. | Crew seat state. A lasting sign on the stick. `bell_one_toll` exists (a cast bell, the wrong kind). |
| **Coming home** | Bell, only after 30 days away; else a mark. Within 3 km of Dorestat the home bed comes in by distance: dogs and a bell, then the smithy, then the market. On mooring the knife cuts the stick and the purse is counted once. | The lit hearths at dusk (3 pixels). The reckon line: what the run made after wages and bread. | A distance gate on the town emitters, driven from the chart. `market_crowd_medieval`, `smithy`, `dogs` exist. |
| **Winter** | Bell, once a year. The chart steps to frost over 3 s in 3 steps. All ship sound stops. The bed is `winter_wind_bare_trees` outside, `hearth_fire_crackling` inside, `ice_lake_fractures` each 40 to 120 s. The saga appears a line each 1.2 s. | Smoke from every lived-in port. Ice dither on the river. The portrait in a cloak, breath showing. | A winter palette for `MapArt`. All three clips are imported. |

## 4. The map

The chart must move while you only watch. Seven cheap things, all drawn on the pixel grid of the chart.

| Thing | Rule and numbers | What the player sees | Build |
|---|---|---|---|
| **Wind** | Streaks 1 px high, 3 to 6 long, moving downwind at 4 px/s per force step. 20 on screen at force 2, 120 at force 7, none in a calm. | The sea is combed by the wind; in a calm it lies flat. Her sail has 3 drawn states: full, shaking, down. | A particle list in `MapView` fed by `WorldSim.WeatherAt` at 9 points on screen. |
| **Smoke** | Each lived-in port has a 2-frame smoke of 3 px that leans with the wind. A burnt port has none for a year. | A wind vane at every town, and the first sign that a town is gone. | One sprite a port, flipped by wind side. |
| **Tide** | On tidal coasts the flats have 3 drawn states on the 12.4 hour cycle (12.4 real minutes at 1x). A bar shows a rust mark for 2 hours round low water. | The Wadden shore breathes. You learn the bars by watching. | A mask tile set for the flats; the phase from `TideWait`. Frisian coast and Thames first. |
| **Light** | Four palettes (dawn, day, dusk, night), swapped in steps each 10 game minutes. Night is three greys, with a lantern ring 24 px round her. | The day passes on the chart. Lit ports are single warm pixels at night. | A palette ramp on the chart material; `MapPalette` already recolours tiles. |
| **Weather** | Cloud shadow: dither patches 40 to 120 px wide that drift. Rain: a 2-frame dither over its area. Fog hides the sails inside it. Lightning: a one-frame light step, thunder 1 s later per 3 km, at most 5 s. | Weather is a place on the chart that you can sail round. | Three low masks (64x36) from the weather model, scaled by whole numbers. |
| **Seasons** | Four land palettes, each change spread over 3 weeks in 3 steps. Geese cross in March and October (a 5 px skein, with `geese_407490`). Sea ice dither in the Baltic from December. | The first skein south says the sea will close. | Palette steps by date. A flock sprite on a line. |
| **Other ships** | A 2-frame bob and a wake of 2 dots (laden) or 4 (fast). A first sighting blinks a bracket twice, no sound. A known raider gets a horn (a gap) once. | The sea is used, and a wake tells you what kind of ship it is. | Wake length from the speed in the fleet simulation. |

**The sound of the chart** follows her mark: the existing `SoundscapeModel` is given her place, hour, wind and coast
kind instead of a 3D listener. `L_Chart` is silent today, so this is the largest gain for the least work.

## 5. The frame

This holds for the thick bevelled frame of template 3 and for the board of `ui_direction.md`.

| Part | Rule and numbers | What the player sees | Build |
|---|---|---|---|
| **Buttons** | Tick. On press the face moves 1 px down and right and the light and dark edges of the bevel swap (the `.on` state in the mockup). The act fires on release. Hover makes no sound. | A button that goes in like wood. | A pressed-sprite swap. Tick pool: 6 cuts of wood on wood from `well_bucket_rope` and `wooden_gate_bolts`. |
| **Panels** | Opening: 6 px in 3 frames (90 ms) from the edge of the frame, with a lid or peg clip. Closing: 60 ms, 6 dB quieter. A shut panel leaves chart, not empty frame. | Modules that are put down and picked up. | One panel animator. The frame is drawn as pieces round each open panel, not as a full ring. |
| **The day stick** | Each midnight a notch is cut: a 3-frame knife mark and a knife-in-wood clip (a gap; a short cut of `axe_chopping` until found). At 4x, at most one clip in 2 s. | Time is something done to wood. | Hook the day change of the clock. |
| **The portrait** | Six faces: plain, glad, grim, weary, cold, hurt. He blinks each 4 to 9 s. He glances for 0.8 s toward what just happened: down at the purse, left at the chart, up at the sky. Hood up in rain, breath in frost. Three drawn ages over a life. In port the speaker takes the place. | The one living thing in the frame. He tells the mood of the boat before any number does. | `PortraitState` listens to `FeelBus` and picks a face by priority (hurt, grim, cold, weary, glad, plain). 18 sprites of 48x48 a life. |

## 6. The ship window

**Rule: the window shows what the chart says, and it is where the sound comes from.** Open or shut, the ear is on her
deck. Opening the window lifts the sounds of the ship herself by 6 dB (deck mode in the audio plan).

- **Wind:** sea state in 4 steps, sail full or shaking, her heel.
- **Hour, weather, season:** sky, rain, fog, the lantern at night, snow on the thwarts, ice at the edge.
- **Place:** a river bank with reeds; a coast with land on the true side; open sea; a waterfront when moored.
- **Her state:** lower in the water when laden (1 px per 3 lots), the right count of men, an empty seat, oars out in a calm.
- **Events:** the sail on a card is seen in the window, on the true side.

Numbers: 320x180, drawn at 15 frames a second, camera fixed off her quarter, a slow drift of 2 px. No controls.

**Cheap first:** layered sprites, like the stand-in in the mockup: sky (4), sea (4 states, 2 frames), ship (3 sails),
bank (4 kinds), rain and fog. About 30 sprites and one night of code. **Later:** the KapmanLook pixel scene in a render
texture, fed the same inputs through one struct, `ShipViewState`.

## 7. What is cheap first

| Step | What | Cost |
|---|---|---|
| 1 | `FeelBus`, `feel.json`, button and panel ticks | 1 night |
| 2 | The sound bed of the chart, from her place (all clips exist) | 1 night |
| 3 | Cast off, sale, toll, card duck and wait, purse count | 1 night |
| 4 | Wake, wind streaks, port smoke, her sail states | 1 night |
| 5 | Light by hour, night, lit ports | 1 night |
| 6 | The storm ladder | 1 to 2 nights |
| 7 | The six faces of the portrait | 1 night, plus drawing |
| 8 | The cut on the stick, the death mark, home, winter | 2 nights |
| 9 | The ship window, sprite version | 1 night, plus drawing |
| 10 | Seasons, tide flats, weather patches | 2 to 3 nights |

**Recordings to find (CC0):** coins on wood, singly and as a purse; a knife cutting wood; a wooden lid and a peg; a
rope thrown on planks; a riveted hand bell; a horn. Each is a gap until found. Every stand-in named above is a real
recording already in the project.

## 8. Yours to decide

1. **The purse count.** May one coin recording be played a little higher each step, or only different real recordings? I assumed the second.
2. **The bell at a death.** A cast bell now, or silence until a hand bell is found?
3. **The portrait.** Six faces and three ages (18 drawings a life), or two faces to start?
4. **The cut on the stick:** heard every midnight, or only on days that carry a sign?

## Sources

1. Game feel (the definition by Swink and his five kinds of polish): https://en.wikipedia.org/wiki/Game_feel
2. Dredge: https://en.wikipedia.org/wiki/Dredge_(video_game) ; https://www.gamedeveloper.com/design/creating-the-creepy-salty-world-of-dredge
3. Return of the Obra Dinn: https://en.wikipedia.org/wiki/Return_of_the_Obra_Dinn
4. Papers, Please: https://en.wikipedia.org/wiki/Papers,_Please
5. Mini Metro: https://en.wikipedia.org/wiki/Mini_Metro_(video_game)
6. FTL: https://en.wikipedia.org/wiki/FTL:_Faster_Than_Light
7. Dorfromantik: https://en.wikipedia.org/wiki/Dorfromantik
8. Balatro: https://en.wikipedia.org/wiki/Balatro
9. Loop Hero: https://en.wikipedia.org/wiki/Loop_Hero