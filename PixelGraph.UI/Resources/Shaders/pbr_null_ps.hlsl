#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/parallax.hlsl"
#include "lib/pbr_material.hlsl"
#include "lib/pbr_filament2.hlsl"
#include "lib/pbr_hcm.hlsl"
#include "lib/tonemap.hlsl"

#define MIN_ROUGH 0.002f
#define WET_DARKEN 0.76f

#pragma pack_matrix(row_major)


float4 main(const ps_input input) : SV_TARGET
{
	const float3 normal = normalize(input.nor);
    const float3 tangent = normalize(input.tan);
    const float3 bitangent = normalize(input.bin);
	const float3 view = normalize(input.eye);

	float2 shadow_tex = 0;
    float shadow_depth = 0;
    float tex_depth = 0;

    const float SNoV = saturate(dot(normal, view));
	const float2 tex = get_parallax_texcoord(input.tex, input.poT, SNoV, shadow_tex, shadow_depth, tex_depth);
	const pbr_material mat = get_pbr_material(tex);

	clip(mat.alpha - EPSILON);

    const float3x3 mTBN = float3x3(tangent, bitangent, normal);
	float roughL = max(mat.rough * mat.rough, MIN_ROUGH);

	//-- Wetness --
    const float wet_factor = saturate(Wetness - (1.0f - mat.porosity));
	const float wet_darken = 1.0f - WET_DARKEN * wet_factor;

    //const float surface_water = pow(saturate(Wetness - mat.porosity), 0.3f);
    const float p1 = pow(Wetness, 1.6f) * 0.65f;
    const float surface_water = lerp(Wetness, p1, mat.porosity);

    const float wet_roughL = lerp(roughL, WATER_ROUGH, saturate(surface_water * 2.0f));

    //-- Slope Normals --
    float3 tex_normal;
    if (EnableSlopeNormals && !EnableLinearSampling && tex_depth - shadow_depth > EPSILON) {
        float3 tex_size;
    	tex_normal_height.GetDimensions(0, tex_size.x, tex_size.y, tex_size.z);

        float2 tex_snapped = round(tex * tex_size.xy) / tex_size.xy;
        float2 tex_offset = tex - tex_snapped;

        if (abs(tex_offset.y) < abs(tex_offset.x)) {
        	tex_normal = bitangent * sign(-tex_offset.y);

			float VdotN = dot(view, tex_normal);
			if (VdotN < 0) tex_normal = tangent * sign(-tex_offset.x);
        }
        else {
        	tex_normal = tangent * sign(-tex_offset.x);

			float VdotN = dot(view, tex_normal);
			if (VdotN < 0) tex_normal = bitangent * sign(-tex_offset.y);
        }
    }
    else
		tex_normal = calc_tex_normal(tex, normal, tangent, bitangent);

    const float3 wet_normal = calc_tex_normal(tex, normal, tangent, bitangent, surface_water * WATER_BLUR);
	
    // Blend base colors
	const float metal = mat.f0 > 0.9f ? 1.0f : 0.0f;
	const float3 tint = srgb_to_linear(vMaterialDiffuse.rgb);
    const float3 diffuse = mat.albedo * tint * wet_darken * (1.0f - metal);

    //-- HCM --
	float3 ior_n, ior_k;
	get_hcm_ior(mat.f0, ior_n, ior_k);
	float3 metal_albedo = lerp(1.0f, mat.albedo * tint, metal);// * wet_diffuse;
	
    const float NoV = saturate(dot(tex_normal, view));
    const float NoV_wet = saturate(dot(wet_normal, view));
	const float ior_n_in = lerp(IOR_N_AIR, IOR_N_WATER, surface_water);
    const float3 f0 = ior_to_f0_complex(ior_n_in, ior_n, ior_k);

    //return float4(f0, 1);

    float3 acc_light = 0.0;
    //float3 acc_diffuse = 0.0;
    //float3 acc_specular = 0.0;
    float3 acc_sss = 0.0;
		
	[loop]
    for (int i = 0; i < NumLights; i++) {
        const float3 light_color = srgb_to_linear(Lights[i].vLightColor.rgb) * 1.6f;
    	
        if (Lights[i].iLightType == 1) {
            const float3 light_dir = normalize(Lights[i].vLightDir.xyz);
        	
        	// light parallax shadows
        	const float SNoL = dot(normal, light_dir);
        	const float2 polT = get_parallax_offset(mTBN, light_dir);
            const float shadow = get_parallax_shadow(shadow_tex, shadow_depth, polT, SNoL);
            if (shadow < EPSILON) continue;
        	
            const float3 H = normalize(light_dir + view);
            const float NoL = saturate(dot(tex_normal, light_dir));
            const float LoH = saturate(dot(light_dir, H));
            const float NoH = saturate(dot(tex_normal, H));
            const float NoH_wet = saturate(dot(wet_normal, H));
            const float VoH = saturate(dot(view, H));
        	
            // Diffuse & specular factors
            const float3 light_diffuse = Diffuse_Burley(NoL, NoV, LoH, roughL) * diffuse * NoL;
            const float3 light_specular = specular_brdf_wet(f0, LoH, NoH, VoH, roughL) * metal_albedo * NoL;
			const float3 light_brdf = clearcoat_brdf(light_diffuse, light_specular, LoH, NoH_wet, wet_roughL, surface_water);
        	//const float3 light_factor = NoL * light_color * shadow;
        	
            //acc_diffuse += light_diffuse * light_factor;
            //acc_specular += light_specular * light_factor;
        	acc_light += shadow * light_color * light_brdf;
        }
        else if (Lights[i].iLightType == 2) {
            float3 light_dir = Lights[i].vLightPos.xyz - input.wp.xyz;
            const float light_dist = length(light_dir);
            light_dir = light_dir / light_dist;
        	
            if (Lights[i].vLightAtt.w < light_dist) continue;

        	// light parallax shadows
        	const float SNoL = dot(normal, light_dir);
        	const float2 polT = get_parallax_offset(mTBN, light_dir);
            const float shadow = get_parallax_shadow(shadow_tex, shadow_depth, polT, SNoL);
            if (shadow < EPSILON) continue;
        	
            const float3 H = normalize(view + light_dir);
            const float NoL = saturate(dot(tex_normal, light_dir));
            const float LoH = saturate(dot(light_dir, H));
            const float NoH = saturate(dot(tex_normal, H));
            const float VoH = saturate(dot(view, H));

        	const float att = rcp(Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * light_dist + Lights[i].vLightAtt.z * light_dist * light_dist);
        	
            // Diffuse & specular factors
            const float3 light_diffuse = Diffuse_Burley(NoL, NoV, LoH, roughL) * diffuse;
            const float3 light_specular = specular_brdf_wet(f0, LoH, NoH, VoH, roughL) * metal_albedo;
			const float3 light_brdf = clearcoat_brdf(light_diffuse, light_specular, LoH, NoH, wet_roughL, surface_water);

        	//acc_diffuse = mad(att * shadow, NoL * light_color * light_diffuse, acc_diffuse);
            //acc_specular = mad(att * shadow, NoL * light_color * light_specular, acc_specular);
        	acc_light = mad(att * shadow, NoL * light_color * light_brdf, acc_light);
        }
        else if (Lights[i].iLightType == 3) {
            float3 light_dir = Lights[i].vLightPos.xyz - input.wp.xyz;
        	
            const float light_dist = length(light_dir);
            if (Lights[i].vLightAtt.w < light_dist) continue;
            light_dir = light_dir / light_dist;

        	// light parallax shadows
        	const float SNoL = dot(normal, light_dir);
        	const float2 polT = get_parallax_offset(mTBN, light_dir);
            const float shadow = get_parallax_shadow(shadow_tex, shadow_depth, polT, SNoL);
            if (shadow < EPSILON) continue;
        	
            const float3 H = normalize(view + light_dir);
            const float NoL = saturate(dot(tex_normal, light_dir));
            const float LoH = saturate(dot(light_dir, H));
            const float NoH = saturate(dot(tex_normal, H));
            const float VoH = saturate(dot(view, H));
        	
            const float3 sd = normalize(Lights[i].vLightDir.xyz); // missuse the vLightDir variable for spot-dir
        	
            // Diffuse & specular factors
            const float3 light_diffuse = Diffuse_Burley(NoL, NoV, LoH, roughL) * diffuse;
            const float3 light_specular = specular_brdf_wet(f0, LoH, NoH, VoH, roughL) * metal_albedo;
			const float3 light_brdf = clearcoat_brdf(light_diffuse, light_specular, LoH, NoH, wet_roughL, surface_water);

            const float rho = dot(-light_dir, sd);
            const float spot = pow(saturate((rho - Lights[i].vLightSpot.x) / (Lights[i].vLightSpot.y - Lights[i].vLightSpot.x)), Lights[i].vLightSpot.z);
            const float att = spot / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * light_dist + Lights[i].vLightAtt.z * light_dist * light_dist);

        	//acc_diffuse = mad(att * shadow, NoL * light_color * light_diffuse, acc_diffuse);
            //acc_specular = mad(att * shadow, NoL * light_color * light_specular, acc_specular);
        	acc_light = mad(att * shadow, NoL * light_color * light_brdf, acc_light);
        }
    }

	const float3 r = reflect(-view, tex_normal);
	const float3 ibl_ior_n_out = lerp(ior_n, IOR_N_WATER, surface_water);
	//const float3 ibl_ior_k = lerp(ior_k, 0.0f, surface_water); // WARN: I don't think this is right
    const float3 ibl_f0 = ior_to_f0_complex(IOR_N_AIR, ibl_ior_n_out, ior_k);
	float3 ibl_ambient = IBL_Ambient(ibl_f0, tex_normal, mat.occlusion);
	float3 ibl_specular = IBL_Specular(ibl_f0, NoV_wet, r, mat.occlusion, wet_roughL);

	float sss_strength = 0.0f;
	float3 ibl_sss = 0.0f;
	if (bHasCubeMap && bRenderShadowMap) {
        float4 wp = input.wp;

		float depth_offset = (1.0f - shadow_depth) / SNoV;
        wp.xyz -= view * depth_offset * ParallaxDepth * CUBE_SIZE;
		
		float4 sp = mul(wp, vLightViewProjection);
		float thickness = SSS_Thickness(sp.xyz / sp.w);
		//return float4(thickness, 0, 0, 1);

		float sss_dist = lerp(160.0f, 60.0f, sqrt(mat.sss));
		sss_strength = saturate(1.0f / (1.0f + thickness * sss_dist)) * mat.sss;
		
		acc_sss += SSS_Light(tex_normal, view, SunDirection) * SunStrength;
		ibl_sss = SSS_IBL(view);
    }

	const float3 emissive = mat.emissive * mat.albedo * 16.0f;

	float3 final_ambient = ibl_ambient * diffuse;
	//float3 final_diffuse = acc_diffuse * diffuse;
	//float3 final_specular = (acc_specular + ibl_specular) * metal_albedo;
	float3 final_sss = diffuse * (acc_sss + ibl_sss) * sss_strength;
    //float3 final_color = final_diffuse + final_specular + final_sss + emissive;
    float3 final_color =  final_ambient + acc_light + (ibl_specular * metal_albedo) + final_sss + emissive;
	float alpha = mat.alpha; // + lum(acc_specular + ibl_specular);

    //return float4(ibl_specular, 1.0f);
	
	final_color = tonemap_AcesFilm(final_color);
	final_color = linear_to_srgb(final_color);
	//final_color = tonemap_HejlBurgess(final_color);
	
    return float4(final_color, alpha);
}
