
using System;
using Unity.Mathematics;
using UnityEngine;
using UnityTools.Common;
using UnityTools.Rendering;

namespace UnityTools.Interaction
{
	[System.Serializable]
	public class TouchMessage : EventArgs
	{
		public int id;
		public float3 worldPos;
		public float2 uv;
	}
	public class TouchMesh : MonoEvent
	{
		[SerializeField] protected bool displayDebug = false;
		[SerializeField] protected float scale = 1;
		protected GameObject debug;
		protected GameObject Debug
		{
			get
			{
				if (this.debug == null)
				{
					var desc = GeometryTool.GeometryDescriptor.EmptyGeometry(PrimitiveType.Sphere);
					this.debug = GeometryTool.Create(desc);
					// this.debug.transform.parent = this.transform;
				}
				return this.debug;
			}
		}
		protected void OnEnable()
		{
			var filter = this.GetComponent<MeshFilter>();
			if (filter != null)
			{
				var collider = this.gameObject.FindOrAddTypeInComponentsAndChildren<MeshCollider>();
				collider.sharedMesh = filter.mesh;
			}

			foreach(var user in ObjectTool.FindAllObject<IEventUser>())
			{
				user.OnSenderAdded(this);
			}

		}

		protected void OnDisable()
		{
			this.debug?.DestoryObj();
			
			foreach(var user in ObjectTool.FindAllObject<IEventUser>())
			{
				user.OnSenderRemoved(this);
			}
		}

		protected void Update()
		{
			if (Input.GetMouseButton(0))
			{
				RaycastHit hit;
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				if (UnityEngine.Physics.Raycast(ray, out hit))
				{
					var worldPos = hit.point;
					var uv = hit.textureCoord;

					var message = new TouchMessage();
					message.worldPos = worldPos;
					message.uv = uv;
					this.OnMessage(this, message);

					if (this.displayDebug)
					{
						this.Debug.transform.localPosition = worldPos;
						this.Debug.transform.localScale = new Vector3(this.scale, this.scale, this.scale);
						this.Debug.SetActive(true);
					}
				}
			}
			if(Input.GetMouseButtonUp(0))
			{
				this.Debug.SetActive(false);
			}

		}

	}
}