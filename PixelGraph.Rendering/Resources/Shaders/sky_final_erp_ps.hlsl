#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/tonemap.hlsl"

#define lumacoeff float3(0.2125, 0.7154, 0.0721)


float4 main(const in ps_input_cube input) : SV_TARGET
{
	const float3 view = normalize(input.tex);
	const float2 tex = getErpCoord(view);
	float3 final_color = tex_equirectangular.SampleLevel(sampler_environment, tex, 0);

	final_color = pow(abs(final_color), 1.f + 2.5f * ErpExposure);
	final_color *= 16.f;

	//const float srcLum = dot(final_color, lumacoeff);
	//final_color = final_color * (1.f + srcLum * 2.f);
	//final_color = tonemap_ACESFit2(final_color);

	final_color = tonemap_ACESFit2(final_color);

	return float4(final_color, 1);
}
