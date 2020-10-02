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
        public static INewGraph Generate(INewGraph graph)
        {
            var ret = graph.Factory.CreateGraph();
            var queue = new Queue<IVertex>();
            var visited = new HashSet<IVertex>();
            var map = new Dictionary<IVertex, IVertex>();
            queue.Enqueue(graph.First());

            while(queue.Count > 0)
            {
                var node = queue.Dequeue();
                visited.Add(node);

                if (map.ContainsKey(node) == false)
                {
                    var v1 = node.Clone() as IVertex;
                    ret.Add(v1);
                    map.Add(node, v1);
                }

                var cnode = map[node];

                foreach(var next in graph.GetNeighborVertices(node))
                {
                    if(visited.Contains(next)) continue;

                    visited.Add(next);
                    queue.Enqueue(next);


                    if (map.ContainsKey(next) == false)
                    {
                        var v2 = next.Clone() as IVertex;
                        ret.Add(v2);
                        map.Add(next, v2);
                    }

                    var cnext = map[next];


                    ret.AddEdge(cnode, cnext);

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