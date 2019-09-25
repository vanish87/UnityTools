using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace UnityTools.Common
{
    public class AssetStruct<T,S> : ScriptableObject
    {
        protected T data;

        public virtual void UpdateFields(S data)
        {

        }

        protected MemoryStream GetStream(bool keepOpen = false)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, data);
            if (!keepOpen)
            {
                ms.Close();
            }
            return ms;
        }
    }
}
