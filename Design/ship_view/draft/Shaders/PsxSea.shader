// PROJECT PATH (later): Assets/Kapman/Shaders/ShipView/PsxSea.shader
// DRAFT, 7 Oct 2026. UNVERIFIED: written without Unity running, never compiled.
//
// THE SEA AS A 1997 CONSOLE DREW IT. A coarse grid of triangles (PsxSeaGrid.cs moves its vertices on the CPU with the
// very waves the ship rides, and colours each vertex: darker in the troughs, lighter on the crests, white where it
// breaks and in her wake). Over that, one small tile read twice with hard texels, the two sliding over each other,
// stretched without perspective; light per vertex or per triangle, a hard glint of the sun on the facets, fog.
// The mesh's vertices are world positions (its object sits at the origin, unturned).
Shader "Kapman/ShipView/PSX Sea"
{
    Properties
    {
        _SeaTex("Sea tile (small; the red channel is read)", 2D) = "gray" {}
        _SeaScale("Tiles a metre", Float) = 0.125
        _SeaDrift("Second layer: size, and metres a second it slides (x, z)", Vector) = (0.61, 0.35, -0.22, 0)
        _SeaTexAmount("How much the tile shows", Range(0, 1)) = 0.5
        _SeaGlint("Sun glint", Range(0, 2)) = 0.6
        _SeaGlintPower("Sun glint: how tight", Range(2, 64)) = 16
    }

    SubShader
    {
        Tags { "RenderPipeline" = "HDRenderPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }

        HLSLINCLUDE
        #pragma target 4.5
        #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

        #include "PsxCommon.hlsl"

        TEXTURE2D(_SeaTex);
        float4 _PsxSeaFlow;       // xy: how far the sea has run past the ship (m), set by PsxSeaGrid; z: its clock (s)

        CBUFFER_START(UnityPerMaterial)
        float4 _SeaDrift;
        float _SeaScale;
        float _SeaTexAmount;
        float _SeaGlint;
        float _SeaGlintPower;
        CBUFFER_END

        struct Attributes
        {
            float3 positionOS : POSITION;       // (a world position: see above)
            float3 normalOS : NORMAL;
            float4 colour : COLOR;              // rgb the water's own colour here, a how much of the tile shows (0 far off)
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float4 uv : TEXCOORD0;
            noperspective float4 uvAffine : TEXCOORD1;
            float4 light : TEXCOORD2;           // rgb the light, a the fog
            float3 positionRWS : TEXCOORD3;
            float4 colour : COLOR0;
            float glint : TEXCOORD4;
        };

        float Glint(float3 n, float3 positionRWS)
        {
            float3 toEye = normalize(-positionRWS);
            float3 h = normalize(toEye + _PsxSunDir.xyz);
            return pow(saturate(dot(n, h)), _SeaGlintPower) * _SeaGlint * step(0.0, _PsxSunDir.y);
        }

        Varyings Vert(Attributes v)
        {
            Varyings o;
            float3 ws = TransformObjectToWorld(v.positionOS);
            o.positionCS = PsxClip(ws);
            o.positionRWS = ws;
            float2 at = (v.positionOS.xz + _PsxSeaFlow.xy) * _SeaScale;
            o.uv = float4(at, at * _SeaDrift.x + _SeaDrift.yz * _PsxSeaFlow.z * _SeaScale);
            o.uvAffine = o.uv;
            o.light = float4(PsxLight(v.normalOS), PsxFogAmount(ws));
            o.colour = v.colour;
            o.glint = Glint(v.normalOS, ws);
            return o;
        }

        float4 FragDepth(Varyings i) : SV_Target
        {
            return 0;
        }

        float4 FragColour(Varyings i) : SV_Target
        {
            float4 uv = lerp(i.uv, i.uvAffine, _PsxOptions.x);
            float tile = SAMPLE_TEXTURE2D_LOD(_SeaTex, psx_point_repeat_sampler, uv.xy, 0).r
                       + SAMPLE_TEXTURE2D_LOD(_SeaTex, psx_point_repeat_sampler, uv.zw, 0).r;
            float3 albedo = i.colour.rgb * lerp(1.0, 0.5 + tile * 0.5, _SeaTexAmount * i.colour.a);
            float3 light = i.light.rgb;
            float glint = i.glint;
            if (_PsxOptions.y > 0.5)
            {
                float3 n = PsxFlatNormal(i.positionRWS);
                light = PsxLight(n);
                glint = Glint(n, i.positionRWS);
            }
            float3 col = albedo * light + _PsxSunColour.rgb * glint;
            col = lerp(col, _PsxFogColour.rgb, i.light.a);
            return float4(col * _PsxOptions.w, 1.0);
        }
        ENDHLSL

        Pass
        {
            Name "DepthForwardOnly"
            Tags { "LightMode" = "DepthForwardOnly" }
            Cull Back
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Stencil
            {
                WriteMask 8
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
            Cull Back
            ZWrite On
            ZTest LEqual
            Stencil
            {
                WriteMask 3
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
