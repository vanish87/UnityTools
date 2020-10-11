using System;
using System.Diagnostics;
using UnityTools.Common;
using UnityTools.Debuging;

namespace UnityTools.Algorithm
{
    public enum IterationAlgorithmMode
    {
        PerStep,
        FullStep
    }
    public interface IIterationAlgorithm : IAlgorithm
    {
        IterationAlgorithmMode RunMode { get; }
        ISolution CurrentSolution { get; }
        bool IsSolutionAcceptable(ISolution solution);
    }

    public class IterationDelta : IDelta
    {
        public float FixedDeltaTime => 1f / 60;
        public float DeltaTime =>this.dt;
        public float Current=>this.current;
        protected Stopwatch stopwatch = new Stopwatch();
        protected float dt = 0;
        protected float current = 0;
        
        public virtual void Reset()
        {
            this.stopwatch.Restart();
            this.dt = 0;
            this.current = 0;
        }

        public virtual void Step()
        {
            this.stopwatch.Stop();
            this.dt = this.stopwatch.Elapsed.Milliseconds / 1000f;
            this.current += this.dt;
            this.stopwatch.Restart();
        }

        public void FixStep()
        {
            this.dt = this.FixedDeltaTime;
            this.current += this.dt;
        }
    }
    /// <summary>
    /// It seems not necessary that to use IterationAlgorithm as MonoBehaviour
    /// Using Thread version below and checking solution/step algorithm 
    /// in MonoBehaviour's Update would be a better way to do this
    /// </summary>
    [System.Serializable]
    public abstract class IterationAlgorithmMono: ObjectStateMachine<IterationAlgorithmMono>, IIterationAlgorithm
    {
        protected IProblem problem;
        protected IDelta dt;
        protected ISolution currentSolution;
        protected IterationAlgorithmMode runMode = IterationAlgorithmMode.FullStep;

        protected Action<IProblem, ISolution, IDelta, IAlgorithm> startActions;
        protected Action<IProblem, ISolution, IDelta, IAlgorithm> perStepActions;
        protected Action<IProblem, ISolution, IDelta, IAlgorithm> endActions;

        public abstract bool IsSolutionAcceptable(ISolution solution);
        public abstract ISolution Solve(IProblem problem);

        public virtual IterationAlgorithmMode RunMode => this.runMode;
        public virtual IterationSateReady Ready { get => IterationSateReady.Instance; }
        public virtual IterationSateRunning Running { get => IterationSateRunning.Instance; }
        public virtual IterationSateDone Done { get => IterationSateDone.Instance; }

        public ISolution CurrentSolution => this.currentSolution;

        public void OnStart(Action<IProblem, ISolution, IDelta, IAlgorithm> startAction)
        {
            this.startActions -= startAction;
            this.startActions += startAction;
        }
        public void PerStep(Action<IProblem, ISolution, IDelta, IAlgorithm> perStepAction)
        {
            this.perStepActions -= perStepAction;
            this.perStepActions += perStepAction;
        }

        public void End(Action<IProblem, ISolution, IDelta, IAlgorithm> endAction)
        {
            this.endActions -= endAction;
            this.endActions += endAction;
        }
        public virtual void Init(IProblem problem, IDelta dt, IterationAlgorithmMode runMode = IterationAlgorithmMode.FullStep)
        {
            this.problem = problem;
            this.dt = dt;
            this.runMode = runMode;
            this.Reset();
        }

        public void Reset()
        {
            this.ChangeState(this.Ready);
        }

        public void TryToRun()
        {
            if(this.currentState == this.Running)
            {
                LogTool.Log("IterationAlgorithm is running, nothing to do");
            }
            else
            {
                this.ChangeState(this.Running);
            }
        }

        private ISolution SolveInternal()
        {
            if (this.RunMode == IterationAlgorithmMode.FullStep)
            {
                this.dt.Step();
            }
            else
            {
                (this.dt as IterationDelta).FixStep();
            }
                    
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
                obj.currentSolution = sol;
                obj.perStepActions?.Invoke(obj.problem, obj.CurrentSolution, obj.dt, obj);

                if (obj.IsSolutionAcceptable(sol))
                {
                    obj.ChangeState(obj.Done);
                }


                if (obj.RunMode == IterationAlgorithmMode.PerStep)
                {
                    obj.ChangeState(obj.Ready);
                }

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
        protected IterationAlgorithmMode runMode = IterationAlgorithmMode.FullStep;

        protected Action<IProblem, ISolution, IDelta, IAlgorithm> startActions;
        protected Action<IProblem, ISolution, IDelta, IAlgorithm> perStepActions;
        protected Action<IProblem, ISolution, IDelta, IAlgorithm> endActions;

        public abstract bool IsSolutionAcceptable(ISolution solution);
        public abstract ISolution Solve(IProblem problem);

        public virtual IterationAlgorithmMode RunMode { get => this.runMode; set => this.runMode = value; }
        public virtual IterationSateReady Ready { get => IterationSateReady.Instance; }
        public virtual IterationSateRunning Running { get => IterationSateRunning.Instance; }
        public virtual IterationSateDone Done { get => IterationSateDone.Instance; }
        public virtual IterationSatePause PauseState { get => IterationSatePause.Instance; }

        public ISolution CurrentSolution => this.currentSolution;
        public void Start(Action<IProblem, ISolution, IDelta, IAlgorithm> startAction)
        {
            this.startActions -= startAction;
            this.startActions += startAction;
        }
        public void PerStep(Action<IProblem, ISolution, IDelta, IAlgorithm> perStepAction)
        {
            this.perStepActions -= perStepAction;
            this.perStepActions += perStepAction;
        }

        public void End(Action<IProblem, ISolution, IDelta, IAlgorithm> endAction)
        {
            this.endActions -= endAction;
            this.endActions += endAction;
        }

        public IterationAlgorithm(IProblem problem, IDelta dt, IterationAlgorithmMode runMode = IterationAlgorithmMode.FullStep) : base()
        {
            this.problem = problem;
            this.dt = dt;
            this.runMode = runMode;
            this.Reset();
        }

        public void Reset()
        {
            this.ChangeState(this.Ready);
        }
        public virtual void TryToRun()
        {
            if (this.currentState == this.Running)
            {
                LogTool.Log("IterationAlgorithm is running, nothing to do");
            }
            else
            {
                this.ChangeState(this.Running);
            }
        }

        private ISolution SolveInternal()
        {
            if (this.RunMode == IterationAlgorithmMode.FullStep)
            {
                this.dt.Step();
            }
            else
            {
                (this.dt as IterationDelta)?.FixStep();
            }
            return this.Solve(this.problem);
        }

        [System.Serializable]
        public class IterationSateReady : StateBase<ObjectStateMachine>
        {
            public static IterationSateReady Instance { get => instance; }
            protected static IterationSateReady instance = new IterationSateReady();
            internal override void Enter(ObjectStateMachine obj) 
            {
                var sim = obj as IterationAlgorithm;
                sim.dt.Reset();
            }
            internal override void Excute(ObjectStateMachine obj) { }
            internal override void Leave(ObjectStateMachine obj) { }
        }
        [System.Serializable]
        public class IterationSatePause : StateBase<ObjectStateMachine>
        {
            public static IterationSatePause Instance { get => instance; }
            protected static IterationSatePause instance = new IterationSatePause();
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
                sim.startActions?.Invoke(sim.problem, sim.CurrentSolution, sim.dt, sim);
            }
            internal override void Excute(ObjectStateMachine obj)
            {
                var sim = obj as IterationAlgorithm;
                var sol = sim.SolveInternal();
                                             
                sim.currentSolution = sol;
                sim.perStepActions?.Invoke(sim.problem, sim.CurrentSolution, sim.dt, sim);

                if (sim.IsSolutionAcceptable(sol))
                {
                    sim.ChangeState(sim.Done);
                }

                if (sim.RunMode == IterationAlgorithmMode.PerStep)
                {
                    obj.ChangeState(sim.PauseState);
                }
            }

            internal override void Leave(ObjectStateMachine obj)
            {
            }
        }

        [System.Serializable]
        public class IterationSateDone : StateBase<ObjectStateMachine>
        {
            public static IterationSateDone Instance { get => instance; }
            protected static IterationSateDone instance = new IterationSateDone();
            internal override void Enter(ObjectStateMachine obj)
            {
                var sim = obj as IterationAlgorithm;
                sim.endActions?.Invoke(sim.problem, sim.CurrentSolution, sim.dt, sim);
            }
            internal override void Excute(ObjectStateMachine obj) { }
            internal override void Leave(ObjectStateMachine obj) { }
        }
    }
}
