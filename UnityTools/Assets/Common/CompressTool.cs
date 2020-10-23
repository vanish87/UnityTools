using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using ZstdNet;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace UnityTools.Common
{
    public class CompressTool
    {
        public enum CompressAlgorithm
        {
            Deflate,
            Zstd,
        }
        public static byte[] Compress(byte[] data, CompressAlgorithm method = CompressAlgorithm.Deflate, CompressionLevel level = CompressionLevel.Optimal)
        {
            MemoryStream output = new MemoryStream();
            switch (method)
            {
                case CompressAlgorithm.Deflate:
                    {
                        using (DeflateStream dstream = new DeflateStream(output, level))
                        {
                            dstream.Write(data, 0, data.Length);
                        }
                    }
                    break;
                case CompressAlgorithm.Zstd:
                    {
                        var opt = new CompressionOptions(CompressionOptions.DefaultCompressionLevel);
                        using (var compressor = new Compressor(opt))
                        {
                            return compressor.Wrap(data);
                        }
                    }
            }
            return output.ToArray();
        }

        public static byte[] Decompress(byte[] data, CompressAlgorithm method = CompressAlgorithm.Deflate)
        {
            MemoryStream input = new MemoryStream(data);
            MemoryStream output = new MemoryStream();
            switch(method)
            {
                case CompressAlgorithm.Deflate:
                    {
                        using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
                        {
                            dstream.CopyTo(output);
                        }
                    }
                    break;
                case CompressAlgorithm.Zstd:
                    {
                        using (var decompressor = new Decompressor())
                        {
                            return decompressor.Unwrap(data);
                        }
                    }
            }
            return output.ToArray();
        }
    }
}