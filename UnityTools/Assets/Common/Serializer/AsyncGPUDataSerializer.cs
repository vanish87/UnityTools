using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityTools.Common
{
    public class AsyncGPUDataSerializer : AsyncGPUDataReader
    {
        [Serializable]
        public struct Parameter
        {
            public int x;
            public int y;
            public bool compressed;
        }
        [Serializable]
        public class FileData
        {
            /// <summary>
            /// for texture, parameter is texture size
            /// for compute buffer, parameter is compute buffer count and stride
            /// </summary>
            public Parameter parameter;
            public byte[] data;
        }

        [SerializeField] protected bool useCompress = true;
        [SerializeField] protected bool savePending = false;
        [SerializeField] protected int currentDataCount = 0;
        [SerializeField] protected uint maxDataCount = 10 * 30; //10 seconds in 30 fps
        [SerializeField] protected string fileName = "TextureArray.data";
        protected Queue<FileData> data = new Queue<FileData>();

        protected override void OnSuccessed(AsyncGPUReadbackRequest readback)
        {
            if (this.data.Count > this.maxDataCount)
            {
                this.data.Dequeue();
            }

            var data = readback.GetData<byte>().ToArray();
            if(this.useCompress) data = CompressTool.Compress(data);
            var para = new Parameter() { x = readback.width, y = readback.height, compressed = this.useCompress };
            this.data.Enqueue(new FileData() { parameter = para, data = data });
        }

        protected override void Update()
        {
            base.Update();
            this.currentDataCount = this.data.Count;
            if (this.savePending)
            {
                this.StartCoroutine(this.SaveToFile());
                this.savePending = false;
            }
        }

        protected IEnumerator SaveToFile()
        {
            yield return 0;
            var filePath = Path.Combine(Application.streamingAssetsPath, DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss_") + this.fileName);
            //var filePath = Path.Combine(Application.streamingAssetsPath, this.fileName);
            FileTool.WriteBinary(filePath, this.data);
        }
    }
}