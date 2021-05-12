using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityTools.Common;
using UnityTools.Debuging;

namespace UnityTools.Networking
{
    [Serializable]
    public class Datatex : MessageDataSocket.IMessageData
    {
        [SerializeField] int test;

        [SerializeField] string testString = "tset PC";

        public string HashString => typeof(Datatex).ToString();

        public void OnDeserialize(byte[] data)
        {
            var d = Serialization.ByteArrayToObject<Datatex>(data);
            this.test = d.test;
            this.testString = d.testString;
        }

        public byte[] OnSerialize()
        {
            return Serialization.ObjectToByteArray(this);
        }
    }
    public class MessageSocket : UDPSocket<MessageSocket.Data>
    {
        [Serializable]
        public class Data
        {
            public int messageID;
            public Type type;
            public short replyPort;
            public byte[] data;
        }

        public interface IMessage
        {
            byte[] OnSerialize();

            void OnSuccessReceived();
        }

        protected readonly float timeout = 10;

        protected ConcurrentDictionary<int, (IMessage, Data, DateTime)> currentSendingData = new ConcurrentDictionary<int, (IMessage, Data, DateTime)>();
        protected ConcurrentQueue<(IMessage, Data)> currentSendingQueue = new ConcurrentQueue<(IMessage, Data)>();
        protected ConcurrentDictionary<int, DateTime> currentReceivedData = new ConcurrentDictionary<int, DateTime>();
        protected List<SocketData> remote = new List<SocketData>();
        protected short replayPort = -1;

        protected int messageCounter = 0;
        protected Action<IMessage> newMessageActions;

        public void NewMessage(Action<IMessage> action)
        {
            this.newMessageActions -= action;
            this.newMessageActions += action;
        }

        protected short GetNextAvailablePortFrom(short port = 10000)
        {
            var currentPorts = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners();
            while (port < short.MaxValue)
            {
                var np = Array.Find(currentPorts, udp => udp.Port == port);
                if (np == null) return port;

                port++;
            }
            return -1;
        }

        protected void SetupReply()
        {
            if (this.replayPort > 0) return;
            this.replayPort = this.GetNextAvailablePortFrom();
            this.StartReceive(this.replayPort);
        }

        public override void OnMessage(SocketData socket, Data data)
        {
            if (this.replayPort > 0)
            {
                var d = default((IMessage, Data, DateTime));
                if (this.currentSendingData.TryRemove(data.messageID, out d))
                {
                    d.Item1.OnSuccessReceived();
                }
            }
            else
            {
                if (!this.currentReceivedData.ContainsKey(data.messageID))
                {
                    var method = typeof(Serialization).GetMethod("ByteArrayToObject", BindingFlags.Public | BindingFlags.Static);
                    var typeFunc = method.MakeGenericMethod(data.type);
                    var ret = (IMessage)typeFunc?.Invoke(null, new object[1] { data.data });
                    this.newMessageActions?.Invoke(ret);
                }

                this.currentReceivedData.AddOrUpdate(data.messageID, DateTime.Now, (mid, old) => DateTime.Now);

                if(data.replyPort > 0)
                {
                    var server = SocketData.Make(socket.endPoint.Address, data.replyPort);
                    data.data = null;
                    this.Send(server, data);
                }
            }
        }

        public void BindRemote(List<SocketData> sockets)
        {
            this.remote = sockets;
        }
        public void Send(IMessage message)
        {
            this.SetupReply();

            var mid = this.messageCounter++ % int.MaxValue;
            var data = new Data() { type = message.GetType(), messageID = mid, replyPort = this.replayPort, data = message.OnSerialize() };
            this.currentSendingData.AddOrUpdate(mid, (message, data, DateTime.Now), (h, old) => (message, data, DateTime.Now));
        }
        public void SendNoneReply(IMessage message)
        {
            var mid = this.messageCounter++ % int.MaxValue;
            var data = new Data() { type = message.GetType(), messageID = mid, replyPort = -1, data = message.OnSerialize() };
            this.currentSendingQueue.Enqueue((message, data));
        }


        public void Update()
        {
            foreach (var r in this.remote)
            {
                foreach (var d in this.currentSendingData)
                {
                    this.Send(r, d.Value.Item2);
                }
                while(this.currentSendingQueue.Count > 0)
                {
                    var d = default((IMessage, Data));
                    if(this.currentSendingQueue.TryDequeue(out d))
                    {
                        this.Send(r, d.Item2);
                        d.Item1.OnSuccessReceived();
                    }
                }
            }

            this.UpdateSendingTimeout();
            this.UpdateReceiveTimeout();

        }

        protected void UpdateSendingTimeout()
        {
            var timeout = new List<int>();
            foreach(var d in this.currentSendingData)
            {
                if(DateTime.Now.Subtract(d.Value.Item3).Seconds > this.timeout)
                {
                    timeout.Add(d.Key);
                }
            }
            foreach(var mid in timeout) this.currentSendingData.TryRemove(mid, out _);

        }

        protected void UpdateReceiveTimeout()
        {

            var timeout = new List<int>();
            foreach(var d in this.currentReceivedData)
            {
                if(DateTime.Now.Subtract(d.Value).Seconds > this.timeout)
                {
                    timeout.Add(d.Key);
                }
            }
            foreach(var mid in timeout) this.currentReceivedData.TryRemove(mid, out _);
        }


        public void OnGUI()
        {
            foreach (var d in this.currentSendingData)
            {
                GUILayout.Label(d.Key.ToString());
            }
        }
    }
    public class MessageDataSocket : UDPSocket<List<MessageDataSocket.InternalData>>
    {
        [System.Runtime.InteropServices.DllImport("msvcrt.dll", CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)]
        static extern int memcmp(byte[] b1, byte[] b2, long count);

        static bool ByteArrayCompare(byte[] b1, byte[] b2)
        {
            // Validate buffers are the same length.
            // This also ensures that the count does not exceed the length of either buffer.  
            return b1.Length == b2.Length && memcmp(b1, b2, b1.Length) == 0;
        }
        public static string GetHash(string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();
            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            var data = algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        protected Dictionary<string, IMessageData> data = new Dictionary<string, IMessageData>();
        protected List<InternalData> sendData = new List<InternalData>();
        [Serializable]
        public class InternalData
        {
            public string hash;
            public byte[] data;
        }
        public interface IMessageData
        {
            string HashString { get; }
            byte[] OnSerialize();
            void OnDeserialize(byte[] data);
        }
        public void CleanUp()
        {
            this.data.Clear();
        }
        public void Bind(IMessageData message, string hashID = null, bool overwrite = false)
        {
            var hash = GetHash(hashID != null ? hashID : message.HashString);

            if (!overwrite && this.data.ContainsKey(hash))
            {
                Debug.LogError("Same hash in the data set, set a unique name in hashID");
                return;
            }

            this.data[hash] = message;
        }

        public void SendData(SocketData server)
        {
            this.sendData.Clear();
            foreach (var m in this.data)
            {
                this.sendData.Add(new InternalData() { hash = m.Key, data = m.Value.OnSerialize() });
            }

            this.Send(server, this.sendData);
        }

        public override void OnMessage(SocketData socket, List<InternalData> data)
        {
            foreach (var d in data)
            {
                if (this.data.ContainsKey(d.hash))
                {
                    this.data[d.hash].OnDeserialize(d.data);
                }
            }
        }

    }
}