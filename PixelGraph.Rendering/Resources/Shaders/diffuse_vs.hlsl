#include "lib/common_structs.hlsl"

#pragma pack_matrix(row_major)


ps_input main(const vs_input input)
{
	ps_input output = (ps_input)0;

	output.tex = input.tex;
	output.wp = mul(input.pos, mWorld);
    output.pos = mul(output.wp, mViewProjection);
	output.nor = mul(input.nor, (float3x3) mWorld);
	output.eye = vEyePos - output.wp.xyz;
	
	return output;
}
