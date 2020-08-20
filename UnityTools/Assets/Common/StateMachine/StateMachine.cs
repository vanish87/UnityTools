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

        public ObjectStateMachine()
        {
            this.thread = new Thread(new ThreadStart(this.ThreadMain));
            this.thread.Name = System.Environment.StackTrace;
            this.thread.Start();

            ThreadDebug.Add(this.thread);

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += this.PlayModeStateChanged;
            #endif
        }
        #if UNITY_EDITOR
        protected void PlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                this.StopThread(true);
            }
        }
        #endif

        ~ObjectStateMachine()
        {
            this.StopThread(true);
            ThreadDebug.Remove(this.thread);
        }

        protected override void DisposeManaged()
        {
            this.StopThread();
        }

        public void StopThread(bool force = false)
        {
            lock(this)
            {
                this.state = ThreadState.Stopped;
            }

            if (force) this.thread.Abort();
        }
        public void Pause()
        {
            lock (this)
            {
                this.state = ThreadState.Pause;
            }
        }

        public void ChangeState(StateBase<ObjectStateMachine> newState)
        {
            lock (this)
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

            lock (this)
            {
                current = this.state = ThreadState.Running;
            }
            try
            {
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

                    lock (this)
                    {
                        current = this.state;
                    }

                    //Debug.Log("running");
                }

                LogTool.Log("Thread Stopped " + this.ToString());
            }
            catch (ThreadAbortException abortException)
            {
                LogTool.Log("Thread Stopped by Abort " + abortException.Message);
            }
        }

    }
    public class ObjectStateMachine<T> : MonoBehaviour where T : class
    {
        [SerializeField] protected StateBase<T> currentState;
        [SerializeField] protected StateBase<T> globalState;

        private T owner = null;

        /// <summary>
        /// here, some objects may not find a owner when calling OnEnable function
        /// because the script T may be attached later
        /// so it is better to GetComponent again when using owner later
        /// </summary>
        private T Owner
        {
            get
            {
                if (this.owner == null)
                {
                    this.owner = this.GetComponent<T>();
                }
                Assert.IsNotNull(owner);
                return this.owner;
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
            this.owner = null;
        }

        public void ChangeState(StateBase<T> newState)
        {
            if (this.currentState == newState)
            {
                LogTool.Log("Current state is same with new state, Restart", LogLevel.Warning);
            }

            if (this.currentState != null)
            {
                this.currentState.Leave(this.Owner);
            }

            this.currentState = newState;

            this.currentState.Enter(this.Owner);
        }

        protected virtual void Update()
        {
            if (this.globalState != null)
            {
                this.globalState.Excute(this.Owner);
            }
            if (this.currentState != null)
            {
                this.currentState.Excute(this.Owner);
            }
        }
    }


    public class ExampleStateMachine : ObjectStateMachine<ExampleStateMachine>
    {

    }
}

