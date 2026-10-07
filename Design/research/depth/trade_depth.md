# Trade that rewards thought

6 October 2026. For the user's ask: "do deep research and UI and game mechanics to make this more in depth and thoughtful".
The web search budget was spent before I began. Pages fetched by name are marked **(read)**; memory is marked **(?)**.

## 1. What Kapman has already

The base is sound. Do not rebuild it.

- A price per unit that moves as you deal: scarcity, glut, season, shock, a 20% gap between asks and pays (`Market.cs`).
- The word on the item: eight rules, a tilt, age in words (`Word.cs`). Prices heard carry an error.
- Acts in town: one haggle a call (a coin flip, 0.5), sample the goods, weigh silver, witnesses, a gift (12 d, feeling +8), credit by standing, the toll and the lie about it.

**What is thin.** Every sale is the same act: click sell until the price sags. The haggle is a roll, not a choice.
Goods have no grade. Silver is the only way to be paid. Nothing you leave behind works for you. The six proposals
below each turn one of these into a decision.

## 2. Findings: the games

| Game | What makes its trade deep | What goes wrong | Lesson for Kapman |
|---|---|---|---|
| **Patrician III** | Each town eats and makes goods, so the price is the stock. Every unit you sell is worth less than the last (?). | Players: "The whole gameplay is just about looking at prices, prices, prices" (quoted in `trade_sims.md`). Routes are then automated and thought stops. | Keep the sagging price. Show it as a few steps, not a number that creeps. |
| **Port Royale 3** | "A helpful four-dot display also shows you the effect of your trade on the city's overall supply" (`trade_sims.md`). | The automatic button: "Making money is trivially easy without any user interaction." | Show the effect of the deal before the click. Never automate the first boat. |
| **Offworld Trading Company** | No fixed price for anything. "Supply and demand fluctuates constantly." A black market buy raises its own price, "giving a wary opponent a chance to prepare" (Wikipedia, read). | It is fast and all numbers. | A price is other people's acts. Tell the player whose. |
| **Recettear** | You bargain per customer; a bad offer and "the buyer [will] leave without purchasing anything". News moves prices (Wikipedia, read). | "Once the player has learned the habits of various characters, the price haggling degenerates into a thoughtless, mechanical exercise" (same page). | A haggle must rest on something that changes each call: what you know today. |
| **Sunless Sea** | Plain cargo pays little. The real pay is knowledge carried home: port reports, stories, secrets (?). | Players felt the trade was a grind (?). | Kapman has this right: the word is the wealth. Let the word buy a better price too. |
| **Uncharted Waters** | You invest in a port and it grows. In the fourth game you may trade only where your house holds a share, won "by signing a contract with the local governor" (Wikipedia, read). | Late-game share painting. | A port you have a tie in should trade differently from a strange one. |
| **Concordia** | The Mercator card lets you trade only two kinds of goods a turn, and the storehouse is small (?). | None. It is loved. | A hard small limit makes a choice. Three talks a call is this. Use it again for buyers. |

**The pattern.** The games that stay thoughtful limit the deals, show what a deal will do before it is done, and make
each port differ by who you know there. No graphs, no stock numbers, no automatic route for the first boat.

## 3. Findings: the period

- **Silver went by weight in the north.** Hacksilver is "fragments of cut and bent silver items that were used as bullion or as currency by weight", weighed on scales with standard weights (Wikipedia, Hacksilver, read). Silver was tested by nicking (?).
- **Most pay was in kind.** Ohthere took his dues from the Sami as 15 marten skins, 5 reindeer skins, a bear skin, 10 ambers of feathers and two ship's ropes of 60 ells (Wikipedia, read). At Raffelstetten (903 to 906) a salt ship paid its toll as "three bushels of salt" (Wikipedia, read). Cloth was a money in Frisia and later in Iceland (?).
- **A deal needed witnesses.** The law of Hlothhere and Eadric of Kent: a Kentishman buying in Lundenwic must do it before "two or three freemen of good standing" or the king's wic-reeve (Wikipedia, read). Its tenth clause makes a host answer for his foreign guest (read); the term is three nights (?).
- **Partners laid their goods together.** Félag: "to lay property together"; the partner is the *félagi* of the runestones (Wikipedia, read). The southern commenda gave three quarters to the man with the silver and one quarter to the man who sailed; first written at Venice in 1073 (Wikipedia, read). It is not attested in the north in 800.
- **A gift opened the door.** Ohthere brought walrus teeth to King Alfred (read). Grierson argued in 1959 that gift and plunder moved as much wealth as sale did (?). The return gift was a duty, and a lord gave back more than he got (?).
- **Quality was cheated on.** Notker tells that Charlemagne forbade paying the old price for the new short Frisian cloaks (?). Frankfurt in 794 fixed the public measure and capped grain and bread (in `economy_and_trade.md`).

## 4. Proposals

Each one adds a choice, not a table. All numbers are first guesses for `trade_rules.json`.

### 4.1 Three buyers, not one market

**Rule.** A port's appetite for a good is split among named buyers: one to three, by port size. Each wants a set
count and pays a set price. The best buyer pays `Pays` × 1.15 for the first third of the appetite. The second pays
`Pays` for the next third. The quay pays `Pays` × 0.80 for the rest, and after that the old glut curve runs.
A buyer who is served is gone for 20 days (the glut fade, 0.12 a day, already does this).

**Numbers.** Size 1 to 2: one buyer. Size 3: two. Size 4 to 5: three. Dorestad, wine, appetite 12: the abbot's
cellarer takes 4, the ale-wife 4, the quay 4.

**Seen.** In the market, under the cask: three short lines. "The cellarer of Saint Martin: four casks, pays
well. The ale-wife: four, pays fair. The quay: the rest, pays thin." Click a line to sell to that buyer. A buyer a
rival has served is struck through: "A Frisian sold him six on Tuesday."

**True.** A sale was two men talking. There was no market that took any amount.

**Build.** `MarketGood.Buyers()` derives the three steps from `appetite` and `glut`: no new saved state. Names come
from the port's people, seeded by port and role. `Deal.Sell` takes a step index. `MarketPanel` draws the lines.
Tests: the three steps sum to the old curve within 5%; a rival's sale strikes the best buyer first.

### 4.2 The haggle is what you know

**Rule.** Replace the coin flip. In a haggle you **name a port**. If your newest note for that good there is *seen*
and under 14 days old, the price moves a third of the way toward that note, capped at 8%. If the note is *told*, half
of that. If you have no note, or it is older than 30 days, you may still say it: a bluff. A bluff works on a word
of doubtful, and if it fails prices are 5% worse and he thinks less of you (`haggled_hard`, as now).

**Numbers.** Still once a call, half an hour. Seen: up to 8%. Told: up to 4%. Bluff: 0.3 chance, plus 0.1 for a
crewman good at the scales. He knows what his own port's ships know: if his board holds a newer note than yours,
your claim fails and he says so. That is a free fact for you.

**Seen.** "Haggle" opens your rune sticks for that good, newest first. Click one. "I saw wine pay 91 at
Lundenwic, two days back." He answers in one line and the price changes.

**True.** A price was an argument, and the argument was news. It also makes a fresh note worth silver twice:
once to choose the port, once at the scales.

**Build.** `Known.Leverage(good, port, here)` returns the shift and the reason from the `Ledger` and the port's
`PortBoard`. The haggle act's `effects` call it in place of `chance: 0.5`. Wording keys `haggle.seen`, `haggle.told`,
`haggle.bluff.won`, `haggle.bluff.lost`, `haggle.outdone`. Tests: a seen note never moves the price past the note.

### 4.3 Paid in kind

**Rule.** When you sell, choose **silver** or **goods**. Paid in goods, you take the port's own wares at the middle
price on both sides: no 10% off what you sell, no 10% on what you take. The port's thin purse does not bind. In the
lands of weighed silver before 860, the best buyer pays in kind only.

**Numbers.** In kind is worth about 20% more than selling for silver and buying with it. The cost: you must take
what the port makes, today, at today's stock. Toll is still owed, in a tenth of the goods where the toll is a tenth.

**Seen.** Two scale pans in the market panel. Drag casks to the left pan and the right pan fills with
what he offers: "Four casks for eleven hides." Add or take off until the beam is level. One button: "Struck."

**True.** Ohthere's dues and the Raffelstetten salt toll were in kind. Silver was thin in the north before the
dirhams came in weight. It also makes the return cargo a thought at the moment of sale, not after.

**Build.** `Deal.Swap(m, hold, give[], take[], who)`: values both sides at `Market.Price`, moves stock and glut,
takes the toll in units. `MarketPanel` gains the two pans. Tests: a level swap changes no silver; the swap beats
sell-then-buy by the gap.

### 4.4 Three grades, and the honest measure

**Rule.** Every lot has a grade: **poor, fair, fine**. It moves the price by 0.75, 1.0, 1.3. Unsampled, you see
only "looks fair". The Sample act (built) shows the truth. Buyers care: a hall or a church takes only fine. The
quay takes anything. Age drops the grade: a lot past two thirds of its `keeps` days falls one step.

**Numbers.** From a maker's port: 20% fine, 65% fair, 15% poor. From a stranger at a beach market: 10, 55, 35.
From a host or a known maker: no poor. You may sell poor as fair to a buyer who does not try it (the quay never
does, the cellarer always does). Found out later with odds of likely: `sold_short`, standing −6 there, and the next
buyer samples everything you bring for a year.

**Seen.** A lot in the hold shows one, two or three notches on its tag. An unsampled lot shows a
question notch. In the sale, a line under the buyer: "He will draw a cup first." or "He takes your word."

**True.** Short cloth, sour casks and damp grain were the daily fraud. The king's measure of 794 was the answer.

**Build.** `Lot.grade` (byte) and `Lot.known` (bool), saved as a fifth and sixth value in `Hold.Capture`. The grade
is rolled in `Deal.Buy` from a seeded draw. `Buyers()` carries `takes` (lowest grade). Tests: an old save loads as
fair and known.

### 4.5 The host keeps your goods

**Rule.** After three nights with one host (built: `lodge`), you may **leave goods with him**. He sells them to the
port's buyers as they come back, at the fair price with no glut, and keeps a tenth. You take the silver, or what he
bought with it on a few spoken words, at your next call.

**Numbers.** He holds up to 10 units a port size. He sells one third of the appetite each 20 days. One host a port.
Three hosts in a first life; more with age and kin. If the port is raided, his store is lost. If your man strikes
someone in his town, his door shuts (already in `scale_to_fleets_plan.md`).

**Seen.** In the market, a third choice beside the buyers: "Leave with Wulfhere." On the chart, a small
cask mark by the port. Hold the cask lens and it reads: "Six casks with Wulfhere since June. No word."

**True.** The host who answers for his guest is in the Kentish law. It is the step before "your man at
Lundenwic", and it is how a man with one small boat beat the glut: with time, not with tonnage.

**Build.** `Host.cs`: a per-port store (lots, silver owed, the last day reckoned), caught up lazily like a market,
saved with the game. A hook in `Shocks` for the raid. A reveal line on arrival. Tests: 12 casks left for 60 days at
appetite 12 sells 12; a raid empties it.

### 4.6 The gift before the trade

**Rule.** At a lord's hall or a king's seat you do not open by selling. The first call each year, you **give**. The
lord gives back when you leave. Until you have given, only the quay deals with you.

**Numbers.** A gift worth 5% of the cargo's value, 12 d at least, opens the best buyer and makes the port
toll-free for the year where the toll was the lord's gift anyway. The return gift is the port's own goods, worth
0.5 to 2 times yours: 0.5 where he hardly knows you, 1.0 at fair standing, 2.0 if the gift was a thing his region
wants at strength 3 (wine at a Norse hall). A fine-grade gift counts half as much again.

**Seen.** The hall's site has one act at the top: "Bring a gift." It opens your hold. Click a lot. On
casting off, one card: "Hakon sends down two bundles of marten skins and a rope of walrus hide."

**True.** Gift and return gift were how strangers became safe to deal with. It is also the one place where
giving away the best cask is the best trade of the call.

**Build.** A flag by port and year, `gifted`. `Buyers()` hides the top two steps without it at ports of kind hall
and royal seat. The return gift is drawn on `cast_off` from the port's `made` goods. Tests: value bands; no gift, no
cellarer.

## 5. What these do together

A call at Kaupang in July, with wine. You give the lord your one fine cask (4.6). His steward will now take four
casks, in kind only (4.1, 4.3). You name the price you saw at Ribe nine days back and he gives way a little (4.2).
You level the pans against hides, and sample them first, because a stranger's hides are one in three poor (4.4).
Four casks are left and the quay pays thin, so they stay with your host until autumn (4.5). Six decisions. No
table was read.

## 6. Order to build

1. Three buyers (4.1): one night. It changes how every sale reads and needs no new saved state.
2. The haggle from notes (4.2): half a night. It pays back the word on the item.
3. Grades (4.4): one night, with the save change.
4. Paid in kind (4.3): one night, most of it the two pans.
5. The gift (4.6): half a night.
6. The host's store (4.5): one to two nights. It is the first thing that works while you are away, so test it most.

## Sources

- Read: https://en.wikipedia.org/wiki/Offworld_Trading_Company ; https://en.wikipedia.org/wiki/Recettear:_An_Item_Shop%27s_Tale ; https://en.wikipedia.org/wiki/Uncharted_Waters ; https://en.wikipedia.org/wiki/Hacksilver ; https://en.wikipedia.org/wiki/Raffelstetten_customs_regulations ; https://en.wikipedia.org/wiki/F%C3%A9lag ; https://en.wikipedia.org/wiki/Commenda ; https://en.wikipedia.org/wiki/Ohthere_of_H%C3%A5logaland ; https://en.wikipedia.org/wiki/Law_of_Hlothhere_and_Eadric
- In the project: `Design/research/fleets/trade_sims.md` (Patrician III, Port Royale 3, Rise of Venice, with player quotes and links); `Design/economy_and_trade.md`; `Data/story/town_actions_793.json` (the evidence lines under haggle, sample, weigh_silver, witness).
- From memory (?): Sunless Sea, Concordia, the Patrician III price rule, Notker on the short cloaks, Grierson 1959, the three nights of the Kentish host.
