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
    public class DownhillSimplexExample : MonoBehaviour, IFunctionProvider
    {
        public class RosenbrockProblem : DownhillSimplex<float>.Problem
        {
            float a = 1;
            float b = 100;
            float3 scale = new float3(1,0.01f,1);

            public RosenbrockProblem(int dim = 2) : base(dim)
            {
            }
            public override float Evaluate(Vector<float> v)
            {
                var x = v[0] * scale.x;
                var y = v[1] * scale.z;

                var by = (y - x * x);
                var ret = (a - x) * (a - x) + b * by * by;
                return ret * scale.y;
            }

            public override Vector<float> Generate(Vector<float> x)
            {
                var p = new Vector<float>(2);
                p[0] = ThreadSafeRandom.NextFloat();
                p[1] = ThreadSafeRandom.NextFloat();
                return p;
            }
        }

        public class HimmelblauProblem : DownhillSimplex<float>.Problem
        {
            float3 scale = new float3(4, 0.01f, 4);
            public HimmelblauProblem(int dim = 2) : base(dim)
            {
            }

            public override float Evaluate(Vector<float> v)
            {
                var x = v[0] * scale.x;
                var y = v[1] * scale.z;

                var v1 = x * x + y - 11;
                var v2 = x + y * y - 7;
                return (v1 * v1 + v2 * v2) * scale.y;
            }

            public override Vector<float> Generate(Vector<float> x)
            {
                var p = new Vector<float>(2);
                p[0] = ThreadSafeRandom.NextFloat();
                p[1] = ThreadSafeRandom.NextFloat();
                return p;
            }
        }

        public class Delta : IDelta
        {
            public int count = 0;
            public void Reset()
            {
                this.count = 0;
            }

            public void Step()
            {
                this.count++;
            }
        }
        [SerializeField] protected IterationAlgorithmMode mode = IterationAlgorithmMode.FullStep;
        protected DownhillSimplex<float> simplex;
        protected DownhillSimplex<float>.Problem p = new HimmelblauProblem();

        public Function<Vector<float>, float> Function => this.p;

        protected void Start()
        {
            this.simplex = new DownhillSimplex<float>(this.p, new Delta(), this.mode);

            this.simplex.End((p, s, dt, a)=>
            {
                LogTool.Log("Solution: ", LogLevel.Info);
                var sol = s as DownhillSimplex<float>.Solution;
                sol.min.Print();
                LogTool.Log("Running count: " + (dt as Delta).count);
            });

            this.simplex.TryToRun();
        }

        protected void Update()
        {
            if(Input.GetKeyDown(KeyCode.S) && this.mode == IterationAlgorithmMode.PerStep)
            {
                this.simplex.TryToRun();
            }
        }

        protected void OnDisable()
        {
            this.simplex.Stop();
        }

        protected void OnDrawGizmos()
        {
            var simplex = this.simplex?.Vertices;
            if (simplex != null)
            {
                using (new GizmosScope(Color.blue, Matrix4x4.identity))
                {
                    var p1 = new Vector3(simplex[0].X[0], simplex[0].Fx, simplex[0].X[1]);
                    var p2 = new Vector3(simplex[1].X[0], simplex[1].Fx, simplex[1].X[1]);
                    var p3 = new Vector3(simplex[2].X[0], simplex[2].Fx, simplex[2].X[1]);
                    Gizmos.DrawLine(p1, p2);
                    Gizmos.DrawLine(p1, p3);
                    Gizmos.DrawLine(p2, p3);
                }
            }
        }
    }
}