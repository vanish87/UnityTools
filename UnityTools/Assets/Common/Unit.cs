using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Common
{
    public class Unit
    {
        //100mm(10cm) in Real world become 1 in Unity
        public const float WorldMMToUnityUnit = 1.0f/100;
        public const float UnityUnitToWorldMM = 1 / WorldMMToUnityUnit;
    }

}
