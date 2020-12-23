using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityTools.Debuging;
using UnityTools.Rendering;

namespace UnityTools.Common
{
    [System.Serializable]
    public class CircleIndexer
    {
        public CircleIndexer(int size)
        {
            LogTool.LogAssertIsTrue(size > 0, "size should be bigger than 0");
            this.size = size;
            this.current = -1;
        }
        public CircleIndexer(int size, int current)
        {
            LogTool.LogAssertIsTrue(size > 0, "size should be bigger than 0");
            this.size = size;
            this.current = this.GetIndex(current);
        }
        [SerializeField] protected int size = 0;
        [SerializeField] protected int current = 0;
        public int Current { get { return this.current; } }
        public int Size { get { return this.size; } }
        public int GetIndex(int index) { if (index < 0) index += this.size; return index % this.size; }
        public void MoveCurrentToPrev() { this.current = this.GetIndex(this.current - 1); }
        public void MoveCurrentToNext() { this.current = this.GetIndex(this.current + 1); }
    }
    [System.Serializable]
    public abstract class CircleDataBase<T, S> : Disposable
    {
        [SerializeField] protected List<T> dataList = new List<T>();
        [SerializeField] protected CircleIndexer circleIndexer;

        public CircleDataBase(int size = 1, S para = default)
        {
            LogTool.LogAssertIsTrue(size > 0, "size should be bigger than 0");
            this.circleIndexer = new CircleIndexer(size, 0);

            var c = 0;
            while (c++ < size)
            {
                this.dataList.Add(this.OnCreate(para));
            }
        }
        protected abstract T OnCreate(S para);
        public T Current { get { return this[this.circleIndexer.Current]; } }
        public T Next { get { return this[this.circleIndexer.Current + 1]; } }
        public T GetCurrentAndMoveNext
        {
            get
            {
                var ret = this[this.circleIndexer.Current];
                this.circleIndexer.MoveCurrentToNext();
                return ret;
            }
        }

        public void MoveToPrev() { this.circleIndexer.MoveCurrentToPrev(); }
        public void MoveToNext() { this.circleIndexer.MoveCurrentToNext(); }

        public T this[int key]
        {
            get => this.dataList[this.circleIndexer.GetIndex(key)];
            set => this.dataList[this.circleIndexer.GetIndex(key)] = value;
        }
    }

    [System.Serializable]
    public class CircleData<T, S> : CircleDataBase<T, S> where T : new()
    {
        public CircleData(int size = 1, S para = default) : base(size, para) { }
        protected override T OnCreate(S para)
        {
            return new T();
        }
    }

    [System.Serializable]
    public class CircleRT : CircleDataBase<RenderTexture, RenderTextureDescriptor>
    {
        public CircleRT(int size, RenderTextureDescriptor desc) : base(size, desc) { }

        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();

            foreach (var t in this.dataList)
            {
                t.DestoryObj();
            }

            this.dataList.Clear();
        }
        protected override RenderTexture OnCreate(RenderTextureDescriptor desc)
        {
            return TextureManager.Create(desc);
        }
    }

    public class CircleComputeBuffer<S> : CircleDataBase<ComputeBuffer, int>
    {
        public CircleComputeBuffer(int size, int dataSize) : base(size, dataSize) { }

        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();

            foreach (var t in this.dataList)
            {
                t.Release();
            }

            this.dataList.Clear();
        }
        protected override ComputeBuffer OnCreate(int dataSize)
        {
            return new ComputeBuffer(dataSize, Marshal.SizeOf<S>());
        }
    }
}
