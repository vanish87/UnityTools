using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityTools.Common;
using UnityTools.Debuging;
using UnityTools.Math;

namespace UnityTools.Algorithm
{    
    public class SimulatedAnnealing : IterationAlgorithm
    {
        public interface IState : Function<Vector<float>, float>
        {

        }

        [System.Serializable]
        public abstract class Problem : IProblem
        {
            internal protected float temperature = 1;
            internal protected float k = 1;
            internal protected float minTemperature = 0.000001f;
            internal protected float alpha = 0.99f;

            public abstract IState Current { get; }
            public abstract IState Next { get; }

            public abstract void MoveToNext();

            public virtual void Cool(bool useNext)
            {
                if(useNext) this.temperature *= this.alpha;
            }
        }
        public class Solution : ISolution
        {
            public IState Current { get; internal set; }
        }


        public SimulatedAnnealing(IProblem problem, IDelta dt, IterationAlgorithmMode mode = IterationAlgorithmMode.FullStep)
                                    : base(problem, dt, mode)
        {

        }

        public override bool IsSolutionAcceptable(ISolution solution)
        {
            var p = this.problem as Problem;
            return p.temperature < p.minTemperature;
        }

        public override ISolution Solve(IProblem problem)
        {
            LogTool.LogAssertIsTrue(this.dt != null, "Dt is null");
            var sol = new Solution();
            var p = this.problem as Problem;

            p.Next.Generate(null);

            var current = p.Current.Evaluate(null);
            var next = p.Next.Evaluate(null);

            var useNext = this.ShouldUseNext(current, next);
            if (useNext)
            {
                p.MoveToNext();
            }

            p.Cool(useNext);

            sol.Current = p.Current;

            return sol;
        }
        protected virtual bool ShouldUseNext(float current, float next)
        {
            if (next < current)
            {
                return true;
            }
            else
            {
                var pro = this.problem as Problem;
                var p = math.pow(math.E, -(next - current) / (pro.k * pro.temperature));
                // LogTool.Log("p is " + p);

                if (p > ThreadSafeRandom.NextFloat())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

    }
}