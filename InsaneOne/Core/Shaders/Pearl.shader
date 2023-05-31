Shader "PBR Pearl" 
{
    Properties 
    {
        _Color ("Color", Color) = (0.9,0,0,1)
        _Color2 ("Color 2", Color) = (0,0.5,0.75,1)
        _Mix("Colors Mix", Range(0, 10)) = 1
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0    
    }
    SubShader 
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0
        
        struct Input
        {
            float3 viewDir;
            float3 worldNormal;
            INTERNAL_DATA
        };
        
        half _Glossiness;
        half _Metallic;
        half4 _Color;
        half4 _Color2;
        half _Mix;

        half pearlMixColors(half inDot)
		{
            inDot = clamp(inDot, 0.001, 1);
			return pow(inDot, _Mix);
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            half dotResult = dot(IN.viewDir, WorldNormalVector(IN, o.Normal));
            half4 c = lerp(_Color2, _Color, pearlMixColors(dotResult));
            
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}