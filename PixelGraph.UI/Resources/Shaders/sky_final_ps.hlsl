#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/tonemap.hlsl"
#include "lib/sky2.hlsl"

#pragma pack_matrix(row_major)


float4 main(const in ps_input_cube input) : SV_TARGET
{
	const float3 view = normalize(input.tex);

    float3 col = get_sky_color(view, SunDirection) * 2.0f;


	//col = tonemap_AcesFilm(col * 2.0);
    //col = linear_to_srgb(col); // gamma
    col = tonemap_HejlBurgess(col);
    //col += noise(uv * iTime) / 255.0; // dither
    
	return float4(col, 1);
}
