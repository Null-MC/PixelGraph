#define PI 3.14159265f
#define IOR_N_WATER 1.333f
#define IOR_N_AIR 1.0f
#define WATER_ROUGH 0.1f

#pragma pack_matrix(row_major)


float F_schlick(const in float f0, const in float fd90, const in float cos_theta)
{
    return f0 + (fd90 - f0) * pow(1.0f - cos_theta, 5.0f);
}

float3 F_schlick(const in float3 f0, const in float3 fd90, const in float cos_theta)
{
    return f0 + (fd90 - f0) * pow(1.0f - cos_theta, 5.0f);
}

float3 F_schlick_roughness(float3 f0, float cos_theta, float rough) {
    return f0 + (max(1.0f - rough, f0) - f0) * pow(saturate(1.0f - cos_theta), 5.0f);
}

//https://docs.chaosgroup.com/display/OSLShaders/Complex+Fresnel+shader
float3 F_complex(const float3 n, const float3 k, const float cos)
{
	const float3 n2 = n*n;
	const float3 k2 = k * k;
	const float cos2 = cos * cos;

	const float3 rs_num = n2 + k2 - 2.0f * n*cos + cos2;
	const float3 rs_den = n2 + k2 + 2.0f * n*cos + cos2;
	const float3 rs = rs_num / rs_den;

	const float3 rp_num = (n2 + k2) * cos2 - 2.0f * n*cos + 1.0f;
	const float3 rp_den = (n2 + k2) * cos2 + 2.0f * n*cos + 1.0f;
	const float3 rp = rp_num / rp_den;
     
    return saturate(0.5f * (rs + rp));
}

//float3 F_complex_wet(const float3 n, const float3 k, const float cos)
//{
//	const float3 n2 = n*n;
//	const float3 k2 = k * k;
//	const float cos2 = cos * cos;
//
//	const float3 rs_num = n2 + k2 - 2.0f * n*cos + cos2;
//	const float3 rs_den = n2 + k2 + 2.0f * n*cos + cos2;
//	const float3 rs = rs_num / rs_den;
//
//	const float3 rp_num = (n2 + k2) * cos2 - 2.0f * n*cos + 1.0f;
//	const float3 rp_den = (n2 + k2) * cos2 + 2.0f * n*cos + 1.0f;
//	const float3 rp = rp_num / rp_den;
//     
//    return saturate(0.5f * (rs + rp));
//}

float D_ggx(const in float NoH, const in float roughness)
{
	const float a = NoH * roughness;
	const float k = roughness / (1.0f - NoH * NoH + a * a);
	return k * k * (1.0f / PI);
}

float V_SmithGGXCorrelated(float NoV, float NoL, float roughness)
{
	const float a2 = roughness * roughness;
	const float GGXV = NoL * sqrt(NoV * NoV * (1.0f - a2) + a2);
	const float GGXL = NoV * sqrt(NoL * NoL * (1.0f - a2) + a2);
    return 0.5f / (GGXV + GGXL);
}

float G_Shlick_Smith_Hable(const float LdotH, const float alpha)
{
    return rcp(lerp(LdotH * LdotH, 1.0f, alpha * alpha * 0.25f));
}

float V_kelemen(const in float LoH) {
    return 0.25f / (LoH * LoH);
}

float3 specular_brdf(const in float3 ior_n, const in float3 ior_k, const in float LoH, const in float NoH, const in float VoH, const in float rough)
{
    // Fresnel
    const float3 F = F_complex(ior_n, ior_k, VoH);

    // Distribution
    const float D = D_ggx(NoH, rough);

    // Geometric Visibility
    const float G = G_Shlick_Smith_Hable(LoH, rough);

	return D * F * G;
}

float3 specular_brdf_wet(const in float3 f0, const in float LoH, const in float NoH, const in float VoH, const in float rough)
{
    // Fresnel
    //const float3 F = F_complex(ior_n, ior_k, VoH);
	const float3 F = F_schlick(f0, 1.0, VoH);

    // Distribution
    const float D = D_ggx(NoH, rough);

    // Geometric Visibility
    const float G = G_Shlick_Smith_Hable(LoH, rough);

	return D * F * G;
}

float3 clearcoat_brdf(const in float3 Fd, const in float3 Fr, const in float LoH, const in float NoH, const in float rough, const in float wetness)
{
	//return Fd + Fr;

	//const float water_roughL = max(pow2(WATER_ROUGH), 0.089f);
	const float D = D_ggx(NoH, WATER_ROUGH);

	const float G = V_kelemen(LoH);

	//const float f90 = 0.5f + 2.0f * rough * LoH * LoH;
	const float F = F_schlick(IOR_N_WATER, 1.0f, LoH) * pow4(wetness);

	const float Frc = D * G * F;

	const float invFc = 1.0f - F;
	return (Fd + Fr * invFc) * invFc + Frc;
}

float3 Diffuse_Burley(const in float NoL, const in float NoV, const in float LoH, const in float rough)
{
	const float f90 = 0.5f + 2.0f * rough * LoH * LoH;
	const float3 light_scatter = F_schlick(1.0f, f90, NoL);
	const float3 view_scatter = F_schlick(1.0f, f90, NoV);
	return light_scatter * view_scatter * rcp(PI);
}


// IBL

float IBL_SpecularOcclusion(float NoV, float ao, float rough)
{
	const float f = saturate(NoV + ao);
	const float x1 = pow(f, exp2(-16.0f * rough - 1.0f));
    return saturate(x1 - 1.0f + ao);
}

float3 IBL_Ambient(const in float3 F, const in float3 n, const in float occlusion)
{
    float3 indirect_diffuse;
    if (bHasCubeMap) {
    	indirect_diffuse = tex_irradiance.SampleLevel(sampler_irradiance, n, 0);
    }
	else {
		indirect_diffuse = srgb_to_linear(vLightAmbient.rgb);
	}
		
    return indirect_diffuse * (1.0f - F) * occlusion;
}

float3 IBL_Specular(const in float3 f0, const in float NoV, const in float3 r, const in float occlusion, const in float roughP)
{
	//return 0.0;

	const float roughL = roughP * roughP;
	const float3 specular_occlusion = IBL_SpecularOcclusion(NoV, occlusion, roughL);

    float3 indirect_specular;
    if (bHasCubeMap) {
		const float mip = roughP * NumEnvironmentMapMipLevels;
	    indirect_specular = tex_environment.SampleLevel(sampler_environment, r, mip);
    }
	else {
		indirect_specular = srgb_to_linear(vLightAmbient.rgb);
	}
	
	const float2 lut_tex = float2(NoV, roughL);
	const float3 F = F_schlick_roughness(f0, NoV, roughL);
	const float2 env_brdf  = tex_brdf_lut.SampleLevel(sampler_brdf_lut, lut_tex, 0);
	return indirect_specular * (F * env_brdf.x + env_brdf.y) * specular_occlusion;
}


// SSS

static const float SSS_Distortion = 0.2f;
static const float SSS_Power = 1.0f;
static const float SSS_Scale = 4.0f;
static const float SSS_MinThickness = 0.0f;

float SSS_Thickness(float3 sp)
{
	const float2 xy = abs(sp.xy) - 1.0f;

	if (xy.x > 0.0f || xy.y > 0.0f || sp.z < 0.0f || sp.z > 1.0f) return 0.0f;

	sp.x = mad(0.5f, sp.x, 0.5f);
	sp.y = mad(-0.5f, sp.y, 0.5f);
	//sp.z -= vShadowMapInfo.z;

	const float d = tex_shadow.SampleLevel(sampler_sss, sp.xy, 0);
	return saturate(sp.z - d + SSS_MinThickness);
}

float SSS_Attenuation(const in float3 light_dir)
{
	const float d = length(light_dir);
    return 1.0f / (1.0f + d + d * d);
}

float SSS_Light(const in float3 normal, const in float3 view, const in float3 light_dir)
{
	//const float attenuation = SSS_Attenuation(light_dir);
	
	const float3 sss_light = light_dir + normal * SSS_Distortion;
	const float sss_dot = saturate(dot(view, -sss_light));

	return pow(sss_dot, SSS_Power) * SSS_Scale;// * attenuation;
}

float3 SSS_IBL(const in float3 view)
{
	const float3 SSS_Ambient = tex_irradiance.SampleLevel(sampler_irradiance, -view, 0);
	return SSS_Ambient * SSS_Scale;
}
