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
    const float SNoV = saturate(dot(normal, view));
	const float2 tex = get_parallax_texcoord(input.tex, input.poT, SNoV, shadow_tex);
	const float3 tex_normal = calc_tex_normal(tex, normal, tangent, bitangent);
	const pbr_material mat = get_pbr_material(tex);
	
	clip(mat.alpha - EPSILON);
	
	const float reflectance = 0.5; // 4%
	const float metal = mat.f0;

	const float roughP = max(mat.rough, MIN_ROUGH);
	const float roughL = roughP * roughP;
	//float roughL = max(mat.rough, MIN_ROUGH);

    // Blend base colors
    const float3 c_diff = lerp(mat.albedo, float3(0, 0, 0), metal);// * mat.occlusion;
    const float3 c_spec = 0.16 * reflectance * reflectance * (1.0 - metal) + mat.albedo * metal;
	
    const float NdotV = saturate(dot(tex_normal, view));
    const float3x3 mTBN = float3x3(tangent, bitangent, normal);

    float3 acc_diffuse = 0.0;
    float3 acc_specular = 0.0;
	
    for (int i = 0; i < NumLights; i++) {
        if (Lights[i].iLightType == 1) {
            // light vector (to light)
            const float3 light_dir = normalize(Lights[i].vLightDir.xyz);

        	// light parallax shadows
            const float SNoL = dot(normal, light_dir);
        	const float2 polT = get_parallax_offset(mTBN, light_dir);
            const float shadow = get_parallax_shadow(shadow_tex, polT, SNoL);
            //if (shadow < EPSILON) continue;

            // Half vector
            const float3 H = normalize(light_dir + view);

            // products
            const float NdotL = saturate(dot(tex_normal, light_dir));
            const float LdotH = saturate(dot(light_dir, H));
            const float NdotH = saturate(dot(tex_normal, H));
        	
            // Diffuse & specular factors
            const float3 light_diffuse = Diffuse_Burley(1.0, NdotL, NdotV, LdotH, roughL) * c_diff;
            const float3 light_specular = Specular_BRDF(roughL, c_spec, NdotV, NdotL, LdotH, NdotH, tex_normal, H);
        	const float3 light_factor = NdotL * Lights[i].vLightColor.rgb * shadow;
          
            acc_diffuse += light_diffuse * light_factor;
            acc_specular += light_specular * light_factor;
        }
        else if (Lights[i].iLightType == 2) {
            float3 light_dir = (Lights[i].vLightPos - input.wp).xyz; // light dir
            const float dl = length(light_dir); // light distance
            if (Lights[i].vLightAtt.w < dl) continue;

            light_dir = light_dir / dl; // normalized light dir

        	// light parallax shadows
            const float SNoL = dot(normal, light_dir);
        	const float2 polT = get_parallax_offset(mTBN, light_dir);
            const float shadow = get_parallax_shadow(shadow_tex, polT, SNoL);
            //if (shadow < EPSILON) continue;

        	const float3 H = normalize(view + light_dir); // half direction for specular

        	// products
            const float NdotL = saturate(dot(tex_normal, light_dir));
            const float LdotH = saturate(dot(light_dir, H));
            const float NdotH = saturate(dot(tex_normal, H));
        	
            // Diffuse & specular factors
            const float3 light_diffuse = c_diff * Diffuse_Burley(NdotL, NdotV);
            const float3 light_specular = Specular_BRDF(roughL, c_spec, NdotV, NdotL, LdotH, NdotH, tex_normal, H);
            const float att = 1.0f / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * dl + Lights[i].vLightAtt.z * dl * dl);

        	//acc_color = mad(att * shadow, NdotL * Lights[i].vLightColor.rgb * (diffuse + specular), acc_color);
        	acc_diffuse = mad(att * shadow, NdotL * Lights[i].vLightColor.rgb * light_diffuse, acc_diffuse);
            acc_specular = mad(att * shadow, NdotL * Lights[i].vLightColor.rgb * light_specular, acc_specular);
        }
        else if (Lights[i].iLightType == 3) {
            float3 light_dir = (Lights[i].vLightPos - input.wp).xyz; // light dir
            const float dl = length(light_dir); // light distance
            if (Lights[i].vLightAtt.w < dl) continue;
        	
            light_dir = light_dir / dl; // normalized light dir

        	// light parallax shadows
            const float SNoL = dot(normal, light_dir);
        	const float2 polT = get_parallax_offset(mTBN, light_dir);
            const float shadow = get_parallax_shadow(shadow_tex, polT, SNoL);
            //if (shadow < EPSILON) continue;
        	
            const float3 H = normalize(view + light_dir); // half direction for specular
            const float3 sd = normalize((float3) Lights[i].vLightDir); // missuse the vLightDir variable for spot-dir

            const float NdotL = saturate(dot(tex_normal, light_dir));
            const float LdotH = saturate(dot(light_dir, H));
            const float NdotH = saturate(dot(tex_normal, H));
        	
            // Diffuse & specular factors
            const float3 light_diffuse = c_diff * Diffuse_Burley(NdotL, NdotV);
            const float3 light_specular = Specular_BRDF(roughL, c_spec, NdotV, NdotL, LdotH, NdotH, tex_normal, H);

            const float rho = dot(-light_dir, sd);
            const float spot = pow(saturate((rho - Lights[i].vLightSpot.x) / (Lights[i].vLightSpot.y - Lights[i].vLightSpot.x)), Lights[i].vLightSpot.z);
            const float att = spot / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * dl + Lights[i].vLightAtt.z * dl * dl);

        	//acc_color = mad(att * shadow, NdotL * Lights[i].vLightColor.rgb * (diffuse + specular), acc_color);
            acc_diffuse = mad(att * shadow, NdotL * Lights[i].vLightColor.rgb * light_diffuse, acc_diffuse);
            acc_specular = mad(att * shadow, NdotL * Lights[i].vLightColor.rgb * light_specular, acc_specular);
        }
    }

	//const float3 ambient_color = srgb_to_linear(vLightAmbient.rgb);
	//float3 specular_env = ambient_color;

	//if (bHasCubeMap) {
 //       specular_env = specular_IBL(tex_normal, view, mat.rough);
	//}
	
	float3 ibl_ambient, ibl_specular;
	IBL(tex_normal, view, c_diff, c_spec, mat.occlusion, roughP, ibl_ambient, ibl_specular);

    //if (bRenderShadowMap)
    //    acc_color *= shadow_strength(input.sp);
	
	//const float3 lit = acc_color + c_spec * specular_env * mat.occlusion;

	//const float3 ambient = ambient_color * mat.occlusion;
	const float3 emissive = mat.emissive * mat.albedo * PI;
	
	//float3 final_color = lit + (ambient + mat.emissive * PI) * mat.albedo;
    float3 final_color = acc_diffuse + ibl_ambient + acc_specular + ibl_specular + emissive;
	float alpha = mat.alpha + lum(acc_specular + ibl_specular);

	final_color = tonemap_HejlBurgess(final_color);
	//final_color = ACESFilm(final_color);
    //final_color = linear_to_srgb(final_color);
	
    return float4(final_color, alpha);
}
