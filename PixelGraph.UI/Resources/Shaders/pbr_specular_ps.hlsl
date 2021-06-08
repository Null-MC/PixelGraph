#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/parallax.hlsl"
#include "lib/pbr.hlsl"

//#define IRON float3(2.9114, 2.9497, 2.5845)
//#define GOLD float3(0.18299, 0.42108, 1.3734)
//#define ALUMINUM float3(1.3456, 0.96521, 0.61722)
//#define CHROME float3(3.1071, 3.1812, 2.3230)
//#define COPPER float3(0.27105, 0.67693, 1.3164)
//#define LEAD float3(1.9100, 1.8300, 1.4400)
//#define PLATINUM float3(2.3757, 2.0847, 1.8453)
//#define SILVER float3(0.15943, 0.14512, 0.13547)

#pragma pack_matrix( row_major )

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
	const float2 parallax_tex = get_parallax_texcoord(input);
	
	const float4 albedo_alpha = tex_albedo_alpha.Sample(sampler_surface, parallax_tex);
	const float3 rough_f0_occlusion = tex_rough_f0_occlusion.Sample(sampler_surface, parallax_tex).rgb;
	const float3 porosity_sss_emissive = tex_porosity_sss_emissive.Sample(sampler_surface, parallax_tex).rgb;

	const float3 eye = normalize(input.eye.xyz);
	const float3 normal = calc_normal(parallax_tex, input.nor, input.tan, input.bin);

	const float rough = rough_f0_occlusion.r;
	const float f0r = rough_f0_occlusion.g;
	const float occlusion = rough_f0_occlusion.b;
	const float emissive = porosity_sss_emissive.b;
    	
    const float NdotV = saturate(dot(normal, eye));
	
    // Burley roughness bias
    const float alpha = rough * rough;

    // Blend base colors
	const float metal = f0r > 0.5 ? 1 : 0;
    const float3 diffuse = lerp(albedo_alpha.rgb, float3(0, 0, 0), metal); //* occlusion;

	
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
	else if (f0r > 0.999) {
		//metal_albedo = float3(10, 10, 10);
    }

	//const float f0 = ior_to_f0(ior);
	
	const float3 f0 = 0.16 * f0r * f0r * (1 - metal) + metal_albedo * metal;

	
	// Output color
    float3 acc_color = 0;

    // Accumulate light values
    for (int i = 0; i < NumLights; i++) {
        if (Lights[i].iLightType == 1) {
            // light vector (to light)
            const float3 L = normalize(Lights[i].vLightDir.xyz);

            // Half vector
            const float3 H = normalize(L + eye);

            // products
            const float NdotL = saturate(dot(normal, L));
            const float LdotH = saturate(dot(L, H));
            const float NdotH = saturate(dot(normal, H));
        	
            // Diffuse & specular factors
            const float diffuse_factor = Diffuse_Burley(NdotL, NdotV, LdotH, rough);
            const float3 light_diffuse = diffuse * diffuse_factor;
            const float3 light_specular = Specular_BRDF(alpha, f0, NdotV, NdotL, LdotH, NdotH, normal, H);
          
            acc_color += NdotL * Lights[i].vLightColor.rgb * (light_diffuse + light_specular);
        }
        else if (Lights[i].iLightType == 2) {
            float3 L = (Lights[i].vLightPos - input.wp).xyz; // light dir
            const float dl = length(L); // light distance
            if (Lights[i].vLightAtt.w < dl) continue;

            L = L / dl; // normalized light dir						
            const float3 H = normalize(eye + L); // half direction for specular

        	// products
            const float NdotL = saturate(dot(normal, L));
            const float LdotH = saturate(dot(L, H));
            const float NdotH = saturate(dot(normal, H));
        	
            // Diffuse & specular factors
            const float diffuse_factor = Diffuse_Burley(NdotL, NdotV, LdotH, rough);
            const float3 light_diffuse = diffuse * diffuse_factor;
            const float3 light_specular = Specular_BRDF(alpha, f0, NdotV, NdotL, LdotH, NdotH, normal, H);
            const float att = 1.0f / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * dl + Lights[i].vLightAtt.z * dl * dl);
            acc_color = mad(att, NdotL * Lights[i].vLightColor.rgb * (light_diffuse + light_specular), acc_color);
        }
        else if (Lights[i].iLightType == 3) {
            float3 L = (Lights[i].vLightPos - input.wp).xyz; // light dir
            const float dl = length(L); // light distance
            if (Lights[i].vLightAtt.w < dl) continue;
        	
            L = L / dl; // normalized light dir					
            const float3 H = normalize(eye + L); // half direction for specular
            const float3 sd = normalize(Lights[i].vLightDir.xyz); // missuse the vLightDir variable for spot-dir

            const float NdotL = saturate(dot(normal, L));
            const float LdotH = saturate(dot(L, H));
            const float NdotH = saturate(dot(normal, H));
        	
            // Diffuse & specular factors
            const float diffuse_factor = Diffuse_Burley(NdotL, NdotV, LdotH, rough);
            const float3 light_diffuse = diffuse * diffuse_factor;
            const float3 light_specular = Specular_BRDF(alpha, f0, NdotV, NdotL, LdotH, NdotH, normal, H);

            const float rho = dot(-L, sd);
            const float spot = pow(saturate((rho - Lights[i].vLightSpot.x) / (Lights[i].vLightSpot.y - Lights[i].vLightSpot.x)), Lights[i].vLightSpot.z);
            const float att = spot / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * dl + Lights[i].vLightAtt.z * dl * dl);
            acc_color = mad(att, NdotL * Lights[i].vLightColor.rgb * (light_diffuse + light_specular), acc_color);
        }
    }

	float3 specular_env = vLightAmbient.rgb * occlusion;

	if (bHasCubeMap)
        specular_env = Specular_IBL(normal, eye, rough);

    float3 lit = acc_color + f0 * specular_env;


	
    if (bRenderShadowMap)
        lit *= shadow_strength(input.sp);

	const float3 ambient = albedo_alpha.rgb * (emissive + vLightAmbient.rgb);
	
    return float4(ambient + lit, albedo_alpha.a);
}
