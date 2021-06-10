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
	const float4 sp = input.sp / input.sp.w;
	const float2 xy = abs(sp).xy - float2(1, 1);

	if (xy.x > 0 || xy.y > 0 || sp.z < 0 || sp.z > 1)
		return float4(1,0,0,1);

	const float4 diffuse_alpha = tex_diffuse_alpha.Sample(sampler_surface, input.tex);
	const float emissive = tex_emissive.Sample(sampler_surface, input.tex).r;

	float3 diffuse = diffuse_alpha.rgb;
	diffuse = srgb_to_linear(diffuse);
	
	const float3 normal = normalize(input.nor);
	const float3 eye = normalize(input.eye.xyz);
	float3 lit = light_surface(input.wp, eye, normal, diffuse);

    if (bRenderShadowMap)
        lit *= shadow_strength(input.sp);

	const float3 ambient = srgb_to_linear(vLightAmbient.rgb);
	float3 final_color = lit + diffuse * (ambient + emissive * PI);
	
	final_color = linear_to_srgb(final_color);
	
    return float4(final_color, diffuse_alpha.a);
}
