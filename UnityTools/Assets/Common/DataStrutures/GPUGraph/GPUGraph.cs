using UnityEngine;
using UnityTools.ComputeShaderTool;
using UnityTools.Debuging;
using UnityTools.Rendering;

namespace UnityTools.Common
{
    public class NodeBase
    {
        public bool active;
    }
    public class EdgeBase
    {
        public bool active;
        public int from;
        public int to;
    }

    public class GPUGraph<Node, Edge, Kernel> : MonoBehaviour
                                        where Node : NodeBase 
                                        where Edge : EdgeBase
                                        where Kernel: System.Enum
    {
        [System.Flags]
        public enum DrawMode
        {
            None = 0,
            Edge = 1, 
            Node = 2,
            All = ~None,
        }
        public enum DefaultKernel
        {
            InitNode,
            InitEdge,
        }
        [System.Serializable]
        public class GPUData : GPUContainer
        {
            [Shader(Name = "_NodeCount")] public int nodeCount = 128;
            [Shader(Name = "_EdgeCount")] public int edgeCount = 128;
            [Shader(Name = "_NodeBuffer")] public GPUBufferVariable<Node> nodeBuffer = new GPUBufferVariable<Node>();
            [Shader(Name = "_EdgeBuffer")] public GPUBufferVariable<Edge> edgeBuffer = new GPUBufferVariable<Edge>();
            [Shader(Name = "_NodeIndexBuffer")] public GPUBufferVariable<int> nodeIndexBuffer = new GPUBufferVariable<int>();
            [Shader(Name = "_EdgeIndexBuffer")] public GPUBufferVariable<int> edgeIndexBuffer = new GPUBufferVariable<int>();
            [Shader(Name = "_NodeIndexBufferConsume")] public GPUBufferVariable<int> nodeIndexBufferConsume = new GPUBufferVariable<int>();
            [Shader(Name = "_EdgeIndexBufferConsume")] public GPUBufferVariable<int> edgeIndexBufferConsume = new GPUBufferVariable<int>();
            public GPUBufferVariable<uint> edgeIndirectBuffer = new GPUBufferVariable<uint>();
            public GPUBufferVariable<uint> nodeIndirectBuffer = new GPUBufferVariable<uint>();
            [Shader(Name = "_LineScale")] public float lineScale = 1;
            [Shader(Name = "_NodeScale")] public float nodeScale = 1;
        }
        [SerializeField] protected DrawMode drawMode = DrawMode.All;
        [SerializeField] protected ComputeShader computeShader;
        [SerializeField] protected GPUData data = new GPUData();
        [SerializeField] protected Shader edgeShader;
        [SerializeField] protected Shader nodeShader;
        [SerializeField] protected DisposableMaterial edgeMaterial;
        [SerializeField] protected DisposableMaterial nodeMaterial;
        [SerializeField] protected Mesh edgeMesh;
        [SerializeField] protected Mesh nodeMesh;

        protected ComputeShaderDispatcher<DefaultKernel> defaultDispatcher;
        protected ComputeShaderDispatcher<Kernel> dispatcher;
        protected virtual void Init()
        {
            this.InitBuffer();
            this.InitDispatcher();
            this.InitNodeAndEdge();

            this.InitRender();
        }

        protected void InitBuffer()
        {
            this.data.nodeBuffer.InitBuffer(this.data.nodeCount, false);
            this.data.edgeBuffer.InitBuffer(this.data.edgeCount, false);

            this.data.nodeIndexBuffer.InitBuffer(this.data.nodeCount, false, false, ComputeBufferType.Append);
            this.data.edgeIndexBuffer.InitBuffer(this.data.edgeCount, false, false, ComputeBufferType.Append);

            this.data.nodeIndexBufferConsume.InitBuffer(this.data.nodeIndexBuffer);
            this.data.edgeIndexBufferConsume.InitBuffer(this.data.edgeIndexBuffer);

            this.data.edgeIndirectBuffer.InitBuffer(5, true, false, ComputeBufferType.IndirectArguments);
            this.data.nodeIndirectBuffer.InitBuffer(5, true, false, ComputeBufferType.IndirectArguments);
        }

        protected void InitDispatcher()
        {
            this.defaultDispatcher = new ComputeShaderDispatcher<DefaultKernel>(this.computeShader);
            foreach(DefaultKernel e in System.Enum.GetValues(typeof(DefaultKernel)))
            {
                this.defaultDispatcher.AddParameter(e, this.data);
            }

            this.dispatcher = new ComputeShaderDispatcher<Kernel>(this.computeShader);
            foreach(Kernel e in System.Enum.GetValues(typeof(Kernel)))
            {
                this.dispatcher.AddParameter(e, this.data);
            }
        }

        protected void InitNodeAndEdge()
        {
            this.data.nodeIndexBuffer.Data.SetCounterValue(0);
            this.defaultDispatcher.Dispatch(DefaultKernel.InitNode, this.data.nodeCount);
            this.data.edgeIndexBuffer.Data.SetCounterValue(0);
            this.defaultDispatcher.Dispatch(DefaultKernel.InitEdge, this.data.edgeCount);
        }

        protected void InitRender()
        {
            if(this.nodeMesh == null)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                this.nodeMesh = go.GetComponent<MeshFilter>().mesh;
                go.DestoryObj();
            }
            if(this.edgeMesh == null)
            {
                this.edgeMesh = this.GenerateEdgeMesh();
            }


            this.edgeMaterial = new DisposableMaterial(this.edgeShader);
            this.nodeMaterial = new DisposableMaterial(this.nodeShader);

            this.SetupIndirectBuffer(this.data.edgeIndirectBuffer, this.edgeMesh, this.data.edgeCount);
            this.SetupIndirectBuffer(this.data.nodeIndirectBuffer, this.nodeMesh, this.data.nodeCount);
        }


        protected void SetupIndirectBuffer(GPUBufferVariable<uint> buffer, Mesh mesh, int count)
        {
            var args = buffer.CPUData;
            var subIndex = 0;
            args[0] = (uint)mesh.GetIndexCount(subIndex);
            args[1] = (uint)count;
            args[2] = (uint)mesh.GetIndexStart(subIndex);
            args[3] = (uint)mesh.GetBaseVertex(subIndex);
            buffer.SetToGPUBuffer();
        }

        protected Mesh GenerateEdgeMesh()
        {
            var mesh = new Mesh();
            mesh.name = "Edge Mesh";
            mesh.vertices = new Vector3[2] { Vector3.zero, Vector3.right };
            mesh.uv = new Vector2[2] { new Vector2(0f, 0f), new Vector2(0f, 1f) };
            mesh.SetIndices(new int[2] { 0, 1 }, MeshTopology.Lines, 0);
            return mesh;
        }

        protected virtual void Deinit()
        {
            this.data?.Release();
            this.edgeMaterial?.Dispose();
            this.nodeMaterial?.Dispose();
        }

        protected virtual void Draw()
        {
            if(this.drawMode.HasFlag(DrawMode.Edge)) this.Draw(this.edgeMesh, this.edgeMaterial, this.data.edgeIndirectBuffer);
            if(this.drawMode.HasFlag(DrawMode.Node)) this.Draw(this.nodeMesh, this.nodeMaterial, this.data.nodeIndirectBuffer);
        }

        protected void Draw(Mesh mesh, Material material, GPUBufferVariable<uint> indirectBuffer)
        {
            if(mesh == null || material == null || indirectBuffer == null) 
            {
                LogTool.Log("Draw buffer is null, nothing to draw", LogLevel.Warning);
                return;
            }
            this.data.UpdateGPU(material);
            var b = new Bounds(Vector3.zero, Vector3.one * 10000);
            Graphics.DrawMeshInstancedIndirect(mesh, 0, material, b, indirectBuffer, 0);
        }

        protected virtual void OnEnable()
        {
            this.Init();
        }

        protected virtual void OnDisable()
        {
            this.Deinit();
        }

        protected virtual void Update()
        {
            this.Draw();
        }

    }

}