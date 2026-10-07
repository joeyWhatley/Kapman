// RAIN AND OTHER SEE-THROUGH THINGS IN THE CONSOLE'S STYLE: flat cards of one colour (a ParticleSystem's colour, or a
// strip mesh's) times a small texture read with hard texels, lit as a card facing up by the same light as the models.
// No soft edges: a texel is there or it is not (Cutout over 0), or it is laid over the picture by the blend chosen
// (One, One adds it, as the console's bright blend did).
// For a ParticleSystem's renderer, a LineRenderer or a plain strip mesh.
Shader "Kapman/ShipView/PSX Sprite"
{
    Properties
    {
        _MainTex("Picture (small)", 2D) = "white" {}
        _Color("Tint", Color) = (1, 1, 1, 1)
        _Cutout("Cutout: texels fainter than this are left out (0: blend instead)", Range(0, 1)) = 0
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
            float _Src;
            float _Dst;
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
                float3 positionRWS : TEXCOORD1;
            };

            Varyings Vert(Attributes v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                Varyings o;
                float3 ws = TransformObjectToWorld(v.positionOS);
                o.positionCS = PsxClip(ws);
                o.positionRWS = ws;
                o.uv = v.uv;
                float3 light = lerp(float3(1, 1, 1), PsxLight(float3(0, 1, 0)), _Lit);
                o.colour = v.colour * _Color * float4(light, 1);
                return o;
            }

            float4 Frag(Varyings i) : SV_Target
            {
                PsxFarFade(i.positionCS, i.positionRWS);
                float4 c = SAMPLE_TEXTURE2D_LOD(_MainTex, s_point_clamp_sampler, i.uv, 0) * i.colour;
                if (_Cutout > 0.001)
                {
                    clip(c.a - _Cutout);
                    c.a = 1;
                }
                return c;
            }
            ENDHLSL
        }
    }
    Fallback Off
}
