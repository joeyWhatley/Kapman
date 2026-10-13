// The asset itself: Assets/Kapman/ShipView/Resources/ShipStageKit.asset, made by Kapman > Ship view > Make the kit
// (Assets/Kapman/Editor/Game/ShipStageKitBuilder.cs).
using System;
using UnityEngine;

namespace Kapman.Game.ShipView
{
    /// <summary>Which sea the stage draws.</summary>
    public enum StageSeaKind { Mesh, HdrpWater }

    /// <summary>One of the pack's materials (by its name) and the HDRP one the stage puts in its place.</summary>
    [Serializable]
    public struct StageMaterialSwap
    {
        public string from;
        public Material to;
    }

    /// <summary>
    /// THE STAGE'S KIT: the prefabs, materials and measures the 3D ship view is built from at run time. One small asset
    /// in a Resources folder, so the stage needs no scene of its own and nothing in the Kapman scene (which ChartBuilder makes
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
        [Tooltip("The length she is SHOWN at (m), when it is not her class's: the pack's ship is a longship, and shrunk to the true 7.7 m of the player's boat her planks and ports are small beside full-size men. 0: her class's own length.")]
        public float shownLength = 0f;
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
        [Tooltip("A man pulls a pair of oars where her rail is no wider than this at his thwart (m); wider, he pulls one oar with both hands, sitting by his own side.")]
        public float pairBeam = 2.3f;
        [Tooltip("The crew: the pack's men, taken in turn (three at the oars and one at the steering oar for the Gislinge boat).")]
        public GameObject[] men = new GameObject[0];

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
        [Tooltip("Rain round the camera: lit, so it is dark at night (the look lab's Rain material: an unlit one glowed).")]
        public Material rain;
        [Tooltip("HDRP's own water exclusion material: drawn on a lid inside her rail, it keeps HDRP's sea from being drawn inside the hull.")]
        public Material waterExclusion;
        [Tooltip("The foam of her wake on HDRP's water: a water decal material (the game's own Assets/Kapman/Water/Ship_Foam.mat, which the 3D levels' ships use).")]
        public Material wakeFoam;
        [Tooltip("The star cube the 3D levels use (Assets/Kapman/Sky/Stars_930.exr).")]
        public Cubemap stars;

        [Header("The PSX looks (PsxStageLook; the shaders are held here so a build takes them with it)")]
        public Shader psxModel;
        public Shader psxSea, psxSprite, psxScreen;
        [Tooltip("The console sea's tile: small (32 x 32 or 64 x 64), grey, read with hard texels. None: a stand-in of short strokes is made in code. Draw your own and put it here.")]
        public Texture2D psxSeaTile;
        [Tooltip("The grain laid over the ship and the jetty in the console look: small, grey. None: a stand-in of plank lines is made in code. Draw your own and put it here.")]
        public Texture2D psxGrain;
        [Tooltip("The look the view starts in: plain (none), console, lowpoly, screen, ink (PsxStyles).")]
        public string startLook = "plain";

        [Header("The sea")]
        [Tooltip("HdrpWater: HDRP's own water surface, the sea of the look lab (KapmanSail), fitted to the sea state. Mesh: the stage's own mesh moved by eight waves (what the PSX looks draw on, and the fallback where HDRP's water is not supported).")]
        public StageSeaKind sea = StageSeaKind.HdrpWater;

        [Header("The stage")]
        [Tooltip("The layer everything on the stage is on (31 needs no name). The stage's camera draws this layer only and takes volumes from it only.")]
        [Range(8, 31)] public int layer = 31;
        [Tooltip("Where the stage stands in the scene: outside the chart's sailing world (which lies within 30,000 of the origin).")]
        public Vector3 origin = new Vector3(0f, 0f, -50000f);
        [Tooltip("The picture's height in pixels in the frame's window, and when it fills the screen (0: the screen's own).")]
        public int smallHeight = 270, bigHeight = 1080;
        [Tooltip("The small window is drawn every this many frames (2: thirty times a second at sixty).")]
        [Range(1, 4)] public int smallEvery = 2;
        [Tooltip("And never more often than this many times a real second, in the small window and filling the screen (0: no such cap). On the frames between, the stage does not step either.")]
        public float smallRate = 30f, bigRate = 60f;
        [Tooltip("The same while she rides HDRP's own waves (a sea of 0.3 m and more: StageHdrpSea.rideAbove). HDRP then copies its waves on the processor for every picture, about 8 ms each on the machine it was measured on, so fewer pictures are drawn.")]
        public float smallRateRiding = 20f, bigRateRiding = 30f;

        [Header("What is seen (the user, 7 Oct 2026: \"you cant see anything other than the boat and the water and just the port no land\")")]
        [Tooltip("Off: no ground anywhere. The jetty stands in the water by itself, with no bank behind it. On: the first build's green bank behind the jetty.")]
        public bool land = false;
        [Tooltip("The farthest one ever sees on the stage, metres (0: as far as the weather lets one). Mist closes the distance, so there is no bare line of horizon to show that nothing is there.")]
        public float mist = 900f;
        [Tooltip("How high the mist lies, metres: low, so the sky and its clouds still show over it.")]
        public float mistTop = 70f;
        [Tooltip("Without land the jetty is a pier this long, metres: long enough that both its ends lie outside the picture.")]
        public float pierLength = 48f;

        [Tooltip("Where the camera stands and looks, in her lengths: its height over the still water, and the height on her it is aimed at.")]
        public float eyeHeight = 0.34f, aimHeight = 0.2f;

        [Header("Light, in real units (as the level clock's)")]
        public float sunLux = 130000f;
        [Tooltip("The Moon's light over its true lux (the look lab's 1.5).")]
        public float moonBoost = 1.5f;
        [Tooltip("The darkest and brightest the fixed exposure goes (EV100).")]
        public float exposureFloor = -8f, exposureCeiling = 15.5f;
        [Tooltip("Stops added to the lighting model's exposure under a full Sun (less as less of the light is the Sun's own: none in fog or under a grey sky). Negative is brighter: the model exposes for a grey card in the Sun, and a boat on dark water wants more.")]
        public float exposureBias = -1.8f;
        [Tooltip("Stops darker than full adaptation deep in the night (the look lab's nightDarkness: the eye never brings a moonlit sea up to daylight).")]
        public float nightDarkness = 1.6f;

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
