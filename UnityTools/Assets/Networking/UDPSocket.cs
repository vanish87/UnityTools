using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace Networking
{
    public class SocketData
    {
        public IPEndPoint endPoint;

        public SocketData(IPEndPoint end)
        {
            this.endPoint = new IPEndPoint(end.Address, end.Port);
        }
        public SocketData(int port = 0)
        {
            this.endPoint = new IPEndPoint(IPAddress.Any, port);
        }

        public SocketData(string ip, int port)
        {
            IPAddress local = IPAddress.Any;
            if (ip.ToLower() == "localhost" || ip == "127.0.0.1")
            {
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
                foreach (var addr in localIPs)
                {
                    if (addr.AddressFamily == AddressFamily.InterNetwork)
                    {
                        local = addr;
                        break;
                    }
                }
            }
            else
            {
                if (IPAddress.TryParse(ip, out local))
                {
                    //nothing to do
                }
                else
                {
                    local = IPAddress.Any;
                    port = 0;
                }
            }

            Assert.IsTrue(IPEndPoint.MinPort <= port && port <= IPEndPoint.MaxPort);
            this.endPoint = new IPEndPoint(local, port);
        }
    }

    public class State
    {
        private const int bufferSize = 64 * 1024; //64K
        public byte[] buffer = new byte[bufferSize];
    }
    public class RecieveState : State
    {
        public SocketData remote = new SocketData();
    }

    //code from https://gist.github.com/darkguy2008/413a6fea3a5b4e67e5e0d96f750088a9
    //for testing latency of UDP
    public class UDPSocket<T>
    {

        public RecieveState recieveState = new RecieveState();
        public bool DebugLog = false;

        ~UDPSocket()
        {
            this.Disconnect();
        }

        public void Disconnect()
        {
            this.socket.Close();

            foreach (var r in this.roleState.Keys)
            {
                this.roleState[r] = false;
            }
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
        protected Dictionary<Connection, List<SocketData>> connections = new Dictionary<Connection, List<SocketData>>()
        {
            { Connection.Incoming   , new List<SocketData>() },
            { Connection.Outcoming  , new List<SocketData>() },
        };

        public void Setup(SocketRole role, SocketData data = null)
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
                        Assert.IsNotNull(data);
                        this.socket.Bind(data.endPoint);
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

        public virtual byte[] OnSerialize(T data)
        {
            return Helper.ObjectToByteArray(data);
        }
        public virtual T OnDeserialize(byte[] data)
        {
            return Helper.ByteArrayToObject<T>(data);
        }

        public virtual void Send(SocketData socket, T data)
        {
            this.Setup(SocketRole.Sender);

            var byteData = this.OnSerialize(data);
            this.SendByte(socket, byteData);
        }
        public virtual void Broadcast(T data, int port)
        {
            this.Setup(SocketRole.Broadcast);

            var epTo = new SocketData();
            epTo.endPoint.Address = IPAddress.Broadcast;
            epTo.endPoint.Port = port;

            var byteData = this.OnSerialize(data);
            this.SendByte(epTo, byteData);
        }
        public virtual void StartRecieve(int port = 0)
        {
            this.recieveState.remote = new SocketData(port);

            this.Setup(SocketRole.Reciever, this.recieveState.remote);

            EndPoint epFrom = this.recieveState.remote.endPoint;

            try
            {
                this.socket.BeginReceiveFrom(this.recieveState.buffer, 0, this.recieveState.buffer.Length, SocketFlags.None, ref epFrom, this.RecieveCallback, this.recieveState);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        private Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        protected void SendByte(SocketData epTo, byte[] byteData)
        {
            try
            {
                this.socket.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None, epTo.endPoint, this.SendCallback, epTo);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
        protected void SendCallback(IAsyncResult ar)
        {
            try
            {
                var socketData = ar.AsyncState as SocketData;
                int bytes = socket.EndSendTo(ar);

                if (this.connections[Connection.Outcoming].Contains(socketData) == false && socketData.endPoint.Address != IPAddress.Broadcast)
                {
                    this.connections[Connection.Outcoming].Add(socketData);
                    if (DebugLog) Debug.LogWarningFormat("Add out connection: {0}", socketData.endPoint.ToString());
                }
                if (DebugLog) Debug.LogFormat("SEND: {0} bytes To {1}", bytes, socketData.endPoint.ToString());
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
                var stateFrom = ar.AsyncState as RecieveState;
                Assert.IsTrue(stateFrom == this.recieveState);

                EndPoint epFrom = stateFrom.remote.endPoint;
                int bytes = socket.EndReceiveFrom(ar, ref epFrom);

                var ipFrom = epFrom as IPEndPoint;
                stateFrom.remote.endPoint = ipFrom;

                if (bytes > 0)
                {
                    //if (DebugLog) Debug.LogFormat("RECV: {0}: {1}, {2}", epFrom.ToString(), bytes, Helper.ByteArrayToObject<CustomSocketData>(stateFrom.buffer).time);
                    this.OnMessage(stateFrom.remote, this.OnDeserialize(stateFrom.buffer));
                }

                bool found = false;
                foreach (var c in this.connections[Connection.Incoming])
                {
                    if (c.endPoint.Address.Equals(ipFrom.Address))
                    {
                        found = true;
                        break;
                    }
                }
                if (found == false)
                {
                    this.connections[Connection.Incoming].Add(new SocketData(ipFrom));
                    if (DebugLog) Debug.LogWarningFormat("Add in connection: {0}", ipFrom.ToString());
                }

                epFrom = this.recieveState.remote.endPoint;
                this.socket.BeginReceiveFrom(this.recieveState.buffer, 0, this.recieveState.buffer.Length, SocketFlags.None, ref epFrom, this.RecieveCallback, this.recieveState);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }


}
