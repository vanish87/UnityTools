using System;
using Unity.Mathematics;
using UnityEngine;
using UnityTools.Algorithm;
using UnityTools.Common;
using UnityTools.ComputeShaderTool;

namespace UnityTools
{
	[RequireComponent(typeof(HashSort))]
	public class SpaceHash<T> : GPUGrid<int3>
	{
		public enum Kernel
		{
			BuildHash,
			ClearIndex,
			BuildIndex,
			BuildSortedObject,
		}
		public class HashData : GPUContainer
		{
			[Shader(Name = "_HashTableSize")] public int hashTableSize = 32 * 1024;
			[Shader(Name = "_ObjectBufferRead")] public GPUBufferVariable<T> objectBuffer= new GPUBufferVariable<T>();
			[Shader(Name = "_ObjectBufferSorted")] public GPUBufferVariable<T> objectBufferSorted = new GPUBufferVariable<T>();
			[Shader(Name = "_ObjectGridIndexBuffer")] public GPUBufferVariable<uint2> objectGridIndexBuffer = new GPUBufferVariable<uint2>();
		}

		[SerializeField] protected ComputeShader gridCS;
		protected HashData hashData = new HashData();

		protected GridIndexSort Sort => this.sort ??= this.GetComponent<GridIndexSort>();
		protected GridIndexSort sort;
		protected ComputeShaderDispatcher<Kernel> dispatcher;

		public void BuildSortedParticleGridIndex(GPUBufferVariable<T> source, out GPUBufferVariable<T> sortedBuffer)
		{
			sortedBuffer = default;

			this.CheckBufferChanged(source);

			this.dispatcher.Dispatch(Kernel.BuildHash, source.Size);
			this.Sort.Sort(ref this.hashData.objectGridIndexBuffer);

			var s = this.gridData.gridSize;
			this.dispatcher.Dispatch(Kernel.ClearIndex, s.x, s.y, s.z);
			this.dispatcher.Dispatch(Kernel.BuildIndex, source.Size);
			this.dispatcher.Dispatch(Kernel.BuildSortedObject, source.Size);

			sortedBuffer = this.hashData.objectBufferSorted;
		}

		protected void CheckBufferChanged(GPUBufferVariable<T> source)
		{
			if(this.hashData.objectBufferSorted == null || this.hashData.objectBufferSorted.Size != source.Size)
			{
				//use source as object buffer
				this.hashData.objectBuffer.InitBuffer(source);
				//create new buffer for sorted data
				this.hashData.objectBufferSorted.InitBuffer(source.Size);
				//create new buffer for object index
				this.hashData.objectGridIndexBuffer.InitBuffer(source.Size);
				
				this.dispatcher = new ComputeShaderDispatcher<Kernel>(this.gridCS);
				foreach (Kernel k in Enum.GetValues(typeof(Kernel)))
				{
					this.dispatcher.AddParameter(k, this.gridData);
					this.dispatcher.AddParameter(k, this.hashData);
				}
			}
			else
			{
				this.hashData.objectBuffer.UpdateBuffer(source);
			}
			
			var gs = this.gridData.gridSize;
			this.hashData.hashTableSize = gs.x * gs.y * gs.z;
		}
		protected void OnDestroy()
		{
			base.Deinit();
			this.hashData?.Release();
		}
	}
}