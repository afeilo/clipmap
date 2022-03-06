Shader "Unlit/VTClipMapLit"
{
    Properties
    {
        _SplitMapArr ("_SplitMap Array", 2DArray) = "" {}
         _NormalMapArr("_NromalMap Array", 2DArray) = "" {}
        [HideInInspector] _Splat3("Layer 3 (A)", 2D) = "grey" {}
        [HideInInspector] _Splat2("Layer 2 (B)", 2D) = "grey" {}
        [HideInInspector] _Splat1("Layer 1 (G)", 2D) = "grey" {}
        [HideInInspector] _Splat0("Layer 0 (R)", 2D) = "grey" {}
        [HideInInspector] _Normal3("Normal 3 (A)", 2D) = "bump" {}
        [HideInInspector] _Normal2("Normal 2 (B)", 2D) = "bump" {}
        [HideInInspector] _Normal1("Normal 1 (G)", 2D) = "bump" {}
        [HideInInspector] _Normal0("Normal 0 (R)", 2D) = "bump" {}
        [HideInInspector] _Mask3("Mask 3 (A)", 2D) = "grey" {}
        [HideInInspector] _Mask2("Mask 2 (B)", 2D) = "grey" {}
        [HideInInspector] _Mask1("Mask 1 (G)", 2D) = "grey" {}
        [HideInInspector] _Mask0("Mask 0 (R)", 2D) = "grey" {}

        [HideInInspector]_LayerHasMask("LayerHasMask", vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        Pass
        {
            HLSLPROGRAM

            #define TERRAIN_VT
            #include "ClipmapCore.hlsl"
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _MASKMAP  
            
            ENDHLSL
        }
        //Pass
        //{
        //    Name "ShadowCaster"
        //    Tags{"LightMode" = "ShadowCaster"}

        //    ZWrite On

        //    HLSLPROGRAM
        //    // Required to compile gles 2.0 with standard srp library
        //    #pragma prefer_hlslcc gles
        //    #pragma exclude_renderers d3d11_9x
        //    #pragma target 2.0

        //    #pragma vertex ShadowPassVertex
        //    #pragma fragment ShadowPassFragment

        //    #pragma multi_compile_instancing
        //    ENDHLSL
        //}

        //Pass
        //{
        //    Name "DepthOnly"
        //    Tags{"LightMode" = "DepthOnly"}

        //    ZWrite On
        //    ColorMask 0

        //    HLSLPROGRAM
        //    // Required to compile gles 2.0 with standard srp library
        //    #pragma prefer_hlslcc gles
        //    #pragma exclude_renderers d3d11_9x
        //    #pragma target 2.0

        //    #pragma vertex DepthOnlyVertex
        //    #pragma fragment DepthOnlyFragment

        //    #pragma multi_compile_instancing
        //    #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap

        //    #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitInput.hlsl"
        //    #include "Packages/com.unity.render-pipelines.universal/Shaders/Terrain/TerrainLitPasses.hlsl"
        //    ENDHLSL
        //}

    }
}
