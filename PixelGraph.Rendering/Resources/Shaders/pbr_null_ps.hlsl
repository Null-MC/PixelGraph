#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/parallax.hlsl"
#include "lib/labPbr_material.hlsl"
#include "lib/pbr_filament2.hlsl"
#include "lib/pbr_hcm.hlsl"
#include "lib/tonemap.hlsl"

#define MIN_ROUGH 0.002f
#define WET_DARKEN 0.76f
#define WATER_DEPTH_SCALE 20.0f
#define WATER_ABS_SCALE 80.0f

#pragma pack_matrix(row_major)

static const float3 absorption_factor = float3(0.0035f, 0.0004f, 0.0f);


float4 main(const ps_input input) : SV_TARGET
{
	const float3 normal = normalize(input.nor);
    const float3 tangent = normalize(input.tan);
    const float3 bitangent = normalize(input.bin);
	const float3 view = normalize(input.eye);

    const float surface_NoV = saturate(dot(normal, view));
    const float3x3 matTBN = float3x3(tangent, bitangent, normal);

	float water_fillFactor = 0.0f;
    if (WaterMode == WATER_FULL) water_fillFactor = 1.0f;
    if (WaterMode == WATER_PUDDLES) water_fillFactor = dot(normal, up);
    
    float water_levelMin = 0.0f, water_levelMax = 0.0f;
	water_levelMin = pow(abs(Wetness), 0.4f) * saturate(water_fillFactor);
	water_levelMax = water_levelMin + rcp(WATER_DEPTH_SCALE) + EPSILON;
    
    pbr_material mat;
    float tex_depth = 0;
    float2 tex = 0, water_tex = 0;
    float3 shadow_tex = 0;

    float3 pom_wp, water_pom_wp;
    //float water_trace_dist = 0.0f;

    if (Wetness > EPSILON) {
        // ETA is purposely reversed!
		const float3 refractDir = refract(-view, normal, ETA_WATER_TO_AIR);

        const float3 refractDirT = mul(matTBN, refractDir);
		const float2 refractParallaxOffset = get_parallax_offset(refractDirT) * input.pDepth;// * aspect;

		tex = get_parallax_texcoord_wet(input.tex, input.vTS, refractParallaxOffset, surface_NoV, water_levelMin, water_tex, shadow_tex, tex_depth);

	    const float h1 = min(max(shadow_tex.z, water_levelMin), 1.0f);
	    const float pom_depth = (1.0f - h1) / max(surface_NoV, EPSILON) * BLOCK_SIZE * input.pDepth;
	    water_pom_wp = input.wp.xyz - pom_depth * view;

        const float wd = max(dot(-refractDir, normal), EPSILON);
	    const float h2 = saturate(water_levelMin - shadow_tex.z);
	    const float water_trace_dist = h2 / wd * BLOCK_SIZE * input.pDepth;
	    pom_wp = water_pom_wp + water_trace_dist * refractDir;
    }
    else {
		tex = get_parallax_texcoord(input.tex, input.vTS, surface_NoV, shadow_tex, tex_depth);
        water_tex = tex;

	    const float pom_depth = (1.0f - shadow_tex.z) / max(surface_NoV, EPSILON) * BLOCK_SIZE * input.pDepth;
	    water_pom_wp = pom_wp = input.wp.xyz - pom_depth * view;
    }

	mat = get_pbr_material(tex);

    if (BlendMode == BLEND_CUTOUT)
		clip(mat.opacity - CUTOUT_THRESHOLD);
    else if (BlendMode == BLEND_TRANSPARENT)
		clip(mat.opacity - EPSILON);

    const float wet_depth = saturate((water_levelMax - shadow_tex.z) * WATER_DEPTH_SCALE);

	//float roughL = max(mat.rough * mat.rough, MIN_ROUGH);
    const float roughP = clamp(1.0 - mat.smooth, MIN_ROUGH, 1.0f);
    const float roughL = roughP * roughP;

    const float porosityL = srgb_to_linear(mat.porosity);
	
    // Blend base colors
	const float metal = mat.f0_hcm > 0.9f ? 1.0f : 0.0f;
	const float3 tint = srgb_to_linear(TintColor);
    float3 diffuse = mat.albedo * tint * (1.0f - metal);

    const float3 src_normal = calc_tex_normal(tex, normal, tangent, bitangent);

	//-- Wetness --
    float surface_water = 0.0f;
    float3 wet_normal = src_normal;

    if (Wetness > EPSILON) {
		diffuse *= 1.0f - WET_DARKEN * max(pow(Wetness, 2), wet_depth) * porosityL;

	    //const float p1 = Wetness * 0.65f + 0.35f;
	    //surface_water = lerp(1.0f, 0.2f, mat.porosity) * Wetness;

	    surface_water = (1.0 - 0.7f * porosityL) * Wetness;

		wet_normal = calc_tex_normal(tex, normal, tangent, bitangent, Wetness * (1.0f - porosityL) * WATER_BLUR);
    }

    //-- Slope Normals --
    float3 tex_normal = src_normal;

    if (EnableSlopeNormals && !EnableLinearSampling && tex_depth - shadow_tex.z > 0.002f) {
	    const float3 slope = apply_slope_normal(tex, input.vTS, shadow_tex.z);

	    wet_normal = tex_normal = mul(slope, matTBN);
    }

    //float surface_wetness = Wetness * surface_water;

    float water = max(surface_water, wet_depth);
	float wet_roughL = lerp(roughL, WATER_ROUGH, water);

    wet_normal = lerp(wet_normal, normal, saturate(water_fillFactor * wet_depth));
    wet_normal = normalize(wet_normal);

    const float NoV = saturate(dot(tex_normal, view));
    const float NoV_wet = saturate(dot(wet_normal, view));
    const float ior_in = lerp(IOR_N_AIR, IOR_N_WATER, water);

    //-- HCM --
	float3 ior_n, ior_k;
	get_hcm_ior(mat.f0_hcm, ior_n, ior_k);
	float3 metal_albedo = lerp(1.0f, mat.albedo * tint, metal);

    float3 acc_light = 0.0;
    float3 acc_sss = 0.0;
    float spec_strength = 0.0f;

    float light_att, NoL;
    float3 light_dir, light_color, light_diffuse, light_specular, H;
    float LoH, NoH, VoH;
    float water_shadow;

	const float4x4 mShadowViewProj = mul(vLightView, vLightProjection);
	//const float4 sp = mul(float4(pom_wp, 1), mShadowViewProj);
    const float4 wet_sp = mul(float4(water_pom_wp, 1), mShadowViewProj);

	float sss_strength = 0.0f;

	[loop]
    for (int i = 0; i < NumLights; i++) {
        light_color = srgb_to_linear(Lights[i].vLightColor.rgb);
        light_att = 1.0f;

	    water_shadow = 1.0f;
    	
        if (Lights[i].iLightType == 1) { // directional
            light_color *= 3.0f;
            light_dir = normalize(Lights[i].vLightDir.xyz);
        }
        else {
            light_dir = Lights[i].vLightPos.xyz - pom_wp;
            const float light_dist = length(light_dir);
			//if (Lights[i].vLightAtt.w < light_dist) continue;

            light_dir /= light_dist;

	        if (Lights[i].iLightType == 2) { // point
        		light_att = rcp(Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * light_dist + Lights[i].vLightAtt.z * light_dist * light_dist);
	        }
	        else if (Lights[i].iLightType == 3) { // spot
	            const float3 sd = normalize(Lights[i].vLightDir.xyz);
	            const float rho = dot(-light_dir, sd);
	            const float spot = pow(saturate((rho - Lights[i].vLightSpot.x) / (Lights[i].vLightSpot.y - Lights[i].vLightSpot.x)), Lights[i].vLightSpot.z);
	            light_att = spot / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * light_dist + Lights[i].vLightAtt.z * light_dist * light_dist);
	        }
        }

        const float surface_NoL = saturate(dot(normal, light_dir));
        const float3 lightT = mul(matTBN, light_dir);
        const float2 polT = get_parallax_offset(lightT) * input.pDepth;

        if ((Lights[i].iLightType == 1 && EnableAtmosphere) || (Lights[i].iLightType == 3 && !EnableAtmosphere)) {
		    float h2 = 1.0f - shadow_tex.z;
		    float light_trace_dist = h2 / max(surface_NoL, EPSILON) * BLOCK_SIZE * input.pDepth;
		    const float3 shadow_wp = pom_wp + light_trace_dist * light_dir;
			const float4 shadow_sp = mul(float4(shadow_wp, 1), mShadowViewProj);

			float thickness = SSS_Thickness(shadow_sp.xyz / shadow_sp.w);
	        float sss_dist = lerp(160.0f, 60.0f, sqrt(mat.sss));
			sss_strength += rcp(1.0f + thickness * sss_dist) * mat.sss * light_att; // * light_color
			
			acc_sss += SSS_Light(tex_normal, view, light_dir) * light_color * light_att;

            if (surface_NoL > 0.0f) {
                light_att *= shadow_strength(shadow_sp.xyz / shadow_sp.w);

				if (Wetness > EPSILON) {
		            float d = max(water_levelMin, shadow_tex.z);
		            water_shadow = get_parallax_shadow(float3(water_tex.xy, d), polT, surface_NoL);
	                water_shadow *= shadow_strength(wet_sp.xyz / wet_sp.w);
		        }
            }
            else {
                light_att = 0.0f;
		        water_shadow = 0.0f;
            }
        }

        // light parallax shadows
        light_att *= get_parallax_shadow(shadow_tex, polT, surface_NoL);

        NoL = saturate(dot(tex_normal, light_dir));
        H = normalize(view + light_dir);
        LoH = saturate(dot(light_dir, H));
        VoH = saturate(dot(view, H));
		NoH = saturate(dot(tex_normal, H));

        light_diffuse = Diffuse_Burley(NoL, NoV, LoH, roughL) * diffuse;

        if (Wetness > EPSILON) {
            const float NoH_1 = saturate(dot(wet_normal, H));

            const float F_1 = F_full(ETA_AIR_TO_WATER, NoH_1);
            const float T_1 = 1.0f - F_1;

            const float3 L_2 = refract(-light_dir, -H, ETA_WATER_TO_AIR);
			const float3 V_2 = refract(-view, -H, ETA_WATER_TO_AIR);
            const float3 T_2 = 1.0f - F_full(ETA_WATER_TO_AIR, saturate(dot(V_2, H)));
            const float3 H_2 = normalize(V_2 + L_2);

            const float  VoH_2 = lerp(VoH, saturate(dot(V_2, H_2)), water);
			const float  NoH_2 = lerp(NoH, saturate(dot(tex_normal, H_2)), water);
			const float  LoH_2 = lerp(LoH, saturate(dot(L_2, H_2)), water);

			const float3 Fr_2 = mat.f0_hcm > 0.9f
        		? specular_brdf_conductor(ior_in, ior_n, ior_k, LoH_2, NoH_2, VoH_2, roughL) * metal_albedo
        		: specular_brdf(ior_n.r / ior_in, LoH_2, NoH_2, VoH_2, roughL);
            
	        // ETA is purposely reversed!
			const float3 light_refractDir = refract(-light_dir, normal, ETA_WATER_TO_AIR);
	        const float wd = max(dot(-light_refractDir, normal), EPSILON);
		    const float h2 = min(max(water_levelMin - shadow_tex.z, 0.0f) + 0.2f * surface_water, 1.0f);
		    const float light_waterTraceDist = max(h2 / wd, 1.0f - light_att) * BLOCK_SIZE * input.pDepth;

			const float3 absorption = exp(-absorption_factor * max(light_waterTraceDist, 0.0f) * WATER_ABS_SCALE);

            light_specular = T_1 * Fr_2 * T_2 * absorption;

			const float D = D_ggx(NoH, wet_roughL);
			const float G = V_kelemen(LoH);
			const float F = F_full(ETA_AIR_TO_WATER, VoH) * water;

			const float Frc = D * G * F;

			const float invFc = 1.0f - F;

            const float3 surface_diffuse = light_diffuse * light_att * NoL * invFc * light_color;
            const float3 surface_specular = light_specular * light_att * NoL * invFc * light_color;
            const float3 water_spec = Frc * water_shadow * surface_NoL * light_color;

            acc_light += surface_diffuse + surface_specular + water_spec;
            spec_strength += luminance(surface_specular + water_spec);
        }
        else {
			light_specular = mat.f0_hcm > 0.9f
        		? specular_brdf_conductor(IOR_N_AIR, ior_n, ior_k, LoH, NoH, VoH, roughL) * metal_albedo
        		: specular_brdf(ior_n.r / IOR_N_AIR, LoH, NoH, VoH, roughL);

            const float3 light_factor = NoL * light_color * light_att;
			acc_light = mad(light_factor, light_diffuse + light_specular, acc_light);
            spec_strength += luminance(light_factor * light_specular);
        }
    }

    const float3 ibl_f0_ambient = ior_to_f0_complex(ior_in, ior_n, ior_k);
	const float3 ibl_F_ambient = F_schlick_roughness(ibl_f0_ambient, NoV, roughL);
	const float3 ibl_ambient = IBL_ambient(ibl_F_ambient, tex_normal) * diffuse * mat.occlusion;

	const float3 r = reflect(-view, wet_normal);
	float3 ibl_specular = IBL_specular(ibl_F_ambient, NoV, r, mat.occlusion, roughP) * metal_albedo;
	float3 ibl_sss = SSS_IBL(view);

    if (Wetness > EPSILON) {
		float3 ibl_F_water = F_full(ETA_AIR_TO_WATER, NoV_wet);

        H = normalize(r + view);
        VoH = saturate(dot(view, H));
        float3 V2 = refract(-view, -H, ETA_WATER_TO_AIR);
        //float NoV_2 = saturate(dot(tex_normal, V2));
        //float NoL_2 = saturate(dot(r, V2));

        float T12 = 1.0f - F_full(ETA_AIR_TO_WATER, VoH) * water;
        float T21 = 1.0f - F_full(ETA_WATER_TO_AIR, saturate(dot(V2, H))) * water;

	    float h2 = saturate(water_levelMin - shadow_tex.z);
		float3 absorption = exp(-absorption_factor * max(h2, 0.0f) * WATER_ABS_SCALE);

        //ibl_specular *= lerp(1.0f, T12 * T21 * absorption, water);
        ibl_specular *= 1.0f + water * (T12 * T21 * absorption - 1.0f);

		ibl_specular += IBL_specular(ibl_F_water, NoV_wet, r, mat.occlusion, wet_roughL) * water; // * metal_albedo
    }

    spec_strength += luminance(ibl_specular);

	const float3 emissive = mat.emissive * mat.albedo * 24.0f;

	float3 ibl_final = ibl_ambient + ibl_specular;

	float3 final_sss = (acc_sss + ibl_sss) * diffuse;

    float3 final_color = ibl_final + emissive;

    final_color += acc_light * (1 - sss_strength);
	final_color += final_sss * sss_strength;

	float alpha = mat.opacity + spec_strength;
    if (BlendMode != BLEND_TRANSPARENT) alpha = 1.0f;

	//final_color = tonemap_ACESFit2(final_color);
	final_color = linear_to_srgb(final_color);
	//final_color = tonemap_Reinhard(final_color);

    //return float4(tex_normal, 1);
    return float4(final_color, alpha);
}
