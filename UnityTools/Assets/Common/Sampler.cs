using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityTools.Debuging;
using UnityTools.Math;

namespace UnityTools.Common
{
	public interface ISampler<Value, Input>
	{
		Value Evaluate(Input input);
		IEnumerable<(Input, Value)> Sample(ISpace space, int3 size);
	}
	[ExecuteInEditMode]
	public class Sampler
	{
		static private List<float3> Sample(ISpace space, float3 basePos, int3 size)
		{
			var ret = new List<float3>();
			var offset = new float3(1.0f / math.max(size.x - 1, 1), 1.0f / math.max(size.y - 1, 1), 1.0f / math.max(size.z - 1, 1));

			foreach (var i in Enumerable.Range(0, size.x))
			{
				foreach (var j in Enumerable.Range(0, size.y))
				{
					foreach (var k in Enumerable.Range(0, size.z))
					{
						var p = space.TRS.MultiplyPoint(basePos + offset * new float3(i, j, k));
						ret.Add(p);
					}
				}
			}
			return ret;
		}

		static public List<float3> SampleMeshSurface(Mesh mesh, int sampleNum = 1024)
		{
			var ret = new List<float3>();

			if(mesh == null || mesh.GetTopology(0) != MeshTopology.Triangles)
			{
				LogTool.Log("Invalid mesh", LogLevel.Warning);
				return ret;
			}

			var triangles = mesh.triangles;

			var area = new List<float>();
			var minArea = float.MaxValue;

			foreach (var idx in Enumerable.Range(0, triangles.Length / 3))
			{
				var pa = mesh.vertices[triangles[idx*3]];
				var pb = mesh.vertices[triangles[idx*3+1]];
				var pc = mesh.vertices[triangles[idx*3+2]];
				var A = pa-pc;
				var B = pb-pc;
				var a = 0.5f * math.length(math.cross(A, B));
				area.Add(a);

				minArea = math.min(minArea, a);
			}

			foreach (var i in Enumerable.Range(0, area.Count)) area[i] /= minArea;

			var weights = area.Select(v=>Mathf.CeilToInt(v)).ToList();
			var rand = new WeightRandom(weights);

			while(sampleNum-->0)
			{
				var idx = rand.Random();

				var pa = mesh.vertices[triangles[idx*3]];
				var pb = mesh.vertices[triangles[idx*3+1]];
				var pc = mesh.vertices[triangles[idx*3+2]];

				var u = UnityEngine.Random.value;
				var v = UnityEngine.Random.value;
				if (u + v > 1) { u = 1 - u; v = 1 - v; }

				var p = u * pa + v * pb + (1 - (u + v)) * pc;

				ret.Add(p);
			}

			return ret;
		}

		static public List<float3> SampleSphereSurface(ISpace space, float density = 1f / 32)
		{
			var ret = new List<float3>();
			// var radius = space.Scale.x / 2;
			// var center = space.Center;
			var radius = 0.5f;
			var center = 0;
			var num = Mathf.CeilToInt(space.Scale.x / density);

			foreach(var theta in Enumerable.Range(0, num))
			{
				foreach(var phi in Enumerable.Range(0, num))
				{
					var t = theta * 1.0f / math.max(num - 1, 1) * math.PI;
					var p = phi * 1.0f / math.max(num - 1, 1) * math.PI * 2;
					var np = new float3(math.cos(p) * math.sin(t), math.sin(p) * math.sin(t), math.cos(t)) * radius;
					ret.Add(np + center);
				}
			}
			return ret;
		}
		static public List<float3> SampleXY(ISpace space, int depth = 1, float density = 1f / 32f)
		{
			var bpos = new float3(-0.5f, -0.5f, depth == 1 ? 0 : -0.5f);
			var size = new float3(space.Scale / density);
			var ns = new int3(Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y), depth);

			return Sample(Space.IdentitySpace, bpos, ns);
		}
		static public List<float3> SampleXZ(ISpace space, int depth = 1, float density = 1f / 32f)
		{
			var bpos = new float3(-0.5f, depth == 1 ? 0 : -0.5f, -0.5f);
			var size = new float3(space.Scale / density);
			var ns = new int3(Mathf.CeilToInt(size.x), depth, Mathf.CeilToInt(size.z));

			return Sample(space, bpos, ns);
		}
		static public List<float3> SampleYZ(ISpace space, int depth = 1, float density = 1f / 32f)
		{
			var bpos = new float3(depth == 1 ? 0 : -0.5f, -0.5f, -0.5f);
			var size = new float3(space.Scale / density);
			var ns = new int3(depth ,Mathf.CeilToInt(size.y), Mathf.CeilToInt(size.z));

			return Sample(space, bpos, ns);
		}

	}

}