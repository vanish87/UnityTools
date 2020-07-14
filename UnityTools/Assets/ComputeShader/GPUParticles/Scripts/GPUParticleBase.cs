#define USE_APPEND_BUFFER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;
using UnityTools.Common;

namespace UnityTools.ComputeShaderTool
{
    public class AlignedGPUData
    {
        //cbuffer data is packed in vector-4 size(16 bytes)
        //https://docs.microsoft.com/en-us/windows/desktop/direct3dhlsl/dx-graphics-hlsl-packing-rules
        //and it is better to align data as same as cbuffer did
        //https://developer.nvidia.com/content/understanding-structured-buffer-performance
    }

    /// <summary>
    /// GPUParticleClassBase will be able to use subclass of AlignedGPUData for reuse of class field
    /// But it does not have a CPU data because ComputeBuffer.SetData does not accept class array parameter
    /// So use compute shader to init data, it also support non-blittable type(eg. bool)
    /// It is better to use GPUParticleClassBase other than GPUParticleStructBase to keep extensible 
    /// but init particles in init kernal function 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GPUParticleClassBase<T> : GPUParticleBase<T> where T : AlignedGPUData
    {

    }

    /// <summary>
    /// GPUParticleStructBase will be able to use struct, and keep a CPU data for init
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GPUParticleStructBase<T> : GPUParticleBase<T> where T : struct
    {
        protected T[] CPUData = null;
        protected override void UpdateGPUDataBuffer()
        {
            base.UpdateGPUDataBuffer();

            Assert.IsNotNull(this.CPUData);

            this.bufferParameter.particlesDataBufferRead.Value.SetData(this.CPUData);
            this.bufferParameter.particlesDataBufferWrite.Value.SetData(this.CPUData);

        }
        protected override void OnCreateParticleData(int desiredNum)
        {
            base.OnCreateParticleData(desiredNum);
            this.CPUData = new T[desiredNum];
        }
    }

    /// <summary>
    /// Defined outside of GPUParticleBase to make it Serializable for Unity inspector
    /// </summary>
    [Serializable]
    public class GPUParticleCBufferParameterContainer : ComputeShaderParameterFileContainer
    {
        /// <summary>
        /// The desired number of particles, the particle buffer count may bigger than this 
        /// to achieve better data performance by making data aligned in 32 bytes 
        /// </summary>
        public ComputeShaderParameterInt numberOfParticles = new ComputeShaderParameterInt("_NumberOfParticles", 1024);
        public ComputeShaderParameterInt activeNumberOfParticles = new ComputeShaderParameterInt("_ActiveNumberOfParticles", 0);
    }
    public class GPUParticleBase<T> : MonoBehaviour
    {
        public class GPUParticleBufferParameterContainer : ComputeShaderParameterContainer
        {
            public ComputeShaderParameterBuffer particlesDataBufferRead = new ComputeShaderParameterBuffer("_ParticlesDataBufferRead");
            public ComputeShaderParameterBuffer particlesDataBufferWrite = new ComputeShaderParameterBuffer("_ParticlesDataBufferWrite");

            #if USE_APPEND_BUFFER
            public ComputeShaderParameterBuffer particlesIndexBufferActive = new ComputeShaderParameterBuffer("_ParticlesIndexBufferActive");
            public ComputeShaderParameterBuffer particlesIndexBufferDead = new ComputeShaderParameterBuffer("_ParticlesIndexBufferDead");
            public ComputeShaderParameterBuffer particlesIndexBufferInit = new ComputeShaderParameterBuffer("_ParticlesIndexBufferInit");

            public ComputeShaderParameterBuffer particlesDataBufferEmitWrite = new ComputeShaderParameterBuffer("_ParticlesDataBufferEmitWrite");
            #endif

            public int CurrentBufferLength { get { return this.currentBufferLength; } }
            protected int currentBufferLength = 0;


            public void InitBuffer(int bufferLength)
            {
                this.ReleaseBuffer();

                this.currentBufferLength = bufferLength;

                int dataSize = Marshal.SizeOf<T>();
                if (dataSize % 16 != 0)
                {
                    Debug.LogWarning("Data size " + dataSize + " is not aligned with 16 bytes");
                }

                this.particlesDataBufferRead.Value = new ComputeBuffer(bufferLength, dataSize);
                this.particlesDataBufferWrite.Value = new ComputeBuffer(bufferLength, dataSize);

                if (DebugOutput) Debug.Log(this.ToString() + " DataSize " + dataSize);

                #if USE_APPEND_BUFFER
                dataSize = Marshal.SizeOf<uint>();
                this.particlesIndexBufferActive.Value = new ComputeBuffer(bufferLength, dataSize, ComputeBufferType.Append);
                this.particlesIndexBufferDead.Value = this.particlesIndexBufferActive.Value;
                this.particlesIndexBufferInit.Value = this.particlesIndexBufferActive.Value;

                this.particlesDataBufferEmitWrite.Value = this.particlesDataBufferRead.Value;
                #endif

            }
        }

        [SerializeField] protected ComputeShaderDispatcher dispather;
        [SerializeField] protected GPUParticleCBufferParameterContainer parameter = new GPUParticleCBufferParameterContainer();
        [SerializeField] protected ComputeShader cs;
        [SerializeField] protected Material m;
        [SerializeField] protected Texture2D particleTexture;

        protected GPUParticleBufferParameterContainer bufferParameter = new GPUParticleBufferParameterContainer();

        protected virtual void OnCreateParticleData(int desiredNum)
        {
            this.bufferParameter.InitBuffer(desiredNum);
        }
        protected virtual void OnResetParticlesData()
        {
            #if USE_APPEND_BUFFER
            this.bufferParameter.particlesIndexBufferActive.Value.SetCounterValue(0);
            //they refer to same buffer instance, so only set count once
            //this.bufferParameter.particlesIndexBufferDead.Value.SetCounterValue(0);
            //this.bufferParameter.particlesIndexBufferInit.Value.SetCounterValue(0);
            this.dispather.Dispatch("Init", this.parameter.numberOfParticles.Value);
            #endif
        }
        protected virtual void UpdateGPUDataBuffer()
        {
        }

        protected virtual void Emit(int num)
        {
            this.parameter.activeNumberOfParticles.Value += num;

            #if USE_APPEND_BUFFER
            this.dispather.Dispatch("Emit", num);
            #endif

        }
        protected void ResizeBuffer(int desiredNum)
        {
            Assert.IsTrue(desiredNum > 0);
            if (this.bufferParameter.CurrentBufferLength != desiredNum)
            {
                this.OnCreateParticleData(desiredNum);
            }

            this.OnResetParticlesData();
            this.UpdateGPUDataBuffer();
        }

        #region MonoBehaviour
        // Start is called before the first frame update
        protected virtual void OnEnable()
        {
            Assert.IsNotNull(this.cs);

            this.dispather = new ComputeShaderDispatcher(this.cs);
            this.dispather.AddParameter("Force", this.parameter);
            this.dispather.AddParameter("Force", this.bufferParameter);

            this.dispather.AddParameter("Integration", this.parameter);
            this.dispather.AddParameter("Integration", this.bufferParameter);

            #if USE_APPEND_BUFFER
            this.dispather.AddParameter("Init", this.bufferParameter);
            this.dispather.AddParameter("Emit", this.bufferParameter);
            #endif

            this.ResizeBuffer(this.parameter.numberOfParticles.Value);

            //this.Emit(512);
        }

        protected virtual void OnDisable()
        {
            this.bufferParameter.ReleaseBuffer();
        }

        protected virtual void OnDestroy()
        {
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            //this.parameter.UpdateGPU();
            //this.bufferParameter.UpdateGPU("Force");
            this.dispather.Dispatch("Force", this.bufferParameter.CurrentBufferLength);
            this.dispather.Dispatch("Integration", this.bufferParameter.CurrentBufferLength);
        }
        protected virtual void OnRenderObject()
        {
            var inverseViewMatrix = Camera.main.worldToCameraMatrix.inverse;

            this.m.SetPass(0);
            this.m.SetMatrix("_InvViewMatrix", inverseViewMatrix);
            this.m.SetFloat("_ParticleSize", 1);
            this.m.SetBuffer("_ParticleBuffer", this.bufferParameter.particlesDataBufferRead.Value);
            this.m.SetTexture("_MainTex", this.particleTexture);

            this.OnSetRenderMaterial(this.m);

            Graphics.DrawProceduralNow(MeshTopology.Points, this.parameter.numberOfParticles.Value);
        }

        protected virtual void OnSetRenderMaterial(Material mat)
        {

        }
        #endregion
    }

}