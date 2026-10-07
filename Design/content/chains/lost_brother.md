# Chain: the lost brother (`lost_brother`)

Cards: `Assets/Kapman/Game/Data/encounters/chain_lost_brother_793.json` (14 cards).
Words: `Assets/Kapman/Game/Data/writing/chain_lost_brother.json` (207 entries: every key, an outcome text for every
option, nine news items, an `art.<card id>` line for every card).
Written 6 Oct 2026. A draft for the user to overwrite. The checker is clean on both files.

## The story in five sentences

In the summer of 810 two hundred Danish ships fall on the Frisian islands, and a neighbour with a burned hand
brings word to the landing: your brother Liafbern, who farms your mother's kin's mound by Doccinga, was rowed off
alive, and his son Wibern, a boy of ten, in another ship. At Sliesthorp the dealer who bought the island lot
remembers every head and has no reason to lie: he split it, the grown men south with a Saxon drover to the Rhine
and Mainz, where merchants of Verdun buy for Spain, the young east with a Svea to Birka. The southern road is far
and short of time, the eastern near and long, and one small boat can be sure of only one of them; a house that is
quick, or rich, or lucky can have both. In the spring of 811 the same house can stand on the Eider, where twelve
Franks and twelve Danes swear the kings' peace, and put a list of forty-three names in front of them, which no
annal says anyone did. It ends at the mound by the creek one of five ways: two at the table, the father without
the son, the son without the father, a folded cloak on an empty bench, or smoke from every mound on the coast.

## How a player meets it

At any of six Frisian landings (Doccinga, Emuthon, Wijnaldum, Medemelacha, Dorestad, Traiectum), from midsummer
810 to the end of 812: card s01, weight 3, so almost surely on the first or second call in those years. It needs
no silver (two of its three options are free). After it the chain waits at Doccinga (s02, the marks to know them
by) and at Sliesthorp (s03, the dealer); s03 does not need s02, so a player who sails straight north loses only
the list, the band or the mound's ninety pennies. The year is fixed by the real raid; a house that is elsewhere
from 810 to 812 does not meet the chain.

The words say "your brother". If the house is in its second generation by 810, the integrator may want "your
father's brother" and "your cousin": the cards read nothing but flags, so only the writing changes.

## The real history under it

- **The raid.** Annales regni Francorum, 810: the emperor at Aachen hears that a fleet of two hundred ships from
  the Northmen's land has come to Frisia, wasted all the islands off the Frisian shore, fought three battles with
  the Frisians and laid a tribute on them, of which a hundred pounds of silver were already paid; he marches to
  the Aller and Weser to wait for King Godfred, who is then killed by one of his own following. The elephant Abul
  Abbas dies at Lippeham that summer (the rider's line in s01). Translation exists (Scholz 1970, not safe to
  quote); nothing is quoted. Also Einhard, Vita Karoli 14 and 17, and the Chronicle of Moissac 810
  (`chronicle_793.json` 810-summer-frisia-raid-810, 810-summer-godfred-murdered, 810-summer-elephant-dies).
  **The annal does not say that people were carried off in 810 (?)**. It is said outright of the next raid on
  Frisia, in 813, when the Northmen carried off men, women and much booty (Chronicle of Moissac 813;
  `chronicle_793.json` 813-frisia-raid-813), so the chain reads three years back, and that is its one large
  invention.
- **The peace.** Annales regni Francorum, 811: the peace made with Hemming, Godfred's nephew, is sworn on weapons
  over a hard winter and confirmed in spring by twelve great men of each people on the river Eider. The Franks
  are led by Count Wala, son of Bernhard (Charles's cousin); the Danes by Hemming's brothers Hankwin and
  Angandeo (`chronicle_793.json` 811-spring-eider-peace). No terms are recorded beyond the peace itself. Hemming
  dies in 812.
- **Sliesthorp.** Godfred destroyed Reric in 808 and carried its merchants to Sliesthorp (ARF 808; spine thread
  `godfreds_market`). People sold at Sliaswich: Vita Rimberti c. 18 (the 870s; later; used for the place and for
  the test of a Christian by what he can recite). Iron shackles from the harbour
  (`economy/furs_amber_ivory_slaves.md` 5.10). Lots are sold from Lammas to Martinmas (the same file, 5.4).
- **The law the southern sale breaks.** Herstal, 779, c. 19: people are sold before the bishop or count, the
  archdeacon, the hundred-man, the bishop's or count's deputy, or before well-known witnesses, and nobody sells
  one beyond the march (MGH Capitularia I no. 20). Lex Frisionum XXI, on man-stealing: who sells a free man out
  of the country pays for him as if he had killed him, or brings him back (set down about 802; the wording is
  from memory **(?)**). The steward's ruling in s06 rests on these two.
- **Verdun and Spain.** Liudprand of Cremona, Antapodosis VI.6 (about 949): the merchants of Verdun and their
  trade to Spain. Agobard of Lyon, about 826: a man who came back from Cordoba twenty-four years after he was
  stolen and sold as a boy (from memory **(?)**). Both are later than 810 and are **read back (?)**; in 793-833
  the lawful trade through the marches was in heathen Wends (`furs_amber_ivory_slaves.md` 5.9), which is why the
  rope in s06 is eleven Wends and a Frisian.
- **The Hellweg.** Diusburg stands at the west end of the road from Saxony (`dossiers/diusburg.md`). That a lot
  from Sliesthorp walked it in 810 is ours **(?)**.
- **Mainz.** Archbishop Riculf, 787 to 9 August 813, Alcuin's pupil; then Haistulf (`dossiers/mogontia.md`); St
  Alban's, consecrated 805; the Frisians' quarter (`ports_793.json`). The steward's line changes at 813.
- **Birka.** A market for people as for furs; Christian captives there in 829 (Rimbert, Vita Anskarii c. 11).
  The only northern price is late: one mark of silver for a bondwoman in Laxdaela saga c. 12 (13th century, of
  about 950), about 125 d.
- **Prices.** A grown person cost 120 to 240 d from the Baltic to England through the whole period
  (`furs_amber_ivory_slaves.md` 5.6). The boy at a mark (130 d) is inside it; the brother at 25 shillings (300 d)
  is a Verdun price, above it on purpose.
- **Imma.** Bede, Historia ecclesiastica IV.22: a thegn taken in 679 whose bonds fell off whenever his brother
  the priest said Mass for him, and who was sold in London to a Frisian. Retold in the priest's own words in
  s02; nothing quoted. It is the one text that puts a Frisian merchant in this trade.
- **Doccinga.** The wooden church of St Paul and St Boniface on its mound (`dossiers/doccinga.md`). The priest
  is Landric (`cast.landric`; real: Altfrid, Vita Liudgeri I.22; everything he says is ours).

**Invented, and marked so in the cards' notes:** Liafbern, his wife Frethburg and their son Wibern (names from
attested parts: Liaf-, -bern, Freth-, -burg, Wi-); the Svea shipman Ingimar; Gundbert of Verdun; the clerk, the
king's man, the ferryman, the steward and the carrier; forty-three taken and thirty-one returned; the kinsman's
share of 160 d when the wrong is proved; that a brother inherits before a wife **(?)**; the whole clause of the
peace. Asulf, Dragomir and Holmfast are the spine's invented cast (`cast.asulf`, `cast.dragomir`,
`cast.holmfast`). Count Wala, King Hemming, Godfred and the two archbishops are real and are spoken of, never met.
Nobody says "Viking" or "slave trade": they are the taken, a lot, a head, the Danes.

No web search was used: the facts are from the files named and from memory, and each one I could not check
tonight carries `(?)` above and in its card's note.

## The steps

Progress flags are `ch_lost_brother_1` to `_9` (one for each of steps 1 to 9). Timers are flags set `for` a
number of days and read with `not`.

| # | Card id | Where and when | The choices | Flags set |
|---|---|---|---|---|
| 1 | `harbour.chain_lost_brother.s01_word` | six Frisian landings, midsummer 810 to 812 | feed the neighbour and hear it all (6 d) / ask the count's rider about ransom (free) / thank him and say nothing (free) | `_1`; timer `ch_lost_brother_search` (1,300 days) |
| 2 | `harbour.chain_lost_brother.s02_mound` | Doccinga, after step 1 | carry the priest's list of forty-three names / take the mound's silver and give your word / take only the end of the boy's band (all free) | `_2`; one of `ch_lost_brother_names`, `ch_lost_brother_vow`, `ch_lost_brother_band` |
| 3 | `harbour.chain_lost_brother.s03_pen` | Sliesthorp, April-Nov, before 816 | a Frankish blade for the telling and his stick / two ounces of silver for the same / carry water in the pen for a day (free) | `_3`; timers `ch_lost_brother_south_open` (200 days), `ch_lost_brother_east_open` (420; 540 with the blade); blade, silver: `ch_lost_brother_tally` |
| 4 | `harbour.chain_lost_brother.s04_eider` | Sliesthorp, March-June 811 only (a turn the chain never waits for) | give the clerk the list and 40 d (needs the list; likely) / speak before the twelve (needs half the Norse tongue and a known name) / watch (free) | `_4`; clause sworn: `ch_lost_brother_peace`, **`changed_eider_peace_captives`**; list failed: `ch_lost_brother_tally` |
| 5 | `harbour.chain_lost_brother.s05_hellweg` | Diusburg, after step 3, while the brother's road is open (a side step) | hire the ferryman as a witness (30 d) / ask and go on (free) | `_5`; witness: `ch_lost_brother_witness` |
| 6 | `harbour.chain_lost_brother.s06_yard` | Mogontia, within 200 days of step 3 | buy him (300 d) / swear him free with the ferryman's oath (needs the witness; sure) / swear him free with the crew (three men; likely or doubtful) / kneel for the church's alms (free) | `_6`, `ch_lost_brother_brother_free`; timer `ch_lost_brother_wait_home` (40 days); oath won: `ch_lost_brother_verdun_grudge`; oath lost, alms: `ch_lost_brother_church_debt` |
| 7 | `harbour.chain_lost_brother.s07_gone_south` | Mogontia, after the 200 days | send a pound of silver with a carrier (240 d; even odds) / ask the archbishop's letter (doubtful) / a Mass at St Alban's (free) | `_7`; `ch_lost_brother_brother_free` or `_brother_lost`; wait 240, 300 or 60 days; writ: `ch_lost_brother_church_debt`; Mass: `ch_lost_brother_masses` |
| 8 | `harbour.chain_lost_brother.s08_booth` | Birka, April-Oct, within 420 (540) days of step 3 | a mark of silver (130 d) / the stick's price (70 d; needs the stick or the peace) / three casks of Rhine wine / your hand to carry his skins unpaid (free) | `_8`, `ch_lost_brother_boy_free`; wait 40 days; hand: `ch_lost_brother_svea_errand` (150 days) |
| 9 | `harbour.chain_lost_brother.s09_gone_east` | Birka, April-Oct, after the days have run | send a mark and a quarter east with Holmfast (150 d; likely) / send the band (needs the band; doubtful) / send word only (free) | `_9`; `ch_lost_brother_boy_free`, or `_boy_lost` with `ch_lost_brother_boy_east`; wait 330 or 60 days |
| 10 | `harbour.chain_lost_brother.s10_end_both` | Doccinga: both free, the wait run, no clause | partner / the mound's weaving / take nothing | **`ch_lost_brother_end_both`**, `ch_lost_brother_done` |
| 11 | `harbour.chain_lost_brother.s11_end_brother` | Doccinga: brother free, boy's road closed, no clause | partner / a hand on sailing east together / leave him his land | **`ch_lost_brother_end_brother`**, `_done`; east: `ch_lost_brother_east_vow` |
| 12 | `harbour.chain_lost_brother.s12_end_boy` | Doccinga: boy free, brother's road closed, no clause | foster him / stand guardian of his land / leave him with his mother | **`ch_lost_brother_end_boy`**, `_done` |
| 13 | `harbour.chain_lost_brother.s13_end_empty_bench` | Doccinga: neither free, both roads closed, no clause | take the share / leave it to the wife / give it to the church | **`ch_lost_brother_end_empty_bench`**, `_done`; church: `ch_lost_brother_masses` |
| 14 | `harbour.chain_lost_brother.s14_end_peace` | Doccinga: the clause sworn, both roads closed one way or the other | the king's peace at Sliesthorp / partner (needs the brother free) / share the silver among the mounds / take the two prices only | **`ch_lost_brother_end_peace`**, `_done` |

**How the two roads close.** A road is closed when its person is free (steps 6, 7, 8, 9), or lost (steps 7, 9),
or when `ch_lost_brother_search` runs out, 1,300 days after the first card. So a house that never sails to
Mainz or to Birka, or never finds the dealer, still comes to a finale at Doccinga: the chain cannot hang.

**What is not told when it is chosen.** At steps 7 and 9 the silver, the letter or the band goes off over the
hills or the sea, and the card says nothing of what comes of it. The roll is made there and the flag is set, but
the house learns it only at its own door, after the wait, by which finale comes up. The integrator should keep
it that way: do not show the flag.

**No dead ends.** Every card has a free option. The oath that fails at Mainz still frees the brother (the
steward buys him with the altar's silver and writes the debt). Coming late to either market opens a second card
there, not nothing. Each danger is said before it is taken: the dealer names the two roads and that there is one
boat; the Wend says the south road is the short one; Holmfast says he keeps his quarter if the boy will not come;
the steward says a letter is slower than a rope of men. The blade at step 3 breaks the king's law (Thionville
805): the danger of that is the toll man's own card at home, and the option's label says what it is.

## The endings

| Ending | Flag | How | What becomes of them | History |
|---|---|---|---|---|
| Two at the table | `ch_lost_brother_end_both` | s10: Mainz and Birka both in time, or late and lucky | Liafbern roofs a new byre; the boy talks Svea to the dog. The brothers swear one purse and two boats, or the mound weaves for the house. | Invented; the hard road. Ransom was the good work of the age, and a man who fetched two would be talked of on every landing. |
| The father's bowl | `ch_lost_brother_end_brother` | s11 | Liafbern is home and reads the crew of every ship from the north. Wibern is a Svea's boy on the Volkhov, fed, and learning to steer. | Invented and ordinary: the young were kept. |
| His father's knife | `ch_lost_brother_end_boy` | s12 | Wibern is home, keeping an edge on a knife out of the ash. Frethburg is neither wife nor widow. Liafbern is in a country without winter. | Invented and ordinary: the grown men went furthest. |
| The empty bench | `ch_lost_brother_end_empty_bench` | s13: both let go, both lost, or never sought | Forty-three names are read at St Boniface's. A cloak that fits nobody lies folded on the bench. The share of the mound goes to the brother, the wife or the church. | **As it really went:** no source says anyone taken in 810 came home, and the peace of 811 says nothing of them. |
| The boats from the Eider | `ch_lost_brother_end_peace` | s14, after the clause at s04 | Thirty-one of the forty-three come up the creek at Lammas; silver is paid for the twelve. The mounds smoke again. The house's own two are home or not, as steps 6 to 9 went, and the wife's line says which. | **History changed:** see below. |

**What follows from the change** (for later cards to read):

- `changed_eider_peace_captives`: the peace of 811 carries a clause that the Danish king gives back the taken who
  are still in his kingdom and makes good the price of those sold out of it. It was sworn before twenty-four
  great men, so it is a custom the next peace must answer to. Later cards can let the envoys of 813 (ARF 813:
  Frankish and Saxon nobles sent over the Elbe to make peace with Harald and Reginfrid) and of 825 and 873
  (Annales Fuldenses 873: the Danes ask that merchants may cross the march in peace) carry the clause again;
  let a house after the sacks of Dorestad (834 to 837) and Walcheren (837) claim its people at Sliesthorp by
  right and not by purse (chains `dorestad_834`, `thread_02`); give Anskar, who comes to the Danes in 826 and
  buys boys to teach (Rimbert c. 15), a king's hall that already knows what giving people back is; and make the
  dealers of Sliesthorp colder to Frisians for a generation (the news item at Sliesthorp says so). It does not
  touch the east: the Svear swore nothing.
- `unlock_charter_sliesthorp` under this king only: Hemming dies in 812. A later card should make the house show
  the mark to the next king (the sons of Godfred, 813) and keep it or lose it.
- `ch_lost_brother_verdun_grudge` (a lasting cost): Gundbert of Verdun paid the wrong and knows the house's sail.
  Later cards on the Mosel wine or at the fair of Saint-Denis may meet him or his partners.
- `ch_lost_brother_church_debt` (a lasting cost): the house was helped with altar silver at Mainz. Step 6's
  `alms` opens a commission of four cakes of wax within 300 days; a later card may have the steward ask more, or
  Utrecht hear of it.
- `ch_lost_brother_east_vow` (a debt of honour): the house gave its hand that Liafbern sails with it the summer
  it goes east. A later card at Birka or Aldeigjuborg (chain `silver_road`, `saga_baltic_east`) can find a young
  man who answers to Wibern, or does not want to.
- `ch_lost_brother_boy_east`: the boy is alive on the east road and knows he is remembered. The same hook, even
  after the empty bench: the priest's line in s13 warns that he may one day walk up the mound with a beard.
- `ch_lost_brother_vow`: the house took the mound's ninety pennies and gave its word before three neighbours.
  If it comes to the empty bench, Frethburg says so; a later card may too.
- `ch_lost_brother_svea_errand` (150 days): the house owes Ingimar a cargo of skins carried to Sliesthorp unpaid.
  The engine does not enforce it; a later card at Birka may ask whether it was done.

## Payouts

| Step | Option | Silver | And |
|---|---|---|---|
| 1 | bread | -6 | the deed `gave_alms` |
| 1 | ask | 0 | a true rumour posted at the port |
| 2 | names | 0 | Latin +0.05; the list (opens `list` at step 4) |
| 2 | silver | +90 | a word given before neighbours (`ch_lost_brother_vow`) |
| 2 | band | 0 | the band (opens the free doubtful road at step 9) |
| 3 | blade | one blade (60 d at home) | the stick; 120 more days on the east road; a true price word: blades up three tenths at Sliesthorp for 120 days |
| 3 | silver | -24 | the stick (60 d off the boy's price at step 8) |
| 4 | list | -40 | likely: the clause, and the `changed_` flag; failing: the king's token, which serves as the stick |
| 4 | speak | 0 | the clause; the king's man's feeling +8 |
| 5 | witness | -30 | makes the oath at Mainz sure (worth 160 d and the 300 d not paid) |
| 5 | ask | 0 | a true price word: Frisian cloth up 15% at Mogontia for 90 days |
| **6 (turn)** | witness, or oath won | **+160** | the brother free; the deed `won_case`; cost: the Verdun grudge |
| 6 | buy | -300 | the brother free |
| 6 | oath lost | 0 | the brother free; the deed `lost_case`; cost: the church debt |
| 6 | alms | 0 | the brother free; a commission of four cakes of wax; cost: the church debt and the kneeling |
| 7 | letter | -240 | even odds that he comes home in 240 days |
| 7 | writ | 0 | doubtful; 300 days; cost: the church debt |
| **8 (turn)** | buy / tally / wine | -130 / -70 / three casks (180 d at home) and +40 back | the boy free; a commission of six bolts of Frisian cloth at a premium of 1.4 (about 115 d over) |
| 8 | hand | 0 | the boy free; the crew's heart -0.03; an errand owed |
| 9 | silver | -150 | likely that he comes home in 330 days |
| 9 | band | 0 | doubtful |
| **10 (finale, the hard road)** | partner | **+700** and twelve bolts of Frisian cloth (1,276 d) | **two** grand rewards: `unlock_partner_liafbern`, `unlock_kin_wibern`; the boy hired as a hand |
| 10 | weave | +500 and sixteen bolts (1,268 d) | **two**: `unlock_factor_doccinga`, `unlock_kin_wibern` |
| 10 | **nothing (the honest road)** | +300 and ten bolts (780 d), laid in the boat by night | **three**: `unlock_partner_liafbern`, `unlock_kin_wibern`, `unlock_plot_doccinga`; the deed `fair_dealing` |
| **11 (finale)** | partner | **+450** and eight bolts (834 d) | `unlock_partner_liafbern` |
| 11 | east | +300 and eight bolts (684 d) | `unlock_partner_liafbern`; the debt of honour |
| 11 | land | fourteen bolts (672 d) | `unlock_factor_doccinga` |
| **12 (finale)** | foster | **+300** and eight bolts (684 d) | `unlock_kin_wibern`; the boy hired as a hand |
| 12 | guardian | +200 and ten bolts (680 d) | `unlock_plot_doccinga` |
| 12 | leave | thirteen bolts (624 d) | `unlock_factor_doccinga` |
| **13 (finale)** | share | **+260** and eight bolts (644 d) | `unlock_plot_doccinga`; the wife's feeling -8 |
| 13 | widow | fourteen bolts (672 d) | `unlock_factor_doccinga` |
| 13 | masses | +150 and ten bolts (630 d) | `unlock_title_st_bonifaces_man`; half toll for a year; the deed `gave_alms` |
| **14 (finale, history changed)** | charter | **+900** and ten bolts (1,380 d) | **two**: `unlock_title_islands_spokesman`, `unlock_charter_sliesthorp` |
| 14 | partner | +800 and ten bolts (1,280 d) | **two**: `unlock_title_islands_spokesman`, `unlock_partner_liafbern` |
| 14 | share | +300 and sixteen bolts (1,068 d) | `unlock_title_islands_spokesman`; the deed `gave_alms` |
| 14 | kin | +600 and six bolts (888 d) | `unlock_title_islands_spokesman` |

A bolt of Frisian cloth is 48 d in `goods_793.json`. The brother's price of 300 d and the boy's of 130 d are what
the chain costs a house that pays for everything; the roads through the law, the stick and the peace are the
cheap ones, and the peace is the richest.

**Why a merchant of the time could have won each grand reward.**

- `unlock_partner_liafbern`: two brothers with one purse and two boats is the ordinary shape of a Frisian house;
  a man who has been walked from one sea nearly to another has reasons to prefer a boat.
- `unlock_kin_wibern`: a brother's son taken into the boat and the house before witnesses; a boy who has herded
  a Svea's sheep for a year speaks the Svea tongue.
- `unlock_plot_doccinga`: a strip of the family's own mound, by gift, by guardianship or by inheritance.
- `unlock_factor_doccinga`: the looms of a mound selling to one house first. The "Frisian cloaks" were woven on
  such mounds, and a woman keeping a house alone needs a buyer who comes every year.
- `unlock_title_st_bonifaces_man`: a man who gives land to a church is that church's man, named in its prayers
  and known as that wherever its clergy are; Doccinga's church was a pilgrim place.
- `unlock_title_islands_spokesman`: the man who brought the names to the Eider is the man the mounds send to the
  count's court afterwards. Frisian districts spoke through such men (the later *asega* and *frana* are a
  different office (?); this is a standing, not an office).
- `unlock_charter_sliesthorp`: a king's mark freeing one foreign boat of his market's dues. Godfred moved a
  whole town for its tolls (ARF 808), so the toll was the king's to forgive; it lasts as long as the king.

## How long it takes

A year and a half at the least; three and a half years at the most.

- Midsummer 810 or after: the word (s01), the mound (s02).
- The same sailing season or the next: Sliesthorp (s03). The two clocks start here.
- 200 days: Diusburg and Mogontia (s05, s06), or the empty yard (s07).
- March to June 811: the Eider (s04), for a house that is at Sliesthorp then.
- 420 days (540 with the blade): Birka (s08), or the shut farm (s09).
- 40 days after a buying, 240 to 330 after a sending, 60 after a letting go: the finale at Doccinga.
- 1,300 days after the first card every open road closes by itself.

Sliesthorp to Mogontia is the whole length of the game's western water (out of the Baltic, down the coast and up
the Rhine), and Birka is the other way: a house that wants both inside the two clocks has to sail the south road
first and hard, and trade little on the way. That is the hard road, and it is meant to be.

## Pictures

One for each step (the same lines as the `art.<card id>` entries in the writing file):

1. **s01** A Frisian landing on a grey summer evening, the tide out: a thin man with a rag-bound burned hand asks
   at a boat's side; a Frankish rider waters a tired horse; smoke far off over the flats.
2. **s02** A dwelling mound in the salt marsh under a wide white sky after rain: burned byre posts in front, a low
   turf house behind; a woman at an upright loom with a red and white band on it; a tonsured priest in a seal-skin
   cloak writing on a wax tablet.
3. **s03** Sliesthorp, a wattle-fenced yard behind the booths at noon, autumn: people under a lean-to roof by a
   trough; a stout Dane with a plaited beard and a hazel switch, notched sticks on a peg; a Wend in a fur-edged cap
   at the gate.
4. **s04** A wet spring meadow by a brown river, low cloud: two lines of twelve cloaked men facing each other with
   weapons upright, horses behind; in the crowd in front, a young Frankish clerk holding a strip of parchment.
5. **s05** The Rhine at the Ruhr's mouth on a windy afternoon: a broad flat ferry at a muddy landing where a rutted
   road comes down from the east; the ferryman leaning on his pole and crossing himself to show what he saw.
6. **s06** Mainz, a walled yard in hard midday light: twelve men along a wall with one rope through their belts; one
   with a broken front tooth looks away; a well-dressed merchant with open hands; an older steward with keys.
7. **s07** The same yard, empty and swept, late-autumn rain: an iron ring in the wall with nothing tied to it; the
   steward in the gateway under his hood; beyond, a carrier loading wine casks on mules.
8. **s08** A farm on a rocky hill above Birka's bay, bright early-summer morning, birches: a cropped-haired boy of
   eleven with a herding stick among sheep on spread nets, a red and white woven belt at his waist; a spare Svea
   trader pointing; the farmer with folded arms.
9. **s09** The same farm, shut, on a still grey day at summer's end: a barred door, an empty boat shed, rollers in
   the grass; the Svea trader with a folding scale; far out, one small sail going east.

One for each ending:

10. **Two at the table (s10)** The mound on a clear evening: a new byre of driftwood, a man with a broken tooth
    laying turf on its roof, a boy handing turves up, a dog; a woman in the doorway with four wooden bowls.
11. **The father's bowl (s11)** The house door in low morning sun: a man mending a net without looking at his
    hands, his eyes on the sea; one small wooden bowl on the shelf above him; a woman in the dark of the doorway.
12. **His father's knife (s12)** A creek bank on a windy bright day: a boy cutting reed with a man's knife too big
    for his hand; his mother above with a bundle of wool; the priest beside her looking south.
13. **The empty bench (s13)** Inside the turf house, daylight from the door and smoke hole: neighbours in good
    cloaks along the walls; a bench with a folded new cloak and nobody sitting; the priest with his tablet.
14. **The boats from the Eider (s14)** The creek at high water on a warm Lammas evening: smoke from every mound to
    the horizon; two long Danish boats at the bank, thin people helped ashore; a Dane with a tally stick and a
    leather bag; the priest reading from his tablet.

## Needs from the spine and the integrator

- The grand-reward flags new with this chain: `unlock_partner_liafbern`, `unlock_kin_wibern`,
  `unlock_plot_doccinga`, `unlock_factor_doccinga`, `unlock_title_st_bonifaces_man`,
  `unlock_title_islands_spokesman`, `unlock_charter_sliesthorp`; and `changed_eider_peace_captives`.
- The raid of 813 (`chronicle_793.json` 813-frisia-raid-813) is the one that is recorded as taking people. If
  the user would rather the chain could also begin then, widen s01's `when` to 813 and change the neighbour's
  line about the ships; s04 would then never come, since the Eider peace is 811.
- A way to tell a christened captive from a heathen one, and a ransom, are still missing from the tables
  (`furs_amber_ivory_slaves.md`, section 11, items 23 and 24). The chain works without them: nobody here is cargo.
- People for the places files, if the integrator wants them met outside the cards: Frethburg, Liafbern and
  Wibern at Doccinga; Ingimar at Birka; Gundbert of Verdun and the archbishop's steward at Mogontia.
