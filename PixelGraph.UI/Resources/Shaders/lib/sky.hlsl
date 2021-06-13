#pragma pack_matrix(row_major)

static const float3 rayleigh = float3(0.251, 0.406, 0.520);
static const float rayleigh_att = 3.0;

static const float mie = 0.021;
static const float mie_att = 1.8;

static const float3 beta_r = float3(0.0195, 0.11, 0.294);
static const float3 beta_m = float3(0.04, 0.04, 0.04);

static const float g1 = 1.2;


float3 atmospheric_scattering(const float s_raleigh, const float s_mie, const float cosine, out float3 extinction)
{
    extinction = exp(-(beta_r * s_raleigh + beta_m * s_mie));

    const float g2 = g1 * g1;
    const float fcos2 = cosine * cosine;
	const float x = 1.0 + g2 + 2.0 * g1 * cosine;
    const float mie_phase = mie * pow(abs(x), -1.5) * (1.0 - g2) / (2.0 + g2);

    return (1.0 + fcos2) * (rayleigh + beta_m / beta_r * mie_phase);
}

float3 get_sky_color(const float3 view)
{
	const float sun = saturate(dot(view, SunDirection));

    // optical depth -> zenithAngle
	const float zenith_angle = max(view.y, EPSILON);
	const float s_raleigh = rayleigh_att / zenith_angle;
	const float s_mie = mie_att / zenith_angle;

    float3 extinction;
    const float3 scatter = atmospheric_scattering(s_raleigh, s_mie, sun, extinction) * (SunStrength * 0.8 + 0.2);
	
    float3 col = scatter * (1.0 - extinction); // * (SunStrength * 0.92 + 0.08);
	
    // sun
    col += 2.0 * float3(1.6, 1.4, 1.0) * pow(sun, 350) * extinction * SunStrength;
	
    // sun haze
    col += 0.4 * float3(0.8, 0.9, 1.0) * pow(sun, 2) * extinction * SunStrength;

	return col;
}
