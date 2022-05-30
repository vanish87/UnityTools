using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Common;

namespace UnityTools.Rendering
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
	public class FieldRender : MonoBehaviour
	{
		public Texture MainTexture { set => this.mainTexture = value; }
		public float Scale { set => this.scale = value; }
        [SerializeField] protected Mesh quad;
        // [SerializeField] protected Shader shader;
        [SerializeField] protected float scale = 1;
		[SerializeField] protected ShaderMaterial material;
        protected MeshRenderer meshRenderer;
        protected MeshFilter meshFilter;
        protected IFieldTexture field;
		protected IFieldTexture Field => this.field ??= this.GetComponent<IFieldTexture>() ?? this.GetComponentInParent<IFieldTexture>();
        protected Texture mainTexture;

        protected void OnEnable()
        {
            this.material.Init();
            this.meshRenderer = this.GetComponent<MeshRenderer>();
            this.meshFilter = this.GetComponent<MeshFilter>();
            this.meshRenderer.material = this.material;
            this.meshFilter.sharedMesh = this.quad;
        }
        protected virtual void Update()
        {
            this.material.UpdateShaderCommand();
            if(this.mainTexture != null || this.Field?.FieldAsTexture != null)
            {
                var tex = this.mainTexture??this.Field?.FieldAsTexture;
				this.material.Mat.mainTexture = tex;
				this.transform.localScale = new Vector3(tex.width * 1.0f / tex.height, 1, 1) * scale;
            }
        }
        protected void OnDisable()
        {
            this.material?.Deinit();
        }

	}
}
