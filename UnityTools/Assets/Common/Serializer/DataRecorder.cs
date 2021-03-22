
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Attributes;
using UnityTools.Debuging;

namespace UnityTools.Common
{
    public enum DataRecorderMode
    {
        Record,
        Replay,
        Paused,
    }
    public class DataRecorder<T> : MonoBehaviour
    {
        [Serializable]
        protected class SaveData
        {
            public DateTime time;
            public Queue<T> current = new Queue<T>();
        }

        private const string FileExtension = ".data";

        [SerializeField, FileNamePopup(Regex = "*" + FileExtension)] protected string fileName;

        public string FilePrefix = "RecordedData";
        public FileTool.SerializerType SaveType = FileTool.SerializerType.Binary;
        public int MaxSize = 1024;
        public DataRecorderMode Mode { get => this.mode; set => this.mode = value; }

        public IEnumerable<T> Data => this.data.current;

        protected SaveData data = new SaveData();
        protected DataRecorderMode mode = DataRecorderMode.Record;
        private object lockObj = new object();

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
            if(this.data == null || this.data.current.Count == 0) return default;
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
                var fileName = this.FilePrefix + this.data.time.ToString("_yyyy_MM_dd_HH_mm_ss");
                var path = System.IO.Path.Combine(Application.streamingAssetsPath, fileName + FileExtension);
                FileTool.Write(path, this.data, this.SaveType);
            }
        }

        public void Load()
        {
            if(this.fileName == null) 
            {
                LogTool.Log("No file to load", LogLevel.Warning);
                return;
            }
            lock(this.lockObj)
            {
                var path = System.IO.Path.Combine(Application.streamingAssetsPath, this.fileName);
                this.data = FileTool.Read<SaveData>(path, this.SaveType);
            }
        }
    }

}