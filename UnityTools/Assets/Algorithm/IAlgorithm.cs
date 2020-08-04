

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
}