using Google.Protobuf;
using Networking;
using UnityEngine;
using UnityTools.Common;
using UnityTools.Rendering;

namespace UnityTools.Networking
{
    public class ProtoBufferSender : AsyncGPUDataReader
    {
        // Start is called before the first frame update

        protected UDPTextureSocket sender = new UDPTextureSocket();
        protected Camera camera;
        [SerializeField] protected RenderTexture target;
        void Start()
        {
            this.target = TextureManager.Create(new RenderTextureDescriptor(256, 256));
            this.camera = this.GetComponent<Camera>();
            this.camera.targetTexture = this.target;
        }

        int count = 0;
        // Update is called once per frame
        protected override void Update()
        {
            base.Update();

            //if (count++ < 1)
            {
                var temp = RenderTexture.GetTemporary(target.width, target.height, 0, RenderTextureFormat.ARGB32);
                {
                    Graphics.Blit(target, temp);
                    this.QueueTexture(temp);
                }
                RenderTexture.ReleaseTemporary(temp);
            }
        }

        protected override void OnSuccessed(FrameData frame)
        {
            var readback = frame.readback;

            var data = readback.GetData<byte>().ToArray();

            var fileData = new Imgfile.FileData();
            fileData.Parameter = new Imgfile.Parameter();
            fileData.Parameter.Width = readback.width;
            fileData.Parameter.Height = readback.height;
            fileData.Data = ByteString.CopyFrom(data);

            var socketData = new SocketData("localhost", 12345);

            this.sender.Send(socketData, fileData);
        }
    }
}