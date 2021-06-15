#define PI 3.14159265f
#define MEDIUMP_FLT_MAX    65504.0
#define saturateMediump(x) min(x, MEDIUMP_FLT_MAX)

#pragma pack_matrix(row_major)


//float3 Fresnel_Shlick(const in float3 f0, const in float NoH)
//{
//    return f0 + (1.0 - f0) * pow(1.0 - NoH, 5.0);
//}

//float Filament_F_Schlick(const float f0, const float VoH) {
//	const float f = pow(1.0 - VoH, 5.0);
//    return f + f0 * (1.0 - f);
//}

float3 F_Schlick(const float3 f0, const float VoH) {
	const float f = pow(1.0 - VoH, 5.0);
    return f + f0 * (1.0 - f);
}

//float Diffuse_Burley(const in float3 f0, const in float NoL, const in float NoV)
//{
//    return Fresnel_Shlick(f0, , NoL).x * Filament_F_Schlick(f0, NoV).x;
//}
//
//float Diffuse_Burley(const in float3 f0, const in float NdotL, const in float NdotV, in float LdotH, in float roughness)
//{
//	return Diffuse_Burley(f0, NdotL, NdotV);
//	
//    //float fd90 = 0.5 + 2 * roughness * LdotH * LdotH;
//    //return Fresnel_Shlick(f0, fd90, NdotL).x * Fresnel_Shlick(1, fd90, NdotV).x;
//}

float Fd_Lambert() {
    return 1.0 / PI;
}

//float Fd_Burley(float3 NoV, float3 NoL, float3 LoH, float roughness) {
//    float f90 = 0.5 + 2.0 * roughness * LoH * LoH;
//    float3 lightScatter = F_Schlick(f90, NoL);
//    float3 viewScatter = F_Schlick(f90, NoV);
//    return lightScatter * viewScatter * (1.0 / PI);
//}

float3 disney_diffuse(const in float3 albedo, float NoV, float NoL, float LoH, float rough)
{
	const float fd90 = 0.5 + 2.0 * rough * rough * LoH * LoH;
	const float light = 1.0 + (fd90 - 1.0) * pow(1.0 - NoL, 5);
	const float view = 1.0 + (fd90 - 1.0) * pow(1.0 - NoV, 5);
	return (albedo) * light * view;
}

float D_GGX(const in float NoH, const in float3 n, const in float3 h, const in float roughness) {
	const float3 NxH = cross(n, h);
	const float a = NoH * roughness;
	const float k = roughness / (dot(NxH, NxH) + a * a);
	const float d = k * k * (1.0 / PI);
    return saturateMediump(d);
}

float V_SmithGGXCorrelated(float NoV, float NoL, float roughness) {
    float a2 = roughness * roughness;
    float GGXV = NoL * sqrt(NoV * NoV * (1.0 - a2) + a2);
    float GGXL = NoV * sqrt(NoL * NoL * (1.0 - a2) + a2);
    return 0.5 / (GGXV + GGXL);
}

float G_Shlick_Smith_Hable(const float alpha, const float LdotH)
{
    return rcp(lerp(LdotH * LdotH, 1.0, alpha * alpha * 0.25f));
}

float3 Specular_BRDF(const in float3 f0, const in float3 N, const in float3 H, const in float LoH, const in float NoH, const in float VoH, const in float rough)
{
    // Specular D (microfacet normal distribution) component
    const float specular_d = D_GGX(NoH, N, H, rough);//Specular_D_GGX(alpha, NdotH);

    // Specular Fresnel
    const float3 specular_f = F_Schlick(f0, VoH);//Fresnel_Shlick(specularColor, 1, LdotH);

    // Specular G (visibility) component
    const float specular_g = G_Shlick_Smith_Hable(LoH, rough * rough);

    return specular_d * specular_g * specular_f;
}


//static const uint sampleCount = 4;
//
//float2 hammersley(uint i, float numSamples) {
//    uint bits = i;
//    bits = (bits << 16) | (bits >> 16);
//    bits = ((bits & 0x55555555) << 1) | ((bits & 0xAAAAAAAA) >> 1);
//    bits = ((bits & 0x33333333) << 2) | ((bits & 0xCCCCCCCC) >> 2);
//    bits = ((bits & 0x0F0F0F0F) << 4) | ((bits & 0xF0F0F0F0) >> 4);
//    bits = ((bits & 0x00FF00FF) << 8) | ((bits & 0xFF00FF00) >> 8);
//    return float2(i / numSamples, bits / exp2(32));
//}
//
//float GDFG(float NoV, float NoL, float a) {
//    float a2 = a * a;
//    float GGXL = NoV * sqrt((-NoL * a2 + NoL) * NoL + a2);
//    float GGXV = NoL * sqrt((-NoV * a2 + NoV) * NoV + a2);
//    return (2 * NoL) / (GGXV + GGXL);
//}
//
//float2 DFG(float NoV, float a) {
//    float3 V;
//    V.x = sqrt(1.0f - NoV*NoV);
//    V.y = 0.0f;
//    V.z = NoV;
//
//    float2 r = 0.0f;
//    for (uint i = 0; i < sampleCount; i++) {
//        float2 Xi = hammersley(i, sampleCount);
//        float3 H = importanceSampleGGX(Xi, a, N);
//        float3 L = 2.0f * dot(V, H) * H - V;
//
//        float VoH = saturate(dot(V, H));
//        float NoL = saturate(L.z);
//        float NoH = saturate(H.z);
//
//        if (NoL > 0.0f) {
//            float G = GDFG(NoV, NoL, a);
//            float Gv = G * VoH / NoH;
//            float Fc = pow(1 - VoH, 5.0f);
//            r.x += Gv * (1 - Fc);
//            r.y += Gv * Fc;
//        }
//    }
//    return r * (1.0f / sampleCount);
//}

float3 irradianceSH(float3 normal)
{
	return 0;
}

float2 prefilteredDFG_LUT(const in float3 NoV, const in float roughP)
{
	return 0;
}

float3 specular_IBL(const in float3 normal, const in float3 view, const in float roughP)
{
    const float3 ref = reflect(-view, normal);
	const float mip = roughP * NumEnvironmentMapMipLevels;
    return tex_cube.SampleLevel(sampler_cube, ref, mip).rgb;
}

float get_specular_occlusion(float NoV, float ao, float rough) {
	const float f = max(NoV + ao, 0.0);
	const float x1 = pow(f, exp2(-16.0 * rough - 1.0));
    return saturate(x1 - 1.0 + ao);
}

float3 IBL(float3 n, float3 v, float3 diffuse, float3 f0, float3 f90, float occlusion, float roughP)
{
    if (!bHasCubeMap) {
    	float3 ambient = srgb_to_linear(vLightAmbient.rgb);
    	
    	return ambient * diffuse;
    }

    return tex_cube.SampleLevel(sampler_cube, n, 0).rgb;
	
	const float NoV = max(dot(n, v), 0.0);

    // Specular indirect
    const float3 indirect_specular = specular_IBL(n, v, roughP);
    const float2 env = prefilteredDFG_LUT(NoV, roughP);
    const float3 specular_color = f0 * env.x + f90 * env.y;

    // Diffuse indirect
    // We multiply by the Lambertian BRDF to compute radiance from irradiance
    // With the Disney BRDF we would have to remove the Fresnel term that
    // depends on NoL (it would be rolled into the SH). The Lambertian BRDF
    // can be baked directly in the SH to save a multiplication here
    const float3 indirect_diffuse = max(irradianceSH(n), 0.0) * Fd_Lambert();

    //float3 NoV = dot(normal, view);
    const float3 specular_occlusion = get_specular_occlusion(NoV, occlusion, roughP * roughP);

	
    // Indirect contribution
    return diffuse * indirect_diffuse * occlusion; // + indirect_specular * specular_color * specular_occlusion;
}
