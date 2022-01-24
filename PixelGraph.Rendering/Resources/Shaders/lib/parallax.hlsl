#pragma pack_matrix(row_major)

static const float slope_strength = 40.0f;
static const float soft_shadow_strength = 60.0;


float2 get_parallax_offset(const in float3 lightT, const in float2 uv_size)
{
	const float length_sq = dot(lightT, lightT);
	const float parallax_length = sqrt(length_sq - lightT.z * lightT.z) / lightT.z;
	float2 parallax_dir = normalize(lightT.xy);

	if (uv_size.x < 0.0f) parallax_dir.y *= -1;
	else if (uv_size.y < 0.0f) parallax_dir.y *= -1;

	parallax_dir.y *= -1;
	return parallax_dir * parallax_length * ParallaxDepth;
}

float2 get_parallax_texcoord(const in float2 tex, const in float2 offsetT, const in float NoV, out float3 shadow_tex, out float tex_depth) //, out float3 hit_normal)
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

float2 get_parallax_texcoord_wet(const in float2 tex, const in float2 vTS, const in float2 rTS, const in float NoV, const in float water_level, out float2 water_tex, out float3 shadow_tex, out float tex_depth)
{
	const int step_count = (int)lerp(ParallaxSamplesMax, ParallaxSamplesMin, NoV);
	const float step_size = rcp(step_count);
	float2 step_offset = step_size * vTS;
	
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
			//water_tex.z = water_level;
			//water_tex.w = saturate(water_level - tex_depth);
			hit_water = true;

			step_offset = step_size * rTS;
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
		//water_tex.z = 0.0f;
	}

	return EnableLinearSampling ? shadow_tex.xy : trace_offset;
}

float get_parallax_shadow(const in float3 tex, const in float2 offsetT, const in float NoL)
{
	if (NoL <= 0.0) return 0.0;
	
	const int step_count = (int)lerp(ParallaxSamplesMax, ParallaxSamplesMin, NoL);
	const float step_size = rcp(step_count);
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

float3 apply_slope_normal(const in float2 tex, const in float2 step_dir, const in float trace_depth)
{
	float3 tex_size;
    tex_normal_height.GetDimensions(0, tex_size.x, tex_size.y, tex_size.z);
	const float2 pixel_size = rcp(tex_size.xy);

	const float2 tex_snapped = floor(tex * tex_size.xy) * pixel_size;
    const float2 tex_offset = tex - tex_snapped - 0.5f * pixel_size;
	const float2 step_sign = sign(step_dir);

	const float2 tex_x = tex_snapped + float2(pixel_size.x * step_sign.x, 0.0f);
	const float height_x = tex_normal_height.SampleLevel(sampler_height, tex_x, 0).a;
	const bool has_x = trace_depth > height_x && sign(tex_offset.x) == step_sign.x;

	const float2 tex_y = tex_snapped + float2(0.0f, pixel_size.y * step_sign.y);
	const float height_y = tex_normal_height.SampleLevel(sampler_height, tex_y, 0).a;
	const bool has_y = trace_depth > height_y && sign(tex_offset.y) == step_sign.y;

    if (abs(tex_offset.x) < abs(tex_offset.y)) {
		if (has_y) return float3(0.0f, -step_sign.y, 0.0f);
		if (has_x) return float3(step_sign.x, 0.0f, 0.0f);
    }
	else {
		if (has_x) return float3(step_sign.x, 0.0f, 0.0f);
		if (has_y) return float3(0.0f, -step_sign.y, 0.0f);
	}

    float s = step(abs(step_dir.y), abs(step_dir.x));
    return float3(float2(1.0f - s, s) * step_sign, 0.0f);
}
