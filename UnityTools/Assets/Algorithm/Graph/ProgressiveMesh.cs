using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityTools.Common;
using UnityTools.Debuging;

namespace UnityTools.Algorithm
{
    public class ProgressiveMesh
    {
        public struct Cost
        {
            public float cost;
            public IVertex to;
        }
        public interface IVertex
        {
            Cost CostTo { get; set; }
            float3 Position { get; }
        }
        public static void CalculateVerticeWeight(INewGraph graph)
        {
            debug.Clear();
            foreach (var v in graph.Vertices)
            {
                LogTool.AssertIsTrue(v is IVertex);
                UpdateEdgeCost(graph, v as IVertex);
            }
        }

        public static void RemoveMinCostVertex(INewGraph graph)
        {
            var min = graph.Vertices.Cast<IVertex>().OrderBy(v => v.CostTo.cost).FirstOrDefault();
            if (min == default) return;

            MergeVertexTo(graph, min, min.CostTo.to);
        }


        protected static void MergeVertexTo(INewGraph graph, IVertex from, IVertex to)
        {
            LogTool.AssertIsFalse(from.Equals(to));
            var fromNeighbors = graph.GetNeighborVertices(from as Common.IVertex).ToList();

            foreach (var n in fromNeighbors)
            {
                var e = graph.GetEdge(from as Common.IVertex, n);
                LogTool.AssertNotNull(e);
                graph.Remove(e);

                if (n.Equals(to)) continue;
                graph.AddEdge(n, to as Common.IVertex);

            }
            graph.Remove(from as Common.IVertex);

            foreach (var n in fromNeighbors)
            {
                UpdateEdgeCost(graph, n as IVertex);
            }
        }

        public static List<(float3, float3)> debug = new List<(float3, float3)>();

        

        protected static void UpdateEdgeCost(INewGraph g, IVertex u)
        {
            // u.CostTo = new Cost() { cost = float.MaxValue, to = default };
            // foreach (IVertex v in g.GetNeighborVertices(u as Common.IVertex))
            // {
            //     var ret = 0f;
            //     var len = math.distance(u.Position, v.Position);


            //     var sides = new List<DualGraph.Face>();
            //     foreach(DualGraph.Face f in (u as VertexGraph.Vertex).Face)
            //     {
            //         if(f.ContainsGeometryVertex(v as Common.IVertex))
            //         {
            //             sides.Add(f);
            //         }
            //     }

            //     foreach(DualGraph.Face f in (u as VertexGraph.Vertex).Face)
            //     {
            //         var mincurv = 1f;
            //         foreach(var s in sides)
            //         {
            //             var unormal = f.Normal;
            //             var uvnormal = s.Normal;
            //             var ndot = math.dot(unormal, uvnormal);
            //             mincurv = math.min(mincurv, (1 - ndot) / 2f);
            //         }
            //         ret = math.max(ret, mincurv);
            //         ret = len * ret;
            //     }
            //     if (ret < u.CostTo.cost)
            //     {
            //         u.CostTo = new Cost() { cost = ret, to = v };
            //     }
            // }
        }

    }
}