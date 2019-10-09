using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VolumetricCaustics
{
    [ExecuteInEditMode]
    public class GPUTranfromTest : MonoBehaviour
    {
        /*[SerializeField] private GameObject V0;
        [SerializeField] private GameObject V1;
        [SerializeField] private GameObject V2;

        [SerializeField] private Vector3 v0;
        [SerializeField] private Vector3 v1;
        [SerializeField] private Vector3 v2;

        [SerializeField] private Vector3 v0Normal;

        [SerializeField] private Vector3 r0;
        [SerializeField] private Vector3 r1;
        [SerializeField] private Vector3 r2;

        [SerializeField] private Vector3 c0;
        [SerializeField] private Vector3 c1;
        [SerializeField] private Vector3 c2;

        [SerializeField] private float alpha = 0;
        [SerializeField] private float beamLength = 1;

        [SerializeField] private GameObject viewTarget;


        //-------------------------
        [SerializeField] private Vector3 v0Local;
        [SerializeField] private Vector3 v1Local;
        [SerializeField] private Vector3 v2Local;

        [SerializeField] private Vector3 v0NormalLocal;

        [SerializeField] private Vector3 r0Local;
        [SerializeField] private Vector3 r1Local;
        [SerializeField] private Vector3 r2Local;

        [SerializeField] private Vector3 c0Local;
        [SerializeField] private Vector3 c1Local;
        [SerializeField] private Vector3 c2Local;

        // Use this for initialization
        void Start()
        {
            this.UpdatePosition();
        }

        // Update is called once per frame
        void Update()
        {
            var transformMat = this.GetBeamSpaceTranform();

            var p = viewTarget.transform.position;

            var p1 = transformMat.MultiplyPoint(p);

            this.UpdatePosition();

        }

        void UpdatePosition()
        {
            this.v0 = this.V0.transform.position;
            this.v1 = this.V1.transform.position;
            this.v2 = this.V2.transform.position;
            this.v0Normal = Vector3.Cross(this.v2 - this.v0, this.v1 - this.v0);

            this.c0 = this.v0 + this.r0 * this.beamLength;
            this.c1 = this.v1 + this.r1 * this.beamLength;
            this.c2 = this.v2 + this.r2 * this.beamLength;


            var transformMat = this.GetBeamSpaceTranform();

            this.v0Local = transformMat.MultiplyPoint(this.v0);
            this.v1Local = transformMat.MultiplyPoint(this.v1);
            this.v2Local = transformMat.MultiplyPoint(this.v2);

            this.r0Local = transformMat.MultiplyVector(this.r0);
            this.r1Local = transformMat.MultiplyVector(this.r1);
            this.r2Local = transformMat.MultiplyVector(this.r2);

            this.c0Local = transformMat.MultiplyPoint(this.c0);
            this.c1Local = transformMat.MultiplyPoint(this.c1);
            this.c2Local = transformMat.MultiplyPoint(this.c2);

            this.v0NormalLocal = transformMat.MultiplyVector(this.v0Normal);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(v0, v1);
            Gizmos.DrawLine(v0, v2);
            Gizmos.DrawLine(v1, v2);

            Gizmos.DrawLine(c0, c1);
            Gizmos.DrawLine(c0, c2);
            Gizmos.DrawLine(c1, c2);

            Gizmos.DrawLine(v0, c0);
            Gizmos.DrawLine(v1, c1);
            Gizmos.DrawLine(v2, c2);

            //Gizmos.DrawLine(v0, v0 + v0Normal * 5);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(this.v0Local, 0.2f);
            Gizmos.DrawLine(v0Local, v1Local);
            Gizmos.DrawLine(v0Local, v2Local);
            Gizmos.DrawLine(v1Local, v2Local);

            Gizmos.DrawLine(c0Local, c1Local);
            Gizmos.DrawLine(c0Local, c2Local);
            Gizmos.DrawLine(c1Local, c2Local);

            Gizmos.DrawLine(v0Local, c0Local);
            Gizmos.DrawLine(v1Local, c1Local);
            Gizmos.DrawLine(v2Local, c2Local);

            Gizmos.DrawLine(v0Local, v0Local + v0NormalLocal * 5);


            var origin = this.v0;
            var x = this.v1 - origin;
            var y = -this.v0Normal;
            var z = Vector3.Cross(x, y);
            this.DrawXYZ(x, y, z, origin);

        }

        void DrawXYZ(Vector3 x, Vector3 y, Vector3 z, Vector3 origin)
        {
            var old = Gizmos.color;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, origin + x * 5);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(origin, origin + y * 5);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(origin, origin + z * 5);
            Gizmos.color = old;
        }


        private Matrix4x4 GetBeamSpaceTranform()
        {
            var origin = this.v0;
            var x = this.v1 - origin;
            var y = -this.v0Normal;
            var z = Vector3.Cross(x, y);

            return LookAtLH(origin, origin + x, y);
            / * same as above but need inverse
            var mat = Matrix4x4.LookAt(origin, origin + x, y);
            return mat.inverse;
            * /
        }

        Matrix4x4 LookAtLH(Vector3 eye, Vector3 at, Vector3 up)
        {

            Vector3 zaxis = Vector3.Normalize(at - eye);
            Vector3 xaxis = Vector3.Normalize(Vector3.Cross(up, zaxis));
            Vector3 yaxis = Vector3.Cross(zaxis, xaxis);

            var ret = Matrix4x4.identity;

            //unity c# matrix is column major
            ret.SetColumn(0, new Vector4(xaxis.x, yaxis.x, zaxis.x, 0));
            ret.SetColumn(1, new Vector4(xaxis.y, yaxis.y, zaxis.y, 0));
            ret.SetColumn(2, new Vector4(xaxis.z, yaxis.z, zaxis.z, 0));
            ret.SetColumn(3, new Vector4(-Vector3.Dot(xaxis, eye), -Vector3.Dot(yaxis, eye), -Vector3.Dot(zaxis, eye), 1));

            return ret;
        }*/
    }
}