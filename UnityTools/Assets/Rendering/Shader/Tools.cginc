
void Unity_Checkerboard_float(float2 UV, float3 ColorA, float3 ColorB, float2 Frequency, out float3 Out)
{
	UV = (UV.xy + 0.5) * Frequency;
	float4 derivatives = float4(ddx(UV), ddy(UV));
	float2 duv_length = sqrt(float2(dot(derivatives.xz, derivatives.xz), dot(derivatives.yw, derivatives.yw)));
	float width = 1.0;
	float2 distance3 = 4.0 * abs(frac(UV + 0.25) - 0.5) - width;
	float2 scale = 0.35 / duv_length.xy;
	float freqLimiter = sqrt(clamp(1.1f - max(duv_length.x, duv_length.y), 0.0, 1.0));
	float2 vector_alpha = clamp(distance3 * scale.xy, -1.0, 1.0);
	float alpha = saturate(0.5f + 0.5f * vector_alpha.x * vector_alpha.y * freqLimiter);
	Out = lerp(ColorA, ColorB, alpha.xxx);
}

float3 checker(float2 uv, int2 size)
{
	float fmodResult = fmod(floor(size.x * uv.x) + floor(size.y * uv.y), 2.0);
	float fin = max(sign(fmodResult), 0.0);
	return float3(fin, fin, fin);
}