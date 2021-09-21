using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityTools.Debuging;
using UnityTools.Rendering;

namespace UnityTools.Common
{
    public class PalletTexture : MonoBehaviour
    {
        public Texture Tex => this.texture;

        [SerializeField] protected List<Gradient> color = new List<Gradient>();
        [SerializeField] protected Texture texture;

        static public Texture2D GenerateGradientTexture(List<Gradient> from, int size = 128)
        {
            var w = size;
            var h = from.Count;

			var texture = TextureManager.Create(w, h, TextureFormat.RGBA32, false, true);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            
            UpdateGradientTexture(texture, from);

            return texture;
        }

        static public void UpdateGradientTexture(Texture2D tex, List<Gradient> from, int size = 128)
        {
            var w = size;
            var h = from.Count;

            LogTool.AssertNotNull(tex);
            LogTool.AssertIsTrue(w == tex.width);
            LogTool.AssertIsTrue(h == tex.height);

            foreach(var y in Enumerable.Range(0, h))
            {
                foreach(var x in Enumerable.Range(0, w))
                {
                    var key = x * 1f / (w-1);
                    var color = from[y].Evaluate(key);
                    tex.SetPixel(x, y, color);
                }
            }

            tex.Apply();
        }

        protected void OnEnable()
        {
            this.texture?.DestoryObj();
            this.texture = GenerateGradientTexture(this.color);
        }

        protected void OnDisable()
        {
            this.texture?.DestoryObj();
        }
    }
}
