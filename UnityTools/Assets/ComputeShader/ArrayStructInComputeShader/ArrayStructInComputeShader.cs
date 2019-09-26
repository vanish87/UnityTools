using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ArrayStructInComputeShader : MonoBehaviour
{
    private const int MaxLights = 64;

    public struct BasicEffectDirectionalLight
    {
        Vector3 posistion;
        Vector3 velocity;
    }

    [StructLayout(LayoutKind.Explicit, Size = (12 * 2 * MaxLights))]
    internal struct BasicEffectLightConstants
    {
        [FieldOffset(0), MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxLights)]
        public BasicEffectDirectionalLight[] DirectionalLights;

    }

    [SerializeField] protected ComputeShader shader = null;
    protected ComputeBuffer buffer = null;
    protected int kernel = -1;
    // Use this for initialization
    void Start ()
    {
        this.kernel = this.shader.FindKernel("StructureArray");
        this.buffer = new ComputeBuffer(1, Marshal.SizeOf<BasicEffectLightConstants>());
    }
	
	// Update is called once per frame
	void Update ()
    {
        this.shader.Dispatch(this.kernel, MaxLights, 1, 1);

        var output = new BasicEffectLightConstants[1];
        output[0] = new BasicEffectLightConstants();
        this.buffer.GetData(output);
    }

}
