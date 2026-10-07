using UnityEngine;

namespace Kapman.Levels
{
    public enum LevelKind { RiverPlace, Ocean }

    /// <summary>
    /// THE LEVEL - what this scene is: a river place at true size (1:1) or an ocean level shortened hard, where it lies in
    /// the real world, and its water. One per level scene; the clock, the sky, the water and the ship read it.
    /// </summary>
    [ExecuteAlways]
    [DefaultExecutionOrder(-200)]
    public class LevelInfo : MonoBehaviour
    {
        public string levelId = "dorestad";
        public string displayName = "Dorestad";
        public LevelKind kind = LevelKind.RiverPlace;
        [Tooltip("The level's centre in the real world (degrees), for the sky, the sun and the tides.")]
        public double latitude = 51.98, longitude = 5.33;
        [Tooltip("The water's resting height in the world (m): the river's level, or the sea's mean level. The tide, the " +
                 "season and floods move the water from here (LevelWater).")]
        public float baseWaterLevel;
        [Tooltip("Real metres per game metre across the level: 1 in a river place, about 150 on the southern North Sea.")]
        public float realMetresPerUnit = 1f;
        [Tooltip("The bend of the world in an ocean level (m): the sea drops away so the horizon comes near. 0 = flat.")]
        public float curveRadius;
        [Tooltip("The player's ship's speed here (m/s): rowing in a river place, sailing on the sea.")]
        public float shipSpeed = 1.6f;

        /// <summary>The water's height now (m): the resting level plus the tide, the season's swell and any flood.</summary>
        public float WaterLevel { get; set; }

        static LevelInfo current;

        public static LevelInfo Current
        {
            get
            {
                if (current == null)
                    current = FindAnyObjectByType<LevelInfo>();
                return current;
            }
        }

        static readonly int CurveId = Shader.PropertyToID("_KapmanCurve");

        void OnEnable()
        {
            if (ChainRuntime.Allows(this))       // a level preloaded beside the current one leaves the static alone
                current = this;
            if (WaterLevel == 0f)
                WaterLevel = baseWaterLevel;
            Push();
        }

        void OnValidate()
        {
            WaterLevel = baseWaterLevel;
            Push();
        }

        void Update() => Push();

        void Push() => Shader.SetGlobalFloat(CurveId, curveRadius > 1f ? 0.5f / curveRadius : 0f);

        /// <summary>How far the bent world drops at a level distance from the camera (m).</summary>
        public float Drop(float distance) => curveRadius > 1f ? distance * distance * 0.5f / curveRadius : 0f;
    }
}
