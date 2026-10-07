using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>An option of an event: its key (for your writing), its label, and what it does (effects).</summary>
    public sealed class EventOption
    {
        public string key, label;
        public Dictionary<string, object> effects;
    }

    /// <summary>A happening (Data/events_793.json): where, when, how likely a day, and its options.</summary>
    public sealed class EventDef
    {
        public string id, where, title, intention;
        public double chance;
        public Dictionary<string, object> conditions;
        public readonly List<EventOption> options = new List<EventOption>();
        public string Key => "event." + id;
    }

    /// <summary>
    /// EVENTS - happenings at sea and in port, from a table you edit (Data/events_793.json; the user's spec: "Events:
    /// text-based, placeholders"). Once a game day each event whose conditions hold (where she is, the month, days at
    /// sea, the crew's health and heart, the wind and fog, what's aboard, the kind of port) may come, by its chance; it
    /// waits until nothing else is on, then the graph "Events - at sea and in port" shows it: its label, its intention,
    /// its options. The option chosen does what its effects say - health, heart, men, silver, goods, water, food spoiled,
    /// a deed, or a chance of one outcome or another - and the outcome is told as facts. No story: the keys are yours.
    /// </summary>
    public static class GameEvents
    {
        public static readonly List<EventDef> All = new List<EventDef>();
        /// <summary>The event showing now (null: none).</summary>
        public static EventDef Current { get; private set; }
        /// <summary>The graphs' "On event": an event is to be shown.</summary>
        public static event Action<EventDef> Due;
        /// <summary>Someone else shows the events (the chart's frame: each as a floating window of its own, so one may
        /// come while the port's menu is up, and never fights it for the text panel). True: it is shown, and its choice
        /// comes back through <see cref="Choose"/>. False: not now (something else wants an answer): it waits.
        /// Null: the graph "Events - at sea and in port" shows it in the text panel, as before.</summary>
        public static Func<EventDef, bool> Show;
        /// <summary>An event is waiting to be shown (a test, a tool).</summary>
        public static bool Waiting => pending != null;

        static EventDef pending;
        static readonly Dictionary<string, double> lastDay = new Dictionary<string, double>();
        static System.Random dice = new System.Random(793);

        public static void Load(TextAsset json)
        {
            All.Clear();
            if (json == null)
                return;
            foreach (var x in Json.Arr(Json.Parse(json.text), "events") ?? new List<object>())
            {
                var e = new EventDef
                {
                    id = Json.Str(x, "id"), where = Json.Str(x, "where", "any"), title = Json.Str(x, "title"), intention = Json.Str(x, "intention"),
                    chance = Json.Num(x, "chance", 0.05), conditions = Json.Obj(x, "conditions") ?? new Dictionary<string, object>(),
                };
                foreach (var o in Json.Arr(x, "options") ?? new List<object>())
                    e.options.Add(new EventOption { key = Json.Str(o, "key"), label = Json.Str(o, "label"), effects = Json.Obj(o, "effects") });
                if (!string.IsNullOrEmpty(e.id) && e.options.Count > 0)
                    All.Add(e);
            }
        }

        /// <summary>Once a game day: an event may come (the first, in the table's order, whose chance falls).</summary>
        public static void Daily(int day)
        {
            var game = KapmanGame.Current;
            if (game == null || pending != null || Current != null)
                return;
            dice = new System.Random((game.World != null ? game.World.seed : 793) * 397 + day);
            foreach (var e in All)
            {
                double cool = Json.Num(e.conditions, "cooldown_days", 20.0);
                if (lastDay.TryGetValue(e.id, out double last) && game.Day - last < cool)
                    continue;
                if (!Holds(e, game) || dice.NextDouble() >= e.chance)
                    continue;
                pending = e;
                lastDay[e.id] = game.Day;
                return;
            }
        }

        /// <summary>Brings an event on now, whatever its conditions (to try one: your writing, its options).</summary>
        public static bool Force(string id)
        {
            var e = All.Find(x => x.id == id);
            if (e == null || Current != null)
                return false;
            pending = e;
            return true;
        }

        /// <summary>Each frame: a pending event is shown as soon as nothing else is on.</summary>
        public static void Look()
        {
            if (pending == null || Current != null)
                return;
            if (MapView.AwayFromSea)
            {
                Alerts.EventWaiting(pending);                               // (it waits for you; the map shows it)
                return;
            }
            var text = TextPanel.Current;
            // (shown in the text panel it waits for the panel to be free; shown by the frame it may come over the port's
            // own menu, and waits only for a talk at sea)
            if (Show == null ? (text == null || text.Open) : (text != null && text.Open && !text.Key.StartsWith("port.", StringComparison.Ordinal)))
                return;
            if (Sim.Encounter.Stage != Sim.EncounterStage.None || Sim.RaiderEncounter.Stage != Sim.RaidStage.None
                || Sim.Patrols.Stage != Sim.PatrolStage.None)
                return;
            var e = pending;
            Current = e;
            pending = null;
            if (Show == null)
                Due?.Invoke(e);
            else if (!Show(e))
            {
                Current = null;                                             // (not now: it waits)
                pending = e;
            }
        }

        /// <summary>A saved game was loaded: an event that was showing, or waiting, was of the game before.</summary>
        public static void Loaded()
        {
            Current = null;
            pending = null;
        }

        /// <summary>Do an event's conditions hold now?</summary>
        public static bool Holds(EventDef e, KapmanGame game)
        {
            var c = e.conditions;
            var port = Port.Nearest(game.ShipPosition);
            bool inPort = game.ship != null && game.ship.anchored && port != null && port.DistanceTo(game.ShipPosition) < port.arrivalRadius;
            if (e.where == "sea" && inPort || e.where == "port" && !inPort)
                return false;
            var months = Json.Arr(c, "months");
            if (months != null && !months.Contains((double)game.Month))
                return false;
            if (Voyage.DaysAtSea < Json.Num(c, "min_days_at_sea", 0))
                return false;
            if (Voyage.Health >= Json.Num(c, "health_below", 2.0) || Voyage.Morale >= Json.Num(c, "morale_below", 2.0))
                return false;
            var weather = Kapman.Weather.LocalWeather.Active;
            float wind = weather != null ? weather.Now.windSpeed : 6f, fog = weather != null ? weather.FogHere : 0f;   // (the bank she's in, not the fog round about)
            if (weather == null && ChartWorld.Current != null && ChartWorld.TryWeather(out var air))
            {
                // (the chart has no level and so no local weather: the air where she is, from the model the world's ships
                // sail by. Without this a calm, a fog and a gale never came on the chart: the wind read 6 m/s for ever)
                wind = air.windSpeed;
                fog = air.fog;
            }
            if (wind <= Json.Num(c, "wind_above", -1.0) || (Json.Bool(c, "calm") && wind >= 1.5f) || fog <= Json.Num(c, "fog_above", -1.0))
                return false;
            var goods = Json.Arr(c, "has_goods");
            if (goods != null && (!Trade.Started || !goods.Exists(g => g is string id && Trade.Hold.Count(id) > 0)))
                return false;
            var kinds = Json.Arr(c, "port_kinds");
            if (kinds != null && (port == null || !kinds.Contains(port.Def?.kind)))
                return false;
            var regions = Json.Arr(c, "regions");
            if (regions != null && (port == null || !regions.Contains(port.Def?.region)))
                return false;
            if (game.Year < Json.Num(c, "year_from", 0) || game.Year > Json.Num(c, "year_to", 99999))
                return false;
            return true;
        }

        /// <summary>The option chosen: its effects done, the outcome as facts.</summary>
        public static string Choose(int choice)
        {
            var e = Current;
            Current = null;
            if (e == null || choice < 0 || choice >= e.options.Count)
                return "";
            string fact = Apply(e.options[choice].effects);
            if (!string.IsNullOrEmpty(fact))
                Voyage.Say(fact);
            return fact;
        }

        /// <summary>Does what effects say; returns the fact (its "text", with {n} the amount the effect used).</summary>
        public static string Apply(Dictionary<string, object> fx)
        {
            if (fx == null)
                return "";
            if (fx.ContainsKey("chance"))
                return Apply(dice.NextDouble() < Json.Num(fx, "chance", 0.5) ? Json.Obj(fx, "success") : Json.Obj(fx, "failure"));
            var game = KapmanGame.Current;
            double n = 0.0;
            bool noRoom = false;
            Voyage.Affect(Json.Num(fx, "health"), Json.Num(fx, "morale"));
            int men = (int)Json.Num(fx, "men");
            if (men < 0)
                n = Voyage.LoseMen(-men);
            else if (men > 0)
                Voyage.Hire(men);
            double water = Json.Num(fx, "water");
            if (water != 0.0)
                n = Math.Abs(Voyage.AddWater(water));
            double spoil = Json.Num(fx, "spoil_food");
            if (spoil > 0.0)
                n = Voyage.Spoil(spoil);
            double silver = Json.Num(fx, "silver");
            if (silver != 0.0 && Trade.Started)
            {
                var port = game != null ? Port.Nearest(game.ShipPosition)?.Def : null;
                var zone = port != null ? TradeRules.Current.ZoneOf(port.region) : null;
                if (silver > 0.0)
                    Trade.Purse.Receive(silver, zone, TradeRules.Current, out _);
                else
                {
                    double have = Trade.Purse.Silver(TradeRules.Current);
                    if (have > 0.0)
                        Trade.Purse.PayUpTo(-silver, TradeRules.Current);
                }
                n = Math.Abs(silver);
            }
            foreach (var kv in Json.Obj(fx, "goods") ?? new Dictionary<string, object>())
            {
                var g = GameData.Good(kv.Key);
                int k = kv.Value is double d ? (int)d : 0;
                if (g == null || k == 0 || !Trade.Started)
                    continue;
                if (k > 0)
                {
                    int take = Math.Min(k, Trade.Hold.RoomFor(g));
                    Trade.Hold.Add(g, take, 0.0, game != null ? game.Day : 0.0);
                    noRoom |= take < k;                                   // (the fact must not say "food for days" over a full hold)
                }
                else
                    Trade.Hold.Remove(g, Math.Min(-k, Trade.Hold.Count(g.id)));
                n = Math.Abs(k);
            }
            string deed = Json.Str(fx, "deed", null), kind = Json.Str(fx, "standing", null);
            if (!string.IsNullOrEmpty(kind))
                Sim.Deeds.Record(kind, deed ?? kind, Sim.Deeds.NearestPlace());    // (his standing there changes by it)
            else if (!string.IsNullOrEmpty(deed))
                Sim.Deeds.Record(deed, Sim.Deeds.NearestPlace()?.name);
            return Json.Str(fx, "text").Replace("{n}", n.ToString("0")) + (noRoom ? " But the hold is full: there was no room aboard for all of it." : "");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearState()
        {
            Current = null;
            pending = null;
            Show = null;
            lastDay.Clear();
        }
    }
}
