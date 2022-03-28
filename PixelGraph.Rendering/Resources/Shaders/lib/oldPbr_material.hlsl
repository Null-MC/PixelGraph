#pragma pack_matrix(row_major)


/* TEXTURE PACKING
 *   tex_albedo_alpha
 *     r=red
 *     g=green
 *     b=blue
 *     a=opacity
 *   tex_normal_height
 *     r=normal-x
 *     g=normal-y
 *     b=normal-z
 *     a=height
 *   tex_rough_f0_occlusion
 *     r=smooth
 *     g=metal
 *     b=emissive
 *   tex_porosity_sss_emissive
 */

struct pbr_material
{
	float3 diffuse;
	float opacity;
	float smooth;
	float metal;
	float emissive;
};

pbr_material get_pbr_material(const in float2 tex)
{
	const float4 albedo_opacity = tex_albedo_alpha.Sample(sampler_surface, tex);
	const float2 smooth_emissive = tex_rough_f0_occlusion.Sample(sampler_surface, tex).rb;
	const float  metal = tex_rough_f0_occlusion.SampleLevel(sampler_surface, tex, 0).g;

	pbr_material mat;
	mat.diffuse = srgb_to_linear(albedo_opacity.rgb);
	mat.opacity = albedo_opacity.a;
	mat.smooth = smooth_emissive.r;
    mat.metal = metal;
	mat.emissive = srgb_to_linear(smooth_emissive.g);

    return mat;
}
