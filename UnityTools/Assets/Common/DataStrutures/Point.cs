using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace UnityTools.Common
{
    public interface IPoint
    {
        float3 Position { get; set; }
    }
    [System.Serializable]
    public class Point : IPoint
    {
        static public Point Zero = new Point { Position = float3.zero };

        [SerializeField] protected float3 pooo { get; set; }
        public Point() { this.Position = float3.zero; }

        public virtual float3 Position { get ; set ; }
    }

    [System.Serializable]
    public class Segment<T> where T : IPoint, new()
    {
        public Segment(int size = 2)
        {
            this.data = new T[size];
            for (var p = 0; p < this.data.Length; p++)
            {
                this.data[p] = new T();
            }
        }
        public T[] data;
        public T Left { get => this.data[0]; set => this.data[0] = value; }
        public T Right { get => this.data[this.data.Length - 1]; set => this.data[this.data.Length - 1] = value; }


        public void OnGizmos()
        {
            for (var p = 1; p < this.data.Length; p++)
            {
                Gizmos.DrawLine(this.data[p - 1].Position, this.data[p].Position);
            }
        }
    }
}
