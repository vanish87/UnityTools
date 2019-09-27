using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Rendering
{
    public class TextureManager 
    {
        #if DEBUG
        public class TextureTracking
        {
            public struct TextureTackingData
            {
                public Texture dataRef;
                public string callingStack;
            }
            protected List<TextureTackingData> list = new List<TextureTackingData>();

            public void Add(Texture tex)
            {
                var stack = new System.Diagnostics.StackFrame(2);
                var stackInfo = stack.GetFileName() + " " + stack.GetFileLineNumber() + " " + stack.GetMethod();
                stackInfo = Environment.StackTrace;

                this.list.Add(
                new TextureTackingData() {
                    dataRef = tex,
                    callingStack = stackInfo
                });
            }

            public void ReportTextures()
            {
                foreach(var t in this.list)
                {
                    Debug.LogFormat("Texture {0} is created {1}", t.dataRef.name, t.callingStack);
                }
            }
        }
        public static TextureTracking tracking = new TextureTracking(); 
        #endif

        public static RenderTexture Create(RenderTextureDescriptor descriptor)
        {
            var ret = new RenderTexture(descriptor);

            #if DEBUG
            tracking.Add(ret);
            #endif

            return ret;
        }

        public static Texture2D Create(int width, int heigh, TextureFormat format = TextureFormat.ARGB32, bool mipChain = true, bool linear = false)
        {
            var ret = new Texture2D(width, heigh, format, mipChain, linear);

            #if DEBUG
            tracking.Add(ret);
            #endif

            return ret;
        }
    }
}