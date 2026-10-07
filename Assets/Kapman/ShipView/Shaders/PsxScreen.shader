// THE CONSOLE'S SCREEN: the finished low picture of the ship (what HDRP wrote into the stage camera's texture,
// already tone-mapped, 8 bits a channel) copied to a second texture of the same size with the colours cut down:
//   - to so many levels a channel (32: the console's 15-bit colour), with its own 4 x 4 ordered dither added first, as
//     the console's video chip did when it wrote a pixel; or
//   - to a few TONES of one ramp of colours, by brightness (the "ink" look: the frame's plum, the sea's colour, its
//     light accent, paper).
// Run by PsxStageLook with one triangle over the whole target (CommandBuffer.DrawProcedural), outside HDRP altogether:
// no custom pass, no custom post-process, nothing to register in the HDRP settings, and it can only ever touch this one
// picture. It includes nothing of Unity's, so no render pipeline has a say in it: the source is read texel for texel
// (Load) at the very pixel being written, which also means nothing can come out upside down.
Shader "Hidden/Kapman/ShipView/PSX Screen"
{
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag

            Texture2D _PsxSource;
            Texture2D _PsxRamp;
            float4 _PsxLift;        // x: the brightness is multiplied by this before it is cut to tones (a night lifted so a print of it can be read)
            float4 _PsxScreen;      // x: levels a channel (32; 256: untouched)  y: dither 0..1  z: tones (0: off)  w: 1 when the project is in linear colour

            // the console's own table (added to an 8-bit value before it is cut to 5 bits)
            static const float Table[16] =
            {
                -4.0,  0.0, -3.0,  1.0,
                 2.0, -2.0,  3.0, -1.0,
                -3.0,  1.0, -4.0,  0.0,
                 3.0, -1.0,  2.0, -2.0
            };

            float4 Vert(uint id : SV_VertexID) : SV_POSITION
            {
                float2 uv = float2((id << 1) & 2, id & 2);
                return float4(uv * 2.0 - 1.0, 0.5, 1.0);
            }

            float3 ToScreen(float3 c)       // linear light to what the screen is given (sRGB)
            {
                c = saturate(c);
                return lerp(c * 12.92, 1.055 * pow(max(c, 1e-6), 1.0 / 2.4) - 0.055, step(0.0031308, c));
            }

            float3 FromScreen(float3 c)
            {
                return lerp(c / 12.92, pow((max(c, 0.0) + 0.055) / 1.055, 2.4), step(0.04045, c));
            }

            float4 Frag(float4 at : SV_POSITION) : SV_Target
            {
                int2 px = int2(at.xy);
                float3 c = _PsxSource.Load(int3(px, 0)).rgb;
                if (_PsxScreen.w > 0.5)
                    c = ToScreen(c);
                float d = Table[(px.x & 3) + (px.y & 3) * 4] * _PsxScreen.y;

                if (_PsxScreen.z > 1.5)
                {
                    // a few tones of one ramp, by brightness
                    float tones = _PsxScreen.z;
                    float lum = dot(c, float3(0.299, 0.587, 0.114)) * max(_PsxLift.x, 1.0);
                    float t = floor(saturate(lum + d / 8.0 / tones) * tones * 0.999);
                    c = _PsxRamp.Load(int3((int)t, 0, 0)).rgb;
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
            ENDHLSL
        }
    }
    Fallback Off
}
