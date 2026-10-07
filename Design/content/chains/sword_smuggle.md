# Chain: sword_smuggle (swords under the ballast)

Cards: `Assets/Kapman/Game/Data/encounters/chain_sword_smuggle_793.json` (13 cards).
Words: `Assets/Kapman/Game/Data/writing/chain_sword_smuggle.json` (196 entries).
The spine's thread: `swords_north`. Written 6 Oct 2026. A draft for the user to overwrite. (?) marks what could not be checked against a source; no web search was made for this chain, so everything rests on the project's own tables and on the spine.

## The story in five sentences

Reginbald, a swordsmith of Colonia whom the count pays late and short, shows a Frisian shipman four blades that were never counted, each with his mark below the hilt, a hammer in a ring.
The blades must pass the count's search at Dorestad and the shipman's own crew before they reach Asulf, a dealer in people on the Slie, who pays in weighed silver or in captives and never says a price twice.
A second cargo, eight blades in a wine cask, must pass the watch boat at the river mouth, and at Sliesthorp a lord's man with gold on his arm asks for more than blades: he wants the smith.
Four months later a Frisian eel fisher lies dead in his boat on a landing with a Frankish sword left in him, and the mark below the hilt is a hammer in a ring.
Then the king's men come to Dorestad asking who carries blades north, and the shipman chooses: go on carrying, give them the road, take the smith north, or lay the blade down at Saint Martin's.

## How a player meets it

- It starts by itself at **Colonia** from 795 to 807: card `s01_smiths_street` is drawn on a call there once the house has 120 d aboard (weight 0.8, once). Colonia is where a Frisian goes for wine, querns and glass, so a trading house finds it in its first seasons.
- **A player who refuses the blades is still in the story.** The search at Dorestad comes anyway (he was seen on the smiths' street), the dealer makes him a lawful offer, and the blade that comes back is one he knew of and did not carry. Three of the four endings are open to him; the runner's ending is open to him too, if he chooses then to begin.
- Steps 4, 5, 7 and 8 are drawn with a high weight (4 to 5) at their ports, so they come on the next call there. Steps 2, 9 and the four endings are scheduled.
- Other chains can read `sw_ran_blades`, `sw_marked` and `sw_buyer_known` (the spine's flags), which this chain sets where their meaning is met.

## The real history under it

**The bans.**
- Capitulary of Herstal, March 779, c. 20: byrnies (bruniae) are not to be sold outside the realm (MGH Capitularia I no. 20). The letter of the law names mail and not swords; in the game the officers stretch it to blades (`trade_rules.json` law `arms`, from 779), and the cards say so in the toll man's mouth.
- Capitulary for the missi, 803, c. 7: no arm-rings or byrnies to be given to merchants (MGH Capit. I no. 40; `chronicle_793.json` 803-arms-to-merchants-803).
- Thionville, December 805, c. 7 (MGH Capit. I no. 44; chronicle 805-12-thionville-border-posts): merchants going to the Slavs and Avars may go only as far as the named border posts and are to carry no arms or byrnies for sale; if found with them they lose all their goods, half to the palace, the other half shared between the officer and the finder. This is the finder's share of cards 2, 9 and 11. That the same rule was applied to a boat bound for the Danes is the game's reading (?).
- Thionville 805 c. 6: every man with twelve manses is to have a byrnie (chronicle 805-12-thionville-arms): the smith's line in card 5, shown from 806.
- Boulogne, October 811: bishops, abbots and abbesses are not to give or sell byrnies or swords to any outsider (MGH Capit. I no. 74; chronicle 811-10-boulogne-capitulary): the abbey steward's line in card 5.
- Pîtres, 25 June 864, c. 25: whoever gives a byrnie, arms or a horse to the Northmen dies as a traitor (MGH Capit. II no. 273; chronicle 864-06-25-pitres). It lies after this chain; the runner's ending names it ("his grandson on pain of death": Charles the Bald was Charlemagne's grandson).

**The blades.**
- Swords were made in the Rhineland and wanted from Ireland to the Volga (`goods_793.json` swords, sword_blades; `dossiers/colonia.md`: swords among Cologne's goods out). A sword with its scabbard is 7 solidi in the Ripuarian law, 84 d: the game's price. The forge prices here (45 d a bare blade, 50 d hilted) and the northern prices (four and five ounces of silver, about 65 d and 80 d) are set to the payout scale.
- About 170 blades inlaid +VLFBERH+T, of the 9th to 11th centuries, lie mostly in Norway, Sweden, the Baltic lands and Rus (Wikipedia, "Ulfberht swords", as read for the spine on 4 Oct 2026). Card 1 has the smith mention "a man up the river" who puts his whole name in the iron, from 800: who Ulfberht was and where he worked is not known (?), and the chain's own smith is given a different mark on purpose. Blades with the name inlaid wrongly are held to be copies made elsewhere (?): the ground under the ending "smith north".
- Reginbald's mark, a hammer in a ring in twisted wire, is INVENTED. Inlaid marks on Frankish blades of the time are known (?).
- Notker of St Gall, De Carolo Magno II.18 (tr. A. J. Grant 1907), tells of Northmen's envoys and a king testing swords by bending them point to hilt. The lord's man bends the blades in card 7 for that reason. Nothing is quoted, and the chapter number is from memory (?).

**The watch.** Einhard, Vita Karoli c. 17: Charles had a fleet built against the Northmen and set watches at the harbours and river mouths (from 800; ships ordered again in 810 and inspected at Boulogne and Ghent in 811). The watch boat of card 6 is that; before 800 the card calls it the count's, which is ours (?). `npc_text_encounters.md` sample 18 is the model of the search.

**The north.**
- Silver goes by weight north of the Eider: no coin there before about 825 (BRIEF.md rule 2). The ounce (eyrir, about 27 g) is the later Norse weight (?).
- Sliesthorp is first named in 804, when Godfred brought his fleet and horse there (Annales regni Francorum 804; chronicle 804-summer-godfred-sliesthorp); in 808 he destroyed Reric and moved its merchants to Sliesthorp (ARF 808; chronicle 808-summer-reric-destroyed): the dealer's line in card 4 from 808. Before 804 the lord's man in card 7 names no lord; from 804 he is the king's.
- A market in people at Hedeby and Birka: Anskar bought Danish and Slav boys (Rimbert, Vita Anskarii c. 15, tr. Robinson 1921). Heathen Wends could lawfully be carried and sold in Francia (`goods_793.json` captives); Christians could not.

**The blade that comes back.**
- 810: a fleet of two hundred ships from the land of the Northmen came to Frisia, wasted the islands, beat the Frisians in three fights and took a hundred pounds of silver as tribute (ARF 810; chronicle 810-summer-frisia-raid-810). If the player reaches card 8 in 810 the card says this, in our own words in the Chronicle's manner. In any other year it is "three ships in the outer channel", which is INVENTED: no sea robbery on the Frisian coast is recorded before 810 (?).
- The spine: "a blade sold in 800 can come back in 834 in the hand of a man burning Dorestad". This chain brings it back sooner and smaller, to one man in one boat, so that the player meets it inside the chain.

**The king's men.** Missi dominici sent in pairs, a churchman and a lay lord, strangers to the district: the capitulary of 802 (tr. Henderson 1896, avalon.law.yale.edu/medieval/capitula.asp). Before 802 the card has a stranger from the palace beside the count, which is ours (?).

**Saint Martin's.** The church of Saint Martin at Utrecht and its men on the banks at Dorestad, who owe the king's officers nothing: Louis the Pious for Utrecht, 18 March 815, confirming older grants (Oorkondenboek Sticht Utrecht I no. 56, in `dossiers/dorestad.md`); that it held before 815 is assumed (?). The blade made into a door hinge is ours.

**Invented, and said so.** Reginbald (Regin- + -bald) and his journeyman Hrodwin (Hrod- + -win), Franks of Colonia; Folcbod (Folc- + -bod) the eel fisher, his widow Liafburg (Liaf- + -burg) and his son Eilger (Eil- + -ger), Frisians of the coast; the count's man, the captain of the watch boat, the lord's man, the king's man, his clerk and the priest, unnamed. The wine shipper "of the lower town" taken in the king's-man ending is nameless and is not Thiadbod.

## The people

| Who | Id | In the cards |
|---|---|---|
| Reginbald, the smith | invented | actor `smith` (role smith), cards 1, 5, 12 |
| Hrodwin, his journeyman | invented | actor `journeyman` (role craftsman), card 5 |
| The count's man | of `cast.folcbert`'s following | actor `buyer`, `searcher` (role lords_man), cards 1, 2 |
| The toll man of Dorestad | the port's fixed toll man | actor `tollman`, cards 2, 9 |
| Aldger, Ravn, Brunger | `cast.crew.aldger`, `cast.crew.ravn`, `cast.crew.brunger` | actors `steersman`, `hand`, `mate`, card 3 |
| Asulf, the dealer in people | `cast.asulf` (to 815) | actor `dealer` (role slave_dealer), cards 4, 7, 10 |
| The lord's man | from 804 King Godfred's (`cast.godfred`) | actor `spokesman` (role warrior), cards 7, 10, 12 |
| The captain of the watch boat | unnamed | actor `captain` (role commander), card 6 |
| Liafburg and Eilger | invented | actors `widow`, `boy` (roles widow, child), cards 8, 13 |
| The king's man and his clerk | unnamed | actors `missus` (role kings_man), `clerk`, cards 9, 11 |
| A priest of Saint Martin's | unnamed | actor `priest`, card 13 |
| Thiadbod | `cast.thiadbod` | never on a card: "one at Dorestad who has never had a cask opened" (card 5), "a bigger house we did not touch" (card 11). Chain house_rival owns his fate. |

## The steps

Card ids are `<stage>.chain_sword_smuggle.<step>`. Every card sets `ch_sword_smuggle_<n>` for its number.

| # | Card | Where and when | The choices | Flags |
|---|---|---|---|---|
| 1 | `harbour ... s01_smiths_street` | Colonia, 795 to 807, 120 d aboard; drawn | Buy the four blades (180 d) / buy one for your own belt (50 d; a riddle) / drink his ale and buy nothing (a true price word) | `_carrying` or `_clean`; schedules 2 in 10 days |
| 2 | `harbour ... s02_search` | Dorestad; scheduled | Leave them under the stones (likely; caught: blades seized, a case, `sw_marked`) / thirty pennies to the toll man / sell them to the count at his price / lift your own boards (only with no blades) | `sw_ran_blades` or `sw_marked`; `_carrying` off if lost or sold |
| 3 | `crew ... s03_crews_word` | at sea with the blades aboard; drawn; passed over if the buyer is reached first | Promise the men a share (a riddle from the Northman) / put the bundle over the side / tell the steersman to mind the tide | `_crew_share`; `_clean` if overboard |
| 4 | `harbour ... s04_buyer` | Sliesthorp or Ripa; drawn | Sell for weighed silver (260 d) / take three captives / sell no blades and take his order for tools | `sw_buyer_known`, `_sold_north`; timer `_rest` 40 days |
| 5 | `harbour ... s05_the_cask` | Colonia, 40 days on; drawn | Buy the cask of eight (400 d) / buy six bundles of tools (90 d) / buy nothing and carry the boy's question | `_carrying` + `_big`, or `_tools`, or `_boy_asked` |
| 6 | `sea ... s06_watch_boat` | at sea after 5; drawn | Lie to with nothing aboard / lie to with the cask among the wine (likely) / run over the shoals (doubtful to likely; the boat is known either way) / roll the cask overboard first | `sw_ran_blades` or `sw_marked` |
| 7 | `harbour ... s07_the_offer` | Sliesthorp; drawn | Sell the eight blades (640 d) / sell the tools (270 d) / sell nothing and drink with them (a price word, half the Danish tongue) | `_offer`, `_sold_north`; timer `_wait` 120 days |
| 8 | `harbour ... s08_blade_comes_back` | Doccinga, Wijnaldum, Emuthon, Medemelacha, Traiectum or Dorestad, 120 days on; drawn | Tell the widow whose mark it is / tell her and lay down 100 d / say only that it is good, and buy it (60 d) | `_told` (+ `_paid_widow`) or `_silent`; schedules 9 in 30 days |
| 9 | `harbour ... s09_kings_men` | Dorestad; scheduled | Give them the road / say nothing and take the smith north / own it and take the blade to Saint Martin's / say you carry wine and go on (likely; if not believed, 60 d and `sw_marked`) | `_informed`, `_warned`, `_swore_off` or `_ran_on`; schedules one ending |
| 10 | `harbour ... s10_end_runner` | Sliesthorp, 60 days on | The plot by the water / silver and a guest's seat | ending `runner` |
| 11 | `harbour ... s11_end_finder` | Dorestad, 40 days on | The whole finder's share / a third, and the smith keeps his forge | ending `kings_man` |
| 12 | `harbour ... s12_end_smith_north` | Sliesthorp, 45 days on | A third of the forge's work / the first blade and a purse | ending `smith_north` |
| 13 | `harbour ... s13_end_laid_down` | Traiectum, 30 days on | Foster the boy / pay his keep | ending `laid_down` |

No dead ends: a caught cargo (cards 2, 6) costs the blades and a fine and moves the player onto the honest road with `_clean`; every card has a choice that costs only time or pride, except that a man who has chosen to carry a cask past the watch boat (card 6) can save himself only by losing the cask. Both dangers warn first: the count's cart and the buyer's law line in card 1, the smith's words about the watch boat in card 5.

## The endings

| Ending | Flag | How | What becomes of them | Picture |
|---|---|---|---|---|
| **The runner** (what really happened) | `ch_sword_smuggle_end_runner` | card 9: say you carry wine | The road goes on through the player's boat. Asulf keeps him; the smith goes on selling; the bans are made again in 811 and 864 and do not hold. The dead man's kin know whose cargo it was. | The strand at Sliesthorp: four posts and a rope, silver on a scale, three marten skins. |
| **The king's finder** (the hard road) | `ch_sword_smuggle_end_kings_man` | card 9: give them the road | A wine shipper of Dorestad in irons, the forge sealed, the smith made a man of the palace forges (or left at his own under the count's eye, if the player speaks for him). The big house is not touched. Another road opens the next year. | The court by the dam: a heap of pennies on a scale, a clerk reading, a ring of shipmen looking at the merchant. |
| **The smith north** (history changed) | `ch_sword_smuggle_end_smith_north`, `changed_frankish_smith_north` | card 9: say nothing and take the smith north | Reginbald forges on the Slie under a Danish lord with bog iron and birch charcoal; Hrodwin keeps the forge and the mark at Colonia. | A turf-roofed forge in the blue northern evening; the smith looks along a new blade; a Dane watches from a chopping block. |
| **The blade laid down** | `ch_sword_smuggle_end_laid_down` | card 9: own it and take the blade to Saint Martin's | The blade becomes a hinge on the west door at Traiectum. The house becomes Saint Martin's man and carries the church's wine; the boy Eilger comes aboard as a fosterling or is kept at the church's bread. | The sword on a white cloth before the altar; the widow and the boy; the priest's hands not yet on it. |

**History changed.** With `changed_frankish_smith_north` a Rhineland master makes pattern blades at Sliesthorp from about 800, a lifetime before the north is thought to have made such blades for itself. What would follow in later years, for cards that read the flag:
- Swords are cheaper in the north (the card sets Sliesthorp's price to 0.7 for a year; later cards may keep it lower for good), and the profit of running blades from the Rhine falls. The Frankish bans matter less: the thing they forbid is made beyond their reach.
- The fleets of 810 and of the 830s are a little better armed. The cards of chains coast_watch_810 and dorestad_834 may say so in a line when the flag is set.
- Blades with a hammer in a ring turn up in northern hands for two generations, and an heir of the house may be shown one.
- Nothing recorded is contradicted: no annal names a smith at Sliesthorp, and none would.

## Payouts

| Where | Tier | Silver or its worth | And |
|---|---|---|---|
| 1 buy one | step | (costs 50 d) a sword | the riddle `riddle_exeter_20_sword` |
| 1 ale | step | 0 | a true price word: tools dear at Sliesthorp for 90 days; the smith's feeling up |
| 2 open | step | 0 | deed fair_dealing; toll eased 30 days; the toll man's feeling up |
| 2 yield | step | 150 d for what cost 180 | the count's man's feeling up |
| 3 share | step | 0 | the crew's heart up; the riddle `r-gest-09` (the bellows that boil a "wound-leek") |
| 4 silver | **turn** | 260 d (cost 180) | a named buyer (`sw_buyer_known`) |
| 4 captives | **turn** | three captives (144 d by the table, more in the south) | deed bought_captives; a named buyer |
| 4 nothing | step | 0 | a commission: six bundles of tools in 150 days at half again |
| 5 tools | step | six bundles of tools for 90 d (180 d by the table) | lawful |
| 7 sell | **turn** | 640 d (cost 400): 240 d clear | the dealer's and the lord's man's feeling up |
| 7 tools | **turn** | 270 d (cost 90): 180 d clear | lawful |
| 7 hear | step | 0 | a true price word (Rhenish wine dear at Sliesthorp for 60 days); the Danish tongue at 0.5 |
| 10 plot | **finale** | 900 d and three marten skins (360 d) | **`unlock_plot_sliesthorp`**. Cost that lasts: `ch_sword_smuggle_blood_owed`, `sw_marked` |
| 10 purse | **finale** | 1,300 d | **`unlock_title_danish_guest_right`**. Cost that lasts: `ch_sword_smuggle_blood_owed` |
| 11 share | **finale, hard road** | 1,600 d | **`unlock_title_kings_protection`** and **`unlock_charter_dorestad`**. Cost that lasts: `ch_sword_smuggle_dane_enmity`; swords half again dearer at Sliesthorp for 240 days |
| 11 smith | **finale, hard road** | 600 d | the same two grand rewards; `ch_sword_smuggle_smith_spared`; the same cost |
| 12 partner | **finale** | 800 d | **`unlock_work_sliesthorp_forge`** and **`unlock_factor_sliesthorp`**. Cost that lasts: `ch_sword_smuggle_rhine_watched`, `sw_marked` (set in card 9) |
| 12 blade | **finale** | 1,100 d and a sword | **`unlock_heirloom_first_northern_blade`**. The same cost |
| 13 foster | **finale** | 650 d of freight paid before the voyage; a commission for six casks of Rhenish wine at 0.6 over; toll eased for a year | **`unlock_title_st_martins_man`** and **`unlock_kin_fosterling_eilger`** |
| 13 keep | **finale** | 900 d of freight less 120 d for the boy's keep; the same commission and eased toll | **`unlock_title_st_martins_man`** |

Why a merchant of the time could have won each:
- **A plot at Sliesthorp, or a guest's seat.** Foreign merchants lay at the northern trading places under a lord's peace; Godfred thought merchants worth carrying off from Reric in 808 to have them at Sliesthorp (ARF 808). Men of Dorestad traded there (Rimbert c. 24, the chapter number from memory (?): the church of about 850 pleased the merchants of Dorestad and Hamburg; chronicle 850-hedeby-church).
- **The king's protection and a toll charter.** The law of 805 itself gives the finder a part of what is seized. Royal letters of protection and toll freedom for named merchants are known from Louis the Pious's reign (Formulae imperiales no. 37, 828, from memory (?)); a grant for Dorestad's own toll goes beyond that letter and is the game's reward for the hardest road.
- **A forge and a factor at Sliesthorp.** The lord's own offer in card 7: a house, iron, charcoal and one lord. A foreign craftsman under a king's hand is as the moneyers and smiths of every court. It is the invented ending, and is marked as history changed.
- **Saint Martin's man.** The church of Utrecht's men at Dorestad were free of the king's officers (charter of 815, confirming older grants). The church pays freight and its casks pass the toll.
- **A fosterling.** Fostering a dead man's son is common to Frisians, Franks, English and Danes; the boy can scull and will grow into a hand, and an heir of the house may call him brother.

## How long it takes

At the least about 280 game days: 10 days from Colonia to the search, the passage to the Danes, 40 days before the smith will deal again, the second passage north, 120 days before the blade comes back, 30 days to the king's men, and 30 to 60 days to the ending. In play it runs over two sailing seasons with a winter between, beside ordinary trading; the two long waits (40 and 120 days) are there so that it overlaps with other chains.

## Pictures

One for each card (the `art.<card id>` entries in the writing file):

1. `s01_smiths_street`: a dark forge at Colonia, late afternoon; the smith lifts a hide from four bare blades; the count's cart rolls away outside under the Roman wall.
2. `s02_search`: the dam at Dorestad, grey morning; a Frank in a red cloak points into the boat; a boy carries ballast stones up in a basket; the toll man looks at the river.
3. `s03_crews_word`: the boat under sail at dusk; the steersman speaking without turning his head; the old Northman smiling at the bilge; the Saxon with folded arms.
4. `s04_buyer`: a beach market in the north, cold noon; a stout Dane with a hazel switch holds up a folding scale; behind him a wattle pen with people in it.
5. `s05_the_cask`: the forge by firelight; a wine cask with its head off, straw and eight hilts; the journeyman at the grinding wheel with his back turned, listening.
6. `s06_watch_boat`: a river mouth in a fresh breeze; a twelve-oared boat with a painted shield bears down on the small boat; white water on the shoals.
7. `s07_the_offer`: evening at a booth on the Slie; a tall Dane with gold arm rings bends a blade into a bow across his knee and watches the merchant, not the blade.
8. `s08_blade_comes_back`: a Frisian landing at low water under a white sky; a dead man on his nets in a small boat; a woman holds a sword level across both hands toward the viewer; a boy beside her.
9. `s09_kings_men`: the count's hall at Dorestad; a count and an abbot on one bench, a clerk with tally sticks; the merchant alone on the rushes.
10. `s10_end_runner` (ending): four posts and a rope on the strand at Sliesthorp; silver on a scale and three marten skins; longships along the shore.
11. `s11_end_finder` (ending): the court by the dam; a heap of pennies on a scale; a man in irons; a ring of shipmen looking at the merchant.
12. `s12_end_smith_north` (ending): a turf-roofed forge by the water in the blue northern evening; the Frankish smith looks along a new blade; a Dane watches.
13. `s13_end_laid_down` (ending): inside Saint Martin's at Traiectum; a sword on a white cloth before the altar; the widow, and the boy looking back at the merchant.

## For the integrator

- **Needs** (nothing was added to a shared table): a deed for informing the king's officers and a deed for running arms, if the user wants standing to move on those (the cards use `fair_dealing`, `bribed_officer`, `goods_seized`, `fled_kings_men`, `taken_for_judgement`, `bought_captives`, `gave_alms`, which exist); the grand-reward flags `unlock_plot_sliesthorp`, `unlock_factor_sliesthorp`, `unlock_work_sliesthorp_forge`, `unlock_heirloom_first_northern_blade`, `unlock_kin_fosterling_eilger` are new names for `story/unlocks_793.json` (`unlock_title_kings_protection`, `unlock_title_danish_guest_right`, `unlock_title_st_martins_man` and `unlock_charter_dorestad` are already set by other chains).
- The finales pay in silver, furs and commissions and not in casks: a Gislinge boat's hold (660 kg) would refuse ten casks of wine.
- `law seizure` is used once (card 2, a failed hide), as in `harbour_793.json`.
- A flag is switched off with `flag ch_sword_smuggle_carrying = 0`, as other chains do.
- Card 3 is skipped if the player reaches the buyer before it is drawn; nothing later asks for `ch_sword_smuggle_3`.
- If the player sells the blades in an ordinary market on the way, the sale options in cards 4 and 7 are greyed (they ask for the swords in the hold) and the "sell nothing" road goes on.
