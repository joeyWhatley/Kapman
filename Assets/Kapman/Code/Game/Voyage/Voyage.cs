using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kapman.Game
{
    // ported 4 Oct: unchanged from the first build (no world or ship in it: the crew, their stores and their pay).
    // The rules are read from a TextAsset (voyage_rules.json in Assets/Kapman/Game/Data), handed in by KapmanGame.

    /// <summary>The rules of a voyage (Data/voyage_rules.json): the crew's keep and pay, their stores, the sea's toll.</summary>
    public sealed class VoyageRules
    {
        public int men = 4;
        public double pay = 0.25, leaveIfOwedDays = 14;
        public double foodKg = 0.7, drinkL = 3.0, litresPerKg = 1.0;
        public string[] foodGoods = { "ships_bread", "cheese", "dried_fish" }, drinkGoods = { "ale" };
        public int casks = 4;
        public double litresPerCask = 60, kgPerLitre = 1.0;
        public double hungerPerDay = 0.08, thirstPerDay = 0.25, recoverPerDay = 0.05;
        public double seasickWaveM = 1.2, seasickPerDay = 0.04, seaLegsDays = 6;
        public double unpaidPerDay = 0.02, hungryPerDay = 0.1, thirstyPerDay = 0.15, recoverInPort = 0.06;
        // aground (Aground.cs): when the choice comes, and what getting her off takes
        public double agroundOfferMinutes = 15, agroundReofferMinutes = 60, tideLookHours = 14, tideWaitRate = 3600;
        public double rowOffMinutes = 30, pushReachLengths = 12, pushSpeed = 0.3, pushMargin = 1.3;
        public double rowHealth = -0.01, rowMorale = -0.01, pushHealth = -0.02, pushMorale = -0.03, pushColdHealth = -0.04;
        public double waitMorale = -0.01;

        public double WaterCapacityL => casks * litresPerCask;

        public static VoyageRules Load(TextAsset json)
        {
            var r = new VoyageRules();
            if (json == null)
                return r;
            var o = Json.Parse(json.text);
            var crew = Json.Obj(o, "crew");
            r.men = (int)Json.Num(crew, "men", r.men);
            r.pay = Json.Num(crew, "pay_per_man_day", r.pay);
            r.leaveIfOwedDays = Json.Num(crew, "leave_if_owed_days", r.leaveIfOwedDays);
            var rations = Json.Obj(o, "rations");
            r.foodKg = Json.Num(rations, "food_kg_per_man_day", r.foodKg);
            r.drinkL = Json.Num(rations, "drink_l_per_man_day", r.drinkL);
            r.litresPerKg = Json.Num(rations, "liquid_l_per_kg", r.litresPerKg);
            r.foodGoods = Strings(Json.Arr(rations, "food_goods"), r.foodGoods);
            r.drinkGoods = Strings(Json.Arr(rations, "drink_goods"), r.drinkGoods);
            var water = Json.Obj(o, "water");
            r.casks = (int)Json.Num(water, "casks", r.casks);
            r.litresPerCask = Json.Num(water, "litres_per_cask", r.litresPerCask);
            r.kgPerLitre = Json.Num(water, "kg_per_litre", r.kgPerLitre);
            var h = Json.Obj(o, "health");
            r.hungerPerDay = Json.Num(h, "hunger_per_day", r.hungerPerDay);
            r.thirstPerDay = Json.Num(h, "thirst_per_day", r.thirstPerDay);
            r.recoverPerDay = Json.Num(h, "recover_per_day", r.recoverPerDay);
            r.seasickWaveM = Json.Num(h, "seasick_wave_m", r.seasickWaveM);
            r.seasickPerDay = Json.Num(h, "seasick_per_day", r.seasickPerDay);
            r.seaLegsDays = Json.Num(h, "sea_legs_days", r.seaLegsDays);
            var m = Json.Obj(o, "morale");
            r.unpaidPerDay = Json.Num(m, "unpaid_per_day", r.unpaidPerDay);
            r.hungryPerDay = Json.Num(m, "hungry_per_day", r.hungryPerDay);
            r.thirstyPerDay = Json.Num(m, "thirsty_per_day", r.thirstyPerDay);
            r.recoverInPort = Json.Num(m, "recover_in_port_per_day", r.recoverInPort);
            var a = Json.Obj(o, "aground");
            r.agroundOfferMinutes = Json.Num(a, "offer_after_minutes", r.agroundOfferMinutes);
            r.agroundReofferMinutes = Json.Num(a, "again_after_minutes", r.agroundReofferMinutes);
            r.tideLookHours = Json.Num(a, "tide_look_hours", r.tideLookHours);
            r.tideWaitRate = Json.Num(a, "waiting_rate", r.tideWaitRate);
            r.rowOffMinutes = Json.Num(a, "row_minutes", r.rowOffMinutes);
            r.pushReachLengths = Json.Num(a, "push_reach_lengths", r.pushReachLengths);
            r.pushSpeed = Json.Num(a, "push_speed_ms", r.pushSpeed);
            r.pushMargin = Json.Num(a, "push_to_depth", r.pushMargin);
            r.rowHealth = Json.Num(a, "row_health", r.rowHealth);
            r.rowMorale = Json.Num(a, "row_morale", r.rowMorale);
            r.pushHealth = Json.Num(a, "push_health", r.pushHealth);
            r.pushMorale = Json.Num(a, "push_morale", r.pushMorale);
            r.pushColdHealth = Json.Num(a, "push_cold_health", r.pushColdHealth);
            r.waitMorale = Json.Num(a, "wait_morale", r.waitMorale);
            return r;
        }

        static string[] Strings(List<object> list, string[] fallback) =>
            list == null || list.Count == 0 ? fallback : list.ConvertAll(s => s as string ?? "").ToArray();
    }

    /// <summary>
    /// THE VOYAGE - the player's crew and their keep: each day aboard they eat and drink from the stores in the hold (food
    /// and ale bought in markets; water put aboard free in any port, in the casks), are owed their pay (paid out in port
    /// from the purse, as the world's crews are), and the sea works on them (seasick in rough water till they find their
    /// sea legs). Hungry, thirsty or unpaid, their health and heart fall; men owed too long may go ashore for good.
    /// Everything that happens is told as a plain fact (Notice) and to the graphs ("Voyage - each day").
    /// </summary>
    public static class Voyage
    {
        public static VoyageRules Rules { get; private set; } = new VoyageRules();
        public static int Men { get; private set; }
        /// <summary>0-1: the crew's health and heart.</summary>
        public static double Health { get; private set; } = 1.0;
        public static double Morale { get; private set; } = 0.8;
        public static double WaterL { get; private set; }
        public static double PayOwed { get; private set; }
        public static int DaysAtSea { get; private set; }
        public static double Seasick { get; private set; }
        static double openFoodKg, openDrinkL;                        // a sack or a cask broached and part eaten

        /// <summary>A plain fact about the voyage ("The ale is gone."): for the notices line.</summary>
        public static event Action<string> Notice;

        /// <summary>Tells a plain fact to the notices line (the course, the voyage planner).</summary>
        public static void Say(string fact) => Notice?.Invoke(fact);

        /// <summary>The crew and their stores as plain data (a saved game).</summary>
        public static Dictionary<string, object> Capture() => new Dictionary<string, object>
        {
            ["men"] = Men, ["health"] = Health, ["morale"] = Morale, ["water"] = WaterL, ["owed"] = PayOwed, ["days"] = DaysAtSea,
            ["seasick"] = Seasick, ["food_open"] = openFoodKg, ["drink_open"] = openDrinkL,
        };

        public static void Restore(Dictionary<string, object> d)
        {
            if (d == null)
                return;
            Men = (int)Json.Num(d, "men", Men);
            Health = Json.Num(d, "health", Health);
            Morale = Json.Num(d, "morale", Morale);
            WaterL = Json.Num(d, "water", WaterL);
            PayOwed = Json.Num(d, "owed");
            DaysAtSea = (int)Json.Num(d, "days");
            Seasick = Json.Num(d, "seasick");
            openFoodKg = Json.Num(d, "food_open");
            openDrinkL = Json.Num(d, "drink_open");
        }

        /// <summary>Health and heart changed by these shares, either way (an event), kept within 0-1.</summary>
        public static void Affect(double health, double heart)
        {
            Health = Math.Max(0.0, Math.Min(1.0, Health + health));
            Morale = Math.Max(0.0, Math.Min(1.0, Morale + heart));
        }

        /// <summary>Men taken on (an event, a port).</summary>
        public static void Hire(int n) => Men += Math.Max(0, n);
        /// <summary>The men aboard set outright (the first jobs begin with one man, Ravn). Never fewer than one.</summary>
        public static void SetMen(int n) => Men = Math.Max(1, n);

        /// <summary>Water in the casks changed (litres, either way), within the casks.</summary>
        public static double AddWater(double litres)
        {
            double before = WaterL;
            WaterL = Math.Max(0.0, Math.Min(Rules.WaterCapacityL, WaterL + litres));
            return WaterL - before;
        }

        /// <summary>A share of the food aboard spoiled (a leak, the sea): the kg lost.</summary>
        public static double Spoil(double share)
        {
            if (!Trade.Started || share <= 0.0)
                return 0.0;
            double kg = openFoodKg * share;
            openFoodKg -= kg;
            foreach (var id in Rules.foodGoods)
            {
                var g = GameData.Good(id);
                int n = g != null ? (int)Math.Ceiling(Trade.Hold.Count(id) * share) : 0;
                if (n <= 0)
                    continue;
                Trade.Hold.Remove(g, n);
                kg += n * g.kg;
            }
            return kg;
        }

        /// <summary>A rest ashore (lodging, a guest house, alms given): health and heart up by these shares.</summary>
        public static void Rest(double health, double heart)
        {
            Health = Math.Min(1.0, Health + health);
            Morale = Math.Min(1.0, Morale + heart);
        }

        /// <summary>Men lost (killed or taken: a fight, raiders); their heart falls with them. Never below one man.</summary>
        public static int LoseMen(int n)
        {
            int lost = Math.Max(0, Math.Min(n, Men - 1));
            Men -= lost;
            if (lost > 0)
                Morale = Math.Max(0.0, Morale - 0.12 * lost);
            return lost;
        }

        public static bool Started { get; private set; }

        public static void Begin(VoyageRules rules)
        {
            Rules = rules ?? new VoyageRules();
            Men = Rules.men;
            Health = 1.0;
            Morale = 0.8;
            WaterL = Rules.WaterCapacityL;
            PayOwed = 0.0;
            DaysAtSea = 0;
            Seasick = 0.0;
            openFoodKg = openDrinkL = 0.0;
            Started = true;
        }

        /// <summary>What's aboard to eat, in kg (the hold's food goods and what's broached).</summary>
        public static double FoodKg => openFoodKg + Aboard(Rules.foodGoods, kg: true);
        /// <summary>What's aboard to drink, in litres (water and ale).</summary>
        public static double DrinkL => WaterL + openDrinkL + Aboard(Rules.drinkGoods, kg: true) * Rules.litresPerKg;
        public static double FoodDays => Men > 0 ? FoodKg / (Men * Rules.foodKg) : 0.0;
        public static double DrinkDays => Men > 0 ? DrinkL / (Men * Rules.drinkL) : 0.0;

        static double Aboard(string[] goods, bool kg)
        {
            if (!Trade.Started)
                return 0.0;
            double sum = 0.0;
            foreach (var id in goods)
            {
                var g = GameData.Good(id);
                if (g != null)
                    sum += Trade.Hold.Count(id) * (kg ? g.kg : 1.0);
            }
            return sum;
        }

        /// <summary>A day of the voyage. atSea: under way (not lying in a port); waveM: the sea's wave height (real m).</summary>
        public static void Day(bool atSea, double waveM)
        {
            if (!Started || Men <= 0)
                return;
            var r = Rules;
            // eat
            bool fed = Take(r.foodGoods, Men * r.foodKg, ref openFoodKg, 1.0, out _);
            // drink: water first, then ale
            double thirst = Men * r.drinkL;
            double fromWater = Math.Min(WaterL, thirst);
            WaterL -= fromWater;
            thirst -= fromWater;
            bool watered = thirst <= 1e-6 || Take(r.drinkGoods, thirst, ref openDrinkL, r.litresPerKg, out _);
            if (WaterL <= 0.0 && fromWater > 0.0)
                Notice?.Invoke("The water casks are empty.");
            // pay
            PayOwed += Men * r.pay;
            // the sea
            if (atSea)
            {
                DaysAtSea++;
                if (waveM > r.seasickWaveM)
                {
                    Seasick = Math.Max(0.0, 1.0 - DaysAtSea / Math.Max(1.0, r.seaLegsDays));
                    Morale -= r.seasickPerDay * Seasick * Math.Min(2.0, waveM / r.seasickWaveM);
                    if (Seasick > 0.3)
                        Notice?.Invoke("The sea is rough; some of the men are seasick.");
                }
                else
                    Seasick *= 0.5;
            }
            // health and heart
            if (!fed)
            {
                Health -= r.hungerPerDay;
                Morale -= r.hungryPerDay;
                Notice?.Invoke("There is no food aboard. The men go hungry.");
            }
            if (!watered)
            {
                Health -= r.thirstPerDay;
                Morale -= r.thirstyPerDay;
                Notice?.Invoke("There is nothing to drink aboard.");
            }
            if (fed && watered)
                Health += r.recoverPerDay;
            if (PayOwed > Men * r.pay * 7.0)
                Morale -= r.unpaidPerDay;
            if (!atSea)
                Morale += r.recoverInPort;
            Health = Math.Max(0.0, Math.Min(1.0, Health));
            Morale = Math.Max(0.0, Math.Min(1.0, Morale));
        }

        /// <summary>In port: the casks filled, the men paid what the purse can, and any owed too long go ashore.</summary>
        public static void InPort(Market m)
        {
            if (!Started || m == null)
                return;
            double before = WaterL;
            WaterL = Rules.WaterCapacityL;
            if (WaterL - before > 1.0)
                Notice?.Invoke("The water casks are filled.");
            if (DaysAtSea > 0)
                DaysAtSea = Math.Max(0, DaysAtSea - 3);              // (sea legs fade ashore)
            if (PayOwed <= 0.01 || !Trade.Started)
                return;
            double can = Math.Min(PayOwed, Trade.Purse.Worth(m.zone, TradeRules.Current));
            if (can > 0.01 && Trade.Purse.Pay(Math.Round(can, 2), m.zone, TradeRules.Current, out _))
            {
                PayOwed -= can;
                Notice?.Invoke($"The men are paid {m.zone.Show(can)}." + (PayOwed > 0.01 ? $" Still owed {m.zone.Show(PayOwed)}." : ""));
            }
            if (PayOwed > Men * Rules.pay * Rules.leaveIfOwedDays && Men > 1)
            {
                Men--;
                PayOwed = Math.Max(0.0, PayOwed - Rules.pay * Rules.leaveIfOwedDays);
                Notice?.Invoke($"A man owed {Rules.leaveIfOwedDays:0} days' pay goes ashore for good. {Men} men left.");
            }
        }

        /// <summary>Takes a day's need from the stores (the soonest to spoil first), broaching a new sack or cask as needed.</summary>
        static bool Take(string[] goods, double need, ref double open, double perKg, out double got)
        {
            got = Math.Min(open, need);
            open -= got;
            while (got < need - 1e-6)
            {
                GoodDef pick = null;
                foreach (var id in goods)
                {
                    var g = GameData.Good(id);
                    if (g == null || !Trade.Started || Trade.Hold.Count(id) <= 0)
                        continue;
                    if (pick == null || (g.keeps > 0 && (pick.keeps == 0 || g.keeps < pick.keeps)))
                        pick = g;
                }
                if (pick == null)
                    return false;
                Trade.Hold.Remove(pick, 1);
                open += pick.kg * perKg;
                double take = Math.Min(open, need - got);
                open -= take;
                got += take;
            }
            return true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            Started = false;
            Notice = null;
        }
    }
}
