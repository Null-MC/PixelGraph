#define MESH

#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/parallax.hlsl"
#include "lib/labPbr_material.hlsl"
#include "lib/pbr_filament2.hlsl"
#include "lib/pbr_hcm.hlsl"
#include "lib/normals.hlsl"
#include "lib/tonemap.hlsl"

#define MIN_ROUGH 0.04f
#define WET_DARKEN 0.86f
#define WATER_DEPTH_SCALE 20.0f
#define WATER_ABS_SCALE 80.0f
#define WATER_BLUR 0.2f

#pragma pack_matrix(row_major)

static const float3 absorption_factor = float3(0.0035f, 0.0004f, 0.0f);


float4 main(const ps_input input, const in bool face : SV_IsFrontFace) : SV_TARGET
{
    //float2 col = tex_dielectric_brdf_lut.SampleLevel(sampler_brdf_lut, input.tex, 0);
    //return float4(col, 0.f, 1.f);

	const float3 normal = normalize(input.nor);
    float3 tangent = normalize(input.tan);
    float3 bitangent = normalize(input.bin);
	const float3 view = normalize(input.eye);

    const float surface_NoV = saturate(dot(normal, view));
    const float3x3 matTBN = float3x3(tangent, bitangent, normal);

	float water_fillFactor = 0.0f;
    if (WaterMode == WATER_FULL) water_fillFactor = 1.0f;
    if (WaterMode == WATER_PUDDLES) water_fillFactor = dot(normal, up);
    
    float water_levelMin = 0.0f, water_levelMax = 0.0f;
	water_levelMin = pow(abs(Wetness), 0.4f) * saturate(water_fillFactor);
	water_levelMax = water_levelMin + rcp(WATER_DEPTH_SCALE) + EPSILON;

	float2 vTS = input.vTS;
	if (!face) {
		vTS = -vTS;
		tangent = -tangent;
		bitangent = -bitangent;
	}

    pbr_material mat;
    float tex_depth = 0;
    float2 tex = 0, water_tex = 0;
    float3 shadow_tex = 0;

    float3 pom_wp, water_pom_wp;

    if (Wetness > EPSILON) {
        // ETA is purposely reversed!
		const float3 refractDir = refract(-view, normal, ETA_WATER_TO_AIR);

        const float3 refractDirT = mul(matTBN, refractDir);
		const float2 refractParallaxOffset = get_parallax_offset(refractDirT) * input.pDepth;// * aspect;

		tex = get_parallax_texcoord_wet(input.tex, vTS, refractParallaxOffset, water_levelMin, water_tex, shadow_tex, tex_depth);

	    const float h1 = 1.0f - min(max(shadow_tex.z, water_levelMin), 1.0f);
	    float pom_depth = rcp(max(surface_NoV, EPSILON)) * h1 * BLOCK_SIZE * ParallaxDepth;

	    water_pom_wp = input.wp.xyz - pom_depth * view;

        const float wd = max(dot(-refractDir, normal), EPSILON);
	    const float h2 = saturate(water_levelMin - shadow_tex.z);
	    float water_trace_dist = rcp(wd) * h2 * BLOCK_SIZE * ParallaxDepth;
        if (wd < EPSILON) water_trace_dist = 0.0;
	    pom_wp = water_pom_wp + water_trace_dist * refractDir;
    }
    else {
		tex = get_parallax_texcoord(input.tex, vTS, shadow_tex, tex_depth);
        water_tex = tex;

	    const float pom_depth = rcp(max(surface_NoV, EPSILON)) * (1.0f - shadow_tex.z) * BLOCK_SIZE * ParallaxDepth;
	    water_pom_wp = pom_wp = input.wp.xyz - pom_depth * view;
    }

	mat = get_pbr_material(tex);

    if (BlendMode == BLEND_CUTOUT)
		clip(mat.opacity - CUTOUT_THRESHOLD);
    else if (BlendMode == BLEND_TRANSPARENT)
		clip(mat.opacity - EPSILON);

    const float wet_depth = saturate((water_levelMax - shadow_tex.z) * WATER_DEPTH_SCALE);

	const float roughP = max(1.0 - mat.smooth, MIN_ROUGH);
	const float roughL = roughP * roughP;

    //return float4(roughP, roughP, roughP, 1.f);
    
    //const float porosityL = srgb_to_linear(mat.porosity);
	
    // Blend base colors
	const float metal = step(0.8999f, mat.f0_hcm);
	const float3 tint = srgb_to_linear(TintColor);
    float3 diffuse = mat.albedo * tint * (1.0f - metal);

	float3 tex_normal = tex_normal_height.Sample(sampler_height, tex).xyz;
    //src_normal = normalize(src_normal * 2.0f - 1.0f);
    tex_normal = decodeNormal(tex_normal);
	//src_normal = mul(src_normal, matTBN);

	//-- Wetness --
    float surface_water = 0.0f;
    float3 wet_normal = tex_normal;

    if (Wetness > EPSILON) {
		diffuse *= 1.0f - mat.porosity * WET_DARKEN * max(pow(Wetness, 2), wet_depth);

	    surface_water = saturate(1.4 * Wetness - 0.7f * mat.porosity);

        float texWidth, texHeight, lodLevels;
        tex_albedo_alpha.GetDimensions(0, texWidth, texHeight, lodLevels);
        float water_lod = surface_water * lodLevels * WATER_BLUR * surface_NoV;

		wet_normal = tex_normal_height.SampleBias(sampler_height, tex, water_lod).xyz;
	    //wet_normal = normalize(wet_normal * 2.0f - 1.0f);
	    wet_normal = decodeNormal(wet_normal);
		//wet_normal = mul(wet_normal, matTBN);
    }

    //-- Slope Normals --
    //float3 tex_normal = src_normal;
    bool isSlope = false;

    if (EnableSlopeNormals && !EnableLinearSampling && tex_depth - shadow_tex.z > 0.002f) {
	    const float3 slope = apply_slope_normal(tex, vTS, shadow_tex.z);

	    //wet_normal = tex_normal = mul(slope, matTBN);
	    wet_normal = tex_normal = slope;
        isSlope = true;
    }

	//src_normal = mul(src_normal, matTBN);
	tex_normal = mul(tex_normal, matTBN);
	wet_normal = mul(wet_normal, matTBN);

    float water = max(surface_water, wet_depth);
	float wet_roughL = lerp(roughL, WATER_ROUGH, water);
    float wet_roughP = sqrt(wet_roughL);

    wet_normal = lerp(wet_normal, normal, saturate(water_fillFactor * wet_depth));
    wet_normal = normalize(wet_normal);

    const float NoV = saturate(dot(tex_normal, view));
    const float NoV_wet = saturate(dot(wet_normal, view));
    const float ior_in = lerp(IOR_N_AIR, IOR_N_WATER, water);

    //-- HCM --
	float3 ior_n, ior_k;
	get_hcm_ior(mat.f0_hcm, mat.albedo, ior_n, ior_k);
	float3 metal_albedo = lerp(1.0f, mat.albedo * tint, mat.f0_hcm > 0.8999 && mat.f0_hcm < 0.999);

    float3 acc_light = 0.0;
    float3 acc_sss = 0.0;
    float spec_strength = 0.0f;

    float light_att, NoL, light_shadow;
    float3 light_dir, light_color, light_diffuse, light_specular, H;
    float LoH, NoH, VoH;
    float water_shadow;

	const float4x4 mShadowViewProj = mul(vLightView, vLightProjection);

	[loop]
    for (int i = 0; i < NumLights; i++) {
        light_color = srgb_to_linear(Lights[i].vLightColor.rgb) * 4.f;

        light_shadow = 1.0f;
	    water_shadow = 1.0f;
    	
        if (Lights[i].iLightType == 1) { // directional
            //light_color *= 2.0f;
            light_dir = normalize(Lights[i].vLightDir.xyz);
			light_att = SunStrength;
        }
        else {
            light_dir = Lights[i].vLightPos.xyz - pom_wp;
            const float light_dist = length(light_dir);

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

        float sss_factor = 0.0f;

        if ((Lights[i].iLightType == 1 && EnableAtmosphere) || (Lights[i].iLightType == 3 && !EnableAtmosphere)) {
		    float h2 = 1.0f - shadow_tex.z;
		    float light_trace_dist = rcp(max(surface_NoL, EPSILON)) * h2 * BLOCK_SIZE * ParallaxDepth;
            if (surface_NoL < EPSILON) light_trace_dist = 0.0;

        	const float3 shadow_wp = pom_wp + light_trace_dist * light_dir;
			const float4 shadow_sp = mul(float4(shadow_wp, 1), mShadowViewProj);

			float thickness = SSS_Thickness(shadow_sp.xyz / shadow_sp.w, mat.sss * 3.0f);

            sss_factor = SSS_Light(tex_normal, view, light_dir, mat.sss, thickness);
			acc_sss += sss_factor * light_color * light_att;

            if (surface_NoL > 0.0f) {
                light_shadow = shadow_strength(shadow_sp.xyz / shadow_sp.w);

				if (Wetness > EPSILON) {
		            float d = max(water_levelMin, shadow_tex.z);
		            water_shadow = get_parallax_shadow(float3(water_tex.xy, d), polT, surface_NoL);


				    float water_lightTraceDist = rcp(max(surface_NoL, EPSILON)) * (1.0f - d) * BLOCK_SIZE * ParallaxDepth;
		            if (surface_NoL < EPSILON) water_lightTraceDist = 0.0;

        			const float3 water_shadow_wp = water_pom_wp + water_lightTraceDist * light_dir;
					const float4 water_shadow_sp = mul(float4(water_shadow_wp, 1), mShadowViewProj);

	                water_shadow *= shadow_strength(water_shadow_sp.xyz / water_shadow_sp.w);
		        }
            }
            else {
                light_shadow = 0.0f;
		        water_shadow = 0.0f;
            }
        }

        // light parallax shadows
        light_shadow *= get_parallax_shadow(shadow_tex, polT, surface_NoL);

        NoL = saturate(dot(tex_normal, light_dir));
        H = normalize(view + light_dir);
        LoH = saturate(dot(light_dir, H));
        VoH = saturate(dot(view, H));
		NoH = saturate(dot(tex_normal, H));

        light_diffuse = Diffuse_Burley(NoL, NoV, LoH, roughL) * diffuse * (1.0 - sss_factor);

        if (Wetness > EPSILON) {
            const float water_NoH = saturate(dot(wet_normal, H));

            const float water_F = F_full(ETA_AIR_TO_WATER, water_NoH);

            const float3 L_2 = refract(-light_dir, -H, ETA_WATER_TO_AIR);
			const float3 V_2 = refract(-view, -H, ETA_WATER_TO_AIR);
            const float3 T_2 = 1.0f - F_full(ETA_WATER_TO_AIR, saturate(dot(V_2, H)));
            const float3 H_2 = normalize(V_2 + L_2);

            //const float  NoV_2 = 0.0f; // ERROR: lerp(wet_NoV, saturate(dot(V_2, H_2)), water);
            const float  VoH_2 = lerp(VoH, saturate(dot(V_2, H_2)), water);
			const float  NoH_2 = lerp(NoH, saturate(dot(tex_normal, H_2)), water);
			const float  LoH_2 = lerp(LoH, saturate(dot(L_2, H_2)), water);

			const float3 surface_Fr = mat.f0_hcm > 0.9f
        		? specular_brdf_conductor(ior_in, ior_n, ior_k, LoH_2, NoH_2, roughL) * metal_albedo
        		: specular_brdf(ior_n.r / ior_in, LoH_2, NoH_2, VoH_2, roughL);
            
	        // ETA is purposely reversed!
			const float3 light_refractDir = refract(-light_dir, normal, ETA_WATER_TO_AIR);
	        const float refract_NoL = max(dot(-light_refractDir, normal), EPSILON);
		    const float water_depth = min(max(water_levelMin - shadow_tex.z, 0.0f) + 0.2f * surface_water, 1.0f);
		    float light_waterTraceDist = rcp(refract_NoL) * water_depth * BLOCK_SIZE * ParallaxDepth;
            if (refract_NoL < EPSILON) light_waterTraceDist = 0.0f;

			const float3 absorption = exp(-absorption_factor * max(light_waterTraceDist, 0.0f) * WATER_ABS_SCALE);

            light_specular = (1.0f - water_F) * surface_Fr * T_2 * absorption;

			const float D = D_ggx(water_NoH, wet_roughL);
			const float G = V_kelemen(LoH);
			const float F = F_full(ETA_AIR_TO_WATER, VoH);

			const float Frc = D * G * F;

			const float invFc = 1.0f - F;

            const float3 surface_diffuse = light_diffuse * light_att * NoL * invFc * light_color;
            const float3 surface_specular = light_specular * light_att * NoL * invFc * light_color;
            const float3 water_spec = Frc * water * water_shadow * light_color;

            acc_light += (surface_diffuse + surface_specular) * light_shadow + water_spec;
            spec_strength += luminance(surface_specular + water_spec);
        }
        else {
			light_specular = mat.f0_hcm > 0.9f
        		? specular_brdf_conductor(IOR_N_AIR, ior_n, ior_k, LoH, NoH, roughL) * metal_albedo
        		: specular_brdf(ior_n.r / IOR_N_AIR, LoH, NoH, VoH, roughL);

            const float3 light_factor = NoL * light_color * light_att * light_shadow;
			acc_light = mad(light_factor, light_diffuse + light_specular, acc_light);
            spec_strength += luminance(light_factor * light_specular);
        }
    }

    //const float3 ibl_f90 = 1.f;
	const float3 r = reflect(-view, wet_normal);
    const float3 ibl_f0 = ior_to_f0_complex(ior_in, ior_n, ior_k);
	const float3 ibl_F = F_schlick_roughness(ibl_f0, NoV, roughL);

	const float3 ibl_ambient = IBL_ambient(ibl_F, tex_normal) * diffuse * mat.occlusion * (1.0f - mat.sss);
	float3 ibl_specular = IBL_specular(ibl_F, NoV, r, mat.occlusion, roughP) * metal_albedo;
	float3 ibl_sss = SSS_IBL(view, ibl_F, mat.sss);
    //return float4(1.0 - ibl_F, 1.0);

    // Fix for reflected IBL passing through object
    if (isSlope) ibl_specular *= saturate(dot(r, normal) * 4.f + 1.f);

    if (Wetness > EPSILON) {
		float3 ibl_F_water = F_full(ETA_AIR_TO_WATER, NoV_wet);

        H = normalize(r + view);
        VoH = saturate(dot(view, H));
        float3 V2 = refract(-view, -H, ETA_WATER_TO_AIR);

        float T12 = 1.0f - F_full(ETA_AIR_TO_WATER, VoH) * water;
        float T21 = 1.0f - F_full(ETA_WATER_TO_AIR, saturate(dot(V2, H))) * water;

	    float h2 = saturate(water_levelMin - shadow_tex.z);
		float3 absorption = exp(-absorption_factor * max(h2, 0.0f) * WATER_ABS_SCALE);

        ibl_specular *= 1.0f + water * (T12 * T21 * absorption - 1.0f);

		ibl_specular += IBL_specular(ibl_F_water, NoV_wet, r, mat.occlusion, wet_roughP) * water;
    }

    spec_strength += luminance(ibl_specular) * 3.f;

	const float3 emissive = mat.emissive * mat.albedo;

	float3 ibl_final = ibl_ambient + ibl_specular;
    //return float4(ibl_final, 1.f);

	float3 final_sss = (acc_sss + ibl_sss) * diffuse;

    float3 final_color = 0.f;

	final_color += ibl_final;
	final_color += emissive;
	final_color += acc_light;
	final_color += final_sss;

	float alpha = saturate(mat.opacity + spec_strength);
    final_color = apply_tonemap(final_color);

	if (BlendMode == BLEND_TRANSPARENT) {
        const float ior = f0_to_ior(clamp(mat.f0_hcm, 0.0f, 0.9f));
        const float eta = IOR_N_AIR / ior;
        float3 refractDir = refract(-view, tex_normal, eta);

        if (lengthSq(refractDir) >= EPSILON) {
	        const float exit_eta = ior / IOR_N_AIR;
	        refractDir = refract(refractDir, normal, exit_eta);
        }

        if (lengthSq(refractDir) >= EPSILON) {
			const float mip = roughP * NumEnvironmentMapMipLevels;
			float3 blendColor = tex_environment.SampleLevel(sampler_environment, refractDir, mip);

			blendColor = apply_tonemap(blendColor);
            const float3 srcMul = (1.0 - alpha) + final_color * alpha;
	        final_color = final_color*alpha + blendColor*srcMul*(1.f - alpha);
        }
	}

    return float4(final_color, 1.f);
}
