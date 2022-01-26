// Crest Ocean System

// Copyright 2020 Wave Harmonic Ltd

Shader "Crest/Inputs/Flow/Add Flow Map"
{
	Properties
	{
		_FlowMap("Flow Map", 2D) = "white" {}
		_Strength( "Strength", float ) = 1
	}

	SubShader
	{
		Tags { "Queue" = "Transparent" }
		Blend One One

		Pass
		{
			CGPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

			#include "UnityCG.cginc"

			sampler2D _FlowMap;
			float4 _FlowMap_ST;

			float _Strength;

			struct Attributes
			{
				float3 positionOS : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			Varyings Vert(Attributes input)
			{
				Varyings o;
				o.positionCS = UnityObjectToClipPos(input.positionOS);
				o.uv = TRANSFORM_TEX(input.uv, _FlowMap);
				return o;
			}

			float2 Frag(Varyings input) : SV_Target
			{
				return (tex2D(_FlowMap, input.uv).xy - 0.5) * _Strength;
			}

			ENDCG
		}
	}
}
