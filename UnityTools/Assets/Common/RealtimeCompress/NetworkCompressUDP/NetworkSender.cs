using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityTools.Networking;

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


        protected long total = 0;
        protected long count = 0;

        public TextureSocket socket = new TextureSocket();
        public SocketData socketData = new SocketData("localhost", 12345);

        protected override void OnSuccessed(FrameData frame)
        {
            var readback = frame.readback;

            var data = readback.GetData<byte>().ToArray();
            var timer = System.Diagnostics.Stopwatch.StartNew();
            data = CompressTool.Compress(data, CompressTool.CompreeAlgorithm.Zstd); timer.Stop();
            total += timer.ElapsedMilliseconds;
            count++;
            var para = new AsyncGPUDataSerializer.Parameter() { x = readback.width, y = readback.height, compressed = true };
            var fileData = new AsyncGPUDataSerializer.FileData() { parameter = para, data = data };

            Debug.Log("Data size " + data.Length);
            this.socket.Send(socketData, fileData);
        }

        protected void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination);

            var temp = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
            {
                Graphics.Blit(source, temp);
                this.QueueTexture(temp);
            }
            RenderTexture.ReleaseTemporary(temp);

            if (count > 0)
            {
                var avgTime = total * 1.0d / count / 1000;
                Debug.LogFormat("Average compress time {0}, fps is {1}", avgTime, 1 / avgTime);
            }
        }
    }
}