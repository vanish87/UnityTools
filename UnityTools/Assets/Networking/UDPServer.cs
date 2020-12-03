using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityTools.Common;

namespace UnityTools.Networking
{

    public class ExampleSocket : UDPSocket<UDPServer.Data>
    {
        public ConcurrentQueue<UDPServer.Data> deltaQueue = new ConcurrentQueue<UDPServer.Data>();
        public override void OnMessage(SocketData socket, UDPServer.Data data)
        {
            deltaQueue.Enqueue(data);
        }
    }
    public class UDPServer : MonoBehaviour
    {
        [System.Serializable]
        public class Data
        {
            public double serverTime;
            public float deltaTime;
        }
        protected ExampleSocket socket = new ExampleSocket();
        protected void Start()
        {
            //this.socket.Client("127.0.0.1", 12345);
            this.StartCoroutine(this.Broadcast());
        }

        // Update is called once per frame
        void Update()
        {
        }

        IEnumerator Broadcast()
        {
            var data = new Data();
            while (true)
            {
                data.deltaTime = Time.deltaTime;
                data.serverTime = Serialization.ConvertFrom2019();
                //this.socket.Send(socket, data);
                this.socket.Broadcast(data, 12347);
                yield return new WaitForSeconds(1);
            }
        }
    }
}