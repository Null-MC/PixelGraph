#define MEDIUMP_FLT_MAX    65504.0
#define saturateMediump(x) min(x, MEDIUMP_FLT_MAX)

#pragma pack_matrix(row_major)


float3 calc_normal(const in float2 tex, const in float3 normal, const in float3 tangent, const in float3 bitangent)
{
    float3 tex_normal = tex_normal_height.Sample(sampler_surface, tex).xyz;
	tex_normal = mad(2.0f, tex_normal, -1.0f);
	
    return normalize(normal + mad(tex_normal.x, tangent, tex_normal.y * bitangent));
}

float3 Fresnel_Shlick(const in float3 f0, const in float3 f90, const in float x)
{
    return f0 + (f90 - f0) * pow(1.f - x, 5);
}

float Filament_F_Schlick(const float f0, const float VoH) {
	const float f = pow(1.0 - VoH, 5.0);
    return f + f0 * (1.0 - f);
}

float3 Filament_F_Schlick(const float3 f0, const float VoH) {
	const float f = pow(1.0 - VoH, 5.0);
    return f + f0 * (1.0 - f);
}

float Diffuse_Burley(const in float NdotL, const in float NdotV, in float LdotH, in float roughness)
{
    return Filament_F_Schlick(1, NdotL).x * Filament_F_Schlick(1, NdotV).x;
    //float fd90 = 0.5f + 2.f * roughness * LdotH * LdotH;
    //return Fresnel_Shlick(1, fd90, NdotL).x * Fresnel_Shlick(1, fd90, NdotV).x;
}

float Filament_D_GGX(const in float linearRoughness, const in float NoH, const in float3 n, const in float3 h) {
	const float3 NxH = cross(n, h);
	const float a = NoH * linearRoughness;
	const float k = linearRoughness / (dot(NxH, NxH) + a * a);
	const float d = k * k * (1.0 / PI);
    return saturateMediump(d);
}

float G_Shlick_Smith_Hable(const float alpha, const float LdotH)
{
    return rcp(lerp(LdotH * LdotH, 1, alpha * alpha * 0.25f));
}

float3 Specular_BRDF(const in float alpha, const in float3 f0, in float NdotV, in float NdotL, const in float LdotH, const in float NdotH, const in float3 N, const in float3 H)
{
    // Specular D (microfacet normal distribution) component
    const float specular_d = Filament_D_GGX(alpha, NdotH, N, H);//Specular_D_GGX(alpha, NdotH);

    // Specular Fresnel
    const float3 specular_f = Filament_F_Schlick(f0, LdotH);//Fresnel_Shlick(specularColor, 1, LdotH);

    // Specular G (visibility) component
    const float specular_g = G_Shlick_Smith_Hable(alpha, LdotH);

    return specular_d * specular_g * specular_f;
}

float3 specular_IBL(const in float3 normal, const in float3 view, const in float lod_bias)
{
    const float3 dir = reflect(-view, normal);
	const float mip = lod_bias * NumEnvironmentMapMipLevels;
    return tex_cube.SampleLevel(sampler_IBL, dir, mip).rgb;
}
