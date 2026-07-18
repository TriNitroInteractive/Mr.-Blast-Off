Shader "Custom/URPAdditiveRay"
{
    Properties
    {
        [HDR] _Color ("Tint Color", Color) = (1.0, 0.45, 0.1, 1.0)
        _FadePower ("Length Fade Power", Float) = 2.0
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline"="UniversalPipeline"
        }
        LOD 100

        Pass
        {
            Name "Unlit"
            Blend One One // Additive Blending
            ZWrite Off
            Cull Off      // Make double-sided cross quads fully visible

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };

            float4 _Color;
            float _FadePower;

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                // Edge softening: fade towards the left and right sides of the quad
                // Since UV.x goes from 0 to 1, sin(UV.x * PI) is 0 at edges and 1 at center.
                float edgeFade = sin(input.uv.x * 3.14159265);

                // Length fading: fade towards the tip (UV.y = 1)
                // We use pow() to customize how quickly the ray tapers off along its height
                float lengthFade = pow(saturate(1.0 - input.uv.y), _FadePower);

                // Combine fades
                float finalAlpha = edgeFade * lengthFade;

                // Return additive HDR color
                return _Color * finalAlpha;
            }
            ENDHLSL
        }
    }
}
