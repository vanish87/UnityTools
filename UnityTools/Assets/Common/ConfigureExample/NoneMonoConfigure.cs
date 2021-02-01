using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Common;

namespace UnityTools.Example
{
    [System.Serializable]
    public class NoneMonoConfigureClass : ConfigureNoneMono<ConfigureData>
    {
    }

    public class NoneMonoConfigure : MonoBehaviour
    {
        //[SerializeField] protected NoneMonoConfigureClass configure;

        protected void Start()
        {
            //configure =  new NoneMonoConfigureClass();
            //configure.LoadAndNotify();
            //configure.Save();
        }

    }
}
