# Chain: riddle_trail (the sticks from a guest)

Cards: `Assets/Kapman/Game/Data/encounters/chain_riddle_trail_793.json` (13 cards).
Words: `Assets/Kapman/Game/Data/writing/chain_riddle_trail.json` (179 entries).
The spine's thread: `riddle_road` (it sets the thread's two flags, `rd_setter_known` and `rd_last_question`).
Written 6 Oct 2026. A draft for the user to overwrite. (?) marks what could not be checked against a source. Two pages were read on the web for it (Wikipedia, "Snoldelev Stone" and "Thyle"); no web search was made; the rest is from the project's own tables and the spine.

## The story in five sentences

A Danish skipper puts a rune stick "from a guest" into a Frisian shipman's hand at Dorestad, and each man who hears its answer (a carter at Ripa, a ship's master at sea, the ferryman on the Slie) hands on the next stick, until the trail runs up the long fjord to the hall of the Danish kings at Hleidr.
The men who hold the sticks all once sat on one bench, and so did Ravn, the Northman in the shipman's own boat, who knows the cutting from the first day and says so only at sea, at night: the setter is Hroald, who kept the old words at the king's feet, and who once asked Ravn a question that must not be answered.
Hroald is old and blind; his son Gunnvald, who held the seat after him, is dead, a stone lies uncut by his door, and he set the riddles to find a head that could hold what is now in one place only.
What lies at the end is not silver in the ground but a choice about memory: raise the son's stone and let the lore die with the father, give him back the pupil he lost, have it all written on calfskin by an English boy four and a half centuries before anyone in the north wrote such things, or open the son's mound by night for the rings.
Each road pays, and each leaves the old man, Ravn and the house somewhere different.

## How a player meets it

- It starts by itself at **Dorestad**, 796 to 805: card `s01_stick` is drawn on a call there once the house has called three times (weight 0.8, once). The first riddle goes into the chest whatever the player says.
- Steps 2, 4 and 6 are drawn with weight 5 at Ripa, Sliesthorp and Hleidr, so each comes on the next call there. Step 3 is a sea card and step 5 a crew card (rates 0.05 and 0.08 an hour), so each comes within a day or two under way. Steps 7 to 13 are scheduled.
- **Solving is rewarded and never required.** Every holder gives the next stick whether or not the last was answered; the answered road pays silver, a deed (`won_riddle`) and a true price word. Only the heir's road at Salhaugar is shut to a player who has solved fewer than five riddles in all or who bullied Ravn in step 5.
- No answer is given away in any line, label or outcome text.

## The real history under it

**The stone.** The Snoldelev stone (Rundata DR 248), found at Snoldelev on Zealand, first noted in 1810 and in the National Museum in Copenhagen since 1811, reads `kun'uAlts stAin sunaR ruHalts þulaR o salHauku(m)`: "Gunnvald's stone, Hroald's son's, thul at Salhaugar". It is an early inscription in the short rune row that still keeps the old h and a signs, dated to the 9th century and usually to about 800 (?); over the words are three interlocked drinking horns and a swastika. Salhaugar is taken to be Salløv, by Snoldelev, some twelve kilometres south-east of Lejre (the distance is from the map, not from the page read (?)). Source: en.wikipedia.org/wiki/Snoldelev_Stone, read 6 Oct 2026.

**The thul.** Old Norse *þulr*, Old English *þyle*: an office at a king's hall whose holder kept the knowledge of the past and judged what was said against it; "reciter" in Rundata's translation. Unferth is Hrothgar's thyle in *Beowulf* and sits at the king's feet; Hávamál calls Odin *fimbulþulr*, "the great thul"; Old English glosses give *orator* and *scurra*. What exactly the office was is not known. Source: en.wikipedia.org/wiki/Thyle, read 6 Oct 2026. The word thul is used once in the cards (the carver reads the stone); elsewhere the words say "the seat" and "the man who kept the hall's words".

**Whose title.** The grammar lets *þulaR* go with either man; Rundata gives it to Gunnvald. The chain has both hold the seat, father then son (?).

**The hall.** Great halls stood at Lejre from the 6th to the 10th century (the excavations of Tom Christensen, Roskilde Museum; `npc_roles_793.json` ports.hleidr); it is the Heorot of *Beowulf* by most reckonings (?). That a king of the Danes kept feasts there about 800 is likely and unrecorded (?). Sigfred is the king of the Danes in the Annales regni Francorum for 777, 782 and 798, and Godfred from 804 to 810; the lines name each only in his years, and nobody between.

**The riddles.** All from `riddles_793.json`, by their ids, and all Gestumblindi's from the saga of King Heidrek (written in the 13th century, older in substance; Vigfusson and Powell, *Corpus Poeticum Boreale*, 1883): `r-gest-02`, `r-gest-28`, `r-gest-05`, `r-gest-23`, `r-gest-12`, `r-gest-27`, `r-gest-34`. Four are given by the people the table gives them to (the carter of Ripa is made the hearer of 02 and the giver of 28, which the table gives to a cattle dealer there; the ship's master, the ferryman on the Slie, the swineherd and the king's skald at Hleidr are the table's own; 27 is the broken stick from the hall's midden). `r-gest-23` is one of the set `aegirs_daughters`. The setter signs himself Gest, "guest": Gestumblindi is "the blind guest", and in the saga he is Odin. The cards never say so. Hroald is blind.

**The last question.** What Odin said in Balder's ear before the pyre (`texts_793.json` t-gest-36, given with `give_text`; it also ends the lay of Vafthruthnir). No man can answer it: Heidrek and the giant both say so, and by it know who is asking. The chain turns this into the craft's own rule: a keeper of words must know what he does not hold, and not fill the gap. Ravn's want in the spine (to hear the question asked once more at Hleidr) is met in the heir's ending.

**Writing.** The lays, riddles and king-lists of the north were first written in Iceland in the 12th and 13th centuries (the Codex Regius about 1270); almost nothing of the Danes' own lore survives in their tongue. Northumbrian scribes did write English with the rune-letters thorn and wynn, which is how Beornwine spells Danish. Alcuin, writing in 797 to a bishop he calls Speratus, asked what Ingeld had to do with Christ, and said a churchman's table wanted a reader and not a harper (MGH Epistolae IV no. 124; our own words; the bishop was long taken for Higbald of Lindisfarne, now for Unwona of Leicester (?)). Ingeld belongs to the songs of the Danish hall (*Beowulf* 2020-2069), which is why the book has a churchman watching it.

**The mound.** The great ship mounds of Oseberg and Gokstad were broken into in the 10th century (?); the northern law-books, written later, treat robbing the dead as a shameful crime (?). *Niðingr*, a man without honour, is on later rune stones and in the laws; its use about 800 is assumed (?). That Gunnvald lay in a mound is ours: the stone was not found on a grave (?), and only the place name speaks of mounds.

**Silver.** North of the Eider silver goes by weight, in rings, wire and hacked pieces; no coin before about 825 (BRIEF.md rule 2). Every payout north of Dorestad is "so many pennies by weight".

**Invented, and said so.** Everything about Hroald and Gunnvald beyond their two names, the seat and the place: the blindness, the son's death at the winter nights, the riddles, the three horns over the seat, the daughter's sons. Ravn's youth at Hleidr is the spine's. The skipper, carter, master, ferryman, steward, swineherd, skald and carver have no names.

## The people

| Who | Id | In the cards |
|---|---|---|
| Hroald of Salhaugar | real name (DR 248); the rest invented | actor `setter` (role setter), cards 8, 10, 11, 12 |
| Gunnvald, his son | real name (DR 248); dead before the chain | spoken of in 8 to 13 |
| Ravn | `cast.crew.ravn` | actor `hand` (role crewman), cards 1, 5, 8 to 13 |
| Beornwine | `cast.crew.beornwine` | actor `lookout`, cards 5, 8, 12 |
| Aldger | `cast.crew.aldger` | actor `steersman`, card 9 |
| A Danish skipper of Ripa | invented | actor `skipper`, cards 1, 13 |
| The carter, the ship's master, the ferryman | invented | cards 2, 3, 4: Hroald's old pupils |
| The steward, the swineherd, the king's skald, the carver | invented | cards 6, 7, 10, 11 |
| The king (Sigfred to 798, Godfred 804-810) | real | seen across the fire, never an actor |
| Sigewine | `cast.sigewine` | named in the notes of card 12 (York's letter-carrier) |

## The steps

All card ids begin `<stage>.chain_riddle_trail.`; all flags begin `ch_riddle_trail_` unless written out.

| # | Card | Where and when | Choices | Flags set |
|---|---|---|---|---|
| 1 | `harbour…s01_stick` | Dorestad, 796-805, drawn | take the stick / have Ravn read it (runes +) / call it kindling (he leaves it anyway). All give `r-gest-02` | `_1` |
| 2 | `harbour…s02_carter` | Ripa, next call | answer (20 d, next stick, price word) / buy the ale (4 d) / help him load. All give `r-gest-28` | `_2` |
| 3 | `sea…s03_fog_hail` | at sea after 2 | shout the answer (15 d, price word) / close and take it / hold your course. All give `r-gest-05` | `_3` |
| 4 | `harbour…s04_ferryman` | Sliesthorp, next call. TURN | answer (120 d, Danish half learned) / row his ferry a day (30 d, Danish) / pay 10 d to be told. All give `r-gest-23` | `_4`, `rd_setter_known` |
| 5 | `crew…s05_ravn_speaks` | at sea after 4 | ask / take his watch (both: the text `t-gest-36`, `rd_last_question`) / press him as his paymaster (no text; shuts the heir's road) | `_5`, `rd_last_question` |
| 6 | `harbour…s06_kings_swine` | Hleidr, next call | answer the steward (`r-gest-12` and `r-gest-27`) / drive the king's pigs / a guest's gift (20 d). Feast in 3 or 5 nights | `_6` |
| 7 | `harbour…s07_hall` | Hleidr, scheduled +3 or +5 days. TURN | answer before the bench (150 d arm-ring) / drink the forfeit / buy the skald's riddle (12 d). All give `r-gest-34` | `_7` |
| 8 | `harbour…s08_salhaugar` | Hleidr, scheduled +2 days | raise the stone / name Ravn (needs `rd_last_question` and 5 riddles solved) / write it on skins (40 d) / mark the new mound | `_8` and `_road_stone`, `_road_heir` or `_road_written` |
| 9 | `harbour…s09_mound` | Hleidr, the next night, only by the fourth choice. Ravn and Aldger warn first | dig (doubtful) / pour ale on the mound / put the spades back. Failure and both refusals join the stone's road | `_9`, `_road_mound` or `_road_stone`; `_caught_digging` on failure |
| 10 | `harbour…s10_end_stone` | Hleidr, +60 days | ENDING 1 | `_end_stone` |
| 11 | `harbour…s11_end_heir` | Hleidr, +200 days (a winter) | ENDING 2 | `_end_heir`, `_ravn_heir` |
| 12 | `harbour…s12_end_written` | Hleidr, +150 days (a winter) | ENDING 3 | `_end_written`, `changed_norse_lore_written` |
| 13 | `harbour…s13_end_mound` | Dorestad, +60 days after a dig that succeeded | ENDING 4 | `_end_mound` |

## The endings

1. **The stone** (`ch_riddle_trail_end_stone`): what really happened. The stone is cut with two names, a seat and a place, and set up; nothing of what the two men knew is on it, and nothing of it has come down to us. It stands in Copenhagen today. Reward: the standing of a king's guest at Hleidr, which is the ordinary protection a foreign merchant had from a northern lord (merchants at Birka under the king and his prefect in Rimbert; Ohthere calling Alfred his lord).
2. **The heir** (`ch_riddle_trail_end_heir`, with `ch_riddle_trail_ravn_heir`): the hard road. The last question is asked again in the hall and Ravn says the one true thing about it. The seat that died with Gunnvald has a man waiting for it; Ravn sails on while the old man lives. A small change to the record (no thul of Salhaugar is heard of after the stone), so later cards can read `ch_riddle_trail_ravn_heir` and, when Hroald would be dead, let Ravn go ashore for good.
3. **The writing** (`ch_riddle_trail_end_written`, with `changed_norse_lore_written`): history changed. Five gatherings of calfskin hold the lore of a Danish hall, written about 800 in an English hand. **What would follow:** a book of northern lays four hundred and seventy years older than the Codex Regius. Kept in the house, it is an heirloom an heir can read from in any hall (later cards may read `unlock_heirloom_thuls_book`). Sent to York, it lies in the library Alcuin praised until that library is lost in 866 and after, unless a later card carries it out; and from 797 a churchman who has read the third skin asks who brought heathen songs to a minster (`ch_riddle_trail_bishop_watches`). Left at Salhaugar, it waits under a leaking roof for someone who can read, which among the Danes means a priest of the mission after 826 at the earliest.
4. **The mound** (`ch_riddle_trail_end_mound`): the low road. The silver is real and so is the word that follows it. Kept, the ring is an heirloom with a feud on it (`ch_riddle_trail_feud_salhaugar`: Danish skippers and dealers will not carry or buy; later cards in Danish ports should read it). Given to Saint Martin's it buys the church's protection and leaves the feud. Sent back with a man's price it ends the matter (`ch_riddle_trail_amends`).

## Payouts

| Step | Road | Silver | And |
|---|---|---|---|
| 1 | any | 0 | riddle `r-gest-02`; runes +0.05 if read aloud |
| 2 | answered | 20 d | `won_riddle`, riddle `r-gest-28`, price word: Frisian cloth x1.25 at Ripa, 60 days |
| 3 | answered | 15 d | `won_riddle`, riddle `r-gest-05`, price word: Rhenish wine x1.3 at Sliesthorp, 90 days |
| 4 (turn) | answered | 120 d | `won_riddle`, riddle `r-gest-23`, Old Norse half learned, `rd_setter_known` |
| 4 | rowed | 30 d | the same tongue and riddle |
| 5 | asked or waited | 0 | the text `t-gest-36`, Ravn's feeling, `rd_last_question` |
| 6 | answered | 0 | `won_riddle`, riddles `r-gest-12` and `r-gest-27` |
| 7 (turn) | answered | 150 d | `won_riddle`, riddle `r-gest-34` |
| 9 | dug, success | 1,100 d | `unlock_heirloom_gunnvalds_ring`; Ravn's feeling -20, heart down |
| 9 | dug, failure | -40 d | health down; joins the stone's road |
| 10 | raised | 800 d | `unlock_title_kings_guest_hleidr`, text `havamal_076`, `fair_dealing` |
| 10 | paid the carver (-60 d) | 700 d | the same, and a second grand reward, `unlock_heirloom_thuls_horn` |
| 10 | left on the tide | 600 d | `unlock_title_kings_guest_hleidr` |
| 11 (hard road) | Ravn answers | 1,400 d | `unlock_title_kings_guest_hleidr`, `unlock_heirloom_thuls_horn`, `unlock_pilot_isefjord`, Old Norse learned, runes +0.3, `won_great_riddle` |
| 11 | you answer | 1,200 d | title, horn, Old Norse learned, `won_great_riddle`; Ravn's feeling down |
| 11 | a whispered guess | 600 d | title |
| 12 | kept | 700 d | `unlock_heirloom_thuls_book`, runes +0.3, Latin +0.2 |
| 12 | to York | 1,500 d | `unlock_factor_eoforwic`, `carried_letters`; cost that lasts: `ch_riddle_trail_bishop_watches` |
| 12 | left with his kin | 600 d | `unlock_title_kings_guest_hleidr` |
| 13 | ring kept | (1,100 d had in step 9) | heirloom kept; cost that lasts: `ch_riddle_trail_feud_salhaugar` |
| 13 | ring to Saint Martin's | (as above) | `unlock_title_saint_martins_man`, `gave_alms`; the feud stays |
| 13 | ring sent back (-300 d) | (800 d net) | no grand reward; no feud (`ch_riddle_trail_amends`) |

**Why a merchant could have won these.** A guest's place under a king's hand was what made northern trade possible at all. A drinking horn with a silver lip and an arm-ring are the ordinary rich gifts of a hall. A fjord's marks are taught by a man of the place. A minster clerk keeping a known shipman's goods is the church's everyday business. A book is the rarest of them, and that is the point of it.

## Pictures

One for each step and ending; the same lines are the `art.<card id>` entries of the writing file.

| Card | Picture |
|---|---|
| s01 | The dam at Dorestad, bright afternoon: a Danish skipper in a hide cloak holds out a squared ash stick cut with runes; Ravn watches from the bow. |
| s02 | The muddy landing at Ripa under low cloud: a carter leaning on an empty cart behind two red oxen, eyeing the stick in the merchant's belt. |
| s03 | Thin fog at sea: a Norse ship with a striped sail passing close, her master on the stern with a stick raised, shouting. |
| s04 | A flat ferry mid-water on the Slie at evening, oars shipped: the ferryman cutting a small heavy bag loose from under his thwart. |
| s05 | Night at sea, one lamp: Ravn's thumb on four rune sticks laid side by side; the English boy watching. |
| s06 | Below the hall at Hleidr, autumn morning: a steward's staff across the path, a swineherd counting a sow and her litter on his fingers. |
| s07 | The hall from the low bench: two fires, the king a shape in the smoke, the skald holding out a full horn. |
| s08 | The farm by six mounds at midday: a blind old man on a bench, his hand on a long uncut stone; Ravn far back at the gate. |
| s09 | Moonless night: raw turf on one mound, two wooden spades, a jar of ale, Ravn standing apart with empty hands. |
| s10, ending 1 | Four men with ropes and rollers tilting the carved stone upright (runes, three locked horns, a four-armed wheel-cross); the old man's hand flat on it. |
| s11, ending 2 | Midwinter in the silent hall: the blind man alone between the fires, Ravn risen from the low bench, every head turned. |
| s12, ending 3 | Spring light in the farmhouse: the old man speaking with shut eyes, the boy writing on calfskin with a goose quill, Ravn marking the words. |
| s13, ending 4 | Dusk on the dam at Dorestad: one stick with one word on the thwart, a Dane walking away, Ravn with his sea-chest on his shoulder, a silver ring on the merchant's arm. |

## How long it takes

Dorestad to Ripa, round the Skaw to the Slie and on to Zealand is one summer's sailing if the house goes straight, and two if it trades on the way. The feast is three to five nights after reaching Hleidr and the meeting two nights later. Then the wait: 60 days for the stone, 150 for the writing, 200 for the heir (a winter in both), 60 for the mound's reckoning. So one sailing season and a winter at the least, and more often two to three years, running under ordinary trade and beside the other northern chains.

## Needs (nothing added to the shared tables)

- The grand-reward flags set here are new names for the game to read: `unlock_title_kings_guest_hleidr`, `unlock_title_saint_martins_man`, `unlock_heirloom_thuls_horn`, `unlock_heirloom_thuls_book`, `unlock_heirloom_gunnvalds_ring`, `unlock_pilot_isefjord`, `unlock_factor_eoforwic`.
- `flag unlock_heirloom_gunnvalds_ring = 0` is used in card 13 to take the ring away again; the game should read a flag of value 0 as not held.
- The tongue is written `old_norse` in `tongue old_norse = 0.5`, as the other chains write it; `tongues_793.json` was not opened to confirm the id (?).
- The chance in card 9 is `by crew.eyes_best`, the stat named in npc_text_encounters.md 2.4; if the crew model names it otherwise, change it there.
- Cards 5, 8, 9, 11, 12 and 13 assume Ravn is aboard, and 12 assumes Beornwine; no condition can ask for a named crewman. If either can be lost before 806, the cards want a variable for it.
- `riddles_793.json` could give Hroald as the `who` of the seven riddles for a player on this trail; it was not changed.
