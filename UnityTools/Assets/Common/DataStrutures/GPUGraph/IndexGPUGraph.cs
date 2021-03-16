using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityTools.Rendering;

namespace UnityTools.Common
{
    public class IndexGPUGraph : GPUGraph<IndexGPUGraph.Node, IndexGPUGraph.Edge, IndexGPUGraph.Kernel>
    {
        public enum Kernel
        {
            InitAdjacentMatrix,
            AddEdge,
            ColorNei,
            InitIndexNode,
        }

        [StructLayout(LayoutKind.Sequential)]
        public class Node : NodeBase
        {
            int index;
            float3 pos;
            float4 color;
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
            [Shader(Name = "_AdjacentMatrix")] public GPUBufferVariable<float> adjacentMatrix = new GPUBufferVariable<float>();
        }

        [SerializeField] protected IndexGPUGraphData indexData = new IndexGPUGraphData();

        [SerializeField] protected Mesh testMesh;

        protected override void Init()
        {
            base.Init();

            this.indexData.edgeToAdd.InitBuffer(this.data.edgeCount, true);
            this.indexData.adjacentMatrix.InitBuffer(this.data.nodeCount * this.data.nodeCount);


            foreach(Kernel e in System.Enum.GetValues(typeof(Kernel)))
            {
                this.dispatcher.AddParameter(e, this.indexData);
            }
            this.data.nodeIndexBuffer.Data.SetCounterValue(0);
            this.dispatcher.Dispatch(Kernel.InitIndexNode, this.data.nodeCount);

            // this.dispatcher.Dispatch(Kernel.InitAdjacentMatrix, this.indexData.adjacentMatrix.Size);

            var edges = this.indexData.edgeToAdd.CPUData;
            foreach(var i in Enumerable.Range(0, edges.Length))
            {
                edges[i].from = -1;
                edges[i].to = -1;
            }

            this.AddMesh(this.testMesh);

            this.dispatcher.Dispatch(Kernel.AddEdge, edges.Length);
            this.dispatcher.Dispatch(Kernel.ColorNei, this.data.nodeCount);
        }

        protected override void Deinit()
        {
            base.Deinit();
            this.indexData?.Release();
        }


        protected EdgeToAdd AddEdge(int from, int to, float3 fromPos, float3 toPos)
        {
            return new EdgeToAdd() { from = from, to = to, formPos = fromPos, toPos = toPos };
        }

        protected void AddMesh(Mesh m)
        {
            var edgeCount = 0;
            var edges = this.indexData.edgeToAdd.CPUData;
            var indexCount = 0;
            var added = new Dictionary<Vector3, int>();
            for (var t = 0; t < m.triangles.Length; t += 3)
            {
                var v1 = m.vertices[m.triangles[t]];
                var v2 = m.vertices[m.triangles[t + 1]];
                var v3 = m.vertices[m.triangles[t + 2]];

                int p1;
                int p2;
                int p3;
                if (!added.TryGetValue(v1, out p1))
                {
                    p1 = indexCount++;
                    added.Add(v1, p1);
                }
                if (!added.TryGetValue(v2, out p2))
                {
                    p2 = indexCount++;
                    added.Add(v2, p2);
                }
                if (!added.TryGetValue(v3, out p3))
                {
                    p3 = indexCount++;
                    added.Add(v3, p3);
                }

                edges[edgeCount++] = this.AddEdge(p1,p2,v1,v2);
                edges[edgeCount++] = this.AddEdge(p2,p3,v2,v3);
                edges[edgeCount++] = this.AddEdge(p3,p1,v3,v1);
            }
        }

    }
}