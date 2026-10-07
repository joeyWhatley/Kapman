# Risk is real: one boat, and the same dice for her

Written 7 October 2026 for the strategy critic's third change (`Design/critique/review_systems.md`, cons 9 and 10,
change 3: "the same gale dice for her as for the other ships (warned first), and sail, leak and shares kept in the
simulation instead of in card flags"). It is written for the clock of ruling 26 (a pause and five speeds).

Nothing under `Assets` was changed. Unity was not run. Paths are under `Assets/Kapman/Code/` unless said; line
numbers are of the files as read today.

**What was checked, and how.** Every edit below was applied to copies of the project's files by a script that
refuses an edit whose old lines are not found exactly once (24 edits, 12 files, all found). `Kapman.Core` and
`Kapman.Game` were then compiled outside Unity with Unity's own compiler and the project's own reference list, with
the new files in: no errors, no warnings in the new or changed files. The new test file was compiled against those
two and **run outside Unity** by a small runner: **22 tests, 22 pass**. The weather figures in 1.6 and the table in
section 5 come from running the game's own code (`WeatherModel`, `Boat`) outside Unity. What could not be checked is
in section 13.

---

## 0. In short

1. **One boat.** A new static, `Boat` (beside `Voyage`), holds what her boat truly has on the chart: her hull's
   soundness, the water in her, the sail's cloth and how it is set, the order for a gale, the men's weariness, and
   whether they sail on shares. It is saved with the game.
2. **The cards write to it without being rewritten.** Nine of the deck's flags (`sea_leak`, `sea_sail_weak`,
   `sea_reefed`, `sea_lashed`, `sea_spar_aboard`, `sea_after_gale`, `crew_on_shares`, `crew_own_bundles`,
   `crew_boat_tarred`) become the boat: a card that sets one changes the boat, a card that
   reads one reads the boat. The deck keeps no second copy. So the content pass now under way is not disturbed.
3. **The simulation reads it.** Her speed on the chart is the world's ships' own rule (`Travel.Speed`) through her
   sail, her water and her men. The water in her and the men's bundles take room in the hold. Shares halve the daily
   wage and pay a tenth of the season's gain at Martinmas.
4. **The same dice.** `WorldSim` and `Boat` both call `GaleDice.Chance(perHour, fragility, state)`. The world's
   ships have state 1. Hers is 1 when she is sound, dry, fresh, full-handed and lying to, and grows with her hurts
   and her choices (running under shortened sail 2.5, carrying sail 8, a started strake 2, an opened hull 5...).
5. **Warned first, wounded before she is lost.** The weather model can be asked what will blow, so the men read a
   gale twelve game hours ahead: three signs (twelve hours, six, one), the clock dropping at each, a card at the
   second and the third. No dice are thrown in a gale she had no sign of. A strike starts a strake first, takes the
   sail next, the boat last. She is also lost if she fills, which is arithmetic the player watches hour by hour.
6. **A wreck is the end** (the user, 2 October): the clock stops, saving is refused, the last save stands.
7. **Fair at speed 5.** Everything runs on the game-hour tick, which comes once for every hour at every speed. The
   throw for an hour comes from the seed and the hour's count, so it is the same at speed 1 and at speed 5, and the
   same after a load: loading again does not throw again.

Five things the user or the lead must rule are in section 12. The largest: **the card rules now take the most
dangerous option by themselves** when the clock is let run (1.7); with real dice that has to turn round.

---

## 1. What the code and the cards do today

### 1.1 How the world's other ships are lost

- `Game/Sim/Travel.cs:167`: `sheltering = windSpeed > 17.0;` and the speed is 0. Every ship in wind over 17 m/s
  lies to.
- `Game/Sim/WorldSim.cs:44`: `public double galeLossPerHour = 0.0004;` ("times her class's fragility").
- `Game/Sim/WorldSim.cs:709-714`, in `PlanMove`, once a game hour for each ship that is `Sailing` and not lying up
  for the night: `if (gale && s.rng.Chance(rules.galeLossPerHour * s.cls.galeFragility)) { s.state = ShipState.Lost; stats.Lost(this, s); return; }`.
  One throw, and she is gone for good (`ShipState.Lost` does nothing more: line 462).
- Each ship has her own dice (`SimShip.rng`, a `SimRandom` from the seed and her id), saved with the world.
- Fragility is in `Game/Data/ship_classes_793.json` (`gale_fragility`, "STARTING GUESS TO TUNE"): the Gislinge boat
  0.6 (line 60), a river barge 1, the seagoing ships 0.35 to 0.5.
- Not asked, but seen: the throw is made wherever the ship is. A barge on the Rhine in wind over 17 m/s throws at
  fragility 1. There is no shelter in the rule except being in port.
- `Seamanship.cs` has no part in loss: it only keeps ships clear of each other. A ship in a gale has `go = 0`.

### 1.2 How hers behaves in a gale

- `Game/Chart/ChartBody.cs:103`: `double made = cls != null ? Travel.Speed(cls, ..., WindFrom, Wind, out lying) : ...;`
  and line 107-108: `if (lying) motion.Say("A gale: she lies to.");`. Her speed is 0 until the wind drops under 17.
- Nothing else happens. No throw, no wear, no water. The world's seamanship pass sees her as a `SimShip` with
  `isPlayer` (`WorldSim.cs:94`), but `PlanMove` is never called for her.
- So the critic is right in every word: risk is time.

### 1.3 What state her boat has in code

| Thing | Where | On the chart? |
|---|---|---|
| Men, health, heart, water casks, pay owed, days at sea, seasick | `Game/Voyage/Voyage.cs:94-101` (static, saved) | yes |
| The hold's room (660 kg: 900 less the casks) | `Game/Trade/Hold.cs:24`, set at `KapmanGame.cs:207` | yes |
| A share of the food spoiled | `Voyage.Spoil` (`Voyage.cs:151`), called only by `GameEvents.cs:210` | yes, for dated events |
| Hull, planks, leak, water in her, bailing, capsize, swamping | `Ships/Bilge.cs` (`Kapman.Ships`), with `ShipBody`, `Grounding`, `HullForces` | **no**: the 3D body only. Nothing under `Game/` names `Bilge` (searched) |
| Sail area, reefing | `Ships/Sail.cs`, the 3D body | no |
| Her class's speeds | `ShipClassDef` (`Game/Sim/SimData.cs:8-20`) | yes: `Travel.Speed` |

On the chart she has **no hull state, no water, no sail state, no weariness, no shares**. The Ship panel says so
itself (`UI/Frame/Panels/ShipPanel.cs:50`: "Her wear, her fittings and a yard to mend her at are not in the game
yet."). From the 3D boat this mend borrows three figures so the two agree: a sound hull lets in 2 litres an hour
(`Data/Ships/ships.json:73`, `leak_m3_per_h` 0.002); a man bails 25 litres a minute and 40 in a burst
(`Bilge.cs`: 1,500 and 2,400 litres an hour); the crew bail by themselves as the water rises.

### 1.4 The flags the sea cards set

`Game/Data/encounters/sea_793.json`: 63 cards. They set **30 different flags in 91 effect lines**. No C# file
names any of them (searched `Code`, `Tests`, `Editor` for each name).

| Flag | Effect lines that set it | Cards that set it | Conditions that read it (all decks) |
|---|---|---|---|
| `sea_leak` | 10 | 8 | 2 (sea) |
| `sea_marks_taken` | 9 | 8 | 5 (sea) |
| `sea_sail_weak` | 9 | 7 | 2 (sea) |
| `sea_man_chilled` | 7 | 2 | 1 (sea) |
| `sea_fever` | 6 | 2 | 2 (sea) |
| `sea_short_water` | 5 | 3 | 2 (sea) |
| `sea_after_gale` | 4 | 1 | 1 (sea) |
| `sea_lashed` | 4 | 4 | 1 (sea) |
| `sea_quarrel` | 4 | 2 | 2 (sea) |
| `sea_fog_warned` | 3 | 1 | 2 (sea) |
| `sea_gale_warned` | 3 | 1 | 2 (sea) |
| `sea_bank_marked` | 2 | 2 | none |
| `sea_bar_known` | 2 | 1 | 1 (sea) |
| `sea_gear_mended` | 2 | 2 | none |
| `sea_knows_sun` | 2 | 1 | 1 (sea) |
| `sea_reefed` | 2 | 1 | 3 (sea) |
| `sea_saved_lives` | 2 | 1 | none |
| `sea_spar_aboard` | 2 | 2 | 1 (sea) |
| `sea_vow_altar` | 2 | 1 | none |
| `sea_broke_fast` | 1 | 1 | none |
| `sea_carried_dead` | 1 | 1 | none |
| `sea_cask_short` | 1 | 1 | none |
| `sea_knows_birds` | 1 | 1 | none |
| `sea_knows_lead` | 1 | 1 | none |
| `sea_knows_star` | 1 | 1 | none |
| `sea_saw_raid` | 1 | 1 | none |
| `sea_season_closing` | 1 | 1 | none |
| `sea_told_weather` | 1 | 1 | none |
| `sea_vow_penny` | 1 | 1 | none |
| `sea_watch_kept` | 1 | 1 | none |
| **30 flags** | **91** | | |

### 1.5 The flags the crew cards set

`Game/Data/encounters/crew_793.json`: 112 cards. They set **48 different flags in 252 effect lines**. No C# file
names any of them.

| Flag | Effect lines that set it | Cards that set it | Conditions that read it (all decks) |
|---|---|---|---|
| `crew_song_tonight` | 88 | 43 | 43 (crew) |
| `crew_short_handed` | 24 | 11 | 2 (crew) |
| `crew_pay_promised` | 12 | 4 | 2 (crew) |
| `crew_feud` | 7 | 3 | 2 (crew) |
| `crew_fever_risk` | 7 | 2 | 2 (crew) |
| `crew_pay_just_promised` | 7 | 4 | 1 (crew) |
| `crew_body_aboard` | 6 | 2 | 2 (crew) |
| `crew_dead_share_owed` | 5 | 2 | 1 (crew) |
| `crew_fight_ashore` | 5 | 2 | 2 (crew) |
| `crew_home_promised` | 5 | 2 | 2 (crew) |
| `crew_man_went_home` | 5 | 2 | 3 (crew) |
| `crew_quarrel` | 5 | 3 | 1 (crew) |
| `crew_scattered` | 5 | 2 | 1 (crew) |
| `crew_body_for_church` | 4 | 2 | 2 (crew) |
| `crew_hauled_out` | 4 | 2 | 1 (crew) |
| `crew_heard_hygelac` | 4 | 2 | 2 (crew) |
| `crew_theft_watch` | 4 | 2 | 3 (crew) |
| `crew_thief_known` | 4 | 2 | 3 (crew) |
| `bk_lost` | 3 | 1 | none |
| `crew_friends` | 3 | 2 | 2 (crew) |
| `crew_home_refused` | 3 | 2 | 1 (crew) |
| `crew_song_paid` | 3 | 1 | 1 (crew) |
| `crew_winter_lodged` | 3 | 1 | 1 (crew) |
| `crew_boat_tarred` | 2 | 1 | 1 (crew) |
| `crew_buried_church` | 2 | 1 | 1 (crew) |
| `crew_english_1` | 2 | 1 | 1 (crew) |
| `crew_first_signed` | 2 | 2 | 1 (crew) |
| `crew_heathen_aboard` | 2 | 1 | 31 (crew) |
| `crew_letters_1` | 2 | 1 | 1 (crew) |
| `crew_norse_2` | 2 | 1 | 1 (crew) |
| `crew_on_shares` | 2 | 2 | 2 (crew) |
| `crew_riddle_answered` | 2 | 1 | 1 (crew) |
| `crew_runes_1` | 2 | 1 | 1 (crew) |
| `crew_word_to_dane` | 2 | 1 | 1 (crew) |
| `crew_buried_ashore` | 1 | 1 | 1 (crew) |
| `crew_buried_at_sea` | 1 | 1 | 1 (crew) |
| `crew_each_his_own` | 1 | 1 | none |
| `crew_finn_answered` | 1 | 1 | none |
| `crew_friend_ashore` | 1 | 1 | none |
| `crew_islands_promised` | 1 | 1 | none |
| `crew_knows_tafl` | 1 | 1 | none |
| `crew_last_blades` | 1 | 1 | none |
| `crew_norse_1` | 1 | 1 | 2 (crew) |
| `crew_own_bundles` | 1 | 1 | 1 (crew) |
| `crew_poured` | 1 | 1 | none |
| `crew_song_whole` | 1 | 1 | 1 (crew) |
| `crew_theft_warned` | 1 | 1 | 1 (crew) |
| `hy_fed_poor` | 1 | 1 | 4 (rhine_frisia) |
| **48 flags** | **252** | | |

Over all 39 card files: 6,829 flag lines, 2,861 different flags (the critic's count of 6,829 is confirmed).

**Which of these the boat takes over** (the rest are the deck's own memory and stay so: vows, lessons, quarrels,
songs, burials):

| Flag | Set lines | Today | With this mend |
|---|---|---|---|
| `sea_leak` | 10 (8 cards) | lapses by itself after 12 days; slows nothing, spoils nothing | the hull: a started strake until it is patched or mended |
| `sea_sail_weak` | 9 (7 cards) | lapses after 20 days; "the sail has begun to tear" slows nobody | the cloth: weak until sewn; not driven; may split in a breeze |
| `sea_reefed` | 2 (1 card) | read by one card only | the sail shortened: seven tenths of her speed |
| `sea_lashed` | 4 (4 cards) | read by one card only | lashed down: the sea's chance times 0.8 |
| `sea_spar_aboard` | 2 (2 cards) | read by one card | a spare spar aboard |
| `sea_after_gale` | 4 (1 card) | set by the gale card | read from the real gale: 48 hours after the last hour of one |
| `crew_on_shares` | 2 (2 cards) | "a tenth of each season's gain" pays nothing, changes no wage | half the wage, a tenth of the gain at Martinmas |
| `crew_own_bundles` | 1 | nothing | 20 kg a man out of the hold's room |
| `crew_boat_tarred` | 2 (1 card) | stops the card coming again | the hull sound, and tarred for 300 days (untarred she wears twice as fast) |

### 1.6 The gale the cards speak of is not the gale that sinks ships

- The swell card `sea.swell.long` comes when `storm > 0.1 and storm < 0.5`; the gale card `sea.gale.breaks` when
  `storm >= 0.5 and wind >= 13`. `storm` is the cover of thunder cloud (`WeatherSample.storm`,
  `Core/Weather/WeatherModel.cs:44`), not wind.
- The world's ships are lost in **wind over 17 m/s**, whatever the cloud.
- So the card can say "It is here" in 13 m/s under a thunder cloud, where no ship is in danger; and a real gale of
  22 m/s under a plain grey sky has no card at all. The warning warns of one thing and the dice are thrown for
  another. Section 7 gives the two `when` lines that mend it.

How often it blows over 17 m/s, from the game's own `WeatherModel(793)` sampled every hour for 793 to 805 (run
outside Unity; months are twelfths of the year):

| Place | Gale hours a year, by month (Jan to Dec) | Gales a year | Length, hours: half / nine in ten / longest | Hours already over 13 m/s before it passed 17: half / one in ten / least |
|---|---|---|---|---|
| Mid sea, Maas mouth to Thames (51.8 N 2.6 E) | 17.5, 15.4, 4.2, 4.6, 2.1, 1.8, 0.5, 0.7, 3.3, 7.7, 23.5, 24.5 (106 in all) | 19 | 3 / 14 / 40 | 9 / 3 / 0 |
| Off Frisia, the Vlie (53.4 N 5.0 E) | 20.4, 21.6, 8.0, 6.2, 2.8, 0.9, 2.1, 1.5, 8.4, 13.5, 32.3, 29.0 (147) | 28 | 3 / 12 / 40 | 8 / 2 / 0 |
| Off Ribe (55.3 N 8.0 E) | 24.2, 18.2, 12.2, 6.1, 1.7, 0.0, 1.1, 1.8, 12.2, 14.8, 30.7, 23.4 (146) | 26 | 3 / 13 / 57 | 8 / 3 / 1 |
| Dover strait (51.0 N 1.5 E) | 18.0, 18.9, 6.8, 4.2, 3.9, 2.3, 0.2, 1.0, 3.7, 7.6, 22.2, 21.8 (111) | 24 | 3 / 10 / 31 | 6 / 2 / 0 |

What follows from it:
- In the sailing months (May to September) a place in the southern North Sea has about **8 gale hours a year**, in
  about two gales. From November to February it has about **80**.
- Half of all gales last 3 hours or less; nine in ten last 14 or less; the longest in thirteen years was 40 to 57.
- In half of them the wind was already over 13 m/s for 6 to 9 hours first, but in one in ten for only 2 or 3, and
  sometimes not at all. So "the wind is rising" is not a warning that can be relied on. The model itself must be
  asked (section 4.2).
- At the table's chance (0.0004 an hour, times 0.6) a sound boat lying to through a whole 12-hour gale is struck
  about once in 275 gales. So **the dice alone are a small risk in summer**. The risk a player feels comes from
  three other things this mend makes real: sailing late in the year, the state she lets her boat get into, and
  what she orders when it blows (section 5).

### 1.7 The option the cards take by themselves is the most dangerous one

`Bake/content/out/card_rules_793.json` (rulings 10 and 12: a pressing card takes an option by itself when its time
runs out):

| Card | Pressing, hours | Takes by itself | Its own reason |
|---|---|---|---|
| `sea.swell.long` (line 36613) | 6 | index 2, "Hold on under full sail while the wind serves (doubtful)" | "Freeze, and she holds on under full sail into the weather." |
| `sea.gale.breaks` (line 36640) | 1 | index 2, "Carry full sail and run for it (doubtful)" | "Freeze, and the gale finds her with the sail up." |
| `sea.leak.gains` (line 37637) | 3 | index 2 (of: beach her; heel and patch; all hands bail and run for port) | not read |
| `sea.sail.splits` (line 37691) | 1 | index 2 (of: get it down and sew; the spare spar; run on under what is left) | not read |

While a gale cost only time this was a fair sting. Once she can be lost it is not: a player who looks away at
speed 4 would have all sail carried into a gale in her name, at eight times the chance. See 12.1.

### 1.8 Smaller things found on the way

- `crew.<skill>_best` reads 3 for every skill (`Game/Encounters/EncounterGameVars.cs:109`), so every "(doubtful)"
  chance by seamanship is the same for every crew. Not mended here.
- `days_since_paid` and `venture_ended` are names a card may use (`Core/Encounters/Cond.cs:44`) that nothing
  answers: they read 0. `crew.pay.reckoning` waits on them. Not mended here.
- The sea cards speak of time that nothing spends: "Lower the yard and stitch it now", "Beach her on the first
  sand", "Bring her head to the wind and restow", "Wait a tide" (`crew.council.wind`, the critic's "neither goes nor
  waits"). The new effect `boat lie_to <hours>` spends it (section 7).
- `GameEffects.Encounter` (`Game/Encounters/GameEffects.cs:216`) is still "to come".

---

## 2. The state her boat truly has

One static class, `Kapman.Game.Boat`, in `Game/Voyage/Boat.cs` (the whole file is in 8.2). Plain data, like
`Voyage`: begun with a new game, saved under `"boat"`, restored on load.

| State | Range | Set by cards | Set by weather and time | Read by the simulation |
|---|---|---|---|---|
| **Hull** (`Boat.Hull`) | 1 sound to 0. Words: sound (0.9 up), working (0.7 up), a strake started (0.5 up), opening (0.3 up), going | `flag sea_leak` (to 0.55); `flag sea_leak = 0` (a patch: up to 0.75); `flag crew_boat_tarred` (sound, tarred); `boat hull -0.2`; a yard in port | wears 0.0005 a day (twice that untarred); in a gale 0.004 an hour lying to, 0.008 running shortened, 0.02 carrying sail; a strike of the sea takes 0.3 | the leak (water in); the sea's chance (2 under 0.7, 5 under 0.5); lost at 0 |
| **Water in her** (`Boat.Water`) | litres, 0 to what swamps her (180 l for each m2 of length by beam: 3,049 l for the Gislinge boat). Words: dry, wet underfoot, a hand deep, over the floor timbers, to the thwarts, awash | `boat water +200` | in by the laps (2 l/h sound; up to 6,000 l/h open; double when she works in a gale) and over the gunwale (60 l/h in a fresh breeze; 300, 1,200 or 4,000 in a gale by her order); out by the men (1,500 l/h a man; 2,400 all hands; less as they tire) | her speed (up to three tenths off); the hold's room (a litre is a kilo); one lot spoiled a day over 400 l; the sea's chance (2 when half full); **lost when she fills** |
| **The sail's cloth** (`Boat.Cloth`) | whole, weak, split | `flag sea_sail_weak` (weak); `= 0` (sewn); `boat sail split`, `boat sail whole` | a weak sail splits with a chance of 0.12 an hour in 9 m/s or more; carrying sail in a gale tears it with 0.15 an hour; the second strike of the sea splits it | her speed: weak, not driven past 0.8 of her class's speed; split, half |
| **How it is set** (`Boat.Set`) | all of it, shortened, down | `flag sea_reefed for 2` (shortened for two days); the Ship panel | - | shortened: 0.7 of her speed; down: the oars |
| **Order for a gale** (`Boat.Order`) | lie to, run under shortened sail, carry sail | the gale card (`boat order ...`); the Ship panel | - | whether she moves in a gale; the water she ships; the hull's wear; the sea's chance (1, 2.5, 8) |
| **Weariness** (`Boat.Weary`) | 0 to 1. Words: fresh, tired, weary, spent | `boat weary +0.15` | each hour under way 0.004 by day, 0.008 by night; 0.03 more at the oars (a calm, or holding her head to a gale); up to 0.02 more by the share of the men bailing (one man in four: 0.005), double with all hands; rested 0.04 an hour made fast, 0.03 lying up | her speed under oars (spent men row at half); what they bail; the sea's chance (times 1 + weariness); over 0.8, heart falls 0.005 an hour |
| **Shares** (`Boat.OnShares`) | on, off | `flag crew_on_shares` | Martinmas (11 November) | the daily wage (half); a tenth of the season's gain owed at Martinmas; heart +0.1 with a gain, -0.1 without |
| **The men's bundles** (`Boat.Bundles`) | on, off | `flag crew_own_bundles` | - | 20 kg a man out of the hold's room |
| Lashed, a spare spar, tarred | | `flag sea_lashed`, `flag sea_spar_aboard`, `flag crew_boat_tarred` | lashed lapses with the card's days; tar lasts 300 days | lashed: the sea's chance times 0.8; untarred: twice the wear |

Every number is a field of `BoatRules` with the value above as its default, and may be put in a table
`Game/Data/boat_rules.json` (8.6) for the user to tune. With no file the game runs on the defaults.

**Weariness and the crossing study.** `Design/research/next/crossing.md` (4.2, build step 2) plans four `CrewMan`
records with a word each and a watch bill. `Boat.Weary` is the one number for the whole crew until then; when the
four men exist it becomes their mean, and nothing that reads it changes. That study also says "Nothing runs faster
than 4x" and "2x is the speed a crossing is tuned for": both were written for the old clock and are overruled by
ruling 26. This mend assumes neither.

---

## 3. The hour

`Boat.Hour(BoatHour h)` is called once for every game hour by `BoatWatch.Hour`, from the game's tick ladder
(`KapmanGame.cs`, `Ticks.Hour`), after the world's ships have had the same hour. In order:

1. **The signs.** The hours to the next gale at her place are read (4.2). A new or nearer sign is raised.
2. **The stance.** In wind over 17 m/s and not made fast, she is in a gale. By her order she lies to, or (with the
   wind 100 degrees or more off her bow, and a sail that is not split) runs.
3. **The hull works** (gale only): the wear of the table above.
4. **The sail's own dice**: a weak sail in a breeze; all sail carried in a gale.
5. **Water.** In: the laps and the seas. Out: the fewest men that keep it down this hour (none under 100 l; one;
   two; then all hands at the burst rate). Made fast, she is bailed dry.
6. **The men** tire or rest.
7. **The sea's dice**, only in a gale she has had a sign of at least one game hour before: `GaleDice.Roll(seed,
   "player", hour) < GaleDice.Chance(0.0004, 0.6, Boat.DiceFactor(...))`. On a hit, `Strike`: a hull still tight
   (0.7 or better) takes a started strake and 300 l; a hull already started loses the sail if it is up; after that
   it is the boat.
8. **Lost** if the water has reached what swamps her, or the hull is at 0.

`BoatWatch.Hour` then sets `Trade.Hold.taken` (the water and the bundles), and `BoatWatch.Day` does the daily
things: the hull's slow wear, one lot spoiled if the water stands over 400 l, and Martinmas.

The hour's count is game hours since the start (`Core/Sim/TickLadder.cs:84-85`: `h = m / 60`), the same scale as
`KapmanGame.Hours`, so the boat's day (`hour / 24`) is the game's day that the cards' effects stamp their flags
with (`GameEffects.Today`).

**Her speed** is `Boat.Speed(cls, course, windFrom, wind, out lying)`, which `ChartBody` calls where it called
`Travel.Speed`. With a sound, dry boat, all sail and fresh men it returns exactly what `Travel.Speed` returns (a
test holds it to that), so nothing changes for a boat with no hurts.

---

## 4. The same dice, warned first

### 4.1 One function for every hull

New file `Game/Sim/GaleDice.cs` (8.1). `WorldSim.cs:710` becomes
`s.rng.Chance(GaleDice.Chance(rules.galeLossPerHour, s.cls.galeFragility))`: the same number as today, drawn from
the same dice of each ship, so **no other ship's fate changes and the world's replay from a seed is untouched**.
Hers is `GaleDice.Chance(rules.galeLossPerHour, cls.galeFragility, Boat.DiceFactor(river, day))`.

`Boat.DiceFactor` is the product of:

| What | Times |
|---|---|
| Order: lie to / run under shortened sail / carry sail | 1 / 2.5 / 8 |
| Hull: 0.7 or better / under 0.7 / under 0.5 | 1 / 2 / 5 |
| Water at half of what swamps her, or more | 2 |
| Weariness | 1 + weariness (so up to 2) |
| Fewer men than her class wants | 1.5 |
| Lashed down | 0.8 |
| On a river | 0.1 |

**Her throw has no memory.** `GaleDice.Roll(seed, "player", hour)` makes a fresh `SimRandom` from the world's
seed, her name and the hour's count, and takes its first number. So:
- the same seed gives the same throw for hour 5,000 at speed 1 and at speed 5;
- a saved game loaded again gets the same throw for the same hour: **loading does not throw the dice again**. Only
  a different order or a mended boat changes what the hour does. That is the honest kind of second chance, and it
  fits "the game can be saved at any time".

**The difference from the other ships, said plainly.** Another ship is lost on her first hit. Hers is wounded on
the first (the critic's own rule: "a started strake, then a split sail, then the boat"), and lost on the first
only if she was already hurt. In return hers pays for her state and her choices, which the others do not have. If
the user wants the others to wear too, that is a later step (12.4).

### 4.2 The signs

The weather model answers for any hour (`WeatherModel.Sample(lat, lon, hours, overSea)`,
`Core/Weather/WeatherModel.cs:432`), and it is the model the world's ships sail by (`WorldSim.weather`). So
`BoatWatch.GaleIn` asks it, each game hour, for each of the next 12 hours at her place, and returns the hours to
the first one over 17 m/s. This is not a forecast with an error: it is what will blow there. (She moves, so it is
asked again every hour.)

| Sign | When | The log says (placeholder words, the user's to change) | The clock | The card |
|---|---|---|---|---|
| 1, the swell | a gale 12 hours off or less | "A long swell from a quarter the wind is not in. Weather is coming, half a day off or less." | drops (to speed 2) | - |
| 2, the sky | 6 hours or less | "The sky is lowering and the gulls are going inland. It will blow hard before the watch is out." | drops (to speed 1) | `sea.swell.long`: make ready (shorten and stow; turn for a lee; hold on) |
| 3, white water | 1 hour or less | "White water to windward. It is on her within the hour." | drops (to speed 1) | `sea.gale.breaks`: the order (lie to; run under shortened sail; carry sail) |

- A gale that is first seen nearer than 12 hours (she cast off into it, or it sprang up) gets the sign for its
  distance at once.
- **In port** the same reading is shown before she casts off (section 6: "Weather: a gale within 9 hours"), so a
  gale is never a surprise at the quay. Made fast, she is out of it: no dice, no seas, no wear.
- The signs are told by the steersman's card where there is one. They need no instrument the men of 793 lacked:
  swell, sky and birds are the weather document's own signs (`Design/weather_and_sea_state.md` 3).

### 4.3 The choices that change the odds

| Choice | How it is made | What it does |
|---|---|---|
| **Shorten sail** | the Ship panel ("Shorten"); the swell card's first option (`flag sea_reefed for 2`) | 0.7 of her speed from now. By itself it changes no odds: it is what "run under shortened sail" needs to be the safer way to keep moving |
| **Lash down** | the swell card's first option (`flag sea_lashed for 3`) | the sea's chance times 0.8 for three days |
| **Run for shelter** | the Ship panel ("Run for the nearest port"); the swell card's second option (`boat shelter`) | her course is laid to the nearest port by water within 150 km (`BoatWatch.RunForShelter`). Made fast before it breaks, she is out of it altogether |
| **Lie to** (the order she starts with) | the Ship panel; the gale card's first option | no way made; 300 l/h shipped; the hull works least; the men are at the oars and tire fastest; the sea's chance as every other ship's |
| **Run under shortened sail** | the Ship panel; the gale card's second option | with the wind abaft the beam she makes 0.7 of her top speed (4.5 knots) through the gale; 1,200 l/h; twice the wear; the chance times 2.5. With the wind forward of that she lies to all the same |
| **Carry sail** | the Ship panel; the gale card's third option | her top speed (6.5 knots); 4,000 l/h, so all hands bail; five times the wear; the sail tears with 0.15 an hour; the chance times 8 |
| **Mend first** | a yard in port (the Ship panel, made fast); tar at the winter haul-out; a patch at sea (the leak cards) | a sound hull has a quarter to a tenth of an opened one's risk, and does not fill |
| **Not sail late** | the calendar | ten times the gale hours from November to February (1.6) |

### 4.4 Fair at the fast speeds

Ruling 26: speed 1 is a game minute a real second; 2 is four times that; 3 a day in about a minute; 4 a day in
about fifteen seconds; 5 a day in about four seconds. At speed 5 a game hour is a sixth of a second, and twelve
hours of warning is two real seconds. So the warning cannot be a line of text alone. Five rules:

1. **Everything of the boat runs on the hour tick.** `KapmanGame.Ticks.Hour` comes once for every game hour, in
   order, however the clock moved (`KapmanGame.cs:520`, `ladder.Advance`, in `Update`). Nothing in `Boat` reads frames or
   real time. So nothing is skipped at speed 5 that would have happened at speed 1.
2. **The clock drops at each sign** (`BoatWatch.OnSign`). Today that is `GameSpeed.Slow()` (back to the slowest).
   TO THE BUILDER OF THE FIVE SPEEDS: give `GameSpeed` a "drop to no faster than level n" and call it here with
   speed 2 for sign 1 and speed 1 for signs 2 and 3; this is ruling 26's "a pressing thing". The settings switch
   is `BoatWatch.DropForWeather`.
3. **The cards hold the clock** when they open (ruling 5, already built). So even with the drop switched off, the
   player meets a card six game hours before the gale and another one hour before.
4. **No dice without a sign.** `Boat.FairlyWarned`: the dice are thrown only if a sign of this gale was raised at
   least one game hour earlier. In a gale with no sign (it cannot happen through play, but a fault or a jump could
   make it) the gale works her hull and fills her at the lying-to rate and cannot strike.
5. **No dice in hours nobody played.** A jump of the clock (`KapmanGame.Jumped`, raised by the tick ladder,
   `Core/Sim/TickLadder.cs:75`: the dev keys) gives grace to the hour after the jump (`Boat.Grace`). A load does
   not jump: `SaveGame.Load` sets the clock and calls `ResyncHours`, so no hours are played through.

And one that is not code: **what is taken when the player does not answer must be the safe thing** (12.1). With
that, a player at speed 5 who never touches the game in a gale gets: the clock dropped, the swell card open and
holding the clock; if she sets it running, six game hours later sail shortened and all lashed by the crew; then
the gale card, and an hour later the sail struck and the boat lying to. That is exactly what every other ship
does, at exactly their odds.

How often it interrupts: about two gales a summer pass any one place, and she is at sea for part of them: about
one such stop a sailing season in summer, more in April and October. At speed 4 a season is about half an hour.

**The same seed at speed 1 and at speed 5** (ruling 26 asks for this test). The boat adds nothing that can differ
except through where she is: at a faster speed her place at each hour differs by what a frame covers, so the wind
read there can differ by a hair. The throw itself cannot. In the builder's test, compare `Json.Write(Boat.Capture())`
at the end with a tolerance on `hull`, `water` and `weary`, and exactly on `cloth`, `lost` and `sign`.

---

## 5. What it costs her: the figures

Run outside Unity with the code of section 8 and the default numbers: a gale of 22 m/s, the wind abaft the beam,
the Gislinge boat, dice at the table's 0.0004 times 0.6. "Lost after" is by filling (no dice in these runs). "A
strike" is the chance of at least one hit of the sea over the whole gale, with her state changing as it goes; the
first hit on a tight hull is a wound.

**A gale of 3 hours (half of all gales are this or shorter)**

| Hull | Order | Men | Lost after | Most water | Hull after | Weary after | A strike |
|---|---|---|---|---|---|---|---|
| sound | lie to | 4 | - | 0 l | 0.99 | 0.13 | 1 in 1,275 |
| sound | run shortened | 4 | - | 0 l | 0.98 | 0.04 | 1 in 540 |
| sound | carry sail | 4 | - | 1,150 l | 0.94 | 0.09 | 1 in 164 |
| a strake started (0.55) | lie to | 4 | - | 21 l | 0.54 | 0.15 | 1 in 629 |
| a strake started | carry sail | 4 | - | 0 l (all hands) | 0.49 | 0.15 | 1 in 52 |
| opening (0.4) | lie to | 4 | - | 0 l (all hands) | 0.39 | 0.25 | 1 in 238 |
| opening | carry sail | 3 | **2 h** | fills | - | - | - |

**A gale of 12 hours (nine in ten are shorter than 14)**

| Hull | Order | Men | Lost after | Most water | Hull after | Weary after | A strike |
|---|---|---|---|---|---|---|---|
| sound | lie to | 4 | - | 0 l | 0.95 | 0.48 | 1 in 274 |
| sound | lie to | 3 | - | 0 l | 0.95 | 0.50 | 1 in 181 |
| sound | run shortened | 4 | - | 0 l | 0.90 | 0.12 | 1 in 130 |
| sound | carry sail | 4 | - | 1,150 l | 0.76 | 0.48 | 1 in 35 |
| working (0.75) | carry sail | 4 | - | 0 l (all hands) | 0.51 | 0.55 | 1 in 18 |
| working | carry sail | 3 | **12 h** | fills | - | - | 1 in 10 |
| a strake started | lie to | 4 | - | 1,171 l | 0.50 | 0.64 | 1 in 130 |
| a strake started | run shortened | 4 | - | 1,011 l | 0.45 | 0.49 | 1 in 31 |
| a strake started | carry sail | 4 | **11 h** | fills | - | - | 1 in 7 |
| opening | lie to | 4 | - | 0 l (all hands) | 0.35 | 0.91 | 1 in 47 |
| opening | lie to | 3 | - | 2,738 l | 0.35 | 0.92 | 1 in 26 |
| opening | carry sail | 4 | **6 h** | fills | - | - | 1 in 11 |

**A gale of 40 hours (the longest in thirteen years at three of four places)**

| Hull | Order | Men | Lost after | Hull after | A strike |
|---|---|---|---|---|---|
| sound | lie to | 4 | - | 0.84 | 1 in 62 |
| sound | run shortened | 4 | - | 0.68 | 1 in 31 |
| sound | carry sail | 4 | - | 0.62 | 1 in 5 |
| a strake started | lie to | 4 | - | 0.39 | 1 in 14 |
| a strake started | lie to | 3 | **29 h** | - | - |
| a strake started | run shortened | 4 | **24 h** | - | - |
| opening | lie to | 4 | **17 h** | - | - |

How to read it: a sound boat that lies to is as safe as any ship in the world, and nearly always comes through
with tired men and a hull a little worked. The player can now buy speed with risk (running makes 4.5 knots where
lying to makes none, at two and a half times the chance; carrying sail 6.5 knots at eight times, with the water
and the sail on top). And a hurt left unmended is what kills: "a strake started" turns a long gale from 1 in 274
into filling in the eleventh hour under sail. The wait for a yard, the 8 d for tar in November, and the choice to
strike the sail are now priced.

---

## 6. What the Ship and Crew panels show

Both files whole are in 8.4 and 8.5; they replace `UI/Frame/Panels/ShipPanel.cs` and `CrewPanel.cs`.

### 6.1 Ship, the full window

The lines it has today, then a heading "As she is":

| Line | Small words under it | Right |
|---|---|---|
| Hull | How tight she is. A started strake leaks; a patch holds it, a yard or a haul-out with tar mends it. | `sound` |
| Water in her | "One man is bailing." / "Two men are bailing." / "All hands are bailing." / (else) It weighs on her, slows her, and spoils what stands in it. | `dry` and `0 l` |
| Sail | The cloth, and how it is set. Shortened, she makes seven tenths of her speed. | `whole` `all of it` |
| Order for a gale | What the men do when it blows a gale. | `lie to` |
| A night of gale, as she is | The sea's chance of striking her in twelve hours of it. The first strike starts a strake, the next takes the sail, the third the boat. | `she would ride it` and `1 in 350` |
| Weather (only with a sign, or in a gale) | The men read the swell and the sky. | `a gale within 9 hours` |

"Carries" now shows the room left after the water in her and the bundles.

Then the orders. At sea: **Set the sail** (All of it, Shorten, Down); **In a gale** (Lie to, Run shortened, Carry
sail: each button's hint says what it costs, in the words of 4.3); **Shelter** (Run for the nearest port). Made
fast: **Her seams** ("Have them done (17 d)") and **Her sail**
("Have it done (3 d)" sewn, 12 d a new cloth), paid from the purse in the port's money.

The yard's price is `8 + 20 x (1 - hull)` deniers, rounded: 9 d for a worked hull at 0.95, 17 d for a started
strake, 20 d for an opened one. The 8 d is the winter card's own price for tar, moss and the strand's men
(`crew.winter.haul_out`). These are guesses: the money study (`Design/research/next/money.md`) should own them.

**The words of the fourth line** come from the chance of a strike in twelve hours: under 1 in 200 "she would ride
it"; under 1 in 50 "she would be pressed"; under 1 in 12 "she would be in danger"; else "she would not live long
in it". The number beside it is the critic's "a loss I can price". If the user would rather the game never showed
a number (the cards show odds as words only), delete the `FrameUi.Num(OneIn(...))` on that line: see 12.3.

### 6.2 Crew, the full window

After Health and Heart: **Weariness** (`fresh`; small words: "Oars, bailing and nights under way tire them; a
mooring rests them. Weary men row slower, and the sea finds a weary boat sooner."); **At the bailing** (only when
men are: `one man`, `two men`, `all hands`; "Men bailing are not at the oars."); **Paid by** (`the wage`: "A wage
each day aboard, paid out in port." or `shares`: "Half the wage each day, and a tenth of the season's gain at
Martinmas."); **Their own bundles** (only when they have them: `80 kg`). Then Pay owed and the rest as today.

### 6.3 The brief faces (ruling 9)

`Design/fixes/brief_faces.md` gives each panel `protected virtual void Brief(List<string> lines)`. It is not in
the code yet, so this is NOT COMPILED; add it to the two panels when that step is in. The first line is the one
that matters now.

```csharp
        // ShipPanel
        protected override void Brief(List<string> lines)
        {
            if (Boat.IsLost) { lines.Add("She is lost."); return; }
            if (Boat.InGale) lines.Add("A gale: " + (ChartBody.Current != null && ChartBody.Current.Sheltering ? "she lies to" : "she runs"));
            else if (Boat.Sign > 0) lines.Add($"A gale within {Boat.GaleIn:0} hours");
            if (Boat.Water >= Boat.R.oneManFrom) lines.Add("Water: " + Boat.WaterWord);
            if (Boat.Hull < 0.9) lines.Add("Hull: " + Boat.HullWord);
            if (Boat.Cloth != SailCloth.Whole || Boat.EffectiveSet != SailSet.Full) lines.Add("Sail: " + Boat.ClothWord + ", " + Boat.SetWord);
            if (lines.Count == 0) lines.Add("Sound and dry, all sail");
        }

        // CrewPanel
        protected override void Brief(List<string> lines)
        {
            lines.Add((Voyage.Men == 1 ? "One man" : Voyage.Men + " men") + ", " + Boat.WearyWord);
            if (Boat.Bailers > 0) lines.Add(Boat.AllHands ? "All hands bailing" : Boat.Bailers + " bailing");
            lines.Add("Heart: " + FrameGame.HeartWord(Voyage.Morale));
            if (Voyage.PayOwed >= 1.0) lines.Add($"Owed {Voyage.PayOwed:0} d" + (Boat.OnShares ? " (shares)" : ""));
        }
```

### 6.4 The last page

`BoatWatch.GameOver` is raised with how she was lost. The frame should show one float over the chart, holding the
clock: the title "She is lost", the sentence (`Boat.LostHow`), the last ten lines of the log (they already tell the
signs, the order and the wounds in order), and two buttons: "Load the last saved game" (`SaveGame.Load` on the
newest slot, as the title's Continue does) and "To the title". Not drafted: it is one `FloatSpec` in
`FrameRoot.Floats.cs`, and that file is being worked on by the builders today. `SaveGame.Busy()` refuses a save
while she is lost (9), so no autosave can write over the saves from before.

---

## 7. The cards: nothing must change, and what should

### 7.1 Without touching a card

With the code of sections 8 and 9 and no card file changed:

- Every card that sets `sea_leak` starts a strake; every card that clears it patches her. The six groundings
  (`sea.fog.shut`, `sea.surf.heard`, `sea.bar.ebb`, `sea.light.ashore`, `sea.ice.floes`, `sea.ice.river`) now cost
  a real leak. The leak does not go away in 12 days: `sea.leak.gains` (`when: under_way and flag.sea_leak`) keeps
  coming until she is patched.
- `sea_sail_weak`, `sea_reefed`, `sea_lashed`, `sea_spar_aboard`, `crew_on_shares`, `crew_own_bundles` and
  `crew_boat_tarred` do what section 2 says.
- The signs come in the log and drop the clock. The two gale cards are offered at signs 2 and 3, **but each opens
  only if its present `when` also holds** (`EncounterDirector.Offer`, line 469, checks it through `CanOffer`), which is by thunder
  cloud (1.6). Until 7.2 is done the player gives the order from the Ship panel, and she starts with "lie to".
- A test or tool that sets one of these nine flags and reads it back still sees 1 (through the boat), with one
  difference: `sea_leak` and `sea_sail_weak` no longer lapse with their days.

### 7.2 The lines to change, for whoever holds the card files after the content pass

By card id and option, so they can be applied after the six agents have finished. `sea_793.json` and
`crew_793.json` are under `Assets` and were not touched.

| Card | Where | Now | Change to |
|---|---|---|---|
| `sea.swell.long` | `when` | `under_way and level_kind = ocean and storm > 0.1 and storm < 0.5 and not flag.sea_gale_warned` | `under_way and level_kind = ocean and gale_in <= 12 and gale = 0 and not flag.sea_gale_warned` |
| `sea.swell.long` | option 2, "Turn for the nearest lee and get behind it" | `["flag sea_gale_warned for 2", "flag sea_reefed for 2", "morale -0.02"]` | add `"boat shelter"` |
| `sea.gale.breaks` | `when` | `under_way and level_kind = ocean and storm >= 0.5 and wind >= 13 and flag.sea_gale_warned` | `under_way and level_kind = ocean and gale_in <= 1` |
| `sea.gale.breaks` | option 1, "Strike the sail and hold her head to it under oars" | `["morale -0.03", "flag sea_after_gale for 2"]` | add `"boat order lie_to"` |
| `sea.gale.breaks` | option 2, "Run before it under the shortened sail, as you made ready" | `["morale +0.03", "flag sea_after_gale for 2"]` | add `"boat order reefed"` |
| `sea.gale.breaks` | option 3, "Carry full sail and run for it" | a chance of 0.4 whose failure sets `sea_sail_weak` and takes two lots | put `"boat order carry"` before the chance. The chance may go: the simulation now does the tearing, the water and the risk hour by hour, and a second roll on top charges twice |
| `sea.leak.gains` | `when` | `under_way and flag.sea_leak` | `under_way and boat.water >= 400` (its words are "with two at it the water still creeps up": let it come when that is true) |
| `sea.leak.gains` | option 1, "Beach her on the first sand and get at it from outside" | `["flag sea_leak = 0", "morale -0.02"]` | add `"boat lie_to 8"` |
| `sea.leak.gains` | option 2, "Heel her at sea and patch it from over the side" | a chance | put `"boat lie_to 3"` before the chance |
| `sea.leak.gains` | option 3, "All hands bail by turns, and run for port" | a chance | put `"boat weary +0.15"` before the chance |
| `sea.leak.seam` | options 1 (failure) and 3 | `"flag sea_leak for 12"` | `"boat hull -0.2"` (a weeping seam is "working", not yet a started strake; a second one makes it one) |
| `sea.sail.splits` | `when` | `under_way and flag.sea_sail_weak and wind >= 9` | `under_way and boat.sail = split` (the simulation splits it, with the card's own 0.12 an hour; the card tells it and offers the mend) |
| `sea.sail.splits` | option 1, "Get it down, hold her head to the sea, and sew" | `["morale -0.02", "flag sea_sail_weak = 0"]` | `["morale -0.02", "boat lie_to 6", "boat sail whole"]` |
| `sea.sail.splits` | option 2, "Set the sound half on the spar you picked up" | `["flag sea_sail_weak = 0", "flag sea_spar_aboard = 0", "morale +0.02"]` | `["boat sail weak", "flag sea_spar_aboard = 0", "morale +0.02"]` |
| `sea.sail.splits` | option 3, "Run on under what is left of it" | a chance, both ends clearing `sea_sail_weak` | drop the two `flag sea_sail_weak = 0`: she stays split, at half speed, until it is sewn |
| `sea.sail.seam` | option 1, "Lower the yard and stitch it now" | `["morale -0.01"]` | add `"boat lie_to 2"` |
| `sea.cargo.shifts` | option 1, "Bring her head to the wind and restow" | `["morale -0.01", "flag sea_lashed for 3"]` | add `"boat lie_to 1"` |
| `crew.council.wind` | option 2, "Wait a tide" | `["npc steersman feeling +3 heard", "npc lookout feeling -1 overruled", "morale -0.01"]` | add `"boat lie_to 6"` (so waiting waits: she does not move for six hours) |
| `crew.pay.martinmas` | options 1 and 2 (40 d, 24 d) | fixed sums | not changed here, but to be looked at by the money pass: with shares on, the tenth is now reckoned from the real gain and added to "Pay owed"; a fixed 24 d on top charges twice |

New names a `when` may use (added to `Cond.cs` in section 9): `gale` (1 in wind over 17 m/s at her place), `gale_in`
(hours to one; 99 when none is in sight within twelve), `boat.hull`, `boat.water` (litres), `boat.weary`,
`boat.sail` (`whole`, `weak`, `split`).

The new effect, one verb: `boat hull <+-f>` | `boat water <+-litres>` | `boat weary <+-f>` | `boat lie_to <hours>` |
`boat sail whole|weak|split` | `boat set full|reefed|down` | `boat order lie_to|reefed|carry` | `boat shares on|off` |
`boat tar` | `boat shelter`. The loader refuses any other word after `boat` (a test holds it).

### 7.3 Two rows for the card rules

`Bake/content/card_rules_overrides.json` (not touched: `Bake/content` is not mine to write). If 12.1 is ruled as
proposed, the default of four cards turns to index 0:

| Card | Default now | Proposed |
|---|---|---|
| `sea.swell.long` | 2, hold on under full sail | 0, "Shorten sail now and stow for weather" |
| `sea.gale.breaks` | 2, carry full sail | 0, "Strike the sail and hold her head to it under oars" |
| `sea.leak.gains` | 2, all hands bail and run for port | 0, "Beach her on the first sand" |
| `sea.sail.splits` | 2, run on under what is left | 0, "Get it down, hold her head to the sea, and sew" |

---

## 8. Code: new files

All five were compiled as they stand here (they are copied into this page by script from the files that were
compiled and tested).

### 8.1 `Assets/Kapman/Code/Game/Sim/GaleDice.cs` (new)

```csharp
using System;

namespace Kapman.Game.Sim
{
    /// <summary>
    /// THE GALE'S DICE, the same for every hull (the strategy critic, 7 Oct 2026: "the same gale dice for her as for the
    /// other ships"). A gale is wind over <see cref="GaleWind"/> where the ship is (the line Travel.Speed draws). Each
    /// game hour a ship is under way in one, the sea strikes her with the chance <see cref="Chance"/>: the table's chance
    /// an hour (SimRules.galeLossPerHour), times her class's fragility, times the state she is in (1 for a sound boat
    /// lying to, which is what the world's ships are; hers is Boat.DiceFactor).
    /// </summary>
    public static class GaleDice
    {
        /// <summary>Metres a second: over this a ship lies to (Travel.Speed) and the dice are thrown.</summary>
        public const double GaleWind = 17.0;

        public static double Chance(double perHour, double fragility, double state = 1.0) =>
            Math.Max(0.0, Math.Min(1.0, perHour * fragility * state));

        /// <summary>The throw for one hull in one game hour, 0 to 1. It has no memory: the seed, the hull's name and the
        /// hour's count give it, so it is the same at every speed of the clock, and the same after a saved game is loaded
        /// (loading again does not throw again: only a different order changes what the hour does).</summary>
        public static double Roll(int seed, string who, long hour) => SimRandom.For(seed, who + "@" + hour).Next();
    }
}
```

### 8.2 `Assets/Kapman/Code/Game/Voyage/Boat.cs` (new)

```csharp
using System;
using System.Collections.Generic;
using Kapman.Game.Sim;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>The cloth: whole; weak (a seam has let go: she is not driven, and it may split in a breeze); split (half a
    /// sail on a jury rig until it is sewn or bought).</summary>
    public enum SailCloth { Whole, Weak, Split }
    /// <summary>How it is set: all of it, shortened, or down (she goes by the oars).</summary>
    public enum SailSet { Full, Reefed, Down }
    /// <summary>The standing order for a gale: strike the sail and hold her head to it under oars (what the world's ships
    /// do); run on under the shortened sail when the wind is abaft the beam; or carry all of it.</summary>
    public enum GaleOrder { LieTo, RunReefed, CarrySail }

    /// <summary>The boat's table (Data/boat_rules.json; every number a STARTING GUESS TO TUNE).</summary>
    public sealed class BoatRules
    {
        // her speed
        public double reefedSpeed = 0.7, weakSailCap = 0.8, splitSpeed = 0.5, wearyOars = 0.5, waterSlow = 0.3, runAbaftDeg = 100.0;
        // the hull: wear, and where a wound or a mend leaves her
        public double wearPerDay = 0.0005, untarredWear = 2.0, galeWearPerHour = 0.004, reefedWear = 2.0, carryWear = 5.0;
        public double strakeHull = 0.55, patchHull = 0.75, strikeHull = 0.3, tarLastsDays = 300.0;
        // water: litres an hour in, and what the men lift out
        public double soundLeak = 2.0, openLeak = 6000.0, workFromWind = 8.0;
        public double freshSeas = 60.0, seasLieTo = 300.0, seasReefed = 1200.0, seasCarry = 4000.0, freshWind = 13.0;
        public double manBails = 1500.0, burstBails = 2400.0, oneManFrom = 100.0, twoMenFrom = 400.0, allHandsFrom = 1200.0;
        public double swampPerM2 = 180.0, spoilFrom = 400.0;
        // weariness, a share an hour
        public double wearySailing = 0.004, wearyNight = 0.008, wearyOarsHour = 0.03, wearyBailing = 0.02, restMoored = 0.04, restLyingUp = 0.03;
        public double wearyHeartFrom = 0.8, wearyHeartPerHour = 0.005;
        // the dice: what multiplies the chance every hull has
        public double diceReefed = 2.5, diceCarry = 8.0, diceStrained = 2.0, diceOpened = 5.0, diceHeavy = 2.0, diceShortHanded = 1.5,
                      diceLashed = 0.8, diceRiver = 0.1;
        public double strainedBelow = 0.7, openedBelow = 0.5;
        // the sail's own dice, a chance an hour
        public double weakSplits = 0.12, weakSplitsWind = 9.0, carryTears = 0.15;
        // the signs: hours ahead the weather is read, and when each sign comes
        public double lookAhead = 12.0, secondSign = 6.0, lastSign = 1.0, fairAfterHours = 1.0;
        // shares and bundles
        public double shareOfGain = 0.1, wageOnShares = 0.5, bundleKgPerMan = 20.0, noGainHeart = -0.1, gainHeart = 0.1;
        public double martinmasMonth = 11.0, martinmasDay = 11.0;
        // a yard's prices, deniers
        public double hullMendBase = 8.0, hullMendFull = 20.0, sailSewn = 3.0, sailNew = 12.0;

        /// <summary>The table, by the fields' own names ("reefedSpeed": 0.7); a name not in the file keeps its number here.</summary>
        public static BoatRules Load(TextAsset json)
        {
            var r = new BoatRules();
            if (json == null)
                return r;
            var o = Json.Parse(json.text);
            foreach (var f in typeof(BoatRules).GetFields())
                if (f.FieldType == typeof(double))
                    f.SetValue(r, Json.Num(o, f.Name, (double)f.GetValue(r)));
            return r;
        }
    }

    /// <summary>What one game hour is like where she is: handed to <see cref="Boat.Hour"/> by the live game (BoatWatch) or
    /// by a test.</summary>
    public struct BoatHour
    {
        /// <summary>The hour's count since the start (the tick ladder's).</summary>
        public long hour;
        /// <summary>The wind where she is, m/s, and how far off it her course lies (0 into it, 180 before it).</summary>
        public double wind, offWind;
        public bool underWay, moored, river, night;
        /// <summary>Hours to the first hour of wind over the gale line at her place, within the look-ahead (0: it blows
        /// now; infinity: none in sight).</summary>
        public double galeIn;
        /// <summary>Men aboard, and the hands her class wants.</summary>
        public int men, hands;
        public double length, beam, fragility, lossPerHour;
        public int seed;
    }

    /// <summary>
    /// THE BOAT SHE TRULY IS, on the chart (the strategy critic, 7 Oct 2026: "the deck sails a second boat that the
    /// simulation never meets"; his third change: "the same gale dice for her as for the other ships (warned first), and
    /// sail, leak and shares kept in the simulation instead of in card flags"). One state, written by the cards and by the
    /// weather, read by the simulation:
    ///   her hull's soundness (1 sound, 0 open), the water in her (litres), the sail's cloth and how it is set, the
    ///   order for a gale, the men's weariness (0 to 1), whether they sail on shares.
    /// Each game hour (<see cref="Hour"/>): the signs of a gale within the look-ahead; the hull works; water comes in by
    /// the laps and over the gunwale and the men bail it; the men tire or rest; a weak sail may split; and in a gale she
    /// has been warned of, the sea's dice are thrown for her as for every hull (GaleDice), times the state she is in.
    /// A strike wounds before it kills: a started strake first, then the sail, then the boat. She is also lost if she
    /// fills. Lost is the end of the game (the user, 2 October: a wreck is game over; the game can be saved at any time).
    /// Her speed (<see cref="Speed"/>) is the world's ships' own (Travel.Speed) through her sail, her water and her men.
    /// Plain data: saved and restored whole; no dice of its own to save (GaleDice.Roll has no memory).
    /// </summary>
    public static class Boat
    {
        public static BoatRules R { get; private set; } = new BoatRules();

        public static double Hull { get; private set; } = 1.0;
        public static double Water { get; private set; }
        public static SailCloth Cloth { get; private set; }
        public static SailSet Set { get; private set; }
        public static GaleOrder Order { get; private set; }
        public static double Weary { get; private set; }
        public static bool OnShares { get; private set; }
        public static bool Bundles { get; private set; }
        public static bool SpareSpar { get; private set; }

        /// <summary>The sign of the gale coming: 0 none, 1 the swell, 2 the sky, 3 within the hour.</summary>
        public static int Sign { get; private set; }
        /// <summary>Hours to the gale at her place when last read (infinity: none in sight; 0: it blows).</summary>
        public static double GaleIn { get; private set; } = double.PositiveInfinity;
        public static bool InGale { get; private set; }
        /// <summary>The men bailing this hour, and whether all hands are at it.</summary>
        public static int Bailers { get; private set; }
        public static bool AllHands { get; private set; }
        public static bool IsLost { get; private set; }
        /// <summary>How she was lost, as a plain sentence ("" while she swims).</summary>
        public static string LostHow { get; private set; } = "";
        /// <summary>What she holds before she is swamped, litres (from her class's length and beam).</summary>
        public static double Swamp { get; private set; } = 3000.0;
        /// <summary>The chance the sea had of striking her in the last gale hour (0 outside one): for the Ship panel.</summary>
        public static double LastChance { get; private set; }

        static long now, signAt = long.MinValue, lastGaleHour = long.MinValue, lieToUntil = long.MinValue;
        static long reefUntil = long.MaxValue, graceUntil = long.MinValue;
        static double lashedUntilDay = double.NegativeInfinity, tarredDay = double.NegativeInfinity;
        static double seasonBase = double.NaN;
        static int paidYear = int.MinValue;
        static int men = 4, hands = 4;

        /// <summary>A sign of weather (1, 2, 3), with the hours to the gale: the live game drops the clock and opens the card.</summary>
        public static event Action<int, double> SignCame;
        /// <summary>A plain fact about the boat ("The sail splits."): for the log.</summary>
        public static event Action<string> Said;
        /// <summary>She is lost (how): the end of the game.</summary>
        public static event Action<string> Lost;

        public static void Begin(BoatRules rules)
        {
            R = rules ?? new BoatRules();
            Hull = 1.0;
            Water = 0.0;
            Cloth = SailCloth.Whole;
            Set = SailSet.Full;
            Order = GaleOrder.LieTo;
            Weary = 0.0;
            OnShares = Bundles = SpareSpar = false;
            Sign = 0;
            GaleIn = double.PositiveInfinity;
            InGale = AllHands = IsLost = false;
            Bailers = 0;
            LostHow = "";
            LastChance = 0.0;
            now = 0;
            signAt = lastGaleHour = lieToUntil = long.MinValue;
            reefUntil = long.MaxValue;
            graceUntil = long.MinValue;
            lashedUntilDay = double.NegativeInfinity;
            tarredDay = -120.0;                                            // (a new game's boat was tarred in the winter before it)
            seasonBase = double.NaN;
            paidYear = int.MinValue;
            men = hands = 4;
        }

        // ------------------------------------------------------------------ the orders (the panel, the cards)

        /// <summary>The sail set (shortened for some hours, when a card says so; else until it is changed).</summary>
        public static void SetSail(SailSet set, double forHours = 0.0)
        {
            Set = set;
            reefUntil = set == SailSet.Reefed && forHours > 0.0 ? now + (long)Math.Ceiling(forHours) : long.MaxValue;
        }

        public static void SetOrder(GaleOrder order) => Order = order;

        /// <summary>She lies to for some hours (a card: the sail sewn, the cargo restowed, a patch from over the side).</summary>
        public static void LieTo(double hours) => lieToUntil = Math.Max(lieToUntil, now + (long)Math.Ceiling(Math.Max(0.0, hours)));

        public static bool LyingTo => lieToUntil > now;
        public static bool Lashed(double day) => day < lashedUntilDay;
        public static bool Tarred(double day) => day - tarredDay < R.tarLastsDays;
        /// <summary>Hours since the last hour of a gale she was out in (infinity: none yet).</summary>
        public static double HoursSinceGale => lastGaleHour == long.MinValue ? double.PositiveInfinity : now - lastGaleHour;
        /// <summary>She has had a sign of this gale long enough ago to have acted on it: only then are the dice thrown.</summary>
        public static bool FairlyWarned => signAt != long.MinValue && now - signAt >= R.fairAfterHours && now >= graceUntil;

        /// <summary>No dice before this hour (the clock was jumped: hours nobody could answer are not held against her).</summary>
        public static void Grace(long untilHour) => graceUntil = Math.Max(graceUntil, untilHour);

        // ------------------------------------------------------------------ wounds and mends (the cards, a yard)

        /// <summary>The hull changed by a share, either way (a grounding, a patch), kept within 0 to 1.</summary>
        public static void HullBy(double share) => Hull = Math.Max(0.0, Math.Min(1.0, Hull + share));

        /// <summary>A started strake: the hull no better than "started" (if it was already, a little worse).</summary>
        public static void Strake() => Hull = Hull > R.strakeHull ? R.strakeHull : Math.Max(0.0, Hull - 0.1);

        /// <summary>Patched at sea: no worse than "working". A patch is not a yard's mend.</summary>
        public static void Patch() => Hull = Math.Max(Hull, R.patchHull);

        /// <summary>Hauled out and tarred (the winter's card, a yard): sound, and tarred from this day.</summary>
        public static void Tar(double day)
        {
            Hull = 1.0;
            tarredDay = day;
        }

        public static void WaterBy(double litres) => Water = Math.Max(0.0, Math.Min(Swamp, Water + litres));
        public static void SetCloth(SailCloth cloth) => Cloth = cloth;
        public static void WearyBy(double share) => Weary = Math.Max(0.0, Math.Min(1.0, Weary + share));
        public static void Lash(double day, double days) => lashedUntilDay = days > 0.0 ? day + days : double.NegativeInfinity;
        public static void Spar(bool aboard) => SpareSpar = aboard;
        public static void SetBundles(bool on) => Bundles = on;

        /// <summary>What a yard asks to make her hull sound, and her sail whole (deniers; 0: nothing to do).</summary>
        public static double HullMendCost => Hull >= 0.999 ? 0.0 : Math.Round(R.hullMendBase + R.hullMendFull * (1.0 - Hull));
        public static double SailMendCost => Cloth == SailCloth.Whole ? 0.0 : Cloth == SailCloth.Weak ? R.sailSewn : R.sailNew;

        // ------------------------------------------------------------------ shares

        /// <summary>The men go on shares (or back on the wage). worthNow: the house's worth now (silver and cargo, less the
        /// debt), from which the season's gain is counted.</summary>
        public static void Shares(bool on, double worthNow)
        {
            if (on && !OnShares)
                seasonBase = worthNow;
            OnShares = on;
        }

        /// <summary>The share of the day's wage that is owed (the whole of it on a wage; part of it on shares).</summary>
        public static double WageFactor => OnShares ? R.wageOnShares : 1.0;

        /// <summary>Martinmas (or the first day after it in a year not yet reckoned): what the men are owed of the
        /// season's gain, deniers (0 when not on shares, when it is not the day, or when there was no gain; `due` says
        /// whether this was the reckoning).</summary>
        public static double Martinmas(int year, int month, int day, double worthNow, out bool due)
        {
            due = OnShares && year != paidYear && (month > R.martinmasMonth || (month == (int)R.martinmasMonth && day >= R.martinmasDay));
            if (!due)
                return 0.0;
            paidYear = year;
            double gain = double.IsNaN(seasonBase) ? 0.0 : worthNow - seasonBase;
            seasonBase = worthNow;
            return gain > 0.0 ? Math.Round(gain * R.shareOfGain, 1) : 0.0;
        }

        /// <summary>Room in the hold taken by what is not cargo: the water in her, and the men's own bundles (kg).</summary>
        public static double TakenKg => Water + (Bundles ? R.bundleKgPerMan * men : 0.0);

        // ------------------------------------------------------------------ what the simulation reads

        static double OffWind(double courseDeg, double windFrom) => Math.Abs(((courseDeg - windFrom) % 360.0 + 540.0) % 360.0 - 180.0);

        /// <summary>The share of her speed the water in her leaves (dry: all of it; half swamped or more: 1 - waterSlow).</summary>
        public static double WaterFactor => 1.0 - R.waterSlow * Math.Min(1.0, Water / Math.Max(1.0, Swamp * 0.5));

        /// <summary>The share of her speed under oars her men leave: weary, short-handed, some of them bailing.</summary>
        public static double OarFactor =>
            (1.0 - R.wearyOars * Weary) * Math.Max(0.0, Math.Min(1.0, (men - Bailers) / (double)Math.Max(1, hands)));

        /// <summary>
        /// Her speed made good along her course (m/s) and whether she lies to: Travel.Speed, the world's ships' own
        /// rule, through her state. Shortened sail is <c>reefedSpeed</c> of it; a weak sail is not driven past
        /// <c>weakSailCap</c> of her class's speed; a split one gives <c>splitSpeed</c>; the sail down, or the oars
        /// better, she rows, as well as her men can; water in her slows her. In a gale she lies to, unless her order
        /// is to run and the wind is abaft the beam.
        /// </summary>
        public static double Speed(ShipClassDef c, double courseDeg, double windFrom, double windSpeed, out bool lying)
        {
            lying = false;
            if (c == null || IsLost)
            {
                lying = IsLost;
                return 0.0;
            }
            if (LyingTo)
            {
                lying = true;
                return 0.0;
            }
            double oars = c.oarsKn * Math.Min(1.0, c.rowingHours / 12.0) * SimUnits.KnotMs;
            if (windSpeed > GaleDice.GaleWind)
            {
                bool run = Order != GaleOrder.LieTo && c.hasSail && Cloth != SailCloth.Split && OffWind(courseDeg, windFrom) >= R.runAbaftDeg;
                lying = !run;
                return run ? c.maxKn * SimUnits.KnotMs * (Order == GaleOrder.RunReefed ? R.reefedSpeed : 1.0) * WaterFactor : 0.0;
            }
            double made = Travel.Speed(c, courseDeg, windFrom, windSpeed, out _);
            double sail = made > oars + 1e-9 ? made : 0.0;                  // (Travel takes the better of the two)
            if (EffectiveSet == SailSet.Down)
                sail = 0.0;
            else if (sail > 0.0)
            {
                if (EffectiveSet == SailSet.Reefed)
                    sail *= R.reefedSpeed;
                if (Cloth == SailCloth.Split)
                    sail *= R.splitSpeed;
                else if (Cloth == SailCloth.Weak)
                    sail = Math.Min(sail, R.weakSailCap * c.sailKn * SimUnits.KnotMs);
            }
            return Math.Max(sail, oars * OarFactor) * WaterFactor;
        }

        /// <summary>How the sail is set now (a reef taken for some hours shakes itself out).</summary>
        public static SailSet EffectiveSet => Set == SailSet.Reefed && now >= reefUntil ? SailSet.Full : Set;

        /// <summary>What her state multiplies the sea's chance by, in a gale: 1 for a sound boat lying to with a full
        /// crew (what the world's ships are).</summary>
        public static double DiceFactor(bool river, double day)
        {
            double f = Order == GaleOrder.CarrySail ? R.diceCarry : Order == GaleOrder.RunReefed ? R.diceReefed : 1.0;
            f *= Hull < R.openedBelow ? R.diceOpened : Hull < R.strainedBelow ? R.diceStrained : 1.0;
            if (Water >= Swamp * 0.5)
                f *= R.diceHeavy;
            f *= 1.0 + Weary;
            if (men < hands)
                f *= R.diceShortHanded;
            if (Lashed(day))
                f *= R.diceLashed;
            if (river)
                f *= R.diceRiver;
            return f;
        }

        /// <summary>Litres an hour through the laps: a sound hull's weep, and far more as she opens and as she works in a sea.</summary>
        public static double Leak(double wind)
        {
            double open = 1.0 - Hull, work = Math.Max(0.0, Math.Min(1.0, (wind - R.workFromWind) / Math.Max(1.0, GaleDice.GaleWind - R.workFromWind)));
            return R.soundLeak + open * open * R.openLeak * (1.0 + work);
        }

        // ------------------------------------------------------------------ the hour

        /// <summary>One game hour of the boat. Returns the facts of the hour, in order (also raised through Said).</summary>
        public static List<string> Hour(BoatHour h)
        {
            var facts = new List<string>();
            if (IsLost)
                return facts;
            now = h.hour;
            men = Math.Max(1, h.men);
            hands = Math.Max(1, h.hands);
            Swamp = Math.Max(500.0, R.swampPerM2 * Math.Max(1.0, h.length) * Math.Max(1.0, h.beam));
            if (Set == SailSet.Reefed && now >= reefUntil)
                SetSail(SailSet.Full);
            double day = h.hour / 24.0;
            bool gale = h.wind > GaleDice.GaleWind && !h.moored;

            // the signs: read ahead, and told once each for one gale (a gale that has blown itself out is done with:
            // the next one has its own signs, and its own hour of warning before any dice)
            if (InGale && !gale)
            {
                Sign = 0;
                signAt = long.MinValue;
            }
            GaleIn = h.moored ? double.PositiveInfinity : gale ? 0.0 : h.galeIn;
            if (!double.IsInfinity(GaleIn) && !double.IsNaN(GaleIn) && GaleIn <= R.lookAhead)
            {
                int sign = GaleIn <= R.lastSign ? 3 : GaleIn <= R.secondSign ? 2 : 1;
                if (sign > Sign)
                {
                    if (Sign == 0)
                        signAt = now;
                    Sign = sign;
                    SignCame?.Invoke(sign, GaleIn);
                }
            }
            else if (Sign != 0)
            {
                Sign = 0;                                                  // (it passed her by, or it is over, or she is made fast)
                signAt = long.MinValue;
            }

            // the stance in a gale, from the order: the sail comes down, is shortened, or is carried
            bool wasInGale = InGale;
            InGale = gale;
            bool running = gale && Order != GaleOrder.LieTo && Cloth != SailCloth.Split && h.offWind >= R.runAbaftDeg;
            bool carrying = running && Order == GaleOrder.CarrySail;
            if (gale)
                lastGaleHour = now;

            // the hull works
            if (gale)
                HullBy(-R.galeWearPerHour * (carrying ? R.carryWear : running ? R.reefedWear : 1.0) * (Tarred(day) ? 1.0 : R.untarredWear));

            // the sail's own dice
            bool sailUp = gale ? running : EffectiveSet != SailSet.Down;
            if (sailUp && h.underWay && Cloth == SailCloth.Weak && h.wind >= R.weakSplitsWind && GaleDice.Roll(h.seed, "player.sail", h.hour) < R.weakSplits)
            {
                Cloth = SailCloth.Split;
                Tell(facts, "The sail splits from the foot to the yard.");
            }
            else if (carrying && Cloth != SailCloth.Split && GaleDice.Roll(h.seed, "player.sail", h.hour) < R.carryTears)
            {
                Cloth = Cloth == SailCloth.Whole ? SailCloth.Weak : SailCloth.Split;
                Tell(facts, Cloth == SailCloth.Weak ? "A seam of the sail lets go." : "The sail splits from the foot to the yard.");
            }

            // water: in by the laps and over the gunwale, out by the men
            if (!h.moored)
            {
                double before = Water;
                bool wasAllHands = AllHands;
                double seas = gale ? (carrying ? R.seasCarry : running ? R.seasReefed : R.seasLieTo) : h.wind >= R.freshWind && !h.river ? R.freshSeas : 0.0;
                double into = Leak(h.wind) + seas, after = Water + into;
                // the fewest men that keep it down this hour: none, one, two, then all hands at a pace they cannot hold;
                // weary men lift less
                double tired = 1.0 - R.wearyOars * Weary, one = R.manBails * tired, two = Math.Min(2, men) * one;
                AllHands = false;
                if (after < R.oneManFrom)
                    Bailers = 0;
                else if (after - one < R.twoMenFrom)
                    Bailers = 1;
                else if (after - two < R.allHandsFrom)
                    Bailers = Math.Min(2, men);
                else
                {
                    Bailers = men;
                    AllHands = true;
                }
                double lifted = AllHands ? men * R.burstBails * tired : Bailers * one;
                Water = Math.Max(0.0, Math.Min(Swamp, after - lifted));
                if (before < R.spoilFrom && Water >= R.spoilFrom)
                    Tell(facts, "The water is over the floor timbers: the lowest of the cargo stands in it.");
                if (AllHands && !wasAllHands)
                    Tell(facts, "All hands to the bailing.");
            }
            else
            {
                Water = Math.Max(0.0, Water - men * R.manBails);           // (made fast: bailed dry at leisure)
                Bailers = 0;
                AllHands = false;
            }

            // the men tire, or rest
            bool rowing = h.underWay && (gale ? !running : EffectiveSet == SailSet.Down || h.wind < 1.5);
            if (h.moored)
                WearyBy(-R.restMoored);
            else if (!h.underWay && !gale)
                WearyBy(-R.restLyingUp);
            else
                WearyBy((h.night ? R.wearyNight : R.wearySailing) + (rowing ? R.wearyOarsHour : 0.0)
                        + R.wearyBailing * Bailers / men * (AllHands ? 2.0 : 1.0));

            // the sea's dice: the same as for every hull, times her state, and only in a gale she was warned of
            LastChance = 0.0;
            if (gale)
            {
                LastChance = GaleDice.Chance(h.lossPerHour, h.fragility, DiceFactor(h.river, day));
                if (!wasInGale)
                    Tell(facts, running ? "The gale is on her: she runs before it." : "The gale is on her: she lies to.");
                if (FairlyWarned && GaleDice.Roll(h.seed, "player", h.hour) < LastChance)
                    Strike(facts, running);
            }

            if (!IsLost && Water >= Swamp)
                Lose(facts, "She filled, and the sea came over the gunwale.");
            if (!IsLost && Hull <= 0.0)
                Lose(facts, "Her strakes opened and she went down under them.");
            return facts;
        }

        /// <summary>The sea strikes her: a wound before a death. A boat still tight takes a started strake; one already
        /// started loses her sail, if it is up; after that it is the boat.</summary>
        static void Strike(List<string> facts, bool sailUp)
        {
            if (Hull >= R.strainedBelow)
            {
                Hull = Math.Min(Hull - R.strikeHull, R.strakeHull);
                WaterBy(300.0);
                Tell(facts, "A sea breaks aboard. A strake has started below the waterline.");
            }
            else if (sailUp && Cloth != SailCloth.Split)
            {
                Cloth = SailCloth.Split;
                WaterBy(300.0);
                Tell(facts, "A sea lays her over. The sail splits, and she comes back up without it.");
            }
            else
                Lose(facts, "A sea broke over her that she could not lift to.");
        }

        static void Lose(List<string> facts, string how)
        {
            IsLost = true;
            LostHow = how;
            Tell(facts, how);
            Lost?.Invoke(how);
        }

        static void Tell(List<string> facts, string fact)
        {
            facts.Add(fact);
            Said?.Invoke(fact);
        }

        /// <summary>A day of the boat: the hull's slow wear (twice it untarred).</summary>
        public static void Day(double day) => HullBy(-R.wearPerDay * (Tarred(day) ? 1.0 : R.untarredWear));

        // ------------------------------------------------------------------ the cards' flags are this boat

        /// <summary>A card set one of the boat's flags: the boat takes it. False for a flag that is not the boat's.</summary>
        public static bool FlagSet(string name, double value, double days, double day, double worthNow)
        {
            bool on = value != 0.0;
            switch (name)
            {
                case "sea_leak":
                    if (on) Strake();
                    else Patch();
                    return true;
                case "sea_sail_weak":
                    if (on && Cloth == SailCloth.Whole) Cloth = SailCloth.Weak;
                    else if (!on && Cloth == SailCloth.Weak) Cloth = SailCloth.Whole;      // (sewn; a split sail takes "boat sail whole")
                    return true;
                case "sea_reefed":
                    SetSail(on ? SailSet.Reefed : SailSet.Full, on ? days * 24.0 : 0.0);
                    return true;
                case "sea_lashed":
                    Lash(day, on ? Math.Max(days, 1.0) : 0.0);
                    return true;
                case "sea_spar_aboard":
                    SpareSpar = on;
                    return true;
                case "sea_after_gale":
                    return true;                                             // (the boat knows: nothing to set)
                case "crew_on_shares":
                    Shares(on, worthNow);
                    return true;
                case "crew_own_bundles":
                    Bundles = on;
                    return true;
                case "crew_boat_tarred":
                    if (on) Tar(day);
                    return true;
            }
            return false;
        }

        /// <summary>What a card reads for one of the boat's flags (null: not the boat's; the deck's own memory answers).</summary>
        public static double? FlagView(string name, double day)
        {
            switch (name)
            {
                case "sea_leak": return Hull < R.strainedBelow ? 1.0 : 0.0;
                case "sea_sail_weak": return Cloth == SailCloth.Weak ? 1.0 : 0.0;
                case "sea_reefed": return EffectiveSet == SailSet.Reefed ? 1.0 : 0.0;
                case "sea_lashed": return Lashed(day) ? 1.0 : 0.0;
                case "sea_spar_aboard": return SpareSpar ? 1.0 : 0.0;
                case "sea_after_gale": return !InGale && HoursSinceGale <= 48.0 ? 1.0 : 0.0;
                case "crew_on_shares": return OnShares ? 1.0 : 0.0;
                case "crew_own_bundles": return Bundles ? 1.0 : 0.0;
                case "crew_boat_tarred": return Tarred(day) ? 1.0 : 0.0;
            }
            return null;
        }

        // ------------------------------------------------------------------ words for the panels

        public static string HullWord => Hull >= 0.9 ? "sound" : Hull >= R.strainedBelow ? "working" : Hull >= R.openedBelow ? "a strake started" : Hull >= 0.3 ? "opening" : "going";
        public static string WaterWord => Water < 20.0 ? "dry" : Water < R.oneManFrom ? "wet underfoot" : Water < R.twoMenFrom ? "a hand deep"
                                        : Water < R.allHandsFrom ? "over the floor timbers" : Water < Swamp * 0.75 ? "to the thwarts" : "awash";
        public static string ClothWord => Cloth == SailCloth.Whole ? "whole" : Cloth == SailCloth.Weak ? "a seam gone" : "split: half a sail";
        public static string SetWord => EffectiveSet == SailSet.Full ? "all of it" : EffectiveSet == SailSet.Reefed ? "shortened" : "down";
        public static string OrderWord => Order == GaleOrder.LieTo ? "lie to" : Order == GaleOrder.RunReefed ? "run under shortened sail" : "carry sail";
        public static string WearyWord => Weary < 0.25 ? "fresh" : Weary < 0.5 ? "tired" : Weary < 0.8 ? "weary" : "spent";

        /// <summary>The chance of a strike in a night of it (twelve hours) at this chance an hour.</summary>
        public static double NightChance(double chancePerHour) => 1.0 - Math.Pow(1.0 - Math.Max(0.0, Math.Min(1.0, chancePerHour)), 12.0);

        /// <summary>How she would fare in a gale as she is, from the chance an hour.</summary>
        public static string GaleWord(double chancePerHour)
        {
            double night = NightChance(chancePerHour);
            return night < 0.005 ? "she would ride it" : night < 0.02 ? "she would be pressed" : night < 0.08 ? "she would be in danger" : "she would not live long in it";
        }

        // ------------------------------------------------------------------ the save

        public static Dictionary<string, object> Capture() => new Dictionary<string, object>
        {
            ["hull"] = Hull, ["water"] = Water, ["cloth"] = (double)(int)Cloth, ["set"] = (double)(int)Set, ["order"] = (double)(int)Order,
            ["weary"] = Weary, ["shares"] = OnShares, ["bundles"] = Bundles, ["spar"] = SpareSpar, ["sign"] = (double)Sign,
            ["now"] = (double)now, ["sign_at"] = Stamp(signAt), ["last_gale"] = Stamp(lastGaleHour),
            ["lie_to"] = Stamp(lieToUntil), ["reef_until"] = reefUntil == long.MaxValue ? -1.0 : reefUntil, ["in_gale"] = InGale,
            ["lashed"] = double.IsInfinity(lashedUntilDay) ? -1.0 : lashedUntilDay, ["tarred"] = double.IsInfinity(tarredDay) ? -1.0e9 : tarredDay,
            ["base"] = double.IsNaN(seasonBase) ? -1.0e18 : seasonBase, ["paid_year"] = (double)paidYear, ["lost"] = IsLost, ["lost_how"] = LostHow,
        };

        static double Stamp(long hour) => hour == long.MinValue ? -1.0 : hour;
        static long Unstamp(double v) => v < 0.0 ? long.MinValue : (long)v;

        /// <summary>A saved game's boat; a save from before the boat was kept (null) is a sound boat on a wage.</summary>
        public static void Restore(Dictionary<string, object> d)
        {
            Begin(R);
            if (d == null)
                return;
            Hull = Json.Num(d, "hull", 1.0);
            Water = Json.Num(d, "water");
            Cloth = (SailCloth)(int)Json.Num(d, "cloth");
            Set = (SailSet)(int)Json.Num(d, "set");
            Order = (GaleOrder)(int)Json.Num(d, "order");
            Weary = Json.Num(d, "weary");
            OnShares = Json.Bool(d, "shares");
            Bundles = Json.Bool(d, "bundles");
            SpareSpar = Json.Bool(d, "spar");
            Sign = (int)Json.Num(d, "sign");
            now = (long)Json.Num(d, "now");
            signAt = Unstamp(Json.Num(d, "sign_at", -1.0));
            lastGaleHour = Unstamp(Json.Num(d, "last_gale", -1.0));
            lieToUntil = Unstamp(Json.Num(d, "lie_to", -1.0));
            double reef = Json.Num(d, "reef_until", -1.0);
            reefUntil = reef < 0.0 ? long.MaxValue : (long)reef;
            InGale = Json.Bool(d, "in_gale");
            double lashed = Json.Num(d, "lashed", -1.0), tarred = Json.Num(d, "tarred", -1.0e9), b = Json.Num(d, "base", -1.0e18);
            lashedUntilDay = lashed < 0.0 ? double.NegativeInfinity : lashed;
            tarredDay = tarred <= -1.0e8 ? double.NegativeInfinity : tarred;
            seasonBase = b <= -1.0e17 ? double.NaN : b;
            paidYear = (int)Json.Num(d, "paid_year", int.MinValue);
            IsLost = Json.Bool(d, "lost");
            LostHow = Json.Str(d, "lost_how");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            SignCame = null;
            Said = null;
            Lost = null;
            Begin(null);
        }
    }
}
```

### 8.3 `Assets/Kapman/Code/Game/Voyage/BoatWatch.cs` (new)

```csharp
using System;
using Kapman.Game.Sim;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// THE BOAT IN THE LIVE GAME - each game hour (KapmanGame's tick ladder: every hour once, in order, at every speed of
    /// the clock) it reads the weather where she is and for the hours ahead (the world's own model, the one the other
    /// ships sail by), hands the hour to <see cref="Boat.Hour"/>, and does what the boat cannot do itself: says the
    /// facts in the log, drops the clock and opens the card when a sign of a gale comes, lets the water take its room
    /// in the hold and spoil what stands in it, reckons the men's share at Martinmas, and ends the game when she is lost.
    /// </summary>
    public static class BoatWatch
    {
        /// <summary>The card the second sign opens (the swell: shorten sail, turn for a lee, or hold on).</summary>
        public const string GaleCard = "sea.swell.long";
        /// <summary>The card the last sign opens (the gale: lie to, run under shortened sail, or carry it).</summary>
        public const string OrderCard = "sea.gale.breaks";
        /// <summary>The settings' switch (ruling 26: each drop of the clock can be switched off). The card still holds the
        /// clock when it opens (ruling 5), so a gale is never met unasked.</summary>
        public static bool DropForWeather = true;
        /// <summary>The game is over (how she was lost): the frame shows the last page.</summary>
        public static event Action<string> GameOver;

        static bool hooked;

        static void Hook()
        {
            if (hooked)
                return;
            hooked = true;
            Boat.Said += Voyage.Say;
            Boat.SignCame += OnSign;
            Boat.Lost += OnLost;
            KapmanGame.Jumped += (from, to) => Boat.Grace((long)Math.Ceiling(to) + 1);   // (a jump of the clock is not play)
        }

        /// <summary>Hours to the first hour of wind over the gale line at a place, looking `look` hours ahead (infinity:
        /// none). The weather model answers for any hour, so this is no guess: it is what will blow there.</summary>
        public static double GaleIn(WorldSim world, WorldXZ at, double look)
        {
            if (world == null || world.weather == null)
                return double.PositiveInfinity;
            var (lat, lon) = WorldFrame.ToLatLon(at.x, at.z);
            for (int k = 1; k <= (int)look; k++)
                if (world.weather.Sample(lat, lon, world.WeatherHours + k, true).windSpeed > GaleDice.GaleWind)
                    return k;
            return double.PositiveInfinity;
        }

        /// <summary>The house's worth now, deniers: silver and the cargo at the table's values, less the debt.</summary>
        public static double Worth()
        {
            if (!Trade.Started)
                return 0.0;
            double v = Trade.Purse.Silver(TradeRules.Current) - Trade.Debt;
            foreach (var (good, count) in Trade.Hold.Contents)
                v += count * (double)good.value;
            return v;
        }

        /// <summary>A game hour has passed (KapmanGame.Ticks.Hour, after the world's ships have had theirs).</summary>
        public static void Hour(long hour)
        {
            var game = KapmanGame.Current;
            var world = game != null ? game.World : null;
            var body = ChartBody.Current;
            if (game == null || world == null || game.ship == null || body == null || !Voyage.Started || Boat.IsLost)
                return;
            Hook();
            var cls = world.ClassOf(body.classId);
            if (cls == null)
                return;
            var at = ChartWorld.ShipWorld();
            var w = world.WeatherAt(at);
            bool moored = Port.LyingId() != null;
            double hourOfDay = ((game.Hours % 24.0) + 24.0) % 24.0;
            double off = Math.Abs(((game.ship.Heading - w.windFrom) % 360.0 + 540.0) % 360.0 - 180.0);
            Boat.Hour(new BoatHour
            {
                hour = hour, wind = w.windSpeed, offWind = off,
                underWay = !moored && !game.ship.anchored && Course.IsSet, moored = moored, river = ChartWorld.OnRiver(),
                night = hourOfDay < 5.0 || hourOfDay >= 21.0,
                galeIn = moored ? double.PositiveInfinity : GaleIn(world, at, Boat.R.lookAhead),
                men = Voyage.Men, hands = Mathf.Max(1, Mathf.RoundToInt(cls.crew)),
                length = cls.length, beam = cls.beam, fragility = cls.galeFragility, lossPerHour = world.rules.galeLossPerHour,
                seed = world.seed,
            });
            // the water in her and the men's bundles take their room in the hold
            if (Trade.Started)
                Trade.Hold.taken = (float)Boat.TakenKg;
            // weary men lose heart
            if (Boat.Weary >= Boat.R.wearyHeartFrom)
                Voyage.Affect(0.0, -Boat.R.wearyHeartPerHour);
        }

        /// <summary>A game day has passed: the hull's wear, what the water spoils, and Martinmas.</summary>
        public static void Day(int day)
        {
            var game = KapmanGame.Current;
            if (game == null || !Voyage.Started || Boat.IsLost)
                return;
            Hook();
            Boat.Day(game.Day);
            if (Trade.Started && Boat.Water >= Boat.R.spoilFrom)
                foreach (var (good, count) in Trade.Hold.Contents)
                    if (count > 0 && WetSpoils(good))
                    {
                        Trade.Hold.Remove(good, 1);
                        Voyage.Say($"Standing in the water: {Trade.Units(good, 1)} spoiled.");
                        break;
                    }
            if (game.clock == null)
                return;
            var date = game.clock.Date;
            double owed = Boat.Martinmas(date.year, date.month, date.day, Worth(), out bool due);
            if (!due)
                return;
            if (owed > 0.0)
            {
                Voyage.Owe(owed);
                Voyage.Affect(0.0, Boat.R.gainHeart);
                Voyage.Say($"Martinmas: a tenth of the season's gain is the men's. {owed:0} d is owed them.");
            }
            else
            {
                Voyage.Affect(0.0, Boat.R.noGainHeart);
                Voyage.Say("Martinmas: the season made no gain, and the men's share of nothing is nothing.");
            }
        }

        /// <summary>The goods water spoils when the lowest of the cargo stands in it.</summary>
        public static bool WetSpoils(GoodDef g) =>
            g != null && (g.keeps > 0 || g.cat == "Food" || g.cat == "Provisions" || g.cat == "Spices" || g.cat == "Textiles"
                          || g.cat == "Fibres" || g.cat == "Furs" || g.cat == "Dyes" || g.cat == "Books and church goods");

        static void OnSign(int sign, double hours)
        {
            // the words are placeholders, the user's to change
            string text = sign == 1 ? "A long swell from a quarter the wind is not in. Weather is coming, half a day off or less."
                        : sign == 2 ? "The sky is lowering and the gulls are going inland. It will blow hard before the watch is out."
                        : "White water to windward. It is on her within the hour.";
            Voyage.Say(text);
            if (DropForWeather)
                GameSpeed.Slow();                                          // (five speeds: sign 1 to speed 2, signs 2 and 3 to speed 1)
            var cards = EncounterRunner.Director;
            if (cards != null && sign >= 2)
                cards.Offer(sign == 2 ? GaleCard : OrderCard);             // (first in line; it holds the clock when it opens)
        }

        /// <summary>
        /// "Turn for the nearest lee": her course is laid to the nearest port she can reach by water (a made-fast boat
        /// is out of the gale: no dice, no seas). The fact for the card's outcome; null when there is nowhere to run.
        /// </summary>
        public static string RunForShelter()
        {
            var game = KapmanGame.Current;
            if (game == null || game.ship == null)
                return null;
            var at = ChartWorld.ShipWorld();
            PortDef best = null;
            double bestKm = double.MaxValue;
            var line = new System.Collections.Generic.List<WorldXZ>();
            foreach (var def in GameData.Ports)
            {
                if (def == null || string.IsNullOrEmpty(def.id))
                    continue;
                var (x, z) = WorldFrame.ToWorld(def.lat, def.lon);
                if (WorldXZ.Distance(at, new WorldXZ(x, z)) > 150000.0)     // (a day's run at the most: no need to plan to them all)
                    continue;
                if (Course.Plan(def.id, at, null, line, out double km) && km < bestKm)
                {
                    best = def;
                    bestKm = km;
                }
            }
            if (best == null || !Course.SetTo(best.id, at, null))
                return null;
            return $"She turns for {best.name}, {bestKm:0} km off.";
        }

        static void OnLost(string how)
        {
            GameSpeed.Hold("lost");                                       // (the clock stops; SaveGame.Busy refuses a save from here)
            GameOver?.Invoke(how);
        }

        /// <summary>A saved game was loaded: the clock's hold for a lost boat is let go if this one swims.</summary>
        public static void Loaded()
        {
            if (!Boat.IsLost)
                GameSpeed.Release("lost");
            if (Trade.Started)
                Trade.Hold.taken = (float)Boat.TakenKg;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            hooked = false;
            GameOver = null;
            DropForWeather = true;
        }
    }
}
```

### 8.4 `Assets/Kapman/Code/Game/UI/Frame/Panels/ShipPanel.cs` (replaces the file)

```csharp
using Kapman.Game.Sim;
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// SHIP - the boat she is: her kind, her size, what she draws and carries, how she goes under sail and under oars,
    /// from her class in the tables; and the boat as she is today (Voyage/Boat.cs): her hull, the water in her, her
    /// sail, the order for a gale, and what a night of gale would be to her as she is. The orders are given here.
    /// </summary>
    public sealed class ShipPanel : FramePanel
    {
        public override string Id => "ship";
        public override string Title => "Ship";
        public override int Order => 90;

        protected override int Stamp()
        {
            var game = KapmanGame.Current;
            int h = FrameUi.Mix(17, game != null && game.ship != null && game.ship.shipClass != null ? game.ship.shipClass.shipName : "");
            h = FrameUi.Mix(h, Trade.Started ? (int)(Trade.Hold.capacity - Trade.Hold.taken) : 0);
            h = FrameUi.Mix(h, Boat.HullWord);
            h = FrameUi.Mix(h, (int)(Boat.Water / 10.0));
            h = FrameUi.Mix(h, (int)Boat.Cloth * 9 + (int)Boat.EffectiveSet * 3 + (int)Boat.Order);
            h = FrameUi.Mix(h, Boat.Bailers + (Boat.AllHands ? 10 : 0) + Boat.Sign * 100);
            h = FrameUi.Mix(h, double.IsInfinity(Boat.GaleIn) ? -1 : (int)Boat.GaleIn);
            h = FrameUi.Mix(h, (int)(Boat.Weary * 10.0));
            h = FrameUi.Mix(h, FrameGame.PortHere != null ? FrameGame.PortHere.id : "");
            return FrameUi.Mix(h, Boat.IsLost);
        }

        /// <summary>The sea's chance of striking her in an hour of gale, as she is now.</summary>
        public static double ChanceNow()
        {
            var game = KapmanGame.Current;
            var world = game != null ? game.World : null;
            var cls = world != null && ChartBody.Current != null ? world.ClassOf(ChartBody.Current.classId) : null;
            if (cls == null)
                return 0.0;
            return GaleDice.Chance(world.rules.galeLossPerHour, cls.galeFragility, Boat.DiceFactor(ChartWorld.OnRiver(), game.Day));
        }

        /// <summary>"1 in 400" for a chance (the nearest round figure), "next to none" under 1 in 5,000.</summary>
        public static string OneIn(double chance)
        {
            if (chance <= 0.0002)
                return "next to none";
            double n = 1.0 / chance;
            double step = n >= 1000.0 ? 500.0 : n >= 100.0 ? 50.0 : n >= 20.0 ? 5.0 : 1.0;
            return "1 in " + (System.Math.Max(1.0, System.Math.Round(n / step) * step)).ToString("0");
        }

        protected override void Fill(VisualElement body)
        {
            var game = KapmanGame.Current;
            var ship = game != null ? game.ship : null;
            if (ship == null || ship.shipClass == null)
            {
                body.Add(FrameUi.P("There is no ship to look over yet."));
                return;
            }
            var c = ship.shipClass;
            body.Add(FrameUi.H("The " + c.shipName));
            if (Boat.IsLost)
            {
                body.Add(FrameUi.P(Boat.LostHow));
                return;
            }
            if (!string.IsNullOrEmpty(c.about))
                body.Add(FrameUi.Text(c.about));
            body.Add(FrameUi.Ln("Length and beam", null, FrameUi.Num($"{c.length:0.#} m by {c.beam:0.#} m")));
            body.Add(FrameUi.Ln("Draws", "The water she needs under her, loaded.", FrameUi.Num($"{c.draught:0.##} m")));
            if (Trade.Started)
                body.Add(FrameUi.Ln("Carries", Trade.Hold.taken >= 1f ? "Room for cargo, the water casks taken off, and what the water in her and the men's bundles take."
                                                                     : "Room for cargo, the water casks taken off.",
                                    FrameUi.Num($"{System.Math.Max(0f, Trade.Hold.capacity - Trade.Hold.taken):0} kg")));
            var cls = game.World != null && ChartBody.Current != null ? game.World.ClassOf(ChartBody.Current.classId) : null;
            if (cls != null)
            {
                if (cls.hasSail)
                    body.Add(FrameUi.Ln("Under sail", "In a fair wind.", FrameUi.Num($"{cls.sailKn:0.#} knots")));
                if (cls.oarsKn > 0f)
                    body.Add(FrameUi.Ln("Under oars", null, FrameUi.Num($"{cls.oarsKn:0.#} knots")));
                if (cls.crew > 0f)
                    body.Add(FrameUi.Ln("Hands she wants", null, FrameUi.Num($"{cls.crew:0}")));
            }

            // the boat as she is today
            body.Add(FrameUi.H("As she is"));
            body.Add(FrameUi.Ln("Hull", "How tight she is. A started strake leaks; a patch holds it, a yard or a haul-out with tar mends it.",
                                FrameUi.Tag(Boat.HullWord)));
            body.Add(FrameUi.Ln("Water in her", Boat.AllHands ? "All hands are bailing." : Boat.Bailers == 2 ? "Two men are bailing."
                                                : Boat.Bailers == 1 ? "One man is bailing." : "It weighs on her, slows her, and spoils what stands in it.",
                                FrameUi.Tag(Boat.WaterWord), FrameUi.Num($"{Boat.Water:0} l")));
            body.Add(FrameUi.Ln("Sail", "The cloth, and how it is set. Shortened, she makes seven tenths of her speed.",
                                FrameUi.Tag(Boat.ClothWord), FrameUi.Tag(Boat.SetWord)));
            body.Add(FrameUi.Ln("Order for a gale", "What the men do when it blows a gale.", FrameUi.Tag(Boat.OrderWord)));
            double p = ChanceNow();
            body.Add(FrameUi.Ln("A night of gale, as she is",
                                "The sea's chance of striking her in twelve hours of it. The first strike starts a strake, the next takes the sail, the third the boat.",
                                FrameUi.Tag(Boat.GaleWord(p)), FrameUi.Num(OneIn(Boat.NightChance(p)))));
            if (Boat.InGale)
                body.Add(FrameUi.Ln("Weather", null, FrameUi.Tag("a gale, now")));
            else if (Boat.Sign > 0)
                body.Add(FrameUi.Ln("Weather", "The men read the swell and the sky.", FrameUi.Tag($"a gale within {Boat.GaleIn:0} hours")));

            // the orders
            var port = FrameGame.PortHere;
            if (port == null)
            {
                body.Add(FrameUi.Ln("Set the sail", null,
                    FrameUi.Btn("All of it", () => Do(() => Boat.SetSail(SailSet.Full)), "Her full speed."),
                    FrameUi.Btn("Shorten", () => Do(() => Boat.SetSail(SailSet.Reefed)), "Seven tenths of her speed; ready for weather."),
                    FrameUi.Btn("Down", () => Do(() => Boat.SetSail(SailSet.Down)), "She goes by the oars. The men tire.")));
                body.Add(FrameUi.Ln("In a gale", null,
                    FrameUi.Btn("Lie to", () => Do(() => Boat.SetOrder(GaleOrder.LieTo)), "Sail struck, her head held to it under oars. She makes no way. The safest: what every other ship does."),
                    FrameUi.Btn("Run shortened", () => Do(() => Boat.SetOrder(GaleOrder.RunReefed)), "With the wind abaft the beam she runs on under the shortened sail. More water, more work for the hull, the sea's chance two and a half times."),
                    FrameUi.Btn("Carry sail", () => Do(() => Boat.SetOrder(GaleOrder.CarrySail)), "All of it, and run. Her best speed; the sail may tear, she ships a great deal of water, the sea's chance eight times.")));
                body.Add(FrameUi.Ln("Shelter", null,
                    FrameUi.Btn("Run for the nearest port", () => Do(() => FrameLog.Add(BoatWatch.RunForShelter() ?? "There is nowhere near to run to.")),
                                "Lays her course for the nearest port by water. Made fast, she is out of it.")));
            }
            else if (Trade.Started)
            {
                double hull = Boat.HullMendCost, sail = Boat.SailMendCost;
                if (hull > 0.0)
                    body.Add(FrameUi.Ln("Her seams", "Hauled up, caulked and tarred: sound again.",
                        FrameUi.Btn($"Have them done ({hull:0} d)", () => Do(() => Mend(port, hull, true)))));
                if (sail > 0.0)
                    body.Add(FrameUi.Ln("Her sail", Boat.Cloth == SailCloth.Split ? "A new cloth sewn to the yard." : "The seam sewn.",
                        FrameUi.Btn($"Have it done ({sail:0} d)", () => Do(() => Mend(port, sail, false)))));
                if (hull <= 0.0 && sail <= 0.0)
                    body.Add(FrameUi.P("She wants nothing from a yard today."));
            }
        }

        void Do(System.Action what)
        {
            what();
            Redraw();
        }

        void Mend(PortDef port, double cost, bool hull)
        {
            var rules = TradeRules.Current;
            if (!Trade.Purse.Pay(cost, rules.ZoneOf(port.region), rules, out _))
            {
                FrameLog.Add("There is not the silver for it.");
                return;
            }
            if (hull)
                Boat.Tar(KapmanGame.Current != null ? KapmanGame.Current.Day : 0.0);
            else
                Boat.SetCloth(SailCloth.Whole);
            FrameLog.Add(hull ? $"Her seams are done and tarred: {cost:0} d." : $"The sail is whole again: {cost:0} d.");
        }
    }
}
```

### 8.5 `Assets/Kapman/Code/Game/UI/Frame/Panels/CrewPanel.cs` (replaces the file)

```csharp
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// CREW - the men aboard as the game knows them today: how many, their health and their heart as words, how weary
    /// they are and what they are doing about the water in her (Voyage/Boat.cs), how they are paid (a wage, or shares)
    /// and what they are owed, how long they have been at sea. They are not yet people with names: the panel says so.
    /// </summary>
    public sealed class CrewPanel : FramePanel
    {
        public override string Id => "crew";
        public override string Title => "Crew";
        public override int Order => 35;

        protected override int Stamp()
        {
            int h = FrameUi.Mix(17, Voyage.Started);
            h = FrameUi.Mix(h, Voyage.Men);
            h = FrameUi.Mix(h, (int)(Voyage.Health * 20));
            h = FrameUi.Mix(h, (int)(Voyage.Morale * 20));
            h = FrameUi.Mix(h, (int)Voyage.PayOwed);
            h = FrameUi.Mix(h, Boat.WearyWord);
            h = FrameUi.Mix(h, Boat.Bailers + (Boat.AllHands ? 10 : 0) + (Boat.OnShares ? 100 : 0) + (Boat.Bundles ? 1000 : 0));
            return FrameUi.Mix(h, Voyage.DaysAtSea);
        }

        protected override void Fill(VisualElement body)
        {
            if (!Voyage.Started)
            {
                body.Add(FrameUi.P("There is no crew to speak of yet."));
                return;
            }
            body.Add(FrameUi.H(Voyage.Men == 1 ? "One man" : Voyage.Men + " men"));
            body.Add(FrameUi.Ln("Health", null, FrameUi.Tag(FrameGame.HealthWord(Voyage.Health))));
            body.Add(FrameUi.Ln("Heart", null, FrameUi.Tag(FrameGame.HeartWord(Voyage.Morale))));
            body.Add(FrameUi.Ln("Weariness", "Oars, bailing and nights under way tire them; a mooring rests them. Weary men row slower, and the sea finds a weary boat sooner.",
                                FrameUi.Tag(Boat.WearyWord)));
            if (Boat.Bailers > 0)
                body.Add(FrameUi.Ln("At the bailing", "Men bailing are not at the oars.",
                                    FrameUi.Tag(Boat.AllHands ? "all hands" : Boat.Bailers == 1 ? "one man" : "two men")));
            body.Add(FrameUi.Ln("Paid by", Boat.OnShares ? "Half the wage each day, and a tenth of the season's gain at Martinmas."
                                                         : "A wage each day aboard, paid out in port.",
                                FrameUi.Tag(Boat.OnShares ? "shares" : "the wage")));
            if (Boat.Bundles)
                body.Add(FrameUi.Ln("Their own bundles", "Each man carries goods of his own: it comes out of the hold's room.",
                                    FrameUi.Num($"{Boat.R.bundleKgPerMan * Voyage.Men:0} kg")));
            body.Add(FrameUi.Ln("Pay owed", null, FrameUi.Num($"{Voyage.PayOwed:0} d")));
            body.Add(FrameUi.Ln("At sea", null, FrameUi.Num(Voyage.DaysAtSea <= 0 ? "not yet" : Voyage.DaysAtSea == 1 ? "1 day" : Voyage.DaysAtSea + " days")));
            body.Add(FrameUi.Ln("Bread and other food", null, FrameUi.Num(Voyage.FoodDays < 0.25 ? "none" : FrameUi.Days(Voyage.FoodDays))));
            body.Add(FrameUi.Ln("Ale and water", null, FrameUi.Num(Voyage.DrinkDays < 0.25 ? "none" : FrameUi.Days(Voyage.DrinkDays))));
            body.Add(FrameUi.P("The crew are not yet people with names, tongues and faiths of their own: that comes with hiring."));
        }
    }
}
```

### 8.6 `Assets/Kapman/Game/Data/boat_rules.json` (new, optional)

The names are the fields of `BoatRules`. Drag it onto `KapmanGame.boatRules` in L_Chart; with none, the code's own
numbers (these) are used. Every number is a starting guess to tune.

```json
{
 "about": "THE BOAT (Code/Game/Voyage/Boat.cs): her wear, leaks, bailing, weariness, signs, shares, and what multiplies the sea's chance in a gale. Every number is a STARTING GUESS TO TUNE. The names are the code's.",
 "_0": "her speed",
 "reefedSpeed": 0.7, "weakSailCap": 0.8, "splitSpeed": 0.5, "wearyOars": 0.5, "waterSlow": 0.3, "runAbaftDeg": 100,
 "_2": "the hull: wear, and where a wound or a mend leaves her",
 "wearPerDay": 0.0005, "untarredWear": 2, "galeWearPerHour": 0.004, "reefedWear": 2, "carryWear": 5, "strakeHull": 0.55, "patchHull": 0.75, "strikeHull": 0.3, "tarLastsDays": 300,
 "_4": "water: litres an hour in, and what the men lift out",
 "soundLeak": 2, "openLeak": 6000, "workFromWind": 8, "freshSeas": 60, "seasLieTo": 300, "seasReefed": 1200, "seasCarry": 4000, "freshWind": 13, "manBails": 1500, "burstBails": 2400, "oneManFrom": 100, "twoMenFrom": 400, "allHandsFrom": 1200, "swampPerM2": 180, "spoilFrom": 400,
 "_6": "weariness, a share an hour",
 "wearySailing": 0.004, "wearyNight": 0.008, "wearyOarsHour": 0.03, "wearyBailing": 0.02, "restMoored": 0.04, "restLyingUp": 0.03, "wearyHeartFrom": 0.8, "wearyHeartPerHour": 0.005,
 "_8": "the dice: what multiplies the chance every hull has",
 "diceReefed": 2.5, "diceCarry": 8, "diceStrained": 2, "diceOpened": 5, "diceHeavy": 2, "diceShortHanded": 1.5, "diceLashed": 0.8, "diceRiver": 0.1, "strainedBelow": 0.7, "openedBelow": 0.5,
 "_10": "the sail's own dice, a chance an hour",
 "weakSplits": 0.12, "weakSplitsWind": 9, "carryTears": 0.15,
 "_12": "the signs: hours ahead the weather is read, and when each sign comes",
 "lookAhead": 12, "secondSign": 6, "lastSign": 1, "fairAfterHours": 1,
 "_14": "shares and bundles",
 "shareOfGain": 0.1, "wageOnShares": 0.5, "bundleKgPerMan": 20, "noGainHeart": -0.1, "gainHeart": 0.1, "martinmasMonth": 11, "martinmasDay": 11,
 "_16": "a yard's prices, deniers",
 "hullMendBase": 8, "hullMendFull": 20, "sailSewn": 3, "sailNew": 12
}
```

---

## 9. Code: edits to existing files

Each block gives the lines as they are today and what they become. The line number is where the old lines begin
in the file as read today. All 24 were applied to copies by script (each found exactly once) and compiled.

### `Assets/Kapman/Code/Core/Encounters/IGameEffects.cs`

**9.1** line 50. Now:

```csharp
        /// <summary>A tongue learned aboard, to a level (0.5: understood in part).</summary>
        string Tongue(string name, double level);
```

Becomes:

```csharp
        /// <summary>A tongue learned aboard, to a level (0.5: understood in part).</summary>
        string Tongue(string name, double level);
        /// <summary>A card set a flag. True when the live game keeps that thing itself (the boat's hurts, the men's
        /// shares: Kapman.Game.Boat) and has taken it: the deck then keeps no flag of its own for it.</summary>
        bool Flagged(string name, double value, double days) => false;
        /// <summary>An order to the boat from a card ("boat sail split", "boat lie_to 6"): what, a word or a number.</summary>
        string BoatOrder(string what, string word, double n) => null;
```

### `Assets/Kapman/Code/Core/Encounters/Effects.cs`

**9.2** line 139. Now:

```csharp
                case "flag":
                    if (b == "add")
```

Becomes:

```csharp
                case "boat": ctx.Say(g?.BoatOrder(a, b, n)); break;
                case "flag":
                    if (b != "add" && g != null && g.Flagged(a, n, days))
                        break;                                             // (the boat's own: kept in the simulation, not in the deck)
                    if (b == "add")
```

**9.3** line 204. Now:

```csharp
            "commission", "give_text", "give_riddle", "open_fetch", "hire", "pay_off", "passenger", "encounter", "law", "learn",
            "tongue",
        };
```

Becomes:

```csharp
            "commission", "give_text", "give_riddle", "open_fetch", "hire", "pay_off", "passenger", "encounter", "law", "learn",
            "tongue", "boat",
        };

        /// <summary>The boat's orders: those that take a number, and those that take one of these words (none: no word).</summary>
        static readonly string[] BoatNumbers = { "hull", "water", "weary", "lie_to" };
        static readonly Dictionary<string, string[]> BoatWords = new Dictionary<string, string[]>
        {
            ["sail"] = new[] { "whole", "weak", "split" }, ["set"] = new[] { "full", "reefed", "down" },
            ["order"] = new[] { "lie_to", "reefed", "carry" }, ["shares"] = new[] { "on", "off" },
            ["tar"] = new string[0], ["shelter"] = new string[0],
        };
```

**9.4** line 489. Now:

```csharp
                case "tongue":
                    if (argc == 3 && t[2] == "=" && Number(t[3], out e.n) && e.n >= 0.0 && e.n <= 1.0)
```

Becomes:

```csharp
                case "boat":
                    // boat hull|water|weary|lie_to <n>  |  boat sail|set|order|shares <word>  |  boat tar  |  boat shelter
                    if (argc == 2 && Array.IndexOf(BoatNumbers, t[1]) >= 0 && Number(t[2], out e.n))
                    {
                        e.a = t[1];
                        return e;
                    }
                    if (argc >= 1 && BoatWords.TryGetValue(t[1], out var words)
                        && (words.Length == 0 ? argc == 1 : argc == 2 && Array.IndexOf(words, t[2]) >= 0))
                    {
                        e.a = t[1];
                        e.b = argc == 2 ? t[2] : "";
                        return e;
                    }
                    why = "'boat' is 'boat hull|water|weary|lie_to <n>', 'boat sail whole|weak|split', 'boat set full|reefed|down', " +
                          "'boat order lie_to|reefed|carry', 'boat shares on|off', 'boat tar' or 'boat shelter'";
                    return null;
                case "tongue":
                    if (argc == 3 && t[2] == "=" && Number(t[3], out e.n) && e.n >= 0.0 && e.n <= 1.0)
```

### `Assets/Kapman/Code/Core/Encounters/EncounterVars.cs`

**9.5** line 19. Now:

```csharp
        /// <summary>The live game's answer for a text not in the table (null: "").</summary>
        public Func<string, string> textSource;
```

Becomes:

```csharp
        /// <summary>The live game's answer for a text not in the table (null: "").</summary>
        public Func<string, string> textSource;
        /// <summary>The live game's answer for a flag it keeps itself (the boat's hurts, the men's shares); null from it,
        /// or no source: the deck's own memory answers.</summary>
        public Func<string, double?> flagSource;
```

**9.6** line 36. Now:

```csharp
            if (name.StartsWith("flag.", StringComparison.Ordinal))
                return state != null ? state.Flag(name.Substring(5), day) : 0.0;
```

Becomes:

```csharp
            if (name.StartsWith("flag.", StringComparison.Ordinal))
            {
                string flag = name.Substring(5);
                double? live = flagSource != null ? flagSource(flag) : null;
                return live ?? (state != null ? state.Flag(flag, day) : 0.0);
            }
```

### `Assets/Kapman/Code/Core/Encounters/Cond.cs`

**9.7** line 48. Now:

```csharp
            ["generation"] = (VarType.Number, null),
```

Becomes:

```csharp
            ["generation"] = (VarType.Number, null),
            // the boat (7 Oct 2026, Game/Voyage/Boat.cs): a gale where she is (1), the hours to one (99: none in sight),
            // her hull (1 sound .. 0), the water in her (litres), the men's weariness (0..1), the sail (whole, weak, split)
            ["gale"] = (VarType.Number, null), ["gale_in"] = (VarType.Number, null), ["boat.hull"] = (VarType.Number, null),
            ["boat.water"] = (VarType.Number, null), ["boat.weary"] = (VarType.Number, null), ["boat.sail"] = (VarType.Text, null),
```

### `Assets/Kapman/Code/Game/Encounters/EncounterGameVars.cs`

**9.8** line 34. Now:

```csharp
            vars.textSource = name => Text(name, GameData.Port(portNow != null ? portNow() : null));
```

Becomes:

```csharp
            vars.textSource = name => Text(name, GameData.Port(portNow != null ? portNow() : null));
            vars.flagSource = name => Boat.FlagView(name, Today);          // (the boat's flags are the boat: Voyage/Boat.cs)
```

**9.9** line 68. Now:

```csharp
                case "days_at_sea":
                    return Voyage.DaysAtSea;
```

Becomes:

```csharp
                case "days_at_sea":
                    return Voyage.DaysAtSea;
                case "gale":
                    return Boat.InGale ? 1.0 : 0.0;
                case "gale_in":
                    return double.IsInfinity(Boat.GaleIn) || double.IsNaN(Boat.GaleIn) ? 99.0 : Boat.GaleIn;
                case "boat.hull":
                    return Boat.Hull;
                case "boat.water":
                    return Boat.Water;
                case "boat.weary":
                    return Boat.Weary;
```

**9.10** line 136. Now:

```csharp
                case "me.people":
                    return Self.People;
```

Becomes:

```csharp
                case "boat.sail":
                    return Boat.Cloth == SailCloth.Whole ? "whole" : Boat.Cloth == SailCloth.Weak ? "weak" : "split";
                case "me.people":
                    return Self.People;
```

### `Assets/Kapman/Code/Game/Encounters/GameEffects.cs`

**9.11** line 237. Now:

```csharp
        public string Learn(string what, double amount) => null;          // (reading lessons from a card: to come)
```

Becomes:

```csharp
        public string Learn(string what, double amount) => null;          // (reading lessons from a card: to come)

        // ------------------------------------------------------------------ the boat (Voyage/Boat.cs)

        /// <summary>A card set a flag: the boat takes the ones that are hers (the leak, the sail, the reef, the lashing,
        /// the spar, the shares, the bundles, the tar), and the deck keeps no second copy.</summary>
        public bool Flagged(string name, double value, double days) => Boat.FlagSet(name, value, days, Today, BoatWatch.Worth());

        /// <summary>A card's order to the boat; what it did as a plain fact (placeholder wording).</summary>
        public string BoatOrder(string what, string word, double n)
        {
            switch (what)
            {
                case "hull":
                    Boat.HullBy(n);
                    return n < 0.0 ? "The hull is hurt: " + Boat.HullWord + "." : "The hull is the better for it: " + Boat.HullWord + ".";
                case "water":
                    Boat.WaterBy(n);
                    return n > 0.0 ? "Water comes aboard." : null;
                case "weary":
                    Boat.WearyBy(n);
                    return n > 0.0 ? "The men are the wearier for it." : "The men are rested.";
                case "lie_to":
                    Boat.LieTo(n);
                    return n > 0.0 ? $"She lies to for {n.ToString("0.#", CultureInfo.InvariantCulture)} hours." : null;
                case "sail":
                    Boat.SetCloth(word == "split" ? SailCloth.Split : word == "weak" ? SailCloth.Weak : SailCloth.Whole);
                    return "The sail: " + Boat.ClothWord + ".";
                case "set":
                    Boat.SetSail(word == "down" ? SailSet.Down : word == "reefed" ? SailSet.Reefed : SailSet.Full);
                    return "The sail is set: " + Boat.SetWord + ".";
                case "order":
                    Boat.SetOrder(word == "carry" ? GaleOrder.CarrySail : word == "reefed" ? GaleOrder.RunReefed : GaleOrder.LieTo);
                    return "The order for the gale: " + Boat.OrderWord + ".";
                case "shares":
                    Boat.Shares(word == "on", BoatWatch.Worth());
                    return word == "on" ? "The men sail on shares from today." : "The men are back on the wage.";
                case "tar":
                    Boat.Tar(Today);
                    return "She is tarred and tight.";
                case "shelter":
                    return BoatWatch.RunForShelter();
            }
            return null;
        }
```

### `Assets/Kapman/Code/Game/Voyage/Voyage.cs`

**9.12** line 240. Now:

```csharp
            PayOwed += Men * r.pay;
```

Becomes:

```csharp
            PayOwed += Men * r.pay * Boat.WageFactor;                    // (on shares: part of the wage, and a tenth of the gain at Martinmas)
```

**9.13** line 139. Now:

```csharp
        /// <summary>Men taken on (an event, a port).</summary>
        public static void Hire(int n) => Men += Math.Max(0, n);
```

Becomes:

```csharp
        /// <summary>Men taken on (an event, a port).</summary>
        public static void Hire(int n) => Men += Math.Max(0, n);

        /// <summary>Pay owed changed by so many deniers (the men's share of the season at Martinmas).</summary>
        public static void Owe(double deniers) => PayOwed = Math.Max(0.0, PayOwed + deniers);
```

### `Assets/Kapman/Code/Game/Trade/Hold.cs`

**9.14** line 28. Now:

```csharp
        public float Free => Math.Max(0f, capacity - Used);
```

Becomes:

```csharp
        /// <summary>Room taken by what is not cargo (kg): the water in her, the men's own bundles (Voyage/Boat.cs sets
        /// it each hour; not saved, it is the boat's).</summary>
        public float taken;
        public float Free => Math.Max(0f, capacity - taken - Used);
```

### `Assets/Kapman/Code/Game/Chart/ChartBody.cs`

**9.15** line 103. Now:

```csharp
                    double made = cls != null ? Travel.Speed(cls, Mathf.Repeat(want, 360f), WindFrom, Wind, out lying) : fallbackKnots * 0.5144;
```

Becomes:

```csharp
                    // (7 Oct: her speed is the world's ships' own rule through the boat she truly is: her sail, the
                    // water in her, her men; and her order for a gale. Voyage/Boat.cs)
                    double made = cls != null ? Boat.Speed(cls, Mathf.Repeat(want, 360f), WindFrom, Wind, out lying) : fallbackKnots * 0.5144;
```

**9.16** line 107. Now:

```csharp
                    if (lying)
                        motion.Say("A gale: she lies to.");
```

Becomes:

```csharp
                    if (lying)
                        motion.Say(Boat.IsLost ? "She is lost." : Wind > GaleDice.GaleWind ? "A gale: she lies to." : "She lies to.");
```

### `Assets/Kapman/Code/Game/Sim/WorldSim.cs`

**9.17** line 710. Now:

```csharp
                if (gale && s.rng.Chance(rules.galeLossPerHour * s.cls.galeFragility))
```

Becomes:

```csharp
                if (gale && s.rng.Chance(GaleDice.Chance(rules.galeLossPerHour, s.cls.galeFragility)))   // (the same dice as the merchant's: GaleDice)
```

### `Assets/Kapman/Code/Game/KapmanGame.cs`

**9.18** line 130. Now:

```csharp
                HourPassed?.Invoke(hour);
                var g = Current;
```

Becomes:

```csharp
                HourPassed?.Invoke(hour);
                BoatWatch.Hour(hour);                                      // (after the world's ships have had their hour: the same weather)
                var g = Current;
```

**9.19** line 138. Now:

```csharp
                DayPassed?.Invoke(day);
                GameEvents.Daily(day);
```

Becomes:

```csharp
                DayPassed?.Invoke(day);
                BoatWatch.Day(day);
                GameEvents.Daily(day);
```

**9.20** line 205. Now:

```csharp
            Voyage.Begin(voyage);
            float cargo
```

Becomes:

```csharp
            Voyage.Begin(voyage);
            Boat.Begin(BoatRules.Load(boatRules));                        // (her hull, her sail, the water in her, the men's shares)
            float cargo
```

**9.21** line 70. Now:

```csharp
        [Tooltip("The world's seed: the same seed plays the same world.")]
```

Becomes:

```csharp
        [Tooltip("Data/boat_rules.json: the boat's wear, leaks, bailing, weariness and her odds in a gale (none: the code's own numbers).")]
        public TextAsset boatRules;
        [Tooltip("The world's seed: the same seed plays the same world.")]
```

### `Assets/Kapman/Code/Game/SaveGame.cs`

**9.22** line 116. Now:

```csharp
                ["voyage"] = Voyage.Capture(),
```

Becomes:

```csharp
                ["voyage"] = Voyage.Capture(),
                ["boat"] = Boat.Capture(),                                    // 7 Oct: her hull, water, sail, order, weariness, shares
```

**9.23** line 192. Now:

```csharp
            Voyage.Restore(Json.Obj(data, "voyage"));
```

Becomes:

```csharp
            Voyage.Restore(Json.Obj(data, "voyage"));
            Boat.Restore(Json.Obj(data, "boat"));                             // (a save without it: a sound boat on a wage)
            BoatWatch.Loaded();
```

**9.24** line 67. Now:

```csharp
            if (Aground.Stage != AgroundStage.None)
                return "Not now: she's being got off the ground.";
```

Becomes:

```csharp
            if (Aground.Stage != AgroundStage.None)
                return "Not now: she's being got off the ground.";
            if (Boat.IsLost && game.Hours > 0.0)
                return "She is lost: load a saved game.";                   // (nothing may be written over the saves from before)
```

**Two other write-ups touch the same line of `ChartBody.cs`.** `Design/fixes/chart_tools.md` (the Oars button)
drafts line 103 as `Oars.Out ? Travel.OarSpeed(...) : Travel.Speed(...)`. With the boat in, keep the one call to
`Boat.Speed` and let the Oars button set the sail instead: `Boat.SetSail(Oars.Out ? SailSet.Down : SailSet.Full)`.
The sail down is the oars alone, slowed by weary and bailing men, and `Travel.OarSpeed` is not needed.
`Design/fixes/card_clock.md` (a pressing card holds the ship) changes line 74 (`float step = ...`): no clash.

**Why the interface gets two methods with bodies.** `IGameEffects` has one more implementer, the tests' `FakeGame`
(`Tests/EditMode/EffectsTests.cs:11`). With default bodies (C# 8; the project compiles at 9.0) it needs no change
and every encounter test stays as it is: for the fake, no flag is the boat's, so flags behave as before.

---

## 10. Tests

### 10.1 `Assets/Kapman/Tests/EditMode/BoatTests.cs` (new): 22 tests, all pass outside Unity

They need no scene and no asset. Run outside Unity by loading the compiled test and calling each `[Test]` after
`[SetUp]`; they should be run again in Unity's test runner with the rest.

```csharp
using System.Collections.Generic;
using Kapman.Encounters;
using Kapman.Game;
using Kapman.Game.Sim;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>The boat she truly is (Voyage/Boat.cs): the same dice as every hull, warned first, wounded before she is
    /// lost; her sail, her water and her men read by her speed; the cards' flags are the boat; shares; the save.</summary>
    public class BoatTests
    {
        static ShipClassDef Gislinge() => new ShipClassDef
        {
            id = "gislinge", length = 7.7f, beam = 2.2f, crew = 4, sailKn = 4.8f, maxKn = 6.5f, oarsKn = 3.5f, rowingHours = 6,
            closestToWind = 58, polar = 1, galeFragility = 0.6f, hasSail = true, sea = true, coast = true, river = true,
        };

        /// <summary>An hour at sea: a wind, and the hours to a gale (infinity: none).</summary>
        static BoatHour At(long hour, double wind, double galeIn = double.PositiveInfinity, double lossPerHour = 0.0004, double offWind = 150.0,
                           int men = 4, bool moored = false, bool river = false) => new BoatHour
        {
            hour = hour, wind = wind, offWind = offWind, underWay = !moored, moored = moored, river = river, night = false, galeIn = galeIn,
            men = men, hands = 4, length = 7.7, beam = 2.2, fragility = 0.6, lossPerHour = lossPerHour, seed = 793,
        };

        [SetUp]
        public void Fresh() => Boat.Begin(new BoatRules());

        // ------------------------------------------------------------------ the dice

        [Test]
        public void A_sound_boat_lying_to_has_the_chance_every_hull_has()
        {
            Assert.AreEqual(1.0, Boat.DiceFactor(false, 0.0), 1e-9, "sound, dry, fresh, full crew, lying to");
            Assert.AreEqual(0.0004 * 0.6, GaleDice.Chance(0.0004, 0.6, Boat.DiceFactor(false, 0.0)), 1e-12, "WorldSim's own figure for her class");
        }

        [Test]
        public void Her_choices_and_her_hurts_multiply_it()
        {
            Boat.SetOrder(GaleOrder.RunReefed);
            Assert.AreEqual(2.5, Boat.DiceFactor(false, 0.0), 1e-9);
            Boat.SetOrder(GaleOrder.CarrySail);
            Assert.AreEqual(8.0, Boat.DiceFactor(false, 0.0), 1e-9);
            Boat.Strake();                                                 // hull 0.55: strained
            Assert.AreEqual(16.0, Boat.DiceFactor(false, 0.0), 1e-9);
            Boat.SetOrder(GaleOrder.LieTo);
            Boat.Lash(0.0, 3.0);
            Assert.AreEqual(2.0 * 0.8, Boat.DiceFactor(false, 1.0), 1e-9, "lashed down for three days");
            Assert.AreEqual(2.0 * 0.8 * 0.1, Boat.DiceFactor(true, 1.0), 1e-9, "on a river");
        }

        [Test]
        public void The_throw_has_no_memory()
        {
            Assert.AreEqual(GaleDice.Roll(793, "player", 1234), GaleDice.Roll(793, "player", 1234));
            Assert.AreNotEqual(GaleDice.Roll(793, "player", 1234), GaleDice.Roll(793, "player", 1235));
            Assert.AreNotEqual(GaleDice.Roll(793, "player", 1234), GaleDice.Roll(794, "player", 1234));
        }

        // ------------------------------------------------------------------ warned first

        [Test]
        public void The_signs_come_in_order_and_once_each()
        {
            var signs = new List<int>();
            Boat.SignCame += (s, h) => signs.Add(s);
            for (int h = 0; h < 12; h++)
                Boat.Hour(At(h, 8.0, 12 - h));                             // a gale twelve hours off, coming on
            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, signs, "the swell at twelve hours, the sky at six, the last at one");
            Assert.AreEqual(3, Boat.Sign);
            Boat.Hour(At(12, 8.0));                                        // it passed her by
            Assert.AreEqual(0, Boat.Sign);
        }

        [Test]
        public void A_second_gale_on_the_heels_of_the_first_has_its_own_signs()
        {
            var signs = new List<int>();
            Boat.SignCame += (s, h) => signs.Add(s);
            Boat.Hour(At(0, 8.0, 1.0));                                    // the last sign of the first
            Boat.Hour(At(1, 20.0, 0.0));
            Boat.Hour(At(2, 20.0, 0.0));
            Boat.Hour(At(3, 9.0, 5.0));                                    // it is over, and another is five hours off
            CollectionAssert.AreEqual(new[] { 3, 2 }, signs, "the second gale is told afresh");
            Assert.IsFalse(Boat.FairlyWarned, "and has its own hour of warning");
            Boat.Hour(At(4, 9.0, 4.0));
            Assert.IsTrue(Boat.FairlyWarned);
        }

        [Test]
        public void No_dice_in_a_gale_she_was_not_warned_of()
        {
            // certain death by the dice (a chance of 1 an hour), and the gale is on her with no sign before it
            Boat.Hour(At(0, 20.0, 0.0, lossPerHour: 10.0));
            Assert.IsFalse(Boat.IsLost);
            Assert.Greater(Boat.Hull, 0.9, "the hour worked her hull and no more");
            Assert.AreEqual(3, Boat.Sign, "the sign came with it");
            // an hour later she has had her warning: the sea strikes, and wounds
            Boat.Hour(At(1, 20.0, 0.0, lossPerHour: 10.0));
            Assert.IsFalse(Boat.IsLost, "the first strike is a wound");
            Assert.LessOrEqual(Boat.Hull, 0.55);
        }

        [Test]
        public void A_strike_wounds_before_it_kills_lying_to()
        {
            Boat.Hour(At(0, 8.0, 3.0, lossPerHour: 10.0));                 // warned three hours ahead
            Boat.Hour(At(3, 20.0, 0.0, lossPerHour: 10.0));
            Assert.IsFalse(Boat.IsLost);
            Assert.AreEqual("a strake started", Boat.HullWord);
            string how = null;
            Boat.Lost += h => how = h;
            Boat.Hour(At(4, 20.0, 0.0, lossPerHour: 10.0));               // the sail is down: nothing left to take but the boat
            Assert.IsTrue(Boat.IsLost);
            Assert.IsNotNull(how);
            Assert.AreEqual(0, Boat.Hour(At(5, 20.0, 0.0)).Count, "nothing more happens to a lost boat");
        }

        [Test]
        public void Running_she_loses_the_sail_between_the_strake_and_the_boat()
        {
            Boat.SetOrder(GaleOrder.RunReefed);
            Boat.Hour(At(0, 8.0, 3.0, lossPerHour: 10.0));
            Boat.Hour(At(3, 20.0, 0.0, lossPerHour: 10.0));
            Assert.Less(Boat.Hull, 0.7);
            Assert.AreNotEqual(SailCloth.Split, Boat.Cloth);
            Boat.Hour(At(4, 20.0, 0.0, lossPerHour: 10.0));
            Assert.AreEqual(SailCloth.Split, Boat.Cloth);
            Assert.IsFalse(Boat.IsLost);
            Boat.Hour(At(5, 20.0, 0.0, lossPerHour: 10.0));
            Assert.IsTrue(Boat.IsLost);
        }

        [Test]
        public void No_dice_in_hours_the_clock_jumped_over()
        {
            Boat.Hour(At(0, 8.0, 3.0, lossPerHour: 10.0));
            Boat.Grace(6);                                                 // (the clock was jumped to hour 5)
            for (int h = 3; h < 6; h++)
                Boat.Hour(At(h, 20.0, 0.0, lossPerHour: 10.0));
            Assert.Greater(Boat.Hull, 0.9, "three hours of gale inside the jump: worked, not struck");
            Boat.Hour(At(6, 20.0, 0.0, lossPerHour: 10.0));
            Assert.Less(Boat.Hull, 0.7, "the first hour that was played");
        }

        [Test]
        public void Made_fast_she_is_out_of_it()
        {
            Boat.Hour(At(0, 8.0, 3.0, lossPerHour: 10.0));
            for (int h = 3; h < 40; h++)
                Boat.Hour(At(h, 25.0, 0.0, lossPerHour: 10.0, moored: true));
            Assert.IsFalse(Boat.IsLost);
            Assert.AreEqual(1.0, Boat.Hull, 1e-9);
            Assert.IsFalse(Boat.InGale);
        }

        // ------------------------------------------------------------------ her speed

        [Test]
        public void Shortened_sail_is_seven_tenths_and_a_split_one_half()
        {
            var c = Gislinge();
            double full = Boat.Speed(c, 90.0, 270.0, 8.0, out bool lying);  // before a fresh breeze
            Assert.IsFalse(lying);
            Assert.AreEqual(Travel.Speed(c, 90.0, 270.0, 8.0, out _), full, 1e-9, "as she was: the world's ships' own speed");
            Boat.SetSail(SailSet.Reefed);
            Assert.AreEqual(0.7 * full, Boat.Speed(c, 90.0, 270.0, 8.0, out _), 1e-9);
            Boat.SetSail(SailSet.Full);
            Boat.SetCloth(SailCloth.Split);
            Assert.AreEqual(0.5 * full, Boat.Speed(c, 90.0, 270.0, 8.0, out _), 1e-9);
            Boat.SetCloth(SailCloth.Weak);
            Assert.LessOrEqual(Boat.Speed(c, 90.0, 270.0, 14.0, out _), 0.8 * c.sailKn * SimUnits.KnotMs + 1e-9, "a weak sail is not driven");
            Boat.SetCloth(SailCloth.Whole);
            Boat.SetSail(SailSet.Down);
            Assert.AreEqual(c.oarsKn * 0.5 * SimUnits.KnotMs, Boat.Speed(c, 90.0, 270.0, 8.0, out _), 1e-9, "the sail down: the oars, a day's mean");
        }

        [Test]
        public void In_a_gale_she_lies_to_unless_ordered_to_run_with_the_wind_abaft()
        {
            var c = Gislinge();
            Assert.AreEqual(0.0, Boat.Speed(c, 90.0, 270.0, 20.0, out bool lying));
            Assert.IsTrue(lying);
            Boat.SetOrder(GaleOrder.RunReefed);
            Assert.AreEqual(0.7 * c.maxKn * SimUnits.KnotMs, Boat.Speed(c, 90.0, 270.0, 20.0, out lying), 1e-9);
            Assert.IsFalse(lying);
            Assert.AreEqual(0.0, Boat.Speed(c, 270.0, 270.0, 20.0, out lying), "into it: she cannot run");
            Assert.IsTrue(lying);
        }

        [Test]
        public void Water_and_weary_men_slow_her()
        {
            var c = Gislinge();
            Boat.Hour(At(0, 8.0));                                         // (sets what swamps her from her size)
            Boat.WaterBy(-1e9);
            double dry = Boat.Speed(c, 90.0, 270.0, 8.0, out _);
            Boat.WaterBy(Boat.Swamp * 0.5);
            Assert.AreEqual(0.7 * dry, Boat.Speed(c, 90.0, 270.0, 8.0, out _), 1e-9, "half swamped: three tenths off");
            Boat.WaterBy(-1e9);
            Boat.SetSail(SailSet.Down);
            Boat.WearyBy(-1.0);
            double fresh = Boat.Speed(c, 90.0, 270.0, 8.0, out _);
            Boat.WearyBy(1.0);
            Assert.AreEqual(0.5 * fresh, Boat.Speed(c, 90.0, 270.0, 8.0, out _), 1e-9, "spent men row at half");
        }

        // ------------------------------------------------------------------ water

        [Test]
        public void A_started_strake_keeps_a_man_bailing_and_a_sound_hull_none()
        {
            Boat.Hour(At(0, 6.0));
            Assert.AreEqual(0, Boat.Bailers);
            Boat.Strake();
            Boat.Hour(At(1, 6.0));
            Assert.AreEqual(1, Boat.Bailers);
            Assert.Less(Boat.Water, 100.0, "one man keeps it down");
            Assert.AreEqual(1.0, Boat.FlagView("sea_leak", 0.0));
        }

        /// <summary>An opened hull (0.4) in a gale of fourteen hours (nine gales in ten are shorter) with no dice at all:
        /// the hours she lasts, or -1.</summary>
        static int HoursAfloat(GaleOrder order, int changeAt = -1)
        {
            Boat.Begin(new BoatRules());
            Boat.HullBy(-0.6);
            Boat.SetOrder(order);
            Boat.Hour(At(0, 8.0, 2.0));
            for (int h = 2; h < 16; h++)
            {
                if (h - 2 == changeAt)
                    Boat.SetOrder(GaleOrder.LieTo);
                Boat.Hour(At(h, 22.0, 0.0, lossPerHour: 0.0));
                if (Boat.IsLost)
                    return h - 1;
            }
            return -1;
        }

        [Test]
        public void She_fills_when_the_men_cannot_keep_it_down()
        {
            int carried = HoursAfloat(GaleOrder.CarrySail);
            Assert.Greater(carried, 3, "all sail on an opened hull: she fills, and it takes hours");
            StringAssert.Contains("filled", Boat.LostHow);
            Assert.AreEqual(-1, HoursAfloat(GaleOrder.CarrySail, changeAt: 3), "the sail struck in the third hour: she lives");
            Assert.IsFalse(Boat.IsLost);
        }

        [Test]
        public void Four_fresh_men_keep_a_sound_boat_afloat_through_a_long_gale_lying_to()
        {
            Boat.Hour(At(0, 8.0, 2.0));
            for (int h = 2; h < 42; h++)                                   // forty hours: the longest the model gave in 13 years
                Boat.Hour(At(h, 22.0, 0.0, lossPerHour: 0.0));
            Assert.IsFalse(Boat.IsLost);
            Assert.Less(Boat.Water, 400.0);
            Assert.Greater(Boat.Hull, 0.6, "she has worked, and is not opened");
            Assert.Greater(Boat.Weary, 0.8, "and the men are spent");
        }

        // ------------------------------------------------------------------ the cards' flags are the boat

        [Test]
        public void The_flags_are_the_boat()
        {
            Assert.IsNull(Boat.FlagView("sea_vow_penny", 0.0), "not the boat's: the deck keeps it");
            Assert.IsFalse(Boat.FlagSet("sea_vow_penny", 1.0, 0.0, 0.0, 0.0));
            Assert.IsTrue(Boat.FlagSet("sea_leak", 1.0, 12.0, 0.0, 0.0));
            Assert.AreEqual(1.0, Boat.FlagView("sea_leak", 400.0), "a leak does not mend itself in twelve days");
            Boat.FlagSet("sea_leak", 0.0, 0.0, 0.0, 0.0);
            Assert.AreEqual(0.0, Boat.FlagView("sea_leak", 0.0));
            Assert.AreEqual(0.75, Boat.Hull, 1e-9, "patched, not sound");
            Boat.FlagSet("sea_sail_weak", 1.0, 20.0, 0.0, 0.0);
            Assert.AreEqual(SailCloth.Weak, Boat.Cloth);
            Boat.FlagSet("sea_reefed", 1.0, 2.0, 0.0, 0.0);
            Assert.AreEqual(SailSet.Reefed, Boat.EffectiveSet);
            Boat.Hour(At(48, 6.0));
            Assert.AreEqual(SailSet.Full, Boat.EffectiveSet, "the card's reef is shaken out after its two days");
            Boat.FlagSet("crew_boat_tarred", 1.0, 300.0, 10.0, 0.0);
            Assert.AreEqual(1.0, Boat.Hull, 1e-9);
            Assert.AreEqual(1.0, Boat.FlagView("crew_boat_tarred", 200.0));
            Assert.AreEqual(0.0, Boat.FlagView("crew_boat_tarred", 320.0));
        }

        [Test]
        public void A_card_reads_the_boat_through_its_flags()
        {
            var vars = new EncounterVars { state = new EncounterState(), flagSource = name => Boat.FlagView(name, 0.0) };
            Assert.AreEqual(0.0, vars.Num("flag.sea_leak"));
            Boat.Strake();
            Assert.AreEqual(1.0, vars.Num("flag.sea_leak"));
            vars.state.SetFlag("sea_vow_penny", 1.0, 0.0, 0.0);
            Assert.AreEqual(1.0, vars.Num("flag.sea_vow_penny"), "the deck's own flags are as they were");
        }

        [Test]
        public void The_boat_verb_parses_and_refuses()
        {
            string why = null;
            Assert.IsNotNull(Effects.ParseVerb("boat sail split", ref why));
            Assert.IsNotNull(Effects.ParseVerb("boat lie_to 6", ref why));
            Assert.IsNotNull(Effects.ParseVerb("boat hull -0.15", ref why));
            Assert.IsNotNull(Effects.ParseVerb("boat order carry", ref why));
            Assert.IsNotNull(Effects.ParseVerb("boat shelter", ref why));
            Assert.IsNull(Effects.ParseVerb("boat sail torn", ref why));
            Assert.IsNull(Effects.ParseVerb("boat hull", ref why));
            Assert.IsNull(Effects.ParseVerb("boat sink", ref why));
        }

        // ------------------------------------------------------------------ shares

        [Test]
        public void Shares_halve_the_wage_and_pay_a_tenth_of_the_gain_at_Martinmas()
        {
            Assert.AreEqual(1.0, Boat.WageFactor);
            Boat.Shares(true, 100.0);
            Assert.AreEqual(0.5, Boat.WageFactor);
            Assert.AreEqual(0.0, Boat.Martinmas(793, 11, 10, 300.0, out bool due));
            Assert.IsFalse(due);
            Assert.AreEqual(20.0, Boat.Martinmas(793, 11, 11, 300.0, out due), 1e-9, "a tenth of 200");
            Assert.IsTrue(due);
            Boat.Martinmas(793, 11, 12, 300.0, out due);
            Assert.IsFalse(due, "once a year");
            Assert.AreEqual(0.0, Boat.Martinmas(794, 11, 20, 250.0, out due), "a losing season pays nothing");
            Assert.IsTrue(due, "but it is still the reckoning");
        }

        // ------------------------------------------------------------------ the save, and every speed

        static string Run(int hours, int saveAt)
        {
            Boat.Begin(new BoatRules());
            Boat.SetOrder(GaleOrder.CarrySail);
            for (int h = 0; h < hours; h++)
            {
                // a made-up season: a breeze, with a gale of nine hours every fifty, each seen twelve hours ahead
                int k = h % 50;
                double wind = k >= 30 && k < 39 ? 21.0 : 7.0 + k % 5;
                double galeIn = k >= 30 && k < 39 ? 0.0 : k >= 18 && k < 30 ? 30 - k : double.PositiveInfinity;
                Boat.Hour(At(h, wind, galeIn, lossPerHour: 0.01));
                if (h == saveAt)
                    Boat.Restore((Dictionary<string, object>)Json.Parse(Json.Write(Boat.Capture())));
            }
            return Json.Write(Boat.Capture());
        }

        [Test]
        public void A_save_in_the_middle_changes_nothing()
        {
            string straight = Run(300, -1);
            Assert.AreEqual(straight, Run(300, 33), "saved and loaded in the middle of a gale");
            Assert.AreEqual(straight, Run(300, 120));
            Assert.AreEqual(straight, Run(300, -1), "and the same again from the same seed");
        }

        [Test]
        public void Water_and_bundles_take_room_in_the_hold()
        {
            var hold = new Hold { capacity = 660f };
            Assert.AreEqual(660f, hold.Free);
            Boat.Hour(At(0, 6.0));
            Boat.WaterBy(-1e9);
            Boat.WaterBy(200.0);
            Boat.SetBundles(true);
            hold.taken = (float)Boat.TakenKg;
            Assert.AreEqual(660f - 200f - 80f, hold.Free, 1e-3, "200 litres in her, and four men's bundles of 20 kg");
        }
    }
}
```

### 10.2 Tests that need Unity (not written: they need the scene, the tables or the clock)

| Test | Where | What it proves |
|---|---|---|
| `TheSameWorldWithAndWithoutGaleDice` | `SimTests.cs` | a year from seed 793 gives the same `stats.lost` and the same ship states as before the edit to `WorldSim.cs:710` (it must: same number, same dice) |
| `TheBoatComesBackFromASave` | `SaveTests.cs` | save with a started strake, a split sail, shares on, 300 l aboard; load; `Json.Write(Boat.Capture())` is equal; a save made before this change loads as a sound boat on a wage |
| `ALostBoatCannotBeSaved` | `AutosaveTests.cs` | after `Boat.Lost`, `SaveGame.Busy()` is not null and `Autosave.Now()` is false |
| `TheCardsLoadWithTheBoatVerb` | `EncounterTests.cs` (the loader's test over every card file) | once 7.2 is in, no card is refused |
| `ALeakFromACardIsTheBoat` | `EncounterChartTests.cs` | choose the last option of `sea.leak.seam` through the director; `Boat.HullWord` is no longer "sound"; `flag.sea_leak` reads from the boat |
| `SharesHalveTheWage` | `VoyageTests.cs` | `Boat.Shares(true, 0)`; `Voyage.Day(true, 0.5)`; pay owed is `men x pay x 0.5` |
| `SpeedOneAndSpeedFive` | the five-speed builder's test (ruling 26) | add `Boat.Capture()` to what is compared (4.4) |
| A play test | by hand, with a screenshot each | (1) cast off from Dorestat in a November of the seed with a gale within the day: the quay shows it; (2) at speed 5 at sea: the clock drops at the first sign, the card opens at the second; (3) carry sail through it: all hands bail, the water figure climbs, the Ship panel's fourth line turns; (4) let her fill: the last page, and F5 refused |

---

## 11. Order of work

Each step compiles and passes the tests by itself.

1. **The boat, dead.** Add `GaleDice.cs`, `Boat.cs`, `BoatTests.cs`. Edit `WorldSim.cs:710`. Nothing calls `Boat`
   yet. Run the tests (the sim's own tests must not move).
2. **Saved.** `SaveGame.cs` (three edits), `KapmanGame.cs` (`Boat.Begin`, the `boatRules` field), `Hold.cs`.
3. **Alive.** `BoatWatch.cs`; the two tick lines in `KapmanGame.cs`; `ChartBody.cs` (two edits); `Voyage.cs` (two
   edits). From here a gale wears her, fills her, warns and can take her. Play test 1 to 4 by the panel alone.
4. **The cards are the boat.** `IGameEffects.cs`, `Effects.cs`, `EncounterVars.cs`, `Cond.cs`,
   `EncounterGameVars.cs`, `GameEffects.cs`.
5. **Shown.** `ShipPanel.cs`, `CrewPanel.cs`; the brief faces when that step is in; the last page (6.4).
6. **Data, after the content pass:** the rows of 7.2 and 7.3; `boat_rules.json` if the user wants to tune.
7. **With the five speeds:** the two drop levels in `BoatWatch.OnSign`; the settings switch; the speed test.

Steps 1 to 3 are the critic's con 9 (a point). Step 4 is his con 10 (a point). He put the two together, with the
order's three switches, at +0.5 as planned.

---

## 12. Yours to rule

1. **What the crew do when the player does not answer** (1.7, 7.3). Proposed: the safe option, for the four sea
   cards named. The card rules' present "freeze, and the worst happens" was written when a gale cost only time.
   If the user keeps it, the game will sometimes lose a boat for a player who was not looking, at speed 4 or 5.
2. **The men's shares.** The card says "a part of the venture instead of a wage" and "a tenth of each season's
   gain". A wage is about 1 d a day for four men (`voyage_rules.json`: 0.25 a man), so about 200 d a sailing
   season; a tenth of the gain is far less than that at today's trade (the money study has a sensible trader
   making about 8 d an hour of play). So "a tenth instead of the wage" would be a gift to the player and the men
   would never ask for it. Drafted instead: **half the wage, and a tenth of the gain**. The other honest choices:
   a third of the gain and no wage; or the wage stays and the tenth is a bonus that only moves their heart.
3. **A number on the Ship panel** ("1 in 350"). The cards never show a number, by the user's earlier rule. The
   critic asks for "a loss I can price". Drafted: the word always, the number beside it. One call to delete if
   not wanted.
4. **Should the other ships be hurt before they are lost too?** Today they die on the first hit and never wear.
   Hers is wounded first. It is the critic's own wording, but it is not literally the same rule. Giving the 139
   others a hull number would cost a field and one line each hour; it would also change every existing run's
   losses. Not drafted.
5. **The table's chance.** At 0.0004 an hour the dice alone are a small summer risk (1.6). Raising
   `galeLossPerHour` raises the whole fleet's losses, and the critic's con 14 is that the fleet can only shrink.
   Left as it is; her risk is tuned in `BoatRules`.

---

## 13. Not checked

- **Nothing was run in Unity.** The compile was done outside it with Unity's compiler and the project's reference
  list; the 22 tests were run outside it. The play tests of 10.2 are not done.
- **Which weather model L_Chart runs.** `KapmanGame.MakeWorld` takes `LocalWeather.Active.Model` if there is one,
  else `new WeatherModel(793)`. The figures of 1.6 are from `WeatherModel(793)` by itself. If the chart scene has a
  local model with another seed the counts differ a little; the rules do not.
- **What 13 samples of the weather an hour cost** (`BoatWatch.GaleIn`). At speed 5 that is about 78 samples a real
  second. The world's 139 ships already take one each an hour with a shared snapshot; `Sample(lat, lon, hours,
  overSea)` makes its own snapshot each call, which I did not time. If it shows in the profiler, take one
  `weather.Snapshot` for each of the 12 hours and reuse it.
- **`Course.Plan` to every port within 150 km** in `RunForShelter`: the calls compile; whether the nearest port by
  water is always a sensible lee (a port up a river she cannot reach in time) is not checked. `Course.Plan` with
  `fromPort` null from the open sea is used the same way by `Course.SetTo`.
- **That an offered card opens within the hour.** `EncounterDirector.Offer` puts it first in line; I did not read
  how soon the runner opens it when another card is open and let run. If it waits, the standing order holds
  ("lie to" unless she changed it), which is the safe one.
- **The 3D levels.** `BoatWatch.Hour` returns at once without `ChartBody.Current`, so in a 3D level the boat's
  state stands still and `Bilge` rules there as before. The two are not joined.
- **Grounding on the chart.** `Aground.cs` and `Ships/Grounding.cs` were not read for whether she can take the
  ground on the chart; if she can, `Boat.Strake()` belongs there too.
- **The card rules' other defaults.** Only the four sea cards of 1.7 were read. Other pressing cards may have the
  same "worst by default" rule where it now costs more than time.
- **`Rest` and `TideWait`** raise the clock's own rate (an hour a second). The hour tick still comes for each
  hour, so the boat is right under them; I did not check what they do to a sign's clock drop.
- **By mistake** one read-only `git log -1` was run while reading (it changed nothing). No other git was used.
