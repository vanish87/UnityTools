using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityTools.Common;

namespace UnityTools.Rendering
{
	public interface IShaderCommand
	{
		string Name { get; }
		bool SetToMaterial(Material material);
	}
	public interface IShaderCommand<T> : IShaderCommand
	{
		T Value { get; set; }
	}

	[Serializable]
	public abstract class IntShaderCommand<T> : IShaderCommand<T>
	{
		public abstract string Name { get; }

		public T Value { get => this.v; set => this.v = value; }
		public T v = default;

		public virtual bool SetToMaterial(Material material)
		{
			material.SetInt(this.Name, Convert.ToInt32(this.v));
			return true;
		}
	}
	[Serializable]
	public abstract class BoolShaderCommand : IntShaderCommand<bool>
	{
		public BoolShaderCommand() : base() { this.Value = false; }
		public override bool SetToMaterial(Material material)
		{
			material.SetOverrideTag(this.Name, this.v ? "True" : "False");
			return true;
		}
	}
	[Serializable]
	public abstract class OnOffShaderCommand : IntShaderCommand<bool>
	{
		public OnOffShaderCommand() : base() { this.Value = false; }
		public override bool SetToMaterial(Material material)
		{
			material.SetInt(this.Name, this.v ? 1 : 0);
			return true;
		}
	}

	[Serializable]
	public class Blend : BoolShaderCommand
	{
		public override string Name => "Blend";
		public BlendMode src = BlendMode.One;
		public BlendMode dst = BlendMode.Zero;
		public Blend() : base() { this.Value = false;}

		public override bool SetToMaterial(Material material)
		{
			if (this.Value)
			{
				material.SetInt("BlendSrc", (int)this.src);
				material.SetInt("BlendDst", (int)this.dst);
			}
			else
			{
				material.SetInt("BlendSrc", (int)BlendMode.One);
				material.SetInt("BlendDst", (int)BlendMode.Zero);
			}
			return true;
		}
	}
	[Serializable]
	public class BlendOperation : IntShaderCommand<BlendOp>
	{
		public override string Name => "BlendOp";
		public BlendOperation() : base() { this.Value = BlendOp.Add;}
	}
	[Serializable]
	public class ColorMask : IntShaderCommand<ColorMask.Mask>
	{
		[Flags]
		public enum Mask
		{
			None = 0,
			Blue = 1 << 1,
			Green = 1 << 2,
			Red = 1 << 3,
			Alpha = 1 << 4,
			RGB = Blue | Green | Red,
			RGBA = Blue | Green | Red | Alpha,
		}
		public override string Name => "ColorMask";
		public ColorMask() : base() { this.Value = Mask.RGB; }
	}
	[Serializable]
	public class Conservative : BoolShaderCommand
	{
		public override string Name => "Conservative";
	}

	[Serializable]
	public class Cull : IntShaderCommand<CullMode>
	{
		public override string Name => "CullMode";
		public Cull() : base() { this.Value = CullMode.Back; }
	}
	[Serializable]
	public class ZClip : BoolShaderCommand
	{
		public override string Name => "ZClip";
	}
	[Serializable]
	public class ZTest : IntShaderCommand<CompareFunction>
	{
		public override string Name => "ZTest";
		public ZTest() : base() { this.Value = CompareFunction.Less; }
	}
	[Serializable]
	public class ZWrite : OnOffShaderCommand
	{
		public override string Name => "ZWrite";
		public ZWrite() : base() { this.Value = true; }
	}
	[Serializable]
	public class RenderQueueTag : IntShaderCommand<RenderQueue>
	{
		public override string Name => "Queue";
		public RenderQueueTag() : base() { this.Value = RenderQueue.Geometry; }
		public override bool SetToMaterial(Material material)
		{
			material.renderQueue = (int)this.Value;
			return true;
		}
	}
	[Serializable]
	public class IgnoreProjectorTag : BoolShaderCommand
	{
		public IgnoreProjectorTag() : base() { this.Value = true; }
		public override string Name => "IgnoreProjector";
	}
	[Serializable]
	public class RenderTypeTag : IntShaderCommand<string>
	{
		public override string Name => "RenderType";
		public override bool SetToMaterial(Material material)
		{
			material.SetOverrideTag(this.Name, this.v);
			return true;
		}
	}

	[Serializable]
	public class DisposableMaterial : DisposableObject<Material>
	{
		public RenderQueueTag quque = new RenderQueueTag();
		public IgnoreProjectorTag ignoreProjector = new IgnoreProjectorTag();
		public RenderTypeTag renderTypeTag = new RenderTypeTag();
		public Blend blend = new Blend();
		public BlendOperation blendOperation = new BlendOperation();
		public ColorMask colorMask = new ColorMask();
		public Cull cull = new Cull();
		public ZClip zClip = new ZClip();
		public ZTest zTest = new ZTest();
		public ZWrite zWrite = new ZWrite();
		protected IEnumerable<IShaderCommand> commands;
		public DisposableMaterial(Material data) : base(new Material(data))
		{
			this.commands = ObjectTool.FindAllFieldValue<IShaderCommand>(typeof(DisposableMaterial), this);
		}

		public DisposableMaterial(Shader shader) : base(new Material(shader))
		{
			this.commands = ObjectTool.FindAllFieldValue<IShaderCommand>(typeof(DisposableMaterial), this);
		}
		public virtual void UpdateProperty()
		{
			foreach (var c in this.commands) c.SetToMaterial(this.Data);
		}

		protected override void DisposeUnmanaged()
		{
			base.DisposeUnmanaged();
			// Assert.IsNotNull(this.data);
			data?.DestoryObj();
			data = null;
		}
		public static implicit operator DisposableMaterial(Material data)
		{
			return new DisposableMaterial(data);
		}
		public static implicit operator Material(DisposableMaterial source)
		{
			return source.Data;
		}
	}

	[Serializable]
	public class DisposableMaterial<Keyword> : DisposableMaterial where Keyword : Enum
	{
		public Keyword keyword = default;

		public DisposableMaterial(Material mat) : base(mat)
		{

		}

		public DisposableMaterial(Shader shader) : base(shader)
		{

		}

		public virtual void UpdateKeyword()
		{
			foreach (Keyword kw in Enum.GetValues(typeof(Keyword)))
			{
				var kwString = kw.ToString();
				if (this.keyword.HasFlag(kw)) this.Data.EnableKeyword(kwString);
				else this.Data.DisableKeyword(kwString);
			}
		}
	}
}
