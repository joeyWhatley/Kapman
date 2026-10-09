using System;
using System.Linq;
using Kapman.Encounters;
using Kapman.Levels;

namespace Kapman.Game
{
    /// <summary>
    /// THE LIVE GAME'S VARIABLES for the cards' conditions (npc_text_encounters.md 2.4): the purse in the port's money, the
    /// debt, the standing, the crew's numbers, the date, the weather, the hold, the chest. The encounter layer's own (flags,
    /// the port's id and calls, the people's feelings and memories) are EncounterVars', and the port's realm, people, faith and
    /// mint, "moored", "under_way", "year" and "month" the director's; this answers the rest when a card is drawn or opened.
    /// What the game has no model of yet reads as nothing (0 or ""), except a crewman's best at a skill (crew.&lt;x&gt;_best),
    /// which reads 3, the middle of the scale, so a chance by it stays as written until the crew are people with stats.
    /// "fog" is the level's weather kind as a share (fog 1, mist 0.6, haze 0.3), not the HDRP fog density.
    /// </summary>
    public static class EncounterGameVars
    {
        /// <summary>The middle of a crewman's 1-5 stat: a chance by it is as written (Effects: p + 0.05 x (stat - 3)).</summary>
        public const double MiddleStat = 3.0;

        /// <summary>Arms, as the law and the cards count them (the swords the toll man asks about).</summary>
        public static bool IsWeapon(GoodDef g) => g != null && (g.law == "arms" || g.cat == "Weapons");

        /// <summary>People for sale.</summary>
        public static bool IsCaptive(GoodDef g) => g != null && (g.law == "captives" || g.cat == "Captives");

        /// <summary>Hooks a card's variables to the live game, at the port the function names ("" or null at sea).</summary>
        public static void Attach(EncounterVars vars, Func<string> portNow)
        {
            if (vars == null)
                return;
            vars.numSource = name => Num(name, GameData.Port(portNow != null ? portNow() : null));
            vars.textSource = name => Text(name, GameData.Port(portNow != null ? portNow() : null));
            vars.flagSource = name => Boat.FlagView(name, Today);          // (the boat's flags are the boat: Voyage/Boat.cs)
        }

        static double Today => KapmanGame.Current != null ? KapmanGame.Current.Day : 0.0;

        /// <summary>A number variable from the live game (0 for what it has no model of).</summary>
        public static double Num(string name, PortDef port)
        {
            if (string.IsNullOrEmpty(name))
                return 0.0;
            var game = KapmanGame.Current;
            var rules = TradeRules.Current;
            var weather = LevelWeather.Current;
            switch (name)
            {
                case "silver":
                    if (!Trade.Started)
                        return 0.0;
                    return port != null ? Trade.Purse.Worth(rules.ZoneOf(port.region), rules) : Trade.Purse.Silver(rules);
                case "debt":
                    return Trade.Started ? Trade.Debt : 0.0;
                case "debt_short":
                case "debt_weight_short":
                case "debt_quarter_short":
                {
                    // how far the silver aboard falls short of the debt (0: it can be met): all of it, by weight in the
                    // new penny, or the quarter a surety asks. The debt cards ask this, not a sum written in them.
                    if (!Trade.Started)
                        return 0.0;
                    double have = port != null ? Trade.Purse.Worth(rules.ZoneOf(port.region), rules) : Trade.Purse.Silver(rules);
                    double share = name == "debt_weight_short" ? GameEffects.DebtByWeight : name == "debt_quarter_short" ? GameEffects.DebtSurety : 1.0;
                    return Math.Max(0.0, Math.Round(Trade.Debt * share) - have);
                }
                case "debt_due_days":
                    return Trade.Started ? Trade.DebtDaysLeft(Today) : 0.0;
                case "standing":
                case "standing_word":
                    return Reputation.At(port);
                case "standing_region":
                    return port != null ? Reputation.OfRegion(port.region) : 0.0;
                case "crew":
                    return Voyage.Men;
                case "health":
                    return Voyage.Health;
                case "heart":
                    return Voyage.Morale;
                case "days_at_sea":
                    return Voyage.DaysAtSea;
                case "gale":
                    return Boat.InGale ? 1.0 : 0.0;
                case "gale_in":
                    return double.IsInfinity(Boat.GaleIn) || double.IsNaN(Boat.GaleIn) ? 99.0 : Boat.GaleIn;
                case "boat.hull":
                    return Boat.Hull;
                case "boat.water":
                    return Boat.Water;
                case "boat.weary":
                    return Boat.Weary;
                case "year":
                    return game != null ? game.Year : 793;
                case "month":
                    return game != null ? game.Month : 6;
                case "hour":
                    return game != null && game.clock != null ? game.clock.HourOfDay : 12.0;
                // (7 Oct: on the chart there is no level's weather: the wind, the storm and the fog are the world's own
                // where she is, WorldSim.WeatherAt, the same the chart's ship sails by)
                case "wind":
                    return weather != null ? weather.Wind : ChartWorld.TryWeather(out var wind) ? wind.windSpeed : 0.0;
                case "fog":
                    return weather != null ? Fog(weather) : ChartWorld.TryWeather(out var fog) ? fog.fog : 0.0;
                case "storm":
                    return weather != null ? weather.Storm : ChartWorld.TryWeather(out var storm) ? storm.storm : 0.0;
                case "ice":
                    return weather != null ? weather.Ice : 0.0;
                case "feast":
                    return FeastToday(game) ? 1.0 : 0.0;
                case "port.toll":
                    return port != null && rules.ZoneOf(port.region) != null ? rules.ZoneOf(port.region).toll : 0.0;
                case "hold.weapons":
                    return Aboard(IsWeapon);
                case "hold.captives":
                    return Aboard(IsCaptive);
                case "hold.value":
                    return HoldValue();
                case "learned.latin":
                    return Chest.KnowsLatin ? 1.0 : 0.0;
                case "chest.latin_count":
                    return Chest.Items.Count(i => i.Text != null && Chest.NeedsLearning(i.Text.language));
                case "chest.riddles_solved":
                    return Chest.Items.Count(i => i.kind == "riddle" && i.solved);
            }
            if (name.StartsWith("goons.", StringComparison.Ordinal))
                return Collectors.Var(name);                              // (the lender's men: Voyage/Collectors.cs)
            if (name.StartsWith("feud.", StringComparison.Ordinal))
                return Feud.Var(name);                                    // (Ravn's death and the feud: Voyage/Feud.cs)
            if (name.StartsWith("hold.from_", StringComparison.Ordinal))
                return 0.0;                                               // (where a lot was bought: not kept by the hold yet)
            if (name.StartsWith("hold.", StringComparison.Ordinal))
                return Trade.Started ? Trade.Hold.Count(name.Substring(5)) : 0.0;
            if (name.StartsWith("text.", StringComparison.Ordinal))
                return Chest.Has(name.Substring(5)) ? 1.0 : 0.0;
            if (name.StartsWith("crew.", StringComparison.Ordinal) && name.EndsWith("_best", StringComparison.Ordinal))
                return MiddleStat;
            // the self (7 Oct 2026): a stat in pips, 1 to 5, and how well a tongue is followed, 0 to 1
            if (name.StartsWith("mark.", StringComparison.Ordinal))
                return Self.HasMark(name.Substring(5)) ? 1.0 : 0.0;
            if (name.StartsWith("me.", StringComparison.Ordinal))
            {
                // (the stat as it is now, marks and all: what a choice asks for; ".base" is what lies under the marks)
                string stat = name.Substring(3);
                if (stat.EndsWith(".base", StringComparison.Ordinal))
                {
                    stat = stat.Substring(0, stat.Length - 5);
                    return Self.IsStat(stat) ? Self.Stat(stat) : 0.0;
                }
                return Self.IsStat(stat) ? Self.StatNow(stat) : 0.0;
            }
            if (name.StartsWith("tongue.", StringComparison.Ordinal))
                return Self.TongueLevel(name.Substring(7));
            return 0.0;
        }

        /// <summary>A text variable from the live game ("" for what it has no model of).</summary>
        public static string Text(string name, PortDef port)
        {
            if (string.IsNullOrEmpty(name))
                return "";
            switch (name)
            {
                case "level_kind":
                    if (LevelInfo.Current == null)                         // (the chart: the water she is on, a river or the sea)
                        return ChartWorld.Current == null ? "" : ChartWorld.OnRiver() ? "river_place" : "ocean";
                    return LevelInfo.Current.kind == LevelKind.Ocean ? "ocean" : "river_place";
                case "port.id":
                    return port != null ? port.id ?? "" : "";             // (EncounterVars answers it from the call's port; the same here)
                case "port.kind":
                    return port != null ? port.kind ?? "" : "";
                case "port.realm":
                    return port != null ? port.realm ?? "" : "";
                case "boat.sail":
                    return Boat.Cloth == SailCloth.Whole ? "whole" : Boat.Cloth == SailCloth.Weak ? "weak" : "split";
                case "me.people":
                    return Self.People;
                case "me.faith":
                    return Self.Faith;
                case "me.past":
                    return Self.Past;
            }
            if (name.StartsWith("riddle.", StringComparison.Ordinal))
            {
                string id = name.Substring(7);
                foreach (var i in Chest.Items)
                    if (i.kind == "riddle" && i.def == id)
                        return i.solved ? "solved" : i.resting ? "resting" : "open";
            }
            return "";
        }

        /// <summary>Units aboard of the goods that answer a test.</summary>
        static double Aboard(Func<GoodDef, bool> which)
        {
            if (!Trade.Started)
                return 0.0;
            int n = 0;
            foreach (var (good, count) in Trade.Hold.Contents)
                if (which(good))
                    n += count;
            return n;
        }

        /// <summary>What the hold carries, at the goods' table values (deniers).</summary>
        static double HoldValue()
        {
            if (!Trade.Started)
                return 0.0;
            double v = 0.0;
            foreach (var (good, count) in Trade.Hold.Contents)
                v += count * (double)good.value;
            return v;
        }

        static double Fog(LevelWeather w)
        {
            if (w == null)
                return 0.0;
            switch (w.kind)
            {
                case WeatherKind.Fog: return 1.0;
                case WeatherKind.Mist: return 0.6;
                case WeatherKind.Haze: return 0.3;
                default: return 0.0;
            }
        }

        static bool FeastToday(KapmanGame game)
        {
            if (game == null || game.clock == null)
                return false;
            var d = game.clock.Date;
            return Feasts.On(d.year, d.month, d.day).HasValue;
        }
    }
}
