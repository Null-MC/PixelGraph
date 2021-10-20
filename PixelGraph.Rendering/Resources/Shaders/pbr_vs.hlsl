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
	
	const float3 binormal = cross(input.tan, input.nor);
	output.nor = mul(input.nor, (float3x3) mWorld);
	output.tan = mul(input.tan, (float3x3) mWorld);
	output.bin = mul(binormal, (float3x3) mWorld);

	output.tex_min = input.tex_min;
	output.tex_max = input.tex_max;

	//const float2 aspect = get_parallax_aspect(input.tex_max - input.tex_min);

	const float3x3 mTBN = float3x3(output.tan, output.bin, output.nor);
    const float3 lightT = mul(mTBN, normalize(output.eye.xyz));
	output.poT = get_parallax_offset(lightT, input.tex_max - input.tex_min);// * aspect;

	//float4 result = float4(0, 0, 0, 1);
    //if (input.tex_max.x < input.tex_min.x) output.poT.y *= -1;
    //if (input.tex_max.y < input.tex_min.y) output.poT.y *= -1;
    //return result;


	//output.p2 = clip_to_screen(output.pos);

	return output;
}
