#include "lib/common_structs.hlsl"

#pragma pack_matrix(row_major)


ps_input main(const vs_input input)
{
	ps_input output;

	output.tex = input.tex;
	
	output.wp = mul(input.pos, mWorld);
    output.pos = mul(output.wp, mViewProjection);
    output.eye = vEyePos - output.wp.xyz;
    output.sp = mul(output.wp, vLightViewProjection);

	const float3 binormal = cross(input.tan, input.nor);
	output.nor = normalize(mul(input.nor, (float3x3) mWorld));
	output.tan = normalize(mul(input.tan, (float3x3) mWorld));
	output.bin = normalize(mul(binormal, (float3x3) mWorld));

	const float3x3 mWorldToTangent = float3x3(output.tan, output.bin, output.nor);
	output.eyeT = mul(mWorldToTangent, output.eye);

	const float length_sq = dot(output.eyeT, output.eyeT);
	const float parallax_length = sqrt(length_sq - output.eyeT.z * output.eyeT.z) / output.eyeT.z;
	const float2 parallax_dir = normalize(output.eyeT.xy);

	output.poT = parallax_dir * parallax_length * ParallaxDepth;

	return output;
}
