#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/parallax.hlsl"

#pragma pack_matrix(row_major)


ps_shadow main(vs_input input)
{
    ps_shadow output;

	output.tex = input.tex;

    const float4 wp = mul(input.pos, mWorld);
    output.pos = mul(wp, vLightViewProjection);

    //float3 eye = vEyePos - wp.xyz;
	
	const float3 binormal = cross(input.tan, input.nor);
	output.nor = mul(input.nor, (float3x3) mWorld);
	float3 tan = mul(input.tan, (float3x3) mWorld);
	float3 bin = mul(binormal, (float3x3) mWorld);
	
	const float3x3 mTBN = float3x3(tan, bin, output.nor);
	//const float2 aspect = get_parallax_aspect(abs(input.tex_max - input.tex_min));
	output.poT = get_parallax_offset(mTBN, SunDirection);// * aspect;
	
    return output;
}
