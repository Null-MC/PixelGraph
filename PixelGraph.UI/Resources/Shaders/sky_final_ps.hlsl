#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/tonemap.hlsl"
#include "lib/sky.hlsl"

#pragma pack_matrix(row_major)


float4 main(const in ps_input_cube input) : SV_TARGET
{
	const float3 view = normalize(input.tex);

	float sun;
	float3 extinction;
    float3 col = get_sky_color(view, sun, extinction);
	col += get_sun_color(sun, extinction);
	    
	//col = ACESFilm(col);
    col = tonemap_HejlBurgess(col);
    
	return float4(col, 1);
}
