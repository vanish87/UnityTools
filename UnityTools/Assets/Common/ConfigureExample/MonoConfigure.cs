using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnityTools.Example
{
    [System.Serializable]
    public class ConfigureData
    {
        public string name = "this is a test";
        public string value = "new value added";
        public Vector3 posNew = new Vector3(1, 2, 3);
    }
    public class MonoConfigure : XmlConfig<ConfigureData>
    {
        [SerializeField] protected ConfigureData data;
        public override ConfigureData Data { get => this.data; set => this.data = value; }
        protected override string filePath { get { return System.IO.Path.Combine(Application.streamingAssetsPath, "config_mono.xml"); } }
    }
}