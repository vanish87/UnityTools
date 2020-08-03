using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityTools.Common
{
    public interface INode
    {
        int Index { get; set; }
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

        public List<Node> GetNeighborsNode(Node from)
        {
            return this.GetNeighborsNode(from.Index);
        }

        public List<Node> GetNeighborsNode(int from)
        {
            var ret = new List<Node>();
            for(var c = 0; c < this.matrix[from].Size; ++c)
            {
                if (this.matrix[from][c] != null) ret.Add(this.nodes[c]);
            }
            return ret;
        }

        public List<AdjacentEdge> GetNeighborsEdges(int from)
        {
            return this.matrix[from].Data.Where(c => c != null).ToList();
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