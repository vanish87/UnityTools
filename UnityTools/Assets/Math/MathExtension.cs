
using Unity.Mathematics;
using UnityEngine;

namespace UnityTools.Common
{
	public static class MathExtension
	{
		public static int CeilToInt(this float v)
		{
			return Mathf.CeilToInt(v);
		}
		public static int2 CeilToInt(this float2 v)
		{
			return new int2(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.y));
		}
		public static int3 CeilToInt(this float3 v)
		{
			return new int3(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.y), Mathf.CeilToInt(v.z));
		}
		public static int FloorToInt(this float v)
		{
			return Mathf.FloorToInt(v);
		}
		public static int2 FloorToInt(this float2 v)
		{
			return new int2(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y));
		}
		public static int3 FloorToInt(this float3 v)
		{
			return new int3(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y), Mathf.FloorToInt(v.z));
		}
	}
}
