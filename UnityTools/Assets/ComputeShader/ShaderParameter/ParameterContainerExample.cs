// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityTools.ComputeShaderTool;

// [System.Serializable]
// public class GPUParameterContainer : ComputeShaderParameterFileContainer
// {
//     public ComputeShaderParameterFloat floatTest = new ComputeShaderParameterFloat("_FloatValue");
//     public ComputeShaderParameterMatrix matrixTest = new ComputeShaderParameterMatrix("_MatrixValue");
//     public ComputeShaderParameterVector vectorTest = new ComputeShaderParameterVector("_VectorValue");
//     public ComputeShaderParameterBuffer bufferTest = new ComputeShaderParameterBuffer("_BufferValue");

//     public GPUParameterContainer() : base() { }
//     public GPUParameterContainer(ComputeShader cs) : base("test.txt", cs)
//     {
//     }
// }

// public class ParameterContainerExample : MonoBehaviour
// {
//     public ComputeShader cs;

//     [SerializeField] protected GPUParameterContainer container = new GPUParameterContainer();
//     // Start is called before the first frame update
//     void Start()
//     {
//         //this.container = new GPUParameterContainer(this.cs);
//         this.container.floatTest.Value = 123;
//         //this.container.SaveFile("test.txt");
//         this.container.floatTest.Value = 321;
//         this.container.vectorTest.Value = Vector4.one;
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         // this.container.();
//     }
// }