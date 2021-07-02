#define CUBE_SIZE 4.0f
#define PI 3.14159265f
#define EPSILON 1e-6f
#define Gamma 2.4

#pragma pack_matrix(row_major)


float srgb_to_linear(const float srgb)
{
	//return srgb;
	return pow(abs(srgb), Gamma);
}

float3 srgb_to_linear(const float3 srgb)
{
	//return srgb;
	return pow(abs(srgb), Gamma);
}

float3 linear_to_srgb(const float3 rgb)
{
	//return rgb;
	return pow(abs(rgb), 1.0 / Gamma);
}

float3 calc_tex_normal(const in float2 tex, const in float3 normal, const in float3 tangent, const in float3 bitangent)
{
    float3 tex_normal = tex_normal_height.Sample(sampler_height, tex).xyz;
	tex_normal = mad(2.0f, tex_normal, -1.0f);
	
    return normalize(normal + mad(tex_normal.x, tangent, tex_normal.y * bitangent));
}

float lengthSq(const in float3 vec)
{
	return vec.x*vec.x + vec.y*vec.y + vec.z*vec.z;
}

float lum(const in float3 color)
{
    return dot(color, float3(0.299f, 0.587f, 0.114f));
}

//float shadow_look_up(const in float4 loc, const in float2 offset)
//{
//    return tex_shadow.SampleCmpLevelZero(sampler_shadow, loc.xy + offset, loc.z);
//}
//
//float shadow_strength(float4 sp)
//{
//	sp = sp / sp.w;
//	const float2 xy = abs(sp).xy - float2(1, 1);
//
//	if (xy.x > 0 || xy.y > 0 || sp.z < 0 || sp.z > 1) return 1;
//
//	sp.x = mad(0.5, sp.x, 0.5f);
//	sp.y = mad(-0.5, sp.y, 0.5f);
//
//	sp.z -= vShadowMapInfo.z;
//
//	float sum = 0;
//	float x = 0;
//	const float range = 1.5;
//	const float2 scale = 1 / vShadowMapSize;
//
//	[unroll]
//	for (float y = -range; y <= range; y += 1.0f) {
//		for (x = -range; x <= range; x += 1.0f) {
//			sum += shadow_look_up(sp, float2(x, y) * scale);
//		}
//	}
//
//	const float shadow_factor = sum / 16;
//	return shadow_factor;
//}

float f0_to_ior(const in float f0) {
	const float sqrt_f0 = sqrt(f0);
	return (1.0f + sqrt_f0) / max(1.0f - sqrt_f0, EPSILON);
}

float3 f0_to_ior(const in float3 f0) {
	const float3 sqrt_f0 = sqrt(f0);
	return (1.0f + sqrt_f0) / max(1.0f - sqrt_f0, EPSILON);
}

float ior_to_f0(const in float ior) {
	return pow((ior - 1.0f) / (ior + 1.0f), 2.0f);
}

float3 ior_to_f0(const in float3 ior) {
	return pow((ior - 1.0f) / (ior + 1.0f), 2.0f);
}
