using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Mathematics;
using System.Linq;
using UnityTools.Rendering;
using UnityTools.ComputeShaderTool;

namespace UnityTools.Common
{
	public class ParticleGrid : ObjectGrid<ParticleGrid.Particle, uint2>, IDataBuffer<ParticleGrid.Particle>
	{
        [StructLayout(LayoutKind.Sequential)]
        public struct Particle
        {
            public float3 pos;
            public float4 col;
        }
        public GPUBufferVariable<Particle> Buffer => this.parameter.particleBuffer;
        [SerializeField] protected int numOfParticles = 1024 * 128;

        [System.Serializable]
		public class ParticleGPUParameter : GPUContainer
        {
			[Shader(Name = "_TargetPos")] public float4 targetPos;

			[Shader(Name = "_ParticleBuffer")]public GPUBufferVariable<Particle> particleBuffer = new GPUBufferVariable<Particle>();
			[Shader(Name = "_ParticleBufferSorted")] public GPUBufferVariable<Particle> particleBufferSorted = new GPUBufferVariable<Particle>();

        }

        [SerializeField] protected ParticleGPUParameter parameter = new ParticleGPUParameter();
        [SerializeField] protected ComputeShader particleCS;

        protected ComputeShaderDispatcher particleDispather;

        protected void OnEnable()
        {
            this.Init(this.gridData.gridSize);
            this.parameter.particleBuffer.InitBuffer(this.numOfParticles, true, false);
			foreach (var i in Enumerable.Range(0, this.parameter.particleBuffer.Size))
			{
				var rand = new float3(UnityEngine.Random.value,UnityEngine.Random.value,UnityEngine.Random.value);
				rand -= 0.5f;
				this.parameter.particleBuffer.CPUData[i].pos = this.space.TRS.MultiplyPoint(rand);
				this.parameter.particleBuffer.CPUData[i].col = 1;
			}
			this.parameter.particleBuffer.SetToGPUBuffer(true);

            this.particleDispather = new ComputeShaderDispatcher(this.particleCS);
            this.particleDispather.AddParameter("ResetColor", this.parameter);
			this.particleDispather.AddParameter("UpdateColor", this.parameter);
			this.particleDispather.AddParameter("UpdateColor", this.gridData);
        }
        protected void OnDisable()
        {
            this.Deinit();
            this.parameter?.Release();
        }

        protected void Update()
        {
            GPUBufferVariable<Particle> sorted;
			this.BuildSortedParticleGridIndex(this.parameter.particleBuffer, out sorted);
            this.parameter.particleBufferSorted.UpdateBuffer(sorted);

            this.particleDispather.Dispatch("ResetColor", this.parameter.particleBufferSorted.Size);
            this.particleDispather.Dispatch("UpdateColor", 1);
        }

	}
}
