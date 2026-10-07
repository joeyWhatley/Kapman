using System.IO;
using System.Linq;
using Kapman.Audio;
using NUnit.Framework;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>The audio design's night-one tests (4.4: 1, 2, 3, 4, 8) and the data: soundscape.json loads, every clip it
    /// names is in the library and in the credits as CC0.</summary>
    public class SoundscapeTests
    {
        static string AudioDir => Path.Combine(Application.dataPath, "Kapman", "Audio");
        static SoundscapeDef Def() => SoundscapeDef.Parse(File.ReadAllText(Path.Combine(AudioDir, "soundscape.json")));

        [Test]
        public void SpeedOfSound()
        {
            Assert.AreEqual(331.3f, SoundscapeModel.SpeedOfSound(0f), 0.2f);
            Assert.AreEqual(343.2f, SoundscapeModel.SpeedOfSound(20f), 0.2f);
        }

        [Test]
        public void ThunderDelay()
        {
            Assert.AreEqual(10.9f, SoundscapeModel.ThunderDelay(3600f, -3f), 0.05f);
            Assert.AreEqual(SoundscapeModel.Thunder.Near, SoundscapeModel.ThunderKind(1200f));
            Assert.AreEqual(SoundscapeModel.Thunder.Far, SoundscapeModel.ThunderKind(8000f));
            Assert.AreEqual(SoundscapeModel.Thunder.Silent, SoundscapeModel.ThunderKind(17000f));
            Assert.Greater(SoundscapeModel.ThunderDb(500f), SoundscapeModel.ThunderDb(1400f), "a nearer crack is louder");
            Assert.Greater(SoundscapeModel.ThunderDb(2000f), SoundscapeModel.ThunderDb(8000f), "a nearer rumble is louder");
        }

        [Test]
        public void Beaufort()
        {
            Assert.AreEqual(0, SoundscapeModel.Beaufort(0.2f));
            Assert.AreEqual(3, SoundscapeModel.Beaufort(5.4f));
            Assert.AreEqual(7, SoundscapeModel.Beaufort(13.9f));
            Assert.AreEqual(12, SoundscapeModel.Beaufort(32.7f));
            float last = -1f;
            for (float u = 0f; u < 40f; u += 0.25f)
            {
                float f = SoundscapeModel.Force(u);
                Assert.GreaterOrEqual(f, last, "the smooth force rises with the wind");
                Assert.AreEqual(SoundscapeModel.Beaufort(u), f, 0.75f, $"the smooth force near the table at {u} m/s");
                last = f;
            }
        }

        [Test]
        public void WindGainMonotonic()
        {
            var wind = Def().Layer("wind");
            Assert.IsNotNull(wind);
            Assert.AreEqual("crossfade", wind.kind);
            var w = new float[wind.clips.Length];
            float last = -999f;
            for (float u = 0f; u < 40f; u += 0.25f)
            {
                var c = new SoundContext { wind = u };
                float db = SoundscapeModel.LayerDb(wind, c);
                Assert.GreaterOrEqual(db, last - 1e-4f, $"the wind never quietens as it rises ({u} m/s)");
                Assert.LessOrEqual(db, -14f, "never past -14 dB outside the deck view");
                last = db;
                SoundscapeModel.Crossfade(c.Get(wind.driver), wind.points, w);
                Assert.AreEqual(1f, w.Sum(x => x * x), 0.01f, $"equal power at {u} m/s");
            }
        }

        [Test]
        public void AirAbsorptionCutoff()
        {
            float near = SoundscapeModel.AirAbsorptionCutoff(200f, 10f, 0.8f);
            float far = SoundscapeModel.AirAbsorptionCutoff(2000f, 10f, 0.8f);
            float farther = SoundscapeModel.AirAbsorptionCutoff(8000f, 10f, 0.8f);
            Assert.Greater(near, far);
            Assert.Greater(far, farther);
            Assert.That(far, Is.InRange(1000f, 3000f), $"{far:0} Hz at 2 km, 10 C, 80 %");
            Assert.Less(SoundscapeModel.AirAbsorptionCutoff(2000f, 10f, 0.4f), far, "drier air takes more of the highs");
            // ISO 9613-1's table: 20 C, 70 %, 4 kHz is 22.9 dB/km
            Assert.AreEqual(0.0229, SoundscapeModel.AirAbsorption(4000.0, 20.0, 0.7), 0.0008);
        }

        [Test]
        public void TheDataLoadsAndEveryClipIsShippable()
        {
            var def = Def();
            Assert.IsNotNull(def.Layer("rain"));
            Assert.IsNotNull(def.Layer("lapping"));
            Assert.IsNotNull(def.Layer("thunder"));
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
        public void TheLappingIsAtTheShoreAndTheRainOnlyWhenItRains()
        {
            var def = Def();
            var lap = def.Layer("lapping");
            var atBank = new SoundContext { aboveWater = 2f, shore = 3f };
            var inland = new SoundContext { aboveWater = 2f, shore = -200f };
            var high = new SoundContext { aboveWater = 150f, shore = 3f };
            Assert.Greater(SoundscapeModel.LayerDb(lap, atBank), -30f);
            Assert.AreEqual(-80f, SoundscapeModel.LayerDb(lap, inland));
            Assert.AreEqual(-80f, SoundscapeModel.LayerDb(lap, high));
            var rain = def.Layer("rain");
            Assert.AreEqual(-80f, SoundscapeModel.LayerDb(rain, new SoundContext { rain = 0f }));
            Assert.Greater(SoundscapeModel.LayerDb(rain, new SoundContext { rain = 1f }), SoundscapeModel.LayerDb(rain, new SoundContext { rain = 0.2f }));
        }
    }
}
