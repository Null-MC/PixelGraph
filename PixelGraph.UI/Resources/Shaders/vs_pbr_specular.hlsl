#include "common_structs.hlsl"
#pragma pack_matrix( row_major )

ps_input main(const vs_input input)
{
	ps_input output;

	output.wp = mul(input.p, mWorld);
    output.pos = mul(output.wp, mViewProjection);
    output.eye = output.wp - float4(vEyePos, 1);
	
	output.nor = normalize(mul(input.n, (float3x3) mWorld));

	output.tex = input.tex;
	output.tan = normalize(mul(input.tan, (float3x3) mWorld));
	output.bin = normalize(mul(input.bin, (float3x3) mWorld));
		
	return output;
}
