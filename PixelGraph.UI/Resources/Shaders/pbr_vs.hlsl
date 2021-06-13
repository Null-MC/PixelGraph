#include "lib/common_structs.hlsl"
#include "lib/parallax.hlsl"

#pragma pack_matrix(row_major)


ps_input main(const vs_input input)
{
	ps_input output;

	output.tex = input.tex;
	
	output.wp = mul(input.pos, mWorld);
    output.pos = mul(output.wp, mViewProjection);
    output.sp = mul(output.wp, vLightViewProjection);
	
	const float3 eye = vEyePos - output.wp.xyz;
    output.eye = float4(normalize(eye), length(eye));
	
	const float3 binormal = cross(input.tan, input.nor);
	output.nor = normalize(mul(input.nor, (float3x3) mWorld));
	output.tan = normalize(mul(input.tan, (float3x3) mWorld));
	output.bin = normalize(mul(binormal, (float3x3) mWorld));
	
	const float3x3 mTBN = float3x3(output.tan, output.bin, output.nor);
	output.poT = get_parallax_offset(mTBN, output.eye.xyz);

	return output;
}
