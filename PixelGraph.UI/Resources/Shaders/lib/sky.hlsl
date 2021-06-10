#define SKY_DENSITY_D 0.35
#define SKY_EXPOSURE_D 0.00
#define SKY_DENSITY_N 0.65
#define SKY_EXPOSURE_N 0.00
#define SKY_DENSITY_W 1.50
#define SKY_EXPOSURE_W 0.00
#define SKY_HORIZON_N 1.00
#define SKY_HORIZON_F 1.50
#define SKY_GROUND 2
#define SKYBOX_BRIGHTNESS 2.00

#define SKY_R 96
#define SKY_G 160
#define SKY_B 255
#define SKY_I 1.00

#define FOG_DENSITY 1.00
#define LIGHT_SHAFT_STRENGTH 1.00

#pragma pack_matrix(row_major)


float3 skyColSqrt = float3(SKY_R, SKY_G, SKY_B) * SKY_I / 255.0;

float3 get_sky_color()
{
	return skyColSqrt * skyColSqrt;
}

float3 get_fog_color()
{
	return skyColSqrt * skyColSqrt;
}

float get_luminance(const float3 color)
{
	return dot(color, float3(0.299, 0.587, 0.114));
}

float3 get_sky_color(float3 viewPos, float3 sun, float3 up, float timeAngle, float timeBrightness, float sunVisibility, float3 lightSun, float rainStrength, bool isReflection)
{
    float3 nViewPos = normalize(viewPos);

    float VoU = clamp(dot(nViewPos,  up), -1, 1);
    float VoL = clamp(dot(nViewPos, sun), -1, 1);

    float groundDensity = 0.08 * (4 - 3 * sunVisibility) *
                          (10 * rainStrength * rainStrength + 1);
    
    float exposure = exp2(timeBrightness * 0.75 - 0.75 + SKY_EXPOSURE_D);
    float nightExposure = exp2(-3.5 + SKY_EXPOSURE_N);
    float weatherExposure = exp2(SKY_EXPOSURE_W);

    float gradientCurve = lerp(SKY_HORIZON_F, SKY_HORIZON_N, VoL);
    float baseGradient = exp(-(1 - pow(1 - max(VoU, 0), gradientCurve)) /
                             (SKY_DENSITY_D + 0.025));

    #if SKY_GROUND > 0
    float groundVoU = saturate(-VoU * 1.015 - 0.015);
    float ground = 1.0 - exp(-groundDensity * FOG_DENSITY / groundVoU);
    #if SKY_GROUND == 1
    if (!isReflection) ground = 1.0;
    #endif
    #else
    float ground = 1.0;
    #endif

    float3 sky = get_sky_color() * baseGradient / (SKY_I * SKY_I);
    sky = sky / sqrt(sky * sky + 1) * exposure * sunVisibility * (SKY_I * SKY_I);

    float sunMix = (VoL * 0.5 + 0.5) * pow(saturate(1 - VoU), 2 - sunVisibility) * pow(1 - timeBrightness * 0.6, 3);
    float horizonMix = pow(1 - abs(VoU), 2.5) * 0.125 * (1 - timeBrightness * 0.5);
    float lightMix = 1 - (1 - sunMix) * (1 - horizonMix);

    float3 lightSky = pow(abs(lightSun), 4 - sunVisibility) * baseGradient;
    lightSky = lightSky / (1 + lightSky * rainStrength);

    sky = lerp(
        sqrt(sky * (1 - lightMix)), 
        sqrt(lightSky), 
        lightMix
    );
    sky *= sky;

    const float night_gradient = exp(-max(VoU, 0) / SKY_DENSITY_N);
    const float3 night_sky = lightNight * lightNight * night_gradient * nightExposure;
    sky = lerp(night_sky, sky, sunVisibility * sunVisibility);

    const float rain_gradient = exp(-max(VoU, 0) / SKY_DENSITY_W);
    float3 weather_sky = weatherCol.rgb * weatherCol.rgb * weatherExposure;
    const float3 ambient_color = get_ambient_color(timeAngle, timeBrightness, sunVisibility, rainStrength);
    weather_sky *= get_luminance(ambient_color / weather_sky) * (0.2 * sunVisibility + 0.2);
    sky = lerp(sky, weather_sky * rain_gradient, rainStrength);

    sky *= ground;

    //if (cameraPosition.y < 1.0) sky *= exp(2.0 * cameraPosition.y - 2.0);

    return sky;
}

void round_sun_moon(inout float3 color, float3 viewPos, float3 sunVec, float3 sunColor, float3 moonColor, float sunVisibility, float moonVisibility, float rainStrength)
{
	float VoL = dot(normalize(viewPos), sunVec);
	float isMoon = float(VoL < 0.0);
	float sun = pow(abs(VoL), 800.0 * isMoon + 800.0) * (1.0 - sqrt(rainStrength));

	float3 sunMoonCol = lerp(moonColor * moonVisibility, sunColor * sunVisibility, float(VoL > 0.0));
	color += sun * sunMoonCol * 32.0;
}

void sun_glare(inout float3 color, float3 viewPos, float3 lightVec, float3 lightCol, float timeBrightness, float shadowFade, float rain_strength, float eBS)
{
	float VoL = dot(normalize(viewPos), lightVec);
	float visfactor = 0.05 * (-0.8 * timeBrightness + 1.0) * (3.0 * rain_strength + 1.0);
	float invvisfactor = 1.0 - visfactor;

	float visibility = saturate(VoL * 0.5 + 0.5);
    visibility = visfactor / (1 - invvisfactor * visibility) - visfactor;
	visibility = saturate(visibility * 1.015 / invvisfactor - 0.015);
	visibility = lerp(1.0, visibility, 0.25 * eBS + 0.75) * (1 - rain_strength * eBS * 0.875);
	visibility *= shadowFade * LIGHT_SHAFT_STRENGTH;
	
	//if (cameraPosition.y < 1) visibility *= exp(2 * cameraPosition.y - 2);

	#ifdef LIGHT_SHAFT
	if (isEyeInWater == 1) color += 0.25 * lightCol * visibility;
	#else
	color += 0.25 * lightCol * visibility;
	#endif
}
