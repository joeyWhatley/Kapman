using System.IO;
using System.Linq;
using Kapman.Game.ShipView;
using NUnit.Framework;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>
    /// THE 3D STAGE STANDS IN THE SCENE (the user, 14 Oct 2026: "l chart is a bunch of scripts still"; by the picker,
    /// "Yes, both: frame and stage"). The Kapman scene's file holds the stage's object with its parts saved on it, by
    /// their names, the boat's model a prefab instance of the kit's ship and the sky and air's profile an asset; and
    /// ShipStage.MakeSaved takes what is there and makes nothing twice.
    /// </summary>
    public class StageInSceneTests
    {
        const string Scene = "Assets/Kapman/Scenes/Kapman.unity";
        const string Kit = "Assets/Kapman/ShipView/Resources/ShipStageKit.asset";
        const string Profile = "Assets/Kapman/ShipView/Ship stage sky and air.asset";

        static readonly string[] Parts = { "Sun", "Moon", "Sky and air", "Ship stage camera", "Rain", "Sea (HDRP water)", "Ship", "Fit-out" };

        [Test]
        public void TheSceneHoldsTheStageAndItsParts()
        {
            Assert.IsTrue(File.Exists(Scene), Scene + " is missing");
            string scene = File.ReadAllText(Scene);
            StringAssert.Contains("m_Name: " + ShipStage.ObjectName + "\n", scene.Replace("\r\n", "\n"), "the scene has no object of the stage (Kapman > Make physical > 3)");
            string script = Guid("Assets/Kapman/Code/Game/ShipView/ShipStage.cs.meta");
            StringAssert.Contains("guid: " + script, scene, "the scene's stage has no ShipStage component");
            foreach (var part in Parts)
                StringAssert.Contains("m_Name: " + part + "\n", scene.Replace("\r\n", "\n"), "the stage in the scene has no part named \"" + part + "\"");
            string kit = Guid(Kit + ".meta");
            StringAssert.Contains("guid: " + kit, scene, "the stage in the scene has no kit");
            // her model is the kit's own prefab, an instance of it
            string ship = Regex("  ship: {fileID: \\d+, guid: ([0-9a-f]{32})", File.ReadAllText(Kit));
            Assert.IsNotNull(ship, "the kit names no ship prefab");
            StringAssert.Contains("guid: " + ship, scene, "the boat's model in the scene is not an instance of the kit's ship prefab");
            Assert.IsTrue(File.Exists(Profile), "the sky and air's profile is not saved at " + Profile);
            StringAssert.Contains("guid: " + Guid(Profile + ".meta"), scene, "the stage's volume does not hold the saved profile");
        }

        [Test]
        public void WhatIsSavedIsTakenUpAndNothingIsMadeTwice()
        {
            var kit = Resources.Load<ShipStageKit>(ShipStageKit.ResourceName);
            Assert.IsNotNull(kit, "no kit (Kapman > Ship view > Make the kit)");
            var go = new GameObject("stage under test");
            try
            {
                var stage = go.AddComponent<ShipStage>();
                stage.kit = kit;
                stage.MakeSaved();
                var sun = stage.Sun;
                var camera = stage.View.Camera;
                var volume = stage.Volume;
                var model = stage.Ship.Model;
                int parts = go.GetComponentsInChildren<Transform>(true).Length;
                foreach (var part in Parts)
                    Assert.IsTrue(go.GetComponentsInChildren<Transform>(true).Any(t => t.name == part), "MakeSaved made no \"" + part + "\"");
                Assert.IsNotNull(volume.sharedProfile, "the volume's profile is not the shared one in the editor");
                Assert.AreEqual(kit.origin, go.transform.position);
                Assert.AreEqual(kit.layer, sun.gameObject.layer);

                // made again on the same object: everything is found, nothing added
                stage.MakeSaved();
                Assert.AreEqual(parts, go.GetComponentsInChildren<Transform>(true).Length, "MakeSaved made parts again that were there");
                Assert.AreSame(sun, stage.Sun, "the Sun was made anew");
                Assert.AreSame(camera, stage.View.Camera, "the camera was made anew");
                Assert.AreSame(volume, stage.Volume, "the volume was made anew");
                Assert.AreSame(model, stage.Ship.Model, "her model was made anew");
                Assert.AreEqual(1, go.GetComponentsInChildren<Light>(true).Count(l => l.name == "Sun"));
                Assert.AreEqual(1, go.GetComponentsInChildren<Camera>(true).Length);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        static string Guid(string meta)
        {
            Assert.IsTrue(File.Exists(meta), meta + " is missing");
            var guid = Regex("guid: ([0-9a-f]{32})", File.ReadAllText(meta));
            Assert.IsNotNull(guid, meta + " has no guid");
            return guid;
        }

        static string Regex(string pattern, string text)
        {
            var m = System.Text.RegularExpressions.Regex.Match(text, pattern);
            return m.Success ? m.Groups[1].Value : null;
        }
    }
}
