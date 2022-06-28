Shader "Custom/ObjectNormals2"
{
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            
            float nrand( float2 n )
            {
	            return frac(sin(dot(n, float2(12.9898, 78.233)))* 43758.5453);
            }
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS               : SV_POSITION;
            };
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS =TransformObjectToHClip(input.positionOS);
                output.uv = input.uv;
                return output;
            }
            half4 frag(Varyings input) : SV_Target
            {
                float col=nrand(input.uv.xy+_Time);
                return col;
            }
            ENDHLSL
        }
    }
}