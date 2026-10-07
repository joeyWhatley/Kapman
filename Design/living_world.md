<!-- Decided 6 Oct 2026 from the three proposals in Design/content/proposals and from the code as it stands. The file formats are in Design/content/SCHEMAS.md. Every number is a starting guess to tune in the headless harness. (?) = not checked against a source. A draft for the user to overwrite. -->

# The living world: decided

**The ask.** "Growth and development, dynamic story telling, different endings to stories, effectively unlimited wars
to play out, the campaign from start to finish, different starts, unlockables, realistic populations, help settlements
thrive, take quests, change history."

**The answer in one paragraph.** The world keeps four small ledgers under the 138 ships: **towns** (how many people,
how much bread), **holds** (who holds each port in which year), **wars** (a state of some ports for some seasons) and
**promises** (what the house has taken on, with a day). One **teller** chooses what reaches the player and when. Left
alone, every ledger follows the annals. The player's weight is the distance between a town and what history made of
it. Nothing new is steered, fought or managed: the player still hears, chooses, sails, arrives and reckons.

## What was taken from each proposal

- **The least:** history's own line for every town; a war as a state that pulls five levers the code has; a hinge as a
  gate on the chronicle; the count of cards a leg. Left: hunger read from the price alone.
- **The world:** bread in months, so the grain run is one number from a boat to a fleet; a war's phases; needs with a
  stake; a heat on every card. Left: realms that tick, four war makers at once, England rewritten after 878.
- **The story:** `heads = line x (1 + mark) - wound`, so any year can be asked without running the years before; three
  lanes; patience and an ending off-stage; three endings a place; the house book. Left: hearths as the measure,
  thirty hinges at once, a story record as a new class (flags and the ledgers already hold it).

## The one rule that keeps it small: the world speaks to the cards in flags

The encounter engine stays as it is. The ledgers **write flags** that any card can read with the grammar of `Cond.cs`
today: `flag.st_dorestad_heads >= 1200`, `flag.here_hungry`, `flag.war_delta_834 >= 2`. No new condition name is
needed, and the content checker already passes them. Two things are added to the engine, both small:

1. `flag <name> +n` and `flag <name> -n` in `Effects.cs` (today a flag can only be set). Wars and counts need it.
2. A second, closed list of **world effects** (`people`, `mark`, `store`, `heal`, `state`, `hold`, `tilt`, `war`,
   `job`, `open`, `work`, `drop`, `tell`), run by the ledgers, written beside a card's or a table's ordinary effects
   and never mixed with them.

## 1. Settlements and their people

**Data.** One file a trade region, `story/settlements/<region>.json`. For each port: its people in 793 as a low and a
high figure with a source, and a figure for each of the seven ages (`ages_793.json`) with extra dated points at the
sharp turns (Dorestad 834, 846, 863). That is the town's **line**: what history made of it.

**State, three numbers a port, saved.** `wound`: people lost and not yet back. `mark`: what the house has added or
cost for good, -0.5 to +0.5. `store`: months of bread in the place, 0 to 12.

    heads = line(year) x (1 + mark) - wound          households = heads / the port's household size (5 to 8)

**The steps.** On the 1st of each month every port takes one step; at Martinmas (11 November) one more.

| When | What | Number |
|---|---|---|
| Lammas (1 August) | The harvest fills the store | 9 months x the year's harvest: 1 in a common year, 0.5 in a famine of the chronicle, else 0.7 to 1.2 by the seed |
| The 1st of a month | A month is eaten | store - 1. Under 2 the place is **hungry**: food half as dear again, 1 in 200 leave. At 0 it is **starving**: 1 in 50 die or leave |
| A raid | The built raid, and a wound | the captives taken (6 to 20, built) and 2 in 100 |
| A burning; a sickness; a flood | A wound | 30 in 100; 10 in 100; the annal's count or 15 in 100 (?) |
| A month of siege | The store falls faster | store - 2, market shut to all but runners |
| Martinmas | The wound heals; the town is weighed; the muster | 8 in 100 of the wound returns, 25 if it was helped that year. Last year's change gives its word. Home ports send out traders by their size and replace lost ships |

**Two built systems read one factor**, `k = heads / the line's first figure`, held within 0.25 and 3. The market's size
is the table's plus `log2(k)` rounded (0 to 7): a town that doubles trades a step larger, a halved one a step smaller.
The raiders' `Defence` is multiplied by `k`. A burned market's lost step comes back as the wound heals (today
`sizeLoss` is for ever). `Market.Ruin` stays for the places history ends.

**How a merchant helps a town thrive.** No new verb of play.

| Help | Done by | What follows |
|---|---|---|
| Feed it | Selling food there while it is hungry. The dear price is the pay | The store rises: `sacks x 60 kg / (heads x 21 kg x the share of its bread that comes by keel)`. The first boat's hold is two and a half months for a village of 120 and a day and a half for Dorestad: one boat saves a hamlet, a town needs a house of ships |
| Take its order | A job the town posts when hungry, burned or besieged (section 4) | Kept: the wound heals at 25 that year, standing, `mark +0.01` |
| Warn it | Fresh word of a band, told to the reeve (a card) | The stock is hidden, no captives, the wound halved |
| Mend it | Timber, iron and tar after a burning | It heals at 25, not 8 |
| Fund a work | The `works` of its places file (`act:fund_work`) | `mark +0.02 to +0.10` and the work's own gift: a wall share and a watch (defence), a beacon (a day's warning), a well or hospice (sickness halved), a town granary (three months of store carried over a bad harvest) |
| Ransom or carry its people | Captives bought back at the raiders' market; families carried from a burned place | The wound falls by the heads brought |

**Thresholds.** A town's file says what opens when it grows and what closes when it fails: a shipyard that can build
the 6-ton trader at 400 households, a mint, a second jetty, a fair. Each sets a flag `open_<port>_<what>`; the places
file's site, work or ship waits for it. So helping a town is how new sites, works and ships are unlocked there.

**What does not turn.** The Kromme Rijn silts whatever anyone does. Birka's harbour rises out of the water. The file
says so (`fixed`), and the game says so in the town's own words. The player decides how a place ends, not always
whether: every place has three endings, and one is what happened.

## 2. Holds and wars without end

**Holds.** `story/wars/<realm>.json` carries, for each realm, dated rows of who holds which port: York is Danish from
867, Dorestad is Rorik's from 850, Rouen is Rollo's from 911. `port.realm` reads it. Tolls, laws, king's men and the
tollman's card already follow `port.realm`, so this one table moves them all.

**A war is not an army.** It is one record: two sides, a theatre of ports, phases with days, a tide from -3 to +3. It
pulls five levers the code has, and sets flags any card can read.

| Lever | Built in | In a war |
|---|---|---|
| Bands | `RaiderBrain` | Each side sends 1 to 4 bands a season at the other's ports |
| Blows | `Shocks`, and section 1 | Raid, burning, a siege by the month |
| Dear goods | `Market.Shock` | Grain, salt meat, horses and arms dearer in the theatre; loot cheap where the winner sells |
| King's men | `Patrols` | Twice as strict at both sides' ports; the levy asks for hulls |
| Word | news boards, flags | `war_<id>` (its phase), `war_<id>_tide`, `here_war`, `here_besieged` |

**The real wars** come on their dates: the same files bind the chronicle's entries into about forty wars (Kent 796 to
798, Godfred 804 to 811, the delta 834 to 863, the great army 865 to 878, Paris 885, the tribute years 991 to 1016,
1066). Their tide is history's. Nothing in them is drawn by dice unless a hinge has turned.

**The wars the world makes** come from `war_templates_793.json`: six shapes (a sea-king's summer, two lords' feud
over a toll, a border and a tribute, a rising, a siege of one town, a levy year), each with phases 30 to 60 days
apart, and a list of **grounds**: pairs of lords who really were at odds in given years, with a source. The teller
starts one when the player's waters are quiet. A phase is one draw: `p = sA / (sA + sB) + 0.07 x tilt`, where `s` is
the built defence of a side's ports times `k`, and `tilt` is what the house did for that side. Templates, grounds,
ports and years multiply to more wars than a campaign can hold; that is "effectively unlimited" without inventing
history. **The honest limits:** a made war never names a king, never kills a real person and never moves a crown; its
leaders are invented counts, reeves and chiefs with attested names; it is shown as news, never among the annals; and
the next real row of the holds table always wins.

**The merchant's part: seven plays, about thirty cards written once**, each reading `here_war` and its fellows.

| Play | The card | Pay and cost |
|---|---|---|
| Supply | A steward's order for grain, meat, horses, tar, sailcloth (arms are against the laws of 779 and 805) | 200 to 800 d a season; tilt +1 |
| Blockade | Ships lie across the river: run it in fog or dark, pay the watch, or turn back | Inside, food at two or three times its price; caught, the cargo is seized |
| Envoy | A man or a letter across the lines by a day | Standing, a charter; the other side's king's men |
| Ransom | Silver north, named captives home | 80 to 250 d a head from kin or church; the town's wound |
| Refuge | People, a minster's books or its plate carried out before the blow | Texts for the chest, a friend, heads moved |
| Profit | Loot bought cheap and sold to both sides | The best margins in the game; standing lost as fast as the news sails |
| Ruin | A plot burns, a debtor dies, the boat is called up | A loss the house carries; a warning card always comes first |

One boat can earn a tilt of about 3, a house under orders up to 8. One boat cannot win a war. It can save a town, a
man or a library.

## 3. Hinges of history

A hinge is a real entry of the chronicle with a test. `Chronicle.Advance` asks before it tells: if the hinge's
`turned` condition holds on the day, the entry is not told; the written `instead` entries are told in its place; later
entries that rested on it are dropped; the holds table may be overridden; and the flag `changed_<event>` is set, as
`CHAINS.md` already asks. News, shocks and markets need no change, for they hear whatever is told. After that the
ledgers carry the change by themselves: nobody writes "Dorestad in 950"; it is what its people and bread have made it.

Three classes, by what a merchant could really bear on:

| Class | Turned by | Rewrites | How many |
|---|---|---|---|
| A place | One boat, a season, news brought in time | 1 to 3 entries, a town's line | about 30, one or two a region and an age |
| A region | A work paid for, a town kept above a count, a factor on the spot | 3 to 8 entries, holds of a few ports | about 8 |
| A realm | A house of ships over two lives; tilt 5 | 10 to 25 written entries | 1066 first; no other until it has been played |

The first three to build: **Dorestad** (the last sack of 863 does not come if two of three hold: the wall share and
watch paid for, 1,200 heads or more, Rorik's man a friend; the river still leaves, so it lives on as a small toll
town); **Reric 808** (its merchants warned and carried where the player takes them); **the fleet of 1066** (Harold's
ships went home on 8 September, their food gone: a house that lands 36 tons of food on the south coast between Lammas
and that day keeps them at sea). Changed entries are always shown apart, under "Otherwise than the annals tell", in
the theme colour. A turned hinge is the best payout in the game: a finale and a grand reward of `CHAINS.md`.

## 4. Quests: jobs and chains

| | A job | A chain |
|---|---|---|
| Comes from | A ledger: a town's state, a war's phase, a person's want. Made from a template, as often as the world has cause | A writer: 8 to 14 cards, once a house |
| Is | A promise with a day: bring (a `commission`), carry (a person or a letter), or find out (news of a port no older than so many days) | A story with flags `ch_<id>_<n>` and three endings |
| Pays | 20 to 120 d over the market for a town's order, 200 to 800 d in a war; standing; the town's ledger | 600 to 2,500 d and a grand reward |
| Fails | It lapses: `broke_commission`, the stake falls on the town, another ship may fill it first | Never with nothing: it goes on by another road |
| Words | Lines by cause, people and year, written once (about 150 lines) | Its own |

The house holds **three promises and one vow** (a life's undertaking: three crossings at its own cost, a church, a
feud ended). A fourth asks which to break. Each shows its day and pay before it is taken. A job met at a hinge pays as
a finale.

## 5. Starts and unlockables

A start is one row of `starts_793.json`: the day, the home, who you are, the ship, silver and debt, standing, waters
known, flags, the first card. Because a town's people are read off its line and the holds off their table, a later
start needs no run of the years between: the world is **set** at that year, as it is now for 793.

| Ship first | Who | The burden | Opens |
|---|---|---|---|
| 793, Dorestat | A Frisian with a Gislinge boat | 480 d owed to the count (built) | From the first |
| 793, Eoforwic | A layman sailing an abbot's ship for a third | The ship is not yours; Lindisfarne is thirteen days old | A first debt paid |
| 808, Sliesthorp | A merchant of Reric carried off by Godfred | No home; a king who owns your plot | A campaign reaches 808, or the chain `reric_808` ends |

Later, each costing writing more than code: 841 Duiblinn, 866 Eoforwic, 886 Lundenwic, 991 Gipeswic, 1020 Ripa, and
"the heir of the last game's house". **Unlockables** live in a small **house book** beside the saves that outlives a
campaign: starts opened, endings reached (a page each), texts read, ship classes sailed, and heirlooms, of which a new
campaign may take one. In a campaign, everything else is unlocked by the flags of `CHAINS.md` and by a town's
thresholds.

## 6. The campaign and its endings

**The honest sum.** 793 to 1066 cannot be sailed day by day. The campaign is about nine lives, and a life is the
seasons the player chooses to sail. The winter screen (designed, not built) gets one more choice, **stay ashore this
year**: the same screen comes again next Martinmas, with the year read back in five lines (each town known, each war,
each ship of the house by a throw on her skipper's skill). The clock's rate never changes; this is waiting by choice.
It stops by itself for a death, a war in reach, a hinge two years off, or an heir come of age.

**Seven ages**, as `ages_793.json` already names them, each with its own danger and its own rung of the house
(`research_scale_to_fleets.md`): the emperor's peace (the boat), the raiding years (the season), the great armies (the
partnership), walled towns and eastern silver (the house), the quiet years, tribute then one king (the name), the last
years.

**Three layers of ending.** A **life** ends by age, wreck or ruin and gets one entry (the twelve of
`winter_793.json`). A **place** ends one of three ways. The **house** is weighed at Christmas 1066, or sooner if it
has no heir: that is an ending, not a reload. `endings_793.json` holds the pages; the highest whose condition holds is
told, in the Chronicle's voice, with which England there is (William's, Harold's, Harald's, or the land divided) and
what became of the three places the house did most for.

What the house became, each a rung the period had and each read from flags the save already holds: thegn-worthy and
gone to the land (`unlock_title_thegn`, three crossings); a sworn brother of the merchants of Tiel or Sigtuna; the
town that stands (a `changed_` flag and a town above its line); a house of ships; the church's house (the last heir
takes the cowl, the chest goes to a minster); far-farers gone to Dublin, Iceland or Kiev; sea-wolves, rich on tribute
and shunned in Christian ports; or the line ends, told as the Chronicle would tell it.

## 7. The storyteller

One small class over the built `Scheduler`, seeded by the world's seed and the day, so a save reloaded tells the same
tale. It does three things only: sets flags, schedules cards, and gives the draw one more factor, `m_story`.

1. **Truth first.** A chronicle entry comes on its day. The teller never moves, hides or invents one.
2. **Heat.** Every card has a heat: 0 colour or relief, 1 a choice, 2 a cost or danger, 3 a crisis. It is given by a
   table of card-id patterns in `storyteller_793.json`, so no card file changes.
3. **The leg's count.** One to three cards a leg. Heat is summed over the last three legs against a band (2 to 5 in
   stage 1, up to 8 in stages 4 and 5). Under the band, heat 1 and more comes twice as often; over it, heat 2 and more
   a quarter as often. After any card, six game hours at a fifth. Twelve hours with none: the next draw is tripled.
4. **The pulse.** One number, 0 to 1: what the house lost lately (a man, a cargo, a third of its silver, the home
   port struck: +0.3 each), halving every 20 days. Above 0.6 only heat 0 and 1 are drawn and undated steps wait.
5. **Three lanes**, one open story in each: World (a war's or a hinge's step), Place (a town's need), House (crew,
   kin, a rival, a letter). A lane empty for 20 days is filled: Place from the nearest known port whose word is not
   "steady", House from a crewman's want, World from the war templates.
6. **The season's count**, each spring, within the house's reach (ports heard of within 60 days, ports with a plot).
   A war counts 3; a raid, burning or famine 2; an open chain step 2; a promise 1. Under the floor it starts one
   thing; over the ceiling it starts nothing and chain starts weigh a third. Floor and ceiling rise with the keels.
7. **Patience.** A story that waits past its days (30 to 90) ends by its default off-stage and comes as news, as old
   as the ship that brought it. The age of news is the flood gate.
8. **Caps.** One made war in reach, none within two years of a real one there; two chains open; three promises; never
   two crises; five quiet days after one; every danger shows hours ahead.

## 8. How they feed each other

The chronicle and the wars deal blows to towns. A town's state posts jobs. A job kept or broken changes the town and
the house's standing. A town's size moves its market, the raiders' choice of target and the traders it sends out.
Hinges read the towns and the flags, and rewrite the chronicle and the holds; the holds and the grounds decide which
wars can come. The teller chooses the order. Every ending is read from the flags all of this leaves.

## 9. What the player sees (nothing new on the table, no new screen)

| Where | Added |
|---|---|
| A port's mark on the chart | Its dot by size, and one sign as old as the news: a ring (war), a flame (burned or shut), hollow (hungry), grey (a ruin). "Hungry, 9 days ago" |
| The port panel's first line | "Dorestat. About 2,400 souls, thinning. Bread for three months. The count's peace. Wants grain and timber." |
| The day stick | A notch for each promise (a seal), a war's next phase (a spear), a town's need taken (a sheaf): six at most, gold ahead, red when passed |
| The chart's layers | One more, War: the theatre hatched, a fleet's mark as old as the word |
| The chest's log | Three headings: The annals (with sources); Heard (news and made wars, with source and age); Otherwise than the annals tell |
| The winter screen | The year read back: a line a town known (up or down, and why), a line a war; sail, or stay ashore |

## 10. The build order: one step at a time, each with its test

Needed first, from the chart game's own list: cards at berths and at sea in the chart scene; the day stick; the
winter screen. Steps 1 to 7 need none of the three and can start now. Tests are EditMode and headless unless said.

| # | Step | Nights | Its test |
|---|---|---|---|
| 1 | `flag <name> +n / -n` in `Effects.cs` | 0.5 | Parse and run: a flag at 2, `+3`, reads 5; a lapsing flag keeps its day |
| 2 | `Settlements`: load `story/settlements/*.json`; `Line(port, year)`, `Heads`, `Households` | 1 | Dorestad 793 is its best figure; a year between two points is between them; every port of `ports_793.json` has a row or a warning |
| 3 | State and steps: wound, mark, store; the monthly and the Martinmas step; save and load | 1.5 | Ten untouched years: heads within 2 in 100 of the line; a famine year: store under 2, hungry; two runs with one seed give one hash |
| 4 | Hooks: `Shocks` and raids make wounds; size from `k` (replacing lasting `sizeLoss`); `Defence x k` | 1 | Burn a town: a step smaller, the step back within ten years, five if helped; the economy tests still pass |
| 5 | The world's flags (`st_<port>_*`, `here_*`), the panel's line, the chart's sign | 1 | A card with `flag.here_hungry` is drawn only at a hungry port; the line reads as designed |
| 6 | World effects (`people`, `mark`, `store`, `heal`, `state`, `open`, `work`); food sold to the hungry fills the store; thresholds | 1.5 | 11 sacks at a village of 120 raise its store by 2.6 months; a threshold sets `open_<port>_<what>` once |
| 7 | Jobs: templates from a town's state on `Commissions`; the cap of three; the stake | 1.5 | A hungry town posts a grain job within a month; kept, its wound heals at 25; broken, `broke_commission` |
| 8 | The teller A: heat table, the leg's count, the pulse, `m_story` | 2 | Twenty headless years printed by the leg: no leg over three cards, no four real minutes empty, never two crises |
| 9 | Holds: dated rows; `port.realm` reads them | 1 | Eoforwic's realm changes on 1 November 866; the Frankish tollman's card no longer comes there |
| 10 | Wars: the record, five levers, phases, flags; the real wars; thirty cards | 3 | The delta war 834: its phase flag climbs on the entries' days; bands sail at theatre ports; a supply job is posted |
| 11 | The teller B: lanes, the season's count, patience; war templates; raid pressure to 1066 | 3 | A hundred headless years: 2 to 6 made wars a decade after 850, none within two years of a real one in its ports, one at most in reach |
| 12 | Hinges: the gate in `Chronicle.Advance`; `drop`, `tell`, `hold`; the third heading in the log | 2 | With the flags set, `863-dorestad-last` is not told and `changed_dorestad_863` is; without them it is told |
| 13 | The muster: traders by a home port's size, lost ships replaced | 1 | A doubled port sends out more sails the next spring; a ruin none |
| 14 | Starts: the table, Dorestad taken out of the code, the world set at a year, the house book | 2 | Start 808 at Sliesthorp: Reric is gone, the chronicle stands at that day, the purse and ship are the row's |
| 15 | Lives and years ashore (needs the winter screen): `generation` fed, succession, the year read back | 3 | A staged death: the heir has the chart, the chest, one feud and one friend; five years ashore tell five read-backs |
| 16 | Endings: the weighing at Christmas 1066 or with no heir; the last page | 1 | Sets of flags give the expected ending ids |

About 26 nights of code. The data is the larger work and can start today: 50 town rows, about 40 holds rows, about 40
real wars, six templates with their grounds, three hinges, three starts, eight endings, the teller's table, and about
150 more real annals for 825 to 1066, where the chronicle is thin (160 of its 582 entries).

**Left out for good:** people counted by age, sex or craft; a sickness model; production chains; realms with
treasuries, marriages or diplomacy; armies on the chart; a battle; a second chronicle; a quest log screen.
**Can wait:** four of the six templates (begin with the sea-king's summer and the siege); starts beyond three; realm
hinges beyond 1066.

## 11. Yours to decide

Each has my recommendation first.

1. **The years between voyages.** How does a house reach 1066?
   (a) *Stay ashore one winter at a time: the winter screen comes again, the year read back (recommended: no new
   device, the clock's rate never changes).* (b) Stay ashore until a notch the teller names (an heir of age, the eve
   of a hinge). (c) No staying ashore: each age is its own short campaign, joined by the house book.
2. **Wars the world makes.**
   (a) *Small news-wars with invented lords; a port may change hands between lords below a king, and the real holds
   table always wins (recommended).* (b) Made wars move prices, tolls and bands only, never who holds a port.
   (c) Real wars only, and more of them written.
3. **How far history may bend.**
   (a) *Towns and regions freely, and 1066 as the one hinge of a realm (recommended).* (b) Towns and regions only: the
   crowns of 1066 stand. (c) Also 878, 911 and 1016, each with 10 to 25 written entries. (d) Nothing bends; the
   player only rides history.
4. **Dorestad past 863.**
   (a) *It can be saved from the sword but not from the river: it lives on as a small toll town (recommended).*
   (b) It can be kept great. (c) It cannot be saved, only left in good order.
5. **A town's people on the screen.**
   (a) *A round number and a word: "About 2,400 souls, thinning" (recommended: you asked for realistic
   populations).* (b) Words only: hamlet, village, market, town, great town. (c) Households.
6. **The first starts.**
   (a) *Three: Dorestat 793, Eoforwic 793, Sliesthorp 808 (recommended).* (b) Dorestat only until the campaign is
   whole. (c) All eight, the late ones thin until their waters are written.

**Sources.** The three proposals; the code named above; `ages_793.json`, `works_793.json`, `unlocks_793.json`,
`winter_793.json`; the Dorestad, Hamwic, Medemblik and Traiectum dossiers. Rates and war numbers are guesses.
