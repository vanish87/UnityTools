using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityTools.Common;

namespace UnityTools.Rendering
{
    public class PostEffectBase : MonoBehaviour
    {
        [SerializeField] protected Shader shader;
        protected DisposableMaterial mat;

        protected virtual void OnEnable()
        {
            Assert.IsNotNull(this.shader);
            this.mat = new DisposableMaterial(this.shader);
        }
        protected virtual void OnDisable()
        {
            this.mat?.Dispose();
        }

        protected virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination, this.mat);
        }
    }

    public class PostEffectTool
    {
        public static void ApplyPostEffectTo(Texture2D source, Shader shader)
        {
            Assert.IsNotNull(source);
            Assert.IsNotNull(shader);

            var width = source.width;
            var height = source.height;

            using (var rt = new RenderTextureTool.RenderTextureTemp(source))
            {
                using (new RenderTextureTool.RenderTextureActive(rt))
                {
                    //apple post effect
                    var mat = new DisposableMaterial(shader);
                    Graphics.Blit(source, rt, mat);
                    mat.Dispose();

                    //read data back
                    source.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                    source.Apply();
                }
            }
        }
    }
}