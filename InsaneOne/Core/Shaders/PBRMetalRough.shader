Shader "PBR Metalness-Roughness" 
{
    Properties 
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _MasksTex ("Masks (R Roughness, G Metallness, B AO, A Custom Mask)", 2D) = "black" {}
    }
    SubShader 
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _MasksTex;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
        };

        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            fixed4 masks = tex2D(_MasksTex, IN.uv_MainTex);
            o.Normal = UnpackNormal(tex2D (_BumpMap, IN.uv_BumpMap));
            
            o.Albedo = c.rgb;
            o.Alpha = c.a;

            o.Smoothness = 1 - masks.r;
            o.Metallic = masks.g;
            o.Occlusion = masks.b;
        }
        ENDCG
    }
    FallBack "Diffuse"
}