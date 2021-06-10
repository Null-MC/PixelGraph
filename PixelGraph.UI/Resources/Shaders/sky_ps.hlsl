#include "lib/common_structs.hlsl"
#include "lib/common_funcs.hlsl"

//#define Rayleigh 1
#define RayleighAtt 1
//#define Mie 1
#define MieAtt 1.2
//#define DistanceAtt 0.00001
//#define SC 250

#pragma pack_matrix(row_major)


static const float3 Rayleigh = float3(5.5e-4, 13.0e-4, 22.4e-4);
static const float Mie = 21e-4;
static const float3 _betaR = float3(0.0195, 0.11, 0.294);
static const float3 _betaM = float3(0.04, 0.04, 0.04);


float3 calcAtmosphericScattering(float sR, float sM, out float3 extinction, float cosine, float g1)
{
    extinction = exp(-(_betaR * sR + _betaM * sM));

    float g2 = g1 * g1;
    float fcos2 = cosine * cosine;
    float miePhase = Mie * pow(1 + g2 + 2 * g1 * cosine, -1.5) * (1 - g2) / (2 + g2);

    return (1 + fcos2) * (Rayleigh + _betaM / _betaR * miePhase);
}

float4 main(const in ps_input_cube input) : SV_TARGET
{
    float3 rd = normalize(input.tex);

    float sundot = saturate(dot(rd, -SunDirection));
	//return float4(sundot, sundot, sundot, 1);

    // optical depth -> zenithAngle
    float zenithAngle = max(0, rd.y); //abs( rd.y);
    float sR = RayleighAtt / zenithAngle;
    float sM = MieAtt / zenithAngle;

    float3 extinction;
    float3 inScatter = calcAtmosphericScattering(sR, sM, extinction, sundot, -0.93);
	
    float3 col = inScatter * (1 - extinction);
	
    // sun
    col += 0.47 * float3(1.6, 1.4, 1.0) * pow(sundot, 350) * extinction;
	
    // sun haze
    col += 0.4 * float3(0.8, 0.9, 1.0) * pow(sundot, 2) * extinction;
    
    // clouds
	//float2 sc = ro.xz + rd.xz * (SC * 1000 - ro.y) / rd.y;
	//col += 2 * float3(1.0, 0.95, 1.0) * extinction * smoothstep(0.5, 0.8, fbm(0.0005 * sc / SC));
    
	col = ACESFilm(col);
    col = linear_to_srgb(col);
    
	return float4(col, 1);
}
