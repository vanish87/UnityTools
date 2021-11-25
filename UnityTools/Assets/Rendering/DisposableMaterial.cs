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
		[SerializeField] protected List<IShaderCommand> commands = new List<IShaderCommand>();
		public DisposableMaterial(Material data) : base(new Material(data))
		{
			foreach(var c in ObjectTool.FindAllFieldValue<IShaderCommand>(typeof(DisposableMaterial), this))
			{
				this.AddShaderCommand(c);
			}
		}

		public DisposableMaterial(Shader shader) : base(new Material(shader))
		{
			foreach(var c in ObjectTool.FindAllFieldValue<IShaderCommand>(typeof(DisposableMaterial), this))
			{
				this.AddShaderCommand(c);
			}
		}

		public void AddShaderCommand(IShaderCommand shaderCommand)
		{
			if(this.commands.Contains(shaderCommand)) 
			{
				LogTool.Log("Duplicated Shader Command" + shaderCommand.Name, LogLevel.Warning);
				return;
			}
			this.commands.Add(shaderCommand);
		}

		public void RemoveShaderCommand(IShaderCommand shaderCommand)
		{
			if(this.commands.Contains(shaderCommand)) 
			{
				this.commands.Remove(shaderCommand);
			}
		}

		public void RemoveAllShaderCommand()
		{
			this.commands.Clear();
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
