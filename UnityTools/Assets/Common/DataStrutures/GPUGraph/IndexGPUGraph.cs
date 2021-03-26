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
            [Shader(Name = "_AdjacentMatrix")] public GPUBufferVariable<int> adjacentMatrix = new GPUBufferVariable<int>();
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

        [SerializeField]float radius = 0.5f;
        [SerializeField]int vcount = 6;
        [SerializeField]bool withCenter = true;
        protected void AddOneCircle(int sid)
        {
            var edges = this.indexData.edgeToAdd.CPUData;
            var edgeCount = 0;
            var vBase = this.pCount;
            var center = new float3(UnityEngine.Random.value, UnityEngine.Random.value, 0) * 0.01f;
            var cid = vcount;
            var rand = UnityEngine.Random.value;
            var circle = this.GetCirclePoint(vcount, this.radius);
            foreach(var i in Enumerable.Range(0,vcount))
            {
                var i1 = i;
                var i2 = (i + 1) % vcount;
                edges[edgeCount++] = new EdgeToAdd(sid, i1 + vBase, i2 + vBase, circle[i1] + center, circle[i2] + center);
            }
            if(this.withCenter)
            {
                foreach (var i in Enumerable.Range(0, vcount))
                {
                    edges[edgeCount++] = new EdgeToAdd(sid, i + vBase, cid + vBase, circle[i] + center, center);
                }
                this.pCount += vcount + 1;
            }
            else
            {
                this.pCount += vcount;
            }

            while (edgeCount < edges.Length) edges[edgeCount++] = new EdgeToAdd(-1, -1, -1, 0, 0);
        }
        protected List<float3> GetCirclePoint(int vcount, float radius = 1)
        {
            var ret = new List<float3>();

            foreach(var i in Enumerable.Range(0,vcount))
            {
                var rad = i * 1f / vcount * 2 * math.PI; 
                var x = math.cos(rad) * radius;
                var y = math.sin(rad) * radius;
                ret.Add(new float3(x,y,0));
            }
            return ret;
        }
        protected void AddH2O()
        {
            var angle = 104.5f;
            var edges = this.indexData.edgeToAdd.CPUData;
            var edgeCount = 0;
            var vBase = this.pCount;
            var vcount = 3;
            var y = 0.1f;
            var x = math.tan(angle * 0.5f * Mathf.Deg2Rad) * y;
            var center = new float3(UnityEngine.Random.value, UnityEngine.Random.value, 0) * 0.01f;
            var rand = UnityEngine.Random.rotationUniform;
            // rand = Quaternion.Euler(0, 0, UnityEngine.Random.value * 2 * Mathf.PI * Mathf.Rad2Deg);
            var p1 = rand * new Vector3(x,y,0);
            var p2 = rand * new Vector3(-x,y,0);
            edges[edgeCount++] = new EdgeToAdd(sid, vBase, vBase + 1, center, center + new float3(p1));
            edges[edgeCount++] = new EdgeToAdd(sid, vBase, vBase + 2, center, center + new float3(p2));
            while (edgeCount < edges.Length) edges[edgeCount++] = new EdgeToAdd(-1, -1, -1, 0, 0);

            this.sid++;
            this.pCount += vcount;
            this.dispatcher.Dispatch(Kernel.AddEdge, this.data.edgeCount);
        }
        
        protected void AddHex()
        {

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
            if(Input.GetKeyDown(KeyCode.H)) this.AddH2O();
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