//#define SCREENQUAD
#define sampleCount 64

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

float3 importanceSampleGGX(const in float2 Xi, const in float3 N, const in float a2)
{
    const float phi = 2.0 * PI * Xi.x;
    const float cosTheta = sqrt((1.0 - Xi.y) / (1.0 + (a2 - 1.0) * Xi.y));
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

float GDFG(const in float NoV, const in float NoL, const in float a2) {
    const float GGXL = NoV * sqrt((-NoL * a2 + NoL) * NoL + a2);
    const float GGXV = NoL * sqrt((-NoV * a2 + NoV) * NoV + a2);
    return (2.f * NoL) / (GGXV + GGXL);
}

float2 DFG(const in float NoV, const in float roughL) {
    // ERROR: This can't be right!
	const float3 N = float3(0.f, 0.f, 1.f);

	const float3 V = float3(sqrt(1.0f - NoV*NoV), 0.0f, NoV);
    const float a2 = roughL * roughL;

    float2 r = 0.0f;
    for (uint i = 0; i < sampleCount; i++) {
	    const float2 Xi = hammersleySequence(i, sampleCount);
        const float3 H = importanceSampleGGX(Xi, N, a2);
        const float3 L = 2.f * dot(V, H) * H - V;

        const float VoH = saturate(dot(V, H));
        const float NoL = saturate(L.z);
        const float NoH = saturate(H.z);

        if (NoL > 0.0f) {
            const float G = GDFG(NoV, NoL, a2);
            const float Gv = G * VoH / NoH;
            const float Fc = pow(1.f - VoH, 5.f);

            //r.x += Gv * (1.f - Fc);
            //r.y += Gv * Fc;
            r.x += Gv * Fc;
            r.y += Gv;
        }
    }

    return r / sampleCount;
}

float4 main(MeshOutlinePS_INPUT input) : SV_Target
{
    return float4(DFG(input.Tex.x, input.Tex.y), 0.f, 1.f);
}
