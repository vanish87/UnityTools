using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityTools.Common;

namespace UnityTools.Rendering
{
    public class TextureComposite : MonoBehaviour
    {
        [Serializable]
        public class TextureObject
        {
            public Texture texture;
            public Vector4 st;
        }
        public enum TextureLayout
        {
            Auto,
            Horizontal,
            Vertical,
        }
        public RenderTexture FinalOutput => this.finalOutput;
        public int TextureCount => this.textureList.Count;
        [SerializeField] protected Shader shader;
        [SerializeField] protected DisposableMaterial combineMat;
        [SerializeField] protected RenderTexture finalOutput;
        [SerializeField] protected List<TextureObject> textureList = new List<TextureObject>();


        public virtual void Clear()
        {
            foreach(var t in this.textureList)
            {
                t.texture?.DestoryObj();
            }
            this.finalOutput?.DestoryObj();
            this.CleanUp();
        }

        protected virtual void OnEnable()
        {
            this.combineMat = new DisposableMaterial(this.shader);
        }

        protected virtual void OnDisable()
        {
            this.combineMat?.Dispose();
            this.finalOutput?.DestoryObj();
        }

        public void AddTexture(Texture tex)
        {
            this.textureList.Add(new TextureObject() { texture = tex });
        }

        protected virtual void CleanUp()
        {
            //this is only a container
            //so it is not responsible for texture resource management
            this.textureList.Clear();
        }
        public void CombineTextures()
        {
            if(this.finalOutput == null) return;

            this.finalOutput.Clear();

            Material mat = this.combineMat;
            foreach (var c in this.textureList)
            {
                mat.SetVector("_ST", c.st);
                Graphics.Blit(c.texture, this.finalOutput, this.combineMat, 0);
            }
        }
        public void SeparateTextures()
        {
            if(this.finalOutput == null) return;
            
            Material mat = this.combineMat;
            foreach (var c in this.textureList)
            {
                mat.SetVector("_ST", c.st);
                var rt = c.texture as RenderTexture;
                if (rt == null) continue;
                rt.Clear();
                Graphics.Blit(this.finalOutput, rt, this.combineMat, 2);
            }
        }

        public virtual void RecalculateTextureParameter(TextureLayout layout = TextureLayout.Auto, int maxLineCount = -1)
        {
            if(this.textureList.Count == 0)
            {
                //create a dummy texture
                this.CreateFinalTexture(new Vector2Int(4, 4));
                return;
            }
            Assert.IsNotNull(this.textureList);
            Assert.IsTrue(this.textureList.Count > 0);

            var maxW = this.textureList.Max(c => c.texture.width);
            var maxH = this.textureList.Max(c => c.texture.height);
            //use max width or height as total width/height
            //another size of texture is sum up of width/height size
            var horizontal = layout == TextureLayout.Auto ?
                             maxH <= maxW :
                             layout == TextureLayout.Horizontal;

            var compacked = layout != TextureLayout.Auto;

            var newSize = new Vector2Int(0, 0);
            var currentSize = new Vector2Int(0, 0);
            
            maxLineCount = maxLineCount != -1? maxLineCount:Mathf.CeilToInt(Mathf.Sqrt(this.textureList.Count));
            var maxLineSize = (horizontal?maxW:maxH) * (compacked?this.textureList.Count:maxLineCount);

            foreach (var t in textureList)
            {
                var offSetX = compacked ? t.texture.width : maxW;
                var offSetY = compacked ? t.texture.height : maxH;
                if (horizontal)
                {
                    currentSize.x += offSetX;
                    if (currentSize.x > maxLineSize)
                    {
                        currentSize.x = offSetX;
                        currentSize.y += offSetY;
                    }
                }
                else
                {
                    currentSize.y += offSetY;
                    if (currentSize.y > maxLineSize)
                    {
                        currentSize.y = offSetY;
                        currentSize.x += offSetX;
                    }
                }
            }

            if (horizontal)
            {
                if(compacked)
                {
                    newSize.x = currentSize.x;
                }
                else
                {
                    bool newLine = currentSize.y >= maxH;
                    newSize.x = newLine ? maxLineSize : currentSize.x;
                }
                newSize.y = currentSize.y + maxH;
            }
            else
            {
                if (compacked)
                {
                    newSize.y = currentSize.y;
                }
                else
                {
                    bool newLine = currentSize.x >= maxW;
                    newSize.y = newLine ? maxLineSize : currentSize.y;
                }
                newSize.x = currentSize.x + maxW;
            }
            //xy is start uv coordinates
            //zw is texture size in uv space
            var currentST = new Vector4(0, 0, 1, 1);
            foreach (var t in textureList)
            {
                var sizeX = t.texture.width * 1f / newSize.x;
                var sizeY = t.texture.height * 1f / newSize.y;

                currentST.z = sizeX;
                currentST.w = sizeY;

                t.st = currentST;

                var offSetX = compacked ? sizeX : (maxW * 1.0f / newSize.x);
                var offSetY = compacked ? sizeY : (maxH * 1.0f / newSize.y);

                if (horizontal)
                {
                    currentST.x += offSetX;

                    if (currentST.x >= maxLineSize * 1.0f / newSize.x)
                    {
                        currentST.x = 0;
                        currentST.y += offSetY;
                    }
                }
                else
                {
                    currentST.y += offSetY;
                    if (currentST.y >= maxLineSize * 1.0f / newSize.y)
                    {
                        currentST.y = 0;
                        currentST.x += offSetX;
                    }
                }
            }

            this.CreateFinalTexture(newSize, this.textureList.FirstOrDefault()?.texture);
        }

        public void UpdateST(List<Vector4> st, Vector2Int newSize)
        {
            foreach(var i in Enumerable.Range(0, st.Count))
            {
                this.textureList[i].st = st[i];
            }

            this.CreateFinalTexture(newSize, this.textureList.FirstOrDefault()?.texture);
        }

        protected void CreateFinalTexture(Vector2Int size, Texture source = null)
        {
            this.finalOutput.DestoryObj();

            var desc = new RenderTextureDescriptor(size.x, size.y);
            desc.enableRandomWrite = true;
            if (source is RenderTexture src)
            {
                desc.colorFormat = src.format;
                desc.enableRandomWrite = src.enableRandomWrite;
                desc.sRGB = src.sRGB;
            }
            this.finalOutput = TextureManager.Create(desc);
            this.finalOutput.name = "FinalOutput";
        }
    }
}