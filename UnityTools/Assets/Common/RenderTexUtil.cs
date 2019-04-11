using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Assertions;

namespace UnityTools.Common
{
    public class RenderTexUtil
    {
        #if DEBUG
        public class TextureTracking
        {
            protected List<RenderTexture> list = new List<RenderTexture>();

            public void Add(RenderTexture tex) => this.list.Add(tex);

            public void ReportTextures()
            {
                Debug.LogWarning("TextureTracking destructor");
            }
        }
        public static TextureTracking tracking = new TextureTracking(); 
        #endif
        static public void Rebuild(ref RenderTexture target, int width, int height, int depth = 0, RenderTextureFormat format = RenderTextureFormat.Default)
        {
            if (target != null)
            {
                target.Release();
                Object.Destroy(target);
                target = null;
            }
            target = Create(width, height, depth, format, false);
        }
        static public RenderTexture Create(int width, int height, int depth = 24, RenderTextureFormat format = RenderTextureFormat.ARGBFloat, bool randomWrite = false)
        {
            var target = new RenderTexture(width, height, depth, format);
            target.enableRandomWrite = randomWrite;
            target.Create();
            Assert.IsTrue(target.IsCreated());

            #if DEBUG
            tracking.Add(target);
            #endif

            return target;
        }

        static public bool ShouldRebuildTarget(RenderTexture src, RenderTexture target)
        {
            return src != null && (target == null || target.width != src.width || target.height != src.height);
        }

        static public void CheckAndRebuild(RenderTexture src, ref RenderTexture target)
        {
            if (ShouldRebuildTarget(src, target))
            {
                Rebuild(ref target, src.width, src.height, src.depth, src.format);
            }
        }

        static public void Clear(RenderTexture target)
        {
            RenderTexUtil.Clear(target, Color.clear);
        }
        static public void Clear(RenderTexture target, Color color)
        {
            if (target == null) return;
            var old = RenderTexture.active;
            RenderTexture.active = target;
            GL.Clear(true, true, color);
            RenderTexture.active = old;
        }

        static public void Destory(RenderTexture target)
        {
            if (target != null)
            {
                Object.Destroy(target);
                target = null;
            }
        }

        public static void savePng(RenderTexture rt, string path)
        {
            Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
            var old = RenderTexture.active;
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            Object.Destroy(tex);
            File.WriteAllBytes(Application.dataPath + $"{path}.png", bytes);
            RenderTexture.active = old;
        }
    }
}
