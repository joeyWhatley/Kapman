// PROJECT PATH (later): Assets/Kapman/Shaders/ShipView/PsxScreen.shader
// DRAFT, 7 Oct 2026. UNVERIFIED: written without Unity running, never compiled.
//
// THE CONSOLE'S SCREEN: the finished low picture of the ship (what HDRP wrote into the stage camera's RenderTexture,
// already tone-mapped, 8 bits a channel) copied to a second RenderTexture of the same size with the colours cut down:
//   - to so many levels a channel (32: the console's 15-bit colour), with its own 4 x 4 ordered dither added first, as
//     the console's video chip did when it wrote a pixel; or
//   - to a few TONES of one ramp of colours, by brightness (the "ink" style: the frame's plum, the sea's colour, its
//     light accent, paper).
// Run by ShipViewLook with Graphics.Blit, outside HDRP altogether: no custom pass, no custom post-process, nothing to
// register in the HDRP settings, and it can only ever touch this one picture (the frame and the chart are other
// pictures). A plain built-in-style shader on purpose: Graphics.Blit draws its own quad whatever the render pipeline.
Shader "Hidden/Kapman/ShipView/PSX Screen"
{
    Properties
    {
        _MainTex("Picture", 2D) = "black" {}
        _RampTex("Ramp (dark to light)", 2D) = "white" {}
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment Frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            sampler2D _RampTex;
            float4 _PsxScreen;      // x: levels a channel (32; 256: untouched)  y: dither 0..1  z: tones (0: off)  w: 1 when the project is in linear colour

            // the console's own table (added to an 8-bit value before it is cut to 5 bits)
            static const float Table[16] =
            {
                -4.0,  0.0, -3.0,  1.0,
                 2.0, -2.0,  3.0, -1.0,
                -3.0,  1.0, -4.0,  0.0,
                 3.0, -1.0,  2.0, -2.0
            };

            float3 ToScreen(float3 c)       // linear light to what the screen is given (sRGB)
            {
                c = saturate(c);
                return lerp(c * 12.92, 1.055 * pow(c, 1.0 / 2.4) - 0.055, step(0.0031308, c));
            }

            float3 FromScreen(float3 c)
            {
                return lerp(c / 12.92, pow((c + 0.055) / 1.055, 2.4), step(0.04045, c));
            }

            float4 Frag(v2f_img i) : SV_Target
            {
                float3 c = tex2D(_MainTex, i.uv).rgb;
                if (_PsxScreen.w > 0.5)
                    c = ToScreen(c);
                float2 px = floor(i.uv * _MainTex_TexelSize.zw);
                float d = Table[(int)fmod(px.x, 4.0) + (int)fmod(px.y, 4.0) * 4] * _PsxScreen.y;

                if (_PsxScreen.z > 1.5)
                {
                    // a few tones of one ramp, by brightness
                    float tones = _PsxScreen.z;
                    float lum = dot(c, float3(0.299, 0.587, 0.114));
                    float t = floor(saturate(lum + d / 8.0 / tones) * tones * 0.999);
                    c = tex2D(_RampTex, float2((t + 0.5) / tones, 0.5)).rgb;
                    if (_PsxScreen.w > 0.5)
                        c = ToScreen(c);
                }
                else if (_PsxScreen.x < 255.5)
                {
                    // so many levels a channel: the table is in steps of an eighth of a level, as on the console
                    float levels = max(2.0, _PsxScreen.x);
                    float step8 = 256.0 / levels;
                    float3 v = clamp(c * 255.0 + d * step8 / 8.0, 0.0, 255.0);
                    c = floor(v / step8) / (levels - 1.0);
                }

                if (_PsxScreen.w > 0.5)
                    c = FromScreen(c);
                return float4(c, 1.0);
            }
            ENDCG
        }
    }
    Fallback Off
}
