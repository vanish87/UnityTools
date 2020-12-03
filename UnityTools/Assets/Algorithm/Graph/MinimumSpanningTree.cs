using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityTools.Common;
using UnityTools.Debuging;

namespace UnityTools.Algorithm
{
    public enum TraverseType
    {
        DFS,
        BFS,
    }
    public class MinimumSpanningTree
    {
        public static INewGraph Generate(INewGraph graph, IVertex first = null)
        {
            var ret = graph.Factory.CreateGraph();
            var queue = new Queue<IVertex>();
            var visited = new HashSet<IVertex>();
            queue.Enqueue(first == null ? graph.First() : first);

            while(queue.Count > 0)
            {
                var node = queue.Dequeue();
                visited.Add(node);

                var v1 = ret.AddVertex(node.Clone() as IVertex);

                foreach(var next in graph.GetNeighborVertices(node))
                {
                    if (visited.Contains(next)) continue;

                    visited.Add(next);
                    queue.Enqueue(next);

                    var v2 = ret.AddVertex(next.Clone() as IVertex);

                    ret.AddEdge(v1, v2);

                }
            }
            return ret;
        }

        public static INewGraph KruskalMST(INewGraph graph)
        {
            LogTool.AssertIsTrue(graph.Edges.First() is IWeightedEdge);

            var ret = graph.Factory.CreateGraph();
            var edges = graph.Edges.OrderBy(e => (e as IWeightedEdge).Weight);
            var currentEdges = new List<IEdge>();

            foreach(var e in edges)
            {
                currentEdges.Add(e);
                if(GraphTools.HasCircle(currentEdges))
                {
                    currentEdges.RemoveAt(currentEdges.Count-1);
                }
                else
                {
                    var nv1 = ret.AddVertex(e.Vertex.Clone() as IVertex);
                    var nv2 = ret.AddVertex(e.OtherVertex.Clone() as IVertex);
                    ret.AddEdge(nv1, nv2);
                }
            }

            return ret;
        }
        // public static Tree<Node> SpanningTree<Node, Edge>(IGraph<Node, Edge> graph, TraverseType type = TraverseType.DFS) where Node : new()
        // {

        // }

        // public static IGraph<Node, Edge> DFS<Node, Edge>(IGraph<Node, Edge> graph) where Node : INode where Edge: IEdge, new()
        // {
        //     var ret = graph.DeepCopy();
        //     ret.Clear();

        //     var newNodes = ret.Nodes.ToList();
        //     var visited = new HashSet<Node>();
        //     var stack = new Stack<Node>();
        //     stack.Push(graph.Nodes.First());

        //     while (stack.Count > 0)
        //     {
        //         var node = stack.Pop();
        //         visited.Add(node);

        //         newNodes[node.Index] = node.DeepCopy();
        //         // LogTool.Log(node.Index.ToString());

        //         foreach (var next in graph.GetNeighborsNodes(node))
        //         {
        //             if (visited.Contains(next)) continue;

        //             var edge = new Edge();
        //             ret.AddEdge(node, next, edge);
        //             stack.Push(next);
        //         }
        //     }
        //     return ret;
        // }

    }
}