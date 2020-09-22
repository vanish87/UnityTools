
namespace UnityTools.Algorithm
{
    [System.Serializable]
    public abstract class Simulator : IterationAlgorithm
    {
        public Simulator(IProblem problem, IDelta dt, IterationAlgorithmMode mode = IterationAlgorithmMode.FullStep) : base(problem, dt, mode)
        {
        }
    }

    [System.Serializable]
    public abstract class SimulatorMono : IterationAlgorithmMono
    {

    }
}