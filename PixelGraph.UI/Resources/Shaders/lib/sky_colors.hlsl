// MORNING
#define LIGHT_MR 255
#define LIGHT_MG 160
#define LIGHT_MB 80
#define LIGHT_MI 1.20

#define AMBIENT_MR 255
#define AMBIENT_MG 204
#define AMBIENT_MB 144
#define AMBIENT_MI 0.40

// DAY
#define LIGHT_DR 192
#define LIGHT_DG 216
#define LIGHT_DB 255
#define LIGHT_DI 1.40

#define AMBIENT_DR 120
#define AMBIENT_DG 172
#define AMBIENT_DB 255
#define AMBIENT_DI 0.60

// EVENING
#define LIGHT_ER 255
#define LIGHT_EG 160
#define LIGHT_EB 80
#define LIGHT_EI 1.20

#define AMBIENT_ER 255
#define AMBIENT_EG 204
#define AMBIENT_EB 144
#define AMBIENT_EI 0.40

// NIGHT
#define LIGHT_NR 96
#define LIGHT_NG 192
#define LIGHT_NB 255
#define LIGHT_NI 1.00

#define AMBIENT_NR 96
#define AMBIENT_NG 192
#define AMBIENT_NB 255
#define AMBIENT_NI 0.60

#define WEATHER_RR 176
#define WEATHER_RG 224
#define WEATHER_RB 255
#define WEATHER_RI 1.20

#pragma pack_matrix(row_major)


float3 lightMorning    = float3(LIGHT_MR,   LIGHT_MG,   LIGHT_MB)   * LIGHT_MI / 255.0;
float3 lightDay        = float3(LIGHT_DR,   LIGHT_DG,   LIGHT_DB)   * LIGHT_DI / 255.0;
float3 lightEvening    = float3(LIGHT_ER,   LIGHT_EG,   LIGHT_EB)   * LIGHT_EI / 255.0;
float3 lightNight      = float3(LIGHT_NR,   LIGHT_NG,   LIGHT_NB)   * LIGHT_NI * 0.3 / 255.0;

float3 ambientMorning  = float3(AMBIENT_MR, AMBIENT_MG, AMBIENT_MB) * AMBIENT_MI / 255.0;
float3 ambientDay      = float3(AMBIENT_DR, AMBIENT_DG, AMBIENT_DB) * AMBIENT_DI / 255.0;
float3 ambientEvening  = float3(AMBIENT_ER, AMBIENT_EG, AMBIENT_EB) * AMBIENT_EI / 255.0;
float3 ambientNight    = float3(AMBIENT_NR, AMBIENT_NG, AMBIENT_NB) * AMBIENT_NI * 0.3 / 255.0;

float4 weatherCol = float4(float3(WEATHER_RR, WEATHER_RG, WEATHER_RB) / 255.0, 1.0) * WEATHER_RI;

float get_morning_evening_fade(float timeAngle)
{
	return 1 - saturate(abs(timeAngle - 0.5) * 8 - 1.5);
}

float3 get_sun_color(float3 morning, float3 day, float3 evening, float timeAngle, float timeBrightness) {
	const float morning_evening_fade = 1 - saturate(abs(timeAngle - 0.5) * 8 - 1.5);
	const float3 morning_evening = lerp(morning, evening, morning_evening_fade);

	const float dfade = 1 - timeBrightness;
	return lerp(morning_evening, day, 1 - dfade * sqrt(dfade));
}

float3 get_light_color(float3 sun, float3 night, float3 weatherCol, float sunVisibility, float rainStrength) {
	float3 c = lerp(night, sun, sunVisibility);
	c = lerp(c, dot(c, float3(0.299, 0.587, 0.114)) * weatherCol, rainStrength);
	return c * c;
}

float3 get_light_sun(float timeAngle, float timeBrightness)
{
	return get_sun_color(lightMorning, lightDay, lightEvening, timeAngle, timeBrightness);
}

float3 get_ambient_sun(float timeAngle, float timeBrightness)
{
	return get_sun_color(ambientMorning, ambientDay, ambientEvening, timeAngle, timeBrightness);
}

float3 get_light_color(float timeAngle, float timeBrightness, float sunVisibility, float rainStrength)
{
	const float3 light_sun = get_light_sun(timeAngle, timeBrightness);
	return get_light_color(light_sun, lightNight, weatherCol.rgb, sunVisibility, rainStrength);
}

float3 get_ambient_color(float timeAngle, float timeBrightness, float sunVisibility, float rainStrength)
{
	const float3 ambient_sun = get_ambient_sun(timeAngle, timeBrightness);
	return get_light_color(ambient_sun, ambientNight, weatherCol.rgb, sunVisibility, rainStrength);
}
