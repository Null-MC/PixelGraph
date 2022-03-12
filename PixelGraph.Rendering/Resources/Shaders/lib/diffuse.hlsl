#define shininess 0.25

#pragma pack_matrix(row_major)


float3 calcBlinnPhongLighting(const in float3 LColor, const in float3 N, const in float3 diffuse, const in float3 L, const in float3 H, const in float3 specular)
{
    //float4 Id = vMaterialTexture * diffuse * saturate(dot(N, L));
    //float4 Is = vMaterialSpecular * pow(saturate(dot(N, H)), sMaterialShininess);
    float4 f = lit(dot(N, L), dot(N, H), shininess);
    return (f.y * diffuse + f.z * specular) * LColor;
}

float3 light_surface(const in float3 diffuse, const in float4 wp, const in float3 N, const in float3 V)
{
    float3 acc_color = 0;
	float3 light_dir;

	const float4x4 mShadowViewProj = mul(vLightView, vLightProjection);
	
	[loop]
    for (int i = 0; i < NumLights; ++i) {
        float light_att = 1.0f;

        if (Lights[i].iLightType == 1) { // directional
            light_dir = normalize(Lights[i].vLightDir.xyz);
        }
        else {
	        light_dir = Lights[i].vLightPos.xyz - wp.xyz;
	        const float light_dist = length(light_dir); // light distance
			//if (Lights[i].vLightAtt.w < light_dist) continue;

	        light_dir /= light_dist; // normalized light dir

		    if (Lights[i].iLightType == 2) { // point
		        light_att = 1.0f / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * light_dist + Lights[i].vLightAtt.z * light_dist * light_dist);
		    }
		    else if (Lights[i].iLightType == 3) { // spot
			    const float3 sd = normalize(Lights[i].vLightDir.xyz);

		        const float rho = dot(-light_dir, sd);
		        const float spot = pow(saturate((rho - Lights[i].vLightSpot.x) / (Lights[i].vLightSpot.y - Lights[i].vLightSpot.x)), Lights[i].vLightSpot.z);
		        light_att = spot / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * light_dist + Lights[i].vLightAtt.z * light_dist * light_dist);
		    }
        }

		if ((Lights[i].iLightType == 1 && EnableAtmosphere) || (Lights[i].iLightType == 3 && !EnableAtmosphere)) {
            if (dot(light_dir, N) > 0) {
				const float4 sp = mul(wp, mShadowViewProj);
                light_att *= shadow_strength(sp.xyz / sp.w);
            }
            else {
                light_att = 0.0f;
            }
		}

        const float3 h = normalize(V + light_dir);
		//const float3 f = lit(dot(N, light_dir), dot(N, h), shininess).xyz;
        const float3 light_color = srgb_to_linear(Lights[i].vLightColor.rgb);

		//const float3 light = (f.y + f.z) * diffuse * light_color;
		const float3 light = calcBlinnPhongLighting(light_color, N, diffuse, light_dir, h, 0.0f);

	    acc_color = mad(light_att, light, acc_color);
    }

    return acc_color;
}

float3 get_ambient(const in float3 normal)
{
    return EnableAtmosphere
		? tex_irradiance.SampleLevel(sampler_irradiance, normal, 0)
		: srgb_to_linear(vLightAmbient.rgb);
}
