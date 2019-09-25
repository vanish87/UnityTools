using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UDPTest : MonoBehaviour
{
    UDPSocketTest server = new UDPSocketTest();
    UDPSocketTest client = new UDPSocketTest();
    private void Start()
    {
        server.Server("127.0.0.1", 12344);
        client.Client("127.0.0.1", 12344);

        client.Send("test123");
    }

    private void Update()
    {
        //client.Send("test123");
    }
    public class UDPSocketTest
    {
        private Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private const int bufSize = 8 * 1024;
        private State state = new State();
        private EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);
        private AsyncCallback recv = null;

        public class State
        {
            public byte[] buffer = new byte[bufSize];
        }

        public void Server(string address, int port)
        {
            _socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            _socket.Bind(new IPEndPoint(IPAddress.Any, port));
            Receive();
        }

        public void Client(string address, int port)
        {
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            //_socket.Connect(IPAddress.Parse(address), port);
            Receive();
        }

        public void Send(string text)
        {
            byte[] data = Encoding.ASCII.GetBytes(text);
            try
            {
                _socket.BeginSendTo(data, 0, data.Length, SocketFlags.None, new IPEndPoint(IPAddress.Broadcast, 12344), (ar) =>
                {
                    State so = (State)ar.AsyncState;
                    int bytes = _socket.EndSendTo(ar);
                    Debug.LogFormat("SEND: {0}, {1}", bytes, text);
                }, state);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        private void Receive()
        {
            _socket.BeginReceiveFrom(state.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv = (ar) =>
            {
                State so = (State)ar.AsyncState;
                int bytes = _socket.EndReceiveFrom(ar, ref epFrom);
                _socket.BeginReceiveFrom(so.buffer, 0, bufSize, SocketFlags.None, ref epFrom, recv, so);
                Debug.LogFormat("RECV: {0}: {1}, {2}", epFrom.ToString(), bytes, Encoding.ASCII.GetString(so.buffer, 0, bytes));
            }, state);
        }
    }
}
