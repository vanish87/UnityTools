using Google.Protobuf;
using Networking;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Common;

namespace TeamLab.Bubble
{
    public class UDPTextureSocket : UDPSocket<Imgfile.FileData>
    {
        public ConcurrentQueue<Imgfile.FileData> fileQueue = new ConcurrentQueue<Imgfile.FileData>();
        public override void OnMessage(SocketData socket, Imgfile.FileData data)
        {
            this.fileQueue.Enqueue(data);
        }

        public override byte[] OnSerialize(Imgfile.FileData data)
        {
            var rawData = data.ToByteArray();
            var newData = CompressTool.Compress(rawData, CompressTool.CompreeAlgorithm.Zstd);
            //Debug.Log("Data size " + newData.Length);
            return newData;
        }

        public override Imgfile.FileData OnDeserialize(byte[] data)
        {
            var newData = CompressTool.Decompress(data, CompressTool.CompreeAlgorithm.Zstd);
            return Imgfile.FileData.Parser.ParseFrom(newData);
        }
    }
}