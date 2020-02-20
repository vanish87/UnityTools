using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Debuging.EditorTool;

namespace UnityTools.Common
{
    [Serializable]
    public class Plane
    {
        public Vector3 center = new Vector3(0, 0, 0);
        public Vector3 rotation = new Vector3(0, 0, 0);
        public Vector3 scale = new Vector3(1, 1, 1);

        public Color color = Color.cyan;
        public bool displayText = true;
        
        public Matrix4x4 LocalToWorldMatrix { get { return Matrix4x4.TRS(this.center, Quaternion.Euler(this.rotation), this.scale); } }

        public Vector3[] localPoints = new Vector3[]
        {
            new Vector3(-0.5f,  0.5f, 0.5f),
            new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3( 0.5f, -0.5f, 0.5f),
            new Vector3( 0.5f,  0.5f, 0.5f),

            new Vector3(-0.5f,  0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3( 0.5f, -0.5f, -0.5f),
            new Vector3( 0.5f,  0.5f, -0.5f),
        };

        public void DrawGizmo()
        {
            #if UNITY_EDITOR
            using (new GizmosScope(this.color, this.LocalToWorldMatrix))
            {
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            }

            var xcenter = new Vector3(0, 0.5f, -0.5f);
            var ycenter = new Vector3(0.5f, 0, -0.5f);
            var zcenter = new Vector3(0.5f, 0.5f, 0);

            if (displayText)
            {
                var s = new GUIStyle();
                s.normal.textColor = this.color;
                UnityEditor.Handles.Label(this.LocalToWorldMatrix.MultiplyPoint(xcenter), "X = " + this.scale.x * Unit.UnityUnitToWorldMM + "mm", s);
                UnityEditor.Handles.Label(this.LocalToWorldMatrix.MultiplyPoint(ycenter), "Y = " + this.scale.y * Unit.UnityUnitToWorldMM + "mm", s);
                UnityEditor.Handles.Label(this.LocalToWorldMatrix.MultiplyPoint(zcenter), "Z = " + this.scale.z * Unit.UnityUnitToWorldMM + "mm", s);
            }
            #endif
        }
    }
    
    #if USE_EDITOR_EXC
    [ExecuteInEditMode]
    #endif
    public class SizeGrid : MonoBehaviour
    {
        [SerializeField] protected Plane plane = new Plane();

        protected virtual void OnDrawGizmos()
        {
            plane.DrawGizmo();
        }
    }

}
