using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityTools.Rendering
{    
    #if USE_EDITOR_EXC
    [ExecuteInEditMode]
    #endif
    public class CameraFrustumGizmos : MonoBehaviour
    {
        public bool AlwaysDisplay = false;
        [SerializeField] protected Camera cam;
        protected void Start()
        {
            this.cam = this.gameObject.FindOrAddTypeInComponentsAndChildren<Camera>();
        }

        protected void OnDrawGizmosSelected()
        {
            if (this.enabled && !this.AlwaysDisplay)
            {
                OnDrawOffCenterCameraFrustum(this.cam);
            }
        }
        protected void OnDrawGizmos()
        {
            if(this.enabled && this.AlwaysDisplay)
            {
                OnDrawOffCenterCameraFrustum(this.cam);
            }
        }

        /// <summary>
        /// This is a simpler version of draw frustum without offset
        /// </summary>
        public static void DrawNormalCameraFrustum(Camera camera)
        {
            if (camera == null) return;

            if (camera.orthographic)
            {
                var oldMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(camera.transform.position, camera.transform.rotation, Vector3.one);
                var far = new Vector3(0, 0, 1) * camera.farClipPlane;
                var near = new Vector3(0, 0, 1) * camera.nearClipPlane;
                var center = (far + near) / 2;
                var height = camera.orthographicSize * 2;
                var width = height * camera.aspect;
                Gizmos.DrawWireCube(center, new Vector3(width, height, camera.farClipPlane - camera.nearClipPlane));
                Gizmos.matrix = oldMatrix;
            }
            else
            {
                var oldMat = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(camera.transform.position, camera.transform.rotation, Vector3.one);
                Gizmos.DrawFrustum(Vector3.zero, camera.fieldOfView, camera.farClipPlane, camera.nearClipPlane, camera.aspect);
                Gizmos.matrix = oldMat;
            }
        }

        /// <summary>
        /// draw correct off center camera frustum
        /// </summary>
        /// <param name="camera"></param>
        public static void OnDrawOffCenterCameraFrustum(Camera camera)
        {
            if (camera == null) return;

            var old = Gizmos.color;
            Gizmos.color = Color.red;
            var ndcPos = new Vector3[]
            {
                new Vector3(-1,-1,-1), new Vector3(1,-1,-1), new Vector3(-1,1,-1), new Vector3(1,1,-1),
                new Vector3(-1,-1, 1), new Vector3(1,-1, 1), new Vector3(-1,1, 1), new Vector3(1,1, 1),
            };

            var viewPos = new Vector3[8];
            var count = 0;
            var ndcToViewMat = camera.projectionMatrix.inverse;
            foreach(var p in ndcPos)
            {
                viewPos[count] = ndcToViewMat.MultiplyPoint(p);
                viewPos[count].z = -viewPos[count].z;
                count++;
            }

            var oldMat = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(camera.transform.position, camera.transform.rotation, Vector3.one);
            //near plane
            Gizmos.DrawLine(viewPos[0], viewPos[1]);
            Gizmos.DrawLine(viewPos[1], viewPos[3]);
            Gizmos.DrawLine(viewPos[3], viewPos[2]);
            Gizmos.DrawLine(viewPos[0], viewPos[2]);

            //far plane
            Gizmos.DrawLine(viewPos[4], viewPos[5]);
            Gizmos.DrawLine(viewPos[5], viewPos[7]);
            Gizmos.DrawLine(viewPos[7], viewPos[6]);
            Gizmos.DrawLine(viewPos[4], viewPos[6]);

            //near->far lines
            Gizmos.DrawLine(viewPos[0], viewPos[4]);
            Gizmos.DrawLine(viewPos[1], viewPos[5]);
            Gizmos.DrawLine(viewPos[2], viewPos[6]);
            Gizmos.DrawLine(viewPos[3], viewPos[7]);

            Gizmos.matrix = oldMat;
            Gizmos.color = old;

        }
    }
}