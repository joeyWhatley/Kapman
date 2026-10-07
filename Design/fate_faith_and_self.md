<!-- Written 7 Oct 2026 by the orchestrator, alone, from memory (no web search, no agents: usage was short). Facts marked (?) are doubtful or rest on legend and must be checked before they are shown as fact in the game. -->

# Fate, faith and the self

**The ask (7 Oct):** the player should "have an impact (in any way) on history": being "at the right place at the right
time" may open "settlement assistance events", a talk with "a key character in relation to history" about "carrying
colonizers to Vinland or Greenland", or "getting a message to a lord regarding key information on a war that changes
history", all depending "on year and dates". Character creation with "culture, religion and personal background". A
"simple stat system for specific dialogue options, like Kingdom Come: Deliverance". "Deep religious themes for all
religions and cultures". "Unique rare special events". "Get inventive with it."

This page fits inside `GAME_MAKER.md` and the decisions of 7 October: one life on twelve ports first, no skipping of
time, the frame runs the game. Nothing here needs a new screen. It adds one page at the start of a game, tags on talk
options, and a family of dated cards.

## 1. The self: three choices before the first morning

One page, three picks, each with a line of what it changes. No sliders.

**Your people** sets your first tongue, how ports greet you, whose law binds your oath, and what your crew expects.
Frisian (the plain start), Frank, English, Dane, Norseman, Svea or Gotlander, Wend, Irish. For the first release all
start at Dorestat: a Dane settled among Frisians is still a Dane.

**Your faith** sets what you may do on which day, what you may carry, how you swear, how you are buried, and which doors
open. It is a way of life with a calendar, not a bonus.

| Faith | What it asks of a merchant | What it opens | True to the period |
|---|---|---|---|
| Christian, of Rome | No trade on Sunday or a feast. Fasts: no meat in Lent, so fish is dear then. Alms. Confession before a long crossing | Abbeys as hosts and bankers, charters, sanctuary, an oath on relics that a court will take | The law of every Frankish and English port |
| Christian, of the Irish houses | The same, with harder fasts and a taste for exile for God's sake | Every Irish house from Iona to Bobbio as a friend; hermits on far rocks | Irish monks were on Thule in 795 (Dicuil) |
| The old gods (Frisian, Saxon, Dane, Norse, Svea) | Offerings at the right times. An oath on the ring. Never refuse a guest. Honour your dead | Halls and things in the north, a crew that trusts your luck | Banned by Frankish law since 785: you keep it quiet south of the Eider |
| Prime-signed | You took the first cross-signing, not baptism. You may stand in a church porch and trade with Christians, and still pour for Thor at home | Both worlds, half trusted by each | A real custom of northern merchants and hired swords (*prima signatio*) |
| The gods of the Wends | Offerings at the grove and the horse oracle | The Wendish shore and its lords | Thietmar and Helmold, later (?) |
| A Jew of the Rhineland | No sailing out or selling on the Sabbath. Clean food, so your own stores. A letter of the emperor's protection in your chest | A chain of kin and letters from the Rhine to Spain and beyond; no toll at some gates; trust on credit | Louis the Pious gave such letters about 825; Ibn Khordadbeh on the Radhanites about 870 |
| Later starts: a Muslim of al-Andalus, a Greek Christian | No wine in your hold; the five prayers; alms. Or the Greek rite and the emperor's seal | Córdoba's silk and silver; Constantinople | The embassy of al-Ghazal to the Northmen about 845 (?); the Rus treaties of 907 and 911 |

**Your past** gives one strength, one thing in the chest, one person who knows you, and one ghost: a thread that comes
back two or three times in a life.

- **The steersman's child.** Sea 3. Your father's lead and line. The pilot at Witla. Ghost: the wreck he never spoke of.
- **Schooled at the minster, and left.** Lore 3, Latin letters. A psalter leaf. A monk at Traiectum who still writes. Ghost: the vow you did not take.
- **A freed thrall.** Nerve 3, a second tongue. A cut tally: the price paid for you. The man who paid it. Ghost: the kin still unfree.
- **The toll-keeper's clerk.** Tongue 3, you know every toll by heart. A false weight you kept. The count's man. Ghost: what you saw him take.
- **A younger child of a hall.** Name 3. A good cloak and brooch. A brother who has the land. Ghost: the feud that goes with the name.
- **The widow who kept the boat.** Tongue 2, Nerve 2. His seal ring. His partner, who wants the boat. Ghost: his debts, not all of them written.
- **A captive come home.** A third tongue, Nerve 3. A scar. The one who ransomed you. Ghost: those left behind.
- **A foundling of the house of God.** Lore 2, Name 1, and the Church for kin. A scrap of the cloth you were found in. Ghost: who left you.

## 2. Five words for a person: the stats

Five, each one to five pips. They rise by use, never by spending points: you become good at what you do.

| Stat | What it is | Rises when | Falls when |
|---|---|---|---|
| **Tongue** | Talking a thing through: the haggle, the plea, the soft answer | A talk option tagged Tongue succeeds | Never; but it does not carry into a tongue you cannot speak |
| **Name** | How you look and who stands behind you: your cloak, your washed hair, the men who vouch | A deed is told of you; a gift is given well; a lord receives you | A broken oath; a shabby winter; a tale told against you |
| **Nerve** | Holding firm: facing a reeve down, not altering course, naming a price and keeping silent | You hold through a danger or a hard talk | You are seen to run |
| **Lore** | What you have read, heard and can quote: law, scripture, verse, the old stories, runes | You read a text in the chest; you solve a riddle; a learned person teaches you | Never |
| **Troth** | How well you keep the way of your own faith | You keep its days and duties | You break them, or stand in two camps too long |

**In a talk,** as in Kingdom Come, an option shows its tag and a word for the odds against this person:
`[Tongue] Ask him to wait till Lammas: likely`. The other person has their own pips, and their people, faith and feeling
for you move the odds. A failed check costs something and the talk goes on.

**Four ways no other game has:**

- **Quote.** `[Lore] Quote...` opens your chest. Pick the line. The right verse to the right ear wins outright: a psalm to
  the abbot, Hávamál on guests to a Dane who is about to turn you away, the law of the Frisians to a reeve who misquotes
  it. The wrong one is worse than silence. This puts the 267 texts in play.
- **As kin.** `[Frisian to Frisian]`, `[Baptised to baptised]`, `[Guest to host]`: options only your people, faith or past
  can see. A freed thrall can say a thing to a slave-dealer that no one else can.
- **The gift.** Not a bribe button. What you give, to whom, before whom. A cask to the toll-keeper is a deed; a book to
  an abbot is a friend for life.
- **How you look.** Name counts your cloak and brooch and whether you are fresh off a week at sea. Danes wash and comb on
  Saturday (an English monk complained that it won them English women). A wash before the hall is worth a pip that day.

For the engine: variables `me.tongue`, `me.name`, `me.nerve`, `me.lore`, `me.troth`, `me.people`, `me.faith`, `me.past`
in the condition language, and a `by me.tongue` on a chance. The cards written so far use `crew.trade_best` and standing:
they keep working.

## 3. Faith as a thing you live

**The calendar bites.** The day rule in the frame shows your faith's days. A Christian who sells on Sunday makes silver
and loses Troth, and the priest hears of it. A Jew who is still at sea at Friday's dusk must lie to or break the
Sabbath. A heathen at a Christian feast eats, or is known for not eating.

**The table is where faiths meet.** The oldest test in the sources is what you will eat and drink with your host. King
Hákon, a Christian among heathens, was made to gape over the kettle of horse broth at the autumn feast at Lade, about
950. The same card comes to the player in a Trondheim hall, in an abbot's refectory in Lent, at a Wendish horse feast.

**Oaths.** A deal is as good as the oath on it, and each faith swears differently: on relics, on the ring, on your own
head. An oath in a form the other does not hold is cheap to break. Finding a form both hold, with oath-helpers, is a
Lore check and a real piece of period law.

**Signs are read, never proved.** Fiery shapes in the sky, a calm that will not end, a seal that follows the boat. The
game never says what it was. Each man of the crew reads it by his faith, aloud, and you choose whose reading the boat
goes by. Troth, and the hearts of the men who were overruled, move with the choice.

**The long themes,** each a chain or a strand through chains:

- **The vow.** Made in a storm, to a saint or to Thor. Cheap at sea, dear on land.
- **Two camps.** The prime-signed merchant: how long can you stand in the porch? Each side asks, in the end.
- **The mission.** You carry the monks north, or you carry word of them to the men who will burn their church.
- **Relics.** Holy bones are the most valuable cargo there is, and theft of them was called a pious deed.
- **Exile for God.** Irish monks put to sea with no oars, to land where God willed. You find one. Or you are asked to tow one out.
- **The dead.** A man dies at sea: the sea, the next churchyard, or home in salt. His faith, his kin's and yours may differ.
- **Conversion.** Never a button. A long chain with a price: who stops trusting you, who starts, what your dead would say.
- **The stranger's God.** An Andalusian envoy, a Khazar who keeps the law of Moses, a Greek with an icon. Each a rare
  passenger with a talk that can only happen if you have the Lore, or the humility, to ask well.

## 4. Being there: the hours of history

**The rule.** History runs on its dates whether you come or not. An **hour** is a real event with a place, a window of
days and a door: something you must know, carry or be to get in. If you are there, with the door open, you get a talk
with a person of history, with your stats in play. Three things can come of it:

- **Witness.** You were there. The chronicle keeps a line with your house in it. An heirloom, a tale that lifts Name for life.
- **Nudge.** The event happens, a little differently. Someone lives. A hall does not burn. A town remembers who came.
- **Turn.** It goes the other way, and later events are rewritten. A flag `changed_...` is set and the chronicle from then
  on is your world's, with a mark where it left ours. At most one Turn in a life, and it takes years of setting up.

**How a player finds an hour without a guide.** Hours cast shadows: a rumour two ports away, an omen in the sky, a
passenger in a hurry, a price that makes no sense. The Tasks panel never says "be at Jarrow in May". A skipper says a
fleet was fitting out in the north. What you make of it is the game.

### In the first life (793 to 805), on or near the twelve ports

| When | Where | The hour | Witness, nudge or turn |
|---|---|---|---|
| Summer 793 | Dorestat, Traiectum, York | Boys were carried off from Lindisfarne. Alcuin writes that he will ask the king about their ransom | Nudge: you carry the letter or the silver, and one comes home. He is your crewman, or a monk who owes you his life |
| Spring 794 | East Anglia, Mercia | King Aethelberht rides to Offa's court to wed, and is beheaded | Turn (hard): a word from a servant of the queen reaches you first (?). If he is warned, East Anglia keeps a king |
| Summer 794 | The Tyne | Northmen come for Jarrow. Their chief is killed and a storm wrecks them | Nudge: you heard of a fleet fitting out. The brothers are ready, or you pull drowning men from the water and must choose which |
| 795 | The Irish Sea | Irish clerics come home from a summer on Thule, where the midnight sun let them pick lice from their shirts | Witness: you meet them. You are the first trader who knows there is land in the far north-west |
| 796 | Lundenwic, Quentovic, the court | The ports reopen; the letter about black stones and cloaks | The chain `black_stones`, already written |
| 796 to 798 | Kent | Kent rises under a priest who left his vows | The chain `kentish_revolt_796` |
| Christmas 800 | Rome, by word only | The king is crowned emperor | Witness by news: the first skipper to bring it up the Rhine drinks free for a season |
| July 802 | The Rhine, Aachen | **An elephant.** Abul-Abbas, the caliph's gift, walks into Francia | Rare event: it is seen from the river. A Jewish merchant, Isaac, brought it from Baghdad: a talk for a player of his faith or with the Lore |
| 804 | The Elbe, Sliesthorp | The last Saxons are carried off. King Godfred comes to the border with his fleet and will not meet the emperor | Nudge: a family wants passage. Or you carry what you saw of the fleet to the count |
| 805 to 806 | Everywhere | The great hunger. The emperor forbids grain to leave the realm | Nudge, port by port: a boat of grain run in, or sold dear, is remembered for a generation |

### In later lives (later releases)

- **808, Reric.** Godfred burns the Wends' market and ships its merchants to his own. Written (`reric_808`).
- **About 825 to 874, the north-west.** A raven-finder needs cattle carried. A man blown off course needs believing. You may be the one who saw it first, and the Book of Settlements carries another name.
- **829, Birka.** The lot is cast at the thing on whether to let the monks preach. You are asked what you have seen in Christian lands.
- **834, Oseberg.** A queen is buried in a ship. You carried the things that go in the mound. Witness, and a grave-gift you were told to deliver and could keep.
- **About 845, the embassy from Córdoba (?).** A poet-envoy of the emir needs a ship north. Islam meets the Northmen, on your thwarts.
- **862, Ladoga (?).** "Our land is great and rich, but there is no order in it. Come and rule." You carry that message over the sea. To whom is yours to choose. A Turn that renames a dynasty.
- **878, the Somerset marshes.** A hunted king sends for his men to meet at a stone. A summons that does not arrive is a battle that is not fought.
- **885, Paris.** The count must slip out through the siege to fetch help. He needs a small boat and a steady hand.
- **921, the Volga.** A chief is burned in his ship, with a slave girl. A man from Baghdad writes it down. You stand beside him. The hardest card in the game.
- **About 950, Lade.** The king who will not eat the horse.
- **985, Iceland.** Twenty-five ships sail for the green land. Fourteen arrive. How they are loaded, and the day they sail, is seamanship. Yours. More may arrive.
- **988, Kiev.** A prince weighs four faiths. He sends men to see. Men who have seen are asked.
- **991, Maldon.** An ealdorman, from pride, lets the raiders cross the causeway to fight fair. You are at his elbow.
- **1000, the law rock.** The lawspeaker lies a day and a night under his cloak, then speaks one law for all. You brought the news of the hostages.
- **1002, St Brice's day.** An order goes out to kill the Danes in England. You hear a day early. You know some of them.
- **About 1010, Vinland.** A colony trades red cloth and milk with the people of the land, then it comes to blows and they go home. You are the trader. If it stays a trade, the colony stays. A Turn that puts Norse houses in the west in 1066.
- **About 1050, the bear.** A poor man spends all he has on a white bear in Greenland, to give to a king. He needs passage, and two kings want the bear.
- **1064, the Channel.** An earl puts to sea and a storm takes his ship to the wrong shore, where he is made to swear an oath on hidden relics. It could be your ship. Where you make land is the hinge.
- **1066.** A duke waits a month for the wind. A comet hangs over all of it. The last card of the game.

### How history leans back

A Turn is never one talk. It needs the door (years of standing with the right people), the hour (the right days), and
the talk (your stats against a stubborn great man). And the world resists: a warned king may ride anyway. The chronicle
keeps both lines for the turned event, the one that was and the one that is.

## 5. Unique and rare: once a life, or once a game

Dated by the sky and the annals, shown in the ship window and the log, each with one card that can come once.

- **The sky.** The fiery shapes of 793. Two darkenings of the sun in 810. The great comet of 837. The eclipse of 840, and the old emperor dead within weeks. The comets of 912 and 989. The red, dim sun and the hungry years after about 939. The eclipse of 1030. The star of 1066.
- **The beasts.** The elephant on the Rhine, 802 to 810. A white bear on a chain. A horn of the sea-unicorn, sold as what it is not. A stranded whale, and the law on whose it is. A gyrfalcon worth a ship.
- **The made things.** The caliph's water clock at Aachen, 807, that drops a ball for each hour. An organ built for the emperor by a Greek, 826. A little bronze Buddha on a shelf at Helgö, and nobody there who knows what it is.
- **The people.** A hermit on a skerry who has not spoken in nine years. A woman who sees for a price, with a staff and a cat-skin hood. A king in disguise in his own hall. A skald who will make a verse on you, good or bad, by what you pay.
- **The sea.** St Elmo's fire on the mast. Ice from the north in June. A ship met with no one aboard. The whirlpool the old men talk of. A drowned bell that is heard in fog.
- **Your own.** Each past's ghost. The day your Name is first said by a stranger. The first grey in the portrait.

Rule for all of them: rare means rare. A list of about forty, each with a date or a one-in-a-life chance, each seen at
most once. A player should finish a life having met five or six and wondering about the rest.

## 6. What to build, and when

1. **With stage 1 of the book:** the creation page (people, faith, past) with only what the twelve ports need: four
   peoples, four faiths (Rome, the old gods, prime-signed, the Rhineland Jew), the eight pasts. The five stats, the tags
   on talk options, Quote from the chest, As kin. The faith calendar on the day rule. No hours yet.
2. **With the 60 cards the user reads:** six of them are hours of the first life (the Lindisfarne boys, Jarrow, the
   Thule clerics, the elephant, the Saxon family, the hunger), and four are rare events.
3. **Stage 2:** signs read by the crew; oaths across faiths; the table cards; Witness and Nudge wired to the chronicle.
4. **Later lives:** the Turns, one an age, with the chronicle that forks.

## 6a. Where you start, and the colour of your world (7 Oct, later)

**The ask:** "different starting ports and depending on your culture you start somewhere else and the world is a
different colour from the preset, take away the option to change it"; "several more 3bit colour palettes created around
Byzantium colour".

A palette is three bits: eight colours, one for each kind of ground on the chart (deep water, sea, shallows, shore,
marsh and heath, cleared land, woodland, high and bare ground). Every palette holds Byzantium (#702963) in one of the
eight places. Your people fix your palette for the whole life; there is no key to change it. The files are in
`Design/palettes` (one `.gpl` each for Aseprite, `palettes.png`, `preview_north_sea.png`), and the game's copy is
`Assets/Kapman/Code/Game/Map/MapPalette.cs`.

| People | Start | Palette | Where Byzantium sits | Why this port |
|---|---|---|---|---|
| Frisian | Dorestat | Tide | the sea | The greatest market of the delta; the game's first home |
| Frank | Quentovic | Vineyard | the family of the whole palette (violet sea, wine woods) | The king's Channel port, with its mint and its tenth |
| English | Lundenwic | Minster | the sea | The Mercian market on the Strand |
| Dane | Ripa | Amber | the woods | The oldest market of the Danes, on the amber coast |
| Norseman | Ogvaldsnes | Fjord | the heath (heather) | A king's manor on the strait every northern ship must pass |
| Svea | Birka | Silver | the heath | The market in the lake, where the eastern silver comes |
| Wend | Reric | Grove | the sea | The Wends' own market, until 808 |
| Irish | Hi (Iona) | Vellum | the high ground | A boat of the great monastery, in the colours of its books |

The first release has twelve ports, so it offers the starts whose home is among them (Frisian, Frank, English, and Dane
if Ripa is in the twelve). The rest wait for their waters. A start is more than a pin: it is the debt you owe and to
whom (a count, an abbot, a king's reeve, a kindred), the first chain, the tongue of your crew, and which ports greet you
as kin.

## 6b. The first and last day

The span of 793 to 1066 is right and should stay: it is the one span every player already knows the name of.

Two changes are worth making:

- **Begin on 1 May 793, not 21 June.** The sailing season is opening. The fiery shapes over Northumbria have been seen.
  Lindisfarne falls on 8 June while you are trading, and the word of it reaches you by ship some two weeks later: the
  game's own rule, news as old as the ship that brought it, shown with the most famous news of the age in the first
  hour. It also gives 150 days to Michaelmas, where the book counted 100: the money must be tuned to that.
- **End at Christmas 1066, not at the battle.** The duke is crowned in Westminster on Christmas Day. The emperor was
  crowned on Christmas Day 800. A game that passes one and closes on the other has a shape, and the last autumn lets a
  house act on what the battle decided.

The 273 years fall into three books, and each is a release: **the markets** (793 to about 865: Dorestat, Quentovic,
Lundenwic, Hamwic, and their end), **the armies** (865 to 954: the Great Army, new walled towns, Normandy, York),
**the kings** (954 to 1066: Christian kings in the north, Iceland, Greenland, Vinland, Cnut, the Conqueror).

## 6c. The tongues: words of the time, and the turning of a line

**The ask:** "make sure the wording and terminology is accurate to the time, maybe making the text language mostly
English with the speaker's original language sprinkled in"; in some moments "you only see their original language and
it does a cool animation translation as it appears and the foreign language disappears as English takes its place".

**Three layers, from common to rare.**

1. **Plain English, with the period's own things by their own names.** No "thee" and "thou". But a toll is a toll, a
   penny a penny, and a thing that has no English name keeps its own: the *thing* (the assembly), the *wic*, *wergild*,
   a *felag*. A word in another tongue is set apart, and a touch or a hover gives its meaning and the tongue it is from.
2. **Sprinkled words.** A Dane says *heill* when he greets you and calls his partner his *félagi*. A monk slips into
   Latin for holy things: *Deo gratias*. A Frank swears *per sanctum Martinum*. Only greetings, oaths, kin words, trade
   words and curses: the words a foreigner really hears first. Each comes from a list of attested words, never made up.
3. **The turning line.** At a moment that matters (a first meeting, an oath, a verse, a prayer, a dying word) the line
   comes up whole in the speaker's own tongue, and turns into English word by word as you watch.

**How the turning works, and why it is a mechanic and not only an effect.** How fast it turns, and how much of it, is
your skill in that tongue (`tongue.<name>`, which the engine already has). A tongue you know turns at once. A tongue
you half know turns slowly, and the hard words stay as they were: you get the gist and miss the catch. A tongue you do
not know does not turn at all unless someone aboard speaks it, and then it turns in his words, coloured by what he
wants you to hear. Each word you have seen turn is a little learned: the tenth time a Dane says *kaupmaðr*, it stays
English from the start. So the player learns the north's words as the merchant does.

**The rule that keeps it true.** A whole line in an old tongue is only ever shown where a real one exists: a verse of
Hávamál, a line of Beowulf or The Wanderer, the Old Saxon baptismal vow, the Latin of the psalms and the Mass, an Irish
monk's gloss. These are in the chest already or in the public domain. Where no real line exists, the speaker talks in
English with sprinkled words. Old Frisian is the hard case: nothing in it is written down before the thirteenth
century, so the game's own people get the closest attested kin (Old English and Old Saxon words) and say so in the
credits. Nothing is invented and passed off as old.

**To build:** a glossary file of attested words by tongue with meaning and source (a few hundred, checked by a person
who knows them before release); an optional `original` and `tongue` on a line of writing; the turning effect in the
frame's speech place; the word-learned record in the saved game.

## 6d. Bonds with anyone, and a world that answers (7 Oct, later still)

**The ask:** "the player will need to form a relationship with an NPC of their choice and it can be anyone"; if you woo
Alfred's wife and she leaves him "it'll have a catastrophic outcome, maybe it changes Alfred's personality for the
better or worse, maybe he goes full war man mode and exterminates the Danelaw or loses his kingdom"; "maybe you can
help the Welsh in some way to reclaim land"; "I want everywhere and everything to be dynamic like this."

No one can write every such story by hand. So the game does not hold stories of what happens if. It holds **people**
and **pillars**, and lets what happens fall out of them.

**People of weight.** Every person in the places files and the cast already has a trait, a want, a fear and ties. A
person of weight (a king, a bishop, an earl, a queen: a few hundred across the span) gets three things more:

- a **temper**: steady, grieving, hardened, broken, vengeful, exalted. It is a state, and it moves.
- what **holds them up**: the two or three bonds their steadiness rests on (a spouse, a son, a lord, their God, their name).
- what they **carry**: the events of history that rest on them.

**Pillars.** Each event of the chronicle stands on named pillars. Edington, 878: the king alive; the king resolved; the
summons carried to the three shires; the Danes in Wessex. On the day, the game counts what stands. All four: the battle
as it was. The summons lost: no host, and the king dies a hunted man. The king broken: the same. The king hardened and
the host come: a battle with no mercy, no baptism of Guthrum, no treaty, and a war that runs ten more years.

The player never presses "change history". The player does things to people, and a pillar falls or a new one rises.

**Your example, run through it.** Ealhswith of the Gaini marries Alfred in 868. A merchant who is often at the hall can
come to know her (bonds, below). If she goes with him:

1. Her bond to the king breaks. That is one of what holds him up.
2. His temper moves, by his own nature (pious, sickly, stubborn) and by how it was done. Quietly, with her kin's
   leave after a wrong done to her: he **grieves**, then turns to God, and is a milder, more bookish king who makes
   peace early. Openly, with a boat at the quay and the hall watching: shame before his men. He **hardens** or he **breaks**.
3. Each event he carries is then read against that temper. Hardened: he wins, spares no one, the Danes of the east are
   driven out or killed, there is no Danelaw, and no Danish kings of York. Broken: Wessex falls in 878 and England is a
   Danish kingdom a century early. Either way the children who were not born (Edward, Aethelflaed) take their own
   pillars with them: no Lady of the Mercians, no reconquest led by her.
4. And the house pays: this is the abduction of a king's wife. Her kin and his want blood or a ruinous price; the
   Church lays penance; every English port is shut to you; a bounty sails as fast as news. You may have changed a
   century and must still get out of the Solent alive.

The game wrote none of that as a story. It followed from one broken bond.

**Bonds with anyone.** The same four steps for a harbour girl, a rival, a monk or a queen:

- **Known**, by meeting and talk. **Trusted**, by a favour done and a thing kept quiet. **Bound**, by an oath, a shared
  danger, a gift that cost you. **Sworn or beloved**: marriage, sworn brotherhood, fostering a child, godparenthood,
  the lord's hand.
- Each step is a talk with your stats in it, and each needs time: seasons, not minutes.
- The period's own rules give it teeth. Marriage is kin's business: a bride price, a morning gift, witnesses. Taking a
  woman without her kin's leave is a crime with a set price, and a feud. A monk or nun has vows. A slave has an owner.
  A lover in another faith must choose, or you must.
- What a bond gives is what that person is: a pilot's bars, a moneyer's true weights, an abbess's letters, a queen's ear.
- What a bond costs is that it can be used against you, and that people die.

**The Welsh, the same way.** The Mercian frontier stands on pillars: Offa's strength, the dyke manned, the Welsh kings
divided. A merchant from Chester or the Severn who carries iron, grain in a hungry year, or the news that the king of
Mercia is dying (796) to a king of Gwynedd or Powys moves a pillar. Land does not change hands because a card says so.
It changes because, on the day a raid is rolled, the Welsh were fed, armed and forewarned.

**What keeps it from running away.**

- **History is heavy.** Most events have a spare pillar: knock one out and it still stands, a little bent. It takes two
  or three to bring one down, which takes years of a life.
- **One fall at a time.** When an event falls, the events that leaned on it are re-read once, in date order, by rule.
  The chronicle prints the new line and marks where it left the world you know.
- **A changed world is still written.** For each event the data holds its plain outcome, one or two bent ones and one
  fallen one, each a line for the chronicle and a list of what it does to ports, prices and people. That is two or
  three lines of writing an event, not a novel. Past that, events with no written line fall back to a plain rule: a
  realm with no strong king is raided more and trades less.
- **The first release stays small.** One life, twelve ports, perhaps thirty people of weight. Bonds to anyone in those
  ports, from the first day. Tempers and pillars on the dozen events of 793 to 805. The great turns, Alfred among
  them, belong to the second book.

**To build, in order:** bonds and their four steps on the people already written (stage 2 of the road); tempers and
what holds a person up for the people of weight of the first life; pillars on the first life's events, with the
chronicle printing bent and fallen lines; then the same for each later book as its ports open.

## 7. Yours to decide

1. May a first-release player choose a faith other than Christian or heathen (the prime-signed, the Rhineland Jew)? I say yes: they are the two most interesting ways to play a merchant.
2. Should Turns exist at all in the first life, or only Witness and Nudge? I say only the two, with one hard Turn (the East Anglian king) for the player who finds it.
3. Stats as pips the player sees, or hidden and shown only as the odds word? I say pips: it is the pleasure of watching yourself grow.
4. The Volga funeral and the St Brice's day killings are dark. Shown plainly, told at a remove, or left out? I say plainly, with the choice to turn away.
