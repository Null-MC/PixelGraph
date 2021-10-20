#include "lib/common_structs.hlsl"
#include "lib/sky.hlsl"

#pragma pack_matrix(row_major)


float4 main(const in ps_input_cube input) : SV_TARGET
{
	//float sun;
	//float3 extinction;
	const float3 view = normalize(input.tex);
    //const float3 col = get_sky_color(view, sun, extinction);
    const float3 col = get_sky_color(view, SunDirection);
	//const float3 min = 0.1f; //srgb_to_linear(vLightAmbient.rgb);

	return float4(min_light + col, 1.0f);
}
