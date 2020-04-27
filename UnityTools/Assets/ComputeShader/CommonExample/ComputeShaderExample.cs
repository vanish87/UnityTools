using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityTools.Rendering;

namespace UnityTools.Example
{
    public class ComputeShaderExample : MonoBehaviour
    {
        [SerializeField] protected Texture input;
        [SerializeField] protected Texture output;
        [SerializeField] protected ComputeShader cs;
        [SerializeField] protected int[] data;
        [SerializeField] protected ComputeBuffer buffer;

        [SerializeField] protected int size = 32;

        protected int kernel = -1;
        // Start is called before the first frame update
        protected void Start()
        {
            this.input = TextureManager.Create(size, 1, TextureFormat.R16, false, true);

            var desc = new RenderTextureDescriptor(size, 1);
            desc.colorFormat = RenderTextureFormat.R16;
            desc.enableRandomWrite = true;
            this.output = TextureManager.Create(desc);

            this.kernel = this.cs.FindKernel("Emit");

            this.data = new int[size];
            this.buffer = new ComputeBuffer(size, Marshal.SizeOf<int>());

            this.buffer.SetData(this.data);
        }

        protected void OnDestroy()
        {
            this.input.DestoryObj();
            this.output.DestoryObj();
            this.buffer.Release();
        }

        // Update is called once per frame
        void Update()
        {
            this.cs.SetTexture(this.kernel, "_Input", this.input);
            this.cs.SetTexture(this.kernel, "_Output", this.output);
            this.cs.SetBuffer(this.kernel, "_Debug", this.buffer);

            this.cs.Dispatch(this.kernel, size, 1, 1);

            this.buffer.GetData(this.data);
            foreach( var d in this.data)
            {
                Debug.Log(d);
            }
        }
    }
}