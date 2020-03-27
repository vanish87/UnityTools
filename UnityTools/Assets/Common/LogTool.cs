using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Debuging
{
    public enum LogLevel
    {
        None = 0,
        Error,
        Warning,
        Network,
        Verbose,
        Info,
        Debug,
        Dev,
    }
    public class LogTool
    {
        public static LogLevel Current { set => logFlag = value; }

        protected static Dictionary<LogLevel, bool> enableList = new Dictionary<LogLevel, bool>();
        protected static LogLevel logFlag = LogLevel.Debug;

        public static void EnableAll(bool enabled)
        {
            foreach (LogLevel e in Enum.GetValues(typeof(LogLevel)))
            {
                Enable(e, enabled);
            }
        }
        public static void Enable(LogLevel level, bool enabled)
        {
            if (enableList.ContainsKey(level)) enableList[level] = enabled;
            else enableList.Add(level, enabled);
        }
        public static void Log(LogLevel level, string message)
        {
            var hasKey = enableList.ContainsKey(level);
            var enabled = !hasKey || (hasKey && enableList[level]);
            if (level <= logFlag && enabled)
            {
                switch (level)
                {
                    case LogLevel.Warning:
                        {
                            Debug.LogWarning("[" + level.ToString() + "]" + ":" + message);
                        }
                        break;
                    case LogLevel.Error:
                        {
                            Debug.LogError("[" + level.ToString() + "]" + ":" + message);
                        }
                        break;
                    default:
                        {
                            Debug.Log("[" + level.ToString() + "]" + ":" + message);
                        }
                        break;
                }
            }
        }
        public static void LogFormat(LogLevel level, string format, params object[] args)
        {
            var hasKey = enableList.ContainsKey(level);
            var enabled = !hasKey || (hasKey && enableList[level]);
            if (level <= logFlag && enabled)
            {
                switch (level)
                {
                    case LogLevel.Warning:
                        {
                            Debug.LogWarningFormat("[" + level.ToString() + "]" + ":" + format, args);
                        }
                        break;
                    case LogLevel.Error:
                        {
                            Debug.LogErrorFormat("[" + level.ToString() + "]" + ":" + format, args);
                        }
                        break;
                    default:
                        {
                            Debug.LogFormat("[" + level.ToString() + "]" + ":" + format, args);
                        }
                        break;
                }
            }
        }
    }
}
