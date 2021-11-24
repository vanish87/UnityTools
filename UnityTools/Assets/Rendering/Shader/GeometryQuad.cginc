
	static const int QuadVertex = 4;
	static const float3 QuadPosition[QuadVertex] =
	{
		float3(-1, 1, 0),
		float3(1, 1, 0),
		float3(-1,-1, 0),
		float3(1,-1, 0),
	};
	static const float2 QuadTextureCoord[QuadVertex] =
	{
		float2(0, 0),
		float2(1, 0),
		float2(0, 1),
		float2(1, 1),
	};

    void AddQuad(in VertexIn vin, inout TriangleStream<VertexOut> outStream)
    {
		[unroll]
		for (int i = 0; i < QuadVertex; i++)
		{
			VertexOut o = (VertexOut)0;
			o.position = float4(QuadPosition[i] * vin.size + vin.pos, 1);
			o.texcoord = QuadTextureCoord[i];

			UpdateVertex(vin, o);
			outStream.Append(o);
		}

		outStream.RestartStrip();
    }