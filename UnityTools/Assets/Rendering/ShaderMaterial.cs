using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Common;
using UnityTools.Debuging;

namespace UnityTools.Rendering
{
	[System.Serializable]
	public class ShaderMaterial<T> : ShaderMaterial where T : Enum
	{
		public T type;
	}
	[System.Serializable]
	public class ShaderMaterial : IInitialize, IShaderCommandUser
	{
		public const string DEFAULT_SHADER_NAME = "Unlit/Texture";
		public string defaultShaderName = DEFAULT_SHADER_NAME;
		public Shader Shader
		{
			get
			{
				if (this.shader == null)
				{
					this.shader = Shader.Find(this.defaultShaderName);
					if (this.shader == null) LogTool.Log("Cannot find shader " + this.defaultShaderName, LogLevel.Error);
				}
				if(this.shader == null) LogTool.Log("Shader not find " + this.defaultShaderName, LogLevel.Error);
				return this.shader;
			}
		}
		public Material Mat => this.material ??= new Material(this.Shader);
		public bool Inited => this.inited;
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
		protected List<IShaderCommand> commands = new List<IShaderCommand>();
		protected bool inited = false;
		protected Material material;
		[SerializeField] protected Shader shader;

		public virtual void Init(params object[] parameters)
		{
			if (this.Inited) return;

			this.AddAllShaderCommand();
			if(String.IsNullOrEmpty(this.defaultShaderName))
			{
				this.defaultShaderName = this.shader != null ? this.shader.name : DEFAULT_SHADER_NAME;
			}

			if (this.shader != null && this.defaultShaderName != this.shader.name)
			{
				LogTool.Log("Inconstant Shade name, using " + this.shader.name, LogLevel.Warning);
				this.defaultShaderName = this.shader.name;
			}

			this.inited = true;
		}
		public virtual void Deinit(params object[] parameters)
		{
			this.material?.DestoryObj();
			this.material = null;
			this.RemoveAllShaderCommand();
			this.inited = false;
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
			foreach (var c in this.commands) c.SetToMaterial(this.Mat);
		}

		public static implicit operator Material(ShaderMaterial source)
		{
			LogTool.AssertNotNull(source.Mat);
			return source.Mat;
		}
	}
}