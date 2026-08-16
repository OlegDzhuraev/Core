#ifndef INSANEONE_PBR_COMMON_INCLUDED
#define INSANEONE_PBR_COMMON_INCLUDED

sampler2D _MainTex;
sampler2D _BumpMap;
sampler2D _MasksTex;

half _BumpScale;
half _OcclusionStrength;
fixed4 _Color;

struct Input
{
    float2 uv_MainTex;
    float2 uv_BumpMap;
};

// Samples albedo/normal/masks and fills the parts of SurfaceOutputStandard shared by
// PBRMetalGloss and PBRMetalRough. Metallic/Smoothness/Occlusion (mask channel packing
// differs between the two) are left for the caller to assign from the returned masks.
void pbrSurfBase(Input IN, inout SurfaceOutputStandard o, out fixed4 albedo, out fixed4 masks)
{
    albedo = tex2D(_MainTex, IN.uv_MainTex) * _Color;
    masks = tex2D(_MasksTex, IN.uv_MainTex);
    o.Normal = UnpackScaleNormal(tex2D(_BumpMap, IN.uv_BumpMap), _BumpScale);

    o.Albedo = albedo.rgb;
    o.Alpha = albedo.a;
}

#endif
