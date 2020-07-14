using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityTools.Rendering
{
    public static class TextureExtension
    {
        public static RenderTexture CloneTemp(this RenderTexture tex, bool copy = true)
        {
            var ret = RenderTexture.GetTemporary(tex.descriptor);
            if(copy) Graphics.CopyTexture(tex, ret);

            #if DEBUG
            TextureManager.tracking.AddTemp(ret);
            #endif

            return ret;
        }
        public static Texture Clone(this Texture tex, bool copy = true)
        {
            var input2D = tex as Texture2D;
            if (input2D != null)
            {
                var ret = new Texture2D(input2D.width, input2D.height, input2D.format, input2D.mipmapCount > 1);
                
                #if DEBUG
                TextureManager.tracking.Add(ret);
                #endif

                if(copy) Graphics.CopyTexture(tex, ret);
                return ret;
            }
            var inputRT = tex as RenderTexture;
            if(inputRT != null)
            {
                var ret = new RenderTexture(inputRT.descriptor);
                
                #if DEBUG
                TextureManager.tracking.Add(ret);
                #endif

                if(copy) Graphics.CopyTexture(tex, ret);

                return ret;
            }

            Assert.IsTrue(false, "Cannot Clone texture");
            return default;
        }
    }
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
            protected List<TextureTackingData> listTemp = new List<TextureTackingData>();

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
            public void AddTemp(Texture tex)
            {
                var stack = new System.Diagnostics.StackFrame(2);
                var stackInfo = stack.GetFileName() + " " + stack.GetFileLineNumber() + " " + stack.GetMethod();
                stackInfo = Environment.StackTrace;

                this.listTemp.Add(
                new TextureTackingData()
                {
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
                foreach (var t in this.listTemp)
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

            ret.Create();

            return ret;
        }

        public static Texture2D Create(int width, int heigh, TextureFormat format = TextureFormat.ARGB32, bool mipChain = true, bool linear = false)
        {
            var ret = new Texture2D(width, heigh, format, mipChain, linear);

            #if DEBUG
            tracking.Add(ret);
            #endif
            
            ret.Apply();

            return ret;
        }
        
        public static Texture2DArray Create(int width, int heigh, int NumOfTexture, TextureFormat format = TextureFormat.ARGB32, bool mipChain = true, bool linear = false)
        {
            var ret = new Texture2DArray(width, heigh, NumOfTexture, format, mipChain, linear);

            #if DEBUG
            tracking.Add(ret);
            #endif

            return ret;
        }        
    }
}