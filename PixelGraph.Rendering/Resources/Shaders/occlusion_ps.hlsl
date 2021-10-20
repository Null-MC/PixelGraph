#include "lib/occlusion_structs.hlsl"
#define EPSILON 1e-6f

#pragma pack_matrix(row_major)


static const float vRayCountFactor = 1.0f / (vRayCount + 1);

bool ray_test(float3 position, const in float3 ray, out float factor)
{
    float heightValue, hit;
    for (uint step = 1; step <= vStepCount; step++) {
        position += ray;

        if (position.z >= vZScale) break;

        heightValue = 1.0f - tex_height.SampleLevel(sampler_height, position.xy, 0);
        heightValue *= vZScale;

        if (position.z - heightValue > -EPSILON) continue;

        // hit, return 
        hit = (float)step / vStepCount;
        factor = 1.0f - pow(hit, vHitPower);
        return true;
    }

    factor = 0.0f;
    return false;
}

float main(const in ps_input input) : SV_TARGET
{
    float heightValue = 1.0f - tex_height.SampleLevel(sampler_height, input.tex, 0);

    heightValue *= vZScale;

    heightValue += vZBias / 100.0f * vZScale;

    float3 position;
    float rayHitFactor;
    float hitFactor = 0.0f;
    for (uint i = 0; i < vRayCount; i++) {
        position.xy = input.tex.xy;
        position.z = heightValue;

        if (ray_test(position, vRayList[i], rayHitFactor))
            hitFactor += rayHitFactor * vRayCountFactor;
    }

    return saturate(1.0f - hitFactor);
}

//void create_rays(out float3 rays[vRayCount])
//{
//	const uint hStepCount = 4 + (int)(vQuality * 356.0f);
//	const uint vStepCount = 1 + (int)(vQuality * 88.0f);
//
//	const float hStepSize = 360.0f / hStepCount;
//	const float vStepSize = 90.0f / vStepCount;
//
//	//const uint count = hStepCount * vStepCount;
//
//    uint v, h, z;
//    float hAngleDegrees, vAngleDegrees;
//    for (v = 0; v < vStepCount; v++) {
//        for (h = 0; h < hStepCount; h++) {
//	        hAngleDegrees = h * hStepSize - 180.0f;
//            vAngleDegrees = v * vStepSize;
//
//	        z = hStepCount * v + h;
//            rays[z].X = cos(hAngleDegrees);
//            rays[z].Y = sin(hAngleDegrees);
//            rays[z].Z = sin(vAngleDegrees);
//        }
//    }
//}
