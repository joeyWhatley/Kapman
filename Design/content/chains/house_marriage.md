# house_marriage: a marriage for the house

Written 4 Oct 2026. Cards: `Assets/Kapman/Game/Data/encounters/chain_house_marriage_793.json` (14 cards).
Words: `Assets/Kapman/Game/Data/writing/chain_house_marriage.json` (keys `ch_house_marriage.*` and `art.<card id>`).
A draft in the game's own files, for the user to overwrite. `(?)` marks what I could not check; LATER marks a source
written long after 793.

## The story in five sentences

A house with one man in it is a boat with a bed, and the widow who keeps the storehouse on the dam at Dorestad says so
and offers to ask about a wife. Four houses would hear a suitor from the dam: a Frisian farmer's on the mounds at
Winaldum, the bishop's steward's at Traiectum, a Danish host's at Ripa, and at Eoforwic the kin of the nobleman's son
whom the widow's own husband killed in the year the Frisians fled York. The merchant chooses one, finds someone to speak
for him, goes to ask, and learns that the price is the small part: a mother's brother, a bishop, a dead man's brother or
a girl's gods must agree first. He comes back months later with the price, weds before her kin in the way of her
people, gives her a morning gift that is hers alone, and takes her home across the sea. A year on the keel comes home to
a lit house, and what the alliance opened is his: a brother-in-law's boat, a plot under Saint Martin, a Frisian hearth
in York again, or a landing among the Danes.

## How a player meets it

The first card (`s01_empty_house`) is an ordinary drawn card at Dorestad, weight 0.3 (likelier where his standing is
good), from his third call there (`port.visits >= 2`) and before 804. The widow stops him at the dam head. If he says
"not this year" she asks again after 120 days. Every later card is scheduled by the one before it, at the port where it
happens, so nothing depends on luck after the start, and a step that is not visited simply waits.

## The real history under it

**What is certain (read tonight).**

- **The kin's will, among Frisians.** Lex Frisionum IX.11 (written down about 802): "Si liberam foeminam extra
  voluntatem parentum eius, vel eorum qui potestatem eius habent, uxorem duxerit, componat tutori eius solidos XX, id est
  denarios LX": who takes a free woman to wife without the will of her parents or of those who have power over her pays
  her guardian 20 solidi; 30 for a noble woman (IX.12). The same law prices the hand of "a woman who makes fresum",
  Frisian cloth, a quarter higher than another's (Additio sapientum II.24). Text and translation: K. Nieuwenhuijsen,
  http://www.keesn.nl/lex/lex_en_text.htm . This is the uncle's speech at Winaldum.
- **The priest asks into the blood.** Capitulary of 802 c. 35, tr. E. F. Henderson, Select Historical Documents of the
  Middle Ages (1896): "let them not presume to be wedded until the bishops and priests, together with the elders of the
  people, shall diligently inquire into the degree of blood-relationship between those being joined together"
  (https://avalon.law.yale.edu/medieval/capitula.asp). The priest's line that names the emperor's order shows only from
  802; the rule against marrying kin is older.
- **Buying a maiden, and the morning gift, among the English.** Laws of Æthelberht of Kent (about 600), tr. B. Thorpe
  1840 as printed in Thatcher 1907 (https://sourcebooks.fordham.edu/source/560-975dooms.asp): 77 "If a man buy a maiden
  with cattle, let the bargain stand, if it be without guile; but if there be guile, let him bring her home again, and
  let his property be restored to him"; 81 "If she bear no child, let her paternal kindred have the fioh and the
  morgengyfe." The monk at Eoforwic gives the first as old Kentish law; the bride's line on the morning after rests on
  the second.
- **A king's marriage as policy, in the game's first year.** Anglo-Saxon Chronicle 792 (Ingram 1823,
  gutenberg.org/ebooks/657): "Ethelred this year, on the third day before the calends of October, took unto himself a
  new wife, whose name was Elfleda": King Æthelred of Northumbria and Offa's daughter. The uncle's bitter line about it
  shows until 796, when Æthelred was killed. The quarrel that shut the Channel ports about 789 was itself over a
  marriage (Charles's son and Offa's daughter: Gesta abbatum Fontanellensium c. 12, in the spine's thread black_stones).
- **The Frisians' flight from York.** Altfrid, Vita Liudgeri I.11 (the spine's thread york_blood): about 773 a Frisian
  merchant killed the son of a count of that province in a brawl, and the Frisians left England "fearing the anger of
  the dead youth's kinsmen". Nothing says they came back.
- **The Frisian wife.** Exeter Book, Gnomic Verses (Maxims I), tr. Benjamin Thorpe, Codex Exoniensis (London 1842),
  p. 339 (archive.org/details/codexexoniensisc00sociuoft): "Dear is the welcome guest to the Frisian wife, when the
  vessel stands, his ship is come, and her husband to home, her own provider, and she calls him in, washes his weedy
  garment, and to him gives new raiment; 'tis pleasant on land to him whom his love awaits." The book was copied about
  970; how old the verse is nobody knows (?). It is the one quotation in the chain, on the last card, and free to use.
- **Saint Martin's bank at Dorestad.** Louis the Pious, 18 March 815 (Oorkondenboek van het Sticht Utrecht I no. 56, in
  `dossiers/dorestad.md`): Utrecht's church holds bank plots at Dorestad; its men there owe no bank due and no one may
  lodge in their houses or seize their goods at death. The king's tenth on sales was still taken from everyone, even the
  emperor's own merchants (828). So the Frankish reward is a plot and an immunity, not a toll charter.
- **The places.** Winaldum: one great farmstead on a sod mound, sunken weaving huts, Rhenish glass, no church
  (`dossiers/wijnaldum.md`). Traiectum: the bishop's stone fort with two churches and no named layman
  (`dossiers/traiectum.md`). Eoforwic and Ripa: the notes in `encounters/npc_roles_793.json`.
- **Lots, and the sign of the cross.** Rimbert, Vita Anskarii c. 19, 24, 27 (tr. Robinson 1921), in the spine's
  gods_and_faith: Danes and Svear ask by lot; at Sliaswic many "willingly signed with the cross" and put off baptism.
  This is of the 850s: LATER for 793.

**What is likely, from sources I know but did not re-read tonight (?).**

- Frankish weddings in public, "noble and common alike": Council of Ver, 755, c. 15 (?).
- The widow's portion where no deed was written: 50 solidi as dower, a third of what the pair gained together, and the
  morning gift (Lex Ribuaria 37) (?). The deed read out at Traiectum follows it with a merchant's smaller sum.
- The pledge "by a shilling and a penny": the Salian betrothal of the formularies and of Fredegar III.18 (?).
- The English order of a betrothal: the groom promises to keep her "by God's law as a man should keep his wife", his
  friends go surety, he names what he gives for her rearing and what is hers if she outlives him, and her kin take care
  when she is led into another land: "Be wifmannes beweddunge" (Thorpe 1840, "Of betrothing a woman"), of about
  1000-1030: LATER (?).
- The fourth knee and kinship at the font as bars: Northumbrian Priests' Law 61 (Thorpe 1840, read tonight), of about
  1020: LATER.
- The mother's brother as the kinsman who must be asked: Tacitus, Germania 20, seven centuries early for Frisians (?).
- Among the Danes: bride-price (mundr), what she takes from home (heimanfylgja), a handclasp before witnesses, the ale
  within the year, six men who see the pair to bed: the Icelandic and Norwegian laws of the 12th and 13th centuries,
  LATER, and (?) for Jutland in 793. The hammer laid in the bride's lap: Thrymskvitha 30 (Bellows 1923; manuscript about
  1270), LATER (?). The mark as a weight is written first about 880 (?). Sigfred as king of the Danes: Annales regni
  Francorum 777, 782, 798 (?); his line ends in 798.
- That the bishops forbade a Christian to wed a heathen (?), and a yearly due of wax from a church's dependants (?).

**What is the game's.** Every price in pennies (a pound of pennies at Winaldum, a shilling and a penny and a pound of
dower at Traiectum, a pound and a half or two pounds at Eoforwic, two marks weighed at Ripa); the Frisian word for the
bride-price, *wetma*, is written down only later (?). The niece at Eoforwic, born after her father's death, is this
chain's invention; her uncle and the widow are the spine's.

## The people

| Who | Card role (the forge names them) | Notes |
|---|---|---|
| The widow on the dam | `widow`, fixed at Dorestad | The spine's cast.thiadburg. Her dead husband struck the blow at York. |
| The priest of Dorestad | `priest`, fixed | Invented; writes the letter, and a year on asks about the font. |
| The steersman and the mate | `steersman`, `mate` | The spine's cast.crew.aldger (the boy on the ship that fled York) and cast.crew.brunger. |
| The bride, in every house | `weaver`, fixed at her port, 17 to 24 | Invented. One woman from the asking to the morning after; her feeling toward the merchant opens the best choices. |
| Her father at Winaldum, and her brother | `farmer`, `skipper`, fixed | Invented: a free Frisian with sheep and a boat. |
| Her mother's brother at Winaldum | `lawman`, fixed | Invented; says the law of IX.11 and wants querns. |
| The bishop's steward at Traiectum | `steward`, fixed | Invented; the bishop (cast.hamacar) stays offstage. |
| The dead man's brother at Eoforwic | `lords_man`, fixed | The spine's cast.ecgwulf; guardian of his niece. |
| The monk at Eoforwic | `monk`, fixed | The spine's cast.sigewine. |
| The host at Ripa and the old woman | `host`, `wise_woman`, fixed | Invented: the Dane who answers for Frisian ships, and the caster of lots. |

## The steps

"+n d" is the wait the card sets before the next one can come.

| # | Card (`…chain_house_marriage.`) | Where, when | The choices, and the flags they set |
|---|---|---|---|
| 1 | `harbour…s01_empty_house` | Dorestad, drawn, year < 804 | Let her ask · give her 12 d to ask properly (`…_asked_well`: hints at step 2) · not this year (`…_wait` for 120 days). Sets `ch_house_marriage_1`. +14 d |
| 2 | `harbour…s02_four_houses` | Dorestad | Choose the house: `…_road_frisian`, `…_road_frankish`, `…_road_english` (also the spine's `yk_feud_known`), `…_road_danish`. Sets `ch_house_marriage_2`; schedules steps 4 and 5; step 3 follows at once. |
| 3 | `harbour…s03_forespeaker` | Dorestad, at once | Carry the widow's half-tally (`…_spoke_widow`) · give 12 d to the church and carry the priest's letter (`…_spoke_priest`) · speak for yourself (`…_spoke_self`). Sets `ch_house_marriage_3`. |
| 4 | `crew…s04_crew_counsel` | Aboard, +2 d | Ask the steersman (`…_counsel`) · stand the crew a cask and listen (`…_counsel`, heart up) · tell them to mind the oars. Sets `ch_house_marriage_4`. |
| 5 | `harbour…s05_frisian_mound` | Winaldum, +5 d | Ask her what her cloth sells for (needs counsel) · lay down the widow's tally (needs it) · promise the uncle querns (`…_promised_stones`) · ask plainly (likely; failure: `…_hard`). Else `…_agreed`. Wedding +60, +75 or +120 d. |
| 5 | `harbour…s05_frankish_table` | Traiectum, +3 d | Hand over the letter (needs it; +40 d) · give two cakes of wax (needs counsel; +60 d) · ask the daughter what a dower should hold (+90 d) · name your kin and wait for the asking (+75 d). |
| 5 | `harbour…s05_english_uncle` | Eoforwic, +10 d | Say whose house stands behind you (needs the widow's tally; `…_honest`, `yk_named`; +120 d) · say you are a merchant of Dorestad and no more (doubtful to likely; failure: `…_found_out`, `yk_named`, +150 d; success +90 d) · ask the monk to take it to the archbishop's man (needs the letter; `…_church_peace`; +120 d). |
| 5 | `harbour…s05_danish_hearth` | Ripa, +8 d | Put your purse on her scale (needs counsel) · give the father a cask and name no price · offer a mark and a half (doubtful; failure: `…_hard`, +160 d). Else `…_agreed`, +100 d. |
| 6 | `harbour…s06_frisian_bride_ale` | Winaldum | Count out a pound · count out the pound and give the uncle two pairs of querns · ask for one more season (+60 d, the card comes again). **Ending `ch_house_marriage_end_frisian`.** |
| 6 | `harbour…s06_frankish_deed` | Traiectum | Earnest and dower under Saint Martin's condition (`…_owes_st_martin`) · the same, keeping the house the count's · one more season. **Ending `ch_house_marriage_end_frankish`.** |
| 6 | `harbour…s06_english_peace` | Eoforwic | Promise her first son the dead man's name and pay a pound and a half (needs `…_honest`) · pay two pounds · lay a pound in the archbishop's man's hand (needs `…_church_peace`; `…_owes_archbishop`) · one more season. **Ending `ch_house_marriage_end_english`, with `changed_york_773`.** |
| 6 | `harbour…s06_danish_handclasp` | Ripa | Two marks and her gods come with her (`…_wife_heathen`, `…_watchful_bishop`) · two marks and she takes the sign of the cross as a guest (needs her regard; `…_wife_signed`) · pay in wine and glass (as the first) · one more season. **Ending `ch_house_marriage_end_danish`.** |
| 7 | `harbour…s07_morning_gift` | The same port, at once | The keys and a third (`…_gift_keys`) · sixty pennies of her own (`…_gift_silver`) · a pair of brooches (`…_gift_fine`). Sets `ch_house_marriage_7`. +300 d |
| 8 | `harbour…s08_keel_home` | Dorestad | Go in · go in and count her third (needs `…_gift_keys`) · first thank the widow with 60 d. Sets `ch_house_marriage_8`, `…_done`, `…_child`. |

Each wedding sets `ch_house_marriage_6` and carries the bride home as a passenger to Dorestad.

**No dead ends.** A suit that goes badly is never refused: it costs time (a longer wait) and regard. A man who cannot
pay asks for one more season and the card comes again. A man who promised the uncle his stones must bring them. Dangers
are told beforehand: the widow says at step 2 that York may show him "the door, or a blade", the steersman says it again
at step 4, and the card at Ripa says what the priest at home will ask.

## The four endings

**1. The Frisian wife** (`ch_house_marriage_end_frisian`): what the sources show. A Frisian skipper weds a
farmer's daughter of the mounds before her kin, her mother's brother having been asked first, and she weaves the cloth
he sells. Her brother's boat sails with his (`unlock_partner_winaldum_brother`); if he gave the uncle his querns as a
gift, the mound keeps his goods as its own (`unlock_factor_wijnaldum`). York stays shut to Frisians. In later years her
kin are on the coast the fleets come to: 810 (Godfred's two hundred ships on the Frisian islands), the flood of
26 December 838, the wasting of Westergo in 846.
Picture: the top of the mound on a windy bright day, kin in a ring, pennies counted onto a cloth, the bride's hand in
her father's, a muddy priest, a second boat on the creek bank below.

**2. Under Saint Martin** (`ch_house_marriage_end_frankish`). He weds the bishop's steward's daughter at the
church door, with a written dower and the bishop's leave. Taking the bishop's condition puts his house on Saint
Martin's bank at Dorestad (`unlock_plot_dorestad`) and makes him the bishop's man for a pound of wax a year and letters
carried (`ch_house_marriage_owes_st_martin`); refusing it keeps him the count's and gives him a corner of the steward's
storehouse inside the stone fort (`unlock_factor_traiectum`). She reads, and he begins to. In later years: the bishop's
bank is where the Danes land in 834 and after; the fort at Traiectum is sacked in 857 and the see moves to Deventer.
Picture: the door of Saint Martin's on a still grey morning, a priest reading the deed, the steward's open hand, the
bride beside a small iron-bound chest.

**3. The peace-weaving** (`ch_house_marriage_end_english`, with `changed_york_773`): **history changed.** In
the sources the Frisians ran from York about 773 and are not heard of there again. Here the dead man's daughter marries
a Frisian of the very house that owed the blood, her uncle stands witness, and a Frisian hearth is lit again by the
Foss (`unlock_plot_eoforwic`). The honest road, told to his face and sealed by promising her first son the dead man's
name, also brings the kin's protection (`unlock_title_northumbrian_kin`) and her father's share kept for her since his
death. The marriage does not pay the blood-price: the spine's thread york_blood stays open (`yk_paid` is only read),
and the uncle says so. What follows in later years: Frisian ships lie at Eoforwic through the ninth century, Rhine wine
and querns go north by this house, and its plot stands in the city the great army takes on 1 November 866. Later cards
can ask `flag.changed_york_773`.
Picture: the strand by the Foss under a cold clear sky, the kin in a half circle, the thegn with one hand raised, the
bride in a cloak pinned with a silver ring, and out on the river the Frisian boat with one man sitting very still in her
stern.

**4. The handclasp** (`ch_house_marriage_end_danish`). He weds the daughter of the Dane who answers for Frisian
ships at Ripa, by handclasp and weighed silver, with the hammer laid in her lap. He has a plot and a landing there
(`unlock_plot_ripa`) and the Danes' tongue in his house. If her gods come untouched, the priest at Dorestad will ask
about the font (`ch_house_marriage_wife_heathen`, `ch_house_marriage_watchful_bishop`). If she likes him well enough to
take the sign of the cross as a guest's courtesy (`ch_house_marriage_wife_signed`), both sides are content and her
father's men answer for his boat on that coast (`unlock_title_danish_guest_right`). In later years: he is a Dane's
kinsman when Godfred's fleet comes to Frisia in 810, and when the first church is allowed at Ripa in the 850s.
Picture: night in the host's house, firelight, two hands clasped over a pair of scales heaped with silver, the bride
seated with a small iron hammer in her lap, the old woman behind her.

## Every payout

A cask of Rhine wine is 60 d, a bolt of Frisian cloth 48 d, of plain wool cloth 18 d, a crate of glass 72 d, of Tating
jugs 24 d, a cake of wax 40 d, a sack of amber 30 d, of jet 24 d, a timber of marten 120 d (goods_793.json).

| Step | Option | Costs | Gives |
|---|---|---|---|
| 1 | let her ask / pay her | 0 / 12 d | the widow's regard +3 / +8; paying brings four true hints at step 2 |
| 3 | tally / letter / alone | 0 / 12 d / pride | the tally opens choices at Winaldum and Eoforwic; the letter at Traiectum and Eoforwic, and counts as alms |
| 4 | ask / cask / oars | 0 / a cask of ale / pride | the steersman's advice (opens the best choice at step 5); the cask also lifts the crew's heart |
| 5 Frisian (the turn) | cloth / tally / stones / plain | 0 | an order for 4 casks of wine at half as much again (about 120 d over the price), wine dear at Winaldum for the season, 2 bolts of cloth (96 d) and her regard on the first |
| 5 Frankish (the turn) | letter / wax / ask her / recite | 0 / 2 cakes of wax | an order for 6 cakes of wax at 1.4 times the price (about 96 d over); for the wax a crate of glass (72 d) and alms; her regard for asking her |
| 5 English (the turn) | honest / plain / church | 0 | an order for 6 or 4 casks of wine at 1.6 times the price (about 215 or 145 d over); on the honest road the text `deor` and her regard |
| 5 Danish (the turn) | weigh / cask / haggle | 0 / a cask of wine | an order for 4 casks at 1.6 times the price (about 145 d over), wine dear at Ripa, 2 or 3 sacks of amber (60 or 90 d), her regard for the scale |
| 6 Frisian | pound | 240 d | 20 bolts of Frisian cloth (960 d), `unlock_kin_wife_frisian`, `unlock_partner_winaldum_brother` |
| 6 Frisian, the honest road | pound and stones | 240 d and 2 pairs of querns | 30 bolts (1,440 d), the same two flags and `unlock_factor_wijnaldum` |
| 6 Frankish | under Saint Martin | 253 d | 300 d, 6 crates of glass, 6 of Tating jugs (876 d in all), Latin +0.25, `unlock_kin_wife_frankish`, `unlock_plot_dorestad`; lasting cost `ch_house_marriage_owes_st_martin` |
| 6 Frankish | the count's man still | 253 d | the same goods, `unlock_kin_wife_frankish`, `unlock_factor_traiectum` |
| 6 English, the hard and honest road | weave | 360 d | 960 d, 20 bolts of wool cloth, 6 sacks of jet (1,464 d in all), Old English, `unlock_kin_wife_english`, `unlock_plot_eoforwic`, `unlock_title_northumbrian_kin`, `changed_york_773` |
| 6 English | two pounds | 480 d | 480 d, 20 bolts, 5 sacks of jet (960 d), Old English, `unlock_kin_wife_english`, `unlock_plot_eoforwic`, `changed_york_773` |
| 6 English | the archbishop's peace | 240 d | 360 d and 14 bolts (612 d), Old English, the same three flags; lasting cost `ch_house_marriage_owes_archbishop` |
| 6 Danish | her gods come with her | 260 d, or 3 casks and a crate of glass | 360 d, 8 sacks of amber, 2 timbers of marten (840 d), Old Norse, `unlock_kin_wife_danish`, `unlock_plot_ripa`; lasting cost `ch_house_marriage_watchful_bishop` |
| 6 Danish, the honest road | the sign of the cross | 260 d and her regard | 480 d, 8 sacks of amber, 3 timbers of marten (1,080 d), Old Norse, `unlock_kin_wife_danish`, `unlock_plot_ripa`, `unlock_title_danish_guest_right` |
| 7 | keys / purse / brooches | 0 / 60 d / a pair of brooches | her regard; the keys open the larger sum at step 8 |
| 8 | go in / count her third / thank the widow | 0 / 0 / 60 d | 120 d / 300 d / 120 d and the widow's regard +25; the crew's heart up |

**Why a merchant could really have won these.** A brother-in-law with a boat is the oldest partnership there is. Goods
left with a wife's kin need no charter. Saint Martin's bank plots and their freedom are in the charter of 815. A
storehouse inside a stone fort is what the merchants of Utrecht ran to in 1007 (LATER). A Frisian quarter at York is
attested before 773, and a stranger in an English town needed a host or kin to answer for him. Ripa was laid out in
plots for traders from about 705, and a Frisian trading household is known at Kaupang in the ninth century (Skre 2010,
in `peoples/norse.md`) (?). Among Danes a stranger's right is his host's.

**Lasting costs.** `ch_house_marriage_owes_st_martin` (a pound of wax each Martinmas and the bishop's letters
carried), `ch_house_marriage_owes_archbishop` (a pound held a year and a day; the archbishop's man watches how she is
kept), `ch_house_marriage_watchful_bishop` (a heathen wife in a Christian port: the priest has not written to the
bishop, yet). Later cards may read them.

## How long it takes

Fourteen days from the first card to the choice; three to ten days' sail to the asking; then 40 to 160 days to the
wedding (a letter at Traiectum is quickest, a concealment found out at Eoforwic or a haggle at Ripa slowest), longer by
60 or 90 days for each season asked; then 300 days to the last card. From first card to last: a little under a year at
the least, thirteen to sixteen months as a rule. The wedding goods are light enough for the Gislinge boat (the largest
load, 30 bolts of cloth, is 180 kg).

## Pictures

One line each, as in the writing file under `art.<card id>`.

1. `s01_empty_house`: evening at a dam head in Dorestad, low sun; the widow in a white head-cloth with keys at her belt stops a young skipper beside his small boat.
2. `s02_four_houses`: inside her storehouse, rain outside, one tallow light; four tally sticks in a row on the head of a wine cask.
3. `s03_forespeaker`: a bright morning outside a small wooden church; she splits a tally stick and holds out half, a priest on the step writes on his knee.
4. `s04_crew_counsel`: at sea under a grey sky; the steersman looks straight ahead and says nothing, the mate grins, one ale cask amidships.
5. `s05_frisian_mound`: a sod house mound above the salt marsh, late sun; two older men on a bench, the merchant standing, a young woman at a loom in the mouth of a sunken hut.
6. `s05_frankish_table`: a timber hall inside Roman stone walls; the steward at a trestle table, a priest with a wax tablet, the daughter pouring from a black jug.
7. `s05_english_uncle`: a Northumbrian hall, noon light through smoke; a tall thegn in a dark red cloak, a monk, the Frisian alone on the floor, a young woman at the back.
8. `s05_danish_hearth`: a longhouse at dusk; the host on his bench, an old woman laying three marked twigs on a white cloth, the daughter in the doorway with folding scales.
9. to 12. The four endings, as above.
13. `s07_morning_gift`: early morning, pale light through an open door; the new wife in a married woman's head-cloth for the first time, a ring of keys held out, a purse and a pair of brooches on the chest.
14. `s08_keel_home`: dusk at the dam a year later, light rain; the boat made fast, a lit doorway with a woman holding a child, the widow watching from under her eave.

## For the other writers

- `ch_house_marriage_6` means the merchant is married; `ch_house_marriage_child` (set at step 8) is for the chain
  house_heir. The four `unlock_kin_wife_*` flags say of which people the wife is.
- The chain reads `yk_paid` and sets `yk_feud_known` (the widow tells the story when the English house is chosen) and
  `yk_named` (when the kin at York learn whose house he is of), as the spine's thread york_blood names those steps. It
  never sets `yk_price_set` or `yk_paid`.
- Wanted from the spine (not added here): a text of the Frisian-wife verse in texts_793.json, so the last card can put
  it in the chest; a role `bride` or `wife`, so her speaker line does not read "the weaver"; rows in the roles file's
  `named` list for cast.thiadburg, cast.ecgwulf and cast.sigewine, so the forge gives them the spine's names; a deed for
  a marriage made before witnesses, so each people can weigh it in his standing.
