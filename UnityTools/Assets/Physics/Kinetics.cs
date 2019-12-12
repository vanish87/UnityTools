using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Physics
{
    public class Kinetics : MonoBehaviour
    {
        public interface IKineticsUser
        {
            Kinetics kinetics { get; set; }
        }
        protected void OnNotify()
        {
            var k = this.GetComponents<IKineticsUser>();
            foreach(var ki in k)
            {
                ki.kinetics = this;
            }
        }
        public Vector3 Velocity { get => this.velocity; }
        public Vector3 Acceleration { get => this.acceleration; }
        [SerializeField] protected Vector3 velocity;
        [SerializeField] protected Vector3 acceleration;

        public void Begin()
        {
            this.acceleration = Vector3.zero;
        }

        public void AddForce(Vector3 force)
        {
            this.acceleration += force;
        }

        public void Interagte(float dt)
        {
            this.velocity += this.acceleration * dt;
        }

        protected void Start()
        {
            this.OnNotify();
        }
    }
}