

using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityTools.Common;
using UnityTools.ComputeShaderTool;
using UnityTools.Debuging;
using UnityTools.Rendering;

namespace UnityTools.Algorithm
{
	public class TextureReduction : MonoBehaviour, IInitialize
	{
		public bool DrawGUI = false;
		public bool Inited => this.inited;
		protected const string ReductionKernelName = "Reduction";
		protected const string ResultKernelName = "Result";
		[SerializeField] protected ComputeShader computeShader;
		protected bool inited = false;
		protected Texture source;
		protected Dictionary<int, Texture> reductionBuffer = new Dictionary<int, Texture>();
		protected GPUBufferVariable<float4> resultBuffer = new GPUBufferVariable<float4>("_ResultBuffer", 1, true, false);
		public void Init(params object[] parameters)
		{
			if(this.Inited) return;

			if(parameters.Length > 0 && parameters[0] is Texture tex)
			{
				this.source = tex;
			}
			this.UpdateReductionBuffer(this.source);

			this.inited = true;
		}
		public void Deinit(params object[] parameters)
		{
			this.CleanUp();
			this.inited = false;
		}

		public float4 GetReductionResult()
		{
			LogTool.AssertIsTrue(this.Inited);
			if(!this.Inited) return 0;

			this.DoReduction(this.source, this.reductionBuffer);
			this.resultBuffer.GetToCPUData();
			return this.resultBuffer.CPUData[0];
		}

		protected void OnDisable()
		{
			this.Deinit();
		}

		protected void CleanUp()
		{
			foreach(var t in this.reductionBuffer)
			{
				t.Value?.DestoryObj();
			}

			this.reductionBuffer.Clear();
			this.resultBuffer?.Release();
		}

		protected void UpdateReductionBuffer(Texture source)
		{
			LogTool.AssertIsTrue(Mathf.IsPowerOfTwo(source.width));
			LogTool.AssertIsTrue(Mathf.IsPowerOfTwo(source.height));

			this.CleanUp();

			var w = source.width;
			var h = source.height;
			for (; w > 0 || h > 0; w = w >> 1, h = h >> 1)
			{
				// Debug.Log(w + " " + h);

				var tw = math.max(1, w);
				var th = math.max(1, h);
				var p = math.max(w, h);

				// Debug.Log(p + " with " + tw + "x" + th);

				var desc = new RenderTextureDescriptor(tw, th, RenderTextureFormat.ARGBFloat);
				desc.enableRandomWrite = true;
				var tex = TextureManager.Create(desc);

				this.reductionBuffer.Add(p, tex);
			}
			Graphics.CopyTexture(source, this.reductionBuffer[math.max(source.width, source.height)]);

			this.resultBuffer.InitBuffer(1, true, false);
		}

		protected void DoReduction(Texture source, Dictionary<int, Texture> reductionBuffer)
		{
			LogTool.AssertNotNull(source);
			LogTool.AssertNotNull(reductionBuffer);

			Graphics.CopyTexture(source, this.reductionBuffer[math.max(source.width, source.height)]);

			var w = source.width;
			var h = source.height;
			for (; w > 0 || h > 0; w = w >> 1, h = h >> 1)
			{
				var k = this.computeShader.FindKernel(ReductionKernelName);
				var p = math.max(w, h);

				if(p > 1)
				{
					var from = p;
					var to = p >> 1;
					var tw = math.max(1, w>>1);
					var th = math.max(1, h>>1);

					// Debug.Log("from " + from + " with " + reductionBuffer[from].width + "x" + reductionBuffer[from].height);
					// Debug.Log("to " + to + " with " + reductionBuffer[to].width + "x" + reductionBuffer[to].height);
					// Debug.Log("Dispatch " + tw + "x" + th);

					this.computeShader.SetTexture(k, "_From", reductionBuffer[from]);
					this.computeShader.SetTexture(k, "_To", reductionBuffer[to]);
					this.computeShader.Dispatch(k, tw, th, 1);
				}
				else
				{
					LogTool.AssertIsTrue(p == 1);
					k = this.computeShader.FindKernel(ResultKernelName);
					this.computeShader.SetTexture(k, "_From", reductionBuffer[p]);
					this.computeShader.SetBuffer(k, this.resultBuffer.ShaderName, this.resultBuffer);
					this.computeShader.Dispatch(k, 1, 1, 1);
				}
			}
		}

		protected void OnGUI()
		{
			if(!this.DrawGUI) return;

			var w = 0;
			foreach(var t in this.reductionBuffer.Values)
			{
				var rect = new Rect(w, 0, t.width, t.height);
				GUI.DrawTexture(rect, t);
				w += t.width;
			}
		}
	}
}