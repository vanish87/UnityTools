using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnityTools.Common
{
    public interface IVariableContainer
    {
        List<Variable> VariableList { get; }
        void Release();
    }
    public abstract class VariableContainer : IVariableContainer
    {
        public List<Variable> VariableList => this.variableList;
        private List<Variable> variableList = new List<Variable>();

        protected object Container => this.container != null ? this.container : this;
        private object container = null;
        public VariableContainer()
        {
            this.InitWithType(this.GetType(), this);
        }
        public VariableContainer(Type type, object container)
        {
            this.InitWithType(type, container);
        }
        public virtual void Release()
        {
            foreach(var v in this.VariableList)
            {
                if (v.Value.FieldType.IsSubclassOf(typeof(UnityEngine.Texture)))
                {
                    var tex = (UnityEngine.Texture)v.Value.GetValue(this.Container);
                    tex?.DestoryObj();
                }
                else
                if (v.Value.FieldType == typeof(UnityEngine.ComputeBuffer))
                {
                    var buffer = (UnityEngine.ComputeBuffer)v.Value.GetValue(this.Container);
                    buffer?.Release();
                }
                else
                if (v.Value.FieldType.IsSubclassOf(typeof(GPUVariable)))
                {
                    var buffer = (GPUVariable)v.Value.GetValue(this.Container);
                    buffer?.Release();
                }
            }
        }
        public void ResetToDefault()
        {
            foreach (var v in this.VariableList)
            {
                if (v.defaultValue != null)
                {
                    v.Value.SetValue(this.Container, v.defaultValue);
                    v.lastValidValue = v.defaultValue;
                }
            }
        }
        
        protected void InitWithType(Type type, object objRef = null)
        {
            this.container = objRef;
            var bindingFlags = BindingFlags.Instance |
                               BindingFlags.NonPublic |
                               BindingFlags.Public |
                               BindingFlags.Static;

            var variableList = type
                     .GetFields(bindingFlags)
                     .Where(field => !Attribute.IsDefined(field, typeof(NoneVariableAttribute)));

            foreach (var v in variableList)
            {
                var name = v.Name;
                var isGPU = false;
                var shaderName = v.Name;
                var mustNotNull = false;
                if (Attribute.IsDefined(v, typeof(GUIMenuAttribute)))
                {
                    var attrib = Attribute.GetCustomAttribute(v, typeof(GUIMenuAttribute)) as GUIMenuAttribute;
                    name = attrib.DisplayName;
                }
                if (Attribute.IsDefined(v, typeof(ShaderAttribute)))
                {
                    var attrib = Attribute.GetCustomAttribute(v, typeof(ShaderAttribute)) as ShaderAttribute;
                    shaderName = attrib.Name;
                    mustNotNull = attrib.MustNotNull;
                    isGPU = true;
                }
                if (v.FieldType.IsSubclassOf(typeof(GPUVariable)))
                {
                    var variable = (GPUVariable)v.GetValue(this.Container);
                    if(variable == null) return;
                    variable.Value = v;
                    variable.defaultValue = null;
                    variable.lastValidValue = null;
                    variable.displayName = name;
                    variable.shaderName = shaderName;
                    variable.mustNotNull = mustNotNull;
                    this.variableList.Add(variable);
                }
                else
                {
                    var value = v.GetValue(this.Container);
                    if(value is IList list)
                    {
                        //TODO
                    }
                    else
                    {
						this.variableList.Add(this.Create(v, name, value, isGPU, shaderName, mustNotNull));
                    }
                }
            }
        }
        private Variable Create(FieldInfo v, string name, object initValue, bool isGPU, string shaderName, bool mustNotNull)
        {
            if (isGPU)
            {
                return new GPUVariable()
                {
                    Value = v,
                    defaultValue = initValue,
                    lastValidValue = initValue,
                    displayName = name,
                    shaderName = shaderName,
                    mustNotNull = mustNotNull,
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