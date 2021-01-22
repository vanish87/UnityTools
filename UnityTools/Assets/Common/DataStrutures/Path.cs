using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Common
{
    public interface IGraphPath : IList<IVertex>
    {
        IEnumerable<IVertex> Vertices { get; }
        ISet<IVertex> ToSet();

        void Append(IVertex v);

    }
    public class GraphPath : List<IVertex>, IGraphPath
    {
        public IEnumerable<IVertex> Vertices => this;


        public void Append(IVertex v)
        {
            this.Add(v);
        }

        public ISet<IVertex> ToSet()
        {
            return new HashSet<IVertex>(this);
        }

    }
}