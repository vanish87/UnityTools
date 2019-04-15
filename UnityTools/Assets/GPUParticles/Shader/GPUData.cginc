
//aligned by float4(16 bytes)
struct ParticleData
{
    bool active;
    float3 position;
    
    float3 velocty;
    float life;
    
    float4 color;
};
