using Google.Protobuf;
using System.Collections.Concurrent;
using UnityTools.Common;

namespace UnityTools.Networking
{
    public class UDPTextureSocket : UDPSocket<ImageFile.FileData>
    {
        public ConcurrentQueue<ImageFile.FileData> fileQueue = new ConcurrentQueue<ImageFile.FileData>();
        public override void OnMessage(SocketData socket, ImageFile.FileData data)
        {
            this.fileQueue.Enqueue(data);
        }

        public override byte[] OnSerialize(ImageFile.FileData data)
        {
            var rawData = data.ToByteArray();
            var newData = CompressTool.Compress(rawData, CompressTool.CompreeAlgorithm.Zstd);
            UnityEngine.Debug.LogFormat("Raw data size {0} KB, Compressed data size {1}", 
                rawData.Length/1024, newData.Length > 1024?newData.Length/1024+"KB": newData.Length + "B");
            return newData;
        }

        public override ImageFile.FileData OnDeserialize(byte[] data, int length)
        {
            var dataValid = data.SubArray(0, length);
            var newData = CompressTool.Decompress(dataValid, CompressTool.CompreeAlgorithm.Zstd);
            return ImageFile.FileData.Parser.ParseFrom(newData);
        }
    }
}