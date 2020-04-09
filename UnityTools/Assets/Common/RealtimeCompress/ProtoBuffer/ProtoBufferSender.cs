using Google.Protobuf;
using UnityEngine;
using UnityTools.Common;
using UnityTools.Rendering;

namespace UnityTools.Networking
{
    public class ProtoBufferSender : AsyncGPUDataReader
    {
        // Start is called before the first frame update

        protected UDPTextureSocket sender = new UDPTextureSocket();
        [SerializeField] protected RenderTexture target;
        void Start()
        {
            var desc = new RenderTextureDescriptor(512, 512);
            desc.colorFormat = RenderTextureFormat.ARGB32;
            this.target = TextureManager.Create(desc);
            var camera = this.GetComponent<Camera>();
            camera.targetTexture = this.target;
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
            this.QueueTexture(target);
        }

        [System.Serializable]
        public class DataFile
        {
            [System.Serializable]
            public class Parameter
            {
                public int Width;
                public int Height;
            }

            public byte[] Data;
            public Parameter parameter;
        }

        protected override void OnSuccessed(FrameData frame)
        {
            var readback = frame.readback;

            var data = readback.GetData<byte>().ToArray();

            var fileData = new ImageFile.FileData();
            fileData.Parameter = new ImageFile.Parameter();
            fileData.Parameter.Width = readback.width;
            fileData.Parameter.Height = readback.height;
            fileData.Data = ByteString.CopyFrom(data);

            var cData = new DataFile();
            cData.parameter = new DataFile.Parameter();
            cData.parameter.Width = readback.width;
            cData.parameter.Height = readback.height;
            cData.Data = data;

            var cBuffer = Helper.ObjectToByteArray(cData);
            var len1 = cBuffer.Length;
            var len2 = fileData.ToByteArray().Length;
            Debug.LogFormat("Size compare c#:{0} protocol buffer:{1}, C#-Proto:{2}", len1, len2, len1-len2);

            var socketData = new SocketData("localhost", 12345);

            this.sender.Send(socketData, fileData);
        }
    }
}