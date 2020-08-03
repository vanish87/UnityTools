using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityTools.Debuging;

namespace UnityTools.Common
{
    [System.Serializable]
    public class Matrix<T>
    {
        protected Vector<T>[] data;

        public Matrix(int row = 0, int col = 0)
        {
            if (row <= 0 || col <= 0)
            {
                LogTool.Log("row/col is less than 0", LogLevel.Error);
                return;
            }

            this.data = new Vector<T>[row];

            for (var r = 0; r < row; ++r)
            {
                this.data[r] = new Vector<T>(col);
            }
        }
        public void Clear()
        {
            if (this.data == null) return;

            for (var r = 0; r < this.data.Length; ++r)
            {
                this.data[r].Clear();
            }
        }

        public T this[int row, int col]
        {
            get => this.data[row][col];
            set => this.data[row][col] = value;
        }

        public Vector<T> this[int row]
        {
            get => this.data[row];
            set => this.data[row] = value;
        }

        public int2 Size { get =>new int2(this.data.Length, this.data[0].Size); }
    

        public void Print(string nullString = "0")
        {
            var str = "\n";
            foreach (var r in this.data)
            {
                foreach(var c in r)
                {
                    str += (c == null ? nullString : c.ToString()) + " ";
                }
                str += "\n";
            }

            LogTool.Log(str);
        }
    }
}