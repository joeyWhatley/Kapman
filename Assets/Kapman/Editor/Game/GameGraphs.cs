using System.Collections.Generic;
using Kapman.Graphs;
using Unity.VisualScripting;
using UnityEngine;

namespace Kapman.EditorTools
{
    /// <summary>
    /// THE GAME'S GRAPHS, as first made (Assets/Kapman/Game/Graphs). After they're made they're the user's: edit them
    /// in the graph window; these only make the ones that are missing. Each is laid out left to right in labelled
    /// groups, with notes. Variables they share live on the "--- GAME ---" object (Object variables):
    ///   Docked port (the Port the ship lies at, or nothing), Leaving (true from casting off till she's out of the harbour)
    /// </summary>
    public static class GameGraphs
    {
        public const string Folder = "Assets/Kapman/Game/Graphs";
        public const string DockedVar = "Docked port", LeavingVar = "Leaving", PortMenuEvent = "Port menu";
        public const string MarketEvent = "Market", TownEvent = "Town";

        public static ScriptGraphAsset ComingInAndLeaving()
        {
            var g = new GraphBuilder("Ports - coming in and leaving",
                "Watches the ship: when she's slowed inside a port's reach she lies to at its berth and the port's menu opens. " +
                "When she casts off she's 'leaving' till she's out of the harbour again.");
            // 1. watching for the ship coming in
            g.Group(-40, -60, 1500, 520, "1. The ship coming in to a port",
                "Every frame: if she isn't lying anywhere, is inside a port's reach, has slowed, and isn't just leaving, she's arrived.", GraphBuilder.Watch);
            var update = g.Add(new Update(), 0, 0);
            var docked = g.Add(new GetVariable { kind = VariableKind.Object }, 0, 180);
            g.Set(docked.name, DockedVar);
            var nullCheck = g.Add(new NullCheck(), 260, 0);
            g.Flow(update.trigger, nullCheck.enter);
            g.Data(docked.value, nullCheck.input);
            var nearest = g.Add(new PortNearestToShip(), 260, 220);
            var speed = g.Add(new ShipSpeed(), 260, 360);
            var slow = g.Add(new Less(), 520, 340);
            g.Data(speed.speed, slow.a);
            g.Set(slow.b, 1.5f);
            var leaving = g.Add(new GetVariable { kind = VariableKind.Object }, 520, 200);
            g.Set(leaving.name, LeavingVar);
            var notLeaving = g.Add(new Negate(), 720, 200);
            g.Data(leaving.value, notLeaving.input);
            var both = g.Add(new And(), 900, 180);
            g.Data(nearest.inPort, both.a);
            g.Data(notLeaving.output, both.b);
            var all = g.Add(new And(), 1060, 260);
            g.Data(both.result, all.a);
            g.Data(slow.comparison, all.b);
            var arrived = g.Add(new If(), 1200, 0);
            g.Flow(nullCheck.ifNull, arrived.enter);
            g.Data(all.result, arrived.condition);
            g.Note(-20, 460, 420, 150, "Tune it here",
                "1.5 on the Less node: how slow (m/s) she must be to count as arrived (0.5 m/s is a knot).\n" +
                "Each port's reach is on its Port component (Arrival Radius, game metres round its Berth).");

            // 2. she's arrived: lie to, and open the port's menu
            g.Group(1460, -60, 980, 360, "2. Arrived: lie to and open the port",
                "Remember the port, stop the ship, and open the menu (Ports - in port).", GraphBuilder.Change);
            var setDocked = g.Add(new SetVariable { kind = VariableKind.Object }, 1500, 0);
            g.Set(setDocked.name, DockedVar);
            g.Flow(arrived.ifTrue, setDocked.assign);
            g.Data(nearest.port, setDocked.input);
            var moor = g.Add(new ShipMoor(), 1760, 0);
            g.Set(moor.moored, true);
            g.Flow(setDocked.assigned, moor.enter);
            var open = g.Add(new TriggerCustomEvent(), 2000, 0);
            g.Set(open.name, PortMenuEvent);
            g.Flow(moor.exit, open.enter);

            // 3. out of the harbour again
            g.Group(-40, 640, 1100, 300, "3. Out of the harbour again",
                "Once she's outside every port's reach she's no longer 'leaving', and can come in somewhere again.", GraphBuilder.Watch);
            var update2 = g.Add(new Update(), 0, 700);
            var nearest2 = g.Add(new PortNearestToShip(), 0, 820);
            var outside = g.Add(new Negate(), 260, 820);
            g.Data(nearest2.inPort, outside.input);
            var gone = g.Add(new If(), 300, 700);
            g.Flow(update2.trigger, gone.enter);
            g.Data(outside.output, gone.condition);
            var clear = g.Add(new SetVariable { kind = VariableKind.Object }, 560, 700);
            g.Set(clear.name, LeavingVar);
            g.Value(clear.input, false, 560, 840);
            g.Flow(gone.ifTrue, clear.assign);
            return g.Save($"{Folder}/Ports - coming in and leaving.asset");
        }

        public static ScriptGraphAsset InPort()
        {
            var g = new GraphBuilder("Ports - in port",
                "The port's menu, as text: what the player can do in port. The market, supplies and people come with their " +
                "systems; for now they say so. Your writing for each key goes in Game/Data/writing.json.");
            g.Group(-40, -60, 2100, 560, "1. The port's menu",
                "Opens on the 'Port menu' event (sent on arriving, and after each errand). The options are in the words of " +
                "the port's people (Game/Data/wording_793.json: Franks, Frisians, English, Northmen...), each with its plain " +
                "intention under it; then the errands of this kind of place (to come).", GraphBuilder.Talk);
            var menu = g.Add(new CustomEvent { coroutine = true }, 0, 0);
            g.Set(menu.name, PortMenuEvent);
            var docked = g.Add(new GetVariable { kind = VariableKind.Object }, 0, 200);
            g.Set(docked.name, DockedVar);
            var name = g.Add(new PortName(), 220, 200);
            g.Data(docked.value, name.port);
            var arrive = g.Add(new PortWords(), 220, 360);
            g.Data(docked.value, arrive.port);
            g.Set(arrive.key, "port.arrive");
            var inPort = g.Add(new VoyageInPort(), 220, -160);                // the casks filled, the men paid
            g.Data(docked.value, inPort.port);
            g.Flow(menu.trigger, inPort.enter);
            var show = g.Add(new TextShow(), 440, 0);
            g.Flow(inPort.exit, show.enter);
            g.Set(show.key, "port.arrive");
            g.Data(name.title, show.title);
            g.Data(arrive.intention, show.intention);
            // the four options, in the words of the port's people (Data/wording_793.json); the fallbacks if it hasn't them
            var options = new (string key, string fallback)[]
            {
                ("port.town", "Explore the town and markets"),          // (supplies and goods are bought in the town)
                ("port.news", "Talk to people"),
                ("port.leave", "Cast off"),
            };
            ControlOutput last = show.exit;
            for (int i = 0; i < options.Length; i++)
            {
                var o = g.Add(new PortAddOption(), 680 + i * 220, 0);
                g.Data(docked.value, o.port);
                g.Set(o.key, options[i].key);
                g.Set(o.fallback, options[i].fallback);
                g.Flow(last, o.enter);
                last = o.exit;
            }
            // and the errands of this kind of place (the procurator, the moneyer, the guest-house...): to come
            var errands = g.Add(new PortAddErrands(), 1560, 0);
            g.Data(docked.value, errands.port);
            g.Flow(last, errands.enter);
            var wait = g.Add(new TextWaitForChoice(), 1780, 0);
            g.Flow(errands.exit, wait.enter);

            g.Group(2100, -60, 2100, 1000, "2. What each choice does",
                "0: the town (Town - explore the town: up into the map, the market and supplies beside it; the menu again " +
                "when you're back at the ship).\n1: talk to people (the waterfront's news, prices into your ledger).\n2: cast off.\n" +
                "3 on: the place's errands (the toll house, the moneyer, lodging, alms...; Code/Game/Ports/PortLife.cs).", GraphBuilder.Change);
            var sw = g.Add(new SwitchOnInteger { options = new List<int> { 0, 1, 2 } }, 2140, 0);
            g.Flow(wait.exit, sw.enter);
            g.Data(wait.choice, sw.selector);
            var toTown = g.Add(new TextHide(), 2420, -160);
            g.Flow(sw.branches[0].Value, toTown.enter);
            var townGo = g.Add(new TriggerCustomEvent(), 2660, -160);
            g.Set(townGo.name, TownEvent);
            g.Flow(toTown.exit, townGo.enter);
            // 1: talk to people -> the facts, then back to the menu
            var talk = g.Add(new PortTalk(), 2420, 0);
            g.Flow(sw.branches[1].Value, talk.enter);
            g.Data(docked.value, talk.port);
            var talkShow = g.Add(new TextShow(), 2660, 0);
            g.Flow(talk.exit, talkShow.enter);
            g.Set(talkShow.key, "port.news.told");
            g.Set(talkShow.title, "Talk to people");
            g.Data(talk.facts, talkShow.intention);
            var orders = g.Add(new PortAddCommissions(), 2900, 0);           // (orders wanted here: take one, or not)
            g.Flow(talkShow.exit, orders.enter);
            g.Data(docked.value, orders.port);
            var talkBack = g.Add(new TextAddOption(), 3140, 0);
            g.Flow(orders.exit, talkBack.enter);
            g.Set(talkBack.key, "port.back");
            g.Set(talkBack.intention, "Back");
            var talkWait = g.Add(new TextWaitForChoice(), 3380, 0);
            g.Flow(talkBack.exit, talkWait.enter);
            var takeOrder = g.Add(new PortTakeCommission(), 3620, 0);
            g.Flow(talkWait.exit, takeOrder.enter);
            g.Data(docked.value, takeOrder.port);
            g.Data(talkWait.choice, takeOrder.choice);
            var again = g.Add(new TriggerCustomEvent(), 3860, 0);
            g.Set(again.name, PortMenuEvent);
            g.Flow(takeOrder.exit, again.enter);

            // 3 on: the place's errands -> the facts (or the town's market, for the minster's trade), then the menu
            var errand = g.Add(new PortDoErrand(), 2420, 560);
            g.Flow(sw.@default, errand.enter);
            g.Data(docked.value, errand.port);
            g.Data(wait.choice, errand.choice);
            var inTown = g.Add(new If(), 2660, 560);
            g.Flow(errand.exit, inTown.enter);
            g.Data(errand.toTown, inTown.condition);
            var hideForTown = g.Add(new TextHide(), 2900, 480);
            g.Flow(inTown.ifTrue, hideForTown.enter);
            var goTown = g.Add(new TriggerCustomEvent(), 3140, 480);
            g.Set(goTown.name, TownEvent);
            g.Flow(hideForTown.exit, goTown.enter);
            var errandShow = g.Add(new TextShow(), 2900, 640);
            g.Flow(inTown.ifFalse, errandShow.enter);
            g.Set(errandShow.key, "port.errand.done");
            g.Data(errand.title, errandShow.title);
            g.Data(errand.facts, errandShow.intention);
            var errandBack = g.Add(new TextAddOption(), 3140, 640);
            g.Flow(errandShow.exit, errandBack.enter);
            g.Set(errandBack.key, "port.back");
            g.Set(errandBack.intention, "Back");
            var errandWait = g.Add(new TextWaitForChoice(), 3380, 640);
            g.Flow(errandBack.exit, errandWait.enter);
            var again2 = g.Add(new TriggerCustomEvent(), 3620, 640);
            g.Set(again2.name, PortMenuEvent);
            g.Flow(errandWait.exit, again2.enter);

            var castOff = g.Add(new ShipMoor(), 2420, 300);
            g.Set(castOff.moored, false);
            g.Flow(sw.branches[2].Value, castOff.enter);
            var undock = g.Add(new SetVariable { kind = VariableKind.Object }, 2660, 300);
            g.Set(undock.name, DockedVar);
            g.Value(undock.input, null, 2660, 440);                      // (lying nowhere now)
            g.Flow(castOff.exit, undock.assign);
            var leave = g.Add(new SetVariable { kind = VariableKind.Object }, 2900, 300);
            g.Set(leave.name, LeavingVar);
            g.Value(leave.input, true, 2900, 440);
            g.Flow(undock.assigned, leave.assign);
            var hide = g.Add(new TextHide(), 3140, 300);
            g.Flow(leave.assigned, hide.enter);
            g.Note(2140, 860, 620, 180, "Adding an errand",
                "A place's errands come from wording_793.json (kinds and ports: 'extra'); what each does is in Code/Game/Ports/" +
                "PortLife.cs (Port: Do errand). Or add an option in group 1 before the errands (Port: Add option, or Text: Add " +
                "option), add its number to the Switch's options, and wire what it does from the Switch's new exit. Your text for " +
                "each key goes in writing.json.");
            return g.Save($"{Folder}/Ports - in port.asset");
        }

        public static ScriptGraphAsset TownExplore()
        {
            var g = new GraphBuilder("Town - explore the town",
                "The town view: on the 'Town' event (the port menu's first choice) the camera rises off the ship into " +
                "the 2D map over the town, with its market docked beside it. When the player goes back to the ship, " +
                "the port's menu opens again.");
            g.Group(-40, -60, 1300, 380, "1. Up into the town's map",
                "Town: Explore plays the rise (camera up, the map fading in, settling on the town), opens the market " +
                "beside the map, and waits till the player is back down at the ship (Back to the ship, or Esc).", GraphBuilder.Talk);
            var go = g.Add(new CustomEvent { coroutine = true }, 0, 0);
            g.Set(go.name, TownEvent);
            var docked = g.Add(new GetVariable { kind = VariableKind.Object }, 0, 200);
            g.Set(docked.name, DockedVar);
            var explore = g.Add(new Kapman.Graphs.TownExplore(), 300, 0);
            g.Flow(go.trigger, explore.enter);
            g.Data(docked.value, explore.port);
            var back = g.Add(new TriggerCustomEvent(), 600, 0);
            g.Set(back.name, PortMenuEvent);
            g.Flow(explore.exit, back.enter);
            g.Note(860, 0, 380, 260, "The map and the market",
                "The map: Code/Game/Map (MapView: the rise, panning and zoom, ports, the ship; MapRenderer: drawing " +
                "the map from the baked land; Exploration: the fog of war). [M] opens it at sea.\n" +
                "The market: Code/Game/UI/MarketPanel.cs, docked on the right. Pick a good: the map marks where it's " +
                "made and wanted, and what it fetched where you've seen it.");
            return g.Save($"{Folder}/Town - explore the town.asset");
        }

        public static ScriptGraphAsset TradeMarket()
        {
            var g = new GraphBuilder("Trade - market",
                "Every deal in a market comes here as 'On trade' (the market itself opens in the town view: Town - " +
                "explore the town).");
            g.Note(-20, -40, 640, 200, "How the market works",
                "Prices: Game/Data/goods_793.json (each good's value, and where it's made and wanted) and " +
                "Game/Data/trade_rules.json (money, tolls, laws, how prices answer supply and demand).\n" +
                "The panel: Code/Game/UI/MarketPanel.cs. The rules: Code/Game/Trade. The market node (Market: Open) " +
                "can also open it over the 3D, without the map.");

            g.Group(-40, 380, 1300, 420, "2. After each deal",
                "'On trade' fires for every buy and sale: what, how many, for how much, where, and the law on the good. " +
                "Reputation (slaving, arms, relics...) and events will hook in here.", GraphBuilder.Watch);
            g.Add(new OnTrade(), 0, 440);
            g.Note(300, 440, 520, 220, "Hook things on here",
                "e.g. law = 'captives' and not bought -> standing with the Church's people falls (Reputation, to come).\n" +
                "e.g. law = 'arms' bought in a Frankish port -> a chance of the count's men searching her as she leaves.");
            return g.Save($"{Folder}/Trade - market.asset");
        }

        public static ScriptGraphAsset TradeEachDay()
        {
            var g = new GraphBuilder("Trade - markets each day",
                "Each game day every market makes and uses its goods: stock settles back, gluts fade, and the prices move with them.");
            g.Group(-40, -60, 900, 300, "1. A new day in the markets", "On game day -> Markets: Next day.", GraphBuilder.Change);
            var day = g.Add(new OnGameDay(), 0, 0);
            var tick = g.Add(new MarketsNextDay(), 300, 0);
            g.Flow(day.trigger, tick.enter);
            g.Note(560, 0, 300, 180, "Traders to come",
                "The AI traders' buying and selling (moving prices between ports) will hook in here too.");
            return g.Save($"{Folder}/Trade - markets each day.asset");
        }

        public static ScriptGraphAsset SimWorld()
        {
            var g = new GraphBuilder("Sim - the world",
                "The world's other ships: each game hour they sail, arrive, deal and choose where next (Code/Game/Sim; " +
                "Plans/ai_fleets.md). Their buying and selling moves the same markets as yours.");
            g.Group(-40, -60, 900, 300, "1. An hour of the world", "On game hour -> Sim: Hour. Without it the world's ships stand still.", GraphBuilder.Change);
            var hour = g.Add(new OnGameHour(), 0, 0);
            var tick = g.Add(new SimHour(), 300, 0);
            g.Flow(hour.trigger, tick.enter);
            g.Note(560, 0, 340, 220, "What tunes it",
                "Data/traffic_793.json: how many traders each port sends out, in which craft, and the sailing seasons.\n" +
                "Data/ship_classes_793.json: each kind of craft.\nSimRules (Code/Game/Sim/WorldSim.cs): the traders' rules of thumb.\n" +
                "Test run: Kapman > Sim > Run a year (headless): spreadsheets in data/work/sim.\nOn the map: [G] shows every ship; [ and ] speed time.");
            return g.Save($"{Folder}/Sim - the world.asset");
        }

        public static ScriptGraphAsset ChronicleEffects()
        {
            var g = new GraphBuilder("Chronicle - what events do",
                "Real events of the time (Data/chronicle_793.json) come on their days; here is what each kind does to the markets " +
                "where it happened. Change the numbers, add kinds (the Switch's options), or wire your own.");
            g.Group(-40, -60, 760, 360, "1. An event's day comes", "On chronicle event -> by its kind.", GraphBuilder.Watch);
            var on = g.Add(new OnChronicleEvent(), 0, 0);
            var kinds = new List<(string kind, string where, string goods, float left, float dearer, string why)>
            {
                ("raid", "ports", "", 0.3f, 0.5f, "a raid: the market plundered, its people killed or fled - little to buy, all of it dear"),
                ("burning", "ports", "", 0.4f, 0.4f, "a town or port burned"),
                ("famine", "regions", "Food, Provisions", 0.5f, 0.6f, "famine: grain and food scarce and dear across the regions"),
                ("harvest", "regions", "Food", 1f, -0.2f, "a good harvest: food cheaper"),
                ("murrain", "regions", "Livestock", 0.4f, 0.5f, "a cattle plague: few beasts, and dear"),
                ("winter", "regions", "Food, Provisions, Timber", 0.7f, 0.3f, "a hard winter: food and firewood dear"),
                ("epidemic", "regions", "", 0.85f, 0.1f, "sickness: fewer hands, a little dearer"),
                ("fair", "ports", "", 1f, 0.2f, "a fair: buyers come, prices up"),
            };
            var sw = g.Add(new SwitchOnString { options = kinds.ConvertAll(k => k.kind) }, 360, 0);
            g.Flow(on.trigger, sw.enter);
            g.Data(on.kind, sw.selector);
            g.Group(700, -60, 900, 120 + kinds.Count * 170, "2. What it does", "Each kind shocks the markets: their stock falls to a " +
                "share and the goods are dearer, fading over the weeks after (trade_rules: shock_fade).", GraphBuilder.Change);
            for (int i = 0; i < kinds.Count; i++)
            {
                var k = kinds[i];
                var shock = g.Add(new ChronicleShock(), 760, i * 170);
                g.Flow(sw.branches[i].Value, shock.enter);
                g.Data(on.id, shock.id);
                g.Set(shock.where, k.where);
                g.Set(shock.goods, k.goods);
                g.Set(shock.stockLeft, k.left);
                g.Set(shock.dearer, k.dearer);
                g.Note(1100, i * 170, 420, 120, k.kind, k.why);
            }
            g.Note(0, 340, 640, 160, "To come",
                "Wars and campaigns closing routes (the sea lanes), raiders taking ships (the AI's raiders), laws by year (already " +
                "in trade_rules), news of the event travelling port to port (who knows, and when).");
            return g.Save($"{Folder}/Chronicle - what events do.asset");
        }

        public static ScriptGraphAsset VoyageEachDay()
        {
            var g = new GraphBuilder("Voyage - each day",
                "The crew's day: they eat and drink from the stores in the hold, are owed their pay, and the sea works on them " +
                "(Data/voyage_rules.json). In port the casks are filled and the men paid (Ports - in port: Voyage: In port).");
            g.Group(-40, -60, 900, 300, "1. A day aboard", "On game day -> Voyage: Day. What comes of it shows as plain facts (Notices).", GraphBuilder.Change);
            var day = g.Add(new OnGameDay(), 0, 0);
            var tick = g.Add(new VoyageDay(), 300, 0);
            g.Flow(day.trigger, tick.enter);
            g.Note(560, 0, 340, 220, "What tunes it",
                "Data/voyage_rules.json: the crew (men, pay), their rations (food kg and drink litres a man a day, which goods " +
                "count), the water casks, hunger, thirst, seasickness and heart.\nVoyage: Crew gives the numbers for your own events " +
                "(e.g. a man falls sick when health is low).");
            return g.Save($"{Folder}/Voyage - each day.asset");
        }

        public const string HailMenuEvent = "Hail menu";

        /// <summary>Meeting a trader at sea: offered when the lookout makes one out; close and hail; alongside, the talk.</summary>
        public static ScriptGraphAsset EncounterTrader()
        {
            var g = new GraphBuilder("Encounter - a trader",
                "A trader made out at sea (the lookout: Code/Game/Sim/SeaWatch.cs) can be met: hold your course, or close and " +
                "hail her. She heaves to, your ship makes for her and lies to alongside; the game's clock stands still while you " +
                "talk (the sea keeps moving). Then you part, she sails on and your course resumes. Facts only: your writing for " +
                "each key goes in Game/Data/writing.json.");
            // 1. a ship made out: the offer
            g.Group(-40, -60, 2400, 560, "1. A trader made out",
                "On ship made out -> if a meeting can be offered now (no other on, at sea, no panel open) -> her facts and the " +
                "choice. With no answer in 60 seconds, you hold your course.", GraphBuilder.Talk);
            var on = g.Add(new OnShipMadeOut { coroutine = true }, 0, 0);
            var can = g.Add(new If(), 260, 0);
            g.Flow(on.trigger, can.enter);
            g.Data(on.canMeet, can.condition);
            var offer = g.Add(new EncounterOffer(), 480, 0);
            g.Flow(can.ifTrue, offer.enter);
            g.Data(on.ship, offer.ship);
            var show = g.Add(new TextShow(), 720, 0);
            g.Flow(offer.offered, show.enter);
            g.Set(show.key, "encounter.trader");
            g.Data(on.label, show.title);
            g.Data(on.facts, show.intention);
            g.Set(show.frost, false);                                     // (the scene stays sharp: she's out there)
            var hold = g.Add(new TextAddOption(), 960, 0);
            g.Flow(show.exit, hold.enter);
            g.Set(hold.key, "encounter.hold");
            g.Set(hold.intention, "Hold your course");
            var hail = g.Add(new TextAddOption(), 1180, 0);
            g.Flow(hold.exit, hail.enter);
            g.Set(hail.key, "encounter.hail");
            g.Set(hail.intention, "Close and hail her");
            var wait = g.Add(new TextWaitForChoiceTimed(), 1400, 0);
            g.Flow(hail.exit, wait.enter);
            g.Set(wait.seconds, 60f);
            g.Set(wait.fallback, 0);
            var sw = g.Add(new SwitchOnInteger { options = new List<int> { 0, 1 } }, 1640, 0);
            g.Flow(wait.exit, sw.enter);
            g.Data(wait.choice, sw.selector);
            var hideHold = g.Add(new TextHide(), 1880, -140);
            g.Flow(sw.branches[0].Value, hideHold.enter);
            g.Flow(sw.@default, hideHold.enter);
            var letGo = g.Add(new EncounterPart(), 2100, -140);
            g.Flow(hideHold.exit, letGo.enter);
            var close = g.Add(new EncounterCloseAndHail(), 1880, 60);
            g.Flow(sw.branches[1].Value, close.enter);
            var hideClose = g.Add(new TextHide(), 2100, 60);
            g.Flow(close.exit, hideClose.enter);

            // 2. closing: till alongside
            g.Group(2380, -60, 1200, 560, "2. Closing with her",
                "She heaves to; your ship makes for her, rowing the last of it (VoyagePlanner). Alongside (40 game metres) the " +
                "clock holds and the talk begins. If the meeting ends first, it's let go.", GraphBuilder.Watch);
            var till = g.Add(new EncounterWaitAlongside(), 2420, 60);
            g.Flow(hideClose.exit, till.enter);
            var there = g.Add(new If(), 2660, 60);
            g.Flow(till.exit, there.enter);
            g.Data(till.alongside, there.condition);
            var talk = g.Add(new TriggerCustomEvent(), 2900, 0);
            g.Set(talk.name, HailMenuEvent);
            g.Flow(there.ifTrue, talk.enter);
            var gone = g.Add(new EncounterPart(), 2900, 200);
            g.Flow(there.ifFalse, gone.enter);

            // 3. alongside: the talk
            g.Group(-40, 600, 2700, 620, "3. Alongside: the talk",
                "The menu (the 'Hail menu' event): ask after the weather she met (the weather model along her track, every 6 " +
                "hours back to 48), ask what prices she knows (into your ledger, as heard from her, where newer than yours), or " +
                "ask what she'd sell (a little of her biggest lot, at her destination's price less 15%), or part ways. What she " +
                "told shows on the menu.", GraphBuilder.Talk);
            var menu = g.Add(new CustomEvent { coroutine = true }, 0, 660);
            g.Set(menu.name, HailMenuEvent);
            var her = g.Add(new EncounterShip(), 0, 860);
            var show2 = g.Add(new TextShow(), 260, 660);
            g.Flow(menu.trigger, show2.enter);
            g.Set(show2.key, "encounter.alongside");
            g.Data(her.label, show2.title);
            g.Data(her.news, show2.intention);
            g.Set(show2.frost, false);
            var options = new (string key, string intention)[]
            {
                ("encounter.weather", "Ask after the weather she met"),
                ("encounter.prices", "Ask what prices she knows"),
                ("encounter.offer", "Ask what she'd sell"),
                ("encounter.part", "Part ways"),
            };
            ControlOutput last = show2.exit;
            for (int i = 0; i < options.Length; i++)
            {
                var o = g.Add(new TextAddOption(), 500 + i * 220, 660);
                g.Flow(last, o.enter);
                g.Set(o.key, options[i].key);
                g.Set(o.intention, options[i].intention);
                last = o.exit;
            }
            var wait2 = g.Add(new TextWaitForChoice(), 1180, 660);
            g.Flow(last, wait2.enter);
            var sw2 = g.Add(new SwitchOnInteger { options = new List<int> { 0, 1, 2, 3 } }, 1420, 660);
            g.Flow(wait2.exit, sw2.enter);
            g.Data(wait2.choice, sw2.selector);
            var weather = g.Add(new EncounterWeatherNews(), 1680, 620);
            g.Flow(sw2.branches[0].Value, weather.enter);
            var again1 = g.Add(new TriggerCustomEvent(), 1920, 620);
            g.Set(again1.name, HailMenuEvent);
            g.Flow(weather.exit, again1.enter);
            var prices = g.Add(new EncounterPricesHeard(), 1680, 800);
            g.Flow(sw2.branches[1].Value, prices.enter);
            var again2 = g.Add(new TriggerCustomEvent(), 1920, 800);
            g.Set(again2.name, HailMenuEvent);
            g.Flow(prices.exit, again2.enter);
            // her offer: a little from her hold -> buy it, or not -> the menu again
            var sale = g.Add(new EncounterMakeOffer(), 1680, 1160);
            g.Flow(sw2.branches[2].Value, sale.enter);
            var offerShow = g.Add(new TextShow(), 1920, 1120);
            g.Flow(sale.offered, offerShow.enter);
            g.Set(offerShow.key, "encounter.offer.made");
            g.Data(her.label, offerShow.title);
            g.Data(sale.facts, offerShow.intention);
            g.Set(offerShow.frost, false);
            var buy = g.Add(new TextAddOption(), 2160, 1120);
            g.Flow(offerShow.exit, buy.enter);
            g.Set(buy.key, "encounter.offer.buy");
            g.Set(buy.intention, "Buy it");
            var decline = g.Add(new TextAddOption(), 2400, 1120);
            g.Flow(buy.exit, decline.enter);
            g.Set(decline.key, "encounter.offer.no");
            g.Set(decline.intention, "No");
            var offerWait = g.Add(new TextWaitForChoice(), 2640, 1120);
            g.Flow(decline.exit, offerWait.enter);
            var offerSw = g.Add(new SwitchOnInteger { options = new List<int> { 0 } }, 2880, 1120);
            g.Flow(offerWait.exit, offerSw.enter);
            g.Data(offerWait.choice, offerSw.selector);
            var take = g.Add(new EncounterTakeOffer(), 3120, 1080);
            g.Flow(offerSw.branches[0].Value, take.enter);
            var again3 = g.Add(new TriggerCustomEvent(), 3360, 1120);
            g.Set(again3.name, HailMenuEvent);
            g.Flow(take.exit, again3.enter);
            g.Flow(offerSw.@default, again3.enter);
            g.Flow(sale.none, again3.enter);

            var hideAll = g.Add(new TextHide(), 1680, 980);
            g.Flow(sw2.branches[3].Value, hideAll.enter);
            g.Flow(sw2.@default, hideAll.enter);
            var part = g.Add(new EncounterPart(), 1920, 980);
            g.Flow(hideAll.exit, part.enter);
            g.Note(2180, 660, 440, 300, "Adding to the talk",
                "Add an option in the row (Text: Add option, a key and its intention), add its number to the Switch's options, " +
                "and wire what it does from the Switch's new exit, then 'Hail menu' again. To come: news of raids and fairs " +
                "she's heard.");
            return g.Save($"{Folder}/Encounter - a trader.asset");
        }

        public const string RaidersAlongsideEvent = "Raiders alongside", RaidersOutcomeEvent = "Raiders outcome";

        /// <summary>Raiders at sea: they come for you; run, fight, pay, talk or give up; the chase live; the outcome.</summary>
        public static ScriptGraphAsset EncounterRaiders()
        {
            var g = new GraphBuilder("Encounter - raiders",
                "Raiders made out at sea (a band of longships with room for more, by daylight) come for you (Code/Game/Sim/" +
                "RaiderEncounter.cs; Data/raiders_793.json). Run - a live chase: their rowing sprint, then a steady pull; it ends " +
                "caught, at nightfall, lost in thick air, or when they give up - or turn and fight, pay tribute, parley in a " +
                "shared tongue, or submit. Every outcome is plain facts, applied to your hold, purse and men. You always live.");
            // 1. they come on
            g.Group(-40, -60, 2600, 560, "1. Raiders made out",
                "On raiders made out -> if they come for you -> the choice (no answer in 20 seconds: run).", GraphBuilder.Talk);
            var on = g.Add(new OnRaidersMadeOut { coroutine = true }, 0, 0);
            var comes = g.Add(new If(), 260, 0);
            g.Flow(on.trigger, comes.enter);
            g.Data(on.comes, comes.condition);
            var threaten = g.Add(new RaidersThreaten(), 480, 0);
            g.Flow(comes.ifTrue, threaten.enter);
            g.Data(on.ship, threaten.ship);
            var status = g.Add(new RaidersStatus(), 480, 200);
            var show = g.Add(new TextShow(), 720, 0);
            g.Flow(threaten.coming, show.enter);
            g.Set(show.key, "encounter.raiders");
            g.Set(show.title, "Raiders");
            g.Data(status.status, show.intention);
            g.Set(show.frost, false);
            var first = new (string key, string intention)[]
            {
                ("encounter.raiders.run", "Run"),
                ("encounter.raiders.fight", "Turn and fight"),
                ("encounter.raiders.tribute", "Pay tribute"),
                ("encounter.raiders.parley", "Parley"),
                ("encounter.raiders.submit", "Submit"),
            };
            ControlOutput last = show.exit;
            for (int i = 0; i < first.Length; i++)
            {
                var o = g.Add(new TextAddOption(), 960 + i * 200, 0);
                g.Flow(last, o.enter);
                g.Set(o.key, first[i].key);
                g.Set(o.intention, first[i].intention);
                last = o.exit;
            }
            var wait = g.Add(new TextWaitForChoiceTimed(), 1980, 0);
            g.Flow(last, wait.enter);
            g.Set(wait.seconds, 20f);
            g.Set(wait.fallback, 0);
            var sw = g.Add(new SwitchOnInteger { options = new List<int> { 0, 1, 2, 3, 4 } }, 2220, 0);
            g.Flow(wait.exit, sw.enter);
            g.Data(wait.choice, sw.selector);

            // 2. running: the chase
            g.Group(-40, 560, 2600, 520, "2. Running",
                "Your ship sails away from them at her best. The status line keeps the numbers; turn and fight, pay or submit " +
                "at any time. It ends caught (they're alongside: section 3), or escaped.", GraphBuilder.Watch);
            var run = g.Add(new RaidersRun(), 0, 620);
            g.Flow(sw.branches[0].Value, run.enter);
            g.Flow(sw.@default, run.enter);
            var showRun = g.Add(new TextShow(), 240, 620);
            g.Flow(run.exit, showRun.enter);
            g.Set(showRun.key, "encounter.raiders.running");
            g.Set(showRun.title, "Running from raiders");
            g.Data(status.status, showRun.intention);
            g.Set(showRun.frost, false);
            var during = new (string key, string intention)[]
            {
                ("encounter.raiders.fight", "Turn and fight"),
                ("encounter.raiders.tribute", "Pay tribute"),
                ("encounter.raiders.submit", "Submit"),
            };
            last = showRun.exit;
            for (int i = 0; i < during.Length; i++)
            {
                var o = g.Add(new TextAddOption(), 480 + i * 200, 620);
                g.Flow(last, o.enter);
                g.Set(o.key, during[i].key);
                g.Set(o.intention, during[i].intention);
                last = o.exit;
            }
            var chase = g.Add(new RaidersChase(), 1100, 620);
            g.Flow(last, chase.enter);
            var how = g.Add(new SwitchOnString { options = new List<string> { "caught", "escaped", "fight", "tribute", "submit" } }, 1340, 620);
            g.Flow(chase.exit, how.enter);
            g.Data(chase.result, how.selector);
            var toAlongside = g.Add(new TriggerCustomEvent(), 1600, 560);
            g.Set(toAlongside.name, RaidersAlongsideEvent);
            g.Flow(how.branches[0].Value, toAlongside.enter);

            // the answers from 1 and 2: each its Answer node, then the outcome
            var answers = new (ControlOutput from, string what, float y)[]
            {
                (sw.branches[1].Value, "fight", -160), (sw.branches[2].Value, "tribute", -60), (sw.branches[3].Value, "parley", 40),
                (sw.branches[4].Value, "submit", 140),
                (how.branches[1].Value, "escaped", 700), (how.branches[2].Value, "fight", 800), (how.branches[3].Value, "tribute", 900),
                (how.branches[4].Value, "submit", 1000),
            };
            foreach (var (from, what, y) in answers)
            {
                var ans = g.Add(new RaidersAnswer(), 2460, y);
                g.Flow(from, ans.enter);
                g.Set(ans.answer, what);
                var told = g.Add(new TriggerCustomEvent { argumentCount = 1 }, 2700, y);
                g.Set(told.name, RaidersOutcomeEvent);
                g.Flow(ans.settled, told.enter);
                g.Data(ans.outcome, told.arguments[0]);
                var again = g.Add(new TriggerCustomEvent(), 2700, y + 50);
                g.Set(again.name, RaidersAlongsideEvent);
                g.Flow(ans.unsettled, again.enter);
            }

            // 3. alongside
            g.Group(-40, 1120, 2000, 460, "3. They're alongside",
                "Caught, or a parley they won't hear: fight, pay, try to talk, or give up.", GraphBuilder.Talk);
            var along = g.Add(new CustomEvent { coroutine = true }, 0, 1180);
            g.Set(along.name, RaidersAlongsideEvent);
            var showAlong = g.Add(new TextShow(), 240, 1180);
            g.Flow(along.trigger, showAlong.enter);
            g.Set(showAlong.key, "encounter.raiders.alongside");
            g.Set(showAlong.title, "Raiders alongside");
            g.Data(status.status, showAlong.intention);
            g.Set(showAlong.frost, false);
            var caughtOptions = new (string key, string intention, string what)[]
            {
                ("encounter.raiders.fight", "Turn and fight", "fight"),
                ("encounter.raiders.tribute", "Pay tribute", "tribute"),
                ("encounter.raiders.parley", "Parley", "parley"),
                ("encounter.raiders.submit", "Submit", "submit"),
            };
            last = showAlong.exit;
            for (int i = 0; i < caughtOptions.Length; i++)
            {
                var o = g.Add(new TextAddOption(), 480 + i * 200, 1180);
                g.Flow(last, o.enter);
                g.Set(o.key, caughtOptions[i].key);
                g.Set(o.intention, caughtOptions[i].intention);
                last = o.exit;
            }
            var waitAlong = g.Add(new TextWaitForChoice(), 1300, 1180);
            g.Flow(last, waitAlong.enter);
            var swAlong = g.Add(new SwitchOnInteger { options = new List<int> { 0, 1, 2, 3 } }, 1540, 1180);
            g.Flow(waitAlong.exit, swAlong.enter);
            g.Data(waitAlong.choice, swAlong.selector);
            for (int i = 0; i < caughtOptions.Length; i++)
            {
                var ans = g.Add(new RaidersAnswer(), 1780, 1180 + i * 110);
                g.Flow(swAlong.branches[i].Value, ans.enter);
                g.Set(ans.answer, caughtOptions[i].what);
                var told = g.Add(new TriggerCustomEvent { argumentCount = 1 }, 2000, 1180 + i * 110);
                g.Set(told.name, RaidersOutcomeEvent);
                g.Flow(ans.settled, told.enter);
                g.Data(ans.outcome, told.arguments[0]);
                var again = g.Add(new TriggerCustomEvent(), 2000, 1230 + i * 110);
                g.Set(again.name, RaidersAlongsideEvent);
                g.Flow(ans.unsettled, again.enter);
            }

            // 4. the outcome
            g.Group(-40, 1620, 1500, 360, "4. What came of it",
                "The facts (what was lost, what was paid, who escaped), then on with the voyage.", GraphBuilder.Change);
            var told2 = g.Add(new CustomEvent { coroutine = true, argumentCount = 1 }, 0, 1680);
            g.Set(told2.name, RaidersOutcomeEvent);
            var showOut = g.Add(new TextShow(), 260, 1680);
            g.Flow(told2.trigger, showOut.enter);
            g.Set(showOut.key, "encounter.outcome");
            g.Set(showOut.title, "What came of it");
            g.Data(told2.argumentPorts[0], showOut.intention);
            g.Set(showOut.frost, false);
            var goOn = g.Add(new TextAddOption(), 500, 1680);
            g.Flow(showOut.exit, goOn.enter);
            g.Set(goOn.key, "encounter.raiders.go_on");
            g.Set(goOn.intention, "Go on");
            var waitOut = g.Add(new TextWaitForChoiceTimed(), 720, 1680);
            g.Flow(goOn.exit, waitOut.enter);
            g.Set(waitOut.seconds, 30f);
            g.Set(waitOut.fallback, 0);
            var hide = g.Add(new TextHide(), 960, 1680);
            g.Flow(waitOut.exit, hide.enter);
            return g.Save($"{Folder}/Encounter - raiders.asset");
        }

        /// <summary>The king's men: a search as you cast off, or a guard boat's signal at sea (from 800); your answer; the
        /// outcome.</summary>
        public static ScriptGraphAsset EncounterKingsMen()
        {
            var g = new GraphBuilder("Encounter - the king's men",
                "The law at ports and at sea (Code/Game/Sim/Patrols.cs; Data/patrols_793.json; the laws in trade_rules.json). " +
                "Casting off from a port with officials, bound out of the realm, with goods under its law aboard: they may search. " +
                "From 800 (or with the table's test flag) guard boats near the watch stations signal ships at sea. Outcomes are " +
                "facts; deeds (bribes, flight) are kept for reputation.");
            g.Group(-40, -60, 2700, 760, "1. The king's men",
                "On the king's men -> in port: let them search / declare / offer a gift; at sea: heave to / dump overboard / run.",
                GraphBuilder.Talk);
            var on = g.Add(new OnKingsMen { coroutine = true }, 0, 0);
            var show = g.Add(new TextShow(), 260, 0);
            g.Flow(on.trigger, show.enter);
            g.Set(show.key, "encounter.officers");
            g.Set(show.title, "The king's men");
            g.Data(on.facts, show.intention);
            g.Set(show.frost, false);
            var sea = g.Add(new If(), 500, 0);
            g.Flow(show.exit, sea.enter);
            g.Data(on.atSea, sea.condition);
            var sets = new (ControlOutput from, float y, (string key, string intention, string what)[] options)[]
            {
                (sea.ifFalse, 0, new[] { ("encounter.officers.search", "Let them search", "search"), ("encounter.officers.declare", "Declare what you carry", "declare"),
                                       ("encounter.officers.gift", "Offer the officer a gift", "gift") }),
                (sea.ifTrue, 360, new[] { ("encounter.officers.heave_to", "Heave to", "heave_to"), ("encounter.officers.dump", "Dump the goods overboard", "dump"),
                                        ("encounter.officers.run", "Run", "run") }),
            };
            foreach (var (from, y, options) in sets)
            {
                ControlOutput last = from;
                for (int i = 0; i < options.Length; i++)
                {
                    var o = g.Add(new TextAddOption(), 740 + i * 200, y);
                    g.Flow(last, o.enter);
                    g.Set(o.key, options[i].key);
                    g.Set(o.intention, options[i].intention);
                    last = o.exit;
                }
                var wait = g.Add(new TextWaitForChoice(), 1360, y);          // (she's held: they wait for your answer)
                g.Flow(last, wait.enter);
                var sw = g.Add(new SwitchOnInteger { options = new List<int> { 0, 1, 2 } }, 1600, y);
                g.Flow(wait.exit, sw.enter);
                g.Data(wait.choice, sw.selector);
                for (int i = 0; i < options.Length; i++)
                {
                    var ans = g.Add(new OfficersAnswer(), 1840, y + i * 100);
                    g.Flow(sw.branches[i].Value, ans.enter);
                    if (i == 0)
                        g.Flow(sw.@default, ans.enter);
                    g.Set(ans.answer, options[i].what);
                    var told = g.Add(new TriggerCustomEvent { argumentCount = 1 }, 2080, y + i * 100);
                    g.Set(told.name, RaidersOutcomeEvent);                // (the same "what came of it" panel as raiders)
                    g.Flow(ans.exit, told.enter);
                    g.Data(ans.outcome, told.arguments[0]);
                }
            }
            g.Note(0, 760, 600, 160, "What came of it",
                "The outcome panel is the 'Raiders outcome' event in 'Encounter - raiders' (section 4): the facts, then Go on.");
            return g.Save($"{Folder}/Encounter - the king's men.asset");
        }

        /// <summary>Happenings at sea and in port, from the events table: shown, chosen, the outcome told.</summary>
        public static ScriptGraphAsset Events()
        {
            var g = new GraphBuilder("Events - at sea and in port",
                "Happenings from the table you edit: Game/Data/events_793.json (where, when, how likely a day, the options and " +
                "what each does). Once a game day one may come; it waits till nothing else is on (Code/Game/Events/GameEvents.cs). " +
                "Your writing for each goes in writing.json under its key ('event.<id>', 'event.<id>.<option>').");
            g.Group(-40, -60, 2600, 420, "1. An event comes",
                "On event -> its title and intention -> its options (from the table) -> your choice -> what it does -> the outcome.",
                GraphBuilder.Talk);
            var on = g.Add(new OnGameEvent { coroutine = true }, 0, 0);
            var show = g.Add(new TextShow(), 260, 0);
            g.Flow(on.trigger, show.enter);
            g.Data(on.key, show.key);
            g.Data(on.title, show.title);
            g.Data(on.intention, show.intention);
            var options = g.Add(new EventAddOptions(), 500, 0);
            g.Flow(show.exit, options.enter);
            var wait = g.Add(new TextWaitForChoice(), 720, 0);
            g.Flow(options.exit, wait.enter);
            var choose = g.Add(new EventChoose(), 960, 0);
            g.Flow(wait.exit, choose.enter);
            g.Data(wait.choice, choose.choice);
            var told = g.Add(new TextShow(), 1200, 0);
            g.Flow(choose.exit, told.enter);
            g.Set(told.key, "event.outcome");
            g.Set(told.title, "What came of it");
            g.Data(choose.outcome, told.intention);
            var goOn = g.Add(new TextAddOption(), 1440, 0);
            g.Flow(told.exit, goOn.enter);
            g.Set(goOn.key, "event.go_on");
            g.Set(goOn.intention, "Go on");
            var waitOut = g.Add(new TextWaitForChoiceTimed(), 1680, 0);
            g.Flow(goOn.exit, waitOut.enter);
            g.Set(waitOut.seconds, 30f);
            g.Set(waitOut.fallback, 0);
            var hide = g.Add(new TextHide(), 1920, 0);
            g.Flow(waitOut.exit, hide.enter);
            g.Note(0, 220, 900, 140, "Adding an event",
                "Add an entry to events_793.json: an id, where (sea / port / any), its chance a day, conditions, a title and an " +
                "intention, and options each with a key, a label and effects (see 'effects_help' at the top of the file).");
            return g.Save($"{Folder}/Events - at sea and in port.asset");
        }

        /// <summary>Stuck fast on the way: the ways to get her off, chosen, begun, told.</summary>
        public static ScriptGraphAsset AgroundGettingOff()
        {
            var g = new GraphBuilder("Aground - getting her off",
                "She's stuck fast on her course and the crew's own try at the oars hasn't freed her (Code/Game/Voyage/Aground.cs). " +
                "The clock stops while you choose: wait for the tide (only where a tide will float her soon enough; time runs " +
                "fast till it does), row her off, over the side and push her to water deep enough, or leave her for now. " +
                "The numbers are in voyage_rules.json ('aground'); your writing goes in writing.json under each key.");
            g.Group(-40, -60, 2600, 420, "1. Stuck fast",
                "On aground -> how she lies (facts) -> the ways there are -> your choice -> it's begun -> what's happening.",
                GraphBuilder.Talk);
            var on = g.Add(new OnAground { coroutine = true }, 0, 0);
            var show = g.Add(new TextShow(), 260, 0);
            g.Flow(on.trigger, show.enter);
            g.Set(show.key, "aground");
            g.Set(show.title, "Aground");
            g.Data(on.facts, show.intention);
            g.Set(show.frost, false);
            var options = g.Add(new AgroundAddOptions(), 500, 0);
            g.Flow(show.exit, options.enter);
            var wait = g.Add(new TextWaitForChoice(), 720, 0);
            g.Flow(options.exit, wait.enter);
            var choose = g.Add(new AgroundChoose(), 960, 0);
            g.Flow(wait.exit, choose.enter);
            g.Data(wait.choice, choose.choice);
            var told = g.Add(new TextShow(), 1200, 0);
            g.Flow(choose.exit, told.enter);
            g.Set(told.key, "aground.outcome");
            g.Set(told.title, "Aground");
            g.Data(choose.outcome, told.intention);
            g.Set(told.frost, false);
            var goOn = g.Add(new TextAddOption(), 1440, 0);
            g.Flow(told.exit, goOn.enter);
            g.Set(goOn.key, "aground.go_on");
            g.Set(goOn.intention, "Go on");
            var waitOut = g.Add(new TextWaitForChoiceTimed(), 1680, 0);
            g.Flow(goOn.exit, waitOut.enter);
            g.Set(waitOut.seconds, 8f);
            g.Set(waitOut.fallback, 0);
            var hide = g.Add(new TextHide(), 1920, 0);
            g.Flow(waitOut.exit, hide.enter);
            g.Note(0, 220, 1100, 160, "How it ends",
                "The tide floats her, they row her off, or they push her into deep enough water: a notice says so and she takes up " +
                "her course. If a try fails, or you leave her, she lies as she is (the crew try the oars now and then) and the " +
                "choice comes again after a while. Running aground at all is rarer now: the charted course keeps to deep water " +
                "where there's a way (Course.cs: DeepWaterM, ShallowCost).");
            return g.Save($"{Folder}/Aground - getting her off.asset");
        }

        /// <summary>The guide to the whole game: notes only. Open it first.</summary>
        public static ScriptGraphAsset GameMap()
        {
            var g = new GraphBuilder("00 Game map - start here",
                "Every system of the game, what it does, and where it lives. Notes only: open the graphs it names.");
            float x = 0, y = 0;
            void Box(string label, string comment, string body, Color color)
            {
                g.Group(x - 20, y - 60, 640, 420, label, comment, color);
                g.Note(x, y, 600, 340, label, body);
                x += 680;
                if (x > 2800)
                {
                    x = 0;
                    y += 500;
                }
            }
            Box("How the game is laid out", "Read me first",
                "The game's logic is in graphs on the '--- GAME ---' object in the world scene (W_World), one Script Machine each, " +
                "and in Assets/Kapman/Game/Graphs.\n\n" +
                "Data the graphs read: Assets/Kapman/Game/Data (ports_793.json ... more tables as systems come).\n" +
                "Your writing: Game/Data/writing.json (every text slot has a key; the game never writes story text).\n\n" +
                "Nodes: the game's own are in the fuzzy finder under Kapman/ (Text, Ports, Ship...) and Events/Kapman. " +
                "Everything in the game's code is a node too (Kapman ▸ Graphs ▸ Set up visual scripting).\n\n" +
                "Shared state: Object variables on '--- GAME ---' (Docked port, Leaving...).", GraphBuilder.Info);
            Box("Ports (built)", "Graphs: Ports - coming in and leaving, Ports - in port",
                "Arriving: she slows inside a port's reach -> lies to at its berth -> the port's menu opens (text).\n" +
                "Casting off: from the menu.\n\n" +
                "Data: Game/Data/ports_793.json (the trading places of 793).\n" +
                "Blockouts: Game/Ports/{id}.prefab - plain boxes named by what they are; replace them with your models.\n" +
                "In the scene under '--- PORTS ---' (a WorldAnchor pins each to its place; the Port component finds its shore " +
                "at the start - turn Auto Place off once you've placed one by hand).", GraphBuilder.Change);
            Box("Trade (built)", "Graphs: Trade - market, Trade - markets each day",
                "The goods of 793-1066: Game/Data/goods_793.json (140, each made and wanted by region, with season, " +
                "weight, bulk, the law on it and its sources).\n" +
                "Markets: stock and prices from supply and demand, seasons, tolls; money: deniers, pennies, dirhams " +
                "and weighed silver, each good in its own zone (Game/Data/trade_rules.json).\n" +
                "The merchant: purse, hold (the ship's cargo), ledger of prices seen, the debt to the count.\n" +
                "To move into the 2D map view (the 'nerd side'; the 3D view is for immersion).", GraphBuilder.Change);
            Box("The map (2D, built)", "Graph: Town - explore the town; [M] at sea",
                "Where the merchant's business is done (the 3D is for the eye). In port, 'explore the town' rises off " +
                "the ship into the map over the town, the market docked beside it. The map is drawn from the same " +
                "baked land as the 3D world (Code/Game/Map): silver sea, light land with its relief, coasts and rivers " +
                "in ink; the fog of war lifting as she sails; ports; the ship. Pick a good in the market and the map " +
                "marks where it's made and wanted. To come: routes, voyage planning, traders, weather.", GraphBuilder.Change);
            Box("Voyages", "To come: Graphs/Voyage",
                "Provisions, water and ale, spare gear; seasickness, fatigue, cold and wet, injury; leaks and wear; morale - " +
                "ticking with the game's hours (On game hour).", GraphBuilder.Info);
            Box("Events", "To come: Graphs/Events",
                "Happenings at sea and in port: conditions, options (intentions) and outcomes, from a table; your text for each.", GraphBuilder.Info);
            Box("Traders and encounters (built: traders)", "Graphs: Sim - the world, Encounter - a trader",
                "The world's traders (Code/Game/Sim): ~140 ships with their own aims, buying and selling in the same markets " +
                "(prices move), carrying news, keeping clear of each other (Data/seamanship_793.json). The lookout (SeaWatch) " +
                "sights them, makes them out, loses them (the sea log). Meeting one: close and hail, the weather she met, the " +
                "prices she knows. To come: raiders, Frankish officers, Saxons; searches for swords and captives by each realm's law.", GraphBuilder.Change);
            Box("Reputation", "To come: Graphs/Reputation",
                "Standing with each settlement and each region, apart: trust, and moral standing by each people's own values " +
                "(slaving, raiding, piety...), spreading along the trade routes.", GraphBuilder.Info);
            Box("Collectables and reading", "To come: Graphs/Texts",
                "Real texts of the time to find and read: the original and a public-domain translation (the Havamal, Bede, " +
                "Alcuin's letters of 793, the Frisian law, the Rok stone...).", GraphBuilder.Info);
            Box("The engine (C#, being moved to graphs)", "Code/Land, Code/Sailing, Code/HDRP, Code/Weather, Code/Build",
                "Still C# for now, to be moved into graphs one by one after the game loop: the land (streaming the baked tiles), " +
                "the sea and the ship (sailing by the wind, the wake), the weather and tides, the cameras, Build mode. " +
                "Heavy inner work will stay inside single named nodes.", GraphBuilder.Info);
            return g.Save($"{Folder}/00 Game map - start here.asset");
        }
    }
}
