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
            [SerializeField, DisableEdit] protected float timer;
            [SerializeField, DisableEdit] protected float scale;
            [SerializeField] protected float scaleTime = 1;

            protected Action<T> enterActions;
            //StateMachine, localTimer
            protected Action<T, float> excuteActions;
            protected Action<T> leaveActions;

            public void Enter(Action<T> enterAction)
            {
                this.enterActions -= enterAction;
                this.enterActions += enterAction;
            }
            public void Excute(Action<T, float> excuteAction)
            {
                this.excuteActions -= excuteAction;
                this.excuteActions += excuteAction;
            }
            public void Leave(Action<T> leaveAction)
            {
                this.leaveActions -= leaveAction;
                this.leaveActions += leaveAction;
            }

            internal override void Enter(T obj)
            {
                this.timer = 0;
                this.scale = 0;

                this.enterActions?.Invoke(obj);
            }
            internal override void Excute(T obj)
            {
                this.timer += Time.deltaTime;

                var delta = (this.scaleTime > 0 ? 1 / this.scaleTime : 1) * Time.deltaTime;
                this.scale += delta;

                this.scale = Mathf.Clamp(this.scale, 0, 1);

                this.excuteActions?.Invoke(obj, this.timer);
            }

            internal override void Leave(T obj)
            {
                this.leaveActions?.Invoke(obj);
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
            public float duration;
            internal override void Enter(Timeline obj)
            {
                base.Enter(obj);
            }
            internal override void Excute(Timeline obj)
            {
                base.Excute(obj);
                if (this.timer > this.duration)
                {
                    obj.MoveToNext();
                }
            }
            internal override void Leave(Timeline obj)
            {
                base.Leave(obj);
            }
        }

        [SerializeField] protected List<TimelineSequence> sequence = new List<TimelineSequence>();
        protected Queue<TimelineSequence> timelineQueue = new Queue<TimelineSequence>();

        protected virtual void Start()
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
            this.globalState_ = new TimelineGlobalState();
        }
        protected void MoveToNext()
        {
            if (this.timelineQueue.Count > 0)
            {
                LogTool.Log("Move to next");
                var next = this.timelineQueue.Dequeue();
                this.ChangeState(next);
            }
            else
            {
                LogTool.Log("to the end");
                this.ChangeState(TimelineEmptyState.Instance);
            }
        }
    }
}
