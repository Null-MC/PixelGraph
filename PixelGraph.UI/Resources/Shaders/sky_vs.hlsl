#include "lib/common_structs.hlsl"

#pragma pack_matrix(row_major)


ps_input_cube main(in float3 input : SV_Position)
{
    ps_input_cube output;
	
    // Finally, scale the Mie and Rayleigh colors
	float4x4 viewNoTranslate = mView;
    viewNoTranslate._m30_m31_m32 = 0;
	
	const float4 posV = mul(float4(input, 0), viewNoTranslate);
    output.pos = mul(posV, mProjection);
	
    //output.c0 = v3FrontColor * (v3InvWavelength * fKrESun);
    //output.c1 = v3FrontColor * fKmESun;
    output.tex = input.xyz;
	
	return output;
}
