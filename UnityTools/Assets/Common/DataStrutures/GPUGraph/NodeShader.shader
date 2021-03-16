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
        float4 color : TEXCOORD1;
        UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	sampler2D _MainTex;
	float4 _ST;


    StructuredBuffer<Node> _Nodes;
    StructuredBuffer<Edge> _Edges;

	v2f vert(appdata i, uint iid : SV_InstanceID) 
	{
        v2f o;
        UNITY_SETUP_INSTANCE_ID(i);
        UNITY_TRANSFER_INSTANCE_ID(i, o);

        Node node = _Nodes[iid];
        float4 wp = float4(node.pos,1);
        o.position = UnityObjectToClipPos(i.vertex + wp);
        o.color = 1;
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
        // Cull Off ZWrite Off ZTest Always
		// Blend One One
		//Blend One OneMinusSrcAlpha
    
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
    }
}
