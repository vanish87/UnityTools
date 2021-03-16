

struct Node
{
    bool active;
    int index;
    float3 pos;
    float4 color;
};

struct Edge
{
    bool active;
    int from;
    int to;
};

struct EdgeToAdd
{
    int from;
    int to;
    float3 fromPos;
    float3 toPos;
};
