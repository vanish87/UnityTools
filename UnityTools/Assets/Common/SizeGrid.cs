using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Debuging.EditorTool;

namespace UnityTools.Common
{   
    #if USE_EDITOR_EXC
    [ExecuteInEditMode]
    #endif
    public class SizeGrid : MonoBehaviour
    {
        [SerializeField] protected Area plane = new Area();

        protected virtual void OnDrawGizmos()
        {
            plane.OnDrawGizmo();
        }
    }

}
