using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;
using UnityTools.Common;

namespace UnityTools.Algorithm
{
    public static class Solver
    {
        public static Vector<float3> SolverFS(Matrix<float3> L, Vector<float3> b)
        {
            var dim = L.Size;
            Assert.IsTrue(dim.x == b.Size);

            //Ly = b
            var y = new Vector<float3>(dim.x);

            var m = dim.x;
            for (var i = 0; i < m; ++i)
            {
                var sum = float3.zero;
                for (var j = 0; j < m - 1; ++j)
                {
                    sum += L[i, j] * y[j];
                }
                y[i] = (b[i] + sum) / L[i, i];
            }

            return y;
        }
        public static Vector<float3> SolverBS(Matrix<float3> U, Vector<float3> y)
        {
            var dim = U.Size;
            Assert.IsTrue(dim.x == y.Size);

            //Ux = y
            var x = new Vector<float3>(dim.x);

            var n = dim.x;
            for (var i = 0; i < n; ++i)
            {
                var sum = float3.zero;
                for (var j = i; j < n; ++j)
                {
                    sum += U[i, j] * x[j];
                }
                x[i] = (y[i] - sum) / U[i, i];
            }

            return x;
        }
    }
}