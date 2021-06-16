#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/parallax.hlsl"
#include "lib/pbr_material.hlsl"
#include "lib/pbr_filament.hlsl"

#pragma pack_matrix(row_major)


float4 main(const ps_input input) : SV_TARGET
{
	const float3 normal = normalize(input.nor);
    const float3 tangent = normalize(input.tan);
    const float3 bitangent = normalize(input.bin);
	const float3 view = normalize(input.eye.xyz);

	const float2 dx = ddx(input.tex);
	const float2 dy = ddy(input.tex);
	
	float depth_offset;
	const float2 tex = get_parallax_texcoord(input.tex, dx, dy, normal, input.poT, view, depth_offset);
	const float3 tex_normal = calc_tex_normal(tex, normal, tangent, bitangent);
	const pbr_material mat = get_pbr_material(tex);
	
	clip(mat.alpha - EPSILON);
	
	const float reflectance = 0.5; // 4%
	const float metal = mat.f0;
	
    const float alpha = mat.rough * mat.rough;

    // Blend base colors
    const float3 c_diff = lerp(mat.albedo, float3(0, 0, 0), metal);// * mat.occlusion;
    const float3 c_spec = 0.16 * reflectance * reflectance * (1.0 - metal) + mat.albedo * metal;
	
    float3 acc_color = 0;
    const float NdotV = saturate(dot(tex_normal, view));
    const float3x3 mTBN = float3x3(tangent, bitangent, normal);

    for (int i = 0; i < NumLights; i++) {
        if (Lights[i].iLightType == 1) {
            // light vector (to light)
            const float3 light_dir = normalize(Lights[i].vLightDir.xyz);

        	// light parallax shadows
        	const float2 polT = get_parallax_offset(mTBN, light_dir);
            const float shadow = get_parallax_shadow(tex, depth_offset, dx, dy, normal, polT, light_dir);
            //if (shadow < EPSILON) continue;

            // Half vector
            const float3 H = normalize(light_dir + view);

            // products
            const float NdotL = saturate(dot(tex_normal, light_dir));
            const float LdotH = saturate(dot(light_dir, H));
            const float NdotH = saturate(dot(tex_normal, H));
        	
            // Diffuse & specular factors
            const float diffuse_factor = Diffuse_Burley(1, NdotL, NdotV, LdotH, mat.rough);
            const float3 diffuse = c_diff * diffuse_factor;
            const float3 specular = Specular_BRDF(alpha, c_spec, NdotV, NdotL, LdotH, NdotH, tex_normal, H);
          
            acc_color += shadow * NdotL * Lights[i].vLightColor.rgb * (diffuse + specular);
        }
        else if (Lights[i].iLightType == 2) {
            float3 light_dir = (Lights[i].vLightPos - input.wp).xyz; // light dir
            const float dl = length(light_dir); // light distance
            if (Lights[i].vLightAtt.w < dl) continue;

            light_dir = light_dir / dl; // normalized light dir

        	// light parallax shadows
        	const float2 polT = get_parallax_offset(mTBN, light_dir);
            const float shadow = get_parallax_shadow(tex, depth_offset, dx, dy, normal, polT, light_dir);
            //if (shadow < EPSILON) continue;

        	const float3 H = normalize(view + light_dir); // half direction for specular

        	// products
            const float NdotL = saturate(dot(tex_normal, light_dir));
            const float LdotH = saturate(dot(light_dir, H));
            const float NdotH = saturate(dot(tex_normal, H));
        	
            // Diffuse & specular factors
            const float diffuse_factor = Diffuse_Burley(NdotL, NdotV);
            const float3 diffuse = c_diff * diffuse_factor;
            const float3 specular = Specular_BRDF(alpha, c_spec, NdotV, NdotL, LdotH, NdotH, tex_normal, H);
            const float att = 1.0f / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * dl + Lights[i].vLightAtt.z * dl * dl);
            acc_color = mad(att * shadow, NdotL * Lights[i].vLightColor.rgb * (diffuse + specular), acc_color);
        }
        else if (Lights[i].iLightType == 3) {
            float3 light_dir = (Lights[i].vLightPos - input.wp).xyz; // light dir
            const float dl = length(light_dir); // light distance
            if (Lights[i].vLightAtt.w < dl) continue;
        	
            light_dir = light_dir / dl; // normalized light dir

        	// light parallax shadows
        	const float2 polT = get_parallax_offset(mTBN, light_dir);
            const float shadow = get_parallax_shadow(tex, depth_offset, dx, dy, normal, polT, light_dir);
            //if (shadow < EPSILON) continue;
        	
            const float3 H = normalize(view + light_dir); // half direction for specular
            const float3 sd = normalize((float3) Lights[i].vLightDir); // missuse the vLightDir variable for spot-dir

            const float NdotL = saturate(dot(tex_normal, light_dir));
            const float LdotH = saturate(dot(light_dir, H));
            const float NdotH = saturate(dot(tex_normal, H));
        	
            // Diffuse & specular factors
            const float diffuse_factor = Diffuse_Burley(NdotL, NdotV);
            const float3 diffuse = c_diff * diffuse_factor;
            const float3 specular = Specular_BRDF(alpha, c_spec, NdotV, NdotL, LdotH, NdotH, tex_normal, H);

            const float rho = dot(-light_dir, sd);
            const float spot = pow(saturate((rho - Lights[i].vLightSpot.x) / (Lights[i].vLightSpot.y - Lights[i].vLightSpot.x)), Lights[i].vLightSpot.z);
            const float att = spot / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * dl + Lights[i].vLightAtt.z * dl * dl);
            acc_color = mad(att * shadow, NdotL * Lights[i].vLightColor.rgb * (diffuse + specular), acc_color);
        }
    }

	const float3 ambient_color = srgb_to_linear(vLightAmbient.rgb);
	float3 specular_env = ambient_color;

	if (bHasCubeMap) {
        specular_env = specular_IBL(tex_normal, view, mat.rough);
	}

    //if (bRenderShadowMap)
    //    acc_color *= shadow_strength(input.sp);
	
	const float3 lit = acc_color + c_spec * specular_env * mat.occlusion;

	const float3 ambient = ambient_color * mat.occlusion;
	float3 final_color = lit + (ambient + mat.emissive * PI) * mat.albedo;

	//final_color = ACESFilm(final_color);
    final_color = linear_to_srgb(final_color);
	
    return float4(final_color, mat.alpha);
}
