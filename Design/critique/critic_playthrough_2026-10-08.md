# Kāpman: a critic's play-through, 8 October 2026

**What I looked at, and what I could not judge.** I did not run the game. I walked through the pictures in `G:\kapman_unity\Kapman2_ship\Captures\chart` in name order and read every word on them as a first-time player would: all 25 of the opening (open_01 to open_19) and 76 of the 89 full-size numbered ones (00 to 73), covering every phase. The files are stamped 7 October, 22:41 to 22:46. I read DECISIONS.md items 47 to 66 and the headings of review_newcomer.md. To check what a still cannot show, I read a few lines of code and ran nothing: the lender's and Ravn's lines in Showtime.cs, the keys the test pressed in OpeningTests.cs, and where a few stock sentences come from. Where a point rests on code and not on a picture, I say so. I could not judge: sound and music, the letter-by-letter speech, the shake of the marks, the cuts through black, how long anything takes, how the pointer feels, or the boat in motion. Only one of the lender's seven paragraphs and two of Ravn's seven are on a picture. The numbered shots come from a test with an older start (the "Who are you?" window, the count as lender, a card's whole text at once, windows stacked on the left). I did not judge the stacking, the bar under it, the old start window, or several paragraphs standing at once in a card. I left out what is already known and being fixed, unless I had something to add.

Each point has four parts. Saw: what was on the picture. Hurts: why it costs a first-time player. Change: what I would do. Size: small (under an hour), medium (an evening), large.

---

## A. Things that look broken or wrong on screen

**A1. The hold never adds up.**
- Saw: 07_panel_voyage.png and 07_panel_port.png: "582 of 660 kg of room taken". The rows under it are 460 kg and 20 kg. 17_voyage_port_picked.png: "444 of 660", rows 345 and 20. 42_guard_boat.png: a 20 kg bundle of fish took 40 kg of room. The only clue is "Light, bulky goods take more room than they weigh."
- Hurts: it is the first sum a merchant does, and it fails. The player decides the game cannot count.
- Change: show one measure. Head it "Room: 582 of 660" and give each row the room it takes (wine 552, bread 30). Drop the weight, or put it second in small type.
- Size: small.

**A2. Distances to ports jump while she sails.**
- Saw: bound for Lundenwic. 40_happening_at_sea.png: 399 km to go, "Dorestat 12 km", "Traiectum 42 km". 42_guard_boat.png: 388 to go, "Traiectum 10 km", "Dorestat 40 km". 43_guard_boat_answered_itself.png: 386 to go, "Traiectum 23 km", "Dorestat 52 km". She moved 13 km in all. The port she left went from 12 km to 52 km. Between the last two shots she moved 2 km and both ports moved 12.
- Hurts: these rows are how a player decides to turn back. They read as random numbers.
- Change: reckon each row from where the ship is, back along the water she came by as well as forward. Add a test: sail 10 km from a port, and the port must read 10 km.
- Size: medium.

**A3. The catch of fish is wrong four ways.**
- Saw: 41_happening_at_sea_answered.png and 42_guard_boat.png. After "Put out the lines" the hold gains "1 bundle of dried fish. Your own from the start." The card says "A good catch: food for days. But the hold is full: there was no room aboard for all of it." The hold had 78 kg of room before and 38 after. The top bar jumps from "bread for 7 days" to "bread for 14 and a half days".
- Hurts: fresh fish is called dried, new goods are called old, a hold with room is called full, and fish is counted as bread. A player who reads closely stops trusting the words.
- Change: call it "fresh fish" with "Caught on 1 May." Say "more than she could hold" in place of "the hold is full". Call the top bar's figure "food", not "bread".
- Size: small. (GameEvents.cs line 248, FrameGame.cs line 172, FrameRoot.cs line 827.)

**A4. Catalogue labels stand where a sailor's word should.**
- Saw: 21_lookout.png: "A Frisian, Frankish and English sea-going merchant ship of Dorestat, to the south-east, 3 km off." 44_trader_hailed.png: a window whose title is "An expanded or double logboat river freighter of Dorestat", met at sea. 47_raiders.png: "A Norse warship of Ögvaldsnes: 2 ships, about 60 men." 07_panel_ship.png: "The Gislinge boat. A 7.7 m working boat for fishing and carrying goods (Denmark, c. 1130)."
- Hurts: these are the strongest "a machine wrote this" lines in the game. One warship is two ships. A river logboat is out at sea. The player's own boat is dated 337 years after the game.
- Change: give every kind of ship a short name a lookout would shout, and use only that on screen: "a Frisian trader", "a river barge", "two longships". Keep the full label for the Book. Put the count in the noun: "Two Norse longships out of Ögvaldsnes, about sixty men."
- Size: small for the names, medium to check every kind. (Encounter.cs, MapView.cs line 1255, RaiderEncounter.cs.)

**A5. A card's rules leak onto the card.**
- Saw: 23_toll_man_card.png and 60_before_the_save.png. Choice 4: "Show the charter. Only with the writing "charter toll dorestad" in your chest." Choice 2: "Hide the swords under the ballast. Either: Only with weapons aboard; or: Only with captives aboard." with the tag "likely", though I carry no swords. Both are greyed so pale they can hardly be read. 71_slowed_nearing_port.png, the log: "Your answer was "Go round by the narrow water, slowly (sure)". Nothing more came of it."
- Hurts: a file's name, a rule written as logic and an odds tag are shown to the player as if they were writing.
- Change: a choice I cannot take shows one plain line in full ink with a small lock: "Show the charter (you have none)." Hide choices about goods I do not carry. No odds on a shut choice. Strip the odds tag before a choice goes into the log.
- Size: small. (Cond.cs lines 235 and 253, LogProse.cs line 35.)

**A6. The bright line on the chart is the wrong one.**
- Saw: 20_at_sea.png. I am bound for Traiectum. The bright dashed yellow line runs west to Witla, a port I only clicked on. My real course is a thin dark line. 40_happening_at_sea.png and 44_a_sail_to_hail.png: at sea the course is a hairline, dark on purple. 44 and 73_three_days_slowly.png: my ship and the other sail are two small white dashes, alike.
- Hurts: the eye follows the wrong line, and I cannot find myself.
- Change: the course I set gets the bright dashed line. A port I only looked at loses its line when I cast off. My own ship gets its own mark: gold, larger, with a dark edge. Other ships stay white.
- Size: small to medium.

**A7. Things cover other things, or are cut off.** A checklist of small ones.
- Saw:
  - The small boxes that come up under the pointer are near-black with round corners and white words. They lie over the "waits for your answer" tag (22_card_on_the_way.png, 40, 60), over the third row of buttons (21_lookout.png, 44_trader_hailed.png) and over port names.
  - A card or talk box covers the chart's tool row and leaves a stray "e" from "Measure" (11_talk_with_ravn.png, 23, 43). Slivers of port labels poke out over the card's top edge (60_before_the_save.png).
  - The boat picture's caption is cut by the screen's edge when it runs to three lines: "On the river, bound for Lundenwic. light air from the south-west" (43, 44_a_sail_to_hail.png, 72).
  - The player's scores wrap in the wrong place: "Tongue 1 Name 1 Nerve" then "2 Lore 2 Troth 3" (open_15_jobs.png and every shot after).
  - The title's menu box sits on the tail of the p (open_02b_title_menu.png). A pale tan patch with a hard slanted edge fills the left third of the title (open_02a_title_later.png). It reads as a fault, not as fog.
- Hurts: each is small. Together they say "unfinished" before the player has read a word.
- Change: dress the pointer's box like the frame and place it so it never covers a tag or a button (see C4). Hide the chart tools while a card is docked. Give the caption two fixed lines and shorten it ("River. For Lundenwic."). Keep each score with its number: five small boxes, or two rows. Drop the menu box below the name.
- Size: small each.

**A8. "waits for your answer" is not always true.**
- Saw: open_13_ravn_speaking.png and open_14_ravn_said.png show the tag while Ravn is still talking and the only answer is "Go on". 42_guard_boat.png: a guard boat card with the tag, in the afternoon. In 43, in the evening, another card has its place. The log in 44_a_sail_to_hail.png: "The small hours. No answer was given to the guard boat. She hove to, and they came aboard." 24_card_and_talk.png: a "Talk to people" window lies open on top of the waiting toll card, while the Port window says "Finish what is in front of you first."
- Hurts: the tag promises the clock is held. Then a card answers itself. I could not judge the timing from stills, and the test may have forced it. But two rules sit under one tag.
- Change: show the tag only when a real choice is up, and word it "The clock is held." A card that will not wait says so on its face ("They will not wait long.") and shows no tag. Do not let a second window open over a card.
- Size: small.

**A9. A Dane's game is a Frisian's game under other names.**
- Saw: 31_a_dane_at_ripa.png. A Dane of the old gods, at home in Ripa. The log: "You owe the count 1920 d by Michaelmas, 29 September 793." The Port window: "Your standing here: a stranger (0)." The market: "Prices in g silver", while the purse reads "86 d silver". 33_line_turned.png: Hroald greets them "A Frisian boat, and small ... Whose guest are you on this bank?" The chart there is teal sea and orange land, where Dorestat's is purple sea and olive land.
- Hurts: a heathen Dane owes a Frankish count by a church feast, is a stranger in their own port and is called a Frisian. Two kinds of money are on one screen with no rate. open_04_hail.png offers all four peoples to a new player.
- Change: for now let the opening offer Frēsland only, with the other three shown shut. Later give each people its own lender, day, coin and standing at home. If each people is meant to have its own chart colours, check the Dane's against the green and Byzantium frame chosen this week. They fight.
- Size: small to shut them, large to do them properly.

**A10. Money has five names and rounds against the player.**
- Saw: 16_bought_at_dorestat.png: "Sold 1 cask of Rhenish wine for 34 d, less 3.4 d toll: got 30 deniers." and "Bought 1 cask of Rhenish wine for 34.7 d: paid 35 deniers." 07_panel_trade.png: "They ask 5.3 d". open_09_showroom.png: "Eight pounds of silver". 31: "g silver". 50_after_the_raiders.png: 86 d, less "50 deniers' worth of silver", leaves 35.
- Hurts: d, deniers, pennies, pounds and grams in one evening. 34 less 3.4 is 30.6, not 30. There is no tenth of a penny.
- Change: one word on screen, "pennies", with "d" only in tables. Whole pennies in every price. Keep the tenths inside the sum and never print them.
- Size: small.

**A11. Small wrong words and numbers, one line each.** All small.
- Every time to a port starts at "half a day": 5 km, 9 km, 30 km and 167 km ("half a day to 2 days") alike (06_list_of_panels.png, 20_at_sea.png, 71_slowed_nearing_port.png). Let a short run say "an hour or two" or "by midday". Narrow the range once today's wind is known.
- "Your standing at Traiectum rises. You are a stranger there now." (26_sold_at_traiectum.png). It reads as a fall. Name the rank only when it changes.
- The card says "about 60 men" (47, 48, 49). The log says "about 65 men" (50). The log also puts "makes out a Norse warship" before "sights a sail".
- "Almonds. Best paid at Parisius, 78 d. Cheapest at Rotomagus, 92 d." (07_panel_wares.png). It reads as a mistake, because the best price is lower than the cheapest. It only means there is no gain in almonds. Say "Sells for most at" and "Bought for least at".
- "You know nobody yet." (07_panel_bonds.png) while Ravn is aboard and the port knows me at +25.
- The log thanks Sigibald (26). The Bonds window has "Meinbod, the cellarer of Traiectum" (61_the_game_went_on.png). The People here list at Traiectum, as far as it shows, has Hamacar, "Hadda, Cellarer" and Radger. That may be two cellarers at one port. Worth a check.
- "You praised the porridge. The crew have lost heart." (61). I cannot tell why one follows the other.
- "Evening at sea." on a card while the caption says "On the river" (43). "What can be seen from the masthead now." on a boat 7.7 m long (21).
- The first morning's card says "early morning" (open_10_first_morning.png). The bar a moment later says "morning" (open_11_free.png). The menus give the hour in figures, "1 May 793, 15:23" (63_title_with_continue.png). The bar gives it in words.
- "bread for 28 and a half days" (open_11_free.png) falls to "bread for 7 days" (open_16_jobs_done.png) when three men come aboard. No line says why.
- The given name "Oodbald" (open_05_name.png) reads as a slip of the keys. It is in names_793.json.

---

## B. The first ten minutes

**B1. After Ravn stops, nothing on screen holds what he said.**
- Saw: open_15_jobs.png. Ravn's last words (open_14_ravn_said.png) were "Mend the sail, find the men, buy well. Then Voyage, pick a port, and Cast off." The screen then shows one job, the sail. Nothing shows the men. The lower half of the left window is empty. Cast off is grey and no reason stands by it. In open_16_jobs_done.png both jobs are done and Cast off is still grey, because no course is set, and nothing says so. Read in code, not seen: the reason is there, but only when the pointer rests on the button. And three texts give three directions for one job. Ravn says "the Alehouse button". Errands says "the People here window". Cast off says "Look under People here". People here has no button among the twelve.
- Hurts: this is the minute a stranger decides. They have five things to do in order and must hold them in their head, with twelve buttons and no mark on any.
- Change: put the list on the screen. "Before she sails" already exists in Errands. Show it at the top of the Port window, or make Errands the one window after Ravn. Five rows that tick off: sail, men (1 of 4), cargo, course, cast off. Print the reason under a grey Cast off at all times. Make all three texts send the player to the alehouse. Mark Ravn's tip in the Trade window ("Ravn's word") and its port on the chart, so one thread runs through the first voyage.
- Size: medium.

**B2. The first window is a list of things not built.**
- Saw: open_11_free.png, open_15_jobs.png, open_16_jobs_done.png. The Port window's rows: "Talk to people. News, prices elsewhere, the weather (to come)." "Visit the toll house. Tolls and permission to trade (to come)." "Exchange. Change foreign coin into local money, at a loss (to come)." Under them: "The market is the Trade window. Casting off is the button above the windows." 15_waterfront_talk.png: at home on the first morning the first row opens a box that says "Nobody here has news you haven't heard."
- Hurts: three of four rows tell a stranger the game is not finished. The first row gives nothing on day one. The one real job is lost among them. The scores "(+25)" and "(+10)" stand in the palest small type on the screen.
- Change: hide every row that is not built. Cut the sentence that says where two buttons are. Show standing as a word and five pips, as Tongues does.
- Size: small. (PortPanel.cs.)

**B3. Whose is the wine, and whose is the silver?**
- Saw: open_15_jobs.png, the log: "She has aboard 4 casks of Rhenish wine that are the Wōkerer's own". 16_sold_at_dorestat.png: "Sold 1 cask of Rhenish wine for 39 d, less 3.9 d toll: got 35 deniers. It was the count's own, so what it fetched is his stake and not your gain." The purse goes from 86 to 121. The bar shows one number.
- Hurts: I cannot tell if I may sell it, if the silver is mine to spend, or if I have just robbed him. Read in code: the lender says of the wine "They are on the stick too", so it is part of the debt and mine to sell. Then "his stake and not your gain" is false, and it frightens. It comes back for every cask (26, 29, 61).
- Change: decide it, and let Ravn say it once: "The wine is yours to sell. You owe him for it all the same." Delete the stake sentence.
- Size: small. (Trade.cs lines 230 to 232.)

**B4. The first voyage ends and nothing marks it.**
- Saw: 72_slowed_made_fast.png: the log says "Arrived at Traiectum." 29_traiectum_at_the_end.png: after the toll, the alms, a sale and a passenger, the bar reads 152 d. Nobody speaks. Nothing sets 152 beside 86, or beside 1920.
- Hurts: this is the answer to "why should I care". A stranger has done one loop and does not know if it went well. Most of the gain came from the lender's wine, so they cannot work it out for themselves.
- Change: at each landing, one short slip in the foot. "Dorestat to Traiectum, half a day. Left with 86. Have 152. On the stick: 1,920." Then one line from Ravn. Keep the slips in Journey.
- Size: medium.

**B5. Where the debt is first shown, it is the smallest thing on the screen.**
- Saw: open_09_showroom.png. Three lines in large white type, then in small yellow: "Eight pounds of silver, owed to the Wōkerer. Enter or a click to go on." The debt shares its line with a key hint the user asked to be rid of (item 62). On the pictures I saw one of the lender's seven paragraphs (open_08_lender.png). The price, the day and the two men with a rope are in code and on no picture.
- Hurts: the boat gets the big type and the debt gets the small. After the opening, nothing on screen says what happens on the day.
- Change: make the debt the plate's last and largest line: "Eight pounds of silver. By Michaelmas." Cut the key hint. Put the threat where it can be found again, in Errands: "The day after, he comes with two men and a rope."
- Size: small.

**B6. The questions take one key press for good, and the lit row can mislead.**
- Saw: open_06_swear.png: "The old gods" is lit and the note under the list is about the old gods. The test pressed 1, and the game began "Christian, of Rome" (open_11_free.png). open_07_before.png: the lit row's note begins "Name 3." before the player knows Name is a score. open_04_hail.png: no row is lit, yet a note describes the first. open_03b_face.png: 17 faces, each about 80 dots wide on a screen 1920 wide. No box has a way back.
- Hurts: the old window said these "hold for the whole life". One stray key sets them. The note may describe a row the player did not take.
- Change: a number key lights a row and shows its note. Enter takes it. Backspace goes back one question. Show the faces at twice the size. Write the note without the score: "A name that opens a hall door once." Let Wursing read the answers back before he cuts the notch: "Radbod. Frisian. Rome's man. A steersman's child. So?"
- Size: small to medium.

**B7. On the first morning the player already knows the world.**
- Saw: 07_panel_wares.png: "135 wares known", under the line "A ware comes in here when you see it sold, hear its price or carry it." 07_panel_journey.png: "35 markets' prices" and "35 places heard of". 07_panel_chronicle.png: "Heard 39 days ago, ships from Eoforwic." 07_panel_book.png: "Ports. 35 of 50 pages open to you."
- Hurts: the windows promise that knowledge is earned. Then it is all there on day one. Hailing a ship and talk on the quay have little left to give.
- Change: start with the home market and the three or four ports a steersman's child would know. Let the rest come from talk, tolls and ships.
- Size: medium. It is a setting more than a build, and it makes the start harder, so try it before keeping it.

---

## C. Reading and layout

**C1. The words get the narrow half of the box.**
- Saw: open_14_ravn_said.png. Ravn's longest paragraph is set in four tight lines beside his face, in a column about 490 dots wide. The right half of the box holds "1. Go on" and a grey "Enter". The same in 21_lookout.png, 25_card_answered.png and 28_happening_answered.png.
- Hurts: this is "far too crowded", made by the layout and not by the writing. Item 62 asked for "more wide text space".
- Change: when the only choice is to go on, show no choices column. The words run the full width. A small mark at the corner shows there is more. The column comes in only with real choices. Drop the grey "Enter".
- Size: small.

**C2. Things are still said twice.**
- Saw: open_13b_ravn_one_paragraph.png: above Ravn's words stands "He came with the boat. Nobody told you." His words are "I come with the boat. He did not say?" Under his name: "Norse · Talks a great deal and means well". 22_card_on_the_way.png and 23_toll_man_card.png: under the speech a grey line retells it. "The toll man comes down to the boat with his tally stick. The king's tenth is taken on what you sell here, and he asks what you carry." 11_talk_with_ravn.png and 33_line_turned.png: under the speech a paragraph lists his looks and clothes. "About forty-five, lean, weathered, fair hair gone grey and tied back ..." 13_line_half_followed.png: a saying stands half in Norse with "You half follow it: some of it is lost on you", and whole in English right under it.
- Hurts: this is "why is he speaking twice" in four new forms. The nature line tells me what his voice already shows. The clothes are a note for the artist. The whole English under the half-heard verse undoes the rule. Three of the four were seen in the test's mode. I could not tell which a real player sees.
- Change: the head of a talk box is a name and a part, no more: "Ravn, your shipman." Cut the grey retelling from every card. Move looks and clothes to the art list. When a saying is half followed, show only the half.
- Size: small.

**C3. The top bar is one run of words.**
- Saw: open_11_free.png: "86 d silver bread for 28 and a half days Owed 1920 d by Michaelmas, 29 September: 151 days left". All one weight. The run is centred, so it starts at a new place each time the words change length (compare open_11_free.png, open_16_jobs_done.png and 04_dorestat_port.png).
- Hurts: the three numbers the player lives by have no fixed place. The eye lands on the date, the only bold thing.
- Change: three sunken boxes in fixed places, as a status bar of those years had. Silver, Food, Owed. The number bold, the word small. "151 days" as a number of its own.
- Size: small to medium.

**C4. Seven pieces are out of the frame's dress, or out of place.**
- Saw:
  - The pointer's boxes: near-black, round corners, white words (open_17_chart_in_port.png, 20, 21). One reads "The chart in the port's place, and back." Another ends "For being aboard. [-]". Neither is clear.
  - The loader's boat window has round corners (open_01_loading.png).
  - The showroom's plate is see-through dark purple with white words and a gold bar (open_09_showroom.png).
  - The first morning's card is black with thin gold lines (open_10_first_morning.png). It is the only black screen in the game.
  - Port names on the chart are grey boxes (open_17_chart_in_port.png). Nothing else on the screen is grey.
  - At night the port's picture is a near-black blue slab over most of the screen (open_19_port_at_night.png). Blue is nowhere else in the look.
  - The marks (open_12_marks.png) are pale yellow, flat and overlapping, in the middle of a busy picture. They point at nothing. I could not judge the shake.
- Hurts: "hard edges", "green grounds, Byzantium words" and "not so sleek" are kept in the frame and broken in each of these.
- Change: one dress for all. A pointer's box as those years had it: a pale ground, a dark line one dot wide, square corners, Byzantium words. Port names on the faded green. The place card on Byzantium, not black. Night as deep Byzantium with a third of the light kept. The marks over Ravn's corner of the screen, gold with a dark edge.
- Size: small each.

**C5. The log repeats the windows, and at sea it is only ships.**
- Saw: 16_sold_at_dorestat.png: the three-line sale sentence stands in the Trade window and in the log at once. 18_course_set.png: "Cast off when you will." twice on one screen. 73_three_days_slowly.png: after three days at sea the log holds only lookout lines, and the same ship of Dorestat is "made out" twice, at 8 km and at 9 km.
- Hurts: the log is the widest text on the screen. It says what I have just read, or it lists ships.
- Change: a log line is short and says a thing once. "Sold 1 cask of wine. 35." A sighting goes in the log only if something comes of it. A new day gets a line of its own with the date.
- Size: small to medium.

---

## D. Voice and wording

**D1. There are two Ravns.**
- Saw: open_13b_ravn_one_paragraph.png and open_14_ravn_said.png: a man who talks too much and forgets things. 11_talk_with_ravn.png: "Heill. You are late. The tide is not. I have a question for you when the cargo is aboard: an old one, the first one." 14_farewell.png: "Go to your count." The second man is dry and sharp and keeps riddles. His written looks, lean and fair gone grey, do not match the heavy dark-bearded face in open_13.
- Hurts: the guide is the one person the stranger is meant to love. He changes nature between the opening and the first talk.
- Change: keep the opening's Ravn. Rewrite his quay lines in that voice. Make the riddle a thing he half remembers. Change "your count" when the lender is Wursing.
- Size: small. The words are the user's.

**D2. Ravn reads out the manual.**
- Saw: open_14_ravn_said.png: "Then Voyage, pick a port, and Cast off. If you forget, it is all under Errands." Read in code, on no picture, his fifth paragraph: "The buttons up there are yours. The top row is the ship: Cast off, Voyage, Crew, Hold. The middle row is where we are: Trade, the alehouse, the minster, Rest. The bottom row is what you know: Wares, Journey, Errands, the Book. The clock is at the very top: Space stops it, and the numbers hurry it." His third names "the Port window, 12 d and six hours".
- Hurts: this is what the user spoke against: "you don't have to explain everything like this". A shipman of 793 lists twelve buttons. It is also seven paragraphs before the player may touch a thing.
- Change: cut the fifth paragraph whole. Ravn speaks of things, not buttons: "The sailmaker wants twelve pennies and the day. The men are in the alehouse. Where else." Light the button he means while he says it. Four paragraphs at most. The rest he says later, when it comes up.
- Size: small for the cut, small for the lit button. (Showtime.cs lines 1410 to 1420.)

**D3. Stock sentences, and people who do not answer.**
- Saw: "Liudbrand thinks better of you for it." "Sigibald thinks better of you for it." "Meinbod thinks better of you for it." (25, 26, 61). "A boatman at Dorestat said Noviomagus was paying poorly for Rhenish wine. A few days back.", and the same frame twice more with "The toll man at Dorestat" (07_panel_trade.png, 17_voyage_port_picked.png). "Prices heard for 14 places, now in your ledger: Walichrum (5 days ago), Witla (5 days ago) ..." (24) and "Prices at 4 ports, now in your ledger: Winaldum (5 days old) ..." (46). 25_card_answered.png: after I choose, the box prints "Declare the cargo", then "You declared the cargo. The toll is taken on what you sell here." The toll man says nothing more.
- Hurts: the frame shows through by the second use. The colon is the tell all over the game: "It asks: ... It opens: ..." (open_06_swear.png), "less 3.9 d toll: got 35 deniers", "A good catch: food for days. But the hold is full: there was no room". The game's own lines say "haven't", "isn't", "They're" and "he'll", while no person in it shortens a word.
- Change: every choice ends in the person's own last line. "Wine. Four casks. Sell it and I will have my tenth." A feeling shows as a small mark by the name, not a sentence. Rumours get three or four shapes and a speaker with a trade. No colon in any line a person might say. One rule for shortened words.
- Size: medium. A rule for the builders, then the user's pen.

**D4. Museum words and modern measures.**
- Saw: Port window headings "Dorestat (Wijk bij Duurstede)", "Traiectum (Utrecht)", "Ripa (Ribe)" (open_11_free.png, 23, 31). "Norse. Hávamál 76." (13). "2 m/s" (07_panel_sky.png). "8.0 km off, holding the gap. Sunset in 15 h 00 min. 0.0 hours since they came on." (47_raiders.png). "5.1 km off, closing at 7.4 knots. Sunset in 14 h 42 min." (48_running_from_raiders.png). "216 litres" (50). Kilometres and kilograms in every window.
- Hurts: the game says Michaelmas, troth and shipman, and then speaks like a ship's computer. The raider line is the worst case. The most frightening moment is an instrument reading.
- Change: modern place names and verse numbers go to the Book. In the Voyage list show the time and drop the distance. Keep figures for the Measure tool. Count the hold in casks and loads. Say the chase in words: "Two miles off and not gaining. Fifteen hours to dark."
- Size: medium. (RaiderEncounter.cs lines 185 and 186.)

**D5. Three cards are stubs beside the good ones.**
- Saw: 27_happening_in_port.png: "A man asks passage. Someone wants carrying to the next port; he'll pay." Take him, or refuse. 40_happening_at_sea.png: "Fish. A shoal under the boat; the men have lines." 42_guard_boat.png: "A boat of the king's men off Dorestat signals you to heave to." 28_happening_answered.png: "He pays 8 deniers for his passage."
- Hurts: no face, no name, no price, no port, nothing to weigh. They share a box with the toll man, who has a voice and a law. The passenger pays before he is carried, and I never learn where to.
- Change: a card needs one person who speaks and one named thing at stake. If it has neither, make it a line in the log.
- Size: medium, mostly words.

**Lines that already have character.** These set the standard. Do not smooth them.
- "Step into the light." (open_03b_face.png)
- "Radbod. Mm. One notch. ...There. You are on the stick." (open_08_lender.png)
- "She floats. He has watched her do it." (open_09_showroom.png)
- "That is my name", as a button (open_05_name.png)
- "Halls and markets in the north, where a Christian is a guest and you are a neighbour." (open_06_swear.png)
- "The widow who kept the boat", "A captive come home", "Schooled at the minster, and left" (open_07_before.png)
- "He did not say? No. He would not." and "I forget things. That is how I know where it is kept." (open_13b, open_14)
- "The sail. Rotten along the foot. She cannot put out under it." (open_15_jobs.png)
- "Not before, not after, and not less." and "She sits deep. Name what is in her, from the stern forward." (23_toll_man_card.png)
- "No man knows who unloaded her." (22_card_on_the_way.png)
- "Small boats pay what they owe." and "If nobody's, you are mine until you shame me." (33_line_turned.png)
- "Nobody steals from a man who is looking at them." (14_farewell.png)
- "A good name does not. That is all the silver I trust." (12_line_in_mid_turn.png)
- "The knots are his." (07_panel_chest.png)
- "Half known: you get the gist and miss the catch." (07_panel_tongues.png)
- "The measure is in your hand" (19_measure.png)
- "She rides high: light, or in ballast." (44_trader_hailed.png)
- "They're alongside. You have 4 men." (49_raiders_alongside.png)
- "Sound: middling" (open_02b_title_menu.png)

---

## E. Feedback and feel

**E1. The worst hour of the game looks like the fish card.**
- Saw: 47_raiders.png, 48_running_from_raiders.png, 49_raiders_alongside.png, 50_after_the_raiders.png. Two ships and sixty men come up in the same small green box, with the same size of title, as "Fish". The boat's picture shows my boat alone on a calm sea. "Pay tribute" has no price and "Turn and fight" no odds, though the toll card had both. Afterwards there is one line in the log: "You gave up without a fight. They took 4 casks of Rhenish wine, 1 bundle of dried fish, and 50 deniers' worth of silver." The hold reads "She is empty." She sails on for Lundenwic, 269 km, with 35 d and five days of food. Nobody speaks. The wine was the lender's, and nothing is said of the debt.
- Hurts: on day three a stranger loses everything and the game does not blink. They will not know if the game is over, or what to do next. I could not judge sound or motion here.
- Change: danger gets a dress of its own, a dark bar and a gold edge, and the raiders get a bold mark on the chart. Put a price on tribute. Afterwards, a card where Ravn speaks: "The wine was his. It is still on the stick." with two choices, go on empty or turn for home.
- Size: medium.

**E2. Buying and selling give nothing back.**
- Saw: 16_sold_at_dorestat.png and 16_bought_at_dorestat.png. A sale changes one number in the bar and prints a three-line sentence in two places. They paid 39 for the first cask and 34 for the second. Then they asked 34.7 for a cask they had asked 46 for that morning. Their stock was 20. Nothing showed the fall coming.
- Hurts: trade is the game, and its one beat is a paragraph. The price falls hard after two casks, and the player learns it by losing. I could not judge whether that fall is meant.
- Change: let the row answer. The count ticks. A small "+35" rises by the purse. The row shows the next price before I press: "They pay 39, then 34." A coin sound, when the user has one that may ship.
- Size: small to medium.

**E3. I close ten kilometres of sea to talk to nobody.**
- Saw: 45_alongside.png: "Alongside. Time stands still while you talk." No master, no name, no face, no greeting. The corner tag says it again: "alongside: the clock stands". 46_alongside_prices.png: asking for prices gives "Prices at 4 ports, now in your ledger: Winaldum (5 days old), Ripa (13 days old) ..." No price is shown. Which window is "the ledger" is not said.
- Hurts: meeting another ship should be the liveliest thing at sea. It is a list of names.
- Change: a named master with one line of greeting, by his people. Put the two or three best prices in his mouth: "Wine fetched fifty at Ripa a fortnight back." Call the window by its name.
- Size: medium. (EncounterNodes.cs line 262, Encounter.cs line 220.)

---

## F. Ideas worth trying

**F1. Make the tally stick a thing on the screen.** Wursing's stick is the best thing in the opening. Draw it in Errands, and small in the top bar: one notch for each pound, cut away as I pay. When I come home he is on the quay, counting. It takes the place of "Owed 1920 d by Michaelmas, 29 September: 151 days left". Size: medium.

**F2. One line from Ravn at each first.** First sale, first toll, first night out, first loss, first sight of the sea. A line in the foot that fades. No window, no choice. It is the cheapest way to put "more personality in the read", and it tells the stranger what just mattered. Size: medium. The lines are the user's.

**F3. More parts from the Windows XP years.** Tick boxes in "The clock slows by itself for", in place of "On:" and "Off:" (70_the_clock_slows_for.png). A bar of blocks that fills while the sail is six hours at the sailmaker's. A box with its name cut into the line, for "Before she sails". A small picture in each window's title bar. Sunken fields in the top bar. Each is flair that costs little, and it is the look that was asked for. Size: small each.

**F4. Use the feast on the first morning.** 07_panel_faith.png: 1 May is "Philip and James ... a feast. Not to be done: work." The first thing the game asks is to have a sail mended and to trade. Let it bite once. The sailmaker asks more, or Ravn says "On the feast? He will want double." Then faith matters in minute five. Size: small.

---

## Where two pictures disagree

- The hold's total and its rows (07_panel_voyage.png, 17, 42). See A1.
- The distance to Dorestat: 12 km, then 40, then 52, over 13 km of sailing (40, 42, 43). See A2.
- "about 60 men" on the card, "about 65 men" in the log (47 and 50).
- Ravn's nature, and Ravn's face against his written looks (open_13b against 11_talk_with_ravn.png).
- The row lit at a question against what the game then began with (open_06_swear.png against open_11_free.png). The test pressed a key while the pointer lay on another row. The data is right. The screen misleads.
- "early morning" on the card, "morning" in the bar a moment later (open_10 against open_11).
- The Crew window says "4 men" and that they have no names (07_panel_crew.png). A card is spoken by "Torhtred, the lookout" (22_card_on_the_way.png). This belongs to the known work on the crew.
- The title a new player gets (open_02_title.png) against the test's (00_title.png). That is the test's older start, not a fault.

---

## Keep these

1. **Wursing and his stick.** "Radbod. Mm. One notch. ...There. You are on the stick." (open_08_lender.png), and questions that begin "Step into the light." A man who counts while he talks and does not care about you. It is the best voice in the game.
2. **The title.** The name as worn green slabs afloat on a purple sea, and nothing else on the screen (open_02_title.png).
3. **The toll man, Hroald and the evening song** (23, 33, 22). People with a law, a manner and an edge. They are the standard every card should meet.
4. **Picking a port and seeing its course drawn on the water**, then "Set this course" (17_voyage_port_picked.png, 19_port_clicked_on_the_chart.png). The clearest minute in the game.
5. **The twelve buttons that change with where she is** (Minster or Shrine, Cast off or Lie to), and **a save that comes back into the very card left open**, with a Continue line that says where and when (62_loaded.png, 63_title_with_continue.png, 64_continued.png).

---

## Score

**Would a stranger keep playing past the first voyage tonight: 4 out of 10.** The single change that would move it most: keep Ravn's five jobs on the screen as a list that ticks off, with the reason printed under a grey Cast off (B1). The landing slip that sets the purse beside the 1,920 (B4) is the next.
