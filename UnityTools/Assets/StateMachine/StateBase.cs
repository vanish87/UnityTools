using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.StateMachine
{
    public abstract class StateBase<T>
    {
        abstract public void Enter(T obj);
        abstract public void Excute(T obj);
        abstract public void Leave(T obj);

        public virtual void OnMessage(T obj, Event msg)
        {
            
        }
    }

    public class ExampleState : StateBase<ExampleStateMachine>
    {
        public static ExampleState Instance { get { return instance; } }
        private static readonly ExampleState instance = new ExampleState();
        private ExampleState() { }

        override public void Enter(ExampleStateMachine obj)
        {
        }

        override public void Excute(ExampleStateMachine obj)
        {
        }

        override public void Leave(ExampleStateMachine obj)
        {
        }
    }

}