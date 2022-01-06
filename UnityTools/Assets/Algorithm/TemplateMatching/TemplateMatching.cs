
using System;
using System.Collections;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityTools.Common;
using UnityTools.ComputeShaderTool;
using UnityTools.Rendering;

namespace UnityTools.Algorithm
{
	public class TemplateMatching : MonoBehaviour
	{
		public Texture2D input;
		public Texture2D template;
		[Serializable]
		public class GPUData : GPUContainer
		{
			[Shader(Name = "_InputTex")] public Texture2D input;
			[Shader(Name = "_TemplateTex")] public Texture2D template;
			[Shader(Name = "_ResultTex")] public RenderTexture resultTex;
			[Shader(Name = "_MaskedTex")] public Texture2D maskedTex;
		}

		public GPUData data = new GPUData();
		public ComputeShader shader;
		protected ComputeShaderDispatcher dispatcher;

		protected void OnEnable()
		{
			this.data.input = this.input.Clone() as Texture2D;
			this.data.maskedTex = this.input.Clone() as Texture2D;
			this.data.template = this.template.Clone() as Texture2D;

			var desc = new RenderTextureDescriptor(this.input.width, this.input.height, RenderTextureFormat.ARGBFloat);
			desc.enableRandomWrite = true;
			this.data.resultTex = TextureManager.Create(desc);

			this.dispatcher = new ComputeShaderDispatcher(this.shader);
			this.dispatcher.AddParameter("Process", this.data);

			var tm = this.GetComponent<TextureReduction>();
			tm.Init(this.data.resultTex);

			//calculate result for each pixel
			var domain = new int2(input.width - template.width, input.height - template.height);
			this.dispatcher.Dispatch("Process", domain.x, domain.y);
			//find max value from result texture
			var result = tm.GetReductionResult();

			Debug.Log(result);

			var pixel = result.xy.FloorToInt();
			var h = new int2(template.width, template.height);
            foreach (var u in Enumerable.Range(0, template.width))
            {
                foreach (var v in Enumerable.Range(0, template.height))
                {
                    this.data.maskedTex.SetPixel(pixel.x + u, pixel.y + v, Color.red);
                }
            }
            this.data.maskedTex.Apply();
		}
		protected void OnDisable()
		{
			this.data.Release();
		}

		protected void OnGUI()
		{
			var y = 0;
			GUI.DrawTexture(new Rect(0, y, this.input.width, this.input.height), this.input);
			GUI.DrawTexture(new Rect(this.input.width + 20, y, this.template.width, this.template.height), this.template);
			y += this.input.height;
			if (this.data.maskedTex != null)
			{
				GUI.DrawTexture(new Rect(0, y, this.data.maskedTex.width, this.data.maskedTex.height), this.data.maskedTex);
			}
		}
	}
}
