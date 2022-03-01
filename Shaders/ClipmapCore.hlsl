#ifndef CLIPMAP_CORE
#define CLIPMAP_CORE 
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 texcoord : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};



struct Varyings
{
    float3 uv                       : TEXCOORD0;
    float4 uvSplat01                : TEXCOORD1; // xy: splat0, zw: splat1
    float4 uvSplat23                : TEXCOORD2; // xy: splat2, zw: splat3
#if defined(_NORMALMAP)
    float4 normal                   : TEXCOORD3;    // xyz: normal, w: viewDir.x
    float4 tangent                  : TEXCOORD4;    // xyz: tangent, w: viewDir.y
    float4 bitangent                : TEXCOORD5;    // xyz: bitangent, w: viewDir.z
#else
    float3 normal                   : TEXCOORD3;
    float3 viewDir                  : TEXCOORD4;
    half3 vertexSH                  : TEXCOORD5; // SH
#endif
    half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light
    float3 positionWS               : TEXCOORD7;
    float4 positionCS : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

UNITY_INSTANCING_BUFFER_START(Props)
UNITY_DEFINE_INSTANCED_PROP(float4, CLIP_DIM)
#define CLIP_DIM UNITY_ACCESS_INSTANCED_PROP(Props, CLIP_DIM)
UNITY_INSTANCING_BUFFER_END(Props)

Texture2DArray _HeightMapArr;
SAMPLER(sampler_HeightMapArr);

Texture2DArray _SplitMapArr;
SAMPLER(sampler_SplitMapArr);

Texture2DArray _NormalMapArr;
SAMPLER(sampler_NormalMapArr);

TEXTURE2D(_Splat0);     SAMPLER(sampler_Splat0);
TEXTURE2D(_Splat1);
TEXTURE2D(_Splat2);
TEXTURE2D(_Splat3);

#ifdef _NORMALMAP
TEXTURE2D(_Normal0);     SAMPLER(sampler_Normal0);
TEXTURE2D(_Normal1);
TEXTURE2D(_Normal2);
TEXTURE2D(_Normal3);
#endif

#ifdef _MASKMAP
TEXTURE2D(_Mask0);      SAMPLER(sampler_Mask0);
TEXTURE2D(_Mask1);
TEXTURE2D(_Mask2);
TEXTURE2D(_Mask3);
#endif

CBUFFER_START(_Terrain)
half4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;
half4 _LayerHasMask;
CBUFFER_END


real UnpackHeightmap2(real4 height)
{
    return height.r * 257;
    //return (height.r + height.g * 256.0) / 257.0; // (255.0 * height.r + 255.0 * 256.0 * height.g) / 65535.0
}


void SplatmapMix(float4 uvSplat01, float4 uvSplat23, inout half4 splatControl, out half weight, out half4 mixedDiffuse, out half4 defaultSmoothness, inout half3 mixedNormal)
{
    half4 diffAlbedo[4];

    diffAlbedo[0] = SAMPLE_TEXTURE2D(_Splat0, sampler_Splat0, uvSplat01.xy);
    diffAlbedo[1] = SAMPLE_TEXTURE2D(_Splat1, sampler_Splat0, uvSplat01.zw);
    diffAlbedo[2] = SAMPLE_TEXTURE2D(_Splat2, sampler_Splat0, uvSplat23.xy);
    diffAlbedo[3] = SAMPLE_TEXTURE2D(_Splat3, sampler_Splat0, uvSplat23.zw);

    // This might be a bit of a gamble -- the assumption here is that if the diffuseMap has no
    // alpha channel, then diffAlbedo[n].a = 1.0 (and _DiffuseHasAlphaN = 0.0)
    // Prior to coming in, _SmoothnessN is actually set to max(_DiffuseHasAlphaN, _SmoothnessN)
    // This means that if we have an alpha channel, _SmoothnessN is locked to 1.0 and
    // otherwise, the true slider value is passed down and diffAlbedo[n].a == 1.0.
    defaultSmoothness = half4(diffAlbedo[0].a, diffAlbedo[1].a, diffAlbedo[2].a, diffAlbedo[3].a);

    // Now that splatControl has changed, we can compute the final weight and normalize
    weight = dot(splatControl, 1.0h);

#ifdef TERRAIN_SPLAT_ADDPASS
    clip(weight <= 0.005h ? -1.0h : 1.0h);
#endif

#ifndef _TERRAIN_BASEMAP_GEN
    // Normalize weights before lighting and restore weights in final modifier functions so that the overal
    // lighting result can be correctly weighted.
    splatControl /= (weight + HALF_MIN);
#endif

    mixedDiffuse = 0.0h;
    mixedDiffuse += diffAlbedo[0] * half4(splatControl.rrr, 1.0h);
    mixedDiffuse += diffAlbedo[1] * half4(splatControl.ggg, 1.0h);
    mixedDiffuse += diffAlbedo[2] * half4(splatControl.bbb, 1.0h);
    mixedDiffuse += diffAlbedo[3] * half4(splatControl.aaa, 1.0h);

#ifdef _NORMALMAP
    half3 nrm = 0.0f;
    nrm += splatControl.r * UnpackNormalScale(SAMPLE_TEXTURE2D(_Normal0, sampler_Normal0, uvSplat01.xy), 1);
    nrm += splatControl.g * UnpackNormalScale(SAMPLE_TEXTURE2D(_Normal1, sampler_Normal0, uvSplat01.zw), 1);
    nrm += splatControl.b * UnpackNormalScale(SAMPLE_TEXTURE2D(_Normal2, sampler_Normal0, uvSplat23.xy), 1);
    nrm += splatControl.a * UnpackNormalScale(SAMPLE_TEXTURE2D(_Normal3, sampler_Normal0, uvSplat23.zw), 1);

    // avoid risk of NaN when normalizing.
#if HAS_HALF
    nrm.z += 0.01h;
#else
    nrm.z += 1e-5f;
#endif

    mixedNormal = normalize(nrm.xyz);
#endif
}

void ComputeMasks(out half4 masks[4], half4 hasMask, Varyings v)
{
    masks[0] = 0.5h;
    masks[1] = 0.5h;
    masks[2] = 0.5h;
    masks[3] = 0.5h;

#ifdef _MASKMAP
    masks[0] = lerp(masks[0], SAMPLE_TEXTURE2D(_Mask0, sampler_Mask0, v.uvSplat01.xy), hasMask.x);
    masks[1] = lerp(masks[1], SAMPLE_TEXTURE2D(_Mask1, sampler_Mask0, v.uvSplat01.zw), hasMask.y);
    masks[2] = lerp(masks[2], SAMPLE_TEXTURE2D(_Mask2, sampler_Mask0, v.uvSplat23.xy), hasMask.z);
    masks[3] = lerp(masks[3], SAMPLE_TEXTURE2D(_Mask3, sampler_Mask0, v.uvSplat23.zw), hasMask.w);
#endif

}
void InitializeInputData(Varyings i, half3 normalTS, out InputData input)
{
    input = (InputData)0;

    input.positionWS = i.positionWS;
    half3 SH = half3(0, 0, 0);

#if defined(_NORMALMAP) && !defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
    half3 viewDirWS = half3(i.normal.w, i.tangent.w, i.bitangent.w);
    input.normalWS = TransformTangentToWorld(normalTS, half3x3(-i.tangent.xyz, i.bitangent.xyz, i.normal.xyz));
    SH = SampleSH(input.normalWS.xyz);
#elif defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
    half3 viewDirWS = i.viewDir;
    float2 sampleCoords = (i.uvMainAndLM.xy / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
    half3 normalWS = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
    half3 tangentWS = cross(GetObjectToWorldMatrix()._13_23_33, normalWS);
    input.normalWS = TransformTangentToWorld(normalTS, half3x3(-tangentWS, cross(normalWS, tangentWS), normalWS));
    SH = SampleSH(input.normalWS.xyz);
#else
    half3 viewDirWS = i.viewDir;
    input.normalWS = i.normal;
    SH = i.vertexSH;
#endif

#if SHADER_HINT_NICE_QUALITY
    viewDirWS = SafeNormalize(viewDirWS);
#endif

    input.normalWS = NormalizeNormalPerPixel(input.normalWS);

    input.viewDirectionWS = viewDirWS;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    input.shadowCoord = i.shadowCoord;
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    input.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
#else
    input.shadowCoord = float4(0, 0, 0, 0);
#endif

    input.fogCoord = i.fogFactorAndVertexLight.x;
    input.vertexLighting = i.fogFactorAndVertexLight.yzw;

    input.bakedGI = SAMPLE_GI(i.uvMainAndLM.zw, SH, input.normalWS);
}

// morphs vertex xy from from high to low detailed mesh position
float3 morphVertex( float3 inPos, float3 vertex, float morphLerpK, float2 g_quadScale)
{
   float2 fracPart = (frac( inPos.xz * 0.5) * 2) * g_quadScale.xy;
   vertex.xz -= fracPart * morphLerpK;
   return vertex;
}

float3 CameraPosition;

Varyings vert(Attributes v)
{
    Varyings o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    float3 positionWS = TransformObjectToWorld(v.positionOS);
    float3 cameraWS = CameraPosition;//GetCameraPositionWS();
    half d = length(half3(positionWS.x - cameraWS.x, positionWS.y - cameraWS.y, positionWS.z - cameraWS.z));

    half morphLerpK = clamp((d - CLIP_DIM.z - 0.667 * CLIP_DIM.z) / (CLIP_DIM.z - 0.667 * CLIP_DIM.z), 0.0, 1.0);
    positionWS = morphVertex((v.positionOS), positionWS, morphLerpK, float2(CLIP_DIM.y, CLIP_DIM.y));

    int _ox = (int)(positionWS.x / 512);
    int _oz = (int)(positionWS.z / 512);
    float x = positionWS.x - _ox * 512.0f + 512.0f;
    float z = positionWS.z - _oz * 512.0f + 512.0f;

    float _x = (x % 512.0f);
    float _z = (z % 512.0f);
    
    int ox = (int)((positionWS.x + 1024) / 512);
    int oz = (int)((positionWS.z + 1024) / 512);
    //int ox = (int)x / 512;
    //int oz = (int)z / 512;

    int index = (3 - ox) * 4 + oz;
    float2 uv = float2((_x / 512.0f), (_z / 512.0f));

    uv *= (256.0 / 257.0);
    uv += (1.0 / 514.0);
    float4 color = SAMPLE_TEXTURE2D_ARRAY_LOD(_HeightMapArr, sampler_HeightMapArr, uv, index, 0);
    float height = UnpackHeightmap2(color);
    positionWS.y = height;
    o.positionCS = TransformWorldToHClip(positionWS);

    half3 SH = half3(0, 0, 0);

    half3 viewDirWS = CameraPosition - positionWS;
#if !SHADER_HINT_NICE_QUALITY
    viewDirWS = SafeNormalize(viewDirWS);
#endif

    v.normalOS = SAMPLE_TEXTURE2D_ARRAY_LOD(_NormalMapArr, sampler_NormalMapArr, uv, index, 0).rgb * 2 - 1;// _TerrainNormalmapTexture.Load(int3(sampleCoords, 0)).rgb * 2 - 1;
#if defined(_NORMALMAP) 
    float4 vertexTangent = float4(cross(float3(0, 0, 1), v.normalOS), 1.0);
    VertexNormalInputs normalInput = GetVertexNormalInputs(v.normalOS, vertexTangent);

    o.normal = half4(v.normalOS.rgb, viewDirWS.x);
    o.tangent = half4(normalInput.tangentWS, viewDirWS.y);
    o.bitangent = half4(normalInput.bitangentWS, viewDirWS.z);
#else
    o.normal = TransformObjectToWorldNormal(v.normalOS);
    o.viewDir = viewDirWS;
    o.vertexSH = SampleSH(o.normal);
#endif
    o.fogFactorAndVertexLight.x = ComputeFogFactor(o.positionCS.z);
    o.fogFactorAndVertexLight.yzw = VertexLighting(o.positionWS, o.normal.xyz);

    o.positionWS = positionWS;
   

    return o;
}

half4 frag(Varyings i) : SV_Target
{
    int _ox = (int)(i.positionWS.x / 512);
    int _oz = (int)(i.positionWS.z / 512);
    float x = i.positionWS.x - _ox * 512.0f + 512.0f;
    float z = i.positionWS.z - _oz * 512.0f + 512.0f;

    float _x = (x % 512.0f);
    float _z = (z % 512.0f);

    int ox = (int)((i.positionWS.x + 1024) / 512);
    int oz = (int)((i.positionWS.z + 1024) / 512);

    int index = (3 - ox) * 4 + oz;
    float2 uv = float2((_x / 512.0f), (_z / 512.0f));
    float4 uvSplat01;
    float4 uvSplat23;
    uvSplat01.xy = TRANSFORM_TEX(uv, _Splat0);
    uvSplat01.zw = TRANSFORM_TEX(uv, _Splat1);
    uvSplat23.xy = TRANSFORM_TEX(uv, _Splat2);
    uvSplat23.zw = TRANSFORM_TEX(uv, _Splat3);

    //float2 uv = i.uv.xy;
    uv *= (256.0 / 257.0);
    uv += (1.0 / 514.0);
    half4 splatControl = SAMPLE_TEXTURE2D_ARRAY(_SplitMapArr, sampler_SplitMapArr, uv, index);
    half alpha = dot(splatControl, 1.0h);
    half3 normalTS = half3(0.0h, 0.0h, 1.0h);
#ifdef TERRAIN_SPLAT_BASEPASS
   /* half3 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uvMainAndLM.xy).rgb;
    half smoothness = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uvMainAndLM.xy).a;
    half metallic = SAMPLE_TEXTURE2D(_MetallicTex, sampler_MetallicTex, i.uvMainAndLM.xy).r;
    half alpha = 1;
    half occlusion = 1;*/
#else

    half4 hasMask = _LayerHasMask;//half4(_LayerHasMask0, _LayerHasMask1, _LayerHasMask2, _LayerHasMask3);
    half4 masks[4];
    ComputeMasks(masks, hasMask, i);

    half weight;
    half4 mixedDiffuse;
    half4 defaultSmoothness;
    SplatmapMix(uvSplat01, uvSplat23, splatControl, weight, mixedDiffuse, defaultSmoothness, normalTS);
    half3 albedo = mixedDiffuse.rgb;
    half4 defaultMetallic = 0;
    half4 defaultOcclusion = 1;

    half4 maskSmoothness = half4(masks[0].a, masks[1].a, masks[2].a, masks[3].a);
    defaultSmoothness = lerp(defaultSmoothness, maskSmoothness, hasMask);
    half smoothness = dot(splatControl, defaultSmoothness);
    half4 maskMetallic = half4(masks[0].r, masks[1].r, masks[2].r, masks[3].r);
    defaultMetallic = lerp(defaultMetallic, maskMetallic, hasMask);
    half metallic = dot(splatControl, defaultMetallic);

    half4 maskOcclusion = half4(masks[0].g, masks[1].g, masks[2].g, masks[3].g);
    defaultOcclusion = lerp(defaultOcclusion, maskOcclusion, hasMask);
    half occlusion = dot(splatControl, defaultOcclusion);
#endif
    //return half4(normalTS,1);
    InputData inputData;
    InitializeInputData(i, normalTS, inputData);

    half4 color = UniversalFragmentPBR(inputData, albedo, metallic, /* specular */ half3(0.0h, 0.0h, 0.0h), smoothness, occlusion, /* emission */ half3(0, 0, 0), alpha);
    color.rgb *= color.a;
    //return half4(color.a, 0, 0, 1);
    // sample the texture
    // apply fog
    //col.rgb = MixFog(col.rgb, i.uv.z);
    return color;
}



struct AttributesLean
{
    float4 position     : POSITION;
    float3 normalOS       : NORMAL;
#ifdef _ALPHATEST_ON
    float2 texcoord     : TEXCOORD0;
#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VaryingsLean
{
    float4 clipPos      : SV_POSITION;
#ifdef _ALPHATEST_ON
    float2 texcoord     : TEXCOORD0;
#endif
    UNITY_VERTEX_OUTPUT_STEREO
};
//
//VaryingsLean ShadowPassVertex(AttributesLean v)
//{
//    VaryingsLean o = (VaryingsLean)0;
//    UNITY_SETUP_INSTANCE_ID(v);
//    TerrainInstancing(v.position, v.normalOS);
//
//    float3 positionWS = TransformObjectToWorld(v.position.xyz);
//    float3 normalWS = TransformObjectToWorldNormal(v.normalOS);
//
//    float4 clipPos = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
//
//#if UNITY_REVERSED_Z
//    clipPos.z = min(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
//#else
//    clipPos.z = max(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
//#endif
//
//    o.clipPos = clipPos;
//
//#ifdef _ALPHATEST_ON
//    o.texcoord = v.texcoord;
//#endif
//
//    return o;
//}
//
//half4 ShadowPassFragment(VaryingsLean IN) : SV_TARGET
//{
//#ifdef _ALPHATEST_ON
//    ClipHoles(IN.texcoord);
//#endif
//    return 0;
//}
#endif