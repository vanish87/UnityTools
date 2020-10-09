using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Assertions;
using UnityTools.Common;

namespace UnityTools.Rendering
{    
    public class RenderTextureTool
    {
        public class RenderTextureActive : Scope
        {
            protected RenderTexture old;

            public RenderTextureActive(RenderTexture next)
                : base()
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
        public class RenderTextureTemp : Scope
        {
            public RenderTexture Tex { get => this.tex; }
            protected RenderTexture tex;
            public RenderTextureTemp(Texture source)
                : base()
            {
                Assert.IsNotNull(source);
                this.tex = RenderTexture.GetTemporary(source.width, source.height);
            }

            public RenderTextureTemp(RenderTexture source)
                : base()
            {
                Assert.IsNotNull(source);
                this.tex = RenderTexture.GetTemporary(source.descriptor);
            }

            protected override void DisposeManaged()
            {
                base.DisposeManaged();
                if (this.tex != null)
                {
                    RenderTexture.ReleaseTemporary(this.tex);
                    this.tex = null;
                }
            }

            public static implicit operator RenderTextureTemp(Texture data)
            {
                return new RenderTextureTemp(data);
            }
            public static implicit operator RenderTexture(RenderTextureTemp source)
            {
                return source.Tex;
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
                    return RenderTextureTool.CheckNullAndSize(s, t) == false;
                });
            }

            protected void CreateTexture()
            {
                Texture2D s = this.source;
                if (s == null) return;

                this.target = TextureManager.Create(s.width, s.height, s.format);
            }
        }
        public static bool CheckNullAndSize(Texture src, Texture target)
        {
            return src != null && (target == null || target.width != src.width || target.height != src.height);
        }

        public static void savePng(RenderTexture rt, string path)
        {
            Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
            using (new RenderTextureActive(rt))
            {
                tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                tex.Apply();
                byte[] bytes = tex.EncodeToPNG();
                tex.DestoryObj();
                File.WriteAllBytes(System.IO.Path.Combine(Application.streamingAssetsPath, path), bytes);
            }
        }

    }
    public static class RenderTextureExtension
    {
        /// <summary>
        /// Return true if target should rebuild
        /// </summary>
        /// <param name="src"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public delegate bool RebuildChecker(Texture src, Texture target);
        public static void MatchSource(this RenderTexture src, ref RenderTexture target, RebuildChecker checker = null)
        {
            var c = checker ?? RenderTextureTool.CheckNullAndSize;
            if(c(src, target))
            {
                target?.DestoryObj();
                target = TextureManager.Create(src.descriptor);
            }
        }        

        public static void Clear(this RenderTexture target)
        {
            Clear(target, Color.clear);
        }

        public static void Clear(this RenderTexture target, Color color)
        {
            if (target == null) return;
            using (new RenderTextureTool.RenderTextureActive(target))
            {
                GL.Clear(true, true, color);
            }
        }
    }
}
