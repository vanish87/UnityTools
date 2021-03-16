using UnityEngine;
using UnityTools.ComputeShaderTool;

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

        }

        [SerializeField] protected ComputeShader computeShader;
        [SerializeField] protected GPUData data = new GPUData();
        [SerializeField] protected Shader shader;
        [SerializeField] protected Shader nodeShader;
        [SerializeField] protected DisposableMaterial material;
        [SerializeField] protected DisposableMaterial nodeMaterial;
        [SerializeField] protected Mesh lineMesh;
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

            this.data.nodeIndexBuffer.InitBuffer(this.data.nodeCount, false, ComputeBufferType.Append);
            this.data.edgeIndexBuffer.InitBuffer(this.data.edgeCount, false, ComputeBufferType.Append);

            this.data.nodeIndexBufferConsume.InitBuffer(this.data.nodeIndexBuffer);
            this.data.edgeIndexBufferConsume.InitBuffer(this.data.edgeIndexBuffer);

            this.data.edgeIndirectBuffer.InitBuffer(5, true, ComputeBufferType.IndirectArguments);
            this.data.nodeIndirectBuffer.InitBuffer(5, true, ComputeBufferType.IndirectArguments);
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
            this.lineMesh = this.GenerateLineMesh();

            this.material = new DisposableMaterial(this.shader);
            this.nodeMaterial = new DisposableMaterial(this.nodeShader);

            var args = this.data.edgeIndirectBuffer.CPUData;
            var subIndex = 0;
            args[0] = (uint)this.lineMesh.GetIndexCount(subIndex);
            args[1] = (uint)this.data.edgeCount;
            args[2] = (uint)this.lineMesh.GetIndexStart(subIndex);
            args[3] = (uint)this.lineMesh.GetBaseVertex(subIndex);
            this.data.edgeIndirectBuffer.UpdateBuffer();

            args = this.data.nodeIndirectBuffer.CPUData;
            subIndex = 0;
            args[0] = (uint)this.nodeMesh.GetIndexCount(subIndex);
            args[1] = (uint)this.data.nodeCount;
            args[2] = (uint)this.nodeMesh.GetIndexStart(subIndex);
            args[3] = (uint)this.nodeMesh.GetBaseVertex(subIndex);
            this.data.nodeIndexBuffer.UpdateBuffer();
        }



        protected Mesh GenerateLineMesh()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[2] { Vector3.zero, Vector3.right };
            mesh.uv = new Vector2[2] { new Vector2(0f, 0f), new Vector2(0f, 1f) };
            mesh.SetIndices(new int[2] { 0, 1 }, MeshTopology.Lines, 0);
            return mesh;
        }

        protected virtual void Deinit()
        {
            this.data?.Release();
            this.material?.Dispose();
            this.nodeMaterial?.Dispose();
        }

        protected virtual void Draw()
        {
            Material mat = this.material;
            mat.SetBuffer("_Nodes", this.data.nodeBuffer);
            mat.SetBuffer("_Edges", this.data.edgeBuffer);
            var b = new Bounds(Vector3.zero, Vector3.one * 10000);
            Graphics.DrawMeshInstancedIndirect(this.lineMesh, 0, this.material, b, this.data.edgeIndirectBuffer, 0);

            mat = this.nodeMaterial;
            mat.SetBuffer("_Nodes", this.data.nodeBuffer);
            mat.SetBuffer("_Edges", this.data.edgeBuffer);
            Graphics.DrawMeshInstancedIndirect(this.nodeMesh, 0, this.nodeMaterial, b, this.data.nodeIndirectBuffer, 0);
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