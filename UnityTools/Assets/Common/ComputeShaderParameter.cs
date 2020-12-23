using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;
using UnityTools.Debuging;

namespace UnityTools.ComputeShaderTool
{
    [AttributeUsage(AttributeTargets.Field)]
    public class NoneGPUAttribute : Attribute
    {

    }
    [AttributeUsage(AttributeTargets.Field)]
    public class ShaderNameAttribute : Attribute
    {
        internal string csName;
        public ShaderNameAttribute(string name)
        {
            this.csName = name;
        }

    }
    public abstract class ComputeShaderParameterContainer
    {
        protected List<IComputeShaderParameter> VarList
        {
            get
            {
                this.InitVariableList();
                return this.variableList;
            }
        }
        protected List<IComputeShaderParameter> variableList = null;

        private Dictionary<FieldInfo, IComputeShaderParameter> noneCSVariables = new Dictionary<FieldInfo, IComputeShaderParameter>();

        protected const bool DebugOutput = true;
        protected bool inited = false;

        public ComputeShaderParameterContainer()
        {
            this.InitVariableList();
        }

        public void UpdateGPU(ComputeShader cs, string kernel = null)
        {
            foreach (var p in this.VarList)
            {
                if (p == null)
                {
                    Debug.LogWarningFormat("variable is null, are you using a non-SerializeField private/protected variable?\nUnity will not create a instance for this");
                }
                else
                {
                    p.SetToGPU(cs, kernel);
                }
            }

            foreach (var np in this.noneCSVariables)
            {
                this.UpdateNoneCSParameterValue(np);
                np.Value.SetToGPU(cs, kernel);
            }

        }
        public virtual void OnGUI()
        {
            foreach (var p in this.VarList)
            {
                p.OnGUI();
            }
            foreach (var p in this.noneCSVariables.Values)
            {
                p.OnGUI();
            }
        }

        public virtual void ReleaseBuffer()
        {
            var bufferList = this.VarList.Where(b => b is ComputeShaderParameterBuffer && (b as ComputeShaderParameterBuffer).Value != null);

            bufferList.ToList().ForEach(b =>
            {
                var buffer = (b as ComputeShaderParameterBuffer);
                //TODO Release called multiple time, is it safe?
                buffer.Release();
            });
        }

        public virtual void ReleaseTexture()
        {
            var bufferList = this.VarList.Where(b => b is ComputeShaderParameterTexture && (b as ComputeShaderParameterTexture).Value != null);

            bufferList.ToList().ForEach(b =>
            {
                var buffer = (b as ComputeShaderParameterTexture);
                buffer.Value?.DestoryObj();
            });
        }

        /// <summary>
        /// This function will get all ComputeShaderParameterBase parameters
        /// Stores them into list and used to update GPU data
        /// </summary>
        protected void InitVariableList()
        {
            if (this.inited) return;

            LogTool.AssertIsTrue(this.variableList == null);
            var bindingFlags = BindingFlags.Instance |
                              BindingFlags.NonPublic |
                              BindingFlags.Public;

            this.variableList = this.GetType()
                     .GetFields(bindingFlags)
                     .Where(field
                        => field.FieldType.IsSubclassOf(typeof(IComputeShaderParameter))
                        && !Attribute.IsDefined(field, typeof(NoneGPUAttribute)))
                     .Select(field => field.GetValue(this) as IComputeShaderParameter)
                     .ToList();


            var noneCSParamter = this.GetType()
                     .GetFields(bindingFlags)
                     .Where(field => !field.FieldType.IsSubclassOf(typeof(IComputeShaderParameter))
                        && !Attribute.IsDefined(field, typeof(NoneGPUAttribute)))
                     .ToList();


            foreach (var p in noneCSParamter)
            {
                var csp = this.CreateParameter(p);
                if (csp == default) continue;

                this.noneCSVariables.Add(p, csp);
            }
            LogTool.AssertIsTrue(this.variableList != null);
            
            this.inited = true;
        }

        protected IComputeShaderParameter CreateParameter(FieldInfo info)
        {
            var name = info.Name;
            if (Attribute.IsDefined(info, typeof(ShaderNameAttribute)))
            {
                var attrib = Attribute.GetCustomAttribute(info, typeof(ShaderNameAttribute)) as ShaderNameAttribute;
                name = attrib.csName;
            }
            if (info.FieldType == typeof(int)) return new ComputeShaderParameterInt(name, (int)info.GetValue(this));
            if (info.FieldType == typeof(float)) return new ComputeShaderParameterFloat(name, (float)info.GetValue(this));
            if (info.FieldType == typeof(Vector2)) return new ComputeShaderParameterVector(name, (Vector2)info.GetValue(this));
            if (info.FieldType == typeof(Vector3)) return new ComputeShaderParameterVector(name, (Vector3)info.GetValue(this));
            if (info.FieldType == typeof(Vector4)) return new ComputeShaderParameterVector(name, (Vector4)info.GetValue(this));
            if (info.FieldType == typeof(Color)) return new ComputeShaderParameterColor(name, (Color)info.GetValue(this));

            return default;

        }

        protected void UpdateNoneCSParameterValue(KeyValuePair<FieldInfo, IComputeShaderParameter> np)
        {
            var info = np.Key;

            if (info.FieldType == typeof(int)) (np.Value as ComputeShaderParameterInt).Value = (int)info.GetValue(this);
            if (info.FieldType == typeof(float)) (np.Value as ComputeShaderParameterFloat).Value = (float)info.GetValue(this);
            if (info.FieldType == typeof(Vector2)) (np.Value as ComputeShaderParameterVector).Value = (Vector2)info.GetValue(this);
            if (info.FieldType == typeof(Vector3)) (np.Value as ComputeShaderParameterVector).Value = (Vector3)info.GetValue(this);
            if (info.FieldType == typeof(Vector4)) (np.Value as ComputeShaderParameterVector).Value = (Vector4)info.GetValue(this);
            if (info.FieldType == typeof(Color)) (np.Value as ComputeShaderParameterColor).Value = (Color)info.GetValue(this);
        }


    }

    public interface IComputeShaderParameter
    {
        /// <summary>
        /// Set data to GPU, provide kernal name for textures and buffer parameters
        /// </summary>
        /// <param name="kernel"></param>
        void SetToGPU(ComputeShader cs, string kernel = null);
        /// <summary>
        /// Draw GUI, mainly used by PrefsXXX
        /// </summary>
        void OnGUI();
        /// <summary>
        /// Bind ComputeShader to this parameter to update
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>

        void UpdateValue(IComputeShaderParameter other);

        /// <summary>
        /// Internal function for call SetXXX API of Unity
        /// </summary>
        /// <param name="kernel"></param>
        void Set(ComputeShader cs, string kernel = null);

        /*internal abstract void OnSerialize(Stream stream, IFormatter formater);
        internal abstract void OnDeserialize(Stream stream, IFormatter formater);*/

    }
    [Serializable]
    public abstract class ComputeShaderParameter<T> : IComputeShaderParameter
    {
        protected string VariableName
        {
            get { return this.variableName; }
            set
            {
                Assert.IsNotNull(value);
                this.variableName = value;
                this.propertyID = Shader.PropertyToID(this.variableName);
            }
        }
        protected string variableName = null;
        /// <summary>
        /// PropertyID for set shader object
        /// </summary>
        protected int propertyID = -1;
        [SerializeField] protected T data;

        //only update value that has been Serialized above
        public void UpdateValue(IComputeShaderParameter other)
        {
            var newValue = other as ComputeShaderParameter<T>;
            if (newValue != null)
            {
                this.VariableName = newValue.VariableName;
                this.Value = newValue.Value;
            }
            else
            {
                Debug.LogWarning("Cannot update value for type " + this.GetType().ToString());
            }
        }


        protected virtual bool IsShaderValid(ComputeShader cs, string kernel = null)
        {
            //only ComputeShaderKernalParameter use kernel string
            return cs != null && this.VariableName != null;
        }

        public ComputeShaderParameter(string name, T defaultValue = default(T))
        {
            LogTool.AssertIsFalse(string.IsNullOrEmpty(name));
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarningFormat("Name is null");
                return;
            }
            this.VariableName = name;
            this.data = defaultValue;
        }
        public ComputeShaderParameter(string name)
        {
            this.VariableName = name;
        }

        public static implicit operator T(ComputeShaderParameter<T> value)
        {
            return value.Value;
        }

        public virtual T Value
        {
            get { return this.data; }
            set { this.data = value; }
        }

        public void SetToGPU(ComputeShader cs, string kernel = null)
        {
            if (this.IsShaderValid(cs, kernel) && this.data != null)
            {
                this.Set(cs, kernel);
            }
            else
            {
                if (this.data == null)
                {
                    LogTool.Log(this.VariableName + " Data is null", LogLevel.Warning);
                }
                else
                {
                    LogTool.LogFormat("Please bind {0} with compute shader first/or supply kernal name", LogLevel.Warning, LogChannel.Debug, this.VariableName);
                }
            }
        }

        public abstract void Set(ComputeShader cs, string kernel = null);

        public void OnGUI()
        {
            #if USE_PREFS
            var prefs = this.data as PrefsParam;
            if (prefs != null)
            {
                prefs.OnGUI();
            }
            #else
            ConfigureGUI.OnGUI(ref this.data, this.VariableName);
            #endif
        }

    }
    [Serializable]
    public abstract class ComputeShaderKernelParameter<T> : ComputeShaderParameter<T>
    {
        protected Dictionary<string, int> kernel = new Dictionary<string, int>();
        public ComputeShaderKernelParameter(string name, T defaultValue) : base(name, defaultValue) { }
        protected override bool IsShaderValid(ComputeShader cs, string kernel = null)
        {
            Assert.IsNotNull(kernel);
            if (kernel == null)
            {
                Debug.LogErrorFormat("You are setting a variable that requires kernel name, try to supply name string in UpdateGPU(string kernel)");
                return false;
            }
            if (this.kernel.ContainsKey(kernel))
            {
                return base.IsShaderValid(cs, kernel);
            }
            else
            {
                var ker = cs.FindKernel(kernel);
                Assert.IsTrue(ker >= 0);
                if (ker >= 0)
                {
                    this.kernel.Add(kernel, ker);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
    [Serializable]
    public class ComputeShaderParameterInt : ComputeShaderParameter<int>
    {
        public ComputeShaderParameterInt(string name, int defaultValue = default) : base(name, defaultValue) { }

        public override void Set(ComputeShader cs, string kernel = null)
        {
            if (this.propertyID != -1)
            {
                cs.SetInt(this.propertyID, this.data);
            }
            else
            {
                cs.SetInt(this.VariableName, this.data);
            }
        }
    }

    [Serializable]
    public class ComputeShaderParameterFloat : ComputeShaderParameter<float>
    {
        public ComputeShaderParameterFloat(string name, float defaultValue = default) : base(name, defaultValue) { }

        public override void Set(ComputeShader cs, string kernel = null)
        {
            if (this.propertyID != -1)
            {
                cs.SetFloat(this.propertyID, this.data);
            }
            else
            {
                cs.SetFloat(this.VariableName, this.data);
            }
        }
    }

    [Serializable]
    public class ComputeShaderParameterFloats : ComputeShaderParameter<float[]>
    {
        public ComputeShaderParameterFloats(string name, float[] defaultValue = default) : base(name, defaultValue) { }

        public override void Set(ComputeShader cs, string kernel = null)
        {
            if (this.propertyID != -1)
            {
                cs.SetFloats(this.propertyID, this.data);
            }
            else
            {
                cs.SetFloats(this.VariableName, this.data);
            }

        }
    }

    [Serializable]
    public class ComputeShaderParameterVector : ComputeShaderParameter<Vector4>
    {
        public ComputeShaderParameterVector(string name, Vector4 defaultValue = default) : base(name, defaultValue) { }

        public override void Set(ComputeShader cs, string kernel = null)
        {
            if (this.propertyID != -1)
            {
                cs.SetVector(this.propertyID, this.data);
            }
            else
            {
                cs.SetVector(this.VariableName, this.data);
            }
        }
    }
    [Serializable]
    public class ComputeShaderParameterVectorArray : ComputeShaderParameter<Vector4[]>
    {

        public ComputeShaderParameterVectorArray(string name, Vector4[] defaultValue = default) : base(name, defaultValue) { }
        public override void Set(ComputeShader cs, string kernel = null)
        {
            if (this.propertyID != -1)
            {
                cs.SetVectorArray(this.propertyID, this.data);
            }
            else
            {
                cs.SetVectorArray(this.VariableName, this.data);
            }
        }

    }
    [Serializable]
    public class ComputeShaderParameterColor : ComputeShaderParameter<Color>
    {
        public ComputeShaderParameterColor(string name, Color defaultValue = default) : base(name, defaultValue) { }

        public override void Set(ComputeShader cs, string kernel = null)
        {
            if (this.propertyID != -1)
            {
                cs.SetVector(this.propertyID, this.data);
            }
            else
            {
                cs.SetVector(this.VariableName, this.data);
            }
        }
    }

    [Serializable]
    public class ComputeShaderParameterMatrix : ComputeShaderParameter<Matrix4x4>
    {
        public ComputeShaderParameterMatrix(string name, Matrix4x4 defaultValue = default) : base(name, defaultValue) { }

        public override void Set(ComputeShader cs, string kernel = null)
        {
            if (this.propertyID != -1)
            {
                cs.SetMatrix(this.propertyID, this.data);
            }
            else
            {
                cs.SetMatrix(this.VariableName, this.data);
            }
        }
    }

    /// <summary>
    /// This only Set buffer to GPU, it does not manage this Texture resource
    /// </summary>
    [Serializable]
    public class ComputeShaderParameterTexture : ComputeShaderKernelParameter<Texture>
    {
        public ComputeShaderParameterTexture(string name, Texture defaultValue = default) : base(name, defaultValue) { }
        public virtual void Release()
        {
            if (this.data != null)
            {
                this.data.DestoryObj();
                this.data = null;
            }
        }
        public override void Set(ComputeShader cs, string kernel = null)
        {
            if (this.kernel.ContainsKey(kernel) && this.data != null)
            {
                if (this.propertyID != -1)
                {
                    cs.SetTexture(this.kernel[kernel], this.propertyID, this.data);
                }
                else
                {
                    cs.SetTexture(this.kernel[kernel], this.VariableName, this.data);
                }
            }
            else
            {
                if (this.data == null)
                {
                    Debug.LogWarningFormat("var name {0} instance: {1} is null", this.VariableName, this.data);
                }
                else
                {
                    Debug.LogWarningFormat("Can not found {0} in shader {1} with var name {2} instance: {3}", kernel, cs.name, this.VariableName, this.data);
                }
            }
        }
    }
    /// <summary>
    /// This only Set buffer to GPU, it does not manage this buffer resource
    /// </summary>
    public class ComputeShaderParameterBuffer : ComputeShaderKernelParameter<ComputeBuffer>
    {

        static public void SwapBuffer(ComputeShaderParameterBuffer a, ComputeShaderParameterBuffer b)
        {
            var temp = a.Value;
            a.Value = b.Value;
            b.Value = temp;
        }
        public ComputeShaderParameterBuffer(string name, ComputeBuffer defaultValue = default) : base(name, defaultValue) { }
        public virtual void Release()
        {
            if (this.data != null)
            {
                this.data.Release();
                this.data = null;
            }
        }

        public override void Set(ComputeShader cs, string kernel = null)
        {
            if (this.kernel.ContainsKey(kernel) && this.data != null)
            {
                if (this.propertyID != -1)
                {
                    cs.SetBuffer(this.kernel[kernel], this.propertyID, this.data);
                }
                else
                {
                    cs.SetBuffer(this.kernel[kernel], this.VariableName, this.data);
                }
            }
            else
            {
                if (this.data == null)
                {
                    Debug.LogWarningFormat("var name {0} instance: {1} is null", this.VariableName, this.data);
                }
                else
                {
                    Debug.LogWarningFormat("Can not found {0} in shader {1} with var name {2} instance: {3}", kernel, cs.name, this.VariableName, this.data);
                }
            }
        }


    }

    public class ComputeShaderParameterBuffer<T> : ComputeShaderParameterBuffer
    {
        public override ComputeBuffer Value { get => base.Value; set => LogTool.LogAssertIsTrue(false, "Use InitBuffer to setup buffer"); }

        public T[] CPUData => this.cpuData;
        public int Size { get; private set; }

        protected T[] cpuData = null;

        public ComputeShaderParameterBuffer(string name, ComputeBuffer defaultValue = default) : base(name, defaultValue) { }
        public ComputeShaderParameterBuffer(string name, int size, bool cpu = false, ComputeBuffer defaultValue = null) : base(name, defaultValue)
        {
            this.InitBuffer(size, cpu);
        }
        public void InitBuffer(int size, bool cpu = false)
        {
            this.Size = size;
            LogTool.AssertIsTrue(size > 0);
            if(this.data != null)
            {
                LogTool.Log("ComputeBuffer is not null", LogLevel.Warning);
                this.Release();
            }
            this.data = new ComputeBuffer(size, Marshal.SizeOf<T>());

            if(cpu) this.cpuData = new T[size];
        }

        public override void Release()
        {
            base.Release();
            this.cpuData = null;
        }

        public override void Set(ComputeShader computeShader, string kernel)
        {
            this.UpdateToGPU();
            base.Set(computeShader, kernel);
        }

        public void UpdateToGPU()
        {
            if(this.CPUData != null)
            {
                this.data.SetData(this.CPUData);
            }
        }

    }

}
