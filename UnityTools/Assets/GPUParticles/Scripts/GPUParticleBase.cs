#define USE_APPEND_BUFFER

using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;
using UnityTools.Common;

public class AlignedGPUData
{
    //cbuffer data is packed in vector-4 size(16 bytes)
    //https://docs.microsoft.com/en-us/windows/desktop/direct3dhlsl/dx-graphics-hlsl-packing-rules
    //and it is better to align data as same as cbuffer did
    //https://developer.nvidia.com/content/understanding-structured-buffer-performance
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
}

/// <summary>
/// GPUParticleClassBase will be able to use subclass of AlignedGPUData for reuse of class field
/// But it does not have a CPU data because ComputeBuffer.SetData does not accept class array parameter
/// So use compute shader to init data, it also support non-blittable type(eg. bool)
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
public class GPUParticleBase<T> : MonoBehaviour
{
    public class GPUParticleBufferParameterContainer : ComputeShaderParameterContainer
    {

        public ComputeShaderParameterBuffer particlesDataBufferRead = new ComputeShaderParameterBuffer("_ParticlesDataBufferRead");
        public ComputeShaderParameterBuffer particlesDataBufferWrite = new ComputeShaderParameterBuffer("_ParticlesDataBufferWrite");

        #if USE_APPEND_BUFFER
        public ComputeShaderParameterBuffer particlesIndexBufferActive = new ComputeShaderParameterBuffer("_ParticlesIndexBufferActive");
        public ComputeShaderParameterBuffer particlesIndexBufferDead   = new ComputeShaderParameterBuffer("_ParticlesIndexBufferDead");
        #endif

        public int CurrentBufferLength { get { return this.currentBufferLength; } }
        protected int currentBufferLength = 0;


        public void InitBuffer(int bufferLength)
        {
            this.ReleaseBuffer();

            this.currentBufferLength = bufferLength;

            int dataSize = Marshal.SizeOf<T>();
            if(dataSize % 16 != 0)
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
            #endif

        }

        public void ReleaseBuffer()
        {
            var bufferList = this.VarList.Where(b =>
            {
                var buffer = b as ComputeShaderParameterBuffer;
                return buffer != null && buffer.Value != null;
            });

            bufferList?.ToList().ForEach(b => 
            {
                var buffer = (b as ComputeShaderParameterBuffer);
                //TODO Release called multiple time, is it safe?
                buffer.Release();
            });
        }
    }

    [SerializeField] protected ComputeShaderDispatcher dispather;
    [SerializeField] protected GPUParticleCBufferParameterContainer parameter = new GPUParticleCBufferParameterContainer();
    [SerializeField] protected ComputeShader cs;

    protected GPUParticleBufferParameterContainer bufferParameter = new GPUParticleBufferParameterContainer();

    protected virtual void OnCreateParticleData(int desiredNum)
    {
        this.bufferParameter.InitBuffer(desiredNum);
    }
    protected virtual void OnResetParticlesData()
    {

    }
    protected virtual void UpdateGPUDataBuffer()
    {
        #if USE_APPEND_BUFFER
        this.bufferParameter.particlesIndexBufferActive.Value.SetCounterValue(0);
        this.bufferParameter.particlesIndexBufferDead.Value.SetCounterValue(0);
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
    protected virtual void Start()
    {
        Assert.IsNotNull(this.cs);

        this.parameter.Bind(this.cs);
        this.bufferParameter.Bind(this.cs);

        this.dispather = new ComputeShaderDispatcher(this.cs);
        this.dispather.AddParameter("Force", this.parameter);
        this.dispather.AddParameter("Force", this.bufferParameter);

        this.ResizeBuffer(1024);
        this.ResizeBuffer(1024);
    }

    protected virtual void OnEnable()
    {
    }
    protected virtual void OnDisable()
    {
    }

    protected virtual void OnDestroy()
    {
        this.bufferParameter.ReleaseBuffer();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        //this.parameter.UpdateGPU();
        //this.bufferParameter.UpdateGPU("Force");
        this.dispather.Dispatch("Force", this.bufferParameter.CurrentBufferLength);
    }
    #endregion
}