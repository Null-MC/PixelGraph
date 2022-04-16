#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/sky_atmosphere.hlsl"
#include "lib/tonemap.hlsl"


float4 main(const in ps_input_cube input) : SV_TARGET
{
	const float3 view = normalize(input.tex);

	float3 final_color = min_light + get_sky_color(view, SunDirection);

	//final_color = tonemap_HejlBurgess(final_color);
	final_color = tonemap_ACESFit2(final_color);
	//final_color = linear_to_srgb(final_color);
    
	return float4(final_color, 1);
}
