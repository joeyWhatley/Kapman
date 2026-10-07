using UnityEngine;

namespace Kapman.Game.Sim
{
    /// <summary>
    /// Hulls only for show, made from the player's boat (the Gislinge model): no physics and no scripts but the water
    /// excluder (which keeps the sea out of the hull). Made under a switched-off holder, so none of the boat's own scripts
    /// wake first (her wake would take the water's only decal anchor from the player's ship). Their rig can be set: sail
    /// set, sail furled (moored), or no mast at all (logboats, punts, barges).
    /// </summary>
    public static class ShipBodies
    {
        public enum Rig { Sailing, Furled, NoMast }

        public static GameObject Make(GameObject prefab, Transform parent, string name)
        {
            var holder = new GameObject("(making a ship for show)");
            holder.SetActive(false);
            var go = Object.Instantiate(prefab, holder.transform);
            go.name = name;
            foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>(true))
                if (mb.GetType().Name != "WaterExcluder")
                    Object.DestroyImmediate(mb);
            foreach (var c in go.GetComponentsInChildren<Collider>(true))
                Object.DestroyImmediate(c);
            foreach (var rb in go.GetComponentsInChildren<Rigidbody>(true))
                Object.DestroyImmediate(rb);
            go.transform.SetParent(parent, false);
            Object.Destroy(holder);
            return go;
        }

        /// <summary>Sets the hull's rig: the sail set, furled (taken in at a mooring), or no mast at all.</summary>
        public static void SetRig(GameObject go, Rig rig)
        {
            foreach (var r in go.GetComponentsInChildren<Renderer>(true))
            {
                string n = r.gameObject.name;
                bool sail = n == "Sail Full" || n == "Sail Ropes";
                bool mast = sail || n == "Mast" || n == "Yard" || n == "Shrouds" || n == "Stay" || n.StartsWith("Yard Rope") || n.StartsWith("Rigging Pin");
                r.enabled = rig == Rig.Sailing || (rig == Rig.Furled && !sail) || (rig == Rig.NoMast && !mast);
            }
        }
    }
}
