using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[ExecuteInEditMode]
public class OffCenterCamera : MonoBehaviour
{
    public float left = -0.5f;
    public float right = 0.5f;
    public float top = 0.5f;
    public float bottom = -0.5f;

    public Vector2 pos = Vector2.zero;
    public Vector2 size = Vector2.one;

    //Note left/right/top/bottom of off center camera will not change the source resolution of OnRenderImage
    //But Camera Rect will change the resolution of source texture on OnRenderImage
    //The destination resolution of camera will be cam.targetTexture's resolution
    //The destination will be null if there is no targetTexture specified.
    public Vector2Int currentSourceRes;
    public Vector2Int currentDestinationRes;

    public Camera cam;

    private void Start()
    {
        this.cam = this.GetComponent<Camera>();
        this.cam.targetTexture = new RenderTexture(256, 128, 24);
    }
    void LateUpdate()
    {
        cam.rect = new Rect(pos, size);        
        Matrix4x4 m = cam.orthographic?OrthograhicOffCenter(left, right, bottom, top, cam.nearClipPlane, cam.farClipPlane): 
                                       PerspectiveOffCenter(left, right, bottom, top, cam.nearClipPlane, cam.farClipPlane);
        cam.projectionMatrix = m;
    }

    protected void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        this.currentSourceRes = new Vector2Int(source.width, source.height);
        if (destination != null) this.currentDestinationRes = new Vector2Int(destination.width, destination.height);

        Graphics.Blit(source, destination);
    }

    public static Matrix4x4 OrthograhicOffCenter(float left, float right, float bottom, float top, float near, float far)
    {
        Assert.IsTrue(left < right && bottom < top);
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
        Assert.IsTrue(left < right && bottom < top);
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
