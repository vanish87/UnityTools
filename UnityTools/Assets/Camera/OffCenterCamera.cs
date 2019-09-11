using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class OffCenterCamera : MonoBehaviour
{
    public float left = -0.2F;
    public float right = 0.2F;
    public float top = 0.2F;
    public float bottom = -0.2F;

    public Vector2Int currentSourceRes;
    public Vector2Int currentDestinationRes;
    void LateUpdate()
    {
        Camera cam = Camera.main;

        Matrix4x4 m = cam.orthographic?OrthograhicOffCenter(left, right, bottom, top, cam.nearClipPlane, cam.farClipPlane): 
            PerspectiveOffCenter(left, right, bottom, top, cam.nearClipPlane, cam.farClipPlane);
        cam.projectionMatrix = m;
    }

    protected void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        this.currentSourceRes = new Vector2Int(source.width, source.height);
        if(destination != null) this.currentDestinationRes = new Vector2Int(destination.width, destination.height);

        Graphics.Blit(source, destination);
    }
    public static void OnDrawOffCenterCamera(Camera camera)
    {
        if (camera == null || camera.enabled == false) return;
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
        foreach (var p in ndcPos)
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
    public static Matrix4x4 OrthograhicOffCenter(float left, float right, float bottom, float top, float near, float far)
    {
        float x = 2.0F/ (right - left);
        float y = 2.0F/ (top - bottom);
        float a = (right + left) / (right - left);
        float b = (top + bottom) / (top - bottom);
        float c = 2.0F / (near - far);
        float d = (near + far) / (near - far);
        float e = 0;
        float f = 1.0F;
        Matrix4x4 m = new Matrix4x4();
        m[0, 0] = x;
        m[0, 1] = 0;
        m[0, 2] = a;
        m[0, 3] = 0;
        m[1, 0] = 0;
        m[1, 1] = y;
        m[1, 2] = b;
        m[1, 3] = 0;
        m[2, 0] = 0;
        m[2, 1] = 0;
        m[2, 2] = c;
        m[2, 3] = d;
        m[3, 0] = 0;
        m[3, 1] = 0;
        m[3, 2] = e;
        m[3, 3] = f;
        return m;
    }

    public static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
    {
        float x = 2.0F * near / (right - left);
        float y = 2.0F * near / (top - bottom);
        float a = (right + left) / (right - left);
        float b = (top + bottom) / (top - bottom);
        float c = -(far + near) / (far - near);
        float d = -(2.0F * far * near) / (far - near);
        float e = -1.0F;
        Matrix4x4 m = new Matrix4x4();
        m[0, 0] = x;
        m[0, 1] = 0;
        m[0, 2] = a;
        m[0, 3] = 0;
        m[1, 0] = 0;
        m[1, 1] = y;
        m[1, 2] = b;
        m[1, 3] = 0;
        m[2, 0] = 0;
        m[2, 1] = 0;
        m[2, 2] = c;
        m[2, 3] = d;
        m[3, 0] = 0;
        m[3, 1] = 0;
        m[3, 2] = e;
        m[3, 3] = 0;
        return m;
    }
}
