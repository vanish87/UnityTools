using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityTools.Common;

namespace UnityTools.Networking
{
    [Serializable]
    public class Datatex : MessageDataSocket.MessageData
    {
        [SerializeField]int test;

        [SerializeField]string testString = "tset PC";

        public string HashString => typeof(Datatex).ToString();

        public void OnDeseialize(byte[] data)
        {
            var d = Serilization.ByteArrayToObject<Datatex>(data);
            this.test = d.test;
            this.testString = d.testString;
        }

        public byte[] OnSerialize()
        {
            return Serilization.ObjectToByteArray(this);
        }
    }
    public class MessageDataSocket : UDPSocket<List<MessageDataSocket.InternalData>>
    {
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
        
        protected Dictionary<string, MessageData> data = new Dictionary<string, MessageData>();
        protected List<InternalData> sendData = new List<InternalData>();
        [Serializable]
        public class InternalData
        {
            public string hash;
            public byte[] data;
        }
        public interface MessageData
        {
            string HashString { get; }
            byte[] OnSerialize();
            void OnDeseialize(byte[] data);
        }
        public void CleanUp()
        {
            this.data.Clear();
        }
        public void Bind(MessageData message, string hashID = null, bool overwrite = false)
        {
            var hash = GetHash(hashID != null ? hashID : message.HashString);

            if(!overwrite && this.data.ContainsKey(hash))
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
                    this.data[d.hash].OnDeseialize(d.data);
                }
            }
        }
        
    }
}