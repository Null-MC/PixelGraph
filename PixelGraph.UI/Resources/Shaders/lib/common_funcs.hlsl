#pragma pack_matrix(row_major)


float3 calc_normal(const in float2 tex, const in float3 normal, const in float3 tangent, const in float3 bitangent)
{
    float3 tex_normal = tex_normal_height.Sample(sampler_surface, tex).xyz;
	tex_normal = mad(2.0f, tex_normal, -1.0f);
	
    return normalize(normal + mad(tex_normal.x, tangent, tex_normal.y * bitangent));
}

float shadow_look_up(const in float4 loc, const in float2 offset)
{
    return tex_shadow.SampleCmpLevelZero(sampler_shadow, loc.xy + offset, loc.z);
}

float shadow_strength(float4 sp)
{
	sp = sp / sp.w;
	const float2 xy = abs(sp).xy - float2(1, 1);

	if (xy.x > 0 || xy.y > 0 || sp.z < 0 || sp.z > 1) return 1;

	sp.x = mad(0.5, sp.x, 0.5f);
	sp.y = mad(-0.5, sp.y, 0.5f);

	//apply shadow map bias
	sp.z -= vShadowMapInfo.z;

	//// --- PCF sampling for shadow map
	float sum = 0;
	float x = 0;
	const float range = 1.5;
	const float2 scale = 1 / vShadowMapSize;

	//// ---perform PCF filtering on a 4 x 4 texel neighborhood
	[unroll]
	for (float y = -range; y <= range; y += 1.0f) {
		for (x = -range; x <= range; x += 1.0f) {
			sum += shadow_look_up(sp, float2(x, y) * scale);
		}
	}

	const float shadow_factor = sum / 16;

	// now, put the shadow-strength into the 0-nonTeil range	
	const float non_teil = 1 - vShadowMapInfo.x;
	return vShadowMapInfo.x + shadow_factor * non_teil;
}
