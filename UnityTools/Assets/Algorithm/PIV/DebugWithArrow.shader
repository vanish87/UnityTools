Shader "Hidden/Debug with Arrow"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}

	CGINCLUDE
	#include "UnityCG.cginc"

	sampler2D _MainTex;

	static const float2 _ArrowTileSize       = float2(0.05, 0.05);
	static const float  _ArrowShaftThickness = 0.0015;
	static const float  _ArrowHeadLength     = 0.0075;
	static const float  _ArrowHeadAngle      = 0.75;
	static const float4 _ArrowColor          = float4(1.0, 1.0, 1.0, 1.0);

	float2 arrowTileCenterCoord(float2 uv)
	{
		return (floor(uv / _ArrowTileSize) + 0.5) * _ArrowTileSize.xy;
	}

	float arrow(float2 p, float2 v)
	{
		p -= arrowTileCenterCoord(p);

		float magV = length(v);
		float magP = length(p);

		if (magV > 0.0)
		{
			float2 dirP = p / magP;
			float2 dirV = v / magV;

			magV = clamp(magV, 0.001, _ArrowTileSize * 0.5);

			v = dirV * magV;

			float dist = 0.0;
			dist = max
			(
				// Shaft
				_ArrowShaftThickness * 0.25 -
				max(abs(dot(p, float2(dirV.y, -dirV.x))), // Width
					abs(dot(p, dirV)) - magV + _ArrowHeadLength * 0.5), // Length

																		// Arrow head
				min(0.0, dot(v - p, dirV) - cos(_ArrowHeadAngle * 0.5) * length(v - p)) * 2.0 + // Front sides
				min(0.0, dot(p, dirV) + _ArrowHeadLength - magV) //Back
			);

			return clamp(1.0 + dist * 512.0, 0.0, 1.0);
		}
		else
		{
			return max(0.0, 1.2 - magP);
		}
		return 0;
	}


	fixed4 frag_0(v2f_img i) : SV_Target
	{
		fixed4 col = tex2D(_MainTex, i.uv);
		return fixed4(col.rgb, length(col.rgb));
	}

	fixed4 frag_1(v2f_img i) : SV_Target
	{
		fixed4 col = tex2D(_MainTex, i.uv);
		fixed alpha = length(col.rgb);
		col.rgb *= 0.5;
		col.rgb += 0.5;
		return fixed4(col.rgb, alpha);
	}

	fixed4 frag_2(v2f_img i) : SV_Target
	{
		float4 mainColor = tex2D(_MainTex, i.uv.xy);
		float alpha = length(mainColor.rgb);
		mainColor.rgb *= 0.5;
		mainColor.rgb += 0.5;
		float arrowStrength = arrow(i.uv.xy, 2.0 * (+1e-8 + tex2D(_MainTex, arrowTileCenterCoord(i.uv.xy))) * _ArrowTileSize.xy * 0.5);
		float4 col = lerp(mainColor * alpha, _ArrowColor, arrowStrength);
		return col;
		return fixed4(col.rgb, alpha);
	}
	ENDCG

	SubShader
	{
		// No culling or depth
		// Cull Off ZWrite Off ZTest Always
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }

        // inside Pass
        // ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        // Blend One OneMinusSrcAlpha
		
		// // Pass 0: 
		// Pass
		// {
		// 	CGPROGRAM
		// 	#pragma vertex   vert_img
		// 	#pragma fragment frag_0
		// 	ENDCG
		// }
		
		// Pass 1:
		Pass
		{
			CGPROGRAM
			#pragma vertex   vert_img
			#pragma fragment frag_1
			ENDCG
		}

		// Pass 2:
		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag_2
			ENDCG
		}
	}
}
