# The voyage itself: making a leg a decision

6 October 2026. For the ask "more in depth and thoughtful". Read: the voyage code (`Voyage`, `Sim`, `Chart`), the
concept's five beats, `weather_and_sea_state.md` section 3, `ui_direction.md`, the third mockup. Web search was shut
for this session, so the game findings rest on pages fetched by name and on memory; **(?)** marks what is not checked.
Every number is a first guess to tune. Nothing was run in Unity.

## 1. What is there now

She sails the charted line by herself at a real speed (`Travel.Speed`): 4.8 knots on a beam reach in 8 m/s, about
1.8 knots made good when beating (she lies no closer than 58 degrees), 1.75 knots as a day's mean under oars, and she
lies to above 17 m/s. Dorestad to London (about 350 km) is 2 days with a fair wind and 4 to 5 against it. **The wind
already makes a threefold difference, and the player has no say in it.** That is the gap.

The world's own ships decide more than the player does: they wait up to 4 days for a wind that serves a crossing
(`WorldSim.FairWind`), and the small coasters lie up from the 21st hour to the 5th. The player has `TideWait`, `Rest`
(a key that speeds the night), `Aground` (now a plain reset), 63 sea cards, and two numbers for all four men.

## 2. Findings

### 2.1 Games: travel is a choice when the price is seen first and the order stands

| Game | What makes its travel a choice | For Kapman |
|---|---|---|
| **80 Days** | The team first showed nearly every route; players skipped the cities and "the market felt like a chore". They then "locked all the routes on the globe", and a route became a thing you earn ("Met a captain? Have a fishing boat."). | A way through a water is learned, not given. |
| **FTL** | A jump costs fuel and moves the rebel fleet on. The map shows a little before you jump: a hazard, a store. | The debt's day and the closing sea are our fleet. Show a little of each leg: the bar, the night, the wind. |
| **The Oregon Trail (1985)** | Pace, rations and rest are standing orders. A river crossing is one screen (ford, float, ferry) with the depth and the weather in the rule. Bouchard wanted the fun to "arise from immersing the player in a historically accurate experience". | Standing orders, not steering. The bar is our river crossing. |
| **Curious Expedition** | The cost of a move is drawn on the path before you click (?). | The day stick's paler stretch. Add the risk words. |
| **Sunless Sea** | The lamp off saves fuel and raises terror (?). The slow ship: tension, and dead time. | Night sailing is our lamp. |
| **Sailwind** | No mark on the map: "Observe the sun and stars, chart your position on the map." | Decision 11's fuzzy place. Not knowing is the game offshore. |
| **HighFleet, Banner Saga, Out There** | Few numbers, hard sums: fuel, heart, being found; rest mends heart and eats stores (?). | Keep one fuel: days. Lying up is our rest; company answers being found. |
| **Dredge** (`research/fleets/one_ship.md`) | "Time became the resource." Players love "trying to get home for the night". | Dusk must be a moment on every coastal leg. |

Three rules come out. **Show the price before the choice. Let the player give an order that stands, and break in
only when the facts change. Make knowledge of a water something the house earns.**

### 2.2 How a skipper of 800 decided

- **He waited for a wind.** Ohthere (told to King Alfred about 890) "waited for a west wind", then for a north wind.
- **He chose between camping and sailing the night.** Ohthere: Halogaland to Sciringes heal could not be sailed in a
  month "if one camped at night" (`on niht wicode`). Wulfstan went from Hedeby to Truso "in seven days and nights",
  the ship "running under sail all the way". Camp along a coast; sail through on open water.
- **He knew the way from men, not a chart.** "Prior familiarity with tides, sailing times, and landmarks." Lost in
  fog had its own word, `hafvilla`.
- **He could sound.** Roman leads survive and the Bayeux Tapestry shows one cast in 1066. Tallow in the lead's foot
  brings up sand, shell or clay, which tells a man where he is. In a Frisian boat of 800: likely, not proven (?).
- **He drew little water and could take the ground.** A metre was enough, and a beach was a harbour. The Frisian way
  inside the islands, over the flats on the tide, follows from this (?).
- **He kept a season.** Vegetius: safe from 27 May to 14 September, shut from 11 November to 10 March (?).
- **He sailed in company when he feared men.** Anskar, bound for Birka about 829 with merchants, was taken by pirates;
  the merchants fought and lost nearly all (Rimbert, ch. 10) (?).
- **He was slow.** A knarr made about 120 km in a day. The Sea Stallion took 44 days from Roskilde to Dublin in 2007.

## 3. Proposals

A leg is three parts: **the word** (before), **the watch** (during), **the landfall** (after). The player never
steers. He gives the word, and is asked again only when the sea changes.

### 3.1 The word: one sentence with three holes

**Rule.** "Set course" opens one sentence, shaped like the fleet orders in `ui_direction.md`: *Cast off* **[now / on
the ebb / at first light / when the wind serves]**, *go* **[inside / by the coast / straight across]**, *and at dusk*
**[lie up / sail on]**. The last word given for a port is kept.

**Numbers.** Each way gets a short and a long count of days: short with the wind as it blows, long with it foul
(1.8 knots) on the sea part. Inside is 10 to 30% longer and needs the tide. Straight across is shortest and has
nowhere to hide.

**The player sees.** Three dotted lines, the chosen one inked. The stick's paler stretch moves as a hole changes.
Under the sentence, at most three risk words: "The bar on the ebb. Two nights. No landing for 90 km."

**Build.** `Course.Plan` run with three `LaneRouter.Profile`s (`offFlats` exists; add `coastOnly`, refusing `Sea`
legs over 40 km, and `direct`). A `PassagePlan` (line, kinds, the two counts, the risks). `Course.Days` returns a pair.

### 3.2 A wind that serves

**Rule.** "When the wind serves" holds her in port until the world ships' own test passes for her: no gale, and the
sea part lies 20 degrees free of her closest point; checked every 6 hours. The clock runs. A day costs 1 d in wages,
bread, a day off the debt, and a day off every price word and fresh good.

**Numbers.** Beating to London costs about 2.5 days more than a fair wind, so one or two days of waiting pay and a
third does not. The weather-wise man reads the sky when asked: "It will back south by morning. Likely." He is right
60% of the time unskilled, 90% skilled.

**The player sees.** The wind arrow by her mark, and what it means: "North-west. Foul for Lundenwic, fair for Ribe."
While he chooses, the chart writes "fair" or "foul" beside each port he knows: a foul wind is a reason to trade
somewhere else.

**Build.** `FairWind` made public for `WorldSim.Player`. A `WindWait` shaped like `TideWait`. `Travel.Speed` for each
known port (50 calls, only while choosing).

### 3.3 Dusk

**Rule.** At dusk (`Rest.Dark`) the standing order acts. **Lie up:** she makes for the nearest landing she knows
within 8 km and stays till first light; heart +0.02; a shore card may come. With no landing she anchors off and heart
does not mend. **Sail on:** free on `Sea` legs. On `Coastal`, `Sound`, `Flats` and `Approach` legs each dark hour may
touch ground: 2% with no moon or under cloud, 0.5% under a moon over a third lit, a quarter of either in a water she
has a pilot for. From the third night running, heart falls 0.02 a night.

**Numbers.** At 52 degrees north the dark lasts about 5 hours at midsummer and 13 in late October (?). The same order
costs a fifth of the day in June and half in autumn. The season is felt with no extra rule.

**The player sees.** Dark bars on the stick's stretch for the leg's nights. Known landings as small hollow marks.
Before dusk the helmsman speaks once: "Dark in an hour. The beach under Sheppey is near." One key changes the order.

**Build.** `Dusk` replaces `Rest`'s key. Landings: ports, plus points baked every 15 to 25 km on `Coastal` lanes
where the shore is beach. `Knowledge` gains "landing known". A seeded `VoyageRisks.Hour()`, so a reload repeats the roll.

### 3.4 The gate: tide, stream and bar

**Rule.** River mouths, the inlets between the islands and the flats lanes are **gates**. A gate has a stream and may
have a bar. With the stream she gains its speed; against it she loses it. On a bar, wind of 8 m/s or more against
the ebb makes a breaking sea.

**Numbers.** Stream 1 to 1.5 knots at springs, two thirds at neaps. Against 1.5 knots a boat rowing at 1.75 stands
still, so the right tide is worth half a day. A breaking bar: 10% a lot of cargo lost, 3% a man hurt ("doubtful").
Her 0.25 m draught means depth rarely stops the first boat; a knarr drawing a metre is stopped, so the gate grows
teeth as the house grows.

**The player sees.** A wedge at each gate: filled on the flood, hollow on the ebb; pointed at, "the ebb runs till the
ninth hour". At a gate at the wrong time, one card (`sea.bar.ebb` exists): wait for slack (hours on the stick), cross
now (odds as a word), go round (days). "On the ebb" in the word times the cast-off so she meets the gate right.

**Build.** `gates_793.json` (lane ids, place, stream at springs, bar or not). `VoyageTide` is tied to one 3D level:
give it `At(lat, lon, hours)` from the same `TideModel` for the chart scene. `ChartBody` adds the stream. `TideWait`
waits for a stream as well as a depth.

### 3.5 The blot and the lead

**Rule (decision 11 made concrete).** Out of sight of land her mark is a blot. Its radius grows by 8% of the distance
run since the last fix, doubled when neither Sun nor stars show, halved by a skilled helmsman. A fix is a landmark
seen or a sounding matched. The true place is inside the blot; the landfall shows where.

**Numbers.** The Rhine mouth to the Thames is about 200 km of open water: a blot of 16 km when clear, 32 in murk. Low
coast shows at 8 km, high land at 30 to 40. A wrong landfall costs 2 to 10 hours. "Keep the lead going" (under 40 m):
way falls a tenth, touching ground falls to a quarter, and a bottom the house has noted before halves the blot.

**The player sees.** The mark spreads, then snaps small at a fix: "Grey sand and shell. Ubbo says the Galloper." A
landfall card: what is seen, who thinks what, how sure. In the window a man swings the lead.

**Build.** `Reckoning`: the true place and a shown place with a radius; the fog lifts only round a fixed place. Three
or four sea-bed kinds on the lanes (?). About 40 named headlands with heights.

### 3.6 Company

**Rule.** The port slip names ships bound the same way within two days. "Sail with them" joins them for that leg: she
goes at the slowest pace and leaves when they leave. One leg's consort, not a convoy to manage (`one_ship.md`).

**Numbers.** Raiders close on three or more ships a third as often (to tune in `RaiderBrain`). The price: 0 to 2
days' wait, a tenth to a fifth slower, and each ship in ahead of you with your good takes 5 to 8% off its price.
Alone is first to market.

**The player sees.** "Two Frisians cast off for Lundenwic on the morning ebb. Wine and querns." Their marks sail
beside hers.

**Build.** A `WorldSim` query for ships in port with the same next port; a `company` id on `SimShip`; `ChartBody`
takes the group's least speed; `RaiderBrain` reads its size.

### 3.7 Weather: wounds before wreck, and a way out

**Rule.** When the storm number passes 0.1 on her next 12 hours (the weather sampled ahead along the line), a man
reads the sign. The player answers once: hold on, shorten sail, or make for shelter (the nearest known landing; the
course is taken up after).

**Numbers.** Shortened: way times 0.7, harm halved. In a gale at sea each hour: 6% a lot goes over the side (the
player picks which), 2% the sail or a strake goes (way times 0.6 until mended), 0.5% a man is lost. The boat is lost
only when already hurt with a lee shore within 10 km: 2% an hour.

**The player sees.** The cards `sea.gale.breaks` and `sea.squall.line`, with the shelter ringed and its hours on the
stick.

**Build.** `WeatherAt` with a time offset. The rolls in `VoyageRisks`. A `damage` state read by `Travel.Speed`. A
detour in `Course` (push the line, go, pop it).

### 3.8 What the crew adds

**Rule.** Each of the four men has one sea craft and one seat. The **helmsman** halves the blot. The **lookout**
sees sails and land a third further. The **leadsman** knows waters: each `unlock_pilot_<water>` flag is his. The
**weather-wise** man reads signs. Every sign, sounding and landfall is said by its man, with his sureness as a word.
A lost man takes his craft with him: the seat is empty and its line is not spoken.

**Numbers.** Craft 0 to 3; each step is 10 points of rightness. One number for the boat, **weary**: +0.1 for a night
sailed, +0.1 for a day of over 6 hours at the oars, +0.15 for a gale; -0.3 for a night ashore. Over 0.6 the oars give
0.7 of their speed and night risk is half as much again. A man overruled three times in his own craft is uneasy.

**Build.** Four `CrewMan` records in place of `Voyage.Men` as a count; `Health` and `Morale` stay as the boat's sums.
`weary` in `Voyage.Day`. A speaker and a sureness on `Voyage.Say`.

### 3.9 The landfall and the log

**Rule.** On mooring, one line is written from the leg's facts, and the water is better known.

**Numbers.** After three passages a water is known: its long count shrinks by a quarter, its landings and gate hours
are inked on the chart, night risk is halved. The heir inherits the ink.

**The player sees.** "Dorestat to Lundenwic. Four days; I had thought three. Lay a night under Sheppey for the ebb. A
cask lost on the bar. 38 d clear after bread and wages." The plan beside the outcome teaches the next word.

**Build.** A `LegLog` fed by `Voyage.Notice`, `VoyageRisks` and the cards; `PassageKnowledge` per water, saved with
the house.

## 4. No new meter, and the order to build in

Everything above is a mark on the chart (arrow, wedge, blot, landing, ring), a bar on the day stick, or a thing seen
in the ship window (the sail's state, the lantern, the lead, the bailing, the company's hulls, a man slumped when
weary). The window's caption line carries one sentence of the watch.

Order: the word, the two day counts and the wind wait (two nights); dusk and `VoyageRisks` (two); gates (two);
weather answers and wounds (two); crew crafts, weary, the log and known waters (three); the blot, the lead and
company last (three), because they touch the fog of war and the fleet. Each step has one headless test: a foul wind
lengthens the long count; a seeded night roll repeats after a reload; with the ebb she gains the table's stream.

## 5. Yours to decide

1. Three holes in the word from the start, or only "when" and "dusk" at first?
2. May the seaman's reading be wrong? (The weather document says yes.)
3. Touching ground at night: a card each time, or a plain fact and an hour lost?
4. The blot: from the first boat, or only once she crosses to Britain?

## Sources

- Fetched: 80 Days postmortem https://www.gamedeveloper.com/business/postmortem-inkle-s-i-80-days-i- ; FTL https://en.wikipedia.org/wiki/FTL:_Faster_Than_Light ; Oregon Trail https://en.wikipedia.org/wiki/The_Oregon_Trail_(1985_video_game) ; Sailwind https://store.steampowered.com/app/1764530/Sailwind/
- Fetched: Ohthere https://en.wikipedia.org/wiki/Ohthere_of_H%C3%A5logaland ; Wulfstan https://en.wikipedia.org/wiki/Wulfstan_of_Hedeby ; https://en.wikipedia.org/wiki/Viking_ships ; the lead https://en.wikipedia.org/wiki/Depth_sounding ; Sea Stallion https://en.wikipedia.org/wiki/Havhingsten_fra_Glendalough
- From memory (?): Sunless Sea, HighFleet, Curious Expedition, Banner Saga, Out There (their Wikipedia pages say little on travel); Vegetius; Rimbert ch. 10
- In the project: `Design/research/fleets/one_ship.md` (Dredge); `Sim/Travel.cs`, `Sim/WorldSim.cs`, `Voyage/TideWait.cs`, `Rest.cs`, `Aground.cs`, `Voyage.cs`, `Chart/ChartBody.cs`, `Data/ship_classes_793.json`, `Data/encounters/sea_793.json`
