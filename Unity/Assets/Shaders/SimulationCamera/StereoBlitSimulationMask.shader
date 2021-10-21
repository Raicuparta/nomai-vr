Shader "Hidden/StereoBlitSimulationMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RightTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Name "StereoBlitSimulationMask"
        ColorMask 0 -1
        ZTest Always
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


            float frag (v2f i) : SV_Depth
            {
                fixed4 col;
                if(unity_StereoEyeIndex == 0) col = tex2D(_MainTex, i.uv);
                else col = tex2D(_RightTex, i.uv);
                col.x = col.w - 1.0;
                bool thr = col.x < 0.0;
                if(((int(thr) * int(0xffffffffu)))!=0){discard;}
                return 1.0f;
            }
            ENDCG
        }
    }
}
