

struct Node
{
    bool active;
    int index;
    int sid;
    float3 pos;
    float4 color;

    float3 predictPos;
    float3 restPos;

    float4 rotation;
    float4 predictRotation;

    float3 w;
    float3 velocity;
    float a;
    float b;
    float c;

    float density;
};

struct Edge
{
    bool active;
    int from;
    int to;
};

struct EdgeToAdd
{
    int sid;
    int from;
    int to;
    float3 fromPos;
    float3 toPos;
};

