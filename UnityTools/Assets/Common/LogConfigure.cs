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
                if(c.Data.remoteLogPC.Find(pc=>currentIPs.Contains(pc.ipAddress)) != null) continue;
                var port = new PCInfo.Port() { name = "remoteLog", port = c.Data.remoteLogPort};
                foreach(var pc in currentIPs)
                {
                    c.Data.remoteLogPC.Add(new PCInfo() 
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
    public class LogConfigure : Config<LogConfigure.LogConfigureData>
    {
        [SerializeField] protected string fileName = "LogConfigureData.xml";
        [SerializeField] protected LogConfigureData data;
        protected override string filePath { get { return System.IO.Path.Combine(Application.streamingAssetsPath, this.fileName); } }
        public override LogConfigureData Data { get => this.data; set=>this.data = value; }

        [Serializable]
        public class LogConfigureData
        {
            public short remoteLogPort = 13210;
            public List<PCInfo> remoteLogPC = new List<PCInfo>();
            public LogChannel channel;
            public List<LevelData> levels = new List<LevelData>();
        }
        [Serializable]
        public class LevelData
        {
            public LogLevel level;
            public bool enabled = true;
        }
        public void SetupChannel()
        {
            this.Initialize();

            if (this.Data.levels.Count == 0)
            {
                LogTool.Log("No log level found, Add all log Channels by default", LogLevel.Warning);
                foreach (LogLevel log in Enum.GetValues(typeof(LogLevel)))
                {
                    this.Data.levels.Add(new LevelData() { level = log, enabled = true });
                }
            }


            foreach (LogLevel log in Enum.GetValues(typeof(LogLevel)))
            {
                if (this.Data.levels.FindAll(c => c.level == log).Count != 1)
                {
                    LogTool.Log("Missing/Duplicate log level configure " + log.ToString(), LogLevel.Warning);
                }
            }

            this.UpdateLog();
        }

        public void SetupLog()
        {
            LogToolNetwork.LogToolNetworkSocket.SetupNetwork(this.Data.remoteLogPC, this.Data.remoteLogPort);
        }
        protected override void Start()
        {
            base.Start();
            this.SetupChannel();
        }

        protected void UpdateLog()
        {
            foreach (var log in this.Data.levels)
            {
                LogTool.Enable(log.level, log.enabled);
            }
            
            LogTool.Enable(this.Data.channel);
        }

        protected override void Update()
        {
            base.Update();
            this.UpdateLog();
        }
    }
}
