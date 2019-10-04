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
        public enum CompreeAlgorithm
        {
            Deflate,
            Zstd,
        }
        public static byte[] Compress(byte[] data, CompreeAlgorithm method = CompreeAlgorithm.Deflate, CompressionLevel level = CompressionLevel.Optimal)
        {
            MemoryStream output = new MemoryStream();
            switch (method)
            {
                case CompreeAlgorithm.Deflate:
                    {
                        using (DeflateStream dstream = new DeflateStream(output, level))
                        {
                            dstream.Write(data, 0, data.Length);
                        }
                    }
                    break;
                case CompreeAlgorithm.Zstd:
                    {
                        var opt = new CompressionOptions(CompressionOptions.DefaultCompressionLevel + 10);
                        using (var compressor = new Compressor(opt))
                        {
                            return compressor.Wrap(data);
                        }
                    }
            }
            return output.ToArray();
        }

        public static byte[] Decompress(byte[] data, CompreeAlgorithm method = CompreeAlgorithm.Deflate)
        {
            MemoryStream input = new MemoryStream(data);
            MemoryStream output = new MemoryStream();
            switch(method)
            {
                case CompreeAlgorithm.Deflate:
                    {
                        using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
                        {
                            dstream.CopyTo(output);
                        }
                    }
                    break;
                case CompreeAlgorithm.Zstd:
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