#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/sky_atmosphere.hlsl"
#include "lib/tonemap.hlsl"


float4 main(const in ps_input_cube input) : SV_TARGET
{
	const float3 view = normalize(input.tex);

	float3 final_color = min_light + get_sky_color(view, SunDirection);
    final_color = apply_tonemap(final_color);
	return float4(final_color, 1);
}
