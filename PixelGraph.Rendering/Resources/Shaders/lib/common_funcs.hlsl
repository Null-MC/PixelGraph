#define BLOCK_SIZE 4.0f
#define PI 3.14159265f
#define PI2 6.2831853f
#define EPSILON 1e-6f
#define GAMMA 2.4

#define BLEND_SOLID 0
#define BLEND_CUTOUT 1
#define BLEND_TRANSPARENT 2
#define CUTOUT_THRESHOLD 0.1f

#define IOR_N_AIR 1.0f
#define IOR_N_WATER 1.333f

#pragma pack_matrix(row_major)

static const float3 up = float3(0.0f, 1.0f, 0.0f);
static const float3 tan_up = float3(0.0f, 0.0f, 1.0f);

static const float ETA_AIR_TO_WATER = IOR_N_WATER / IOR_N_AIR;
static const float ETA_WATER_TO_AIR = IOR_N_AIR / IOR_N_WATER;

static const float InvPI = 1.0f / PI;
static const float InvGamma = 1.0f / GAMMA;
//static const float3 lum_factor = float3(0.299f, 0.587f, 0.114f);


float pow2(const in float x) {return x*x;}
float3 pow2(const in float3 x) {return x*x;}
float pow4(const in float x) {return x*x*x*x;}

float4 clip_to_screen(const in float4 pos)
{
	float4 o = pos * 0.5f;
	o.xy = float2(o.x, -o.y) + o.w;
	o.zw = pos.zw;
	return o;
}

float srgb_to_linear(const float srgb) {
	//return pow(abs(srgb), GAMMA);
    if (step(srgb, 0.04045f) == 1.0f) return srgb / 12.92f;
    return pow((abs(srgb) + 0.055f) / 1.055f, 2.4f);
}

float2 srgb_to_linear(const float2 srgb) {
	//return pow(abs(srgb), GAMMA);
    const float2 shit = step(srgb, 0.04045f);
    if (all(shit == 1.0f)) return srgb / 12.92f;
    return pow((srgb + 0.055f) / 1.055f, 2.4f);
}

float3 srgb_to_linear(const float3 srgb) {
	//return pow(abs(srgb), GAMMA);
	const float3 shit = step(srgb, 0.04045f);
    if (all(shit == 1.0f)) return srgb / 12.92f;
    return pow((abs(srgb) + 0.055f) / 1.055f, 2.4f);
}

float3 linear_to_srgb(const float3 v) {
	//return pow(abs(rgb), InvGamma);
	const float3 shit = step(v, 0.0031308f);
    if (all(shit == 1.0f)) return v * 12.92f;
    return pow(abs(v), 1.0f / 2.4f) * 1.055f - 0.055f;
}

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

float3 ior_to_f0_complex(const in float3 ior_n, const in float3 ior_k) {
	const float3 k2 = ior_k * ior_k;
	return (pow2(ior_n - 1.0) + k2) / (pow2(ior_n + 1.0) + k2);
}

float3 ior_to_f0_complex(const in float3 ior_n_from, const in float3 ior_n_to, const in float3 ior_k) {
	const float3 k2 = ior_k * ior_k;
	return (pow2(ior_n_to - ior_n_from) + k2) / (pow2(ior_n_to + ior_n_from) + k2);
}

float lengthSq(const in float2 vec)
{
	return dot(vec, vec); //vec.x*vec.x + vec.y*vec.y;
}

float lengthSq(const in float3 vec)
{
	return dot(vec, vec); // vec.x*vec.x + vec.y*vec.y + vec.z*vec.z;
}

//float lum(const in float3 color)
//{
//    return dot(color, lum_factor);
//}


// Shadows

float shadow_lookup(const in float3 loc, const in float2 offset)
{
    return tex_shadow.SampleCmpLevelZero(sampler_shadow, loc.xy + offset, loc.z);
}

float shadow_strength(in float3 sp)
{
    float2 xy = abs(sp).xy - 1.0f;
    if (xy.x > 0 || xy.y > 0 || sp.z < 0 || sp.z > 1) return 1.0f;

    sp.x = mad(0.5f, sp.x, 0.5f);
    sp.y = mad(-0.5f, sp.y, 0.5f);

    //apply shadow map bias
    sp.z -= vShadowMapInfo.z;

    //// --- not in shadow, hard cut
    //float shadowMapDepth = texShadowMap.Sample(PointSampler, sp.xy+offsets[1]).r;
    //return whengt(shadowMapDepth, sp.z);

    //// --- basic hardware PCF - single texel
    //float shadowFactor = texShadowMap.SampleCmpLevelZero(samplerShadow, sp.xy, sp.z).r;

    //// --- PCF sampling for shadow map
    float sum = 0;
    const float range = 1.5;
    const float2 scale = rcp(vShadowMapSize);
    float x, y;

    //// ---perform PCF filtering on a 4 x 4 texel neighborhood
    [unroll]
    for (y = -range; y <= range; y += 1.0f) {
		[unroll]
	    for (x = -range; x <= range; x += 1.0f) {
		    sum += shadow_lookup(sp, float2(x, y) * scale);
	    }
    }

	return saturate(sum * 0.0625f);

    // now, put the shadow-strength into the 0-nonTeil range
    //return vShadowMapInfo.x + shadow_factor * (1.0f - vShadowMapInfo.x);
}


// Sky

float2 getErpCoord(const in float3 dir)
{
	return float2(atan2(dir.z, dir.x) / PI2 + 0.5f, acos(dir.y) / PI);
}
