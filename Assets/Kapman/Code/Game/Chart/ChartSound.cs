using Kapman.Audio;
using Kapman.Levels;
using Kapman.Weather;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// THE CHART'S SOUND (7 Oct 2026; Design/sound_audit.md, Design/fixes/chart_sound.md). The chart has no level: no
    /// level weather, no shore map, no hull on the water. So the modelled soundscape the 3D levels have
    /// (LevelSoundscape2, Audio/soundscape.json) is fed from the game's own state each frame: the weather where she is
    /// (the world's model, as her sail has it), the hour and the day, the ground by where she is (in port, on a river,
    /// off a coast, on the open sea), her speed and whether she is rowed. It sits beside the soundscape on the listener.
    ///
    /// It is the room, not the show: a game of reading. The level is low (a third of the 3D levels'), lower again while
    /// a card, a talk or the menu is up, the birds call half as often, and thunder never cracks close by. One setting
    /// turns it down or off (the pause menu and the title: Sound). In a headless run it makes no noise at all.
    /// Recordings only, all CC0 (Audio/CREDITS_sounds.md): nothing here is made or generated.
    /// </summary>
    public class ChartSound : MonoBehaviour
    {
        /// <summary>Where the ear is.</summary>
        public enum Ground { Port, River, Coast, Sea }

        [Tooltip("The soundscape's level on the chart, at the setting 'middling' (the 3D levels use 0.7).")]
        [Range(0f, 1f)] public float level = 0.35f;
        [Tooltip("The share of that level left while a card, a talk or the menu is up.")]
        [Range(0f, 1f)] public float readingShare = 0.5f;
        [Tooltip("Seconds to blend from one ground to the next (a port's lapping does not snap off at the cast off).")]
        public float groundBlend = 20f;
        [Tooltip("How often the birds call, times their pace in the 3D levels.")]
        [Range(0.1f, 1f)] public float callRate = 0.5f;
        [Tooltip("Thunder: the mean wait between flashes under a full storm sky, real seconds.")]
        public float flashEvery = 40f;
        [Tooltip("Game metres from a port within which a ship lying to with no course is off a coast, not on the open sea.")]
        public float coastWithin = 170f;

        public static ChartSound Current { get; private set; }
        /// <summary>The ground the ear is on now.</summary>
        public static Ground Now { get; private set; } = Ground.Port;
        /// <summary>The level the soundscape is given now, 0 to 1 (what it would be on a machine with a screen: a headless
        /// run plays nothing, and this still says what was asked).</summary>
        public static float Asked { get; private set; }
        /// <summary>The context last fed (for tests and tools).</summary>
        public static SoundContext Last { get; private set; }

        // ------------------------------------------------------------------ the setting

        const string Pref = "kapman.sound";
        static readonly string[] Words = { "off", "low", "middling", "loud" };
        static readonly float[] Factors = { 0f, 0.5f, 1f, 1.7f };
        static int setting = -1;

        /// <summary>0 off, 1 low, 2 middling (as made), 3 loud. Kept between runs.</summary>
        public static int Setting
        {
            get
            {
                if (setting < 0)
                    setting = Mathf.Clamp(PlayerPrefs.GetInt(Pref, 2), 0, Words.Length - 1);
                return setting;
            }
            set
            {
                setting = Mathf.Clamp(value, 0, Words.Length - 1);
                PlayerPrefs.SetInt(Pref, setting);
            }
        }

        /// <summary>How much louder or quieter than made the setting is (0 off, 1 as made).</summary>
        public static float SettingFactor => Factors[Setting];
        /// <summary>The setting in a word: "off", "low", "middling", "loud".</summary>
        public static string SettingWord => Words[Setting];
        /// <summary>The next setting, round again after the last (the menu's Sound line).</summary>
        public static void NextSetting() => Setting = (Setting + 1) % Words.Length;

        // ------------------------------------------------------------------ the grounds

        // shore (m, + over water), water, reed, hedge, wood, fields, town, basin: what lies round the ear, set outright
        // (the levels' guess from the shore distance is Dorestad's bank and puts a town 250 m inland everywhere)
        static readonly float[][] Grounds =
        {
            new[] {    5f, 0.5f, 0.10f, 0.05f, 0.05f, 0.05f, 0.45f, 0f    },   // Port: made fast at the bank, the place behind her
            new[] {   40f, 0.6f, 0.30f, 0.10f, 0.15f, 0.20f, 0f,    0.05f },   // River: mid-stream, reeds and trees on the banks
            new[] {  400f, 0.9f, 0.05f, 0f,    0f,    0.05f, 0f,    0.05f },   // Coast: a few cables off the land
            new[] { 5000f, 1f,   0f,    0f,    0f,    0f,    0f,    0f    },   // Sea: no land in the ear
        };

        // never heard from a deck: an inland field's dry grass, a lark over ploughland, geese grazing a meadow
        static readonly string[] Never = { "dry_grass", "skylark", "geese_meadow" };
        // the voices that belong off a coast, and on the open sea (the rest are the land's)
        static readonly string[] CoastVoices = { "gulls", "curlew", "geese_skein", "hooded_crow", "crows", "mallard" };
        static readonly string[] SeaVoices = { "gulls", "geese_skein" };
        // the beds that are the land's own: the bank's lapping, wind in reeds and trees, rain on a roof
        static readonly string[] LandBeds = { "lapping", "reeds", "reeds_shore", "leaves", "bare_trees", "rain_thatch" };
        static readonly string[] Voices =
        {
            "nightingale", "blackbird", "skylark", "swallows", "gulls", "curlew", "heron", "mallard", "rooks", "crows", "hooded_crow",
            "geese_skein", "geese_meadow", "tawny_owl",
        };

        /// <summary>Is a layer of the soundscape silent on a ground? The land's birds do not sing on the open sea (several
        /// ask nothing of the place in the data and would), and three sounds are never heard from a deck.</summary>
        public static bool ShutAt(Ground ground, string layer)
        {
            if (System.Array.IndexOf(Never, layer) >= 0)
                return true;
            if (ground == Ground.Port || ground == Ground.River)
                return false;
            if (System.Array.IndexOf(LandBeds, layer) >= 0)
                return true;
            if (System.Array.IndexOf(Voices, layer) < 0)
                return false;                                             // (the wind, the rain, the ship: everywhere)
            return System.Array.IndexOf(ground == Ground.Coast ? CoastVoices : SeaVoices, layer) < 0;
        }

        /// <summary>The leaves by the day of the year, as the levels' weather has them: bare to day 100, full by 135 and to
        /// 285, bare again by 320.</summary>
        public static float Leaf(float day) =>
            Mathf.Clamp01(Mathf.InverseLerp(100f, 135f, day)) * (1f - Mathf.Clamp01(Mathf.InverseLerp(285f, 320f, day)));

        /// <summary>The kind of weather by its name, as the levels' weather names it (it sets the air's damp).</summary>
        public static string KindOf(WeatherSample s)
        {
            float vis = s.VisibilityKm;
            if (s.storm > 0.5f && s.windSpeed > 14f) return "Storm";
            if (s.Snow) return "Snow";
            if (s.rain > 0.1f) return "Rain";
            if (vis < 1f) return "Fog";
            if (vis < 4f) return "Mist";
            if (s.Cover > 0.8f) return "Overcast";
            return "Fair";
        }

        /// <summary>
        /// What the ear is in, from plain facts: the ground's shares (`g`: shore, water, reed, hedge, wood, fields, town,
        /// basin), the weather where she is, the hour and the day, the Sun's height, and her speed made good (real m/s; 0
        /// lying still or with the clock held). No hull rides a chart, so her roll and pitch are the wind's, by the
        /// ground: none made fast, little on a river, more off a coast, most at sea. She is rowed on a river and in a
        /// calm, and sails otherwise (until the ship has a state of her own to ask).
        /// </summary>
        public static SoundContext For(Ground ground, float[] g, WeatherSample s, float hour, float day, float sun, float speed)
        {
            float force = SoundscapeModel.Force(s.windSpeed);
            bool inland = ground == Ground.River || ground == Ground.Port;
            float roll = ground == Ground.Port ? 0f : (ground == Ground.River ? 0.15f : ground == Ground.Coast ? 0.35f : 0.5f) * force;
            bool rowed = speed > 0.05f && (inland || s.windSpeed < 1.6f);
            return new SoundContext
            {
                wind = s.windSpeed,
                rain = Mathf.Clamp01(s.rain * (1f - s.SnowShare)),
                storm = s.storm,
                temperature = s.temperature,
                humidity = SoundscapeModel.HumidityFromWeather(KindOf(s), hour, sun),
                cloud = Mathf.Clamp01(s.Cover),
                hour = hour,
                day = day,
                sunAltitude = sun,
                night = Mathf.Clamp01(sun / -12f),
                leaf = Leaf(day),
                aboveWater = 2f,                                          // (the ear is on deck, not at the chart's eye)
                shore = g[0], water = g[1], reed = g[2], hedge = g[3], wood = g[4], fields = g[5], town = g[6], basin = g[7],
                shipSpeed = speed,
                shipRowing = rowed ? Mathf.InverseLerp(0.05f, 0.6f, speed) : 0f,
                shipRollRate = roll,
                shipPitchRate = roll * 0.6f,
                shipDistance = 0f,
            };
        }

        /// <summary>The ground of a kind of lane: a river (and a carry between rivers) is the river; the open sea is the
        /// sea; coasts, sounds, flats and a port's approach are the coast.</summary>
        public static Ground GroundOf(byte laneKind) =>
            laneKind == LaneGraph.River || laneKind == LaneGraph.Pending || laneKind == LaneGraph.Portage ? Ground.River
            : laneKind == LaneGraph.Sea ? Ground.Sea : Ground.Coast;

        /// <summary>The shares of a ground, as they are set (a copy).</summary>
        public static float[] SharesOf(Ground ground) => (float[])Grounds[(int)ground].Clone();

        // ------------------------------------------------------------------ each frame

        readonly float[] g = new float[8];
        bool started;
        float nextFlash, given;

        void OnEnable()
        {
            Current = this;
            LevelSoundscape2.Feed = Fill;
            LevelSoundscape2.Shut = id => ShutAt(Now, id);
            LevelSoundscape2.CallRate = callRate;
        }

        void OnDisable()
        {
            if (Current != this)
                return;
            Current = null;
            LevelSoundscape2.Feed = null;
            LevelSoundscape2.Shut = null;
            LevelSoundscape2.CallRate = 1f;
        }

        /// <summary>How loud: by the setting, and lower while something is being read. Eased, never snapped.</summary>
        void Update()
        {
            var s = LevelSoundscape2.Current;
            if (s == null)
                return;
            LevelSoundscape2.CallRate = callRate;
            var frame = Frame.FrameRoot.Current;
            var menu = MainMenu.Current;
            bool reading = (frame != null && frame.FloatCount > 0) || (menu != null && menu.Open);
            float to = level * Factors[Setting] * (reading ? readingShare : 1f);
            given = Mathf.MoveTowards(given, to, Time.unscaledDeltaTime * (to < given ? 0.25f : 0.12f));
            Asked = Mathf.Clamp01(given);
            s.master = Application.isBatchMode ? 0f : Asked;              // (a headless or test run makes no noise on the machine)
        }

        /// <summary>The ground she is on now: made fast in a port; else the water of the leg she sails (or, lying still,
        /// the water round her).</summary>
        Ground GroundNow(KapmanGame game)
        {
            var port = Port.Nearest(game.ShipPosition);
            bool atPort = port != null && port.DistanceTo(game.ShipPosition) < port.arrivalRadius + 5f;
            if (game.ship.anchored && atPort)
                return Ground.Port;
            // under way: the water of the leg she sails (a course has a kind for each point, the leg that ends there;
            // its first and its last are marked as sea whatever they are, so the legs next to them stand for them)
            if (Course.IsSet && Course.Kinds.Count >= 3)
                return GroundOf(Course.Kinds[Mathf.Clamp(Course.Next, 1, Course.Kinds.Count - 2)]);
            if (ChartWorld.OnRiver())
                return Ground.River;
            return port != null && port.DistanceTo(game.ShipPosition) < coastWithin ? Ground.Coast : Ground.Sea;
        }

        SoundContext Fill()
        {
            var game = KapmanGame.Current;
            if (game == null || game.ship == null || !ChartWorld.TryWeather(out WeatherSample s))
                return started ? Last : Quiet();
            var clock = LevelClock.Current;
            float hour = clock != null ? clock.HourOfDay : 12f;
            float day = clock != null ? clock.DayOfYear : 121f;
            var at = ChartWorld.ShipWorld();
            var (lat, _) = WorldFrame.ToLatLon(at.x, at.z);
            float sun = (float)Kapman.Nature.Presence.ApproxSunAltitude(lat, day, hour);

            // the ground, eased from the last one
            Now = GroundNow(game);
            var to = Grounds[(int)Now];
            float k = started ? 1f - Mathf.Exp(-Time.unscaledDeltaTime / Mathf.Max(groundBlend / 3f, 0.1f)) : 1f;
            for (int i = 0; i < g.Length; i++)
                g[i] = Mathf.Lerp(g[i], to[i], k);
            started = true;

            // her way through the water: none made fast, none while the clock stands
            var body = ChartBody.Current;
            bool still = game.ship.anchored || GameSpeed.Paused || body == null;
            float speed = still ? 0f : body.Knots * 0.5144f;

            var c = For(Now, g, s, hour, day, sun, speed);
            // the wind the far calls bend in: where it blows to (windFrom is the compass point it blows from)
            float rad = (s.windFrom + 180f) * Mathf.Deg2Rad;
            LevelSoundscape2.FeedWind = new Vector2(Mathf.Sin(rad), Mathf.Cos(rad)) * s.windSpeed;
            Thunder(s.storm);
            Last = c;
            return c;
        }

        /// <summary>Before the world is made: a still morning on the river, so nothing blares.</summary>
        static SoundContext Quiet() => new SoundContext
        {
            wind = 2f, temperature = 12f, humidity = 0.8f, hour = 12f, day = 121f, sunAltitude = 45f, leaf = 0.6f,
            aboveWater = 2f, shore = 40f, water = 0.6f, shipDistance = 0f,
        };

        /// <summary>Flashes under a storm sky: the more of it the oftener, two to fourteen kilometres off (never close:
        /// a crack does not land in the middle of a sentence).</summary>
        void Thunder(float storm)
        {
            if (storm < 0.4f || LevelSoundscape2.Current == null || GameSpeed.Paused)
            {
                nextFlash = 0f;
                return;
            }
            if (nextFlash <= 0f)
                nextFlash = Time.unscaledTime + flashEvery / storm * Random.Range(0.4f, 1.6f);
            if (Time.unscaledTime < nextFlash)
                return;
            nextFlash = 0f;
            LevelSoundscape2.Current.Strike(Random.Range(2000f, 14000f));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Current = null;
            Now = Ground.Port;
            Asked = 0f;
            Last = default;
            setting = -1;
        }
    }
}
