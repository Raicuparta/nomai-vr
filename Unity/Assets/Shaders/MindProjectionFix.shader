Shader "NomaiVR/Mind_Projection_Fix"
{
    //Moves projections from viewspace to clipspace coordinates
    Properties {
		[HideInInspector] _MainTex ("Texture", 2D) = "white" {}
		[NoScaleOffset] _EyeTex ("Eye Texture", 2D) = "white" {}
		[HideInInspector] _SlideTex ("Slide Texture", 2D) = "black" {}
		[NoScaleOffset] _VignetteTex ("Vignette Texture", 2D) = "clear" {}
		_NoiseTex1 ("Vignette Noise 1", 2D) = "grey" {}
		_NoiseTex2 ("Vignette Noise 2", 2D) = "grey" {}
		_NoiseStrength ("Noise Strength", Float) = 0.1
		_EyeOpenness ("Eye Openness", Float) = 1
		_BlendWidth ("Blend Width", Float) = 0.5
		_SlideFade ("Slide Fade Level", Float) = 0
	}
	SubShader
	{
		Pass
		{
			ZTest Always
			ZWrite Off
			Cull Off

			CGPROGRAM
				#pragma vertex MainVS
				#pragma fragment MainPS
				
			// Includes -------------------------------------------------------------------------------------------------------------------------------------------------
			#include "UnityCG.cginc"

			// Structs --------------------------------------------------------------------------------------------------------------------------------------------------
			struct VertexInput
			{
				float4 vertex : POSITION;
                float2 uv : TEXCOORD;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD;

				UNITY_VERTEX_OUTPUT_STEREO
			};
				// Globals --------------------------------------------------------------------------------------------------------------------------------------------------
				UNITY_INSTANCING_BUFFER_START( Props )
					UNITY_DEFINE_INSTANCED_PROP( float4, _NoiseTex1_ST )
					UNITY_DEFINE_INSTANCED_PROP( float4, _NoiseTex2_ST )
					UNITY_DEFINE_INSTANCED_PROP( float4, _NoiseStrength )
					UNITY_DEFINE_INSTANCED_PROP( float, _EyeOpenness )
					UNITY_DEFINE_INSTANCED_PROP( float, _BlendWidth )
					UNITY_DEFINE_INSTANCED_PROP( float, _SlideFade )
					UNITY_DEFINE_INSTANCED_PROP( float, _UnscaledTime )
				UNITY_INSTANCING_BUFFER_END( Props )

				sampler2D _MainTex;
				sampler2D _EyeTex;
				sampler2D _SlideTex;
				sampler2D _NoiseTex1;
				sampler2D _NoiseTex2;
				sampler2D _VignetteTex;

				VertexOutput MainVS( VertexInput i )
				{
					VertexOutput o;
					UNITY_SETUP_INSTANCE_ID( i );
					UNITY_INITIALIZE_OUTPUT( VertexOutput, o );
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o ); 

					o.vertex = UnityObjectToClipPos(i.vertex);
                    o.uv = i.uv;

					return o;
				}

				float4 MainPS( VertexOutput i ) : SV_Target
				{
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( i );
                    float4 Noise1_ST = UNITY_ACCESS_INSTANCED_PROP( Props, _NoiseTex1_ST.xyzw );
                    float4 Noise2_ST = UNITY_ACCESS_INSTANCED_PROP( Props, _NoiseTex2_ST.xyzw );
					float time = UNITY_ACCESS_INSTANCED_PROP( Props, _UnscaledTime);
					float blendWidth = UNITY_ACCESS_INSTANCED_PROP( Props, _BlendWidth );
					float noiseStrength = UNITY_ACCESS_INSTANCED_PROP( Props, _NoiseStrength );

                    float2 noiseUV = Noise1_ST.zw * time;
                    noiseUV =  i.uv * Noise1_ST.xy + noiseUV;
                    float4 noise1 = tex2D(_NoiseTex1, noiseUV);
                    noise1.x = noise1.x * 2 - 1;
                    float2 noise2UV = Noise2_ST.zw * time;
                    noise2UV.xy = i.uv * Noise2_ST.xy + noise2UV.xy;
                    float4 noise2 = tex2D(_NoiseTex2, noise2UV.xy);
                    float temp2 = noise2.x * 2 - 1;
                    noise1.x = temp2 + noise1.x;
                    noise1.x = noise1.x * noiseStrength;
                    bool flip = 0.5 < i.uv.x;
                    temp2 = (flip) ? 1 : -1;
                    float temp = noise1.x * temp2 + i.uv.x;
                    float4 slideTex = tex2D(_SlideTex, i.uv);
                    float4 vignette = tex2D(_VignetteTex, float2(temp, i.uv.y));
                    float oneMinusVignette = clamp(1 - vignette.a, 0, 1);
                    slideTex = slideTex * oneMinusVignette + float4(0, 0, 0, -1);
					slideTex = slideTex * UNITY_ACCESS_INSTANCED_PROP( Props, _SlideFade ) + float4(0, 0, 0, 1);
                    float mainTex = tex2D(_MainTex, i.uv);
                    mainTex = vignette - slideTex;
                    float eyeCloseLevel = 1 - UNITY_ACCESS_INSTANCED_PROP( Props, _EyeOpenness );
                    eyeCloseLevel = eyeCloseLevel * (blendWidth + 1) - blendWidth;
                    float4 eyeTex = tex2D(_EyeTex, i.uv);
                    eyeCloseLevel = eyeTex.x - eyeCloseLevel;
                    eyeCloseLevel = eyeCloseLevel / blendWidth;
                    eyeCloseLevel = clamp(eyeCloseLevel, 0.0, 1.0);
                    return eyeCloseLevel * mainTex + slideTex;
				}
			ENDCG
		}
	}
}