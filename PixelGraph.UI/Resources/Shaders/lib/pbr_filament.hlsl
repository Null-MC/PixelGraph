#define MEDIUMP_FLT_MAX    65504.0
#define saturateMediump(x) min(x, MEDIUMP_FLT_MAX)

#pragma pack_matrix(row_major)


float3 Fresnel_Shlick(const in float3 f0, const in float3 f90, const in float x)
{
    return f0 + (f90 - f0) * pow(1.0 - x, 5.0);
}

float Filament_F_Schlick(const float f0, const float VoH) {
	const float f = pow(1.0 - VoH, 5.0);
    return f + f0 * (1.0 - f);
}

float3 Filament_F_Schlick(const float3 f0, const float VoH) {
	const float f = pow(1.0 - VoH, 5.0);
    return f + f0 * (1.0 - f);
}

float Diffuse_Burley(const in float NdotL, const in float NdotV)
{
    return Filament_F_Schlick(1.0, NdotL).x * Filament_F_Schlick(1, NdotV).x;
}

float Diffuse_Burley(const in float3 f0, const in float NdotL, const in float NdotV, in float LdotH, in float roughness)
{
	return Diffuse_Burley(NdotL, NdotV);
	
    //float fd90 = 0.5 + 2 * roughness * LdotH * LdotH;
    //return Fresnel_Shlick(f0, fd90, NdotL).x * Fresnel_Shlick(1, fd90, NdotV).x;
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
    return rcp(lerp(LdotH * LdotH, 1.0, alpha * alpha * 0.25f));
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
    return tex_environment.SampleLevel(sampler_environment, dir, mip).rgb;
}

//float3 fresnelSchlickRoughness(const in float3 f0, const in float cosTheta, const in float rough)
//{
//	const float3 smooth = 1.0 - rough;
//    return f0 + (max(float3(smooth), f0) - f0) * pow(max(1.0 - cosTheta, 0.0), 5.0);
//}

//float3 diffuse_IBL(const in float3 normal, const in float3 view, const in float3 f0, const in float rough)
//{
//	const float NoV = max(dot(normal, view), 0.0);
//	const float3 kS = fresnelSchlickRoughness(f0, NoV, rough);
//	const float3 irradiance = tex_irradiance.SampleLevel(sampler_irradiance, normal, 0);
//	return (1.0 - kS) * irradiance;
//}
