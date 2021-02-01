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
    public class PCConfigure : Configure<PCConfigureData>
    {
    }
}
