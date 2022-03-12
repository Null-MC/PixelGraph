#include "lib/common_structs.hlsl"
#include "lib/tonemap.hlsl"
#include "lib/sky.hlsl"

#pragma pack_matrix(row_major)


float4 main(const in ps_input_cube input) : SV_TARGET
{
	if (!EnableAtmosphere) return float4(vLightAmbient.rgb, 1.0f);

	const float3 view = normalize(input.tex);
    const float3 col = get_sky_color(view, SunDirection);
	//const float3 min = srgb_to_linear(vLightAmbient.rgb);
    
	//float3 col_final = tonemap_AcesFilm(col * 2.0);
    float3 final_color = min_light + col;
	//final_color = tonemap_ACESFit2(final_color);
	final_color = linear_to_srgb(final_color);

    //col = linear_to_srgb(col); // gamma
    //col += noise(uv * iTime) / 255.0; // dither
    
	return float4(final_color, 1);
}
