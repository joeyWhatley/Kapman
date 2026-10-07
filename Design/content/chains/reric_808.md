# Chain: reric_808 (the beach that was carried away)

Cards: `Assets/Kapman/Game/Data/encounters/chain_reric_808_793.json` (14 cards).
Words: `Assets/Kapman/Game/Data/writing/chain_reric_808.json` (every key of the cards, an outcome text for every option, 5 news items, 14 `art.` lines).
The spine's thread: `godfreds_market`. Written 6 Oct 2026. A draft for the user to overwrite. (?) marks what could not be checked against a source. One web page was read for this chain (de.wikipedia "Reric"); the annals are given from the project's chronicle and from memory, in our own words, and nothing is quoted.

## The story in five sentences

Dragomir, a Wend with a wax booth third from the water at Reric, wants Rhineland iron and a friend in the west, and a Frisian shipman can be that friend or not.
In the spring of 808 the Danish king's ships are called out, and Asulf the dealer in people knows where they are bound: the Wends' land, and the beach at Reric last of all.
The shipman can tell Dragomir alone (who sends his best goods west in the Frisian's boat), tell the whole beach (who may believe him and hide behind the island), count the booths for the king, or sell out and go.
In high summer the sails come into the bay, the booths burn, and the merchants are rowed north in the king's wake and set down between hazel rods on the strand at Sliesthorp, where a wall is being dug from sea to sea.
What the shipman did is paid for a season or a year later: a plot in the king's new town, a sworn partnership with the Wend, a beach that was never carried away at all, or a finder's share weighed out while the Wends look on.

## How a player meets it

- **By trade, before the war.** Card `s01_wax_booth` is drawn at **Reric** or at **Sliesthorp** from 798 to 807 (weight 1.0, once). A house that trades wine or tools to the Danes meets Dragomir at Sliesthorp, where he says where his booth is.
- **By the war itself.** Card `s03_ships_called` is drawn at **Sliesthorp, Ripa or Hleidr** from March to July 808 (weight 5) whether or not the player ever met Dragomir. It is the warning before the danger.
- **By being there.** Card `s05_sails_in_the_bay` is drawn for any boat lying at Reric in July or August 808 (weight 12: sure).
- **By hearing of it.** A player who knew Dragomir or saw the ships and was elsewhere in the summer gets `s06_heard_of_it` at any other port from September 808 to the end of 809, and the story goes on at Sliesthorp.
- Other chains can read the spine's `gf_reric_friend` and `gf_warned`, which this chain sets where their meaning is met, and `changed_reric_survives`.

## The real history under it

**The annals.** Annales regni Francorum (the Royal Frankish Annals), entries for 804, 808, 809, 810 and 811; in the game as `chronicle_793.json` ids `804-summer-godfred-sliesthorp`, `808-summer-godfred-abodrites`, `808-summer-reric-destroyed`, `808-summer-danevirke`, `809-badenfliot`, `810-summer-frisia-raid-810`. English: B. W. Scholz, Carolingian Chronicles (1970), which is modern and is NOT quoted; everything here is our own rendering.

- **804.** Godfred, king of the Danes, came with his fleet and all the horsemen of his kingdom to Sliesthorp, on the border of his realm and Saxony, promised to meet the emperor and did not. The same year the emperor gave the Saxons' land beyond the Elbe to the Obodrites. (Dragomir's lines in cards 1 and 2.)
- **808.** Godfred crossed with an army into the Obodrites' land. The Wilzi, old enemies of the Obodrites, joined him of their own will. He took several strongholds; drove out Thrasco, their prince, who distrusted the loyalty of his own people; hanged Godelaib, another chief, taken by treachery; and made two thirds of the Obodrites pay him tribute. He lost the best of his men, and Reginold, his brother's son, at the storming of a stronghold. Before he went home he destroyed the trading place on the shore which in the Danes' tongue was called Reric and which by the tolls it paid was of great profit to his kingdom; he carried the merchants off from there and sailed with his whole army to the harbour called Sliesthorp. There he stayed some days and decided to close the border of his kingdom toward Saxony with a rampart from the eastern bay to the western sea along the north bank of the Eider, with a single gate for wagons and horsemen, and shared the work among the leaders of his troops. (Cards 3 to 8.)
- **809.** Godfred sent word to the emperor **by certain merchants** (per negotiatores quosdam) that he had heard the emperor was angry with him for leading an army against the Obodrites, that he wished to clear himself, and that the others had first broken the peace; he asked for a meeting of counts beyond the Elbe. They met at Badenfliot (Beidenfleth on the Stör) and nothing came of it. Later that year Thrasco was killed by Godfred's men, by treachery, **at the emporium of Reric**. (Cards 9 and 10; lines in the endings from 810. The season of the killing is not given (?).)
- **810.** Two hundred ships from the land of the Northmen fell on Frisia and took a hundred pounds of silver; Godfred was killed by one of his own following; Hemming, his brother's son, made peace. **811.** Twelve men a side swore the peace on the Eider. (A line in ending 1 from 811. Chain `coast_watch_810` tells 810 itself.)

**The place.** Reric is identified with the trading settlement dug at Gross Strömkendorf on the Wismar bay, opposite the island of Poel (`ports_793.json` reric). Excavations from 1989; a settlement and a cemetery of at least 20 hectares; glass working among the crafts; wells with timbers felled from 735/736 to **806, 809 and 811** (dendrochronology; de.wikipedia "Reric", read 6 Oct 2026). The name is linked to reeds (?). Sunk-floored huts (Grubenhäuser) and boat graves in the cemetery are from memory of the excavation reports (?). The timbers of 809 and 811, and the annal of 809 that still calls it an emporium, mean something of Reric outlived the burning: this is the ground under the changed ending.

**The law on the Frankish side.** Capitulary of Thionville, December 805, c. 7 (`chronicle_793.json` 805-12-thionville-border-posts): merchants going to the Slavs and Avars go no further than the named posts (Bardowick, Schezla, Magdeburg, Erfurt and others) and carry no arms or mail for sale. Tools are not arms; and the law names roads, not the sea. Dragomir's joke in card 1 is that a boat does not pass through Bardowick.

**The goods.** Wax, honey, furs and captives are the Wends' goods (`goods_793.json`: wax and honey made in wendland 3, marten 1, captives 3; `peoples/wendish.md`). Iron tools are made in the Rhineland (iron_tools, rhineland 2). Prices: wax 40 d a cake, marten 120 d a timber of forty skins, honey 24 d a crock, tools 30 d a crate, a captive 48 d by the table.

**Sliesthorp.** The market at the head of the Slie, later Hedeby (`ports_793.json` sliesthorp). That the first plots were measured out is an inference from the regular plots of the early harbour settlement (?); the hazel rods are ours (they are how ground is hallowed for a court or a fight in the sagas, which are much later (?)). That the newcomers owed days on the wall is ours: the annals say the work was shared among the leaders of the troops.

**The toll as a gift.** In the north a landing is paid for with a gift to the lord's man (`lore/law_tolls.md` rule 11): the plea in card 5 and the 'landing gift' of ending 1.

**Silver.** No coin north of the Eider before about 825: silver goes by weight. The cards reckon an ounce at 12 d for round numbers; a real ounce of about 27 g is nearer 16 pennies' weight (?).

**Invented, and said so.** Dragomir (`cast.dragomir`), Asulf (`cast.asulf`), the count Folcbert (`cast.folcbert`), Holmfast (`cast.holmfast`) and the crewman Ravn are the spine's invented people. Liuba, Dragomir's wife (Liub-), and Ratimir, his sister's son (Rati- + -mir), are from `names_placeholder.json`. The king's spokesman, Thrasco's man on the beach, the old woman who sells combs, the guard ship's steersman and the count's clerk have no names. Godfred is seen once, on a horse on the rise, and does not speak (the spine: 'speaks to merchants through a spokesman'). The oath over water is the brief's own guess about the Wends (?). The counts of booths (41, nine with locks) are ours.

## The people

| Who | Id | In the cards |
|---|---|---|
| Dragomir, the wax merchant | `cast.dragomir` | actor `wend` (role merchant), cards 1, 2, 4, 5, 8, 9, 11, 12, 13, 14 |
| His Saxon interpreter | unnamed | actor `interp`, card 1 (by card 2 Dragomir has learned Danish) |
| Thrasco's man on the beach | of `cast.thrasco`'s following | actor `chiefsman` (role lords_man), cards 2, 4, 13 |
| Asulf, the dealer in people | `cast.asulf` | actor `dealer` (role slave_dealer), cards 3, 8, 14 |
| The king's spokesman | of `cast.godfred`'s following | actor `kingsman` / `spokesman` (role kings_man), cards 3, 5, 8, 9, 11, 14 |
| King Godfred | `cast.godfred` | seen in card 5; never an actor |
| A guard ship's steersman | unnamed | actor `captain` (role commander), card 7 |
| Ravn | `cast.crew.ravn` | actor `hand`, card 7 |
| A shipman with news | unnamed | actor `skipper`, card 6 |
| The count at Dorestad and his clerk | `cast.folcbert` | actors `count`, `clerk`, card 10 |
| Liuba, Dragomir's wife | invented | actor `wife` (role farm_wife), card 12 |
| Ratimir, his sister's son | invented | in the words of cards 8 and 12 |
| Holmfast | `cast.holmfast` | named in card 12: he brings Dragomir's kin's wax round by sea |

## The steps

Card ids are `<stage>.chain_reric_808.<step>`. Flags are `ch_reric_808_<name>`; the short names are given here.

| # | Card | Where and when | The choices | Flags |
|---|---|---|---|---|
| 1 | `harbour ... s01_wax_booth` | Reric or Sliesthorp, 798 to 807; drawn | Take his order for six crates of tools (a true price word) / buy three cakes of wax (75 d; a riddle) / eat at his table (half the Wends' tongue; a price word on honey) | `_1`; `_order` |
| 2 | `harbour ... s02_iron_for_wax` | Reric, next call, to June 808; drawn | Deliver six crates for six wax and a timber of marten / give two crates as a guest's gift (he swears friendship over water) / say you brought none | `_2`; `gf_reric_friend` |
| 3 | `harbour ... s03_ships_called` | Sliesthorp, Ripa or Hleidr, March to July 808; drawn; also a start | Ale for Asulf (15 d): he tells it / ask the king's man (doubtful to likely: he offers finder's work; or a 24 d gift and you guess it anyway) / ask nothing and sell to the fleet | `_3`; `_knows`; `_kings_offer` |
| 4 | `harbour ... s04_word_on_the_beach` | Reric, 808 before July, knowing; drawn | Tell Dragomir alone (his goods go into your boat) / say it aloud (doubtful to likely: the beach hides; or only Dragomir believes) / count the booths for the king / sell out and go | `_4`; `_goods`, `_hid`, `_counted` or `_left`; `gf_reric_friend` |
| 5 | `harbour ... s05_sails_in_the_bay` | Reric, July and August 808; drawn, sure | Go with the fleet / slip the cable by night (doubtful to likely) / plead and lay down 60 d / lie behind the island (only if the beach hid) | `_5`; `_carried`, `_slipped` or `_pleaded` |
| 6 | `harbour ... s06_heard_of_it` | any port but Reric, September 808 to 809, if not there in the summer | Ask after Dragomir / buy four cakes of the plunder (80 d) / say nothing | `_5`, `_absent`; `_bought_loot` |
| 7 | `sea ... s07_kings_wake` | at sea in 808, carried or slipped; drawn | In the line: keep your place / drop astern at dusk (doubtful). Run from it: over the shoals (likely) / heave to and be put back | `_7`; `_carried` and `_slipped` may change places |
| 8 | `harbour ... s08_set_down` | Sliesthorp, first call after, to 810; drawn | Take a plot of the king's / tell Dragomir his goods are safe (only with his goods) / buy his sister's son out of the pen (90 d; only without) / ask for the finder's share (only if you counted) | `_8`; `_road_town`, `_road_faith` or `_road_finder`; schedules an ending |
| 9 | `harbour ... s09_word_by_merchants` | Sliesthorp, March to July 809; drawn | Carry the king's word to Dorestad (40 d) / carry it and Dragomir's too / refuse (a price word on wine) | `_9`; `_word`, `_word_wend`; schedules 10 in 20 days |
| 10 | `harbour ... s10_counts_hall` | Dorestad; scheduled | Give the words plainly (60 d, toll eased 60 days) / tell everything you saw (120 d, toll eased 180 days) / leave it with the clerk | `_10`; `gf_warned`, `_two_tongues` |
| 11 | `harbour ... s11_end_new_town` | Sliesthorp, 240 days after 8 | The king's word on the plot / build on it / sell it and stay a guest | ending `new_town` |
| 12 | `harbour ... s12_end_kept_faith` | Sliesthorp, 200 days after 8 | Pay the whole price (480 d) / pay less a fair freight (360 d) / the boy was a gift / your hand on it and a year | ending `kept_faith` |
| 13 | `harbour ... s13_end_reric_holds` | Reric or a Baltic or Danish landing, from September 808; drawn | A booth and a partner / a partner / the pilotage of the bay | ending `reric_holds`; `changed_reric_survives` |
| 14 | `harbour ... s14_end_kings_finder` | Sliesthorp, 60 days after 8 | The Wend boat and silver / silver and Asulf's store / silver and the king's peace | ending `kings_finder` |

**No dead ends.** Every card has a choice that costs only time or pride. A failed check hurts and goes on: the king's man's anger still tells you where the ships are bound (3); a beach that will not listen still leaves Dragomir, who does (4); a failed run puts you in the line (5, 7). A player who never learns where the ships are bound, or is not at Reric in the summer, comes in by card 5 or card 6. Card 8 always offers the plot. Both dangers warn first: the ships and the rope in card 3, the guard ships in card 5's own words before card 7.

**Roads through.** The longest honest road is 1, 2, 3, 4, (5 or 6), (7), 8, 9, 10, 12: ten or eleven cards. The changed-history road is 1, 2, 3, 4, 5, 13, with 9 and 10 open as well. A player who starts at card 3 and counts the booths plays 3, 4, 5, 7, 8, 14, with 9 and 10.

## The endings

| Ending | Flag | How | What becomes of them | Picture |
|---|---|---|---|---|
| **The king's new town** (what really happened) | `ch_reric_808_end_new_town` | card 8: take a plot | The merchants of Reric stay where they were set down and the strand becomes a town. The house has ground in it. Dragomir is a neighbour who did not choose his plot; if the house kept his goods and took a plot besides, he counts and waits. Thrasco is killed on the old beach in 809 and nobody asks for the Wends again. | Sliesthorp a year on: wattle houses in rows, a plank road, smoke, a Frisian boat at a half-built jetty, a green wall on the skyline. |
| **Faith kept** (the honest road) | `ch_reric_808_end_kept_faith` | card 4: carry his goods out, then card 8: say so; or card 8: buy the boy free | Dragomir has a roof and goods again, and a partner with a boat that needs no gate. His sister walks eleven days to find her son alive. The Wends of the Slie deal with the house first. | A low house on the Slie by firelight: pennies counted in fours, a bowl of water between two men, a woman holding out a ring of twisted silver. |
| **Reric holds** (history changed) | `ch_reric_808_end_reric_holds`, `changed_reric_survives` | card 4: say it aloud, and be believed | The king burns forty-one empty roofs and finds sand in nine chests. Nobody is carried to the Slie. The beach is built again under new reed. | The beach at Reric in autumn light: burnt posts, new booths, forty boats coming in from behind the island. |
| **The king's finder** (the beach sold) | `ch_reric_808_end_kings_finder` | card 3: be offered the work; card 4: count; card 8: ask for the share | The taking is shared out by tally while the Wends watch. Dragomir tells it over water to every Wend who comes to the strand. | The strand at Sliesthorp: silver on a scale, a Wend boat by the rods, her owner standing with his arms at his sides. |

**History changed.** With `changed_reric_survives` the merchants of Reric were never moved. What would follow, for cards that read the flag:

- **Reric stays a port.** `ports_793.json` closes it in 808; with the flag it should stay open, smaller, to about 811 at least and as long as the user likes. The Obodrites keep their own door to the sea instead of trading through Sliesthorp or the Elbe posts.
- **Sliesthorp grows more slowly.** The plots stand empty; wax, honey and furs are dearer there (the card sets wax to 1.3 for a year). The town that becomes Hedeby is still at the crossing of the two seas and still grows, but the king's toll is thinner, and the mint of about 825 and the church of about 850 may come later (?). Cards at Sliesthorp from 808 that speak of 'the Wends' strand' should not show when the flag is set.
- **Thrasco.** In 809 he was killed at Reric by Godfred's men. With the beach warned once, a later card may let him be warned too, or not: this chain leaves him to the chronicle.
- **The king.** Godfred knows a stranger warned the beach (`ch_reric_808_godfred_enmity`, 700 days: until he is dead in 810 and a little after). The fleet of 810 and the wall are not changed by anything here.
- Nothing recorded after 811 is contradicted that the game cannot bend: the annals do not name Reric again.

**Costs that last.**

- `ch_reric_808_danes_man` (900 days; endings 1 and 4, except the guest's option): Franks and Frisians count the house a Dane's man through the enemy years to the peace of 811.
- `ch_reric_808_two_tongues` (card 10, tell all): the king's spokesman knows; the charter option of ending 1 is closed.
- `ch_reric_808_owes_dragomir` (ending 2, the hand): 480 d owed.
- `ch_reric_808_wend_feud` (ending 4): no Wend on the Slie deals with the house.
- `ch_reric_808_godfred_enmity` (700 days; ending 3).
- `ch_reric_808_bought_loot` (card 6): the wax had a thumbprint in it. Nothing reads this yet.

## Payouts

| Where | Tier | Silver or its worth | And |
|---|---|---|---|
| 1 order | step | 0 | a true price word: tools at 1.5 at Reric for a year; Dragomir's feeling up |
| 1 wax | step | three cakes for 75 d (120 d by the table) | the riddle `riddle_exeter_27_mead` |
| 1 table | step | 0 | the Wends' tongue at 0.5; a price word on honey |
| 2 deliver | **turn** | six wax and a timber of marten (360 d) for six crates of tools (180 d) | deed kept_commission; `gf_reric_friend` |
| 2 gift | step | three crocks of honey (72 d) for two crates (60 d) | the tongue at 0.5; feeling +16; `gf_reric_friend` |
| 3 sell | step | 0 | a true price word: ale and ship's bread at 1.4 where the ships gather, 40 days |
| 4 quiet / aloud failed | **turn** | six wax and three timbers of marten aboard (600 d by the table): Dragomir's, to be answered for in card 8 and ending 2 | `gf_reric_friend` |
| 5 slip | step | 0 | the crew's heart up; deed fled_kings_men |
| 6 buy | step | four cakes for 80 d (160 d by the table) | |
| 9 carry | step | 40 d | the spokesman's feeling up |
| 10 plain | **turn** | 60 d | deed carried_letters; toll eased 60 days at Dorestad |
| 10 all | **turn** | 120 d | deed carried_letters; toll eased 180 days; `gf_warned`; the cost `_two_tongues` |
| 11 charter | **finale** | 300 d and four timbers of marten (780 d) | **`unlock_charter_sliesthorp`**; cost `_danes_man` |
| 11 plot | **finale** | 400 d and eight wax (720 d) | **`unlock_plot_sliesthorp`**; cost `_danes_man` |
| 11 guest | **finale** | 700 d | **`unlock_title_guest_on_the_slie`**; no cost |
| 12 whole | **finale, hard road** | eight marten, ten wax, four amber (1,480 d), after paying over 480 d of the 600 d his goods were worth | **`unlock_partner_dragomir`** and **`unlock_heirloom_wend_temple_ring`**; deed fair_dealing |
| 12 freight | **finale** | five marten and six wax (840 d), after paying over 360 d | **`unlock_partner_dragomir`**; deed fair_dealing |
| 12 boy | **finale** | four marten and six wax (720 d) | **`unlock_partner_dragomir`** |
| 12 hand | **finale** | the 600 d of his goods, kept and owed | **`unlock_factor_sliesthorp`**; cost `_owes_dragomir` |
| 13 booth | **finale, hard road** | eight marten, twelve wax, eight honey (1,632 d) | **`unlock_plot_reric`** and **`unlock_partner_dragomir`**; `changed_reric_survives`; cost `_godfred_enmity` |
| 13 partner | **finale** | ten marten and eight wax (1,520 d) | **`unlock_partner_dragomir`**; the same |
| 13 pilot | **finale** | six marten and six wax (960 d) | **`unlock_pilot_wendish_bay`**; the Wends' tongue at 1; the same |
| 14 boat | **finale** | 600 d | **`unlock_ship_baltic_coastal_boat`**; costs `_wend_feud`, `_danes_man` |
| 14 store | **finale** | 900 d | **`unlock_factor_sliesthorp`**; the same costs |
| 14 peace | **finale** | 700 d | **`unlock_title_kings_peace_of_the_danes`**; the same costs |

Why a merchant of the time could have won each:

- **A plot, a charter or a guest's seat at Sliesthorp.** The king thought merchants worth carrying across the sea to have them at his landing (ARF 808); a foreign merchant who came of his own will and held ground through the first winter is the kind of man such a town was made of. Frisians traded at Hedeby (Rimbert, Vita Anskarii c. 24 (?): the merchants of Dorestad). Freedom from the landing gift is a king's spoken word before witnesses, not a written charter: the flag's name is the game's.
- **A sworn partner.** The felag, partnership in a boat and its lading, is on rune stones of the 10th and 11th centuries; a Wend and a Frisian swearing it in 809 is early and is assumed (?).
- **The temple ring.** Rings of silver wire worn at the temples are the Wendish women's ornament in the spine's description of Dragomir's wife; a gift of a worn ornament is the largest thing she has to give.
- **A booth at Reric.** A stranger with a booth on a Wendish beach under the prince's peace: Danes and Saxons traded there already (`npc_roles_793.json` reric).
- **The pilotage of the bay.** Local knowledge given by a local man at the steering oar, as every bar and river mouth was learned.
- **A Wend boat.** Plunder shared by tally among those who served is every following's custom. `baltic_coastal_boat` is the class in `ship_classes_793.json`.
- **The king's peace.** A lord's protection on the water, spoken before witnesses.

## How long it takes

From the first call at Dragomir's booth (798 to 807) to the burning is anything from one year to ten: the chain sleeps between card 2 and the spring of 808, which is the point. From card 3 the waits are real ones: spring to high summer 808 (about 60 to 120 days), the passage to the Slie, then 60 days (the finder), 200 days (faith kept) or 240 days (the new town) to the ending, with the king's message of spring 809 and the 20-day passage to Dorestad between. A player who meets it only at card 3 is in it for about a year; one who met Dragomir in 800 has carried the thread for nine.

## Pictures

One for each card (the `art.<card id>` entries in the writing file):

1. `s01_wax_booth`: a beach market on a summer afternoon; a wide-faced Wend in a fur-edged cap before a stack of round wax cakes, a wax tally in his hand, a thin Saxon at his shoulder; sunk huts with reed roofs and boats on the sand behind.
2. `s02_iron_for_wax`: inside a sunk booth, light from the door; the Wend tries an axe edge on his thumbnail; open crates of tools; a bowl of water between the two men; his wife, silver rings at her temples, sets down bread and honey.
3. `s03_ships_called`: a Danish strand in spring, grey sky; long hulls being tarred, shields under a sail; a stout dealer with a plaited beard counting coils of rope; a king's man with a spear looking straight at us.
4. `s04_word_on_the_beach`: Reric at dusk in early summer, calm; booths, fires, children catching crabs; the Frisian and the Wend close together in the foreground; the island dark across the bay.
5. `s05_sails_in_the_bay`: Reric at noon in high summer; the bay crowded with ships; armed men carrying wax, looms and chests to the water; the first roofs alight; on the rise, small and still, a horseman in a red cloak.
6. `s06_heard_of_it`: a windy autumn landing; a shipman telling news with both hands; cakes of wax with a thumbprint at his feet.
7. `s07_kings_wake`: evening at sea, low red light; a long line of ships along a flat coast; a Wend boat ahead with a loom across her thwarts and a goat in the bows; a guard ship outside the line.
8. `s08_set_down`: the strand at Sliesthorp, dull morning; king's men with a knotted rope and hazel rods; families sitting on bundles; a rope pen of captives; men with spades up the slope.
9. `s09_word_by_merchants`: a misty spring morning at the jetty; a Dane in a clean shirt cutting a piece from a silver arm-ring beside the merchant's boat; his men out of hearing.
10. `s10_counts_hall`: a timber hall at Dorestad; a grey count leaning forward on a bench; a tonsured clerk writing on wax; the merchant standing cap in hand; the river through the door.
11. `s11_end_new_town` (ending): Sliesthorp a year on, bright and cold; new houses in rows, a plank road, smoke, a Frisian boat at a half-built jetty; a green wall on the skyline.
12. `s12_end_kept_faith` (ending): a low house on the Slie, firelight; pennies counted in fours; a bowl of water and a raised hand; the wife holding out a ring of twisted silver; a boy at the door.
13. `s13_end_reric_holds` (ending): Reric in clear autumn light; black burnt posts and new booths under green reed; forty boats coming in from behind the island; a Wend and a Frisian at a booth third from the water.
14. `s14_end_kings_finder` (ending): the strand at Sliesthorp, flat grey afternoon; a scale heaped with cut silver; the spokesman with a tally; a Wend boat by the rods; her owner looking at the merchant.

## Needs (for the spine and the integrator; nothing here was added to a shared table)

- **`ports_793.json` closes Reric with `"to": 808`.** If that shuts the port at the start of 808, cards 2, 4 and 5 (which are set at Reric to August 808) cannot fire: the port should stay open through August 808 (and `realms/denmark.md` already asks for 809, for Thrasco's death).
- **`changed_reric_survives`** should keep the port `reric` open after 808, and `unlock_plot_reric` is only of use if it does. If the port cannot be kept, give ending 3's first option a plot elsewhere.
- **New grand-reward flags** for the game to read: `unlock_charter_sliesthorp`, `unlock_title_guest_on_the_slie`, `unlock_partner_dragomir`, `unlock_heirloom_wend_temple_ring`, `unlock_plot_reric`, `unlock_pilot_wendish_bay`, `unlock_title_kings_peace_of_the_danes`. (`unlock_plot_sliesthorp`, `unlock_factor_sliesthorp` and `unlock_ship_baltic_coastal_boat` are also set by other chains or are a listed class.)
- **Dragomir's goods** in card 4 are put in the hold with `goods`, so the game cannot tell them from the player's own: the debt is carried by the flag `ch_reric_808_goods` and settled in card 12. A 'held for another' mark on goods would be truer.
- **A scheduled card at a port** (`schedule ... at sliesthorp`) is assumed to wait until the boat next lies there, as in the other chains.
- Outcome texts (`...out_<option>`) are written for every option and named in each option's note; the engine has no field for them yet.
