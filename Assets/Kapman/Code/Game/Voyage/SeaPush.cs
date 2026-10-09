using System;
using System.Collections.Generic;
using Kapman.Game.Sim;
using Kapman.Weather;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>The numbers of the sea's push (Game/Data/sea_push.json, by the fields' own names; every one a STARTING
    /// GUESS TO TUNE, the user's).</summary>
    public sealed class SeaPushRules
    {
        /// <summary>Off: the sea pushes nothing and she keeps her line (as before 9 Oct 2026).</summary>
        public bool on = true;
        /// <summary>The tide's stream times this (1: as reckoned from the tide model), and the most it may run, m/s.</summary>
        public double tide = 1.0, tideMost = 3.0;
        /// <summary>Leeway under sail, degrees off her heading: with the wind forward of 60 degrees off the bow, on the
        /// beam (60 to 120), and abaft that; at leewayWind m/s of wind (more in more wind, less in less), and never
        /// more than leewayMost.</summary>
        public double leewayBeating = 10.0, leewayReaching = 6.0, leewayRunning = 2.0, leewayWind = 8.0, leewayMost = 25.0;
        /// <summary>The share of the wind's speed she is blown downwind at with no sail drawing (under oars, the sail
        /// down), and lying to.</summary>
        public double windageBare = 0.02, windageLyingTo = 0.035;
        /// <summary>The share of the wind's speed the top of the sea itself runs at, and how many degrees to the right
        /// of the wind it runs (north of the line).</summary>
        public double seaShare = 0.015, seaTurn = 20.0;
        /// <summary>Real metres off her line at which the log says so and the clock drops back.</summary>
        public double offLineWarnM = 8000.0;
        /// <summary>Real metres from the end of a run of open water at which the steersman sees his landfall and steers
        /// for it by eye: as far as the chart shows her what is round her (the fog's own sight, Exploration.Sight), so
        /// that he sees what the player sees. The share of that by night; and in thick weather never further than
        /// the air lets him see.</summary>
        public double landfallM = 30000.0, landfallNight = 0.4;
        /// <summary>Along a coast he finds out this share of his error in an hour, by day and by night (on the open sea
        /// none of it: he steers by reckoning alone).</summary>
        public double coastEye = 0.5, coastEyeNight = 0.1;
        /// <summary>Lying to in water no deeper than this (real metres), and not in a gale, she lies to her anchor and
        /// is not carried.</summary>
        public double anchorDepthM = 12.0;
        /// <summary>What an hour held off a shore costs the hull (a share of it) when she has not the way on to keep
        /// clear of it; in a gale; and how many times the push onto the shore her own way must be to keep clear.</summary>
        public double shoreStrainHour = 0.01, shoreStrainGale = 0.03, clawOff = 1.2;
        /// <summary>Rough water: the seas an hour over the gunwale (litres) and the wear of the hull (a share an hour)
        /// where the roughness is 1; and the share of her speed she keeps there.</summary>
        public double roughSeas = 400.0, roughWear = 0.003, roughSlow = 0.6;

        /// <summary>The table from its text's root (null: the numbers above).</summary>
        public static SeaPushRules Read(Dictionary<string, object> o)
        {
            var r = new SeaPushRules();
            if (o == null)
                return r;
            foreach (var f in typeof(SeaPushRules).GetFields())
            {
                if (f.IsStatic)
                    continue;
                if (f.FieldType == typeof(double))
                    f.SetValue(r, Json.Num(o, f.Name, (double)f.GetValue(r)));
                else if (f.FieldType == typeof(bool))
                    f.SetValue(r, Json.Bool(o, f.Name, (bool)f.GetValue(r)));
            }
            return r;
        }
    }

    /// <summary>
    /// WHAT PUSHES HER (the user, 9 Oct 2026: "We need an intelligent realistic current and weather system that can
    /// blow and push the ship off course and rough ocean areas"). Three things move her besides her own way:
    ///   - THE TIDE'S STREAM, reckoned from the tide model the game has had since the first build (EOT20's ten
    ///     constituents, good for the tides of 793): it turns with the real tide, some four times a day, and runs
    ///     hardest where the water is shallow and the range large (TideModel.StreamAt);
    ///   - THE WIND'S DRIFT: under sail her leeway, sideways and downwind of her heading, most when she is close to
    ///     the wind; with no sail drawing (under oars, the sail down, lying to) she is blown bodily downwind;
    ///   - THE TOP OF THE SEA ITSELF, which a steady wind sets running a little to the right of where it blows.
    /// This file only reckons them (no state but the tide model, read once). What they do to her is Adrift.
    /// </summary>
    public static class SeaPush
    {
        public const string File = "sea_push.json";
        public const string DefaultTides = "G:/kapman_unity/data/baked/world/tides.bin";

        static readonly ContentCache<SeaPushRules> cache =
            new ContentCache<SeaPushRules>("the sea's push", () => SeaPushRules.Read(ContentFiles.Exists(File) ? ContentFiles.ParseObject(File) : null));
        static SeaPushRules own;

        /// <summary>The numbers as the game has them (or a test's: <see cref="Use"/>).</summary>
        public static SeaPushRules Rules => own ?? cache.Value;

        /// <summary>Puts a table in the game's place (a test); null gives the game's back.</summary>
        public static void Use(SeaPushRules rules) => own = rules;

        /// <summary>A test's sea: the whole push at a place, east and north in m/s, in place of the tide and the wind
        /// (null: the real ones). Never set by the game.</summary>
        public static Func<WorldXZ, (double east, double north)> Fixed;

        static TideModel tides;
        static string tidesFrom;

        /// <summary>The tide model of the whole chart (the file the levels' water reads too), read once; null where it
        /// is not there: then the tide runs no stream.</summary>
        public static TideModel Tides(string path = null)
        {
            path = path ?? DefaultTides;
            if (tidesFrom == path)
                return tides;
            tidesFrom = path;
            tides = null;
            try
            {
                if (System.IO.File.Exists(path))
                    tides = TideModel.Load(path);
                else
                    Debug.LogWarning("Kapman sea: no tides file at " + path + ": the tide runs no stream.");
            }
            catch (Exception e)
            {
                Debug.LogWarning("Kapman sea: the tides file could not be read (" + e.Message + "): the tide runs no stream.");
            }
            return tides;
        }

        /// <summary>The depth of the water at a place, real metres (0 on the land; 30 where the world bake is not there
        /// to ask).</summary>
        public static double Depth(WorldXZ at)
        {
            try
            {
                var land = MapLand.Shared();
                return land == null ? 30.0 : Math.Max(0.0, -land.HeightAt(at.x, at.z));
            }
            catch (Exception)
            {
                return 30.0;
            }
        }

        /// <summary>The tide's stream at a place and a moment (the clock's own Julian day): east and north, m/s.</summary>
        public static (double east, double north) Stream(WorldXZ at, double clockJulianDay) => Stream(at, clockJulianDay, Depth(at));

        /// <summary>The same in water of a given depth (real metres).</summary>
        public static (double east, double north) Stream(WorldXZ at, double clockJulianDay, double depth)
        {
            var r = Rules;
            var model = Tides();
            if (model == null || r.tide <= 0.0)
                return (0.0, 0.0);
            var (lat, lon) = WorldFrame.ToLatLon(at.x, at.z);
            // (the clock keeps the time of the chart's middle; the tide is reckoned in the time of Greenwich)
            var moment = MomentAt(model, Sky.ToUt(clockJulianDay, WorldFrame.CentreLon));
            model.StreamAt(lat, lon, moment, Math.Max(4.0, depth), out double east, out double north);
            east *= r.tide;
            north *= r.tide;
            double speed = Math.Sqrt(east * east + north * north);
            if (speed > r.tideMost && speed > 0.0)
            {
                east *= r.tideMost / speed;
                north *= r.tideMost / speed;
            }
            return (east, north);
        }

        // (the Moon and the Sun are the same for every place at one moment: kept for the minute they are asked in)
        static double momentJd = double.NaN;
        static TideModel.Moment moment;
        static TideModel momentOf;

        static TideModel.Moment MomentAt(TideModel model, double jdUt)
        {
            if (jdUt != momentJd || !ReferenceEquals(momentOf, model))
            {
                momentJd = jdUt;
                momentOf = model;
                moment = model.At(jdUt);
            }
            return moment;
        }

        /// <summary>
        /// The wind's part: east and north, m/s. `heading`: where her bow points, degrees. `speed`: her way through the
        /// water, m/s. `windFrom`: where the wind blows from, degrees. `sail`: her sail is drawing (else she is under
        /// oars, or has no way on). `lyingTo`: hove to.
        /// </summary>
        public static (double east, double north) Wind(double heading, double speed, double windFrom, double windSpeed, bool sail, bool lyingTo)
        {
            var r = Rules;
            if (windSpeed <= 0.0)
                return (0.0, 0.0);
            double east = 0.0, north = 0.0;
            double down = (windFrom + 180.0) * Math.PI / 180.0;              // (where the wind blows to)
            double downE = Math.Sin(down), downN = Math.Cos(down);
            if (sail && !lyingTo && speed > 0.05)
            {
                // leeway: sideways off her heading, to the side the wind blows to
                double off = Math.Abs(((heading - windFrom) % 360.0 + 540.0) % 360.0 - 180.0);   // 0: the wind dead ahead; 180: dead astern
                // (how many degrees: the graph "Rule - her leeway under sail")
                double angle = GameRules.Leeway(off, windSpeed, r.leewayBeating, r.leewayReaching, r.leewayRunning, r.leewayWind, r.leewayMost);
                double h = heading * Math.PI / 180.0;
                double rightE = Math.Cos(h), rightN = -Math.Sin(h);          // (her starboard beam)
                double across = rightE * downE + rightN * downN;
                // (dead before the wind or dead into it there is no side for her to be set to)
                double side = Math.Abs(across) < 0.05 ? 0.0 : across > 0.0 ? 1.0 : -1.0;
                double lee = speed * Math.Tan(angle * Math.PI / 180.0);
                east += rightE * side * lee;
                north += rightN * side * lee;
            }
            else
            {
                // no sail drawing: she is blown bodily downwind (how fast: the graph "Rule - blown downwind with no sail")
                double blown = GameRules.Blown(windSpeed, lyingTo, r.windageBare, r.windageLyingTo);
                east += downE * blown;
                north += downN * blown;
            }
            // the top of the sea itself, a little to the right of the wind (how fast: the graph "Rule - the top of the sea runs")
            double sea = (windFrom + 180.0 + r.seaTurn) * Math.PI / 180.0;
            double runs = GameRules.SeaRuns(windSpeed, r.seaShare);
            east += Math.Sin(sea) * runs;
            north += Math.Cos(sea) * runs;
            return (east, north);
        }

        /// <summary>Where a push sets, one of eight ("north-east"); "" when it is nothing.</summary>
        public static string Sets(double east, double north) =>
            east * east + north * north < 1e-8 ? "" : SeaWatch.Airt(0.0, 0.0, east, north);

        /// <summary>How hard a push runs, as a sailor says it ("" under a fifth of a knot). PLACEHOLDER WORDS, the
        /// user's to write.</summary>
        public static string Runs(double east, double north)
        {
            double knots = Math.Sqrt(east * east + north * north) / 0.5144;
            return knots < 0.2 ? "" : knots < 0.4 ? "a quarter of a knot" : knots < 0.75 ? "half a knot" : knots < 1.25 ? "a knot"
                 : knots < 1.75 ? "a knot and a half" : knots < 2.5 ? "two knots" : knots < 3.5 ? "three knots" : $"{knots:0} knots";
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            own = null;
            Fixed = null;
            momentJd = double.NaN;
        }
    }
}
