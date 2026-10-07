using Kapman.World;
using NUnit.Framework;

namespace Kapman.Tests
{
    /// <summary>
    /// The game's projection must put every place exactly where the bake (GDAL) does. The reference numbers
    /// come from Bake/tools/world_reference_points.py - re-run it and paste here if the world frame changes.
    /// </summary>
    public class WorldFrameTests
    {
        readonly struct Ref
        {
            public readonly string name;
            public readonly double lat, lon, x, z;

            public Ref(string name, double lat, double lon, double x, double z)
            {
                this.name = name;
                this.lat = lat;
                this.lon = lon;
                this.x = x;
                this.z = z;
            }
        }

        static readonly Ref[] Places =
        {
            new Ref("Centre of the projection", 56.0000, 5.0000, 0.000, 0.000),
            new Ref("Dorestad", 51.9750, 5.3450, 23386.396, -441329.382),
            new Ref("Emden", 53.3670, 7.2060, 144671.922, -286380.217),
            new Ref("Domburg", 51.5650, 3.4970, -102848.056, -485287.028),
            new Ref("Ribe", 55.3280, 8.7610, 234955.931, -67324.631),
            new Ref("Lundenwic", 51.5120, -0.1210, -350561.303, -479311.047),
            new Ref("Trondheim", 63.4300, 10.3900, 267532.927, 828504.688),
        };

        [Test]
        public void PlacesLandWhereTheBakePutsThem()
        {
            foreach (var p in Places)
            {
                var (x, z) = WorldFrame.ToWorld(p.lat, p.lon);
                Assert.AreEqual(p.x, x, 0.01, $"{p.name}: x");
                Assert.AreEqual(p.z, z, 0.01, $"{p.name}: z");
            }
        }

        [Test]
        public void LatLonComesBackTheSame()
        {
            foreach (var p in Places)
            {
                var (lat, lon) = WorldFrame.ToLatLon(p.x, p.z);
                Assert.AreEqual(p.lat, lat, 1e-7, $"{p.name}: latitude");
                Assert.AreEqual(p.lon, lon, 1e-7, $"{p.name}: longitude");
            }
        }
    }
}
