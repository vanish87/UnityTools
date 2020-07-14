using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityTools.Common;

namespace UnityTools.Networking
{

    public class GPUSocket : UDPSocket<UDPServer.GPUData>
    {
        public ConcurrentQueue<UDPServer.GPUData> deltaQueue = new ConcurrentQueue<UDPServer.GPUData>();
        public override void OnMessage(SocketData socket, UDPServer.GPUData data)
        {
            //Task.Run(() => UDPServer.DebugReport(data));

            deltaQueue.Enqueue(data);
        }
    }
    public class UDPServer : MonoBehaviour
    {
        public static void DebugReport(GPUData data)
        {
            double now = Serilization.ConvertFrom2019();
            double diff = now - data.serverTime;
            Debug.LogFormat("Server time is {0:0.000000}, with delta {1:0.000000}", data.serverTime, data.deltaTime);
            Debug.LogFormat("Client time is {0:0.000000}", now);
            Debug.LogFormat("Diff is {0:0.000000}", diff);
        }
        [System.Serializable]
        public class GPUData
        {
            public double serverTime;
            public float deltaTime;
        }
        public UDPSocket<GPUData> socket = new UDPSocket<GPUData>();
        // Start is called before the first frame update
        void Start()
        {
            //this.socket.Client("127.0.0.1", 12345);
            //this.socket.StartRecieve(12345);
            //this.socket.Broadcast(new GPUData() { serverTime = Time.realtimeSinceStartup, deltaTime = Time.deltaTime }, 12345);
            this.StartCoroutine(this.Broadcast());
        }

        // Update is called once per frame
        void Update()
        {
        }

        IEnumerator Broadcast()
        {
            var socket = SocketData.Make("localhost", 12345);
            var socket1 = SocketData.Make("localhost", 12346);
            //socket.endPoint.Address = IPAddress.Broadcast;
            //this.socket.Setup(UDPSocket<GPUData>.SocketRole.Broadcast);
            var data = new GPUData();
            while (true)
            {
                data.deltaTime = Time.deltaTime;
                data.serverTime = Serilization.ConvertFrom2019();
                //this.socket.Send(socket, data);
                //this.socket.Send(socket1, data);
                this.socket.Broadcast(data, 12347);
                this.socket.Broadcast(data, 12348);
                yield return new WaitForEndOfFrame();
            }
        }
    }
}