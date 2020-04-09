using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityTools.Debuging;

namespace UnityTools.Editor
{
    [CustomEditor(typeof(LogConfigure))]
    public class EmitterConfigureEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var configure = target as LogConfigure;
            if (configure == null) return;

            if (GUILayout.Button("Save"))
            {
                configure.Save();
            }
            if (GUILayout.Button("Load"))
            {
                configure.LoadAndNotify();
            }
            base.OnInspectorGUI();
        }
    }
}
