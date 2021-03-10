
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Common
{
    public enum DataRecorderMode
    {
        Record,
        Replay,
        Paused,
    }
    public class DataRecorder<T>
    {
        [Serializable]
        public class SaveData
        {
            public DateTime time;
            public Queue<T> current = new Queue<T>();
        }

        public string FileName = "RecordedData";
        public FileTool.SerializerType SaveType = FileTool.SerializerType.Binary;
        public int MaxSize = 1024;
        public DataRecorderMode Mode { get => this.mode; set => this.mode = value; }

        public IEnumerable<T> Data => this.data.current;

        protected SaveData data = new SaveData();
        protected DataRecorderMode mode = DataRecorderMode.Record;
        private object lockObj = new object();

        public DataRecorder(string name)
        {
            this.FileName = name + this.FileName;
        }

        public void Reset(DataRecorderMode mode = DataRecorderMode.Record)
        {
            lock(this.lockObj)
            {
                this.data.current.Clear();
                this.mode = mode;
            }
        }
        public void Record(T data)
        {
            lock (this.lockObj)
            {
                if (this.mode == DataRecorderMode.Record)
                {
                    while (this.data.current.Count > this.MaxSize) this.data.current.Dequeue();
                    this.data.current.Enqueue(data.DeepCopy());
                }
            }
        }

        public T Next()
        {
            lock(this.lockObj)
            {
                if (this.mode == DataRecorderMode.Replay)
                {
                    var next = this.data.current.Dequeue();
                    this.data.current.Enqueue(next.DeepCopy());
                    return next;
                }

                return this.data.current.Peek().DeepCopy();
            }
        }
        public void Save()
        {
            lock(this.lockObj)
            {
                this.data.time = DateTime.Now;
                var path = System.IO.Path.Combine(Application.streamingAssetsPath, this.FileName + ".data");
                FileTool.Write(path, this.data, this.SaveType);
            }
        }

        public void Load()
        {
            lock(this.lockObj)
            {
                var path = System.IO.Path.Combine(Application.streamingAssetsPath, this.FileName + ".data");
                this.data = FileTool.Read<SaveData>(path, this.SaveType);
            }
        }
    }

}