#ifndef GPUBOIDSDATA_BASE_INCLUDED
#define GPUBOIDSDATA_BASE_INCLUDED
// 群れデータ(MBSBoids Fish Data)
struct BoidData
{
	float3 position;
	float3 velocity;
	float4 rotation;
	float3 up;
	float4 color;		// 表示時の色
	float4 bodyColor;	// 体色
	float animeTime;
	float speed;
	float offsetLimit;
	float size;
	float colorChangePower;  // 人の色に染まる速度(強さ)
	float hueShiftPower;     // fluidTexから取ってくる色相のズレ幅(0～1)
};

#endif // GPUBOIDSDATA_BASE_INCLUDED