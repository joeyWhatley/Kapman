using System;
using Kapman.Levels;
using Kapman.Levels.Water;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game.Sim
{
    /// <summary>
    /// THE STAGE - where the world simulation meets the 3D scene (ported 4 Oct). The simulation itself is abstract: ports,
    /// legs and days, in real metres in the world frame (WorldFrame: x east, z north). The first build showed it on one
    /// 1:32 map through FloatingOrigin, LandStreamer, WaterSource and LocalWeather; the rebuild is a chain of levels (river
    /// places at true size, seas shortened hard), each its own scene with its own terrains, water, weather and clock.
    /// Everything in Sim/ that puts a ship of the world on the water, asks the land or the water under a point, reads the
    /// wind or the clock, or finds the merchant's ship, goes through here. So the level chain plugs in at ONE place: give
    /// <see cref="SimStage.Current"/> another stage (an ocean level's own mapping, say: a shortened sea isn't a straight
    /// squeeze of the real one) and nothing else in Sim/ changes.
    /// </summary>
    public interface ISimStage
    {
        /// <summary>A point of the scene (Unity metres) as real metres in the world frame.</summary>
        WorldXZ ToWorld(Vector3 scene);
        /// <summary>A point of the world (real metres) in the scene, on the water (y = the water's height there).</summary>
        Vector3 ToScene(WorldXZ world);
        /// <summary>Whether a point of the world lies within this level's scene at all (a ship of the world a sea away is
        /// not on this stage, however near in shortened scene metres she'd come out).</summary>
        bool OnStage(WorldXZ world);
        /// <summary>The water's height at a point of the scene (Unity metres), tide and waves included.</summary>
        float WaterHeight(Vector3 scene);
        /// <summary>The land's height above the water's resting level at a point of the world (metres; below 0 is water,
        /// -100 off every terrain, NaN when the scene has no terrain at all).</summary>
        float LandAboveWater(WorldXZ world);
        /// <summary>The wind at the merchant's ship now: where it blows from (degrees, 0 north) and how hard (m/s).</summary>
        void Wind(out float windFrom, out float windSpeed);
        /// <summary>How far the air lets a lookout see here (real km): rain and fog.</summary>
        double HazeKm { get; }
        /// <summary>How far the air lets a lookout see from the merchant's ship to a point of the world (real km).</summary>
        double VisibilityKmTo(WorldXZ world);
        int DayOfYear { get; }
        double HourOfDay { get; }
        double JulianDay { get; }
        /// <summary>Holds the game's clock (two ships alongside, talking) or lets it run.</summary>
        void HoldClock(bool hold);
    }

    /// <summary>
    /// THE LEVEL AS THE STAGE - the default: the scene the level chain has loaded now. Scene and world are related
    /// through FloatingOrigin (as Ports, Voyage and Chest relate them): its Origin is the world position of Unity's
    /// (0,0,0) and its HorizontalScale the level's real metres per Unity metre. A level scene without a FloatingOrigin
    /// falls back to its LevelInfo (its centre's latitude/longitude at the scene's origin, realMetresPerUnit across),
    /// so the sim's ships still come out somewhere sensible - put a FloatingOrigin in each level so everyone agrees.
    /// </summary>
    public sealed class LevelStage : ISimStage
    {
        /// <summary>Scene metres from the origin within which a point of the world counts as on this stage.</summary>
        public float stageRadius = 12000f;

        TerrainHeights land;
        UnityEngine.SceneManagement.Scene landScene;
        static bool warnedNoOrigin;

        // the scene's origin in the world, and real metres per scene metre: FloatingOrigin when the scene has one,
        // else the level's own centre and scale
        static void Frame(out double ox, out double oz, out double scale)
        {
            var origin = FloatingOrigin.Origin;
            if (origin.x != 0.0 || origin.z != 0.0)
            {
                ox = origin.x;
                oz = origin.z;
                scale = FloatingOrigin.HorizontalScale;
                return;
            }
            var info = LevelInfo.Current;
            if (info != null)
            {
                (ox, oz) = WorldFrame.ToWorld(info.latitude, info.longitude);
                scale = Math.Max(0.001, info.realMetresPerUnit);
                if (!warnedNoOrigin && Application.isPlaying)
                {
                    warnedNoOrigin = true;
                    Debug.LogWarning($"Kapman: level '{info.levelId}' has no FloatingOrigin; the world simulation takes the level's " +
                                     "centre as the scene's origin and realMetresPerUnit as its scale. Put a FloatingOrigin in the " +
                                     "level (Origin at its centre, horizontalScale = realMetresPerUnit) so ports, courses and the " +
                                     "world's ships all agree on where the scene is.");
                }
                return;
            }
            ox = 0.0;
            oz = 0.0;
            scale = FloatingOrigin.HorizontalScale;
        }

        public WorldXZ ToWorld(Vector3 scene)
        {
            Frame(out double ox, out double oz, out double scale);
            return new WorldXZ(ox + scene.x * scale, oz + scene.z * scale);
        }

        public Vector3 ToScene(WorldXZ world)
        {
            Frame(out double ox, out double oz, out double scale);
            var at = new Vector3((float)((world.x - ox) / scale), 0f, (float)((world.z - oz) / scale));
            at.y = WaterHeight(at);
            return at;
        }

        public bool OnStage(WorldXZ world)
        {
            Frame(out double ox, out double oz, out double scale);
            double x = (world.x - ox) / scale, z = (world.z - oz) / scale;
            return x * x + z * z <= (double)stageRadius * stageRadius;
        }

        public float WaterHeight(Vector3 scene) => WaterSampler.Height(scene);

        public float LandAboveWater(WorldXZ world)
        {
            // the scene's terrains, read once per scene (TerrainHeights copies every heightmap)
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (land == null || scene != landScene)
            {
                land = TerrainHeights.FromScene();
                landScene = scene;
            }
            if (land.Terrains == 0)
                return float.NaN;
            Frame(out double ox, out double oz, out double scale);
            float water = LevelInfo.Current != null ? LevelInfo.Current.WaterLevel : 0f;
            return land.HeightAt((world.x - ox) / scale, (world.z - oz) / scale) - water;
        }

        public void Wind(out float windFrom, out float windSpeed)
        {
            var w = LevelWeather.Current;
            if (w == null)
            {
                // (the chart has no level, so no level weather: the air where she is, from the model the world's ships
                // sail by. Before, a band of raiders chased her on the chart by a south-westerly breeze that never changed)
                if (ChartWorld.Current != null && ChartWorld.TryWeather(out var air))
                {
                    windFrom = air.windFrom;
                    windSpeed = air.windSpeed;
                    return;
                }
                windFrom = 225f;                                              // (a south-westerly breeze, failing a weather)
                windSpeed = 5f;
                return;
            }
            // WindVector points where the wind blows to (x east, y north); it comes from the other way
            Vector2 v = w.WindVector;
            windFrom = v.sqrMagnitude > 1e-6f ? (Mathf.Atan2(-v.x, -v.y) * Mathf.Rad2Deg + 360f) % 360f : 225f;
            windSpeed = w.Wind;
        }

        public double HazeKm
        {
            get
            {
                var w = LevelWeather.Current;
                if (w == null)
                {
                    // (the chart: the fog and rain where she is, so the lookout sees no farther than the Look out window says)
                    if (ChartWorld.Current != null && ChartWorld.TryWeather(out var air))
                        return Math.Max(0.05, Math.Min(40.0, air.VisibilityKm));
                    return 40.0;
                }
                double haze = 40.0 / (1.0 + 0.35 * w.Rain);                   // (as the first build's FogBankField.HazeKm)
                if (w.FogDensity > 1e-7f)
                    haze = Math.Min(haze, 3.912 / w.FogDensity / 1000.0);     // (Koschmieder: visibility 3.9 / extinction)
                return Math.Max(0.05, haze);
            }
        }

        // one weather for the whole level: no fog banks between here and there
        public double VisibilityKmTo(WorldXZ world) => HazeKm;

        public int DayOfYear => LevelClock.Current != null ? LevelClock.Current.DayOfYear : 172;
        public double HourOfDay => LevelClock.Current != null ? LevelClock.Current.HourOfDay : 12.0;
        public double JulianDay => LevelClock.Current != null ? LevelClock.Current.JulianDay : SkyMath.JulianDay(793, 6, 21, 12.0);

        public void HoldClock(bool hold) { if (hold) GameSpeed.Hold("alongside"); else GameSpeed.Release("alongside"); }   // 4 Oct: through GameSpeed's holds (it owns the time scale and would unpause the clock each frame)
    }

    /// <summary>The stage in use: the level loaded now, unless the level chain gives another.</summary>
    public static class SimStage
    {
        static ISimStage current;

        public static ISimStage Current
        {
            get => current ?? (current = new LevelStage());
            set => current = value;
        }

        public static WorldXZ ToWorld(Vector3 scene) => Current.ToWorld(scene);
        public static Vector3 ToScene(WorldXZ world) => Current.ToScene(world);
        public static bool OnStage(WorldXZ world) => Current.OnStage(world);
        public static float WaterHeight(Vector3 scene) => Current.WaterHeight(scene);
        public static float LandAboveWater(WorldXZ world) => Current.LandAboveWater(world);
        public static void Wind(out float windFrom, out float windSpeed) => Current.Wind(out windFrom, out windSpeed);
        public static double HazeKm => Current.HazeKm;
        public static double VisibilityKmTo(WorldXZ world) => Current.VisibilityKmTo(world);
        public static int DayOfYear => Current.DayOfYear;
        public static double HourOfDay => Current.HourOfDay;
        public static double JulianDay => Current.JulianDay;
        public static void HoldClock(bool hold) => Current.HoldClock(hold);

        /// <summary>Real km between two points of the world.</summary>
        public static double Km(WorldXZ a, WorldXZ b) => WorldXZ.Distance(a, b) / 1000.0;

        /// <summary>The sun's height above the horizon (degrees) at a point of the world, by the stage's clock.</summary>
        public static double SunElevationAt(WorldXZ world)
        {
            var (lat, _) = WorldFrame.ToLatLon(world.x, world.z);
            return Sight.SunElevation(lat, DayOfYear, HourOfDay);
        }

        // play mode starts without a domain reload: a stage from the last run must not linger
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearStatics() => current = null;
    }
}
