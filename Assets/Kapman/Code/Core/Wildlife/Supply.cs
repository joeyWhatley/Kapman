using System;
using System.Collections.Generic;

namespace Kapman.Nature
{
    /// <summary>
    /// THE SEASONS' SUPPLY (Design/wildlife_and_seasons.md 2.9; GAME_PLAN 2.4.5: the seasons' curves belong to the
    /// animals and the farm) - how much more or less of a good a region has on a day of a year, because of the farm's
    /// tasks and the animals that are in: SeasonalSupply = 1 + the sum, over the tasks and the species that name the
    /// good, of amplitude x the window's strength (or the species' presence x its range there) x the year's stock. A
    /// year whose stock die failed (0.3) turns a task's or a run's supply into its failSupply (more wanted, less made);
    /// a good year above 1 (a mast of 1.5) scales the supply up. The whole is held within 1 +/- maxSwing (decision 16:
    /// strong swings, up to 60%). No prices here: the economy turns supply into price with its own elasticities.
    /// </summary>
    public sealed class Supply
    {
        readonly WildlifeModel model;

        public Supply(WildlifeModel model)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
        }

        /// <summary>The supply multiplier of a good in a region on a Julian day of the year (1 = the year's plain level).</summary>
        public double SeasonalSupply(string goodId, string regionId, int dayOfYear, int year)
        {
            double sum = 0.0;
            foreach (var (task, strength) in model.Farm.Tasks(regionId, dayOfYear))
                sum += strength * Term(task.supply, task.failSupply, goodId, model.Years.Stock(task.stock, year));
            var region = model.Regions.Get(regionId);
            foreach (var s in model.Species.species)
            {
                if (!s.supply.ContainsKey(goodId) && !s.failSupply.ContainsKey(goodId))
                    continue;
                double weight = s.Range(regionId);
                if (weight <= 0.0)
                    continue;
                double present = model.Season(s, dayOfYear, year) * s.Trend(year, regionId) * model.Years.K(s, region, year);
                sum += weight * present * Term(s.supply, s.failSupply, goodId, model.Years.Stock(s.stock, year));
            }
            double swing = model.Farm.maxSwing;
            return Math.Max(1.0 - swing, Math.Min(1.0 + swing, 1.0 + sum));
        }

        /// <summary>
        /// One cause's supply of a good in a year of the given stock: the plain amplitude at a stock of 1, scaled up in a
        /// good year (above 1); below 1 it moves linearly to the failSupply at the failed stock of 0.3 (with no
        /// failSupply written, the amplitude simply shrinks with the stock).
        /// </summary>
        public static double Term(Dictionary<string, double> supply, Dictionary<string, double> failSupply, string goodId, double stock)
        {
            supply.TryGetValue(goodId, out double plain);
            if (stock >= 1.0)
                return plain * stock;
            if (failSupply == null || !failSupply.TryGetValue(goodId, out double failed))
                return plain * stock;
            double t = Presence.Clamp01((stock - 0.3) / 0.7);
            return failed + (plain - failed) * t;
        }
    }
}
