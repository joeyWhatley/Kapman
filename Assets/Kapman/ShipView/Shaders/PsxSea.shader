// THE SEA AS A 1997 CONSOLE DREW IT. A coarse grid of triangles (PsxSeaGrid.cs moves its vertices on the CPU with the
// very waves the ship rides, and colours each vertex: darker in the troughs, lighter on the crests; how white it is,
// where it breaks and in her wake, comes beside the colour). Over that, one small tile read twice with hard texels,
// the two sliding over each other, carried across the screen without perspective (so it warps on the big near
// triangles, as it did then); light per vertex or per triangle, a hard glint of the Sun on the facets, the sky's own
// light thrown back where the water is seen at a low angle (so a face turned to the eye is dark and one turned away is
// bright: that is what shows the waves' shape under a grey sky, and what turns the water warm at dusk), and the far
// water broken up into the sky.
// The mesh's vertices are metres east and north of the stage's middle (its object sits there, unturned).
Shader "Kapman/ShipView/PSX Sea"
{
    Properties
    {
        _SeaTex("Sea tile (small; the red channel is read)", 2D) = "gray" {}
        _SeaScale("Tiles a metre", Float) = 0.25
        _SeaSecond("Second layer: its size against the first", Float) = 0.625
        _SeaTexAmount("How much the tile shows", Range(0, 1)) = 0.85
        _SeaTexFar("Metres at which the tile has faded out (it shimmers far off: no mip-maps, as then)", Float) = 110
        _SeaGlint("Sun glint", Range(0, 2)) = 0.4
        _SeaGlintPower("Sun glint: how tight", Range(2, 128)) = 64
        _SeaFoamColour("Foam (set by the grid)", Color) = (0.6, 0.65, 0.67, 1)
        _SeaSky("How much of the sky's light the water throws back along its surface (rough water: about half)", Range(0, 3)) = 0.6
    }

    SubShader
    {
        Tags { "RenderPipeline" = "HDRenderPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }

        HLSLINCLUDE
        #pragma target 4.5
        #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

        #include "PsxCommon.hlsl"

        TEXTURE2D(_SeaTex);
        float4 _PsxSeaFlow;       // xy: how far the first layer has slid (m), zw: the second (set by PsxSeaGrid, both wrapped)

        CBUFFER_START(UnityPerMaterial)
        float _SeaScale;
        float _SeaSecond;
        float _SeaTexAmount;
        float _SeaTexFar;
        float _SeaGlint;
        float _SeaGlintPower;
        float _SeaSky;
        float4 _SeaFoamColour;
        CBUFFER_END

        struct Attributes
        {
            float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 colour : COLOR;              // rgb the water's own colour here, a how much of the tile shows (0 far off)
            float2 foam : TEXCOORD0;            // x: how white it is here (breaking crests, her wake)
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float4 uv : TEXCOORD0;
            noperspective float4 uvAffine : TEXCOORD1;
            float3 light : TEXCOORD2;
            float3 positionRWS : TEXCOORD3;
            float4 colour : COLOR0;
            float4 shine : TEXCOORD4;           // rgb the sky thrown back, a the Sun's glint
            float foam : TEXCOORD5;
        };

        float Glint(float3 n, float3 positionRWS)
        {
            float3 toEye = normalize(-positionRWS);
            float3 h = normalize(toEye + _PsxSunDir.xyz);
            return pow(saturate(dot(n, h)), _SeaGlintPower) * _SeaGlint * step(0.0, _PsxSunDir.y);
        }

        // the sky's light thrown back: a fiftieth looking straight down into the water, all of it along the surface. What
        // sky: HDRP's own, from the side the water throws the eye's line to (its ambient probe, which is soft: a
        // console's sea took one colour from its sky, not a picture of it), so it is warm toward a low Sun
        float3 Sky(float3 n, float3 positionRWS)
        {
            float3 toEye = normalize(-positionRWS);
            float f = 1.0 - saturate(dot(n, toEye));
            float f2 = f * f;
            float3 to = reflect(-toEye, n);
            to = normalize(float3(to.x, abs(to.y) + 0.6, to.z));      // (well over the horizon: sky, not the ground under it)
            float3 sky = max(EvaluateAmbientProbe(to), 0.0) * GetCurrentExposureMultiplier();
            return sky * (_SeaSky * (0.02 + 0.98 * f2 * f2 * f));
        }

        // the same places as Vert and none of its light, for the depth pass
        Varyings VertDepth(Attributes v)
        {
            Varyings o = (Varyings)0;
            float3 ws = TransformObjectToWorld(v.positionOS);
            o.positionCS = PsxClip(ws);
            o.positionRWS = ws;
            return o;
        }

        Varyings Vert(Attributes v)
        {
            Varyings o;
            float3 ws = TransformObjectToWorld(v.positionOS);
            o.positionCS = PsxClip(ws);
            o.positionRWS = ws;
            o.uv = float4((v.positionOS.xz + _PsxSeaFlow.xy) * _SeaScale, (v.positionOS.xz + _PsxSeaFlow.zw) * _SeaScale * _SeaSecond);
            o.uvAffine = o.uv;
            o.light = PsxLight(v.normalOS);
            o.colour = v.colour;
            o.foam = v.foam.x;
            o.colour.a *= saturate(1.0 - length(ws) / max(_SeaTexFar, 1.0));
            o.shine = float4(Sky(v.normalOS, ws), Glint(v.normalOS, ws));
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
            float4 uv = lerp(i.uv, i.uvAffine, _PsxOptions.x);
            // (two layers of a tile whose middle grey is a half: their sum is 1 where neither has a stroke)
            float tile = SAMPLE_TEXTURE2D_LOD(_SeaTex, psx_point_repeat_sampler, uv.xy, 0).r
                       + SAMPLE_TEXTURE2D_LOD(_SeaTex, psx_point_repeat_sampler, uv.zw, 0).r;
            float marks = lerp(1.0, tile, _SeaTexAmount * _PsxOptions.w * i.colour.a);
            float3 light = i.light;
            float4 shine = i.shine;
            float foam = i.foam;
            if (_PsxOptions.y > 0.5)
            {
                float3 n = PsxFlatNormal(i.positionRWS);
                light = PsxLight(n);
                shine = float4(Sky(n, i.positionRWS), Glint(n, i.positionRWS));
                // (one shade to a facet, so the foam too is there or not: hard-edged patches cut straight across the triangles)
                foam = step(0.3, foam) * 0.85;
            }
            float3 water = lerp(i.colour.rgb, _SeaFoamColour.rgb, saturate(foam));
            float3 col = (water * light + shine.rgb * (1.0 - 0.7 * saturate(foam))) * marks + _PsxSunColour.rgb * shine.a;
            return float4(col, 1.0);
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
            #pragma vertex VertDepth
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
