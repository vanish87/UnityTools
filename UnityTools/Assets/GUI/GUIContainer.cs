using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityTools.Common;

namespace UnityTools.GUITool
{
    public class GUIContainer : VariableContainer
    {
        private delegate void GUIDraw(object containter, Variable variable, Dictionary<string, string> unparsedString);
        static private Dictionary<Type, GUIDraw> TypeDrawerMap = new Dictionary<Type, GUIDraw>()
        {
            {typeof(bool),    HandleBool},
            {typeof(Vector2), HandleVector2},
            {typeof(Vector3), HandleVector3},
            {typeof(Vector4), HandleVector4},
            {typeof(Color),   HandleColor},
            {typeof(ComputeBuffer), HandleGPUResource},
            {typeof(Texture),       HandleGPUResource},
            {typeof(Texture2D),     HandleGPUResource},
            {typeof(Texture3D),     HandleGPUResource},
            {typeof(RenderTexture), HandleGPUResource},
            {typeof(GPUVariable),   HandleGPUVariable},
        };
        private Dictionary<string, string> unParsedString = new Dictionary<string, string>();
        private string classHashString;

        public GUIContainer() : base()
        {
            this.classHashString = System.Environment.StackTrace;
        }
        public GUIContainer(Type type, object container): base(type, container)
        {
            this.classHashString = System.Environment.StackTrace;
        }
        public virtual void OnGUI()
        {
            this.OnGUIInternal();
        }
        private void OnGUIInternal()
        {
            foreach (var v in this.VariableList)
            {
                var t = v.Value.FieldType;
                var key = TypeDrawerMap.ContainsKey(t) ? t : TypeDrawerMap.Keys.Where(k => t.IsSubclassOf(k)).FirstOrDefault();
                if (key != null)
                {
                    TypeDrawerMap[key].Invoke(this.Container, v, this.unParsedString);
                }
                else
                {
                    var method = typeof(GUIContainer).GetMethod("HandleFieldValue", BindingFlags.NonPublic | BindingFlags.Static);
                    var typeFunc = method.MakeGenericMethod(v.Value.FieldType);
                    typeFunc?.Invoke(null, new object[3] { this.Container, v, this.unParsedString });
                }
            }
        }
        static private void HandleBool(object container, Variable v, Dictionary<string, string> unparsedString)
        {
            var op = new[] { GUILayout.MinWidth(70f) };
            var toggle = (bool)v.Value.GetValue(container);
            using (var h = new GUILayout.HorizontalScope())
            {
                toggle = GUILayout.Toggle(toggle, v.displayName);
            }
            v.Value.SetValue(container, toggle);
        }

        static private void HandleVector2(object container, Variable variable, Dictionary<string, string> unparsedString)
        {
            using (var h = new GUILayout.HorizontalScope())
            {
                var v = (Vector2)variable.Value.GetValue(container);
                var lv = (Vector2)variable.lastValidValue;
                OnFieldGUI(ref v.x, variable.displayName + ".x", ref lv.x, unparsedString);
                OnFieldGUI(ref v.y, variable.displayName + ".y", ref lv.y, unparsedString);
                variable.Value.SetValue(container, v);
                variable.lastValidValue = lv;
            }
        }

        static private void HandleVector3(object container, Variable variable, Dictionary<string, string> unparsedString)
        {
            using (var h = new GUILayout.HorizontalScope())
            {
                var v = (Vector3)variable.Value.GetValue(container);
                var lv = (Vector3)variable.lastValidValue;
                OnFieldGUI(ref v.x, variable.displayName + ".x", ref lv.x, unparsedString);
                OnFieldGUI(ref v.y, variable.displayName + ".y", ref lv.y, unparsedString);
                OnFieldGUI(ref v.z, variable.displayName + ".z", ref lv.z, unparsedString);
                variable.Value.SetValue(container, v);
                variable.lastValidValue = lv;
            }
        }
        static private void HandleVector4(object container, Variable variable, Dictionary<string, string> unparsedString)
        {
            using (var h = new GUILayout.HorizontalScope())
            {
                var v = (Vector4)variable.Value.GetValue(container);
                var lv = (Vector4)variable.lastValidValue;
                OnFieldGUI(ref v.x, variable.displayName + ".x", ref lv.x, unparsedString);
                OnFieldGUI(ref v.y, variable.displayName + ".y", ref lv.y, unparsedString);
                OnFieldGUI(ref v.z, variable.displayName + ".z", ref lv.z, unparsedString);
                OnFieldGUI(ref v.w, variable.displayName + ".w", ref lv.w, unparsedString);
                variable.Value.SetValue(container, v);
                variable.lastValidValue = lv;
            }
        }

        static private void HandleColor(object container, Variable variable, Dictionary<string, string> unparsedString)
        {
            using (var h = new GUILayout.HorizontalScope())
            {
                var v = (Color)variable.Value.GetValue(container);
                var lv = (Color)variable.lastValidValue;
                OnFieldGUI(ref v.r, variable.displayName + ".r", ref lv.r, unparsedString);
                OnFieldGUI(ref v.g, variable.displayName + ".g", ref lv.g, unparsedString);
                OnFieldGUI(ref v.b, variable.displayName + ".b", ref lv.b, unparsedString);
                OnFieldGUI(ref v.a, variable.displayName + ".a", ref lv.a, unparsedString);
                variable.Value.SetValue(container, v);
                variable.lastValidValue = lv;
            }
        }
        static private void HandleGPUResource(object container, Variable variable, Dictionary<string, string> unparsedString)
        {
            if (variable.Value.FieldType == typeof(ComputeBuffer)) return;

            var v = (Texture)variable.Value.GetValue(container);
            if (v == null) return;

            GUILayout.Label(variable.displayName);
            GUILayout.Box(v);
        }
        static private void HandleGPUVariable(object container, Variable variable, Dictionary<string, string> unparsedString)
        {
            var v = (GPUVariable)variable.Value.GetValue(container);
            if (v == null) return;

            GUILayout.Label(v.ToString());
        }
        static private void HandleFieldValue<T>(object container, Variable variable, Dictionary<string, string> unparsedString)
        {
            var v = (T)variable.Value.GetValue(container);
            var lv = (T)variable.lastValidValue;
            if (v == null) return;

            OnFieldGUI<T>(ref v, variable.displayName, ref lv, unparsedString);
            variable.Value.SetValue(container, v);
            variable.lastValidValue = lv;
        }

        static private void OnFieldGUI<T>(ref T v, string displayName, ref T lastValidValue, Dictionary<string, string> unParsedString)
        {
            var op = new[] { GUILayout.MinWidth(70f) };
            using (var h = new GUILayout.HorizontalScope())
            {
                GUILayout.Label(displayName);
                var hash = displayName;
                var target = v.ToString();
                var hasUnparsedStr = unParsedString.ContainsKey(hash);
                if (hasUnparsedStr)
                {
                    target = unParsedString[hash];
                }
                var color = hasUnparsedStr ? Color.red : GUI.color;

                using (new GUITool.ColorScope(color))
                {
                    var ret = GUILayout.TextField(target, op);

                    var newValue = default(T);
                    var canParse = false;
                    try
                    {
                        newValue = (T)Convert.ChangeType(ret, typeof(T));
                        canParse = newValue.ToString() == ret;
                    }
                    catch (Exception) { }

                    if (canParse)
                    {
                        v = newValue;
                        lastValidValue = newValue;
                        if (hasUnparsedStr) unParsedString.Remove(hash);
                    }
                    else
                    {
                        unParsedString[hash] = ret;
                    }
                    if (hasUnparsedStr && GUILayout.Button("Reset"))
                    {
                        v = lastValidValue;
                        unParsedString.Remove(hash);
                    }
                }
            }
        }

    }

}