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
        public Vector2 posNew1 = new Vector2(2, 3);
        public float sliderFloat = 10;
    }
    public class MonoConfigure : XmlConfig<ConfigureData>
    {
        [SerializeField] protected ConfigureData data;
        public override ConfigureData Data { get => this.data; set => this.data = value; }
        protected override string filePath { get { return System.IO.Path.Combine(Application.streamingAssetsPath, "config_mono.xml"); } }

        protected override void OnDrawGUI()
        {
            base.OnDrawGUI();

            ConfigureGUI.OnGUI(ref this.data.name, "GUI name");
            ConfigureGUI.OnGUI(ref this.data.posNew, "new Position");
            ConfigureGUI.OnGUISlider(ref this.data.sliderFloat, 0, 10, "Slider");
            ConfigureGUI.OnGUISlider(ref this.data.posNew1, Vector2.zero, Vector2.one * 10, "slider vector2");
        }
    }
}