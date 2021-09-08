#include "lib/occlusion_structs.hlsl"

#pragma pack_matrix(row_major)


ps_input main(const vs_input input)
{
	ps_input output;

	output.tex = input.tex;

    return output;
}
