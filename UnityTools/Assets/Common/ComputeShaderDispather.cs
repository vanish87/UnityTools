using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine;
using System;
using Unity.Mathematics;

namespace UnityTools.ComputeShaderTool
{
    public class ComputeShaderDispatcher
    {
        public class KernelInfo
        {
            public int kernel = -1;
            public uint3 kernelDimesion = 0;
            public List<ComputeShaderParameterContainer> parameters = new List<ComputeShaderParameterContainer>();
        }

        protected ComputeShader cs = null;
        protected Dictionary<string, KernelInfo> kernel = new Dictionary<string, KernelInfo>();

        public ComputeShaderDispatcher(ComputeShader cs)
        {
            this.Bind(cs);
        }
        public void Bind(ComputeShader cs)
        {
            Assert.IsNotNull(cs);
            this.cs = cs;
            this.kernel.Clear();
        }
        public void ClearParameters()
        {
            foreach(var k in this.kernel)
            {
                k.Value.parameters.Clear();
            }
        }
        public void AddParameter(string kernel, ComputeShaderParameterContainer parameter)
        {
            if (this.kernel.ContainsKey(kernel))
            {
                if (this.kernel[kernel].parameters.Contains(parameter) == false)
                {
                    this.kernel[kernel].parameters.Add(parameter);
                }
            }
            else
            {
                this.AddNewKernelInfo(kernel);
                this.kernel[kernel].parameters.Add(parameter);
            }
        }
        public void Dispatch(string kernel, int X = 1, int Y = 1, int Z = 1)
        {
            Assert.IsNotNull(kernel);
            Assert.IsNotNull(this.cs);
            if(this.kernel.ContainsKey(kernel) == false)
            {
                this.AddNewKernelInfo(kernel);
            }

            var kernelInfo = this.kernel[kernel];
            var threadNum = kernelInfo.kernelDimesion;

            this.UpdateParameter(kernel);
            this.cs.Dispatch(kernelInfo.kernel, this.GetDispatchSize(X, threadNum.x), this.GetDispatchSize(Y, threadNum.y), this.GetDispatchSize(Z, threadNum.z));
        }

        protected void AddNewKernelInfo(string kernel)
        {
            var kernelId = this.cs.FindKernel(kernel);
            Assert.IsTrue(kernelId >= 0);

            uint x = 0, y = 0, z = 0;
            this.cs.GetKernelThreadGroupSizes(kernelId, out x, out y, out z);
            this.kernel.Add(kernel, new KernelInfo()
            {
                kernel = kernelId,
                kernelDimesion = new uint3(x, y, z),
                parameters = new List<ComputeShaderParameterContainer>(),
            });
        }
        protected void UpdateParameter(string kernel)
        {
            if(this.kernel.ContainsKey(kernel))
            {
                foreach(var p in this.kernel[kernel].parameters)
                {
                    p.UpdateGPU(this.cs, kernel);
                }
            }
        }
        protected int GetDispatchSize(int desired, uint threadNum)
        {
            Assert.IsTrue(desired > 0);
            Assert.IsTrue(threadNum > 0);

            return (int)((desired + threadNum - 1) / threadNum);
        }
    }
}