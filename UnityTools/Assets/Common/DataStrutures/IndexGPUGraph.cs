using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace UnityTools.Common
{
    public class IndexGPUGraph : GPUGraph<IndexGPUGraph.Node, IndexGPUGraph.Edge, IndexGPUGraph.Kernel>
    {
        public enum Kernel
        {

        }

        [StructLayout(LayoutKind.Sequential)]
        public class Node : NodeBase
        {
            float3 pos;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class Edge : EdgeBase
        {

        }

        public class IndexGPUGraphData: GPUContainer
        {

        }

        protected override void Init()
        {
            base.Init();

        }
    }
}