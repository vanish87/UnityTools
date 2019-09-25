using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Common;

namespace UnityTools.Rendering
{
    #if USE_EDITOR_EXC
[   ExecuteInEditMode]
    #endif
    public class DepthNormalCamera : MonoBehaviour
    {
        public enum OutputMode
        {
            DepthOnly = 0,
            NormalOnly,
            DepthNormalEncoded,
            DepthNomalSeparated,
        }

        public enum NormalSpace
        {
            None,//for depth
            View,
            World,
        }

        protected Dictionary<OutputMode, DepthTextureMode> OutputModeToCameraMode = new Dictionary<OutputMode, DepthTextureMode>()
        {
            { OutputMode.DepthOnly          , DepthTextureMode.Depth},
            { OutputMode.NormalOnly         , DepthTextureMode.DepthNormals},
            { OutputMode.DepthNormalEncoded , DepthTextureMode.DepthNormals},
            { OutputMode.DepthNomalSeparated, DepthTextureMode.DepthNormals},
        };

        protected Dictionary<OutputMode, RenderTextureFormat> OutputModeToRenderTextureFormat = new Dictionary<OutputMode, RenderTextureFormat>()
        {
            { OutputMode.DepthOnly          , RenderTextureFormat.RFloat},
            { OutputMode.NormalOnly         , RenderTextureFormat.ARGBFloat},
            { OutputMode.DepthNormalEncoded , RenderTextureFormat.ARGB32},
        };

        protected Dictionary<OutputMode, int> OutputModeToPass = new Dictionary<OutputMode, int>()
        {
            { OutputMode.DepthOnly          , 0},
            { OutputMode.NormalOnly         , 1},
            { OutputMode.DepthNormalEncoded , 2},
        };

        protected static bool DoUseWorld(NormalSpace valueSpace)
        {
            return valueSpace == NormalSpace.World;
        }

        public RenderTexture DepthTexture  { get { return this.depthTexture; } }
        public RenderTexture NormalTexture { get { return this.normalTexture; } }
        public Camera Cam { get { return this.renderCamera; } }


        /// <summary>
        /// Pack current camera depth into depthTexture for future use
        /// </summary>
        [SerializeField] protected RenderTexture depthTexture;
        [SerializeField] protected RenderTexture normalTexture;

        [SerializeField] protected OutputMode   outputMode   = OutputMode.DepthOnly;
        [SerializeField] protected NormalSpace  normalSpace  = NormalSpace.None;

        [SerializeField] protected Shader renderToDepth;
        protected DisposableMaterial mat;

        /// <summary>
        /// depth camera is a place holder for real cv camera
        /// all parameter will come from this camera, and its configure will be saved in xml
        /// even if we use cv camera for depth, 
        /// this camera will provide camera parameter(like position, fov etc.) for recovering position later
        /// </summary>
        protected Camera renderCamera;


        protected void OnSettingChanged()
        {
            this.DepthTexture.DestoryObj();
            this.NormalTexture.DestoryObj();

            if (this.renderCamera == null) return;
            this.renderCamera.depthTextureMode = this.OutputModeToCameraMode[this.outputMode];
            if (this.outputMode == OutputMode.DepthOnly) this.normalSpace = NormalSpace.None;
        }

        protected void OnEnable()
        {
            this.mat = this.mat ?? new DisposableMaterial(this.renderToDepth);
            this.renderCamera = this.renderCamera ?? this.GetComponent<Camera>();

            this.OnSettingChanged();
        }
        protected void OnDisable()
        {
            if (this.renderCamera != null) this.renderCamera.targetTexture = null;

            this.DepthTexture.DestoryObj();
            this.NormalTexture.DestoryObj();
            this.mat.Dispose();
        }

        protected void OnValidate()
        {
            this.OnSettingChanged();
        }


        protected void CheckTexture(RenderTexture source, string name, RenderTextureFormat format, ref RenderTexture target)
        {
            if (RenderTextureTool.CheckNullAndSize(source, target))
            {
                target?.DestoryObj();

                RenderTextureDescriptor desc = source.descriptor;
                desc.colorFormat = format;
                desc.sRGB = false;

                target = TextureManager.Create(desc);
                target.name = name;
            }
        }

        protected void RenderToTextures(RenderTexture source)
        {
            Material m = this.mat;
            m.SetMatrix("_ViewToWorldMat", DoUseWorld(this.normalSpace) ? this.renderCamera.cameraToWorldMatrix : Matrix4x4.identity);
            m.SetFloat("_PackToColor", 0);

            switch (this.outputMode)
            {
                case OutputMode.DepthOnly:
                case OutputMode.NormalOnly:
                case OutputMode.DepthNormalEncoded:
                    {
                        this.CheckTexture(source, this.outputMode.ToString(), this.OutputModeToRenderTextureFormat[outputMode], ref this.depthTexture);
                        this.normalTexture = this.depthTexture;

                        Graphics.Blit(null, this.depthTexture, this.mat, this.OutputModeToPass[this.outputMode]);
                    }
                    break;
                case OutputMode.DepthNomalSeparated:
                    {
                        this.CheckTexture(source, nameof(this.depthTexture), this.OutputModeToRenderTextureFormat[OutputMode.DepthOnly], ref this.depthTexture);
                        this.CheckTexture(source, nameof(this.normalTexture), this.OutputModeToRenderTextureFormat[OutputMode.NormalOnly], ref this.normalTexture);

                        Graphics.Blit(null, this.depthTexture, this.mat, this.OutputModeToPass[OutputMode.DepthOnly]);
                        Graphics.Blit(null, this.normalTexture, this.mat, this.OutputModeToPass[OutputMode.NormalOnly]);
                    }
                    break;
            }
        }

        protected void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            this.RenderToTextures(source);

            //UnityEngine.Graphics.Blit(null, this.DepthTexture, this.mat, this.renderCamera.orthographic ? 1 : 0);
            Graphics.Blit(this.DepthTexture, destination);
        }
    }
}