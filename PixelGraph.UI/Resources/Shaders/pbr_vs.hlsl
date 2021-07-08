#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/parallax.hlsl"

#pragma pack_matrix(row_major)


ps_input main(const vs_input input)
{
	ps_input output;

	output.tex = input.tex;
	output.wp = mul(input.pos, mWorld);
    output.pos = mul(output.wp, mViewProjection);
    output.eye = vEyePos - output.wp.xyz;
	
	const float3 binormal = cross(input.tan, input.nor);
	output.nor = mul(input.nor, (float3x3) mWorld);
	output.tan = mul(input.tan, (float3x3) mWorld);
	output.bin = mul(binormal, (float3x3) mWorld);
	
	const float3x3 mTBN = float3x3(output.tan, output.bin, output.nor);
	output.poT = get_parallax_offset(mTBN, normalize(output.eye.xyz));

	//output.p2 = clip_to_screen(output.pos);

	return output;
}
