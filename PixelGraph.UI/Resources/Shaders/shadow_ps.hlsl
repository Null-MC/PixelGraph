#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/parallax.hlsl"

#pragma pack_matrix(row_major)


float main(ps_shadow input) : SV_Depth
{
	const float3 normal = normalize(input.nor);
	//const float3 view = normalize(input.eye);

	float2 shadow_tex = 0;
    float shadow_depth = 0;
    float tex_depth = 0;

    //float3 tex_size;
    //tex_normal_height.GetDimensions(0, tex_size.x, tex_size.y, tex_size.z);
    //const float2 tex_aspect = get_parallax_aspect(tex_size.xy);

    const float SNoV = saturate(dot(normal, SunDirection));
	const float2 tex = get_parallax_texcoord(input.tex, input.poT, SNoV, shadow_tex, shadow_depth, tex_depth);

	const float alpha = tex_albedo_alpha.SampleLevel(sampler_surface, tex, 0).a;
	clip(alpha - 0.5f);
	
	const float d = length(float3(input.tex - shadow_tex, 1.0f - shadow_depth) * float3(1.0f, 1.0f, ParallaxDepth));

	//return input.pos.z;
	return input.pos.z + d * CUBE_SIZE * 0.01f;
}
