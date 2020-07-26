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
    public class Graph<Node, Adjacent> where Node: INode, new()
    {
        protected List<Node> nodes  = null;
        protected Matrix<Adjacent> matrix = null;
        protected bool isDirectioanl = false;

        public IEnumerable<Node> Nodes { get => this.nodes; }
        public Matrix<Adjacent> AdjMatrix { get => this.matrix; }

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
            this.matrix = new Matrix<Adjacent>(newSize, newSize);
        }

        public List<Node> GetNeighborsNode(Node from)
        {
            return this.GetNeighborsNode(from.Index);
        }

        public List<Node> GetNeighborsNode(int from)
        {
            var ret = new List<Node>();
            for(var c = 0; c < this.matrix[from].Length; ++c)
            {
                if (this.matrix[from][c] != null) ret.Add(this.nodes[c]);
            }
            return ret;
        }

        public List<Adjacent> GetNeighborsEdges(int from)
        {
            return this.matrix[from].Where(c => c != null).ToList();
        }

        public Adjacent GetEdge(Node from, Node to)
        {
            return this.matrix[from.Index, to.Index];
        }

        public Adjacent GetEdge(int from, int to)
        {
            return this.matrix[from, to];
        }

        public void AddEdge(Node from, Node to, Adjacent value)
        {
            this.AddEdge(from.Index, to.Index, value);
        }

        public void AddEdge(int from, int to, Adjacent value)
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