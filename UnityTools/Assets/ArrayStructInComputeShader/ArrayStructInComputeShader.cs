using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ArrayStructInComputeShader : MonoBehaviour
{
    private const int MaxLights = 3;

    public class BasicEffectDirectionalLight
    {

    }

    [StructLayout(LayoutKind.Explicit, Size = (12 * MaxLights))]
    internal struct BasicEffectLightConstants
    {
        [FieldOffset(0), MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxLights)]
        public Vector3[] DirectionalLights;

    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
