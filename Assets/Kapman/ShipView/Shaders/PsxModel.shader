// THE SHIP, THE MEN AND THE JETTY IN THE CONSOLE'S STYLE. One texture read with hard texels (the pack's small palette),
// light worked out per vertex (Gouraud) or one shade per triangle (flat), vertices snapped to the picture's grid,
// textures carried across the screen without perspective (affine), far things broken up into the sky. Each is a number
// set by PsxStageLook (see PsxCommon.hlsl), not a keyword: one material, every look.
// The pack's models have no picture on their faces (each face sits in one swatch of a palette), so there is nothing on
// them for the affine stretch to pull about. A small GRAIN tile is laid over them for that (a stand-in made in code
// until the user draws one): flat on whichever side a face mostly looks to, in the model's own space.
//
// Two passes, the fewest HDRP will draw a solid object rightly with:
//   "DepthForwardOnly": into HDRP's depth prepass. Without it the object is missing from the depth HDRP's fog and
//       water read. It writes depth only (ColorMask 0: the normal buffer bound there is left alone).
//   "ForwardOnly": the colour. ZTest LEqual, not Equal, so a rounding difference between the two passes cannot punch holes.
// No shadow pass (she casts none; the console had none), no motion vectors (the stage's camera has them off: with
// them on HDRP leaves moving objects out of the prepass).
Shader "Kapman/ShipView/PSX Model"
{
    Properties
    {
        _BaseMap("Colours (the pack's palette)", 2D) = "white" {}
        _BaseColor("Tint", Color) = (1, 1, 1, 1)
        _DetailMap("Grain (a small grey tile; you may draw your own)", 2D) = "gray" {}
        _DetailScale("Grain: tiles a metre", Float) = 2.5
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
        float _Cull;
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
            float2 grain : TEXCOORD1;
            noperspective float2 grainAffine : TEXCOORD2;   // the same, carried straight across the screen
            float3 light : TEXCOORD3;
            float3 positionRWS : TEXCOORD4;
        };

        // the same places as Vert and none of its light, for the depth pass
        Varyings VertDepth(Attributes v)
        {
            UNITY_SETUP_INSTANCE_ID(v);
            Varyings o = (Varyings)0;
            float3 ws = TransformObjectToWorld(v.positionOS);
            o.positionCS = PsxClip(ws);
            o.positionRWS = ws;
            return o;
        }

        Varyings Vert(Attributes v)
        {
            UNITY_SETUP_INSTANCE_ID(v);
            Varyings o;
            float3 ws = TransformObjectToWorld(v.positionOS);
            float3 n = TransformObjectToWorldNormal(v.normalOS);
            o.positionCS = PsxClip(ws);
            o.positionRWS = ws;
            o.uv = v.uv;
            // the grain lies flat on whichever side the surface mostly faces, in the model's own space (it stays put on
            // her as she rolls), in true metres whatever the model's own unit and scale are, its seams running level:
            // the model's axis that stands most upright is found from its matrix (a pack's meshes are not all y-up)
            float3x3 m = (float3x3)UNITY_MATRIX_M;
            float3 ax = float3(m._m00, m._m10, m._m20), ay = float3(m._m01, m._m11, m._m21), az = float3(m._m02, m._m12, m._m22);
            float3 size = max(float3(length(ax), length(ay), length(az)), 1e-5);
            float3 p = v.positionOS * size;
            float3 upness = abs(float3(ax.y, ay.y, az.y)) / size;
            int up = upness.x > upness.y ? (upness.x > upness.z ? 0 : 2) : (upness.y > upness.z ? 1 : 2);
            int h1 = up == 0 ? 1 : 0, h2 = up == 2 ? 1 : 2;
            float3 a = abs(v.normalOS / size);
            float2 g = a[up] > a[h1] && a[up] > a[h2] ? float2(p[h1], p[h2]) : (a[h1] > a[h2] ? float2(p[h2], p[up]) : float2(p[h1], p[up]));
            o.grain = g * _DetailScale;
            o.grainAffine = o.grain;
            o.light = PsxLight(n);
            return o;
        }

        float4 FragDepth(Varyings i) : SV_Target
        {
            PsxFarFade(i.positionCS, i.positionRWS);
            return 0;
        }

        float4 FragColour(Varyings i) : SV_Target
        {
            PsxFarFade(i.positionCS, i.positionRWS);
            // (the palette is never stretched: a swatch is a few texels, and an affine uv would wander into its neighbours)
            float3 albedo = SAMPLE_TEXTURE2D_LOD(_BaseMap, s_point_clamp_sampler, i.uv, 0).rgb * _BaseColor.rgb;
            float amount = _DetailAmount * _PsxOptions.z;
            if (amount > 0.001)
            {
                float2 guv = lerp(i.grain, i.grainAffine, _PsxOptions.x);
                float grain = SAMPLE_TEXTURE2D_LOD(_DetailMap, psx_point_repeat_sampler, guv, 0).r * 2.0;
                albedo *= lerp(1.0, grain, amount);
            }
            float3 light = i.light;
            if (_PsxOptions.y > 0.5)
                light = PsxLight(PsxFlatNormal(i.positionRWS));
            return float4(albedo * light, 1.0);
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
            #pragma vertex VertDepth
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
