using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Example
{
    public class SingletonMonoConfigure : XmlConfigMonoSingleton<ConfigureData>
    {
        [SerializeField] protected ConfigureData data;
        public override ConfigureData Data { get => this.data; set => this.data = value; }
        protected override string filePath { get { return System.IO.Path.Combine(Application.streamingAssetsPath, "config_singleton_mono.xml"); } }
    }
}
