using System;
using Kapman.Levels.Hdrp;
using Kapman.World;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.Levels
{
    /// <summary>
    /// THE SKY DOME - drives HDRP's Physically Based Sky on the level's sky volume: the first build's J2000 star cube
    /// goes in as the sky's space emission, turned each frame to the moment, the latitude and the precession (the same
    /// sky that places the Sun and the Moon places the stars; HDRP draws them behind the clouds and under the air, so
    /// dusk and cloud hide them by themselves); the sky's horizon and zenith go cool at night; and the exposure's floor
    /// follows the player's eye (<see cref="Eye"/>, the EyeModel: decision 12 of 4 October, night as a dark-adapted eye
    /// sees it), which this dome steps with the clock's game minutes against the light the LightingModel puts on the
    /// ground (Sun, sky, Moon, stars, plus a lamp or a lightning flash in sight): slowly down into the dark, so moonless
    /// nights come up to the stars and a ship's outline over twenty game minutes instead of staying black, and fast back
    /// up when a lamp is lit. HDRP keeps its own reflection of the sky (the water and every lit surface read it), so there is no
    /// reflection probe to refresh any more. In an ocean level the sky's planet is shrunk to the level's bend
    /// (LevelInfo.curveRadius) so the sky's horizon lies on the sea's; on so small a planet the high air stays sunlit
    /// after sunset, so the Sun is dimmed on the sky as the Earth's shadow would (<see cref="SunInSky"/>, read by the
    /// clock).
    /// </summary>
    [ExecuteAlways]
    [DefaultExecutionOrder(-90)]
    public class SkyDome : MonoBehaviour
    {
        [Tooltip("The level's sky volume (global). Empty: the highest-priority global Volume in the scene.")]
        public Volume volume;
        [Tooltip("The J2000 star cube (Assets/Kapman/Sky/Stars_930.exr). Empty: loaded from there in the editor.")]
        public Cubemap stars;
        [Tooltip("The stars' light: 1 is true; a little more shows the sky as the dark-adapted eye sees it against the camera's night.")]
        [Min(0f)] public float starLight = 5f;
        public static float DiagFixedExposure = -100f;    // (diagnostics) > -100: the exposure fixed at this EV instead of automatic
        [Tooltip("The exposure's ceiling (EV).")]
        public float exposureCeiling = 14f;
        [Tooltip("The sky's horizon and zenith tints at full night (cool: the night sky's airglow, not the twilight's red).")]
        public Color nightHorizonTint = new Color(0.62f, 0.78f, 1f), nightZenithTint = new Color(0.85f, 0.9f, 1f);

        [Header("The eye")]
        [Tooltip("The exposure's floor (EV) for the day's eye, and for the fully dark-adapted eye. The floor follows the eye's " +
                 "adaptation, so the camera opens up into the night only as fast as an eye would.")]
        public float dayFloor = 2f, nightFloor = -6f;
        [Tooltip("Dark adaptation's time constant, game minutes (the rods: 20-30 minutes; one game minute is one real second).")]
        [Min(0.1f)] public float darkAdaptMinutes = 20f;
        [Tooltip("Light adaptation's time constant, game seconds (seconds: a lamp lit on deck undoes the dark at once).")]
        [Min(0.1f)] public float lightAdaptSeconds = 8f;
        [Tooltip("The sky's own ground (below its horizon): the colour the haze takes looking down, a light hazy grey.")]
        public Color groundTint = new Color(0.6f, 0.64f, 0.68f);
        [Tooltip("In an ocean level: the sky's planet radius in level curve radii (LevelInfo.curveRadius). Four puts the sky's " +
                 "horizon exactly on the sea's; a little less keeps it just below. A river place (no curve) gets the real Earth.")]
        [Range(3f, 4f)] public float skyRadiusInCurveRadii = 3.6f;
        [Tooltip("On a small sky planet: how much of the Sun's light the sky's air gets as the Sun goes down (log10 of the share, " +
                 "every 2 degrees under the horizon from 0 to -18), measured in the first build so the small sky darkens as the real one.")]
        public float[] twilightLog = { 0f, -0.26f, -0.54f, -0.92f, -1.39f, -2.08f, -2.83f, -3.52f, -3.97f, -4.16f };
        [Tooltip("Degrees under the horizon where the Sun's light on a small sky planet is gone (full night).")]
        public float nightBelow = -24f;

        [Header("Kept from the URP build (unused on HDRP)")]
        [Tooltip("Unused: HDRP's Physically Based Sky draws the sky now. Kept so older builders compile.")]
        public Material skyMaterial;
        [Tooltip("Unused: HDRP keeps its own sky reflection. Kept so older builders compile; don't add a probe.")]
        public ReflectionProbe probe;
        [Tooltip("Unused on HDRP.")]
        public float refreshDegrees = 2f;

        /// <summary>The level's sky dome (the one the chain's current level owns while two levels are loaded).</summary>
        public static SkyDome Current { get; private set; }

        /// <summary>The player's eye: one for the game, carried from level to level (the current level's dome steps it).
        /// The clock's night fill reads it through <see cref="Dusk"/>; the grade and the lookout are to read it too.</summary>
        public static readonly EyeModel Eye = new EyeModel();
        /// <summary>How far the eye has adapted to the dark: 0 the day's eye .. 1 fully dark-adapted (the eye's adaptation;
        /// the clock's night fill comes in with it).</summary>
        public static float Dusk => (float)Eye.Adaptation;
        /// <summary>The light of a lamp, a fire-pot or a hearth the eye is looking at, as lux on the deck: whoever lights one
        /// sets it (0 with none lit), and the eye counts it with the sky's light, so a lit lamp undoes the dark adaptation
        /// within seconds and covering it brings the stars back only over the eye's twenty minutes.</summary>
        public static float LampLux;
        /// <summary>The scene's exposure value the eye was last stepped against (EV100 on an 18% grey ground).</summary>
        public static float SceneEv { get; private set; }
        /// <summary>The share of the Sun's light the sky gets now (1 while it's up, and always on the real Earth; on an ocean
        /// level's small sky planet it falls after sunset as the Earth's shadow would). The clock dims the Sun by it.</summary>
        public static float SunInSky { get; private set; } = 1f;

        const float EarthRadiusKm = 6378.1f;

        static double eyeDay = double.NaN;         // the clock's moment (Julian day) the eye was last stepped at (NaN: settle next)
        bool settleEye;

        Volume bound;
        PhysicallyBasedSky sky;
        Exposure exposure;
        VisualEnvironment environment;
        float appliedKm;

        void OnEnable()
        {
            if (ChainRuntime.Allows(this))       // a level preloaded beside the current one leaves the static alone
                Current = this;
            bound = null;
#if UNITY_EDITOR
            if (stars == null)
                stars = UnityEditor.AssetDatabase.LoadAssetAtPath<Cubemap>("Assets/Kapman/Sky/Stars_930.exr");
#endif
        }

        void OnDisable()
        {
            if (Current == this)
                Current = null;
            SunInSky = 1f;
            // the eye is the player's, not the level's: it is left as it is, for the next level's dome to step on
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearStatics()
        {
            Current = null;
            SunInSky = 1f;
            LampLux = 0f;
            SceneEv = 0f;
            eyeDay = double.NaN;
            Eye.SetAdaptation(0.0);
        }

        /// <summary>Takes hold of the volume's components (once, and again if the volume changes).</summary>
        bool Bind()
        {
            var v = HdrpVolume.Find(volume);
            if (v == null)
                return false;
            if (v == bound && sky != null && exposure != null && environment != null)
                return true;
            bound = v;
            environment = HdrpVolume.Get<VisualEnvironment>(v);
            sky = HdrpVolume.Get<PhysicallyBasedSky>(v);
            exposure = HdrpVolume.Get<Exposure>(v);
            environment.skyType.Override((int)SkyType.PhysicallyBased);
            environment.skyAmbientMode.Override(SkyAmbientMode.Dynamic);       // the ambient light follows the time of day
            environment.renderingSpace.Override(RenderingSpace.World);
            environment.centerMode.Override(VisualEnvironment.PlanetMode.Automatic);   // the planet's top at the Unity origin
            sky.groundTint.Override(groundTint);
            if (stars != null)
                sky.spaceEmissionTexture.Override(stars);
            sky.spaceEmissionMultiplier.Override(starLight);
            exposure.mode.Override(ExposureMode.AutomaticHistogram);
            exposure.limitMax.Override(exposureCeiling);
            appliedKm = 0f;
            return true;
        }

        void LateUpdate()
        {
            if (!Bind())
                return;

            // the sky's planet: the Earth, or the ocean level's bend (set once; changing it makes HDRP work the sky out afresh)
            var info = LevelInfo.Current;
            float km = info != null && info.curveRadius > 1f ? Mathf.Min(skyRadiusInCurveRadii * info.curveRadius / 1000f, EarthRadiusKm) : EarthRadiusKm;
            if (Mathf.Abs(km - appliedKm) > 0.01f)
            {
                environment.planetRadius.Override(km);
                appliedKm = km;
            }

            if (!Sky.Known)
                return;
            float alt = (float)Sky.Sun.altitude;
            float night = LevelClock.Night;
            StepEye(!Application.isPlaying || settleEye);
            settleEye = false;
            SunInSky = appliedKm < EarthRadiusKm - 1f ? Twilight(alt) : 1f;

            // the stars, turned to the moment and the place; the night's cool tints
            sky.spaceRotation.Override(SkyRotation(Sky.Latitude, Sky.Lst, Sky.Precession).eulerAngles);
            sky.spaceEmissionMultiplier.Override(starLight);
            sky.horizonTint.Override(Color.Lerp(Color.white, nightHorizonTint, night));
            sky.zenithTint.Override(Color.Lerp(Color.white, nightZenithTint, night));

            // the eye adapting: the exposure's floor comes down only as fast as the eye adapts to the dark
            exposure.limitMin.Override((float)Eye.LimitMin);
            if (DiagFixedExposure > -100f)
            {
                exposure.mode.Override(ExposureMode.Fixed);
                exposure.fixedExposure.Override(DiagFixedExposure);
            }
            else
                exposure.mode.Override(ExposureMode.AutomaticHistogram);
        }

        /// <summary>Applies the sky for this moment now (after a jump in time), the eye settled on it as if it had been
        /// there a while (the shots, a loaded game). HDRP renders its own reflection.</summary>
        public void Refresh()
        {
            settleEye = true;
            LateUpdate();
        }

        /// <summary>
        /// Steps the player's eye by the game minutes the clock has moved since the last step, against the light on the
        /// ground now: the LightingModel's Sun, sky, Moon and stars under the weather's overcast and storm (an eclipse
        /// taking its share), plus a lamp in sight and a lightning flash. Settles it instead in the editor, after a
        /// Refresh, with no clock, and the first time. Only the current level's dome steps it.
        /// </summary>
        void StepEye(bool settle)
        {
            if (Current != null && Current != this)
                return;
            var weather = LevelWeather.Current;
            double overcast = weather != null ? weather.Overcast : 0.0, storm = weather != null ? weather.Storm : 0.0;
            var clock = LevelClock.Current;
            double lux = LightingModel.TotalLux(Sky.Sun.altitude, overcast, storm, Sky.MoonLux, Sky.Moon.altitude, Sky.SunCovered)
                         + Mathf.Max(0f, LampLux)
                         + (clock != null ? clock.lightningLux * Mathf.Max(0f, LevelClock.Flash) : 0f);
            double ev = LightingModel.EV100FromLux(lux);
            SceneEv = (float)ev;
            Eye.dayFloor = dayFloor;
            Eye.nightFloor = nightFloor;
            Eye.darkMinutes = darkAdaptMinutes;
            Eye.lightMinutes = lightAdaptSeconds / 60.0;
            if (clock == null || settle || double.IsNaN(eyeDay))
            {
                Eye.Settle(ev);
                eyeDay = clock != null ? clock.JulianDay : double.NaN;
                return;
            }
            double day = clock.JulianDay;       // (the moment, not the level's own hours: a level joined has its own start day)
            Eye.Step((day - eyeDay) * 1440.0, ev);      // a step back in time settles it; paused, it holds
            eyeDay = day;
        }

        /// <summary>The share of the Sun's light a small sky planet's air gets at an altitude (degrees): smooth through the table.</summary>
        public float Twilight(float altitude)
        {
            if (altitude >= 0f || twilightLog == null || twilightLog.Length == 0)
                return 1f;
            float last = (twilightLog.Length - 1) * 2f;                       // the table's deepest point, degrees under
            float depth = -altitude;
            float log;
            if (depth <= last)
            {
                float at = depth / 2f;
                int i = Mathf.Min((int)at, twilightLog.Length - 2);
                log = Mathf.Lerp(twilightLog[i], twilightLog[i + 1], at - i);
            }
            else
            {
                float t = Mathf.InverseLerp(last, -nightBelow, depth);
                if (t >= 1f)
                    return 0f;
                log = Mathf.Lerp(twilightLog[twilightLog.Length - 1], -6.5f, Mathf.SmoothStep(0f, 1f, t));
            }
            return Mathf.Pow(10f, log);
        }

        /// <summary>
        /// The turn from the star cube's frame to the world (x east, y up, z north), from the first build (NightSky): the
        /// cube's direction (x, y, z) = (cos d sin a, sin d, -cos d cos a) for J2000 a, d; then precession to the date,
        /// the sidereal time and the latitude. At the pole at sidereal time 0 with no precession it's no turn at all.
        /// </summary>
        public static Quaternion SkyRotation(double latitude, double lst, double[] P)
        {
            double phi = latitude * SkyMath.D2R, th = lst * SkyMath.D2R;
            Vector3 Map(double tx, double ty, double tz)
            {
                double ux = -tz, uy = tx, uz = ty;                                         // cube -> J2000 equatorial
                double vx = P[0] * ux + P[1] * uy + P[2] * uz, vy = P[3] * ux + P[4] * uy + P[5] * uz, vz = P[6] * ux + P[7] * uy + P[8] * uz;
                double hx = Math.Cos(th) * vx + Math.Sin(th) * vy, hy = -Math.Sin(th) * vx + Math.Cos(th) * vy;   // hour angles
                return new Vector3((float)hy, (float)(Math.Sin(phi) * vz + Math.Cos(phi) * hx), (float)(Math.Cos(phi) * vz - Math.Sin(phi) * hx));
            }
            return Quaternion.LookRotation(Map(0, 0, 1), Map(0, 1, 0));
        }
    }
}
