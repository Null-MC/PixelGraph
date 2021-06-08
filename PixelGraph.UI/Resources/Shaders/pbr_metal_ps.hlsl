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


float4 main(const ps_input input) : SV_TARGET
{
	const float3 eye = normalize(input.eye.xyz);
	const float2 parallax_tex = get_parallax_texcoord(input.tex, input.poT, input.nor, eye);

	const float4 albedo_alpha = tex_albedo_alpha.Sample(sampler_surface, parallax_tex);
	const float3 rough_f0_occlusion = tex_rough_f0_occlusion.Sample(sampler_surface, parallax_tex).rgb;
	const float3 porosity_sss_emissive = tex_porosity_sss_emissive.Sample(sampler_surface, parallax_tex).rgb;

	const float3 normal = calc_normal(parallax_tex, input.nor, input.tan, input.bin);

	const float rough = pow(rough_f0_occlusion.r, 2.0f);
	const float f0r = rough_f0_occlusion.g;
	const float occlusion = rough_f0_occlusion.b;
	const float emissive = porosity_sss_emissive.b;

	const float NdotV = saturate(dot(normal, eye));

	// Burley roughness bias
	const float alpha = rough * rough;

	float3 lit = float3(0.0f, 0.0f, 0.0f);

	if (bRenderShadowMap)
		lit *= shadow_strength(input.sp);

	//const float3 ambient = albedo_alpha.rgb * (emissive + vLightAmbient.rgb);

	return float4(lit, albedo_alpha.a);
}
