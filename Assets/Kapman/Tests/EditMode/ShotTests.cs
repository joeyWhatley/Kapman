using Kapman.Game;
using Kapman.Game.Sim;
using Kapman.Sailing;
using NUnit.Framework;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>The meetings' event shots: a band's karves in echelon astern of the leader; the two-shot from abeam with
    /// both ships in frame; the sighting shot from behind your boat, its lens on her.
    /// ported 4 Oct: the first build placed her through FloatingOrigin (its one world was the stage); the rebuild's shots
    /// find her through SimStage (ToScene, WaterHeight), so the test gives the simulation a stage of its own: the scene's
    /// metres are the world's, the water lies at 0, as the first build's empty test scene had it. The framing is
    /// unchanged, and so are the checks.</summary>
    public class ShotTests
    {
        GameObject camGo, mineGo, directorGo;

        /// <summary>An identity stage: scene metres are world metres, the water at 0, nothing in the way.</summary>
        sealed class FlatStage : ISimStage
        {
            public WorldXZ ToWorld(Vector3 scene) => new WorldXZ(scene.x, scene.z);
            public Vector3 ToScene(WorldXZ world) => new Vector3((float)world.x, 0f, (float)world.z);
            public bool OnStage(WorldXZ world) => true;
            public float WaterHeight(Vector3 scene) => 0f;
            public float LandAboveWater(WorldXZ world) => -10f;
            public void Wind(out float windFrom, out float windSpeed)
            {
                windFrom = 225f;
                windSpeed = 5f;
            }
            public double HazeKm => 40.0;
            public double VisibilityKmTo(WorldXZ world) => 40.0;
            public int DayOfYear => 172;
            public double HourOfDay => 12.0;
            public double JulianDay => Kapman.World.SkyMath.JulianDay(793, 6, 21, 12.0);
            public void HoldClock(bool hold) { }
        }

        [SetUp]
        public void Stage() => SimStage.Current = new FlatStage();

        [TearDown]
        public void Done()
        {
            SimStage.Current = null;                                        // (the level's stage again)
            Object.DestroyImmediate(camGo);
            Object.DestroyImmediate(mineGo);
            Object.DestroyImmediate(directorGo);
        }

        [Test]
        public void ABandLiesInEchelonAstern()
        {
            var first = WorldShips3D.EchelonOffset(1, 0f, 20f);               // heading north
            var second = WorldShips3D.EchelonOffset(2, 0f, 20f);
            var third = WorldShips3D.EchelonOffset(3, 0f, 20f);
            Assert.Less(first.x, 0f, "the first to port");
            Assert.Greater(second.x, 0f, "the second to starboard");
            Assert.Less(first.z, 0f, "astern");
            Assert.AreEqual(first.z, second.z, 0.01f, "the same rank");
            Assert.Less(third.z, first.z, "the next rank further astern");
            var east = WorldShips3D.EchelonOffset(1, 90f, 20f);              // heading east: astern is west
            Assert.Less(east.x, 0f);
        }

        [Test]
        public void TheShotsFrameTheirSubjects()
        {
            camGo = new GameObject("camera");
            var cam = camGo.AddComponent<Camera>();
            cam.aspect = 16f / 9f;
            mineGo = new GameObject("mine");
            var mine = mineGo.AddComponent<SailingShip>();
            directorGo = new GameObject("director");
            var director = directorGo.AddComponent<ShotDirector>();

            // her 40 m east of your boat (the scene's metres: the stage's world metres), heading north
            var her = new SimShip
            {
                id = "t.1", cls = new ShipClassDef { id = "t", length = 12f, beam = 3f, hasSail = true },
                heading = 0.0, pos = new WorldXZ(40.0, 0.0), prevPos = new WorldXZ(40.0, 0.0),
            };
            mineGo.transform.position = Vector3.zero;
            camGo.transform.position = new Vector3(20f, 10f, -50f);           // the camera south of the pair

            Assert.IsTrue(director.Frame(ShotDirector.Shot.TwoShot, her, mine, cam, out var pos, out var rot, out float fov, out _));
            Assert.Less(pos.z, -20f, "from abeam, on the side the camera was");
            Assert.AreEqual(20f, pos.x, 2f, "square to the middle of the pair");
            cam.transform.SetPositionAndRotation(pos, rot);
            cam.fieldOfView = fov;
            foreach (var p in new[] { new Vector3(0f, 1.6f, 0f), new Vector3(40f, 2f, 0f) })
            {
                var v = cam.WorldToViewportPoint(p);
                Assert.That(v.x, Is.InRange(0.05f, 0.95f), "both ships in frame");
                Assert.That(v.y, Is.InRange(0.4f, 0.95f), "in the upper part, clear of the panel");
            }

            Assert.IsTrue(director.Frame(ShotDirector.Shot.Sighting, her, mine, cam, out pos, out rot, out fov, out float focus));
            Assert.Less(pos.x, 0f, "from behind your boat");
            Assert.That(fov, Is.InRange(director.minFov, director.maxFov));
            Assert.AreEqual(Vector3.Distance(pos, new Vector3(40f, 2f, 0f)), focus, 0.5f, "focused on her");
            cam.transform.SetPositionAndRotation(pos, rot);
            cam.fieldOfView = fov;
            var seen = cam.WorldToViewportPoint(new Vector3(40f, 2f, 0f));
            Assert.That(seen.x, Is.InRange(0.3f, 0.7f), "her in the middle");
            Assert.Greater(seen.z, 0f, "in front of the camera");
        }
    }
}
