using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static UnityTools.ConfigureGUI;

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
    public abstract class VariableContainer: IVariableContainer
    {
        public interface IVariable
        {
            FieldInfo Value { get; set;}
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
            private delegate void Setter(object value, string shaderName, ComputeShader cs, string kernel);
            static private Dictionary<Type, Setter> TypeSetterMap = new Dictionary<Type, Setter>()
            {
              {typeof(bool), (value, varName, cs, kernel) =>{ cs.SetBool(varName, (bool)value);} },
              {typeof(float), SetFloat}
            };
            public string shaderName;
            public virtual void SetToGPU(object container, ComputeShader cs, string kernel = null) 
            {
                var t = this.Value.FieldType;
                var value = this.Value.GetValue(container);
                TypeSetterMap[t].Invoke(value, this.shaderName, cs, kernel);
            }
            public virtual void Release(){}

            static private int SetBool(FieldInfo field, string varName, ComputeShader computeShader, string kernel)
            {
                return 0;
            }

            static private void SetFloat(object value, string varName, ComputeShader computeShader, string kernel)
            {
                computeShader.SetFloat(varName, (float)value);
            }
        }

        public class GPUVariable<T> : GPUVariable
        {
            private T[] cpuData;
            public GPUVariable(int size, bool cpuData) : base()
            {

            }

            public override void SetToGPU(object container, ComputeShader cs, string kernel = null)
            {
            }
        }
        public List<Variable> VariableList => this.variableList;
        private List<Variable> variableList = new List<Variable>();


        public VariableContainer()
        {
            var bindingFlags = BindingFlags.Instance  |
                               BindingFlags.NonPublic |
                               BindingFlags.Public;

            var variableList = this.GetType()
                     .GetFields(bindingFlags)
                     .Where(field => !Attribute.IsDefined(field, typeof(NoneVariableAttribute)));

            foreach(var v in variableList)
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
                if(v.FieldType.IsSubclassOf(typeof(GPUVariable)))
                {
                    var variable = (GPUVariable)v.GetValue(this);
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


        private Variable Create(FieldInfo v, string name, object initValue, bool isGPU, string shaderName)
        {
            if(isGPU)
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
    public abstract class GUIContainer: VariableContainer
    {
        private Dictionary<string, string> unParsedString = new Dictionary<string, string>();
        private string classHashString;

        public GUIContainer() : base()
        {
            this.classHashString = Environment.StackTrace;
        }

        public void ResetToDefault()
        {
            foreach(var v in this.VariableList)
            {
                v.Value.SetValue(this, v.defaultValue);
            }
        }

        public virtual void OnGUI()
        {
            this.OnGUIInternal();
        }
        private void OnGUIInternal()
        {
            foreach (var v in this.VariableList)
            {
                var method = this.GetType().BaseType.GetMethod("HandleFieldValue", BindingFlags.NonPublic | BindingFlags.Instance);
                var typeFunc = method.MakeGenericMethod(v.Value.FieldType);
                typeFunc?.Invoke(this, new object[1] { v });
            }
        }
        private void HandleBool(Variable v)
        {
            var op = new[] { GUILayout.MinWidth(70f) };
            var toggle = (bool)v.Value.GetValue(this);
            using (var h = new GUILayout.HorizontalScope())
            {
                toggle = GUILayout.Toggle(toggle, v.displayName);
            }
            v.Value.SetValue(this, toggle);
        }
        
        private void HandleFieldValue<T>(Variable variable)
        {
            var t = variable.Value.FieldType;
            if (t == typeof(bool))
            {
                this.HandleBool(variable);
            }
            else
            if (t == typeof(Vector2))
            {
                using (var h = new GUILayout.HorizontalScope())
                {
                    var v = (Vector2)variable.Value.GetValue(this);
                    var lv = (Vector2)variable.lastValidValue;
                    this.OnFieldGUI(ref v.x, variable.displayName + ".x", ref lv.x);
                    this.OnFieldGUI(ref v.y, variable.displayName + ".y", ref lv.y);
                    variable.Value.SetValue(this, v);
                    variable.lastValidValue = lv;
                }
            }
            else
            if (t == typeof(Vector3))
            {
                using (var h = new GUILayout.HorizontalScope())
                {
                    var v = (Vector3)variable.Value.GetValue(this);
                    var lv = (Vector3)variable.lastValidValue;
                    this.OnFieldGUI(ref v.x, variable.displayName + ".x", ref lv.x);
                    this.OnFieldGUI(ref v.y, variable.displayName + ".y", ref lv.y);
                    this.OnFieldGUI(ref v.z, variable.displayName + ".z", ref lv.z);
                    variable.Value.SetValue(this, v);
                    variable.lastValidValue = lv;
                }
            }
            else
            if (t == typeof(Vector4))
            {
                using (var h = new GUILayout.HorizontalScope())
                {
                    var v = (Vector4)variable.Value.GetValue(this);
                    var lv = (Vector4)variable.lastValidValue;
                    this.OnFieldGUI(ref v.x, variable.displayName + ".x", ref lv.x);
                    this.OnFieldGUI(ref v.y, variable.displayName + ".y", ref lv.y);
                    this.OnFieldGUI(ref v.z, variable.displayName + ".z", ref lv.z);
                    this.OnFieldGUI(ref v.w, variable.displayName + ".w", ref lv.w);
                    variable.Value.SetValue(this, v);
                    variable.lastValidValue = lv;
                }
            }
            else
            {
                var v = (T)variable.Value.GetValue(this);
                var lv = (T)variable.lastValidValue;
                this.OnFieldGUI<T>(ref v, variable.displayName, ref lv);
                variable.Value.SetValue(this, v);
                variable.lastValidValue = lv;
            }
        }

        private void OnFieldGUI<T>(ref T v, string displayName, ref T lastValidValue)
        {
            var op = new[] { GUILayout.MinWidth(70f) };
            using (var h = new GUILayout.HorizontalScope())
            {
                GUILayout.Label(displayName);
                var hash = displayName;
                var target = v.ToString();
                var hasUnparsedStr = this.unParsedString.ContainsKey(hash);
                if (hasUnparsedStr)
                {
                    target = this.unParsedString[hash];
                }
                var color = hasUnparsedStr ? Color.red : GUI.color;

                using (var cs = new ColorScope(color))
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
                        if (hasUnparsedStr) this.unParsedString.Remove(hash);
                    }
                    else
                    {
                        this.unParsedString[hash] = ret;
                    }
                    if (hasUnparsedStr && GUILayout.Button("Reset"))
                    {
                        v = lastValidValue;
                        this.unParsedString.Remove(hash);
                    }
                }
            }
        }

    }

}