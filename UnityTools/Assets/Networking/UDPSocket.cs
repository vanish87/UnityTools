using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityTools.Networking
{
    public class SocketData
    {
        public IPEndPoint endPoint;
        public bool reachable = false;
        public Socket socket;

        public static SocketData Make(IPEndPoint end)
        {
            var ret = new SocketData(end);
            ret.reachable = Tool.IsReachable(ret.endPoint);
            return ret;
        }
        public static SocketData Make(int port = 0)
        {
            var ret = new SocketData(port);
            ret.reachable = Tool.IsReachable(ret.endPoint);
            return ret;
        }
        public static SocketData Make(string ip, int port)
        {
            var ret = new SocketData(ip, port);
            ret.reachable = Tool.IsReachable(ret.endPoint);
            return ret;
        }

        protected SocketData(IPEndPoint end)
        {
            this.endPoint = new IPEndPoint(end.Address, end.Port);
        }
        protected SocketData(int port = 0)
        {
            this.endPoint = new IPEndPoint(IPAddress.Any, port);
        }
        protected SocketData(string ip, int port)
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

        ~SocketData()
        {
            this.endPoint = null;
        }       
    }

    public class State
    {
        private const int bufferSize = 64 * 1024; //64K
        public byte[] buffer = new byte[bufferSize];
    }
    public class RecieveState : State
    {
        public SocketData remote = SocketData.Make();
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
            this.socket.Dispose();

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

                        if (DebugLog) Debug.Log("Start Reciever on " + data.endPoint);
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
        
        public virtual T OnDeserialize(byte[] data, int length)
        {
            //Note data may has different size than length
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

            var epTo = SocketData.Make();
            epTo.endPoint.Address = IPAddress.Broadcast;
            epTo.endPoint.Port = port;

            var byteData = this.OnSerialize(data);
            this.SendByte(epTo, byteData);
        }
        public virtual void StartRecieve(int port = 0)
        {
            this.recieveState.remote = SocketData.Make(port);
            this.recieveState.remote.socket = this.socket;

            this.Setup(SocketRole.Reciever, this.recieveState.remote);

            EndPoint epFrom = this.recieveState.remote.endPoint;

            try
            {
                this.socket.BeginReceiveFrom(this.recieveState.buffer, 0, this.recieveState.buffer.Length, SocketFlags.None, ref epFrom, this.RecieveCallback, this.recieveState);
            }
            catch (Exception e)
            {
                //check flag
                Debug.Log(e.ToString());
            }
        }

        private Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        protected void SendByte(SocketData epTo, byte[] byteData)
        {
            epTo.socket = this.socket;

            Assert.IsNotNull(epTo);
            Assert.IsNotNull(byteData);
            Assert.IsTrue(byteData.Length > 0);
            Assert.IsTrue(byteData.Length < 64 * 1024);
            Assert.IsNotNull(epTo.socket);

            if (epTo.reachable)
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
            else
            {
                Debug.LogWarning(epTo.endPoint.ToString() + " is unreachable");
            }
        }
        protected void SendCallback(IAsyncResult ar)
        {
            try
            {
                if (ar.IsCompleted)
                {
                    var socketData = ar.AsyncState as SocketData;

                    int bytes = socketData.socket.EndSendTo(ar);

                    bool found = false;
                    foreach (var c in this.connections[Connection.Outcoming])
                    {
                        if (c.endPoint.Address.Equals(socketData.endPoint.Address))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found == false && socketData.endPoint.Address != IPAddress.Broadcast)
                    {
                        this.connections[Connection.Outcoming].Add(socketData);
                        if (DebugLog) Debug.LogWarningFormat("Add out connection: {0}", socketData.endPoint.ToString());
                    }
                    if (DebugLog) Debug.LogFormat("SEND: {0} bytes To {1}", bytes, socketData.endPoint.ToString());

                    if (DebugLog)
                    {
                        foreach (var c in this.connections)
                        {
                            Debug.Log("connections count " + c.Key.ToString() +" " + c.Value.Count);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                Debug.Log((e as SocketException).ErrorCode);
                Debug.Log((e as SocketException).Message);
            }
        }
        protected void RecieveCallback(IAsyncResult ar)
        {
            try
            {
                var stateFrom = ar.AsyncState as RecieveState;
                Assert.IsTrue(stateFrom == this.recieveState);

                EndPoint epFrom = stateFrom.remote.endPoint;
                int bytesReceived = stateFrom.remote.socket.EndReceiveFrom(ar, ref epFrom);

                var ipFrom = epFrom as IPEndPoint;
                stateFrom.remote.endPoint = ipFrom;

                if (bytesReceived > 0)
                {
                    if (DebugLog) Debug.LogFormat("RECV: {0}: {1}", epFrom.ToString(), bytesReceived);
                    T data = this.OnDeserialize(stateFrom.buffer, bytesReceived);
                    this.OnMessage(stateFrom.remote, data);
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
                    this.connections[Connection.Incoming].Add(SocketData.Make(ipFrom));
                    if (DebugLog) Debug.LogWarningFormat("Add in connection: {0}", ipFrom.ToString());
                }

                epFrom = this.recieveState.remote.endPoint;
                stateFrom.remote.socket.BeginReceiveFrom(this.recieveState.buffer, 0, this.recieveState.buffer.Length, SocketFlags.None, ref epFrom, this.RecieveCallback, this.recieveState);
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                Debug.Log((e as SocketException).ErrorCode);
                Debug.Log((e as SocketException).Message);
            }
        }
    }


}
