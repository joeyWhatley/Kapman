# People and standing: how they should matter over 273 years

6 October 2026. For the question "make this more in depth and thoughtful", the part about people: the portrait panel,
the crew, hosts, rivals and heirs. (?) marks what I could not check today. Game facts marked "from memory" were not
re-read on a page today. Nothing here is built; every number is a first guess to tune.

## 1. What is already there

- `Reputation.cs`: a standing for each port and region, -100 to +100, moved by deeds whose news travels with ships.
- `NpcRegistry.cs`: each person has a feeling toward the merchant (six words), silver owed, and 12 memories. An
  heir takes half the feeling. Its own note says what is missing: no fading, and no talk between people.
- The spine and the places files: people with a trait, a want, a fear, a voice, looks and ties.
- Decided: four named crew; three talks a call; heirs who take "one feud and one friend"; the host of three nights.

Missing: **kinds of tie**, **favours that can be spent**, **change over time**, and **one place to see it**.

## 2. Findings from games

| Game | What it does | What to take |
|---|---|---|
| **Crusader Kings III** | Opinion is a sum of named lines (from memory). A **weak hook** is spent in one use and lapses in about 10 years; a **strong hook** forces a yes, can be used again after 5 years, and stops the person acting against you. **Stress** rises when you act against your own traits (wiki, read today). | Favours you hold and spend. Reasons as lines, never a bare number. Acting against a nature costs. |
| **Wildermyth** | Heroes come back in later tellings as legends, in five tiers (wiki, read today). They age, retire, and wear their changes on the body (from memory). | Traits are earned and seen. An old hand does not vanish. |
| **Darkest Dungeon** | Stress 0 to 200. At 100 a test: usually an affliction, sometimes a virtue. At 200 a heart attack (wiki, read today). | The breaking point is a roll with a good side. |
| **Sunless Sea** | Few officers, each with one long story across far ports; its end changes him for good (from memory). | A crewman's want is a route, and its end is the reward. |
| **Battle Brothers** | Traits, a wage, a mood, lasting injuries, a list of the dead (from memory). | Mood in plain view. A roll of the dead. |
| **Pentiment, Citizen Sleeper** | A town over 25 years where your words come back; people as clocks you fill by turning up (from memory). | Memory quoted aloud. Three talks already does the second. |
| **King of Dragon Pass, Six Ages** | A ring of seven advisers, each speaking from his own skill and god; they age and die. Promises to other clans are called in (from memory). | Advice in a voice. A list of what is owed, both ways. |

**The common rule.** A relationship matters when it is (1) a named reason, not a number; (2) something you can spend
or lose; (3) remembered aloud; (4) changed by time whether you act or not.

## 3. Findings from the period

- **Kin.** A man answers with and for his kin. A killing is paid by wergild, before witnesses, or it is feud. Lex
  Frisionum: a free man 53 and a third solidi, a noble 80 (?); at 12 d a solidus, 640 d and 960 d (?).
- **Feud ends by settlement,** not victory: arbiters both sides trust, payment in terms, then a marriage or a
  fostering to hold it (W. I. Miller, *Bloodtaking and Peacemaking*, 1990, for Iceland: LATER).
- **Fostering.** A child raised in another house binds the two houses for life (Norse and Irish custom).
- **Oaths and lords.** A man without a lord has no one to answer for him. Charlemagne had every free man swear
  faith in 789 and 802. An oath is sworn with oath-helpers who stake their own names.
- **Gifts.** A gift asks for a gift: Havamal 41, 42 and 46, in the chest already. M. Mauss, *The Gift*, 1925.
- **Hospitality.** Laws of Hlothhere and Eadric, c. 15: he who lodges a trader three nights answers for him.
- **Sworn partners.** The Norse *felag*: men who pool goods and share the outcome; named on rune stones.
- **The church.** Sponsoring a baptism makes spiritual kin, across peoples (J. Lynch, *Godparents and Kinship in
  Early Medieval Europe*, 1986). Minsters wrote friends' names in a book of life. G. Althoff, *Family, Friends and
  Followers*, 2004: kin, sworn friendship and lordship are the three bonds, made in public with food and gifts.

**The common rule.** Ties are few, named, public, two-sided and inherited.

## 4. Proposals

### 4.1 Bonds: a tie has a kind

**Rule.** Feeling stays as it is. On top of it a person may hold one or more **bonds** with the house. A bond is
made by a public act, belongs to the house, and passes to heirs on both sides.

| Bond | Made by | Gives | Costs |
|---|---|---|---|
| **Kin** | birth, marriage | feeling never below "easy"; will stand surety | their feuds reach you |
| **Foster** | a child raised 7 years in their house | as kin; their tongue; a great claim each way | the child is away 7 years |
| **Gossip** (god-kin) | sponsoring a baptism | as kin, across peoples; church standing +5 there | heathen kin of his: feeling -10 |
| **Sworn** (felag) | one shared venture, then an oath | a partner who sails with or for you | his losses are half yours |
| **Guest-friend** | three nights lodged in one year | a fourth talk at his port; 4 hold cells stored; he swears with you in court; he warns you before the town refuses you | a gift at the first call each year; your crew's deeds there count double on him |
| **Lord** | an oath in his hall | searches halved and the toll eased in his district | one service a year, cut on the day stick; breaking the oath is a deed worth -25 everywhere |
| **Feud** | a killing, a great wrong | nothing | section 4.6 |

**Numbers.** One guest-friend a port, one lord at a time. At most 12 living bonds beside kin. A bond not visited
in 5 years goes cold: kept, but it gives nothing until a gift renews it.

**The player sees** a small mark by the name (a ring, a cradle, a font, two hands, a hearth, a banner, a broken
knife) and the same mark on that port on the chart.

**To build.** `Bond { kind, since, lastKept }` on `Relationship`; a feeling floor by kind; condition
`npc.<role>.bond = <kind>`; effect `bond <role> <kind>`; the winter check; chart marks.

### 4.2 Claims: favours you can hold in your hand

**Rule.** A favour done becomes a **claim**: a notch on a tally stick, as Hartwin cuts them. A **great claim** is
for a life saved, a wergild paid, a child fostered. Others hold claims on you and call them in by card; to refuse
is a deed (`broke_word`).

| | Small claim | Great claim |
|---|---|---|
| Lapses | 7 years, or at either death | never; heirs hold it |
| Use | once | once every 3 years |
| Most held on one person | 3 | 1 |
| Buys | a talk that does not count; a word to a third person (start at "friendly"); surety to 120 d; 30 days' delay; one search waved off | surety for the whole debt; an oath-helper in court; a winter's shelter; fostering a child; a berth in his convoy |

**The player sees** notches under the portrait: cut on the top edge are yours on him, on the bottom edge his on
you. Hover gives the reason and the year: "He owes you: the letter carried through the ban, 795."

**To build.** `Claim { holder, on, great, reason, day }` in the registry; effects `claim <role> +1 [great]` and
`call <role>`; condition `claim.<role>`; the yearly lapse; claim options on the talk panel.

### 4.3 Gifts

**Rule.** One gift a person a year counts. It must be a thing he **prizes**, worth at least his rank's floor.
It lifts feeling and puts the return on him: within a year he gives back a true rumour, a good at cost, or a small
claim. A gift he cannot honourably take (a heathen thing to a bishop) costs feeling.

**Numbers.** Floors: a neighbour 6 d, a reeve or steward 24 d, a count or abbot 60 d, a king 240 d. Feeling +8,
or +12 for what he wants most. Three times the floor makes the return a small claim for sure.

**The player sees** one line on the talk panel, "Would be glad of: wine, a hawk", once a talk has learned it.

**To build.** `prizes` and `rank` on people rows; effect `gift`; a timer that schedules the return card.

### 4.4 Memory that fades, is kept, and is told

**Rule.** (a) Each winter, feeling moves 10 in 100 of the way back toward the bond's floor. (b) The three
memories with the largest effect are **carved**: never dropped from the 12, and their share does not fade. (c) A
memory worth 10 points or more is **told**: it travels as news does, and moves that person's friends and kin by
half and his rivals by a quarter the other way.

**The player sees** the greeting quote a carved memory ("You sold the letter. My father told me.").

**To build.** A yearly `Fade()`; a `carved` flag; `ties` loaded from the spine and places files into the
registry; telling on the news travel in `Reputation.Daily`; greeting lines chosen by memory word.

### 4.5 The crew: marks, patience, age

**Rule.** Each man keeps his trait, want and fear, and can earn up to three **marks** from what happens, each
with one small effect and a change to his portrait. Each has **patience** of three notches. Overruled against his
nature, one is cut. At the third there is one card: he leaves, he fights, or (1 time in 4) he **vows** and comes
out steadier with a mark. A notch is mended by a step toward his want, a feast in his home port, or his share
paid on the day.

**Marks, to start.** Storm-tried (3 gales aboard: gale cards one word better). Sea-shy (a wreck survived: a notch
to sail after Michaelmas). Scarred (a wound: slower at the oar, better with raiders). Close-mouthed (a secret
kept: the house's secrets do not travel). Grey (age 50: one more water known).

**Age.** From 55, each winter 1 in 5 he comes ashore. He is not lost: he settles in his home port as a
guest-friend, a pilot's teacher, or your man abroad. A man dead at sea leaves kin: 60 d and a visit owed within a
year, or his home port's standing falls 10 and a kinsman holds a great claim on you.

**The player sees** four small heads under the big portrait, each with three pips for patience and a speech mark
when he has something to say. Keys 1 to 4 put him in the big frame.

**To build.** `CrewMember { marks, patience, joined }`; effect `overrule <crew> <why>`; the break card; the
winter step that turns a crewman into a port person with a bond; portrait overlays.

### 4.6 Rivals and feud

**Rule.** Enmity is a ladder of four public rungs: **rival** (he outbids you), **at odds** (he buys your debt,
sours a reeve), **feud** (his kin refuse you everywhere), **blood feud** (a card of violence can come at any port
of his kin). A rung up takes a deed. A rung down takes a **settlement**: an arbiter "friendly" or better with
both, payment before witnesses, and for a blood feud a marriage or a fostering to seal it.

**Numbers.** Wergild: 640 d a free man, 960 d a noble (?), in three yearly parts cut on the day stick. An
unsettled feud draws in one more kinsman every 5 years. Kin of a feuding house: feeling capped at "cool".
Thiadbod starts as rival; Ecgwulf's blood feud reaches the house through Aldger and the widow.

**The player sees** the broken knife on those ports, the rung as a word, and "what would end this" once told.

**To build.** `Feud { house, rung, cause, since }`; kin groups from the ties; three generic settlement cards;
day-stick notches for payments; the rival's moves as scheduled cards read from the AI merchant's real state.

### 4.7 The house: marriage, children, fostering, death

**Rule.** All of it happens on the winter screen, one choice a winter.

- **Marriage.** Offered through a bond (a guest-friend's daughter, a partner's sister). You give a morning-gift
  of 60 to 240 d. She brings three named kin in her port and keeps the keys while you sail and between heirs.
- **Children.** Each winter at home, 1 in 3 until she is 40. One child in 4 dies before 7 (?), told in one line.
- **Raising.** At 7 choose: **at home** (her trait), **aboard at 12** as a fifth mouth (your waters and tongues,
  one a season), **fostered** (their tongue, their bond, a trait of theirs), or **to the minster** (Latin).
- **Three traits an heir.** One born (seeded), one from the raising, one earned on his first voyage as master.
- **Age and death.** You start at about 30. From 50, each winter 3 in 100, rising 2 a year. At 55 you may hand
  over and sit by the fire: the old man gives one line of advice on cards until he dies.
- **Succession.** The heir takes the chart, the chest, all kin and great claims, bonds gone cold until renewed,
  half of all feelings (the registry's rule), **one friend you choose** and **the deepest feud, which the game
  chooses**. Small claims lapse. No child: adopt a nephew, a foster-son or a crewman of 10 years.

About 28 years a life gives nine or ten masters to 1066. The chest keeps a **roll of the house**.

**To build.** `House { master, spouse, children, roll }`; the winter choices; `Succession()` calling the registry's
generation step; seeded child traits; adoption; the fireside advice line (one string a card, optional).

### 4.8 The portrait panel: all of it at a glance

Top right, in the thick frame. At sea it is you. In a talk it is whoever you speak to. Keys 1 to 4 show the crew.

| Row | Shows |
|---|---|
| Portrait | the face, aged by decade, with marks drawn on it |
| Name line | name, role, age as a word: "Thiadburg, the widow, old" |
| Feeling | the word, and an arrow if it moved this call |
| Bonds | up to three marks |
| Tally stick | claims: top edge yours, bottom edge theirs |
| Remembers | three carved memories: "795: carried the letter" |
| Wants, fears | "not yet known" until a talk learns it |
| Crew strip | four heads, three pips each, a speech mark; always there |

No numbers on it. Closed, it is the portrait and the crew strip only, and the chart shows through the rest.
A **People** page in the chest lists everyone met, by port and by bond, with the rolls of the house and the dead.

**To build.** One UI Toolkit panel bound to a `PersonView` made from the registry; decade portrait variants and
mark overlays (art for you to draw: 5 ages, 5 marks); the chest page; the chart marks.

## 5. Order, and what is yours to decide

Build order, each with its test: (1) the portrait panel on today's data; (2) fading, carved memories, telling;
(3) claims; (4) bonds and chart marks; (5) gifts; (6) crew marks and patience; (7) the feud ladder; (8) the house.

Decisions for you:

1. Claims as notches on a stick (proposed), or a written list?
2. Child deaths in the game (1 in 4), or left out?
3. The inherited feud chosen by the game (proposed), or by you?
4. May the old master sit by the fire and advise, or does a life end clean?

## Sources

Read today: ck3.paradoxwikis.com/Hooks and /Stress; wildermyth.com/wiki/Legacy; darkestdungeon.wiki.gg/wiki/Stress.
The King of Dragon Pass, Battle Brothers and Sunless Sea wikis refused the fetch. Every period source is cited from
memory and was not re-read today; the wergild sums are the least sure. In the project: `Reputation.cs`,
`NpcRegistry.cs`, `spine.md`, `places/lundenwic.json`, `scale_to_fleets_plan.md` 3, `town_interactivity.md` 3.
