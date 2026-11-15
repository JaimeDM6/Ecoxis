Shader "Ecoxis/Selva/SiluetaOriginal"
{
    Properties
    {
        [PerRendererData] _MainTex("Silueta original (no modifica colisiones)", 2D) = "white" {}
        _JungleTex("Textura del atlas de la jungla", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _Mode("0 roca, 1 musgo, 2 espinas", Float) = 0
        _PixelsPerUnit("Densidad del pixel", Float) = 32
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "CanUseSpriteAtlas"="False" }
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            Name "SelvaUnlit"
            Tags { "LightMode"="Universal2D" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_JungleTex); SAMPLER(sampler_JungleTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Mode;
                float _PixelsPerUnit;
            CBUFFER_END
            struct Attributes { float3 positionOS : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float2 world : TEXCOORD1; float4 color : COLOR; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 world = TransformObjectToWorld(input.positionOS);
                output.positionCS = TransformWorldToHClip(world);
                output.world = world.xy;
                output.uv = input.uv;
                output.color = input.color * _Color;
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                // Solo se cambia el aspecto: se mantiene el alpha del sprite de referencia.
                half4 original = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                clip(original.a - 0.01h);
                float2 pixels = floor(input.world * _PixelsPerUnit);
                float2 tilePixel = frac((pixels + 0.5) / 32.0) * 32.0;
                // Fragmento rocoso de 32x32, extraído del atlas existente de 768x368.
                float2 rockUV = (float2(352.0, 128.0) + tilePixel) / float2(768.0, 368.0);
                half4 rockSample = SAMPLE_TEXTURE2D(_JungleTex, sampler_JungleTex, rockUV);
                // El atlas contiene transparencia entre sus detalles de piedra: se rellena
                // con roca oscura, nunca con negro ni alterando la silueta del terreno.
                half3 rock = lerp(half3(0.10h, 0.17h, 0.20h), rockSample.rgb, rockSample.a);
                half3 result = rock;
                if (_Mode > 0.5 && _Mode < 1.5)
                {
                    float2 grassUV = float2((32.5 + fmod(pixels.x + 32000.0, 128.0)) / 768.0,
                                           (128.5 + fmod(pixels.y + 32000.0, 16.0)) / 368.0);
                    half4 grass = SAMPLE_TEXTURE2D(_JungleTex, sampler_JungleTex, grassUV);
                    result = lerp(rock, grass.rgb, grass.a);
                }
                else if (_Mode > 1.5)
                {
                    // Espinas de raíz: misma forma, posición y daño que las trampas del polo.
                    half shade = dot(original.rgb, half3(0.299h, 0.587h, 0.114h));
                    result = lerp(half3(0.17h,0.095h,0.035h), half3(0.67h,0.43h,0.13h), shade);
                }
                result *= input.color.rgb;
                return half4(result, original.a * input.color.a);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
