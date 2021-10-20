#define MAX_RAYS 300

#pragma pack_matrix(row_major)


Texture2D<float> tex_height : register(t0);
SamplerState sampler_height : register(s0);

cbuffer cbTransforms : register(b0)
{
    float4x4 mViewProjection;
    float4 vViewport;

	float vQuality;
	uint vStepCount;
	float vHitPower = 1.5f;
	float vZScale;
	float vZBias;

    uint vRayCount;
    float3 vRayList[MAX_RAYS];
};

struct vs_input
{
	float4 pos : POSITION0;
	float2 tex  : TEXCOORD0;
};

struct ps_input
{
	float2 tex  : TEXCOORD0;
};
