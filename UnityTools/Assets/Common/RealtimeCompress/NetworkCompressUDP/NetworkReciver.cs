using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Rendering;

namespace UnityTools.Common
{
    public class NetworkReciver : MonoBehaviour
    {
        [SerializeField] protected Texture2D currentTexture = null;
        [SerializeField] protected int fileQueueCount = 0;

        protected NetworkSender.TextureSocket socket = new NetworkSender.TextureSocket();


        protected void Start()
        {
            this.socket.StartRecieve(12345);
        }

        protected void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            this.fileQueueCount = this.socket.fileQueue.Count;

            if (this.currentTexture == null && this.socket.fileQueue.Count > 0)
            {
                AsyncGPUDataSerializer.FileData d;
                this.socket.fileQueue.TryPeek(out d);
                this.currentTexture = TextureManager.Create(d.parameter.x, d.parameter.y, TextureFormat.RGBA32, false);
            }

            if(this.currentTexture != null)
            {
                AsyncGPUDataSerializer.FileData d;
                if (this.socket.fileQueue.TryDequeue(out d))
                {
                    var data = CompressTool.Decompress(d.data);
                    this.currentTexture.LoadRawTextureData(data);
                    this.currentTexture.Apply();
                }
            }

            Graphics.Blit(this.currentTexture, destination);
        }
    }
}
