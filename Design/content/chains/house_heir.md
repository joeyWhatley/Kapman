# house_heir: an heir for the house

Written 6 Oct 2026. Cards: `Assets/Kapman/Game/Data/encounters/chain_house_heir_793.json` (14 cards).
Words: `Assets/Kapman/Game/Data/writing/chain_house_heir.json` (keys `ch_house_heir.*` and `art.<card id>`).
A draft in the game's own files, for the user to overwrite. `(?)` marks what I could not check; LATER marks a source
written long after the years of the chain.

## The story in five sentences

The boy of the house knows every knot and one tongue, and the widow on the dam says one tongue is a poor cargo: three
roofs would take him, the bishop's school at Traiectum, an English house above the Strand at Lundenwic, or a Danish
hearth at Ripa. The merchant takes him there himself, settles his keep, hears a year later that he is in trouble, and
fetches him home after three winters a hand taller and speaking like the people who raised him. On his first watch at
the steering oar there is a bar ahead at dusk, and what the father does with his own hands then is remembered. A year
on the boy wants a share and his own way with it, and they quarrel. Two years after that the merchant takes the wrong
side of a withy at dusk, his steersman tells him his eyes are going, and he must choose how the house goes on: the oar
handed over before witnesses, the house divided into two keels, or the whole house set down in writing, which no
merchant of the north did in our history.

## How a player meets it

The first card (`s01_three_roofs`) is an ordinary drawn card at Dorestad, weight 0.3, from the merchant's third call
there, in the years 800 to 825. It comes from 806 if the chain house_marriage has left a child
(`flag.ch_house_marriage_child`: the boy is then his son), from 800 if no marriage was ever begun (the boy is a dead
sister's son, brought down the coast by a skipper), and from 814 in any case. "Not this year" brings it again after 120
days. After the start nothing depends on luck but the first watch, which is a sea card at 0.05 an hour (likelier than
not within one crossing) once the boy is aboard. Every other card is scheduled by the one before it at the port where it
happens, and a step that is not visited simply waits. The chain is over before 834.

The heir is never an actor on a card: the engine's forge would give him a new name at every port. His words are lines
without a speaker, and after the first card no line calls him son or nephew, so the same words serve both.

## The real history under it

**What is certain (read for this chain or already in the spine and dossiers).**

- **A Frisian boy sent out to learn.** Altfrid, Vita Liudgeri I.9-12 (written 839-849; ed. Diekamp 1881; the spine's
  threads cross_and_grove and york_blood): Liudger, born near Utrecht about 742, was given as a boy to Gregory's school
  at Saint Martin's, went on to Alcuin at York for three years and six months, and came home with the Frisian merchants
  who fled York about 773. The school at Traiectum is the school road; `npc_roles_793.json` has its master.
- **Boys bought and taught for a purpose.** Rimbert, Vita Anskarii c. 8 and c. 15 (tr. C. H. Robinson 1921, read at
  sourcebooks.fordham.edu/basis/anskar.asp): with King Harald about 826 Anskar and Autbert "established a school for
  twelve or more boys"; at Hamburg he "began to buy Danish and Slav boys and to redeem some from captivity so that he
  might train them for God's service". Sending a boy across a border to learn was an ordinary tool of the age.
- **A father who had his children taught, and began to learn too late himself.** Einhard, Vita Karoli c. 19 and c. 25
  (tr. S. E. Turner 1880, read at sourcebooks.fordham.edu/basis/einhard.asp): "The plan that he adopted for his
  children's education was, first of all, to have both boys and girls instructed in the liberal arts"; and of himself,
  he "used to keep tablets and blanks in bed under his pillow ... however, as he did not begin his efforts in due
  season, but late in life, they met with ill success."
- **The day a father hands over while he lives.** Einhard c. 30 (Turner 1880), September 813 at Aachen: "He appointed
  Louis, with their unanimous consent, to rule with himself over the whole kingdom and constituted him heir to the
  imperial name; then, placing the diadem upon his son's head, he bade him be proclaimed Emperor." The widow says this
  on the day card in 813 to 816. Before it, the Divisio regnorum of 6 February 806 gave each of Charles's three sons a
  portion (Annales regni Francorum 806): her line at the quarrel from 806. After it, Louis shared the realm among his
  own sons in 817 (Ordinatio imperii), they rose against him in 830, and in June 833 on the field near Colmar his men
  went over to them (Annales Bertiniani; `chronicle_793.json`): her lines for 817-829, 830-832 and 833.
- **Frisians at London.** Bede, Historia ecclesiastica IV.22: a Frisian bought the captive Imma at London in 679; II.3:
  London a mart of many peoples coming by land and sea. The English road.
- **Ripa** as the Danes' summer market full of Frisian ships (`npc_roles_793.json`); no coin among the Danes before
  about 825, silver by weight (BRIEF.md rule 2). The Danish road.
- **Saint Martin's men at Dorestad.** Louis the Pious, 18 March 815 (Oorkondenboek van het Sticht Utrecht I no. 56,
  in `dossiers/dorestad.md`): the church's men on the bank owe no bank due. This is what the school's boy offers when
  the house is divided.
- **A word-list as the way to a tongue, and its joke.** The Kassel glosses, written about 810 by two scribes of the
  Regensburg country (read at en.wikipedia.org/wiki/Kassel_conversations): "Stulti sunt Romani, sapienti sunt Paioari":
  the Romans are fools, the Bavarians wise. The schoolmaster tells it from 810.
- **Rumours on the word card.** Godfred's two hundred ships on the Frisian islands and the hundred pounds of silver
  (Annales regni Francorum 810); the emperor's death on 28 January 814, said as "a week before Candlemas".
- **The verse.** Hávamál 72, tr. H. A. Bellows, The Poetic Edda (1923): "A son is better, though late he be born, and
  his father to death have fared; memory-stones seldom stand by the road save when kinsman honors his kin." The one
  quotation in the chain, on the card of the oar; quoted from memory of Bellows and to be checked against the print (?).
  The manuscript is of about 1270 and the stanza is the Northmen's, so the card says only that the Danes have it.

**What is likely, from sources I know but did not re-read (?).**

- Gregory's pupils at Utrecht were Franks, English, Frisians, Saxons, Bavarians and Swabians: Liudger, Vita Gregorii
  c. 11 (?). The master names four of them.
- Schools where boys may learn to read: Admonitio generalis 789 c. 72 (?). Monastery schools shut to all but boys
  given to God: council of Aachen 817 (?); the master's line from 817.
- Latin only at table and the rod: the later colloquies; Ælfric's boys would rather be flogged than not know (about
  1000, LATER).
- The host who answers for a guest after three nights: Hlothhere and Eadric c. 15 (?). A boy of ten winters answers
  for himself among the Kentish: Hlothhere and Eadric c. 6, Ine 7.2 (?). Fifteen among the Ribuarian Franks: Lex
  Ribuaria 81 (?); twelve by the Salic law (?).
- The short Frisian cloak sold at the price of the long one: Notker, Gesta Karoli I.34 (tr. Grant 1907), about 885,
  LATER, and told of Charles himself.
- Fostering in another man's house, the child set on the knee, and the fosterer held the lesser man: Heimskringla,
  Haralds saga hárfagra c. 39-41, of about 930 and written about 1230, LATER, and (?) for Jutland in the 800s.
- Horse flesh forbidden to Christians: Pope Gregory III to Boniface, 732, and the legates in England, 786 (?).
- Hammer and cross worn together: the mould from Trendgaarden in Jutland, 10th century, LATER (?).
- The sister's son as dear to his uncle as to his father: Tacitus, Germania 20, seven centuries early (?).
- An heir made before witnesses by a thing handed over: Lex Salica 46, De acfatmire, the stalk thrown into the heir's
  lap (?). Equal shares among sons: Lex Salica 59 (?). A written deed with witnesses as proof in court: Lex Ribuaria
  59-60 (?).
- A layman who gives to a church and is kept by it for life (precaria, prebend), as Frisians gave to Liudger's Werden
  from 799 (?).
- A deed cut in two along a wavy line so that each half proves the other (the chirograph): English from the ninth
  century (?). It is only in an outcome text.

**What is the game's.** The steering oar as the token of the house; every price; the silver paid in at each ending
(what the dams owe the house, called in when the master changes or when the names are said aloud to be written); the
boy, the widow's storehouse, the three hosts. Ages are kept vague on purpose ("of an age to answer for himself"),
because the chain compresses a fostering of five or six years into three winters.

## The people

| Who | Card role (the forge names them) | Notes |
|---|---|---|
| The boy, the heir | none: lines without a speaker | Invented. The son of house_marriage, or a dead sister's son. |
| The widow on the dam | `widow`, fixed at Dorestad | The spine's cast.thiadburg: she starts it, keeps the tally, and has kept a stool. |
| The priest of Dorestad | `priest`, fixed | Invented; wants the school, dislikes the hearth, writes the deed. |
| The master of Saint Martin's school | `schoolmaster`, fixed at Traiectum | Invented; no master is named for these years. The bishop (cast.hamacar to 806) stays offstage. |
| The English host | `host`, fixed at Lundenwic | Invented: lodges Frisians above the Strand and answers for them to the reeve. |
| The Danish host | `host`, fixed at Ripa | Invented: the Dane who answers for Frisian ships (the same office as in house_marriage). |
| The skipper with word | `skipper`, not fixed | Whoever is in from that coast. |
| The steersman | `steersman` | The spine's cast.crew.aldger: a ship's boy at twelve on the ship that fled York; says the worst first. |

## The steps

"+n d" is the wait the card sets before the next one can come.

| # | Card (`…chain_house_heir.`) | Where, when | The choices, and the flags they set | Picture |
|---|---|---|---|---|
| 1 | `harbour…s01_three_roofs` | Dorestad, drawn, 800-825 | The school (`…_road_school`, +3 d) · the English house (`…_road_strand`, +6 d) · the Dane's hearth (`…_road_hearth`, +8 d; the priest thinks worse of you) · not this year (`…_wait`, 120 d). Sets `ch_house_heir_1`. | The dam head, the widow pointing three ways, the boy splicing rope. |
| 2 | `harbour…s02_school` | Traiectum | Two cakes of wax and 60 d (`…_kept_well`) · promise letters carried and wax brought (`…_bound`) · ask for God's sake (pride). Sets `ch_house_heir_4`. +300 d to step 3, +900 d to step 4. | The stone schoolroom, the boy's thumbnail in a wax tablet. |
| 2 | `harbour…s02_strand` | Lundenwic | A cask of wine and 40 d (`…_kept_well`) · first choice of your querns (`…_bound`) · he will work for his bread (pride). As above. | The bank above the Strand, the boy counting ships. |
| 2 | `harbour…s02_hearth` | Ripa | A crate of glass and 30 d by weight (`…_kept_well`) · the first of your wine each summer (`…_bound`) · set him on the knee and say nothing (pride). As above. | The Dane's fire, his sons along the wall, a piece of dried fish held out. |
| 3 | `harbour…s03_word` | Dorestad, +300 d | Send your own knife and the word "hold", 12 d (`…_stood_by`) · tell his keepers not to spare him (`…_hard`) · send nothing. Sets `ch_house_heir_5`. Skipped if he is already fetched. | The skipper at the dam head holding out a wax tablet. |
| 4 | `harbour…s04_school_door` | Traiectum, +900 d | Thank the master, 60 d · take him and go · one more year (`…_stay`, `…_long`, +300 d, the card comes again). Sets `ch_house_heir_6`. | The church door, the boy pointing up at the cut letters. |
| 4 | `harbour…s04_strand_tide` | Lundenwic, +900 d | Thank the host, 48 d · take him and go · one more year. As above. | The tide line, the boy holding up four fingers to an eel-wife. |
| 4 | `harbour…s04_hearth_spring` | Ripa, +900 d | Thank the host, 48 d by weight · take him and go · one more year. As above. | The beach, five youths in a line, one with a cross and a bone hammer. |
| 5 | `sea…s05_first_watch` | At sea, once | Leave the oar with him (likely; success `…_proved`; failure `…_shaken`, the boat hurt a little) · stand behind him, hand over his (`…_beside`) · take the oar (`…_shamed`). Sets `ch_house_heir_7`. +365 d | The stern at dusk, white water ahead, the boy's feet wide. |
| 6 | `harbour…s06_quarrel` | Dorestad, +365 d | Hear him out and stake him, 120 d (`…_staked`) · he sails under you or not at all (`…_broken`) · say nothing (`…_cooled`). Sets `ch_house_heir_8`. +600 d | The house on the dam, a winter night, his fists on the table. |
| 7 | `harbour…s07_the_day` | Dorestad, +600 d | The oar (`…_9a`) · two keels (`…_9b`) · in writing (`…_9c`; needs the school's boy or 60 d) · one more season (`…_wait9`, 120 d, heart down). Sets `ch_house_heir_9`; each road goes at once to its ending. | The channel at dusk, the withy on the wrong side, the steersman's hand over yours. |
| 8 | `harbour…s08_oar` | Dorestad, at once | **Ending `ch_house_heir_end_oar`.** Keep the storehouse · ship as his crew one last summer (needs `…_staked` and `…_proved` or `…_beside`) · give a pound to Saint Martin and go under his roof (`…_father_cloister`). | See the endings. |
| 8 | `harbour…s08_two_keels` | Dorestad, at once | **Ending `ch_house_heir_end_two_keels`.** Part as friends, by the road he was raised on (not after `…_broken`) · let him buy the old boat and his third (`…_estranged`). | See the endings. |
| 8 | `harbour…s08_written` | Dorestad, at once | **Ending `ch_house_heir_end_written`, with `changed_merchant_writing`.** He writes it himself (needs the school road) · the priest writes it, 60 d · two leaves, one in the church's chest, 120 d (`…_deed_at_church`). | See the endings. |

Every ending sets `ch_house_heir_done`.

**No dead ends.** Each card has a choice that costs only time or pride. The boy is taken at every roof whatever is
given. A father who sends no word, takes the oar away or refuses the quarrel still reaches every ending card; what he
loses is the best choice on it. A boy who touches on the bar hurts the boat and the story goes on. Dangers are told
beforehand: the widow says what each roof does to a boy, each keeper says "I tell you now", the steersman names the bar
before the choice, and on the last day he says that next time it is a bank in the dark.

## The three endings

**1. The steering oar** (`ch_house_heir_end_oar`): what the custom shows. The oar is unshipped at the dam head and
handed across the wale before the men of the dams, as the Franks made an heir by a thing given before witnesses. The
heir is master of the boat and the house has an heir of full age whom every man on the dam has seen made
(`unlock_kin_heir`). The old man keeps the storehouse and a stool in its door. If he staked the boy at the quarrel and
trusted him at the bar, he may ship as his heir's crew for one last summer, and the old oar is hung over the house door
(`unlock_heirloom_fathers_oar`). Or he gives a pound to Saint Martin and lives under the church's roof, and the bishop's
men know the house's boat (`unlock_title_st_martins_man`, with `ch_house_heir_father_cloister`).
Picture: the dam head on a bright cold morning, men of the dams in a half ring, the old merchant holding the oar out
across the boat's side, the heir's two hands closing on its worn loom, the priest with a hand raised, the widow with a
stool under her arm.

**2. Two keels** (`ch_house_heir_end_two_keels`). The house is divided on its own floor in three heaps while the old
man lives, as the kings of the age divided theirs. The heir takes a third and a keel of his own and goes to the roof
that raised him; he is a partner who sails a second boat (`unlock_partner_heir`). Parted as friends he is also the
house's man there: Saint Martin's merchant at Traiectum (`unlock_factor_traiectum`), a hearth above the Strand
(`unlock_factor_lundenwic`), or one of his foster-father's own on the beach at Ripa (`unlock_factor_ripa`). A father
who refused him at the quarrel cannot part as friends: the boy buys the old boat and his third with his foster roof's
silver and will keep every bargain the tally names and nothing more (`ch_house_heir_estranged`, the lasting cost).
In later years the second hearth is what the house has when Dorestad burns in 834.
Picture: the swept storehouse floor at noon, cloth, casks and stones in three heaps with a little silver on each, the
widow between them with a split tally, father and heir on opposite sides, two boats on the river beyond.

**3. The house in writing** (`ch_house_heir_end_written`, with `changed_merchant_writing`): **history changed.** In
our history no merchant of Dorestad left a line of his own; all that is known of them was written by clerks about them,
and a merchant's own book is not heard of in the north for three hundred years. Here the house's goods, its debts, the
names of those who owe it and the shares of father and heir are set on a calfskin leaf by the church door and
witnessed. The leaf is the first of the house's book, which every heir inherits and adds to
(`unlock_heirloom_house_book`). If the boy learned his letters at Saint Martin's he writes it himself, in short words
a toll man can read, and father and heir are partners by its shares (`unlock_partner_heir`). Otherwise the priest
writes it; for a second leaf laid in the church's chest the toll man takes the house's tally on trust for a year
(`ch_house_heir_deed_at_church`). What would follow: a house that can prove a debt in the count's court after every
witness is dead; heirs who know what their fathers were owed, and by whom, in 834, in 863 and after the move to Tiel;
other houses on the dam asking the same priest for the same thing; and, for a historian, the one thing the real
Dorestad never left. Later cards can ask `flag.changed_merchant_writing`.
Picture: a table by the church door on a still grey day, a calfskin leaf pegged flat, an ink horn, the old merchant
making his mark, the men of the dams standing well back, and three of them slipping away toward their houses.

## Every payout

A cask of Rhine wine is 60 d, a bolt of Frisian cloth 48 d, a cake of wax 40 d, a crate of glass 72 d, a sack of amber
30 d, a timber of marten 120 d, a pair of walrus tusks 90 d, a bundle of parchment 60 d, a pair of querns 12 d
(goods_793.json). All the goods given are light: the boat carries 660 kg.

| Step | Option | Costs | Gives |
|---|---|---|---|
| 1 | any roof | 0 | the widow's regard; the priest's up for the school, down for the hearth |
| 2 school | wax and silver / service / plain | 2 cakes of wax and 60 d / 0 / pride | Symphosius's riddle of the ship and alms / an order for 4 cakes of wax at a quarter more (40 d over) and letters carried / the boy sleeps by the door |
| 2 Strand | wine and silver / service / plain | a cask and 40 d / 0 / pride | the Exeter riddle of the ship / an order for 6 pairs of querns at half as much again (36 d over) / nothing |
| 2 hearth | glass and silver / service / plain | a crate and 30 d / 0 / pride | the text havamal_005 / an order for 4 casks of wine at half as much again (120 d over) / nothing |
| 3 | knife / hard / nothing | 12 d / 0 / 0 | the skipper's and the widow's regard; a true rumour in 810 and in 814 |
| 4 school (the turn) | thank / take / stay | 60 d / 0 / time | reading: Latin +0.5 (+0.4 if only taken); thanked: Aldhelm's riddle of the tablets, alms, an order for 6 cakes of wax at 1.4 times the price (96 d over) |
| 4 Strand (the turn) | thank / take / stay | 48 d / 0 / time | the English tongue aboard; thanked: the Exeter riddle of the anchor, an order for 4 casks of wine at half as much again (120 d over) |
| 4 hearth (the turn) | thank / take / stay | 48 d / 0 / time | the Danes' tongue aboard; thanked: 3 sacks of amber (90 d), the text havamal_047, an order for 3 crates of glass at half as much again (108 d over) |
| 5 | leave / beside / take | a risk / 0 / his pride | the crew's heart; `…_proved` opens the best finale |
| 6 | stake / refuse / silent | 120 d / his regard / 0 | `…_staked` opens the best finale; refusing shuts the friendly division |
| 8 oar | keep the storehouse | 0 | 600 d, 6 bolts of Frisian cloth (288 d): 888 d; `unlock_kin_heir` |
| 8 oar, the hard road | one last summer as his crew | 0 (needs the stake and the bar) | 1,100 d, 8 bolts (384 d): 1,484 d; `unlock_kin_heir`, `unlock_heirloom_fathers_oar` |
| 8 oar | under Saint Martin's roof | 240 d | 700 d, 6 bolts (288 d): 988 d (748 d clear); `unlock_kin_heir`, `unlock_title_st_martins_man` |
| 8 two keels, friends, school | he sails for Saint Martin | 0 | 600 d, 8 cakes of wax (320 d), 2 bundles of parchment (120 d), Latin +0.2: 1,040 d; `unlock_partner_heir`, `unlock_factor_traiectum` |
| 8 two keels, friends, Strand | a hearth above the Strand | 0 | 900 d, an order for 8 casks of wine at half as much again (240 d over): 1,140 d; `unlock_partner_heir`, `unlock_factor_lundenwic` |
| 8 two keels, friends, hearth | his foster-father's beach | 0 | 500 d, 3 timbers of marten (360 d), 2 pairs of tusks (180 d), 4 sacks of amber (120 d): 1,160 d; `unlock_partner_heir`, `unlock_factor_ripa` |
| 8 two keels, cold | he buys the old boat and his third | pride | 700 d; `unlock_partner_heir`; lasting cost `ch_house_heir_estranged` |
| 8 written, the hard road | he writes it himself | 0 (needs the school road) | 1,000 d, Latin +0.2; `unlock_heirloom_house_book`, `unlock_partner_heir`, `changed_merchant_writing` |
| 8 written | the priest writes it | 60 d | 760 d (700 d clear); `unlock_heirloom_house_book`, `changed_merchant_writing` |
| 8 written | two leaves, one in the church's chest | 120 d | 820 d (700 d clear), half toll at Dorestad for a year, alms; `unlock_heirloom_house_book`, `changed_merchant_writing`; lasting tie `ch_house_heir_deed_at_church` |

**Why a merchant could have won these.** An heir sworn in before the dam (`unlock_kin_heir`) and a thing kept by the
house (`unlock_heirloom_fathers_oar`, `unlock_heirloom_house_book`) need no charter. A kinsman with his own keel is the
plainest partner there is (`unlock_partner_heir`). A trusted man in another port (`unlock_factor_…`) is what the
fostering was for: he was raised under that roof and is answered for there. Saint Martin's protection for a house that
gives to the church (`unlock_title_st_martins_man`) is the charter of 815 applied to one more man, and the same flag
the chain house_debt uses.

## How long it takes

From the first card: 3 to 8 days to the roof, 900 days to the fetching (1,200 with the extra year), a voyage or two to
the first watch, 365 days to the quarrel, 600 to the day. About five years and two months of game time at the
quickest, five of them spent trading as usual. Started in 806 it ends in 811 or 812, with Godfred's fleet (810) and the
crowning at Aachen (813) passing through it as rumours.

## For the integrator

- The game's own succession (the player going on as the heir) is not an effect verb. The endings only set flags:
  `ch_house_heir_end_oar`, `_end_two_keels`, `_end_written`, and `unlock_kin_heir`. See `needs` in the report.
- `learn latin` and `tongue <name> = 1` are given at the fetching as if the boy were aboard from then on; if the heir
  later sails his own keel (two keels) the tongue stays with the house, which is generous.
- The chain reads `flag.ch_house_marriage_child` and `flag.ch_house_marriage_1` (house_marriage) and nothing else of
  another chain. It sets `unlock_title_st_martins_man`, which house_debt also sets, with the same meaning.
