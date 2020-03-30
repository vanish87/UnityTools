using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTools.Debuging
{
    public enum LogLevel
    {
        Error,
        Warning,
        Info,
        Verbose,
        Dev,
    }
    public enum LogChannel
    {
        Debug,
        Network,
        IO,
    }
    public class LogTool
    {
        protected static Dictionary<LogLevel, bool> levelList = new Dictionary<LogLevel, bool>();
        protected static Dictionary<LogChannel, bool> chanelList = new Dictionary<LogChannel, bool>();

        public static void Enable(LogLevel level, bool enabled)
        {
            if (levelList.ContainsKey(level)) levelList[level] = enabled;
            else levelList.Add(level, enabled);
        }
        public static void Enable(LogChannel channel, bool enabled)
        {
            if (chanelList.ContainsKey(channel)) chanelList[channel] = enabled;
            else chanelList.Add(channel, enabled);
        }
        public static void Log(string message, LogLevel level = LogLevel.Verbose, LogChannel channel = LogChannel.Debug)
        {
            if (levelList.ContainsKey(level) && !levelList[level]) return;
            if (chanelList.ContainsKey(channel) && !chanelList[channel]) return;

            var msg = FormatMessage(message, level, channel);
            switch(level)
            {
                case LogLevel.Error: Debug.LogError(msg);break;
                case LogLevel.Warning: Debug.LogWarning(msg);break;
                case LogLevel.Verbose:
                case LogLevel.Info:
                case LogLevel.Dev:
                default: Debug.Log(msg); break;
            }
        }
        public static void LogFormat(string format, LogLevel level = LogLevel.Verbose, LogChannel channel = LogChannel.Debug, params object[] args)
        {
            if (levelList.ContainsKey(level) && !levelList[level]) return;
            if (chanelList.ContainsKey(channel) && !chanelList[channel]) return;

            var msg = FormatMessage(format, level, channel, args);
            switch (level)
            {
                case LogLevel.Error: Debug.LogError(msg); break;
                case LogLevel.Warning: Debug.LogWarning(msg); break;
                case LogLevel.Verbose:
                case LogLevel.Info:
                case LogLevel.Dev:
                default: Debug.Log(msg); break;
            }
        }

        protected static string FormatMessage(string message, LogLevel level, LogChannel channel)
        {
            var color = "white";
            switch (level)
            {
                case LogLevel.Warning: color = "yellow"; break;
                case LogLevel.Error: color = "red"; break;
                case LogLevel.Info: color = "cyan"; break;
                case LogLevel.Verbose: break;
                case LogLevel.Dev: color = "orange";break;
                default: break;
            }
            var ccolor = color;
            switch(channel)
            {
                case LogChannel.Debug:  break;
                case LogChannel.IO:  break;
                case LogChannel.Network: ccolor = "green"; break;
                default: break;
            }
            return string.Format("<color={1}>[{2}]</color><color={0}>{3}</color>", color, ccolor, channel.ToString(), message);
        }
        protected static string FormatMessage(string format, LogLevel level, LogChannel channel, params object[] args)
        {
            var message = string.Format(format, args);
            return FormatMessage(message, level, channel);
        }
    }
}
