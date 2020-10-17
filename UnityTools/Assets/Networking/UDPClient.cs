using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Common;

namespace UnityTools.Networking
{
    public class UDPClient : MonoBehaviour
    {
        public ExampleSocket socket = new ExampleSocket();
        protected void Start()
        {
            this.socket.StartReceive(12347);
        }
        protected void Update()
        {
            UDPServer.Data data;
            if (this.socket.deltaQueue.TryDequeue(out data))
            {
                Debug.Log(data.deltaTime);
            }
        }
        protected void OnDestroy()
        {
            this.socket.Dispose();
        }
    }
}