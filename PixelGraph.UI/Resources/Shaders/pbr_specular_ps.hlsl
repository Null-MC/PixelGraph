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

float3 step3(float3 x, float3 y) {
	return float3(step(x.r, y.r), step(x.g, y.g), step(x.b, y.b));
}

float3 linearToSrgb(const float3 color) {
	return pow(color, 1.0f/2.2f);
}

float3 srgbToLinear(const float3 color) {
	return pow(color, 2.2f);
}

float3 diffuseIBL(const in float3 N, const in float3 V, const in float lodBias)
{
	const float3 dir = reflect(-V, N);
	const float mip = lodBias * NumEnvironmentMapMipLevels;
	return tex_cube.SampleLevel(sampler_IBL, dir, mip).rgb;
}

float radicalInverse_VdC(uint bits) {
	bits = (bits << 16u) | (bits >> 16u);
	bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
	bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
	bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
	bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
	return float(bits) * 2.3283064365386963e-10; // / 0x100000000
}

float2 hammersley2d(uint i, uint N) {
	return float2(float(i) / float(N), radicalInverse_VdC(i));
}

float3 generateUnitVector(float2 hash) {
	hash.x *= 2.0 * pi; hash.y = hash.y * 2.0 - 1.0;
	return float3(float2(sin(hash.x), cos(hash.x)) * sqrt(1.0 - hash.y * hash.y), hash.y);
}

float3 diffuseIBL(const float3 albedo, const float3 normal, const float3 eye, const float f0, const float alpha) {
	uint N = 128u;
	float3 result = float3(0.0f, 0.0f, 0.0f);
	for (uint i = 0u; i < N; ++i) {
		float3 dir = generateUnitVector(hammersley2d(i, N));

		const float nDotV = dot(normal, eye);
		const float nDotL = saturate(dot(normal, dir));
		const float nDotH = abs(dot(normal, normalize(dir + eye))) + 1e-5;
		const float lDotV = dot(dir, eye);
		const float vDotH = dot(eye, normalize(dir + eye));

		float3 diffuse = hammonDiffuse(albedo, f0, nDotV, nDotL, nDotH, lDotV, alpha);
		result += diffuse * tex_cube.Sample(sampler_IBL, dir).rgb;
	}
	return result / (float)N;
}

float3 tonemapHejlBurgess(float3 color) {
    color *= rcp(1.1f);
	color = max(float3(0.0f, 0.0f, 0.0f), color - 0.0008f);
	color = (color * (6.2f * color + 0.5f)) / (color * (6.2f * color + 1.7f) + 0.06f);

	return color;
}

float4 main(const ps_input input) : SV_TARGET
{
	const float3 eye = normalize(input.eye.xyz);
	const float2 parallax_tex = input.tex;//get_parallax_texcoord(input.tex, input.poT, input.nor, eye);
	
	const float4 albedo_alpha = tex_albedo_alpha.Sample(sampler_surface, parallax_tex);
	const float3 linearAlbedo = srgbToLinear(albedo_alpha.xyz);
	const float3 rough_f0_occlusion = tex_rough_f0_occlusion.Sample(sampler_surface, parallax_tex).rgb;
	const float3 porosity_sss_emissive = tex_porosity_sss_emissive.Sample(sampler_surface, parallax_tex).rgb;

	const float3 normal = calc_normal(parallax_tex, input.nor, input.tan, input.bin);

	const float rough = rough_f0_occlusion.r * rough_f0_occlusion.r;
	const float f0r = rough_f0_occlusion.g;
	const float occlusion = rough_f0_occlusion.b;
	const float emissive = porosity_sss_emissive.b;

	float3 lightDir = normalize(float3(0.8, 1.0, 1.0));// normalize(Lights[0].vLightPos.xyz - input.wp.xyz);

	const float nDotV = dot(normal, eye);
	const float nDotL = saturate(dot(normal, lightDir));
	const float nDotH = abs(dot(normal, normalize(lightDir + eye))) + 1e-5;
	const float lDotV = dot(lightDir, eye);
	const float vDotH = dot(eye, normalize(lightDir + eye));

	// Burley roughness bias
	const float alpha = rough * rough;

	float attenuation = 1.0 / pow(length(Lights[0].vLightPos.xyz), 2.0f);

	float3 lightColor = float3(2.0f, 2.0f, 2.0f);

	float3 lit  = lightColor * hammonDiffuse(linearAlbedo, f0r, nDotV, nDotL, nDotH, lDotV, alpha);
		   lit += lightColor * specularBRDF(nDotL, nDotV, nDotH, vDotH, f0r, alpha);
		   lit += diffuseIBL(linearAlbedo, normal, eye, f0r, alpha);

    if (bRenderShadowMap)
        lit *= shadow_strength(input.sp);

	lit = tonemapHejlBurgess(lit);
	
    return float4(lit, albedo_alpha.a);
}
