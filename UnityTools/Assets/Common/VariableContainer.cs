using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnityTools.Common
{
    public interface IVariableContainer
    {
        List<Variable> VariableList { get; }
    }
    public abstract class VariableContainer : IVariableContainer
    {
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

}