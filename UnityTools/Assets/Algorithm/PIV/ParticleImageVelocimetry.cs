
using Unity.Mathematics;
using UnityEngine;
using UnityTools.Attributes;
using UnityTools.Common;
using UnityTools.ComputeShaderTool;
using UnityTools.Debuging;
using UnityTools.Rendering;

namespace UnityTools.Algorithm
{
	public class ParticleImageVelocimetry : MonoBehaviour
	{
		[System.Serializable]
		public class GPUData : GPUContainer
		{
			[Shader(Name = "_WindowSize")] public int2 windowSize = new int2(32, 32);
			[Shader(Name = "_TextureSize"), DisableEdit] public int2 textureSize = new int2(32, 32);
			[Shader(Name = "_PrevTex")] public Texture prevTex;
			[Shader(Name = "_CurrTex")] public Texture currTex;
			[Shader(Name = "_WindowResult")]public Texture windowResult;
			[Shader(Name = "_Result")]public Texture result;

		}
		protected readonly string WindowKernel = "PIVWindow";
		protected readonly string MaxKernel = "PIVMax";
		[SerializeField] protected ComputeShader computeShader;
		[SerializeField] protected GPUData data = new GPUData();
		protected ComputeShaderDispatcher dispatcher;
		protected void OnEnable()
		{
			LogTool.AssertIsTrue(this.computeShader != null);

			this.dispatcher = new ComputeShaderDispatcher(this.computeShader);
			this.dispatcher.AddParameter(WindowKernel, this.data);
			this.dispatcher.AddParameter(MaxKernel, this.data);
		}
		protected void OnDisable()
		{
			this.data?.Release();
		}

		public void Process(Texture input, Texture output, params object[] parameters)
		{
			if (input == null || output == null) return;

			if (this.data.prevTex == null)
			{
				this.data.prevTex = input.Clone();
				this.data.prevTex.name = "Prev Texture";
			}
			if(this.data.result == null)
			{
				var rsize = new int2(input.width, input.height) / this.data.windowSize;
				var desc = new RenderTextureDescriptor(rsize.x, rsize.y, RenderTextureFormat.ARGBFloat);
				desc.enableRandomWrite = true;
				desc.sRGB = false;
				this.data.result = TextureManager.Create(desc);
				this.data.result.name = "Result Velocity Texture";

				desc = new RenderTextureDescriptor(input.width, input.height, RenderTextureFormat.ARGBFloat);
				desc.enableRandomWrite = true;
				desc.sRGB = false;
				this.data.windowResult = TextureManager.Create(desc);
				this.data.windowResult.name = "Pixel Window Result Texture";
			}

			if (this.computeShader != null)
			{
				var resolution = new int2(input.width, input.height);
				this.data.textureSize = resolution;
				this.data.currTex = input;
				this.dispatcher.DispatchNoneThread(WindowKernel, resolution.x, resolution.y, 1);

				var dispatchSize = resolution / this.data.windowSize;
				this.dispatcher.DispatchNoneThread(MaxKernel, dispatchSize.x, dispatchSize.y, 1);
			}

			Graphics.CopyTexture(input, this.data.prevTex);
			Graphics.Blit(this.data.result, output as RenderTexture);
		}

	}
}