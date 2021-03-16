using System.Linq;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace UnityTools.Common
{
    public class IndexGPUGraph : GPUGraph<IndexGPUGraph.Node, IndexGPUGraph.Edge, IndexGPUGraph.Kernel>
    {
        public enum Kernel
        {
            AddEdge,
        }

        [StructLayout(LayoutKind.Sequential)]
        public class Node : NodeBase
        {
            int index;
            float3 pos;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class Edge : EdgeBase
        {

        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EdgeToAdd
        {
            public int from;
            public int to;
            public float3 formPos;
            public float3 toPos;
        }


        [System.Serializable]
        public class IndexGPUGraphData: GPUContainer
        {
            [Shader(Name = "_EdgeToAddBuffer")] public GPUBufferVariable<EdgeToAdd> edgeToAdd = new GPUBufferVariable<EdgeToAdd>();
        }

        [SerializeField] protected IndexGPUGraphData indexData = new IndexGPUGraphData();

        protected override void Init()
        {
            base.Init();

            this.indexData.edgeToAdd.InitBuffer(128, true);
            foreach(Kernel e in System.Enum.GetValues(typeof(Kernel)))
            {
                this.dispatcher.AddParameter(e, this.indexData);
            }


            var edges = this.indexData.edgeToAdd.CPUData;
            foreach(var i in Enumerable.Range(0, edges.Length))
            {
                edges[i].from = -1;
                edges[i].to = -1;
            }

            
            foreach(var i in Enumerable.Range(0, 32))
            {
                edges[i] = this.AddEdge(0, i);
            }

            this.dispatcher.Dispatch(Kernel.AddEdge, 32);
        }

        protected override void Deinit()
        {
            base.Deinit();
            this.indexData?.Release();
        }


        protected EdgeToAdd AddEdge(int from, int to)
        {
            return new EdgeToAdd() { from = from, to = to, formPos = 0, toPos = UnityEngine.Random.onUnitSphere * 10 };
        }

    }
}