// ported 4 Oct: THE LEVEL'S PLACE IN THE WORLD FRAME. The first build was one scene under a FloatingOrigin, and the game
// layer turns Unity positions into real metres through FloatingOrigin.ToWorld/ToUnity (60 call sites). The rebuild is a
// chain of level scenes with no FloatingOrigin of their own: a river place at true size, an ocean level shortened
// (LevelInfo.realMetresPerUnit), each a Unity terrain with its south-west corner at (0, 0) and its middle at the level's
// real place (LevelInfo.latitude/longitude). This gives the level a FloatingOrigin set so that the same calls keep
// working: the origin is the world position of Unity (0, 0, 0), the horizontal scale is the level's, heights are 1:1, and
// it never shifts (no focus). It agrees with the map's bridge (MapWorld) and the simulation's stage (SimStage.Frame).
using Kapman.Levels;
using Kapman.World;
using UnityEngine;

namespace Kapman.Game
{
    public static class LevelFrame
    {
        static bool warned;

        /// <summary>The FloatingOrigin is set for this level (Ensure has run with a LevelInfo).</summary>
        public static bool Ready { get; private set; }

        /// <summary>
        /// Makes sure the scene has a FloatingOrigin placed for its level, and returns it (null without a LevelInfo:
        /// the first build's scene or a test, where whatever FloatingOrigin there is stays as it is).
        /// </summary>
        public static FloatingOrigin Ensure()
        {
            var fo = Object.FindAnyObjectByType<FloatingOrigin>();
            var info = LevelInfo.Current;
            if (info == null)
            {
                if (!warned)
                {
                    warned = true;
                    Debug.LogWarning("Kapman: no LevelInfo in this scene, so the world frame is whatever FloatingOrigin there is (the game's world positions may be wrong).");
                }
                return fo;
            }
            if (fo == null)
                fo = new GameObject("World frame (the level's place in the world)").AddComponent<FloatingOrigin>();
            fo.focus = null;                                                  // never shifts: the terrain's corner stays at (0, 0)
            fo.shiftDistance = float.MaxValue;
            fo.horizontalScale = Mathf.Max(0.001f, info.realMetresPerUnit);
            fo.verticalScale = 1f;
            // OnEnable takes the scales (and clears the origin): run it again now the fields are set
            fo.enabled = false;
            fo.enabled = true;
            var (cx, cz) = WorldFrame.ToWorld(info.latitude, info.longitude);
            Vector3 c = MapWorld.LevelCentreUnity;                            // the terrain's middle (the Map owner's bridge)
            if (Terrain.activeTerrain == null)
                Debug.LogWarning("Kapman: the level has no active terrain, so its centre is taken as Unity (0, 0): world positions may be off by half the level.");
            double s = FloatingOrigin.HorizontalScale;
            FloatingOrigin.MoveOriginTo(cx - c.x * s, cz - c.z * s);
            Ready = true;
            return fo;
        }

        // play mode starts without a domain reload: the next level sets its own frame
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            Ready = false;
            warned = false;
        }
    }
}
