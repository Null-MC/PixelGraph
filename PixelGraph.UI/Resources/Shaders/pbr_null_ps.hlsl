#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/parallax.hlsl"
#include "lib/pbr_material.hlsl"
#include "lib/pbr_filament2.hlsl"
#include "lib/pbr_hcm.hlsl"
#include "lib/tonemap.hlsl"

#define MIN_ROUGH 0.002f
#define WATER_ROUGH 0.02f
#define WET_DARKEN 0.88f

#pragma pack_matrix(row_major)


float4 main(const ps_input input) : SV_TARGET
{
	const float3 normal = normalize(input.nor);
    const float3 tangent = normalize(input.tan);
    const float3 bitangent = normalize(input.bin);
	const float3 view = normalize(input.eye);

	float3 shadow_tex = 0;
    const float SNoV = saturate(dot(normal, view));
	const float2 tex = get_parallax_texcoord(input.tex, input.poT, SNoV, shadow_tex);
	float3 tex_normal = calc_tex_normal(tex, normal, tangent, bitangent);
	const pbr_material mat = get_pbr_material(tex);
	
	clip(mat.alpha - EPSILON);

    const float3x3 mTBN = float3x3(tangent, bitangent, normal);
	float roughL = max(mat.rough * mat.rough, MIN_ROUGH);
	
    // Blend base colors
	const float metal = mat.f0 > 0.9f ? 1.0f : 0.0f;
	const float3 tint = srgb_to_linear(vMaterialDiffuse.rgb);
    float3 diffuse = lerp(mat.albedo * tint, 0.0f, metal);

	// Wetness
    float p_diffuse = 1.0f - WET_DARKEN * mat.porosity;
	diffuse *= lerp(1.0f, p_diffuse, Wetness);

    float surface_water = saturate(Wetness * (2.0f - roughL) * (1.0f - mat.porosity));
	roughL = lerp(roughL, WATER_ROUGH, surface_water);
	float roughP = sqrt(roughL);
	//tex_normal = normalize(lerp(tex_normal, normal, surface_water * 0.5));

    //const float f0r = lerp(mat.f0, 0.02, surface_water * (1.0 - metal));
        
    // HCM
	float3 ior_n, ior_k;
	get_hcm_ior(mat.f0, ior_n, ior_k);
    //float3 metal_f0 = get_hcm_f0(mat.f0) * mat.albedo;
	float3 metal_albedo = lerp(1.0f, mat.albedo * tint, metal);
	
	//const float3 f0 = f0r * (1.0 - metal) + metal_f0 * metal;
    const float NoV = saturate(dot(tex_normal, view));
    
    float3 acc_diffuse = 0.0;
    float3 acc_specular = 0.0;
    float3 acc_sss = 0.0;
		
	[loop]
    for (int i = 0; i < NumLights; i++) {
        const float3 light_color = srgb_to_linear(Lights[i].vLightColor.rgb);
		//const float thickness = SSS_Thickness(input.sp.xyz / input.sp.w);
    	
        if (Lights[i].iLightType == 1) {
            // light vector (to light)
            const float3 light_dir = normalize(Lights[i].vLightDir.xyz);
        	
            //acc_sss += SSS_Light(tex_normal, view, light_dir) * (1.0f - thickness) * light_color;

        	// light parallax shadows
        	const float SNoL = dot(normal, light_dir);
        	const float2 polT = get_parallax_offset(mTBN, light_dir);
            const float shadow = get_parallax_shadow(shadow_tex, polT, SNoL);
            if (shadow < EPSILON) continue;
        	
            // Half vector
            const float3 H = normalize(light_dir + view);

            // products
            const float NoL = saturate(dot(tex_normal, light_dir));
            const float LoH = saturate(dot(light_dir, H));
            const float NoH = saturate(dot(tex_normal, H));
            const float VoH = saturate(dot(view, H));
        	
            // Diffuse & specular factors
            //const float3 light_diffuse = disney_diffuse(diffuse, f0, roughL, NoV, NoL, LoH);
            const float3 light_diffuse = Diffuse_Burley(NoL, NoV, LoH, roughP);
            const float3 light_specular = Specular_BRDF(ior_n, ior_k, tex_normal, H, LoH, NoH, VoH, roughL);
        	const float3 light_factor = NoL * light_color * shadow;
        	
            acc_diffuse += light_diffuse * light_factor;
            acc_specular += light_specular * light_factor;
        }
        else if (Lights[i].iLightType == 2) {
            float3 light_dir = Lights[i].vLightPos.xyz - input.wp.xyz; // light dir
            const float dl = length(light_dir); // light distance
            light_dir = light_dir / dl; // normalized light dir
        	
            //acc_sss += SSS_Light(tex_normal, view, light_dir) * (1.0f - thickness) * light_color;
        	
            if (Lights[i].vLightAtt.w < dl) continue;

        	// light parallax shadows
        	const float SNoL = dot(normal, light_dir);
        	const float2 polT = get_parallax_offset(mTBN, light_dir);
            const float shadow = get_parallax_shadow(shadow_tex, polT, SNoL);
            if (shadow < EPSILON) continue;
        	
            const float3 H = normalize(view + light_dir); // half direction for specular

        	// products
            const float NoL = saturate(dot(tex_normal, light_dir));
            const float LoH = saturate(dot(light_dir, H));
            const float NoH = saturate(dot(tex_normal, H));
            const float VoH = saturate(dot(view, H));
        	
            // Diffuse & specular factors
            //const float3 light_diffuse = disney_diffuse(diffuse, f0, roughL, NoV, NoL, LoH);
            const float3 light_diffuse = Diffuse_Burley(NoL, NoV, LoH, roughP);
            const float3 light_specular = Specular_BRDF(ior_n, ior_k, tex_normal, H, LoH, NoH, VoH, roughL);
        	const float att = 1.0f / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * dl + Lights[i].vLightAtt.z * dl * dl);

        	acc_diffuse = mad(att * shadow, NoL * light_color * light_diffuse, acc_diffuse);
            acc_specular = mad(att * shadow, NoL * light_color * light_specular, acc_specular);
        }
        else if (Lights[i].iLightType == 3) {
            float3 light_dir = Lights[i].vLightPos.xyz - input.wp.xyz; // light dir
        	
            //acc_sss += SSS_Light(tex_normal, view, light_dir) * (1.0f - thickness) * light_color;
        	
            const float dl = length(light_dir); // light distance
            if (Lights[i].vLightAtt.w < dl) continue;
        	
            light_dir = light_dir / dl; // normalized light dir

        	// light parallax shadows
        	const float SNoL = dot(normal, light_dir);
        	const float2 polT = get_parallax_offset(mTBN, light_dir);
            const float shadow = get_parallax_shadow(shadow_tex, polT, SNoL);
            if (shadow < EPSILON) continue;
        	
            const float3 H = normalize(view + light_dir); // half direction for specular
            const float3 sd = normalize(Lights[i].vLightDir.xyz); // missuse the vLightDir variable for spot-dir

            const float NoL = saturate(dot(tex_normal, light_dir));
            const float LoH = saturate(dot(light_dir, H));
            const float NoH = saturate(dot(tex_normal, H));
            const float VoH = saturate(dot(view, H));
        	
            // Diffuse & specular factors
            //const float3 light_diffuse = disney_diffuse(diffuse, f0, roughL, NoV, NoL, LoH);
            const float3 light_diffuse = Diffuse_Burley(NoL, NoV, LoH, roughP);
            const float3 light_specular = Specular_BRDF(ior_n, ior_k, tex_normal, H, LoH, NoH, VoH, roughL);

            const float rho = dot(-light_dir, sd);
            const float spot = pow(saturate((rho - Lights[i].vLightSpot.x) / (Lights[i].vLightSpot.y - Lights[i].vLightSpot.x)), Lights[i].vLightSpot.z);
            const float att = spot / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * dl + Lights[i].vLightAtt.z * dl * dl);

        	acc_diffuse = mad(att * shadow, NoL * light_color * light_diffuse, acc_diffuse);
            acc_specular = mad(att * shadow, NoL * light_color * light_specular, acc_specular);
        }
    }

	const float3 r = reflect(-view, tex_normal);
    //const float3 r = lerp(n, reflect(-v, n), (1.0f - roughL) * (sqrt(1.0f - roughL) + roughL));
	const float3 F = F_schlick_complex(ior_n, ior_k, NoV);
	
	float3 ibl_ambient = IBL_Ambient(F, tex_normal, mat.occlusion);
	float3 ibl_specular = IBL_Specular(F, NoV, r, mat.occlusion, roughP);


	float sss_strength = 0.0f;
	float3 ibl_sss = 0.0f;
	if (bHasCubeMap && bRenderShadowMap) {
        float4 wp = input.wp;

		float depth_offset = (1.0f - shadow_tex.z) / SNoV;
        wp.xyz -= view * depth_offset * ParallaxDepth * CUBE_SIZE;
		
		float4 sp = mul(wp, vLightViewProjection);
		float thickness = SSS_Thickness(sp.xyz / sp.w);

		float sss_dist = lerp(160.0f, 60.0f, sqrt(mat.sss));
		sss_strength = saturate(1.0f / (1.0f + thickness * sss_dist)) * mat.sss;
		
		//return float4(thickness, 0, 0, 1);
		
		acc_sss += SSS_Light(tex_normal, view, SunDirection) * SunStrength;
		ibl_sss = SSS_IBL(view);
    }


	const float3 emissive = mat.emissive * mat.albedo * PI;

	float3 final_diffuse = (ibl_ambient + acc_diffuse) * diffuse;
	float3 final_specular = (acc_specular + ibl_specular) * metal_albedo;
	float3 final_sss = diffuse * (acc_sss + ibl_sss) * sss_strength;
    float3 final_color = final_diffuse + final_specular + final_sss + emissive;
	float alpha = mat.alpha + lum(acc_specular + ibl_specular);

    //return float4(final_sss, 1.0f);
	
	final_color = tonemap_AcesFilm(final_color);
	//final_color = tonemap_HejlBurgess(final_color);
	final_color = linear_to_srgb(final_color);
	
    return float4(final_color, alpha);
}
