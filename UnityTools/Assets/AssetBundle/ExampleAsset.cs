using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.AssetBundle
{
    public class ExampleAsset : AssetStruct<ExampleAsset, ExternalStruct>
    {
        public string assetName;
        public Texture2D texture;

        public override void UpdateFields(ExternalStruct data)
        {
            this.assetName = data.name;
            this.texture = new Texture2D(256, 256);
            this.texture.name = data.name;
        }
    }
    public class ExternalStruct
    {
        public string name;
    }
}