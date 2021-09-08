#define PI 3.14159265f
#define MEDIUMP_FLT_MAX    65504.0
#define saturateMediump(x) min(x, MEDIUMP_FLT_MAX)

#pragma pack_matrix(row_major)


float3 F_schlick(const in float3 f0, const in float3 fd90, const in float cos_theta)
{
    return f0 + (fd90 - f0) * pow(1.0f - cos_theta, 5.0f);
}

//https://docs.chaosgroup.com/display/OSLShaders/Complex+Fresnel+shader
float3 F_schlick_complex(const float3 n, const float3 k, const float cos)
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

float3 Diffuse_Burley(const in float NdotL, const in float NdotV, in float LdotH, in float roughP)
{
	const float fd90 = 0.5f + 2.0f * roughP * LdotH * LdotH;
    return F_schlick(1.0f, 1.0f, NdotL) * F_schlick(1.0f, fd90, NdotV);
}

float D_GGX(const in float NoH, const in float3 n, const in float3 h, const in float roughness)
{
	const float3 NxH = cross(n, h);
	const float a = NoH * roughness;
	const float k = roughness / (dot(NxH, NxH) + a * a);
	const float d = k * k * (1.0f / PI);
    return saturateMediump(d);
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

float3 Specular_BRDF(const in float3 ior_n, const in float3 ior_k, const in float3 N, const in float3 H, const in float LoH, const in float NoH, const in float VoH, const in float roughL)
{
    // Distribution
    const float D = D_GGX(NoH, N, H, roughL);//Specular_D_GGX(alpha, NdotH);

    // Fresnel
    const float3 F = F_schlick_complex(ior_n, ior_k, VoH);

    // Visibility
    const float G = G_Shlick_Smith_Hable(LoH, roughL * roughL);

    return D * F * G;
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

float3 IBL_Specular(const in float3 F, const in float NoV, const in float3 r, const in float occlusion, const in float roughP)
{
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
	
    //const float2 env_brdf  = srgb_to_linear(tex_brdf_lut.SampleLevel(sampler_brdf_lut, float2(NoV, roughL), 0));
    const float2 env_brdf  = tex_brdf_lut.SampleLevel(sampler_brdf_lut, float2(NoV, roughL), 0);
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
