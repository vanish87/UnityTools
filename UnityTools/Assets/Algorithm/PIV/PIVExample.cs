using Unity.Mathematics;
using UnityEngine;
using UnityTools.Common;

namespace UnityTools.Algorithm
{
	public class PIVExample : MonoBehaviour, IFieldTexture
	{
		public RenderTexture input;
		public RenderTexture result;
		public Camera f1Camera;
		public int2 textureSize = new int2(512, 512);
		public Texture FieldAsTexture => this.result;
		public ParticleImageVelocimetry piv;
		public bool drawGUI = false;
		protected void Start()
		{
			var size = this.textureSize;
			var desc = new RenderTextureDescriptor(size.x, size.y, RenderTextureFormat.ARGBFloat);
			desc.enableRandomWrite = true;
			this.result = new RenderTexture(desc);

			desc = new RenderTextureDescriptor(size.x, size.y, RenderTextureFormat.ARGB32);
			this.input = new RenderTexture(desc);
			this.f1Camera.targetTexture = this.input;

			this.piv = this.GetComponent<ParticleImageVelocimetry>();
		}

		protected void OnDestory()
		{
			this.result?.DestoryObj();

			this.f1Camera.targetTexture = null;

			this.input?.DestoryObj();
		}

		protected void Update()
		{
			this.piv.Process(this.input, this.result);
		}

		protected void OnGUI()
		{
			if (!this.drawGUI) return;

			var rect = new Rect(0, 0, this.textureSize.x, this.textureSize.y);
			GUI.DrawTexture(rect, this.input);
			rect = new Rect(this.textureSize.x * 1, 0, this.textureSize.x, this.textureSize.y);
			GUI.DrawTexture(rect, this.result);
		}
	}
}
