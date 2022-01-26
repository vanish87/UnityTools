
using UnityEngine;
using UnityTools.Common;

namespace UnityTools.Rendering
{
	public class AutoMaterial : MonoBehaviour, IInitialize
	{
		public bool Inited => this.inited;

		[SerializeField] protected DisposableMaterial material;
		protected bool inited = false;

		public void Deinit(params object[] parameters)
		{
			this.material?.Dispose();
			this.inited = false;
		}

		public void Init(params object[] parameters)
		{
			if (Inited) return;
			if (parameters.Length < 1) return;

			if (parameters[0] is Shader shader)
			{
				shader = shader != null ? shader : Shader.Find("Unlit/Texture");
				this.material = new DisposableMaterial(shader);
				var render = this.gameObject.FindOrAddTypeInComponentsAndChildren<MeshRenderer>();
				render.sharedMaterial = this.material;
			}

			this.inited = true;
		}

		protected void OnDestory()
		{
			this.Deinit();
		}

	}
}