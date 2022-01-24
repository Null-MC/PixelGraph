#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/parallax.hlsl"

#pragma pack_matrix(row_major)


ps_input main(const vs_input_ex input)
{
	ps_input output;

	output.tex = input.tex;
	output.wp = mul(input.pos, mWorld);
    output.pos = mul(output.wp, mViewProjection);
    output.eye = vEyePos - output.wp.xyz;
	
	output.nor = mul(input.nor, (float3x3) mWorld);
	output.tan = mul(input.tan, (float3x3) mWorld);
	output.bin = mul(input.bin, (float3x3) mWorld);

	output.tex_min = input.tex_min;
	output.tex_max = input.tex_max;

	const float3x3 mTBN = float3x3(output.tan, output.bin, output.nor);
    output.vT = mul(mTBN, normalize(output.eye.xyz));
	output.vTS = get_parallax_offset(output.vT, input.tex_max - input.tex_min);

	return output;
}
