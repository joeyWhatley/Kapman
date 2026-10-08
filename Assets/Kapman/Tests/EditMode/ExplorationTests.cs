using Kapman.Game;
using NUnit.Framework;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>
    /// THE FOG OF WAR HIDES WHAT SHE HAS NOT SEEN (the user, 9 Oct 2026: "Why are we doing such a subtle fog? Shouldn't it
    /// be you can't see anything you haven't been?"). Three states, as its picture has them: never seen (the fog, whole:
    /// nothing shows through); seen, but out of sight now (a thin veil: what is remembered); in sight now (clear). And
    /// what she has passed stays known behind her.
    /// </summary>
    public class ExplorationTests
    {
        const double Cell = Exploration.CellSize;

        [TearDown]
        public void Done() => Exploration.Clear();

        /// <summary>The fog's own square over a place of a world 800 km a side with its south-west corner at 0, 0.</summary>
        static Color32 Fog(double x, double z)
        {
            var tex = Exploration.Texture;
            int c = (int)System.Math.Floor(x / Cell), r = (int)System.Math.Floor((800000.0 - z) / Cell);
            return tex.GetPixels32()[(tex.height - 1 - r) * tex.width + c];      // (the picture's rows run south first)
        }

        [Test]
        public void TheFogHidesWhatSheHasNotSeenAndLiftsWhereSheHasBeen()
        {
            Exploration.Begin(0.0, 800000.0, 800000.0, 0.0);
            Assert.AreEqual(1f, Exploration.FogAlpha, "the fog over the unknown should hide it altogether");
            var tex = Exploration.Texture;
            Assert.AreEqual(100, tex.width);
            Assert.AreEqual(100, tex.height);
            foreach (var p in tex.GetPixels32())
            {
                Assert.AreEqual(255, p.a, "a new world is not all in fog");
                Assert.AreEqual(Exploration.Dark.r, p.r);
            }
            Assert.AreEqual(0f, Exploration.Seen(400000.0, 400000.0));

            // home waters are known from the start; she lies in the middle of them and sees round her
            Exploration.Reveal(400000.0, 400000.0, Exploration.Home);
            Exploration.See(400000.0, 400000.0, Exploration.Sight);
            Exploration.Apply();
            Assert.AreEqual(0, Fog(400000.0, 400000.0).a, "where she lies is not clear");
            Assert.AreEqual((byte)(255 * Exploration.RememberedAlpha), Fog(500000.0, 400000.0).a, "home waters out of her sight should lie under the thin veil of what is remembered");
            Assert.AreEqual(255, Fog(700000.0, 400000.0).a, "water she has never seen shows through the fog");
            Assert.AreEqual(1f, Exploration.Seen(500000.0, 400000.0));
            Assert.AreEqual(0f, Exploration.Seen(700000.0, 400000.0));
            Assert.Greater(Exploration.InSight(400000.0, 400000.0), 0.99f);
            Assert.AreEqual(0f, Exploration.InSight(500000.0, 400000.0));

            // she sails 250 km east, out of her home waters: the fog lifts round her as she goes, and stays lifted behind
            for (double x = 400000.0; x <= 650000.0; x += 5000.0)
            {
                Exploration.See(x, 400000.0, Exploration.Sight);
                Exploration.Apply();
            }
            Assert.AreEqual(0, Fog(650000.0, 400000.0).a, "the fog did not lift where she is now");
            Assert.AreEqual((byte)(255 * Exploration.RememberedAlpha), Fog(590000.0, 400000.0).a, "what she passed an hour ago is not remembered");
            Assert.AreEqual(1f, Exploration.Seen(590000.0, 400000.0));
            Assert.AreEqual(255, Fog(650000.0, 500000.0).a, "a hundred km off her track the fog lifted");
            Assert.AreEqual(255, Fog(760000.0, 400000.0).a, "the fog lifted far ahead of her");
            // the edge of what she sees is a ramp of two squares, not a step
            byte edge = Fog(650000.0, 400000.0 + Exploration.Sight).a;
            Assert.IsTrue(edge > 20 && edge < 235, "the edge of her sight is a hard step: " + edge);
        }
    }
}
