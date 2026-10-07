// HDRP names are from Library/PackageCache/com.unity.render-pipelines.high-definition@8aadf824e504/Runtime:
//   Sky/VisualEnvironment.cs (skyType, cloudType, skyAmbientMode, renderingSpace), Sky/PhysicallyBasedSky,
//   Sky/CloudSystem/CloudLayer/CloudLayer.cs (opacity, layerA.opacityR/G/B/A, layerA.tint),
//   Lighting/AtmosphericScattering/Fog.cs (enabled, meanFreePath, baseHeight, maximumHeight, enableVolumetricFog),
//   PostProcessing/Components/Exposure.cs (mode, fixedExposure), Material/MaterialExtension.cs:369 (HDMaterial.ValidateMaterial).
// The Sun's and the Moon's set-up is LevelClock.SetUp's (Assets/Kapman/Code/Runtime/Levels/LevelClock.cs), line for line.
using System;
using System.Collections.Generic;
using System.IO;
using Kapman.World;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.Game.ShipView
{
    /// <summary>
    /// THE STAGE OF THE 3D SHIP VIEW (the user, 7 Oct 2026: "start working on the 3D view for the ship, it'll just be
    /// water and when it docks just a simple dock model ... simulated ocean weather wind everything"). A small world of
    /// its own, built at run time from a kit (ShipStageKit), on its own layer and far from the chart's sailing world:
    /// the sea (StageSea its state; drawn as HDRP's own water by StageHdrpSea, or as the stage's own mesh by StageWater),
    /// the boat held at its middle (StageShip), the jetty she lies at (StageDock), a Sun
    /// and a Moon placed for her own latitude (StageSky), a sky, cloud, fog and exposure of its own on a volume only its
    /// camera takes, and that camera (ShipStageView). It reads one record a frame (ShipStageState, from Source) and
    /// knows nothing else of the game. It holds none of the 3D levels' singletons (no LevelInfo, LevelWeather, HdrpSea,
    /// SkyDome): the chart game reads those statics and would take the chart for a level. Its HDRP water surface is one
    /// the game's finders cannot see (StageHdrpSea says how).
    /// Its axes are the compass: x east, z north. She keeps her true heading and the water goes by under her.
    /// </summary>
    [DefaultExecutionOrder(960)]                                        // after the chart's ship (20) and the frame (900), before the map draws (1000)
    public sealed class ShipStage : MonoBehaviour
    {
        public static ShipStage Current { get; private set; }

        public ShipStageKit kit;
        /// <summary>Who fills the record (the chart game, a test).</summary>
        public IShipStageSource Source;
        /// <summary>The record as last read.</summary>
        [NonSerialized] public ShipStageState State = ShipStageState.Fair();
        /// <summary>The picture fills the screen (the camera stands a little further off).</summary>
        public bool Big;
        /// <summary>
        /// THE SHOWROOM (the user, 7 Oct 2026: "just the ship, a rotating floating 3D model of the ship rotating like
        /// the old psx demos, no water just the pink void"). The boat alone: no sea, no jetty, no ropes, no rain, no
        /// mist, no sky behind her; a flat pink where the sky was; she hangs a little above nothing, rocking slowly, and
        /// the camera goes round her with the light held to it, so she seems to turn under a lamp. The record of the
        /// game is not read while it is on: it is always a fair noon.
        /// </summary>
        public bool Showroom
        {
            get => showroom;
            set
            {
                if (showroom == value)
                    return;
                showroom = value;
                snap = true;
                if (!value)
                    LeaveShowroom();
            }
        }
        bool showroom;
        float showTurn;
        /// <summary>The void's colour as the picture should show it, and whether the camera's clear colour is taken as
        /// light to be exposed like the rest (so it must be made as bright as the scene's exposure expects).</summary>
        public static Color ShowroomVoid = new Color(0.86f, 0.42f, 0.66f);
        public static bool ShowroomVoidIsExposed = false;
        /// <summary>Degrees a second she turns in the showroom.</summary>
        public static float ShowroomTurn = 24f;

        /// <summary>
        /// THE TITLE'S SEA (the user, 7 Oct 2026: "a top down of a stormy ocean with rain, with the title of the game
        /// animated in the middle"). No boat, no jetty: a gale's sea under rain, seen from almost straight above by a
        /// camera that turns slowly and drifts. The record of the game is not read while it is on.
        /// </summary>
        public bool TitleSea
        {
            get => titleSea;
            set
            {
                if (titleSea == value)
                    return;
                titleSea = value;
                titleSince = -1f;
                snap = true;
                if (!value)
                    LeaveTitleSea();
            }
        }
        bool titleSea;
        /// <summary>Metres the title's camera hangs over the sea, and degrees a second it turns.</summary>
        public static float TitleSeaHeight = 31f, TitleSeaTurn = 0f;
        /// <summary>Degrees the title's camera looks down from level (the user, 7 Oct 2026: "need to be looking at the
        /// ocean on more of an angle"; it was 84, almost straight down). With its 50 degrees of view the top of the
        /// picture still looks down at the water: there is no horizon in it.</summary>
        public static float TitleSeaPitch = 88f;

        /// <summary>
        /// THE TITLE'S WEATHER GOES ROUND FOR EVER (the user, 7 Oct 2026: "let it cycle forever through all possible
        /// weather conditions for as long as the game isn't started"): each stands a while and passes into the next, and
        /// after the last comes the first. The sea follows the wind in its own time, as it does in the game.
        /// </summary>
        public static readonly (string name, float wind, float rain, float snow, float fog)[] TitleWeather =
        {
            ("a gale with rain", 24f, 0.9f, 0f, 0f),
            ("the rain passing on a fresh breeze", 12f, 0.3f, 0f, 0f),
            ("light airs", 3f, 0f, 0f, 0f),
            ("fog on a slow swell", 2f, 0f, 0f, 0.8f),
            ("a rising wind and whitecaps", 16f, 0f, 0f, 0f),
            ("snow on a hard wind", 20f, 0f, 0.9f, 0.1f),
        };
        /// <summary>Seconds each weather of the title stands, and of those the last seconds in which it passes into the next.</summary>
        public static float TitleSeaEach = 26f, TitleSeaPass = 9f;
        /// <summary>Seconds into the round, set by hand (a picture, a test); null: the wall clock since the sea came up.</summary>
        public static float? TitleSeaClock;
        /// <summary>How much the picture's brightness is lifted before a look of few tones cuts it (the raking light of
        /// the title's sea is dim: without this every wave falls into the darkest tone).</summary>
        public static float TitleSeaLift = 1.25f;
        /// <summary>The share of the Sun that lights the title sea (its light stands high behind the camera).</summary>
        public static float TitleSeaSun = 0.28f;
        /// <summary>The weather of the title now, in words.</summary>
        public string TitleWeatherNow { get; private set; } = "";
        float titleSince = -1f;
        int titleFresh;

        ShipStageState StormState()
        {
            if (titleSince < 0f)
            {
                titleSince = Time.unscaledTime;
                titleFresh = 4;                                           // (the sea is the title sea at once: see TitleSeaStep)
            }
            float t = TitleSeaClock ?? (Time.unscaledTime - titleSince);
            int n = TitleWeather.Length;
            int at = (int)Mathf.Floor(t / TitleSeaEach) % n;
            float within = Mathf.Repeat(t, TitleSeaEach);
            float k = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(TitleSeaEach - TitleSeaPass, TitleSeaEach, within));
            var a = TitleWeather[at];
            var b = TitleWeather[(at + 1) % n];
            TitleWeatherNow = k < 0.5f ? a.name : b.name;
            float wind = Mathf.Lerp(a.wind, b.wind, k);

            var s = ShipStageState.Fair();
            s.place = StagePlace.OpenSea;
            s.portId = "";
            s.moored = false;
            s.berthing = false;
            s.berthAhead = 0f;
            s.underOars = false;
            s.lyingTo = true;
            s.speed = 0f;
            s.heading = 0f;
            s.windFrom = 250f;
            s.windSpeed = wind;
            s.gust = wind * 1.3f;
            // (the sky is left broken through every weather: the look turns the picture into tones of purple, and the
            // light on the title's sea is its own, laid low: PsxStageLook.Light. The weather is in the sea and the air.)
            s.cloud = 0.6f;
            s.overcast = 0f;
            s.storm = 0f;
            s.rain = Mathf.Lerp(a.rain, b.rain, k);
            s.snow = Mathf.Lerp(a.snow, b.snow, k);
            s.fog = Mathf.Lerp(a.fog, b.fog, k);
            s.visibility = Mathf.Lerp(25000f, 400f, s.fog);
            s.cloudLight = 0.8f;
            s.fetchKm = 300f;
            s.depth = 40f;
            s.latitude = 55.0;
            s.longitude = 4.0;
            s.julianDay = ShipStageState.JulianDayOf(793, 260, 14.0);
            s.hours = 14.0;
            s.timeFactor = 1f;
            return s;
        }

        float nearBefore = -1f;

        // ------------------------------------------------------------------ the name, afloat

        /// <summary>
        /// THE NAME'S LETTERS ARE THINGS, AFLOAT (the user, 7 Oct 2026: "can you 3d the Title text and have it float on
        /// the water"). Each letter's picture of the art folder (title/title.letter.N.png) is made into a slab: its
        /// painted face up, a side wherever the picture has an edge, about a metre thick. The six lie on the title's
        /// sea in a row, facing the camera, and each rides the water under it: lifted and dropped by the swell, and
        /// leaning with its slope. The show says how far each has come up (TitleIn, 0 under the water to 1 afloat) and
        /// how far it has gone down again (TitleAway).
        /// </summary>
        sealed class Afloat
        {
            public Transform t;
            public float x, y;
            public Vector3 lean = Vector3.up;
        }

        readonly List<Afloat> titleLetters = new List<Afloat>();
        Transform titleRoot;
        Color seaDeepWas, seaCrestWas, seaFoamWas;
        bool seaColoursKept;
        float seaSkyWas = -1f;
        /// <summary>How far each letter has come up out of the sea (0 to 1), and how far it has gone under again.</summary>
        public float[] TitleIn = new float[0], TitleAway = new float[0];
        /// <summary>The name's letters are on the stage as things (there were pictures to make them of).</summary>
        public bool TitleLettersUp => titleLetters.Count > 0;
        /// <summary>Metres to a pixel of a letter's picture, the slab's thickness, and how far beyond the middle of the
        /// picture the row lies.</summary>
        public static float TitleLetterPixel = 0.11f, TitleLetterThick = 1.3f, TitleNameAhead = 0.5f;
        /// <summary>How far a letter stands up from lying flat (degrees: 0 lies afloat as a plank does, the whole of it
        /// seen; stood up, the tail of the p went under and the name read wrong), and how high its middle rides over
        /// the water.</summary>
        public static float TitleLetterStand = 0f, TitleLetterRide = 0.3f;
        /// <summary>The title's sea in the game's own colour: its deeps, its crests and its foam, as paint.</summary>
        public static Color TitleSeaDeep = new Color(0.39f, 0.11f, 0.41f), TitleSeaCrest = new Color(0.66f, 0.25f, 0.62f),
                            TitleSeaFoam = new Color(0.95f, 0.87f, 0.72f);
        /// <summary>The rain over the title's sea: cream, as in the picture of 7 Oct that the user called perfect.</summary>
        public static Color TitleRain = new Color(0.95f, 0.87f, 0.72f, 0.55f);
        /// <summary>How much of the sky the title sea throws back (the look own: 0.6).</summary>
        public static float TitleSeaSky = 0.08f;

        void BuildTitleLetters()
        {
            if (titleRoot != null)
                return;
            titleRoot = new GameObject("The name, afloat").transform;
            titleRoot.SetParent(transform, false);
            float across = 0f;
            for (int i = 1; i <= 16; i++)
            {
                string file = null;
                foreach (string kind in new[] { "final", "placeholder" })
                {
                    string path = Path.Combine(Kapman.Game.Frame.FrameArt.Root, kind, "title", "title.letter." + i + ".png");
                    if (File.Exists(path))
                    {
                        file = path;
                        break;
                    }
                }
                if (file == null)
                    break;
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
                if (!tex.LoadImage(File.ReadAllBytes(file), false))
                    continue;
                var mesh = LetterSlab(tex, TitleLetterPixel, TitleLetterThick);
                if (mesh == null)
                    continue;
                var go = new GameObject("Letter " + i, typeof(MeshFilter), typeof(MeshRenderer));
                go.transform.SetParent(titleRoot, false);
                go.GetComponent<MeshFilter>().sharedMesh = mesh;
                var m = new Material(Shader.Find("HDRP/Lit")) { name = "The name's letter " + i, hideFlags = HideFlags.DontSave };
                m.SetTexture("_BaseColorMap", tex);
                m.SetColor("_BaseColor", Color.white);
                m.SetFloat("_Smoothness", 0.05f);
                m.SetFloat("_Metallic", 0f);
                HDMaterial.ValidateMaterial(m);
                var r = go.GetComponent<MeshRenderer>();
                r.sharedMaterial = m;
                r.shadowCastingMode = ShadowCastingMode.Off;
                float w = tex.width * TitleLetterPixel;
                titleLetters.Add(new Afloat { t = go.transform, x = across + w * 0.5f });
                across += w;
            }
            foreach (var l in titleLetters)
                l.x -= across * 0.5f;                                    // (the row's middle is the picture's)
            TitleIn = new float[titleLetters.Count];
            TitleAway = new float[titleLetters.Count];
            if (titleLetters.Count > 0)
                Adopt(titleRoot.gameObject);                             // (the stage's layer; and the look takes their materials for its own)
        }

        /// <summary>
        /// A letter's picture as a slab lying flat: its face up (y), its head away (z), a pixel of the picture so many
        /// metres. The face is the picture itself (a strip for each run of painted pixels in a row); there is a side
        /// wherever a painted pixel has an unpainted one beside it, in that pixel's own colour (the dark line round
        /// the letter); the underside is the face again. The picture's flat shadow (its near black) is left out: a
        /// thing has its own.
        /// </summary>
        static Mesh LetterSlab(Texture2D tex, float pixel, float thick)
        {
            int W = tex.width, H = tex.height;
            var px = tex.GetPixels32();
            bool Solid(int x, int y)
            {
                if (x < 0 || y < 0 || x >= W || y >= H)
                    return false;
                var c = px[y * W + x];
                return c.a > 127 && !(c.r < 20 && c.g < 16 && c.b < 26);
            }
            var v = new List<Vector3>();
            var n = new List<Vector3>();
            var uv = new List<Vector2>();
            var tri = new List<int>();
            float ox = -W * pixel * 0.5f, oz = -H * pixel * 0.5f, t = thick * 0.5f;
            void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal, Vector2 ua, Vector2 ub, Vector2 uc, Vector2 ud)
            {
                int at = v.Count;
                v.Add(a); v.Add(b); v.Add(c); v.Add(d);
                n.Add(normal); n.Add(normal); n.Add(normal); n.Add(normal);
                uv.Add(ua); uv.Add(ub); uv.Add(uc); uv.Add(ud);
                tri.Add(at); tri.Add(at + 1); tri.Add(at + 2);
                tri.Add(at); tri.Add(at + 2); tri.Add(at + 3);
            }
            for (int y = 0; y < H; y++)
            {
                float z0 = oz + y * pixel, z1 = z0 + pixel;
                float v0 = y / (float)H, v1 = (y + 1) / (float)H;
                int x = 0;
                while (x < W)
                {
                    if (!Solid(x, y))
                    {
                        x++;
                        continue;
                    }
                    int from = x;
                    while (x < W && Solid(x, y))
                        x++;
                    float x0 = ox + from * pixel, x1 = ox + x * pixel, u0 = from / (float)W, u1 = x / (float)W;
                    // the face, and the underside
                    Quad(new Vector3(x0, t, z0), new Vector3(x0, t, z1), new Vector3(x1, t, z1), new Vector3(x1, t, z0), Vector3.up,
                         new Vector2(u0, v0), new Vector2(u0, v1), new Vector2(u1, v1), new Vector2(u1, v0));
                    Quad(new Vector3(x0, -t, z0), new Vector3(x1, -t, z0), new Vector3(x1, -t, z1), new Vector3(x0, -t, z1), Vector3.down,
                         new Vector2(u0, v0), new Vector2(u1, v0), new Vector2(u1, v1), new Vector2(u0, v1));
                }
                for (x = 0; x < W; x++)
                {
                    if (!Solid(x, y))
                        continue;
                    float x0 = ox + x * pixel, x1 = x0 + pixel;
                    var own = new Vector2((x + 0.5f) / W, (y + 0.5f) / H);
                    if (!Solid(x + 1, y))
                        Quad(new Vector3(x1, -t, z0), new Vector3(x1, t, z0), new Vector3(x1, t, z1), new Vector3(x1, -t, z1), Vector3.right, own, own, own, own);
                    if (!Solid(x - 1, y))
                        Quad(new Vector3(x0, -t, z1), new Vector3(x0, t, z1), new Vector3(x0, t, z0), new Vector3(x0, -t, z0), Vector3.left, own, own, own, own);
                    if (!Solid(x, y + 1))
                        Quad(new Vector3(x1, -t, z1), new Vector3(x1, t, z1), new Vector3(x0, t, z1), new Vector3(x0, -t, z1), Vector3.forward, own, own, own, own);
                    if (!Solid(x, y - 1))
                        Quad(new Vector3(x0, -t, z0), new Vector3(x0, t, z0), new Vector3(x1, t, z0), new Vector3(x1, -t, z0), Vector3.back, own, own, own, own);
                }
            }
            if (v.Count == 0)
                return null;
            var mesh = new Mesh { name = "A letter of the name", hideFlags = HideFlags.DontSave };
            if (v.Count > 65000)
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(v);
            mesh.SetNormals(n);
            mesh.SetUVs(0, uv);
            mesh.SetTriangles(tri, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>Each letter in its place in the row, on the water that is under it now, leaning with it.</summary>
        void FloatTitleLetters(Vector3 away, float yaw, float dt)
        {
            if (titleRoot == null)
                return;
            if (!titleRoot.gameObject.activeSelf)
                titleRoot.gameObject.SetActive(true);
            Vector3 right = Quaternion.Euler(0f, yaw, 0f) * Vector3.right;
            const float d = 2.6f;
            float ease = snap ? 1f : 1f - Mathf.Exp(-dt * 4f);
            for (int i = 0; i < titleLetters.Count; i++)
            {
                var l = titleLetters[i];
                Vector3 at = away * TitleNameAhead + right * l.x;
                float h = Height(at.x, at.z);
                float across = Height(at.x + right.x * d, at.z + right.z * d) - Height(at.x - right.x * d, at.z - right.z * d);
                float along = Height(at.x + away.x * d, at.z + away.z * d) - Height(at.x - away.x * d, at.z - away.z * d);
                Vector3 slope = Vector3.Cross(away * (2f * d) + Vector3.up * along, right * (2f * d) + Vector3.up * across).normalized;
                if (slope.y < 0f)
                    slope = -slope;
                l.y = Mathf.Lerp(l.y, h, ease);
                l.lean = Vector3.Slerp(l.lean, slope, ease);
                float k = i < TitleIn.Length ? Mathf.Clamp01(TitleIn[i]) : 1f;
                k = k * k * (3f - 2f * k);
                float gone = i < TitleAway.Length ? Mathf.Clamp01(TitleAway[i]) : 0f;
                // It lies afloat, face up, a good half of its thickness out of the water: its face and its near side are
                // both seen from the camera above. It comes up from under, and goes down again turning over.
                float y = l.y + TitleLetterRide - (1f - k) * 6.5f - gone * gone * 8f;
                l.t.localPosition = new Vector3(at.x, y, at.z);
                l.t.localRotation = Quaternion.FromToRotation(Vector3.up, Vector3.Slerp(Vector3.up, l.lean, 0.8f)) * Quaternion.Euler(0f, yaw, 0f)
                                    * Quaternion.Euler(-TitleLetterStand + gone * 62f, 0f, gone * 28f * (i % 2 == 0 ? 1f : -1f));
                bool seen = k > 0.01f && gone < 0.995f;
                if (l.t.gameObject.activeSelf != seen)
                    l.t.gameObject.SetActive(seen);
            }
        }

        void TitleSeaStep(in ShipStageState s, float dt)
        {
            if (Ship != null && Ship.Root != null && Ship.Root.gameObject.activeSelf)
                Ship.Root.gameObject.SetActive(false);
            if (Dock != null && Dock.Root != null && Dock.Root.gameObject.activeSelf)
                Dock.Root.gameObject.SetActive(false);
            bowLine.enabled = sternLine.enabled = false;
            wakeRenderer.enabled = false;
            mistVeil = 0f;
            if (titleFresh > 0)
            {
                // (its first frames: the sea and the light are those of the title at once, not eased over from whatever
                // the stage showed before, which came up as a pale sheet for a second)
                titleFresh--;
                Settle();
            }
            float t = Time.unscaledTime;
            var cam = View.Camera.transform;
            float pitch = Mathf.Clamp(TitleSeaPitch, 30f, 89f);
            float yaw = Mathf.Repeat(t * TitleSeaTurn, 360f);
            Vector3 away = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;   // the way the camera looks, on the flat
            float back = TitleSeaHeight / Mathf.Tan(pitch * Mathf.Deg2Rad);
            // (it stands back so that the middle of the picture is the middle of the sea's grid, and goes slowly round it)
            cam.localPosition = -away * back + Vector3.up * TitleSeaHeight;
            cam.localRotation = Quaternion.Euler(pitch, yaw, 0f);
            View.Camera.fieldOfView = 50f;
            // the name's letters afloat on it, and the sea in the game's own colour (in a look that keeps colours)
            BuildTitleLetters();
            FloatTitleLetters(away, yaw, dt);
            if (look is PsxStageLook psx && psx.Grid != null)
            {
                if (!seaColoursKept)
                {
                    seaDeepWas = psx.Grid.deep;
                    seaCrestWas = psx.Grid.crest;
                    seaFoamWas = psx.Grid.foam;
                    seaColoursKept = true;
                }
                psx.Grid.deep = TitleSeaDeep;
                psx.Grid.crest = TitleSeaCrest;
                psx.Grid.foam = TitleSeaFoam;
                psx.Grid.noShip = true;
                if (psx.Grid.Material != null)
                {
                    if (seaSkyWas < 0f)
                        seaSkyWas = psx.Grid.Material.GetFloat("_SeaSky");
                    psx.Grid.Material.SetColor("_SeaFoamColour", TitleSeaFoam);
                    // (little of the sky thrown back: under a gale it is grey, and would drown the colour)
                    psx.Grid.Material.SetFloat("_SeaSky", TitleSeaSky);
                }
            }
            if (nearBefore < 0f)
                nearBefore = View.Camera.nearClipPlane;
            View.Camera.nearClipPlane = 4f;                               // (no drop right at the glass, as wide as a plank)

            // THE RAIN ALL OVER THE PICTURE (the user, 7 Oct 2026: "the rain needs to be all over the screen"). In the
            // game it falls from a small sheet hung before a camera that looks along the water. Here the sheet is laid
            // level, well over the camera, wide enough for everything the picture takes in, and a little upwind so the
            // drops are carried into it; the drops are bigger (they are seen from far off) and live until they reach
            // the sea. Snow falls slower.
            float amount = Mathf.Clamp01(s.rain + s.snow * 0.6f);
            float snow = s.rain + s.snow > 0.001f ? s.snow / (s.rain + s.snow) : 0f;
            float fall = Mathf.Lerp(13f, 5f, snow);
            float drop = TitleSeaHeight + 7f;
            float life = drop / fall + 0.1f;
            var main = rain.main;
            main.startLifetime = life;
            main.startSpeed = fall;
            main.startSize = Mathf.Lerp(0.09f, 0.16f, snow);
            main.startColor = TitleRain;
            main.maxParticles = 7000;
            var shape = rain.shape;
            shape.scale = new Vector3(120f, 100f, 1f);
            float to = (s.windFrom + 180f) * Mathf.Deg2Rad;
            Vector3 carried = new Vector3(Mathf.Sin(to), 0f, Mathf.Cos(to)) * (s.windSpeed * 0.35f * life * 0.5f);
            rain.transform.position = transform.TransformPoint(away * 8f - carried + Vector3.up * drop);
            rain.transform.rotation = transform.rotation * Quaternion.Euler(90f, 0f, 0f);
            var emission = rain.emission;
            emission.rateOverTime = Mathf.Min(amount * 3200f, 6500f / life);
            if (amount > 0.01f && !rain.isPlaying)
                rain.Play();
        }

        void LeaveTitleSea()
        {
            if (Ship != null && Ship.Root != null)
                Ship.Root.gameObject.SetActive(true);
            if (Dock != null && Dock.Root != null)
                Dock.Root.gameObject.SetActive(true);
            if (View != null && View.Camera != null && nearBefore > 0f)
                View.Camera.nearClipPlane = nearBefore;
            nearBefore = -1f;
            if (titleRoot != null)
                titleRoot.gameObject.SetActive(false);
            if (seaColoursKept && look is PsxStageLook psx && psx.Grid != null)
            {
                psx.Grid.deep = seaDeepWas;
                psx.Grid.crest = seaCrestWas;
                psx.Grid.foam = seaFoamWas;
                psx.Grid.noShip = false;
                if (psx.Grid.Material != null)
                {
                    psx.Grid.Material.SetColor("_SeaFoamColour", seaFoamWas);
                    if (seaSkyWas >= 0f)
                        psx.Grid.Material.SetFloat("_SeaSky", seaSkyWas);
                }
            }
            seaSkyWas = -1f;
            seaColoursKept = false;
            if (rain != null)
            {
                // the game's own rain again: its small sheet hung before the camera (as Rain() made it)
                rain.Clear();
                rain.transform.localPosition = new Vector3(0f, 9f, 9f);
                rain.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                var main = rain.main;
                main.startLifetime = 1.3f;
                main.startSpeed = 13f;
                main.startSize = 0.025f;
                main.startColor = new Color(0.7f, 0.74f, 0.78f, 0.35f);
                main.maxParticles = 1500;
                var shape = rain.shape;
                shape.scale = new Vector3(34f, 22f, 1f);
            }
        }

        static ShipStageState ShowroomState()
        {
            var s = ShipStageState.Fair();
            s.place = StagePlace.River;
            s.portId = "";
            s.moored = false;
            s.berthing = false;
            s.berthAhead = 0f;
            s.underOars = false;
            s.speed = 0.06f;                                              // (way enough for the sail to be set, none to raise a wake)
            s.heading = 0f;
            s.windFrom = 180f;
            s.windSpeed = 3f;
            s.gust = 3.5f;
            s.cloud = 0f;
            s.overcast = 0f;
            s.storm = 0f;
            s.rain = 0f;
            s.snow = 0f;
            s.fog = 0f;
            s.visibility = 40000f;
            s.cloudLight = 1f;
            s.timeFactor = 1f;
            return s;
        }

        /// <summary>After the stage's own step in the showroom: what is not the boat is put away, the void is cleared
        /// to, she rocks, and the camera and the light go round.</summary>
        void ShowroomStep(float dt)
        {
            if (SeaObject != null && SeaObject.activeSelf)
                SeaObject.SetActive(false);
            if (Dock != null && Dock.Root != null && Dock.Root.gameObject.activeSelf)
                Dock.Root.gameObject.SetActive(false);
            bowLine.enabled = sternLine.enabled = false;
            wakeRenderer.enabled = false;
            if (rain != null)
            {
                var emission = rain.emission;
                emission.rateOverTime = 0f;
            }
            fog.enabled.Override(false);
            mistVeil = 0f;
            View.Data.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
            float lift = ShowroomVoidIsExposed ? 1.2f * Mathf.Pow(2f, ev) : 1f;
            Color linear = ShowroomVoid.linear;
            View.Data.backgroundColorHDR = new Color(linear.r * lift, linear.g * lift, linear.b * lift, 1f);

            float t = Time.unscaledTime;
            float L = Ship.Length;
            Ship.Root.localPosition = new Vector3(0f, 0.1f * L * 0.2f * Mathf.Sin(t * 1.1f), 0f);
            Ship.Root.localRotation = Quaternion.Euler(3.5f * Mathf.Sin(t * 0.8f), 0f, 5f * Mathf.Sin(t * 0.63f + 1f));
            showTurn = snap ? 205f : Mathf.Repeat(showTurn + ShowroomTurn * dt, 360f);
            Vector3 eye = Quaternion.Euler(0f, showTurn, 0f) * Vector3.forward * (L * 1.85f);
            eye.y = L * 0.5f;
            Vector3 aim = new Vector3(0f, L * 0.26f, 0f);
            var cam = View.Camera.transform;
            cam.localPosition = eye;
            cam.localRotation = Quaternion.LookRotation(aim - eye, Vector3.up);
            View.Camera.fieldOfView = 36f;
            // the lamp: over the camera's left shoulder, so her shading changes as she turns
            if (Sun != null)
                Sun.transform.rotation = cam.rotation * Quaternion.Euler(38f, 32f, 0f);
        }

        void LeaveShowroom()
        {
            if (SeaObject != null)
                SeaObject.SetActive(true);
            if (Dock != null && Dock.Root != null)
                Dock.Root.gameObject.SetActive(true);
            if (fog != null)
                fog.enabled.Override(true);
            if (View != null && View.Data != null)
                View.Data.clearColorMode = HDAdditionalCameraData.ClearColorMode.Sky;
        }
        /// <summary>Where the camera stands (the user, 7 Oct 2026: "try all of them with different angles, maybe one from almost
        /// directly above but angled slightly"): 0 astern on her quarter, 1 abeam and a little ahead, 2 off her bow looking
        /// back, 3 from almost overhead. Bearings are from her bow, clockwise; distances and heights in her lengths.</summary>
        public static readonly string[] AngleNames = { "astern", "abeam", "off the bow", "from above" };
        static readonly float[] AngleBearing = { -148f, 72f, 28f, -165f };
        static readonly float[] MooredBearing = { -106f, -76f, -46f, -165f };
        static readonly float[] AngleOff = { 1.5f, 1.55f, 1.7f, 0.42f };
        static readonly float[] AngleUp = { 0f, 0.30f, 0.30f, 2.1f };       // (0: the kit's own eye height)
        public int Angle { get; private set; }
        public void SetAngle(int angle)
        {
            Angle = ((angle % AngleNames.Length) + AngleNames.Length) % AngleNames.Length;
            snap = true;
        }
        /// <summary>Off: the stage stands still and draws nothing (the frame is hidden).</summary>
        public bool Running = true;

        public StageSea Sea { get; private set; }
        public StageWater Water { get; private set; }
        public StageShip Ship { get; private set; }
        public StageDock Dock { get; private set; }
        public ShipStageView View { get; private set; }
        public Volume Volume { get; private set; }
        public Light Sun { get; private set; }
        public Light Moon { get; private set; }
        public StageSkyNow SkyNow { get; private set; }
        /// <summary>The lighting model's exposure for this moment (EV100), and the one the picture is drawn at.</summary>
        public float SceneEv { get; private set; }
        public float Ev => ev;
        /// <summary>The jetty is on the stage, and where its water-side edge is (stage metres east and north of her).</summary>
        public bool DockUp { get; private set; }
        /// <summary>How far the mist has closed in for the moment a jetty is put at her side or taken away (0 open .. 1 closed).</summary>
        public float Veil => mistVeil;
        /// <summary>She is pushing off from the jetty: it is kept as it lay to her until her length is clear of it (DockStep).</summary>
        public bool PushingOff { get; private set; }
        public Vector3 DockAt => dockAt;
        /// <summary>Another sea than StageSea for her to float on (a look that draws waves of its own, the other builder's
        /// PsxSeaGrid, sets this while its sea is the one drawn): the height over the still water at a point so many metres
        /// east and north of her. Null: StageSea's.</summary>
        public Func<float, float, float> HeightAt;
        /// <summary>A look that draws her wake itself (the console sea paints it into its own triangles) switches the
        /// mesh sea's ribbon off with this.</summary>
        public bool WakeRibbonOff;
        /// <summary>Something was put on the stage or taken onto it (the jetty came, she was fitted out, a man was seated):
        /// a look that swaps materials finds them again. Once a frame at most.</summary>
        public event Action Changed;
        bool changed;

        /// <summary>HDRP's own water, the look lab's sea (null where the kit asks for the mesh sea only, or this HDRP
        /// asset draws no water).</summary>
        public StageHdrpSea Hdrp { get; private set; }
        /// <summary>Her wake as foam in HDRP's water (null with the mesh sea only: there the ribbon is her wake).</summary>
        public StageWake FoamWake { get; private set; }

        /// <summary>
        /// WHICH SEA IS DRAWN: HDRP's water (the plain look), or the stage's own mesh (Water: what a PSX look draws on).
        /// The other is switched off. A look that wants the mesh sea sets this to Mesh in Apply and back in Remove.
        /// Either way the sea's state is the same (Sea), and she floats on the one that is drawn.
        /// </summary>
        public StageSeaKind SeaKind
        {
            get => seaKind;
            set
            {
                seaKind = value == StageSeaKind.HdrpWater && Hdrp == null ? StageSeaKind.Mesh : value;
                if (Hdrp != null && Hdrp.Object != null)
                    Hdrp.Object.SetActive(seaKind == StageSeaKind.HdrpWater);
                if (Water != null && Water.Object != null)
                    Water.Object.SetActive(seaKind == StageSeaKind.Mesh);
                if (Ship != null)
                    Ship.LidOn(seaKind == StageSeaKind.HdrpWater);
                changed = true;
            }
        }
        StageSeaKind seaKind;

        /// <summary>The object of the sea that is drawn now (a look that hides the sea while it draws its own).</summary>
        public GameObject SeaObject => seaKind == StageSeaKind.HdrpWater && Hdrp != null ? Hdrp.Object : Water.Object;

        /// <summary>The water's height over its level where she and the camera are concerned: a look's own sea if it gave
        /// one, HDRP's waves while its water is drawn and answering, else the stage's eight waves.</summary>
        public float Height(float east, float north)
        {
            if (HeightAt != null)
                return HeightAt(east, north);
            if (seaKind == StageSeaKind.HdrpWater && Hdrp != null && Hdrp.TryHeight(kit.origin + new Vector3(east, 0f, north), out float y))
                return y - kit.origin.y;
            return Sea.Height(east, north);
        }

        HDAdditionalLightData sunData, moonData;
        VisualEnvironment environment;
        PhysicallyBasedSky sky;
        CloudLayer clouds;
        Fog fog;
        Exposure exposure;
        ParticleSystem rain;
        LineRenderer bowLine, sternLine;
        Mesh wakeMesh;
        MeshRenderer wakeRenderer;
        float nextCameraLook;
        Vector3[] wakeVerts;
        Color32[] wakeColours;
        Vector3 dockAt;
        float dockYaw, camAzimuth = -148f, camHeading, ev = 12f;
        string dockFor;
        double[] precession;
        double precessionDay;
        bool snap = true;
        float mistVeil, sailedSince;
        double sailedAt;
        bool veilWanted;
        IShipStageLook look;
        IShipCrewRig crew;
        readonly Dictionary<Color, Material> made = new Dictionary<Color, Material>();

        // ------------------------------------------------------------------ making it

        /// <summary>Makes the stage. One at a time: a second call gives the first back.</summary>
        public static ShipStage Build(ShipStageKit kit, IShipStageSource source)
        {
            if (Current != null)
                return Current;
            var go = new GameObject("--- SHIP STAGE (the 3D view) ---");
            var stage = go.AddComponent<ShipStage>();
            stage.kit = kit;
            stage.Source = source;
            stage.Make();
            return stage;
        }

        void Make()
        {
            Current = this;
            gameObject.layer = kit.layer;
            transform.position = kit.origin;
            Sea = new StageSea();
            Water = new StageWater();
            Water.Build(transform, Lit(kit.water, new Color(0.05f, 0.14f, 0.18f), 0.86f), kit.layer);
            if (kit.sea == StageSeaKind.HdrpWater && StageHdrpSea.Supported)
            {
                Hdrp = new StageHdrpSea();
                Hdrp.Build(transform, kit.layer);
            }
            if (Hdrp != null && kit.wakeFoam != null)
            {
                FoamWake = new StageWake();
                FoamWake.Build(this, kit.wakeFoam);
            }
            Ship = new StageShip();
            Ship.Build(this, kit, transform);
            Dock = new StageDock();
            View = new ShipStageView();
            View.Build(transform, kit);
            View.Drawing = on => Hdrp?.Drawing(on);                       // (HDRP is made to work its water out only for the stage: StageHdrpSea)
            Lights();
            Atmosphere();
            Rain();
            Wake();
            bowLine = Line("Bow line");
            sternLine = Line("Stern line");
            Adopt(gameObject);
            SeaKind = kit.sea;
            KeepSeaFromOtherCameras();
        }

        /// <summary>
        /// HDRP draws every water surface for a camera whose culling mask is 0, whatever layer the surface is on
        /// (HDRenderPipeline.WaterSystem.cs, ShouldRenderSurface: the layer is only checked when the mask is not 0), and
        /// the chart's camera is such a camera. So Water is switched off in that camera's own frame settings: it was never
        /// meant to draw anything. A camera with any other mask is left alone (HDRP checks the layer for it). Called when
        /// the stage is made and once a second after (a camera may come later).
        /// </summary>
        public void KeepSeaFromOtherCameras()
        {
            if (Hdrp == null)
                return;
            foreach (var cam in Camera.allCameras)
            {
                if (cam == null || cam == View.Camera || cam.cullingMask != 0)
                    continue;
                if (!cam.TryGetComponent<HDAdditionalCameraData>(out var data))
                    data = cam.gameObject.AddComponent<HDAdditionalCameraData>();
                data.customRenderingSettings = true;
                data.renderingPathCustomFrameSettingsOverrideMask.mask[(uint)FrameSettingsField.Water] = true;
                data.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Water, false);
            }
        }

        void OnDestroy()
        {
            look?.Remove(this);
            crew?.Detach();
            Ship?.Destroy();
            Water?.Destroy();
            Hdrp?.Destroy();
            Dock?.Destroy();
            View?.Release();
            if (wakeMesh != null)
                Destroy(wakeMesh);
            foreach (var m in made.Values)
                if (m != null)
                    Destroy(m);
            if (Volume != null && Volume.profile != null)
                Destroy(Volume.profile);
            if (Current == this)
                Current = null;
        }

        /// <summary>The Sun and the Moon as the level clock makes them: real lux, drawn on the physically based sky.</summary>
        void Lights()
        {
            Sun = NewLight("Sun", out sunData);
            Sun.useColorTemperature = true;
            Sun.colorTemperature = 5778f;
            Sun.color = Color.white;
            sunData.interactsWithSky = true;
            sunData.angularDiameter = 0.53f;
            sunData.celestialBodyShadingSource = HDAdditionalLightData.CelestialBodyShadingSource.Emission;
            sunData.flareMultiplier = 0.15f;
            sunData.flareSize = 1.2f;
            sunData.flareFalloff = 6f;
            sunData.distance = 1.496e11f;
            Moon = NewLight("Moon", out moonData);
            Moon.useColorTemperature = false;
            Moon.color = new Color(0.78f, 0.85f, 1f);
            moonData.interactsWithSky = true;
            moonData.celestialBodyShadingSource = HDAdditionalLightData.CelestialBodyShadingSource.ReflectSunLight;
            moonData.sunLightOverride = Sun;
            moonData.angularDiameter = 0.52f;
            moonData.diameterMultiplerMode = false;
            moonData.diameterOverride = 0.52f;
            moonData.distance = 3.84e8f;
            moonData.flareMultiplier = 0f;
            moonData.earthshine = 1f;
        }

        Light NewLight(string name, out HDAdditionalLightData data)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var l = go.AddComponent<Light>();
            l.type = LightType.Directional;
            l.lightUnit = LightUnit.Lux;
            l.intensity = 0f;
            l.shadows = LightShadows.None;
            data = go.AddComponent<HDAdditionalLightData>();
            return l;
        }

        /// <summary>The stage's own volume, on its own layer: only the stage's camera takes it (its volumeLayerMask), and
        /// that camera takes no other but the project's defaults, which this overrides where it matters.</summary>
        void Atmosphere()
        {
            var go = new GameObject("Sky and air");
            go.transform.SetParent(transform, false);
            Volume = go.AddComponent<Volume>();
            Volume.isGlobal = true;
            Volume.priority = 100f;
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "Ship stage (runtime)";
            profile.hideFlags = HideFlags.DontSave;
            Volume.profile = profile;
            environment = profile.Add<VisualEnvironment>(false);
            environment.skyType.Override((int)SkyType.PhysicallyBased);
            environment.cloudType.Override((int)CloudType.CloudLayer);
            environment.skyAmbientMode.Override(SkyAmbientMode.Dynamic);
            environment.renderingSpace.Override(RenderingSpace.Camera);   // (the sky goes with the camera: the stage may stand anywhere)
            sky = profile.Add<PhysicallyBasedSky>(false);
            sky.groundTint.Override(new Color(0.6f, 0.64f, 0.68f));
            if (kit.stars != null)
            {
                sky.spaceEmissionTexture.Override(kit.stars);
                sky.spaceEmissionMultiplier.Override(5f);
            }
            clouds = profile.Add<CloudLayer>(false);
            clouds.layerA.distortionMode.Override(CloudDistortionMode.Procedural);   // (they move: the Visual Environment's wind)
            fog = profile.Add<Fog>(false);
            fog.enabled.Override(true);
            fog.enableVolumetricFog.Override(false);
            fog.maxFogDistance.Override(20000f);
            exposure = profile.Add<Exposure>(false);
            exposure.mode.Override(ExposureMode.Fixed);
            var vc = profile.Add<VolumetricClouds>(false);
            vc.enable.Override(false);
        }

        void Rain()
        {
            var go = new GameObject("Rain", typeof(ParticleSystem));
            go.transform.SetParent(View.Camera.transform, false);
            go.transform.localPosition = new Vector3(0f, 9f, 9f);
            go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            rain = go.GetComponent<ParticleSystem>();
            var main = rain.main;
            main.loop = true;
            // (the look lab's rain: TankBuilder.Precipitation, 13 m/s drops 2.5 cm wide, drawn stretched, with a lit material)
            main.startLifetime = 1.3f;
            main.startSpeed = 13f;
            main.startSize = 0.025f;
            main.startColor = new Color(0.7f, 0.74f, 0.78f, 0.35f);
            main.maxParticles = 1500;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;   // (its camera is never enabled: nothing would count it as seen)
            main.useUnscaledTime = true;                                   // (the rain falls at its own speed whatever the clock's, and while a card holds the clock)
            var emission = rain.emission;
            emission.rateOverTime = 0f;
            var shape = rain.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(34f, 22f, 1f);
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = kit.rain != null ? kit.rain : Unlit(null, new Color(0.7f, 0.74f, 0.78f, 0.35f));
            r.renderMode = ParticleSystemRenderMode.Stretch;
            r.velocityScale = 0.05f;
            r.lengthScale = 1f;
            r.shadowCastingMode = ShadowCastingMode.Off;
        }

        /// <summary>Her wake: a ribbon of foam astern, laid on the water (the levels' ShipWake draws into HDRP's water
        /// surface, which the stage has none of).</summary>
        void Wake()
        {
            const int n = 14;
            wakeVerts = new Vector3[n * 2];
            wakeColours = new Color32[n * 2];
            var uv = new Vector2[n * 2];
            var up = new Vector3[n * 2];
            var tris = new int[(n - 1) * 6];
            for (int i = 0; i < n; i++)
            {
                // (u across, v from her stern to the ribbon's end: the kit's wake material fades by its texture along v
                // and to both edges, since HDRP/Lit does not read a vertex's alpha)
                uv[i * 2] = new Vector2(0f, i / (float)(n - 1));
                uv[i * 2 + 1] = new Vector2(1f, i / (float)(n - 1));
                up[i * 2] = up[i * 2 + 1] = Vector3.up;
            }
            for (int i = 0, t = 0; i < n - 1; i++)
            {
                int a = i * 2;
                tris[t++] = a; tris[t++] = a + 1; tris[t++] = a + 2;
                tris[t++] = a + 1; tris[t++] = a + 3; tris[t++] = a + 2;
            }
            wakeMesh = new Mesh { name = "Ship stage wake", hideFlags = HideFlags.HideAndDontSave };
            wakeMesh.MarkDynamic();
            wakeMesh.vertices = wakeVerts;
            wakeMesh.normals = up;
            wakeMesh.uv = uv;
            wakeMesh.colors32 = wakeColours;
            wakeMesh.triangles = tris;
            wakeMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 200f);
            var go = new GameObject("Wake");
            go.transform.SetParent(transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = wakeMesh;
            wakeRenderer = go.AddComponent<MeshRenderer>();
            wakeRenderer.sharedMaterial = kit.wake != null ? kit.wake : Unlit(null, new Color(0.9f, 0.94f, 0.95f, 0.25f));
            wakeRenderer.shadowCastingMode = ShadowCastingMode.Off;
        }

        LineRenderer Line(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var l = go.AddComponent<LineRenderer>();
            l.useWorldSpace = false;
            l.positionCount = 3;
            l.widthMultiplier = 0.035f;
            l.sharedMaterial = Lit(kit.line, new Color(0.55f, 0.47f, 0.33f, 1f), 0.1f);
            l.shadowCastingMode = ShadowCastingMode.Off;
            l.enabled = false;
            return l;
        }

        // ------------------------------------------------------------------ what the builders of its parts call

        /// <summary>One of the kit's prefabs under a parent, or (the prefab missing) a plain shape of the size it would
        /// have: an empty holder with the shape inside it, so the caller may scale and turn either the same way.</summary>
        public GameObject Spawn(GameObject prefab, Transform parent, string name, PrimitiveType standIn, Vector3 standInSize)
        {
            GameObject go;
            if (prefab != null)
                go = Instantiate(prefab, parent);
            else
            {
                go = new GameObject();
                go.transform.SetParent(parent, false);
                var shape = GameObject.CreatePrimitive(standIn);
                shape.name = "Stand-in";
                shape.transform.SetParent(go.transform, false);
                shape.transform.localScale = standInSize;
                shape.GetComponent<Renderer>().sharedMaterial = Lit(null, new Color(0.42f, 0.3f, 0.2f), 0.2f);
            }
            go.name = name;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            Adopt(go);
            return go;
        }

        /// <summary>Takes an object onto the stage: its layer, no colliders (the chart's ship asks Physics what lies under
        /// her: SailingShip.GroundAt, ShipMotion.Ground), and the kit's HDRP materials for the pack's URP ones.</summary>
        public void Adopt(GameObject go)
        {
            changed = true;
            foreach (var t in go.GetComponentsInChildren<Transform>(true))
                t.gameObject.layer = kit.layer;
            foreach (var c in go.GetComponentsInChildren<Collider>(true))
                Destroy(c);
            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                bool changed = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    var to = kit.Swap(mats[i]);
                    if (to == null && mats[i] != null && (mats[i].shader == null || !mats[i].shader.isSupported || mats[i].shader.name == "Hidden/InternalErrorShader"))
                        to = Lit(null, new Color(0.45f, 0.33f, 0.22f), 0.2f);     // (pink otherwise: a pack material the kit has no swap for)
                    if (to != null)
                    {
                        mats[i] = to;
                        changed = true;
                    }
                }
                if (changed)
                    r.sharedMaterials = mats;
            }
        }

        /// <summary>Every renderer's materials through a look's own choice (null: left as it is).</summary>
        public void Remat(Func<Renderer, Material, Material> pick)
        {
            foreach (var r in GetComponentsInChildren<Renderer>(true))
            {
                if (kit.waterExclusion != null && r.sharedMaterial == kit.waterExclusion)
                    continue;                                             // (the lid in her hull: not a thing that is seen)
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++)
                    mats[i] = pick(r, mats[i]) ?? mats[i];
                r.sharedMaterials = mats;
            }
        }

        /// <summary>The kit's material, or (missing) a plain HDRP/Lit of a colour, made once.</summary>
        public Material Lit(Material fromKit, Color colour, float smoothness)
        {
            if (fromKit != null)
                return fromKit;
            if (made.TryGetValue(colour, out var m) && m != null)
                return m;
            m = new Material(Shader.Find("HDRP/Lit")) { name = "Ship stage (made)", hideFlags = HideFlags.DontSave };
            m.SetColor("_BaseColor", colour);
            m.SetFloat("_Smoothness", smoothness);
            m.SetFloat("_Metallic", 0f);
            HDMaterial.ValidateMaterial(m);
            made[colour] = m;
            return m;
        }

        Material Unlit(Material fromKit, Color colour)
        {
            if (fromKit != null)
                return fromKit;
            if (made.TryGetValue(colour, out var m) && m != null)
                return m;
            m = new Material(Shader.Find("HDRP/Unlit")) { name = "Ship stage (made, unlit)", hideFlags = HideFlags.DontSave };
            m.SetColor("_UnlitColor", colour);
            if (colour.a < 0.99f)
            {
                m.SetFloat("_SurfaceType", 1f);
                m.SetFloat("_BlendMode", 0f);
            }
            HDMaterial.ValidateMaterial(m);
            made[colour] = m;
            return m;
        }

        /// <summary>Tells a look that something on the stage is new (a crew rig that has seated its men calls this).</summary>
        public void RaiseChanged() => changed = true;

        /// <summary>The look in use (null: the plain one). Setting it takes the old one off first.</summary>
        public IShipStageLook Look
        {
            get => look;
            set
            {
                if (look == value)
                    return;
                look?.Remove(this);
                look = value;
                look?.Apply(this);
            }
        }

        /// <summary>The crew aboard (null: none).</summary>
        public IShipCrewRig Crew
        {
            get => crew;
            set
            {
                if (crew == value)
                    return;
                crew?.Detach();
                crew = value;
                crew?.Attach(this, Ship);
            }
        }

        /// <summary>The next step shows the state as if it had long been so: the sea raised, the boat settled, the jetty in
        /// its place, the camera where it wants to be (a test's picture, a loaded game).</summary>
        public void Settle()
        {
            snap = true;
            Sea.Settle();
            (crew as IShipStageSettles)?.Settle();
            (look as IShipStageSettles)?.Settle();
        }

        // ------------------------------------------------------------------ each frame

        float owed;

        /// <summary>
        /// REAL SECONDS, NEVER THE GAME'S. In the chart game the clock's speed is Unity's time scale (GameSpeed), so
        /// Time.deltaTime runs 2, 4 and more times as fast at the higher speeds and stands still while a card holds the
        /// clock. The view keeps its own calm motion through all of that (the user, 7 Oct 2026: "the view of the ship
        /// does not race at high speed: it keeps its own calm motion and lets the sky and light pass"): the sea, her
        /// riding of it, the oars and the camera go by the wall clock; only the record (the hour, the weather, where she
        /// is) comes from the game. And the stage steps only on the frames its picture is drawn on (ShipStageView.Turn).
        /// </summary>
        void LateUpdate()
        {
            if (!Running)
            {
                owed = 0f;
                return;
            }
            float real = Mathf.Min(RealSeconds, 0.1f);
            owed += real;
            if (!View.Turn(real))
                return;
            float dt = Mathf.Min(owed, 0.25f);
            owed = 0f;
            StepOn(dt);
        }

        /// <summary>The real seconds this frame took. While pictures are being taken at a fixed pace (a film, a test:
        /// Time.captureFramerate) a frame is one of those pictures, whatever the wall clock says.</summary>
        static float RealSeconds => Time.captureFramerate > 0 ? 1f / Time.captureFramerate : Time.unscaledDeltaTime;

        /// <summary>Reads the record and moves the stage on by dt real seconds, then draws the picture if its turn has come.</summary>
        public void Step(float dt)
        {
            View.Turn(dt);
            StepOn(dt);
        }

        void StepOn(float dt)
        {
            if (Source != null && !showroom && !titleSea)
                Source.Read(ref State);
            var s = showroom ? ShowroomState() : titleSea ? StormState() : State;
            Sea.Step(s, dt);
            if (seaKind == StageSeaKind.HdrpWater && Hdrp != null)
                Hdrp.Tick(s, Sea.State, kit.origin, dt, snap);
            else
                Water.Tick(Sea);
            Ship.Tick(s, Sea, dt, snap);
            FoamWake?.Tick(s, Ship, dt, seaKind == StageSeaKind.HdrpWater);
            if (Time.unscaledTime >= nextCameraLook)
            {
                nextCameraLook = Time.unscaledTime + 1f;
                KeepSeaFromOtherCameras();
            }
            SkyAndAir(s, dt);
            DockStep(s, dt);
            CameraStep(s, dt);
            WakeStep(s);
            if (showroom)
                ShowroomStep(dt);
            else if (titleSea)
                TitleSeaStep(s, dt);
            else
                crew?.Tick(s, dt);
            snap = false;
            if (changed)
            {
                changed = false;
                Changed?.Invoke();
            }
            // (a look that makes the picture itself draws it now; else the stage's own view does, when its turn has come)
            if (look is IShipStageLookDraws draws)
                draws.Draw(this, dt);
            else
                View.Render();
        }

        void SkyAndAir(in ShipStageState s, float dt)
        {
            var now = StageSky.At(s.julianDay, s.latitude, s.longitude);
            SkyNow = now;
            float through = s.cloudLight > 0f ? s.cloudLight : 1f;
            Sun.transform.localRotation = Quaternion.LookRotation(-now.toSun, Vector3.up);
            Sun.intensity = kit.sunLux * (float)LightingModel.CloudDim(through) * (1f - now.sunCovered);
            Sun.enabled = true;                                           // (all night: the sky's twilight comes from where it stands under the horizon)
            bool sunShadows = now.sunAltitude > 1f;
            sunData.EnableShadows(sunShadows);
            bool moonUp = now.moonAltitude > -1f;
            float veil = Mathf.Lerp(1f, 0.3f, Mathf.Max(s.overcast, s.cloud * 0.5f));
            Moon.transform.localRotation = Quaternion.LookRotation(-now.toMoon, Vector3.up);
            Moon.intensity = moonUp ? now.moonLux * kit.moonBoost * veil : 0f;
            Moon.enabled = true;
            moonData.EnableShadows(!sunShadows && moonUp && Moon.intensity > 0.01f);
            float night = Mathf.InverseLerp(0f, -12f, now.sunAltitude);
            moonData.surfaceTint = Color.white * Mathf.Lerp(0.15f, 0.01f, night);
            sky.horizonTint.Override(Color.Lerp(Color.white, new Color(0.62f, 0.78f, 1f), night));
            sky.zenithTint.Override(Color.Lerp(Color.white, new Color(0.85f, 0.9f, 1f), night));
            if (kit.stars != null)
            {
                // the stars, turned to the moment and her place as the levels' SkyDome turns them (precession once a game day)
                double ut = Kapman.World.Sky.ToUt(s.julianDay, s.longitude);
                if (precession == null || Math.Abs(ut - precessionDay) > 1.0)
                {
                    precession = SkyMath.Precession(SkyMath.Jde(ut));
                    precessionDay = ut;
                }
                sky.spaceRotation.Override(Kapman.Levels.SkyDome.SkyRotation(s.latitude, SkyMath.Gmst(ut) + s.longitude, precession).eulerAngles);
            }

            // the exposure: fixed at what the lighting model says the scene is, so it never hunts and a test's picture is
            // the same every run (the levels' SkyDome lets HDRP meter the picture and floors it by the eye's adaptation)
            // Two corrections to the model's number. By day it exposes for a grey card in the Sun, which leaves a brown boat
            // on dark water a stop and a half under: exposureBias, in the measure the light is the Sun's own (none in fog
            // or under a grey sky, where the card and the picture agree). By night it would bring a moonlit sea up to
            // daylight: nightDarkness stops are held back, the look lab's rule (SkyDome there: nothing by day, all of it
            // deep in the night), so night reads as night.
            double lux = LightingModel.TotalLux(now.sunAltitude, s.overcast, s.storm, now.moonLux, now.moonAltitude, now.sunCovered);
            float sceneEv = (float)LightingModel.EV100FromLux(lux);
            float sunShare = lux > 1e-9 ? Mathf.Clamp01((float)(LightingModel.SunLux(now.sunAltitude, s.overcast, s.storm) * (1.0 - now.sunCovered) / lux)) : 0f;
            sunShare *= 1f - Mathf.Clamp01(s.fog);                         // (in fog none of the light is the Sun's own)
            float under = Mathf.Clamp01((6f - sceneEv) / 10f);
            float want = Mathf.Clamp(sceneEv + kit.exposureBias * sunShare + kit.nightDarkness * under, kit.exposureFloor, kit.exposureCeiling);
            SceneEv = sceneEv;
            ev = snap ? want : Mathf.Lerp(ev, want, 1f - Mathf.Exp(-dt * 2f));
            exposure.fixedExposure.Override(ev);

            // the air: fog by how far one sees (Koschmieder: the range is 3.912 mean free paths), lying low when it is thick.
            // MIST CLOSES THE DISTANCE (the user, 7 Oct 2026: "you cant see anything other than the boat and the water and
            // just the port no land"): one never sees farther than kit.mist, so the water goes into a pale band and there
            // is no bare line of horizon to show that nothing stands on it; the mist lies low (kit.mistTop), so the sky
            // and its clouds still show over it. And THE VEIL: for the moment the jetty has to be put at her side at once
            // (DockStep), the mist closes right in and opens again.
            float sees = Mathf.Max(s.visibility, 30f);
            float top = 2500f;
            if (kit.mist > 0f)
            {
                sees = Mathf.Min(sees, kit.mist);
                top = Mathf.Max(20f, kit.mistTop);
            }
            mistVeil = snap ? 0f : Mathf.MoveTowards(mistVeil, veilWanted ? 1f : 0f, dt / (veilWanted ? 0.6f : 1.6f));
            float closed = Mathf.SmoothStep(0f, 1f, mistVeil);
            sees = Mathf.Lerp(sees, 14f, closed);
            fog.meanFreePath.Override(Mathf.Clamp(sees / 3.912f, 3f, 40000f));
            fog.baseHeight.Override(kit.origin.y + 6f);
            fog.maximumHeight.Override(kit.origin.y + Mathf.Lerp(Mathf.Lerp(top, Mathf.Min(top, 120f), Mathf.Clamp01(s.fog)), 600f, closed));

            // the cloud: HDRP's cloud layer (one flat layer drawn with the sky: cheap), more of its stratus as it greys over
            // (the layer cannot have fewer clouds, only fainter ones: so the heaped clouds of its red channel are kept well
            // seen from a third of cover up, with blue between them, and the grey sheet of its green channel comes over as
            // the sky greys; the high wisps of blue and alpha are always there a little)
            float heaped = Mathf.Clamp01(s.cloud * 1.5f) * (1f - 0.6f * s.overcast);
            clouds.opacity.Override(Mathf.Clamp01(Mathf.Max(heaped, s.overcast, 0.25f * Mathf.Clamp01(s.cloud * 4f))));
            clouds.layerA.opacityR.Override(Mathf.Clamp01(heaped));
            clouds.layerA.opacityG.Override(Mathf.Clamp01(s.overcast));
            clouds.layerA.opacityB.Override(Mathf.Lerp(0.12f, 0.3f, Mathf.Clamp01(s.cloud)));
            clouds.layerA.opacityA.Override(Mathf.Lerp(0.08f, 0.2f, Mathf.Clamp01(s.cloud)));
            // they drift with the wind, a little livelier than at the surface (the game's HdrpWeatherDraw.PushWind)
            environment.windSpeed.Override(s.windSpeed * 3.6f * 1.5f);
            environment.windOrientation.Override(Mathf.Repeat(90f - (s.windFrom + 180f), 360f));
            clouds.layerA.tint.Override(Color.Lerp(Color.white, new Color(0.5f, 0.52f, 0.56f), Mathf.Max(s.overcast, s.storm)));

            // rain, driven down the wind
            var emission = rain.emission;
            emission.rateOverTime = (s.rain + s.snow * 0.5f) * 900f;
            var vel = rain.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;
            float to = (s.windFrom + 180f) * Mathf.Deg2Rad;
            vel.x = new ParticleSystem.MinMaxCurve(Mathf.Sin(to) * s.windSpeed * 0.35f);
            vel.y = new ParticleSystem.MinMaxCurve(0f);
            vel.z = new ParticleSystem.MinMaxCurve(Mathf.Cos(to) * s.windSpeed * 0.35f);
            if (emission.rateOverTime.constant > 1f && !rain.isPlaying)
                rain.Play();
        }

        /// <summary>
        /// The jetty comes and goes without a jump. It stands in the world: while it is on the stage it goes astern with
        /// the water at her speed. Coming in (berthing) it is put ahead of her by the chart's own distance to the berth
        /// (one game metre of the chart is one metre here, and both close at her speed, so jetty and water move as one)
        /// and a little wide, closing to her side as she comes up; once she is made fast it is eased the last of the way
        /// alongside (she stops up to five metres short on the chart: here she is warped in). Leaving, it is simply left
        /// astern and taken away once it is lost in the mist.
        /// IT NEVER FLIES OVER THE WATER. The chart's distance closes at her speed times the clock's, and the stage's
        /// water goes by at her own speed whatever the clock's (ShipStageState.Way). So the jetty is never moved over the
        /// water faster than she herself goes; and when the chart has her made fast while the jetty is still more than a
        /// length and a half off (the clock ran fast, or she was put there), the mist closes for a moment (the veil,
        /// SkyAndAir) and the jetty is put at her side while nothing can be seen.
        /// </summary>
        void DockStep(in ShipStageState s, float dt)
        {
            float a = s.heading * Mathf.Deg2Rad;
            Vector3 fwd = new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)), right = new Vector3(fwd.z, 0f, -fwd.x);
            Vector3 way = fwd * s.Way;
            // how far off a jetty can be made out at all: the mist's reach. The jetty she is bound for is put on the
            // stage as soon as the chart has her that near (ShipStageState.berthAhead), so it comes up out of the mist
            // ahead and is never seen to appear; `berthing` is the port's own reach, much nearer, where the crew make ready.
            float reach = kit.mist > 0f ? Mathf.Max(260f, kit.mist) : 260f;
            float ahead = s.berthing ? s.berthDistance : s.berthAhead;
            bool coming = !s.moored && (s.berthing || (ahead > 0f && ahead < reach));
            bool want = s.moored || coming;
            bool here = DockUp && dockFor == s.portId;
            veilWanted = false;
            PushingOff = false;
            // (the chart's own reckoning of how far she has come from the jetty that is on the stage, by the game's hours:
            // a game minute is a second of her speed on the chart, whatever the clock's speed)
            double gone = s.hours - sailedAt;
            sailedAt = s.hours;
            if (here && want)
                sailedSince = 0f;
            else if (gone > 0.0 && gone < 48.0)
                sailedSince += s.speed * (float)(gone * 60.0);
            if (DockUp && !snap && (!want || (!here && !s.moored)))
            {
                // LEAVING the jetty that is on the stage (bound nowhere near, or for another port's).
                // SHE PUSHES OFF BEFORE SHE TURNS. On the chart she takes up her course at once, at any angle, and on the
                // stage she is held at its middle while everything else turns about her: a turn towards the pier would
                // swing her bow through it. So until her whole length is clear of the boards the pier is kept as it lay
                // to her (it turns with her and only slides aft and away at a walk); once clear, it stands in the world
                // and she turns from it. With no land in sight there is nothing else to tell her turning by.
                float yaw = dockYaw * Mathf.Deg2Rad;
                Vector3 along = new Vector3(Mathf.Sin(yaw), 0f, Mathf.Cos(yaw)), toWater = new Vector3(along.z, 0f, -along.x);
                float off = -Vector3.Dot(dockAt, toWater), abeam = Mathf.Abs(Vector3.Dot(dockAt, along));
                float clear = Ship.Length * 0.5f + 0.8f;
                if (off < clear && off > -Dock.Width && abeam < Dock.length * 0.5f + clear)
                {
                    float turned = Mathf.DeltaAngle(dockYaw, s.heading);
                    dockAt = Quaternion.Euler(0f, turned, 0f) * dockAt;
                    dockYaw = s.heading;
                    dockAt -= right * Mathf.Min(1f * dt, clear - off);
                    PushingOff = true;
                }
                dockAt -= way * dt;
                // IT GOES when the mist has it, or as soon as it is well off and out of the picture. And when it MUST go
                // while it can still be seen (the clock ran fast: the chart has her far from that berth, or already near
                // the next, while on the stage it lies a few lengths astern), the mist closes for a moment and takes it.
                bool mustGo = sailedSince > reach || (coming && ahead < 250f);
                if (dockAt.magnitude > reach || (dockAt.magnitude > 60f && DockUnseen()) || (mustGo && mistVeil >= 0.98f))
                {
                    Dock.Destroy();
                    DockUp = false;
                    dockFor = null;
                }
                else if (mustGo)
                    veilWanted = true;
            }
            else if (want)
            {
                Vector3 target = -right * (Ship.Beam * 0.5f + Dock.gap);
                if (!s.moored)
                    target += fwd * ahead - right * (6f * Mathf.Clamp01(ahead / 40f));
                if (here)
                    dockAt -= way * dt;
                bool farOff = s.moored && (!here || (dockAt - target).magnitude > Ship.Length * 1.5f);
                if (snap || (farOff && mistVeil >= 0.98f) || (!here && !s.moored))
                {
                    // put in its place at once: settled, behind the veil, or ahead of her as she comes into the port's reach
                    if (!here)
                        Dock.Build(this, kit, transform, s.portId);
                    dockFor = s.portId;
                    dockAt = target;
                    dockYaw = s.heading;
                    DockUp = true;
                }
                else if (farOff)
                    veilWanted = true;
                else
                {
                    float k = 1f - Mathf.Exp(-dt * (s.moored ? 1.2f : 1.5f));
                    Vector3 step = (target - dockAt) * k;
                    float most = Mathf.Max(s.speed + 0.5f, s.moored ? 1.2f : 0f) * dt;   // (over the water no faster than she goes herself; made fast, she is warped in at a walk)
                    if (step.magnitude > most)
                        step *= most / step.magnitude;
                    dockAt += step;
                    dockYaw = Mathf.LerpAngle(dockYaw, s.heading, k);
                }
            }
            else if (DockUp)
            {
                // (settled, with no jetty wanted: the state is shown as if long so, with none left astern)
                Dock.Destroy();
                DockUp = false;
                dockFor = null;
            }
            bool lines = false;
            if (DockUp && Dock.Root != null)
            {
                Dock.Root.localPosition = new Vector3(dockAt.x, 0f, dockAt.z);
                Dock.Root.localRotation = Quaternion.Euler(0f, dockYaw, 0f);
                Vector3 berth = -right * (Ship.Beam * 0.5f + Dock.gap);
                lines = s.moored && (dockAt - berth).magnitude < 1.5f;
                if (lines)
                {
                    Rope(bowLine, Ship.Root.localPosition + Ship.Root.localRotation * Ship.BowCleat, Dock.Root.localPosition + Dock.Root.localRotation * Dock.BowPost);
                    Rope(sternLine, Ship.Root.localPosition + Ship.Root.localRotation * Ship.SternCleat, Dock.Root.localPosition + Dock.Root.localRotation * Dock.SternPost);
                }
            }
            bowLine.enabled = sternLine.enabled = lines;
        }

        /// <summary>No part of the jetty lies in the camera's picture now (its middle, its ends and the points between, taken
        /// level, against the picture's width with some room to spare): it can be taken away and nobody sees it go.</summary>
        bool DockUnseen()
        {
            var cam = View.Camera.transform;
            Vector3 eye = cam.localPosition, look = cam.localRotation * Vector3.forward;
            eye.y = 0f;
            look.y = 0f;
            if (look.sqrMagnitude < 1e-6f)
                return false;
            look.Normalize();
            float yaw = dockYaw * Mathf.Deg2Rad;
            Vector3 along = new Vector3(Mathf.Sin(yaw), 0f, Mathf.Cos(yaw));
            float halfWidth = Mathf.Atan(Mathf.Tan(View.Camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * Mathf.Max(1f, View.Camera.aspect)) * Mathf.Rad2Deg;
            float cone = Mathf.Cos(Mathf.Min(89f, halfWidth + 12f) * Mathf.Deg2Rad);
            for (int i = -2; i <= 2; i++)
            {
                Vector3 to = dockAt + along * (Dock.length * 0.25f * i) - eye;
                to.y = 0f;
                if (to.sqrMagnitude < 1f || Vector3.Dot(to.normalized, look) > cone)
                    return false;
            }
            return true;
        }

        static void Rope(LineRenderer line, Vector3 from, Vector3 to)
        {
            line.SetPosition(0, from);
            line.SetPosition(1, (from + to) * 0.5f + Vector3.down * 0.12f);
            line.SetPosition(2, to);
        }

        /// <summary>The camera stands off her port quarter and turns with her, slowly; with the jetty alongside (to port)
        /// it goes round to her starboard side, so the jetty and the bank are behind her. It keeps its height over the
        /// still water (she is seen to ride the seas), but never under a crest.</summary>
        void CameraStep(in ShipStageState s, float dt)
        {
            float L = Ship.Length;
            bool dockSide = DockUp && dockAt.magnitude < 45f;
            int ang = Angle;
            // Moored at the jetty she is seen FROM THE LAND'S SIDE, looking out past her over the water (the user, 7 Oct
            // 2026, late: "when it's ported make it so it's looking away from the port, otherwise it gives off the
            // illusion": the stage's one bare jetty is not the port that the port's own picture shows). The jetty lies
            // on her left hand: the camera stands over that side at every angle but the one from overhead.
            float wantAz = dockSide ? MooredBearing[ang] : AngleBearing[ang];
            if (snap)
            {
                camAzimuth = wantAz;
                camHeading = s.heading;
            }
            else
            {
                camAzimuth = Mathf.LerpAngle(camAzimuth, wantAz + 9f * Mathf.Sin(Time.unscaledTime * 0.11f), 1f - Mathf.Exp(-dt * 0.5f));
                camHeading = Mathf.LerpAngle(camHeading, s.heading, 1f - Mathf.Exp(-dt * 0.8f));
            }
            float off = L * AngleOff[ang] * (Big ? 1.75f / 1.5f : 1f);
            Vector3 eye = Quaternion.Euler(0f, camHeading + camAzimuth, 0f) * Vector3.forward * off;
            // (a third of a length up and aimed at the height of her yard's foot: the horizon then lies a third of the way down
            // the picture and there is sky to see: the first pictures, from 0.42 up and aimed lower, had a fifth of sky and
            // only its palest part)
            eye.y = Mathf.Max(L * (AngleUp[ang] > 0f ? AngleUp[ang] : kit.eyeHeight), Height(eye.x, eye.z) + 0.7f);
            Vector3 aim = new Vector3(0f, L * kit.aimHeight + Ship.Root.localPosition.y * 0.5f, 0f);
            var cam = View.Camera.transform;
            cam.localPosition = eye;
            cam.localRotation = Quaternion.LookRotation(aim - eye, Vector3.up);
            View.Camera.fieldOfView = Big ? 40f : 34f;
        }

        void WakeStep(in ShipStageState s)
        {
            int n = wakeVerts.Length / 2;
            float a = s.heading * Mathf.Deg2Rad;
            Vector3 fwd = new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a)), right = new Vector3(fwd.z, 0f, -fwd.x);
            float strength = Mathf.Clamp01(s.speed / 3f) * (s.moored ? 0f : 1f);
            // (only on the mesh sea for now: on HDRP's water the ribbon reads as a grey smear; her wake there is to be foam
            // put into the water itself)
            wakeRenderer.enabled = strength > 0.02f && seaKind == StageSeaKind.Mesh && !WakeRibbonOff;
            if (!wakeRenderer.enabled)
                return;
            float half = Ship.Length * 0.5f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)(n - 1);
                float back = half * 0.86f + i * (0.45f + 0.4f * s.speed);
                // (a streak of disturbed water that opens a little astern and thins out: the material's texture fades it
                // along and to its edges; a look's shader may read the vertex alpha below instead)
                float wide = Ship.Beam * (0.3f + 0.45f * t);
                byte alpha = (byte)(255f * strength * (1f - t) * 0.55f);
                for (int side = 0; side < 2; side++)
                {
                    Vector3 p = -fwd * back + right * (side == 0 ? -wide : wide);
                    p.y = Height(p.x, p.z) + 0.03f;
                    wakeVerts[i * 2 + side] = p;
                    wakeColours[i * 2 + side] = new Color32(255, 255, 255, alpha);
                }
            }
            wakeMesh.vertices = wakeVerts;
            wakeMesh.colors32 = wakeColours;
            // how strong it is with her speed: the material's own colour, its alpha scaled (on this renderer only)
            wakeBlock ??= new MaterialPropertyBlock();
            var tint = wakeRenderer.sharedMaterial != null && wakeRenderer.sharedMaterial.HasProperty(BaseColorId)
                ? wakeRenderer.sharedMaterial.GetColor(BaseColorId) : Color.white;
            tint.a *= strength;
            wakeBlock.SetColor(BaseColorId, tint);
            wakeRenderer.SetPropertyBlock(wakeBlock);
        }

        MaterialPropertyBlock wakeBlock;
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    }
}
