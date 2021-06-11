#pragma pack_matrix(row_major)

static const float3 rayleigh = float3(5.5e-2, 13.0e-2, 22.4e-2);
static const float rayleigh_att = 1;

static const float mie = 21e-2;
static const float mie_att = 1.2;

static const float3 beta_r = float3(0.0195, 0.11, 0.294);
static const float3 beta_m = float3(0.04, 0.04, 0.04);

static const float g1 = 1.2;


float3 atmospheric_scattering(const float s_raleigh, const float s_mie, const float cosine, out float3 extinction)
{
    extinction = exp(-(beta_r * s_raleigh + beta_m * s_mie));

    const float g2 = g1 * g1;
    const float fcos2 = cosine * cosine;
	const float x = 1 + g2 + 2 * g1 * cosine;
    const float mie_phase = mie * pow(abs(x), -1.5) * (1 - g2) / (2 + g2);

    return (1 + fcos2) * (rayleigh + beta_m / beta_r * mie_phase);
}

float3 get_sky_color(const float3 view)
{
	const float sun = saturate(dot(view, SunDirection));

    // optical depth -> zenithAngle
	const float zenith_angle = max(view.y, EPSILON);
	const float s_raleigh = rayleigh_att / zenith_angle;
	const float s_mie = mie_att / zenith_angle;

    float3 extinction;
    const float3 scatter = atmospheric_scattering(s_raleigh, s_mie, sun * SunStrength, extinction);
	
    float3 col = scatter * (1 - extinction);
	
    // sun
    col += 0.47 * float3(1.6, 1.4, 1.0) * pow(sun, 350) * extinction;
	
    // sun haze
    col += 0.4 * float3(0.8, 0.9, 1.0) * pow(sun, 2) * extinction;

	return col;
}

//float3 diffuse_IBL(const float3 albedo, const float3 normal, const float3 eye, const float f0, const float alpha) {
//    uint N = 256u;
//    float3 result = float3(0.0f, 0.0f, 0.0f);
//    for (uint i = 0u; i < N; ++i) {
//        float3 dir = generateUnitVector(hammersley2d(i, N));
//
//        const float nDotV = dot(normal, eye);
//        const float nDotL = saturate(dot(normal, dir));
//        const float nDotH = abs(dot(normal, normalize(dir + eye))) + 1e-5;
//        const float lDotV = dot(dir, eye);
//        const float vDotH = dot(eye, normalize(dir + eye));
//
//        float3 diffuse = hammonDiffuse(albedo, f0, nDotV, nDotL, nDotH, lDotV, alpha);
//        result += diffuse * tex_cube.Sample(sampler_IBL, dir).rgb;
//    }
//	
//    return result / (float)N;
//}
