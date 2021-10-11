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
			Blend SrcAlpha OneMinusSrcAlpha

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
			};

			struct VertexOutput
			{
				float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD;
			};
				// Globals --------------------------------------------------------------------------------------------------------------------------------------------------

				float _NoiseStrength;
				float _EyeOpenness;
				float _BlendWidth;
				float _SlideFade;
				float _UnscaledTime;

				sampler2D _MainTex;
				sampler2D _EyeTex;
				sampler2D _SlideTex;
				sampler2D _NoiseTex1;
				sampler2D _NoiseTex2;
				sampler2D _VignetteTex;
				
				float4 _NoiseTex1_ST;
				float4 _NoiseTex2_ST;

				VertexOutput MainVS( VertexInput i )
				{
					VertexOutput o;
					UNITY_INITIALIZE_OUTPUT( VertexOutput, o );

					o.vertex = UnityObjectToClipPos(i.vertex);
                    o.uv = i.uv;

					return o;
				}

				float4 MainPS( VertexOutput i ) : SV_Target
				{
                    float2 noiseUV =  i.uv * _NoiseTex1_ST.xy + _NoiseTex1_ST.zw * _UnscaledTime;
                    float4 noise1 = tex2D(_NoiseTex1, noiseUV);
                    noise1.x = noise1.x * 2.0 - 1.0;
                    float2 noise2UV = i.uv * _NoiseTex2_ST.xy + _NoiseTex2_ST.zw * _UnscaledTime;
                    float4 noise2 = tex2D(_NoiseTex2, noise2UV);
                    noise1.x += noise2.x * 2.0 - 1.0;
                    noise1.x *= _NoiseStrength;
                    bool flip = 0.5 < i.uv.x;
                    float temp2 = (flip) ? 1.0 : -1.0;
                    float temp = noise1.x * temp2 + i.uv.x;
                    float4 slideTex = tex2D(_SlideTex, i.uv);
                    float4 vignette = tex2D(_VignetteTex, float2(temp, i.uv.y));
                    float oneMinusVignette = clamp(1.0 - vignette.a, 0.0, 1.0);
                    slideTex = slideTex * oneMinusVignette + float4(0.0, 0.0, 0.0, -1.0);
					slideTex = slideTex * _SlideFade + float4(0.0, 0.0, 0.0, 1.0);
                    float eyeCloseLevel = 1.0 - _EyeOpenness;
                    eyeCloseLevel = eyeCloseLevel * (_BlendWidth + 1) - _BlendWidth;
                    float4 eyeTex = tex2D(_EyeTex, i.uv);
                    eyeCloseLevel = (eyeTex.x - eyeCloseLevel) / _BlendWidth;
                    eyeCloseLevel = clamp(eyeCloseLevel, 0.0, 1.0);
                    return eyeCloseLevel * (-slideTex) + slideTex;
				}
			ENDCG
		}
	}
}