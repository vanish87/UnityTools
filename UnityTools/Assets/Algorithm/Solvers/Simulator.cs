
namespace UnityTools.Algorithm
{
    [System.Serializable]
    public abstract class Simulator : IterationAlgorithm
    {
        public Simulator(IProblem problem, IDelta dt) : base(problem, dt)
        {
        }
    }
}