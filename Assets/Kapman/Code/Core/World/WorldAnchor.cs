using UnityEngine;

namespace Kapman.World
{
    /// <summary>
    /// WORLD ANCHOR - pins a GameObject to a place in the world (double precision) and keeps it there when
    /// the <see cref="FloatingOrigin"/> shifts. Put it on anything that sits somewhere in the world.
    /// </summary>
    public class WorldAnchor : MonoBehaviour
    {
        [Tooltip("Where this object is: x = east, y = height above sea level, z = north (metres).")]
        public WorldPos position;

        void OnEnable()
        {
            FloatingOrigin.Shifted += Place;
            Place();
        }

        void OnDisable() => FloatingOrigin.Shifted -= Place;

        /// <summary>Moves the transform to <see cref="position"/> as seen from the current origin.</summary>
        public void Place() => transform.position = FloatingOrigin.ToUnity(position);
    }
}
