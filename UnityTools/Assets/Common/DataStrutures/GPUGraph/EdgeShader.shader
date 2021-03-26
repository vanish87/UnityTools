Shader "Unlit/EdgeShader"
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
        float4 color : TEXCOORD1;
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

    float _NodeScale;
    float _LineScale;


    StructuredBuffer<Node> _NodeBuffer;
    StructuredBuffer<Edge> _EdgeBuffer;

	v2f vert(appdata i, uint iid : SV_InstanceID) 
	{
        v2f o;
        UNITY_SETUP_INSTANCE_ID(i);
        UNITY_TRANSFER_INSTANCE_ID(i, o);

        Edge e = _EdgeBuffer[iid];
        Node from = _NodeBuffer[e.from];
        Node to = _NodeBuffer[e.to];
        float3 epos = (from.pos+to.pos)*0.5f;
        // float3 pos = lerp(from.pos, to.pos, i.vid);
        float3 dir = normalize(to.pos-from.pos);
        float2x2 rotation = float2x2(dir.y, -dir.x, dir.x, dir.y);
        float2x2 scale = float2x2(0.02f, 0,0,distance(from.pos,to.pos)) * _LineScale;
        float2 pos =  mul(mul(scale, i.vertex), rotation);
        float4 vertex = float4(pos + epos, 0, 1);
        o.position = UnityObjectToClipPos(vertex);

        o.color = e.active;
        return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
        return 1;
	}

	ENDCG

    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
		// Blend One One
		//Blend One OneMinusSrcAlpha
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
