#pragma pack_matrix( row_major )

static const float soft_shadow_bias = 0.1;
static const float soft_shadow_strength = 60.0;


float2 get_parallax_offset(const in float3x3 mTBN, const in float3 direction)
{
	const float3 lightT = mul(mTBN, direction);
	const float length_sq = dot(lightT, lightT);
	const float parallax_length = sqrt(length_sq - lightT.z * lightT.z) / lightT.z;
	const float2 parallax_dir = normalize(lightT.xy);
	return parallax_dir * parallax_length * ParallaxDepth;
}

float2 get_parallax_texcoord(const float2 tex, const float2 dx, const float2 dy, const float3 normal, const float2 offsetT, const float3 view_dir, out float depth_offset)
{
	const int step_count = (int)lerp(ParallaxSamplesMax, ParallaxSamplesMin, dot(-view_dir, normal));
	const float step_size = 1.0 / step_count;
	const float2 step_offset = step_size * offsetT;
	
	float trace_depth = 1.0 - step_size;
	float2 trace_offset = tex;

	//float prev_tex_depth = 1.0;
	//float prev_trace_depth = 1.0;

	for (int step = 0; step < step_count; ++step) {
		trace_offset -= step_offset;
		trace_depth -= step_size;
		
		const float tex_depth = tex_normal_height.SampleGrad(sampler_height, trace_offset, dx, dy).a;
		
		if (trace_depth < tex_depth) {
			depth_offset = tex_depth - trace_depth;
			return trace_offset;
			//const float t = (prev_trace_depth - prev_tex_depth) / (tex_depth - prev_tex_depth + prev_trace_depth - trace_depth);
			//return (trace_offset + step_offset) - t * step_offset;
		}

		//prev_tex_depth = tex_depth;
		//prev_trace_depth = trace_depth;
	}

	depth_offset = 0.0;
	return trace_offset;
}

float get_parallax_shadow(const float2 tex, const float depth_offset, const float2 dx, const float2 dy, const float3 normal, const float2 offsetT, const float3 light_dir)
{
	const float NdotL = dot(normal, light_dir);
	if (NdotL < 0.0) return 0.0;
	
	const int step_count = (int)lerp(ParallaxSamplesMax, ParallaxSamplesMin, NdotL);
	const float step_size = 1.0 / step_count;
	const float2 step_offset = step_size * offsetT;

	
    float2 trace_tex = tex;
    float tex_depth = tex_normal_height.SampleGrad(sampler_height, trace_tex, dx, dy).a;
    float trace_depth = tex_depth - depth_offset;
	
	float result = 1.0;
	for (int step = 0; step < step_count; ++step) {
        trace_tex += step_offset;
        trace_depth += step_size;
    	
        tex_depth = tex_normal_height.SampleGrad(sampler_height, trace_tex, dx, dy).a;
		
    	if (trace_depth < tex_depth) {
    		//const float step_factor = 1.0 - step / (float)step_count;
    		float distance = 1; //saturate(1 - 0.1 * length(trace_tex - tex));
            const float sample_result = saturate((tex_depth - trace_depth) * soft_shadow_strength) * distance;
    		result = min(result, 1.0 - sample_result);
    	}
	}

    return result;
}
