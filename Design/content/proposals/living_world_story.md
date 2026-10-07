<!-- Proposal, 4 Oct 2026, night: the living world from one stance, THE STORY FIRST. Written from the code as it stands.
Every number is a starting guess to tune; (?) marks a fact not checked. A draft for the user to overwrite. -->

# The living world, story first

**The stance.** Everything the player meets is a story with an ending: a place's, a war's, a promise's, the house's.
Nothing is simulated that the player cannot see and change. A place never visited follows history and costs nothing.
A war ignored ends as it really did, or by one throw of the dice, and comes as news as old as the ship that brought it.

**What is there today** (read from the code, not guessed):

- The chronicle fires 582 real entries on their days, as news and as market shocks. 455 lie before 850 and 127 after:
  the late game is nearly empty. Only famine, raid and burning move a market; a war is news. No entry can be stopped.
- A market has a size of 1 to 5 that can only shrink (`Market.Burn`, `Market.Ruin`). No people are counted.
- 138 ships trade by what they believe, and stay as they are: they make news old. Raider bands weigh their men against
  a place's defence. Orders with a day (`Commissions`) and standing by port are built. Cards have conditions, effects,
  lapsing flags and follow-ups (`schedule <card> in <days> at <port>`): enough to carry any story.
- Not there: heirs (`generation` is a name in `Cond.cs` that nothing feeds), winter, the day stick, cards in the chart
  scene, realms as a table, any pacing beyond "one card waits".

## 0. One thing to build: a story in play

A place, a war, a hinge of history, a promise and the house's own affairs are one kind of thing to the game:

`{ id, kind, where: [ports], stage, due: a day, patience: days, tone, default: an ending, endings: [...] }`

It moves by its date, by a choice on a card, or because its patience ran out: then it takes its `default` ending
off-stage and comes as news. Its steps are cards of the engine as it stands, its memory is flags, its day is a notch on
the day stick. The sixty chains being written tonight and every place's `stories` already have this shape.

How they fit: places give the needs, wars and hinges give the dates, promises are how the player takes hold, the
storyteller chooses the order, and every ending is read from the flags they leave.

## 1. Settlements: three numbers and a word

`hearths = curve(year) × (1 + mark) − wound`, and `store`, the months of bread in the place (0 to 12).

- **curve**: history's own course for the port, four to six anchor years each, in a new `settlements_793.json`. A
  hearth is about six souls (the Medemblik dossier's households of six to ten).
- **wound**: hearths lost and not yet back. A raid takes 5 to 15 in a hundred, a burning 25 to 40, a hunger year 5 to
  10, a sickness 10 to 20; a flood by the count (838: 2,437 drowned). It heals 8 in a hundred a year, 25 when helped.
- **mark**: what the player's house has added or cost for good, −0.5 to +0.5.
- **store**: the harvest fills it to 9 × h (h is 1 in a common year, 0.5 in a famine of the chronicle, else a seeded
  0.5 to 1.3: economy design 2.5, not built). A month eats one. Under 2 the place is hungry; at 0 it starves and the
  wound grows by 1 in a hundred a week.
- **the word**: thriving (mark over +0.15), steady, hungry, starving, burned, emptying (its curve is falling), a ruin.

Nothing ticks. Curve and store are worked out from the date when someone asks; only a port with a wound or a mark
keeps state, and takes one step at winter. Two built systems read one factor, k = hearths now ÷ hearths in the port's
first year, held within 0.25 to 3: `Market.Resize` (usual stock and appetite × k) and the raiders' `Defence` (× k). A
burned market's lost step then comes back as the wound heals; today `sizeLoss` is for ever.

| Place | Souls | Hearths | From |
|---|---|---|---|
| Dorestat, about 800 | "a couple of thousand" along 3 km of bank | 350 | nl.wikipedia, Dorestad |
| Hamwic, 700 to 850 | 2,000 to 3,000 | 400 | en.wikipedia, Hamwic |
| Birka at its largest | 500 to 1,000; about 700 | 115 | en.wikipedia, Birka |
| Sliesthorp, 10th century | at least 1,500 within 26 ha | 250 | de.wikipedia, Haithabu |
| Traiectum, 793 | 150 to 400 | 45 | the dossier's own guess (?) |
| London, 1066 | about 20,000 | 3,300 | en.wikipedia, Anglo-Saxon London |

A minster is 50 to 300 souls, a terp village 10 to 30 hearths (?). Each port is to be sourced by itself.

**How a merchant helps a place thrive:**

| Act | What it is, and the numbers |
|---|---|
| Feed it | Grain landed while it is hungry. A hearth eats about 115 kg a month (?). The Gislinge's 660 kg is a month for six hearths; a 24-ton ship's is a month for 200. One boat saves a village for a week; a house saves a town. |
| Warn it | News of a fleet under five days old, told to the reeve: the stock is hidden (a raid leaves 0.7, not 0.35) and the wound is halved. |
| Mend it | Timber, iron and silver after a burning: it heals at 25, not 8. |
| Fund a work | The places files' `works` (`act:fund_work`), 300 to 2,400 d, 30 to 180 days: +0.03 to +0.10 on the mark and one gift each. A jetty: berths. A granary: three months of store carried over. A ditch and bank: defence × 1.5. A well: sickness halved. |
| Bring people | Families carried from a burned place: a hearth each. A craftsman settled: a good made there. |

It pays in standing, in the grand rewards of CHAINS.md (a plot, a charter, a factor), in a larger market, and in the
place's ending: every place has three. One honest rule: some things do not turn. The Kromme Rijn silts whatever
anyone does; the player decides whether Dorestad ends in ashes, or its people go in good order to Daventre and
Traiectum with the house's plot among theirs.

## 2. Wars: twelve great ones on their dates, and a war-maker

**The real ones.** Twelve runs of entries already in the chronicle become war stories whose beats fire on the real
days. Eight have a chain tonight; four do not.

| Years | The war | Chain |
|---|---|---|
| 796 to 798 | Kent rises against Mercia | kentish_revolt_796 |
| 804 to 811 | Godfred and Charles: Reric, 200 ships on Frisia, the Eider peace | reric_808, coast_watch_810 |
| 834 to 863 | The sacks of Dorestad and the delta; Rorik | dorestad_834, rorik_850 |
| 840 to 845 | The brothers' war; Quentovic, Paris and Hamburg | none |
| 865 to 878 | The great army in England | great_army_865 |
| 879 to 892 | The great army on Scheldt, Meuse, Rhine and Seine | none |
| 892 to 954 | The winning back of the Danelaw; York's last kings | york_954 |
| to 911 | The Seine Northmen and Rollo | rollo_911 |
| 934, 974 to 983 | The Danevirke wars of Henry I and Otto II | none |
| 980 to 1016 | The tribute years, Maldon to Assandun | maldon_991, cnut_1016 |
| 1028 to 1050 | Cnut, Olaf, Magnus, Harald; Hedeby burned | none |
| 1066 | Three fleets in one year | year_1066 |

**What a merchant does in a war.** Seven plays, each a card or two over what exists. None is a battle.

| Play | On the table | Built from | It pays |
|---|---|---|---|
| Supply | A commander's order: grain, horses, iron, a hull, by a day | `commission`, premium 1.5 to 2.5 | 80 to 250 d a load |
| Blockade | A port shut to one side; run it by night or in fog | `Patrols`, the search chance raised | prices doubled inside; seizure if caught |
| Envoys | A man or a letter carried by a day | `passenger`, `carried_letters` | 100 to 300 d; a lord's good will |
| Ransom | Silver carried to a camp for a named captive | `hold.captives`, `bought_captives` | a tenth of it (?); kin who owe |
| Refuge | Monks, relics, books and families carried out | `passenger`, `give_text` | texts for the chest; hearths moved |
| Profit | Loot cheap where raiders sell; grain and horses dear in the theatre | `price_shock` | the plain trade, and a name for it |
| Ruin | Your plot burns, your factor is taken, your ship is pressed | the unlock flags | nothing: the price of size |

Each play done for a side adds one to the player's **tilt** in that war. One boat can earn about 3, a house of
skippers under orders up to 8. One boat cannot win a war. It can save a town, a man or a library.

**The war-maker.** Beside and after the real wars the world makes its own, from six causes that each have a precedent
in the chronicle: brothers over a kingdom (813, Godfred's sons), a tribute refused (854, the Svear in Courland), a
toll (1018, Vlaardingen), a rising (983, the Slavs), an exile home with ships (827, Harald Klak), a raid avenged (902,
Dublin).

- Sides are lords who hold ports that year. A made war never invents a king and never kills a real one; its invented
  men are counts, reeves and chiefs with attested names.
- Two to five ports the player knows; one to three seasons; five beats 30 to 60 days apart (the muster, the first
  blow, the turn, the talks, the end). A side wins a beat with p = sA ÷ (sA + sB) + 0.07 × tilt, where s is the built
  defence figure of its ports × k. It ends in a tribute, a port that changes hands, a lord's fall, or both sides tired.
- Forty war cards are written once, with slots the name forge fills. No made war needs new writing.
- One at a time in the player's waters, the next 2 to 6 years after the last, none within two years of a great war
  there: about forty in a campaign, most after 850. The chest marks every made entry "told in this game".

## 3. Changing history: hinges

A hinge is a real entry, what must be true on its day, and what it rewrites, in a new `hinges_793.json`:

`{ entry, turned: a condition, cancels: [ids], adds: [entries], ports: { id: { realm, to } }, flag: changed_<event> }`

`Chronicle.Advance` asks before it tells: a cancelled entry is skipped, an added one fires. News, shocks and markets
need no change, for they hear whatever fires. About 30 hinges turn a place (one boat, a season, news brought in time),
about 8 a region (tilt 3, a factor on the spot), 4 a realm (tilt 5, a house of ships, most often over two lives:
878, 911, 1016 and 1066). Most turn on knowing in time: knowledge is the treasure here too.

| Entry | History | Turned by | What follows |
|---|---|---|---|
| `808-summer-reric-destroyed` | Godfred destroys Reric and moves its merchants to Sliesthorp | Warning them and carrying them out | They settle where the player takes them: 30 hearths and a factor. (A place.) |
| `834-dorestad-sacked` and three more | Four sacks in four years | The ditch and bank funded; warnings in time | Each wound halved. The river still goes: a small toll place, not a ruin. (A region.) |
| `842-quentovic-sacked` | Only the houses that paid were spared | Paying for your street, 300 to 900 d | Your plot and its neighbours stand, and remember. (A place.) |
| `845-hamburg-burned` | Church and books burn; Ansgar flees | A boat at the bank that night | The books are saved: texts no other game has. (A place.) |
| `878-05-edington` | Alfred beats Guthrum | Tilt 5 for the army in the winter of Athelney | No Wessex: England's ports under Danish kings, weighed silver, Cnut's kingdom a century early. (A realm.) |
| `911-rollo-rouen` | Rollo is given Rouen | Tilt 5 against the grant | No Normandy; in 1066 there is no William. (A realm.) |
| `1066-10-14-hastings` | Harald falls at Stamford Bridge, Harold at Hastings | Ships, grain, silver or news to one of three fleets; tilt 5 | Four Englands: William's, Harold's, Harald's, or a land divided. (A realm.) |

**How later history follows:** by rewriting the list, never by simulating a realm. Each realm hinge names the later
entries it cancels and brings 10 to 25 written ones in their place. Past that, one rule: an entry whose port is gone or
whose realm has another holder is not told, and the war-maker fills its waters. Changed history is as deep as it is
written: 4 hinges × 2 branches × about 20 entries is about 160 entries.

## 4. Quests: promises with a day

A quest is a promise: who asked, what, where, by which day, for what. Four lengths, paid on the scale of CHAINS.md:

| Kind | Length | Pays | Built from |
|---|---|---|---|
| Errand | a leg, some days | 20 to 80 d | `Commissions` (built), a letter, a passenger |
| Charge | a season | 80 to 250 d and a Turn reward | a place's need, a war's play, a crewman's want |
| Chain | 8 to 14 cards, a season to years | 600 to 2,500 d, a grand reward, three endings | the sixty chains |
| Vow | a life, or the house's | a title, a relic, kin | three crossings at your own cost; a church; a feud ended |

They come from a person at a site, a place's word (hungry: grain; burned: timber; raided: a ransom), a war's beat, a
crewman's want and the chest. The player holds at most three promises and one vow. A fourth asks which to break, and
a broken one costs standing (`broke_commission`, built).

## 5. Different starts

A start is one row in a new `starts_793.json`: year, port, people, ship class, crew, silver, debt, tongues, standing,
known waters, first chain. `Chronicle.StartAt` already takes any date; ports and goods carry their years.

| Start | Year and port | You are | Boat (class id) | Opened by |
|---|---|---|---|---|
| The debtor | 793 Dorestat | a Frisian owing the count 480 d | `gislinge` | from the first |
| The minster's man | 793 Eoforwic | a layman sailing an abbot's ship for a third | `coaster_minster_ship` | a first debt paid |
| The moved man | 808 Sliesthorp | a merchant of Reric carried off by Godfred | `baltic_coastal_boat` | reaching the Baltic |
| The ship-fort | 841 Duiblinn | a Northman at the new longphort | `norse_allround_ship` | a hinge turned |
| The townsman | 1000 Lundenwic | a London merchant with two keels | `north_sea_merchant_ship` | a house brought to 950 |

Late starts cost data more than code: raider pressure after 814 (where the table stops), patrols, fleets by century,
and ports the table lacks (Tiel, Sigtuna, Schleswig). **Unlockables** live in a small "house book" beside the saves
that outlives a campaign: starts opened, endings reached (a page each), texts read, ship classes sailed, and
heirlooms, of which a new campaign may take one.

## 6. The campaign, 793 to 1066, and its endings

By the concept's own count a season is an evening; 273 seasons played through would be some 800 hours. So the
campaign is **told years**: four to six in each of about nine lives, 40 to 50 evenings. At the winter screen the
player chooses: sail in spring, or let the winters pass until a notch the storyteller names (the heir is of age; the
eve of a hinge; a war opens), one to eight winters. For each, the house's ships bring one line by a throw on their
skipper's skill and every place takes its step. The clock's rate never changes: this is the concept's winter jump,
made longer by the player's own choice.

| Age | Years | Its danger | Realm hinge | The house (fleets design) |
|---|---|---|---|---|
| The emporia | 793 to 834 | the debt, the first raids, Godfred | none | the boat, the season |
| The raids | 834 to 878 | Dorestad dies; the great army | 878 | the partnership |
| The settling | 878 to 954 | the Danelaw, the Seine, York's kings | 911 | the house |
| The towns | 954 to 1016 | coin laws, the tribute years | 1016 | the house, the name |
| The kings | 1016 to 1066 | Cnut's peace, Hedeby burned, three fleets | 1066 | the name |

**Three layers of ending.** **A life** ends at sea, in bed, in a cowl, as an outlaw, in a feud, or gone west and not
heard of; the heir takes the chart, the chest, one feud and one friend. **A place** ends three ways. **The house**
ends at Christmas 1066, or sooner with no heir: that is an ending too, not a reload. Its last page is put together
from flags, in the Chronicle's voice: which England (four), what the house became (six), and what became of the three
places it did most for.

What the house became, each a rung the period had: thegn-worthy and gone to the land (three crossings of the wide sea
at his own cost: *Geþyncðo*); sworn brothers who "swear and drink together" (Tiel, about 1020); the king's merchants;
far-farers in Iceland or Kiev; the church's house; sea-wolves, rich on tribute and shunned in Christian ports.

## 7. The storyteller

It sits over the Scheduler and does three things only: sets flags, schedules cards, and gives the draw one more
factor, m_story (0 or 1). It is seeded by the world's seed and the day, so a save reloaded tells the same tale.

- **Three lanes push at the player**, one open story in each: World (a war's or a hinge's beat), Place (a settlement's
  need), House (crew, kin, a rival, a letter). With three promises and a vow, at most seven stories are open.
- **The card budget.** A leg: one story card and two plain ones at most. A call: the arrival set and one drawn card,
  as built; a story card due here takes that slot first.
- **Tension**, 0 to 100. A loss +15, a danger met +10, a hard choice +5; a quiet day −4, a safe arrival −20, winter
  −40. At 60 or more only cards of tone `relief` are drawn (a feast, a fair price, good news) and undated beats
  wait. At 20 or less for ten days the oldest waiting story's next beat is brought to the next port.
- **Never empty.** A lane empty for 20 days is filled: Place from the nearest known port whose word is not "steady",
  House from a crewman's want, World from the war-maker.
- **Never a flood.** A story that waits past its patience (30 to 90 days) ends by its default off-stage and is posted
  as news at its port (`WorldSim.PostAt`, built).
- **Quotas.** A season has one Turn-tier beat at least and two finales at most. Every winter something has ended; if
  nothing has, the oldest story closes by its default. The chronicle's dates never move: made stories go round them.
- **It grows with the house**, by the fleets design's stages. 1 and 2: the Place and House lanes; wars are news and
  prices. 3: the World lane opens. 4 and 5: beats also come as letters.

Cards gain one field, `"tone": "relief" | "plain" | "hard"`.

## 8. The small interface

Nothing new stands on the table. Each thing is a number, a word or a day; there are no meters.

| Where | What is added |
|---|---|
| The day stick | A notch for each open story, six at most, with its mark: a sheaf (a place's need), a spear (a war's beat), a seal (a promise), a ring (the house). Gold ahead, red when passed. |
| The chart | A port you have news of wears its word and the news's age: "hungry, 12 days". A war's waters are hatched; a shut port has a bar. |
| The place panel | Its first line: "Dorestat. About 350 hearths. Steady. Bread for 7 months. Seen today." Works are acts at their sites. |
| The chest | One tab, The tale: the open stories, a line each with its day; under them the endings reached. In the chronicle a changed entry is struck through beside what happened instead. |
| The winter screen | What ended this year; what the places you know became; sail, or let the winters pass. |

## What a programmer must build, in order

Each step leaves the game playable. First, from the chart game's own list: cards in the chart scene, the day stick,
the winter screen.

| # | Step | Nights | To write |
|---|---|---|---|
| 1 | Settlements: the file, the three numbers, the winter step, k into `Market.Resize` and `Defence`, the word on the chart; names `port.hearths`, `port.store`, `port.state`; verbs `hearths`, `store`, `work` | 2 | 50 curves; 3 endings a place |
| 2 | Hooks: shocks and raids make wounds; grain sold to the hungry fills the store; seeded harvests | 1 | nothing |
| 3 | The story record and the storyteller (lanes, tension, budget, patience, defaults); `tone`; the notches | 3 | a tone on each card |
| 4 | Promises: `Commissions` made general, the cap of three, a place's needs as charges | 1 to 2 | 20 charge cards |
| 5 | Hinges: the file, the gate in `Chronicle.Advance`, port overrides, "was and is" in the chronicle | 2 | 30 place hinges, then the realm ones |
| 6 | Wars: sides and theatres, `realms_793.json` (who holds which port, by year), tilt (`flag <name> +n`), a shut port, war prices, the war-maker | 3 to 4 | 40 war cards; 4 great wars |
| 7 | Lives: `generation` fed, succession, a life's ending, letting winters pass | 2 to 3 | 6 life endings |
| 8 | Starts and the house book | 1 to 2 | a first chain a start |
| 9 | 1066 and the last page | 1 | 4 × 6 paragraphs |

About 16 to 20 nights of code. The writing is the larger cost, and much of it is under way tonight.

**What can be left out**, first to last: needs by good (one store of bread is enough); rival houses that grow by
themselves (keep them as people in cards); the shut port on the chart (a card and a search chance will do); starts
beyond the first two; realm hinges beyond 878 and 1066. **Never build:** households by age, a sickness model, a
diplomacy engine, armies on the chart, a battle.

**For the user to decide:** (1) letting winters pass, which the "no time warp" rule may or may not allow; (2) hearths
as the measure, and the seven words; (3) whether a made war may change who holds a port.

**Sources.** The code and tables named above; `Design/economy_and_trade.md` 1.6 and 2.5;
`Design/research_scale_to_fleets.md`; the Medemblik and Traiectum dossiers; the Wikipedia pages named in the table
(read 4 Oct 2026). The wound and healing rates, the grain a head and the war numbers are guesses.
