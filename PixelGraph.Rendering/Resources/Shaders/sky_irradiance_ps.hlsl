#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"

#pragma pack_matrix(row_major)

static const float sample_delta = 0.025f;


float4 main(const in ps_input_cube input) : SV_TARGET
{
	const float3 view = normalize(input.tex);
	const float3 right = normalize(cross(up, view));
	const float3 up = normalize(cross(view, right));

	float3 irradiance = 0.0;
	float sample_count = 0.0;
	
	for (float phi = 0.0f; phi < 2.0f * PI; phi += sample_delta) {
		for (float theta = 0.0f; theta < 0.5f * PI; theta += sample_delta) {
			const float cos_theta = cos(theta);
			const float sin_theta = sin(theta);
			
			// spherical to cartesian (in tangent space)
			const float3 sampleT = float3(sin_theta * cos(phi),  sin_theta * sin(phi), cos_theta);
			
			// tangent space to world
			const float3 sampleW = sampleT.x * right + sampleT.y * up + sampleT.z * view;

			const float3 col = tex_environment.SampleLevel(sampler_environment, sampleW, 4);

			irradiance += col * cos_theta * sin_theta;
			sample_count++;
		}
	}
	
	irradiance = PI * irradiance / float(sample_count);
	return float4(irradiance, 1.0f);
}
