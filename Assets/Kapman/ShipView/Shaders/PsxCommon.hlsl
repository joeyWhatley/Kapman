// What the ship view's console-style shaders share: where a vertex lands (snapped to a coarse grid of the picture), the
// light on it (worked out per vertex or per triangle, from numbers the game sets, not by HDRP's light loop), and how
// the far things break up into the sky. Everything here is switched by numbers PsxStageLook sets as shader globals, so
// one material serves every look and nothing has to be recompiled to change it.
//
// HDRP facts this rests on (com.unity.render-pipelines.high-definition 17.6):
//   - ShaderVariables.hlsl brings SpaceTransforms.hlsl (through ShaderVariablesFunctions.hlsl): TransformObjectToWorld
//     gives a CAMERA-RELATIVE world position, so its length is the distance to the eye; TransformWorldToHClip takes it.
//   - The colour a pass writes is taken by HDRP as light times its exposure. The Sun's light set here is already
//     multiplied by the stage's exposure (it is a fixed one, known in C#). THE SKY'S LIGHT IS HDRP'S OWN: the ambient
//     probe it works out from its sky (clouds and the ground under it included), read here as its lit materials read it
//     (core's AmbientProbe.hlsl: SampleSH9 of _AmbientProbeData, which ShaderVariables.hlsl declares) and multiplied by
//     HDRP's own exposure. So these things are as bright against HDRP's sky as HDRP's lit materials would be, in sun
//     and in shade, by day and by night, with nothing to tune. (The first try gave the sky's light as a number from
//     the game's lighting model: a hull's shaded side came out a third as bright as HDRP's.)
//   - HDRP lays its own fog over these pixels as over any other (they are in the depth buffer).
#ifndef KAPMAN_PSX_COMMON_INCLUDED
#define KAPMAN_PSX_COMMON_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/AmbientProbe.hlsl"

float4 _PsxGrid;          // xy: how many steps across and down the picture a vertex may land on (0: no snapping)
float4 _PsxOptions;       // x: affine texturing 0..1   y: flat shading 0 or 1   z: grain on the models 0..1   w: the tile on the console sea 0..1
float4 _PsxSunDir;        // xyz: toward the light (the Sun, or the Moon by night), world
float4 _PsxSunColour;     // rgb: that light on a surface square to it, exposed
float4 _PsxFade;          // x: where far things begin to break up (m)   y: where they are gone (m)   z: 1 when on

// (HDRP declares s_point_clamp_sampler; there is no point-repeat one, so this is ours: Unity reads the filter and the
// wrap from the name)
SAMPLER(psx_point_repeat_sampler);

// The clip-space place of a vertex, snapped. The console had no sub-pixel precision: a vertex landed on a whole pixel,
// so edges crawl and whole models shiver as they move. _PsxGrid is the grid (the picture's size, or coarser).
float4 PsxClip(float3 positionRWS)
{
    float4 cs = TransformWorldToHClip(positionRWS);
    if (_PsxGrid.x > 0.5 && cs.w > 0.0001)
    {
        float2 steps = _PsxGrid.xy * 0.5;                  // (clip x and y run -1 .. 1)
        cs.xy = round(cs.xy / cs.w * steps) / steps * cs.w;
    }
    return cs;
}

// The light on a surface facing `n` (world): HDRP's own sky light from that side, and the Sun or the Moon.
// (Only for a colour pass: HDRP has laid out neither its sky's light nor its exposure when it draws depth.)
float3 PsxLight(float3 n)
{
    float3 ambient = max(EvaluateAmbientProbe(n), 0.0) * GetCurrentExposureMultiplier();
    return ambient + _PsxSunColour.rgb * saturate(dot(n, _PsxSunDir.xyz));
}

// One normal for the whole triangle, from how the position changes across the screen; turned to face the eye.
float3 PsxFlatNormal(float3 positionRWS)
{
    float3 n = normalize(cross(ddy(positionRWS), ddx(positionRWS)));
    return dot(n, positionRWS) > 0.0 ? -n : n;
}

// The console's short draw distance. It had no fog colour to match a sky with: far things were simply left out, and
// the better games broke them up first through a fixed 4 x 4 pattern so they did not pop. Done so here: between
// _PsxFade.x and _PsxFade.y metres a growing share of the pixels is left out, and the sky behind shows through.
// Called by the depth pass and the colour pass alike, so both leave out the same pixels.
static const float PsxPattern[16] = { 0.0, 8.0, 2.0, 10.0, 12.0, 4.0, 14.0, 6.0, 3.0, 11.0, 1.0, 9.0, 15.0, 7.0, 13.0, 5.0 };

void PsxFarFade(float4 positionSS, float3 positionRWS)
{
    if (_PsxFade.z > 0.5)
    {
        float gone = saturate((length(positionRWS) - _PsxFade.x) / max(_PsxFade.y - _PsxFade.x, 0.01));
        uint2 px = (uint2)positionSS.xy;
        float keep = (PsxPattern[(px.x & 3) + ((px.y & 3) << 2)] + 0.5) / 16.0;
        clip(keep - gone);
    }
}

#endif
