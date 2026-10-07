using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Kapman.Levels.Water
{
    /// <summary>
    /// WATER LINE - what stands in the water is wet. Below the water's surface it's darker and shines, and so is a band
    /// above it where the waves wash, fading out: a mooring post is wet to a little above the water line and dry above
    /// it, and so is the ship's planking, the piles of the dams and the mud of the foreshore. It's one HDRP decal box
    /// round the camera: its top is the water level (tide and flood included) plus the wash, and it reaches down a few
    /// metres. HDRP decals change the colour and gloss of whatever's inside, whichever way it faces. It follows the
    /// camera and the water, and the wash climbs higher in a wind. The water itself takes no decals (HDRP's water
    /// shader doesn't receive them), so the box never darkens the river. Brought from the first build.
    /// </summary>
    [DefaultExecutionOrder(1010)]
    public class WaterLine : MonoBehaviour
    {
        [Tooltip("An HDRP/Decal material: wet colour and gloss; its texture's alpha is opaque up to 90% of its height and clear at the top (HdrpWater makes it).")]
        public Material wet;
        [Tooltip("Metres the wet band reaches above the calm water: the first in a calm, plus the second for each m/s of wind.")]
        public Vector2 wash = new Vector2(0.05f, 0.012f);
        [Tooltip("Metres round the camera it covers for each metre the camera's above the water (between the limits).")]
        public float perHeight = 8f;
        public Vector2 sizeLimits = new Vector2(300f, 4000f);

        // the texture fades over its top tenth: the box is ten washes tall, so the fade is the wash, above the water
        const float FadeShare = 0.1f;

        DecalProjector box;

        void Start()
        {
            if (wet == null)
            {
                enabled = false;
                return;
            }
            var go = new GameObject("Water line (wet below the water)");
            go.transform.SetParent(transform, false);
            box = go.AddComponent<DecalProjector>();
            box.material = wet;
            box.pivot = Vector3.zero;
            box.fadeFactor = 1f;
        }

        void LateUpdate()
        {
            var cam = Camera.main;
            if (cam == null || box == null)
                return;
            var info = LevelInfo.Current;
            float water = info != null ? info.WaterLevel : 0f;
            var weather = LevelWeather.Current;
            float band = wash.x + wash.y * (weather != null ? weather.Wind : 3f);
            float tall = band / FadeShare;
            var eye = cam.transform.position;
            float across = Mathf.Clamp(Mathf.Max(0f, eye.y - water) * perHeight, sizeLimits.x, sizeLimits.y);
            // its x and y across the texture (y up the gradient), z the way it projects: every face inside is wetted by
            // its height alone, whichever way it faces
            box.transform.SetPositionAndRotation(new Vector3(eye.x, water + band - 0.5f * tall, eye.z), Quaternion.identity);
            box.size = new Vector3(across, tall, across);
            box.drawDistance = across + Mathf.Abs(eye.y - water) * 2f;
        }
    }
}
