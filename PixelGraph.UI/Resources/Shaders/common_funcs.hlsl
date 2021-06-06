#include "common_structs.hlsl"
#pragma pack_matrix( row_major )


float3 calcNormal(const ps_input input)
{
    float3 normal = normalize(input.nor);
    const float3 tangent = normalize(input.tan);
    const float3 bitangent = normalize(input.bin);

    float3 tex_normal = tex_normal_height.Sample(sampler_surface, input.tex).xyz;

	tex_normal = mad(2.0f, tex_normal, -1.0f);
    normal += mad(tex_normal.x, tangent, tex_normal.y * bitangent);
    return normalize(normal);
}