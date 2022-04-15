#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/sky_atmosphere.hlsl"

#pragma pack_matrix(row_major)


float4 main(const in ps_input_cube input) : SV_TARGET
{
	const float3 view = normalize(input.tex);
    const float3 col = min_light + get_sky_color(view, SunDirection);

	return float4(col, 1.0f);
}
