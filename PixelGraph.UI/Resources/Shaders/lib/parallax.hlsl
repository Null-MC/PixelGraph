#pragma pack_matrix( row_major )


float2 get_parallax_texcoord(const float2 tex, const float2 offsetT, const float3 normal, const float3 eye) {
	const float2 dx = ddx(tex);
	const float2 dy = ddy(tex);
	
	const int step_count = (int)lerp(ParallaxSamplesMax, ParallaxSamplesMin, dot(-eye, normal));

	const float step_size = 1.0 / (float) step_count;

	const float2 step_offset = step_size * offsetT;
	
	float current_bound = 1.0;
	//float current_height = 0.0;
	float2 current_offset = tex;
	//float2 prev_offset = tex;
	//float prev_surface_height = 0.0;
	//float prev_height = 1.0;

	int step_index = 0;
	while (step_index < step_count) {
		current_offset -= step_offset;

		const float current_height = tex_normal_height.SampleGrad(sampler_surface, current_offset, dx, dy).a;

		current_bound -= step_size;

		if (current_height > current_bound) break;
		
		step_index++;
		//prev_height = current_height;
		//prev_offset = current_offset;
		//prev_surface_height = current_height;
	}

	//const float prev_difference = prev_height - prev_surface_height;
	//const float difference = current_height - step_size;
	//const float t = prev_difference / (prev_difference + difference);
	//return prev_offset - step_offset * t;
	return current_offset;
}
