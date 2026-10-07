# Sound audit (7 Oct 2026)

Read from the files only. Nothing was listened to. Unity was not run.
Lengths are from each mp3's Xing header (frame count), so they are exact to about 0.1 s.

## 1. The short answer

- 65 sound files in the project, all .mp3, all in `Assets/Kapman/Audio/Sounds/`.
  No .wav, .ogg or .flac anywhere under Assets. All are 128 kbit/s mp3.
- All 65 have a credit line in `Assets/Kapman/Audio/CREDITS_sounds.md`:
  each is CC0 1.0 from freesound.org, with uploader and link. None is UNKNOWN.
  None is synthesised as far as the credits say.
- 38 of the 65 are named in `Assets/Kapman/Audio/soundscape.json`. 27 are on disk but not named there.
- About 38 more recordings are credited in the same file but are NOT in Assets.
  They sit in `G:\kapman_unity\data\sounds_cc0\` only (section 3). Several are exactly what the ship view needs.
- The chart scene plays no sound today (section 5).
- The credits file's first line says the sounds are "used in Audio/Resources/Sounds". That folder does not exist; the real one is `Audio/Sounds`. Stale line.

## 2. Every sound on disk

Folder: `Assets/Kapman/Audio/Sounds/`. Licence for every row: CC0 1.0, freesound.org.
The number at the end of each file name is the freesound id (link = freesound.org/s/<id>).
"In json" = named in soundscape.json (which layer).

### Wind
| file | s | what it is | by | in json |
|---|---|---|---|---|
| light_breeze_139337.mp3 | 181.0 | wind in grass, trees behind, Patagonia | felix.blume | wind (low end) |
| light_breeze_620099.mp3 | 114.5 | field ambience | szelestamas | no |
| strong_wind_215853.mp3 | 361.0 | wind in dry grass, Texas | felix.blume | wind (high end) |
| reeds_wind_411847.mp3 | 38.4 | wind crossing reeds | SamuelGremaud | reeds |
| reeds_wind_shore_844604.mp3 | 65.5 | Baltic reed bed, waves behind | Funkelfang | reeds_shore |
| wind_leaves_spring_575645.mp3 | 57.4 | large leaves, spring birds and a cuckoo behind | Garuda1982 | leaves |
| wind_dry_grass_845415.mp3 | 38.5 | dry grasses in wind, Ohio | flumpytripod | dry_grass |
| winter_wind_bare_trees_637566.mp3 | 55.2 | light wind, bare winter trees, no birds | kyles | bare_trees |
| night_breeze_village_637147.mp3 | 93.2 | very quiet village night air, light breeze | kyles | no |

### Rain, thunder, ice
| file | s | what it is | by | in json |
|---|---|---|---|---|
| light_rain_200273.mp3 | 95.1 | light rain, rural (mono) | jmbphilmes | rain |
| rain_soft_loop_595717.mp3 | 22.5 | soft rain, England, looped by uploader | _lynks | rain |
| heavy_rain_125119.mp3 | 284.8 | thunderstorm with heavy rain | Framefive | rain |
| rain_on_pond_399281.mp3 | 38.1 | rain drops on a pond | Mjeno | rain_water |
| rain_on_tent_397916.mp3 | 51.5 | rain on a canvas tent from inside (stand-in for thatch) | Bronxio | rain_thatch |
| close_thunder_347853.mp3 | 19.1 | thunder crack | cognito perceptu | thunder near |
| close_thunder_574412.mp3 | 18.0 | close explosive crack | TRP | thunder near |
| distant_thunder_107518.mp3 | 151.4 | distant thunder | moxobna | thunder far |
| distant_thunder_361401.mp3 | 427.4 | distant thunder | richwise | thunder far |
| hailstorm_thunder_620489.mp3 | 32.0 | hail with far thunder | Cinetony | no |
| ice_lake_fractures_737896.mp3 | 58.2 | lake ice cracking, close (mono) | Vrymaa | no |

### Water and ship
| file | s | what it is | by | in json |
|---|---|---|---|---|
| calm_water_lapping_the_hull_851577.mp3 | 134.1 | gentle lake waves heard from INSIDE a sailboat cabin (Ammersee) | myLoop | ship_bow (slow) |
| water_past_the_hull_while_sailing_360631.mp3 | 78.7 | sailing boat bow wave, close | Pfannkuchn | ship_bow (fast) |
| wake_rowing_boat_588308.mp3 | 34.3 | rear wave behind a rowing boat, faint oars | Fenodyrie | ship_wake |
| oars_234361.mp3 | 334.0 | rowing boat, paddles, creak (mono) | martian | ship_oars (fast) |
| oars_paddles_calm_249707.mp3 | 38.7 | paddles on a calm stream, from inside the boat | brunoboselli | ship_oars (slow) |
| ship_creaks_sloshing_450255.mp3 | 40.1 | below deck of the sailing ship Lovis: creaks at the mast foot, water sloshing | kyles | ship_creak |
| river_lapping_bank_318064.mp3 | 59.3 | river lapping a pebbly shore at night (Hudson) | ceich93 | lapping |
| ripples_lake_shore_464801.mp3 | 48.1 | tiny waves on a lake shore | guyburns | no |
| wake_under_boardwalk_758858.mp3 | 110.3 | boat wake gulping under a wooden boardwalk, birds behind (mono) | Alex_hears_things | no |
| weir_small_560723.mp3 | 36.6 | small waterfall, weir-sized | cupido-1 | no |

### Birds and night
| file | s | what it is | by | in json |
|---|---|---|---|---|
| black_headed_gull_554806.mp3 | 32.9 | black-headed gull colony | Tom Corax | gulls |
| crow_741366.mp3 | 4.8 | hooded crow cawing (mono) | Mish7913 | hooded_crow |
| crow_carrion_call_181088.mp3 | 22.3 | single carrion crow | cfrooos | crows |
| curlew_475971.mp3 | 166.6 | garden birdsong with curlew | mossie55 | curlew |
| curlew_829120.mp3 | 178.7 | titled "Pink-footed geese, sound collage" (name and content may not match) | Sandy-Ogilvie | no |
| geese_407490.mp3 | 18.0 | geese flying overhead | mike_stranks | geese_skein |
| geese_flock_landing_484825.mp3 | 88.7 | Canada + greylag flock landing (Canada geese are wrong for 793) | naturenotesuk | no |
| geese_wild_meadow_535067.mp3 | 29.9 | wild geese in a meadow, Germany | derjuli | geese_meadow |
| heron_call_nest_825293.mp3 | 15.0 | heron at nest, one harsh call | inspire153 | heron |
| heron_river_park_834209.mp3 | 77.4 | heron in a river park, Catalonia | Aenimamusica | heron |
| mallard_quack_flying_456770.mp3 | 25.9 | mallard quacking, flying off | WavJunction.com | mallard |
| rooks_nesting_434847.mp3 | 44.3 | rookery on a windy day (mono) | mike_stranks | rooks |
| skylark_387426.mp3 | 75.2 | single skylark | Kinoton | skylark |
| skylark_635912.mp3 | 202.8 | skylarks over windy dunes | Kinoton | skylark |
| swallows_farm_nl_819880.mp3 | 137.2 | barn swallows at a farm, Apeldoorn (mono) | P.Romeijn | swallows |
| blackbird_evening_517816.mp3 | 48.8 | blackbird, forest evening, Vienna | stujun | blackbird |
| nightingale_england_632259.mp3 | 38.4 | nightingale, southern England | ollytc | nightingale |
| tawny_owl_688869.mp3 | 22.6 | tawny owl | Elanor1995 | tawny_owl |
| tawny_owl_203598.mp3 | 140.4 | crickets and tawny owl | raoul_slayer | no |
| night_crickets_522299.mp3 | 222.6 | crickets at night | Defelozedd94 | no (retired: wrong for the wet delta, json "about" line) |

### Town and harbour
| file | s | what it is | by | in json |
|---|---|---|---|---|
| bell_one_toll_554655.mp3 | 31.9 | low bell solo | HMTSCCSound | no |
| church_bell_village_321627.mp3 | 36.1 | old village church bell, Italy | extrafonia | no |
| fire_454745.mp3 | 20.4 | campfire on a summer night | roumahum | no |
| bonfire_night_364992.mp3 | 35.8 | bonfire at night (mono) | forfie | no |
| hearth_fire_crackling_707262.mp3 | 39.5 | wood in a fireplace | Sadiquecat | no |
| smithy_254370.mp3 | 78.8 | at the blacksmith | Emmaproductions | no |
| smithy_bellows_anvil_609757.mp3 | 27.3 | real smithy, bellows and anvil | LarsErikErtzgaardRingen | no |
| hand_saw_log_760611.mp3 | 26.5 | hand saw on a log | SoundsLikeYukon | no |
| axe_chopping_tree_573812.mp3 | 60.0 | axe on a dead tree | TRP | no |
| well_bucket_rope_649240.mp3 | 65.6 | bucket from a well, windlass and rope | Fedor_Ogon | no |
| footsteps_mud_path_449653.mp3 | 29.6 | walking in mud (mono) | Magnesus | no |
| wooden_gate_bolts_741621.mp3 | 36.5 | creaky wooden gate and bolts | GoatsheadCastle | no |
| hand_loom_582994.mp3 | 21.6 | Irish hand loom, shuttle clack | snapcam | no |
| kaval_wooden_flute_325111.mp3 | 96.5 | Balkan kaval solo | EKVelika | no |
| market_crowd_medieval_424790.mp3 | 9.6 | crowded street at a re-enactment market | bolkmar | no |

"No" in the last column means soundscape.json does not name it. The older `LevelSoundscape.cs` holds clips set in the scene file
(its fields `dayBirds`, `waterBirds`, `nightBirds`, `thunderNear`, `thunderFar`, crickets, lines 22-35), so a few "no" files may still be wired in L_Dorestad. Not checked.

## 3. Credited, on G:, but not in the project

In `G:\kapman_unity\data\sounds_cc0\` (with `manifest.json`) and credited in CREDITS_sounds.md lines 5-78, but no file under Assets:

- Ship: hull_creaking_265553 (moored sailboat interior, strong breeze), hull_creaking_195193 ("LargeWoodenShip"),
  ropes_straining_145721 (mooring rope), sail_flapping_448975 (a flag), sail_flapping_554439 (sailboat sail, summer insects behind),
  rain_on_the_sail_117626 (tent, wind and rain), oars_588307 (rowing boat from inside),
  water_past_the_hull_while_sailing_371169, stone_anchor_splash_583348 and _737644, footsteps_on_planks_521589 and _463669.
- Sea: background_waves_412308, surf_on_a_beach_426076 and _339517.
- Wind: gale_463373 ("storm wind whistling through harbour boat masts"), gale_181935, strong_wind_712091.
- Rain: heavy_rain_200272, light_rain_96729.
- Birds: herring_gull_818829 and _538016, oystercatcher_350569, raven_437620.
- Town: cattle x2, chickens x2, dogs x2, horses x2, pigs x2, sheep x2, fire_588401, smithy_321889.

The 40 added on 4 October are in `G:\kapman_unity\data\sounds_cc0\new\` with `CREDITS_new.csv`; all 40 are in Assets.

## 4. How the game chooses what to play

Data: `Assets/Kapman/Audio/soundscape.json` (31 layers). Code:
`Core/Audio/SoundContext.cs` (the values), `Core/Audio/SoundscapeDef.cs` (the loader), `Core/Audio/SoundscapeModel.cs` (the levels),
`Runtime/Audio/LevelSoundscape2.cs` (the sources), `Runtime/Audio/SoundVoices.cs` (bird calls),
`Editor/Audio/SoundscapeSetup.cs` (puts it in a scene). Older, simpler: `Runtime/Levels/LevelSoundscape.cs`.

Each frame the code fills a SoundContext (SoundContext.cs lines 9-31) and every layer reads it:

- Weather: wind in m/s and as Beaufort "force", rain 0-1, storm, temperature, humidity, cloud.
- Hour: `hour`, `night` 0-1, `sun` (the Sun's height in degrees). Season: `day` of the year, `leaf` (leaves on trees).
- Place: height above water, signed distance to the shore, and what lies round the listener
  (reed, hedge, water, basin, town, wood, field as 0-1 shares).
  These shares are GUESSED from the shore distance today (SoundContext.cs lines 22-26); the real cover map was never made.
- Ship: speed, roll rate, pitch rate, rowing 0-1, distance to the listener.

Kinds of layer (SoundscapeDef.cs lines 36-43):
- crossfade: clips set along a value. wind by force (breeze at 1, strong at 7); rain by amount (0.1 / 0.45 / 0.9);
  ship_bow by speed (lapping at 0.3 m/s, bow wave at 3); ship_oars by speed (0.6 / 1.6).
- bed: one loop with conditions. lapping near water and shore; reeds, leaves, bare_trees, dry_grass by wind and surroundings and season;
  rain_thatch in town, rain_water over water; ship_wake when rowed; ship_creak by roll and pitch.
- event: thunder on a lightning flash, late by the speed of sound, far ones low-passed, near ones duck the rest.
- voice: 14 bird voices fired at random waits from a random spot, each with its season (`day` window), hour (`sun` or `night`) and place.
  "mask": "wildlife" silences them in a near gale and thins them in rain.
- group: dawn_dusk_chorus and night_chorus keep at least one member calling.

So it chooses by weather, hour, season, the ground round you and what the ship does. It does NOT choose by port:
there is no port, region or sea name in the context. Dorestad and Ripa would sound alike.

Scenes that have it: `Assets/Kapman/Scenes/L_Dorestad.unity` and `L_NorthSea.unity` only (3D levels).

## 5. Does the chart scene play any of it today? No.

- `Assets/Kapman/Scenes/L_Chart.unity` has an AudioListener (line 534) and nothing else: no AudioSource, no LevelSoundscape, no LevelSoundscape2.
- Nothing under `Assets/Kapman/Code/Game` (the Frame HUD included) mentions AudioSource, AudioClip or Soundscape.
- LevelSoundscape2 needs a LevelWeather, a clock, a shore map and a ShipMotion from a 3D level. The chart has none of them as scene objects.
- To get sound in the chart or the ship view, something must fill a SoundContext from the game's own state
  (wind, rain, hour, day, ship speed, rowing, in port or at sea). The json and the layer code can be reused as they are.

## 6. What the ship view needs: have / missing

| need | have today | state |
|---|---|---|
| wind, light | light_breeze_139337 (grass and trees) | wrong place: land wind. MISSING an open-water one |
| wind, fresh | none (the crossfade just mixes light and strong) | MISSING |
| wind, gale | strong_wind_215853 is dry grass in Texas; gale x2 on G: only | MISSING for sea; gale_463373 is modern masts, likely unusable |
| sea on hull, calm | calm_water_lapping_the_hull_851577 | from inside a modern cabin; check by ear, likely boomy. Weak |
| sea on hull, swell | water_past_the_hull_360631 (bow wave) | bow wave yes; slap and thump of swell on planks MISSING |
| open sea bed | background_waves, surf x2 on G: only | all three are shore surf, not open sea. MISSING |
| rain on wood | none (rain_on_tent is canvas) | MISSING |
| rain on water | rain_on_pond_399281 | pond, 38 s. Works for calm; rain on sea MISSING |
| rain on the sail | rain_on_the_sail_117626 on G: only (a tent) | bring in and judge |
| oars in tholes | oars_234361, oars_paddles_calm_249707 | both small modern boats; paddles, not a thole-and-grommet knock. Weak |
| the pull (many oars together) | none | MISSING |
| sail filling | none | MISSING |
| sail flapping | sail_flapping x2 on G: only (one is a flag, one has insects) | bring in and judge; likely MISSING for a heavy wool sail |
| timber creak | ship_creaks_sloshing_450255; hull_creaking x2 on G: | one in use, below deck with sloshing baked in. Need a dry creak alone |
| rope under load | ropes_straining_145721 on G: only | bring in and judge |
| harbour: water on piles | wake_under_boardwalk_758858 (not wired) | have; birds behind it |
| harbour: gulls | black_headed_gull_554806; herring_gull x2 on G: | have one; herring gulls not in project |
| harbour: voices far off | market_crowd_medieval_424790, 9.6 s (not wired) | too short to loop unheard; modern re-enactment, words may be heard. MISSING |
| night at sea | none | MISSING |
| night in harbour | night_breeze_village_637147 (not wired), tawny_owl, nightingale | land night only. Harbour night MISSING |

## 7. The list to go and find

Rule for all: a real recording, CC0 or public domain, source written into CREDITS_sounds.md the day it comes in. Never synthesised, never "designed" from layers by the uploader.
Aim for 60 s or more of steady sound so a loop does not show. Mono or stereo both fine.
Ruins anything: an engine or generator far off, road hum, aircraft, a phone, modern speech, halyards clanking on metal masts, wind buffeting the microphone, music, an uploader's added reverb or EQ, the word "designed", "synth" or "generated" in the description.

1. Wind, light (force 1-3) over open water.
   Would do: a quiet lake or estuary shore on a breezy day, a furry windshield, no trees near, no birds close.
   Ruins it: leaves or grass in it (it is then land), mic rumble, a distant road.
2. Wind, fresh (force 4-6) at sea or on a bare coast.
   Would do: steady wind on a headland or dunes with no surf close, a body to it without howling.
   Ruins it: whistling in wires or fences, surf louder than the wind, gusts that clip.
3. Wind, gale (force 7-9).
   Would do: a gale on open moor or cliff top, a low roar with gust swells.
   Ruins it: rigging whine and halyard clank (modern masts), rain mixed in (we layer rain ourselves), a building rattling.
4. Sea against a hull, calm.
   Would do: mic on deck or over the side of a WOODEN boat at anchor or drifting; small slaps and chuckles on planks.
   Ruins it: the hollow boom of glassfibre or steel, fenders squeaking, a bilge pump, cabin interior tone.
5. Sea against a hull, swell.
   Would do: a wooden boat under sail in a seaway: the thump of the bow, the wash running aft, spray. Recorded on deck.
   Ruins it: an engine or prop wash, winches, sheets on metal blocks, crew talk.
6. Open sea bed (no shore).
   Would do: recorded from a boat well offshore or from a high cliff: a broad wash with no single breaking wave.
   Ruins it: beach surf with a rhythm, shingle rattle, gull colony, boat motor.
7. Rain on wood.
   Would do: rain on a plank deck, a wooden jetty, a shed roof of boards or an upturned wooden boat, close.
   Ruins it: tin or plastic roof ping, gutters and drainpipes, cars passing on a wet road.
8. Rain on open water, light and heavy.
   Would do: rain falling on a lake or harbour from a boat or a jetty end, no shelter over the mic. Two strengths.
   Ruins it: drips off a roof or umbrella near the mic, thunder baked in, traffic hiss that sounds like rain.
9. Rain on a sail.
   Would do: rain on heavy canvas or a wool blanket stretched taut, from below. The tent ones may serve.
   Ruins it: nylon or polyester tent crackle (thin, plasticky), zips, a groundsheet.
10. Oars in tholes.
    Would do: a traditional clinker boat with thole pins or kabes and rope grommets (faering, Shetland yoal, a Viking Ship Museum replica): the wooden knock and the rope creak, close.
    Ruins it: metal rowlocks clicking, sliding seats, a coxswain calling, plastic paddles.
11. The pull (many oars together).
    Would do: a replica longship or a gig crew rowing, from on board: the catch, the surge of water, the breath of the stroke, 8 or more oars.
    Ruins it: shouted modern orders, cheering crowds, a safety boat engine, a race commentary.
12. A sail filling.
    Would do: a square sail or a gaff sail of canvas being sheeted home; the soft whump as it takes the wind. Traditional boats.
    Ruins it: the crack of Dacron or Mylar (bright and papery), winch ratchets, a metal boom.
13. A sail flapping (luffing).
    Would do: heavy canvas or a tarpaulin luffing in a breeze, low and thuddy; an awning or old tent in wind may do.
    Ruins it: a thin flag snapping, halyard on a pole, insects or birds baked in (554439 has insects).
14. Timber creak, alone.
    Would do: a wooden ship working at her mooring or under sail with no water in it; an old barn, mill or timber frame creaking in wind is as good.
    Ruins it: sloshing baked in, door hinges (a different creak), footsteps, electric hum below deck.
15. A rope under load.
    Would do: natural-fibre rope (hemp, manila) tightening round a wooden post or bollard, the dry groan and tick.
    Ruins it: nylon squeak, chain, a metal cleat or shackle, rubber fender squeal.
16. Harbour: water on piles.
    Would do: small waves among wooden piles or under a timber jetty at a quiet time. wake_under_boardwalk_758858 may serve once the birds are judged.
    Ruins it: concrete quay slap (hard and hollow), moored boats knocking, masts, a distant ferry.
17. Harbour: gulls.
    Would do: herring gulls and black-headed gulls over a shore, some near and some far, single calls we can cut out.
    Ruins it: town behind them, children, traffic. (Bring in herring_gull_818829 and _538016 from G: first.)
18. Harbour: voices far off, no words.
    Would do: a crowd at 50 m or more outdoors, a murmur with no word to catch, at least 60 s; a livestock market or a fair in another language recorded far enough off.
    Ruins it: any word you can make out, English or Dutch, laughter close to the mic, a PA or music, coins and tills, children's toys, applause.
19. Night at sea.
    Would do: a quiet anchorage at night from a wooden boat: slow water, a small creak now and then, air. Nothing else.
    Ruins it: crickets (wrong at sea, and retired here already), a generator, an anchor chain, a harbour hum, a foghorn.
20. Night in harbour.
    Would do: a small waterside village at night, water lapping, a dog far off once, wind low.
    Ruins it: any mains hum, a road, a clock tower with a modern chime, church bells in peals (793 has single small bells).

Also to settle, not to find:
- Bring the ship recordings on G: (section 3) into Assets and judge each by ear before hunting for more.
- The five sounds in use for the ship today are all from modern boats (cabin interior, small rowing boats). Listen for glassfibre tone.
- No layer names a port or a sea. If ports should differ (Ripa against Lundenwic), the context needs a place value.
