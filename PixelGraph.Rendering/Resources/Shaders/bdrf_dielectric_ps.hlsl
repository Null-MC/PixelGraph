//#define SCREENQUAD
#define sampleCount 1024

#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"


float vdcSequence(in uint bits) 
{
    bits = (bits << 16u) | (bits >> 16u);
    bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
    bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
    bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
    bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
    return float(bits) * 2.3283064365386963e-10;
}

float2 hammersleySequence(const in uint i, const in uint N)
{
    return float2(float(i) / float(N), vdcSequence(i));
}

float3 importanceSampleGGX(const in float2 Xi, const in float3 N, const in float rough)
{
    const float a = rough * rough;
    const float phi = 2.0 * PI * Xi.x;
    const float cosTheta = sqrt((1.0 - Xi.y) / (1.0 + (a*a - 1.0) * Xi.y));
    const float sinTheta = sqrt(1.0 - cosTheta * cosTheta);
	
    // from spherical coordinates to cartesian coordinates
    const float3 H = float3(
        cos(phi) * sinTheta,
		sin(phi) * sinTheta,
		cosTheta);
	
    // from tangent-space vector to world-space sample vector
    const float3 up = abs(N.z) < 0.999
		? float3(0.f, 0.f, 1.f)
		: float3(1.f, 0.f, 0.f);

    const float3 tangent   = normalize(cross(up, N));
    const float3 bitangent = cross(N, tangent);
	
    const float3 sampleVec = tangent * H.x + bitangent * H.y + N * H.z;
    return normalize(sampleVec);
}

float V_SmithGGXCorrelated(const in float NoV, const in float NoL, const in float roughness)
{
	const float a2 = pow(roughness, 4.0);
	const float GGXV = NoL * sqrt(NoV * NoV * (1.0 - a2) + a2);
	const float GGXL = NoV * sqrt(NoL * NoL * (1.0 - a2) + a2);
    return 0.5 / (GGXV + GGXL);
}

float2 integrateBRDF(const in float NoV, const in float rough) {
	const float3 V = float3(sqrt(1.0f - NoV*NoV), 0.0f, NoV);

    float2 r = 0.0f;
    for (uint i = 0; i < sampleCount; i++) {
	    const float2 Xi = hammersleySequence(i, sampleCount);
        const float3 H = importanceSampleGGX(Xi, tan_up, rough);
        const float3 L = 2.f * dot(V, H) * H - V;

        const float VoH = saturate(dot(V, H));
        const float NoL = saturate(L.z);
        const float NoH = saturate(H.z);

        if (NoL > 0.0) {
            const float V_pdf = V_SmithGGXCorrelated(NoV, NoL, rough) * VoH * NoL / NoH;
            const float Fc = pow(1.0 - VoH, 5.0);

            r.x += V_pdf * (1.0 - Fc);
            r.y += V_pdf * Fc;
        }
    }

    return r / sampleCount;
}

float4 main(MeshOutlinePS_INPUT input) : SV_Target
{
    return float4(integrateBRDF(input.Tex.x, input.Tex.y), 0.f, 1.f);
}
