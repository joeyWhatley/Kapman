# Chain: kentish_revolt_796 (Kent rises, and what comes after)

Cards: `Assets/Kapman/Game/Data/encounters/chain_kentish_revolt_796_793.json` (14 cards).
Words: `Assets/Kapman/Game/Data/writing/chain_kentish_revolt_796.json` (228 entries).
Written 6 Oct 2026. A draft for the user to overwrite. (?) marks what could not be checked against a source.
Flags are written short below: `_1` stands for `ch_kentish_revolt_796_1`, and so on.

## The story in five sentences

King Offa dies three days before Lammas 796, and within weeks Kent has a king of its own kin, Eadberht called Praen, "the priest", while the Mercian archbishop leaves Canterbury; at the toll benches of the Wantsum two men sit where one sat, each with his own tally.
The player has to choose on whose stick to pay, and then whom to serve: the wic-reeve of Lundenwic wants eyes in Kent and no iron going there, and the new king's steward at Canterbury's landing wants spearheads, or at least corn, and pays in bright pennies with a new name on them.
At Quentovic a monk has Abbot Alcuin's letter to the whole people of Kent, telling them to call their archbishop home, and it can be read aloud at the church door, handed to the king's steward, or left with a priest.
In the spring of 798 the pope's answer reaches the Mercian king (a priest cannot be a king) and the army will go after the hay: the player can pilot the king's corn boats, warn Kent, stay out of it, or, if his letter was read openly, carry the archbishop's offer to a tonsured king in a ferryman's hut at Sarre by night.
Then Coenwulf lays Kent waste as far as the Marsh and leads Eadberht away bound, and the player is paid by whoever he served, by both with two tallies on one table, or (if the king took the boat over the water) by a Kent that still has its roofs on.

## How a player meets it

- It starts by itself at **Sarre, Dofras or Fordwic** on any call from August 796 to June 798: card `s01_two_tallies` (weight 3, once). It costs nothing to begin: "sell nothing today and ask who is king" is enough.
- Step 1 schedules the next two steps at Fordwic (3 days) and Lundenwic (5 days). After that every Kent port, Lundenwic and Quentovic carries a piece of it, so a player on the Thames to Canche run meets it without looking.
- A place writer may add `card:harbour.chain_kentish_revolt_796.s01_two_tallies` to `sarre.toll` and `dofras.toll`, and `card:harbour.chain_kentish_revolt_796.s10_as_far_as_the_marsh` to the beaches.
- A player who first reaches Kent after June 798 has missed it: the wasting needs step 1.

## The real history under it

**The rising.** Offa died on 29 July 796 (Anglo-Saxon Chronicle 794 for 796; `chronicle_793.json` 796-07-29-offa-dies). His son Ecgfrith reigned 141 days; Coenwulf was king of the Mercians by December (796-12-cenwulf-king). Eadberht Praen took the kingdom of Kent; Archbishop Aethelheard, Offa's man, fled Canterbury (796-summer-eadberht-kent; Alcuin's letters of 796-797). Coins in Eadberht's name were struck at Canterbury (same chronicle entry); the moneyers' names usually given, Babba, Aethelmod and Iaenberht, are from memory and were not checked tonight (?). The rising probably took hold after Ecgfrith's death, and Christ Church was probably plundered (D. P. Kirby, *The Earliest English Kings*, p. 178, read through Wikipedia, "Coenwulf of Mercia") (?). East Anglia broke away in the same months under Eadwald, known only from his coins.

**The priest and the Franks.** "Praen" is taken to mean priest. Eadberht had lived in exile under Charlemagne's protection (Wikipedia, "Eadberht III Praen", after Kirby and Story). Charles's letter to Offa of 796 (MGH Epistolae IV no. 100) speaks of a priest Odberht who came back from Rome wishing to live abroad, whom the king has sent on to Rome with other exiles to be judged: he is often taken to be Eadberht (?). The game puts him at the guests' table at Saint-Josse by Quentovic once, which is ours.

**Alcuin's letters, 797.** Alcuin wrote to the people of Kent urging them to take their archbishop back, and lamented that scarcely anyone was left of the old stock of kings (MGH Epistolae IV no. 129, through J. Story, *Carolingian Connections*, p. 145, in the same Wikipedia page). He also wrote to Aethelheard about leaving his flock (no. 128 (?)). The cards render the sense in our own words and quote nothing. Saint-Josse was Alcuin's cell for English travellers (`places/quentovic.json`). How the letter crossed is not recorded.

**The pope's answer, 798.** Coenwulf asked Pope Leo III to move the southern archbishopric from Canterbury to London. Leo refused, and put Eadberht out of the Church as a priest who had made himself king (Leo III to Coenwulf, 798, Haddan and Stubbs, *Councils* III; `chronicle_793.json` 798-leo-canterbury; English Historical Documents no. 205 as cited by Wikipedia). That the letter likens him to Julian the Apostate is from memory (?). The cards quote none of it.

**The wasting, 798.** "Cynewulf [for Cenwulf], King of Mercia, made inroads upon the inhabitants of Kent as far as the marsh; and the Mercians seized Edbert Pryn, their king, led him bound into Mercia, and suffered men to pick out his eyes, and cut off his hands" (Anglo-Saxon Chronicle, annal 796 for 798, tr. J. Ingram, 1823, read tonight at avalon.law.yale.edu/medieval/ang08.asp). Two phrases of it are quoted in step 10 and in the news. The Marsh is Romney Marsh (?). The season is not recorded: `chronicle_793.json` fires 798-kent-ravaged at 1 July, and the chain follows it ("after the hay"). Later tradition (Roger of Wendover; William of Malmesbury on the dedication of Winchcombe, 811) has Coenwulf set him free in the end (?).

**After.** Cuthred, Coenwulf's brother, was king of Kent to 807 (in charters by 801). Aethelheard came home, went to Rome in 801, and died in 805. London burned in 798 "with great loss of life" (Historia Regum; 798-london-fire-798): ending 3 is set in the ash.

**Tolls and ships.** The toll at Sarre and Fordwic was the king's own and went by the ship; King Eadberht II freed ships of Minster in Thanet there in 763 or 764, with leave to replace a ship "broken, worn out with age or wrecked" (Sawyer 29, `dossiers/sarre.md`, `dossiers/fordwic.md`). The charters in the endings copy that form.

**Invented, and said so.** Two tallies on one bench; the reeve's asking merchants for word; the order for spearheads; the Kentish watch boat off the Foreland (coast watch in Kent is recorded only later); the reading at the church door; lightering for the king's hall at Dofras; the archbishop waiting at Lundenwic; his offer of a penitent's life; the night at Sarre; fugitives on the beaches; the king's corn boats in the Wantsum; Kentish thegns at Quentovic. People: Heahred (Heah- + -red), Osnoth (Os- + -noth), Wigheard (Wig- + -heard), Ealdhun (Eald- + -hun), Beornheah (Beorn- + -heah), and the unnamed moneyer, watchman, spearman, reeve and clerk.

Web: no searches; three page fetches (Wikipedia "Eadberht III Praen" and "Coenwulf of Mercia"; the Avalon Project's Anglo-Saxon Chronicle, eighth century).

## The people

| Who | Id | In the cards |
|---|---|---|
| Wighelm, wic-reeve of Lundenwic | `cast.wighelm` | actor `reeve` (s02, s08, end_both) |
| Aethelheard, archbishop of Canterbury 792/3 to 805 | `cast.aethelheard`, historical | actor `bishop` (s08, end_spared) |
| Eadberht Praen, king of Kent 796 to 798 | historical; not in the spine | actor `praen` (s09 only), role priest |
| Osmund, guest master of Saint-Josse | `quentovic.osmund` | actor `guest` (s04) |
| Dunwalh, the old toll man at Sarre | `sarre.tollman` | actor `old` (s01, when at Sarre) |
| Beornflaed, alewife at Dofras | `dofras.alewife` | actor `alewife` (s07) |
| Heahred, a Kentish thegn's man | invented | actor `new` (s01), `thegn` (s09) |
| Osnoth, King Eadberht's steward | invented | actor `steward` (s03, s06) |
| Wigheard, a priest of Christ Church who stayed | invented | actor `priest` (s06) |
| Ealdhun, Eadberht's man at Dofras | invented | actor `buyer` (s07) |
| Beornheah, a thegn of the Marsh | invented | actor `thegn` (s10, end_kent) |

King Offa, King Coenwulf, King Cuthred, King Charles, Abbot Alcuin and Pope Leo are spoken of and never met.

## The steps

Scheduled cards with weight 0 open on the first call at their port after their day. "Likely" is a chance of 0.6 to 0.7, "doubtful" 0.3 to 0.5, before the crew's skill.

| # | Card id (after `chain_kentish_revolt_796.`) | Where and when | The choices | Flags set |
|---|---|---|---|---|
| 1 | `harbour. ... s01_two_tallies` | Sarre, Dofras or Fordwic, Aug 796 to June 798; drawn | pay on the old tally / pay on the new king's / sell nothing and ask | `_1`; `_paid_mercia` or `_paid_kent` |
| 2 | `harbour. ... s02_reeve_asks` | Lundenwic, 5 days on, before July 798 | be the reeve's eyes (30 d) / say Kent is quiet (likely) / "I count casks, not kings" | `_2`; `_mercia`, or `_covered` |
| 3 | `harbour. ... s03_new_pennies` | Fordwic, 3 days on, before July 798; also drawn | order for six bundles of spearheads / order for twelve sacks of wheat / weigh the penny and take none | `_3`; `_kent`, or `_corn` |
| 4 | `harbour. ... s04_alcuins_letter` | Quentovic, 797 to April 798; drawn | carry the letter / carry it and stay three days to hear it / decline | `_4`; `_letter`, `_knows_letter` |
| 5 | `sea. ... s05_watch_boat` | at sea with the letter aboard, 797-798; optional | show the letter (likely) / be searched / run for the Goodwins (doubtful) | `_hailed`; `_letter_opened` |
| 6 | `harbour. ... s06_church_door` | Fordwic, 2 days after step 4 (5 if he stayed); also drawn | read it at the church door (likely quiet) / give it to the steward (40 d) / leave it with the priest | `_6`, `_letter_done`, `_letter` cleared; `_read` |
| 7 | `harbour. ... s07_whose_coin` | Dofras, 797 to June 798; drawn | freight for new pennies (160 d) / for Offa's (100 d) / for cloth (7 bolts) / refuse | `_7`; `_praen_coin` |
| 8 | `harbour. ... s08_popes_answer` | Lundenwic, January to June 798; drawn | pilot the king's corn boats (120 d) / carry the archbishop's offer (needs `_read`) / warn Kent / promise nothing | `_8`; `_mercia` and `_pilot`, or `_offer`, or `_kent` and `_warned` |
| 9 | `harbour. ... s09_night_at_sarre` | Sarre, 4 days after step 8, before August 798; the hard road only | offer passage plainly (doubtful) / speak as Kent's friend (likely; needs `_corn`, `_paid_kent` or `_kent`) / leave the letter and wait (doubtful) | `_9`; `_spared` and `changed_kent_798_spared`, or `_offer_refused` |
| 10 | `harbour. ... s10_as_far_as_the_marsh` | Sarre, Dofras or Fordwic, July 798 to 799, unless Kent was spared; drawn | take people off for nothing / take the thegn's household for 150 d / buy plunder (60 d) / pilot the corn boats (needs `_pilot`, 160 d) | `_10`; `_ferried`, `_ferried_paid`, `_loot` or `_piloted` |
| E1 | `harbour. ... end_mercia` | Sarre or Fordwic, Sept 798 to 800; not `_kent` | the king's writing (needs `_mercia`) / the archbishop's reward (needs `_offer_refused`) / his protection (needs `_letter_done`) / a burnt toft and a corn order (anyone) | `_end_mercia`, `_done` |
| E2 | `harbour. ... end_kent` | Quentovic or Dofras, Sept 798 to 800; `_kent` and not `_mercia` | sworn fellowship (needs `_ferried`) / his coaster for the debt / his wool and the Wantsum | `_end_kent`, `_done`, `_mercia_remembers` |
| E3 | `harbour. ... end_both` | Lundenwic, Sept 798 to 800; `_kent` and `_mercia` | names for silence / the seized ship for 300 d / "a merchant sells to whoever stands on the beach" | `_end_both`, `_done`; `_informer`, `_two_faced` or `_watched` |
| E4 | `harbour. ... end_spared` | Fordwic, 40 days after step 9, 798 to 800; `_spared` | protection and a free keel at Fordwic / ground and a factor at Fordwic / the freedom of Sarre, protection, and a tenth to the poor | `_end_spared`, `changed_kent_798_spared`, `_done` |

**Roads through.** Mercia's man: 1, 2 (eyes), 8 (pilot), 10 (pilot), E1. Kent's man: 1, 3 (arms), 7, 8 (warn), 10, E2. Both: 2 or 8 for the reeve and 3 or 8 for Kent, then 10 and E3. The letter's road: 1, 4, (5), 6 (read), 8 (offer), 9, then E4 if the king goes, or 10 and E1's hard option if he stands. The bystander: 1, 10, and E1's open option.

**No dead ends.** Only step 1 is needed for step 10, and step 10 for an ending, so every skipped or missed step still leads on. Every card has an option that costs only time or pride (ask; count casks; weigh the penny; decline or carry; be searched; leave it with the priest; say the tide does not serve; promise nothing; wait on the strand; take people off for nothing; and one open option at each ending). A failed chance hurts and goes on: a lie seen through costs the reeve's goodwill; a failed run costs 40 d and a broken seal, and the letter still arrives; a stone at the church door costs a little health and the letter is read all the same; a refused offer leads to the wasting and then to the best option of ending 1. Each danger warns first: the moneyer about named iron, the guest master about the watch boats, the priest about stones, the alewife about the new coin, the spearman about altar silver, the reeve about what names cost.

**A fear that does not bite.** `_praen_coin` only changes what people say. Eadberht's pennies were good silver as far as is known (?), and after 798 a moneyer would take them by weight: the reeve at Sarre says so. In ending 3 the reeve keeps them as evidence.

## The payouts

| Tier | Where | Silver | And |
|---|---|---|---|
| Step | 1 | 0 | a true price word (spearheads x1.6 at Fordwic for 90 days), or the news |
| Step | 2 | 30 d | half toll at Lundenwic for 60 days; or a true price word (Frankish wine x1.3 at Lundenwic) |
| Turn | 3 | commission: 6 bundles of spearheads at +80% (about 430 d for 240 d of iron) | or 12 sacks of wheat at +50%; shields x1.5 at Fordwic |
| Step | 4 | 10 d | deed carried_letters; latin +0.1 if he stays |
| Step | 5 | 0 (or a 40 d fine) | the watchman's feeling |
| Step | 6 | 0, or 40 d from the steward | the priest's friendship; `_read` |
| Turn | 7 | 160 d, or 100 d | or 7 bolts of woollen cloth (126 d) |
| Turn | 8 | 120 d | or deed carried_letters and the archbishop's feeling |
| Step | 9 | 0 | a king for a passenger |
| Turn | 10 | 150 d or 160 d | or church plate and 6 bolts (about 350 d) for 60 d; or deed gave_alms and the thegn's friendship |
| Finale E1 | Sarre or Fordwic | 700 d | `unlock_charter_sarre` |
| Finale E1 | | 600 d, latin +0.1 | `unlock_title_archbishops_man` |
| Finale E1, hard road that failed | | 1,100 d | `unlock_title_archbishops_man` and `unlock_charter_fordwic` |
| Finale E1, anyone | | 250 d, a commission of 20 sacks of wheat at double, wheat x2 at Sarre and Fordwic for 180 days | `unlock_plot_sarre` |
| Finale E2, honest road | Quentovic or Dofras | 600 d | `unlock_partner_beornheah` and `unlock_pilot_wantsum` |
| Finale E2 | | 250 d | `unlock_ship_coaster_minster_ship` |
| Finale E2 | | 100 d and 30 bolts of woollen cloth (540 d) | `unlock_pilot_wantsum` |
| Finale E3 | Lundenwic | 800 d | `unlock_factor_lundenwic` |
| Finale E3 | | costs 300 d | `unlock_ship_north_sea_merchant_ship` (worth several thousand) |
| Finale E3 | | 600 d and a commission of 20 sacks of wheat at +80% | `unlock_pilot_wantsum` |
| Finale E4, hard road | Fordwic | 1,200 d | `unlock_title_archbishops_man` and `unlock_charter_fordwic` |
| Finale E4, hard road | | 900 d | `unlock_plot_fordwic` and `unlock_factor_fordwic` |
| Finale E4, hard road | | 600 d, deed gave_alms | `unlock_charter_sarre` and `unlock_title_archbishops_man` |

**Why a merchant could have won these.** A king's writing freeing one named ship of toll at Sarre and Fordwic, with another to replace her, is exactly what Sawyer 29 gives; the charters that survive are for minsters, so a layman's is a stretch the game makes (?). An archbishop's protection is the same thing a lord's is: the house's boats are his guests. Burnt ground at a landing after a harrying is the reeve's to give to whoever will roof it and bring corn. A coaster "built for the minster's corn" is the class `coaster_minster_ship`; a seized Kentish trader sold cheap by a reeve with a burnt town to feed is how a `north_sea_merchant_ship` comes within a small man's reach. A landless thegn with a chest and a head full of creeks is a real partner for a second boat. The Wantsum's sands are learned by being shown them at low water.

**Costs that last.** `_mercia_remembers` (ending 2): the reeve at Lundenwic has the house's name on the same stick as Kent's. `_informer` (ending 3, names): the Kentish know who gave them. `_two_faced` (ending 3, the hull): she has a Kentish name cut in her stem. `_watched` (ending 3, plain): the reeve keeps both sticks. `_loot` (step 10): the archbishop's people ask after altar silver; a later card may find the cup.

## The endings

| Ending | Flag | How | What becomes of it | Picture (`art.chain_kentish_revolt_796.ending.*`) |
|---|---|---|---|---|
| One tally again | `_end_mercia` | served the reeve or the archbishop, or stood by | Kent is burnt to the Marsh, Eadberht is led away bound and maimed, Cuthred is king, the archbishop is home; the player has a charter, a lord or a toft | `mercia`: a strip of parchment with a king's cross on a charred beam |
| What Kent owes | `_end_kent` | armed or warned Kent and never served the reeve | the same year from the losing side: the thegn of the Marsh, landless at Quentovic, pays Kent's debt with a fellowship, a coaster or his wool and his creeks | `kent`: one hand over another on a boat's stem |
| Two tallies, side by side | `_end_both` | served the reeve and Kent | in burnt Lundenwic the reeve lays both sticks on the table and buys the player again, or sells him a ship nobody else dares buy | `both`: two tally sticks of one length tied together on a nail |
| Roofs still on | `_end_spared`, `changed_kent_798_spared` | had the letter read openly, carried the offer, and the king went | Eadberht lays the kingdom down and crosses to Saint-Josse whole; the Mercian king enters Kent without burning it; the archbishop comes home through standing corn | `spared`: a gold ring left on an altar cloth |

**What really happened** is ending 1, and ending 2 is the same year from the other shore. Ending 3 is also the true year; only the player's part differs.

**History changed, and what follows** (`changed_kent_798_spared`).

- The chronicle has no entry for Kent in 798: later cards must not show the harrying, and the era text of Sarre and Dofras for 798 ("burns its way across Kent", "beaten flat") should give way. Corn is not dear on the Kentish beaches in 798-799.
- Eadberht is not blinded or maimed. He lives as a penitent in Abbot Alcuin's cell at Saint-Josse (to 804, Alcuin's death, and after), so the later story that Coenwulf freed him at Winchcombe in 811 never arises. A player may meet a tonsured librarian at Quentovic who asks a great deal about ships.
- Cuthred is still king of Kent from 798 to 807 and Coenwulf after him: the player has changed how Kent was taken, not who holds it. The pope's refusal to move the see stands.
- Kent keeps its halls, its thegns and its minsters' goods. The quarrel of Coenwulf and Archbishop Wulfred over Minster and Reculver (816-821) is then between a king and a richer, less cowed Kent (?), and when the West Saxons come in 825 the Kentish have less to avenge and may turn to them less gladly (?). These are suggestions for whoever writes the 820s.
- The house is known at Canterbury as the boat that brought the letter: `unlock_title_archbishops_man` should open the archbishops' later cards (Aethelheard's voyage to Rome in 801, Wulfred's ships).

## Needs (for the integrator; nothing here was added to a shared table)

- Grand reward flags new to this chain: `unlock_charter_sarre`, `unlock_charter_fordwic`, `unlock_plot_sarre`, `unlock_plot_fordwic`, `unlock_factor_fordwic`, `unlock_pilot_wantsum`, `unlock_partner_beornheah`, `unlock_title_archbishops_man`, `unlock_ship_coaster_minster_ship`. (`unlock_factor_lundenwic` and `unlock_ship_north_sea_merchant_ship` are already set by other chains.)
- The history flag `changed_kent_798_spared`: `chronicle_793.json` 798-kent-ravaged and any card that shows the harrying should not fire when it is set.
- There is no `places/fordwic.json` yet: the Fordwic cards name no site or person from one. When it is written, Osnoth the steward (796 to 798), Wigheard the priest and the moneyer could be its people, and `fordwic.aethelheard` (the spine's place id for the archbishop) its bishop.
- The story `dofras.archbishops_chest` (places/dofras.json) runs in the same years with its own flags; this chain does not read them. A card could let a player who carried the chest (`dofras_chest_hidden` or its other endings) be greeted for it at step 8.
- A text for the chest: the Chronicle's annal for 796 (Ingram 1823), to be given at step 10 with `give_text`. `chronicle_793` in `texts_793.json` may already hold it; not checked.
- The commission verb takes its port from the call, so the spearheads and the wheat are delivered where they were ordered (Fordwic; Sarre or Fordwic; Lundenwic).
- No deed fits "armed a rebel" or "bought plunder": the chain uses flags (`_kent`, `_loot`) instead. If a deed is wanted, the spine may add one.

## Pictures

One for each card (`art.<card id>` in the writing file) and one for each ending (above).

| Card | Picture |
|---|---|
| s01 | A toll bench on a chalk strand, late summer: an old toll man and a young man with a sword, each with a tally, looking opposite ways |
| s02 | The Strand at low water: the reeve in a green cloak, two fingers raised, questioning a skipper in the shallows |
| s03 | A river landing below a walled city: one bright penny on a thwart beside a balance; a steward; spears against a shed |
| s04 | The Canche at ebb, wind: a monk holds out a sealed letter; a stone cell and a guest hall behind |
| s05 | Under a white headland: an eight-oared boat across the bow, a spear upright, the skipper's hand at his shirt |
| s06 | A landing at evening: a priest and a steward each reaching for the same letter |
| s07 | The shingle at Dofras: sacks and barrels, a man with two handfuls of coin, bright and dull; an alewife in her doorway |
| s08 | A hall by firelight: the reeve with a letter and a hanging lead seal; a tonsured man by the wall with a book chest |
| s09 | A hut at night, one rushlight: a man with a ring and a half-grown tonsure reading; a thegn behind, hand on hilt |
| s10 | A Kentish beach at noon, three smokes on the down: a crowd with bundles, a thegn holding a child, a spearman with a sack |
| end_mercia | The bench in autumn: one man, one tally, black rafters; corn sacks coming up from a boat |
| end_kent | Quentovic in drizzle: a thegn by a sea chest with a boy of twelve; a weeded coaster on the mud |
| end_both | A half-burnt hall: two tally sticks side by side, bright pennies between, the reeve's hands flat on the table |
| end_spared | The landing in a golden harvest evening: whole thatch, corn ricks, the archbishop walking down to a Frisian boat |

## How long it takes

The chain is held to its true dates, so it runs two years at the least. Step 1 can come from August 796; steps 2 and 3 follow within a week; the letter (4 to 6) waits for 797; the freight at Dofras for 797; the pope's answer for the first half of 798, with the night at Sarre four days after it; the wasting for July 798; and the endings for September 798 or later (ending 4 comes 40 days after the night at Sarre, so about midsummer 798). A player who starts at Lammas 796 and trades the Thames, the Wantsum and the Canche as usual finishes in the autumn of 798, a little over two years on, and for most of that time the chain is a handful of flags, two commissions and a choice of whose coin to take. The endings stay open to the end of 800.
