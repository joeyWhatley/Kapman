// PROJECT PATH (later): Assets/Kapman/Shaders/ShipView/PsxSprite.shader
// DRAFT, 7 Oct 2026. UNVERIFIED: written without Unity running, never compiled.
//
// SPRAY, RAIN, SPLASHES AND THE WAKE'S FOAM IN THE CONSOLE'S STYLE: flat cards with a small texture read with hard
// texels, times the vertex colour (a ParticleSystem's colour, or a strip mesh's), lit by the same light as the models
// but as a card facing up. No sorting tricks, no soft edges: a texel is there or it is not (Cutout over 0), or it is
// laid over half see-through as the console's blend did.
// For a ParticleSystem's renderer, a LineRenderer or a plain strip mesh.
Shader "Kapman/ShipView/PSX Sprite"
{
    Properties
    {
        _MainTex("Picture (small)", 2D) = "white" {}
        _Color("Tint", Color) = (1, 1, 1, 1)
        _Cutout("Cutout: texels fainter than this are left out (0: blend instead)", Range(0, 1)) = 0.5
        _Lit("How much the light colours it (0: its own colour)", Range(0, 1)) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _Src("Blend: source", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _Dst("Blend: destination (One to add, as the console's bright blend)", Float) = 10
    }

    SubShader
    {
        Tags { "RenderPipeline" = "HDRenderPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" }

        Pass
        {
            Name "ForwardOnly"
            Tags { "LightMode" = "ForwardOnly" }
            Cull Off
            ZWrite Off
            ZTest LEqual
            Blend [_Src] [_Dst]

            HLSLPROGRAM
            #pragma target 4.5
            #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch
            #pragma multi_compile_instancing
            #pragma vertex Vert
            #pragma fragment Frag

            #include "PsxCommon.hlsl"

            TEXTURE2D(_MainTex);

            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float _Cutout;
            float _Lit;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float4 colour : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 colour : COLOR0;
                float fog : TEXCOORD1;
            };

            Varyings Vert(Attributes v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                Varyings o;
                float3 ws = TransformObjectToWorld(v.positionOS);
                o.positionCS = PsxClip(ws);
                o.uv = v.uv;
                float3 light = lerp(float3(1, 1, 1), PsxLight(float3(0, 1, 0)), _Lit);
                o.colour = v.colour * _Color * float4(light, 1);
                o.fog = PsxFogAmount(ws);
                return o;
            }

            float4 Frag(Varyings i) : SV_Target
            {
                float4 c = SAMPLE_TEXTURE2D_LOD(_MainTex, s_point_clamp_sampler, i.uv, 0) * i.colour;
                if (_Cutout > 0.001)
                {
                    clip(c.a - _Cutout);
                    c.a = 1;
                }
                c.rgb = lerp(c.rgb, _PsxFogColour.rgb, i.fog);
                return float4(c.rgb * _PsxOptions.w, c.a);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
