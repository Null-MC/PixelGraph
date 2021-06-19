#define EPSILON 1e-6f

#pragma pack_matrix(row_major)

static const float soft_shadow_strength = 60.0;


float2 get_parallax_offset(const in float3x3 mTBN, const in float3 direction)
{
	const float3 lightT = mul(mTBN, direction);
	const float length_sq = dot(lightT, lightT);
	const float parallax_length = sqrt(length_sq - lightT.z * lightT.z) / lightT.z;
	const float2 parallax_dir = normalize(lightT.xy);
	return parallax_dir * parallax_length * ParallaxDepth;
}

float2 get_parallax_texcoord(const float2 tex, const float2 dx, const float2 dy, const float2 offsetT, const float NoV, out float3 shadow_tex)
{
	const int step_count = (int)lerp(ParallaxSamplesMax, ParallaxSamplesMin, NoV);
	const float step_size = 1.0 / step_count;
	const float2 step_offset = step_size * offsetT;
	
	float trace_depth = 1.0;
	float2 trace_offset = tex;
	float2 prev_trace_offset = tex + step_offset;

	float tex_depth = 1.0;
	float prev_tex_depth = 1.0;
	float prev_trace_depth = 1.0 + step_size;

	[loop]
	for (int step = 0; step < step_count; ++step) {
		tex_depth = tex_normal_height.SampleLevel(sampler_height, trace_offset, 0).a;
		if (trace_depth < tex_depth) break;

		prev_tex_depth = tex_depth;
		prev_trace_depth = trace_depth;
		prev_trace_offset = trace_offset;
		
		trace_offset -= step_offset;
		trace_depth -= step_size;
	}

	const float t = (prev_trace_depth - prev_tex_depth) / max(tex_depth - prev_tex_depth + prev_trace_depth - trace_depth, EPSILON);
	float2 tex_i = prev_trace_offset - t * step_offset;
	float depth_i = prev_trace_depth - t * step_size;
	shadow_tex = float3(tex_i, depth_i);
	
	return EnableLinearSampling ? tex_i : trace_offset;
}

float get_parallax_shadow(const float3 tex, const float2 dx, const float2 dy, const float2 offsetT, const float NoL)
{
	if (NoL <= 0.0) return 0.0;
	
	const int step_count = (int)lerp(ParallaxSamplesMax, ParallaxSamplesMin, NoL);
	const float step_size = 1.0 / step_count;
	const float2 step_offset = step_size * offsetT;
	
	float trace_depth = tex.z;
    float2 trace_tex = tex.xy + step_offset;

	[loop]
	float result = 0.0;
	for (int step = int(tex.z); step < step_count; ++step) {
        trace_tex += step_offset;
        trace_depth += step_size;

        const float tex_depth = tex_normal_height.SampleLevel(sampler_height, trace_tex, 0).a;

        const float h = tex_depth - trace_depth;
		
    	if (h > 0.0) {
	        const float dist = 1.0 + lengthSq(float3(trace_tex, trace_depth) - float3(tex.xy, 1.0)) * 20.0;

            const float sample_result = saturate(h * soft_shadow_strength * (1.0 / dist));
    		result = max(result, sample_result);
    		if (1.0 - result < EPSILON) break;
    	}
	}

    return 1.0 - result;
}
