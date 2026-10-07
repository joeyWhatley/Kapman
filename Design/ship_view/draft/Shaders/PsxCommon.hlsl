// PROJECT PATH (later): Assets/Kapman/Shaders/ShipView/PsxCommon.hlsl
// DRAFT, 7 Oct 2026. UNVERIFIED: written without Unity running, never compiled. Kept short so a compile error is found fast.
//
// What the ship window's console-style shaders share: where a vertex lands (snapped to a coarse grid of the picture),
// the light on it (worked out per vertex, from numbers the game sets, not by HDRP's lighting), and the fog.
// Everything here is switched by numbers ShipViewLook sets as shader globals each frame, so one material serves every
// style and nothing has to be recompiled to change the look.
//
// HDRP facts this rests on (read in the package, not tried):
//   - ShaderVariables.hlsl brings SpaceTransforms.hlsl (through ShaderVariablesFunctions.hlsl): TransformObjectToWorld
//     gives a CAMERA-RELATIVE world position (UNITY_MATRIX_M has the camera's place taken off), so its length is the
//     distance to the eye; TransformWorldToHClip takes that position.
//   - GetCurrentExposureMultiplier() is NOT used: the colours written are final picture colours times _PsxOptions.w,
//     so the ship is as bright as the game's own light numbers say, whatever HDRP's exposure does.
#ifndef KAPMAN_PSX_COMMON_INCLUDED
#define KAPMAN_PSX_COMMON_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

float4 _PsxGrid;          // xy: how many steps across and down the picture a vertex may land on (0: no snapping)
float4 _PsxOptions;       // x: affine texturing 0..1   y: flat shading 0 or 1   z: depth steps (0: off)   w: white (the output's scale)
float4 _PsxSunDir;        // xyz: toward the light, world
float4 _PsxSunColour;     // rgb: the light's colour and strength (about 1 at full day)
float4 _PsxSkyColour;     // rgb: light from above
float4 _PsxGroundColour;  // rgb: light from below
float4 _PsxFogColour;     // rgb
float4 _PsxFog;           // x: where the fog starts (m)   y: where it is full (m)   z: how much at most 0..1

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
    if (_PsxOptions.z > 0.5 && cs.w > 0.0001)
    {
        // the console sorted whole polygons and had no depth buffer: near things fought. A hint of that: depth in steps.
        cs.z = round(cs.z / cs.w * _PsxOptions.z) / _PsxOptions.z * cs.w;
    }
    return cs;
}

// The light on a surface facing `n` (world): a light from above and one from below, and the sun or the moon.
float3 PsxLight(float3 n)
{
    float3 ambient = lerp(_PsxGroundColour.rgb, _PsxSkyColour.rgb, n.y * 0.5 + 0.5);
    return ambient + _PsxSunColour.rgb * saturate(dot(n, _PsxSunDir.xyz));
}

// How much fog lies between the eye and a point (camera-relative world position).
float PsxFogAmount(float3 positionRWS)
{
    float d = length(positionRWS);
    return saturate((d - _PsxFog.x) / max(_PsxFog.y - _PsxFog.x, 0.01)) * _PsxFog.z;
}

// One normal for the whole triangle, from how the position changes across the screen; turned to face the eye.
float3 PsxFlatNormal(float3 positionRWS)
{
    float3 n = normalize(cross(ddy(positionRWS), ddx(positionRWS)));
    return dot(n, positionRWS) > 0.0 ? -n : n;
}

#endif
