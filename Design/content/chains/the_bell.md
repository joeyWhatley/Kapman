# Chain: the_bell (a bell for the church at Sliesthorp)

Cards: `Assets/Kapman/Game/Data/encounters/chain_the_bell_793.json` (14 cards: 13 harbour, 1 sea).
Words: `Assets/Kapman/Game/Data/writing/chain_the_bell.json` (222 entries, with an `art.<card id>` line for every card).
Written 6 Oct 2026. A draft for the user to overwrite. (?) marks what could not be checked against a source.
It belongs to the spine's thread `cross_and_grove` and stands a few years after the chain `mission_north` (the bell at Birka, about 852): if that chain's flag `ch_mission_north_bell` is set, the Danish chief says so.

## The story in five sentences

In 854 the old King Horik dies in a war of kinsmen, the count of Sliesthorp nails the church door shut, and within a year or two the young King Horik puts the count out, lets the priest come back and allows the church a bell, a thing the heathen held unlawful; the priest builds the frame the same week and has nothing to hang in it.
The Christians of the port make up a purse and ask a merchant who goes up the Rhine to have one poured at Colonia, while in the ale-house a ship-owner of the old count's kin says plainly what he fears from it and what he will do: buy it and put it in the water.
The founder wants six bars of copper and one of tin, or 420 pennies, or the broken bronze on a Danish caster's cloth that was a bell on the Elbe until the ships burnt Hamburg; he pours at night, and twelve days later the bell is struck for the first time with a mallet of applewood.
It comes down the Rhine under straw, past Dorestad, which has lost its own bell and offers good silver for this one, and through a fog on the coast in which a horn is blowing on the wrong side of a bank.
At Sliesthorp it is carried up to the church to wait for the king's man and the lot, or taken round to the king's ground at Ripa, or sold on the jetty; and it ends rung for Mass, rung first in fog for every keel on the Slie, hung on a corner post at Ripa, sunk in the inner water, worn as brooches, or hanging on the gable at Dorestad with a boy beside it watching the river.

## How a player meets it

- `s01_empty_frame` is drawn on a call at **Sliesthorp from 855 to 861** (weight 4; it comes again after 20 days if passed by). The frame, the plank and the mallet are in the picture of the church site: a place writer may add `card:harbour.chain_the_bell.s01_empty_frame` to the church site at Sliesthorp and `card:harbour.chain_the_bell.s03_founder` to `colonia.smiths_row`.
- Asking "what the Danes say" before taking the errand opens `s02_what_they_fear` first.
- The news `chain_the_bell.news.passing` is posted at Dorestad if the bell is rung there from the yard, so a second house (or an heir reading old news) hears of it.

## The real history under it

**The church and the bell.** Rimbert, Vita Anskarii (written about 870; tr. C. H. Robinson 1921). c. 24: the elder Horik lets Anskar build a church at Sliaswic, "the port of his kingdom", gives the plot, and merchants of Dorstadt and Hamburg then come there freely. c. 31: in 854 Horik and nearly all the great men fall in a war with his kinsman; the count of the port, Hovi, shuts the church and forbids the rite, and the priest leaves. c. 32: the younger Horik drives Hovi out, asks for the priest back, and allows the church a bell, "the use of which the pagans regarded as unlawful" (the words as quoted in `story/unlocks_793.json`; Rimbert's Latin word is nefandum); he also gives ground at Ripa for a second church and leave for a priest to live there. The years are put at about 855-857 (`realms/denmark.json`; German Wikipedia gives 854 for the leave). The Fordham page of Robinson's translation was fetched tonight but came back cut off at c. 16, so c. 24 and c. 31-32 are given from the project's own notes and from memory (?).

**Burghard.** Rimbert c. 32 names a count Burghard, kin to both Horiks, who had helped Anskar with the elder king and stood well with the younger (from memory of Robinson (?)). He is the king's man on the stone at step 10. Whether he was baptised is not said (?).

**Why the heathen held it unlawful.** Rimbert does not say. English Wikipedia (Horik II) says the fear was that bells would frighten away the land spirits and harm the harvest, without a source. The nearest real warrant is much later and Icelandic: the first law is said to have ordered ships to take down their carved heads before sighting land so as not to frighten the land's guardian spirits (Landnamabok, 13th century, from memory (?)). The chain gives Asgaut that fear, with herring and corn as its stake, and marks it a guess.

**Lots.** Among Danes and Svear a new thing was put to the gods by lot in the open before the thing: Rimbert c. 19, 27 (Birka), c. 30 (the Danes before Birka and Kurland). That lots were cast at Sliesthorp over the bell is invented.

**Hamburg, 845.** King Horik's fleet takes Hamburg; church, cloister and Anskar's books burn (Rimbert c. 16; Annales Bertiniani 845). That its bell was broken up and lay eleven years later in a brooch-maker's scrap at Sliesthorp is invented; the church's old metal going north as scrap is how it would have gone.

**The bell found at Hedeby.** A beehive-shaped bronze bell cast about 950 was found in the harbour of Hedeby in 1978 and is in the museum there; a fragment of an older bell was reconstructed in 1998 as the "Ansgar bell" (de.wikipedia, Glocke von Haithabu; fetched tonight). The page gives no weight; "what a boy of ten weighs" (about 25 kg) is from memory (?). It is why the water is where a bell goes in ending 3, a century early; how the real one came to lie there nobody knows.

**How a bell was made.** About four parts of copper to one of tin (en.wikipedia, Bellfounding; `economy/stone_metal_arms.md`). The method in the cards (a loam core, the bell modelled on it in wax, a loam cope, the wax burnt out, the mould buried in a pit, a furnace blown by several bellows) is Theophilus, De diversis artibus III.85, of about 1120: nearly three centuries LATE, taken as the older practice (?). Walahfrid Strabo, about 840, knows bells both cast and beaten (De exordiis c. 5, from memory (?)). Pouring at night to judge the colour is told from memory of later founders' practice (?). Six bars of copper and one of tin are the figures of `story/works_793.json` 'bell'; tin is Cornish (`goods_793.json`).

**The cheating founder.** Notker of Saint Gall, Gesta Karoli I.29 (883-887; Grant 1907): a founder asks Charlemagne for a hundred pounds of silver in place of tin, keeps the silver, and is killed by the clapper of his own dumb bell. Told here in our own words by Reginbert as a trade story, thirty years before Notker wrote it down (?). Not quoted.

**Blessing, not baptising.** The Admonitio generalis of 789 forbids the "baptism" of bells (c. 34, from memory (?)), which shows they were being washed and named.

**Cologne, 857.** In a great storm the people ran into Saint Peter's and rang the bells; lightning killed a priest, a deacon and a layman at three altars (Annales Fuldenses 857; `dossiers/colonia.md`). The founder's line about it shows from 857.

**Dorestad.** Burnt in 846-847, held by the Dane Rorik for the emperor Lothar from 850, plundered again in 857 while he is away, sacked for the last time in January 863 (Annales Bertiniani; `realms/denmark.json`). "Many churches, priests, and clergy, and a multitude of poor people" (Rimbert c. 20). That the upper church's bell was carried off is invented.

**Ripa.** Ground and leave from the younger Horik (Rimbert c. 32); a church about 860; graves under the later cathedral from these years (`realms/denmark.json`). Ripa's sailings to Frisia, England and Saxony: Adam of Bremen iv.1 (about 1075, late).

**Invented, and said so.** Folcward the priest (Rimbert names no priest here), Hebbe of Dorestad, Asgaut the ship-owner and kinsman of Hovi, Tosti the caster, Reginbert the founder, Hildulf of the upper church; the purse; the fog; the offer on the jetty. Eystein the toll man is the spine's (`cast.eystein`, invented).

Web: no searches; four fetches (Fordham's Rimbert, cut off; de.wikipedia Glocke von Haithabu; en.wikipedia Bellfounding and Horik II). A fifth, Fordham's Notker, returned a summary that named the wrong translator, so nothing was taken from it.

## The people

| Who | Id | In the cards |
|---|---|---|
| Anskar, archbishop at Bremen | `cast.anskar` (to 865), historical | spoken of ("the bishop"), never met |
| The two Horiks; Hovi, count of the port; Rorik | `cast.horik`, `cast.rorik`; Hovi is not in the spine; historical | spoken of |
| Burghard, the king's man | historical (Rimbert c. 32 (?)); not in the spine | actor `count` at step 10 |
| Folcward, priest at Sliesthorp | invented | actor `priest` at steps 1, 9, 10, 11 |
| Hebbe, a Dorestad man | invented | actor `merchant` at steps 1, 11 |
| Asgaut, ship-owner | invented | actor `chief` at steps 2, 9, 10, 11, 13 |
| Tosti, caster of brooches | invented | actor `caster` at steps 2, 13 |
| Reginbert, founder at Colonia | invented | actor `founder` at steps 3 to 6 |
| Hildulf, priest of the upper church | invented | actor `priest` at steps 7, 14 |
| Eystein, Rorik's toll man | `cast.eystein`, invented | actor `tollman` at steps 7, 14 |
| A priest of Saint Peter's; the priest and the king's man at Ripa; a steersman and a Danish hand | unnamed | actors `clerk`, `priest`, `kings_man`, `steersman`, `crewman` |

## The steps

`P_` stands for `ch_the_bell_`. Waits are flags with a term (`P_3_wait for 40`): the next card will not come until the days have run. "Likely" is a chance of 0.6 to 0.65 before the crew's skill, "doubtful" 0.5, "sure" 0.8 and over.

| # | Card id | Where and when | The choices | Flags set |
|---|---|---|---|---|
| 1 | `harbour.chain_the_bell.s01_empty_frame` | Sliesthorp, 855 to 861; drawn | take the errand and 120 d of the pledge / take it and leave the purse (a wine order instead) / ask what the Danes say / pass | `P_1`; `P_purse` or `P_nopurse`; `P_warned` |
| 2 | `harbour.chain_the_bell.s02_what_they_fear` | Sliesthorp, after 1 or after asking; optional; **the warning** | ask what a ship does in fog (needs Norse) / buy the broken bronze (90 d) / thank him and go | `P_2`; `P_fog_idea`; `P_scrap`; `P_2_done` |
| 3 | `harbour.chain_the_bell.s03_founder` | Colonia, after 1 | metal and work, 420 d / bring the bars, pay the work later / the broken bronze and 150 d / watch a pour first (a riddle) | `P_3`, `P_3_wait` (40 days); `P_metal` or `P_bring`; `P_old_bronze`; `P_watched` |
| 4 | `harbour.chain_the_bell.s04_the_metal` | Colonia, only on the "bring" road | six copper, one tin and 150 d / his metal after all, 440 d / the broken bronze and 150 d / "the bars are coming" | `P_metal`; `P_old_bronze` |
| 5 | `harbour.chain_the_bell.s05_the_pour` | Colonia, 40 days after 3, metal in hand | crew on the bellows (sure) / sleep aboard (likely) / sixty pennies for the pot | `P_5`, `P_5_wait` (12 days); `P_flawed` on a failure |
| 6 | `harbour.chain_the_bell.s06_the_voice` | Colonia, 12 days after 5 | load it plain / have it blessed (12 d) / break and pour again (120 d, 20 days; only if flawed) | `P_6`, `P_aboard`; `P_blessed`; clears `P_flawed` |
| 7 | `harbour.chain_the_bell.s07_dorestad` | Dorestad before 863, bell aboard; can be sailed past | sail on / ring it once from the yard / let it hang here | `P_7`; `P_heard_dorestad`; or `P_dorestad`, `P_7_wait` (14 days), `P_aboard` cleared |
| 8 | `sea.chain_the_bell.s08_fog` | at sea, bell aboard; once | strike the bell for the other boat (likely) / feel off with the lead (sure) / anchor | `P_8`; `P_fog_saved` on a success |
| 9 | `harbour.chain_the_bell.s09_the_strand` | Sliesthorp, bell aboard | carry it to the church / tell him about the fog (needs `P_fog_idea` or `P_fog_saved`) / take it to Ripa / ask his price | `P_9`; `P_9_wait` (20 days), `P_aboard` cleared; `P_fog_offer`; `P_to_ripa`; `P_sold` |
| 10 | `harbour.chain_the_bell.s10_the_lot` | Sliesthorp, 20 days after it goes ashore | let the lot fall (doubtful) / offer it to the port for fog (needs the fog flags; sure) / a gift to the lot-cutter (80 d; sure) | `P_10`; `P_lot_for` and `P_10_wait` (25 days), or `P_lot_against`; `P_peace`; `P_lot_bought` |
| E1 | `harbour.chain_the_bell.s11_first_ringing` | Sliesthorp, 25 days after a lot for; at once after a lot against | ring it for Mass / ring it first in fog (needs `P_peace`) / hang it tongueless a year (lot against; the card returns after 300 days) / take it to Ripa (lot against) | `P_end_rung`, `P_done`; `changed_sliesthorp_bell_for_all`; `P_tongueless`; `P_to_ripa` |
| E2 | `harbour.chain_the_bell.s12_ripa` | Ripa, any year from 855, bell aboard | hang it and be paid / give it and send the purse back / sail on | `P_end_ripa`, `changed_first_bell_at_ripa`, `P_done` |
| E3, E4 | `harbour.chain_the_bell.s13_inner_water` | Sliesthorp, at once from step 9's "price" | sink it for his silver / sell it to the caster / say no after all | `P_end_drowned` or `P_end_melted`, `changed_sliesthorp_bell_silenced`, `P_church_remembers`, `P_done`; "no" clears `P_sold` and sets `P_9_wait` |
| E5 | `harbour.chain_the_bell.s14_upper_church` | Dorestad before 863, 14 days after step 7's sale | silver and a plot / less silver and toll freedom / send the northern purse back first | `P_end_dorestad`, `changed_dorestad_bell`, `P_done` |

**Roads through.** The straight one: 1, 3, 5, 6, (7), (8), 9, 10, E1. The careful one: 1, 2 (fog, scrap), 3, 5, 6, 8 (strike), 9 (fog), 10 (offer), E1 by the fog. The thrifty one: 2 (scrap), 3 (scrap). The trader's: 3 (bring), 4. To Ripa: from 9, or from E1 after a lot against, or simply by calling at Ripa with the bell aboard. To the water: 9 (price), E3 or E4, or back to 10. To Dorestad: 7 (sell), E5.

**No dead ends.** Every card has an option that costs only time or pride (pass, thank him, bring the bars, "they are coming", the bellows, load it plain, sail on, anchor, carry it ashore, let the lot fall, wait a year, sail on, say no, take the plot). A bad pour gives a bell with a short voice, which every ending still takes. A lot against is a year's wait or Ripa. A price asked can be refused. The sale at Dorestad is not offered in 862, so that the finale can still be reached before the town is gone. The bell is a flag, not a unit in the hold.

**Every danger warns first.** Asgaut says at step 2, before witnesses, exactly what he will do, and says it again on the jetty. The founder explains a cold pour before the choice to sleep. The steersman says what the weight will do if she touches.

## The payouts

| Tier | Where | Silver | And |
|---|---|---|---|
| Step | 1 (take) | +120 d (the pledge's advance) | the errand |
| Turn | 1 (no purse) | a premium of 1.5 on four casks of Rhine wine (about 120 d over the price) | deed fair_dealing; the priest's and Hebbe's feeling |
| Step | 2 | 0 (the bronze costs 90 d) | the fog road; the cheapest metal |
| Step | 3 (watch) | 0 | the riddle `riddle_exeter_83_ore`; the founder's story |
| Turn | 3 (bring) | a premium of 1.8 on three bars of tin (about 96 d over the price) | a commission at Colonia |
| Cost | 3, 4 | -420 d all in; or -150 d and six copper and one tin (about 220 d at common prices); or -150 d with the broken bronze (-90 d) | the bell |
| Step | 5 (silver) | -60 d | deed gave_alms; the founder's feeling |
| Step | 6 (bless) | -12 d | the crew's hearts; deed gave_alms |
| Step | 7 (ring) | +24 d thrown into the boat | the crew's hearts; news at Dorestad |
| Turn | 8 (strike, won) | a timber of marten (120 d) | `P_fog_saved`: a Danish boat owes you its keel |
| Finale | E1 ring | 700 d | `unlock_work_sliesthorp_bell`; half toll at Sliesthorp for a year; deed kept_commission |
| Hard road | E1 by the fog | 1,100 d and two timbers of marten (240 d) | `unlock_work_sliesthorp_bell` **and** `unlock_plot_sliesthorp`; Asgaut a friend |
| Finale | E2 hang | 500 d and six sacks of amber (180 d) | `unlock_plot_ripa`, `unlock_pilot_ripa`, `unlock_work_ripa_bell` |
| Finale | E2 give | ten sacks of amber (300 d); a premium of 1.8 on six casks of wine (about 290 d over the price) | the same three, deeds gave_alms and fair_dealing, the purse cleared |
| Finale | E3 sink | 900 d | `unlock_title_danish_guest_right`; the cost `ch_the_bell_church_remembers` |
| Finale | E4 melt | 620 d and eight pairs of brooches (192 d) | `unlock_heirloom_bell_brooches`; the same cost |
| Finale | E5 | 650 d, or 600 d | `unlock_plot_dorestad` or `unlock_charter_dorestad`, with `unlock_work_dorestad_bell` |

**Why a merchant could have won these.**
- *The bell as the house's work* (`unlock_work_sliesthorp_bell`, already in `story/unlocks_793.json`): the king allowed a bell and somebody paid for it. The table gives what it does: Christian merchants trust the port and the house; heard on the water in fog.
- *A plot at Sliesthorp* (in the table): plots there were the king's man's to give to merchants who would stay. On the hard road Burghard gives it because the port, not only the church, is in the house's debt.
- *A plot and the river at Ripa* (both in the table): the plots along the river were handed out by the king's man; the channel over the flats is what a grateful old pilot has to give.
- *A first bell at Ripa* (`unlock_work_ripa_bell`, new): an instance of the work `bell` at the church the king gave ground for.
- *Danish guest right* (in the table, first set by the chain house_marriage): guest-friendship with a ship-owner keeps his ships and his friends' off yours. It is what Asgaut has to sell besides silver.
- *The brooches* (`unlock_heirloom_bell_brooches`, new): a pair of oval brooches, the first out of the mould, cast from the bell. Rhenish bronze was melted in the north for exactly this. An heir's wife wears them; a priest who knows what they are will say so.
- *Dorestad* (plot and charter in the table; `unlock_work_dorestad_bell` new): the church held bank land; the toll was Rorik's and his man's word was the only charter there was in those years. Both end with the town in 863.

**Costs that last.**
- `ch_the_bell_church_remembers` (E3, E4): Anskar's priests know who sold the bell. To be read by Christian cards at Sliesthorp, Ripa and Dorestad, and by Rimbert after 865.
- `ch_the_bell_purse` still set after `ch_the_bell_done`: the Sliesthorp pledge of 120 d was never sent back (possible at E2 hang, E3, E4, E5). A pledge broken; Hebbe is a Dorestad man.
- `ch_the_bell_lot_bought`: the port knows the lot-cutter was paid. Asgaut says so at the ringing.
- `ch_the_bell_flawed`: the bell has a short voice. It is remarked on and changes no payout.
- `ch_the_bell_blessed`: the Danes heard it was sung over. Words only.

## The endings

| Ending | Flag | How | What becomes of it | Picture |
|---|---|---|---|---|
| Rung | `ch_the_bell_end_rung` | carried ashore, the lot for it (or a year's silence), rung at the feast | **What happened.** The bell is rung at Sliesthorp by the king's leave. Some Danes turn their backs. The Christians of the port trade more easily, as Rimbert says they did. | The church door on a feast morning: the merchant on the rope, the bell at the top of its swing, Christians close, Danes at a chosen distance. |
| Rung for all (the hard road) | `ch_the_bell_end_rung` with `ch_the_bell_peace`, `changed_sliesthorp_bell_for_all` | learned what the port fears and what it lacks, struck the bell in fog at sea, said so on the jetty and at the thing | **History changed.** The first time the bell is heard it brings three boats up the Slie in fog, one of them Asgaut's. The thing the heathen held unlawful becomes a thing the port owns. | The jetty in thick fog: the bell slung from a post, the merchant ringing, a boat's grey shape coming in on the sound. |
| Ripa | `ch_the_bell_end_ripa`, `changed_first_bell_at_ripa` | taken west from the jetty, or after a lot against, or by choice on the way | **History changed.** The first bell among the Danes hangs at Ripa on a corner post before the church has walls. Sliesthorp's priest goes on beating a plank. | Ripa at low water: four posts on pegged ground, a sail for a roof, the bell under a hat of thatch, geese going up. |
| Drowned | `ch_the_bell_end_drowned`, `changed_sliesthorp_bell_silenced` | sold to Asgaut and rowed out | **History changed.** The king's leave stands and the frame stays empty. The merchant is a chief's guest on that coast and no friend of the bishop's. | Night on black water: two men tipping a bell over the gunwale; one light on shore by an empty frame. |
| Melted | `ch_the_bell_end_melted`, `changed_sliesthorp_bell_silenced` | sold to the caster | **History changed.** Half the women of the port wear the Christians' bell to the old gods' feasts. The house keeps the first pair. | Behind a workshop: a caster with a stone maul over a broken bell; a pair of new oval brooches on a cloth in front. |
| Dorestad | `ch_the_bell_end_dorestad`, `changed_dorestad_bell` | sold to the upper church on the way down the river | **History changed.** Dorestad has a bell and a watch on the roof. The church among the Danes waits for the next merchant. | The upper church's gable: the bell under the eaves, a boy on the ridge shading his eyes down the river. |

**What would follow in later years** (for later cards to read from the flags):
- `ch_the_bell_end_rung`: nothing changes in the record. Rimbert (after 865) and later priests at Sliesthorp know the house; the work's own rule applies (trust among Christians there, the bell in fog, a raid's take from stores halved).
- `changed_sliesthorp_bell_for_all`: the bell is the port's as well as the church's. When the church is next in danger (the sources are silent on Sliesthorp's church from the 860s to 948), a card may let the ship-owners keep the bell rung "for the fog" while the door is shut; an heir is Asgaut's kin's guest without having sunk anything.
- `changed_first_bell_at_ripa`: Ripa, not Sliesthorp, is where a Christian trader first feels at home among the Danes; when bishops are named for both in 948 the house's name stands at Ripa. Sliesthorp's frame is empty until someone else fills it (the work `unlock_work_sliesthorp_bell` can still be bought there in the ordinary way).
- `changed_sliesthorp_bell_silenced`: the Christians of Sliesthorp keep their church and have no bell; merchants of Dorestad go there a little less freely than Rimbert says they did. With `ch_the_bell_end_drowned`, a card of about 950 may have a new bell brought, and an old man who remembers his grandfather's story of where the first one lies. With `ch_the_bell_end_melted`, the heirloom is recognised by a priest sooner or later.
- `changed_dorestad_bell`: in January 863 the Danes come up the Rhine and sack Dorestad for the last time, killing many Frisian merchants. With the bell and the boy on the roof, the house (and a card may say: half the upper bank) gets its people and its scales into boats in time. The town still ends; fewer of its merchants die in it, and more of them are at Tiel and Deventer afterwards to remember who bought the bell.

## How long it takes

It can begin no earlier than 855. Sliesthorp to Colonia is a long voyage in a small boat. The mould takes 40 days and the cooling 12, so the bell is aboard two months after the founder is first met, at the soonest. Twenty days ashore wait for the king's man and 25 more for the feast: by the straight road a bell asked for in spring hangs by autumn or the next Easter. A second pour adds 20 days; a lot against adds 300 days of silence, or the voyage round to Ripa. Told at an ordinary trader's pace, with the metal fetched from Hamwic and the tin order kept, it is a year to two years, alongside other work. The start stays open to 861; Dorestad's ending closes with 862.

## For the integrator

- New flags the game will read: `unlock_work_ripa_bell`, `unlock_work_dorestad_bell`, `unlock_heirloom_bell_brooches`; the five `changed_` flags; the costs `ch_the_bell_church_remembers`, `ch_the_bell_purse`, `ch_the_bell_lot_bought`.
- Flags from the unlock table that the chain sets: `unlock_work_sliesthorp_bell`, `unlock_plot_sliesthorp`, `unlock_plot_ripa`, `unlock_pilot_ripa`, `unlock_title_danish_guest_right`, `unlock_plot_dorestad`, `unlock_charter_dorestad`.
- The chain reads one flag of another chain, `ch_mission_north_bell`, for a single line.
- Clearing a flag is written `flag <name> = 0`.
- The outcome texts (`...out_<option>`, with `_success` and `_failure` after a chance) are in the writing file; each option's `note` names its own. The six `chain_the_bell.news.*` entries are the news the chain posts.
- Sliesthorp has no places file yet: Folcward, Hebbe, Asgaut and Tosti would be its people (`sliesthorp.folcward` and so on), and the church site there should show the empty frame until `unlock_work_sliesthorp_bell` or `unlock_work_ripa_bell` is set.
