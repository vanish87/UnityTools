using Google.Protobuf;
using UnityEngine;
using UnityEngine.Rendering;
using UnityTools.Common;
using UnityTools.Rendering;

namespace UnityTools.Networking
{
    public class ProtoBufferSender : AsyncGPUDataReader
    {
        public class TextureData: FrameData
        {
            public string id;
        }

        public int Resolution { get => this.resolution; }
        public int CompositeRes { get => this.compositeRes; }

        [SerializeField] protected int resolution = 2048;
        [SerializeField] protected int compositeRes = 512;
        protected UDPTextureSocket sender = new UDPTextureSocket();
        [SerializeField] protected RenderTexture target;
        [SerializeField] protected CameraComposite cameraComposite;
        protected void Start()
        {
            this.target = TextureManager.Create(new RenderTextureDescriptor(this.resolution, this.resolution, RenderTextureFormat.ARGB32, 0));
            var camera = this.GetComponent<Camera>();
            camera.targetTexture = this.target;
        }
        protected void OnDestroy()
        {
            this.target.DestoryObj();
        }

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();
            if (this.resolution > 512)
            {
                foreach (var tex in this.cameraComposite.TextureList)
                {
                    this.QueueFrame(new TextureData() { id = tex.texture.name, readback = AsyncGPUReadback.Request(tex.texture) });
                }
            }
            else
            {
                this.QueueFrame(new TextureData() { id = this.target.name, readback = AsyncGPUReadback.Request(this.target)});
            }
        }

        protected override void OnSuccessed(FrameData frame)
        {
            var tframe = frame as TextureData;
            var readback = tframe.readback;

            var data = readback.GetData<byte>().ToArray();

            var fileData = new ImageFile.FileData();
            fileData.Parameter = new ImageFile.Parameter();
            fileData.Data = ByteString.CopyFrom(data);

            fileData.Parameter.Width = readback.width;
            fileData.Parameter.Height = readback.height;
            fileData.Parameter.Id = tframe.id;

            var socketData = SocketData.Make("localhost", 12345);

            this.sender.Send(socketData, fileData);
        }
    }
}