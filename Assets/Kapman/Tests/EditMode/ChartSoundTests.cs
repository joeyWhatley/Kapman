using System.IO;
using System.Linq;
using Kapman.Audio;
using Kapman.Game;
using Kapman.Weather;
using NUnit.Framework;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>
    /// THE CHART'S SOUND (7 Oct 2026): the soundscape of the 3D levels fed by where she is, the weather and the hour
    /// (Game/Chart/ChartSound.cs). No sound is played here: these hold what each layer's level would be on each ground,
    /// which voices are shut where, the setting's steps, and that every recording the soundscape names is in the
    /// project with its credit line (recordings only, CC0: never made or generated).
    /// </summary>
    public class ChartSoundTests
    {
        const string Data = "Assets/Kapman/Audio/soundscape.json";
        const string Sounds = "Assets/Kapman/Audio/Sounds/";
        const string Credits = "Assets/Kapman/Audio/CREDITS_sounds.md";
        SoundscapeDef def;

        [SetUp]
        public void Load() => def = SoundscapeDef.Parse(File.ReadAllText(Data));

        SoundLayerDef Layer(string id)
        {
            var l = def.layers.FirstOrDefault(x => x.id == id);
            Assert.IsNotNull(l, "soundscape.json has no layer " + id);
            return l;
        }

        static WeatherSample Weather(float wind, float rain = 0f, float storm = 0f) =>
            new WeatherSample { windSpeed = wind, windFrom = 240f, rain = rain, storm = storm, temperature = 12f };

        /// <summary>A layer's level (dB; -80 silent) on a ground, as the chart would have it.</summary>
        float Db(ChartSound.Ground ground, string layer, WeatherSample s, float speed, float hour = 12f, float day = 150f)
        {
            if (ChartSound.ShutAt(ground, layer))
                return -80f;
            float sun = (float)Kapman.Nature.Presence.ApproxSunAltitude(52.0, day, hour);
            var c = ChartSound.For(ground, ChartSound.SharesOf(ground), s, hour, day, sun, speed);
            return SoundscapeModel.LayerDb(Layer(layer), c);
        }

        [Test]
        public void EveryRecordingIsInTheProjectWithItsCredit()
        {
            string credits = File.ReadAllText(Credits);
            var ids = def.ClipIds().Distinct().ToList();
            Assert.Greater(ids.Count, 30, "the soundscape names its recordings");
            foreach (string id in ids)
            {
                Assert.IsTrue(File.Exists(Sounds + id + ".mp3"), "soundscape.json names a recording that is not in the project: " + id);
                string line = credits.Split('\n').FirstOrDefault(l => l.Contains("(" + id + ".mp3"));
                Assert.IsNotNull(line, "no credit line in CREDITS_sounds.md for " + id + ".mp3");
                StringAssert.Contains("CC0", line, id + ": its credit line does not say CC0");
                StringAssert.Contains("freesound.org", line, id + ": its credit line has no source");
            }
        }

        [Test]
        public void OnTheOpenSeaTheLandIsSilentAndTheShipIsHeard()
        {
            var blow = Weather(9f);                                       // a fresh breeze, force 5
            foreach (string land in new[] { "lapping", "reeds", "reeds_shore", "leaves", "bare_trees", "rain_thatch", "dry_grass" })
                Assert.LessOrEqual(Db(ChartSound.Ground.Sea, land, blow, 3f), -79f, land + " is heard on the open sea");
            Assert.Greater(Db(ChartSound.Ground.Sea, "wind", blow, 3f), -40f, "the wind at force 5");
            Assert.Greater(Db(ChartSound.Ground.Sea, "ship_bow", blow, 3f), -40f, "the bow wave at 3 m/s");
            Assert.Greater(Db(ChartSound.Ground.Sea, "ship_creak", blow, 3f), -60f, "her timbers working in a seaway");
            Assert.LessOrEqual(Db(ChartSound.Ground.Sea, "ship_oars", blow, 3f), -79f, "she sails in a breeze: no oars");
            Assert.Greater(Db(ChartSound.Ground.Sea, "rain_water", Weather(9f, 0.6f), 3f), -60f, "rain on the sea");
            // in a calm she is rowed, at sea too
            Assert.Greater(Db(ChartSound.Ground.Sea, "ship_oars", Weather(1f), 1.2f), -60f, "rowed in a calm");
        }

        [Test]
        public void InPortSheLiesStillAndTheBankIsHeard()
        {
            var wet = Weather(3f, 0.5f);
            Assert.LessOrEqual(Db(ChartSound.Ground.Port, "ship_oars", wet, 0f), -79f, "no oars made fast");
            Assert.LessOrEqual(Db(ChartSound.Ground.Port, "ship_wake", wet, 0f), -79f, "no wake made fast");
            Assert.Greater(Db(ChartSound.Ground.Port, "lapping", wet, 0f), -40f, "the river lapping at the bank");
            Assert.Greater(Db(ChartSound.Ground.Port, "rain", wet, 0f), -40f, "the rain");
            Assert.Greater(Db(ChartSound.Ground.Port, "rain_thatch", wet, 0f), -60f, "rain on the roofs of the place");
            Assert.LessOrEqual(Db(ChartSound.Ground.Port, "rain", Weather(3f), 0f), -79f, "no rain, no rain heard");
        }

        [Test]
        public void OnTheRiverSheIsRowedPastTheReeds()
        {
            var air = Weather(4f);
            Assert.Greater(Db(ChartSound.Ground.River, "ship_oars", air, 1.2f), -60f, "the oars");
            Assert.Greater(Db(ChartSound.Ground.River, "ship_wake", air, 1.2f), -60f, "her wake");
            Assert.Greater(Db(ChartSound.Ground.River, "reeds", air, 1.2f), -60f, "wind in the reeds");
            Assert.LessOrEqual(Db(ChartSound.Ground.River, "ship_oars", air, 0f), -79f, "lying still: no oars");
            // the trees: in leaf in June, bare in January
            Assert.Greater(Db(ChartSound.Ground.River, "leaves", air, 0f, 12f, 170f), -60f, "leaves in June");
            Assert.LessOrEqual(Db(ChartSound.Ground.River, "leaves", air, 0f, 12f, 20f), -79f, "no leaves in January");
            Assert.Greater(Db(ChartSound.Ground.River, "bare_trees", air, 0f, 12f, 20f), -60f, "bare trees in January");
        }

        [Test]
        public void TheLandsBirdsDoNotSingAtSea()
        {
            var voices = def.OfKind("voice").Select(l => l.id).ToList();
            Assert.Greater(voices.Count, 10);
            foreach (string id in voices)
            {
                bool ofTheSea = id == "gulls" || id == "geese_skein";
                Assert.AreEqual(!ofTheSea, ChartSound.ShutAt(ChartSound.Ground.Sea, id),
                                id + (ofTheSea ? " should be heard at sea" : " is a voice of the land: shut on the open sea (a new voice in soundscape.json must be named in ChartSound)"));
            }
            foreach (string never in new[] { "dry_grass", "skylark", "geese_meadow" })
                foreach (ChartSound.Ground g in System.Enum.GetValues(typeof(ChartSound.Ground)))
                    Assert.IsTrue(ChartSound.ShutAt(g, never), never + " is never heard from a deck");
            Assert.IsFalse(ChartSound.ShutAt(ChartSound.Ground.Coast, "curlew"));
            Assert.IsTrue(ChartSound.ShutAt(ChartSound.Ground.Coast, "nightingale"));
            Assert.IsFalse(ChartSound.ShutAt(ChartSound.Ground.River, "nightingale"));
            Assert.IsFalse(ChartSound.ShutAt(ChartSound.Ground.Port, "swallows"));
            foreach (string everywhere in new[] { "wind", "rain", "rain_water", "ship_bow", "ship_creak", "thunder" })
                Assert.IsFalse(ChartSound.ShutAt(ChartSound.Ground.Sea, everywhere), everywhere + " is heard everywhere");
            // the groups keep no chorus going where every one of their voices is shut
            foreach (var group in def.OfKind("group"))
                CollectionAssert.IsSubsetOf(group.members, voices, group.id + " names a voice the soundscape does not have");
        }

        [Test]
        public void TheGroundsTheLeavesAndTheKindsOfWeather()
        {
            Assert.AreEqual(ChartSound.Ground.River, ChartSound.GroundOf(Kapman.World.LaneGraph.River));
            Assert.AreEqual(ChartSound.Ground.River, ChartSound.GroundOf(Kapman.World.LaneGraph.Pending));
            Assert.AreEqual(ChartSound.Ground.Sea, ChartSound.GroundOf(Kapman.World.LaneGraph.Sea));
            Assert.AreEqual(ChartSound.Ground.Coast, ChartSound.GroundOf(Kapman.World.LaneGraph.Coastal));
            Assert.AreEqual(ChartSound.Ground.Coast, ChartSound.GroundOf(Kapman.World.LaneGraph.Flats));
            Assert.AreEqual(0f, ChartSound.Leaf(50f), 1e-6f, "bare in February");
            Assert.AreEqual(1f, ChartSound.Leaf(170f), 1e-6f, "in full leaf in June");
            Assert.AreEqual(0f, ChartSound.Leaf(330f), 1e-6f, "bare again by the end of November");
            Assert.AreEqual("Rain", ChartSound.KindOf(Weather(4f, 0.5f)));
            Assert.AreEqual("Storm", ChartSound.KindOf(Weather(18f, 0.5f, 0.8f)));
            // a context's plain facts
            var c = ChartSound.For(ChartSound.Ground.Sea, ChartSound.SharesOf(ChartSound.Ground.Sea), Weather(9f), 23f, 150f, -20f, 2f);
            Assert.AreEqual(1f, c.night, 1e-6f, "the Sun twenty degrees under: night");
            Assert.AreEqual(2f, c.aboveWater, 1e-6f, "the ear is on deck");
            Assert.AreEqual(0f, c.shipDistance, 1e-6f, "the listener is aboard");
            Assert.AreEqual(2f, c.shipSpeed, 1e-6f);
            Assert.Greater(c.shipRollRate, 0f, "she works in a seaway");
            Assert.AreEqual(0f, ChartSound.For(ChartSound.Ground.Port, ChartSound.SharesOf(ChartSound.Ground.Port), Weather(9f), 12f, 150f, 50f, 0f).shipRollRate, 1e-6f,
                            "made fast she does not roll");
        }

        [Test]
        public void TheSettingGoesRoundOffLowMiddlingLoud()
        {
            int was = ChartSound.Setting;
            try
            {
                ChartSound.Setting = 2;
                Assert.AreEqual("middling", ChartSound.SettingWord);
                ChartSound.NextSetting();
                Assert.AreEqual("loud", ChartSound.SettingWord);
                ChartSound.NextSetting();
                Assert.AreEqual("off", ChartSound.SettingWord);
                Assert.AreEqual(0, ChartSound.Setting);
                ChartSound.NextSetting();
                Assert.AreEqual("low", ChartSound.SettingWord);
                ChartSound.Setting = 99;
                Assert.AreEqual(3, ChartSound.Setting, "held within its steps");
            }
            finally
            {
                ChartSound.Setting = was;
            }
        }
    }
}
