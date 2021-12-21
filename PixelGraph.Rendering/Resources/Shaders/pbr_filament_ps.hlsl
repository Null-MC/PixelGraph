#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/parallax.hlsl"
#include "lib/pbr_material.hlsl"
#include "lib/pbr_filament.hlsl"
#include "lib/tonemap.hlsl"

#define MIN_ROUGH 0.002

#pragma pack_matrix(row_major)


float4 main(const ps_input input) : SV_TARGET
{
	const float3 normal = normalize(input.nor);
    const float3 tangent = normalize(input.tan);
    const float3 bitangent = normalize(input.bin);
	const float3 view = normalize(input.eye.xyz);

	float3 shadow_tex = 0;
    float tex_depth = 0;

    const float SNoV = saturate(dot(normal, view));
	const float2 tex = get_parallax_texcoord(input.tex, input.poT, SNoV, shadow_tex, tex_depth);
	const float3 src_normal = calc_tex_normal(tex, normal, tangent, bitangent);
	const pbr_material mat = get_pbr_material(tex);
	
    if (BlendMode == BLEND_CUTOUT)
		clip(mat.alpha - CUTOUT_THRESHOLD);
    else if (BlendMode == BLEND_TRANSPARENT)
		clip(mat.alpha - EPSILON);

    //-- Slope Normals --
    float3 tex_normal = src_normal;
    const float slope_depth = tex_depth - shadow_tex.z;
    if (EnableSlopeNormals && !EnableLinearSampling && slope_depth > EPSILON)
        tex_normal = get_slope_normal(tex, view, tangent, bitangent);

	const float reflectance = 0.5; // 4%
	const float metal = mat.f0;
	const float roughP = max(mat.rough, MIN_ROUGH);
	const float roughL = roughP * roughP;

    // Blend base colors
	const float3 tint = srgb_to_linear(vMaterialDiffuse.rgb);
    const float3 c_diff = lerp(mat.albedo * tint, 0.0f, metal);
    const float3 c_spec = 0.16 * reflectance * reflectance * (1.0 - metal) + mat.albedo * metal;
	
    const float NoV = saturate(dot(tex_normal, view));
    const float3x3 mTBN = float3x3(tangent, bitangent, normal);
	const float4x4 mShadowViewProj = vLightView * vLightProjection;

    const float pom_depth = 1.0f - tex_depth;// abs((tex - input.tex) / input.poT);
    const float3 pom_wp = input.wp.xyz + pom_depth * -view * CUBE_SIZE * ParallaxDepth;

    float3 acc_light = 0.0;
    float spec_strength = 0.0f;

    float light_att, light_shadow, NoL;
    float3 light_dir, light_color, light_diffuse, light_specular, H;
    float LoH, NoH; // VoH;

    for (int i = 0; i < NumLights; i++) {
        light_color = srgb_to_linear(Lights[i].vLightColor.rgb) * 1.6f;
        light_shadow = 1.0f;
    	
        if (Lights[i].iLightType == 1) {
            light_dir = normalize(Lights[i].vLightDir.xyz);
            light_att = 1.0f;

            if (bHasShadowMap && bRenderShadowMap) {
	            float d = dot(light_dir, normal);

	            if (d > 0) {
					const float4 sp = mul(float4(pom_wp, input.wp.w), mShadowViewProj);
	                light_shadow = shadow_strength(sp.xyz / sp.w);
	            }
                else {
	                light_shadow = 0.0f;
                }
			}
        }
        else if (Lights[i].iLightType == 2) {
            light_dir = Lights[i].vLightPos.xyz - input.wp.xyz;
            const float light_dist = length(light_dir);
            light_dir = light_dir / light_dist;

        	light_att = rcp(Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * light_dist + Lights[i].vLightAtt.z * light_dist * light_dist);
        }
        else if (Lights[i].iLightType == 3) {
            light_dir = Lights[i].vLightPos.xyz - input.wp.xyz;
        	
            const float light_dist = length(light_dir);

            light_dir = light_dir / light_dist;

            const float3 sd = normalize(Lights[i].vLightDir.xyz);
            const float rho = dot(-light_dir, sd);
            const float spot = pow(saturate((rho - Lights[i].vLightSpot.x) / (Lights[i].vLightSpot.y - Lights[i].vLightSpot.x)), Lights[i].vLightSpot.z);
            light_att = spot / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * light_dist + Lights[i].vLightAtt.z * light_dist * light_dist);
        }

        // light parallax shadows
        const float SNoL = dot(normal, light_dir);
        const float3 lightT = mul(mTBN, light_dir);
        const float2 polT = get_parallax_offset(lightT, input.tex_max - input.tex_min);
        light_shadow *= get_parallax_shadow(shadow_tex, polT, SNoL);
        //if (light_shadow < EPSILON) continue;

        NoL = saturate(dot(tex_normal, light_dir));
        H = normalize(view + light_dir);
        LoH = saturate(dot(light_dir, H));
        //VoH = saturate(dot(view, H));
		NoH = saturate(dot(tex_normal, H));

        // Diffuse & specular factors
        light_diffuse = Diffuse_Burley(NoL, NoV) * c_diff; // TODO: missing LoH, roughL
		light_specular = Specular_BRDF(roughL, c_spec, NoV, NoL, LoH, NoH, tex_normal, H);

        const float3 light_factor = light_shadow * NoL * light_color * light_att;
		acc_light += light_factor * (light_diffuse + light_specular);
        spec_strength += lum(light_factor * light_specular);
    }

	float3 ibl_ambient, ibl_specular;
	IBL(tex_normal, view, c_diff, c_spec, mat.occlusion, roughP, ibl_ambient, ibl_specular);

    spec_strength += lum(ibl_specular);

	const float3 emissive = mat.emissive * mat.albedo * PI;
	
    float3 final_color = ibl_ambient + acc_light + ibl_specular + emissive;

	float alpha = mat.alpha + spec_strength;
    if (BlendMode != BLEND_TRANSPARENT) alpha = 1.0f;

	//final_color = tonemap_AcesFilm(final_color);
 //   final_color = linear_to_srgb(final_color);
	final_color = tonemap_HejlBurgess(final_color);
	
    return float4(final_color, alpha);
}
