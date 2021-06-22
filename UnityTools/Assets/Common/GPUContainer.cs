using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityTools.GUITool;

namespace UnityTools.Common
{
    public interface IGPUContainer
    {
        void UpdateGPU(ComputeShader computeShader, string kernel = null);
        void UpdateGPU(Material material);
    }
    public class GPUContainer : GUIContainer, IGPUContainer
    {
        public void UpdateGPU(ComputeShader computeShader, string kernel = null)
        {
            foreach(var p in this.VariableList)
            {
                if(p is IGPUVariable gpu)
                {
                    gpu.SetToGPU(this, computeShader, kernel);
                }
            }
        }
        public void UpdateGPU(Material material)
        {
            foreach(var p in this.VariableList)
            {
                if(p is IGPUVariable gpu)
                {
                    gpu.SetToMaterial(this, material);
                }
            }
        }

    }

}