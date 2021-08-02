
//normally it has same num as Grid Buffer Data
int _HashTableSize;
int3 _GridSize;
float3 _GridSpacing;

//Not working TODO

static const int P1 = 73856093;
static const int P2 = 19349663;
static const int P3 = 83492791;

int PosToHash(float3 pos, float3 gridSpacing)
{
	int3 cell = (int3)(pos+gridSpacing*0.5f);
	cell *= int3(P1, P2, P3);
	return (cell.x ^ cell.y ^ cell.z) % _HashTableSize;
}

int3 PosToCellIndex(float3 pos, float3 gridSpacing)
{
	return 0;
}

int2 PosToHashPair(float3 pos, uint pid, float3 gridSpacing)
{
	return int2(PosToHash(pos, gridSpacing), pid);
}

uint CellIndexToCellID(int3 index, int3 gridSize)
{
	return index.x + index.y * gridSize.x + index.z * gridSize.x * gridSize.y;
}

