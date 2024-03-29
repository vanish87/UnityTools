using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Mathematics;
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
            {typeof(int2),    HandleInt2},
            {typeof(int3),    HandleInt3},
            {typeof(int4),    HandleInt4},
            {typeof(float2),  HandleFloat2},
            {typeof(float3),  HandleFloat3},
            {typeof(float4),  HandleFloat4},
            {typeof(Color),   HandleColor},
            {typeof(ComputeBuffer), HandleGPUResource},
            {typeof(Texture),       HandleGPUResource},
            {typeof(Texture2D),     HandleGPUResource},
            {typeof(Texture3D),     HandleGPUResource},
            {typeof(RenderTexture), HandleGPUResource},
            {typeof(GPUVariable),   HandleGPUVariable},
            {typeof(ListVariable),  HandleListVariable},
        };
        static private HashSet<Type> DrawableType = new HashSet<Type>()
        {
			typeof(bool), typeof(float), typeof(int), typeof(string),
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
            foreach (var v in this.VariableList)
            {
				OnGUIInternal(v, this.Container, this.unParsedString);
            }
        }
        static private void OnGUIInternal(Variable v, object container, Dictionary<string, string> unParsedString)
        {
            {
                var t = v.Value.FieldType;
                var key = TypeDrawerMap.ContainsKey(t) ? t : TypeDrawerMap.Keys.Where(k => t.IsSubclassOf(k)).FirstOrDefault();
                if (key != null)
                {
                    TypeDrawerMap[key].Invoke(container, v, unParsedString);
                }
                else if(DrawableType.Contains(t))
                {
                    var method = typeof(GUIContainer).GetMethod("HandleFieldValue", BindingFlags.NonPublic | BindingFlags.Static);
                    var typeFunc = method.MakeGenericMethod(v.Value.FieldType);
                    typeFunc?.Invoke(null, new object[3] { container, v, unParsedString });
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
                var dv = (Vector2)variable.defaultValue;
                OnFieldGUI(ref v.x, variable.displayName + ".x", ref lv.x, dv.x, unparsedString);
                OnFieldGUI(ref v.y, variable.displayName + ".y", ref lv.y, dv.y, unparsedString);
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
                var dv = (Vector3)variable.defaultValue;
                OnFieldGUI(ref v.x, variable.displayName + ".x", ref lv.x, dv.x, unparsedString);
                OnFieldGUI(ref v.y, variable.displayName + ".y", ref lv.y, dv.y, unparsedString);
                OnFieldGUI(ref v.z, variable.displayName + ".z", ref lv.z, dv.z, unparsedString);
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
                var dv = (Vector4)variable.defaultValue;
                OnFieldGUI(ref v.x, variable.displayName + ".x", ref lv.x, dv.x, unparsedString);
                OnFieldGUI(ref v.y, variable.displayName + ".y", ref lv.y, dv.y, unparsedString);
                OnFieldGUI(ref v.z, variable.displayName + ".z", ref lv.z, dv.z, unparsedString);
                OnFieldGUI(ref v.w, variable.displayName + ".w", ref lv.w, dv.w, unparsedString);
                variable.Value.SetValue(container, v);
                variable.lastValidValue = lv;
            }
        }
        static private void HandleInt2(object container, Variable variable, Dictionary<string, string> unparsedString)
        {
            using (var h = new GUILayout.HorizontalScope())
            {
                var v = (int2)variable.Value.GetValue(container);
                var lv = (int2)variable.lastValidValue;
                var dv = (int2)variable.defaultValue;
                OnFieldGUI(ref v.x, variable.displayName + ".x", ref lv.x, dv.x, unparsedString);
                OnFieldGUI(ref v.y, variable.displayName + ".y", ref lv.y, dv.y, unparsedString);
                variable.Value.SetValue(container, v);
                variable.lastValidValue = lv;
            }
        }
        static private void HandleInt3(object container, Variable variable, Dictionary<string, string> unparsedString)
        {
            using (var h = new GUILayout.HorizontalScope())
            {
                var v = (int3)variable.Value.GetValue(container);
                var lv = (int3)variable.lastValidValue;
                var dv = (int3)variable.defaultValue;
                OnFieldGUI(ref v.x, variable.displayName + ".x", ref lv.x, dv.x, unparsedString);
                OnFieldGUI(ref v.y, variable.displayName + ".y", ref lv.y, dv.y, unparsedString);
                OnFieldGUI(ref v.z, variable.displayName + ".z", ref lv.z, dv.z, unparsedString);
                variable.Value.SetValue(container, v);
                variable.lastValidValue = lv;
            }
        }
        static private void HandleInt4(object container, Variable variable, Dictionary<string, string> unparsedString)
        {
            using (var h = new GUILayout.HorizontalScope())
            {
                var v = (int4)variable.Value.GetValue(container);
                var lv = (int4)variable.lastValidValue;
                var dv = (int4)variable.defaultValue;
                OnFieldGUI(ref v.x, variable.displayName + ".x", ref lv.x, dv.x, unparsedString);
                OnFieldGUI(ref v.y, variable.displayName + ".y", ref lv.y, dv.y, unparsedString);
                OnFieldGUI(ref v.z, variable.displayName + ".z", ref lv.z, dv.z, unparsedString);
                OnFieldGUI(ref v.w, variable.displayName + ".w", ref lv.w, dv.w, unparsedString);
                variable.Value.SetValue(container, v);
                variable.lastValidValue = lv;
            }
        }
        static private void HandleFloat2(object container, Variable variable, Dictionary<string, string> unparsedString)
        {
            using (var h = new GUILayout.HorizontalScope())
            {
                var v = (float2)variable.Value.GetValue(container);
                var lv = (float2)variable.lastValidValue;
                var dv = (float2)variable.defaultValue;
                OnFieldGUI(ref v.x, variable.displayName + ".x", ref lv.x, dv.x, unparsedString);
                OnFieldGUI(ref v.y, variable.displayName + ".y", ref lv.y, dv.y, unparsedString);
                variable.Value.SetValue(container, v);
                variable.lastValidValue = lv;
            }
        }
        static private void HandleFloat3(object container, Variable variable, Dictionary<string, string> unparsedString)
        {
            using (var h = new GUILayout.HorizontalScope())
            {
                var v = (float3)variable.Value.GetValue(container);
                var lv = (float3)variable.lastValidValue;
                var dv = (float3)variable.defaultValue;
                OnFieldGUI(ref v.x, variable.displayName + ".x", ref lv.x, dv.x, unparsedString);
                OnFieldGUI(ref v.y, variable.displayName + ".y", ref lv.y, dv.y, unparsedString);
                OnFieldGUI(ref v.z, variable.displayName + ".z", ref lv.z, dv.z, unparsedString);
                variable.Value.SetValue(container, v);
                variable.lastValidValue = lv;
            }
        }
        static private void HandleFloat4(object container, Variable variable, Dictionary<string, string> unparsedString)
        {
            using (var h = new GUILayout.HorizontalScope())
            {
                var v = (float4)variable.Value.GetValue(container);
                var lv = (float4)variable.lastValidValue;
                var dv = (float4)variable.defaultValue;
                OnFieldGUI(ref v.x, variable.displayName + ".x", ref lv.x, dv.x, unparsedString);
                OnFieldGUI(ref v.y, variable.displayName + ".y", ref lv.y, dv.y, unparsedString);
                OnFieldGUI(ref v.z, variable.displayName + ".z", ref lv.z, dv.z, unparsedString);
                OnFieldGUI(ref v.w, variable.displayName + ".w", ref lv.w, dv.w, unparsedString);
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
                var dv = (Color)variable.defaultValue;
                OnFieldGUI(ref v.r, variable.displayName + ".r", ref lv.r, dv.r, unparsedString);
                OnFieldGUI(ref v.g, variable.displayName + ".g", ref lv.g, dv.g, unparsedString);
                OnFieldGUI(ref v.b, variable.displayName + ".b", ref lv.b, dv.b, unparsedString);
                OnFieldGUI(ref v.a, variable.displayName + ".a", ref lv.a, dv.a, unparsedString);
                variable.Value.SetValue(container, v);
                variable.lastValidValue = lv;
            }
        }
        static private void HandleGPUResource(object container, Variable variable, Dictionary<string, string> unparsedString)
        {
            if (variable.Value.FieldType == typeof(ComputeBuffer)) return;

            var v = (Texture)variable.Value.GetValue(container);
            if (v == null) return;

			var aspect = v.width * 1.0f / v.height;
            var w = 256f;
            var h = w / aspect;
            GUILayout.Label(variable.displayName);
            GUILayout.Box(v, GUILayout.Width(w), GUILayout.Height(h));
        }
        static private void HandleGPUVariable(object container, Variable variable, Dictionary<string, string> unparsedString)
        {
            var v = (GPUVariable)variable.Value.GetValue(container);
            if (v == null) return;

            GUILayout.Label(v.ToString());
        }
        static private void HandleListVariable(object container, Variable variable, Dictionary<string, string> unparsedString)
        {
            var vl = (ListVariable)variable.Value.GetValue(container);
            if (vl == null) return;

            //TODO
        }
        static private void HandleFieldValue<T>(object container, Variable variable, Dictionary<string, string> unparsedString)
        {
            var v = (T)variable.Value.GetValue(container);
            var lv = (T)variable.lastValidValue;
            var dv = (T)variable.defaultValue;
            if (v == null) return;

            OnFieldGUI<T>(ref v, variable.displayName, ref lv, dv, unparsedString);
            variable.Value.SetValue(container, v);
            variable.lastValidValue = lv;
        }

        static private void OnFieldGUI<T>(ref T v, string displayName, ref T lastValidValue, T defaultValue, Dictionary<string, string> unParsedString)
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
                    if(GUILayout.Button("Default"))
                    {
                        v = defaultValue;
                    }
                }
            }
        }

    }

}