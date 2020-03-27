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
            public List<LevelData> levels = new List<LevelData>();
            public List<ChannelData> chanels = new List<ChannelData>();
        }
        [Serializable]
        public class LevelData
        {
            public LogLevel level;
            public bool enabled = true;
        }
        [Serializable]
        public class ChannelData
        {
            public LogChannel channel;
            public bool enabled = true;
        }
        public void SetupChannel()
        {
            this.Initialize();

            if (this.Data.chanels.Count == 0)
            {
                LogTool.Log("Add all log Channels", LogLevel.Warning);
                foreach (LogLevel log in Enum.GetValues(typeof(LogLevel)))
                {
                    this.Data.levels.Add(new LevelData() { level = log, enabled = true });
                }

                foreach (LogChannel log in Enum.GetValues(typeof(LogChannel)))
                {
                    this.Data.chanels.Add(new ChannelData() { channel = log, enabled = true });
                }
            }


            foreach (LogLevel log in Enum.GetValues(typeof(LogLevel)))
            {
                if (this.Data.levels.FindAll(c => c.level == log).Count != 1)
                {
                    LogTool.Log("Missing/Duplicate log level configure " + log.ToString(), LogLevel.Warning);
                }
            }

            foreach (LogChannel log in Enum.GetValues(typeof(LogChannel)))
            {
                if (this.Data.chanels.FindAll(c => c.channel == log).Count != 1)
                {
                    LogTool.Log("Missing/Duplicate log chanels configure " + log.ToString(), LogLevel.Warning);
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
            foreach (var log in this.Data.levels)
            {
                LogTool.Enable(log.level, log.enabled);
            }
            foreach (var log in this.Data.chanels)
            {
                LogTool.Enable(log.channel, log.enabled);
            }
        }

        protected override void Update()
        {
            base.Update();
            this.UpdateLog();
        }
    }
}
