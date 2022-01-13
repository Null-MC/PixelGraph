#define shininess 0.25

#pragma pack_matrix(row_major)


float3 light_surface(const in float3 diffuse, const in float4 wp, const in float3 N, const in float3 V)
{
    float dl, light_att;
    float3 light_color, light_dir, h, sd;
    float3 acc_color = 0;

	const float4x4 mShadowViewProj = vLightView * vLightProjection;
	
	[loop]
    for (int i = 0; i < NumLights; ++i) {
        light_att = 1.0f;

        if (Lights[i].iLightType == 1) { // directional
            light_dir = normalize(Lights[i].vLightDir.xyz);

            if (bHasShadowMap && bRenderShadowMap) {
	            if (dot(light_dir, N) > 0) {
					const float4 sp = mul(wp, mShadowViewProj);
	                light_att = shadow_strength(sp.xyz / sp.w);
	            }
                else {
	                light_att = 0.0f;
                }
			}
        }
        else if (Lights[i].iLightType == 2) { // point
            light_dir = Lights[i].vLightPos.xyz - wp.xyz;
            dl = length(light_dir); // light distance
            light_dir = light_dir / dl; // normalized light dir						
            light_att = rcp(Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * dl + Lights[i].vLightAtt.z * dl * dl);
        }
        else if (Lights[i].iLightType == 3) { // spot
            light_dir = Lights[i].vLightPos.xyz - wp.xyz;
            dl = length(light_dir); // light distance
            //if (Lights[i].vLightAtt.w < dl) continue;
            
            light_dir = light_dir / dl;
            sd = normalize(Lights[i].vLightDir.xyz);

            float rho = dot(-light_dir, sd);
            float spot = pow(saturate((rho - Lights[i].vLightSpot.x) / (Lights[i].vLightSpot.y - Lights[i].vLightSpot.x)), Lights[i].vLightSpot.z);
            light_att = spot / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * dl + Lights[i].vLightAtt.z * dl * dl);
        }

        h = normalize(V + light_dir);
		const float3 f = lit(dot(N, light_dir), dot(N, h), shininess).xyz;

        light_color = srgb_to_linear(Lights[i].vLightColor.rgb);
	    acc_color = mad(light_att, (f.y + f.z) * diffuse * light_color, acc_color);
    }

    return acc_color;
}

float3 get_ambient(const in float3 normal)
{
	return  tex_irradiance.Sample(sampler_irradiance, normal).rgb;
}
