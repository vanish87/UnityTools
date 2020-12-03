using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityTools.Debuging;

namespace UnityTools.Common
{
    public interface IHalfEdge : IEdge
    {
        IHalfEdgeFace Face { get; set; }
        IHalfEdge Next { get; set;}
        IHalfEdge Previous { get; set;}
        IHalfEdge Opposite { get; set;}

    }
    public interface IHalfEdgeVertex : IVertex
    {
        IHalfEdge Outgoing { get; set;}
    }
    public interface IHalfEdgeFace: IFace
    {
        IHalfEdge HalfEdge { get; set;}

    }
    public interface IHalfEdgeFactory : IGraphFactory
    {
        IHalfEdgeFace CreateFace();

    }

    public class NoneHalfEdgeObject : IHalfEdge, IHalfEdgeVertex, IHalfEdgeFace
    {
        public static NoneHalfEdgeObject None = new NoneHalfEdgeObject();

        public bool IsDirectional => true;

        public IVertex Vertex { get => None; set => throw new System.NotImplementedException(); }
        public IVertex OtherVertex { get => None; set => throw new System.NotImplementedException(); }
        IHalfEdge IHalfEdgeVertex.Outgoing { get => None; set => throw new NotImplementedException(); }
        IHalfEdgeFace IHalfEdge.Face { get => None; set => throw new NotImplementedException(); }
        IHalfEdge IHalfEdge.Next { get => None; set => throw new NotImplementedException(); }
        IHalfEdge IHalfEdge.Previous { get => None; set => throw new NotImplementedException(); }
        IHalfEdge IHalfEdge.Opposite { get => None; set => throw new NotImplementedException(); }

        IVertex IEdge.Vertex { get => None; set => throw new NotImplementedException(); }
        IVertex IEdge.OtherVertex { get => None; set => throw new NotImplementedException(); }
        IHalfEdge IHalfEdgeFace.HalfEdge { get => None; set => throw new NotImplementedException(); }

        public object Clone()
        {
            return this;
        }

        public bool Equals(IVertex other)
        {
            return this == other;
        }

        public bool Equals(IEdge other)
        {
            return this == other;
        }

        public bool Equals(IFace other)
        {
            return this == other;
        }

        object ICloneable.Clone()
        {
            return this;
        }

        bool IEquatable<IVertex>.Equals(IVertex other)
        {
            return this == other;
        }

    }
    public class HalfEdgeGraph<V, E, F, GraphFactory> : NewGraph<V, E, GraphFactory>
                                                        where V : IHalfEdgeVertex
                                                        where E : IHalfEdge
                                                        where F : IHalfEdgeFace
                                                        where GraphFactory : IHalfEdgeFactory, new()
    {

        public GraphEnum<F> Faces => new GraphEnum<F>(this.faces.Cast<F>().GetEnumerator());
        protected HashSet<IHalfEdgeFace> faces = new HashSet<IHalfEdgeFace>();
        public virtual IHalfEdgeFace GetFace(IEdge edge)
        {
            var e = edge as IHalfEdge;
            while(e.Next != edge)
            {
                if(e.Face != NoneHalfEdgeObject.None) return e.Face;
                e = e.Next;
            }
            return NoneHalfEdgeObject.None;
        }
        public override IEdge AddEdge(IVertex v1, IVertex v2, bool isDirectional = false)
        {
            if (this.GetEdge(v1, v2) != default) return this.GetEdge(v1, v2);
            //Half edge is always directional
            isDirectional = true;
            var v12Edge =  base.AddEdge(v1, v2, isDirectional) as IHalfEdge;
            var v21Edge = base.AddEdge(v2, v1, isDirectional) as IHalfEdge;
            var from = v1 as IHalfEdgeVertex;
            var to = v2 as IHalfEdgeVertex;

            if (from.Outgoing == null) from.Outgoing = NoneHalfEdgeObject.None;
            if (to.Outgoing == null) to.Outgoing = NoneHalfEdgeObject.None;

            if (v12Edge.Face == null) v12Edge.Face = NoneHalfEdgeObject.None;
            if (v12Edge.Next == null) v12Edge.Next = v21Edge;
            if (v12Edge.Opposite == null) v12Edge.Opposite = v21Edge;
            if (v12Edge.Previous == null) v12Edge.Previous = v21Edge;

            if (v21Edge.Face == null) v21Edge.Face = NoneHalfEdgeObject.None;
            if (v21Edge.Next == null) v21Edge.Next = v12Edge;
            if (v21Edge.Opposite == null) v21Edge.Opposite = v12Edge;
            if (v21Edge.Previous == null) v21Edge.Previous = v12Edge;


            if(from.Outgoing != NoneHalfEdgeObject.None)
            {
                LogTool.AssertIsTrue(from.Outgoing != null);
                LogTool.AssertIsTrue(v21Edge.Vertex == to);
                LogTool.AssertIsTrue(v21Edge.OtherVertex == from);
                var prev = from.Outgoing.Previous;
                var next = from.Outgoing.Previous.Next;

                prev.Next = v12Edge;
                v12Edge.Previous = prev;

                next.Previous = v21Edge;
                v21Edge.Next = next;
            }
            if(to.Outgoing != NoneHalfEdgeObject.None)
            {
                LogTool.AssertIsTrue(to.Outgoing != null);
                LogTool.AssertIsTrue(v12Edge.Vertex == from);
                LogTool.AssertIsTrue(v12Edge.OtherVertex == to);

                var prev = to.Outgoing.Previous;
                var next = to.Outgoing.Previous.Next;

                prev.Next = v21Edge;
                v21Edge.Previous = prev;

                next.Previous = v12Edge;
                v12Edge.Next = next;
            }

            from.Outgoing = v12Edge;
            to.Outgoing = v21Edge;

            this.CheckFace(v12Edge);
            this.CheckFace(v21Edge);

            LogTool.AssertIsTrue(v12Edge.Opposite == v21Edge);
            LogTool.AssertIsTrue(v21Edge.Opposite == v12Edge);
            LogTool.AssertNotNull(v12Edge.Face);
            LogTool.AssertNotNull(v21Edge.Face);
            return v12Edge;
        }

        protected void CheckFace(IHalfEdge edge)
        {
            var e = edge.Next;
            var ecount = 0;
            while(e != edge)
            {
                if(e.Face != NoneHalfEdgeObject.None)
                {
                    edge.Face = e.Face;
                    return;
                }
                e = e.Next;
                ecount++;

                if(ecount > 100000)
                {
                    LogTool.Log("Error loop for edge " + edge, LogLevel.Error);
                    return;
                }
            }
            if(ecount >= 3)
            {
                var newFace = (this.Factory as IHalfEdgeFactory).CreateFace();
                newFace.HalfEdge = edge;
                edge.Face = newFace;
                var ret = this.faces.Add(newFace);
                LogTool.AssertIsTrue(ret);

                e = newFace.HalfEdge.Next;
                var str = " ";
                while(e != edge)
                {
                    str += " "+e.ToString();
                    e = e.Next;
                }
                LogTool.Log("Add face "  + str);
            }
        }
    }
}
