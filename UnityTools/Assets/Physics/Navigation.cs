using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Physics
{
    public class Navigation : MonoBehaviour
    {
        public interface INavigationUser
        {
            List<Navigation> Navigations { get;}
        }
        protected void OnNotify()
        {
            var navis = this.GetComponents<INavigationUser>();
            foreach(var n in navis)
            {
                if (n.Navigations == null) continue;

                if(n.Navigations.Contains(this) == false)
                {
                    n.Navigations.Add(this);
                }
            }
        }

        protected void Start()
        {
            this.OnNotify();
        }

        protected virtual float Power { get; }
        public virtual Vector3 GetForce(float dt, Kinetics k)
        {
            return Vector3.zero;
        }


    }
}