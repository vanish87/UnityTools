using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Common;

namespace UnityTools.Networking
{
    public class NetworkController : MonoBehaviour
    {
        public class NetworkData
        {
            public PCInfo current = new PCInfo();
            public PCInfo server = new PCInfo();
            public List<PCInfo> client = new List<PCInfo>();
        }
        public interface INetworkUser
        {
            void OnInit(NetworkData networkData);
        }
        protected void NotifyUser(NetworkData data)
        {
            var user = ObjectTool.FindAllObject<INetworkUser>();
            foreach (var n in user)
            {
                n.OnInit(data);
            }
        }
    }
}