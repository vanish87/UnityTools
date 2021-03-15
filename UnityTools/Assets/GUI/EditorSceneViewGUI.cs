using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.GUITool
{
    public class EditorSceneViewGUI : MonoBehaviour
    {
        #if UNITY_EDITOR
        public interface IEditorSceneViewUser
        {
            void OnSceneGUI(UnityEditor.SceneView sceneView);
        }
        protected List<IEditorSceneViewUser> Users = new List<IEditorSceneViewUser>();
        protected void OnEnable()
        {
            UnityEditor.SceneView.duringSceneGui -= this.OnSceneGUI;
            UnityEditor.SceneView.duringSceneGui += this.OnSceneGUI;
            this.Users = ObjectTool.FindAllObject<IEditorSceneViewUser>();
        }
        protected void OnDisable()
        {
            UnityEditor.SceneView.duringSceneGui -= this.OnSceneGUI;
        }

        protected void OnSceneGUI(UnityEditor.SceneView sceneView)
        {
            this.Users.ForEach(u=>u.OnSceneGUI(sceneView));
        }
        #endif

    }
}