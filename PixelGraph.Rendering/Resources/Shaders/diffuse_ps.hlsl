#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/diffuse.hlsl"
#include "lib/tonemap.hlsl"

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

    if (BlendMode == BLEND_CUTOUT)
		clip(diffuse_alpha.a - CUTOUT_THRESHOLD);
    else if (BlendMode == BLEND_TRANSPARENT)
		clip(diffuse_alpha.a - EPSILON);

	const float3 tint = srgb_to_linear(vMaterialDiffuse.rgb);
	const float3 diffuse = srgb_to_linear(diffuse_alpha.rgb) * tint;
	
	const float3 normal = normalize(input.nor);
	const float3 eye = normalize(input.eye.xyz);
	const float3 lit = light_surface(diffuse, input.wp, normal, eye);
	const float3 ambient = get_ambient(normal);
	
	float3 final_color = lit + diffuse * (ambient + emissive * PI);

	float alpha = diffuse_alpha.a;
    if (BlendMode != BLEND_TRANSPARENT) alpha = 1.0f;

	final_color = linear_to_srgb(final_color);
	//final_color = tonemap_ACESFit2(final_color);

    return float4(final_color, alpha);
}
