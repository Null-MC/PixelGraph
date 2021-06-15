#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"

#pragma pack_matrix(row_major)

static const float3 up = float3(0.0, 1.0, 0.0);
static const float sample_delta = 0.025;


float4 main(const in ps_input_cube input) : SV_TARGET
{
	const float3 view = normalize(input.tex);
	const float3 right = normalize(cross(up, view));
	const float3 up = normalize(cross(view, right));

	float3 irradiance = 0.0;
	float sample_count = 0.0;
	
	for (float phi = 0.0; phi < 2.0 * PI; phi += sample_delta) {
		for (float theta = 0.0; theta < 0.5 * PI; theta += sample_delta) {
			const float cos_theta = cos(theta);
			const float sin_theta = sin(theta);
			
			// spherical to cartesian (in tangent space)
			const float3 sampleT = float3(sin_theta * cos(phi),  sin_theta * sin(phi), cos_theta);
			
			// tangent space to world
			const float3 sampleW = sampleT.x * right + sampleT.y * up + sampleT.z * view; 

			irradiance += tex_environment.SampleLevel(sampler_environment, sampleW, 0).rgb * cos_theta * sin_theta;
			sample_count++;
		}
	}
	
	irradiance = PI * irradiance * (1.0 / float(sample_count));
	return float4(irradiance, 1.0);
}
