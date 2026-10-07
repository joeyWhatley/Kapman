# Chain: black_stones (the black stones and the cloaks)

Cards: `Assets/Kapman/Game/Data/encounters/chain_black_stones_793.json` (14 cards).
Words: `Assets/Kapman/Game/Data/writing/chain_black_stones.json` (210 entries).
Written 6 Oct 2026. A draft for the user to overwrite. (?) marks what could not be checked against a source.
The spine's thread `black_stones` (Design/content/spine.md no. 1): this chain tells it in full and sets its three flags.

## The story in five sentences

Since about 789 King Charles has shut his shores to English merchants and King Offa his to Franks, over a marriage that was never made, and at Dorestad a quern cutter sits on forty pairs of black stones cut for the English that nobody fetches.
The player takes stones to Lundenwic, where the wic-reeve asks twice whose man a Frisian is, lets him land for a gift or for three tides' patience, or sees him go round by Hamwic, the half-shut back door where the rival Thiadbod's man sends English cloaks over as pilgrims' bundles.
The reeve then wants a quiet ship: a sealed letter for Abbot Gervold, who takes the Frankish king's tolls at Quentovic and is Offa's friend, and with it a hazel rod cut to the length of the stones Offa wants; the abbot's answer comes back from Saint-Wandrille with a cord knotted at the length a cloak used to be.
The player can carry both honestly, carry them and eight pigs of English lead for the abbot's church roofs through the ban, sell the answer to Thiadbod, or refuse the letters and live in the gap.
From that come four ends: the peace of 796 as it happened, a peace a year or two early that the player's lead bought, a quarrel that outlives Offa because the answer never arrived, or the runner's reckoning when the letter's line about traders among pilgrims closes the gap.

## How a player meets it

- It starts by itself at Dorestad, the home port, any time before 796: card `s01_unsold_stones` is drawn on a call there (weight 1.5, once). It costs nothing to begin: "ask what the kings fell out over" is enough.
- The long-years card of the ban (`long_793.json`) and the spine's hooks at Lundenwic, Quentovic, Hamwic, Colonia and Rotomagus point the same way; a place writer may add `card:harbour.chain_black_stones.s01_unsold_stones` to a Dorestad site (the quern cutters' workshops).
- After step 1 the story waits at Lundenwic. From step 2 on, any call at Hamwic may bring the way round (step 3h), and from step 4 any call at Colonia may bring the stone dealer (step 8).

## The real history under it

**The quarrel.** Gesta abbatum Fontanellensium c. 12 (written at Saint-Wandrille 823-867; ed. Loewenfeld 1886, pp. 46-47; the spine's reading): Gervold, abbot of Saint-Wandrille from 787, was "for many years procurator over the kingdom's trade, collecting tributes and tolls in divers ports and cities, chiefly at Quentawich" (our rendering). He was Offa's friend; Offa's letters to him still lay at the abbey; he went to Offa many times as the king's envoy, the last time over the marriage. The younger Charles asked for Offa's daughter; Offa would not agree unless Bertha, Charles's daughter, were given to his son Ecgfrith; the king, "somewhat moved" (aliquantulum commotus), ordered that no one from Britain and the English people should touch the shore of Gaul to trade. The Gesta says Gervold's plea stopped the order.

**The ban.** Alcuin to Colcu, 790 (MGH Epistolae IV no. 7; Jaffé, Bibliotheca VI no. 14): the quarrel has gone so far that on both sides sailing is forbidden to merchants, and he may be sent to make peace. So the game has the ban real but leaky from 789 to 796 (spine). How long it held, how far it reached, and whether it bound Frisians are not recorded (?). No reeve is recorded turning Franks away at Lundenwic (`dossiers/lundenwic.md`); the wic-reeve himself is in the laws of Hlothhere and Eadric c. 16.

**The letter of 796.** Charles to Offa (MGH Epistolae IV no. 100; Haddan and Stubbs III p. 496; in part in William of Malmesbury, Gesta regum, tr. J. A. Giles 1847). Pilgrims are to "go peaceably without any molestation"; those among them "not seeking the cause of religion, but that of gain" are to "pay the customary tolls in proper places" (Giles; quoted in the ending cards). The merchants of each realm have the other king's protection and may appeal to him. Offa has made known his wish about the length of the black stones: let him send a man to say what kind, and Charles will have them given and help with their carriage; and Charles asks in turn that the cloaks be made as they used to come (our rendering; this passage is not in Giles). He sends "a belt, an Hungarian sword, and two silk cloaks" (Giles): the sword is Avar loot from the Ring, taken in the winter of 795-796 (`chronicle_793.json` 796-winter-avar-ring-plundered). Chronicle entry: 796-spring-offa-letter.

**The black stones.** Usually read as lava quernstones from Mayen in the Eifel (?), by others as dark stone for building or for fonts (?): the dealer at Colonia says both. Mayen roughouts come down the Rhine and are finished at Dorestad (`dossiers/dorestad.md`, `dossiers/colonia.md`); Eifel lava querns lie in nearly every pit at Hamwic and at Lundenwic (`dossiers/hamwic.md`, `dossiers/lundenwic.md`). `goods_793.json`: quernstones, 12 d the pair, 90 kg.

**The cloaks.** Besides the letter: Notker, Gesta Karoli I.34 (written 883-887: LATER): the Frisians sold short cloaks at the price of long ones and the king forbade it. The chain `house_rival` quotes Notker; this chain does not, so the same words are not said twice.

**The people and dates.** Offa died 29 July 796 (Anglo-Saxon Chronicle 794 for 796), three days before Lammas; his son Ecgfrith reigned 141 days; Coenwulf followed and had Kent's rising under Eadberht Praen on his hands, 796-798. Offa had King Aethelberht of the East Angles beheaded in 794 (ASC 792 for 794). Beorhtric of Wessex married Offa's daughter Eadburh (ASC 787 for 789): the factor's "size of the gap" at Hamwic. Gervold roofed Saint Peter's and Saint Michael's at his abbey with lead (Gesta c. 12); English lead is the likely source (?). Toll freedom for a ship at Lundenwic by a king's writing is modelled on King Aethelbald's remissions of ship toll there for Rochester, Worcester and Minster in Thanet in the 730s and 740s (from memory; charters not opened tonight (?)).

**Invented, and said so.** The rod and the cord (that the two lengths travelled as a hazel stick and a knotted string); that a reeve and an abbot wrote to each other through the ban by a Frisian boat; the eight pigs of lead in the king's store; the king's boat in the Channel in 793-796 (coast guards are ordered only from 800, Annales regni Francorum); Hamwic as the half-open door (spine). People: Wighelm, Thiadbod and Sigewine are the spine's invented cast; ours are Folkmar the quern cutter (Folc- + -mar), Hildulf, Thiadbod's man at Hamwic (Hild- + -ulf), Hunberht the English shipmaster (Hun- + -berht), Gerwin the stone dealer (Ger- + -win), and the unnamed clerk, weaver, pilgrim, toll man and king's officer.

No web search was used: the history is the spine's and the dossiers', with the sources they name.

## The people

| Who | Id | In the cards |
|---|---|---|
| Wighelm, wic-reeve of Lundenwic | `cast.wighelm` (to 810) | actor `reeve` |
| Gervold, abbot of Saint-Wandrille, the king's toll-gatherer | `cast.gervold` (787 to 806), historical | actor `abbot` at Rotomagus, Quentovic and (as envoy) Lundenwic |
| Thiadbod, the rival Frisian skipper | `cast.thiadbod` (to 825) | actor `rival` |
| Sigewine, the Northumbrian monk who carries letters | `cast.sigewine` (to 810) | actor `monk` at Rotomagus |
| Folkmar, quern cutter of Dorestad | invented | actor `cutter` |
| Hildulf, Thiadbod's man at Hamwic | invented | actor `factor` |
| Hunberht, master of the arrested English ship | invented | actor `skipper` |
| Gerwin, stone dealer at Colonia | invented | actor `dealer` |
| The abbot's clerk, the toll man, a cloak-maker, a false pilgrim, the officer of the twelve-oared boat | invented, unnamed | roles clerk, tollman, weaver, pilgrim, kings_man |

King Charles, King Offa and Alcuin are spoken of and never met.

## The steps

Scheduled cards (weight 0) open on the first call at their port after their day. "Likely" is a chance of 0.6 to 0.7, "doubtful" 0.45 to 0.5, before the crew's skill.

| # | Card id | Where and when | The choices | Flags set |
|---|---|---|---|---|
| 1 | `harbour.chain_black_stones.s01_unsold_stones` | Dorestad, before 796; drawn | four pairs on trust / buy four pairs at 6 d (24 d) / leave the stones and ask about the quarrel | `ch_black_stones_1`; `_trust` |
| 2 | `harbour.chain_black_stones.s02_reeve` | Lundenwic, the first call after | a gift and a story (20 d) / lie off three tides / put about for Hamwic | `ch_black_stones_2`, `bs_ban_heard` (spine); `_round` |
| 3h | `harbour.chain_black_stones.s03_hamwic` | Hamwic, before 797; drawn for anyone past step 2, scheduled by the road round and by a refusal | carry six bundles over for a third (likely) / sell openly and leave the bundles / buy the pilgrim ale and listen | `ch_black_stones_3h`; `_runner`, `_watched` (caught), `_knows_bundles` |
| 4 | `harbour.chain_black_stones.s04_letter` | Lundenwic, 25 days after step 2 (40 by the road round), before Easter 796 | take letter and rod / take them for a price / refuse and go Thiadbod's way | `ch_black_stones_4`; `bs_letter` (spine) and `_rod`, or `_refused` with `_runner` |
| 5 | `sea.chain_black_stones.s05_kings_boat` | at sea with the letter aboard; drawn, optional | show the seal (likely) / be searched and say nothing / run for the shoals (doubtful) | `ch_black_stones_hailed`; `_seal_shown`, `_letter_taken` |
| 6 | `harbour.chain_black_stones.s06_quentovic` | Quentovic, the first call after step 4 | hand it over sealed / hand it over and ask the English ship's release (doubtful) / let Thiadbod's man read it first (90 d; doubtful that it goes unseen) | `ch_black_stones_6`, `bs_letter` cleared; `_ship_freed`, `_read`, `_seal_seen` |
| 7 | `harbour.chain_black_stones.s07_wandrille` | Rotomagus, 20 days on | take answer and cord / take them and stay a week to learn letters / take them and ask a cask for the freight | `ch_black_stones_7`, `_cord`, `bs_letter` |
| 8 | `harbour.chain_black_stones.s08_colonia` | Colonia, before 797; drawn for anyone who carried the rod, optional | sell the length (150 d) / pay 60 d to have twelve pairs cut to it and held / show nothing | `ch_black_stones_8`; `_length_sold` or `_stones_cut` |
| 9 | `harbour.chain_black_stones.s09_answer` | Lundenwic, 30 days after step 7 | give the reeve the answer / give it and take eight pigs of lead for the abbot / let it go astray to Thiadbod (240 d) | `ch_black_stones_9`, `bs_letter` cleared; `_lead` or `_sold` |
| 10 | `harbour.chain_black_stones.s10_lead` | Quentovic, 12 days on; the hard road only | land all eight pigs on the abbot's word / sell it as your own / tell him what became of it | `ch_black_stones_10`; `_lead_done` and the timer `_envoys` (150 days) |
| E1 | `harbour.chain_black_stones.end_early` | Lundenwic, from the Easter after the abbot's embassy (April 794 to March 796); drawn | the king's writing / a man in the toll house / ground on the Strand / your own stones from Colonia | `ch_black_stones_end_early`, `changed_offa_peace_early`, `bs_peace` |
| E2 | `harbour.chain_black_stones.end_peace` | Lundenwic, from May 796 (to 799); drawn | first stones and the writing (needs step 9) / the writing and a man on the Canche (needs the lead landed) / the reeve's order and his pilot (anyone) / your own stones from Colonia | `ch_black_stones_end_peace`, `bs_peace` |
| E3 | `harbour.chain_black_stones.end_shut` | Hamwic, from Lammas 796 (to 799), if the answer was sold; drawn | a share and a man at Hamwic / all in silver and the creeks / own up to the abbot, late | `ch_black_stones_end_shut`, `changed_offa_letter_lost`; `_abbot_remembers`, or `_owned_up` with `bs_peace` |
| E4 | `harbour.chain_black_stones.end_runner` | Hamwic, from May 796 (to 799), if the letter was refused; drawn | a share and a house at Hamwic / silver and the creeks / pay the slipped tolls and start clean | `ch_black_stones_end_runner`, `bs_peace`; `_watched`, or `_paid_tolls` |

**Roads through.** Carrier: 1, 2, (3h), 4, (5), 6, 7, (8), 9, then E2; with the lead 10 and E1 (or E2's hard option if the early peace was missed). Seller: the same to 9, then E3. Runner: 1, 2, 3h, 4 (refuse), E4.

**No dead ends.** Every card has an option that costs only time or pride (ask, lie off three tides, sell openly or buy ale, carry, be searched, hand it over, carry, show nothing, deliver, tell the truth; and at every ending one option with no check). A failed chance hurts and goes on: a cut bundle is a fine and a name on a list; a doubted seal is a lost day; a failed run loses the cargo and the letter still reaches the abbot in the king's boat; a seen seal costs the clerk's goodwill and one sharp line from the abbot. Each danger warns first: the factor says what happens if a bundle is opened; the twelve-oared boat "has more oars than you have men". A player who dawdles past Easter 796 without taking the letter still has E2's open option.

## The payouts

| Tier | Where | Silver | And |
|---|---|---|---|
| Step | 1 | 0 (or four pairs of stones on trust, worth 48 d) | the true price word: stones dear at Lundenwic; the news of the ban |
| Step | 2 | 0 | half toll at Lundenwic for 30 days; stones x1.6 there for 30 days; the reeve's feeling |
| Step | 3h | 110 d (or a 30 d fine) | English understood better (tongue old_english 0.75); deed fair_dealing |
| Step | 4 | 12 d, or 40 d for a price | deed carried_letters; the king's length |
| Step | 5 | 0 | deed fled_kings_men, or the cargo seized |
| Step | 6 | 20 d, or 90 d for the seal | half toll at Quentovic for 30 days; the English shipmaster's friendship |
| Step | 7 | 25 d, or a cask of Frankish wine (50 d) | latin +0.1 and Aldhelm's riddle of the scales (`riddle_aldhelm_23_trutina`) |
| Turn | 8 | 150 d for the length | or twelve pairs cut to it for 60 d and Tatwine's whetstone riddle (`riddle_tatwine_39_coticulo`); or a true price word for Dorestad |
| Turn | 9 | 30 d; or 240 d for the answer | or eight pigs of lead (120 d) |
| Turn | 10 | 160 d and no toll | the abbot's feeling +15; lead x1.5 at Quentovic if sold instead |
| Finale E1 (hard road) | Lundenwic | 900 to 1,500 d | `unlock_title_two_kings_merchant` and one of `unlock_charter_lundenwic`, `unlock_factor_quentovic`, `unlock_plot_lundenwic`; a commission of six pairs at double price (or eight pigs of lead at +80%) |
| Finale E2 | Lundenwic | 600 to 1,500 d | `unlock_charter_lundenwic` (carrier), with `unlock_factor_quentovic` (lead landed), or `unlock_pilot_thames` (anyone); a commission of six pairs |
| Finale E3 | Hamwic | 600 to 1,300 d | `unlock_factor_hamwic` or `unlock_pilot_solent`; stones x1.8 at Lundenwic for a year |
| Finale E4 | Hamwic | 600 to 1,000 d | `unlock_factor_hamwic` or `unlock_pilot_solent`; half toll at Hamwic for 120 days if the tolls are paid |

**Why a merchant could have won these.** A king's writing freeing a ship of toll at Lundenwic is what English kings gave (to churches) in the 730s and 740s (?), and the letter of 796 puts merchants under each king's protection with appeal to the king: that is `unlock_title_two_kings_merchant`. Gervold ran the toll houses of the Channel ports and could seat a man in one. Ground on the Strand is the reeve's to pace out for the king. A pilot's knowledge of the Thames or of the Solent creeks is learned by being shown, or by three summers of landing at night.

**Costs that last.** `ch_black_stones_abbot_remembers` (E3, unless the player owns up): Gervold keeps the Frankish Channel tolls until 806. `ch_black_stones_watched` (a cut bundle at step 3h, or E4 without paying): the toll men at Quentovic have the boat on a list. `ch_black_stones_runner` stays set for any later card to read.

## The endings

| Ending | Flag | How | What becomes of it | Picture (`art.chain_black_stones.ending.*`) |
|---|---|---|---|---|
| The early peace | `ch_black_stones_end_early`, `changed_offa_peace_early` | carried both letters and landed the lead | Gervold crosses once more; the kings are at peace by the Easter after; the player lands the first stones cut to the rod | a Frankish and an English ship side by side on the Strand, a rod and a cord crossed on a pair of querns |
| The peace of 796 | `ch_black_stones_end_peace` | carried the answer (or only reached the Strand) | the letter is read on the beach; Offa dies three days before Lammas; the stones are still wanted | an open letter, a belt, a sword and two silk cloaks; a boat unloading stones beyond the door |
| The quarrel outlives the king | `ch_black_stones_end_shut`, `changed_offa_letter_lost` | sold the abbot's answer to Thiadbod | no letter is written; Offa dies unreconciled; the Strand stays shut and two Frisians do not mind | a knotted cord sinking in grey river water |
| A line for us | `ch_black_stones_end_runner` | refused the letter and ran bundles | the letter's line about traders among pilgrims closes the gap; someone else landed the first stones | a pilgrim's bundle cut open on a beach, a toll man's knife beside it |

**What really happened** is the peace of 796 (E2), and E4 is the same year seen from the other side.

**History changed, and what follows.**

- `changed_offa_peace_early`: the ports open in 794 or 795. Offa lives to see one or two summers of open trade; English cloth and wool reach Quentovic and Dorestad that much sooner and quernstones go over at the old rate (the ban cards stop: `bs_peace`). The letter of 796 is then not the peace itself: no Avar sword is in the gift (the Ring falls only in the winter of 795-796), and Gervold, not Alcuin, is remembered as the man who mended it. Later cards may make the reeve Wighelm safer after Offa's death, and should not read out the 796 letter as news.
- `changed_offa_letter_lost`: no letter about black stones is written. Offa dies on 29 July 796 with the quarrel open, Ecgfrith after 141 days, and Coenwulf has Kent to put down first. The game should hold the ban at Mercian ports until about 798 (?), keep stones dear at Lundenwic and English cloth dear on the Rhine (the ending sets a year of it), and leave the rule "true pilgrims free, traders pay" unwritten until some later king asks for it (Cnut at Rome in 1027 would be asking for the first time). If the player owns up, `bs_peace` is set and the peace is made late, in 797, with Coenwulf.

## Needs (for the integrator; nothing here was added to a shared table)

- `long_793.json`, card `long.offa_letter.796`: add `and not flag.changed_offa_letter_lost and not flag.changed_offa_peace_early` to its `when`, or it will read out the letter of 796 in a world where it was not written. (The early ending also sets `bs_peace`, which already stops it.)
- A text for the chest: Charles's letter to Offa of 796 in a public-domain translation (Giles 1847 has it in part), as `texts_793.json` id `charles_to_offa_796`, to be given at E2 with `give_text`.
- Grand reward flags new to this chain: `unlock_charter_lundenwic`, `unlock_plot_lundenwic`, `unlock_factor_quentovic`, `unlock_factor_hamwic`, `unlock_pilot_thames`, `unlock_pilot_solent`, `unlock_title_two_kings_merchant`.
- The twelve pairs ordered at Colonia are abstracted: the ending pays for eight sold and puts four aboard. If the game gets stored goods at a port, they could lie at Colonia for real.

## Pictures

One for each card (`art.<card id>` in the writing file) and one for each ending (above).

| Card | Picture |
|---|---|
| s01 | Dorestad behind the dams, grey noon: the quern cutter on his stack of black stones, two skippers arguing, masts over the roofs |
| s02 | The Strand at low water: the reeve with his rod raised and two spearmen, a small boat nosing the shingle |
| s03h | The hythe at Hamwic: a factor roping square bundles onto pilgrims' backs |
| s04 | Night, a horn lantern: the reeve with a letter and a hazel rod; the skipper notching his gunwale |
| s05 | The Channel: a twelve-oared boat across the bow, the skipper's hand at his shirt |
| s06 | The Canche at ebb: the clerk's hand held out; an English ship with a sealed hatch on the mud |
| s07 | The quay at Rotomagus: the abbot hands down a letter and a knotted cord; a cold English monk beside him |
| s08 | The Rhine bank at Colonia: quern blanks stacked like loaves; a dealer measuring between two notches |
| s09 | The Strand at evening: Thiadbod at the player's ear; a weaver holding a short cloak against the cord |
| s10 | Quentovic in rain: eight pigs of lead under the bottom boards; the abbot pointing at a roofless gable |
| end_early | The Strand in spring: a Frankish ship beached and nobody minding; abbot and reeve side by side |
| end_peace | The Strand, summer: a priest reading a letter to a crowd; a woman turning a new quern |
| end_shut | A net shed at Hamwic by tallow light: silver and a tally on a cask head |
| end_runner | The hythe at Hamwic: a tally stick split over a knee; a pilgrim with no bundle |

## How long it takes

The waits are 2, 25 (or 40), 1, 20, 30 (or 37) and 12 days between the scheduled steps, about a hundred days with the sailing, so the errands fit in one season: a player who starts at midsummer 793 can have the answer on the Strand before the winter. The endings then wait for their real time. The early peace comes at the first Easter at least 150 days after the lead is landed (April 794 at the soonest); the peace of 796, the runner's reckoning and the sold answer all wait for 796 (May, May and Lammas). So the chain runs from one season to three years, and for most of that time it is two or three flags and a price at Lundenwic while the player trades as usual.
