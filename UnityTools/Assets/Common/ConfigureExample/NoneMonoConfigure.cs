using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnityTools.Example
{
    [System.Serializable]
    public class NoneMonoConfigureClass : ConfigNoneMono<ConfigureData>
    {
        [SerializeField] protected ConfigureData data;
        public override ConfigureData Data { get => this.data; set => this.data = value; }
        protected override string filePath { get { return System.IO.Path.Combine(Application.streamingAssetsPath, "config_none_mono.xml"); } }
    }

    public class NoneMonoConfigure : MonoBehaviour
    {
        [SerializeField] protected NoneMonoConfigureClass configure = new NoneMonoConfigureClass();

        protected void Start()
        {
            configure.LoadAndNotify();
            configure.Save();
        }

    }
}
