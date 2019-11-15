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
            foreach (var t in this.textureList)
            {
                t.texture.DestoryObj();
            }

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

        protected void RecalculateTextureParameter(TextureLayout layout = TextureLayout.Auto)
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

            var newSize = new Vector2Int(0, 0);
            var currentSize = new Vector2Int(maxW, maxH);

            var maxLineSize = (horizontal ? maxW : maxH) * Mathf.CeilToInt(Mathf.Sqrt(this.textureList.Count));

            foreach (var t in textureList)
            {
                if (horizontal)
                {
                    currentSize.x += maxW;
                    if (currentSize.x > maxLineSize)
                    {
                        currentSize.x = maxW;
                        currentSize.y += maxH;
                    }
                }
                else
                {
                    currentSize.y += maxH;
                    if (currentSize.y > maxLineSize)
                    {
                        currentSize.y = maxH;
                        currentSize.x += maxW;
                    }
                }
            }

            newSize = currentSize;

            if (currentSize.y > maxH)
            {
                if (horizontal)
                {
                    newSize.x = maxLineSize;
                }
                else
                {
                    newSize.y = maxLineSize;
                }
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

                if (horizontal)
                {
                    currentST.x += sizeX;

                    if (currentST.x >= maxLineSize / newSize.x)
                    {
                        currentST.x = 0;
                        currentST.y += maxH * 1.0f / newSize.y;
                    }
                }
                else
                {
                    currentST.y += sizeY;
                    if (currentST.y >= maxLineSize / newSize.y)
                    {
                        currentST.y = 0;
                        currentST.x += maxW * 1.0f / newSize.x;
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