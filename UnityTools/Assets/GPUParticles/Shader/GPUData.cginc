
//aligned by float4(16 bytes)
struct ParticleData
{
    bool active;
    float3 position;
    
    float3 velocity;
    float life;
    
    float4 color;
};
