using System;
using UnityTools.Common;

namespace UnityTools.Algorithm
{
    public interface IIterationAlgorithm : IAlgorithm
    {
        ISolution CurrentSolution { get; }
        bool IsSolutionAcceptable(ISolution solution);
    }
    [System.Serializable]
    public abstract class IterationAlgorithmMono: ObjectStateMachine<IterationAlgorithmMono>, IIterationAlgorithm
    {
        protected IProblem problem;
        protected IDelta dt;
        protected ISolution currentSolution;

        protected Action<IProblem, ISolution, IDelta, IAlgorithm> startActions;
        protected Action<IProblem, ISolution, IDelta, IAlgorithm> endActions;

        public abstract bool IsSolutionAcceptable(ISolution solution);
        public abstract ISolution Solve(IProblem problem);

        public virtual IterationSateReady Ready { get => IterationSateReady.Instance; }
        public virtual IterationSateRunning Running { get => IterationSateRunning.Instance; }
        public virtual IterationSateDone Done { get => IterationSateDone.Instance; }

        public ISolution CurrentSolution => this.currentSolution;

        public void Start(Action<IProblem, ISolution, IDelta, IAlgorithm> startAction)
        {
            this.startActions -= startAction;
            this.startActions += startAction;
        }

        public void End(Action<IProblem, ISolution, IDelta, IAlgorithm> endAction)
        {
            this.endActions -= endAction;
            this.endActions += endAction;
        }
        public IterationAlgorithmMono(IProblem problem, IDelta dt) : base()
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
        public class IterationSateReady : StateBase<IterationAlgorithmMono>
        {
            public static IterationSateReady Instance { get => instance; }
            protected static IterationSateReady instance = new IterationSateReady();
            internal override void Enter(IterationAlgorithmMono obj) { }
            internal override void Excute(IterationAlgorithmMono obj) { }
            internal override void Leave(IterationAlgorithmMono obj) { }
        }

        [System.Serializable]
        public class IterationSateRunning : StateBase<IterationAlgorithmMono>
        {
            public static IterationSateRunning Instance { get => instance; }
            protected static IterationSateRunning instance = new IterationSateRunning();

            internal override void Enter(IterationAlgorithmMono obj)
            {
                obj.dt.Reset();
                obj.startActions?.Invoke(obj.problem, obj.CurrentSolution, obj.dt, obj);
            }
            internal override void Excute(IterationAlgorithmMono obj)
            {
                var sol = obj.SolveInternal();

                if (obj.IsSolutionAcceptable(sol))
                {
                    obj.ChangeState(obj.Done);
                }

                obj.currentSolution = sol;
            }

            internal override void Leave(IterationAlgorithmMono obj)
            {
                obj.endActions?.Invoke(obj.problem, obj.CurrentSolution, obj.dt, obj);
            }
        }

        [System.Serializable]
        public class IterationSateDone : StateBase<IterationAlgorithmMono>
        {
            public static IterationSateDone Instance { get => instance; }
            protected static IterationSateDone instance = new IterationSateDone();
            internal override void Enter(IterationAlgorithmMono obj) { }
            internal override void Excute(IterationAlgorithmMono obj) { }
            internal override void Leave(IterationAlgorithmMono obj) { }
        }
    }
    [System.Serializable]
    public abstract class IterationAlgorithm : ObjectStateMachine, IIterationAlgorithm
    {
        protected IProblem problem;
        protected IDelta dt;
        protected ISolution currentSolution;

        protected Action<IProblem, ISolution, IDelta, IAlgorithm> startActions;
        protected Action<IProblem, ISolution, IDelta, IAlgorithm> endActions;

        public abstract bool IsSolutionAcceptable(ISolution solution);
        public abstract ISolution Solve(IProblem problem);

        public virtual IterationSateReady Ready { get => IterationSateReady.Instance; }
        public virtual IterationSateRunning Running { get => IterationSateRunning.Instance; }
        public virtual IterationSateDone Done { get => IterationSateDone.Instance; }

        public ISolution CurrentSolution => this.currentSolution;
        public void Start(Action<IProblem, ISolution, IDelta, IAlgorithm> startAction)
        {
            this.startActions -= startAction;
            this.startActions += startAction;
        }

        public void End(Action<IProblem, ISolution, IDelta, IAlgorithm> endAction)
        {
            this.endActions -= endAction;
            this.endActions += endAction;
        }

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
        public class IterationSateReady : StateBase<ObjectStateMachine>
        {
            public static IterationSateReady Instance { get => instance; }
            protected static IterationSateReady instance = new IterationSateReady();
            internal override void Enter(ObjectStateMachine obj) { }
            internal override void Excute(ObjectStateMachine obj) { }
            internal override void Leave(ObjectStateMachine obj) { }
        }

        [System.Serializable]
        public class IterationSateRunning : StateBase<ObjectStateMachine>
        {
            public static IterationSateRunning Instance { get => instance; }
            protected static IterationSateRunning instance = new IterationSateRunning();

            internal override void Enter(ObjectStateMachine obj)
            {
                var sim = obj as IterationAlgorithm;
                sim.dt.Reset();

                sim.startActions?.Invoke(sim.problem, sim.CurrentSolution, sim.dt, sim);
            }
            internal override void Excute(ObjectStateMachine obj)
            {
                var sim = obj as IterationAlgorithm;
                var sol = sim.SolveInternal();

                if (sim.IsSolutionAcceptable(sol))
                {
                    sim.ChangeState(sim.Done);
                }

                sim.currentSolution = sol;
            }

            internal override void Leave(ObjectStateMachine obj)
            {
                var sim = obj as IterationAlgorithm;
                sim.endActions?.Invoke(sim.problem, sim.CurrentSolution, sim.dt, sim);
            }
        }

        [System.Serializable]
        public class IterationSateDone : StateBase<ObjectStateMachine>
        {
            public static IterationSateDone Instance { get => instance; }
            protected static IterationSateDone instance = new IterationSateDone();
            internal override void Enter(ObjectStateMachine obj) { }
            internal override void Excute(ObjectStateMachine obj) { }
            internal override void Leave(ObjectStateMachine obj) { }
        }
    }
}
