using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityTools.Common.Example;
using UnityTools.Debuging;

namespace UnityTools.Editor
{
    [CustomEditor(typeof(LogConfigure))]
    public class LogConfigureEditor : UnityEditor.Editor
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
    [CustomEditor(typeof(PCConfigure))]
    public class PCConfigureEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var configure = target as PCConfigure;
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
