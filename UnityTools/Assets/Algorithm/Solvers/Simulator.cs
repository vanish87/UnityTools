
namespace UnityTools.Algorithm
{
    public abstract class Simulator : IterationAlgorithm
    {
        public Simulator(IProblem problem, IDelta dt) : base(problem, dt)
        {
        }
    }
}