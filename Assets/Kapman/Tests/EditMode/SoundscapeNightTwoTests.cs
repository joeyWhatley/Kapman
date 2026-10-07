using System;
using System.IO;
using System.Linq;
using Kapman.Audio;
using Kapman.World;
using NUnit.Framework;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>The audio design's night-two tests (4.4: 5, 6, 7, 11, 13), adapted to the species the library has, and the
    /// night-two data: the voices, the groups, the vegetation, the ship's laws, the humidity rule and the load checks.</summary>
    public class SoundscapeNightTwoTests
    {
        const string Crickets = "night_crickets_522299";
        static string AudioDir => Path.Combine(Application.dataPath, "Kapman", "Audio");
        static SoundscapeDef Def() => SoundscapeDef.Parse(File.ReadAllText(Path.Combine(AudioDir, "soundscape.json")));

        static readonly string[] Vegetation = { "reeds", "reeds_shore", "leaves", "bare_trees", "dry_grass" };

        /// <summary>Dorestad's dams on 21 June at 4.48, fair, B2 (design 3.1 and test 5).</summary>
        static SoundContext JuneDawn() => new SoundContext
        {
            day = 172f, hour = 4.8f, sunAltitude = 6f, night = 0f,
            wind = 2f, rain = 0f, temperature = 11f, humidity = 0.9f, cloud = 0.1f, leaf = 1f,
            aboveWater = 3f, shore = 10f,
            reed = 0.3f, hedge = 0.1f, water = 0.4f, basin = 0.3f, fields = 0.2f, wood = 0.15f, town = 0.2f,
            shipDistance = 1e6f,
        };

        /// <summary>Dorestad on a November storm night, B9, rain, by a house in the town (design 3.1 and test 6).</summary>
        static SoundContext NovemberStorm() => new SoundContext
        {
            day = 320f, hour = 23f, sunAltitude = -45f, night = 1f,
            wind = 22f, rain = 1f, storm = 1f, temperature = 6f, humidity = 0.95f, cloud = 1f, leaf = 0f,
            aboveWater = 3f, shore = -100f,
            reed = 0.3f, hedge = 0.1f, water = 0.2f, basin = 0.2f, fields = 0.2f, wood = 0.3f, town = 0.6f,
            shipDistance = 1e6f,
        };

        [Test]
        public void JuneDawnDorestad()
        {
            var def = Def();
            var c = JuneDawn();
            Assert.AreEqual(1f, SoundscapeModel.WildlifeOpen(c), 1e-4f, "B2 and dry: the wildlife is not masked");
            foreach (var id in new[] { "nightingale", "blackbird", "skylark", "swallows", "gulls" })
            {
                var v = def.Layer(id);
                Assert.IsNotNull(v, id);
                Assert.AreEqual("voice", v.kind, id);
                Assert.Greater(v.When(c), 0f, $"{id} is heard at a June dawn");
            }
            Assert.Greater(def.Layer("dawn_dusk_chorus").When(c), 0f, "the dawn chorus group is on");
            Assert.AreEqual(0f, def.Layer("geese_skein").When(c), "no wild geese skeins in June");
            Assert.AreEqual(0f, def.Layer("geese_meadow").When(c), "no wild geese on the meadows in June");
            Assert.AreEqual(0f, def.Layer("tawny_owl").When(c), "the owl is a night voice");
            Assert.Less(SoundscapeModel.LayerDb(def.Layer("wind"), c), -25f, "a light breeze, nothing of a gale");
            Assert.Greater(SoundscapeModel.LayerDb(def.Layer("leaves"), c), -80f, "the leaves are on the trees");
            Assert.AreEqual(-80f, SoundscapeModel.LayerDb(def.Layer("bare_trees"), c), "no bare trees in June");
            Assert.AreEqual(-80f, SoundscapeModel.LayerDb(def.Layer("rain_thatch"), c), "no rain");
        }

        [Test]
        public void NovemberStormNight()
        {
            var def = Def();
            var c = NovemberStorm();
            Assert.AreEqual(0f, SoundscapeModel.WildlifeOpen(c), "B9: the wildlife is masked");
            foreach (var l in def.layers.Where(x => x.kind == "voice" || x.kind == "group"))
                Assert.AreEqual(0f, l.When(c), $"{l.id} is silent in a storm");
            float wind = SoundscapeModel.LayerDb(def.Layer("wind"), c);
            Assert.That(wind, Is.InRange(-14.5f, -14f), "the pure wind bed at its cap");
            float vegetation = Vegetation.Max(id => SoundscapeModel.LayerDb(def.Layer(id), c));
            Assert.Greater(vegetation, wind, "the gale is heard in what it blows across, above the wind bed");
            Assert.Greater(SoundscapeModel.LayerDb(def.Layer("bare_trees"), c), -80f, "bare trees in November");
            Assert.AreEqual(-80f, SoundscapeModel.LayerDb(def.Layer("leaves"), c), "no leaves in November");
            Assert.GreaterOrEqual(SoundscapeModel.LayerDb(def.Layer("rain"), c), -12.5f, "the rain at full");
            Assert.Greater(SoundscapeModel.LayerDb(def.Layer("rain_thatch"), c), -80f, "rain on the thatch in the town");
            var open = c;
            open.town = 0f;
            Assert.AreEqual(-80f, SoundscapeModel.LayerDb(def.Layer("rain_thatch"), open), "no thatch away from the town");
        }

        [Test]
        public void FrostyCalmMorning()
        {
            var frost = new SoundContext { wind = 0.5f, temperature = -3f, cloud = 0f, hour = 7f, sunAltitude = 4f, day = 20f };
            Assert.AreEqual(1f, SoundscapeModel.Inversion(frost), 1e-4f, "a calm clear frosty morning has an inversion");
            Assert.AreEqual(4f, SoundscapeModel.RefractionDb(500f, 0f, frost, out float f), 0.01f, "far sources +4 dB");
            Assert.AreEqual(1.5f, f, 0.01f, "and their low-pass corner times 1.5");
            Assert.AreEqual(0f, SoundscapeModel.RefractionDb(40f, 0f, frost, out float near), 1e-4f, "a near source is not bent");
            Assert.AreEqual(1f, near, 1e-4f);

            var noon = new SoundContext { wind = 6f, temperature = 14f, cloud = 0.9f, hour = 12.5f, sunAltitude = 45f, day = 20f };
            Assert.AreEqual(0f, SoundscapeModel.Inversion(noon), 1e-4f, "no inversion on a breezy overcast noon");
            Assert.AreEqual(3f, SoundscapeModel.RefractionDb(500f, 6f, noon, out float down), 0.01f, "downwind +3 dB");
            Assert.AreEqual(1.5f, down, 0.01f);
            Assert.AreEqual(-6f, SoundscapeModel.RefractionDb(500f, -6f, noon, out float up), 0.01f, "upwind -6 dB");
            Assert.AreEqual(0.5f, up, 0.01f);
            Assert.AreEqual(0f, SoundscapeModel.RefractionDb(500f, 0f, noon, out _), 0.01f, "a cross wind bends nothing");

            var evening = frost;
            evening.hour = 15f;
            evening.sunAltitude = 10f;
            Assert.AreEqual(0f, SoundscapeModel.Inversion(evening), 1e-4f, "the afternoon ground is still warm");
            // (the design's snow crunch, louder in the cold, waits for a footsteps-in-snow recording: none in the library)
        }

        [Test]
        public void HumidityFollowsTheWeather()
        {
            Assert.AreEqual(1f, SoundscapeModel.HumidityFromWeather("Fog", 12f, 40f), 1e-4f);
            Assert.AreEqual(1f, SoundscapeModel.HumidityFromWeather("Mist", 6f, 5f), 1e-4f);
            Assert.AreEqual(0.95f, SoundscapeModel.HumidityFromWeather("Rain", 12f, 40f), 1e-4f);
            Assert.AreEqual(0.85f, SoundscapeModel.HumidityFromWeather("Overcast", 12f, 40f), 1e-4f);
            Assert.AreEqual(0.7f, SoundscapeModel.HumidityFromWeather("Fair", 13f, 50f), 1e-4f, "fair by day");
            Assert.AreEqual(0.9f, SoundscapeModel.HumidityFromWeather("Fair", 2f, -20f), 1e-4f, "fair at night");
            Assert.AreEqual(0.9f, SoundscapeModel.HumidityFromWeather("Haze", 6f, 5f), 1e-4f, "the dew after dawn");
            Assert.Less(SoundscapeModel.AirAbsorptionCutoff(2000f, 10f, 0.7f), SoundscapeModel.AirAbsorptionCutoff(2000f, 10f, 1f),
                "fog's wet air takes less of the highs than a fair day's");
        }

        [Test]
        public void CricketsNotAtDorestad()
        {
            var def = Def();
            // a mild June night in a river place with wet surroundings, through the whole year
            for (int day = 1; day <= 365; day++)
            {
                var c = new SoundContext
                {
                    day = day, hour = 23f, sunAltitude = -15f, night = 1f, wind = 1f, temperature = 18f, cloud = 0.2f, leaf = 1f,
                    aboveWater = 3f, shore = -20f, water = 0.4f, reed = 0.3f, basin = 0.3f, hedge = 0.1f, fields = 0.1f,
                    shipDistance = 1e6f,
                };
                foreach (var l in def.layers.Where(x => x.clips.Contains(Crickets)))
                    Assert.AreEqual(0f, l.When(c), $"'{l.id}' plays field crickets in the wet delta on day {day}");
            }
            Assert.IsFalse(def.ClipIds().Contains(Crickets), "night two retires the crickets from the river places' data");
        }

        [Test]
        public void Weekday()
        {
            Assert.AreEqual(6, Calendar.Weekday(2451544.5), "1 January 2000 (Gregorian) was a Saturday");
            Assert.AreEqual(2, Calendar.Weekday(2010701.5), "1 January 793 (Julian) was a Tuesday");
            for (int y = 793; y <= 1066; y += 7)
                for (int doy = 1; doy <= Calendar.DaysIn(y); doy += 11)
                {
                    // LevelClock.JulianDay at midnight of that day
                    double jd = 1721423.5 + 365.0 * (y - 1) + Math.Floor((y - 1) / 4.0) + (doy - 1);
                    var d = Calendar.At(y, doy, 0.0);
                    int zeller = ZellerJulian(d.year, d.month, d.day);
                    Assert.AreEqual(zeller, Calendar.Weekday(jd), $"{d} at midnight");
                    Assert.AreEqual(zeller, Calendar.Weekday(jd + 0.99), $"{d} late in the evening");
                }
        }

        /// <summary>Zeller's congruence for the Julian calendar, turned to 0 = Sunday.</summary>
        static int ZellerJulian(int year, int month, int day)
        {
            if (month < 3)
            {
                month += 12;
                year -= 1;
            }
            int k = year % 100, j = year / 100;
            int h = (day + 13 * (month + 1) / 5 + k + k / 4 + 5 + 6 * j) % 7;     // 0 = Saturday
            return (h + 6) % 7;
        }

        [Test]
        public void TheNightTwoDataLoadsAndIsShippable()
        {
            var def = Def();
            var voices = def.OfKind("voice").ToList();
            Assert.GreaterOrEqual(voices.Count, 10, "the roster of 1.4 for the species the library has");
            Assert.GreaterOrEqual(def.OfKind("group").Count(), 1);
            foreach (var g in def.OfKind("group"))
                foreach (var m in g.members)
                    Assert.AreEqual("voice", def.Layer(m)?.kind, $"group '{g.id}' member '{m}'");
            foreach (var v in voices)
            {
                Assert.Greater(v.clips.Length, 0, v.id);
                Assert.GreaterOrEqual(v.distMin, 1f, v.id);
                Assert.Greater(v.intervalMin, 0f, v.id);
                Assert.Greater(v.spatial, 0f, $"{v.id} is placed in the world");
                Assert.IsTrue(v.When(NovemberStorm()) == 0f, $"{v.id} takes the wildlife mask");
            }
            foreach (var id in Vegetation.Concat(new[] { "rain_thatch", "rain_water", "ship_bow", "ship_wake", "ship_oars", "ship_creak" }))
                Assert.IsNotNull(def.Layer(id), id);
            string credits = File.ReadAllText(Path.Combine(AudioDir, "CREDITS_sounds.md"));
            foreach (var id in def.ClipIds().Distinct())
            {
                Assert.IsTrue(File.Exists(Path.Combine(AudioDir, "Sounds", id + ".mp3")), $"{id}.mp3 is in the library");
                var line = credits.Split('\n').FirstOrDefault(l => l.Contains("(" + id + ".mp3"));
                Assert.IsNotNull(line, $"{id} has its credit line");
                Assert.IsTrue(line.Contains("CC0") || line.Contains("public domain"), $"{id} is CC0 or public domain");
            }
        }

        [Test]
        public void UnknownNamesFailAtLoad()
        {
            Assert.Throws<ArgumentException>(() => SoundscapeDef.Parse(
                "{\"layers\":[{\"id\":\"x\",\"kind\":\"bed\",\"clip\":\"y\",\"when\":{\"surround.marsh\":[0,1]}}]}"));
            Assert.Throws<ArgumentException>(() => SoundscapeDef.Parse(
                "{\"layers\":[{\"id\":\"x\",\"kind\":\"crossfade\",\"driver\":\"force\",\"fade_by\":\"ship.knots\",\"clips\":[[\"y\",1]]}]}"));
            Assert.Throws<FormatException>(() => SoundscapeDef.Parse(
                "{\"layers\":[{\"id\":\"g\",\"kind\":\"group\",\"members\":[\"nobody\"]}]}"));
            Assert.Throws<FormatException>(() => SoundscapeDef.Parse(
                "{\"layers\":[{\"id\":\"v\",\"kind\":\"voice\",\"clips\":[\"y\"],\"mask\":\"storm\"}]}"));
            Assert.Throws<FormatException>(() => SoundscapeDef.Parse(
                "{\"layers\":[{\"id\":\"v\",\"kind\":\"voice\",\"clips\":[]}]}"));
            Assert.Throws<FormatException>(() => SoundscapeDef.Parse(
                "{\"layers\":[{\"id\":\"v\",\"kind\":\"bed\",\"clip\":\"y\"},{\"id\":\"v\",\"kind\":\"bed\",\"clip\":\"z\"}]}"));
        }

        [Test]
        public void WindowsWrapAndRampsHaveFloors()
        {
            var def = SoundscapeDef.Parse(
                "{\"layers\":[{\"id\":\"w\",\"kind\":\"voice\",\"clips\":[\"a\"],\"when\":{\"day\":[[300,330],[40,60]],\"night\":[-1,1]}}]}");
            var w = def.Layer("w");
            float At(float day, float night) => w.When(new SoundContext { day = day, night = night });
            Assert.AreEqual(1f, At(10f, 1f), 1e-4f, "deep winter, over the year's end");
            Assert.AreEqual(1f, At(360f, 1f), 1e-4f);
            Assert.AreEqual(0f, At(180f, 1f), 1e-4f, "not in summer");
            Assert.AreEqual(0.5f, At(315f, 1f), 1e-4f, "rising in November");
            Assert.AreEqual(0.5f, At(50f, 1f), 1e-4f, "falling in February");
            Assert.AreEqual(0.5f, At(10f, 0f), 1e-4f, "[-1, 1] gives a floor of 0.5 by day");
        }

        [Test]
        public void VegetationFollowsTheSeasonAndTheWind()
        {
            var def = Def();
            var july = new SoundContext { day = 200f, leaf = 1f, wind = 6f, wood = 0.3f, reed = 0.3f, fields = 0.3f, shore = -100f };
            var january = july;
            january.day = 15f;
            january.leaf = 0f;
            Assert.Greater(SoundscapeModel.LayerDb(def.Layer("leaves"), july), -80f);
            Assert.AreEqual(-80f, SoundscapeModel.LayerDb(def.Layer("bare_trees"), july));
            Assert.AreEqual(-80f, SoundscapeModel.LayerDb(def.Layer("leaves"), january));
            Assert.Greater(SoundscapeModel.LayerDb(def.Layer("bare_trees"), january), -80f);
            Assert.Greater(SoundscapeModel.LayerDb(def.Layer("dry_grass"), january), -80f, "the dead grass of winter");
            var may = july;
            may.day = 130f;
            Assert.AreEqual(-80f, SoundscapeModel.LayerDb(def.Layer("dry_grass"), may), "the grass is green in May");
            var calm = july;
            calm.wind = 0.5f;
            foreach (var id in Vegetation)
                Assert.AreEqual(-80f, SoundscapeModel.LayerDb(def.Layer(id), calm), $"{id} is still in a calm");
            float last = -999f;
            for (float u = 1.5f; u < 30f; u += 0.5f)
            {
                var c = july;
                c.wind = u;
                float db = SoundscapeModel.LayerDb(def.Layer("reeds"), c);
                Assert.GreaterOrEqual(db, last - 1e-4f, $"the reeds never quieten as the wind rises ({u} m/s)");
                Assert.LessOrEqual(db, -8f, "the vegetation's cap");
                last = db;
            }
        }

        [Test]
        public void TheShipsLaws()
        {
            var def = Def();
            SoundContext Ship(float speed, float rowing, float rate, float distance) =>
                new SoundContext { shipSpeed = speed, shipRowing = rowing, shipRollRate = rate, shipPitchRate = 0f, shipDistance = distance };
            var bow = def.Layer("ship_bow");
            Assert.AreEqual("ship.speed", bow.FadeKey, "the bow wave's clips are set along her speed");
            Assert.IsTrue(bow.OnShip);
            float still = SoundscapeModel.LayerDb(bow, Ship(0f, 0f, 0f, 10f));
            float rowed = SoundscapeModel.LayerDb(bow, Ship(1.6f, 1f, 0f, 10f));
            float fast = SoundscapeModel.LayerDb(bow, Ship(4f, 0f, 0f, 10f));
            Assert.Greater(rowed, still, "the bow wave rises with her speed");
            Assert.Greater(fast, rowed);
            Assert.AreEqual(bow.capDb, fast, 0.01f, "at its cap by 4 m/s");
            Assert.AreEqual(12.46f, SoundscapeModel.BowWaveDb(1.6f), 0.05f, "20 log10(1 + v^2 / 0.8) at 1.6 m/s");
            Assert.AreEqual(-80f, SoundscapeModel.LayerDb(bow, Ship(1.6f, 1f, 0f, 500f)), "not heard 500 m off");

            var oars = def.Layer("ship_oars");
            Assert.AreEqual(-80f, SoundscapeModel.LayerDb(oars, Ship(0f, 0f, 0f, 10f)), "no oars at rest");
            Assert.AreEqual(-80f, SoundscapeModel.LayerDb(oars, Ship(3f, 0f, 0f, 10f)), "no oars under sail");
            Assert.Greater(SoundscapeModel.LayerDb(oars, Ship(1.6f, 1f, 0f, 10f)), -80f, "the oars when she is rowed");

            var creak = def.Layer("ship_creak");
            Assert.Greater(SoundscapeModel.LayerDb(creak, Ship(0f, 0f, 8f, 10f)), SoundscapeModel.LayerDb(creak, Ship(0f, 0f, 1f, 10f)),
                "the hull works harder as she rolls faster");
            Assert.AreEqual(12f, SoundscapeModel.HullWorkDb(2f, 2f), 0.01f, "12 dB a doubling over 4 degrees a second");
        }

        [Test]
        public void AVoiceFallsSixDbADoubling()
        {
            var def = Def();
            var v = def.Layer("nightingale");
            float at20 = SoundscapeModel.VoiceDb(v, 20f, def.splToDb), at40 = SoundscapeModel.VoiceDb(v, 40f, def.splToDb);
            Assert.AreEqual(6.02f, at20 - at40, 0.01f);
            Assert.AreEqual(v.splDb + def.splToDb + v.trimDb, SoundscapeModel.VoiceDb(v, 1f, def.splToDb), 1e-3f, "its source level at 1 m");
        }

        [Test]
        public void TheGuessedSurroundingsFollowTheShore()
        {
            var river = new SoundContext { shore = 100f, aboveWater = 5f };
            SoundscapeModel.GuessSurroundings(ref river);
            Assert.AreEqual(1f, river.water, 1e-4f, "out on the river");
            Assert.AreEqual(0f, river.town, 1e-4f);
            var bank = new SoundContext { shore = -5f, aboveWater = 3f };
            SoundscapeModel.GuessSurroundings(ref bank);
            Assert.Greater(bank.reed, 0.3f, "reeds along the bank");
            var town = new SoundContext { shore = -150f, aboveWater = 4f };
            SoundscapeModel.GuessSurroundings(ref town);
            Assert.Greater(town.town, 0.3f, "the town on the levee");
            Assert.Less(town.water, 0.01f);
            var basin = new SoundContext { shore = -900f, aboveWater = 2f };
            SoundscapeModel.GuessSurroundings(ref basin);
            Assert.Greater(basin.basin, 0.4f, "the wet basins far back");
            var high = new SoundContext { shore = -150f, aboveWater = 400f };
            SoundscapeModel.GuessSurroundings(ref high);
            Assert.AreEqual(0f, high.town + high.wood + high.hedge + high.reed, 1e-4f, "high up, the land's rustle is far below");
        }
    }
}
