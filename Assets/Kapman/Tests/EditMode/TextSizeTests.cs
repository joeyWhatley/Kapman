using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kapman.Game;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// THE TEXT'S SIZES (7 Oct 2026; Design/text_size_audit.md): four sizes only in the frame (18, 22, 26 and 30 at
    /// 1080p), each a variable on the stylesheet's root; no size written as a number anywhere else in the frame's
    /// stylesheet; and a floor of 18 true pixels on any screen down to half of 1080p, kept by a class on the root
    /// (TextFloor). The play test looks at the screen itself; this holds the sums and the files.
    /// </summary>
    public class TextSizeTests
    {
        const string Frame = "Assets/Kapman/Game/UI/Frame.uss";
        const string Menu = "Assets/Kapman/Game/UI/MainMenu.uss";
        const string Map = "Assets/Kapman/Game/UI/MapView.uss";

        [Test]
        public void TheFloorHoldsEighteenTruePixels()
        {
            Assert.IsNull(TextFloor.ClassFor(1f), "1920 x 1080: no floor needed");
            Assert.IsNull(TextFloor.ClassFor(2f), "a larger screen: none");
            Assert.IsNull(TextFloor.ClassFor(0f));
            Assert.AreEqual("t-floor-22", TextFloor.ClassFor(900f / 1080f), "1600 x 900");
            Assert.AreEqual("t-floor-26", TextFloor.ClassFor(768f / 1080f), "1366 x 768");
            Assert.AreEqual("t-floor-28", TextFloor.ClassFor(720f / 1080f), "1280 x 720");
            Assert.AreEqual("t-floor-36", TextFloor.ClassFor(0.5f), "960 x 540");
            Assert.AreEqual("t-floor-36", TextFloor.ClassFor(0.3f), "smaller than the frame can be shown on: the last class stands");
            // every scale from a half up: the least size the class gives, times the scale, is 18 or over
            for (float s = 0.5f; s < 1f; s += 0.005f)
            {
                int px = TextFloor.PanelPx(TextFloor.ClassFor(s));
                Assert.GreaterOrEqual(px * s, 17.99f, "scale " + s);
                Assert.Less(px * s, 21f, "scale " + s + ": lifted further than it need be");
            }
        }

        [Test]
        public void TheFramesStylesheetHasFourSizesAndNoOther()
        {
            string uss = File.ReadAllText(Frame);
            // (Hearth, a pixel face of 8: two sizes at 1080p, 16 and 24, under the four names)
            StringAssert.Contains("--t-quiet: 16px;", uss);
            StringAssert.Contains("--t-body: 16px;", uss);
            StringAssert.Contains("--t-name: 24px;", uss);
            StringAssert.Contains("--t-title: 24px;", uss);
            var written = Regex.Matches(uss, @"font-size:\s*([^;]+);").Cast<Match>().Select(m => m.Groups[1].Value.Trim()).ToList();
            Assert.Greater(written.Count, 40, "the frame's stylesheet sets no sizes?");
            foreach (string size in written)
                StringAssert.IsMatch(@"^var\(--t-(quiet|body|name|title)\)$", size, "a size in Frame.uss that is not one of the four: font-size: " + size);
            // every step the floor can ask for has its class, and a class never makes a size smaller
            foreach (var root in new[] { (Frame, ".frame-root"), (Menu, ".menu-root"), (Map, ".map-root") })
            {
                string sheet = File.ReadAllText(root.Item1);
                for (int step = 20; step <= 36; step += 2)
                {
                    var m = Regex.Match(sheet, Regex.Escape(root.Item2) + @"\.t-floor-" + step + @"\s*\{([^}]*)\}");
                    Assert.IsTrue(m.Success, root.Item1 + " has no class " + root.Item2 + ".t-floor-" + step);
                    foreach (Match v in Regex.Matches(m.Groups[1].Value, @"--t-(quiet|body|name|title):\s*(\d+)px"))
                    {
                        Assert.AreEqual(step, int.Parse(v.Groups[2].Value), "a floor class sets every size it touches to its own number");
                        int usual = v.Groups[1].Value == "quiet" || v.Groups[1].Value == "body" ? 16 : 24;
                        Assert.Greater(step, usual, root.Item1 + " t-floor-" + step + " makes " + v.Groups[1].Value + " smaller than it is");
                    }
                    StringAssert.Contains("--t-quiet", m.Groups[1].Value, "every floor class lifts the quiet size");
                    if (step > 22) StringAssert.Contains("--t-body", m.Groups[1].Value);
                    if (step > 26) StringAssert.Contains("--t-name", m.Groups[1].Value);
                    if (step > 30) StringAssert.Contains("--t-title", m.Groups[1].Value);
                }
            }
        }

        [Test]
        public void TheMenuReadsInTheSameSizes()
        {
            string uss = File.ReadAllText(Menu);
            var written = Regex.Matches(uss, @"font-size:\s*([^;]+);").Cast<Match>().Select(m => m.Groups[1].Value.Trim()).ToList();
            // (the game's own name on the title is the one sign that is larger: 44, and it scales with the screen)
            Assert.AreEqual(1, written.Count(s => !s.StartsWith("var(--t-")), "MainMenu.uss: only the game's name on the title is not one of the four: " + string.Join(", ", written));
            CollectionAssert.Contains(written, "44px");
        }
    }
}
