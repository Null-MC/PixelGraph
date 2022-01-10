#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/parallax.hlsl"
#include "lib/pbr_material.hlsl"
#include "lib/pbr_filament2.hlsl"
#include "lib/pbr_hcm.hlsl"
#include "lib/tonemap.hlsl"

#define MIN_ROUGH 0.002f
#define WET_DARKEN 0.89f
#define WATER_DEPTH_SCALE 6.0f

#define ETA_AIR_TO_WATER (IOR_N_WATER / IOR_N_AIR)
#define ETA_WATER_TO_AIR (IOR_N_AIR / IOR_N_WATER)

#pragma pack_matrix(row_major)

static const float3 up = float3(0.0f, 1.0f, 0.0f);
//static const int blendMode = BLEND_CUTOUT;


float4 main(const ps_input input) : SV_TARGET
{
	const float3 normal = normalize(input.nor);
    const float3 tangent = normalize(input.tan);
    const float3 bitangent = normalize(input.bin);
	const float3 view = normalize(input.eye);

    float2 tex;
	float3 shadow_tex = 0;
    float3 water_tex = 0;
    float tex_depth = 0;

	const float surface_up = dot(normal, up);

	const float water_max = 1.0f + rcp(WATER_DEPTH_SCALE);
    float water_level_min = 0.0f; // was wet_depth
    float water_level_max = 0.0f;

	if (EnablePuddles) {
		water_level_min = sqrt(Wetness) * saturate(surface_up); // max(Wetness - (1.0f - rcp(WATER_DEPTH_SCALE)), 0.0f) * WATER_DEPTH_SCALE;
		water_level_max = water_level_min * water_max; // max(Wetness - (1.0f - rcp(WATER_DEPTH_SCALE)), 0.0f) * WATER_DEPTH_SCALE;
        //return float4(0, 0, water_level, 1);
    }

 //   wet_normal = lerp(wet_normal, up, surface_up * saturate(wet_depth));

 //   float water = saturate(max(pow(abs(tex_water), 0.3f), wet_depth));
	//float wet_roughL = lerp(roughL, WATER_ROUGH, water);
    
    const float SNoV = saturate(dot(normal, view));

    if (Wetness > EPSILON)
		tex = get_parallax_texcoord_wet(input.tex, input.poT, SNoV, water_level_min, water_tex, shadow_tex, tex_depth);
    else {
		tex = get_parallax_texcoord(input.tex, input.poT, SNoV, shadow_tex, tex_depth);
        water_tex.xy = tex;
        water_tex.z = 0.0f;
    }

    float pom_depth = (1.0f - tex_depth) / max(SNoV, EPSILON) * CUBE_SIZE * ParallaxDepth;
    const float3 pom_wp = input.wp.xyz - pom_depth * view;
    //return float4(pom_wp / CUBE_SIZE, 1);

    const float water_pom_depth = 1.0f - max(water_level_min, tex_depth);// abs((tex - input.tex) / input.poT);
    const float3 water_pom_wp = input.wp.xyz + water_pom_depth * -view * CUBE_SIZE * ParallaxDepth;

    //return float4(water_pom_depth, 0, 0, 1);
    //return float4(abs(water_pom_wp - input.wp.xyz), 1);

    const float wet_depth = saturate((water_level_max - tex_depth) * WATER_DEPTH_SCALE);
    //return float4(wet_depth * 4, 0, 0, 1);

	const pbr_material mat = get_pbr_material(tex);

    if (BlendMode == BLEND_CUTOUT)
		clip(mat.alpha - CUTOUT_THRESHOLD);
    else if (BlendMode == BLEND_TRANSPARENT)
		clip(mat.alpha - EPSILON);

    //if (wet_depth > EPSILON) return float4(0,1,0,1);

    const float3x3 mTBN = float3x3(tangent, bitangent, normal);
	float roughL = max(mat.rough * mat.rough, MIN_ROUGH);
	
    // Blend base colors
	const float metal = mat.f0 > 0.9f ? 1.0f : 0.0f;
	//const float3 tint = srgb_to_linear(vMaterialDiffuse.rgb);
	const float3 tint = srgb_to_linear(TintColor);
    float3 diffuse = mat.albedo * tint * (1.0f - metal);

    const float3 src_normal = calc_tex_normal(tex, normal, tangent, bitangent);

	//-- Wetness --
    float surface_water = 0.0f;
    float3 wet_normal = src_normal;

    if (Wetness > EPSILON) {
		diffuse *= 1.0f - saturate(WET_DARKEN * Wetness - (1.0f - mat.porosity));

	    const float p1 = pow(abs(Wetness), 1.6f) * 0.65f;
	    surface_water = lerp(Wetness, p1, mat.porosity);

		wet_normal = calc_tex_normal(tex, normal, tangent, bitangent, surface_water * (1.0f - mat.porosity) * WATER_BLUR);
    }

    //-- Slope Normals --
    float3 tex_normal = src_normal;

    const float slope_depth = tex_depth - shadow_tex.z;
    if (EnableSlopeNormals && !EnableLinearSampling && slope_depth > EPSILON) {
        tex_normal = get_slope_normal(tex, view, tangent, bitangent);

        wet_normal = tex_normal;
    }

    //float tex_water = saturate(0.5f + 0.5f * dot(tex_normal, up)) * surface_water;
    float surface_wetness = saturate(0.5f + 0.5f * dot(tex_normal, up)) * surface_water;
    //return float4(surface_wetness, 0, 0, 1);

 //   float wet_depth = 0.0f;
	//if (EnablePuddles) wet_depth = max(Wetness - (1.0f - rcp(WATER_DEPTH_SCALE)) * shadow_depth, 0.0f) * WATER_DEPTH_SCALE;

    wet_normal = lerp(wet_normal, up, saturate(surface_up * wet_depth));

    //return float4(wet_normal, 1);

    float water = max(surface_wetness, wet_depth);
	float wet_roughL = lerp(roughL, WATER_ROUGH, water);

    const float NoV = saturate(dot(tex_normal, view));
    const float NoV_wet = saturate(dot(wet_normal, view));
    const float ior_in = lerp(IOR_N_AIR, IOR_N_WATER, surface_wetness);

    //-- HCM --
	float3 ior_n, ior_k;
	get_hcm_ior(mat.f0, ior_n, ior_k);
	float3 metal_albedo = lerp(1.0f, mat.albedo * tint, metal);

    float3 acc_light = 0.0;
    float3 acc_sss = 0.0;
    float spec_strength = 0.0f;

    float light_att, light_shadow, NoL;
    float3 light_dir, light_color, light_diffuse, light_specular, H;
    float LoH, NoH, VoH;

	const float4x4 mShadowViewProj = mul(vLightView, vLightProjection);
	const float4 sp = mul(float4(pom_wp, 1), mShadowViewProj);
	//const float4 sp = mul(input.wp, mShadowViewProj);

    //return float4(abs(pom_wp - input.wp.xyz), 1);


    // WARN: TESTING
    //light_shadow = shadow_strength(sp.xyz / sp.w);
    //return float4(light_shadow, light_shadow, light_shadow, 1);
	//const float shadow_depth = tex_shadow.SampleLevel(sampler_light, sp.xy / sp.w, 0);
	//const float shadow_depth = sp.z / sp.w;
	//return float4(shadow_depth, shadow_depth, shadow_depth, 1);


	[loop]
    for (int i = 0; i < NumLights; i++) {
        light_color = srgb_to_linear(Lights[i].vLightColor.rgb) * 6.0f;
        light_shadow = 1.0f;
    	
        if (Lights[i].iLightType == 1) {
            light_dir = normalize(Lights[i].vLightDir.xyz);
            light_att = 1.0f;

            if (bHasShadowMap && bRenderShadowMap) {
	            float d = dot(light_dir, normal);

	            if (d > 0) {
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
        VoH = saturate(dot(view, H));
		NoH = saturate(dot(tex_normal, H));

        // Diffuse & specular factors
        light_diffuse = Diffuse_Burley(NoL, NoV, LoH, roughL) * diffuse;

        if (Wetness > EPSILON) {
            //const float NoL_1 = saturate(dot(wet_normal, light_dir));
            const float NoH_1 = saturate(dot(wet_normal, H));

            const float d = max(tex_depth, water_level_min);
            float water_shadow = get_parallax_shadow(float3(water_tex.xy, d), polT, SNoL);

            if (bHasShadowMap && bRenderShadowMap) {
	            float d2 = dot(light_dir, normal);

	            if (d2 > 0) {
					float4 wet_sp = mul(float4(water_pom_wp, input.wp.w), mShadowViewProj);
	                water_shadow *= shadow_strength(wet_sp.xyz / wet_sp.w);

	                //water_shadow = shadow_strength(sp.xyz / sp.w);
                    //return float4(water_shadow, 0, 0, 1);
	            }
                else {
	                water_shadow = 0.0f;
                }
			}

            const float F_1 = F_full(ETA_AIR_TO_WATER, NoH_1);
            const float T_1 = 1.0f - F_1;
            //float Fr_1 = F_1 * (202.0f / 8.0f) * pow(NoH_1, 200.0f);
            //float3 Fr_1 = specular_brdf(ETA_AIR_TO_WATER, LoH, NoH_1, VoH, wet_roughL);

            const float3 L_2 = refract(-light_dir, -H, ETA_WATER_TO_AIR);
			const float3 V_2 = refract(-view, -H, ETA_WATER_TO_AIR);
            const float3 T_2 = 1.0f - F_full(ETA_WATER_TO_AIR, saturate(dot(V_2, H)));
            const float3 H_2 = normalize(V_2 + L_2);

            const float  VoH_2 = lerp(VoH, saturate(dot(V_2, H_2)), surface_wetness);
			const float  NoH_2 = lerp(NoH, saturate(dot(tex_normal, H_2)), surface_wetness);
			const float  LoH_2 = lerp(LoH, saturate(dot(L_2, H_2)), surface_wetness);
			const float  NoL_2 = lerp(NoL, saturate(dot(tex_normal, L_2)), surface_wetness);
			const float  NoV_2 = lerp(NoV, saturate(dot(tex_normal, V_2)), surface_wetness);
            //return float4(0, 0, water_shadow, 1);

            //float3 F_2, Fr_2;
            //if (mat.f0 > 0.9f) F_2 = F_conductor(VoH_2, ior_in, ior_n, ior_k);
            //else F_2 = F_full(ior_n.r / ior_in, VoH_2);

            //float  SpecPower = exp2((1.0f - wet_roughL) * 11.0f);

			//Fr_2 = light_diffuse * light_att + F_2 * ((SpecPower + 2.0f) / 8.0f) * pow(NoH_2, SpecPower) * metal_albedo;
			const float3 Fr_2 = mat.f0 > 0.9f
        		? specular_brdf_conductor(ior_in, ior_n, ior_k, LoH_2, NoH_2, VoH_2, roughL) * metal_albedo
        		: specular_brdf(ior_n.r, LoH_2, NoH_2, VoH_2, roughL);

            //return float4(F_2 * ((SpecPower + 2.0f) / 8.0f) * pow(NoH_2, SpecPower) * metal_albedo, 1);

            const float dist = (0.2f * surface_water + 2.0f * wet_depth) * rcp(max(NoV_2, 0.001f)) + rcp(max(NoL_2, 0.001f));
            const float3 absorption = exp(-float3(0.0035f, 0.0004f, 0.0f) * dist);

            //float3 Fr = Fr_1 + T_1 * Fr_2 * Absorption * T_2;
            //light_specular = Fr_1 + T_1 * Fr_2 * Absorption * T_2;
            light_specular = T_1 * Fr_2 * absorption * T_2;

            const float3 Fr_1 = specular_brdf_clearcoat(ETA_AIR_TO_WATER, light_diffuse, light_specular, LoH, NoH_1, VoH, wet_roughL, water, light_shadow, water_shadow);
            //return float4(Fr_1, 1);

			acc_light += NoL * light_color * light_att * Fr_1;
        }
        else {
			light_specular = mat.f0 > 0.9f
        		? specular_brdf_conductor(IOR_N_AIR, ior_n, ior_k, LoH, NoH, VoH, roughL) * metal_albedo
        		: specular_brdf(ior_n.r, LoH, NoH, VoH, roughL);

            const float3 light_factor = light_shadow * NoL * light_color * light_att;
			acc_light += light_factor * (light_diffuse + light_specular);
            spec_strength += lum(light_factor * light_specular);
        }
    }

	const float3 r = reflect(-view, wet_normal);

    const float3 ibl_f0_ambient = ior_to_f0_complex(ior_in, ior_n, ior_k);
	const float3 ibl_F_ambient = F_schlick_roughness(ibl_f0_ambient, NoV, roughL);
	const float3 ibl_ambient = IBL_ambient(ibl_F_ambient, r) * diffuse * mat.occlusion;

	float3 ibl_specular = IBL_specular(ibl_F_ambient, NoV, r, mat.occlusion, mat.rough) * metal_albedo;

    if (Wetness > EPSILON) {
		float3 ibl_F_water = F_full(ETA_AIR_TO_WATER, NoV_wet);

        H = normalize(r + view);
        VoH = saturate(dot(view, H));
        float3 V2 = refract(-view, -H, ETA_WATER_TO_AIR);
        float NoV_2 = saturate(dot(tex_normal, V2));
        float NoL_2 = saturate(dot(r, V2));

        float T12 = 1.0f - F_full(ETA_AIR_TO_WATER, VoH) * surface_wetness;
        float T21 = 1.0f - F_full(ETA_WATER_TO_AIR, saturate(dot(V2, H))) * surface_wetness;

        float dist = (0.2f * surface_water + 2.0f * wet_depth) * rcp(max(NoV_2, 0.001f)) + rcp(max(NoL_2, 0.001f));
		float3 absorption = exp(-float3(0.0035, 0.0004, 0.0) * dist);

        ibl_specular *= lerp(1.0f, T12 * T21 * absorption, water);

		ibl_specular += IBL_specular(ibl_F_water, NoV_wet, r, mat.occlusion, wet_roughL) * water; // * metal_albedo
    }

    spec_strength += lum(ibl_specular);

	float sss_strength = 0.0f;
	float3 ibl_sss = 0.0f;

	if (bHasCubeMap && bRenderShadowMap) {
  //      float4 wp = input.wp;

		//float depth_offset = (1.0f - shadow_tex.z) / SNoV;
  //      wp.xyz -= view * depth_offset * ParallaxDepth * CUBE_SIZE;
		//
		//float4 sp = mul(wp, mShadowViewProj);
		float thickness = SSS_Thickness(sp.xyz / sp.w);

		float sss_dist = lerp(160.0f, 60.0f, sqrt(mat.sss));
		sss_strength = rcp(1.0f + thickness * sss_dist) * mat.sss * SunStrength;
		
		acc_sss += SSS_Light(tex_normal, view, SunDirection) * SunStrength;
		ibl_sss = SSS_IBL(view);
	}

	const float3 emissive = mat.emissive * mat.albedo * 16.0f;

	float3 ibl_final = ibl_ambient + ibl_specular;

	float3 final_sss = diffuse * (acc_sss + ibl_sss);

    float3 final_color = ibl_final + emissive;

    final_color += acc_light * (1 - sss_strength);
	final_color += final_sss * sss_strength;

	float alpha = mat.alpha + spec_strength;
    if (BlendMode != BLEND_TRANSPARENT) alpha = 1.0f;

	//final_color = tonemap_AcesFilm(final_color);
	//final_color = linear_to_srgb(final_color);
	final_color = tonemap_HejlBurgess(final_color);

    return float4(final_color, alpha);
}
