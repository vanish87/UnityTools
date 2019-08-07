// KlakNDI - NDI plugin for Unity
// https://github.com/keijiro/KlakNDI

using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor; // Needed to use delayCall
#endif

namespace Klak.Ndi
{
    [ExecuteInEditMode]
    [AddComponentMenu("Klak/NDI/NDI Sender")]
    public sealed class NdiSender : MonoBehaviour
    {
        #region Source texture

        [SerializeField] RenderTexture _sourceTexture;

        public RenderTexture sourceTexture {
            get { return _sourceTexture; }
            set { _sourceTexture = value; }
        }

        [SerializeField] bool _invertY = true;

        #endregion

        #region Format option


        [SerializeField] FourCC _dataFormat = FourCC.UYVA;
        public FourCC DataFormat
        {
            get { return _dataFormat; }
            set
            {
                if (_dataFormat == value) return;
                _dataFormat = value;
            }
        }

        #endregion
        #region Name


        [SerializeField] string _name = "NDISender";
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;

                //create a new sender with new name
                if (_plugin != IntPtr.Zero)
                {
                    PluginEntry.DestroySender(_plugin);
                    _plugin = IntPtr.Zero;
                }
            }
        }

        #endregion

        #region Private members

        Material _material;
        RenderTexture _converted;
        int _lastFrameCount = -1;

        #endregion

        #region Frame readback queue

        struct Frame
        {
            public int width, height;
            public FourCC format;
            public AsyncGPUReadbackRequest readback;
        }

        Queue<Frame> _frameQueue = new Queue<Frame>(4);

        void QueueFrame(RenderTexture source)
        {
            if (!PluginEntry.IsAvailable) return;

            if (_frameQueue.Count > 3)
            {
                Debug.LogWarning("Too many GPU readback requests.");
                return;
            }

            // On Editor, this may be called multiple times in a single frame.
            // To avoid wasting memory (actually this can cause an out-of-memory
            // exception), check the frame count and reject duplicated requests.
            if (_lastFrameCount == Time.frameCount) return;
            _lastFrameCount = Time.frameCount;

            // Return the old render texture to the pool.
            if (_converted != null) RenderTexture.ReleaseTemporary(_converted);

            var sw = source.width;
            var sh = source.height;

            var tw = sw;
            var th = sh;

            var alphaSupport = _dataFormat == FourCC.UYVA;
            var isRGBA = true;
            switch (_dataFormat)
            {
                case FourCC.UYVA:
                case FourCC.UYVY:
                    {
                        tw = sw / 2;
                        th = sh * (alphaSupport ? 3 : 2) / 2;
                        isRGBA = false;
                    }
                    break;
            }

            // Allocate a new render texture.
            _converted = RenderTexture.GetTemporary(
                tw, th, 0,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear
            );

            // Lazy initialization of the conversion shader.
            if (_material == null)
            {
                _material = new Material(Shader.Find("Hidden/KlakNDI/Sender"));
                _material.hideFlags = HideFlags.DontSave;
            }

            // Apply the conversion shader.
            if (isRGBA)
            {
                if (_invertY)
                {
                    Graphics.Blit(source, _converted, _material, 2);//pass 2 is invert y pass
                }
                else
                {
                    Graphics.Blit(source, _converted);
                }
            }
            else
            {
                Graphics.Blit(source, _converted, _material, alphaSupport ? 1 : 0);
            }

            // Request readback.
            _frameQueue.Enqueue(new Frame{
                //note this is source width/height, not texture size
                width = sw, height = sh, format = _dataFormat,
                readback = AsyncGPUReadback.Request(_converted)
            });
        }

        void ProcessQueue()
        {
            while (_frameQueue.Count > 0)
            {
                var frame = _frameQueue.Peek();

                // Edit mode: Wait for readback completion every frame.
                if (!Application.isPlaying) frame.readback.WaitForCompletion();

                // Skip error frames.
                if (frame.readback.hasError)
                {
                    Debug.LogWarning("GPU readback error was detected.");
                    _frameQueue.Dequeue();
                    continue;
                }

                // Break when found a frame that hasn't been read back yet.
                if (!frame.readback.done) break;

                // Okay, we're going to send this frame.

                // Lazy initialization of the plugin sender instance.
                if (_plugin == IntPtr.Zero) _plugin = PluginEntry.CreateSender(this.Name);

                // Feed the frame data to the sender. It encodes/sends the
                // frame asynchronously.
                unsafe {
                    PluginEntry.SendFrame(
                        _plugin, (IntPtr)frame.readback.GetData<Byte>().GetUnsafeReadOnlyPtr(),
                        frame.width, frame.height, frame.format
                    );
                }

                // Done. Remove the frame from the queue.
                _frameQueue.Dequeue();
            }

            // Edit mode: We're not sure when the readback buffer will be
            // disposed, so let's synchronize with the sender to prevent it
            // from accessing disposed memory area.
            if (!Application.isPlaying && _plugin != IntPtr.Zero) PluginEntry.SyncSender(_plugin);
        }

        #if UNITY_EDITOR

        // Delayed update callback
        // The readback queue works on the assumption that the next update will
        // come in a short period of time. In editor, this assumption is not
        // guaranteed -- updates can be discontinuous. The last update before
        // a pause won't be shown immediately, and it will be delayed until the
        // next user action. This can mess up editor interactivity.
        // To solve this problem, we use EditorApplication.delayUpdate to send
        // discontinuous updates in a synchronous fashion.

        bool _delayUpdateAdded;

        void DelayedUpdate()
        {
            _delayUpdateAdded = false;

            // Queue the last update in the render texture mode.
            if (!_hasCamera && _sourceTexture != null) QueueFrame(_sourceTexture);

            // Process the readback queue to send the last update.
            ProcessQueue();
        }

        #endif

        #endregion

        #region Misc variables

        IntPtr _plugin;
        bool _hasCamera;

        #endregion

        #region MonoBehaviour implementation

        IEnumerator Start()
        {
            _hasCamera = (GetComponent<Camera>() != null);

            // Only run the sync coroutine in the play mode.
            if (!Application.isPlaying) yield break;

            // Synchronize with the async sender at the end of every frame.
            for (var wait = new WaitForEndOfFrame();;)
            {
                yield return wait;
                if (enabled && _plugin != IntPtr.Zero) PluginEntry.SyncSender(_plugin);
            }
        }

        void OnDestroy()
        {
            if (_material != null)
            {
                if (Application.isPlaying)
                    Destroy(_material);
                else
                    DestroyImmediate(_material);
                _material = null;
            }
        }

        void OnDisable()
        {
            if (_converted != null)
            {
                RenderTexture.ReleaseTemporary(_converted);
                _converted = null;
            }

            if (_plugin != IntPtr.Zero)
            {
                PluginEntry.DestroySender(_plugin);
                _plugin = IntPtr.Zero;
            }

        #if UNITY_EDITOR
            _delayUpdateAdded = false;
        #endif
        }

        void Update()
        {
        #if UNITY_EDITOR
            // Edit mode: Register the delayed update callback.
            if (!Application.isPlaying && !_delayUpdateAdded)
            {
                EditorApplication.delayCall += DelayedUpdate;
                _delayUpdateAdded = true;
            }
        #endif

            // Edit mode: Check the camera capture mode every frame.
            if (!Application.isPlaying) _hasCamera = (GetComponent<Camera>() != null);

            // Check if in the render texture mode.
            if (!_hasCamera && _sourceTexture != null)
            {
                // Process the readback queue before enqueuing.
                ProcessQueue();

                // Push the source texture into the readback queue.
                QueueFrame(_sourceTexture);
            }
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (source != null)
            {
                // Process the readback queue before enqueuing.
                ProcessQueue();

                // Push the source image into the readback queue.
                QueueFrame(source);
            }

            // Dumb blit
            Graphics.Blit(source, destination);
        }

        #endregion
    }
}
