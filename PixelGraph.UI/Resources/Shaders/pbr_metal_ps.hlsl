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

	const float2 parallax_tex = get_parallax_texcoord(input.tex, input.poT, normal, eye);
	const float3 normalT = calc_normal(parallax_tex, normal, tangent, bitangent);

	const pbr_material mat = get_pbr_material(parallax_tex);
	const float reflectance = 0.5; // 4%
	const float metal = mat.f0;
	
	// WARN: This probably needs to be squared twice like Jessie suggested
    const float alpha = mat.rough * mat.rough;

    // Blend base colors
    const float3 c_diff = lerp(mat.albedo, float3(0, 0, 0), metal) * mat.occlusion;
    const float3 c_spec = 0.16 * reflectance * reflectance * (1 - metal) + mat.albedo * metal;
	
    float3 acc_color = 0;
    const float NdotV = saturate(dot(normalT, eye));

    for (int i = 0; i < NumLights; i++) {
        if (Lights[i].iLightType == 1) {
            // light vector (to light)
            const float3 L = normalize(Lights[i].vLightDir.xyz);

            // Half vector
            const float3 H = normalize(L + eye);

            // products
            const float NdotL = saturate(dot(normalT, L));
            const float LdotH = saturate(dot(L, H));
            const float NdotH = saturate(dot(normalT, H));
        	
            // Diffuse & specular factors
            const float diffuse_factor = Diffuse_Burley(NdotL, NdotV, LdotH, mat.rough);
            const float3 diffuse = c_diff * diffuse_factor;
            const float3 specular = Specular_BRDF(alpha, c_spec, NdotV, NdotL, LdotH, NdotH, normalT, H);
          
            acc_color += NdotL * Lights[i].vLightColor.rgb * (diffuse + specular);
        }
        else if (Lights[i].iLightType == 2) {
            float3 L = (Lights[i].vLightPos - input.wp).xyz; // light dir
            const float dl = length(L); // light distance
            if (Lights[i].vLightAtt.w < dl) continue;

            L = L / dl; // normalized light dir						
            const float3 H = normalize(eye + L); // half direction for specular

        	// products
            const float NdotL = saturate(dot(normalT, L));
            const float LdotH = saturate(dot(L, H));
            const float NdotH = saturate(dot(normalT, H));
        	
            // Diffuse & specular factors
            const float diffuse_factor = Diffuse_Burley(NdotL, NdotV, LdotH, mat.rough);
            const float3 diffuse = c_diff * diffuse_factor;
            const float3 specular = Specular_BRDF(alpha, c_spec, NdotV, NdotL, LdotH, NdotH, normalT, H);
            const float att = 1.0f / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * dl + Lights[i].vLightAtt.z * dl * dl);
            acc_color = mad(att, NdotL * Lights[i].vLightColor.rgb * (diffuse + specular), acc_color);
        }
        else if (Lights[i].iLightType == 3) {
            float3 L = (Lights[i].vLightPos - input.wp).xyz; // light dir
            const float dl = length(L); // light distance
            if (Lights[i].vLightAtt.w < dl) continue;
        	
            L = L / dl; // normalized light dir					
            const float3 H = normalize(eye + L); // half direction for specular
            const float3 sd = normalize((float3) Lights[i].vLightDir); // missuse the vLightDir variable for spot-dir

            const float NdotL = saturate(dot(normalT, L));
            const float LdotH = saturate(dot(L, H));
            const float NdotH = saturate(dot(normalT, H));
        	
            // Diffuse & specular factors
            const float diffuse_factor = Diffuse_Burley(NdotL, NdotV, LdotH, mat.rough);
            const float3 diffuse = c_diff * diffuse_factor;
            const float3 specular = Specular_BRDF(alpha, c_spec, NdotV, NdotL, LdotH, NdotH, normalT, H);

            const float rho = dot(-L, sd);
            const float spot = pow(saturate((rho - Lights[i].vLightSpot.x) / (Lights[i].vLightSpot.y - Lights[i].vLightSpot.x)), Lights[i].vLightSpot.z);
            const float att = spot / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * dl + Lights[i].vLightAtt.z * dl * dl);
            acc_color = mad(att, NdotL * Lights[i].vLightColor.rgb * (diffuse + specular), acc_color);
        }
    }

	const float3 ambient_color = srgb_to_linear(vLightAmbient.rgb);
	float3 specular_env = ambient_color;

	if (bHasCubeMap)
        specular_env = specular_IBL(normalT, eye, mat.rough);

    if (bRenderShadowMap)
        acc_color *= shadow_strength(input.sp);
	
	const float3 lit = acc_color + c_spec * specular_env * mat.occlusion;

	const float3 ambient = ambient_color * mat.occlusion;
	float3 final_color = lit + (ambient + mat.emissive * PI) * mat.albedo;

	//final_color = ACESFilm(final_color);
    final_color = linear_to_srgb(final_color);
	
    return float4(final_color, mat.alpha);
}
