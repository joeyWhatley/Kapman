using UnityEngine;

namespace Kapman.Game.ShipView
{
    /// <summary>
    /// THE PACK SHIP'S RAIL: its top along the starboard side, in the model's own metres (z towards the bow, x out from
    /// the keel, y over the model's origin). Measured from Models/SMV_Ship.fbx on 7 Oct 2026 (Design/ship_view/JOIN.md,
    /// E1). The port side mirrors. Times StageShip.Scale on the stage. The thwarts' width and the tholes' places follow
    /// it, so nothing of ours hangs outside her planking where she narrows.
    /// </summary>
    public static class ShipRail
    {
        static readonly float[,] Rail =
        {
            { -5.25f, 1.57f, 2.47f }, { -4.25f, 1.87f, 2.30f }, { -2.21f, 2.22f, 2.15f }, { -1.00f, 2.41f, 2.11f }, { 0f, 2.48f, 2.09f },
            { 1.00f, 2.41f, 2.12f }, { 2.21f, 2.22f, 2.15f }, { 4.30f, 1.87f, 2.30f }, { 5.25f, 1.57f, 2.47f },
        };

        /// <summary>The rail's half-breadth (x) and height (y) at a place along her (z), all in the model's metres.</summary>
        public static void At(float z, out float x, out float y)
        {
            int last = Rail.GetLength(0) - 1;
            z = Mathf.Clamp(z, Rail[0, 0], Rail[last, 0]);
            for (int i = 0; i < last; i++)
                if (z <= Rail[i + 1, 0])
                {
                    float t = Mathf.InverseLerp(Rail[i, 0], Rail[i + 1, 0], z);
                    x = Mathf.Lerp(Rail[i, 1], Rail[i + 1, 1], t);
                    y = Mathf.Lerp(Rail[i, 2], Rail[i + 1, 2], t);
                    return;
                }
            x = Rail[last, 1];
            y = Rail[last, 2];
        }
    }
}
