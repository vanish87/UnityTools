Shader "Camera/CameraComposite"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }


	CGINCLUDE
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

	sampler2D _MainTex;
	sampler2D _TargetTex;
	float4 _ST;

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		//o.uv.y = 1 - v.uv.y;
		return o;
	}

	fixed4 fragComposite(v2f i) : SV_Target
	{
		fixed4 col = fixed4(i.uv, 0, 1);

		if (i.uv.x < _ST.x || i.uv.x > _ST.x + _ST.z || i.uv.y < _ST.y || i.uv.y >_ST.y + _ST.w)
		{
			return fixed4(0, 0, 0, 0);
		}
		else
		{
			return tex2D(_MainTex, (i.uv - _ST.xy) / _ST.zw);
		}
	}
	
	fixed4 fragAddTarget(v2f i) : SV_Target
	{
		return tex2D(_TargetTex, i.uv) + tex2D(_MainTex, i.uv);
	}
	ENDCG

    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
		Blend One One

        Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragComposite			
			ENDCG
		}
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragAddTarget			
			ENDCG
		}
    }
}
