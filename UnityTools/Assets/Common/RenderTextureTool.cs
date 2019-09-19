using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Assertions;

namespace UnityTools.Rendering
{
    public class RenderTextureTool
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

        public class AutoObject<T> : Disposable
        {
            public T Data { get => this.data; set => this.data = value; }
            protected T data;

            public AutoObject(T data)
                : base()
            {
                this.data = data;
            }
        }

        public class AutoRenderTexture : AutoObject<RenderTexture>
        {
            public AutoRenderTexture(RenderTexture data) : base(data)
            {
            }

            protected override void DisposeManaged()
            {
                base.DisposeManaged();
                if (data != null)
                {
                    data.DestoryObj();
                }
            }
            public static implicit operator AutoRenderTexture(RenderTexture data)
            {
                return new AutoRenderTexture(data);
            }
            public static implicit operator RenderTexture(AutoRenderTexture source)
            {
                return source.Data;
            }
        }
        public class AutoTexture2D : AutoObject<Texture2D>
        {
            public AutoTexture2D(Texture2D data) : base(data)
            {
            }

            protected override void DisposeManaged()
            {
                base.DisposeManaged();
                if (data != null)
                {
                    data.DestoryObj();
                }
            }
            public static implicit operator AutoTexture2D(Texture2D data)
            {
                return new AutoTexture2D(data);
            }
            public static implicit operator Texture2D(AutoTexture2D source)
            {
                return source.Data;
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

        public class RTMatcher : TextureMatcher<AutoRenderTexture>
        {
            public RTMatcher(AutoRenderTexture source, AutoRenderTexture target) : base(source, target)
            {
                this.validator.Validation += () => this.CreateTexture();
                this.validator.SetCheckers(() =>
                {
                    RenderTexture s = this.source;
                    RenderTexture t = this.target;
                    return ShouldRebuild(s, t) == false;
                });
            }

            protected void CreateTexture()
            {
                RenderTexture s = this.source;
                if (s == null) return;

                this.target = TextureManager.Create(s.descriptor);
            }
        }

        public class Texture2DMatcher : TextureMatcher<AutoTexture2D>
        {
            public Texture2DMatcher(AutoTexture2D source, AutoTexture2D target) : base(source, target)
            {
                this.validator.Validation += () => this.CreateTexture();
                this.validator.SetCheckers(() =>
                {
                    Texture2D s = this.source;
                    Texture2D t = this.target;
                    return ShouldRebuild(s,t) == false;
                });
            }

            protected void CreateTexture()
            {
                Texture2D s = this.source;
                if (s == null) return;

                this.target = TextureManager.Create(s.width, s.height, s.format);
            }
        }
        static public void MatchSource(RenderTexture src, ref RenderTexture target)
        {
            if(ShouldRebuild(src, target))
            {
                target?.DestoryObj();
                target = TextureManager.Create(src.descriptor);
            }
        }

        static public bool ShouldRebuild(Texture src, Texture target)
        {
            return src != null && (target == null || target.width != src.width || target.height != src.height);
        }
        
        static public void Clear(RenderTexture target)
        {
            Clear(target, Color.clear);
        }

        static public void Clear(RenderTexture target, Color color)
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
