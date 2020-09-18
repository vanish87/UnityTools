using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Attributes
{
    public class DisableEditAttribute : PropertyAttribute
    {
    }
}
namespace UnityTools.Editor
{
    #if UNITY_EDITOR
    using UnityEditor;
    using UnityTools.Attributes;

    [CustomPropertyDrawer(typeof(DisableEditAttribute))]
    public class DisableEditAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }
    #endif
}