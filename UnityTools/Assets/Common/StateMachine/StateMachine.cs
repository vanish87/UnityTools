using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityTools.Common
{
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

