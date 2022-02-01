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
            void Unity_Checkerboard_float(float2 UV, float3 ColorA, float3 ColorB, float2 Frequency, out float3 Out)
            {
                UV = (UV.xy + 0.5) * Frequency;
                float4 derivatives = float4(ddx(UV), ddy(UV));
                float2 duv_length = sqrt(float2(dot(derivatives.xz, derivatives.xz), dot(derivatives.yw, derivatives.yw)));
                float width = 1.0;
                float2 distance3 = 4.0 * abs(frac(UV + 0.25) - 0.5) - width;
                float2 scale = 0.35 / duv_length.xy;
                float freqLimiter = sqrt(clamp(1.1f - max(duv_length.x, duv_length.y), 0.0, 1.0));
                float2 vector_alpha = clamp(distance3 * scale.xy, -1.0, 1.0);
                float alpha = saturate(0.5f + 0.5f * vector_alpha.x * vector_alpha.y * freqLimiter);
                Out = lerp(ColorA, ColorB, alpha.xxx);
            }

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
