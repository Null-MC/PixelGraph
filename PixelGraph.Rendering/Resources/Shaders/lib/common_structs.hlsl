#define LIGHTS 8

#define WATER_SURFACE 0
#define WATER_PUDDLES 1
#define WATER_FULL 2

#pragma pack_matrix(row_major)


struct vs_input
{
	float4 pos : POSITION;
	float3 nor : NORMAL;
	float3 tan : TANGENT;
	float3 bin : BINORMAL;
	float2 tex : TEXCOORD0;
	float4 mr0 : TEXCOORD1;
	float4 mr1 : TEXCOORD2;
	float4 mr2 : TEXCOORD3;
	float4 mr3 : TEXCOORD4;
    float4 col : COLOR;
};

struct vs_input_ex
{
	float4 pos : POSITION;
	float3 nor : NORMAL;
	float3 tan : TANGENT;
	float3 bin : BINORMAL;
	float2 tex : TEXCOORD0;
	float2 tex_min : TEXCOORD1;
	float2 tex_max : TEXCOORD2;
    float4 col : COLOR;
	//float4 mr0 : TEXCOORD1;
	//float4 mr1 : TEXCOORD2;
	//float4 mr2 : TEXCOORD3;
	//float4 mr3 : TEXCOORD4;
};

struct ps_input
{
	float4 pos  : SV_POSITION;
	float4 wp   : POSITION0;
	float3 eye  : POSITION1;
	float3 vT   : POSITION2;
	float2 vTS  : POSITION3;
	//float2 rTS  : POSITION4;
	float  pDepth : POSITION4;
    float3 nor  : NORMAL;
	float3 tan  : TANGENT;
	float3 bin  : BINORMAL;
	float2 tex  : TEXCOORD0;
	float2 tex_min : TEXCOORD1;
	float2 tex_max : TEXCOORD2;
};

struct ps_input_cube
{
	float4 pos     : SV_POSITION;
	float3 tex     : POSITION0;
};

struct ps_shadow
{
    //float4 pos : SV_POSITION;
	float2 tex : TEXCOORD0;
	float2 poT : POSITION1;
    float3 nor : NORMAL;
};

struct LightStruct
{
    int iLightType;
    float3 paddingL;
    float4 vLightDir; // the light direction is here the vector which looks towards the light
    float4 vLightPos;
    float4 vLightAtt;
    float4 vLightSpot; // outer angle , inner angle, falloff, free
    float4 vLightColor;
    matrix mLightView;
    matrix mLightProj;
};

struct MeshOutlinePS_INPUT
{
    float4 Pos : SV_POSITION;
    noperspective
    float2 Tex : TEXCOORD0;
};

cbuffer cbTransforms : register(b0)
{
    float4x4 mView;
    float4x4 mProjection;
    float4x4 mViewProjection;
    float4 vFrustum; // [fov,asepct-ratio,near,far]
    float4 vViewport; // [w,h,1/w,1/h]
    float4 vResolution; // [x, y, w, h]
    float3 vEyePos;
    bool SSAOEnabled;
    float SSAOBias;
    float SSAOIntensity;
    float TimeStamp; // by seconds
    bool IsPerspective;
    float OITPower;
    float OITSlope;
    int OITWeightMode;
    float DpiScale;
};

#ifdef MESH
cbuffer cbMesh : register(b1) 
{
    float4x4 mWorld;
    bool bInvertNormal = false;
    bool bHasInstances = false;
    bool bHasInstanceParams = false;
    bool bHasBones = false;
    float4 vParams = 0.0f;
    float4 vColor = 1.0f;
    float4 wireframeColor;
    bool3 bParams;
    bool bBatched = false;
    float minTessDistance;
	float maxTessDistance;
	float minTessFactor;
	float maxTessFactor;
    float4 vMaterialDiffuse;
    float4 vMaterialAmbient;
    float4 vMaterialEmissive;
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
    bool bHasDisplacementMap;
    bool bRenderPBR;  
    bool bRenderFlat;
    float sMaterialShininess;
    float4 displacementMapScaleMask;
    float4 uvTransformR1;
    float4 uvTransformR2;
    float vertColorBlending;
    float3 padding4;
};
#endif

cbuffer cbMinecraftScene : register(b2)
{
	bool EnableAtmosphere = true;
	float3 SunDirection;
	float SunStrength;
    float TimeOfDay;
    float Wetness;
    //int WaterMode = WATER_SURFACE;
    //bool padding9;
    //float ParallaxDepth;
    //int ParallaxSamples;
    float ErpExposure;
    //int OpacityMode;
	//float Padding2;
};

#ifdef MESH
cbuffer cbMinecraftMesh : register(b4) 
{
    int BlendMode = 0;
    float3 TintColor = 1.0f;
	bool EnableLinearSampling = false;
    float ParallaxDepth;
    int ParallaxSamples;
	bool EnableSlopeNormals = false;
    int WaterMode = WATER_SURFACE;
    float SubSurfaceBlur = 0.33f;
};
#endif

cbuffer cbLights : register(b3)
{
    LightStruct Lights[LIGHTS];
    float4 vLightAmbient = float4(0.2f, 0.2f, 0.2f, 1.0f);
    int NumLights;
    bool bHasEnvironmentMap;
    int NumEnvironmentMapMipLevels;
    float padding;
};

cbuffer cbShadow : register(b5)
{
    float2 vShadowMapSize = float2(1024, 1024);
    bool bHasShadowMap = false;
    float paddingShadow0;
    float4 vShadowMapInfo = float4(0.005, 1.0, 0.5, 0.0);
    float4x4 vLightView;
    float4x4 vLightProjection;
};

//#ifdef SCREENQUAD
//cbuffer cbScreenQuad : register(b9)
//{
//    float4x4 mWorld;
//    float4 VertCoord[4];
//    float4 TextureCoord[4];
//};
//#endif

Texture2D tex_albedo_alpha : register(t0);
Texture2D tex_normal_height : register(t1);
Texture2D tex_rough_f0_occlusion : register(t2);
Texture2D tex_porosity_sss_emissive : register(t3);

Texture2D tex_diffuse_alpha : register(t0);
Texture2D tex_emissive : register(t1);

TextureCube<float3> tex_environment : register(t20);
Texture2D<float3> tex_equirectangular : register(t21);
TextureCube<float3> tex_irradiance : register(t22);
Texture2D<float2> tex_dielectric_brdf_lut : register(t23);
Texture2D<float> tex_shadow : register(t30);

SamplerState sampler_surface : register(s0);
SamplerState sampler_irradiance : register(s1);
SamplerState sampler_height : register(s2);
SamplerState sampler_environment : register(s4);
SamplerComparisonState sampler_shadow : register(s5);
SamplerState sampler_light : register(s6);
SamplerState sampler_brdf_lut : register(s7);
