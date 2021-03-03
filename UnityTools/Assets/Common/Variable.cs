using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using UnityTools.Debuging;

namespace UnityTools.Common
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

    public interface IVariable
    {
        FieldInfo Value { get; set; }
    }

    public interface IGPUVariable
    {
        void SetToGPU(object container, ComputeShader cs, string kernel = null);
        void Release();
    }

    public class Variable : IVariable
    {
        public FieldInfo Value { get; set; }

        internal object defaultValue;
        internal object lastValidValue;
        internal string displayName;

    }

    public class GPUVariable : Variable, IGPUVariable
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
            {typeof(int2),          (value, shaderVarName, cs, kernel) =>{ var v = (int2)value;cs.SetVector(shaderVarName, new Vector4(v.x,v.y,0,0));} },
            {typeof(Vector2),       (value, shaderVarName, cs, kernel) =>{ cs.SetVector(shaderVarName, (Vector2)value);} },
            {typeof(Vector3),       (value, shaderVarName, cs, kernel) =>{ cs.SetVector(shaderVarName, (Vector3)value);} },
            {typeof(Vector4),       (value, shaderVarName, cs, kernel) =>{ cs.SetVector(shaderVarName, (Vector4)value);} },
            {typeof(Texture),       (value, shaderVarName, cs, kernel) =>{ cs.SetTexture(cs.FindKernel(kernel), shaderVarName, (Texture)value);} },
            {typeof(Texture2D),     (value, shaderVarName, cs, kernel) =>{ cs.SetTexture(cs.FindKernel(kernel), shaderVarName, (Texture2D)value);} },
            {typeof(Texture3D),     (value, shaderVarName, cs, kernel) =>{ cs.SetTexture(cs.FindKernel(kernel), shaderVarName, (Texture3D)value);} },
            {typeof(RenderTexture), (value, shaderVarName, cs, kernel) =>{ cs.SetTexture(cs.FindKernel(kernel), shaderVarName, (RenderTexture)value);} },
        };
        internal string shaderName;
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
    public class GPUBufferVariable<T> : GPUVariable
    {
        public static implicit operator ComputeBuffer(GPUBufferVariable<T> value)
        {
            return value.Data;
        }
        public ComputeBuffer Data => this.gpuBuffer ??= new ComputeBuffer(this.size, Marshal.SizeOf<T>(), this.type);
        public T[] CPUData => this.cpuData;
        public int Size => this.size;
        private T[] cpuData;
        private int size;
        private ComputeBufferType type = ComputeBufferType.Default;
        private ComputeBuffer gpuBuffer;
        public GPUBufferVariable(string name, int size, bool cpuData, ComputeBufferType type)
        {
            this.displayName = name;
            this.shaderName = name;
            this.type = type;
            this.InitBuffer(size, cpuData);
        }
        public void InitBuffer(int size, bool cpuData = false)
        {
            this.size = size;
            this.cpuData = cpuData ? new T[this.size] : null;
        }

        public override void Release()
        {
            base.Release();
            this.Data.Release();
            this.gpuBuffer = null;
            this.cpuData = null;
        }

        public override void SetToGPU(object container, ComputeShader cs, string kernel = null)
        {
            LogTool.AssertNotNull(container);
            LogTool.AssertNotNull(cs);
            if (cs == null) return;
            this.UpdateBuffer();
            var id = cs.FindKernel(kernel);
            cs.SetBuffer(id, this.shaderName, this.Data);
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
}
