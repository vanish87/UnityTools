using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Common;
using UnityTools.Debuging;
using UnityTools.Attributes;

namespace UnityTools.Animation
{
    public class Timeline : ObjectStateMachine<Timeline>
    {
        [Serializable]
        public abstract class TimelineSate<T> : StateBase<T> where T : new()
        {
            [SerializeField, DisableEdit] internal protected float timer;
            [SerializeField, DisableEdit] internal protected float scale;
            [SerializeField] internal protected float scaleTime = 1;

            protected Action<T, TimelineSate<T>> enterActions;
            //StateMachine, localTimer
            protected Action<T, TimelineSate<T>,  float> excuteActions;
            protected Action<T, TimelineSate<T>> leaveActions;

            public void Enter(Action<T, TimelineSate<T>> enterAction)
            {
                this.enterActions -= enterAction;
                this.enterActions += enterAction;
            }
            public void Excute(Action<T, TimelineSate<T>, float> excuteAction)
            {
                this.excuteActions -= excuteAction;
                this.excuteActions += excuteAction;
            }
            public void Leave(Action<T, TimelineSate<T>> leaveAction)
            {
                this.leaveActions -= leaveAction;
                this.leaveActions += leaveAction;
            }

            internal override void Enter(T obj)
            {
                this.timer = 0;
                this.scale = 0;

                this.enterActions?.Invoke(obj, this);
            }
            internal override void Excute(T obj)
            {
                double inf = this.timer + Time.deltaTime;
                if(inf >= float.MaxValue) 
                {
                    LogTool.Log("Timer float over flow", LogLevel.Warning);
                    this.timer = 0;
                }

                this.timer += Time.deltaTime;

                var delta = (this.scaleTime > 0 ? 1 / this.scaleTime : 1) * Time.deltaTime;
                this.scale += delta;

                this.scale = Mathf.Clamp(this.scale, 0, 1);

                this.excuteActions?.Invoke(obj, this, this.timer);
            }

            internal override void Leave(T obj)
            {
                this.leaveActions?.Invoke(obj, this);
            }

        }
        [Serializable]
        public class TimelineEmptyState : EmptyState<Timeline>
        {
            public static TimelineEmptyState Instance { get => instance; }
            protected static TimelineEmptyState instance = new TimelineEmptyState();
        }


        [Serializable]
        public class TimelineGlobalState : TimelineSate<Timeline>
        {

        }


        [Serializable]
        public class TimelineSequence : TimelineSate<Timeline>
        {
            public enum Mode
            {
                Countdown,
                Timer,
                Infinite
            }
            [SerializeField] protected string name;
            [SerializeField] protected float duration;
            [SerializeField] protected Mode mode = Mode.Infinite;

            public TimelineSequence(string name, Mode mode, float duration = 0)
            {
                this.name = name;
                this.mode = mode;
                this.duration = duration;
                if(this.mode == Mode.Countdown) this.timer = duration;
            }
            internal override void Enter(Timeline obj)
            {
                base.Enter(obj);
            }
            internal override void Excute(Timeline obj)
            {
                switch(this.mode)
                {
                    case Mode.Timer:
                    {
                        base.Excute(obj);
                        if (this.timer > this.duration)
                        {
                            obj.MoveToNext();
                        }
                    }
                    break;
                    case Mode.Countdown:
                    {
                        this.timer -= Time.deltaTime;

                        var delta = (this.scaleTime > 0 ? 1 / this.scaleTime : 1) * Time.deltaTime;
                        this.scale += delta;

                        this.scale = Mathf.Clamp(this.scale, 0, 1);

                        this.excuteActions?.Invoke(obj, this, this.timer);
                        if (this.timer < 0)
                        {
                            obj.MoveToNext();
                        }
                    }
                    break;
                    case Mode.Infinite:
                    default:
                    {
                        base.Excute(obj);
                        break;
                    }

                }
            }
            internal override void Leave(Timeline obj)
            {
                base.Leave(obj);
            }



            public TimelineSequence DeepCopy(bool copyActions = true)
            {
                var ret = this.DeepCopyJson();
                if (copyActions)
                {
                    ret.enterActions = this.enterActions;
                    ret.excuteActions = this.excuteActions;
                    ret.leaveActions = this.leaveActions;
                }
                return ret;
            }
        }
        [Serializable]
        public class TimelineRestartSequence : TimelineSequence
        {
            public TimelineRestartSequence(string name = "Restart") : base(name, Mode.Timer)
            {
            }

            internal override void Enter(Timeline obj)
            {
                base.Enter(obj);

                obj.InitTimelineQueue();
                obj.MoveToNext();
            }
        }

        [SerializeField] protected List<TimelineSequence> sequence = new List<TimelineSequence>();
        protected Queue<TimelineSequence> timelineQueue = new Queue<TimelineSequence>();

        protected void Play()
        {
            this.InitTimelineQueue();
            this.MoveToNext();
        }

        protected void InitTimelineQueue()
        {
            this.timelineQueue.Clear();
            foreach (var t in this.sequence)
            {
                this.timelineQueue.Enqueue(t);
            }
            this.globalState = new TimelineGlobalState();
        }
        protected void MoveToNext()
        {
            if (this.timelineQueue.Count > 0)
            {
                //LogTool.Log("Move to next");
                var next = this.timelineQueue.Dequeue();
                this.ChangeState(next);
            }
            else
            {
                LogTool.Log("no state left move to the empty", LogLevel.Warning);
                this.ChangeState(TimelineEmptyState.Instance);
            }
        }
        protected void Stop()
        {
            this.ChangeState(TimelineEmptyState.Instance);
        }
    }
}
