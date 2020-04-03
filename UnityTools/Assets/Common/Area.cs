using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Debuging;
using UnityTools.Debuging.EditorTool;

namespace UnityTools.Common
{
    [Serializable]
    public class Area
    {
        public Vector3 center = new Vector3(0, 0, 0);
        public Vector3 rotation = new Vector3(0, 0, 0);
        public Vector3 scale = new Vector3(1, 1, 1);

        public Color color = Color.cyan;
        public bool displayText = true;

        public Matrix4x4 LocalToWorldMatrix 
        { 
            get 
            {
                LogTool.LogAssertIsTrue(this.scale.x != 0, "Scale should not be 0");
                LogTool.LogAssertIsTrue(this.scale.y != 0, "Scale should not be 0");
                LogTool.LogAssertIsTrue(this.scale.z != 0, "Scale should not be 0");
                return Matrix4x4.TRS(this.center, Quaternion.Euler(this.rotation), this.scale);
            }
        }
        public Matrix4x4 WorldToLocalMatrix { get { return this.LocalToWorldMatrix.inverse; } }
        public Bounds Bound 
        { 
            get 
            {
                Bounds ret = new Bounds();
                ret.center = this.center;
                foreach(var lp in this.localPoints)
                {
                    ret.Encapsulate(this.LocalToWorldMatrix.MultiplyPoint(lp));
                }
                return ret;
            }
        }

        public readonly Vector3[] localPoints = new Vector3[]
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

        public Vector3 WorldToLocalPosition(Vector3 worldPos, bool is01 = true, bool revert = false)
        {
            var local = this.WorldToLocalMatrix.MultiplyPoint(worldPos);
            if (is01)
            {
                local.x = local.x + 0.5f;
                local.y = local.y + 0.5f;
                local.z = local.z + 0.5f;
            }
            if (revert)
            {
                local.x = 1 - local.x;
                local.y = 1 - local.y;
                local.z = 1 - local.z;
            }
            return local;
        }
        public Vector3 LocalToWordPosition(Vector3 normalizedPos, bool is01 = true, bool revert = false)
        {
            if(is01)
            {
                normalizedPos.x = normalizedPos.x - 0.5f;
                normalizedPos.y = normalizedPos.y - 0.5f;
                normalizedPos.z = normalizedPos.z - 0.5f;
            }
            if (revert)
            {
                normalizedPos.x = 1 - normalizedPos.x;
                normalizedPos.y = 1 - normalizedPos.y;
                normalizedPos.z = 1 - normalizedPos.z;
            }
            return this.LocalToWorldMatrix.MultiplyPoint(normalizedPos);
        }
        public void OnDrawGizmo()
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

}
