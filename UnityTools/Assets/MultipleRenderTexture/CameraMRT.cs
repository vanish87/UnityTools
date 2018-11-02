using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityTools.Common;

namespace UnityTools.MultipleRenderTarget
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class CameraMRT : MonoBehaviour
    {
        private static readonly int NumberOfRenderTarget = 2;
        [SerializeField]private RenderTexture[] mrtTex = new RenderTexture[NumberOfRenderTarget];
        private RenderBuffer[] mrtRB = new RenderBuffer[NumberOfRenderTarget];

        private Camera targetCamera = null;

        // Use this for initialization
        void Start()
        {
            this.targetCamera = this.GetComponent<Camera>();

            var source = this.targetCamera.targetTexture;

            for (var i = 0; i < this.mrtTex.Length; ++i)
            {
                RenderTexUtil.CheckAndRebuild(source, ref this.mrtTex[i]);
                this.mrtRB[i] = mrtTex[i].colorBuffer;
            }

            this.targetCamera?.SetTargetBuffers(mrtRB, mrtTex[0].depthBuffer);
        }
        private void OnPreRender()
        {
            for (var i = 0; i < this.mrtTex.Length; ++i)
            {
                RenderTexUtil.Clear(this.mrtTex[i]);
            }

        }
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination);
            //Graphics.Blit(this.mrtTex[0], destination);
            //Graphics.Blit(this.mrtTex[1], destination);
        }
    }

}
