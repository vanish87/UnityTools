using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Debuging;

namespace UnityTools.Common
{
    [Serializable]
    public abstract class StateBase<T>
    {
        internal abstract void Enter(T obj);
        internal abstract void Excute(T obj);
        internal abstract void Leave(T obj);

        public virtual void OnMessage(T obj, Event msg)
        {

        }
    }
    [Serializable]
    public abstract class StateBaseStatic<T> : StateBase<T>
    {
        protected StateBaseStatic() { }
    }
    [Serializable]
    public abstract class EmptyState<T> : StateBaseStatic<T> 
    {
        internal override void Enter(T obj)
        {
            LogTool.Log("Enter Empty");
        }
        internal override void Excute(T obj)
        {
        }
        internal override void Leave(T obj)
        {
        }
    }

    public class ExampleState : StateBaseStatic<ExampleStateMachine>
    {
        internal override void Enter(ExampleStateMachine obj)
        {

        }

        internal override void Excute(ExampleStateMachine obj)
        {
        }

        internal override void Leave(ExampleStateMachine obj)
        {
        }
    }

}