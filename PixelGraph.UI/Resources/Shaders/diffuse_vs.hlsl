#include "lib/common_structs.hlsl"

#pragma pack_matrix(row_major)


ps_input main(const vs_input input)
{
	ps_input output = (ps_input)0;

	output.tex = input.tex;
	
	output.wp = mul(input.pos, mWorld);
    output.pos = mul(output.wp, mViewProjection);
    output.sp = mul(output.wp, vLightViewProjection);

	const float3 eye = vEyePos - output.wp.xyz;
    output.eye = float4(normalize(eye), 1);
	
	return output;
}
