using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityTools.Debuging;

namespace UnityTools.Common
{
    public interface IGraph<Node, Edge>
    {
        IEnumerable<Node> Nodes { get; }
        IEnumerable<Edge> Edges { get; }
        bool IsDirectional { get; }

        Node AddNode();
        Edge GetEdge(Node from, Node to);
        void AddEdge(Node from, Node to, Edge edge);
        IEnumerable<Node> GetNeighborsNodes(Node from);
        IEnumerable<Edge> GetNeighborsEdges(Node from);

        void Clear();
    }
    public interface INode
    {
        int Index { get; set; }
    }

    public interface IEdge
    {

    }

    [System.Serializable]
    public abstract class NewGraph<Node, Edge> : IGraph<Node, Edge> where Node : INode, new() where Edge : IEdge
    {
        protected List<Node> nodeList = new List<Node>();
        protected bool isDirectional = false;

        public abstract IEnumerable<Edge> GetEdges();
        public IEnumerable<Node> Nodes => this.nodeList;
        public IEnumerable<Edge> Edges => this.GetEdges();

        public bool IsDirectional => this.isDirectional;

        public abstract void Clear();
        public abstract Edge GetEdge(Node from, Node to);
        public abstract void AddEdge(Node from, Node to, Edge edge);
        public abstract IEnumerable<Edge> GetNeighborsEdges(Node from);
        public abstract IEnumerable<Node> GetNeighborsNodes(Node from);

        public Node AddNode()
        {
            var node = new Node() { Index = this.nodeList.Count};
            this.nodeList.Add(node);
            return node;
        }
    }
    [System.Serializable]
    public class NewGraphAdj<Node, Edge> : NewGraph<Node, Edge> where Node : INode, new() where Edge : Segment<Node>, IEdge
    {
        protected Matrix<Edge> matrix = null;
        protected int size = 0;

        public NewGraphAdj(int size, bool isDirectional = false) :base()
        {
            this.isDirectional = isDirectional;
            this.size = size;
            this.Clear();
        }

        public void AddEdge(int from, int to, Edge edge)
        {
            this.ChekcIndex(from, to);

            this.matrix[from][to] = edge;
            if(this.isDirectional == false)
            {
                var cedge = edge.DeepCopy();
                cedge.Start = edge.End;
                cedge.End = edge.Start;
                this.matrix[to][from] = cedge;
            }
        }
        public Edge GetEdge(int from, int to)
        {
            this.ChekcIndex(from, to);
            return this.matrix[from][to];
        }
        public IEnumerable<Node> GetNeighborsNodes(int from)
        {
            this.ChekcIndex(from, 0);
            return this.matrix[from].Where(n => n != null).Select(e => e.End);
        }
        public IEnumerable<Edge> GetNeighborsEdges(int from)
        {
            this.ChekcIndex(from, 0);
            return this.matrix[from].Where(n => n != null);
        }

        public override void AddEdge(Node from, Node to, Edge edge)
        {
            this.AddEdge(from.Index, to.Index, edge);
        }

        public override Edge GetEdge(Node from, Node to)
        {
            return this.GetEdge(from.Index, to.Index);
        }

        public override IEnumerable<Edge> GetEdges()
        {
            if (this.isDirectional)
            {
                return this.matrix.Where(e => e != null);
            }
            else
            {
                var ret = new List<Edge>();
                for (var r = 0; r < this.matrix.Size.x; ++r)
                {
                    for (var c = r; c < this.matrix.Size.y; ++c)
                    {
                        var s = this.GetEdge(r, c);
                        if (s == null) continue;
                        ret.Add(s);
                    }
                }
                return ret;
            }
        }

        public override IEnumerable<Edge> GetNeighborsEdges(Node from)
        {
            return this.GetNeighborsEdges(from.Index);
        }

        public override IEnumerable<Node> GetNeighborsNodes(Node from)
        {
            return this.GetNeighborsNodes(from.Index);
        }

        protected void ChekcIndex(int x, int y)
        {
            LogTool.LogAssertIsTrue(0 <= x && x < this.matrix.Size.x, "Invalid Index");
            LogTool.LogAssertIsTrue(0 <= y && y < this.matrix.Size.y, "Invalid Index");
        }

        public override void Clear()
        {
            this.matrix = new Matrix<Edge>(this.size, this.size);
            this.nodeList.Clear();
            foreach (var r in Enumerable.Range(0, size))
            {
                this.AddNode();
            }
        }
    }


    [System.Serializable]
    public class Graph<Node, AdjacentEdge> where Node: INode, new()
    {
        protected List<Node> nodes  = null;
        protected Matrix<AdjacentEdge> matrix = null;
        protected bool isDirectioanl = false;

        public IEnumerable<Node> Nodes { get => this.nodes; }
        public IEnumerable<AdjacentEdge> Edges { get => this.GetAllEdges(); }
        public Matrix<AdjacentEdge> AdjMatrix { get => this.matrix; }

        public Graph(): this(1) { }
        public Graph(int size, bool directional = false)
        {
            this.isDirectioanl = directional;
            this.Resize(size);
        }

        public void Resize(int newSize)
        {
            this.nodes = new List<Node>();
            for (var n = 0; n < newSize; ++n)
            {
                this.nodes.Add(new Node() { Index = n });
            }
            this.matrix = new Matrix<AdjacentEdge>(newSize, newSize);
        }

        public List<Node> GetNeighborsNodes(Node from)
        {
            return this.GetNeighborsNodes(from.Index);
        }

        public List<Node> GetNeighborsNodes(int from)
        {
            var ret = new List<Node>();
            for(var c = 0; c < this.matrix[from].Size; ++c)
            {
                LogTool.LogAssertIsTrue(false, "bug here for directional graph");
                if (this.matrix[from][c] != null) ret.Add(this.nodes[c]);
            }
            return ret;
        }

        public List<AdjacentEdge> GetNeighborsEdges(int from)
        {
            return this.matrix[from].Where(c => c != null).ToList();
        }

        public AdjacentEdge GetEdge(Node from, Node to)
        {
            return this.GetEdge(from.Index, to.Index);
        }

        public AdjacentEdge GetEdge(int from, int to)
        {
            return this.matrix[from, to];
        }

        public IEnumerable<AdjacentEdge> GetAllEdges()
        {
            var ret = new List<AdjacentEdge>();
            for (var r = 0; r < this.AdjMatrix.Size.x; ++r)
            {
                for (var c = this.isDirectioanl?0:r; c < this.AdjMatrix.Size.y; ++c)
                {
                    var s = this.GetEdge(r, c);
                    if (s == null ) continue;
                    ret.Add(s);
                }
            }

            return ret;
        }

        public void AddEdge(Node from, Node to, AdjacentEdge value)
        {
            this.AddEdge(from.Index, to.Index, value);
        }

        public void AddEdge(int from, int to, AdjacentEdge value)
        {
            this.matrix[from, to] = value;
            if (this.isDirectioanl == false) this.matrix[to, from] = value;
        }

        public void Print()
        {
            this.matrix.Print();
        }
    }
}