Shader "Unlit/OutlineShader"
{
    Properties
    {
        _Color ("Outline Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        _SizeX ("Outline Size X", Range (0, 1)) = 0.1
        _SizeY ("Outline Size Y", Range (0, 1)) = 0.1
        _SizeZ ("Outline Size Z", Range (0, 1)) = 0.1
        _Offset ("Offset", Range (0, 1)) = 0.1
    }
    SubShader 
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry"}
        Pass 
        {
            Cull Front
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _SizeX, _SizeY, _SizeZ, _Offset;
            v2f vert (appdata v)
            {
                v.vertex.xyz = v.vertex.xyz + float3(v.normal.x * (sin(_Time.w) * 0.5 + 0.5 + _Offset) * _SizeX, v.normal.y * (sin(_Time.w) * 0.5 + 0.5 + _Offset) * _SizeY, v.normal.z * (sin(_Time.w) * 0.5 + 0.5 + _Offset) * _SizeZ);
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            float4 frag (v2f i) : SV_Target 
            {
                float4 col = tex2D(_MainTex, i.uv);
                return col * _Color;
            }
            ENDHLSL
        }
    }
}
