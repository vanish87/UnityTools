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
        void SetToMaterial(object container, Material material);
        void Release();
    }

    public class Variable : IVariable
    {
        public FieldInfo Value { get; set; }

        internal object defaultValue;
        internal object lastValidValue;
        internal string displayName;

    }

    public class ListVariable : Variable
    {
        public List<Variable> variables = new List<Variable>();
    }
    public class GPUVariable : Variable, IGPUVariable
    {
        private delegate void Setter(object value, string shaderVarName, ComputeShader cs, string kernel);
        private delegate void SetterMat(object value, string shaderVarName, Material mat);
        static private Dictionary<Type, Setter> TypeSetterMap = new Dictionary<Type, Setter>()
        {
            {typeof(Enum),          (value, shaderVarName, cs, kernel) =>{ cs.SetInt(shaderVarName, (int)value);} },
            {typeof(bool),          (value, shaderVarName, cs, kernel) =>{ cs.SetBool(shaderVarName, (bool)value);} },
            {typeof(int),           (value, shaderVarName, cs, kernel) =>{ cs.SetInt(shaderVarName, (int)value);} },
            {typeof(float),         (value, shaderVarName, cs, kernel) =>{ cs.SetFloat(shaderVarName, (float)value);} },
            {typeof(Matrix4x4),     (value, shaderVarName, cs, kernel) =>{ cs.SetMatrix(shaderVarName, (Matrix4x4)value);} },
            {typeof(int[]),         (value, shaderVarName, cs, kernel) =>{ cs.SetInts(shaderVarName, (int[])value);} },
            {typeof(float[]),       (value, shaderVarName, cs, kernel) =>{ cs.SetFloats(shaderVarName, (float[])value);} },
            {typeof(Matrix4x4[]),   (value, shaderVarName, cs, kernel) =>{ cs.SetMatrixArray(shaderVarName, (Matrix4x4[])value);} },
            {typeof(int2),          (value, shaderVarName, cs, kernel) =>{ var v = (int2)value;cs.SetVector(shaderVarName, new Vector4(v.x,v.y,0,0));} },
            {typeof(int3),          (value, shaderVarName, cs, kernel) =>{ var v = (int3)value;cs.SetVector(shaderVarName, new Vector4(v.x,v.y,v.z,0));} },
            {typeof(int4),          (value, shaderVarName, cs, kernel) =>{ var v = (int4)value;cs.SetVector(shaderVarName, new Vector4(v.x,v.y,v.z,v.w));} },
            {typeof(float2),        (value, shaderVarName, cs, kernel) =>{ var v = (float2)value;cs.SetVector(shaderVarName, new Vector4(v.x,v.y,0,0));} },
            {typeof(float3),        (value, shaderVarName, cs, kernel) =>{ var v = (float3)value;cs.SetVector(shaderVarName, new Vector4(v.x,v.y,v.z,0));} },
            {typeof(float4),        (value, shaderVarName, cs, kernel) =>{ var v = (float4)value;cs.SetVector(shaderVarName, new Vector4(v.x,v.y,v.z,v.w));} },
            {typeof(Vector2),       (value, shaderVarName, cs, kernel) =>{ cs.SetVector(shaderVarName, (Vector2)value);} },
            {typeof(Vector3),       (value, shaderVarName, cs, kernel) =>{ cs.SetVector(shaderVarName, (Vector3)value);} },
            {typeof(Vector4),       (value, shaderVarName, cs, kernel) =>{ cs.SetVector(shaderVarName, (Vector4)value);} },
            {typeof(Color),         (value, shaderVarName, cs, kernel) =>{ cs.SetVector(shaderVarName, (Color)value);} },
            {typeof(Texture),       (value, shaderVarName, cs, kernel) =>{ cs.SetTexture(cs.FindKernel(kernel), shaderVarName, (Texture)value);} },
            {typeof(Texture2D),     (value, shaderVarName, cs, kernel) =>{ cs.SetTexture(cs.FindKernel(kernel), shaderVarName, (Texture2D)value);} },
            {typeof(Texture3D),     (value, shaderVarName, cs, kernel) =>{ cs.SetTexture(cs.FindKernel(kernel), shaderVarName, (Texture3D)value);} },
            {typeof(RenderTexture), (value, shaderVarName, cs, kernel) =>{ cs.SetTexture(cs.FindKernel(kernel), shaderVarName, (RenderTexture)value);} },
        };
        static private Dictionary<Type, SetterMat> TypeSetterMatMap = new Dictionary<Type, SetterMat>()
        {
            {typeof(Enum),          (value, shaderVarName, mat) =>{ mat.SetInt(shaderVarName, (int)value);} },
            {typeof(bool),          (value, shaderVarName, mat) =>{ mat.SetInt(shaderVarName, (bool)value?1:0);} },
            {typeof(int),           (value, shaderVarName, mat) =>{ mat.SetInt(shaderVarName, (int)value);} },
            {typeof(float),         (value, shaderVarName, mat) =>{ mat.SetFloat(shaderVarName, (float)value);} },
            {typeof(Matrix4x4),     (value, shaderVarName, mat) =>{ mat.SetMatrix(shaderVarName, (Matrix4x4)value);} },
            {typeof(Matrix4x4[]),   (value, shaderVarName, mat) =>{ mat.SetMatrixArray(shaderVarName, (Matrix4x4[])value);} },
            {typeof(int2),          (value, shaderVarName, mat) =>{ var v = (int2)value;  mat.SetVector(shaderVarName, new Vector4(v.x,v.y,0,0));} },
            {typeof(int3),          (value, shaderVarName, mat) =>{ var v = (int3)value;  mat.SetVector(shaderVarName, new Vector4(v.x,v.y,v.z,0));} },
            {typeof(int4),          (value, shaderVarName, mat) =>{ var v = (int4)value;  mat.SetVector(shaderVarName, new Vector4(v.x,v.y,v.z,v.w));} },
            {typeof(float2),        (value, shaderVarName, mat) =>{ var v = (float2)value;mat.SetVector(shaderVarName, new Vector4(v.x,v.y,0,0));} },
            {typeof(float3),        (value, shaderVarName, mat) =>{ var v = (float3)value;mat.SetVector(shaderVarName, new Vector4(v.x,v.y,v.z,0));} },
            {typeof(float4),        (value, shaderVarName, mat) =>{ var v = (float4)value;mat.SetVector(shaderVarName, new Vector4(v.x,v.y,v.z,v.w));} },
            {typeof(Vector2),       (value, shaderVarName, mat) =>{ mat.SetVector(shaderVarName, (Vector2)value);} },
            {typeof(Vector3),       (value, shaderVarName, mat) =>{ mat.SetVector(shaderVarName, (Vector3)value);} },
            {typeof(Vector4),       (value, shaderVarName, mat) =>{ mat.SetVector(shaderVarName, (Vector4)value);} },
            {typeof(Color),         (value, shaderVarName, mat) =>{ mat.SetColor(shaderVarName,  (Color)value);} },
            {typeof(Texture),       (value, shaderVarName, mat) =>{ mat.SetTexture(shaderVarName, (Texture)value);} },
            {typeof(Texture2D),     (value, shaderVarName, mat) =>{ mat.SetTexture(shaderVarName, (Texture2D)value);} },
            {typeof(Texture3D),     (value, shaderVarName, mat) =>{ mat.SetTexture(shaderVarName, (Texture3D)value);} },
            {typeof(RenderTexture), (value, shaderVarName, mat) =>{ mat.SetTexture(shaderVarName, (RenderTexture)value);} },
        };
        internal string shaderName;
        public virtual void SetToGPU(object container, ComputeShader cs, string kernel = null)
        {
            LogTool.AssertNotNull(container);
            LogTool.AssertNotNull(cs);
            var t = this.Value.FieldType;
            var value = this.Value.GetValue(container);
            if(TypeSetterMap.ContainsKey(t) == false)
            {
                LogTool.Log(t.ToString() + " Handler not found");
                return;
            }
            TypeSetterMap[t].Invoke(value, this.shaderName, cs, kernel);
        }
        public virtual void SetToMaterial(object container, Material material)
        {
            LogTool.AssertNotNull(container);
            LogTool.AssertNotNull(material);
            var t = this.Value.FieldType;
            var value = this.Value.GetValue(container);
            if(TypeSetterMatMap.ContainsKey(t) == false)
            {
                LogTool.Log(t.ToString() + " Handler not found");
                return;
            }
            TypeSetterMatMap[t].Invoke(value, this.shaderName, material);
        }
        public virtual void Release()
        {
        }
    }
    public class GPUBufferVariable<T> : GPUVariable
    {
        protected const int MIN_INDIRECT_BUFFER_SIZE = 5;//5 ints
        public static void SwapBuffer(GPUBufferVariable<T> lhs, GPUBufferVariable<T> rhs)
        {
            LogTool.AssertIsTrue(lhs.Size == rhs.Size);
            LogTool.AssertIsTrue(lhs.type == rhs.type);

            var temp = lhs.gpuBuffer;
            lhs.gpuBuffer = rhs.gpuBuffer;
            rhs.gpuBuffer = temp;
        }
        public static implicit operator ComputeBuffer(GPUBufferVariable<T> value)
        {
            return value.Data;
        }
        public ComputeBuffer Data 
        {
            get
            {
                LogTool.AssertIsTrue(this.Size > 0);
                return this.gpuBuffer ??= new ComputeBuffer(this.size, Marshal.SizeOf<T>(), this.type);
            }
        } 
        public T[] CPUData => this.cpuData;
        public int Size => this.size;
        public string ShaderName => this.shaderName;
        private T[] cpuData;
        private int size;
        private bool autoSet = true;
        private ComputeBufferType type = ComputeBufferType.Default;
        private ComputeBuffer gpuBuffer = null;
        public GPUBufferVariable()
        {
            //empty buffer for dynamic init 
            //also init buffer in following two constructors with declearation
        }
        public GPUBufferVariable(string name, int size, bool cpuData = false, bool autoSet = true, ComputeBufferType type = ComputeBufferType.Default)
        {
            this.displayName = name;
            this.shaderName = name;
            this.InitBuffer(size, cpuData, autoSet, type);
        }
        public GPUBufferVariable(int size, bool cpuData = false, bool autoSet = false, ComputeBufferType type = ComputeBufferType.Default)
        {
            this.InitBuffer(size, cpuData, autoSet, type);
        }
        public virtual void InitBuffer(int size, bool cpuData = false, bool autoSet = true, ComputeBufferType type = ComputeBufferType.Default)
        {
            LogTool.AssertIsTrue(size > 0);

            this.Release();

            this.size = size;
            this.type = type;
            this.autoSet = autoSet;
			this.cpuData = cpuData ? new T[this.size] : null;
        }

        public void InitBuffer(GPUBufferVariable<T> other)
        {
            LogTool.AssertIsTrue(other.Size > 0);

            this.Release();

            this.size = other.Size;
            this.type = other.type;
            this.cpuData = other.cpuData != null ? new T[this.size] : null;
            this.gpuBuffer = other.Data;
        }
        public void UpdateBuffer(GPUBufferVariable<T> other)
        {
            this.size = other.size;
            this.type = other.type;
            this.cpuData = other.cpuData != null ? new T[this.size] : null;
            this.gpuBuffer = other.Data;
        }
        public void ClearData(bool gpu = true)
        {
            this.cpuData = this.cpuData != null ? new T[this.size] : null;
            if (gpu) this.SetToGPUBuffer();
        }
       public override void Release()
        {
            base.Release();
            this.gpuBuffer?.Release();
            this.gpuBuffer = null;
            this.cpuData = null;
        }

        public override void SetToGPU(object container, ComputeShader cs, string kernel = null)
        {
            LogTool.AssertNotNull(container);
            LogTool.AssertNotNull(cs);
            LogTool.AssertNotNull(kernel);
            // LogTool.AssertIsTrue(this.Size > 0);
			// if (cs == null || this.Size == 0) { LogTool.Log(this.displayName + " is not set to GPU", LogLevel.Warning); return; }
			if (cs == null || this.Size == 0) return;

            this.SetToGPUBuffer();
            var id = cs.FindKernel(kernel);
            cs.SetInt(this.shaderName + "Count", this.Size);
            cs.SetBuffer(id, this.shaderName, this.Data);
        }
        public override void SetToMaterial(object container, Material material)
        {
            LogTool.AssertNotNull(container);
            LogTool.AssertNotNull(material);
            if (material == null) return;
            this.SetToGPUBuffer();
            material.SetBuffer(this.shaderName, this.Data);
        }

        public override string ToString()
        {
            return "ComputeBuffer " + this.shaderName + " of type " + typeof(T) + " with size " + this.size;
        }

        public void SetToGPUBuffer(bool force = false)
        {
            if(this.CPUData != null && (force || this.autoSet))
            {
                this.Data.SetData(this.CPUData);
            }
        }

        public void GetToCPUData()
        {
            if(this.CPUData != null)
            {
                this.Data.GetData(this.CPUData);
            }
        }
    }

    public class GPUBufferIndirectArgument: GPUBufferVariable<int>
    {
        public void InitBuffer(Mesh instance, int instanceCount, int subMeshIndex = 0)
        {
            base.InitBuffer(MIN_INDIRECT_BUFFER_SIZE, true, false, ComputeBufferType.IndirectArguments);

			var args = this.CPUData;
			args[0] = (int)instance.GetIndexCount(subMeshIndex);
			args[1] = instanceCount;
			args[2] = (int)instance.GetIndexStart(subMeshIndex);
			args[3] = (int)instance.GetBaseVertex(subMeshIndex);
			this.SetToGPUBuffer();
        }
    }
    public class GPUBufferAppendConsume<T>: GPUBufferVariable<T>
    {
        protected GPUBufferVariable<int> counterBuffer;
		protected GPUBufferVariable<int> CounterBuffer => this.counterBuffer ??= new GPUBufferVariable<int>();

        public void InitAppendBuffer(int size, bool autoSet = false)
        {
            LogTool.AssertIsTrue(size > 0);

            //no cpu data for append buffer
            base.InitBuffer(size, false, autoSet, ComputeBufferType.Append);
            this.ResetCounter();
        }

        public void ResetCounter(uint counter = 0)
        {
            LogTool.AssertIsTrue(this.Size > 0);
            this.Data.SetCounterValue(counter);

			if (this.counterBuffer == null) this.CounterBuffer.InitBuffer(MIN_INDIRECT_BUFFER_SIZE, true, false, ComputeBufferType.IndirectArguments);
            this.CounterBuffer.ClearData();
        }

        public int GetCounter()
        {
            ComputeBuffer.CopyCount(this.Data, this.CounterBuffer, 0);
            this.CounterBuffer.GetToCPUData();
            //only first int is valid, rest 4 ints in CPU data should be 0(undefined);
            return this.CounterBuffer.CPUData[0];
        }

        public override void InitBuffer(int size, bool cpuData = false, bool autoSet = true, ComputeBufferType type = ComputeBufferType.Default)
        {
            LogTool.Log("Use InitAppendBuffer(int size, bool autoSet = false) for clear code", LogLevel.Warning);
            this.InitAppendBuffer(size);
        }

        public override void Release()
        {
            base.Release();
            this.counterBuffer?.Release();
            this.counterBuffer = null;
        }
    }
}
