#include "lib/common_structs.hlsl"
#include "lib/tonemap.hlsl"
#include "lib/sky.hlsl"

#pragma pack_matrix(row_major)


float4 main(const in ps_input_cube input) : SV_TARGET
{
	const float3 view = normalize(input.tex);
    const float3 col = get_sky_color(view, SunDirection);
	//const float3 min = srgb_to_linear(vLightAmbient.rgb);
    
	//float3 col_final = tonemap_AcesFilm(col * 2.0);
    const float3 col_final = tonemap_HejlBurgess(min_light + col);
    //col = linear_to_srgb(col); // gamma
    //col += noise(uv * iTime) / 255.0; // dither
    
	return float4(col_final, 1);
}
