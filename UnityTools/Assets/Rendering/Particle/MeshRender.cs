
using UnityEngine;
using UnityTools.Common;
using UnityTools.Debuging;

namespace UnityTools.Rendering
{
	public class MeshRender<T>: DataRenderBase<T>
	{
		[SerializeField] protected Mesh mesh;
		protected GPUBufferIndirectArgument indirectBuffer = new GPUBufferIndirectArgument();

		public override void Init()
		{
			base.Init();

			LogTool.AssertNotNull(this.mesh);
			this.indirectBuffer.InitBuffer(this.mesh, this.buffer.Buffer.Size);
		}
		public override void Deinit()
		{
			base.Deinit();
			this.indirectBuffer?.Release();
		}

		protected override void Draw(Material material)
		{
			if (this.buffer == null || this.mesh == null || material == null || this.indirectBuffer == null)
			{
				LogTool.Log("Draw buffer is null, nothing to draw", LogLevel.Warning);
				return;
			}
			this.indirectBuffer.UpdateGPU(material);
            material.SetBuffer("_ParticleBuffer", this.buffer.Buffer);
			var b = new Bounds(Vector3.zero, Vector3.one * 10000);
			Graphics.DrawMeshInstancedIndirect(mesh, 0, material, b, this.indirectBuffer, 0);
		}
	}
}