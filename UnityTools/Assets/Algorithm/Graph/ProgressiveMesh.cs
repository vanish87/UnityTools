using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityPaperModel;
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
            u.CostTo = new Cost() { cost = float.MaxValue, to = default };
            foreach (IVertex v in g.GetNeighborVertices(u as Common.IVertex))
            {
                var ret = 0f;
                var len = math.distance(u.Position, v.Position);

                var uList = new HashSet<IVertex>(g.GetNeighborVertices(u as Common.IVertex).Cast<IVertex>());
                var vList = new HashSet<IVertex>(g.GetNeighborVertices(v as Common.IVertex).Cast<IVertex>());
                var uvOnly = new HashSet<IVertex>() { u, v };
                var uvList = new HashSet<IVertex>(uList);
                uvList.IntersectWith(vList);
                uvList.ExceptWith(uvOnly);

                uList.ExceptWith(uvOnly);
                vList.ExceptWith(uvOnly);

                var ul = uList.ToList();
                var i = 0;
                // for (var i = 0; i < ul.Count; ++i)
                {
                    for (var j = i + 1; j < ul.Count; ++j)
                    {
                        var nu1 = ul[i];
                        var nu2 = ul[j];
                        if (nu1.Equals(v) || nu2.Equals(v)) continue;
                        if (g.GetEdge(nu1 as Common.IVertex, nu2 as Common.IVertex) == default) continue;
                        var mincurv = 1f;
                        foreach (var nv in uvList)
                        {
                            LogTool.AssertIsFalse(nv.Equals(u));
                            LogTool.AssertIsFalse(nv.Equals(v));
                            var shared = nv;
                            var unormal = float3.zero;
                            var pos = float3.zero;
                            if(nu1.Equals(shared) || nu2.Equals(shared))
                            {
                                var other = nu1.Equals(shared) ? nu2 : nu1;
                                unormal = math.normalize(math.cross(other.Position - u.Position, shared.Position - u.Position));

                                pos = other.Position + shared.Position + u.Position;
                            }
                            else
                            {
                                unormal = math.normalize(math.cross(nu1.Position - u.Position, nu2.Position - u.Position));

                                pos = nu1.Position + nu2.Position + u.Position;
                            }
                            var uvnormal = math.normalize(math.cross(shared.Position - v.Position, shared.Position - u.Position));

                            // unormal = DualGraphMono.normals[(shared as Common.IVertex, other as Common.IVertex, u as Common.IVertex)];
                            // uvnormal = DualGraphMono.normals[(shared as Common.IVertex, v as Common.IVertex, u as Common.IVertex)];

                            pos /= 3;
                            debug.Add((pos, unormal));
                            pos = shared.Position + v.Position + u.Position;
                            pos /= 3;
                            // debug.Add((pos, uvnormal));

                            var ndot = math.dot(unormal, uvnormal);
                            mincurv = math.min(mincurv, (1 - ndot) / 2f);
                        }
                        ret = math.max(ret, mincurv);

                    }
                }
                ret = len * ret;
                if (ret < u.CostTo.cost)
                {
                    u.CostTo = new Cost() { cost = ret, to = v };
                }
            }
        }

    }
}