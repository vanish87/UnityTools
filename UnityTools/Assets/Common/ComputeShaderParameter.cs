
//#define USE_PREFS

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Assertions;

#if USE_PREFS
using PrefsGUI;
#endif

namespace UnityTools.Common
{
    public class ComputeShaderParameterFileContainer : ComputeShaderParameterContainer
    {
        public ComputeShaderParameterFileContainer() : base() { }
        public ComputeShaderParameterFileContainer(string fileName, ComputeShader cs): base(cs)
        {

        }

        public void LoadFile(string fileName)
        {
            var shaderBasePath = Application.streamingAssetsPath;
            var outputPath = shaderBasePath;

            var fi = new FileInfo(Path.Combine(outputPath, fileName));
            using (var binaryFile = fi.OpenRead())
            {
                var serializer = new BinaryFormatter();
                var newValues = (List<ComputeShaderParameterBase>)serializer.Deserialize(binaryFile);
                var candidate = this.VarList.Where(v => v.IsSerializable() == true).ToList();

                Assert.IsTrue(newValues.Count == candidate.Count);
                for(var i = 0; i < candidate.Count; ++i)
                {
                    candidate[i].UpdateValue(newValues[i]);
                }
            }

            this.Bind(this.currentCS);
        }

        public void SaveFile(string fileName)
        {
            var shaderBasePath = Application.streamingAssetsPath;
            var outputPath = shaderBasePath;

            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            var candidates = this.VarList.Where(v => v.IsSerializable() == true);

            var fi = new FileInfo(Path.Combine(outputPath, fileName));
            using (var binaryFile = fi.Create())
            {
                var serializer = new BinaryFormatter();
                serializer.Serialize(binaryFile, candidates);
                binaryFile.Flush();
            }
        }

    }
    public abstract class ComputeShaderParameterContainer
    {
        protected List<ComputeShaderParameterBase> VarList
        {
            get
            {
                if (this.variableList == null)
                {
                    this.InitVariableList();
                }
                return this.variableList;
            }
        }
        protected List<ComputeShaderParameterBase> variableList = null;
        protected ComputeShader currentCS = null;
        public ComputeShaderParameterContainer(ComputeShader cs)
        {
            this.Bind(cs);
        }
        public ComputeShaderParameterContainer() { }

        public void Bind(ComputeShader cs)
        {
            Assert.IsNotNull(cs);
            this.currentCS = cs;

            foreach (var p in this.VarList)
            {
                if (p == null)
                {
                    Debug.LogWarningFormat("variable is null, are you using a non-SerializeField private variable?\nUnity will not create a instance for this");
                }
                else
                {
                    p.Bind(this.currentCS);
                }
            }
        }
        public void UpdateGPU(string kernal = null)
        {
            foreach (var p in this.VarList)
            {
                if (p == null)
                {
                    Debug.LogWarningFormat("variable is null, are you using a non-SerializeField private/protected variable?\nUnity will not create a instance for this");
                }
                else
                {
                    p.SetToGPU(kernal);
                }
            }
        }
        public virtual void OnGUI()
        {
            foreach (var p in this.VarList)
            {
                p.OnGUI();
            }
        }

        /// <summary>
        /// This function will get all ComputeShaderParameterBase parameters
        /// Stores them into list and used to update GPU data
        /// </summary>
        protected void InitVariableList()
        {
            Assert.IsTrue(this.variableList == null);
            var bindingFlags = BindingFlags.Instance |
                              BindingFlags.NonPublic |
                              BindingFlags.Public;

            this.variableList = this.GetType()
                     .GetFields(bindingFlags)
                     .Where(field => field.FieldType.IsSubclassOf(typeof(ComputeShaderParameterBase)))
                     .Select(field => field.GetValue(this) as ComputeShaderParameterBase)
                     .ToList();

            Assert.IsTrue(this.variableList != null);
        }

    }

    [Serializable]
    public abstract class ComputeShaderParameterBase 
    {
        /// <summary>
        /// Set data to GPU, provide kernal name for textures and buffer parameters
        /// </summary>
        /// <param name="kernal"></param>
        public abstract void SetToGPU(string kernal = null);
        /// <summary>
        /// Draw GUI, mainly used by PrefsXXX
        /// </summary>
        public abstract void OnGUI();
        /// <summary>
        /// Bind ComputeShader to this parameter to update
        /// </summary>
        /// <param name="cs"></param>
        /// <returns></returns>
        public abstract bool Bind(ComputeShader cs);

        public abstract void UpdateValue(ComputeShaderParameterBase other);
        public virtual bool IsSerializable() { return true; }

        /// <summary>
        /// Internal function for call SetXXX API of Unity
        /// </summary>
        /// <param name="kernal"></param>
        protected abstract void Set(string kernal = null);

        /*internal abstract void OnSerialize(Stream stream, IFormatter formater);
        internal abstract void OnDeserialize(Stream stream, IFormatter formater);*/
    }
    [Serializable]
    public abstract class ComputeShaderParameter<T> : ComputeShaderParameterBase, ISerializable, IXmlSerializable
    {
        protected ComputeShader shader = null;
        
        protected string variableName = null;
        [SerializeField] protected T data;

        #region ISerializable
        public ComputeShaderParameter(SerializationInfo info, StreamingContext context)
        {
            variableName = (string)info.GetValue("variableName", typeof(string));
            data = (T)info.GetValue("data", typeof(T));
        }
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("variableName", variableName);
            info.AddValue("data", data);
        }
        //only update value that has been Serialized above
        public override void UpdateValue(ComputeShaderParameterBase other)
        {
            var newValue = other as ComputeShaderParameter<T>;
            if (newValue != null)
            {
                this.variableName = newValue.variableName;
                this.Value = newValue.Value;
            }
        }

        #endregion

        #region IXmlSerializable
        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
            while (reader.Read())
            {
                // Only detect start elements.
                if (reader.IsStartElement())
                {
                    // Get element name and switch on it.
                    switch (reader.Name)
                    {

                    }

                    Debug.Log(reader.Name);
                }
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
            writer.WriteStartElement("MyList");
            {
                writer.WriteElementString("ListItem", this.data.ToString());
            }
            writer.WriteEndElement();
        }
        /*internal override void OnSerialize(Stream stream, IFormatter formater)
        {
            formater.Serialize(stream, this);
        }

        internal override void OnDeserialize(Stream stream, IFormatter formater)
        {
            var obj = (ComputeShaderParameter<T>)formater.Deserialize(stream);
            this.variableName = obj.variableName;
            this.data = obj.data;
        }*/
        #endregion

        protected virtual bool IsShaderValid(string kernal = null)
        {
            //only ComputeShaderKernalParameter use kernal string
            return this.shader != null && this.variableName != null;
        }

        public ComputeShaderParameter(string name, T defaultValue = default(T))
        {
            Assert.IsFalse(string.IsNullOrEmpty(name));
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarningFormat("Name is null");
                return;
            }
            this.variableName = name;
            this.data = defaultValue;
        }
        public ComputeShaderParameter(ComputeShader shader, string varName)
        {
            this.variableName = varName;
            this.Bind(shader);
        }

        public virtual T Value
        {
            get { return this.data; }
            set { this.data = value;}
        }

        public override bool Bind(ComputeShader cs)
        {
            Assert.IsNotNull(cs);

            this.shader = cs;
            return true;
        }

        public override void SetToGPU(string kernal = null)
        {
            if (this.IsShaderValid(kernal) && this.data != null)
            {
                this.Set(kernal);
            }
            else
            {
                if (this.data == null)
                {
                    Debug.LogWarningFormat(this.variableName + " Data is null");
                }
                else
                {
                    Debug.LogWarningFormat("Please bind {0} with compute shader first/or supply kernal name", this.variableName);
                }
            }
        }

        public override void OnGUI()
        {
            #if USE_PREFS
            var prefs = this.data as PrefsParam;
            if (prefs != null)
            {
                prefs.OnGUI();
            }
            #endif
        }

    }
    [Serializable]
    public abstract class ComputeShaderKernalParameter<T> : ComputeShaderParameter<T>
    {
        protected Dictionary<string, int> kernal = new Dictionary<string, int>();
        public ComputeShaderKernalParameter(string name, T defaultValue) : base(name, defaultValue) { }
        public ComputeShaderKernalParameter(ComputeShader cs, string kernal, string varName) : base(cs, varName)
        {
            this.IsShaderValid(kernal);
        }

        protected override bool IsShaderValid(string kernal = null)
        {
            Assert.IsNotNull(kernal);
            if (kernal == null)
            {
                Debug.LogErrorFormat("You are setting a variable that requires kernal name, try to supply name string in UpdateGPU(string kernal)");
                return false;
            }
            if (this.kernal.ContainsKey(kernal))
            {
                return base.IsShaderValid();
            }
            else
            {
                var ker = this.shader.FindKernel(kernal);
                Assert.IsTrue(ker >= 0);
                if (ker >= 0)
                {
                    this.kernal.Add(kernal, ker);
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

        protected override void Set(string kernal = null)
        {
            this.shader.SetInt(this.variableName, this.data);
        }
    }

    [Serializable]
    public class ComputeShaderParameterFloat : ComputeShaderParameter<float>
    {
        public ComputeShaderParameterFloat(string name, float defaultValue = default) : base(name, defaultValue) { }

        public ComputeShaderParameterFloat(SerializationInfo info, StreamingContext context) : base(info, context) { }

        protected override void Set(string kernal = null)
        {
            this.shader.SetFloat(this.variableName, this.data);
        }
    }

    [Serializable]
    public class ComputeShaderParameterFloats : ComputeShaderParameter<float[]>
    {
        public ComputeShaderParameterFloats(string name, float[] defaultValue = default) : base(name, defaultValue) { }
        public ComputeShaderParameterFloats(ComputeShader cs, string name) : base(cs, name) { }

        protected override void Set(string kernal = null)
        {
            this.shader.SetFloats(this.variableName, this.data);
        }
    }

    [Serializable]
    public class ComputeShaderParameterVector : ComputeShaderParameter<Vector4>
    {
        public ComputeShaderParameterVector(string name, Vector4 defaultValue = default) : base(name, defaultValue) { }
        public ComputeShaderParameterVector(ComputeShader cs, string name) : base(cs, name) { }
        public ComputeShaderParameterVector(SerializationInfo info, StreamingContext context) : base("NotValid")
        {
            variableName = (string)info.GetValue("variableName", typeof(string));
            data.x = (float)info.GetValue("x", typeof(float));
            data.y = (float)info.GetValue("y", typeof(float));
            data.z = (float)info.GetValue("z", typeof(float));
            data.w = (float)info.GetValue("w", typeof(float));
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("variableName", variableName);
            info.AddValue("x", data.x);
            info.AddValue("y", data.y);
            info.AddValue("z", data.z);
            info.AddValue("w", data.w);
        }

        protected override void Set(string kernal = null)
        {
            this.shader.SetVector(this.variableName, this.data);
        }
    }
    [Serializable]
    public class ComputeShaderParameterVectorArray : ComputeShaderParameter<Vector4[]>
    {
        public ComputeShaderParameterVectorArray(string name, Vector4[] defaultValue = default) : base(name, defaultValue) { }
        public ComputeShaderParameterVectorArray(ComputeShader cs, string name) : base(cs, name) { }

        public override bool IsSerializable() { return false; }

        protected override void Set(string kernal = null)
        {
            this.shader.SetVectorArray(this.variableName, this.data);
        }
    }
    [Serializable]
    public class ComputeShaderParameterColor : ComputeShaderParameter<Color>
    {
        public ComputeShaderParameterColor(string name, Color defaultValue = default) : base(name, defaultValue) { }
        public ComputeShaderParameterColor(SerializationInfo info, StreamingContext context) : base("NotValid")
        {
            variableName = (string)info.GetValue("variableName", typeof(string));
            data.r = (float)info.GetValue("r", typeof(float));
            data.g = (float)info.GetValue("g", typeof(float));
            data.b = (float)info.GetValue("b", typeof(float));
            data.a = (float)info.GetValue("a", typeof(float));
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("variableName", variableName);
            info.AddValue("r", data.r);
            info.AddValue("g", data.g);
            info.AddValue("b", data.b);
            info.AddValue("a", data.a);
        }
        protected override void Set(string kernal = null)
        {
            this.shader.SetVector(this.variableName, this.data);
        }
    }

    [Serializable]
    public class ComputeShaderParameterMatrix : ComputeShaderParameter<Matrix4x4>
    {
        public ComputeShaderParameterMatrix(string name, Matrix4x4 defaultValue = default) : base(name, defaultValue) { }

        public override bool IsSerializable() { return false; }
        protected override void Set(string kernal = null)
        {
            this.shader.SetMatrix(this.variableName, this.data);
        }
    }

    /// <summary>
    /// This only Set buffer to GPU, it does not manage this Texture resource
    /// </summary>
    [Serializable]
    public class ComputeShaderParameterTexture : ComputeShaderKernalParameter<Texture>
    {
        public ComputeShaderParameterTexture(string name, Texture defaultValue = default) : base(name, defaultValue) { }

        public override bool IsSerializable() { return false; }
        protected override void Set(string kernal = null)
        {
            if (this.kernal.ContainsKey(kernal) && this.data != null)
            {
                this.shader.SetTexture(this.kernal[kernal], this.variableName, this.data);
            }
            else
            {
                if (this.data == null)
                {
                    Debug.LogWarningFormat("var name {0} instance: {1} is null", this.variableName, this.data);
                }
                else
                {
                    Debug.LogWarningFormat("Can not found {0} in shader {1} with var name {2} instance: {3}", kernal, this.shader.name, this.variableName, this.data);
                }
            }
        }
    }
    /// <summary>
    /// This only Set buffer to GPU, it does not manage this buffer resource
    /// </summary>
    [Serializable]
    public class ComputeShaderParameterBuffer : ComputeShaderKernalParameter<ComputeBuffer>
    {
        static public void SwapBuffer(ComputeShaderParameterBuffer a, ComputeShaderParameterBuffer b)
        {
            var temp = a.Value;
            a.Value = b.Value;
            b.Value = temp;
        }
        public ComputeShaderParameterBuffer(string name, ComputeBuffer defaultValue = default) : base(name, defaultValue) { }
        public ComputeShaderParameterBuffer(ComputeShader cs, string kernal, string varName) : base(cs, kernal, varName) { }

        public override bool IsSerializable() { return false; }
        public void Release()
        {
            if (this.data != null)
            {
                this.data.Release();
                this.data = null;
            }
        }

        protected override void Set(string kernal = null)
        {
            if (this.kernal.ContainsKey(kernal) && this.data != null)
            {
                this.shader.SetBuffer(this.kernal[kernal], this.variableName, this.data);
            }
            else
            {
                if (this.data == null)
                {
                    Debug.LogWarningFormat("var name {0} instance: {1} is null", this.variableName, this.data);
                }
                else
                {
                    Debug.LogWarningFormat("Can not found {0} in shader {1} with var name {2} instance: {3}", kernal, this.shader.name, this.variableName, this.data);
                }
            }
        }
    }

#if USE_PREFS
    [Serializable]
    public class ComputeShaderParameterPrefsInt : ComputeShaderParameter<PrefsInt>
    {
        public ComputeShaderParameterPrefsInt(string name, int defaultValue = default) : base(name)
        {
            data = new PrefsInt(name, defaultValue);
        }
        //public ComputeShaderParameterPrefsInt(ComputeShader cs, string name) : base(cs, name) { }
    
        public override bool IsSerializable() { return false; }
        protected override void Set(string kernal = null)
        {
            this.shader.SetInt(this.variableName, this.data);
        }
    }
    [Serializable]
    public class ComputeShaderParameterPrefsFloat : ComputeShaderParameter<PrefsFloat>
    {
        public ComputeShaderParameterPrefsFloat(string name, float defaultValue = default) : base(name)
        {
            data = new PrefsFloat(name, defaultValue);
        }
        //public ComputeShaderParameterPrefsFloat(ComputeShader cs, string name) : base(cs, name) { }
        /*public ComputeShaderParameterFloat(ComputeShader cs, string name, ref float target) : base(cs, name, ref target) { }*/
    
        public override bool IsSerializable() { return false; }
        protected override void Set(string kernal = null)
        {
            this.shader.SetFloat(this.variableName, this.data);
        }
    }
    [Serializable]
    public class ComputeShaderParameterPrefsVector : ComputeShaderParameter<PrefsVector4>
    {
        public ComputeShaderParameterPrefsVector(string name, Vector4 defaultValue = default) : base(name)
        {
            data = new PrefsVector4(name, defaultValue);
        }
        //public ComputeShaderParameterPrefsFloat(ComputeShader cs, string name) : base(cs, name) { }
        /*public ComputeShaderParameterFloat(ComputeShader cs, string name, ref float target) : base(cs, name, ref target) { }*/
    
        public override bool IsSerializable() { return false; }
        protected override void Set(string kernal = null)
        {
            this.shader.SetVector(this.variableName, this.data);
        }
    }
    [Serializable]
    public class ComputeShaderParameterPrefsColor : ComputeShaderParameter<PrefsColor>
    {
        public ComputeShaderParameterPrefsColor(string name, Color defaultValue = default) : base(name)
        {
            data = new PrefsColor(name, defaultValue);
        }
        //public ComputeShaderParameterPrefsFloat(ComputeShader cs, string name) : base(cs, name) { }
        /*public ComputeShaderParameterFloat(ComputeShader cs, string name, ref float target) : base(cs, name, ref target) { }*/
    
        public override bool IsSerializable() { return false; }
        protected override void Set(string kernal = null)
        {
            this.shader.SetVector(this.variableName, this.data);
        }
    }
#endif
}
