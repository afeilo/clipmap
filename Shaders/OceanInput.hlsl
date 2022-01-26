// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

#ifndef CREST_OCEAN_INPUT_INCLUDED
#define CREST_OCEAN_INPUT_INCLUDED

#include "OceanConstants.hlsl"

/////////////////////////////
// Samplers
#if defined(TEXTURE2D)
TEXTURE2D(_CameraDepthTexture);
SAMPLER(sampler_CameraDepthTexture);
#else
UNITY_DECLARE_TEX2D(_CameraDepthTexture);
#endif

#if defined(TEXTURE2D)
TEXTURE2D(_CameraOpaqueTexture);
SAMPLER(sampler_CameraOpaqueTexture);
#else
UNITY_DECLARE_TEX2D(_CameraOpaqueTexture);
#endif

sampler2D _Normals;
sampler2D _ReflectionTex;
//sampler2D _ReflectionCubemapOverride;
sampler2D _FoamTexture;
sampler2D _CausticsTexture;

Texture2DArray _LD_TexArray_AnimatedWaves;
Texture2DArray _LD_TexArray_WaveBuffer;
Texture2DArray _LD_TexArray_SeaFloorDepth;
Texture2DArray _LD_TexArray_ClipSurface;
Texture2DArray _LD_TexArray_Foam;
Texture2DArray _LD_TexArray_Flow;
Texture2DArray _LD_TexArray_DynamicWaves;
Texture2DArray _LD_TexArray_Shadow;

// These are used in lods where we operate on data from
// previously calculated lods. Used in simulations and
// shadowing for example.
Texture2DArray _LD_TexArray_AnimatedWaves_Source;
Texture2DArray _LD_TexArray_WaveBuffer_Source;
Texture2DArray _LD_TexArray_SeaFloorDepth_Source;
Texture2DArray _LD_TexArray_ClipSurface_Source;
Texture2DArray _LD_TexArray_Foam_Source;
Texture2DArray _LD_TexArray_Flow_Source;
Texture2DArray _LD_TexArray_DynamicWaves_Source;
Texture2DArray _LD_TexArray_Shadow_Source;

SamplerState LODData_linear_clamp_sampler;


/////////////////////////////
// Constant buffer: CrestPerMaterial

CBUFFER_START(CrestInputsPerMaterial)
float _CrestTime;
float3 _OceanCenterPosWorld;

half3 _Diffuse;
half3 _DiffuseGrazing;

half _RefractionStrength;
half4 _DepthFogDensity;

half3 _SubSurfaceColour;
half _SubSurfaceBase;
half _SubSurfaceSun;
half _SubSurfaceSunFallOff;
half _SubSurfaceHeightMax;
half _SubSurfaceHeightPower;
half3 _SubSurfaceCrestColour;

half _SubSurfaceDepthMax;
half _SubSurfaceDepthPower;
half3 _SubSurfaceShallowCol;
half3 _SubSurfaceShallowColShadow;

half _CausticsTextureScale;
half _CausticsTextureAverage;
half _CausticsStrength;
half _CausticsFocalDepth;
half _CausticsDepthOfField;
half _CausticsDistortionScale;
half _CausticsDistortionStrength;

half3 _DiffuseShadow;

half _NormalsStrength;
half _NormalsScale;

half3 _SkyBase, _SkyAwayFromSun, _SkyTowardsSun;
half _SkyDirectionality;

half _Specular;
half _Smoothness;
half _SmoothnessFar;
half _SmoothnessFarDistance;
half _SmoothnessPower;
half _ReflectionBlur;
half _FresnelPower;
half _LightIntensityMultiplier;
float  _RefractiveIndexOfAir;
float  _RefractiveIndexOfWater;
half _PlanarReflectionNormalsStrength;
half _PlanarReflectionIntensity;

half _FoamScale;
float4 _FoamTexture_TexelSize;
half4 _FoamWhiteColor;
half4 _FoamBubbleColor;
half _FoamBubbleParallax;
half _ShorelineFoamMinDepth;
half _WaveFoamFeather;
half _WaveFoamBubblesCoverage;
half _WaveFoamNormalStrength;
half _WaveFoamSpecularFallOff;
half _WaveFoamSpecularBoost;
half _WaveFoamLightScale;
half2 _WindDirXZ;

// Hack - due to SV_IsFrontFace occasionally coming through as true for backfaces,
// add a param here that forces ocean to be in undrwater state. I think the root
// cause here might be imprecision or numerical issues at ocean tile boundaries, although
// i'm not sure why cracks are not visible in this case.
float _ForceUnderwater;

float _HeightOffset;

// Settings._jitterDiameterSoft, Settings._jitterDiameterHard, Settings._currentFrameWeightSoft, Settings._currentFrameWeightHard
float4 _JitterDiameters_CurrentFrameWeights;

float3 _CenterPos;
float3 _Scale;
float _LD_SliceIndex_Source;
float4x4 _MainCameraProjectionMatrix;

float _FoamFadeRate;
float _WaveFoamStrength;
float _WaveFoamCoverage;
float _ShorelineFoamMaxDepth;
float _ShorelineFoamStrength;
float _SimDeltaTime;
float _SimDeltaTimePrev;
float _LODChange;

half _Damping;
float2 _LaplacianAxisX;
half _Gravity;
CBUFFER_END


/////////////////////////////
// Constant buffer: CrestInputsPerObject
CBUFFER_START(CrestInputsPerObject)
// MeshScaleLerp, FarNormalsWeight, LODIndex (debug)
float3 _InstanceData;

// Geometry data
// x: Grid size of lod data - size of lod data texel in world space.
// y: Grid size of geometry - distance between verts in mesh.
// zw: normalScrollSpeed0, normalScrollSpeed1
float4 _GeomData;

// Create two sets of LOD data, which have overloaded meaning depending on use:
// * the ocean surface geometry always lerps from a more detailed LOD (0) to a less detailed LOD (1)
// * simulations (persistent lod data) read last frame's data from slot 0, and any current frame data from slot 1
// * any other use that does not fall into the previous categories can use either slot and generally use slot 0

// _LD_Params: float4(world texel size, texture resolution, shape weight multiplier, 1 / texture resolution)
float4 _LD_Params[MAX_LOD_COUNT + 1];
float3 _LD_Pos_Scale[MAX_LOD_COUNT + 1];
float _LD_SliceIndex;
float4 _LD_Params_Source[MAX_LOD_COUNT + 1];
float3 _LD_Pos_Scale_Source[MAX_LOD_COUNT + 1];

float _GridSize;
float _TexelsPerWave;
float _MaxWavelength;
float _ViewerAltitudeLevelAlpha;
float _SliceCount;
CBUFFER_END

#endif // OCEAN_INPUT_INCLUDED
