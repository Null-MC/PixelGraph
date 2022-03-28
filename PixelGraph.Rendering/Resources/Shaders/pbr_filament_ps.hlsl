#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/parallax.hlsl"
#include "lib/oldPbr_material.hlsl"
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

    const float3x3 mTBN = float3x3(tangent, bitangent, normal);

	float3 shadow_tex = 0;
    float tex_depth = 0;

    const float SNoV = saturate(dot(normal, view));
	const float2 tex = get_parallax_texcoord(input.tex, input.vTS, SNoV, shadow_tex, tex_depth);
	const float3 src_normal = calc_tex_normal(tex, normal, tangent, bitangent);
	const pbr_material mat = get_pbr_material(tex);
	
    if (BlendMode == BLEND_CUTOUT)
		clip(mat.opacity - CUTOUT_THRESHOLD);
    else if (BlendMode == BLEND_TRANSPARENT)
		clip(mat.opacity - EPSILON);

    //-- Slope Normals --
    float3 tex_normal = src_normal;
    if (EnableSlopeNormals && !EnableLinearSampling) {
		if (tex_depth - shadow_tex.z > 0.002f) {
			const float3 slope = apply_slope_normal(tex, input.vTS, shadow_tex.z);
			tex_normal = mul(slope, mTBN);
        }
    }

	const float reflectance = 0.5f; // 4%
	//const float metal = mat.metal;
    const float roughP = clamp(1.0 - mat.smooth, MIN_ROUGH, 1.0f);
    const float roughL = roughP * roughP;

	//const float roughP = max(mat.rough, MIN_ROUGH);
	//const float roughL = roughP * roughP;

    // Blend base colors
	const float3 tint = srgb_to_linear(vMaterialDiffuse.rgb);
    const float3 c_diff = lerp(mat.diffuse * tint, 0.0f, mat.metal);
    const float3 c_spec = 0.16 * reflectance * reflectance * (1.0 - mat.metal) + mat.diffuse * mat.metal;
	
    const float NoV = saturate(dot(tex_normal, view));
	//const float4x4 mShadowViewProj = vLightView * vLightProjection;

	const float pom_depth = (1.0f - tex_depth) / max(SNoV, EPSILON) * BLOCK_SIZE * input.pDepth;
    const float3 pom_wp = input.wp.xyz - pom_depth * view;

    float3 acc_light = 0.0;
    float spec_strength = 0.0f;

    float light_att, NoL; //light_shadow
    float3 light_dir, light_color, light_diffuse, light_specular, H;
    float LoH, NoH; // VoH;

	const float4x4 mShadowViewProj = mul(vLightView, vLightProjection);
	//const float4 sp = mul(float4(pom_wp, 1), mShadowViewProj);

    [loop]
    for (int i = 0; i < NumLights; i++) {
        light_color = srgb_to_linear(Lights[i].vLightColor.rgb);
        //light_shadow = 1.0f;
        light_att = 1.0f;
    	
        if (Lights[i].iLightType == 1) {
            light_color *= 4.0f;
            light_dir = normalize(Lights[i].vLightDir.xyz);
        }
        else {
            light_dir = Lights[i].vLightPos.xyz - pom_wp;
            const float light_dist = length(light_dir);
            light_dir /= light_dist;

			if (Lights[i].iLightType == 2) {
        		light_att = rcp(Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * light_dist + Lights[i].vLightAtt.z * light_dist * light_dist);
	        }
	        else if (Lights[i].iLightType == 3) {
	            const float3 sd = normalize(Lights[i].vLightDir.xyz);
	            const float rho = dot(-light_dir, sd);
	            const float spot = pow(saturate((rho - Lights[i].vLightSpot.x) / (Lights[i].vLightSpot.y - Lights[i].vLightSpot.x)), Lights[i].vLightSpot.z);
	            light_att = spot / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * light_dist + Lights[i].vLightAtt.z * light_dist * light_dist);
	        }
        }

        // light parallax shadows
        const float surface_NoL = dot(normal, light_dir);
        const float3 lightT = mul(mTBN, light_dir);
        const float2 polT = get_parallax_offset(lightT) * input.pDepth;
        light_att *= get_parallax_shadow(shadow_tex, polT, surface_NoL);
        //if (light_shadow < EPSILON) continue;
        
        if ((Lights[i].iLightType == 1 && EnableAtmosphere) || (Lights[i].iLightType == 3 && !EnableAtmosphere)) {
            if (surface_NoL > 0) {
			    float h2 = 1.0f - shadow_tex.z;
			    float light_trace_dist = h2 / surface_NoL * BLOCK_SIZE * input.pDepth;
			    const float3 shadow_wp = pom_wp + light_trace_dist * light_dir;
				const float4 shadow_sp = mul(float4(shadow_wp, 1), mShadowViewProj);

                light_att *= shadow_strength(shadow_sp.xyz / shadow_sp.w);
            }
            else {
                light_att = 0.0f;
            }
        }
        
        NoL = saturate(dot(tex_normal, light_dir));
        H = normalize(view + light_dir);
        LoH = saturate(dot(light_dir, H));
        //VoH = saturate(dot(view, H));
		NoH = saturate(dot(tex_normal, H));

        // Diffuse & specular factors
        light_diffuse = Diffuse_Burley(NoL, NoV, LoH, roughL) * c_diff;
		light_specular = Specular_BRDF(roughL, c_spec, NoV, NoL, LoH, NoH, tex_normal, H);

        const float3 light_factor = NoL * light_color * light_att;
		acc_light += light_factor * (light_diffuse + light_specular);
        spec_strength += luminance(light_factor * light_specular);
    }

	float3 ibl_ambient, ibl_specular;
	IBL(tex_normal, view, c_diff, c_spec, 1.0f, roughP, ibl_ambient, ibl_specular);

    spec_strength += luminance(ibl_specular);

	const float3 emissive = mat.emissive * mat.diffuse * PI;
	
    float3 final_color = ibl_ambient + acc_light + ibl_specular + emissive;

	float alpha = mat.opacity + spec_strength;
    if (BlendMode != BLEND_TRANSPARENT) alpha = 1.0f;

	//final_color = tonemap_ACESFit2(final_color);
	final_color = linear_to_srgb(final_color);
	//final_color = tonemap_Reinhard(final_color);
	
    return float4(final_color, alpha);
}
