# Hours check: hours_793.json against the first release

Checked 7 Oct 2026. Read: Game/Data/story/hours_793.json (all 17 hours), Code/Core/Encounters/Cond.cs (VarNames), Code/Core/Encounters/Effects.cs (Verbs and ParseVerb). Nothing was run in Unity; the loader was not run on this file. Nothing in the project was changed.

The twelve: dorestad, traiectum, walichrum, lundenwic, witla, medemelacha, doccinga, daventre, quentovic, sarre, fordwic, ripa. The life: 793 to 805.

## What the engine knows

Condition names (Cond.cs, VarNames): whole names silver, debt, standing, crew, health, heart, month, year, hour, wind, fog, storm, ice, feast, fast, moored, under_way, port.id, port.kind, port.people, port.faith, port.realm, port.has_mint, port.toll, port.visits, port.best_feeling, hold.weapons, hold.captives, hold.value, learned.latin, learned.runes, chest.*, generation, and me.tongue, me.name, me.nerve, me.lore, me.troth, me.people, me.faith, me.past (all eight are in the table since 7 Oct). Families: hold.<good>, hold.from_<x>, tongue.<name>, crew.<x>_best, crew.<x>_count, npc.<role>.feeling / .met / .owed / .generation / .last, text.<id>, riddle.<id>, flag.<name>.

Effect verbs (Effects.cs): silver, goods, health, morale, men, toll, deed, npc, flag, schedule, news, price_shock, commission, give_text, give_riddle, open_fetch, hire, pay_off, passenger, encounter, law, learn, tongue.

## The table

"There" = its port is one of the twelve (or its water lies between them) and its window opens within 793 to 805. "Door" = every variable name in the door is one VarNames accepts. "Effects" = every string in every outcome's "effects" list starts with a verb of Effects.cs and fits that verb's grammar in ParseVerb.

| # | hour | window | port / water | reach | there? | door names | effects verbs |
|---|------|--------|--------------|-------|--------|------------|---------------|
| 1 | lindisfarne_boys_793 | 793-06-22 to 794-06-01 | dorestad, quentovic | twelve | yes | all known | ok (flag, deed, npc, give_text, silver) |
| 2 | frisian_rising_793 | 793-09-01 to 794-08-31 | doccinga | twelve | yes | all known | ok (flag, npc, deed) |
| 3 | aethelberht_794 | 794-04-20 to 794-05-12 | lundenwic | twelve | yes | all known | ok (flag, flag .. for 720, price_shock, npc) |
| 4 | donemuthe_794 | 794-06-01 to 794-08-31 | gyrwe; off the mouth of the Tyne | dim | NO: gyrwe is not of the twelve, the water is north of them | all known | ok (flag, deed) |
| 5 | thule_795 | 795-08-01 to 796-12-31 | fordwic, quentovic | word | ports yes, dates yes; but reach says "word" (news only) | all known | ok (flag) |
| 6 | black_stones_796 | 796-04-10 to 796-07-29 | quentovic, lundenwic | twelve | yes | all known | ok (flag, deed, npc, toll eased) |
| 7 | offa_dead_796 | 796-07-29 to 796-08-25 | lundenwic, fordwic, sarre | twelve | yes | all known | ok (flag, silver, npc) |
| 8 | kent_798 | 798-05-01 to 798-09-30 | sarre, fordwic | twelve | yes | all known | ok (flag, npc, toll eased) |
| 9 | king_on_the_coast_800 | 800-03-20 to 800-05-05 | quentovic | twelve | yes | all known | ok (flag, silver, toll eased, npc) |
| 10 | archbishop_crossing_801 | 801-04-12 to 801-09-30 | fordwic, quentovic; the strait | twelve | yes | all known | ok (flag, silver, npc, toll eased) |
| 11 | elephant_802 | 802-05-01 to 802-07-20 | colonia; the Rhine above the twelve | word | NO: colonia is not of the twelve; where_met says "In the first release, nowhere" | all known | ok (flag) |
| 12 | eadburh_802 | 802-03-01 to 802-10-31 | quentovic; the strait | twelve | yes | all known | ok (flag, silver, npc) |
| 13 | ecgberht_802 | 802-03-01 to 802-10-31 | quentovic; the strait and the south coast | twelve | yes | all known | ok (flag, silver, npc) |
| 14 | frisian_law_802 | 802-11-01 to 803-09-30 | dorestad | twelve | yes | all known | ok (flag, toll eased) |
| 15 | godescalc_798 | 798-03-10 to 798-04-08 | ripa | twelve | yes | all known | ok (flag, silver, passenger .. to dorestad, npc) |
| 16 | saxons_804 | 804-06-15 to 804-09-15 | daventre, ripa | twelve | yes | all known | ok (flag, silver) |
| 17 | hunger_805 | 805-09-01 to 806-08-15 | dorestad, traiectum, walichrum, witla, medemelacha, doccinga, daventre, quentovic | twelve | yes, but it runs to 15 Aug 806, past 805 | all known | ok (flag, deed, npc, toll eased) |

Totals: 17 hours. 15 on the twelve by port, 2 not (donemuthe_794, elephant_802). 16 wholly within 793 to 805; hunger_805 opens in 805 and runs into 806. Doors: 17 of 17 use only names VarNames accepts. Effects: every effect string in the 17 hours uses a verb of Effects.cs and fits its grammar.

## The hours that cannot be played as written, and why

1. donemuthe_794. Port gyrwe and the water off the Tyne are outside the twelve (the file itself marks it "dim"). Its door, "flag.hr_fleet_heard and under_way", names no place: as written it is true anywhere at sea once the Ripa rumour has been heard. In the first release it is a rumour and a chronicle line only. Its flag hr_donemuthe_witness is one of the ways into king_on_the_coast_800, so that way in is closed in the first release.

2. elephant_802. Port colonia is outside the twelve; where_met: "In the first release, nowhere: the news comes down the Rhine." Its door, "me.faith = jewish or me.lore >= 4", names no place, so nothing in the door keeps it off the twelve. News only.

3. thule_795. Its ports (fordwic, quentovic) and dates are fine and it has a talk, but reach is "word", which the file's own key reads as "it comes only as news". One of the two is wrong. Its door names no port and no "moored".

4. ecgberht_802, the option "sell" (the Turn). Its check is "year < 802" but the hour's window is 802-03-01 to 802-10-31, so inside the window the check is never true. The Turn is also marked "held": true, and its chronicle line is dated 799. The witness and nudge of this hour are playable.

5. hunger_805. Playable from 1 Sept 805, but ten and a half months of its window (to 15 Aug 806) lie after 805; its nudge chronicle line is dated 806. If the first life ends with 805, most of the hour is cut off.

## Playable, but with things the data does not hold together (not checked against the loader)

- Doors with no place in them. Only the hour's "port" field ties these to a place; the door itself would be true anywhere: lindisfarne_boys_793, donemuthe_794, thule_795, black_stones_796, king_on_the_coast_800, elephant_802, the Ripa half of saxons_804. hunger_805 uses "port.realm = franks" and no "moored". Whether the runner reads the "port" field as a gate I did not check.
- Flags a door needs that no effect in this file sets (see the note further down: the names do occur in other data files):
  - lf_trail_north (doors 1 and 9): thread holy_island; not set here.
  - bs_ban_heard (door 6): not set here (chain black_stones).
  - won_case_dorestad (door 14): not set here.
  - gf_warned (door 9, year 800): named only in the "then" prose of saxons_804, which is 804, four years later.
  - hr_archbishop_warned (door 10): named only in the "then" prose of offa_dead_796; no effect sets it.
  - lf_knows_boys (door 1) is set only by hour 1's own nudge; ms_carried_priest (door 2) only by hour 2's own nudge. Each door has other ways in.
- Flags set by a shadow's "sets", not by an effect: hr_lul_penny, hr_fleet_heard, hr_offa_news, hr_king_asks, hr_envoy_seen, hr_godfred_muster. These doors (3, 4, 7, 9, 15, 16) depend on the rumour system setting flags.
- npc roles in doors and effects that are not among the hour's "actors" keys: npc.priest (hour 2; the only actor is great, role priest), npc.abbot (hour 6; only great, role abbot), npc.reeve (hour 8; actors great, abbess), npc.bishop (hour 10; only great, role bishop), npc.abbot (hour 13, door and the Turn's "npc abbot feeling -40"; only great), npc.host (hour 16; actors great, count). Hour 7 names an actor "bishop" that no effect uses. Whether a role resolves to the port's own person of that role I did not check.
- Words compared in doors that I did not check against their tables: the feeling words friendly and easy; port.realm = franks; the pasts captive_home, toll_clerk, hall_child; the peoples frisian, danish, irish, english, frankish; the faiths christian, jewish; tongue.irish, tongue.old_english.
- Checked after writing the list above: the goods wheat, rye, barley, oats, wool_cloth and ipswich_ware are all ids in goods_793.json. The five flags lf_trail_north, bs_ban_heard, gf_warned, won_case_dorestad, hr_archbishop_warned together occur 132 times in 35 files under Game/Data (among them chain_lindisfarne_book_793.json, chain_black_stones_793.json, chain_coast_watch_810_793.json, england_south_east_793.json, places/dorestad.json, places/fordwic.json, places/ripa.json), so they are not names used only here; I did not count each flag by itself or check which of those lines set it rather than read it.
- Ids in effects not checked against their tables: deeds carried_letters, gave_alms, fought_raiders; text alcuin_793.
- Outside the verbs by the file's own word: "stats" (name, nerve, lore, troth pips), "heirloom" (hours 3, 9, 10) and everything in "then" (the per-option flags hr_praen_escaped, hr_guard_<port>, hr_law_wreck / hr_law_oath / hr_law_tolls, hr_isaac_letter, hy_fed_<port>, hy_sold_dear, hy_ran_grain, db_extension, hr_told_the_priest). No effect sets any of these. The "fails" texts are prose with effects written inside them ("npc great feeling -8", "flag wanted_mercia for 720"); they are not effect lists.
- Each hour has one witness and one nudge outcome, but several hours say in "then" that the result differs "by the option" (7, 8, 9, 14, 16, 17). The format has no place for that yet.
- Turns: aethelberht_794 has the one Turn of the first life; ecgberht_802's is held (see 4 above). The rule "the door stood open for at least two years" is not expressible in the door of aethelberht_794: its flag hr_lul_penny is set by a rumour 60 days before the window.
