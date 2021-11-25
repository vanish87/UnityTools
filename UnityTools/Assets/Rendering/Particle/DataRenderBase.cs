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

	public interface IDataRender
	{
		DataRenderMode Mode { get; }
		void OnUpdateDraw();
		void OnRenderDraw();
	}

	public enum DataRenderMode
	{
		OnUpdate,
		OnRender,
	}

	public class DataRenderBase<T> : MonoBehaviour, IInitialize, IDataRender
	{
		public bool Inited => this.inited;
		public DataRenderMode Mode => this.mode;

		[SerializeField] protected Shader dataShader;
		[SerializeField] protected DataRenderMode mode = DataRenderMode.OnUpdate;
		protected IDataBuffer<T> Source => this.source ??= this.gameObject.GetComponent<IDataBuffer<T>>();
		protected IDataBuffer<T> source; 
		protected DisposableMaterial dataMaterial;
		protected bool inited = false;

		public virtual void Init(params object[] parameters)
		{
			if (this.Inited) return;

			this.dataMaterial = new DisposableMaterial(this.dataShader);
			this.inited = true;
		}
		public virtual void Deinit(params object[] parameters)
		{
			this.dataMaterial?.Dispose();
			this.inited = false;
		}

		public virtual void OnUpdateDraw()
		{
			this.dataMaterial.UpdateShaderCommand();
			if (this.Source == null || this.dataMaterial == null)
			{
				LogTool.Log("Draw buffer is null, nothing to draw", LogLevel.Warning);
				return;
			}
			this.Source.Buffer.UpdateGPU(this.dataMaterial);
			Graphics.DrawProcedural(this.dataMaterial, this.Source.Space.Bound, MeshTopology.Points, this.Source.Buffer.Size);
		}

		public virtual void OnRenderDraw()
		{
			this.dataMaterial.UpdateShaderCommand();
			if (this.Source == null || this.dataMaterial == null)
			{
				LogTool.Log("Draw buffer is null, nothing to draw", LogLevel.Warning);
				return;
			}
			this.Source.Buffer.UpdateGPU(this.dataMaterial);
			this.dataMaterial.Data.SetPass(0);
			Graphics.DrawProceduralNow(MeshTopology.Points, this.Source.Buffer.Size, 1);
		}
		protected void OnEnable()
		{
			this.Init();
		}
		protected void OnDisable()
		{
			this.Deinit();
		}
		protected void Update()
		{
			if (this.Mode == DataRenderMode.OnUpdate)
			{
				this.OnUpdateDraw();
			}
		}
		protected void OnRenderObject()
		{
			if (this.Mode == DataRenderMode.OnRender)
			{
				this.OnRenderDraw();
			}
		}
	}
}
