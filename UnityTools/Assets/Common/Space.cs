using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityTools.Debuging;
using UnityTools.Debuging.EditorTool;

namespace UnityTools.Common
{
    public enum SpaceCorner
    {
        NearLeftUp,
        NearLeftBottom,
        NearRightUp,
        NearRightBottom,
        FarLeftUp,
        FarLeftBottom,
        FarRightUp,
        FarRightBottom,
    }
    public interface ISpace
    {
        float3 Center { get; set; }
        quaternion Rotation { get; set; }
        float3 Scale { get; set; }
        Matrix4x4 TRS { get; }
        Matrix4x4 ToLocal { get; }
        Matrix4x4 ToWorld { get; }

        float3 Forward { get; }
        float3 Up { get; }
        float3 Right { get; }

		Color Color { get; }
		bool DisplayText { get; set;}
		bool DisplayBound { get; set;}
		bool DisplayAxis { get; set;}

		Bounds Bound { get; }

        float3 LocalPoint(SpaceCorner corner);
        bool3 IsInSpace(float3 point);

        float3 LocalToWorld(float3 local);
        float3 WorldToLocal(float3 world);
    }

    public static class Space
    {
		public static readonly Space3D LargeSpace = new Space3D() { Center = 0, Rotation = quaternion.identity, Scale = 10000 };
		public static readonly Space3D IdentitySpace = new Space3D() { Center = 0, Rotation = quaternion.identity, Scale = 1 };
        public static readonly Dictionary<SpaceCorner, float3> SpaceCorners = new Dictionary<SpaceCorner, float3>()
        {
            {SpaceCorner.NearLeftUp,        new float3(-0.5f, 0.5f, -0.5f) },
            {SpaceCorner.NearLeftBottom,    new float3(-0.5f, -0.5f, -0.5f) },
            {SpaceCorner.NearRightUp,       new float3(0.5f, 0.5f, -0.5f) },
            {SpaceCorner.NearRightBottom,   new float3(0.5f, -0.5f, -0.5f) },

            {SpaceCorner.FarLeftUp,         new float3(-0.5f, 0.5f, 0.5f) },
            {SpaceCorner.FarLeftBottom,     new float3(-0.5f, -0.5f, 0.5f) },
            {SpaceCorner.FarRightUp,        new float3(0.5f, 0.5f, 0.5f) },
            {SpaceCorner.FarRightBottom,    new float3(0.5f, -0.5f, 0.5f) },
        };
        public static int2 SpaceToPixelSize(ISpace space, int pixelSize)
        {
            LogTool.AssertIsTrue(space.Scale.x > 0);
            LogTool.AssertIsTrue(pixelSize > 2);

            var aspect = space.Scale.y / space.Scale.x;
			return new int2(pixelSize, Mathf.CeilToInt(pixelSize * aspect));
        }
        public static void SetGameObjectToSpace(GameObject go, ISpace space)
        {
			if (go == null || space == null) return;

            go.transform.localPosition = space.Center;
            go.transform.localRotation = space.Rotation;
            go.transform.localScale = space.Scale;
        }
        public static void SetSpaceFromGameObject(GameObject go, ISpace space)
        {
            space.Center = go.transform.localPosition;
            space.Rotation = go.transform.localRotation;
            space.Scale = go.transform.localScale;
        }
        public static void SetCameraToSpace(Camera camera, ISpace space)
        {
            if(!camera.orthographic) LogTool.Log("Camera is not orthographic", LogLevel.Warning);

            camera.gameObject.transform.localPosition = space.TRS.MultiplyPoint(new Vector3(0,0,-0.5f));
            camera.gameObject.transform.localRotation = space.Rotation;
        }
    }
    

    [System.Serializable]
    public class Space3D : ISpace
    {
        public float3 Center { get => this.center; set => this.center = value; }
        public quaternion Rotation { get => this.rotation; set => this.rotation = value; }
        public float3 Scale { get => this.scale; set => this.scale = value; }

        public Matrix4x4 TRS => Matrix4x4.TRS(this.Center, this.Rotation, this.Scale);

        public float3 Forward => math.mul(this.Rotation, new float3(0, 0, 1));

        public float3 Up => math.mul(this.Rotation, new float3(0, 1, 0));

        public float3 Right => math.mul(this.Rotation, new float3(1, 0, 0));

		public Matrix4x4 ToLocal => this.TRS.inverse;
		public Matrix4x4 ToWorld => this.TRS;

		public Color Color => this.color;

		public bool DisplayText { get => this.displayText; set => this.displayText = value; }
		public bool DisplayBound { get => this.displayBound; set => this.displayBound = value; }
		public bool DisplayAxis { get => this.displayAxis; set => this.displayAxis = value; }

        public Bounds Bound 
        { 
            get 
            {
                Bounds ret = new Bounds();
                ret.center = this.Center;
                foreach(SpaceCorner lp in Enum.GetValues(typeof(SpaceCorner)))
                {
                    ret.Encapsulate(this.TRS.MultiplyPoint(this.LocalPoint(lp)));
                }
                return ret;
            }
        }
        [SerializeField] protected float3 center;
        [SerializeField] protected quaternion rotation = quaternion.identity;
		[SerializeField] protected float3 scale = new float3(1, 1, 1);
        [SerializeField] protected bool displayAxis = true;
        [SerializeField] protected bool displayText = true;
        [SerializeField] protected bool displayBound = true;
        [SerializeField] protected Color color = Color.cyan;
		[SerializeField] protected Dictionary<SpaceCorner, float3> localPoints = Space.SpaceCorners;

		public virtual bool3 IsInSpace(float3 point)
        {
			var local = new float3(this.TRS.inverse.MultiplyPoint(point));
			return math.abs(local) < 0.5f;
        }

		public virtual void OnDrawGizmos()
        {
            if(this.displayBound)
            {
				using (new GizmosScope(this.Color, this.TRS))
				{
					Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
				}
            }
            if(this.displayAxis)
            {
				using (new GizmosScope(Color.blue, Matrix4x4.identity))
				{
					Gizmos.DrawLine(this.Center, this.Center + this.Forward * this.Scale.z * 0.5f);
				}
				using (new GizmosScope(Color.red, Matrix4x4.identity))
				{
					Gizmos.DrawLine(this.Center, this.Center + this.Right * this.Scale.x * 0.5f);
				}
				using (new GizmosScope(Color.green, Matrix4x4.identity))
				{
					Gizmos.DrawLine(this.Center, this.Center + this.Up * this.Scale.y * 0.5f);
				}
            }
            if(this.displayText)
            {
                #if UNITY_EDITOR
                var xcenter = new Vector3(0, 0.5f, -0.5f);
                var ycenter = new Vector3(0.5f, 0, -0.5f);
                var zcenter = new Vector3(0.5f, 0.5f, 0);

                if (displayText)
                {
                    var s = new GUIStyle();
                    s.normal.textColor = this.color;
                    UnityEditor.Handles.Label(this.TRS.MultiplyPoint(xcenter), "X = " + this.scale.x * Unit.UnityUnitToWorldMM + "mm", s);
                    UnityEditor.Handles.Label(this.TRS.MultiplyPoint(ycenter), "Y = " + this.scale.y * Unit.UnityUnitToWorldMM + "mm", s);
                    UnityEditor.Handles.Label(this.TRS.MultiplyPoint(zcenter), "Z = " + this.scale.z * Unit.UnityUnitToWorldMM + "mm", s);
                }
                #endif
            }
        }

		public float3 LocalPoint(SpaceCorner corner)
		{
            if(this.localPoints == null) this.localPoints = Space.SpaceCorners;
            return this.localPoints[corner];
		}

		public float3 LocalToWorld(float3 local)
		{
            return this.TRS.MultiplyPoint(local);
		}

		public float3 WorldToLocal(float3 world)
		{
            return this.ToLocal.MultiplyPoint(world);
		}
	}
    // public class Space : MonoBehaviour, ISpace
    // {
    //     public virtual float3 Center
    //     {
    //         get => new float3(this.transform.localPosition) + (this.ZeroBased ? this.size * 0.5f : 0);
    //         set => this.transform.localPosition = value - (this.ZeroBased ? this.size * 0.5f : 0);
    //     }
    //     public quaternion Rotation { get => this.transform.localRotation; set => this.transform.localRotation = value; }
    //     public float3 Scale { get => this.transform.localScale; set => this.transform.localScale = value; }
    //     public float3 Size { get => this.size; set => this.size = value; }
    //     public Matrix4x4 TRS => Matrix4x4.TRS(this.transform.position, this.transform.rotation, this.transform.lossyScale);

    //     public virtual bool ZeroBased => this.zeroBased;

    //     [SerializeField] protected bool zeroBased = false;
    //     [SerializeField] protected float3 size = 1;
    //     [SerializeField] protected Color color = new Color(1, 1, 1, 1);

    //     protected virtual void OnDrawGizmos()
    //     {
    //         using (new GizmosScope(this.color, this.TRS))
    //         {
    //             Gizmos.DrawWireCube(Vector3.zero, this.Size);
    //         }
    //     }
    // }
}