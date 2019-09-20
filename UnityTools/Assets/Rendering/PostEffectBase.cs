using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityTools.Common;

namespace UnityTools.Rendering
{
    public class PostEffectBase : MonoBehaviour
    {
        [SerializeField] protected Shader shader;
        private AutoMaterial mat;


        private T1 t;


        public class Test
        {
            public Test()
            {
                Debug.Log("con");
            }

            ~Test()
            {
                Debug.Log("decon");

            }
        }

        public class T1 : AutoObject<Material>
        {
            public T1(Shader s) : base(new Material(s))
            {
                Debug.Log("T1 new");
            }
            protected override void DisposeUnmanaged()
            {
                base.DisposeUnmanaged();
                if (Data == null) Debug.Log("data null");

                Data?.DestoryObj();
            }
        }

        protected virtual void Start()
        {
            Assert.IsNotNull(this.shader);
            this.mat = new AutoMaterial(this.shader);

            //t = new T1(shader);
        }

        protected virtual void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            //Graphics.Blit(source, destination, this.mat);
        }
        protected void OnDestroy()
        {
            //t.Dispose();
            this.mat.Dispose();
        }
    }
}