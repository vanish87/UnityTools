Shader "Unlit/NodeShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }


	CGINCLUDE
	#include "UnityCG.cginc"
	#include "IndexGPUGraphData.cginc"

	struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
        uint vid : SV_VertexID;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct v2f
    {
        float4 position : SV_POSITION;
		float2 uv : TEXCOORD0;
        float4 color : COLOR;
        UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	sampler2D _MainTex;
	float4 _ST;

    float _NodeScale;
    float _LineScale;

    StructuredBuffer<Node> _NodeBuffer;
    StructuredBuffer<Edge> _EdgeBuffer;

	v2f vert(appdata i, uint iid : SV_InstanceID) 
	{
        v2f o = (v2f)0;
        UNITY_SETUP_INSTANCE_ID(i);
        UNITY_TRANSFER_INSTANCE_ID(i, o);

        Node node = _NodeBuffer[iid];
        float4 wp = float4(i.vertex.xyz * _NodeScale + node.pos,1) * node.active;
        o.position = UnityObjectToClipPos(wp);
        o.color = float4(node.color.xyz,node.active);
        return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
        return i.color;
	}

	ENDCG

    SubShader
    {
        // No culling or depth
        // Cull On ZWrite On ZTest Always
        // ZWrite On ZTest Always
		// Blend One One
		// Blend One OneMinusSrcAlpha
        Blend SrcAlpha OneMinusSrcAlpha
    
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
    }
}
