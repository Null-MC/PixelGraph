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
 *     g=f0/hcm
 *     b=occlusion
 *   tex_porosity_sss_emissive
 *     r=porosity
 *     g=sss
 *     b=emissive
 */

struct pbr_material
{
	float3 albedo;
	float opacity;
	float smooth;
	float f0_hcm;
	float occlusion;
	float porosity;
	float sss;
	float emissive;
};

pbr_material get_pbr_material(const in float2 tex)
{
	const float4 albedo_opacity = tex_albedo_alpha.Sample(sampler_surface, tex);
	const float2 smooth_occlusion = tex_rough_f0_occlusion.Sample(sampler_surface, tex).rb;
	const float  f0_hcm = tex_rough_f0_occlusion.SampleLevel(sampler_surface, tex, 0).g;
	const float3 porosity_sss_emissive = tex_porosity_sss_emissive.Sample(sampler_surface, tex).rgb;

	pbr_material mat;
	mat.albedo = srgb_to_linear(albedo_opacity.rgb);
	mat.opacity = albedo_opacity.a;
	mat.smooth = smooth_occlusion.r;
    mat.f0_hcm = f0_hcm;
	mat.occlusion = smooth_occlusion.g;
	mat.porosity = porosity_sss_emissive.r;
	mat.sss = porosity_sss_emissive.g;
	mat.emissive = srgb_to_linear(porosity_sss_emissive.b);
		
    return mat;
}
