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
        protected T[][] data;

        public Matrix(int row = 0, int col = 0)
        {
            this.Clean();

            if (row <= 0 || col <= 0)
            {
                LogTool.Log("row/col is less than 0", LogLevel.Error);
                return;
            }

            this.data = new T[row][];

            for (var r = 0; r < row; ++r)
            {
                this.data[r] = new T[col];
                for (var c = 0; c < this.data[r].Length; ++c)
                {
                    this.data[r][c] = default;
                }
            }
        }
        public void Clean()
        {
            if (this.data == null) return;

            for (var r = 0; r < this.data.Length; ++r)
            {
                for (var c = 0; c < this.data[r].Length; ++c)
                {
                    this.data[r][c] = default;
                }
            }
        }

        public T this[int row, int col]
        {
            get => this.data[row][col];
            set => this.data[row][col] = value;
        }

        public T[] this[int row]
        {
            get => this.data[row];
            set => this.data[row] = value;
        }

        public int2 Size { get =>new int2(this.data.Length, this.data[0].Length); }
    

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