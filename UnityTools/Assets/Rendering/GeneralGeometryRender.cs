
using UnityEngine;
using UnityTools.Common;
using UnityTools.Interaction;

namespace UnityTools.Rendering
{
	public interface IGeneralGeometryProvier
	{
		bool AutoSpace { get; }
		ISpace Space { get; }
		bool EnableTouch { get; }

		bool AutoTextureSize { get; }
		Texture MainTexture { get; }

		PrimitiveType GeometryType { get; }

	}
	public class GeneralGeometryRender : MonoBehaviour, IInitialize
	{
		public const string DEFAULT_SHADER_NAME = "UnityTools/GeneralShader";
		public bool Inited => this.inited;
		[SerializeField] protected ShaderMaterial material;
		protected bool inited = false;
		protected IGeneralGeometryProvier Provier => this.provier ??= this.GetComponent<IGeneralGeometryProvier>() ?? this.GetComponentInParent<IGeneralGeometryProvier>();
		protected IGeneralGeometryProvier provier;
		protected TouchMesh touchMesh;

		protected GameObject primitiveInstance;

		public virtual void Init(params object[] parameters)
		{
			if (this.Inited) return;

			this.material.defaultShaderName ??= DEFAULT_SHADER_NAME;
			this.material.Init();
			this.inited = true;
		}
		public virtual void Deinit(params object[] parameters)
		{
			this.material?.Deinit();
			this.primitiveInstance?.DestoryObj();
			this.primitiveInstance = null;
			this.inited = false;
		}

		protected void Update()
		{
			this.material.UpdateShaderCommand();
			if (this.Provier != null)
			{
				if (this.primitiveInstance == null)
				{
					this.primitiveInstance = GameObject.CreatePrimitive(this.Provier.GeometryType);
					this.primitiveInstance.transform.parent = this.transform;
					this.primitiveInstance.transform.localPosition = Vector3.zero;
					this.primitiveInstance.transform.localRotation = Quaternion.identity;
					this.primitiveInstance.transform.localScale = Vector3.one;
					this.primitiveInstance.layer = this.gameObject.layer;
					var render = this.primitiveInstance.GetComponent<MeshRenderer>();
					render.sharedMaterial = this.material;
				}
				if (this.Provier.AutoSpace)
				{
					Common.Space.SetGameObjectToSpace(this.gameObject, this.Provier.Space);
				}
				if (this.Provier.EnableTouch && this.touchMesh == null)
				{
					this.touchMesh = this.gameObject.FindOrAddTypeInComponentsAndChildren<TouchMesh>();
				}
				if (this.Provier.MainTexture != null)
				{
					var tex = this.Provier.MainTexture;
					Material mat = this.material;
					mat.mainTexture = tex;

					if (this.Provier.AutoTextureSize)
					{
						var aspect = 1f * tex.width / tex.height;
						var scale = this.Provier.Space.Scale;
						this.gameObject.transform.localScale = new Vector3(aspect * scale.y, scale.y, scale.z);
					}
				}
			}
		}

		protected virtual void OnEnable()
		{
			this.Init();
		}
		protected virtual void OnDisable()
		{
			this.Deinit();
		}
	}
}