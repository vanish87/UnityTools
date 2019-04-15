using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Sequential, Size = 48)]
public struct ParticleData
{
    public BlittableBool active;
    public Vector3 position;

    public Vector3 velocity;
    public float life;

    public Color color;
}
public class GPUParticleStruct : GPUParticleStructBase<ParticleData>
{
    protected override void OnResetParticlesData()
    {
        for(var i = 0; i< this.CPUData.Length; ++i)
        {
            //this.CPUData[i].color = Color.red;
        }
    }
}
