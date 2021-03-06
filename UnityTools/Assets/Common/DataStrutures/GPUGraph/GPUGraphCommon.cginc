#define THREAD [numthreads(128,1,1)]
#define THREAD512 [numthreads(512,1,1)]
#define THREAD1024 [numthreads(1024,1,1)]
#define THREAD_ONE [numthreads(1,1,1)]

static const float PI = 3.141592653;
static const float3 GRAVITY = float3(0, -9.8, 0);

int _NodeCount;
int _EdgeCount;
RWStructuredBuffer<Node> _NodeBuffer;
RWStructuredBuffer<Edge> _EdgeBuffer;

AppendStructuredBuffer<int> _NodeIndexBuffer;
AppendStructuredBuffer<int> _EdgeIndexBuffer;
ConsumeStructuredBuffer<int> _NodeIndexBufferConsume;
ConsumeStructuredBuffer<int> _EdgeIndexBufferConsume;


THREAD
void InitNode (uint3 idx : SV_DispatchThreadID)
{
    int id = idx.x;
    _NodeBuffer[id] = (Node)0;
    _NodeIndexBuffer.Append(id);
}

THREAD
void InitEdge (uint3 idx : SV_DispatchThreadID)
{
    int id = idx.x;
    _EdgeBuffer[id] = (Edge)0;
    _EdgeIndexBuffer.Append(id);
}

void ConnectNode(int from, int to)
{
    int eid = _EdgeIndexBufferConsume.Consume();

    Edge e = _EdgeBuffer[eid];
    e.from = from;
    e.to = to;
    e.active = true;

    _EdgeBuffer[eid] = e;
}

int CreateNode()
{
    int id = _NodeIndexBufferConsume.Consume();
    _NodeBuffer[id].active = true;
    return id;
}