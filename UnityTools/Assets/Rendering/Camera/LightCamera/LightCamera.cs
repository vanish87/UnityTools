using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityTools.Common;

namespace UnityTools.Rendering
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Light))]
    public class LightCamera : MonoBehaviour
    {
        private Camera targetCamera = null;
        private Light targetLight = null;

        [SerializeField] private Vector3 lightSize = new Vector3(10,10,20);
        [SerializeField] private Vector2Int lightTextureSize = new Vector2Int(512, 512);

        // Use this for initialization
        void Start()
        {
            this.targetLight = this.GetComponent<Light>();
            Assert.IsNotNull(this.targetLight);

            this.targetCamera = this.GetComponentInChildren<Camera>();
            if(this.targetCamera == null)
            {
                GameObject go = new GameObject("Light Camera");
                go.AddComponent<Camera>();
                go.AddComponent<CameraRender>();
                go.transform.parent = this.gameObject.transform;
                this.targetCamera = go.GetComponent<Camera>();
                Assert.IsNotNull(this.targetCamera);
                go.hideFlags = HideFlags.DontSave;
                this.targetCamera.enabled = false;
                this.targetCamera.clearFlags = CameraClearFlags.SolidColor;
                this.targetCamera.targetTexture = TextureManager.Create(new RenderTextureDescriptor(lightTextureSize.x, lightTextureSize.y));
            }

            this.OnLightChange();
        }
        protected void OnDestroy()
        {
            if(this.targetCamera.targetTexture != null)
            {
                var temp = this.targetCamera.targetTexture;
                this.targetCamera.targetTexture = null;
                temp.DestoryObj();
            }
        }

        private void OnLightChange()
        {
            if (this.targetCamera == null) return;

            Transform cam = this.targetCamera.transform;
            cam.position = transform.position;
            cam.rotation = transform.rotation;


            if (this.targetLight.type == LightType.Directional)
            {
                this.targetCamera.orthographic = true;
                this.targetCamera.orthographicSize = this.lightSize.y * 0.5f;
                this.targetCamera.nearClipPlane = this.targetLight.shadowNearPlane;
                this.targetCamera.farClipPlane = this.lightSize.z;
                this.targetCamera.aspect = this.lightSize.x / this.lightSize.y;
            }
            else
            {
                this.targetCamera.orthographic = false;
                this.targetCamera.nearClipPlane = this.targetLight.shadowNearPlane;
                this.targetCamera.farClipPlane = this.lightSize.z * this.targetLight.range;
                this.targetCamera.fieldOfView = this.targetLight.spotAngle;
                this.targetCamera.aspect = 1;
            }

        }

        private void Update()
        {
            this.OnLightChange();
        }
        
    }
}