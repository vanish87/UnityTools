using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Common
{
    public class AutoObject<T> : Disposable
    {
        public T Data { get => this.data; set => this.data = value; }
        [SerializeField] protected T data;

        public AutoObject(T data)
            : base()
        {
            this.data = data;
        }
    }

    public class AutoRenderTexture : AutoObject<RenderTexture>
    {
        public AutoRenderTexture(RenderTexture data) : base(data)
        {
        }

        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();
            if (data != null)
            {
                data.DestoryObj();
            }
        }
        public static implicit operator AutoRenderTexture(RenderTexture data)
        {
            return new AutoRenderTexture(data);
        }
        public static implicit operator RenderTexture(AutoRenderTexture source)
        {
            return source.Data;
        }
    }
    public class AutoTexture2D : AutoObject<Texture2D>
    {
        public AutoTexture2D(Texture2D data) : base(data)
        {
        }

        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();
            if (data != null)
            {
                data.DestoryObj();
            }
        }
        public static implicit operator AutoTexture2D(Texture2D data)
        {
            return new AutoTexture2D(data);
        }
        public static implicit operator Texture2D(AutoTexture2D source)
        {
            return source.Data;
        }
    }

    public class AutoMaterial : AutoObject<Material>
    {
        public AutoMaterial(Material data) : base(data)
        {
        }

        public AutoMaterial(Shader shader) : base(new Material(shader))
        {
            Debug.Log("AutoMaterial");
        }

        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();
            if (Data == null) Debug.Log("data null");

            data?.DestoryObj();
        }
        public static implicit operator AutoMaterial(Material data)
        {
            return new AutoMaterial(data);
        }
        public static implicit operator Material(AutoMaterial source)
        {
            return source.Data;
        }
    }
}
