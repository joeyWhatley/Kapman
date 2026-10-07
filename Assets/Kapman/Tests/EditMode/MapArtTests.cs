using System.IO;
using Kapman.Game;
using Kapman.World;
using NUnit.Framework;
using UnityEngine;

namespace Kapman.Tests
{
    /// <summary>The chart's picture (MapArt): the placeholder's frame is read, it covers the sailing world, a place falls
    /// in the tile its numbers say, and every tile the index promises is on disk.</summary>
    public class MapArtTests
    {
        static string Folder => Path.Combine(Application.dataPath, "..", "MapArt", "world");

        [Test]
        public void FrameIsRead_AndCoversTheSailingWorld()
        {
            var art = MapArt.Open(Folder);
            Assert.IsNotNull(art, "no map art at " + Folder + " (Bake/world/make_map_art.py)");
            Assert.AreEqual(120.0, art.MetresPerPixel, 1e-6);
            Assert.AreEqual(4096, art.TilePixels);
            Assert.AreEqual(6, art.Cols);
            Assert.AreEqual(6, art.Rows);
            foreach (var (lat, lon, name) in new[] { (51.972, 5.343, "Dorestad"), (51.51, -0.12, "Lundenwic"), (59.335, 17.545, "Birka"),
                                                     (58.52, 31.27, "Holmgardr"), (50.45, 30.52, "Kiev"), (44.84, -0.58, "Bordeaux"), (53.35, -6.26, "Dublin") })
            {
                var (x, z) = WorldFrame.ToWorld(lat, lon);
                Assert.IsTrue(art.TileAt(x, z, out _, out _), name + " lies outside the picture");
            }
            var (ix, iz) = WorldFrame.ToWorld(64.15, -21.94);                 // Reykjavik: beyond the sailing world's window
            Assert.IsFalse(art.TileAt(ix, iz, out _, out _), "Iceland's west should lie outside this window");
        }

        [Test]
        public void DorestadFallsInItsTile_AndEveryTileIsOnDisk()
        {
            var art = MapArt.Open(Folder);
            Assert.IsNotNull(art);
            var (x, z) = WorldFrame.ToWorld(51.972, 5.343);
            Assert.IsTrue(art.TileAt(x, z, out int row, out int col));
            var (w, n) = art.Corner(row, col);
            Assert.That(x, Is.InRange(w, w + art.TileMetres));
            Assert.That(z, Is.InRange(n - art.TileMetres, n));
            for (int r = 0; r < art.Rows; r++)
                for (int c = 0; c < art.Cols; c++)
                    Assert.IsTrue(File.Exists(Path.Combine(Folder, $"t_{r}_{c}.png")), $"tile {r},{c} is missing");
        }

        [Test]
        public void WithoutAPyramid_ThereIsOneLevel_AndEveryZoomShowsIt()
        {
            string bare = Path.Combine(Path.GetTempPath(), "kapman_mapart_bare");
            Directory.CreateDirectory(bare);
            File.Copy(Path.Combine(Folder, "map_art.json"), Path.Combine(bare, "map_art.json"), true);
            var art = MapArt.Open(bare);
            Assert.IsNotNull(art);
            Assert.AreEqual(1, art.LevelCount);
            Assert.AreEqual(0, art.LevelFor(5.0));
            Assert.AreEqual(0, art.LevelFor(120.0));
            Assert.AreEqual(0, art.LevelFor(4000.0));
            Assert.IsTrue(art.Has(0, 3, 2));
            Assert.IsFalse(art.Has(1, 3, 2), "no finer level without pyramid.json");
        }

        /// <summary>The finer levels are baked on the machine (Bake/world/make_map_pyramid.py) and kept out of git: where
        /// they are not there, this is not a fault.</summary>
        [Test]
        public void ThePyramid_GivesFinerLevelsRoundDorestad_AndTheZoomPicksThem()
        {
            if (!File.Exists(Path.Combine(Folder, "pyramid.json")))
                Assert.Ignore("no pyramid.json: the finer levels have not been baked here");
            var art = MapArt.Open(Folder);
            Assert.IsNotNull(art);
            Assert.GreaterOrEqual(art.LevelCount, 3, "120 m, 60 m and 30 m at the least");
            for (int i = 1; i < art.LevelCount; i++)
                Assert.Less(art.LevelInfo(i).metresPerPixel, art.LevelInfo(i - 1).metresPerPixel, "each level finer than the last");
            // the zoom: the coarsest level whose pixel is no bigger than a screen pixel
            Assert.AreEqual(0, art.LevelFor(400.0));
            Assert.AreEqual(0, art.LevelFor(120.0));
            Assert.AreEqual(1, art.LevelFor(100.0));
            Assert.AreEqual(2, art.LevelFor(30.0));
            Assert.AreEqual(art.LevelCount - 1, art.LevelFor(1.0), "closer than the finest: the finest");
            // Dorestad has a tile at every level, and each is on disk where the level says it is
            var (x, z) = WorldFrame.ToWorld(51.972, 5.343);
            for (int level = 0; level < art.LevelCount; level++)
            {
                Assert.IsTrue(art.TileAt(level, x, z, out int row, out int col), "level " + level);
                Assert.IsTrue(art.Has(level, row, col), $"level {level} has no tile {row},{col} at Dorestad");
                var (w, n) = art.Corner(level, row, col);
                double size = art.TileMetresAt(level);
                Assert.That(x, Is.InRange(w, w + size));
                Assert.That(z, Is.InRange(n - size, n));
                string file = art.LevelInfo(level).pattern.Replace("{row}", row.ToString()).Replace("{col}", col.ToString());
                Assert.IsTrue(File.Exists(Path.Combine(Folder, file)), file + " is missing");
            }
            // the open North Sea has no 30 m tile: the chart shows the coarser level there
            var (sx, sz) = WorldFrame.ToWorld(56.5, 3.0);
            Assert.IsTrue(art.TileAt(2, sx, sz, out int srow, out int scol));
            Assert.IsFalse(art.Has(2, srow, scol), "the middle of the North Sea should be a sea tile with no file");
            Assert.AreNotEqual(MapArt.Key(1, 2, 3), MapArt.Key(2, 2, 3));
        }
    }
}
