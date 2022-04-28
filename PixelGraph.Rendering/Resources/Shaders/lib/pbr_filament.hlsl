//#include "common_funcs.hlsl"

#define MEDIUMP_FLT_MAX    65504.0
#define saturateMediump(x) min(x, MEDIUMP_FLT_MAX)

#pragma pack_matrix(row_major)


float Fresnel_Shlick(const in float f0, const in float f90, const in float x)
{
    return f0 + (f90 - f0) * pow(1.0 - x, 5.0);
}

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

float Diffuse_Burley(const in float NoL, const in float NoV, const in float LoH, const in float rough)
{
	const float f90 = 0.5f + 2.0f * rough * LoH * LoH;
	const float light_scatter = Fresnel_Shlick(1.0f, f90, NoL);
	const float view_scatter = Fresnel_Shlick(1.0f, f90, NoV);
	return light_scatter * view_scatter * InvPI;
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


// IBL

float3 fresnelSchlickRoughness(const in float3 f0, const in float cosTheta, const in float rough)
{
	const float3 smooth = 1.0 - rough;
    return f0 + (max(float3(smooth), f0) - f0) * pow(max(1.0 - cosTheta, 0.0), 5.0);
}

float get_specular_occlusion(float NoV, float ao, float rough) {
	const float f = max(NoV + ao, 0.0);
	const float x1 = pow(f, exp2(-16.0 * rough - 1.0));
    return saturate(x1 - 1.0 + ao);
}

float3 diffuse_IBL(const in float3 normal, const in float3 kS)
{
	const float3 irradiance = tex_irradiance.SampleLevel(sampler_irradiance, normal, 0);
	return (1.0 - kS) * irradiance;
}

float3 specular_IBL(const in float3 ref, const in float3 kS, const in float roughP)
{
	const float mip = roughP * NumEnvironmentMapMipLevels;
    return tex_environment.SampleLevel(sampler_environment, ref, mip) * kS;
}

void IBL(float3 n, float3 v, float3 diffuse, float3 f0, float occlusion, float roughP, out float3 ambient, out float3 specular)
{
    float3 indirect_diffuse = srgb_to_linear(vLightAmbient.rgb);
    float3 indirect_specular = indirect_diffuse;
    float3 specular_occlusion = 1.0;
		
	const float NoV = max(dot(n, v), 0.0);
	const float3 kS = fresnelSchlickRoughness(f0, NoV, roughP);
    const float3 specular_color = lerp(f0, 1.0, kS); // WARN: wrong af
	
    if (bHasCubeMap) {
		const float3 ref = reflect(-v, n);
    	
    	indirect_diffuse = diffuse_IBL(n, kS);
		indirect_specular = specular_IBL(ref, kS, roughP);
		specular_occlusion = get_specular_occlusion(NoV, occlusion, roughP);
    }
	
    ambient = diffuse * indirect_diffuse * occlusion;
	specular = indirect_specular * specular_color * specular_occlusion;
}
