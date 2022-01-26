

using Unity.Mathematics;
using UnityEngine;

namespace UnityTools.Rendering
{
	public class TextureAutoSpace : AutoSpace
	{
		public Texture Target { get; set; }

		public override void Init(params object[] parameters)
		{
			base.Init(parameters);

			if (parameters[2] is Texture tex)
			{
				this.Target = tex;
			}

		}

		public override void Update()
		{
			if (this.Target != null)
			{
				var aspect = 1f * this.Target.width / this.Target.height;
				this.Space.Scale = new float3(aspect * this.Space.Scale.y, this.Space.Scale.y, this.Space.Scale.z);
			}
			base.Update();
		}
	}
}