using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace UnityTools.Common
{
    public class Serilization
    {
        // Convert an object to a byte array
        public static byte[] ObjectToByteArray<T>(T obj)
        {
            if (obj == null)
            {
                Debug.LogWarning("obj is null");
                return default;
            }
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        // Convert a byte array to an Object
        public static T ByteArrayToObject<T>(byte[] arrBytes)
        {
            if (arrBytes.Length == 0)
            {
                Debug.LogWarning("Byte array length is " + arrBytes.Length);
                return default;
            }
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = (T)binForm.Deserialize(memStream);
                return obj;
            }
        }

        public static double ConvertFrom2019()
        {
            DateTime origin = new DateTime(2019, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = DateTime.UtcNow - origin;
            return diff.TotalMilliseconds;
        }
    }
}