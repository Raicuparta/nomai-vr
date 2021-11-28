//======= Copyright (c) Valve Corporation, All rights reserved. ===============
// UNITY_SHADER_NO_UPGRADE
Shader "Custom/SteamVR_Fade_WorldSpace"
{
	SubShader
	{
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest Always
			Cull Off
			ZWrite Off

			CGPROGRAM
				#pragma vertex MainVS
				#pragma fragment MainPS
				
			// Includes -------------------------------------------------------------------------------------------------------------------------------------------------
			#include "UnityCG.cginc"

			// Structs --------------------------------------------------------------------------------------------------------------------------------------------------
			struct VertexInput
			{
				float4 vertex : POSITION;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 vertex : SV_POSITION;

				UNITY_VERTEX_OUTPUT_STEREO
			};
			
				// Globals --------------------------------------------------------------------------------------------------------------------------------------------------
				UNITY_INSTANCING_BUFFER_START( Props )
					UNITY_DEFINE_INSTANCED_PROP( float4, fadeColor )
				UNITY_INSTANCING_BUFFER_END( Props )

				VertexOutput MainVS( VertexInput i )
				{
					VertexOutput o;
					UNITY_SETUP_INSTANCE_ID( i );
					UNITY_INITIALIZE_OUTPUT( VertexOutput, o );
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o ); 

					o.vertex = UnityObjectToClipPos(i.vertex);

					return o;
				}

				float4 MainPS( VertexOutput i ) : SV_Target
				{
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( i );

					return UNITY_ACCESS_INSTANCED_PROP( Props, fadeColor.rgba );
				}
			ENDCG
		}
	}
}