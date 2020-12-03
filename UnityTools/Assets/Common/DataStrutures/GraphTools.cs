using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityTools.Common
{
    public class GraphTools
    {
        public static void Traverse(INewGraph graph)
        {

        }

        public static bool HasCircle(INewGraph graph)
        {
            var queue = new Queue<IVertex>();
            var visited = new HashSet<IVertex>();
            var parent = new Dictionary<IVertex, IVertex>();
            queue.Enqueue(graph.First());
            while(queue.Count > 0)
            {
                var node = queue.Dequeue();
                visited.Add(node);
                foreach(var next in graph.GetNeighborVertices(node))
                {                        
                    if(visited.Contains(next)) continue;
                    queue.Enqueue(next);

                    var px = Find(parent, node);
                    var py = Find(parent, next);
                    if(px.Equals(py)) return true;

                    Union(parent, node, next);
                }
            }
            return false;
        }

        public static bool HasCircle(IEnumerable<IEdge> edges)
        {
            var parent = new Dictionary<IVertex, IVertex>();
            foreach(var e in edges)
            {
                var px = Find(parent, e.Vertex);
                var py = Find(parent, e.OtherVertex);
                if(px.Equals(py)) return true;

                Union(parent, e.Vertex, e.OtherVertex);
            }

            return false;
        }

        public static T Find<T>(Dictionary<T, T> parent, T x)
        {
            if(!parent.ContainsKey(x))
            {
                parent.Add(x,x);
                return x;
            }
            else
            {
                if(parent[x].Equals(x)) return x;

                parent[x] = Find(parent, parent[x]);
                return parent[x];
            }
        }

        public static void Union<T>(Dictionary<T,T> parent, T x, T y)
        {
            var px = Find(parent, x);
            var py = Find(parent, y);

            if(!px.Equals(py)) parent[px] = py;
        }

        public static bool SameParent<T>(Dictionary<T,T> parent, T x, T y)
        {
            return Find(parent, x).Equals(Find(parent, y));
        }


        public static IPath GetPath(INewGraph graph, IVertex from, IVertex to)
        {
            var ret = new Path();
            var visited = new HashSet<IVertex>();

            var found = GetPathDFS(graph, from, to, ret, visited);

            if (found) return ret;
            return default;
        }

        private static bool GetPathDFS(INewGraph g, IVertex n, IVertex to, IPath current, HashSet<IVertex> visited)
        {
            visited.Add(n);
            current.Append(n);

            if (n.Equals(to)) return true;

            foreach (var next in g.GetNeighborVertices(n))
            {
                if (visited.Contains(next)) continue;

                if(GetPathDFS(g, next, to, current, visited)) return true;
                current.Remove(next);
            }

            return false;

        }

    }
}
