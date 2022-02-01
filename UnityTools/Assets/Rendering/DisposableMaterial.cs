using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityTools.Common;
using UnityTools.Debuging;

namespace UnityTools.Rendering
{
	[Serializable]
	public class DisposableMaterial : DisposableObject<Material>, IShaderCommandUser
	{
		[SerializeField] protected RenderQueueTag renderQueueTag = new RenderQueueTag();
		[SerializeField] protected IgnoreProjectorTag ignoreProjectorTag = new IgnoreProjectorTag();
		[SerializeField] protected RenderTypeTag renderTypeTag = new RenderTypeTag();

		[SerializeField] protected Blend blend = new Blend();
		[SerializeField] protected BlendOperation blendOperation = new BlendOperation();
		[SerializeField] protected ColorMask colorMask = new ColorMask();

		[SerializeField] protected Cull cull = new Cull();
		[SerializeField] protected ZClip zClip = new ZClip();
		[SerializeField] protected ZTest zTest = new ZTest();
		[SerializeField] protected ZWrite zWrite = new ZWrite();
		[SerializeField] protected CheckBoard checkBoardSize = new CheckBoard();
		[SerializeField] protected List<IShaderCommand> commands = new List<IShaderCommand>();
		public DisposableMaterial(Material data) : base(new Material(data))
		{
			this.AddAllShaderCommand();
		}

		public DisposableMaterial(Shader shader) : base(new Material(shader))
		{
			this.AddAllShaderCommand();
		}
		public virtual void AddAllShaderCommand()
		{
			foreach (var c in ObjectTool.FindAllFieldValue<IShaderCommand>(this.GetType(), this))
			{
				this.AddShaderCommand(c);
			}
		}
		public virtual void RemoveAllShaderCommand()
		{
			this.commands?.Clear();
		}

		public void AddShaderCommand(IShaderCommand shaderCommand)
		{
			if (this.commands.Contains(shaderCommand))
			{
				LogTool.Log("Duplicated Shader Command" + shaderCommand.Name, LogLevel.Warning);
				return;
			}
			this.commands.Add(shaderCommand);
		}

		public void RemoveShaderCommand(IShaderCommand shaderCommand)
		{
			if (this.commands.Contains(shaderCommand))
			{
				this.commands.Remove(shaderCommand);
			}
		}

		public virtual void UpdateShaderCommand()
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
