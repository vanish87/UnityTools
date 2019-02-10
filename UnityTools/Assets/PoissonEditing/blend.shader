Shader "Custom/blend"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

			sampler2D _MainTex;
			sampler2D _MaskTex;
			sampler2D _TargetTex;

			float4 _SizeOffset;

            fixed4 frag (v2f i) : SV_Target
            {
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 target = tex2D(_TargetTex, i.uv);
				fixed4 mask = tex2D(_MaskTex, i.uv);
                
				
				fixed3 result =  tex2D(_MainTex, i.uv + _SizeOffset.xy).rgb - tex2D(_MainTex, i.uv).rgb;
					   result += tex2D(_MainTex, i.uv - _SizeOffset.xy).rgb - tex2D(_MainTex, i.uv).rgb;
					   result += tex2D(_MainTex, i.uv + _SizeOffset.zw).rgb - tex2D(_MainTex, i.uv).rgb;
					   result += tex2D(_MainTex, i.uv - _SizeOffset.zw).rgb - tex2D(_MainTex, i.uv).rgb;

				if(mask.x > 0)
				{

				}
				else
				{
					result = col.rgb;
				}

				return fixed4(result * 10, 1);
			}
            ENDCG
        }
    }
}
