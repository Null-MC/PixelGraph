#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/parallax.hlsl"

#pragma pack_matrix(row_major)


ps_shadow main(vs_input_ex input, out float4 pos : SV_POSITION)
{
    ps_shadow output;

	output.tex = input.tex;

    const float4 wp = mul(input.pos, mWorld);
	const float4x4 mShadowViewProj = mul(vLightView, vLightProjection);
    pos = mul(wp, mShadowViewProj);

	const float3 binormal = cross(input.tan, input.nor);
	output.nor = mul(input.nor, (float3x3) mWorld);
	float3 tan = mul(input.tan, (float3x3) mWorld);
	float3 bin = mul(binormal, (float3x3) mWorld);

    const float2 uv_size = input.tex_max - input.tex_min;
	const float pDepth = get_parallax_length(uv_size);

	const float3x3 matTBN = float3x3(tan, bin, output.nor);
	const float3 lightT = mul(matTBN, SunDirection);
	output.poT = get_parallax_offset(lightT) * pDepth;
	
    return output;
}
