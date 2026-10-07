using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using Kapman.Game.ShipView;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Kapman.Tests
{
    /// <summary>
    /// THE TITLE'S SEA, LOOKED AT BY ITSELF (the user, 7 Oct 2026: "a top down of a stormy ocean with rain"). The stage
    /// alone, in its title sea, drawn in the looks of purple tones, with the rain on and off and from two heights:
    /// pictures in Captures/ship_view/title_sea, and how light and how varied each one is in title_sea.txt. A sea that
    /// is one flat tone is a fault.
    /// Run: -runTests -testPlatform PlayMode -testFilter TitleSeaTests   (with graphics: no -nographics)
    /// </summary>
    [PrebuildSetup(typeof(ShipStageTests))]
    public class TitleSeaTests
    {
        [UnityTest]
        public IEnumerator TheTitlesSeaIsASeaAndNotAFlatTone()
        {
            var kit = ShipStageKit.Load();
            Assert.IsNotNull(kit, "no Resources/" + ShipStageKit.ResourceName + ": run Kapman > Ship view > Make the kit");
            ShipStageHook.TurnOff();
            string dir = ShipViewShots.Folder("title_sea");
            foreach (string old in Directory.GetFiles(dir, "*.png"))
                File.Delete(old);
            var fake = new ShipViewShots.Fake { state = ShipStageState.Fair() };
            var stage = ShipStage.Build(kit, fake);
            var look = new PsxStageLook("ink3");
            stage.Look = look;
            stage.Big = true;
            stage.TitleSea = true;
            Time.captureFramerate = 30;
            var log = new StringBuilder("picture; mean light; spread; colours\n");
            float best = 1f;
            try
            {
                int n = ShipStage.TitleWeather.Length;
                foreach (string style in new[] { "console", "ink3" })
                {
                    look.SetStyle(style);
                    look.Show(1920, 1080);
                    for (int w = 0; w < n; w++)
                    {
                        // (early in each weather's own time; the sea is given some seconds of it to come to that wind)
                        ShipStage.TitleSeaClock = w * ShipStage.TitleSeaEach + 6f;
                        for (int li = 0; li < stage.TitleIn.Length; li++)
                            stage.TitleIn[li] = 1f;                       // (the name's letters afloat)
                        stage.Settle();
                        for (int f = 0; f < (style == "console" ? 200 : 60); f++)
                        {
                            for (int li = 0; li < stage.TitleIn.Length; li++)
                                stage.TitleIn[li] = 1f;
                            yield return null;
                        }
                        string name = style + "_" + (w + 1) + "_" + ShipStage.TitleWeather[w].name.Replace(' ', '_');
                        var picture = look.Output as RenderTexture;
                        Assert.IsNotNull(picture, "the look " + style + " drew no picture of its own");
                        var tex = ShipViewShots.Read(picture);
                        File.WriteAllBytes(Path.Combine(dir, name + ".png"), tex.EncodeToPNG());
                        var px = tex.GetPixels32();
                        double sum = 0.0, sq = 0.0;
                        var seen = new System.Collections.Generic.HashSet<int>();
                        foreach (var c in px)
                        {
                            double y = (0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b) / 255.0;
                            sum += y;
                            sq += y * y;
                            seen.Add(c.r << 16 | c.g << 8 | c.b);
                        }
                        double mean = sum / px.Length, spread = System.Math.Sqrt(System.Math.Max(0.0, sq / px.Length - mean * mean));
                        log.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0}; {1:0.000}; {2:0.000}; {3}", name, mean, spread, seen.Count));
                        if (style == "console")
                            best = Mathf.Min(best, (float)spread);
                        if (style == "console" && w == 0)
                            Assert.IsTrue(stage.TitleLettersUp, "the name's letters were not made into things for the title's sea");
                        Assert.AreEqual(ShipStage.TitleWeather[w].name, stage.TitleWeatherNow);
                        Object.Destroy(tex);
                    }
                }
            }
            finally
            {
                File.WriteAllText(Path.Combine(dir, "title_sea.txt"), log.ToString());
                ShipStage.TitleSeaClock = null;
                Object.Destroy(stage.gameObject);
                Time.captureFramerate = 0;
                ShipStageHook.Off = ShipStageHook.StartsOff;
            }
            yield return null;
            Debug.Log("The title's sea:\n" + log);
            Assert.Greater(best, 0.02f, "one of the title's weathers is one flat tone:\n" + log);
        }
    }
}
