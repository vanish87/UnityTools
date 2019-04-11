using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ArrayStructInComputeShader : MonoBehaviour
{
    private const int MaxLights = 3;

    public class BasicEffectDirectionalLight
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

    Texture2D tex;

    [SerializeField] protected RenderTexture texture = null;
    [SerializeField] protected ComputeShader shader = null;
    protected ComputeBuffer buffer = null;
    protected int kernel = -1;
    // Use this for initialization
    void Start ()
    {
        this.texture = new RenderTexture(512, 512, 0);
        this.texture.enableRandomWrite = true;
        this.texture.Create();

        this.kernel = this.shader.FindKernel("Random");

        tex = new Texture2D(512, 512);
    }
	
	// Update is called once per frame
	void Update ()
    {
        this.shader.SetTexture(this.kernel, "Result", this.texture);
        this.shader.Dispatch(this.kernel, 512/8,512/8,1);
	}

    protected void OnGUI()
    {
        GUI.DrawTexture(new Rect(10, 10, this.texture.width, this.texture.height), this.texture);
    }
}
