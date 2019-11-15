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
        [SerializeField] protected Shader shader;
        [SerializeField] protected DisposableMaterial combineMat;
        [SerializeField] protected RenderTexture finalOutput;
        [SerializeField] protected List<TextureObject> textureList = new List<TextureObject>();

        protected virtual void OnEnable()
        {
            this.combineMat = new DisposableMaterial(this.shader);
        }

        protected virtual void OnDisable()
        {
            this.combineMat.Dispose();
            this.finalOutput.DestoryObj();
        }

        protected void AddTexture(Texture tex)
        {
            this.textureList.Add(new TextureObject() { texture = tex });
        }

        protected virtual void CleanUp()
        {
            //this is only a container
            //so it is not responsible for texture resource management
            this.textureList.Clear();
        }
        protected void CombineTextures()
        {
            this.finalOutput.Clear();

            Material mat = this.combineMat;
            foreach (var c in this.textureList)
            {
                mat.SetVector("_ST", c.st);
                Graphics.Blit(c.texture, this.finalOutput, this.combineMat, 0);
            }
        }

        protected virtual void RecalculateTextureParameter(TextureLayout layout = TextureLayout.Auto)
        {
            Assert.IsNotNull(this.textureList);
            Assert.IsTrue(this.textureList.Count > 0);

            var maxW = this.textureList.Max(c => c.texture.width);
            var maxH = this.textureList.Max(c => c.texture.height);
            //use max width or height as total width/height
            //another size of texture is sum up of width/height size
            var horizontal = layout == TextureLayout.Auto ?
                             maxW <= maxH :
                             layout == TextureLayout.Horizontal;

            var compacked = layout != TextureLayout.Auto;

            var newSize = new Vector2Int(0, 0);
            var currentSize = new Vector2Int(0, 0);
            
            var maxLineSize = (horizontal?maxW:maxH) * (compacked?this.textureList.Count:Mathf.CeilToInt(Mathf.Sqrt(this.textureList.Count)));
            

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

            this.finalOutput.DestoryObj();

            var desc = new RenderTextureDescriptor(newSize.x, newSize.y);
            this.finalOutput = TextureManager.Create(desc);
            this.finalOutput.name = "FinalOutput";
        }
    }
}