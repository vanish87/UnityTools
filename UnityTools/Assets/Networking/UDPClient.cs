using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTools.Common;

namespace UnityTools.Networking
{
    public class UDPClient : MonoBehaviour
    {
        public GPUSocket socket = new GPUSocket();
        
        double currentDalta = 0;
        float sin = 0;

        // Update is called once per frame
        void Update()
        {
            //this.socket.Send(Time.deltaTime.ToString());

            UDPServer.GPUData data;
            var now = Serilization.ConvertFrom2019();
            //Debug.Log("Frame Start");

            var deltaThisFrame = 0.0d;
            while (this.socket.deltaQueue.TryPeek(out data))
            {
                var server = data.serverTime;
                var clientTarget = server + 5000;

                deltaThisFrame = clientTarget - now;
                //Debug.Log("deltaThisFrame" + deltaThisFrame);
                if (deltaThisFrame < 0)
                {
                    if (this.socket.deltaQueue.TryDequeue(out data))
                    {
                        //this.gameObject.transform.Rotate(Vector3.right * (data.deltaTime) * 20);
                        var timeInSecond = (float)deltaThisFrame * 0.001f;
                        var timeTick = data.deltaTime;
                        this.gameObject.transform.localPosition = new Vector3(
                            transform.localPosition.x, 
                            Mathf.Sin(sin += timeTick) * 5, 
                            transform.localPosition.z);

                        currentDalta += timeInSecond + data.deltaTime;
                        //Debug.Log("Tick" + deltaThisFrame);
                        //Debug.Log("currentDalta" + currentDalta);
                    }
                }
                else
                {
                    break;
                }
            }

            if (currentDalta > 0 && false)
            {
                var tickTime = Mathf.Min(Time.deltaTime, (float)currentDalta);
                this.gameObject.transform.localPosition = new Vector3(
                      transform.localPosition.x,
                      Mathf.Sin(sin += tickTime) * 5,
                      transform.localPosition.z);
                currentDalta -= tickTime;
                //Debug.Log("currentDalta Tick" + currentDalta);
            }
        }

        void Start()
        {
            //this.socket.Server(12345);
            //this.StartCoroutine(this.Broadcast());
            this.socket.StartRecieve(12347);
        }
        private void OnDestroy()
        {
            this.socket.Dispose();
        }

        /*IEnumerator Broadcast()
        {
            var socket = new UDPSocket<CustomSocketData>.SocketData("localhost", 12345);
            var data = new CustomSocketData();
            while (true)
            {
                yield return new WaitForSeconds(1);
                data.time = Time.realtimeSinceStartup.ToString();
                this.socket.Send(socket, data);
            }
        }*/
    }
}