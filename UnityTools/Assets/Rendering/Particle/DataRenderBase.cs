using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Common;
using UnityTools.Debuging;

namespace UnityTools.Rendering
{
	public interface IDataBuffer<T>
	{
		GPUBufferVariable<T> Buffer { get; }
		ISpace Space { get; }
	}

	public class DataRenderBase<T> : MonoBehaviour, IInitialize
	{
		[SerializeField] protected Shader dataShader;
		protected IDataBuffer<T> buffer;
		protected DisposableMaterial particleMaterial;
		protected bool inited = false;

		public bool Inited => this.inited;

		public void Start()
		{
			if(!this.Inited) this.Init();
		}
		protected void OnDestroy()
		{
			this.Deinit();
		}
		protected void Update()
		{
			this.Draw(this.particleMaterial);
		}
		protected virtual void Draw(Material material)
		{
			if (this.buffer == null || material == null)
			{
				LogTool.Log("Draw buffer is null, nothing to draw", LogLevel.Warning);
				return;
			}
            material.SetBuffer("_DataBuffer", this.buffer.Buffer);
			var b = new Bounds(Vector3.zero, Vector3.one * 10000);
			Graphics.DrawProcedural(material, b, MeshTopology.Points, this.buffer.Buffer.Size);
		}
		public virtual void Init()
		{
			this.buffer = this.gameObject.GetComponent<IDataBuffer<T>>();
			LogTool.AssertNotNull(this.buffer);
			
			this.particleMaterial = new DisposableMaterial(this.dataShader);
			this.inited = true;
		}
		public virtual void Deinit()
		{
			this.particleMaterial?.Dispose();
		}
	}
}
