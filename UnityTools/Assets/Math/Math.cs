using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace UnityTools.Math
{
    public class RandomExtension
    {
        //note: do not use readonly here
        public static Unity.Mathematics.Random R = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);
        public static float2 Rand2 => R.NextFloat2();
        public static float3 Rand3 => R.NextFloat3();
        public static float4 Rand4 => R.NextFloat4();
    }
    public static class Operation
    {
        public static float2x2 OuterProduct(float2 lhs, float2 rhs)
        {
            return new float2x2(lhs[0] * rhs[0], lhs[0] * rhs[1], 
                                lhs[1] * rhs[0], lhs[1] * rhs[1]);
        }

        // public static float3x3 OuterProduct(float3 lhs, float3 rhs)
        // {
        //     return new float3x3(lhs[0] * rhs[0], lhs[0] * rhs[1], lhs[0] * rhs[2],
        //                         lhs[1] * rhs[0], lhs[1] * rhs[1], lhs[1] * rhs[2],
        //                         lhs[2] * rhs[0], lhs[2] * rhs[1], lhs[2] * rhs[2]);

        // }

        public static float3x3 OuterProduct(this float3 lhs, float3 rhs)
        {
            return new float3x3(lhs[0] * rhs[0], lhs[0] * rhs[1], lhs[0] * rhs[2],
                                lhs[1] * rhs[0], lhs[1] * rhs[1], lhs[1] * rhs[2],
                                lhs[2] * rhs[0], lhs[2] * rhs[1], lhs[2] * rhs[2]);

        }
    }
}