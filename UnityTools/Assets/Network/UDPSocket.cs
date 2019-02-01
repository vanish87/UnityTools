using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace Network
{
    [Serializable]
    public class CustomSocketData
    {
        public string time;
    }

    //code from https://gist.github.com/darkguy2008/413a6fea3a5b4e67e5e0d96f750088a9
    //for testing latency of UDP
    public class UDPSocket<T> where T : CustomSocketData
    {
        public class SocketData
        {
            public IPEndPoint endPoint;
            

            public SocketData(IPEndPoint end)
            {
                this.endPoint = end;
            }
            public SocketData(int port = 0)
            {
                this.endPoint = new IPEndPoint(IPAddress.Any, port);
            }

            public SocketData(string ip, int port)
            {
                IPAddress addr;
                if(ip.ToLower() == "localhost")
                {
                    ip = "127.0.0.1";
                }
                if(IPAddress.TryParse(ip, out addr))
                {
                    this.endPoint = new IPEndPoint(addr, port);
                }
                else
                {
                    this.endPoint = new IPEndPoint(IPAddress.Any, 0);
                }
            }
        }

        public class ConnectionData
        {
            public class State
            {
                private const int bufferSize = 32 * 1024; //32K
                public byte[] buffer = new byte[bufferSize];
            }

            public SocketData socketData = new SocketData();
            public State state = new State();
        }

        public enum Connection
        {
            Incoming,
            Outcoming,
        }

        public enum SocketRole
        {
            Sender,
            Reciever,
            Broadcast,
        }
        protected Dictionary<SocketRole, bool> roleState = new Dictionary<SocketRole, bool>()
        {
            {SocketRole.Sender      , false },
            {SocketRole.Reciever    , false },
            {SocketRole.Broadcast   , false },
        };
        protected Dictionary<Connection, List<ConnectionData>> connections = new Dictionary<Connection, List<ConnectionData>>()
        {
            { Connection.Incoming   , new List<ConnectionData>() },
            { Connection.Outcoming  , new List<ConnectionData>() },
        };

        public void Setup(SocketRole role)
        {
            if (this.roleState[role]) return;

            switch (role)
            {
                case SocketRole.Sender:
                    {
                        //connect is optional
                        //socket.Connect(IPAddress.Parse(address), port);
                    }
                    break;
                case SocketRole.Reciever:
                    {
                        this.socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
                        //this.socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
                        this.socket.Bind(this.socketConfigure.endPoint);
                    }
                    break;
                case SocketRole.Broadcast:
                    {
                        this.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                        //this.socket.Bind(this.socketConfigure.endPoint);
                    }
                    break;
            }
            this.roleState[role] = true;
        }

        public virtual void OnConnect(SocketData socket) { }
        public virtual void OnMessage(SocketData socket, T data) { }
        public virtual void OnDisconnect(SocketData socket) { }
        public virtual void OnError(SocketData socket) { }

        public virtual void Send(SocketData socket, T data)
        {
            ConnectionData connection = new ConnectionData();
            foreach (var c in this.connections[Connection.Outcoming])
            {
                if (c.socketData.endPoint == socket.endPoint)
                {
                    connection = c;
                    break;
                }
            }
            connection.socketData = socket;
            EndPoint epFrom = connection.socketData.endPoint;

            var byteData = Helper.ObjectToByteArray(data);

            try
            {
                this.socket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epFrom, this.SendCallback, connection);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
        public virtual void Broadcast(T data, int port)
        {
            this.Setup(SocketRole.Broadcast);
            ConnectionData connection = new ConnectionData();
            connection.socketData.endPoint.Address = IPAddress.Broadcast;
            connection.socketData.endPoint.Port = port;
            EndPoint epFrom = connection.socketData.endPoint;

            var byteData = Helper.ObjectToByteArray(data);
            try
            {
                this.socket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epFrom, this.SendCallback, connection);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
        public virtual void StartRecieve(int port = 0)
        {
            this.socketConfigure.endPoint.Port = port;

            this.Setup(SocketRole.Reciever);
            var connection = new ConnectionData();
            EndPoint epFrom = connection.socketData.endPoint;

            try
            {
                this.socket.BeginReceiveFrom(connection.state.buffer, 0, connection.state.buffer.Length, SocketFlags.None, ref epFrom, this.RecieveCallback, connection);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        protected virtual void OnConfigure(ref SocketData configureData) { }

        private Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        private SocketData socketConfigure = new SocketData("localhost", 0);

        private void Setup()
        {
            this.OnConfigure(ref this.socketConfigure);
        }

        protected void SendCallback(IAsyncResult ar)
        {
            try
            {
                var connection = ar.AsyncState as ConnectionData;
                int bytes = socket.EndSendTo(ar);

                if (this.connections[Connection.Outcoming].Contains(connection) == false && connection.socketData.endPoint.Address != IPAddress.Broadcast)
                {
                    this.connections[Connection.Outcoming].Add(connection);
                    Debug.LogFormat("Add out connection: {0}", connection.socketData.endPoint.ToString());
                }
                Debug.LogFormat("SEND: {0} To {1}", bytes, connection.socketData.endPoint.ToString());
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
        protected void RecieveCallback(IAsyncResult ar)
        {
            try
            {
                var connection = ar.AsyncState as ConnectionData;
                EndPoint epFrom = connection.socketData.endPoint;
                int bytes = socket.EndReceiveFrom(ar, ref epFrom);

                if (bytes > 0)
                {
                    Debug.LogFormat("RECV: {0}: {1}, {2}", epFrom.ToString(), bytes, Helper.ByteArrayToObject<CustomSocketData>(connection.state.buffer).time);
                }

                connection.socketData.endPoint = epFrom as IPEndPoint;

                if (this.connections[Connection.Incoming].Contains(connection) == false)
                {
                    this.connections[Connection.Incoming].Add(connection);
                    Debug.LogFormat("Add in connection: {0}", connection.socketData.endPoint.ToString());
                }
                this.socket.BeginReceiveFrom(connection.state.buffer, 0, connection.state.buffer.Length, SocketFlags.None, ref epFrom, this.RecieveCallback, connection);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }

   
}