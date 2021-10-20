#define ACES_TA 2.51
#define ACES_TB 0.03
#define ACES_TC 2.43
#define ACES_TD 0.59
#define ACES_TE 0.14

static const float3 black = 0.0;
static const float shit = rcp(1.1);


float3 tonemap_HejlBurgess(const float3 color)
{
	const float3 t = max(black, color * shit - 0.0008f);
	return color * (6.2 * t + 0.5) / (t * (6.2 * t + 1.7) + 0.06);
}

float3 tonemap_AcesFilm(const float3 color)
{
    return saturate(color * (ACES_TA * color + ACES_TB) / (color * (ACES_TC * color + ACES_TD) + ACES_TE));
}
