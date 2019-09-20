using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityTools.Rendering;

namespace UnityTools.LightCamera
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class CameraRender : MonoBehaviour
    {
        private static readonly int NumberOfRenderTarget = 2;
        [SerializeField] private RenderTexture[] mrtTex = new RenderTexture[NumberOfRenderTarget];
        private RenderBuffer[] mrtRB = new RenderBuffer[NumberOfRenderTarget];


        private Camera targetCamera = null;
        private Shader lightShader = null;

        private void Start()
        {
            this.lightShader = Shader.Find("Custom/RenderToGbuffer");
            Assert.IsNotNull(this.lightShader);

            this.targetCamera = this.GetComponent<Camera>();
            Assert.IsNotNull(this.targetCamera);

            var source = this.targetCamera.targetTexture;

            for (var i = 0; i < this.mrtTex.Length; ++i)
            {
                source.MatchSource(ref this.mrtTex[i]);
                this.mrtRB[i] = mrtTex[i].colorBuffer;
            }

            this.targetCamera?.SetTargetBuffers(mrtRB, mrtTex[0].depthBuffer);
        }

        private void OnPreRender()
        {
            foreach(var t in this.mrtTex)
            {
                t.Clear();
            }

        }
        private void Update()
        {
            this.targetCamera.RenderWithShader(this.lightShader, null);
        }
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination);
            //Graphics.Blit(this.mrtTex[0], destination);
            //Graphics.Blit(this.mrtTex[1], destination);
        }
    }

}

