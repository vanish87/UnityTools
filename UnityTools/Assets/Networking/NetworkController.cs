using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Common;
using UnityTools.Debuging;

namespace UnityTools.Networking
{
    public class NetworkController : MonoBehaviour
    {
        public class NetworkData
        {
            public PCInfo current = new PCInfo();
            public PCInfo server = new PCInfo();
            public PCInfo devPC = new PCInfo();
            public List<PCInfo> client = new List<PCInfo>();
        }
        public interface INetworkUser
        {
            void OnInit(NetworkData networkData);
        }
        protected NetworkData GetNetworkData(List<PCInfo> pcList)
        {
            var currentPC = default(PCInfo);
            var currentIPs = Tool.GetLocalIPAddress();
            foreach (var ip in currentIPs)
            {
                var pcData = pcList.FindAll(pc => pc.ipAddress == ip);
                if (pcData != null && pcData.Count > 0)
                {
                    var first = pcData[0];
                    if (pcData.Count > 1)
                    {
                        Debug.LogWarningFormat("Multiple pc ip found, using the first one({0}) for networking", first.name);
                    }

                    currentPC = first;
                    break;
                }
            }
            var serverData = pcList.Find(pc => pc.role == PCInfo.Role.Server);

            if (currentPC == null)
            {
                LogTool.Log("Current pc not found, use default", LogLevel.Warning);
                currentPC = new PCInfo();
            }
            if (serverData == null)
            {
                LogTool.Log("serverData not found, use default", LogLevel.Warning);
                serverData = new PCInfo() { role = PCInfo.Role.Server };
            }
            LogTool.LogFormat("setup current pc ip {1} as {0}", LogLevel.Info, LogChannel.Debug | LogChannel.Network, currentPC.role.ToString(), currentPC.ipAddress);

            var clientData = pcList.FindAll(pc => pc.role == PCInfo.Role.Client && currentIPs.Contains(pc.ipAddress) == false);
            var devData = pcList.Find(pc => pc.role == PCInfo.Role.Development);

            var data = new NetworkData()
            {
                current = currentPC,
                server = serverData,
                devPC = devData,
                client = new List<PCInfo>(clientData)
            };

            return data;
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