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