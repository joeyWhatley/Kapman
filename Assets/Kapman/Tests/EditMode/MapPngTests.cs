using System.IO;
using Kapman.Game;
using NUnit.Framework;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>
    /// The map's own reader of PNGs (MapPng: it reads a tile off the main thread) gives the same pixels as the engine's:
    /// for a picture the engine itself packed (RGBA, every kind of row filter), and for the chart's real tiles as the
    /// bakers write them (RGB at level 0, colours of a palette at the finer levels), as painted and recoloured.
    /// </summary>
    public class MapPngTests
    {
        static Color32[] ByTheEngine(byte[] file, int which, out int w, out int h)
        {
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            Assert.IsTrue(tex.LoadImage(file, false));
            w = tex.width;
            h = tex.height;
            var px = tex.GetPixels32();
            Object.DestroyImmediate(tex);
            if (which > 0)
                for (int i = 0; i < px.Length; i++)
                    px[i] = MapPalette.Map(px[i], which);
            return px;
        }

        static void Same(Color32[] want, Color32[] got, string what)
        {
            Assert.IsNotNull(got, what + ": not read");
            Assert.AreEqual(want.Length, got.Length, what + ": another size");
            for (int i = 0; i < want.Length; i++)
                if (want[i].r != got[i].r || want[i].g != got[i].g || want[i].b != got[i].b || want[i].a != got[i].a)
                    Assert.Fail($"{what}: pixel {i} is {got[i]}, the engine reads {want[i]}");
        }

        [Test]
        public void APictureTheEnginePackedIsReadTheSame()
        {
            var tex = new Texture2D(97, 61, TextureFormat.RGBA32, false);
            var rnd = new System.Random(793);
            var px = new Color32[97 * 61];
            for (int i = 0; i < px.Length; i++)
            {
                // (runs of one colour, gradients and noise: so the packer uses its different row filters)
                int x = i % 97, y = i / 97;
                px[i] = y < 20 ? new Color32((byte)(x * 2), (byte)(y * 9), 40, 255)
                      : y < 40 ? new Color32((byte)rnd.Next(256), (byte)rnd.Next(256), (byte)rnd.Next(256), (byte)rnd.Next(256))
                      : new Color32((byte)(x / 16 * 60), 90, (byte)(y * 3), (byte)(x > 50 ? 255 : 128));
            }
            tex.SetPixels32(px);
            tex.Apply(false);
            byte[] file = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);
            var want = ByTheEngine(file, 0, out int w, out int h);
            var got = MapPng.Read(file, 0, null, out int gw, out int gh);
            Assert.AreEqual(w, gw);
            Assert.AreEqual(h, gh);
            Same(want, got, "a packed RGBA picture");
            // and into an array handed in, of the right size: that array is the one filled
            var into = new Color32[w * h];
            Assert.AreSame(into, MapPng.Read(file, 0, into, out _, out _), "the array handed in was not used");
            Same(want, into, "the same, into an array handed in");
        }

        [Test]
        public void WhatIsNotAPngIsDeclined()
        {
            Assert.IsNull(MapPng.Read(null, 0, null, out _, out _));
            Assert.IsNull(MapPng.Read(new byte[40], 0, null, out _, out _));
            Assert.IsNull(MapPng.Read(System.Text.Encoding.ASCII.GetBytes("this is no picture at all, only words in a row"), 0, null, out _, out _));
        }

        [Test]
        public void TheChartsOwnTilesAreReadTheSame()
        {
            string folder = Path.Combine(Application.dataPath, "..", "MapArt", "world");
            if (!File.Exists(Path.Combine(folder, "map_art.json")))
                Assert.Ignore("No map art beside this project (MapArt/world): nothing to read.");
            int read = 0;
            foreach (string sub in new[] { "", "L1", "L2", "L3" })
            {
                string dir = Path.Combine(folder, sub);
                if (!Directory.Exists(dir))
                    continue;
                string[] files = Directory.GetFiles(dir, "t_*.png");
                if (files.Length == 0)
                    continue;
                System.Array.Sort(files, System.StringComparer.Ordinal);
                string file = files[files.Length / 2];
                byte[] bytes = File.ReadAllBytes(file);
                foreach (int which in new[] { 0, 1 })
                {
                    var want = ByTheEngine(bytes, which, out int w, out int h);
                    var got = MapPng.Read(bytes, which, null, out int gw, out int gh);
                    Assert.IsNotNull(got, file + ": MapPng declined a tile of the bakers' own: every tile would be read the slow way");
                    Assert.AreEqual(w, gw, file);
                    Assert.AreEqual(h, gh, file);
                    Same(want, got, Path.GetFileName(file) + " of " + (sub.Length > 0 ? sub : "level 0") + (which > 0 ? ", recoloured" : ", as painted"));
                }
                read++;
            }
            Assert.Greater(read, 0, "no tile was found to read");
        }
    }
}
