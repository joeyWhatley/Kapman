# Ship physics and seamanship

Kāpman 2 design document, 4 October 2026. Scope: how the player's ship (and the AI ships near her) float, sail, row, heel, leak, broach, ground, anchor and are towed, as a real rigid body on the HDRP water, and what the player orders. It puts a body that is pushed by water, wind, oars and rope *underneath* the kinematic `ShipMotion` (which today slides a point along at a cruise speed and tilts her by five sampled heights): `ShipMotion` stays on the player's ship as the facade that the rest of the level reads (`ShipMotion.Player`, `.Velocity`, `.Speed`, `.Pitch`, `.Roll`, `.Status`, `.MakeFor`, the static `Ground`), and stops integrating when a `ShipBody` drives it (4).

How this was verified: pages were opened on 4 October 2026 by direct fetch (the Viking Ship Museum's site, Wikipedia, the Met Office Beaufort page, Unity's scripting reference and the Engineering Toolbox answered; search engines and the game wikis did not answer this session, and the English Wikipedia pages for the Graveney boat and the Klåstad ship could not be reached, so the Norwegian one was used for Klåstad). Facts that come only from the project's own earlier notes (PROGRESS.md, the first build's `ship_classes_793.json`, `Plans/environment_research/swell_storm_seas.md`) are marked *(project notes)*; numbers I derived are marked *(derived)*; numbers with no source behind them are marked *(guess)*. Nothing here needs any non-shippable data: it is all physics and public measurements.

Standing design decisions this document obeys (from the GDD and the user's answers): no arcade steering, the player gives orders and the crew carries them out; 1 real second = 1 game minute for the clock, but the ship moves at her true speed through real-size water; a wreck is game over in single player, and the game can be saved at any time; river places are 1:1; ocean levels are shortened hard across the whole level (`LevelInfo.realMetresPerUnit`, 150 on `L_NorthSea`: 492 km real is 3.3 km in the game, *project notes: PROGRESS.md*) but the ship and the waves on her stay real size (2.12 says what that does to leeway and drift); storms are to look and act 1:1.

---

## 1. What is true

### 1.1 The ships

| Ship | Date | Length x beam x draught (m) | Displacement / cargo | Sail | Oars / crew | Source |
|---|---|---|---|---|---|---|
| **Dorestat 1** (Dorestad's own find): clinker, iron-riveted | wood felled after 739; replica sailed 2021 | not in the sources opened this session | - | - | - | *(guess, source to find: carried from PROGRESS.md with no citation behind it; see the task in 5)* |
| **Vleuten 1** (Utrecht-type expanded logboat, no iron) | 734 | 15 x 1.5 | - | towing mast or none | paddles/poles | *(guess, source to find: carried from PROGRESS.md with no citation behind it)* |
| **Utrecht 1** (the type's best-known hull) | dendro 997-1027 | 17.8 x 3.8; draught 0.7 laden | ca 13 t cargo | small mast step near the bow (towing) | - | nl.wikipedia "Utrechtse schip" https://nl.wikipedia.org/wiki/Utrechtse_schip ; the first build's table cites MASS for 17.45 x 3.84, 10-15 t *(project notes)* |
| **Eke boat** (Bremen-Seehausen, Weser): double logboat widened with frames | 8th c. | 10.25 x 1.4-1.5 | - | - | - | *(project notes, citing "Medieval boat and ship finds of Germany...")* |
| **Gislinge boat** (the stand-in in the game now; open drawings free of charge) | c. 1130 | 7.7 x 1.5 (beam from the museum's plans, *project notes*) | about 1 t ("four men and ten sheep", *project notes*) | small square sail (reconstructed) | 6 oars / 2-5 men | https://www.vikingeskibsmuseet.dk/en/professions/boatyard/the-gislinge-boat-open-source-project |
| **Skuldelev 1** (knarr; reconstruction *Ottar*, 2000): the cargo type | c. 1030, Sognefjord pine | 15.84 x 4.8 x 1.0 | 20 t displacement | 90 m2 | 2-4 oars / 6-8 men | https://en.wikipedia.org/wiki/Skuldelev_ships ; https://www.vikingeskibsmuseet.dk/en/visit-the-museum/exhibitions/the-five-viking-ships/skuldelev-1 |
| **Skuldelev 3** (coaster; reconstruction *Roar Ege*, 1984) | c. 1040, oak | 14 x 3.3 x 0.9 | 9.6 t displacement, 4.6 t cargo | 45 m2 | 5 oar ports / 5-8 men | https://en.wikipedia.org/wiki/Skuldelev_ships ; https://www.vikingeskibsmuseet.dk/en/visit-the-museum/exhibitions/the-five-viking-ships/skuldelev-3 |
| **Skuldelev 2** (longship; *Sea Stallion from Glendalough*, 2004) | c. 1042 | 30 long | - | 112 m2 flax, 8,000 rivets, 600 l tar | 60 rowers | https://www.vikingeskibsmuseet.dk/en/visit-the-museum/exhibitions/the-five-reconstructions/the-sea-stallion-from-glendalough-skuldelev-2 |
| **Oseberg** (*Saga Oseberg*, 2012) | 820 | 21.58 x 5.10 | - | ~90 m2, mast 9-10 m | 30 oars | https://en.wikipedia.org/wiki/Oseberg_Ship |
| **Gokstad** (*Viking*, 1893; *Gaia*, 1990) | c. 890 | 23.80 x 5.10 | - | ~110 m2 (Gaia 120) | 32 oars | https://en.wikipedia.org/wiki/Gokstad_ship |
| **Graveney boat** (Kent; the English coaster type) | c. 895 | 13.6 x ~4 | 6-7 t cargo | - | - | *(project notes, citing MASS; its Wikipedia page could not be opened this session, so the date is unverified on both sides)* |
| **Äskekärr ship** (Göta älv, Sweden): the earliest knarr-type find | c. 930 | - | - | - | - | https://en.wikipedia.org/wiki/Viking_ships ("believed to be from about 930") |
| **Klåstad ship** (Vestfold): a cargo ship, "probably a knarr", whetstone cargo | dendro 998 | keel 16.25; beam 4.5 | - | - | - | https://no.wikipedia.org/wiki/Kl%C3%A5stadskipet ("årringsdatert til 998"; "sannsynligvis et lasteskip, en knarr") |
| **Kvalsund** (the 793 Norse all-rounder) | 780-800 | 18 long; a 9.5 m rowboat beside it | - | sail uncertain | side rudder | https://en.wikipedia.org/wiki/Kvalsund_ship |
| "Knarr" in general | - | ~16 x 4.6, 50 t displacement, up to 24 t cargo; 75 miles a day; crew 20-30 (Wikipedia's generic figures) | | | | https://en.wikipedia.org/wiki/Viking_ships |

What matters for the game: the Frisian merchant of 793 sails something between the Gislinge boat (a working boat, four men) and a 14 m Graveney/Skuldelev 3-sized coaster; the Utrecht-type logboat is the river freighter (flat, no keel, towed). The knarr is the big cargo type he (or an heir) can buy later, and "later" is real: the earliest knarr-type finds are Äskekärr (c. 930) and Klåstad (998), and Skuldelev 1 is c. 1030, so the shipwright must not offer a knarr in 800. The first build's `ship_classes_793.json` already carries from/to years per class and calls the knarr "LATER"; the shipwright's offer reads those (2.1). The dates of the hulls used as stand-ins: Gislinge 1130, Skuldelev 3 c. 1040, Graveney c. 895, Oseberg 820, Kvalsund 780-800; in 793 only the Dorestat 1 and the Utrecht type are near the date, and riveted clinker hulls are attested at Dorestad, so the 14 m coaster stands in for Dorestat 1 from 793. There is no securely dated 8th-century Frisian sea-going hull *(project notes)*; the model below is parametric, so a Dorestat 1 hull can be dropped in when its measurements are to hand. The Gislinge boat is a stand-in in look only.

Derived mass budget *(derived)*, with sea water 1,025 kg/m3 and a block coefficient Cb of 0.45-0.55 for these round, fine-ended hulls:

| Ship | Hull mass (guess) | Men + gear | Cargo | Total displacement | Waterline draught at that mass |
|---|---|---|---|---|---|
| Gislinge 7.7 x 1.5 | 0.5 t | 4 x 80 kg + 0.1 t = 0.42 t | 0.9 t | 1.8 t | ~0.30 m (Cb 0.5: 0.5 x 7.3 x 1.5 x T x 1,025 = 1,800 -> T = 0.32) |
| Coaster 14 x 3.6 (Graveney/Skuldelev 3 class) | 3.5 t | 6 x 80 + 0.3 t = 0.8 t | 5 t | 9.3 t (Skuldelev 3 is 9.6 t) | ~0.75 m |
| Knarr 15.8 x 4.8 (Skuldelev 1) | 6 t | 8 x 80 + 0.5 t = 1.1 t | 13 t | 20 t (matches Wikipedia) | 1.0 m |
| Utrecht-type 17.8 x 3.8 | 5 t | 0.4 t | 13 t | 18 t | 0.7 m laden (Wikipedia) |

### 1.2 Speeds from replica trials

| Measurement | Value | Source |
|---|---|---|
| Sustained sailing speed, wind 5-10 m/s (force 3-5) from the side or abaft the beam | "about 5-6 knots can be maintained over a longer period" | Viking Ship Museum, standardised trials: https://www.vikingeskibsmuseet.dk/en/professions/boatyard/experimental-archaeological-research/sailing-trials/standardised-trials |
| Made good to windward, tacking | "at best ... 2 knots" | same page |
| Rowing a Skuldelev 5-sized ship in light wind | 3.5-4 knots | same page |
| Roskilde to Kaupang, 235 nautical miles, wind stable W-SW | "just under 50 hours" (4.7 kn average) | same page |
| Skuldelev 1 / Ottar | average 5-7 kn, top "ca 13 knots" | https://en.wikipedia.org/wiki/Skuldelev_ships |
| Skuldelev 3 / Roar Ege | average 4-5 kn, top 8-10 kn | same |
| Saga Oseberg, March 2014 open-sea trial | 10 knots under full sail | https://en.wikipedia.org/wiki/Oseberg_Ship |
| Gokstad replica *Viking*, Atlantic 1893 | "around 10 or 11 knots"; hull flexed 18 mm, gunwale twisted 15 cm, did not leak | https://en.wikipedia.org/wiki/Gokstad_ship |
| Hodding Carter's knarr, Greenland-Newfoundland 1998 | "could not sail to windward" until the side rudder's mounting and balance were rebuilt | https://en.wikipedia.org/wiki/Knarr |
| Sea Stallion Roskilde-Dublin 2007 | left 1 July, arrived 14 August (with stops) | https://en.wikipedia.org/wiki/Sea_Stallion_from_Glendalough |
| Largest longships | 5-6 kn under oars, up to 10 kn under sail; draught allows water 1 m deep and beach landings | https://en.wikipedia.org/wiki/Viking_ships |
| Sea Stallion's average under way ~5.5 kn, bursts over 15 kn; close-hauled course about 60 deg to the true wind with 10-15 deg leeway, so about 70-75 deg made good | *(guess; from memory of the museum's published 2007-08 logs, not opened this session)* |

Consistency check *(derived)*: a square-rigger that lies 60 deg off the true wind, makes 10-15 deg of leeway and does 4.5 kn through the water makes good 4.5 x cos(72 deg) = 1.4 kn to windward; at 5.5 kn and 70 deg, 1.9 kn. That matches the museum's "at best 2 knots". So the game's windward numbers are: closest heading 60 deg, course made good 70-75 deg, VMG 1.5-2 kn in a fresh breeze, nothing in a light one.

### 1.3 Hull speed and the Froude number

Hull speed v = 2.43 x sqrt(LWL in m) knots; the resistance climbs steeply from a Froude number Fn = v / sqrt(g L) of about 0.35, with speed-length ratios 1.20 (Fn 0.35), 1.35 (0.40), 1.50 (0.45), 1.70 (0.50) marking the hump (https://en.wikipedia.org/wiki/Hull_speed). For our hulls *(derived)*:

| Ship | LWL (m) | Hull speed (kn) | Fn at the recorded top speed |
|---|---|---|---|
| Gislinge | 6.8 | 6.3 | 6.5 kn -> 0.41 |
| Coaster 14 m | 12.5 | 8.6 | 9 kn -> 0.42 |
| Skuldelev 1 | 14.5 | 9.3 | 13 kn -> 0.56 |
| Oseberg | 20 | 10.9 | 10 kn -> 0.37 |

The light, long, fine clinker hulls run past "hull speed" on a strong reach (Fn 0.5+): the model's resistance curve must have a hump, not a wall.

### 1.4 Wind and sea

Beaufort wind speeds from https://en.wikipedia.org/wiki/Beaufort_scale; the wave heights for forces 5-11 are the Met Office's "probable - probable maximum" pairs (https://weather.metoffice.gov.uk/guides/coast-and-sea/beaufort-scale, opened this session; the same table is in swell_storm_seas.md for forces 6-10), open-sea and fully developed; forces 0-4 and 12 are Wikipedia's ranges:

| Force | m/s | kn | Wave height, probable - highest (m) | Sea |
|---|---|---|---|---|
| 0 | 0-0.2 | <1 | 0 | mirror |
| 1 | 0.3-1.5 | 1-3 | 0-0.3 | ripples |
| 2 | 1.6-3.3 | 4-6 | 0.3-0.6 | wavelets, glassy crests |
| 3 | 3.4-5.4 | 7-10 | 0.6-1.2 | scattered white horses |
| 4 | 5.5-7.9 | 11-16 | 1-2 | frequent white horses |
| 5 | 8-10.7 | 17-21 | 2 - 2.5 (Met Office) | many white horses |
| 6 | 10.8-13.8 | 22-27 | 3 - 4 (Met Office) | large waves, foam crests |
| 7 | 13.9-17.1 | 28-33 | 4 - 5.5 (Met Office) | sea heaps up, foam streaks |
| 8 | 17.2-20.7 | 34-40 | 5.5 - 7.5 (Met Office) | spindrift |
| 9 | 20.8-24.4 | 41-47 | 7 - 10 (Met Office) | sea begins to roll |
| 10 | 24.5-28.4 | 48-55 | 9 - 12.5 (Met Office) | overhanging crests, white surface |
| 11 | 28.5-32.6 | 56-63 | 11.5 - 16 (Met Office) | exceptionally high |
| 12 | >=32.7 | >=64 | >=14 | air filled with foam |

Waves (https://en.wikipedia.org/wiki/Dispersion_(water_waves)): deep water L = g T^2 / 2 pi, c = g T / 2 pi (a 10 s swell is 156 m long and runs at 15.6 m/s = 30 kn; a 5 s wind sea is 39 m long at 7.8 m/s = 15 kn); shallow water c = sqrt(g h) and the wave steepens; a wave breaks at a steepness H/L of about 1/7 in deep water and at H of about 0.78 x depth over a bank *(standard coastal-engineering values, not re-verified this session)*.

Sea-state limits the project already proposed, by significant wave height Hs *(project notes, swell_storm_seas.md: "starting guesses")*: logboat 0.3 m, Gislinge 1.2 m, currach 2, Baltic coaster 2, longship 2.5, North Sea merchant 3.5. The same note found she rises and falls 3 m over 5 s on a 156 m swell with slopes of at most 3.5 deg: a long swell is harmless; it is the short steep wind sea and breaking crests that hurt.

### 1.5 Seamanship: what was done, and how well it is known

| Practice | What is known | Source / confidence |
|---|---|---|
| Reefing a square sail | reef bands with reef points tie the foot (or head) up to the yard; several bands "shorten sail to different degrees"; done "to preserve ... stability in strong winds" | https://en.wikipedia.org/wiki/Reefing (general); reef points on Viking sails are reconstructed, not found *(guess)* |
| Heaving to | on a square-rigger, courses clewed up and a topsail backed so she lies with the wind forward of the beam, drifting slowly to leeward and forereaching a little | https://en.wikipedia.org/wiki/Heaving_to ; for a one-sail ship: sail lowered or a scrap of it sheeted flat, the ship held bow-to-weather with the steering oar and, if need be, oars; a drag (a sea anchor of spars and sail) is plausible but not attested for 793 *(guess)* |
| Running before a storm | the ship runs with the wind and sea aft, sail reefed hard or struck; the danger is wave-riding at the wave's speed | https://en.wikipedia.org/wiki/Broach_(nautical) |
| Broaching | "an abrupt, involuntary change in a vessel's course, towards the wind, resulting from loss of directional control, when the vessel's rudder becomes ineffective"; happens when a vessel travels at near the wave speed, the orbital motion robbing the rudder of flow; she "swing[s] across the waves, roll[s] to one side, and perhaps capsize[s]", ending beam-on | same |
| Pooping | a following sea breaking over the stern into the open hull | general seamanship; the mechanics are in the model below *(derived)* |
| Bailing | open clinker hulls take water over the gunwale and through the laps; wooden bailers (scoops) are common finds in Norse boat graves; the 1893 Gokstad replica's planking worked 18 mm "without leaking" | https://en.wikipedia.org/wiki/Gokstad_ship ; the bailer finds *(from memory, not re-verified)* |
| The steering oar | "normally attached to the starboard side in larger vessels"; the Kvalsund ship (780-800) had "a detachable rudder on one side"; Carter's knarr steered badly until the oar's mounting and balance were rebuilt | https://en.wikipedia.org/wiki/Steering_oar ; https://en.wikipedia.org/wiki/Kvalsund_ship ; https://en.wikipedia.org/wiki/Knarr |
| Beaching | shallow draught "allowed navigation in waters only one meter deep and permitted beach landings" | https://en.wikipedia.org/wiki/Viking_ships |
| Towing upriver | Utrecht 1 has a small mast step near the bow "suggesting towing"; the first build's table gives 18 km/day towed upstream, 50 km/day downstream | *(project notes)* |
| Anchoring | iron anchors with wooden stocks are known from Norse ship burials (Oseberg, Ladby); stone anchors for small boats | *(from memory, not re-verified)* |
| Ice | the Rhine delta freezes in hard winters (the weather code already models `Ice` 0-1) | LevelWeather.cs |
| Clinker build | overlapping planks nailed through the lap over a rove and clenched; the hull's shape comes from the planks (shell first) | https://en.wikipedia.org/wiki/Clinker_(boat_building) |

### 1.6 Stability facts

- The metacentre: a ship is stable while the metacentre M lies above the centre of gravity G (https://en.wikipedia.org/wiki/Ship_stability). The righting arm at a small heel is GZ = GM sin(phi); the righting moment is displacement x g x GZ *(standard naval architecture)*.
- The free-surface effect: loose water in the hull reduces GM by i / V, where i is the second moment of the loose water's surface about the centreline (for a rectangle l b^3 / 12) and V the ship's displaced volume *(standard)*. For the Gislinge boat, 7 m of bilge 1.5 m wide gives i = 7 x 1.5^3 / 12 = 1.97 m4 against V = 1.8 m3: a loss of over 1 m of GM *(derived)*. An open boat with 20-30 cm of water sloshing in her is close to capsizing even on flat water. This is the physical reason bailing is a life-or-death order and not a chore.
- Downflooding: an open hull takes water when the heel reaches the angle at which the gunwale touches the water: for a box, phi_flood = atan(2 F / B) with F the freeboard amidships, B the beam. Gislinge (F 0.34 m, B 1.5 m): 24 deg. A 14 m coaster (F 0.55, B 3.6): 17 deg. A knarr (F ~0.8 laden, B 4.8): 18 deg *(derived from the first build's table)*. The sheer at the ends adds margin; a laden ship has less.
- Capsize ratio (a yacht rule of thumb): 2.0 or less "suitability for offshore navigation" (https://en.wikipedia.org/wiki/Capsizing). Beam-on: "even large vessels may be rolled by being hit broadside by a large wave or swell" (same). Small-craft research puts the capsizing breaking wave at about the beam's height or more *(guess)*.

---

## 2. The model

### 2.1 Shape of the model

One rigid body per ship, 6 degrees of freedom, stepped at 0.02 s by **our own integrator** (`ShipIntegrator`, pure C#: semi-implicit Euler, body-frame diagonal inertia, a 6 x 6 diagonal mass matrix that carries the added mass of 2.2), driven by our own forces:

```
(m + m_a) a   = F_buoyancy + F_gravity + F_hydro_damping + F_resistance + F_sail + F_oars + F_rudder + F_rope + F_contact + F_breaker
(I + I_a) w'  = sum (r_i x F_i)   + M_wave_slope + M_rudder + M_oars(asymmetric) + M_contact + M_breaker
```

PhysX is **not** the integrator. The Unity `Rigidbody` on the ship is kinematic and exists for collision detection only: `ShipBody` moves it with `MovePosition`/`MoveRotation` to the integrator's state, and reads contact impulses and points back from `OnCollisionEnter`/`OnCollisionStay` as `F_contact`/`M_contact` for the next step (2.9). So the code the EditMode tests run (the integrator and the pure force classes, 4) is exactly the code that moves the ship in the scene; nothing about the ship's motion depends on PhysX's own stepping.

Everything is in metres and seconds (game metres: 2.12). The clock: `LevelClock.Speed` is a plain static that every consumer multiplies in itself (`LevelClock` advances `GameHours` by `Time.deltaTime x rate x Speed`; `HdrpSea` sets `surface.timeMultiplier = Speed`; `WaterWaves` and `RiverCurrent` do the same). `Time.timeScale` is therefore **never touched**: raising it would run the clock, the HDRP waves and the river at 4x of 4x. Instead `ShipBody.Update` keeps an accumulator `acc += LevelClock.Speed x Time.deltaTime` and takes `floor(acc / 0.02)` integrator steps of 0.02 s per frame (1 step per frame at 1x and 60 fps, 2 at 2x, 4 at 4x; never faster than 4x, as the design says; multiplayer runs 1x), each substep sampling the water from HDRP's current CPU buffer (the sea moves at `Speed` already, so the substeps see it advance at the same rate the body does). Collisions: `Physics.simulationMode = SimulationMode.Script` ("execute the physics simulation manually when you call `Physics.Simulate`", https://docs.unity3d.com/6000.0/Documentation/ScriptReference/SimulationMode.html) and `Physics.Simulate(0.02f)` once per integrator step after the kinematic body has been moved, so the contacts it reports belong to that step; nothing else in the levels has a non-kinematic `Rigidbody` (the moored boats are kinematic, `ShipMotion.Ground` only raycasts), so Script mode changes nothing else. Paused (`LevelClock.Paused`): the integrator does not step, the body stays kinematic, and `ShipMotion`'s existing five-probe height/pitch/roll follow is used so she still rides the sea that goes on moving under her (`HdrpSea` keeps `timeMultiplier` at 1 while paused); the integrator's state is re-seated from the follow when play resumes. A test checks the rates (4, test 14).

State per ship (the `ShipState` record, pure C#, serialisable to the save):

| Field | Meaning |
|---|---|
| position, rotation, velocity, angular velocity | the body (world units; re-based on floating-origin shifts) |
| heading_order (deg), sail_set (0 struck, 1-3 reefs, 4 full), yard_brace (deg), oars_out (0..N), rowing_effort (0..1), anchor_down, hove_to, towing | the standing orders |
| water_aboard (m3), hull_integrity (0..1), leak_rate (m3/h), rig_integrity, sail_integrity | damage and the bilge |
| cargo: list of (item id, mass, position in hull), lashed (bool) | mass and trim |
| crew_fatigue per man, seamanship (the crew's average, 1..5), helm_skill | the men (from the crew design) |
| aground (bool, ground kind), tow_line (anchor point, length, tension) | constraints |

Parameters per ship class (`ships/<class>.json`; the first build's `ship_classes_793.json` is the seed, with these columns added):

| Parameter | Gislinge | Coaster 14 m | Knarr (Skuldelev 1) | Utrecht type | Notes |
|---|---|---|---|---|---|
| from year (the shipwright offers her from) / to | 793 / - | 793 / - | ~950 / - | 793 / - | the first build's `ship_classes_793.json` from/to columns; the Gislinge-class working boat and the Utrecht type are at the date; the 14 m coaster stands in for Dorestat 1 (riveted clinker is attested at Dorestad); the knarr not before ~950 *(guess, between Äskekärr c. 930 and Klåstad 998, 1.1)*; longship classes by their finds (Oseberg 820, Gokstad c. 890, Skuldelev 2 c. 1042) |
| L, B, D (depth to gunwale), LWL (m) | 7.7, 1.5, 0.6, 6.8 | 14, 3.6, 1.35, 12.5 | 15.84, 4.8, 1.8, 14.5 | 17.8, 3.8, 1.2, 17 | finds and the table |
| hull mass (kg), CG height above keel (m) | 500, 0.25 | 3,500, 0.5 | 6,000, 0.7 | 5,000, 0.4 | *(guess)* |
| Cb, Cwp (waterplane coefficient) | 0.50, 0.70 | 0.48, 0.72 | 0.50, 0.75 | 0.75, 0.9 (flat) | *(guess)* |
| keel depth below the hull (m), lateral plane A_lat (m2) at load draught | 0.08, 2.0 | 0.15, 9 | 0.2, 13 | 0, 11 | *(derived from draught x LWL)* |
| sail area A_sail (m2), centre of effort height z_CE (m above waterline), reef rows | 12, 2.5, 2 | 45, 4.5, 3 | 90, 6, 3 | 20 (if rigged), 3, 1 | Skuldelev 3 and 1 sails; the rest *(guess)* |
| oars, sustained power per man (W), oar efficiency | 6, 90, 0.55 | 4, 90, 0.5 | 4, 90, 0.5 | 2 + poles, 90, 0.5 | *(guess; 90 W is a fit man for hours)* |
| steering oar area A_r (m2), its arm x_r (m aft of CG) | 0.12, 3.2 | 0.35, 6 | 0.45, 7 | 0.3, 8 | *(guess)* |
| GM light / laden (m) | 0.45 / 0.35 | 0.9 / 0.7 | 1.2 / 0.9 | 1.1 / 0.8 | *(guess; the parametric cells give a first value, the mesh bake a better one)* |
| radii of gyration k_roll, k_pitch, k_yaw (m) | 0.6, 1.9, 1.9 | 1.44, 3.5, 3.5 | 1.92, 4.0, 4.0 | 1.52, 4.5, 4.5 | k_roll 0.4 B, k_pitch = k_yaw 0.25 L *(guess)*; 2.3 |
| freeboard light / laden (m) | 0.45 / 0.34 | 0.8 / 0.55 | 1.1 / 0.8 | 0.6 / 0.4 | table and *(guess)* |
| Hs limit (m), the sea she should not be out in | 1.2 | 2.5 | 3.5 | 0.3 | *(project notes)* |
| hull strength: impact speed that cracks a plank (m/s) | 1.5 | 1.8 | 2.0 | 1.2 | *(guess)* |

### 2.2 Buoyancy from sampled water heights

The hull is cut into N probe cells: 5 sections along the length x 3 across (port, centre, starboard) for small boats (N = 15), 7 x 3 (N = 21) for ships over 12 m. Each cell knows its position in the hull, its footprint area a_i (m2) and a depth-to-volume curve v_i(d) (m3 of hull below the water when the water stands d metres above the cell's keel point). The cells come **parametrically** from L, B, D and Cb first (`HullProbes.Parametric`, pure C#): the section area at station x is A(x) = A_mid x (1 - |2x/L|^p) with p chosen so the integral gives Cb x L x B x T, and each section's area-to-depth curve is the power law A(d) = A_mid (d/T)^q with q = 1 for a flat logboat (straight line) and 1.5 for a round bilge (a curve), halved per side for the port and starboard cells. A mesh baker (`HullProbeBaker`, an editor step that slices the hull mesh; the Gislinge FBX is an open "Planking" shell that `HdrpWater.cs` already has to lid from its vertices, so the slicing needs a closed lid first) is a later refinement and must agree with the parametric cells within 5% on displacement, waterplane area and GM, or the parametric set stays.

Each step, for each cell: ask `WaterSampler.Sample` for the water height h_i and normal n_i under the cell's world position (the HDRP CPU simulation; the sine fallback when there is none). The call is made with `feelDecals = false` (`includeDeformation`), so the hull never feels her own wake decal; `feelDecals: true` becomes an option for the shore-surf decal only (not the ship's own wake) once `ShoreWater`'s surf is in the deformation channel, and until then surf is a separate term (the Breaker field, 2.7). Immersion d_i = h_i - y_i (the cell's keel point in the world). Then

```
F_b,i = rho g v_i(d_i)  along n_i (not straight up: the wave's slope pushes her sideways and that is the real wave force on a small hull)
```

The sum of the cells' volume curves at rest equals the displacement, so she floats at the right draught by construction (a test checks it). Froude-Krylov only (the pressure of the undisturbed wave): correct while the hull is short against the wave, and the probes still catch the pitching on a wave of her own length. Diffraction and radiation are folded into added mass and damping:

- Added mass: the heave added mass of these wide shallow sections is of the order of the displaced mass, and in surge a quarter to a third of it ("the added mass can easily reach one fourth or one third of the mass of the ship", https://en.wikipedia.org/wiki/Added_mass; the sphere's coefficient is 0.5). Use coefficients heave 0.9, sway 0.6, surge 0.25, roll 0.3, pitch 0.5, yaw 0.5 on the respective inertias *(guess)*. Because the integrator is ours (2.1), these go in properly: the body-frame mass matrix is diag(m(1 + c_surge), m(1 + c_sway), m(1 + c_heave)) and the inertia diag(I_roll(1 + c_roll), I_pitch(1 + c_pitch), I_yaw(1 + c_yaw)); forces are summed through the centre of mass and torques summed separately, each axis divided by its own entry. (The alternative of dividing off-centre forces by a per-axis factor before `AddForceAtPosition` scales the torques by the wrong factor, roll by the sway and heave factors; it is not used.)
- Damping per cell: F_d,i = -(b1 + b2 |w_i|) w_i, where w_i is the cell's vertical velocity relative to the water surface's vertical velocity (h_i's time derivative, kept per cell). b1 and b2 set so that heave and roll decay in 2-3 cycles; a wave tank test in EditMode fixes them.
- Natural periods *(derived)*: heave T = 2 pi sqrt((m + m_a) / (rho g A_wp)): Gislinge 1.8 t + 1.6 t over A_wp 8 m2 -> 1.3 s; knarr 20 + 18 t over 55 m2 -> 1.65 s. Roll T = 2 pi k / sqrt(g GM), k the roll radius of gyration (~0.4 B): Gislinge 0.6 / sqrt(9.81 x 0.4) = 1.9 s; knarr 1.9 / sqrt(9.81 x 1.0) = 3.8 s. All well above the 0.02 s step; no stiffness trouble.

Orbital velocity: HDRP gives heights, not water velocity. For the resistance and rudder flow we add the wave's orbital velocity u = pi H / T at the surface (deep-water linear theory), in the wave's direction on the crest and against it in the trough, phased by the sampled height at the ship's centre (height above mean -> crest). H is the **local** crest-to-trough height at the ship: the difference between the stern cell's highest and lowest sampled height over the last T_p (a ring buffer per ship), so a big wave gives a big u and a small one a small u; T and the direction come from the sea state (`ShipSeaState`: `HdrpSea` now, the swell_storm_seas.md model later). When no sample history exists (the first T_p after a phase) the fallback is H = 0.63 H_s, since H_s is the mean of the highest third and using it for every crest overstates the mean orbital speed by about 1.6x *(derived)*. This is what robs the steering oar in a following sea (2.7).

### 2.3 Gravity, cargo mass and trim

Gravity 9.81 m/s2 on the whole mass: hull + men (80 kg each, at their stations) + cargo (each item at its stowage point) + water aboard (at the bilge, its own centre). Gravity is a term in the force sum (2.1), not something the engine adds: the kinematic `Rigidbody` has `useGravity = false` and takes no part in the dynamics. The integrator's mass, centre of mass and inertia are rebuilt whenever cargo, crew or bilge water change (not every step): the CG from the mass-weighted positions, the inertia as the diagonal m k_roll^2, m k_pitch^2, m k_yaw^2 about the CG with k_roll = 0.4 B and k_pitch = k_yaw = 0.25 L *(guess; the class table)*, plus the parallel-axis terms of the cargo items so a heavy load at the ends slows the pitch. A `Rigidbody` left to itself computes `centerOfMass` and `inertiaTensor` from its one convex collider, which gives the wrong CG height and the wrong roll and pitch inertia; nothing here reads those. The roll period derivation in 2.2 and test 3 (the period within 15% of 2 pi k / sqrt(g GM)) hold only because the tensor is set this way. Effects that fall out of this without more code: a laden ship sits deeper (less freeboard, lower phi_flood, more wetted surface, slower); cargo stowed forward trims her by the bow (she plunges and steers badly); cargo on one side lists her (the sail's heel adds to it on one tack); a heavy load high up (livestock, a stack of querns above the thwarts) raises G and cuts GM; unlashed cargo slides to the low side when the roll passes 15 deg (a one-off impulse and a permanent list until re-stowed, a crew order).

### 2.4 Hull resistance

Along the body's own axes, from the water-relative velocity v_w (ship velocity minus the current from `RiverCurrent.Drift` minus the orbital velocity):

- Ahead: R = 0.5 rho S v^2 (C_f(Re) + C_r(Fn)) with S the wetted surface at the current draught (from the probe cells), C_f the ITTC 1957 line 0.075 / (log10 Re - 2)^2, Re = v LWL / 1.19e-6, and the residuary coefficient from a table against Fn:

| Fn | 0.1 | 0.2 | 0.3 | 0.35 | 0.4 | 0.45 | 0.5 | 0.6 |
|---|---|---|---|---|---|---|---|---|
| C_r (fine clinker hull) | 0.0005 | 0.001 | 0.002 | 0.0035 | 0.006 | 0.009 | 0.012 | 0.016 |
| C_r (flat logboat, Cb 0.75) | 0.001 | 0.002 | 0.005 | 0.010 | 0.018 | 0.03 | 0.045 | - |

*(guess; shaped on the Froude hump at 0.35-0.5 from https://en.wikipedia.org/wiki/Hull_speed and calibrated so the top speeds in 1.2 come out: the calibration is a test, not a hand tune)*. Worked example *(derived)*: Skuldelev 1 at 7 kn (3.6 m/s), S ~55 m2: Re 4.4e7, C_f 0.0024, Fn 0.30, C_r 0.002: R = 0.5 x 1025 x 55 x 13 x 0.0044 = 1.6 kN. Her 90 m2 sail in 10 m/s true wind on a beam reach: the apparent wind is about 10.6 m/s at 70 deg off the bow, q A = 0.5 x 1.225 x 90 x 10.6^2 = 6.2 kN, and the **forward** component of the sail force is q A (C_L sin 70 - C_D cos 70) with the twist and heel losses of a baggy woollen sail, roughly 0.5 q A = 3.1 kN of drive (not the full 0.8 q A: the rest is side force, which heels her and makes leeway, 2.5). With the C_r table above that balances R near 8-8.5 kn (at 8.3 kn, 4.3 m/s: Fn 0.36, C_r about 0.004, R = 0.5 x 1025 x 55 x 18.5 x 0.0063 = 3.3 kN), a knot or more above the trials' 5-7 kn average in force 3-5; so test 6 **fits** the C_r table (raising the 0.3-0.4 entries) until 10 m/s on a beam reach gives 6-7 kn and 14 m/s on a broad reach gives 11-13 kn, the trials' 13 kn top. The hump at Fn 0.4-0.5 is what stops her: with the fitted table she climbs it only in a strong reaching wind.
- Sideways (leeway): F_lat = -0.5 rho A_lat |v_lat| v_lat C_lat, C_lat ~1.0 for a keeled hull at small drift angles, 0.4 for the flat logboat *(guess)*. With the sail's side force this sets the leeway angle: beta = atan(v_lat / v_ahead). Expected: 8-12 deg close-hauled in a fresh breeze for the keeled boats, 20 deg or more for the Utrecht type, more at low speed (so the first minutes after a tack lose ground).
- Rotation: yaw damping -c_y w_y |w_y| with c_y from A_lat and LWL (the long hulls are directionally stiff; the short Gislinge turns in her own length).
- Shallow water: when depth under the keel is less than 2 x draught, multiply C_r by 1 + 0.5 (2 T / h - 1)^2 (squat and the shallow-water hump; the ship slows over a bank before she touches, a cue the player learns) *(guess)*.
- Ice: `LevelWeather.Ice` 0-0.3 adds a slush drag R_ice = k_ice Ice^2 v with k_ice tuned so 0.3 halves her speed; above 0.5 the way is closed and the hull takes damage if driven on *(guess)*.

### 2.5 The sail

Apparent wind: V_a = V_true (from `LevelWeather.WindVector`, plus a gust field: wind x (1 + 0.25 x smooth noise over 20-60 s, 0.4 in a storm), measured at the sail's height with a log profile from the 10 m value) minus the ship's velocity. The sail is one square sail on a yard the crew braces; the angle of attack alpha is between the apparent wind and the sail's chord (the yard's line). The crew sets the brace to the best alpha for the current point of sail with a lag of 5-20 s by seamanship (a good crew trims as the wind shifts; a poor one leaves it); the player never touches the yard.

Sail force = 0.5 rho_air A_eff |V_a|^2 (C_L l + C_D d), with l perpendicular to the apparent wind in the sail's plane and d along it:

| alpha (deg) | 0 | 10 | 20 | 30 | 40 | 50 | 60 | 90 (running) |
|---|---|---|---|---|---|---|---|---|
| C_L | 0 | 0.5 | 0.95 | 1.2 | 1.15 | 1.0 | 0.8 | 0 |
| C_D | 0.1 | 0.15 | 0.3 | 0.5 | 0.65 | 0.85 | 1.0 | 1.3 |

*(guess; a loose-footed woollen square sail with a wind-tunnel-like curve, flattened; the tests fit it to the trial speeds, which is the only ground truth we have)*. A_eff = A_sail x (1 - 0.3 x reefs taken) for three reef rows, and 0 when struck. The sail cannot be driven closer than about 60 deg to the true wind: as the heading comes within that, the achievable alpha falls below 10 deg, C_L collapses and the sail shakes (its own sound and look); the crew will say so and hold her at 60 deg unless ordered to tack.

Tacking and wearing: a tack is a turn through the wind with way on; the body does it by physics (the steering oar's yaw moment while she still has speed, the sail aback for a few seconds pushing her astern and round). If her speed falls under about 0.5 m/s before she has passed the eye, she is in irons: she falls back on the old tack, or the crew puts out two oars forward to pull the bow round (a crew order with a delay). Wearing (turning away from the wind through a gybe) always works, loses 100-200 m to leeward, and is what a careful steersman does in a big sea.

Heel: the side component of the sail force acts at z_CE above the waterline and the hull's lateral resistance at about 0.4 x draught below it; the moment is M_heel = F_side (z_CE + 0.4 T). Righting: M_right = Delta g GZ(phi), with GZ(phi) = GM sin(phi) up to the downflooding angle and a curve from there to zero at the vanishing angle (phi_flood + 25 deg for the keeled boats, phi_flood + 10 deg for the logboat) *(guess)*. The steady heel is where they balance: Gislinge in 8 m/s on a beam reach: F_side ~ 0.5 x 1.225 x 12 x 9^2 x 0.9 = 540 N at an arm of 2.6 m = 1.4 kN m, against 1,800 x 9.81 x 0.4 x sin(phi): phi ~ 11 deg *(derived)*: she heels, she does not go over, and a gust of 12 m/s doubles the moment to 24 deg, which is the downflooding angle: that is when water comes over the lee gunwale and the crew reefs or luffs. That is the whole storm game in a sentence: the margin between the sail's heel and the gunwale.

Rig failure: if the sail load exceeds the rig's strength (A_eff x q above a class limit, roughly full sail in more than 15 m/s, one reef in 20, two in 25 *(guess)*), the dice (SimRandom, seeded) decide per minute between a split sail (A_eff drops to 0, repairable in port or over hours with cloth and a needle) and a sprung yard or stay (the sail comes down on deck; the ship is under oars until it is fished with a spar). The crew warns before this ("She can't carry it").

### 2.6 Oars and poles

Thrust from rowing: T = n_oars x eta x P_man x effort / max(v_w, 0.8), capped by a stroke-rate limit at low speed (the oar cannot pull harder than about 250 N per man at the catch) *(guess)*. Fatigue: each man has a stamina reservoir that drains at effort^2 x minutes and refills at rest; at the "sprint" effort the raiders' numbers from the AI plan hold (5.5 kn for 1.5 h, then 4 kn, *project notes*); at "steady" the museum's 3.5-4 kn holds for hours; at "easy" 2.5 kn for a whole day. Check *(derived)*: Gislinge, 6 oars x 0.55 x 90 W = 300 W; at 1.9 m/s (3.7 kn) that is 157 N of thrust against about 90 N of resistance (S 9 m2, C_f 0.003, C_r 0.001 at Fn 0.23), so she reaches about 4.2 kn at steady effort, 3.5 with a lazy man or a headwind. Rowing against the wind: the windage of the hull and the bare mast (0.5 rho_air A_windage V_a^2, A_windage ~ 0.6 x L x (freeboard + 0.3)) is added to the resistance; a Gislinge under oars makes no headway against 12 m/s.

Rowing on one side only, or backing one side, gives a yaw moment of T_side x B/2: how she is turned in a harbour, pulled off a bank or brought head to sea when the steering oar has no flow. Poles (the logboat, the shoal) push off the bottom: thrust only when depth under the keel is under 2.5 m.

### 2.7 The steering oar

A lifting surface on the starboard quarter: F_r = 0.5 rho A_r v_r^2 C_r(delta), with v_r the local flow (ship's water-relative velocity plus the orbital velocity at the stern, plus any oar wash), delta the oar's angle (the helmsman's output, limited to 35 deg and slewed at 30 deg/s), C_r = 1.5 sin(delta) up to a stall at 30 deg *(guess)*. The yaw moment is F_r x x_r; the oar also makes a small heel and a drag. The helmsman is a controller, not the player: given the heading order, he steers with a PD rule tuned by his skill (a good helmsman anticipates the yaw from each wave; a poor one oversteers and loses a few percent of speed). In shallow water (depth under the keel under 0.3 m) the oar is lifted and only the oars steer. Backing down (astern) the oar is weak and reversed.

Broaching, as the model does it: in a following sea where the ship's speed approaches the wave's phase speed (c = 1.56 T), the orbital velocity under the stern on the crest runs with the ship, so v_r falls towards zero and the oar's force with it (the square). At the same time the wave's slope under the probe cells gives a sideways component of buoyancy at the stern that is larger than at the bow (the stern is on the steeper face), which is a yaw moment towards broaching. These two are emergent from the sampled field: run fast on a steep sea and she will come round beam-on; the helmsman's skill and the ship's yaw damping decide how often.

What is **not** emergent yet, and is said plainly: HDRP's CPU simulation has no shore surf, no depth-limited steepening and no breaking, and `ShoreWater`'s surf is a decal the hull does not feel. The sea the probes meet over a bank is the same spectrum as 10 km out, and no sampled height ever "breaks". So until the sea-state model and the shore cap exist (dependencies, 2.11), breakers are a separate term in the force model, owned by `ShipSeaState`: a **Breaker field** with H_s, depth and period that fires synthetic breaking-crest events. The rate: in deep water a crest breaks at a steepness of about 1/7, so the event rate rises with H_s / L_p; over a bank the cap is H_s <= 0.55 x depth (the single-wave 0.78 x depth is the limit, not the average), so where the sampled sea stands higher than the cap the excess is turned into events. Each event, for the T/4 it lasts, adds (a) a crest-to-gunwale overtopping at the cells facing it (2.8, road 1, as if the gunwale were under by the event's height), (b) an impulse at the stern on a following sea (the crest's momentum, rho x event height x cell area x c, spread over the event's T/4 as F_breaker, which is the pooping surge), and (c) on the beam, a roll impulse that scales with the event's height against the beam (small-craft research puts the capsizing breaking wave at about the beam, 1.6). The tests already drive the ship with a synthetic sea (test 12), so the Breaker field tests the same way. Beam-on with a breaking crest arriving, the next section decides her fate.

### 2.8 Water aboard, bailing, capsizing

Water comes in by four roads, all in m3 per second:

1. Over the gunwale: for each gunwale cell whose relative water height (h_i minus the gunwale's height at that cell, which includes the heel) is positive, inflow = C_w x cell length x (excess)^1.5 x sqrt(2 g) (a weir), with C_w = 0.1 *(guess: a heeled open boat with a gunwale awash is not a sharp-crested weir, whose coefficient would be ~0.4, because the water alongside is moving with her and the flow is a sheet, not a head; 0.1 is a quarter of that)*. So a dip of 10 cm over 2 m of gunwale lets in 0.1 x 2 x 0.0316 x 4.43 = 0.028 m3/s, about 28 l/s *(derived)*, and a green sea over the bow in a storm (half a metre over 4 m of gunwale: 0.1 x 4 x 0.354 x 4.43 = 0.63 m3/s) swamps a Gislinge in seconds. Pooping is this, at the stern cells, on a following sea, with the Breaker field's events (2.7) adding to the excess.
2. Spray: in a wind sea with Hs above 0.3 x freeboard, a trickle of 0.005 x Hs x (1 - cos(angle to the sea)) m3 per minute per metre of beam *(guess)*: wet, cold, slow, and the reason a crew bails on and off all day.
3. The laps: leak_rate = base (0.002 m3/h for a sound hull) x (1 + 10 x working), where working is the hull's bending rate (the pitch and roll rates and the wave-induced shear between the bow and stern cells); plus damage leaks from 2.10.
4. Rain: `LevelWeather.Rain` x the open hull's area x 10 mm/h.

Bailing: each man set to bail removes 0.6 l per scoop, one scoop every 1.5 s when the water is deep, slower (reaching) when under 5 cm, so about 25 l/min per man sustained, 40 in a burst *(guess)*; four men bailing hold about 100 l/min (1.7 l/s), so a steady sea that puts 28 l/s over the gunwale cannot be bailed, only stopped (reef, luff, run, or turn to put the sea on the quarter). Bailing is a standing order the crew keeps for themselves (3): below a bilge threshold (5 cm) nobody bails; above it one man bails until it is under; above 15 cm two; the skipper's only decision is the emergency one, pulling men off the oars and sheets to bail or not.

The bilge water is a mass at the bilge's centre (it lowers G a little) but also a free surface: GM_eff = GM - i_fs / V with i_fs the bilge pool's surface moment from its length and width at that depth (the probe cells' widths). The pool slides to the low side with the heel (its centre follows -sin(phi) x width/3), which is what makes a flooded boat lurch. When GM_eff goes below zero she lolls to the low side; the next wave does the rest.

Capsize: when the heel passes the vanishing angle or the gunwale cells are under for more than 3 s with the water aboard above 60% of the hull's volume, she is lost. In single player at sea that is the end (the design: a wreck is game over). In a river place she sinks in her own depth (the harbour is 1-3 m): the design question in 6.

### 2.9 Anchoring, grounding, beaching, towing

Anchor: a rope from the bow to a point on the bottom. Holding force by ground for a fluked iron anchor (sand 10x, mud 8x, gravel 6x, weed 2x, rock 1x the anchor's weight; 15 kg iron for the small boat, so 1,470 N in sand) and for a stone (3x in sand, 2x in mud, 1x elsewhere; a 25 kg stone holds 735 N in sand) *(guess)*, reduced when the scope (rope length over depth) is under 3:1. The rope is a spring (only in tension) between the bow and the anchor point; when the tension exceeds the holding force the anchor point moves with the ship at the rope's angle (it drags) and the crew calls it: **dragging is the normal failure**. Tide: `LevelWater` moves the water; a boat anchored at high water in 2 m finds herself aground at low (the Dorestad shoal does exactly this; so do the Wadden flats). Riding to anchor in a sea, the bow is held up to the waves and she pitches hard, and the snatch loads as the bow lifts are several times the steady wind load (a Gislinge's windage of ~3.5 m2 in 20 m/s is only 860 N steady; the snatches at a short scope in a 1.5 m sea reach 2-4 kN *(guess)*), which is what drags her. Rope strength: 3-strand manila of 16 mm breaks at 17.6 kN and of 24 mm at 36 kN (Engineering Toolbox, https://www.engineeringtoolbox.com/manila-rope-strength-d_1512.html); period hemp, wet and spliced, is taken as 10 kN for the small boat's 16 mm line and 35 kN for the knarr's 24 mm *(guess)*. Parting is therefore an age-and-chafe event, not the normal one: the rope has an integrity (1 new, falling with months at sea and every night it saws on a stone bottom) and parts when the tension passes integrity x strength; a new rope only parts on rock in a storm, an old one in a gale.

Grounding: the keel cells test the ground height (`ShipMotion.Ground`'s raycast, kept) against their world height each step. Contact gives a normal force (a stiff spring and damper in our integrator, so it is stable) plus friction by ground: mud 0.3, sand 0.5, gravel 0.7, rock 0.9 x the normal force, and a damage roll when the contact speed exceeds the class's plank limit on gravel or rock, or on posts and dams. Soft ground at under 1 kn: she stops, no damage; she comes off when the tide lifts her, when the crew lightens her (cargo on the beach), or by rowing/poling astern against the friction. Sandbanks at sea are the real killer: a bank in a breaking sea holds her while the waves fill her (2.8 does the rest). Until the sampled field steepens and breaks over banks (2.11), it is the Breaker field (2.7) that supplies those breakers: its depth comes from the same raycast, so a ship held on a bank in 1 m of water with a 2 m sea outside gets events at the capped height of 0.55 m every few seconds, and that is enough to fill a Gislinge. Beaching on purpose is grounding at under 2 kn on a shelving sand or mud cell, sail down; she is then dragged up by the crew (an order; above the tide line she is safe and the game can save).

Collisions: the hull is a convex mesh collider on the kinematic `Rigidbody` (2.1) against the dams, posts, piles, other hulls and the banks; PhysX detects the contacts (stepped by `Physics.Simulate` after each integrator step) and `ShipBody` reads each contact's point, normal and penetration from `OnCollisionEnter`/`OnCollisionStay` and turns them into `F_contact` (a penalty spring and damper along the normal, friction across it) and `M_contact` for the next integrator step; two kinematic bodies do not report contacts to each other, so ship-to-ship hits use a trigger on one hull and the same penalty rule. A contact impulse above the plank limit cracks a plank (a leak of 0.05-0.2 m3/h each, visible as a wet seam, patched in port) and above twice it stoves one in (a leak of 1 m3/h, she must be beached). Posts and mooring lines: a mooring is two anchor-like ropes to bollards; moored boats are kinematic as now.

Towing upriver: a rope from the mast step (or the Utrecht type's forward mast) to a tow point that walks the bank at 0.6-1.0 m/s (the towpath is a baked line per river place); the rope's tension pulls her along and towards the bank, the steering oar holds her off; the river's current (`RiverCurrent.Drift`, with `pushesShips` on for the body) opposes at 0.3-0.5 m/s. Net: 0.4-0.6 m/s over the ground, 15-20 km in a day with rests (the table's 18 km/day). Where there is no towpath she is rowed or poled, at half that. Downstream she drifts with the stream and the oars: 50 km/day.

### 2.10 Damage and the crew's mitigations

| Danger | What the model does | Crew action that mitigates | Delay (good crew / poor) |
|---|---|---|---|
| Too much sail in a gust | heel past the gunwale, water in; rig failure roll | reef (one row per 60-120 s), or strike sail; luff to spill wind | 60 s / 150 s per reef; luff 5 s / 15 s |
| Breaking wind sea on the beam | gunwale cells dip; roll resonance when the wave period meets the roll period | bring her head to sea (heave to under oars and a scrap of sail) or put the sea on the quarter | 20 s / 60 s |
| Following sea, wave-riding | oar loses flow, wave-slope yaw: broach | shorten sail so she runs slower than the waves; tow a drag (guess); steer actively | 60 s / 150 s |
| Pooping | stern cells under, big inflow | same as above; bail | - |
| Water aboard | free surface, lower freeboard | bail (men off the oars and sheets); lighten her (cargo over the side: real money lost) | immediate / 30 s |
| Leaks from working | leak_rate rises with the sea | bail; run for shelter; beach | - |
| Grounding on a bank | friction, waves fill her | sound the depth (lead line: the crew calls it from the lookout's "Eyes"), lighten, pole off, wait for the tide | - |
| Anchor dragging on a lee shore | ship moves towards the bank | more rope (scope), second anchor, oars, cut and run | 30 s / 90 s |
| Ice | drag and plank damage | go the long way, wait, haul out | - |
| Fog/night | no mechanic here (the lookout's) | - | - |

Hull integrity (0-1) falls with each plank cracked, each grounding on hard ground, and age; it raises the base leak and lowers the plank limit; it is restored by the shipwright in port (a cost in silver and days, so a merchant weighs it).

### 2.11 Response to season, year, weather and events

- Wind and gusts: `LevelWeather` (today: one wind, mostly south-west, a kind per six hours). The storm plan's S1-S3 (real lows, gusts that move, the chronicle's storms on their days) feeds this model with no change here: the body only reads a wind vector at the sail and a sea state.
- Sea state: `HdrpSea` already lags the swell and foams by Beaufort; the swell_storm_seas.md model (wind sea by fetch and duration, swell from distant lows) supplies Hs, Tp and direction for the orbital velocity and the spray. The HDRP surface supplies what the hull actually meets.
- Tide and flood: `LevelWater`/`TideModel` move the water; the ship simply floats on it, grounds when it leaves, and feels the current reversing with the tide (`RiverCurrent.Factor`).
- Season: cold (`Temperature` under 5 C) doubles fatigue drain and halves bailing after an hour wet; ice closes rivers; winter seas are higher (the weather); daylight (the clock) decides whether the crew will sail at all (an order they can refuse).
- Chronicle events: a storm on its day is just weather to the body; a flood raises the river's speed and drowns the dams (grounding on a submerged dam at speed is a plank gone); the port's dams and posts are colliders, so a raid that burns a dam leaves posts to hit.
- Year: the ship ages (integrity) and the player buys bigger classes (only those whose from-year has come, 2.1); the heirs inherit the hull and its state.

Dependencies on work this document does not do, listed so the mechanics above are not claimed before they exist:

| Needed | Owned by | Until then |
|---|---|---|
| The sea-state model (wind sea by fetch and duration, swell from lows): H_s, T_p, direction | swell_storm_seas.md / the weather pass | `ShipSeaState` reads `HdrpSea`'s Beaufort-driven settings |
| Depth-limited breaking (H_s cap 0.55 x depth) and shore surf in the sampled field, so the probes feel the breakers | the water pass (`ShoreWater`'s surf into the deformation channel; a depth cap on the HDRP spectrum near shore) | the Breaker field (2.7) |
| Gusts that move (storm plan S1-S3) | the storm plan | a noise gust on `LevelWeather.WindVector` |
| Crew stats (seamanship, helm, oar, stamina) | the crew design | a flat crew of 3 |
| The save system | the modes/map design | `ShipState` is serialisable from day one |

### 2.12 Game metres on a shortened sea

All forces and motion in this document are in **game metres**: the ship is her real size, the waves under her are real size, and the integrator does not know what level it is on. On the river places (1:1) that is the end of it. On an ocean level the whole level is shortened by `LevelInfo.realMetresPerUnit` (150 on `L_NorthSea`: 492 km of real route in 3.3 km, *project notes*), and the ship still moves at her real speed through game metres. Wind, tide drift and leeway are real in angle and speed, so their **geographic** effect on a 1:150 level is exaggerated 150x: 10 deg of leeway over the 3.3 km crossing is about 580 game metres, which is 87 km of real coast; the 100-200 m a wear loses to leeward is 15-30 real km; the Flemish banks the player stands off are a few hundred game metres wide. This is accepted on purpose: it is what makes the wind and the tide matter in a 30-minute Dorestad-London voyage, which is the whole point of the shortened sea (the user's rule: feel and grandeur, not 1:1 units). The chart, the voyage planner and the AI fleets think in real kilometres and convert at `realMetresPerUnit`; the physics never does. The one real decision is whether the sideways drifts (leeway, tide set, wind drift at anchor and hove to) should be scaled by 1 / realMetresPerUnit on ocean levels so that a crossing lands where a real one would, while the ship's heel, speed and motion stay real: question 6 in section 6. The default here is no scaling.

---

## 3. In the game

What the player does is give orders; the crew's seamanship and the physics decide the rest. The orders (in the council/compass panel in 3D voyage, on the chart, and from the deck in isometric):

| Order | What happens | Feedback the player sees and hears |
|---|---|---|
| Make for (click the water or a mark) / Hold course | the helmsman steers; the sail is trimmed and reefed as the crew sees fit (a standing "carry sail: easy / full / press her" setting) | the heading on the rose; the yard swings; the wake bends |
| Set sail / one reef / two reefs / strike sail | the crew works the sail, 1-2 minutes per row, men off the oars | the sail's thunder when it luffs, the creak of the yard, the ship standing up as a reef comes in |
| Out oars / easy / steady / sprint / oars in | thrust and fatigue | oar-locks, the stroke's splash in time, the men's breathing; speed over the ground only in the dev HUD (the player reads the water) |
| (standing) Bail | the crew bail on their own below a bilge threshold (2.8); the skipper only decides, in an emergency, "all hands to bail" (men off the oars and sheets) or not | splash, the water in the bilge sloshing, the heel steadying; "She's taking water" when the threshold is passed |
| Heave to | sail struck or a scrap, head to sea under oars, drift 0.5-1 kn to leeward | she pitches hard, spray; a quiet choice to wait a storm out |
| Run before it | sail reefed to two, course downwind with the sea on the quarter | the following seas lifting the stern, the helmsman's calls, the risk of the broach |
| Let go / weigh anchor; more rope | the rope's spring; the drag warning | the splash, the rope creaking, the bow swinging to the wind |
| Lighten her | a cargo item over the side (real loss) | the splash and the ledger |
| Tack / wear | the turn through or away from the wind | the sail aback, the oars put out if she stalls |
| Beach her / haul out; tow | grounding on purpose; the tow line | the keel grinding on sand, the rope's hum |

What she looks like when it is right: the hull lifts a quarter of a second after the crest passes under the bow and the stern drops after; the heel is a steady lean that stiffens as the sail is reefed; the gunwale dips to the water and spray comes over on the weather bow, not from nowhere; in a calm the sail hangs and slats and she rolls to the old swell; in a storm the sea stands up above the stern and she lifts to it, or does not. The sounds (all CC0 clips in `data/sounds_cc0`, chosen by state): rigging creak by sail load, the sail's flap by luffing, oar-locks by the stroke rate, the bilge slosh by water aboard, the keel's grind on sand, the rope on the bollard, the hull's groan in a seaway by the working rate, the thud of a sea on the bow by the overtopping event. Every sound is driven by a number the model already has.

The crew's warnings are keyed to the same numbers, so every failure has a readable cue before it, and the broach (the one failure that ends the game in a second) most of all:

| Number | Threshold | Line (placeholder wording; the user writes the real ones) |
|---|---|---|
| helm authority (v_r^2 at the oar against its flat-water value) on a crest | under 40% | "She's not answering, skipper." |
| GM_eff with the free surface | under 0.1 m | "She's getting sluggish." |
| H_s against the class limit | above it | "This is no sea for her." |
| ship speed against the wave speed on a following sea | above 0.8 c | "She's running too fast for these seas." |
| heel against the downflooding angle | within 5 deg | "The gunwale's awash." |
| rig load against the class limit | above 80% | "She can't carry it." |
| anchor tension against holding | above 80% | "The anchor's working." |
| bilge depth | above the standing threshold | "She's taking water." |

The game can be saved at any time (the user's rule), so a loss at sea is a reload of the last save, not a lost evening; the cues above are what make the loss feel earned rather than sudden.

Fun and replay: every voyage is the same water with different wind; the player learns the ship as a sailor does (when to take the reef, which tide to leave Dorestad on, how far to stand off the Flemish banks, that the logboat cannot sail and must be towed, that four men bailing cannot hold a swamping sea). Risk is a trade decision: sail into the dark with the fair wind or anchor; press her and reach the fair a day early or arrive with the cargo wet. The ships are a ladder in time as well as size: the four-man boat and the 14 m coaster from 793, the knarr from about 950 (the heirs' ship: 1.1), each with its own numbers and its own failures. Emergent stories: the broach survived, the night at anchor off a lee shore with the rope humming, the anchor that dragged while the crew slept. In multiplayer the same physics answer a raider's chase: oars in a calm, sail in a breeze, and the shoal the local knows.

What reads as real, from the games that tried it (the game wikis and Steam pages were not all readable this session; these are from memory of play and from the Sailwind Steam page, https://store.steampowered.com/app/1764530/Sailwind/ , which lists "realistic sailing physics", tacking, reefing in heavy weather, waves and no fast travel):

| Game | Model | What reads as real | What does not |
|---|---|---|---|
| Sailwind | apparent wind on each sail, hand-trimmed sheets and halyards, leeway, heel, reefing, waves that stop the boat, celestial navigation, real-time passages | the boat slows when the sail is wrong, luffs when too close, heels in gusts, and days pass; the player feels the wind | the player is the whole crew, hands on ropes; no crew, no orders (our design is the opposite: orders) |
| Sea of Thieves | sail length and sail angle to a wind arrow, fixed speeds per ship, no leeway, no heel to speak of, waves are visual, water aboard from holes bailed with buckets | the crew roles and the bucket (water aboard is a shared emergency) | instant response, sailing into the wind at reduced speed, no capsize *(from memory)* |
| Valheim | wind sector against the heading with a no-sail zone ahead, three sail settings, a rudder, rowing in the no-sail zone | wind matters and the player waits for it; the raft wallows | one speed per setting, no heel, waves do not endanger *(from memory)* |
| Naval Action | polar speed diagrams per ship, manual yard bracing, heel changes gun range, leeway, depowering; square-riggers cannot point | the polars are per ship and tacking a square-rigger is a plan, not a click | the ship as a gun platform; no water aboard, no capsize *(from memory)* |

The lesson for Kāpman: Sailwind's physics with Naval Action's per-ship polars, but through the crew. Our ship reads as real when (1) she has a no-go zone and makes leeway against a mark on shore, (2) she heels and the gunwale is a line the player watches, (3) the sea lifts her by its own motion (she is on the HDRP water, not animated), (4) orders take time and the men are seen doing them, (5) the sounds follow the numbers, and (6) she can be lost.

---

## 4. Build plan for Unity

Assemblies and namespaces: a **new asmdef `Kapman.Ships`** at `Assets/Kapman/Code/Ships/` that references only `Kapman.Core` (no HDRP, no Input System; `UnityEngine` only for `Vector3`/`Quaternion`/`Mathf`), holding the pure classes and the integrator; and `Kapman.Levels.Ships` inside the existing `Kapman.Runtime` for the MonoBehaviours. The test asmdef `Kapman.Tests.EditMode` today references only `Kapman.Core`, so `Kapman.Ships` is added to its `references`; the tests then run headless with the command in PROGRESS.md (`-batchmode -nographics -runTests -testPlatform EditMode`) without a scene, and they run the exact code that moves the ship in the scene, because the scene's `ShipBody` calls the same `ShipIntegrator` (2.1).

| Class | Kind | Does | Hooks |
|---|---|---|---|
| `ShipClassData` | data | the class table (2.1), loaded from `Assets/Kapman/Data/ships/*.json` with `Core/Data/Json` | the first build's `ship_classes_793.json` is converted by an editor command |
| `HullProbes` | data | the N cells: local position, footprint, volume curve, gunwale height, keel point; `HullProbes.Parametric(L, B, D, Cb, q)` builds them from the class numbers (2.2); `HullProbeBaker` (Editor, later) slices the hull mesh and must agree within 5% | `Editor/` beside `DorestadBuilder` for the baker |
| `ShipState` | record | 2.1's state; save/load | the save system (later) |
| `ShipIntegrator` | pure | the 6-DoF semi-implicit Euler step at 0.02 s: body-frame diagonal mass and inertia with the added mass (2.2), the force and torque sums, position/rotation/velocity/angular velocity; `Step(ShipState, forces, 0.02f)` | everything; the only thing that moves a ship |
| `Hydrostatics` | pure | immersion -> volume, waterplane, GM from the cells; free-surface correction; mass, CG and inertia rebuild (2.3) | tests |
| `HullForces` | pure | buoyancy + damping per cell, resistance (2.4), lateral, yaw damping, shallow-water and ice factors | `WaterSampler` for heights and normals (through an interface `IWaterField` so tests feed a sine or a flat sea) |
| `SquareSail` | pure | apparent wind, brace, alpha, C_L/C_D, A_eff, heel moment, rig load, failure rolls | `LevelWeather.WindVector`, a new `Gusts` (noise field) |
| `SteeringOar`, `Oars`, `Poles` | pure | 2.6, 2.7 | crew stats (from the crew design: Seamanship, Oar) |
| `Bilge` | pure | inflow by road, bailing, free surface, capsize test (2.8) | `LevelWeather.Rain`, the sea state |
| `Mooring` | pure | anchor rope, tow rope, mooring lines: spring forces and the drag/part rules (2.9) | ground kind from the terrain layers |
| `Grounding` | pure | keel contact, friction, damage rolls | `ShipMotion.Ground` (the raycast), terrain layer -> ground kind |
| `Helm` | pure | the helmsman's PD controller, tack/wear/in-irons logic, the "she won't lie closer" refusal | orders |
| `ShipOrders` | pure | the order set and the crew's delays by skill | the council UI (later), `DevHud` now |
| `ShipBody` | MonoBehaviour | in `Update`: the `LevelClock.Speed` accumulator (2.1), per substep samples the water, calls the pure parts, calls `ShipIntegrator.Step`, moves the kinematic `Rigidbody` with `MovePosition`/`MoveRotation`, calls `Physics.Simulate(0.02f)`, collects contacts; rebuilds mass/CG/inertia on cargo change; floating-origin shifts; pause (hands the follow back to `ShipMotion`) | **drives** `ShipMotion` on the same GameObject (next row); `Physics.simulationMode = Script` set once by the level |
| `ShipMotion` (existing, kept) | MonoBehaviour | the facade. Gets a `driven` flag: when a `ShipBody` is on the GameObject, `ShipMotion` stops integrating (its `Update` neither moves nor tilts her) and instead publishes the body's position, `heading`, `Velocity`, `Speed`, `Pitch`, `Roll` and `Status` each frame; `MakeFor` forwards to `Helm` as a heading order; `Player` and the static `Ground` are unchanged; while paused it does its own five-probe follow as today. Moored and distant boats keep the undriven `ShipMotion` as now | read today by `ShipWake` (its `ship` field), `Birds.cs:136`, `DevHud.cs:52`, `LevelExit.cs:29/71`, `LevelSoundscape.cs:68`, `ShoreWater.cs:198`, `OrbitCamera.cs:78` (`Ground`), and the first build's shims (`KapmanGame`, `SailingShip`, `VoyagePlanner`, `LocalWeather`); the editor builders (`DorestadTown.cs:1120`, `HdrpWater.cs:332`) keep adding it to the player prefab and add `ShipBody` beside it. None of these change |
| `ShipSounds` | MonoBehaviour | maps the numbers to the CC0 clips | `LevelSoundscape` |
| `ShipSeaState` | MonoBehaviour | Hs/Tp/direction from `HdrpSea` now and the sea-state model later; the local wave height ring buffer and orbital velocity (2.2); the Breaker field (2.7) | `HdrpSea`, `ShipMotion.Ground` for depth |

Where it hooks into what exists: `WaterSampler.Sample` (heights, normals; already tight, 8 iterations for the player); `LevelInfo.WaterLevel` (the fallback); `RiverCurrent.Drift` (set `pushesShips` on for the body); `LevelWater.TideRate`; `LevelWeather` (Wind, WindVector, Rain, Ice, Temperature; add `Gust(position)`); `HdrpSea.RippleWindTo` and the swell wind for the sea state; `ShipWake`, `OrbitCamera`, `DevHud`, `Birds`, `LevelSoundscape`, `ShoreWater` and `LevelExit` go on reading `ShipMotion`, which is now fed by the body; `LevelExit`/`LevelEntry` carry `ShipState` across a phase (velocity kept, position re-based). `FloatingOrigin`: on a shift, add the delta to the integrator's position and set the kinematic `Rigidbody`'s `position` (not `MovePosition`); velocities are untouched. `LevelClock`: **no `Time.timeScale`**; the body takes `Speed` substeps per real step (2.1) and `GameHours` go on advancing at 1 s = 1 min x `Speed` exactly as today; the accumulator is capped at 8 steps per frame (a hitch at 4x does not spiral). `Physics.simulationMode = SimulationMode.Script` is set by `ShipBody.OnEnable` on the player's ship and restored on disable; the only other physics users in the levels are raycasts, which Script mode does not affect.

Data files: `Assets/Kapman/Data/ships/gislinge.json`, `coaster_14.json`, `knarr_skuldelev1.json`, `utrecht_logboat.json` (and the AI classes from the first build's table); `sail_coefficients.json` (the alpha table); `ground_kinds.json` (friction, holding, damage); per prefab a baked `HullProbes` asset.

Tests (EditMode, headless, in `Assets/Kapman/Tests/EditMode/Ships/`, in the `Kapman.Tests.EditMode` asmdef with `Kapman.Ships` added to its references), all on the pure classes and `ShipIntegrator` with a fake water field (`IWaterField`: flat, a sine, or a synthetic sea), stepping the same integrator the scene runs, so a passing test is a statement about the ship in the scene:

1. Hydrostatics: a box 7.7 x 1.5 x 0.6 at 1,800 kg floats at 0.152 m in sea water (within 1 mm); the parametric Gislinge probe set at 1,800 kg floats within 2 cm of its class draught; the mesh-baked set, when it exists, within 5% of the parametric one on displacement, A_wp and GM.
2. Heave: dropped 0.2 m on flat water she settles within 3 cycles and the period is within 15% of 2 pi sqrt((m + m_a) / (rho g A_wp)).
3. Roll: a 10 deg heel released on flat water decays; GM from the cells is within 10% of the class value; the roll period from the integrator is within 15% of 2 pi k_roll / sqrt(g GM) (this checks that the inertia tensor of 2.3 is in use); with 0.25 m3 of water in the Gislinge's bilge GM_eff goes below zero (the capsize mechanic exists).
4. Polar: on a 2,000 s run in 8 m/s true wind, speeds at 60, 90, 120, 150, 180 deg to the wind are within 10% of the class polar (Gislinge: 4.8 kn beam, 5.5 kn at 130 deg, *project notes*); at 50 deg the sail luffs and the helm refuses.
5. Leeway: close-hauled in 8 m/s the drift angle is 8-15 deg (keeled), 18-30 deg (logboat); the sign is to leeward.
6. Top speed and the C_r fit: Skuldelev 1 in 10 m/s on a beam reach settles at 6-7 kn; in 14 m/s on a broad reach reaches 11-13 kn and no more (the hump works); the Gislinge 6-7 kn. This test is the calibration of the C_r table (2.4): the table is edited until it passes, and the passing table is the data file.
7. Rowing: six men steady give 3.5-4.2 kn on flat water in a calm; against 10 m/s of wind under 1.5 kn.
8. Heel: the Gislinge in 8 m/s beam wind heels 8-14 deg; 12 m/s full sail passes the downflooding angle; with one reef it does not.
9. Overtopping: a gunwale cell of 2 m (the test's own cell length; the Gislinge's cells are 1.36 m, 5 sections over LWL 6.8 m) held 0.1 m under for 1 s lets in 25-32 l (C_w 0.1); four men bailing remove 90-110 l/min.
10. Anchor: the 15 kg iron anchor at 3:1 scope in sand holds in 10 m/s with a 0.5 m sea and drags in 20 m/s with a 1.5 m sea; the 10 kN rope does not part in either; with integrity 0.1 it parts under the 20 m/s snatch loads.
11. Grounding: a keel cell on a 1:50 sand slope at 0.5 m/s stops without damage; at 2 m/s on gravel cracks a plank.
12. Broach: on a synthetic following sea (Hs 2 m, T 5 s, c 7.8 m/s) with the ship at 7.5 m/s, the oar's authority falls under 20% on the crest; a poor helmsman broaches within 10 waves, a good one within 50 *(a statistical test over seeds, SimRandom)*; the Breaker field at Hs 2 m over 1 m of depth fires events capped at 0.55 m.
13. Determinism: the same seed, orders and water give the same state after 10,000 steps. What this buys: single-player replays and save/load that reproduce exactly on one machine. It does **not** promise lockstep across machines (floats, Burst and HDRP's CPU water are not bit-identical across CPUs), and multiplayer does not rely on it: each peer owns its ship's state and sends position, velocity and orders (the multiplayer plan's authority model), with the sea state seeded so all peers see the same waves.
14. The clock: at `Speed` 4 the body covers 4x the distance per real second that it covers at 1x (within 1%) while `GameHours` advance at 4 x (1 s = 1 min), exactly as before; at `Speed` 1 she covers her real speed in game metres per real second; the accumulator never takes more than 8 steps a frame.
15. Resistance: the Gislinge coasting from 2 m/s on flat water with no drive drops under 1 m/s in 40-80 s (Re 1.1e7, C_f 0.0029, Fn 0.25, S 9 m2: R at 2 m/s is about 80 N against 2.25 t with the surge added mass; quadratic drag gives v(t) = v0 / (1 + k v0 t) with k = 0.5 rho S C / m = 0.009 per metre, so 2 to 1 m/s takes about 55 s *(derived)*).

Not a test but a checklist for the first night in the editor, since the facade is what the rest of the level reads: `DevHud` shows her speed, the wake draws and bends, `LevelSoundscape` hears her, the birds scatter from her, `LevelExit` still fires at the phase line, and pause leaves her riding the sea.

Performance budget (RTX 4070, 1440p/60; the CPU is the Ryzen 7700X): the player's ship, 21 cells x `ProjectPointOnWaterSurface` at 8 iterations, 50 Hz = 1,050 queries a second, each about 2-4 us on HDRP's CPU simulation: under 0.3 ms per frame at 1x, four times that at 4x (1.2 ms, still inside the budget of a frame that has no other physics). Eight AI bodies at 9 cells, 25 Hz, 4 iterations: about 0.2 ms. The pure force math and the integrator are microseconds. `Physics.Simulate(0.02f)` with one kinematic convex hull against static colliders: well under 0.1 ms a call, 4 calls a frame at 4x. HDRP's CPU water simulation itself (already on, "full resolution 128") is the real cost and is paid already. Target: the whole ship system under 0.6 ms CPU per frame at 1x and 1.6 ms at 4x, no GPU cost beyond what `ShipWake` already draws. If `ProjectPointOnWaterSurface` proves dearer, drop the player's cells to 15 and the AI to 5 with no change to the pure code.

Risks: HDRP's CPU simulation lags the GPU surface by a frame (she may sit a centimetre off the drawn water in a fast sea; the `WaterLine` decal hides it); the floating origin; the camera reads `ShipMotion` once a frame, so the body's state is published after the last substep and interpolated by `Speed x Time.deltaTime - steps x 0.02` to keep the chase smooth; the sine fallback has no normals (use the height difference across the cells); `Physics.simulationMode = Script` is global, so any later non-kinematic `Rigidbody` in a level (a dropped cargo item, a floating log) is stepped by the ship's calls, which is fine at 1x and 4x faster at 4x (acceptable, the clock is 4x too).

---

## 5. First slice (one night: she floats)

The first task of the night, before any physics: the `driven` flag on `ShipMotion` (4), so that the moment a `ShipBody` exists every reader of `ShipMotion.Player` (the wake, the HUD, the birds, the sounds, the exits, the shore water, the camera's `Ground`) keeps working with nothing changed in them. Then build, in this order: the `Kapman.Ships` asmdef and its line in the test asmdef; `ShipClassData` (the Gislinge row, with from/to); `HullProbes.Parametric` (15 cells from L, B, D, Cb; **no mesh slicing tonight**); `Hydrostatics` (volume, A_wp, GM, mass/CG/inertia); `HullForces` (buoyancy along the normal, damping, ahead and lateral resistance, yaw damping); `ShipIntegrator`; `ShipBody` with the `Speed` accumulator, the kinematic `Rigidbody` and `Physics.Simulate`, driving the facade, on the player's ship in `L_NorthSea` and `L_Dorestad`. Drive tonight is a constant ahead force from a fixed steady oar effort (2.6's number for six men), so she moves; the click-to-make-for order becomes a heading order answered by a plain yaw torque (the `Helm` and the steering oar's flow model are night two). No sail, no bilge, no anchor, no grounding yet (the old "too shallow ahead: she stops" guard stays as a stand-in).

Done when: tests 1 (parametric), 2, 3 (the GM and roll-period parts, not the bilge), 14 and 15 pass headless with the command in PROGRESS.md; and in the editor she floats at her waterline in Dorestad's harbour and rides the swell off Katwijk with the HDRP sea under her, pitching and rolling by the water rather than by five lerped heights, at 1x, 2x and 4x without the clock or the waves changing rate, and paused on a moving sea; the checklist in 4 passes (HUD, wake, sounds, birds, exit, pause). Commit with the tests; the user play-tests before the sail goes in.

Before the Dorestat 1 numbers go into `ships/*.json`: find the excavation report or the replica project's page and cite it (the rows in 1.1 are carried from PROGRESS.md with no source).

Night two, if the first holds: `SquareSail` with the coefficient table and the auto-brace, `SteeringOar` and `Helm` with the helmsman and the 60 deg refusal, tests 4 (at 90 and 150 deg), 5, 6 (the C_r fit) and 8 (8 m/s only); done when she heels to a beam wind and stands up when the wind drops, makes about 4.8 kn on the beam in an 8 m/s wind (the Overcast kind's 5 m/s raised with a dev key), and refuses a heading inside 60 deg of the wind with a line in the panel.

Night three: `Bilge` (overtopping, the standing bail order, free surface), `Mooring` (anchor with integrity), `Grounding` with the tide, the Breaker field, the crew's warning lines, and the sounds. The mesh baker (`HullProbeBaker`) comes after, as a refinement that must agree with the parametric cells within 5%.

---

## 6. Open questions for the user

1. **Tacking.** Let the ship tack through the wind when she has way on (the replicas do, with a stall risk and the oars to help), or only wear round (safer, loses ground)? Default in this document: both, the crew choosing wear in a big sea.
2. **Capsize in a river place.** At sea a capsize is the end (your rule). In Dorestad's harbour or a river she sinks in 1-3 m of water: game over too, or a salvage (days and silver, cargo spoiled)? Default here: salvage in a port or river, loss at sea.
3. **Storm waves at 1:1.** The 2 October storm plan squashed waves 1:4 like the land heights; your 4 October words ask for storms that look and act 1:1. This document assumes 1:1 wave heights everywhere the ship is, on the shortened ocean levels too (the sea is shortened in length, not in height). Confirm.
4. **The starting ship.** Keep the 7.7 m Gislinge stand-in (four men, a tonne of cargo) until a Dorestat 1 model exists, or start in the Utrecht-type logboat (a towed river freighter that cannot sail: the first voyage to the sea is then a different game)? Default: the Gislinge stand-in with the Dorestat 1's numbers dropped in when they are to hand.
5. **Throwing cargo.** "Lighten her" throws real goods over the side. Allowed as a player order, or only something the crew does on their own in extremity (and the player hears of it after)?
6. **Leeway on the shortened sea.** On `L_NorthSea` (1:150) the ship's real leeway and tide drift move her 150x further across the real map than they would at sea: 10 deg of leeway over the crossing is 87 real km of coast (2.12). Is that acceptable (wind and tide matter hard in a 30-minute voyage; the default here), or should the sideways drifts alone be scaled by 1 / realMetresPerUnit on ocean levels so a crossing lands where a real one would, while her speed, heel and motion stay real?
