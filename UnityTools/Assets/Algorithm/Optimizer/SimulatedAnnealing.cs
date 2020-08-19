using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityTools.Debuging;

namespace UnityTools.Algorithm
{
    public class ThreadSafeRandom
    {
        private static readonly System.Random _global = new System.Random();
        [System.ThreadStatic] private static System.Random _local;
        public static float NextFloat()
        {
            if (_local == null)
            {
                lock (_global)
                {
                    if (_local == null)
                    {
                        int seed = _global.Next();
                        _local = new System.Random(seed);
                    }
                }
            }

            return _local.Next() * 1.0f / System.Int32.MaxValue;
        }
        public static int Next()
        {
            if (_local == null)
            {
                lock (_global)
                {
                    if (_local == null)
                    {
                        int seed = _global.Next();
                        _local = new System.Random(seed);
                    }
                }
            }

            return _local.Next();
        }
    }
    public class SimulatedAnnealing : IterationAlgorithm
    {
        public interface State
        {
            float E { get; }
            void UpdateNewValue();
        }

        public abstract class Problem : IProblem
        {
            internal protected float temperature;
            internal protected float minTemperature;
            internal protected float alpha;

            public abstract State Current { get; }
            public abstract State Next { get; }

            public abstract void MoveToNext();
        }
        public class Solution : ISolution
        {
            public State Current { get; internal set; }
        }


        public SimulatedAnnealing(IProblem problem, IDelta dt) : base(problem, dt)
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

            var current = p.Current.E;
            var next = p.Next.E;

            if(this.ShouldUseNext(current, next))
            {
                p.MoveToNext();
                p.temperature *= p.alpha;
            }
            else
            {
                p.Next.UpdateNewValue();
            }

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
                var p = math.pow(math.E, -(next - current) / pro.temperature);
                //LogTool.Log("p is " + p);

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