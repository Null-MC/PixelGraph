#include "lib/common_structs.hlsl"

#define PI 3.14159265358979

#pragma pack_matrix(row_major)


float3 get_sun_angle(const in float time, const in float roll)
{
	const float tilt = frac(time / 24000) * 360;

	const float fx = radians(roll);
	const float fy = radians(tilt);

	const float fx_cos = cos(fx);
	const float fy_cos = cos(fy);
    const float fy_sin = sin(fy);

    return float3(fx_cos * fy_cos, fy_sin * fx_cos, fy_sin);
}

ps_input_cube main(float3 input : SV_Position)
{
	ps_input_cube output;
    output.tex = input;

	float4x4 viewNoTranslate = mView;
    viewNoTranslate._m30_m31_m32 = 0;
	
    //Set w = 0 to make sure depth is infinite. Must disable depth clipping
	output.posV = mul(float4(input, 0), viewNoTranslate);
    output.pos = mul(output.posV, mProjection);

	
	//const float2 sunRotationData = float2(cos(SUN_PATH_ROTATION * 0.01745329251994), -sin(SUN_PATH_ROTATION * 0.01745329251994));

	//const float tAmin = frac(SUN_ANGLE - 0.033333333);
	//const float tAlin = tAmin < 0.433333333 ? tAmin * 1.15384615385 : tAmin * 0.882352941176 + 0.117647058824;
	//const float hA = tAlin > 0.5 ? 1 : 0;
	//const float tAfrc = frac(tAlin * 2);
	//const float tAfrs = tAfrc * tAfrc * (3 - 2 * tAfrc);
	//const float tAmix = hA < 0.5 ? 0.3 : -0.1;
	//const float timeAngle = (tAfrc * (1 - tAmix) + tAfrs * tAmix + hA) * 0.5;
	
	//float ang = frac(timeAngle - 0.25);
	
	//ang = (ang + (cos(ang * PI) * -0.5 + 0.5 - ang) / 3) * 6.28318530717959;

	//float3 x1 = float3(-sin(ang), cos(ang) * sunRotationData) * 2000;
	output.sun = normalize(mul(float4(SunDirection, 1), mView).xyz);

	output.up = normalize(mView[1].xyz);
	
	//output.star = float(gl_Color.r == gl_Color.g && gl_Color.g == gl_Color.b && gl_Color.r > 0.0);

	
	return output;
}
