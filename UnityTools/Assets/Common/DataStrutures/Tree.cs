using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UnityTools.Common
{
    public class Tree<T> where T : new()
    {
        public class Node 
        {
            public T Data => this.data;
            public List<Node> Children => this.children;
            protected T data = new T();
            protected List<Node> children = new List<Node>();

        }
        public bool IsEmpty => this.root == null;
        public Node Root { get => this.root; set => this.root = value; }
        protected Node root;

    }
}