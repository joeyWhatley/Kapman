# Trap chains: nine chains that start in the first release and cannot end there

7 October 2026. First release: one life, 793 to 805, twelve ports
(dorestad, traiectum, walichrum, lundenwic, witla, medemelacha, doccinga, daventre, quentovic, sarre, fordwic, ripa).
Card files: Assets/Kapman/Game/Data/encounters/chain_<name>_793.json (read only; nothing changed).
Method: every card's "when", the flags its options set and the cards it schedules were listed by a script
and followed by hand from the start card. Not run in the game. The words files were not read.

## Short answer

| chain | starts | first step that cannot happen | recommend |
|---|---|---|---|
| amber_road | s01_beadmaker, ripa | s02_wake: truso | leave out |
| dirham_hoard | s01_dying_gute, dorestad or ripa, 805 only | s04_beach: paviken | leave out |
| house_new_ship | s01_wright, dorestad | s02_oak: colonia | move s02_oak to daventre, drop its river-ship option |
| lindisfarne_book | s01_strip_of_gilt, ripa | s05_box_of_skins: ogvaldsnes, skiringssal | move s05 to ripa; ends at quentovic (end_alms) |
| riddle_trail | s01_stick, dorestad, 796 on | s04_ferryman: sliesthorp | end early after s03 with one new card at dorestad |
| saga_rhine_frisia | s01_lots, doccinga, 794 to 801 | s02_east: emuthon | leave out |
| saxon_exiles_804 | s01_column, daventre, July 804 on | s02_sheep_house: emuthon | leave out |
| silver_road | s01_faceless_coin, dorestad, 797 on | s02_scale: birka | leave out |
| wine_fleet | s01_second_keel, dorestad, 796 to 802 | s03_saints_day: parisius (s02: rotomagus) | leave out |

Six out, two moved, one ended early. cards.md line 429 already plans to leave all nine out; that is the safe
choice. The three repairs below are the only ones that cost one card each.

## amber_road (14 cards)
- Starts: s01_beadmaker, harbour, ripa (or sliesthorp), year < 846. Sets ch_amber_road_1.
- Blocked: s02_wake, "port.id = truso". Truso is not one of the twelve.
- After it: s03 to s06, s09 and three endings are all at truso; end_shut is at sliesthorp. s07_west at ripa
  needs ch_amber_road_6 (set only at truso). s08_wends (sea) needs ch_amber_road_2 (truso). x_wulfstan is 880 to 899.
- So only s01 ever shows. Its "order" option takes a commission for six sacks of lake amber that cannot be met.
- No early ending is honest: the chain is the stay at Truso.
- RECOMMEND: leave the whole chain out.

## dirham_hoard (14 cards)
- Starts: s01_dying_gute, harbour, dorestad or ripa (or sliesthorp, birka), "year >= 805". Only the last year.
- s02_ashes (scheduled "in 3 at here") can happen. s03_rune_stick (crew, at sea) can happen if the stick was kept.
- Blocked: s04_beach, "port.id = paviken". Paviken (Gotland) is not one of the twelve.
- After it: every later step and all five endings are at paviken.
- The player is left with an oath to a dead man, his ashes and a rune stick, for good.
- RECOMMEND: leave the whole chain out. It opens only in 805, so little is lost.

## house_new_ship (14 cards)
- Starts: s01_wright, harbour, dorestad, year < 833, 2 visits, silver >= 240. Sets ch_house_new_ship_1.
- Blocked: s02_oak (file line 48), "port.id = colonia". Colonia is not one of the twelve. Only s02 sets
  ch_house_new_ship_2, which s03_keel needs; without the keel nothing from s07 on runs.
- Also outside: s05_weavers and s06_sail at wijnaldum; s13_river (an ending) at colonia.
- Inside already: s04_iron at daventre; s03, s07, s08, s09, s10, s11, s12 all at dorestad.
- The sail is not asked for by any ending: s10_launch, s11_patched, s12_dane read flags 9, 7a or 7b, flawed, slip.
  They do not read 5 or 6. So the wijnaldum cards can stay shut and the ship is still launched.
- Smallest remedy: move s02_oak to daventre. The scene is a forest clearing with standing oaks and a steward,
  so it cannot stand on the Dorestad bank; woods above the IJssel can carry it, and s04_iron is there already.
  Take off its fourth option, "trunk" (the great oak made into a river ship): it sets ch_house_new_ship_2r and
  schedules s13_river at colonia, which would be a new trap.
- Then the endings at dorestad open: end_keel (s10), end_patched (s11), end_dane (s12), end_count (inside s09).
  s14_penny (812 to 818) stays shut; harmless.
- Check first: whether the first release does anything with unlock_ship_north_sea_merchant_ship and
  unlock_ship_norse_allround_ship. If a new ship does nothing in the release, leave the chain out instead.
- Check also: the s02 words for "Colonia" or the Rhine by name.
- RECOMMEND: move s02_oak to daventre, drop its "trunk" option. Second choice: leave out.

## lindisfarne_book (13 cards)
- Starts: s01_strip_of_gilt, harbour, ripa (or skiringssal, sliesthorp), August 793 to 803.
- s04_hole_in_the_page (crew) can happen after s01.
- s02_what_is_gone (lindisfarena) and s03_calves (eoforwic) are outside, but the chain does not need them:
  s05 reads "ch_lindisfarne_book_1 or ch_lindisfarne_book_2".
- Blocked: s05_box_of_skins (file line 146), "port.id = ogvaldsnes or port.id = skiringssal". Neither is one
  of the twelve. It is the only card that first sets ch_lindisfarne_book_aboard (the book in the hold).
  Every ending but end_left reads that flag or a flag that follows from it.
- One ending is already inside: end_alms at quentovic, 794 to April 804, needs only "aboard".
- Smallest remedy: move s05_box_of_skins to ripa (keep its months, 5 to 9). A raider selling church loot on
  the Ripa strand is the scene s01 already shows there.
- Then: buy or steal the book at ripa, s06_green_water and s07_square_sail at sea, end_alms at quentovic.
- Still dead after the move: the s05 options that set "seen" or "caught" lead to s08_asulfs_lot at sliesthorp;
  s09_low_water and end_home (lindisfarena), end_york (eoforwic), end_left (Norway). A player who only looks
  at the book, or is caught, gets no ending. If that is not acceptable, leave the chain out.
- RECOMMEND: move s05 to ripa. Second choice: leave out. The home ending waits for the later ports either way.

## riddle_trail (13 cards)
- Starts: s01_stick, harbour, dorestad, 796 to 805, more than 2 visits.
- s02_carter at ripa can happen. s03_fog_hail (sea, rate 0.05; file line 89) can happen; sets ch_riddle_trail_3.
  Note: s03 uses the verb "encounter", which card_census.py lists as dead (it does nothing today).
- Blocked: s04_ferryman, "port.id = sliesthorp". Not one of the twelve.
- After it: s06 to s12 are all at hleidr (Lejre). Only s13_end_mound is at dorestad, and it needs the mound at hleidr.
- Moving s04 does not help: s06 at hleidr is the next wall.
- Smallest remedy: one new ending card after s03.
  New card: harbour.chain_riddle_trail.end_east, dorestad, when ch_riddle_trail_3 and not ch_riddle_trail_4.
  The man who gave the stick hears the answers so far and says the next is asked beyond the Danes, where
  this ship does not go; he pays for the answers, or the player keeps the stick. Sets ch_riddle_trail_end_east.
- Honest because the trail does run on east; the card says so and stops. It must not set flag 4, so the
  rest of the chain can open in a later release.
- RECOMMEND: end early at s03_fog_hail with end_east at dorestad. Second choice: leave out.

## saga_rhine_frisia (14 cards)
- Starts: s01_lots, harbour, doccinga, 794 to 801.
- Blocked: s02_east, "port.id = emuthon". Emuthon is not one of the twelve.
- After it: s03 and s06 at wijnaldum, s08 and s09 at noviomagus, all outside. s04 (medemelacha), s05 (dorestad,
  802 on) and s07 (traiectum) are inside but each needs the flag of the step before.
- Three ports are missing (emuthon, wijnaldum, noviomagus), so no single move mends it. No step before s09
  is a fair place to stop: every ending needs the hearing at the palace.
- RECOMMEND: leave the whole chain out.

## saxon_exiles_804 (14 cards)
- Starts: s01_column, harbour, daventre (or diusburg, colonia), July 804 to the end of 805.
- Blocked: s02_sheep_house, "port.id = emuthon". Not one of the twelve. s01 schedules it "in 5 at emuthon".
- After it: s03, s05, s08, s10 at emuthon. s09_priest and s13_end_church are at daventre, and s06, s07 and
  s11_end_north at ripa, but they need the flags "took" or "taken", set only at emuthon or after it.
- The years too: s10 needs 806 on and s14_end_home 810 on. The chain starts 18 months before the release ends.
- s01 alone is a whole scene (the column of Saxons). It could stay as a single card if its schedule line and
  chain flags were taken off. That is a rewrite of s01, not one of the three remedies.
- RECOMMEND: leave the whole chain out.

## silver_road (14 cards)
- Starts: s01_faceless_coin, harbour, dorestad, 797 to 809, 3 visits. Sets ch_silver_road_1 and the shared
  flags sv_first_dirham and sv_moneyer_trusts.
- Blocked: s02_scale, "port.id = birka". Not one of the twelve.
- After it: birka, aldeigjuborg, paviken for every step but s09_moneyer and s13_end_ransom (dorestad), which
  need ch_silver_road_7 or "ransom", both set at aldeigjuborg.
- RECOMMEND: leave the whole chain out. Check whether a kept card reads sv_first_dirham or sv_moneyer_trusts;
  if one does, it loses its only source inside the release.

## wine_fleet (14 cards)
- Starts: s01_second_keel, harbour, dorestad, 796 to 802, 2 visits.
- Blocked: s02_tide_and_toll at rotomagus and s03_saints_day at parisius. Neither is one of the twelve.
  s03 needs only flag 1, so it is the true gate; s03, s04, s05 (all parisius) lead to flag 5, which every
  later card reads.
- Inside but cut off: s08 and s09 (dorestad, lundenwic, quentovic), s10_reckoning (dorestad), end_abbey (lundenwic).
- A move would take three cards (s03, s04, s05), and the Saint Denis fair cannot honestly stand anywhere else.
- RECOMMEND: leave the whole chain out.

## Not checked
- The words files (Data/writing/chain_*.json): a moved card needs its words read for the old port name.
- Whether the dead verbs (passenger, learn, tongue, encounter) break a step that is otherwise open.
- Whether ship unlock flags do anything in the first release (decides house_new_ship).
- card_census.py was not rerun. Nothing under Assets was changed.
