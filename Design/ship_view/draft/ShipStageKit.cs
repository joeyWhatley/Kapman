// PATH IN THE PROJECT: Assets/Kapman/Code/Game/ShipView/ShipStageKit.cs   (assembly Kapman.Game)
// The asset itself: Assets/Kapman/Resources/ShipStageKit.asset, made by Kapman > Ship view > Make the kit
// (Assets/Kapman/Editor/Game/ShipStageKitBuilder.cs).
// DRAFT of 7 Oct 2026. Compile-checked outside Unity against Kapman.Game.rsp; never run.
using System;
using UnityEngine;

namespace Kapman.Game.ShipView
{
    /// <summary>One of the pack's materials (by its name) and the HDRP one the stage puts in its place.</summary>
    [Serializable]
    public struct StageMaterialSwap
    {
        public string from;
        public Material to;
    }

    /// <summary>
    /// THE STAGE'S KIT: the prefabs, materials and measures the 3D ship view is built from at run time. One small asset
    /// in a Resources folder, so the stage needs no scene of its own and nothing in L_Chart (which ChartBuilder makes
    /// afresh): ShipStageHook loads it by name. Every prefab may be missing (the pack is bought and not in git): the
    /// stage then stands a plain box or post in its place, so a checkout without the pack still runs.
    /// The measures are the pack's own, read from its files on 7 Oct 2026 (Models/SMV_Ship.fbx and SMV_Environment_1.fbx
    /// are in centimetres, imported at 0.01): see Design/ship_view/PLAN.md, parts 4 and 5.
    /// </summary>
    [CreateAssetMenu(menuName = "Kapman/Ship stage kit", fileName = "ShipStageKit")]
    public class ShipStageKit : ScriptableObject
    {
        public const string ResourceName = "ShipStageKit";

        [Header("The ship: the pack's SMV_Ship (a longship 17.88 m by 5.01 m), shown at her class's length")]
        public GameObject ship;
        [Tooltip("The hull mesh's length and beam (m), how far its keel lies under the model's origin, and the heights of its deck and of its rail's top amidships over the origin.")]
        public float modelLength = 17.88f, modelBeam = 5.01f, keelBelowOrigin = 0.41f, deckHeight = 1.45f, gunwaleHeight = 2.09f;
        [Tooltip("The water the model was made to float in, over its origin (m): the pack's own Demo scene has Water_1 at y 1.22 and its ships at 0.024. She is floated on this line, scaled: the hull under it was never meant to be seen.")]
        public float modelWaterline = 1.2f;
        [Tooltip("On: the model is scaled to her class's length (0.43 for the 7.7 m boat), and the men and oars are full size. Off: the model at her own 17.88 m whatever the class.")]
        public bool scaleToClass = true;
        [Tooltip("On: the stage fits her out itself (a thwart and a pair of oars for every two oars of her class). Off: the ship prefab brings its own.")]
        public bool fitOut = true;
        [Tooltip("Where the mast stands along the model (m ahead of its origin): the sail is braced round this.")]
        public float mastAhead = 1.15f;
        [Tooltip("Children of the ship prefab that are switched off: a longship's shields, her seventeen oars and thirteen benches, and the deck's clutter, all of which would be half size on a 7.7 m boat.")]
        public string[] hide =
        {
            "Ship_Shields_L", "Ship_Shields_R", "Ship_Shiled_Holder", "Ship_Oars_L", "Ship_Oars_R", "Benches_R", "Benches_R (1)",
            "WeaponRack_1", "Cup_1", "Cup_2", "Cup_3", "Bedroll_1", "Sack_1", "Sack_2", "Sack_3", "Barrel_1", "Crate_1", "Bucket_2", "Rope_1",
        };
        [Tooltip("An oar at full size (the pack's Oar_1: 2.65 m, its origin at the oar port, 0.47 m of loom inboard), and a thwart (Plank_4: 1.15 m).")]
        public GameObject oar, thwart;
        public float oarOutboard = 2.18f, thwartLength = 1.15f;

        [Header("The jetty, from the pack's props")]
        [Tooltip("Plank_2 (2.12 x 0.10 x 0.47 m): a deck board, laid across.")]
        public GameObject deckPlank;
        public Vector3 deckPlankSize = new Vector3(2.12f, 0.098f, 0.469f);
        [Tooltip("Plank_3 (3.09 x 0.11 x 0.56 m): a stringer, on edge under the boards.")]
        public GameObject stringer;
        public Vector3 stringerSize = new Vector3(3.09f, 0.108f, 0.562f);
        [Tooltip("Short_log_1 (0.33 m across, 0.23 m high): a pile, stretched to length.")]
        public GameObject pile;
        public Vector2 pileSize = new Vector2(0.33f, 0.234f);
        public GameObject barrel, crate, sackA, sackB, ropeCoil, bucket;

        [Header("Materials (HDRP; the pack's own are URP and draw pink here)")]
        public StageMaterialSwap[] swaps = new StageMaterialSwap[0];
        public Material water, shore, wake, line;
        [Tooltip("The star cube the 3D levels use (Assets/Kapman/Sky/Stars_930.exr).")]
        public Cubemap stars;

        [Header("The stage")]
        [Tooltip("The layer everything on the stage is on (31 needs no name). The stage's camera draws this layer only and takes volumes from it only.")]
        [Range(8, 31)] public int layer = 31;
        [Tooltip("Where the stage stands in the scene: outside the chart's sailing world (which lies within 30,000 of the origin).")]
        public Vector3 origin = new Vector3(0f, 0f, -50000f);
        [Tooltip("The picture's height in pixels in the frame's window, and when it fills the screen (0: the screen's own).")]
        public int smallHeight = 270, bigHeight = 540;
        [Tooltip("The small window is drawn every this many frames (2: thirty times a second at sixty).")]
        [Range(1, 4)] public int smallEvery = 2;

        [Header("Light, in real units (as the level clock's)")]
        public float sunLux = 130000f;
        public float moonBoost = 3f;
        [Tooltip("The darkest and brightest the fixed exposure goes (EV100), and what is added to the model's value.")]
        public float exposureFloor = -5f, exposureCeiling = 15.5f, exposureBias = 0f;

        /// <summary>The HDRP material for one of the pack's, or null when the kit has none for it.</summary>
        public Material Swap(Material from)
        {
            if (from == null || swaps == null)
                return null;
            for (int i = 0; i < swaps.Length; i++)
                if (swaps[i].to != null && swaps[i].from == from.name)
                    return swaps[i].to;
            return null;
        }

        public static ShipStageKit Load() => Resources.Load<ShipStageKit>(ResourceName);
    }
}
