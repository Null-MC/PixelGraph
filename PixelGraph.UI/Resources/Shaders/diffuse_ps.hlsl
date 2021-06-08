#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/diffuse.hlsl"

#pragma pack_matrix(row_major)

/* TEXTURE PACKING
 *   tex_albedo_alpha
 *     r=red
 *     g=green
 *     b=blue
 *     a=alpha
 */


float4 main(const ps_input input) : SV_TARGET
{
	const float4 diffuse_alpha = tex_diffuse_alpha.Sample(sampler_surface, input.tex);
	const float emissive = tex_emissive.Sample(sampler_surface, input.tex).r;
	
	const float3 normal = normalize(input.nor);
	const float3 eye = normalize(input.eye.xyz);
	float3 lit = light_surface(input.wp, eye, normal, diffuse_alpha);
	
    if (bRenderShadowMap)
        lit *= shadow_strength(input.sp);
		
	const float3 ambient = diffuse_alpha.rgb * (vLightAmbient.rgb + emissive);
	
    return float4(ambient + lit, diffuse_alpha.a);
}
