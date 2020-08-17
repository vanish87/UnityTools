

using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityTools.Common;

namespace UnityTools.Algorithm
{
    public interface IAlgorithm
    {
        ISolution Solve(IProblem problem);
    }

    public interface IProblem
    {

    }

    public interface ISolution
    {

    }

    public interface IDelta
    {
        void Reset();
        void Step();
    }

    public interface IParticle
    {

    }
    public interface IState<S>
    {
        public S State { get; set; }
        S Dev(int order);
    }

    public abstract class ParticleSystemBase<Data, S> : IState<S>, IProblem where Data : IParticle
    {
        protected List<Data> particles = new List<Data>();

        public abstract S State { get; set; }
        public abstract S Dev(int order);
    }

    public class ParticleEulerSolver : IterationAlgorithmMono
    {
        public ParticleEulerSolver(IProblem problem, IDelta dt) : base(problem, dt)
        {
        }

        public override bool IsSolutionAcceptable(ISolution solution)
        {
            throw new System.NotImplementedException();
        }

        public override ISolution Solve(IProblem problem)
        {
            throw new System.NotImplementedException();
        }
    }

    public class Particle: IParticle
    {
        public float3 position;
        public float3 velocity;
        public float3 force;
    }
    public class ParticleSystem : ParticleSystemBase<Particle, Vector<float3>>
    {
        public override Vector<float3> State 
        {
            get 
            {
                var ret = new Vector<float3>(this.particles.Count * 2);
                for (var i = 0; i < ret.Size; i += 2)
                {
                    ret[i] = this.particles[i].position;
                    ret[i+1] = this.particles[i].velocity;
                }

                return ret;
            } set => throw new System.NotImplementedException(); }

        public override Vector<float3> Dev(int order)
        {
            throw new System.NotImplementedException();
        }
    }

}