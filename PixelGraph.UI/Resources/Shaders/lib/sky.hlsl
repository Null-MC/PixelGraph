#include "common_funcs.hlsl"

#define PI_2 (3.1415926535f * 2.0)
#define SAMPLES_NUMS 16

#pragma pack_matrix(row_major)


struct ScatteringParams
{
    float sunRadius;
	float sunRadiance;

	float mieG;
	float mieHeight;

	float rayleighHeight;

	float3 waveLambdaMie;
	float3 waveLambdaOzone;
	float3 waveLambdaRayleigh;

	float earthRadius;
	float earthAtmTopRadius;
	float3 earthCenter;
};

float2 ComputeRaySphereIntersection(const in float3 position, const in float3 dir, const in float3 center, const in float radius)
{
	const float3 origin = position - center;
	const float B = dot(origin, dir);
	const float C = dot(origin, origin) - radius * radius;
	float D = B * B - C;

	if (D < 0.0f) return -1.0f;

	D = sqrt(D);
	return float2(-B - D, -B + D);
}

float3 ComputeWaveLambdaRayleigh(const in float3 lambda)
{
	const float n = 1.0003f;
	const float N = 2.545E25f;
	const float pn = 0.035f;
	const float n2 = n * n;
	const float pi3 = PI * PI * PI;
	const float rayleighConst = (8.0f * pi3 * pow(n2 - 1.0f, 2.0f)) / (3.0f * N) * ((6.0f + 3.0f * pn) / (6.0f - 7.0f * pn));
	return rayleighConst / (lambda * lambda * lambda * lambda);
}

float ComputePhaseMie(const in float theta, const in float g)
{
	const float g2 = g * g;
	return (1.0f - g2) / pow(1.0f + g2 - 2.0f * g * saturate(theta), 1.5f) / (4.0f * PI);
}

float ComputePhaseRayleigh(const in float theta)
{
	const float theta2 = theta * theta;
	return (theta2 * 0.75f + 0.75f) / (4.0f * PI);
}

float ChapmanApproximation(const in float X, const in float h, const in float cosZenith)
{
	const float c = sqrt(X + h);
	const float c_exp_h = c * exp(-h);

	if (cosZenith >= 0.0f)
		return c_exp_h / (c * cosZenith + 1.0f);
	
	const float x0 = sqrt(1.0f - cosZenith * cosZenith) * (X + h);
	const float c0 = sqrt(x0);

	return 2.0f * c0 * exp(X - x0) - c_exp_h / (1.0f - c * cosZenith);
}

float GetOpticalDepthSchueler(const in float h, const in float H, const in float earthRadius, const in float cosZenith)
{
	return H * ChapmanApproximation(earthRadius / H, h / H, cosZenith);
}

float3 GetTransmittance(const in ScatteringParams setting, const in float3 L, const in float3 V)
{
	const float ch = GetOpticalDepthSchueler(L.y, setting.rayleighHeight, setting.earthRadius, V.y);
	return exp(-(setting.waveLambdaMie + setting.waveLambdaRayleigh) * ch);
}

float2 ComputeOpticalDepth(const in ScatteringParams setting, const in float3 samplePoint, const in float3 V, const in float3 L, const in float neg)
{
	const float rl = length(samplePoint);
	const float h = rl - setting.earthRadius;
	const float3 r = samplePoint / rl;

	const float cos_chi_sun = dot(r, L);
	const float cos_chi_ray = dot(r, V * neg);

	const float opticalDepthSun = GetOpticalDepthSchueler(h, setting.rayleighHeight, setting.earthRadius, cos_chi_sun);
	const float opticalDepthCamera = GetOpticalDepthSchueler(h, setting.rayleighHeight, setting.earthRadius, cos_chi_ray) * neg;

	return float2(opticalDepthSun, opticalDepthCamera);
}

void AerialPerspective(const in ScatteringParams setting, const in float3 start, const in float3 end, const in float3 V, const in float3 L, const in bool infinite, out float3 transmittance, out float3 insctrMie, out float3 insctrRayleigh)
{
	const float inf_neg = infinite ? 1.0f : -1.0f;

	const float3 sampleStep = (end - start) / float(SAMPLES_NUMS);
	float3 samplePoint = end - sampleStep;
	
	const float3 sampleLambda = setting.waveLambdaMie + setting.waveLambdaRayleigh + setting.waveLambdaOzone;

	const float sampleLength = length(sampleStep);

	float3 scattering = 0.0;
	float2 lastOpticalDepth = ComputeOpticalDepth(setting, end, V, L, inf_neg);

	[unroll]
	for (int i = 1; i < SAMPLES_NUMS; i++, samplePoint -= sampleStep) {
		const float2 opticalDepth = ComputeOpticalDepth(setting, samplePoint, V, L, inf_neg);

		const float3 segment_s = exp(-sampleLambda * (opticalDepth.x + lastOpticalDepth.x));
		const float3 segment_t = exp(-sampleLambda * (opticalDepth.y - lastOpticalDepth.y));
		
		transmittance *= segment_t;
		
		scattering = scattering * segment_t;
		scattering += exp(-(length(samplePoint) - setting.earthRadius) / setting.rayleighHeight) * segment_s;

		lastOpticalDepth = opticalDepth;
	}

	insctrMie = scattering * setting.waveLambdaMie * sampleLength;
	insctrRayleigh = scattering * setting.waveLambdaRayleigh * sampleLength;
}

float ComputeSkyboxChapman(const in ScatteringParams setting, in float3 eye, const in float3 V, const in float3 L, out float3 transmittance, out float3 insctrMie, out float3 insctrRayleigh)
{
	bool neg = true;

	float2 outerIntersections = ComputeRaySphereIntersection(eye, V, setting.earthCenter, setting.earthAtmTopRadius);
	
	if (outerIntersections.y < 0.0f) {
		transmittance = 0.0f;
		insctrMie = 0.0f;
		insctrRayleigh = 0.0f;
		return 0.0f;
	}
	
	const float2 innerIntersections = ComputeRaySphereIntersection(eye, V, setting.earthCenter, setting.earthRadius);
	
	if (innerIntersections.x > 0.0f) {
		neg = false;
		outerIntersections.y = innerIntersections.x;
	}

	eye -= setting.earthCenter;

	const float3 start = eye + V * max(0.0f, outerIntersections.x);
	const float3 end = eye + V * outerIntersections.y;

	AerialPerspective(setting, start, end, V, L, neg, transmittance, insctrMie, insctrRayleigh);

	const bool intersectionTest = innerIntersections.x < 0.0f && innerIntersections.y < 0.0f;
	return intersectionTest ? 1.0f : 0.0f;
}

float4 ComputeSkyInscattering(const in ScatteringParams setting, const in float3 eye, const in float3 V, const in float3 L)
{
	float3 insctrMie = 0.0f;
	float3 insctrRayleigh = 0.0f;
	float3 insctrOpticalLength = 1.0f;
	const float intersectionTest = ComputeSkyboxChapman(setting, eye, V, L, insctrOpticalLength, insctrMie, insctrRayleigh);

	const float phaseTheta = dot(V, L);
	const float phaseMie = ComputePhaseMie(phaseTheta, setting.mieG);
	const float phaseRayleigh = ComputePhaseRayleigh(phaseTheta);
	const float phaseNight = 1.0f - saturate(insctrOpticalLength.x * EPSILON);

	const float3 insctrTotalMie = insctrMie * phaseMie;
	const float3 insctrTotalRayleigh = insctrRayleigh * phaseRayleigh;

	float3 sky = (insctrTotalMie + insctrTotalRayleigh) * setting.sunRadiance;

	const float angle = saturate((1.0f - phaseTheta) * setting.sunRadius);
	const float cosAngle = max(cos(angle * PI * 0.5f), 0.0f);
	const float edge = (angle >= 0.9f) ? smoothstep(0.9f, 1.0f, angle) : 0.0f;
                         
	float3 limbDarkening = GetTransmittance(setting, -L, V);
	limbDarkening *= pow(cosAngle, float3(0.420f, 0.503f, 0.652f)) * lerp(1.0f, float3(1.2f, 0.9f, 0.5f), edge) * intersectionTest;

	sky += limbDarkening;

	return float4(sky, phaseNight * intersectionTest);
}

float noise(const in float2 uv)
{
	return frac(dot(sin(uv.xyx * uv.xyy * 1024.0f), float3(341896.483f, 891618.637f, 602649.7031f)));
}

float3 get_sky_color(const in float3 view, const in float3 light)
{
	ScatteringParams setting;
	setting.sunRadius = 800.0f;
	setting.sunRadiance = 20.0f;
	setting.mieG = 0.76f;
	setting.mieHeight = 1200.0f;
	setting.rayleighHeight = 8000.0f;
	setting.earthRadius = 6360000.0f;
	setting.earthAtmTopRadius = 6420000.0f;
	setting.earthCenter = float3(0.0f, -setting.earthRadius, 0.0f);
	setting.waveLambdaMie = 2e-7f;
    
    // wavelength with 680nm, 550nm, 450nm
    setting.waveLambdaRayleigh = ComputeWaveLambdaRayleigh(float3(680e-9f, 550e-9f, 450e-9f));
    
    // see https://www.shadertoy.com/view/MllBR2
	setting.waveLambdaOzone = float3(1.36820899679147f, 3.31405330400124f, 0.13601728252538f) * 0.6e-6f * 2.504f;

	const float3 eye = float3(0.0f, 1000.0f, 0.0f);
   	return ComputeSkyInscattering(setting, eye, view, light).rgb;
}
