
Shader "Hidden/Vestigia/CameraBlur"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "CameraBlur"
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D_X(_BlitTexture);
            float4 _BlitTexture_TexelSize;
            float _BlurSize;

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;

                output.positionHCS =
                    GetFullScreenTriangleVertexPosition(input.vertexID);

                output.uv =
                    GetFullScreenTriangleTexCoord(input.vertexID);

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 offset =
                    _BlitTexture_TexelSize.xy * _BlurSize;

                half4 color = SAMPLE_TEXTURE2D_X(
                    _BlitTexture, sampler_LinearClamp, input.uv
                ) * 0.2;

                color += SAMPLE_TEXTURE2D_X(
                    _BlitTexture, sampler_LinearClamp,
                    input.uv + float2(offset.x, 0)
                ) * 0.2;

                color += SAMPLE_TEXTURE2D_X(
                    _BlitTexture, sampler_LinearClamp,
                    input.uv - float2(offset.x, 0)
                ) * 0.2;

                color += SAMPLE_TEXTURE2D_X(
                    _BlitTexture, sampler_LinearClamp,
                    input.uv + float2(0, offset.y)
                ) * 0.2;

                color += SAMPLE_TEXTURE2D_X(
                    _BlitTexture, sampler_LinearClamp,
                    input.uv - float2(0, offset.y)
                ) * 0.2;

                return color;
            }

            ENDHLSL
        }
    }
}
