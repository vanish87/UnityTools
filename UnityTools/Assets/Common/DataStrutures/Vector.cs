using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityTools.Debuging;

namespace UnityTools.Common
{
    [System.Serializable]
    public class Vector<T> : IEnumerable<T>
    {
        private T[] data;

        public Vector(int size = 0)
        {
            if (size <= 0)
            {
                LogTool.Log("row/col is less than 0", LogLevel.Error);
                return;
            }

            this.data = new T[size];

            this.Clear();
        }
        public void Clear()
        {
            if (this.data == null) return;

            for (var r = 0; r < this.data.Length; ++r)
            {
                this.data[r] = default;
            }
        }

        public T this[int index]
        {
            get => this.data[index];
            set => this.data[index] = value;
        }

        public int Size { get =>this.data.Length; }    

        public void Print(string nullString = "0")
        {
            var str = "\n";
            foreach (var r in this.data)
            {
                str += (r.ToString()) + " ";
                str += "\n";
            }

            LogTool.Log(str);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.data.GetEnumerator();
        }
    }
}