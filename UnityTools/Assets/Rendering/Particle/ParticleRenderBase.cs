using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Common;
using UnityTools.Debuging;

namespace UnityTools.Rendering
{
	public interface IParticleBuffer<T>
	{
		GPUBufferVariable<T> Buffer { get; }
	}

	public class ParticleRenderBase<T> : MonoBehaviour
	{
		protected IParticleBuffer<T> buffer;

		protected DisposableMaterial particleMaterial;
		[SerializeField] protected Shader particleShader;
		[SerializeField] protected Mesh particleMesh;

		public class RenderData : GPUContainer
		{
			public GPUBufferIndirectArgument particleIndirectBuffer = new GPUBufferIndirectArgument();
		}

		protected RenderData data = new RenderData();

		public void Start()
		{
			if(this.buffer == null)
			{
				this.buffer = this.gameObject.GetComponent<IParticleBuffer<T>>();
			}
			LogTool.AssertNotNull(this.buffer);
			
			this.particleMaterial = new DisposableMaterial(this.particleShader);

			this.data.particleIndirectBuffer.InitBuffer(this.particleMesh, this.buffer.Buffer.Size);
		}

		protected void OnDestroy()
		{
			this.data?.Release();
			this.particleMaterial?.Dispose();
		}

		protected void Update()
		{
			this.Draw(this.particleMesh, this.particleMaterial, this.data.particleIndirectBuffer);
		}

		protected virtual void Draw(Mesh mesh, Material material, GPUBufferIndirectArgument indirectBuffer)
		{
			if (this.buffer == null || mesh == null || material == null || indirectBuffer == null)
			{
				LogTool.Log("Draw buffer is null, nothing to draw", LogLevel.Warning);
				return;
			}
			this.data.UpdateGPU(material);
            material.SetBuffer("_ParticleBuffer", this.buffer.Buffer);
			var b = new Bounds(Vector3.zero, Vector3.one * 10000);
			Graphics.DrawMeshInstancedIndirect(mesh, 0, material, b, indirectBuffer, 0);
		}
	}
}
