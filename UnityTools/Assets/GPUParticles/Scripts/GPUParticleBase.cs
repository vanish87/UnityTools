#define USE_APPEND_BUFFER

using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;
using UnityTools.Common;

[StructLayout(LayoutKind.Sequential, Size = 32)]
public class AlignedGPUData
{

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

public class GPUParticleBase<T> : MonoBehaviour where T : AlignedGPUData
{
    public class GPUParticleBufferParameterContainer : ComputeShaderParameterContainer
    {

        public ComputeShaderParameterBuffer particlesDataBufferRead = new ComputeShaderParameterBuffer("_ParticlesDataBufferRead");
        public ComputeShaderParameterBuffer particlesDataBufferWrite = new ComputeShaderParameterBuffer("_ParticlesDataBufferWrite");

        #if USE_APPEND_BUFFER
        public ComputeShaderParameterBuffer particlesIndexBufferActive = new ComputeShaderParameterBuffer("_ParticlesIndexBufferActive");
        public ComputeShaderParameterBuffer particlesIndexBufferDead   = new ComputeShaderParameterBuffer("_ParticlesIndexBufferDead");
        #endif


        public void InitBuffer(int bufferLength)
        {
            this.ReleaseBuffer();

            int dataSize = Marshal.SizeOf<T>();
            this.particlesDataBufferRead.Value = new ComputeBuffer(bufferLength, dataSize);
            this.particlesDataBufferWrite.Value = new ComputeBuffer(bufferLength, dataSize);


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

            bufferList.ToList().ForEach(b => 
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
    protected T[] CPUData = null;

    protected virtual void OnResetParticlesData()
    {
        foreach(var boid in this.CPUData)
        {
            
        }
    }
    protected virtual void OnUpdateParameter(string kernalName)
    {
        
    }
    protected void UpdateGPUDataBuffer()
    {        
        this.bufferParameter.particlesDataBufferRead.Value.SetData(this.CPUData);
        this.bufferParameter.particlesDataBufferWrite.Value.SetData(this.CPUData);

        #if USE_APPEND_BUFFER
        this.bufferParameter.particlesIndexBufferActive.Value.SetCounterValue(0);
        this.bufferParameter.particlesIndexBufferDead.Value.SetCounterValue(0);
        #endif
    }
    protected void ResizeBuffer(int desiredNum)
    {
        Assert.IsTrue(desiredNum > 0);
        this.bufferParameter.InitBuffer(desiredNum);
        this.CPUData = new T[desiredNum];

        this.OnResetParticlesData();
        this.UpdateGPUDataBuffer();

        this.dispather.AddParameter("Force", this.parameter);
        this.dispather.AddParameter("Force", this.bufferParameter);
    }

    #region MonoBehaviour
    // Start is called before the first frame update
    protected void Start()
    {
        Assert.IsNotNull(this.cs);

        this.parameter.Bind(this.cs);
        this.dispather = new ComputeShaderDispatcher(this.cs);

        this.ResizeBuffer(1024);
        this.ResizeBuffer(1024);
    }

    protected void OnEnable()
    {
    }
    protected void OnDisable()
    {
    }

    protected void OnDestroy()
    {
        this.bufferParameter.ReleaseBuffer();
    }

    // Update is called once per frame
    protected void Update()
    {
        //this.parameter.UpdateGPU();
        //this.bufferParameter.UpdateGPU("Force");
        this.dispather.Dispatch("Force", this.parameter.numberOfParticles.Value);
    }
    #endregion
}