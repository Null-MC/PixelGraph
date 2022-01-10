#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/parallax.hlsl"

#pragma pack_matrix(row_major)


inline float depth_out_to_linear(const in float z)
{
    return rcp(vFrustum.z * z + vFrustum.w);
}

inline float depth_linear_to_out(const in float z)
{
    return (1.0f - vFrustum.w * z) / (vFrustum.z * z);
}

float main(ps_shadow input) : SV_Depth
{
	const float3 normal = normalize(input.nor);

	float3 shadow_tex = 0;
    float tex_depth = 0;

    const float SNoV = saturate(dot(normal, SunDirection));
	const float2 tex = get_parallax_texcoord(input.tex, input.poT, SNoV, shadow_tex, tex_depth);

	const float alpha = tex_albedo_alpha.SampleLevel(sampler_surface, tex, 0).a;

    if (BlendMode == BLEND_CUTOUT)
		clip(alpha - CUTOUT_THRESHOLD);
    else if (BlendMode == BLEND_TRANSPARENT)
		clip(alpha - EPSILON);

	const float pom_depth = (1.0f - tex_depth) / max(SNoV, EPSILON) * CUBE_SIZE * ParallaxDepth;
    const float3 pom_wp = input.wp.xyz - pom_depth * SunDirection;
	const float4x4 mShadowViewProj = mul(vLightView, vLightProjection);
	const float4 sp = mul(float4(pom_wp, 1), mShadowViewProj);
	return sp.z / sp.w;
}
