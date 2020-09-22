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
    public abstract class Graph<Node, Edge> : IGraph<Node, Edge> where Node : INode, new() where Edge : IEdge
    {
        protected List<Node> nodeList = new List<Node>();
        protected int size = 0;
        protected bool isDirectional = false;

        public abstract IEnumerable<Edge> GetEdges();
        public IEnumerable<Node> Nodes => this.nodeList;
        public IEnumerable<Edge> Edges => this.GetEdges();

        public bool IsDirectional => this.isDirectional;

        public virtual void Clear()
        {
            this.nodeList.Clear();
            foreach (var r in Enumerable.Range(0, size))
            {
                this.AddNode();
            }
        }
        public abstract Edge GetEdge(Node from, Node to);
        public abstract void AddEdge(Node from, Node to, Edge edge);
        public abstract IEnumerable<Edge> GetNeighborsEdges(Node from);
        public abstract IEnumerable<Node> GetNeighborsNodes(Node from);

        public Graph(int size, bool isDirectional = false)
        {
            this.isDirectional = isDirectional;
            this.size = size;
        }
        public Node AddNode()
        {
            var node = new Node() { Index = this.nodeList.Count};
            this.nodeList.Add(node);
            return node;
        }
    }
    /// <summary>
    /// WARNING: it has performance issues right now
    /// </summary>
    /// <typeparam name="Node"></typeparam>
    /// <typeparam name="Edge"></typeparam>
    [System.Serializable]
    public class GraphEdgeList<Node, Edge> : Graph<Node, Edge> where Node : INode, new() where Edge : Segment<Node>, IEdge
    {
        protected Dictionary<Node, List<Edge>> edgeList = new Dictionary<Node, List<Edge>>();
        public GraphEdgeList(int size, bool isDirectional = false) : base(size, isDirectional)
        {
            this.Clear();
        }

        public void AddEdge(int from, int to, Edge edge)
        {
            if (this.Edges.Contains(edge))
            {
                LogTool.Log("Edge From " + edge.Start.Index + " To " + edge.End.Index + " exists nothing to do", LogLevel.Error);
                return;
            }

            //LogTool.AssertIsTrue(edge.Start.Index == from);
            //LogTool.AssertIsTrue(edge.End.Index == to);

            edge.Start = this.Nodes.Where(n => n.Index == from).First();
            edge.End = this.Nodes.Where(n => n.Index == to).First();

            if (this.edgeList.ContainsKey(edge.Start))
            {
                this.edgeList[edge.Start].Add(edge);
            }
            else
            {
                this.edgeList.Add(edge.Start, new List<Edge>() { edge });
            }

            if (this.isDirectional == false && from != to)
            {
                var cedge = edge.DeepCopy();
                cedge.Start = edge.End;
                cedge.End = edge.Start;

                if (this.edgeList.ContainsKey(cedge.Start))
                {
                    this.edgeList[edge.End].Add(cedge);
                }
                else
                {
                    this.edgeList.Add(edge.End, new List<Edge>() { cedge });
                }
            }
        }
        public Edge GetEdge(int from, int to)
        {
            var f = this.Nodes.Where(n => n.Index == from).First();
            return this.edgeList[f].Find(e=>e.End.Index == to);
        }
        public IEnumerable<Node> GetNeighborsNodes(int from)
        {
            var f = this.Nodes.Where(n => n.Index == from).First();
            return this.edgeList[f].Where(e => e.Start.Index != e.End.Index).Select(e => e.End);
        }
        public IEnumerable<Edge> GetNeighborsEdges(int from)
        {
            var f = this.Nodes.Where(n => n.Index == from).First();
            return this.edgeList[f];
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
                var ret = new List<Edge>();
                foreach (var el in this.edgeList.Values)
                {
                    ret.AddRange(el);
                }
                return ret;
            }
            else
            {
                var ret = new List<Edge>();
                foreach (var el in this.edgeList.Values)
                {
                    foreach (var e in el)
                    {
                        if (ret.Find(re => re.Start.Index == e.End.Index && re.End.Index == e.Start.Index) != null) continue;
                        ret.Add(e);
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

        public override void Clear()
        {
            base.Clear();
            this.edgeList.Clear();
        }
    }
    [System.Serializable]
    public class GraphAdj<Node, Edge> : Graph<Node, Edge> where Node : INode, new() where Edge : Segment<Node>, IEdge
    {
        protected Matrix<Edge> matrix = null;

        public GraphAdj(int size, bool isDirectional = false) :base(size, isDirectional)
        {
            this.Clear();
        }

        public void AddEdge(int from, int to, Edge edge)
        {
            this.CheckIndex(from, to);

            if(this.Edges.Contains(edge))
            {
                LogTool.Log("Edge From " + edge.Start.Index + " To " + edge.End.Index + " exists nothing to do", LogLevel.Error);
                return;
            }

            //LogTool.AssertIsTrue(edge.Start.Index == from);
            //LogTool.AssertIsTrue(edge.End.Index == to);

            edge.Start  = this.Nodes.Where(n => n.Index == from).First();
            edge.End    = this.Nodes.Where(n => n.Index == to).First();

            this.matrix[from][to] = edge;
            if (this.isDirectional == false && from != to)
            {
                var cedge = edge.DeepCopy();
                cedge.Start = edge.End;
                cedge.End = edge.Start;
                this.matrix[to][from] = cedge;
            }
        }
        public Edge GetEdge(int from, int to)
        {
            this.CheckIndex(from, to);
            return this.matrix[from][to];
        }
        public IEnumerable<Node> GetNeighborsNodes(int from)
        {
            this.CheckIndex(from, 0);
            return this.matrix[from].Where(n => n != null && n.Start.Index != n.End.Index).Select(e => e.End);
        }
        public IEnumerable<Edge> GetNeighborsEdges(int from)
        {
            this.CheckIndex(from, 0);
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

        public override void Clear()
        {
            base.Clear();
            this.matrix = new Matrix<Edge>(this.size, this.size);
        }

        protected void CheckIndex(int x, int y)
        {
            LogTool.LogAssertIsTrue(0 <= x && x < this.matrix.Size.x, "Invalid Index");
            LogTool.LogAssertIsTrue(0 <= y && y < this.matrix.Size.y, "Invalid Index");
        }
    }


    /*[System.Serializable]
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
*/


    public class GraphTest : Test.ITest
    {
        public string Name => this.ToString();

        [System.Serializable]
        public class Node : INode
        {
            protected int index = -1;
            public int Index { get => this.index; set => this.index = value; }
        }
        [System.Serializable]

        public class Edge : Segment<Node>, IEdge
        {
            public bool connected;
        }

        public void RunTest()
        {
            var g = new GraphEdgeList<Node, Edge>(10);
            var gadj = new GraphAdj<Node, Edge>(10);

            this.TestGraph(g);
            this.TestGraph(gadj);
        }

        protected void TestGraph(GraphAdj<Node, Edge> g)
        {
            var e00 = new Edge() { connected = true };
            var e01 = new Edge() { connected = true };
            var e02 = new Edge() { connected = true };
            var e03 = new Edge() { connected = true };
            var e04 = new Edge() { connected = true };
            g.AddEdge(0, 0, e00);
            g.AddEdge(0, 1, e01);
            g.AddEdge(0, 2, e02);
            g.AddEdge(0, 3, e03);
            g.AddEdge(0, 4, e04);
            g.AddEdge(2, 5, new Edge() { connected = true });

            LogTool.AssertIsTrue(e00 == g.GetEdge(0, 0));
            LogTool.AssertIsTrue(e01 == g.GetEdge(0, 1));
            LogTool.AssertIsTrue(e02 == g.GetEdge(0, 2));
            LogTool.AssertIsTrue(e03 == g.GetEdge(0, 3));
            LogTool.AssertIsTrue(g.GetEdge(0, 1) != g.GetEdge(0, 3));

            //this should do nothing but a error message
            //g.AddEdge(0, 0, e01);

            var nodes = g.Nodes.ToList();
            var n0 = nodes[0];
            var neighbor = g.GetNeighborsNodes(n0).ToList();
            LogTool.AssertIsFalse(neighbor.Contains(n0));

            LogTool.AssertIsTrue(neighbor.Count == 4);
            LogTool.AssertIsTrue(neighbor.Contains(nodes[1]));
            LogTool.AssertIsTrue(neighbor.Contains(nodes[2]));
            LogTool.AssertIsTrue(neighbor.Contains(nodes[3]));
            LogTool.AssertIsTrue(neighbor.Contains(nodes[4]));



            var nedges = g.GetNeighborsEdges(n0).ToList();

            LogTool.AssertIsTrue(nedges.Count == 5);
            LogTool.AssertIsTrue(nedges.Contains(e00));
            LogTool.AssertIsTrue(nedges.Contains(e01));
            LogTool.AssertIsTrue(nedges.Contains(e02));
            LogTool.AssertIsTrue(nedges.Contains(e03));
            LogTool.AssertIsTrue(nedges.Contains(e04));

        }
        protected void TestGraph(GraphEdgeList<Node, Edge> g)
        {

            var e00 = new Edge() { connected = true };
            var e01 = new Edge() { connected = true };
            var e02 = new Edge() { connected = true };
            var e03 = new Edge() { connected = true };
            var e04 = new Edge() { connected = true };
            g.AddEdge(0, 0, e00);
            g.AddEdge(0, 1, e01);
            g.AddEdge(0, 2, e02);
            g.AddEdge(0, 3, e03);
            g.AddEdge(0, 4, e04);
            g.AddEdge(2, 5, new Edge() { connected = true });

            LogTool.AssertIsTrue(e00 == g.GetEdge(0, 0));
            LogTool.AssertIsTrue(e01 == g.GetEdge(0, 1));
            LogTool.AssertIsTrue(e02 == g.GetEdge(0, 2));
            LogTool.AssertIsTrue(e03 == g.GetEdge(0, 3));
            LogTool.AssertIsTrue(g.GetEdge(0, 1) != g.GetEdge(0, 3));

            //this should do nothing but a error message
            //g.AddEdge(0, 0, e01);

            var nodes = g.Nodes.ToList();
            var n0 = nodes[0];
            var neighbor = g.GetNeighborsNodes(n0).ToList();
            LogTool.AssertIsFalse(neighbor.Contains(n0));

            LogTool.AssertIsTrue(neighbor.Count == 4);
            LogTool.AssertIsTrue(neighbor.Contains(nodes[1]));
            LogTool.AssertIsTrue(neighbor.Contains(nodes[2]));
            LogTool.AssertIsTrue(neighbor.Contains(nodes[3]));
            LogTool.AssertIsTrue(neighbor.Contains(nodes[4]));



            var nedges = g.GetNeighborsEdges(n0).ToList();

            LogTool.AssertIsTrue(nedges.Count == 5);
            LogTool.AssertIsTrue(nedges.Contains(e00));
            LogTool.AssertIsTrue(nedges.Contains(e01));
            LogTool.AssertIsTrue(nedges.Contains(e02));
            LogTool.AssertIsTrue(nedges.Contains(e03));
            LogTool.AssertIsTrue(nedges.Contains(e04));
        }

        public void Report()
        {
        }
    }
}