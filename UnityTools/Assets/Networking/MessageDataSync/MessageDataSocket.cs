using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        [SerializeField]int test;

        [SerializeField]string testString = "tset PC";

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
            public string hash;
            public int messageID;
            public short replyPort;
            public byte[] data;
        }

        public interface IMessage
        {
            string hash { get; }
            byte[] OnSerialize();
            void OnDeserialize(byte[] data);

            void OnSuccessReceived();
        }

        protected ConcurrentDictionary<string, Data> currentSendingData = new ConcurrentDictionary<string, Data>();
        protected ConcurrentDictionary<int, Data> currentReceivedData = new ConcurrentDictionary<int, Data>();
        protected ConcurrentDictionary<string, IMessage> messageMap = new ConcurrentDictionary<string, IMessage>();
        protected List<SocketData> remote = new List<SocketData>();
        protected short replayPort = -1;

        protected int messageCounter = 0;

        protected short GetNextAvailablePortFrom(short port = 10000)
        {
            var currentPorts = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners();
            while(port < short.MaxValue)
            {
                var np = Array.Find(currentPorts,udp=>udp.Port == port);
                if(np == null) return port;

                port++;
            }
            return -1;
        }

        public void SetupReply()
        {
            if(this.replayPort > 0) return;
            this.replayPort = this.GetNextAvailablePortFrom();
            this.StartReceive(this.replayPort);
        }

        public override void OnMessage(SocketData socket, Data data)
        {
            lock(obj)
            {
            if(this.replayPort > 0 && this.currentSendingData.ContainsKey(data.hash))
            {
                var d = default(Data);
                this.currentSendingData.TryRemove(data.hash, out d);
                if (this.messageMap.ContainsKey(data.hash)) this.messageMap[data.hash].OnSuccessReceived();
            }
            else
            {
                if(!this.currentReceivedData.ContainsKey(data.messageID))
                {
                    this.messageMap[data.hash].OnDeserialize(data.data);
                }

                this.currentReceivedData.AddOrUpdate(data.messageID, data, (mid, old) => data);
                var server = SocketData.Make(socket.endPoint.Address, data.replyPort);
                this.Send(server, data);
            }
            }
        }

        public void BindRemote(List<SocketData> sockets)
        {
            this.remote = sockets;
        }
        public void BindData(IMessage message)
        {
            if(this.messageMap.ContainsKey(message.hash))
            {
                LogTool.Log("Same hash, use different hash", LogLevel.Warning);
                return;
            }
            this.messageMap.AddOrUpdate(message.hash, message, (h, old) => message);
        }
        public void Send(IMessage message)
        {
            // this.SetupReply();

            var hash = message.hash;
            var mid = this.messageCounter++%int.MaxValue;
            var data = new Data() { hash = message.hash, messageID = mid, replyPort = this.replayPort, data = message.OnSerialize() };
            this.currentSendingData.AddOrUpdate(hash, data, (h, old)=>data);
        }

        object obj = new object();

        public void Update()
        {
            lock(obj)
            {
            foreach(var r in this.remote)
            {
                foreach (var d in this.currentSendingData)
                {
                    this.Send(r, d.Value);
                }
            }
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