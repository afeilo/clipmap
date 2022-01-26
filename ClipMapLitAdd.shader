Shader "Unlit/ClipMapLitAdd"
{
    Properties
    {
        _HeightMapArr ("HeightMap Array", 2DArray) = "" {}
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
        blend One One
        Pass
        {
            HLSLPROGRAM
            #define TERRAIN_SPLAT_ADDPASS
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
    }
}
