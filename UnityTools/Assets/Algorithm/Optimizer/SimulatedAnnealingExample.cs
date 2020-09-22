using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityTools.Common;
using UnityTools.Debuging;
using UnityTools.Math;

namespace UnityTools.Algorithm
{
    public class SimulatedAnnealingExample : MonoBehaviour
    {
        public class HimmelblauState : CircleData<HimmelblauState.Data, int>
        {
            public class Data : SimulatedAnnealing.IState
            {
                protected float3 scale = new float3(1, 0.01f, 1);
                protected float2 currentx;

                public float2 CurrentX => this.currentx;
                public float Evaluate(SimulatedAnnealing.IState state)
                {
                    var x = currentx.x * scale.x;
                    var y = currentx.y * scale.z;

                    var v1 = x * x + y - 11;
                    var v2 = x + y * y - 7;
                    return (v1 * v1 + v2 * v2) * scale.y;
                }
                public SimulatedAnnealing.IState Generate(SimulatedAnnealing.IState x)
                {
                    this.currentx = new float2(ThreadSafeRandom.NextFloat(), ThreadSafeRandom.NextFloat());
                    //make random value to cover the range of min
                    //see for detail: https://en.wikipedia.org/wiki/Himmelblau%27s_function
                    this.currentx = (this.currentx - 0.5f) * 2 * 10;

                    return x;
                }
            }
            public HimmelblauState() : base(2)
            {

            }

            protected override Data OnCreate(int para)
            {
                return new Data();
            }
        }
        public class Problem : SimulatedAnnealing.Problem
        {
            protected HimmelblauState state = new HimmelblauState();
            public override SimulatedAnnealing.IState Current => this.state.Current;

            public override SimulatedAnnealing.IState Next => this.state.Next;

            public override void MoveToNext()
            {
                this.state.MoveToNext();
            }
        }
        public class Delta : IDelta
        {
            public void Reset()
            {
            }

            public void Step()
            {
            }
        }

        protected Problem problem;
        protected SimulatedAnnealing SA;

        protected void Start()
        {
            this.problem = new Problem()
            {
                temperature = 1,
                minTemperature = 0.0001f,
                k = 1,
                alpha = 0.99f
            };
            this.SA = new SimulatedAnnealing(this.problem, new Delta());
            this.SA.TryToRun();


            /*this.SA.PerStep((p, s, dt, a) =>
            {
                var state = (p as Problem).Current as State.Data;
                var next = (p as Problem).Next as State.Data;

                LogTool.Log("Current: " + state.currentx + " " + state.Evaluate(null), LogLevel.Info);
                LogTool.Log("Next: " + next.currentx + " " + next.Evaluate(null), LogLevel.Info);
            });*/

            this.SA.End((p, s, dt, a) =>
            {
                var state = (p as Problem).Current as HimmelblauState.Data;
                LogTool.Log("Solution: " + state.CurrentX + " min= " + state.Evaluate(null), LogLevel.Info);
            });
        }
        
    }
}