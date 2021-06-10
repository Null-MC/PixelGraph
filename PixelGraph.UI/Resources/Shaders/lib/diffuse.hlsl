#define shininess 0.25

#pragma pack_matrix(row_major)


float3 blinnPhong_lighting(const float4 light_color, const float3 normal, const float3 diffuse, const float3 L, const float3 H)
{
	const float3 f = lit(dot(normal, L), dot(normal, H), shininess).xyz;
    return diffuse * f.y * light_color.rgb;
}

float3 light_surface(const float4 wp, const in float3 V, const in float3 N, const float3 diffuse)
{
    float3 acc_color = 0;
	
    for (int i = 0; i < NumLights; ++i) {
        if (Lights[i].iLightType == 1) {
        	// directional
            float3 d = normalize(Lights[i].vLightDir.xyz); // light dir	
            float3 h = normalize(V + d);
            acc_color += blinnPhong_lighting(Lights[i].vLightColor, N, diffuse, d, h);
        }
        else if (Lights[i].iLightType == 2) {
        	// point
            float3 d = Lights[i].vLightPos.xyz - wp.xyz; // light dir
            float dl = length(d); // light distance
            if (Lights[i].vLightAtt.w < dl) continue;
            
            d = d / dl; // normalized light dir						
            float3 h = normalize(V + d); // half direction for specular
            float att = 1.0f / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * dl + Lights[i].vLightAtt.z * dl * dl);
            acc_color = mad(att, blinnPhong_lighting(Lights[i].vLightColor, N, diffuse, d, h), acc_color);
        }
        else if (Lights[i].iLightType == 3) {
        	// spot
            float3 d = Lights[i].vLightPos.xyz - wp.xyz; // light dir
            float dl = length(d); // light distance
            if (Lights[i].vLightAtt.w < dl) continue;
            
            d = d / dl; // normalized light dir					
            float3 h = normalize(V + d); // half direction for specular
            float3 sd = normalize(Lights[i].vLightDir.xyz); // missuse the vLightDir variable for spot-dir

            float rho = dot(-d, sd);
            float spot = pow(saturate((rho - Lights[i].vLightSpot.x) / (Lights[i].vLightSpot.y - Lights[i].vLightSpot.x)), Lights[i].vLightSpot.z);
            float att = spot / (Lights[i].vLightAtt.x + Lights[i].vLightAtt.y * dl + Lights[i].vLightAtt.z * dl * dl);
            acc_color = mad(att, blinnPhong_lighting(Lights[i].vLightColor, N, diffuse, d, h), acc_color);
        }
    }
		
    return saturate(acc_color);
}
