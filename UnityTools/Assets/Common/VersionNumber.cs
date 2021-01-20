using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityTools.Common
{

    [System.Serializable]
    public class VersionInfo
    {
        public int major = 0;
        public int minor = 0;
        public int fix = 0;
        public string comment;
        public string buildTime;
        public string buildDeviceName;
    }

    #if UNITY_EDITOR
    public class VersionNumber : UnityEditor.Build.IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;
        private readonly string fileName = "Version.txt";

        public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
        {
            var launchers = ObjectTool.FindAllObject<ILauncher>();
            foreach (var l in launchers)
            {
                var env = l.RunTime;
                if (env.appSetting.useVersionNum)
                {
                    var path = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
                    var data = new VersionInfo() {buildTime = DateTime.Now.ToString(), buildDeviceName = SystemInfo.deviceName};
                    if (File.Exists(path))
                    {
                        var comment = env.appSetting.versionInfo.comment;
                        data = FileTool.Read<VersionInfo>(path, FileTool.SerializerType.XML);
                        if(data == null) data = new VersionInfo();
                        data.major = Mathf.Max(data.major, env.appSetting.versionInfo.major);
                        data.minor = Mathf.Max(data.minor, env.appSetting.versionInfo.minor);
                        data.fix = Mathf.Max(data.fix, env.appSetting.versionInfo.fix);
                        data.fix++;
                        data.comment = comment;
                        data.buildTime = DateTime.Now.ToString();
                        data.buildDeviceName = SystemInfo.deviceName;
                    }
                    FileTool.Write(path, data, FileTool.SerializerType.XML);
                    env.appSetting.versionInfo = data;
                }
            }
        }
    }
    #endif
}