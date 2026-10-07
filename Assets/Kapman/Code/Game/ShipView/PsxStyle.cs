// From the draft of 7 Oct 2026 (Design/ship_view/draft/PsxStyle.cs); the numbers are as they stood after the looks were
// first drawn and looked at the same day (Design/ship_view/BUILD_LOG.md, part 2).
using System;
using UnityEngine;

namespace Kapman.Game.ShipView
{
    /// <summary>How the ship, the men and the jetty are shaded.</summary>
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
        /// <summary>The stage's own: HDRP's water surface (the modern sea).</summary>
        Hdrp,
        /// <summary>A coarse grid of triangles moved by the same waves (PsxSeaGrid).</summary>
        Grid,
    }

    /// <summary>
    /// ONE LOOK OF THE SHIP VIEW, as data: every part of the 1997 console's picture is a number here that can be
    /// changed on its own (the user, 7 Oct 2026: "create a few different shaders, make it psx style in a few different
    /// ways"). PsxStageLook applies it. Edit the numbers below, or add a look: nothing else has to change.
    /// </summary>
    [Serializable]
    public sealed class PsxStyle
    {
        public string id, name;
        [TextArea] public string says;

        [Header("The picture")]
        [Tooltip("Lines of the picture (240: the console's). 0: the stage's own picture, untouched.")]
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
        [Tooltip("Tones of one ramp, by brightness (0: off; 2 to 8). The ramp is the frame's colours.")]
        public int tones;

        [Header("The models")]
        public PsxShade shade = PsxShade.Hdrp;
        [Tooltip("Vertices land on a grid of every so many picture pixels (0: off, 1: whole pixels, 2: every other pixel).")]
        public float snap;
        [Range(0f, 1f)] [Tooltip("Textures carried across the screen without perspective (1: the console's).")]
        public float affine;
        [Range(0f, 1f)] [Tooltip("How much of the grain tile shows on the ship and the jetty (it is what the affine stretch pulls about there).")]
        public float grain;

        [Header("The sea and the far water")]
        public PsxSea sea = PsxSea.Hdrp;
        [Tooltip("The grid sea: metres to a cell.")]
        public float seaCell = 2.5f;
        [Range(0f, 1f)] [Tooltip("How much of the small tile shows on the grid sea (0: plain facets).")]
        public float seaTile;
        [Tooltip("How far each point of the grid sea bobs by itself in a light air (m): what makes facets in a calm.")]
        public float seaBob = 0.04f;
        [Tooltip("Metres drawn at most (0: all of it). Far things break up into the sky through a fixed pattern between the two shares of it below.")]
        public float drawDistance;
        public float fadeStart = 0.25f, fadeEnd = 0.9f;
    }

    /// <summary>The looks the user can flip between (PsxStageLook.Next).</summary>
    public static class PsxStyles
    {
        public static readonly PsxStyle[] All =
        {
            new PsxStyle
            {
                id = "plain", name = "None",
                says = "The stage's own picture: HDRP's water, sky and lit ship at the window's full size. To compare the others against.",
                lines = 0, minPixel = 1, framesPerSecond = 0, levels = 256, dither = 0f,
                shade = PsxShade.Hdrp, sea = PsxSea.Hdrp,
            },
            new PsxStyle
            {
                id = "console", name = "The honest console",
                says = "Everything on: 240 lines with hard pixels, vertices on whole pixels so she shivers, a grain that warps, light worked out at the corners, 15-bit colour with the console's dither, a sea of big triangles with a sliding tile, the far water breaking up, 30 pictures a second.",
                lines = 240, minPixel = 2, framesPerSecond = 30, levels = 32, dither = 1f,
                shade = PsxShade.Vertex, snap = 1f, affine = 1f, grain = 0.8f,
                sea = PsxSea.Grid, seaCell = 3f, seaTile = 0.9f, drawDistance = 260f, fadeStart = 0.18f, fadeEnd = 0.9f,
            },
            new PsxStyle
            {
                id = "lowpoly", name = "The clean low-poly",
                says = "Hard pixels and flat colours, nothing wobbles: one shade to a triangle on the ship and on a sea of facets, full colour, no dither.",
                lines = 360, minPixel = 2, framesPerSecond = 0, levels = 256, dither = 0f,
                shade = PsxShade.Flat, snap = 0f, affine = 0f, grain = 0f,
                sea = PsxSea.Grid, seaCell = 2.5f, seaBob = 0.14f, drawDistance = 900f, fadeStart = 0.35f, fadeEnd = 0.95f,
            },
            new PsxStyle
            {
                id = "screen", name = "A modern sea on a console screen",
                says = "HDRP's own water, sky and lit ship, shown at 240 lines in 15-bit colour with the dither: today's sea on yesterday's screen.",
                lines = 240, minPixel = 2, framesPerSecond = 0, levels = 32, dither = 1f,
                shade = PsxShade.Hdrp, snap = 0f, affine = 0f,
                sea = PsxSea.Hdrp, drawDistance = 0f,
            },
            new PsxStyle
            {
                id = "ink", name = "The dithered ink",
                says = "Four tones of the frame's own colours, laid in a heavy dither: the ship as a print. Flat shading and the sea of facets under it, because big plain shapes are what a dither draws well.",
                lines = 240, minPixel = 2, framesPerSecond = 30, levels = 256, dither = 1f, tones = 4,
                shade = PsxShade.Flat, snap = 1f, affine = 0f, grain = 0f,
                sea = PsxSea.Grid, seaCell = 3f, seaTile = 0.5f, seaBob = 0.1f, drawDistance = 500f, fadeStart = 0.25f, fadeEnd = 0.9f,
            },
            // the same print at other depths, each ramp built round Byzantium (the user, 7 Oct 2026: "1bit, 2bit, 3bit, 4Bit")
            new PsxStyle
            {
                id = "ink1", name = "Ink, 1 bit",
                says = "Two colours: Byzantium and paper, and a dither between them.",
                lines = 240, minPixel = 2, framesPerSecond = 30, levels = 256, dither = 1f, tones = 2,
                shade = PsxShade.Flat, snap = 1f, affine = 0f, grain = 0f,
                sea = PsxSea.Grid, seaCell = 3f, seaTile = 0.5f, seaBob = 0.1f, drawDistance = 500f, fadeStart = 0.25f, fadeEnd = 0.9f,
            },
            new PsxStyle
            {
                id = "ink3", name = "Ink, 3 bit",
                says = "Eight tones from near black through Byzantium to sand and paper.",
                lines = 240, minPixel = 2, framesPerSecond = 30, levels = 256, dither = 1f, tones = 8,
                shade = PsxShade.Flat, snap = 1f, affine = 0f, grain = 0f,
                sea = PsxSea.Grid, seaCell = 3f, seaTile = 0.5f, seaBob = 0.1f, drawDistance = 500f, fadeStart = 0.25f, fadeEnd = 0.9f,
            },
            new PsxStyle
            {
                id = "ink4", name = "Ink, 4 bit",
                says = "Sixteen tones round Byzantium, with gold near the top.",
                lines = 240, minPixel = 2, framesPerSecond = 30, levels = 256, dither = 1f, tones = 16,
                shade = PsxShade.Flat, snap = 1f, affine = 0f, grain = 0f,
                sea = PsxSea.Grid, seaCell = 3f, seaTile = 0.5f, seaBob = 0.1f, drawDistance = 500f, fadeStart = 0.25f, fadeEnd = 0.9f,
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
