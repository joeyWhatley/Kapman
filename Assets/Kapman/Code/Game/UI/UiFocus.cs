using System.Collections.Generic;
using UnityEngine;

namespace Kapman
{
    /// <summary>
    /// UI FOCUS - whether a game panel (the text panel, the market, the map...) has the player's attention. While one
    /// does, the cameras and the ship's controls leave the keyboard and the mouse wheel alone: the number keys pick
    /// options, and the wheel scrolls lists (it also zooms). A panel takes the focus while it's open and gives it back
    /// when it closes.
    /// Ported 4 Oct from the first build's Code/UiFocus.cs (it lived in the runtime there; the rebuild's runtime has no
    /// such thing, so it comes over with the panels that use it). Same namespace as before (Kapman), so Rest and the
    /// cameras find it unchanged. The rebuild's OrbitCamera and ShipMotion don't read it yet: see the wiring notes.
    /// </summary>
    public static class UiFocus
    {
        static readonly HashSet<object> holders = new HashSet<object>();
        static readonly HashSet<object> clear = new HashSet<object>();         // holders that keep the scene sharp

        /// <summary>True while any panel holds the focus.</summary>
        public static bool Modal => holders.Count > 0;

        /// <summary>True while a panel that frosts the scene behind it holds the focus (the map's rise and fall don't:
        /// the camera's flight is to be seen).</summary>
        public static bool Frosted => holders.Count > clear.Count;

        public static void Take(object who, bool frost = true)
        {
            if (who == null)
                return;
            holders.Add(who);
            if (frost) clear.Remove(who);
            else clear.Add(who);
        }

        public static void Release(object who)
        {
            if (who == null)
                return;
            holders.Remove(who);
            clear.Remove(who);
        }

        // Enter Play Mode runs without a domain reload: start each play with nobody holding it
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            holders.Clear();
            clear.Clear();
        }
    }
}
