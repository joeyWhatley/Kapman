using System;
using System.Collections.Generic;
using System.IO;

namespace Kapman.Nature
{
    /// <summary>
    /// THE WILDLIFE MODEL - the four tables (regions, species, the farm year, the events), the chronicle's hard winters and
    /// the world's seed, with the sums of Design/wildlife_and_seasons.md 2.2: a species' season on a day (its curve slid
    /// by the century, the year's die and the warm index), times its trend and its crash state after hard winters, by
    /// region. The level's Wildlife component, the economy (through Supply) and the tests all ask this one object.
    /// </summary>
    public sealed class WildlifeModel
    {
        /// <summary>The project folder of the wildlife tables, and the chronicle's file.</summary>
        public const string ProjectFolder = "Assets/Kapman/Data/Wildlife";
        public const string ChroniclePath = "Assets/Kapman/Game/Data/chronicle_793.json";

        public readonly RegionTable Regions;
        public readonly SpeciesTable Species;
        public readonly FarmYear Farm;
        public readonly WildlifeEvents Events;
        public readonly Supply Supply;
        public int Seed { get; private set; }
        public YearState Years { get; private set; }

        readonly List<ChronicleWinter> winters;

        public WildlifeModel(RegionTable regions, SpeciesTable species, FarmYear farm, WildlifeEvents events,
                             List<ChronicleWinter> winters, int seed)
        {
            Regions = regions ?? throw new ArgumentNullException(nameof(regions));
            Species = species ?? throw new ArgumentNullException(nameof(species));
            Farm = farm ?? throw new ArgumentNullException(nameof(farm));
            Events = events ?? throw new ArgumentNullException(nameof(events));
            this.winters = winters ?? new List<ChronicleWinter>();
            Seed = seed;
            Years = new YearState(seed, regions.climate, this.winters);
            Supply = new Supply(this);
        }

        /// <summary>The model from the JSON texts of the four tables and the chronicle.</summary>
        public static WildlifeModel FromJson(string regions, string species, string farmYear, string events, string chronicle, int seed) =>
            new WildlifeModel(RegionTable.Parse(regions), SpeciesTable.Parse(species), FarmYear.Parse(farmYear),
                              WildlifeEvents.Parse(events), YearState.WintersFrom(chronicle), seed);

        /// <summary>The model from a folder holding the four tables and the chronicle's path (the tests and the editor:
        /// paths relative to the project).</summary>
        public static WildlifeModel FromFolder(string folder, string chroniclePath, int seed) =>
            FromJson(File.ReadAllText(Path.Combine(folder, RegionTable.FileName)),
                     File.ReadAllText(Path.Combine(folder, SpeciesTable.FileName)),
                     File.ReadAllText(Path.Combine(folder, FarmYear.FileName)),
                     File.ReadAllText(Path.Combine(folder, WildlifeEvents.FileName)),
                     File.ReadAllText(chroniclePath), seed);

        /// <summary>The model of the project's own tables.</summary>
        public static WildlifeModel FromProject(int seed) => FromFolder(ProjectFolder, ChroniclePath, seed);

        /// <summary>A new world seed (a new or loaded game): the year state is thrown again.</summary>
        public void Reseed(int seed)
        {
            if (seed == Seed)
                return;
            Seed = seed;
            Years = new YearState(seed, Regions.climate, winters);
        }

        /// <summary>The warm index of a year (decision 17).</summary>
        public double WarmIndex(int year) => Regions.climate.WarmIndex(year);

        /// <summary>A species' season on a Julian day of a year (0..1): its curve, the century's lag, the year's slide.</summary>
        public double Season(Species s, int dayOfYear, int year) => Nature.Presence.At(s, Seed, dayOfYear, year, WarmIndex(year));

        /// <summary>
        /// How much of a species is in a region on a day (0..1 of its full numbers there): its season, times its trend and
        /// its crash state after hard winters; 0 where its range does not reach. The range's weight is not in it (that is
        /// how many, not how much of them is here now).
        /// </summary>
        public double Presence(Species s, Region region, int dayOfYear, int year)
        {
            if (s == null || region == null || s.Range(region.id) <= 0.0)
                return 0.0;
            return Season(s, dayOfYear, year) * s.Trend(year, region.id) * Years.K(s, region, year);
        }

        /// <summary>
        /// A flock's presence (0..1) for a species id or a group: for a group, the members' presences weighted by their
        /// numbers in the region (range x perKm2), so a summer flock of the wild geese in the delta is the resident
        /// greylags alone, a fraction of the winter's. Species with no numbers in the region are left out; a group with
        /// none there is 0. Throws for an id that is neither a species nor a group (a builder's typo is an error).
        /// </summary>
        public double FlockPresence(string speciesOrGroup, Region region, int dayOfYear, int year)
        {
            var members = Species.Resolve(speciesOrGroup);
            if (members.Count == 0)
                throw new ArgumentException("No wildlife species or group '" + speciesOrGroup + "' in " + SpeciesTable.FileName);
            if (region == null)
                return 0.0;
            if (members.Count == 1)
                return Presence(members[0], region, dayOfYear, year);
            double sum = 0.0, weights = 0.0;
            foreach (var s in members)
            {
                double n = s.Range(region.id) * Math.Max(s.perKm2, 0.0);
                if (n <= 0.0)
                    continue;
                sum += n * Presence(s, region, dayOfYear, year);
                weights += n;
            }
            return weights > 0.0 ? sum / weights : 0.0;
        }

        /// <summary>What of a species is seen at this hour (2.2): its presence, times the hour's band and the weather.</summary>
        public double Active(Species s, Region region, WildlifeContext x) =>
            Presence(s, region, x.day, x.year) * Nature.Presence.Hours(s, x.band)
            * Nature.Presence.Weather(s, x.wind, x.rain, x.fogDensity, x.frost, x.storm);

        /// <summary>The supply multiplier of a good (Supply.SeasonalSupply).</summary>
        public double SeasonalSupply(string goodId, string regionId, int dayOfYear, int year) =>
            Supply.SeasonalSupply(goodId, regionId, dayOfYear, year);
    }
}
