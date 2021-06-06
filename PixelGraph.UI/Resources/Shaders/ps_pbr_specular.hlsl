#include "common_funcs.hlsl"
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
 *   tex_smooth_f0_occlusion
 *     r=smooth
 *     g=f0
 *     b=occlusion
 *   tex_porosity_sss_emissive
 *     r=porosity
 *     g=sss
 *     b=emissive
 */

float4 main(const ps_input input) : SV_TARGET
{
	const float4 albedo_alpha = tex_albedo_alpha.Sample(sampler_surface, input.tex);
	
	float3 n = calcNormal(input);
	
	return albedo_alpha * float4(n, 1.0f);
}
