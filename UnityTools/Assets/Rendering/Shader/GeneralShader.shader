Shader "UnityTools/GeneralShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {

        Blend [BlendSrc] [BlendDst]
        BlendOp [BlendOp]
        ColorMask [ColorMask]
        Cull [CullMode]
        ZClip [ZClip]
        ZTest [ZTest]
        ZWrite [ZWrite]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "Tools.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            int CheckBoard; 

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                // col.a = length(col.rgb);
                if(CheckBoard) Unity_Checkerboard_float(i.uv, 1, 0, CheckBoard, col.rgb);
                return col;
            }
            ENDCG
        }
    }
}
