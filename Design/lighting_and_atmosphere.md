# Physically real light and atmosphere

Design document for Kāpman 2 (Unity 6000.6.3f1, HDRP 17.6), written 4 October 2026. It covers what the eye sees at each hour, weather and season at 52 N; how HDRP reproduces it; the five signature moments (the four the user named and the frosty calm at sea the brief asks for); the camera's behaviour; a realism checklist; and the HDRP problems now in the project, read against HDRP's source in `Library/PackageCache/com.unity.render-pipelines.high-definition@8aadf824e504`. Those problems are two: the night specular fireflies on the rigging (4.7) and the snow/frost ground pass, which blacked out every frame it ran on and is switched off with a dated comment (4.4). The black fog and black mist of the 3 October diagnostic sheets were that same pass, not the fog; they are a re-shoot to confirm, not a bug to hunt (4.6).

Everything here was checked against the code as it stands (`Assets/Kapman/Code`: LevelClock, SkyDome, LevelWeather, Hdrp/HdrpWeatherDraw, Hdrp/SnowFrostPass, the shader `Shaders/Hdrp/KapmanWeatherPasses.shader`, `Assets/Kapman/Editor/ProjectSetup.cs`, `Assets/Kapman/Editor/Shots.cs`, `Game/Sim/Sight.cs`, `Game/Data/chronicle_793.json`) and against the batch shots in `Captures/` (sheet_diag_fog and sheet_diag_fog2, shot 05:13 and 05:19 before commit 1cedc25 at 05:23; sheet_dorestad_hdrp7 and sheet_sea_hdrp7, shot after it; P08, P10, P12, P13, P14, N7, N8). Figures marked (guess) are estimates; figures with a URL were read from that page on 4 October 2026. Companion research: `Research/hdrp_realism_recipe.md` (property names and defaults) and `Research/look_realism.md`.

---

## 1. What is true

### 1.1 The Sun at 52 N (Dorestad 52.0 N; London 51.5 N; the whole playing field lies between 49 and 60 N)

Computed from the declination (+23.44°, 0, -23.44°) for 52 N; the same arithmetic SkyMath does exactly.

| Date | Noon altitude | Day length | Civil twilight after sunset (Sun 0 to -6°) | Nautical (to -12°) | Full night (-18°) reached |
|---|---|---|---|---|---|
| 21 June | 61.4° | 16.5 h | 56 min | a further 79 min | never: the Sun only reaches -14.6° at midnight; the sky is never fully dark from late May to late July |
| Equinoxes | 38.0° | 12.0 h | 39 min | 40 min | 2 h 1 min after sunset |
| 21 December | 14.6° | 7.5 h | 48 min | 44 min | 2 h 13 min after sunset |

Wikipedia gives Greenwich civil twilight as 33-48 min through the year (https://en.wikipedia.org/wiki/Twilight); the computed 39-56 min above includes the June case where the Sun crosses the horizon at a shallow angle. The practical point for the game: a summer voyage never gets a black sky, a winter one has 16.5 hours of darkness with a 7.5-hour day in which the Sun never climbs above 15°.

The shadow of a 1 m post: Sun at 5° -> 11.4 m; 10° -> 5.7 m; 15° -> 3.7 m; 20° -> 2.7 m; 30° -> 1.7 m. Between late November and late January the noon Sun is 15-17° high, so every shadow at Dorestad is at least about 3.3 times the height of what casts it, all day (3.8 at the solstice itself; 3.7 holds only within two weeks of it).

### 1.2 Illuminance on the ground (lux)

Anchors, from Wikipedia's Daylight and Lux pages and HDRP's own reference table (https://en.wikipedia.org/wiki/Daylight, https://en.wikipedia.org/wiki/Lux, https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@17.0/manual/Physical-Light-Units.html):

| Condition | Lux |
|---|---|
| Brightest sunlight | 120,000 |
| Bright sunlight (direct, horizontal) | 110,000 (HDRP table); 32,000-100,000 (Lux page, by altitude) |
| Full daylight, not in direct sun | 10,000-25,000 |
| Shade under a clear blue sky, midday | 20,000 |
| Overcast day, midday | 1,000-2,000 |
| Extreme thick storm clouds, midday | < 200 |
| Sunrise or sunset, clear | 400 |
| Fully overcast at sunrise/sunset | 40 |
| Storm clouds at sunrise/sunset | < 1 |
| End of civil twilight (Sun -6°) | 3.4 |
| Full Moon, clear | The two pages disagree: the Daylight page's table gives 0.25 ("full moon on a clear night"); the Moonlight page gives "about 0.05-0.1 lux" for a typical full Moon and up to 0.32 for a perigee Moon at upper culmination from the tropics (https://en.wikipedia.org/wiki/Moonlight). The game uses neither: it uses `SkyMath.MoonPhase`'s magnitude (-12.717 + 1.49·i + 0.0431·i⁴ + 5·log10(r_sun·r_moon/385,000.6), i the phase angle in radians, the JPL Horizons form) and `Lux(m) = 2.54e-6·10^(-0.4 m)`, which gives 0.31 lux at full (mag -12.7) and 0.028 at a quarter (mag -10.1), then ×3 for the camera (2.2) |
| Quarter Moon, clear | 0.01 (the Daylight page); 0.028 by SkyMath, i.e. 9% of full, not 4% |
| Starlight and airglow, clear moonless | 0.002 |
| Starlight without airglow | 0.0002 |
| Overcast moonless night | 0.0001 |

Clear-sky horizontal illuminance by Sun altitude, from a simple model (extraterrestrial 133 klux, extinction 0.25 per Kasten-Young air mass, diffuse 10% of extraterrestrial times the square root of sin(altitude)) anchored so that it returns 100 klux at the June noon and 400 lux at sunset; the shape between anchors is (guess), good to about 20%:

| Sun altitude | Air mass | Direct, normal to the Sun | Direct on flat ground | Sky (diffuse) | Total on flat ground |
|---|---|---|---|---|---|
| 0° (sunset) | 38 | ~10 | 0 | 400 | 400 |
| 2° | 19.4 | 1,000 | 40 | 2,500 | 2,500 |
| 5° | 10.3 | 10,000 | 900 | 3,900 | 4,800 |
| 10° | 5.6 | 33,000 | 5,700 | 5,500 | 11,000 |
| 15° (December noon) | 3.8 | 51,000 | 13,000 | 6,800 | 20,000 |
| 20° | 2.9 | 64,000 | 22,000 | 7,800 | 30,000 |
| 30° | 2.0 | 81,000 | 40,000 | 9,400 | 50,000 |
| 38° (equinox noon) | 1.6 | 89,000 | 55,000 | 10,400 | 65,000 |
| 50° | 1.3 | 96,000 | 74,000 | 11,600 | 85,000 |
| 61.4° (June noon) | 1.14 | 100,000 | 88,000 | 12,500 | 100,000 |

What this says: a December noon at Dorestad is a fifth of a June noon; a winter overcast noon (1,000-2,000 lux) is a hundredth; the light at 5° altitude is dominated by the sky, not the Sun, which is why low-Sun shadows are blue (see colour below). Note that the "direct on flat ground" column is what a horizontal field gets; a vertical sail or a hull side facing the low Sun gets the "direct, normal" figure, i.e. a sail at sunset catches 10-30 klux while the ground gets 400 lux: the ship glows against a dim land.

### 1.3 Colour of the light

From https://en.wikipedia.org/wiki/Color_temperature: candle flame 1,850 K; sunrise/sunset 1,850 K; horizon daylight 5,000 K; direct noon sunlight 5,500-6,000 K; overcast daylight 6,500 K; clear blue poleward sky 15,000-27,000 K. The Sun outside the air is 5,778 K (the clock sets this; HDRP's sky reddens it on the way down). Shadows under a clear sky are lit only by that 15,000-27,000 K sky, so they are blue; under an overcast everything is one 6,500 K grey; by a fire everything is 1,850 K orange. Moonlight is physically a little redder than sunlight (the Moon's rock is brownish) but looks blue-silver because of the Purkinje effect, the eye's rods taking over (https://en.wikipedia.org/wiki/Moonlight). The night sky's own light is 65% airglow, 27% zodiacal light and 7% scattered starlight, 21.9 mag/arcsec² at the zenith in V (https://en.wikipedia.org/wiki/Sky_brightness).

### 1.4 The eye

From https://en.wikipedia.org/wiki/Adaptation_(eye): full dark adaptation takes 20-30 minutes (the rods keep improving for about two hours); the cones adapt in 9-10 minutes; light adaptation takes seconds; the eye spans nine orders of magnitude overall but only about 1,000:1 at any one moment; as it adapts, colour perception shifts (Purkinje), and fully dark-adapted vision has no colour at all. One game minute is one real second, so a 20-game-minute adaptation is 20 real seconds: the clock makes the real figure usable as it stands.

### 1.5 Fires and lamps (the only artificial light in 793)

Anchor: a candle is about 1 candela, about 13 lumen (HDRP: 12.57 lm), about 80 W of heat, 0.16 lm/W, flame colour 1,850 K (https://en.wikipedia.org/wiki/Candle, HDRP's table). Tallow candles and rushlights were the poor household's light; beeswax was a luxury few burned at home (same page); a rushlight is a peeled rush pith soaked in bacon grease or mutton tallow, burning 10-15 minutes for 30 cm, "a good clear light" by one 18th-century account (https://en.wikipedia.org/wiki/Rushlight). Oil lamps of the period were open saucers burning fish, seal or animal fat (https://en.wikipedia.org/wiki/Oil_lamp). The brazier lighthouses: Dover's Roman pharos (early 2nd century, five levels, octagonal, the tallest standing Roman structure in England) and its pair at Boulogne, the Tour d'Ordre (built c. AD 39-40, twelve octagonal storeys stepping in, estimates 40-65 m high), which "was restored or rebuilt on the order of Charlemagne around 810" with a fire lit nightly to guide ships, and stood until the cliff took it in 1644 (https://en.wikipedia.org/wiki/Dover_Castle, https://fr.wikipedia.org/wiki/Tour_d%27Ordre). Dover's was reused as a belfry c. 1000; whether it still burned a fire in 793 is not known (treat as dark unless the chronicle says otherwise).

Light output scaled from the candle's 0.16 lm/W (all (guess); no measured figures for open wood fires were found):

| Source | Heat | Lumen | Candela (point) | Colour | Lights the ground to 1 lux at |
|---|---|---|---|---|---|
| Rushlight / tallow saucer lamp, one wick | 50-80 W | 8-15 | 0.7-1.2 | 1,850 K | 1 m |
| Tallow lamp, three wicks | 200 W | 30-45 | 2.5-3.5 | 1,850 K | 1.8 m |
| Open hearth, a house, 3-8 kW | 3-8 kW | 500-1,500 | 40-120 | 1,900 K | 7-11 m |
| Hall hearth, 10-20 kW | 10-20 kW | 1,500-3,000 | 120-240 | 1,900 K | 11-15 m |
| Beach fire, 50-150 kW | 50-150 kW | 8,000-25,000 | 650-2,000 | 1,800 K | 25-45 m |
| Lighthouse brazier, 100-300 kW | 100-300 kW | 15,000-50,000 | 1,200-4,000 | 1,800 K | 35-60 m; the point of light itself is visible at sea to about 13 km in hazy air (0.8/km) and 19-25 km in clear air (0.9/km), against a 10⁻⁶ lux threshold (guess) |

The lesson is in the last column: a hearth lights its own room and nothing else; a town of 400 houses at night is 400 doorways glowing and black between them; only a beach fire lights a stretch of water. Nothing in 793 lights a street.

### 1.6 Frost, rime, fog, breath

- Hoar frost grows on "cold, clear nights when heat radiates into space faster than it can be replaced", by vapour going straight to ice (no liquid); it needs calm air, humidity near saturation but short of condensing, and a surface below the frost point; it is feathery (https://en.wikipedia.org/wiki/Hoar_frost). Because it is made by radiative cooling it forms on what faces the open sky: grass tips, twig ends, rope, the top of a plank, the thatch ridge; the underside of a jetty stays bare.
- Rime is different: supercooled fog droplets freezing on contact. Soft rime in light fog and calm: "narrow white icy needles and scales", fragile; hard rime in wind at -2 to -10 °C, dense, comb-like, with streaks pointing into the wind; both grow on the windward side (https://en.wikipedia.org/wiki/Rime_ice). A foggy frosty morning at Dorestad is rime on the windward side of every post, not hoar frost.
- "Ice fog" in the strict sense is a fog of ice crystals at or below about -35 °C and does not happen in the Low Countries; what the brief means is freezing fog, a fog of supercooled droplets at or below 0 °C that rimes everything it touches (https://en.wikipedia.org/wiki/Fog). Diamond dust (glitter in low sun, halos, light pillars) is likewise a deep-cold phenomenon, rare near 0 °C (https://en.wikipedia.org/wiki/Diamond_dust).
- Fog proper is visibility under 1 km; mist is more than that. Fog forms when the air is within 2.5 °C of its dew point. Radiation fog: clear calm nights after sunset, pooling in the low ground (the basins, the river). Advection fog, the North Sea's: warm moist air carried over cold water, the spring and early-summer sea fog that lasts all day. Steam fog ("sea smoke"): cold air over warmer water, rising in turbulent wisps 20-30 m high at most, the winter-morning smoke off a river (https://en.wikipedia.org/wiki/Fog, https://en.wikipedia.org/wiki/Sea_smoke). KNMI counts 40-90 fog days a year in the Netherlands, more in 793 than now (see Research/look_realism.md 3.2).
- Breath is visible when exhaled air (37 °C, saturated) mixes with cold air and passes its dew point: reliably below about 5-10 °C in humid air, intermittently to 15 °C (guess; the page could not be fetched).
- At sea the same cold morning looks different from the land's. Sea air is at or near saturation, so what grows on the ship is rime, not hoar: supercooled droplets freezing on the windward side of every rope, shroud and spar (https://en.wikipedia.org/wiki/Rime_ice), with hoar only on the deck planks and the gunwale tops that radiate to the open sky. Sea smoke is not only a river thing: in a cold outbreak (polar air behind a winter front, air 5-10 °C colder than the water) the whole open sea steams in wisps 20-30 m high at most (https://en.wikipedia.org/wiki/Sea_smoke). In that same cold, dry air, flat hexagonal plate crystals falling horizontally mirror the low Sun into a vertical column, a sun pillar, "usually when it is near or even below the horizon" (https://en.wikipedia.org/wiki/Light_pillar); near 0 °C it is rare and short (guess). A glassy calm is the fourth sign: the cold outbreak's first morning is often windless before the north-westerly fills in (guess).

### 1.7 Storm and lightning

Beaufort rows (https://en.wikipedia.org/wiki/Beaufort_scale): force 7, 13.9-17.1 m/s, 4-5.5 m waves, "foam from breaking waves begins to be blown in streaks"; force 8, 17.2-20.7 m/s, "foam blown in well-marked streaks"; force 9, 20.8-24.4 m/s, 7-10 m, "dense streaks of foam, sea begins to roll, spray affects visibility"; force 10, 24.5-28.4 m/s, "the surface takes on a white appearance, visibility affected". The southern North Sea is shallow and its storm waves are shorter and steeper than the open-ocean heights in that table (guess). Light under a storm: under 200 lux at midday, under 1 lux at sunset (table above).

Lightning (https://en.wikipedia.org/wiki/Lightning): a flash is typically 3 or 4 strokes (up to 30), each a few tens of microseconds, 40-50 ms apart; median whole-flash duration 0.52 s; the channel is blue-white, over 27,800 °C; only about 25% of flashes reach the ground, the rest stay in the cloud (lighting the deck from inside). Thunder (https://en.wikipedia.org/wiki/Thunder): 3 s per km of distance; a sharp crack close by, a rumble that can last over 30 s from far off; 165-180 dB at the source.

---

## 2. The model

### 2.1 Light sources, in physical units, by hour, cloud and fog

The level's light is the sum of six sources, each in lux on a horizontal surface (or lumens for a flame), and each a function the clock and the weather can evaluate without rendering. This is the `LightingModel` (pure C#, Core):

```
s                      = max(0, sin h)
E_direct(h, fog)       = 133,000 · exp(-0.25 · m(h)) · s · T_fog(fog)                              the clear-sky Sun on the flat
E_sun(h, overcast, storm, fog) = E_direct · (1 - overcast)² · (1 - storm)                          0 under a full deck
E_sky_clear(h)         = 13,300 · sqrt(s) · (0.4 + 0.6·[h>0])                                       the blue sky (12,500 at June noon)
E_sky_over(h)          = 1,500 · s / sin 61.4°                                                      a full overcast: 1,500 at June noon, 430 at a December noon
E_sky(h, overcast, storm) = lerp(E_sky_clear, E_sky_over, overcast) · (1 - 0.9·storm) + E_twilight(h) · (1 - 0.9·overcast)
E_twilight(h)          = 400 · 10^(h / 2.90)          for -6° < h ≤ 0        (400 at 0, 3.4 at -6°)
                       = 3.4 · 10^((h + 6) / 2.84)    for -18° ≤ h ≤ -6°     (3.4 at -6°, 0.03 at -12°, 2e-4 at -18°)
                       = 0                            below -18°
E_moon(h_m, overcast)  = Sky.MoonLux · max(0, sin h_m) · (1 - 0.7·overcast)   Sky.MoonLux from SkyMath's magnitude (1.2): 0.31 full, 0.028 quarter
E_stars                = 0.002 clear (0.0001 overcast)
E_flame(kind)          = table 1.5, falling as 1/r², times 1,850 K
E_flash(d, t)          = 25,000 · S(t) / max(1, (d/300)²), floor 500 lux beyond 2 km   S(t) the stroke train of 2.4
```

`m(h)` is the Kasten-Young air mass the clock already uses (`LevelClock.Transmittance`); `T_fog` = exp(-h_fog / mfp) for the fog's own column (so a 200 m fog at noon still passes most of the Sun, which is right: fog is bright from inside, see 2.3). The overcast terms are the single source of truth for cloud: a full deck leaves no direct Sun at all and 1,500 lux of diffuse at a June noon (table 1.2's 1,000-2,000), a storm deck a tenth of that (150 lux; table 1.2 says < 200). This replaces the clock's `overcastLight` 0.45, which on its own would leave 39,600 lux of direct Sun under a full overcast; it only ever looked right on screen because HDRP's cloud shadows (`shadowOpacity` 0.5-0.9 in `HdrpWeatherDraw.PushClouds`) multiplied it a second time. From now on the model gives the Sun's lux and the dimming, and the clock subtracts HDRP's cloud-shadow opacity so nothing is counted twice (4.2). The twilight curve is a two-piece log fit through the three cited anchors (0°, -6°, -18°); the -12° value (0.03) falls out of the second piece and is a (guess) only in that no page gives it. The Moon is not modelled twice: `Sky.MoonLux` is the project's one Moon, and the model only applies the altitude and the cloud.

Checked values of the summed model (E_sun + E_sky), which the EV test in 4.10 is written against: June noon clear 100,000; June noon full overcast 1,500 (EV 9.4); June noon storm 150 (EV 6.1); December noon overcast 430 (EV 7.6); sunset clear 400; civil dusk 3.4.

Colour: the Sun's by `Transmittance(h)` (already in the clock, 5,778 K outside the air), the sky's from HDRP's physically based sky (which is what makes the shadows blue for free), fires at 1,850-1,900 K, lightning at ~9,000 K blue-white, the Moon perceived as (0.78, 0.85, 1.0) (the clock's `moonColour`), justified below as part of the eye model rather than the physics.

### 2.2 What the camera does: the exposure model and the eye

HDRP's exposure is a scalar multiplier applied to every scene value before tonemapping: `exposure = 1 / (1.2 · 2^EV100)`, with the automatic modes measuring the frame's luminance and `EV100 = log2(L · 100 / 12.5)` (Common.hlsl's `ConvertEV100ToExposure`; the 12.5 is the standard calibration constant). For an 18% grey ground the scenes above land on:

| Scene | Lux on the ground | Luminance (cd/m²) | EV100 |
|---|---|---|---|
| June noon, direct sun | 100,000 | 5,700 | 15.5 |
| Shade, clear sky | 20,000 | 1,150 | 13.2 |
| Overcast noon | 1,500 | 86 | 9.4 |
| Sunset, clear | 400 | 23 | 7.5 |
| End of civil twilight | 3.4 | 0.19 | 0.6 |
| Full Moon (true, SkyMath, at the zenith) | 0.31 | 0.018 | -2.8 |
| Full Moon with the clock's ×3 boost | 0.93 | 0.053 | -1.2 |
| Quarter Moon (SkyMath) | 0.028 | 0.0016 | -6.3 |
| Starlight and airglow | 0.002 | 0.0001 | -10.1 |
| The clock's `nightFill` 0.04 lux | 0.04 | 0.0023 | -5.8 |

That table is the whole exposure design. The project's limits (`ProjectSetup.ExposureMin/Max` -5/+15; `SkyDome.nightFloor` -6 overriding at full night, `dayFloor` +2, ceiling 14) cover June noon to a moonlit night. A starlit night sits at EV -10, four stops below the floor, so a camera shows it black; the eye does not. Three deliberate cheats make the camera see what a dark-adapted eye sees, and they must be documented as cheats so nobody "fixes" them:

1. `moonBoost` 3 (the full Moon at 0.93 lux instead of SkyMath's 0.31) and `nightFill` 0.04 lux from straight above on moonless nights (20× the real starlight): these stand in for scotopic sensitivity.
2. `starLight` 5 on the star cube (ProjectSetup writes 3; SkyDome overrides to 5 each frame; pick one, see 4.5).
3. The exposure floor sliding from +2 to -6 as the Sun goes from 0 to -5° (`floorDownBy`).

The eye model (`EyeModel`, Core) replaces the third cheat with the real time course and adds the Purkinje shift:

```
state: adaptation a ∈ [0,1]   (0 photopic/day, 1 fully dark-adapted)
target  a* = clamp01( (EV_scene_lower_bound - 2) / (-8 - 2) )     i.e. 0 at EV 2, 1 at EV -8
dark-adapting:   da/dt = (a* - a) / τ_dark,  τ_dark = 20 game minutes (= 20 real s at 1x)   [rods: 20-30 min]
light-adapting:  da/dt = (a* - a) / τ_light, τ_light = 10 game seconds                       [seconds]
outputs:
  exposure.limitMin = lerp(+2, -6, a)                       (what SkyDome does today, but with the eye's lag)
  fill lux          = 0.04 · a                               (the rods' floor comes in only as they adapt)
  colour drain      = lerp(0, -50, smoothstep(a, 0.6, 1))   on the shadows and midtones only (ShadowsMidtonesHighlights, or a
                                                             luminance-masked grade): rod vision is grey, but a fire or a lamp in
                                                             frame still drives the cones and stays vividly orange, so a global
                                                             saturation override would be wrong; capped at -50
  grain             = lerp(0.28, 0.45, a)                    (the noise of low light; keeps the night from banding)
  moon tint         = lerp(white·moonActual, (0.78,0.85,1), a)  (the Purkinje blue is an eye effect; by day the Moon's light is neutral)
```

HDRP's own progressive adaptation (speeds 3 dark-to-light, 1 light-to-dark in `ProjectSetup`) stays and handles the seconds-scale part: stepping out of a lit hall onto the deck at night, the frame first goes black, then over 20 real seconds the stars, the horizon and the ship's lines come up. Stepping back in, the hall dazzles for about a second. The histogram percentages (40-90) keep fires from driving the exposure: a fire in frame is allowed to clip white, which is what the eye does too.

### 2.3 Fog, haze and mist as a lit medium

Fog is a participating medium with extinction σ = 1/mfp (mean free path), albedo ~0.95, and an anisotropy g (0.4 in the weather code). The in-scattered radiance along a ray is the light reaching each point times albedo times the phase function; the fog's brightness is therefore the brightness of the light inside it, not a colour. From that follow the rules the weather code must obey:

| Kind | mfp (m) | Visibility | Lit by | What it looks like |
|---|---|---|---|---|
| Clear, dry | 15,000-50,000 | 30+ km | sky | distant trees go blue-grey; the horizon is sharp |
| Haze (summer afternoon) | 3,000-6,000 | 8-15 km | sky and Sun (forward scatter: bright toward the Sun) | a bright white glare round the Sun; the far bank pale |
| Mist | 1,000-2,000 | 1-2 km | sky | the far bank gone; near things full colour |
| Fog | 100-400 | 100-400 m | the sky overhead; the Sun as a bright patch | white-grey inside (fog is bright: a foggy noon is still 1,000-10,000 lux at the ground); dark grey only under a thick deck or at dusk |
| Freezing fog | 100-300 | | same | the same, plus rime |
| Ground mist (radiation) | 50-200, 1-5 m deep | | the sky; the Sun grazing | a white sheet over the water and basins at dawn, the trees standing out of it; it burns off from the top within an hour of sunrise in summer |
| Steam fog (river) | 20-60 in wisps | | | wisps 1-3 m high rising and drifting; winter mornings when the river is warmer than the air |
| Sea smoke (open sea) | 30-100 in wisps, 5-30 m deep | 0.5-3 km through it | the low Sun from the side; the sky | the whole sea steaming on a cold-outbreak morning, the wisps leaning with the first of the wind; the horizon lost in it while the sky above is clear; a sun pillar when the Sun is at the horizon |
| Storm | 500-1,500 plus rain | | a dark deck | grey-blue, the land drained of colour, the sea black-green |

Height profile: the project uses HDRP's exponential height fog with `baseHeight` at the water and `maximumHeight` as the e-fold height (60 m in fog, 400 m in clear air); a radiation mist on top of it is the local volume (2-5 m deep, patchy, drifting). That is the right structure, and the light inside it was never the problem: the black fog of the 3 October sheets was the ground custom pass painting over the frame (4.6). The mist's local volume is still switched off and is judged once the DiagFog re-run confirms the fog.

### 2.4 Lightning

A flash is a stroke train, not a pulse:

```
strokes n ~ 3 + geometric(0.3)   (median 3-4, tail to 30)
interval 40-50 ms (so at 60 fps: a lit frame, then two dark, then lit, ...)
each stroke: 1 frame at full L, then L·0.3 the next frame (the channel's afterglow)
continuing current (1 in 4 flashes): a dim glow (L·0.1) for 100-300 ms between strokes
L = 25,000 lux at 300 m, falling as 1/(d/300)² beyond, with a floor of 500 lux from 2 km out (a flash 6 km off still
    shows on the deck, at about a stop over a storm's 200 lux, not four stops over it like noon)
colour 9,000 K
direction: from the strike point (azimuth drawn from the storm's cell position, elevation 30-70°)
thunder: 3 s/km after the first stroke; a crack under 2 km, a rumble over 5 km (LevelSoundscape already takes the distance)
cloud-to-cloud (3 in 4): no strike point, the deck lit from inside: elevation 60-90°, L × 0.3
```

A real stroke's light lasts 60-70 microseconds (https://en.wikipedia.org/wiki/Lightning); holding it for a whole 16.7 ms frame is already a perceptual cheat that stands in for the eye's afterimage, which is why the floor must fall with distance rather than sit at thousands of lux.

The current `LevelWeather` flash (one 0.18-0.43 s pulse with 30 Hz noise) is replaced by this train. The light is the existing directional "Lightning" light the clock makes (`LevelClock`, `Make("Lightning")`, colour 0.82/0.86/1.0): it already lights the volumetric fog and the deck; it takes the stroke train, the direction and 9,000 K. "The deck lit from inside" (cloud-to-cloud) is done in the clouds themselves, not with the Sun: for the flash frames the cloud LUT's blue channel ("how much light gets in") and `VolumetricClouds.ambientLightProbeDimmer` are raised, so the deck's underside brightens from within and falls back on the next stroke. Pointing the Sun at the strike is not used: the Physically Based Sky's hash includes the Sun, so the sky cubemap and the ambient probe re-render on the swap frame; the Sun's disc is drawn on the sky at the strike (`interactsWithSky` true) and the sky reddens toward it; the Moon's `sunLightOverride = sun` shading flips; and the clouds' `temporalAccumulationFactor` (HDRP default 0.95) plus the volumetric fog's reprojection smear a one-frame swap into a glow that fades over about a second, so the 40-50 ms stroke train turns to mush. If it is ever tried, it needs `interactsWithSky` off, the clouds' `temporalAccumulationFactor` 0 and the Moon's override cleared for that frame, and the train then reads as a single blur (open question 2).

### 2.5 Frost and rime, on the ground and on the ship

Decided once per game hour from the weather's temperature, relative humidity (new; a table per kind, below), wind, cloud cover, the hours since sunset and a surface class:

```
surface class         = Ground | Ship (deck, gunwale, thwarts) | Rigging (rope, shrouds, spars, sail)
RH by kind (%)        = Fair 70, Haze 60, Mist 95, Fog 100, Overcast 80, Rain 95, Storm 90, Snow 85; at sea +5, capped 100
frost point reached   = T_surface < 0 and RH > 80%
T_surface             = T_air - radiative drop,  drop = 4 °C · (1 - cloud) · clamp01(hours_dark / 6) · clamp01(1 - wind/6)
hoar  (0..1)          = frost point reached and fog = 0 and wind < 3 m/s   -> grows 0.25/h, on sky-facing surfaces, up to 1
                        Ground and Ship classes only; Rigging gets hoar at 0.3 of that (thin rope radiates little)
rime  (0..1, side)    = frost point reached and (fog > 0 or class = Rigging and RH ≥ 95)   -> grows 0.3/h on the windward side
                        (soft under 3 m/s, hard above), up to 1; at sea the rigging rimes before the deck frosts
melt                  = T_air > 1 °C: both fall 0.4/h in Sun, 0.15/h in shade; hoar vanishes within an hour of direct Sun;
                        spray (Beaufort ≥ 5 at sea) clears hoar on the deck at once
breath                = T_air < 8 °C (always) ; 8-15 °C in fog/mist
sea smoke (0..1)      = at sea, T_water - T_air > 5 °C and wind < 6 m/s: 0..1 over 5-12 °C of difference (the water's
                        temperature from the month: 5 °C in February, 17 °C in August off Walcheren, guess)
snow lying            = as LevelWeather does now
```

The old factor `(1 - wind/6)` went negative above 6 m/s and made the surface warmer than the air; it is clamped. This replaces the single `Frost` scalar in `LevelWeather` (temperature under 1 °C and no rain) with two amounts, a side and a class, and is the input to the frost draw in 4.4, whose first surfaces are the ship's. Sea smoke enters the weather's probabilities as a cold-outbreak day (a Fair kind with a north-westerly, T_air 5-10 °C under the water: perhaps 6-12 mornings a winter, guess) and is drawn as a local volumetric fog volume over the water surface, 5-30 m deep.

### 2.6 Season and year

Nothing in the light itself changes between 793 and 1066 except the calendar (Julian) and the chronicle's events. The project's chronicle (`Assets/Kapman/Game/Data/chronicle_793.json`) already holds the sky events that matter, and the light must read them through a `SkyEvents` hook (4.2):

| Chronicle entry | Kind, certainty | What the sky does |
|---|---|---|
| `939-eldgja` (fire 939-07-01, end 940-12): the Eldgjá fissure eruption in Iceland; "a haze dims the sun in Europe"; sources Greenland ice cores (Oppenheimer et al. 2018), tree rings, AU 939 ("the sun like blood from morning to midday") | evidence, sure | a sulphate-haze state for the entry's span: the Sun's transmittance ×0.6-0.8 and redder (an extra aerosol term in `LevelClock.Transmittance`), the sky's horizon milky, sunsets long and red; a cold summer in 940 is the weather's business |
| `812-11-04-comet-812`: a comet at Constantinople "like two moons joined and then parted" (Theophanes) | portent, likely | a comet body in the sky for a few nights (guess: the entry has no span), low in the south-east before dawn |
| `837-04-halley-comet` (fire 837-04-11): Halley seen 25 days in Virgo from Easter week; closest to the Earth on 10-11 April 837 at perhaps 0.03 AU, the tail "may have stretched 60 degrees across the sky" (https://en.wikipedia.org/wiki/Halley%27s_Comet) | portent, sure | a comet body with a 40-60° tail, the brightest sky event of the whole game |
| Halley's return of 989 (September; not in the chronicle, seen by Eilmer of Malmesbury "in 989 and 1066", same page) | (guess: add to the chronicle as "likely") | a comet body, modest |
| `1066-04-24-halley`: seen for a week from 24 April (ASC, Bayeux Tapestry), closest approach 0.10 AU, "four times the size of Venus" | portent, sure | the game's last year: a comet body with a tail pointing away from the Sun, the Tapestry's star |

The comet is a body `SkyDome` draws from a table of RA/Dec by date (a short ephemeris per apparition, interpolated nightly) with a tail quad pointing away from the Sun's direction, brightness and tail length from the table; the haze is a state with a start and end date. Both are data in `data/light/sky_events.json` (beacons.json-style, read through `Json`), keyed by the chronicle entry's id so the chronicle remains the one calendar; the RA/Dec rows are (guess) until an ephemeris is fetched and cited. An aurora is already drawn; an eclipse is handled by `Sky.SunCovered`. Season enters through the Sun's altitude, the day length, the weather's probabilities (LevelWeather.Auto, now with cold-outbreak mornings) and the frost model.

---

## 3. In the game

### 3.1 The five signature moments

**A frosty calm morning (January, Dorestad, 09:30).** On 15 January at 52 N the Sun rises about 08:10 local mean time and climbs about 7.5° an hour, so at 08:30 it is barely 2° up; by 09:30 it stands at 8-10°, which is the picture the user means. The night was clear and still; the eye model has the exposure floor at -6 and colour drained; as the Sun comes up the floor rises over 10 game seconds and colour returns. The Sun at 8-10° (2,500-3,000 K, 9,000-11,000 lux on the flat, 30,000 on anything facing it): every east face of a house, every rope and the sail glow orange; every shadow is 5.7-7 m long per metre of height and blue (lit by the 20,000 K sky, through HDRP's sky ambient, which needs APV sky occlusion so the shade under the eaves is dark blue, not grey). Hoar frost on every sky-facing edge: the thatch ridges, the dam planks, the rope's upper side, the reed tips, the grass; the undersides and the lee of the posts bare. Steam fog in wisps off the river, which is warmer than the air; a thin radiation mist in the basins burning off from the top. Breath from the crew. Smoke from 400 hearths rising straight up and spreading flat under the inversion at 20-30 m, lit orange from below by the Sun. Frost sparkle: the camera close to a frosted rope sees pinpoint glints as it moves (the facets). Sound: nothing moves; a goose; ice creaking at the dam ditches (CC0 only). The frost is gone from the sunlit side by 10:00 and stays in the shade all day.

**An extremely stormy sea (October, off the Rhine mouth, 15:00).** Force 9-10: the deck under 200 lux (EV 7; the exposure opens up, the frame is grainy and grey); the sea black-green under a deck dark enough to show no Sun; dense foam streaks downwind, spray that cuts the visibility to a few hundred metres (fog mfp 500 with the rain); rain driven flat. The flashes: a stroke train from a strike point 1-6 km off: for one frame the sea, the deck and the crew are lit blue-white from that side with hard shadows, the deck lit from inside (the sky's horizon lifts), then black, then again 45 ms later; the exposure cannot follow a 25,000-lux flash and must not (it is 7 stops over the scene), so the frame blows out and the afterimage lingers in the eye model's slow light-adaptation. Thunder 3 s per km later, a crack when close. No fire is lit aboard in weather; the only steady light is the sky. The directional "Lightning" light does the strike's side-light on the sea, the deck and the fog; the cloud deck's own flash (three strikes in four never leave the cloud) is the LUT's blue channel and the ambient dimmer lifted for the flash frames (2.4), so the deck brightens from inside without the Sun being moved.

**Dusk with fires (September, the harbour, 19:30-20:30).** The Sun down; 400 -> 3.4 lux over 40 minutes (EV 7.5 -> 0.6); the sky 5,000 K at the horizon, 10,000 K above; the fire baskets at the dam heads (1,500 lm, 1,900 K) and the doorways (500-1,500 lm) take over one by one as the exposure drops past EV 4. Reflections of the fires in the water (SSR on the water) lengthen as it darkens. Smoke lit orange from below. The histogram (40-90%) lets the fires clip. By 20:30 the doors are shut against the cold and what glows is the smoke holes, the gable ends and the eaves gaps of 400 houses, the thatch itself leaking a little (the halls had no chimneys: Research/look_realism.md), black between, the sky still blue-grey in the west and the Moon's path on the river if it is up.

**A moonlit night (full Moon, the sea).** 0.31 lux true (SkyMath), 0.93 with the ×3 boost: EV -1.2; the sea's glitter path (the shot N7 already has it), the sail a grey shape, the horizon a line; colour drained from the shadows and midtones (2.2); stars at ×5 but their individual specular glints must be off the ropes (4.7); the Moon's disc dim enough not to burn (the clock's `surfaceTint` fix). Two hours of full dark adaptation make the deck readable; a fat lamp lit in its clay pot on deck resets it in a second and the stars vanish until it is covered (a real reason to keep the deck dark at sea: the lookout's night vision; this is a mechanic, see 3.2).

**A frosty calm at sea (January, off Walcheren, 09:30).** The brief's own words: "frosty mornings on a calm open ocean". A cold outbreak: the air -4 °C, the water 5 °C, the wind not yet in, the sea glassy (the HDRP water at its lowest wind, the swell long and slow). The whole sea steams: sea smoke in wisps leaning a little, 5-30 m deep, the horizon lost in it while the sky overhead is clear and deep blue; the low Sun (8-10°, 2,500 K) pushes a pillar up through the crystals in the air for the first half hour. The ship is the object 2 m from the camera and it is white: rime on the windward side of every shroud and halyard, comb-like where the wind blew in the night, soft needles where it did not; hoar on the deck planks and the gunwale tops that faced the sky, bare under the thwarts; the sail stiff and pale; the crew's breath; a thin skin of ice in the bilge. Sparkle as the camera moves. Sound: the rigging ticking as the rime drops, the water lapping, nothing else; a gull far off (CC0 only). The frost goes from the sunlit side by mid-morning and stays on the shaded side of the mast all day. On the chart this is a day to sail: no wind for an hour, then a fair north-westerly.

### 3.2 How it feeds the play

- **Night vision is a resource.** There is no evidence for lanterns on 8th-century Frisian or Scandinavian ships, and 1.5 says the period's lamps were open saucers of fat; the object is a fire-pot or a fat lamp in a shielded clay pot (guess), and an open flame on a tarred deck is itself an event. Who tends it is the crew document's; it is never lit in weather (the storms document). Its effect runs through the sight range that already exists, `Game/Sim/Sight.cs` `RangeKm` (night term 0.5 km moonless, 2.5 km under a full Moon, twilight between): the eye model's adaptation `a` scales that night term, so a lit pot drops the lookout to 0.5 km at once and the full range comes back only over 20 game minutes of dark. The player chooses dark or lit; no new lookout system is built.
- **A beacon is a real landmark.** The Tour d'Ordre's fire (from 811 by the chronicle) is a 3,000-cd point seen 13-25 km out on a clear night, less in haze; Dover's is dark unless the user decides otherwise. Hoard-fetching and night landfalls use it.
- **Twilight length is season.** Summer voyages can sail on into a sky that never goes black; winter ones have 16 hours of dark and the fog days.
- **Fog is a decision, not a filter.** Fog of mfp 200 m hides a coast at 400 m; the 2D chart and the lead line matter; the prompt at the river mouth comes earlier.
- **Replayability.** Weather, frost, moon phase and twilight are computed, not scripted, so the same voyage on another date is a different picture; the signature moments above are emergent states the player can recognise and, with the almanac in the chart mode, plan for (sail at the full Moon in June).

---

## 4. Build plan for Unity

### 4.1 New classes

| Class | Where | Does |
|---|---|---|
| `Kapman.World.LightingModel` (static, pure) | `Code/Core/Light/LightingModel.cs` | The equations of 2.1: `SunLux(alt, overcast, storm, fogMfp, fogHeight)`, `SkyLux(alt, overcast, storm)`, `TwilightLux(alt)`, `MoonLux(skyMoonLux, alt, overcast)`, `StarLux(overcast)`, `CloudDim(overcast, storm)` (the single cloud factor the clock applies), `FlameLumens(FlameKind)`, `EV100FromLux(lux, albedo)`, `ExposureFromEV100(ev)`. Pure `System.Math`, no Unity types (the test assembly references Kapman.Core only). |
| `Kapman.World.EyeModel` (plain class) | `Code/Core/Light/EyeModel.cs` | The state machine of 2.2: `Step(dtGameMinutes, sceneEvLow)`, properties `Adaptation`, `LimitMin`, `FillLux`, `ColourDrain`, `Grain`, `MoonTint`, `NightSightScale` (for `Sight.RangeKm`). |
| `Kapman.World.FlashModel` (plain class, seeded by SimRandom) | `Code/Core/Light/FlashModel.cs` | Stroke trains of 2.4: `Start(distance, cloudToCloud)`, `Sample(timeSeconds)` -> lux, direction, colour, cloud-glow; thunder delay. |
| `Kapman.World.FrostModel` (plain class) | `Code/Core/Weather/FrostModel.cs` | 2.5: `Step(hours, tAir, tWater, rh, wind, cloud, fog, sunUp, SurfaceClass)` -> `Hoar`, `Rime`, `RimeSide`, `Breath`, `SeaSmoke`; the RH-per-kind table from `data/light/weather_kinds.json`. |
| `Kapman.World.SkyEvents` (plain class) | `Code/Core/Light/SkyEvents.cs` | 2.6: reads `sky_events.json`; `HazeAt(date)` -> transmittance factor and redness; `CometAt(date)` -> RA, Dec, magnitude, tail length, or none. |
| `Kapman.Levels.Lamps` (MonoBehaviour) | `Code/Runtime/Levels/Lamps.cs` | Replaces the intensities in `TownLife`: every hearth, fire basket, bonfire and brazier is an HDRP point light in lumens from the 1.5 table, 1,850-1,900 K, flicker ±15% at 8-12 Hz on intensity only (never on position), `fadeDistance` 120 m. Shadows cannot be on all of them: Dorestad has 409 houses and 37 fire baskets, HDRP's cached punctual shadow atlas defaults to 2048² (`HDShadowManager.cs` line 385; 16 lights at 512, or 64 at 4096²) and `maxShadowRequests` is 128 (line 404). So: the nearest 16-24 lamps cast shadows (512 for the nearest 8, 256 beyond), the set re-chosen by distance once a second, `ShadowUpdateMode.OnEnable` so a cached shadow renders once when a lamp enters the set (a swap costs one 512² point-shadow render, about 0.3 ms, spread over the second); all other lamps unshadowed with range ≤ 6 m so they cannot leak through a neighbour's wall, plus an emissive quad at the smoke hole and the gable ends for the glow seen from outside (an unshadowed point light inside a closed blockout house leaks straight through its walls, so the "a hearth lights its own room and nothing else" of 1.5 comes from the range cap and the emissive, not from the light). `volumetricDimmer` 1 for the nearest eight and 0 beyond 80 m; contact shadows on the nearest four. `ProjectSetup.Configure` sets `cachedPunctualLightShadowAtlas` 4096 (the recipe's figure). |
| `Kapman.Levels.Hdrp.SkyLightProbe` (helper) | `Code/Runtime/Levels/Hdrp/SkyLightProbe.cs` | Reads `RenderSettings.ambientProbe` (the sky's SH, which HDRP writes back from its async readback) and evaluates E_up, E_horizon as colours for the passes and the mist, so no pass depends on `_SkyTexture` being bound. |

### 4.2 Where they hook in

- `LevelClock.Apply`: the Sun's ground lux and the cloud dimming come from `LightingModel` (`SunLux`, `CloudDim`) instead of the inline `cloudDim = lerp(1, overcastLight, overcast)` at line 260; `overcastLight` 0.45 goes. Because HDRP's volumetric clouds also shadow the Sun (`clouds.shadowOpacity` 0.5-0.9 in `HdrpWeatherDraw.PushClouds`, line 185), the clock divides the model's factor by (1 - shadowOpacity·cover) before it dims the HDRP Sun, so the ground gets the model's lux once, not the model's times HDRP's. `MoonLightLux` is `LightingModel.MoonLux(Sky.MoonLux, alt, overcast)` (the same arithmetic as line 298-300 today, named). `fill` comes from `EyeModel`. The HDRP Sun keeps its true 130,000 lux outside the air; the model decides the dimming, the fill and the eye. The clock owns the `EyeModel` and steps it with game time; `SkyDome.LateUpdate` reads `EyeModel.LimitMin` instead of `Dusk`-lerping its own floor; the `ShadowsMidtonesHighlights` (shadows and midtones only) and `FilmGrain.intensity` overrides on the runtime profile take the eye's values (through `HdrpVolume.Get`). The clock also owns `SkyEvents`: the haze factor multiplies `Transmittance`, and `SkyDome` draws the comet body and tail from `SkyEvents.CometAt`.
- `LevelWeather`: owns the `FrostModel` (stepped per surface class: Ground for the level, Ship and Rigging for the player's ship) and a `FlashModel`; `Frost` becomes `Hoar`, `Rime`, `RimeSide`, `SeaSmoke`; `Lightning` is raised per flash with the stroke train, and the clock's existing "Lightning" directional light reads `FlashModel.Sample` for its intensity, direction and colour; the cloud-glow value goes to `HdrpWeatherDraw` which lifts the LUT's blue channel and `ambientLightProbeDimmer` for those frames. The Sun is not moved.
- `HdrpWeatherDraw.Push`: passes hoar/rime/side and the sky's E_up from `SkyLightProbe` to the passes; a sea-smoke local fog volume over the water when `SeaSmoke` > 0.
- `Sight.RangeKm` (`Game/Sim/Sight.cs`): its night term (0.5 + 2.0·moonLit km) is multiplied by `EyeModel.NightSightScale` (0.2 at no adaptation, 1 at full), so a lit fire-pot cuts the lookout and 20 game minutes of dark restore him.
- `TownLife`: keeps the schedule (lit at dusk), hands the intensities to `Lamps`.
- `DorestadBuilder` / `OceanBuilder`: assign `weather.weatherShader` explicitly (so a player build does not strip the hidden shader), add the Boulogne brazier as a `Lamps` entry of kind Brazier, the emissive smoke-hole and gable quads per house, and the physical camera settings of 4.8.

### 4.3 Data files

`G:\kapman_unity\data\light\flames.json` (the 1.5 table: kind, lumen range, kelvin, range, shadow resolution); `twilight.json` (the three anchors, the two-piece fit, and the ocean levels' small-planet twilight table, the ten values now hard-coded in `SkyDome.twilightLog`, moved here so `SkyDome` loads them through `Json` and the test assembly, which references Kapman.Core only, can check the file against `LightingModel.TwilightLux` without touching Kapman.Runtime); `beacons.json` (place id, lit from year, candela: Boulogne 811 3,000 cd; Dover unknown); `sky_events.json` (2.6: chronicle id, haze span and factor, comet ephemeris rows); `weather_kinds.json` (RH per kind and the sea-smoke thresholds of 2.5). All read through `Json`.

### 4.4 Snow and frost: move from the full-screen pass to the surfaces

What the pass is today, and why it is off. `SnowFrostPass.Push` (line 84) has `bool groundOn = false && (snow > 0.002f || frost > 0.002f || wetness > 0.01f)` with the 4 October comment: "the ground pass blacked out every frame it ran on (every weather with wetness, snow or frost: the opaque image came back black at BeforePreRefraction, only the water and clouds drawn after it survived)". The cause is in the shader and the injection point, not in the lighting maths. The "Snow Frost Wet" pass of `Shaders/Hdrp/KapmanWeatherPasses.shader` reads `CustomPassLoadCameraColor(px, 0)` (line 81), which is a read of HDRP's *colour pyramid*, with `Blend Off` (line 71); the volume is injected at `CustomPassInjectionPoint.BeforePreRefraction` (`SnowFrostPass.cs` line 51). HDRP 17's Custom Pass Injection Points table lists BeforePreRefraction as "Color (no pyramid | Read | Write)" and the pyramid as first available at BeforeTransparent, "Color (Pyramid | Read | Write)" (https://docs.unity3d.com/Packages/com.unity.render-pipelines.high-definition@17.0/manual/Custom-Pass-Injection-Points.html). So the read returned black and, with Blend Off, the pass wrote black over every pixel including the sky, and only what HDRP drew afterwards (the water, a transparent; the clouds) survived: exactly the recorded symptom. `LevelWeather.Targets` gives `wet` 0.3 to Fog, 0.2 to Mist, 0.1 to Overcast, 1 to Rain and Storm, 0.2 to Snow and nothing to Fair or Haze, so the pass ran, and the frame went black, in exactly the kinds the 3 October sheets showed black, and not in the kinds that rendered. P14 (shot after the pass was disabled) therefore shows nothing about the pass; it shows the scene without snow because the pass is off.

Three steps, in order:

1. Fix the read (one evening). Either move the volume: `volume.injectionPoint = CustomPassInjectionPoint.BeforeTransparent`, keep `fetchColorBuffer = true`, and read with `CustomPassSampleCameraColor`/`CustomPassLoadCameraColor` at LOD 0; the water is a transparent and is still not drawn at that point, so the "under the water" rule holds. Or stay at BeforePreRefraction and bind the camera colour yourself (a `CustomPass` subclass using `GetCameraBuffers`, not a `FullScreenCustomPass`). The first is a one-line change and is the one to try first. Done when a Fog-kind shot (wetness 0.3) renders lit with the pass enabled, and a Fair one is unchanged.
2. Rewrite the lighting in the pass (the same evening, once 1 renders): the pass today builds the snow's colour from the picture's own luminance ×4, clamped between a floor and a ceiling both from `SampleSkyTexture(up)` (shader line 110); under a dark deck ×4 of dark is dark, so once the pass is on again it would still draw almost nothing in a snow shot. Compute the snow's radiance from the irradiance instead: `white = 0.85/π · (E_sky_up·(0.5+0.5·n.y) + E_sun·max(0,n·s) + E_moon·max(0,n·m)) · exposure`, with `E_sky_up` passed in from `SkyLightProbe` (CPU SH), and use the picture only for shadowing: `shade = clamp(lum / expectedLum, 0.2, 1)` where `expectedLum` is the same irradiance times a 0.15 ground albedo. The grey-card check for the cloud LUT that sets `E_sky_up`: under a uniform overcast L_sky = E/π and an 18% ground is 0.18·E/π, so the sky should read about 2.5 stops (5:1) over the ground, never 8. Frost: `col = lerp(col, white·0.9, hoar·upness·through + rime·windward·through)` where `windward = max(0, dot(n.xz, -windDir))`. Add a sparkle: `glint = step(0.97, hash(ws·40 + viewDir·3))·hoar·sunVisible`. Done when P14 re-shot shows white snow on the dam planks and the thatch under the overcast, darker in the shade than in the open, nothing on the posts' undersides, and the Fog shot of step 1 still renders.
3. Then (the right way): snow and frost as a material feature lit by HDRP itself: a global shader keyword `_KAPMAN_FROST` with `_KapmanSnow`, `_KapmanHoar`, `_KapmanRime`, `_KapmanWindDir` globals, read by a Shader Graph sub-graph. The ship's materials first, because the ship is the object 2 m from the camera in the brief's frosty-morning-at-sea: the rope and shroud material (rime on the windward side, comb-like streaks from a noise stretched along the wind), the spar and mast, the deck plank and gunwale (hoar on the up-facing), the sail (pale, stiffened); then the terrain layers (an extra "snow" layer whose weight is the global times the up-mask times noise), the thatch, the land planks and the reed details. Then snow takes SSAO, contact shadows, the Sun's PCSS shadows, APV sky occlusion and SSR sparkle without any pass, and the pass is left only for the aurora and for props without the sub-graph.

### 4.5 Exposure, stars and Moon: one owner

Today `ProjectSetup` writes starLight 3 / limits -5..15, `SkyDome` overrides starLight 5 / floor -6..ceiling 14 every frame, and the clock boosts the Moon ×3. Make `SkyDome` the single owner at runtime (it already is in effect), set ProjectSetup's values to match (starLight 5, limits -6/14), and expose on SkyDome the three cheats of 2.2 under one header "The eye" with the EyeModel's time constants.

### 4.6 The black fog and black mist: already explained, a re-shoot to confirm

The black fog and the black mist of `Captures/sheet_diag_fog.jpg` (05:13) and `sheet_diag_fog2.jpg` (05:19) were the ground custom pass of 4.4, which is now off. The evidence, in order:

- Both sheets were shot before commit 1cedc25 (05:23), the commit that disabled the pass. The pass ran whenever wetness > 0.01, i.e. in Fog, Mist, Overcast, Rain, Storm and Snow and never in Fair or Haze, and it overwrote the whole frame with black (4.4). That is why all eight DiagFog variants (non-volumetric, depth 300, anisotropy 0, quarter density, top 400, constant colour) are pixel-identical: no fog knob could show through a pass that painted over the frame.
- `sheet_diag_fog2.jpg` already contains the "decisive" tests an earlier draft of this document proposed: "G fog no clouds" and "G fog no clouds fixed 12 EV" are wholly black (so the cloud deck darkening the sky cubemap was not the cause), and "G fair fixed 12 EV" is perfect (so the per-camera sky lighting context was not the cause either: Fair renders through the same context).
- After the commit, `sheet_dorestad_hdrp7.jpg` shows P10 dawn mist, P13 storm and P14 snow lit, and `sheet_sea_hdrp7.jpg` shows N8 sea fog at dawn as a lit grey fog.

What remains is to prove it on the current commit and then judge the fog on its merits: re-run `Batch.DiagFog` unchanged (no new variants) as an unattended GPU job; the eight variants should now differ from one another and none be black. Then re-enable `PushMist` (remove the `false &&` at `HdrpWeatherDraw.cs` line 260) and the 3D mask (`LocalVolumetricFog` reads an RGBA32 mask as RGB tint and alpha density, which is the form the mask has; `LocalVolumetricFog.cs` line 278), and judge the mist on its own sheet.

One gotcha to keep from the old diagnosis: `Fog.color` is an HDR colour in scene-linear absolute units, multiplied by the exposure in the shader (AtmosphericScattering.hlsl line 328: `GetFogColor(...) * GetCurrentExposureMultiplier()`), so a 0.7 grey at a daylight EV of 13 is 0.7/(1.2·8192) = black. A constant-colour fog test at noon needs a value of the order of thousands.

### 4.7 Night specular fireflies on ropes at 4K

The pinpoints on the rigging in N7 are specular aliasing: sub-pixel rope geometry with a smooth material reflecting (a) the star cube at ×5 in the sky reflection (the GGX convolution keeps the stars sharp at low roughness) and (b) the Moon's disc. Fixes, cheapest first, all in the builder's material setup and the camera:

1. Rope and spar materials first: smoothness 0.3 (hemp and tarred wood are not glossy), and enable Geometric Specular AA on the HDRP Lit material (`_EnableGeometricSpecularAA` 1, `_SpecularAAScreenSpaceVariance` 0.5, `_SpecularAAThreshold` 0.3): this is the fix made for thin geometry. Re-shoot N7.
2. Camera second: TAA quality High, `taaAntiFlicker` 0.8 (default 0.5, `HDAdditionalCameraData.cs` line 338), `taaAntiRinging` on; the shots already force TAA (Shots.Render).
3. Only if the glints remain: keep ×5 for the *visible* sky but give the lighting sky ×1 through the lighting override volume (`skyManager.lightingOverrideLayerMask`, HDCamera.cs line 1881): a second volume on a "Sky lighting" layer with `spaceEmissionMultiplier` 1. The cost: a lighting-override sky is a second Physically Based Sky that `SkyDome` must keep in sync every frame (planet radius, space rotation, tints), and it doubles the sky cubemap work; that is why it comes last. The diag shot "X night no stars" was the right experiment; it should now be read against this.
- Moon disc: already dimmed by `surfaceTint` to a few nits at night; keep.

### 4.8 The camera

Physical camera on the main camera: `usePhysicalProperties`, sensor 36×24, focal 35 mm for the chase view (50 mm close), gate fit horizontal, ISO 200, shutter 1/50 (motion blur reference only; `MotionBlur.intensity` stays 0 per the user's rule, or 0.2 if the user allows), aperture 5.6 (DoF off in play; physically based DoF only for stills), blade count 7. Exposure stays Automatic Histogram (the eye model needs the wider range a physical ISO/shutter would not give). No vignette, no chromatic aberration, no lens distortion, no LUT: the only "look" is ACES, contrast 8, saturation -8, lifted blacks 0.015, Thin1 grain 0.28 (0.45 at full dark adaptation) and `dithering` on the camera to kill banding in fog. Adaptation lag: HDRP's progressive 3/1 for seconds; the EyeModel for the 20-game-minute dark adaptation.

### 4.9 Global illumination and shadows

Adaptive Probe Volumes with sky occlusion and sky direction (the recipe's section 1 has the exact settings), baked headless per level, is what makes the blue shadows of the frosty morning and the dark under the eaves at dusk; SSGI (on today in `ProjectSetup`) is screen-space only and shimmers in fog; keep SSGI off by default and APV on, SSGI as a local override inside halls. The HDRP asset already has `lightProbeSystem: 1` (APV) but `supportProbeVolumes: 0` in the global settings asset: the builder must turn that on and put one global `ProbeVolume` in each level. Shadows: the Sun's PCSS at 4096 to 900 m (as `ProjectSetup` sets: `maxDirectionalShadowMapResolution` 4096), cascades 0.04/0.12/0.35; the Moon takes the one shadow-casting directional below -10° (as the clock does); lamps per 4.1: the nearest 16-24 shadowed from a 4096² cached punctual atlas, the rest range-capped and unshadowed.

### 4.10 Tests (EditMode, headless, `Assets/Kapman/Tests/EditMode`)

| Test | Asserts |
|---|---|
| `LightingModelTests.SunAtJuneNoon52N` | `SkyMath` gives 61.4° ± 0.3 at Dorestad on 21 June 793 at local noon; `SunLux` + `SkyLux` = 90,000-110,000 |
| `LightingModelTests.Sunset` | at 0°: 300-500 lux; at -6°: 3.0-3.8; at -12°: 0.02-0.04; at -18°: 1e-4..3e-4; below -18°: 0; no NaN at any altitude from -90° to 90° |
| `LightingModelTests.Overcast` | June noon: direct Sun 0 at overcast 1; summed 1,200-1,800 lux; storm 100-200; December noon overcast 300-600; `CloudDim(1, 0)` × E_direct = 0 |
| `LightingModelTests.Moon` | `MoonLux(0.31, 45°, 0)` = 0.18-0.24; `MoonLux(0.028, 45°, 0)` = 0.015-0.025 (the quarter Moon by SkyMath's magnitude); overcast ×0.3; below the horizon 0 |
| `LightingModelTests.EV100` | against the summed model: noon clear 15-16; overcast noon 9-10; storm noon 6-7; full Moon (0.31 lux) -3.5..-2.5; starlight -11..-9 |
| `FrostModelTests.Ship` | Rigging class at sea, -3 °C, RH 95, wind 2, no fog: rime > 0.5 after 3 h, side = windward; Ship class same night: hoar > 0.3, rime 0; Beaufort 5 spray clears the deck's hoar in one step; `SeaSmoke` > 0.5 at T_water - T_air = 10 °C, 0 at 3 °C or wind 7 m/s |
| `SkyEventsTests` | `HazeAt(939-09-01)` transmittance factor 0.6-0.8, `HazeAt(941-01-01)` 1; `CometAt(837-04-11)` present with tail ≥ 40°, `CometAt(1066-04-27)` present, `CometAt(900-01-01)` none; every row's id exists in chronicle_793.json |
| `EyeModelTests.DarkAdaptation` | from EV 10 to EV -8: `Adaptation` reaches 0.63 at 20 ± 1 game minutes and 0.95 by 60; back to day in under 30 game seconds |
| `EyeModelTests.LanternResets` | a lantern (EV 2 in frame) drops adaptation to < 0.2 within 15 game seconds |
| `FlashModelTests.Train` | over 10,000 seeded flashes: median strokes 3-4, max ≤ 30, intervals 40-50 ms, median duration 0.4-0.6 s, 25% ± 3% cloud-to-ground |
| `FrostModelTests` | clear, calm, -3 °C, RH 90: hoar > 0.5 after 4 h, rime 0; fog at -2 °C: rime > 0.5, side = windward; cloud 0.9 or wind 5 m/s: both 0; Sun at +2 °C clears hoar in ≤ 1 h |
| `FlamesTableTests` | every kind in flames.json has lumen > 0, kelvin 1,700-2,000, and candela = lumen/4π within 5% |
| `TwilightTableTests` | the ocean-level table in `data/light/twilight.json` (read through `Json`, Core only: the test assembly's `overrideReferences` lists Kapman.Core and the test runner, not Kapman.Runtime, and pulling HDRP into a `-nographics` run is fragile) falls within one decade of `LightingModel.TwilightLux` at every 2° step; `SkyDome` loads the same file |

None of these renders; the rendering checks are the DiagFog re-run of 4.6 and the Fog/P14 shots of 4.4, which the user reads.

### 4.11 Performance budget (RTX 4070, 1440p, 60 fps = 16.6 ms; all est., from the recipe's section 9)

| Item | ms (est.) | Note |
|---|---|---|
| Geometry: 320,000 terrain trees with impostors, 769,000 detail cells, 409 houses, the terrain's 13 layers | 4-6 | the largest single cost; depends on the impostor distance |
| HDRP Water System: sea + river at High256 with `waterFullCPUSimulation` | 2-3 | the recipe's 1.5-3 |
| Volumetric clouds, Performance 48/4, 256 cloud shadows | 2 | |
| Sun PCSS, 4 cascades at 4096 (one number, as ProjectSetup sets) | 1.5-2 | PCF saves ~0.6 |
| Volumetric fog Medium, `depthExtent` 500 | 1.2 | High 2.5-3 |
| SSR with PBR accumulation, opaque and transparent | 0.7-1 | |
| SSAO | 0.7 | |
| Post (exposure, grain, ACES, drain) | 0.8 | |
| TAA | 0.4 | |
| Sky update: period 0.25 s (`updateMode` Realtime), or OnChanged with the clock rounding the Sun's direction to 0.05° so the hash changes only then; today OnChanged re-renders whenever the Sun moves, i.e. every frame | 0.3 amortised | once a second is too coarse: at 1 s = 1 game minute the Sun moves 0.25° per update and the ambient probe and reflections step visibly at dawn and dusk |
| Lamps at night: 16-24 cached shadows, re-rendered only on a swap | 0.3-0.5 | 8 live-updated would be 2 ms |
| APV sampling | 0.3 | |
| The frost/snow pass (until 4.4 step 3 removes it) | 0.3 | 0.8 at 4K |
| **Sum** | **15.2-19.5** | |

The sum does not hold 60 fps at 1440p native at night in town; it holds by day at sea. DLSS Quality (or STP where there is no DLSS) is therefore the default at 1440p, not the fallback: at its 960p internal resolution the screen-space items (fog, SSR, SSAO, TAA, post, the pass) fall by roughly 40% and the sum lands at 11-14 ms. Night costs more than day by the lamps; a storm costs the clouds plus rain particles. Cut order: cloud shadows, PCSS to PCF, fog High to Medium, impostor distance.

---

## 5. First slice (three nights; the first is the one with a headless done-when)

**Night 1.**
(a) Kick off `Batch.DiagFog` unchanged on the current commit as an unattended GPU job (no new variants: it confirms the fog is fixed, 4.6). Its sheet is read the next morning; nothing else waits on it.
(b) `LightingModel` and `EyeModel` as pure C# in `Code/Core/Light/`, with the tests of 4.10 rows 1-6 (SunAtJuneNoon52N, Sunset, Overcast, Moon, EV100, DarkAdaptation, LanternResets). The only wiring is `SkyDome.LateUpdate` reading `EyeModel.LimitMin` in place of its own `Dusk` lerp (the clock owns and steps the eye). Done when the EditMode run is green: `Unity.exe -batchmode -nographics -projectPath G:\kapman_unity\Kapman2 -runTests -testPlatform EditMode -testResults <xml> -logFile <log>` (PROGRESS.md), with the new tests in the results file and no test skipped.

**Night 2.** The pass moved to `BeforeTransparent` (4.4 step 1) and the irradiance rewrite with `SkyLightProbe` (4.4 step 2); `LevelClock.Apply` taking its Sun lux and cloud dimming from `LightingModel` with HDRP's cloud-shadow opacity subtracted (4.2); the colour drain through `ShadowsMidtonesHighlights` and the grain through `HdrpVolume`. Done when a Fog-kind shot renders lit with the pass on, P14 shows white snow darker in the shade, and P12/N7 re-shot keep their framing with the sky no darker than before.

**Night 3.** `Lamps` with the shadowed-set swap and the emissive smoke holes (4.1), and `FrostModel` with its surface classes and tests (4.10), wired into `LevelWeather`. Done when the EditMode run is green with the frost rows and a January dawn shot of Dorestad shows hoar on the ridges and the dam planks and nothing on the undersides.

`SkyEvents`, the comet and the haze, the sea-smoke volume and the ship's frost materials (4.4 step 3) follow in the nights after.

---

## 6. Open questions for the user

1. **Eye or camera at night?** The design shows the night as a dark-adapted eye sees it (stars, the ship's outline, no colour) with three documented cheats. A camera would show a moonless night as black. Which is the window back in time?
2. **How far to push the lightning.** The design keeps the existing directional "Lightning" light for the strike (it lights the sea, the deck and the fog from the strike's side) and brightens the cloud deck from inside through the cloud LUT and the ambient dimmer. Pointing the Sun at the strike would let HDRP's clouds themselves light from the strike's side, but it costs a sky cubemap and ambient re-render on the swap frame, puts the Sun's disc on the sky at the strike, flips the Moon's shading, and the clouds' temporal accumulation (0.95) smears the 40-50 ms stroke train into a one-second blur unless accumulation is switched off for the frame, which then makes the clouds flicker. Accept the cheaper version, or is the storm important enough to try the Sun swap and judge it on a sheet?
3. **Beacons.** Boulogne's fire from 811 (the chronicle); Dover dark. Should the game light Dover's pharos too, as a guess, or keep to the sources?
4. **APV bake per level.** Blue shadows and dark eaves need a baked probe volume per level (minutes per level headless, ~450 levels eventually, disk space ×4 if scenarios are used). Accept the bake step in the level pipeline, or live with sky ambient and SSAO until the levels are final?
