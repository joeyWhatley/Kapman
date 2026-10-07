# Kapman chains: event chains and story lines (4 Oct 2026)

The user: "create event chains, story line chains", "extensive", "with good payouts".

A **chain** is one story told over eight to fourteen cards, across several ports and over months or years, with real
choices, at least two ways through, and a finale that pays well. It is built only from what the encounter engine
already has (BRIEF.md rule 5, BRIEF_2.md): cards, conditions, effects, flags and scheduled cards.

## Your three files

- `Assets/Kapman/Game/Data/encounters/chain_<id>_793.json` : the cards, in the engine's format
  (`{"about": "...", "cards": [ ... ]}`, exactly as `harbour_793.json`).
- `Assets/Kapman/Game/Data/writing/chain_<id>.json` : real words for every key your cards use.
- `Design/content/chains/<id>.md` : one page for a human: the story in five sentences, the real history under it with
  sources, a list of the steps (card id, where, when, what the choices are, which flag each sets), a small table of
  every payout, and how long the chain takes in game time.

## How a chain is wired

1. **Card ids** are `<stage>.chain_<id>.<step>`, for example `harbour.chain_black_stones.s03_reeve`. Give each card a
   `"note"` saying which step it is and its source.
2. **The start** is an ordinary card with a low weight that can come up where and when the story begins
   (`"when": "moored and port.id = dorestad and year < 797 and not flag.ch_black_stones_1"`), or a card another card
   schedules. A chain must be findable: say in your `.md` page how a player meets it.
3. **Progress is flags.** Each step sets `flag ch_<id>_<n>` (letters, digits, underscores; no dots). The next step's
   `when` asks for the last flag and refuses its own (`flag.ch_x_2 and not flag.ch_x_3`). Branches set different flags
   (`ch_x_3a`, `ch_x_3b`) and later steps may ask for either.
4. **Moving the story** to another port or a later day: `schedule <card id> in <days> at <port id>`, or a `when` with
   `port.id = <port>`, `year`, `month`, `feast`, `days_at_sea`, `hold.<good>`, `standing`, `tongue.<name>`,
   `npc.<role>.feeling`. Use the variables of `Cond.cs` and nothing else.
5. **No dead ends.** Every step has an option that costs only time or pride. A failed check hurts and moves the story
   on by another road; it never ends the chain with nothing. Every danger warns first.
6. **Time.** A chain should take a season at the least and may take years. Give steps real waits (a feast day, the
   next spring, a ship's passage) so chains overlap with ordinary trading and with each other.
7. **People.** Use the spine's cast (`Design/content/spine.json`) where they fit, by their ids in your notes, and the
   roles of `encounters/npc_roles_793.json` in `actors`. Invented people are ordinary people with attested names.
8. **Truth.** The story rests on something real: an event, a law, a custom, a find, a text. Say what, with a source.
   The card's `when` keeps it inside its true years.

## Payouts: make them good

The scale of the game's money: the player starts owing 480 d. A cask of wine is about 50 d at Dorestad and about 80 d
at Lundenwic. An ordinary leg of a voyage makes 10 to 40 d; a good voyage 100 to 300 d. A small boat costs about
600 d, a sea-going trader several thousand.

| Tier | When | Silver or its worth | And one of |
|---|---|---|---|
| Step | every step | 0 to 40 d | a price word, a rumour that is true, a friend's feeling up, a deed, a riddle, a text |
| Turn | the middle of the chain, once or twice | 80 to 250 d | a rare good, a hired man with a skill, a commission with a premium, a learned tongue |
| Finale | the end, on every road through | 600 to 2,500 d, or its worth in goods | one **grand reward** from the list below |
| Hard road | the finale by the hardest or most honest road | up to twice the finale | a second grand reward |

Pay in the engine's verbs: `silver +n`, `goods <good> +n`, `commission <good> <n> by <days> premium <f>`,
`give_text <id>`, `give_riddle <id>`, `hire <role>`, `learn latin +f`, `tongue <name> = f`, `deed <kind>`,
`npc <role> feeling +n`, `toll eased <days>`, `price_shock <port> <good> x<f> for <days>`, `news <key> at <port>`.

**Grand rewards** are flags the game will read (they are being wired tonight). Set one with `flag <name>`:

- `unlock_ship_<class id>` : the right to have that ship (a class id from `Data/ship_classes_793.json`), or the ship itself.
- `unlock_charter_<port id>` : a charter of toll freedom at that port.
- `unlock_plot_<port id>` : a house plot or storehouse there.
- `unlock_factor_<port id>` : a trusted man who keeps your goods there.
- `unlock_partner_<name>` : a sworn partner (the Norse felag) who will sail a second boat.
- `unlock_pilot_<water>` : the knowledge of a bar, a strait or a river (name the water in one word).
- `unlock_upgrade_<id>` : a thing fitted to the ship (ids as in `Data/story/ship_upgrades_793.json` when it exists; else name it plainly).
- `unlock_work_<port id>_<what>` : a work in a town, done or granted.
- `unlock_title_<name>` : a standing among men: a seat among the sworn merchants, a lord's protection, a thegn's right.
- `unlock_relic_<name>`, `unlock_heirloom_<name>` : a thing the house keeps and an heir inherits.
- `unlock_kin_<name>` : a marriage, a fostering, a rescued kinsman.

Every reward must be something a merchant of the time could really have won, and the `.md` page says why.
A reward may carry a cost that lasts (a feud, a debt of honour, a watchful bishop): say so and set a flag for it.

## What good looks like

Eleven cards. A priest at Dorestad asks you to carry a letter. At Lundenwic the reeve will not let a Frankish boat
land (true: the quarrel of about 790). You can wait, bribe, land at Sarre under a minster's privilege, or sell the
letter. Each road goes on. Three ports and two winters later the ports open (796), and the man who helped the letter
through is the first Frisian the reeve lets sell black stones on the Strand: a charter at Lundenwic, 900 d of querns
on commission, and a friend in the king's hall. The man who sold the letter has the silver and a bishop who remembers.

## Endings (added 22:35)

The user asked for "different endings to stories" and to be able to "change history". Every chain has **at least three
endings** that differ in what becomes of the people and the place, not only in the size of the purse. Each ending sets
its own flag, `ch_<id>_end_<name>`, pays a finale of its own, and is listed with a picture in the chain page. Where the
chain rests on a real event, one ending is what really happened and at least one is history changed by the player:
say in the page what would follow from it in later years, and set a flag `changed_<event>` so later cards can know.
Add a writing entry `art.<card id>` for every card: one line of what its picture shows.

## If your chain id is thread_01 to thread_12, read this first (added 23:05)

Your task was written before the spine existed. The spine now has 14 threads, and most of them are already being told
by other chains tonight (black_stones, lindisfarne_book, reric_808, saxon_exiles_804, old_gods, mission_north,
silver_road, sword_smuggle, dorestad_834, maldon_991, riddle_trail, house_debt). Do not tell those again. This table
replaces the rule in your task: write the chain named here for your number, under your own id (chain_thread_NN).

| Your id | Write this chain |
|---|---|
| thread_01 | Harald Klak, king of some of the Danes, baptised at Ingelheim near Mainz in 826 with his household, given Rustringen in Frisia, and sent home with Anskar: a merchant carries part of that company and is tied to a king who cannot hold his kingdom. |
| thread_02 | Walcheren, 837: the Danes take tribute on the island, Count Eggihard and Hemming are killed, and the emperor holds an inquiry at Nijmegen into why the coast watch failed. A merchant was there. |
| thread_03 | The Seine, 845: a fleet under Ragnar goes up to Paris at Easter, the king pays 7,000 pounds of silver, and a sickness follows the fleet home. The merchants of the Seine before, during and after. |
| thread_04 | Nantes on St John's day 843 and the island of Noirmoutier (Herio) as the Northmen's winter camp: the salt trade in the hands of raiders, and who still sails for salt. |
| thread_05 | Iona to Kells, 806 to 814: after 68 of the community are killed, the relics of Columba and a great gospel book are carried over the Irish Sea to a new house. A boat is needed, and silence. |
| thread_06 | The spine's thread york_blood (the blood owed at York): tell it in full, with its hooks and cast. |
| thread_07 | Lundenwic left for the walled city: Alfred takes London in 886, plots are granted at the new hythe (Queenhithe, the charters of 889 and 898), and a Frisian house must move its trade inside the walls or lose it. |
| thread_08 | The spine's thread hungry_years (the hungry years): tell it in full, with its hooks and cast. |
| thread_09 | The end of Hedeby: Harald Hardrada burns it about 1050, the Wends sack it in 1066, and the trade moves over the water to Schleswig. A house with a plot there has to choose when to go. |
| thread_10 | The spine's thread book_road (the book road): tell it in full, with its hooks and cast. |
| thread_11 | The house moves: after 863 the river leaves Dorestad and the trade goes to Tiel and Deventer. Moving a house, its debts and its dead, and what is left behind (not the raids themselves: dorestad_834 tells those). |
| thread_12 | Norway made Christian by its kings, 995 to 1030: Olaf Tryggvason, Olaf Haraldsson, Stiklestad, and the saint's shrine at Nidaros that pilgrims and merchants then sail to. A merchant between the old lords and a missionary king. |

Use the spine (Design/content/spine.json) for the cast in every chain: its people have ids, dates and ties.
