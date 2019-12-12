using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Physics
{
    public class Controller : MonoBehaviour, Kinetics.IKineticsUser, Navigation.INavigationUser
    {
        public Kinetics kinetics { get ; set ; }
        public List<Navigation> Navigations { get =>this.navigations; }

        protected List<Navigation> navigations = new List<Navigation>();

        protected virtual void Integrate(float dt)
        {

        }

        protected virtual void Update()
        {
            var dt = Time.deltaTime;

            this.kinetics.Begin();
            foreach(var n in this.Navigations)
            {
                this.kinetics.AddForce(n.GetForce(dt, this.kinetics));
            }
            this.kinetics.Interagte(dt);

            this.Integrate(dt);
        }
    }
}