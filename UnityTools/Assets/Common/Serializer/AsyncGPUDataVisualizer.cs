using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityTools.Rendering;

namespace UnityTools.Common
{
    public class AsyncGPUDataVisualizer : MonoBehaviour
    {
        [SerializeField] protected string fileNmae;
        [SerializeField] protected List<Texture> dataList = new List<Texture>();
        protected void Start()
        {
            this.StartCoroutine(this.ReadData());
        }

        protected IEnumerator ReadData()
        {
            yield return null;

            foreach(var t in this.dataList)
            {
                t.DestoryObj();
            }

            this.dataList.Clear();

            var filePath = Path.Combine(Application.streamingAssetsPath, this.fileNmae);
            var fileData = FileTool.ReadBinary<Queue<AsyncGPUDataSerializer.FileData>>(filePath);

            foreach (var d in fileData)
            {
                var newTex = TextureManager.Create(d.parameter.x, d.parameter.y, TextureFormat.RGBA32, false);
                var data = d.parameter.compressed?CompressTool.Decompress(d.data):d.data;
                newTex.LoadRawTextureData(data);
                newTex.Apply();
                this.dataList.Add(newTex);
                yield return null;
            }
        }
    }
}