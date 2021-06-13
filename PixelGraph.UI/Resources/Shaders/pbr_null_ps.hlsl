#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/parallax.hlsl"
#include "lib/pbr_material.hlsl"
#include "lib/pbr_filament.hlsl"

#pragma pack_matrix(row_major)

static const float3 black = 0;
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
	const float3 view = normalize(input.eye.xyz);

	float2 dx = ddx(input.tex);
	float2 dy = ddy(input.tex);
	
	float tex_depth;
	const float2 tex = get_parallax_texcoord(input.tex, dx, dy, normal, input.poT, view, tex_depth);

	float SNdotV = saturate(dot(normal, view));
	float bias = (1.0 - SNdotV) * 0.003 + 0.002;
    float side_bias = saturate((tex_depth - bias) * 200.0);
	
	const float3 tex_normal = calc_tex_normal(tex, normal, tangent, bitangent);
	pbr_material mat = get_pbr_material(tex);

	clip(mat.alpha < EPSILON);
	        
    // Blend base colors
	const float metal = mat.f0 > 0.5 ? 1.0 : 0.0;
    const float3 diffuse = lerp(mat.albedo, black, metal);	
	float3 metal_albedo = mat.albedo;

    if (mat.f0 > 0.900 && mat.f0 < 0.902) metal_albedo = f0_iron;
    else if (mat.f0 > 0.902 && mat.f0 < 0.906) metal_albedo = f0_gold;
    else if (mat.f0 > 0.906 && mat.f0 < 0.910) metal_albedo = f0_titanium;
    else if (mat.f0 > 0.910 && mat.f0 < 0.914) metal_albedo = f0_chrome;
    else if (mat.f0 > 0.914 && mat.f0 < 0.918) metal_albedo = f0_copper;
    else if (mat.f0 > 0.918 && mat.f0 < 0.922) metal_albedo = f0_lead;
    else if (mat.f0 > 0.922 && mat.f0 < 0.926) metal_albedo = f0_platinum;
    else if (mat.f0 > 0.926 && mat.f0 < 0.930) metal_albedo = f0_silver;
	
	const float3 f0 = mat.f0 * (1.0 - metal) + metal_albedo * metal; // * mat.occlusion;
	
    const float alpha = max(mat.rough * mat.rough, EPSILON);
    const float NdotV = saturate(dot(tex_normal, view));
    const float3x3 mTBN = float3x3(tangent, bitangent, normal);
    
    float3 acc_color = 0.0;
    for (int i = 0; i < NumLights; i++) {
            	
        if (Lights[i].iLightType == 1) {
            // light vector (to light)
            const float3 light_dir = normalize(Lights[i].vLightDir.xyz);

        	// light parallax shadows
        	const float2 polT = get_parallax_offset(mTBN, light_dir);
            const float shadow = get_parallax_shadow(tex, side_bias, dx, dy, normal, polT, light_dir);
            if (shadow < EPSILON) continue;
        	
            // Half vector
            const float3 H = normalize(light_dir + view);

            // products
            const float NdotL = saturate(dot(tex_normal, light_dir));
            const float LdotH = saturate(dot(light_dir, H));
            const float NdotH = saturate(dot(tex_normal, H));
        	
            // Diffuse & specular factors
            const float diffuse_factor = Diffuse_Burley(f0, NdotL, NdotV, LdotH, mat.rough);
            const float3 light_diffuse = diffuse * diffuse_factor;
            const float3 light_specular = Specular_BRDF(alpha, f0, NdotV, NdotL, LdotH, NdotH, tex_normal, H);
          
            acc_color += NdotL * Lights[i].vLightColor.rgb * (light_diffuse + light_specular) * shadow;
        }
        else if (Lights[i].iLightType == 2) {
            float3 light_dir = Lights[i].vLightPos.xyz - input.wp.xyz; // light dir
            const float dl = length(light_dir); // light distance
            if (Lights[i].vLightAtt.w < dl) continue;

            light_dir = light_dir / dl; // normalized light dir

        	// light parallax shadows
        	const float2 polT = get_parallax_offset(mTBN, light_dir);
            const float shadow = get_parallax_shadow(tex, side_bias, dx, dy, normal, polT, light_dir);
            if (shadow < EPSILON) continue;
        	
            const float3 H = normalize(view + light_dir); // half direction for specular

        	// products
            const float NdotL = saturate(dot(tex_normal, light_dir));
            const float LdotH = saturate(dot(light_dir, H));
            const float NdotH = saturate(dot(tex_normal, H));
        	
            // Diffuse & specular factors
            const float diffuse_factor = Diffuse_Burley(f0, NdotL, NdotV, LdotH, mat.rough);
            const float3 light_diffuse = diffuse * diffuse_factor;
            const float3 light_specular = Specular_BRDF(alpha, f0, NdotV, NdotL, LdotH, NdotH, tex_normal, H);
            const float att = 1.0 / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * dl + Lights[i].vLightAtt.z * dl * dl);
            acc_color = mad(att * shadow, NdotL * Lights[i].vLightColor.rgb * (light_diffuse + light_specular), acc_color);
        }
        else if (Lights[i].iLightType == 3) {
            float3 light_dir = Lights[i].vLightPos.xyz - input.wp.xyz; // light dir
            const float dl = length(light_dir); // light distance
            if (Lights[i].vLightAtt.w < dl) continue;

        	// light parallax shadows
        	const float2 polT = get_parallax_offset(mTBN, light_dir);
            const float shadow = get_parallax_shadow(tex, side_bias, dx, dy, normal, polT, light_dir);
            if (shadow < EPSILON) continue;
        	
            light_dir = light_dir / dl; // normalized light dir
        	
            const float3 H = normalize(view + light_dir); // half direction for specular
            const float3 sd = normalize(Lights[i].vLightDir.xyz); // missuse the vLightDir variable for spot-dir

            const float NdotL = saturate(dot(tex_normal, light_dir));
            const float LdotH = saturate(dot(light_dir, H));
            const float NdotH = saturate(dot(tex_normal, H));
        	
            // Diffuse & specular factors
            const float diffuse_factor = Diffuse_Burley(f0, NdotL, NdotV, LdotH, mat.rough);
            const float3 light_diffuse = diffuse * diffuse_factor;
            const float3 light_specular = Specular_BRDF(alpha, f0, NdotV, NdotL, LdotH, NdotH, tex_normal, H);

            const float rho = dot(-light_dir, sd);
            const float spot = pow(saturate((rho - Lights[i].vLightSpot.x) / (Lights[i].vLightSpot.y - Lights[i].vLightSpot.x)), Lights[i].vLightSpot.z);
            const float att = spot / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * dl + Lights[i].vLightAtt.z * dl * dl);
            acc_color = mad(att * shadow, NdotL * Lights[i].vLightColor.rgb * (light_diffuse + light_specular), acc_color);
        }
    }

	float3 ambient_color = srgb_to_linear(vLightAmbient.rgb);
	float3 specular_env = ambient_color;

	if (bHasCubeMap) {
		ambient_color = diffuse_IBL(tex_normal, alpha);
        specular_env = specular_IBL(tex_normal, view, alpha);
		//return float4(ambient_color, 1);
	}

    //if (bRenderShadowMap)
    //    acc_color *= shadow_strength(input.sp);


	const float3 ambient_spec = f0 * specular_env;
	const float3 ambient_diff = ambient_color * diffuse;
	const float3 ambient = (ambient_diff * (1.0 - f0) + ambient_spec) * mat.occlusion;
	//return float4(ambient_spec, 1);
		
	const float3 emissive = mat.emissive * mat.albedo * PI;
	
    float3 final_color = ambient + acc_color + emissive;
	
	//final_color = ACESFilm(final_color);
    final_color = linear_to_srgb(final_color);
	
    return float4(final_color, mat.alpha);
}
