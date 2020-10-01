using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityTools.Debuging;

namespace UnityTools.Common
{
    public interface INewGraph: ISet<IVertex>
    {
        IEdge AddEdge(IVertex v1, IVertex v2, bool isDirectional = false);
        IEdge GetEdge(IVertex v1, IVertex v2);
        bool Contains(IEdge edge);
        void Remove(IEdge edge);

        IEnumerable<IEdge> GetNeighborEdges(IVertex from);
        IEnumerable<IVertex> GetNeighborVertices(IVertex from);

        IGraphFactory Factory { get; }
    }

    public interface IVertex : IEquatable<IVertex>, ICloneable
    {
    }
    public interface IEdge : IEquatable<IEdge>, ICloneable
    {
        bool IsDirectional { get; }
        IVertex Vertex { get; set; }
        IVertex OtherVertex { get; set; }
    }
    public interface IGraphFactory
    {
        IVertex CreateVertex();
        IEdge CreateEdge(IVertex v1, IVertex v2, bool isDirectional = false);
    }

    public class IndexGraphFactory : IGraphFactory
    {
        private Queue<int> indexPool = new Queue<int>();
        private int currentIndex = 0;
        public IEdge CreateEdge(IVertex v1, IVertex v2, bool isDirectional = false)
        {
            return new DefaultEdge(isDirectional) { Vertex = v1, OtherVertex = v2 };
        }

        public IVertex CreateVertex()
        {
            var newID = this.indexPool.Count == 0 ? this.currentIndex++ : this.indexPool.Dequeue();
            return new IndexVertex() { index = newID };
        }
    }


    public class IndexVertex : IVertex
    {
        internal int index = -1;
        public object Clone()
        {
            return new IndexVertex() { index = this.index };
        }

        public bool Equals(IVertex other)
        {
            if(other is IndexVertex)
            {
                return this.index == (other as IndexVertex).index;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.index;
        }

        public override string ToString()
        {
            return this.index.ToString();
        }
    }

    [System.Serializable]
    public class DefaultEdge : IEdge
    {
        private bool isDirectional = false;

        public IVertex Vertex { get ;  set ; }
        public IVertex OtherVertex { get ; set ; }

        public bool IsDirectional => this.isDirectional;

        public DefaultEdge(bool isDirectional = false)
        {
            this.isDirectional = isDirectional;
        }
        public object Clone()
        {
            return new DefaultEdge(this.IsDirectional) { Vertex = this.Vertex.Clone() as IVertex, OtherVertex = this.OtherVertex.Clone() as IVertex };
        }

        public bool Equals(IEdge other)
        {
            if(this.IsDirectional != other.IsDirectional) return false;

            if(this.IsDirectional) return (this.Vertex == other.Vertex && this.OtherVertex == other.OtherVertex);

            return (this.Vertex.Equals(other.Vertex) && this.OtherVertex.Equals(other.OtherVertex)) 
                || (this.Vertex.Equals(other.OtherVertex) && this.OtherVertex.Equals(other.Vertex));
        }
        public override string ToString()
        {
            return this.Vertex.ToString() + (this.IsDirectional?"->":"<->")+ this.OtherVertex.ToString();
        }
   }

    public class GraphEdgeEnumerator<Edge> : IEnumerator<Edge>
    {
        private List<Edge> adjList;
        private HashSet<Edge> visited = new HashSet<Edge>();
        private int current = -1;

        public object Current => this.adjList[this.current];

        Edge IEnumerator<Edge>.Current => (Edge)this.Current;

        public GraphEdgeEnumerator(Dictionary<IVertex, List<Edge>> adjList)
        {
            this.adjList = new List<Edge>();
            foreach(var l in adjList.Values) this.adjList.AddRange(l);
        }

        public bool MoveNext()
        {
            this.current++;
            while(this.current < this.adjList.Count && this.visited.Contains(this.adjList[this.current])) this.current++;
            if(this.current < this.adjList.Count)
            {
                this.visited.Add(this.adjList[this.current]);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Reset()
        {
            this.visited.Clear();
            this.current = -1;
        }

        public void Dispose()
        {
            this.visited.Clear();
            this.adjList.Clear();
        }
    }
    
    public class GraphEnum<T> : IEnumerable<T>
    {
        private IEnumerator<T> instance;
        public GraphEnum(IEnumerator<T> e)
        {
            this.instance = e;
        }
        public IEnumerator<T> GetEnumerator()
        {
            return this.instance;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.instance;
        }
    }

    // public class GraphAdjMatrix<Vertex, Edge, GraphFactory> : NewGraph<Vertex, Edge, GraphFactory> where Vertex : IndexVertex where Edge : IEdge where GraphFactory : IGraphFactory, new()
    // {
    //     private Matrix<IEdge> edges = new Matrix<IEdge>();
    //     public override GraphEnum<Edge> Edges => new GraphEnum<Edge>(this.edges.Cast<Edge>().Where(e=>e!=null).GetEnumerator());

    //     // adj matrix need num of vertices to init matrix
    //     public void InitVertices(int num)
    //     {
    //         var current = this.Count;
    //         for (var i = 0; i < num - current; ++i)
    //         {
    //             this.Add(this.Factory.CreateVertex());
    //         }
    //         this.edges.Resize(num, num);
    //     }

    
    //     protected int2 GetIndex(IVertex v1, IVertex v2)
    //     {
    //         var id1 = (v1 as IndexVertex).index;
    //         var id2 = (v2 as IndexVertex).index;

    //         return this.IsDirectional == false && id1 < id2 ? new int2(id1, id2) : new int2(id2, id1);
    //     }
    // }

    public class NewGraph<Vertex, Edge, GraphFactory> : HashSet<IVertex>, INewGraph where Vertex : IVertex where Edge : IEdge where GraphFactory : IGraphFactory, new()
    {
        private Dictionary<IVertex, List<IEdge>> adjacentList = new Dictionary<IVertex, List<IEdge>>();
        private IGraphFactory factory;

        public IGraphFactory Factory => this.factory;

        public GraphEnum<Vertex> Vertices => new GraphEnum<Vertex>(this.Cast<Vertex>().GetEnumerator());
        public GraphEnum<Edge> Edges => new GraphEnum<Edge>(new GraphEnum<IEdge>(new GraphEdgeEnumerator<IEdge>(this.adjacentList)).Cast<Edge>().GetEnumerator());

        public NewGraph()
        {
            this.factory = new GraphFactory();

            //Todo Check FactoryType with graph type
            // var t = typeof(GraphFactory).GetMethod("CreateVertex").ReturnType;
            // var tt = typeof(Vertex);
            // LogTool.LogAssertIsTrue(typeof(GraphFactory).GetMethod("CreateVertex").ReturnType == typeof(Vertex), "factory should create vertex of type " + typeof(Vertex));
            // LogTool.LogAssertIsTrue(typeof(GraphFactory).GetMethod("CreateEdge").ReturnType == typeof(Edge), "factory should create edge of type " + typeof(Edge));
        }

        protected void TryAddEdge(IVertex v, IEdge edge)
        {
            if(!this.adjacentList.ContainsKey(v))
            {
                this.adjacentList.Add(v, new List<IEdge>());
            }
            if(!this.adjacentList[v].Contains(edge))
            {
                this.adjacentList[v].Add(edge);
            }
        }
        protected void TryRemoveEdge(IVertex v, IEdge edge)
        {
            if(this.adjacentList.ContainsKey(v))
            {
                this.adjacentList[v].Remove(edge);
            }

        }

        public IEdge AddEdge(IVertex v1, IVertex v2, bool isDirectional = false)
        {
            var ret = this.GetEdge(v1,v2);
            if(ret != default) return ret;

            ret = this.Factory.CreateEdge(v1, v2, isDirectional);
            this.TryAddEdge(v1, ret);
            if(!isDirectional)
            {
                this.TryAddEdge(v2, ret);
            }
            return ret;
        }

        public bool Contains(IEdge edge)
        {
            return this.GetEdge(edge.Vertex, edge.OtherVertex) != default;
        }

        public IEdge GetEdge(IVertex v1, IVertex v2)
        {
            if(this.adjacentList.ContainsKey(v1))
            {
                return this.adjacentList[v1].Find(e=>(e.Vertex == v2 && e.OtherVertex == v1) || (e.OtherVertex == v2 && e.Vertex == v1));
            }
            return default;
        }

        public void Remove(IEdge edge)
        {
            var v1 = edge.Vertex;
            var v2 = edge.OtherVertex;

            this.TryRemoveEdge(v1, edge);
            this.TryRemoveEdge(v2, edge);
        }

        public IEnumerable<IEdge> GetNeighborEdges(IVertex from)
        {
            if(this.adjacentList.ContainsKey(from))
            {
                return this.adjacentList[from];
            }
            else
            {
                return new List<IEdge>();
            }
        }

        public IEnumerable<IVertex> GetNeighborVertices(IVertex from)
        {
            return this.GetNeighborEdges(from).Select(e=>e.Vertex==from?e.OtherVertex:e.Vertex);
        }
    }
    // public interface IGraph<Node, Edge>
    // {
    //     IEnumerable<Node> Nodes { get; }
    //     IEnumerable<Edge> Edges { get; }
    //     bool IsDirectional { get; }

    //     Node AddNode();
    //     Edge GetEdge(Node from, Node to);
    //     void AddEdge(Node from, Node to, Edge edge);
    //     void RemoveEdge(Edge edge);
    //     IEnumerable<Node> GetNeighborsNodes(Node from);
    //     IEnumerable<Edge> GetNeighborsEdges(Node from);

    //     void Clear();
    // }
    // public interface INode
    // {
    //     int Index { get; set; }
    // }


    

    // [System.Serializable]
    // public abstract class Graph<Node, Edge> : IGraph<Node, Edge> where Node : INode, new() where Edge : IEdge
    // {
    //     protected List<Node> nodeList = new List<Node>();
    //     protected int size = 0;
    //     protected bool isDirectional = false;

    //     public abstract IEnumerable<Edge> GetEdges();
    //     public IEnumerable<Node> Nodes => this.nodeList;
    //     public IEnumerable<Edge> Edges => this.GetEdges();

    //     public bool IsDirectional => this.isDirectional;

    //     public virtual void Clear()
    //     {
    //         this.nodeList.Clear();
    //         foreach (var r in Enumerable.Range(0, size))
    //         {
    //             this.AddNode();
    //         }
    //     }
    //     public abstract Edge GetEdge(Node from, Node to);
    //     public abstract void AddEdge(Node from, Node to, Edge edge);
    //     public abstract void RemoveEdge(Edge edge);
    //     public abstract IEnumerable<Edge> GetNeighborsEdges(Node from);
    //     public abstract IEnumerable<Node> GetNeighborsNodes(Node from);

    //     public Graph(int size, bool isDirectional = false)
    //     {
    //         this.isDirectional = isDirectional;
    //         this.size = size;
    //     }
    //     public Node AddNode()
    //     {
    //         var node = new Node() { Index = this.nodeList.Count};
    //         this.nodeList.Add(node);
    //         return node;
    //     }
    // }
    // /// <summary>
    // /// WARNING: it has performance issues right now
    // /// </summary>
    // /// <typeparam name="Node"></typeparam>
    // /// <typeparam name="Edge"></typeparam>
    // [System.Serializable]
    // public class GraphEdgeList<Node, Edge> : Graph<Node, Edge> where Node : INode, new() where Edge : Segment<Node>, IEdge
    // {
    //     protected Dictionary<Node, List<Edge>> edgeList = new Dictionary<Node, List<Edge>>();
    //     public GraphEdgeList(int size, bool isDirectional = false) : base(size, isDirectional)
    //     {
    //         this.Clear();
    //     }

    //     public void AddEdge(int from, int to, Edge edge)
    //     {
    //         if (this.Edges.Contains(edge))
    //         {
    //             LogTool.Log("Edge From " + edge.Start.Index + " To " + edge.End.Index + " exists nothing to do", LogLevel.Error);
    //             return;
    //         }

    //         //LogTool.AssertIsTrue(edge.Start.Index == from);
    //         //LogTool.AssertIsTrue(edge.End.Index == to);

    //         edge.Start = this.Nodes.Where(n => n.Index == from).First();
    //         edge.End = this.Nodes.Where(n => n.Index == to).First();

    //         if (this.edgeList.ContainsKey(edge.Start))
    //         {
    //             this.edgeList[edge.Start].Add(edge);
    //         }
    //         else
    //         {
    //             this.edgeList.Add(edge.Start, new List<Edge>() { edge });
    //         }

    //         if (this.isDirectional == false && from != to)
    //         {
    //             var cedge = edge.DeepCopy();
    //             cedge.Start = edge.End;
    //             cedge.End = edge.Start;

    //             if (this.edgeList.ContainsKey(cedge.Start))
    //             {
    //                 this.edgeList[edge.End].Add(cedge);
    //             }
    //             else
    //             {
    //                 this.edgeList.Add(edge.End, new List<Edge>() { cedge });
    //             }
    //         }
    //     }
    //     public Edge GetEdge(int from, int to)
    //     {
    //         var f = this.Nodes.Where(n => n.Index == from).First();
    //         return this.edgeList[f].Find(e=>e.End.Index == to);
    //     }
    //     public IEnumerable<Node> GetNeighborsNodes(int from)
    //     {
    //         var f = this.Nodes.Where(n => n.Index == from).First();
    //         return this.edgeList[f].Where(e => e.Start.Index != e.End.Index).Select(e => e.End);
    //     }
    //     public IEnumerable<Edge> GetNeighborsEdges(int from)
    //     {
    //         var f = this.Nodes.Where(n => n.Index == from).First();
    //         return this.edgeList[f];
    //     }

    //     public override void AddEdge(Node from, Node to, Edge edge)
    //     {
    //         this.AddEdge(from.Index, to.Index, edge);
    //     }

    //     public override Edge GetEdge(Node from, Node to)
    //     {
    //         return this.GetEdge(from.Index, to.Index);
    //     }

    //     public override IEnumerable<Edge> GetEdges()
    //     {
    //         if (this.isDirectional)
    //         {
    //             var ret = new List<Edge>();
    //             foreach (var el in this.edgeList.Values)
    //             {
    //                 ret.AddRange(el);
    //             }
    //             return ret;
    //         }
    //         else
    //         {
    //             var ret = new List<Edge>();
    //             foreach (var el in this.edgeList.Values)
    //             {
    //                 foreach (var e in el)
    //                 {
    //                     if (ret.Find(re => re.Start.Index == e.End.Index && re.End.Index == e.Start.Index) != null) continue;
    //                     ret.Add(e);
    //                 }
    //             }
    //             return ret;
    //         }
    //     }

    //     public override IEnumerable<Edge> GetNeighborsEdges(Node from)
    //     {
    //         return this.GetNeighborsEdges(from.Index);
    //     }

    //     public override IEnumerable<Node> GetNeighborsNodes(Node from)
    //     {
    //         return this.GetNeighborsNodes(from.Index);
    //     }

    //     public override void Clear()
    //     {
    //         base.Clear();
    //         this.edgeList.Clear();
    //     }

    //     public override void RemoveEdge(Edge edge)
    //     {
    //         throw new System.NotImplementedException();
    //     }
    // }
    // [System.Serializable]
    // public class GraphAdj<Node, Edge> : Graph<Node, Edge> where Node : INode, new() where Edge : Segment<Node>, IEdge
    // {
    //     protected Matrix<Edge> matrix = null;

    //     public GraphAdj(int size, bool isDirectional = false) :base(size, isDirectional)
    //     {
    //         this.Clear();
    //     }

    //     public void AddEdge(int from, int to, Edge edge)
    //     {
    //         this.CheckIndex(from, to);

    //         if(this.Edges.Contains(edge))
    //         {
    //             LogTool.Log("Edge From " + edge.Start.Index + " To " + edge.End.Index + " exists nothing to do", LogLevel.Error);
    //             return;
    //         }

    //         //LogTool.AssertIsTrue(edge.Start.Index == from);
    //         //LogTool.AssertIsTrue(edge.End.Index == to);

    //         edge.Start  = this.Nodes.Where(n => n.Index == from).First();
    //         edge.End    = this.Nodes.Where(n => n.Index == to).First();

    //         this.matrix[from][to] = edge;
    //         if (this.isDirectional == false && from != to)
    //         {
    //             var cedge = edge.DeepCopy();
    //             cedge.Start = edge.End;
    //             cedge.End = edge.Start;
    //             this.matrix[to][from] = cedge;
    //         }
    //     }
    //     public Edge GetEdge(int from, int to)
    //     {
    //         this.CheckIndex(from, to);
    //         return this.matrix[from][to];
    //     }
    //     public IEnumerable<Node> GetNeighborsNodes(int from)
    //     {
    //         this.CheckIndex(from, 0);
    //         return this.matrix[from].Where(n => n != null && n.Start.Index != n.End.Index).Select(e => e.End);
    //     }
    //     public IEnumerable<Edge> GetNeighborsEdges(int from)
    //     {
    //         this.CheckIndex(from, 0);
    //         return this.matrix[from].Where(n => n != null);
    //     }

    //     public override void AddEdge(Node from, Node to, Edge edge)
    //     {
    //         this.AddEdge(from.Index, to.Index, edge);
    //     }
    //     public override void RemoveEdge(Edge edge)
    //     {
    //         var from = edge.Start.Index;
    //         var to = edge.End.Index;
    //         if (this.isDirectional)
    //         {
    //             this.matrix[from][to] = null;
    //         }
    //         else
    //         {
    //             this.matrix[from][to] = null;
    //             this.matrix[to][from] = null;
    //         }
    //     }

    //     public override Edge GetEdge(Node from, Node to)
    //     {
    //         return this.GetEdge(from.Index, to.Index);
    //     }

    //     public override IEnumerable<Edge> GetEdges()
    //     {
    //         if (this.isDirectional)
    //         {
    //             return this.matrix.Where(e => e != null);
    //         }
    //         else
    //         {
    //             var ret = new List<Edge>();
    //             for (var r = 0; r < this.matrix.Size.x; ++r)
    //             {
    //                 for (var c = r; c < this.matrix.Size.y; ++c)
    //                 {
    //                     var s = this.GetEdge(r, c);
    //                     if (s == null) continue;
    //                     ret.Add(s);
    //                 }
    //             }
    //             return ret;
    //         }
    //     }

    //     public override IEnumerable<Edge> GetNeighborsEdges(Node from)
    //     {
    //         return this.GetNeighborsEdges(from.Index);
    //     }

    //     public override IEnumerable<Node> GetNeighborsNodes(Node from)
    //     {
    //         return this.GetNeighborsNodes(from.Index);
    //     }

    //     public override void Clear()
    //     {
    //         base.Clear();
    //         this.matrix = new Matrix<Edge>(this.size, this.size);
    //     }

    //     protected void CheckIndex(int x, int y)
    //     {
    //         LogTool.LogAssertIsTrue(0 <= x && x < this.matrix.Size.x, "Invalid Index");
    //         LogTool.LogAssertIsTrue(0 <= y && y < this.matrix.Size.y, "Invalid Index");
    //     }

        
    // }


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


    public class NewGraphTest : Test.ITest
    {

        public string Name => this.ToString();

        public void Report()
        {
        }

        public void RunTest()
        {
            var g = new NewGraph<IndexVertex, DefaultEdge, IndexGraphFactory>();
            this.TestNewGraph(g);

        }
        protected void TestNewGraph(NewGraph<IndexVertex, DefaultEdge, IndexGraphFactory> graph)
        {
            foreach(var e in Enumerable.Range(0,20))
            {
                graph.Add(graph.Factory.CreateVertex());
            }
            var nodes = graph.Vertices.OrderBy(v=>v.index).ToList();
            var e01 = graph.AddEdge(nodes[0], nodes[1]);
            var e02 = graph.Factory.CreateEdge(nodes[3], nodes[4]);


            var e56 = graph.AddEdge(nodes[5], nodes[6]);
            graph.AddEdge(nodes[5], nodes[7]);
            graph.AddEdge(nodes[5], nodes[8]);
            graph.AddEdge(nodes[5], nodes[5]);
            graph.AddEdge(nodes[5], nodes[1]);
            graph.AddEdge(nodes[5], nodes[11], true);

            graph.AddEdge(nodes[1], nodes[15]);

            LogTool.AssertIsTrue(graph.Contains(e01));
            LogTool.AssertIsFalse(graph.Contains(e02));
            LogTool.AssertIsTrue(e56 ==graph.GetEdge(nodes[5], nodes[6]));
            LogTool.AssertIsTrue(e56 ==graph.GetEdge(nodes[6], nodes[5]));
            LogTool.AssertIsFalse(e56 ==graph.GetEdge(nodes[5], nodes[5]));

            foreach(var e in graph.Edges)
            {
                LogTool.Log(e.ToString());
            }

            LogTool.Log("Neighbor 5");
            foreach(var e in graph.GetNeighborVertices(nodes[5]))
            {
                LogTool.Log(e.ToString());
            }
        }

    }


    // public class GraphTest : Test.ITest
    // {
    //     public string Name => this.ToString();

    //     [System.Serializable]
    //     public class Node : INode
    //     {
    //         protected int index = -1;
    //         public int Index { get => this.index; set => this.index = value; }
    //     }
    //     [System.Serializable]

    //     public class Edge : Segment<Node>, IEdge
    //     {
    //         public bool connected;
    //     }

    //     public void RunTest()
    //     {
    //         var g = new GraphEdgeList<Node, Edge>(10);
    //         var gadj = new GraphAdj<Node, Edge>(10);

    //         this.TestGraph(g);
    //         this.TestGraph(gadj);
    //     }



    //     protected void TestNewGraph(GraphAdjMatrix<IndexVertex, DefaultEdge, IndexGraphFactory> graph)
    //     {
    //         graph.InitVertices(10);
    //         var nodes = graph.Vertices.OrderBy(v=>v.index).ToList();
    //         var e01 = graph.AddEdge(nodes[0], nodes[1]);

    //     }

    //     protected void TestGraph(GraphAdj<Node, Edge> g)
    //     {
    //         var e00 = new Edge() { connected = true };
    //         var e01 = new Edge() { connected = true };
    //         var e02 = new Edge() { connected = true };
    //         var e03 = new Edge() { connected = true };
    //         var e04 = new Edge() { connected = true };
    //         g.AddEdge(0, 0, e00);
    //         g.AddEdge(0, 1, e01);
    //         g.AddEdge(0, 2, e02);
    //         g.AddEdge(0, 3, e03);
    //         g.AddEdge(0, 4, e04);
    //         g.AddEdge(2, 5, new Edge() { connected = true });

    //         LogTool.AssertIsTrue(e00 == g.GetEdge(0, 0));
    //         LogTool.AssertIsTrue(e01 == g.GetEdge(0, 1));
    //         LogTool.AssertIsTrue(e02 == g.GetEdge(0, 2));
    //         LogTool.AssertIsTrue(e03 == g.GetEdge(0, 3));
    //         LogTool.AssertIsTrue(g.GetEdge(0, 1) != g.GetEdge(0, 3));

    //         //this should do nothing but a error message
    //         //g.AddEdge(0, 0, e01);

    //         var nodes = g.Nodes.ToList();
    //         var n0 = nodes[0];
    //         var neighbor = g.GetNeighborsNodes(n0).ToList();
    //         LogTool.AssertIsFalse(neighbor.Contains(n0));

    //         LogTool.AssertIsTrue(neighbor.Count == 4);
    //         LogTool.AssertIsTrue(neighbor.Contains(nodes[1]));
    //         LogTool.AssertIsTrue(neighbor.Contains(nodes[2]));
    //         LogTool.AssertIsTrue(neighbor.Contains(nodes[3]));
    //         LogTool.AssertIsTrue(neighbor.Contains(nodes[4]));



    //         var nedges = g.GetNeighborsEdges(n0).ToList();

    //         LogTool.AssertIsTrue(nedges.Count == 5);
    //         LogTool.AssertIsTrue(nedges.Contains(e00));
    //         LogTool.AssertIsTrue(nedges.Contains(e01));
    //         LogTool.AssertIsTrue(nedges.Contains(e02));
    //         LogTool.AssertIsTrue(nedges.Contains(e03));
    //         LogTool.AssertIsTrue(nedges.Contains(e04));

    //     }
    //     protected void TestGraph(GraphEdgeList<Node, Edge> g)
    //     {

    //         var e00 = new Edge() { connected = true };
    //         var e01 = new Edge() { connected = true };
    //         var e02 = new Edge() { connected = true };
    //         var e03 = new Edge() { connected = true };
    //         var e04 = new Edge() { connected = true };
    //         g.AddEdge(0, 0, e00);
    //         g.AddEdge(0, 1, e01);
    //         g.AddEdge(0, 2, e02);
    //         g.AddEdge(0, 3, e03);
    //         g.AddEdge(0, 4, e04);
    //         g.AddEdge(2, 5, new Edge() { connected = true });

    //         LogTool.AssertIsTrue(e00 == g.GetEdge(0, 0));
    //         LogTool.AssertIsTrue(e01 == g.GetEdge(0, 1));
    //         LogTool.AssertIsTrue(e02 == g.GetEdge(0, 2));
    //         LogTool.AssertIsTrue(e03 == g.GetEdge(0, 3));
    //         LogTool.AssertIsTrue(g.GetEdge(0, 1) != g.GetEdge(0, 3));

    //         //this should do nothing but a error message
    //         //g.AddEdge(0, 0, e01);

    //         var nodes = g.Nodes.ToList();
    //         var n0 = nodes[0];
    //         var neighbor = g.GetNeighborsNodes(n0).ToList();
    //         LogTool.AssertIsFalse(neighbor.Contains(n0));

    //         LogTool.AssertIsTrue(neighbor.Count == 4);
    //         LogTool.AssertIsTrue(neighbor.Contains(nodes[1]));
    //         LogTool.AssertIsTrue(neighbor.Contains(nodes[2]));
    //         LogTool.AssertIsTrue(neighbor.Contains(nodes[3]));
    //         LogTool.AssertIsTrue(neighbor.Contains(nodes[4]));



    //         var nedges = g.GetNeighborsEdges(n0).ToList();

    //         LogTool.AssertIsTrue(nedges.Count == 5);
    //         LogTool.AssertIsTrue(nedges.Contains(e00));
    //         LogTool.AssertIsTrue(nedges.Contains(e01));
    //         LogTool.AssertIsTrue(nedges.Contains(e02));
    //         LogTool.AssertIsTrue(nedges.Contains(e03));
    //         LogTool.AssertIsTrue(nedges.Contains(e04));
    //     }

    //     public void Report()
    //     {
    //     }
    // }
}