#define PI 3.14159265f
#define EPSILON 1e-6f
#define Gamma 1.4
//#define Gamma 2.233333333

#pragma pack_matrix(row_major)


float3 srgb_to_linear(const float3 srgb)
{
	//return srgb;
	//return srgb * (srgb * (srgb * 0.305306011 + 0.682171111) + 0.012522878);
	return pow(abs(srgb), Gamma);
}

float3 linear_to_srgb(const float3 rgb)
{
	//return rgb;
	//const float3 s1 = sqrt(rgb);
	//const float3 s2 = sqrt(s1);
	//const float3 s3 = sqrt(s2);
	//return 0.662002687 * s1 + 0.684122060 * s2 - 0.323583601 * s3 - 0.0225411470 * rgb;
	
	//return max(1.055 * pow(abs(rgb), 0.416666667) - 0.055, 0);
	return pow(abs(rgb), 1 / Gamma);
	
	//return pow(rgb, float3(Gamma, Gamma, Gamma));
}

float3 ACESFilm(float3 x)
{
    float tA = 2.51;
    float tB = 0.03;
    float tC = 2.43;
    float tD = 0.59;
    float tE = 0.14;
	
    return saturate((x * (tA * x + tB)) / (x * (tC * x + tD) + tE));
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
	return shadow_factor;
	
	// now, put the shadow-strength into the 0-nonTeil range	
	const float non_teil = 1 - vShadowMapInfo.x;
	return vShadowMapInfo.x + shadow_factor * non_teil;
}
