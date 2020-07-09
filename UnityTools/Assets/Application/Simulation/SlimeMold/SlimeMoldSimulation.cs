using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Mathematics;
using UnityTools.ComputeShaderTool;
using UnityTools.Rendering;
using UnityTools.Common;
using System;

namespace UnityTools.Applications.Simulation
{
    [StructLayout(LayoutKind.Sequential, Size = 28)]
    public class MoldParticleData : AlignedGPUData
    {
        public bool active;
        public float3 position;
        public float3 direction;
    }

    public class SlimeMoldSimulation : GPUParticleClassBase<MoldParticleData>
    {
        [Serializable]
        public class PtoTParameterContainer : ComputeShaderParameterContainer
        {
            public ComputeShaderParameterVector mousePos = new ComputeShaderParameterVector("_MousePos");
            public ComputeShaderParameterVector trailTextureSize = new ComputeShaderParameterVector("_TrailTextureSize");
            public ComputeShaderParameterMatrix worldToLocalMat = new ComputeShaderParameterMatrix("_WorldToLocalMat");

            public ComputeShaderParameterBuffer particleBuffer = new ComputeShaderParameterBuffer("_ParticlesDataBufferRead");

            public ComputeShaderParameterTexture trailTexture = new ComputeShaderParameterTexture("_Trail");
            public ComputeShaderParameterTexture diffuseTrail = new ComputeShaderParameterTexture("_DiffuseTrail");
            public ComputeShaderParameterTexture decayTrail = new ComputeShaderParameterTexture("_DecayTrailTex");

            public ComputeShaderParameterFloat dt = new ComputeShaderParameterFloat("_DT");
            public ComputeShaderParameterVector bound = new ComputeShaderParameterVector("_Bound");

            public ComputeShaderParameterFloat sensorAngle = new ComputeShaderParameterFloat("_SensorAngle", 30);
            public ComputeShaderParameterFloat sensorDistance = new ComputeShaderParameterFloat("_SensorDistance", 1);
            public ComputeShaderParameterFloat trunAngle = new ComputeShaderParameterFloat("_TrunAngle", 15);
            public ComputeShaderParameterFloat moveSpeed = new ComputeShaderParameterFloat("_MoveSpeed", 1);
            public ComputeShaderParameterFloat decaySpeed = new ComputeShaderParameterFloat("_DecaySpeed", 0.3f);
            public ComputeShaderParameterFloat depositSpeed = new ComputeShaderParameterFloat("_DepositSpeed", 0.01f);

        }
        [SerializeField] protected Area particleArea;
        [SerializeField] protected int pixelSize = 32;

        [SerializeField] protected PtoTParameterContainer PtoTparameter = new PtoTParameterContainer();
        [SerializeField] protected UnityBlur blur;

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            this.PtoTparameter.trailTexture.Value.DestoryObj();
            this.PtoTparameter.diffuseTrail.Value.DestoryObj();
            this.PtoTparameter.decayTrail.Value.DestoryObj();
        }

        protected override void OnResetParticlesData()
        {
            base.OnResetParticlesData();

            var bound = this.particleArea.Bound;
            var size = new Vector2Int(Mathf.CeilToInt(bound.size.x * this.pixelSize), Mathf.CeilToInt(bound.size.y * this.pixelSize));

            this.PtoTparameter.trailTextureSize.Value = new Vector4(size.x, size.y);

            var desc = new RenderTextureDescriptor(size.x, size.y, RenderTextureFormat.RFloat);
            desc.enableRandomWrite = true;
            this.PtoTparameter.trailTexture.Value = TextureManager.Create(desc);
            this.PtoTparameter.diffuseTrail.Value = TextureManager.Create(desc);
            this.PtoTparameter.decayTrail.Value = TextureManager.Create(desc);

            this.dispather.AddParameter("MapParticleToTrail", this.PtoTparameter);
            this.dispather.AddParameter("Emit", this.PtoTparameter);
            this.dispather.AddParameter("DecayTrail", this.PtoTparameter);
            this.dispather.AddParameter("Integration", this.PtoTparameter);

            this.PtoTparameter.particleBuffer.Value = this.bufferParameter.particlesDataBufferRead.Value;
            this.PtoTparameter.worldToLocalMat.Value = this.particleArea.WorldToLocalMatrix;
        }

        protected override void Update()
        {
            if (Input.GetMouseButton(0))
            {
                var pos = new float2(Input.mousePosition.x, Input.mousePosition.y) / new float2(Screen.width, Screen.height);
                var posWorld = this.particleArea.LocalToWordPosition(new Vector3(pos.x, pos.y, 0));
                this.PtoTparameter.mousePos.Value = new Vector4(posWorld.x, posWorld.y, 0, 0);

                this.Emit(512);
            }

            this.PtoTparameter.dt.Value = Time.deltaTime;

            base.Update();
            ComputeShaderParameterBuffer.SwapBuffer(this.bufferParameter.particlesDataBufferRead, this.bufferParameter.particlesDataBufferWrite);
            this.PtoTparameter.particleBuffer.Value = this.bufferParameter.particlesDataBufferRead.Value;
            this.bufferParameter.particlesDataBufferEmitWrite.Value = this.bufferParameter.particlesDataBufferRead.Value;

            (this.PtoTparameter.trailTexture.Value as RenderTexture).Clear();
            (this.PtoTparameter.diffuseTrail.Value as RenderTexture).Clear();

            //Deposit
            this.MapParticleToTrail();
            //Diffuse
            this.BlurTrail();
            //Decay
            this.DecayTrail();
        }

        protected void OnDrawGizmos()
        {
            this.particleArea.OnDrawGizmo();
        }
        protected void OnGUI()
        {
            var offset = 0;
            GUI.DrawTexture(new Rect(offset, 0, 256, 256), this.PtoTparameter.trailTexture.Value, ScaleMode.ScaleToFit, false);
            offset += 256;
            GUI.DrawTexture(new Rect(offset, 0, 256, 256), this.PtoTparameter.diffuseTrail.Value, ScaleMode.ScaleToFit, false);
            offset += 256;
            GUI.DrawTexture(new Rect(offset, 0, 256, 256), this.PtoTparameter.decayTrail.Value, ScaleMode.ScaleToFit, false);
        }

        protected void MapParticleToTrail()
        {
            this.dispather.Dispatch("MapParticleToTrail", this.parameter.numberOfParticles.Value);
        }

        protected void BlurTrail()
        {
            this.blur.BlurTexture(this.PtoTparameter.trailTexture.Value as RenderTexture, this.PtoTparameter.diffuseTrail.Value as RenderTexture);
        }
        protected void DecayTrail()
        {
            var size = this.PtoTparameter.trailTextureSize.Value;
            var dsize = new int2(Mathf.CeilToInt(size.x), Mathf.CeilToInt(size.y));
            this.dispather.Dispatch("DecayTrail", dsize.x, dsize.y);
        }
    }
}
