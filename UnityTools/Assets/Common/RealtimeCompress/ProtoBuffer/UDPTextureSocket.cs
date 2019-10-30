using Google.Protobuf;
using Networking;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityTools.Common;

namespace UnityTools.Networking
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
            return newData;
        }

        public override Imgfile.FileData OnDeserialize(byte[] data, int length)
        {
            var dataValid = data.SubArray(0, length);
            var newData = CompressTool.Decompress(dataValid, CompressTool.CompreeAlgorithm.Zstd);
            return Imgfile.FileData.Parser.ParseFrom(newData);
        }
    }
}