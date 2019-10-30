using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Common;
using UnityTools.Rendering;

namespace UnityTools.Networking
{
    public class ProtoBufferReceiver : MonoBehaviour
    {
        [SerializeField] protected Texture2D currentTexture = null;
        [SerializeField] protected int fileQueueCount = 0;

        protected UDPTextureSocket socket = new UDPTextureSocket();
        protected long total = 0;
        protected long count = 0;
        // Start is called before the first frame update
        void Start()
        {
            this.socket.StartRecieve(12345);
        }

        // Update is called once per frame
        void Update()
        {
            this.fileQueueCount = this.socket.fileQueue.Count;

            if (this.currentTexture == null && this.socket.fileQueue.Count > 0)
            {
                ImageFile.FileData d;
                this.socket.fileQueue.TryPeek(out d);
                this.currentTexture = TextureManager.Create(d.Parameter.Width, d.Parameter.Height, TextureFormat.RGBA32, false);
            }

            if (this.currentTexture != null)
            {
                ImageFile.FileData d;
                if (this.socket.fileQueue.TryDequeue(out d))
                {
                    this.currentTexture.LoadRawTextureData(d.Data.ToByteArray());
                    this.currentTexture.Apply();
                }
            }
        }

        private void OnGUI()
        {
            if (this.currentTexture != null)
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), this.currentTexture, ScaleMode.ScaleToFit);
            }
        }
    }
}