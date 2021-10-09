Shader "Hidden/StereoBlitSimulationComposite"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RightTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Name "StereoBlitSimulationComposite"
        Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
        ColorMask RGB -1
        ZTest Always
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                float4 pos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0));
                o.vertex = mul(UNITY_MATRIX_VP, pos);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _RightTex;

            float4 frag (v2f i) : SV_Target
            {
                if(unity_StereoEyeIndex == 0) return tex2D(_MainTex, i.uv);
                else return tex2D(_RightTex, i.uv);
            }
            ENDCG
        }
    }
}
