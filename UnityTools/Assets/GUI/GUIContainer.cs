using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityTools.Debuging;

namespace UnityTools.GUITool
{
    [AttributeUsage(AttributeTargets.Field)]
    public class NoneVariableAttribute : Attribute
    {

    }
    [AttributeUsage(AttributeTargets.Field)]
    public class GUIMenuAttribute : Attribute
    {
        public string DisplayName;
    }
    [AttributeUsage(AttributeTargets.Field)]
    public class ShaderAttribute : Attribute
    {
        public string Name;
    }
    public interface IVariableContainer
    {
        List<VariableContainer.Variable> VariableList { get; }
    }
    public class GPUBufferVariable<T> : VariableContainer.GPUVariable
    {
        public static implicit operator ComputeBuffer(GPUBufferVariable<T> value)
        {
            return value.Data;
        }
        public ComputeBuffer Data => this.gpuBuffer;
        public T[] CPUData => this.cpuData;
        public int Size => this.size;
        private T[] cpuData;
        private int size;
        private ComputeBuffer gpuBuffer;
        public GPUBufferVariable(string name, int size, bool cpuData)
        {
            this.displayName = name;
            this.shaderName = name;
            this.InitBuffer(size, cpuData);
        }
        public void InitBuffer(int size, bool cpuData = false)
        {
            this.size = size;
            this.cpuData = cpuData ? new T[this.size] : null;
            this.gpuBuffer = new ComputeBuffer(this.size, Marshal.SizeOf<T>());
        }

        public override void Release()
        {
            base.Release();
            this.gpuBuffer.Release();
            this.cpuData = null;
        }

        public override void SetToGPU(object container, ComputeShader cs, string kernel = null)
        {
            if (cs == null) return;
            var id = cs.FindKernel(kernel);
            cs.SetBuffer(id, this.shaderName, this.gpuBuffer);
        }

        public override string ToString()
        {
            return "ComputeBuffer " + this.shaderName + " of type " + typeof(T) + " with size " + this.size;
        }

        public void UpdateBuffer()
        {
            this.Data.SetData(this.CPUData);
        }
    }
    public abstract class VariableContainer : IVariableContainer
    {
        public interface IVariable
        {
            FieldInfo Value { get; set; }
        }

        public class Variable : IVariable
        {
            public FieldInfo Value { get; set; }

            public object defaultValue;
            public object lastValidValue;
            public string displayName;

        }

        public class GPUVariable : Variable
        {
            private delegate void Setter(object value, string shaderVarName, ComputeShader cs, string kernel);
            static private Dictionary<Type, Setter> TypeSetterMap = new Dictionary<Type, Setter>()
            {
                {typeof(bool),          (value, shaderVarName, cs, kernel) =>{ cs.SetBool(shaderVarName, (bool)value);} },
                {typeof(int),           (value, shaderVarName, cs, kernel) =>{ cs.SetInt(shaderVarName, (int)value);} },
                {typeof(float),         (value, shaderVarName, cs, kernel) =>{ cs.SetFloat(shaderVarName, (float)value);} },
                {typeof(Matrix4x4),     (value, shaderVarName, cs, kernel) =>{ cs.SetMatrix(shaderVarName, (Matrix4x4)value);} },
                {typeof(int[]),         (value, shaderVarName, cs, kernel) =>{ cs.SetInts(shaderVarName, (int[])value);} },
                {typeof(float[]),       (value, shaderVarName, cs, kernel) =>{ cs.SetFloats(shaderVarName, (float[])value);} },
                {typeof(Matrix4x4[]),   (value, shaderVarName, cs, kernel) =>{ cs.SetMatrixArray(shaderVarName, (Matrix4x4[])value);} },
                {typeof(Vector2),       (value, shaderVarName, cs, kernel) =>{ cs.SetVector(shaderVarName, (Vector2)value);} },
                {typeof(Vector3),       (value, shaderVarName, cs, kernel) =>{ cs.SetVector(shaderVarName, (Vector3)value);} },
                {typeof(Vector4),       (value, shaderVarName, cs, kernel) =>{ cs.SetVector(shaderVarName, (Vector4)value);} },
                {typeof(Texture),       (value, shaderVarName, cs, kernel) =>{ cs.SetTexture(cs.FindKernel(kernel), shaderVarName, (Texture)value);} },
                {typeof(Texture2D),     (value, shaderVarName, cs, kernel) =>{ cs.SetTexture(cs.FindKernel(kernel), shaderVarName, (Texture2D)value);} },
                {typeof(Texture3D),     (value, shaderVarName, cs, kernel) =>{ cs.SetTexture(cs.FindKernel(kernel), shaderVarName, (Texture3D)value);} },
                {typeof(RenderTexture), (value, shaderVarName, cs, kernel) =>{ cs.SetTexture(cs.FindKernel(kernel), shaderVarName, (RenderTexture)value);} },
            };
            public string shaderName;
            public virtual void SetToGPU(object container, ComputeShader cs, string kernel = null)
            {
                LogTool.AssertNotNull(container);
                LogTool.AssertNotNull(cs);
                var t = this.Value.FieldType;
                var value = this.Value.GetValue(container);
                TypeSetterMap[t].Invoke(value, this.shaderName, cs, kernel);
            }
            public virtual void Release()
            {
            }
        }

        public List<Variable> VariableList => this.variableList;
        private List<Variable> variableList = new List<Variable>();


        public VariableContainer()
        {
            var bindingFlags = BindingFlags.Instance |
                               BindingFlags.NonPublic |
                               BindingFlags.Public;

            var variableList = this.GetType()
                     .GetFields(bindingFlags)
                     .Where(field => !Attribute.IsDefined(field, typeof(NoneVariableAttribute)));

            foreach (var v in variableList)
            {
                var name = v.Name;
                var isGPU = false;
                var shaderName = v.Name;
                if (Attribute.IsDefined(v, typeof(GUIMenuAttribute)))
                {
                    var attrib = Attribute.GetCustomAttribute(v, typeof(GUIMenuAttribute)) as GUIMenuAttribute;
                    name = attrib.DisplayName;
                }
                if (Attribute.IsDefined(v, typeof(ShaderAttribute)))
                {
                    var attrib = Attribute.GetCustomAttribute(v, typeof(ShaderAttribute)) as ShaderAttribute;
                    shaderName = attrib.Name;
                    isGPU = true;
                }
                if (v.FieldType.IsSubclassOf(typeof(GPUVariable)))
                {
                    var variable = (GPUVariable)v.GetValue(this);
                    variable.Value = v;
                    variable.defaultValue = null;
                    variable.lastValidValue = null;
                    variable.displayName = name;
                    variable.shaderName = shaderName;
                    this.variableList.Add(variable);
                }
                else
                {
                    this.variableList.Add(this.Create(v, name, v.GetValue(this), isGPU, shaderName));
                }
            }
        }
        public void ResetToDefault()
        {
            foreach (var v in this.VariableList)
            {
                if (v.defaultValue != null)
                {
                    v.Value.SetValue(this, v.defaultValue);
                    v.lastValidValue = v.defaultValue;
                }
            }
        }

        private Variable Create(FieldInfo v, string name, object initValue, bool isGPU, string shaderName)
        {
            if (isGPU)
            {
                return new GPUVariable()
                {
                    Value = v,
                    defaultValue = initValue,
                    lastValidValue = initValue,
                    displayName = name,
                    shaderName = shaderName
                };
            }
            else
            {
                return new Variable()
                {
                    Value = v,
                    defaultValue = initValue,
                    lastValidValue = initValue,
                    displayName = name,
                };
            }
        }

    }
    public abstract class GUIContainer : VariableContainer
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
            this.classHashString = Environment.StackTrace;
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
                    TypeDrawerMap[key].Invoke(this, v, this.unParsedString);
                }
                else
                {
                    var method = this.GetType().BaseType.GetMethod("HandleFieldValue", BindingFlags.NonPublic | BindingFlags.Static);
                    var typeFunc = method.MakeGenericMethod(v.Value.FieldType);
                    typeFunc?.Invoke(this, new object[3] { this, v, this.unParsedString });
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

                using (var cs = new ConfigureGUI.ColorScope(color))
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