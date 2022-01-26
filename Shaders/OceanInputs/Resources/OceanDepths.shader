// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

// Renders ocean depth - signed distance from sea level to sea floor
Shader "Crest/Inputs/Depth/Ocean Depth From Geometry"
{
	SubShader
	{
		Pass
		{
			BlendOp Min

			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

			CBUFFER_START(CrestPerOceanInput)
			float4 _LD_Params_0;
			float4 _LD_Params_1;
			float3 _LD_Pos_Scale_0;
			float3 _LD_Pos_Scale_1;
			float3 _GeomData;
			float3 _OceanCenterPosWorld;
			CBUFFER_END

			#include "UnityCG.cginc"

			struct Attributes
			{
				float3 positionOS : POSITION;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float depth : TEXCOORD0;
			};

			Varyings Vert(Attributes input)
			{
				Varyings o;
				o.positionCS = UnityObjectToClipPos(input.positionOS);

				float altitude = mul(unity_ObjectToWorld, float4(input.positionOS, 1.0)).y;

				o.depth = _OceanCenterPosWorld.y - altitude;

				return o;
			}

			float Frag(Varyings input) : SV_Target
			{
				return input.depth;
			}
			ENDCG
		}
	}
}
