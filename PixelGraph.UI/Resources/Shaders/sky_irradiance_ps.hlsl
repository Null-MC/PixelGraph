#include "lib/common_structs.hlsl"

#pragma pack_matrix(row_major)


float4 main(const in ps_input_cube input) : SV_TARGET
{
	const float3 n = normalize(input.tex);
	const float3 color = tex_cube.SampleLevel(sampler_cube, n, 0);
	return float4(1.0 - color, 1);
}
