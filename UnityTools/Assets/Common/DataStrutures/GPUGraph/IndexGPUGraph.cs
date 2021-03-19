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
            Prediction,
            PositionCorrection,
            VelocityUpdate,
        }

        [StructLayout(LayoutKind.Sequential)]
        public class Node : NodeBase
        {
            int index;
            int sid;
            float3 pos;
            float4 color;


            float3 predictPos;
            float3 restPos;

            float4 rotation;
            float4 predictRotation;

            float3 w;
            float3 velocity;
            float a;
            float b;
            float c;

            float density;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class Edge : EdgeBase
        {

        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EdgeToAdd
        {
            public int sid;
            public int from;
            public int to;
            public float3 formPos;
            public float3 toPos;

            public EdgeToAdd(int sid, int from, int to, float3 fromPos, float3 toPos)
            {
                this.sid = sid;
                this.from = from;
                this.to = to;
                this.formPos = fromPos;
                this.toPos = toPos;
            }
        }


        [System.Serializable]
        public class IndexGPUGraphData: GPUContainer
        {
            [Shader(Name = "_EdgeToAddBuffer")] public GPUBufferVariable<EdgeToAdd> edgeToAdd = new GPUBufferVariable<EdgeToAdd>();
            [Shader(Name = "_AdjacentMatrix")] public GPUBufferVariable<float> adjacentMatrix = new GPUBufferVariable<float>();
            [Shader(Name = "dt")] public float dt = 0.05f;
            [Shader(Name = "stiffness")] public float stiffness = 1;
            [Shader(Name = "dmin")] public float3 dmin = 0;
            [Shader(Name = "dmax")] public float3 dmax = 10000;
            [Shader(Name = "UseGravity")] public bool useGravity = false;

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
            this.dispatcher.Dispatch(Kernel.InitIndexNode, this.data.nodeCount);

            this.dispatcher.Dispatch(Kernel.InitAdjacentMatrix, this.data.nodeCount, this.data.nodeCount);

            var edges = this.indexData.edgeToAdd.CPUData;
            foreach(var i in Enumerable.Range(0, edges.Length))
            {
                edges[i].from = -1;
                edges[i].to = -1;
            }

            // this.AddMesh(this.testMesh);
            // this.AddCircle();

            this.dispatcher.Dispatch(Kernel.AddEdge, edges.Length);
            this.dispatcher.Dispatch(Kernel.ColorNei, this.data.nodeCount);
        }

        protected override void Deinit()
        {
            base.Deinit();
            this.indexData?.Release();
        }

        int sid = 0;
        int pCount = 0;
        protected void AddCircle()
        {
            this.AddOneCircle(sid++);
            this.dispatcher.Dispatch(Kernel.AddEdge, this.data.edgeCount);
        }

        [SerializeField]float raduis = 0.5f;
        protected void AddOneCircle(int sid)
        {
            var center = new float3(UnityEngine.Random.value, UnityEngine.Random.value, 0) * 0.01f;
            var edges = this.indexData.edgeToAdd.CPUData;
            var edgeCount = 0;
            var prev = new float3(0);
            var vcount = 64;
            edges[edgeCount++] = new EdgeToAdd(sid, pCount, 1 + pCount, new float3(0, 0, 0), new float3(raduis, 0, 0));
            foreach(var i in Enumerable.Range(1,vcount))
            {
                var rad = i * 1f / vcount * 2 * math.PI;
                var x = math.cos(rad) * raduis;
                var y = math.sin(rad) * raduis;

                var nrad = (i+1)%vcount * 1f / vcount * 2 * math.PI;
                var x1 = math.cos(nrad) * raduis;
                var y1 = math.sin(nrad) * raduis;

                edges[edgeCount++] = new EdgeToAdd(sid, i + pCount, (i + 1) % vcount + pCount, new float3(x, y, 0) + center, new float3(x1, y1, 0) + center);
                edges[edgeCount++] = new EdgeToAdd(sid, pCount, (i + 1) % vcount + pCount, new float3(0, 0, 0), new float3(x1, y1, 0) + center);
            }
            this.pCount += vcount + 1;

        }

        protected void AddMesh(Mesh m)
        {
            var edgeCount = 0;
            var edges = this.indexData.edgeToAdd.CPUData;
            var indexCount = this.pCount;
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

                v1 = this.transform.TransformPoint(v1);
                v2 = this.transform.TransformPoint(v2);
                v3 = this.transform.TransformPoint(v3);

                edges[edgeCount++] = new EdgeToAdd(this.sid, p1,p2,v1,v2);
                edges[edgeCount++] = new EdgeToAdd(this.sid, p2,p3,v2,v3);
                edges[edgeCount++] = new EdgeToAdd(this.sid, p3,p1,v3,v1);
            }
            this.pCount += added.Count;
            this.sid++;

            this.dispatcher.Dispatch(Kernel.AddEdge, this.data.edgeCount);
        }

        // public bool update = false;
        protected override void Update()
        {
            if(Input.GetKeyDown(KeyCode.C)) this.AddCircle();
            if(Input.GetKeyDown(KeyCode.M)) this.AddMesh(this.testMesh);
            // if(update)
            // foreach(var i in Enumerable.Range(0,3))
            {
                this.dispatcher.Dispatch(Kernel.Prediction, this.data.nodeCount);
                this.dispatcher.Dispatch(Kernel.PositionCorrection, this.data.nodeCount);
                this.dispatcher.Dispatch(Kernel.VelocityUpdate, this.data.nodeCount);
            }

            this.Draw();
        }

    }
}