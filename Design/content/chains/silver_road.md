# Chain: the silver road (`silver_road`)

Cards: `Assets/Kapman/Game/Data/encounters/chain_silver_road_793.json` (14 cards).
Words: `Assets/Kapman/Game/Data/writing/chain_silver_road.json` (197 entries: every key, an outcome text for every
option, six news items, an `art.<card id>` line for every card).
Written 6 Oct 2026. A draft for the user to overwrite. The checker is clean on both files.

## The story in five sentences

A Svea trader at Dorestad pays for wine with silver he weighs, and among it is a thin coin with writing on both
sides and no king's head, which the moneyer cannot stop looking at. The merchant follows the coin to Birka, learns
the folding scale and the nick, and the next summer sails east in the Svea's company, over the sea, up a short
river and across a shallow lake to Aldeigja on the Volkhov, where skins come out of the woods and silver comes up
from the south. There he is shown what most of the silver is paid for: people, roped in a pen behind the dealer's
house, and among them a Frisian ship's boy who keeps the dealer's tally. He can buy the boy out, give his hand on
bringing six people next summer, or say his boat carries things that do not need feeding. A year later the road
ends one of four ways: one purse with the Svea, a ship bought with six people, a boy brought home who goes back
east as the house's own man, or a Frisian house on the Volkhov and the east's silver in the pot at Dorestad.

## How a player meets it

At Dorestad, in the sailing months (May to September) of any year from 797 to 809, once the house has called there
three times: card s01, weight 0.8, so within a summer or two of ordinary trading. It does not need silver in the
purse (the option "ask him where such silver comes from" is free). After that the chain waits at Birka: s02 comes
up almost surely (weight 3) on the first call there in the market months.

## The real history under it

- **The town.** Staraya Ladoga (the Norse Aldeigja, Aldeigjuborg) stands from 753 by the tree-rings of its oldest
  timbers; a Norse smith's tools lie in its first layer. No lord is named there before 839. Sea ships stop at the
  town: above it the Volkhov runs through rapids. (en.wikipedia "Staraya Ladoga"; `realms/rus.json` who_holds and
  road; `peoples/baltic_east.md`.)
- **The silver.** The oldest hoard at Ladoga closes with a coin of 786/7. Dirhams reach the Baltic about 800. The
  Peterhof hoard on the Gulf of Finland (youngest coin about 805) has marks scratched in four scripts: Norse runes,
  Greek, Arabic and Turkic runes. A dirham weighs about 2.9 g, has writing and no image, and in the north is
  weighed, cut and nicked, never counted. (T. S. Noonan, *The Islamic World, Russia and the Vikings, 750-900*,
  1998, as cited in `goods_793.json`; `chronicle_793.json` 805-peterhof-hoard; spine thread `silver_road`.)
- **The reckoning at home.** Charles's new penny of about 1.7 g, to be taken at its face in every market: synod of
  Frankfurt, June 794, c. 5 (MGH Capitularia I no. 28; `chronicle_793.json` 794-06-frankfurt-new-denier). So twenty
  dirhams weigh as thirty-four pennies, which is the moneyer's line.
- **What went east and what came back.** The Rus "bring beaver skins, black fox and swords": Ibn Khurradadhbih,
  *Book of Roads and Kingdoms* (about 846, revised about 885). Slaves as the chief thing sold for silver: Ibn Rusta
  (about 903-913) and Ibn Fadlan (922). All three are later than the chain and are used only for what the road
  carried; nothing is quoted. Green glass beads at a dirham a bead are Ibn Fadlan's (LATER).
- **The law the blades break.** No mail coat out of the realm: Herstal, 779, c. 20. No arms to the Slavs and Avars,
  and trade with them only at the named posts: Thionville, December 805, c. 7 (`chronicle_793.json`
  805-12-thionville-arms, 805-12-thionville-border-posts). Holmfast's line changes at 805.
- **The law the captives break, and the good work.** Slaves to be sold before the bishop or count and not beyond
  the march: Herstal, 779, c. 19. The ransom of captives: Rimbert, *Vita Anskarii* c. 15 and 35 (Anskar buys boys
  to teach them and gives for captives); *Vita Rimberti* c. 17-18 (Rimbert gives his horse for a captive nun at
  Sliaswic). Chapter numbers were not re-checked tonight (?).
- **Birka and Dorestad.** Men of Birka had been to Dorstadt; Catla carried her mother's silver there: Rimbert c. 20
  and 27 (tr. Robinson 1921 exists; not quoted). Ships taken by pirates on the way to Birka: Rimbert c. 10.
- **Gotland.** Over 700 silver hoards; the Spillings hoard, 67 kg, buried after 870 under a storehouse floor
  (`peoples/svea_gotlandic.md`; `riddles_793.json` rw_hoard_spillings).
- **Where the silver really went.** It stayed in the north. Eastern silver is first seen on the Rhine in 965, when
  Ibrahim ibn Yaqub of Tortosa saw dirhams of Samarkand at Mainz (`chronicle_793.json` 965-ibrahim-hedeby).

**Invented, and marked so in the cards' notes:** that a dirham reached Dorestad as early as 797 (?); the dealer
Vigmund; the Gotlander Botmund; the boy Folkbern, his boat and his kin; the price of a mark and two ounces (about
150 d; reckoned from later northern law, where a thrall is three marks (?)); the "skin in ten" the Svear are said
to take for showing the way (?); the ship left for a debt. Holmfast (`cast.holmfast`) and the moneyer Sigimar
(`cast.sigimar`) are the spine's invented cast; Ravn and Aldger (`cast.crew.ravn`, `cast.crew.aldger`) are the
spine's crew. Nobody in the speech says "dirham", "Rus" or "Viking": it is the Saracens' silver, the writing
silver, the east road, Aldeigja.

## The steps

Progress flags are `ch_silver_road_1` to `_10`. `sv_*` flags are the spine's shared flags for this thread.

| # | Card id | Where and when | The choices | Flags set |
|---|---|---|---|---|
| 1 | `harbour.chain_silver_road.s01_faceless_coin` | Dorestad, May-Sept, 797-809, third call or later | buy the coin and give it to the moneyer (6 d) / sell a cask of wine on his scale (likely) / ask where it comes from (free) | `_1`, `sv_first_dirham`; buy: `sv_moneyer_trusts`, `ch_silver_road_has_coin` |
| 2 | `harbour.chain_silver_road.s02_scale` | Birka, May-Aug | buy a scale and learn the nick (30 d) / trade on the neighbour's scale (doubtful) / sit and watch (free) | `_2`; scale: `sv_scale`; schedules step 3 in 2 days |
| 3 | `harbour.chain_silver_road.s03_order` | Birka, two days later | promise four blades (against the king's law) / promise beads and wine / promise nothing (free) | `_3`, `ch_silver_road_blades` or `ch_silver_road_lawful`; timers `ch_silver_road_wait_a` (240 days), `ch_silver_road_baltic` (40 days) |
| 4 | `sea.chain_silver_road.s04_low_ship` | at sea within 40 days of step 3 (a side step; may never come) | tow a Gotland boat off a lee shore (likely) / name a price first / stand by (free) | `_4`; tow: `ch_silver_road_gute_friend` |
| 5 | `harbour.chain_silver_road.s05_in_company` | Birka, May-July of the next year (after `wait_a`) | show the blades / show beads or wine / come empty and keep his tally (free) | `_5`, one of `_5_blades`, `_5_goods`, `_5_empty` |
| 6 | `harbour.chain_silver_road.s06_volkhov` | Aldeigjuborg, June-Sept | sell four blades / ten strings of beads / three casks of wine / sell nothing and keep the tally (free) | `_6`, `sv_first_dirham`, one of `_6_blades`, `_6_goods`, `_6_tally`; schedules step 7 in 2 days |
| 7 | `harbour.chain_silver_road.s07_pen` | Aldeigjuborg, two days later | ransom the boy (150 d) / give your hand on six people / say you carry skins (free) | `_7`, one of `ch_silver_road_ransom`, `ch_silver_road_people`, `ch_silver_road_furs`; timers `ch_silver_road_wait_b` (45 days), `ch_silver_road_wait_c` (270 days) |
| 8 | `harbour.chain_silver_road.s08_under_floor` | Paviken, any call after step 7 (a branch; not needed for endings 1 to 3) | take the Gotlander's pilotage for the oar's sake (needs step 4's tow) / pay his fee (80 d) / keep faith with Holmfast (free) | `_8`, `ch_silver_road_own_road` or `ch_silver_road_keeps_faith` |
| 9 | `harbour.chain_silver_road.s09_moneyer` | Dorestad, after `wait_b` | sell the bag for the pot / keep it whole for the north / give him one coin to study (free) | `_9`; pot: `ch_silver_road_mint`, `sv_moneyer_trusts`; whole: `ch_silver_road_keeps_whole`, `sv_scale`; gift: `sv_moneyer_trusts` |
| 10 | `crew.chain_silver_road.s10_three_silvers` | at sea, any time after step 6 (a side step) | weigh it all / change it all at a mint / let them argue (free) | `_10`; weigh: `sv_scale`; mint: `ch_silver_road_trusts_stamp` |
| 11 | `harbour.chain_silver_road.s11_end_partner` | Birka, May-Aug, after `wait_c`; not on the own road; not while carrying people east | swear on his ring / swear your own way / no oath, a yearly bargain | **`ch_silver_road_end_partner`**, `ch_silver_road_done`; ring: `ch_silver_road_ring_oath` |
| 12 | `harbour.chain_silver_road.s12_end_slaver` | Aldeigjuborg, June-Sept, after `wait_c`, with `ch_silver_road_people` and six captives aboard | take the silver and the ship / take less and the Frisian boy / turn back at the door (free) | **`ch_silver_road_end_slaver`**, `ch_silver_road_done`, `ch_silver_road_blood_silver`; back: `ch_silver_road_people = 0`, `ch_silver_road_broke_hand` |
| 13 | `harbour.chain_silver_road.s13_end_ransom` | Dorestad or Wijnaldum, after `wait_b`, with `ch_silver_road_ransom` | take what the kin lay down / take back the 150 d only / take nothing (free) | **`ch_silver_road_end_ransom`**, `ch_silver_road_done` |
| 14 | `harbour.chain_silver_road.s14_end_frisian_house` | Aldeigjuborg, June-Sept, after `wait_c`, with `ch_silver_road_own_road` | take the ground and carry the silver to the Rhine mint (needs step 9's pot) / take the ground and keep the silver in the north / take the ground and send Holmfast a third (pride) | **`ch_silver_road_end_frisian_house`**, `ch_silver_road_done`, `changed_frisians_on_volkhov`; mint: `changed_dirhams_on_rhine`; mint, north: `ch_silver_road_holmfast_feud` |

**No dead ends.** Every card has a free option. Steps 4, 8 and 10 are side steps the chain never waits for. A
failed check (the scale at step 1 or 2, the tow at step 4) costs a little and the story goes on; the tow that
parts still makes the Gotlander a friend. The danger of the blades is told before the promise (Holmfast's line on
the king's law) and is the toll man's own card at home, not this chain's. The man who gives his hand on six and
turns back at the door loses the dealer and keeps the other endings. A house that takes the own road and never
sails it has no finale: say so to the integrator if that should fall back to s11 (drop `not
flag.ch_silver_road_own_road` from its `when`).

## The endings

| Ending | Flag | How | What becomes of them | History |
|---|---|---|---|---|
| One purse | `ch_silver_road_end_partner` | s11 at Birka | Holmfast keeps the east road and the Frisian is his partner in the west: skins and wax up the Rhine, wine and beads back. The boy stays in the pen unless he was ransomed. | **As it really went:** the east's silver and the road stayed with the Svear and Gotlanders; Frisians bought at Birka. |
| Six, sound and young | `ch_silver_road_end_slaver` | s12 at Aldeigjuborg with six captives | Six people go south for silver. The house has a sea-going ship and a name the priests know. The boy stays, or goes home having cut the notches. | **As it really went:** people were the thing that drew the silver north. |
| A mark and two ounces | `ch_silver_road_end_ransom` | s13 at Dorestad or Wijnaldum | Folkbern comes home, and in spring goes back east free, as the house's own man at Aldeigja; on the hardest option he is fostered into the house. | Invented, and true to the time: the ransom of captives was a known good work. |
| Ground on the Volkhov | `ch_silver_road_end_frisian_house` | s14 at Aldeigjuborg | A Frisian house stands between the dealer's and the smithy; Holmfast is cut out (a feud, unless he is sent a third). | **History changed:** see below. |

**What follows from the changes** (for later cards to read):

- `changed_frisians_on_volkhov`: a western house at the head of the east road, a century and a half before any
  western merchant is known there. Later cards can give the heirs a storehouse and a man at Aldeigjuborg; in 839,
  when men who call themselves Rhos come to Louis at Ingelheim and prove to be Svear (Annals of St Bertin), an heir
  of this house is the one man at court who has seen where they come from; about 865, when Ladoga burns, the house
  burns with it and must be rebuilt under Rurik's men or given up.
- `changed_dirhams_on_rhine`: the east's silver goes into the pot at Dorestad in Charles's lifetime, not first at
  Mainz in 965. Later cards can make Dorestad's mint busier and its pennies more plentiful to 834, which also
  makes the town a richer prize when the Danes come (chain `dorestad_834`), and can let the first coin of Hedeby,
  about 825, copy a Dorestad penny struck from eastern silver.
- `ch_silver_road_holmfast_feud` (a lasting cost): Holmfast, or his son after him, will not deal with the house
  at Birka, and says so at Paviken and Sliesthorp.
- `ch_silver_road_blood_silver` (a lasting cost): the news is posted at Dorestad; priests there refuse the house's
  alms and the count's steward watches its cargo. The deed `sold_captives` is done at Aldeigjuborg, where it costs
  nothing; it is the news at home that costs.
- `ch_silver_road_ring_oath` (small): a baptised man swore on a heathen ring; a priest at home may ask about it.

## Payouts

| Step | Option | Silver | And |
|---|---|---|---|
| 1 | buy | -6 | the moneyer's trust (`sv_moneyer_trusts`), his feeling +8 |
| 1 | sell wine | +85 or +55 for one cask (60 d at home) | |
| 1 | ask | 0 | a true price word: wine up a fifth at Birka for 120 days |
| 2 | scale | -30 | `sv_scale`; Old Norse to 0.5 |
| 2 | borrow | +35 or -20 | |
| 2 | watch | 0 | runes +0.1 |
| 3 | blades / beads | a timber of squirrel (24 d) | Holmfast's feeling |
| 4 | tow | +36, or a hurt hand | the Gotlander's friendship (opens the free road at step 8) |
| 4 | price | +70 | he remembers it against you |
| 5 | any | 0 | Old Norse to 0.75 |
| **6 (turn)** | blades | **+340** for four blades (240 d at home) | a timber of marten (120 d) |
| 6 | beads | +200 for ten strings (120 d) | two bundles of beaver (120 d) |
| 6 | wine | +270 for three casks (180 d) | a cake of wax (40 d) |
| 6 | tally | +40 | two timbers of squirrel (48 d) |
| 7 | ransom | -150 | the deed `gave_alms`; the boy as a passenger |
| 7 | people | +30 earnest | |
| 8 | fee | -80 | the own road |
| 8 | faith | 0 | a true price word: Frisian cloth up a fifth at Paviken for 200 days |
| **9 (turn)** | pot | **+160** | the moneyer's trust; news at Dorestad |
| 9 | whole | 0 | `sv_scale`; a price word for Sliesthorp |
| 10 | any | 0 | the crew's heart +0.02 to +0.05 |
| **11 (finale)** | ring / own oath | **+700**, two timbers of marten, three cakes of wax (about 1,060 d) | grand reward `unlock_partner_holmfast` |
| 11 | bargain | +600, a timber of marten (720 d) | grand reward `unlock_factor_birka` |
| **12 (finale)** | sell | **+1,700** for six captives | grand reward `unlock_ship_norse_allround_ship`; cost `ch_silver_road_blood_silver` |
| 12 | sell, and the boy | +1,450 | the same ship and cost; the boy goes home |
| **13 (finale)** | take | **+400** and ten bolts of Frisian cloth (880 d) | grand reward `unlock_factor_aldeigjuborg` |
| 13 | price | +150 | `unlock_factor_aldeigjuborg`; the deed `fair_dealing` |
| 13 | **nothing (the honest road)** | **+500** and twelve bolts of cloth (1,076 d), laid in the boat by night | **two** grand rewards: `unlock_factor_aldeigjuborg`, `unlock_kin_folkbern`; the boy hired as a hand; Old Norse to 1, Slavic to 0.5; the deed `gave_alms` |
| **14 (finale)** | **mint (the hardest road)** | **+1,500** and a timber of sable (1,980 d) | **two** grand rewards: `unlock_plot_aldeigjuborg`, `unlock_pilot_volkhov`; both `changed_` flags; cost: the feud |
| 14 | north | +1,100 and a timber of sable (1,580 d) | `unlock_plot_aldeigjuborg`; cost: the feud |
| 14 | a third to Holmfast | +750 and a timber of marten (870 d) | `unlock_plot_aldeigjuborg`; no feud |

**Why a merchant of the time could have won each grand reward.**

- `unlock_partner_holmfast`: the northern fellowship of one purse (felag) between two traders at the two ends of a
  sea; Birka men did come to Dorestad.
- `unlock_factor_birka`: a man of the house kept at a partner's booth through the market months.
- `unlock_ship_norse_allround_ship`: a hull taken for a debt and passed on as part of a price; the class is the one
  the east road used below the rapids.
- `unlock_factor_aldeigjuborg`: a freed man who tallies and speaks Frisian, Norse and some Slavic is exactly what a
  western house would need there, and exactly what the dealer was keeping him for.
- `unlock_kin_folkbern`: a fostering before witnesses, the terp's way of paying a debt that will not take silver (?).
- `unlock_plot_aldeigjuborg`: ground given by the man with the biggest house in a town with no lord (before 839).
- `unlock_pilot_volkhov`: the Neva, the lake crossed in the night calm, and the river up to the town, told once by
  a Gotlander who sails it every summer.

## How long it takes

Two years at the least, three on the own road.

- Summer 1: Dorestad (s01), then Birka (s02, s03) in the same market season or the next.
- 240 days: the winter.
- Summer 2: Birka (s05), the passage east, Aldeigjuborg (s06, s07).
- 45 days: the way home. Autumn 2: Dorestad (s09), and the ransom's ending (s13).
- 270 days from step 7: Summer 3: Birka (s11), or Aldeigjuborg (s12, s14).

The waits are timed flags, so ordinary trading and other chains fill them. Start years 797 to 809 keep the whole
chain before 839 (the first named ruler of the Rhos) and inside the lives of Holmfast and Sigimar.

## Pictures

One for each step (the same lines as the `art.<card id>` entries in the writing file):

1. **s01** Dorestad, a timber dam at midday in summer: a spare, pale-eyed Svea in a fur-trimmed cap holds a small
   folding scale over a wine cask; a stooped moneyer in a leather apron bends close to one thin broad coin in the pan.
2. **s02** Birka, a booth on the jetty in clear morning light: furs to the roof, a bowl of cut coins, the Svea
   showing a knife-nick on a coin's edge; next door a man with his thumb on a scale beam.
3. **s03** Birka at dusk, late summer: the Svea kneeling on a bale of skins to rope it; his ship drawn up for the
   winter; a bare sword blade drawn with a finger in the dust of the floor.
4. **s04** The open Baltic, grey afternoon, a hard wind: a Gotland boat low in the water with no steering oar, men
   bailing, a hide-covered chest amidships, breakers close under her.
5. **s05** Birka's bay on a bright early-summer day, birches in new leaf: four loaded ships and one very small
   Frisian boat alongside the nearest.
6. **s06** Aldeigja on the brown Volkhov under a low white sky: log houses, skins stacked like firewood, trappers
   standing apart, a heavy man weighing thin silver in a doorway, white water upstream.
7. **s07** Behind a log house in flat evening light: a pen of split logs, people roped by the neck in fours, faces
   down; one boy of thirteen with a tally stick looking straight out.
8. **s08** Paviken, inside a storehouse with the door open to a bright beach and a half-built hull: a shipwright
   laying floor planks over freshly dug earth; an oar hung on the wall.
9. **s09** The mint in Dorestad's upper town by lamplight: a heap of coins covered in writing, the moneyer with a
   balance, a priest holding one coin to the lamp.
10. **s10** Aboard at sea in a fair wind: an open purse on a thwart with three little heaps of silver; a Dane
    testing a penny with his knife, a Frisian jabbing a finger at him, the crew grinning.

One for each ending:

- **One purse** (s11): Birka at midsummer evening: two stools, a jug of ale, a scale, and a plain silver arm ring on
  the bench between the Svea and the Frisian; a divided heap of silver at the Svea's elbow.
- **Six, sound and young** (s12): Aldeigja's landing in hard noon light: six roped people in a row on the bank, the
  dealer looking at a man's teeth; behind him the boy cutting notches, eyes down; a long empty ship against the bank.
- **A mark and two ounces** (s13): a Frisian landing under a wide evening sky, a terp on the skyline: a boy held
  hard by a woman and a big farmer; before the merchant a cloak on the ground with folded cloth and a little silver.
- **Ground on the Volkhov** (s14): the river bank on a still misty morning: four new house posts in bare ground
  between a big log house and a smithy; the dealer pacing the plot, a Gotlander pointing at the ice line, one small
  Frisian boat alone at the landing.

## Needs (for the integrator; nothing was added to the shared tables)

- Grand-reward flags that are new names: `unlock_partner_holmfast`, `unlock_factor_birka`,
  `unlock_factor_aldeigjuborg`, `unlock_kin_folkbern`, `unlock_plot_aldeigjuborg`, `unlock_pilot_volkhov`
  (`unlock_ship_norse_allround_ship` is already used by another chain).
- `changed_frisians_on_volkhov`, `changed_dirhams_on_rhine`, and the cost flags above, for later cards to read.
- The sea card (s04) cannot ask which sea she is in: there is no region variable at sea. It uses a 40-day flag set
  at Birka instead. A `sea.region` variable would make it exact.
- A deed for ransoming a captive would fit step 7 better than `gave_alms`.
- No text of the chest is given: `texts_793.json` has nothing of the east road for these years.
