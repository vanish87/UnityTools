// KlakNDI - NDI plugin for Unity
// https://github.com/keijiro/KlakNDI

using UnityEngine;
using UnityEditor;

namespace Klak.Ndi
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(NdiSender))]
    public sealed class NdiSenderEditor : Editor
    {
        SerializedProperty _sourceTexture;
        SerializedProperty _invertY;

        void OnEnable()
        {
            _sourceTexture = serializedObject.FindProperty("_sourceTexture");
            _invertY = serializedObject.FindProperty("_invertY");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var sender = (NdiSender)target;

            if (targets.Length == 1)
            {
                var camera = sender.GetComponent<Camera>();

                if (camera != null)
                {
                    EditorGUILayout.HelpBox(
                        "NDI Sender is running in camera capture mode.",
                        MessageType.None
                    );

                    // Hide the source texture property.
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "NDI Sender is running in render texture mode.",
                        MessageType.None
                    );

                    EditorGUILayout.PropertyField(_sourceTexture);
                }
            }
            else
                EditorGUILayout.PropertyField(_sourceTexture);
            
            EditorGUILayout.PropertyField(this._invertY);
            sender.DataFormat = (FourCC)EditorGUILayout.EnumPopup("Data Format", sender.DataFormat);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
