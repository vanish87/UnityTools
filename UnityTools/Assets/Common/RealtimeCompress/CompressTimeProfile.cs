using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;

namespace UnityTools.Common
{
    public class CompressTimeProfile : MonoBehaviour
    {
        [SerializeField] protected string fileNmae;
        protected List<AsyncGPUDataSerializer.FileData> dataList = new List<AsyncGPUDataSerializer.FileData>();
        protected void Start()
        {
            this.StartCoroutine(this.ReadData());
            this.StartCoroutine(this.Profile());
        }

        protected IEnumerator ReadData()
        {
            yield return null;

            var filePath = Path.Combine(Application.streamingAssetsPath, this.fileNmae);
            this.dataList = FileTool.ReadBinary<Queue<AsyncGPUDataSerializer.FileData>>(filePath).ToList();
        }

        protected IEnumerator Profile()
        {
            yield return 0;
            while (true)
            {
                if (this.dataList.Count > 0)
                {
                    int count = this.dataList.Count;
                    long totalCompress = 0;
                    long totalDecompress = 0;
                    foreach (var file in this.dataList)
                    {
                        var timer = Stopwatch.StartNew();
                        var data = CompressTool.Decompress(file.data);
                        timer.Stop();
                        UnityEngine.Debug.LogFormat("Decompress costs {0}", timer.ElapsedMilliseconds);
                        totalDecompress += timer.ElapsedMilliseconds;

                        timer = Stopwatch.StartNew();
                        data = CompressTool.Compress(data);
                        timer.Stop();
                        UnityEngine.Debug.LogFormat("Compress costs {0}", timer.ElapsedMilliseconds);
                        totalCompress += timer.ElapsedMilliseconds;
                        yield return 0;
                    }

                    UnityEngine.Debug.LogFormat("Total {2} costs {0}, {1}", totalCompress, totalDecompress, count);
                    UnityEngine.Debug.LogFormat("30 frame time costs {0}, {1}", totalCompress/30.0f/count, totalDecompress/30.0f/count);
                }
            }
        }
    }
}