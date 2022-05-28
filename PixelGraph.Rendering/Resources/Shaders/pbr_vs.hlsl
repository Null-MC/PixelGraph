#define MESH

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

	output.tex_min = input.tex_min;
	output.tex_max = input.tex_max;

	float3 nor =  normalize(input.nor);
	float3 tan =  normalize(input.tan);
	float3 bin = -normalize(input.bin);

	float2 uv_size = input.tex_max - input.tex_min;
	output.pDepth = get_parallax_length(uv_size);

	//if ((uv_size.x < 0.0f && uv_size.y > 0.0f) || (uv_size.x > 0.0f && uv_size.y < 0.0f)) bin = -bin;
	//if (sign(uv_size.x) != sign(uv_size.y)) bin = -bin;
	bin *= sign(uv_size.x * uv_size.y);

	output.tan = mul(tan, (float3x3) mWorld);
	output.bin = mul(bin, (float3x3) mWorld);
	output.nor = mul(nor, (float3x3) mWorld);

	const float3x3 mTBN = float3x3(output.tan, output.bin, output.nor);
    output.vT = mul(mTBN, normalize(output.eye.xyz));
	output.vTS = get_parallax_offset(output.vT) * output.pDepth;

	return output;
}
