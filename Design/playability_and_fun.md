# Playability, fun and replayability

Design pass of 4 October 2026, from the user's brief of 4 October ("the playability, the fun, the replay ability") and the
GDD of 3 October (https://claude.ai/code/artifact/ad87e81b-b44c-44fd-b11c-e942dc636ee4). Revised the same day after a
critique: the clock's units corrected throughout (one real minute is one game hour), the first slice re-scoped to the
Dorestad level that exists, landmarks and the tide moved into the builders' data, the position model made physical, one
anachronism removed, and the port loop and the debt's arithmetic added. This document designs the minute-by-minute loop,
the voyage's pacing, risk and reward, progression, the first hour, the port and the evening, fairness, replayability, the
three modes and the Build mode, what to cut, and a set of fun tests per milestone. It designs systems, not dressing and not
words: every line of text stays a placeholder with an intention and an outcome, as the user set on 1 October.

What it depends on (built or planned elsewhere): the storm plan (Plans/storms.md), the crew plan (Plans/crew.md), map play
(Plans/map_play.md), AI fleets (Plans/ai_fleets.md), reputation, places, collectables and multiplayer; and the sister
documents of this pass (Design/economy_and_trade.md, gateways_and_level_chain.md, npc_text_encounters.md,
weather_and_sea_state.md, ship_physics.md). It does not redesign them; it says how they are paced and felt, and where they
need a hook.

Facts carry a source. Numbers without a source are marked (guess) and live in data tables to be tuned at the playtest.

**Units, once and for all.** The clock runs 1 real second = 1 game minute (the user's rule). So 1 real minute = 1 game
hour, 24 real minutes = 1 game day, and a 30-real-minute voyage is 30 game hours. Everywhere a player has to act, this
document gives the time in **real minutes** (and in game hours where that helps, the same number). A "game minute" is one
real second and is never used as a lead time. Data files name their unit in a `units` field, as raiders_793.json and
seamanship_793.json already do.

---

## 1. What is true

### 1.1 How fast she goes

Replica trials of the Roskilde finds, the base of the ship classes already in the data (ship_classes_793.json):

| Find | Type | Length | Crew | Cargo | Replica speed | Source |
|---|---|---|---|---|---|---|
| Skuldelev 1 (Ottar) | sea-going cargo ship, knarr | 16 m | 6-8 | about 24 t for this hull (Viking Ship Museum figure, from memory: guess); the knarr type "could carry up to 30 tons" | top about 13 kn ([Skuldelev ships](https://en.wikipedia.org/wiki/Skuldelev_ships)); a sailed average of 5-7 kn is (guess) | [Skuldelev ships](https://en.wikipedia.org/wiki/Skuldelev_ships), [Knarr](https://en.wikipedia.org/wiki/Knarr) |
| Skuldelev 3 (Roar Ege) | coastal cargo ship, byrding | 14 m | 5-8 | "4-5 tons", draught 0.9 m | top "c. 10 knots"; a sailed average of 4-5 kn is (guess) | [Skuldelev ships](https://en.wikipedia.org/wiki/Skuldelev_ships) |
| Skuldelev 5 (Sebbe Als) | small warship | 17.3 m | about 30 | - | 5 kn on oars alone, 12 kn under sail | [Skuldelev ships](https://en.wikipedia.org/wiki/Skuldelev_ships) |
| Skuldelev 2 (Sea Stallion) | longship | about 30 m | 60-65 | - | Roskilde 1 July to Dublin 14 August 2007, with stops | [Sea Stallion](https://en.wikipedia.org/wiki/Sea_Stallion_from_Glendalough) |
| Dorestat 1 | clinker ship found at Dorestad, wood felled after 739, riveted; a replica sailed in 2021 | - | - | - | - | PROGRESS.md (the project's own notes) |
| Vleuten 1 | the Utrecht type: an expanded logboat, 734, 15 m by 1.5 m | 15 m | - | - | - | PROGRESS.md |

The player's first boat is the Gislinge boat, 7.7 m, 4 men, 0.9 t of cargo, 0.34 m of laden freeboard
(ship_classes_793.json `gislinge`). The GDD's speed rule stands: 4-6 kn under sail, about 3.5 kn rowing, by the class
polar. The Roskilde finds are 11th century and the Gislinge is c. 1130 (PROGRESS.md), so they are speed evidence and
stand-ins; the Frisian evidence for the ships a Dorestad house would actually own is Dorestat 1 and the Utrecht type, and
the ladder in 2.5 is named by the data's own Frisian and Frankish classes, not by the Danish finds.

### 1.2 How far, and how long, Dorestad to Lundenwic

| Quantity | Value | Where from |
|---|---|---|
| Real distance by the lanes | 380-480 km (the lane test's bounds); about 430 km | Plans/ai_fleets.md (EditMode test "Dorestad-Lundenwic is 380-480 km") |
| Real time at 5 kn, non-stop | 430 km / 9.26 km/h = about 46 game hours | computed. Plans/ai_fleets.md quotes "about 87 h", which is not consistent with its own lane length at 5 kn; that figure should be re-derived from the lane and the polar's average, not quoted |
| Real time coasting with nights at anchor | 4-5 days (guess) | - |
| In the game, at 1x | about 30 real minutes = about 30 game hours = 1.25 game days | GDD decisions table: "about a day and a quarter of game time, against 3 to 4 days in history" |
| The southern North Sea level | 492 km real shortened 1:150 to 3.3 km corner to corner | PROGRESS.md |
| The Rhine mouth to the Thames mouth inside it | about 1,400 m sailed at 2.5 m/s = 9-10 real minutes | Design/gateways_and_level_chain.md, worked budget |
| GDD target for an ocean level | 8 to 15 minutes at 1x | GDD, "Two kinds of level" |
| **Measured: Dorestad, the mooring to the exit** | the Kromme Rijn channel in the level data is 2,679 m long (135 points, `G:\kapman_unity\data\levels\dorestad\meta.json`); the exit sits at 2,619 m along it (DorestadTown: `line[Count - 4]`); the player's ship is moored at the first storehouse dam past 790 m from the fork (`Apex - 60`, Apex = 850); so about **1,800 m of channel to sail**. At the level's `shipSpeed` = 1.6 m/s (DorestadBuilder) that is **19 real minutes = 19 game hours**; with the stream at 2.0 m/s, 15 minutes; at 3.5 m/s, 8.5 minutes | computed from the built level's data and code |

Two things follow. The ocean is within the GDD's budget once the sailed leg (not the level's diagonal) is counted: 9-10
minutes of open sea. The river place is not: the GDD's rule is "1 to 5 minutes a place", the gateway document proposes a
join at the first big bend 1-1.5 km below the dam heads (its table gives "1,300 m at 2.0 m/s = 6 min"; the arithmetic is
11 minutes), and today's level is 15-19 minutes from the mooring to the exit, most of an hour's evening in Dorestad before
the sea. The 30-minute voyage depends on this more than on the ocean's ratio. Section 6, question 1, puts the choice to the
user: raise `shipSpeed` in river places (the ship "at her natural speed" already means 1/60 of her real speed in game time,
so 3-4 m/s on screen is defensible), move the mooring to the downstream end of the harbour, or cut the level with a phase at
the harbour's end. Everything below assumes a 30-minute voyage with 9-12 minutes on the open sea and Dorestad at 6-8.

**The simulation's pace.** The AI simulation of the first build sailed its traders on real kilometres and real hours: 46
game hours non-stop for this crossing, 4-5 days coasting (say 45-90 game hours). The player does it in 30 game hours. That
is a gap of 1.5-3x, not an order of magnitude: the player is a little faster than the sim, not unreachable. Either the
sim's routes move onto the levels' shortened distances (the GDD already says the sim's "routes moved onto the new water
network"), or the player's open-sea shortening is tuned so the crossing takes about 45 game hours (45 real minutes) and the
sim stays on real distances. Section 6, question 2.

### 1.3 Day, night and tide on this clock

| Thing | Real value at 52 N | On the clock (1x) | Source |
|---|---|---|---|
| One game hour; one game day | - | 1 real minute; 24 real minutes | the user's rule |
| Sun below the horizon, 21 June | about 7.3 h (about 20:20 to 03:40 solar time) | 7.3 real minutes | solar geometry at 52 N, declination 23.4 deg (computed; SkyMath renders it) |
| Of that, darker than civil twilight (sun below -6 deg) | about 5.6 h | 5.6 real minutes | same |
| Of that, darker than nautical twilight (below -12 deg) | about 3 h | 3 real minutes | same |
| Astronomical night (below -18 deg) in June | none: the sun's lowest is about -14.6 deg at 52 N | - | same. The June night is a long twilight; the bright stars show, the faint do not |
| Sun below the horizon, 21 December | about 16.3 h; true dark (below -18 deg) about 12 h | 16 real minutes; 12 | same; Plans/multiplayer.md's "7 real minutes in June, 16 in December" agrees |
| One tide (high to high) | 12 h 25 min | 12.4 real minutes | TideModel (EOT20), Core |
| The flood up the Thames | about 6 hours | about 6 real minutes | half a tide |
| A year | 8,760 hours | about 146 real hours | GDD "Making 273 years playable" |

So a 30-minute voyage holds about two and a half tides and one whole night, whenever you leave. A dawn cast-off from
Dorestad lands you at the Thames mouth at the next dawn and at Lundenwic mid-morning; a noon cast-off puts the landfall in
the dark. The tide and the light are the voyage's natural timers, and the only ones (GDD pacing rule: "No timers aboard
except the ones the sea makes").

### 1.4 How they found their way

Ohthere's account to King Alfred (c. 890) is the one period description of a northern voyage in the sailor's own terms. He
measures in days of sailing and in which side the land lies: north "as far as he could sail in three days", then he had
to "wait for a west wind and slightly north", then "sailed east along the land for four days"; "there was waste land all
the way on his starboard side" and "open sea had always been on his port side". From Sciringes heal to Hedeby: "Denmark
to port and a wide sea to starboard for three days", then two days with "islands belonging to Denmark on his port side"
and "Jutland and many islands to starboard" ([Ohthere of Hålogaland](https://en.wikipedia.org/wiki/Ohthere_of_H%C3%A5logaland)).

What that gives the game, as facts: pilotage was by the land in sight and which hand it lay on; progress was counted in
days; a foul wind meant waiting; the open sea was crossed by holding a heading from a known departure point, and the
heading was kept by the sun and the land, the position found by landfall. Charts and the magnetic compass were not used at
sea before 1066 (GDD, "The chart and the compass rose are the game's tools, not the period's"). Sun, stars, swell and
birds as aids are reasonable but the specific Norse practices (the sun-board, the "sunstone") are later or disputed and
are not modelled; the lead line is attested in the Roman world (Acts 27) but its Viking-age use is unproven, so soundings
are kept as a game tool and say so (guess).

### 1.5 The Thames and its tide

Today: the tidal Thames rises and falls twice a day "by up to 7 m (24 ft)" in London, with Teddington the tidal limit
([Tideway](https://en.wikipedia.org/wiki/Tideway)); "the estuary just east of the Tideway has a tidal range of 4 metres"
and its stream "moves at 2.6 knots (4.8 km/h; 3.0 mph) in bi-monthly spring tides"
([Thames Estuary](https://en.wikipedia.org/wiki/Thames_Estuary)). An earlier draft quoted "8 miles per hour" from the
Tideway article's estuary line; the 2.6 kn spring figure is the one to use.

In 793 the river was another thing. There was no bridge: the Roman one "fell into disrepair" after the early 5th century,
a Saxon bridge "may have been rebuilt by Alfred the Great soon after the Battle of Edington" or "around 990 under
Æthelred", and the first written reference is c. 1016 ([London Bridge](https://en.wikipedia.org/wiki/London_Bridge)).
The river ran unembanked, wider and shallower, and the tidal range at London was far smaller than today's 7 m: Roman-period
reconstructions put it at roughly 1.5-2 m, rising over the following centuries (guess from memory; verify in Milne, *The
Port of Roman London*, 1985, and Sidell et al., *The Holocene Evolution of the London Thames*, 2000, before the figure
goes into data). Lundenwic was the beach market along the Strand, "the beach" ([Lundenwic](https://en.wikipedia.org/wiki/Lundenwic)),
so the ship was run up on the foreshore on the tide, not moored to a quay.

The mechanic survives the smaller numbers: a ship making 4-5 kn against a 2-3 kn stream makes 1-3 kn over the ground and
goes up on the flood or waits. This is the single best "free" mechanic the real world gives the first voyage. In the game
the tidal head and the range at each river place are **per-level parameters** in the level's tide data, set from the period
figure where one exists, not from EOT20's modern value alone (EOT20 gives the timing; the level scales the range).

### 1.6 Dorestad and the raid that opens the game

- Dorestad: "approximately 3 square kilometres", "a large settlement for the time period"; "It is assumed that there was a
  toll as well as harbour fees, collected by the king's representatives at Dorestad"; peak "around the 820s and 830s";
  raided in 834; its "disappearance in the 860s" is argued to follow a shift of the river channel rather than the raids
  ([Dorestad](https://en.wikipedia.org/wiki/Dorestad)). The project's chronicle has raids repeated after 834, from the
  Annals of St-Bertin; that difference between sources is itself a thing the game can show (the "annals disagree" play in
  map_play.md).
- Lindisfarne, 8 June 793 (Swanton's correction of the Chronicle's "vi id Ianr" to "vi id Iun"); the Chronicle's portents:
  "excessive whirlwinds, lightning, and fiery dragons were seen flying in the sky"; Alcuin: "Never before has such terror
  appeared in Britain" ([Lindisfarne](https://en.wikipedia.org/wiki/Lindisfarne)). The game starts on 21 June 793
  (GDD): the news is 13 days old at Dorestad, and may not have arrived yet (news travels by ship).
- Raid pressure on the southern North Sea in 793 is low. The project's raiders_793.json puts 2 bands out in 793, both from
  western Norway by the northern isles, striking Northumbria, Scotland and Ireland (its basis: ASC 789, 793, 794, 795). The
  Danes on the Frisian coast come from about 800 (RFA 800: Charles fortifies the coast), and the first great raid on Frisia
  is Gudfred's in 810: "A Viking fleet of 200 sails unexpectedly attacked Frisia" and the people paid "100 pounds of
  silver" ([Gudfred](https://en.wikipedia.org/wiki/Gudfred)). The hazard tables in 2.3 read from that data.

### 1.7 The decisions already made that this design must obey

| Decision | Date | Where |
|---|---|---|
| 1 real second = 1 game minute; the ship at her natural speed; up to 4x; never faster when zoomed out | 26, 28 Sep; 2, 3 Oct | design decisions, GDD |
| No joystick: orders and "make for there" | 26 Sep | design decisions; ShipMotion.cs |
| Save any time, storms included | 2 Oct | Plans/storms.md answers |
| A wreck (foundered or ashore) is game over until heirs exist | 2 Oct | same |
| Storms: a careful player survives ordinary storms; the recorded storms can sink anyone in the open; natural warnings only | 2 Oct | same |
| Riddles hard, written, slow, with big rewards; limited tries that return when you learn something | 2 Oct | Plans/map_play.md |
| Placeholder text only: intention and outcome, the user writes the words | 1 Oct | gameplay spec |
| Three modes on one clock; the crew as a council; reading with the crew | 3 Oct | GDD |
| Nights play in real time with the choice to rest | 2 Oct | gameplay spec |
| Multiplayer: no pause, no 4x, no resting; full raiding; after the single-player core | 2 Oct | Plans/multiplayer.md |
| Build mode: the user dresses the world; I make tools | 29 Sep | build mode |
| The game layer of the first build is reused, adapted to the level chain, not reinvented | 3 Oct | PROGRESS.md |

### 1.8 What other games teach (verified against their records)

| Game | What it does | The lesson for Kāpman | Source |
|---|---|---|---|
| Sunless Sea | Permadeath with a legacy: "subsequent characters can inherit some of their predecessor's possessions, and a player may create a will"; "The ship movement is quite slow. This, accompanied by the music, creates an atmosphere of tension"; GameSpot found its discovery thrill ran out | Slow ships work when every mile has a question and the dark has a cost; a legacy softens permadeath; discovery must not be the only fuel, because it depletes. Our heirs are the legacy; our fuel is trade and the chronicle, which do not deplete | [Sunless Sea](https://en.wikipedia.org/wiki/Sunless_Sea) |
| Sailwind | "Use navigation instruments such as the compass and quadrant to determine your heading and position"; "Observe the sun and stars, chart your position on the map"; "tack, reef the sails in heavy weather, keep track of the direction of the wind" | Position-finding as play is proven fun in a small indie sailing game. We do it without instruments: the land, the sun, the stars and the swell, with the chart's mark growing unsure offshore | [Sailwind on Steam](https://store.steampowered.com/app/1764530/Sailwind/) |
| Sea of Thieves | No HUD markers: the map table aboard and landmarks; crew roles (steering, sails, bailing with buckets, patching holes); launch "suffered from a lack of content", voyages "elaborated fetch quests" | Reading the world instead of a marker is right. A beautiful sea with nothing to do fails at launch; the table aboard, the crew and the chest are what fill it, never a timer that pops a panel | [Sea of Thieves](https://en.wikipedia.org/wiki/Sea_of_Thieves) |
| Mount & Blade: Warband | "non-linear and has no set objectives"; start as nobody; companions; trade on price differences; renown opens doors; "capture a town or castle and start their own faction" | A ladder of standing with no scripted goal works for 100+ hours when each rung changes who deals with you. Our rungs: debt, boat, ship, standing, house, guild | [Warband](https://en.wikipedia.org/wiki/Mount_%26_Blade:_Warband) |
| Patrician II/III | "Goods of varying types are produced in the towns, and may be sold in other towns"; eight ranks; "turn to the dark side, engage in piracy, smuggle goods"; reviews mixed, 64-75 | Pure buy-low sell-high goes stale by the mid-game once automated. Our answer: knowledge decays, so nothing is automated without a man you trust; the dark road (captives, contraband) costs standing by people | [Patrician](https://en.wikipedia.org/wiki/Patrician_III:_Rise_of_the_Hanse) |
| Uncharted Waters | Trades, exploration and piracy in one sandbox; Daikoukai Jidai III required "the hiring of translators and teachers" and had generational play; early reviews: "flawed and repetitive game play" | Tongues as a mechanic (hire a man who speaks the port's language) and generations are old, good ideas; repetition is the danger of a trade sandbox | [Uncharted Waters](https://en.wikipedia.org/wiki/Uncharted_Waters:_New_Horizons) |
| Valheim | Biome progression with escalating danger; a procedurally generated world; the community filled what the game left unexplained | Danger by region, discovered by sailing into it, is a progression system in itself. Our regions are real seas with real raid pressure by year | [Valheim](https://en.wikipedia.org/wiki/Valheim) |
| Kingdom Come: Deliverance | "Abilities and stats grow depending on what the player does"; the restrictive save system was the most-criticised feature ("30 hours of real-time play, but only 19 hours had been spent advancing the game"); scholars found its "historically accurate" claim selective | Learn by doing is right (our runes and Latin). Never lock saves: the user already chose save-any-time. Say plainly what is a game tool and what is period practice | [Kingdom Come](https://en.wikipedia.org/wiki/Kingdom_Come:_Deliverance) |
| Pentiment | 25 years in three acts, the character ages; "no 'golden route,' with each decision having its pros and cons"; "you're never given enough time to pursue them all [leads]"; period scripts as a device | History seen from one small life, with choices that have no right answer and never enough time to do everything, is replayable by design. Our heirs and eras are the acts; our scripts are the real runes and Latin | [Pentiment](https://en.wikipedia.org/wiki/Pentiment_(video_game)) |
| Expeditions: Viking | Chieftain as "conquerors, merchants, or diplomats"; praised for immersion, "mangled by dozens of technical hiccups and anachronisms" (GameSpot 6/10) | The period sells itself; bugs and anachronisms are what reviewers punish. Stable first, period-true second, features third | [Expeditions: Viking](https://en.wikipedia.org/wiki/Expeditions:_Viking) |

---

## 2. The model

### 2.1 The loop at four scales, and the beat

The GDD's four loops stand: a moment aboard (1-5 real minutes), a voyage (5-60), a season (evenings), a generation. This
document adds the unit under all of them: the **beat**.

A beat is one question put to the player by the world, with a cost either way and a consequence that lands later. A voyage
is a sequence of beats. The game is fun when beats come at the right rate, are different in kind, and their consequences
can be traced back.

**Beat density.** For a voyage of length T (real minutes at 1x) with N beats, the density D = N / T. Targets (guess, to
tune at the playtest):

| Measure | Target at 1x | At 4x |
|---|---|---|
| Density D | 1 beat per 2-4 real minutes | the same beats, so 4x denser in real time; 4x is for passages the player already knows |
| Longest quiet gap G | 4 real minutes (240 game minutes), never more | 1 real minute |
| Share of beats that are mandatory (the world insists) | about half | - |
| Share that are optional (the crew, the chest, the chart) | about half | - |

Beats are placed by the world, not by timers: a landmark, a bar, a bend, a tide window, a wind shift, a sail on the
horizon, a crewman who has something to say. A quiet gap means a stretch of water with nothing in it, and the fix is to
compose the level, not to add a timer: **the gap rule is a composition rule, checked by the voyage log after the fact, and
never a runtime behaviour**. Nothing is pulled forward to fill silence (GDD: "No timers aboard except the ones the sea
makes"). The one place where the gaps are long by nature is the open sea, and there the fill is the ship mode (the crew's
unforced talk, a line and not a panel; the chest) and the chart, which the player opens when they want to.

**Beat kinds**, each with who raises it, its question, its cost and its lead time (lead times in real minutes at 1x, which
are game hours):

| Kind | Raised by | The question | Costs | Lead time |
|---|---|---|---|---|
| Pilotage | the lookout sees a landmark, a bar, a bend | hold the line, or make for there | leaving the known depths | 1-2 real minutes (1-2 game hours) from first sight, by the horizon formula and the ship's speed |
| Tide | the water (LevelWater) | go now on the ebb, or wait | waiting costs light; going costs the stream against you | the whole tide is visible on the chart's tide note, 12 real minutes ahead |
| Sail | the wind shifts (LevelWeather.WindVector) | make sail, shorten, row | men tire; strain on the mast | a gust front is seen on the water first, about half a real minute (guess) |
| Weather sign | the sky, swell, birds | run for shelter, or press on | time, or the storm | about 2 real minutes (2 game hours) for a squall, a day (24 real minutes) for a recorded storm (storms plan) |
| Encounter | a sail sighted (AI fleets Phase 2) | hail, hold, stand off, bear away | time, risk, news | first sighting head-on at 17-18 km real is "about 2 game h before meeting" (AI fleets Phase 2) = about 2 real minutes |
| Crew | a man (crew plan C5) | hear him, play, ask help, settle a quarrel | time at the table; heart | optional, held back under pressure |
| Chart | the player's own doubt | fix the position, measure a day's sail | nothing but attention | offshore, the doubt grows |
| Business | the count's date, a price note going stale | sell here, or carry on | silver, standing | the chart shows the dates |
| Port | a person in port (PortLife, GameEvents from events_793.json): an ask, a commission, news, a hire, the toll house, the count's man | take it, or not; declare, or not | time in port, silver, standing | while moored; none mandatory except the toll and the count's man on his date (section 3.7) |

### 2.2 The pressure meter

A single number P in 0-1, recomputed each game minute (once a real second at 1x), from things the player can also see:

P = clamp( 0.35 * sea + 0.25 * night_or_fog + 0.2 * threat + 0.1 * debt_near + 0.1 * leak )

where sea is the real sea state scaled to the ship's limit (storms plan S1), night_or_fog is the visibility deficit,
threat is 1 while a raider or the king's men are sighted, debt_near rises in the last tenth of the count's term (which the
terms of 2.8 make reachable), leak is water aboard over bailing. Weights are (guess) in data.

P does three things:

1. **Pacing.** Optional beats are held back while P > 0.5, so a crewman never asks for a game during a gale. Mandatory
   beats are never held back: the world does not wait. P never *adds* a beat.
2. **The camera and the sound.** The cinematic camera's cut rate and the sparse music (GDD: rare, long, quiet pieces)
   read P. Low P: long takes, silence, the water. High P: cuts, wind, no music.
3. **Telemetry.** The playtest records P over time; a session whose P never rises above 0.3 was dull, one that sat above
   0.7 for ten real minutes was exhausting. Section 4 writes it to CSV.

### 2.3 Risk and reward

Every voyage has an expected value the player can roughly feel without seeing the numbers:

EV = sum over goods of (sell - buy) * quantity  -  tolls  -  pay  -  food and water  -  E[loss]

E[loss] = sum over hazards h of p_h * L_h

The design rule is not a target EV but a **lever rule**: for every hazard, the player's choices must move p_h by at least
3x between careless and careful play. If a hazard's probability cannot be moved by the player, it is not a risk, it is a
tax, and it goes.

Base probabilities for a summer Dorestad-Lundenwic run in 793 (guesses, to tune against the headless year in
Plans/ai_fleets.md):

| Hazard | Careless | Careful | The lever | Loss L |
|---|---|---|---|---|
| Grounding on the Rhine bar or the Thames sands | 0.3 | 0.05 | the tide window, sounding, a pilot | hours waiting; hull damage if a sea runs; a wreck on rock |
| Ordinary gale at sea | 0.08 per voyage (June) | 0.08 | you cannot move the weather | - |
| Loss in an ordinary gale, given one | 0.5 | 0.03 | shorten sail, run, heave to, shelter early (per ship class: 2.4) | the ship (game over) |
| Raiders sighted, southern North Sea, 793 | 0.01 (guess) | 0.01 | by year and region, not by you: the value is **read from raiders_793.json** (`pressure.by_year`, the origins' targets and years), and the hazards row holds a multiplier on that data, not a constant. The same run is about 0.1 (guess) in 810 or 834 | - |
| Loss to raiders, given a sighting | 0.6 | 0.15 | night, fog, shoal water, a fort, a fast ship, paying | cargo, men, the ship |
| Searched by the king's men (contraband aboard) | 0.4 at Dorestad | 0.05 | the port, the direction, a gift, bulky cover | seizure, fine, judgement |
| A stale price (bought on a note over 30 days old) | 0.5 of voyages lose | 0.1 | fresher sources, several sources | a wasted voyage |
| Spoilage | by good | by good | the cargo mix, the season | part of the cargo |

Variance is the player's to choose, which is itself a fun lever: coasting is low variance (groundings, tolls, time);
crossing is high (weather, raiders, big margins); captives and swords are highest.

### 2.4 Fairness: sign, lead time, counter

The rule: **no loss without a sign the player could have read, with time to act, and at least one counter**. Every hazard
row in hazards_793.json has those three fields, and a test fails if any hazard lacks one.

| Hazard | Sign (natural only, 2 Oct) | Lead time (real minutes = game hours) | Counters |
|---|---|---|---|
| Ordinary gale | the wind backs; cloud lowers; whitecaps; the swell lengthens; gulls go inland (guess) | about 2 | shorten sail, run, heave to, make for shelter, beach her on soft ground |
| Recorded storm (12 named, storms plan S3) | a long swell the day before; a wrong-coloured sky; the crew's talk (the lookout seat); ships running for harbour | 12-24 | be in harbour. The open sea is lethal by design |
| The bar at the Rhine mouth | breakers on the bar at low water; the lookout's call; the chart's tide note | the tide is visible all day | wait for the flood to cover it; sound; take a pilot |
| The Thames sands | colour of the water; birds standing on the flats; a wreck's ribs (dressing, the user's) | 1-2 | the lead line; the flood lifts you; keep to the channel you know |
| Raiders | a sail with a low hull and many oars; a smoke on the coast; news at the last port | about 2 at sea (raiders see 12 km, raiders_793.json `at_sea.sight_km`; a head-on sighting at 17-18 km); days by news | night, fog, shoal water, a fort, pay, parley, fight |
| The king's men | their boat from the toll house; news of searches | at cast-off, or in sight | declare, dump (over 2 km off), gift, run |
| Debt | the date on the chart; the count's man at Dorestad | weeks | pay, part-pay, renegotiate (standing), flee (standing) |

**Storm survivability** (binds the storm plan's S6 to the user's answer 2): per game hour in a storm,

p_founder = base(sea / limit) * f_sail * f_heading * f_load * f_hull * f_crew

with base rising steeply once sea exceeds the ship's limit; f_sail = 1 reefed, 3 full sail (guess); f_heading = 1 head to
sea or running clean, 4 beam-on; f_load = 1 + water aboard / freeboard; f_hull by soundness; f_crew by the steersman's
seamanship (1.5 at skill 1, 0.7 at 5). The limit and the freeboard come from ship_classes_793.json per class, so the user's
"a careful player comes through ordinary storms" holds **per class**, through the sea limit the storms plan S1 defines:

| Class (ship_classes_793.json) | Length, freeboard, gale_fragility | The ordinary storm she can live through at sea | The careful counter above that | Bars (Monte Carlo, 6 game hours) |
|---|---|---|---|---|
| `gislinge` (the first boat) | 7.7 m, 0.34 m, 0.6 | force 7-8, reefed and running for a lee | be ashore or in a lee before force 9 arrives; the sign and the 2-hour lead make that possible, and the delta is never more than 2 hours from a shore | force 7-8 careful under 5%, careless over 50%; force 9-10 at sea over 50% whatever she does |
| `north_sea_merchant_ship` (14 m, 6 t, Dorestat 1's kind) | 14 m, 0.55 m, 0.4 | force 9-10 | heave to, run, shelter | careful under 5%, careless over 50% |
| `norse_allround_ship` | 15 m, 0.7 m, 0.35 | force 9-10 | the same | careful under 5%, careless over 50% |
| any class | - | the recorded storms | be in harbour | open sea over 80% whatever you do; in harbour 0% |

The sim's `Travel.Speed` already returns 0 and `sheltering = true` at a wind over 17 m/s (force 8), so the AI traders obey
the same rule by class limit once that constant becomes a class field.

### 2.5 Progression: a ladder of knowledge, not of numbers

The player has no level, no XP. What grows is **what the house knows and who it knows**, and the ship it can afford. The
ship rungs are the data's own Frisian, Frankish and English classes (ship_classes_793.json), with the Dorestad finds
(Dorestat 1, Vleuten 1: PROGRESS.md) as the evidence that a Dorestad house owned such craft; the Danish 11th-century
finds of 1.1 are speed evidence, not the ladder.

| Rung | What it takes | What changes |
|---|---|---|
| The count's debt paid | the voyages the terms of 2.8 demand (3 crossings on the easy terms, 24 on the hard) | the boat is yours; the count becomes a lord to gift, not a creditor |
| Water known | each passage adds channels, bars and tide notes (GDD "knowing the water") | you can leave later and arrive sooner; fewer groundings; 4x makes sense on known water |
| A delta and coast ship: `coaster_minster_ship` (10 m, 2.5 t, 4 men) or `logboat_expanded` (the Utrecht type, 10 m, 1.7 t, 3 men) | silver, a shipwright's port, a crew | real cargo; the Frisian coast and the Scheldt; a council of three |
| Standing in a port over +40 | deeds, work done for people, a crewman from there | better prices, less searching, lodging, commissions, hiring |
| A sea-going ship: `north_sea_merchant_ship` (14 m, 6 t, 5 men) | more silver; a hiring pool with seamen | the crossings west and north; Lundenwic, Hamwic, Quentovic, Ribe |
| A northern ship: `norse_allround_ship` (15 m, 4 t, 12 men), a later or foreign buy at a Norse market | standing with Norse dealers, silver | Birka, Hedeby, the Rus roads; a crew that rows |
| Runes, then Latin, read without help | readings done with the crew (GDD) | riddles and texts open; the monasteries deal |
| A partner, an agent, a factor | standing, silver, a man you trust | news from afar; a second route sailed for you |
| An heir | years, a marriage (M8) | the house outlives you |
| A guild | later in a life (GDD) | other merchants bound to you; multiplayer's natural unit |

Wealth should grow about linearly per voyage early, then by ship steps: each class step is about 2.5-3x the cargo by the
data (0.9 t, 2.5 t, 6 t) at roughly 3-4x the price and 1-2x the crew (guess). Debt, pay, food and tolls scale with the
ship so the margin per voyage stays a decision, not a certainty.

### 2.6 Heirs and 273 years

At 146 real hours a year, no one sails 1066 voyage by voyage. The GDD proposes three tools; this document chooses and
shapes them:

- **Played years and passed years.** The player sails the years that matter to them. A **quiet year** is passed from the
  chart in one sitting: the house's standing trades run as settled voyages on the sim (same code as the AI traders, with
  the house's own ships and a trusted man's skill), the chronicle's events of that year land on the markets and the ports,
  and a year's annal comes back with slots for the user's text. The player can stop a quiet year at any event the annal
  reports (a raid on a port where the house has goods; a death) and sail from there.
- **Era starts** (793, 834, 865, 911, 980, 1016) as new-game options, each with a house built to the time. These are the
  replay doors.
- **Heirs.** The merchant ages on the clock; death by age, sea, fight or captivity; the heir takes the mark, the ships, the
  debts and the standing (GDD). Until M8, a wreck is game over, as chosen.

The equation that keeps this honest: years passed quietly never add silver faster than years sailed. Quiet-year income
= the sim's result minus a trust loss by the factor's honesty trait (crew plan), so a quiet year with a light-fingered
factor can lose money. This stops the quiet year from being the optimal way to play.

### 2.7 Replayability: where the variation comes from

| Source | Varies by | What it changes between games |
|---|---|---|
| The world seed | the save | ordinary weather, where the traders are, the hiring pools, which riddles and texts are where, rival masters' names and temper, hoard owners and their landmarks |
| The chronicle | never: real dates in every game | the same history, met from a different place each game: Dorestad burning in 834 is a catastrophe if your warehouse is there and an opening if you are at Tiel |
| The start era | the player's choice | the ports open, the law, the coin, the raid pressure (raiders_793.json by year), the goods available |
| Home after Dorestad falls | the player's choice | which coast's rhythm you learn; which people's standing you build |
| The crew | seed plus the player's hiring | who can read what; which stories and games; which quarrels |
| The routes | knowledge | a player who learned the Thames plays a different game from one who learned the Schlei |
| The hoards | seed, placed among real landmarks, never on a real find spot (3 Oct) | a different set of owners, directions and shore walks |
| The stories | the readings done | which whole works (Beowulf, the Edda) open, and in what order |
| The dark road | the player's choice | captives, contraband, mound-breaking: a different set of ports that deal with you |
| Multiplayer | the host's world | shared markets, raiding, partnerships; an evening scenario or a kept world |
| The Build mode | the user's own work | the world's dressing is theirs, and so can be anyone's |

"Chronicle branches" in the brief are read this way: the chronicle does not branch, the player's position in it does.
That is Pentiment's device ("no 'golden route'", never "enough time to pursue them all") applied to real history.

### 2.8 Difficulty: the count's terms, in real hours

No sliders. Difficulty is **the count's terms** at the start of a game: how much is owed and by when. Everything else is
the world as it was. Multiplayer fixes 1x, no pause, no rest (2 Oct). Because this is the only lever, the terms must bite,
and that has to be computed on this clock, not felt.

Inputs: the built rule is 480 d (two pounds) due in 365 days (trade_rules.json `debt`, `due_days`); the economy document's
worked example makes 355 d gross on ten casks of wine Dorestad to Lundenwic on a good run and half that on a glutted one
(Design/economy_and_trade.md 2.3), so the **expected margin of a crossing round trip is about 200 d** (guess); a round trip
with selling is about 3 game days = 72 real minutes; a delta run (Dorestad-Traiectum and back, or the mouth and back) is
about half a game day = 12 real minutes at a margin of about 50 d (guess). From these:

voyages needed = sum / margin; days needed = voyages x days per voyage; **slack = term in days / days needed**.

| Term | Sum | Due | Term (days) | Real hours at 1x / 4x | Voyage shape | Voyages needed | Days needed | Slack |
|---|---|---|---|---|---|---|---|---|
| As built | 480 d | 21 June 794 | 365 | 146 / 36.5 | crossings | 3 | 9 | 40: never bites; `debt_near` never fires |
| Easy | 480 d | Michaelmas, 29 Sep 793 | 100 | 40 / 10 | crossings | 3 | 9 | 11: a leash you feel only if you dawdle |
| Fair | 2,400 d (10 pounds) | Michaelmas 793 | 100 | 40 / 10 | crossings | 12 | 36 | 2.8 |
| Hard | 4,800 d (20 pounds) | Michaelmas 793 | 100 | 40 / 10 | crossings | 24 | 72 | 1.4: one bad run or one storm week and the date is missed |
| Evening (multiplayer.md: "the evening ends on 29 June, when the debt falls due") | 480 d | 29 June 793 | 8 | 3.2 / - | delta runs | 10 | 5 | 1.6 |

All sums but the first are (guess) and live in debt_terms.json with the derived columns; the test `DebtClock_Dates`
asserts each term's slack is inside its stated band (hard 1.3-1.6) against the margins in the economy data, so a change
to prices that loosens the leash fails a test. The P formula's `debt_near` fires in the last tenth of the term, which on
the hard terms is the last 10 game days, about 4 real hours of play.

---

## 3. In the game

### 3.1 Dorestad to Lundenwic, minute by minute

Thirty real minutes at 1x, cast off at dawn on 21 June. On this clock that is 30 game hours: the voyage crosses one whole
June night and about two and a half tides, and the Thames is reached at the next dawn. Times are targets for the level
chain of the GDD (Dorestad's jetties, Traiectum's reach, the Rhine mouth, the North Sea, the Thames mouth, Lundenwic),
assuming the Dorestad decision of section 6 (6-8 minutes there). The tide times in the table are the table's assumption
(low water at Katwijk about 13:00, the Nore flood from about 06:00 the next morning); in play EOT20 gives the real times
for the date, and the chart's tide note shows them. What the player sees and hears comes from systems that exist (sky,
weather, water, birds, soundscape) or are planned; the words are placeholders.

| Real minute | Game clock | Where | What you see and hear | The beat | What can go wrong |
|---|---|---|---|---|---|
| 0-1 | 04:40-05:40 | Dorestad, at a dam head | the harbour comb, fire baskets dying in the dawn, smoke, geese; the toll house | **Business:** the king's toll is taken at cast-off; the count's date shows on the chart; the king's men may search (Dorestad is strict, patrols plan) | swords or captives aboard: seizure |
| 1-3 | 05:40-07:40 | the Kromme Rijn below the town | the stream carries her; a logboat ferry; a fish weir narrows the channel | **Pilotage:** keep to your side past the weir; **Sail:** row or sail with the stream; Ravn's first intention line | a barge coming up: wait below the narrows or risk a collision (seamanship rules) |
| 3-6 | 07:40-10:40 | Traiectum's reach | the Roman fort's walls, the ford, the cut bank on the bend; the morning sun on the starboard quarter | **Crew:** the mate suggests the men eat now (an optional beat: accept or not); the phase at the bend | nothing; this is the calm stretch that teaches the camera and the modes |
| 6-9 | 10:40-13:40 | the Rhine mouth at Katwijk | dunes, the bar, breakers at low water, the sea's light opening ahead; terns | **Tide:** the ebb runs out through the mouth and the bar is shoalest about 13:00; the lookout calls it; **Sound the depth**; the prompt "Sail out to the sea" | go on the last of the ebb: ground on the bar in the breakers; wait for the young flood to cover it: 2-3 real minutes, then the stream against you |
| 9-11 | 13:40-15:40 | the open sea, the dunes astern | swell, the first roll; the sun ahead over the port bow (heading about 245 deg, sun at 205-240 deg azimuth); the soundscape shifts to wind and water; the chart's mark becomes an ellipse of doubt that grows | **Chart:** take a departure: note the dune's bearing on the chart; **Sail:** make full sail if the wind serves | a SW wind is foul for the Thames: she beats slowly or you go for the Flemish coast instead |
| 11-15 | 15:40-19:40 | the crossing, the afternoon | nothing but sea; the sun crossing the bow to starboard through the afternoon (that swing is the heading check of 3.2); a trader's sail hull-down to the south (AI fleets Phase 2); clouds | **Encounter:** hail her for the weather ahead and prices at Lundenwic (Phase 3); **Crew:** a man's story, a rune stick, a game of dice "only while she runs under sail" (map play B2) | you hail and she is wary (a raid nearby); you spend 3 minutes at the table and miss a wind shift |
| 15-17 | 19:40-21:40 | the crossing, sunset | the wind backs at sunset; cloud lowers; whitecaps; the sun sets broad on the starboard bow about 20:20; the lookout's seat gives the sign early | **Weather sign:** shorten sail now, or run on; **Sail:** reef | full sail into a squall: the mast strains, a leak starts (storms plan S6) |
| 17-23 | 21:40-03:40 | the night at sea | a long twilight, never fully dark at 52 N in June (1.3); the bright stars and Polaris; the stern lantern; the watch; the tide turns twice unseen beneath her | **Sail** (mandatory, about minute 19-20): the wind veers and eases after the squall: make sail again, or stand on reefed; **Chart** (optional): a Polaris sight resets the heading; **Crew** (optional): the table, a story, the board | the watch dozes with full sail in a rising wind; a wrong heading all night puts the landfall 30 km out |
| 23-25 | 03:40-05:40 | landfall at dawn | the chalk of Thanet low on the bow; the Roman fort's walls at Reculver with the minster inside, low and square on the shore (the twin towers are late 12th century, [Reculver](https://en.wikipedia.org/wiki/Reculver): not in this game); the Wantsum's mouth; gulls again | **Pilotage:** which is it? the lookout's call is sure within sight of a known landmark; **Chart:** the doubt ellipse collapses to a line, then a point | mistaking the North Foreland for Reculver puts you on the sands |
| 25-27 | 05:40-07:40 | the Thames mouth | the prompt: "Sail up the Thames to Lundenwic" or "Put in at the Wantsum"; brown water; birds standing on the flats; the Isle of Sheppey | **Tide:** the chart's tide note gives the flood from about 06:00; go now and ride it, or anchor and wait (a wrong call costs up to 6 real minutes) | the ebb catches you in the narrows: anchor or go backwards |
| 27-29 | 07:40-09:40 | up the river on the flood | the river narrowing; marsh, a minster's boat; the current carries her at a walk; the morning sun astern | **Sail:** sail or row with the flood; **Crew:** the trader seat says what he has heard of Lundenwic's prices; **Chart:** compare with the note from the hailed ship | the king's men at Lundenwic's toll (patrols plan: lower strictness than Dorestad) |
| 29-30 | 09:40-10:40 | Lundenwic | the Roman walls of Londinium ahead, empty; the beach market along the Strand to the west; boats drawn up; smoke | **Business:** run her up on the foreshore; the port menu; the chart opens on the town's business (1 Oct flow) | you arrive on a falling tide and she dries out till the evening flood: sell from the beach, or wait |

Count: 14 beats in 30 real minutes, 8 mandatory, 6 optional. Density 1 per 2.1 real minutes; the longest mandatory gap
is 4 real minutes, on the crossing (minutes 11-15, the afternoon), with the night's gaps at 2.5 and 3. Those gaps are
where the ship mode earns its place. The voyage's natural timers, stated correctly: two to three tides, one night, and an
arrival in daylight unless the player casts off in the afternoon, in which case the landfall is in the dark and the
Reculver mistake is the likelier one.

### 3.2 Pilotage by landmarks, sun and stars

- **The lookout sees first.** Landmarks are rows in each level's `landmarks_<level>.json` (emitted by the level's builder
  from its data, never hand-placed in the scene: section 4.3) with a name, a real identity, a height, from/to years, and
  a sight range by height (the horizon formula from AI fleets Phase 2: 3.86 * (sqrt(eye) + sqrt(target)) km). When one
  comes in range and visibility allows, the lookout seat raises a pilotage beat: the name, the hand it lies on, and the
  choice.
- **The chart's doubt, physically.** Within sight of a known landmark the ship's mark on the chart is a point. Offshore
  the belief is an **ellipse**, because the two errors are different things:
  - *Along-track* error grows with distance run, from not knowing her speed through the water and the set of the tide:
    a = k_a * run, k_a about 0.1 at steersman skill 3 (guess), doubled in fog and at night. Nothing at sea shrinks it but
    a landfall.
  - *Across-track* error grows with the heading error: b = run * tan(e), where e drifts from the last sight at about 2
    degrees per game hour unseen (guess) and is reset by a sight of the sun (its real azimuth from SkyMath, so a noon sun
    is south) or of Polaris at night (the true sky from the star cube) to the sight's own accuracy, 3-5 degrees by eye
    (guess). A celestial body is at infinity: its bearing fixes **which way is north, not where the ship is**, so a sight
    stops the ellipse widening across-track and does not shrink it. Polaris's altitude by hand-widths (1-2 degrees, 110-220
    km) is useless on a 300 km crossing, so there is no latitude play.
  - A bearing to a **landmark** is a position line: it collapses the ellipse to a line. A second landmark, or a sounding
    matched to the depths the house knows (`Known`), collapses it to a point.
  The player takes a sight by dragging the compass rose to a sighted body or landmark on the chart. This is a game tool
  and says so; the period's seamen did the same in their heads, and it is what Ohthere's text supports: heading by sun and
  land, position by landfall.
- **The payoff.** Landfall on the wrong headland is the voyage's classic mistake and it is survivable: the chart was
  wrong, not the world. Learning the landmarks of a coast is real progression (section 2.5).

### 3.3 Sail handling and orders

The GDD's order table stands (make sail, shorten, strike; row all, half, hold water; make for there; sound; anchor; heave
to; take the ground; hail; lighten). In play they are one panel with one key each, available in all three modes; each
shows its cost line as a fact ("The men will tire. 4 oars manned."). The wind arrow on the chart and the sail's set in 3D
are the only feedback; there is no speed number aboard, only "she goes well" in the status line and the wake. The DevHud
keeps the numbers, and in the first slice the DevHud *is* the panel (section 5); the glass panel comes with M3.

### 3.4 Watching the weather

Natural signs only (2 Oct). In play the signs are things the renderer already makes (cloud base and darkness, wind on the
water, swell length, rain curtains and the shelf cloud in the storms plan S5) plus the lookout seat's line, which is a
placeholder intention ("He thinks it will blow by evening.") whose accuracy is his Eyes skill. A pressure reading exists only
in the DevHud.

### 3.5 The crew aboard

The crew plan and the GDD's council stand. For pacing, the ship mode's asks are **optional beats** gated by P (section 2.2):
a story, a game, a reading, a quarrel to settle. A reading (GDD "reading with your crew") is the longest optional beat, 3-8
real minutes, so it is offered on the open sea and in the night watches, never in a river place. Quarrels and theft are
mandatory beats: the world insists.

### 3.6 Encounters, the chart, the three modes, nights

- **Encounters** are as built in the first build (Phases 2-5): sighting, made out, the panel of options, the chase with
  live facts. In the rebuild they cut to an event shot and the clock holds only alongside (GDD).
- **The chart** is the table aboard (map play): the clock runs; alerts call you back; one click takes you there.
- **The three modes** share the clock; the voyage mode is the one with nothing to do, by design ("vibe out", the user's
  words), and the pressure meter keeps it quiet: at low P the camera takes long shots and the sound is the water. Nothing
  pops a panel on the open sea that the world did not raise.
- **Nights** play on the clock. On a crossing you sail on with a watch; coasting you anchor or run her up (GDD). "Rest
  till dawn" passes the night in seconds. A June night at sea is about 7 real minutes, and at 52 N it is a long twilight
  with about 3 real minutes darker than nautical twilight and no true night (1.3): the bright stars and Polaris show, the
  faint ones do not; a December night is 16 real minutes with 12 of true dark. The sightings cap at night keys on the sun's
  altitude (-6 and -12 degrees, which SkyMath gives), not on a clock hour, so the same rule is right in both seasons.

### 3.7 In port, and an evening

The user's brief names "NPC interactivity through text based adventure" and the GDD's loops include "time in port" and
"a season (evenings)". The port is where half the fun tests live (M3, M6, M8), so it has its own beat kind and density.

**The port loop.** Mooring opens the port menu (1 Oct flow): the market, the host, the toll house, the hiring place, the
minster or the hall, the count's man at Dorestad, "explore the town" to the chart. Each of those is a person, and a person
is a **Port beat**: an ask, a commission, news, a hire, a judgement. The raisers are PortLife (the people the port has by
its kind and size) and GameEvents, which fires the text events of events_793.json on their dates and conditions (a raid
heard of, a famine, a fair, a death). The words are the user's; the system gives each beat its intention, its options, its
cost line and its outcome, as the voyage's beats do. The design of the text encounters themselves is
Design/npc_text_encounters.md; this document sets their pacing:

| Measure | Target per port call (guess) |
|---|---|
| Port beats offered | 3-5 |
| Mandatory | the toll (at a toll station) and the count's man on his date; nothing else |
| Longest beat | a reading or a riddle with the minster, 3-8 real minutes |
| Time in port, typical | 2-6 real minutes (2-6 game hours), the market open from dawn to dusk only |

A port call that offers nothing is a composition fault logged like a quiet gap; a port that offers more than five is a
queue, and the rest keep till the next call.

**An evening.** A session is 2-3 voyages or one crossing, and it should end well: at a mooring, with the ledger closed
(the voyage's accounts shown, the debt's date and the standing lines updated), and the chart showing what the next evening
could be. The telemetry marks the session's end and whether it fell at a mooring. The "one more voyage" test (appendix)
measures the pull; the ledger's closing line measures whether the session had a shape.

### 3.8 The first hour: the crew teach, there is no tutorial

Rules: nothing is taught before it matters; a lesson is a crewman's suggestion with an intention line (placeholder) and a
one-key accept; ignoring it is allowed and has the natural consequence; each lesson fires once per house (saved), and a
house that has learned a thing never hears it again. Ravn is the first teacher (26 Sep); after him the seats teach their own
part. Times assume Dorestad at 6-8 minutes (section 6, question 1); at today's 1.6 m/s and exit, the river rows double.

| Real minute | What happens | Who teaches | What the player learns |
|---|---|---|---|
| 0-2 | The title, then Dorestad at dawn. The count's man at the dam head: the debt and its date (intention + outcome, slots). | the count's man | why you sail; the date on the chart |
| 2-4 | Ravn: "make for the river's bend" (click the water). She goes. | Ravn | make for there; she stops short of shoals on her own |
| 4-6 | A barge comes up. The mate: keep to your side. Accept or not. | the mate | the seamanship rule; the consequence if ignored (a scrape, a word from the barge) |
| 6-10 | The sail: the wind is fair down the Kromme Rijn. Ravn: make sail. | Ravn | the orders panel; the cost line |
| 10-14 | Traiectum: moor; the port menu; "explore the town" zooms to the chart (1 Oct flow). The market: buy what is cheap here (a price note from your own eyes, dated today). The first port beat: the host has news. | the trader seat (Ravn for now) | buying; the ledger; "only what you know" |
| 14-20 | Back down the river. The first optional beat: Ravn offers a story at the table (ship mode). The chest: the first rune stick, which Ravn reads in part. | Ravn | the ship mode; the chest; reading with the crew |
| 20-26 | Dorestad: sell. The price has moved a little (the AI traders). The count's date is nearer. The hiring place: a man looking for a ship. | the host, the count's man | selling; news; hiring |
| 26-40 | A second delta run, chosen from the chart: the Old Rhine mouth and back, or up to Traiectum again. The tide at Katwijk for the first time. | the lookout, the steersman | the tide beat; sounding; the prompt at a river mouth |
| 40-60 | The crossing is offered: Lundenwic, or Quentovic, or Witla. The player chooses; the lookout's and the trader's seats give their facts. The hour ends with her at sea, the dunes astern, the doubt ellipse growing. | the seats | the voyage loop; that the sea is wide and the chart is theirs |

By the end of the hour, as the GDD asks: two or three delta voyages, a ledger filling, the first riddle in the chest. The
fun tests (appendix, M1-M3) check it.

### 3.9 Risk and reward, as the player feels it

- **Storms** (storms plan S6): the stage with the facts and the choices; minute by minute water aboard, bailing, the men
  tiring. Survivable by seamanship within the ship's class (2.4); the recorded storms are announced by a day of signs,
  and the crew talk of nothing else.
- **Raids:** the chase with live numbers; the fort, the shoal water, the night. Rare in 793 on this coast, by the data.
- **Sandbanks:** the grounding choices on real depths (GDD, `Aground`), with time running fast while you wait.
- **Tolls:** taken at cast-off and at the river tolls; a charter in the chest frees you; standing lowers them.
- **Debt:** the one clock the player cannot stop; its date on the chart; paying it is the first win of the game. On the
  hard terms of 2.8 it is 24 crossings in 100 game days (40 real hours), with a slack of 1.4: a missed tide or a storm week
  is felt.

### 3.10 Progression, as the player sees it

No numbers on the player; the ship, the men, the chart's cleared fog, the known channels drawn on the chart, the chest's
contents, the standing lines on port cards, the house's annal. A bigger ship is the visible rung; a man who reads Latin is
the invisible one.

### 3.11 Replayability, as the player meets it

A new game asks three things: the era, the count's terms, and the name and mark. Everything else is the seed. The annal of
the last house (slots for the user's text) sits on the title screen; the chronicle's full book is a find.

### 3.12 The Build mode

The user dresses the world in the in-game Build mode (29 Sep decisions). For play it has one rule: dressing never changes
the water, the depths, the landmarks' identities or the berths; those are the level's data and the checker test guards
them. A landmark can be re-dressed, not moved. The Build mode is also a replay lever: the dressing is per level and
shareable, so a level can be dressed again without touching the game.

### 3.13 What to cut for the first playable

Keep: orders, beats, the tide, landmarks and the doubt ellipse, the trader hail, the chest with one reading, the count's
debt, the port menu, save and load.

Cut until after M4: the full council (one seat, the lookout, at first), board games, raids (M5), storms (M7), standing
beyond the port card, quiet years, era starts, heirs, multiplayer, agents, factors, rumours, verse-making, the law case.

Rule: a cut feature leaves its data table in place and a hook that logs "not built" to the voyage log, so the tests for the
beat density run on the real route from the start.

---

## 4. Build plan for Unity

**Precondition (step 0 of the first slice).** The brought-over game layer in `Assets/Kapman/Code/Game` is untracked in
git and has a `Kapman.Game.asmdef` (references Kapman.Core, Kapman.Runtime, InputSystem, HDRP, Mathematics) and a `Shims`
folder (`LevelFrame`, `SailingShip`, `Weather/`), but five files (`KapmanGame`, `Commissions`, `LocalBoats`, `SeaWatch`,
`WorldSim`) still reference a `GameClock` that has no definition in the project. Before the slice starts: `GameClock`
replaced by `LevelClock` or shimmed, the project compiling, the three existing EditMode tests (SkyTests, TideModelTests,
WorldFrameTests) green under the headless command. The test assembly `Kapman.Tests.EditMode` today references only
`Kapman.Core`; it gains `Kapman.Sailing` and `Kapman.Game`.

### 4.1 Where it hooks into what exists (Assets/Kapman/Code)

| Existing | Used for |
|---|---|
| `LevelClock` (TimeFactor, GameHours, Date, JulianDay, Night) | the beat tick (once per game minute), the debt date, nights |
| `LevelWeather` (Wind, WindVector, Storm, FogDensity, Rain) | sail beats, weather signs, visibility for sightings and the doubt ellipse; wrapped by the `IWindState` adapter |
| `LevelWater` / `TideModel` | tide beats and windows (where `tideShare` > 0: not at Dorestad); wrapped by `IWaterState` |
| `ShipMotion` (MakeFor, Speed, Velocity, Status, Player, Ground) | orders; the helm; reads `SpeedModel` in place of the constant `LevelInfo.shipSpeed`; wrapped by `IShipState` |
| `LevelInfo` (realMetresPerUnit, kind, shipSpeed) | mapping progress to the chart; ocean vs river beat rules |
| `LevelExit` / `LevelEntry` / `LevelTravel` (to become `JoinTrigger` / `JoinAnchor`, gateways doc) | phase events into the voyage log; the prompt is already a beat |
| `DorestadBuilder` / `OceanBuilder` (Editor) | emit `landmarks_<level>.json` from the plan (the fort, the ford, the spit, the dam heads, the weir, the bend; the dunes and the bar) when the level is built |
| `Game/Sim/Travel.Speed(ShipClassDef, courseDeg, windFrom, windSpeed, out sheltering)` | the polar, the wind angle and beating: `SpeedModel` wraps it, it does not re-derive it |
| `Game/Voyage/Voyage` (Men, Health, Morale, WaterL, `Affect`, `Rest`, `AddWater`, `Day`) | the men's state; `OrderBook` applies rowing fatigue through `Voyage.Affect` and rest through `Voyage.Rest`; the leak term of P from `WaterL` |
| `Game/Voyage/TideWait` (`VoyageTide`: Known, Level, Rising) | the tide beat's facts and the wait |
| `Game/Voyage/Aground` (Stage, Offer, Options, TideWillFloat, FindDeepWater) | the grounding beat, as built |
| `Game/Known/Known` (facts, news, sightings, standing) | the chart's known depths and tide notes for the position fix; the stale-price hazard |
| `Game/Sim/Seamanship` | the rules of the road at the weir and the barge (the first hour's lesson) |
| `Game/Events` (GameEvents, events_793.json) and `Game/Ports` | the Port beats of 3.7 |
| `Birds` | a weather sign (gulls go inland) if a hook is added |
| `DevHud` | the orders panel, the beat panel and the pressure meter in the dev build; the first slice's only UI |
| `SkyMath` / `SkyDome` | sun and star azimuths for the heading reset |
| `SimRandom` | all beat and hazard rolls, seeded per voyage |

### 4.2 Classes

Assembly `Kapman.Sailing` (new, plain C#, no UnityEngine scene types, so EditMode tests can drive it):

- Interfaces the rest takes, so a test can script the world: `IWaterState { float Tide; float TideRate; }`,
  `IWindState { Vector2 WindVector; float Visibility; float SunAltitude; }`, `IShipState { Vector3 Position; float
  Heading; float Speed; }`, `ILandmarks { IReadOnlyList<Landmark> All; }`. In play, `Runtime` adapters wrap `LevelWater`,
  `LevelWeather`, `ShipMotion` and the loaded landmark file; in tests a JSON script supplies them.
- `Orders` (enum: MakeSail, Shorten, Strike, RowAll, RowHalf, HoldWater, Sound, Anchor, Weigh, HeaveTo, TakeGround, Hail,
  Lighten) and `OrderBook` (the current `SailState`, the costs, `Apply(order, crew, wind)` returning a fact line and the
  state delta; its fatigue and rest costs go through `Voyage.Affect` and `Voyage.Rest`).
- `SailState` (sail 0/half/full, oars manned, anchored, hove to) and `SpeedModel`: a thin wrapper that calls
  `Travel.Speed` for the polar speed, multiplies it by the sail state (full 1, half about 0.5, struck 0), adds the oars
  (`oarsKn` when the sail gives less), and subtracts or adds the current; `ShipMotion` reads it.
- `Landmark` (data row: id, name, position, height, identity, fromYear, toYear, the hand along each route) loaded from
  `landmarks_<level>.json`, and `LandmarkSight` (per game minute, which landmarks are in range by the horizon formula and
  visibility; raises pilotage beats with the lead time of 2.1).
- `PositionBelief` (the ellipse of 3.2: `Run(distance, skill, fog, night)`, `CelestialSight(azimuthError)` resets the
  heading error, `LandmarkBearing(landmark)` collapses to a line, a second bearing or `Sounding(depth, known)` to a point).

Assembly `Kapman.Game` (voyage part, beside the brought-over code):

- `Beat` (data: kind, raiser, question, options with cost lines, lead time in real minutes, mandatory, lesson id) and
  `BeatScheduler` (one tick per game minute; collects candidate beats from the raisers through the interfaces; holds
  optional ones while `Pressure.P > 0.5`; writes each beat to the log). It has **no gap filler**: the gap rule lives in the
  log's report.
- `Pressure` (the P formula, weights from data).
- `Hazards` (hazards_793.json loaded; `Roll(hazard, levers, year, region)`; the raiders row multiplies raiders_793.json's
  pressure; the sign/lead/counter invariant).
- `StormSurvival` (p_founder per hour from section 2.4 with the class's limit and freeboard from ship_classes_793.json;
  used by the storm stage in S6; Monte Carlo in tests).
- `Lessons` (lessons_793.json; `Offer(id)` once per house; saved).
- `DebtClock` (the count's terms from debt_terms.json; the date; outcomes; the slack check).
- `PortBeats` (the Port beat kind of 3.7: asks PortLife and GameEvents for the call's people and events, paces them 3-5 a
  call, queues the rest).
- `VoyageLog` (every beat, order, phase, fact line and outcome with the game time and the real time; saved; exported as
  CSV for the playtest; its report flags gaps over 4 real minutes and port calls with no beat).
- `FunTelemetry` (P over time, beat gaps, port beats per call, orders per minute, time in each mode, losses and their
  attributed sign, the session's end and whether it fell at a mooring; one CSV per session in
  `G:\kapman_unity\data\work\playtests\<date>\`).

Presentation: in the first slice the beats, the orders and the pressure draw in `DevHud` (IMGUI, the existing prompt box
with number keys for options). The glass-style UI Toolkit panels (`OrdersPanel`, `BeatPanel`, `ChartFixTool`) come with
M3's port menu; the project has no UI Toolkit setup today and that is a night of its own.

### 4.3 Data files (Assets/Kapman/Data, JSON, every number tunable, each with a `units` field)

| File | Holds |
|---|---|
| `beats_793.json` | beat kinds, their raisers, lead times (`units: real_minutes`), mandatory flag, the gap report threshold (`gap_real_minutes: 4`), the P threshold |
| `pressure.json` | the weights of P |
| `hazards_793.json` | each hazard: base probabilities careless/careful, levers with multipliers, sign, lead time, counters, loss; the raiders row as a multiplier on raiders_793.json |
| `storm_survival.json` | base curve, f_sail, f_heading, f_load, f_hull, f_crew; the limits are read per class from ship_classes_793.json |
| `lessons_793.json` | lesson id, trigger (first time a beat kind or order matters), teacher seat, intention slot, outcome slot |
| `landmarks_<level>.json` | per level, **written by the level's builder** from its plan: id, name, position, height, identity, `from` and `to` years, the hand along each route. Hand-placed scene objects are not used: the builders start from an empty scene and would wipe them |
| `progression_793.json` | the ship ladder by class id: cargo, crew, price, which ports sell her |
| `debt_terms.json` | the count's terms per difficulty: sum, date, consequences, and the derived columns of 2.8 (voyages needed, days needed, slack band) |
| `port_beats_793.json` | the port beat density targets and which people raise what, by port kind |

### 4.4 Tests (EditMode, headless, `Unity.exe -batchmode -nographics -runTests -testPlatform EditMode`)

All of these are pure C# through the interfaces of 4.2: no scene, no HDRP volume, no `tides.bin`. The route tests drive a
scripted ship along `landmarks_<level>.json` with a tide and wind script in JSON.

| Test | Asserts |
|---|---|
| `BeatDensity_DorestadToExit` (first slice) | the scripted Dorestad route (the mooring to the "Sail down the Rhine" prompt, with the landmark file and a wind script) yields 4-8 beats and no gap over 4 real minutes (240 game minutes) at 1x |
| `BeatDensity_DorestadLundenwic` (M2+) | the scripted 30-minute chain yields 10-18 beats, no gap over 4 real minutes at 1x, about half optional |
| `Pressure_HoldsOptionalBeats` | with P = 0.8 no optional beat is offered; with P = 0.2 the held beats are offered in order; nothing is offered that no raiser raised |
| `Hazards_EveryRowHasSignLeadCounter` | the data invariant |
| `Hazards_LeverRule` | for every hazard, careful/careless probability ratio is at least 3 |
| `Hazards_RaidersFollowData` | the raiders hazard for the southern North Sea is under 0.02 in 793 and over 0.05 in 810 and 834, from raiders_793.json |
| `Landmarks_PeriodTrue` | every landmark's `from` year is not after the level's year and its `to` year not before it (Reculver's towers would fail it) |
| `StormSurvival_Careful` | Monte Carlo 10,000 runs per class for `gislinge` (force 7-8), `north_sea_merchant_ship` and `norse_allround_ship` (force 9-10), 6 hours, reefed and head to sea: loss under 5% |
| `StormSurvival_Careless` | the same classes, full sail, beam-on, overloaded: loss over 50%; and the Gislinge at sea in force 9-10 over 50% however handled |
| `StormSurvival_Recorded` | the recorded storm's base in the open sea: loss over 80% for every class; in harbour 0 |
| `PositionBelief_Grows_And_Fixes` | both axes grow with run, faster in fog and at night; a celestial sight stops across-track growth and does not shrink either axis; a landmark bearing narrows to a line; a second bearing, or a sounding matched to known depths, collapses to a point |
| `Speed_SailState` | `SpeedModel` on the Gislinge polar at 8 m/s beam wind gives 4-6 kn full, about half at half sail, 3.5 kn all oars in a calm, and equals `Travel.Speed` at full sail with no oars |
| `Lessons_FireOnce` | each lesson offers once per house across save and load |
| `DebtClock_Dates` | each term's date lands on the right Julian day; the outcomes apply once; each term's slack, from the economy data's margins, is inside its band (hard 1.3-1.6) |
| `PortBeats_Density` | a scripted port call offers 3-5 beats, only the toll and the count's man mandatory, the rest queued |
| `VoyageLog_RoundTrip` | a log saved and loaded is equal; the CSV has one row per beat; the report flags a scripted 5-minute gap |
| `QuietYear_NotBetter` | (M8) a quiet year's income never exceeds the same year's sim result; a light-fingered factor loses |

### 4.5 Performance budget (RTX 4070, 1440p, 60 fps: 16.7 ms a frame)

All of this is plain C# ticking once per game minute (once a real second at 1x, four times at 4x): a budget of 0.2 ms per
tick and zero allocations per frame. Landmark sight checks: at most 64 landmarks per level, one distance and one visibility
test each per tick. The DevHud redraws its IMGUI boxes only while a beat is open; the later UI Toolkit panels redraw only
on change. Telemetry writes to a buffer and flushes to disk on phase and on save. Nothing here touches the render budget,
which belongs to the water, sky and weather documents.

---

## 5. First slice (one night)

**"A voyage with beats, the dam head to the Rhine prompt."** On the existing `L_Dorestad` only. Dorestad has no tide
(`tideShare = 0` in DorestadBuilder) and the Rhine mouth's bar exists only as a 1:150 blockout in `L_NorthSea`, so the
tide beat and Pressure are **not** in this slice; they come the night after, on `L_NorthSea`, once the Rhine-mouth river
place exists.

0. The game layer compiles (the precondition of section 4): `GameClock` resolved, the three existing EditMode tests
   green, the test asmdef referencing `Kapman.Sailing` and `Kapman.Game`.
1. `Orders` and `OrderBook` with four orders live: make sail, shorten, row all, anchor; `SpeedModel` wrapping
   `Travel.Speed` and replacing the constant in `ShipMotion` (the sail state and oars change her speed; the DevHud shows
   it).
2. `DorestadBuilder` emits `landmarks_dorestad.json` from the plan: the fort, the ford, the fork's spit, the dam heads,
   the weir, the bend; `LandmarkSight` raising pilotage beats through the `ILandmarks` and `IShipState` adapters.
3. `BeatScheduler` with pilotage, sail (from the `IWindState` adapter over `LevelWeather.WindVector` shifts) and the
   existing `LevelExit` prompt as beats; `VoyageLog` to CSV; the DevHud draws each beat as the existing prompt box with
   number keys.
4. Two lessons as placeholder intentions: make for there (Ravn), keep to your side (the mate).

**Done when:** `BeatDensity_DorestadToExit` passes on the pure-C# scripted route; `Speed_SailState` and `Lessons_FireOnce`
pass; the headless test command runs green; and a play from the dam head to the "Sail down the Rhine to the sea" prompt
produces a voyage log the user can read back, with every beat's game time, real time, question and choice. The log's gap
report on that play is the first measurement of question 1 below.

---

## 6. Open questions for the user

1. **How long Dorestad is.** Measured on the built level, the mooring to the exit is about 1,800 m of channel: 19 real
   minutes at today's 1.6 m/s, 15 with the stream. The GDD wants a river place at 1-5 minutes. Three ways: raise
   `shipSpeed` in river places to 3-4 m/s on screen (she is already at 1/60 of her real pace in game time, so this is not a
   cheat), move the mooring to the downstream end of the harbour comb, or cut the level with a phase at the first big bend
   below the dams (the gateway document's proposal, about 11 minutes at 2 m/s). The 30-minute voyage depends on this
   more than on the ocean's ratio. Which?
2. **The simulation's distances.** The sim's traders take 45-90 game hours for a crossing the player makes in 30. Either
   the sim sails the levels' shortened network (the player and the traders at one pace; the chart's "days" become a
   looser word), or the player's open sea is tuned to about 45 game hours (45 real minutes) and the sim stays on real
   distances. I recommend the first.
3. **The doubt ellipse.** The chart's ship mark becoming an ellipse offshore, with sun and Polaris sights fixing the
   heading and landmarks fixing the place, is a game tool that stands in for dead reckoning in the steersman's head. Keep
   it, or keep the mark exact and lose the "where are we" play?
4. **Difficulty as the count's terms** only, no sliders, with the terms of 2.8 (easy 480 d by Michaelmas, hard 20 pounds
   by Michaelmas, a slack of 1.4). Yes?
5. **Quiet years.** A year passed from the chart with the house's trades settled by the sim and an annal with slots for
   your text: in, from M8? Or only era starts?

---

## Appendix: fun tests per milestone

Each is asked of a tester (the user, then strangers) after a session, with the telemetry beside it. A milestone is not done
until its tests pass; "pass" is the bar in the last column (guess, to firm up after the first three sessions).

| Milestone | Questions | Measured by | Pass |
|---|---|---|---|
| M0 look test | Did you want to keep looking at it for five minutes with nothing to do? Which of the takes would you show a friend? | the user's pick; time before quitting the look scene | the user picks one without hedging |
| M1 Dorestad | Could you find the way out of the harbour without being told? Name one thing you saw that told you where you were. Did she stop where you expected? | first make-for to the exit prompt: time and clicks; "She can't float there" count | out in under 8 real minutes (4 once question 1 is settled); under 5 refused clicks |
| M2 down the Rhine to the sea | How many decisions do you remember from the run? Which one cost you something? Did the phase at the bend break the spell? | beats offered vs chosen; the gap report; the tester lists them unprompted | 3 or more remembered; no gap over 4 real minutes; the phase unnoticed by 2 of 3 testers |
| M3 ports and trade | Why did you buy what you bought? Did a stale price cost you, and did you know why? What is the count's date? Who did you talk to in port? | ledger notes used per deal; losses on notes over 30 days; the tester states the date; port beats taken | the reason is a note, not a guess; the date known; one port beat named |
| M4 tides and seamanship | Did you wait for a tide on purpose? Did you ground, and was it your fault? Did you use an order you had not used before? | tide beats accepted; groundings with their sign read or not; distinct orders used | 1 deliberate wait; every grounding traced to a sign in the log; 4 or more orders used |
| M5 the living North Sea | Did you change course because of a sail or a sky? Did a hail tell you something you used? Did you feel alone out there, and was that good? | course changes after sightings; price notes from hails used in deals; P trace | 1 course change; 1 hail used; P under 0.3 for at least 5 real minutes with no quit |
| M6 the ship and its crew | Name two men aboard and one thing about each. Did one of them help you read something? Did you want to settle a quarrel, or let it run? | readings done; asks accepted; quarrels resolved vs ignored | two names with a trait each; one reading finished |
| M7 storms | Did you see it coming? What did you do? Was the loss, if any, your fault? Would you sail that day again? | lead time between the first sign and the first order; losses by careful/careless and by class; the tester's attribution | 2 of 3 testers act before the stage opens; losses attributed to self 3 of 4 times |
| M8 history and heirs | Did an event in the chronicle change your plan? Did the house's annal read like your game? Would you start again in another era? | plan changes after news; annal slots filled; the era chosen on restart | 1 plan change; the tester restarts without being asked |
| Every milestone | The one-more-voyage test: did you cast off again when you meant to stop? The story test: tell the session in one minute. The blame test: was each loss yours? The evening test: did the session end at a mooring with the ledger closed? | session length vs intended; the story recorded; losses with signs read; the session's end in the telemetry | sessions run over by 20% or more; the story names a place, a man and a decision; 3 of 4 losses self-blamed; 3 of 4 sessions end at a mooring |

The telemetry (`FunTelemetry`) is the same CSV in every milestone, so the numbers compare across the build: beats per
real minute, the longest gap, port beats per call, orders used, P over time, time in each mode, losses with their sign and
lead time, the session's end.
