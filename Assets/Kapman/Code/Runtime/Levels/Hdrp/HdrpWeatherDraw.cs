using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.Levels.Hdrp
{
    /// <summary>
    /// HDRP WEATHER DRAW - draws the level's weather (<see cref="LevelWeather"/>) with HDRP, as real as it can:
    ///   - clouds: HDRP's volumetric clouds in their "manual" mode. A small cloud map round the level (how much cloud,
    ///     rain, which kind, how tall) is made from the weather's cover, darkness and storm, patchy when the cover is
    ///     thin and an unbroken deck when it's full; the kinds come from our own profile texture (a thick low deck of
    ///     stratus, thin altostratus, puffy fair-weather cumulus, towering cumulonimbus). Under the weather engine the kind,
    ///     the base and the depth are the model's (weather document 2.5: stratus at a few hundred metres, nimbostratus
    ///     thousands of metres deep, the cumulonimbus tower). The shapes drift with the wind (the Visual Environment's wind,
    ///     which HDRP's clouds read), and cloud shadows sweep the land; their mean share of the Sun goes to the clock so it
    ///     does not dim the Sun twice;
    ///   - the haze and fog: HDRP's volumetric fog, its mean free path from the weather's visibility (MOR / 3.912, through
    ///     a 40 m clamp), its top the fog's own (radiation fog low, sea fog higher, the haze's mixed layer), grey and dark
    ///     in a storm;
    ///   - the ground mist at dawn and dusk: a Local Volumetric Fog volume lying on the water up to the mist's height,
    ///     patchy from a 3D noise mask, drifting with the wind, lit and shadowed by HDRP as any fog is;
    ///   - snow, frost, wet ground and the aurora: two full-screen passes of our own (<see cref="SnowFrostPass"/>).
    /// It changes the sky volume's runtime profile, never the asset. Made and thrown away by LevelWeather.
    /// </summary>
    public class HdrpWeatherDraw : System.IDisposable
    {
        const int MapSize = 64;
        const float EarthRadius = 6378100f;
        // the kinds of cloud: where each sits in the cloud map's "type" channel (the profile texture's columns)
        const float Stratus = 0.12f, Altostratus = 0.35f, Cumulus = 0.55f, Cumulonimbus = 0.82f;

        readonly LevelWeather weather;
        Volume bound;
        VolumetricClouds clouds;
        Fog fog;
        VisualEnvironment environment;
        Texture2D cloudMap, cloudKinds;
        Color32[] texels;
        float[] patches;
        float mapCover = -1f, mapDark = -1f, mapRain = -1f, mapStorm = -1f, mapKind = -2f;
        /// <summary>The lowest cloud base the volumetric clouds are drawn at, m: HDRP's clouds smear the screen from inside, and
        /// a 150 m stratus would fade from a camera on a mast or the isometric view (guess).</summary>
        const float LowestDrawnBase = 300f;
        LocalVolumetricFog mist;
        GameObject mistGo;
        Texture3D mistMask;
        Vector3 mistCentre;
        bool mistPlaced;
        SnowFrostPass passes;

        public HdrpWeatherDraw(LevelWeather weather)
        {
            this.weather = weather;
            cloudKinds = MakeCloudKinds();
            cloudMap = new Texture2D(MapSize, MapSize, TextureFormat.RGBA32, false, true)
            {
                name = "Weather: cloud map", wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.DontSave,
            };
            texels = new Color32[MapSize * MapSize];
            patches = MakePatches();
            mistMask = MakeMistMask();
            MakeMist();
            passes = new SnowFrostPass(weather.transform, weather.weatherShader);
        }

        public void Dispose()
        {
            // the scene objects are switched off, not destroyed (they go with the weather's object; destroying a child
            // while the hierarchy is coming down logs an error); the next draw replaces them. The textures go now, so
            // nothing is left pointing at them
            if (clouds != null)
            {
                clouds.enable.Override(false);
                clouds.cloudMap.Override(null);
                clouds.cloudLut.Override(null);
            }
            if (mist != null)
            {
                var p = mist.parameters;
                p.volumeMask = null;
                mist.parameters = p;
            }
            if (mistGo != null)
                mistGo.SetActive(false);
            passes?.Dispose();
            passes = null;
            CoreUtils.Destroy(cloudMap);
            CoreUtils.Destroy(cloudKinds);
            CoreUtils.Destroy(mistMask);
            mistGo = null;
            mist = null;
            bound = null;
        }

        // ------------------------------------------------------------------ the volume

        /// <summary>Takes hold of the volume's components (once, and again if the volume changes), and sets what never changes.</summary>
        bool Bind()
        {
            var v = HdrpVolume.Find(weather.volume);
            if (v == null)
                return false;
            if (v == bound && clouds != null && fog != null && environment != null)
                return true;
            bound = v;
            environment = HdrpVolume.Get<VisualEnvironment>(v);
            clouds = HdrpVolume.Get<VolumetricClouds>(v);
            fog = HdrpVolume.Get<Fog>(v);

            clouds.enable.Override(!DiagNoClouds);
            clouds.cloudControl.Override(VolumetricClouds.CloudControl.Manual);
            clouds.cloudMap.Override(cloudMap);
            clouds.cloudLut.Override(cloudKinds);
            clouds.bottomAltitude.Override(DrawnBase(weather));
            clouds.altitudeRange.Override(DrawnDepth(weather));
            clouds.shadows.Override(true);                                   // cloud shadows sweep the land and the water
            clouds.shadowDistance.Override(8000f);
            clouds.shadowResolution.Override(VolumetricClouds.CloudShadowResolution.Medium256);
            clouds.sunLightDimmer.Override(1f);                              // HDRP thins the Sun's light at the cloud itself
            clouds.ambientLightProbeDimmer.Override(1f);

            fog.enabled.Override(true);
            fog.colorMode.Override(FogColorMode.SkyColor);                   // the fog takes the sky's light in each direction
            fog.maxFogDistance.Override(20000f);
            fog.mipFogMaxMip.Override(0.6f);
            fog.mipFogNear.Override(0f);
            fog.mipFogFar.Override(3000f);
            fog.enableVolumetricFog.Override(true);                          // lit and shadowed: light shafts, dark under cloud
            fog.depthExtent.Override(1500f);
            fog.anisotropy.Override(0.4f);                                   // brighter towards the Sun (0.6 haloed it)
            fog.albedo.Override(new Color(0.95f, 0.95f, 0.97f));
            mapCover = -1f;
            return true;
        }

        // ------------------------------------------------------------------ each frame

        /// <summary>Pushes this frame's weather to HDRP (after the clock has placed the Sun and the Moon).</summary>
        public void Push()
        {
            var w = weather;
            var info = LevelInfo.Current;
            float wl = info != null ? info.WaterLevel : 0f;
            var eye = Camera.main;
            Vector3 eyePos = eye != null ? eye.transform.position : Vector3.zero;

            if (Bind())
            {
                PushClouds(w, eyePos.y);
                PushWind(w);
                PushFog(w, wl);
            }
            PushMist(w, wl, eyePos);
            if (passes != null)
            {
                float mfp = FogPath(w, 1f);
                float top = FogTop(w, wl);
                passes.Push(w.SnowCover, w.Frost, w.Wetness, wl, 1f / mfp, wl - 2f, 3f / Mathf.Max(top - (wl - 2f), 10f),
                            LevelClock.SunLightLux, LevelClock.MoonLightLux, w.Aurora * LevelClock.Night, w.auroraLight);
            }
        }

        void PushClouds(LevelWeather w, float eyeHeight)
        {
            float cover = w.CloudCover, dark = w.CloudDark, storm = w.Storm, rain = Mathf.Max(w.Rain, w.Snowfall * 0.6f);
            // under the weather engine the kind of cloud is the model's (weather 2.5); set by hand it follows the cover
            float kind = w.SkyFromEngine ? KindColumn(w.CloudType) : -1f;
            if (Mathf.Abs(cover - mapCover) > 0.01f || Mathf.Abs(dark - mapDark) > 0.03f || Mathf.Abs(rain - mapRain) > 0.03f
                || Mathf.Abs(storm - mapStorm) > 0.03f || kind != mapKind)
            {
                BuildMap(cover, dark, rain, storm, kind);
                mapCover = cover;
                mapDark = dark;
                mapRain = rain;
                mapStorm = storm;
                mapKind = kind;
            }
            // put the map over the level: HDRP spreads it over its "normalisation" distance round the planet's top (the
            // Unity origin) times the tiling, so one map width is MapSize patches of cloud
            float bottom = DrawnBase(w), depth = DrawnDepth(w);
            float radius = environment.planetRadius.value * 1000f;
            float mid = bottom + depth * 0.5f;
            float normalisation = Mathf.Sqrt(Mathf.Max(1f, (EarthRadius + mid) * (EarthRadius + mid) - EarthRadius * radius));
            float span = Mathf.Max(1f, MapSize * w.cloudPatch);
            clouds.cloudTiling.Override(new Vector2(normalisation / span, normalisation / span));
            clouds.cloudOffset.Override(Vector2.zero);
            clouds.bottomAltitude.Override(bottom);
            clouds.altitudeRange.Override(depth);

            // a full deck is smoother and fuller; a storm's is denser and darker; and the clouds fade away as the camera
            // climbs towards them, so it never flies into them (HDRP's clouds smear the screen from inside)
            float deck = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.62f, 0.9f, cover));
            float below = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(bottom * 0.45f, bottom * 0.9f, eyeHeight));
            clouds.densityMultiplier.Override(Mathf.Lerp(0.32f, 0.7f, Mathf.Max(storm, dark)) * below);
            clouds.shapeFactor.Override(Mathf.Lerp(0.9f, 0.55f, deck));
            clouds.erosionFactor.Override(Mathf.Lerp(0.85f, 0.55f, deck));
            float shadow = Mathf.Lerp(0.5f, 0.9f, Mathf.Max(dark, storm));
            clouds.shadowOpacity.Override(shadow);
            bool on = !DiagNoClouds && below > 0.001f && cover > 0.02f;
            clouds.enable.Override(on);
            // the share of the Sun the drawn clouds' shadows take on average, so the clock does not dim it twice
            CloudShadowShare = on ? shadow * Mathf.Clamp01(cover) : 0f;
        }

        /// <summary>The share of the Sun HDRP's volumetric cloud shadows take on average (their opacity times the cover;
        /// 0 while the clouds are off). LevelClock divides it out of the weather's own dimming (LightingModel.CloudDim).</summary>
        public float CloudShadowShare { get; private set; }

        /// <summary>The cloud layer's base as drawn: the weather's, never under <see cref="LowestDrawnBase"/>.</summary>
        static float DrawnBase(LevelWeather w) => Mathf.Max(w.CloudBase, LowestDrawnBase);

        /// <summary>The cloud layer's depth as drawn: the weather's, at least 200 m.</summary>
        static float DrawnDepth(LevelWeather w) => Mathf.Max(w.CloudDepth, 200f);

        /// <summary>Where a kind of cloud sits in the cloud map's "type" channel (the profile texture's columns), or -1 to let
        /// the cover choose (stratocumulus: puffs closing to a deck; the high veils, which the volumetric layer does not draw).</summary>
        static float KindColumn(Kapman.Weather.CloudType type)
        {
            switch (type)
            {
                case Kapman.Weather.CloudType.Stratus:
                case Kapman.Weather.CloudType.Nimbostratus:
                    return Stratus;
                case Kapman.Weather.CloudType.Altostratus:
                case Kapman.Weather.CloudType.Altocumulus:
                    return Altostratus;
                case Kapman.Weather.CloudType.Cumulus:
                    return Cumulus;
                case Kapman.Weather.CloudType.Cumulonimbus:
                    return Cumulonimbus;
                default:
                    return -1f;
            }
        }

        /// <summary>The wind HDRP's clouds (and anything else reading the Visual Environment) drift with.</summary>
        void PushWind(LevelWeather w)
        {
            // a little livelier than the surface wind (the wind aloft is stronger), but not the clock's 60x: a time-lapse
            environment.windSpeed.Override(w.Wind * 3.6f * 1.5f);           // km/h
            float heading = Mathf.Atan2(w.WindVector.x, w.WindVector.y) * Mathf.Rad2Deg;   // where it blows towards, from north
            environment.windOrientation.Override(Mathf.Repeat(90f - heading, 360f));
        }

        /// <summary>
        /// HDRP's fog mean free path (m) from the weather's visibility (weather 1.1, 2.5: the meteorological optical range
        /// is 3.912 mean free paths), through the 40 m clamp: dense fog rendered black in batch shots (3 Oct) and the clamp
        /// stays until that is found; it lowers to 10 m only then. <paramref name="densityScale"/> is the fog diagnostics'.
        /// </summary>
        static float FogPath(LevelWeather w, float densityScale)
        {
            float path = w.SkyFromEngine ? (float)Kapman.Weather.Visibility.MeanFreePath(w.Visibility) : 1f / Mathf.Max(w.FogDensity, 1e-6f);
            return Mathf.Clamp(path / Mathf.Max(densityScale, 1e-6f), 40f, 50000f);
        }

        static float FogTop(LevelWeather w, float wl)
        {
            // under the weather engine the fog's own top (radiation fog 30-120 m, sea fog 200-500, the haze's mixed layer
            // 1,000-2,500: weather 2.5)
            if (w.SkyFromEngine)
                return wl + Mathf.Clamp(w.FogTop, 30f, 2500f);
            float fogness = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.0005f, 0.004f, w.FogDensity));
            return Mathf.Lerp(wl + 400f, wl + 60f, fogness);                 // high haze in clear air; a low bank in fog
        }

        // diagnostics (Shots.DiagFog): knobs to find why dense fog renders black in batch shots; all off by default
        public static bool DiagNoVolumetric;
        public static float DiagDepthExtent = -1f, DiagAnisotropy = -1f, DiagDensityScale = 1f, DiagTop = -1f;
        public static bool DiagConstantColour;
        public static bool DiagNoClouds;

        void PushFog(LevelWeather w, float wl)
        {
            fog.meanFreePath.Override(FogPath(w, DiagDensityScale));
            fog.baseHeight.Override(wl - 2f);
            fog.maximumHeight.Override(DiagTop > 0f ? wl + DiagTop : FogTop(w, wl));
            fog.tint.Override(Color.Lerp(Color.white, new Color(0.5f, 0.52f, 0.56f), w.Storm * 0.8f));
            fog.enableVolumetricFog.Override(!DiagNoVolumetric);
            if (DiagDepthExtent > 0f) fog.depthExtent.Override(DiagDepthExtent);
            if (DiagAnisotropy >= 0f) fog.anisotropy.Override(DiagAnisotropy);
            fog.colorMode.Override(DiagConstantColour ? FogColorMode.ConstantColor : FogColorMode.SkyColor);
            if (DiagConstantColour) fog.color.Override(new Color(0.7f, 0.72f, 0.75f));
        }

        // ------------------------------------------------------------------ the mist on the water

        void MakeMist()
        {
            const string name = "Mist on the water (local fog)";
            var old = weather.transform.Find(name);                          // left from before a domain reload
            if (old != null)
                CoreUtils.Destroy(old.gameObject);
            mistGo = new GameObject(name) { hideFlags = HideFlags.HideAndDontSave };
            mistGo.transform.SetParent(weather.transform, false);
            mist = mistGo.AddComponent<LocalVolumetricFog>();
            var p = mist.parameters;
            p.albedo = new Color(0.9f, 0.92f, 0.95f);
            p.meanFreePath = 100f;
            p.blendingMode = LocalVolumetricFogBlendingMode.Additive;
            p.maskMode = LocalVolumetricFogMaskMode.Texture;
            // 3 Oct: with the 3D mask the mist rendered black in batch shots; no mask (uniform) until that is understood
            p.volumeMask = null;
            p.scaleMode = LocalVolumetricFogScaleMode.ScaleInvariant;
            p.falloffMode = LocalVolumetricFogFalloffMode.Linear;
            p.positiveFade = new Vector3(0.05f, 0.75f, 0.05f);              // thins away towards its top
            p.negativeFade = new Vector3(0.05f, 0f, 0.05f);
            p.invertFade = false;
            p.distanceFadeStart = 100000f;
            p.distanceFadeEnd = 100000f;
            mist.parameters = p;
            mistGo.SetActive(false);
        }

        void PushMist(LevelWeather w, float wl, Vector3 eyePos)
        {
            if (mist == null)
                return;
            // 3 Oct: the local mist volume rendered black in every batch shot (mask or no mask); off until it is understood, the
            // global fog carries the mist for now
            bool on = false && w.MistDensity > 2e-4f && w.mistHeight > 0.2f;
            if (mistGo.activeSelf != on)
                mistGo.SetActive(on);
            if (!on)
                return;
            // a wide box lying on the water, kept round the camera (moved only when the camera has gone far, so the
            // patches don't swim with it)
            float extent = Mathf.Max(w.mistExtent, 200f);
            if (!mistPlaced || Mathf.Abs(eyePos.x - mistCentre.x) > extent * 0.25f || Mathf.Abs(eyePos.z - mistCentre.z) > extent * 0.25f)
            {
                mistCentre = new Vector3(eyePos.x, 0f, eyePos.z);
                mistPlaced = true;
            }
            float height = w.mistHeight + 1f;
            mistGo.transform.position = new Vector3(mistCentre.x, wl - 1f + height * 0.5f, mistCentre.z);
            var p = mist.parameters;
            p.size = new Vector3(extent, height, extent);
            // the mask averages about a half: twice the density gives the weather's mean
            p.meanFreePath = Mathf.Clamp(2f / w.MistDensity, 60f, 5000f);    // 3 Oct: 25 m made a black wall at dawn; a hundred metres is a river mist
            float patch = Mathf.Max(w.mistPatch, 5f);
            p.textureTiling = new Vector3(extent / patch, 1f, extent / patch);
            p.textureScrollingSpeed = new Vector3(w.WindVector.x, 0f, w.WindVector.y) * (0.3f / patch);   // tiles a second: a slow drift
            mist.parameters = p;
        }

        /// <summary>A tiling 3D noise: RGB white (no tint), A the mist's patches (HDRP reads the alpha as density).</summary>
        static Texture3D MakeMistMask()
        {
            const int n = 32, d = 8;
            var t = new Texture3D(n, n, d, TextureFormat.RGBA32, false)
            {
                name = "Weather: mist patches", wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.DontSave,
            };
            var px = new Color32[n * n * d];
            for (int z = 0; z < d; z++)
                for (int y = 0; y < n; y++)
                    for (int x = 0; x < n; x++)
                    {
                        float v = 0.6f * Noise3(x / (float)n, y / (float)n, z / (float)d, 4, 2, 7)
                                + 0.4f * Noise3(x / (float)n, y / (float)n, z / (float)d, 8, 3, 19);
                        float a = Mathf.Clamp01((v - 0.35f) * 1.8f);           // gaps between the patches
                        px[(z * n + y) * n + x] = new Color32(255, 255, 255, Byte(a));
                    }
            t.SetPixels32(px);
            t.Apply(false, false);
            return t;
        }

        // ------------------------------------------------------------------ the clouds' textures

        /// <summary>
        /// The kinds of cloud, as HDRP's clouds read them: across, the kind (the cloud map's "type"); up, the height in the
        /// cloud layer. R is how dense, G how much the noise shapes and erodes it (low: a smooth deck; high: puffs), B how
        /// much light gets in (darker bases). From the first build.
        /// </summary>
        static Texture2D MakeCloudKinds()
        {
            const int w = 256, h = 64;
            var texels = new Color32[w * h];
            for (int y = 0; y < h; y++)
            {
                float z = (y + 0.5f) / h;                                    // height in the cloud layer, 0 = its base
                for (int x = 0; x < w; x++)
                {
                    float t = (x + 0.5f) / w;
                    float stratus = Band(t, 0f, 0.24f), alto = Band(t, 0.24f, 0.45f), cumulus = Band(t, 0.45f, 0.68f), cb = Band(t, 0.68f, 1.01f);
                    float density = stratus * Slab(z, 0.01f, 0.4f, 0.05f)                                   // a thick low deck
                                  + alto * Slab(z, 0.46f, 0.6f, 0.04f)                                        // thin, mid-level
                                  + cumulus * Slab(z, 0.02f, 0.3f, 0.09f)                                     // low puffs, rounded tops
                                  + cb * Slab(z, 0.02f, 0.95f, 0.06f) * Mathf.Lerp(0.85f, 1f, z);             // the tower
                    float shaping = stratus * 0.16f + alto * 0.35f + cumulus * 0.8f + cb * 0.65f;
                    float light = stratus * Mathf.Lerp(0.45f, 1f, z / 0.4f) + alto * 0.85f + cumulus * Mathf.Lerp(0.4f, 1f, z / 0.3f) + cb * Mathf.Lerp(0.2f, 1f, z);
                    texels[y * w + x] = new Color32(Byte(density), Byte(shaping), Byte(light), 255);
                }
            }
            var lut = new Texture2D(w, h, TextureFormat.RGBA32, false, true)
            {
                name = "Weather: kinds of cloud", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.DontSave,
            };
            lut.SetPixels32(texels);
            lut.Apply(false, false);
            return lut;
        }

        /// <summary>Tiling patches of more and less cloud over the map, 0..1 about a half.</summary>
        static float[] MakePatches()
        {
            var p = new float[MapSize * MapSize];
            for (int y = 0; y < MapSize; y++)
                for (int x = 0; x < MapSize; x++)
                {
                    float u = x / (float)MapSize, v = y / (float)MapSize;
                    p[y * MapSize + x] = 0.55f * Noise3(u, v, 0.3f, 4, 1, 3) + 0.3f * Noise3(u, v, 0.7f, 8, 1, 11) + 0.15f * Noise3(u, v, 0.1f, 16, 1, 29);
                }
            return p;
        }

        /// <summary>
        /// HDRP's cloud map from the weather: R how much cloud, G rain, B which kind, A how tall (a share of the layer).
        /// Thin cover is patchy (fair-weather cumulus between blue); as the cover closes it becomes an unbroken grey
        /// deck of stratus; a storm's clouds tower.
        /// </summary>
        void BuildMap(float cover, float dark, float rain, float storm, float kindOverride)
        {
            float deck = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.62f, 0.9f, cover)) * (1f - storm);
            float stormy = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.2f, 0.6f, storm));
            float kind = Mathf.Lerp(Mathf.Lerp(Cumulus, Stratus, deck), Cumulonimbus, stormy);
            float top = Mathf.Lerp(Mathf.Lerp(0.36f, 0.45f, deck), 1f, stormy);
            float thin = Mathf.Clamp01(Mathf.InverseLerp(0.45f, 0.2f, cover)) * (1f - dark * 2f);   // a hazy, bright sky: a veil of altostratus
            kind = Mathf.Lerp(kind, Altostratus, Mathf.Clamp01(thin) * 0.5f);
            if (kindOverride >= 0f)
            {
                // the weather engine's kind: its column, and tall enough for its profile (the tower, the mid-level sheet)
                kind = kindOverride;
                top = kindOverride >= Cumulonimbus ? 1f : kindOverride == Altostratus ? 0.65f : top;
            }
            for (int i = 0; i < texels.Length; i++)
            {
                float n = patches[i];
                float c = Mathf.Clamp01(cover + (n - 0.5f) * 0.8f * (1f - cover * cover));
                texels[i] = new Color32(Byte(c), Byte(rain), Byte(kind), Byte(top));
            }
            cloudMap.SetPixels32(texels);
            cloudMap.Apply(false, false);
        }

        // ------------------------------------------------------------------ helpers

        /// <summary>1 inside [from, to), easing out either side, so neighbouring kinds cross-fade where a map blends them.</summary>
        static float Band(float t, float from, float to) =>
            Mathf.Clamp01((t - from + 0.06f) / 0.12f) * Mathf.Clamp01((to + 0.06f - t) / 0.12f);

        /// <summary>1 between two heights, with soft edges.</summary>
        static float Slab(float z, float bottom, float top, float soft) =>
            Mathf.SmoothStep(0f, 1f, (z - bottom) / soft + 0.5f) * (1f - Mathf.SmoothStep(0f, 1f, (z - top) / soft + 0.5f));

        static byte Byte(float v) => (byte)Mathf.Clamp(Mathf.RoundToInt(v * 255f), 0, 255);

        /// <summary>Tiling value noise in [0, 1]: a lattice of cellsXY by cellsZ cells over the unit cube, wrapped, seeded.</summary>
        static float Noise3(float u, float v, float w, int cellsXY, int cellsZ, int seed)
        {
            float x = u * cellsXY, y = v * cellsXY, z = w * cellsZ;
            int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y), z0 = Mathf.FloorToInt(z);
            float fx = Smooth(x - x0), fy = Smooth(y - y0), fz = Smooth(z - z0);
            float Lattice(int i, int j, int k)
            {
                i = ((i % cellsXY) + cellsXY) % cellsXY;
                j = ((j % cellsXY) + cellsXY) % cellsXY;
                k = ((k % cellsZ) + cellsZ) % cellsZ;
                unchecked
                {
                    uint h = (uint)(i * 73856093) ^ (uint)(j * 19349663) ^ (uint)(k * 83492791) ^ (uint)(seed * 2654435761);
                    h ^= h >> 13;
                    h *= 0x5bd1e995;
                    h ^= h >> 15;
                    return (h & 0xFFFFFF) / (float)0x1000000;
                }
            }
            float c00 = Mathf.Lerp(Lattice(x0, y0, z0), Lattice(x0 + 1, y0, z0), fx);
            float c10 = Mathf.Lerp(Lattice(x0, y0 + 1, z0), Lattice(x0 + 1, y0 + 1, z0), fx);
            float c01 = Mathf.Lerp(Lattice(x0, y0, z0 + 1), Lattice(x0 + 1, y0, z0 + 1), fx);
            float c11 = Mathf.Lerp(Lattice(x0, y0 + 1, z0 + 1), Lattice(x0 + 1, y0 + 1, z0 + 1), fx);
            return Mathf.Lerp(Mathf.Lerp(c00, c10, fy), Mathf.Lerp(c01, c11, fy), fz);
        }

        static float Smooth(float t) => t * t * (3f - 2f * t);
    }
}
