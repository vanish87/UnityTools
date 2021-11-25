
using UnityEngine;
using UnityTools.Common;
using UnityTools.Debuging;

namespace UnityTools.Rendering
{
	public class MeshRender<T>: DataRenderBase<T>
	{
		[SerializeField] protected Mesh mesh;
		protected GPUBufferIndirectArgument indirectBuffer = new GPUBufferIndirectArgument();
		public override void Init(params object[] parameters)
		{
			base.Init();
			LogTool.AssertNotNull(this.mesh);
		}
		public override void Deinit(params object[] parameters)
		{
			base.Deinit();
			this.indirectBuffer?.Release();
		}

		public override void OnUpdateDraw()
		{
			if (this.Source == null || this.mesh == null || this.dataMaterial == null)
			{
				LogTool.Log("Draw buffer is null, nothing to draw", LogLevel.Warning);
				return;
			}
			if(this.indirectBuffer.Size == 0 || this.indirectBuffer.CPUData[0] != this.mesh.GetIndexCount(0))
			{
				this.indirectBuffer.InitBuffer(this.mesh, this.Source.Buffer.Size);
			}

			this.dataMaterial.UpdateShaderCommand();
			this.indirectBuffer.UpdateGPU(this.dataMaterial);
			this.Source.Buffer.UpdateGPU(this.dataMaterial);
			Graphics.DrawMeshInstancedIndirect(this.mesh, 0, this.dataMaterial, this.Source.Space.Bound, this.indirectBuffer, 0);
		}
		public override void OnRenderDraw()
		{
			if (this.Source == null || this.mesh == null || this.dataMaterial == null || this.indirectBuffer == null)
			{
				LogTool.Log("Draw buffer is null, nothing to draw", LogLevel.Warning);
				return;
			}

			LogTool.Log("Not supported now", LogLevel.Warning);
		}
	}
}