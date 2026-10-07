# Chain: house_new_ship (the new ship)

Cards: `Assets/Kapman/Game/Data/encounters/chain_house_new_ship_793.json` (14 cards, written 4 Oct 2026).
Words: `Assets/Kapman/Game/Data/writing/chain_house_new_ship.json` (232 entries, written 6 Oct 2026).
A draft for the user to overwrite. (?) marks what could not be checked against a source.

## The story in five sentences

The wright at Dorestad's dam heads tells a house that has outgrown its four-oared boat to order a keel ship, takes a pound of pennies in earnest, and sends the merchant off with a notched stick to find the stuff himself.
The oak is bought standing in the king's wood across the river from Colonia and rafted down, the rivets come from the heath iron of Daventre, and the sail is four women and a winter on the looms of the terp at Wijnaldum.
Through the winter the wright will not be hurried, a Dane off a Ripa ship says the hull is a trough and offers to line off a lighter one, and the count rides down to the slip to ask whose ship she is.
Every corner cut on the way (dry windfall oak, twisted trunks, old rivets from the breaking beach, a sail of cloak-cloth, work by torchlight on feast days) is remembered in the hull.
In spring she goes down the rollers as the Frisian keel ship the finds show, or as a patched ship the wright will not mark, or as a long ship in the Danish manner built forty years too early, or she was never built and the house has a river ship of one tree at Colonia, or she was given to the count for the king's service.

## How a player meets it

- It starts by itself at Dorestad, the home port: card `s01_wright` is drawn on a call there between July and November, in any year before 833, once the house has called twice before and has 240 d aboard (weight 1.0; it comes again every thirty days while the player only thinks about it).
- It does not come to a house that already has the right to the sea trader (`unlock_ship_north_sea_merchant_ship`), or that has ended this chain (`ch_house_new_ship_done`).
- After the order, three errands lie open at once and in any order: Colonia (oak), Daventre (iron), Wijnaldum (sail). Each is a card with weight 30 on the first call there. The news `chain_house_new_ship.news.ordered` is posted at Dorestad.
- The epilogue, `s14_penny`, is drawn at Dorestad in 812 to 818 for a house that launched a sea ship here.

## The real history under it

**The ship.**
- Dorestad's own sea-going find: strakes and a floor timber of a keeled clinker ship from the northern harbour (Hoogstraat II, dug 1973-74), iron rivets, sheep's wool in the seams, oak felled after about 739; she seems to have sunk where she lay. https://mass.cultureelerfgoed.nl/dorestad-shipfragment ; Brouwers, Jansma and Manders 2015 (in `Design/content/ships.md` 2.1). Card 3 lets the player raise her knees and floors; that she still lay there in the 790s and could be robbed is the game's own (?).
- The class built is `north_sea_merchant_ship` (`Data/ship_classes_793.json`: 4 oars, 5 men, 6 t). Fourteen oaks, about 100 kg of iron (two thousand rivets at 50 g with the rove), a sail of about 50 square metres, ten men and a winter: derived from the measured Roskilde replicas (`ships.md` 4.10, 7.1-7.2). The keel's length in feet (34) and the notched stick are invented.
- Ordered by Martinmas, afloat by Easter, the buyer finding timber, iron and sail; 2,700 d new: `Data/story/town_actions_793.json`, build_ship.
- Planks cleft with wedges and hewn with the axe, worked green; a green hull "takes up" as it swells; old hulls mended with let-in lengths (Skuldelev 3) and re-riveted (Roar Ege: half her rivets in thirty-one years): `ships.md` 7.1, 7.3. These are later ships and replica builders' experience (?) carried back to 800.

**The oak.**
- Ship timber in the delta before about 850 is oak from the German Rhineland, and many hulls were built upstream: Brouwers, Jansma and Manders 2015 (`ships.md` 2.1).
- The royal estate in the old fort across from Colonia (Deutz): `dossiers/colonia.md` 1. The king's woods kept and his clearings cleared: Capitulare de villis c. 36, about 800 (chapter number from memory (?)).
- Felling after midsummer and before the turn of the year, from the fifteenth to the twenty-second day of the moon: Vegetius, Epitoma rei militaris IV.35-36 (https://www.thelatinlibrary.com/vegetius4.html), given in the steward's own words. That a steward had heard of the book is invented.
- Rafting: Bede has timber brought down the Tyne on rafts about 721; a raft on the Rhine in the 790s is likely and not recorded (?). The men away in Saxony: the Saxon war ends in 804.

**The iron.** Bloomery iron from the brown stones of the Veluwe heath, west of the IJssel, smelted with charcoal; Veluwe iron reaches Dorestad (`goods_793.json` bar_iron; `dossiers/dorestad.md` 4; https://nl.wikipedia.org/wiki/Veluwe). The emperor's order for ships, 810-811: Annales regni Francorum 811; Einhard, Vita Karoli 17 (`chronicle_793.json` 810-fleet-ordered-810).

**The sail.**
- Sunken huts with loom weights and spindle whorls on the terp at Wijnaldum until about 850, sheep the commonest bones: `dossiers/wijnaldum.md` 2-3.
- The Lex Frisionum (Additio sapientum) prices the hand of a woman who weaves "fresum" with a harper's and a goldsmith's, and sets death or ransom on breaking into a weaving hut: `dossiers/wijnaldum.md`, `dossiers/doccinga.md`; http://www.keesn.nl/lex/lex_en_text.htm.
- No servile work on the Lord's day, and for women no weaving, sewing, carding or shearing: Admonitio generalis 789 c. 81 (MGH Capit. I no. 22; from memory, not opened (?)). The Frisians' fine for Sunday work: Lex Frisionum tit. XVIII (from memory (?)); the game's fine is 12 d.
- About 1.1 kg of wool and 100 to 130 hours of work the square metre: Bender Jorgensen 2012 and the sails of Roar Ege and Ottar (`ships.md` 7.1-7.2). Sewn of narrow lengths, of the long outer wool, dressed with fat and red ochre: the replica makers' practice (?).

**The Dane's ship.**
- Two patterns were known: Alfred's long ships of 896 were "shapen neither like the Frisian nor the Danish" (Anglo-Saxon Chronicle 897, tr. Giles 1847; not quoted in the cards). The class is `norse_allround_ship` (16 oars, 12 men, 4 t).
- Ripa and Dorestad traded (the finds at both); a Danish hand wintering at Dorestad about 800 is likely and not recorded (?). Godfred with his fleet at Sliesthorp, 804, and the war to 810: Annales regni Francorum (`chronicle_793.json`).

**The count.** Boats hauled overland into Saxony, 797 (Annales Guelferbytani); a fleet and guards on the coast, March 800 (Annales regni Francorum); ships to be made ready, 802 (MGH Capit. I no. 34 c. 13) and 808 (no. 51 c. 9-10); the fleet of 810-811. All in `chronicle_793.json`. That a count took or was given a merchant's hull is invented; no count of Dorestad is named in any source.

**The river ship.** The Utrecht type: a logboat hollowed thin, spread, raised with planks, pegged with wood, a small mast step forward for the tow line (Vleuten 1, felled after about 734; Utrecht 1: `ships.md` 2.1, 4.6; https://mass.cultureelerfgoed.nl/utrecht-1). Class `logboat_expanded`. Hauling upstream by rope and the Frisian skippers at Saint Goar's cell: Wandalbert of Prum, Miracula sancti Goaris (839, of earlier years: `dossiers/mogontia.md`). No new toll where only a rope is stretched: Thionville, December 805 (the line is gated to 806 and after).

**The penny.** Dorestad's and Quentovic's pennies of 812-818 carry a ship; before the reform of 793-794 Dorestad's light penny carried an axe (`dossiers/dorestad.md` 4). That the die-cutter drew the player's ship is invented.

**The launching.** A blessing of a ship that names Noah's ark stands in the Romano-German pontifical of about 950, and is said to stand in Frankish books of the 790s (not opened (?)); the priest's words are our own rendering, not a translation. His riddle is Symphosius 13, Navis (`texts_793.json` symphosius_13_navis; Riese 1894), also in our rendering.

## The people

Nobody is named in the words: the forge names the invented ones.

| Who | Actor role | Where | Note |
|---|---|---|---|
| The wright of the dam heads | shipwright | Dorestad | invented; Frisian, 40 to 60; cards 1, 3, 7, 8, 10, 11, 12 |
| The steward of the king's estate | steward | Colonia | invented; Frankish; cards 2, 13 |
| The smith on the IJssel bank | smith | Daventre | invented; card 4 |
| The woman who speaks for the weavers | weaver | Wijnaldum | invented; Frisian; cards 5, 6 |
| The Dane off the Ripa ship | sailor | Dorestad | invented; a wright's son; cards 7, 12; can be hired at the end |
| The count | count | Dorestad | `cast.folcbert` in his years (to about 812), then the forge's count; card 9 |
| The priest of the upper church | priest | Dorestad | invented; card 10 |
| The wright of the river bank | shipwright | Colonia | invented; card 13 |
| The moneyer | moneyer | Dorestad | `cast.sigimar` in his years; card 14 |
| The other bidder for the sail | (not seen) | Wijnaldum | `cast.thiadbod`, the rival of chain house_rival; spoken of in card 6, not named |
| The house's own Northman | (not seen) | Dorestad | `cast.crew.ravn`; one narrated line in card 7, shown when Norse is spoken aboard |

## The steps

All cards are stage `harbour`. Flags are written without the prefix `ch_house_new_ship_` where a short name is given in brackets.

| # | Card id | Where and when | The choices | Flags set |
|---|---|---|---|---|
| 1 | `s01_wright` | Dorestad, July to November, before 833, two earlier calls, 240 d aboard | a pound in earnest (240 d); half a pound and your name (120 d, standing known); ask the cost and think (time only) | `ch_house_new_ship_1` |
| 2 | `s02_oak` | Colonia, any call after 1 | fourteen standing oaks (280 d, raft in 50 days); windfalls (110 d, 25 days, flawed); lend the crew (no silver, health and heart, 65 days); the great oak for a river ship (160 d, the river road) | `_2`, or `_2r` (river); `_dry_oak` + `_flawed` |
| 3 | `s03_keel` | Dorestad, scheduled when the raft is in | three more oaks (60 d); raise the old ship's knees (standing known, health); build with what came (pride, flawed) | `_3`; `_frost` for 20 or 30 days; `changed_dorestad_old_keel`; `_twist` + `_flawed` |
| 4 | `s04_iron` | Daventre, any call after 1 and before 8 | ten kegs (130 d); carry blooms for a week and pay half (65 d, health); nothing today (time only) | `_4`; ten kegs of `rivets` in the hold |
| 5 | `s05_weavers` | Wijnaldum, any call after 1 | order the sail, half now (90 d); pay the half in two querns and a cask of wine; cloak-cloth sewn today (110 d, flawed); not yet (time only) | `_5`; `_6` + `_cloak_sail` + `_flawed` on the quick road |
| 6 | `s06_sail` | Wijnaldum, scheduled 110 days after 5 | pay the second half (90 d); pay and match the rival's offer as a gift (135 d: the turn's reward); haggle to two thirds (doubtful: the sail is lost on a failure); leave it (20 days) | `_6`; `_sail_good` |
| 7 | `s07_lines` | Dorestad, when `_frost` has lapsed | keep the wright's ship; let the Dane line her off (40 d); ask the wright before his men what he would take from the Dane (likely; a riddle) | `_7` with `_7a` (Frisian) or `_7b` (Danish); `_strakes` for 25 days; `_iron_wait` for 60 |
| 8 | `s08_rivets` | Dorestad, when `_strakes` has lapsed and ten kegs are aboard (or 60 days are gone) | his own pace (90 days); four more men (100 d, 60 days); feast days and torchlight (30 days, flawed, doubtful that the priest does not hear); no iron: old rivets (flawed) | `_8`; `_slip` for 30, 60 or 90 days; `_planking` for 10 or 15; `_rushed`, `_old_iron`, `_flawed` |
| 9 | `s09_count` | Dorestad, when `_planking` has lapsed | keep her, with respect; a carrying a year for an easier toll; the carrying for a year's delay of the debt (only with a debt); give her to the count (**ending count**) | `_9`; `_counts_carrying`; `db_extension`; or the ending's flags |
| 10 | `s10_launch` | Dorestad, `_slip` lapsed, pattern 7a, not flawed | launch with the terp sail (hard road, wright friendly, 300 d); launch under any sailcloth (300 d); launch under the old boat's sail (300 d); not yet | **ending keel** |
| 11 | `s11_patched` | Dorestad, `_slip` lapsed, pattern 7a, flawed | launch her as she is (220 d, likely she holds); a month and 60 d to put it right (then card 10, without its hard road); not yet | **ending patched**, or `_mended` and `_flawed` cleared |
| 12 | `s12_dane` | Dorestad, `_slip` lapsed, pattern 7b | sail on, pay, and ask the Dane to steer (sound hull, 300 d); pay and launch (300 d); not yet | **ending dane** |
| 13 | `s13_river` | Colonia, scheduled 100 days after the great oak | pay the wright and take the estate's carrying (120 d); pay and owe nothing (120 d); leave her (15 days) | **ending river** |
| 14 | `s14_penny` | Dorestad, 812 to 818, after a sea-ship ending | two worn pennies for the first struck; ask it as a gift (moneyer friendly); a penny is a penny | `_penny`; `unlock_heirloom_ship_penny` |

No dead ends: every card has a choice that costs only time or pride; a lost sail (card 6) can be replaced with bought sailcloth or the old boat's sail; a flawed hull can be launched or mended; a house without silver at the launch waits four days at a time, and the ship keeps.

## The endings

| Ending | Flag | How | What becomes of it | Finale |
|---|---|---|---|---|
| **The keel ship** (what the finds show) | `ch_house_new_ship_end_keel` | The wright's pattern, nothing skimped (or mended), launched in spring | A Frisian keeled trader of Dorestad, of the kind whose timbers were dug up in 1973-74 and whose picture is on the pennies of 812-818. | `unlock_ship_north_sea_merchant_ship` (worth 2,700 d). Hard road also `unlock_work_dorestad_slip`, the wright's friendship, a commission for tar. |
| **The patched ship** | `ch_house_new_ship_end_patched` | A flawed hull launched as she is | The house has its ship and a bailer that is never dry; the wright will not put his mark on her. | `unlock_ship_north_sea_merchant_ship`, with `ch_house_new_ship_weeps` as the cost that lasts. |
| **The long ship** (history changed) | `ch_house_new_ship_end_dane` | The Dane lined her off | A northern ship built by a Frisian at Dorestad about 800, and perhaps a Dane at the steering oar. | `unlock_ship_norse_allround_ship`. Hard road also `unlock_pilot_ripa`, the Dane hired, half the Norse tongue, a riddle. Cost: `ch_house_new_ship_danish_hull`. |
| **The river house** | `ch_house_new_ship_end_river` | The great oak bought at Colonia | No sea ship: the house turns to the Rhine, and the Dorestad wright keeps the earnest. | `unlock_ship_logboat_expanded`. With the carrying also `unlock_pilot_rhine`, three casks of wine and a commission for sixteen bolts of cloaks at a premium. |
| **The count's shipman** | `ch_house_new_ship_end_count` | The hull given for the king's service | The ship sails for the king; the house has land, standing and a lord. | 900 d, `unlock_title_counts_shipman`, `unlock_plot_dorestad`, half toll at Dorestad for a year. |

Every ending also sets `ch_house_new_ship_done`.

**Why a merchant could really have won these.** A ship built to order is the ordinary way to get one. A slip on one's own dam is a strip of sand and blocks. A felag's steersman who knows his home water is how pilotage was had. The estate's carrying is the ordinary tie between a royal fisc and a Frisian skipper (the Frisians of the Rhine in Wandalbert). A count rewarding a man who served the king's fleet with land and protection is the Carolingian pattern of service; the particular gift is invented.

## History changed

- **`changed_dorestad_dane_yard`** (ending dane). Really: no Northman is in Dorestad's record before the raids of 834, and the digs see Scandinavian craft there only after about 850. If a Frisian wright builds on the northern pattern about 800, then by the 830s Dorestad's own houses could have ships that can row away from a raider, the count's coast watch (card 9) has hulls that can catch one, and the wright's sons build both kinds. Later cards (dorestad_834, the coast-watch cards) can read the flag: a town that is harder to surprise, and one that Franks trust a little less.
- **`changed_dorestad_old_keel`** (card 3, a small thing). Really: the old keel ship lay in the northern harbour until 1973. If her knees and floors are raised and built into a new hull, the diggers find less, and the easter egg about the find should show four strakes and no floor timber.
- **`ch_house_new_ship_end_count`** changes nothing in the record (the fleet of 810-811 was built; whose hulls it had is not known) but ties the house to the count's side in the chains that follow.
- What really happened is the keel ending: Frisian keel ships were built at Dorestad of Rhineland oak with iron rivets and wool in the seams, and one is on the penny.

## Payouts

| Step | Choice | Costs | Gives |
|---|---|---|---|
| 1 | earnest / half | 240 d / 120 d | the order; wright's feeling +8 / +2; news at Dorestad |
| 2 | standing / windfall / axes / trunk | 280 / 110 / 0 (health, heart) / 160 d | the raft in 50 / 25 / 65 days, or the river road; steward's feeling +5 / +1 / +10 / +5 |
| 3 | more / old keel / make do | 60 d / health / pride | the keel laid; wright's feeling +5 / +3 / -6; news at Dorestad |
| 4 | gauge / carry | 130 d / 65 d and health | 10 kegs of rivets (table price 16 d a keg); smith's feeling +5 / +10 |
| 5 | order / barter / ready | 90 d / 2 querns and a cask / 110 d | the sail in 110 days (weaver +5 / +12), or 4 bolts of sailcloth now |
| 6 (turn) | pay / gift / haggle | 90 / 135 / 60 d | 4 bolts of sailcloth, deed fair_dealing; with the gift: weaver +20, a commission for 4 casks of wine at a premium of half, and the terp's cloaks a fifth cheaper for 150 days |
| 7 | Frisian / Danish / both | 0 / 40 d / 0 | the pattern; on "both" (success) riddle `r-gest-06`, wright +12 |
| 8 | pace / hands / press / old iron | kegs / kegs + 100 d / kegs / nothing | the launch in 90 / 60 / 30 / 90 days; a pressed build risks a 12 d fine and deed lost_case |
| 9 (turn) | carry / carry for the debt | a duty that lasts | half toll at Dorestad for 180 days / a year's delay of the debt (`db_extension`) |
| 9 (finale) | give | the ship | 900 d, title, plot, half toll for 365 days, count +30 |
| 10 (finale) | full / plain / bare | 300 d (+ sail) | the sea trader; text `symphosius_13_navis`; full: the slip, deed kept_commission, commission for 6 barrels of tar at a premium of half, wright +20 |
| 11 (finale) | launch / mend | 220 d / 60 d and 30 days | the sea trader, weeping / the way back to card 10 |
| 12 (finale) | with the Dane / plain | 300 d (+ sail) / 300 d | the long ship; with the Dane: pilot of Ripa, the Dane hired, Norse 0.5, riddle `r-gest-06` |
| 13 (finale) | carry / plain | 120 d | the river ship; with the carrying: pilot of the Rhine, 3 casks of wine, commission for 16 bolts of cloaks at a premium of 0.6 |
| 14 | keep / gift | 2 d / nothing | `unlock_heirloom_ship_penny` |

**The reckoning.** The honest road to the keel ship costs 240 + 280 + 60 + 130 + 180 + 300 = 1,190 d for a ship that costs 2,700 d to order outright: about 1,500 d of worth, and the slip. The poor man's road (half earnest, the crew's axes, the old ship's knees, carried blooms, querns for the sail) costs about 575 d and a good deal of sweat for the same ship. The river road costs 520 d in all (the earnest is lost). The count's road pays back 900 d in silver against an outlay of about 700 to 900 d by that point, and adds land and a title.

## How long it takes

From the order to the launch, at the least: the voyage to Colonia, 50 days for the raft, 30 for the keel, 25 for the first strakes, 90 at the wright's pace: about 200 days, so an order at Martinmas swims about Whitsun and an order in August swims before Easter. The sail takes 110 days from the day it is ordered, and the iron must be at Dorestad within 60 days of card 7. Pressed and skimped, about 110 days. The river road is 100 days from the buying of the oak. The epilogue comes between 812 and 818, which for a house that builds in the 790s is its second decade.

## Pictures

One for each card (the full lines are the `art.<card id>` entries of the writing file):

1. `s01_wright`: the empty slip at the dam heads, late summer; the wright notching a hazel stick; the small boat deep in the water behind.
2. `s02_oak`: the king's oak wood in autumn mist; the steward with his hand on the one enormous oak.
3. `s03_keel`: the raft low in the water with a Frank asleep on it; the wright listening to a trunk; men cleaving with wedges.
4. `s04_iron`: the IJssel bank under the heath's smoke; the smith trying a rivet against the stick; ten kegs.
5. `s05_weavers`: inside a sunken weaving hut; the loom and its weights; the weaver looking at her hands.
6. `s06_sail`: the great grey sail folded on a bench; the red thread of knots; the women along the wall.
7. `s07_lines`: frost; the keel, stems and first strakes; the Dane pointing along her, the wright with his adze.
8. `s08_rivets`: midwinter dusk, snow; four strakes a side; the wright sitting on a keg, not to be moved.
9. `s09_count`: the count with his thumbs in his belt before the hull; the wright's men all at the far end.
10. `s10_launch`: spring; the ship tarred black on tallowed rollers; the priest with his book; the town on the ropes.
11. `s11_patched`: rain coming; the wright with folded arms under the stem; a boy with a bailer.
12. `s12_dane`: the long low ship among the broad Frisian hulls; the Dane at the steering oar; the count's spearmen apart.
13. `s13_river`: the one-tree boat on the shingle below the old fort; the embers of the long fire; three casks.
14. `s14_penny`: new pennies with a ship on a board; the moneyer holding one up; a wax tablet with a ship scratched on it.

One for each ending:

- **Keel:** the new ship at her post under a grey sail, the small boat beside her, the wright's mark just visible under the stem at low water.
- **Patched:** the same ship at dusk with a lantern in her hold and a man bailing over the side.
- **Dane:** the long ship going down the Rhine under oars and sail, a watchman on a dune standing up to look.
- **River:** the one-tree ship hauled upstream by men on a tow path, casks aboard, the rock and cell of Saint Goar ahead.
- **Count:** the hull on the slip with the count's spearmen around her, and the merchant pacing out a plot at the dam heads with the steward.

## Flags other writers may read

- Endings: `ch_house_new_ship_end_keel`, `_end_patched`, `_end_dane`, `_end_river`, `_end_count`; all set `ch_house_new_ship_done`.
- History: `changed_dorestad_dane_yard`, `changed_dorestad_old_keel`.
- Costs that last: `ch_house_new_ship_weeps` (a leaking hull), `ch_house_new_ship_danish_hull` (taken for a raider on Frankish coasts), `ch_house_new_ship_small_sail` (slow until a sail is fitted), `ch_house_new_ship_counts_carrying` (one carrying a year at the count's word).
- Grand rewards: `unlock_ship_north_sea_merchant_ship`, `unlock_ship_norse_allround_ship`, `unlock_ship_logboat_expanded`, `unlock_work_dorestad_slip`, `unlock_pilot_ripa`, `unlock_pilot_rhine`, `unlock_title_counts_shipman`, `unlock_plot_dorestad`, `unlock_heirloom_ship_penny`.
- From the spine: `db_extension` is set by card 9 (thread counts_day).

## Needs (not added by this writer)

- The engine shows the effects' own facts, not the `out_<option>` texts: the outcome keys are written and named in each option's `note`, for the integrator.
- Something must read the four lasting-cost flags above (a bailing card at sea, a coast-watch card, a speed penalty, the count's yearly carrying); none is written here.
- `unlock_title_counts_shipman`, `unlock_work_dorestad_slip`, `unlock_pilot_ripa` and `unlock_pilot_rhine` are new names for the game to wire.
- The line `s07.northman` shows on `tongue.old_norse >= 1`; a variable for "a Northman is in the crew" would be truer.
- The ship itself: the unlock flag gives the right to the class; whether the finale hands over the hull at once is the game's to decide (the cards assume it does).
