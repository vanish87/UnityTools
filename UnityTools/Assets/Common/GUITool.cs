using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Assertions;
using UnityTools.Common;

namespace UnityTools.GUITool
{
    public class ColorScope : Scope
    {
        protected Color oldColor;
        public ColorScope(Color color)
            : base()
        {
            this.oldColor = GUI.color;
            GUI.color = color;
        }
        protected override void DisposeManaged()
        {
            base.DisposeManaged();
            GUI.color = this.oldColor;
        }
    }
}
