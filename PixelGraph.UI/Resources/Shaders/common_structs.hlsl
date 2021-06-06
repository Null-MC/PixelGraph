#pragma pack_matrix( row_major )


cbuffer cbTransforms : register(b0)
{
    float4x4 mView;
    float4x4 mProjection;
    float4x4 mViewProjection;
    float4 vFrustum;
    float4 vViewport;
    float3 vEyePos;
    bool SSAOEnabled;
    float SSAOBias;
    float SSAOIntensity;
    float TimeStamp;
    bool IsPerspective;
    float OITPower;
    float OITSlope;
    int OITWeightMode;
    float DpiScale;
};

cbuffer cbMesh : register(b1) 
{
    float4x4 mWorld;
    bool bInvertNormal = false;
    bool bHasInstances = false;
    bool bHasInstanceParams = false;
    bool bHasBones = false;
    float4 vParams = float4(0, 0, 0, 0);
    float4 vColor = float4(1, 1, 1, 1);
    float4 wireframeColor;
    bool3 bParams;
    bool bBatched = false;

	float minTessDistance = 1;
	float maxTessDistance = 100;
	float minTessFactor = 4;
	float maxTessFactor = 1;

    float4 vMaterialDiffuse = 0.5f;
    float4 vMaterialAmbient = 0.25f;
    float4 vMaterialEmissive = 0.0f;
    float ConstantAO;
    float ConstantRoughness;
    float ConstantMetallic;
    float ConstantReflectance;
    float ClearCoat;
    float ClearCoatRoughness;
    float padding1;
    bool bHasAOMap;
    bool bHasDiffuseMap = false;
    bool bHasNormalMap = false;
    bool bHasCubeMap = false;
    bool bRenderShadowMap = false;
    bool bHasEmissiveMap = false;
    bool bHasRMMap;    
    bool bHasIrradianceMap; 
    bool bAutoTengent;
    bool bHasDisplacementMap = false;
    bool bRenderPBR = false;  
    bool bRenderFlat = false;
    float sMaterialShininess = 1.0f;

    float4 displacementMapScaleMask = float4(0, 0, 0, 1);
    float4 uvTransformR1;
    float4 uvTransformR2;
    float vertColorBlending;
    float3 padding4;
};

struct vs_input
{
	float4 p : POSITION;
	float3 n : NORMAL;
	float3 tan : TANGENT;
	float3 bin : BINORMAL;
	float2 tex : TEXCOORD;
    float4 c : COLOR;
	float4 mr0 : TEXCOORD1;
	float4 mr1 : TEXCOORD2;
	float4 mr2 : TEXCOORD3;
	float4 mr3 : TEXCOORD4;
};

struct ps_input
{
	float4 pos : SV_POSITION;
	float4 wp  : POSITION0;
	float4 eye  : POSITION1;
    float3 nor : NORMAL;
	float3 tan : TANGENT;
	float3 bin : BINORMAL;
	float2 tex : TEXCOORD0;
};

Texture2D tex_albedo_alpha : register(t0);
Texture2D tex_normal_height : register(t1);
Texture2D tex_rough_f0_occlusion : register(t2);
Texture2D tex_porosity_sss_emissive : register(t3);
Texture2D<float> tex_shadow : register(t30);

SamplerState sampler_surface : register(s0);
SamplerState sampler_cube : register(s4);

SamplerComparisonState sampler_shadow : register(s5);
