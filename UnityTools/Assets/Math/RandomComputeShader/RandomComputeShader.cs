using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityTools.Math
{
    public class RandomComputeShader : MonoBehaviour
    {
        Texture2D tex;

        [SerializeField] protected RenderTexture texture = null;
        [SerializeField] protected ComputeShader shader = null;
        protected ComputeBuffer buffer = null;
        protected int kernel = -1;
        // Use this for initialization
        void Start()
        {
            var desc = new RenderTextureDescriptor(512, 512, RenderTextureFormat.ARGBFloat);
            desc.enableRandomWrite = true;
            desc.sRGB = false;

            this.texture = new RenderTexture(desc);
            this.texture.Create();

            this.kernel = this.shader.FindKernel("Random");

            tex = new Texture2D(512, 512);
        }

        // Update is called once per frame
        void Update()
        {
            this.shader.SetTexture(this.kernel, "Result", this.texture);
            this.shader.Dispatch(this.kernel, 512 / 8, 512 / 8, 1);
        }

        protected void OnGUI()
        {
            GUI.DrawTexture(new Rect(10, 10, this.texture.width, this.texture.height), this.texture);
        }
    }
}