using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityTools.Common;
using UnityTools.Debuging;

namespace UnityTools.Rendering
{
    public class UnityBlur : PostEffectBase
    {
        public int DownSample { get => this.downsample; set => this.downsample = value; }
        public float BlurSize { get => this.blurSize; set => this.blurSize = value; }
        public int BlurIterations { get => this.blurIterations; set => this.blurIterations = value; }

        [Range(0, 2)]
        [SerializeField] protected int downsample = 1;

        [Range(0.0f, 10.0f)]
        [SerializeField] protected float blurSize = 3.0f;

        [Range(1, 4)]
        [SerializeField] protected int blurIterations = 2;

        [SerializeField] protected BlurType blurType = BlurType.StandardGauss;

        public enum BlurType
        {
            StandardGauss = 0,
            SgxGauss = 1,
        }
        public void BlurTexture(RenderTexture source, RenderTexture output)
        {
            Assert.IsNotNull(source);
            Assert.IsNotNull(output);
            if (output == null || this.mat == null) return;

            var mat = this.mat.Data;

            float widthMod = 1.0f / (1.0f * (1 << downsample));

            mat.SetVector("_Parameter", new Vector4(blurSize * widthMod, -blurSize * widthMod, 0.0f, 0.0f));
            source.filterMode = FilterMode.Bilinear;

            int rtW = source.width >> downsample;
            int rtH = source.height >> downsample;

            if(rtW == 0 || rtH == 0)
            {
                LogTool.Log("downsample to large", LogLevel.Error);
                return;
            }

            // downsample
            RenderTexture rt = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);

            rt.filterMode = FilterMode.Bilinear;
            Graphics.Blit(source, rt, mat, 0);

            var passOffs = blurType == BlurType.StandardGauss ? 0 : 2;

            for (int i = 0; i < blurIterations; i++)
            {
                float iterationOffs = (i * 1.0f);
                mat.SetVector("_Parameter", new Vector4(blurSize * widthMod + iterationOffs, -blurSize * widthMod - iterationOffs, 0.0f, 0.0f));

                // vertical blur
                RenderTexture rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
                rt2.filterMode = FilterMode.Bilinear;
                Graphics.Blit(rt, rt2, mat, 1 + passOffs);
                RenderTexture.ReleaseTemporary(rt);
                rt = rt2;

                // horizontal blur
                rt2 = RenderTexture.GetTemporary(rtW, rtH, 0, source.format);
                rt2.filterMode = FilterMode.Bilinear;
                Graphics.Blit(rt, rt2, mat, 2 + passOffs);
                RenderTexture.ReleaseTemporary(rt);
                rt = rt2;
            }

            Graphics.Blit(rt, output);

            RenderTexture.ReleaseTemporary(rt);
        }
        public void BlurTexture(RenderTexture output)
        {
            Assert.IsNotNull(output);
            if (output == null || this.mat == null) return;

            var mat = this.mat.Data;
            var source = output.CloneTemp();
            this.BlurTexture(source, output);
            RenderTexture.ReleaseTemporary(source);
        }
    }

}