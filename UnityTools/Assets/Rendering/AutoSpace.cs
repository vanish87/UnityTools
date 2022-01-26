
using UnityEngine;
using UnityTools.Common;

namespace UnityTools.Rendering
{
	public class AutoSpace : MonoBehaviour, IInitialize
	{
		public bool Inited => this.inited;
		protected bool inited = false;
		protected ISpace Space { get; set; }
		protected bool autoSpace = false;

		public virtual void Deinit(params object[] parameters)
		{
			this.inited = false;
		}

		public virtual void Init(params object[] parameters)
		{
			if (this.Inited) return;
			if (parameters.Length < 2) return;

			if (parameters[0] is ISpace s && parameters[1] is bool auto)
			{
				this.autoSpace = auto;
				this.Space = s != null ? s : Common.Space.IdentitySpace;
			}

			this.inited = true;
		}

		public virtual void Update()
		{
			if(this.autoSpace)
			{
				Common.Space.SetGameObjectToSpace(this.gameObject, this.Space);
			}
		}

	}
}