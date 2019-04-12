using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using System;

namespace UnityTools.Common
{
    public class ComputeShaderDispatcher
    {
        public class KernalInfo
        {
            public int kernal = -1;
            public Vector3Int kernalDimesion = Vector3Int.zero;
            public List<ComputeShaderParameterContainer> parameters = new List<ComputeShaderParameterContainer>();
        }

        protected ComputeShader cs = null;
        protected Dictionary<string, KernalInfo> kernal = new Dictionary<string, KernalInfo>();

        public ComputeShaderDispatcher(ComputeShader cs)
        {
            this.Bind(cs);
        }
        public void Bind(ComputeShader cs)
        {
            Assert.IsNotNull(cs);
            this.cs = cs;
            this.kernal.Clear();
        }
        public void AddParameter(string kernal, ComputeShaderParameterContainer parameter)
        {
            if (this.kernal.ContainsKey(kernal))
            {
                if (this.kernal[kernal].parameters.Contains(parameter) == false)
                {
                    this.kernal[kernal].parameters.Add(parameter);
                }
            }
            else
            {
                this.AddNewKernalInfo(kernal);
                this.kernal[kernal].parameters.Add(parameter);
            }
        }
        public void Dispatch(string kernal, int X = 0, int Y = 0, int Z = 0)
        {
            Assert.IsNotNull(kernal);
            Assert.IsNotNull(this.cs);
            if(this.kernal.ContainsKey(kernal) == false)
            {
                this.AddNewKernalInfo(kernal);
            }

            var kernalInfo = this.kernal[kernal];
            var threadNum = kernalInfo.kernalDimesion;

            this.UpdateParameter(kernal);
            this.cs.Dispatch(kernalInfo.kernal, this.GetDispatchSize(X, threadNum.x), this.GetDispatchSize(Y, threadNum.y), this.GetDispatchSize(Z, threadNum.z));
        }

        protected void AddNewKernalInfo(string kernal)
        {
            var kernalId = this.cs.FindKernel(kernal);
            Assert.IsTrue(kernalId >= 0);

            uint x = 0, y = 0, z = 0;
            this.cs.GetKernelThreadGroupSizes(kernalId, out x, out y, out z);
            this.kernal.Add(kernal, new KernalInfo()
            {
                kernal = kernalId,
                kernalDimesion = new Vector3Int((int)x, (int)y, (int)z),
                parameters = new List<ComputeShaderParameterContainer>(),
            });
        }
        protected void UpdateParameter(string kernal)
        {
            if(this.kernal.ContainsKey(kernal))
            {
                foreach(var p in this.kernal[kernal].parameters)
                {
                    p.UpdateGPU(kernal);
                }
            }
        }
        protected int GetDispatchSize(int desired, int threadNum)
        {
            if (desired == 0) return 1;

            return (desired + threadNum - 1) / threadNum;
        }
    }
}