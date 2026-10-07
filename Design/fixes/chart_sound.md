# Chart sound: the hook (7 Oct 2026)

Read from the files only. Unity not run, nothing compiled, nothing listened to.
Paths are under `G:/kapman_unity/Kapman2/Assets/Kapman/` unless they start with `Design/`.
Lines marked CHECK were not opened in the time given; the names there are my best reading and must be looked at before pasting.

## 1. Why the chart is silent, and the smallest fix

- `Scenes/L_Chart.unity` has an AudioListener and no source (Design/sound_audit.md section 5).
- The player is `Code/Runtime/Audio/LevelSoundscape2.cs`. Its `Context(Vector3 at)` (lines 183-221) reads four 3D-level objects:
  `LevelWeather.Current`, `LevelClock.Current`, `LevelInfo.Current`, `ShipMotion.Player`. All four are null in the chart.
  It does not crash on null: it falls to wind 3 m/s, no rain, hour 12, day 172, shore 0, ship 1e6 m away.
  So dropping it in the chart as it is gives a fixed summer noon on a river bank with no ship. That is wrong, not silent.
- The data (`Audio/soundscape.json`, 31 layers) and the level maths (`Code/Core/Audio/SoundscapeModel.cs`) need no change.
- The chart already knows everything the context wants:
  - weather: `ChartWorld.TryWeather(out WeatherSample)` (`Code/Game/Chart/ChartWorld.cs` line 135), which is `WorldSim.WeatherAt` (`Code/Game/Sim/WorldSim.cs` line 130)
  - river or sea: `ChartWorld.OnRiver()` (ChartWorld.cs line 153)
  - hour and day: `KapmanGame.Current.Hours`, `.Season`, `.Month` (`Code/Game/KapmanGame.cs` lines 100-109), the clock's `DayOfYear`
  - her speed: `KapmanGame.Current.ship.Speed` (`Code/Game/Shims/SailingShip.cs` line 109), `Rowing` (line 127, today only "speed > 0.05")
  - in port: the mooring watch in `Code/Game/Encounters/EncounterDirector.cs` (`MooredAt`, line 52)
- `Kapman.Game.asmdef` references `Kapman.Runtime` and `Kapman.Core`, so Game code may set a static on LevelSoundscape2. Runtime must not name Game types, so the feed is a delegate.

The fix is three small edits and one new file:
1. LevelSoundscape2: a static feed that replaces `Context(at)` when set (section 4.1).
2. LevelSoundscape2: a `mask` list of layer ids to keep silent, and a public `Strike(metres)` for thunder (section 4.1).
3. New `Code/Game/Chart/ChartSound.cs`: fills the context each frame (section 4.2).
4. In the scene (done in Unity by whoever has the editor, not here): one object "Sound" at the AudioListener with LevelSoundscape2 (data + clips) and ChartSound (section 4.3).

## 2. What the chart must feed each frame

One `SoundContext` (`Code/Core/Audio/SoundContext.cs` lines 7-31). Every field, where it comes from in the chart:

| field | unit | chart source | note |
|---|---|---|---|
| wind | m/s at 10 m | `sample.windSpeed` | real m/s already (Core/Weather/WeatherModel.cs line 35) |
| rain | 0-1 | `sample.rain` is mm an hour: `Mathf.Clamp01(sample.rain / 8f)` | the json crossfades at 0.1 / 0.45 / 0.9, so 0.8 mm/h = light, 3.6 = steady, 7+ = heavy. CHECK how LevelWeather turns mm/h into its 0-1 `Rain` and use the same number |
| storm | 0-1 | `sample.storm` | cumulonimbus cover |
| temperature | C | `sample.temperature` | |
| humidity | 0-1 | `SoundscapeModel.HumidityFromWeather(kindName, hour, sun)` | kind name from the sample: "Storm" if storm > 0.5, "Rain" if rain > 0.1, "Fog" if fog > 0.4, else "Fair". CHECK the names the function knows (LevelSoundscape2.cs line 197 passes `WeatherKind.ToString()`) |
| cloud | 0-1 | `Mathf.Clamp01(sample.cumulus + sample.layered + sample.storm)` | |
| hour | 0-24 | `(float)date.hour` | `GameDate.hour` (Core/World/Calendar.cs line 9) |
| day | 1-365 | `date.dayOfYear` | the season of every bird voice hangs on this |
| sunAltitude | degrees | `Kapman.Nature.Presence.ApproxSunAltitude(lat, day, hour)` (`Code/Core/Wildlife/Presence.cs` line 133) | latitude from `WorldFrame.ToLatLon(x, z)` as WorldSim.WeatherAt does (WorldSim.cs line 134) |
| night | 0-1 | `Mathf.InverseLerp(0f, -6f, sunAltitude)` | 0 with the Sun up, 1 at the end of civil twilight |
| leaf | 0-1 | by day of year: 0 before day 90, up to 1 by day 135, 1 to day 275, down to 0 by day 320 | LevelWeather.Leaf (Runtime/Levels/LevelWeather.cs line 114) is not in the chart. CHECK its curve and copy it |
| aboveWater | m | 2 | the ear is on deck. Never the camera height: the chart camera is kilometres up and would fade the land cover out |
| shore | m, + over water | from the ground (below) | |
| reed, hedge, water, basin, town, wood, fields | 0-1 | from the ground (below), set by hand, NOT `GuessSurroundings` | the guess (SoundscapeModel.cs lines 228-240) is tuned for Dorestad's bank and puts a town 250 m inland everywhere |
| shipSpeed | m/s | `ship.Speed`, 0 when the game is paused or she is moored | CHECK it is m/s (line 109 gives Motion.Speed; maxKnots is separate) |
| shipRowing | 0-1 | 1 when rowed, else 0 | today: rowed on a river when she makes way (the same stand-in LevelSoundscape2 line 261 uses), sail at sea. When the ship state has a real "rowing" flag, read that |
| shipRollRate, shipPitchRate | deg/s | made up from the wind: `roll = 0.5 * force` at sea, `0.15 * force` on a river, 0 in port; pitch = 0.6 of roll | there is no hull motion on a chart. Force 5 at sea gives 2.5 + 1.5 deg/s, enough for ship_creak to speak. CHECK against `SoundscapeModel.HullWorkDb` |
| shipDistance | m | 0 | the listener is aboard |

### The ground, by where she is

Four grounds. The chart sets `shore` and the seven shares outright; blend over about 20 s when the ground changes so nothing snaps.

| ground | how the chart knows | shore | water | reed | hedge | wood | fields | town | basin |
|---|---|---|---|---|---|---|---|---|---|
| in port (moored) | mooring watch says moored | +5 | 0.5 | 0.10 | 0.05 | 0.05 | 0.05 | 0.45 | 0 |
| river | `ChartWorld.OnRiver()` true | +40 | 0.6 | 0.30 | 0.10 | 0.15 | 0.20 | 0 | 0.05 |
| coast | not river, and the leg or nearest lane is `LaneGraph.Coastal` or `Sound` | +400 | 0.9 | 0.05 | 0 | 0 | 0.05 | 0 | 0 |
| open sea | not river, lane is `LaneGraph.Sea` or none | +5000 | 1 | 0 | 0 | 0 | 0 | 0 | 0 |

- River and sea come free from `ChartWorld.OnRiver()`.
- Coast against open sea: `OnRiver()` already reads the lane kind (ChartWorld.cs lines 184-186: `River`, `Pending`, `Sea`, `Coastal`, `Sound`) but throws it away as a bool.
  Smallest change: keep the kind it found in a static next to `riverNow` (line 150) and add `public static bool OnCoast()`. CHECK lines 153-195 before writing it. Until then treat all sea as open sea.
- In port: CHECK how the frame knows she is moored (EncounterDirector.cs lines 37-82, `MooringWatch.MooredAt`; the watch may be private to the director). The draft below takes it through one bool the chart sets, so the hook does not care where it comes from.

## 3. Which of the 31 layers fit a chart and a small ship view

The player sees a chart and a small picture of the ship. She is always aboard. So: weather and ship always, the bank only when the bank is near, land birds thinned.
"Play" = leave as the json has it. "Mask" = on the mask list in the chart scene (section 4.1), so the json stays as it is for the 3D levels.

| # | layer (json line) | kind | chart | why |
|---|---|---|---|---|
| 1 | wind (5) | crossfade | play | the main weather voice. Both clips are wind in grass (audit section 6): fine on a river, wrong at sea until a sea wind is found |
| 2 | rain (8) | crossfade | play | |
| 3 | lapping (12) | bed | play | river and port only; the shore value keeps it off at sea |
| 4 | thunder (15) | event | play, by `Strike` | needs a caller: the chart has no LevelWeather.Lightning. See 4.2 |
| 5 | reeds (20) | bed | play | river only (reed share) |
| 6 | reeds_shore (23) | bed | play | river and port |
| 7 | leaves (26) | bed | play, low | river only, summer. Has a cuckoo and spring birds baked in: it will sound in August. Mask it if that grates |
| 8 | bare_trees (29) | bed | play | river, winter |
| 9 | dry_grass (32) | bed | mask | inland field sound; she is never in a field |
| 10 | rain_thatch (36) | bed | play | in port only (town share). It is canvas, which also passes for rain on the awning |
| 11 | rain_water (39) | bed | play | everywhere afloat |
| 12 | ship_bow (43) | crossfade | play | THE sound of being under way. Speed must be real m/s |
| 13 | ship_wake (47) | bed | play | when rowed |
| 14 | ship_oars (50) | crossfade | play | when rowed |
| 15 | ship_creak (54) | bed | play | driven by the made-up roll and pitch |
| 16 | nightingale (58) | voice | play | river and port, May nights |
| 17 | blackbird (61) | voice | play | river and port |
| 18 | skylark (64) | voice | mask | a bird over fields far from the water; odd from a deck |
| 19 | swallows (67) | voice | play | over the river in summer, and at a town |
| 20 | gulls (70) | voice | play | port, coast, river mouth. The one bird that belongs at sea |
| 21 | curlew (73) | voice | play | coast and estuary |
| 22 | heron (76) | voice | play | river |
| 23 | mallard (79) | voice | play | river and port |
| 24 | rooks (82) | voice | play | river, by woods |
| 25 | crows (85) | voice | play | river and port |
| 26 | hooded_crow (88) | voice | play | winter, coast and port |
| 27 | geese_skein (91) | voice | play | overhead anywhere, autumn and spring. Good at sea |
| 28 | geese_meadow (94) | voice | mask | grazing flock on land |
| 29 | tawny_owl (97) | voice | play | river and port nights |
| 30 | dawn_dusk_chorus (101) | group | play | it keeps one member calling. CHECK SoundVoices.cs that a group whose members are all shut by place stays quiet at sea (members: blackbird, skylark, rooks, nightingale, swallows, crows; none belongs on open sea) |
| 31 | night_chorus (104) | group | play | same check: nightingale, tawny_owl, geese_meadow must not call at open sea |

Fit: 28 play, 3 masked (dry_grass, skylark, geese_meadow). At open sea only 8 can speak: wind, rain, rain_water, thunder, ship_bow, ship_creak, gulls (if their place rule lets them), geese_skein.

The one real risk is the place rules at sea: each voice has its own place condition in the json (lines 58-99) reading `surround.*` and `shore`. With the open-sea row of section 2 (all land shares 0, shore 5000) any voice that asks for wood, field, reed, town or a near shore is shut. CHECK each voice's `when` for one that asks for nothing and would sing at sea; and the two groups, which FORCE a member to call.

Missing for the chart (from the audit, not to be made here): a sea wind, an open sea bed, a dry timber creak, a sail. Until they are found the open sea is wind in Texas grass plus a bow wave. If that is worse than quiet, turn the wind down at open sea only and let the bow wave and the creak carry it.

## 4. The hook as code

### 4.1 `Code/Runtime/Audio/LevelSoundscape2.cs` (four edits)

(a) After line 30 (`public static LevelSoundscape2 Current { get; private set; }`), add:

```csharp
        /// <summary>Where there is no 3D level (the chart): who fills the context instead of Context(at). Null in a level.</summary>
        public static System.Func<SoundContext> Feed;
        /// <summary>The wind the calls bend in when there is a feed (x east, y north, m/s).</summary>
        public static Vector2 FeedWind;
```

and after line 43 (`[Range(1, 24)] public int voicePool = 12;`), add:

```csharp
        [Tooltip("Layer ids that stay silent in this scene (the chart: dry_grass, skylark, geese_meadow).")]
        public string[] mask = new string[0];
        [Tooltip("Play nothing until something has set the Feed (the chart), instead of the level defaults.")]
        public bool waitForFeed;
```

(b) In `Awake`, as the first line inside `foreach (var layer in def.layers)` (line 104):

```csharp
                if (System.Array.IndexOf(mask, layer.id) >= 0)
                    continue;
```

This drops beds and crossfades. Voices are built by `new SoundVoices(def, transform, Clip, flat, voicePool)` (line 119) from the whole def, so the mask must reach them too.
CHECK `Code/Runtime/Audio/SoundVoices.cs` lines 56-108: add a `string[] mask` argument to the constructor, skip masked ids where it lists the voices, and skip them as group members. Call becomes `new SoundVoices(def, transform, Clip, flat, voicePool, mask)`.

(c) In `Update` (line 280 on). Change line 293

```csharp
            var ctx = Context(at);
```

to

```csharp
            if (waitForFeed && Feed == null)
                return;
            var ctx = Feed != null ? Feed() : Context(at);
            if (Feed != null)
                rh = ctx.humidity;
```

and line 329

```csharp
            Vector2 windWay = w != null ? w.WindVector : Vector2.zero;
```

to

```csharp
            Vector2 windWay = w != null ? w.WindVector : Feed != null ? FeedWind : Vector2.zero;
```

(d) Thunder. `OnLightning(float metres)` (line 341) is private and hooked only to `LevelWeather.Lightning` (line 288). Add under it:

```csharp
        /// <summary>A lightning flash this far off (m), from a scene with no LevelWeather (the chart).</summary>
        public void Strike(float metres) => OnLightning(metres);
```

Nothing else in LevelSoundscape2 changes. The `OnShip` sources (line 305) stay at the object's own position when `ShipMotion.Player` is null, which is the listener: right.
`Feed` is null in every 3D level, so L_Dorestad and L_NorthSea behave exactly as before.

### 4.2 New file `Code/Game/Chart/ChartSound.cs`

```csharp
using Kapman.Audio;
using Kapman.Weather;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// THE CHART'S SOUND: the chart has no level, so no LevelWeather, clock, shore map or hull. This fills the sound
    /// context from the game's own state each frame (the weather where she is, the hour and the day, the ground by where
    /// she is, her speed and the oars) and hands it to LevelSoundscape2 by its Feed. Sits beside it on the listener.
    /// </summary>
    public class ChartSound : MonoBehaviour
    {
        public enum Ground { Port, River, Coast, Sea }

        [Tooltip("Seconds to blend from one ground to the next.")]
        public float groundBlend = 20f;
        [Tooltip("Rain in mm an hour that counts as the heaviest (1).")]
        public float heavyRainMm = 8f;
        [Tooltip("Thunder: the mean wait between flashes at full storm, seconds of real time.")]
        public float flashEvery = 40f;
        [Range(0f, 1f)] public float level = 0.35f;
        [Range(0f, 1f)] public float readingLevel = 0.18f;

        /// <summary>Set by whoever knows she is tied up (the frame's port state).</summary>
        public static bool Moored;
        /// <summary>Set while a long text is open (FramePassage, PersonTalk).</summary>
        public static bool Reading;
        /// <summary>The settings slider: 0 off, 1 as designed, up to 2.</summary>
        public static float Volume = 1f;
        /// <summary>The ground now, for the dev panel.</summary>
        public static Ground Now { get; private set; } = Ground.River;

        // shore, water, reed, hedge, wood, fields, town, basin
        static readonly float[][] grounds =
        {
            new[] {    5f, 0.5f, 0.10f, 0.05f, 0.05f, 0.05f, 0.45f, 0f    },   // Port
            new[] {   40f, 0.6f, 0.30f, 0.10f, 0.15f, 0.20f, 0f,    0.05f },   // River
            new[] {  400f, 0.9f, 0.05f, 0f,    0f,    0.05f, 0f,    0f    },   // Coast
            new[] { 5000f, 1f,   0f,    0f,    0f,    0f,    0f,    0f    },   // Sea
        };
        readonly float[] g = new float[8];
        System.Func<SoundContext> feed;
        bool started;
        float nextFlash, now;
        SoundContext last;

        void OnEnable()
        {
            feed = Fill;
            LevelSoundscape2.Feed = feed;
        }

        void OnDisable()
        {
            if (LevelSoundscape2.Feed == feed)
                LevelSoundscape2.Feed = null;
        }

        /// <summary>How loud: down for a game of reading, and down again while a long text is open.</summary>
        void Update()
        {
            var s = LevelSoundscape2.Current;
            if (s == null)
                return;
            float to = (Reading ? readingLevel : level) * Volume;
            now = Mathf.MoveTowards(now, to, Time.unscaledDeltaTime * (to < now ? 0.09f : 0.06f));   // 2 s down, 3 s up
            s.master = Mathf.Clamp01(now);
        }

        static Ground GroundNow()
        {
            if (Moored)
                return Ground.Port;
            if (ChartWorld.OnRiver())
                return Ground.River;
            return Ground.Sea;   // when ChartWorld.OnCoast() is added (section 2): OnCoast() ? Ground.Coast : Ground.Sea
        }

        SoundContext Fill()
        {
            var game = KapmanGame.Current;
            if (game == null || !ChartWorld.TryWeather(out WeatherSample s))
                return started ? last : Quiet();

            // the hour and the season
            var date = game.World.Date;                                  // WorldSim.Date (WorldSim.cs line 124): a GameDate
            float hour = (float)date.hour;
            float day = date.dayOfYear;
            var at = ChartWorld.ShipWorld();
            var (lat, lon) = Kapman.Game.Sim.WorldFrame.ToLatLon(at.x, at.z);   // CHECK the namespace of WorldFrame
            float sun = (float)Kapman.Nature.Presence.ApproxSunAltitude(lat, day, hour);

            // the ground, blended
            Now = GroundNow();
            var to = grounds[(int)Now];
            float k = started ? 1f - Mathf.Exp(-Time.unscaledDeltaTime / Mathf.Max(groundBlend / 3f, 0.1f)) : 1f;
            for (int i = 0; i < g.Length; i++)
                g[i] = Mathf.Lerp(g[i], to[i], k);
            started = true;

            // the weather
            float rain = Mathf.Clamp01(s.rain / heavyRainMm);
            string kind = s.storm > 0.5f ? "Storm" : rain > 0.1f ? "Rain" : s.fog > 0.4f ? "Fog" : "Fair";   // CHECK the names HumidityFromWeather reads
            float force = SoundscapeModel.Force(s.windSpeed);

            // the ship: no hull on a chart, so the roll and pitch are the wind's, by the ground
            var ship = game.ship;
            bool still = Moored || Time.timeScale <= 0f;                 // CHECK the chart's own pause flag
            float speed = ship != null && !still ? Mathf.Abs(ship.Speed) : 0f;   // CHECK: real metres a second
            bool inland = Now == Ground.River || Now == Ground.Port;
            float roll = Now == Ground.Port ? 0f : (inland ? 0.15f : 0.5f) * force;

            var c = new SoundContext
            {
                wind = s.windSpeed,
                rain = rain,
                storm = s.storm,
                temperature = s.temperature,
                humidity = SoundscapeModel.HumidityFromWeather(kind, hour, sun),
                cloud = Mathf.Clamp01(s.cumulus + s.layered + s.storm),
                hour = hour,
                day = day,
                sunAltitude = sun,
                night = Mathf.InverseLerp(0f, -6f, sun),
                leaf = Leaf(day),
                aboveWater = 2f,
                shore = g[0], water = g[1], reed = g[2], hedge = g[3], wood = g[4], fields = g[5], town = g[6], basin = g[7],
                shipSpeed = speed,
                // as LevelSoundscape2 line 261, until she has a real rowing state: rowed on a river, under sail at sea
                shipRowing = inland && speed > 0.05f ? Mathf.InverseLerp(0.05f, 0.6f, speed) : 0f,
                shipRollRate = roll,
                shipPitchRate = roll * 0.6f,
                shipDistance = 0f,
            };

            // the wind the calls bend in: windFrom is compass degrees it blows FROM
            float rad = (s.windFrom + 180f) * Mathf.Deg2Rad;
            LevelSoundscape2.FeedWind = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) * s.windSpeed;

            Thunder(s.storm);
            last = c;
            return c;
        }

        /// <summary>The leaves by the day of the year: bare to day 90, full by 135, full to 275, bare by 320.</summary>
        static float Leaf(float day) =>
            Mathf.Min(Mathf.InverseLerp(90f, 135f, day), Mathf.InverseLerp(320f, 275f, day));

        /// <summary>Before the world is made: a still noon on the river, so nothing blares.</summary>
        static SoundContext Quiet() => new SoundContext
        {
            wind = 2f, temperature = 12f, humidity = 0.8f, hour = 12f, day = 172f, sunAltitude = 45f, leaf = 1f,
            aboveWater = 2f, shore = 40f, water = 0.6f, shipDistance = 0f,
        };

        /// <summary>Flashes under a storm cloud: the more cloud the more often, 2 to 14 km off.</summary>
        void Thunder(float storm)
        {
            if (storm < 0.4f || LevelSoundscape2.Current == null)
            {
                nextFlash = 0f;
                return;
            }
            if (nextFlash <= 0f)
                nextFlash = Time.unscaledTime + flashEvery / storm * Random.Range(0.4f, 1.6f);
            if (Time.unscaledTime < nextFlash)
                return;
            nextFlash = 0f;
            LevelSoundscape2.Current.Strike(Random.Range(2000f, 14000f));
        }
    }
}
```

Guesses in that draft, to check against the source before it is compiled:
- `game.World.Date` exists (WorldSim.cs line 124) but is the world sim's clock; KapmanGame's private `clock` has `Date` and `DayOfYear` too (KapmanGame.cs lines 105, 107). Use whichever the frame's date line uses, so the ear and the HUD agree.
- `WorldFrame.ToLatLon` namespace (called unqualified at WorldSim.cs line 134).
- `SailingShip.Speed` is `Motion.Speed` of a shim (Code/Game/Shims/SailingShip.cs lines 59, 109). In the chart the ship is moved by `Code/Game/Chart/ChartBody.cs` (class at line 23). CHECK that ChartBody writes the speed into that Motion, in m/s of the REAL world, not chart units a second. If the chart speed is scaled (the chart shortens the seas), feed the real speed through the water instead (`Travel.Speed`, ChartBody.cs line 16 comment), about 0 to 3.5 m/s. The json fades ship_bow from lapping at 0.3 m/s to bow wave at 3, and ship_oars from 0.6 to 1.6.
- `ChartSound.Moored` needs one writer. Smallest: where the director learns of a mooring (EncounterDirector.cs lines 239-240, `Change.Moored` then `BeginCall`) set `ChartSound.Moored = true`; on `Change.CastOff` set it false. Also set it when a game starts or loads in a port (`ChartWorld.StartAt`, ChartWorld.cs line 94).
- `ChartSound.Reading` needs writers in `Code/Game/UI/Frame/FramePassage.cs` and `PersonTalk.cs` (open = true, shut = false). Not opened.
- Thunder distance is random, not from the weather model; the model has storm cloud cover only. Good enough for a chart.
- The chart clock runs 1 s = 1 min, so the dawn chorus comes and goes in about two minutes and night falls in the ear every 24. Voices fire at random waits in real seconds, so they still sound natural. Nothing to change; just expect it.

### 4.3 The scene (done in the editor; not by this note)

In `Scenes/L_Chart.unity`:
1. An empty object "Sound", a child of the object that holds the AudioListener (scene file line 534), local position 0. The ship layers have `"spatial": 0.7`, so they must sit on the listener or they pan and fade with the chart camera.
2. Add `LevelSoundscape2`: `data` = `Audio/soundscape.json`; `clips` = the 38 named clips; `shoreMap` = none; `master` = 0.35; `mask` = dry_grass, skylark, geese_meadow; `waitForFeed` = on.
   The setup tool that fills `clips` is named `SoundscapeSetup.AddTo` in the code comment (LevelSoundscape2.cs line 133) and the audit puts it at `Editor/Audio/SoundscapeSetup.cs`. It is NOT under `Assets/Kapman/Code/` (searched, no such file): look in `Assets/Kapman/Editor` or search the project for `SoundscapeSetup`.
3. Add `ChartSound` on the same object.
4. In a build the clips must be on the component: the editor finds clips the data names by itself, a build has only those the scene was given (comment at line 133).
5. Do NOT add the older `LevelSoundscape` (Runtime/Levels/LevelSoundscape.cs); `LevelRoot` adds it in levels (LevelRoot.cs line 85) and the chart has no LevelRoot.

## 5. How loud, beside a game of reading

The player reads for minutes at a time. Sound is the room, not the show.

- `master` 0.35 in the chart (the 3D levels use 0.7, LevelSoundscape2.cs line 39): about 6 dB under the levels. `ChartSound.level` owns it (the Update in 4.2).
- One sound slider in the frame's settings (`ChartSound.Volume`, 0 to 2, default 1). No music/effects split: there is no music.
- While a long text is open (a reading from the chest, a letter, a person talking), a further 6 dB down over 2 s (`readingLevel` 0.18) and back over 3 s when it shuts. Not to silence: the weather should still be there under the page.
- Birds: sharp calls pull the eye off the page. The wildlife mask already thins them in rain and wind. For the chart also halve how often they call: CHECK SoundVoices.cs for the wait between calls and give its constructor a rate multiplier (chart 0.5) rather than editing the json, which the 3D levels share.
- Near thunder ducks everything else (json line 15, `duckWithinM`). `Strike` in the draft never comes nearer than 2 km, so a crack never lands in the middle of a sentence. A wreck or a storm card can call `Strike(300f)` on purpose.
- Paused or in a menu: `shipSpeed` is 0 when still; the pause sheet should set `Reading` true or `Volume` 0.
- No UI clicks, page rustles or coin sounds come out of this: none exist in the project, and sound is recordings only (CC0 or public domain, never made up). If wanted, they are a list to find, not to make.

## 6. Order of work, and what proves it

1. Edit LevelSoundscape2 (4.1 a, c, d) and add ChartSound. Compile. The existing tests must still pass: `Feed` is null in every level.
2. Scene object (4.3). Play: `LevelSoundscape2.Loudest` (shown by `Code/Runtime/Levels/DevHud.cs` lines 75-76 as "Sound: ...") lists the three loudest layers and the Beaufort force. It should change with the weather the frame shows. If DevHud is not in the chart, log `Loudest` once a second while testing.
3. Edit-mode test, no scene needed: build a context for each ground and assert through `SoundscapeModel.LayerDb(layer, ctx)` (SoundscapeModel.cs line 116) that
   - open sea, force 5, speed 3: `lapping`, `reeds`, `leaves`, `rain_thatch` are under -60 dB; `ship_bow` is above -40;
   - port, speed 0, rain 0.5: `ship_oars` under -60, `rain_thatch` above -50;
   - river, speed 1.2: `ship_oars` and `ship_wake` above -50.
   For that, split the context building out of `Fill` into `public static SoundContext For(Ground, WeatherSample, float hour, float day, float sun, float speed)`. The dB numbers are guesses to be set from the first run.
4. Mask (4.1 b) and the SoundVoices mask argument.
5. `OnCoast`, the `Moored` and `Reading` writers, the slider.

## 7. Not done, not checked

- Nothing was compiled or run. Every CHECK above is a line I did not open.
- `Audio/soundscape.json` was read for layer ids and line numbers only; the `when` conditions of the 14 voices and the two groups were not read. Section 3's fit for birds is from the audit's description, not from the conditions.
- `SoundVoices.cs`, `SoundscapeDef.cs`, `ChartBody.cs` (beyond its header), `EncounterDirector.cs`, `FramePassage.cs`, `PersonTalk.cs` were not read.
- Design/DECISIONS.md (7 October, items 1 to 16), Design/QUEUE.md and Design/wiring_plan.md section 8 were not opened. If a ruling there speaks of sound or of the ship view, it wins over this note.
- A first try at writing this file by shell failed and left a 12-line first pass; this text replaced it whole. Nothing else was written anywhere.
