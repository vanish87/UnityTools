using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Assertions;
using UnityTools.Common;

namespace UnityTools.Debuging.EditorTool
{
    public class GizmosScope : Scope
    {
        protected Color oldColor;
        protected Matrix4x4 oldMatrix;

        public GizmosScope(Color color, Matrix4x4 mat)
            : base()
        {
            this.oldColor = Gizmos.color;
            this.oldMatrix = Gizmos.matrix;

            Gizmos.color = color;
            Gizmos.matrix = mat;
        }
        protected override void DisposeManaged()
        {
            base.DisposeManaged();

            Gizmos.color = this.oldColor;
            Gizmos.matrix = this.oldMatrix;
        }
    }
}
