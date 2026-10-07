# Chain: pilgrim_road (the English road to Rome)

Cards: `Assets/Kapman/Game/Data/encounters/chain_pilgrim_road_793.json` (14 cards).
Words: `Assets/Kapman/Game/Data/writing/chain_pilgrim_road.json` (215 entries).
Written 6 Oct 2026. A draft for the user to overwrite. (?) marks what could not be checked against a source.
No web search or fetch was used: the history is from the spine, the dossiers (quentovic, dofras, lundenwic), `lore/law_tolls.md`, the places files of Quentovic, Sarre and Dofras, the chronicle table, and memory; everything from memory carries a (?).

## The story in five sentences

On a Kentish strand a widow, her mass-priest and a girl ask for a keel to Cwantawic, on their way to the thresholds of the Apostles, and the priest keeps his back to the road.
At the Frankish toll house pilgrims pass free but a priest must show his bishop's letter, and this one has none, because he is of the old Kentish kings' kin and King Offa would like him nearer to hand; the cell of Saint Josse feeds them, the widow needs a mule, and the priest asks the skipper to carry half a cut penny back to a steward at Dofras, where the king's reeve is paying for answers.
Two hundred days later a monk brings word from Rome: the girl is buried in the English quarter, the widow has the fever and everything she went for.
She comes back to the Canche with a cloth from the Apostle's tomb, pepper, incense, a dead girl's shoes and a sealed leaf the priest left for the first of Kent to pass, and at Sarre she asks the skipper what to do with it.
Carried on, it helps raise Kent when Offa dies in 796, as happened; given to the king's man, there is no rising at all; locked in the abbess's chest, it is nobody's business, and the widow's own ending is the one that pays.

## How a player meets it

- It starts by itself: card `s01_waiting` is drawn on a call at Dofras, Sarre, Fordwic or Lundenwic between April and September, before 795 (weight 1.5, once). All three choices begin the story, and the cheapest costs nothing.
- The spine already points here: "Dofras (before 800): pilgrims waiting for a ship to Quentovic. Also at Sarre and Fordwic." A place writer may add `card:harbour.chain_pilgrim_road.s01_waiting` to `dofras.strand`-type sites, `sarre.strand` and `quentovic.saint_josse`.
- It is not the false pilgrims with a bundle of cloaks (`sarre.pilgrims_bundle`, `dofras.pilgrims_bundle`, and step 3h of black_stones): these three are true pilgrims, and what one of them hides is himself.

## The real history under it

**The road.** Quentovic was the usual landing of the English bound for Francia and Rome (Bede, Historia ecclesiastica IV.1: Theodore fell sick there in 668-669; Vita Bonifatii c. 5: Boniface landed at "the mouth of the river called Cuent"; `dossiers/quentovic.md`). About eleven weeks' walking to Rome: the eighty stages of Archbishop Sigeric's return in 990 (British Library, Cotton Tiberius B.v) are LATER and end at Sombre near Wissant, not on the Canche; the pass is the Mount of Jove (the Great St Bernard), with Saint-Maurice below it. Boniface wrote to Archbishop Cuthbert in 747 (Tangl no. 78) that English women should be kept from the Rome road, because many perished on it and few came home as they left (our rendering): women did go, and some died.

**Tolls.** Pilgrims to Rome pay no toll at bridges, passes or ferries: Pippin's capitulary of about 754-755 (MGH Capitularia I no. 13 c. 4; `lore/law_tolls.md`). From 796 traders found among pilgrims pay (Charles to Offa, MGH Epistolae IV no. 100; chronicle `796-spring-offa-letter`): the clerk's line for `year >= 796`. A cleric away from his church needs his bishop's letter and is not to be received without it: Admonitio generalis, 789, c. 3 (?) (from memory; the rule is older canon law). The procurator's toll house at Quentovic and Abbot Gervold, Offa's friend: Gesta abbatum Fontanellensium c. 12.

**Hosts.** King Charles "once entrusted the cell of Saint Josse" to Alcuin "to give alms to pilgrims", about 792 (Lupus of Ferrières, letter 11); it cared for "those from over the sea and other poor" (letter 71); Alcuin's priest Martin lay sick there in the autumn of 794 (Alcuin, letter 25: the guest-master's line for that autumn). The saint's hair cut by his keepers is later legend (?).

**Rome.** By Saint Peter's lay the "schools" of the foreigners. The Liber Pontificalis has those of the Franks, Frisians, Saxons (the English) and Lombards come out to meet Pope Leo in 799 (Leo III c. 19 (?)): so there were Frisians at Rome with a house of their own, which the monk's joke about herring prices rests on. "The English school at Rome" burned in 816/817 (Anglo-Saxon Chronicle 816); King Burgred was buried in 874 "in St Mary's church in the school of the English nation" (ASC 874). The fever: Wigheard, archbishop-elect, and nearly all his companions died of the pestilence at Rome in 667 (Bede III.29). Pope Hadrian died on 25 December 795 (chronicle `795-12-25-hadrian-dies`).

**What a pilgrim brings back.** A cloth laid on the Apostle's tomb and carried home as a relic: Gregory the Great to the empress Constantina, 594 (Registrum IV.30); Gregory of Tours has the cloth weighed before and after and found heavier (Glory of the Martyrs 27 (?)). Roman chant brought home: Bede IV.18 (John the archcantor). Pepper, incense and silk bought in Italy: `economy/pots_glass_luxuries.md`; the English buying at Pavia is the Honorantiae civitatis Papiae, LATER, about 1000 (?). And sickness, and the dead left there.

**The priest.** In his letter of 796 Charles tells Offa that the priest Odberht and other exiles, who "in fear of death" had taken refuge under his protection, have been sent to Rome to be judged there, and are not sent back (MGH Epistolae IV no. 100; our rendering). Odberht is usually taken to be Eadberht Praen (?). The Anglo-Saxon Chronicle (794 for 796): Offa died, and "Eadberht, whose other name was Praen, obtained the kingdom of Kent" (our rendering); Archbishop Æthelheard, Offa's man, left Canterbury (chronicle `796-summer-eadberht-kent`). Pennies in Eadberht's name exist (?), as do pennies of Ecgberht II of Kent, about 765-780 (?). Pope Leo called him an apostate priest (Leo III to Coenwulf, 798, Haddan and Stubbs III p. 523 (?)). In 798 "Coenwulf, king of the Mercians, laid waste Kent as far as the Marsh, and took Praen their king and led him bound into Mercia" (ASC 796 for 798, our rendering); manuscript F adds that his eyes were put out and his hands cut off. Cuthred, Coenwulf's brother, was king of Kent 798-807. William of Malmesbury has Coenwulf free him at the hallowing of Winchcombe in 811 (Gesta regum I.95: LATER (?)).

**The minster.** Minster in Thanet's ships went free of toll at Sarre and Fordwic by King Eadberht II's charter of about 763 (Sawyer 29), which let a worn-out ship be replaced under the same freedom, and at London by Æthelbald's (Sawyer 86, 91): `places/sarre.json`, whose unlock `sarre.abbess_freedom` is the same `unlock_charter_sarre` this chain can give. The abbess of 793 is not known (Sarre dossier), so none is named: the nun Cwenburh speaks for the house.

**Invented, and said so.** That Eadberht crossed the Channel as a widow's mass-priest, in 793-794, in a Frisian boat; that he was of the old kings' kin (his descent is not recorded); the half penny, the steward Heahstan, the sealed leaf and its six names; that Offa's reeves asked skippers whom they carried (what is recorded is only that Offa asked Charles for the exiles); the mule and its price. People: Wilburh the widow (Wil- + -burh) and Heahstan (Heah- + -stan) are ours; Osmund, Hildric, Hereberht, Cwenburh and the toll man of Sarre are the places files' invented people; Sigewine, Beornwine, Ravn and Wighelm are the spine's.

## The people

| Who | Id | In the cards |
|---|---|---|
| Wilburh, a Kentish thegn's widow | invented | actor `pilgrim` |
| Eadberht, later called Praen, priest, king of Kent 796-798 | historical (?) for the identification with Odberht | actor `priest`, then `king` |
| Hildric, the abbot's clerk at Quentovic | `quentovic.hildric` | actor `clerk` |
| Osmund, guest-master of Saint Josse | `quentovic.osmund` | actor `guest_master` |
| Heahstan, a thegn's steward | invented | actor `steward` |
| Hereberht, the king's reeve at Dofras; Wighelm at Lundenwic | `dofras.reeve`; `cast.wighelm` | actor `reeve` |
| Sigewine, the monk who carries letters | `cast.sigewine` | actor `monk` |
| Beornwine and Ravn, of the crew | `cast.crew.beornwine`, `cast.crew.ravn` | actors `lookout`, `crewman` |
| Cwenburh, the nun who keeps the minster's ships | `sarre.cwenburh` | actor `nun` |
| A Mercian of the king's following; the toll man | invented, unnamed; `sarre.tollman` at Sarre | actors `kings_man`, `tollman` |

King Offa, King Charles, King Coenwulf, Abbot Gervold, Alcuin and his priest Martin are spoken of and never met.

## The steps

Scheduled cards (weight 0) open on the first call at their port after their day. Steps 4 and 5 follow step 3 at once by the option's `then`. Sea and crew cards use `rate` (per game hour), which is what the scheduler reads for those stages. "Likely" is a chance of 0.6 to 0.7, "doubtful" 0.5, before the crew's skill.

| # | Card id | Where and when | The choices | Flags set (all `ch_pilgrim_road_...`) |
|---|---|---|---|---|
| 1 | `harbour.chain_pilgrim_road.s01_waiting` | Dofras, Sarre, Fordwic or Lundenwic; April to September, before 795; drawn | a fare of 30 d / for a prayer / hear their road and wish them a keel | `_1`; `_fare`, `_alms` or `_left`; `_aboard` (8 days) |
| 2 | `sea.chain_pilgrim_road.s02_mid_channel` | under way with them aboard; optional | ask him who he is / watch (likely) / ask the widow about the road | `_2`; `_knows` |
| 3 | `harbour.chain_pilgrim_road.s03_no_letter` | Quentovic, 1 day on (6 if another skipper took them) | swear for him (likely) / 15 d on the clerk's table / let the name be written | `_3`; `_vouched` or `_named` |
| 4 | `harbour.chain_pilgrim_road.s04_saint_josse` | Quentovic, at once | lend 60 d for a mule / 12 d to the pilgrims' pot / wish her a dry road | `_4`; `_lent`; the timer `_road` (200 days) |
| 5 | `harbour.chain_pilgrim_road.s05_half_penny` | Quentovic, at once | take the half penny / take it and name a price / give it back | `_5`; `_token`, `_promised` |
| 6 | `harbour.chain_pilgrim_road.s06_two_men` | Dofras, 3 days on; only with the half penny | pass it in a handshake (likely) / tell the reeve for 120 d / drop it in deep water | `_6`, `_token` cleared; `_token_given` (and `_seen`), `_sold` or `_dropped` |
| 7 | `harbour.chain_pilgrim_road.s07_word_from_rome` | any Frankish port, once the 200 days have run; drawn | send 40 d south / send word only / promise the boat and ask after the priest | `_7`; `_sent_silver` |
| 8 | `harbour.chain_pilgrim_road.s08_what_she_brought` | Quentovic, 120 days on | take her home and settle the mule in pepper and incense (if lent) / for a length of silk / for nothing | `_8`; `_home_free`; `_home_aboard` (10 days) |
| 9 | `crew.chain_pilgrim_road.s09_third_day` | under way with her aboard; optional | your cloak and the course / the cloth on the mast (likely) / heave to and lose the tide | `_9` |
| 10 | `harbour.chain_pilgrim_road.s10_sealed_leaf` | Sarre, 1 day on | see the leaf over the ferry (sure if unwatched; doubtful if `_sold`, `_named` or `_seen`) / give it to the king's man for 200 d (before Lammas 796) / the abbess's chest | `_10`; `_10a`, `_10b` or `_10c` (with the timer `_wait`, 90 days) |
| E1 | `harbour.chain_pilgrim_road.end_praen` | Sarre, Dofras or Fordwic, August 796 to the end of 797; needs `_10a`; drawn | his writing / the writing and a place among his men (hard road) / his pennies and nothing written | `_end_praen`; `_praens_man` |
| E2 | `harbour.chain_pilgrim_road.end_quiet_kent` | Lundenwic or Dofras, August 796 to 802; needs `_10b`; drawn | the king's protection / ground on the Strand / the protection, and half the bag to Saint Josse | `_end_quiet_kent`, `changed_kent_no_rising_796`; `_kent_remembers`, `_atoned` |
| E3 | `harbour.chain_pilgrim_road.end_minster` | Sarre, 90 days after `_10c` (or from 798 for a `_10a` that missed E1); drawn | the abbess's cross / a strip of the cloth / both (hard road) / only to be written in the book | `_end_minster` |
| 12 | `harbour.chain_pilgrim_road.s12_after_the_marsh` | Sarre, Dofras or Fordwic, autumn 798 to 803; after E1 | buy the new king's word (150 d) / let the writing lapse / "I sold him wine" / send 30 d for the blinded king's keep | `_12`; `_confirmed`, `_charter_void`, `_remembered_praen` |

**Roads through.** The errand: 1, (2), 3, 4, 5 (take), 6 (give), 7, 8, (9), 10 (carry), E1, 12. The informer: the same with 6 (sell) or 10 (give), then E2. The pilgrim's friend: 1, 3, 4, 5 (refuse), 7, 8, (9), 10 (chest), E3. Any mixture works: step 10 is open to everyone, whatever was done with the half penny.

**No dead ends.** Every card has an option that costs only time or pride (hear, ask the widow, let him write, wish her well, give it back, drop it, send word, take her home for nothing, your own cloak, the chest, pennies only, half to the exiles, the book, "I sold him wine"). A failed chance hurts and goes on: a doubted oath puts a name on a tablet; a seen handshake costs 20 d and the reeve's goodwill, and the token is still given; a stopped crossing costs 30 d and sends the leaf into the Wantsum and the story to E3. Each danger warns first: the clerk says where his tablets go; the reeve says he pays for answers; the horseman on the bank speaks before the ferry is tried. A player who reaches step 10 after Lammas 796 cannot inform; after 797 only the chest is left, and E3 is still a full ending.

## The payouts

| Tier | Where | Silver | And |
|---|---|---|---|
| Step | 1 | 30 d fare, or 0 | deed gave_alms and the widow's feeling; a true rumour (Kent's ports are watched) |
| Step | 2 | 0 | who the priest is; a true account of the road to Rome |
| Step | 3 | 0 (or 15 d paid) | half toll at Quentovic for 30 days on a believed oath |
| Step | 4 | 0 (60 d lent, or 12 d given) | Aldhelm's lighthouse riddle (`riddle_aldhelm_92_farus`); or a true price word: wax dear at Quentovic for 40 days |
| Step | 5 | 20 d if a price is named | the priest's promise |
| Turn | 6 | 40 d and a commission of two casks of Frankish wine at +50%; or 120 d from the reeve | the steward's friendship, or the reeve's |
| Step | 7 | 0 (or 40 d sent) | Aldhelm's book-chest riddle (`riddle_aldhelm_89_arca_libraria`), latin +0.05, true news of the exiles |
| Turn | 8 | two of pepper and two of incense (168 d) for the 60 d lent; or a length of silk trim (60 d) | deed gave_alms if for nothing |
| Step | 9 | 0 | the crew's heart, up or down |
| Turn | 10 | 200 d from the king's man; or 0 | half toll at Sarre for 60 days (the chest) |
| Finale E1 | Kent | 600 d, or 900 d | `unlock_charter_sarre`; or `unlock_factor_dofras` |
| Finale E1, hard road | Kent | 1,200 d | `unlock_charter_sarre` and `unlock_title_kent_kings_man` (needs the token given and the widow carried for nothing or lent to) |
| Finale E2 | Lundenwic or Dofras | 800, 600 or 400 d | `unlock_title_kings_protection`, or `unlock_plot_lundenwic` |
| Finale E3 | Sarre | 650 d | `unlock_charter_sarre`, or `unlock_relic_peters_cloth`, or `unlock_title_minster_thanet_brother` |
| Finale E3, hard road | Sarre | 1,200 d | `unlock_charter_sarre` and `unlock_relic_peters_cloth`, and a commission of six casks of Frankish wine at +80% (needs: brought home for nothing, and lent or sent silver) |

**Why a merchant could have won these.** A king's writing freeing a ship of toll in the Wantsum is exactly what Kentish kings gave (Sawyer 29), and a new king's first acts are such writings. The minster could put another hull in the place of a worn-out ship under its charter. A seat at a king's table, or a king's protection for a house, is what Charles's letter of 796 promises merchants of either realm. A factor at Dofras is a steward with a storehouse who owes you his neck. A strip of a cloth that lay on Saint Peter's tomb is the commonest relic a lay person could own. Names of friends were written in a church's book to be prayed for (the Durham Liber Vitae is LATER (?)).

**Costs that last.** `ch_pilgrim_road_praens_man`: your name is on a writing of a king who lasts two years; step 12 lets you buy King Cuthred's word over it (150 d, `_confirmed`) or lose the charter (`_charter_void`). `ch_pilgrim_road_kent_remembers` (E2): Kent knows, more or less, who sold the leaf; `_atoned` softens it. `_named`, `_seen` and `_sold` stay set for any later card at Quentovic or Dofras to read.

## The endings

| Ending | Flag | How | What becomes of it | Picture (`art.chain_pilgrim_road.ending.*`) |
|---|---|---|---|---|
| The priest is over the water | `ch_pilgrim_road_end_praen` | the leaf went over the ferry | Offa dies; Eadberht lands and takes Kent; the archbishop flees; two years later Coenwulf takes him in the Marsh, blinds and maims him (step 12) | `praen`: a new penny beside half of an old cut one on a thwart |
| Kent kept quiet | `ch_pilgrim_road_end_quiet_kent`, `changed_kent_no_rising_796` | the king's man was given the leaf while Offa lived | the six named are taken with their sons; when Offa dies nobody rises; the priest stays an exile at the Frankish court; the widow enters the minster and does not ask after you | `quiet_kent`: an unlit beacon stack grown over, a calm strait below |
| Written in the book | `ch_pilgrim_road_end_minster` | the leaf went into the abbess's chest (or into the Wantsum) | the rising comes and goes without it; the widow gives her land to the minster, takes the veil and teaches the sisters the Roman Alleluia | `minster`: a book of names on a lectern, a boat with a cross on her stem |

**What really happened** is E1 with step 12 after it. E3 is the same history seen from outside it.

**History changed, and what follows** (`changed_kent_no_rising_796`). Kent does not rise when Offa dies. There is no King Eadberht Praen and no coinage in his name; Archbishop Æthelheard never leaves Canterbury (so the Dofras story of the archbishop's chest, `dofras.archbishops_chest`, has no cause); Pope Leo writes no letter about an apostate priest; Coenwulf does not lay Kent waste in 798, so Sarre, Fordwic and Dofras are not harried that year, and he has no need to set his brother Cuthred over the Kentish (the game may still let him). The Mercian grip on Kent is unbroken from Offa to Coenwulf, which makes Coenwulf's plan to move the archbishop's seat to London (798) likelier to be pressed, and leaves Kent's own line without its last king a generation before Wessex takes the kingdom in 825. The planned chain kentish_revolt_796 should not start in such a world.

## Needs (for the integrator; nothing here was added to a shared table)

- Chronicle: when `flag.changed_kent_no_rising_796` is set, skip `796-summer-eadberht-kent` and `798-kent-ravaged`, and any card that tells them as news.
- The chain kentish_revolt_796 (CHAINS_LIST.md), if it is written: add `not flag.changed_kent_no_rising_796` to its start; it can read `ch_pilgrim_road_end_praen` and `ch_pilgrim_road_praens_man` to know the player is already the priest's man.
- `places/dofras.json` story `dofras.archbishops_chest`: add the same guard to its beats.
- `ch_pilgrim_road_charter_void` (step 12): withdraw `unlock_charter_sarre` if this chain alone gave it. The engine has no verb to unset an unlock safely when another story may have given the same one, so the card only sets the flag. A writing left unconfirmed after step 12 (the option "send thirty pennies for his keep" with `_praens_man` still set) should be treated the same way.
- Grand reward flags new to this chain: `unlock_title_kent_kings_man`, `unlock_factor_dofras`, `unlock_relic_peters_cloth`, `unlock_title_minster_thanet_brother`. Used and already known: `unlock_charter_sarre`, `unlock_title_kings_protection`, `unlock_plot_lundenwic`.
- Fordwic has no places file yet: the start, E1 and step 12 name `port.id = fordwic` and will simply not fire there until it is a port the player can lie at.
- A deed for carrying pilgrims (`carried_pilgrims`, asked for in npc_text_encounters.md sample 8) does not exist: `gave_alms` stands in.
- The lookout and the crewman in steps 2 and 9 are written as the spine's Beornwine and Ravn. If either has left the boat, the lines want another speaker (the engine will forge one of that role).
- For the engine: sea and crew cards are drawn by `rate`, not `weight` (Scheduler.HourlyWeight). This chain uses `rate`; some earlier chain files give their sea and crew cards a `weight` only, and those cards will never be drawn.

## Pictures

One for each card (`art.<card id>` in the writing file) and one for each ending (above).

| Card | Picture |
|---|---|
| s01 | A Kentish shingle strand, windy morning: the widow with staff and bundle, the girl, the priest with his back to the track, a small beached boat |
| s02 | Mid-channel in a calm, noon: the priest at an oar, the widow handing him bread first, the lookout watching them |
| s03 | The toll house on the Canche, grey morning: the clerk's stylus pointed at the priest; the widow and girl already in the doorway |
| s04 | The cell of Saint Josse, late sun: pilgrims along the south wall, a monk with a pot, the widow counting pennies, dunes behind |
| s05 | The bank at dusk, tide out: half a cut penny on the priest's open palm |
| s06 | The shingle at Dofras under the Roman wall: a steward tasting wine, a reeve questioning a skipper twenty paces off |
| s07 | A Frankish jetty in the wet: a barefoot monk on the gunwale with a satchel of letters |
| s08 | The south wall again, clear morning: the widow gaunt, two bundles, a girl's shoes, a folded cloth and a sealed leaf |
| s09 | The boat under sail, grey chop: the widow shaking under a cloak, three men looking at the skipper |
| s10 | The strand at Sarre, evening: the widow raised by a nun, the leaf held out; a horseman still on the bank |
| end_praen | A hall by firelight at harvest: the priest in a red cloak with a ring, pressing new pennies on the skipper |
| end_quiet_kent | A reeve's house, dull afternoon: a sealed bag on the table; through the door a strand with nobody armed |
| end_minster | The ship-house at Sarre, cold morning: a nun handing down a purse and a strip of cloth; the veiled widow above |
| s12 | The toll post in autumn rain: a Mercian with a list, his finger on a name; smoke inland |

## How long it takes

Steps 1 to 6 take about a week of sailing between Kent and the Canche. Then the road: 200 days until word comes from Rome, and 120 more until the widow is back at Saint Josse, so about eleven months from the first crossing to the second, during which the chain is two flags and the player trades as usual. Step 10 is the day after. The endings then wait for their real time: the widow's own comes 90 days after the chest; the other two wait for King Offa's death (drawn from August 796), and step 12 for the autumn of 798. A player who starts in the summer of 793 reaches the choice at Sarre in the summer of 794, the quiet ending that autumn, the kings' endings two years later, and the reckoning in 798: from fourteen months to five years.
