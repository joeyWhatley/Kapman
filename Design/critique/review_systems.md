# Kapman: the systems review

7 October 2026. One of three critics' pages the user asked for. This one is by the strategy and trading veteran.
Nothing in the project was changed but this file. Unity was not run and git was not used.

**What I could see.** All 18 pictures in `Captures/chart` (01 to 18), each opened and looked at. The code of the frame
and every panel (`Code/Game/UI/Frame`), the market (`Trade/Market.cs`, `Deal.cs`, `Purse.cs`, `Hold.cs`, `Trade.cs`),
the other ships (`Sim/WorldSim.cs`, `TraderBrain.cs`), the word (`Known/Word.cs`), standing (`Reputation.cs`), the
crew's keep (`Voyage/Voyage.cs`), orders (`Ports/Commissions.cs`), the cards' effects (`Core/Encounters/Effects.cs`),
the start (`Chart/ChartWorld.cs`), the names of every test and the whole play test. The tables: `trade_rules.json`,
all 135 goods traded in 793, the 50 ports, shocks, knowing, reputation, voyage rules, commissions. 41 cards in full
(words, choices, effects) from seven files: `sea_793` 10, `chain_house_debt` 6 and most of a seventh, `rhine_frisia`
9, `crew` 6, `england_south_east` 5, `chain_storm_vow` 4, `harbour` 1. Dorestat's place file (its eras and its 16
people). Three chest texts (Hávamál 76, Alcuin's letter of 793, Ohthere). The book, the decisions, the queue, wiring
plan 8, both earlier critics, the money, cards, crossing and pop-up studies, the checks, the fixes, the four audits,
the wiki page, the ship view plans, the Steam page, the art list and DRAW FIRST.

**What I could not see.** The game running. So I cannot judge how it feels in the hand, how often a card really comes,
whether a save comes back whole, the sound, how the other ships behave over ten years (no run's report is on disk),
the wiki and the 3D view of the ship (not built), or what the money fix does in the real game (the soak is a Python
copy of the rules, by its own word). A number marked *(my sum)* is mine, worked from the code and tables. No web
search was made: what I say of other games' reviews is from memory and says so.

## 1. Where I stand

- A thousand hours in Crusader Kings, Patrician, Port Royale, Mount and Blade, Kenshi, Dwarf Fortress and Pirates. I
  write the long review other buyers read before they pay.
- I care whether the economy, time, risk and the other ships make decisions that still matter at hour fifty, and
  whether the numbers add up when I check them with a pencil.
- I do not trust a feature until I find the line of code that reads it. A game that says a thing and does not do it
  loses me faster than a game that never said it.

## 2. Pros (17, each new, or a different judgement of something already named)

1. **One market, one way of dealing, for you and for every other ship.** `Trade/Deal.cs`: the player and the 138
   ships buy and sell through the same function, unit by unit, into the same stock; the test
   `SimGroundworkTests.EveryMerchantDealsTheSameWay` holds it. In `09_port_trade.png` one cask sold at Traiectum
   moves what they pay from 36 d to 29 d and what they ask from 44 d to 35 d. Patrician moves a price by a whole
   town's stock; this does it with a size and two exponents, and the depth is cut to the boat. Four casks are a real
   quantity here. That is rare and right.
2. **The price rule can be read, bounded and tuned.** `Market.Price` is seven factors with floors and ceilings
   (scarcity held between 0.6 and 2.2; the glut a square root, so no price runs to nothing), and every number sits in
   `trade_rules.json` beside a note saying what it does. I could work every figure on the screenshots back to the
   table with a pencil. I have never managed that with Port Royale.
3. **The hold keeps what each lot cost.** `Hold.cs` takes the oldest lot first and carries its cost; the sale line
   says "more than they cost" or "less than they cost" (`Trade.Sell`), and the row's tip says "cost you N d each"
   (`TradePanel.cs`). In `02_frame_at_sea.png` the starting casks read "Your own from the start". A game that keeps
   my cost basis respects my hour.
4. **The rivals have purses, wages and bad days.** `WorldSim.cs`: a trader who cannot pay her crew sells what she
   carries at any price (`TraderBrain.SellToPay`), is paid off and laid up (`GoBroke`), and after ninety days "a new
   partner puts silver into her" (`NewPartner`). Going broke is a course of events, not a deleted row. That is the
   Dwarf Fortress habit, and it is why their prices can be trusted to mean something.
5. **The rivals can be wrong.** `TraderBrain.BelievedPays`: a price a ship saw fades by half in thirty days toward
   "what everyone knows", and she picks among her five best ports by weighted chance, so they do not all rush one
   gap. A field of traders with stale beliefs is the only kind a player can beat by knowing more. Most trading games
   give the others perfect sight or none.
6. **The same seed plays the same, and it is proved.** `SimTests.TheSameSeedPlaysTheSame`, `Markets.Hash` (a
   fingerprint of every stock, glut and closing), and `EconomyNightTwoTests.TheCatchUpIsTheSameHoweverItIsSplit` (a
   market brought forward in one step or in forty ends the same). For a world meant to run 273 years this is the
   plumbing that makes a bug findable.
7. **Markets die on their dates.** `Market.Ruin`, `Burn`, `SizeIn`; `shocks_793.json`; the tests
   `DorestadIsRuinedByItsLastSack` and `ABurningRebuildsAMarketSmaller`. The home port shrinks a step at each burning
   and is gone after January 863. Over fifty hours the place where the silver is moves, for a reason I can look up.
   Crusader Kings does this with borders. I know few trading games where a market can shrink or shut for good on a
   date from the record.
8. **Standing is local, travels at the speed of a ship, and pays.** `reputation_793.json` and `Reputation.cs`: each
   people weighs each deed its own way; a deed counts where it was done at once and elsewhere only "when its news
   gets there" (`ReputationTests.ADeedIsHeardFarAwayOnlyWhenItsNewsGetsThere`); at +100 you buy a tenth cheaper and
   sell a tenth dearer, which is the whole spread. A day's dealing is worth half a point. This is the one thing in
   the build that compounds over years, and it is the right thing.
9. **Some cards reach into the ledger.** By my count over the 56 files of the encounters folder: 195 `price_shock`
   effects, 479 `news`, 169 `toll`, 220 `commission`. `Deal.Sell` asks `EncounterRunner.TollFactor`, so "toll eased
   90" for paying the count on his day (`chain_house_debt` s03) is a real tenth off real sales for ninety days. Where
   a card does this, the story has a balance sheet.
10. **The calendar is reckoned right, and I checked it.** `16_faith_and_days.png`: Rogation 13 May, Ascension
    16 May, Pentecost 26 May, Friday "in 2 days" from 1 May. By the Julian rule Easter 793 falls on 7 April *(my sum)*,
    which makes 1 May a Wednesday and every date on that screen correct. Each day also says what it forbids. That is
    an instrument, the day it is made to bite.
11. **The field of rival sails is made of the real ships.** `Panels/FieldPanel.cs`, built since the earlier critics
    wrote: who is bound where you are bound, how far each has to run, and "the market will have met her cargo
    first". Because of pro 1 that sentence is true. A race against real hulls is worth more than any card.
12. **One true piece of seamanship is in, with its price on the button.** `FrameRoot.Rack.cs`: Wait "till the wind
    is fair for" your port, and the window says "The clock runs while you wait, and bread is eaten." The boat's
    speeds are real (4.8 knots on a beam reach, 1.75 at the oars: `crossing.md` section 1), so a fair wind halves a
    crossing and a foul one doubles it. Bread and pay against days: that is a decision.
13. **The windows are built for someone who lives in ledgers.** `FrameWindows.cs` and `05`, `06`: four workspaces;
    rows, columns or tabs; any window holds any panel; R, C, T, N, X and the arrows; the layout kept between runs
    (`FrameWindowsTests.TheArrangementIsKeptBetweenRuns`). It is plain C# under test. The skeleton is right for a
    player of Europa Universalis or EVE, even though what hangs on it is still thin (con 15).
14. **A quote, not a price tag.** `01_frame_dorestat_port.png`: "They ask 40 d, 27 here. They pay 34 d. By the
    cask." Both sides, the stock and the unit in one line, and under it where the word came from and how old it is.
    Only a price seen with your own eyes shows a number (`Word.cs`, rule 2). As a rule of knowledge it is clean. See
    con 4 for what it costs.
15. **Four coinages, and I would keep them.** `Purse.Rate` and the zones of `trade_rules.json`: the king's lands
    take a tenth off foreign silver for the mint, the lands of weighed silver three in a hundred. The earlier critics
    would cut this to one purse. From where I stand it is the most Patrician thing in the box, and it pays off in the
    best card I read: `chain_house_debt` s05, where the new heavy penny lets you pay 480 by tale or 367 by weight and
    the count likes you less. Keep it, and show it (con 7).
16. **The debt chain is a set of credit instruments, not only a story.** `chain_house_debt` s03 and s06: a year's
    grace for the boat's work (only if you are "known"), the boat pledged, the widow's surety ("six score pennies in
    my hand now and a third of your next three cargoes", only if she is easy with you), the rival buying your tally
    from the count, an appeal to the bishop on usury. Each is a different way to carry a debt. That is finance as
    play, and the gates are standing and feeling, which the game does keep.
17. **A bad card file no longer silences the deck, and a play test leaves pictures.**
    `EncounterChartTests.AFaultyCardFileShutsOnlyItself`; `ChartGameTests` now walks a new game through the frame,
    answers the toll man, sells a cask and saves the 18 pictures I judged from. The earlier critic's "the tests test
    something else" is half answered. The other half is in con 13.

## 3. Cons (18, each new, or a different judgement)

For each: how bad, and the smallest mend.

1. **The first release is about 230 hours long, and the book says twelve to fifteen.** A game day is six real
   minutes at 4x (`money.md` section 3; `crossing.md` section 1). From 1 May to Martinmas is 194 days: 19 hours at
   the fastest clock, 78 at 1x. Twelve such years, 793 to 805, are about 230 hours with every winter as one board
   *(my sum)*. `GAME_MAKER.md` stage 3 says "Twelve to fifteen hours for a life": that figure was written with "Sail
   on" in it, which ruling 1 of 7 October struck, and before ruling 5 let the clock run in port. One sailing season
   is already the whole budget the book gave a life.
   *How bad:* it would stop me finishing, and it stops you testing: nobody can play-test a 230-hour life alone.
   *Smallest mend:* no new rule for the clock. Cut the release to the clock: 1 May 793 to the first Martinmas board is
   the game, with the reckoning as its ending and the heir as its last page. The other eleven years are the next
   releases.
2. **After the money fix an hour of trading makes eight pennies.** By the study's own tables (`money.md` sections 1
   and 7, the tuned set, which the user approved as ruling 14): the sensible trader makes 8 d in the middle ten days,
   and ten game days are one real hour at 4x; a laden leg clears 13 d in the mean; 29 laden legs in 100 lose silver;
   55 legs in 100 carry nothing; "Dorestat asks 60 d and Lundenwic pays 59 d. Nothing." Even the all-knowing trader
   makes 71 d an hour. A second hull is 300 d, a coaster 1,200 d, a keel 3,000 d. The debt was made a stake by
   making the trade not pay. The study says so itself ("You cannot have a 480 d debt, a 150-day season and 30 d legs
   together") and chose the debt. I would choose the legs.
   *How bad:* a point and a half. In a trading game the trade is the verb, and its middle result is a rounding error.
   *Smallest mend:* one number and one re-run. `SimRules.minMargin` from 0.12 to about 0.25, so the others stop
   closing a gap while a quarter is still in it; then run `soak.py` again and set the debt (or its day) to whatever
   keeps Michaelmas in doubt.
3. **Every click in the king's lands is rounded against you.** The frame deals one unit a click
   (`TradePanel.DealOne`). `Purse.Pay` takes whole coins rounded up and gives change only in whole coins rounded
   down, so a sack of bread asked at 5.2 d (`01`) costs 6. `Purse.Receive` rounds a sale to the nearest coin: the
   line in `09` reads "for 36 d, less 3.6 d toll: got 33 deniers", which is not what 36 less 3.6 comes to. The
   crew's pay goes the same way at every call (`Voyage.InPort`): half a day's pay of 0.5 d costs a whole denier. The
   other ships deal five units at a time and start in cut silver, so they barely feel it. At 65 calls a season the
   pay alone loses about 30 d *(my sum)*, a tenth of the tuned season's whole gain of 286 d.
   *How bad:* a point once the margins are thin. Today it is hidden by fat margins.
   *Smallest mend:* let Buy and Sell take a count (Shift for five, Ctrl for all that fits) and settle once; give
   change under a penny in cut silver.
4. **"Pays poorly" is measured against a number the player is never shown.** `Word.PaysKey` and
   `knowing_793.json`: "well" is 1.15 times the good's table value or more, "poorly" 0.85 times or less. Rhenish wine
   is 60 d in the table, so anything under 51 d is "poorly". In `08_voyage_pick.png` the word is "Traiectum was
   paying poorly for Rhenish wine"; in `03` Traiectum pays 36 d and in `01` home pays 34 d. The word told me not to
   go where the price is higher than where I stand. In a region that makes wine every port "pays poorly", so the word
   cannot choose between them; after the money fix Lundenwic at 59 d will read "the usual". The bands are fifteen
   parts in a hundred wide and the margin the other ships leave is twelve.
   *How bad:* a point. The game's best idea gives its worst advice in the first ten minutes.
   *Smallest mend:* say it against where she lies ("more than here", "less than here", "much the same"), and let a
   price told at first hand be "about 36 d" with its age.
5. **The measure is an envelope, not a forecast.** `FramePassage.cs`: the low figure is a fresh breeze on the
   quarter the whole way, the high one light air dead ahead the whole way, both sailing night and day. So "About 2
   to 5 and a half days for your boat, by the wind" (`04`) is the same sentence every day of the year, and "by the
   wind" is not true of it. Today's wind is only a word under it. "Half a day" is said of 8 km, 16 km and 30 km alike
   (`02`, `10`). Bread is for 7 days and Ripa is 563 km: I cannot tell from the frame whether the sack lasts. And
   "fair for Lundenwic" is judged on the first 8 km of the course (`WindNow`), which is a river bend.
   *How bad:* half a point. It is the instrument the whole frame is built round.
   *Smallest mend:* one more figure between the two: the days if today's wind holds along the whole course.
6. **Numbers on one screen do not agree with each other.** `10_lookout.png`: 16 km to go of a 30 km course, home
   "8 km by water" behind her, and the lookout says home "lies 9 km to the south-east": the straight line is longer
   than the water. The cause is in `VoyagePanel.Fresh`: the list's distances are kept until she has moved 5 km.
   `14_a_dane_at_ripa.png`: the market is in grams of silver and the purse above it in d. `03`: the bar says the
   debt is due in 364 days and the count's own card (`s01_tally`) schedules his day in 100.
   *How bad:* a scratch each. Together they teach an expert not to trust the frame's figures.
   *Smallest mend:* the list reckons again every kilometre on a river; the bar speaks the zone's own unit.
7. **The purse shows the wrong figure and hides the coins.** The bar's "86 d silver" is `Purse.Silver`: all the
   silver by weight. What it buys at Dorestat is `Purse.Worth`: 81 d *(my sum)*, because 25 pennies, 6 dirhams and
   30 g of cut silver each lose a tenth there (`trade_rules.json`, start). `Deal.Buy` reckons on the 81 while the
   bar shows 86. No panel among the nineteen lists the coins.
   *How bad:* a scratch today; it is why pro 15 reads as a tax and not as a system.
   *Smallest mend:* the bar shows what it buys here; a hover lists the four heaps and what each loses.
8. **The field sees everything, where every price is hearsay.** `FieldPanel.Field` reads `game.World.ships`
   directly: every ship in the world bound for your port, to the kilometre, wherever she lies. A game whose promise
   is "you never see a true price" hands out a perfect radar of the competition.
   *How bad:* a scratch now, half a point when the field starts to matter.
   *Smallest mend:* list only ships seen leaving her own port in the last three days (`Word.cs` rule 6 already finds
   them) and ships a talk has named, each with the age of the word.
9. **Hers is the only hull on the North Sea that cannot be lost.** `WorldSim.cs`: any other ship under way in a
   gale is lost with a chance of 0.0004 an hour times her fragility. `ChartBody.cs`: the player's boat "lies to" and
   waits. The sea deck's own note says "No card takes the boat: the sea itself does that, in the ship's physics",
   and the chart has no ship's physics. So risk is time and nothing else, and the 793 lines of weather reach the
   player as a slower dot.
   *How bad:* a point. Without a loss I can price, the wait for a wind (pro 12) is the only choice at sea.
   *Smallest mend:* the same dice for her, behind the deck's own good rule that every danger warns first and wounds
   before it kills: a started strake, then a split sail, then the boat.
10. **The deck sails a second boat that the simulation never meets.** Of about 19,300 effect lines in the cards,
    6,829 set flags and 4,736 move a person's feeling *(my count)*: six in ten write to the deck's own memory. The
    flags `sea_reefed`, `sea_leak`, `sea_sail_weak` and `crew_on_shares` are read by no C# file. So "the sail has
    begun to tear" slows nobody; "a tenth of each season's gain, shared among them at Martinmas"
    (`crew.pay.wage_or_share`) changes no wage and gives heart for nothing; "Go or wait" (`crew.council.wind`)
    neither goes nor waits. The earlier critics counted the verbs that are missing. My charge is narrower: where the
    deck and the simulation both keep a thing (her sail, her leak, the men's pay), they keep two different things.
    *How bad:* a point. An expert finds it on the second gale and stops reading the failure texts.
    *Smallest mend:* four readers in code: reefed is seven tenths of her speed; a leak spoils one lot a day; a weak
    sail caps her speed; shares pay a tenth of the season's gain at Martinmas.
11. **Orders are broken three ways.** (a) A card that says you took an order only posts it:
    `GameEffects.Commission` never sets `mine`, so it must be taken again in the Tasks window, and until then it has
    four chances in a hundred a day of being "filled by another" (`Commissions.Daily`). (b) Nothing checks the hold.
    The boat carries four casks; 35 of the cards' 70 wine orders ask for 6 to 16, a monastery's own orders ask for 3
    to 8, there is no part delivery, and a broken order costs standing. (c) 86 of the cards' 220 orders carry a
    premium of 1.0 or more, 49 of them 1.5 or more, paid on top of the market's price and free of toll. The minster
    at Sarre orders four casks at premium 1.5: about 600 to 800 d for one hold *(my sum)*, more than the debt. The
    cap of 60 d a card counts only `silver +N` and does not touch these.
    *How bad:* a point; (c) alone undoes the money fix.
    *Smallest mend:* the card sets `mine`; an order larger than `Hold.RoomFor` is greyed with the reason; premiums
    capped at 0.5 and tolled like any sale.
12. **A stat's whole range is worth ten points in a hundred.** `Effects.cs`: the chance is the card's own plus
    0.05 for each pip over three. The odds are shown as three words whose bands are 25 points wide
    (`knowing_793.json`), so most pips never change the word on the card. The steersman's child's gift, "You read
    water", is five points, and `15_yourself.png` prints it as "A check 'by crew.seamanship_best' counts one pip
    better". Ruling 18 caps all marks at two pips: ten points again.
    *How bad:* half a point. Three choices "hold for the whole life" and move a die by a twentieth.
    *Smallest mend:* a tenth a pip, or five words for the odds.
13. **A second life is the first life in other colours.** `14_a_dane_at_ripa.png` and the comment in
    `ChartWorld.StartAt` ("the boat, the cargo and the debt are the same at every home"): a heathen Dane of Ripa owes
    480 d to a Frankish count, is "a stranger (0)" in his own town where the Frisian is "known (+25)", "saw ship's
    bread pay 4.3 d at Dorestat, this morning", and holds four casks of Rhenish wine in the one port of the twelve
    that pays most for them and takes no toll: about 335 d on today's untouched table and about 230 d after the
    pre-run, for four clicks on the first morning, by the money study's two tables. `KapmanGame.worldSeed` is still
    793 and the chronicle is dated, so the same ships meet the same famine. The play test plays one day on one seed;
    the ten-year run is marked Explicit and is not in the suite.
    *How bad:* a point off replay, which is what my kind of player buys.
    *Smallest mend:* one row for each people in a starts table (cargo, creditor, standing at home, what is known);
    roll the seed on New game.
14. **The fleet can only shrink.** A ship lost in a gale is `Lost` for good and nothing makes another: the only
    `new SimShip` outside the start are raiders and the loading of a save. The money study calls the count of
    traders "the strongest number in the economy" (42 ships: the debt is paid 95 times in 100; 70 ships: 30 times).
    So the world drifts easier every year, by a rate nobody has measured.
    *How bad:* a scratch in one life; over the house's 273 years it empties the sea.
    *Smallest mend:* a lost hull gets a new partner after a year, as a broke one does after ninety days; put the
    ten-year run's two checks in a weekly run.
15. **The ledger an expert wants is not there, and the windows are too narrow for the one that is.** The Word
    window shows only the goods already aboard, three at most, at sixteen ports (`WordPanel.cs`). There is no way to
    ask "where is honey cheap?" before buying. In `01` the Trade window shows two goods at a time of about a dozen;
    in `05` four columns break "Medemel-acha", "Walichru-m" and "quernstone-s" across lines and wrap one quote into
    six.
    *How bad:* half a point. I would keep my own spreadsheet beside the game, which Port Royale also made me do.
    *Smallest mend:* a Ledger panel: goods down, known ports across, the price as a number where seen and a word
    where told, coloured by age; and one line a good in Trade.
16. **The world the text describes is not the world the numbers run.** Three cases. In `05` one column says
    "finished quernstones pile up unsold" and the next says "Mayen quernstones. None on offer." The same text says
    "English ports are shut to Frankish ships", and the money study's best run of the year is Frankish wine from
    Quentovic to Lundenwic: nothing in `Deal` or `TraderBrain` knows of the quarrel. The salter at Walichrum says "at
    the hearth it is two pennies the sack ... at Dorestad four. You have a boat. Do the reckoning." By the table
    Dorestat pays about 1.6 d for a sack it neither makes nor wants, once the famine is on grain only *(my sum)*.
    *How bad:* a scratch each, and each one costs the text a little of my trust.
    *Smallest mend:* a price in speech is a slot filled from the market, as the word on the item already is; the
    quarrel is a closing of the English markets to Frankish hulls until 796.
17. **A number key answers a card the instant it opens.** `FrameRoot.Keys`: 1 to 4 change workspace.
    `FloatsKeys`: with any floating window up, 1 to 9 pick its choice at once. A card opens about two seconds after
    she moors, which is when my hand is on the number keys. The pop-up study asked for half a second's wait before a
    card takes a key (`popups_and_windows.md`, rule 7). It was not built. In the same family: the Tasks window lets
    me pay the count's debt at any market in the world, with no count in sight.
    *How bad:* a scratch, until it pledges the boat to the count by mistake.
    *Smallest mend:* the half second; and the debt paid at Dorestat only.
18. **The one picture of the measure shows the course on the wrong map.** `04_measure.png`: the port marks and the
    yellow line have moved to a wider view and the picture under them has not. Lundenwic sits in the sea off
    Holland, Dommoc and Gipeswic on the Dutch dunes, Quentovic in Brabant, and 434 km of sea is drawn across green
    land. I cannot tell whether the player sees this or only the test's camera does.
    *How bad:* a scratch if it is the capture; a point if it is the game, since `steam_assets.md` names this very
    view as a store picture.
    *Smallest mend:* the test waits for the tiles before it saves, and somebody looks at the picture. The same pass
    can take the raw names off the screen: "charter toll dorestad" (`03`), "crew.seamanship_best" (`15`).

## 4. The review

*Written as a long review in the manner of Wargamer or Strategy Gamer, the kind that is posted again as the Steam
review other buyers read first. It reviews the first release as the plans describe it: one life, 793 to 805, twelve
ports. I mark what I saw [seen] and what rests on a plan [plan]. A plan that has code, a fix file, a ruling or a
place in the queue behind it I count in full. A hope of the book with nothing behind it yet (the order and its
switches, the signs, the reveal, the reckoning, rationed talk, four named men, the winter board, the heir) I count at
half, because twice already the build has gone somewhere other than the book. What I say other games' reviews
praised or faulted is from memory.*

### Kapman: the ledger of a small boat

**What it is.** Kapman is a trading game with no battle screen, no town view and no helm. You are a Frisian of
Dorestat in the year 793 (or a Frank, an Englishman or a Dane) with a six-oar boat that holds four casks of wine,
four men, bread for a week and a debt of 480 silver pennies to the count. You run her from a frame of tiled text
windows laid round a chart of the North Sea, with a small view of the boat in one corner [seen; the 3D view is a
plan]. On my shelf it stands between Patrician III and Port Royale for its markets, Sunless Sea and Vagrus for its
text and its payroll, and 80 Days for its cards. It differs from all of them in one thing, and that thing is the
reason to buy it: you never see a price you have not stood in front of. Every other price is something a toll man
or a skipper said, with his name and its age on it [seen].

**The first hour.** Three picks make you: a people, a faith, a past [seen]. Then the frame. The market asks 40 d
for a cask and pays 34; in the tuned game it will ask nearer 60 [plan: the money fix, approved]. You choose a port
in the Voyage window, the frame says "about 2 to 5 and a half days", and you press Cast off [seen]. Then you wait.
By the designer's ruling the clock has three speeds, the fastest is four game minutes a second, and nothing is ever
skipped. A day at sea is six minutes of your evening and London is a quarter to half an hour away. The plan fills
that time with a card every minute or two, a lookout, a lead line, a field of rival sails and the boat on her own
water [seen in part; plan for the rest]. By the game's own money study the first hour holds one crossing and the
sale of the count's four casks.

**What works.** The market is the best I have read in a game of this size. As I recall, Patrician III was praised
for towns that ate and made things and prices that answered, and faulted for an interface that hid how. Kapman's
model is of that family and smaller in a useful way. A price moves with every single cask. You and 138 other hulls
deal through one rule. The others carry stale beliefs, go broke, lay up and are bought out [seen in code]. As I
recall, Mount and Blade tells a trader with enough skill which runs pay; Kapman turns that same knowledge into a
sentence somebody said at Dorestat a few days back, and makes you wonder whether it still holds. That idea is built,
and I know no other game that has it.

The frame is made for people who like frames. Crusader Kings III was praised in nearly every review I remember for
tooltips that open inside tooltips; Kapman's planned wiki, where any word on the screen opens its page, is the same
idea turned on history [plan]. The tiled windows will please anyone who has rearranged EVE [seen].

And some of the writing has a balance sheet. The count's debt runs through a year of cards in which you may pay by
tale or by weight, pledge the boat, take a widow's surety for a third of three cargoes, or go to the bishop about
usury [seen in data]. The calendar is right to the day [seen, and checked by hand].

**What does not.** Time, first. The complaint I remember best from the reviews of Sunless Sea is the slow boat: long
quiet water and early voyages sailed again and again. Sunless Skies was credited, as I recall, with mending the
pace. Kapman has chosen the slow boat on principle. At six minutes a day a twelve-year life is about 230 hours by my
sum, where its own book hoped for fifteen.

Second, what the hours pay. In the tuned economy the others close every gap to about an eighth, and the game's own
study has a sensible trader making 8 d in the middle hour, with a debt of 480, a second hull at 300 and a real ship
at 1,200. As I recall, reviews of Vagrus faulted an early economy that kept the player poor for a long time. This one
is thinner. As I remember it, Patrician lets you lay down a second ship within the first evening. Kapman's first
release gives you one boat for twelve years [plan].

Third, the leg itself. Today there are two things to decide at sea: where to, and whether to wait for a wind
[seen]. The order with its five switches is on paper [plan, counted at half]. Nothing at sea can take the boat
[seen], and the damage the cards speak of lives in flags the simulation does not read [seen]. As I recall, Port
Royale 4 was faulted for sea battles that were the weak half of the game. Kapman has no weak half because it has no
second half: when the trade is thin there is nothing to do with the evening but read.

Fourth, the game gives the player softer numbers than it runs on: "pays poorly", "2 to 5 and a half days", "likely"
[seen]. King of Dragon Pass and Six Ages were loved for their writing and, as I recall, faulted for hiding the sums
under it. Kapman hides its sums behind good manners, and my kind of player will open a spreadsheet.

Fifth, a second life. Every new game is the same world on the same seed, with the same dated famine, and the Dane
starts with the Frisian's cargo and the Frisian's count [seen].

Last, the page will say, as Steam now asks, that the code and much of the text were made with an AI model
(`steam_release.md`). Buyers like me then go looking for the seams, and here they are in the sameness, not in any
one line: 36 of the 43 chains are fourteen cards long, 931 of 1,392 cards have exactly three choices, and 96 in 100
of the changes to the crew's heart are five hundredths or less [seen, counted]. The best cards do not read that
way. The average of them does. Mount and Blade II in early access was praised for its battles and faulted, as I
recall, for systems that were only names. A store page for Kapman must not list Hail, Rest, Offer, the House and the
Crew while they still answer "not yet".

**Who it is for.** Players who read the Patrician manual, kept a notebook for Port Royale, liked Roadwarden's
budget of days and Pentiment's patience, and think an hour spent waiting for a north-east wind is play. It is not
for anyone who wants Pirates, a fleet by Friday, or a fight.

**Verdict.** The most honest small market I have read, a way of knowing prices that deserves to be copied, and a
frame worth living in, set in a game whose clock asks more hours than its trade can pay for.

**6 out of 10** (Wargamer's scale, out of ten, as I recall it). On Steam: **Recommended**, with every warning above.

*What this score rests on.* Seen: the market, the other ships, the word, standing, the frame, the calendar, the
cards I read. Planned and counted in full: the money fix, the sea meetings, the routes, text sizes, autosave, card
urgency, the wiki, the 3D view, the art stand-ins. Planned and counted at half: the order, the signs, the reveal,
the reckoning, rationed talk, the winter board, the heir. Not judged at all: how any of it feels in the hand.

## 5. The same review's verdict if it shipped as it is today

- You can make a person, read a market, set a course, cast off, wait for a wind, answer a toll man and sell four
  casks. That is a working model of a good idea. It is not yet a game.
- Nothing can go wrong. The debt does nothing on its day; the boat cannot be lost; raiders, guard boats and other
  sails do not meet her on the chart; four of the twelve ports cannot be reached.
- Three of the eight buttons write "not yet" in the log, and Bonds, Crew, Ship, House and Faith say in their own
  words that they are not kept yet. The honesty is to the game's credit. The count of them is not.
- The first trade any player makes, home to Traiectum, loses a fifth by design, and the word on the item told him
  the wrong thing about it.
- The seed is fixed, there is no autosave, one "Talk to people" gives the prices of fourteen places, and the text
  is too small. All of these are known and queued.

**3 out of 10.** On Steam: **Not Recommended**, and I would write "come back in a year" under it and mean it.

## 6. What would move my score most

In order. With all five I would give 8.5.

1. **Fit the release to the clock: one year, ended properly.** 1 May 793 to the first Martinmas, the reckoning as
   the ending, the heir as the last page; about twenty hours at the clock as ruled, which one person can play-test.
   **+1.**
2. **Make an hour of trading pay.** Leave wider gaps for the player (`minMargin` toward 0.25), scale the debt to
   what the soak then says, and close the doors that pay more than trade does: order premiums, untolled orders,
   cards over a leg's worth. **+0.5.**
3. **Make the leg a decision with a loss in it.** The order's first three switches (when to cast off, which way,
   what at dusk); the same gale dice for her as for the others, warned first; and the boat's hurts kept in the
   simulation, not in the deck's flags. **+0.5.**
4. **Give the expert numbers he can use.** A ledger of goods by ports; deals by the lot with one settling; the word
   said against where she lies, and "about 36 d" for a price told at first hand; the days if today's wind holds.
   **+0.25.**
5. **Make a second life differ.** A starts table for each people (cargo, creditor, standing, what is known), the
   seed rolled, one rival with a name among the real ships, lost traders replaced. **+0.25.**
