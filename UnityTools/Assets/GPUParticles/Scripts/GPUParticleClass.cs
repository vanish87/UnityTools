
using System.Runtime.InteropServices;
using UnityEngine;


[StructLayout(LayoutKind.Sequential, Size = 48)]
public class ParticleDataClass : AlignedGPUData
{
    public bool active;
    public Vector3 position;

    public Vector3 velocity;
    public float life;

    public Color color;
}

public class GPUParticleClass : GPUParticleClassBase<ParticleDataClass>
{
    protected override void OnResetParticlesData()
    {
        base.OnResetParticlesData();

        //use a kernal to init data
    }
}


