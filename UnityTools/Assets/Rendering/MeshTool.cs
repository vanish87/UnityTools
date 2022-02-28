using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityTools.Common;
using UnityTools.Interaction;

namespace UnityTools.Rendering
{
	public class GeometryTool
	{
		public class GeometryDescriptor
		{
			public static GeometryDescriptor EmptyGeometry(PrimitiveType type)
			{
				return new GeometryDescriptor()
				{
					type = type,
					space = Common.Space.IdentitySpace,
					autoSpace = false,
					enableTouch = false,
					hasCollider = false,
					shader = null,
					target = null,
				};
			}

			public PrimitiveType type = PrimitiveType.Quad;
			public ISpace space;
			public bool autoSpace = true;
			public bool enableTouch = true;
			public Shader shader;
			public bool hasCollider;
			public Texture target;
		}
		public static GameObject Create(GeometryDescriptor descriptor)
		{
			var go = GameObject.CreatePrimitive(descriptor.type);
			go.name = descriptor.type.ToString();
			
			if(!descriptor.hasCollider)
			{
				var collider = go.GetComponent<Collider>();
				GameObject.Destroy(collider);
			}

			var mat = go.FindOrAddTypeInComponentsAndChildren<AutoMaterial>();
			mat.Init(descriptor.shader);

			var autoSpace = go.FindOrAddTypeInComponentsAndChildren<TextureAutoSpace>();
			autoSpace.Init(descriptor.space, descriptor.autoSpace, descriptor.target);
			autoSpace.Update();

			if(descriptor.enableTouch) go.FindOrAddTypeInComponentsAndChildren<TouchMesh>();

			return go;
		}
	}
	public class MeshTool
	{
		public enum MeshType
		{
			Quad,
			Cube,
			Sphere,
			Circle,
		}
		public static Mesh Generate(MeshType type)
		{
			var mesh = new Mesh();
			mesh.name = type.ToString();

			switch (type)
			{
				case MeshType.Quad:
					{
						var vertices = new Vector3[4]
						{
							new Vector3(-0.5f, -0.5f, 0f),
							new Vector3(-0.5f, 0.5f, 0f),
							new Vector3(0.5f, 0.5f, 0f),
							new Vector3(0.5f, -0.5f, 0f),
						};
						var indices = new int[6]
						{
							0, 1, 2,
							0, 2, 3
						};
						var uv = new Vector2[4]
						{
							new Vector2(0, 0),
							new Vector2(1, 0),
							new Vector2(1, 1),
							new Vector2(0, 1),
						};

						mesh.vertices = vertices;
						mesh.SetIndices(indices, MeshTopology.Triangles, 0);
						mesh.uv = uv;

					}
					break;
				case MeshType.Cube:
					{
						var vertices = new Vector3[8]
						{
							new Vector3(-0.5f, -0.5f, -0.5f),
							new Vector3( 0.5f, -0.5f, -0.5f),
							new Vector3( 0.5f,  0.5f, -0.5f),
							new Vector3(-0.5f,  0.5f, -0.5f),

							new Vector3(-0.5f, -0.5f, 0.5f),
							new Vector3( 0.5f, -0.5f, 0.5f),
							new Vector3( 0.5f,  0.5f, 0.5f),
							new Vector3(-0.5f,  0.5f, 0.5f),
						};
						var indices = new int[6]
						{
							0, 1, 2,
							0, 2, 3,

							// 1, 6, 2, 
							// 1, 5, 6,


						};
						var uv = new Vector2[4]
						{
							new Vector2(0, 0),
							new Vector2(1, 0),
							new Vector2(1, 1),
							new Vector2(0, 1),
						};

						mesh.vertices = vertices;
						mesh.SetIndices(indices, MeshTopology.Triangles, 0);
						mesh.uv = uv;

					}
					break;
				case MeshType.Circle:
					{
						var vertices = new List<Vector3>();
						var div = 16;
						foreach(var i in Enumerable.Range(0, div))
						{
							var rad = Mathf.Lerp(0, 2f * Mathf.PI, i * 1.0f / div);
							vertices.Add(new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0));
						}


					}
					break;
				default: break;
			}

			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.RecalculateTangents();

			return mesh;

		}
	}
}
