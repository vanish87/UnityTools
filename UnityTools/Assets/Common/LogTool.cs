using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityTools.Common;
using UnityTools.Networking;

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
    [Flags]
    public enum LogChannel
    {
        None    = 0,
        Debug   = 1,
        Network = 2,
        IO      = 4,
        Everything = ~None,
    }

    [Flags]
    public enum LogMode
    {
        None    = 0,
        Console = 1,
        File    = 2,
        Network = 4,
        Everything = ~None,
    }
    public class LogToolNetwork
    {
        public class LogToolNetworkSocket : UDPSocket<LogToolNetworkSocket.Data>
        {
            [Serializable]
            public class Data
            {
                internal protected Queue<string> queue = new Queue<string>();
            }
            protected static Data localData = new Data();
            protected static SocketData server = SocketData.Make("127.0.0.1", 13210);
            protected static LogToolNetworkSocket logSocket = new LogToolNetworkSocket();

            protected static Dictionary<string, Data> serverData = new Dictionary<string, Data>();

            internal protected static void Add(string message)
            {
                localData.queue.Enqueue(message);
                if (localData.queue.Count > 10000) localData.queue.Dequeue();
            }
            internal protected static Dictionary<string, List<string>> Get(LogChannel channel = LogChannel.Everything)
            {
                var ret = new Dictionary<string, List<string>>();
                foreach(var client in serverData)
                {
                    var logs = client.Value.queue.ToList();
                    if (channel != LogChannel.Everything)
                    {
                        logs = client.Value.queue.Where(s =>
                        {
                            var start = s.IndexOf('[');
                            var end = s.IndexOf(']');
                            var str = s.Substring(start, end - start);
                            return str.Contains(channel.ToString());
                        }).ToList();
                    }

                    ret.Add(client.Key, logs);
                }

                return ret;
            }

            internal protected static void Send()
            {
                logSocket.Send(server, localData);
            }

            public override void OnMessage(SocketData socket, Data remoteData)
            {
                var remoteIP = socket.endPoint.Address.ToString();
                if (serverData.ContainsKey(remoteIP))
                {
                    serverData[remoteIP].queue.Clear();
                    foreach (var r in remoteData.queue)
                    {
                        serverData[remoteIP].queue.Enqueue(r);
                        if (serverData[remoteIP].queue.Count > 1000) serverData[remoteIP].queue.Dequeue();
                    } 
                }
                else
                {
                    serverData.Add(remoteIP, remoteData);
                }
            }
            public static void SetupNetwork(LogConfigure.LogPC logPC)
            {
                server = SocketData.Make(logPC.ipAddress, logPC.logPort);
                LogTool.Log("Send log to " + server.endPoint.ToString(), LogLevel.Verbose, LogChannel.Network | LogChannel.Debug);
            }

            public override byte[] OnSerialize(Data data)
            {
                var raw = Serilization.ObjectToByteArray(data);
                return CompressTool.Compress(raw); 
            }

            public override Data OnDeserialize(byte[] data, int length)
            {
                var remote = CompressTool.Decompress(data);
                return Serilization.ByteArrayToObject<Data>(remote);
            }
        }


        public static void Log(string message)
        {
            LogToolNetworkSocket.Add(message);
            LogToolNetworkSocket.Send();
        }
        public static void Update()
        {
            LogToolNetworkSocket.Send();
        }
    }

    public class LogTool
    {
        protected static Dictionary<LogLevel, bool> levelList = new Dictionary<LogLevel, bool>();
        protected static LogChannel channels = ~LogChannel.None;
        protected static LogMode modes = LogMode.Console;

        public static void Enable(LogLevel level, bool enabled)
        {
            if (levelList.ContainsKey(level)) levelList[level] = enabled;
            else levelList.Add(level, enabled);
        }
        public static void Enable(LogChannel channel, bool enabled)
        {
            channels = enabled ? (channels | channel) : (~channel & channel);
        }
        public static void Enable(LogChannel channel)
        {
            channels = channel;
        }
        public static void LogAssertIsTrue(bool predict, string message, LogLevel level = LogLevel.Error, LogChannel channel = LogChannel.Debug)
        {
            if (!predict) Log(message, level, channel);
        }
        public static void LogAssertIsFalse(bool predict, string message, LogLevel level = LogLevel.Error, LogChannel channel = LogChannel.Debug)
        {
            if (predict) Log(message, level, channel);
        }
        public static void Log(string message, LogLevel level = LogLevel.Verbose, LogChannel channel = LogChannel.Debug)
        {
            if (levelList.ContainsKey(level) && !levelList[level]) return;
            if ((channels & channel) == LogChannel.None) return;

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
            LogToolNetwork.Log(msg);
        }
        public static void LogFormat(string format, LogLevel level = LogLevel.Verbose, LogChannel channel = LogChannel.Debug, params object[] args)
        {
            if (levelList.ContainsKey(level) && !levelList[level]) return;
            if ((channels & channel) == LogChannel.None ) return;

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

            LogToolNetwork.Log(msg);
        }

        protected static string FormatMessage(string message, LogLevel level, LogChannel channel)
        {
            var color = "white";
            switch (level)
            {
                case LogLevel.Warning:  color = "yellow";   break;
                case LogLevel.Error:    color = "red";      break;
                case LogLevel.Info:     color = "cyan";     break;
                case LogLevel.Verbose: break;
                case LogLevel.Dev:      color = "orange";   break;
                default: break;
            }
            var ccolor = color;

            if((channel & LogChannel.Network) != LogChannel.None) ccolor = "green";

            var time = DateTime.Now.ToString("yy/MM/dd|HH:mm:ss");
            return string.Format("<color={1}>[{4}|{2}]</color><color={0}>{3}</color>", color, ccolor, channel.ToString(), message, time);
        }
        protected static string FormatMessage(string format, LogLevel level, LogChannel channel, params object[] args)
        {
            var message = string.Format(format, args);
            return FormatMessage(message, level, channel);
        }
    }
}
