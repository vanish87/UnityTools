using System;
using Unity.Mathematics;
using UnityEngine;
using UnityTools.Common;
using UnityTools.Math;

namespace UnityTools.Test
{
    public class DIscreteFunctionTest : MonoBehaviour
    {
        public AnimationCurve curve;
        public X2FDiscreteFunction<float> test;
        public float val = 0;
        void Start()
        {

            var interval = new float2(0, 20);
            var sampleNum = 10;
            var start = new Tuple<float, float>(0, 10);
            var end = new Tuple<float, float>(0, 20);
            var y = new Vector<float>(sampleNum);
            for (var i = 0; i < sampleNum; ++i)
            {
                y[i] = math.sin((i * 1f / (sampleNum-1)) * math.PI)*10;
            }
            this.test = new X2FDiscreteFunction<float>(5, 20, y);
            this.curve = this.test.ToAnimationCurve();


        }

        // Update is called once per frame
        void Update()
        {

        }
        void OnDrawGizmos()
        {
            if (this.test != null)
            {
                var s = this.test.Start;
                var e = this.test.End;
                var t = s.Item1;
                var n = this.test.SampleNum;
                var dt = (e.Item1 - s.Item1) / (2 * n);
                for (var i = 0; i < this.test.SampleNum; ++i)
                {
                    var p = new float3(t, this.test.Evaluate(t), 0);
                    var p1 = new float3(t + dt, this.test.Evaluate(t + dt), 0);
                    Gizmos.DrawLine(p, p1);
                    t += dt;
                }

                var p2 = new float3(this.val, this.test.Evaluate(this.val), 0);
                Gizmos.DrawSphere(p2, 1);
            }
        }
    }
}
