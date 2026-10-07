# Wording fixes: sea, crew, long and chain cards

Made 7 October 2026. Checked against Design/content/lore/period_words.md part (b).
Scope: keys beginning sea, crew, long, chain_ (and their art. keys) in Data/writing.json and Data/writing/*.json.
A script listed every line holding a listed word (665 hits; each key stands in both files, counted once).
Part 1 is judged by reading. Part 2 is the raw list for the words there was no time to read: NOT judged.
Only the sentence with the fault is shown, not the whole entry. Nothing in Assets was changed.

## Part 1. Real faults (judged)

| Key | As it stands | Mended | Rule broken |
|---|---|---|---|
| chain_amber_road.x_wulfstan.title | Lands to port, lands to starboard | Lands on the back side, lands on the steering side | port and starboard |
| chain_amber_road.x_wulfstan.seven | The Wends' land to starboard. | The Wends' land on the steering side. | port and starboard |
| chain_amber_road.x_wulfstan.seven | To port Langaland, Laland, Falster and Scania, and all those belong to Denmark. | On the back side Langaland, Laland, Falster and Scania, and all those belong to Denmark. | port and starboard (Denmark is right here: Wulfstan's own word) |
| chain_amber_road.x_wulfstan.lands | And the Wends still to starboard, as far as the mouth of the Wisle. | And the Wends still on the steering side, as far as the mouth of the Wisle. | port and starboard |
| chain_amber_road.x_wulfstan.clerk | ...lands to port and lands to starboard, like a man telling knots on a rope. | ...lands on the back side and lands on the steering side, like a man telling knots on a rope. | port and starboard |
| chain_walrus_ivory.s14.out_copy | ...the lands to starboard and the lands to port, and blots it with sand. | ...the lands on the steering side and the lands on the back side, and blots it with sand. | port and starboard |
| chain_walrus_ivory.s07.intention | Two days out, with Denmark to port, a long ship comes out from behind an island under oars. | Two days out, with Denmark on the back side, a long ship comes out from behind an island under oars. | port and starboard |
| chain_saxon_exiles_804.s04.out_heave_to_success | The captain waves you on. | The master of the guard boat waves you on. | captain |
| chain_saxon_exiles_804.s04.out_heave_to_failure | The captain fines you forty pennies on the spot and has your name. | The master of the guard boat fines you forty pennies on the spot and has your name. | captain |
| chain_sword_smuggle.s05.smith_watch_boat | They keep a boat at the river mouth now, with a captain who likes to hear ballast move. | They keep a boat at the river mouth now, with a count's man in her who likes to hear ballast move. | captain |
| chain_sword_smuggle.s06.out_search | The captain says it is a dull thing to search an honest boat and wishes you a fair wind. | The count's man says it is a dull thing to search an honest boat and wishes you a fair wind. | captain |
| chain_sword_smuggle.s06.out_among_wine_failure | The captain takes the blades, sixty pennies for his trouble, and the look of your boat... | The count's man takes the blades, sixty pennies for his trouble, and the look of your boat... | captain |
| chain_sword_smuggle.s06.out_run_failure | ...and the captain tells you the day you are to stand before the count. | ...and the count's man tells you the day you are to stand before the count. | captain |
| chain_sword_smuggle.s06.out_over | The captain looks at the gap, and at you, and can make nothing of either. | The count's man looks at the gap, and at you, and can make nothing of either. | captain |
| chain_dirham_hoard.s07.rower_ask | What rides under your deck boards? | What rides under your boards? | deck |
| chain_dirham_hoard.s07.opt_show | Heave to, lift the deck boards, and tell him who has the silver | Heave to, lift the boards, and tell him who has the silver | deck |
| chain_house_new_ship.s10.old_boat | From up here she looks like something the new ship might carry on deck. | From up here she looks like something the new ship might carry amidships. | deck |
| chain_silver_road.news.end_slaver | A boat of Dorestad went east with people under her deck and came home a ship. | A boat of Dorestad went east with people under her tent and came home a ship. | deck |
| chain_walrus_ivory.s06.out_king | Ohthere watches you from his own deck and says nothing at all. | Ohthere watches you from his own ship and says nothing at all. | deck |
| art.sea.sail.seam | Looking up at the square sail from the deck with the sun behind it... | Looking up at the square sail from the boards with the sun behind it... | deck (picture note, not shown as a line) |
| art.crew.chain_house_marriage.s04_crew_counsel | ...the steersman at the side rudder looks straight ahead... | ...the steersman at the steering oar looks straight ahead... | rudder (picture note) |
| art.crew.chain_sword_smuggle.s03_crews_word | ...the steersman at the side rudder speaking without turning his head... | ...the steersman at the steering oar speaking without turning his head... | rudder (picture note) |
| art.sea.quarrel.oar | ...each with one hand clamped on the tiller of the side rudder... | ...each with one hand clamped on the tiller of the steering oar... | rudder (picture note) |
| chain_barrow.s02.intention | The Deben opens two hours up the coast. | The Deben opens a short sail up the coast. | hour as sixty minutes |
| chain_relic_theft.s06.steersman_tide | Three hours of ebb yet. | Half the ebb still to run. | hour as sixty minutes |
| chain_storm_vow.s06.boatman_tide | She is out for three hours yet. | She is out till past noon. | hour as sixty minutes (pick the time of day that fits the card) |
| chain_saga_northumbria_irish_sea.s08.out_sail | At the bottom of the ebb the sea is three miles off and the sand lies like a floor. | At the bottom of the ebb the sea is a long walk off and the sand lies like a floor. | miles |
| chain_year_1066.s08.intention | His ships lie ten miles down the Ouse. | His ships lie half a day's row down the Ouse. | miles on the water |
| chain_saxon_exiles_804.s04.out_back | You go about while there is still half a mile between you... | You go about while there is still a long bowshot between you... | miles on the water |
| chain_silver_road.s04.intention | ...her steering oar gone and a lee shore half a mile under her. | ...her steering oar gone and a lee shore close under her. | miles on the water |
| chain_storm_vow.s06.intention | The island lies a mile off across wet sand that shines like a shield. | The island lies a short walk off across wet sand that shines like a shield. | miles |
| chain_the_bell.s03.size | Two men lift it, one man rings it, and it is heard over a mile of water. | Two men lift it, one man rings it, and it is heard far over the water. | miles on the water |
| chain_cnut_1016.s05.out_speak_success | You ask the toll man whether he would sell his own dinner. | You ask the toll man whether he would sell his own meat. | dinner (two meals only) |
| chain_dorestad_834.s06.rival_debt | I do not intend it to last until dinner. | I do not intend it to last until the night-meal. | dinner |
| chain_house_debt.s08.hawk | He wants one for the king's fist more than he wants his dinner. | He wants one for the king's fist more than he wants his meat. | dinner |
| chain_pilots_secret.s05.out_take | ...as a man watches his own dinner carried past. | ...as a man watches his own meat carried past. | dinner |
| chain_coast_watch_810.end_fleet.intention | Shipwrights, monks, soldiers and the count's men stand about... | Shipwrights, monks, men of the levy and the count's men stand about... | soldier |
| chain_coast_watch_810.s02.farmer_seventh | I am the seventh part of a soldier, and it is hay time. | I am the seventh part of a man of the host, and it is hay time. | soldier |
| chain_coast_watch_810.s08.out_carry | ...with sacks to the gunwale and a soldier sitting on them. | ...with sacks to the gunwale and one of the count's men sitting on them. | soldier |
| chain_coast_watch_810.s09.out_truth | Men at hay time who are each one seventh of a soldier. | Men at hay time who are each one seventh of a man of the host. | soldier |
| chain_wreckers.s14.trade | I am a soldier. | I am a lord's man. | soldier (check the speaker; "warrior" if he has no lord) |
| chain_black_stones.s05.out_run_failure | The officer says the abbot will have it by morning... | The king's man says the abbot will have it by morning... | officer |
| chain_black_stones.end_early.out_charter | ...and both kings' officers know your name. | ...and both kings' men know your name. | officer |
| chain_house_debt.s02.intention | ...the best dam head in Dorestad and a gift for every officer... | ...the best dam head in Dorestad and a gift for every king's man... | officer |
| chain_house_debt.s07.martin | ...owe the king's officers nothing: no toll, no lodging... | ...owe the king's men nothing: no toll, no lodging... | officer |
| chain_house_debt.s13.house | No officer of the king lodges in it... | No man of the king lodges in it... | officer |
| chain_saxon_exiles_804.s01.intention | An officer of the host walks along the boats, asking for hulls. | A count's man of the host walks along the boats, asking for hulls. | officer |
| chain_saxon_exiles_804.s01.opt_hire | Take the officer's hire and carry a boatload over | Take the count's man's hire and carry a boatload over | officer |
| chain_saxon_exiles_804.s01.out_hire | The officer counts them out on the far bank... | The count's man counts them out on the far bank... | officer |
| chain_saxon_exiles_804.s01.out_watch | The officer shrugs and goes on to the next boat. | The count's man shrugs and goes on to the next boat. | officer |
| chain_sword_smuggle.s02.tollman_law_805 | The other half between the officer and the man who finds. | The other half between the king's man and the man who finds. | officer |
| chain_sword_smuggle.s09.missus_share_805 | Of the other half, a part to the officer and a part to the man who finds. | Of the other half, a part to the king's man and a part to the man who finds. | officer |
| chain_sword_smuggle.s11.missus_share_805 | Of the rest, the officer's part and the finder's part. | Of the rest, the king's man's part and the finder's part. | officer |
| chain_year_1066.s04.out_count_failure | A sergeant finds you pacing out a slipway. | One of the duke's men finds you pacing out a slipway. | sergeant (not yet) |
| chain_year_1066.s07.intention | A sergeant sends you round to join it, loaded or empty. | One of the duke's men sends you round to join it, loaded or empty. | sergeant |
| chain_year_1066.s07.out_slip_failure | ...with a sergeant sitting on your sea chest. | ...with one of the duke's men sitting on your sea chest. | sergeant |
| chain_year_1066.s09.out_supply | The sergeant says again that you should come to London at midwinter. | The duke's man says again that you should come to London at midwinter. | sergeant |
| chain_york_954.s06.intention | ...the archbishop has been taken south to the English king's prison. | ...the archbishop has been taken south and is held in bonds by the English king. | prison |
| chain_york_954.s06.host_changed | And the archbishop, who had a hand in making both, in the English king's prison. | And the archbishop, who had a hand in making both, in the English king's bonds. | prison |
| chain_york_954.end_stainmore.host_wulfstan | The archbishop is out of prison and has a bishop's seat again... | The archbishop is out of his bonds and has a bishop's seat again... | prison |
| chain_walrus_ivory.s12.intention | The archbishop's carver works in a shed against the cathedral wall. | The archbishop's carver works in a shed against the wall of the bishop's church. | cathedral |
| chain_saxon_exiles_804.s09.priest_before_805 | I am a priest with a wide parish and a king's letter, no more. | I am a priest with a wide land to serve and a king's letter, no more. | parish |
| chain_saxon_exiles_804.s10.out_pay | He does not say goodbye. | He does not say farewell. | goodbye |
| chain_cnut_1016.s08.clerk_asks | Slowly, please. | Slowly, I ask you. | please |
| chain_lindisfarne_book.end_york.scribe_hand | I am sorry. | It grieves me. | sorry (if he means his own fault: "Forgive me.") |
| chain_wine_fleet.s03.opt_judgment | Say aloud, before the abbey's man, that King Pippin struck this tax down | Say aloud, before the abbey's man, that King Pippin struck this toll down | tax |
| chain_year_1066.end_norman.clerk_list | He has also laid a tax on the whole land, to pay it with. | He has also laid a tribute on the whole land, to pay it with. | tax |
| chain_amber_road.s07.brew_people | ...since my husband died, and he drinks the profit. | ...since my husband died, and he drinks the gain. | profit |
| chain_saga_england_south_east.s07.seller_price | I tell you my profit to your face. | I tell you my gain to your face. | profit |
| chain_saga_england_south_east.s08.opt_price | Name what you paid and a fair profit on it | Name what you paid and a fair gain on it | profit |
| chain_dorestad_834.end_wall.out_seat | The shares come back with interest, twelve hundred pennies... | The shares come back with something over, twelve hundred pennies... | interest (usury) |
| long.years_london.emperors_men.l1 | Flanders, Ponthieu, Normandy and France show their goods and pay. | Flanders, Ponthieu, Normandy and Francia show their goods and pay. | France |
| chain_year_1066.s02.out_willing | He goes on to Scotland with twelve small ships. | He goes on to the Scots' land with twelve small ships. | Scotland |
| chain_reric_808.s06.intention | A shipman in from the Baltic has news. | A shipman in from the East Sea has news. | Baltic (first used c. 1075) |
| chain_maldon_991.s08.toke_offer | ...a foreigner needs a man to answer for him... | ...an outlander needs a man to answer for him... | foreigner |
| chain_sword_smuggle.s12.chronicle | ...the first smith there was a foreigner who never learned to like the ale. | ...the first smith there was an outlander who never learned to like the ale. | foreigner |
| chain_year_1066.s08.thegn_ask | You are a foreigner, and you have been among them. | You are a stranger here, and you have been among them. | foreigner |
| chain_lost_brother.s05.out_ask | He wishes you luck with your Wend. | He wishes you good fortune with your Wend. | luck (only as a thing a man has) |
| chain_saga_england_south_east.s09.opt_luck | Wish him luck, and ask Ceolmund what sells | Wish him good fortune, and ask Ceolmund what sells | luck |
| chain_saga_england_south_east.s09.out_luck | He thanks you for the luck as a man does who has asked for something else. | He thanks you for the wish as a man does who has asked for something else. | luck |
| chain_house_new_ship.s12.out_with_dane | ...and gives you a riddle of his people for luck. | ...and gives you a riddle of his people for good fortune. | luck |
| chain_house_rival.end_partner.out_agreement | ...a stick with a verse cut on it, for luck. | ...a stick with a verse cut on it, for good fortune. | luck |
| chain_sword_smuggle.s10.out_plot | ...lays three marten skins on it for luck. | ...lays three marten skins on it for good fortune. | luck |
| crew.cargo.book.stern.out | ...and by luck the bilge stays low. | ...and by God's favour the bilge stays low. | luck |
| sea.porpoise.roll.luck | Take them as luck, and watch (sure) | Take them as a good sign, and watch (sure) | luck |
| sea.porpoise.roll.hunt.success | By luck more than skill the hook takes the last of them behind the fin. | By hap more than skill the hook takes the last of them behind the fin. | luck |
| sea.rat.bread.say.one | In a month, with luck, still one rat. | In a month, with good fortune, still one rat. | luck |
| sea.raven.yard.say.highone | A raven on a ship is the best luck there is. | A raven on a ship is the best sign there is. | luck |
| chain_cnut_1016.s13.reeve_sons | ...they will have half of it each, if they are lucky. | ...they will have half of it each, if fortune holds. | lucky |
| crew.song.26.lead | A man is lucky, and grows proud, and begins to hoard... | A man has good fortune, and grows proud, and begins to hoard... | lucky |
| long.rhine.burned_882.o.salt | Count yourself lucky to trade on salt water | Count it good fortune to trade on salt water | lucky |
| chain_amber_road.end_shut.caught | ...a thief at a burning is unlucky to kill. | ...it is an ill thing to kill a thief at a burning. | lucky (weak case) |
| chain_lindisfarne_book.end_york.scribe_hand | The island's hand, sixty or seventy years old. | The island's hand, sixty or seventy winters old. | age in years |
| chain_riddle_trail.s08.setter_why | ...the place is seventy years old and coughs too. | ...the place has seventy winters and coughs too. | age in years |

## Read and found fine (leave)

- honestly (8): always "in an honest way", never the filler word.
- sorry (17 of 18): "is sorry", "a sorry one" = regretful or wretched, not an apology.
- please (8 of 9): "as they please", "to please him".
- knots (15): knots in cord, never speed. wheel (8): cheese, grindstone, halo. margin (2): a book's margin.
  shares (7): shares in a ship or cargo. loan (2): a lent tool. boss: shield boss. hell (2): the real place.
- hours (8 of 11): the hours of prayer, or the tide "hours" a pilot keeps (chain_pilots_secret.s09: Bede's reckoning; keep).
- mile(s) on land in chain_amber_road (s06, s09, x_wulfstan.ride): Wulfstan's own word for the horse race. Keep.
- Denmark (10): all c. 890 or later (Wulfstan, Ohthere, Cnut). Keep. Sweden and French, in 1016 and 1066: keep.
- religion (2), Christianity (1): inside real quoted letters and law (Charles to Offa; the Saxon capitulary). Keep.
- pirates (4): the Frankish annals' own word (piratae), in clerks' and annal lines. Keep; in a boatman's mouth
  (chain_rorik_850.s08.merchant_where) "raiders" would be plainer.
- foreigners (chain_saga_northumbria_irish_sea.s05.abbot_shrine): an Irish abbot's Gaill. Keep.
- supper (7): not on the list by name, but there are only two meals: "night-meal" is the plain word if wanted.
- rights (chain_tiel_1020.end_bishop.bishop_count, "the count's rights over that land"): a legal grant; "the count's dues" is safer.
- Baltic in art. keys (4) and "the Channel" in art.sea.chain_black_stones.s05_kings_boat: picture notes, never shown as lines.

## Part 2. NOT judged: hits for the words there was no time to read

Each needs a look. Likely faults: skipper (say steersman, shipmaster), helm (the steering oar), the hold (amidships among
the cargo), England before c. 1000 (the English, or the kingdom), business (trade, dealings), money (silver, pennies),
wage(s) (hire, keep, a share), company as a firm (the house, a partnership), country as a state (land, the king's land),
judge as an office (the reeve, the lawspeaker), freedom as an idea (free, law-worthy), hour as sixty minutes.
"mate" is wrong only as a word of address; "the mate" as a man of the crew is the game's own name.
Format: word | key | sentence.

wages | crew.riddle.fog.dane | 'Who is the huge one, who passes over the earth, swallowing water and woods; he fears the wind, but no man, and wages war on the sun?'
freedom | chain_coast_watch_810.end_fleet.opt_frisian | Tell the envoy your idea: sworn Frisian shipmen on the watch, paid in toll freedom
freedom | chain_coast_watch_810.end_fleet.out_frisian | Swear the Frisian keels to the watch, forty days a summer each, in turns; pay them with freedom from the toll, which costs the emperor nothing he has in his hand.
freedom | chain_coast_watch_810.end_eider.opt_gate | Stand witness for both sides, and ask the freedom of the king's gate for your boat
freedom | chain_coast_watch_810.end_eider.out_clause | You have four hundred pennies, six timbers of marten from the king's own store, the freedom of the gate, and a guest's right in any Danish harbour: the first merchant the peace was written for.
freedom | chain_house_debt.s11.charter | Ask him to carry your asking to the king: the freedom of the toll
freedom | chain_kentish_revolt_796.end_spared.out_little | He gives you the freedom of Sarre and his protection besides, and says he asked you to be sensible, not holy.
freedom | chain_saga_england_south_east.end_fallen.out_carried | The steward goes away and comes back with the lady's answer: eleven hundred pennies, the cup, and the freedom of the toll, and a message that she does not argue with merchants about what things are worth.
freedom | chain_saga_england_south_east.end_minster.opt_ship | Take the steering oar of the minster's ship, under her freedom of toll
freedom | chain_sword_smuggle.s12.out_blade | You have sold a man his freedom and a king his swords, and it is hard to say which of them paid more.
freedom | chain_tiel_1020.end_charter.opt_silver | Take silver for your part, keep the toll freedom, and stay your own man
freedom | chain_york_954.end_stainmore.out_blood | Sixteen hundred pennies, and the earl's seal on your freedom from the river toll.
freedom | chain_york_954.end_dublin.opt_plot | Ask the king for ground at York and freedom from his toll there
freedom | long.charles.dead.l2 | Every grant and every freedom is to be shown again and sealed again in the new name.
freedom | long.charles.dead.l3 | Your freedom from the toll at Dorestad was given in the old emperor's name.
freedom | long.charles.dead.o.confirm | Have your freedom sealed again in the new emperor's name (60 d to his clerk)
freedom | long.tiel.dorestads_freedom.title | The freedom that was Dorestad's
freedom | long.tiel.dorestads_freedom.intention | This year King Zwentibold confirmed to the church of Traiectum that its men who live at Daventre and at Tiel shall have the same freedom from the king's toll that they had at Dorestad.
freedom | long.tiel.dorestads_freedom.l2 | If your house is the bishop's man, the freedom is yours where the bishop's men now sit.
freedom | long.tiel.dorestads_freedom.o.old | Show the old Dorestad freedom, and have it written over to Daventre
company | chain_cnut_1016.s06.intention | They are loading in one company, and there is room in it.
company | chain_cnut_1016.s06.opt_company | Go down in the Tiel men's company and pay what the count's men ask: thirty pennies
company | chain_cnut_1016.s06.out_company | The count's men take thirty pennies from you and more from the wine ships, and wave the company through by noon.
company | chain_dirham_hoard.s13.hand_dog | I have been found in better company.
company | chain_house_new_ship.s10.riddle | Long and swift I go, daughter of a fair forest, with a great company crowded close about me.
company | chain_kentish_revolt_796.s07.out_new_coin | Whether they are good company is another matter.
company | chain_lindisfarne_book.s03.out_letter | At the door he says, to the page and not to you, that the first leaf of John is where a liar's book and a true one part company.
company | chain_mission_north.s10.opt_company | Sail in company and carry only their stores
company | chain_mission_north.s11.out_old | They are good company and they pour well.
company | chain_pilgrim_road.s04.host_company | A company leaves on Monday for Saint-Riquier and Saint-Denis.
company | chain_pilgrim_road.s07.opt_send | Send forty pennies south by the next company, for her road home
company | chain_pilgrim_road.s07.out_send | The monk sews the forty pennies into a hem for the next company going south and writes her name on a scrap to go with them.
company | chain_pilgrim_road.s07.out_ask | The priest left the company at Saint-Denis and went to the king.
company | chain_riddle_trail.s12.out_keep | "It has been in worse company.
company | chain_silver_road.s05.title | In company
company | chain_storm_vow.s05.out_both | The steersman crosses himself and looks at the water, and the hand, who has heard all of it, says the thunderer does not mind company; it is the other one who is jealous.
company | chain_tiel_1020.end_seat.out_cup | Your pound has come home with company: eight hundred pennies on the cloak.
company | chain_wine_fleet.s01.opt_own | Sail in his company, but for your own purse
company | crew.faith.radbod.side | Say that a king who goes to hell for the company is still in hell
company | crew.faith.radbod.side.out | The Dane says that company is most of what a man gets anywhere, and lets it drop.
money | chain_coast_watch_810.s02.out_stand | Gerwin pays you the count's ration money, twenty-four pennies, and says it is the first week all summer he has slept.
money | chain_great_army_865.s01.title | Money for peace
money | chain_great_army_865.s01.intention | The heathen army sits on Thanet, and the men of Kent have promised it money to stay there.
money | chain_great_army_865.s01.reeve_promise | The men of Kent have promised them money for peace.
money | chain_great_army_865.s01.out_pay | The money was promised and the burning was done anyway.
money | chain_great_army_865.s02.out_tell | You tell him: the peace in Kent, the money promised, the night ride after.
money | chain_great_army_865.s03.dealer_price | A grown man, forty pennies of your money.
money | chain_great_army_865.news.thanet | This year the heathen army sat on Thanet, and the men of Kent promised it money for peace; and under cover of the peace the army stole away by night and harried all the east of Kent.
money | chain_maldon_991.s03.intention | In the ale-house a grey man is singing how the ealdorman stood at the causeway, and the mass-priest is waiting for him to finish so he can ask you for money.
money | chain_relic_theft.s12.out_silver | Two thousand pennies, counted in tens on the tail of the cart, beside the box, with the white cloth turned back so that it should not be touched by money.
money | chain_riddle_trail.s04.out_pay | You have bought with money what the others earned, and it is worth what money is worth." He gives you the stick all the same.
money | chain_saga_england_south_east.end_minster.opt_carriage | Ask the carriage money for a night's work, and no more
money | chain_saga_northumbria_irish_sea.s05.out_shrine | 'It is all we have that a Saxon would call money.'
money | chain_tiel_1020.end_count.steward_words | His words have been worth more than his cheese money, my lord.
money | chain_wine_fleet.s04.partner_choose | I have lost money both ways.
money | chain_year_1066.s02.narr_haven | He took money and food on Wight and has burned his way east along the coast since.
money | crew.hire.second_son.man | Take him at a man's wage from the first day, with hand-money (2 d)
money | crew.hire.second_son.man.out | Two pennies of hand-money and a man's wage from the first stroke.
money | crew.winter.spring.send | Send his cousin back with hand-money and a day named (2 d)
money | crew.winter.spring.send.out | Hand-money taken is a hiring made.
money | long.tiel.sworn_merchants.l2 | They gather money in common for it, and feast on the great days as if drunkenness were a solemn rite.
money | art.long.tiel.sworn_merchants | A long bench of red-faced merchants in a timber hall at first light, drinking horns and a shared money chest on the table, one man swearing with his hand raised, a thin monk watching from the door.
wage | chain_amber_road.s07.brew_share | Not a wage.
wage | chain_house_new_ship.s01.out_think | He reckons it on his fingers: the earnest, the oak, the iron, the sail, his wage at the launch.
wage | chain_pilots_secret.end_boy.opt_keep | Take the marks as your wage, pilot for yourself this summer, and leave the boy to what the old man told him
wage | chain_riddle_trail.s05.out_press | There is your wage's worth." He gathers the sticks, gives them back to you, and takes the bow watch though it is not his turn.
wage | chain_sword_smuggle.s03.out_share | A share is a different thing from a wage, and the men sit differently on the thwarts after it.
wage | chain_wine_fleet.end_abbey.clerk_choice | If there is none, he may ask the abbot's writing for himself at the fair, or take a carrier's wage.
wage | chain_wine_fleet.end_abbey.opt_wage | Sell the wine, carry the silver back, and take a carrier's wage
wage | chain_wine_fleet.end_abbey.out_wage | You carry the silver back across the sea, short by nothing, and the cellarer counts out a carrier's wage, six hundred pennies, and shows you a dry corner of the storehouse that is yours to use.
wage | crew.pay.promise_due.pay.out | A word kept is remembered longer than a wage.
wage | crew.pay.wage_or_share.title | A wage or a share
wage | crew.pay.wage_or_share.intention | The mate asks whether the men might have a part of the venture instead of a wage.
wage | crew.pay.wage_or_share.bundle | Keep the wage, but each man may carry a bundle of his own to trade
wage | crew.pay.wage_or_share.wage | A wage is a wage
wage | crew.pay.wage_or_share.wage.out | The mate says a wage is at least a thing a man can count.
wage | crew.hire.second_son.mate | Half a wage until he stops being sick over the side: that is fair, and he knows it.
wage | crew.hire.second_son.hire | Take him at a boy's wage till Martinmas
wage | crew.hire.second_son.man | Take him at a man's wage from the first day, with hand-money (2 d)
wage | crew.hire.second_son.hire.out | A boy's wage till Martinmas, and a handclasp.
wage | crew.hire.second_son.man.out | Two pennies of hand-money and a man's wage from the first stroke.
wage | crew.hire.dane.signed.out | The Dane counts it as part of the wage.
wage | crew.leave.bigger_ship.hand | It is a better wage and a drier ship.
wage | crew.leave.widow.season | Ask him for one more season, at a share and not a wage
wage | crew.leave.widow.season.out | A share and not a wage: he works the sum, and then she works it, faster.
helm | chain_barrow.s05.out_grave | If it is a king, a helm.
helm | chain_barrow.s08.out_great_success | A helm with a man's face.
helm | chain_barrow.s11.bishop_helm | A helm with a man's face.
helm | chain_barrow.s11.out_gear | The helm the bishop will not melt.
helm | chain_barrow.s12.reeve_helm | I have been reeve here eleven winters and I never answered for a helm with a face before.
helm | chain_barrow.s12.out_gear | The helm is packed in straw.
helm | chain_barrow.s13.smith_helm | The helm I do not want to break.
helm | chain_barrow.s13.opt_helm | Sell the gold and the silver, and keep the helm
helm | chain_barrow.s13.out_helm | The helm goes back to the boat in a sack of bran.
helm | chain_barrow.s13.opt_all | Sell it all, the helm too, and have him speak for you at the shipwright's
helm | chain_barrow.s13.out_all | The helm he breaks for its silver wire and its red stones, which takes an hour, and which he does with his mouth shut tight.
helm | chain_black_stones.s02.out_round | You put the helm over.
helm | chain_mission_north.s02.out_run | You put the helm over and the wind takes you clear.
helm | chain_relic_theft.s12.gozhelm_story | It will be written that the martyrs turned a Frisian's helm with their own hands, wishing to rest beside the holy Sebastian.
helm | chain_reric_808.s07.out_astern_success | When the last light goes you put the helm over and let the land breeze take you.
helm | chain_saga_northumbria_irish_sea.s07.opt_book | Leave the helm to the steersman, put about, and lash the book-chest yourself
helm | chain_saga_northumbria_irish_sea.s07.opt_pilot | Give Ketil the helm, who knows this sea, and hold on for Ireland
helm | chain_saga_northumbria_irish_sea.end_ships_boy.out_felag | In spring there is a second hull, with a Dane at her helm who counts everything and steals nothing.
helm | chain_saxon_exiles_804.s04.out_run_success | You put the helm up and go where the water is the colour of ale.
helm | chain_saxon_exiles_804.s04.out_run_failure | You put the helm up and go where the water is the colour of ale, and then where it is the colour of mud.
helm | chain_sword_smuggle.s03.out_order | He hears you out, looks at the sky, and puts the helm over a little for the ebb.
helm | chain_walrus_ivory.s07.out_run_success | You put the helm over for the gap between two islands where the water goes pale.
helm | chain_wreckers.s10.out_torch_failure | She sees the torch late and puts her helm the wrong way.
judge | chain_lindisfarne_book.s01.out_drink | He had four men to bury after it, so judge what he calls a bad one.'
judge | chain_mission_north.s11.out_old | King Eric was a fair judge of a horse, they say, and will make a sensible god.
judge | chain_pilgrim_road.s10.widow_two_dead | So I am not the one to judge it.
judge | chain_rorik_850.s13.bishop_ask | So he is mine to judge, as he never was while he was a heathen.
judge | chain_saga_rhine_frisia.s05.count_judges | After this the judges judge by what is written, and take no gifts.
judge | chain_saga_rhine_frisia.end_king.clerk_one | It is to be read out at every court from the Zwin to the Weser, and the judges are to judge by it.
judge | chain_saga_rhine_frisia.news_proclaimed | The judges are to judge by the writing and take no gifts.
judge | chain_saxon_exiles_804.s09.out_redeem | Know, he says, what was asked of them before you judge how they answered.
judge | chain_storm_vow.end_saint.out_candle | He says that a man is the judge of his own words until he meets a better one.
judge | chain_tiel_1020.s05.monk_charter | They say the emperor gave them a charter to judge as they please.
judge | chain_tiel_1020.s06.out_settle_success | We judge by our own will, he says.
judge | chain_tiel_1020.s09.elder_all | That we judge whoever trades on our bank.
judge | chain_tiel_1020.s09.opt_modest | Ask for what is true: Tiel men judge trade among themselves, and answer strangers before the king's man
judge | chain_tiel_1020.end_seat.out_key | Somewhere a monk is writing that the Tiel men judge by their own will.
judge | chain_tiel_1020.end_charter.priest_all | That the merchants of Tiel judge all who trade upon their bank, and that the oath of one, upheld by his fellows, ends a claim.
judge | chain_tiel_1020.end_charter.out_seat | From today the Tiel men judge by a writing, and it was your tongue that made them buy it.
judge | chain_tiel_1020.end_bishop.bishop_count | They will find that I judge by the law, and that I have read it.
judge | chain_wreckers.s11.out_oaths | Liudbrand swears first, in Frisian, with his whole crew behind him, and it takes some time because there are nine of them and the judge lets every man speak.
judge | chain_year_1066.end_house.out_sworn | They judge their own quarrels, lend to each other without witnesses, and drink at fixed feasts as if it were a duty.
judge | crew.hunger.wheat.steersman_before | So it lies between you and the Judge, which is a harder market.
judge | crew.bored.wonders.intention | You are the judge.
judge | crew.song.05.lead | The warden says a sharp man must judge words as well as deeds, and sets a guard on their ship.
judge | long.tiel.sworn_merchants.intention | They judge cases not by the law but by their own will, and say the emperor has granted them this by charter.
business | chain_cnut_1016.s13.reeve_emperor | Your business with the emperor's people is no affair of ours.
business | chain_dorestad_834.s01.opt_trade | Finish your business first.
business | chain_great_army_865.s08.opt_aside | Sell your cargo and stay out of kings' business
business | chain_great_army_865.s08.out_aside | That is all of the war that is your business.
business | chain_house_new_ship.s01.debt | That is between you and him, until he makes it my business.
business | chain_kentish_revolt_796.s02.reeve_offer | A Frisian keel is nobody's business.
business | chain_pilgrim_road.s03.opt_let | It is not your business: let him write the name down
business | chain_pilgrim_road.s10.intention | Then she takes the priest's sealed leaf out of her bundle and asks you what to do with it, because she can no longer tell what is a pilgrim's business.
business | chain_pilgrim_road.s10.opt_chest | Tell her it is no pilgrim's business, and let the abbess's chest have it
business | chain_relic_theft.s07.opt_false_out | A blank label is his cellarer's business
business | chain_reric_808.s03.out_sell | The ships are somebody else's business.
business | chain_riddle_trail.s09.out_dig_failure | Then torches, and housemen with staves who know their business.
business | chain_riddle_trail.s10.out_raise | The old man says the name three times, to the south, the west and the north, and then stops, as if the east were someone else's business.
business | chain_rorik_850.s05.out_away | You tell Eystein you have business up the river.
business | chain_rorik_850.s10.tollman_cross | I wear the cross for the day's business and the hammer for the night's.
business | chain_saga_danes_norway.s01.opt_wine | Say kings are no Frisian's business, and ask what wine fetches
business | chain_saga_northumbria_irish_sea.s03.ketil_weight | What it was for is your business.
business | chain_saxon_exiles_804.s07.brother_kin | There is one thing more, and it is not the king's business.
business | chain_saxon_exiles_804.s11.brother_watched | That is a cost, and it came of our business.
business | chain_saxon_exiles_804.s12.man_hired | That is twice you have been of use in this business, on the Rhine and here.
business | chain_saxon_exiles_804.s13.intention | The count has answered: five Saxons more or less on the bishop's land are the bishop's business.
business | chain_tiel_1020.s07.stranger_bishop | A false oath is the bishop's business.
business | chain_wine_fleet.s03.out_judgment_success | The count's man remembers business in the city.
business | chain_year_1066.s01.opt_trade | Stars are the clerk's business.
business | long.years_coin.witness.o.beach | Do your larger business on the beach beyond the stakes
business | sea.sail.far.hold.done | By evening she is a little smaller, and then she is gone, and you never learn her business.
country | chain_barrow.s03.lookout_guthlac | Felix wrote that for a king of this very country.
country | chain_coast_watch_810.s08.carter_oxen | They say a man from Benevento is going about the country with a bag of dust.
country | chain_lost_brother.s06.steward_law | A free man of the king's own Frisians, no: the law of his country says who sells him out of it pays as for a killing.
country | chain_lost_brother.s12.wife_husband | I have a husband in a country where they do not have winter.
country | chain_old_gods.s05.hand_owed | I know of no such place in your country.
country | chain_old_gods.s13.dane_heard | He finds that a civil thing in a Christian country.
country | chain_rollo_911.s05.steward_bread | The country will sell us nothing now it has seen us run.
country | chain_rorik_850.s08.merchant_where | The man was away getting himself a second country.
country | chain_saga_danes_norway.s08.intention | The sons of Godfred are home from the Svear with every man who lost last year, and the country has to choose again.
country | chain_saga_danes_norway.s08.envoy_home | The kings came back from the north with a tired host and found the country had got up before them.
country | chain_saga_danes_norway.s13.opt_ferry | Carry the country people and their goods over the Slie, at the ferryman's rate
country | chain_saga_danes_norway.s13.out_bread | The host stays three days on the shore, takes forty hostages from the country round about and goes home over the Eider.
country | chain_saga_england_south_east.s01.chief_winter | No army has sat a winter in this country before.
country | art.chain_saga_neustria_west.ending.saint | A country road through marsh and vines on the mainland, summer noon: a marble coffin on a frame carried on men's shoulders at the head of a long line of monks and country people, 
country | chain_saga_northumbria_irish_sea.s02.bearer_road | If the Tyne is shut we go west over the hills, to the abbot's country.
country | chain_saxon_exiles_804.s01.out_watch | You stand and look for as long as it takes to see that it is a whole country sitting there.
country | chain_saxon_exiles_804.s11.father_five | When they ask me in this country who my people are, I say your name after my father's.
country | chain_walrus_ivory.s04.king | The king asks every stranger about his own country.
country | chain_wine_fleet.s03.tollman_all_tolls | From the saint's Mass until the market ends, every toll in this country is Saint Denis's: boats, landing, carts, pack beasts, wheels, bridges, gates.
country | crew.home.michaelmas.boat | A boat for the terp country lies three berths down.
country | crew.home.michaelmas.break.fail | In the morning his thwart is empty and the boat for the terp country has sailed.
country | crew.home.frisian_wife.lookout | In my country they teach boys a saying about your people.
country | crew.cold.seafarer.lookout | There is a song in my country that a man made about this.
country | crew.bored.wonders.lookout | A king's counsellor in my country said that is a man's life: what went before and what follows, we do not know.
country | crew.leave.widow.fact | A widow in this country holds her dead husband's house and gives or keeps it in her own name.
country | long.eardwulf.exile.intention | This year Eardwulf, king of the Northumbrians, driven from his kingdom and his country, came to the emperor at Noviomagus and went on from there to Rome.
country | long.frisia.flood_838.l3 | The mound country has no cattle, no cheese and no seed corn this year.
country | long.pitres.edict_864.intention | the Seine King Charles ordered that whoever gives or sells a mail coat, arms or a horse to the Northmen, for ransom or any price, shall lose his life as a traitor to his country.
country | sea.frost.dawn.say.autumn | In my country this is called autumn.
country | sea.geese.skeins.say.wise | It knows when to leave a cold country, and it carries no cargo.
country | sea.night.light.say.north | North of my country it does not set at all.
country | sea.sick.fever.say.marsh | In the marsh country it comes on every third day and goes again, like a toll man.
country | sea.river.tow.pay.done | The crew sit with their hands in their laps and watch the country.
england | chain_amber_road.x_wulfstan.out_silent | In the morning he sails for England.
england | chain_black_stones.end_early.out_stones | Fifteen hundred pennies, the king's writing, and the only stones of the right length in England.
england | chain_cnut_1016.s01.intention | The tribute is being gathered over all England, and the town's share is weighed out on a bench by the water.
england | chain_cnut_1016.s06.intention | Since the count beat the emperor's army in the marsh at Flardinga, his men sit at the river mouth and take what they please from every keel bound for England.
england | chain_cnut_1016.s09.out_aboard | Every weaver's son in England will want to see Rome next spring, and he will carry cloth to pay his way.
england | chain_cnut_1016.news.fleet_paid | The tribute is weighed out in England: seventy-two thousand pounds, and ten thousand five hundred more from Lunden.
england | chain_cnut_1016.news.rome_tolls | The king's letter is read in England: the emperor and the king of the Burgundians have freed his people, merchants and pilgrims, of barriers and tolls on the road to Rome.
england | chain_lost_brother.s02.priest_imma | In England once a taken man's bonds fell off him each morning.
england | chain_lost_brother.s12.priest_masses | If bonds can fall off in England they can fall off in Spain.
england | chain_maldon_991.s07.intention | Men home from England are buying everything twice, and by the south jetty an Englishman is striking pennies for the king of the Danes.
england | chain_maldon_991.s07.asgot_son | He has gone back to England with a new boat, to live there.
england | chain_maldon_991.s07.asgot_son_unknown | I have a son in England.
england | chain_maldon_991.s07.opt_sell | Sell into the silver: let the men home from England bid against each other
england | chain_maldon_991.s07.out_weigh | He says cloth is what he would bring here: every man home from England wants a cloak cut the English way, to show where he has been.
england | chain_maldon_991.s10.priest_writ | On Saint Brice's mass-day, all the Danish men who are in England.
england | chain_maldon_991.s10.out_gunhild_success | You are past the bridge before the tide turns, and an outlaw in England by morning.
england | chain_maldon_991.end_north.intention | The king's ships go to England after the lambing.
england | chain_maldon_991.end_north.toke_house | But when England has one king over both shores, and it will, I am going home to Billingsgate.
england | chain_maldon_991.end_saved.kingsman_swein | The king goes to England after the lambing.
england | chain_maldon_991.end_saved.opt_peace | Ask only that when his ships go to England they keep what was sworn in 994: peace for a merchant keel in a river mouth
england | chain_maldon_991.news.brice | This year the English king ordered all the Danish men who were in England to be killed on Saint Brice's mass-day, two days after Martinmas.
england | chain_saga_northumbria_irish_sea.news.ireland | The north of England has lost its saint, and says who took him.
england | chain_storm_vow.s02.steersman_cost | His island is the far end of England, and a candle that long eats two cakes of wax.
england | chain_tiel_1020.s07.stranger_tiel | Every cask for England goes past their toll house.
england | chain_tiel_1020.s08.elder_count | In spring it goes out again on the keels, to England, to Cologne.
england | chain_walrus_ivory.s08.opt_fellow | Lay three hundred pennies in with his goods and sail for England
england | chain_walrus_ivory.s08.opt_pilot | Pilot him to England for a tenth
england | chain_year_1066.end_house.out_olaf | He builds a market town, and he never once sends a fleet to England.
england | chain_year_1066.news.york_norse | There are two kings in England.
england | crew.home.frisian_wife.steersman | Nobody in England has asked her what she says about the smell.
england | crew.winter.haul_out.steersman | Leave it and we bail our way to England.
england | art.long.alfred.ring_redeemed | A boy in a West Saxon cloak on a sunny quay holding out a knotted cloth of pennies; behind him a ship making ready for England.
england | art.long.cnut.fleet_paid_off | A Danish shipman home from England emptying a leather bag of silver pennies onto a ship's thwart in a Jutland harbour, his wife and neighbours watching, a merchant holding out a cup of wine to be tasted; 
england | long.vlaardingen.counts_toll.l1 | We told the emperor: if you do not protect us, we cannot cross to England, and the English cannot come to us, and then your own tolls will be the poorer.
england | long.fleets.three_fleets.l1 | Pay is promised on both shores, and on both shores it is the kingdom of England that is to pay it.
england | long.battles.two_battles.l1 | We went the length of England twice in three weeks, on foot.
the hold | chain_cnut_1016.s13.out_board | You fill the hold.
the hold | chain_coast_watch_810.s03.out_sell | They take everything in the hold that can be eaten or tied, weigh out a hundred and forty pennies' worth of hack-silver without arguing, and tell you why they are in a hurry: the fleet goes west along t
the hold | chain_coast_watch_810.end_islands.out_fires | You swear with them, a hand on the meeting's staff: when a fire is lit, your keel carries the word past the next headland, whatever is in the hold.
the hold | chain_dirham_hoard.s07.out_show_failure | Two of his men come over the gunwale anyway and go through the hold with their hands.
the hold | chain_great_army_865.s03.out_buy | They sit in the hold and do not look back at the town.
the hold | chain_kentish_revolt_796.s05.opt_search | Heave to, let him turn the hold over, and say nothing
the hold | chain_mission_north.s08.out_slip_success | By first light you are in open water with the hold whole.
the hold | chain_saga_england_south_east.s01.tollman_ask | I used to ask what was in the hold.
the hold | chain_saxon_exiles_804.s11.out_plot | The brothers will not let you go with nothing in the hold: five bundles of marten, the best of a winter's trading.
the hold | chain_saxon_exiles_804.s13.out_factor | Ten cakes of wax, six crocks of honey and a hundred pennies go into the hold.
the hold | chain_storm_vow.s01.hand_thor | Promise him the best thing in the hold, and mean it.
the hold | chain_storm_vow.s01.lookout_best | And the best thing in the hold.
the hold | chain_storm_vow.s01.opt_saint | Vow Saint Cuthbert a candle your own height, carried to his island, and the best thing in the hold
the hold | chain_storm_vow.s01.opt_thor | Vow Thor the best thing in the hold
the hold | chain_storm_vow.s02.lookout_recite | And the best thing in the hold on the day you land.
the hold | chain_storm_vow.s02.hand_recite | You said the best thing in the hold.
the hold | chain_storm_vow.s02.lookout_none | A candle your height to Saint Cuthbert, carried by you, and the best of the hold.
the hold | chain_storm_vow.s02.hand_none | The best of the hold, to the thunderer.
the hold | chain_storm_vow.s02.opt_stand_saint | Stand by it: the candle, the journey and the best of the hold
the hold | chain_storm_vow.s02.opt_stand_thor | Stand by it: the best thing in the hold, given in the north
the hold | chain_storm_vow.s02.out_stand_thor | You say it again in daylight: the best thing in the hold, in the north, before the year is out.
the hold | chain_storm_vow.s03.opt_clean | Give him two cakes of clean wax from the hold
the hold | chain_storm_vow.s05.mate_both | A candle to the one and a thing from the hold to the other.
the hold | chain_storm_vow.s05.opt_both | Pay both: a candle to the saint and a thing from the hold to the thunderer
the hold | chain_storm_vow.end_saint.bishop_best | The best thing in the hold.
the hold | chain_storm_vow.end_saint.lookout_silk | I only know what is in the hold.
the hold | chain_year_1066.end_house.narr_dead_gear | The mail shirt with the hole in it is still in the hold.
the hold | crew.home.returns.cheeses | Take him back, and buy his mother's cheeses for the hold (4 d)
the hold | crew.hunger.wheat.intention | There is famine ashore and wheat in the hold.
the hold | crew.hunger.wheat.fact | The sacks in the hold are tight and smell of summer.
the hold | crew.tongue.english.hold | Have the lookout give you the English for every thing in the hold
the hold | crew.winter.haul_out.own | Haul her and broach the tar in the hold (3 d for moss and rollers)
the hold | crew.winter.long_nights.rope | Set them to lay rope from the hemp in the hold
the hold | crew.cargo.swords.intention | There are blades in the hold that the king's law forbids.
the hold | long.england.great_hunger.o.sell | Sell two sacks from the hold at what they will give
the hold | sea.gale.breaks.full.failure | Whatever was lowest in the hold is soaked through, and the sail has begun to tear.
the hold | sea.sail.seam.patch | Cut a patch from the sailcloth in the hold (sure)
the hold | sea.cargo.shifts.fact | On a hard roll something heavy slides in the hold and fetches up against the lee side with a thump that everyone feels in their feet.
skipper | chain_amber_road.s05.intention | His son comes to you because you are the only other baptised skipper on the lake.
skipper | chain_black_stones.s05.out_search | Nobody searches a skipper's shirt.
skipper | chain_black_stones.news.late | A Frisian skipper has sent word to the abbot on the Canche of an answer that went astray two summers ago.
skipper | art.sea.chain_black_stones.s05_kings_boat | ay, white cliffs low on the horizon: a long twelve-oared boat lies across the bow of a small sailing boat; a Frankish officer stands in her bow with one hand raised; the skipper's hand is at the neck of his shirt.
skipper | chain_dorestad_834.end_flight.intention | A boat from the Rhine ties up beside yours, and her skipper tells the bank what this summer cost Dorestad.
skipper | chain_house_new_ship.s12.offer | My skipper sails for Ripa without me if I ask him.
skipper | chain_kentish_revolt_796.s06.out_read_failure | All Canterbury knows which boat brought it, and that her skipper bleeds like anyone.
skipper | chain_kentish_revolt_796.end_mercia.intention | Behind him the hall roofs are black rafters, and the first thing he asks any skipper is whether he carries corn.
skipper | art.sea.chain_kentish_revolt_796.s05_watch_boat |  under a white chalk headland, choppy water and low cloud: an eight-oared boat lies across the bow of a small sailing boat; a man stands in her with a spear upright; the skipper's hand is at the neck of his shirt.
skipper | chain_lindisfarne_book.end_alms.out_king | Then he wraps the book in his own cloak and goes to look for another ship north, apologising to the skipper before he has found him.
skipper | art.crew.chain_lindisfarne_book.s04_hole_in_the_page | oat under a brown sail, low sun, calm water: a thin English youth holds up a wax tablet on which he has drawn a page with a small round hole in it; the steersman and the skipper lean in over their bread.
skipper | art.sea.chain_lindisfarne_book.s06_green_water | The small boat hard pressed under a dark afternoon sky, a green sea breaking over her quarter; the skipper crouched under the tilt with a wrapped bundle held against his chest; the broad steersman braced at the steering oar, looking to windward.
skipper | chain_old_gods.s05.intention | A Danish skipper who knows your hand of old is loading for the islands.
skipper | chain_old_gods.s07.steward_sit | You are the Frisian the skipper spoke for.
skipper | chain_old_gods.s13.intention | A Danish ship lies off the beach, and her skipper is wading ashore with something wrapped in cloth.
skipper | chain_old_gods.s13.out_both | The skipper has asked about you along the coast.
skipper | chain_pilgrim_road.s03.out_vouch_success | He says a skipper who swears for strangers ought at least to have his cargo looked at kindly, and tells the toll man so.
skipper | chain_pilgrim_road.s06.intention | Twenty paces off the king's reeve is asking each skipper whom he carried over this summer.
skipper | art.crew.chain_pilgrim_road.s09_third_day | The open boat under sail off a low shore, late afternoon, a grey chop: the widow lying under a cloak on the bottom boards, shaking; three crewmen turned toward the skipper at the steering oar; a strip of pale cloth in her fist.
skipper | chain_relic_theft.s08.hand_vow | Skipper, if we vowed them a candle.
skipper | chain_riddle_trail.s01.intention | A Danish skipper in from Ripa walks the dam asking which boat is the Frisian's with the Northman in it.
skipper | chain_rollo_911.s00.intention | A skipper in from the Channel is selling his news for the price of listening.
skipper | chain_rorik_850.s05.merchant_open | Every skipper between here and Ripa knows by now that the river has no keeper this summer.
skipper | chain_rorik_850.s06.opt_home | Hear the skipper out, buy nothing, and turn for home on the next wind
skipper | chain_rorik_850.s06.out_home | You buy the skipper ale and hear it all twice.
skipper | art.chain_rorik_850.end_sworn | The ending 'the count's man': a broad Danish-built ship on a river in clear morning light, three strokes cut in her stem, a Frisian skipper at the steering oar; on the bank behind, the count raises one hand, and further back a clerk writes.
skipper | chain_silver_road.s04.out_help_success | Her skipper is Botmund, of Paviken on Gotland.
skipper | chain_silver_road.s04.out_help_failure | Her skipper, Botmund of Paviken, calls over the water that he saw who tried.
skipper | chain_storm_vow.end_thor.lord_marked | I had it from a skipper before you were past the skerries.
skipper | chain_storm_vow.news.candle | A Frisian skipper has brought a candle his own height to Saint Cuthbert's island for a vow made at sea.
skipper | chain_storm_vow.news.light | A skipper who owed a saint paid for it.
skipper | art.crew.chain_storm_vow.s03_dipping | A boat on a strand at evening, a fire and a long wooden trough of melted wax beside her: the skipper standing against the mast while the boy holds a tarred line from his heel to his crown; a pale candle as long as an oar hanging from the lowered yard.
skipper | art.crew.chain_storm_vow.s05_both_ways | e boat becalmed, sail hanging, low sun: a big fair Saxon with a long knife across his belly sitting on a thwart with his elbows on his knees, looking steadily aft at the skipper; the steersman behind, looking at the water.
skipper | art.sea.chain_storm_vow.s14_second_storm | The boat in a gale at dusk, well reefed and holding her course: every man bailing or hauling with his head down, the skipper at the steering oar, and by the mast an old cord thick with knots swinging in the wind.
skipper | chain_walrus_ivory.s08.intention | A skipper who knows the eastern sea is drinking with him, and a Frisian from the Rhine has already asked the price of the whole cargo.
skipper | chain_year_1066.s01.intention | There is a star with hair in the evening sky, and a skipper in from London who says he can tell you what it is for.
skipper | chain_year_1066.s01.opt_ask | Buy the skipper his ale and get all three stories straight, with the names
skipper | chain_year_1066.s02.narr_warned | The skipper who talked under the star said this would happen, and when.
skipper | chain_year_1066.s05.intention | The king's man on the landing wants two things of a foreign skipper: food, and what he knows of the English rivers.
skipper | chain_year_1066.s10.out_south | A Humber skipper pays thirty pennies to hear all of it first, with the names.
skipper | chain_year_1066.end_norman.narr_york | There are two crowns in this land tonight, and a foreign skipper had a hand in that.
skipper | chain_year_1066.end_norse.intention | His men went to the bridge in their mail, all of them, because a foreign skipper came down the river on a Sunday night.
skipper | chain_year_1066.end_house.narr_fugitives | Her boy has sent word by a wool skipper.
skipper | crew.leave.bigger_ship.bless | Let him go, paid to the day, and shake the skipper's hand (6 d)
skipper | crew.leave.bigger_ship.match.out | The skipper laughs and says a small boat that pays like a great one will soon be neither.
skipper | crew.leave.bigger_ship.bless.out | Paid to the day, and your hand to the skipper.
skipper | long.news.offa_hamwic | A Frisian skipper says the West Saxon wic at Hamwic is half open while the kings quarrel: the reeve there asks fewer questions than the one at Lundenwic.
skipper | long.offa_quarrel.ports_shut.intention | A Frisian skipper back from the Thames says how it stands.
skipper | art.long.offa_quarrel.ports_shut | A Frisian skipper on a river jetty at evening, his boat riding low with rough black quernstones, jerking his thumb toward the west; grey sky, slack sail.
skipper | long.reric.merchants_moved.o.word | Send word to Sliesthorp by this skipper, to ask after the men you knew (4 d)
skipper | long.godfred.dead.o.first | Send word ahead to Sliesthorp that wine is coming (3 d to the skipper)
skipper | art.long.dorestad.again_835 | A Frisian skipper at an ale-house bench counting off raids on two fingers, behind him through the door a river with half-rebuilt jetties; rain.
skipper | long.rorik.holds_dorestad.o.gift | Send Rorik's toll man a gift by this skipper, so your name is known (15 d)
hour | chain_barrow.s02.out_dig_success | In an hour you have the gold mount, a silver buckle black as a coal, and the rim of the bronze bowl with burnt bone in it.
hour | chain_barrow.s05.out_ask | He talks for an hour.
hour | chain_barrow.s13.out_all | The helm he breaks for its silver wire and its red stones, which takes an hour, and which he does with his mouth shut tight.
hour | chain_black_stones.s09.reeve_hour | If you have something for me, I will lay it before the king at the right hour.
hour | chain_black_stones.end_early.reeve_first | I asked him at the right hour.
hour | chain_black_stones.end_shut.rival_dead_king | The new king has Kent in arms and no hour for letters.
hour | chain_coast_watch_810.s05.hand_kings | One small keel is not worth an hour of anybody's rowing.
hour | chain_dirham_hoard.s05.brother_first | I sent for you, niece, within the hour.
hour | chain_dorestad_834.s01.out_warn | You cast off within the hour with a fisherman's word and nothing to hang it on: many ships, some smoke.
hour | chain_dorestad_834.s02.intention | An hour's walk of open bank, a storehouse on every dam, and not so much as a ditch between the river and the richest market in the north.
hour | chain_dorestad_834.s07.out_short | You sell down to what one boat can lift in an hour, and move your bedding aboard.
hour | chain_dorestad_834.s09.out_run_failure | She has you inside the hour.
hour | chain_dorestad_834.s10.out_short | When they come there is an hour's warning, your boat is loaded, and what burns of yours is a roof.
hour | chain_kentish_revolt_796.s01.old_empty | It is the first quiet hour I have had since the hay.
hour | chain_lost_brother.s02.priest_imma | His brother, a priest, thought him dead and said Mass for him at that hour.
hour | chain_maldon_991.end_saved.toke_wife | I say that every hour.
hour | chain_mission_north.s02.steersman | There is a beach under that low point, an hour off.
hour | chain_mission_north.s06.opt_wine | Sell Frideburg a cask to keep for her last hour
hour | chain_old_gods.s01.title | A bad hour
hour | chain_pilots_secret.s09.master_bede | The sea follows the moon, and each day the flood comes later by four parts in five of an hour, as she does.
hour | chain_relic_theft.s06.intention | The deacon has gone up to the church to hear the office and will be an hour.
hour | chain_relic_theft.s06.gozhelm_now | An hour.
hour | chain_relic_theft.s09.out_name_failure | It takes a hundred pennies and an hour before he agrees that the name may be the right one.
hour | chain_riddle_trail.s02.out_shrug | You load his cart with another man's hides for an hour.
hour | chain_rollo_911.s01.out_look | Odilard shows you the town in an hour: the two churches with a gallery between them, the empty places where the saints lay before they were carried inland, the old theatre where the Northmen keep horses.
hour | chain_rorik_850.s11.merchant_carts | It has a ditch and a stone barn, and it is an hour from any river.
hour | chain_saga_england_south_east.s03.nun_island | It is on the same island as they are, an hour's walk from their huts.
hour | chain_saga_england_south_east.end_minster.abbess_both | Both ealdormen are dead and the heathen men are still an hour from my gate.
hour | chain_saga_neustria_west.s03.intention | On the feast of Saint Philibert nine ships of the Northmen came to the island, and the island fought them from the ninth hour until evening.
hour | chain_saga_neustria_west.s03.narr_day | At the ninth hour a boy on the east head shouts, and there they are: nine sails, low, coming round the point on a flat sea.
hour | chain_saga_neustria_west.s03.out_wall_ok | They come up the sand at the ninth hour and are still coming at dusk, fewer each time.
hour | chain_saga_neustria_west.s03.out_wall_fail | A spear butt breaks two of your ribs somewhere between the ninth hour and evening, and you see the end of it sitting down.
hour | chain_saga_neustria_west.s05m.intention | Between the two there is an hour when a flat boat on a marked line could bring horsemen across.
hour | chain_saga_neustria_west.s07.erm_write | It is the third hour, unless I am mistaken, and I would be glad if you told me so.
hour | chain_saga_neustria_west.s08.out_vow | The air comes within the hour.
hour | chain_saga_neustria_west.e3.narr_vigil | At the ninth hour every man on the wall looks at the east head.
hour | chain_saga_neustria_west.e4.narr_tithe | You vowed a tenth and a lamp's oil off Batz, and the wind came within the hour.
hour | chain_saga_neustria_west.s13.out_plain | You tell him: the mound above the tide line, the channel, the hour of the flood.
hour | chain_saga_neustria_west.news.feast_fight | The count of Herbauge and the abbey's men fought them from the ninth hour until evening and beat them off.
hour | chain_saga_northumbria_irish_sea.s07.steersman_backs | In an hour we cannot.
hour | chain_saga_northumbria_irish_sea.s10.abbot_price | They are to pay her his price, lead him up Oswigesdune at the sixth hour, and put a gold ring on his right arm.
hour | chain_saxon_exiles_804.s02.out_refuse | Tetta walks you back to the creek and says only that the tide is at the fourth hour, in case cloth should change its mind.
hour | chain_silver_road.s04.out_help_failure | An hour later the wind backs and she claws off by herself, with your oar.
hour | chain_storm_vow.s06.out_barefoot | On the far side a brother is waiting who has seen you coming for an hour.
hour | chain_tiel_1020.end_count.out_peace_success | He walks the dyke with you for an hour.
hour | chain_walrus_ivory.s05.out_reeve | The squall leans on it for an hour and goes away.
hour | chain_wreckers.s03.murk | This is the hour.
hour | chain_year_1066.s05.out_sell_success | He argues for an hour and then sends for the scales.
hour | chain_year_1066.s07.out_cross | At the third hour of the day the keel takes the shingle at Pevensey.
hour | crew.bored.calm.row | Out oars: an hour's pull earns the evening ale
hour | crew.bored.calm.row.out | An hour at the oars across water like a floor.
hour | crew.bored.calm.dice.fail | By the ninth hour they have stopped speaking to each other.
hour | crew.heart.deor.work | A hard hour at the oars, and a plain word about where the next silver is
hour | crew.heart.deor.work.out | An hour's pull that leaves no breath for brooding, and then you say it plainly: where the cargo sells, what it should fetch, when they are paid.
hour | crew.friend.one_cloak.watch.out | The boat is better looked after between midnight and dawn than at any other hour.
hour | crew.riddle.fog.take.out | The men argue over it in whispers for an hour, which is an hour they do not spend hearing rocks.
hour | crew.winter.long_nights.fact | It is dark by the ninth hour, and dark again whenever anyone wakes.
hour | crew.song.24.lead | Up above, the Danes see blood come to the surface, and at the ninth hour they go home.

### mate: only lines where it may be a word of address

mate | art.crew.faith.vow | Dusk on the water: the big Saxon mate stands to speak with one hand lifted, as if answering a question from a bank; in the cloud behind him, men waist-deep in a river and spearmen on the shore.
mate | art.crew.tongue.letters | A calm bright day at sea: the lookout holds a small waxed tablet on his knee and scratches at it with a stylus; the big mate bends close over it, frowning, a thick finger hovering over one mark.
