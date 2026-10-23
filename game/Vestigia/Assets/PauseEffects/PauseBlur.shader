
Shader "CustomEffects/PauseBlur"
{
    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

    float _HorizontalBlur;
    float _VerticalBlur;

    float4 BlurVertical(Varyings input) : SV_Target
    {
        float2 uv = input.texcoord;
        float offset = _VerticalBlur / _BlitTexture_TexelSize.w;

        float4 color = 0;

        for (int i = -4; i <= 4; i++)
        {
            color += SAMPLE_TEXTURE2D_X(
                _BlitTexture, sampler_LinearClamp,
                uv + float2(0, offset * i)
            );
        }

        return color / 9.0;
    }

    float4 BlurHorizontal(Varyings input) : SV_Target
    {
        float2 uv = input.texcoord;
        float offset = _HorizontalBlur / _BlitTexture_TexelSize.z;

        float4 color = 0;

        for (int i = -4; i <= 4; i++)
        {
            color += SAMPLE_TEXTURE2D_X(
                _BlitTexture, sampler_LinearClamp,
                uv + float2(offset * i, 0)
            );
        }

        return color / 9.0;
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        Cull Off

        Pass
        {
            Name "VerticalBlur"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment BlurVertical
            ENDHLSL
        }

        Pass
        {
            Name "HorizontalBlur"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment BlurHorizontal
            ENDHLSL
        }
    }
}
