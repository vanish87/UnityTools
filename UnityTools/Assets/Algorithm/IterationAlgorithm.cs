using UnityTools.Common;

namespace UnityTools.Algorithm
{
    public interface IIterationAlgorithm : IAlgorithm
    {
        ISolution CurrentSolution { get; }
        bool IsSolutionAcceptable(ISolution solution);
    }

    public abstract class IterationAlgorithm : ObjectStateMachine, IIterationAlgorithm
    {
        protected IProblem problem;
        protected IDelta dt;
        protected ISolution currentSolution;

        public abstract bool IsSolutionAcceptable(ISolution solution);
        public abstract ISolution Solve(IProblem problem);

        public virtual SimulatorSateReady Ready { get => SimulatorSateReady.Instance; }
        public virtual SimulatorSateRunning Running { get => SimulatorSateRunning.Instance; }
        public virtual SimulatorSateDone Done { get => SimulatorSateDone.Instance; }

        public ISolution CurrentSolution => this.currentSolution;

        public IterationAlgorithm(IProblem problem, IDelta dt) : base()
        {
            this.problem = problem;
            this.dt = dt;
            this.Reset();
        }

        public void Reset()
        {
            this.ChangeState(this.Ready);
        }

        private ISolution SolveInternal()
        {
            this.dt.Step();
            return this.Solve(this.problem);
        }

        [System.Serializable]
        public class SimulatorSateReady : StateBase<ObjectStateMachine>
        {
            public static SimulatorSateReady Instance { get => instance; }
            protected static SimulatorSateReady instance = new SimulatorSateReady();
            internal override void Enter(ObjectStateMachine obj) { }
            internal override void Excute(ObjectStateMachine obj) { }
            internal override void Leave(ObjectStateMachine obj) { }
        }

        [System.Serializable]
        public class SimulatorSateRunning : StateBase<ObjectStateMachine>
        {
            public static SimulatorSateRunning Instance { get => instance; }
            protected static SimulatorSateRunning instance = new SimulatorSateRunning();

            internal override void Enter(ObjectStateMachine obj)
            {
                var sim = obj as IterationAlgorithm;
                sim.dt.Reset();
            }
            internal override void Excute(ObjectStateMachine obj)
            {
                var sim = obj as IterationAlgorithm;
                var sol = sim.SolveInternal();

                if (sim.IsSolutionAcceptable(sol))
                {
                    sim.ChangeState(SimulatorSateDone.Instance);
                }

                sim.currentSolution = sol;
            }

            internal override void Leave(ObjectStateMachine obj)
            {
            }
        }

        [System.Serializable]
        public class SimulatorSateDone : StateBase<ObjectStateMachine>
        {
            public static SimulatorSateDone Instance { get => instance; }
            protected static SimulatorSateDone instance = new SimulatorSateDone();
            internal override void Enter(ObjectStateMachine obj) { }
            internal override void Excute(ObjectStateMachine obj) { }
            internal override void Leave(ObjectStateMachine obj) { }
        }
    }
}
