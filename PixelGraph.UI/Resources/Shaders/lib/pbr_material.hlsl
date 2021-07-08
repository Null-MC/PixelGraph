#pragma pack_matrix(row_major)


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

struct pbr_material
{
	float alpha;
	float3 albedo;
	float rough;
	float f0;
	float occlusion;
	float porosity;
	float sss;
	float emissive;
};

pbr_material get_pbr_material(const in float2 tex)
{
	const float4 albedo_alpha = tex_albedo_alpha.Sample(sampler_surface, tex);
	const float2 rough_occlusion = tex_rough_f0_occlusion.Sample(sampler_surface, tex).rb;
	const float  f0 = tex_rough_f0_occlusion.SampleLevel(sampler_surface, tex, 0).g;
	const float3 porosity_sss_emissive = tex_porosity_sss_emissive.Sample(sampler_surface, tex).rgb;

	pbr_material mat;
	mat.albedo = srgb_to_linear(albedo_alpha.rgb);
	mat.alpha = albedo_alpha.a;
	mat.rough = rough_occlusion.r;
    mat.f0 = f0;
	mat.occlusion = srgb_to_linear(rough_occlusion.g);
	mat.porosity = srgb_to_linear(porosity_sss_emissive.r);
	mat.sss = srgb_to_linear(porosity_sss_emissive.g);
	mat.emissive = srgb_to_linear(porosity_sss_emissive.b);
		
    return mat;
}
