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

	float2 shadow_tex = 0;
    float shadow_depth = 0;
    float tex_depth = 0;

    const float SNoV = saturate(dot(normal, view));
	const float2 tex = get_parallax_texcoord(input.tex, input.poT, SNoV, shadow_tex, shadow_depth, tex_depth);
	const float3 tex_normal = calc_tex_normal(tex, normal, tangent, bitangent);
	const pbr_material mat = get_pbr_material(tex);
	
	clip(mat.alpha - EPSILON);
	
	const float reflectance = 0.5; // 4%
	const float metal = mat.f0;

	const float roughP = max(mat.rough, MIN_ROUGH);
	const float roughL = roughP * roughP;

    // Blend base colors
	const float3 tint = srgb_to_linear(vMaterialDiffuse.rgb);
    const float3 c_diff = lerp(mat.albedo * tint, 0.0f, metal);
    const float3 c_spec = 0.16 * reflectance * reflectance * (1.0 - metal) + mat.albedo * metal;
	
    const float NdotV = saturate(dot(tex_normal, view));
    const float3x3 mTBN = float3x3(tangent, bitangent, normal);

    float3 acc_diffuse = 0.0;
    float3 acc_specular = 0.0;
	
    for (int i = 0; i < NumLights; i++) {
        const float3 light_color = srgb_to_linear(Lights[i].vLightColor.rgb) * 1.6f;
    	
        if (Lights[i].iLightType == 1) {
            const float3 light_dir = normalize(Lights[i].vLightDir.xyz);

        	// light parallax shadows
            const float SNoL = dot(normal, light_dir);
        	const float2 polT = get_parallax_offset(mTBN, light_dir, input.tex_max - input.tex_min);
            const float shadow = get_parallax_shadow(shadow_tex, shadow_depth, polT, SNoL);

            const float3 H = normalize(light_dir + view);
            const float NdotL = saturate(dot(tex_normal, light_dir));
            const float LdotH = saturate(dot(light_dir, H));
            const float NdotH = saturate(dot(tex_normal, H));
        	
            // Diffuse & specular factors
            const float3 light_diffuse = Diffuse_Burley(1.0, NdotL, NdotV, LdotH, roughL) * c_diff;
            const float3 light_specular = Specular_BRDF(roughL, c_spec, NdotV, NdotL, LdotH, NdotH, tex_normal, H);
        	const float3 light_factor = NdotL * light_color * shadow;
          
            acc_diffuse += light_diffuse * light_factor;
            acc_specular += light_specular * light_factor;
        }
        else if (Lights[i].iLightType == 2) {
            float3 light_dir = (Lights[i].vLightPos - input.wp).xyz;
            const float light_dist = length(light_dir);
            if (Lights[i].vLightAtt.w < light_dist) continue;
            light_dir = light_dir / light_dist;

        	// light parallax shadows
            const float SNoL = dot(normal, light_dir);
        	const float2 polT = get_parallax_offset(mTBN, light_dir, input.tex_max - input.tex_min);
            const float shadow = get_parallax_shadow(shadow_tex, shadow_depth, polT, SNoL);

        	const float3 H = normalize(view + light_dir);
            const float NdotL = saturate(dot(tex_normal, light_dir));
            const float LdotH = saturate(dot(light_dir, H));
            const float NdotH = saturate(dot(tex_normal, H));
        	
            // Diffuse & specular factors
            const float3 light_diffuse = c_diff * Diffuse_Burley(NdotL, NdotV);
            const float3 light_specular = Specular_BRDF(roughL, c_spec, NdotV, NdotL, LdotH, NdotH, tex_normal, H);
            const float att = 1.0f / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * light_dist + Lights[i].vLightAtt.z * light_dist * light_dist);

        	acc_diffuse = mad(att * shadow, NdotL * light_color * light_diffuse, acc_diffuse);
            acc_specular = mad(att * shadow, NdotL * light_color * light_specular, acc_specular);
        }
        else if (Lights[i].iLightType == 3) {
            float3 light_dir = (Lights[i].vLightPos - input.wp).xyz;
            const float light_dist = length(light_dir);
            if (Lights[i].vLightAtt.w < light_dist) continue;
            light_dir = light_dir / light_dist;

        	// light parallax shadows
            const float SNoL = dot(normal, light_dir);
        	const float2 polT = get_parallax_offset(mTBN, light_dir, input.tex_max - input.tex_min);
            const float shadow = get_parallax_shadow(shadow_tex, shadow_depth, polT, SNoL);
        	
            const float3 H = normalize(view + light_dir);
            const float NdotL = saturate(dot(tex_normal, light_dir));
            const float LdotH = saturate(dot(light_dir, H));
            const float NdotH = saturate(dot(tex_normal, H));
        	
            const float3 sd = normalize((float3) Lights[i].vLightDir); // missuse the vLightDir variable for spot-dir
        	
            // Diffuse & specular factors
            const float3 light_diffuse = c_diff * Diffuse_Burley(NdotL, NdotV);
            const float3 light_specular = Specular_BRDF(roughL, c_spec, NdotV, NdotL, LdotH, NdotH, tex_normal, H);

            const float rho = dot(-light_dir, sd);
            const float spot = pow(saturate((rho - Lights[i].vLightSpot.x) / (Lights[i].vLightSpot.y - Lights[i].vLightSpot.x)), Lights[i].vLightSpot.z);
            const float att = spot / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * light_dist + Lights[i].vLightAtt.z * light_dist * light_dist);

            acc_diffuse = mad(att * shadow, NdotL * light_color * light_diffuse, acc_diffuse);
            acc_specular = mad(att * shadow, NdotL * light_color * light_specular, acc_specular);
        }
    }

	float3 ibl_ambient, ibl_specular;
	IBL(tex_normal, view, c_diff, c_spec, mat.occlusion, roughP, ibl_ambient, ibl_specular);

	const float3 emissive = mat.emissive * mat.albedo * PI;
	
    float3 final_color = ibl_ambient + acc_diffuse + acc_specular + ibl_specular + emissive;
	float alpha = mat.alpha + lum(acc_specular + ibl_specular);

	final_color = tonemap_AcesFilm(final_color);
    final_color = linear_to_srgb(final_color);
	//final_color = tonemap_HejlBurgess(final_color);
	
    return float4(final_color, alpha);
}
