using System;
using Kapman.Levels;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game
{
    /// <summary>
    /// THE CHART'S PLACE IN THE WORLD (ported 4 Oct) - the bridge between a level's Unity metres and the chart's real
    /// metres (the world frame: x east, z north, metres from 56N 5E).
    /// In the first build the whole world was one scene under a FloatingOrigin, and FloatingOrigin.ToWorld turned any
    /// Unity position into real metres. The rebuild is a chain of level scenes: a river place at true size, an ocean
    /// level shortened hard (LevelInfo.realMetresPerUnit), each built as a Unity terrain whose south-west corner sits at
    /// Unity (0, 0) and whose centre is the level's real place (LevelInfo.latitude/longitude). So a Unity position is
    /// turned into real metres by: the level's centre in the world frame, plus the position's offset from the terrain's
    /// centre times the level's scale. With no LevelInfo in the scene (the first build's scene, a test) it falls back to
    /// FloatingOrigin as before.
    /// This is right within a level; between levels the chart jumps with the ship (the sea voyage is shortened). Where
    /// the chart needs the real distance along a course it reads the course's own figures, not these.
    /// </summary>
    public static class MapWorld
    {
        /// <summary>Real metres per Unity metre in the level now (1 in a river place, about 150 at sea).</summary>
        public static double MetresPerUnit
        {
            get
            {
                var info = LevelInfo.Current;
                return info != null ? Math.Max(0.001, info.realMetresPerUnit) : FloatingOrigin.HorizontalScale;
            }
        }

        /// <summary>The Unity position of the level's centre: the middle of its terrain (its south-west corner is at the origin).</summary>
        public static Vector3 LevelCentreUnity
        {
            get
            {
                var t = Terrain.activeTerrain;
                if (t == null || t.terrainData == null)
                    return Vector3.zero;
                var size = t.terrainData.size;
                return t.transform.position + new Vector3(size.x * 0.5f, 0f, size.z * 0.5f);
            }
        }

        /// <summary>The level's centre in the world frame (real metres), or the floating origin's where there is no level.</summary>
        public static (double x, double z) LevelCentreWorld()
        {
            var info = LevelInfo.Current;
            if (info == null)
                return (FloatingOrigin.Origin.x, FloatingOrigin.Origin.z);
            return WorldFrame.ToWorld(info.latitude, info.longitude);
        }

        /// <summary>Unity position (this level's metres) -> world position (real metres).</summary>
        public static WorldPos ToWorld(Vector3 u)
        {
            var info = LevelInfo.Current;
            if (info == null)
                return FloatingOrigin.ToWorld(u);
            var (cx, cz) = WorldFrame.ToWorld(info.latitude, info.longitude);
            var c = LevelCentreUnity;
            double s = MetresPerUnit;
            return new WorldPos(cx + (u.x - c.x) * s, u.y, cz + (u.z - c.z) * s);
        }

        /// <summary>World position (real metres) -> Unity position (this level's metres).</summary>
        public static Vector3 ToUnity(WorldPos w)
        {
            var info = LevelInfo.Current;
            if (info == null)
                return FloatingOrigin.ToUnity(w);
            var (cx, cz) = WorldFrame.ToWorld(info.latitude, info.longitude);
            var c = LevelCentreUnity;
            double s = MetresPerUnit;
            return new Vector3((float)((w.x - cx) / s + c.x), (float)w.y, (float)((w.z - cz) / s + c.z));
        }
    }
}
