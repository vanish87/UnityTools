using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Assertions;
using UnityTools.Common;

namespace UnityTools.Rendering
{
    public static class RenderTextureTool
    {
        public class RenderTextureScope : Scope
        {
            protected RenderTexture old;

            public RenderTextureScope(RenderTexture next)
                :base()
            {
                this.old = RenderTexture.active;
                RenderTexture.active = next;
            }
            protected override void DisposeManaged()
            {
                base.DisposeManaged();
                RenderTexture.active = this.old;
            }
        }        

       

        public class TextureMatcher<T>
        {
            protected T source;
            protected T target;
            protected nobnak.Gist.Validator validator = new nobnak.Gist.Validator();
            public TextureMatcher(T source, T target)
            {
                this.source = source;
                this.target = target;
            }

            public void Set(T source, T target)
            {
                this.source = source;
                this.target = target;
                this.validator.Validate();
            }
        }

        public class RTMatcher : TextureMatcher<DisposableRenderTexture>
        {
            public RTMatcher(DisposableRenderTexture source, DisposableRenderTexture target) : base(source, target)
            {
                this.validator.Validation += () => this.CreateTexture();
                this.validator.SetCheckers(() =>
                {
                    RenderTexture s = this.source;
                    RenderTexture t = this.target;
                    return CheckNullAndSize(s, t) == false;
                });
            }

            protected void CreateTexture()
            {
                RenderTexture s = this.source;
                if (s == null) return;

                this.target = TextureManager.Create(s.descriptor);
            }
        }

        public class Texture2DMatcher : TextureMatcher<DisposableTexture2D>
        {
            public Texture2DMatcher(DisposableTexture2D source, DisposableTexture2D target) : base(source, target)
            {
                this.validator.Validation += () => this.CreateTexture();
                this.validator.SetCheckers(() =>
                {
                    Texture2D s = this.source;
                    Texture2D t = this.target;
                    return CheckNullAndSize(s,t) == false;
                });
            }

            protected void CreateTexture()
            {
                Texture2D s = this.source;
                if (s == null) return;

                this.target = TextureManager.Create(s.width, s.height, s.format);
            }
        }

        /// <summary>
        /// Return true if target should rebuild
        /// </summary>
        /// <param name="src"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public delegate bool RebuildChecker(Texture src, Texture target);
        public static void MatchSource(this RenderTexture src, ref RenderTexture target, RebuildChecker checker = null)
        {
            var c = checker ?? CheckNullAndSize;
            if(c(src, target))
            {
                target?.DestoryObj();
                target = TextureManager.Create(src.descriptor);
            }
        }

        public static bool CheckNullAndSize(Texture src, Texture target)
        {
            return src != null && (target == null || target.width != src.width || target.height != src.height);
        }

        public static void Clear(this RenderTexture target)
        {
            Clear(target, Color.clear);
        }

        public static void Clear(this RenderTexture target, Color color)
        {
            if (target == null) return;
            using (new RenderTextureScope(target))
            {
                GL.Clear(true, true, color);
            }
        }

        public static void savePng(RenderTexture rt, string path)
        {
            Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
            using (new RenderTextureScope(rt))
            {
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                tex.Apply();
                byte[] bytes = tex.EncodeToPNG();
                tex.DestoryObj();
                File.WriteAllBytes(Path.Combine(Application.streamingAssetsPath, path), bytes);
            }
        }
    }
}
