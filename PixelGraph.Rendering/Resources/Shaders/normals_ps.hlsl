#define MESH

#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"
#include "lib/parallax.hlsl"
#include "lib/normals.hlsl"
#include "lib/labPbr_material.hlsl"

#pragma pack_matrix(row_major)


float4 main(const ps_input input, const in bool face : SV_IsFrontFace) : SV_TARGET
{
	//float2 tex_size = input.tex_max - input.tex_min;
	//float2 tex_sign = sign(tex_size) * 0.5 + 0.5;
	//return float4(tex_sign, 0, 1);

	const float3 normal = normalize(input.nor);
    float3 tangent = normalize(input.tan);
    float3 bitangent = normalize(input.bin);

	float tex_depth = 0;
    float3 shadow_tex = 0;

	float2 vTS = input.vTS;
	if (!face) {
		vTS = -vTS;
		tangent = -tangent;
		bitangent = -bitangent;
	}

	const float2 tex = get_parallax_texcoord(input.tex, vTS, shadow_tex, tex_depth);

	const pbr_material mat = get_pbr_material(tex);

    if (BlendMode == BLEND_CUTOUT)
		clip(mat.opacity - CUTOUT_THRESHOLD);
    else if (BlendMode == BLEND_TRANSPARENT)
		clip(mat.opacity - EPSILON);

	const float3x3 matTBN = float3x3(tangent, bitangent, normal);
	float3 tex_normal = tex_normal_height.Sample(sampler_height, tex).xyz;
    //tex_normal = normalize(tex_normal * 2.0f - 1.0f);
    tex_normal = decodeNormal(tex_normal);

    if (EnableSlopeNormals && !EnableLinearSampling && tex_depth - shadow_tex.z > 0.002f)
	    tex_normal = apply_slope_normal(tex, vTS, shadow_tex.z);

	if (!face) tex_normal = -tex_normal;

	tex_normal = mul(tex_normal, matTBN);

    return float4(tex_normal * 0.5f + 0.5f, 1.0f);
}
