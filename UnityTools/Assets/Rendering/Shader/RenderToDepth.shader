Shader "UnityTools/Graphics/RenderToDepthNormal"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}

	CGINCLUDE
	#include "UnityCG.cginc"
	#include "Common.cginc"

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

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		return o;
	}

	void HandleNormal(inout float3 normal, float4x4 mat, bool packToColor)
	{
		normal = mul(mat, normal);
		normal = normalize(normal);
		if (packToColor)
		{
			normal = (normal + 1) * 0.5f;
		}
	}

	sampler2D _MainTex;
	sampler2D _CameraDepthTexture;
	sampler2D _CameraDepthNormalsTexture;

	float _PackToColor;
	float4x4 _ViewToWorldMat;

	float4 fragDepth(v2f i) : SV_Target
	{
		float depth = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.uv));

		depth = Linear01DepthWithOrtho(depth);

		return float4(depth.xxx,1);
	}

	float4 fragNormal(v2f i) : SV_Target
	{
		float4 depthnormal = tex2D(_CameraDepthNormalsTexture, i.uv);

		float3 normal;
		float depth;
		DecodeDepthNormal(depthnormal, depth, normal);

		HandleNormal(normal, _ViewToWorldMat, _PackToColor > 0);

		return float4(normal,1);
	}

	float4 fragDepthNormal(v2f i) : SV_Target
	{
		float4 depthnormal = tex2D(_CameraDepthNormalsTexture, i.uv);
		return depthnormal;
	}
	ENDCG

	SubShader
	{ 
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragDepth			
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragNormal
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment fragDepthNormal
			ENDCG
		}
	}
	FallBack "Diffuse"
}
