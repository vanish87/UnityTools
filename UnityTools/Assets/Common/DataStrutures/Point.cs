using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityTools.Debuging.EditorTool;

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

        [SerializeField] protected float3 position;
        public Point() { this.Position = float3.zero; }

        public virtual float3 Position { get => this.position; set => this.position = value; }
        public void OnGizmos(float size = 1)
        {
            using( new GizmosScope(Color.cyan, Matrix4x4.identity))
            {
                Gizmos.DrawSphere(this.Position, size);
            }
        }
    }
    [System.Serializable]
    public class PointSegment<T> : Segment<T> where T : IPoint, new()
    {
        public void OnGizmos()
        {
            for (var p = 1; p < this.data.Length; p++)
            {
                Gizmos.DrawLine(this.data[p - 1].Position, this.data[p].Position);
            }
        }
    }

    [System.Serializable]
    public class Segment<T> where T : new()
    {
        public Segment(int size = 2)
        {
            this.data = new T[size];
            for (var p = 0; p < this.data.Length; p++)
            {
                this.data[p] = this.OnCreate();
            }
        }
        protected T[] data;
        public T Start { get => this.data[0]; set => this.data[0] = value; }
        public T End { get => this.data[this.data.Length - 1]; set => this.data[this.data.Length - 1] = value; }


        protected virtual T OnCreate() { return new T(); }
    }
}
