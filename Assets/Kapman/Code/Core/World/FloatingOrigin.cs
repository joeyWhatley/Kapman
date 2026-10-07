using System;
using UnityEngine;

namespace Kapman.World
{
    /// <summary>
    /// FLOATING ORIGIN - keeps the camera close to Unity's (0,0,0), where floats are precise, and turns real-world
    /// positions (metres in the world frame) into game positions.
    ///
    /// THE GAME SCALE: the game world is the real world made smaller - distances divided by
    /// <see cref="horizontalScale"/> (32: 1 real km = 31 game metres, so Dorestad to London is 13 km, about
    /// 1.5 hours at a real ship's speed) and heights by <see cref="verticalScale"/> (4, so dunes and cliffs still
    /// read next to a real-size ship). Ships, waves and people are real size in game metres.
    /// Everything that places things in the world goes through <see cref="ToUnity"/> and <see cref="ToWorld"/>.
    ///
    /// When the focus (the camera) strays more than <see cref="shiftDistance"/> game metres from the origin, the
    /// origin jumps to it and everything pinned with a <see cref="WorldAnchor"/> moves to match. Things that move
    /// about by themselves (ships, their courses, the ship camera's aim) listen to <see cref="Shifted"/> and move by
    /// <see cref="LastShift"/>, so they stay where they are in the world.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class FloatingOrigin : MonoBehaviour
    {
        [Tooltip("What to keep near the origin: the camera.")]
        public Transform focus;

        [Tooltip("Game metres the focus may stray from the origin before the world is shifted back to it.")]
        public float shiftDistance = 2000f;

        [Header("Game scale")]
        [Tooltip("Real metres per game metre, across (32: Dorestad-London takes ~1.5 h at a real ship's speed; 1 = life size).")]
        public float horizontalScale = 1f;
        [Tooltip("Real metres per game metre, up and down (4 keeps dunes and cliffs readable next to a real-size ship).")]
        public float verticalScale = 1f;

        /// <summary>The world position (real metres) of Unity's (0,0,0) right now.</summary>
        public static WorldPos Origin { get; private set; }

        /// <summary>Real metres per game metre, across and up.</summary>
        public static double HorizontalScale { get; private set; } = 1.0;
        public static double VerticalScale { get; private set; } = 1.0;

        /// <summary>Raised after every shift (the focus has already been moved).</summary>
        public static event Action Shifted;

        /// <summary>How far every Unity position moved in the latest shift (game metres): add it to anything kept in Unity coordinates.</summary>
        public static Vector3 LastShift { get; private set; }

        // Play mode starts without a domain reload, so static state from the last run must be cleared by hand
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearStatics()
        {
            Shifted = null;
            Origin = default;
            LastShift = Vector3.zero;
        }

        /// <summary>World position (real metres) -> Unity position (game metres).</summary>
        public static Vector3 ToUnity(WorldPos p) =>
            new Vector3((float)((p.x - Origin.x) / HorizontalScale), (float)(p.y / VerticalScale), (float)((p.z - Origin.z) / HorizontalScale));

        /// <summary>Unity position (game metres) -> world position (real metres).</summary>
        public static WorldPos ToWorld(Vector3 u) =>
            new WorldPos(Origin.x + u.x * HorizontalScale, u.y * VerticalScale, Origin.z + u.z * HorizontalScale);

        /// <summary>Puts the origin at a world x/z (real metres), e.g. just before placing the camera there.</summary>
        public static void MoveOriginTo(double x, double z)
        {
            var to = new WorldPos(Math.Round(x), 0.0, Math.Round(z));
            LastShift = new Vector3((float)((Origin.x - to.x) / HorizontalScale), 0f, (float)((Origin.z - to.z) / HorizontalScale));
            Origin = to;
            Shifted?.Invoke();
        }

        void OnEnable()
        {
            Origin = default;
            HorizontalScale = Math.Max(0.001, horizontalScale);
            VerticalScale = Math.Max(0.001, verticalScale);
        }

        void LateUpdate()
        {
            if (focus == null)
                return;
            Vector3 p = focus.position;
            if (p.x * p.x + p.z * p.z < shiftDistance * shiftDistance)
                return;
            double dx = Math.Round(p.x), dz = Math.Round(p.z);
            Origin = new WorldPos(Origin.x + dx * HorizontalScale, 0.0, Origin.z + dz * HorizontalScale);
            LastShift = new Vector3(-(float)dx, 0f, -(float)dz);
            focus.position += LastShift;
            Shifted?.Invoke();
        }
    }
}
