using System;

namespace Kapman.World
{
    /// <summary>
    /// A place in the world, in double precision: x = east, y = up (height above sea level), z = north, in metres.
    /// Unity's own positions are floats, which are only precise near (0,0,0); world positions are kept in
    /// these and turned into Unity positions through <see cref="FloatingOrigin"/>.
    /// </summary>
    [Serializable]
    public struct WorldPos
    {
        public double x, y, z;

        public WorldPos(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        /// <summary>The world position of a latitude/longitude (degrees), at a height in metres.</summary>
        public static WorldPos FromLatLon(double lat, double lon, double height = 0.0)
        {
            var (x, z) = WorldFrame.ToWorld(lat, lon);
            return new WorldPos(x, height, z);
        }

        /// <summary>Latitude and longitude (degrees) of this position.</summary>
        public (double lat, double lon) LatLon => WorldFrame.ToLatLon(x, z);

        public override string ToString() => $"(x {x:F1}, y {y:F1}, z {z:F1})";
    }
}
