# Chain: storm_vow (a vow made in a storm, and the long way to keeping it)

Cards: `Assets/Kapman/Game/Data/encounters/chain_storm_vow_793.json` (14 cards: 8 harbour, 4 crew, 2 sea).
Words: `Assets/Kapman/Game/Data/writing/chain_storm_vow.json` (231 entries: every key, an outcome text for every option, 4 news items, 14 `art.<card id>` lines and 4 ending pictures).
Written 6 Oct 2026. A draft for the user to overwrite. (?) marks what could not be checked against a source.
No web search or fetch was used: the history is from the project's own research (`lore/gods_cult.md` 4, 9, 10; `lore/sea_lore.md` 11.3; `economy/food.md` 5; the places files of Lindisfarena, Gyrwe and Doccinga; the chronicle table; the spine) and from memory, and everything from memory carries a (?).

## The story in five sentences

On a night when the withy of the steering oar parts on a lee shore, the lookout shouts for the master to promise Saint Cuthbert a candle his own height, carried by himself to the saint's island, and the best thing in the hold; the Northman says the one who has the weather is Thor, and that he wants the best thing in the hold and no wax.
In the calm the crew says the vow back word for word, the steersman hangs a cord by the mast with a knot for every day, and custom gives it a year and a day.
For the saint, the lookout dips the candle from two cakes of wax, a mother with a sick son asks to buy it or to be cut a hand's length, the sand to Lindisfarena is walked at the ebb, and at the coffin the bishop asks for the rest and says what he thinks is best in any hold; for Thor, an old shipmaster says a thing is given only where an ox is killed and men are watching, and at winter's start a lord's hall sees a sword go into the tarn.
A master who lets the cord fill, or says the wind made the words, can still pay in another coin at the church on the mound at Doccinga: a lantern for the creek, or forty days on bread and water.
And a house on this road that lies at Gyrwe in the spring of 794, when long ships are seen off the Coquet, can carry Bede's books up the Tyne and leave the heathen a swept church and a storm.

## How a player meets it

- It starts by itself **at sea in a full storm** (`storm >= 0.6`), any year before 870: card `s01_storm`, 0.12 an hour while the weather lasts, once. A house that sails the North Sea in autumn meets it in its first year or two.
- It keeps clear of the chain old_gods (a vow to Saint Martin or to the sea, and the stone at Walichrum), which also begins in a gale: this start waits while `ch_old_gods_owe_wait` runs (150 days), the danger is a different one (the oar, not the bailing), and the steersman has a line for a man who already owes Saint Martin. A house can play both, a season apart.
- After the storm every step is drawn at once where it belongs (harbour weight 30, crew rate 0.3 to 0.5): the calm (anywhere), the dipping (moored anywhere), the mother (any English port), **Lindisfarena** (the saint), any **Danish or Norse port** and then **Skiringssal or Ogvaldsnes in the ninth to eleventh month** (Thor), **Doccinga** (the other coin), **Gyrwe from the fourth to the sixth month of 794** (the books).
- A place writer may add `card:harbour.chain_storm_vow.s06_sand` to `lindisfarena.sand`, `card:harbour.chain_storm_vow.end_saint` to `lindisfarena.church`, `card:harbour.chain_storm_vow.end_commuted` to `doccinga.church`, and `card:harbour.chain_storm_vow.s12_sails` to `gyrwe.beach`. The act `act:vow` at those sites is the same custom in small.

## The real history under it

| What the chain shows | What is known | Source |
|---|---|---|
| A vow in a storm, to a saint or to a god, with a price and a day | In danger a man promises something if he lives: a fast, alms, wax, a pilgrimage, a gift to a named church. It binds. The best account is of heathens: the Svear before a Curonian fort, about 854, found by lot that none of their gods would help and, on the word of merchants, vowed Christ a fast and alms, and kept it. An old man at Birka says the Christians' god has helped many of them at sea. | Rimbert, Vita Anskarii c. 19, 27, 30; `lore/sea_lore.md` 11.3; `lore/gods_cult.md` 9, 10.2 |
| A candle the height of a man | A candle of the sick or saved person's own weight or measure, given to the saint, is a habit of the miracle books. The clear cases are later than 793 (?); the project's lore allows it ("a candle of his own weight in wax", "candles of a man's measure"). Two ten-kilo cakes for a candle a man's height and a forearm thick is our reckoning. | `lore/sea_lore.md` 11.3; `lore/gods_cult.md` 9 (?) |
| Wax, not tallow | Altar lights must be wax. Wax is the Church's endless want, dearest before Candlemas and Easter; the great Easter candle is a pillar of it. | `economy/food.md` 5; `lore/saints_calendar.md` |
| "Now I keep words, now I drive back the dark" | Eusebius, riddle 31, De cera: wax as tablet and as candle. Eusebius is usually taken for Hwaetberht, abbot of Wearmouth-Jarrow (?), which is why the lookout, a Gyrwe boy, has it. The card gives the game's riddle item, not a translation. | `riddles_793.json` `riddle_eusebius_31_cera` (Giles 1851) |
| Saint Cuthbert and the sea | As a boy he prayed, and the wind turned for the monks' rafts being driven out of the Tyne, while the country people on the bank jeered that nobody should pray for men who had taken away the old worship. A storm stilled among the Picts. Brothers held on Farne by a gale for disobeying him. | Bede, prose Life of Cuthbert c. 3, 11, 36 (in our own words) |
| The island, the sand, the coffin | Lindisfarne is cut off twice a day. The body was raised in 698, found whole, and set above the floor at the right of the altar; the cloths taken off it were kept and kissed; earth from where the washing water was poured healed a boy; the sick came to the tomb. That pilgrims crossed the sand barefoot is later custom (?). | Bede, Historia ecclesiastica III.3, IV.30 to 32; prose Life c. 41, 42; `places/lindisfarena.json` |
| The church with its doors off; "few lamps" | 8 June 793: the heathen men at Lindisfarne. Bishop Higbald stayed. Alcuin wrote to him and to the brothers. In 793 the bishop wants wax and wine for the altar. | Anglo-Saxon Chronicle 793; Alcuin, letters 16, 20; `places/lindisfarena.json` (lindisfarena.higbald) |
| Books in bare boards given by a robbed house | The raiders wanted metal, not books; the gospel book of Lindisfarne survived. A book is what such a house has that travels. | `places/lindisfarena.json` story `lindisfarena.missing_cover` |
| "Your house's name is in the book" | Names of friends and givers written in a church's book and read at the altar: the Durham Liber Vitae, begun about 840, perhaps on this island (?). LATER for 793. | British Library, Cotton Domitian A.vii (?) |
| Thor has the weather | "Thor presides over the air, and governs thunder and lightning, winds and rain, fair weather and crops": written of the Svear about 1075. A settler of Iceland, Helgi the Lean, "believed in Christ, but called on Thor for sea voyages and hard matters": written down about 1275. Both LATER; the crew chain already has Thunaer in a squall (`crew.faith.thunder`). | Adam of Bremen IV.26; Landnamabok (?) ; `lore/gods_cult.md` 9 |
| No house with a door; an ox, and men watching | No priesthood or temple a merchant can pay at. What is attested is acts: a beast killed and eaten together in the lord's own hall, a share set aside; weapons and tools thrown into the lake at Tisso. Nothing is recorded of words spoken, so the cards write none. | `lore/gods_cult.md` 4, 6 |
| The halls | A great hall of these years stood at Huseby above the market of Skiringssal (?); Ogvaldsnes on the Karmsund was a lord's seat on the sheltered way north. An autumn feast: Sigvat's verses of about 1019 (LATER); "winter's start" as its day is the project's. | D. Skre, Kaupang in Skiringssal (2007) (?); `ports_793.json`; `lore/gods_cult.md` 7 |
| "The Franks say we kill a man for a fair wind" | Dudo of Saint-Quentin, about 1015, says the Northmen of old killed a man to Thor before sailing. He is writing a dark past for a Christian duke; the old shipmaster denies it. That Franks said so in 793 is ours (?). | Dudo, De moribus I (?); `lore/gods_cult.md` 9 |
| "I forsake Thunaer and Woden and Saxnot" | The Old Saxon baptismal vow. Vows at springs, trees and groves forbidden to the Saxons on pain of a fine. | MGH Capitularia I no. 107 (`texts_793.json` `saxon_vow`); Capitulatio de partibus Saxoniae c. 21 |
| Another coin | Penance by the book: a term on bread, water and salt for each sin, turned by commutation into alms, silver or a work, which churchmen complained of and took. A vow to the old gods is itself the sin. Forty days and a hundred and fifty pennies are our tariff (?). | The penitentials of Theodore and Egbert, Halitgar about 830 (?); Indiculus superstitionum; `lore/gods_cult.md` 10.3 |
| The church on the mound and its light | Boniface was killed at Dokkum on 5 June 754; a mound was raised against the tides, a church built on it, and a spring found. On his day the marsh fills and the church needs wax and wine carried in. King Charles had the old lighthouse at Boulogne restored and a fire lit on it at night in 811. That a marsh church kept a lantern on storm nights is invented (?). | Willibald, Vita Bonifatii c. 9 (?); `places/doccinga.json`; Annales regni Francorum 811 |
| Gyrwe, 794 | "The heathen ravaged among the Northumbrians and plundered Ecgfrith's monastery at the mouth of the Don; one of their leaders was killed there, some of their ships were broken by bad weather and many men drowned, and those who came alive to the shore were killed at the river mouth." The season is not given; the chronicle table fires it on 15 July. Symeon of Durham (about 1107) makes the storm Saint Cuthbert's vengeance (?). | Anglo-Saxon Chronicle D, E 794 (our words); `chronicle_793.json` `794-summer-donemuthe-raid`; Symeon, Libellus II.5 (?) |
| "You live by the sea, from which this plague first came" | Alcuin to the brothers of Wearmouth and Jarrow, 793. Benedict Biscop ordered his library kept whole. | Alcuin, letter 19 (our rendering); Bede, Lives of the Abbots c. 11 |
| The king's writing | Aethelred of Northumbria holds the Tyne in 794 and is killed on 18 April 796. Kings in the south freed minsters' ships of toll; no Northumbrian writing of the kind survives (?). | Anglo-Saxon Chronicle 794 (for 796); Sawyer 29, 86, 91 |

**Invented, and said so.** The storm and the vow; the cord of knots; the mother and her son; the old shipmaster, the lord of the hall and his tarn; that a fisher saw the ships of 794 a day ahead, and that the chests went up the river; the lantern on the mound. What must not be claimed, and is not: hammer amulets (they are of the tenth century); a temple or a priest of Thor; words said over the ox; a miracle (the wind drops in every version, and nobody in the words says who dropped it except the brothers of Gyrwe, whom the abbot answers).

## The people

| Who | Id | In the cards |
|---|---|---|
| The lookout, a runaway oblate of Gyrwe | `cast.crew.beornwine` (or an English lookout of the forge) | actor `lookout` |
| The hand, a Northman who keeps the old gods | `cast.crew.ravn` | actor `hand` |
| The steersman, baptised at Doccinga | `cast.crew.aldger` | actor `steersman` |
| The mate, a Saxon baptised under the law | `cast.crew.brunger` | actor `mate` |
| A thegn's widow with a sick son | invented, named by the forge | actor `mother` |
| The boatman who knows the sand | `lindisfarena.eadstan` (to 812) | actor `boatman` |
| The bishop of Lindisfarne | `cast.higbald` to 803; then Ecgberht, Heathured, Ecgred, Eanberht, Eardulf (?) | actor `bishop`, never named |
| An old shipmaster; the lord of a hall | invented | actors `elder`, `lord` |
| The priest of the church on the mound | `doccinga.meinward` (to 822) | actor `priest` |
| Abbot Friduin, the guest-master Wynhelm, the fisher Dudda | `gyrwe.abbot`, `gyrwe.guest_master`, `gyrwe.fisherman` | actors `abbot`, `guest_master`, `fisher` |

Saint Cuthbert, Saint Martin, King Charles, King Aethelred and "the master" (Bede) are spoken of and never met. The men who "were west over the sea" at the lord's board are `cast.thorleif`'s kind; he is not named.

## The steps

Sea and crew cards are drawn by `rate` (per game hour), harbour cards by `weight`. "Likely" is a chance of 0.6 to 0.7 before the crew's seamanship.

| # | Card id | Where and when | The choices | Flags set (all `ch_storm_vow_...`) |
|---|---|---|---|---|
| 1 | `sea.chain_storm_vow.s01_storm` | at sea, `storm >= 0.6`, before 870 | vow Saint Cuthbert the candle, the journey and the best of the hold / vow Thor the best of the hold / promise nobody and take the oar (likely; a hurt hand if not) | `_1`; `_saint`, `_thor` or `_none` |
| 2 | `crew.chain_storm_vow.s02_what_was_said` | the first calm | stand by the saint's vow / stand by Thor's / say a gift can be small / say the wind made the words | `_2`; the timer `_year` (400 days); `_small`; or `_broken` |
| 3 | `crew.chain_storm_vow.s03_dipping` | moored anywhere; saint's road; again every 20 days until done | two cakes of clean wax / one cake and mutton fat / 110 d for wax ashore / not yet | `_3`, `_candle`, the timer `_cure` (7 days); `_tallow` |
| 4 | `harbour.chain_storm_vow.s04_buyer` | any English port but the island, candle aboard | cut her a hand's length / sell it for 150 d and dip another / 12 d for wax / refuse | `_4`; `_short`; or `_sold` (clears `_candle` and `_3`) |
| 5 | `crew.chain_storm_vow.s05_both_ways` | a slack watch; either road; optional | one debt, one creditor / pay both / hear the Saxon vow | `_5`; `_both`, `_two_lords` (and both vow flags) |
| 6 | `harbour.chain_storm_vow.s06_sand` | Lindisfarena, candle aboard and cured | barefoot over the sand / round by the harbour / send the lookout | `_6`; `_barefoot` or `_sent`; then E1 at once |
| E1 | `harbour.chain_storm_vow.end_saint` | Lindisfarena, at the coffin | the silk (hard road) / two casks of Rhenish / 100 d / the candle only | `_end_saint`; the timer `_rest` (200 days) |
| 8 | `harbour.chain_storm_vow.s08_thors_man` | any Danish or Norse port; Thor's road | name the sword and have it marked / name the wine / say you will know in the hall | `_8`; `_marked` |
| E2 | `harbour.chain_storm_vow.end_thor` | Skiringssal or Ogvaldsnes, ninth to eleventh month | the marked sword to the tarn (hard road) / two casks for the hall / 100 d of silver to the tarn / eat and keep it back | `_end_thor`, `_rest`; or `_broken` |
| 10 | `crew.chain_storm_vow.s10_overdue` | when `_year` has run out unpaid; again every 60 days | swear it again (200 days more) / ask the priest at Doccinga / cut the cord down | `_year` again with `_late`; or `_broken` (`_asked`) |
| E3 | `harbour.chain_storm_vow.end_commuted` | Doccinga, once `_broken`; again every 60 days if refused | pay for the light (150 d) / keep the fast / both (hard road) / neither | `_end_commuted`, `_rest`; or `_forsworn` |
| 12 | `harbour.chain_storm_vow.s12_sails` | Gyrwe, fourth to sixth month of 794, any house past step 2 | carry the book chests up the river (likely; a wet chest if not) / give counsel / pray and sail | `_12`; `_warned`; `_carried`, `_wet` |
| E4 | `harbour.chain_storm_vow.end_gyrwe` | Gyrwe, eighth month of 794 to 799, if `_warned` | the house's gift / the gift and the king's writing (hard road, needs `_carried`) / only prayers | `_end_gyrwe`, `changed_jarrow_794` |
| 14 | `sea.chain_storm_vow.s14_second_storm` | at sea in hard weather, 200 days after E1, E2 or E3 | say nothing and steer / give thanks and promise nothing / vow again | `_done`; `_again` |

**Roads through.** The saint: 1, 2, 3, (4), (5), 6, E1, 14. Thor: 1, 2, (5), 8, E2, 14. The other coin: 1, 2 (deny) or 10 (the cord) or E2 (keep it back), then E3, 14. Both lords: 5 (both) opens the saint's and Thor's roads together; both finales can be reached, neither hard road can, and `ch_storm_vow_two_lords` stays set. The books of Gyrwe: 12 and E4 stand beside whichever of these the house is on, in 794 only.

**No dead ends.** Every card has an option that costs only time or pride (take the oar, bargain, "not yet", refuse the mother, one creditor, come by the harbour, the candle only, "I will know it in the hall", swear again, the fast, give counsel, only prayers, steer). A failed chance hurts and goes on: a crushed hand and the men's vow stands; a wet chest and the books still go up the river. Each danger warns first: the steersman names the shore, the guest-master names the shoals, the cord shows how much is left. A sold candle is dipped again. A man who denies everything and refuses the priest keeps a card at Doccinga that comes back every sixty days; the only way to end with nothing is never to go there.

## The payouts

| Tier | Where | Silver or its worth | And |
|---|---|---|---|
| Step | 1 | 0 | the crew's heart; a man's feeling up, another's down |
| Step | 2 | 0 | the term set; the crew's heart |
| Step | 3 | 0 (2 cakes of wax, 80 d, or 110 d paid) | the wax riddle `riddle_eusebius_31_cera`; the lookout's feeling |
| Turn | 4 | 150 d if sold; 0 otherwise | deed `gave_alms` and the mother's feeling for the cut piece or the pennies |
| Step | 5 | 0 | the text `saxon_vow` and Old Saxon half learned; or both roads opened |
| Step | 6 | 0 | the boatman's feeling |
| Turn | 8 | 0 | Old Norse half learned; a true price word (Rhenish wine up by a third at Skiringssal for 150 days) |
| Finale E1 | Lindisfarena | a book and 120 d, about 480 d, with an order for six casks of Rhenish at double price (about 360 d more); or a book and the order; or the orders for wine and wax alone (about 600 d) | `unlock_relic_cuthberts_cloth`; for the candle only, `unlock_relic_cuthberts_earth` |
| Finale E1, hard road | Lindisfarena | two books and 240 d, about 960 d, with orders for six casks of wine and six cakes of wax at double price (about 600 d more) | `unlock_relic_cuthberts_cloth` and `unlock_title_st_cuthberts_brother` |
| Finale E2 | Skiringssal or Ogvaldsnes | four timbers of marten and two pairs of tusks, about 660 d (630 d for silver) | `unlock_title_norse_guest_right` |
| Finale E2, hard road | the same | marten, tusks and a timber of sable, about 1,140 d; Old Norse three quarters learned | `unlock_title_norse_guest_right` and `unlock_pilot_nordvegr` |
| Finale E3 | Doccinga | eight bolts of Frisian cloth, about 380 d (six for the fast), with orders for wine and wax at double price (about 600 d more) | `unlock_work_doccinga_light`, or `unlock_factor_doccinga` |
| Finale E3, hard road | Doccinga | twelve bolts and 200 d, about 780 d, and the orders | `unlock_work_doccinga_light` and `unlock_factor_doccinga` |
| Finale E4 | Gyrwe | two books and 150 d, about 870 d | `unlock_factor_gyrwe`; the storm riddle `riddle_exeter_02_storm_sea` |
| Finale E4, hard road | Gyrwe | three books and 300 d, about 1,380 d | `unlock_factor_gyrwe` and `unlock_charter_eoforwic` |
| Step | 14 | 0 | `unlock_heirloom_vow_cord`; the storm riddle; the crew's heart |

**The hard road** in E1 and E2 is the vow kept as it was said: inside the first term (`_year` still running and not `_late`), to the one who was promised (not `_both`), not bargained down (not `_small`); for the saint also barefoot, with clean wax (not `_tallow`) that was never sold (not `_sold`), and the gift is the silk; for Thor the sword that was named and marked at step 8. A hand's length cut for a sick boy does not close it: the bishop counts the candle whole.

**Why a merchant could have won these.** A strip of the cloths taken off the saint in 698 is exactly the relic Bede describes being kept and handed about; earth from the washing place was every pilgrim's. A name in a house's book of the living and the dead is what givers got. Guest-right in a northern lord's hall is what every foreign trader there lived by (Rimbert's merchants are always under some king's or lord's protection), and a lord's own pilot for the inner way north is the knowledge Ohthere had. A lantern for a creek is a small work a priest could set as penance, and a widow who keeps your goods is how a factor begins. A cellarer's dry corner at Gyrwe and a king's strip of sheepskin freeing a keel of toll are what a saved house and a surprised king had to give. A book is the one thing a monastery has that is worth a cargo.

**Costs that last.** `ch_storm_vow_two_lords`: both were paid, and any priest's or bishop's card may read it (the chain old_gods has a priest with a small book). `ch_storm_vow_forsworn`: the priest at Doccinga and the steersman remember a refused penance. `ch_storm_vow_again`: a second vow, for a later card to collect. `ch_storm_vow_tallow`, `_sold`, `_sent`, `_small` and `_late` stay set. A Frankish sword given in the north breaks the king's ban on arms (779, 805): no deed is set, because it went into a tarn and not to a buyer, but a toll man who finds a marked scabbard before it gets there is the ordinary toll card's business.

## The endings

| Ending | Flag | How | What becomes of it | Picture (`art.chain_storm_vow.ending.*`) |
|---|---|---|---|---|
| Kept to the saint | `ch_storm_vow_end_saint` | the candle carried to the coffin, and something given for the rest | The tallest light in a robbed church burns to the Advent fast. The lookout, who ran from a cloister, has done one thing a brother would have done. The house carries the island's wine and wax, and is prayed for. | `saint`: the candle burned to a stump by the coffin; a strip of linen and a book in bare boards |
| Kept to Thor | `ch_storm_vow_end_thor` | the vow said before a hall at winter's start, and a thing given | A sword, or silver, lies in a tarn where nobody will fetch it. A lord's door knows a Frisian; the hand has stood in a hall again as an equal's man. The steersman says nothing for a week. | `thor`: a ring on black water at first light; furs and tusks on the bank |
| Paid in another coin | `ch_storm_vow_end_commuted` | the vow broken, and the priest's price paid | A horn lantern burns on the mound at Doccinga from Martinmas to Easter and boats find the creek by it; or a merchant is seen fasting through forty days of other men's dinners. The steersman can come home to his own church. | `commuted`: the lantern in rain, a small sail turning toward it |
| The books of Gyrwe | `ch_storm_vow_end_gyrwe`, `changed_jarrow_794` | the house warned in the spring of 794 | The heathen find a swept church and spears in the reeds; their chief dies on the mud and the storm takes the ships, as it did. The library Benedict ordered kept whole is whole. | `gyrwe`: a book chest open and full, wrecks through the door |

**What really happened.** Nothing of the vow is in any chronicle: all three of its endings are true to the custom. Of 794 the Chronicle says the house at the mouth of the Don was plundered before the chief was killed and the storm came: that is the game's world for every house that prays and sails at step 12, or is not at Gyrwe that spring, and the places story `gyrwe.wreck_on_the_bar` tells it.

**History changed, and what follows** (`changed_jarrow_794`). The monastery is not plundered. The Chronicle's entry for the year would read as the news item does: the heathen came, found it watched, lost their chief and their ships. Bede's own house keeps its books, glass and plate; the letters from the Continent asking for the master's works (the story `gyrwe.masters_book`) can be answered from a full chest. The Northumbrian coast has an answer to Alcuin's letter that is not only prayer: a reeve who has won once at a river mouth will keep men there, so the later tale of Hartness and Tynemouth plundered in 800 is less likely. The king has two years to live either way. The raiders learn the opposite lesson and learn it at once: the Chronicle's next raids are in the west and in Ireland, as they were. The wrecks, the rune stick and the half-drowned boy of `gyrwe.wreck_on_the_bar` are all still there, since the storm is not the player's to change.

## Needs (for the integrator; nothing here was added to a shared table)

- Grand reward flags new to this chain: `unlock_relic_cuthberts_cloth`, `unlock_relic_cuthberts_earth`, `unlock_title_st_cuthberts_brother`, `unlock_title_norse_guest_right`, `unlock_pilot_nordvegr`, `unlock_work_doccinga_light`, `unlock_factor_doccinga`, `unlock_factor_gyrwe`, `unlock_charter_eoforwic`, `unlock_heirloom_vow_cord`. (`unlocks_793.json` has `unlock_title_danish_guest_right` from another chain; this one is the Norwegian lords'.)
- Chronicle: when `flag.changed_jarrow_794` is set, tell `794-summer-donemuthe-raid` with the words of `chain_storm_vow.news.gyrwe_held` and do not mark Gyrwe plundered. `places/gyrwe.json` story `gyrwe.wreck_on_the_bar` needs no change (its beats are about the wrecks), but any line there that says the house was robbed should ask `not flag.changed_jarrow_794`.
- This chain reads three flags it does not set: `ch_old_gods_owe_wait` and `ch_old_gods_vow_martin` (the chain old_gods) and `gy_warned` (the Gyrwe story). All three are optional: unset, the cards behave the same without one line.
- Skiringssal and Ogvaldsnes have no places file yet. E2 fires at either port by its id; when the files are written, a hall site there may carry `card:harbour.chain_storm_vow.end_thor`.
- A deed for a vow kept or broken does not exist in `reputation_793.json` (`lore/sea_lore.md` 11 asks for one: "a kept vow is a deed; a broken one is remembered"). The cards use `gave_alms` where alms are given and otherwise move feelings; two deeds, `kept_vow` and `broke_vow`, would let the news carry it.
- The crew speak the boat's tongue (`old_frisian`) in these cards so that the master follows them; shore people speak their own. The lines are written for the spine's four crewmen; if one has left the boat the forge fills his seat and the lines still fit the seat.
- `ch_storm_vow_candle` and `ch_storm_vow_3` are cleared with `flag <name> = 0` when the candle is sold. If the engine does not read a flag set to 0 as unset, step 3 will not come again after a sale and step 6 will open without a candle: check `EncounterState.SetFlag`.
- The option field `then` (step 6 to E1) follows the dirham_hoard and house_debt chains. If `then` is not wired, E1 still opens on the next call at the island by its own `when`.

## Pictures

One for each card (`art.<card id>` in the writing file) and one for each ending (above).

| Card | Picture |
|---|---|
| s01 | The boat across a breaking sea at night, the steering oar loose, two men on its loom, a boy shouting at the mast, a Northman bailing and looking up |
| s02 | A flat grey morning: the boy counting three on his fingers, the Northman whetting a knife, the steersman tying a first knot in a cord |
| s03 | A strand at evening, a fire and a trough of wax: the skipper against the mast, a tarred line from heel to crown, a pale candle hanging from the yard |
| s04 | An English shingle strand, dull afternoon: a woman in a worn good cloak holding out a purse; the candle along the thwarts with the boy's hand on it |
| s05 | Becalmed, low sun: a big Saxon on a thwart looking steadily aft; the steersman looking at the water |
| s06 | A mile of wet shining sand: the boatman ahead with his pole, a barefoot man behind with the candle on his shoulder, prints filling |
| end_saint | A timber church with no doors, dusk: the oak coffin, one great candle taller than the gaunt bishop beside it |
| s08 | A northern jetty, clear and cold: an old shipmaster on a stick looking down into the hold |
| end_thor | A long hall at night in autumn, a cauldron steaming: a lord rising with a horn; a Frisian holding a scabbarded sword across both palms |
| s10 | The mast close up: a cord knotted to its very end, the last hand's breadth between the steersman's finger and thumb |
| end_commuted | A wooden church on a green mound in a salt marsh: a muddy priest weaving a hurdle, the skipper with his hood in his hands |
| s12 | Mudflat below two stone churches, early light: an abbot, a monk holding up a knotted cord, brothers carrying chests to a beached boat |
| end_gyrwe | A river mouth after a storm: ribs of broken ships in pale sand, and on the rise two stone churches, whole |
| s14 | The boat in a gale at dusk, reefed and holding: every head down, the old cord of knots swinging at the mast |

## How long it takes

The storm and the calm are one voyage. Then the vow has a year and a day (400 days in the data, to leave room for a winter). The saint's road needs wax (cheapest in the east: Reric, Sliesthorp, Truso), a week for the candle to cure, and a voyage to the far end of Northumbria, which a house of Dorestad does not make by accident: two seasons at the least, and most of a year for a master who also trades. Thor's road needs a call in the north and then the right season: the feast is only at winter's start, so a vow made in spring waits half a year for it, and a vow made in late autumn waits nearly a whole one. The other coin cannot be reached before the term runs out unless the vow is denied outright. The books of Gyrwe are fixed in time: the spring of 794 for the warning, the autumn for the thanks. The last card waits two hundred days after an ending and then for a storm. From the first night to the cord going into the sea chest: about two years, three for a house that takes the long way round.
