#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/parallax.hlsl"
#include "lib/pbr.hlsl"

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
    	
    const float NdotV = saturate(dot(normal, eye));

	const float rough = rough_f0_occlusion.r;
	const float f0_gray = rough_f0_occlusion.g;
	const float occlusion = rough_f0_occlusion.b;
	const float emissive = porosity_sss_emissive.b;
	
    // Burley roughness bias
    const float alpha = rough * rough;

    // Blend base colors
	const float metal = f0_gray > 0.5 ? 1 : 0;
    const float3 c_diff = lerp(albedo_alpha.rgb, float3(0, 0, 0), metal) * occlusion;

    //float3 f0 = float3(f0_gray, f0_gray, f0_gray); // * rough_f0_occlusion.b; //0.16 * reflectance * reflectance * (1 - metal) + albedo_alpha.rgb * metal; //lerp(reflectance, albedo, metallic) * ambientOcclusion;
    const float3 f0 = float3(f0_gray, f0_gray, f0_gray);
	const float3 spec = 0.16 * f0_gray * f0_gray * (1 - f0_gray) + albedo_alpha.rgb * f0_gray; //lerp(reflectance, albedo, metallic) * ambientOcclusion;

	// TODO: avoid branching and add HCM support
    //if (f0_gray > 0.5f) {
    //	c_diff = float3(0, 0, 0);
    //}
    //if (f0_gray > 0.9f) {
    //	f0 = albedo_alpha.rgb;
    //}
	
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
            const float3 diffuse = c_diff * diffuse_factor;
            const float3 specular = Specular_BRDF(alpha, f0, NdotV, NdotL, LdotH, NdotH, normal, H);
          
            acc_color += NdotL * Lights[i].vLightColor.rgb * (diffuse + specular);
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
            const float3 diffuse = c_diff * diffuse_factor;
            const float3 specular = Specular_BRDF(alpha, f0, NdotV, NdotL, LdotH, NdotH, normal, H);
            const float att = 1.0f / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * dl + Lights[i].vLightAtt.z * dl * dl);
            acc_color = mad(att, NdotL * Lights[i].vLightColor.rgb * (diffuse + specular), acc_color);
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
            const float3 diffuse = c_diff * diffuse_factor;
            const float3 specular = Specular_BRDF(alpha, f0, NdotV, NdotL, LdotH, NdotH, normal, H);

            const float rho = dot(-L, sd);
            const float spot = pow(saturate((rho - Lights[i].vLightSpot.x) / (Lights[i].vLightSpot.y - Lights[i].vLightSpot.x)), Lights[i].vLightSpot.z);
            const float att = spot / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * dl + Lights[i].vLightAtt.z * dl * dl);
            acc_color = mad(att, NdotL * Lights[i].vLightColor.rgb * (diffuse + specular), acc_color);
        }
    }

	float3 specular_env = vLightAmbient.rgb * occlusion;

	if (bHasCubeMap)
        specular_env = Specular_IBL(normal, eye, rough);

    float3 lit = acc_color + spec * specular_env;


	
    if (bRenderShadowMap)
        lit *= shadow_strength(input.sp);

	const float3 ambient = albedo_alpha.rgb * (emissive + vLightAmbient.rgb);
	
    return float4(ambient + lit, albedo_alpha.a);
}
