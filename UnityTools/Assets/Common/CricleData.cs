using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Debuging;
using UnityTools.Rendering;

namespace UnityTools.Common
{
    [System.Serializable]
    public class CricleIndexer
    {
        public CricleIndexer(int size)
        {
            LogTool.LogAssertIsTrue(size > 0, "size should be bigger than 0");
            this.size = size;
            this.current = -1;
        }
        public CricleIndexer(int size, int current)
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
        public void MoveCurrentToNext() { this.current = this.GetIndex(this.current + 1); }
    }
    [System.Serializable]
    public abstract class CricleDataBase<T, S> : Disposable
    {
        [SerializeField] protected List<T> dataList = new List<T>();
        [SerializeField] protected CricleIndexer cricleIndexer;

        public CricleDataBase(int size = 1, S para = default)
        {
            LogTool.LogAssertIsTrue(size > 0, "size should be bigger than 0");
            this.cricleIndexer = new CricleIndexer(size, 0);

            var c = 0;
            while (c++ < size)
            {
                this.dataList.Add(this.OnCreate(para));
            }
        }
        protected abstract T OnCreate(S para);
        public T Current { get { return this[this.cricleIndexer.Current]; } }
        public T Next { get { return this[this.cricleIndexer.Current + 1]; } }
        public T GetCurrentAndMoveNext
        {
            get
            {
                var ret = this[this.cricleIndexer.Current];
                this.cricleIndexer.MoveCurrentToNext();
                return ret;
            }
        }

        public void MoveToNext() { this.cricleIndexer.MoveCurrentToNext(); }

        public T this[int key]
        {
            get => this.dataList[this.cricleIndexer.GetIndex(key)];
            set => this.dataList[this.cricleIndexer.GetIndex(key)] = value;
        }
    }


    public class CricleData<T, S> : CricleDataBase<T, S> where T : new()
    {
        protected override T OnCreate(S para)
        {
            return new T();
        }
    }

    [System.Serializable]
    public class CricleRT : CricleDataBase<RenderTexture, RenderTextureDescriptor>
    {
        public CricleRT(int size, RenderTextureDescriptor desc) : base(size, desc) { }

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
}
