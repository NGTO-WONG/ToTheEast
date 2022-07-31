Shader "_Custom/ObjectNormals"
{
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
         TEXTURE2D(_CameraDepthTexture);
         SAMPLER(sampler_CameraDepthTexture);
            
        float4 _CameraColorTexture_TexelSize;
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
            };
            struct Varyings
            {
                float3 normalWS                 : TEXCOORD0;
                float4 positionCS               : SV_POSITION;
            };
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS =TransformObjectToHClip(input.positionOS);
                output.normalWS =TransformObjectToWorldNormal(input.normalOS);
                return output;
            }
            half4 frag(Varyings input) : SV_Target
            {
                float2 size=_CameraColorTexture_TexelSize;
                half3 col= mul(input.normalWS,unity_MatrixInvV);
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}