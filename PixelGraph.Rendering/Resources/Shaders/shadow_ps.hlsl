#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/parallax.hlsl"

#pragma pack_matrix(row_major)


void main(ps_shadow input)
{
	float2 tex = input.tex;

	if (bHasDisplacementMap) {
		const float3 normal = normalize(input.nor);
	    const float surface_NoV = saturate(dot(normal, SunDirection));
		float3 shadow_tex = 0;
	    float tex_depth = 0;

		tex = get_parallax_texcoord(input.tex, input.poT, surface_NoV, shadow_tex, tex_depth);
	}

	const float alpha = tex_albedo_alpha.SampleLevel(sampler_surface, tex, 0).a;

    if (BlendMode == BLEND_CUTOUT)
		clip(alpha - CUTOUT_THRESHOLD);
    else if (BlendMode == BLEND_TRANSPARENT)
		clip(alpha - EPSILON);
}
