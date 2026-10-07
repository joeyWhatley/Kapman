# Orders: taken or refused, brought in parts, paid like a sale, closed on their day

7 October 2026. For the builder. It answers point 11 of `Design/critique/review_systems.md` ("Orders are broken three
ways") and the part of its change 2 that names orders ("close the doors that pay more than trade does: order premiums,
untolled orders").

**State of this write-up.** Every code block below was compiled outside Unity with the project's own compiler and
references (Roslyn of 6000.6.3f1; the response files of `Library/Bee/artifacts/1900b0aE.dag`), all drafts in at once:
`Kapman.Game`, `Kapman.Graphs` and `Kapman.Tests.EditMode` build with no error and no new warning. Nothing was run:
the tests need Unity. Nothing in the project was changed. The table in part 7 comes from a script over the card files
as they stood at about 12:25 today; the content pass on the cards is still working, so run the script of part 5 after it
lands (it can be run again at any time).

## 1. What is wrong today, with file and line

All paths are under `Assets/Kapman/Code` unless they say otherwise.

| # | What | Where |
|---|---|---|
| a | A card that says "take the order" only posts it. `GameEffects.Commission` adds the order with `mine` left false. It then has to be taken a second time in Tasks, at that port, and until then `Commissions.Daily` gives it 4 chances in 100 a day of being filled by another (`aiFill`, 0.04). | `Game/Encounters/GameEffects.cs` 161-175; `Game/Ports/Commissions.cs` 63-64 |
| b | An order is brought whole or not at all: `Deliver` skips it while `Trade.Hold.Count(c.good) < c.units`. The first boat has 660 kg of room (cargo 900 kg less four water casks of 60 l), and a cask of wine takes 138 (115 kg x bulk 1.2): four casks. 63 of the cards' 220 orders ask more than one hold of that boat; 35 of the 70 wine orders do. Nothing tells the player before he takes it. A broken order costs 5 points of standing. | `Game/Ports/Commissions.cs` 128; `Game/Data/voyage_rules.json` 15-18 (under `Assets/Kapman`) |
| c | The pay is the market's price that day times (1 + premium) times the units, fixed, and paid whole into the purse with no toll (`Trade.Purse.Receive(c.pay, ...)`). 126 of the 220 card orders have a premium over 0.5; 86 have 1.0 or more; 49 have 1.5 or more. At usual prices the 220 orders pay 87,073 d, of which 43,071 d is over the market. The 60 d cap of ruling 14 counts only `silver +N` and does not see them. | `Game/Ports/Commissions.cs` 84, 131; `GameEffects.cs` 169 |
| d | The price is taken from the day's stock. Where the good is made, buying the town's stock out first makes the price up to 2.2 times dearer (`price_max`), and the order then locks that in, times (1 + premium), for the goods just bought. A way to print silver. | `Game/Trade/Market.cs` 273-283 |
| e | After a load, new orders can get the id of an old one (`made = All.Count + 1000`). It did no harm while ids were only used for redrawing; the ask of part 3 names its window by id. | `Game/Ports/Commissions.cs` 154 |
| f | 208 card options give the 220 orders. 85 of the 220 are given by a choice whose words do not name an order ("Ask instead for the hall's wine custom", "Lay a gift of thirty pennies on the hall's table"). If the card simply set `mine`, the player would be bound by promises he did not know he made. | the cards (a word search of the choices' labels, so the 85 is rough; not listed here, because rule 1 makes it harmless) |

The Sarre order the critic names (`harbour.wine_for_the_minster.sarre`, `rhenish_wine 4 by 240 premium 1.5`): the
market at Sarre usually pays 75.6 d a cask (60 d x 1.4 wanted, less half the spread). Today 4 x 75.6 x 2.5 = 756 d,
untolled. The debt is 480 d.

## 2. The rules

1. **Offered, then taken or refused.** A card does not take an order for the player. It *offers* one, by name. The
   offer is kept for him (no other merchant fills it) and the game asks at once, in a small window over the chart:
   the terms, then "Take the order" or "Refuse it". Shut without an answer, the offer waits in Tasks under "Orders
   offered to you" for 7 game days and then goes, as an ordinary card left open takes the choice that changes least
   (ruling 12). Refusing costs nothing. A port's own posted order is as before: open to all, taken or left in Tasks
   where she lies, and another merchant may fill it.
2. **Taken, it stands in Tasks with its day.** One line an order: what, for where, by what date, the price a unit
   and the whole; under it the days left, how many are brought, how many are aboard, and how many loads it is for the
   boat she has. A taken order cannot be handed back.
3. **The pay is the market's price there plus at most a half, and at most 60 d over the market in all.** The price
   is so much a unit, fixed on the day the order is given. "The market's price there" is what that market pays a
   seller that day, but never more than it usually pays in that month (`Market.UsualPrice`, less half the spread), so
   neither a run on the stock nor a famine can be locked into an order. On top comes the premium the card or the
   port's kind asks, cut to 0.5. Then, if premium x market x units is more than 60 d, the premium is cut again so
   that the whole order pays 60 d over the market and no more: an order from a card is a card's pay, and ruling 14
   says no card pays over 60 d. Both numbers are in `commissions_793.json` (`premium_max`, `bonus_max_d`; 0 turns
   the second off).
4. **Tolled like any sale.** Each part landed pays the toll of the place on what it fetched: the zone's share times
   the card's toll factor (`EncounterRunner.TollFactor`), nothing where a riddle won freedom from toll
   (`Chest.TollFreeAt`). The same line as `Deal.Sell` (`Game/Trade/Deal.cs` 114-115). An order's goods go to the
   one who ordered them, not to the market: they make no glut.
5. **In parts.** In that port, the player hands over what he has aboard, up to what is still wanted; each part is
   paid as it lands. The last part closes the order: a deed kept (`kept_commission`, +4 there). Nothing checks the
   size of an order against the hold any more, except that an order she cannot carry one unit of cannot be taken,
   and the button says why ("She cannot carry horses: one alone is more than her hold").
6. **When its day passes.** At the first day's turn after the date, with units still wanted: nothing brought, the
   order is broken (`broke_commission`, -5 there, and the news of it travels); part brought, it is short (a new
   deed, `short_commission`, -2), and what was landed stays paid. Either way the order goes from Tasks and the log
   says so. Goods still aboard are the player's to sell where he likes. There is no late delivery.
7. **Warned first.** Seven game days before its day, and again on the last day, the log says what is still to bring
   and the clock drops to its slow speed (ruling 26: it drops back when something needs the player).

What rules 3 and 4 do to the Sarre order: at most 4 x 75.6 + 60 = 362 d, less a tenth in toll (the English toll as
ruled) = 326 d in the purse; 756 d today. If other merchants have brought wine there and the market pays less that
day, the order is reckoned from the lower price.

## 3. What the player sees

The ask, when a card offers an order (a floating window, `id` "order.<the order's id>", holds the clock as a card
does and lets it run if the player sets it running):

> **An order** - At Serræ
> 4 casks of rhenish wine for Serræ by 27 December 793, at 128 p each (513 p).
> Each part is paid as it lands, less the toll there (10 in the hundred today). Bring all of it by the day and they
> will think the better of you. Bring none and they will remember it; bring part and they will remember that a little.
> *Unanswered, the offer stands 7 days and then goes to another.*
> **1** Take the order **2** Refuse it

(The figures are the Sarre order at the usual price, taken on 1 May 793, with the English toll at a tenth as ruled.
Money is shown in the reckoning of the place, as the market shows it: pennies at Sarre. An order of more than one
hold adds the line "She carries 4 at a time: 3 loads. It may be brought in parts.")

Tasks, full window, the order parts:

- **Orders offered to you** (only when there are any): the line, "The offer stands 5 days more.", and the buttons
  Take and Refuse.
- **Orders taken**: "4 casks of rhenish wine for Serræ by 27 December 793, at 128 p each (513 p)" and under it
  "81 days left. Brought: 2 of 4. Aboard: 2." When she lies there with some aboard: "This is the place: the toll here
  is 10 in the hundred." and the button **Hand over 2**.
- **Orders posted at <port>**: as today, with the loads line, and Take greyed with the reason when she cannot carry
  one unit. Under them: "Left untaken, another merchant may fill it."

The log, in order of a life: "An order is offered: ... Take it or refuse it." (the card's own outcome line) / "You
took the order: ..." / "Aboard for your order here: 2 casks of rhenish wine. Hand them over in the Tasks window." /
"Delivered 2 casks of rhenish wine of the 4 ordered, for 257 p, less the toll of 26 p: took 231 pennies. 2 still to
bring by 27 December 793." / "7 days left for the order at Serræ: 2 casks of rhenish wine still to bring." /
"Delivered the last 2 casks of rhenish wine of the 4 ordered, for ... The order is kept." or "The order for 4 casks of
rhenish wine at Serræ has run out: you brought 2, and were paid for them. They wanted the rest."

For the brief face of Tasks (`Design/fixes/brief_faces.md`, lines 91-95), line 3 should read, in this order of need:
an offer waiting ("1 order offered: take or refuse"), else the taken order nearest its day ("Wine for Serræ: 2 of 4,
12 days"), else "No orders."

## 4. Code

Five files change and one is new. `SaveGame.cs` (124, 201), `PortLife.cs` (62-65), `PortNodes.cs` (194-229),
`Effects.cs`, `IGameEffects.cs` and the test double in `EffectsTests.cs` need no change: the names and shapes they
use are kept.

### 4.1 `Assets/Kapman/Code/Game/Ports/Commissions.cs`: the whole file (it is 164 lines today, last changed 4 Oct)

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>An order: a place wants so many of a good brought to it by a day, at so much a unit.</summary>
    public sealed class Commission
    {
        public string id, port, good;
        public int units;
        /// <summary>Deniers for the whole order before the toll (units x each); the day it runs out; the day it was given.</summary>
        public double pay, deadline, posted;
        /// <summary>Deniers a unit before the toll, fixed on the day the order is given.</summary>
        public double each;
        /// <summary>Units brought so far: an order may be brought in parts.</summary>
        public int done;
        /// <summary>Taken by you: it stands in Tasks until it is brought or its day passes.</summary>
        public bool mine;
        /// <summary>Offered to you by name (a card): kept for you until you take or refuse it, or the offer runs out.</summary>
        public bool offered;
        /// <summary>The last warning said (0 none, 1 a week left, 2 a day left).</summary>
        public int warned;

        /// <summary>Units still to bring.</summary>
        public int Left => Math.Max(0, units - done);
    }

    /// <summary>
    /// ORDERS (Data/commissions_793.json; Design/fixes/orders.md). A place wants so many of a good by a day.
    ///   - WHERE THEY COME FROM. A port posts them by its kind, open to every merchant (others fill them too). A card
    ///     offers one to you by name: it is kept for you, and asked of you at once (take it or refuse it).
    ///   - TAKEN, it stands in Tasks with its day. It may be brought in parts: each part is paid as it lands.
    ///   - THE PAY is so much a unit, fixed when the order is given: what the market there pays that day (never more
    ///     than it usually pays), plus at most a half, and that part over the market is never more than 60 d for the
    ///     whole order. The toll comes off every part landed, as off any sale.
    ///   - ITS DAY PASSED: nothing brought, it is broken and they remember it; part brought, it is short, a smaller
    ///     mark; what was landed stays paid. All brought in time: kept, and your standing there rises.
    /// </summary>
    public static class Commissions
    {
        public static readonly List<Commission> All = new List<Commission>();
        static Dictionary<string, object> byKind = new Dictionary<string, object>();
        static double aiFill = 0.04, premiumMax = 0.5, bonusMax = 60.0, offerDays = 7.0, warnDays = 7.0;
        static int made;
        static System.Random dice = new System.Random(800);

        /// <summary>An order was offered to you by name (the frame asks at once: take it or refuse it).</summary>
        public static event Action<Commission> Offered;

        /// <summary>The most an order pays over the market, as a share of it; and in deniers for the whole order (0: no such cap).</summary>
        public static double PremiumMax => premiumMax;
        public static double BonusMax => bonusMax;
        /// <summary>Game days an offer stands unanswered.</summary>
        public static double OfferDays => offerDays;

        public static void Load(TextAsset json)
        {
            All.Clear();
            made = 0;
            aiFill = 0.04;
            premiumMax = 0.5;
            bonusMax = 60.0;
            offerDays = 7.0;
            warnDays = 7.0;
            byKind = new Dictionary<string, object>();
            if (json == null)
                return;
            var o = Json.Parse(json.text);
            byKind = Json.Obj(o, "by_kind") ?? new Dictionary<string, object>();
            aiFill = Json.Num(o, "ai_fill_per_day", aiFill);
            premiumMax = Json.Num(o, "premium_max", premiumMax);
            bonusMax = Json.Num(o, "bonus_max_d", bonusMax);
            offerDays = Json.Num(o, "offer_days", offerDays);
            warnDays = Json.Num(o, "warn_days", warnDays);
        }

        // ------------------------------------------------------------------ the price

        /// <summary>What the market there pays for one unit, as an order reckons it: its price to a seller today, but never
        /// more than it usually pays in this month. So a run on the town's stock, or a famine, cannot be locked into an order.</summary>
        public static double MarketEach(PortDef p, GoodDef g, int year, int month, double today)
        {
            if (p == null || g == null)
                return 0.0;
            var m = Markets.Of(p, year, today);
            var mg = m != null ? m.Get(g.id) : null;
            if (mg == null)
                return g.value;
            return Math.Min(m.Pays(mg, month), m.UsualPrice(mg, month) * (1f - m.Spread));
        }

        /// <summary>Deniers a unit an order pays, before the toll: the market's price there plus the premium asked, the
        /// premium cut to the most allowed (a half) and to the most the whole order may pay over the market (60 d).</summary>
        public static double PriceEach(PortDef p, GoodDef g, int units, double premium, int year, int month, double today)
        {
            double market = MarketEach(p, g, year, month, today);
            if (market <= 0.0 || units <= 0)
                return 0.0;
            double share = Math.Max(0.0, Math.Min(premium, premiumMax));
            if (bonusMax > 0.0 && market * units * share > bonusMax)
                share = bonusMax / (market * units);
            return Math.Round(market * (1.0 + share), 2);
        }

        static Commission Make(PortDef p, GoodDef g, int units, double days, double premium, int year, int month, double today)
        {
            double each = PriceEach(p, g, units, premium, year, month, today);
            return new Commission
            {
                id = "c" + (++made), port = p.id, good = g.id, units = units, each = each, pay = Math.Round(each * units),
                posted = today, deadline = today + days,
            };
        }

        // ------------------------------------------------------------------ the day's work

        /// <summary>Once a game day: new orders posted, others filled by other merchants, yours warned of and closed.</summary>
        public static void Daily(int day)
        {
            var game = KapmanGame.Current;
            if (game != null)
                Daily(game.Day, game.Year, game.Month, game.World != null ? game.World.seed : 793);
        }

        /// <summary>The day's work, given the day, year, month and the world's seed.</summary>
        public static void Daily(double today, int year, int month, int seed)
        {
            dice = new System.Random(seed * 911 + (int)Math.Floor(today));
            for (int i = All.Count - 1; i >= 0; i--)
            {
                var c = All[i];
                if (c.mine)
                {
                    if (today > c.deadline)
                    {
                        All.RemoveAt(i);
                        Close(c);
                    }
                    else
                        Warn(c, today);
                }
                else if (c.offered)
                {
                    // (kept for you: no other merchant fills it; unanswered, it is let go, which changes least)
                    if (today > c.posted + offerDays || today > c.deadline)
                    {
                        All.RemoveAt(i);
                        Voyage.Say($"The order offered at {GameData.Port(c.port)?.name} had no answer from you: it has gone to another.");
                    }
                }
                else if (today > c.deadline || dice.NextDouble() < aiFill)
                    All.RemoveAt(i);                                        // (filled by another, or no longer wanted)
            }
            foreach (var p in GameData.Ports)
            {
                if (!p.Exists(year) || !(Json.Obj(byKind, p.kind ?? "") is Dictionary<string, object> kind))
                    continue;
                if (dice.NextDouble() >= Json.Num(kind, "chance_per_month") / 30.0 || All.Exists(c => c.port == p.id && !c.mine && !c.offered))
                    continue;
                var goods = Json.Obj(kind, "goods");
                if (goods == null || goods.Count == 0)
                    continue;
                var keys = new List<string>(goods.Keys);
                string id = keys[dice.Next(keys.Count)];
                var good = GameData.Good(id);
                var range = goods[id] as List<object>;
                if (good == null || range == null || range.Count < 2)
                    continue;
                int units = dice.Next((int)(double)range[0], (int)(double)range[1] + 1);
                All.Add(Make(p, good, units, Json.Num(kind, "days", 40), Json.Num(kind, "premium", 0.3), year, month, today));
            }
        }

        /// <summary>A taken order whose day has passed with units still to bring: broken (none brought) or short (part brought).</summary>
        static void Close(Commission c)
        {
            var p = GameData.Port(c.port);
            var good = GameData.Good(c.good);
            string what = good != null ? Trade.Units(good, c.units) : c.units + " " + c.good;
            if (c.done <= 0)
            {
                Sim.Deeds.Record("broke_commission", $"didn't bring {what} to {p?.name}", p);
                Voyage.Say($"The order for {what} at {p?.name} has run out with nothing brought: they'll remember it.");
            }
            else
            {
                Sim.Deeds.Record("short_commission", $"brought {c.done} of {what} to {p?.name}", p);
                Voyage.Say($"The order for {what} at {p?.name} has run out: you brought {c.done}, and were paid for them. They wanted the rest.");
            }
        }

        /// <summary>A week before its day, and a day before: said once each, and the clock drops to its slow speed.</summary>
        static void Warn(Commission c, double today)
        {
            double left = c.deadline - today;
            int stage = left <= 1.0 ? 2 : left <= warnDays ? 1 : 0;
            if (stage <= c.warned)
                return;
            c.warned = stage;
            var good = GameData.Good(c.good);
            string still = good != null ? Trade.Units(good, c.Left) : c.Left + " " + c.good;
            Voyage.Say(stage == 2
                ? $"Tomorrow is the day for the order at {GameData.Port(c.port)?.name}: {still} still to bring."
                : $"{Math.Ceiling(left):0} days left for the order at {GameData.Port(c.port)?.name}: {still} still to bring.");
            GameSpeed.Slow();
        }

        // ------------------------------------------------------------------ reading them

        /// <summary>Orders open at a port: posted there for any merchant, not yet taken by you.</summary>
        public static List<Commission> At(PortDef p) => All.FindAll(c => p != null && c.port == p.id && !c.mine && !c.offered);

        /// <summary>Orders offered to you by name and not yet answered.</summary>
        public static List<Commission> OfferedToYou => All.FindAll(c => c.offered && !c.mine);

        /// <summary>Orders you've taken.</summary>
        public static List<Commission> Mine => All.FindAll(c => c.mine);

        /// <summary>How many units of the order's good her hold takes when it is empty (0: she cannot carry it at all).</summary>
        public static int FitsAtOnce(Commission c)
        {
            var g = c != null ? GameData.Good(c.good) : null;
            if (g == null || !Trade.Started)
                return 0;
            return (int)Math.Floor(Trade.Hold.capacity / Math.Max(0.001f, g.Space) + 1e-6);
        }

        /// <summary>How many full holds what is left of the order comes to (0: she cannot carry it at all).</summary>
        public static int Loads(Commission c)
        {
            int fits = FitsAtOnce(c);
            return fits <= 0 || c == null ? 0 : (c.Left + fits - 1) / fits;
        }

        /// <summary>"She carries 4 at a time: 3 loads." ("" when one load does it; the reason when she cannot carry it.)</summary>
        public static string LoadsLine(Commission c)
        {
            if (c == null || !Trade.Started)
                return "";
            var g = GameData.Good(c.good);
            int fits = FitsAtOnce(c);
            if (fits <= 0)
                return $"She cannot carry {(g != null ? g.name.ToLowerInvariant() : c.good)}: one alone is more than her hold.";
            int loads = Loads(c);
            return loads <= 1 ? "" : $"She carries {fits} at a time: {loads} loads. It may be brought in parts.";
        }

        static ZoneDef ZoneOf(PortDef p) => p != null && TradeRules.Current != null ? TradeRules.Current.ZoneOf(p.region) : null;

        static string Show(PortDef p, double deniers)
        {
            var z = ZoneOf(p);
            return z != null ? z.Show(deniers) : $"{deniers:0.#} d";
        }

        /// <summary>The date of a game day, "3 August 793".</summary>
        static string DateOf(double day)
        {
            // ported 4 Oct: the first build read the start year and day off KapmanGame.clock (the old GameClock); the
            // rebuild's level clock (Kapman.Levels.LevelClock) carries the same two fields
            var clock = Kapman.Levels.LevelClock.Current;
            return clock != null ? Kapman.World.Calendar.At(clock.year, clock.startDay, day * 24.0).ToString() : $"day {day:0}";
        }

        /// <summary>"5 casks of Rhenish wine for Lindisfarena by 3 August 793, at 76 d each (380 d)".</summary>
        public static string Describe(Commission c)
        {
            var good = GameData.Good(c.good);
            var p = GameData.Port(c.port);
            string what = good != null ? Trade.Units(good, c.units) : c.units + " " + c.good;
            return $"{what} for {p?.name} by {DateOf(c.deadline)}, at {Show(p, c.each)} each ({Show(p, c.each * c.units)})";
        }

        // ------------------------------------------------------------------ offered, taken, refused

        /// <summary>A card offers you an order by name: kept for you, asked of you at once. Null if it cannot be made.</summary>
        public static Commission Offer(PortDef p, GoodDef g, int units, double days, double premium, int year, int month, double today)
        {
            if (p == null || g == null || units <= 0 || days <= 0.0)
                return null;
            var c = Make(p, g, units, days, premium, year, month, today);
            c.offered = true;
            All.Add(c);
            Offered?.Invoke(c);
            return c;
        }

        /// <summary>Why you cannot take it ("" if you can).</summary>
        public static string WhyNot(Commission c)
        {
            if (c == null || c.mine)
                return "";
            return Trade.Started && FitsAtOnce(c) <= 0 ? LoadsLine(c) : "";
        }

        public static string Take(Commission c)
        {
            if (c == null || c.mine)
                return "";
            string why = WhyNot(c);
            if (why.Length > 0)
                return why;
            c.mine = true;
            c.offered = false;
            return $"You took the order: {Describe(c)}.";
        }

        /// <summary>You refuse an order offered to you: it goes, and nobody thinks the worse of you. (An order you have
        /// taken cannot be refused: it is brought, or its day passes.)</summary>
        public static string Refuse(Commission c)
        {
            if (c == null || c.mine || !c.offered)
                return "";
            All.Remove(c);
            return $"You refused the order at {GameData.Port(c.port)?.name}.";
        }

        // ------------------------------------------------------------------ bringing it

        /// <summary>In port: what is aboard for your orders here, as one line to say on arrival ("" if nothing is).</summary>
        public static string Waiting(PortDef p)
        {
            if (p == null || !Trade.Started)
                return "";
            var told = new List<string>();
            foreach (var c in Mine)
            {
                var good = GameData.Good(c.good);
                int n = good != null && c.port == p.id ? Math.Min(Trade.Hold.Count(c.good), c.Left) : 0;
                if (n > 0)
                    told.Add(Trade.Units(good, n));
            }
            return told.Count == 0 ? "" : $"Aboard for your order here: {string.Join(", ", told)}. Hand them over in the Tasks window.";
        }

        /// <summary>In port: every order of yours for here, as much of each as is aboard, landed and paid. The facts ("" if none).</summary>
        public static string Deliver(PortDef p)
        {
            if (p == null || !Trade.Started)
                return "";
            var told = new List<string>();
            foreach (var c in Mine)
            {
                string fact = Deliver(c, p);
                if (fact.Length > 0)
                    told.Add(fact);
            }
            return string.Join(" ", told);
        }

        /// <summary>In port: as much of this order as is aboard (at most `most` units) landed and paid, less the toll of
        /// the place, as on any sale. The last part closes the order and is a deed kept. The fact ("" if nothing landed).</summary>
        public static string Deliver(Commission c, PortDef p, int most = int.MaxValue)
        {
            var good = c != null ? GameData.Good(c.good) : null;
            if (p == null || good == null || !c.mine || c.port != p.id || !Trade.Started)
                return "";
            int n = Math.Min(Math.Min(Trade.Hold.Count(c.good), c.Left), most);
            if (n <= 0)
                return "";
            var game = KapmanGame.Current;
            int year = game != null ? game.Year : 793;
            double today = game != null ? game.Day : c.posted;
            var zone = ZoneOf(p);
            double total = Math.Round(n * c.each, 2);
            bool free = Chest.TollFreeAt(p, year);
            double toll = free || zone == null ? 0.0 : Math.Round(total * zone.toll * EncounterRunner.TollFactor(p.id, today), 2);
            Trade.Hold.Remove(good, n);
            Trade.Purse.Receive(total - toll, zone, TradeRules.Current, out _);
            c.done += n;
            string paid = toll > 0.0 ? $"{Show(p, total)}, less the toll of {Show(p, toll)}: took {Trade.Purse.LastCoins}" : $"{Show(p, total)}: took {Trade.Purse.LastCoins}";
            if (c.Left > 0)
                return $"Delivered {Trade.Units(good, n)} of the {c.units} ordered, for {paid}. {c.Left} still to bring by {DateOf(c.deadline)}.";
            All.Remove(c);
            Sim.Deeds.Record("kept_commission", $"brought {Trade.Units(good, c.units)} to {p.name} as ordered", p);
            return c.done == n
                ? $"Delivered {Trade.Units(good, n)} as ordered, for {paid}."
                : $"Delivered the last {Trade.Units(good, n)} of the {c.units} ordered, for {paid}. The order is kept.";
        }

        // ------------------------------------------------------------------ the saved game

        public static List<object> Capture() => All.ConvertAll(c => (object)new Dictionary<string, object>
        {
            ["id"] = c.id, ["port"] = c.port, ["good"] = c.good, ["units"] = c.units, ["pay"] = c.pay, ["deadline"] = c.deadline,
            ["posted"] = c.posted, ["mine"] = c.mine, ["each"] = c.each, ["done"] = c.done, ["offered"] = c.offered, ["warned"] = c.warned,
        });

        public static void Restore(List<object> data)
        {
            All.Clear();
            foreach (var x in data ?? new List<object>())
            {
                var c = new Commission
                {
                    id = Json.Str(x, "id"), port = Json.Str(x, "port"), good = Json.Str(x, "good"), units = (int)Json.Num(x, "units"),
                    pay = Json.Num(x, "pay"), deadline = Json.Num(x, "deadline"), posted = Json.Num(x, "posted"), mine = Json.Bool(x, "mine"),
                    done = (int)Json.Num(x, "done"), offered = Json.Bool(x, "offered"), warned = (int)Json.Num(x, "warned"),
                };
                // (a game saved before orders had a price a unit: the whole pay shared out)
                c.each = Json.Num(x, "each", c.units > 0 ? c.pay / c.units : 0.0);
                All.Add(c);
            }
            // (ids go on from the highest saved, so a new order never shares one with an old)
            made = 1000;
            foreach (var c in All)
                if (c.id != null && c.id.Length > 1 && int.TryParse(c.id.Substring(1), out int n) && n > made)
                    made = n;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearState()
        {
            All.Clear();
            made = 0;
        }
    }
}
```

### 4.2 `Assets/Kapman/Code/Game/Encounters/GameEffects.cs`: two edits

Line 25 and the empty line after it go (the counter is not used any more):

```csharp
        int ordersMade;

```

Lines 161-175, between `PriceShock` (ends at line 159) and the comment line `// ---...--- the chest`, today:

```csharp
        public string Commission(string port, string good, int count, double days, double premium)
        {
            var p = GameData.Port(port) ?? Here;
            var g = GameData.Good(good);
            if (p == null || g == null || count <= 0 || days <= 0.0)
                return null;
            var m = Markets.Of(p, Year, Today);
            var mg = m != null ? m.Get(good) : null;
            double each = (mg != null ? m.Pays(mg, Month) : g.value) * (1.0 + premium);
            Commissions.All.Add(new Kapman.Game.Commission
            {
                id = "enc" + (++ordersMade) + "." + p.id + "." + Today.ToString("0.###", CultureInfo.InvariantCulture),
                port = p.id, good = g.id, units = count, pay = Math.Round(each * count), posted = Today, deadline = Today + days,
            });
            return $"An order at {p.name}: {Trade.Units(g, count)} within {days.ToString("0", CultureInfo.InvariantCulture)} days.";
        }
```

become:

```csharp
        /// <summary>A card offers the merchant an order by name (Design/fixes/orders.md): priced by the one rule for all
        /// orders (the market there plus at most a half, at most 60 d over the market in all), kept for the merchant, and
        /// asked at once: take it or refuse it. Taken, it stands in Tasks with its day.</summary>
        public string Commission(string port, string good, int count, double days, double premium)
        {
            var p = GameData.Port(port) ?? Here;
            var g = GameData.Good(good);
            if (p == null || g == null || count <= 0 || days <= 0.0)
                return null;
            var c = Commissions.Offer(p, g, count, days, premium, Year, Month, Today);
            return c == null ? null : $"An order is offered: {Commissions.Describe(c)}. Take it or refuse it.";
        }
```

The effect in the cards keeps its form, `commission <good> <n> by <days> premium <f>`. The premium is what the card
asks; the rule of part 2 decides what is paid.

### 4.3 `Assets/Kapman/Code/Game/UI/Frame/OrderAsk.cs`: a new file

It hooks itself to `Commissions.Offered`; `FrameRoot.cs` is not touched (other builders are in it).

```csharp
using UnityEngine;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// THE ASK FOR AN ORDER (Design/fixes/orders.md). When a card offers you an order, a small floating window comes up
    /// at once with its terms: what, where, by what day, at how much a unit, the toll, how many loads it is for the boat
    /// you have, and what it costs to break it. Two answers: take it, or refuse it. It holds the clock as a card does
    /// (and, like a card, lets the clock run if you set it running). Shut without an answer, the offer waits in the
    /// Tasks window for its seven days.
    /// </summary>
    public static class OrderAsk
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Hook()
        {
            Commissions.Offered -= Ask;
            Commissions.Offered += Ask;
        }

        static string IdOf(Commission c) => "order." + c.id;

        public static void Ask(Commission c)
        {
            var frame = FrameRoot.Current;
            if (frame == null || c == null)
                return;                                                  // (no frame: the offer waits in Tasks)
            var port = GameData.Port(c.port);
            var spec = new FloatSpec { id = IdOf(c), title = "An order", who = port != null ? "At " + port.name : "", face = "", holdsClock = true };
            spec.lines.Add((Cap(Commissions.Describe(c)) + ".", true));
            string loads = Commissions.LoadsLine(c);
            if (loads.Length > 0)
                spec.lines.Add((loads, true));
            double toll = 0.0;
            var game = KapmanGame.Current;
            if (port != null && game != null && TradeRules.Current != null)
            {
                var zone = TradeRules.Current.ZoneOf(port.region);
                toll = zone != null ? zone.toll * EncounterRunner.TollFactor(port.id, game.Day) : 0.0;
            }
            spec.lines.Add(((toll > 0.0 ? $"Each part is paid as it lands, less the toll there ({toll * 100.0:0} in the hundred today). " : "Each part is paid as it lands. ") +
                            "Bring all of it by the day and they will think the better of you. Bring none and they will remember it; bring part and they will remember that a little.", true));
            spec.small = $"Unanswered, the offer stands {Commissions.OfferDays:0} days and then goes to another.";
            string why = Commissions.WhyNot(c);
            spec.options.Add(new FloatOption { label = "Take the order", enabled = why.Length == 0, rule = why, pick = () => Answer(c, true) });
            spec.options.Add(new FloatOption { label = "Refuse it", pick = () => Answer(c, false) });
            frame.OpenFloat(spec);
        }

        static void Answer(Commission c, bool take)
        {
            string fact = take ? Commissions.Take(c) : Commissions.Refuse(c);
            if (!string.IsNullOrEmpty(fact))
                FrameLog.Add(fact);
            FrameRoot.Current?.CloseFloat(IdOf(c));
        }

        static string Cap(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);
    }
}
```

### 4.4 `Assets/Kapman/Code/Game/UI/Frame/Panels/TasksPanel.cs`: the whole file (125 lines today, last changed 7 Oct 10:09)

If the builder of the brief faces has changed this file since, keep his changes and take from here only: the two
added lines of `Stamp` (the hold's size, and `c.done` and `c.offered`), the new block "orders offered to you", and
the blocks "orders taken" and "orders posted here" (lines 70-104 today). The debt block and the rest are as they were.

```csharp
using UnityEngine.UIElements;

namespace Kapman.Game.Frame
{
    /// <summary>
    /// TASKS - what you have taken on: the debt and its day (and paying toward it at a market); the orders offered to you
    /// and not yet answered (take or refuse); the orders you have taken, each with its day, how much of it is brought and
    /// how much is aboard (landed here, in parts if need be, when she lies where it is wanted); the orders posted where
    /// she lies; and the passengers (the game carries none yet, and says so).
    /// </summary>
    public sealed class TasksPanel : FramePanel
    {
        public override string Id => "tasks";
        public override string Title => "Tasks";
        public override int Order => 40;

        string said = "";

        protected override int Stamp()
        {
            var game = KapmanGame.Current;
            var here = FrameGame.PortHere;
            int h = FrameUi.Mix(FrameUi.Mix(17, here != null ? here.id : ""), said);
            if (Trade.Started)
            {
                h = FrameUi.Mix(FrameUi.Mix(h, (int)Trade.Debt), (int)FrameGame.Silver);
                h = FrameUi.Mix(h, game != null ? (int)(Trade.DebtDue - game.Day) : 0);
                h = FrameUi.Mix(h, FrameGame.HoldStamp());
                h = FrameUi.Mix(h, (int)Trade.Hold.capacity);
            }
            foreach (var c in Commissions.All)
            {
                h = FrameUi.Mix(FrameUi.Mix(FrameUi.Mix(h, c.id), c.mine), game != null ? (int)(c.deadline - game.Day) : 0);
                h = FrameUi.Mix(FrameUi.Mix(h, c.done), c.offered);
            }
            return h;
        }

        protected override void Fill(VisualElement body)
        {
            var game = KapmanGame.Current;
            if (game == null || !Trade.Started)
            {
                body.Add(FrameUi.P("Nothing taken on yet."));
                return;
            }
            var here = FrameGame.PortHere;
            if (said.Length > 0)
                body.Add(FrameUi.Text(said));

            // ---- the debt and its day
            body.Add(FrameUi.H("Owed"));
            if (Trade.Debt > 0.01)
            {
                double days = Trade.DebtDue - game.Day;
                string when = days >= 0.0 ? $"Due on {FrameGame.DateOfDay(Trade.DebtDue)}: {days:0} days from now." : $"It fell due on {FrameGame.DateOfDay(Trade.DebtDue)}: {-days:0} days ago.";
                double silver = FrameGame.Silver;
                string pace = silver >= Trade.Debt ? "You have the silver for it." : $"You are short of it by {Trade.Debt - silver:0} d.";
                body.Add(FrameUi.Ln("The debt", when + " " + pace, FrameUi.Num($"{Trade.Debt:0} d")));
                if (here != null)
                {
                    var market = Markets.Of(here, game.Year, game.Day);
                    var part = FrameUi.Btn("Pay 20 d", () => Pay(market, 20.0));
                    var all = FrameUi.Btn("Pay it all", () => Pay(market, Trade.Debt));
                    part.SetEnabled(silver >= System.Math.Min(20.0, Trade.Debt));
                    all.SetEnabled(silver >= Trade.Debt);
                    body.Add(FrameUi.Row(part, all));
                }
                else
                    body.Add(FrameUi.P("It is paid at a market, in port."));
            }
            else
                body.Add(FrameUi.P("You owe nothing."));

            // ---- orders offered to you by name, not yet answered
            var offers = Commissions.OfferedToYou;
            if (offers.Count > 0)
            {
                body.Add(FrameUi.H("Orders offered to you"));
                foreach (var c in offers)
                {
                    var order = c;
                    double stands = c.posted + Commissions.OfferDays - game.Day;
                    string why = Commissions.WhyNot(c);
                    string small = (why.Length > 0 ? why : Commissions.LoadsLine(c)) + $" The offer stands {System.Math.Max(0.0, stands):0} days more.";
                    var take = FrameUi.Btn("Take", () => Say(Commissions.Take(order)), why.Length > 0 ? why : null);
                    take.SetEnabled(why.Length == 0);
                    body.Add(FrameUi.Ln(Cap(Commissions.Describe(c)), small.Trim(), take, FrameUi.Btn("Refuse", () => Say(Commissions.Refuse(order)))));
                }
            }

            // ---- orders taken
            body.Add(FrameUi.H("Orders taken"));
            var mine = Commissions.Mine;
            if (mine.Count == 0)
                body.Add(FrameUi.P("None. A port posts its orders where she lies; some people offer you one themselves."));
            foreach (var c in mine)
            {
                var order = c;
                double left = c.deadline - game.Day;
                int aboard = Trade.Hold.Count(c.good);
                int now = System.Math.Min(aboard, c.Left);
                bool isHere = here != null && c.port == here.id;
                string small = (left >= 0.0 ? $"{left:0} days left. " : "Its day has passed. ") + $"Brought: {c.done} of {c.units}. Aboard: {aboard}.";
                string loads = Commissions.LoadsLine(c);
                if (loads.Length > 0)
                    small += " " + loads;
                if (isHere && now > 0)
                {
                    var market = Markets.Of(here, game.Year, game.Day);
                    double toll = market.zone.toll * EncounterRunner.TollFactor(here.id, game.Day);
                    small += toll > 0.0 ? $" This is the place: the toll here is {toll * 100.0:0} in the hundred." : " This is the place: no toll here.";
                    body.Add(FrameUi.Ln(Cap(Commissions.Describe(c)), small,
                                        FrameUi.Go(now >= c.Left ? $"Hand over {now}" : $"Hand over {now} now", () => Say(Commissions.Deliver(order, here)))));
                }
                else
                    body.Add(FrameUi.Ln(Cap(Commissions.Describe(c)), small + (isHere ? " This is the place, but none of it is aboard." : "")));
            }

            // ---- orders posted here
            if (here != null)
            {
                var posted = Commissions.At(here);
                body.Add(FrameUi.H("Orders posted at " + here.name));
                if (posted.Count == 0)
                    body.Add(FrameUi.P("None posted here now."));
                foreach (var c in posted)
                {
                    var order = c;
                    string why = Commissions.WhyNot(c);
                    string small = $"{c.deadline - game.Day:0} days to bring it. " + (why.Length > 0 ? why : Commissions.LoadsLine(c));
                    var take = FrameUi.Btn("Take", () => Say(Commissions.Take(order)), why.Length > 0 ? why : null);
                    take.SetEnabled(why.Length == 0);
                    body.Add(FrameUi.Ln(Cap(Commissions.Describe(c)), small.Trim(), take));
                }
                if (posted.Count > 0)
                    body.Add(FrameUi.P("Left untaken, another merchant may fill it."));
            }

            // ---- passengers
            body.Add(FrameUi.H("Passengers"));
            body.Add(FrameUi.P("Nobody is carried as a passenger."));
        }

        void Pay(Market market, double deniers)
        {
            Say(Trade.PayDebt(market, deniers).text);
        }

        void Say(string text)
        {
            said = text ?? "";
            FrameLog.Add(said);
            Redraw();
        }

        static string Cap(string s) => string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);
    }
}
```

### 4.5 `Assets/Kapman/Code/Graphs/VoyageNodes.cs`: lines 71-73

Today, inside `Run` of the node "Voyage: In port", after `Voyage.InPort(...)` on line 70:

```csharp
                string delivered = Commissions.Deliver(p.Def);             // (orders for here, from the hold)
                if (delivered.Length > 0)
                    Voyage.Say(delivered);
```

become:

```csharp
                // (7 Oct, Design/fixes/orders.md: an order is handed over by the player, in Tasks, part by part. On
                // arrival she is only told what is aboard for it.)
                string waiting = Commissions.Waiting(p.Def);
                if (waiting.Length > 0)
                    Voyage.Say(waiting);
```

Why: with parts and a toll, when to hand over is the player's to choose (a card may have eased the toll for some
days; the casks may be meant for another order). If the user would sooner have it done on arrival, leave these three
lines as they are: `Commissions.Deliver(PortDef)` still lands everything that is aboard for that port.

## 5. Data edits

1. **`Assets/Kapman/Game/Data/commissions_793.json`.** After the line `"units": ...` (line 3) add:

```json
  "premium_max": 0.5,
  "about_premium": "the most any order pays over the market's price there, as a share of it; a card's order asking more is cut to this",
  "bonus_max_d": 60,
  "about_bonus": "the most a whole order pays over the market, in deniers (ruling 14 of 7 Oct 2026: no card pays over 60 d); 0: no such cap",
  "offer_days": 7,
  "about_offer": "game days an order offered by a card waits for an answer before it goes to another (ruling 12: an ordinary card waits a week)",
  "warn_days": 7,
  "about_warn": "game days before its day that a taken order is called out in the log and the clock drops to slow; again on the last day",
```

   and change the end of `"about"` from "taken or not; delivered when you lie there with the goods aboard" to "taken
   or not; brought in parts or whole when you lie there with the goods aboard, each part paid by the unit and tolled
   like a sale". The four kinds' own premiums (0.25 to 0.4) and days (35 to 45) stand.
2. **`Assets/Kapman/Game/Data/reputation_793.json`.** After the `broke_commission` entry (lines 38-39) add:

```json
    "short_commission": { "frankish": -2, "frisian": -2, "english": -2, "norse": -2, "slavic": -1.5, "baltic": -1.5, "finnic_rus": -1.5, "gaelic": -2,
      "about": "took an order and brought only part of it by the day" },
```

   Until this is in, a short order costs no standing (an unknown deed is worth nothing: `Reputation.cs` 84); nothing
   breaks.
3. **The cards: 126 premiums to 0.5.** Every row marked P in part 7. The code would cut them anyway; the data
   should say what is true, and test `NoCardWritesAnOrderOverAHalf` keeps it so. Do it with this script, saved as
   `Bake/economy/cap_order_premiums.py` (I could not write there). It changes only the number after `premium`. I ran
   it on a copy of the card files: 220 orders seen, 126 cut on 119 lines, every file still loads as JSON, and the
   files are byte for byte the same but for those numbers. Run it after the content pass on the cards has landed:

```python
# cap_order_premiums.py - writes "premium 0.5" into every card order whose premium is over a half.
# Design/fixes/orders.md, rule 3. Run with no argument to see what it would do; with --apply to do it.
# It changes nothing but the number after "premium" inside "commission ..." effects: no other byte of a file moves.
import glob, io, json, os, re, sys

ROOT = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", ".."))
DATA = os.environ.get("KAPMAN_DATA") or os.path.join(ROOT, "Assets", "Kapman", "Game", "Data")
CAP = 0.5
RX = re.compile(r'("commission\s+[a-z_0-9]+\s+\d+\s+by\s+[\d.]+\s+premium\s+)([\d.]+)(\s*")')

def cut_text(text):
    """The text with every order over a half cut to a half; how many orders it holds; how many were cut."""
    count = {"seen": 0, "cut": 0}
    def cut(m):
        count["seen"] += 1
        if float(m.group(2)) <= CAP + 1e-9:
            return m.group(0)
        count["cut"] += 1
        return m.group(1) + "0.5" + m.group(3)
    return RX.sub(cut, text), count["seen"], count["cut"]

def main():
    apply = "--apply" in sys.argv
    files = sorted(glob.glob(os.path.join(DATA, "encounters", "*.json"))) + [os.path.join(DATA, "events_793.json")]
    seen = changed = 0
    for path in files:
        with io.open(path, "r", encoding="utf-8", newline="") as f:      # (newline="": line ends are kept as they are)
            text = f.read()
        new, n, cut = cut_text(text)
        seen += n
        if cut == 0:
            continue
        json.loads(new.lstrip("﻿"))                                  # still a JSON file, or stop here
        changed += cut
        print("%-52s %3d of %3d orders cut to a half" % (os.path.basename(path), cut, n))
        if apply:
            with io.open(path, "w", encoding="utf-8", newline="") as f:
                f.write(new)
    print("%d orders seen, %d over a half%s" % (seen, changed, ": written" if apply else ": nothing written (run with --apply)"))

if __name__ == "__main__":
    main()
```

   `& "C:\Program Files\QGIS 3.44.12\bin\python-qgis-ltr.bat" Bake\economy\cap_order_premiums.py` shows what it would
   do; add `--apply` to do it.
4. **No card needs its units changed for the first release.** With rule 5 an order of two to four loads is honest
   work, and the game says how many loads before the player takes it. 56 card orders are of two to four
   loads of the first boat with 15 game days or more for each load; they stand. 7 are more than that (the
   third table of part 7): none of the 7 can open in the first release (their ports are not among the twelve, or their
   chain begins after 805). Leave them; look again when the later ages' boats are settled.
5. **No words need changing because of rule 1.** The 135 choices that say "take the order" are now followed by the
   ask with the terms; the 85 that do not name the order are made safe by it.

## 6. Tests

`Assets/Kapman/Tests/EditMode/CommissionTests.cs`, the whole file. The first test is the one that is there today,
unchanged, and must still pass. `NoCardWritesAnOrderOverAHalf` fails until data edit 3 is done: it is the gate for
it. Compiled against the drafted assembly; not run.

```csharp
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kapman.Game;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>Orders (Design/fixes/orders.md): places post them by their kind; a card offers one by name and it is taken
    /// or refused; taken, it may be brought in parts, each paid by the unit and tolled; the pay is the market there plus
    /// at most a half and at most 60 d over the market; past its day it is broken or short.</summary>
    public class CommissionTests
    {
        TradeRules rules;

        [SetUp]
        public void Load()
        {
            GameData.LoadGoods(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/goods_793.json"));
            GameData.LoadPorts(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/ports_793.json"));
            rules = TradeRules.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/trade_rules.json"));
            Wording.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/wording_793.json"));
            Reputation.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/reputation_793.json"));
            Commissions.Load(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Kapman/Game/Data/commissions_793.json"));
            Markets.Clear();
            Trade.Begin(rules, 9000f, 0.0);
        }

        [TearDown]
        public void Done()
        {
            Markets.Clear();
            Reputation.Load(null);
            Commissions.Load(null);
        }

        /// <summary>A card's order, as GameEffects.Commission makes it.</summary>
        static Commission Offer(string port, string good, int units, double days = 60.0, double premium = 0.3, double today = 0.0) =>
            Commissions.Offer(GameData.Port(port), GameData.Good(good), units, days, premium, 793, 7, today);

        [Test]
        public void OrdersArePostedKeptAndBroken()
        {
            for (int d = 0; d < 60; d++)
                Commissions.Daily(d, 793, 7, 793);
            Assert.Greater(Commissions.All.Count, 0, "orders posted in two months");
            var c = Commissions.All[0];
            var port = GameData.Port(c.port);
            Assert.IsTrue(port.kind == "monastery" || port.kind == "royal seat" || port.kind == "town" || port.kind == "emporium");
            Commissions.Take(c);
            Assert.AreEqual(1, Commissions.Mine.Count);
            // delivered: paid, gone, and his standing there rises
            double standing = Reputation.At(port), silver = Trade.Purse.Silver(rules);
            Trade.Hold.Add(GameData.Good(c.good), c.units, 0.0, 0.0);
            string fact = Commissions.Deliver(port);
            StringAssert.StartsWith("Delivered", fact);
            Assert.AreEqual(0, Commissions.Mine.Count);
            Assert.Greater(Trade.Purse.Silver(rules), silver, "paid");
            Assert.Greater(Reputation.At(port), standing, "kept: standing up");
            // another taken and let run out: standing down
            var other = Commissions.All.FirstOrDefault(x => !x.mine);
            if (other != null)
            {
                Commissions.Take(other);
                var there = GameData.Port(other.port);
                double before = Reputation.At(there);
                Commissions.Daily(other.deadline + 1.0, 793, 9, 793);
                Assert.IsFalse(Commissions.All.Contains(other), "broken");
                Assert.Less(Reputation.At(there), before, "broken: standing down");
            }
        }

        [Test]
        public void ACardsOrderIsKeptForYouUntilYouAnswer()
        {
            var sarre = GameData.Port("sarre");
            Commission asked = null;
            System.Action<Commission> hear = x => asked = x;
            Commissions.Offered += hear;
            Commission c;
            try
            {
                c = Offer("sarre", "rhenish_wine", 4);
            }
            finally
            {
                Commissions.Offered -= hear;
            }
            Assert.AreSame(c, asked, "the frame is told at once, so it can ask");
            Assert.IsTrue(c.offered);
            Assert.IsFalse(c.mine);
            Assert.AreEqual(0, Commissions.At(sarre).Count, "it is not on the board for other merchants");
            for (int d = 1; d <= 7; d++)
                Commissions.Daily(d, 793, 7, 793);
            Assert.IsTrue(Commissions.All.Contains(c), "nobody else fills it while the offer stands");
            StringAssert.StartsWith("You took the order", Commissions.Take(c));
            Assert.IsTrue(c.mine);
            Assert.IsFalse(c.offered);
            Assert.IsTrue(Commissions.Mine.Contains(c), "taken, it stands among the orders taken");
        }

        [Test]
        public void AnOfferRefusedOrLeftAWeekGoesAndCostsNothing()
        {
            var sarre = GameData.Port("sarre");
            double before = Reputation.At(sarre);
            var refused = Offer("sarre", "wax", 2);
            StringAssert.StartsWith("You refused", Commissions.Refuse(refused));
            Assert.IsFalse(Commissions.All.Contains(refused));
            var left = Offer("sarre", "wax", 2);
            for (int d = 1; d <= 8; d++)
                Commissions.Daily(d, 793, 7, 793);
            Assert.IsFalse(Commissions.All.Contains(left), "seven days, then it goes");
            Assert.AreEqual(before, Reputation.At(sarre), 1e-9, "no order was taken, so none was broken");
            // an order once taken cannot be refused
            var taken = Offer("sarre", "wax", 2, 60.0, 0.3, 8.0);
            Commissions.Take(taken);
            Assert.AreEqual("", Commissions.Refuse(taken));
            Assert.IsTrue(Commissions.All.Contains(taken));
        }

        [Test]
        public void ItPaysTheMarketPlusAtMostAHalf()
        {
            var sarre = GameData.Port("sarre");
            var wax = GameData.Good("wax");
            double market = Commissions.MarketEach(sarre, wax, 793, 7, 0.0);
            Assert.Greater(market, 0.0);
            Assert.AreEqual(market * 1.5, Commissions.PriceEach(sarre, wax, 1, 1.5, 793, 7, 0.0), 0.011, "a premium of 1.5 is cut to a half");
            Assert.AreEqual(market * 1.1, Commissions.PriceEach(sarre, wax, 1, 0.1, 793, 7, 0.0), 0.011, "a small premium stands as written");
        }

        [Test]
        public void TheWholeOrderPaysAtMostSixtyOverTheMarket()
        {
            // the minster's four casks at Sarre (premium 1.5 in the card: about 756 d before the mend)
            var sarre = GameData.Port("sarre");
            var wine = GameData.Good("rhenish_wine");
            double market = Commissions.MarketEach(sarre, wine, 793, 7, 0.0);
            double each = Commissions.PriceEach(sarre, wine, 4, 1.5, 793, 7, 0.0);
            Assert.Greater(market * 4 * 0.5, Commissions.BonusMax, "a half of four casks is more than the cap, or this test proves nothing");
            Assert.AreEqual(Commissions.BonusMax, (each - market) * 4, 0.05);
        }

        [Test]
        public void ARunOnTheStockDoesNotRaiseAnOrder()
        {
            // where the good is made, buying the town's stock out makes today's price dear; the order keeps to the usual one
            var dorestad = GameData.Port("dorestad");
            var wine = GameData.Good("rhenish_wine");
            var m = Markets.Of(dorestad, 793, 0.0);
            var mg = m.Get("rhenish_wine");
            double usual = m.UsualPrice(mg, 7) * (1f - m.Spread);
            mg.stock = 1f;
            Assert.Greater(m.Pays(mg, 7), usual, "today's price is dear");
            Assert.AreEqual(usual, Commissions.MarketEach(dorestad, wine, 793, 7, 0.0), 0.01);
        }

        [Test]
        public void BroughtInPartsEachPartPaidByTheUnitAndTolled()
        {
            Trade.Begin(rules, 660f, 0.0);                                // the first boat: four casks at a time
            var port = GameData.Port("traiectum");
            var wine = GameData.Good("rhenish_wine");
            var c = Offer("traiectum", "rhenish_wine", 6);
            Commissions.Take(c);
            Assert.AreEqual(4, Commissions.FitsAtOnce(c));
            Assert.AreEqual(2, Commissions.Loads(c));
            double standing = Reputation.At(port), silver = Trade.Purse.Silver(rules);
            double toll = rules.ZoneOf(port.region).toll;
            Assert.Greater(toll, 0.0, "a Frankish town takes a toll, or this test proves nothing");

            Trade.Hold.Add(wine, 4, 0.0, 0.0);
            StringAssert.StartsWith("Delivered", Commissions.Deliver(port));
            Assert.AreEqual(4, c.done);
            Assert.AreEqual(2, c.Left);
            Assert.IsTrue(Commissions.Mine.Contains(c), "it stands until the rest is brought");
            Assert.AreEqual(0, Trade.Hold.Count("rhenish_wine"));
            Assert.AreEqual(4 * c.each * (1.0 - toll), Trade.Purse.Silver(rules) - silver, 1.0, "four casks at the price a cask, less the toll");
            Assert.AreEqual(standing, Reputation.At(port), 1e-9, "a part earns no thanks yet");

            silver = Trade.Purse.Silver(rules);
            Trade.Hold.Add(wine, 3, 0.0, 0.0);
            StringAssert.StartsWith("Delivered", Commissions.Deliver(port));
            Assert.AreEqual(1, Trade.Hold.Count("rhenish_wine"), "only what was ordered is taken");
            Assert.AreEqual(2 * c.each * (1.0 - toll), Trade.Purse.Silver(rules) - silver, 1.0);
            Assert.IsFalse(Commissions.All.Contains(c), "all brought: the order is closed");
            Assert.Greater(Reputation.At(port), standing, "kept: standing up");
        }

        [Test]
        public void SheCannotTakeWhatSheCannotCarry()
        {
            Trade.Begin(rules, 660f, 0.0);
            var c = Offer("ripa", "horses", 1);
            Assert.AreEqual(0, Commissions.FitsAtOnce(c));
            Assert.IsNotEmpty(Commissions.WhyNot(c));
            Commissions.Take(c);
            Assert.IsFalse(c.mine, "not taken: one horse is more than her hold");
        }

        [Test]
        public void PastItsDayNothingBroughtIsBrokenAndPartBroughtIsShort()
        {
            var port = GameData.Port("traiectum");
            var wine = GameData.Good("rhenish_wine");
            var a = Offer("traiectum", "rhenish_wine", 4, 30.0);
            Commissions.Take(a);
            double before = Reputation.At(port);
            Commissions.Daily(31.0, 793, 8, 793);
            Assert.IsFalse(Commissions.All.Contains(a));
            double broke = before - Reputation.At(port);
            Assert.Greater(broke, 0.0, "nothing brought: they remember it");

            var b = Offer("traiectum", "rhenish_wine", 4, 30.0, 0.3, 40.0);
            Commissions.Take(b);
            Trade.Hold.Add(wine, 2, 0.0, 40.0);
            Commissions.Deliver(port);
            double silver = Trade.Purse.Silver(rules);
            before = Reputation.At(port);
            Commissions.Daily(71.0, 793, 9, 793);
            Assert.IsFalse(Commissions.All.Contains(b));
            double shortBy = before - Reputation.At(port);
            Assert.Less(shortBy, broke, "part brought costs less than none");
            Assert.AreEqual(silver, Trade.Purse.Silver(rules), 1e-9, "what was landed stays paid");
        }

        [Test]
        public void AWeekBeforeItsDayYouAreToldOnce()
        {
            var said = new List<string>();
            System.Action<string> hear = said.Add;
            Voyage.Notice += hear;
            try
            {
                var c = Offer("traiectum", "rhenish_wine", 4, 30.0);
                Commissions.Take(c);
                Commissions.Daily(22.0, 793, 7, 793);
                Assert.AreEqual(0, said.Count(s => s.Contains("Traiectum")), "eight days left: nothing said yet");
                Commissions.Daily(23.0, 793, 7, 793);
                Commissions.Daily(24.0, 793, 7, 793);
                Assert.AreEqual(1, said.Count(s => s.Contains("days left for the order at Traiectum")), "said once");
                Commissions.Daily(29.0, 793, 7, 793);
                Assert.AreEqual(1, said.Count(s => s.StartsWith("Tomorrow is the day")));
            }
            finally
            {
                Voyage.Notice -= hear;
            }
        }

        [Test]
        public void SavedAndLoadedAndAnOlderSaveRead()
        {
            var port = GameData.Port("traiectum");
            var c = Offer("traiectum", "rhenish_wine", 6);
            Commissions.Take(c);
            Trade.Hold.Add(GameData.Good("rhenish_wine"), 2, 0.0, 0.0);
            Commissions.Deliver(port);
            var waiting = Offer("sarre", "wax", 2);
            Commissions.Restore(Json.Parse(Json.Write(Commissions.Capture())) as List<object>);
            var mine = Commissions.Mine.Single();
            Assert.AreEqual(2, mine.done);
            Assert.AreEqual(c.each, mine.each, 1e-9);
            Assert.AreEqual(1, Commissions.OfferedToYou.Count, "an offer not yet answered is still an offer");
            Assert.AreEqual(waiting.id, Commissions.OfferedToYou[0].id);
            var next = Offer("sarre", "wax", 1);
            Assert.AreEqual(1, Commissions.All.Count(x => x.id == next.id), "a new order never shares an id with a saved one");
            // a save from before the mend: the whole pay and no price a unit
            Commissions.Restore(Json.Parse("[{\"id\":\"c7\",\"port\":\"traiectum\",\"good\":\"rhenish_wine\",\"units\":4,\"pay\":200,\"deadline\":50,\"posted\":10,\"mine\":true}]") as List<object>);
            Assert.AreEqual(50.0, Commissions.Mine.Single().each, 1e-9);
            Assert.AreEqual(0, Commissions.Mine.Single().done);
        }

        /// <summary>The gate for the data edits of Design/fixes/orders.md: no card may write an order over a half.</summary>
        [Test]
        public void NoCardWritesAnOrderOverAHalf()
        {
            var rx = new Regex(@"commission\s+[a-z_0-9]+\s+\d+\s+by\s+[\d.]+\s+premium\s+([\d.]+)");
            var files = Directory.GetFiles("Assets/Kapman/Game/Data/encounters", "*.json").ToList();
            files.Add("Assets/Kapman/Game/Data/events_793.json");
            var over = new List<string>();
            int seen = 0;
            foreach (string path in files)
                foreach (Match m in rx.Matches(File.ReadAllText(path)))
                {
                    seen++;
                    if (double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) > Commissions.PremiumMax + 1e-9)
                        over.Add(Path.GetFileName(path) + ": " + m.Value);
                }
            Assert.Greater(seen, 100, "the cards' orders were found");
            Assert.IsEmpty(over, over.Count + " orders in the cards are written over a half:\n" + string.Join("\n", over.Take(20)));
        }
    }
}
```

What they prove, one line each: a card's order is told to the frame, is not on the board, is not filled by another,
and is mine only when taken; an offer refused or left a week goes and costs nothing, and a taken order cannot be
refused; the price is the market plus at most a half; the whole order pays at most 60 d over the market (the Sarre
case); a run on the stock does not raise it; six casks in a four-cask boat are brought as four and two, each part
paid by the cask less the toll, standing rising only at the end; an order of horses cannot be taken in the first
boat; past its day, none brought costs more standing than part brought, and what was landed stays paid; the warning
is said once at seven days and once on the last; a save and a load keep the parts and the offers, ids stay apart,
and a save from before the mend is read.

To add when the five-speed clock is in: its own test that runs one seed at speed 1 and at speed 5 (ruling 26) should
count the orders too. They hang on the day tick (`KapmanGame.cs` 136-140, the tick ladder raises each day exactly
once however the clock moved), so they should come out the same; I did not run it.

By hand, once, in the game: at Sarre choose "Take the minster's order"; the ask comes up over the card; take it;
Tasks shows it with its day; buy two casks at Dorestat, lie at Sarre, hand over two; the purse rises by two casks
less the toll and the order still stands with "Brought: 2 of 4".

## 7. Every card order that breaks the rules, with the numbers it should have

All 220 card orders (208 choices on 162 cards) break rules 1 and 4 today: none is taken by the
card, none pays toll. Those two are mended in code for all of them and are not listed row by row. The tables list
the 161 that break more:

- **P** (126): the premium is over a half. Write 0.5 (data edit 3).
- **H** (63): more than one hold of the first boat (660 kg of room). No edit: rule 5; the last column says how
  many loads and how many game days there are for each.
- **B** (23): the premium is a half or less, but on that many units it comes to more than 60 d over the
  market. No edit: the code holds the whole order to 60 d over. (Counting the P rows as well, the
  60 d cap is what decides the pay of 103 of the 220 orders.)

How to read the numbers. "Pays today" is units x the market's usual price there x (1 + premium), no toll. "Pays at
most" is under the rules of part 2, before the toll; "of it, over the market" is the part ruling 14 caps; "toll" is
at the share of the place (a tenth in the Frankish realm and, as ruled today, in England; none where silver goes by
weight). The market's usual price is the goods table's value times the made and wanted factors of the port's region,
less half the spread; the season (0.85 to 1.2) and the day's stock are left out, and where other merchants have
filled the market the real figure is lower. "At": the port named in the card's `when`; "+" means the card can open
in more than one port and the first is reckoned; "any" means it opens by people or realm, and the row is reckoned at
the good's bare value (marked ~).

Sums at usual prices, before toll:

| | Orders | Pays today | With the half | With the half and 60 d | The market's part |
|---|---|---|---|---|---|
| All cards | 220 | 87,073 d | 65,166 d | 53,502 d | 44,002 d |
| Over the market | | 43,071 d | 21,164 d | 9,500 d | |
| Can open 793 to 805 on the twelve ports | 68 | 24,696 d | 19,154 d | 16,070 d | 12,868 d |
| Over the market | | 11,828 d | 6,286 d | 3,202 d | |

### 7.1 Can open in the first release (51 of 68 break a rule: P 44, H 18, B 5)

"Can open" is a rough reading: the card's port is one of the twelve and its `when` does not shut out 793 to 805;
chains named for a year after 805 are left out. A chain's later step may still need years the first life does not
have.

| Card · option | File | At | The order today | Pays today (d) | Faults | Premium to write | Pays at most (d) | Of it, over the market (d) | Toll (d) | Loads in the first boat |
|---|---|---|---|---|---|---|---|---|---|---|
| `harbour.chain_black_stones.end_early` · charter | chain_black_stones | lundenwic | `quernstones 6 by 120 premium 1.0` | 181 | P | 0.5 | 136 | 45 | 14 | 1 |
| `harbour.chain_black_stones.end_early` · factor | chain_black_stones | lundenwic | `lead 8 by 150 premium 0.8` | 156 | P | 0.5 | 130 | 43 | 13 | 1 |
| `harbour.chain_black_stones.end_early` · plot | chain_black_stones | lundenwic | `quernstones 6 by 120 premium 1.0` | 181 | P | 0.5 | 136 | 45 | 14 | 1 |
| `harbour.chain_black_stones.end_peace` · both | chain_black_stones | lundenwic | `quernstones 6 by 120 premium 1.0` | 181 | P | 0.5 | 136 | 45 | 14 | 1 |
| `harbour.chain_black_stones.end_peace` · charter | chain_black_stones | lundenwic | `quernstones 6 by 120 premium 1.0` | 181 | P | 0.5 | 136 | 45 | 14 | 1 |
| `harbour.chain_house_debt.s11_friend` · charter | chain_house_debt | dorestad | `rhenish_wine 16 by 365 premium 1.0` | 1123 | PH | 0.5 | 622 | 60 | 62 | 4 (91 d a load) |
| `harbour.chain_house_debt.s11_friend` · dam_head | chain_house_debt | dorestad | `rhenish_wine 12 by 240 premium 0.8` | 758 | PH | 0.5 | 481 | 60 | 48 | 3 (80 d a load) |
| `harbour.chain_house_debt.s11_friend` · seat | chain_house_debt | dorestad | `rhenish_wine 12 by 240 premium 0.8` | 758 | PH | 0.5 | 481 | 60 | 48 | 3 (80 d a load) |
| `harbour.chain_house_debt.s12_enemy` · letter | chain_house_debt | dorestad | `frisian_cloth 10 by 200 premium 0.6` | 415 | P | 0.5 | 319 | 60 | 32 | 1 |
| `harbour.chain_house_debt.s12_enemy` · letter | chain_house_debt | dorestad | `rhenish_wine 8 by 200 premium 0.6` | 449 | PH | 0.5 | 341 | 60 | 34 | 2 (100 d a load) |
| `harbour.chain_house_debt.s12_enemy` · restitution | chain_house_debt | dorestad | `frisian_cloth 10 by 200 premium 0.6` | 415 | P | 0.5 | 319 | 60 | 32 | 1 |
| `harbour.chain_house_debt.s12_enemy` · restitution | chain_house_debt | dorestad | `rhenish_wine 8 by 200 premium 0.6` | 449 | PH | 0.5 | 341 | 60 | 34 | 2 (100 d a load) |
| `harbour.chain_house_debt.s13_st_martin` · school | chain_house_debt | traiectum | `wax 8 by 240 premium 0.6` | 276 | P | 0.5 | 233 | 60 | 23 | 1 |
| `harbour.chain_house_debt.s13_st_martin` · wax_wine | chain_house_debt | traiectum | `rhenish_wine 8 by 240 premium 0.6` | 449 | PH | 0.5 | 341 | 60 | 34 | 2 (120 d a load) |
| `harbour.chain_house_debt.s13_st_martin` · wax_wine | chain_house_debt | traiectum | `wax 8 by 240 premium 0.6` | 276 | P | 0.5 | 233 | 60 | 23 | 1 |
| `harbour.chain_house_heir.s02_hearth` · service | chain_house_heir | ripa | `rhenish_wine 4 by 300 premium 1.5` | 918 | P | 0.5 | 427 | 60 | 0 | 1 |
| `harbour.chain_house_heir.s02_school` · service | chain_house_heir | traiectum | `wax 4 by 300 premium 1.25` | 194 | P | 0.5 | 130 | 43 | 13 | 1 |
| `harbour.chain_house_heir.s02_strand` · service | chain_house_heir | lundenwic | `quernstones 6 by 300 premium 1.5` | 227 | P | 0.5 | 136 | 45 | 14 | 1 |
| `harbour.chain_house_heir.s04_hearth_spring` · thank | chain_house_heir | ripa | `glass_vessels 3 by 240 premium 1.5` | 826 | P | 0.5 | 390 | 60 | 0 | 1 |
| `harbour.chain_house_heir.s04_school_door` · thank | chain_house_heir | traiectum | `wax 6 by 240 premium 1.4` | 311 | P | 0.5 | 190 | 60 | 19 | 1 |
| `harbour.chain_house_heir.s04_strand_tide` · thank | chain_house_heir | lundenwic | `rhenish_wine 4 by 240 premium 1.5` | 756 | P | 0.5 | 362 | 60 | 36 | 1 |
| `harbour.chain_house_heir.s08_two_keels` · strand | chain_house_heir | dorestad | `rhenish_wine 8 by 300 premium 1.5` | 702 | PH | 0.5 | 341 | 60 | 34 | 2 (150 d a load) |
| `harbour.chain_house_marriage.s05_danish_hearth` · cask | chain_house_marriage | ripa | `rhenish_wine 4 by 100 premium 1.6` | 955 | P | 0.5 | 427 | 60 | 0 | 1 |
| `harbour.chain_house_marriage.s05_danish_hearth` · weigh | chain_house_marriage | ripa | `rhenish_wine 4 by 100 premium 1.6` | 955 | P | 0.5 | 427 | 60 | 0 | 1 |
| `harbour.chain_house_marriage.s05_frankish_table` · ask_her | chain_house_marriage | traiectum | `wax 6 by 90 premium 1.4` | 311 | P | 0.5 | 190 | 60 | 19 | 1 |
| `harbour.chain_house_marriage.s05_frankish_table` · letter | chain_house_marriage | traiectum | `wax 6 by 40 premium 1.4` | 311 | P | 0.5 | 190 | 60 | 19 | 1 |
| `harbour.chain_house_marriage.s05_frankish_table` · wax | chain_house_marriage | traiectum | `wax 6 by 60 premium 1.4` | 311 | P | 0.5 | 190 | 60 | 19 | 1 |
| `harbour.chain_house_new_ship.s10_launch` · full | chain_house_new_ship | dorestad | `tar 6 by 180 premium 0.5` | 186 | B | stands | 184 | 60 | 18 | 1 |
| `harbour.chain_house_rival.s07_widow` · store | chain_house_rival | dorestad | `frisian_cloth 8 by 90 premium 0.35` | 280 | B | stands | 267 | 60 | 27 | 1 |
| `harbour.chain_kentish_revolt_796.end_both` · plain | chain_kentish_revolt_796 | lundenwic | `wheat 20 by 90 premium 0.8` | 126 | PH | 0.5 | 105 | 35 | 11 | 3 (30 d a load) |
| `harbour.chain_kentish_revolt_796.end_mercia` · ground | chain_kentish_revolt_796 | sarre+ | `wheat 20 by 120 premium 1.0` | 140 | PH | 0.5 | 105 | 35 | 11 | 3 (40 d a load) |
| `harbour.chain_kentish_revolt_796.s03_new_pennies` · arms | chain_kentish_revolt_796 | fordwic | `spearheads 6 by 150 premium 0.8` | 233 | P | 0.5 | 190 | 60 | 19 | 1 |
| `harbour.chain_kentish_revolt_796.s03_new_pennies` · corn | chain_kentish_revolt_796 | fordwic | `wheat 12 by 120 premium 0.5` | 63 | H | stands | 63 | 21 | 6 | 2 (60 d a load) |
| `harbour.chain_old_gods.s05_dane` · wine | chain_old_gods | ripa+ | `rhenish_wine 6 by 150 premium 0.6` | 881 | PH | 0.5 | 611 | 60 | 0 | 2 (75 d a load) |
| `harbour.chain_pilgrim_road.end_minster` · both | chain_pilgrim_road | sarre | `frankish_wine 6 by 120 premium 0.8` | 826 | PH | 0.5 | 519 | 60 | 52 | 2 (60 d a load) |
| `harbour.chain_pilots_secret.s08_vlie` · old | chain_pilots_secret | medemelacha | `rhenish_wine 4 by 90 premium 0.5` | 454 | B | stands | 362 | 60 | 36 | 1 |
| `harbour.chain_pilots_secret.s08_vlie` · sound | chain_pilots_secret | medemelacha | `rhenish_wine 4 by 90 premium 0.5` | 454 | B | stands | 362 | 60 | 36 | 1 |
| `harbour.chain_storm_vow.end_commuted` · both | chain_storm_vow | doccinga | `rhenish_wine 6 by 300 premium 1.0` | 907 | PH | 0.5 | 514 | 60 | 51 | 2 (150 d a load) |
| `harbour.chain_storm_vow.end_commuted` · both | chain_storm_vow | doccinga | `wax 6 by 300 premium 1.0` | 497 | P | 0.5 | 308 | 60 | 31 | 1 |
| `harbour.chain_storm_vow.end_commuted` · fast | chain_storm_vow | doccinga | `rhenish_wine 6 by 300 premium 1.0` | 907 | PH | 0.5 | 514 | 60 | 51 | 2 (150 d a load) |
| `harbour.chain_storm_vow.end_commuted` · fast | chain_storm_vow | doccinga | `wax 6 by 300 premium 1.0` | 497 | P | 0.5 | 308 | 60 | 31 | 1 |
| `harbour.chain_storm_vow.end_commuted` · light | chain_storm_vow | doccinga | `rhenish_wine 6 by 300 premium 1.0` | 907 | PH | 0.5 | 514 | 60 | 51 | 2 (150 d a load) |
| `harbour.chain_storm_vow.end_commuted` · light | chain_storm_vow | doccinga | `wax 6 by 300 premium 1.0` | 497 | P | 0.5 | 308 | 60 | 31 | 1 |
| `harbour.chain_sword_smuggle.s13_end_laid_down` · foster | chain_sword_smuggle | traiectum | `rhenish_wine 6 by 200 premium 0.6` | 337 | PH | 0.5 | 271 | 60 | 27 | 2 (100 d a load) |
| `harbour.chain_sword_smuggle.s13_end_laid_down` · keep | chain_sword_smuggle | traiectum | `rhenish_wine 6 by 200 premium 0.6` | 337 | PH | 0.5 | 271 | 60 | 27 | 2 (100 d a load) |
| `harbour.blue_for_toki.ripa` · order | danes_norway | ripa | `raw_glass 4 by 300 premium 0.5` | 184 | B | stands | 182 | 60 | 0 | 1 |
| `harbour.black_stones_wanted.lundenwic` · order | england_south_east | lundenwic | `quernstones 8 by 240 premium 1.6` | 314 | P | 0.5 | 181 | 60 | 18 | 1 |
| `harbour.comb_pit.lundenwic` · order | england_south_east | lundenwic | `antler 6 by 300 premium 1.5` | 49 | P | 0.5 | 29 | 10 | 3 | 1 |
| `harbour.herd_of_calves.fordwic` · promise | england_south_east | fordwic | `cattle_hides 6 by 200 premium 1.4` | 224 | P | 0.5 | 140 | 47 | 14 | 1 |
| `harbour.wine_for_the_minster.sarre` · order | england_south_east | sarre | `rhenish_wine 4 by 240 premium 1.5` | 756 | P | 0.5 | 362 | 60 | 36 | 1 |
| `harbour.breaking_beach.dorestad` · oak | rhine_frisia | dorestad | `oak_timber 4 by 120 premium 0.3` | 197 | H | stands | 197 | 45 | 20 | 4 (30 d a load) |


### 7.2 The rest (110)

| Card · option | File | At | The order today | Pays today (d) | Faults | Premium to write | Pays at most (d) | Of it, over the market (d) | Toll (d) | Loads in the first boat |
|---|---|---|---|---|---|---|---|---|---|---|
| `harbour.ash.aldeigjuborg` · timber | baltic_east | aldeigjuborg | `oak_timber 3 by 150 premium 0.8` | 87 | PH | 0.5 | 73 | 24 | 7 | 3 (50 d a load) |
| `harbour.holmfasts_door.birka` · order | baltic_east | birka | `glass_vessels 2 by 365 premium 0.4` | 308 | B | stands | 280 | 60 | 0 | 1 |
| `harbour.vigfasts_word.birka` · victual | baltic_east | birka | `salt_pork 6 by 90 premium 0.6` | 73 | P | 0.5 | 68 | 23 | 0 | 1 |
| `harbour.chain_amber_road.end_ale` · brewhouse | chain_amber_road | truso | `malt 20 by 240 premium 1.0` | 130 | PH | 0.5 | 97 | 32 | 0 | 2 (120 d a load) |
| `harbour.chain_amber_road.s06_steward` · ale | chain_amber_road | truso | `malt 6 by 80 premium 1.0` | 39 | P | 0.5 | 29 | 10 | 0 | 1 |
| `harbour.chain_amber_road.s06_steward` · horse | chain_amber_road | truso | `horses 1 by 80 premium 1.0` | 130 | PH | 0.5 | 97 | 32 | 0 | cannot carry one |
| `harbour.chain_barrow.s11_end_church` · freight | chain_barrow | dommoc | `frankish_wine 4 by 180 premium 0.5` | 378 | B | stands | 312 | 60 | 31 | 1 |
| `harbour.chain_barrow.s11_end_church` · gear | chain_barrow | dommoc | `frankish_wine 4 by 180 premium 0.5` | 378 | B | stands | 312 | 60 | 31 | 1 |
| `harbour.chain_barrow.s11_end_church` · sack | chain_barrow | dommoc | `frankish_wine 4 by 180 premium 0.5` | 378 | B | stands | 312 | 60 | 31 | 1 |
| `harbour.chain_cnut_1016.s03_one_penny` · wine | chain_cnut_1016 | ripa | `rhenish_wine 6 by 240 premium 1.5` | 1377 | PH | 0.5 | 611 | 60 | 0 | 2 (120 d a load) |
| `harbour.chain_coast_watch_810.s08_host` · bread | chain_coast_watch_810 | dorestad+ | `ships_bread 8 by 40 premium 0.8` | 31 | P | 0.5 | 26 | 9 | 3 | 1 |
| `harbour.chain_coast_watch_810.s11_light` · pitch | chain_coast_watch_810 | quentovic | `pitch 6 by 45 premium 0.7` | 83 | P | 0.5 | 73 | 24 | 7 | 1 |
| `harbour.chain_dorestad_834.end_ashes` · head | chain_dorestad_834 | dorestad | `oak_timber 10 by 120 premium 0.5` | 567 | HB | stands | 438 | 60 | 44 | 10 (12 d a load) |
| `harbour.chain_dorestad_834.end_ashes` · yard | chain_dorestad_834 | dorestad | `oak_timber 10 by 120 premium 0.5` | 567 | HB | stands | 438 | 60 | 44 | 10 (12 d a load) |
| `harbour.chain_dorestad_834.s06_homecoming` · share | chain_dorestad_834 | dorestad | `rhenish_wine 10 by 120 premium 0.5` | 526 | HB | stands | 411 | 60 | 41 | 3 (40 d a load) |
| `harbour.chain_great_army_865.end_line` · bond | chain_great_army_865 | lundenwic | `wool_cloth 12 by 240 premium 1.7` | 341 | P | 0.5 | 186 | 60 | 19 | 1 |
| `harbour.chain_great_army_865.end_line` · surety | chain_great_army_865 | lundenwic | `wool_cloth 12 by 240 premium 1.7` | 341 | P | 0.5 | 186 | 60 | 19 | 1 |
| `harbour.chain_great_army_865.s02_horses` · iron | chain_great_army_865 | gipeswic+ | `bar_iron 6 by 150 premium 1.6` | 161 | P | 0.5 | 93 | 31 | 9 | 1 |
| `harbour.chain_house_marriage.s05_english_uncle` · church | chain_house_marriage | eoforwic | `rhenish_wine 4 by 120 premium 1.6` | 786 | P | 0.5 | 362 | 60 | 36 | 1 |
| `harbour.chain_house_marriage.s05_english_uncle` · honest | chain_house_marriage | eoforwic | `rhenish_wine 6 by 120 premium 1.6` | 1179 | PH | 0.5 | 514 | 60 | 51 | 2 (60 d a load) |
| `harbour.chain_house_marriage.s05_english_uncle` · plain | chain_house_marriage | eoforwic | `rhenish_wine 4 by 90 premium 1.6` | 786 | P | 0.5 | 362 | 60 | 36 | 1 |
| `harbour.chain_house_marriage.s05_frisian_mound` · cloth | chain_house_marriage | wijnaldum | `rhenish_wine 4 by 75 premium 1.5` | 756 | P | 0.5 | 362 | 60 | 36 | 1 |
| `harbour.chain_house_marriage.s05_frisian_mound` · plain | chain_house_marriage | wijnaldum | `rhenish_wine 4 by 75 premium 1.5` | 756 | P | 0.5 | 362 | 60 | 36 | 1 |
| `harbour.chain_house_marriage.s05_frisian_mound` · stones | chain_house_marriage | wijnaldum | `rhenish_wine 4 by 75 premium 1.5` | 756 | P | 0.5 | 362 | 60 | 36 | 1 |
| `harbour.chain_house_marriage.s05_frisian_mound` · tally | chain_house_marriage | wijnaldum | `rhenish_wine 4 by 60 premium 1.5` | 756 | P | 0.5 | 362 | 60 | 36 | 1 |
| `harbour.chain_house_new_ship.s06_sail` · gift | chain_house_new_ship | wijnaldum | `rhenish_wine 4 by 150 premium 0.5` | 454 | B | stands | 362 | 60 | 36 | 1 |
| `harbour.chain_house_new_ship.s13_river` · carry | chain_house_new_ship | colonia | `frisian_cloth 16 by 150 premium 0.6` | 1548 | P | 0.5 | 1028 | 60 | 103 | 1 |
| `harbour.chain_lindisfarne_book.end_home` · peace | chain_lindisfarne_book | lindisfarena | `rhenish_wine 6 by 300 premium 1.8` | 1270 | PH | 0.5 | 514 | 60 | 51 | 2 (150 d a load) |
| `harbour.chain_lindisfarne_book.s02_what_is_gone` · order | chain_lindisfarne_book | lindisfarena | `rhenish_wine 4 by 240 premium 1.6` | 786 | P | 0.5 | 362 | 60 | 36 | 1 |
| `harbour.chain_lindisfarne_book.s03_calves` · order | chain_lindisfarne_book | eoforwic | `pigments 2 by 300 premium 1.7` | 327 | P | 0.5 | 181 | 60 | 18 | 1 |
| `harbour.chain_lost_brother.s06_yard` · alms | chain_lost_brother | mogontia | `wax 4 by 300 premium 1.0` | 490 | P | 0.5 | 305 | 60 | 30 | 1 |
| `harbour.chain_lost_brother.s08_booth` · buy | chain_lost_brother | birka | `frisian_cloth 6 by 365 premium 1.4` | 1058 | P | 0.5 | 501 | 60 | 0 | 1 |
| `harbour.chain_lost_brother.s08_booth` · tally | chain_lost_brother | birka | `frisian_cloth 6 by 365 premium 1.4` | 1058 | P | 0.5 | 501 | 60 | 0 | 1 |
| `harbour.chain_lost_brother.s08_booth` · wine | chain_lost_brother | birka | `frisian_cloth 6 by 365 premium 1.4` | 1058 | P | 0.5 | 501 | 60 | 0 | 1 |
| `harbour.chain_maldon_991.end_london` · river | chain_maldon_991 | lundenwic | `frankish_wine 12 by 200 premium 0.6` | 1469 | PH | 0.5 | 978 | 60 | 98 | 3 (66 d a load) |
| `harbour.chain_maldon_991.s07_pennies_north` · order | chain_maldon_991 | sliesthorp | `rhenish_wine 8 by 240 premium 0.8` | 1322 | PH | 0.5 | 794 | 60 | 0 | 2 (120 d a load) |
| `harbour.chain_maldon_991.s08_billingsgate` · shares | chain_maldon_991 | lundenwic | `frankish_wine 10 by 200 premium 0.6` | 1224 | PH | 0.5 | 825 | 60 | 82 | 3 (66 d a load) |
| `harbour.chain_mission_north.s05_church` · books | chain_mission_north | birka | `rhenish_wine 4 by 300 premium 1.5` | 918 | P | 0.5 | 427 | 60 | 0 | 1 |
| `harbour.chain_mission_north.s05_church` · order | chain_mission_north | birka | `rhenish_wine 4 by 300 premium 1.5` | 918 | P | 0.5 | 427 | 60 | 0 | 1 |
| `harbour.chain_mission_north.s05_church` · passage | chain_mission_north | birka | `rhenish_wine 4 by 300 premium 1.5` | 918 | P | 0.5 | 427 | 60 | 0 | 1 |
| `harbour.chain_mission_north.s05_church` · scribe | chain_mission_north | birka | `rhenish_wine 4 by 300 premium 1.5` | 918 | P | 0.5 | 427 | 60 | 0 | 1 |
| `harbour.chain_mission_north.s13_thing` · bell | chain_mission_north | birka | `rhenish_wine 8 by 365 premium 1.5` | 1836 | PH | 0.5 | 794 | 60 | 0 | 2 (182 d a load) |
| `harbour.chain_mission_north.s13_thing` · witness | chain_mission_north | birka | `rhenish_wine 6 by 365 premium 1.5` | 1377 | PH | 0.5 | 611 | 60 | 0 | 2 (182 d a load) |
| `harbour.chain_mission_north.s14_against` · elder | chain_mission_north | birka | `rhenish_wine 6 by 365 premium 1.5` | 1377 | PH | 0.5 | 611 | 60 | 0 | 2 (182 d a load) |
| `harbour.chain_pilgrim_road.s06_two_men` · give | chain_pilgrim_road | dofras | `frankish_wine 2 by 90 premium 0.5` | 230 | B | stands | 213 | 60 | 21 | 1 |
| `harbour.chain_relic_theft.s11_end_saint_bavo` · palace | chain_relic_theft | ganda | `wax 6 by 200 premium 0.5` | 194 | B | stands | 190 | 60 | 19 | 1 |
| `harbour.chain_relic_theft.s11_end_saint_bavo` · plot | chain_relic_theft | ganda | `wax 6 by 200 premium 0.5` | 194 | B | stands | 190 | 60 | 19 | 1 |
| `harbour.chain_relic_theft.s12_end_soissons` · fair | chain_relic_theft | parisius | `rhenish_wine 3 by 240 premium 0.6` | 156 | P | 0.5 | 146 | 49 | 15 | 1 |
| `harbour.chain_rollo_911.s04_blades` · grain | chain_rollo_911 | rotomagus | `wheat 6 by 180 premium 1.6` | 42 | P | 0.5 | 24 | 8 | 2 | 1 |
| `harbour.chain_rorik_850.s04_kings_sons` · victual | chain_rorik_850 | dorestad | `ships_bread 30 by 30 premium 1.8` | 181 | PH | 0.5 | 97 | 32 | 10 | 2 (15 d a load) |
| `harbour.chain_saga_danes_norway.s03_hall_feast` · order | chain_saga_danes_norway | hleidr | `rope 8 by 110 premium 1.6` | 112 | P | 0.5 | 65 | 22 | 0 | 1 |
| `harbour.chain_saga_england_south_east.s02_creek` · sell | chain_saga_england_south_east | sarre | `wheat 6 by 30 premium 2.0` | 63 | P | 0.5 | 32 | 11 | 3 | 1 |
| `harbour.chain_saga_england_south_east.s05_ships_wanted` · supply | chain_saga_england_south_east | hamwic+ | `tar 4 by 40 premium 0.8` | 149 | P | 0.5 | 124 | 41 | 12 | 1 |
| `harbour.chain_saga_england_south_east.s09_huda` · ale | chain_saga_england_south_east | lundenwic | `ale 6 by 40 premium 0.6` | 52 | PH | 0.5 | 49 | 16 | 5 | 2 (20 d a load) |
| `harbour.chain_saga_neustria_west.s01_wall` · stakes | chain_saga_neustria_west | herio+ | `oak_timber 4 by 150 premium 0.4` | 91 | H | stands | 91 | 26 | 9 | 4 (37 d a load) |
| `harbour.chain_saga_northumbria_irish_sea.end_ireland` · cellar | chain_saga_northumbria_irish_sea | hi+ | `rhenish_wine 12 by 365 premium 0.8` | 700 | PH | 0.5 | 449 | 60 | 0 | 3 (121 d a load) |
| `harbour.chain_saga_northumbria_irish_sea.end_oswigesdune` · peace | chain_saga_northumbria_irish_sea | gyrwe+ | `fleeces 12 by 240 premium 0.5` | 78 | H | stands | 78 | 26 | 8 | 2 (120 d a load) |
| `harbour.chain_saga_rhine_frisia.s02_east` · querns | chain_saga_rhine_frisia | emuthon | `quernstones 4 by 150 premium 1.4` | 176 | P | 0.5 | 110 | 37 | 11 | 1 |
| `harbour.chain_saga_rhine_frisia.s10_given_back` · remembered | chain_saga_rhine_frisia | emuthon | `iron_tools 6 by 150 premium 1.5` | 243 | P | 0.5 | 146 | 49 | 15 | 1 |
| `harbour.chain_saga_rhine_frisia.s10_given_back` · trade | chain_saga_rhine_frisia | emuthon | `iron_tools 6 by 150 premium 1.5` | 243 | P | 0.5 | 146 | 49 | 15 | 1 |
| `harbour.chain_saga_rhineland_flanders.s03_ransom_or_gates` · gates | chain_saga_rhineland_flanders | colonia | `oak_timber 8 by 90 premium 1.6` | 337 | PH | 0.5 | 190 | 60 | 19 | 8 (11 d a load) |
| `harbour.chain_saga_rhineland_flanders.s05_ditch_outside_the_wall` · wall | chain_saga_rhineland_flanders | mogontia | `oak_timber 10 by 120 premium 1.7` | 437 | PH | 0.5 | 222 | 60 | 22 | 10 (12 d a load) |
| `harbour.chain_saga_rhineland_flanders.s07_gates_bars_and_locks` · order | chain_saga_rhineland_flanders | colonia | `rye 24 by 150 premium 1.8` | 145 | PH | 0.5 | 78 | 26 | 8 | 3 (50 d a load) |
| `harbour.chain_saga_rhineland_flanders.s08_winter_fort` · feed | chain_saga_rhineland_flanders | diusburg | `rye 20 by 75 premium 2.0` | 130 | PH | 0.5 | 65 | 22 | 6 | 3 (25 d a load) |
| `harbour.chain_saga_rhineland_flanders.s13_end_burnt_well` · purse | chain_saga_rhineland_flanders | diusburg | `oak_timber 10 by 120 premium 1.6` | 421 | PH | 0.5 | 222 | 60 | 22 | 10 (12 d a load) |
| `harbour.chain_storm_vow.end_saint` · candle | chain_storm_vow | lindisfarena | `rhenish_wine 6 by 240 premium 1.0` | 907 | PH | 0.5 | 514 | 60 | 51 | 2 (120 d a load) |
| `harbour.chain_storm_vow.end_saint` · candle | chain_storm_vow | lindisfarena | `wax 6 by 240 premium 1.0` | 605 | P | 0.5 | 362 | 60 | 36 | 1 |
| `harbour.chain_storm_vow.end_saint` · silk | chain_storm_vow | lindisfarena | `rhenish_wine 6 by 240 premium 1.0` | 907 | PH | 0.5 | 514 | 60 | 51 | 2 (120 d a load) |
| `harbour.chain_storm_vow.end_saint` · silk | chain_storm_vow | lindisfarena | `wax 6 by 240 premium 1.0` | 605 | P | 0.5 | 362 | 60 | 36 | 1 |
| `harbour.chain_storm_vow.end_saint` · silver | chain_storm_vow | lindisfarena | `rhenish_wine 6 by 240 premium 1.0` | 907 | PH | 0.5 | 514 | 60 | 51 | 2 (120 d a load) |
| `harbour.chain_storm_vow.end_saint` · wine | chain_storm_vow | lindisfarena | `rhenish_wine 6 by 240 premium 1.0` | 907 | PH | 0.5 | 514 | 60 | 51 | 2 (120 d a load) |
| `harbour.chain_the_bell.s01_empty_frame` · nopurse | chain_the_bell | sliesthorp | `rhenish_wine 4 by 300 premium 1.5` | 918 | P | 0.5 | 427 | 60 | 0 | 1 |
| `harbour.chain_the_bell.s03_founder` · bring | chain_the_bell | colonia | `tin 3 by 240 premium 1.8` | 423 | P | 0.5 | 211 | 60 | 21 | 1 |
| `harbour.chain_the_bell.s12_ripa` · give | chain_the_bell | ripa | `rhenish_wine 6 by 300 premium 1.8` | 1542 | PH | 0.5 | 611 | 60 | 0 | 2 (150 d a load) |
| `harbour.chain_tiel_1020.end_seat` · guest | chain_tiel_1020 | noviomagus | `raw_wool 40 by 200 premium 1.0` | 346 | PH | 0.5 | 233 | 60 | 23 | 6 (33 d a load) |
| `harbour.chain_tiel_1020.s03_emperors_men` · pepper | chain_tiel_1020 | lundenwic | `pepper 10 by 150 premium 0.6` | 968 | P | 0.5 | 665 | 60 | 66 | 1 |
| `harbour.chain_tiel_1020.s07_cologne` · order | chain_tiel_1020 | colonia | `rhenish_wine 12 by 120 premium 0.5` | 486 | HB | stands | 384 | 60 | 38 | 3 (40 d a load) |
| `harbour.chain_walrus_ivory.s04_reeve` · own | chain_walrus_ivory | lundenwic | `walrus_ivory 4 by 400 premium 1.4` | 1322 | P | 0.5 | 611 | 60 | 61 | 1 |
| `harbour.chain_walrus_ivory.s12_altar` · yearly | chain_walrus_ivory | colonia | `walrus_ivory 10 by 400 premium 1.5` | 3443 | P | 0.5 | 1437 | 60 | 144 | 1 |
| `harbour.chain_walrus_ivory.s13_kings_share` · kings_man | chain_walrus_ivory | ogvaldsnes | `walrus_ivory 8 by 400 premium 1.4` | 933 | P | 0.5 | 449 | 60 | 0 | 1 |
| `harbour.chain_wreckers.s06_the_abbess` · writ | chain_wreckers | sarre | `rhenish_wine 4 by 240 premium 1.6` | 786 | P | 0.5 | 362 | 60 | 36 | 1 |
| `harbour.chain_year_1066.s03_wight` · order | chain_year_1066 | lundenwic | `wheat 30 by 60 premium 0.4` | 147 | H | stands | 147 | 42 | 15 | 4 (15 d a load) |
| `harbour.chain_year_1066.s04_rouen` · stores | chain_year_1066 | rotomagus | `rope 20 by 60 premium 0.6` | 173 | P | 0.5 | 162 | 54 | 16 | 1 |
| `harbour.chain_year_1066.s09_dover` · supply | chain_year_1066 | dofras+ | `flour 20 by 40 premium 0.6` | 138 | PH | 0.5 | 130 | 43 | 13 | 2 (20 d a load) |
| `harbour.chain_york_954.end_town` · plot | chain_york_954 | eoforwic | `rhenish_wine 10 by 240 premium 0.6` | 1210 | PH | 0.5 | 816 | 60 | 82 | 3 (80 d a load) |
| `harbour.chain_york_954.s02_three_doors` · hall | chain_york_954 | eoforwic | `rhenish_wine 6 by 120 premium 0.5` | 680 | HB | stands | 514 | 60 | 51 | 2 (60 d a load) |
| `harbour.a_mans_grip.skiringssal` · order | danes_norway | skiringssal | `sword_blades 4 by 400 premium 1.5` | 324 | P | 0.5 | 190 | 60 | 0 | 1 |
| `harbour.chair_and_birds.hleidr` · order | danes_norway | hleidr | `quicksilver 1 by 400 premium 1.0` | 65 | P | 0.5 | 49 | 16 | 0 | 1 |
| `harbour.eight_feet.ahus` · answer | danes_norway | ahus | `madder 2 by 300 premium 0.6` | 21 | P | 0.5 | 19 | 6 | 0 | 1 |
| `harbour.four_posts.ripa` · order | danes_norway | ripa | `oak_timber 4 by 300 premium 0.4` | 174 | H | stands | 174 | 50 | 0 | 4 (75 d a load) |
| `harbour.glass_for_the_mound.ogvaldsnes` · promise | danes_norway | ogvaldsnes | `glass_vessels 1 by 400 premium 1.0` | 78 | P | 0.5 | 58 | 19 | 0 | 1 |
| `harbour.herring_shore.ahus` · order | danes_norway | ahus | `sea_salt 8 by 360 premium 0.6` | 21 | P | 0.5 | 19 | 6 | 0 | 1 |
| `harbour.rivet_bank.ahus` · order | danes_norway | ahus | `oak_timber 3 by 300 premium 0.5` | 73 | H | stands | 73 | 24 | 0 | 3 (100 d a load) |
| `harbour.second_ship.ogvaldsnes` · order | danes_norway | ogvaldsnes | `oak_timber 4 by 400 premium 0.6` | 112 | PH | 0.5 | 105 | 35 | 0 | 4 (100 d a load) |
| `harbour.stewards_tally.hleidr` · querns | danes_norway | hleidr | `quernstones 4 by 300 premium 0.6` | 97 | P | 0.5 | 91 | 30 | 0 | 1 |
| `harbour.kings_new_church.lundenwic` · order | england_south_east | lundenwic | `lead 6 by 240 premium 1.5` | 162 | P | 0.5 | 97 | 32 | 10 | 1 |
| `long.cnut.fleet_paid_off` · order | long | any | `frisian_cloth 3 by 90 premium 0.5` | ~194 | B | stands | ~190 | 60 | 19 | 1 |
| `long.cnut.fleet_paid_off` · order | long | any | `rhenish_wine 4 by 90 premium 0.5` | ~324 | B | stands | ~276 | 60 | 28 | 1 |
| `long.england.great_hunger` · corn | long | any | `wheat 8 by 40 premium 0.7` | ~73 | P | 0.5 | ~65 | 22 | 6 | 1 |
| `long.frisia.flood_838` · corn | long | any | `wheat 6 by 50 premium 0.6` | ~52 | P | 0.5 | ~49 | 16 | 5 | 1 |
| `long.frisia.flood_838` · timber | long | any | `oak_timber 2 by 70 premium 0.5` | ~81 | H | stands | ~81 | 27 | 8 | 2 (35 d a load) |
| `long.offa_letter.796` · stones | long | any | `quernstones 4 by 120 premium 0.6` | ~69 | P | 0.5 | ~65 | 22 | 6 | 1 |
| `long.tiel.burned_1006` · timber | long | any | `oak_timber 3 by 80 premium 0.5` | ~122 | H | stands | ~122 | 40 | 12 | 3 (26 d a load) |
| `harbour.chest_for_kells.hi` · timber | northumbria_irish_sea | hi | `oak_timber 2 by 240 premium 0.5` | 49 | H | stands | 49 | 16 | 0 | 2 (120 d a load) |
| `harbour.king_has_heard.eoforwic` · custom | northumbria_irish_sea | eoforwic | `rhenish_wine 4 by 300 premium 0.4` | 423 | B | stands | 362 | 60 | 36 | 1 |
| `harbour.quarrel_in_a_letter.eoforwic` · wine | northumbria_irish_sea | eoforwic | `rhenish_wine 3 by 240 premium 0.3` | 295 | B | stands | 287 | 60 | 29 | 1 |
| `harbour.silver_on_a_cloak.eoforwic` · service | northumbria_irish_sea | eoforwic | `rhenish_wine 6 by 365 premium 0.1` | 499 | H | stands | 499 | 45 | 50 | 2 (182 d a load) |
| `harbour.kings_cellar.noviomagus` · wine | rhine_frisia | noviomagus | `rhenish_wine 6 by 90 premium 0.3` | 274 | HB | stands | 271 | 60 | 27 | 2 (45 d a load) |
| `harbour.andoverpis.stag_heads` · order | rhineland_flanders | andoverpis | `antler 3 by 200 premium 0.6` | 16 | P | 0.5 | 15 | 5 | 1 | 1 |
| `harbour.ganda.emperors_ships` · order | rhineland_flanders | ganda | `oak_timber 4 by 150 premium 0.5` | 97 | H | stands | 97 | 32 | 10 | 4 (37 d a load) |


### 7.3 More than four loads of the first boat, or one unit will not go in (7; no edit now, see data edit 4)

| Card · option | File | At | The order today | Pays today (d) | Faults | Premium to write | Pays at most (d) | Of it, over the market (d) | Toll (d) | Loads in the first boat |
|---|---|---|---|---|---|---|---|---|---|---|
| `harbour.chain_amber_road.s06_steward` · horse | chain_amber_road | truso | `horses 1 by 80 premium 1.0` | 130 | PH | 0.5 | 97 | 32 | 0 | cannot carry one |
| `harbour.chain_dorestad_834.end_ashes` · head | chain_dorestad_834 | dorestad | `oak_timber 10 by 120 premium 0.5` | 567 | HB | stands | 438 | 60 | 44 | 10 (12 d a load) |
| `harbour.chain_dorestad_834.end_ashes` · yard | chain_dorestad_834 | dorestad | `oak_timber 10 by 120 premium 0.5` | 567 | HB | stands | 438 | 60 | 44 | 10 (12 d a load) |
| `harbour.chain_saga_rhineland_flanders.s03_ransom_or_gates` · gates | chain_saga_rhineland_flanders | colonia | `oak_timber 8 by 90 premium 1.6` | 337 | PH | 0.5 | 190 | 60 | 19 | 8 (11 d a load) |
| `harbour.chain_saga_rhineland_flanders.s05_ditch_outside_the_wall` · wall | chain_saga_rhineland_flanders | mogontia | `oak_timber 10 by 120 premium 1.7` | 437 | PH | 0.5 | 222 | 60 | 22 | 10 (12 d a load) |
| `harbour.chain_saga_rhineland_flanders.s13_end_burnt_well` · purse | chain_saga_rhineland_flanders | diusburg | `oak_timber 10 by 120 premium 1.6` | 421 | PH | 0.5 | 222 | 60 | 22 | 10 (12 d a load) |
| `harbour.chain_tiel_1020.end_seat` · guest | chain_tiel_1020 | noviomagus | `raw_wool 40 by 200 premium 1.0` | 346 | PH | 0.5 | 233 | 60 | 23 | 6 (33 d a load) |


## 8. The new clock (ruling 26)

Nothing in the orders counts real time, so the five speeds change no rule here: the days of an order, the seven days
of an offer and the warnings are game days, and the day's work runs once for each game day whatever the speed. What
the new clock does change is how it feels, and the rules above are written for that:

- A 30-day order is seven and a half real minutes at speed 4 and two minutes at speed 5. The old code said nothing
  until the order was already broken. Rule 7 (a week's warning and a last-day warning, each dropping the clock) is
  there for this. The draft calls `GameSpeed.Slow()` (`Game/UI/GameSpeed.cs` 99), which today means "back to 1x".
  To the builder of the five speeds: if the drop-back gets a reason and a switch in the settings, give this one the
  reason "an order's day"; the call is one line in `Commissions.Warn`.
- The offer's seven days are 28 seconds at speed 5. The ask holds the clock when it opens, as a card does, so the
  player sees it; if he sets the clock running and leaves it, it goes after its week like any ordinary card.
- The days written in `commissions_793.json` (35 to 45) and in the cards (30 to 400) were set when the clock was
  slow. They are sailing days, not waiting time, so they stand: legs in the first release take one to five game days.
- A town or an emporium posts an order about once in three game months (0.3 a month, and one at a time); in the twelve
  ports eight places post, so at speed 4 a new posted order somewhere about every three real minutes. They raise no
  window: they are seen in Tasks where she lies, and in the talk of the waterfront.

## 9. Choices I made that the user may overrule

1. **The 60 d cap on what an order pays over the market.** My reading of ruling 14. The task gave me "the market's
   price there plus at most a half"; with only that, the Sarre order pays 454 d and the 220 orders 21,164
   d over the market, 6,286 d of it in the first release, where the money study says a laden leg makes 13 d.
   With the cap it is 9,500 d and 3,202 d. To turn it off: `"bonus_max_d": 0`.
2. **The market's price is the lower of today's and the usual.** So an order in a famine pays less than the famine
   market while the famine lasts (it is worth bringing later, as the price falls back), and an order where other
   merchants have glutted the market is reckoned from the glutted price. The other choice, today's price whatever it
   is, lets a run on the stock set an order's price.
3. **Every card order is asked once, even where the choice already said "take the order".** One more press for 135
   choices; in return the terms are seen before the promise binds, and the 85 choices that give an order without
   naming it bind nobody unasked.
4. **No late delivery and no handing back.** A promise with a day. The softer rule (a week's grace at the market's
   price, no thanks) is three lines in `Commissions.Daily` if wanted.
5. **Handing over is the player's act** (4.5). Leave `VoyageNodes.cs` alone to keep it on arrival.
6. **A short order costs 2 where a broken one costs 5**, whatever share was brought.

## 10. What I could not check

- The tests compile; they have not been run. The window of 4.3 and the panel of 4.4 have not been seen on a screen.
  `FloatSpec` was read, not driven: I take `who` to be the line under the title and `rule` to be shown on a choice
  that cannot be taken.
- Whether the node "Voyage: In port" runs on the chart (the graphs are assets; I did not open them). If it does not,
  4.5 changes nothing and the Tasks button is the only way to hand over, which is what is wanted.
- The numbers of part 7 are at usual prices with no season, no stock and no other ships. They are upper figures for
  "pays at most" and rough ones for "pays today".
- Whether any card's written lines name the silver its order pays (so that a cut premium would make the words
  untrue). I checked the choices' labels: four name a sum, and all four are prices paid or taken on the spot, not
  the order's pay. The lines in `Data/writing/*.json` I did not search.
- The chain cap of 240 d (ruling 14) should count up to 60 d for each order a chain gives. `Bake/economy/
  cap_payouts.py` counts only `silver +N`. Twelve choices give two orders at once (up to 120 d over the market).
  I did not reckon which chains go over 240 d with orders counted.
- An order already in a save made before the mend keeps its old whole pay, shared out by the unit: a save with the
  756 d Sarre order in it still pays 756 d, tolled. No game has shipped, so I left it.
- 13 orders come from cards that open "at any Frankish port" and the like; their real pay depends on the port. 13
  more can open in two to four ports and are reckoned at the first.
- Sarre, Fordwic and Medemelacha are of the kind "market" and Doccinga a "village": `commissions_793.json` has no
  entry for those kinds, so four of the twelve ports never post an order of their own. That is as it was; I did not
  change it. It may be wanted.

## 11. The script behind part 7

To make the counts again after the cards have changed (it reads the cards, the goods, the ports and the trade rules,
writes `scan.json` beside itself and prints the counts; it changes nothing in the project). The tables of part 7 are
`scan.json` laid out row by row.

```python
# scan.py - reads the cards, changes nothing. Lists every "commission" effect with what it pays today and what it
# should pay under the rules of Design/fixes/orders.md.
import json, os, glob, re, math, collections, sys
ROOT = r"G:/kapman_unity/Kapman2"
DATA = ROOT + "/Assets/Kapman/Game/Data"
FIRST = ["dorestad", "traiectum", "walichrum", "lundenwic", "witla", "medemelacha", "doccinga", "daventre", "quentovic", "sarre", "fordwic", "ripa"]
HOLD = 660.0            # kg of room in the first boat: cargo_kg 900 less 4 water casks of 60 l (voyage_rules.json)
PREMIUM_MAX = 0.5
BONUS_MAX = 60.0        # ruling 14: no card pays over 60 d; here, the part of the pay of an order that is over the market
SPREAD = 0.20
UNWANTED = 0.6
RX = re.compile(r"^\s*commission\s+([a-z_0-9]+)\s+(\d+)\s+by\s+([\d.]+)\s+premium\s+([\d.]+)\s*$")

def load(p):
    with open(p, "r", encoding="utf-8-sig") as f:
        return json.load(f)

G = load(DATA + "/goods_793.json")
MADE = {float(k): v for k, v in G["made_factor"].items()}
WANT = {float(k): v for k, v in G["wanted_factor"].items()}
GOODS = {g["id"]: g for g in G["goods"]}
PORTS = {p["id"]: p for p in load(DATA + "/ports_793.json")["ports"]}
RULES = load(DATA + "/trade_rules.json")
ZONE = {}
for z in RULES["zones"]:
    for r in z["regions"]:
        ZONE[r] = z

def interp(table, x):
    if x <= 0: return 1.0
    ks = sorted(table)
    if x <= ks[0]: return table[ks[0]]
    for i in range(1, len(ks)):
        if x <= ks[i]:
            t = (x - ks[i - 1]) / (ks[i] - ks[i - 1])
            return table[ks[i - 1]] + (table[ks[i]] - table[ks[i - 1]]) * t
    return table[ks[-1]]

def usual_pays(good, port):
    """What the market there usually pays a seller for one unit: Market.UsualPrice (no season) less half the spread."""
    g = GOODS[good]
    if port is None:
        return g["value"] * (1 - SPREAD / 2), "value only"
    region = PORTS[port]["region"]
    made = g.get("made", {}).get(region, 0)
    wanted = g.get("wanted", {}).get(region, 0)
    p = g["value"] * interp(MADE, made) * interp(WANT, wanted)
    how = "made %s wanted %s" % (made, wanted)
    if made <= 0 and wanted <= 0:
        p *= UNWANTED
        how = "neither made nor wanted"
    return max(0.05, p) * (1 - SPREAD / 2), how

def space(good):
    g = GOODS[good]
    return g["kg"] * max(0.1, g.get("bulk", 1))

def years(when, fname):
    lo, hi = 793, 1066
    for op, n in re.findall(r"\byear\s*(<=|>=|<|>|=)\s*(\d{3,4})", when or ""):
        n = int(n)
        if op == "<": hi = min(hi, n - 1)
        elif op == "<=": hi = min(hi, n)
        elif op == ">": lo = max(lo, n + 1)
        elif op == ">=": lo = max(lo, n)
        elif op == "=": lo, hi = max(lo, n), min(hi, n)
    m = re.match(r"chain_.*_(\d{3,4})_793\.json", fname)
    if m and int(m.group(1)) > 805:
        lo = max(lo, int(m.group(1)) - 5)
    return lo, hi

rows = []
def card_rows(card, fname):
    when = card.get("when", "") or ""
    ports = re.findall(r"port\.id\s*=\s*([a-z_0-9]+)", when)
    m = re.search(r"port\.id\s+in\s+\[([^\]]*)\]", when)
    if m:
        ports += re.findall(r"[a-z_0-9]+", m.group(1))
    ports = [p for p in ports if p in PORTS]
    for o in card.get("options", []) or []:
        effs = []
        def take(x, branch):
            if isinstance(x, str):
                if RX.match(x): effs.append((x, branch))
            elif isinstance(x, list):
                for v in x: take(v, branch)
            elif isinstance(x, dict):
                for k, v in x.items(): take(v, k)
        take(o.get("effects"), "")
        for e, branch in effs:
            good, n, days, prem = RX.match(e).groups()
            rows.append(dict(file=fname, card=card.get("id", "?"), opt=o.get("key", "?"), label=o.get("label", ""), branch=branch,
                             when=when, stage=card.get("stage", ""), ports=ports, good=good, units=int(n), days=float(days),
                             prem=float(prem), effect=e.strip(), check=o.get("check", ""), cost=o.get("cost", "")))

for p in sorted(glob.glob(DATA + "/encounters/*.json")) + sorted(glob.glob(DATA + "/places/*.json")) + [DATA + "/events_793.json"]:
    try:
        d = load(p)
    except Exception as ex:
        print("CANNOT READ", p, str(ex)[:100]); continue
    fname = os.path.basename(p)
    for c in d.get("cards", []) or []:
        if isinstance(c, dict): card_rows(c, fname)
    for ev in d.get("events", []) or []:
        if isinstance(ev, dict) and isinstance(ev.get("card"), dict):
            c = dict(ev["card"]); c.setdefault("id", ev.get("id", "?")); c.setdefault("when", ev.get("when", ""))
            card_rows(c, fname)

out = []
for r in rows:
    g = GOODS.get(r["good"])
    r["known_good"] = g is not None
    if g is None:
        out.append(r); continue
    port = r["ports"][0] if len(r["ports"]) >= 1 else None
    r["port"] = port
    r["many_ports"] = len(set(r["ports"])) > 1
    pays, how = usual_pays(r["good"], port)
    r["pays"], r["how"] = pays, how
    sp = space(r["good"])
    r["space"] = sp
    r["fits"] = int(math.floor(HOLD / sp + 1e-6))
    r["loads"] = int(math.ceil(r["units"] / r["fits"])) if r["fits"] > 0 else 999
    r["today"] = round(pays * (1 + r["prem"]) * r["units"])
    p1 = min(r["prem"], PREMIUM_MAX)
    market = pays * r["units"]
    bonus = market * p1
    p2 = p1 if bonus <= BONUS_MAX else BONUS_MAX / market
    r["p_half"], r["p_new"] = p1, p2
    r["market"] = market
    r["new_half"] = round(market * (1 + p1))
    r["new"] = round(market * (1 + p2))
    zone = ZONE.get(PORTS[port]["region"]) if port else None
    r["zone"] = zone["id"] if zone else "?"
    toll = (0.10 if zone and zone["id"] == "english" else zone["toll"]) if zone else 0.10   # English toll a tenth (ruling 14)
    r["toll_share"] = toll
    r["toll"] = round(market * (1 + p2) * toll)
    r["net"] = r["new"] - r["toll"]
    lo, hi = years(r["when"], r["file"])
    r["lo"], r["hi"] = lo, hi
    r["first"] = port in FIRST and lo <= 805 and hi >= 793
    b = []
    if r["prem"] > PREMIUM_MAX + 1e-9: b.append("P")
    if r["units"] > r["fits"]: b.append("H")
    if bonus > BONUS_MAX + 0.5 and r["prem"] <= PREMIUM_MAX + 1e-9: b.append("B")
    if port is None: b.append("?")
    r["breaks"] = "".join(b)
    out.append(r)

json.dump(out, open(os.path.join(os.path.dirname(os.path.abspath(__file__)), "scan.json"), "w", encoding="utf-8"), indent=1, ensure_ascii=False)
ok = [r for r in out if r["known_good"]]
print("card orders:", len(out), " unknown good:", len(out) - len(ok), [r["good"] for r in out if not r["known_good"]])
print("premium >= 1.0:", sum(1 for r in ok if r["prem"] >= 1.0), " >= 1.5:", sum(1 for r in ok if r["prem"] >= 1.5), " > 0.5:", sum(1 for r in ok if r["prem"] > 0.5))
print("over the hold:", sum(1 for r in ok if r["units"] > r["fits"]))
wine = [r for r in ok if "wine" in r["good"]]
print("wine orders:", len(wine), " over the hold:", sum(1 for r in wine if r["units"] > r["fits"]), collections.Counter(r["good"] for r in wine))
print("no port in when:", sum(1 for r in ok if r["port"] is None), " several ports:", sum(1 for r in ok if r["many_ports"]))
print("breaks any:", sum(1 for r in ok if r["breaks"].replace("?", "")), " first release:", sum(1 for r in ok if r["first"]), " first release and breaks:", sum(1 for r in ok if r["first"] and r["breaks"].replace("?", "")))
print("bonus over 60 at the half:", sum(1 for r in ok if r["market"] * r["p_half"] > 60.5))
print("sum today:", sum(r["today"] for r in ok), " sum new (half):", sum(r["new_half"] for r in ok), " sum new (half and 60):", sum(r["new"] for r in ok))
print(collections.Counter(r["stage"] for r in ok))
print(collections.Counter(r["branch"] for r in ok))
print(collections.Counter(r["prem"] for r in ok))
for r in ok:
    if r["port"] is None: print("  NO PORT", r["file"], r["card"], r["opt"], "|", r["when"][:110])
for r in ok:
    if r["many_ports"]: print("  MANY", r["file"], r["card"], r["ports"])
```
