using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Rendering;

namespace UnityTools.Networking
{
    public class CameraComposite : TextureComposite
    {
        public List<TextureObject> TextureList { get => this.textureList; }

        protected void Start()
        {
            var sender = this.GetComponent<ProtoBufferSender>();
            var count = 0;
            var num = sender.Resolution / sender.CompositeRes;
            for (var i = 0; i < num; ++i)
            {
                for(var j = 0; j < num; ++j)
                {
                    var tex = TextureManager.Create(new RenderTextureDescriptor(sender.CompositeRes, sender.CompositeRes));
                    tex.name = count.ToString();
                    this.AddTexture(tex);

                    count++;
                }
            }

            this.RecalculateTextureParameter();

            this.finalOutput.DestoryObj();
            this.finalOutput = null;
        }
        protected void OnDestroy()
        {
            foreach(var tex in this.textureList)
            {
                tex.texture.DestoryObj();
            }
            this.CleanUp();
        }
        protected void Update()
        {
            if (this.finalOutput == null)
            {
                var cam = this.GetComponent<Camera>();
                this.finalOutput = cam.targetTexture;
            }

            this.SeperateTextures();
        }
    }
}