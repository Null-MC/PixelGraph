#define EPSILON 1e-6f

#pragma pack_matrix(row_major)

static const float soft_shadow_strength = 60.0;


float2 get_parallax_offset(const in float3 lightT, const in float2 uv_size)
{
	//const float3 lightT = mul(mTBN, view);
	const float length_sq = dot(lightT, lightT);
	const float parallax_length = sqrt(length_sq - lightT.z * lightT.z) / lightT.z;
	float2 parallax_dir = normalize(lightT.xy);

	//if (uv_size.x < 0.0f) parallax_dir.x *= -1;
	//if (uv_size.y < 0.0f) parallax_dir.y *= -1;
	if (uv_size.x < 0.0f && uv_size.y < 0.0f) {
		//parallax_dir.xy *= -1;
	}
	else {
		if (uv_size.x < 0.0f) parallax_dir.y *= -1;
		if (uv_size.y < 0.0f) parallax_dir.x *= -1;
	}

    //if (uv_size.x < 0.0f) parallax_dir.y *= -1;
    //if (uv_size.y < 0.0f) parallax_dir.x *= -1;

	return parallax_dir * parallax_length * ParallaxDepth;
}

float2 get_parallax_texcoord(const in float2 tex, const in float2 offsetT, const in float NoV, out float3 shadow_tex, out float tex_depth)
{
	const int step_count = (int)lerp(ParallaxSamplesMax, ParallaxSamplesMin, NoV);
	const float step_size = 1.0 / step_count;
	const float2 step_offset = step_size * offsetT;
	
	float trace_depth = 1.0;
	float2 trace_offset = tex;
	float2 prev_trace_offset = tex + step_offset;

	tex_depth = 1.0;
	float prev_tex_depth = 1.0;
	float prev_trace_depth = 1.0 + step_size;

	[loop]
	for (int step = 0; step < step_count; ++step) {
		tex_depth = tex_normal_height.SampleLevel(sampler_height, trace_offset, 0).a;
		if (trace_depth <= tex_depth) break;

		prev_tex_depth = tex_depth;
		prev_trace_depth = trace_depth;
		prev_trace_offset = trace_offset;
		
		trace_offset -= step_offset;
		trace_depth -= step_size;
	}

	const float t = (prev_trace_depth - prev_tex_depth) / max(tex_depth - prev_tex_depth + prev_trace_depth - trace_depth, EPSILON);
	shadow_tex.xy = prev_trace_offset - t * step_offset;
	shadow_tex.z = prev_trace_depth - t * step_size;
	
	return EnableLinearSampling ? shadow_tex.xy : trace_offset;
}

float2 get_parallax_texcoord_wet(const in float2 tex, const in float2 offsetT, const in float NoV, const in float water_level, out float3 water_tex, out float3 shadow_tex, out float tex_depth)
{
	const int step_count = (int)lerp(ParallaxSamplesMax, ParallaxSamplesMin, NoV);
	const float step_size = 1.0 / step_count;
	const float2 step_offset = step_size * offsetT;
	
	float trace_depth = 1.0;
	float2 trace_offset = tex;
	float2 prev_trace_offset = tex + step_offset;

	tex_depth = 1.0;
	float prev_tex_depth = 1.0;
	float prev_trace_depth = 1.0 + step_size;

	bool hit_water = false;

	[loop]
	for (int step = 0; step < step_count; ++step) {
		tex_depth = tex_normal_height.SampleLevel(sampler_height, trace_offset, 0).a;

		if (!hit_water && trace_depth <= water_level) {
			water_tex.xy = trace_offset;
			water_tex.z = max(water_level - tex_depth, 0.0f);
			hit_water = true;
		}

		if (trace_depth <= tex_depth) break;

		prev_tex_depth = tex_depth;
		prev_trace_depth = trace_depth;
		prev_trace_offset = trace_offset;
		
		trace_offset -= step_offset;
		trace_depth -= step_size;
	}

	const float t = (prev_trace_depth - prev_tex_depth) / max(tex_depth - prev_tex_depth + prev_trace_depth - trace_depth, EPSILON);
	shadow_tex.xy = prev_trace_offset - t * step_offset;
	shadow_tex.z = prev_trace_depth - t * step_size;

	if (!hit_water) {
		water_tex.xy = shadow_tex.xy;
		water_tex.z = 0.0f;
	}

	return EnableLinearSampling ? shadow_tex.xy : trace_offset;
}

float get_parallax_shadow(const in float3 tex, const in float2 offsetT, const in float NoL)
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

float3 get_slope_normal(const in float2 tex, const in float3 view, const in float3 tangent, const in float3 bitangent)
{
	float3 tex_size, tex_normal;
    tex_normal_height.GetDimensions(0, tex_size.x, tex_size.y, tex_size.z);
	const float2 tex_snapped = round(tex * tex_size.xy) / tex_size.xy;
    float2 tex_offset = tex - tex_snapped;

    if (abs(tex_offset.y) < abs(tex_offset.x)) {
        tex_normal = bitangent * sign(-tex_offset.y);

        const float VoN = dot(view, tex_normal);
		if (VoN < 0) tex_normal = tangent * sign(-tex_offset.x);
    }
    else {
        tex_normal = tangent * sign(-tex_offset.x);

		float VoN = dot(view, tex_normal);
		if (VoN < 0) tex_normal = bitangent * sign(-tex_offset.y);
    }

	return tex_normal;
}