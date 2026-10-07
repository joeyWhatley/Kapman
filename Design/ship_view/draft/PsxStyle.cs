// PROJECT PATH (later): Assets/Kapman/Code/Game/ShipView/PsxStyle.cs      assembly: Kapman.Game
// DRAFT, 7 Oct 2026. Compile-checked outside Unity (not run).
using System;
using UnityEngine;

namespace Kapman.Game.ShipView
{
    /// <summary>How the ship and the dock are shaded.</summary>
    public enum PsxShade
    {
        /// <summary>HDRP's own lit materials (the pack's palette on HDRP/Lit).</summary>
        Hdrp,
        /// <summary>The console's way: the light worked out at each vertex and blended across the triangle.</summary>
        Vertex,
        /// <summary>One shade for each triangle.</summary>
        Flat,
    }

    /// <summary>Which sea is drawn.</summary>
    public enum PsxSea
    {
        /// <summary>HDRP's water surface, as the 3D levels have it.</summary>
        Hdrp,
        /// <summary>A coarse grid of triangles moved by the same waves (PsxSeaGrid).</summary>
        Grid,
    }

    /// <summary>
    /// ONE WAY OF DRAWING THE SHIP WINDOW, as data: every part of the 1997 console's look is a number here that can be
    /// changed on its own (the user, 7 Oct 2026: "create a few different shaders, make it psx style in a few different
    /// ways"). ShipViewLook applies it. Edit the numbers below, or add a style: nothing else has to change.
    /// </summary>
    [Serializable]
    public sealed class PsxStyle
    {
        public string id, name;
        [TextArea] public string says;

        [Header("The picture")]
        [Tooltip("Lines of the picture (240: the console's). 0: as many as the window has, a pixel for a pixel.")]
        public int lines;
        [Tooltip("Each picture pixel is at least this many screen pixels wide (so the small window still shows hard pixels).")]
        public int minPixel = 1;
        [Tooltip("Pictures a second (0: every frame). The console ran at 30 or less; holding a picture also halves the cost.")]
        public int framesPerSecond;

        [Header("The colours")]
        [Tooltip("Levels of each colour channel: 32 is the console's 15-bit colour, 256 leaves the colours alone.")]
        public int levels = 256;
        [Range(0f, 1f)] [Tooltip("How strong the 4 x 4 dither pattern is (1: the console's).")]
        public float dither;
        [Tooltip("Tones of one ramp, by brightness (0: off; 2 to 8). The ramp is ShipViewLook's (the frame's colours).")]
        public int tones;

        [Header("The models")]
        public PsxShade shade = PsxShade.Hdrp;
        [Tooltip("Vertices land on a grid of every so many picture pixels (0: off, 1: whole pixels, 2: every other pixel).")]
        public float snap;
        [Range(0f, 1f)] [Tooltip("Textures stretched across the screen without perspective (1: the console's).")]
        public float affine;
        [Tooltip("Depth in so many steps, so near surfaces fight as the console's sorted polygons did (0: off; 4096 is mild).")]
        public float depthSteps;

        [Header("The sea and the air")]
        public PsxSea sea = PsxSea.Hdrp;
        [Tooltip("The grid sea: metres to a cell.")]
        public float seaCell = 2.5f;
        [Tooltip("The console styles' own fog: where it starts and where it is full, as shares of how far one can see; and the most it reaches (0: off).")]
        public float fogStart = 0.25f, fogEnd = 1f, fogMost;
        [Tooltip("Metres drawn at most (0: the camera's own).")]
        public float drawDistance;
        [Tooltip("HDRP's bloom, lens and film effects on the picture (tone mapping stays in every style).")]
        public bool lensEffects;
        [Tooltip("HDRP's temporal anti-aliasing (only for the plain style: it smears hard pixels).")]
        public bool smoothEdges;
        [Tooltip("The output's scale for the console shaders (1: white is white).")]
        public float white = 1f;
    }

    /// <summary>The styles the user can flip between (ShipViewLook.Next).</summary>
    public static class PsxStyles
    {
        public static readonly PsxStyle[] All =
        {
            new PsxStyle
            {
                id = "console", name = "The honest console",
                says = "Everything on: 240 lines with hard pixels, vertices on whole pixels so she shivers, textures without perspective, light per vertex, 15-bit colour with the console's dither, a sea of big triangles, fog close in, 30 pictures a second.",
                lines = 240, minPixel = 2, framesPerSecond = 30, levels = 32, dither = 1f,
                shade = PsxShade.Vertex, snap = 1f, affine = 1f, depthSteps = 0f,
                sea = PsxSea.Grid, seaCell = 3f, fogStart = 0.15f, fogEnd = 0.8f, fogMost = 1f, drawDistance = 400f,
            },
            new PsxStyle
            {
                id = "lowpoly", name = "The clean low-poly",
                says = "Hard pixels and flat colours, nothing wobbles: one shade to a triangle on the ship and on a sea of facets, full colour, no dither.",
                lines = 360, minPixel = 2, framesPerSecond = 0, levels = 256, dither = 0f,
                shade = PsxShade.Flat, snap = 0f, affine = 0f,
                sea = PsxSea.Grid, seaCell = 2.5f, fogStart = 0.3f, fogEnd = 1f, fogMost = 1f, drawDistance = 800f,
            },
            new PsxStyle
            {
                id = "screen", name = "A modern sea behind a console screen",
                says = "HDRP's own water, sky and lit ship, shown at 240 lines in 15-bit colour with the dither: today's sea on yesterday's screen.",
                lines = 240, minPixel = 2, framesPerSecond = 0, levels = 32, dither = 1f,
                shade = PsxShade.Hdrp, snap = 0f, affine = 0f,
                sea = PsxSea.Hdrp, fogMost = 0f, drawDistance = 0f, lensEffects = false,
            },
            new PsxStyle
            {
                id = "ink", name = "The dithered ink",
                says = "Four tones of the frame's own colours, laid in a heavy dither: the ship as a print. Flat shading and the sea of facets under it, because big plain shapes are what a dither draws well.",
                lines = 240, minPixel = 2, framesPerSecond = 30, levels = 256, dither = 1f, tones = 4,
                shade = PsxShade.Flat, snap = 1f, affine = 0f,
                sea = PsxSea.Grid, seaCell = 3f, fogStart = 0.2f, fogEnd = 0.9f, fogMost = 1f, drawDistance = 500f,
            },
            new PsxStyle
            {
                id = "plain", name = "None",
                says = "HDRP as it is, at the window's full size: to compare the others against.",
                lines = 0, minPixel = 1, framesPerSecond = 0, levels = 256, dither = 0f,
                shade = PsxShade.Hdrp, sea = PsxSea.Hdrp, lensEffects = true, smoothEdges = true,
            },
        };

        public static int Index(string id)
        {
            for (int i = 0; i < All.Length; i++)
                if (All[i].id == id)
                    return i;
            return 0;
        }
    }
}
