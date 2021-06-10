#include "lib/common_structs.hlsl"

#pragma pack_matrix(row_major)


ps_input_cube main(in float3 input : SV_Position)
{
    ps_input_cube output;
    output.tex = input.xyz;
	
	float4x4 viewNoTranslate = mView;
    viewNoTranslate._m30_m31_m32 = 0;
	
	const float4 posV = mul(float4(input, 0), viewNoTranslate);
    output.pos = mul(posV, mProjection);
	
	output.sun_dir = float3(0,0,0);
	output.sun_str = 0;

	// use first directional light as sun
	for (int i = 0; i < NumLights; i++) {
        if (Lights[i].iLightType != 1) continue;
		
        output.sun_dir = Lights[i].vLightDir.xyz;
		output.sun_str = Lights[i].vLightColor.a;
		break;
	}
		
	return output;
}
