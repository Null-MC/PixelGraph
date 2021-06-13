#pragma pack_matrix( row_major )

static const float soft_shadow_bias = 0.1;
static const float soft_shadow_strength = 40.0;


float2 get_parallax_offset(const in float3x3 mTBN, const in float3 direction)
{
	const float3 lightT = mul(mTBN, direction);
	const float length_sq = dot(lightT, lightT);
	const float parallax_length = sqrt(length_sq - lightT.z * lightT.z) / lightT.z;
	const float2 parallax_dir = normalize(lightT.xy);
	return parallax_dir * parallax_length * ParallaxDepth;
}

float2 get_parallax_texcoord(const float2 tex, const float2 dx, const float2 dy, const float3 normal, const float2 offsetT, const float3 view_dir, out float depth)
{
	const int step_count = (int)lerp(ParallaxSamplesMax, ParallaxSamplesMin, dot(-view_dir, normal));
	const float step_size = 1.0 / step_count;
	const float2 step_offset = step_size * offsetT;
	
	float trace_depth = 1.0 - step_size;
	float2 trace_offset = tex;

	float prev_tex_depth = 1.0;
	float prev_trace_depth = 1.0;

	int step_index = 0;
	while (step_index < step_count) {
		trace_offset -= step_offset;
		trace_depth -= step_size;
		
		const float tex_depth = tex_normal_height.SampleGrad(sampler_height, trace_offset, dx, dy).a;
		
		if (trace_depth < tex_depth) {
			depth = tex_depth - trace_depth;
			return trace_offset;
			//const float t = (prev_trace_depth - prev_tex_depth) / (tex_depth - prev_tex_depth + prev_trace_depth - trace_depth);
			//return (trace_offset + step_offset) - t * step_offset;
		}

		prev_tex_depth = tex_depth;
		prev_trace_depth = trace_depth;
		step_index++;
	}

	depth = prev_tex_depth;
	return trace_offset;
}

float get_parallax_shadow(const float2 tex, const float side_bias, const float2 dx, const float2 dy, const float3 normal, const float2 offsetT, const float3 light_dir)
{
	const float NdotL = dot(normal, light_dir);
	if (NdotL < 0.0) return 0.0;
	
	const int step_count = (int)lerp(ParallaxSamplesMax, ParallaxSamplesMin, NdotL);
	const float step_size = 1.0 / step_count;
	const float2 step_offset = step_size * offsetT;

	
    float2 trace_tex = tex - step_offset * side_bias;
    float tex_depth = tex_normal_height.SampleGrad(sampler_height, trace_tex, dx, dy).a;

	//float2 trace_tex = tex - step_offset;
    //float tex_depth = tex_normal_height.SampleGrad(sampler_height, trace_tex, dx, dy).a;
    float trace_depth = tex_depth; //min(tex_depth, depth);

	//float ignore = 1 - saturate((max(depth, tex_depth) - trace_depth) * 20);
	//return ignore;

	float result = 1.0;

	for (int step = 0; step < step_count; ++step) {
        trace_tex += step_offset;
        trace_depth += step_size;
    	
        tex_depth = tex_normal_height.SampleGrad(sampler_height, trace_tex, dx, dy).a;
		
    	if (trace_depth < tex_depth) {
    		//return 0;
    		const float step_factor = 1.0 - step / (float)step_count;
	        float newShadowMultiplier = saturate((tex_depth - trace_depth) * soft_shadow_strength) * step_factor;

    		//if (step_count - step < 1)
    		//	newShadowMultiplier -= ignore * step_factor;
    		
    		result = min(result, 1.0 - newShadowMultiplier);
    	}
	}

    return result;
}
