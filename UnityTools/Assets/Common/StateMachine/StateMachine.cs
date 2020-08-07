using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;
using UnityTools.Debuging;

namespace UnityTools.Common
{
    public class ThreadDebug
    {
        protected static List<Thread> currentThread = new List<Thread>();
        public static void Add(Thread thrad) { currentThread.Add(thrad); }
        public static void Remove(Thread thrad) { currentThread.Remove(thrad); }

        public static void Print() { foreach(var t in currentThread) { LogTool.Log(t.Name + " is running"); } }
    }
    [System.Serializable]
    public class ObjectStateMachine : Disposable
    {
        private enum ThreadState
        {
            Ready,
            Running,
            Pause,
            Stopped,
        }
        [SerializeField] protected StateBase<ObjectStateMachine> currentState = null;
        [SerializeField] protected StateBase<ObjectStateMachine> globalState = null;

        [NonSerialized] private Thread thread;
        [NonSerialized] private ThreadState state = ThreadState.Ready;
        protected object lockObj = new object();

        public ObjectStateMachine()
        {
            this.thread = new Thread(new ThreadStart(this.ThreadMain));
            this.thread.Name = System.Environment.StackTrace;
            this.thread.Start();

            ThreadDebug.Add(this.thread);
        }

        ~ObjectStateMachine()
        {
            this.Stop();
        }

        protected override void DisposeManaged()
        {
            this.Stop();
        }

        public void Stop()
        {
            lock(this.lockObj)
            {
                this.state = ThreadState.Stopped;
            }

            ThreadDebug.Remove(this.thread);
        }
        public void Pause()
        {
            lock (this.lockObj)
            {
                this.state = ThreadState.Pause;
            }
        }

        public void ChangeState(StateBase<ObjectStateMachine> newState)
        {
            lock (this.lockObj)
            {
                if (this.currentState == newState)
                {
                    LogTool.Log("Current state is same with new state, Restart", LogLevel.Warning);
                }

                if (this.currentState != null)
                {
                    this.currentState.Leave(this);
                }

                this.currentState = newState;

                this.currentState.Enter(this);
            }
        }

        private void ThreadMain()
        {
            var current = ThreadState.Ready;

            lock (this.lockObj)
            {
                current = this.state = ThreadState.Running;
            }

            while (current != ThreadState.Stopped)
            {
                if (current == ThreadState.Running)
                {
                    if (this.globalState != null)
                    {
                        this.globalState.Excute(this);
                    }
                    if (this.currentState != null)
                    {
                        this.currentState.Excute(this);
                    }
                }

                lock (this.lockObj)
                {
                    current = this.state;
                }

                Debug.Log("running");
            }

            Debug.Log("Stoped");
        }

    }
    public class ObjectStateMachine<T> : MonoBehaviour where T : class
    {
        [SerializeField] protected StateBase<T> currentState_;
        [SerializeField] protected StateBase<T> globalState_;

        private T owner_ = null;

        /// <summary>
        /// here, some objects may not find a owner when calling OnEnable function
        /// because the script T may be attached later
        /// so it is better to GetComponent again when using owner later
        /// </summary>
        private T Owner
        {
            get
            {
                if (this.owner_ == null)
                {
                    this.owner_ = this.GetComponent<T>();
                }
                Assert.IsNotNull(owner_);
                return this.owner_;
            }
        }

        /// <summary>
        /// here, some objects may not find a owner
        /// because the script T may be attached later
        /// so it is better to GetComponent again when using owner later
        /// </summary>
        protected virtual void OnEnable()
        {

        }

        protected virtual void OnDisable()
        {
            this.owner_ = null;
        }

        public void ChangeState(StateBase<T> newState)
        {
            if (this.currentState_ == newState)
            {
                LogTool.Log("Current state is same with new state, Restart", LogLevel.Warning);
            }

            if (this.currentState_ != null)
            {
                this.currentState_.Leave(this.Owner);
            }

            this.currentState_ = newState;

            this.currentState_.Enter(this.Owner);
        }

        protected virtual void Update()
        {
            if (this.globalState_ != null)
            {
                this.globalState_.Excute(this.Owner);
            }
            if (this.currentState_ != null)
            {
                this.currentState_.Excute(this.Owner);
            }
        }
    }


    public class ExampleStateMachine : ObjectStateMachine<ExampleStateMachine>
    {

    }
}

