# Chain: the hoard at Aikgard (`dirham_hoard`)

Cards: `Assets/Kapman/Game/Data/encounters/chain_dirham_hoard_793.json` (14 cards).
Words: `Assets/Kapman/Game/Data/writing/chain_dirham_hoard.json` (203 entries: every key, an outcome text for every
option, six news items, an `art.<card id>` line for every card).
Written 6 Oct 2026. A draft for the user to overwrite. The checker is clean on both files.

## The story in five sentences

A Gotland trader dies of a fever in a western harbour with none of his own people near, and tells a stranger with a
boat where his silver lies: twelve feet from his storehouse door toward the tall stone, under the floor. The merchant
carries the word to Gotland, where the brother who holds the farm and the daughter who is the dead man's only child
each know half of it and each claim all of it. The floor gives up fifteen marks, the dead man had spoken of forty
less one, and the quarrel goes to the thing, where the Frisian is the only witness to what was said. The rest,
twenty-four marks, lies under the stern stone of an old ship of stones in the pasture, and only the Frisian knows it
(if he gave the dying man his hand, or kept and read his rune stick). He can give it to the daughter, give it to the
brother, leave it where it lies for ever, dig it himself by night, or bring the whole family to put it into a ship.

## How a player meets it

At Sliesthorp, Ripa, Birka or Dorestad, in the sailing months (May to October) of any year from 805 to 949, on any
call after the first there: card s01, weight 0.6 at four ports, so within a few summers of ordinary trading in the
north. It needs no silver (hearing him out is free). After that the chain waits at Paviken on Gotland: s04 comes up
almost surely (weight 4) on the first call there between May and September.

## The real history under it

- **The hoards.** More than 700 silver hoards have been found on Gotland, more than anywhere else in the north; the
  Spillings hoard alone held 67 kg in two lots, buried after 870 under the floor of a building, with arm rings made
  to a weight of about 100 g. Most Gotland hoards lie on farms, many inside the footings of houses
  (`peoples/svea_gotlandic.md` 5; `economy/silver_coin.md`; `riddles_793.json` rw_hoard_spillings; M. Ostergren,
  *Mellan stengrund och stenhus*, 1989, from memory (?)). Every one of them is a hoard nobody came back for.
- **The silver.** The Saracens' coin reaches the Baltic from about 800 (T. S. Noonan 1998, as cited in
  `goods_793.json`; `chronicle_793.json` 805-peterhof-hoard) and stops soon after 1000 (`chronicle_793.json`
  1000-silver-fades). In the north it is weighed, cut and pecked, never counted: mark = 8 ounces (ore) = 24 ortugar.
  The chain reckons a mark at about 204 g, so about 120 of Charles's pennies (?).
- **Why a man buried it, and why it stayed.** The practice the game models: `npc_text_encounters.md` 1.5. That a
  man has in the next world what he himself put in the ground is Snorri's telling of Odin's law (Ynglinga saga c. 8,
  tr. Laing 1844): written about 1230, **late**, and used here only as a thing a Norse hand says "they say" (?).
  Egil hiding his silver with two thralls (Egils saga, **late**) is the same story.
- **The island.** Farmers who are also the merchants, no town, no king, their own law; beach harbours such as
  Paviken with shipwrights and smiths (`ports_793.json`; `realms/sweden_gotland.json`; Guta saga, **late**).
- **The law of the quarrel.** Sons inherit first, daughters where there are no sons, and land does not leave the
  kin: Guta Lag, written about 1220, **late**, used for custom only (?). Wergild of a Gotlander 24 marks of silver,
  of an outlander 10 (Guta Lag 15, **late**): Vivil's line at s09 and the danger at s13. The thing decides, not a
  king (Rimbert, *Vita Anskarii* c. 26 to 27, of the Svear). Gotland's all-thing and its judges are attested only in
  the 13th-century law, so the cards say only "the thing" and "an old man who knows the law by heart" (?).
- **Women with silver.** Frideburg and Catla of Birka (Rimbert c. 20); women who raise stones and hold farms (U 29,
  Vs 24); scales in women's graves at Birka (?).
- **The ship of stones.** Ship settings of standing stones are common on Gotland and were already ancient (Bronze
  Age: Gannarve, Tjelvar's grave). That a hoard was laid in one is invented.
- **The pyre.** The burning of the dead as the custom of the Svear (Heimskringla, preface, tr. Laing 1844,
  **late**); both burning and earth burial on Gotland (?). The verse at the fire is Havamal 77 (Bellows 1923), given
  as the text `havamal_077`, not written out.
- **Strangers on Gotland.** No foreign merchants are known to have held ground on the island before the Germans of
  the 12th century (Henry the Lion's peace with the Gotlanders at Artlenburg, 1161, from memory (?)).

**Invented, and marked so in the cards' notes:** every person (Hegbjorn, Vivil, Ailikn, Ottar, the boy Liknat, the
Svea rower Torkel; names from G 280, G 111 (?), G 113, U 379); the farm Aikgard; the sizes of both lots (15 and 24
marks: a rich farm's hoard, not Spillings); the rune stick; a Gotlander as far west as Dorestad (?). None of the
spine's cast fits; Ravn and Aldger (`cast.crew.ravn`, `cast.crew.aldger`) speak on the crew card and at the stones.
Nobody says "dirham": it is the writing silver. No web search was made; everything is from the project's own
research files and from memory, and what could not be checked is marked `(?)`.

## The steps

Progress flags are `ch_dirham_hoard_1` to `_9`. All flags below carry the prefix `ch_dirham_hoard_` unless shown whole.

| # | Card id | Where and when | The choices | Flags set |
|---|---|---|---|---|
| 1 | `harbour.chain_dirham_hoard.s01_dying_gute` | Sliesthorp, Ripa, Birka or Dorestad, May-Oct, 805-949 | give him your hand (free; he then tells the second lot) / pay for his care (12 d) / hear him out (free) | `_1`; swear: `oath`, `full`; care: `cared` |
| 2 | `harbour.chain_dirham_hoard.s02_ashes` | the same harbour, three days later (scheduled; skipped if the house has sailed) | pay lodging and pyre, take the chest (20 d) / pay lodging, take the chest (8 d) / leave the chest (free) | `_2`; `stick`, `chest`; pyre: `rites`; leave: `stick_lost` |
| 3 | `crew.chain_dirham_hoard.s03_rune_stick` | at sea, with the stick aboard (a side step) | have it read aloud / spell it out alone (even odds) / carry it unread (free) | `_3`; aloud: `full`, `crew_knows`; alone, success: `full` |
| 4 | `harbour.chain_dirham_hoard.s04_beach` | Paviken, May-Sept, first call after step 1 | to the brother first / to the daughter first / a day of ale and law on the beach (6 d) | `_4`; `first_brother`, `first_daughter` or `knows_law` |
| 5 | `harbour.chain_dirham_hoard.s05_farm` | Paviken, a day or two later | tell the daughter alone / tell the brother alone (not if sworn) / say it aloud to both | `_5`; `told_daughter`, `told_brother` or `told_both` |
| 6 | `harbour.chain_dirham_hoard.s06_floor` | Paviken, two days later | weigh it aloud (needs the dead man's scale or `sv_scale`) / ask an earnest (likely) / stand in the door (free) | `_6`; timers `wait_thing` (40 days) and `sea_window` (40 days); `weighed` or `took_earnest` |
| 7 | `sea.chain_dirham_hoard.s07_rower` | at sea within 40 days of step 6 (a side step) | pay the dead man's rower his wages (60 d) / heave to and show the hold (likely) / run (likely) | `_7`; `rower_paid`, `rower_owed` or `rower_enemy` |
| 8 | `harbour.chain_dirham_hoard.s08_thing` | Paviken, June-Aug, after `wait_thing`: mostly the next summer | say every word / speak for the daughter / speak for the brother (not if sworn) | `_8`; `spoke_true`, `spoke_daughter` or `spoke_brother` |
| 9 | `harbour.chain_dirham_hoard.s09_stone_ship` | Paviken, the next day | tell the daughter / tell the brother (not if sworn) / tell all three and propose a ship (only after `spoke_true`) / say nothing (free) | `_9`; `road_daughter`, `road_brother`, `road_fellowship` (and timer `wait_ship`, 400 days) or `road_buried` |
| 10 | `harbour.chain_dirham_hoard.s10_end_daughter` | Paviken, two days later | take the third / take half of it and send the rest to the uncle | **`end_daughter`**, `done`; `brother_bitter` or `peace` |
| 11 | `harbour.chain_dirham_hoard.s11_end_brother` | Paviken, two days later | take the pay / take it and plead for the niece (doubtful) | **`end_brother`**, `done`; `daughter_wronged` or `peace` |
| 12 | `harbour.chain_dirham_hoard.s12_end_buried` | Paviken, the same evening | leave it / leave it and give the stick to the grandson (needs the stick) / send for a spade (needs `full`) | **`end_buried`**, `done`; stick: `boy_has_stick`; dig: `spade`, then s13 |
| 13 | `harbour.chain_dirham_hoard.s13_end_thief` | Paviken, that night | take all (likely) / take a third and cover the rest / fill the hole | **`end_thief`**, `done`, and `feud` or `secret`; or **`end_buried`** |
| 14 | `harbour.chain_dirham_hoard.s14_end_fellowship` | Paviken, Aug-Oct, after `wait_ship` | take the share and swear again / leave the share in the ship / take the share and swear nothing | **`end_fellowship`**, `done`, `changed_gotland_hoard_spent`, `changed_frisians_on_gotland`; ride: `share_rides` |

**No dead ends.** Every card has an option that costs nothing. Steps 2, 3 and 7 are side steps the chain never
waits for. A house that swore nothing and kept no stick never learns of the second lot: for it step 9 has one
option, and the end is `end_buried`, with the finale paid from the first lot. Failing to read the stick alone costs
nothing, because the stick can still be handed to people who read. The rower is told of twice (the host at s02, the
shipwright at s04) before he appears; the dogs and the island's way with a stranger found digging are said on s12
before the spade is sent for. The scheduled cards (s05, s06, s09, s10, s11) also carry a weight, so a house that
sails away meets them on its next call at Paviken. A man who gave his hand at step 1 finds the brother's options
greyed at steps 5, 8 and 9.

## The endings

| Ending | Flag | How | What becomes of them | History |
|---|---|---|---|---|
| Her father's daughter | `ch_dirham_hoard_end_daughter` | s10 | Ailikn has twenty-four marks, buys a shore strip and half a ship, and keeps the Frisian house's goods. Vivil digs up half his pasture for nothing, unless she is talked into carrying him a share. | Invented, and true to the time: a woman with silver of her own. |
| The farm's silver | `ch_dirham_hoard_end_brother` | s11 | Vivil pays for the carrying and lays the rest back in the ground in a place he tells nobody. He lives eleven more winters. | **As it really went:** the silver changes holes and is lost with its owner. |
| The stern stone | `ch_dirham_hoard_end_buried` | s12, or s13's last option | The second lot stays an arm deep on the seaward side of the stern stone. The kin search the farm in vain. On the honest option the dead man's stick is given unread to his grandson of four. | **As it really went:** one of the 700. |
| An arm deep | `ch_dirham_hoard_end_thief` | s13 | The Frisian digs by night. Either the island knows by morning whose spade it was, or nobody knows but his own crew. | Invented; some hoards must have gone this way. |
| Silver that floats | `ch_dirham_hoard_end_fellowship` | s14 | Both lots build a ship at Paviken: Ottar steers, Ailikn tallies, Vivil grumbles and leaves the farm's share in, and a Frisian booth stands on the beach. | **History changed:** see below. |

**What follows from the changes** (for later cards to read):

- `changed_gotland_hoard_spent`: one of Gotland's hoards was never laid down. A small change, but a visible one:
  later cards can show Ottar's ship on the east road and at Birka, her sons' ships after her, and Aikgard as a
  house that trades. When the Saracens' silver fails about 1000 the family has ships and partners, not a hole.
- `changed_frisians_on_gotland`: a western house with ground on a Gotland beach some three hundred years before
  any outland merchant is known there. Later cards can give the heirs a booth and winter berth at Paviken, a say
  when the harbour silts and the beach moves (about 1000 to 1050), and a grievance when Olaf Haraldsson takes
  tribute of the island about 1007 (`realms/sweden_gotland.json`) and counts the Frisian booth as Gotland's.
- `ch_dirham_hoard_feud` (a lasting cost, end_thief 'all'): news is posted at Paviken; no Gotlander deals with the
  house, at home or at Birka, and a Frisian's life there is priced at ten marks.
- `ch_dirham_hoard_secret` (a lasting cost, end_thief 'third'): three of the crew know. A crew card can raise it
  when the pay is late.
- With `ch_dirham_hoard_oath` and `ch_dirham_hoard_end_thief` together the house is an oath-breaker to a dead man:
  the Norse hands remember it (his line at s13).
- `ch_dirham_hoard_daughter_wronged`, `ch_dirham_hoard_brother_bitter`: one of the two will not speak to the house.
- `ch_dirham_hoard_boy_has_stick`: about fifteen years on, Liknat can read. A later card may let him.
- `ch_dirham_hoard_share_rides`: a third share left in the ship, for a later card to pay out.

## Payouts

A mark is reckoned at 120 d.

| Step | Option | Silver | And |
|---|---|---|---|
| 1 | swear | 0 | the second measure (`full`) |
| 1 | care | -12 | the deed `gave_alms`; the host's feeling |
| 2 | pyre | -20 | the text `havamal_077`; the crew's heart; the scale and the stick |
| 2 | chest | -8 | the scale and the stick |
| 3 | aloud / alone | 0 | runes +0.1 to +0.2; the second measure |
| 4 | ale | -6 | a true price word: Rhine wine up a fifth at Paviken for 120 days |
| 5 | tell the brother | 0 | a bundle of fleeces (8 d) |
| 5 | tell both | 0 | a crock of honey (24 d); the deed `fair_dealing` |
| 6 | weigh | +30 | the deed `fair_dealing`; both kin's feeling |
| **6 (turn)** | earnest | **+200** or +90 | |
| 6 | stand | 0 | two cakes of wax (80 d) |
| 7 | wages | -60 | the rower as a witness at the thing; the deed `fair_dealing` |
| 7 | show | 0 or -30 | |
| 8 | every word | +45 | the deed `fair_dealing`; opens the fellowship road |
| **8 (turn)** | for the brother | **+120** | the daughter's ill will |
| 8 | for the daughter | +20 | the brother's ill will |
| **10 (finale)** | the third | **+960** | grand reward `unlock_factor_paviken` |
| 10 | **half, and peace** | +480 and two timbers of marten (720 d) | **two** grand rewards: `unlock_factor_paviken`, `unlock_title_gotland_guest_right` |
| **11 (finale)** | take / plead | **+600** and three cakes of wax (720 d) | grand reward `unlock_plot_paviken` |
| **12 (finale)** | leave | **+600** | grand reward `unlock_heirloom_gute_arm_ring` |
| 12 | **leave, and the stick to the boy** | +600 | **two** grand rewards: `unlock_heirloom_gute_arm_ring`, `unlock_title_gotland_guest_right` |
| **13 (finale)** | all | **+2,300**, or +800 and a hurt man | grand reward `unlock_heirloom_stern_stone_ring`; cost: the feud |
| 13 | a third | +960 | `unlock_heirloom_stern_stone_ring`; cost: the secret |
| 13 | fill the hole | +600 | `unlock_heirloom_gute_arm_ring` (ends as end_buried) |
| **14 (finale)** | **swear again (the hardest road)** | **+1,100**, a timber of sable, two of marten, three cakes of wax (1,940 d) | **two** grand rewards: `unlock_partner_ottar`, `unlock_plot_paviken`; both `changed_` flags |
| 14 | leave the share in | +300 and a timber of sable (780 d) | the same two grand rewards; `share_rides` |
| 14 | take it and swear nothing | +1,500 | `unlock_plot_paviken` |

**Why a merchant of the time could have won each grand reward.**

- `unlock_factor_paviken`: a farm near the beach with a dry storehouse and an owner who weighs her own silver is
  what a foreign house needs on an island with no town.
- `unlock_title_gotland_guest_right`: the word of two farms that the house may winter on the island and name them
  to whoever asks. An outlander's life is cheap there without a host (`peoples/svea_gotlandic.md` 12).
- `unlock_plot_paviken`: twenty paces of a farm's shore strip for a boathouse, given before neighbours. With no
  king and no town, ground can only come from a farmer.
- `unlock_heirloom_gute_arm_ring`: a plain ring of about 100 g, made to a weight and never cut, given by the dead
  man's kin. Rings were worn, shown and inherited before they were silver by weight.
- `unlock_heirloom_stern_stone_ring`: a twisted neck ring, too well known to sell in the north.
- `unlock_partner_ottar`: the fellowship of one purse (the word is on 11th-century stones; the thing is older (?)),
  with a Gotlander who sails east each summer and wants what the Rhine sells.

## How long it takes

A season at the least; a year for most houses; two and a half on the fellowship road.

- Summer 1: the dying man (s01, s02: three days). The passage to Gotland. Paviken (s04, s05, s06: five days).
- 40 days, and then only in June, July or August: the thing (s08). A house that reaches Aikgard in May can be at
  the thing the same summer; any other house comes back the next.
- The day after: s09. Two days more: s10 or s11. The same evening: s12 and s13.
- 400 days from s09, and then only from August to October: the ship is home (s14).

The waits are timed flags and months, so ordinary trading and other chains fill them.

## Pictures

One for each step (the same lines as the `art.<card id>` entries in the writing file):

1. **s01** A low back room in a host's house, late afternoon light through a smoke hole: a grey, big-boned
   Gotlander of fifty-five under a sheepskin, gripping the merchant's hand; a folding scale and a yew stick on the
   chest beside him; the host in the doorway.
2. **s02** A harbour shore at dusk, wind off the land: a small pyre above the tide line, a Northman standing by it;
   in front, an open sea chest with a scale in a bronze case, a bag of weights, a comb and a rune-cut stick.
3. **s03** Aboard at sea on a grey, steady day: a lean Northman reading a short yew stick with his thumb; a broad
   Frisian steersman looking away; the folded scale on the thwart between them.
4. **s04** Paviken on a bright windy morning: shingle, boat sheds, forge smoke, a half-planked hull; a shipwright
   who has just laid down his adze, pointing inland up a stream.
5. **s05** Inside a Gotland long house at midday: a heavy farmer of fifty setting bread and ale before the
   merchant; by the open door a young widow with a small boy; through the door a storehouse on a stone footing and
   a tall picture stone.
6. **s06** A storehouse with its floor boards stacked against the wall: a hole under a lifted slab, a bronze bowl
   of arm rings and thin cut coins; the farmer kneeling on one side, his niece standing on the other, neighbours in
   the doorway.
7. **s07** A low skerry coast under a white sky: a six-oared boat lying across the Frisian boat's bow, oars level;
   a fair-bearded Svea standing in her stern with one hand out, palm up.
8. **s08** A grassy slope above farms in late summer light: sixty men sitting in a ring, spears upright behind
   them; a bronze bowl on a hide in the middle, the farmer on one side, the widow on the other, an old man with a
   staff, the merchant standing alone.
9. **s09** A farm table by the open door at dusk; beyond a pasture fence a ship setting of standing stones black
   against a yellow sky, sheep among them; the merchant looking that way while nobody else does.

One for each ending:

- **Her father's daughter** (s10): the stone ship at first light, mist in the pasture: the widow kneeling at a
  scale set on a fallen stone, an open pitch-sealed bowl of coiled rings beside her, a fresh hole at the stern
  stone; her uncle a dark figure at the fence.
- **The farm's silver** (s11): the farm table in early morning: an empty pitch-smeared bowl, five small heaps of
  ring silver and three cakes of wax; the farmer with earth on both knees, a spade against the door post.
- **The stern stone** (s12): the stone ship on a long summer evening: the merchant with one hand flat on the
  leaning stern stone, uncut turf at its foot; a Norse hand with folded arms; far off, the farm's smoke, a woman
  and a small boy at the fence.
- **An arm deep** (s13): the same stones on a moonless night, by a lamp held under a cloak: two men lifting a
  sealed bowl out of a hole, a spade in the turf, the merchant looking back at one small red light at the farm.
- **Silver that floats** (s14): Paviken beach on a clear early-autumn day: a new ship on the shingle, her chequered
  sail spread to dry; a new booth with a mark cut over its door; a young skipper, the widow with a tally stick and
  the old farmer with folded arms, waiting by a scale.

## Needs (for the integrator; nothing was added to the shared tables)

- Grand-reward flags that are new names: `unlock_factor_paviken`, `unlock_plot_paviken`, `unlock_partner_ottar`,
  `unlock_title_gotland_guest_right`, `unlock_heirloom_gute_arm_ring`, `unlock_heirloom_stern_stone_ring`.
- `changed_gotland_hoard_spent`, `changed_frisians_on_gotland`, and the cost flags above, for later cards to read.
- The sea card (s07) cannot ask which sea she is in: there is no region variable at sea. It uses a 40-day flag set
  at the farm instead. A `sea.region` variable would make it exact.
- Three cards schedule with `at here` (s01, three times: the harbour of the call). `Effects.cs` resolves "here";
  if the loader's port cross-check does not, name the port by splitting s01 into four cards.
- s09 and s12 use the option field `then` (as `chain_house_debt` does) to open the next card at once.
- A deed for an oath broken to a dead man, and one for digging another man's ground, would fit s13 better than
  feeling alone. `fair_dealing` stands in for "bore true witness" at s08.
- `open_fetch` and `hoards_793.json` are not used: the game's fetch walks a shore party over the chart, and this
  farm has no place on it. If Aikgard is given one, s06 and s13 could open a fetch instead.
- No place file exists yet for `paviken`: Vivil, Ailikn, Ottar and the shipwright should become people there, and
  Aikgard and the stone ship sites.
