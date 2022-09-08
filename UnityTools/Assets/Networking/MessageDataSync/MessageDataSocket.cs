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
            void OnDeserialize(byte[] data);
        }

        protected readonly float timeout = 10;

        protected ConcurrentDictionary<int, (IMessage, Data, DateTime)> currentSendData = new ConcurrentDictionary<int, (IMessage, Data, DateTime)>();
        protected ConcurrentDictionary<int, DateTime> currentReceivedData = new ConcurrentDictionary<int, DateTime>();
        protected List<SocketData> remote = new List<SocketData>();
        protected short replayPort = -1;
        protected int messageCounter = 0;
        protected Action<IMessage> newMessageActions;
        protected Action<IMessage> messageReceivedActions;
        public void NewMessage(Action<IMessage> action)
        {
            this.newMessageActions -= action;
            this.newMessageActions += action;
        }
        public void MessageReceived(Action<IMessage> action)
        {
            this.messageReceivedActions -= action;
            this.messageReceivedActions += action;
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
			Debug.Log("Reply port is " + this.replayPort);
            this.StartReceive(this.replayPort);
        }

        public override void OnMessage(SocketData socket, Data data)
        {
			Debug.Log("Get Message from server");
            //if it is sending data and is waiting for reply
            if (this.replayPort > 0)
            {
                var d = default((IMessage, Data, DateTime));
                if (this.currentSendData.TryRemove(data.messageID, out d))
                {
					//get a reply from remote
                    //sending succeed
					this.messageReceivedActions?.Invoke(d.Item1);

                    // Debug.Log("Send on " + d.Item3.ToString());
                    Debug.Log("Latency " + (DateTime.Now - d.Item3).TotalMilliseconds);
                }
                else
                {
                    Debug.Log("Get Reply but no message is waiting discard it");
                }
            }
            //if it is receiving data
            else
            {
                // Debug.Log("Get Message from server");
                //if received this message at first time
                //call new message function
                if (!this.currentReceivedData.ContainsKey(data.messageID))
                {
                    // var method = typeof(Serialization).GetMethod("ByteArrayToObject", BindingFlags.Public | BindingFlags.Static);
                    // var typeFunc = method.MakeGenericMethod(data.type);
                    // var ret = (IMessage)typeFunc?.Invoke(null, new object[1] { data.data });

                    var obj = (IMessage)Activator.CreateInstance(data.type);
                    obj.OnDeserialize(data.data);
                    this.newMessageActions?.Invoke(obj);
                }
                else
                {
                    Debug.Log("Client get a duplicated massage");
                }

                this.currentReceivedData.AddOrUpdate(data.messageID, DateTime.Now, (mid, old) => DateTime.Now);

                //try to reply to server with an empty message
                if(data.replyPort > 0)
                {
                    var server = SocketData.Make(socket.endPoint.Address, data.replyPort);
                    data.data = null;
                    if(!Tool.IsReachable(server.endPoint))
                    {
                        Debug.Log("Cannot reach " + server.endPoint);

                    }
                    else
                    {
                        this.Send(server, data);
                        Debug.Log("Send reply");
                    }
                    
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
            this.currentSendData.AddOrUpdate(mid, (message, data, DateTime.Now), (h, old) => (message, data, DateTime.Now));

            //send them immediately to reduce latency
            this.SendQueue();
        }
        public void SendNoneReply(IMessage message)
        {
            var mid = this.messageCounter++ % int.MaxValue;
            var data = new Data() { type = message.GetType(), messageID = mid, replyPort = -1, data = message.OnSerialize() };
            foreach (var r in this.remote)
            {
				this.Send(r, data);
            }
        }

        protected void SendQueue()
        {
            foreach (var r in this.remote)
            {
                foreach (var d in this.currentSendData)
                {
                    // Debug.Log("From send to socket send " + (DateTime.Now - d.Value.Item3).TotalMilliseconds);
                    this.Send(r, d.Value.Item2);
                    // Debug.Log("Send to" + r.endPoint.ToString() + d.Value.Item2);
                }
            }
        }


        public void Update()
        {
            this.SendQueue();

            this.UpdateSendingTimeout();
            this.UpdateReceiveTimeout();

        }

        protected void UpdateSendingTimeout()
        {
            var timeout = new List<int>();
            foreach(var d in this.currentSendData)
            {
                if(DateTime.Now.Subtract(d.Value.Item3).Seconds > this.timeout)
                {
                    timeout.Add(d.Key);
                }
            }
            foreach(var mid in timeout) this.currentSendData.TryRemove(mid, out _);

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
            foreach (var d in this.currentSendData)
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