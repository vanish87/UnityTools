using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace UnityTools.ComputeShaderTool
{

    //TODO: There is a better way to organize data and setup as SOA
    //See Fluid Engine Development P103 for this
    public class SOAData<T, S>
    {
        public struct ScalarData
        {
            public List<T> data;
        }
        public struct VectorData
        {
            public List<S> data;
        }

        public List<ScalarData> scalarData;
        public List<VectorData> vectorData;
    }

    public class SOADataContainer : ComputeShaderParameterContainer
    {
        public List<ComputeShaderParameterBuffer> scalarList;
        public List<ComputeShaderParameterBuffer> vectorList;

        public void InitBuffer()
        {
            foreach (var s in this.scalarList)
            {
                s.Value = new ComputeBuffer(100, 10);
            }
        }
    }

    public class SOAAlignedGPUData : SOAData<float, Vector4>
    {

    }

    public class SOABase
    {

    }
    public class GPUParticleSOA : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}