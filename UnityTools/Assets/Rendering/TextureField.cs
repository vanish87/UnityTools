using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityTools.Debuging;
using UnityTools.Rendering;

namespace UnityTools.Common
{
	public interface IField<T>
	{
		T Field { get; }
		int3 Size { get; set; }
	}
	public interface IFieldTexture
	{
		Texture FieldAsTexture { get; }
	}

	[System.Serializable]
	public class TextureField : GPUVariable, IField<Texture>, IFieldTexture
	{
		public Texture Field => this.data;
		public Texture FieldAsTexture => this.data;
		public int3 Size { get => this.fieldSize; set => this.fieldSize = value; }


		[SerializeField] protected int3 fieldSize = new int3(512, 512, 1);
		[SerializeField] protected RenderTextureFormat format = RenderTextureFormat.ARGBFloat;
		[SerializeField] protected Texture data;

		public virtual void Init(int3 size, RenderTextureFormat format = RenderTextureFormat.ARGBFloat)
		{
			this.Release();

            this.fieldSize = size;
            this.format = format;
            
			var rt = new RenderTexture(this.fieldSize.x, this.fieldSize.y, this.fieldSize.z, this.format);
			rt.enableRandomWrite = true;
            rt.name = this.shaderName;
			this.data = rt;
		}
		public override void Release()
		{
			base.Release();
			this.data?.DestoryObj();
		}

		public override void SetToGPU(object container, ComputeShader cs, string kernel = null)
		{
			LogTool.AssertNotNull(container);
			LogTool.AssertNotNull(cs);
			if (cs == null) return;
			var id = cs.FindKernel(kernel);
			cs.SetVector(this.shaderName + "Size", new Vector4(this.Size.x, this.Size.y, this.Size.z, 0));
			cs.SetTexture(id, this.shaderName, this.data);
		}
		public override void SetToMaterial(object container, Material material)
		{
			LogTool.AssertNotNull(container);
			LogTool.AssertNotNull(material);
			if (material == null) return;
			material.SetTexture(this.shaderName, this.data);
		}
	}
}
