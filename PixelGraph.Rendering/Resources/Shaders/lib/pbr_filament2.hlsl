#define PI 3.14159265f
#define F0_WATER 0.02f
#define WATER_ROUGH 0.014f
#define WATER_BLUR 1.2f

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

// https://seblagarde.wordpress.com/2013/03/19/water-drop-3a-physically-based-wet-surfaces/
// Eta is relative IOR : N2/N1 with to / From
float F_full(const in float eta, const in float VoH)
{
	const float temp = eta * eta + VoH * VoH - 1.0f;

	if (temp < 0.0f) return 1.0f;
	const float g = sqrt(temp);

	return 0.5f * pow2((g - VoH) / (g + VoH)) * (1.0f + pow2(((g + VoH) * VoH - 1.0f) / ((g - VoH) * VoH + 1.0f)));
}

float3 F_conductor(const in float VoH, const in float n1, const in float3 n2, const in float3 k)
{
	const float3 eta = n2 / n1;
	const float3 eta_k = k / n1;

	const float cos_theta2 = VoH * VoH;
	const float sin_theta2 = 1.0f - cos_theta2;
	const float3 eta2 = eta * eta;
	const float3 eta_k2 = eta_k * eta_k;

	const float3 t0 = eta2 - eta_k2 - sin_theta2;
	const float3 a2_plus_b2 = sqrt(t0 * t0 + 4.0f * eta2 * eta_k2);
	const float3 t1 = a2_plus_b2 + cos_theta2;
	const float3 a = sqrt(0.5f * (a2_plus_b2 + t0));
	const float3 t2 = 2.0f * a * VoH;
	const float3 rs = (t1 - t2) / (t1 + t2);

	const float3 t3 = cos_theta2 * a2_plus_b2 + sin_theta2 * sin_theta2;
	const float3 t4 = t2 * sin_theta2;
	const float3 rp = rs * (t3 - t4) / (t3 + t4);

	return 0.5f * (rp + rs);
}

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

float specular_brdf(const in float eta, const in float LoH, const in float NoH, const in float VoH, const in float rough)
{
    // Fresnel
    const float F = F_full(eta, VoH);

    // Distribution
    const float D = D_ggx(NoH, rough);

    // Geometric Visibility
    const float G = G_Shlick_Smith_Hable(LoH, rough);

	return D * F * G;
}

float3 specular_brdf_conductor(const in float ior_n1, const in float3 ior_n2, const in float3 ior_k, const in float LoH, const in float NoH, const in float VoH, const in float rough)
{
    // Fresnel
    const float3 F = F_conductor(VoH, ior_n1, ior_n2, ior_k);

    // Distribution
    const float D = D_ggx(NoH, rough);

    // Geometric Visibility
    const float G = G_Shlick_Smith_Hable(LoH, rough);

	return D * F * G;
}

float Diffuse_Burley(const in float NoL, const in float NoV, const in float LoH, const in float rough)
{
	const float f90 = 0.5f + 2.0f * rough * LoH * LoH;
	const float light_scatter = F_schlick(1.0f, f90, NoL);
	const float view_scatter = F_schlick(1.0f, f90, NoV);
	return light_scatter * view_scatter * InvPI;
}


// IBL

float IBL_SpecularOcclusion(float NoV, float ao, float rough)
{
	const float f = saturate(NoV + ao);
	const float x1 = pow(f, exp2(-16.0f * rough - 1.0f));
    return saturate(x1 - 1.0f + ao);
}

float3 IBL_ambient(const in float3 F, const in float3 reflect)
{
    float3 irradiance = EnableAtmosphere
		? tex_irradiance.SampleLevel(sampler_irradiance, reflect, 0)
		: srgb_to_linear(vLightAmbient.rgb);

    return irradiance * (1.0f - F);// * InvPI;
}

float3 IBL_specular(const in float3 F, const in float NoV, const in float3 r, const in float occlusion, const in float roughP)
{
	//return 0.0;

	const float roughL = roughP * roughP;
	const float3 specular_occlusion = IBL_SpecularOcclusion(NoV, occlusion, roughL);

    float3 indirect_specular;
    if (EnableAtmosphere) {
		//const float roughP = sqrt(roughL);
		const float mip = roughP * NumEnvironmentMapMipLevels;
	    indirect_specular = tex_environment.SampleLevel(sampler_environment, r, mip);
    }
	else {
		indirect_specular = srgb_to_linear(vLightAmbient.rgb);
	}
	
	const float2 lut_tex = float2(NoV, roughP);
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
	sp.z -= vShadowMapInfo.z;

	//const float d = tex_shadow.SampleLevel(sampler_light, sp.xy, 0);
	float sum = 0;
    const float range = 1.5;
    const float2 scale = rcp(vShadowMapSize);
    float x, y;
	float2 tex;

	[unroll]
    for (y = -range; y <= range; y += 1.0f) {
		[unroll]
	    for (x = -range; x <= range; x += 1.0f) {
			tex = sp.xy + float2(x, y) * scale;
		    sum += tex_shadow.SampleLevel(sampler_light, tex, 0);
	    }
    }

	return saturate(sp.z - sum * 0.0625f + SSS_MinThickness);
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
	const float3 SSS_Ambient = EnableAtmosphere
		? tex_irradiance.SampleLevel(sampler_irradiance, -view, 0)
		: srgb_to_linear(vLightAmbient.rgb);

	return SSS_Ambient * SSS_Scale;
}
