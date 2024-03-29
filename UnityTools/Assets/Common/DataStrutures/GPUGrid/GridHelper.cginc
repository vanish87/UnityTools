float3 _GridMin;
float3 _GridMax;
float3 _GridSize;
float3 _GridSpacing;

#define LOOP_RANGE(I, J, K, CURRENT, RANGE, SIZE) \
for(int I = max(CURRENT.x - RANGE.x, 0); I <= min(CURRENT.x + RANGE.x, SIZE.x-1); ++I)\
for(int J = max(CURRENT.y - RANGE.y, 0); J <= min(CURRENT.y + RANGE.y, SIZE.y-1); ++J)\
for(int K = max(CURRENT.z - RANGE.z, 0); K <= min(CURRENT.z + RANGE.z, SIZE.z-1); ++K)

float3 PosToCellPos(float3 pos, float3 gridMin, float3 gridMax, float3 gridSpacing)
{
	pos = clamp(pos, gridMin, gridMax);
	pos = pos - gridMin; //Start from grid left bottom corner
	return pos/gridSpacing;
}

uint3 CellPosToCellIndex(float3 pos)
{
	return (uint3)pos;
}
uint CellIndexToCellID(int3 index, int3 gridSize)
{
	return index.x + index.y * gridSize.x + index.z * gridSize.x * gridSize.y;
}

uint2 PosToGridIndexPair(uint pid, float3 pos, float3 gridMin,float3 gridMax, int3 gridSize, float3 gridSpacing)
{
	float3 cellPos = PosToCellPos(pos, gridMin, gridMax, gridSpacing);
	uint3 cellIndex = CellPosToCellIndex(cellPos);
	uint cellID = CellIndexToCellID(cellIndex, gridSize);

	return uint2(cellID, pid);
}
