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
    public class MonoConfigure : Configure<ConfigureData>
    {
    }
}