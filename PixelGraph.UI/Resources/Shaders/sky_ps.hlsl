#include "lib/common_structs.hlsl"
#include "lib/sky_colors.hlsl"
#include "lib/sky.hlsl"

#define STAR 0
#define ANIMATION_SPEED 1
#define rainStrength 0

// eyeBrightnessSmooth.y
#define SKY_BRIGHTNESS 255

#pragma pack_matrix(row_major)


float3 get_sun(float2 uv, float2 sun_uv)
{
	float sun = 1.0 - distance(uv, sun_uv);
    sun = clamp(sun,0.0,1.0);
    
    float glow = sun;
    glow = clamp(glow,0.0,1.0);
    
    sun = pow(sun,100.0);
    sun *= 100.0;
    sun = clamp(sun,0.0,1.0);
    
    glow = pow(glow,6.0) * 1.0;
    glow = pow(glow,(uv.y));
    glow = clamp(glow,0.0,1.0);
    
    sun *= pow(dot(uv.y, uv.y), 1.0 / 1.65);
    
    glow *= pow(dot(uv.y, uv.y), 1.0 / 2.0);
    
    sun += glow;
    
    return float3(1, 0.6, 0.05) * sun;
}

float4 main(const ps_input_cube input) : SV_TARGET
{
	//const float3 color = tex_cube.SampleLevel(sampler_cube, input.t, 0);

	//float4 screenPos = float4(input.t.xy / float2(vViewport.x, vViewport.y), input.t.z, 1);
	//vec4 viewPos = gbufferProjectionInverse * (screenPos * 2 - 1);
	//viewPos /= viewPos.w;

	float3 view_pos = normalize(input.posV.xyz);
	float3 sun_pos = normalize(input.posV.xyz);
	float3 sun = get_sun(view_pos, )
	return float4(, 1);
	
	//const float3 view_pos = input.posV.xyz;
	//return float4(normalize(view_pos), 1);
	//float l = pow(dot(-SunDirection, input.tex), 10);
	//return float4(l, l, l, 1);
	
	float eBS = SKY_BRIGHTNESS / 240;
	float sunVisibility  = saturate((dot(input.sun, input.up) + 0.05) * 10);
	float moonVisibility = saturate((dot(-input.sun, input.up) + 0.05) * 10);

	float SUN_ANGLE = 0;
	float tAmin = frac(SUN_ANGLE - 0.033333333);
	float tAlin = tAmin < 0.433333333 ? tAmin * 1.15384615385 : tAmin * 0.882352941176 + 0.117647058824;
	float hA = tAlin > 0.5 ? 1 : 0;
	float tAfrc = frac(tAlin * 2);
	float tAfrs = tAfrc * tAfrc * (3 - 2 * tAfrc);
	float tAmix = hA < 0.5 ? 0.3 : -0.1;
	float timeAngle = (tAfrc * (1 - tAmix) + tAfrs * tAmix + hA) * 0.5;
	float timeBrightness = max(sin(timeAngle * 6.28318530718), 0);
	float shadowFade = saturate(1 - (abs(abs(SUN_ANGLE - 0.5) - 0.25) - 0.23) * 100);
	
	float3 lightVec = input.sun * (1 - 2 * float(timeAngle > 0.5325 && timeAngle < 0.9675));

	//float frametime = TimeStamp * 0.05 * ANIMATION_SPEED;

	float3 light_sun = get_light_sun(timeAngle, timeBrightness);
	float3 albedo = get_sky_color(view_pos.xyz, input.sun, input.up, timeAngle, timeBrightness, sunVisibility, light_sun, rainStrength, false);

	return float4(albedo, 1);
	
	float mefade = get_morning_evening_fade(timeAngle);
	float3 lightMA = lerp(lightMorning, lightEvening, mefade);
    float3 sunColor = lerp(lightMA, sqrt(lightDay * lightMA * LIGHT_DI), timeBrightness);
    float3 moonColor = sqrt(lightNight);

	round_sun_moon(albedo, view_pos.xyz, input.sun, sunColor, moonColor, sunVisibility, moonVisibility, rainStrength);

	#ifdef STARS
	if (moonVisibility > 0.0) DrawStars(albedo.rgb, viewPos.xyz);
	#endif

	//float dither = Bayer64(gl_FragCoord.xy);

	#ifdef AURORA
	albedo.rgb += DrawAurora(viewPos.xyz, dither, 24);
	#endif
	
	#ifdef CLOUDS
	vec4 cloud = DrawCloud(viewPos.xyz, dither, lightCol, ambientCol);
	albedo.rgb = lerp(albedo.rgb, cloud.rgb, cloud.a);
	#endif

	const float3 light_color = get_light_color(timeAngle, timeBrightness, sunVisibility, rainStrength);
	sun_glare(albedo, view_pos.xyz, lightVec, light_color, timeBrightness, shadowFade, rainStrength, eBS);

	albedo.rgb *= 4.0 - 3.0 * eBS;
	
	return float4(albedo, 1 - STAR);
}
