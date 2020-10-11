using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityTools.Common;
using UnityTools.Debuging;
using UnityTools.Debuging.EditorTool;
using UnityTools.Math;

namespace UnityTools.Algorithm
{
    public class SimulatedAnnealingExample : MonoBehaviour, IFunctionProvider, Function<Vector<float>, float>
    {
        public class HimmelblauState : CircleData<HimmelblauState.Data, int>
        {
            public class Data : SimulatedAnnealing.IState
            {
                protected float3 scale = new float3(1, 0.01f, 1);
                protected float2 currentx;

                public float2 CurrentX => this.currentx;
                public float Evaluate(Vector<float> input)
                {
                    var x = currentx.x * scale.x;
                    var y = currentx.y * scale.z;

                    var v1 = x * x + y - 11;
                    var v2 = x + y * y - 7;
                    return (v1 * v1 + v2 * v2) * scale.y;
                }
                public Vector<float> Generate(Vector<float> input)
                {
                    this.currentx = new float2(ThreadSafeRandom.NextFloat(), ThreadSafeRandom.NextFloat());
                    //make random value to cover the range of min
                    //see for detail: https://en.wikipedia.org/wiki/Himmelblau%27s_function
                    this.currentx = (this.currentx - 0.5f) * 2 * 10;

                    return input;
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

            public float Evaluate(Vector<float> x)
            {
                return this.Current.Evaluate(x);
            }

            public Vector<float> Generate(Vector<float> x)
            {
                throw new System.NotImplementedException();
            }

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


        [SerializeField] protected float temp = 1;
        [SerializeField] protected IterationAlgorithmMode mode = IterationAlgorithmMode.FullStep;

        public Function<Vector<float>, float> Function => this; 

        protected void Start()
        {
            this.problem = new Problem()
            {
                temperature = 100,
                minTemperature = 0.0001f,
                k = 1,
                alpha = 0.99f
            };
            this.SA = new SimulatedAnnealing(this.problem, new Delta(), this.mode);
            this.SA.TryToRun();


            // this.SA.PerStep((p, s, dt, a) =>
            // {
            //     var state = (p as Problem).Current as HimmelblauState.Data;
            //     var next = (p as Problem).Next as HimmelblauState.Data;

            //     LogTool.Log("Current: " + state.CurrentX + " " + state.Evaluate(null), LogLevel.Info);
            //     LogTool.Log("Next: " + next.CurrentX + " " + next.Evaluate(null), LogLevel.Info);
            // });

            this.SA.End((p, s, dt, a) =>
            {
                var state = (p as Problem).Current as HimmelblauState.Data;
                LogTool.Log("Solution: " + state.CurrentX + " min= " + state.Evaluate(null), LogLevel.Info);
            });
        }


        protected void Update()
        {
            if(Input.GetKey(KeyCode.Space))
            {
                this.SA.TryToRun();
            }

            this.temp = this.problem.temperature;
        }

        protected void OnDrawGizmos()
        {
            if(this.problem != null)
            {
                var x = (this.problem as Problem).Current as HimmelblauState.Data;
                var y = x.Evaluate(null);
                var nx = (this.problem as Problem).Next as HimmelblauState.Data;
                var ny = nx.Evaluate(null);


                using (new GizmosScope(Color.cyan, Matrix4x4.identity))
                {
                    Gizmos.DrawSphere(new float3(x.CurrentX.x/4, y, x.CurrentX.y/4), 0.1f);
                }
                using (new GizmosScope(Color.red, Matrix4x4.identity))
                {
                    Gizmos.DrawSphere(new float3(nx.CurrentX.x/4, ny, nx.CurrentX.y/4), 0.1f);
                }
            }
        }

        public float Evaluate(Vector<float> input)
        {
            var scale = new float3(4,0.01f,4);
            var x = input[0] * scale.x;
            var y = input[1] * scale.z;

            var v1 = x * x + y - 11;
            var v2 = x + y * y - 7;
            return (v1 * v1 + v2 * v2) * scale.y;

        }

        public Vector<float> Generate(Vector<float> x)
        {
            return default;
        }
    }
}