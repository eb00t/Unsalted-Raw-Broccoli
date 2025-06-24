Shader "Custom/DeathScreenCircle"
{
    Properties
    {
        _Color("Overlay Color", Color) = (0,0,0,1)
        _HoleRadius("Hole Radius", Range(0, 1)) = 0.5
        _FadeEdge("Edge Softness", Range(0.001, 0.5)) = 0.05
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _Color;
            float _HoleRadius;
            float _FadeEdge;

            v2f vert (appdata v)
            {
                v2f o;
                o.position = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float2 centerUV = i.uv - float2(0.5, 0.5);
                float dist = length(centerUV);
                
                float edge = smoothstep(_HoleRadius, _HoleRadius - _FadeEdge, dist);
                float alpha = (1.0 - edge) * _Color.a;
                return float4(_Color.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
