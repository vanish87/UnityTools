

float3 checker(float2 uv, int2 size)
{
	float fmodResult = fmod(floor(size.x * uv.x) + floor(size.y * uv.y), 2.0);
	float fin = max(sign(fmodResult), 0.0);
	return float3(fin, fin, fin);
}