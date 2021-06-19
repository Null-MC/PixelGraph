#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/parallax.hlsl"
#include "lib/pbr_material.hlsl"
#include "lib/pbr_filament2.hlsl"
#include "lib/tonemap.hlsl"

#define MIN_ROUGH 0.002
#define WATER_ROUGH 0.03
#define WET_DARKEN 0.88

#pragma pack_matrix(row_major)

//static const float3 black = 0;
static const float3 f0_iron = float3(0.56, 0.57, 0.58); // 230
static const float3 f0_gold = float3(1, 0.71, 0.29); // 231
static const float3 f0_titanium = float3(0.91, 0.92, 0.92); // 232
static const float3 f0_chrome = float3(0.550, 0.556, 0.554); // 233
static const float3 f0_copper = float3(0.955, 0.637, 0.538); // 234
static const float3 f0_lead = float3(0.0, 0.0, 0.0); // 235 - WARN: MISSING
static const float3 f0_platinum = float3(0.83, 0.81, 0.78); // 236
static const float3 f0_silver = float3(0.97, 0.96, 0.91); // 237


float4 main(const ps_input input) : SV_TARGET
{
	const float3 normal = normalize(input.nor);
    const float3 tangent = normalize(input.tan);
    const float3 bitangent = normalize(input.bin);
	const float3 view = normalize(input.eye);
	const float2 dx = ddx(input.tex);
	const float2 dy = ddy(input.tex);
	

	float3 shadow_tex = 0;
    const float SNoV = saturate(dot(normal, view));
	const float2 tex = get_parallax_texcoord(input.tex, dx, dy, input.poT, SNoV, shadow_tex);
	float3 tex_normal = calc_tex_normal(tex, normal, tangent, bitangent);
	const pbr_material mat = get_pbr_material(tex);
	
	clip(mat.alpha - EPSILON);

    const float3x3 mTBN = float3x3(tangent, bitangent, normal);
	float roughL = max(mat.rough * mat.rough, MIN_ROUGH);
	
    // Blend base colors
	const float metal = mat.f0 > 0.5 ? 1.0 : 0.0;
    float3 diffuse = lerp(mat.albedo, black, metal);

	// Wetness
    float p_diffuse = 1.0 - WET_DARKEN * mat.porosity;
	diffuse *= lerp(1.0, p_diffuse, Wetness);

    float surface_water = saturate(Wetness * (2.0 - roughL) * (1.0 - mat.porosity));
	roughL = lerp(roughL, WATER_ROUGH, surface_water);
	//tex_normal = normalize(lerp(tex_normal, normal, surface_water * 0.5));

    const float f0r = lerp(mat.f0, 0.02, surface_water * (1.0 - metal));
        
    // HCM
	float3 metal_f0 = mat.albedo;
    if (f0r > 0.900 && mat.f0 < 0.902) metal_f0 *= f0_iron;
    else if (f0r > 0.902 && f0r <= 0.906) metal_f0 *= f0_gold;
    else if (f0r > 0.906 && f0r <= 0.910) metal_f0 *= f0_titanium;
    else if (f0r > 0.910 && f0r <= 0.914) metal_f0 *= f0_chrome;
    else if (f0r > 0.914 && f0r <= 0.918) metal_f0 *= f0_copper;
    else if (f0r > 0.918 && f0r <= 0.922) metal_f0 *= f0_lead;
    else if (f0r > 0.922 && f0r <= 0.926) metal_f0 *= f0_platinum;
    else if (f0r > 0.926 && f0r <= 0.930) metal_f0 *= f0_silver;
    //else if (f0r > 0.930) metal_f0 *= mat.albedo;
	
	const float3 f0 = f0r * (1.0 - metal) + metal_f0 * metal;
    	
    const float NoV = saturate(dot(tex_normal, view));
    
    float3 acc_diffuse = 0.0;
    float3 acc_specular = 0.0;
    float3 acc_sss = 0.0;
	[loop]
    for (int i = 0; i < NumLights; i++) {
        if (Lights[i].iLightType == 1) {
            // light vector (to light)
            const float3 light_dir = normalize(Lights[i].vLightDir.xyz);
        	
            acc_sss += Lights[i].vLightColor.rgb * pow(saturate(dot(-view, light_dir)), 60.0) * mat.sss * diffuse * 2.0;

        	// light parallax shadows
        	const float SNoL = dot(normal, light_dir);
        	const float2 polT = get_parallax_offset(mTBN, light_dir);
            const float shadow = get_parallax_shadow(shadow_tex, dx, dy, polT, SNoL);
            if (shadow < EPSILON) continue;
        	
            // Half vector
            const float3 H = normalize(light_dir + view);

            // products
            const float NoL = saturate(dot(tex_normal, light_dir));
            const float LoH = saturate(dot(light_dir, H));
            const float NoH = saturate(dot(tex_normal, H));
            const float VoH = saturate(dot(view, H));
        	
            // Diffuse & specular factors
            const float3 light_diffuse = disney_diffuse(diffuse, NoV, NoL, LoH, roughL);
            const float3 light_specular = Specular_BRDF(f0, tex_normal, H, LoH, NoH, VoH, roughL);
        	
            acc_diffuse += NoL * Lights[i].vLightColor.rgb * light_diffuse * shadow;
            acc_specular += NoL * Lights[i].vLightColor.rgb * light_specular * shadow;
        }
        else if (Lights[i].iLightType == 2) {
            float3 light_dir = Lights[i].vLightPos.xyz - input.wp.xyz; // light dir
            const float dl = length(light_dir); // light distance
            light_dir = light_dir / dl; // normalized light dir
        	
            acc_sss += Lights[i].vLightColor.rgb * pow(saturate(dot(-view, light_dir)), 30.0) * mat.sss * diffuse * 2.0;
        	
            if (Lights[i].vLightAtt.w < dl) continue;

        	// light parallax shadows
        	const float SNoL = dot(normal, light_dir);
        	const float2 polT = get_parallax_offset(mTBN, light_dir);
            const float shadow = get_parallax_shadow(shadow_tex, dx, dy, polT, SNoL);
            if (shadow < EPSILON) continue;
        	
            const float3 H = normalize(view + light_dir); // half direction for specular

        	// products
            const float NoL = saturate(dot(tex_normal, light_dir));
            const float LoH = saturate(dot(light_dir, H));
            const float NoH = saturate(dot(tex_normal, H));
            const float VoH = saturate(dot(view, H));
        	
            // Diffuse & specular factors
            const float3 light_diffuse = disney_diffuse(diffuse, NoV, NoL, LoH, roughL);
            const float3 light_specular = Specular_BRDF(f0, tex_normal, H, LoH, NoH, VoH, roughL);
            const float att = 1.0 / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * dl + Lights[i].vLightAtt.z * dl * dl);
            acc_diffuse = mad(att * shadow, NoL * Lights[i].vLightColor.rgb * light_diffuse, acc_diffuse);
            acc_specular = mad(att * shadow, NoL * Lights[i].vLightColor.rgb * light_specular, acc_specular);
        }
        else if (Lights[i].iLightType == 3) {
            float3 light_dir = Lights[i].vLightPos.xyz - input.wp.xyz; // light dir
        	
            acc_sss += Lights[i].vLightColor.rgb * pow(saturate(dot(-view, light_dir)), 60.0) * mat.sss * diffuse * 2.0;
        	
            const float dl = length(light_dir); // light distance
            if (Lights[i].vLightAtt.w < dl) continue;
        	
            light_dir = light_dir / dl; // normalized light dir

        	// light parallax shadows
        	const float SNoL = dot(normal, light_dir);
        	const float2 polT = get_parallax_offset(mTBN, light_dir);
            const float shadow = get_parallax_shadow(shadow_tex, dx, dy, polT, SNoL);
            if (shadow < EPSILON) continue;
        	
            const float3 H = normalize(view + light_dir); // half direction for specular
            const float3 sd = normalize(Lights[i].vLightDir.xyz); // missuse the vLightDir variable for spot-dir

            const float NoL = saturate(dot(tex_normal, light_dir));
            const float LoH = saturate(dot(light_dir, H));
            const float NoH = saturate(dot(tex_normal, H));
            const float VoH = saturate(dot(view, H));
        	
            // Diffuse & specular factors
            const float3 light_diffuse = disney_diffuse(diffuse, NoV, NoL, LoH, roughL);
            const float3 light_specular = Specular_BRDF(f0, tex_normal, H, LoH, NoH, VoH, roughL);

            const float rho = dot(-light_dir, sd);
            const float spot = pow(saturate((rho - Lights[i].vLightSpot.x) / (Lights[i].vLightSpot.y - Lights[i].vLightSpot.x)), Lights[i].vLightSpot.z);
            const float att = spot / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * dl + Lights[i].vLightAtt.z * dl * dl);
            acc_diffuse = mad(att * shadow, NoL * Lights[i].vLightColor.rgb * light_diffuse, acc_diffuse);
            acc_specular = mad(att * shadow, NoL * Lights[i].vLightColor.rgb * light_specular, acc_specular);
        }
    }

	const float3 ibl = IBL(tex_normal, view, diffuse, f0, 1.0, mat.occlusion, roughL);
	
	const float3 emissive = mat.emissive * mat.albedo * PI;

	if (bHasCubeMap) {
		const int level = int((1.0 - mat.sss) * NumEnvironmentMapMipLevels);
        acc_sss += tex_environment.SampleLevel(sampler_environment, view, level) * mat.sss * diffuse;
	}
	
	//final_color = ACESFilm(final_color);
    float3 final_color = acc_diffuse + acc_sss + acc_specular + ibl + emissive;
	float alpha = mat.alpha + lum(acc_specular);

	final_color = tonemap_HejlBurgess(final_color);
	//final_color = linear_to_srgb(final_color);
	
    return float4(final_color, alpha);
}
