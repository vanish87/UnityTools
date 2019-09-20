using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools
{
    public static class ObjectTool
    {
        public static void DestoryObj(this Object obj, float t = 0f)
        {
            if (obj != null)
            {
                if (Application.isPlaying)
                    Object.Destroy(obj, t);
                else
                    Object.DestroyImmediate(obj);
            }
        }
    }
}