Shader "PBR Glossines-Metallic"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Strength", Range(0, 2)) = 1
        _MasksTex ("Masks (R AO, G Glossiness, B Metallic, A Custom Mask)", 2D) = "black" {}
        _OcclusionStrength ("Occlusion Strength", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        #pragma multi_compile_instancing

        #include "PBRCommon.cginc"

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c, masks;
            pbrSurfBase(IN, o, c, masks);

            o.Metallic = masks.b;
            o.Smoothness = masks.g;
            o.Occlusion = lerp(1, masks.r, _OcclusionStrength);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
