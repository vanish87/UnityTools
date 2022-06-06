using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityTools.Common;
using UnityTools.ComputeShaderTool;

namespace UnityTools.Rendering
{
	public class GPUMeshExample : MonoBehaviour, IInitialize
	{
        [System.Serializable]
        public class GPUData : GPUContainer
        {

        }
        [SerializeField] protected GPUData gpuData = new GPUData();
        [SerializeField] protected ComputeShader cs;
        [SerializeField] protected Mesh sourceMesh;
        [SerializeField] protected Mesh mesh;
        [SerializeField] protected ShaderMaterial shader;
        protected ComputeShaderDispatcher dispatcher;
        protected GraphicsBuffer vertexBuffer;

        protected MeshFilter meshFilter;
        protected MeshRenderer meshRenderer;
        

		public bool Inited => this.intied;
        protected bool intied = false;

		public void Init(params object[] parameters)
		{
            if(this.Inited) return;

            this.shader?.Init();

            this.dispatcher = new ComputeShaderDispatcher(this.cs);
            this.dispatcher.AddParameter("Update", this.gpuData);

            this.mesh = new Mesh();
            this.HandleMesh();

            this.mesh.indexBufferTarget |= GraphicsBuffer.Target.Raw;
            this.mesh.vertexBufferTarget |= GraphicsBuffer.Target.Raw;

            // this.mesh.vertices = this.sourceMesh.vertices;
			// this.mesh.SetIndices(this.sourceMesh.GetIndices(0), this.sourceMesh.GetTopology(0), 0);

            var vdesc = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3);
            var uvdesc = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2);
            this.mesh.SetVertexBufferParams(this.mesh.vertexCount, vdesc, uvdesc);
            
            Debug.Log(SystemInfo.SupportsVertexAttributeFormat(VertexAttributeFormat.Float32, 2));

            this.vertexBuffer = this.mesh.GetVertexBuffer(0);

            this.meshFilter = this.GetComponent<MeshFilter>();
            this.meshFilter.sharedMesh = this.mesh;

            this.meshRenderer = this.GetComponent<MeshRenderer>();
            this.meshRenderer.sharedMaterial = this.shader;
		}

		public void Deinit(params object[] parameters)
		{
            this.shader?.Deinit();

            this.vertexBuffer?.Dispose();
            this.gpuData?.Release();
            this.mesh?.DestoryObj();
		}
        protected void HandleMesh()
        {
            var newVertices = new List<Vector3>();
            var newIndices = new List<int>();
            var newUVs = new List<Vector2>();

            var m = this.sourceMesh;
            var added = new Dictionary<Vector3, int>();
            var uvs = new Dictionary<Vector3, Vector2>();
            var indexCount = 0;
            for (var t = 0; t < m.triangles.Length; t += 3)
            {
                var v1 = m.vertices[m.triangles[t]];
                var v2 = m.vertices[m.triangles[t + 1]];
                var v3 = m.vertices[m.triangles[t + 2]];

                var uv1 = m.uv[m.triangles[t]];
                var uv2 = m.uv[m.triangles[t+1]];
                var uv3 = m.uv[m.triangles[t+2]];

                int p1;
                int p2;
                int p3;
                if (!added.TryGetValue(v1, out p1))
                {
                    p1 = indexCount++;
                    added.Add(v1, p1);
                    uvs.Add(v1, uv1);

                    newVertices.Add(v1);
					newUVs.Add(uv1);
                }
                if (!added.TryGetValue(v2, out p2))
                {
                    p2 = indexCount++;
                    added.Add(v2, p2);
                    uvs.Add(v2, uv2);

                    newVertices.Add(v2);
					newUVs.Add(uv2);
                }
                if (!added.TryGetValue(v3, out p3))
                {
                    p3 = indexCount++;
                    added.Add(v3, p3);
                    uvs.Add(v3, uv3);

                    newVertices.Add(v3);
					newUVs.Add(uv3);
                }

                newIndices.Add(p1);
                newIndices.Add(p2);
                newIndices.Add(p3);

            }

            this.mesh.vertices = newVertices.ToArray();
            this.mesh.uv = newUVs.ToArray();
            this.mesh.SetIndices(newIndices.ToArray(), MeshTopology.Triangles, 0);
            this.mesh.RecalculateBounds();
            // this.mesh.RecalculateNormals();

            Debug.Log("Source vertice num " + m.vertexCount);
            Debug.Log("New Mesh vertice num " + this.mesh.vertexCount);

            Debug.Log("Source indices num " + m.GetIndexCount(0));
            Debug.Log("New Mesh indices num " + this.mesh.GetIndexCount(0));

        }
        protected void OnEnable()
        {
            this.Init();

            this.cs.SetBuffer(0, "_VertexBuffer", this.vertexBuffer);
            this.cs.Dispatch(0, this.mesh.vertexCount, 1, 1);
            this.mesh.RecalculateBounds();
        }
        protected void OnDisable()
        {
            this.Deinit();

        }
        protected void Update()
        {

        }
	}
}