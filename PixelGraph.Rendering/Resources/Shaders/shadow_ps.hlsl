#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/parallax.hlsl"

#pragma pack_matrix(row_major)


float main(ps_shadow input) : SV_Depth
{
	const float3 normal = normalize(input.nor);
	//const float3 view = normalize(input.eye);

	float3 shadow_tex = 0;
    float tex_depth = 0;

    //float3 tex_size;
    //tex_normal_height.GetDimensions(0, tex_size.x, tex_size.y, tex_size.z);
    //const float2 tex_aspect = get_parallax_aspect(tex_size.xy);

    const float SNoV = saturate(dot(normal, SunDirection));
	const float2 tex = get_parallax_texcoord(input.tex, input.poT, SNoV, shadow_tex, tex_depth);

	const float alpha = tex_albedo_alpha.SampleLevel(sampler_surface, tex, 0).a;
	clip(alpha - 0.5f);
	
	//const float d = length(float3(input.tex - tex, 1.0f - tex_depth) * float3(1.0f, 1.0f, ParallaxDepth));
    const float pom_depth = 1.0f - tex_depth;// abs((tex - input.tex) / input.poT);
    //const float3 pom_wp = input.wp.xyz + pom_depth * -view * CUBE_SIZE * ParallaxDepth;

	//return input.pos.z;
	return input.pos.z + pom_depth * CUBE_SIZE * ParallaxDepth;
}
