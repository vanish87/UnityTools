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
        public bool MustNotNull = false;
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
        private delegate void Setter(object value, string shaderVarName, ComputeShader cs, string kernel, bool mustNotNull = false);
        private delegate void SetterMat(object value, string shaderVarName, Material mat, bool mustNotNull = false);
        static private Dictionary<Type, Setter> TypeSetterMap = new Dictionary<Type, Setter>()
        {
            {typeof(Enum),          (value, shaderVarName, cs, kernel, notNull) =>{ cs.SetInt(shaderVarName, (int)value);} },
            {typeof(bool),          (value, shaderVarName, cs, kernel, notNull) =>{ cs.SetBool(shaderVarName, (bool)value);} },
            {typeof(int),           (value, shaderVarName, cs, kernel, notNull) =>{ cs.SetInt(shaderVarName, (int)value);} },
            {typeof(float),         (value, shaderVarName, cs, kernel, notNull) =>{ cs.SetFloat(shaderVarName, (float)value);} },
            {typeof(Matrix4x4),     (value, shaderVarName, cs, kernel, notNull) =>{ cs.SetMatrix(shaderVarName, (Matrix4x4)value);} },
            {typeof(int[]),         (value, shaderVarName, cs, kernel, notNull) =>{ cs.SetInts(shaderVarName, (int[])value);} },
            {typeof(float[]),       (value, shaderVarName, cs, kernel, notNull) =>{ cs.SetFloats(shaderVarName, (float[])value);} },
            {typeof(Matrix4x4[]),   (value, shaderVarName, cs, kernel, notNull) =>{ cs.SetMatrixArray(shaderVarName, (Matrix4x4[])value);} },
            {typeof(bool2),         (value, shaderVarName, cs, kernel, notNull) =>{ var v = (bool2)value;cs.SetVector(shaderVarName, new Vector4(v.x?1:0,v.y?1:0,0,0));} },
            {typeof(bool3),         (value, shaderVarName, cs, kernel, notNull) =>{ var v = (bool3)value;cs.SetVector(shaderVarName, new Vector4(v.x?1:0,v.y?1:0,v.z?1:0,0));} },
            {typeof(bool4),         (value, shaderVarName, cs, kernel, notNull) =>{ var v = (bool4)value;cs.SetVector(shaderVarName, new Vector4(v.x?1:0,v.y?1:0,v.z?1:0,v.w?1:0));} },
            {typeof(int2),          (value, shaderVarName, cs, kernel, notNull) =>{ var v = (int2)value;cs.SetVector(shaderVarName, new Vector4(v.x,v.y,0,0));} },
            {typeof(int3),          (value, shaderVarName, cs, kernel, notNull) =>{ var v = (int3)value;cs.SetVector(shaderVarName, new Vector4(v.x,v.y,v.z,0));} },
            {typeof(int4),          (value, shaderVarName, cs, kernel, notNull) =>{ var v = (int4)value;cs.SetVector(shaderVarName, new Vector4(v.x,v.y,v.z,v.w));} },
            {typeof(float2),        (value, shaderVarName, cs, kernel, notNull) =>{ var v = (float2)value;cs.SetVector(shaderVarName, new Vector4(v.x,v.y,0,0));} },
            {typeof(float3),        (value, shaderVarName, cs, kernel, notNull) =>{ var v = (float3)value;cs.SetVector(shaderVarName, new Vector4(v.x,v.y,v.z,0));} },
            {typeof(float4),        (value, shaderVarName, cs, kernel, notNull) =>{ var v = (float4)value;cs.SetVector(shaderVarName, new Vector4(v.x,v.y,v.z,v.w));} },
            {typeof(Vector2),       (value, shaderVarName, cs, kernel, notNull) =>{ cs.SetVector(shaderVarName, (Vector2)value);} },
            {typeof(Vector3),       (value, shaderVarName, cs, kernel, notNull) =>{ cs.SetVector(shaderVarName, (Vector3)value);} },
            {typeof(Vector4),       (value, shaderVarName, cs, kernel, notNull) =>{ cs.SetVector(shaderVarName, (Vector4)value);} },
            {typeof(Color),         (value, shaderVarName, cs, kernel, notNull) =>{ cs.SetVector(shaderVarName, (Color)value);} },
			{typeof(Texture),       (value, shaderVarName, cs, kernel, notNull) =>{ SetTexture( shaderVarName, (Texture)value,cs, kernel, notNull);} },
			{typeof(Texture2D),     (value, shaderVarName, cs, kernel, notNull) =>{ SetTexture( shaderVarName, (Texture2D)value,cs, kernel, notNull);} },
			{typeof(Texture3D),     (value, shaderVarName, cs, kernel, notNull) =>{ SetTexture( shaderVarName, (Texture3D)value,cs, kernel, notNull);} },
			{typeof(RenderTexture), (value, shaderVarName, cs, kernel, notNull) =>{ SetTexture( shaderVarName, (RenderTexture)value,cs, kernel, notNull);} },
        };
        static private Dictionary<Type, SetterMat> TypeSetterMatMap = new Dictionary<Type, SetterMat>()
        {
            {typeof(Enum),          (value, shaderVarName, mat, notNull) =>{ mat.SetInt(shaderVarName, (int)value);} },
            {typeof(bool),          (value, shaderVarName, mat, notNull) =>{ mat.SetInt(shaderVarName, (bool)value?1:0);} },
            {typeof(int),           (value, shaderVarName, mat, notNull) =>{ mat.SetInt(shaderVarName, (int)value);} },
            {typeof(float),         (value, shaderVarName, mat, notNull) =>{ mat.SetFloat(shaderVarName, (float)value);} },
            {typeof(Matrix4x4),     (value, shaderVarName, mat, notNull) =>{ mat.SetMatrix(shaderVarName, (Matrix4x4)value);} },
            {typeof(Matrix4x4[]),   (value, shaderVarName, mat, notNull) =>{ mat.SetMatrixArray(shaderVarName, (Matrix4x4[])value);} },
            {typeof(bool2),         (value, shaderVarName, mat, notNull) =>{ var v = (bool2)value; mat.SetVector(shaderVarName, new Vector4(v.x?1:0,v.y?1:0,0,0));} },
            {typeof(bool3),         (value, shaderVarName, mat, notNull) =>{ var v = (bool3)value; mat.SetVector(shaderVarName, new Vector4(v.x?1:0,v.y?1:0,v.z?1:0,0));} },
            {typeof(bool4),         (value, shaderVarName, mat, notNull) =>{ var v = (bool4)value; mat.SetVector(shaderVarName, new Vector4(v.x?1:0,v.y?1:0,v.z?1:0,v.w?1:0));} },
            {typeof(int2),          (value, shaderVarName, mat, notNull) =>{ var v = (int2)value;  mat.SetVector(shaderVarName, new Vector4(v.x,v.y,0,0));} },
            {typeof(int3),          (value, shaderVarName, mat, notNull) =>{ var v = (int3)value;  mat.SetVector(shaderVarName, new Vector4(v.x,v.y,v.z,0));} },
            {typeof(int4),          (value, shaderVarName, mat, notNull) =>{ var v = (int4)value;  mat.SetVector(shaderVarName, new Vector4(v.x,v.y,v.z,v.w));} },
            {typeof(float2),        (value, shaderVarName, mat, notNull) =>{ var v = (float2)value;mat.SetVector(shaderVarName, new Vector4(v.x,v.y,0,0));} },
            {typeof(float3),        (value, shaderVarName, mat, notNull) =>{ var v = (float3)value;mat.SetVector(shaderVarName, new Vector4(v.x,v.y,v.z,0));} },
            {typeof(float4),        (value, shaderVarName, mat, notNull) =>{ var v = (float4)value;mat.SetVector(shaderVarName, new Vector4(v.x,v.y,v.z,v.w));} },
            {typeof(Vector2),       (value, shaderVarName, mat, notNull) =>{ mat.SetVector(shaderVarName, (Vector2)value);} },
            {typeof(Vector3),       (value, shaderVarName, mat, notNull) =>{ mat.SetVector(shaderVarName, (Vector3)value);} },
            {typeof(Vector4),       (value, shaderVarName, mat, notNull) =>{ mat.SetVector(shaderVarName, (Vector4)value);} },
            {typeof(Color),         (value, shaderVarName, mat, notNull) =>{ mat.SetColor(shaderVarName,  (Color)value);} },
            {typeof(Texture),       (value, shaderVarName, mat, notNull) =>{ SetTexture(shaderVarName, (Texture)value, mat, notNull);} },
            {typeof(Texture2D),     (value, shaderVarName, mat, notNull) =>{ SetTexture(shaderVarName, (Texture2D)value, mat, notNull);} },
            {typeof(Texture3D),     (value, shaderVarName, mat, notNull) =>{ SetTexture(shaderVarName, (Texture3D)value, mat, notNull);} },
            {typeof(RenderTexture), (value, shaderVarName, mat, notNull) =>{ SetTexture(shaderVarName, (RenderTexture)value, mat, notNull);} },
        };

		static private void SetTexture(string shaderName, Texture texture, ComputeShader computeShader, string kernel, bool mustNotNull)
		{
            if(texture == null && mustNotNull) LogTool.Log(shaderName + " Must Not null", LogLevel.Warning);

            if(texture != null)
            {
				if (texture is Texture2D) computeShader.SetVector(shaderName + "Size", new Vector4(texture.width, texture.height));
				if (texture is RenderTexture rt) computeShader.SetVector(shaderName + "Size", new Vector4(rt.width, rt.height, rt.depth));
				if (texture is Texture3D t3d) computeShader.SetVector(shaderName + "Size", new Vector4(t3d.width, t3d.height, t3d.depth));
            }
			computeShader.SetTexture(computeShader.FindKernel(kernel), shaderName, texture);
		}
		static private void SetTexture(string shaderName, Texture texture, Material material, bool mustNotNull)
		{
            if(texture == null && mustNotNull) LogTool.Log(shaderName + " Must Not null", LogLevel.Warning);

            if(texture != null) 
            {
				if (texture is Texture2D) material.SetVector(shaderName + "Size", new Vector4(texture.width, texture.height));
				if (texture is RenderTexture rt) material.SetVector(shaderName + "Size", new Vector4(rt.width, rt.height, rt.depth));
				if (texture is Texture3D t3d) material.SetVector(shaderName + "Size", new Vector4(t3d.width, t3d.height, t3d.depth));
            }
			material.SetTexture(shaderName, texture);
		}
        internal string shaderName;
        internal bool mustNotNull = false;
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

            TypeSetterMap[t].Invoke(value, this.shaderName, cs, kernel, this.mustNotNull);
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

            TypeSetterMatMap[t].Invoke(value, this.shaderName, material, this.mustNotNull);
        }
        public virtual void Release()
        {
        }
    }
    public class GPUBufferVariable<T> : GPUVariable, IGPUContainer
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
                return this.gpuBuffer ??= new ComputeBuffer(this.Size, Marshal.SizeOf<T>(), this.type);
            }
        } 
        public T[] CPUData => this.cpuData;
        public int Size => this.size;
        public string ShaderName => this.shaderName;
		protected bool Inited => this.inited;
        private bool inited = false; 

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
            
            this.inited = true;
        }

        public virtual void InitBuffer(GPUBufferVariable<T> other)
        {
            LogTool.AssertIsTrue(other.Size > 0);
            LogTool.AssertIsTrue(this != other);//self assignment is dangerous

            this.Release();

            this.size = other.Size;
            this.type = other.type;
            this.autoSet = other.autoSet;
            if(other.cpuData != null)
            {
                if(this.cpuData == null || this.cpuData.Length != other.cpuData.Length) this.cpuData = new T[this.size];
                Array.Copy(other.cpuData, this.cpuData, this.size);
            }
            this.gpuBuffer = other.Data;

            this.inited = true;
        }
        public void UpdateBuffer(GPUBufferVariable<T> other)
        {
            this.size = other.size;
            this.type = other.type;
            this.autoSet = other.autoSet;
            if(other.cpuData != null)
            {
                if(this.cpuData == null || this.cpuData.Length != other.cpuData.Length) this.cpuData = new T[this.size];
                Array.Copy(other.cpuData, this.cpuData, this.size);
            }
            this.gpuBuffer = other.Data;
            
            this.inited = true;
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
            LogTool.AssertNotNull(cs);
            LogTool.AssertNotNull(kernel);
            // LogTool.AssertIsTrue(this.Size > 0);
			// if (cs == null || this.Size == 0) { LogTool.Log(this.displayName + " is not set to GPU", LogLevel.Warning); return; }
			if (cs == null || !this.Inited) return;

            this.SetToGPUBuffer();
            var id = cs.FindKernel(kernel);
            cs.SetInt(this.shaderName + "Count", this.Size);
            cs.SetBuffer(id, this.shaderName, this.Data);
        }
        public override void SetToMaterial(object container, Material material)
        {
            LogTool.AssertNotNull(material);
            if (material == null || !this.Inited) return;
            this.SetToGPUBuffer();
            material.SetBuffer(this.shaderName, this.Data);
            material.SetInt(this.shaderName+"Count", this.Size);
        }
		public void UpdateGPU(ComputeShader computeShader, string kernel = null)
		{
			this.SetToGPU(null, computeShader, kernel);
		}

		public void UpdateGPU(Material material)
		{
            this.SetToMaterial(null, material);
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

        public void OverWriteCPUData(T[] newData)
        {
            this.cpuData = newData;
        }

        public void GetToCPUData()
        {
            if(this.CPUData != null)
            {
                this.Data.GetData(this.CPUData);
            }
            else
            {
                LogTool.Log("No CPU Data available", LogLevel.Warning);
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
			this.SetToGPUBuffer(true);
        }
    }
    public class GPUBufferAppendConsume<T>: GPUBufferVariable<T>
    {
        protected GPUBufferVariable<int> counterBuffer;
		protected GPUBufferVariable<int> CounterBuffer => this.counterBuffer ??= new GPUBufferVariable<int>(MIN_INDIRECT_BUFFER_SIZE, true, false, ComputeBufferType.IndirectArguments);

        public void InitAppendBuffer(int size, bool autoSet = false)
        {
            LogTool.AssertIsTrue(size > 0);

            //no cpu data for append buffer
            base.InitBuffer(size, false, autoSet, ComputeBufferType.Append);
            this.ResetCounter();
        }
        public void InitAppendBuffer(GPUBufferAppendConsume<T> other)
        {
            LogTool.AssertIsTrue(other.Size > 0);

            //no cpu data for append buffer
            base.InitBuffer(other);
            this.ResetCounter();
        }

        public void ResetCounter(uint counter = 0)
        {
            LogTool.AssertIsTrue(this.Size > 0);
            this.Data.SetCounterValue(counter);

            this.CounterBuffer.ClearData();
        }

        public int GetCounter()
        {
            //Note: GetCounter is very expensive, use it wisely
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
        public override void InitBuffer(GPUBufferVariable<T> other)
        {
            LogTool.Log("Use InitAppendBuffer(GPUBufferAppendConsume<T> other)", LogLevel.Warning);
            LogTool.AssertIsTrue(other is GPUBufferAppendConsume<T>);

            this.InitAppendBuffer(other as GPUBufferAppendConsume<T>);
        }

        public override void Release()
        {
            base.Release();
            this.counterBuffer?.Release();
            this.counterBuffer = null;
        }
        public override void SetToGPU(object container, ComputeShader cs, string kernel = null)
        {
            base.SetToGPU(container, cs, kernel);

			if (cs == null || !this.Inited) return;

            var id = cs.FindKernel(kernel);
            ComputeBuffer.CopyCount(this.Data, this.CounterBuffer, 0);
            cs.SetBuffer(id, this.shaderName + "ActiveCount", this.CounterBuffer);
        }
        public override void SetToMaterial(object container, Material material)
        {
            base.SetToMaterial(container, material);

			if (material == null || !this.Inited) return;

            ComputeBuffer.CopyCount(this.Data, this.CounterBuffer, 0);
            material.SetBuffer(this.shaderName + "ActiveCount", this.CounterBuffer);
        }
    }
}
