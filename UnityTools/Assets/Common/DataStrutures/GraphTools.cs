using System.Collections;
using System.Collections.Generic;
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

    }
}
