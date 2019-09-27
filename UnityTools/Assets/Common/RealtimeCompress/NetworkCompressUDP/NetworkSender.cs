using Networking;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityTools.Common
{
    public class NetworkSender : AsyncGPUDataReader
    {

        public class TextureSocket : UDPSocket<AsyncGPUDataSerializer.FileData>
        {
            public ConcurrentQueue<AsyncGPUDataSerializer.FileData> fileQueue = new ConcurrentQueue<AsyncGPUDataSerializer.FileData>();
            public override void OnMessage(SocketData socket, AsyncGPUDataSerializer.FileData data)
            {
                //Task.Run(() => UDPServer.DebugReport(data));

                fileQueue.Enqueue(data);
            }
        }


        public TextureSocket socket = new TextureSocket();

        protected override void OnSuccessed(AsyncGPUReadbackRequest readback)
        {
            var data = readback.GetData<byte>().ToArray();
            data = CompressTool.Compress(data);
            var para = new AsyncGPUDataSerializer.Parameter() { x = readback.width, y = readback.height, compressed = true };
            var fileData = new AsyncGPUDataSerializer.FileData() { parameter = para, data = data };

            var socketData = new SocketData("localhost", 12345);
            this.socket.Send(socketData, fileData);
        }

        protected void OnRenderImage(RenderTexture source, RenderTexture destination)
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