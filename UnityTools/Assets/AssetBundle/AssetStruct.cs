/********************************************************************
	Created:		2018/05/22
	Author:			Li Yuan
	Email:			liyuan@team-lab.com
	
	Description:	This class defines data format that used to store json file
                    received from websocket server
*********************************************************************/
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace UnityTools.AssetBundle
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
