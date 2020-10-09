using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace UnityTools.Common
{
    public class Trapezoidal
    {
        public float3 Left =>this.left;
        public float3 Right =>this.right;
        protected float3 left;
        protected float3 right;

        protected float height;
        protected float alpha;
        protected float beta;


        public void OnDrawGizmos()
        {
            var p1 = this.Left;
            var p2 = this.Right;
            var p3 = math.sin(this.alpha) * this.height;
            var p4 = math.sin(this.beta) * this.height;
            // Gizmos.DrawLine(p1,p2);
            // Gizmos.DrawLine(p2,p3);
            // Gizmos.DrawLine(p3,p4);
            // Gizmos.DrawLine(p4,p1);
        }
    }
}
