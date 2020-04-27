using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Common.Example
{
    [Serializable]
    public class PCConfigureData
    {
        public List<PCInfo> pcList = new List<PCInfo>();
    }    

    #if USE_EDITOR_EXC
    [ExecuteInEditMode]
    #endif
    public class PCConfigure : Config<PCConfigureData>
    {
        [SerializeField] protected string fileName = "PCConfigure.xml";
        [SerializeField] protected PCConfigureData data;
        public override PCConfigureData Data { get => this.data; set => this.data = value; }


        protected override string filePath { get { return System.IO.Path.Combine(Application.streamingAssetsPath, this.fileName); } }
    }
}
