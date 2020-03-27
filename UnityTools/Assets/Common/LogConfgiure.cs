using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Debuging
{
    #if USE_EDITOR_EXC
    [ExecuteInEditMode]
    #endif
    public class LogConfgiure : Config<LogConfgiure.LogConfigureData>
    {
        [SerializeField] protected string fileName = "LogConfigureData.xml";
        [SerializeField] protected LogConfigureData data;
        protected override string filePath { get { return System.IO.Path.Combine(Application.streamingAssetsPath, this.fileName); } }
        public override LogConfigureData Data { get => this.data; set=>this.data = value; }

        [Serializable]
        public class LogConfigureData
        {
            public List<LogChannel> chanels = new List<LogChannel>();
        }
        [Serializable]
        public class LogChannel
        {
            public LogLevel level;
            public bool enabled = true;
        }
        public void SetupChannel()
        {
            this.Initialize();

            if (this.Data.chanels.Count == 0)
            {
                LogTool.Log(LogLevel.Warning, "Add all log Channels");
                foreach (LogLevel log in Enum.GetValues(typeof(LogLevel)))
                {
                    this.Data.chanels.Add(new LogChannel() { level = log, enabled = true });
                }
            }
            foreach (LogLevel log in Enum.GetValues(typeof(LogLevel)))
            {
                if (this.Data.chanels.FindAll(c => c.level == log).Count != 1)
                {
                    LogTool.Log(LogLevel.Warning, "Missing/Duplicate log level configure " + log.ToString());
                }
            }
            this.UpdateLog();
        }
        protected override void Start()
        {
            base.Start();
            this.SetupChannel();
        }

        protected void UpdateLog()
        {
            foreach (var log in this.Data.chanels)
            {
                LogTool.Enable(log.level, log.enabled);
            }

        }
        protected override void Update()
        {
            base.Update();
            this.UpdateLog();
        }
    }
}
