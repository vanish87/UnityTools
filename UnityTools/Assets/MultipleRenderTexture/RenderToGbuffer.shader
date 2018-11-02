Shader "Custom/RenderToGbuffer"
{
	CGINCLUDE
	#include "UnityCG.cginc"
	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float3 posWS : POSITION1;
		float3 normalWS :NORMAL1;
	};

	struct PixelOutput
	{
		float4 col0 : SV_Target0;
		float4 col1 : SV_Target1;
	};

	v2f vert(appdata_full v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;
		o.posWS = mul(unity_ObjectToWorld, v.vertex);
		o.normalWS = mul(unity_ObjectToWorld, v.normal);
		return o;
	}

	PixelOutput fragGBuffer(v2f pixelData)
	{
		PixelOutput o;
		o.col0 = float4(pixelData.posWS, 1.0f);
		o.col1 = float4(pixelData.normalWS, 1.0f);
		return o;
	}
	ENDCG

	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragGBuffer
			ENDCG
		}
	}
}
