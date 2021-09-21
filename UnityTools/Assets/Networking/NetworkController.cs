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
            public List<PCInfo> servers = new List<PCInfo>();
            public List<PCInfo> devPCs = new List<PCInfo>();
            public List<PCInfo> clients = new List<PCInfo>();

            public void OnGUIDraw()
            {
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label("Curret");
                    this.current.OnGUIDraw();
                }
                GUILayout.Label("Servers " + this.servers.Count);
                foreach (var s in this.servers) s.OnGUIDraw();
                GUILayout.Label("Clients " + this.clients.Count);
                foreach (var c in this.clients) c.OnGUIDraw();
                GUILayout.Label("DevPCs " + this.devPCs.Count);
                foreach (var d in this.devPCs) d.OnGUIDraw();
            }
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

            if (currentPC == null)
            {
                var pcData = pcList.FindAll(pc => pc.name == SystemInfo.deviceName);
                if (pcData != null && pcData.Count > 0)
                {
                    currentPC = pcData[0];
					currentPC.ipAddress = currentIPs[0];
					LogTool.Log("Use pc name " + currentPC.name + " " + currentPC.ipAddress.ToString() +" as Current PC", LogLevel.Dev);
                }
                else
                {
					currentPC = new PCInfo();
					currentPC.name = SystemInfo.deviceName;
					currentPC.ipAddress = currentIPs[0];
					currentPC.role = PCInfo.Role.Development;
					LogTool.Log("Current pc not found, use current ip and Set " + currentPC.name + " to Dev" + currentPC.ipAddress, LogLevel.Warning);
                }
            }
            
            var serverData = pcList.FindAll(pc => pc.role == PCInfo.Role.Server);
            if (serverData == null)
            {
                LogTool.Log("serverData not found, add a default one", LogLevel.Warning);
                serverData = new List<PCInfo>() { new PCInfo() { role = PCInfo.Role.Server } };
            }
            LogTool.LogFormat("setup current pc ip {1} as {0}", LogLevel.Info, LogChannel.Debug | LogChannel.Network, currentPC.role.ToString(), currentPC.ipAddress);

            var clientData = pcList.FindAll(pc => pc.role == PCInfo.Role.Client && currentIPs.Contains(pc.ipAddress) == false);
            var devData = pcList.FindAll(pc => pc.role == PCInfo.Role.Development);

            var data = new NetworkData()
            {
                current = currentPC,
                servers = serverData,
                devPCs = devData,
                clients = new List<PCInfo>(clientData)
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