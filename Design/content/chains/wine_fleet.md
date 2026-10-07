# Chain: wine_fleet (the wine fleet to Saint-Denis)

Cards: `Assets/Kapman/Game/Data/encounters/chain_wine_fleet_793.json` (14 cards).
Words: `Assets/Kapman/Game/Data/writing/chain_wine_fleet.json` (every key the cards use, an outcome text for every option, 4 news items, 14 picture lines).
Written 6 Oct 2026. A draft for the user to overwrite. (?) marks what could not be checked against a source.

## The story in five sentences

Radbern, an old Frisian shipman of Dorestad, has gone up the Seine to the fair of Saint-Denis every October for twenty years, and when his second boat is lost he asks the player's small boat to sail ahead of his ship: cloth up, wine down, for a share.
At Rotomagus the king's toll is taken and the river learned; at the saint's strand the abbey takes every toll for the month of the fair, a man of the count's tries the old head-penny that King Pippin struck down, and in the abbey's storehouse the cellarer offers cheap wine that is still working, dear wine that has settled, or the abbot's own.
Twelve days into the fair Radbern lies in the hospice with a fever, and his tally and best casks are in the player's hands, with an Englishman of Hamwic offering ready silver for them.
The way home is a frost and a weeping bung, and at the first quay the cask is broached: a winter's profit, or vinegar.
After the reckoning with Radbern's daughter a year passes, and at the next fair three people want the boat: the house (fellowship), the abbot (a landing place for the saint at Lundenwic), or the Englishman (a cellar at Rotomagus and the trade for oneself).

## How a player meets it

- It starts by itself at Dorestad, the home port: card `s01_second_keel` is drawn on a call there in July, August or September of 796 to 802, once the house has made two calls (weight 0.9, likelier for a house in good standing). Saying "not this year" costs nothing, gives a true price word, and he asks again the next summer.
- It begins in 796 because that is the year the two kings made peace for merchants; the English are part of the story.
- After step 1 the cards wait where the story is: Rotomagus in September to November (optional), the strand at Parisius in October and November (the fair). A player who misses the fair meets the card at the next one; nothing is lost.
- The player's boat carries only a few casks (a cask is 115 kg; the first boat holds 660 kg). The fleet's wine is in Radbern's ship; the player's share is paid in silver by the cards, and the two or three casks in the hold are the player's own to sell at any market.

## The real history under it

**The fair.** The fair of Saint-Denis opened on the saint's day, 9 October. It lasted a month (the repo's `fairs_793.json` has 28 days) or seven weeks, to Saint Andrew's (the Saint-Denis archaeology unit). "Saxons and Frisian merchants" came "to sell wool and woolen cloth, and to buy wine and honey"; the abbey sold its grain and the spare wine of its vineyards. Pennies of Dorestad and of Quentovic and a sceat were found in the digs. Source: Unité d'archéologie de Saint-Denis, "The Saint-Denis fair", https://archeologie.culture.gouv.fr/saint-denis/en/saint-denis-fair (read 6 Oct 2026). A pitch among the graves at two pennies is from the same page and is of the later fair (?).

**The abbey's tolls and the count's head-penny.** Read through `Design/content/dossiers/parisius.md` (which read the Latin in J. Tardif, Monuments historiques, 1866: https://archive.org/details/monumentshistori00tard):
- Tardif no. 44 (13 December 710): the count had claimed half the fair's toll and lost.
- Tardif no. 55 (8 July 753): merchants came "to buy wine in the ports and on the rivers"; since Charles Martel's day the count had taken four pennies a head from every free merchant, and five from a slave, who first had to swear that he was one; the monks said the merchants were staying away; King Pippin struck the tax down. From Saint Denis's Mass until the market ends every toll in the district of Paris is the abbey's.
- Tardif no. 77 (about 775): Charles confirms it: ships, harbours, carts, pack beasts, and the wheel, bridge and gate tolls.
- Tardif no. 81 (27 March 779): the traders of Saint-Germain-des-Prés go toll-free at Rouen, Quentovic, Amiens, Maastricht, Dorestad and Paris. This is the model for the reward "the abbey's own merchant" (`unlock_charter_parisius`).

**People who were really there.** Fardulf, a Lombard, abbot of Saint-Denis 793 to 806, given the abbey for uncovering the plot of Pippin the Hunchback (Annales regni Francorum 792, revised text; https://fr.wikipedia.org/wiki/Fardulf). Stephen, count of Paris 779 to 811/815; in 802 he and Fardulf were the king's envoys for the Paris district (MGH Capit. I no. 34). Gervold, abbot of Saint-Wandrille and the king's procurator of the Channel tolls, 787 to 806 (Gesta abbatum Fontanellensium c. 12; spine `cast.gervold`): the toll man at Rotomagus is his man. The reeve at Lundenwic is the spine's invented Wighelm (`cast.wighelm`, to 810). Fardulf's guest house for the king at Saint-Denis is from his own verses, from memory (?).

**The kings' peace, and false pilgrims.** About 790 Charles and Offa quarrelled over a marriage and closed their ports to each other's merchants; in 796 Charles wrote to Offa that merchants should have their old protection in his kingdom, and that men who mixed themselves among pilgrims to trade must pay the tolls at the proper places (`chronicle_793.json` 796-spring-offa-letter; given in our own words, nothing is quoted). Offa died on 29 July 796; Coenwulf was king from December. Card 2's "pilgrim" option is that letter turned into a choice.

**The river.** A boat coming up from Rouen reaches Saint-Denis before the city, because the river loops north below Paris; an island lies beside the abbey (`dossiers/parisius.md` 1). Ships rode the flood up to Rouen (Nithard on 841, LATER; `dossiers/rotomagus.md` 1). The abbey of Saint-Denis is described in a text of 799 as 245 feet long with 101 windows (the line is shown only from 799).

**The wine.** That wine of the year is still working at an October fair, that a bung driven tight on working wine starts the staves, and that a cask not kept full and still turns to vinegar, is the common knowledge of vintners in every age; I give it from memory with no period text behind it (?). The vintage at Saint Remigius's Mass (1 October) and the tasting of the new wine at Martinmas (11 November) are later custom (?). Casks of oak and silver fir with willow hoops are the casks found re-used as well linings at Dorestad (`Design/economy_and_trade.md`). Vinegar as a ware: the Emperor's men pay part of their London toll in vinegar about 1000 (IV Æthelred 2, LATER). Spiced, honeyed wine is the Roman conditum of Symphosius's riddle 82, which is in `texts_793.json`; the cellarer's riddle is Aldhelm's on the wine cask (`riddles_793.json` riddle_aldhelm_78_cupa_vinaria).

**The saint's landing place.** A charter in the abbey's archive has King Offa, in 790, give Saint-Denis land "in the port called Lundenuuic". It is a forgery, perhaps on a true base (Sawyer 133: https://esawyer.lib.cam.ac.uk/charter/133.html; `dossiers/parisius.md` 8). The abbot's errand in the chain is built on that claim: the abbey says it was promised, the reeve never heard of it.

**Rouen and wine.** The vine does not thrive at Rouen; the wine is the Paris basin's, come down river; about 1000 the men of Rouen carry wine to London (`dossiers/rotomagus.md` 4; IV Æthelred 2, LATER). A cellar there is the natural place to hold wine for the sea.

**No sworn bands.** Capitulary of Herstal, 779, c. 16: no oaths sworn in guilds, but men may agree about alms, fire and shipwreck (MGH Capit. I no. 20; from memory (?), as in chain house_rival). So the fellowship is hands joined and no oath.

**Invented, and said so.** Radbern (Rad- + -bern) and his daughter Aldburg (Ald- + -burg), Leofstan of Hamwic (Leof- + -stan), Brother Waldric the cellarer (Wald- + -ric), Ursio the cooper (an attested Frankish name (?)), the count's man, the hospice brother, the steersman, the clerk. That a man of Count Stephen tried the head-penny again in the 790s is invented (the tax and its striking down are real). The abbot's letter to the reeve is invented. No toll rate survives for Rouen: the six pennies are ours.

## The people

| Who | Id | In the cards |
|---|---|---|
| Radbern, shipman of Dorestad | invented | actor `partner` (role owner) |
| Aldburg, his daughter, who cuts the tally | invented | actor `daughter` (role merchant) |
| Leofstan of Hamwic | invented | actor `english` (role merchant) |
| Brother Waldric, cellarer of Saint-Denis | invented | actor `cellarer` |
| Abbot Fardulf | historical, 793 to 806 | actor `abbot` (ending 1 only; elsewhere he is "the lord abbot") |
| The king's toll man at Rotomagus | invented; Gervold's man (`cast.gervold`) | actor `tollman` |
| Ursio, a cooper of Rotomagus | invented | actor `cooper`; can be hired |
| The reeve of Lundenwic | `cast.wighelm` (to 810) | actor `reeve` |
| The mate | `cast.crew.brunger` | actor `mate` at sea |
| A count's man, a river man, a hospice brother, a steward, a priest, a salter, a clerk, a steersman | invented, unnamed | roles lords_man, pilot, guest_master, steward, priest, salter, clerk, steersman |

The forge names the people on the panel; the words call them by the names above. The integrator should give these people fixed names when the cast table allows (see `needs` in the report).

## The steps

Drawn cards (weight 30 or 40) open on a call at their port in their months. Scheduled cards (weight 0) open on the first call at their port after their day. "Likely" is a chance of 0.55 to 0.6, "doubtful" 0.5, before the crew's skill.

| # | Card id | Where and when | The choices | Flags set |
|---|---|---|---|---|
| 1 | `harbour.chain_wine_fleet.s01_second_keel` | Dorestad, July to September, 796-802; drawn | join hands for a share / sail in company for your own purse (20 d) / not this year (a price word) | `ch_wine_fleet_1`; `_share` or `_own_purse` |
| 2 | `harbour.chain_wine_fleet.s02_tide_and_toll` | Rotomagus, September to November; drawn; optional | pay the king's toll (6 d) / pass as a pilgrim (doubtful) / lie over a tide and learn the river | `ch_wine_fleet_2`; `_pilgrim`, `_river_word` |
| 3 | `harbour.chain_wine_fleet.s03_saints_day` | Parisius, October and November; drawn | pay the count's head-penny (16 d) / name King Pippin's judgment (likely) / ask at the abbey gate | `ch_wine_fleet_3`; `_paid_head`, `_spoke_753` or `_abbey_ear` |
| 4 | `harbour.chain_wine_fleet.s04_cellar` | Parisius, 3 days on | new wine, three casks / old wine, two casks / the abbot's casks on trust (needs the abbey's ear, the judgment spoken, or Latin) | `ch_wine_fleet_4`; `_new_wine` or `_old_wine`; `_abbey_wine` |
| 5 | `harbour.chain_wine_fleet.s05_fever` | Parisius, 12 days on | take his tally and casks and sail / stay until the fever turns / sell his casks to the Englishman (240 d) | `ch_wine_fleet_5`; `_trust` or `_sold_english`; `_radbern_lives` and `_late` if you stay |
| 6 | `harbour.chain_wine_fleet.s06_cooper` | Rotomagus, October to March; drawn; optional | fill and hoop (12 d) / hire the cooper (30 d) / keep your pennies (a price word) | `ch_wine_fleet_6`; `_topped`, `_cooper` |
| 7 | `sea.chain_wine_fleet.s07_frost` | at sea, November to March; drawn; optional | nurse the casks / run for it (likely) / drive the bungs home | `ch_wine_fleet_7`; `_careful`, `_fast`, `_shaken` or `_bunged` |
| 8 | `harbour.chain_wine_fleet.s08_broach_sound` | Dorestad, Lundenwic, Hamwic or Quentovic, when the wine has kept | sell on the quay / sell half and take an order / give a cask for the Mass | `ch_wine_fleet_8`, `_sound` |
| 9 | `harbour.chain_wine_fleet.s09_broach_sour` | the same call, when the wine has turned | sell it as vinegar / mend it with honey and pepper (10 d; doubtful) / say whose hand was on the bung | `ch_wine_fleet_8`, `_sour`; `_spiced`, `_owned_fault` |
| 10 | `harbour.chain_wine_fleet.s10_reckoning` | Dorestad, 4 days on | lay down the tally and the casks (needs `_trust`) / count out the Englishman's 240 d (needs `_sold_english` and the silver) / a short reckoning | `ch_wine_fleet_9`; `_honest` or `_kept`; timer `_rest` 200 d |
| 11 | `harbour.chain_wine_fleet.s11_next_october` | Parisius, October and November, after the 200 days; drawn | the house again (needs `_honest`) / the abbot's errand (needs the abbey's ear, wine or the judgment, or Latin) / your own purse with the Englishman | `ch_wine_fleet_10`; `_road_fellow`, `_road_abbey` or `_road_own` |
| 12 | `harbour.chain_wine_fleet.end_fellowship` | Parisius, 14 days on (the close of the fair) | join hands / join hands and ask the abbot for a strand (the hard road) / take your share | `ch_wine_fleet_end_fellowship` |
| 13 | `harbour.chain_wine_fleet.end_abbey` | Lundenwic, November to April; drawn | stand surety for the saint's plot (60 d; the hard road) / ask the abbot's writing for yourself / a carrier's wage | `ch_wine_fleet_end_abbey` |
| 14 | `harbour.chain_wine_fleet.end_own` | Rotomagus, October to March; drawn | the cellar, half and half / buy the dead man's ship (400 d; only if Radbern died) / the cellar, and a third sent to Aldburg | `ch_wine_fleet_end_own` |

**When the wine turns.** The wine is sour at the broaching if: it was new wine and the bungs were driven tight at sea; or the casks were shaken (a failed run) and never filled by the cooper; or it was new wine, sailed late (you stayed at the hospice), and was neither nursed at sea nor filled by the cooper. Otherwise it is sound. Every one of these was warned of first: by the cellarer in card 4, the hospice brother in card 5, the cooper in card 6 and the mate in card 7. Old wine with a driven bung is safe, as the cellarer says.

**No dead ends.** Every card has a road that costs only time or pride (not this year, lie over a tide, ask at the gate, choose a wine, carry or stay, keep your pennies, nurse the casks, sell on the quay, sell as vinegar or own the fault, a short reckoning, your own purse, take your share, a carrier's wage, the cellar). Cards 2, 6 and 7 are optional: later cards never ask for their flags. A failed chance hurts and goes on. The spoiled cargo still pays (vinegar), still reaches the reckoning, and all three endings stay open after it. At card 11 "your own purse" is always open, so a player who gave a short reckoning still has an ending.

## The endings

| Ending | Flag | How it is reached | What becomes of the people and the place |
|---|---|---|---|
| **Fellowship** | `ch_wine_fleet_end_fellowship` | An honest reckoning, then the house again at the next fair. | The ship and the boat keep one tally; Aldburg runs the house, with her father in the stern if you stayed by him at the hospice, or in the monks' ground if you did not. On the hard road the Frisians and the English together win a strand of their own from the abbot. |
| **The abbey's man** | `ch_wine_fleet_end_abbey` | The abbot's errand at the next fair, carried to Lundenwic. | You become Saint Denis's shipman. On the hard road the saint has a real landing place on the Strand and you keep it, owing a cask a year (`ch_wine_fleet_abbey_due`). Otherwise the landing place stays a claim, and you trade toll-free at the fair, or keep a corner of the storehouse. |
| **Your own purse** | `ch_wine_fleet_end_own` | The Englishman's offer at the next fair. | A cellar at Rotomagus with Leofstan. Radbern's house is left cold (`ch_wine_fleet_house_cold`), is ended, its ship bought from the daughter and its place on the dam sold (`ch_wine_fleet_house_ended`), or is mended by a third sent unasked (`ch_wine_fleet_house_mended`). |

The spoiled cargo is not an ending but colours all three: the daughter, the cellarer and the steward each have a line for it, and it shuts the hard road of the fellowship.

**What really happened, and what the player changes.**

- What really happened is the plain road of each ending: Frisians and English came each October, paid the saint's tolls, and went home ("take your share"); Saint-Denis held no proved plot at Lundenwic ("ask the abbot's writing for yourself"); a merchant went alone.
- `changed_saint_denis_lundenwic` (ending 2, the hard road): the landing place the forged charter claims becomes true, let by the reeve on the player's surety about 797 to 804. What would follow: the abbey needs no forgery; Seine wine has a standing house on the Strand two centuries before the men of Rouen appear in the London tolls of about 1000; when the trade moves inside the walls after 886 the saint's plot has a claim to move with it (chain thread_07), and when the Northmen are on the Seine from 841 the abbey has a refuge and a creditor over the sea. Later cards at Lundenwic and Parisius may read the flag.
- `changed_saint_denis_strand` (ending 1, the hard road): the abbot grants the merchants from over the sea a fixed strand, a plot for their casks, and a toll by the keel instead of by the cask, in writing. Nothing like it is recorded; the abbey kept every toll and the merchants had only the king's judgment of 753. What would follow: a body of foreign merchants with a written right on the Seine a generation before the raids; in 845 and 861, when merchants flee upriver by boat, they have a place and a patron; the fair's toll by the keel favours big ships, so bigger Frisian hulls on the Seine. Later cards of the Seine chains may read the flag.

## Payouts

Scale (CHAINS.md): step 0-40 d, turn 80-250 d, finale 600-2,500 d with a grand reward, hard road up to twice with a second grand reward.

| Card | Option | Silver and goods | And |
|---|---|---|---|
| 1 | share | 4 bolts of Frisian cloth to carry | his feeling +8 |
| 1 | own | +20 d, 2 bolts | |
| 1 | later | | a true price word: Frisian cloth x1.2 at Parisius for 90 days |
| 2 | declare | -6 d | toll eased 30 days |
| 2 | pilgrim | doubtful: nothing paid, or -12 d and a deed (goods seized) | |
| 2 | river | | the river man's feeling +6 |
| 3 | pay | -16 d | |
| 3 | judgment | likely: toll eased 30 days, deed won_case; else -8 d | opens the abbot's roads |
| 3 | gate | | opens the abbot's roads |
| 4 | any | +90 d (your part of the cloth: a turn); 3 or 2 casks of Seine wine (about 50 d each at Dorestad, 80 d at Lundenwic) | Aldhelm's riddle of the wine cask |
| 5 | stay | | Radbern lives; a little Latin (+0.05) |
| 5 | sell | +240 d (his, until the reckoning) | |
| 6 | top | -12 d | the wine is safe from shaking and lateness |
| 6 | hire | -30 d | a cooper hired (a turn: a man with a skill); the wine is safe as above |
| 6 | pass | | a true price word: Seine wine x1.2 at Lundenwic for 40 days |
| 8 | quay | +180 d (a turn) | |
| 8 | order | +110 d | an order: 3 casks at 50% over, 120 days |
| 8 | mass | +150 d, less one cask | deed gave_alms; the priest's feeling +12 |
| 9 | vinegar | +45 d | |
| 9 | spice | -10 d; doubtful: +140 d, else +20 d | Symphosius's riddle of spiced wine (text) |
| 9 | truth | +30 d | deed fair_dealing; the steward's feeling +8 |
| 10 | tally | +120 d | deed kept_commission; opens the fellowship |
| 10 | silver | -240 d, +100 d | deed kept_commission; opens the fellowship |
| 10 | short | +90 d (and the 240 d, if you sold) | shuts the fellowship |
| 11 | abbey, own | 2 casks | |
| **12** | hands | **+900 d** | `unlock_partner_radberns_house`, `unlock_pilot_seine` |
| **12** | strand (hard) | **+1,500 d** | the same two, and `unlock_plot_parisius`; `changed_saint_denis_strand` |
| **12** | share | **+1,100 d** | `unlock_pilot_seine` |
| **13** | plot (hard) | -60 d, **+1,300 d** | `unlock_plot_lundenwic`, `unlock_title_saint_denis_man`; `changed_saint_denis_lundenwic`; cost `ch_wine_fleet_abbey_due` |
| **13** | charter | **+900 d** | `unlock_charter_parisius` |
| **13** | wage | **+600 d** | `unlock_factor_parisius`; deed kept_commission |
| **14** | cellar | **+800 d** | `unlock_plot_rotomagus`, `unlock_partner_leofstan` |
| **14** | ship | -400 d for a ship worth several thousand | `unlock_ship_north_sea_merchant_ship`, `unlock_plot_rotomagus` |
| **14** | third | **+600 d** | `unlock_plot_rotomagus`; deed fair_dealing |

**Why a merchant could really have won these.** A sworn partner with a second boat is the ordinary way of the trade. Knowing the Seine from the sea to the abbey is what twenty Octobers buys. Toll freedom as an abbey's own merchant is exactly what Saint-Germain's men had from 779. A corner of an abbey's storehouse, a cellar under a town wall and a let plot on a foreign strand are the kinds of holding the charters of the age record for churches and their men. A sea ship bought cheap from a house with no master is a hard bargain, not a marvel.

## Pictures

One for each step (the `art.<card id>` entries in the writing file):

1. `s01_second_keel`: the dam head at Dorestad on a late summer evening; the old shipman looks down into the small boat; his daughter holds a split hazel stick; a broken boat on the bank.
2. `s02_tide_and_toll`: the row of ships under the Roman wall of Rotomagus in autumn mist; the toll man and his boy; a very still "pilgrim" with a large bundle.
3. `s03_saints_day`: the crowded strand below the abbey church on a bright October morning; two hands held out to one merchant.
4. `s04_cellar`: the long stone storehouse; the cellarer with his ear to a cask and one finger raised.
5. `s05_fever`: the hospice by rushlight; half a hazel stick pressed into a hand; the Englishman in the doorway with a purse.
6. `s06_cooper`: the cooper kneeling in the boat, knuckle raised to rap a cask.
7. `s07_frost`: the small boat in a grey winter sea at dawn, ice on the ropes, a man with a mallet over the casks.
8. `s08_broach_sound`: the winter quay; the steward drinks with his eyes shut; the priest leans in.
9. `s09_broach_sour`: the same quay; the cup at arm's length; sprung staves; the salter counting on his fingers.
10. `s10_reckoning`: the Frisian house in winter; half a tally on a bare table; the daughter looking up.
11. `s11_next_october`: the strand again; three people look at the merchant from three sides.

One for each ending:

- **Fellowship** (`end_fellowship`): the emptying strand under a November sky; a man and a woman clasp hands over a cask head before a half-ring of shipmen; the abbey church behind.
- **The abbey's man** (`end_abbey`): the beach at Lundenwic on a winter afternoon; the reeve with a silver cup, a clerk reading, two casks with a burned mark.
- **Your own purse** (`end_own`): the arched cellar door in the foot of the wall at Rotomagus; the Englishman holds up a key; a steersman watches from a larger ship.

## How long it takes

At the least fifteen months of game time: the asking in late summer, the fair in October, the reckoning in winter, the next fair a year on, and the ending in that autumn or winter (at Lundenwic by spring). Started in July 796 it ends between November 797 and April 798. The eight waits are real ones: the tides after Michaelmas, the saint's day, three days to the buying, twelve to the fever, the frost months at sea, four days to the reckoning, two hundred days and the next October, and the close of the fair. A player who misses a fair adds a year; the chain's start closes after 802 and nothing in it needs a year later than about 805.
