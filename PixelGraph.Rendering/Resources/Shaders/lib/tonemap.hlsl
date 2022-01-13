//====  Stuff from Jessie ====//

float3 tonemap_HejlBurgess(const float3 color)
{
    static const float f = rcp(1.1);

	const float3 t = max(0.0f, color * f - 0.0008f);
	return color * (6.2f * t + 0.5f) / (t * (6.2f * t + 1.7f) + 0.06f);
}

float3 tonemap_AcesFilm(const float3 color)
{
    return saturate(color * (2.51f * color + 0.03f) / (color * (2.43f * color + 0.59f) + 0.14f));
}


//====  Stuff from Tech ====//

static const float3 luma_factor = float3(0.2126f, 0.7152f, 0.0722f);


float luminance(const in float3 color)
{
    return dot(color, luma_factor);
}

float3 tonemap_Reinhard(const in float3 color)
{
    return color / (1.0f + color);
}

float3 tonemap_ReinhardJodie(const in float3 color)
{
	const float luma = luminance(color);
	const float3 tonemapped_color = color / (1.0f + color);
    return lerp(color / (1.0f + luma), tonemapped_color, tonemapped_color);
}

float3 tonemap_Uncharted2(const in float3 x)
{
    static const float A = 0.15f;
    static const float B = 0.50f;
    static const float C = 0.10f;
    static const float D = 0.20f;
    static const float E = 0.02f;
    static const float F = 0.30f;

    return ((x*(A*x+C*B)+D*E)/(x*(A*x+B)+D*F))-E/F;
}

float3 tonemap_ACESFit(const in float3 x)
{
    static const float a = 1.9f;
    static const float b = 0.04f;
    static const float c = 2.43f;
    static const float d = 0.59f;
    static const float e = 0.14f;

    return clamp(x * (a * x+b) / (x * (c*x + d) + e), 0.0f, 1.0f);
}

// Based on http://www.oscars.org/science-technology/sci-tech-projects/aces
float3 tonemap_ACESFit2(const in float3 color){
    static const float3x3 m1 = float3x3(
        0.59719, 0.07600, 0.02840,
        0.35458, 0.90834, 0.13383,
        0.04823, 0.01566, 0.83777
    );

    static const float3x3 m2 = float3x3(
        1.60475, -0.10208, -0.00327,
        -0.53108,  1.10813, -0.07276,
        -0.07367, -0.00605,  1.07602
    );

    const float3 v = mul(color, m1);
    const float3 a = v * (v + 0.0245786) - 0.000090537;
    const float3 b = v * (0.983729 * v + 0.4329510) + 0.238081;
    return pow(clamp(mul(a / b, m2), 0.0f, 1.0f), 1.0f / 2.2f);
}

float3 tonemap_FilmicHejl2015(const in float3 hdr, const in float whitePoint)
{
	const float4 vh = float4(hdr, whitePoint);    // pack: [r, g, b, w]
	const float4 va = 1.425f * vh + 0.05f;
    float4 vf = (vh * va + 0.004f) / (vh * (va + 0.55f) + 0.0491f) - 0.0821f;
    return vf.rgb / vf.www;
}

float3 tonemap_Burgess(const in float3 color)
{
    const float3 maxColor = max(color - 0.004f, 0.0f);
    return maxColor * (6.2f * maxColor + 0.5f) / (maxColor * (6.2f * maxColor + 1.7f) + 0.06f);
}

float3 _ChangeLuma(const in float3 c_in, const in float l_out)
{
	const float l_in = luminance(c_in);
    return c_in * (l_out / l_in);
}

float3 tonemap_ReinhardExtendedLuminance(const in float3 color, const in float maxWhiteLuma)
{
	const float luma_old = luminance(color);
	const float numerator = luma_old * (1.0f + luma_old / (maxWhiteLuma * maxWhiteLuma));
	const float luma_new = numerator / (1.0f + luma_old);
    return _ChangeLuma(color, luma_new);
}

//#define TONEMAP_EXPOSURE 4.0
//#define TONEMAP_WHITE_CURVE 1.5
//#define TONEMAP_LOWER_CURVE 1.0
//#define TONEMAP_UPPER_CURVE 1.5
//
//void BSLTonemap(inout float3 color){
//    color = TONEMAP_EXPOSURE * color;
//    color = color / pow(pow(color, float3(TONEMAP_WHITE_CURVE)) + 1.0, float3(1.0 / TONEMAP_WHITE_CURVE));
//    color = pow(color, lerp(float3(TONEMAP_LOWER_CURVE), float3(TONEMAP_UPPER_CURVE), sqrt(color)));
//}

// Original by Dawson Burgess
// Modified by: https://github.com/TechDevOnGithub/
float3 tonemap_Burgess_Modified(const in float3 color)
{
	const float3 max_color = color * min(1.0f, 1.0f - exp(-1.0f / (luminance(color) * 0.1f) * color));
    return max_color * (6.2f * max_color + 0.5f) / (max_color * (6.2f * max_color + 1.7f) + 0.06f);
}

// My custom tonemap, feel free to use, make sure to give credit though :D
float3 tonemap_Tech(const in float3 color)
{
    float3 a = color * min(1.0f, 1.0f - exp(-1.0f / 0.038f * color));
    a = lerp(a, color, color * color);
    return a / (a + 0.6f);
}

//float3 _LinearTosRGB(const in float3 x)
//{
//    if (step(x, 0.0031308f) == 1.0f) return x * 12.92f;
//    return pow(abs(x), 1.0f / 2.4f) * 1.055f - 0.055f;
//}
//
//float3 _SRGBToLinear(const in float3 x)
//{
//    if(step(x, 0.04045f) == 1.0f) return x / 12.92f;
//    return pow((x + 0.055f) / 1.055f, 2.4f);
//}
