using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Attributes
{
    public class FileNamePopupAttribute : PropertyAttribute
    {
        public string regex;

        public FileNamePopupAttribute(string searchPattern)
        {
            this.regex = searchPattern;
        }
    }
}
 namespace UnityTools.Editor
 {
    #if UNITY_EDITOR
    using System.Linq;
    using System.IO;
    using UnityEditor;
    using UnityTools.Attributes;

    [CustomPropertyDrawer(typeof(FileNamePopupAttribute))]
    public class FilePopupDrawer : PropertyDrawer
    {
        string[] displayNames = null;
        int selectedIndex = -1;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                Debug.LogError("type: " + property.propertyType + " is not supported.");
                EditorGUI.LabelField(position, label.text, "Use FilePopup with string.");
                return;
            }

            if (displayNames == null)
            {
                string regex = (attribute as FileNamePopupAttribute).regex;
                InitDisplayNames(regex);
            }

            if (selectedIndex < 0)
            {
                selectedIndex = FindSelectedIndex(property.stringValue);
            }

            EditorGUI.BeginProperty(position, label, property);

            selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, displayNames);
            property.stringValue = displayNames[selectedIndex];

            EditorGUI.EndProperty();
        }

        void InitDisplayNames(string regex)
        {
            string[] fullpathes = Directory.GetFiles(Application.streamingAssetsPath, regex, SearchOption.AllDirectories);

            displayNames = fullpathes.Select(f =>
            {
                string path = f.Replace(Application.streamingAssetsPath, "")
                               .Replace('\\', '/');
                if (path.StartsWith("/"))
                {
                    path = path.Substring(1);
                }
                return path;
            }).ToArray();
        }

        int FindSelectedIndex(string value)
        {
            for (int i = 0; i < displayNames.Length; i++)
            {
                if (displayNames[i] == value)
                {
                    return i;
                }
            }
            return 0;
        }
    }
    #endif // UNITY_EDITOR
 }