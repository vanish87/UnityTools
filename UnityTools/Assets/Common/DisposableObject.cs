using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Common
{
    public class DisposableObject<T> : Disposable
    {
        public T Data { get => this.data; set => this.data = value; }
        [SerializeField] protected T data;

        public DisposableObject(T data)
            : base()
        {
            this.data = data;
        }
    }

    public class DisposableRenderTexture : DisposableObject<RenderTexture>
    {
        public DisposableRenderTexture(RenderTexture data) : base(data)
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
        public static implicit operator DisposableRenderTexture(RenderTexture data)
        {
            return new DisposableRenderTexture(data);
        }
        public static implicit operator RenderTexture(DisposableRenderTexture source)
        {
            return source.Data;
        }
    }
    public class DisposableTexture2D : DisposableObject<Texture2D>
    {
        public DisposableTexture2D(Texture2D data) : base(data)
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
        public static implicit operator DisposableTexture2D(Texture2D data)
        {
            return new DisposableTexture2D(data);
        }
        public static implicit operator Texture2D(DisposableTexture2D source)
        {
            return source.Data;
        }
    }

    public class DisposableMaterial : DisposableObject<Material>
    {
        public DisposableMaterial(Material data) : base(data)
        {
        }

        public DisposableMaterial(Shader shader) : base(new Material(shader))
        {

        }

        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();
            data?.DestoryObj();
        }
        public static implicit operator DisposableMaterial(Material data)
        {
            return new DisposableMaterial(data);
        }
        public static implicit operator Material(DisposableMaterial source)
        {
            return source.Data;
        }
    }
}
