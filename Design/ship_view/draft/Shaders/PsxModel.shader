// PROJECT PATH (later): Assets/Kapman/Shaders/ShipView/PsxModel.shader
// DRAFT, 7 Oct 2026. UNVERIFIED: written without Unity running, never compiled.
//
// THE SHIP, THE MEN AND THE DOCK IN THE CONSOLE'S STYLE. One texture read with hard texels (the pack's 32 x 32 palette),
// light worked out per vertex (Gouraud) or one shade per triangle (flat), vertices snapped to the picture's grid,
// textures stretched across the screen without perspective (affine), fog by distance. Each is a number set by
// ShipViewLook (see PsxCommon.hlsl), not a keyword: one material, every style.
//
// Two passes, the fewest HDRP will draw a solid object rightly with:
//   "DepthForwardOnly": into HDRP's depth prepass. Without it the object is missing from the depth HDRP's fog and
//       water read (they are made from the prepass), and the ship would be fogged as if she were the horizon behind her.
//       It writes depth only (ColorMask 0: the normal buffer bound there is left alone).
//   "ForwardOnly": the colour. ZTest LEqual, not Equal, so a rounding difference between the two passes cannot punch holes.
// No shadow pass (she casts none; the console had none), no motion vectors (the stage's camera has them off in the
// console styles: with them on HDRP leaves moving objects out of the prepass, see LOOK_AND_MOTION.md).
Shader "Kapman/ShipView/PSX Model"
{
    Properties
    {
        _BaseMap("Colours (the pack's palette)", 2D) = "white" {}
        _BaseColor("Tint", Color) = (1, 1, 1, 1)
        _DetailMap("Grain (a small tile you draw; optional)", 2D) = "gray" {}
        _DetailScale("Grain: tiles a metre", Float) = 1
        _DetailAmount("Grain: how strong", Range(0, 1)) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Which faces are left out (Off for a sail seen from both sides)", Float) = 2
    }

    SubShader
    {
        Tags { "RenderPipeline" = "HDRenderPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }

        HLSLINCLUDE
        #pragma target 4.5
        #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch
        #pragma multi_compile_instancing

        #include "PsxCommon.hlsl"

        TEXTURE2D(_BaseMap);
        TEXTURE2D(_DetailMap);

        CBUFFER_START(UnityPerMaterial)
        float4 _BaseColor;
        float _DetailScale;
        float _DetailAmount;
        CBUFFER_END

        struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float2 uv : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float2 uv : TEXCOORD0;
            noperspective float2 uvAffine : TEXCOORD1;      // the same uv, carried straight across the screen
            float2 grain : TEXCOORD2;
            noperspective float2 grainAffine : TEXCOORD3;
            float4 light : TEXCOORD4;                       // rgb the light, a the fog
            float3 positionRWS : TEXCOORD5;
        };

        Varyings Vert(Attributes v)
        {
            UNITY_SETUP_INSTANCE_ID(v);
            Varyings o;
            float3 ws = TransformObjectToWorld(v.positionOS);
            float3 n = TransformObjectToWorldNormal(v.normalOS);
            o.positionCS = PsxClip(ws);
            o.positionRWS = ws;
            o.uv = v.uv;
            o.uvAffine = v.uv;
            // the grain lies flat on whichever side the surface mostly faces, in the model's own space (it stays put on her)
            float3 a = abs(v.normalOS);
            float2 g = a.y > a.x && a.y > a.z ? v.positionOS.xz : (a.x > a.z ? v.positionOS.zy : v.positionOS.xy);
            o.grain = g * _DetailScale;
            o.grainAffine = o.grain;
            o.light = float4(PsxLight(n), PsxFogAmount(ws));
            return o;
        }

        float4 FragDepth(Varyings i) : SV_Target
        {
            return 0;
        }

        float4 FragColour(Varyings i) : SV_Target
        {
            float2 uv = lerp(i.uv, i.uvAffine, _PsxOptions.x);
            float3 albedo = SAMPLE_TEXTURE2D_LOD(_BaseMap, s_point_clamp_sampler, uv, 0).rgb * _BaseColor.rgb;
            if (_DetailAmount > 0.001)
            {
                float2 guv = lerp(i.grain, i.grainAffine, _PsxOptions.x);
                float grain = SAMPLE_TEXTURE2D_LOD(_DetailMap, psx_point_repeat_sampler, guv, 0).r * 2.0;
                albedo *= lerp(1.0, grain, _DetailAmount);
            }
            float3 light = i.light.rgb;
            if (_PsxOptions.y > 0.5)
                light = PsxLight(PsxFlatNormal(i.positionRWS));
            float3 col = lerp(albedo * light, _PsxFogColour.rgb, i.light.a);
            return float4(col * _PsxOptions.w, 1.0);
        }
        ENDHLSL

        Pass
        {
            Name "DepthForwardOnly"
            Tags { "LightMode" = "DepthForwardOnly" }
            Cull [_Cull]
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Stencil
            {
                WriteMask 8      // (as HDRP's Unlit: clears "trace a reflection ray here")
                Ref 0
                Comp Always
                Pass Replace
            }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragDepth
            ENDHLSL
        }

        Pass
        {
            Name "ForwardOnly"
            Tags { "LightMode" = "ForwardOnly" }
            Cull [_Cull]
            ZWrite On
            ZTest LEqual
            Stencil
            {
                WriteMask 3      // (as HDRP's Unlit: clears "lit by the deferred pass" and "subsurface")
                Ref 0
                Comp Always
                Pass Replace
            }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragColour
            ENDHLSL
        }
    }
    Fallback Off
}
