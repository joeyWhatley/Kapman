# Calendar, first season: 1 May to 29 September 793

Source: `Assets/Kapman/Game/Data/story/calendar_793.json` only (read 7 Oct 2026; nothing changed in the project). All dates Julian. Nothing here is added to what the file holds, except the weekdays, which are reckoned as shown, and a few lines marked "reckoned here".

## How the weekdays were worked out

- 793 is not a leap year in the Julian calendar (793 mod 4 = 1), so the months run 31, 28, 31, 30, 31, 30, 31, 31, 30.
- Your sum holds: **1 January 793 was a Tuesday.** Two checks from the data's own anchors:
  - the data gives 8 June 793 (Lindisfarne) as a Saturday. 8 June is 158 days after 1 January (31+28+31+30+31+7). 158 mod 7 = 4. Saturday less 4 days = Tuesday.
  - the data gives Easter 793 as 7 April and says every Easter was checked to be a Sunday. 7 April is 96 days after 1 January. 96 mod 7 = 5. Sunday less 5 = Tuesday.
- So 1 May 793 (120 days after 1 January, 120 mod 7 = 1) was a **Wednesday**; the data's own note on Philip and James says the same ("a Wednesday").
- Further agreement with the data: Rogation from 13 May comes out a Monday, Ascension 16 May a Thursday, Pentecost 26 May a Sunday, and the Day of Atonement 21 September a Saturday (the data says "Saturday 21 September").
- First of each month: 1 May Wed, 1 June Sat, 1 July Mon, 1 Aug Thu, 1 Sept Sun.

## Marks

- **(?)** = the data itself marks the date, rule or source as not checked.
- **attested** is the data's own grade: period / later_law (written after 805, older custom) / later ("it is said") / computed (by rule, no record) / game (the game's own rule).
- "x" is the data's starting guess for the price move (above 1 dearer, below 1 cheaper). Goods are ids of goods_793.json.
- Whole file: the data calls itself "a draft for the user to overwrite".

## Every week (rules that bite on every such day in the season)

| Weekday | What | Faith | Where | Shuts / forbids / makes dear | Attested, unsure |
|---|---|---|---|---|---|
| Sunday | The Lord's day (from Saturday dusk in Kent by Wihtred's law; Sunday dawn elsewhere) | christian, christian_irish, prime_signed | port.faith = christian | May not: load or unload; sell on the open market (a sin everywhere; forbidden by the king in Francia only from 809); cart goods except food, a body for burial, or for the host; bring a suit (no court sits). Bread, ale and fish sell on Saturday; no hands can be hired till Monday. Penalties: Frisia a fine in solidi, heavier beyond the Lauwers; Kent a free man pays his healsfang, a lord 80 sceattas for a servant set to work; Wessex a slave made to work goes free. | period. UNSURE: the Frisian sums "12 and 4 solidi (?)"; the 809 capitulary reference (?) |
| Friday | Friday fast | christian, christian_irish | port.faith = christian | May not eat meat (unless a feast falls on it). Dear: herring, dried_fish, eels, x 1.05 | later_law (in English law only from 1008) |
| Wednesday | Wednesday, the first fast | christian_irish | port.people = irish | May not eat meat | period. UNSURE: the Rule of Tallaght source (?) |
| Saturday (Friday dusk to Saturday dusk) | The Sabbath | jewish | everywhere | May not: buy, sell, weigh or count silver; load, unload or carry a burden ashore; kindle a fire; put to sea in the three days before it unless the voyage is a duty; walk beyond the town's edge. May: stay aboard a ship already under way and let a crew of other faiths work her. A port whose market day is Saturday is closed to a Jewish merchant. | period. UNSURE: the three-days-before-sailing source (Talmud, Shabbat 19a) (?) |
| Saturday | Washing day | old_gods, prime_signed | port.people = danish or norse | Forbids nothing. A wash, comb and clean shirt before a hall is worth a pip of Name that day (the game's rule) | later (13th-century chronicle: "it is said") |
| any | The moon and the tide | all five | everywhere | Forbids nothing. Springs at full and new moon; high water at a different hour in each port | period |

Saturdays in the season: 4, 11, 18, 25 May; 1, 8, 15, 22, 29 June; 6, 13, 20, 27 July; 3, 10, 17, 24, 31 Aug; 7, 14, 21, 28 Sept. Sundays are the day after each; Fridays the day before; Wednesdays begin 1 May.

## The days, in order

| Date 793 | Weekday | What it is | Faith | Where | Shuts / forbids / makes dear | Attested, unsure |
|---|---|---|---|---|---|---|
| 1 May | Wed | Philip and James. The game begins on this feast. A feast with meat, because it lies in Easter time. | christian | port.faith = christian | May not work. Dear: ale, cattle, x 1.1 | later_law (V Aethelred 14, 1008) |
| 1 to 26 May | - | Sailing year: "doubtful sailing" (10 March to 26 May) | all | sea | - | Vegetius IV.39 |
| April and May | - | "The hungry weeks: grain is dearest in April and May" (note on Easter) | all | - | Grain dear (no figure given) | note only |
| 13, 14, 15 May | Mon, Tue, Wed | The Rogation days: the fields are walked with the cross and the relics | christian (not christian_irish) | port.faith = christian | May not eat meat; may not work until the procession is home | period |
| 16 May | Thu | The Ascension. "The first crossings to England are made about now." | christian, christian_irish | port.faith = christian | May not work or trade | later_law (Mainz 813) |
| 22 and 23 May (from dusk on Tue 21 May) | Wed, Thu | Shavuot, 2 days | jewish | everywhere | May not trade or load | computed. UNSURE: every Jewish day "may be one day off what a community of 793 kept (?)" |
| 25 May (to 12 Aug) | Sat | The summer market at Ripa opens (80 days). Outside these weeks the landing is half empty. | all | ripa | Forbids nothing. Dear: rhenish_wine, frisian_cloth, quernstones. Cheap: amber, combs, glass_beads, cattle (no x given) | computed: the season is from the digs; "the days are the game's" |
| 26 May | Sun | Pentecost (Whitsun), kept 8 days: 26 May to 2 June (Sun to Sun). Second day for baptisms. Wool is shorn about now and the long voyages start. | christian, christian_irish | port.faith = christian | May not work or trade through the week. Dear: fine_linen. Cheap: raw_wool, fleeces. x 1.1 | later_law (Mainz 813) |
| 26 May | Sun | Saint Augustine of Canterbury: a crowd at the abbey a mile from the hythe. Falls on Pentecost Sunday in 793 (reckoned here). | christian | fordwic, sarre | May not work. Dear: wax, ale, x 1.15 | period |
| 27 May | Mon | Sailing year: "safe sailing" begins (27 May to 14 Sept) | all | sea | - | Vegetius IV.39 |
| after Pentecost | - | The summer Lent of the Irish (one of the three Lents), kept by the stricter Irish houses. No dates given. | christian_irish | - | A fast; rule not spelled out | UNSURE: source (the Tallaght documents) (?) |
| 3 to 6 June | Mon to Thu | The gathering at Boniface's church: pilgrims by boat, pedlars and the lord's men in one yard (feast market) | - | doccinga | Nothing forbidden in the entry | period |
| 5 June | Wed | Saint Boniface and his companions (killed at Doccinga 5 June 754). "In 793 it fell three days before Lindisfarne." | christian | doccinga, traiectum, medemelacha | May not work at Doccinga. Dear: ale, wax, ships_bread, x 1.2 | period |
| 8 June | Sat | Lindisfarne (named only as a weekday anchor and in the Boniface note; the event is in chronicle_793.json, not here) | - | - | - | - |
| "second week of June" | a Wed, Fri and Sat | Ember days (the fasts of the four seasons) | christian | - | A fast. The data gives no day for 793 and says "the weeks were not the same everywhere"; Mainz 813 fixed the second week of June for the Frankish realm | rule is of 813, twenty years after; NO DATE IN THE DATA |
| 23 June | Sun | The eve of Saint John | christian | port.faith = christian | May not eat meat. Falls on a Sunday in 793, so the Sunday rules hold too (reckoned here) | later_law. UNSURE: "fires on the eve are well known later and likely now (?)" |
| 24 June | Mon | Saint John the Baptist (midsummer): "the great feast of the sailing months: every town full and off its guard" | christian, christian_irish | port.faith = christian | May not work or trade. Dear: ale, cattle, x 1.1 | later_law (Mainz 813) |
| Monday after Saint John | Mon | The count's court (mallus): debts and thefts heard, oaths sworn, fines set; not on a Sunday or a feast | - | dorestad; also walichrum, witla, medemelacha, daventre, quentovic | A court day. Saint John is itself a Monday in 793, so the Monday after is 1 July (reckoned here; the data gives no date) | period for the court. UNSURE: number of sittings (?), source (?); "the days are the game's" |
| 29 June | Sat | Saints Peter and Paul. Patronal day of Saint Paul's at Lundenwic: a crowd there on the days around it. | christian, christian_irish | port.faith = christian | May not work or trade. With Sunday 30 June that is two shut days running (reckoned here) | later_law |
| 4 July | Thu | Saint Martin in summer: a gathering at Traiectum | christian | traiectum, dorestad | Forbids nothing. Dear: wax, x 1.1 | period. UNSURE: source "the Frankish calendars (?)" |
| 13 July | Sat | Saint Mildrith of Thanet: the day of the minster whose abbess has the toll on her own ships at Sarre and Fordwic | christian | sarre | May not work on the abbess's land. Dear: ale, wax, x 1.15 | period. UNSURE: the charters "S 29 of 763 or 764 (?); S 86 and others (?)"; the legend is written down later |
| 1 Aug | Thu | Lammas (loaf-mass; Saint Peter's Chains). Harvest has begun: grain is about to fall and no hands can be hired. | christian | port.people = english | Forbids nothing. Cheap: wheat, barley, rye, x 0.9 | later_law. UNSURE: "the English name is written from the 9th century, so likely in 793 (?)" |
| 12 Aug | Mon | Last day of the summer market at Ripa | all | ripa | After it the landing is half empty | computed; the days are the game's |
| 15 Aug | Thu | The Assumption of Saint Mary. Harvest in full swing; honey is taken. | christian, christian_irish | port.faith = christian | May not work or trade. Cheap: honey, x 0.9 | later_law |
| 12 and 13 Sept (from dusk on Wed 11 Sept) | Thu, Fri | The Jewish new year, 2 days. The Sabbath follows on Sat 14 Sept: three days without trade for a Jewish merchant (reckoned here) | jewish | everywhere | May not trade or load | computed. UNSURE: may be one day off (?) |
| 14 Sept | Sat | Last day of "safe sailing" | all | sea | - | Vegetius IV.39 |
| 15 Sept | Sun | "Doubtful sailing" begins (15 Sept to 10 Nov) | all | sea | - | Vegetius IV.39 |
| "third week of September" | a Wed, Fri and Sat | Ember days | christian | - | A fast. No day given for 793 | rule is of 813; NO DATE IN THE DATA |
| 21 Sept (from dusk on Fri 20 Sept) | Sat | The Day of Atonement. The data: "In 793 it fell on Saturday 21 September, eight days before the count's Michaelmas reckoning." | jewish | everywhere | May not eat or drink from dusk to dusk; may not trade, load or sail by choice | computed. UNSURE: may be one day off (?) |
| 26 Sept (from dusk on Wed 25 Sept), 8 days to 3 Oct | Thu | Booths begins. Meals are eaten in a booth of branches: for a moored boat, a booth on the bank. | jewish | everywhere | May not trade or load on the first two and last two days: 26 and 27 Sept (Thu, Fri) fall in the season; the last two (2 and 3 Oct) are after it (reckoned here) | computed. UNSURE: may be one day off (?) |
| 29 Sept | Sun | Michaelmas: the reckoning. The day the house's debt falls due. The harvest is sold; the autumn gales begin. A count who is owed sits to be paid on a feast, and may not hear a suit that day. Falls on a Sunday in 793 (reckoned here). | christian, christian_irish | port.faith = christian | May not work or trade. Cheap: wheat, rye, barley, oats, x 0.85 | **game**: "that Dorestat reckoned debts on it in 793 is not recorded". The feast itself is from Mainz 813 |

## In the season with no day, or expressly not in it

- **The king's assembly and the host, 793: none.** "The king is at Regensburg all year, digging the canal between Altmuhl and Rednitz." So the dear oats, horses, bar_iron and salt_pork along the host's road do not apply this first summer. Attested: period.
- **Heathen days: none falls in the season.** The feast at the start of summer is "mid April; the game's day for Ripa: 14 April (?)" and the winter feast "mid October; ... 14 October (?)", before and after it. "The town's feast for its god" (ripa or sliesthorp): when "not known", attested later, and shown at Ripa in the 790s "it is a guess (?)". "The darkened moon": no eclipse listed for 793 (the first is 28 March 796).
- **Horse flesh**: never eaten by Christians; on the table at every heathen feast. A standing rule, all year.
- **The fair of Saint-Denis** (from 9 October, 28 days; heard at quentovic, lundenwic, dorestad) and **the count's court on the Monday after Saint Remigius** begin after 29 September. The data does not say when rumours of the fair start.
- **No fair with a charter and a date is known at any of the twelve ports**; no thing of the Frisians, none described at Ripa (the data's "what is not there").

## Things to look at (found while reading, not changed)

1. The game's first day, Wed 1 May, is a feast on which a Christian may not work, and an Irish Wednesday fast.
2. Ember days have no dates for 793 in the data: the June and September weeks cannot be shown on a day until someone sets them.
3. The count's court "Monday after Saint John" is ambiguous in 793 because Saint John is itself a Monday (a feast, on which the court may not sit): 1 July is the reading used above.
4. Michaelmas 793 is a Sunday: the debt falls due on a day when no court sits and no one may trade or load.
5. The fixed feasts grade mostly as later_law (Mainz 813, eight years after this life ends), by the data's own word.
6. Not read for this check: Code/Runtime/Levels/Feasts.cs (the engine's own feast list), fairs_793.json, seasons_793.json, chronicle_793.json. Whether they agree with this calendar is unchecked.
