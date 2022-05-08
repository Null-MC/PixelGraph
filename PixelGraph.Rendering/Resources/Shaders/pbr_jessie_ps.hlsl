#define MESH

#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/complex_numbers.hlsl"
#include "lib/parallax.hlsl"
#include "lib/labPbr_material.hlsl"
#include "lib/pbr_jessie.hlsl"
#include "lib/normals.hlsl"
#include "lib/tonemap.hlsl"

#pragma pack_matrix(row_major)


float3 hammonDiffuse(const float3 albedo, const float F0, const float nDotV, const float nDotL, const float nDotH, const float lDotV, const float roughness) {
	//My modified Hammon diffuse model.
	complexFloat3 n1;
	n1.real = 1.00029f;
	n1.imag = 0.0f;
	complexFloat3 n2;
	n2.real = f0_to_ior(F0);
	n2.imag = 0.0f;

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

float3 diffuseIBL(const in float3 N, const in float3 V, const in float lodBias)
{
	const float3 dir = reflect(-V, N);
	const float mip = lodBias * NumEnvironmentMapMipLevels;
	return tex_environment.SampleLevel(sampler_environment, dir, mip).rgb;
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
		//const float vDotH = dot(eye, normalize(dir + eye));

		float3 diffuse = hammonDiffuse(albedo, f0, nDotV, nDotL, nDotH, lDotV, alpha);
		result += diffuse * tex_irradiance.Sample(sampler_irradiance, dir).rgb * pi;
	}
	return result / (float)N;
}

static const float phi1 = 1.61803398874989484820458683436563; // = phi
static const float phi2 = 1.32471795724474602596090885447809; // = plastic constant, plastic ratio, etc
static const float phi3 = 1.220744084605759475361685349108831;

float R1(float n) {
	float s0 = 0.5;
	float alpha = 1.0 / phi1;
	return frac(s0 + n * alpha);
}
float2 R2(float n) {
	float s0 = 0.5;
	float2 alpha = 1.0 / float2(phi2, phi2 * phi2);
	return frac(s0 + n * alpha);
}
float3 R3(float n) {
	float s0 = 0.5;
	float3 alpha = 1.0 / float3(phi3, phi3 * phi3, phi3 * phi3 * phi3);
	return frac(s0 + n * alpha);
}

float R2Dither(float2 xy) {
	float2 alpha = 1.0 / float2(phi2, phi2 * phi2);
	return frac(dot(xy, alpha));
}
float R2DitherContinuous(float2 xy) { // technically better but only slightly
	float z = R2Dither(xy);
	return z < 0.5 ? 2.0 * z : 2.0 - 2.0 * z;
}

float bayer2(float2 c) { c = 0.5 * floor(c); return frac(1.5 * frac(c.y) + c.x); }
float bayer4(float2 c) { return 0.25 * bayer2(0.5 * c) + bayer2(c); }
float bayer8(float2 c) { return 0.25 * bayer4(0.5 * c) + bayer2(c); }
float bayer16(float2 c) { return 0.25 * bayer8(0.5 * c) + bayer2(c); }
float bayer32(float2 c) { return 0.25 * bayer16(0.5 * c) + bayer2(c); }
float bayer64(float2 c) { return 0.25 * bayer32(0.5 * c) + bayer2(c); }
float bayer128(float2 c) { return 0.25 * bayer64(0.5 * c) + bayer2(c); }

float3 ggxDistribution(const float3 normal, float4 noise, const float alpha2) {
	noise.xyz = normalize(cross(normal, noise.xyz * 2.0 - 1.0));
	noise.w *= 0.9;
	return normalize(noise.xyz * sqrt(alpha2 * noise.w / (1.0 - noise.w)) + normal);
}

float3 specularReflection(const float dither, const float3 normal, const float3 eye, const float f0, const float alpha) {
	uint N = 64u;
	float3 result = float3(0.0f, 0.0f, 0.0f);

	int metalID = int(f0 * 255.0 - 229.5);

	float3 n = f0_to_ior(float3(f0, f0, f0));
	float3 k = float3(0.0f, 0.0f, 0.0f);

	complexFloat3 n1;
	n1.real = float3(1.00029f, 1.00029f, 1.00029f);
	n1.imag = float3(0.0f, 0.0f, 0.0f);
	complexFloat3 n2;
	n2.real = n;
	n2.imag = k;

	for (uint i = 0u; i < N; ++i) {
		float3 roughNormal = ggxDistribution(normal, float4(R3((i + dither) * 32.0), R1((i + dither) * 32.0)), alpha);

		float3 reflectionDirection = reflect(-eye, roughNormal);

		float3 fresnel = fresnelNonPolarized(dot(roughNormal, eye), n1, n2);
		float G2_over_G1 = saturate(smithGGXMaskingShadowing(dot(normal, eye), abs(dot(normal, reflectionDirection)), alpha));

		result += tex_environment.Sample(sampler_environment, reflectionDirection).rgb * fresnel * G2_over_G1;
	} result /= N;

	return result;
}

float4 main(const ps_input input) : SV_TARGET
{
	const float3 in_normal = normalize(input.nor);
	const float3 tangent = normalize(input.tan);
	const float3 bitangent = normalize(input.bin);
	const float3 view = normalize(input.eye);

	float3 shadow_tex = 0;
    float tex_depth = 0;

	const float2 tex = get_parallax_texcoord(input.tex, input.vTS, shadow_tex, tex_depth);
	
	pbr_material mat = get_pbr_material(tex);
	//mat.rough = mat.rough * mat.rough;
    const float roughP = clamp(1.0 - mat.smooth, 0.0, 1.0f);
    const float roughL = roughP * roughP;

	clip(mat.opacity - EPSILON);

    const float3x3 mTBN = float3x3(tangent, bitangent, in_normal);
	float3 normal = tex_normal_height.Sample(sampler_height, tex).xyz;
    //normal = normalize(normal * 2.0f - 1.0f);
    normal = decodeNormal(normal);
	normal = mul(normal, mTBN);

	const float3 lightDir = SunDirection;

	const float nDotV = dot(normal, view);
	const float nDotL = saturate(dot(normal, lightDir));
	const float nDotH = abs(dot(normal, normalize(lightDir + view))) + 1e-5;
	const float lDotV = dot(lightDir, view);
	const float vDotH = dot(view, normalize(lightDir + view));

	// Burley roughness bias
	const float alpha = roughL * roughL;

	//float attenuation = 1.0 / pow(length(Lights[0].vLightPos.xyz), 2.0f);

	const float3 lightColor = 2.0f;

	const float3 BRDF = specularBRDF(nDotL, nDotV, nDotH, vDotH, mat.f0_hcm, alpha);

	float3 lit  = lightColor * hammonDiffuse(mat.albedo, mat.f0_hcm, nDotV, nDotL, nDotH, lDotV, alpha);
		   lit += pi * diffuseIBL(mat.albedo, normal, view, mat.f0_hcm, alpha);
		   lit += lightColor * BRDF;
		   lit += specularReflection(bayer32(input.tex * 1920.0), normal, view, mat.f0_hcm, alpha);

    //if (bRenderShadowMap)
    //    lit *= shadow_strength(input.sp);

	//lit = tonemap_HejlBurgess(lit);
	//lit = tonemap_Uncharted2(lit);
	//lit = linear_to_srgb(lit);
	lit = apply_tonemap(lit);
	
    return float4(lit, mat.opacity);
}
