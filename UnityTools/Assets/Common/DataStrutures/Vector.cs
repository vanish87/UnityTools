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

        public static Vector<T> operator +(Vector<T> a, Vector<T> b)
        {
            LogTool.LogAssertIsTrue(a.Size == b.Size, "Vector size is not same");

            var ret = new Vector<T>(a.Size);
            for(var i = 0; i < a.Size; ++i)
            {
                dynamic lhs = a[i];
                dynamic rhs = b[i];
                ret[i] = lhs + rhs;

            }
            return ret;
        }

        public static Vector<T> operator -(Vector<T> a, Vector<T> b)
        {
            LogTool.LogAssertIsTrue(a.Size == b.Size, "Vector size is not same");

            var ret = new Vector<T>(a.Size);
            for (var i = 0; i < a.Size; ++i)
            {
                dynamic lhs = a[i];
                dynamic rhs = b[i];
                ret[i] = lhs - rhs;

            }
            return ret;
        }
        public static Vector<T> operator *(float b, Vector<T> a)
        {
            return a * b;
        }

        public static Vector<T> operator *(Vector<T> a, float b)
        {
            dynamic rhs = b;
            var ret = new Vector<T>(a.Size);
            for (var i = 0; i < a.Size; ++i)
            {
                dynamic lhs = a[i];
                ret[i] = lhs * rhs;

            }
            return ret;
        }
        public static Vector<T> operator /(Vector<T> a, float b)
        {
            dynamic rhs = b;
            LogTool.LogAssertIsTrue(rhs != 0, "Vector size is not same");
            var ret = new Vector<T>(a.Size);
            for (var i = 0; i < a.Size; ++i)
            {
                dynamic lhs = a[i];
                ret[i] = lhs / rhs;

            }
            return ret;
        }

        public static Vector<T> Concatenation(Vector<T> lhs, Vector<T> rhs)
        {
            var ret = new Vector<T>(lhs.Size + rhs.Size);
            var i = 0;
            foreach (var v in lhs) ret[i++] = v;
            foreach (var v in rhs) ret[i++] = v;
            return ret;
        }

        public Vector(int size = 1)
        {
            if (size <= 0)
            {
                LogTool.Log("row/col is less than 0", LogLevel.Error);
                return;
            }

            this.data = new T[size];

            this.Clear();
        }
        public Vector(T[] data)
        {
            this.data = data.DeepCopy();
        }
        public Vector(List<T> data)
        {
            this.data = data.ToArray().DeepCopy();
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