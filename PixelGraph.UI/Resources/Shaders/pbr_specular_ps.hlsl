#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/parallax.hlsl"
#include "lib/pbr.hlsl"

#pragma pack_matrix(row_major)

/* TEXTURE PACKING
 *   tex_albedo_alpha
 *     r=red
 *     g=green
 *     b=blue
 *     a=alpha
 *   tex_normal_height
 *     r=normal-x
 *     g=normal-y
 *     b=normal-z
 *     a=height
 *   tex_rough_f0_occlusion
 *     r=rough
 *     g=f0
 *     b=occlusion
 *   tex_porosity_sss_emissive
 *     r=porosity
 *     g=sss
 *     b=emissive
 */


float4 main(const ps_input input) : SV_TARGET
{
	const float3 normal = normalize(input.nor);
    const float3 tangent = normalize(input.tan);
    const float3 bitangent = normalize(input.bin);
	const float3 eye = normalize(input.eye.xyz);
	
	const float2 parallax_tex = get_parallax_texcoord(input.tex, input.poT, normal, eye);
	const float3 normalT = calc_normal(parallax_tex, normal, tangent, bitangent);

	const float4 albedo_alpha = tex_albedo_alpha.Sample(sampler_surface, parallax_tex);
	const float3 rough_f0_occlusion = tex_rough_f0_occlusion.Sample(sampler_surface, parallax_tex).rgb;
	const float3 porosity_sss_emissive = tex_porosity_sss_emissive.Sample(sampler_surface, parallax_tex).rgb;

	
	const float rough = rough_f0_occlusion.r;
	const float f0r = rough_f0_occlusion.g;
	const float occlusion = rough_f0_occlusion.b;
	const float emissive = porosity_sss_emissive.b;
    
    // Blend base colors
	const float metal = f0r > 0.5 ? 1 : 0;
    const float3 diffuse = lerp(albedo_alpha.rgb, float3(0, 0, 0), metal);

	
	float3 metal_albedo = albedo_alpha.rgb;

    if (f0r > 0.900 && f0r < 0.902) { // 230: IRON
    	metal_albedo = float3(0.77, 0.78, 0.78);
    }
    else if (f0r > 0.902 && f0r < 0.906) { // 231: GOLD
    	metal_albedo = float3(1.00, 0.85, 0.57);
    }
    else if (f0r > 0.906 && f0r < 0.910) { // 232: TITANIUM
    	metal_albedo = float3(0.91, 0.92, 0.92);
    }
    else if (f0r > 0.910 && f0r < 0.914) { // 233: CHROME
    	metal_albedo = float3(0, 0, 0); // WARN: MISSING
    }
    else if (f0r > 0.914 && f0r < 0.918) { // 234: COPPER
    	metal_albedo = float3(0.97, 0.74, 0.62);
    }
    else if (f0r > 0.918 && f0r < 0.922) { // 235: LEAD
    	metal_albedo = float3(0, 0, 0); // WARN: MISSING
    }
    else if (f0r > 0.922 && f0r < 0.926) { // 236: PLATINUM
    	metal_albedo = float3(0.83, 0.81, 0.78);
    }
    else if (f0r > 0.926 && f0r < 0.930) { // 237: SILVER
    	metal_albedo = float3(0.97, 0.96, 0.91);
    }
	
	const float3 f0 = f0r * (1 - metal) + metal_albedo * metal * occlusion;
	
	
    float3 acc_color = 0;
    const float NdotV = saturate(dot(normalT, eye));
    const float alpha = rough * rough;

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
            const float diffuse_factor = Diffuse_Burley(NdotL, NdotV, LdotH, rough);
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
            const float diffuse_factor = Diffuse_Burley(NdotL, NdotV, LdotH, rough);
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
            const float diffuse_factor = Diffuse_Burley(NdotL, NdotV, LdotH, rough);
            const float3 light_diffuse = diffuse * diffuse_factor;
            const float3 light_specular = Specular_BRDF(alpha, f0, NdotV, NdotL, LdotH, NdotH, normalT, H);

            const float rho = dot(-light_dir, sd);
            const float spot = pow(saturate((rho - Lights[i].vLightSpot.x) / (Lights[i].vLightSpot.y - Lights[i].vLightSpot.x)), Lights[i].vLightSpot.z);
            const float att = spot / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * dl + Lights[i].vLightAtt.z * dl * dl);
            acc_color = mad(att, NdotL * Lights[i].vLightColor.rgb * (light_diffuse + light_specular), acc_color);
        }
    }

	float3 specular_env = vLightAmbient.rgb * occlusion;

	if (bHasCubeMap)
        specular_env = Specular_IBL(normalT, eye, rough);

    //return float4(f0 * specular_env, 1);
	
    float3 lit = acc_color + f0 * specular_env;


	
    if (bRenderShadowMap)
        lit *= shadow_strength(input.sp);

	const float3 ambient = albedo_alpha.rgb * (emissive + vLightAmbient.rgb);
	
    return float4(ambient + lit, albedo_alpha.a);
}
