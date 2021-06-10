#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/parallax.hlsl"
#include "lib/pbr_material.hlsl"
#include "lib/pbr.hlsl"

#pragma pack_matrix(row_major)


float4 main(const ps_input input) : SV_TARGET
{
	const float3 normal = normalize(input.nor);
    const float3 tangent = normalize(input.tan);
    const float3 bitangent = normalize(input.bin);
	const float3 eye = normalize(input.eye.xyz);

	const float2 tex = get_parallax_texcoord(input.tex, input.poT, normal, eye);
	const float3 normalT = calc_normal(tex, normal, tangent, bitangent);
	pbr_material mat = get_pbr_material(tex);
	
	//mat.albedo = srgb_to_linear(mat.albedo);
	//mat.rough = mat.rough * mat.rough;

    
    // Blend base colors
	const float metal = mat.f0 > 0.5 ? 1 : 0;
    const float3 diffuse = lerp(mat.albedo, float3(0, 0, 0), metal);	
	float3 metal_albedo = mat.albedo;

    if (mat.f0 > 0.900 && mat.f0 < 0.902) { // 230: IRON
    	metal_albedo = float3(0.77, 0.78, 0.78);
    }
    else if (mat.f0 > 0.902 && mat.f0 < 0.906) { // 231: GOLD
    	metal_albedo = float3(1.00, 0.85, 0.57);
    }
    else if (mat.f0 > 0.906 && mat.f0 < 0.910) { // 232: TITANIUM
    	metal_albedo = float3(0.91, 0.92, 0.92);
    }
    else if (mat.f0 > 0.910 && mat.f0 < 0.914) { // 233: CHROME
    	metal_albedo = float3(0, 0, 0); // WARN: MISSING
    }
    else if (mat.f0 > 0.914 && mat.f0 < 0.918) { // 234: COPPER
    	metal_albedo = float3(0.97, 0.74, 0.62);
    }
    else if (mat.f0 > 0.918 && mat.f0 < 0.922) { // 235: LEAD
    	metal_albedo = float3(0, 0, 0); // WARN: MISSING
    }
    else if (mat.f0 > 0.922 && mat.f0 < 0.926) { // 236: PLATINUM
    	metal_albedo = float3(0.83, 0.81, 0.78);
    }
    else if (mat.f0 > 0.926 && mat.f0 < 0.930) { // 237: SILVER
    	metal_albedo = float3(0.97, 0.96, 0.91);
    }
	
	const float3 f0 = mat.f0 * (1 - metal) + metal_albedo * metal * mat.occlusion;
	
    const float alpha = mat.rough * mat.rough;
    const float NdotV = saturate(dot(normalT, eye));

    float3 acc_color = 0;
    for (int i = 0; i < NumLights; i++) {
        if (Lights[i].iLightType == 1) {
            // light vector (to light)
            const float3 light_dir = normalize(Lights[i].vLightDir.xyz);

            // Half vector
            const float3 H = normalize(light_dir + eye);

            // products
            const float NdotL = saturate(dot(normalT, light_dir));
            const float LdotH = saturate(dot(light_dir, H));
            const float NdotH = saturate(dot(normalT, H));
        	
            // Diffuse & specular factors
            const float diffuse_factor = Diffuse_Burley(NdotL, NdotV, LdotH, mat.rough);
            const float3 light_diffuse = diffuse * diffuse_factor;
            const float3 light_specular = Specular_BRDF(alpha, f0, NdotV, NdotL, LdotH, NdotH, normalT, H);
          
            acc_color += NdotL * Lights[i].vLightColor.rgb * (light_diffuse + light_specular);
        }
        else if (Lights[i].iLightType == 2) {
            float3 light_dir = Lights[i].vLightPos.xyz - input.wp.xyz; // light dir
            const float dl = length(light_dir); // light distance
            if (Lights[i].vLightAtt.w < dl) continue;

            light_dir = light_dir / dl; // normalized light dir						
            const float3 H = normalize(eye + light_dir); // half direction for specular

        	// products
            const float NdotL = saturate(dot(normalT, light_dir));
            const float LdotH = saturate(dot(light_dir, H));
            const float NdotH = saturate(dot(normalT, H));
        	
            // Diffuse & specular factors
            const float diffuse_factor = Diffuse_Burley(NdotL, NdotV, LdotH, mat.rough);
            const float3 light_diffuse = diffuse * diffuse_factor;
            const float3 light_specular = Specular_BRDF(alpha, f0, NdotV, NdotL, LdotH, NdotH, normalT, H);
            const float att = 1.0f / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * dl + Lights[i].vLightAtt.z * dl * dl);
            acc_color = mad(att, NdotL * Lights[i].vLightColor.rgb * (light_diffuse + light_specular), acc_color);
        }
        else if (Lights[i].iLightType == 3) {
            float3 light_dir = Lights[i].vLightPos.xyz - input.wp.xyz; // light dir
            const float dl = length(light_dir); // light distance
            if (Lights[i].vLightAtt.w < dl) continue;
        	
            light_dir = light_dir / dl; // normalized light dir					
            const float3 H = normalize(eye + light_dir); // half direction for specular
            const float3 sd = normalize(Lights[i].vLightDir.xyz); // missuse the vLightDir variable for spot-dir

            const float NdotL = saturate(dot(normalT, light_dir));
            const float LdotH = saturate(dot(light_dir, H));
            const float NdotH = saturate(dot(normalT, H));
        	
            // Diffuse & specular factors
            const float diffuse_factor = Diffuse_Burley(NdotL, NdotV, LdotH, mat.rough);
            const float3 light_diffuse = diffuse * diffuse_factor;
            const float3 light_specular = Specular_BRDF(alpha, f0, NdotV, NdotL, LdotH, NdotH, normalT, H);

            const float rho = dot(-light_dir, sd);
            const float spot = pow(saturate((rho - Lights[i].vLightSpot.x) / (Lights[i].vLightSpot.y - Lights[i].vLightSpot.x)), Lights[i].vLightSpot.z);
            const float att = spot / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * dl + Lights[i].vLightAtt.z * dl * dl);
            acc_color = mad(att, NdotL * Lights[i].vLightColor.rgb * (light_diffuse + light_specular), acc_color);
        }
    }

	float3 specular_env = vLightAmbient.rgb;

	if (bHasCubeMap)
        specular_env = specular_IBL(normalT, eye, mat.rough);

    //specular_env = srgb_to_linear(specular_env);

    //return float4(f0 * specular_env, 1);
	
    if (bRenderShadowMap)
        acc_color *= shadow_strength(input.sp);
	
	const float3 lit = acc_color + f0 * specular_env * mat.occlusion;
	const float3 ambient = vLightAmbient.rgb * mat.occlusion;
    float3 final_color = lit + (ambient + mat.emissive * PI) * mat.albedo;
	
    //final_color = linear_to_srgb(final_color);
	
    return float4(final_color, mat.alpha);
}
