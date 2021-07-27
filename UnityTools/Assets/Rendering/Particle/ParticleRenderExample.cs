using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityTools.Common;

namespace UnityTools.Rendering
{
	public class ParticleRenderExample : MonoBehaviour, IParticleBuffer<ParticleRenderExample.Particle>
	{
		public struct Particle
		{
			public float3 pos;
			public float4 col;
		}

		public GPUBufferVariable<Particle> Buffer => this.particleBuffer;
        protected GPUBufferVariable<Particle> particleBuffer = new GPUBufferVariable<Particle>();


		protected void OnEnable()
        {
            this.particleBuffer.InitBuffer(1024, true);
            foreach(var i in Enumerable.Range(0, this.particleBuffer.Size)) 
            {
                this.particleBuffer.CPUData[i].pos = UnityEngine.Random.insideUnitSphere;
                this.particleBuffer.CPUData[i].col = 1;
            }
            this.particleBuffer.SetToGPUBuffer();
        }

        protected void OnDisable()
        {
            this.particleBuffer?.Release();
        }

	}
}
