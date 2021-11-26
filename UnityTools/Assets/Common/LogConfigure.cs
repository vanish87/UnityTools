using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Common;
using UnityTools.Networking;

namespace UnityTools.Debuging
{
    #if UNITY_EDITOR
    public class LogConfigureBuildEvent : UnityEditor.Build.IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            var currentIPs = Tool.GetLocalIPAddress();
            foreach(var c in ObjectTool.FindAllObject<LogConfigure>())
            {
                if(c.D.remoteLogPC.Find(pc=>currentIPs.Contains(pc.ipAddress)) != null) continue;
                var port = new PCInfo.Port() { name = "remoteLog", port = c.D.remoteLogPort};
                foreach(var pc in currentIPs)
                {
                    c.D.remoteLogPC.Add(new PCInfo() 
                    { 
                        name = SystemInfo.deviceName, 
                        ipAddress = pc, 
                        ports = new List<PCInfo.Port>(){port}, 
                        role = PCInfo.Role.Development
                    });

                    LogTool.Log("Add dev pc into log configure " + pc.ToString(), LogLevel.Verbose, LogChannel.Debug);
                }
                c.Save();
            }
        }
    }

    #endif
    #if USE_EDITOR_EXC
    [ExecuteInEditMode]
    #endif
    public class LogConfigure : Configure<LogConfigure.LogConfigureData>
    {
        [Serializable]
        public class LogConfigureData
        {
            public short remoteLogPort = 13210;
            public List<PCInfo> remoteLogPC = new List<PCInfo>();
            public LogChannel channel;
            public LogLevel level;
        }
        public void SetupChannel()
        {
            if (this.D.level == LogLevel.None)
            {
                LogTool.Log("No log level found, set to Everything by default", LogLevel.Warning);
                this.D.level = LogLevel.Everything;
            }
            this.UpdateLog();
        }

        public void SetupLog()
        {
            LogToolNetwork.LogToolNetworkSocket.SetupNetwork(this.D.remoteLogPC, this.D.remoteLogPort);
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            this.SetupChannel();
        }

        protected void UpdateLog()
        {
            LogTool.Enable(this.D.level);
            LogTool.Enable(this.D.channel);
        }

        protected override void Update()
        {
            base.Update();
            this.UpdateLog();
        }
    }
}
