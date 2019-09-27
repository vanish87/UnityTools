using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Common
{
    public class SerializerExample : AsyncGPUDataSerializer
    {
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination);

            var temp = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
            {
                Graphics.Blit(source, temp);
                this.QueueFrame(temp);
            }
            RenderTexture.ReleaseTemporary(temp);
        }
    }
}