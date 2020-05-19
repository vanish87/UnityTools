using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Common;
using UnityTools.Rendering;

namespace UnityTools.Networking
{
    public class ProtoBufferReceiver : TextureComposite
    {
        protected Dictionary<string, Texture2D> recievedTextures = new Dictionary<string, Texture2D>();

        [SerializeField] protected int fileQueueCount = 0;

        protected UDPTextureSocket socket = new UDPTextureSocket();
        protected long total = 0;
        protected long count = 0;
        // Start is called before the first frame update
        protected void Start()
        {
            this.socket.StartRecieve(12345);
        }
        protected void OnDestroy()
        {
            foreach(var tex in this.textureList)
            {
                tex.texture.DestoryObj();
            }

            this.CleanUp();
        }

        // Update is called once per frame
        protected void Update()
        {
            this.fileQueueCount = this.socket.fileQueue.Count;

            var count = 16;
            while(this.socket.fileQueue.Count > 0 && count-->0)
            {
                ImageFile.FileData d;
                if (this.socket.fileQueue.TryDequeue(out d))
                {
                    Texture2D tex = default;

                    if(this.recievedTextures.ContainsKey(d.Parameter.Id))
                    {
                        tex = this.recievedTextures[d.Parameter.Id];
                    }
                    else
                    {
                        tex = TextureManager.Create(d.Parameter.Width, d.Parameter.Height, TextureFormat.RGBA32, false);
                        this.recievedTextures.Add(d.Parameter.Id, tex);

                        this.AddTexture(tex);
                        this.RecalculateTextureParameter();
                    }

                    tex.LoadRawTextureData(d.Data.ToByteArray());
                    tex.Apply();
                }
            }

            this.CombineTextures();
        }

        private void OnGUI()
        {
            if (this.finalOutput != null)
            {
                GUI.DrawTexture(new Rect(50, 50, Screen.width-100, Screen.height-100), this.finalOutput, ScaleMode.ScaleToFit);
            }
        }
    }
}