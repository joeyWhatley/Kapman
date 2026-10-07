# Hyper-specific audio: the layered soundscape

Design document, 4 October 2026, revised the same day after review. For Kāpman 2 (Unity 6000.6.3f1, HDRP 17.6). Rule of 2 October: public-domain or CC0 recordings only, never made or synthesised; editing (trimming, looping, layering, filtering) real recordings is allowed, making sounds from nothing is not. No music unless the user says otherwise (environment plan, 2 October).

What this replaces: `Assets/Kapman/Code/Runtime/Levels/LevelSoundscape.cs` (110 lines: six hard-coded loops, one bird voice, thunder at 340 m/s) and, later, the `AddSound` point sources that `Editor/DorestadTown.cs` drops next to smithies, the bell and hearths (lines 717, 925, 1139; `AddSound` at 1594; `DorestadBuilder.Sounds()` at 620). What it keeps: every clip and credit line, the M key, the AudioSources the builder already places (the first slice drives them instead of replacing them), and the rule that the final level of every layer is tuned by ear, from a physical starting point (2.2).

Sections: 1 What is true · 2 The model · 3 In the game · 4 Build plan · 5 First slice · 6 Open questions.

---

## 1. What is true

### 1.1 Wind, sea state and what you hear at each force

The Beaufort scale ties wind speed to the sea's look and to what the land sounds like. The wave heights are "probable" open-sea heights; in the harbour at Dorestad, 100 km inland, the water never gets past a chop (Wikipedia, Beaufort scale: https://en.wikipedia.org/wiki/Beaufort_scale).

| Force | Name | Wind (m/s) | Open-sea wave (m) | Sea | Land, and what the ear gets |
|---|---|---|---|---|---|
| 0 | Calm | 0–0.2 | 0 | like a mirror | smoke rises straight; you hear everything: drips, a far dog, your own hull |
| 1 | Light air | 0.3–1.5 | 0–0.3 | ripples, no foam | smoke drifts; no wind noise at the ear |
| 2 | Light breeze | 1.6–3.3 | 0.3–0.6 | small wavelets, glassy crests | wind felt on the face, leaves rustle; first reed hiss |
| 3 | Gentle breeze | 3.4–5.4 | 0.6–1.2 | large wavelets, crests begin to break | leaves and twigs in constant motion; a slack sail stirs |
| 4 | Moderate breeze | 5.5–7.9 | 1–2 | small waves, white horses | dust and loose straw lift; the sail fills with a thump; chop slaps the hull |
| 5 | Fresh breeze | 8–10.7 | 2–3 | moderate waves, many white horses | small trees sway; wind noise starts to mask far sounds |
| 6 | Strong breeze | 10.8–13.8 | 3–4 | large waves, foam crests, spray | large branches move; the stays begin to sing (the land scale's "whistling in wires") |
| 7 | Near gale | 13.9–17.1 | 4–5.5 | sea heaps up, foam in streaks | whole trees move; a steady roar in the ears |
| 8 | Gale | 17.2–20.7 | 5.5–7.5 | moderately high waves, spindrift | twigs break off; everything but the sea and the rigging is masked |
| 9 | Strong gale | 20.8–24.4 | 7–10 | high waves, dense foam, crests topple | slight damage to roofs (thatch lifts) |
| 10 | Storm | 24.5–28.4 | 9–12.5 | very high waves, overhanging crests | trees uprooted |
| 11 | Violent storm | 28.5–32.6 | 11.5–16 | exceptionally high waves | widespread damage |
| 12 | Hurricane | ≥ 32.7 | ≥ 14 | air filled with foam and spray | devastation |

The storms plan (Plans/storms.md, step S1) will give the sea its real state (height, period, direction, from the wind's fetch and duration, falling slowly after). Until then the audio takes the wave height from this table. Note for tuning: the present weather generator never passes 16 m/s (B7): `LevelWeather`'s Storm target is `wind = 16f` (LevelWeather.cs, Targets, line 230), so B8–B9 cannot be heard in Play until storms S1/S2 land; see 5.

What the ear actually hears of wind: a recording of "wind" is mostly the microphone's own buffet. Outdoors the ear hears wind through what it blows across (reeds, leaves, grass, rigging, the ear's own turbulence), not as a broadband bed. That is a rule of the model (2.3): the pure WIND bed stays low and the loudness of a gale comes from the VEGETATION bed on land and the rigging and the sea at sea.

### 1.2 Sound in air

- **Speed of sound:** c ≈ 331.3 + 0.606·T m/s (T in °C): 331.3 at 0 °C, 337.3 at 10 °C, 343.2 at 20 °C. Humidity adds at most 0.1–0.6 % (Wikipedia, Speed of sound: https://en.wikipedia.org/wiki/Speed_of_sound). Over 6 km, the difference between a frost and a summer evening is about 0.6 s of thunder delay. The current code uses 340 m/s flat.
- **Thunder:** the US National Weather Service: thunder "can be heard for a distance of only about 10 miles" (16 km); near strikes give "a sharp crack or click", far ones "more like a rumble"; count seconds and divide by five for miles (https://www.weather.gov/safety/lightning-science-thunder). So the rumble is the real signal of distance, not only the delay. The present generator only ever fires `Lightning` at 300–6,300 m (LevelWeather.cs line 196); real strike distances wait on the storms plan.
- **Refraction:** sound bends up and away from a listener when the air cools with height (the normal daytime case), making an acoustic shadow; it bends down, and carries, when the ground is colder than the air above it (a clear calm night, a frosty morning). A wind gradient does the same: downwind carries, upwind is shadowed; a wind shear of 4 m/s per km equals a 7.5 °C/km temperature gradient (Wikipedia, Speed of sound, refraction section, as above). This is why a frosty calm morning on open water is the loudest-at-distance condition in the game: you hear the bell across the Lek from the dams, and a ship's oars across the whole harbour.
- **Air absorption:** ISO 9613-1:1993 gives the attenuation of sound by the air for 50 Hz–10 kHz, −20 to +50 °C, 10–100 % humidity (Sengpiel's calculator page, which cites the standard: http://www.sengpielaudio.com/calculator-air.htm). Absorption rises steeply with frequency; at 10 °C and 80 % humidity it is of the order of a few dB/km at 1 kHz, tens of dB/km at 4 kHz and near 100 dB/km at 8 kHz (guess, from memory of the ISO tables; the build plan computes the real coefficient from the standard's formula). This is why far sources sound dull: a distance low-pass is physics, not a stylistic choice. The formula needs a relative humidity; `LevelWeather` has none, so the model derives one from the weather kind (2.1).
- **Snow:** fresh fluffy snow absorbs sound at the ground ("dampening sound"); old, melted-and-refrozen snow reflects it, so sounds "travel farther and seem clearer"; snow crunches underfoot from ice grains rubbing, louder the colder it is, going silent near the melting point (NSIDC, Science of snow: https://nsidc.org/learn/parts-cryosphere/snow/science-snow).
- **Water sounds are bubbles:** a bubble of radius a in water rings at f·a ≈ 3.26 m/s (Minnaert resonance, Wikipedia: https://en.wikipedia.org/wiki/Minnaert_resonance): a 1 mm bubble rings at 3.3 kHz, a 1 cm bubble at 330 Hz. Every lap, slap, gurgle and raindrop on water is a cloud of these. A bow wave is many small bubbles (bright hiss); water gulping under a dam's planking is big bubbles (low "glonk"). The underwater peak of rain on water near 14 kHz from the smallest drops (Pumphrey and Crum, 1980s) is from memory (guess) and not needed for the mix; what matters is that rain on water is bright and hissy and rain on thatch is dull and soft.
- **Source levels.** Sound pressure levels at a stated distance, which the gain law (2.2) needs: a riveting machine 110 dB and "bird calls" 44 dB (distance not given) (Purdue noise-level table: https://www.chem.purdue.edu/chemsafety/Training/PPETrain/dblevels.htm); a jackhammer 100 dB at 1 m, normal conversation 40–60 dB at 1 m, a very calm room 20–30 dB (Wikipedia, Sound pressure, examples table: https://en.wikipedia.org/wiki/Sound_pressure); a whisper or rustling leaves 20 dB (Purdue, as above). Every other figure in the emitter table (2.4) is an estimate anchored to these: a hammer on an anvil is a riveting machine's cousin (105–110 at 1 m), a cockerel and a barking dog are of the order of a shout to a jackhammer close to (90–100 at 1 m), a nightingale at 1 m is a loud songbird (85–90), a crackling fire or a well bucket is conversation level (60–70). Marked (estimate) in the table; the one global knob (2.2) absorbs any common error, and the relative order is what the mix depends on.

### 1.3 The town's sounds c. 800

- Churches. The excavated vicus had no church; a first wooden church was built c. 800 in the Frankenhof cemetery (south), and building H29A once read as a church with a bell-cage is now left open (Research/dorestad_c800_notes.md, row "Churches"). But the 777 charter's church of Saint Martin, "Upkirika" (Bovenkerk, "built upriver from Dorestad"), is placed inside the old castellum area across the Lek, south of the fork near Rijswijk, with the only true church-cemetery of phase 4; the villa churches of Rijswijk and Leut lay beyond (dorestad notes, row 37, rated "likely"; row 107). The bake marks that zone: `RIJSWIJK = (3600, 800, 300)` in `Bake/levels/make_dorestad.py` (level metres, a 300 m clearing), about 1.25 km south-east of the port point (meta.json `port` = (2822.6, 1781.4)). So in 793 there is a church within hearing across the Lek, and from c. 800 a second, nearer one at the Frankenhof. Whether either had a bell is unknown; a hand bell at Upkirika from 793 is as likely as one at the Frankenhof from 800, and it gives the player a period-true "navigation by ear" cue from the first day (guess, both).
- Bells of the period. Larger cast bells were made at the end of the 7th and in the 8th century; in northern Europe the early common form was the quadrangular sheet-metal (riveted) bell of Irish tradition (Wikipedia, Church bell: https://en.wikipedia.org/wiki/Church_bell; the Catholic Encyclopedia's "Bells" entry describes the early Celtic hand bells including St Patrick's bell, https://www.newadvent.org/cathen/02418b.htm). An 8th-century riveted iron hand bell clanks with a short decay; it does not toll. It is closer to a cowbell or a goat bell than to a cast church bell, which is why the library's cast bells are the wrong sound (1.6). The same Catholic Encyclopedia entry quotes Charlemagne's capitularies as decreeing "ut cloccas non baptizent" (that they should not baptise bells), undated there; the Admonitio generalis of 789 is real (https://en.wikipedia.org/wiki/Admonitio_generalis) but whether that is the capitulary meant is unverified. Nothing in the model depends on it; it is colour for the reading texts.
- 18 smithies, 651 wells, 305 granaries, 409 houses, 37 farms with kraals, strip fields, four cemeteries, dams 40–200 m long with gangways and bollards, fire baskets at 37 dam heads (PROGRESS.md). Cattle hoof-prints in the Lek silt at the levee edge: the basin margins are pasture (dorestad notes, row 56). Plots were fenced with wattle; hedges of hawthorn or gorse (row 88).
- Ships: a 30 m longship with 65 crew carried a 120 m² square sail (Wikipedia, Sea Stallion from Glendalough: https://en.wikipedia.org/wiki/Sea_Stallion_from_Glendalough). The player's boat is far smaller (ShipMotion: 7.7 m by 1.5 m, draught 0.45 m, a few men). A rowing cadence of 22–30 strokes a minute at 1.6 m/s is a guess from modern replica rowing (guess).

### 1.4 The wildlife calendar: the 793 roster

This is the roster of the Rhine delta in 793, not today's. Dates are present-day Dutch dates (Vogelbescherming Nederland, the Dutch bird society), the best available stand-in; the climate of 793–1066 was on the whole similar to or a little warmer than today's (guess). Species marked **[gone, back]** were lost from the Netherlands after 793 and have returned; **[gone]** have not; they are in the roster because the game is set in 793, and the present-day book cannot give them. What made the delta loud and is missing from any modern Dutch night: corncrakes rasping all night in the floodplain hay meadows, snipe drumming over the wet basins, marsh and pool frogs by the thousand in the ditches, cranes breeding in the marsh woods, storks clattering on the roofs, beavers slapping the river at night, and the farmyard's geese.

| Species (sound) | When heard | Time of day | Where | Source |
|---|---|---|---|---|
| Corncrake (rasping "crex crex", two syllables, "like a nail file") **[nearly gone: 50–60 pairs in 2025, thousands a century ago]** | arrives early May, leaves from August; calls through the breeding season May–July | night, above all | "hooilanden in uiterwaarden van de grote rivieren": the floodplain hay meadows; carries "tot op ruim een kilometer" in still air | https://www.vogelbescherming.nl/ontdek-vogels/kennis-over-vogels/vogelgids/vogel/kwartelkoning |
| Snipe (drumming, "hemelgeit": air through the spread outer tail feathers in a dive; also the "tick-a" call) | breeding April–June (laying April–June) | dusk and dawn, also night | wet, extensively used peat grassland with mud: the basins | https://www.vogelbescherming.nl/ontdek-vogels/kennis-over-vogels/vogelgids/vogel/watersnip |
| Marsh frog ("lachend gekwaak", the laughing chorus, far louder than the common frog's purr) and pool frog (the "sonore" purr-chorus) | May–June is the counting season for calling males; calls from about 18 °C water | warm evenings and nights, also by day in May | ditches, pools, the basins' standing water; today mainly the west and far north, but native, so in the 793 delta's basins (guess) | https://nl.wikipedia.org/wiki/Meerkikker ; https://en.wikipedia.org/wiki/Marsh_frog |
| Common crane, BREEDING ("luid getrompetter"; pairs duet at the nest site; flocks call on take-off and landing) **[gone, back: breeds again only since 2001 "na eeuwen van afwezigheid"]** | breeding April–July in "uitgestrekte moerasbossen en hoogvenen" (the Langbroek peat and the carr); on passage late Feb–early March and late Oct–Dec | dawn duets; day and night on passage | the big marsh behind the carr; passing over in V-lines | https://www.vogelbescherming.nl/ontdek-vogels/kennis-over-vogels/vogelgids/vogel/kraanvogel |
| White stork ("klepperen met snavel op nest": bill clattering, no voice) **[gone, back: nearly vanished by the mid-1970s, reintroduced from 1969, 1,900–2,200 pairs in 2025]** | arrives April, breeds April–August | day, at the nest | on roofs and tall trees in the town and the farms | https://www.vogelbescherming.nl/ontdek-vogels/kennis-over-vogels/vogelgids/vogel/ooievaar |
| Beaver (tail slap on the water, a loud flat crack; gnawing) **[gone, back: "completely extirpated by 1826", reintroduced since 1988]** | all year | night, dusk | the river banks, the carr's channels | https://en.wikipedia.org/wiki/Eurasian_beaver |
| Domestic goose (the loudest yard animal; honking, hissing at the ship's crew) | all year | day; alarm at night | every farmyard | kept everywhere in the period (guess, by the bone finds of every northern vicus); clip: Freesound CC0 fthgurdy 528945 "Geese honking around a farmyard" |
| White-fronted goose (high yodelling cackle, skeins) | early October to March | dawn and dusk flights, grazing by day, restless by night | wet meadows, basins | https://www.vogelbescherming.nl/ontdek-vogels/kennis-over-vogels/vogelgids/vogel/kolgans |
| Greylag goose (deeper honk) | all year, flocks Sept–March | dawn, dusk | reed, meadows | clip notes in CREDITS_new.csv (geese_flock_landing_484825) |
| Nightingale (loud song with pauses) | arrives mid-April, breeds from mid-May, leaves July–mid-Sept (song mid-April to early June) | night and day, loudest at night | scrub, hedges, carr edge | https://www.vogelbescherming.nl/ontdek-vogels/kennis-over-vogels/vogelgids/vogel/nachtegaal |
| Cuckoo ("koe-koek") | arrives mid-April, calls from May, adults leave June/July | day, dawn | carr, hedges | https://www.vogelbescherming.nl/ontdek-vogels/kennis-over-vogels/vogelgids/vogel/koekoek |
| Bittern (low deep boom) | April–May, "on fine evenings and mornings in spring"; needs old standing reed; suffers in hard winters | dusk, dawn, night | big reed beds | https://www.vogelbescherming.nl/ontdek-vogels/kennis-over-vogels/vogelgids/vogel/roerdomp |
| Skylark (continuous song aloft) | Feb–July (clip: dunes) | day | fields, dunes, salt marsh | library (skylark_387426, 635912); season dates (guess) |
| Blackbird (evening song) | March–July | dusk, dawn | town, hedges | library (blackbird_evening_517816, April) |
| Swallow (twitter) | April–Sept | day, evening over water | farms, water | library (swallows_farm_nl_819880, a Dutch farm) |
| Tawny owl (hoot, kewick) | all year, loudest autumn–winter | night | woods, town edge | library (tawny_owl_203598, 688869); autumn peak (guess) |
| Rook (rookery din), carrion/hooded crow | rookery March–May; crows all year | day, roost at dusk | tall trees, fields | library (rooks_nesting_434847, crow_741366, crow_carrion_call_181088) |
| Grey heron (harsh "fraank") | all year | dawn, dusk, when flushed | shoals, pools | library (heron_call_nest_825293, heron_river_park_834209) |
| Mallard (quack) | all year | day, flushed | slack water | library (mallard_quack_flying_456770) |
| Black-headed gull (colony chatter), herring gull | colony April–July; gulls all year, follow boats | day | harbour, flats | library (black_headed_gull_554806, herring_gull_538016, 818829) |
| Curlew, oystercatcher (flats) | all year on the flats, most Aug–March | day, tide-driven | estuaries, mud, sand bars | library (curlew_475971, 829120; oystercatcher_350569) |
| Raven (croak) | all year | day | beaches, cliffs, battlefields | library (raven_437620) |
| Field cricket (chirp) | May–July, only above 13 °C | daytime and the first part of the night | dry sunny short grass, heath: NOT wet river meadows; red-listed and declining in NL | https://en.wikipedia.org/wiki/Gryllus_campestris |
| Grasshoppers (meadow buzz) | July–Aug | warm afternoons | hay meadows, dry levee tops | gap; Freesound CC0: bruno.auzet 529670 (Brittany, July), 538794, Kamix0 397679 |
| Common frog (low purring chorus) | spawning late Feb–late June, mostly April | night, mild wet evenings | pools, ditches | https://en.wikipedia.org/wiki/Common_frog |
| Red deer (roar in the rut) | rut August into early winter; loudest late Sept–Oct (guess); "most common during the early dawn and late evening" | dawn, dusk, night | woods, heath beyond the carr | https://en.wikipedia.org/wiki/Red_deer |
| Wolf (howl) **[gone]** | all year; winter nights carry (guess) | night | far woods; rare, a chronicle/famine event | gap; Freesound CC0 real recordings: betchkal 500646, ondrosik 143568 |
| Bees (hum) | April–Sept, sunny days | day | hives, lime trees, heather | gap; Freesound CC0: simongray 372595, kiwimana 341639 |
| Cockerel | all year | first light, then scattered | every yard | gap; Freesound CC0: Garuda1982 627335 ("rooster crows in the village"), JoMungus 719773 |

The night wet-meadow bed of the basins is corncrake + frogs + snipe + bittern (May–June), which is what the basins sounded like; the model (2.3) treats it as one voice group so the four never fall silent together on a mild May night.

The current `night_crickets_522299` loop (summer nights, days 150–250, in the code) is the wrong animal for the Rhine delta: field crickets live on dry heath and sing by day and early night in May–July. It is retired at Dorestad; the dry fields get a daytime grasshopper layer for July–August and the wet basins get the bed above.

### 1.5 Loudness and Unity's audio

- EBU R 128: programme loudness −23 LUFS ±0.5 LU, true peak −1 dBTP (Wikipedia, EBU R 128: https://en.wikipedia.org/wiki/EBU_R_128). Console game guidance (Sony ASWG-R001, −24 LUFS) is from memory (guess); −23 LUFS integrated over ten minutes of ordinary play, −1 dBTP, is the target here. Unity's Audio Mixer meters show peak and RMS in dB, not LUFS, so they only guard the −1 dBTP ceiling; the LUFS figure is measured on a 10-minute capture with ffmpeg's `ebur128` filter (listed in the filter documentation, https://ffmpeg.org/ffmpeg-filters.html#ebur128; not installed on this machine yet) or a free LUFS meter in a DAW.
- Unity's AudioLowPassFilter: cutoff 0–22,000 Hz (default 5,000), resonance Q 1–10, with a rolloff curve so cutoff can follow the distance between source and listener; it can sit on an AudioSource or on the AudioListener (https://docs.unity3d.com/6000.0/Documentation/Manual/class-AudioLowPassFilter.html).
- AudioReverbZone: a min radius (full reverb) and max radius (none), preset reverbs, driven by the listener's position; zones mix (https://docs.unity3d.com/6000.0/Documentation/Manual/class-AudioReverbZone.html).
- Audio Random Container (Unity 6): a playlist asset an AudioSource plays, in Manual mode (Play picks the next clip by the container's playback mode) or Automatic (it triggers itself on a timer) (https://docs.unity3d.com/6000.0/Documentation/Manual/AudioRandomContainer.html, …/AudioRandomContainer-fundamentals.html). Good for bird calls, slaps, hammer blows.
- The project's audio settings (`ProjectSettings/AudioManager.asset`): DSP buffer 1024 samples, 32 real voices, 512 virtual voices, default speaker mode stereo, no spatializer plugin.

### 1.6 The library: what exists

`G:\kapman_unity\data\sounds_cc0`: 63 clips in the root (CREDITS_sounds.md, manifest.json) and 40 in `new\` (CREDITS_new.csv): **103 clips**, not 105 (the two credit files were counted and the mp3s counted; 63 + 40). All CC0 from Freesound; every one has a credit line with a URL. **65** of them are already imported into `Assets/Kapman/Audio/Sounds` (counted: 65 .mp3 files) and used by the Dorestad builder.

Every file in the library is a Freesound "preview hq" MP3 (128 kbps): the 40 new ones say so in their CREDITS_new.csv notes, and the root 63 are the manifest's .mp3 files. Unity then re-encodes to Vorbis: two lossy passes on the layer the brief calls "hyper-specific", when the original WAV or FLAC (for example 476464 is 96 kHz/24-bit, 513978 is 48 kHz/16-bit, 350713 is 48 kHz/16-bit) is one free-account download away. The import rule (4.1) prefers an original beside each preview and the manifest records which clips are still previews; fetching the originals for the beds and the ship (the clips heard longest) is a daytime data job, not a build night.

Clip-to-layer map (layer names are those of section 2.3):

| Layer | Clips (root) | Clips (new) |
|---|---|---|
| WIND bed | light_breeze_139337, light_breeze_620099, strong_wind_215853, strong_wind_712091, gale_181935, gale_463373 | night_breeze_village_637147, winter_wind_bare_trees_637566 |
| VEGETATION bed | — | reeds_wind_411847, reeds_wind_shore_844604, wind_leaves_spring_575645, wind_dry_grass_845415 |
| WATER bed (river/shore/sea) | calm_water_lapping_the_hull_851577, surf_on_a_beach_339517, surf_on_a_beach_426076, background_waves_412308 | river_lapping_bank_318064, ripples_lake_shore_464801, wake_under_boardwalk_758858, weir_small_560723 |
| RAIN bed | light_rain_96729, light_rain_200273, heavy_rain_125119, heavy_rain_200272, rain_on_the_sail_117626 | rain_soft_loop_595717, rain_on_pond_399281, rain_on_tent_397916 (thatch stand-in), hailstorm_thunder_620489 |
| THUNDER events | close_thunder_347853, close_thunder_574412, distant_thunder_107518, distant_thunder_361401 | — |
| ICE events | — | ice_lake_fractures_737896 |
| SHIP loops | hull_creaking_195193, hull_creaking_265553, ropes_straining_145721, sail_flapping_448975, sail_flapping_554439, water_past_the_hull_while_sailing_360631, 371169, oars_234361, oars_588307 | ship_creaks_sloshing_450255, oars_paddles_calm_249707, wake_rowing_boat_588308 |
| SHIP events | stone_anchor_splash_583348, 737644, footsteps_on_planks_463669, 521589 | — |
| WILDLIFE voices | black_headed_gull_554806, herring_gull_538016, 818829, curlew_475971, 829120, oystercatcher_350569, geese_407490, crow_741366, raven_437620, skylark_387426, 635912, tawny_owl_203598, 688869, night_crickets_522299 (retire at Dorestad) | geese_flock_landing_484825, geese_wild_meadow_535067, rooks_nesting_434847, crow_carrion_call_181088, swallows_farm_nl_819880, heron_river_park_834209, heron_call_nest_825293, mallard_quack_flying_456770, blackbird_evening_517816, nightingale_england_632259 |
| TOWN emitters | smithy_254370, smithy_321889, fire_454745, fire_588401, bell_one_toll_554655, cattle_163727, 513565, chickens_197103, 553068, dogs_440865, 832435, horses_416873, 538438, pigs_158746, 692838, sheep_200329, 210511 | smithy_bellows_anvil_609757, hearth_fire_crackling_707262, bonfire_night_364992, hand_saw_log_760611, axe_chopping_tree_573812, well_bucket_rope_649240, footsteps_mud_path_449653, wooden_gate_bolts_741621, church_bell_village_321627, hand_loom_582994, market_crowd_medieval_424790, kaval_wooden_flute_325111 (music: held until the user says) |

Clips with problems: `night_crickets_522299` (wrong habitat); `oystercatcher_350569` has thunder in it (cut the thunder out, or use it only as a storm-on-the-flats bed); `curlew_829120` is a pink-footed goose collage with curlew in it; `geese_flock_landing_484825` has Canada geese (not in 793 Europe: cut to the greylag passages); `surf_on_a_beach_339517` is 26 minutes (1,565 s in manifest.json; trim to a 90 s loop); `hull_creaking_195193` is 10 minutes (600 s; trim); `bell_one_toll_554655` and `church_bell_village_321627` are cast bells, the wrong instrument for a riveted 8th-century hand bell. The fix is a different recording, not a pitch shift: pitching a cast bell down makes a bigger cast bell, the opposite of a small iron hand bell, and it bends the no-synthesis rule further than a cut does. Freesound CC0 has real single strikes of sheet-iron animal bells, which is what a riveted hand bell is: RP1312 350713 "Cowbell - Tic 03" (an authentic north-Italian cowbell, one strike, 0.5 s, 48 kHz/16-bit WAV, https://freesound.org/people/RP1312/sounds/350713/), and under "goat bell": fmiramar_ 397349 "Bell0005", skoul 546844 "goatbell", both to audition for single clean hits. The bell emitter uses one of these, unpitched; if none is clean enough, the lower cast bell untouched, and the credits say so.

### 1.7 The gaps and where CC0 recordings exist

Freesound searches with the CC0 licence filter, 4 October 2026 (the search ANDs every word; single-word searches work best). Freesound CC0 is the only large shippable source; xeno-canto bird recordings are mostly CC BY-NC-SA (not shippable); the BBC sound-effects archive is non-commercial; Wikimedia Commons carries some PD/CC0 field recordings (guess, to check per file).

| Gap | Needed for | Found (CC0, Freesound id) | Not found yet |
|---|---|---|---|
| Corncrake | May–July nights in the basins | MieliTietty 819258 "corncrakes" (two corncrakes and other night birds, 35 s, 44.1 kHz/16-bit WAV, August 2025); Walking.With.Microphones 556225 "Corn crake (Crex crex) very far" (a ready-made distant layer); himura.kazuto 435787 (a village sunrise with one in it, to audition) | a close, clean single bird for the 15–60 m band |
| Snipe drumming | April–June dusk over the basins | none under "snipe", "bekassine", "watersnip" | try "drumming", "wader", "Gallinago" |
| Frogs (Dutch) | May–June ditches and pools | remmert 513978 "Pool Frogs with birds (Pelophylax lessonae)", Beetsterzwaag, Friesland, 17 April 2020, 6 min 39 s, 48 kHz/16-bit WAV: a real Dutch chorus; musicbrain "frogs_01" (a Swiss marsh pond, id to fetch) | a marsh-frog (ridibundus) laughing chorus; try "meerkikker", "ridibundus", "Seefrosch" |
| White stork bill clattering | April–August on the roofs | Breviceps 705861 "Storks bill-clapping" (several white storks, 8 s, 44.1 kHz WAV); be_a_hero_not_a_patriot 476464 (a Czech village meadow with storks clapping, wind in trees and grass, 5 min, 96 kHz/24-bit: also a summer vegetation bed) | — |
| Beaver tail slap | night on the river | none under "beaver" (eight unrelated results) | try "splash" cuts, "Biber", "castor"; else cut a flat slap from the stone-anchor clips (a cut, not a synthesis) |
| Domestic geese | the farmyards | fthgurdy 528945 "Geese and some pigeons" (honking round a farmyard, pigeons cooing) | a hissing goose |
| Breeding crane duet | April–July dawn in the marsh | the passage clips below (duets are in them at the roost) | a clean dawn duet |
| Rain on thatch | RAIN at the houses | forestfjord 865820, 865923 ("water dripping from thatched roof", Dalstorp, Sweden; a thundershower in woods: cut the drips) | a clean thatch loop |
| Hail | hail on deck, water, thatch | heklei 842650, JonnyThePonny 393864, felix.blume 779923 (rain and light hail, countryside) | hail on wood, on water |
| Cuckoo | May carr | Elanor1995 688844, SamuelGremaud 517781, se2001 478670 | — |
| Cranes on passage | migration passes | farandjoun 826886 ("Cranes 1"), 826888 (with tawny owl), emmanuel.faivre 611617 (grues cendrées, Mont-de-Marsan) | — |
| Bittern | April–May reed beds | only little bittern (Sacha.Julien 725451), wrong species | the great bittern's boom |
| Grasshoppers | July–Aug fields | bruno.auzet 529670 (Brittany, July), 538794, 524593, Kamix0 397679, miklovan 198277 | — |
| Bees | summer yards, lime trees | simongray 372595, kiwimana 341639, RaphSandler 717237/717238 | — |
| Wolf | rare night event | betchkal 500646 (real, Cooper Creek), ondrosik 143568, Paresh 158780 | — |
| Cockerel | first light | Garuda1982 627335, JoMungus 719773, nomadas 623796 | — |
| Hand bell (riveted iron) | the churches | RP1312 350713 (single cowbell strike), fmiramar_ 397349, skoul 546844 (goat bells) | — |
| Footsteps in snow | winter town | Nox_Sound 613849, Nimlos 432285, cabled_mess 384423 | — |
| Lapwing | spring fields | solarpsychedelic 871003 (species to check) | a sure northern lapwing |
| Red deer roar | Sept–Oct dusk | none under "red deer rut" | try "deer", "hirsch", "cerf" |
| Bailing | storm stage | none | try "bucket splash", "scoop water" |
| Swans (whooper call, mute swan wingbeats) | winter water, flights | none | try "swan" alone |
| Mast and rigging creak, halyard, sail hoist | ship | none under those phrases (ship_creaks_sloshing_450255 covers the creak) | try "halyard", "sailing" |
| Tern colony | summer flats | none | try "tern" |
| Cattle herd lowing, winter goose skeins, ice on a hull | pasture, winter | none under those phrases | single-word searches; "geese" alone gave the two in the library and 528945 |
| Cargo shifting, oar thump at the tholepin, drips off the oars, the sail's thump filling, a stone anchor's rope paying out, water sloshing in the bilge | ship | partly in oars_234361 (creaks), ship_creaks_sloshing_450255 (slosh) | cut from existing clips; search "thud wood", "rope creak" |
| Crowd and voices in Frisian, Old English, Old Norse, Latin | town, hall, church | market_crowd_medieval_424790 (9.6 s, re-enactment, language indistinct) | none; see open question 1 |
| Ice booming on a river | hard winters | ice_lake_fractures_737896 (lake; fine) | — |

---

## 2. The model

### 2.1 Listener context (computed every frame, cheap)

The listener is the camera (OrbitCamera in the voyage, the chart's eye in 2D, the deck's eye in the isometric mode). Each frame the soundscape builds one `SoundContext`:

| Field | Source | Notes |
|---|---|---|
| wind U (m/s), gust g | LevelWeather.Wind (gust: storms S1, later) | Beaufort B from the table in 1.1 |
| wave height Hs (m) | storms S1 real sea; until then Hs = f(B) from the table, ×0 in a river place | |
| rain R, snowfall S, snowCover, frost, ice, temperature T, storm, fogDensity, mistDensity, cloudCover, wetness | LevelWeather (its public state: CloudCover, Overcast, Rain, Snowfall, SnowCover, Frost, Wetness, Wind, WindVector, FogDensity, MistDensity, Storm, Ice, Temperature, Leaf, Autumn) | all 0–1 but T (°C) |
| humidity RH (0–1) | derived from LevelWeather.kind, since the weather has no humidity: Fog and Mist 1.0; Rain, Snow, Storm 0.95; Overcast 0.85; Fair and Haze 0.7 by day, 0.9 at night and in the two hours after dawn (guess: the river valley's dew). The first slice uses a constant 0.8 and the rule comes with night two | feeds the air-absorption cutoff only |
| hour h, dayOfYear d, year y, night n, sunAltitude, weekday | LevelClock; weekday = `Calendar.Weekday(LevelClock.JulianDay)` (new, 4.3) | n = LevelClock.Night |
| feast, sunday | Feasts.On, weekday == 0 | church days: the bell, no smithy |
| height above water a (m) | camera.y − LevelInfo.WaterLevel | |
| surroundings: water w, reed r, wood f, field p, town t, dam k, mud m (fractions 0–1) | 16 taps in a disc of radius 60 m round the listener, weighted 1/d, from the level's baked **acoustic cover map**: a 512² byte map at 10 m cells, made by the builder from the bake's `cover.u8` (2.5 m, n = 2049) plus a town mask from the builder's own house list; a scene asset, not a G:\ file (4.2) | one small texture read on the CPU |
| shore distance s (m), signed | the level's **acoustic distance map**: a 512² map at 10 m made by the builder from the bake's `water_sd.f32` (signed distance to the water's edge in metres, + in the water, − on land; it exists already, `Bake/levels/make_dorestad.py`); 4 taps, bilinear; shifted by the flood (2.9) | drives surf, lapping and the flood's edge from night one |
| inside i (0–1) | listener within a house's hull volume (the builder knows every house's box) | reverb and occlusion |
| ship: v (m/s), heading, pitch rate, roll rate, heel, relative wind angle, sail set, rowing, water aboard, bow wave-relative speed | ShipMotion (Speed, Pitch, Roll, Velocity), the wind vector; sail/rowing/water aboard from the storms step S6 when it lands (until then: rowing in a river place, sail on the sea) | |
| ship distance D (m), ship near | camera to ShipMotion.Player | |
| level kind | LevelInfo.kind (RiverPlace, Ocean), realMetresPerUnit | an ocean level is 1:150 across but audio works in game metres |

### 2.2 The gain law for a point or bed, in dB

Every layer's loudness is built the same way:

L = L_base + L_cond + L_dist + L_wind + L_occ + L_duck

- **L_base**, derived, not guessed: every point source and voice has a **source level at 1 m, L_src in dB SPL** (tables 2.4, 2.7 and the wildlife rows), from the real figures and estimates in 1.2. One global offset turns SPL into the game's full-scale: **K = −106 dB**, so that 100 dB SPL at the listener plays at −6 dBFS, and L_base = L_src + K. Beds (which have no "1 m") keep a tuned L_base (table 2.3), set once against a reference emitter (a cockerel at 20 m should sit where it does in a real yard). K is the single mix knob: moving it moves the whole world together, and it is what holds the −23 LUFS target; the per-source trims that remain after that are the by-ear part, in a `trim_db` field, expected within ±4 dB.
- **L_cond:** the condition curve: a product of 0–1 responses to the context fields, written in the data file as piecewise-linear "smoothstep" ranges (`"wind": [1, 6]` means 0 at 1 m/s rising to 1 at 6 m/s; `"hour": [[4, 6], [20, 22]]` a window with soft edges). Converted to dB as 20·log10.
- **L_dist:** for point emitters, −20·log10(d/d0) with d0 = 1 m (inverse distance, the real law in open air), clamped at the layer's min distance; plus air absorption: a low-pass whose cutoff f_c(d) is the frequency where the ISO 9613-1 absorption at the current T and RH reaches 15 dB over d (so at 10 °C, RH 0.8: roughly 20 kHz at 50 m, ~6 kHz at 500 m, ~2.5 kHz at 2 km, ~1.2 kHz at 6 km; guess from the orders of magnitude in 1.2; the code computes it from the standard's formula and the table is checked by a test).
- **L_wind:** the masking and the refraction. Masking: beds other than WIND and SEA are not reduced in level (the ear does its own masking) except that wildlife voices above B7 are dropped (−∞): birds do not sing in a gale. Refraction: for a source farther than 100 m, +3 dB and f_c × 1.5 downwind, −6 dB and f_c × 0.5 upwind (angle between the wind vector and the source bearing, cosine-weighted), and on a calm clear cold spell (U < 2, clear sky, T below the day's mean, so an inversion is likely: night and the first two hours after dawn) +4 dB and f_c × 1.5 for everything beyond 100 m (all guesses, to tune; the direction of each effect is the physics of 1.2).
- **L_occ:** occlusion by raycasts from listener to emitter (section 2.6).
- **L_duck:** ducking from the events (2.5): a close thunder ducks every other bus −8 dB for 2.5 s, released over 4 s; a bell ducks the town bus −3 dB.

Levels are smoothed in dB at a rate per layer (beds 1.5 dB/s, point sources 6 dB/s) so nothing clicks when the weather flips with G.

### 2.3 Layers: beds and voices, with conditions

A **bed** is a loop (or a crossfade set of loops chosen by one driver) that is always playing when its condition is above zero. A **voice** is a pool of one-shots fired at random intervals from a position chosen by rule. A **point emitter** is a placed source (2.4). The data file holds all three.

Rule for wind (from 1.1): the pure WIND bed is capped low, −14 dB at B9, because an ear outdoors does not hear wind as a bed; the gain growth with force goes to VEGETATION on land (reeds, leaves, grass up to −8 dB at B9) and, at sea, to the rigging and stays (2.7) and the sea bed. An "ear buffet" factor (the WIND bed up to −8 dB) exists only in the deck view, where the listener's head is in the wind.

| Layer | Clips (crossfade driver) | Condition (0–1 factors) | L_base (dB) | Position |
|---|---|---|---|---|
| WIND | light_breeze (B1–3) → strong_wind (B4–6) → gale (B7+), equal-power crossfade on U; night_breeze_village at B0–1 by night in the town; winter_wind_bare_trees replaces light_breeze when leaf < 0.3 | wind [0.5, 1.5] | −34 at B1 rising 2.5 dB per force to −14 at B9, flat beyond; deck view: cap −8 | 2D (non-spatial), stereo width full |
| VEGETATION | reeds_wind (r), wind_leaves_spring (f × leaf), wind_dry_grass (p × (1−leaf) in late summer, p in winter), reeds_wind_shore (r × |s| < 40 m) | wind [1.5, 8] × surroundings fraction; level rises 2 dB per force from B3 to a cap of −8 at B9 | −24 at B3 | 2D, panned toward the densest sector of that cover |
| WATER: lapping | river_lapping_bank × (w, |s| < 30 m), ripples_lake_shore (w, B0–2), wake_under_boardwalk (k: on a dam) | above water a [60, 8] (louder the lower) × shore distance |s| [60, 5] | −26 | 3D at the nearest water edge point (from the distance map's gradient), spread 90° |
| WATER: surf | surf_on_a_beach_426076 (Hs > 1) crossfaded with surf_339517 (Hs 0.3–1) | ocean level only; surf = shore distance |s| [600, 40] × Hs [0.2, 2] | −20 | 3D along the shoreline (nearest point), spread 120° |
| WATER: open sea | background_waves_412308 crossfaded with the lower (trough) passages of surf_426076 by Hs; hull recordings never leave the ship bus | ocean, a < 40, Hs [0.5, 4]; level +2 dB per force from B4 | −24 | 2D |
| RAIN | rain_soft_loop (R 0–0.4) → light_rain (0.3–0.7) → heavy_rain (0.6–1); plus rain_on_pond × w, rain_on_tent (thatch stand-in) × t, rain_on_the_sail × ship near × sail set; hailstorm when hail (a storm with T 2–8 °C, 10 % of storm hours) | rain [0.05, 0.8] | −18 | 2D; the surface layers 3D at their surroundings |
| SNOW SILENCE | winter_wind_bare_trees at a low level; everything else gets L_occ −3 dB (fresh snow) rising to 0 when snowCover is old (ice > 0.5 = refrozen) | snowfall or snowCover > 0.3 | −34 | 2D |
| GRASSHOPPERS | grasshopper meadow clips (gap list) | day d [185, 245], hour [11, 18], T > 20, p × (1−R) | −30 | 3D in the field sector |
| WET-MEADOW NIGHT (a voice group) | corncrake (819258 cuts, 556225 far), pool/marsh frogs (513978 cuts), snipe drumming (gap), bittern (gap) | day d [125, 200], night [0.3, 1], T [8, 14], (1−R)², B < 5, surroundings (reed + mud + field-by-water) [0.1, 0.5]; the group guarantees at least one member every 20 s on a mild May night | per species from L_src: corncrake 85 at 1 m, frog chorus 80 at 10 m, bittern 95 at 1 m (estimates) | 3D in the basin sector, 60–600 m, the far ones low-passed |
| WILDLIFE voices | per species, table 1.4: a clip pool, a season window, an hour window, a surroundings weight, a mean interval, a distance band (e.g. skylark 30–120 m up and over fields; nightingale 15–60 m in hedges; stork on the nearest roof; cranes 300–1200 m overhead moving with the wind, and a breeding pair's duet at dawn from the marsh sector; goose skeins moving 15 m/s; beaver slap at the water's edge, 40–200 m, night) | the product of the windows × (1−R)² × (B < 7) | from L_src: nightingale 88, blackbird 80, cuckoo 85, stork clatter 85, crane 100, goose 90, cockerel 95, beaver slap 90, gull 85, owl 80, skylark 75 (estimates, dB SPL at 1 m) | 3D, a position from the rule, the far ones low-passed by distance |
| TOWN voices | cockerel (first light), dogs (night, startled by the ship's arrival), domestic geese, chickens, pigs, cattle, sheep, horses (yards and pasture), market murmur (market days, 8–16 h), well bucket, gate, loom, saw, axe | per emitter, 2.4 | — | point emitters |
| SHIP | section 2.7 | — | — | on the ship |

### 2.4 Point emitters: the town's day

The builder knows where every smithy, well, house, farm kraal, dam head, fire basket and church is. It serialises one row per emitter (kind, position, radius, orientation, start phase) onto a `SoundEmitterSet` component in the scene (the editor may write `emitters.json` as its intermediate, but the game never reads a file for this), rather than dropping 700 AudioSources. At run time only the N nearest emitters of each kind within hearing (N = 4, hearing = 120 m for a well, 400 m for a smithy, 2500 m for a bell) get a real AudioSource; the rest are virtual (not created). The day's rhythm comes from a schedule table (`schedule.json`):

| Emitter | Clip(s) | L_src at 1 m (dB SPL) | Hours (local mean time) | Days | Weather | Radius (min / max, m) | Notes |
|---|---|---|---|---|---|---|---|
| Smithy | smithy_bellows_anvil (blows), smithy_254370 (bed) | 108 (estimate: a riveting machine is 110) | 7–12, 13–18 | not Sundays, not feasts | not storm | 3 / 400 | 18 of them; each has its own random start so the hammers never line up; a blow pool fired 20–60 per minute |
| Hearth | hearth_fire_crackling | 60 (estimate: conversation level) | always | — | — | 1 / 8 | inside; audible at the door, occluded by the walls (2.6); TownLife's cooking peaks (7 h, 18.5 h) raise it 3 dB |
| Fire basket | fire_454745 | 65 (estimate) | dusk–dawn | — | not rain (hiss then) | 2 / 30 | 37 at the dam heads; flames shown by TownLife at dusk > 0.12 |
| Bonfire | bonfire_night | 75 (estimate) | dusk | feast days with fires (Feasts) | — | 3 / 120 | with the market murmur |
| Well | well_bucket_rope | 70 (estimate) | 6–9, 17–20 | — | not storm | 1 / 60 | 651 wells: the 4 nearest, each fires once per 2–5 min at the water-fetching hours |
| Loom | hand_loom | 75 (estimate) | 8–17 | not Sundays | — | 1 / 25 | inside one house in ten, winter-weighted (indoor work) |
| Saw, axe | hand_saw_log, axe_chopping_tree | 90 / 95 (estimate) | 8–16 | Nov–Feb mostly (coppicing, firewood), any day for the shipwright at the dismantled hull | not rain | 2 / 250 | at the woodpiles and the landing beach |
| Gate | wooden_gate_bolts | 75 (estimate) | 6–20 | — | — | 1 / 30 | at kraals, when stock go out (dawn) and come in (dusk) |
| Domestic geese | 528945 (farmyard geese) | 90 (estimate) | 6–20; alarm at night when the ship moors within 60 m | — | — | 2 / 200 | one flock per farm, the loudest yard animal |
| Chickens, pigs, cattle, sheep, horses | the library clips | chickens 70, pigs 90 (a squeal 100), cattle 90, sheep 80, horse and cart 80 (estimates) | chickens 5–19, pigs 7–18, cattle dawn and dusk (milking, moving), sheep by day, horse and cart 8–17 on the levee road | cattle out on the basin margins May–Oct, in the yards Nov–Apr (guess: the historic pattern of marsh grazing) | — | 2 / 120–300 | the horse and cart is a moving emitter along the road |
| Dogs | dogs_440865 (far), 832435 | 95 (estimate) | all day, more at night; always when the ship arrives at a dam | — | — | 2 / 300 | a "bark chain": one bark triggers neighbours within 150 m with 0.5–2 s delays |
| Cockerel | gap (Garuda1982 627335) | 95 (estimate) | first light: sunAltitude −8° to +5°, then 1 per 10 min to noon | — | — | 2 / 250 | 2–3 per hundred houses |
| Market murmur | market_crowd_medieval | 70 at 10 m (estimate: many conversations) | 8–16 | the port's `market_day` (a weekday per port in ports data, Dorestad's a guess), fair season (fairs_793.json, which lists fairs by port, not market days), feast days | not storm | 10 / 150 | at the landing beach / the dam heads; voices indistinct, by design |
| Bell, Upkirika (across the Lek) | a sheet-iron hand bell: 350713 or a goat bell (1.6), unpitched | 100 (estimate: a small hand bell) | from **793**: before mass on Sundays and feasts (dawn, and about 9 h), and for a death | Sundays, feasts, chronicle deaths | heard only on calm mornings (B ≤ 2) and downwind, by the refraction law | 10 / 2500 | position: the bake's RIJSWIJK zone, (3600, 800) level metres, ~1.25 km SE of the port point (guess: the castellum is lost); a few strokes, hand-rung: uneven timing (0.9–1.4 s) |
| Bell, Frankenhof | the same bell clip, its own strike pool | 100 (estimate) | from **c. 800**: the same offices | Sundays, feasts, chronicle deaths | — | 10 / 1500 | the builder's existing bell source at the Frankenhof (DorestadTown.cs line 925) becomes this emitter |
| Footsteps on planks | footsteps_on_planks_521589 (pool) | 65 (estimate) | 6–20 | — | — | 1 / 40 | from invisible "walkers" that move along the dams by day (a dozen agents on the dam centrelines; they are the people and props to come) |
| Footsteps in mud | footsteps_mud_path | 60 (estimate) | 6–20 | wet > 0.3 | — | 1 / 25 | on the lanes |
| Fish weir / weir gurgle | weir_small | 70 (estimate) | always | — | high water louder | 2 / 60 | at the pools' weirs and the dam ends where the current gulps |
| Ice | ice_lake_fractures (pool) | 100 at 10 m (estimate; a boom) | night and the first hours of sun | ice > 0.3 | T < −2 by night, or warming fast after a frost (the booming comes with the temperature swing) | 20 / 2000 | on the river's ice, random positions |

All emitters are silenced by the Beaufort masking rule above B8 except the smithy (which is indoors) and the bells.

### 2.5 Events

Events are one-shots fired by the simulation, with a delay rule and a ducking rule:

| Event | Trigger | Delay | Clip choice | Ducks |
|---|---|---|---|---|
| Thunder | LevelWeather.Lightning(distance) | distance / c(T), c = 331.3 + 0.606·T | d < 1.5 km: close_thunder (crack then rumble); 1.5–16 km: distant_thunder, low-passed by f_c(d); > 16 km: nothing (NWS). The generator today fires 300–6,300 m only, so the 16 km rule waits on the storms plan's real strike distances | −8 dB everything else when d < 1 km |
| Anchor | `ShipMotion.Arrived` (new one-line event where Goal is cleared with "There.") and no dam head within 12 m | 0 | stone_anchor_splash (pool) | — |
| Mooring | `ShipMotion.Arrived` within 12 m of a dam head (the dam-head positions are on `SoundEmitterSet`; the builder knows them) | 0.5 s | ropes_straining (3 s), footsteps_on_planks | dogs bark chain, the farm geese |
| Sail set / struck | storms S6 sail state change | 0 | sail_flapping (2 s cut), then the fill thump (gap) | — |
| Wave slap | bow wave-relative vertical speed > 1.2 m/s (sampled from WaterSampler at the bow) | 0 | a pool cut from water_past_the_hull_360631 (its slaps), louder with the impact speed | — |
| Water aboard | storms S6: green water over the gunwale | 0 | a heavy slap + ship_creaks_sloshing raised for 6 s | −4 dB wind |
| Cargo shift | roll > 18° or a slam | 0 | gap (a thud pool cut from axe/planks clips) | — |
| Bailing | water aboard > 0 and the crew bailing (S6) | rhythmic, 1 scoop / 2.5 s | gap | — |
| Ice crack | 2.4 | 0 | ice_lake_fractures pool | — |
| Bell | 2.4 | 0 | bell pool | −3 dB town |
| Goose skein | Flock (Geese) takes flight | 0 | geese_407490 / geese_flock_landing (greylag cuts), moving source on the flock's centre | — |
| Heron flush | Flock (Herons) startled (startle 45 m) | 0.3 s | heron_call_nest | — |
| Gulls on the ship | Flock (Gulls) following, D < 60 m | periodic 6–20 s | herring_gull_538016, black_headed_gull | — |
| Raid / fire (chronicle) | chronicle event at this place | — | bonfire bed raised, dogs and geese, the bells at any hour, the market murmur replaced by nothing; horns are a gap | — |
| **SoundHeard** (to the game) | any voice, emitter or event above −40 dB at the listener for the first time this year, or a rare one | — | no clip: a message `{sound, species, position, year}` the crew remarks and the first-heard log consume (3.2) | — |

### 2.6 Occlusion and reverb

- **Occlusion:** for each real point emitter, one raycast listener→emitter every 4th frame (so 32 emitters cost 8 rays a frame), on a physics layer that holds the house hulls, the earth dams' cores and the terrain. Per hit material: thatch/wattle wall −10 dB and f_c = 1.5 kHz; earth bank or terrain −18 dB and f_c = 600 Hz; a dense tree belt (the carr, from the wood fraction along the ray, sampled from the acoustic cover map every 20 m) −1.5 dB per 20 m and f_c × 0.9 per 20 m (guesses, to tune by ear against the look). An emitter inside a house reaches the outside through its door: the emitter's reported position becomes the door when the listener is outside, with the wall's loss applied to the fraction of the ray not through the doorway (cheap: two rays, door and centre, take the louder).
- **Reverb zones:** Unity AudioReverbZone presets, driven by the listener's position: inside a house (preset Room, decay 0.4 s, dry; a thatched hall is soft), under the storehouses' chained fronts on a dam (preset Alley, short, for the slap off the boards), in the alder carr (Forest), on the ice (Plain, longer: hard snow and ice reflect, 1.2). Open water and open meadow: none. At most two zones mix at once (nearest).
- **Spatial blend:** beds 0 (2D); point emitters 1.0 (3D) with Unity's custom rolloff curve set to the inverse-distance law and spread 0–30°; the wildlife voices 0.8 (a little in the head, so they do not pinpoint like a point source); the ship's own loops in the voyage view 0.7 at the ship's position, 0 in the deck view (you are on her).
- **Doppler:** off on everything (a 1.6 m/s boat and 15 m/s geese; the pitch wobble reads as a bug).
- **Low-pass:** one AudioLowPassFilter per real point emitter and per far wildlife voice, cutoff = f_c(d) × occlusion factor × refraction factor, slewed at 2 octaves/s. One on the listener for underwater (the camera below the water: cutoff 400 Hz, −12 dB) and for the inside-a-house state.

### 2.7 The ship's own sounds

The driver fields come from ShipMotion (Speed, Pitch, Roll, Velocity; pitch and roll rates by differencing) and, when the storms plan's S6 lands, from her state (sail set/reefed, rowing, hull soundness, water aboard, cargo weight). Until S6, rowing = (river place and v > 0), sail = (ocean and v > 0). Source levels at 1 m (dB SPL, all estimates): bow wave 60 at 0.6 m/s to 80 at 4 m/s; hull creak 70; rigging strain 75, stays singing 85 at B8; sail slat 95; oar at the thole 75; a wave slap 90; green water 100; an anchor splash 90.

| Sound | Clip | Driver and law |
|---|---|---|
| Bow wave | calm_water_lapping (v 0–0.6 m/s) → water_past_the_hull_371169 (0.6–2.5) → 360631 (2.5–6, with its slaps) | level L_src(v) = 60 + 20·log10(1 + v²/0.8), capped at 80; pitch 1.0 (never pitch-shift water: it reads as a filter) |
| Gurgle under the hull / wake | wake_rowing_boat (rowing), wake_under_boardwalk at a dam when the chop passes under the gangway | v [0.3, 1.5] × near |
| Hull working | hull_creaking_265553 (moored, strong breeze), ship_creaks_sloshing (under way), hull_creaking_195193 (big ship, far: for traders passing) | level ∝ roll rate and pitch rate: L_src = 55 + 12·log2(1 + (|dRoll/dt| + |dPitch/dt|)/4°/s); + 6 dB when Hs > 2; the slosh part ∝ water aboard |
| Rigging | ropes_straining | sail set × wind pressure (U² · cos(relative wind angle)) normalised to B6 = 0 dB; at B6+ the stays "sing": the gale clip 463373 (masts whistling in a harbour) blended in at the mast, 3D; this layer, not the WIND bed, carries the gale at sea (rule in 2.3) |
| Sail | sail_flapping_554439 (slow luff), 448975 (hard slat) | luffing: relative wind angle < 40° or sail just set/struck, or at anchor with the sail up; a slat pool at B5+ when she comes about |
| Oars | oars_234361 (creak of the thole), oars_588307 (from inside), oars_paddles_calm (calm stream) | rowing × near; stroke period = 60 / (22 + 8·v/1.6) s, a pool of single-stroke cuts fired on the period with ±8 % jitter and a 0.2–0.4 s catch-to-finish; drips (gap) 0.5 s after each stroke when B < 3 |
| Crew | footsteps_on_planks pool (men moving on the thwarts), bailing (gap), effort breaths on the oars (gap; open question 1) | moving about when she stops, moors, sets sail; bailing from water aboard |
| Cargo | thud pool (gap) | roll > 18°, slam events; also a low hollow creak (from hull_creaking_195193's low notes) ∝ cargo weight × roll rate |
| Anchor | stone_anchor_splash | event 2.5 |
| Rain on the sail / deck | rain_on_the_sail × sail set; a planks pool (gap) | R × near |
| Spray | cut from surf_426076's crest hits | Hs > 2.5 and bow wave-relative speed > 2 m/s: a hiss over the bow, downwind side |

Deck mode (isometric): the listener is on the ship; the ship bus goes 2D, +6 dB, with the sloshing and the creaks the loudest things in the world, the wind in the stays overhead, the ear-buffet WIND cap raised to −8 dB, and the beds of the level still around (the harbour's town at a distance, or the open sea).

### 2.8 Phases (gateways)

The soundscape lives on a root object that survives the phase (DontDestroyOnLoad; the levels' prompts "Sail down the Rhine to the sea" load the next scene for now, and `LevelExit.Go` is a single-mode `SceneManager.LoadScene`, so the camera and its AudioListener are destroyed and remade). Two rules make that work: the component is a singleton (a second instance, the one in the newly loaded scene, hands its level data, emitter set and acoustic maps to the survivor and destroys itself), and the survivor re-finds `Camera.main`'s AudioListener after each load. The beds never stop: the new level's `SoundContext` is blended in over 20 real seconds (the river's lapping fades, the surf and the open-sea bed rise), and the point emitters of the old level are released as their distance goes beyond hearing. The ship's own loops are untouched by the phase: she keeps creaking through it.

### 2.9 Season, year, weather and events in one table

| Driver | What changes in the sound |
|---|---|
| Season (dayOfYear) | the wildlife roster (1.4): the wet-meadow night group May–June, the storks April–August, the breeding cranes' dawn duets April–July, the corncrake until August, the geese from October; the vegetation bed (leaves in summer, dry grass in late summer and winter, bare trees in winter); farm work (saw and axe in winter, cattle out May–Oct); the grasshoppers; the ice |
| Hour | the beds' wildlife windows; the town's schedule (cooking peaks, water-fetching, the smithy's hours, the dusk gate); the refraction bonus at night and after dawn; the mist hours (dawn: dripping, the sound deadened: −2 dB and f_c × 0.8 when mistDensity > 0.01; fog: ships' and birds' sounds come from nowhere: spatial blend 0.5, no visual); RH by the hour |
| Weekday | Sunday: the bells, no hammers, no loom; the port's market day: the murmur |
| Weather kind | the WIND crossfade and level; the RAIN set; SNOW silence; the storm's thunder; hail; lightning ducks; RH for the absorption cutoff |
| Wind force and direction | WIND, VEGETATION and sea beds; the ship's rigging and sail; the Beaufort mask on wildlife and the town above B7–8; upwind/downwind gain and cutoff on far sources (the Upkirika bell is the clearest case: a calm Sunday dawn with the air from the south-east) |
| Sea state Hs | surf, open-sea bed, slaps, hull working, spray, cargo |
| Temperature | the speed of sound (thunder delay), the crunch of snow underfoot (colder = louder), ice booming, grasshoppers and crickets off below 13–20 °C, frogs on mild nights (above ~8 °C air, chorus from 18 °C water) |
| Floods (flood 0–1) | the lapping bed moves inland with the water's edge (the signed distance map is offset by the flood's rise over the local slope; the builder bakes a second map at the flood level and the model lerps); the weirs roar; the dam-gurgle rises; the town's lanes go to mud |
| Feasts and Sundays | bells (Upkirika from 793, Frankenhof from c. 800), no smithy, bonfires, market murmur, (music: held) |
| Chronicle events | raids (bells at any hour, dogs and geese, fire beds, no market), famine years (fewer animals: the stock emitters × 0.4), hard winters (ice, wolves at night, fewer birds; the bittern silent the spring after), floods (above) |
| Year | the Frankenhof bell appears c. 800; a bigger church bell when a stone church comes (the parish church sold in 1016: a cast bell from then, guess); the first-heard log resets each year (3.2) |

---

## 3. In the game

### 3.1 Four moments

**Dorestad's dams, a June dawn, fair, B2.** Still water: ripples_lake_shore at −30, almost nothing. The gurgle under the gangway where the dam's chop passes. From the basins behind the town the night's corncrakes are still rasping, two of them at their own rhythms, and the frog chorus is thinning as the light comes; a snipe drums once over the wet meadow. A blackbird in a hedge behind the storehouses, loud, 20 m; a cuckoo far in the carr across the Kromme Rijn, low-passed to 4 kHz at 600 m, carried on the inversion of a clear calm morning; a skylark climbing over the strip fields, 90 m up; black-headed gulls starting up at the shoal; swallows along the water from 5 h; a stork clatters on a roof ridge; a cockerel, then another beyond it, and the farm geese answering; the first well buckets at 6; a goose skein lifting at 5.20 going over the river with wingbeats and calls, moving 15 m/s. If it is a Sunday, faint and uneven from the south-east across the Lek, a hand bell at Upkirika, a kilometre and more off, and no hammers all day. Smoke from every roof at the cooking peak: hearths crackle at the doors you pass. On a weekday at 7 the first smithy, 300 m away, dull and single, and a second one with its own rhythm. Thatch drips from the dawn mist. Your own ship: her mooring ropes creak as she shifts, a man's steps on the planks.

**Dorestad, a November storm night, B9, rain.** The reeds and the bare trees roar (VEGETATION at its cap), the pure wind bed under them at −14; the harbour masts' whistling from the moored boats at the dam heads. Nothing from the wildlife bus. Rain heavy; where you stand by a house it is rain_on_tent (thatch) in front and rain_on_pond behind; at the dam, the chop slaps the caisson boards and gulps under the gangway. The moored hulls work and groan (hull_creaking_265553), the ropes grind. A dog, upwind, only when the wind drops between gusts. Thunder arrives 11 s after a flash 3.6 km off, as a rumble through 2.5 kHz; the close one, 400 m, at 1.2 s, a crack, and every other bus ducks 8 dB for 2.5 s. The fire baskets hiss in the rain. Inside a house (if the player steps into the door): the storm drops −10 dB and dulls to 1.5 kHz, the thatch takes the rain as a soft hush, the hearth is the loudest thing, the Room reverb short. (Until storms S1/S2 land the generator stops at B7, so this scene is heard through the DevHud wind override, 4.3.)

**The open sea in a gale, B8, Hs 6 m, sailing.** The stays singing at the mast (3D, above you) and the sea bed carry the gale; the pure wind bed sits under them. The sail hard and silent until she comes off the wind and it slats. The bow wave at its cap with the slaps firing at every crest; green water: a heavy slap and six seconds of slosh; the hull creaks with every roll (12 dB per doubling of roll rate). Spray hisses over the weather bow. No birds. No thunder unless the storm bed says so. The crew's steps and the bailing's scoop every 2.5 s when there is water aboard. If the user stops time for the storm stage (storms S6), the audio keeps running: the storm is still there while you decide.

**A frosty calm morning on the open sea, B0–1, −3 °C, clear.** Silence as a thing: the beds at their floors, the stereo field almost empty, the hull's ice-cold creak at every slow roll, drips from the oars half a second after each stroke, water past the hull as a thin hiss at 1.6 m/s. Across 2 km, a trader's oars at their own cadence and the men's steps on her planks, carried on the inversion (+4 dB, cutoff ×1.5): you hear her before you see her in the haze. A curlew on the flats a kilometre off. The far surf as a bass bed. Whooper swans (gap) going over. If the fog comes, the sounds lose their place: spatial blend 0.5.

### 3.2 What the player does with sound

- **Navigation by ear:** in fog, surf tells the shore side and distance (its level and the signed distance map are the truth); the bell of a port, the smithies' hammers, the geese and the dogs tell you a town before the coast resolves; a weir's gurgle marks a river mouth or a dam end. The lookout plan (ai_fleets.md) already hails by sight; by ear the oars of a longship upwind are a warning the player can learn.
- **Reading the weather:** the stays begin to sing at B6, the sail's slat means she is luffing, the thunder's delay tells the storm's distance and whether it is coming (the delay shortens), the goose skeins and the heron flush tell dusk. The first spring cuckoo, the corncrakes starting in May and the last swallow are the season's clock in sound. Snow silence tells you snow has fallen in the night before you look.
- **The town's day:** the schedule gives the market, the smithy's working hours and the church days a sound before a text panel says anything; the Sunday silence of the hammers and the bell across the Lek are a date you can hear.
- **The crew hear it too (hook 1):** the `SoundHeard` event (2.5) feeds the same text channel the game already has (`Game/UI/TextPanel`): a crewman remarks on a sound the first time it is heard that year or when it matters: "First cuckoo." "Geese going south: the year is turning." "Surf to larboard." "Oars, upwind. More than one pair." "That's a bell: there's a church past the fork." The lines are placeholder text, written by the user later; the system is one data table `remarks.json` (sound id → line, who says it by crew role, cooldown), not new code beyond the event. The remark is also how the player is told once what a sound means, so that navigation by ear is learnable.
- **The first-heard log (hook 2):** every species and named sound has a row in the collectables data (G:\kapman_unity\Kapman\Plans\collectables.md, a new "Heard" section): first heard this year, at which place, in which weather. The rare ones (wolf, bittern, crane duet, ice booming, the beaver's slap, a stork on the roof of your own house) are entries worth going out at dawn for. Data rows, not a system: the log is the collectables UI reading `SoundHeard`.
- **Replayability:** the roster is a product of season × hour × weather × weekday × year × place (the ocean levels have the flats' waders and surf, the river places the carr's birds, the basins' night group and the town), so no two arrivals sound alike, and the chronicle's years (famine, hard winter, raid) each have their own sound. The log makes the depth visible: a year in which the player never heard a bittern is a year that sounded different.
- **M mutes; H's panel** gets one line: the loudest three layers and their dB (for tuning by ear, from the physical start).

---

## 4. Build plan for Unity

### 4.1 Classes

Pure, testable model in `Kapman.Core` (no UnityEngine types beyond Vector3 and Mathf; the test assembly references Kapman.Core only):

- `Core/Audio/SoundContext.cs` — the struct of section 2.1.
- `Core/Audio/SoundLayerDef.cs` — a layer from the data: kind (Bed, Crossfade, Voice, Group, Emitter, Event), clips, driver, condition ranges, L_src or L_base, trim, smoothing rate, spatial blend, distance band, interval. `Condition.Evaluate(SoundContext) → 0–1`.
- `Core/Audio/SoundscapeModel.cs` — `Evaluate(context, defs) → LayerOutput[]` (gain dB, crossfade weights, cutoff Hz, position rule, fire-this-frame flags), the Beaufort function, `SpeedOfSound(T)`, `AirAbsorptionCutoff(d, T, RH)` from ISO 9613-1's formula, `HumidityFromWeather(kind, hour)`, `RefractionGain(bearing, wind, inversion)`, the equal-power crossfade, the SPL→dBFS offset K.
- `Core/Audio/SoundSchedule.cs` — the town's schedule (hours, weekday, feast/Sunday, market day, weather, year) → per-emitter activity 0–1.
- `Core/Audio/SoundLibrary.cs` — loads the clip manifest (id, file, licence, credit URL, loop, cuts, preview-or-original) and refuses any clip whose licence is not CC0/PD.
- `Core/World/Calendar.cs` gets `Weekday(double julianDay) = (int)(Math.Floor(julianDay + 1.5) % 7)` (0 = Sunday), the standard rule; `LevelClock.JulianDay` already exists.

Unity side in `Kapman.Runtime` (`Runtime/Audio/`):

- `LevelSoundscape2` — the MonoBehaviour that builds the context each frame, calls the model, sets volumes, filters, positions. In the first slice it drives the AudioSources `DorestadBuilder.Sounds()` and `DorestadTown.AddSound` already place (it finds them by name and assigns them to layers), and only from night three does it own a pool (beds: one source per crossfade slot; voices: a pool of 12; emitters: a pool of 32 real sources assigned by distance each half second). Keeps M. Survives phases as a singleton (2.8).
- `AcousticProbe` — the surroundings sampler (the two 512² acoustic maps, the inside-a-house test) and the occlusion rays (spread over frames).
- `SoundEmitterSet` — a component with the serialized emitter rows, the dam-head positions and the two acoustic map assets for the level.
- `SoundEvents` — subscribes to LevelWeather.Lightning, ShipMotion.Arrived, Flock startles, Feasts; fires the event table; raises `SoundHeard`.
- `ShipSounds` — the section 2.7 laws, reading ShipMotion each frame (and the storms' ship state when it exists).
- `SoundMix` — the AudioMixer asset with buses Master → Ambience (Wind, Vegetation, Water, Rain), Wildlife, Town, Ship, Events, UI; snapshots Outside, Inside, Underwater, Deck, Chart; exposed parameters for the ducks and K. The mixer's meters guard −1 dBTP; −23 LUFS is measured offline (1.5).

Editor: `Editor/Audio/SoundscapeBuilder.cs` — from the Dorestad builder's placements (smithies, wells, hearths, kraals, dam heads, fire baskets, the Frankenhof church from 800, the Upkirika zone from 793, the woodpiles) fills `SoundEmitterSet`, and bakes the two acoustic maps (512², 10 m) from `cover.u8` + the house list and from `water_sd.f32` into assets under `Assets/Kapman/Levels/<level>/`; `SoundImport.cs` — copies clips from `G:\kapman_unity\data\sounds_cc0` into `Assets/Kapman/Audio/Sounds`, preferring an original `.wav`/`.flac` beside each preview `.mp3` when present and recording in `clips.json` which are still previews, applies the cuts and loop trims listed in the manifest (no DSP beyond cut, fade and gain; no pitch shift), sets import to Vorbis quality 70 % (from an original; previews are imported as-is to avoid a third pass, until replaced), load type Streaming for loops over 30 s and Compressed in memory for the rest, and regenerates `CREDITS_sounds.md` from the manifest so every shipped clip has its line.

### 4.2 Data files

- `Assets/Kapman/Audio/soundscape.json` — layers, conditions, crossfades, voices, groups, the event table, the mix levels. Example rows:

```json
{
  "spl_to_dbfs": -106,
  "layers": [
    { "id": "wind", "kind": "crossfade", "driver": "wind",
      "clips": [ ["light_breeze_139337", 1.0], ["strong_wind_215853", 7.0], ["gale_181935", 15.0] ],
      "base_db": -34, "db_per_force": 2.5, "cap_db": -14, "deck_cap_db": -8, "smooth_db_per_s": 1.5, "spatial": 0 },
    { "id": "reeds", "kind": "bed", "clip": "reeds_wind_411847",
      "when": { "wind": [1.5, 8], "surround.reed": [0.05, 0.5] }, "base_db": -24, "db_per_force": 2, "cap_db": -8, "spatial": 0, "pan": "surround.reed" },
    { "id": "lapping", "kind": "bed", "clip": "river_lapping_bank_318064",
      "when": { "above_water": [60, 8], "shore_abs": [60, 5], "surround.water": [0.05, 0.4] }, "base_db": -26, "spatial": 1, "at": "nearest_shore" },
    { "id": "nightingale", "kind": "voice", "clips": ["nightingale_england_632259"], "spl_1m": 88,
      "when": { "day": [[100, 110], [150, 165]], "night": [0.3, 1.0], "rain": [0.5, 0.0], "wind": [12, 8], "surround.hedge": [0.02, 0.2] },
      "interval_s": [25, 70], "distance_m": [15, 60], "height_m": [1, 4], "spatial": 0.8 },
    { "id": "wet_meadow_night", "kind": "group", "members": ["corncrake", "frogs", "snipe", "bittern"], "min_interval_s": 20,
      "when": { "day": [[125, 200]], "night": [0.3, 1.0], "temp": [8, 14], "wind": [8, 3], "surround.basin": [0.1, 0.5] } },
    { "id": "thunder", "kind": "event", "on": "lightning",
      "delay": "distance / speed_of_sound(T)", "near_clips": ["close_thunder_347853", "close_thunder_574412"],
      "far_clips": ["distant_thunder_107518", "distant_thunder_361401"], "near_m": 1500, "silent_beyond_m": 16000,
      "duck": { "bus": "all_but_events", "db": -8, "hold_s": 2.5, "release_s": 4, "within_m": 1000 } }
  ]
}
```

- `Assets/Kapman/Audio/schedule.json` — the emitter kinds' hours/weekdays/weather (table 2.4), `market_day` per port (guess for Dorestad until the ports data carries it).
- `Assets/Kapman/Audio/clips.json` — the library manifest (from `manifest.json` + `CREDITS_new.csv`, with `licence`, `url`, `loop`, `cuts`, `gain_db`, `preview`), the single source of the credits file.
- `Assets/Kapman/Audio/remarks.json` — sound id → crew line (placeholder text), role, cooldown (3.2).
- `Assets/Kapman/Levels/<level>/acoustic_cover.asset`, `acoustic_sd.asset` (and `acoustic_sd_flood.asset`) — 512² byte and half-float maps at 10 m, about 0.5 MB each, baked by `SoundscapeBuilder` from the bake's `cover.u8` and `water_sd.f32` (G:\kapman_unity\data\levels\dorestad\, 2.5 m cells, n = 2049, 4 MB and 16 MB: never read at run time). The emitter list lives serialized on `SoundEmitterSet` in the scene; `emitters.json` under the level's data folder is the editor's intermediate only. The game reads nothing from G:\ (the one such read left today, `LevelWater.tidesFile`, is the first build's habit and not a pattern to add to); tests that need files use `Application.dataPath`.
- `Assets/Kapman/Audio/Kapman.mixer` — the mixer.

### 4.3 Where it hooks into the existing code

- `LevelWeather`: reads Wind, WindVector, Rain, Snowfall, SnowCover, Frost, Wetness, Ice, Temperature, Storm, FogDensity, MistDensity, CloudCover, Leaf, kind, flood; subscribes to `Lightning`. (Gust and Hs come from the storms plan's S1 when it lands; the model has the fields now and takes Beaufort-table Hs until then.) A DevHud wind override (a key pair, ±2 m/s, shown on the H panel) is added so the B8–B9 end of the crossfades can be tuned before the storms plan raises the generator's 16 m/s cap.
- `LevelClock`: Night, HourOfDay, DayOfYear, Year, SunAltitude, JulianDay (→ weekday), TimeFactor (voices' intervals scale with the time factor so 4x gives 4x the calls per real second, capped at 2x so it does not turn into a din).
- `ShipMotion`: Speed, Pitch, Roll, Velocity, Player; a new `public event Action Arrived;` invoked where `Goal = null; Status = "There.";` (ShipMotion.cs line 80) — one line — so nothing polls the Status string.
- `Flock`: a `Startled` and `TookFlight` event to add (two lines), and the flock centre for the moving goose and gull voices.
- `TownLife`: its cooking Bell(hour) for the hearths; `Feasts.On` for the bells and the fires; `LevelInfo` for the level kind, the water level, the scale.
- `Calendar.Weekday` (new); `Feasts` unchanged.
- `DorestadBuilder.Sounds()` and `DorestadTown.AddSound`: kept in the first two nights (their sources are driven by name), replaced by `SoundscapeBuilder` on night three (the old components are removed from the scene by the build).
- `TextPanel` (`Game/UI/TextPanel.cs`): receives the crew remarks; the collectables UI receives the first-heard rows.
- The phase: `LevelExit`/`LevelEntry` call `LevelSoundscape2.BeginPhase(nextLevelId)` so the beds blend across the load; the singleton rule of 2.8.

### 4.4 Tests (EditMode, headless, `Kapman.Tests.EditMode`, Kapman.Core only)

1. `SpeedOfSound`: 331.3 at 0 °C, 343.2 at 20 °C (±0.2).
2. `ThunderDelay`: 3,600 m at −3 °C gives 10.9 s; near clip under 1.5 km, far clip at 8 km; silent beyond 16 km (the model's rule; the generator does not reach it yet).
3. `Beaufort`: 0.2 → 0, 5.4 → 3, 13.9 → 7, 32.7 → 12; the Hs table is monotonic.
4. `WindGainMonotonic`: the WIND level never falls as U rises and never passes −14 dB outside the deck view; VEGETATION and the sea bed exceed WIND at B8 on land and at sea respectively; the crossfade weights are equal-power (w1² + w2² = 1 ± 0.01) at every U.
5. `JuneDawnDorestad`: context (day 172, hour 4.8, U 2, R 0, T 11, surround reed 0.3, hedge 0.1, water 0.4, basin 0.3) → nightingale, cuckoo and stork windows > 0, the wet-meadow group > 0, gale bed 0, grasshoppers 0, wildlife not masked.
6. `NovemberStormNight`: (day 320, hour 23, U 22, R 1, storm 1) → wildlife masked to 0, WIND at its cap, VEGETATION above WIND, rain at full, the thatch rain layer > 0 where surround town 0.6.
7. `FrostyCalmMorning`: (ocean, U 0.5, T −3, clear, hour 7) → inversion bonus on, far-source gain +4 dB and cutoff × 1.5; snow crunch loudness > a 0 °C one.
8. `AirAbsorptionCutoff`: falls with distance, falls with lower RH at high frequency, and is between 1 and 3 kHz at 2 km at 10 °C and RH 0.8; `HumidityFromWeather` gives 1.0 for Fog, 0.7 for Fair by day.
9. `ScheduleSunday`: a Sunday in 793 at dawn → smithy 0, Frankenhof bell 0, Upkirika bell > 0; a Sunday in 801 → both bells > 0; a Monday in 801 → both 0, smithy > 0.
10. `LibraryIsShippable`: every clip in clips.json has licence CC0 or PD and a URL; every clip referenced by soundscape.json and schedule.json exists under `Application.dataPath/Kapman/Audio/Sounds`; CREDITS lists every clip; the test reports how many are still previews.
11. `CricketsNotAtDorestad`: no layer with the crickets clip is active in a river place with wet surroundings.
12. `Occlusion`: thatch −10 dB and 1.5 kHz; two walls stack; the door path wins when it is louder.
13. `Weekday`: `Calendar.Weekday` gives Saturday (6) for JD 2451544.5 (1 January 2000, Gregorian) and Tuesday (2) for JD 2010701.5 (1 January 793, Julian, which is `LevelClock.JulianDay` at that date); checked against Zeller's rule for the Julian calendar.
14. `SourceLevelOrder`: with K applied, at 20 m the smithy is louder than the cockerel, the cockerel than the well, the well than the hearth at the door; a nightingale at 30 m and a cockerel at 100 m are within 6 dB of each other (the relative order of the estimates in 1.2 is what the mix stands on).

### 4.5 Performance budget (RTX 4070, 1440p, 60 fps; the audio is CPU)

| Item | Budget |
|---|---|
| Real voices at once | ≤ 28 of the 32 set in AudioManager (raise to 64 real / 1024 virtual when the deck view and a port's full emitter set both run) |
| AudioSources alive | beds 10, voices 12, emitters 32, ship 10, events 6: 70, pooled, never instantiated at run time |
| Low-pass filters | ≤ 40 (one per real 3D source) — Unity's are cheap; measured in the profiler's Audio module, target ≤ 0.5 ms DSP per 1024-sample buffer |
| Reverb zones | ≤ 2 active |
| Occlusion rays | 8 per frame (32 emitters over 4 frames), ≤ 0.05 ms |
| Surroundings sample | 16 cover taps + 4 distance taps per frame on two 512² maps (0.5 MB each), ≤ 0.02 ms |
| Model evaluate | ≤ 0.1 ms (a few hundred layer conditions, no allocation after start) |
| Memory | loops streamed; one-shots compressed in memory; total ≤ 120 MB after the long clips are trimmed; the acoustic maps 1.5 MB |
| DSP buffer | keep 1024 (21 ms at 48 kHz); the thunder delay and the oar cadence are scheduled with `AudioSource.PlayScheduled` on the DSP clock, not Update, so they are sample-accurate |

---

## 5. First slice (one night) and the two after it

The old slice was three nights' work. This one is one, and it leaves the builder's scene alone.

**Night one.** 
1. `Core/Audio/SoundscapeModel` with `Beaufort`, `SpeedOfSound`, `AirAbsorptionCutoff` (RH constant 0.8 for now), the equal-power crossfade, `Condition.Evaluate`, and the `soundscape.json` loader. 
2. `LevelSoundscape2` that reuses the AudioSources `DorestadBuilder.Sounds()` already makes (found by name) and replaces the six hard-coded loops with: WIND as a three-clip equal-power crossfade on Beaufort, capped at −14 dB; RAIN three-way; lapping by height above water and by the signed shore distance (sampled straight from the bake's `water_sd.f32` through a temporary editor-time copy into a 512² asset: the builder step is ten lines); thunder at c(T) with the near/far split and the far low-pass on `PlayScheduled`. 
3. The H panel line: the three loudest layers and their dB. 
4. Tests 1, 2, 3, 4 and 8.

Done when: the five tests pass headless (`-runTests -testPlatform EditMode`); in Play at Dorestad, G cycles the eight weathers and the wind bed crossfades with no click or jump (the gale end only up to B7 until the DevHud override of night two); a storm's flash is followed by its thunder at distance/c, a rumble for the far ones, a crack with a duck for the near; the lapping gets louder as the camera drops to the water and fades inland of the dams.

**Night two.** The VEGETATION beds and the baked acoustic cover map (from `cover.u8` + the house list); the wildlife roster of 1.4 for the species the library has (nightingale, blackbird, skylark, swallows, geese, gulls, curlew, oystercatcher, heron, mallard, rooks, crows, raven, tawny owl), the wet-meadow night group once the corncrake and frog clips are in (see the data job), season and hour windows and the B7 mask; the crickets retired at Dorestad; the RH rule; `Calendar.Weekday`; the DevHud wind override; the ship laws of 2.7 (bow wave by speed, oars by cadence, hull creak by roll and pitch rate; `ShipMotion.Arrived`); tests 5, 6, 7, 11, 13. Done when: T through a 21 June day gives the skylark and swallows by day, the blackbird at dusk, the nightingale, corncrakes and frogs by night, and no crickets anywhere; U to November gives geese and no nightingale; the oars and bow wave rise and fall with her speed.

**Night three.** `SoundEmitterSet` filled by `SoundscapeBuilder` (replacing the builder's `AddSound` sources), `schedule.json` with the source levels of 2.4 and K, the pool by distance, occlusion rays, reverb zones, the two bells (Upkirika from 793, Frankenhof from 800) with the hand-bell clip, the `SoundHeard` event with `remarks.json` and the first-heard rows, the phase singleton; tests 9, 10, 12, 14. Done when: a Sunday dawn in 793 with a light south-easterly carries one faint bell from across the Lek and no hammers; a weekday gives 18 smithies that never line up; the credits file lists every clip the scene loads; the crew remark once on the first cuckoo.

**Daytime data job (not a build night):** fetch the new Freesound clips (corncrake 819258 and 556225, pool frogs 513978, storks 705861 and 476464, farm geese 528945, the cowbell 350713 and the goat bells, the cuckoo, the cranes, the cockerel, the grasshoppers); download the original WAV/FLAC for the beds and the ship clips in place of their previews; trim the 26-minute surf and the 10-minute creak; cut the Canada geese out of 484825 and the thunder out of 350569; update `clips.json` and the credits.

---

## 6. Open questions for the user

1. **Human voices.** Spoken words are out (text is yours, and no CC0 Old Frisian exists). Are wordless human sounds in: a market murmur, effort breaths on the oars, a shout when the sail is set, a dog-startled "hey"? Or no human voice at all, so the only people you hear are feet on planks and hammers?
2. **The bells.** Two hand bells, Upkirika across the Lek from 793 (the 777 charter's church; its site is a guess) and the Frankenhof church from c. 800, each rung before mass on Sundays and feasts and for a death, and nothing else, is what the finds support. Good, or do you want hours rung as the monasteries did (which Dorestad did not have)? And is a cowbell strike an acceptable stand-in for a riveted iron hand bell until something closer is found?
3. **Music.** The plan says no music. The library holds a kaval (Balkan flute) clip. Keep the rule, or allow one diegetic player at a feast (a flute or a lyre in the hall, real recordings only)?
4. **Spatialiser.** Built-in panning plus our own low-pass and ray occlusion (this document), or a free HRTF/occlusion plug-in (Steam Audio is open source under Apache 2.0 as far as I know; guess) for headphones? The second gives real occlusion and head-related direction; it is one more dependency.
5. **Own recordings.** The rule is "no invented sound". Would you allow recordings you or I make with a microphone (a bucket of water, a wooden gate, a rope on a post, a flat slap for the beaver) as public-domain-by-us, to close the gaps (bailing, cargo thuds, oar drips, snipe is not recordable this way) that Freesound CC0 does not have?
