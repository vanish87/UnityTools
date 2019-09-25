using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityTools.Rendering;

namespace UnityTools.CloestHit
{
    public class CameraClosestHit : MonoBehaviour
    {
        [SerializeField] private bool useCPU = false;
        [SerializeField] private Material renderToDepthMat;
        [SerializeField] private ComputeShader pointComputeShader;

        [SerializeField] private GameObject debugTarget;
        [SerializeField] private bool runTest = false;

        private ComputeBuffer inputBuffer = null;
        private ComputeBuffer outputBuffer = null;

        private int NUM_THREAD_X = 8;

        private Vector4[] pointInput = null;
        private Vector4[] pointOutput = null;

        private Camera cameraCache = null;
        private RenderTexture depthTextureCache;

        private Texture2D cpuDepthTexture;

        // Use this for initialization
        void Start()
        {
            if (runTest)
            {
                var test = new Vector4[2];
                for(int i = 0;i<test.Length;++i)
                {
                    var r = UnityEngine.Random.insideUnitCircle * 10;
                    test[i] = new Vector4(r.x, r.y, r.x, 1);
                }
                this.UpdatePoints(test);
            }
        }

        /// <summary>
        /// To update buffer with input positions in world space
        /// </summary>
        /// <param name="newPointList"></param>
        public void UpdatePoints(Vector4[] newPointList)
        {
            var oldCount = this.GetSize(this.pointInput);
            var newCount = this.GetSize(newPointList);

            this.pointInput = newCount > 0 ? newPointList.Clone() as Vector4[] : null;

            if (oldCount != newCount)
            {
                this.ResizeBuffer();
            }
        }

        /// <summary>
        /// Get the list of hit positions with current camera and its depth texture
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="depthTexture"></param>
        /// <param name="AsCopy"></param>
        /// <returns></returns>
        public Vector4[] GetHitPoints(Camera camera, RenderTexture depthTexture, bool asCopy = true)
        {
            this.CalucateCS(camera, depthTexture);

            return asCopy? this.pointOutput.Clone() as Vector4[]:this.pointOutput;
        }
        /// <summary>
        /// Return current hit list without calculation in compute shader 
        /// </summary>
        /// <param name="AsCopy"></param>
        /// <returns></returns>
        public Vector4[] GetHitPoints(bool asCopy = true)
        {
            return asCopy ? this.pointOutput.Clone() as Vector4[] : this.pointOutput;
        }

        private int GetSize(Vector4[] array)
        {
            if (array == null) return 0;

            return array.Length;
        }

        private void ResizeBuffer()
        {
            this.ReleaseBuffer();

            var count = this.GetSize(this.pointInput);
            if (count > 0)
            {
                this.inputBuffer = new ComputeBuffer(count, Marshal.SizeOf<Vector4>());
                this.outputBuffer = new ComputeBuffer(count, Marshal.SizeOf<Vector4>());

                this.pointOutput = this.pointInput.Clone() as Vector4[];
            }
            else
            {
                this.pointOutput = null;
            }

        }

        private void OnDrawGizmos()
        {
            if (runTest)
            {
                if (this.GetSize(this.pointInput) == 0 || cameraCache == null) return;

                foreach (var p in this.pointInput)
                {
                    Gizmos.DrawSphere(p, 0.1f);
                }

                var old = Gizmos.color;
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(this.cameraCache.transform.position, 0.5f);


                Gizmos.color = Color.red;
                foreach (var p in this.pointOutput)
                {
                    Gizmos.DrawSphere(p, 0.7f);
                }
                Gizmos.color = old;
            }

        }

        private void OnDestroy()
        {
            this.ReleaseBuffer();
        }

        private void ReleaseBuffer()
        {
            this.inputBuffer?.Release();
            this.outputBuffer?.Release();

            this.inputBuffer = null;
            this.outputBuffer = null;
        }


        // Update is called once per frame
        void Update()
        {
            if (runTest)
            {
                if (this.GetSize(this.pointInput) > 0 && debugTarget != null)
                {
                    this.pointInput[0] = debugTarget.transform.position;
                    this.pointInput[0].w = 1;
                }

                if (this.cameraCache != null && debugTarget != null)
                {
                    Debug.DrawLine(this.cameraCache.transform.position, debugTarget.transform.position + (debugTarget.transform.position - this.cameraCache.transform.position) * 4, Color.red);
                }
            }
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            source.MatchSource(ref this.depthTextureCache);

            Graphics.Blit(source, this.depthTextureCache, this.renderToDepthMat);

            if (this.useCPU)
            {
                var width = this.depthTextureCache.width;
                var height = this.depthTextureCache.height;
                if (this.cpuDepthTexture == null)
                    this.cpuDepthTexture = new Texture2D(width, height);
                this.cpuDepthTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                this.cpuDepthTexture.Apply();
            }

            Graphics.Blit(source, destination);

            this.CalucateCS(Camera.current, this.depthTextureCache);
        }

        private void CalucateCS(Camera camera, RenderTexture depthTexture)
        {
            this.cameraCache = camera;
            this.depthTextureCache = depthTexture;

            if (this.cameraCache == null || this.depthTextureCache == null) return;

            var size = this.GetSize(this.pointInput);

            if (size <= 0 || size != this.GetSize(this.pointOutput)) return;


            Matrix4x4 V = camera.worldToCameraMatrix;
            Matrix4x4 P = camera.projectionMatrix;//GL.GetGPUProjectionMatrix(Camera.main.projectionMatrix, true);

            var _ProjToViewMat = P.inverse;
            var _ViewToWorldMat = V.inverse;
            var _VPMat = P * V;
            var _SizeAndNearFar = new Vector4(depthTexture.width, depthTexture.height, camera.nearClipPlane, camera.farClipPlane);

            if (this.useCPU)
            {
                for (int i = 0; i < size; ++i)
                {
                    var worldPos = this.pointInput[i];

                    var clipPos = _VPMat.MultiplyPoint(worldPos);

                    var texCood = new Vector2((clipPos.x + 1) * 0.5f, (clipPos.y + 1) * 0.5f);

                    var cood = new Vector2(texCood.x * _SizeAndNearFar.x, texCood.y * _SizeAndNearFar.y);
                    float depth = this.cpuDepthTexture.GetPixel((int)cood.x, (int)cood.y).r;

	                depth = 1 - depth;

                    //set depth to [-1,1] becasue _ProjToViewMat has z range [-1,1]
                    clipPos.z = (depth * 2) - 1;

                    var viewPos = _ProjToViewMat.MultiplyPoint(clipPos);
                    var depthPos = _ViewToWorldMat.MultiplyPoint(viewPos);

                    this.pointOutput[i] = new Vector4(depthPos.x, depthPos.y, depthPos.z, 1);
                }
            }
            else
            {
                this.inputBuffer.SetData(this.pointInput);
                
                var cs = pointComputeShader;
                var kernelId = cs.FindKernel("CSMain");
                //Set CBuffer Values
                cs.SetTexture(kernelId, "_Depth", this.depthTextureCache);
                cs.SetBuffer(kernelId, "_Input", this.inputBuffer);
                cs.SetBuffer(kernelId, "_Result", this.outputBuffer);
                cs.SetMatrix("_ViewToWorldMat", _ViewToWorldMat);
                cs.SetMatrix("_ProjToViewMat", _ProjToViewMat);
                cs.SetMatrix("_VPMat", _VPMat);
                cs.SetVector("_SizeAndNearFar", _SizeAndNearFar);

                cs.Dispatch(kernelId, Mathf.CeilToInt((float)size / NUM_THREAD_X), 1, 1);

                this.outputBuffer.GetData(this.pointOutput);                
            }

            if (runTest)
            {
                Debug.LogFormat("viewmat\n {0}", V);
                Debug.LogFormat("projectmat\n {0}", P);

                foreach (var p in this.pointInput)
                {
                    var eye = V.MultiplyPoint(p);
                    var prj = P.MultiplyPoint(eye);
                    Debug.LogFormat("pin, {0}", p);
                    Debug.LogFormat("eye, {0}", eye);
                    Debug.LogFormat("prj, {0}", prj);
                }

                foreach (var p in this.pointOutput)
                {
                    Debug.LogFormat("pout, {0}", p);
                }
            }

        }
    }
}
