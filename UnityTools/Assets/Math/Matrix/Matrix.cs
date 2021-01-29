using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Legacy
{
    [ExecuteInEditMode]
    public class Matrix : Point
    {
        public Camera currentCam = null;
        public List<GameObject> pos = new List<GameObject>();
        public Frustum objectsRef = new Frustum();
        public Frustum cameraRef = new Frustum();

        [Serializable]
        public struct MatContainer
        {
            public Matrix4x4 viewMat;
            public Matrix4x4 projMat;
            public Matrix4x4 viewportMat;

        }

        [Serializable]
        public class Frustum
        {
            public List<GameObject> pos = new List<GameObject>();
            public MatContainer matContainer = new MatContainer();
            public void UpdateMats(MatContainer mats)
            {
                this.matContainer = mats;
            }
            public void Draw()
            {
                foreach (var p in this.pos)
                {
                    //Gizmos.DrawSphere(p.transform.position, 0.5f);
                }

                var trangleIndex = new Vector2Int[12]
                {
                new Vector2Int(0,1),new Vector2Int(0,3),new Vector2Int(1,2),new Vector2Int(2,3),
                new Vector2Int(4,5),new Vector2Int(4,7),new Vector2Int(5,6),new Vector2Int(6,7),
                new Vector2Int(0,4),new Vector2Int(1,5),new Vector2Int(2,6),new Vector2Int(3,7),
                };

                foreach (var index in trangleIndex)
                {
                    var pos1 = this.pos[index.x].transform.position;
                    var pos2 = this.pos[index.y].transform.position;
                    var mat = /*this.matContainer.projMat * */this.matContainer.viewMat;
                    var viewPos1 = mat.MultiplyPoint(pos1);
                    var viewPos2 = mat.MultiplyPoint(pos2);
                    Gizmos.DrawLine(pos1, pos2);
                    Gizmos.DrawLine(viewPos1, viewPos2);
                }

            }

        }
        public MatContainer cpuMat = new MatContainer();
        public MatContainer cameraMat = new MatContainer();
        // Start is called before the first frame update
        void Start()
        {
            this.currentCam = this.GetComponent<Camera>();
        }

        // Update is called once per frame
        void Update()
        {
            this.cameraMat.viewMat = this.currentCam.worldToCameraMatrix;
            this.cameraMat.projMat = this.currentCam.projectionMatrix;

            var center = this.currentCam.transform.position;
            var forward = this.currentCam.transform.forward;
            var up = this.currentCam.transform.up;
            this.cpuMat.viewMat = Matrix4x4.LookAt(center, forward, up);

            var fov = this.currentCam.fieldOfView;
            var aspect = this.currentCam.aspect;
            var near = this.currentCam.nearClipPlane;
            var far = this.currentCam.farClipPlane;
            this.cpuMat.projMat = Matrix4x4.Perspective(fov, aspect, near, far);

            Debug.Log(this.cpuMat.viewMat);
            Debug.Log("cam" + this.cameraMat.viewMat);

            var refpos = pos[0].transform.position;
            var view = this.cameraMat.viewMat.MultiplyPoint(refpos);
            var project = this.cameraMat.projMat.MultiplyPoint(view);

            this.pos[1].transform.position = view;
            this.pos[2].transform.position = project;

            this.cameraRef.UpdateMats(this.cpuMat);
            this.objectsRef.UpdateMats(this.cpuMat);
        }

        void OnDrawGizmos()
        {
            this.objectsRef.Draw();
            //this.cameraRef.Draw();
        }
    }
}