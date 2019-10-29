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


        protected long total = 0;
        protected long count = 0;

        protected void Start()
        {
            this.socket.StartRecieve(22345);
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
                    var timer = System.Diagnostics.Stopwatch.StartNew();
                    var data = CompressTool.Decompress(d.data, CompressTool.CompreeAlgorithm.Zstd);
                    timer.Stop();
                    total += timer.ElapsedMilliseconds;
                    count++;
                    this.currentTexture.LoadRawTextureData(data);
                    this.currentTexture.Apply();

                    Debug.Log("res is " + d.parameter.x + " " + d.parameter.y);
                }
            }

            Graphics.Blit(this.currentTexture, destination);

            if (count > 0)
            {
                var avgTime = total * 1.0d / count / 1000;
                Debug.LogFormat("Average decompress time {0}, fps is {1}", avgTime, 1/avgTime);
            }
        }
    }
}
