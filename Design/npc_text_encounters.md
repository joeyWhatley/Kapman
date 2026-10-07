# NPC interactivity as text-based adventure: the encounter and dialogue layer

Design document, 4 October 2026, revised the same day after review. For the Kāpman 2 rebuild (Unity 6, HDRP 17.6).
The user writes every final line; everything quoted here as speech is a placeholder showing the shape, the voice and
the stakes. Sources are listed at the end; a fact without a source or marked (to check) or (guess) is what it says it is.

References for the shape of the thing: Crusader Kings III event cards (a situation, checked options, lasting flags),
Sunless Sea (ports as story places with a slow return, qualities that gate text), King of Dragon Pass (a council that
argues before you choose, consequences years later), Pathologic (people who remember and the town that changes behind
your back). None of their text or data is used.

The rules that bind this document, from the plans: placeholder text only, the user writes (gameplay spec, 1 Oct);
"only what you know" (Plans/map_play.md); plain short wording, black see-through glass, Gentium (kapman-ui-look);
riddles hard, written, with notes and limited tries (answers of 2 Oct); hoard-fetching on real practice, not the
treasure-map trope (3 Oct); reading with the crew replaces the instant translate switch for runes and Latin (3 Oct,
pending row); crew as people with tongues, faith, traits and stats (Plans/crew.md).

What changed in the revision: cards are drawn per port call, not per hour (the old hourly rates would never have shown
the rare cards in a 30-game-hour loop); one Poisson draw instead of one roll per card; the engine catches up over time
jumps; effects go through a Core interface so the tests need no game layer; the effect grammar is formal; aggregate
variables replace the quantifiers the grammar had no syntax for; feeling is shown as a word; NPCs die and heirs inherit
halved feelings; monks speak their own tongue; a short multiplayer section; the first slice is cut to one night; and the
anachronisms in the sample lines (a barometer, an emperor before 800, the Rus in the 790s, the Hook) are gated or gone.

---

## 1. What is true

### 1.1 Who a merchant met in a harbour, 793-1066

| Person | The evidence | What they did to a merchant |
|---|---|---|
| The toll man (telonarius) | A royal toll (teloneum) and harbour fees are assumed at Dorestad, "collected by the king's representatives" [Dorestad]. At Quentovic the royal toll "at its height, amounted to 10% per exchange" [Quentovic] (Wikipedia, unreferenced there; the Carolingian tenth, the decima, is the usual guess; the game keeps it as the tunable `port.toll`). Charlemagne's capitulary of Thionville (805) separated just tolls (for a bridge, a market) from unjust ones (for passing a road or a forest) [Teloneum]. | Took a share of sales; could demand to see the cargo; refused "pilgrims" who were really traders (below). |
| The port's master or procurator | Gervold, abbot of Saint-Wandrille (abbot from 787, died 806-807), "managed customs duties in the ports and markets between Rouen and Quentovic" and was Charlemagne's envoy to Offa of Mercia [Gervold]. Under Charlemagne abbots were appointed as procurators who "directly control the taxation process and report to him" [Quentovic]. At Birka the "prefect of this town named Herigar, who was a counsellor of the king" (Vita Anskarii ch. XI) [Anskar]; Wikipedia's Ansgar page has him as "the king's steward, Hergeir" [Ansgar-wp]. | Set where you lay, took the toll's accounts, could ban you (Charlemagne forbade English ships his ports c. 789-790 [Offa]). |
| The mint | Dorestad had a mint in the upper town from the 7th century (Madelinus) and struck Carolingian coin [Dorestad]; Hedeby struck its own coins from 825 [Hedeby]. The Carolingian monetary reform, the heavier denier and the Carolingian pound, "can be dated to the years 793 and 794"; the Council of Frankfurt (794) ordered that "new silver pennies bearing the monogram of Charlemagne were to be minted throughout the realm" [Frankfurt]. That reform falls in the game's first year. (The Edict of Pîtres, 864, on mints: to check.) | Changed foreign and old silver, at a loss to you; after 794 the old deniers are refused at face. |
| The king's men | Charlemagne's missi dominici, made regular by the capitulary of 802, travelled in pairs, a churchman and a count, "complete strangers to the district", checking counts, tolls and justice [Missi]. In England the reeve (gerefa), including the portreeve of a trading place, witnessed sales and presided over courts [Reeve]. The capitulary of Thionville (805) was "a kind of weapons embargo" against the Slavs: trade with them only through named border posts, Bardowick, "Skaesla", Magdeburg, Erfurt, Hallstadt, Forchheim, Premberg, Regensburg and Lorch, and no arms or mail to be carried for sale [Thionville]. Herstal (779) on mail coats (brunias) leaving the realm: to check. | Searched for forbidden goods (arms to the Slavs and Avars; Christians sold abroad), heard cases, took oaths. |
| Merchants of other peoples | Frisians at York under Alcuin: "In 772 friction arose between the Anglo-Saxons and the Frisians" and Liudger left "for the sake of his personal safety" [Ludger]. (Altfrid's Vita Liudgeri says a Frisian merchant killed a count's son and the Frisians were driven out: book and chapter to check.) Frisian guild-brothers on the Sigtuna runestones (11th c.): to check. Ohthere, a Norwegian who lived "northmost of all Norwegians", told King Alfred of his walrus ivory, Sámi tribute in skins and feathers, and ropes of whale and seal hide, c. 890 [Ohthere]. Wulfstan sailed Hedeby to Truso in "seven days and nights", c. 880-890 [Wulfstan]. | Partnerships, news, prices, hiring, feuds. |
| Monks | The Rule of St Benedict ch. 53: guests "received with all care and hospitality, for it is in them that Christ is received", kept apart from the community, under one monk's care; ch. 66 a porter at the gate [Benedict]. Charlemagne had the Rule copied and distributed (c. 787, from the Monte Cassino copy: the date is the usual one, to check); Louis the Pious made it the standard: at the Aachen synods of August 816 and July 817, under Benedict of Aniane, "the Benedictine Rule was declared the universally valid norm for communities of monks and nuns" [Aachen]. Alcuin wrote after Lindisfarne (793): "Never before has such terror appeared in Britain. Behold the church of St Cuthbert, splattered with the blood of God's priests, robbed of its ornaments" [Alcuin]. Monks spoke the tongue of where they were; Latin was the written leaf and the liturgy. | Lodging, letters to carry, Latin read, books bought and sold, alms asked. |
| Slavers | Anskar bought "Danish and Slav boys" and redeemed some from captivity to train them (Vita Anskarii ch. XV) [Anskar]. Hedeby and Birka were markets of the northern slave trade [Slavery]. The Church forbade selling Christians to non-Christian lands (councils of Koblenz 922 and later) [Slavery]; Ine's laws (694) and the Frankish capitularies on selling countrymen or Christians abroad: wording to check. The Rus are first named in a Western source in 839 (the Annals of St Bertin: men "who called themselves Rhos" at Louis the Pious's court at Ingelheim) [Rus]; Verdun's trade in captives is a 10th-century fact [Slavery]. | Offered captives as a lot; asked you to carry them; the law and the Church watched. |
| Pilgrims | Charlemagne's letter to Offa (796) deals with English pilgrims' passage and with trade in "black stones" and cloaks [Offa]; it complains that some who travel as pilgrims are traders dodging the toll (English Historical Documents I no. 197; wording to check). | Worked a passage; were a cover for trade; a good deed to carry. |
| Raiders and their envoys | Godfred destroyed Reric in 808 and moved its merchants to Hedeby [Hedeby]; Frisia paid 100 pounds of silver in 810 (Plans/map_play_ideas.md, from the Frankish annals). | Sold loot, demanded tribute, hired out. |
| A widow with a cargo | Frideburg, a rich widow at Birka, had her daughter Catla carry her silver to Dorestad to be given to the churches and the poor: Vita Anskarii ch. XX in Robinson's 1921 translation; the project's reputation_793.json already cites it ("Rimbert, Vita Anskarii c. 20: Catla at Dorestad"); Wikipedia has "the wealthy widow Mor Frideborg" as a member of Ansgar's congregation at Birka with Hergeir [Ansgar-wp]. The Fordham excerpt stops in ch. XVI [Anskar]. | Entrusted a cargo and a mission to a merchant. |
| The host | The Kentish laws of Hlothhere and Eadric (673-686) make a host answer for a guest he keeps three nights, and have a Kentishman buy in London before witnesses or the king's wic-reeve (clauses 15-16; wording to check: the Fordham excerpt omits 15). Ine 20: "If a man from a distance or a foreigner goes through the wood off the track, and does not shout nor blow a horn, he is to be assumed to be a thief, to be either killed or redeemed" [Ine]. | Lodging with liability; news of rivals; the law of the stranger. |
| Drunk sailors | Hávamál st. 11-14: "no worse provision can a man carry than too much ale-drinking"; st. 1-4 on the guest who looks at every door and needs fire, water, a towel and a welcome [Havamal]. Codex Regius c. 1270, the poem older. | Fights, theft, a man lost or gained. |
| The crew's terms | Plans/crew.md C3 lists three kinds, marked to check: hired hands (paid at the journey's end or monthly: the monthly wage is a modern assumption (guess)), partners by share (the Norse félag, not attested before the 11th-century runestones: to check), and kin or household. For 793 the share paid out at the venture's end and the household crew are the better model. | Grievances come at the venture's end, not on a calendar. |

### 1.2 How people spoke

- A merchant's own words, c. 995, in Ælfric's Colloquy (a Latin teaching dialogue with an Old English gloss): "Ic astige min scyp mid hlæstum minum, and rowe ofer sælice dælas, and cype mine þing, and bicge þing deorwyrþe þa on þisum lande ne beoþ acennede" (I board my ship with my wares and row over the sea-ways, sell my things and buy precious things not bred in this land). His goods: "purple and silk, precious gems and gold, strange garments and spices, wine and oil, ivory and brass, copper and tin, sulphur and glass." He will not sell at what he paid: "what then would my toil profit me?" He sometimes suffers shipwreck "with loss of all my things, barely escaping alive" [Aelfric]. This is the register of the game's merchant: concrete nouns, the sea as work, profit said plainly.
- A hall's manners, in Hávamál: look at the doors before you go in; a guest needs fire, water, a towel and a welcome; the wise guest listens; "friends should gladden each other with weapons and garments"; "a gift always looks for a return" [Havamal].
- A churchman's register: Alcuin's letters (above) and the Rule: guests are Christ; the porter answers the knock with "Thanks be to God" (ch. 66, wording to check) [Benedict]. The formula is Latin; the talk after it is the porter's own tongue.
- Hedeby c. 965 through a foreign eye: Ibrahim ibn Yaqub (al-Tartushi) noted that "the right to divorce belongs to the women" and that both sexes wore eye make-up [Hedeby]. The sacrifice-poles, the fish diet and the singing "like dogs" are in the same excerpt (to check in the al-Bakri text).
- What a Norse host wanted to hear, and what cost you: Finnsburh sung to Frisians or the Ingeld feud to Ingeld's side (Plans/map_play_ideas.md). A singer was paid in rings (Widsith; Egil's Head-Ransom at York for Eric Bloodaxe [Egils]).
- Faith at the door: Egil and Thorolf were prime-signed in England so they could deal with Christians and heathens alike (Egils saga; chapter to check) [Egils].
- Words a steersman did not have: the barometer is Torricelli's, 1643 [Barometer]; the magnetic needle is first described for Europe by Alexander Neckam c. 1190 [Neckam]; so no "glass", no "compass", no "o'clock", no "mile" (the Roman mile was a learned word, not a sailor's). Weather signs a steersman had: a swell from a quarter with no wind in it, gulls gone inland, a halo, the colour of the dawn. These go in the VoiceTests word list (4.4).

### 1.3 Names (for the name forge)

Germanic names were made of two parts (dithematic); the parts need not mean anything together; families repeated parts and alliterated [Names].

| People | First parts (attested) | Second parts (attested) | Whole names attested in 793-1066 sources |
|---|---|---|---|
| English | Æthel-, Ead-, Ælf-, Wulf-, Os-, Cyne-, Beorht-, Leof-, Sige-, Wig- | -red, -wine, -stan, -wulf, -beorht, -helm, -gar, -frith, -mund; women -gifu, -thryth, -burh, -flæd, -wynn [Names] | Alcuin, Higbald, Æthelred, Æthelstan, Wulfstan |
| Frisian | Rad-, Ald-, Thiad-, Liaf-, Liud-, Bern-, Wur-, Hilde- | -bod, -gisl, -grim, -burg, -ger, -lef, -sing | Radbod, Aldgisl, Thiadgrim, Liafburg, Liudger, Bernlef [Ludger]; Wursing (to check); Poppo, the Frisian king killed by Charles Martel at the Boarn in 734 [Boarn] (Bubo in the Frankish annals, the same man under two spellings: to check) |
| Frankish | Hlud-/Lud- (Hludowic; the Merovingian Chlod- spelling is gone by 793), Hild-, Ger-, Angil-, Ein-, Ans- | -bert, -old, -hard, -win, -gis | Gervold [Gervold], Einhard, Angilbert, Ansegisus [Gervold] |
| Norse | Þor-, Ás-, Ragn-, Sig-, Hall-, Gunn-, Her- | -steinn, -björn, -ulfr, -mundr, -leifr, -gar; women -ríðr, -hildr, -dís, -gerðr; patronymics -son, -dóttir; bynames (the Red, Bloodaxe) (general knowledge, to check against a name list) | Herigar/Hergeir, Björn, Olof [Birka]; Godfred [Hedeby]; Ohthere, Wulfstan (English spellings) |
| Gaelic | Mael-, Donn-, Flann, Cellach (guess) | | to check |
| Slavic (Wends, Obodrites) | Drasco, Slavomir (Frankish annals, to check) | | |

### 1.4 Reading and tongues

- Few merchants read Latin; most Norse traders could read runes (Plans/map_play.md). The Younger Futhark's 16 runes make one rune stand for several sounds (the first build's RuneReading.cs implements this).
- Old Frisian, Old English and Old Saxon were close kin; Old Norse further; Latin, Irish, Slavic and Baltic unrelated to all of them. The exact degree of mutual understanding is not recorded; the table in 2.6 is a game model (guess). For Norse and English there is a scholarly case for substantial mutual intelligibility: Townend, Language and History in Viking Age England (Brepols, 2002), on "the linguistic relations between and mutual intelligibility of speakers of Old Norse and Old English" [Townend]. The old 0.25 was the low end; the data defaults to 0.35 and rises to 0.5 in the Danelaw years (2.6).
- Riddles with agreed answers: Exeter Book 7 (swan), 26 (gospel book), 35 (mail-coat), 47 (bookworm), 60 (reed pen); c. 94 riddles, manuscript c. 970s; Tupper's 1910 edition is public domain [Exeter]. Gestumblindi's riddles (Hervarar saga, 13th c.) in Vigfusson and Powell 1883 are already in Data/texts_793.json.

### 1.5 Silver in the ground

Hoards were buried for safety by the thousand; the famous ones were never recovered by their owners (Plans/map_play.md, answers of 3 Oct). Egil hid his silver with two slaves' help near Mosfell [Egils]. So the period practice the game models is: a person buries their savings when raiders come; they or their heir want it back; a merchant with a ship and men is the way to fetch it.

---

## 2. The model

### 2.1 One engine, three stages

Every piece of NPC text in the game is an **encounter**: a data card the engine shows on one panel. Three stages feed it:

| Stage | Where it opens | Trigger | Who |
|---|---|---|---|
| Harbour | the port menu ("Talk to people", the errands) and on arrival | arrival in a level with a port (the arrival set, then one visit draw); the daily draw for stays past a day; an errand chosen | toll man, harbour master, mint, merchants, monks, king's men, slavers, pilgrims, raiders, drunk sailors, widow, host |
| Sea | an alert in 3D or on the chart, the game slowed to 1x (2 Oct answer) | a ship within hail; a sighting (wreck, boat, bank); the hourly draw | masters, survivors, passengers, raiders, king's ships, fishermen |
| Crew | the ship mode (isometric), the Crew tab; before a crossing | the hourly draw; a crossing planned; a feast or fast; a text or riddle opened | the council seats, any man aboard |

Reading sessions, riddles and the silver fetch are **long encounters**: a card that stays open in the chest or crew tab across days, with its own steps.

**Arrival sets are short.** At most two cards run on arrival, in order: the toll man where there is a toll, and the berth card only when the berth choice matters (a fee to pay, or a known man to name). After the first visit to a port the toll man opens with a one-key default, "As last time" (declare), and one memory line ("You hid nothing last time. Good."); the other options stay for those who want them. Everything else at a port is a visit draw (2.3) or a choice from "Talk to people". The sea is the game; a port call must not be a toll of cards.

### 2.2 The card

```
Encounter
  id, stage (harbour|sea|crew|long), title_key, intention_key
  arrival: true | false         when: conditions (2.4)
  weight: visit weight (harbour) | rate per game hour (sea, crew) (2.3)
  once | cooldown_days          months: [..] (optional)
  actors: [role -> NpcSpec]     (2.5: a named NPC is forged or recalled for each role)
  lines:  [ {who, key, tongue?, when?} ]   (the speech slots; text from writing.json; tongue defaults to the speaker's)
  options: [ {key, label_key, check (2.4), cost, effects (2.7), then: encounter id | null, default: true|false} ]
  memory: what the NPCs remember of each option (2.5)
```

Options are shown always; a failed check greys the option and shows its rule in the tooltip ("He must be easy with you"; "He will not deal with a man shunned here"), never a percentage. Chance exists only where the period had chance (a fever, a search finding the hidden swords), and then the panel says **sure, likely or doubtful**, the three words the Known store already uses (knowing_793.json `words`: sure 0.8, likely 0.55, doubtful 0.3), mapped at the same thresholds. `then` is a field of the option, never an effect. `default` marks the one-key option (Enter) a returning player gets.

### 2.3 Scheduling

The play loop is in game hours, not days: 1 real second is 1 game minute, a port call of 10 real minutes at 1x is 10 game hours, and a Dorestad-London run is about 30 game hours. So harbour cards are drawn **per port call**, and only sea, crew and long-stay cards are drawn per hour.

**The visit draw.** On arrival, after the arrival set, the harbour pool is rolled once. Every card whose conditions hold is a candidate with weight

    w_i = weight_i × m_standing × m_season × m_port

and the draw is one Poisson process over the pool:

    λ = Σ w_i          p(a card this call) = 1 - exp(-λ)          the card is picked with probability w_i / λ

At most one drawn card is shown per call, after the arrival set. Because the process is one, each card's realised rate is exactly its own weight (times its multipliers), not the pool's share of a single roll, so the rates in the table below hold per card and the scheduler test can check them exactly.

- `m_port`: the port's traffic this month from traffic_793.json (`harbours.ports.<id>.present[month]`, hulls at the landing) divided by 40, clamped 0.4..1.5: Dorestad in July (91.5) gives 1.5, a middling market about 1, a village 0.4. Bigger ports have more people about.
- `m_standing`: cards marked `wants_good_name` scale by (1 + standing/200); `wants_bad_name` by (1 - standing/200), standing from the game's Reputation.At(port) in -100..100 (read through the effects interface, 4.1).
- `m_season`: a card can name months; outside them 0. A chronicle tag (`after_raid`, `famine`, `succession`, 2.8) is a condition, not a multiplier.

**The hourly draw.** The engine ticks once a game hour for the sea and crew pools, and for the harbour pool only once the ship has lain in port more than 24 hours (the long-stay rate: the visit weights divided by 72, so a week moored is worth about two more calls). Per hour the same one-process rule applies with `w_i = rate_i × m_stage × m_standing × m_season`; `m_stage` is sea 1 under way, 0.3 at anchor; crew 1 always; harbour 1 while moored past the first day, else 0.

**Determinism and time jumps.** The draw for hour h is seeded by (world seed, day, hour) with SimRandom, so a save reloaded gives the same draw. A drawn card waits (`pending`) until no panel is open, then opens; at most one card is pending, and a draw made while one is pending is dropped. The runner keeps `lastHour` and, each frame, runs one tick for every whole hour between `lastHour` and floor(GameHours), in order, in the same frame. LevelClock lets the player jump time (T +1 h, Y +24 h, U +30 days: LevelClock.cs lines 149-151) and Hoards.Dig skips six hours; a 30-day jump is 720 ticks, under 0.1 ms each, so about 50 ms once, and it yields exactly the cards that 720 single ticks would (the world variables are frozen at the jump's start, an accepted approximation). Backward jumps (the T/Y/U "back" modifier) do not rerun: `lastHour` follows the clock down and nothing fires until it climbs past the hour already drawn.

| Card family | How drawn | Weight / rate | Expected |
|---|---|---|---|
| harbour, arrival (toll man, berth) | arrival set | not drawn | every call where the conditions hold; at most two |
| harbour, common (merchant, drunk sailor, host, pilgrims, mint after the reform) | per call | 0.6 total over the family at `m_port` 1 (about 0.15 each) | about one call in two shows one of them; Dorestad in summer two calls in three |
| harbour, rare (widow, king's men, slaver, raider's envoy) | per call | 0.12 total (about 0.03 each; the king's men ×3 with `flag.hid_goods`) | one call in nine; one call in six at Dorestad in summer |
| harbour, long stay | per hour after the first 24 h moored | visit weights / 72 | a week moored adds about two cards |
| sea, hail | by the lane's traffic (traffic_793.json): ships within 3 km that CanOffer | as the sim gives | several per crossing on the Rhine mouth lanes, none in the open sea |
| sea, found (wreck, boat, bank) | per hour under way | 0.01, ×4 within a day of a recorded storm | one crossing in four (30 h: p = 0.26); after a storm, likelier than not |
| crew, grievance | per hour | 0.01 × (1 - heart) × days_at_sea / 10 | rises with a long, sour voyage |
| crew, faith and feast | on the day (Feasts.On) | 1 | on the day |

### 2.4 Conditions and checks (the grammar)

Conditions are a small expression language over named variables, parsed once at load and evaluated in a few microseconds. No scripting engine, no reflection. An unknown variable is an error at load, not at play, so every variable a card uses must be in this list (or in the data-driven families marked with `<..>`).

```
expr   := term (("and" | "or") term)*
term   := ["not"] (comparison | flag | var | "(" expr ")")       (a bare var is true when non-zero)
comparison := var op number | var op word
op     := < <= = != >= >
var    := silver | debt | debt_due_days | standing | standing_region | standing_word
        | crew | crew_min | crew_full | health | heart | days_at_sea | month | year | hour
        | wind | fog | storm | ice | feast | fast | moored | under_way | level_kind
        | port.kind | port.people | port.faith | port.realm | port.has_mint | port.toll | port.visits
        | port.best_feeling                      (the highest feeling of any known NPC at this port)
        | hold.<good> | hold.weapons | hold.captives | hold.value
        | hold.from_<people>                     (units aboard bought in that people's ports)
        | tongue.<name> (0, 0.5, 1 aboard) | crew.<stat>_best | crew.<trait>_count | crew.kind (hired|partners|kin)
        | days_since_paid | venture_ended
        | npc.<role>.feeling | npc.<role>.met | npc.<role>.owed | npc.<role>.generation
        | text.<id> (owned) | riddle.<id> (solved|resting|open) | learned.latin | learned.runes
        | chest.latin_count | chest.riddles_solved | chest.rune_count
        | reform.coin (0 before the 793/4 reform reaches this port, 1 after)
flag   := flag.<name>          (set by effects, saved with the game; may carry an expiry day)
word   := a standing word (reputation_793.json) | a feeling word (2.5) | sure | likely | doubtful | a people, faith or realm id
```

Words compare by their threshold: `npc.tollman.feeling >= easy` means feeling ≥ 0, `standing_word >= known` means standing ≥ 30 (reputation_793.json `words`). Tooltips are built from the words, so the player reads "He must be easy with you", never a number.

Examples: `moored and port.realm = frankish and hold.weapons > 0`; `under_way and fog > 0.6 and crew.eyes_best < 3`; `npc.tollman.feeling < cool and silver >= 40`; `port.best_feeling >= friendly`; `chest.latin_count > 0`; `crew.herb_wise_count > 0`.

An option's `check` uses the same grammar. A check with chance is an object in the data (2.7): `{"chance": 0.7, "by": "crew.seamanship_best", "success": [..], "failure": [..]}`, resolved as `p = chance + 0.05 × (stat - 3)`, clamped 0.05..0.95; the panel shows sure (p ≥ 0.8), likely (≥ 0.55) or doubtful, not the number.

### 2.5 Procedural NPCs with memory

```
Npc
  id, name (forged: 2 parts + byname), people, tongue(s), faith, role, home_port, born_year, dies_year
  traits: 2 of the crew spec's 12 (Plans/crew.md), reused for NPCs
  alive, last_seen_day, last_seen_port
Relationship (keyed by (npc id, merchant id); 2.11)
  feeling: -100..100 toward this merchant
  memory: up to 12 entries {day, card id, option key, delta, generation}, oldest dropped
  owed: silver the merchant owes them or they owe (a tally)
```

- **Forging.** A role spec says what kind of person fills it (`tollman: people = port.people, faith = port.faith, role = tollman, age 30..60, home = this port`). The registry first looks for a living NPC of that role at that port (the same toll man meets you each time); otherwise it forges one with SimRandom seeded by (port id, role, year/10), so the Dorestad toll man of the 790s is the same man in every game of the same seed. Named historical people (Herigar at Birka 829-c. 850, Gervold over the ports of Neustria 787-806) are rows in the data with fixed years and override the forge.
- **They die.** `dies_year = born_year + lifespan`, lifespan drawn by SimRandom seeded by the NPC id from 45..75 (guess: adult life expectancy once past childhood); in a chronicle plague, famine or raid year at the NPC's home the draw is made again against that year and takes the earlier of the two. A fixed role whose holder is dead re-forges (a new toll man, a new generation of the family where the role is hereditary: `heir_of` in the data, the name alliterating with the father's). A dead NPC's relationship is kept for the heir's inheritance below, then dropped.
- **Feeling is a word.** The speaker line shows it (3.1) from `npc_roles_793.json` `feeling_words`: shunned (< -50), wary (-50..-20), cool (-20..0), easy (0..20), friendly (20..50), fast friend (≥ 50). Checks in the data use the words (2.4).
- **Memory decays.** `feeling` moves toward 0 by half every 2 game years (decay per game day = 0.5^(1/730)). A memory entry older than 6 years is dropped. News (Reputation's news pipeline) carries a deed to other NPCs of the same people: on hearing, their feeling moves by 0.25 × the deed's points.
- **Heirs.** On the merchant's succession every feeling toward him is halved and every memory entry keeps its `generation` tag, so a card can say "your father cheated my father" (`npc.<role>.generation < generation`). An NPC's own heir inherits the same way: half the feeling, the memories tagged. Nothing is forgotten outright; it fades twice as fast across a death.
- **They turn up again.** A master hailed at sea is a SimShip with an Npc attached; a man paid off in port goes to that port's hiring pool; a widow who gave you a cargo waits at her port with a `scheduled` card ("Did you bring word from Dorestad?") that fires when you next moor there.
- **Cap.** 2,000 live NPCs; the oldest unseen (last_seen_day) are dropped first. An NPC with a name, two tongues, two traits, 12 memory entries and an owed tally is 600-1,000 bytes in the project's plain-text JSON (Json.Write), so the registry is 1-2 MB in the save; memory entries store card and option ids as short ints from the loader's table to keep it near the low end.

### 2.6 Tongues and the comprehension model

Each line in a card carries its tongue, and **a line's tongue is the speaker's native tongue**: the role spec gives it (a monk at Utrecht is Old Frisian or Old Frankish; a Northumbrian monk is Old English; the guest-master at a Frisian monastery is understood by a Frisian merchant in full). Latin applies only to `text.*` items (leaves, books, charters) and to formula lines marked `tongue: latin` (the porter's "Deo gratias"), which show in Latin with `C` from `learned.latin` or a reader aboard. Comprehension `C` of a tongue is the best aboard:

    C = max(merchant[t], max over crew of crew[t], kin[t', t] × best[t'])

| From \ To | Old Frisian | Old English | Old Saxon | Old Norse | Latin | Irish | Slavic | Baltic |
|---|---|---|---|---|---|---|---|---|
| Old Frisian | 1 | 0.5 | 0.5 | 0.35 | 0 | 0 | 0 | 0 |
| Old English | 0.5 | 1 | 0.5 | 0.35 (0.5 in the Danelaw years, 878-1066, by `tongues_793.json`'s year table) | 0 | 0 | 0 | 0 |
| Old Saxon | 0.5 | 0.5 | 1 | 0.35 | 0 | 0 | 0 | 0 |
| Old Norse | 0.35 | 0.35 (0.5 Danelaw) | 0.35 | 1 | 0 | 0 | 0 | 0 |
| Latin | 0 | 0 | 0 | 0 | 1 | 0 | 0 | 0 |

(Game model, data in `tongues_793.json`; the real degree is unrecorded, the Norse-English figure follows Townend's case for mutual intelligibility [Townend]. The user can retune the table.) Frankish is Old Frankish/early Old Dutch in the Rhine delta and Romance further south: two rows to add (guess: Rhineland Frankish 0.5 to Frisian; Romance 0).

What the panel does with `C`:
- `C = 1`: the full English line (the user's text).
- `0.5 ≤ C < 1`: nouns and numbers in English, the rest shown as the original-tongue words from a small glossary per card (the user writes both; if no original is written, the words are replaced by "…").
- `C < 0.5`: gesture only: the intention line ("He points at your hold and holds up three fingers") and the options stay, the speech is hidden.
- Latin text: `C = 1` only when `learned.latin` (from a monk passenger over 40 game days, or a monastery winter) or a reader is aboard. Runes: `learned.runes` or a crewman who knows them (the 3 Oct reading-with-crew rule, replacing the instant switch). The translate switch on a text in the chest is the same test. **Chest.ReaderAboard is a stub returning false in the ported code (Chest.cs line 143); it must be implemented over the crew list (any crewman with `reads_latin` or `reads_runes`) before this works.**

A crewman who speaks the port's tongue lifts `C`, gets a better price (Plans/crew.md), and brings an extra line of news. That is the whole reason to hire a Dane for the Hedeby run.

### 2.7 Effects (the verbs)

The first build's effect vocabulary is kept (health, morale, men, silver, goods, water, spoil_food, deed/standing, chance) and extended. **The grammar is formal: one effect per string, `verb arg*`, args separated by spaces; a good id or a text key is one token; chance is an object, not a string.** The loader rejects any verb not in this table.

| Effect | Does | Persists in |
|---|---|---|
| `silver ±n`, `goods <id> ±n`, `health ±f`, `morale ±f`, `men ±n` | as the first build's GameEvents.Apply | the merchant |
| `toll sales` / `toll none` / `toll eased <days>` | this call's toll is taken on sales at `port.toll`, waived (a charter), or halved for `days` | the port's toll state |
| `deed <kind>` | Reputation.Deed at this place; news travels | standing, news |
| `npc <role> feeling ±n [memory word]` | moves the NPC's feeling; writes a memory entry | NPC registry |
| `flag <name> [= value] [for <days>]` | sets a saved flag, with an expiry day when `for` is given | flags |
| `schedule <card id> in <days> [at <port>]` | a follow-up card fires then | the scheduler |
| `news <text key> at <port>` | posts a news item on the port's board; it spreads by ship | the world's boards |
| `price_shock <port> <good> ×<f> for <days>` | moves a market's price (an event's effect on the economy, e.g. a raid rumour) | the market |
| `commission <good> <n> by <days> premium <f>` | posts an order (Commissions) | orders |
| `give_text <id>`, `give_riddle <id>`, `open_fetch <id>` | puts a text or riddle in the chest; opens a silver-fetch card | the chest |
| `hire <npc>`, `pay_off <npc>`, `passenger <npc> to <port>` | crew and passengers | the crew |
| `encounter <kind>` | hands the sea sim an order: `close_and_hail`, `heave_to`, `run` (the first build's Encounter) | the sim |
| `law <kind> [in <days>]` | a case before the reeve or the count's man: fine, seizure or hearing in n days | the law state |
| `learn latin|runes +f` | progress toward reading | the merchant |
| `tongue <name> = 0.5` | a crewman teaches: progress | the merchant |
| `{"chance": f, "by": "<var>", "success": [effects], "failure": [effects]}` | one roll, resolved as in 2.4; the panel shows the word | whatever the branch does |

### 2.8 Season, year, weather and events

- The chronicle (chronicle_793.json) feeds the draw: a raid on a port within 60 days of the player's visit adds the cards tagged `after_raid` (loot sold cheap, survivors, the widow, the priest hiding plate, the fetch); a famine year adds `famine` cards (grain asked at the gate, alms); a king's death adds `succession` (the toll changes hands, charters void); the 793/4 coin reform sets `reform.coin` port by port as the new deniers reach them (Frankish mints first, then the ports they trade with).
- Season: feast and fast days from Feasts.On gate the crew's faith cards and the monks' hospitality (Lent: no meat at the guest house). Winter closes the sailing season (traffic_793.json): the harbour cards shift to lodging, debts and reading.
- Weather: `fog`, `storm`, `wind`, `ice` from LevelWeather gate sea cards (fog: a hail you can hear but not see; storm: survivors after; ice: a frozen harbour and the men idle).
- Year: realm and faith tables by year (Plans/map_play.md promised them) choose the toll man's realm, the king's men's law, and the port's faith; prime-signing cards exist only where Christians and heathens both trade (Hedeby after 850, Birka). Lines carry their own `when` for year gates: "the king's writ" before 801 and "the emperor's writ" after (Charlemagne's crowning, 25 Dec 800, is in the chronicle), "the count's man" in Frankish realms and "the reeve" in English ones, "the Rus" only after 839.

### 2.9 Riddles through NPCs

An NPC gives a riddle (give_riddle) as a reward, a sale or a challenge (riddles_793.json already holds the palette, the acquire rules, wakeOn and rewards). The encounter layer adds the **setter**: an NPC with memory. Returning the right answer to the setter is an option on his card (check: `riddle.<id> = solved`), worth `won_riddle` standing and his feeling +10; a wrong answer at the setter costs feeling -5 and one try; when tries run out the riddle rests until wakeOn (seen, heard, read, visited), as the 2 Oct answers say. A crewman with the right tongue and the `talkative` trait can do one part of a chained riddle for you (the 3 Oct rule): the panel shows his part filled in, the rest blank.

### 2.10 Fetching buried silver

The long card `fetch`:

1. **The owner.** An NPC with `buried_silver` (set when the chronicle raids their port while they are a known NPC, or forged with it in an after_raid card): a trader who fled, a priest with the church plate, a widow. Amount: personal savings, 50-600 g silver (guess: a few hundred deniers' worth), never a real hoard.
2. **The directions.** Three to five measures in the hoards data's format (from landmark, toward landmark, so many paces; dig a forearm deep), by remembered landmarks that stood then. The spot is generated among real landmarks near their port, never on a real find's place.
3. **The terms.** A share (a third is the default; haggle with a check on `crew.trade_best` and feeling), or an advance and a smaller share.
4. **The walk.** Anchor at the nearest water; a shore party of n men walks at 4 km/h on the chart, digs 6 game hours (Hoards.DigHours = 6), returns. Risks: the king's men (a stranger off the track without a horn is a thief: Ine 20), the owner's enemies, a wrong spot (within 40 m counts: Hoards.Within).
5. **The reckoning.** Bring it back: share paid, feeling +30, standing `kept_commission`; keep it all: `flag cheated_<npc>`; if the owner later hears (news), `broke_commission` and a feud card; dig church ground: `dug_church_ground`.

### 2.11 Multiplayer (128 players, peer to peer, later; Plans/multiplayer.md)

- NPC forging is global and seed-derived: every client forges the same Dorestad toll man from (port, role, decade) and the world seed, and his birth and death are the same for all. Named historical people are the same rows.
- Relationship state (feeling, memory, owed, flags, scheduled cards, cooldowns) is per merchant: the registry keys it by (npc id, merchant id), and a save holds only its own merchant's rows. Single player is the case with one merchant id.
- Cards are per client and never pause the host's clock (no pause, no 4x online). A card open on one client is a card open for that player only; a `pending` card unanswered at the end of the game hour it was drawn in expires online (in single player it waits).
- Effects that touch shared state (a price shock, news on a board, a commission, a seizure) are sent to the host, which validates them against the card's data (the option existed, its check held on the host's copy of the variables, its cost was paid) before applying; the rest apply locally.

---

## 3. In the game

### 3.1 What the player sees

- **In 3D (voyage):** nothing opens on its own. An alert chip at the top bar ("A ship hails, 1 km to the north") and the game drops to 1x. Click it or press Enter and the card opens over the 3D, the scene dimmed by the glass (and softly blurred if the user wants it: 4.3). Cards at sea are short: four or five lines, three options.
- **On the chart:** the card docks in the left drawer under the Crew or News tab; the ship keeps moving.
- **In port:** the port menu's "Talk to people" opens a short list of who is about today (the names the registry has at this port, each a card, each with its feeling word), the way Sunless Sea's ports list their people. The toll man and the errands keep their fixed places (kapman-ui-look).
- **The speaker line** carries the person's name, role and the feeling word: "Hrodgar, the count's toll man · cool". The word is the state every check reads, so a greyed option's tooltip ("He must be easy with you") can be read against it.
- **In the ship mode:** the council. Before a crossing, the seats speak in turn (steersman, lookout, mate, trader, skald or priest) and the player chooses; the arguments come from their traits and stats.

### 3.2 What the player hears

No voice acting and no invented audio (sound rule: CC0/PD recordings only). A card opening plays one short CC0 clip by stage from G:\kapman_unity\data\sounds_cc0 (a harbour: gulls and water against piles; the crew: the hull's creak; a hail: a distant shout, if a CC0 one exists, otherwise nothing). The soundscape keeps running under the card.

### 3.3 What the player does

Reads, weighs the checks shown in tooltips against the feeling word on the speaker line, chooses with keys 1-9 (Enter for the default on a card seen before), and lives with it. The fun is the CK3 fun: a small situation that you can read the rules of, a choice that costs something you can see (silver, a man, standing, a day), and a world that remembers. The replayability is: the same seed gives the same toll man, but every merchant treats him differently; the chronicle's raids shuffle which widows, survivors and loot-sellers exist in which years; a crew of other tongues opens lines you could not read; riddles rest and wake on what you happen to see; the man you cheated dies and his son has half the grudge and the whole story.

### 3.4 What is not here

No romance, no karma meter, no "good/evil" labels. Standing by people, feeling by person, law by realm, as built. No dialogue trees deeper than three steps inside one card; depth comes from cards that schedule other cards months later.

---

## 4. Build plan for Unity

### 4.0 The dependency on the game layer

Everything that touches the live game (Reputation.Deed, Trade.Purse, Commissions, Chest, Known, Encounter.CanOffer, TextPanel's `[write:]`) lives in `G:\kapman_unity\Kapman2\Assets\Kapman\Code\Game`, the port of the first build made on 4 Oct. On 4 Oct it is **untracked** (`git status`: `?? Assets/Kapman/Code/Game/`), has its own asmdef (`Kapman.Game`, referencing Kapman.Core, Kapman.Runtime, the Input System and HDRP), and has not yet been compiled by Unity or committed; it carries shims (LevelFrame, SailingShip, LocalWeather) and the first build's KapmanGame, SaveGame, Patrols/Deeds. The test asmdef (`Kapman.Tests.EditMode`) references only `Kapman.Core`. So:

- the pure parts of this layer (everything in 4.1 marked Core) depend on nothing in Game and are tested through a fake;
- `Effects` (Runtime/Encounters) is the only class that references `Kapman.Game`, and it is built after the Game port compiles and is committed;
- `Kapman.Game` is added to the test asmdef only when that assembly compiles; until then no test touches it.

The Gentium fonts, TextPanel.uss and the PanelSettings asset are **not in Kapman2** (nothing under Assets matches gentium, .uss, .uxml or PanelSettings); they are in `G:\kapman_unity\Kapman\Assets\Kapman\Game\UI` (Fonts/ with GentiumBookPlus-Regular and -Bold and the OFL file, TextPanel.uss, GamePanel.asset). Copying them is a step of the first slice. There is no depth-of-field volume in Kapman2 either (HdrpVolume.cs has only Find and Get).

### 4.1 Code (new namespace `Kapman.Encounters`; pure-C# parts in Kapman.Core so the tests run headless)

| Class | Where | Does |
|---|---|---|
| `EncounterDef`, `OptionDef`, `LineDef`, `RoleSpec`, `ChanceDef` | Core/Encounters | the card as data; loaded by `EncounterLoader` from JSON with the existing `Json` parser (Core/Data/Json.cs); the loader interns card and option ids to short ints |
| `Cond` (parser + evaluator) | Core/Encounters | the grammar of 2.4 over an `IVars` interface (name → double or string); words resolved to thresholds at load from the word tables; no Unity types |
| `IGameEffects` | Core/Encounters | the seam to the live game: `Silver(n)`, `Goods(id, n)`, `Deed(kind, place)`, `Standing(place)`, `Flag(name, value, days)`, `Schedule(card, days, port)`, `News(key, port)`, `Toll(mode, days)`, `PriceShock(..)`, `Commission(..)`, `GiveText/GiveRiddle/OpenFetch`, `Hire/PayOff/Passenger`, `Law(kind, days)`, `Learn(what, f)`, `Encounter(kind)` |
| `Effects` (the parser and applier) | Core/Encounters | parses the strings of 2.7 at load into verb records; `Apply(option, IGameEffects, IRng)` runs them, chance objects included |
| `FakeGame : IGameEffects` | Tests/EditMode | records every call in lists (`Deeds`, `Silver`, `Flags`, ..) for the tests |
| `GameEffects : IGameEffects` | Runtime/Encounters (needs Kapman.Game) | the real one over Trade.Purse, the hold, the crew, Reputation, Commissions, the news boards and the chest; the first build's GameEvents.Apply is the model |
| `Scheduler` | Core/Encounters | the visit draw and the hourly draw of 2.3, one Poisson process each, SimRandom-seeded; `Tick(hour, vars)`, `Visit(port, month, vars)`; pending (one); cooldowns; scheduled follow-ups; flag expiry; save/load |
| `NpcRegistry`, `Npc`, `Relationship`, `NameForge` | Core/Encounters | 2.5 and 1.3; relationships keyed by (npc id, merchant id); `NameForge.Make(people, sex, seed)` from names_793.json (night two: a 20-row placeholder list first) |
| `Tongues` | Core/Encounters | the kin table by year and `Comprehension(tongue)`; the line-masking of 2.6 |
| `EncounterRunner` (MonoBehaviour) | Runtime/Encounters | keeps `lastHour`; each frame runs every whole hour from `lastHour` to floor(LevelClock.Current.GameHours) through `Scheduler.Tick` (2.3: a 30-day jump is 720 ticks in one frame); builds the `IVars` from LevelInfo, LevelWeather.Current (Wind, FogDensity, Storm, Ice), ShipMotion.Player, Feasts.On, the merchant, the crew; opens the panel; raises `Due(EncounterDef)` and `Chosen(option)` C# events for visual scripting |
| `MooringTrigger` | Runtime/Encounters | a LevelExit-style radius trigger at each dam head or landing: moored when `ShipMotion.Player.Speed < 0.05` and `Goal == null` inside the radius for 2 s; fires `Moored(port, berth)` once per call and `Cast off` when she leaves it. ShipMotion exposes only `Goal`, `Speed` and `Player` (ShipMotion.cs lines 30-33); it has no mooring event, so this is the hook |
| `EncounterPanel` (UIDocument) | Runtime/Encounters/UI | the card on screen (4.3); keys 1-9, Enter for the default, Esc; tooltips with the rule and the feeling word |
| `EncounterTriggers` | Runtime/Encounters | hooks: `MooringTrigger.Moored` → arrival set then the visit draw; `LevelExit.Go` → "leaving" cards (a passenger asks to land first); a `SimShip` within hail → sea card (the first build's `Encounter.CanOffer` logic) |
| `EncounterSave` | Runtime/Encounters (needs Kapman.Game's SaveGame) | the registry, relationships, flags, scheduler and cooldowns in the save (save version +1); the Core classes expose `Write(JsonWriter)`/`Read` so a test can round-trip without SaveGame |

### 4.2 Data files (Assets/Kapman/Game/Data)

| File | Holds |
|---|---|
| `encounters/harbour_793.json`, `encounters/sea_793.json`, `encounters/crew_793.json`, `encounters/long_793.json` | the cards (format below) |
| `npc_roles_793.json` | role specs: people/faith/age/traits/tongue by port kind and realm; `heir_of` roles; `feeling_words`; named historical people with years |
| `names_793.json` | name parts by people and sex, bynames, the alliteration rule (night two; a `names_placeholder.json` of 20 rows first) |
| `tongues_793.json` | the kin table with year ranges; which tongue each people and port speaks by year |
| `writing.json` (exists) | every `*_key`: the user's text; a missing key shows `[write: key]` as the first build's TextPanel does |
| `knowing_793.json`, `reputation_793.json`, `riddles_793.json`, `texts_793.json`, `hoards_793.json`, `traffic_793.json` (exist) | read, not changed (`m_port` reads traffic's `harbours.ports.<id>.present`) |

**The card format (JSON; YAML would need a parser the project does not have).** Every effect is `verb arg*`; chance is an object; `then` and `default` are option fields; a line may carry a `when`.

```json
{
  "id": "harbour.tollman.frankish",
  "stage": "harbour",
  "arrival": true,
  "when": "moored and port.realm = frankish and port.toll > 0",
  "weight": 0, "cooldown_days": 0,
  "actors": { "tollman": { "role": "tollman", "people": "port", "faith": "port", "tongue": "port", "age": [30, 60], "fixed": true } },
  "title_key": "tollman.title",
  "intention_key": "tollman.intention",
  "lines": [
    { "who": "tollman", "key": "tollman.greet", "when": "port.visits = 0" },
    { "who": "tollman", "key": "tollman.greet_again", "when": "port.visits > 0" },
    { "who": "tollman", "key": "tollman.law_mail", "when": "year < 805" },
    { "who": "tollman", "key": "tollman.law_swords", "when": "year >= 805" },
    { "who": "tollman", "key": "tollman.ask_cargo", "when": "hold.value > 0" }
  ],
  "options": [
    { "key": "declare", "label_key": "tollman.declare", "check": "true", "default": true,
      "effects": [ "toll sales", "npc tollman feeling +2 honest" ] },
    { "key": "hide", "label_key": "tollman.hide", "check": "hold.weapons > 0 or hold.captives > 0",
      "effects": [ { "chance": 0.75, "by": "crew.trade_best",
                     "success": [ "flag hid_goods_at_port for 60" ],
                     "failure": [ "law seizure", "deed goods_seized", "npc tollman feeling -25 caught" ] } ] },
    { "key": "gift", "label_key": "tollman.gift", "check": "silver >= 12 and npc.tollman.feeling >= wary",
      "cost": "silver 12",
      "effects": [ "deed bribed_officer", "npc tollman feeling +8 gift", "toll eased 30" ] },
    { "key": "charter", "label_key": "tollman.charter", "check": "text.charter_toll_dorestad",
      "effects": [ "toll none", "npc tollman feeling -3 charter" ] }
  ],
  "memory": { "hide.failure": "caught hiding goods", "gift": "gave a gift" }
}
```

A sea card with a tongue and a follow-up:

```json
{
  "id": "sea.hail.master",
  "stage": "sea",
  "when": "under_way and fog < 0.6",
  "rate": "traffic",
  "actors": { "master": { "role": "master", "people": "ship", "tongue": "ship", "fixed": true } },
  "title_key": "hail.title", "intention_key": "hail.intention",
  "lines": [ { "who": "master", "key": "hail.where_bound" } ],
  "options": [
    { "key": "hail", "label_key": "hail.close", "check": "true",
      "effects": [ "encounter close_and_hail" ], "then": "sea.hail.alongside" },
    { "key": "pass", "label_key": "hail.pass", "check": "true", "effects": [ "npc master feeling -1" ] }
  ]
}
```

### 4.3 The panel (UI Toolkit, the user's style)

- Black see-through glass: `rgba(8, 9, 11, 0.78)` over 3D, `0.92` over the chart (UI Toolkit blends in linear light; lower looks grey). Border 1 px pure black, `border-radius: 0`. Text `rgb(232, 231, 226)`; secondary text at 65%. Font Gentium Book Plus (SIL OFL [Gentium]) at 18 px body, 22 px title, 15 px tooltips: **copied into Kapman2 from the first build** (`G:\kapman_unity\Kapman\Assets\Kapman\Game\UI\Fonts\` with `GentiumBookPlus-OFL.txt`, plus `TextPanel.uss` and `GamePanel.asset`), as a step of the first slice.
- Layout, top to bottom: title (the place or the happening) · the speaker line in secondary text, with the feeling word ("Hrodgar, the count's toll man · cool") · the lines (a line that is not understood shows the gesture in secondary text) · the intention in plain words · the options, one per row, `1.` to `9.`, the default marked for Enter, a greyed option showing its rule on hover · the outcome in facts after the choice · a footer: "Esc: later" where the card allows it.
- Width 560 px at 1440p, left of centre in 3D (the ship stays visible on the right). Behind it, the glass alpha dims the scene; a blur is optional: add a DepthOfField override on the level volume (HdrpVolume.Get<DepthOfField>) driven by the runner at weight 0.6 while open, costing about 0.3-0.6 ms at 1440p on a 4070 (within the 2 ms open-frame budget), or leave it out and dim with the glass only. The user's call (7.5).
- No icons, no portraits (the user's art later), no colour accents; a warning (law, a man's life) in a muted ochre.
- Plain wording in the labels: "Declare the cargo", "Give him something", "Show the charter", "Pass her by", "As last time".

### 4.4 Tests (EditMode, headless: `-runTests -testPlatform EditMode`; all in Kapman.Tests.EditMode over Kapman.Core only)

| Test | Checks |
|---|---|
| `CondParserTests` | every expression in every card and every line parses; precedence (`a or b and c`); words resolve to thresholds; unknown variable is an error at load, not at play; the aggregate variables (port.best_feeling, chest.latin_count, hold.from_norse, crew.herb_wise_count) evaluate over a fake IVars |
| `EncounterLoaderTests` | all JSON loads; every key referenced exists in writing.json or is reported as `[write:]`; no option without a label key; every `then` target exists (it is an option field, so the check is direct); no card both `arrival` and `weight > 0`; every effect string parses to a verb in 2.7; chance objects have both branches; at most one `default` per card |
| `EffectsTests` | with `FakeGame`: each verb changes exactly what it says; a chance object takes the success branch at a forced roll of 0 and the failure branch at 1; `flag .. for 30` expires on day 31; `toll eased 30` halves the toll for 30 days; `price_shock` expires |
| `SchedulerTests` | same seed + same hour → same draw; at most one pending; cooldown honoured; a scheduled card fires on its day and at its port only; 10,000 visit draws over a pool of five live cards give each card's weight within 5% (one process, so exact in expectation); 10,000 hours at sea give the found-card rate within 5%; a 30-day jump (720 ticks in one call) yields the same pending card and cooldown state as 720 single ticks; a backward jump fires nothing |
| `NameForgeTests` (night two) | deterministic by seed; parts only from the people's lists; no duplicate within a port's 50 NPCs |
| `NpcRegistryTests` | fixed role returns the same NPC for the same merchant; relationships are per merchant id; feeling decays by half in 730 days; memory capped at 12; cap of 2,000 drops the oldest unseen; an NPC forged at age 60 in 793 is dead by 850 and the role re-forges; on succession every feeling is halved and memory entries carry the generation; save and load round-trip byte-equal |
| `TonguesTests` | kin table symmetric where it should be; the year table lifts Norse-English to 0.5 in 900; comprehension picks the best aboard; a Latin formula line needs learned.latin, a monk's own line does not; the masking keeps nouns and numbers at 0.5 |
| `ArrivalTests` | plays the Dorestad arrival set with `FakeGame` (5, done-when 2) |
| `VoiceTests` | a lint over writing.json: a word list of modern idiom the user can edit ("okay", "guys", "percent", "minutes" at sea, "o'clock", "glass" (as weather), "barometer", "compass", "mile", "emperor" in a line gated before 801); fails the build only if the user turns it on |

### 4.5 Performance (RTX 4070, 1440p, 60 fps)

- The hourly tick evaluates at most ~300 conditions: < 0.1 ms once a real minute; a 30-day jump is 720 ticks in one frame, about 50 ms once. Per frame the runner does nothing but compare `lastHour` with the clock and check `pending`.
- The visit draw is one tick's cost on mooring.
- The panel is one UIDocument; opening it costs one layout pass (~0.3 ms). The optional DepthOfField override costs 0.3-0.6 ms while open.
- The registry at 2,000 NPCs: 1-2 MB in memory and in the save; lookups by (port, role) and (npc, merchant) through dictionaries.
- Budget: 0.2 ms per frame average, 2 ms on the frame a card opens. No GC allocations in the tick (conditions are compiled to delegates at load; variables are read through a struct).

---

## 5. First slice

### Night one (the headless core and one card on screen)

**Build:** `Cond`, `EncounterLoader` (with the cross-checks of 4.4), `Effects` parser + `IGameEffects` + `FakeGame`, `Scheduler` (arrival sets and the visit draw only), `NpcRegistry` (fixed roles, feeling with words, memory, relationships by merchant id; no decay or death yet; names from a 20-row `names_placeholder.json`, no NameForge), `MooringTrigger` at the Dorestad dam heads, `EncounterRunner` with one hook (`Moored` → arrival set), `EncounterPanel` as the first build's TextPanel copied with `TextPanel.uss`, `GamePanel.asset` and the Gentium fonts from `G:\kapman_unity\Kapman\Assets\Kapman\Game\UI` (the styled panel of 4.3 is night two). Data: `harbour_793.json` with **one card**, the toll man (the sample above), `npc_roles_793.json` with the tollman role and the feeling words, `writing.json` keys as `[write:]`. No reference to `Kapman.Game` anywhere in it: `GameEffects` waits for the port to compile.

**Done when** (all headless except the last):
1. `CondParserTests`, `EncounterLoaderTests`, `EffectsTests`, `NpcRegistryTests` (the night-one subset) pass.
2. `ArrivalTests` plays the arrival set with `FakeGame`: the toll man opens first; "declare" records `Toll(sales)`; "gift" records `Silver(-12)` and `Deed(bribed_officer, dorestad)` **in the fake's logs** (not through Reputation); the toll man's feeling is +8 with one memory entry; a second arrival in the same game meets the same NPC (same id, same name), shows the `greet_again` line, and has "declare" as the default; a different merchant id meets the same NPC with feeling 0.
3. In Play (the user's smoke test): sail to a dam head and come to rest; the card opens on black glass with the Gentium font; keys 1-4 choose, Enter takes the default on the second arrival, Esc is refused on the toll man; the panel shows `[write: tollman.greet]` where no text is written. Nothing happens to the purse yet (the fake is wired in Play too until `GameEffects` exists).

### Night two

`NameForge` + `names_793.json` + `NameForgeTests`; the berth card and one "Talk to people" merchant; the styled panel of 4.3 with the feeling word and tooltips; `SchedulerTests` for the visit draw and the time jump; `GameEffects` over the Game port once it compiles and is committed, and the `Kapman.Game` reference in the test asmdef only then.

---

## 6. The 25 sample encounters (placeholders; the user rewrites every line)

Each: where and when · who · the lines (shape and voice only; a line gate is given as `when`) · options with checks · what persists. Standing kinds are from reputation_793.json; feeling words from 2.5. "He" and "she" follow the role; the user chooses names from the forge or writes them.

### Harbour

**1. The toll man (Dorestad, Frankish realm)** · arrival, every call, Frankish ports with a toll · a fixed NPC, Frank or Frisian, Christian, speaking the port's tongue.
Lines: "The count's toll is a tenth on what you sell here. What do you carry?" · `when year < 805`: "No mail coat leaves the realm, and no Christian is sold out of it. You know this." (Herstal 779 on brunias: to check.) · `when year >= 805`: "Swords and mail go nowhere east of the Elbe but by the king's posts. You know this." (Thionville 805 [Thionville].) · `when port.visits > 0`: "As last time?"
Options: Declare the cargo (toll sales; the default on a return visit) · Hide the swords under the ballast (check `hold.weapons > 0`; chance 0.75 by `crew.trade_best`; failure: seizure, `goods_seized`, feeling -25) · Give him something (12 d; `bribed_officer`; toll eased 30 days) · Show the charter (check `text.charter_toll_dorestad`; toll none).
Persists: his feeling and memory; the flag `hid_goods_at_port` makes the next search likelier (×3 on the king's-men card).

**2. The harbour master: a berth** · arrival, emporia and towns, only when a fee or a known man is in play · fixed NPC, the port's people.
Lines: "The heads of the dams are for the men who pay the year. You lie inside, by the ditch, unless…" (Dorestad's comb of dams: PROGRESS.md.)
Options: Lie inside (no fee; a day longer to load: `days +0.5` on departure) · Pay for a head (6 d) · Name a man he knows (check `port.best_feeling >= friendly`; free) · Argue your standing (check `standing_word >= known`; free, feeling -3).
Persists: where the ship is moored in the level (the berth id used next time).

**3. The mint, and the new deniers** · errand, ports with a mint; and a card of its own on the first call after `reform.coin` turns 1 at that port (the 793/4 reform [Frankfurt]) · a moneyer, fixed NPC.
Lines (the reform card): "The king's new deniers, and only those, pass here now. Your old ones go by weight, less my fee." · (the errand): "Foreign silver passes by weight, not by any stamp. These sceattas are old and thin. Half, by weight."
Options: Change by weight (fee 8%, exact by the scale; `Trade.Purse` zones) · Change only what you need (you choose the amount) · Keep your silver (nothing; after the reform the Frankish markets take it at weight less a discount until changed).
Persists: nothing but silver; the moneyer remembers a man who argued over a clipped coin (feeling).

**4. A merchant proposes a partnership (félag)** · visit draw, Norse and Frisian emporia · a forged master with a ship on the lanes.
Lines: "I have cloth and no room, you have room and a debt. Half the venture to Hedeby, half the loss." (The félag as a Norse partnership; a notched tally as the record, to check for the 790s.)
Options: Agree to half (he adds goods to your hold as his lot; at the sale, half the gain to him; `flag partner_<npc>`) · Agree to a third · Refuse (feeling -2) · Ask what he knows first (check `tongue.<his> >= 0.5`; a line of prices into the ledger as `told`).
Persists: the partner NPC; at the sale port a scheduled card settles the shares; cheat him and the news travels along the lanes.

**5. The monastery's guest house** · errand at monastery ports and towns with a minster · the guest-master, fixed, speaking the port's tongue (at Utrecht Old Frisian or Old Frankish); the porter's formula first (Rule ch. 66), in Latin.
Lines (porter, `tongue: latin`): "Deo gratias. Who knocks?" (the formula shows in Latin; the question in his own tongue) · (guest-master): "You are received as Christ is. Three nights, bread and ale, and no meat in Lent. The abbot has letters for Tours, if a Frisian ship will carry them."
Options: Take the letters (check `not flag.carrying_letters`; `carried_letters` deed on delivery; a scheduled card at the target port) · Ask a brother to read your leaf (check `chest.latin_count > 0`; `learn latin +0.1`; the leaf's translation unlocks for that text) · Buy a book (a text from the collectables table at the port's price) · Give alms (10 d; `gave_alms`).
Persists: the letters as a commission with a day; the learned Latin; the brother's feeling.

**6. The king's men search you** · visit draw, rare, Frankish and English realms, ×(1 - standing/200), ×3 if `flag.hid_goods_at_port` · two NPCs, a count's man and a cleric (the missi travel in pairs).
Lines: `when year < 801`: "By the king's writ." · `when year >= 801`: "By the emperor's writ." · "Every bale, every cask. A merchant who carries arms to the Slavs is no merchant."
Options: Open the hold (if nothing forbidden: nothing; else seizure and `goods_seized`, or a hearing: `law hearing in 7`) · Offer a gift (check `silver >= 30`; chance 0.4 by `npc.count.feeling`; failure: `bribed_officer` and `taken_for_judgement`) · Cast off now (check `under_way` possible; `fled_kings_men`, standing -12 among Franks, outlaw flag in the realm) · Show the charter.
Persists: the realm's law state; the pair remembers; the news of a seizure reaches the next port by ship.

**7. The slaver's lot** · visit draw, rare: Hedeby, Birka, Dublin after 841; Frankish ports only after a raid year (`after_raid`) · a forged dealer, Norse or Frisian.
Lines: "Six, taken on the Elbe. Sound." · `when year < 900`: "They go to the Franks or to the Arabs, I care not which, and I'd rather a ship than a road." · `when year >= 900`: "They go to Verdun or to the Rus, I care not which, and I'd rather a ship than a road." (The Rus named from 839 [Rus]; Verdun's trade 10th-century [Slavery].)
Options: Buy the lot (a normal lot in the market; `bought_captives`) · Buy one to free him (price; `gave_alms` ×2; a freed NPC who may join the crew at Oar 2) · Refuse (nothing) · Tell the count's man or the reeve (Christian realm only; `law report`; the dealer's feeling -60; the port's dealers' feeling -10; standing +4 with the Church, to be added as a body).
Persists: the captives in the hold carry the law of every port they pass (Trade.LawNotes); Ravn's line later.

**8. Pilgrims ask a passage** · visit draw, English and Frankish Channel ports, April-September · three forged English, one of them not what he seems.
Lines: "We go to the thresholds of the Apostles. We have no silver but our prayers and our hands at the oars." · (the trader among them, if `crew.eyes_best >= 3` or `tongue.old_english = 1`): his bundle is cloaks. (Charlemagne to Offa, 796, on traders among pilgrims: wording to check.)
Options: Carry them to Quentovic (three passengers; heart +0.05 for a pious crew; `carried_pilgrims` deed, to add) · Carry them for a fee (6 d each; the pious man's feeling -5) · Refuse · Ask about the bundle (check as above; the "pilgrim" offers you the cloaks' sale at a share, and the toll man's card at Quentovic gets a `hid_goods` risk).
Persists: the false pilgrim as an NPC who remembers you either way.

**9. A raider's envoy ashore** · after Godfred's 810 demand, Frisian ports; later wherever the chronicle has a fleet nearby · a Dane with two men, not fixed.
Lines: "The king of the Danes asks a hundred pounds of silver of Frisia. Your town's share is on the board. A merchant who pays early sails unmolested this season. One who does not…"
Options: Pay your share (silver; `paid_raiders`; the raiders' sim marks your ship safe for 90 days in that fleet's waters) · Refuse (nothing now; the fleet's raiders hunt you at ×2 for the season) · Hire out as a carrier (check `standing_word <= mistrusted` in Frankish lands or `faith = old gods`; silver; `fled_kings_men`-class standing loss if found) · Report to the count (the count's man's feeling +10; the envoy's -40).
Persists: the fleet's attitude flag; the chronicle's own raid still happens.

**10. Drunk sailors on the strand** · evening draw at any port (long stay) or visit draw, ×2 on a feast day · two forged sailors of another ship, one of your men if `crew.rash_count > 0`.
Lines: "Your Frisian called my steersman a thief. Fists, or silver, or we take it to" · `when port.realm = frankish`: "the count's man." · `when port.realm = english`: "the reeve." · `when port.realm = norse`: "the thing."
Options: Pay the amends (8 d; nothing else) · Let them fight (chance by `crew.fight_best`; win: heart +0.1, their master's feeling -10; lose: a man hurt, health -0.1) · Go to the law (`law case`; win: `won_case`, lose: `lost_case`) · Haul your man aboard (feeling of that crewman -5; nothing else).
Persists: the other master as an NPC on the lanes; a feud flag if the fight went badly.

**11. The widow with a cargo** · after_raid or in a Christian port with a monastery; rare · a forged widow (Frideburg's shape: Vita Anskarii ch. XX, cited in reputation_793.json as c. 20), fixed to the port.
Lines: "My husband is dead and I am old. This silver is for the churches and the poor of Dorestad; my daughter goes with it. Carry them, and God will know your name."
Options: Carry them (the daughter as a passenger NPC; the chest holds a sealed cargo; at Dorestad a scheduled card: give it to the church, `gave_alms` ×3, or keep it, `flag cheated_<widow>`) · Carry them for a fee (10 d; feeling -3) · Refuse.
Persists: the daughter remembers; the widow hears by news within a season; a church that received the alms names you in its prayers (a standing bonus at that port for a year).

**12. The host's three nights** · errand "Lodging" at English ports · a forged householder, fixed.
Lines: "Three nights and I answer for you before the reeve. Who else is in the wic this week? Two Frisians, a Dane buying cloth, and a man who asked your name." (Hlothhere and Eadric 15: to check.)
Options: Lodge three nights (4 d; the crew's heart +0.05; the host's news into Known as `told`, hops 0; rivals in port shown) · One night (2 d; half the news) · Sleep aboard (nothing; a theft chance ×2 that night) · Ask who asked after you (check `npc.host.feeling >= easy`; the asker's card is scheduled for tomorrow).
Persists: the host's feeling; what he told you is dated and sourced in the Known store.

### At sea

**13. A ship hails** · a SimShip within hail (the first build's Encounter) · her master, fixed to the ship.
Lines: "Whence, and whither? We are out of Quentovic for the Rhine, wine and querns. The wind backed south-west off the Foreland yesterday." (The weather she met: Encounter.WeatherNews.)
Options: Close and hail (`encounter close_and_hail`, then `sea.hail.alongside`: she heaves to; prices into the ledger as `told`; a small trade from her hold; a riddle now and then, `Chest.GivenAtSea`) · Pass her by · Ask her master aboard (check `tongue.<his> >= 0.5` and `npc.master.feeling >= easy`; a shared meal, heart +0.03, his feeling +5; a story text if he is `talkative`).
Persists: the master's feeling and memory; he carries your name to his next port (news).

**14. Wreck survivors on a sandbank** · found card, ×4 within a day after a storm (LevelWeather.Storm > 0 in the last 24 h) · three forged survivors, their master dead or not.
Lines: "She broke on the bank in the night. Four of us. The master is under her. Take us off, for the love of God, or the tide takes us."
Options: Take them off (crew +n as passengers; food and water drawn; at the next port they go, or one asks to hire on at a known Oar stat; `fought_raiders`-class honour, to add as `saved_lives`) · Take them off for their master's goods (a lot from the wreck; feeling -10 each; the law of wreck in that realm: to check) · Leave them (heart -0.1 for a pious crew; a scheduled card: their kin hear, feeling -50, news).
Persists: the saved men as NPCs with a debt to you (`owed`), who may repay with news, a berth, or a tip years later.

**15. A monk who reads Latin** · a passenger card at a monastery port, or found adrift in a small boat (rare) · a forged monk, Northumbrian (Old English) or Frankish (Old Frankish), fixed; his own lines are in his own tongue, understood by the kin table like anyone's.
Lines: "I go to Utrecht, to Willibrord's church. I have no silver. I can read for you, and teach you the letters, if the voyage is long enough."
Options: Carry him (a passenger; each 10 game days aboard: `learn latin +0.25`; Latin texts in the chest translate while he is aboard: ReaderAboard over the crew and passengers) · Carry him and keep him a season (check `crew.pious_count > 0` or `npc.monk.feeling >= friendly`; he stays as crew at Trade 2, Fight 1; teaches to 1.0) · Refuse.
Persists: the merchant's Latin for good; the monk at Utrecht later as a fixed NPC who sells books.

**16. A raider's parley at sea** · the raider sim's chase ends alongside (RaiderEncounter) · the raider captain, fixed to the band.
Lines: "Half your hold and you sail on. All of it and your lives if you make us board." · (if `tongue.old_norse >= 0.5` and a Dane aboard): "He knows my steersman's brother. The price is a third."
Options: Pay a third or half (goods; `paid_raiders`) · Fight (chance by `crew.fight_best` and men; `fought_raiders` win or lose; men lost) · Run (chance by `crew.seamanship_best` and wind) · Offer a man as surety (check a crewman willing: `rash` or `brave`; he goes with them for a season; a scheduled card at Hedeby to buy him back).
Persists: the band's captain remembers a merchant who paid, fought or ran; his band's attitude flag on the lanes.

**17. Becalmed beside a fisherman** · `wind < 1.5` on a stretch with fish weirs or boats · a forged local fisherman.
Lines: "You'll not get round the Maas mouth on that tide. The channel runs north of the bank since the winter floods. Fish? Two baskets, for a knife." (Not "the Hook": Hoek van Holland is a name of the 1860s [Hook].)
Options: Buy fish (a knife or 1 d; `goods dried_fish +1`) · Ask the channel (check `tongue.<local> >= 0.5`; a local pilot note: the level's shoal marked on the chart for this season; `Known` as `told`) · Hire him as pilot to the next port (3 d; the ship's grounding risk ×0.3 on this stretch) · Sail on.
Persists: the fisherman at that stretch; the chart note ages.

**18. A king's ship stops you** · Frankish coast after 800 (Charlemagne's fleet orders: to check), English after 896; ×(1 - standing/200) · a forged commander and a clerk.
Lines: `when year < 801`: "The king's ships keep this coast against the Northmen." · `when year >= 801`: "The emperor's ships keep this coast against the Northmen." · "Your name, your port, your lading. A Frisian with Danish cloth in June is either a trader or a scout."
Options: Answer plainly (if `hold.from_norse > 0` and a Dane aboard: a hearing at the next port, `law hearing in 7`; else nothing) · Show letters from a monastery (check `flag.carrying_letters`; free passage, feeling +5) · Run (chance by wind; `fled_kings_men`).
Persists: the realm's law state; the commander on the lanes.

### The crew

**19. Council before a crossing** · the ship mode, when a crossing of more than a day is planned · the seats: steersman, lookout, mate, trader, skald or priest.
Lines (each from traits): steersman (steady): "The swell is up from the west before any wind. Wait a tide." · lookout (rash): "Go now, the wind serves." · the pious man: "An offering at the shore first, or a mass, before open water." · the trader: "Every day here is a day the Hedeby price falls."
Options: Go now · Wait a tide (6 game hours) · Wait for the offering or the mass (half a day; heart +0.05 for the faithful of that kind, -0.03 for the others; a quarrel pair moves toward feud) · Ask the steersman only (check `npc.steersman.feeling >= friendly`; the weather model's next 12 h as `likely`).
Persists: each seat's feeling about being heard; a steersman overruled twice asks to be paid off.

**20. Grievance: the share and the pay** · by the crew's kind (1.1, Plans/crew.md C3): hired hands on `crew.kind = hired and days_since_paid > 60 and venture_ended`; partners on the settlement card at the sale port (card 4's follow-up); kin never · the mate, speaking for the men.
Lines (hired): "The venture is sold and nothing shared. The men say a merchant who cannot pay is no merchant. Half now, or they drink the ale themselves at Lundenwic." · (partners): "The cloth is sold. The tally says a third is ours. Count it out before the toll man does."
Options: Pay half now (silver) · Promise at the next port (heart -0.05; a scheduled card at the port that checks you paid: if not, two men leave) · Give the ale (goods; heart +0.1) · Refuse (heart -0.15; `light-fingered` men steal from the hold that night).
Persists: the mate's feeling; the men's pairwise feelings; who left and where (they go to that port's pool and talk).

**21. Sickness aboard** · `days_at_sea > 4 and health < 0.85`, as the first build's `fever` · the sick man and the mate.
Lines: "Thorkel burns and shakes. The old man says bleed him; the priest says pray. He rows for no one today."
Options: Rest him in the stern (chance 0.7; failure: he dies, heart -0.1, his friends' feeling -10) · Keep him at the oar (chance 0.4) · Put in to the nearest shore (a day lost; chance 0.85) · Ask the man who knows herbs (check `crew.herb_wise_count > 0`; `herb-wise` is not in Plans/crew.md's 12 traits: the user adds it as a seventh pair, herb-wise/ham-fisted, or the option goes; chance 0.8).
Persists: a death is a memory for every man aboard, and news at his home port (his kin's feeling).

**22. Faith clash: a fast day and a feast** · Feasts.On gives a fast (Lent, Good Friday) with heathens aboard, or a heathen offering day with Christians · the pious Christian and the heathen.
Lines: "He puts meat on the fire on Good Friday. Either he eats it ashore or I do." · "Your Frank spat at the offering. On my coast that costs a hand."
Options: Order the meat put away (the heathen's feeling -8; the Christian's +5) · Let each keep his own (both -3; the pair moves toward feud) · Put one ashore at the next port (that man leaves; his friends' feeling -10) · Eat with the heathen (check `faith = old gods` or `prime-signed`; Christians -10).
Persists: pairwise feelings; a feud becomes a fight card in a week.

**23. Reading with the crew: a rune stick** · the chest, when a rune text is opened and a crewman knows runes · that crewman.
Lines: "Give it here. ᚴᚢᚦᛘᚢᛏᚱ… that is a man's name, Guthmund. The rest is yours; I'll say the sounds and you say the word." (RuneReading: one rune, several sounds.)
Options: Let him read his part (his words fill one word in three; the rest are the player's slide-the-sounds puzzle) · Ask him to read it all (check `npc.<him>.feeling >= friendly`; he does, but the merchant learns nothing: `learn runes +0`) · Read it yourself (`learn runes +0.1` per word).
Persists: the merchant's runes skill; the crewman's pride (feeling +3 when asked).

**24. A riddle set in the hall** · at a Norse hall port, a feast evening; or by a `talkative` crewman at anchor · the host or the crewman as setter.
Lines: "A guest who cannot answer a riddle drinks for it. 'I came from home, I wended my way from home, I saw a way of ways; way underneath, way above head, way on all sides.' Read my riddle." (Gestumblindi 2, texts_793.json, Vigfusson and Powell 1883.)
Options: Answer now (drag from the palette, riddles_793.json; right: `won_riddle`, the host's feeling +10, a reward; wrong: a try lost and a drink) · Take it to your chest (give_riddle; answer him on a later visit: the setter's card with `riddle.<id> = solved`) · Set one back (check `chest.riddles_solved > 0`; a contest; win: the host's feeling +15 and a gift).
Persists: the riddle's tries and rest state; the setter's memory of a wrong answer.

**25. The owner's buried silver** · after_raid, the owner is a known NPC whose port the chronicle raided while they were ashore · the owner (a trader, a priest, a widow), or their heir with the father's memory of you (`npc.owner.generation`).
Lines: "When the Danes came I put the house's silver in the ground: from the old Roman stone, toward the mill, four hundred paces, then a forearm deep by the alder. I cannot walk it now. A third is yours."
Options: Agree to a third (open_fetch; the measures to the chart; the walk and the dig of 2.10) · Ask for half (check `crew.trade_best >= 4` or `npc.owner.feeling >= friendly`) · Take an advance and a quarter (20 d now) · Refuse.
Persists: the fetch card until done; the share paid or the cheat flag; if church plate, the `dug_church_ground` question when the spot is in the churchyard.

---

## 7. Open questions for the user

1. **How deep does a single card go?** This plan caps one card at three steps and makes depth come from follow-ups months later (the CK3 way). If you want long branching scenes in one sitting (the Sunless Sea way), the data format needs `then` chains with their own state, and the panel a back-scroll. Which?
2. **Chance shown as a word or hidden entirely?** "Sure / likely / doubtful" from the Known words, as proposed, or nothing (the player reads the rule only)?
3. **Named historical people as NPCs** (Gervold at Quentovic, Herigar at Birka, Alcuin at Tours): yes with fixed years, or forged people only, with the real ones kept to the chronicle?
4. **The Church as a standing body** of its own (alms, letters, captives freed, church ground dug), alongside places and regions: yes or fold it into the places?
5. **Comprehension masking:** show the original-tongue words you cannot read (you write both), or show "…"? The first is more work for you per line. And behind the card: the glass alone, or a depth-of-field blur too (0.3-0.6 ms)?
6. **A seventh trait pair** (herb-wise / ham-fisted) for card 21, or drop the herb option?

---

## Sources

- [Dorestad] https://en.wikipedia.org/wiki/Dorestad (mint; toll and harbour fees assumed, collected by the king's men; raids 834-863)
- [Quentovic] https://en.wikipedia.org/wiki/Quentovic (Gervold procurator; royal toll up to 10% per exchange, a figure the article gives without a reference; abbots as procurators)
- [Gervold] https://en.wikipedia.org/wiki/Gervold (abbot of Saint-Wandrille from 787, d. 806-807; customs in the ports and markets between Rouen and Quentovic; envoy to Mercia)
- [Offa] https://en.wikipedia.org/wiki/Offa_of_Mercia (the 796 letter: pilgrims, black stones, cloaks; the c. 789-790 closing of Frankish ports; Gervold's part)
- [Teloneum] https://en.wikipedia.org/wiki/Teloneum (telonarii; Thionville 805 on just and unjust tolls; exemptions)
- [Thionville] https://de.wikipedia.org/wiki/Diedenhofener_Kapitular (805: "a kind of weapons embargo" against the Slavs; trade only at Bardowick, "Skaesla", Magdeburg, Erfurt, Hallstadt, Forchheim, Premberg, Regensburg and Lorch; text in Boretius, MGH Capit. I nos. 43-44, pp. 120-126)
- [Frankfurt] https://en.wikipedia.org/wiki/Council_of_Frankfurt (794: "new silver pennies bearing the monogram of Charlemagne were to be minted throughout the realm"; the reform and the Carolingian pound dated to 793 and 794)
- [Missi] https://en.wikipedia.org/wiki/Missus_dominicus (802; pairs of a churchman and a count; strangers to the district)
- [Reeve] https://en.wikipedia.org/wiki/Reeve_(England) (gerefa; portreeve; witnessing sales; courts)
- [Anskar] https://sourcebooks.fordham.edu/basis/anskar.asp (Rimbert, Life of Anskar, tr. C. H. Robinson 1921: Herigar ch. XI; boys bought and redeemed ch. XV; merchants and pirates ch. X; lots cast at Birka; the excerpt stops in ch. XVI, before the Frideburg chapter, XX)
- [Ansgar-wp] https://en.wikipedia.org/wiki/Ansgar ("the wealthy widow Mor Frideborg"; "the king's steward, Hergeir")
- [Birka] https://en.wikipedia.org/wiki/Birka (c. 750-975; Ansgar 829-831 and 852; Herigar; kings Björn and Olof; the thing)
- [Hedeby] https://en.wikipedia.org/wiki/Hedeby (c. 770; Reric's merchants moved 808; coins from 825; Ibrahim ibn Yaqub c. 965 on divorce and eye make-up; end 1066)
- [Rus] https://en.wikipedia.org/wiki/Rus%27_people (the Annals of St Bertin, 839: men "who called themselves Rhos" at Louis the Pious's court at Ingelheim, found to be Swedes)
- [Ludger] https://en.wikipedia.org/wiki/Ludger (Thiadgrim and Liafburg; York 767; friction 772; Bernlef)
- [Boarn] https://en.wikipedia.org/wiki/Battle_of_the_Boarn (734: "the Frisian army was beaten and Poppo killed")
- [Alcuin] https://en.wikipedia.org/wiki/Alcuin (the Lindisfarne letters, quoted)
- [Benedict] https://en.wikipedia.org/wiki/Rule_of_Saint_Benedict (ch. 53 guests received as Christ; ch. 66 the porter; "Charlemagne had Benedict's Rule copied and distributed"). The Henderson 1910 translation: https://sourcebooks.fordham.edu/source/rul-benedict.asp
- [Aachen] https://en.wikipedia.org/wiki/Synods_of_Aachen_(816%E2%80%93819) (August 816 and July 817, Louis the Pious and Benedict of Aniane: "the Benedictine Rule was declared the universally valid norm for communities of monks and nuns")
- [Slavery] https://en.wikipedia.org/wiki/Slavery_in_medieval_Europe (Hedeby and Birka; Verdun; councils forbidding sale to non-Christian lands; pactum Lotharii 840)
- [Ine] https://en.wikipedia.org/wiki/Ine_of_Wessex (laws 694; clause on the stranger off the track, quoted)
- [Aelfric] https://archive.org/stream/analectaanglosax00thor/analectaanglosax00thor_djvu.txt (Thorpe, Analecta Anglo-Saxonica, 1834: Ælfric's Colloquy, the merchant's speech, OCR); date c. 995 from https://en.wikipedia.org/wiki/%C3%86lfric_of_Eynsham
- [Havamal] https://en.wikipedia.org/wiki/H%C3%A1vam%C3%A1l (st. 1-4, 11-14, 41-42; Codex Regius)
- [Egils] https://en.wikipedia.org/wiki/Egils_saga (Æthelstan's service; the Head-Ransom; Egil's hidden silver; manuscript c. 1250)
- [Ohthere] https://en.wikipedia.org/wiki/Ohthere_of_H%C3%A5logaland (c. 890; goods; voyages)
- [Wulfstan] https://en.wikipedia.org/wiki/Wulfstan_of_Hedeby (seven days and nights to Truso)
- [Exeter] https://en.wikipedia.org/wiki/Exeter_Book_Riddles (c. 94 riddles, 970s; agreed answers; Tupper 1910)
- [Names] https://en.wikipedia.org/wiki/Anglo-Saxon_names (dithematic parts, listed)
- [Townend] https://www.brepols.net/products/IS-9782503512921-1 (Matthew Townend, Language and History in Viking Age England: Linguistic Relations between Speakers of Old Norse and Old English, Brepols 2002; on "the mutual intelligibility of speakers of Old Norse and Old English")
- [Barometer] https://en.wikipedia.org/wiki/Barometer ("Evangelista Torricelli is usually credited with inventing the barometer in 1643")
- [Neckam] https://en.wikipedia.org/wiki/Alexander_Neckam (De naturis rerum, c. 1190: "the earliest European notices of the magnetized needle as a guide to seamen")
- [Hook] https://en.wikipedia.org/wiki/Hook_of_Holland (the settlement grew with the New Waterway, 1866-1868; "hoek" is corner)
- [Gentium] https://en.wikipedia.org/wiki/Gentium (SIL Open Font License; Gentium Book Plus)
- [Raffelstetten] https://en.wikipedia.org/wiki/Raffelstetten_customs_regulations (903-906; salt and landing dues; MGH Capit. II pp. 249-252)
- [CapVillis] https://en.wikipedia.org/wiki/Capitulare_de_villis (c. 771-800; the iudex's accounts)
- Project files read: PROGRESS.md; Plans/crew.md, map_play.md, map_play_ideas.md, collectables.md, reputation.md, places.md, storms.md, multiplayer.md; Data/events_793.json, riddles_793.json, texts_793.json, reputation_793.json (standing words; the Catla basis line), knowing_793.json (sure 0.8, likely 0.55, doubtful 0.3), hoards_793.json, commissions_793.json, wording_793.json, traffic_793.json (`harbours.ports.<id>.present`); the first build's GameEvents.cs, Encounter.cs, PortLife.cs, TextPanel.cs, RuneReading.cs, Hoards.cs (DigHours 6, Within 40), Reputation.cs, Known.cs, Chest.cs (ReaderAboard stub); Kapman2's LevelClock.cs (T/Y/U jumps, lines 149-151), LevelInfo.cs, LevelExit.cs, LevelWeather.cs, ShipMotion.cs (Goal, Speed, Player), Feasts.cs, HdrpVolume.cs; the asmdefs (Kapman.Core, Kapman.Runtime, Kapman.Game, Kapman.Tests.EditMode); the first build's UI folder (Fonts, TextPanel.uss, GamePanel.asset).

Not verified this pass (each is marked in the text): the exact wording of Charlemagne's 796 letter (EHD I no. 197); Hlothhere and Eadric 15-16 and Wihtred 28; Ine 11 and 25; the Herstal 779 clause on brunias and the later capitularies on selling Christians; the Lex Frisionum shrine clause (Additio Sapientum); Charlemagne's copy of the Rule c. 787; the Bubo spelling of Poppo; Altfrid's York episode; the Sigtuna Frisian guild stones; Charlemagne's coast-guard fleets of 800 and 810; the félag's date.
