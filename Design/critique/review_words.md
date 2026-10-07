<!-- 7 Oct 2026. One of three critics the user asked for. Read and looked at, not played: Unity was not run, git was
not used, and nothing in the project was changed but this file. "My count" means a small script run over the data
files that day (the scripts are not kept in the project). No web search was made: what I say of other games and
their reviews is from memory and says so, and no review is quoted. Lines in quotation marks from Kapman are copied
from its own files or read off its captures. -->

# Kapman: the words and the history

The narrative and history critic's pros, cons and review.

## 1. Where I stand

I review games that are led by their writing, and I read medieval history for pleasure: annals, saints' lives, laws.
I care whether the words are worth reading at length, whether the period is lived in or worn as a costume, and what
a game says about its people when it shows faith, slavery and violence.
I also read with poor eyes and little time, so I count how much a screen asks of a reader before it gives anything back.

## What I saw, and what I did not

**Seen.** All eighteen captures of this morning's test in `Captures/chart`. They are the only place I saw the game.

**Read in full, words, choices and effects: 78 cards** from thirteen files: the whole of `chain_house_debt` (14), the
toll man, nine sea cards, eleven crew cards, fourteen port cards of `rhine_frisia`, eight of `chain_silver_road`,
four of Lundenwic, one of Duiblinn, three each of `chain_storm_vow` and `chain_old_gods`, two of
`chain_dorestad_834`, four of `long_793`, four of Ripa. Dorestat's whole place entry (summary, eight eras, sixteen
people, twelve sites, ten sayings, eight easter eggs). Four texts of the chest. The tongues file and its rules. One
hour of history.

**Read as code:** the frame and its panels, `PersonTalk`, the card float, `EncounterSession`, `Self`, `Trade`, the
play test `ChartGameTests` and what it asserts. **Read as plan:** the book, the rulings, the queue, the mock-up, the
faith page, the wiki page, the ship view plans, the Steam page, both earlier critiques and today's studies.

**Not seen, so not judged:** the game moving; a crossing at any speed; a chain card on screen (the captures show one
card, the toll man); the turning line turning; the chest open; any sound; any of the maker's own art; whether the
420 cards that can open in the first release share the faults of the 78 I read (I chose mine from every kind of
file, not from that list).

## 2. Pros

Each is one thing, where I saw it, and why it is good from where I stand. None repeats the earlier critics; where
they praised the same file I say what I think is different.

1. **The law sits on the thwart as a third voice.** `crew.faith.lent_meat`: the Dane has pork on the fire in Lent,
   and between the two men's lines the card sets the king's Saxon law, that a man who eats flesh in the Lenten fast
   out of contempt "let him be punished by death", and then: "The mate has stopped chewing." The mate is a baptised
   Saxon. The violence of the conversion arrives as a silence at a meal. This is how a period is worn lightly.

2. **A real vow in its own words, and a heathen's answer to it.** `crew.faith.vow`: the mate says the Old Saxon
   baptismal vow as it stands in the manuscript, "Thunaer ende Uoden ende Saxnote", and the Dane says: "He named
   them out loud to give them up? That is not how I would stop a god listening." Two beliefs in three lines, and
   neither is mocked.

3. **The burial card.** `crew.death.body`: "The boat is seven paces long, and there is nowhere aboard her that is
   not beside him." Then, for the sea: "The cloak goes down pale, then green, then is not there. The mate cuts the
   dead man's notches from the tally and keeps the piece." Death has bookkeeping. These are the best sentences I
   read in the project.

4. **What the debt chain says about its people.** The earlier critics praised `chain_house_debt` as a set of
   dilemmas. I praise its argument. Every ending is a way of belonging to someone: a hen at Michaelmas for the
   count's ground (`s11`), hands between the bishop's (`s13`), a third of the boat to Thiadbod (`s14`), a letter
   that puts you under the king's word (`s12`). Nobody ends free, and the bishop says the price aloud: "But a man is
   not a stick ... Think before you kneel." That is what freedom was in 793, and I know no game that says it.

5. **The stone on Walcheren.** `harbour.chain_old_gods.s02_stone` and `s03_quarrel`: a gale uncovers an altar of a
   woman with apples, a dog and a ship's prow. The shepherd's wife tells how the bishop broke its fellow and why
   "we are Christians here". The steersman calls it an idol. The hand calls it "a tally. A man promised something
   in bad weather and paid when he came home, and had it cut in stone so nobody could say he had not." A vow read
   as a debt is the one real idea about faith in the game, and it is a merchant's idea.

6. **The tongues file will not invent.** `story/tongues_793.json` and `talk_tongues_793.json`: every line is a real
   line with where it stands; each has a year before which it may not be spoken (the Heliand not before 830); 95 of
   the 318 words and 7 of the 111 lines are marked for someone who knows the tongue to check (my count); and a
   Frisian is given no line of his own because "nothing in Frisian is written down before the thirteenth century".
   Saying what cannot be known is rarer than research.

7. **Understanding is a skill you can watch.** `FrameTongues.cs` and the play test: Ravn's saying comes up in Norse
   and turns to English piece by piece, and a Frisian who follows "three parts in ten of Norse" is left with pieces
   that do not turn ("You half follow it: some of it is lost on you"). I did not see it move. As a design it is the
   nearest thing here to what Heaven's Vault did with a dead language: the tongue is play, not flavour.

8. **Easter is right.** Capture 16, the Faith and days window: 1 May 793 a feast (Philip and James), Friday in two
   days, the Rogation days on 13 May, the Ascension on 16 May, Pentecost on 26 May. I worked it by hand: 1 May 793
   was a Wednesday, the paschal moon fell on 1 April and Easter on 7 April. It is right. A game that reckons Easter
   can be trusted with the rest, and a year of feasts and fasts is how these people told time.

9. **Sin and law are kept apart.** The same window, on Sunday work: "a sin everywhere; forbidden by the king in
   Francia only from 809". And on oaths: "In Kent a stranger clears himself at the altar; in Lundenwic a purchase
   is good if the king's wic-reeve saw it." `harbour.wine_over_the_side.lundenwic` then makes that law a choice: a
   cask bought "with nobody to witness". Law here is the grain of a deal, not a wall of text.

10. **The slave trade is written without spectacle.** `chain_silver_road.s12`: the Frisian boy who keeps the
    dealer's tally "cuts six notches without looking up" and says: "Six. Four men and two women. One of the women
    speaks my tongue. I have cut it as six." At Sliesthorp (`chain_dorestad_834.s05`): "Two Dorestad skippers are
    selling wine thirty paces off. They have seen the bench. They are very busy with a cask." The arithmetic and
    the bystanders carry it. Nobody tells me what to feel.

11. **A town with a death in it.** `places/dorestad.json`, the eight eras: from "The hungry year" through "Rorik's
    town" to "Clay over the plots" and, last, "Children find green pennies with a ship on them after the plough."
    And the brooch in the well at De Heul: "Nobody comes back for it. It lies there until 1969." The long road to
    1066 is planned; this page is why it would be worth walking.

12. **Coin against weight, as character.** Capture 01 against capture 14: Dorestat prices "in d" with a toll of "10
    in the hundred"; Ripa prices "in g silver" and "No toll is taken". The goldsmith of Ripa: "Silver has one face,
    and it is the inside." The Frisian in `crew.chain_silver_road.s10`: "A coin should have someone to hang." The
    first critic wanted one purse for the sake of play. He may be right about the purse. The words must stay: this
    is the best piece of cultural drawing on the screen.

13. **The data says what is invented.** Every person carries `historical` and a source. Hrotberct the wax seller is
    the "black merchant" of Alcuin's poem, who gives no lodging; Catla of Birka is Rimbert's; the count of Dorestad
    "is invented" and the card's note says so. Card notes mark what is doubtful with (?). The wiki plan keeps "In
    the game" apart from "What is known". A player can always tell the game from the record, which is the respect
    Pentiment showed its reader.

14. **The annal voice.** `long.dorestad.sacked_834` opens: "Some of the people they killed, some they carried off,
    and part of the place they burned." That is how a ninth-century annalist writes, and here it is the news. The
    last card of all, `long.william.midwinter`, offers: "Lay the boat up for the winter, and read the chest through
    from the beginning." The game knows what its ending is.

15. **A faith is duties and doors.** Captures 12 and 13, the first window: under each faith "It asks: ..." and "It
    opens: ...". Under the old gods: "Halls and markets in the north, where a Christian is a guest and you are a
    neighbour." No bonus, no number. The pasts are eight lives in a line each ("A captive come home", "A foundling
    of the house of God").

16. **A person in words.** Capture 15: Name is "How you look and who stands behind you: your cloak, your washed
    hair, the men who vouch." The levels are words: "halting", "steady enough", "has heard things". And one line I
    would play for: "Someone knows you: The pilot at Witla sailed with your father and taught you the bars."

17. **Riddles are answered with what you know.** `Chest.cs` and `riddles_793.json`: you answer by putting a thing
    you have seen or heard of onto the riddle, and a riddle you cannot answer "rests" until you learn something
    new. The riddles are real ones (the ale riddle of the Norse king's hall, Symphosius on money and the anchor,
    Aldhelm on salt). Knowledge as a key is the theme of the whole game, made into a toy. Not seen working.

18. **The gospel leaves at Ripa.** `harbour.leaves_by_the_handful.ripa`: a goldsmith wraps brooch moulds in painted
    calfskin from "two boards" a north-way crew sold him. "They take the heat well. Nine left." The raid on the
    holy island, seen from the buyer's bench, and gated to come only after July 793. One card does what a cutscene
    could not.

## 3. Cons

Each is one thing, where I saw it, how bad it is, and the smallest mend. "Known" means an earlier study found the
fact; I then say what I judge differently.

1. **The deck is written to one man, and the first window offers eight lives.** Captures 12 to 14 let me be a Dane
   of Ripa, of the old gods, or "The widow who kept the boat". The cards speak to a baptised Frisian skipper of
   Dorestat. "shipman" stands 174 times in the card files (my count). At Ripa, to a Dane who starts at Ripa:
   "A Frisian boat, and small ... Whose guest are you on this bank?" and "New face, Frisian boat, four men." The
   steward says "his mooring rope". One chain's words call me "a man who had not sat a horse since harvest".
   Capture 14 shows the rest: the Dane owes "480 d" to nobody named and is "a stranger (0)" in his own town.
   Today's card study found that no card asks who you are (known) and proposes tags. I say tags will not do: the
   words do not merely fail to ask, they answer wrongly.
   *How bad:* it would stop me playing as anyone but the default, and it costs a point for making a promise on the
   first screen that the next screen breaks.
   *Smallest mend:* for the first release offer the Frisian of Dorestat only, man or woman, and write the woman's
   address into the sixty first-hour cards. Open a people when its home port's cards have been read with that
   people in mind.

2. **Lindisfarne is told five weeks before it happens.** The game begins on 1 May 793 (the play test asserts it,
   and asserts that on that day "Lindisfarne has not fallen yet"). The raid is 8 June. But the priest of Dorestat
   greets you with "I have a letter from York, thirteen days old, and I wish I had not read it", his first rumour
   gives the day ("the sixth day before the ides of June"), and `harbour.book_for_utrecht.dorestad` (weight 0.6,
   `year < 795`, no month) has him say "Heathen men have been at Saint Cuthbert's on the holy island". A person's
   lines are gated by the year and nothing finer. The words were written for a start on 21 June.
   *How bad:* a point, in this game above all. Its whole promise is that news is true and has an age.
   *Smallest mend:* a month on that card, as `leaves_by_the_handful.ripa` already has (`year > 793 or month >= 7`),
   a way for a person's line to carry a first day, and a pass over the Dorestat lines for "thirteen days" and the
   like. Or put the start back to 21 June.

3. **A card does not say who is speaking.** `FrameRoot.Floats.cs`, `CardSpec`: each line goes in as bare text. The
   head of the card names the first speaker and adds the others after "with". 740 of the 1,392 cards have two or
   more speakers (my count). In `s05_new_penny` three men speak five to seven lines. In the faith cards the whole
   point is which of two men said it. Capture 03 shows the look: three paragraphs, no names.
   *How bad:* it would stop me reading the best cards in the game.
   *Smallest mend:* put the speaker's name or office before each line, small and in another weight. The session
   already holds it (`ShownLine.who`).

4. **Nine choices in ten end on a ledger line.** Of 4,456 choices, 473 have a written outcome, all of them in the
   sea and crew files. The chains have none for 1,887 choices, the port cards none for 1,867, the long deck none
   for 225 (my count). Capture 11 shows what the player gets instead: "The toll is taken on what you sell here.
   Liudbrand thinks better of you (easy)." So after "Kneel, and put your hands between his" the game will say that
   a bishop thinks better of me. The first critic said the panel hides the outcomes that exist (known). Nobody said
   that for the story cards they were never written.
   *How bad:* a point. A choice with no sentence after it is a button.
   *Smallest mend:* one sentence for each choice a first-release player can make that cannot be undone; show the
   written ones that exist; where there is none, show only what was paid or gained and drop "thinks better of you".

5. **A talk is a jukebox.** `PersonTalk.cs`: every person is asked the same things, "Ask what is said here", "Ask
   for a saying", "Ask for the riddle", "Speak of the debt". All 477 people have a saying, exactly one each (my
   count). A talk "needs no answer", holds no clock and changes nothing (capture 18: "How the people of the towns
   feel about you is not kept yet"). The first critic heard one voice in every mouth. This is where it comes from:
   the form asks every living soul for an aphorism.
   *How bad:* a point.
   *Smallest mend:* take "Ask for a saying" out. Let a saying fall inside a rumour for one person in five. Give each
   person of the first five ports one question only they can be asked: the files already hold a `want` and a `fear`
   for each, and nothing reads them.

6. **The machine's hand can be counted.** The user says the game "reads like made by AI" when it is bland. It is
   not bland line by line. It is the same line by line, and these are the tells (my counts over 20,764 texts: the
   card words and the people's lines).
   - Of the people's lines that run to two sentences or more, 38 in the hundred end on a sentence of six words or
     fewer. "I asked." "Yet." "I only mention it." "Somebody must." "He does not have to."
   - 55 texts hold an exclamation mark. That is one in 380. One text in the whole stock has a contraction.
   - "nobody" stands in one text in twenty (1,062). "twice" in 492. Somebody is counting something in 756.
   - Fear is named in 75 texts and weeping in 63.
   - A thing that "does not" do what a person does: "Wine does not ring." "Silver does not lie." "Oak does not
     improve by lying." "The river does not care whose house it is." "The fire does not ask" (on two cards).
   - The denial: "I name no boat." "I say this quietly." "Do not tell anyone I said so." "I only say what I see."
   *How bad:* a point, and for this maker the one that matters most.
   *Smallest mend:* a script that lists every line ending on a short sentence, and every thing that "does not". The
   maker strikes the closer from two lines in three, by hand, starting with the first hour. No new writing.

7. **There is a dry joke at every grave.** A man who fled the sack of his town: "The toll man hid in a wine cask,
   which is the first use I ever knew him make of one." A slave dealer, of a girl on his bench: "Now and then it is
   true, and then I am glad I listened." A hand in a gale: "I have been in worse. I did not enjoy that either."
   Violence is rightly kept offstage for a merchant. But it reaches me as wit every time, and wit is a way of not
   being there.
   *How bad:* a scratch on any one card, a point over a life.
   *Smallest mend:* in every card of a death, a raid or a sale of people, cut the last sentence of each speech and
   read what is left.

8. **Faith costs nothing.** In all five crew cards I read that set the Dane against the steersman there is an
   answer that leaves both men content, for nothing or for two pennies: pour and pray (+2 and -1, heart up), ask
   each of his father (+2, +2), the joke about Thursday (+2, +2), buy the pork (+2, +2), sign him at the church
   door (+2, +4, +6). The Faith window says of its own calendar: "Keeping these days is not counted yet: nothing
   stops a sale on a feast, and Troth does not move by them." The first critic said the safe choice on a sea card
   is free. This is another thing: the kind, modern, both-sides answer always wins, in a century that hanged men
   for the other.
   *How bad:* a point.
   *Smallest mend:* take the third answer out of two of the five cards. Let the first sale on a Sunday cost one thing
   the player can see.

9. **The old gods are someone else's faith.** In the card files "priest" stands 1,517 times, "bishop" 1,450,
   "Saint" 794, "Mass" 200. Thor stands 6 times, Odin 2, Woden 2, "the gods" 34. "Jew" stands in no card at all,
   though a Jewish merchant is planned as a life to play (my counts). The heathen is "the Dane", watched from a
   Christian's bench. The Offer button says "not yet". A player who picks "The old gods" in capture 13 will read a
   Christian's deck about himself.
   *How bad:* it would stop me playing a heathen. For the default start it is a scratch.
   *Smallest mend:* do not offer a faith until twenty cards are written from inside it. The first: the horn of ale
   at the stem, with you holding it.

10. **The narrator stands in two centuries.** Capture 01, on the first morning of May 793: "from midsummer 794 only
    the heavy one passes, the king's price for grain is cried on the dams". Capture 14, of Ripa: "no ditch, no
    church, no hall anyone has found." The first is next year's law. The second is an excavator's sentence. The
    wiki plan keeps the game and the record apart; the People here window stirs them together.
    *How bad:* a scratch, but one a reader of history sees in the first minute.
    *Smallest mend:* the port's lines say only what a man on the dam knows that day. The rest moves to the wiki's
    second part.

11. **The word on a good is a form letter.** Capture 08, under one port: "The toll man at Dorestat said Traiectum
    was paying poorly for Rhenish wine. A few days back.", then the same for quernstones, then the same for bread.
    The book's promise is that "every price is something a named person told you". On screen it is an office, not a
    name, and one sentence three times. Both earlier critics called this the best idea in the game. I agree, and I
    say the screen wastes it.
    *How bad:* a point, because it is the promise.
    *Smallest mend:* name the teller where the port has one, and say all he said in one line: "Aldgrim at the toll:
    Traiectum pays poorly for wine and stones, well for bread. A few days back."

12. **The workshop shows through the words.** Capture 12: "the world has the colours of the Tide palette". Capture
    15: "A check 'by crew.seamanship_best' counts one pip better". Captures 03 and 07: "Only with the writing
    "charter toll dorestad" in your chest" and "Either: Only with weapons aboard; or: Only with captives aboard."
    Captures 01 and 05: "(to come)" on three rows. Captures 03 and 11: a person's feeling as a bare "easy".
    *How bad:* a scratch each. Together, in a game that is all words, a point.
    *Smallest mend:* five strings, and a rule that no text reaches the screen with an id in it.

13. **Captives are a row in the market.** Read in code, not seen: `goods_793.json` has "Captives", unit "person",
    48 d, with the note "Shown as a lot in the market like any other". `TradePanel` gives every row a Buy and a
    Sell button, and a sale's line ends with how much "more than they cost". The cards have earned better than that
    sentence.
    *How bad:* a point, and a matter for the Steam survey.
    *Smallest mend:* no row. A person is bought or sold only on a card or in a talk with the dealer, as so many men
    and so many women.

14. **Only the captive who is like you may speak.** `chain_silver_road.s07`: among twenty roped people one boy
    speaks, a Frisian: "I can count, so he kept me." `harbour.strand_of_captives.duiblinn`: one calls out, a
    reeve's son: "I can tally. I can read." `chain_dorestad_834.s05`: a rich skipper's daughter. Each can be bought
    free, the buying is a good deed, and kin pay you back. The rest are "Slavs from up the river" in "a tongue
    nobody on your boat knows". Three cards, one shape, and the hard part of the matter is outside it. (That the
    freed cannot yet ride home, because `passenger` does nothing, is known.)
    *How bad:* a point from where I stand.
    *Smallest mend:* one card where the one who calls to you cannot be bought, or where the one you can afford is
    not the one who asked.

15. **The chest speaks the oldest English in the game.** `texts_793.json`, Beowulf in the translation of 1892:
    "Waxed 'neath the welkin, world-honor gained". Forty-three evenings of it. The Edda is in the English of 1923.
    The licence rule (a translator dead by 1955) is right and the credits audit checked it. Nobody checked the
    reading. Beside cards without one archaism it is another game's voice, and for a reader short of time or sight
    it is a wall.
    *How bad:* a point for the chest. It is the line between a gift and a gimmick.
    *Smallest mend:* show the game's own plain rendering first, as the tongues file already does for its turning
    lines, and keep the old translation behind one press.

16. **A riddle on every bench.** 167 effects in the cards hand out a riddle (my count). At Dorestat alone Ravn sets
    one, the mint's clerk sells one for 10 d, a schoolboy at the alehouse sells one for 8 d and a salter sells one
    for 12 d. Capture 14 logs four things "Into the chest" in one morning at Ripa.
    *How bad:* a scratch that grows. A rare thing has been made common.
    *Smallest mend:* one riddle a port in the first release, and never two for sale in one town.

17. **Ravn knows everyone, and is three people.** His entry has 41 ties, to people in 24 of the 50 port files (my
    count). About ten of them turn on what he will not say of himself, and sixteen on riddles. A secret kept in
    forty ports is not one. And the game holds him three ways that do not meet: a person to talk to at the dam
    heads (capture 17), the `ravn` of the port cards, and "the Dane" or "the hand" of the crew cards.
    *How bad:* a scratch today. A point on the day the maker sits down to write him.
    *Smallest mend:* keep five ties, and make one of them a person who dislikes him for a reason.

18. **The men who die have no names.** The Crew window: "The crew are not yet people with names". The burial card
    buries "the hand". Names are planned (the book: four men with names and seats). Until then the best card in
    the deck is spent on nobody.
    *How bad:* a point until it is built.
    *Smallest mend:* four names before any other work on the crew.

19. **A card is a page with none of a page's comforts.** A card holds 130 words of speech at the middle of the
    range and 208 at the ninetieth part in a hundred, before its choices; the longest holds 402 (my count of all
    written lines, before the card's own conditions thin them). In capture 03 a line of speech runs to about ninety
    letters and the small print to about a hundred and ten. The project's own rule is 45 to 75
    (`research/next/art.md`, C2). There is no setting for the size of text and no way to read the last card again.
    The text size audit measured sizes (known). It did not measure line length or how much a card asks.
    *How bad:* it would stop a reader with poor sight. For the rest it costs a point.
    *Smallest mend:* a text column never wider than 70 letters; a size setting of three steps; a card over 150 words
    split where the speaker changes.

20. **The purge of modern words would make the prose worse.** `content/wording_fixes_sea.md` and `_ports.md`, 145
    lines waiting for the maker: "To port Langaland" becomes "On the back side Langaland"; "an hour of tide left"
    becomes "a short while of the tide left"; "Take the flood at its second hour" becomes "when it is a third run".
    The banned list holds "hour", "luck" and "sorry". A deck on a boat that has none is a fault of fact, and so is a
    captain. "Hour" is not. This is the costume the rest of the writing avoids.
    *How bad:* a scratch a line, a point if all 145 go in.
    *Smallest mend:* apply the faults of fact (captain, deck, hatch) and leave the words every reader needs.

21. **In one capture the chart lies.** Capture 04, the measure from Dorestat to Lundenwic: the marks have moved and
    the picture has not. Lundenwic, Sarre and Dofras sit on the Dutch coast and the course is drawn over dry land.
    It may be the capture's timing. Outside my trade, but I saw it.
    *How bad:* a scratch, unless it is how the measure looks in play.
    *Smallest mend:* have the test wait for the chart's picture before it saves, and look again.

## 4. The review

Written as a long piece in the manner of Eurogamer would read (a site that, as I recall, has scored out of five
stars since 2023), if the first release shipped as the plans describe: one life, 793 to about 805, on twelve
ports, about 420 cards, the frame drawn by its maker. I did not play it: the playing below is how the plans say it
would go. Where a judgement rests on a thing I saw or read today I mark it *(seen)*. Where it rests on a plan I
mark it *(planned)*.

### Kapman review: a library with a boat tied up outside

*A merchant's life on the North Sea of 793, told in words round a chart. The learning is a gift and some of the
writing is the real thing. The one voice it is all written in, and the hours it asks you to wait, are the price.*

Three weeks into my first summer the steersman would not sit downwind of the Dane. It was Lent, the Dane had pork
on the fire-stone, and between their two complaints the game laid a line of the king's law for the conquered
Saxons: death for the man who eats flesh in the fast. "The mate has stopped chewing." The mate is a Saxon, baptised
in a river with spearmen on the bank. I bought the pork for two pennies, put it in the brine tub till Easter, and
both men thought the better of me *(seen, as a card in the files)*. That card is Kapman at its best. It is also the
place to begin doubting it, and I will come back to why.

You are a Frisian of Dorestat, the king's toll port on the Rhine, with a six-oar boat, four men and 480 pennies
owed to the count by Michaelmas. You never see a price. You see what somebody told you and how old the telling is,
and you stake a cargo and a week on it *(seen)*. You run all this from a frame of small windows round a chart of
the sea; people and happenings float up over it as cards; your boat sits in a little window of her own water
*(seen as a stand-in; the 3D view is planned)*. One person made it, and the store page says an AI model wrote the
code and the first drafts of the words *(planned: the Steam notes say it must be said)*. So the question I was
asked most while playing was whether it reads as if a machine wrote it. In places it does, and I can say where.

First, what it gets right, because it gets a great deal right. Pentiment, as I recall, was praised for wearing its
learning in its hands and not on its sleeve: scripts that changed with a speaker's schooling, a village followed
over twenty-five years, a murder it declined to solve. Kapman's learning is of that kind. Its calendar reckons
Easter for 793 and gets it right *(seen, and checked)*. Its law is something you trade under: in Lundenwic a
purchase is good only if the king's reeve saw it, and a Frank with wine he may not land will sell it to you over
the far side of his hull *(seen)*. Its great set piece is not a battle but a coinage reform. In 794 the king makes
the penny heavier, and your debt of forty dozen pennies can be paid by tale, in the new coin, or by the weight that
was lent. The moneyer starts to name what the difference is, and the count stops him: "Do not say the word you are
about to say, moneyer." *(seen)* I have read the canon on usury. I had not felt it before.

Kingdom Come: Deliverance was praised, as I recall, for a Bohemia in which you are nobody, and faulted at launch
for its bugs and for the edges of what its accuracy chose to show. Kapman is honest about its edges in a way I have
not met before. Every person in its files is marked as found in a source or made up, and the in-game book keeps
"In the game" apart from "What is known" *(the files seen; the book planned)*. Its old tongues are the same. A
Dane's saying comes up in Norse and turns to English a piece at a time, more of it the more Norse you have
*(seen in the code, not moving)*, and every such line is a real line of a real text. The Frisians, your own
people, get none, because nothing in their tongue survives from so early. Heaven's Vault made a dead language into
play. This is a quieter cousin of that, and it is a gift.

Now the pork. I felt clever buying it, and I felt nothing else, and after a dozen such cards I saw why. Whenever
the Christian and the heathen in my boat fell out, there was an answer that pleased both and cost nothing
*(seen: all five such cards I read)*. The first screen tells you what each faith asks of you. Then what it asks
hangs in a window as a calendar, and whether you keep it is, on the day I read the code, not
counted *(seen; counting it is planned)*. Six Ages, and King of Dragon Pass before it, were praised, as I recall,
because their gods had to be got right and the price of getting them wrong was real. Here the old gods are named a
handful of times in half a million words, and always from the Christian's bench *(seen)*. I could choose to be a
Dane of the old faith on the first screen, and the people of my own town then greeted me with "A Frisian boat, and
small" *(seen)*.

And the voice. Disco Elysium put two dozen voices in one skull and its reviews, as I recall, could tell every one
of them apart. Kapman has four hundred and seventy people and one voice: dry, exact, sparing, turning at the end
of each speech on a short sentence. It is a good voice. The steward's "A day has no price, and that is what makes
it dear" would grace any game. But the alewife has it, and the bishop, and the slave dealer, and the man who has
just run from the burning of his town and makes a joke about the toll man in a cask. I counted, because I began to
hear it in my sleep. More than a third of what people say ends on a sentence of six words or fewer. One line in
380 has an exclamation mark. In all those words, fear is named 75 times *(seen)*. Nobody babbles, nobody is
stupid, nobody is frightened in their own voice. That is the sound of a machine's first draft, and it is the sound
of a single human stylist too. Either way it wants a second hand through it, and the maker's is the only one that
will do.

The trade in people is in the game, and I think it right that it is. Where it is written, it is the best thing
here: a Frisian boy kept by a dealer on the Volkhov because he can count, cutting six notches for your six and not
looking up. Where it is merely counted, as a row in a market with a Buy button *(seen in the code)*, it is the
worst. And three times I met the same captive: young, lettered or well born, of my own speech, with kin to pay me
back *(seen)*. The ones nobody on my boat could understand stayed on the bench and said nothing. A game this
careful about what a tithe is should be more careful about that.

Then there is the sitting. The clock runs at a minute of the game to a second, four times that at most, and the
maker has ruled that empty hours are not skipped *(planned, as a ruling)*. Sunless Sea and Heaven's Vault were both
loved, as I recall, for their words and sighed over for their crossings; inkle, I believe, later let you skip much
of the sailing. Kapman knows this and means to fill the leg: signs from the crew, other sails, a chest to read
while she runs *(planned)*. I could not judge whether that holds. On paper a crossing to Lundenwic is twelve real
minutes at the top speed with two cards in it. I would spend them in the chest, and there I would meet Beowulf in
a translation of 1892, "Waxed 'neath the welkin", forty-three evenings long *(seen)*.

One thing is not a matter of taste. This is a game of reading, and it makes reading harder than it need be. The
type is small *(seen; a mend is planned)*. Lines run to ninety letters. When three men speak on a card the card
does not say which of them said what, and when you choose, a ledger answers: so-and-so "thinks better of you"
*(seen)*. In a game like this these things are not polish. They are the page.

Who is it for? Anyone who read Pentiment's margins, or who has ever looked up what Michaelmas was. Anyone who
liked that Roadwarden, as I recall, made forty days the thing at stake, and wished 80 Days had weighed more and
hurried less. It is not for anyone who wants to steer, or to fight, or to be told what to do next. It is not for
anyone with ten minutes. And until there is a setting for larger type it is not for anyone whose eyes are as tired
as mine.

Kapman knows more about the year 793 than any game I have played, and it sounds, too often, like one clever man
talking to himself in four hundred hats. Its best cards stand the comparison with Pentiment. Its worst habit is
that it has a best manner and uses it for everything. I would send a friend who reads to it without a second
thought. I would tell the rest to wait for the second hand.

**Three stars out of five.**

*What this verdict rests on.* Seen: the prose of 78 cards, the people of one port, the frame in eighteen still
pictures, the rules in code. Planned and not seen: that cards come at a sane pace, that a crossing is worth its
minutes, that the debt bites, the maker's own drawing, sound, the 3D boat, the in-game book. If the plans slip, the
score falls. If the voice is broken up by the maker's hand, it rises by a whole star, and I say so below.

## 5. The same review's verdict if it shipped as it is today

1. Today it is a frame, a chart and a market. You can say who you are, talk to the people of a port, set a course,
   sail a river leg, meet a toll man and sell a cask of wine. That is what the eighteen captures and the play test
   show, and it is all they show.
2. The story is in the files and not on the first morning. The steward and his hazel stick do not come down to the
   boat (the test asserts that no card opens at home in a new game), so the debt on the top bar belongs to nobody,
   and the priest, if you ask him, tells you of a raid that has not happened yet.
3. Nothing answers. A choice ends on a machine's line, a talk changes nothing, the days of your faith are listed
   and not counted, how people feel "is not kept yet", and three of the eight buttons say "not yet".
4. The page fights the reader: small type, long lines, no names on the speeches, a purple sea, and workshop words
   in the windows ("the Tide palette", "crew.seamanship_best", "(to come)").
5. I would stop inside twenty minutes. I would remember the Faith and days window, the line under Name, and Ravn
   saying "You are late. The tide is not."

**One star out of five.** No critic would score a build this unfinished. Made to, that is the score: there is
something here worth a second look, and nothing yet to play.

## 6. What would move my score most

From the three stars of section 4, in this order.

1. **The maker's own hand through the voice: one whole star (3 to 4).** Start with the sixty first-hour cards and
   the people of the first five ports. Strike the short closing sentence from two speeches in three. Give each
   port one person who is afraid, or loud, or stupid, or lying badly. Take "Ask for a saying" out of the talk. This
   is also the whole answer to "reads like made by AI". It needs no new writing, only cutting, and nobody else can
   do it.
2. **Name the speaker on every line, and give every first-release choice a written sentence after it: half a
   star.** The first is a few lines of code. The second is one sentence for each choice that can be met in the
   first release, or fewer choices. Without both, the best cards cannot be followed and the gravest choices end on
   a ledger.
3. **Make the words fit who you are, and make faith cost: half a star.** Offer only the lives the deck can speak
   to. Date the lines that outrun the calendar. Let Troth move. Take the free third answer out of the faith cards.
   Do not offer the old gods until twenty cards are written from inside them.
4. **Give the crossing to the reader: half a star.** If the empty hours stay, make them the hours when the game
   reads to you: a crewman saying the next part of the song in plain English while she runs, a line that turns,
   a riddle worked at the oar. Not a card to answer each evening. This is the difference between Sunless Sea's
   sighs and a reason to stay aboard. It rests on play I could not see, so I hold it to half.
5. **Mend the page: half a star, and for a reader with poor sight the whole score.** A text column of 70 letters at
   most, a size setting of three steps, the plain rendering first in the chest, the leaked strings cleaned, the
   captives taken off the market row and put on cards.

All five together would make this a five-star game for the readers it is for, and still a three for those it is
not. The first of them alone is worth more than the other four.
