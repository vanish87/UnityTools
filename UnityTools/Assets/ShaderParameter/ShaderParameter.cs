using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Common;

public class ShaderParameter : MonoBehaviour
{
    public ComputeShader cs;
    [SerializeField] protected ComputeShaderParameterFloat floatTest = new ComputeShaderParameterFloat("_floatGPU");

    float refFloat;
    // Start is called before the first frame update
    void Start()
    {
        //floatTest = 

        floatTest.Bind(cs);

        var test = new ComputeShaderParameterFloat("_Test");

        test.Value = 10;

        test.Value = 12;

        var l = ComputeShaderParameterManager.Instance.nameList;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
