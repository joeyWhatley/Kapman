<!-- 6 Oct 2026. Pages fetched by name, listed at the foot; no web search. (?) = from memory, not checked.
(D) = a number I judged, to tune in the headless harness. A draft for the user to overwrite. -->

# The long game: 273 years

**In one line.** Nobody plays 273 years. They play about **nine lives of five chosen years each**, in a world that
changes its rules seven times, and the house is weighed by what it kept its word on.

## 1. What other games teach

| Game | What keeps it alive | Where it dies |
|---|---|---|
| **Crusader Kings III** | The ruler dies; the dynasty does not. Renown is earned by living kin, never lost, and buys legacies every later ruler keeps: the first costs 250, each next one 500 more. Thirteen levels of "splendor" change how strangers greet a child of the house. | Past a size nothing can hurt you. Lands split among sons at death (?): players hate it and need it, for it is the only reset. | ck3.paradoxwikis.com/Dynasty |
| **Civilization VI and VII** | VI: each era is scored; a poor era gives a dark age, and a dark age well used gives a "heroic" one (?). VII: three ages, each ending in "crisis events which simulate societal disruption". | VII made the player change people at each age. Players called it "immersion breaking"; "the abruptness of the ages system" drew "the most heavy criticism"; a later update let one people be played throughout. |
| **Old World** | A turn is a year (?). Rulers "age and die, and an heir must be ready". Goals are offered as ambitions, a few at a time, and winning is a count of ambitions kept (?). The designer cut the span on purpose: 6,000 years was "a blessing and a curse". | Fewer years, more felt. |
| **RimWorld** | Danger is a sum of wealth and heads, times an **adaptation factor from 0.4 to 1.47**. A death takes 20 to 30 "days" off it; a quiet half-day adds about one. A loss buys peace. | Wealth is a hidden fine on doing well. Players learn to stay poor. | rimworldwiki.com/wiki/Raid_points |
| **Dwarf Fortress** | No win. "Every fortress, no matter how successful, will eventually fall." The fall is kept: Legends mode holds "any figure who has lived or died", and a later game "can visit their retired or ruined fortresses". | Nothing new to fear once the fort is safe. |
| **Rogue Legacy** | Death is the shop. Choose one of **three** heirs, each with a quirk. Gold goes into the manor, and "any unspent gold must be paid to Charon": no hoarding across a death. | Only numbers grow. The castle is the same castle. |
| **Massive Chalice** | 300 years. A hero is retired as regent to found a line; a fallen hero's weapon "may become a Relic ... passed down to younger members of their house". | "By mid-game, what was novel and enticing becomes a slog"; "too many quickly aging, mortal characters". |

Soren Johnson's warning sits over all of them: "Given the opportunity, players will optimize the fun out of a game",
and once a best way is known "the knowledge cannot be ignored or forgotten". A long game needs the best way to change.

**Seven findings.**

1. **The house lasts; the person does not.** What crosses a death must be a few named things, not a bank balance.
2. **A death should make the house smaller and the world wider.**
3. **The world must change by a calendar the player can see**, not only by the player growing.
4. **Never take the player's identity at an age turn.** Change the world's rules; leave the house alone.
5. **A fall must open the best story**, not be a fine.
6. **Danger that reads wealth must be open, and must ease after a loss.**
7. **Few people, many years.** Massive Chalice shows the cost of the other way round.


## 2. Proposals

### P1. The sum: five chosen years a life

**The rule.** A head of the house has about 30 years. The player sails the ones worth sailing and stays ashore for
the rest. The game never forces a year at sea and never skips one without telling it.

**The numbers (D).**

| Thing | Number |
|---|---|
| Lives from 793 to 1066 | 8 to 10; a head takes over at about 22 and hands over or dies at about 52 |
| Years sailed in a life | 4 to 7 of about 30. Under 3, the life is remembered as "kept the house" |
| A full sailed year | 3 to 4 hours (the 1012 year of the fleets plan) |
| A short year: one voyage of your own, then winter | 60 to 90 minutes |
| A year ashore | 2 minutes: five lines read back, one stick to break |
| The whole campaign | 90 to 120 hours. Fifty hours is four or five lives: about 793 to 930 |

Staying ashore stops by itself for the causes already written (a death, a war in reach, a hinge two years off, an
heir of age) and for an age turn, added here. Absence costs something seen: a run not crossed in 10 years needs one
crossing again, and a host forgets you after 6.

**The player sees.** On the winter page, under the day stick, a longer stick: the life. Thirty notches, sailed
years cut deep, years ashore shallow, the age turn and known hinges marked ahead in Byzantium.

**To build.** `Life` (born, head since, years sailed, years ashore), saved. `RunLog` gets a last-crossed year. The
long stick reuses the day stick's drawing. Test: twenty years ashore from a seeded save give one `StateHash` twice,
and stop on each of the five causes.

### P2. The arc of one life: three parts, and the year together

**The rule.** A life has three parts. Nothing is a stat; each part changes what the world lets you do.

| Part | Ages | What is different |
|---|---|---|
| **The son** | 14 to about 22 | Aboard the father's boat. One year is played this way, the old head's last sailed year: **the year together** |
| **The head** | 22 to about 45 | The game as designed. Your oath counts. Vows can be taken |
| **The old man** | 45 on | Each winter a draw: 2 in 100 at 45, 2 more each year (30 at 59). It always warns one winter ahead, by a card (a cough, a bad knee). After the warning, a crossing after Michaelmas is "doubtful" |

**The year together.** When the heir is 14 or more, the head may take him aboard for a year. Every run crossed that
year counts as the heir's one crossing. Every person met keeps their feeling for him whole at the succession, not
halved. The father gives one line of counsel on each card, true as far as he knows. A house that never does this
still passes on, colder.


**The player sees.** A fifth face aboard, smaller. His line under each card, in the theme colour. The warning card.

**To build.** The death draw on `Life`, its warning card scheduled first. A flag `heir_aboard`; the succession reads
it for runs and feelings. A `counsel` field in the writing files, shown only under that flag (about 120 lines).
Test: 1,000 seeded lives end between 46 and 66, median 55 to 58; no death ashore without a warning the winter before.

### P3. The handover: three heirs, one choice, nothing hoarded

**The rule.** At a death or a handover the house is divided (fleets plan, section 6). The player then chooses whom to
play from **up to three** of those the ranks in `winter_793.json` allow. Each offer is one face and three lines.

> **Liudger, 24.** Sailed with his father. Has the Danish tongue. Keeps the keel *Gull*, and the feud with Odo of Tiel.
>
> **Wulf, the foster-son.** The six-oar boat only. Blood kin cool for a life. A hall at Ribe from the start.

**Nothing hoarded.** Loose silver is divided as already written (two thirds to the heir, a third to the widow). Only
silver that **became something** passes whole: a plot, a keel, a bell, a dowry. So the last winters of a life are
spent, as Rogue Legacy's gold is. **One heirloom is named by the dying man** and gains a line: "the scales Folkmar
weighed with at Birka, 829". An heirloom with three lines gives its small gift twice.

**The player sees.** One page, clock held: his line in the chronicle, his stone, the boats going to faces, the offers.

**To build.** `Succession.Offers()` from the ranks, capped at three; `Divide()` as fleets step 13; `Heirloom.lines`.
Test: a staged death with two grown children and a foster-son gives three offers, and each choice gives the keel,
feud and friend its offer named. **A mismatch to settle:** `winter_793.json` says the heir keeps every hull and
`unlocks_793.json` says eight keels; the fleets plan says the division, and six.

### P4. The age turn: one page, one rule changed, nothing taken by rule

**The rule.** In the first winter of a new age the winter page opens on a spread, **"The world your father knew"**:
three true changes from `ages_793.json` (`new`, `gone`, `goods`), each with the port it touches. The house loses
nothing by the turn itself. What changes is the best way to play, so last age's habit stops being the answer.

| Age | The one rule that changes in play | The run that pays |
|---|---|---|
| **1. 793 to 833** | One penny, one toll law: tolls fixed and known | Rhine wine to the English wics |
| **2. 834 to 864** | A port's word can be "burned". Word of the fleet is worth more than word of a price. Home must move by 863 | Grain into a struck town; loot and ransom, at a cost in name |
| **3. 865 to 899** | Two purses: counted pennies in the west, weighed silver in Danish ports. A charter dies with its king | Horses and corn to an army; walrus ivory from the north |
| **4. 900 to 954** | Trade only in a walled town, before the reeve: no beach sale over 20 d | The long road east for silver: a whole season |
| **5. 955 to 990** | Few raids. At a recoinage old pennies lose part of their worth on a known day (?). Eastern silver stops | Margins, standing, charters |
| **6. 991 to 1041** | To 1016 a tribute year can take a ship for the levy. After it, the safest seas and 60-ton hulls held in halves | Food into a paying town; then bulk under one king |
| **7. 1042 to 1066** | A strait can be shut each summer. Kings call out ships | Wool out, cloth back, through Flanders |

Each age also lays **three deeds** on the table that first winter, drawn from its stories and hinges (age 1: be at
Reric before Godfred; carry the black stones; reach Birka). They are vows that last an age. One may be taken.

**The dark year.** A life that begins after a ruin (no heir, the home burned) is offered deeds worth double in name,
for that life only. The fall opens the best page.

**The player sees.** The spread, once; then the three deeds as small things on the winter table.

**To build.** `AgeTurn` picks three entries by the ports the house has heard of. Seven rule switches, each an
existing lever (toll table, `Market.Ruin`, two purses, a sale-place check, a recoinage shock, the levy card, strait
closures). Twenty-one deeds as flags `deed_age<n>_<id>`. Test: a save moved to the winter of 834 shows the spread
once, naming only known ports.

### P5. The weighing: stones, not silver

**The rule.** The house has one lasting measure, its **name**, and it is never spent. Each life leaves one **stone**:
its entry among the twelve of `winter_793.json`, and notches.

| A notch for | Notches |
|---|---|
| A vow kept | 1 |
| An age deed | 2 (4 in a dark year) |
| A town above its line at your death, by your mark | 2 |
| A hinge turned | 3 |
| Five texts brought to the chest | 1 |
| A man ransomed; a kin house founded | 1 |
| A broken oath; a town's order taken and left | minus 2 |

**The name** is the sum over all stones, said as a word (D): under 5 *unknown*; 5 *known on the bank*; 12 *known
in the delta*; 25 *known across the sea*; 45 *a name in three realms*; 70 *told of*. A good life cuts 6 to 10.

**What the name does.** At a succession, standing is halved toward the name's floor, not toward nothing (at *known
across the sea*, no port where a stone's deed was done falls below "known"). A stranger's first offer starts one
step warmer. And it sets which endings of `endings_793.json` can be told. Silver enters the weighing once: as the
things it became.

**The player sees.** In the chest, **the house book**: a row of up to ten small stones, each with a face, two dates,
one line and its notches. Click one to read that life's log. At Christmas 1066 the row is the last screen, with the
England there is.

**To build.** `Stone` (life, entry id, notches with causes), `HouseName.Word()`, the succession's floor. The book
reads saved stones only, so it outlives a campaign. Test: sets of flags give the expected notches; the floor holds
a port at "known" through a succession.

### P6. Loss that does not end the game, and danger that eases

**The ladder of loss.** Every rung has a way back and is told with its cause.

| Lost | The way back |
|---|---|
| A cargo | One good leg |
| A man | His kin, a Mass, a stone |
| The boat | The six-oar boat, never sold: a season in the home river |
| The home port | The chart, the chest and kin houses remain. Move the house: one winter, 480 d |
| The head | The heir, by P3 |
| All heirs | "A new man of the same bank" with the dead man's chart and the stones: a dark year (P4) |

**Danger eases after a blow.** The teller's pulse already quiets the cards for 20 days. Add a slower one for the
house: the word said of you drops **one step for two years** after the loss of a keel, a plot or the head. The three
lines of why say so: "They burned your store at Quentovic. Nobody envies you this year." This is RimWorld's
adaptation, shown.

**Danger comes from the calendar, not the purse.** The age sets how much danger there is (little in age 5, most in
age 2). Seen wealth only chooses *where* a blow lands. A small house in 845 is in danger; a great one in 970 is
mostly safe. So staying poor is never the best way to play.

**The player sees.** The word beside the tally, one step lower, and its three lines of why.

**To build.** `HouseWord.pity` (steps, until day). `RaiderBrain` reads the age for how many bands, the word for
where. Test: over seeded decades, raids on house stock fall by a third or more in the two years after a loss, and
the bands a decade follow the age whatever the house owns.

## 3. After fifty hours, a player should be able to say

1. "I paid my father's debt to the count."
2. "I was at Birka before any Frisian I knew."
3. "I lost the *Gull* off Texel and came home in the six-oar boat."
4. "I sailed a year beside my father, and then without him."
5. "Dorestad burned four times. The fourth time our store was empty, because I had word."
6. "I moved the house to Tiel."
7. "My brother's house is at Hedeby. He carries my word for nothing."
8. "I fed a village through a famine winter with one hold."
9. "Three stones stand. My grandfather's has the most notches, and he died poor."
10. "The annals say Reric's merchants were carried off. Mine say otherwise."


## 4. Order, and your decisions

Build P1 and P6 first (they need only what is planned), then P2 and P3 with fleets step 13, P5 with the endings, P4
last (mostly data). About 9 nights of code (D), plus 120 counsel lines, 21 deeds and 7 spreads to write.

1. **Years sailed in a life:** (a) *4 to 7, the player chooses (recommended).* (b) Every year, kept short. (c) One great voyage a life.
2. **Whom you play at a death:** (a) *one of up to three (recommended).* (b) Always the first in rank.
3. **The name:** (a) *a word, never spent (recommended).* (b) A number that buys things, as renown does.
4. **Age turns:** (a) *one rule changed, nothing taken (recommended).* (b) Told as news only.

## Sources

Fetched 6 Oct 2026 (Stellaris and The Guild 2 pages were read too and gave nothing used here): ck3.paradoxwikis.com/Dynasty; rimworldwiki.com/wiki/Raid_points;
en.wikipedia.org/wiki/ Massive_Chalice, Rogue_Legacy, Civilization_VII,
Old_World_(video_game), Dwarf_Fortress; designer-notes.com/game-developer-column-17-water-finds-a-crack.
Refused: civilization.fandom.com (Civilization VI era score, VII ages), so those rules are from memory (?). In the
project: `ages_793.json`, `winter_793.json` (inheritance, remembered), `unlocks_793.json`, `scale_to_fleets_plan.md`,
`living_world.md`, `research/fleets/dynasties.md`, `ages.md`, `DECISIONS.md`. Not checked: the recoinage rule of
age 5, CK3's partition, Old World's ambition count and year turns, when the Civilization VII update came.
