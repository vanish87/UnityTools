
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityTools.Rendering
{
	public interface IShaderCommandUser
	{
		void AddShaderCommand(IShaderCommand shaderCommand);
		void UpdateShaderCommand();
		void RemoveShaderCommand(IShaderCommand shaderCommand);
	}
	public interface IShaderCommand
	{
		string Name { get; }
		bool SetToMaterial(Material material);
	}
	public enum ShaderCommand
	{
		RenderQueueTag,
		IgnoreProjectorTag,
		RenderTypeTag,
		Blend,
		BlendOperation,
		ColorMask,
		Cull,
		ZClip,
		ZTest,
		ZWrite,
	}
	public interface IShaderCommand<T> : IShaderCommand
	{
		T Value { get; set; }
	}
	public static class ShaderCommandHelper
	{
		public static IShaderCommand Create(ShaderCommand type)
		{
			switch(type)
			{
				case ShaderCommand.RenderQueueTag: return new RenderQueueTag();
				case ShaderCommand.IgnoreProjectorTag: return new IgnoreProjectorTag();
				case ShaderCommand.RenderTypeTag: return new RenderTypeTag();

				case ShaderCommand.Blend: return new Blend();
				case ShaderCommand.BlendOperation: return new BlendOperation();
				case ShaderCommand.ColorMask: return new ColorMask();

				case ShaderCommand.Cull: return new Cull();
				case ShaderCommand.ZClip: return new ZClip();
				case ShaderCommand.ZTest: return new ZTest();
				case ShaderCommand.ZWrite: return new ZWrite();
			}

			return default;
		}

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
}