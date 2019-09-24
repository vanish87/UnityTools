Shader "Rendering/PostEffectBase"
{
    Properties
    {
        [HideInInspector]
        _MainTex("Texture", 2D) = "white" {}
    }

	CGINCLUDE
	#include "UnityCG.cginc"

	sampler2D _MainTex;

	fixed4 frag(v2f_img input) : SV_Target
	{
		float4 color = tex2D(_MainTex, input.uv);
		color.rgb = 1 - color.rgb;

		return color;
	}

	ENDCG

	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag    
            ENDCG
        }
    }
}
