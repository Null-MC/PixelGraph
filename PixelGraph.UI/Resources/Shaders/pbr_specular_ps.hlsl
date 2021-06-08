#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/parallax.hlsl"
#include "lib/complex_numbers.hlsl"
#include "lib/pbr.hlsl"

//#define IRON float3(2.9114, 2.9497, 2.5845)
//#define GOLD float3(0.18299, 0.42108, 1.3734)
//#define ALUMINUM float3(1.3456, 0.96521, 0.61722)
//#define CHROME float3(3.1071, 3.1812, 2.3230)
//#define COPPER float3(0.27105, 0.67693, 1.3164)
//#define LEAD float3(1.9100, 1.8300, 1.4400)
//#define PLATINUM float3(2.3757, 2.0847, 1.8453)
//#define SILVER float3(0.15943, 0.14512, 0.13547)

#pragma pack_matrix( row_major )

/* TEXTURE PACKING
 *   tex_albedo_alpha
 *     r=red
 *     g=green
 *     b=blue
 *     a=alpha
 *   tex_normal_height
 *     r=normal-x
 *     g=normal-y
 *     b=normal-z
 *     a=height
 *   tex_rough_f0_occlusion
 *     r=rough
 *     g=f0
 *     b=occlusion
 *   tex_porosity_sss_emissive
 *     r=porosity
 *     g=sss
 *     b=emissive
 */

float3 hammonDiffuse(const float3 albedo, const float F0, const float nDotV, const float nDotL, const float nDotH, const float lDotV, const float roughness) {
	//My modified Hammon diffuse model.
	complexFloat3 n1;
	n1.real = float3(1.00029f, 1.00029f, 1.00029f);
	n1.imag = float3(0.0f, 0.0f, 0.0f);
	complexFloat3 n2;
	n2.real = f0ToIOR(float3(F0, F0, F0));
	n2.imag = float3(0.0f, 0.0f, 0.0f);

	float facing = 0.5 + 0.5 * lDotV;
	float rough = nDotH <= 0.0 ? 0.0 : facing * (0.9f - 0.4f * facing) * ((0.5f + nDotH) * rcp(max(nDotH, 0.05f)));
	float3 fresnel_v = 1.0f - fresnelNonPolarized(nDotV, n1, n2);
	float3 fresnel_l = 1.0f - fresnelNonPolarized(nDotV, n1, n2);
	float energyConservationFactor = 1.0f - (4.0f * sqrt(F0) + 5.0f * F0 * F0) * rcp(9.0f);
	float3 smooth_v = (fresnel_l * fresnel_v) * rcp(energyConservationFactor);
	float3 single = lerp(smooth_v, float3(rough, rough, rough), roughness) * rcp(pi);
	float multi = 0.1159f * roughness;

	return max(albedo * (single + albedo * multi) * nDotL, 0.0f);
}

float4 main(const ps_input input) : SV_TARGET
{
	const float2 parallax_tex = get_parallax_texcoord(input);
	
	const float4 albedo_alpha = tex_albedo_alpha.Sample(sampler_surface, parallax_tex);
	const float3 rough_f0_occlusion = tex_rough_f0_occlusion.Sample(sampler_surface, parallax_tex).rgb;
	const float3 porosity_sss_emissive = tex_porosity_sss_emissive.Sample(sampler_surface, parallax_tex).rgb;

	const float3 eye = normalize(input.eye.xyz);
	const float3 normal = calc_normal(parallax_tex, input.nor, input.tan, input.bin);

	const float rough = rough_f0_occlusion.r * rough_f0_occlusion.r;
	const float f0r = rough_f0_occlusion.g;
	const float occlusion = rough_f0_occlusion.b;
	const float emissive = porosity_sss_emissive.b;

	float3 lightDir = normalize(Lights[0].vLightPos.xyz - input.wp.xyz);

	const float nDotV = dot(normal, eye);
	const float nDotL = saturate(dot(normal, lightDir));
	const float nDotH = abs(dot(normal, normalize(lightDir + eye))) + 1e-5;
	const float lDotV = dot(lightDir, eye);
	
    // Burley roughness bias
	const float alpha = rough * rough;

	float3 lit = hammonDiffuse(albedo_alpha.xyz, f0r, nDotV, nDotL, nDotH, lDotV, alpha);

    if (bRenderShadowMap)
        lit *= shadow_strength(input.sp);

	//const float3 ambient = albedo_alpha.rgb * (emissive + vLightAmbient.rgb);
	
    return float4(lit, albedo_alpha.a);
}
